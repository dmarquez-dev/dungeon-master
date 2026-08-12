using DungeonMaster.App.Behaviors;
using DungeonMaster.App.Theme;

namespace DungeonMaster.App.Views.DungeonDetail;

/// <summary>
///		A single tool chip of the designer's map toolbar: an icon glyph, its keyboard-shortcut
///		letter, and an outline that lights up while its tool is active.
/// </summary>
/// <remarks>
///		<para>
///			This replaces six near-identical 33-line blocks of markup in
///			<c>DungeonDetailPage.xaml</c>. A <c>Style</c> could not have factored them out because
///			what repeated was element structure — a Border wrapping a stack of two Labels — not a
///			set of property values.
///		</para>
///		<para>
///			The page's code-behind used to reach into all three parts of every chip by name
///			(<c>ToolDrawBorder.Stroke</c>, <c>ToolDrawIcon.TextColor</c>,
///			<c>ToolDrawLabel.TextColor</c>) — eighteen assignments for six chips. Setting
///			<see cref="IsActive"/> now does the same work behind one property, which is why the
///			inner Labels no longer need <c>x:Name</c>s the page can see.
///		</para>
/// </remarks>
public partial class ToolTileView : ContentView
{
	/// <summary>
	///		Identifies the <see cref="Glyph"/> bindable property.
	/// </summary>
	public static readonly BindableProperty GlyphProperty =
		BindableProperty.Create(
			nameof(Glyph),
			typeof(string),
			typeof(ToolTileView),
			string.Empty,
			propertyChanged: (bindable, _, newValue) =>
				((ToolTileView)bindable).IconLabel.Text = (string?)newValue ?? string.Empty);

	/// <summary>
	///		Identifies the <see cref="Shortcut"/> bindable property.
	/// </summary>
	public static readonly BindableProperty ShortcutProperty =
		BindableProperty.Create(
			nameof(Shortcut),
			typeof(string),
			typeof(ToolTileView),
			string.Empty,
			propertyChanged: (bindable, _, newValue) =>
				((ToolTileView)bindable).ShortcutLabel.Text = (string?)newValue ?? string.Empty);

	/// <summary>
	///		Identifies the <see cref="IsActive"/> bindable property.
	/// </summary>
	public static readonly BindableProperty IsActiveProperty =
		BindableProperty.Create(
			nameof(IsActive),
			typeof(bool),
			typeof(ToolTileView),
			false,
			propertyChanged: (bindable, _, _) =>
				((ToolTileView)bindable).ApplyActiveState());

	/// <summary>
	///		Raised when the tile is tapped.
	/// </summary>
	public event EventHandler? Clicked;

	/// <summary>
	///		Gets or sets the icon-font codepoint shown on the tile.
	/// </summary>
	public string Glyph
	{
		get => (string)GetValue(GlyphProperty);
		set => SetValue(GlyphProperty, value);
	}

	/// <summary>
	///		Gets or sets the keyboard-shortcut letter shown beside the icon.
	/// </summary>
	public string Shortcut
	{
		get => (string)GetValue(ShortcutProperty);
		set => SetValue(ShortcutProperty, value);
	}

	/// <summary>
	///		Gets or sets a value indicating whether this tile's tool is the active one, which
	///		outlines the tile and lifts both of its Labels from muted gold to full gold.
	/// </summary>
	public bool IsActive
	{
		get => (bool)GetValue(IsActiveProperty);
		set => SetValue(IsActiveProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="ToolTileView"/> and wires its tap and cursor
	///		behaviour.
	/// </summary>
	public ToolTileView()
	{
		InitializeComponent();

		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) =>
			Clicked?.Invoke(
				this,
				EventArgs.Empty);
		Chrome.GestureRecognizers.Add(tap);
		Chrome.Behaviors.Add(new HandCursorBehavior());

		ApplyActiveState();
	}

	// DmTheme is read here rather than in a field initializer: see DmTheme's remarks and the
	// guard test NoEagerDmThemeRead.
	private void ApplyActiveState()
	{
		var color = IsActive
			? DmTheme.Gold
			: DmTheme.GoldMuted;

		Chrome.Stroke = new SolidColorBrush(
			IsActive
				? DmTheme.Gold
				: Colors.Transparent);
		IconLabel.TextColor = color;
		ShortcutLabel.TextColor = color;
	}
}
