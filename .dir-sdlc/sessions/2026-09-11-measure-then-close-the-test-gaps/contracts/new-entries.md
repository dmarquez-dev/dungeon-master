# Contract — the four new master-index entries this session files

Frozen at Gate 2. **Consumed by T5**, which is the only task that writes
`docs/audit/00-master-index.md`. No other task files anything.

Authority: escalation **E2**, resolved — P1, P2 and P3 are *filed*, not fixed. No task in this
session changes Domain production behaviour.

## Reserved identifiers

`MASTER-167` is the highest identifier in `docs/audit/00-master-index.md` at `1407090`. The
four below are appended at ranks 168-171, on the same rule the index already states for
`MASTER-159`-`167`: ranks are append-only because renumbering would break every citation the
file has ever carried. None has a source record; `Sources` is `Session 6b-iii` and the
evidence is this session's `measurement.md`.

| ID | Sev | Layers | Category | Effort | Risk | Title |
|---|---|---|---|---|---|---|
| MASTER-168 | S3 | CORE | correctness | S | low | Area.SetOutline stores a degenerate or null outline with no segment-count and no null check |
| MASTER-169 | S3 | CORE | correctness | XS | low | Floor accepts a zero or negative floor number in its constructor and in UpdateNumber |
| MASTER-170 | S3 | CORE | correctness | XS | low | PathSegment permits a CubicBezier with null control points, contradicting its own doc comment |
| MASTER-171 | S4 | CORE | naming | XS | low | RotateDirection.cs declares a type named RotationDirection |

Severity and effort above are this session's judgment, not a measured value; T5 records them
as judgment.

## MASTER-168 — P1

`src/DungeonMaster.Domain/Entities/Area/AreaMethods.cs:38-44` does `Clear()` then `AddRange()`
with no segment-count check and no null check, so a 0-, 1- or 2-segment "polygon" is stored
silently. The aggregate is protected only by a caller it does not control: the Application
validator rejects empty and null
(`src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateAreaOutlineCommandValidatorTests.cs:89`).

**Why it is filed rather than fixed.** `TEST-020`'s clause (d) reads as a missing test; it is a
missing production invariant, and a test cannot assert behaviour that does not exist. Deciding
where the rule lives — aggregate or validator — is a design decision outside a coverage
session.

## MASTER-169 — P2

`Floor..ctor` and `Floor.UpdateNumber` accept any `int`, including `0` and negatives
(`src/DungeonMaster.Domain/Entities/Floor/Floor.cs:27`). `Dungeon.AddFloor` and
`Dungeon.RemoveFloor` only ever pass `_floors.Count + 1` and `i + 1`, so nothing in the
aggregate reaches the unguarded values today; the type permits them.

## MASTER-170 — P3

`src/DungeonMaster.Domain/ValueObjects/PathSegment.cs:18-22` permits a `CubicBezier` with null
control points, contradicting the doc comment on the same type.

## MASTER-171 — P4, **restated**: the measurement's claim for this row is falsified

The re-measurement recorded P4 as *"`RotateDirection` is dead — zero references in `src/`
outside its declaring file."* **That is false, and T5 must record it as false rather than
repeat it.** The zero came from searching the *file* name; the *type* is named differently.

Measured in the session worktree at `1407090`:

```
grep -rn "RotationDirection" src/
```

-> **6 lines in 3 files**. One is the declaration
(`src/DungeonMaster.Domain/Entities/Floor/RotateDirection.cs:3`). The other **five** are live
references in two files under `src/DungeonMaster.App/Views/DungeonDetailPage/`:
`ControlsPaletteView.xaml.cs:8`, `:14`, `:15`, and `DungeonDetailPage.xaml.cs:139`, `:150`.

```
grep -rn "RotateDirection" src/
```

-> **0 lines**. No type, member or identifier by that name exists anywhere in `src/`.

So the real, previously unrecorded finding is a **naming defect, not dead code**: the file
`RotateDirection.cs` declares `public enum RotationDirection`, so the file name and the type it
contains disagree, and a search on the file's name — which is what the re-measurement did —
returns nothing and reads as "unused". The type is used.

T5 files `MASTER-171` on that corrected basis, and records in the same place that the P4 row of
this session's `measurement.md` was falsified during decomposition, with both commands above.
This is the same class of error the index already warns about at
`docs/audit/00-master-index.md:522`, caught one layer further in.

## The two MASTER-020 residuals — MASTER-172 and MASTER-173

The re-measurement found two gaps while establishing that `MASTER-020` is closed, and recorded
them as **explicitly not part of it, not to be smuggled into it**. Closing `MASTER-020` without
filing them would lose them, so they are filed. They are not a reason to hold `MASTER-020`
open and they are not this session's code work.

| ID | Sev | Layers | Category | Effort | Risk | Title |
|---|---|---|---|---|---|---|
| MASTER-172 | S3 | TEST | async | XS | low | No test asserts that Dispatcher propagates its CancellationToken to the handler it resolves |
| MASTER-173 | S3 | TEST | testability | XS | low | Dispatcher's missing-handler failure mode is untested |

Source: `Session 6b-iii`. Evidence: this session's `measurement.md`, "Residuals found but **not**
part of MASTER-020". Severity and effort are this session's judgment.

`MASTER-172` is adjacent to `MASTER-070` but is **not** covered by it: T1 proves forwarding at
the handler and repository boundaries, not at the dispatcher. T5 states that distinction
where it closes `MASTER-070`, so a later reader does not take one for the other.
