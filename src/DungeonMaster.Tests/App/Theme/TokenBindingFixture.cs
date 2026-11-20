using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		One token declaration found in the theme dictionaries, as the scan saw it.
/// </summary>
/// <param name="Where">
///		The declaring dictionary's path relative to the application project root.
/// </param>
/// <param name="Key">
///		The <c>x:Key</c> value verbatim.
/// </param>
/// <param name="QualifiedElementName">
///		The declaring element with its XAML prefix restored, for example <c>x:Int32</c>.
/// </param>
public sealed record TokenDeclaration(
	string Where,
	string Key,
	string QualifiedElementName);

/// <summary>
///		One <c>{StaticResource Dm…}</c> reference found in the application's XAML, attributed to
///		the element and property it targets. The CLR type of that property is NOT part of this
///		record: attribution is a scan result, typing is a table lookup, and keeping them apart is
///		what lets the scan be computed once and the tables be applied per fact.
/// </summary>
/// <param name="Where">
///		The file and line, as "<c>relative/path.xaml:12</c>".
/// </param>
/// <param name="Element">
///		The element the binding lands on, qualified as <c>ThemeSource.QualifiedElementName</c>
///		spells it.
/// </param>
/// <param name="Property">
///		The property name as written, including an attached property's dot.
/// </param>
/// <param name="Key">
///		The <c>Dm*</c> resource key referenced.
/// </param>
public sealed record TokenBinding(
	string Where,
	string Element,
	string Property,
	string Key);

/// <summary>
///		The whole-tree scan the facts in <see cref="TokenTypeMatchesTargetPropertyTests"/> stand
///		on, computed ONCE per test class instead of once per fact.
/// </summary>
/// <remarks>
///		<para>
///		MASTER-068 / TEST-017. Four facts in that class each re-read and re-parsed every
///		<c>.xaml</c> file under the application project and re-scanned every <c>.cs</c> file, so
///		the same disk walk ran four times per run for one identical answer. As an
///		<c>IClassFixture</c> this runs once; the XAML parse itself additionally goes through
///		<c>ThemeSource.AppDocuments()</c>, whose cache is per process, so a second class asking
///		the same question does not re-read the tree either.
///		</para>
///		<para>
///		NOTHING HERE ASSERTS. Every scan may come back empty and say so by being empty;
///		non-emptiness is a named <c>[Fact]</c> in the consuming class, per
///		<c>contracts/anti-vacuity.md</c> section 1.
///		</para>
/// </remarks>
public sealed class TokenBindingFixture
{
	/// <summary>
	///		The XAML language namespace, needed to read <c>x:Name</c>. Element names are qualified
	///		through <c>ThemeSource.QualifiedElementName</c> rather than here.
	/// </summary>
	private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2009/xaml";

	/// <summary>
	///		The plain substring a <c>BindableProperty</c> declaration always contains, whatever
	///		shape it is written in. Counting it is what keeps
	///		<see cref="AppBindablePropertyPattern"/> from failing open.
	/// </summary>
	private const string BindablePropertyCreation = "BindableProperty.Create";

	/// <summary>
	///		Matches a <c>BindableProperty.Create</c> declaration in the application's own
	///		controls, capturing the property name, its type and its owning type. The app's
	///		controls carry properties MAUI has never heard of — <c>DmDivider.IconFontSize</c> —
	///		and reading their real declarations beats a hand-copied table that can go stale.
	///		<para>
	///		This is a SOURCE-TEXT scan and nothing more. The behavioural claim about the
	///		application's bindable properties — that each one really reports a name, a return type
	///		and an owner — is established by reflection over the built assembly in
	///		<c>DungeonMaster.App.UnitTests.Theme.MauiPropertyTypeTests</c>, because this assembly
	///		may not reference <c>DungeonMaster.App</c> and so can never see a
	///		<c>BindableProperty</c> object. MASTER-109.
	///		</para>
	/// </summary>
	internal static readonly Regex AppBindablePropertyPattern =
		new(
			@"BindableProperty\.Create(?:AttachedProperty)?\(\s*"
			+ @"nameof\((?<property>\w+)\)\s*,\s*"
			+ @"typeof\((?<type>[\w.?<>]+)\)\s*,\s*"
			+ @"typeof\((?<owner>\w+)\)",
			RegexOptions.Compiled);

	/// <summary>
	///		Matches a <c>{StaticResource Dm…}</c> markup extension.
	/// </summary>
	internal static readonly Regex StaticResourcePattern =
		new(
			@"\{StaticResource\s+(?<key>Dm[A-Za-z0-9]*)\}",
			RegexOptions.Compiled);

	/// <summary>
	///		Initializes a new instance of <see cref="TokenBindingFixture"/>, running the scan.
	/// </summary>
	public TokenBindingFixture()
	{
		XamlFiles = RepoPaths.AppSourceFiles("*.xaml");

		StaticResourceOccurrences = XamlFiles
			.Sum(path =>
				StaticResourcePattern
					.Matches(ThemeSource.BlankXmlComments(File.ReadAllText(path)))
					.Count);

		TokenDeclarations = ScanTokenDeclarations();

		var sources = RepoPaths
			.AppSourceFiles("*.cs")
			.Select(path =>
				ThemeSource.BlankCSharpComments(File.ReadAllText(path)))
			.ToList();

		AppBindablePropertyDeclarations = sources
			.Sum(text =>
				AppBindablePropertyPattern
					.Matches(text)
					.Count);

		AppBindablePropertyOccurrences = sources
			.Sum(text =>
				Occurrences(
					text,
					BindablePropertyCreation));

		AppPropertyTypes = ScanAppPropertyTypes(sources);
		Bindings = ScanBindings();
	}

	/// <summary>
	///		Every <c>.xaml</c> file under the application project root. May be empty.
	/// </summary>
	public IReadOnlyList<string> XamlFiles { get; }

	/// <summary>
	///		The number of <c>{StaticResource Dm…}</c> occurrences in that XAML, comments blanked.
	/// </summary>
	public int StaticResourceOccurrences { get; }

	/// <summary>
	///		Every <c>Dm*</c> token declared in the theme dictionaries, as the scan saw it.
	/// </summary>
	public IReadOnlyList<TokenDeclaration> TokenDeclarations { get; }

	/// <summary>
	///		The declared type of every <c>BindableProperty</c> the application source declares,
	///		keyed "Owner.Property", as read from source text.
	/// </summary>
	public IReadOnlyDictionary<string, string> AppPropertyTypes { get; }

	/// <summary>
	///		How many declarations <see cref="AppBindablePropertyPattern"/> matched.
	/// </summary>
	public int AppBindablePropertyDeclarations { get; }

	/// <summary>
	///		How many plain <c>BindableProperty.Create</c> substrings the same files contain, with
	///		comments blanked. A structured pattern that stops matching while the substring is
	///		still there has failed open, and comparing these two numbers is what says so.
	/// </summary>
	public int AppBindablePropertyOccurrences { get; }

	/// <summary>
	///		Every <c>Dm*</c> token binding in the application's XAML, attributed to the element and
	///		property it targets. May be empty.
	/// </summary>
	public IReadOnlyList<TokenBinding> Bindings { get; }

	/// <summary>
	///		Maps a C# type name as written in source onto the type vocabulary the tables in
	///		<see cref="TokenTypeMatchesTargetPropertyTests"/> are written in.
	/// </summary>
	/// <param name="type">
	///		The type name as it appears in the source text.
	/// </param>
	internal static string Normalise(string type)
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

	private static int Occurrences(
		string text,
		string value)
	{
		var count = 0;
		var index = text.IndexOf(
			value,
			StringComparison.Ordinal);

		while (index >= 0)
		{
			count++;

			index = text.IndexOf(
				value,
				index + value.Length,
				StringComparison.Ordinal);
		}

		return count;
	}

	private static IReadOnlyList<TokenDeclaration> ScanTokenDeclarations()
	{
		return ThemeSource
			.KeyedElements()
			.Where(keyed =>
				keyed.Key.StartsWith(
					"Dm",
					StringComparison.Ordinal))
			.Select(keyed =>
				new TokenDeclaration(
					keyed.RelativePath,
					keyed.Key,
					keyed.QualifiedElementName))
			.ToList();
	}

	private static IReadOnlyDictionary<string, string> ScanAppPropertyTypes(IReadOnlyList<string> sources)
	{
		var types = new Dictionary<string, string>(StringComparer.Ordinal);

		foreach (var text in sources)
		{
			foreach (Match match in AppBindablePropertyPattern.Matches(text))
			{
				types.TryAdd(
					$"{match.Groups["owner"].Value}.{match.Groups["property"].Value}",
					Normalise(match.Groups["type"].Value));
			}
		}

		return types;
	}

	/// <summary>
	///		Returns the name of the element a <c>Setter</c> applies to.
	///		<para>
	///		Three shapes reach here. A setter inside a keyed <c>Style</c> applies to that style's
	///		<c>TargetType</c>. A setter inside a <c>VisualStateManager.VisualStateGroups</c>
	///		attached directly to an element — DmCard.xaml does this — applies to that element, so
	///		the walk skips the property-element and state scaffolding to find it.
	///		</para>
	///		<para>
	///		The third is MAUI's documented cross-element form: a setter inside a group on element A
	///		carrying <c>TargetName="B"</c> and a type-qualified <c>Property</c> such as
	///		<c>Border.Stroke</c>, meaning "set <c>Stroke</c> on the element named B". THE ANCESTOR
	///		WALK IS WRONG FOR IT — it would attribute the setter to A and compose the fictitious
	///		<c>Entry.Border.Stroke</c>, which no table can name, failing
	///		<c>EveryDmTokenBindingTargetsAPropertyOfAKnownType</c> over correct XAML. So
	///		<c>TargetName</c> is consulted first and resolved against the <c>x:Name</c>s in the
	///		same file, which is the scope MAUI resolves it in. A <c>TargetName</c> naming no element
	///		in the file resolves to the unknown marker rather than to whatever happens to be up the
	///		tree; the binding is then checked against the element-independent table alone, exactly
	///		as a <c>Style</c> with no <c>TargetType</c> is. MEASURED, not assumed: a setter with an
	///		unresolvable <c>TargetName</c> and <c>Property="Border.Stroke"</c> passes both facts,
	///		because <c>Stroke</c> is element-independent. This guard checks token types, not name
	///		resolution — a dangling <c>TargetName</c> is the XAML compiler's to catch.
	///		</para>
	/// </summary>
	private static string SetterTarget(XElement setter)
	{
		var targetName = setter
			.Attribute("TargetName")
			?.Value;

		if (targetName is not null)
		{
			return NamedElement(
				setter,
				targetName);
		}

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
					?? TokenTypeMatchesTargetPropertyTests.UnknownType;
			}

			// Scaffolding, not a target: property-element syntax carries a dot, and the visual
			// state containers are not elements a setter can apply to.
			if (name.Contains('.', StringComparison.Ordinal)
				|| name is "VisualStateGroupList" or "VisualStateGroup" or "VisualState" or "Setter")
			{
				continue;
			}

			return ThemeSource.QualifiedElementName(ancestor.Name);
		}

		return TokenTypeMatchesTargetPropertyTests.UnknownType;
	}

	/// <summary>
	///		Returns the element type of the element carrying <c>x:Name="…"</c> in the same file, or
	///		the unknown marker if no element does.
	/// </summary>
	private static string NamedElement(
		XElement setter,
		string name)
	{
		var target = setter
			.Document
			?.Descendants()
			.FirstOrDefault(element =>
				string.Equals(
					element
						.Attribute(XName.Get(
							"Name",
							XamlNamespace))
						?.Value,
					name,
					StringComparison.Ordinal));

		return target is null
			? TokenTypeMatchesTargetPropertyTests.UnknownType
			: ThemeSource.QualifiedElementName(target.Name);
	}

	private static void AddSetter(
		List<TokenBinding> bindings,
		string relative,
		XElement setter)
	{
		var value = setter.Attribute("Value")?.Value;
		var property = setter.Attribute("Property")?.Value;

		if (value is null || property is null)
		{
			return;
		}

		// The cross-element form qualifies Property with a type. That qualifier names where the
		// property is DECLARED, not the element it is set on — the element is TargetName — so it
		// is stripped here and only here. Elsewhere a dotted Property is an attached property
		// (Shell.BackgroundColor) whose dot is part of the name.
		if (setter.Attribute("TargetName") is not null)
		{
			property = property
				.Split('.')
				.Last();
		}

		foreach (Match match in StaticResourcePattern.Matches(value))
		{
			bindings.Add(
				new TokenBinding(
					Where(
						relative,
						setter),
					SetterTarget(setter),
					property,
					match.Groups["key"].Value));
		}
	}

	private static string Where(
		string relative,
		XElement element)
	{
		return $"{relative}:{((System.Xml.IXmlLineInfo)element).LineNumber}";
	}

	private static IReadOnlyList<TokenBinding> ScanBindings()
	{
		var bindings = new List<TokenBinding>();

		foreach (var document in ThemeSource.AppDocuments())
		{
			foreach (var element in document.Document.Descendants())
			{
				// A <Setter> names its target in Property= rather than carrying it as an
				// attribute, and the element it applies to is somewhere up the tree.
				if (element.Name.LocalName == "Setter")
				{
					AddSetter(
						bindings,
						document.RelativePath,
						element);

					continue;
				}

				foreach (var attribute in element.Attributes())
				{
					// Every match, not the first: an attribute can carry more than one
					// reference, and counting one of them would let the accounting fact report
					// green over a reference nothing had type-checked.
					foreach (Match match in StaticResourcePattern.Matches(attribute.Value))
					{
						bindings.Add(
							new TokenBinding(
								Where(
									document.RelativePath,
									element),
								ThemeSource.QualifiedElementName(element.Name),
								attribute.Name.LocalName,
								match.Groups["key"].Value));
					}
				}
			}
		}

		return bindings;
	}
}
