using DungeonMaster.App.Views;

namespace DungeonMaster.App;

/// <summary>
///		Defines the Shell navigation structure for the DungeonMaster application.
/// </summary>
public partial class AppShell : Shell
{
	/// <summary>
	///		Initializes a new instance of <see cref="AppShell"/> and registers navigation routes.
	/// </summary>
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(
			"dungeondetail",
			typeof(DungeonDetailPage));
	}
}
