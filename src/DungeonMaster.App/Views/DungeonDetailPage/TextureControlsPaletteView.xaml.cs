namespace DungeonMaster.App.Views.DungeonDetail;

public enum TexturePlacementMode
{
	SingleTile,
	PaintFill,
	PointStructure,
	EdgeStructure,
}

public partial class TextureControlsPaletteView : ContentView
{
	public event EventHandler<TexturePlacementMode>? ModeChanged;

	private TexturePlacementMode _mode = TexturePlacementMode.SingleTile;
	private bool _isFloorOrWall = true;

	public TexturePlacementMode ActiveMode => _mode;

	public TextureControlsPaletteView()
	{
		InitializeComponent();

		AttachButtonBehavior(LeftModeBorder,  OnLeftModeClicked);
		AttachButtonBehavior(RightModeBorder, OnRightModeClicked);

		this.Loaded += (_, _) =>
		{
			// Icon labels start in floor/wall mode (matching XAML defaults).
			// Just wire up tooltips and active highlight; no text changes needed.
			ToolTipProperties.SetText(LeftModeBorder,  "Single Tile");
			ToolTipProperties.SetText(RightModeBorder, "Paint Fill");
			UpdateButtonHighlights();
		};
	}

	/// <summary>
	///		Updates the toggle labels to match the selected category (Floor, Wall, or Structure).
	///		Auto-sets mode: Structure Point → PointStructure, Structure Edge → EdgeStructure.
	/// </summary>
	public void SetCategory(string category, bool isEdgeStructure = false)
	{
		_isFloorOrWall = category is "Floor" or "Wall";

		if (_isFloorOrWall)
		{
			// Show the RPG Awesome icon glyphs defined in XAML (&#xE963; / &#xEA02;)
			LeftModeIconLabel.IsVisible  = true;
			LeftModeLabel.Text           = "Single";
			ToolTipProperties.SetText(LeftModeBorder, "Single Tile");

			RightModeIconLabel.IsVisible = true;
			RightModeLabel.Text          = "Fill";
			ToolTipProperties.SetText(RightModeBorder, "Paint Fill");

			if (_mode is TexturePlacementMode.PointStructure or TexturePlacementMode.EdgeStructure)
				SetMode(TexturePlacementMode.SingleTile);
		}
		else
		{
			LeftModeIconLabel.IsVisible  = false;
			LeftModeLabel.Text           = "Point";
			ToolTipProperties.SetText(LeftModeBorder, "Point Structure");

			RightModeIconLabel.IsVisible = false;
			RightModeLabel.Text          = "Edge";
			ToolTipProperties.SetText(RightModeBorder, "Edge Structure");

			SetMode(isEdgeStructure ? TexturePlacementMode.EdgeStructure : TexturePlacementMode.PointStructure);
		}

		UpdateButtonHighlights();
	}

	private void OnLeftModeClicked(object? sender, TappedEventArgs e)
	{
		SetMode(_isFloorOrWall ? TexturePlacementMode.SingleTile : TexturePlacementMode.PointStructure);
		UpdateButtonHighlights();
	}

	private void OnRightModeClicked(object? sender, TappedEventArgs e)
	{
		SetMode(_isFloorOrWall ? TexturePlacementMode.PaintFill : TexturePlacementMode.EdgeStructure);
		UpdateButtonHighlights();
	}

	private void SetMode(TexturePlacementMode mode)
	{
		_mode = mode;
		ModeChanged?.Invoke(this, mode);
	}

	private void UpdateButtonHighlights()
	{
		var leftActive = _mode is TexturePlacementMode.SingleTile or TexturePlacementMode.PointStructure;
		SetButtonActive(LeftModeBorder,  leftActive);
		SetButtonActive(RightModeBorder, !leftActive);
	}

	private static void SetButtonActive(Border btn, bool active)
	{
		// Dark ink border is far more legible against parchment than the gold accent color.
		btn.Stroke          = active ? Color.FromArgb("#2C1A08") : Colors.Transparent;
		btn.StrokeThickness = active ? 2 : 1;
	}

	private static void AttachButtonBehavior(Border btn, EventHandler<TappedEventArgs> action)
	{
		var tap = new TapGestureRecognizer();
		tap.Tapped += action;
		btn.GestureRecognizers.Add(tap);
	}
}
