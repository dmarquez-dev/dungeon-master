using DungeonMaster.App.Controls;

using FluentAssertions;

namespace DungeonMaster.App.UnitTests.Fixtures;

/// <summary>
///		Establishes each guarantee <see cref="AppFixture"/> makes, so that a consumer's failure can
///		be read as a failure of the consumer rather than of the fixture underneath it.
/// </summary>
/// <remarks>
///		These are not tests of MAUI. They are the fixture's own contract: an application is current,
///		the merged theme dictionaries are reachable, a key that does not exist reads back as absent
///		rather than as something, a XAML control constructs with its content and bindable properties
///		working, and a whole XAML <c>Shell</c> constructs.
/// </remarks>
[Collection(AppTestCollection.Name)]
public class AppFixtureTests
{
	/// <summary>
	///		A key no theme dictionary declares. Deliberately not a plausible token name, so that
	///		adding a real token can never accidentally define it.
	/// </summary>
	private const string UndeclaredKey = "DmKeyThatIsDeclaredNowhere_AppFixtureNegativeControl";

	private readonly AppFixture _fixture;

	/// <summary>
	///		Initializes a new instance of <see cref="AppFixtureTests"/>.
	/// </summary>
	/// <param name="fixture">The collection fixture.</param>
	public AppFixtureTests(AppFixture fixture)
	{
		_fixture = fixture;
	}

	/// <summary>
	///		<c>Application.Current</c> is the fixture's own instance, not merely some application.
	/// </summary>
	[Fact]
	public void TheCurrentApplicationIsTheFixturesApplication()
	{
		Microsoft.Maui.Controls.Application.Current
			.Should()
			.NotBeNull("XAML resource resolution reads Application.Current, so a null one makes " +
				"every StaticResource in this assembly unresolvable")
			.And
			.BeSameAs(
				_fixture.Application,
				"a second Application constructed anywhere in the process would silently take " +
				"over Current and bring its own resources with it");
	}

	/// <summary>
	///		A token the theme dictionaries do declare is reachable through the fixture.
	/// </summary>
	/// <remarks>
	///		<c>DmStrokeHairline</c> is the specific key whose absence made <c>new DmCard()</c> throw
	///		<c>XamlParseException</c> before the fixture constructed an application.
	/// </remarks>
	[Fact]
	public void FindResourceReachesAMergedThemeToken()
	{
		_fixture.FindResource("DmStrokeHairline")
			.Should()
			.NotBeNull("the seven merged theme dictionaries are what let a XAML control's " +
				"StaticResource references resolve");
	}

	/// <summary>
	///		A key nothing declares reads back as null.
	/// </summary>
	/// <remarks>
	///		This is the negative control for <see cref="FindResourceReachesAMergedThemeToken"/>.
	///		Without it, a lookup that returned something for every key would satisfy that test
	///		exactly as a populated dictionary does, and the pair could not tell the two apart.
	/// </remarks>
	[Fact]
	public void FindResourceReturnsNullForAKeyNothingDeclares()
	{
		_fixture.FindResource(UndeclaredKey)
			.Should()
			.BeNull("absence is a fact a consumer reports; a lookup answering every key would " +
				"make the positive case above prove nothing");
	}

	/// <summary>
	///		A XAML-declared control constructs through the fixture, inflates its content, and its
	///		bindable properties work.
	/// </summary>
	[Fact]
	public void ConstructInflatesAXamlControlAndItsBindableProperties()
	{
		var card = _fixture.Construct(() => new DmCard());

		card.Content
			.Should()
			.NotBeNull("a non-null Content is the evidence that the generated InitializeComponent " +
				"ran and populated the visual tree from DmCard.xaml");

		card.Title = "Spike";

		card.GetValue(DmCard.TitleProperty)
			.Should()
			.Be("Spike", "the bindable property has to round-trip through the MAUI property store, " +
				"not merely through a CLR field");
	}

	/// <summary>
	///		A whole XAML <c>Shell</c> constructs.
	/// </summary>
	/// <remarks>
	///		<para>
	///		<c>AppShell</c> is the app's whole navigation graph and registers a route in its
	///		constructor. A <c>Shell</c> is also a far heavier inflation than a single control.
	///		</para>
	///		<para>
	///		Its <c>Items</c> are asserted rather than only its non-nullness. <c>AppShell.xaml</c>
	///		declares no <c>StaticResource</c> and attaches no dispatcher-dependent behaviour, so a
	///		bare non-null check here would survive a fixture that supplied neither prerequisite —
	///		measured: this fact stayed green under both the empty-resources injection and the
	///		no-dispatcher injection. A populated <c>Items</c> is evidence the generated
	///		<c>InitializeComponent</c> actually ran, which is the claim the fact is making.
	///		</para>
	/// </remarks>
	[Fact]
	public void ConstructInflatesTheApplicationShell()
	{
		var shell = _fixture.Construct(() => new DungeonMaster.App.AppShell());

		shell
			.Should()
			.NotBeNull("the shell is the app's whole navigation graph; if it cannot be inflated " +
				"here, nothing built on it can be tested here either");

		shell.Items
			.Should()
			.NotBeEmpty("AppShell.xaml declares a ShellContent, so an empty Items means the " +
				"generated InitializeComponent did not inflate the markup");
	}
}
