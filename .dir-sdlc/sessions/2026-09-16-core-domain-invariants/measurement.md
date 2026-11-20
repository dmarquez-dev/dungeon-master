# Measurement — session 6b-vii

**Fixed commit: `dff976d59a7bb08d4074d31dd25bd5286461e2e8` (`dff976d`).** Every figure below was produced by a
command run in this session at that commit, and each is stated with the command that produced it. Nothing is
carried from the handoff or from the entry prose; where the two disagree, this file is the record.

---

## 1. The survey, re-derived

Command: the Python classifier from `HANDOFF-session-6b-vii.md` §6, run unmodified at `dff976d`.

```
total entry rows: 176
   120  closed
    53  open
     3  part done

by layer:   CORE 20, UI 9, UIKIT 9, TEST 8, INFRA 6, "UI, XCUT" 1
by severity: S3 12, S4 31, S5 10
open S2:    NONE
part done:  :282 MASTER-094, :286 MASTER-098, :296 MASTER-108
```

No row classified `UNCLASSIFIED`. This matches the handoff's §2 exactly, including the zero open S2s, so
`MASTER-176`'s closure marker from 6b-vi is still well-formed.

---

## 2. The three entries, re-measured against the code

The index rows are `00-master-index.md:356`, `:357`, `:358`; all three have **no source record**, and their
shared rationale is the prose block at `00-master-index.md:374`. Read that block, not the row titles — the
rows are one line each and omit every qualification.

### MASTER-168 — `Area.SetOutline`

Command: `sed -n '25,60p' src/DungeonMaster.Domain/Entities/Areas/AreaMethods.cs`

`SetOutline` is declared at `AreaMethods.cs:38` and its body is `_outline.Clear(); _outline.AddRange(outline);
Touch();` — **confirmed: no null check and no segment-count check.** The entry's claim holds verbatim.

Callers, from `grep -rn "SetOutline" src --include=*.cs` with `/obj/` and `/bin/` excluded — **3 hits, and
only one is a caller**: the declaration at `AreaMethods.cs:38`, `FloorMethods.cs:161` inside
`UpdateAreaOutline`, and a test name at `AreaOutlineTests.cs:62`.

**Reachability, traced end to end — the guard cannot break a live path.** The Application validator
(`UpdateAreaOutlineCommandValidator.cs`) applies `.NotNull().NotEmpty()` to `command.Outline` and **no rule on
segment count**. `AddAreaCommand` carries **no outline parameter at all** (`AddAreaCommand.cs`, four
properties: `DungeonId`, `FloorId`, `Name`, `Description`), and `AddAreaCommandValidator` constrains only
`Name`. That leaves the segment count ungoverned between the UI and `_outline.AddRange`, so the reachability
question has to be settled at the **producer**, not the validator.

`grep -rn "UpdateAreaOutlineCommand(\|BuildOutline()" src --include=*.cs`, source files only, gives **two
production sites**, both in `MapCanvasViewModel.cs`:

- `:219` in `AddAreaWithOutlineAsync`, fed from `DungeonDetailPage.xaml.cs:1775`
  (`_viewModel.Canvas.DrawingSession.BuildOutline()`). `MapDrawingSession.BuildOutline` (`:111-127`) returns
  `[]` when `_vertices.Count < 3`, and otherwise emits one segment per vertex from index 1 plus a closing
  segment back to `_vertices[0]` — **0 or ≥3 segments, never 1 or 2**. `AddVertex` (`:68`) only closes the
  polygon at `Count >= 3`.
- `:257` in `MoveAreaOutlineAsync`, which sends a **translated** existing outline. A translation preserves the
  segment count, so it cannot shrink one below 3.

**So the degenerate case is not reachable either, and the record at `:374` is right that the whole entry is
"a latent invariant gap rather than a live defect".** This was measured rather than assumed, and it is the
load-bearing fact for the risk section of the spec: adding the guard **cannot change the behaviour of any
path a user can drive today**, and no App-layer work is entailed. A first pass of this measurement claimed
the degenerate case was reachable, reasoning from the validator alone; tracing the producer refuted it.

**Persistence does not go through `SetOutline`.** `AreaConfiguration.cs:82-97` maps `Area.Outline` with
`.HasField("_outline")` and `PropertyAccessMode.PreferFieldDuringConstruction`, so EF materialises into the
backing field directly. A guard added to `SetOutline` cannot affect loading existing data.

### MASTER-169 — `Floor` number

Command: `sed -n '1,70p' src/DungeonMaster.Domain/Entities/Floors/Floor.cs` and
`sed -n '1,30p' src/DungeonMaster.Domain/Entities/Floors/FloorMethods.cs`

- `Floor..ctor` at `Floor.cs:27-34` assigns `Number = number` with **no guard**. Confirmed.
- `FloorMethods.UpdateNumber` at `FloorMethods.cs:13` is `Number = number; Touch();` — **no guard**. Confirmed.

Both are `internal`, so neither has a `MutationOutcome` return channel; the aggregate's public edge
(`Dungeon.ReorderFloor`) already returns `MutationOutcome.OutOfRange`.

Callers, from `grep -rn "UpdateNumber" src/DungeonMaster.Domain src/DungeonMaster.Application`, source files
only — **4 call sites, all in `DungeonMethods.cs`**: `:83` `sorted[i].UpdateNumber(i + 1)` in `RemoveFloor`,
`:132` `shifted.UpdateNumber(shifted.Number + 1)` and `:140` `shifted.UpdateNumber(shifted.Number - 1)` in
`ReorderFloor`, and `:144` `floor.UpdateNumber(newNumber)`.

**A `< 1` guard is verified safe against every one of them**, read from `DungeonMethods.cs:60-150`:

- `:83` passes `i + 1`, minimum `1`.
- `:144` is reached only after `if (newNumber < 1 || newNumber > _floors.Count) return OutOfRange;`.
- `:140`'s loop filters `floor.Number > oldNumber`, so the decremented value is at least `oldNumber`, and
  `oldNumber` is an existing floor number.
- `:132` only increments.

So the entry's "nothing in the aggregate reaches the unguarded values today" is confirmed by reading the
filters, not assumed.

### MASTER-170 — `PathSegment`

Command: `cat src/DungeonMaster.Domain/ValueObjects/PathSegment.cs`

The file is 22 lines. The type is a positional record `PathSegment(PathSegmentKind Kind, TilePoint End,
TilePoint? Ctrl1, TilePoint? Ctrl2)` with **no body and no validation**, and its own doc comment on `Ctrl1`
and `Ctrl2` reads *"used only when `PathSegmentKind.CubicBezier` is specified."* **Confirmed:** a
`CubicBezier` with null control points is constructible, and so is a `Line` carrying them.

Construction sites. **The first census here was wrong, and `dir-architect` caught it during decomposition.**

- `grep -rn "new PathSegment(" src --include=*.cs`, excluding `/obj/` and `/bin/` — **26 hits**.
- `grep -rn "new PathSegment(\|new(PathSegmentKind\." src --include=*.cs`, same exclusions — **42 hits**.

The first form misses the **target-typed `new(...)`** used inside collection expressions, which is 16 of the
42 sites. Both counts above were re-run by the orchestrator at `dff976d`; the 42-hit form is the census.

The sole **production** construction site is still `MapDrawingSession.cs:122` and `:125`, both
`PathSegmentKind.Line` with `null, null`, so the decision's persisted-data safety argument survives intact:
no `CubicBezier` has ever been written to the database.

**What the miss concealed.** Among the 16 missed sites is
`src/DungeonMaster.Tests/AppCore/Map/AreaOutlineGeometryTests.cs:63` —
`new(PathSegmentKind.CubicBezier, new TilePoint(0, 0), new TilePoint(1, 1), null)` — a `CubicBezier` with
`Ctrl2` null, which **decision 3 makes illegal**. Its test is
`Translate_SegmentWithOnlyCtrl2Null_OffsetsCtrl1AndLeavesCtrl2Null` at `:59`, and its entire premise is that a
half-specified `CubicBezier` can exist. That premise is what this session retires, so the test is retired with
it rather than repaired.

This is the third session in which a confident `grep` form produced a wrong number that a second command
caught. The rule it re-earns: **a census over a C# type must cover the target-typed `new(...)` form**, because
a collection expression never spells the type.

**The consequence that decides where the guard goes.** `AreaConfiguration.cs:82-97` persists the outline as
`TEXT` via `JsonSerializer.Serialize` / `JsonSerializer.Deserialize<List<PathSegment>>`. `System.Text.Json`
constructs a positional record **through its constructor**, so any validation placed in that constructor runs
on **every read of every area outline from the database**, not only on new construction. The risk that
creates is bounded by the paragraph above: production has only ever written `Line` segments with null control
points, so no persisted row can violate a bidirectional rule.

---

## 3. Domain guard precedent

Command: `grep -rn "ArgumentException.ThrowIfNullOrWhiteSpace\|ArgumentNullException.ThrowIfNull\|ArgumentOutOfRangeException" src/DungeonMaster.Domain --include=*.cs`, source files only — **6 hits**.

| Site | Form |
|---|---|
| `Area.cs:31`, `AreaMethods.cs:24`, `Dungeon.cs:25`, `DungeonMethods.cs:24` | `ArgumentException.ThrowIfNullOrWhiteSpace(name);` |
| `PlacedObject.cs:30` (ctor), `PlacedObject.cs:82` (`Update`) | `throw new ArgumentOutOfRangeException(nameof(x), x, "...")` guarding a numeric range |

`ArgumentNullException.ThrowIfNull` appears **nowhere** in the Domain project. `PlacedObject` is the direct
precedent for this session: an out-of-range numeric rejected by throwing from an `internal` member, guarded
identically in both the constructor and the updater — which is the shape `MASTER-169` needs.

---

## 4. Decisions taken at Gate 1

Three escalations, batched into one ask, resolved before decomposition. Recorded here because the
implementers must build to the decision and not re-derive it.

| Entry | Decision |
|---|---|
| `MASTER-168` | `SetOutline` rejects **null** and **fewer than 3 segments**. Empty is included in the rejection; no `ClearOutline` affordance is added. |
| `MASTER-169` | **Throw `ArgumentOutOfRangeException` when `number < 1`, in both `Floor..ctor` and `UpdateNumber`**, matching `PlacedObject`. The aggregate keeps returning `MutationOutcome.OutOfRange` at its public edge; a `FloorNumber` value object was considered and rejected as a larger session. |
| `MASTER-170` | Validate **in the record constructor, both directions**: `CubicBezier` requires both control points non-null, `Line` requires both null. Accepted consequence, stated at the gate: a corrupt or hand-edited row now throws on load rather than rendering silently wrong. |

---

## 5. Standing environment facts carried into every dispatch

From the handoff §7, not re-measured here except where noted:

- Tracked files are **CRLF** and `IDE0055` is a **build error**. `sed -i` rewrites a tracked file to LF and
  fails the build on every line. Agents must edit through the edit tools.
- `grep -c $'\r$'` returns a confident `0` on a correctly-CRLF file in this environment. Count in binary.
- Tasks share one worktree and one `obj/`/`bin/`. Two building tasks must be serialised.
- Close any running `DungeonMaster.App` before building the Windows TFM, or `MSB3026`/`MSB3027`.
- Verify in the main checkout after the merge, not only in the worktree.
