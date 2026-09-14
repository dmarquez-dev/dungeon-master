# Contract C1 — the floor reorder path

Frozen at Gate 2. **AMENDED 2026-09-11 after freeze, by the user's resolution of escalation E6** — the only authority that may change a frozen contract. Amended passages are marked in place. Applies to `T1`, `T2`, `T3`, `T5`, `T6`.

`T1` diagnoses and repairs this path. `T2` and `T3` edit the same two files afterwards and must not
break what `T1` established. `T5` asserts against it from out of process. `T6` describes it in the
audit record. This file is the only thing those four tasks share about it.

Everything below is stated as it stands at `b3b1641`. `T1` **may change any of it**, because the
defect may live in any of it — but a change to a line marked **FROZEN** is a contract change and
requires an escalation, and a change to a line marked **T1-MUTABLE** must be reported by `T1` in its
report so `T2`, `T3` and `T5` are dispatched against the new value.

## 1. The drag payload — FROZEN

The drag data package carries exactly one property.

| Key | Type as stored | Value |
| --- | --- | --- |
| `"FloorId"` | `string` | `Floor.Id.ToString()` — the **dragged** floor's id, invariant `Guid` "D" format |

Written at `FloorSidebarItem.OnDragStarting` (`FloorSidebarItem.xaml.cs:373-381`). Read at both drop
sites (`FloorSidebarItem.OnDrop`, `FloorSidebarItem.xaml.cs:403`; the placeholder's `Drop` closure,
`DungeonDetailPage.xaml.cs:2260`).

**Error cases, and they are the part most often guessed differently:**

| Condition | Required behaviour |
| --- | --- |
| Key `"FloorId"` absent from `e.Data.Properties` | Return without reordering. No error message. |
| Value is not a `string`, or does not `Guid.TryParse` | Return without reordering. No error message. |
| Parsed id equals the **target** floor's id (drop on self) | Return without reordering. No error message. |
| Target `FloorSidebarItem.Floor` is `null` | Return without reordering. No error message. |
| `ReorderFloorCommand` throws | `_viewModel.ErrorMessage` is set to `$"Could not reorder the floors: {exception.Message}"`. The exception does not escape the `async void` handler. |

The four silent returns are silent **on purpose**: they are not failures, they are a drop that means
nothing. The fifth is the only user-visible failure and a test in `DungeonMaster.Tests` already
enforces that the view model surfaces it (refuted candidate #3 in `measurement.md` §1/§5).

## 2. The command invocation — FROZEN

```csharp
await _viewModel.FloorList.ReorderFloorCommand.ExecuteAsync(
    new ReorderFloorArgs(draggedFloorId, targetFloorNumber));
```

- `draggedFloorId` is the `Guid` parsed from the payload.
- `targetFloorNumber` is the **target** floor's `Number` (1-based), not its index in
  `FloorListContainer.Children` and not its index in the source collection.
- `_editor.SelectedFloorId` is set to `draggedFloorId` **before** the await.
- There are exactly two production call sites, `DungeonDetailPage.xaml.cs:860` and `:2290`. A task
  that adds a third must say so in its report.

## 3. The element that starts the drag — T1-MUTABLE

At `b3b1641` the `DragGestureRecognizer` is on the drag-handle `Label`
(`FloorSidebarItem.xaml:164-179`) and nowhere else, so that handle is the only reorder affordance in
the source. `OnKeyDown` (`DungeonDetailPage.xaml.cs:361-430`) has no reorder key.

`T3` moves that handle's position in the row. `T3` **must not** move, remove, or re-host the
`DragGestureRecognizer`, and must not change its `DragStarting`/`DropCompleted` handler names.
Moving the handle out of the scrollbar gutter is a layout change only.

## 4. The drop targets — T1-MUTABLE

**THREE** distinct drop targets exist and all three reorder. **AMENDED AT ESCALATION E8** — it was
two at `b3b1641`; `T3` added the third, which is the fix for the silent discard described below:

1. The card's root `Border` — `DropGestureRecognizer DragOver="OnDragOver" DragLeave="OnDragLeave"
   Drop="OnDrop"` (`FloorSidebarItem.xaml`, inside `Border.GestureRecognizers`).
2. The transient placeholder `Border` built in `ShowDragPlaceholder`
   (`DungeonDetailPage.xaml.cs:2203-2310`), which carries its own `DropGestureRecognizer` with an
   inline `DragOver` and `Drop`.
3. **`FloorListContainer` itself** (`DungeonDetailPage.xaml:668`) — added by `T3` under escalations
   E6/E8. It catches every release that targets 1 and 2 miss, and resolves the release to the nearest
   floor card by **vertical midpoint**, in a pure function in `App.Core/Editor/FloorReorder*.cs` that
   reuses `FloorReorderDrop.Decide` and the existing `ReorderFloorCommand` call shape. Because the
   resolution is a pure function, this target — unlike 1 and 2 — is reachable by an in-process test.

`DataPackageOperation.Copy` is the accepted operation at all three. It is the **only** non-`None`
member MAUI's `DataPackageOperation` enum has; `.Move` does not exist there. Refuted candidate: a task
must not "fix" this by setting `.Move`.

`T2` and `T3` must leave all three drop targets wired. If `T1` removes or merges one, `T1` says so in
its report and the surviving wiring becomes the frozen shape for `T2`, `T3` and `T5`. **`T5` and `T6`
are dispatched against the count THREE**; a task reporting two is reading a pre-E8 copy.

**The gap between them, and it is the session's actual defect (escalation E6).** `FloorListContainer`
— `<VerticalStackLayout x:Name="FloorListContainer" Spacing="0" />` at `DungeonDetailPage.xaml:668` —
carries **no `DropGestureRecognizer`**, and that file declares none at all. Every pixel of the floor
list that is not a card and not the transient placeholder is therefore dead to a drop: the release is
discarded with no handler, no error and no trace. **`T3` closed this by taking shape (a)** — the
third drop target listed above — chosen over surfacing the miss because a near-miss that still
reorders is what the user was reaching for, and a pure resolution function is testable where a gesture
is not. The count in this section is therefore settled at **three**.

**Escalation E8, recorded because it was a decomposition error, not a discovery.** `T3`'s criterion
originally asserted that owning `DungeonDetailPage.xaml` sufficed. It did not: a XAML event handler
resolves against `DungeonDetailPage.xaml.cs`, which was `T4`'s file, and the command-bound alternative
needs `DungeonDetailViewModel`, which no task owned. `T3` **escalated rather than writing outside its
set**, which is the behaviour this contract exists to produce. The user granted `T3`
`DungeonDetailPage.xaml.cs` and `App.Core/Editor/FloorReorder*.cs`; both intersections are ordered by
a direct `dependsOn` edge.

## 4a. AMENDED at escalation E6 — the reorder is NOT broken

Everything in §5 below was written before the orchestrator ran `T1`'s observation protocol with the
user. **It has now been run, and the reorder works end to end.** At 1920x1080 maximized, all 11 floors
visible and **no scrollbar rendered**: the source card vanished at drag start, the target showed the
gold `DragOver` stroke, a placeholder gap appeared, the order changed to the expected arrangement, no
error line appeared, and the database confirmed the move persisted (`Guest Rooms` 3 → 7). Transcript:
`observations/T1-reorder.md`.

Read against `T1`'s own discriminator table, (a)+(b)+(c) all YES eliminates candidates A and D and
leaves B or C — but **both of those predict no reorder, and the reorder happened correctly**. So none
of the four candidates is the mechanism, because **in that configuration there is no defect**.

The failing observation in `measurement.md` §2 was taken in a **non-maximized, scrolling** window. The
difference is the scrollbar. So:

- **The feature works; what fails is reaching it.** That is `MASTER-163`, not a new peer defect.
- **`MASTER-163`'s severity is corrected UPWARD, not downward** — a missed drop fails silently, which
  is worse than awkward ergonomics. Contract C4 §3 correction 3 carries the amended text.
- **`MASTER-174` is not "the reorder does not reorder".** It is the silent discard. Contract C4 §1.
- **`T1`'s work stands**: a behaviour-preserving refactor plus the first tests ever to exist on the
  gesture-to-command path, having correctly declined to ship a speculative fix. **No task may write or
  rely on a claim that `T1` fixed a reorder defect.**

§5 is retained below as the pre-E6 record of what was suspected and why, not as live guidance.

## 5. The suspected mechanism — SUPERSEDED by §4a; retained as the pre-E6 record

`ShowDragPlaceholder` is invoked **from inside** `FloorSidebarItem.OnDragOver`
(`FloorSidebarItem.xaml.cs:385-392` raises `PlaceholderRequested`; `DungeonDetailPage.xaml.cs:883`
handles it), and it mutates `FloorListContainer.Children` — `Remove` at `:2238`, `Insert` at `:2309`.
Mutating the visual tree under the pointer from within a `DragOver` handler is candidate #1 for why
`Drop` never fires.

**This is a candidate, not a diagnosis.** `T1` owns the diagnosis and is not bound to this
explanation. Three other theories are already refuted with evidence in `measurement.md` §1 and §5 and
must not be re-opened without new evidence:

- `DataPackageOperation.Copy` being wrong — refuted, it is the only non-`None` value available.
- The repaint after reorder being conditional — refuted, the repaint path is unconditional.
- The view model swallowing the failure — refuted, it surfaces failures and a test enforces it.

## 6. What has no coverage at all — stated so no task claims otherwise

At `b3b1641` **nothing** between the gesture and the command is covered by any test: no test names
`OnDragStarting`, `OnDragOver`, `OnDrop`, `ShowDragPlaceholder` or `ClearDragPlaceholder`. Any task
report claiming an existing test guards this path is wrong.

**Amended at E6:** `T1` has since added the first tests on that path. A task dispatched after `T1`
reads the count from `T1`'s report, not from this line, and states the command it read it with.
