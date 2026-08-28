// ============================================================================================
// FROZEN CONTRACT — DungeonMaster.App.Core editor kernel
// Session: 2026-08-27-give-the-editor-a-seam (sub-session 5b)
// Owner:    T1 (dir-backend-engineer)
// Consumers: T2 (tests, DungeonMaster.Tests), T5 (DungeonDetailPage.xaml.cs)
//
// Everything declared here lives in src/DungeonMaster.App.Core, target framework net10.0.
// That project references ONLY DungeonMaster.Domain and DungeonMaster.Application. It has no
// MAUI reference and MUST NOT acquire one. In particular:
//
//   - Microsoft.Maui.Graphics types (PointF, RectF, Color, ...) MUST NOT appear in any
//     signature or field declared here. There is no Microsoft.Maui.Graphics PackageVersion in
//     Directory.Packages.props (verified: `grep -c Maui.Graphics Directory.Packages.props` -> 0),
//     so adding one would change Directory.Packages.props AND
//     src/DungeonMaster.App.Core/packages.lock.json, neither of which any task in this manifest
//     owns.
//   - Pixel and screen positions are carried as DungeonMaster.Domain.ValueObjects.CanvasPoint
//     (a `record CanvasPoint(float X, float Y)`), as bare float pairs, or as the CanvasRect
//     declared below. Tile positions are carried as DungeonMaster.Domain.ValueObjects.TilePoint
//     (a `record TilePoint(int Col, int Row)`).
//
// T1 adds NO ProjectReference and NO PackageReference. It therefore does not touch
// src/DungeonMaster.App.Core/DungeonMaster.App.Core.csproj or its packages.lock.json, and does
// not need to own them.
//
// House style, binding on every file created under this contract:
//   - tab indentation (NOT spaces), CRLF line endings, UTF-8 with no byte-order mark,
//     final newline. .editorconfig:4 declares `indent_style = tab` and .editorconfig:8 declares
//     `end_of_line = crlf`; IDE0055 is at `warning` (.editorconfig:64) under
//     TreatWarningsAsErrors (Directory.Build.props:5), so a space-indented new file fails the
//     build.
//   - Every public type and member carries an XML doc comment, matching the surrounding
//     App.Core files (see Map/TileCoordinateHelper.cs).
// ============================================================================================

// --------------------------------------------------------------------------------------------
// SECTION 1 — namespace DungeonMaster.App.Core.Map
// New file: src/DungeonMaster.App.Core/Map/AreaOutlineGeometry.cs
// --------------------------------------------------------------------------------------------

namespace DungeonMaster.App.Core.Map;

using DungeonMaster.Domain.ValueObjects;

/// <summary>
///		Pure geometry over an area outline. No MAUI type appears in this file.
/// </summary>
public static class AreaOutlineGeometry
{
	/// <summary>
	///		Returns a new outline in which End, Ctrl1 and Ctrl2 of every segment are offset by
	///		<paramref name="delta"/>. Null control points stay null.
	/// </summary>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="outline"/> or <paramref name="delta"/> is null, or any element of
	///		<paramref name="outline"/> is null.
	/// </exception>
	/// <remarks>
	///		This is the exact value that is persisted by UpdateAreaOutlineCommand. It is
	///		extracted verbatim from the `.Select(segment => segment with { ... })` expression in
	///		DungeonDetailPage.xaml.cs — measured 2026-08-27 at lines 1712-1719, in the
	///		`ActiveTool == MapTool.Move && _isMoveDragging` branch of OnMapEndInteraction
	///		(`grep -n "PathSegment" src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`
	///		-> one hit, :1724). An empty outline returns an empty list. A zero delta returns an
	///		equal-valued outline; it is NOT required to return the same instance.
	/// </remarks>
	public static IReadOnlyList<PathSegment> Translate(
		IReadOnlyList<PathSegment> outline,
		TilePoint delta);

	/// <summary>
	///		Builds the closed pixel-space polygon of an outline.
	/// </summary>
	/// <remarks>
	///		Reproduces the vertex construction inside HitTestArea (measured 2026-08-27 at
	///		DungeonDetailPage.xaml.cs:1830-1893) EXACTLY, and the order matters:
	///		  1. the first vertex is TileCoordinateHelper.ToPixel(outline[^1].End, tileSize) —
	///		     the polygon is closed, so the last segment's end is also the start;
	///		  2. then TileCoordinateHelper.ToPixel(segment.End, tileSize) for every segment in
	///		     order;
	///		  3. then, when the resulting list holds more than one vertex, the LAST vertex is
	///		     removed, because step 2's final entry duplicates step 1's.
	///		A drag preview is applied by calling <see cref="Translate"/> first and passing the
	///		translated outline here. Translate touches Ctrl1/Ctrl2 as well as End; this method
	///		reads End only, so the two compose to exactly the behaviour the page has today.
	///		An empty <paramref name="outline"/> returns an empty list.
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="outline"/> is null, or any element is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="tileSize"/> is not greater than zero.</exception>
	public static IReadOnlyList<(float X, float Y)> ToPolygon(
		IReadOnlyList<PathSegment> outline,
		float tileSize);

	/// <summary>
	///		Ray-casting point-in-polygon test.
	/// </summary>
	/// <remarks>
	///		Extracted verbatim from IsPointInPolygon (measured 2026-08-27 at
	///		DungeonDetailPage.xaml.cs:1895-1908). The crossing rule is
	///		`((yi > y) != (yj > y)) && x < (xj - xi) * (y - yi) / (yj - yi) + xi`, with the
	///		vertex pair walked as `for (int i = 0, j = n - 1; i < n; j = i++)`. Behaviour on the
	///		boundary is whatever that expression yields and is NOT specified here; do not
	///		"fix" it. A polygon of fewer than three vertices returns false.
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="polygon"/> is null.</exception>
	public static bool ContainsPoint(
		IReadOnlyList<(float X, float Y)> polygon,
		float x,
		float y);

	/// <summary>
	///		The absolute area of a polygon by the shoelace formula.
	/// </summary>
	/// <remarks>
	///		Extracted verbatim from PolygonArea (measured 2026-08-27 at
	///		DungeonDetailPage.xaml.cs:1910-1917): `sum += (polygon[j].X + polygon[i].X) *
	///		(polygon[j].Y - polygon[i].Y)` over the same index walk, returning
	///		`MathF.Abs(sum * 0.5f)`. Winding order does not affect the result.
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="polygon"/> is null.</exception>
	public static float Area(IReadOnlyList<(float X, float Y)> polygon);
}

// --------------------------------------------------------------------------------------------
// SECTION 2 — namespace DungeonMaster.App.Core.Map
// New file: src/DungeonMaster.App.Core/Map/MapHitTester.cs
// --------------------------------------------------------------------------------------------

/// <summary>
///		Hit testing over the presentation shapes the map canvas draws.
/// </summary>
public static class MapHitTester
{
	/// <summary>
	///		Returns the area whose outline polygon contains the given pixel point, preferring the
	///		smallest such area, or null when none does.
	/// </summary>
	/// <param name="areas">The floor's areas. An area with an empty Outline is skipped.</param>
	/// <param name="pixelX">Floor-absolute pixel X of the hit point.</param>
	/// <param name="pixelY">Floor-absolute pixel Y of the hit point.</param>
	/// <param name="tileSize">Tile edge length in pixels. Must be greater than zero.</param>
	/// <param name="dragPreviewAreaId">
	///		The area currently being drag-previewed, or null. When it matches an area's Id, that
	///		area's outline is translated by <paramref name="dragPreviewDelta"/> before testing,
	///		so hit testing follows the dragged position.
	/// </param>
	/// <param name="dragPreviewDelta">
	///		The drag offset in tiles, or null. Ignored when <paramref name="dragPreviewAreaId"/>
	///		is null.
	/// </param>
	/// <remarks>
	///		TIE-BREAK, frozen: the comparison is STRICTLY less-than on polygon area
	///		(`if (polyArea &lt; bestArea)`), exactly as DungeonDetailPage.xaml.cs:1885 has it
	///		today. Two containing areas of equal polygon area therefore resolve to the one that
	///		comes FIRST in <paramref name="areas"/>. This is observable and a test must pin it.
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="areas"/> is null, or any element is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="tileSize"/> is not greater than zero.</exception>
	public static AreaItem? HitTestArea(
		IReadOnlyList<AreaItem> areas,
		float pixelX,
		float pixelY,
		float tileSize,
		Guid? dragPreviewAreaId,
		TilePoint? dragPreviewDelta);

	/// <summary>
	///		Returns the placed object whose axis-aligned bounding box contains the given
	///		floor-absolute canvas point, preferring the smallest such object, or null when none
	///		does.
	/// </summary>
	/// <param name="objects">The floor's placed objects.</param>
	/// <param name="canvasX">Floor-absolute canvas X of the hit point.</param>
	/// <param name="canvasY">Floor-absolute canvas Y of the hit point.</param>
	/// <param name="tileSize">Tile edge length in pixels. Must be greater than zero.</param>
	/// <param name="objectSize">
	///		Logical bounding size of a kind in TILE units. In the App layer this is supplied as
	///		`ObjectKindInfo.GetObjectSize`. It is a delegate rather than a direct call because
	///		ObjectKindInfo lives in src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs,
	///		which no task in this manifest owns and which App.Core cannot reference. Do NOT move
	///		ObjectKindInfo in this session.
	/// </param>
	/// <remarks>
	///		Reproduces GetObjectAtCanvasPoint (measured 2026-08-27 at
	///		DungeonDetailPage.xaml.cs:1921-1947): half-extents are
	///		`wTiles * tileSize / 2f` and `hTiles * tileSize / 2f` about
	///		PlacedObjectItem.Position, which is the bounding-box CENTRE (see the Position doc on
	///		src/DungeonMaster.App.Core/Presentation/PlacedObjectItem.cs), the containment test is
	///		inclusive on all four edges, and the tie-break ranks on `wTiles * hTiles` — the
	///		UNSCALED tile area, NOT the pixel area and NOT PlacedObjectItem.Scale. The comparison
	///		is again STRICTLY less-than, so an exact tie resolves to the first element of
	///		<paramref name="objects"/>. PlacedObjectItem.Scale and Rotation are deliberately not
	///		consulted; preserving that is required, because changing it changes which object the
	///		user picks.
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="objects"/> or <paramref name="objectSize"/> is null, or any element is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="tileSize"/> is not greater than zero.</exception>
	public static PlacedObjectItem? HitTestObject(
		IReadOnlyList<PlacedObjectItem> objects,
		float canvasX,
		float canvasY,
		float tileSize,
		Func<ObjectKind, (float W, float H)> objectSize);
}

// --------------------------------------------------------------------------------------------
// SECTION 3 — namespace DungeonMaster.App.Core.Editor
// New files: src/DungeonMaster.App.Core/Editor/MapTool.cs
//            src/DungeonMaster.App.Core/Editor/DrawState.cs
//            src/DungeonMaster.App.Core/Editor/CanvasRect.cs
//            src/DungeonMaster.App.Core/Editor/ToolChange.cs
//            src/DungeonMaster.App.Core/Editor/MapEditorState.cs
// --------------------------------------------------------------------------------------------

namespace DungeonMaster.App.Core.Editor;

/// <summary>
///		Identifies the active drawing tool on the map canvas.
/// </summary>
/// <remarks>
///		THE MOVE OF THIS ENUM IS A THREE-STEP MIGRATION AND THE STEPS ARE FROZEN. The enum
///		exists today at src/DungeonMaster.App/ViewModels/MapTool.cs in namespace
///		DungeonMaster.App.ViewModels, and is named by exactly three .cs files
///		(`grep -rln "MapTool" src` on 2026-08-27 -> ViewModels/MapTool.cs,
///		ViewModels/DungeonDetailViewModel.cs, Views/DungeonDetailPage/DungeonDetailPage.xaml.cs;
///		the fourth hit, DungeonDetailPage.xaml, is `x:Name="MapToolbar"` and is NOT a use of the
///		type). The tree must build at 0 warnings after EACH task, so:
///
///		  Step 1 (T1): CREATE this enum here with the same six members in the same order —
///		    Draw, Move, Pan, Place, Select, Erase — and the same XML doc text. DO NOT delete or
///		    edit src/DungeonMaster.App/ViewModels/MapTool.cs. Two identically named enums now
///		    exist in different namespaces; nothing breaks, because same-namespace lookup keeps
///		    binding the App-layer one.
///		  Step 2 (T3): in DungeonDetailViewModel.cs add the USING ALIAS
///		    `using MapTool = DungeonMaster.App.Core.Editor.MapTool;`. A using alias outranks
///		    same-namespace lookup, so the view model retypes onto this enum with no other edit.
///		    A plain `using DungeonMaster.App.Core.Editor;` would NOT work there and must not be
///		    used for this purpose.
///		  Step 3 (T5): DELETE src/DungeonMaster.App/ViewModels/MapTool.cs and add
///		    `using DungeonMaster.App.Core.Editor;` to DungeonDetailPage.xaml.cs. The delete and
///		    the using land in the same task, so CS0104 ambiguity never occurs.
/// </remarks>
public enum MapTool
{
	Draw,
	Move,
	Pan,
	Place,
	Select,
	Erase,
}

/// <summary>
///		The three states of the outline-drawing interaction.
/// </summary>
/// <remarks>
///		Moved from the private nested `enum DrawState` in DungeonDetailPage.xaml.cs (measured
///		2026-08-27 at :41). Member names and order are unchanged.
/// </remarks>
public enum DrawState
{
	Idle,
	DrawingReady,
	Drawing,
}

/// <summary>
///		An axis-aligned rectangle in canvas pixels. Exists because Microsoft.Maui.Graphics.RectF
///		is unavailable to this project; it replaces the page's `RectF _popupBounds`.
/// </summary>
public sealed record CanvasRect(float X, float Y, float Width, float Height)
{
	/// <summary>
	///		Whether the point lies within this rectangle, inclusive on all four edges.
	/// </summary>
	public bool Contains(float x, float y);
}

/// <summary>
///		What the view must do as a result of a tool change. Returned by
///		<see cref="MapEditorState.SetActiveTool"/> so the decision is testable in App.Core while
///		the MAUI-touching work stays in the page.
/// </summary>
/// <param name="AbortInProgressDraw">
///		True when a draw was in progress and the new tool ends it. The page calls its own
///		AbortInProgressDraw().
/// </param>
/// <param name="ClearHoveredArea">
///		True when the new tool is none of Move, Select, Erase, so the canvas area-hover highlight
///		must be cleared.
/// </param>
/// <param name="LeavingPlaceTool">
///		True when the new tool is not Place. The page dismisses the secondary palette, clears the
///		ghost object and resets the controls-palette scale.
/// </param>
public readonly record struct ToolChange(
	bool AbortInProgressDraw,
	bool ClearHoveredArea,
	bool LeavingPlaceTool);

/// <summary>
///		The dungeon editor's interaction state and its transitions, with no MAUI dependency.
/// </summary>
/// <remarks>
///		<para>
///		WHAT THIS TYPE DOES NOT OWN. The active tool stays on MapCanvasViewModel (see
///		contracts/app-detail-view-models.cs); it has exactly one owner and this is not it. Every
///		method below that needs the tool takes it as a parameter. Likewise the in-progress
///		vertex list stays on MapDrawingSession, which already exists at
///		src/DungeonMaster.App.Core/Map/MapDrawingSession.cs and is untouched by this session.
///		</para>
///		<para>
///		WHAT STAYS IN THE PAGE. UI-016 names view work that legitimately belongs there and must
///		NOT move: gesture plumbing, cursor shape, opacity and InputTransparent toggling, and
///		placeholder Border insertion. Concretely, the page keeps `_dragPlaceholder`,
///		`_draggingItem`, `_currentPlaceholderTarget`, `_placeholderAfterTarget`,
///		`_longPressDetector`, `_keyboardBehavior`, `_runtimeHandlersAttached`, `_pinchGesture`,
///		`_pinchStartZoom`, `_viewModel` and `_drawable` — measured 2026-08-27, 28 private fields
///		in total by
///		`grep -cE "^\s+private (readonly )?[A-Za-z_].*\b_[a-zA-Z]+\s*(=|;)" DungeonDetailPage.xaml.cs`.
///		</para>
/// </remarks>
public sealed class MapEditorState
{
	/// <summary>Distance in device-independent units a pointer must travel before a press on an object becomes a drag. Was `private const float ObjectDragThreshold = 6f` at DungeonDetailPage.xaml.cs:63.</summary>
	public const float ObjectDragThreshold = 6f;

	// -- Selection ---------------------------------------------------------------------------

	/// <summary>Was `_selectedFloorId`.</summary>
	public Guid? SelectedFloorId { get; set; }

	/// <summary>Was `_focusedAreaId`.</summary>
	public Guid? FocusedAreaId { get; set; }

	// -- Drawing -----------------------------------------------------------------------------

	/// <summary>Was `_drawState`. Only <see cref="SetActiveTool"/> and <see cref="ResetDrawState"/> change it.</summary>
	public DrawState DrawState { get; private set; }

	// -- Area move drag ----------------------------------------------------------------------

	/// <summary>Was `_isMoveDragging`.</summary>
	public bool IsMoveDragging { get; private set; }

	/// <summary>Was `_draggingAreaId`.</summary>
	public Guid? DraggingAreaId { get; private set; }

	/// <summary>Was `_moveGrabTile`.</summary>
	public TilePoint? MoveGrabTile { get; private set; }

	// -- Pan and ghost -----------------------------------------------------------------------

	/// <summary>Was `PointF? _panAnchorScreen`.</summary>
	public CanvasPoint? PanAnchorScreen { get; set; }

	/// <summary>Was `_ghostRotation`. Defaults to <see cref="ObjectRotation.None"/>.</summary>
	public ObjectRotation GhostRotation { get; set; }

	// -- Object interaction ------------------------------------------------------------------

	/// <summary>Was `_hoveredObjectId` — the canvas highlight that follows the cursor.</summary>
	public Guid? HoveredObjectId { get; set; }

	/// <summary>Was `_selectedObjectId` — the popup target, set on click or long press.</summary>
	public Guid? SelectedObjectId { get; set; }

	/// <summary>Was `RectF _popupBounds` — the hit-zone guard for popup button dispatch. Null when no popup is shown.</summary>
	public CanvasRect? PopupBounds { get; set; }

	/// <summary>Was `PointF _lastTouchPoint`.</summary>
	public CanvasPoint? LastTouchPoint { get; set; }

	/// <summary>Was `_pendingObjectClickId`.</summary>
	public Guid? PendingObjectClickId { get; private set; }

	/// <summary>Was `PointF _objectDragAnchorScreen`.</summary>
	public CanvasPoint? ObjectDragAnchorScreen { get; private set; }

	/// <summary>Was `_isObjectDragConfirmed`.</summary>
	public bool IsObjectDragConfirmed { get; private set; }

	/// <summary>Was `_objectDragTargetCanvas` — the floor-absolute pixel position being previewed.</summary>
	public CanvasPoint? ObjectDragTargetCanvas { get; set; }

	/// <summary>Was `_objectLongPressHandled` — prevents EndInteraction re-triggering a click after a long press.</summary>
	public bool ObjectLongPressHandled { get; set; }

	// -- Transitions -------------------------------------------------------------------------

	/// <summary>
	///		Applies the state half of a tool change and reports what the view must do.
	/// </summary>
	/// <param name="tool">The tool being switched to.</param>
	/// <param name="drawingSessionIsActive">MapDrawingSession.IsActive at the moment of the switch.</param>
	/// <returns>The view work implied by the change.</returns>
	/// <remarks>
	///		Frozen behaviour, taken from SetActiveTool (measured 2026-08-27 at
	///		DungeonDetailPage.xaml.cs:1082-1136):
	///		  - tool == Draw: DrawState becomes Drawing when <paramref name="drawingSessionIsActive"/>,
	///		    otherwise DrawingReady; ToolChange.AbortInProgressDraw is false.
	///		  - any other tool: ToolChange.AbortInProgressDraw is true IF AND ONLY IF DrawState
	///		    was Drawing on entry; DrawState then becomes Idle unconditionally.
	///		  - ToolChange.ClearHoveredArea is true when tool is none of Move, Select, Erase.
	///		  - ToolChange.LeavingPlaceTool is true when tool is not Place.
	///		  - Object interaction is cleared unconditionally, exactly as
	///		    <see cref="ClearObjectInteraction"/> does, on EVERY tool change including a switch
	///		    to the same tool.
	///		Note the ordering trap this preserves: the page reads DrawState BEFORE assigning Idle
	///		in order to decide whether to abort. A reordering here silently stops aborts firing.
	/// </remarks>
	public ToolChange SetActiveTool(MapTool tool, bool drawingSessionIsActive);

	/// <summary>
	///		Resets draw and area-move state after a draw completes or is abandoned.
	/// </summary>
	/// <param name="aFloorIsSelected">Whether a floor is selected; pass `SelectedFloorId is not null`.</param>
	/// <remarks>
	///		Frozen behaviour, taken from ResetDrawState (measured 2026-08-27 at
	///		DungeonDetailPage.xaml.cs:1282-1300): DrawState becomes DrawingReady when a floor is
	///		selected and Idle otherwise; IsMoveDragging becomes false; DraggingAreaId and
	///		MoveGrabTile become null. It does NOT touch object interaction state, and it does NOT
	///		reset the drawing session or any FloorMapDrawable field — the page still does both,
	///		because both are outside this type.
	/// </remarks>
	public void ResetDrawState(bool aFloorIsSelected);

	/// <summary>
	///		Begins an area move drag.
	/// </summary>
	public void BeginMoveDrag(Guid areaId, TilePoint grabTile);

	/// <summary>
	///		Ends an area move drag: IsMoveDragging false, DraggingAreaId and MoveGrabTile null.
	///		Calling it when no drag is in progress is a no-op, not an error.
	/// </summary>
	public void EndMoveDrag();

	/// <summary>
	///		Records a press on an object that has not yet been disambiguated into a click or a
	///		drag. Sets PendingObjectClickId and ObjectDragAnchorScreen; IsObjectDragConfirmed
	///		becomes false.
	/// </summary>
	public void BeginObjectPress(Guid objectId, CanvasPoint screenPoint);

	/// <summary>
	///		Promotes a pending object press to a confirmed drag once the pointer has moved
	///		further than <see cref="ObjectDragThreshold"/> from the anchor.
	/// </summary>
	/// <returns>
	///		True when the press is now a confirmed drag — including when it was already
	///		confirmed. False when there is no pending press, or the pointer is still inside the
	///		threshold.
	/// </returns>
	/// <remarks>
	///		The comparison is on Euclidean distance and is STRICTLY greater-than, so a pointer
	///		exactly <see cref="ObjectDragThreshold"/> units from the anchor is NOT yet a drag.
	///		A test must pin the boundary at exactly 6f and at a value just above it.
	/// </remarks>
	public bool ConfirmObjectDragIfPastThreshold(CanvasPoint screenPoint);

	/// <summary>
	///		Clears every object-interaction field: SelectedObjectId, HoveredObjectId,
	///		PendingObjectClickId, ObjectDragAnchorScreen, ObjectDragTargetCanvas, PopupBounds all
	///		null, and IsObjectDragConfirmed and ObjectLongPressHandled false.
	/// </summary>
	public void ClearObjectInteraction();
}
