namespace DungeonMaster.App;

/// <summary>
///		Represents the root application class for DungeonMaster.
/// </summary>
public partial class App : Microsoft.Maui.Controls.Application
{
	/// <summary>
	///		Initializes a new instance of <see cref="App"/> and loads XAML resources.
	/// </summary>
	public App()
	{
		InitializeComponent();

#if WINDOWS
		// The WinUI system brushes (scrollbar, text selection, ComboBox popup) have to be
		// overridden from C# because App.xaml on Windows is parsed by WinUI and cannot see
		// MAUI's tokens. Those overrides read DmTheme, which needs Application.Current.Resources
		// — and that only exists once InitializeComponent above has run. This is therefore the
		// earliest point at which they can be applied, and it is still before the first window
		// is created. See the remarks on WinUI.App.ApplyThemeResources.
		global::DungeonMaster.App.WinUI.App.ApplyThemeResources();
#endif
	}

	/// <summary>
	///		Creates the main application window hosting the <see cref="AppShell"/>.
	/// </summary>
	/// <param name="activationState">
	///		The activation state provided by the platform at startup.
	/// </param>
	/// <returns>
	///		A new <see cref="Window"/> containing the application shell.
	/// </returns>
	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
