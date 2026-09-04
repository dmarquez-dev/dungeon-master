using System.Text;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		The smallest <c>cmap</c> reader that answers "can this font draw this codepoint".
///		No glyph outlines, no shaping — only the character-to-glyph table, and only the four
///		subtable formats a desktop TTF is permitted to use for it.
/// </summary>
/// <remarks>
///		<para>
///		MASTER-101 / TEST-015. This was a private nested class inside
///		<see cref="IconFontCoverageTests"/>, reachable only through a fact about icon glyphs. Three
///		of its four subtable branches — formats 0, 6 and 12 — were never executed by any test,
///		because the one font the suite reads declares format 4 and nothing else. A parser living
///		inside a test is a parser nobody tests; it is a named unit here so that
///		<see cref="TrueTypeCmapTests"/> can drive every branch with a synthetic subtable.
///		</para>
///		<para>
///		All four formats are KEPT rather than narrowed to format 4. Narrowing would make the
///		parser throw the moment a font carrying a Macintosh format 0 or a supplementary-plane
///		format 12 subtable is added to <c>Resources/Fonts</c>, which is a normal thing for a font
///		to carry; keeping them costs nothing now that each is covered.
///		</para>
///		<para>
///		Nothing here asserts and nothing here throws on malformed input. Every read is bounds
///		checked and a table this reader cannot make sense of contributes no codepoints, so the
///		caller's own guard fact — not an exception from inside a helper — is what reports a scan
///		that reached nothing. See <c>contracts/anti-vacuity.md</c> section 1.
///		</para>
/// </remarks>
internal static class TrueTypeCmap
{
	/// <summary>
	///		Returns every codepoint the font's <c>cmap</c> maps to a non-zero glyph.
	/// </summary>
	/// <param name="font">
	///		The whole font file as bytes.
	/// </param>
	/// <returns>
	///		The covered codepoints. EMPTY when the font carries no <c>cmap</c>, when the table
	///		directory does not fit, or when every subtable offset points outside the file — an
	///		empty result is the honest answer to an unreadable font, never an exception.
	/// </returns>
	public static HashSet<int> Codepoints(byte[] font)
	{
		var covered = new HashSet<int>();
		var cmap = TableOffset(
			font,
			"cmap");

		if (cmap < 0)
		{
			return covered;
		}

		var subtables = ReadUInt16(
			font,
			cmap + 2);

		for (var record = 0; record < subtables; record++)
		{
			var recordOffset = cmap + 4 + (record * 8);

			if (recordOffset + 8 > font.Length)
			{
				break;
			}

			var subtable = cmap
				+ (int)ReadUInt32(
					font,
					recordOffset + 4);

			if (subtable < 0 || subtable + 4 > font.Length)
			{
				continue;
			}

			ReadSubtable(
				font,
				subtable,
				covered);
		}

		return covered;
	}

	private static void ReadSubtable(
		byte[] font,
		int subtable,
		HashSet<int> covered)
	{
		switch (ReadUInt16(
			font,
			subtable))
		{
			case 0:
				ReadFormat0(
					font,
					subtable,
					covered);

				break;

			case 4:
				ReadFormat4(
					font,
					subtable,
					covered);

				break;

			case 6:
				ReadFormat6(
					font,
					subtable,
					covered);

				break;

			case 12:
				ReadFormat12(
					font,
					subtable,
					covered);

				break;
		}
	}

	/// <summary>
	///		Format 0 — a flat byte-per-codepoint array covering U+0000 to U+00FF, at
	///		<c>subtable + 6</c>.
	/// </summary>
	private static void ReadFormat0(
		byte[] font,
		int subtable,
		HashSet<int> covered)
	{
		for (var code = 0; code < 256; code++)
		{
			var glyphIndex = subtable + 6 + code;

			if (glyphIndex < font.Length && font[glyphIndex] != 0)
			{
				covered.Add(code);
			}
		}
	}

	/// <summary>
	///		Format 4 — segmented coverage of the basic multilingual plane, and the branch the
	///		real font actually takes.
	/// </summary>
	/// <remarks>
	///		The four parallel arrays follow the header back to back, separated only by the single
	///		reserved padding word after <c>endCode</c>. Every one of those four offsets is an
	///		off-by-one waiting to happen, and an off-by-one here does not throw: it returns a
	///		plausible-looking codepoint set that the coverage fact then trusts as ground truth.
	///		That is why <see cref="TrueTypeCmapTests"/> pins the arithmetic against a hand-built
	///		subtable with a known expected answer.
	/// </remarks>
	private static void ReadFormat4(
		byte[] font,
		int subtable,
		HashSet<int> covered)
	{
		var segmentCountTimesTwo = ReadUInt16(
			font,
			subtable + 6);

		var endCodes = subtable + 14;
		var startCodes = endCodes + segmentCountTimesTwo + 2;
		var deltas = startCodes + segmentCountTimesTwo;
		var rangeOffsets = deltas + segmentCountTimesTwo;

		for (var segment = 0; segment < segmentCountTimesTwo / 2; segment++)
		{
			var end = ReadUInt16(
				font,
				endCodes + (segment * 2));

			var start = ReadUInt16(
				font,
				startCodes + (segment * 2));

			var delta = ReadUInt16(
				font,
				deltas + (segment * 2));

			var rangeOffsetPosition = rangeOffsets + (segment * 2);

			var rangeOffset = ReadUInt16(
				font,
				rangeOffsetPosition);

			for (var code = (int)start; code <= end && code != 0xFFFF; code++)
			{
				ushort glyph;

				if (rangeOffset == 0)
				{
					glyph = (ushort)((code + delta) & 0xFFFF);
				}
				else
				{
					var glyphPosition = rangeOffsetPosition
						+ rangeOffset
						+ ((code - start) * 2);

					if (glyphPosition + 2 > font.Length)
					{
						continue;
					}

					glyph = ReadUInt16(
						font,
						glyphPosition);

					if (glyph != 0)
					{
						glyph = (ushort)((glyph + delta) & 0xFFFF);
					}
				}

				if (glyph != 0)
				{
					covered.Add(code);
				}
			}
		}
	}

	/// <summary>
	///		Format 6 — a dense run of <c>count</c> glyph ids beginning at <c>first</c>.
	/// </summary>
	private static void ReadFormat6(
		byte[] font,
		int subtable,
		HashSet<int> covered)
	{
		var first = ReadUInt16(
			font,
			subtable + 6);

		var count = ReadUInt16(
			font,
			subtable + 8);

		for (var entry = 0; entry < count; entry++)
		{
			var glyphPosition = subtable + 10 + (entry * 2);

			if (glyphPosition + 2 > font.Length)
			{
				break;
			}

			if (ReadUInt16(
					font,
					glyphPosition)
				!= 0)
			{
				covered.Add(first + entry);
			}
		}
	}

	/// <summary>
	///		Format 12 — 32-bit segmented coverage, the only format that can name a
	///		supplementary-plane codepoint.
	/// </summary>
	private static void ReadFormat12(
		byte[] font,
		int subtable,
		HashSet<int> covered)
	{
		var groups = (int)ReadUInt32(
			font,
			subtable + 12);

		for (var group = 0; group < groups; group++)
		{
			var groupOffset = subtable + 16 + (group * 12);

			if (groupOffset + 12 > font.Length)
			{
				break;
			}

			var start = (int)ReadUInt32(
				font,
				groupOffset);

			var end = (int)ReadUInt32(
				font,
				groupOffset + 4);

			for (var code = start; code <= end && code <= 0x10FFFF; code++)
			{
				covered.Add(code);
			}
		}
	}

	/// <summary>
	///		Returns the file offset of the named table, or <c>-1</c> when the font does not carry
	///		it or is too short to hold a table directory at all.
	/// </summary>
	private static int TableOffset(
		byte[] font,
		string tag)
	{
		if (font.Length < 12)
		{
			return -1;
		}

		var tables = ReadUInt16(
			font,
			4);

		for (var table = 0; table < tables; table++)
		{
			var record = 12 + (table * 16);

			if (record + 16 > font.Length)
			{
				break;
			}

			var name = Encoding.ASCII.GetString(
				font,
				record,
				4);

			if (string.Equals(
					name,
					tag,
					StringComparison.Ordinal))
			{
				return (int)ReadUInt32(
					font,
					record + 8);
			}
		}

		return -1;
	}

	private static ushort ReadUInt16(
		byte[] font,
		int offset) =>
		offset + 2 > font.Length
			? (ushort)0
			: (ushort)((font[offset] << 8) | font[offset + 1]);

	private static uint ReadUInt32(
		byte[] font,
		int offset) =>
		offset + 4 > font.Length
			? 0u
			: ((uint)font[offset] << 24)
			| ((uint)font[offset + 1] << 16)
			| ((uint)font[offset + 2] << 8)
			| font[offset + 3];
}
