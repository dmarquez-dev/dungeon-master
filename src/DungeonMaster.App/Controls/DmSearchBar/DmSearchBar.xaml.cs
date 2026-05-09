using MauiApplication = Microsoft.Maui.Controls.Application;

namespace DungeonMaster.App.Controls;

public partial class DmSearchBar : ContentView
{
    public static readonly BindableProperty SearchTextProperty =
        BindableProperty.Create(
            nameof(SearchText), typeof(string), typeof(DmSearchBar), string.Empty,
            BindingMode.TwoWay);

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public DmSearchBar() => InitializeComponent();

    private void OnSearchFocused(object? sender, FocusEventArgs e)
    {
        SearchBorder.Stroke = (Brush)MauiApplication.Current!.Resources["DMCardBorderHoverBrush"];
        if (SearchBorder.Shadow is Shadow shadow) shadow.Opacity = 0.35f;
    }

    private void OnSearchUnfocused(object? sender, FocusEventArgs e)
    {
        SearchBorder.Stroke = (Brush)MauiApplication.Current!.Resources["DMCardBorderBrush"];
        if (SearchBorder.Shadow is Shadow shadow) shadow.Opacity = 0f;
    }
}
