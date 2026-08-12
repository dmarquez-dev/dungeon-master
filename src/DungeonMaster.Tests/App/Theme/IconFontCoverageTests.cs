using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Ratchet binding every control that declares the icon font to the glyphs that font can
///		actually draw.
/// </summary>
/// <remarks>
///		<para>
///			<c>RpgAwesome</c> maps private-use codepoints and the space, and nothing else. A control
///			that declares <c>FontFamily="{StaticResource DmFontIcon}"</c> and then sets its
///			<c>Text</c> to an ordinary symbol — a close cross, a triangular disclosure arrow, a
///			vertical ellipsis — is asking for a glyph the font does not have. Nothing goes missing
///			on screen, because the platform silently substitutes a system font; only the metrics
///			change, so the glyph lands off-centre and differently on every machine.
///		</para>
///		<para>
///			That is invisible to a build, invisible to a launch and invisible to a screenshot taken
///			by the person who wrote it. It has now been shipped twice: once in
///			<c>DungeonDetailPage.xaml</c>, and once across six further controls that a hand audit
///			scoped to the first file did not reach. A file-scoped audit cannot catch a class of
///			defect; reading the font's own <c>cmap</c> can.
///		</para>
///		<para>
///			Only the literal attribute form is checked. A <c>Text</c> assigned from code-behind or
///			bound is outside a static scan, which is why the sibling facts in
///			<c>DungeonMaster.Tests.App.Accessibility</c> exist — but a code-behind assignment still
///			inherits the <c>FontFamily</c> its XAML declares, so fixing the declaration here fixes
///			the imperative writer with it.
///		</para>
/// </remarks>
public class IconFontCoverageTests
{
    /// <summary>
    ///		The file backing the <c>RpgAwesome</c> alias that <c>DmFontIcon</c> names. Read as a
    ///		build asset out of the application project rather than from the build output, so the
    ///		fact runs against the font the source tree ships.
    /// </summary>
    private const string IconFontFile = "rpgawesome-webfont.ttf";

    /// <summary>
    ///		Matches the opening tag of any element together with its own attributes.
    ///		<c>[^&gt;]</c> cannot cross into a nested element, so the attribute run belongs to this
    ///		tag and no other.
    /// </summary>
    private static readonly Regex ElementTag =
        new(
            @"<(?<element>[A-Za-z_][\w.:-]*)\b(?<attributes>[^>]*?)/?>",
            RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex IconFontAttribute =
        new(
            @"FontFamily\s*=\s*""\{\s*StaticResource\s+DmFontIcon\s*\}""",
            RegexOptions.Compiled);

    private static readonly Regex TextAttribute =
        new(
            @"(?<![A-Za-z.])Text\s*=\s*""(?<value>[^""]*)""",
            RegexOptions.Compiled);

    private static readonly Regex CharacterEntity =
        new(
            @"&#(?<hex>x?)(?<code>[0-9A-Fa-f]+);",
            RegexOptions.Compiled);

    private static readonly Regex XmlComment =
        new(
            @"<!--.*?-->",
            RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    ///		Every literal <c>Text</c> on a control declaring <c>DmFontIcon</c> must be drawable by
    ///		the icon font.
    /// </summary>
    [Fact]
    public void EveryDmFontIconTextIsInTheIconFontsCmap()
    {
        var covered = IconFontCodepoints();

        var offenders = new List<string>();
        var checkedControls = 0;

        foreach (var (relative, source) in XamlSources())
        {
            var text = Blank(source);

            foreach (Match tag in ElementTag.Matches(text))
            {
                var attributes = tag.Groups["attributes"].Value;

                if (!IconFontAttribute.IsMatch(attributes))
                {
                    continue;
                }

                var declaredText = TextAttribute.Match(attributes);

                if (!declaredText.Success)
                {
                    continue;
                }

                checkedControls++;

                var glyphs = Decode(declaredText.Groups["value"].Value);
                var line = LineNumber(
                    text,
                    tag.Index);

                foreach (var codepoint in Codepoints(glyphs))
                {
                    if (covered.Contains(codepoint))
                    {
                        continue;
                    }

                    offenders.Add(
                        $"{relative}:{line} -> <{tag.Groups["element"].Value}> declares DmFontIcon "
                        + $"but its Text contains U+{codepoint:X4}, which RpgAwesome does not map");
                }
            }
        }

        offenders
            .Should()
            .BeEmpty(
                "a glyph outside the icon font's cmap is drawn by whatever the platform "
                + "substitutes, so it renders with foreign metrics and differs by machine; use "
                + "DmFontBody for a glyph the icon font does not carry");

        // Anti-vacuity. Four plausible edits make the scan above reach nothing while staying
        // green: the alias renamed, the fonts folder moved, DmFontIcon declared through a Setter
        // instead of an attribute, or every icon Text moved to code-behind. Each of those must be
        // a failure here rather than a silent retirement of the guard.
        checkedControls
            .Should()
            .BeGreaterThan(
                0,
                "the scan must reach at least one control declaring DmFontIcon with a literal "
                + "Text, or this fact guards nothing");
    }

    /// <summary>
    ///		Asserts the font parse itself is honest: a <c>cmap</c> read that silently produced an
    ///		empty set would make the fact above pass everything, forever.
    /// </summary>
    [Fact]
    public void TheIconFontCmapParsesToPrivateUseGlyphs()
    {
        var covered = IconFontCodepoints();

        covered
            .Should()
            .NotBeEmpty("the cmap parse must find glyphs or the coverage fact is vacuous");

        covered
            .Where(codepoint =>
                codepoint is >= 0xE000 and <= 0xF8FF)
            .Should()
            .NotBeEmpty(
                "RpgAwesome maps its icons into the private-use area, so a parse that finds none "
                + "of them has read the wrong table");

        // The two codepoints the application actually asks the icon font for at its two extremes.
        // Named literally so a parse that returns a plausible-looking but wrong range fails.
        covered.Should().Contain(0xEA8F, "the notebook glyph is in use at FloorSidebarItem.xaml");
        covered.Should().Contain(0xEAD4, "the divider ornament is in use at DmDivider.xaml");
    }

    private static IEnumerable<(string Relative, string Source)> XamlSources()
    {
        foreach (var path in RepoPaths.AppSourceFiles("*.xaml"))
        {
            yield return (
                Path.GetRelativePath(
                    RepoPaths.AppRoot,
                    path),
                File.ReadAllText(path));
        }
    }

    /// <summary>
    ///		Blanks XML comments out rather than deleting them, so a comment containing something
    ///		that looks like markup cannot be matched while every reported line number still counts
    ///		against the file as it is on disk. Deleting them shifted every offender's line, which
    ///		is precisely the stale-citation failure this branch has been correcting elsewhere.
    /// </summary>
    private static string Blank(string source) =>
        XmlComment.Replace(
            source,
            match =>
                new string(
                    match
                        .Value
                        .Select(character =>
                            character is '\n' or '\r'
                                ? character
                                : ' ')
                        .ToArray()));

    private static string Decode(string attributeValue) =>
        CharacterEntity
            .Replace(
                attributeValue,
                match =>
                    char.ConvertFromUtf32(
                        int.Parse(
                            match.Groups["code"].Value,
                            match.Groups["hex"].Value.Length == 0
                                ? NumberStyles.Integer
                                : NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture)))
            .Replace(
                "&amp;",
                "&",
                StringComparison.Ordinal);

    /// <summary>
    ///		Yields the codepoints a font would have to draw. Whitespace is dropped because it is
    ///		layout rather than a glyph, and a markup binding is dropped because its value is not
    ///		knowable from the source.
    /// </summary>
    private static IEnumerable<int> Codepoints(string text)
    {
        if (text.Contains(
                '{',
                StringComparison.Ordinal))
        {
            yield break;
        }

        foreach (var rune in text.EnumerateRunes())
        {
            if (Rune.IsWhiteSpace(rune))
            {
                continue;
            }

            yield return rune.Value;
        }
    }

    private static int LineNumber(
        string text,
        int index) =>
        text
            .Take(index)
            .Count(character =>
                character == '\n') + 1;

    private static HashSet<int> IconFontCodepoints()
    {
        var path = Path.Combine(
            RepoPaths.AppRoot,
            "Resources",
            "Fonts",
            IconFontFile);

        File
            .Exists(path)
            .Should()
            .BeTrue($"the icon font must be readable at '{path}' for this fact to mean anything");

        return TrueTypeCmap.Codepoints(File.ReadAllBytes(path));
    }

    /// <summary>
    ///		The smallest <c>cmap</c> reader that answers "can this font draw this codepoint".
    ///		No glyph outlines, no shaping — only the character-to-glyph table, and only the four
    ///		subtable formats a desktop TTF is permitted to use for it.
    /// </summary>
    private static class TrueTypeCmap
    {
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
}
