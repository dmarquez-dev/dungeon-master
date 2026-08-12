using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Ratchet guarding the migration of application source onto the theme dictionaries.
///		Every file that still carries a raw colour, font size or font family is named in
///		<see cref="PendingMigration"/>; removing an entry makes these facts fail until the
///		file is migrated.
/// </summary>
public class ThemeHygieneTests
{
    // (?<![A-Za-z]) stops this matching DmDivider's IconFontSize bindable property.
    private static readonly Regex BareFontSizePattern =
        new(
            @"(?<![A-Za-z])FontSize=""[0-9]",
            RegexOptions.Compiled);

    private static readonly Regex BareFontFamilyPattern =
        new(
            @"FontFamily=""[A-Za-z]",
            RegexOptions.Compiled);

    // The C# forms of the two typography literals. (?<![A-Za-z]) keeps IconFontSize out, and
    // =(?!>) keeps expression-bodied members out. Both patterns match an assignment only —
    // "FontSize = 13", "canvas.FontSize = 9f", "FontFamily = \"EBGaramondRegular\"" — so
    // "nameof(FontSize)" and a bindable property's positional default are outside their reach.
    // That exemption is deliberate and named in the PendingMigration comment below.
    private static readonly Regex CSharpBareFontSizePattern =
        new(
            @"(?<![A-Za-z])FontSize\s*=(?!>)\s*[0-9]",
            RegexOptions.Compiled);

    private static readonly Regex CSharpBareFontFamilyPattern =
        new(
            @"(?<![A-Za-z])FontFamily\s*=(?!>)\s*""",
            RegexOptions.Compiled);

    private static readonly Regex LegacyThemeKeyPattern =
        new(
            @"\{StaticResource\s+DM[A-Za-z0-9]*\s*\}",
            RegexOptions.Compiled);

    /// <summary>
    ///		Files not yet migrated onto the theme system. THE MIGRATION IS COMPLETE and this set
    ///		is empty: every colour, font size, font family and theme key in the application
    ///		resolves from Resources/Theme. It stays here as the ratchet's escape hatch — if a new
    ///		file has to carry a literal for a while, name it here and the facts below go green
    ///		again, but an entry that stops offending is itself a failure, so nothing can be
    ///		parked and forgotten.
    ///
    ///		TWO FILES SURVIVE BY DESIGN, and neither is reachable by these patterns:
    ///
    ///		  Controls\DmDivider\DmDivider.xaml.cs        IconFontSize default 16.0
    ///		  Controls\DmInlineEditor\DmInlineEditor.xaml.cs  FontSize 18.0, FontFamily "EBGaramondRegular"
    ///
    ///		A BindableProperty default is evaluated in a static field initialiser, which runs at
    ///		type-initialisation time and can precede Application.Current.Resources — so it cannot
    ///		read DmTheme without throwing. They are declared positionally
    ///		(<c>typeof(DmDivider), 16.0</c>), not as an assignment, so the two C# patterns above
    ///		never see them. BindablePropertyDefaultsMatchTokensTests compares each against the
    ///		token it mirrors instead, which is the guard that actually fits their shape.
    /// </summary>
    private static readonly HashSet<string> PendingMigration = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///		Asserts that colour literals appear only in the theme dictionaries.
    /// </summary>
    [Fact]
    public void NoRawColourLiteralsOutsideTheThemeDictionaries()
    {
        Offenders("*.xaml", ThemeSource.HexLiteralPattern)
            .Concat(Offenders("*.cs", ThemeSource.HexLiteralPattern))
            .Should()
            .BeEmpty();
    }

    /// <summary>
    ///		Asserts that numeric font sizes appear only in the theme dictionaries, in markup and
    ///		in code-behind alike.
    /// </summary>
    [Fact]
    public void NoBareFontSizesOutsideTheThemeDictionaries()
    {
        Offenders("*.xaml", BareFontSizePattern)
            .Concat(Offenders("*.cs", CSharpBareFontSizePattern))
            .Should()
            .BeEmpty();
    }

    /// <summary>
    ///		Asserts that font family names appear only in the theme dictionaries, in markup and
    ///		in code-behind alike.
    /// </summary>
    [Fact]
    public void NoBareFontFamiliesOutsideTheThemeDictionaries()
    {
        Offenders("*.xaml", BareFontFamilyPattern)
            .Concat(Offenders("*.cs", CSharpBareFontFamilyPattern))
            .Should()
            .BeEmpty();
    }

    /// <summary>
    ///		Asserts that no migrated file still references a legacy <c>DM</c>-prefixed key from
    ///		<c>Resources/Styles/Colors.xaml</c>, which Task 10 deleted. Migrating a file means
    ///		moving it onto the <c>Dm</c>-prefixed tokens in <c>Resources/Theme/Tokens.xaml</c>,
    ///		not merely off raw literals.
    /// </summary>
    [Fact]
    public void NoLegacyThemeKeyReferencesOutsideThePendingMigrationSet()
    {
        Offenders("*.xaml", LegacyThemeKeyPattern).Should().BeEmpty();
    }

    /// <summary>
    ///		Asserts that every allow-listed path names a file that exists and still carries at
    ///		least one of the literals the three facts above forbid. Without this the ratchet is
    ///		not self-checking as it shrinks: a stale or mistyped entry exempts nothing, and a
    ///		migration task can "remove its entry" and see green because the entry never bit.
    ///		<para>
    ///		THIS FACT IS VACUOUS TODAY AND THAT IS THE POINT. <see cref="PendingMigration"/> is
    ///		empty because the migration finished, so there is nothing to check and the loop below
    ///		runs zero times. It is NOT dead code: it is a self-arming ratchet that starts checking
    ///		the moment somebody adds the first new entry, which is exactly when it is needed and
    ///		exactly when nobody would think to write it. Deleting it because "it asserts nothing"
    ///		removes the guard from every future allow-list entry.
    ///		</para>
    /// </summary>
    [Fact]
    public void EveryAllowListedFileExistsAndStillOffends()
    {
        var stale = PendingMigration
            .Where(relative =>
                !StillOffends(relative))
            .Select(relative =>
                File.Exists(
                    Path.Combine(
                        RepoPaths.AppRoot,
                        relative))
                    ? $"{relative} -> no longer offends; remove it from PendingMigration"
                    : $"{relative} -> file does not exist; the entry exempts nothing")
            .OrderBy(
                entry =>
                    entry,
                StringComparer.Ordinal)
            .ToList();

        stale.Should().BeEmpty();
    }

    private static bool StillOffends(string relative)
    {
        var path = Path.Combine(
            RepoPaths.AppRoot,
            relative);

        if (!File.Exists(path))
        {
            return false;
        }

        var text = File.ReadAllText(path);

        return ThemeSource.HexLiteralPattern.IsMatch(text)
            || BareFontSizePattern.IsMatch(text)
            || BareFontFamilyPattern.IsMatch(text)
            || CSharpBareFontSizePattern.IsMatch(text)
            || CSharpBareFontFamilyPattern.IsMatch(text)
            || LegacyThemeKeyPattern.IsMatch(text);
    }

    private static IEnumerable<string> Offenders(
        string searchPattern,
        Regex pattern)
    {
        var separator = Path.DirectorySeparatorChar;

        var scanned = RepoPaths
            .AppSourceFiles(searchPattern)
            .Select(path =>
                Path.GetRelativePath(
                    RepoPaths.AppRoot,
                    path))
            .Where(relative =>
                !relative.StartsWith(
                    $"Resources{separator}Theme{separator}",
                    StringComparison.OrdinalIgnoreCase))
            .Where(relative =>
                !PendingMigration.Contains(relative))
            .ToList();

        // Without this, a directory rename or a widened exclusion turns all five hygiene facts
        // green while they scan nothing at all — the failure mode behind three of the five
        // structurally-inert guards this migration shipped.
        scanned
            .Should()
            .NotBeEmpty($"the '{searchPattern}' scan must find application sources or every "
                + "hygiene fact built on it is vacuous");

        var offenders = new List<string>();

        foreach (var relative in scanned)
        {
            var text = File.ReadAllText(
                Path.Combine(
                    RepoPaths.AppRoot,
                    relative));

            // Reported with the line number and the matched text: the bare relative path alone
            // left a 700-line XAML file to be searched by hand for what tripped the pattern.
            foreach (Match match in pattern.Matches(text))
            {
                var line = ThemeSource.LineNumber(
                    text,
                    match.Index);

                offenders.Add($"{relative}:{line} -> {match.Value}");
            }
        }

        return offenders;
    }
}
