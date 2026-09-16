using DungeonMaster.App.Core.Editor;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore.Editor;

public class MapEditorStateTests
{
	[Fact]
	public void SetActiveTool_DrawWithAnActiveDrawingSession_SetsDrawStateToDrawing()
	{
		var state = new MapEditorState();

		var change = state.SetActiveTool(MapTool.Draw, drawingSessionIsActive: true);

		state.DrawState.Should().Be(DrawState.Drawing);
		change.AbortInProgressDraw.Should().BeFalse();
	}

	[Fact]
	public void SetActiveTool_DrawWithNoActiveDrawingSession_SetsDrawStateToDrawingReady()
	{
		var state = new MapEditorState();

		var change = state.SetActiveTool(MapTool.Draw, drawingSessionIsActive: false);

		state.DrawState.Should().Be(DrawState.DrawingReady);
		change.AbortInProgressDraw.Should().BeFalse();
	}

	[Fact]
	public void SetActiveTool_DrawWhileAlreadyDrawing_ReportsNoAbort()
	{
		var state = new MapEditorState();
		state.SetActiveTool(MapTool.Draw, drawingSessionIsActive: true);

		var change = state.SetActiveTool(MapTool.Draw, drawingSessionIsActive: true);

		change.AbortInProgressDraw.Should().BeFalse();
		state.DrawState.Should().Be(DrawState.Drawing);
	}

	[Theory]
	[InlineData(MapTool.Move)]
	[InlineData(MapTool.Pan)]
	[InlineData(MapTool.Place)]
	[InlineData(MapTool.Select)]
	[InlineData(MapTool.Erase)]
	public void SetActiveTool_NonDrawToolWhileDrawing_ReportsAbortAndLeavesDrawStateIdle(MapTool tool)
	{
		var state = new MapEditorState();
		state.SetActiveTool(MapTool.Draw, drawingSessionIsActive: true);
		state.DrawState.Should().Be(DrawState.Drawing);

		var change = state.SetActiveTool(tool, drawingSessionIsActive: true);

		change.AbortInProgressDraw.Should().BeTrue();
		state.DrawState.Should().Be(DrawState.Idle);
	}

	[Theory]
	[InlineData(MapTool.Move)]
	[InlineData(MapTool.Pan)]
	[InlineData(MapTool.Place)]
	[InlineData(MapTool.Select)]
	[InlineData(MapTool.Erase)]
	public void SetActiveTool_NonDrawToolFromDrawingReady_ReportsNoAbortAndLeavesDrawStateIdle(MapTool tool)
	{
		var state = new MapEditorState();
		state.SetActiveTool(MapTool.Draw, drawingSessionIsActive: false);
		state.DrawState.Should().Be(DrawState.DrawingReady);

		var change = state.SetActiveTool(tool, drawingSessionIsActive: false);

		change.AbortInProgressDraw.Should().BeFalse();
		state.DrawState.Should().Be(DrawState.Idle);
	}

	[Theory]
	[InlineData(MapTool.Move)]
	[InlineData(MapTool.Pan)]
	[InlineData(MapTool.Place)]
	[InlineData(MapTool.Select)]
	[InlineData(MapTool.Erase)]
	public void SetActiveTool_NonDrawToolFromIdle_ReportsNoAbortAndLeavesDrawStateIdle(MapTool tool)
	{
		var state = new MapEditorState();
		state.DrawState.Should().Be(DrawState.Idle);

		var change = state.SetActiveTool(tool, drawingSessionIsActive: false);

		change.AbortInProgressDraw.Should().BeFalse();
		state.DrawState.Should().Be(DrawState.Idle);
	}

	[Theory]
	[InlineData(MapTool.Draw, true)]
	[InlineData(MapTool.Pan, true)]
	[InlineData(MapTool.Place, true)]
	[InlineData(MapTool.Move, false)]
	[InlineData(MapTool.Select, false)]
	[InlineData(MapTool.Erase, false)]
	public void SetActiveTool_ReportsClearHoveredAreaForEveryToolButMoveSelectAndErase(
		MapTool tool,
		bool expected)
	{
		var state = new MapEditorState();

		var change = state.SetActiveTool(tool, drawingSessionIsActive: false);

		change.ClearHoveredArea.Should().Be(expected);
	}

	[Theory]
	[InlineData(MapTool.Draw, true)]
	[InlineData(MapTool.Move, true)]
	[InlineData(MapTool.Pan, true)]
	[InlineData(MapTool.Select, true)]
	[InlineData(MapTool.Erase, true)]
	[InlineData(MapTool.Place, false)]
	public void SetActiveTool_ReportsLeavingPlaceToolForEveryToolButPlace(MapTool tool, bool expected)
	{
		var state = new MapEditorState();

		var change = state.SetActiveTool(tool, drawingSessionIsActive: false);

		change.LeavingPlaceTool.Should().Be(expected);
	}

	[Theory]
	[InlineData(MapTool.Draw)]
	[InlineData(MapTool.Move)]
	[InlineData(MapTool.Pan)]
	[InlineData(MapTool.Place)]
	[InlineData(MapTool.Select)]
	[InlineData(MapTool.Erase)]
	public void SetActiveTool_AnyTool_ClearsObjectInteraction(MapTool tool)
	{
		var state = new MapEditorState();
		FillObjectInteraction(state);

		state.SetActiveTool(tool, drawingSessionIsActive: false);

		AssertObjectInteractionIsClear(state);
	}

	[Fact]
	public void SetActiveTool_SwitchingToTheSameToolAgain_StillClearsObjectInteraction()
	{
		var state = new MapEditorState();
		state.SetActiveTool(MapTool.Select, drawingSessionIsActive: false);
		FillObjectInteraction(state);

		state.SetActiveTool(MapTool.Select, drawingSessionIsActive: false);

		AssertObjectInteractionIsClear(state);
	}

	[Fact]
	public void ConfirmObjectDragIfPastThreshold_NoPendingPress_ReturnsFalse()
	{
		var state = new MapEditorState();

		var confirmed = state.ConfirmObjectDragIfPastThreshold(new CanvasPoint(1000f, 1000f));

		confirmed.Should().BeFalse();
		state.IsObjectDragConfirmed.Should().BeFalse();
	}

	[Fact]
	public void ConfirmObjectDragIfPastThreshold_ExactlyAtTheThreshold_ReturnsFalseAndLeavesTheDragUnconfirmed()
	{
		var state = new MapEditorState();
		state.BeginObjectPress(Guid.NewGuid(), new CanvasPoint(0f, 0f));

		var confirmed = state.ConfirmObjectDragIfPastThreshold(
			new CanvasPoint(MapEditorState.ObjectDragThreshold, 0f));

		confirmed.Should().BeFalse();
		state.IsObjectDragConfirmed.Should().BeFalse();
	}

	[Fact]
	public void ConfirmObjectDragIfPastThreshold_JustPastTheThreshold_ReturnsTrueAndConfirmsTheDrag()
	{
		var state = new MapEditorState();
		state.BeginObjectPress(Guid.NewGuid(), new CanvasPoint(0f, 0f));

		var confirmed = state.ConfirmObjectDragIfPastThreshold(
			new CanvasPoint(MapEditorState.ObjectDragThreshold + 0.01f, 0f));

		confirmed.Should().BeTrue();
		state.IsObjectDragConfirmed.Should().BeTrue();
	}

	[Fact]
	public void ConfirmObjectDragIfPastThreshold_WellInsideTheThreshold_ReturnsFalse()
	{
		var state = new MapEditorState();
		state.BeginObjectPress(Guid.NewGuid(), new CanvasPoint(100f, 100f));

		var confirmed = state.ConfirmObjectDragIfPastThreshold(new CanvasPoint(101f, 101f));

		confirmed.Should().BeFalse();
		state.IsObjectDragConfirmed.Should().BeFalse();
	}

	[Fact]
	public void ConfirmObjectDragIfPastThreshold_CalledBackInsideTheThresholdAfterConfirming_StillReturnsTrue()
	{
		var state = new MapEditorState();
		state.BeginObjectPress(Guid.NewGuid(), new CanvasPoint(0f, 0f));
		state.ConfirmObjectDragIfPastThreshold(
			new CanvasPoint(MapEditorState.ObjectDragThreshold + 0.01f, 0f))
			.Should().BeTrue();

		var confirmed = state.ConfirmObjectDragIfPastThreshold(new CanvasPoint(0f, 0f));

		confirmed.Should().BeTrue();
		state.IsObjectDragConfirmed.Should().BeTrue();
	}

	[Fact]
	public void GhostRotation_OnANewState_DefaultsToNoRotation()
	{
		new MapEditorState().GhostRotation.Should().Be(ObjectRotation.None);
	}

	[Fact]
	public void BeginObjectPress_RecordsThePendingClickAndTheAnchorWithTheDragUnconfirmed()
	{
		var state = new MapEditorState();
		var objectId = Guid.NewGuid();

		state.BeginObjectPress(objectId, new CanvasPoint(12f, 34f));

		state.PendingObjectClickId.Should().Be(objectId);
		state.ObjectDragAnchorScreen.Should().Be(new CanvasPoint(12f, 34f));
		state.IsObjectDragConfirmed.Should().BeFalse();
	}

	[Fact]
	public void ResetDrawState_WithAFloorSelected_SetsDrawStateToDrawingReady()
	{
		var state = new MapEditorState();

		state.ResetDrawState(aFloorIsSelected: true);

		state.DrawState.Should().Be(DrawState.DrawingReady);
	}

	[Fact]
	public void ResetDrawState_WithNoFloorSelected_SetsDrawStateToIdle()
	{
		var state = new MapEditorState();
		state.SetActiveTool(MapTool.Draw, drawingSessionIsActive: true);

		state.ResetDrawState(aFloorIsSelected: false);

		state.DrawState.Should().Be(DrawState.Idle);
	}

	[Fact]
	public void ResetDrawState_DuringAMoveDrag_ClearsTheMoveDragState()
	{
		var state = new MapEditorState();
		state.BeginMoveDrag(Guid.NewGuid(), new TilePoint(2, 3));

		state.ResetDrawState(aFloorIsSelected: true);

		state.IsMoveDragging.Should().BeFalse();
		state.DraggingAreaId.Should().BeNull();
		state.MoveGrabTile.Should().BeNull();
	}

	[Fact]
	public void ResetDrawState_WithObjectInteractionInFlight_LeavesEveryObjectInteractionFieldUntouched()
	{
		var state = new MapEditorState();
		var pressedId = Guid.NewGuid();
		state.SelectedObjectId = Guid.NewGuid();
		state.HoveredObjectId = Guid.NewGuid();
		state.BeginObjectPress(pressedId, new CanvasPoint(1f, 2f));
		var selectedId = state.SelectedObjectId;
		var hoveredId = state.HoveredObjectId;

		state.ResetDrawState(aFloorIsSelected: true);

		state.SelectedObjectId.Should().Be(selectedId);
		state.HoveredObjectId.Should().Be(hoveredId);
		state.PendingObjectClickId.Should().Be(pressedId);
		state.ObjectDragAnchorScreen.Should().Be(new CanvasPoint(1f, 2f));
	}

	[Fact]
	public void BeginMoveDrag_SetsTheDraggingFlagTheAreaIdAndTheGrabTileTogether()
	{
		var state = new MapEditorState();
		var areaId = Guid.NewGuid();

		state.BeginMoveDrag(areaId, new TilePoint(4, 7));

		state.IsMoveDragging.Should().BeTrue();
		state.DraggingAreaId.Should().Be(areaId);
		state.MoveGrabTile.Should().Be(new TilePoint(4, 7));
	}

	[Fact]
	public void EndMoveDrag_AfterABeginMoveDrag_ClearsTheDraggingFlagTheAreaIdAndTheGrabTile()
	{
		var state = new MapEditorState();
		state.BeginMoveDrag(Guid.NewGuid(), new TilePoint(4, 7));

		state.EndMoveDrag();

		state.IsMoveDragging.Should().BeFalse();
		state.DraggingAreaId.Should().BeNull();
		state.MoveGrabTile.Should().BeNull();
	}

	[Fact]
	public void EndMoveDrag_WithNoDragInProgress_LeavesTheMoveDragStateClearWithoutThrowing()
	{
		var state = new MapEditorState();

		Action act = state.EndMoveDrag;

		act.Should().NotThrow();
		state.IsMoveDragging.Should().BeFalse();
		state.DraggingAreaId.Should().BeNull();
		state.MoveGrabTile.Should().BeNull();
	}

	[Fact]
	public void ClearObjectInteraction_WithEveryObjectFieldSet_ClearsEachOfThem()
	{
		var state = new MapEditorState();
		FillObjectInteraction(state);

		state.ClearObjectInteraction();

		state.SelectedObjectId.Should().BeNull();
		state.HoveredObjectId.Should().BeNull();
		state.PendingObjectClickId.Should().BeNull();
		state.ObjectDragAnchorScreen.Should().BeNull();
		state.ObjectDragTargetCanvas.Should().BeNull();
		state.PopupBounds.Should().BeNull();
		state.IsObjectDragConfirmed.Should().BeFalse();
		state.ObjectLongPressHandled.Should().BeFalse();
	}

	[Fact]
	public void ClearObjectInteraction_LeavesTheSelectionAndDrawStateAlone()
	{
		var state = new MapEditorState();
		var floorId = Guid.NewGuid();
		var focusedAreaId = Guid.NewGuid();
		state.SelectedFloorId = floorId;
		state.FocusedAreaId = focusedAreaId;
		state.SetActiveTool(MapTool.Draw, drawingSessionIsActive: true);

		state.ClearObjectInteraction();

		state.SelectedFloorId.Should().Be(floorId);
		state.FocusedAreaId.Should().Be(focusedAreaId);
		state.DrawState.Should().Be(DrawState.Drawing);
	}

	// Sets every field ClearObjectInteraction is specified to clear, so a test that clears them
	// starts from a state where each one is observably non-default.
	private static void FillObjectInteraction(MapEditorState state)
	{
		state.SelectedObjectId = Guid.NewGuid();
		state.HoveredObjectId = Guid.NewGuid();
		state.ObjectDragTargetCanvas = new CanvasPoint(7f, 8f);
		state.PopupBounds = new CanvasRect(1f, 2f, 3f, 4f);
		state.ObjectLongPressHandled = true;
		state.BeginObjectPress(Guid.NewGuid(), new CanvasPoint(0f, 0f));
		state.ConfirmObjectDragIfPastThreshold(
			new CanvasPoint(MapEditorState.ObjectDragThreshold + 1f, 0f));
	}

	private static void AssertObjectInteractionIsClear(MapEditorState state)
	{
		state.SelectedObjectId.Should().BeNull();
		state.HoveredObjectId.Should().BeNull();
		state.PendingObjectClickId.Should().BeNull();
		state.ObjectDragAnchorScreen.Should().BeNull();
		state.ObjectDragTargetCanvas.Should().BeNull();
		state.PopupBounds.Should().BeNull();
		state.IsObjectDragConfirmed.Should().BeFalse();
		state.ObjectLongPressHandled.Should().BeFalse();
	}
}
