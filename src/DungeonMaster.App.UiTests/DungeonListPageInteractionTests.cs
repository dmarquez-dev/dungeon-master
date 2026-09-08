using System.Globalization;
using FluentAssertions;
using Xunit.Abstractions;

namespace DungeonMaster.App.UiTests;

/// <summary>
/// Drives the dungeon list page, which is the app's start route, through a real Appium session.
/// </summary>
/// <remarks>
/// <para>
/// Every id these tests address is already declared in the app's XAML. This project adds none and
/// edits no <c>.xaml</c> file.
/// </para>
/// <para>
/// The three facts form one experiment. <see cref="ASessionOpensAndTheDungeonListPageIsReachable"/>
/// and <see cref="TheCreateDungeonPopupIsAbsentBeforeTheButtonIsClicked"/> are the controls: they
/// establish that the apparatus reaches the app and that the popup is genuinely not there to begin
/// with. Only then does <see cref="ClickingCreateDungeonOpensTheCreateDungeonPopup"/> mean anything
/// — without the controls, a passing click test cannot be told apart from one asserting on a UI it
/// never reached.
/// </para>
/// </remarks>
public sealed class DungeonListPageInteractionTests
{
	/// <summary>The floating action button on the dungeon list page. Declared in DungeonListPage.xaml.</summary>
	private const string CreateDungeonButtonId = "CreateDungeonButton";

	/// <summary>The title label inside the shared popup chrome. Declared in DmPopup.xaml.</summary>
	private const string PopupTitleId = "PopupTitle";

	/// <summary>The title CreateDungeonPopup.xaml sets on its DmPopup chrome.</summary>
	private const string CreateDungeonPopupTitle = "New Dungeon";

	private readonly ITestOutputHelper _output;

	public DungeonListPageInteractionTests(ITestOutputHelper output) => _output = output;

	[Fact]
	public void ASessionOpensAndTheDungeonListPageIsReachable()
	{
		using var session = AppSession.Open();
		_output.WriteLine(session.Describe());

		session.SessionId.Should().NotBeNullOrWhiteSpace(
			because: "the server must issue a session id before anything else in this project means anything");

		var fab = session.WaitForElement(CreateDungeonButtonId);
		fab.Should().NotBeNull(
			because: "AccessibilityId must resolve the AutomationId DungeonListPage.xaml declares on the create button");

		_output.WriteLine(string.Create(
			CultureInfo.InvariantCulture,
			$"found {CreateDungeonButtonId}: displayed={fab.Displayed} enabled={fab.Enabled}"));
	}

	[Fact]
	public void TheCreateDungeonPopupIsAbsentBeforeTheButtonIsClicked()
	{
		using var session = AppSession.Open();
		_output.WriteLine(session.Describe());

		// Reaching the button first is what distinguishes "the popup is not open" from
		// "the session is looking at nothing".
		session.WaitForElement(CreateDungeonButtonId);

		var count = session.CountElements(PopupTitleId);
		_output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{PopupTitleId} count before click = {count}"));

		count.Should().Be(0,
			because: "the create-dungeon popup has not been opened, so its title label is not in the tree");
	}

	[Fact]
	public void ClickingCreateDungeonOpensTheCreateDungeonPopup()
	{
		using var session = AppSession.Open();
		_output.WriteLine(session.Describe());

		var fab = session.WaitForElement(CreateDungeonButtonId);
		_output.WriteLine(string.Create(
			CultureInfo.InvariantCulture,
			$"located {CreateDungeonButtonId} by AccessibilityId: displayed={fab.Displayed}"));

		var before = session.CountElements(PopupTitleId);
		_output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{PopupTitleId} count before click = {before}"));
		before.Should().Be(0, because: "the popup must not already be open, or the click proves nothing");

		fab.Click();
		_output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"clicked {CreateDungeonButtonId}"));

		var title = session.WaitForElement(PopupTitleId);
		_output.WriteLine(string.Create(
			CultureInfo.InvariantCulture,
			$"{PopupTitleId} after click: text='{title.Text}'"));

		title.Text.Should().Be(CreateDungeonPopupTitle,
			because: "clicking the create button opens CreateDungeonPopup, whose DmPopup chrome carries this title");
	}
}
