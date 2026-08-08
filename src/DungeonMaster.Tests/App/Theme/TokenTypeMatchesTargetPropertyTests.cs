using System.Text.RegularExpressions;
using System.Xml.Linq;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Guards the one thing every other theme fact takes on trust: that a <c>Dm*</c> token can
///		actually convert to the type of the property it is bound to.
///		<para>
///		THE FAILURE THIS EXISTS FOR IS SILENT, AND IT WAS LIVE. The radius tokens were declared
///		<c>x:Double</c> while <c>Button.CornerRadius</c> is <c>System.Int32</c>.
///		<c>BindableProperty.TryConvert</c> will not narrow a double to an int, so every
///		<c>Button.CornerRadius="{StaticResource DmRadius…}"</c> in the app — the implicit style,
///		the <c>DmButtonBase</c> setter and eleven inline sites — was REJECTED on every
///		evaluation. The build was clean, the XAML was valid, every existing theme fact was green,
///		and no button in the app had a themed corner radius. The only outward signs were a
///		console warning per evaluation and the fact that the property sat on its <c>-1</c>
///		default, which WinUI renders close enough to 3-4px to survive a glance.
///		</para>
///		<para>
///		A key existing, resolving, and being spelled correctly is what the sibling facts check.
///		None of them reads the TYPE of the property on the far side of the binding, which is
///		what decides whether the value lands at all.
///		</para>
/// </summary>
public class TokenTypeMatchesTargetPropertyTests
{
    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2009/xaml";

    /// <summary>
    ///		Marks a property whose type the fact could not determine. Carried as a type name so a
    ///		binding onto it flows through the same pipeline as any other and is reported by
    ///		<see cref="EveryDmTokenBindingTargetsAPropertyOfAKnownType"/> rather than dropped.
    /// </summary>
    private const string UnknownType = "?";

    /// <summary>
    ///		The CLR type each token-declaring XAML element produces.
    ///		<para>
    ///		A token declared as an element this fact does not know is a failure, not a skip: the
    ///		whole point is that retyping a token is invisible everywhere else.
    ///		</para>
    /// </summary>
    private static readonly Dictionary<string, string> TokenElementTypes =
        new(StringComparer.Ordinal)
        {
            ["x:Double"] = "double",
            ["x:Int32"] = "int",
            ["x:String"] = "string",
            ["Thickness"] = "Thickness",
            ["Color"] = "Color",
            ["SolidColorBrush"] = "Brush",
            ["Style"] = "Style",
        };

    /// <summary>
    ///		Properties whose declared type depends on the element that owns them. These are the
    ///		live traps, and the reason a property-name-only table would be a lie.
    ///		<para>
    ///		Read off Microsoft.Maui.Controls.dll 10.0.41 by reflection on 2026-08-07. MAUI's six
    ///		corner-radius-bearing properties span FOUR different types, and the two <c>int</c>
    ///		ones are the only members of that set that reject a double.
    ///		</para>
    ///		<para>
    ///		<c>ImageButton</c> and <c>Frame</c> are listed although the app uses neither. They are
    ///		the rest of the trap class — <c>Frame.CornerRadius</c> is <c>System.Single</c> and
    ///		rejects a double just as <c>Button</c>'s does — and seeding them costs nothing,
    ///		whereas discovering them the way <c>Button</c> was discovered costs a release.
    ///		</para>
    /// </summary>
    private static readonly Dictionary<string, string> PropertyTypesByElement =
        new(StringComparer.Ordinal)
        {
            ["Button.CornerRadius"] = "int",
            ["ImageButton.CornerRadius"] = "int",
            ["Frame.CornerRadius"] = "float",
            ["BoxView.CornerRadius"] = "CornerRadius",
            ["RoundRectangle.CornerRadius"] = "CornerRadius",
            ["Border.StrokeShape"] = "IShape",
        };

    /// <summary>
    ///		Properties whose declared type is the same whatever element carries them.
    ///		<para>
    ///		<c>CornerRadius</c> is deliberately ABSENT. It is the property this whole fact exists
    ///		for, and it has no element-independent type; leaving it out means a
    ///		<c>CornerRadius</c> binding onto an element nobody has taught this table about fails
    ///		here rather than being guessed at.
    ///		</para>
    /// </summary>
    private static readonly Dictionary<string, string> PropertyTypesByName =
        new(StringComparer.Ordinal)
        {
            ["Padding"] = "Thickness",
            ["Margin"] = "Thickness",

            ["FontFamily"] = "string",

            ["FontSize"] = "double",
            ["CharacterSpacing"] = "double",
            ["LineHeight"] = "double",
            ["StrokeThickness"] = "double",
            ["BorderWidth"] = "double",
            ["Spacing"] = "double",
            ["RowSpacing"] = "double",
            ["ColumnSpacing"] = "double",
            ["ItemSpacing"] = "double",
            ["HeightRequest"] = "double",
            ["WidthRequest"] = "double",
            ["MinimumHeightRequest"] = "double",
            ["MinimumWidthRequest"] = "double",

            ["MaxLines"] = "int",

            ["Style"] = "Style",
            ["BasedOn"] = "Style",

            ["Stroke"] = "Brush",
            ["Fill"] = "Brush",

            ["Color"] = "Color",
            ["TextColor"] = "Color",
            ["BackgroundColor"] = "Color",
            ["BorderColor"] = "Color",
            ["PlaceholderColor"] = "Color",

            ["Shell.BackgroundColor"] = "Color",
            ["Shell.ForegroundColor"] = "Color",
            ["Shell.TitleColor"] = "Color",
            ["Shell.DisabledColor"] = "Color",
            ["Shell.UnselectedColor"] = "Color",
            ["Shell.TabBarBackgroundColor"] = "Color",
            ["Shell.TabBarForegroundColor"] = "Color",
            ["Shell.TabBarTitleColor"] = "Color",
            ["Shell.TabBarUnselectedColor"] = "Color",
        };

    /// <summary>
    ///		The conversions <c>BindableProperty.TryConvert</c> performs, other than the identity.
    ///		<para>
    ///		EVERY ENTRY AND EVERY ABSENCE WAS MEASURED against Microsoft.Maui.Controls 10.0.41 on
    ///		2026-08-07 by calling <c>SetValue</c> with a boxed value of the source type and
    ///		reading the property back — because the failure mode is a value that is dropped, a
    ///		clean build and a clean launch prove nothing here.
    ///		</para>
    ///		<para>
    ///		The results that matter, using 7 as the probe value:
    ///		</para>
    ///		<para>
    ///		ACCEPTED, reading back as 7 (or as a SolidColorBrush): int → double, int → float,
    ///		int → CornerRadius, int → Thickness, double → CornerRadius, double → Thickness,
    ///		Color → Brush.
    ///		</para>
    ///		<para>
    ///		DROPPED, leaving the property on its default: double → int, double → float,
    ///		string → int, string → double, string → CornerRadius, string → Thickness.
    ///		</para>
    ///		<para>
    ///		int widens to everything and double narrows to nothing. That asymmetry is the whole
    ///		shape of the defect this fact guards.
    ///		</para>
    ///		<para>
    ///		int reaching <c>CornerRadius</c> is a chained conversion — TryConvert widens the int
    ///		and then applies CornerRadius's implicit double operator — and it is exactly the kind
    ///		of thing that is easy to assume in either direction. It is in this table because it
    ///		was measured, not because it was reasoned.
    ///		</para>
    /// </summary>
    private static readonly HashSet<string> WideningConversions =
        new(StringComparer.Ordinal)
        {
            "int->double",
            "int->float",
            "int->CornerRadius",
            "int->Thickness",
            "double->CornerRadius",
            "double->Thickness",
            "Color->Brush",
        };

    /// <summary>
    ///		Matches a <c>BindableProperty.Create</c> declaration in the application's own
    ///		controls, capturing the property name, its type and its owning type. The app's
    ///		controls carry properties MAUI has never heard of — <c>DmDivider.IconFontSize</c> —
    ///		and reading their real declarations beats a hand-copied table that can go stale.
    /// </summary>
    private static readonly Regex AppBindablePropertyPattern =
        new(
            @"BindableProperty\.Create(?:AttachedProperty)?\(\s*"
            + @"nameof\((?<property>\w+)\)\s*,\s*"
            + @"typeof\((?<type>[\w.?<>]+)\)\s*,\s*"
            + @"typeof\((?<owner>\w+)\)",
            RegexOptions.Compiled);

    /// <summary>
    ///		Matches a <c>{StaticResource Dm…}</c> markup extension.
    /// </summary>
    private static readonly Regex StaticResourcePattern =
        new(
            @"\{StaticResource\s+(?<key>Dm[A-Za-z0-9]*)\}",
            RegexOptions.Compiled);

    /// <summary>
    ///		Asserts that every <c>Dm*</c> token bound in the application's XAML converts to the
    ///		declared type of the property it targets. This is the fact that would have caught the
    ///		radius defect on the commit that introduced it.
    /// </summary>
    [Fact]
    public void EveryDmTokenBindingConvertsToItsTargetPropertyType()
    {
        var tokenTypes = TokenTypesByKey();

        var offenders = Bindings()
            .Where(binding =>
                binding.PropertyType != UnknownType)
            .Where(binding =>
                tokenTypes.ContainsKey(binding.Key))
            .Where(binding =>
                !Converts(
                    tokenTypes[binding.Key],
                    binding.PropertyType))
            .Select(binding =>
                $"{binding.Where} -> {binding.Element}.{binding.Property} is "
                + $"{binding.PropertyType}, but '{binding.Key}' is declared "
                + $"{tokenTypes[binding.Key]}; MAUI drops the value and the property keeps its "
                + "default")
            .OrderBy(
                entry =>
                    entry,
                StringComparer.Ordinal)
            .ToList();

        offenders
            .Should()
            .BeEmpty(
                "a token whose type the target property cannot accept is discarded at every "
                + "evaluation with only a console warning to show for it");
    }

    /// <summary>
    ///		Asserts that this fact can name the type of every property a token is bound to.
    ///		<para>
    ///		Without this, the guard above degrades quietly: an unrecognised element or property
    ///		would simply not be checked, and the next <c>Button.CornerRadius</c> would slip
    ///		through a green suite exactly as the first one did. Extend
    ///		<see cref="PropertyTypesByElement"/> or <see cref="PropertyTypesByName"/> when this
    ///		fails — do not widen the scan to skip the unknown.
    ///		</para>
    /// </summary>
    [Fact]
    public void EveryDmTokenBindingTargetsAPropertyOfAKnownType()
    {
        var unknown = Bindings()
            .Where(binding =>
                binding.PropertyType == UnknownType)
            .Select(binding =>
                $"{binding.Where} -> {binding.Element}.{binding.Property} (binds '{binding.Key}')")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(
                entry =>
                    entry,
                StringComparer.Ordinal)
            .ToList();

        unknown
            .Should()
            .BeEmpty(
                "this fact cannot check a binding whose target type it cannot name; add the "
                + "element+property to PropertyTypesByElement, or the property to "
                + "PropertyTypesByName, with the type read off Microsoft.Maui.Controls.dll");
    }

    /// <summary>
    ///		Asserts that every <c>{StaticResource Dm…}</c> occurrence in the application's XAML
    ///		was attributed to an element and a property.
    ///		<para>
    ///		This is the fact that keeps the other two honest. The scan understands three binding
    ///		shapes — an attribute on an element, a <c>Setter</c> inside a keyed <c>Style</c>, and
    ///		a <c>Setter</c> inside a <c>VisualStateManager.VisualStateGroups</c> attached to an
    ///		element — and a fourth shape appearing in the tree would otherwise be silently
    ///		invisible to a guard that still reported green.
    ///		</para>
    /// </summary>
    [Fact]
    public void EveryStaticResourceReferenceInTheXamlIsSeenByTheScan()
    {
        var seen = Bindings().Count;

        var total = XamlFiles()
            .Sum(path =>
                StaticResourcePattern
                    .Matches(StripComments(File.ReadAllText(path)))
                    .Count);

        total
            .Should()
            .BeGreaterThan(0, "the XAML scan must find token references or every fact here is vacuous");

        seen
            .Should()
            .Be(
                total,
                "every {{StaticResource Dm…}} in the application's XAML must be attributed to an "
                + "element and a property; an unattributed one is a binding shape this scan does "
                + "not understand and therefore does not check");
    }

    /// <summary>
    ///		Asserts that the scan actually reaches at least one binding onto an <c>int</c>-typed
    ///		property.
    ///		<para>
    ///		This is a fact about the GUARD, not about the app. A guard that is structurally
    ///		incapable of firing passes forever, and the int-typed properties are the only ones in
    ///		MAUI's geometry surface that can reject a plausible-looking token. If every such
    ///		binding is refactored away, this fails and says so, rather than leaving a fact that
    ///		looks like cover and is not.
    ///		</para>
    /// </summary>
    [Fact]
    public void TheScanReachesTheIntTypedPropertiesItExistsToGuard()
    {
        var intTargets = Bindings()
            .Where(binding =>
                binding.PropertyType == "int")
            .Select(binding =>
                $"{binding.Element}.{binding.Property}")
            .Distinct(StringComparer.Ordinal)
            .ToList();

        intTargets
            .Should()
            .NotBeEmpty(
                "the int-typed properties are the live trap this fact exists for; if no token "
                + "binds to one any more, say so deliberately rather than leaving a guard that "
                + "cannot fire");
    }

    private static bool Converts(
        string from,
        string to)
    {
        return string.Equals(
                from,
                to,
                StringComparison.Ordinal)
            || WideningConversions.Contains($"{from}->{to}");
    }

    private static IReadOnlyList<string> XamlFiles()
    {
        var files = RepoPaths.AppSourceFiles("*.xaml");

        files
            .Should()
            .NotBeEmpty("the XAML scan must find files or every fact here is vacuous");

        return files;
    }

    private static string StripComments(string text)
    {
        return ThemeSource.XmlCommentPattern.Replace(
            text,
            string.Empty);
    }

    /// <summary>
    ///		Returns the CLR type of every <c>Dm*</c> token, keyed by resource key.
    /// </summary>
    private static Dictionary<string, string> TokenTypesByKey()
    {
        var types = new Dictionary<string, string>(StringComparer.Ordinal);
        var unrecognised = new List<string>();

        foreach (var path in ThemeSource.ThemeFiles())
        {
            var document = XDocument.Parse(File.ReadAllText(path));

            foreach (var element in document.Descendants())
            {
                var key = element
                    .Attribute(XName.Get("Key", XamlNamespace))
                    ?.Value;

                if (key is null || !key.StartsWith("Dm", StringComparison.Ordinal))
                {
                    continue;
                }

                var name = QualifiedName(element.Name);

                if (!TokenElementTypes.TryGetValue(name, out var type))
                {
                    unrecognised.Add($"{Path.GetFileName(path)} declares '{key}' as <{name}>");

                    continue;
                }

                types.TryAdd(
                    key,
                    type);
            }
        }

        unrecognised
            .Should()
            .BeEmpty(
                "a token declared as an element this fact does not know cannot be type-checked; "
                + "add the element to TokenElementTypes");

        types
            .Should()
            .NotBeEmpty("the token declaration scan must find tokens or every fact here is vacuous");

        return types;
    }

    /// <summary>
    ///		Returns the declared type of every <c>BindableProperty</c> the application defines,
    ///		keyed as "Owner.Property".
    /// </summary>
    private static Dictionary<string, string> AppPropertyTypes()
    {
        var types = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var path in RepoPaths.AppSourceFiles("*.cs"))
        {
            foreach (Match match in AppBindablePropertyPattern.Matches(File.ReadAllText(path)))
            {
                types.TryAdd(
                    $"{match.Groups["owner"].Value}.{match.Groups["property"].Value}",
                    Normalise(match.Groups["type"].Value));
            }
        }

        types
            .Should()
            .NotBeEmpty(
                "the application declares bindable properties; finding none means the pattern "
                + "has stopped matching and every app-control binding is silently unchecked");

        return types;
    }

    /// <summary>
    ///		Maps a C# type name as written in source onto this fact's type vocabulary.
    /// </summary>
    private static string Normalise(string type)
    {
        var bare = type
            .Split('.')
            .Last()
            .TrimEnd('?');

        return bare switch
        {
            "Double" => "double",
            "Int32" => "int",
            "Single" => "float",
            "String" => "string",
            _ => bare,
        };
    }

    /// <summary>
    ///		Returns the element name with its XAML prefix restored, so <c>x:Int32</c> is
    ///		distinguishable from a hypothetical <c>Int32</c> in the default namespace.
    /// </summary>
    private static string QualifiedName(XName name)
    {
        return name.NamespaceName == XamlNamespace
            ? $"x:{name.LocalName}"
            : name.LocalName;
    }

    /// <summary>
    ///		Returns every <c>Dm*</c> token binding in the application's XAML, attributed to the
    ///		element and property it targets.
    /// </summary>
    private static IReadOnlyList<Binding> Bindings()
    {
        var appTypes = AppPropertyTypes();
        var bindings = new List<Binding>();

        foreach (var path in XamlFiles())
        {
            var relative = Path.GetRelativePath(
                RepoPaths.AppRoot,
                path);

            var document = XDocument.Parse(
                File.ReadAllText(path),
                LoadOptions.SetLineInfo);

            foreach (var element in document.Descendants())
            {
                // A <Setter> names its target in Property= rather than carrying it as an
                // attribute, and the element it applies to is somewhere up the tree.
                if (element.Name.LocalName == "Setter")
                {
                    AddSetter(
                        bindings,
                        appTypes,
                        relative,
                        element);

                    continue;
                }

                foreach (var attribute in element.Attributes())
                {
                    // Every match, not the first: an attribute can carry more than one
                    // reference, and counting one of them would let the accounting fact below
                    // report green over a reference nothing had type-checked.
                    foreach (Match match in StaticResourcePattern.Matches(attribute.Value))
                    {
                        bindings.Add(
                            Describe(
                                appTypes,
                                relative,
                                element,
                                QualifiedName(element.Name),
                                attribute.Name.LocalName,
                                match.Groups["key"].Value));
                    }
                }
            }
        }

        return bindings;
    }

    private static void AddSetter(
        List<Binding> bindings,
        Dictionary<string, string> appTypes,
        string relative,
        XElement setter)
    {
        var value = setter.Attribute("Value")?.Value;
        var property = setter.Attribute("Property")?.Value;

        if (value is null || property is null)
        {
            return;
        }

        foreach (Match match in StaticResourcePattern.Matches(value))
        {
            bindings.Add(
                Describe(
                    appTypes,
                    relative,
                    setter,
                    SetterTarget(setter),
                    property,
                    match.Groups["key"].Value));
        }
    }

    /// <summary>
    ///		Returns the name of the element a <c>Setter</c> applies to.
    ///		<para>
    ///		Two shapes reach here. A setter inside a keyed <c>Style</c> applies to that style's
    ///		<c>TargetType</c>. A setter inside a <c>VisualStateManager.VisualStateGroups</c>
    ///		attached directly to an element — DmCard.xaml does this — applies to that element, so
    ///		the walk skips the property-element and state scaffolding to find it.
    ///		</para>
    /// </summary>
    private static string SetterTarget(XElement setter)
    {
        foreach (var ancestor in setter.Ancestors())
        {
            var name = ancestor.Name.LocalName;

            if (name == "Style")
            {
                return ancestor
                    .Attribute("TargetType")
                    ?.Value
                    .Split(':')
                    .Last()
                    ?? UnknownType;
            }

            // Scaffolding, not a target: property-element syntax carries a dot, and the visual
            // state containers are not elements a setter can apply to.
            if (name.Contains('.', StringComparison.Ordinal)
                || name is "VisualStateGroupList" or "VisualStateGroup" or "VisualState" or "Setter")
            {
                continue;
            }

            return QualifiedName(ancestor.Name);
        }

        return UnknownType;
    }

    private static Binding Describe(
        Dictionary<string, string> appTypes,
        string relative,
        XElement element,
        string owner,
        string property,
        string key)
    {
        var line = ((System.Xml.IXmlLineInfo)element).LineNumber;

        return new Binding(
            $"{relative}:{line}",
            owner,
            property,
            key,
            PropertyType(
                appTypes,
                owner,
                property));
    }

    private static string PropertyType(
        Dictionary<string, string> appTypes,
        string owner,
        string property)
    {
        // The application's own controls first: DmDivider.IconFontSize is not a MAUI property
        // and the tables below have no business guessing at it.
        if (appTypes.TryGetValue(
            $"{owner}.{property}",
            out var appType))
        {
            return appType;
        }

        if (PropertyTypesByElement.TryGetValue(
            $"{owner}.{property}",
            out var elementType))
        {
            return elementType;
        }

        return PropertyTypesByName.TryGetValue(
            property,
            out var namedType)
            ? namedType
            : UnknownType;
    }

    private sealed record Binding(
        string Where,
        string Element,
        string Property,
        string Key,
        string PropertyType);
}
