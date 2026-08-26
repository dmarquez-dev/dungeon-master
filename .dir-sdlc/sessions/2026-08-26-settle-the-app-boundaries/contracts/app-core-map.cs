// Contract — DungeonMaster.App.Core.Map
//
// Frozen at Gate 2. Session 2026-08-26-settle-the-app-boundaries (sub-session 5a).
// Authored by T2. Consumed by T3, by DungeonMaster.App, by DungeonMaster.Tests,
// and by sub-sessions 5b and 5c.
//
// This file is a CONTRACT, not source. It declares the public surface the moved types must have.
// It is not compiled and must not be copied into the tree as-is: the real files carry the XML doc
// comments they already have, and the members below are written signature-only for brevity.
//
// ---------------------------------------------------------------------------------------------
// WHAT THIS CLOSES, AND THE CONFLICT IT RESOLVES
// ---------------------------------------------------------------------------------------------
//
// MASTER-082 / CORE-020: TileCoordinateHelper, SnapZoneCalculator and MapDrawingSession have no
// caller inside DungeonMaster.Application. Verified in this session: `grep -rn "Common.Map" src`
// over the worktree finds their only production callers in DungeonMaster.App, in three files —
// Graphics/FloorMapDrawable.cs, ViewModels/DungeonDetailViewModel.cs and
// Views/DungeonDetailPage/DungeonDetailPage.xaml.cs.
//
// One of the three has no production caller in App either: SnapZoneCalculator's only callers
// anywhere are its own tests. It is therefore deleted rather than moved — see DELETED AT GATE 2
// below for the counts and the reasoning.
//
// MASTER-027 / UIKIT-005 suggests moving the feet-per-tile arithmetic "to Application alongside the
// other map helpers". That CONFLICTS with MASTER-082, which moves those helpers out of Application.
// The conflict is resolved in favour of MASTER-082: the helpers leave Application, and the
// feet-per-tile arithmetic follows them here rather than going to Application. Nothing in this
// session adds a type to DungeonMaster.Application.
//
// ---------------------------------------------------------------------------------------------
// THE MOVE IS BEHAVIOUR-PRESERVING
// ---------------------------------------------------------------------------------------------
//
// TileCoordinateHelper, MapDrawingSession and DrawSessionResult move from
//   src/DungeonMaster.Application/Common/Map/
// to
//   src/DungeonMaster.App.Core/Map/
// with their bodies, their XML doc comments and their observable behaviour UNCHANGED, save for the
// one member deletion recorded under DELETED AT GATE 2 below. The namespace changes from
// DungeonMaster.Application.Common.Map to DungeonMaster.App.Core.Map, and nothing else does. No
// argument validation is added, no signature is widened or narrowed, no member is renamed.
//
// Argument-error behaviour is therefore also unchanged, and this is deliberate rather than an
// oversight. None of these methods validates its arguments today:
//   - A tileSize of 0f in SnapToGrid or ToPixel divides by zero in float arithmetic,
//     yielding +/-Infinity or NaN, whose conversion to int is unspecified. No exception is thrown.
//   - A negative tileSize is accepted and produces mirrored coordinates.
//   - MapDrawingSession has no upper bound on vertex count and is not thread-safe.
// A task that "fixes" any of these has changed behaviour this contract freezes as unchanged.
//
// src/DungeonMaster.Application/Common/Map/ must not exist after the move. The directory is
// deleted, not emptied.
//
// ---------------------------------------------------------------------------------------------

namespace DungeonMaster.App.Core.Map;

using DungeonMaster.Domain.ValueObjects;

/// Moved verbatim from DungeonMaster.Application.Common.Map.TileCoordinateHelper.
public static class TileCoordinateHelper
{
	public static TilePoint SnapToGrid(float x, float y, float tileSize);

	public static (float x, float y) ToPixel(TilePoint tile, float tileSize);
}

// ---------------------------------------------------------------------------------------------
// DELETED AT GATE 2 — DO NOT RE-ADD
// ---------------------------------------------------------------------------------------------
//
// Two operations that an earlier draft of this contract declared are NOT part of the surface. The
// user decided this at Gate 2, on coverage gaps this contract's own coverage table reported as
// having an implementer and no consumer. They are recorded here, with their evidence, so that a
// later reader does not "restore" them on the assumption they were dropped by accident.
//
// 1. SnapZoneCalculator — THE WHOLE TYPE, not just the member.
//
//    `grep -rn "SnapZoneCalculator" src --include=*.cs | grep -v "/bin/" | grep -v "/obj/"`, run in
//    the worktree on 2026-08-26, returned SEVEN lines across TWO files: the type declaration at
//    src/DungeonMaster.Application/Common/Map/SnapZoneCalculator.cs:8, and six lines in
//    src/DungeonMaster.Tests/Application/Common/Map/SnapZoneCalculatorTests.cs — the test class
//    declaration at :7 and five ToTile call sites at :15, :26, :38, :51 and :63. Production
//    callers: ZERO.
//
//    ToTile is SnapZoneCalculator's only member — the file declares one static method and nothing
//    else — so deleting the member deletes the type, and the type goes. SnapZoneCalculatorTests.cs
//    goes with it: every test in it calls the deleted method, and relocating the tests of a deleted
//    type is the same mistake as relocating the dead code. That removes five executed tests
//    (`grep -c -E "\[Fact\]|\[Theory\]"` reported 5, `grep -c InlineData` reported 0).
//
//    This is what MASTER-082 asks for. MASTER-082's finding is that these geometry types have no
//    caller in the Application layer; a type with no caller ANYWHERE is not answered by giving it a
//    new address in App.Core. If a future session needs canvas-to-tile conversion, note that
//    TileCoordinateHelper.SnapToGrid already performs it and survives here.
//
// 2. MapDrawingSession.LastVertex — the member only. The type survives.
//
//    `grep -rn "[^o]LastVertex\|^LastVertex" src --include=*.cs --include=*.xaml | grep -v "/bin/" |
//    grep -v "/obj/" | grep -v "UndoLastVertex"`, run in the worktree on 2026-08-26, returned
//    exactly TWO lines: the declaration at
//    src/DungeonMaster.Application/Common/Map/MapDrawingSession.cs:49 and ONE test assertion at
//    src/DungeonMaster.Tests/Application/Common/Map/MapDrawingSessionTests.cs:72. Production
//    callers: ZERO.
//
//    The one caller is a test, so the deletion is not free. That assertion —
//    `session.LastVertex.Should().Be(new TilePoint(0, 0));` inside
//    UndoLastVertex_RemovesLast_ReturnsTrue — is rewritten to read the same fact through the public
//    Vertices property that survives: `session.Vertices[^1].Should().Be(new TilePoint(0, 0));`. The
//    test keeps its name, its other assertions and its [Fact] attribute.
//
//    UndoLastVertex is a DIFFERENT member with a real production caller
//    (DungeonDetailPage.xaml.cs:1256) and is NOT deleted. The two names are one character apart and
//    a careless grep conflates them; the command above excludes UndoLastVertex deliberately.
//
// ---------------------------------------------------------------------------------------------

/// Moved verbatim from DungeonMaster.Application.Common.Map.DrawSessionResult.
/// Member order and names are unchanged; these values are compared by name at four call sites in
/// DungeonDetailPage.xaml.cs.
public enum DrawSessionResult
{
	Added,
	Closed,
	Ignored,
}

/// Moved verbatim from DungeonMaster.Application.Common.Map.MapDrawingSession.
/// Still mutable, still not thread-safe, still held as a get-only property by DungeonDetailViewModel.
/// Deciding whether this belongs to the view model or to a command is CORE-020's open half and is
/// NOT this session's work — see the note at the foot of this file.
public sealed class MapDrawingSession
{
	public IReadOnlyList<TilePoint> Vertices { get; }

	public bool IsActive { get; }

	// LastVertex is deliberately absent — deleted at Gate 2. See DELETED AT GATE 2 above.

	public DrawSessionResult PlaceVertex(TilePoint point);

	public bool UndoLastVertex();

	public void Reset();

	public IReadOnlyList<PathSegment> BuildOutline();
}

// ---------------------------------------------------------------------------------------------
// NEW TYPE — the feet-per-tile arithmetic extracted from the renderer
// ---------------------------------------------------------------------------------------------
//
// This is the one behaviour that MOVES rather than being copied. It is currently inline in
// src/DungeonMaster.App/Graphics/FloorMapDrawable.cs at lines 553-558 (verified in this session by
// reading the file; the audit record UIKIT-005 cites 519-524, which is stale after Session 4's
// edits to that file). Extracting it is MASTER-027's "the map drawable further mixes a domain
// calculation into a render method".
//
// The extraction must be arithmetically identical, including the rounding mode. MathF.Round uses
// banker's rounding (to-even), so 2.5f rounds to 2 and 3.5f rounds to 4. A switch to
// MidpointRounding.AwayFromZero would change the label a user sees and fails this contract.
//
// The `if (distTiles > 0f)` guard at FloorMapDrawable.cs:556 is a RENDERING decision — whether to
// draw a label at all — and stays in the drawable. It does not move into this type.

/// New. Extracted from FloorMapDrawable, not moved from Application.
public static class TileDistance
{
	/// Returns the whole-foot distance between two tile-grid points.
	///
	/// Required implementation, exactly:
	///     var dx = to.Col - from.Col;
	///     var dy = to.Row - from.Row;
	///     var distanceInTiles = MathF.Sqrt(dx * dx + dy * dy);
	///     return (int)MathF.Round(distanceInTiles * feetPerTile);
	///
	/// No argument validation, consistent with every other type in this namespace. A negative or
	/// zero feetPerTile is accepted and returns a negative or zero result; a feetPerTile of NaN
	/// returns an unspecified int. Callers today pass FloorMapDrawable.TileSizeInFeet, which
	/// defaults to 5f (FloorMapDrawable.cs:178).
	public static int InFeet(TilePoint from, TilePoint to, float feetPerTile);
}

// ---------------------------------------------------------------------------------------------
// EXPLICITLY OUT OF SCOPE FOR THIS SESSION
// ---------------------------------------------------------------------------------------------
//
// CORE-020's second half — "decide explicitly whether MapDrawingSession is application state (in
// which case a command should consume it) or view state (in which case it belongs to the view
// model)" — is NOT decided here. MASTER-082's title and the entry this session carries is only
// "Common/Map holds three geometry types that no command, query or handler in the layer uses", and
// moving the type out of Application answers that. Where its ownership settles is the editor-seam
// question that sub-session 5b takes. Nothing in this session may relocate DrawingSession off
// DungeonDetailViewModel or introduce a command that consumes it.
