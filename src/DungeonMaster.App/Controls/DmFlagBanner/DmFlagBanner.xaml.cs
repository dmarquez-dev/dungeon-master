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
				// TitleLabel is NOT written here. Its Text is bound to Title in the XAML and that
				// binding is its only writer; an imperative write beside it wins under MAUI value
				// precedence and leaves two mechanisms driving one label. TitleEditor.Text has no
				// binding, so pushing it here is the whole of its ownership.
				var banner = (DmFlagBanner)bindable;
				banner.TitleEditor.Text = (string?)newValue ?? string.Empty;
			});

	/// <summary>
	///		Identifies the <see cref="IsTitleEditable"/> bindable property.
	/// </summary>
	public static readonly BindableProperty IsTitleEditableProperty =
		BindableProperty.Create(
			nameof(IsTitleEditable),
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
	public bool IsTitleEditable
	{
		get => (bool)GetValue(IsTitleEditableProperty);
		set => SetValue(IsTitleEditableProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="DmFlagBanner"/> and loads the XAML component tree.
	/// </summary>
	public DmFlagBanner()
	{
		InitializeComponent();
	}

	private void ApplyEditability(bool editable)
	{
		TitleLabel.IsVisible = !editable;
		TitleEditor.IsVisible = editable;
	}

	private void OnTitleCommitted(object? sender, string? value)
	{
		// Route the committed value through Title rather than painting TitleLabel directly, so the
		// static label stays seamless on the way back without a second writer. Title is the one
		// owner of the banner heading; the label is its projection through the XAML binding.
		Title = value ?? string.Empty;
		TextCommitted?.Invoke(this, value);
	}
}
