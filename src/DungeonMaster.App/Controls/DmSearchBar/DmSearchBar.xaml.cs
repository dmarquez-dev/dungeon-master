using MauiApplication = Microsoft.Maui.Controls.Application;

namespace DungeonMaster.App.Controls;

/// <summary>
///		A themed search bar control that wraps a native entry with styled focus and blur states.
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

	private void OnSearchFocused(object? sender, FocusEventArgs e)
	{
		SearchBorder.Stroke = (Brush)MauiApplication.Current!.Resources["DMCardBorderHoverBrush"];
	}

	private void OnSearchUnfocused(object? sender, FocusEventArgs e)
	{
		SearchBorder.Stroke = (Brush)MauiApplication.Current!.Resources["DMCardBorderBrush"];
	}
}
