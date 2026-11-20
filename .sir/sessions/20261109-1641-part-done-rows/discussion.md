# Part-done register rows: MASTER-081, 130

Clear the last scheduled backlog heading, `.sir/backlog.md:13` ("Part-done register rows: MASTER-081, 130"), so that both register rows leave the `part done` state: MASTER-130 by finishing its two residues, MASTER-081 by closing it as deliberately unchanged.

## Context

- Backlog rows covered: `.sir/backlog.md:15` (MASTER-081), `:16` (MASTER-130 summary), `:22` (`ct` residue, 2026-10-09 core-layer-batch), `:24` (`obj` residue, 2026-10-09 core-layer-batch). `:22` and `:24` duplicate halves of `:16`; all three go when MASTER-130 closes.
- Register rows: `docs/audit/00-master-index.md:269` (MASTER-081, CORE-016, part done 6b-xv) and `:318` (MASTER-130, CORE-024, part done 20261105-1907-core-heavy-rows, with EXCLUSION 2 = `ct`, EXCLUSION 3 = `obj`). Their detailed entries live in `docs/audit/01-domain-application.md` (CORE-016, CORE-024).
- Previous session (20261106-2050-detail-page-trio, Gate 1) recorded "081 decide later"; this session's Gate 1 made that decision.

Re-measurement at HEAD `91bbbf4`, 2026-11-09, run in the main checkout:

- `ct`: `grep -rnwE "CancellationToken ct" src/DungeonMaster.Tests src/DungeonMaster.App --include=*.cs --exclude-dir=obj --exclude-dir=bin | wc -l` prints 28, which matches the backlog's 28 (27 Tests + 1 App). By directory, 1 file is in `src/DungeonMaster.App/Assets/ObjectAssets` and the rest are under `src/DungeonMaster.Tests/Application/...` (files: 1 + 2 + 1 + 14, listed by `grep -rlwE ... | sed 's#/[^/]*$##' | sort | uniq -c`). Control: the same grep over `src/DungeonMaster.Application` prints 0, and `CancellationToken cancellationToken` there prints 46.
- `<param name="ct">`: `grep -rnF '<param name="ct">' src --include=*.cs --exclude-dir=obj --exclude-dir=bin | wc -l` prints 15, which matches the backlog's 15.
- `obj`: the record says "17 lines across 3 files". The three files exist as named: `Persistence/Configurations/FloorConfiguration.cs`, `Persistence/Configurations/PlacedObjectConfiguration.cs` and `Persistence/Repositories/DungeonReadRepository.cs`. `grep -rnwE "obj =>"` gives 1 + 9 + 1 = 11 lambda-head lines. `grep -cw obj` per file gives 2 + 18 + 8 = 28 lines containing the word. Neither is the record's unit. Settle which unit the 17 counted (the record's own instrument, in `docs/audit/00-master-index.md:318` or the CORE-024 AMENDED blocks) before calling any figure drift. `Migrations/` contains 0 `obj =>` files.
- MASTER-081: the status cell at `:269` is unchanged since 6b-xv. It reads: `ValidateScale` helper and `CanvasPoint` finiteness guard built; policy layer, `PolicyResult`, `DomainException` and `ObjectScale` deliberately not built.

## Decisions

- Track: Standard (two register rows across Tests, App, Infrastructure and the audit docs; one row is a design decision).
- Session name: `part-done-rows`.
- MASTER-130: finish both residues in this session.
  - Rename every remaining `ct` parameter in `DungeonMaster.Tests` and `DungeonMaster.App` to `cancellationToken`, and update its `<param name="ct">` tag in the same task so no tag goes stale.
  - Rename the non-descriptive `obj` lambda parameters in the three Infrastructure files (the whole recorded population, not only `DungeonReadRepository.cs`). Choose names that describe the lambda's subject.
  - Then close MASTER-130 in the register and amend CORE-024.
- MASTER-081: close as **DELIBERATELY UNCHANGED**, with no code change, in the form MASTER-145 uses (`docs/audit/00-master-index.md:333`). The reasoning to record: 6b-xv's `PlacedObject.ValidateScale` already removed the duplicated scale rule the row was about. `PolicyResult`, `DomainException` and an `ObjectScale` value object were considered and declined. Amend CORE-016 to match.
- Backlog effect at Gate 3: remove `.sir/backlog.md:15`, `:16`, `:22` and `:24`. Heading `:13` is then empty and kept, as earlier sessions kept emptied headings.
- Not in scope: the `BtnCW`/`BtnCCW` rotate-button renames (`.sir/backlog.md:194`). They are a separate row even though they cross-reference MASTER-130. The deferred "Scale value object" memory item is answered by the MASTER-081 decision; nothing is built.
- Carry the standing CRLF/whitespace process lessons into every dispatch (`sed -i`, `Write` on existing files, `Edit` trailing whitespace, lone CR). The source tree is CRLF.
