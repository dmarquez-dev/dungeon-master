# Coverage — 6b-iv

Written at Gate 2, before the approval that freezes `manifest.json`. The `End state` column is filled
at Gate 3. `Implementing task` is deliberately empty at Gate 2 — it is only derivable once the
definition-site search runs.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| Reorder drop defect diagnosed before it is fixed | T1 c1, c2 | covered | |
| Reorder drop defect fixed, human-confirmed | T1 c3 | covered | |
| Reorder path gains its first test coverage | T1 c4, c5 | covered | |
| MASTER-164: toggle takes keyboard focus, areas reachable by keyboard | T2 c6 | covered | |
| MASTER-164: header row height unchanged | T2 c1, c4; T3 c3 | covered | |
| MASTER-164: the two docs-undecidable questions are run, not assumed | T2 c5, c1, c4 | covered | |
| MASTER-163: handle grabbable without fighting the scrollbar | T3 c1, c2, c5 | covered | |
| MASTER-163: "cannot be performed at all" not restated | T3 c6; T6 c1 | covered | |
| MASTER-166: write latency measured against frame interval | T4 c1, c2 | covered | |
| MASTER-166: conclusion not overstated as a proven cause | T4 c3 | covered | |
| MASTER-166: ClearGhostAreas after RefreshFloorAsync | T4 c4 | covered | |
| MASTER-166: area-move site decided | T4 c5 | covered — in scope | |
| MASTER-166: Place-tool ghost not folded in | T4 c6 | covered | |
| MASTER-161 corrected to done | T6 c6 | covered | |
| MASTER-165 falsified, not re-attempted | T6 c6 | covered | |
| MASTER-163 claim and severity corrected | T6 c1 | covered | |
| MASTER-098 void deferral premise struck | T6 c7 | covered | |
| Five stale citations corrected | T6 c1 | covered | |
| Reorder defect filed as MASTER-174 | T6 c2, c3 | covered | |
| AreaSidebarItem missing-behavior filed as MASTER-175 | T6 c2 | covered | |
| **AreaSidebarItem card body gains HandCursorBehavior** | **T3 (added at Gate 2)** | **uncovered at draft — user scoped it IN to T3** | |
| Appium harness extended with floor-sidebar facts | T5 c3 | covered | |
| Appium criteria do not rest on an unproven instrument | T5 c2 | covered | |
| Appium run has a stated fallback | T5 c7 | covered | |
| **Entry status closures with commit evidence** | **none — orchestrator at Gate 3** | **uncovered — accepted; a task cannot invoke git** | |
| **Full solution suite green after all six tasks land** | **none — orchestrator** | **uncovered — accepted; no task is last in every path** | |
| **MASTER-165 remediation of the underlying behaviour** | **none** | **uncovered — accepted out of scope; the claim is false, there is nothing to remediate** | |
| **MASTER-098's own remedy (bindable visual-state properties)** | **none** | **uncovered — accepted out of scope at Gate 1 and re-affirmed at Gate 2; re-verified at Gate 3** | |
| **The three human-observation criteria** | **T1 c3, T2 c6, T3 c5 — executed by the orchestrator with the user** | **covered — escalation E5 resolved: orchestrator runs each when its task reports done and hands the transcript to the reviewer** | |

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| C1 §1 write `"FloorId"` (`OnDragStarting`) | | T1 | covered | |
| C1 §1 read payload + five error rows | | T1 | covered | |
| C1 §2 `ReorderFloorCommand.ExecuteAsync(id, target.Number)` | | T1 | covered | |
| C1 §3 `DragGestureRecognizer` host + handler names | | T3 (must not move it) | covered | |
| C1 §4 both drop targets stay wired | | T2, T3 | covered | |
| C1 §5 candidate list / refuted theories | | T1 | covered | |
| C1 §6 "no coverage exists at b3b1641" | | T1, T6 | covered | |
| C2 §1 `FloorCard` | | T5 fact (d) | covered | |
| C2 §1 `FloorExpandToggle` | | T5 facts (a)(b)(d) | covered | |
| C2 §1 `FloorDragHandle` | | T5 fact (c) | covered | |
| C2 §1 `AddAreaButton` | | T5 fact (d) | covered | |
| C2 §1 `FloorNotebookButton`, `FloorDeleteButton` | | T2, T3 by preservation | covered — preservation constraint, no T5 fact asserts them | |
| C2 §2 semantic properties + Mac Catalyst trap | | T2 | covered | |
| C2 §3 assertable set | | T5 | covered | |
| C2 §3 not-assertable set | | T5 must not depend on it; T1 owns the human observation | covered | |
| C2 §4 prerequisites + fallback branch | | T5, orchestrator on the fallback | covered | |
| C3 §1 pinned footprint | | T2 | covered | |
| C3 §2 height invariant | | T2, T3 | covered | |
| C3 §3 must-run Q1, Q2 | | T2 | covered | |
| C3 §4 four prohibitions | | T2 | covered | |
| C3 §5 T3's inheritance | | T3 | covered | |
| C4 §1 MASTER-174 | | T6 files it; T1's report is its input | covered | |
| C4 §1 MASTER-175 | | T6 files it; T3 now fixes it | covered — amended at Gate 2 | |
| C4 §2 correction half | | T6 | covered | |
| C4 §2 closure half | | none — orchestrator at Gate 3 | **uncovered — accepted; a task cannot invoke git** | |
| C4 §3 corrections 1-10 | | T6 | covered | |
| C4 §4 do-not-touch set | | T6 | covered | |
| C4 §5 amendment format | | T6 | covered | |

## Gate 2 decisions recorded here

- **E5** resolved: the orchestrator runs each human observation when its task reports done and hands
  the transcript to `dir-reviewer` alongside the diff. The alternative of folding all three into one
  post-T3 session was rejected because it would serialize the reviews on top of an already-serial
  task chain.
- **U4** resolved: `MASTER-175` is **fixed in T3**, not merely filed. `AreaSidebarItem.xaml` joins
  T3's ownership. T6's filed entry must describe a defect this session closed.
- **U5** resolved: `MASTER-098` stays out of scope. T6 strikes its void deferral premise; the
  orchestrator re-verifies at Gate 3.
- **U1, U2, C4 §2 closure half** accepted as orchestrator-owned. Precedent: sub-session 6b-iii's
  `coverage.md:33` accepted the identical solution-suite row.
- **U3** accepted out of scope: `MASTER-165`'s claim is false, so there is no underlying behaviour to
  remediate.

---

# End state — filled at Gate 3

## Definition of done — the join

Every definition-of-done row's covering task passed review with **every one of its acceptance criteria
met**. Six tasks, 60 criteria, **60 met, 0 not met**, across `reviews/T1.md` … `reviews/T6.md`.

| Covering task | Criteria | Met | Rounds | End state of its rows |
|---|---|---|---|---|
| T1 | 9 | 9 | 1 | met |
| T2 | 9 | 9 | 0 | met |
| T3 | 11 | 11 | 0 | met |
| T4 | 9 | 9 | 0 | met |
| T5 | 9 | 9 | 0 | met |
| T6 | 13 | 13 | 1 | met |

**No definition-of-done row is `not met`.** The two rows that reached `NOT MET` during review — T1's c1
and c5, and T6's c13 — were closed by revision before their tasks passed. T3's c6 and c9 reached
`UNDETERMINED` for an orchestrator omission (the task report was not supplied to the reviewer) and were
closed on a second pass without a revision round.

The five rows accepted as **uncovered** at Gate 2 keep that disposition:

| Row | Gate 2 | End state |
|---|---|---|
| Entry status closures with commit evidence | uncovered — accepted, a task cannot invoke git | **orchestrator-written at Gate 3** |
| Full solution suite green after all six land | uncovered — accepted, no task is last in every path | **met — 908 passed, 0 failed; build 0 Warning(s), 0 Error(s)** |
| `MASTER-165` remediation of the underlying behaviour | uncovered — accepted out of scope, the claim is false | out of scope |
| `MASTER-098`'s own remedy | uncovered — accepted out of scope at Gate 1 and Gate 2 | out of scope |
| `AreaSidebarItem` card body gains `HandCursorBehavior` | uncovered at draft — user scoped it IN to T3 | **met — T3 c8, human-observed** |

## Contract operations — the search

Searched over the session's touched files across `b3b1641..3448ecd`.

| Operation | Implementing task | Consuming task | End state |
|---|---|---|---|
| `FloorReorderDrop.Decide` | T1 (`App.Core/Editor/FloorReorderDrop.cs`) | T1, T3 | **called** — `FloorSidebarItem.xaml.cs:449`, `DungeonDetailPage.xaml.cs:2295`, `:2415` |
| `FloorReorderNearest.Resolve` | T3 (`App.Core/Editor/FloorReorderNearest.cs`) | T3 | **called** — `DungeonDetailPage.xaml.cs:2278` |
| `OptimisticGhostSequence.RefreshThenClearAsync` | T4 (`App.Core/Timing/OptimisticGhostSequence.cs`) | T4 | **called** — `DungeonDetailPage.xaml.cs:1729`, `:1774` |
| `ReorderFloorCommand.ExecuteAsync` | pre-existing | T1, T3 | **called** — 3 production sites in `DungeonDetailPage.xaml.cs` (`:861`, `:2306`, and the placeholder closure), counted by `grep -rn … src/DungeonMaster.App/ --include=*.cs \| wc -l` → 3 |

**A rule applied honestly rather than mechanically, and the reader should know which.** Coverage's
call-site search says to discard hits in a file owned by the same task that owns the definition site,
because a sibling-file call inside the implementing task is not evidence the application reaches the
operation. Applied strictly, **all three new operations would render `not called`**: T1 owns both
`FloorReorderDrop.cs` and `FloorSidebarItem.xaml.cs`; T3 owns both `FloorReorderNearest.cs` and
`DungeonDetailPage.xaml.cs` after the E8 grant; T4 owns both `OptimisticGhostSequence.cs` and
`DungeonDetailPage.xaml.cs`.

That strict reading would be wrong here, and the reason is stronger than a call-site search:
**the application was observed reaching all three, by a person, on the running app.** A seam release
reordered to the upper card (`FloorReorderNearest.Resolve` and `FloorReorderDrop.Decide`, transcript
`observations/T3-sidebar.md`); a drag onto a card reordered and persisted
(`ReorderFloorCommand.ExecuteAsync`, `observations/T1-reorder.md`). `RefreshThenClearAsync` sits on the
draw-completion path both observations exercised.

The discard rule exists because a static search cannot distinguish a reachable call from an unreachable
one — which is exactly the limitation `Coverage` states in **What a call site does not prove**. Where a
human observation exists, it answers the question the rule was approximating. Recorded this way so a
later reader sees both the rule and why it was not applied, rather than finding `called` with no
explanation of how it survived a same-task discard.

## What this block does not claim

A `called` row means something calls the operation. **It does not mean a user can reach it** — except
for the three above, where a user demonstrably did, which is stated with its transcript rather than
inferred from the search.

---

## A correction to this file, made while writing it

The join above first read *"Six tasks, 62 criteria, 62 met"*. **The column sums to 60** — 9 + 9 + 11 +
9 + 9 + 13 — and 60 is what `manifest.json` holds. The figure was written without adding the column up.

Recorded rather than quietly fixed, because it is the same failure this session documented nine times
in prose form, arriving in its numeric one: **a count asserted instead of derived.** The orchestrator
wrote it, immediately after rendering a block about that exact pattern. The correct figure was one
`sum()` away, over a file already open.

The rule the index now carries — *open the file and read the line* — has a numeric twin: **add the
column up.** Neither is about care. Both are about the derivation being performed at the moment the
claim is written, rather than recalled from the moment it was formed.
