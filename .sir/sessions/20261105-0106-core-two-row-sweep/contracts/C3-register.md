<!-- frozen: 20261105-0106-core-two-row-sweep, Gate 2 -->

# C3 -- Register update: the MASTER-110 status cell and the CORE-022 amendment block

No callable operation. T4 owns both files and runs after T1. The backlog rows `.sir/backlog.md:9`,
`:34`, `:65` and `:191` are removed by the orchestrator at Gate 3, keeping the `:7` heading. No task
removes them. MASTER-180 is already closed (`00-master-index.md:368`) and is not edited.

Lesson 2026-11-03 binds every sentence T4 writes: **cite what you measured, and label what you were
told.** In this contract, "measured" means T4 ran the command on the post-T1 worktree. "Told" means the
value comes from C1, C2 or a review record, and the text says so.

## 1. Status cell, `docs/audit/00-master-index.md:298`

At startRef line 298 begins `| 110 | MASTER-110 |`, ends `| CORE-022 | |`, and R (C1 section 12) prints 11.
Only its last (status) cell changes. It becomes:

```
**done** (20261105-0106-core-two-row-sweep, 2026-11-05) -- <body>
```

With `*` stripped, the cell's first token is `done` (register-classifier lesson, `.sir/lessons.md:43`).
The body states:

- **The remedy:** Gate 1 chose "conform both", the fold-back option of `CORE-022`'s `Suggested direction.`,
  applied to `PlacedObject` too. `FloorObjectMethods.cs` was folded into `FloorMethods.cs` and deleted.
  `PlacedObject` became `public sealed partial class`, split into `PlacedObject.cs` (constructors and
  properties) and `PlacedObjectMethods.cs` (`Update`, `ValidateScale`). dm-conventions is unchanged.
- **Closure evidence, measured by T4 on the post-T1 worktree, each figure beside its command:**
  - PC (`grep -n partial src/DungeonMaster.Domain/Entities/*/*.cs`) prints eight lines, two per
    entity, for `Area`, `Dungeon`, `Floor` and `PlacedObject`.
  - `ls` reports `src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs` absent.
  - M (C1 section 8) prints the startRef digest for both entities. The startRef digest is labelled as
    measured by the architect at `11f9562` and stated in C1 (told).
- **Build and test:** stated only as attributed to T1's review record (told), never as T4's own measurement.
  The cell may also leave them out.
- **A pointer** to the `AMENDED 20261105-0106-core-two-row-sweep` block on `CORE-022` in
  `docs/audit/01-domain-application.md`.
- **The sentence:** no commit hash is cited, because tasks run without git and the orchestrator makes
  the commit.

**Escaping.** Write every literal `|` inside the cell as `\|`. The cell cites instruments by their C1
section and does not quote them, so no instrument text enters the cell. Several instruments contain `|`.
The cell may cite `.sir/sessions/20261105-0106-core-two-row-sweep/contracts/C1-measurement.md` by path
(precedent: MASTER-180 at `:368`).

No other line of `00-master-index.md` changes. The file keeps 691 lines.

## 2. Amendment block, `docs/audit/01-domain-application.md`

One block goes after `CORE-022`'s `**Suggested direction.**` paragraph (startRef `:1572-1575`) and
before `### CORE-023` (startRef `:1577`). It is separated from both by one empty line, as at `:2884`
and `:2917`.

**Form** (house form, as at `:2885-2916`): the first line begins
`**[AMENDED 20261105-0106-core-two-row-sweep, 2026-11-05 -- `, and the last line ends `**]**`. The block
says **Nothing below deletes any previous wording.** It carries:

1. **Closure.** `MASTER-110` (`00-master-index.md:298`) is CLOSED **done** by this session's task `T1`.
   The block gives the remedy as in section 1, and the closure evidence T4 measured (PC, the `ls`, M),
   each with its command or its C1 section.
2. **Correction of the stale figures.** Quote verbatim the words on startRef `:1567`: "`Floor.cs` at 73
   lines is genuinely readable next to 305 lines of behaviour". Then state the figures:
   - At `11f9562`, `wc -l` gives `Floor.cs` 94, `FloorMethods.cs` 176 and `FloorObjectMethods.cs` 112, so
     Floor's behaviour came to 288 lines. These are told: measured by the architect and stated in C1
     and C2.
   - After T1, measured by T4 with `wc -l`: `Floor.cs` 94 and `FloorMethods.cs` 282.

   - At the audit's own commit `16d594b` (the commit that wrote `:1567`, found by
     `git log -S` over `docs/audit/01-domain-application.md` for the quoted phrase's
     opening, `Floor.cs` plus its closing backtick plus " at 73 lines"; it returned `16d594b` alone), `git show 16d594b:<path> | wc -l`
     gives `src/DungeonMaster.Domain/Entities/Floor/Floor.cs` 73, `FloorMethods.cs` 148 and
     `FloorObjectMethods.cs` 157, so behaviour came to 148 + 157 = 305. The folder was then singular
     `Floor/`. These are told: measured by the orchestrator in the spec phase (tasks have no git), with
     the control `git show 11f9562:src/DungeonMaster.Domain/Entities/Floors/Floor.cs | wc -l` printing 94.

   **Classification: DRIFT** -- both figures were true at `16d594b` and the code has since moved. Say
   so, and say the evidence was supplied by the orchestrator. The block notes that the layer verdict at
   startRef `:2940` restates both figures, and that this block supersedes it there too.
3. **The `What.` paragraph's state claims are superseded.** "`Floor` adds a third partial" and
   "`PlacedObject` is not partial at all" describe startRef. Neither holds after T1.
4. **`locations:` is kept unchanged.** It describes the pre-change state. After T1,
   `FloorObjectMethods.cs:5` no longer exists. The other seven citations still name a partial
   declaration on the cited line, `PlacedObject.cs:8` now included. T4 checks this against PC's output.
5. **Other mentions, deliberately untouched.** `:2302`, inside another entry's `AMENDED 6b-xvii` block,
   refers to "`CORE-022`'s stale line counts, which `MASTER-110` carries". This block is where those
   counts are corrected. `:2940-2941`, the layer verdict, stays as written: it is dated audit narrative.
   Neither line is edited.
6. The record's severity, category, effort and risk are unchanged.

Nothing in the entry is deleted. The diff to `01-domain-application.md` consists only of inserted lines.
The opener and closer censuses (C1 section 12) each print 15, where they print 14 at startRef.

## 3. Line endings

Both files are CRLF with loneCR = 0 at startRef (C1 section 12). Edit them only with `Edit` (C1 section 14).
After the last edit, B prints CR=LF=CRLF=691 for `00-master-index.md` and CR=LF=CRLF=(2950 + the inserted
line count) for `01-domain-application.md`, with loneCR=0.
