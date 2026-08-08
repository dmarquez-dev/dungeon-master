using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Guards <c>DmTheme</c>, the C# accessor over the theme dictionaries. Every key it resolves
///		is a string literal, so a typo or a deleted token fails at runtime rather than at build;
///		this fact turns that into a test failure instead.
/// </summary>
public class DmThemeKeysResolveTests
{
    private static readonly Regex ResolveCallPattern =
        new(
            @"Resolve<(?<type>[\w.]+)>\(""(?<key>Dm[A-Za-z0-9]*)""\)",
            RegexOptions.Compiled);

    private static readonly Regex KeyedElementPattern =
        new(
            @"<(?<element>[A-Za-z][A-Za-z0-9:.]*)[^>]*?x:Key=""(?<key>Dm[A-Za-z0-9]*)""",
            RegexOptions.Compiled);

    /// <summary>
    ///		The XAML element each <c>Resolve&lt;T&gt;</c> generic argument requires the declaring
    ///		token to be. <c>Resolve&lt;T&gt;</c> returns only when <c>value is T</c>, and otherwise
    ///		throws with a message naming the two causes that are NOT this one — a missing key, or a
    ///		read that ran before <c>Application.Current.Resources</c> existed. A token whose element
    ///		is changed under a call site therefore fails at runtime with a message that points away
    ///		from the fault.
    /// </summary>
    private static readonly Dictionary<string, string> ExpectedDeclaringElement =
        new(StringComparer.Ordinal)
        {
            ["Color"] = "Color",
            ["Brush"] = "SolidColorBrush",
            ["double"] = "x:Double",
            ["string"] = "x:String",
            ["Thickness"] = "Thickness",
        };

    /// <summary>
    ///		Asserts that every key <c>DmTheme</c> asks for is defined under Resources/Theme.
    /// </summary>
    [Fact]
    public void EveryKeyDmThemeResolves_IsDefinedInTheThemeDictionaries()
    {
        var requested = ResolveCalls()
            .Select(call =>
                call.Key)
            .ToHashSet(StringComparer.Ordinal);

        var defined = KeyedElementsInThemeDictionaries()
            .Keys
            .ToHashSet(StringComparer.Ordinal);

        requested.Except(defined).Should().BeEmpty();
    }

    /// <summary>
    ///		Asserts that every key <c>DmTheme</c> resolves is declared by the XAML element the
    ///		generic argument requires. The sibling fact above proves only that a key EXISTS; both
    ///		its call pattern and its definition pattern discard the type, so a token converted from
    ///		<c>&lt;Color&gt;</c> to <c>&lt;SolidColorBrush&gt;</c> — or a <c>&lt;x:Double&gt;</c>
    ///		retyped as an <c>&lt;x:String&gt;</c> — leaves the whole suite green and throws on first
    ///		read at runtime, with a message that blames a missing key or a startup ordering fault.
    ///		This is the fact that reads the type.
    /// </summary>
    [Fact]
    public void EveryKeyDmThemeResolves_IsDeclaredAsTheTypeItIsResolvedAs()
    {
        var declarations = KeyedElementsInThemeDictionaries();

        var mismatches = new List<string>();

        foreach (var call in ResolveCalls())
        {
            if (!ExpectedDeclaringElement.TryGetValue(call.Type, out var expectedElement))
            {
                mismatches.Add(
                    $"DmTheme resolves '{call.Key}' as {call.Type}, a type this fact has no "
                    + "expected declaring element for; extend ExpectedDeclaringElement");

                continue;
            }

            if (!declarations.TryGetValue(call.Key, out var declaration))
            {
                // A key that is absent altogether is the sibling fact's finding, not this one's.
                continue;
            }

            if (!string.Equals(
                declaration.Element,
                expectedElement,
                StringComparison.Ordinal))
            {
                mismatches.Add(
                    $"DmTheme resolves '{call.Key}' as {call.Type} but {declaration.FileName} "
                    + $"declares it as <{declaration.Element}>, so Resolve<{call.Type}> will throw "
                    + $"at runtime — it expects <{expectedElement}>");
            }
        }

        mismatches.Should().BeEmpty();
    }

    /// <summary>
    ///		Asserts that no static field initialiser in the application reads <c>DmTheme</c>.
    ///		A field initialiser runs at type-initialisation time, which can precede
    ///		<c>Application.Current.Resources</c>; <c>DmTheme</c> throws when those resources are
    ///		absent, and the message reads like a missing key when the real fault is the ordering.
    ///		This class of bug was hit twice during the theme migration, so it is guarded rather
    ///		than merely commented.
    ///		Four eager shapes are caught, one per alternative plus the wrapped form of each: a
    ///		static field initialiser (including one split across lines per dotnet-format.md §3),
    ///		a <c>BindableProperty.Create</c> positional default, a static auto-property
    ///		initialiser (<c>static Color X { get; } = …</c>, which the compiler emits as a
    ///		backing-field initialiser and which therefore runs at type-init time), and a static
    ///		constructor body. Two lazy shapes are deliberately left alone: an expression-bodied
    ///		member, and a lambda such as <c>defaultValueCreator</c> that sits inside a static
    ///		field initialiser but does not run with it.
    /// </summary>
    [Fact]
    public void NoEagerDmThemeRead()
    {
        // Line-spanning comes from [^;] and the (?!=>) guard, NOT from RegexOptions.Singleline:
        // there is no unescaped '.' outside a character class here, so that option would be inert
        // and is not set. The match runs from the declaration to the first ';' or the first '=>',
        // whichever comes first — the '=>' bound is what keeps the lazy shapes out.
        var eagerRead = new Regex(
            @"(static\s+(readonly\s+)?[\w<>?\[\].]+\s+\w+\s*=(?!>)"
            + @"|static\s+[\w<>?\[\].]+\s+\w+\s*\{[^}]*\}\s*=(?!>)"
            + @"|static\s+\w+\s*\(\s*\)\s*\{)"
            + @"((?!=>)[^;])*\bDmTheme\.",
            RegexOptions.Compiled);

        var sourceFiles = RepoPaths.AppSourceFiles("*.cs");

        sourceFiles
            .Should()
            .NotBeEmpty("the C# scan must find application sources or this fact is vacuous");

        var offenders = new List<string>();

        foreach (var path in sourceFiles)
        {
            var text = File.ReadAllText(path);

            var relative = Path.GetRelativePath(
                RepoPaths.AppRoot,
                path);

            foreach (Match match in eagerRead.Matches(text))
            {
                var line = ThemeSource.LineNumber(
                    text,
                    match.Index);

                offenders.Add($"{relative}:{line} -> {Condense(match.Value)}");
            }
        }

        offenders
            .Should()
            .BeEmpty(
                "an eager DmTheme read runs before Application.Current.Resources exists; convert "
                + "it to an expression-bodied member, a property with a get body, or a "
                + "BindableProperty defaultValueCreator lambda, all of which defer the read to "
                + "first use");
    }

    private static IReadOnlyList<ResolveCall> ResolveCalls()
    {
        var source = File.ReadAllText(
            Path.Combine(
                RepoPaths.AppRoot,
                "Theme",
                "DmTheme.cs"));

        var calls = ResolveCallPattern
            .Matches(source)
            .Select(match =>
                new ResolveCall(
                    match.Groups["key"].Value,
                    match.Groups["type"].Value))
            .ToList();

        calls
            .Should()
            .NotBeEmpty("DmTheme must resolve at least one key or the regex is broken");

        return calls;
    }

    private static Dictionary<string, KeyDeclaration> KeyedElementsInThemeDictionaries()
    {
        var declarations = new Dictionary<string, KeyDeclaration>(StringComparer.Ordinal);

        foreach (var path in ThemeSource.ThemeFiles())
        {
            var text = ThemeSource.XmlCommentPattern.Replace(
                File.ReadAllText(path),
                string.Empty);

            foreach (Match match in KeyedElementPattern.Matches(text))
            {
                declarations.TryAdd(
                    match.Groups["key"].Value,
                    new KeyDeclaration(
                        match.Groups["element"].Value,
                        Path.GetFileName(path)));
            }
        }

        declarations
            .Should()
            .NotBeEmpty("the theme dictionary scan must find keyed elements or the regex is broken");

        return declarations;
    }

    private static string Condense(string value)
    {
        return Regex.Replace(
            value,
            @"\s+",
            " ");
    }

    private sealed record ResolveCall(
        string Key,
        string Type);

    private sealed record KeyDeclaration(
        string Element,
        string FileName);
}
