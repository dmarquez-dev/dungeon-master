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

	/// <summary>Dark-tavern primary text gold. Mirrors the DmTextTavern token.</summary>
	public static readonly Color TextTavern = Color.FromArgb("#F5E4B0");

	/// <summary>Destructive action red. Mirrors the DmDestructive token.</summary>
	public static readonly Color Destructive = Color.FromArgb("#8B2020");

	/// <summary>Faint red wash behind a hovered destructive icon button. Mirrors the DmDestructiveHoverSurface token.</summary>
	public static readonly Color DestructiveHoverSurface = Color.FromArgb("#248B2020");

	/// <summary>The notebook's "saved" stamp green. Mirrors the DmSavedStamp token.</summary>
	public static readonly Color SavedStamp = Color.FromArgb("#4A6A2A");

	/// <summary>Parchment ink for text on a tool-panel surface. Mirrors the DmTextParchment token.</summary>
	public static readonly Color TextParchment = Color.FromArgb("#2C1A08");

	/// <summary>Tool-palette chip surface — normal state. Mirrors the DmSurfaceToolChip token.</summary>
	public static readonly Color SurfaceToolChip = Color.FromArgb("#C8AC78");

	/// <summary>Tool-palette chip surface — hover state. Mirrors the DmSurfaceToolChipHover token.</summary>
	public static readonly Color SurfaceToolChipHover = Color.FromArgb("#E8CC9A");

	/// <summary>Tool-palette chip border — normal state. Mirrors the DmBorderToolChip token.</summary>
	public static readonly Color BorderToolChip = Color.FromArgb("#9A7820");

	/// <summary>Object action popup button wash — gold hover. Mirrors the DmPopupButtonHover token.</summary>
	public static readonly Color PopupButtonHover = Color.FromArgb("#33C8A040");

	/// <summary>Object action popup button wash — gold pressed. Mirrors the DmPopupButtonPressed token.</summary>
	public static readonly Color PopupButtonPressed = Color.FromArgb("#66C8A040");

	/// <summary>Object action popup button wash — destructive hover. Mirrors the DmPopupButtonDestructiveHover token.</summary>
	public static readonly Color PopupButtonDestructiveHover = Color.FromArgb("#33CC3030");

	/// <summary>Object action popup button wash — destructive pressed. Mirrors the DmPopupButtonDestructivePressed token.</summary>
	public static readonly Color PopupButtonDestructivePressed = Color.FromArgb("#66CC3030");

	/// <summary>Map canvas — grid line colour.</summary>
	public static readonly Color MapGrid = Color.FromArgb("#1AC4862A");

	/// <summary>Map canvas — committed area outline colour. Shares its value with <see cref="Gold"/> and <see cref="ScrollGoldLight"/> today, but is deliberately a separate member: the map's area colour and the UI accent gold are independent design decisions that a future restyle may want to move apart.</summary>
	public static readonly Color MapArea = Color.FromArgb("#C8A040");

	/// <summary>Map canvas — outline vertex handle colour.</summary>
	public static readonly Color MapVertex = Color.FromArgb("#F5E4B0");

	/// <summary>Map canvas — fill of the area currently being drawn.</summary>
	public static readonly Color MapAreaFillActive = Color.FromArgb("#40C8A040");

	/// <summary>Map canvas — stroke of the area currently being drawn.</summary>
	public static readonly Color MapAreaStrokeActive = Color.FromArgb("#90C8A040");
}
