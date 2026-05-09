namespace DungeonMaster.App.Controls;

public partial class DmPopup : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(DmPopup), string.Empty);

    public static readonly BindableProperty BodyProperty =
        BindableProperty.Create(nameof(Body), typeof(View), typeof(DmPopup), null,
            propertyChanged: (bindable, _, newValue) =>
            {
                if (bindable is DmPopup popup && newValue is View view)
                    popup.BodyPresenter.Content = view;
            });

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public View Body
    {
        get => (View)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public DmPopup() => InitializeComponent();
}
