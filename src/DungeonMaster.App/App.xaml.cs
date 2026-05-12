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
