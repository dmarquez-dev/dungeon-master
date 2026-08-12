namespace DungeonMaster.App.Controls;

/// <summary>
///		Decorative horizontal rule with a centred RPG Awesome crossed-swords icon and fading gold gradient lines.
/// </summary>
public partial class DmDivider : ContentView
{
	/// <summary>
	///		Identifies the <see cref="IconFontSize"/> bindable property.
	///		The default mirrors <c>DmIconSizeMedium</c> and stays a literal deliberately: a bindable
	///		default is evaluated in this static field initialiser, which can run before
	///		<c>Application.Current.Resources</c> exists, so it cannot read <c>DmTheme</c>.
	///		<c>BindablePropertyDefaultsMatchTokensTests</c> fails if the two drift apart.
	/// </summary>
	public static readonly BindableProperty IconFontSizeProperty =
		BindableProperty.Create(
			nameof(IconFontSize),
			typeof(double),
			typeof(DmDivider),
			16.0);

	/// <summary>
	///		Gets or sets the font size of the centred crossed-swords icon. Defaults to 16; callers in
	///		tighter spaces (e.g. the 300px sidebar) can shrink it so the rule doesn't read too heavy.
	/// </summary>
	public double IconFontSize
	{
		get => (double)GetValue(IconFontSizeProperty);
		set => SetValue(IconFontSizeProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="DmDivider"/> and loads the XAML component tree.
	/// </summary>
	public DmDivider()
	{
		InitializeComponent();
	}
}
