namespace DungeonMaster.App.Controls;

/// <summary>
///		A themed popup chrome control that wraps arbitrary body content inside a styled frame with a title header.
/// </summary>
public partial class DmPopup : ContentView
{
	/// <summary>
	///		Identifies the <see cref="Title"/> bindable property.
	/// </summary>
	public static readonly BindableProperty TitleProperty =
		BindableProperty.Create(
			nameof(Title),
			typeof(string),
			typeof(DmPopup),
			string.Empty);

	/// <summary>
	///		Identifies the <see cref="Body"/> bindable property.
	/// </summary>
	public static readonly BindableProperty BodyProperty =
		BindableProperty.Create(
			nameof(Body),
			typeof(View),
			typeof(DmPopup),
			null,
			propertyChanged: (bindable, _, newValue) =>
			{
				if (bindable is DmPopup popup && newValue is View view)
				{
					popup.BodyPresenter.Content = view;
				}
			});

	/// <summary>
	///		Gets or sets the heading text displayed in the popup title bar.
	/// </summary>
	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	/// <summary>
	///		Gets or sets the <see cref="View"/> rendered inside the popup body area.
	/// </summary>
	public View Body
	{
		get => (View)GetValue(BodyProperty);
		set => SetValue(BodyProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="DmPopup"/> and loads the XAML component tree.
	/// </summary>
	public DmPopup()
	{
		InitializeComponent();
	}
}
