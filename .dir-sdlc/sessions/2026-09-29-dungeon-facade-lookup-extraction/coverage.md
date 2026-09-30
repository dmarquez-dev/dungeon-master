# Coverage mapping — session 6b-xiv

Written at Gate 2 by the orchestrator, from `dir-architect`'s reported mapping. The `Gate 2` columns are
complete; `End state` is empty until Gate 3, and `Implementing task` stays empty at Gate 2 by rule.

**14 definition-of-done items**, one added after the architect was dispatched — item 14, the corrected
causal claim, from `measurement.md` §6 item 3.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1 — lookup appears once | T2 · B1, B2 | covered | **met** — B1, B2 |
| 2 — outcome re-check at most once | T2 · B3 | covered | **met** — B3 |
| 3 — 12 signatures unchanged | T2 · B4 | covered | **met** — B4 |
| 4 — 8 handler files untouched | T2 · B6 (SHA-256, needs no git) | covered | **met** — B6 |
| 5 — build clean | T1 · A8; T2 · B7 | covered | **met** — A8, B7 |
| 6 — suite 0 failed / 0 skipped | T1 · A9; T2 · B8 | covered | **met** — A9, B8; 980 final |
| 7 — the 8 `NotFound` floor-absent paths tested | T1 · A1–A5 (the 5 uncovered), A6 (the 3 covered) | covered | **met** — A1–A6 |
| 7a — *"shown discriminating by injected defect"* | **none** | **uncovered** — E1, resolved: carried as a dispatch instruction in T2 · B9, verified from T2's report; B9's verdict is the clean final state | **attributed** — `reviews/T2.md`, injections I1–I3 |
| 8 — the 2 `null` floor-absent paths tested | T1 · A6 | covered, satisfied at `startRef` by design | **met** — A6 |
| 9 — `MASTER-107` closed with corrected figures | T3 · C1–C5 | covered | **met** — C1–C5 |
| 10 — `CORE-015` `AMENDED 6b-xiv` block | T3 · C6–C11 | covered | **met** — C6–C11 |
| 11 — the `:993` false claim corrected | T3 · C12 | covered | **met** — C12 |
| 12 — `:999-1001` sound, backlog item falsified | T3 · C13, C14 | covered | **met** — C13, C14 |
| 13 — citation-drift property recorded | T3 · C15 | covered | **met** — C15, after amendment A1 |
| 14 — the causal claim corrected | T3 · **C17** | **uncovered at presentation**; the user chose to add it, so C17 was written into the draft before the freeze | **met** — C17 |
| *session constraint* — `sed -i` / CRLF | T1 · A10; T2 · B10; T3 · C16 | covered | **met** — A10, B10, C16 |

**Two uncovered rows, both presented at Gate 2 rather than retired.** Neither was closed by declaring it
out of scope.

### Why 7a has no criterion

The clause asks that each test be *shown* discriminating. That is a property of a task's **report**, and
this session's dispatch constraint forbids a criterion on what a task reports — only on what the
repository contains. A transient injection leaves nothing behind. The framework already carries it by
another route: `test.md` Coverage binds every implementer, the agent brief's report format mandates
showing a test can fail, and `dir-reviewer` receives the implementer's report. So the demonstration is
owed and checked; what it does not get is a formal per-criterion verdict.

**The ten floor-absent tests cannot be shown red** — five pass at `startRef` by design and five are new
but assert behaviour that already holds. Injection is their only route, and it must happen in
`DungeonMethods.cs`, which **T2 owns and T1 does not**. That is why T2 depends on T1 rather than running
beside it: it is the ownership constraint that makes the evidence possible, not a convenience.

### Why 14 was uncovered, and how it was closed

It was found after `dir-architect` was dispatched, so nothing it wrote covers it. `spec.md` is signed at
Gate 2 rather than frozen, so adding item 14 needed no amendment, and the manifest was still a draft, so
adding **C17** to T3 was an ordinary pre-freeze edit — no amendment, no `state.json` amendment record.
**The user chose to add it at the gate.** T3 was already editing that exact block for C12–C15, so it
costs no new ownership and no new dependency.

## Contract operations

One contract, `contracts/dungeon-public-surface.md`. Its "operations" are the 12 frozen public
signatures; §§2–7 are baseline tables rather than callable operations, and their consumers are noted
below the table.

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `Update` | T2 | T1 — `Update_SetsAllThreeFields` | covered | **called** — base-ref exception; **prediction confirmed** |
| `AddFloor` | T2 | T1 — `AddFloor_IncrementsFloorCount` | covered | **called** — base-ref exception; **prediction confirmed** |
| `RemoveFloor` | T2 | T1 — A6 | covered | **called** — base-ref exception; **prediction confirmed** |
| `ReorderFloor` | T2 | T1 — A6 | covered | **called** — base-ref exception; **prediction confirmed** |
| `UpdateFloor` | T2 | T1 — A6 | covered | **called** — base-ref exception; **prediction confirmed** |
| `AddArea` | T2 | T1 — A6 | covered | **called** — base-ref exception; **prediction confirmed** |
| `RemoveArea` | T2 | T1 — A1 | covered | **called** — base-ref exception; **prediction confirmed** |
| `UpdateArea` | T2 | T1 — A2 | covered | **called** — base-ref exception; **prediction confirmed** |
| `UpdateAreaOutline` | T2 | T1 — A3 | covered | **called** — base-ref exception; **prediction confirmed** |
| `AddPlacedObject` | T2 | T1 — A6 | covered | **called** — base-ref exception; **prediction confirmed** |
| `RemovePlacedObject` | T2 | T1 — A4 | covered | **called** — base-ref exception; **prediction confirmed** |
| `UpdatePlacedObject` | T2 | T1 — A5 | covered | **called** — base-ref exception; **prediction confirmed** |

**No declared operation lacks a consumer.** Every one of the 12 is invoked by a test in
`DungeonAggregateTests.cs`, the file T1 owns.

Non-operation sections and who consumes them: §2 dispositions — asserted by T1 A1–A6, preserved by T2
B5. §5 digests — T2 B6. §6 tested/untested split — T1 A1–A7. §7 baseline — T1 A9, T2 B8.

**A prediction for the Gate 3 end-state search, written before it runs.** The search scopes itself to
files the session touched and discards hits in test files. **Every consumer of all 12 operations is a
test**, and the 8 production call sites are frozen byte-identical by B6 and so cannot appear in the
diff range. Applied literally the search will therefore return **`not called` for all 12**. That is a
false alarm of exactly the shape two prior sessions hand-recorded, and the base-ref exception covers it:
all 8 production call sites exist at `chore/repo-audit` `20b0b09`, checkable with `git show <base>:<path>`.
**Predicted end state for all 12 rows: `called`, by the base-ref exception.** Recorded here so a
surprising result at Gate 3 reads as a surprise rather than as a late excuse.

## Consistency check

Run by the orchestrator, per-task, criteria read as a set and in both directions. **One finding, which
is not a contradicting pair — it is a criterion whose permission is broader than its guard.** Presented
at Gate 2 for the user's decision; see the Gate 2 presentation.

`B3` permits the outcome re-check to reach **zero** occurrences, "provided B5 and B7 still hold." Those
two do not constrain the thing at risk:

- `B5` covers only the **floor-absent** dispositions (unknown `floorId`), not the **inner-absent** ones
  (unknown `areaId` / `objectId`).
- `B7` is a clean build, and the change at issue compiles.
- `B8` is the suite, and **nothing in the suite covers it.**

The five re-checks exist so `Touch()` does **not** fire when the inner operation failed. Collapsing them
naively — `var outcome = floor.X(...); Touch(); return outcome;` — advances `LastModified` on a failed
area or object operation. `LastModified` is a mapped EF column in all four configurations, and is
surfaced to the UI through `DungeonDetailDto.LastModified` and `DungeonSummaryDto.LastModified` via
`DungeonReadRepository.cs:31` and `:57`. So the change is persisted and user-visible.

**Nothing guards it.** `grep -rn --include=*.cs "LastModified" src/DungeonMaster.Tests/` returns **7
lines in 2 files** — `EntityEqualityTests.cs` (hash stability across a `Touch`) and `FloorMethodsTests.cs`
(`UpdateNumber` *does* touch). **No test asserts that a failed operation does not touch.**

**Resolved at Gate 2 by user decision: criterion `B11` was added to T2 before the freeze.** It requires
that in each of the five methods, a non-`Success` inner outcome returns without a reachable `Touch()`.
`B3` is left unchanged and still permits zero occurrences — a design that removes the re-check while
preserving the disposition satisfies both. The user declined the variant that would also have
commissioned a T1 test for it, so **`B11` is verified by a human reading the five methods, not by the
suite**, and that is stated inside the criterion.

Everything else checked clean. The architect's own four pair-checks — `C5`'s vacuity on an empty cell,
`C2`/`C3` versus `C5`'s no-hash rule, `C12`'s absence-versus-quotation trap, `B1` versus `B5` — were
re-read and each holds. Ownership sets are disjoint: `DungeonAggregateTests.cs` / `DungeonMethods.cs` /
the two `docs/audit` files, no glob intersecting another.

## Vacuity check

Criteria satisfied at `startRef`, each deliberate and each labelled as preserved behaviour in its own
text: `A6`, `A7`, `A8`, `A10`, `B4`, `B5`, `B6`, `B7`, `B10`, `C16`. `spec.md` item 8 is in this class
too, and correctly so.

**Not vacuous, verified by running the check rather than reading the intent:** `A1`–`A5` each return 0
from their `grep -cF` at `20b0b09`; `B1` returns 10 where 1 is required; `B3` returns 5 where ≤1 is
required; `C1` finds `MASTER-107`'s Status cell empty; `C6` finds `grep -cF "AMENDED 6b-xiv"` → 0.

**One criterion was trivially true and was fixed before the freeze:** `C5` (no commit hash in the
`MASTER-107` cell) is satisfied by an empty cell, so the architect made it conditional on `C1` and
explicit that a reviewer must not close it against a cell that is still empty.
