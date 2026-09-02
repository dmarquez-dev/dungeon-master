using DungeonMaster.App.Controls;
using DungeonMaster.App.Views;

using Xunit.Abstractions;

namespace DungeonMaster.App.Tests.Hosting;

/// <summary>
///		Proves the application host is genuinely up, by behaviour rather than by inspection. Every
///		assertion here is something that fails without a started WinUI application: anything
///		deriving from <c>Microsoft.Maui.Controls.Element</c> cannot be constructed without one, and
///		a XAML <c>StaticResource</c> lookup needs the application's merged resource dictionaries
///		(spike-host-report.md, sections 2b, 2c and 3).
/// </summary>
[Collection(MauiHostCollection.Name)]
public sealed class MauiHostFixtureTests(MauiHostFixture host, ITestOutputHelper output)
{
	/// <summary>
	///		<see cref="DmCard"/> is a <c>ContentView</c> whose XAML resolves <c>StaticResource</c>
	///		keys from the application's resources, so constructing one exercises both the
	///		<c>Element</c> type initializer and the loaded resource dictionaries. Without the host
	///		this throws <c>TypeInitializationException</c>; with the host but without the
	///		application's resources it throws <c>XamlParseException: StaticResource not found for
	///		key DmStrokeHairline</c>.
	/// </summary>
	[Fact]
	public void ConstructingADmCardUnderTheHostReturnsANonNullContentView()
	{
		DmCard? card = null;

		host.Invoke(() => card = new DmCard());

		Assert.NotNull(card);
	}

	/// <summary>
	///		A bare <c>ContentPage</c> is the simplest <c>Element</c> subclass in the tree the pages
	///		are built from; it constructs only under a started application.
	/// </summary>
	[Fact]
	public void ConstructingAContentPageUnderTheHostReturnsANonNullPage()
	{
		ContentPage? page = null;

		host.Invoke(() => page = new ContentPage());

		Assert.NotNull(page);
	}

	/// <summary>
	///		The host is the real application entry point, so its DI container is the real one and
	///		<see cref="DungeonDetailPage"/> - a registered transient - resolves from it. The
	///		resolution happens inside <c>Invoke</c> because the resolved page is an <c>Element</c>.
	/// </summary>
	[Fact]
	public void ResolvingDungeonDetailPageFromTheHostContainerReturnsANonNullPage()
	{
		DungeonDetailPage? page = null;

		host.Invoke(() => page = host.Resolve<DungeonDetailPage>());

		Assert.NotNull(page);
	}

	/// <summary>
	///		The host survives startup only because unhandled exceptions on the WinUI dispatcher are
	///		marked handled. This test is the guard against that becoming a blanket mask: every one
	///		is recorded, carries a type and a message, and is written to the test output so a later
	///		startup regression is diagnosable instead of silent.
	/// </summary>
	[Fact]
	public void EveryExceptionTheHostMarksHandledIsRecordedWithATypeAndAMessage()
	{
		var handled = host.HandledExceptions;

		foreach (var exception in handled)
		{
			output.WriteLine(
				FormattableString.Invariant(
					$"Handled on the WinUI dispatcher: {exception.GetType().FullName}: {exception.Message}"));

			Assert.False(string.IsNullOrWhiteSpace(exception.GetType().FullName));
			Assert.False(string.IsNullOrWhiteSpace(exception.Message));
		}

		output.WriteLine(
			FormattableString.Invariant($"Handled exception count: {handled.Count}"));
	}
}
