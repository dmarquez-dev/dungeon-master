namespace DungeonMaster.App.Controls;

/// <summary>
///		A themed flag-style banner that displays a title as static, display-only text.
/// </summary>
public partial class DmFlagBanner : ContentView
{
	/// <summary>
	///		Identifies the <see cref="Title"/> bindable property.
	/// </summary>
	public static readonly BindableProperty TitleProperty =
		BindableProperty.Create(
			nameof(Title),
			typeof(string),
			typeof(DmFlagBanner),
			string.Empty);

	/// <summary>
	///		Gets or sets the text displayed in the banner heading.
	/// </summary>
	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="DmFlagBanner"/> and loads the XAML component tree.
	/// </summary>
	public DmFlagBanner()
	{
		InitializeComponent();
	}
}
