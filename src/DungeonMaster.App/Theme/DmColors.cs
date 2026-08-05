namespace DungeonMaster.App.Theme;

/// <summary>
///		Centralises semantic color definitions used across the Dungeon Master UI.
/// </summary>
public static class DmColors
{
	/// <summary>Active tool / focus gold highlight.</summary>
	public static readonly Color Gold = Color.FromArgb("#C8A040");

	/// <summary>Inactive tool / muted label color.</summary>
	public static readonly Color GoldMuted = Color.FromArgb("#A08050");

	/// <summary>Gold used for scroll / border glow effects.</summary>
	public static readonly Color ScrollGold = Color.FromArgb("#C4862A");

	/// <summary>Subtle gold hover tint background.</summary>
	public static readonly Color GoldHoverTint = Color.FromArgb("#10C8A040");

	/// <summary>Selected area border gold.</summary>
	public static readonly Color AreaSelectedBorder = Color.FromArgb("#C8A040");

	/// <summary>Selected area background tint.</summary>
	public static readonly Color AreaSelectedTint = Color.FromArgb("#18C8A040");

	/// <summary>Floor card surface — normal state.</summary>
	public static readonly Color FloorSurfaceNormal = Color.FromArgb("#2E8B5E1A");

	/// <summary>Floor card surface — hover state.</summary>
	public static readonly Color FloorSurfaceHover = Color.FromArgb("#528B5E1A");

	/// <summary>Floor card surface — expanded/active state.</summary>
	public static readonly Color FloorSurfaceActive = Color.FromArgb("#6A8B5E1A");

	/// <summary>Floor card border brush — normal state.</summary>
	public static readonly SolidColorBrush FloorBorderNormal = new(Color.FromArgb("#4DC4862A"));

	/// <summary>Floor card border brush — active/hover state.</summary>
	public static readonly SolidColorBrush FloorBorderActive = new(Color.FromArgb("#A6C4862A"));

	/// <summary>Save action — border/stroke green.</summary>
	public static readonly Color SaveGreen = Color.FromArgb("#4A8A30");

	/// <summary>Save action — text/icon green.</summary>
	public static readonly Color SaveGreenText = Color.FromArgb("#8ACA60");

	/// <summary>Discard action — border/stroke red.</summary>
	public static readonly Color DiscardRed = Color.FromArgb("#A03030");

	/// <summary>Discard action — text/icon red.</summary>
	public static readonly Color DiscardRedText = Color.FromArgb("#E08080");

	/// <summary>Drag-and-drop placeholder stroke.</summary>
	public static readonly Color DragPlaceholderStroke = Color.FromArgb("#C8A040");

	/// <summary>Drag-and-drop placeholder background tint.</summary>
	public static readonly Color DragPlaceholderFill = Color.FromArgb("#20C8A040");

	/// <summary>Focus/hover glow background tint for inline editors.</summary>
	public static readonly Color EditorGlowTint = Color.FromArgb("#08C4862A");

	/// <summary>Notebook icon — gold when the entity has authored content.</summary>
	public static readonly Color ScrollGoldLight = Color.FromArgb("#C8A040");

	/// <summary>Notebook icon — muted when the entity has no authored content.</summary>
	public static readonly Color InkMuted = Color.FromArgb("#7A5030");
}
