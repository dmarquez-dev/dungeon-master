using System.Globalization;
using System.Text.RegularExpressions;
using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Accessibility;

/// <summary>
///		Ratchet for the accessibility of icon-only controls.
/// </summary>
/// <remarks>
///		<para>
///			Before this fact the application declared zero <c>SemanticProperties</c> and zero
///			<c>AutomationId</c>s, across roughly twenty controls whose entire label is a private-use
///			codepoint out of an icon font. A screen reader announces nothing usable for any of them
///			— not a wrong name, no name at all — and nothing in the build, the suite or the running
///			app said so.
///		</para>
///		<para>
///			A <c>ToolTipProperties.Text</c> is not a substitute. It is a hover affordance, so it
///			reaches a mouse user and nobody else.
///		</para>
/// </remarks>
public class SemanticDescriptionTests
{
    /// <summary>
    ///		Matches the opening tag of a <c>Label</c> or <c>Button</c>, including the multi-line
    ///		attribute layout this project writes. <c>[^&gt;]</c> cannot cross into a nested element,
    ///		so the match is the tag's own attributes and nothing else.
    /// </summary>
    private static readonly Regex ControlTag =
        new(
            @"<(?<element>Label|Button)\b(?<attributes>[^>]*?)/?>",
            RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex TextAttribute =
        new(
            @"\sText=""(?<value>[^""]*)""",
            RegexOptions.Compiled);

    private static readonly Regex CharacterEntity =
        new(
            @"&#x(?<code>[0-9A-Fa-f]+);",
            RegexOptions.Compiled);

    /// <summary>
    ///		Files whose icon-only controls are knowingly still undescribed. Empty, and meant to
    ///		stay that way: every offender this fact found when it was written was fixed rather than
    ///		listed, so an entry here is a regression with a note attached, not a normal state.
    /// </summary>
    private static readonly IReadOnlySet<string> PendingDescription =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///		Every <c>Label</c> or <c>Button</c> whose entire <c>Text</c> is a single private-use or
    ///		symbol codepoint must either carry a <c>SemanticProperties.Description</c> or be taken
    ///		out of the accessibility tree outright.
    /// </summary>
    /// <remarks>
    ///		The second option is not a loophole. A glyph sitting on top of an already-labelled
    ///		button, or beside a wordmark that says the same thing, has no announcement of its own to
    ///		make, and <c>AutomationProperties.IsInAccessibleTree="False"</c> is how MAUI says so —
    ///		as opposed to leaving it in the tree with nothing to read out.
    /// </remarks>
    [Fact]
    public void EveryIconOnlyControlHasASemanticDescription()
    {
        var offenders = new List<string>();

        foreach (var path in RepoPaths.AppSourceFiles("*.xaml"))
        {
            var relative = Path.GetRelativePath(
                RepoPaths.AppRoot,
                path);

            if (PendingDescription.Contains(relative))
            {
                continue;
            }

            var source = File.ReadAllText(path);

            foreach (Match tag in ControlTag.Matches(source))
            {
                var attributes = tag.Groups["attributes"].Value;
                var text = TextAttribute.Match(attributes);

                if (!text.Success)
                {
                    continue;
                }

                var glyph = Decode(text.Groups["value"].Value);

                if (!IsIconOnly(glyph))
                {
                    continue;
                }

                if (attributes.Contains("SemanticProperties.Description")
                    || attributes.Contains(@"AutomationProperties.IsInAccessibleTree=""False"""))
                {
                    continue;
                }

                var line = source
                    .Take(tag.Index)
                    .Count(character =>
                        character == '\n') + 1;

                offenders.Add(
                    $"{relative}:{line} -> <{tag.Groups["element"].Value} "
                    + $"Text=\"U+{(int)glyph[0]:X4}\"> has no SemanticProperties.Description "
                    + "and is not excluded from the accessibility tree");
            }
        }

        offenders
            .Should()
            .BeEmpty(
                "a control whose whole label is an icon codepoint announces nothing to a screen "
                + "reader unless it carries SemanticProperties.Description, or is marked "
                + @"AutomationProperties.IsInAccessibleTree=""False"" because something beside it "
                + "already says the same thing");
    }

    /// <summary>
    ///		Every file named by <see cref="PendingDescription"/> must exist and must still contain
    ///		an undescribed icon-only control.
    /// </summary>
    /// <remarks>
    ///		Without this, the allow-list decays into a list of files that were once a problem, and
    ///		the ratchet quietly stops applying to whatever is added to them later. It is the same
    ///		shape as the geometry suite's <c>EveryGeometryAllowListEntryStillOffends</c>, and it is
    ///		why the list above can safely be empty: an empty list has nothing to go stale.
    /// </remarks>
    [Fact]
    public void EverySemanticAllowListEntryStillOffends()
    {
        var stale = new List<string>();

        foreach (var relative in PendingDescription)
        {
            var path = Path.Combine(
                RepoPaths.AppRoot,
                relative);

            if (!File.Exists(path))
            {
                stale.Add($"{relative} -> file does not exist; the entry exempts nothing");
                continue;
            }

            if (!HasUndescribedIconControl(File.ReadAllText(path)))
            {
                stale.Add($"{relative} -> no longer offends; remove it from the allow-list");
            }
        }

        stale
            .Should()
            .BeEmpty("an allow-list entry that no longer exempts anything hides the next offender");
    }

    private static bool HasUndescribedIconControl(string source) =>
        ControlTag
            .Matches(source)
            .Select(tag =>
                tag.Groups["attributes"].Value)
            .Any(attributes =>
            {
                var text = TextAttribute.Match(attributes);

                return text.Success
                    && IsIconOnly(Decode(text.Groups["value"].Value))
                    && !attributes.Contains("SemanticProperties.Description")
                    && !attributes.Contains(@"AutomationProperties.IsInAccessibleTree=""False""");
            });

    private static string Decode(string attributeValue) =>
        CharacterEntity
            .Replace(
                attributeValue,
                match =>
                    ((char)int.Parse(
                        match.Groups["code"].Value,
                        NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture))
                    .ToString())
            .Replace(
                "&amp;",
                "&",
                StringComparison.Ordinal);

    /// <summary>
    ///		A control is icon-only when its whole text is one private-use or symbol codepoint.
    ///		Letters and digits are excluded, so the toolbar's shortcut letters and the zoom
    ///		readout are not swept up; a run of two or more characters is excluded too, because
    ///		something like "&#x2190; Back" carries a word a screen reader can read.
    /// </summary>
    private static bool IsIconOnly(string text)
    {
        if (text.Length != 1)
        {
            return false;
        }

        return CharUnicodeInfo.GetUnicodeCategory(text[0]) switch
        {
            UnicodeCategory.PrivateUse => true,
            UnicodeCategory.OtherSymbol => true,
            UnicodeCategory.MathSymbol => true,
            UnicodeCategory.ModifierSymbol => true,
            UnicodeCategory.CurrencySymbol => true,
            _ => false,
        };
    }
}
