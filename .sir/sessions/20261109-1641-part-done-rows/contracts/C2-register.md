# C2 -- Register and audit updates: MASTER-130 / CORE-024 closed done, MASTER-081 / CORE-016 closed DELIBERATELY UNCHANGED

No callable operation. One task, **T3**, edits two status cells in `docs/audit/00-master-index.md` (rows 269 and
318) and inserts two amendment blocks in `docs/audit/01-domain-application.md` (on `CORE-016` and `CORE-024`).
T3 runs after T2, so every figure it writes describes the finished tree. T3 runs no `dotnet` command: where a
cell or block states a build, test or EF outcome, it is labelled **told** and attributed to the review record of
the task that ran it (`.sir/sessions/20261109-1641-part-done-rows/reviews/T1.md`, `reviews/T2.md`).
**The backlog (`.sir/backlog.md`) is trimmed by the orchestrator at Gate 3** (section 5); no task touches `.sir/`
or `.dir-sdlc/`.

Editing hazards: C1 section 2, binding. Both files are CRLF with loneCR=0 at startRef (C1 section 1).

## 0. Rules for every cell and block

- **Cite what you measured; label what you were told.** "Measured" means T3 ran the command on the worktree during
  its own run and the text gives the command, or its C1 section, beside the figure. "Told" means the value comes
  from a contract, a review record, the signed discussion or the orchestrator, and the text names the source.
- **Status tokens.** With `*` stripped, row 269's cell begins with the token `DELIBERATELY UNCHANGED` and row 318's
  with `done` (the classifier tests `deliberately unchanged` before `part done` before `done`,
  `.sir/lessons.md:43`). Each cell's first text is exactly
  `**DELIBERATELY UNCHANGED** (20261109-1641-part-done-rows, 2026-11-09) -- ` (row 269) or
  `**done** (20261109-1641-part-done-rows, 2026-11-09) -- ` (row 318), the form of `MASTER-145` at `:333`.
- **Escaping.** Every literal `|` inside a cell is written `\|`. R (C1 section 5) prints 11 on each edited line.
  Cite instruments by their contract section rather than quoting one that contains `|`.
- **Previous wording.** Each cell ends with the sentence `**Previous status wording, retained unchanged below.**`,
  one space, the whole startRef cell verbatim (OC, C1 section 5), then ` |`. Row 318's startRef cell already ends
  in its own retained wording; it is carried whole, so the line then holds two markers. V (C1 section 5, the
  first-marker script) reproduces OC's startRef digest and byte count for each row.
- **Nothing else in the index changes:** U (C1 section 5) prints `97a020fc...` after the edits, and the file keeps
  691 lines.
- **Blocks.** Form of the newest blocks in this file, `AMENDED 20261105-1907-core-heavy-rows` at `:1770-1823`
  (on `CORE-024`) and `:2957-3053` (on `CORE-035`, a DELIBERATELY UNCHANGED closure): first line begins
  `**[AMENDED 20261109-1641-part-done-rows, 2026-11-09 -- `, the opening sentence is bold and ends `.**`, the
  last line ends `.**]**`; the block names its register row by `00-master-index.md:<line>`, states
  `**Nothing below deletes any previous wording.**`, states that every figure is labelled measured by `T3` or told,
  and ends with the severity/category/effort/risk sentence. Each block has one empty line before and after it and
  consists of inserted lines only: nothing in either entry is edited or deleted, `locations:` included (ID01, C1
  section 5).
- **Positions inside `01-domain-application.md` are cited by name, not by line,** in both new blocks and both new
  cells (e.g. "the `AMENDED 6b-xv` block on `CORE-016`"), because the insertions move every line below them.
  The one exception is the moved-citation paragraph of section 2.2 item 6, which states positions in the post-T3
  file deliberately. Positions in `00-master-index.md` (`:269`, `:318`, `:239`, `:333`) are safe to cite: T3
  changes no line count there.
- **No hash of this session's commit** appears anywhere, and each cell says why: tasks run without git and the
  orchestrator makes the commit (precedent: `MASTER-145` at `:333`, `MASTER-157` at `:345`).
- A cell or block may cite `.sir/sessions/20261109-1641-part-done-rows/contracts/<file>` by path. It never cites a
  scratch path (`grep -c AppData` on each file prints its startRef value after T3; measure it before the first
  edit).
- After the last edit, B (C1 section 1): `00-master-index.md` CR=LF=CRLF=691, loneCR=0;
  `01-domain-application.md` CR=LF=CRLF=3214 + the inserted line count, loneCR=0. AO prints 20.

## 1. MASTER-130 / CORE-024 -- closed done

### 1.1 Status cell, `00-master-index.md:318`

startRef cell: OC (C1 section 5), 11846 bytes, `part done (20261105-1907-core-heavy-rows, 2026-11-05) -- ...`.
The new cell begins `**done** (20261109-1641-part-done-rows, 2026-11-09) -- ` and states, in this order:

1. **Closure:** both residues that kept the row part done are cleared, so `MASTER-130` closes done; the `CW90`
   family was cleared by `20261105-1907-core-heavy-rows` (pointer to the previous wording, not re-measured).
2. **EXCLUSION 2 (`ct`) cleared, measured by T3:** CT-DECL (C1 section 4.1) prints 0 lines (startRef 28 in 18
   files); TAG (C1 section 4.2) prints 0 (startRef 15); CT-NEW prints 28 and TAG-NEW 15, in the same files;
   CT-WORD prints exactly one line, `src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs:151`, which
   is a comment naming the argument of a rejected `Task.Delay` alternative and not a parameter -- the one residue,
   left deliberately (C1 section 4.1). The 15 tags moved with their parameters, as the previous wording said
   they must.
3. **EXCLUSION 3 (`obj`) cleared, measured by T3:** OBJ-WORD (C1 section 4.3), the instrument EXCLUSION 3 used,
   prints only the four exempt `Entity.cs` lines; OBJ-HEAD prints 0 (startRef 11). The eleven lambda parameters
   in `FloorConfiguration.cs`, `PlacedObjectConfiguration.cs` and `DungeonReadRepository.cs` are named
   `placedObject`, the name `CORE-024`'s `Suggested direction.` gives, under the `dotnet-format.md:42` rule the
   previous wording cites; the two `DungeonReadRepository.cs` comments naming the parameter were renamed with it.
4. **The "17 lines" figure, settled:** the record counted LINES; at this session's startRef the same instrument
   gave 28 non-exempt lines, 18 + 8 + 2, with still eleven lambdas -- DRIFT, not error (C1 section 4.3's unit
   settlement, **told**: orchestrator measurement in the spec phase).
5. **Model and behaviour unchanged, told:** from `T2`'s review record, `dotnet ef migrations has-pending-model-changes`
   reports no changes after the rename (and reported pending changes under T2's injected must-fail control), the
   solution build is at 0 warnings and 0 errors, and the full suite passes at 217, 976 and 137 with 0 failed.
6. **What the row does not cover, measured by T3:** `obj` still names locals and parameters in
   `src/DungeonMaster.App` and `src/DungeonMaster.Tests` (C1 section 4.3's out-of-population figures, re-run by
   T3), outside the row's recorded population, which was Application, Infrastructure and Domain; they are not a
   residue of this row and are recorded so that the closure is not read as covering them. Gate 2 decided this
   population is left for later work: the item says it is carried to `.sir/backlog.md` as a new item, added by
   the orchestrator at Gate 3 (told: this session's `spec.md`, Scope), and states no backlog line number.
7. **Quotations left as records:** QOBJ (C1 section 4.5) -- audit text quoting the old `obj` lambda code, startRef
   15 lines across `00-master-index.md`, `01-domain-application.md` and `02-infrastructure.md` -- is not edited,
   including this cell's own previous wording.
8. A pointer to the `AMENDED 20261109-1641-part-done-rows` block on `CORE-024` in `01-domain-application.md`.
9. The no-commit-hash sentence, with `MASTER-051` at `:239`.
10. `**Previous status wording, retained unchanged below.**` and the startRef cell.

### 1.2 Amendment block on CORE-024

Inserted after the `AMENDED 20261105-1907-core-heavy-rows` block on `CORE-024` (whose last line, startRef
`:1823`, is `category, effort and risk are unchanged.**]**`) and before the heading
`### CORE-025 -- Six public Domain types carry no XML documentation`. Anchor on that heading text. It carries:

1. **Closure:** `MASTER-130` (`00-master-index.md:318`) is CLOSED done by this session; the two residues the
   `AMENDED 20261105-1907-core-heavy-rows` block left part done are cleared, with section 1.1 items 2-4 by figure.
2. **Line contents quoted by the earlier blocks now read differently, by rename rather than drift.** The
   `AMENDED 6b-xvii` block quotes `builder.Property(obj => obj.Rotation)` at `PlacedObjectConfiguration.cs:30`, and
   the `AMENDED 20261105-1907-core-heavy-rows` block quotes `:39` `builder.Property(obj =>` and `:40`
   `obj.Rotation)`. T3 reads `:39` and `:40` after T2 and quotes what they hold now (the same lines, with
   `placedObject`); those quotations are records of the pre-rename text and are not edited (QOBJ, C1 section 4.5).
3. **`FloorObjectMethods.cs`, measured:** the `AMENDED 6b-xvii` block's `obj` clearance is located in
   `src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs`, which `ls src/DungeonMaster.Domain/Entities/Floors/`
   no longer lists; `grep -rnw placedObject` finds the lambdas that block describes in `FloorMethods.cs`
   (T3 quotes the lines it finds). No classification word unless T3 can cite the session that moved it from a
   committed record; `.sir/lessons.md:70` records the fold into `FloorMethods.cs` (told).
4. **What remains outside the row:** section 1.1 item 6, by figure.
5. A pointer to section 2.2 item 6's moved-citation paragraph in the `CORE-016` block, which covers this block's
   insertion too.
6. The entry's `What.`, `Why it matters.` and `Suggested direction.` paragraphs and its `locations:` list are left
   exactly as written; the severity/category/effort/risk sentence.

## 2. MASTER-081 / CORE-016 -- closed DELIBERATELY UNCHANGED

### 2.1 Status cell, `00-master-index.md:269`

startRef cell: OC (C1 section 5), 303 bytes: `part done (6b-xv, 2026-09-30) -- the duplicated scale rule is now one
\`PlacedObject.ValidateScale\` helper and \`CanvasPoint\` gained a finiteness guard; the policy layer,
\`PolicyResult\`, \`DomainException\` and an \`ObjectScale\` value object were deliberately not built, which is why
this entry is not closed`. The new cell begins `**DELIBERATELY UNCHANGED** (20261109-1641-part-done-rows, 2026-11-09) -- `
and states, in this order:

1. **Closure:** closed with no code change, by user decision at Gate 1 (**told**:
   `.sir/sessions/20261109-1641-part-done-rows/discussion.md`, Decisions).
2. **Why, the row's own subject:** the duplication the title names is already gone -- 6b-xv's `ValidateScale` is
   one helper. Measured by T3: `grep -rn ValidateScale src --include=*.cs --exclude-dir=obj --exclude-dir=bin`
   prints 3 lines, the declaration `private static void ValidateScale(float scale, string paramName)` in
   `src/DungeonMaster.Domain/Entities/Floors/PlacedObjectMethods.cs` (a partial of `PlacedObject`) and its two
   calls, in `PlacedObject.cs` (constructor) and `PlacedObjectMethods.cs` (`Update`); T3 cites each by
   `file:line` from that output.
3. **What was considered and declined (told: Gate 1):** a policy layer, `PolicyResult`, `DomainException` and an
   `ObjectScale` value object. Measured by T3, each still absent or unchanged: `find src -type d -name Policies`
   (excluding `obj`/`bin` paths) prints nothing; `grep -rnwE` for `PolicyResult` and `DomainException` over `src`
   (`*.cs`, `obj`/`bin` excluded) prints nothing; `ObjectScale` is still `public static class ObjectScale`
   (`src/DungeonMaster.Domain/Entities/Floors/ObjectScale.cs`, line by `grep -n`). Cite the alternation as two
   separate commands or by this contract section, so no `|` enters the cell.
4. **What the decision leaves, stated plainly:** `CORE-016`'s `Suggested direction.` stands unactioned by decision,
   and the scale bounds are still checked in two layers -- the entity helper and the two command validators'
   `InclusiveBetween(ObjectScale.Min, ObjectScale.Max)` (T3 cites both validator lines by `grep -n`), both
   referencing the `ObjectScale` constants rather than restating the numbers.
5. A pointer to the `AMENDED 20261109-1641-part-done-rows` block on `CORE-016`.
6. **No commit hash, and none is owed:** no code changed for this row (the ground `MASTER-145` at `:333` closes on);
   `MASTER-051` at `:239`.
7. `**Previous status wording, retained unchanged below.**` and the startRef cell.

### 2.2 Amendment block on CORE-016

Inserted after the `AMENDED 6b-xv` block on `CORE-016` (whose last line, startRef `:1059`, is
`**OPEN as part done**.**]**`) and before the heading
`### CORE-017 -- ObjectKind has no zero value, so default(ObjectKind) silently means DoorWood`. Anchor on that
heading text. It carries:

1. **Closure:** `MASTER-081` (`00-master-index.md:269`) is CLOSED **DELIBERATELY UNCHANGED** by user decision at
   Gate 1, with no code change; section 2.1 items 2-4 by figure.
2. **Supersession, quoted verbatim:** the `AMENDED 6b-xv` block's last sentence ends "`MASTER-081` / `CORE-016`
   stays **OPEN as part done**" (at startRef `:1058-1059`, given here for T3's orientation only -- the block
   cites the sentence by block name, per section 0); this block supersedes that status and does not edit it.
3. **The 6b-xv block's positions have moved, measured by T3:** that block places the helper at
   `PlacedObject.cs:91-100`, called from `:28` and `:70`; T3 states where the declaration and the two calls are
   now (section 2.1 item 2's output). No classification word unless T3 cites a committed record for the move;
   `.sir/lessons.md:70` records that `PlacedObject`'s methods were split into their own file (told).
   `locations:` is kept unchanged as a record of the audited state.
4. **The `Suggested direction.` is declined, not pending:** it is left exactly as written as the record of what
   was proposed; this block, not that paragraph, is the current disposition.
5. **The deferred "Scale value object" item is answered by this decision** (told: discussion.md, Not in scope);
   nothing is built.
6. **Moved citations, recorded rather than left to drift** -- the hazard the `AMENDED 6b-xv` block itself
   describes in its penultimate paragraph. T3 runs CITE-IN and CITE-X (C1 section 6) after both insertions and
   lists every citation that was **live and correct at startRef** (C1 section 6's table, each target re-read by
   T3) and whose target this session's two insertions moved, each as `cited at` -> `startRef position` ->
   `position in the post-T3 file`, and states the count. It names the citations that were already stale or dated
   at startRef as such, by count, without re-deriving them. **No citation is edited** -- not in this file and not
   in `03-app-shell-and-views.md` or `04-app-controls-and-theme.md`, which no task owns (decided at Gate 2).
7. The severity/category/effort/risk sentence.

## 3. Order of edits inside T3

Insert the `CORE-024` block first, then the `CORE-016` block (lower insertion first keeps the `CORE-016` heading
region stable while the first block is written). Write both cells after both blocks. Measure CITE-IN and CITE-X
before the first edit and after the last; write item 6 of section 2.2 last, from the post-edit measurement, and
re-run CITE-IN after writing it (the paragraph adds citations of its own, which the final count includes and
names).

## 4. Figures T3 must not restate from memory

Every figure in a cell or block is either re-run by T3 (and the text gives the C1 section or command) or labelled
told with its source. In particular the 217 / 976 / 137 test totals, the 0-warning build and the EF result are
told from T2's review record; CT-*, TAG*, OBJ-*, QOBJ, the `ValidateScale` lines, the `Policies` / `PolicyResult` /
`DomainException` absences and CITE are measured by T3.

## 5. Backlog, by the orchestrator at Gate 3

Remove `.sir/backlog.md:15` (MASTER-081), `:16` (MASTER-130), `:22` (`ct` residue) and `:24` (`obj` residue)
(told: discussion.md, Decisions). Heading `:13` is then empty and kept. Append one new item (Gate 2 decision)
for the `obj` population outside `MASTER-130`'s recorded scope (section 1.1 item 6's figures, as T3 measured
them). No task edits `.sir/`.
