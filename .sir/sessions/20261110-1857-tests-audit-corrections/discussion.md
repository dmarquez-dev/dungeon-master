# Tests audit corrections

Correct four stale or inaccurate statements in the audit record, concentrated in `docs/audit/05-tests.md` (one row also reaches `docs/audit/06-cross-cutting.md`), taken from `.sir/backlog.md` rows `:95`, `:96`, `:108` and `:177`, and remove those four rows from the backlog at Gate 3.

## Context

The five scheduled headings at `.sir/backlog.md:5-13` are all empty. The previous session (20261110-1618-audit-record-corrections, pushed at `07f0512`) closed six audit-correction rows in `00-master-index.md` and `01-domain-application.md` and declared the older correction rows (`.sir/backlog.md:88-108`) and `05-tests.md` (`:177`) out of its scope. This session takes the rows whose target is `05-tests.md`, choosing by file concentration rather than severity, as recent sessions have.

Each row's target was spot-checked to still exist at discuss time (HEAD `07f0512`, `chore/repo-audit`, clean tree). These were existence checks, not re-measurements; every row must be re-measured in spec before a criterion is written on it.

- `.sir/backlog.md:95`: `grep -n 'src/\.\.\./Validators' docs/audit/05-tests.md` printed `:274`, `$ git show 413be78:src/.../Validators/AddAreaCommandValidatorTests.cs | sed -n '15,19p'`. The full path must be derived from `git show 413be78 --name-only` / `git ls-tree`, not guessed. Note the quoted command itself uses `sed -n`, which strips CR (lessons FORM 7); it reads content only, so that is not a defect in the quote, but spec should not reuse it to judge line endings.
- `.sir/backlog.md:96`: `grep -n 'did not move in between' docs/audit/05-tests.md` printed `:2239` ("same line it still stood at when T4 removed it, so it did not move in between. Authorship error, not"). The row asks to narrow the clause, since two measured endpoints do not establish the intervening path; spec decides whether the narrowing is a rewrite or a dated AMENDED insertion, following the register's own precedent.
- `.sir/backlog.md:108`: `grep -n DependencyInjection.cs docs/audit/05-tests.md docs/audit/06-cross-cutting.md` printed five lines: `05-tests.md:377`, `06-cross-cutting.md:117`, `:118`, `:130`, `:334`. The row says search by filename, not type name. Each citation's line numbers point into the old file and must be re-derived against `InfrastructureServiceCollectionExtensions.cs` at the base commit (or stated as of a named historical commit) — the filename swap alone would leave stale line anchors. Whether a historical citation should be rewritten or left as-of-its-commit with a note is a spec decision, made from register precedent.
- `.sir/backlog.md:177`: the TEST-023 (`05-tests.md:1670`), TEST-029 (`:2014`) and TEST-030 (`:2079`) headings exist. The row says `git show 2603170:` settles both UNDETERMINED classifications (P1 there prints 13 sites; `UpdatePlacedObjectCommandHandlerTests.cs` has `var obj = … .Which;` at :186), so they become DRIFT, and the TEST-029 block ends with a stray `]**` closer. Note `05-tests.md` legitimately uses `.**]**` as its amendment-block closer at many lines (e.g. `:115`, `:161`); spec must establish which closer form the stray one is and what the correct closer is from the file's own precedent (and lesson 2026-10-21 on the `**]**` vs plain `]` closer row at `.sir/backlog.md:144`), not from the row's wording.

## Decisions

- Track: Standard. Two files, but four rows carrying several independently checkable claims; per the "count the judgments" rule this is not Small.
- Session name: `tests-audit-corrections`.
- In scope: the four rows at `.sir/backlog.md:95`, `:96`, `:108` and `:177`, and nothing else.
- Out of scope: the other audit-correction rows (`.sir/backlog.md:88-94`, `:97-107`, `:186`); the session-path-citation questions (`:170`, `:178`); any source, test or build change; anything needing a running app or a human observation.
- The deliverable is audit prose only, so no build is needed for evidence, but each edited file's line-ending state must be proven by byte counts (lone CR = total `0d` minus `0d 0a`, measured before and after; lessons FORM 13 and edit-lone-cr), and a file carrying lone CR must not be written with `Edit`.
- Every corrected citation is re-derived by a command at the session's base commit and the command is stated with it (lesson: a correction is new prose and gets no exemption). Line anchors are quoted with their content, never derived by subtraction.
- Classifications that git history can settle (DRIFT vs ERROR vs UNDETERMINED) are settled by the orchestrator with `git show <sha>:<path>` before drafting criteria (lessons 2026-11-05 and the orchestrator-owns-git-evidence rule), since tasks cannot run git.
- Corrections follow the register's existing amendment convention (dated AMENDED insertions rather than silent rewrites) unless spec finds file precedent for an in-place fix of a mechanical defect such as an elided path or a stray closer; spec states which it chose per row and why.
- Each closed row is removed from `.sir/backlog.md` at Gate 3; `.sir/backlog.md` is measured for lone CR first. `:95`, `:96` and `:108` sit under `## Corrections to the audit record` and `:177` under `## Appended at Gate 3`; neither heading empties.
- Base branch is `chore/repo-audit`, not `main` (lesson 2026-11-09 20261109-1641-part-done-rows).
