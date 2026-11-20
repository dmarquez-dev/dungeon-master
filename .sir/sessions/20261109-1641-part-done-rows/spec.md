# part-done-rows

Take MASTER-130 and MASTER-081 out of `part done`. MASTER-130 closes **done** by renaming its two residues
(`ct`, `obj`). MASTER-081 closes **DELIBERATELY UNCHANGED** with no code change.

## Context

This is the last scheduled backlog heading, `.sir/backlog.md:13`. Gate 1's decisions are in `discussion.md`. Every
figure below was measured at `91bbbf4` (session startRef), with the command and its control in C1.

- **MASTER-130 / CORE-024** (`00-master-index.md:318`). The `CW90` family was cleared by
  20261105-1907-core-heavy-rows. Two residues remain:
  - **`ct` (EXCLUSION 2).** There are 28 `CancellationToken ct` declarations in 18 files (17 in Tests, 1 in App,
    `ObjectPackRegistry.cs`) and 15 `<param name="ct">` tags. CT-WORD finds 63 `ct` lines in 19 files. 62
    occurrences are renamed. The 63rd line is a comment at `NotebookView.xaml.cs:151`, `Task.Delay(ct)`, and is
    not a parameter.
  - **`obj` (EXCLUSION 3).** The record's "17 lines" counted **lines** of `grep -rnw obj` over Application,
    Infrastructure and Domain, minus 4 exempt `Entity.cs` lines (9 + 6 + 2). The same instrument now gives 28 lines
    (18 + 8 + 2). The cause is **drift**: PlacedObjectConfiguration's lambdas were rewrapped onto two lines, and
    two `DungeonReadRepository.cs` comments citing `obj.Position` were added (`:91`, `:133`). There are still 11
    lambdas (`obj =>`), all in the three Infrastructure files.
- **MASTER-081 / CORE-016** (`:269`). 6b-xv's `ValidateScale` already made the scale rule one helper. It now lives
  at `PlacedObjectMethods.cs:45`, called from `PlacedObject.cs:30` and `PlacedObjectMethods.cs:24`. The policy
  layer, `PolicyResult`, `DomainException` and an `ObjectScale` value object were not built. Gate 1 declined them.
- **Line endings.** Every target file is CRLF with 0 lone CRs, measured byte by byte (C1 section 1). That includes
  `.sir/backlog.md`: 197 CRLF, 0 lone CRs.

## Definition of done

- [ ] **D1** No `CancellationToken ct` remains in `DungeonMaster.Tests` or `DungeonMaster.App`. All 28 declarations
  read `cancellationToken`, in the same 18 files.
- [ ] **D2** All 15 `<param name="ct">` tags read `cancellationToken`, and each pairs with a live parameter in its
  file.
- [ ] **D3** Every `ct` use is renamed. The only `ct` word left in `src` is the `NotebookView.xaml.cs:151`
  comment.
- [ ] **D4** The 11 `obj` lambda parameters are named `placedObject`, and so are the two `DungeonReadRepository.cs`
  comments. OBJ-WORD prints only the 4 exempt `Entity.cs` lines.
- [ ] **D5** The EF model is unchanged: `has-pending-model-changes` reports no changes, and its must-fail injection
  is shown to fail. No migration is added.
- [ ] **D6** Each code task's owned files equal the exact word rename of their startRef content. Each matches its
  C1 §4.4 digest.
- [ ] **D7** The solution builds with 0 warnings and 0 errors. The suite passes 217 / 976 / 137 with 0 failed and
  0 skipped.
- [ ] **D8** Every edited file keeps CRLF line endings, 0 lone CRs and its first bytes.
- [ ] **D9** Row 318 (MASTER-130) reads **done**. Its figures are measured, and its previous wording is retained
  whole.
- [ ] **D10** CORE-024 carries an `AMENDED 20261109-1641-part-done-rows` block.
- [ ] **D11** Row 269 (MASTER-081) reads **DELIBERATELY UNCHANGED** in MASTER-145's form, with the Gate 1
  reasoning. Its previous wording is retained.
- [ ] **D12** CORE-016 carries an `AMENDED 20261109-1641-part-done-rows` block that records the line citations the
  insertions move.
- [ ] **D13** Nothing else in the master index, the domain file or any other audit file changes, and no scratch
  path is cited.
- [ ] **D14** The orchestrator's item, at Gate 3: `.sir/backlog.md:15`, `:16`, `:22` and `:24` are removed. One
  item is appended for the `obj` population outside the row. Every other line stays byte-identical, and heading
  `:13` is kept empty.
- [ ] **D15** The orchestrator's item, at Gate 3: the session's diff touches only the 23 paths the three tasks own.

## Scope

The three tasks run as a serial chain: T1 → T2 → T3.

| Task | What it does |
|---|---|
| T1 | `ct` → `cancellationToken`: 28 parameters, 15 tags and every use, in 17 Tests files and `ObjectPackRegistry.cs` |
| T2 | `obj` → `placedObject`: 11 lambdas and 2 comments in the three Infrastructure files; the EF model is proven unchanged |
| T3 | Rows 269 and 318, plus the CORE-016 and CORE-024 amendment blocks (C2) |

Gate 2 decisions:

- **Q1.** T3 inserts the CORE-016 block and lists every citation it moves, old position and new. No citation is
  edited.
- **Q2.** MASTER-130 closes done on its recorded population. `obj` in App (32 lines), Tests (46) and App.Tests (1)
  is recorded in the cell as outside the row. It goes to the backlog as one new item at Gate 3.
- **Q3.** D14 and D15 are the orchestrator's, with the evidence named in `coverage.md`.

### Not in scope

- `BtnCW`/`BtnCCW` and `PopupBtnCW`/`PopupBtnCCW`. Their backlog row (`.sir/backlog.md:194`) is deferred work and
  stays as it is.
- The `NotebookView.xaml.cs:151` comment. It is prose about a rejected alternative, and T1 pins its digest.
- Any `Scale` value object, policy layer, `PolicyResult` or `DomainException`. MASTER-081's decision answers the
  deferred memory item, and nothing is built. D6 and D15 hold the code change to the renames.
- Audit text that quotes the old `obj` code (QOBJ: 15 lines). Those lines are records and are not edited.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| What unit did the "17 lines" count? | Lines of `grep -rnw obj` over Application, Infrastructure and Domain, minus 4 exempt `Entity.cs` lines. The 28 at startRef is drift, with 11 lambdas unchanged. | repo | `00-master-index.md:318` EXCLUSION 3 instrument; re-run at `91bbbf4` (C1 §4.3) |
| Is the `NotebookView.xaml.cs:151` comment a `ct` site? | No. It is a comment naming a rejected alternative's argument, and it stays as the one CT-WORD residue. | repo | the line's text; CT-WORD (C1 §4.1) |
| What does `obj` become? | `placedObject`, the lambda's subject. The repo names lambdas by subject (`floor =>`, `area =>`). | repo | `01-domain-application.md:1685` Suggested direction; `FloorMethods.cs:226`; `dotnet-format.md:42` |
| Do the two `DungeonReadRepository.cs` comments go with the rename? | Yes. They name the parameter and would go stale. | repo | `DungeonReadRepository.cs:91`, `:133` |
| Which agent owns the `ct` rename? | `sir-backend-engineer`. The rule is a naming convention, and one of the 18 files is production code. | judgment | — |
| Can two tasks build at once? | No. Every project writes `obj/` and `bin/` inside the shared worktree, so T1 → T2 is serial. T3 builds nothing but measures the post-state. | repo | worktree layout; startRef build |
| How is "EF model unchanged" proven? | `dotnet ef migrations has-pending-model-changes` reports no changes, and a `"REAL"` → `"TEXT"` injection (T2 J1) is the must-fail control. | repo | startRef probe, exit 0 (state.json `verification`) |
| What guards a stale `<param>` tag? | Only the TAG and TAG-NEW censuses. `GenerateDocumentationFile` is set nowhere, so the build cannot catch one. | repo | grep over `*.props`, `*.csproj` and `*.targets`: 0 lines |
| Where do build, test and EF figures in the register come from? | They are told, from `reviews/T1.md` and `reviews/T2.md`. T3 runs no `dotnet` command. | repo | C2 §4 |
| Does `ObjectScale` already exist? | Yes, as `public static class ObjectScale` (constants). The declined item is a value object. | repo | `src/DungeonMaster.Domain/Entities/Floors/ObjectScale.cs` |
| Worktree root | `C:/dm-wt/`, per `.sir/config.json` `git.worktreeRoot` (the WinUI MAX_PATH limit) | repo | `.sir/config.json` |

## Contracts

- [C1 — measurement instruments, baselines, build rules, editing hazards](contracts/C1-measurement.md)
- [C2 — register cells and amendment blocks](contracts/C2-register.md)

No contract declares a callable operation. Both contracts are instruments and wording rules.

## Open questions

None. Gate 2 decided Q1, Q2 and Q3 (see Scope).
