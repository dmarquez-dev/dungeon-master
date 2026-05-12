namespace DungeonMaster.App.Controls;

/// <summary>
///		A themed flag-style banner that displays a title with optional inline editing support.
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
			string.Empty,
			propertyChanged: (bindable, _, newValue) =>
			{
				var banner = (DmFlagBanner)bindable;
				banner.TitleLabel.Text = (string?)newValue ?? string.Empty;
				banner.TitleEditor.Text = (string?)newValue ?? string.Empty;
			});

	/// <summary>
	///		Identifies the <see cref="IsEditable"/> bindable property.
	/// </summary>
	public static readonly BindableProperty IsEditableProperty =
		BindableProperty.Create(
			nameof(IsEditable),
			typeof(bool),
			typeof(DmFlagBanner),
			false,
			propertyChanged: (bindable, _, newValue) =>
				((DmFlagBanner)bindable).ApplyEditability((bool)newValue));

	/// <summary>
	///		Raised when the user commits an edited title value.
	/// </summary>
	public event EventHandler<string?>? TextCommitted;

	/// <summary>
	///		Gets or sets the text displayed in the banner heading.
	/// </summary>
	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	/// <summary>
	///		Gets or sets a value indicating whether the banner title can be edited inline.
	/// </summary>
	public bool IsEditable
	{
		get => (bool)GetValue(IsEditableProperty);
		set => SetValue(IsEditableProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="DmFlagBanner"/> and loads the XAML component tree.
	/// </summary>
	public DmFlagBanner()
	{
		InitializeComponent();
	}

	/// <summary>
	///		Commits any in-progress title edit without requiring user interaction.
	/// </summary>
	public void TryCommit() =>
		TitleEditor.TryCommit();

	private void ApplyEditability(bool editable)
	{
		TitleLabel.IsVisible = !editable;
		TitleEditor.IsVisible = editable;
	}

	private void OnTitleCommitted(object? sender, string? value)
	{
		// Keep the static label in sync so switching back is seamless
		TitleLabel.Text = value ?? string.Empty;
		TextCommitted?.Invoke(this, value);
	}
}
