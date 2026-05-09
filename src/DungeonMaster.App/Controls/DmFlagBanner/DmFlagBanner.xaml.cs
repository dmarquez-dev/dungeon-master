namespace DungeonMaster.App.Controls;

public partial class DmFlagBanner : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(DmFlagBanner), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public DmFlagBanner() => InitializeComponent();
}
