using System.Text;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Drives every branch of <see cref="TrueTypeCmap"/> against hand-built subtables with known
///		expected answers.
/// </summary>
/// <remarks>
///		<para>
///		MASTER-101 / TEST-015. The parser used to be a private class inside a fact about icon
///		glyphs, and the only font it ever read declares format 4 three times over — so the
///		format 0, format 6 and format 12 branches had never executed. They execute here.
///		</para>
///		<para>
///		Every fixture is BYTES BUILT IN SOURCE rather than a binary file added to the repository,
///		so a reviewer can read what each test feeds the parser out of the diff. The builders below
///		lay out an sfnt table directory and a <c>cmap</c> exactly as the OpenType specification
///		does; a builder that quietly disagreed with the specification would make every expectation
///		here self-fulfilling, which is why the format 4 test also pins the arithmetic against a
///		deliberate two-byte shift.
///		</para>
/// </remarks>
public class TrueTypeCmapTests
{
	/// <summary>
	///		The sfnt header plus one 16-byte table record — the offset every synthetic font's
	///		single table starts at.
	/// </summary>
	private const int TableDirectoryLength = 28;

	/// <summary>
	///		The codepoint set each synthetic subtable is built to cover. Hoisted to fields because
	///		CA1861 rejects a constant array argument, and named so a failure reads as a disagreement
	///		about a format rather than about a literal.
	/// </summary>
	private static readonly int[] Format0Expected = [0x20, 0x41, 0xFF];

	private static readonly int[] Format4Expected = [0x41, 0x42, 0x43, 0xE000, 0xE002];

	private static readonly int[] Format6Expected = [0x0100, 0x0102];

	private static readonly int[] Format12Expected = [0x1F600, 0x1F601, 0x1F602];

	private static readonly int[] AllFormatsExpected =
		[0x41, 0x42, 0x43, 0x0100, 0xE000, 0xE002, 0x1F600];

	/// <summary>
	///		One <c>cmap</c> format 4 segment, as the builder takes it.
	/// </summary>
	/// <param name="Start">
	///		The first codepoint of the segment.
	/// </param>
	/// <param name="End">
	///		The last codepoint of the segment.
	/// </param>
	/// <param name="Delta">
	///		The <c>idDelta</c> added to a code to get its glyph id.
	/// </param>
	/// <param name="Glyphs">
	///		The glyph ids reached through <c>idRangeOffset</c>, one per code in the segment. EMPTY
	///		selects the <c>idDelta</c> arm instead, which is the other half of format 4 and the
	///		one the real font uses.
	/// </param>
	private sealed record Segment(
		int Start,
		int End,
		int Delta,
		int[] Glyphs);

	[Fact]
	public void AFormat0SubtableCoversExactlyTheCodesWithANonZeroGlyph()
	{
		var font = Font(Format0(
			0x20,
			0x41,
			0xFF));

		TrueTypeCmap
			.Codepoints(font)
			.Should()
			.BeEquivalentTo(
				Format0Expected,
				"format 0 is a flat byte per code from U+0000 to U+00FF and a zero byte means "
				+ "the font has no glyph for that code");
	}

	[Fact]
	public void AFormat4SubtableCoversExactlyTheCodepointsItsSegmentsName()
	{
		TrueTypeCmap
			.Codepoints(Font(Format4(Segments())))
			.Should()
			.BeEquivalentTo(
				Format4Expected,
				"the four parallel arrays are read at endCode, endCode + segCountX2 + 2, and two "
				+ "further segCountX2 strides; an off-by-one in any of them returns a plausible "
				+ "but wrong set that the coverage fact would then trust as ground truth");
	}

	[Fact]
	public void AFormat4SubtableReadsGlyphIdsThroughARangeOffsetAsWellAsThroughADelta()
	{
		var covered = TrueTypeCmap.Codepoints(Font(Format4(Segments())));

		// The U+E000 segment carries an idRangeOffset and a glyph id array of 5, 0, 7. The zero
		// is the discriminating entry: a parser that treated a range-offset segment as covering
		// its whole span, rather than reading each glyph id, would report U+E001 as drawable.
		covered
			.Should()
			.Contain(0xE000);

		covered
			.Should()
			.NotContain(
				0xE001,
				"its glyph id array entry is 0, which means the font has no glyph for it");

		covered
			.Should()
			.Contain(0xE002);
	}

	[Fact]
	public void AFormat4SegmentWhoseDeltaWrapsItsGlyphIdToZeroIsNotCovered()
	{
		// U+0050 with an idDelta of 0xFFB0 gives (0x0050 + 0xFFB0) & 0xFFFF == 0, which is .notdef
		// and means the font cannot draw it. This is the segment that makes a two-byte shift of the
		// idDelta array detectable at all: coverage otherwise depends only on the glyph id being
		// non-zero, and a wrong delta is still non-zero.
		TrueTypeCmap
			.Codepoints(Font(Format4(Segments())))
			.Should()
			.NotContain(
				0x0050,
				"its idDelta maps it to glyph 0, which is the absence of a glyph rather than one");
	}

	[Fact]
	public void AFormat6SubtableCoversTheDenseRunAndSkipsItsZeroGlyphs()
	{
		var font = Font(Format6(
			0x0100,
			3,
			0,
			9));

		TrueTypeCmap
			.Codepoints(font)
			.Should()
			.BeEquivalentTo(
				Format6Expected,
				"format 6 is a dense run of glyph ids from firstCode, and a zero entry is a hole "
				+ "in the run rather than the end of it");
	}

	[Fact]
	public void AFormat12SubtableCoversASupplementaryPlaneGroup()
	{
		var font = Font(Format12((0x1F600, 0x1F602)));

		TrueTypeCmap
			.Codepoints(font)
			.Should()
			.BeEquivalentTo(
				Format12Expected,
				"format 12 is the only subtable format that can name a codepoint above U+FFFF");
	}

	[Fact]
	public void EverySubtableFormatInOneCmapIsRead()
	{
		var font = Font(
			Format0(0x41),
			Format4(Segments()),
			Format6(
				0x0100,
				3),
			Format12((0x1F600, 0x1F600)));

		TrueTypeCmap
			.Codepoints(font)
			.Should()
			.BeEquivalentTo(
				AllFormatsExpected,
				"a cmap carries one subtable per encoding and the reader takes the union of all "
				+ "of them, so all four format branches must run rather than three sitting dead");
	}

	[Fact]
	public void AFontWithNoCmapTableCoversNothing()
	{
		var font = Sfnt(
			"glyf",
			[1, 2, 3, 4]);

		TrueTypeCmap
			.Codepoints(font)
			.Should()
			.BeEmpty(
				"a font that declares no character map covers no codepoint, and saying so is the "
				+ "negative control that makes a non-empty answer mean something");
	}

	[Fact]
	public void ACmapSubtableOffsetPastTheEndOfTheFontCoversNothingRatherThanThrowing()
	{
		var cmap = new List<byte>();

		cmap.AddRange(Be16(0));
		cmap.AddRange(Be16(1));
		cmap.AddRange(Be16(3));
		cmap.AddRange(Be16(1));
		cmap.AddRange(Be32(0x00FFFFFF));

		var font = Sfnt(
			"cmap",
			cmap.ToArray());

		TrueTypeCmap
			.Codepoints(font)
			.Should()
			.BeEmpty(
				"a truncated or malformed font must come back empty rather than throw an index "
				+ "exception out of a helper the calling fact cannot attribute");
	}

	[Fact]
	public void AFormat4SubtableTruncatedMidwayCoversNothingRatherThanThrowing()
	{
		var font = Font(Format4(Segments()));

		TrueTypeCmap
			.Codepoints(font[..(TableDirectoryLength + 20)])
			.Should()
			.BeEmpty(
				"every read is bounds checked, so a subtable whose arrays run past the end of the "
				+ "file contributes nothing instead of failing");
	}

	[Fact]
	public void AFontTooShortToHoldATableDirectoryCoversNothing()
	{
		TrueTypeCmap
			.Codepoints([0, 1, 0, 0])
			.Should()
			.BeEmpty("there is no table directory to read, so there is no cmap to find");
	}

	/// <summary>
	///		The format 4 subtable every format 4 test is built from. Four segments, each present
	///		for a reason a shifted array would break:
	/// </summary>
	/// <remarks>
	///		<para>
	///		U+0041–U+0043 through <c>idDelta</c> — the arm the real font takes.
	///		</para>
	///		<para>
	///		U+0050 through an <c>idDelta</c> of 0xFFB0, which wraps its glyph id to 0 and so means
	///		"no glyph". MEASURED, not assumed: without this segment a two-byte shift of the
	///		<c>idDelta</c> array is INVISIBLE, because every other segment's coverage depends only
	///		on the glyph id being non-zero and a different delta is still non-zero. The first run of
	///		this injection left the format 4 tests green.
	///		</para>
	///		<para>
	///		U+E000–U+E002 through <c>idRangeOffset</c>, with a 0 in the middle of its glyph id
	///		array so that the range-offset arm has to read each entry rather than cover the span.
	///		</para>
	///		<para>
	///		The terminating U+FFFF segment the specification requires and the reader must skip.
	///		</para>
	/// </remarks>
	private static Segment[] Segments()
	{
		return
		[
			new Segment(
				0x0041,
				0x0043,
				0x0010,
				[]),
			new Segment(
				0x0050,
				0x0050,
				0xFFB0,
				[]),
			new Segment(
				0xE000,
				0xE002,
				0x0000,
				[5, 0, 7]),
			new Segment(
				0xFFFF,
				0xFFFF,
				0x0001,
				[]),
		];
	}

	private static byte[] Be16(int value)
	{
		return
		[
			(byte)(value >> 8),
			(byte)value,
		];
	}

	private static byte[] Be32(long value)
	{
		return
		[
			(byte)(value >> 24),
			(byte)(value >> 16),
			(byte)(value >> 8),
			(byte)value,
		];
	}

	/// <summary>
	///		Wraps one table of the supplied four-character tag in a minimal sfnt table directory.
	/// </summary>
	private static byte[] Sfnt(
		string tag,
		byte[] table)
	{
		var font = new List<byte>();

		font.AddRange(Be32(0x00010000));
		font.AddRange(Be16(1));

		// searchRange, entrySelector and rangeShift are binary-search hints the reader never
		// consults, so they are written as zero rather than computed.
		font.AddRange(Be16(0));
		font.AddRange(Be16(0));
		font.AddRange(Be16(0));

		font.AddRange(Encoding.ASCII.GetBytes(tag));
		font.AddRange(Be32(0));
		font.AddRange(Be32(TableDirectoryLength));
		font.AddRange(Be32(table.Length));
		font.AddRange(table);

		return font.ToArray();
	}

	/// <summary>
	///		Builds a font whose <c>cmap</c> carries one encoding record per supplied subtable.
	/// </summary>
	private static byte[] Font(params byte[][] subtables)
	{
		var cmap = new List<byte>();

		cmap.AddRange(Be16(0));
		cmap.AddRange(Be16(subtables.Length));

		var offset = 4 + (subtables.Length * 8);

		foreach (var subtable in subtables)
		{
			cmap.AddRange(Be16(3));
			cmap.AddRange(Be16(1));
			cmap.AddRange(Be32(offset));

			offset += subtable.Length;
		}

		foreach (var subtable in subtables)
		{
			cmap.AddRange(subtable);
		}

		return Sfnt(
			"cmap",
			cmap.ToArray());
	}

	private static byte[] Format0(params int[] codes)
	{
		var subtable = new List<byte>();

		subtable.AddRange(Be16(0));
		subtable.AddRange(Be16(262));
		subtable.AddRange(Be16(0));

		var glyphs = new byte[256];

		foreach (var code in codes)
		{
			glyphs[code] = 1;
		}

		subtable.AddRange(glyphs);

		return subtable.ToArray();
	}

	private static byte[] Format4(Segment[] segments)
	{
		var body = new List<byte>();

		foreach (var segment in segments)
		{
			body.AddRange(Be16(segment.End));
		}

		body.AddRange(Be16(0));

		foreach (var segment in segments)
		{
			body.AddRange(Be16(segment.Start));
		}

		foreach (var segment in segments)
		{
			body.AddRange(Be16(segment.Delta));
		}

		var rangeOffsets = new List<byte>();
		var glyphArray = new List<byte>();

		for (var index = 0; index < segments.Length; index++)
		{
			if (segments[index].Glyphs.Length == 0)
			{
				rangeOffsets.AddRange(Be16(0));

				continue;
			}

			// idRangeOffset is a byte distance measured from its OWN slot, not from the start of
			// the array — which is the detail that makes this format worth a test.
			rangeOffsets.AddRange(Be16(((segments.Length - index) * 2) + glyphArray.Count));

			foreach (var glyph in segments[index].Glyphs)
			{
				glyphArray.AddRange(Be16(glyph));
			}
		}

		body.AddRange(rangeOffsets);
		body.AddRange(glyphArray);

		var subtable = new List<byte>();

		subtable.AddRange(Be16(4));
		subtable.AddRange(Be16(14 + body.Count));
		subtable.AddRange(Be16(0));
		subtable.AddRange(Be16(segments.Length * 2));
		subtable.AddRange(Be16(0));
		subtable.AddRange(Be16(0));
		subtable.AddRange(Be16(0));
		subtable.AddRange(body);

		return subtable.ToArray();
	}

	private static byte[] Format6(
		int first,
		params int[] glyphs)
	{
		var subtable = new List<byte>();

		subtable.AddRange(Be16(6));
		subtable.AddRange(Be16(10 + (glyphs.Length * 2)));
		subtable.AddRange(Be16(0));
		subtable.AddRange(Be16(first));
		subtable.AddRange(Be16(glyphs.Length));

		foreach (var glyph in glyphs)
		{
			subtable.AddRange(Be16(glyph));
		}

		return subtable.ToArray();
	}

	private static byte[] Format12(params (int Start, int End)[] groups)
	{
		var subtable = new List<byte>();

		subtable.AddRange(Be16(12));
		subtable.AddRange(Be16(0));
		subtable.AddRange(Be32(16 + (groups.Length * 12)));
		subtable.AddRange(Be32(0));
		subtable.AddRange(Be32(groups.Length));

		var glyph = 1;

		foreach (var group in groups)
		{
			subtable.AddRange(Be32(group.Start));
			subtable.AddRange(Be32(group.End));
			subtable.AddRange(Be32(glyph));

			glyph += group.End - group.Start + 1;
		}

		return subtable.ToArray();
	}
}
