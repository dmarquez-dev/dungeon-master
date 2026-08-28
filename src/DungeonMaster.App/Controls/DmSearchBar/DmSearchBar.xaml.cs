namespace DungeonMaster.App.Controls;

/// <summary>
///		A themed search bar control that wraps a native entry.
///		Focus and blur are painted declaratively in DmSearchBar.xaml, not from here; see the
///		comment there for why not a visual state. This class carries the
///		<see cref="SearchText"/> binding and nothing else.
/// </summary>
public partial class DmSearchBar : ContentView
{
	/// <summary>
	///		Identifies the <see cref="SearchText"/> bindable property.
	/// </summary>
	public static readonly BindableProperty SearchTextProperty =
		BindableProperty.Create(
			nameof(SearchText),
			typeof(string),
			typeof(DmSearchBar),
			string.Empty,
			BindingMode.TwoWay);

	/// <summary>
	///		Gets or sets the current search query text. Supports two-way data binding.
	/// </summary>
	public string SearchText
	{
		get => (string)GetValue(SearchTextProperty);
		set => SetValue(SearchTextProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="DmSearchBar"/> and loads the XAML component tree.
	/// </summary>
	public DmSearchBar()
	{
		InitializeComponent();
	}
}
