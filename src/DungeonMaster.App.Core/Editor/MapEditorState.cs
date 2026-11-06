using System.ComponentModel;
using System.Runtime.CompilerServices;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.App.Core.Editor;

/// <summary>
///		Holds the dungeon editor's interaction state and its transitions, with no MAUI dependency.
/// </summary>
/// <remarks>
///		The active tool is not owned here: it stays on the map canvas view model, and every method
///		that needs it takes it as a parameter. The in-progress vertex list likewise stays on
///		MapDrawingSession. Gesture plumbing, cursor shape, opacity and input transparency
///		toggling, and placeholder insertion all remain view work in the page.
///		<para>
///		The two selection facts, <see cref="SelectedFloorId"/> and <see cref="FocusedAreaId"/>,
///		are observable: the sidebar rows bind to them, so they are the one owner of which floor is
///		selected and which area is focused. No other member raises
///		<see cref="PropertyChanged"/>; the drawing, drag, pan and object-interaction state is read
///		by the page on demand and nothing binds to it.
///		</para>
/// </remarks>
public sealed class MapEditorState : INotifyPropertyChanged
{
	/// <summary>
	///		The distance in device-independent units a pointer must travel before a press on an
	///		object becomes a drag.
	/// </summary>
	public const float ObjectDragThreshold = 6f;

	private Guid? _selectedFloorId;

	private Guid? _focusedAreaId;

	/// <summary>
	///		Raised after <see cref="SelectedFloorId"/> or <see cref="FocusedAreaId"/> changes to a
	///		different value. Setting either to the value it already holds raises nothing.
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	// -- Selection ---------------------------------------------------------------------------

	/// <summary>
	///		Gets or sets the identifier of the floor currently selected in the sidebar, or null when
	///		no floor is selected.
	/// </summary>
	/// <remarks>
	///		A change raises <see cref="PropertyChanged"/> once, after the new value is stored.
	/// </remarks>
	public Guid? SelectedFloorId
	{
		get => _selectedFloorId;
		set
		{
			if (_selectedFloorId == value)
			{
				return;
			}

			_selectedFloorId = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	///		Gets or sets the identifier of the area the notebook is focused on, or null when none is.
	/// </summary>
	/// <remarks>
	///		A change raises <see cref="PropertyChanged"/> once, after the new value is stored.
	/// </remarks>
	public Guid? FocusedAreaId
	{
		get => _focusedAreaId;
		set
		{
			if (_focusedAreaId == value)
			{
				return;
			}

			_focusedAreaId = value;
			OnPropertyChanged();
		}
	}

	// -- Drawing -----------------------------------------------------------------------------

	/// <summary>
	///		Gets the state of the outline-drawing interaction. Only <see cref="SetActiveTool"/> and
	///		<see cref="ResetDrawState"/> change it.
	/// </summary>
	public DrawState DrawState { get; private set; }

	// -- Area move drag ----------------------------------------------------------------------

	/// <summary>
	///		Gets a value indicating whether an area outline is currently being dragged.
	/// </summary>
	public bool IsMoveDragging { get; private set; }

	/// <summary>
	///		Gets the identifier of the area being dragged, or null when no drag is in progress.
	/// </summary>
	public Guid? DraggingAreaId { get; private set; }

	/// <summary>
	///		Gets the tile the pointer grabbed the dragged area by, or null when no drag is in
	///		progress.
	/// </summary>
	public TilePoint? MoveGrabTile { get; private set; }

	// -- Pan and ghost -----------------------------------------------------------------------

	/// <summary>
	///		Gets or sets the screen point a pan gesture started from, or null when no pan is in
	///		progress.
	/// </summary>
	public CanvasPoint? PanAnchorScreen { get; set; }

	/// <summary>
	///		Gets or sets the rotation applied to the ghost object the place tool previews.
	/// </summary>
	public ObjectRotation GhostRotation { get; set; }

	// -- Object interaction ------------------------------------------------------------------

	/// <summary>
	///		Gets or sets the identifier of the object under the cursor — the canvas highlight that
	///		follows it — or null when the cursor is over no object.
	/// </summary>
	public Guid? HoveredObjectId { get; set; }

	/// <summary>
	///		Gets or sets the identifier of the selected object, the popup target set on click or
	///		long press, or null when nothing is selected.
	/// </summary>
	public Guid? SelectedObjectId { get; set; }

	/// <summary>
	///		Gets or sets the hit zone guarding popup button dispatch, or null when no popup is shown.
	/// </summary>
	public CanvasRect? PopupBounds { get; set; }

	/// <summary>
	///		Gets or sets the last screen point a touch was seen at, or null when none has been seen.
	/// </summary>
	public CanvasPoint? LastTouchPoint { get; set; }

	/// <summary>
	///		Gets the identifier of the object pressed but not yet disambiguated into a click or a
	///		drag, or null when no press is pending.
	/// </summary>
	public Guid? PendingObjectClickId { get; private set; }

	/// <summary>
	///		Gets the screen point the pending object press started from, or null when no press is
	///		pending.
	/// </summary>
	public CanvasPoint? ObjectDragAnchorScreen { get; private set; }

	/// <summary>
	///		Gets a value indicating whether the pending object press has been promoted to a drag.
	/// </summary>
	public bool IsObjectDragConfirmed { get; private set; }

	/// <summary>
	///		Gets or sets the floor-absolute pixel position the dragged object is being previewed at,
	///		or null when no drag preview is showing.
	/// </summary>
	public CanvasPoint? ObjectDragTargetCanvas { get; set; }

	/// <summary>
	///		Gets or sets a value indicating whether a long press has already been handled, which
	///		prevents the end of the interaction re-triggering a click.
	/// </summary>
	public bool ObjectLongPressHandled { get; set; }

	// -- Transitions -------------------------------------------------------------------------

	/// <summary>
	///		Applies the state half of a tool change and reports what the view must do.
	/// </summary>
	/// <param name="tool">
	///		The tool being switched to.
	/// </param>
	/// <param name="drawingSessionIsActive">
	///		Whether the drawing session is active at the moment of the switch.
	/// </param>
	/// <returns>
	///		The view work implied by the change.
	/// </returns>
	/// <remarks>
	///		Object interaction is cleared unconditionally on every tool change, including a switch
	///		to the tool that is already active. Note the ordering this preserves: the draw state is
	///		read before it is assigned Idle, because that read is what decides whether an
	///		in-progress draw must be aborted. Reordering the two silently stops aborts firing.
	/// </remarks>
	public ToolChange SetActiveTool(MapTool tool, bool drawingSessionIsActive)
	{
		var abortInProgressDraw = false;

		if (tool == MapTool.Draw)
		{
			DrawState = drawingSessionIsActive
				? DrawState.Drawing
				: DrawState.DrawingReady;
		}
		else
		{
			// Read before the assignment below: an abort is owed only when a draw was in progress.
			abortInProgressDraw = DrawState == DrawState.Drawing;
			DrawState = DrawState.Idle;
		}

		// The canvas hover highlight is shown only by the tools that act on an existing area.
		var clearHoveredArea =
			tool != MapTool.Move &&
			tool != MapTool.Select &&
			tool != MapTool.Erase;

		var leavingPlaceTool = tool != MapTool.Place;

		ClearObjectInteraction();

		return new ToolChange(abortInProgressDraw, clearHoveredArea, leavingPlaceTool);
	}

	/// <summary>
	///		Resets draw and area-move state after a draw completes or is abandoned.
	/// </summary>
	/// <param name="aFloorIsSelected">
	///		Whether a floor is selected; pass <c>SelectedFloorId is not null</c>.
	/// </param>
	/// <remarks>
	///		This does not touch object-interaction state, and it does not reset the drawing session
	///		or any drawable field — the page still does both, because both are outside this type.
	/// </remarks>
	public void ResetDrawState(bool aFloorIsSelected)
	{
		DrawState = aFloorIsSelected
			? DrawState.DrawingReady
			: DrawState.Idle;

		IsMoveDragging = false;
		DraggingAreaId = null;
		MoveGrabTile = null;
	}

	/// <summary>
	///		Begins an area move drag.
	/// </summary>
	/// <param name="areaId">
	///		The identifier of the area being dragged.
	/// </param>
	/// <param name="grabTile">
	///		The tile the pointer grabbed the area by.
	/// </param>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="grabTile"/> is null.
	/// </exception>
	public void BeginMoveDrag(Guid areaId, TilePoint grabTile)
	{
		ArgumentNullException.ThrowIfNull(grabTile);

		IsMoveDragging = true;
		DraggingAreaId = areaId;
		MoveGrabTile = grabTile;
	}

	/// <summary>
	///		Ends an area move drag. Calling it when no drag is in progress is a no-op, not an error.
	/// </summary>
	public void EndMoveDrag()
	{
		IsMoveDragging = false;
		DraggingAreaId = null;
		MoveGrabTile = null;
	}

	/// <summary>
	///		Records a press on an object that has not yet been disambiguated into a click or a drag.
	/// </summary>
	/// <param name="objectId">
	///		The identifier of the pressed object.
	/// </param>
	/// <param name="screenPoint">
	///		The screen point the press landed at, which becomes the drag anchor.
	/// </param>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="screenPoint"/> is null.
	/// </exception>
	public void BeginObjectPress(Guid objectId, CanvasPoint screenPoint)
	{
		ArgumentNullException.ThrowIfNull(screenPoint);

		PendingObjectClickId = objectId;
		ObjectDragAnchorScreen = screenPoint;
		IsObjectDragConfirmed = false;
	}

	/// <summary>
	///		Promotes a pending object press to a confirmed drag once the pointer has moved further
	///		than <see cref="ObjectDragThreshold"/> from the anchor.
	/// </summary>
	/// <param name="screenPoint">
	///		The current screen point of the pointer.
	/// </param>
	/// <returns>
	///		True when the press is now a confirmed drag, including when it was already confirmed.
	///		False when there is no pending press, or the pointer is still inside the threshold.
	/// </returns>
	/// <remarks>
	///		The comparison is on Euclidean distance and is strictly greater-than, so a pointer
	///		exactly <see cref="ObjectDragThreshold"/> units from the anchor is not yet a drag.
	/// </remarks>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="screenPoint"/> is null.
	/// </exception>
	public bool ConfirmObjectDragIfPastThreshold(CanvasPoint screenPoint)
	{
		ArgumentNullException.ThrowIfNull(screenPoint);

		if (PendingObjectClickId is null || ObjectDragAnchorScreen is null)
		{
			return false;
		}

		if (IsObjectDragConfirmed)
		{
			return true;
		}

		var dx = screenPoint.X - ObjectDragAnchorScreen.X;
		var dy = screenPoint.Y - ObjectDragAnchorScreen.Y;
		var distance = MathF.Sqrt((dx * dx) + (dy * dy));

		if (distance > ObjectDragThreshold)
		{
			IsObjectDragConfirmed = true;
			return true;
		}

		return false;
	}

	/// <summary>
	///		Clears every object-interaction field.
	/// </summary>
	public void ClearObjectInteraction()
	{
		SelectedObjectId = null;
		HoveredObjectId = null;
		PendingObjectClickId = null;
		ObjectDragAnchorScreen = null;
		ObjectDragTargetCanvas = null;
		PopupBounds = null;
		IsObjectDragConfirmed = false;
		ObjectLongPressHandled = false;
	}

	private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
