# Coverage mapping — session 6b-vi

Produced by `dir-architect` with the draft manifest; written here by the orchestrator, who holds the user's
Gate 2 decisions. The `Gate 2` columns were written **before** the approval that froze `manifest.json`; the
`End state` column and `Implementing task` were written at Gate 3.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 — a keyboard-only user can move a floor one position up and one down, and it persists | T1 + T3 | covered | **met** — observation **O2** ①, and from every focusable child of the card, repeatedly |
| D2 — the drag handle takes keyboard focus and is `Tab`-reachable | T3 | covered | **not met, superseded by escalation E1** — see below |
| D3 — reorder keys do nothing unless a handle holds focus; no collision with the tool shortcuts | T3 | covered | **met, as amended** — the gate is the card, not the handle (A1 §3A); T3 review round 1 criterion 5, O2 ① |
| D4 — pointer drag-reorder still works, unchanged | T3, T4 | covered | **met** — observation **O2** ②, after failing O1 ② |
| D5 — the keyboard route is announced to assistive technology | T3 + T4 | covered | **met** — A1 §5A hint string, asserted by T4 and reviewer-reproduced by production-side injection |
| D6 — `FloorReorderKeys.Decide` covered by tests shown able to fail, with a positive control | T4 | covered | **met** — `injected defect`, reproduced independently by T4's reviewer: 1 failure, named control still passing |
| D7 — the floor card header row footprint is not regressed | T4 | covered | **met** — O1 ③ (no height change), O2 ③ (name recovered); see the measurement limit in O2 ③ |
| D8 — `MASTER-098` has a citable evidence artifact at a fixed absolute path | T2 | covered | **met** — `artifacts/MASTER-098-verification.md`, committed `553493f` |
| D9 — 0 warnings / 0 errors on both App TFMs; full suite green | T3, T4 | covered | **met, on three TFMs not two** — `0 Warning(s), 0 Error(s)`; suite `Failed: 0, Passed: 796` |
| D10 — the drop placeholder is not touched | T3 | covered | **met** — T3 review criterion 14, all five members present and untouched |

### D2 was not met, and that is a decision rather than a failure

**D2 said the drag handle itself takes keyboard focus and is `Tab`-reachable.** It was met at `3cf6e1f` and
is **not met at `4c79364`**, because escalation **E1** deliberately gave it up: observation O1 found that the
`Button` which made the handle focusable also **broke the pointer drag entirely** and squeezed the floor
name. The user chose to revert, accepting weaker discoverability in exchange for a working drag.

**O2 ④ confirms the cost in the running application** — `Tab` skips the handle glyph, and a screen-reader
user meets its `Hint` as static text rather than as a focusable control.

**`MASTER-176`'s actual requirement is D1, not D2** — that a keyboard-only user can reorder floors — and D1
is met. D2 was the means this session first chose, not the entry's requirement.

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `FloorReorderKeys.Decide` | T1 | T3 (page `OnKeyDown`) | covered | **called** — `DungeonDetailPage.xaml.cs:2264`, a different task from the definition site |
| `FloorKeyboardReorderDecision.None` | T1 | T3 (the no-op path) | covered | **not called** — see below |
| `FloorSidebarItem.ContainsKeyboardFocus` *(was `IsDragHandleFocused`, renamed by A1 §3A)* | T3 | T3 (the page's focus gate) | covered | **not called** by the rule — same-task, as predicted at Gate 2 |
| `FloorSidebarItem.FocusReorderAffordance()` *(was `FocusDragHandle()`, renamed by A1 §3A)* | T3 | T3 (page, after `RebuildFloorList`) | covered | **not called** by the rule — same-task, as predicted at Gate 2 |
| `ReorderFloorCommand.ExecuteAsync` (pre-existing) | Application layer (outside this session) | T3 — the fourth call site | covered | **called** — `DungeonDetailPage.xaml.cs:2276` |
| C1 §5A announced `Hint` / `ToolTip` strings | T3 (markup) | T4 (asserted) | covered | **test-only by the rule** — and see below |

Search method, per the framework: `git diff --name-only $(git merge-base chore/repo-audit HEAD)..HEAD` gave
the seven touched files; each operation was matched as a whole word across them; hits were discarded at the
definition site, in files owned by the same task that owns the definition site, and in test files.

### `FloorKeyboardReorderDecision.None` is `not called`, and this one is a real search result

Every `None` hit outside `FloorReorderKeys.cs` is either **`ObjectRotation.None`** — an unrelated type in
`DungeonDetailPage.xaml.cs:1156-1216` — or doc-comment prose in the tests. **No production code outside the
defining file names `FloorKeyboardReorderDecision.None`.**

**It is nonetheless benign, and the reason is worth writing down rather than waving at.** `None` is a
*returned sentinel*, not an invoked operation: `Decide` returns it, and the page consumes the result by
reading `decision.ShouldReorder` rather than by comparing against `None`. So the value is reached on every
no-op keystroke — O2 ① exercised exactly that — while the *name* appears nowhere outside its own file. The
call-site search cannot distinguish "nobody uses this" from "everybody uses this without naming it", and
here it is the second.

### The two same-task rows were predicted at Gate 2, and the prediction held

`ContainsKeyboardFocus` and `FocusReorderAffordance()` have their implementer and their consumer both inside
T3, so the framework's same-task discard makes them search as `not called` however thoroughly they are
wired. `coverage.md` said so **before** dispatch rather than discovering it here.

Their substitute evidence is **stronger than it was at Gate 2**, because it is no longer only per-criterion
verdicts: **observation O2 ① exercised both in the running application** — the gate from several focusable
children of the card, and the restoration landing on the chevron after a reorder. That is the direct
behavioural evidence the search structurally cannot supply.

### C2 and C3 are rule-shaped, and the operations check does not apply

`C2-editing-build-and-counting-rules.md` and `C3-master-098-closure-evidence.md` declare **rules** — editing
prohibitions, counting forms, required artifact content — not callable operations. The call-site search was
**deliberately not run** over them; it would have produced a meaningless `not called` on every row or
incidental text matches dressed as call sites. Their substitute evidence is the per-criterion review verdicts
of the tasks they bind.

Stated at Gate 2 rather than rediscovered here, and identical to the limit 6b-v recorded and carried to the
backlog.
