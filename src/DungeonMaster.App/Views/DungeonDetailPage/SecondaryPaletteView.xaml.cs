using DungeonMaster.App.Assets.ObjectAssets;
using DungeonMaster.App.Behaviors;
using DungeonMaster.Domain.Entities.Area;

namespace DungeonMaster.App.Views.DungeonDetail;

public partial class SecondaryPaletteView : ContentView
{
    public event EventHandler<ObjectKind>? ObjectSelected;
    public event EventHandler? CloseRequested;

    public SecondaryPaletteView()
    {
        InitializeComponent();
    }

    private void OnCloseClicked(object? sender, EventArgs e) =>
        CloseRequested?.Invoke(this, EventArgs.Empty);

    public void Populate(string category, IReadOnlyList<ObjectKind> objects)
    {
        TitleLabel.Text = $"All {category}";
        ObjectGrid.Children.Clear();
        ObjectGrid.RowDefinitions.Clear();

        var rows = (int)Math.Ceiling(objects.Count / 5.0);
        for (var r = 0; r < rows; r++)
        {
            ObjectGrid.RowDefinitions.Add(new RowDefinition { Height = 48 });
        }

        for (var i = 0; i < objects.Count; i++)
        {
            var kind = objects[i];
            var name = ObjectKindInfo.GetDisplayName(kind);

            View content;
            if (ObjectAssetCatalogue.Paths.TryGetValue(kind, out var path))
            {
                content = new Image
                {
                    Source = path,
                    Aspect = Aspect.AspectFit,
                    Margin = 3
                };
            }
            else
            {
                content = new Label
                {
                    Text = name.Length > 0 ? name[..1] : "?",
                    FontSize = 11,
                    TextColor = Color.FromArgb("#2C1A08"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
            }

            var border = new Border
            {
                WidthRequest = 48,
                HeightRequest = 48,
                Padding = 0,
                BackgroundColor = Color.FromArgb("#C8AC78"),
                Stroke = new SolidColorBrush(Color.FromArgb("#9A7820")),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2 },
                Content = content
            };
            ToolTipProperties.SetText(border, name);

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) =>
                ObjectSelected?.Invoke(this, kind);
            border.GestureRecognizers.Add(tap);

            border.Behaviors.Add(new HandCursorBehavior());
            var ptr = new PointerGestureRecognizer();
            ptr.PointerEntered += (_, _) =>
            {
                border.BackgroundColor = Color.FromArgb("#E8CC9A");
                border.Stroke = new SolidColorBrush(Color.FromArgb("#C8A040"));
            };
            ptr.PointerExited += (_, _) =>
            {
                border.BackgroundColor = Color.FromArgb("#C8AC78");
                border.Stroke = new SolidColorBrush(Color.FromArgb("#9A7820"));
            };
            border.GestureRecognizers.Add(ptr);

            ObjectGrid.Add(border, i % 5, i / 5);
        }
    }
}
