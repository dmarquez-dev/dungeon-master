# Tests -- code quality audit

## Scope

| Path or glob | Files | Lines |
|---|---|---|
| `src/DungeonMaster.Tests/App/Accessibility/**` | 1 | 260 |
| `src/DungeonMaster.Tests/App/Theme/**` | 12 | 4,430 |
| `src/DungeonMaster.Tests/Application/Common/Map/**` | 3 | 223 |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/**` | 14 | 1,324 |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/**` | 2 | 129 |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/**` | 4 | 220 |
| `src/DungeonMaster.Tests/Domain/Entities/**` | 5 | 478 |
| `src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/**` | 4 | 484 |
| **Total** | **45** | **7,548** |

Rolled up by sub-tree: `App/` 13 files / 4,690 lines; `Application/` 23 files / 1,896 lines;
`Domain/` 5 files / 478 lines; `Infrastructure/` 4 files / 484 lines.

**Files read: 45. Files cited in at least one record: 45.** Every `.cs` file in the scope table is
cited by at least one record below; no file was read and left unaccounted for.

Not audited, with the reason for each:

- `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` -- the contract assigns all five `.csproj`
  files to `XCUT` (T6). One record here (`TEST-032`) cites a single line of it because the
  observation is about the test suite's coverage tooling rather than about the project file's own
  quality; the file-level project record belongs to T6.
- `src/DungeonMaster.Tests/bin/**` and `src/DungeonMaster.Tests/obj/**` -- build output, not source.
- There is **no generated code in this project**: no EF Core migrations, no `*.Designer.cs`, no
  `*.g.cs`, and no XAML source-generator output. The test project does not reference
  `DungeonMaster.App`, so `MauiXamlInflator=SourceGen` produces nothing here. Every one of the 45
  files is hand-written and was audited against the same standard.

## Method

- **Base commit:** `45495c1bd39a52dbc942eceab897b3c1ac553ee9`, audited in the worktree
  `dungeon-master-worktrees/2026-08-17-code-quality-audit`.
- **`dm-conventions` documents read:**
  - `backend/dotnet-format.md` -- 794 lines, **read in full**, before any style, naming, formatting
    or XML-documentation record in this document was written. Its section 12, "Testing in C#", is
    the only C# testing guidance in that repository and is cited where it applies. Its
    `testing/` documents are Vitest and Playwright and are **not** cited anywhere here.
- **Microsoft Learn pages consulted (all fetched, not recalled):**
  - https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
  - https://learn.microsoft.com/en-us/ef/core/testing/
  - https://learn.microsoft.com/en-us/ef/core/managing-schemas/ensure-created
  - https://learn.microsoft.com/en-us/dotnet/standard/base-types/compilation-and-reuse-in-regular-expressions
  - https://learn.microsoft.com/en-us/dotnet/standard/base-types/best-practices-regex
  - https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
  - https://learn.microsoft.com/en-us/dotnet/standard/base-types/character-encoding-introduction
- **Other sources consulted:** https://xunit.net/docs/running-tests-in-parallel (fetched, for the
  default collection and parallelism behaviour used in the shared-state analysis).
- **Tooling run (read-only):**
  - `dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` from the worktree.
    Result: `Failed: 0, Passed: 292, Skipped: 0, Total: 292, Duration: 2 s`. Writes went only to
    `bin/` and `obj/`. The green run is reported as a fact about the suite's current state, not as
    evidence of its quality.
  - The same command re-run with `--filter FullyQualifiedName~<namespace>` once per sub-tree, to
    obtain test-case counts rather than method counts: `App/` 155, `Application/` 85, `Domain/` 38,
    `Infrastructure/` 14, summing to the 292 of the full run.
- **Shared-state and parallelism analysis** (the conclusion is stated in `## Layer verdict`; this is
  the working behind it). xUnit v2 defaults to one collection per test class and runs different
  classes in parallel -- confirmed against the fetched xunit.net page above. Every Infrastructure
  fixture opens its own `Data Source=:memory:` connection in its constructor, so each test method
  gets a private database, and there is no file-backed SQLite anywhere in the project. The only
  static state in the project is `RepoPaths.LazyRoot`, the compiled regexes, and the never-mutated
  allow-list `HashSet`s, all of which are immutable after initialisation. No test mutates state
  another test reads, so the default parallelism is safe here.
  - `git status --porcelain` after the run: no entry under `src/`.
  - `grep`/`find` inventories of indentation characters, XML-doc presence, and the
    `src/DungeonMaster.Application` and `src/DungeonMaster.Infrastructure` type inventories, to
    establish which production types have no test at all.

## Summary

| Severity | Count |
|---|---|
| S1 | 0 |
| S2 | 4 |
| S3 | 17 |
| S4 | 9 |
| S5 | 2 |
| **Total** | 32 |

## Records

**[AMENDED 6b-ii, 2026-09-08 -- what this pass did and did not establish.** Seven mis-citations were
corrected in place, across `TEST-002`, `TEST-008`, `TEST-009`, `TEST-012`, `TEST-013` and `TEST-026`,
plus one cross-reference in `## Layer verdict`. Every figure in those amendments was measured against
the fixed commit `850a10b` on 2026-09-08 by DIR session `2026-09-08-fixtures-and-de-duplication`
(6b-ii), and every command is quoted with its anchor so a later reader can paste it and get the stated
number back.

This document was authored at `2603170` on 2026-08-17
(`git log --diff-filter=A -- docs/audit/05-tests.md`) and has not been edited since
(`git log --oneline 2603170..850a10b -- docs/audit/05-tests.md | wc -l` -> `0`), while **28** commits
have touched the test project in the meantime
(`git log --oneline bcb49dc^..850a10b -- src/DungeonMaster.Tests | wc -l` -> `28`; the `..HEAD` form
returns the same 28 when run at `850a10b` but drifts as the branch moves, so the fixed anchor is the
one recorded).
Each amendment therefore states whether the claim held **at `2603170`** -- DRIFT if it did,
MEASUREMENT ERROR if it did not, UNDETERMINED if the evidence does not settle it -- and that
classification is itself measured, never assumed.

**The outcome of that test: six of the seven corrections are DRIFT, and the repository moved mostly on
2026-08-18, the day after this document was written.** The exceptions are two genuine errors of
authorship, recorded as such: `TEST-012`'s subsumption reasoning, which was wrong at `2603170` and
would have destroyed real coverage if acted on, and `TEST-013`'s title.

**The remaining entries were NOT re-measured.** This document is now correct in seven places that were
checked; nothing here licenses trusting the rest. `MASTER-020`, `MASTER-070`, `MASTER-086`,
`MASTER-155` and `MASTER-167` were deliberately left alone -- they are *suspected* stale, which is not
the same as *measured* stale, and a correction not backed by a command in this pass would be the exact
defect this pass exists to correct.**]**

**[AMENDED 6b-iii, 2026-09-11 -- what THIS pass re-measured, and what it still did not.** The block
above is 6b-ii's and is left exactly as it stood. DIR session
`2026-09-11-measure-then-close-the-test-gaps` (6b-iii) took the five entries 6b-ii deliberately left
alone and re-measured them, each by an independent read-only agent against a single fixed commit,
`1407090`, before any decomposition. **Four records in this document were re-measured and corrected
in place: `TEST-001`, `TEST-019`, `TEST-020` and `TEST-032`.** (`MASTER-167`, the fifth, has no
source record here; its correction is written into its narrative entry in
`docs/audit/00-master-index.md`.)

**The remaining 28 of this document's 32 records were NOT re-measured by this pass either.** Between
6b-ii and 6b-iii, ten records here have now been checked -- `TEST-001`, `TEST-002`, `TEST-008`,
`TEST-009`, `TEST-012`, `TEST-013`, `TEST-019`, `TEST-020`, `TEST-026` and `TEST-032`. Nothing about
the other 22 has been confirmed; they are unchecked, which is not the same as correct. This is the
same distinction 6b-ii drew about itself, and the one the master index's own inheritance note --
*"What 6b-iii must do before it decomposes anything"* -- demands before any of these entries is acted
on. That note is at `docs/audit/00-master-index.md:536` after this session's appends; it stood at
`:522` at `1407090`, which is the number the 6b-ii block above still cites.

**The outcome of this pass, and it differs from 6b-ii's.** 6b-ii found six of seven corrections were
DRIFT. Here too most corrections are DRIFT, but **three clauses were wrong when written**: `TEST-020`'s
`AddFloor`-boundary clause (refuted by the entry's own first cited location), `TEST-020`'s
"`PlacedObjectTests` is the exception" clause (refuted by tests predating the audit by three months),
and `TEST-032`'s cited line number. A further correction has no drift-or-error verdict at all and
says so: `TEST-032`'s suggested `[assembly: CollectionBehavior]` remedy is a **measured regression**
today (~27% nondeterministic), which was not measurable on the audit date.

**Where this pass could not settle a classification it wrote `UNDETERMINED` rather than guessing, and
five such classifications were then measured rather than guessed.** Round 1 left five clauses
unsettled -- three in `TEST-001` (the `Common/Map/` directory inventory, the four-validator-class
count and the `AddAreaCommandValidatorTests.cs:17` anchor) and two in `TEST-032` (the singular *"the
test project"*, left unstated inside CORRECTION 1, and the unpinned-parallelism claim of CORRECTION
3) -- because a DIR task may not invoke git and the evidence it held established nothing about
2026-08-17. Round 2 settled all five against the audit commit `413be78`, by read-only commands run by
this session's orchestrator, which is not so barred: **every one of the five is DRIFT** -- accurate
when written, overtaken since. Each command and its output is quoted at the correction it settles and
recorded in
`.dir-sdlc/sessions/2026-09-11-measure-then-close-the-test-gaps/handover-addendum.md`. One
classification still carries no drift-or-error verdict and says why -- `TEST-032` CORRECTION 2, whose
subject could not exist on the audit date. An unmeasured classification asserted confidently would be
the exact defect these passes exist to correct, and this session produced **four** such citation
errors in its own working artifacts before catching them, each caught by someone downstream of
whoever wrote the number -- the four enumerated at
`.dir-sdlc/sessions/2026-09-11-measure-then-close-the-test-gaps/handover.md:200-203`, the artifact
that figure is counted from. A **fifth** was caught in this document's own review: the `8bb2d79`
mis-citation, corrected at `TEST-032` CORRECTION 1 below.**]**

### TEST-001 -- The dispatcher, the validation pipeline and the Result type have no tests at all (FALSIFIED -- all three were tested on 2026-08-18; see CORRECTION 1)

```yaml
id: TEST-001
layer: TEST
severity: S2
category: testability
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:  # AMENDED 6b-iii -- the AddAreaCommandValidatorTests.cs:17 anchor is dead; see CORRECTION 6
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:1
  - src/DungeonMaster.Application/Common/Behaviors/ValidationBehavior.cs:1
  - src/DungeonMaster.Application/Common/Results/Result.cs:1
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs:17
effort: M
risk: low
independent: true
```

**What.** `src/DungeonMaster.Tests/Application/` contains `Common/Map/` and `Features/Dungeons/`
and nothing else: there is no test anywhere in the project for `Dispatcher`, for
`ValidationBehavior`, or for `Result`. The four validator test classes invoke
`_validator.Validate(command)` directly, so they prove a rule object's rules and never that the
rule object is reached by the pipeline that is supposed to run it.

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 1, the title and the headline claim. Classification: DRIFT.**
Previous wording, verbatim, as the record's title: *"The dispatcher, the validation pipeline and the
Result type have no tests at all"*. All three types have behaviour-level tests, counted from this
session's `measurement.md` at the fixed commit `1407090`: `DispatcherTests.cs` (**6** facts, over a
real `ServiceCollection`, asserting behaviour ordering -- `first:enter, second:enter, handler,
second:exit, first:exit` -- and invoker-cache identity), `ValidationBehaviorTests.cs` (**11** facts),
`ResultTests.cs` (**5**), `ResultOfTTests.cs` (**8**), `ResultMessageTests.cs` (**9**).

**Classification evidence.** They were added on **2026-08-18** -- the day after this document was
written -- by `09f4360`, `f9a665b` and `3b44d91`, and `3b44d91` also moved `Result<T>` to its own
file `Common/Results/ResultOfT.cs`. The entry was right on the day and the repository moved under it
the following day. This is the same one-day staleness pattern 6b-ii recorded for `TEST-002`.**]**

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 2, the directory inventory. Classification: DRIFT.**
Previous wording, verbatim: *"`src/DungeonMaster.Tests/Application/` contains `Common/Map/` and
`Features/Dungeons/` and nothing else"*. At `1407090` there is **no `Common/Map/` under that path at
all**, and the directory is not the two-entry inventory the sentence describes -- `Common/Dispatch/`,
`Common/Behaviors/`, `Common/Results/` and `Common/Handlers/` are all present, which is precisely why
CORRECTION 1 holds.

**Classification evidence.** *Measured at the audit commit.* The two-entry inventory was **correct
when written**: at `413be78` (2026-08-17, the audit commit, the same day this document was authored
at `2603170`), `Common/` held `Map` and nothing else and `Features/` held `Dungeons` and nothing
else --

```
$ git ls-tree -r --name-only 413be78 -- src/DungeonMaster.Tests/Application/Common/ | sed 's|.*/Common/||' | cut -d/ -f1 | sort -u
Map
$ git ls-tree -r --name-only 413be78 -- src/DungeonMaster.Tests/Application/Features/ | sed 's|.*/Features/||' | cut -d/ -f1 | sort -u
Dungeons
```

-- so the sentence is overtaken, not mistaken. `MASTER-027` / `MASTER-087` / `MASTER-082` moved that
surface afterwards. These commands were run read-only by this session's orchestrator, which is not
barred from git as its tasks are, and are recorded verbatim in
`.dir-sdlc/sessions/2026-09-11-measure-then-close-the-test-gaps/handover-addendum.md`; the round-1
pass held no evidence at the authoring commit and wrote `UNDETERMINED` rather than guess.**]**

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 3, "no test anywhere". Classification: DRIFT.**
Previous wording, verbatim: *"there is no test anywhere in the project for `Dispatcher`, for
`ValidationBehavior`, or for `Result`"*. Falsified by the five files named in CORRECTION 1, including
`ValidationBehaviorTests.HandleAsync_WithOneFailure_ReturnsValidationFailureAndDoesNotCallNext`,
which asserts `nextCalled == false` -- the short-circuit the "Why it matters" paragraph below says
nothing would catch.

**Classification evidence.** Same three commits, same date: `09f4360`, `f9a665b`, `3b44d91`,
2026-08-18.**]**

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 4, the validator test class count. Classification: DRIFT.**
Previous wording, verbatim: *"The four validator test classes"*. At `1407090` there are **14**,
counted from this session's `measurement.md`.

**Classification evidence.** *Measured at the audit commit.* Four was **correct when written**:

```
$ git ls-tree -r --name-only 413be78 -- src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/ | wc -l
4
```

-- `AddAreaCommandValidatorTests.cs`, `CreateDungeonCommandValidatorTests.cs`,
`UpdateAreaCommandValidatorTests.cs`, `UpdateDungeonCommandValidatorTests.cs`, at `413be78`
(2026-08-17). The ten further classes arrived afterwards; the count is overtaken, not an authorship
undercount. Command run read-only by this session's orchestrator and recorded in
`.dir-sdlc/sessions/2026-09-11-measure-then-close-the-test-gaps/handover-addendum.md`.**]**

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 5, "never that the rule object is reached by the pipeline". Classification: DRIFT.**
Previous wording, verbatim: *"they prove a rule object's rules and never that the rule object is
reached by the pipeline that is supposed to run it"*. **Three `SendAsync_...IsRejectedBy...` facts
prove exactly that** -- counted from this session's `measurement.md` at `1407090` -- dispatch through
`IDispatcher` end to end against a deliberately failing validator -- which is the "Suggested direction" paragraph below, already implemented.

**Classification evidence.** Those facts live in the files added on 2026-08-18 by `09f4360` /
`f9a665b`; see CORRECTION 1.**]**

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 6, the dead location anchor. Classification: DRIFT.**
Previous wording, verbatim, as the fourth entry of `locations`:
*`src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs:17`*.
At `1407090` that line lands on `protected override string NamePropertyName` -- a member of an
extracted shared validator test base, not the direct `_validator.Validate(command)` call the entry
cites it for. The anchor is dead as a citation.

**Classification evidence.** *Measured at the audit commit.* The anchor was **live when written** --
line 17 of that file at `413be78` (2026-08-17) was exactly the `_validator.Validate(command)` call
the entry cites it for:

```
$ git show 413be78:src/.../Validators/AddAreaCommandValidatorTests.cs | sed -n '15,19p'

		// Act
		var result = _validator.Validate(command);

		// Assert
```

The anchor died afterwards, when `8784326` (6b-ii) reshaped the file by base-class extraction. Drift,
not a mis-cited line. Command run read-only by this session's orchestrator and recorded in
`.dir-sdlc/sessions/2026-09-11-measure-then-close-the-test-gaps/handover-addendum.md`.**]**

**Why it matters.** The Microsoft guidance is that tests exist to protect against regression in the
code most likely to change, and the dispatcher is the highest-risk code in the Application layer: it
resolves handlers by reflection over closed generic types and composes behaviours through `dynamic`,
so a mistake there is invisible to the compiler in a way none of the tested handlers are. Today a
validator could be deleted from dependency injection, or `ValidationBehavior` could stop short-
circuiting on failure, and all 292 tests would still pass.

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 7, the 292-test premise, which is the premise of the S2 rating. Classification: DRIFT.**
Previous wording, verbatim: *"Today a validator could be deleted from dependency injection, or
`ValidationBehavior` could stop short-circuiting on failure, and all 292 tests would still pass."*
Both halves are false at `1407090`. The short-circuit is asserted directly by
`ValidationBehaviorTests.HandleAsync_WithOneFailure_ReturnsValidationFailureAndDoesNotCallNext`
(`nextCalled == false`), and a validator removed from dependency injection is caught by the three
`SendAsync_...IsRejectedBy...` dispatch facts. This sentence is not incidental: it **is** the
argument for the `S2` severity, so the rating rests on a premise that stopped holding on 2026-08-18
(`09f4360`, `f9a665b`, `3b44d91`). The severity is left at `S2` as the historical record; a reader
re-rating this entry today should know the premise is gone.**]**

**Suggested direction.** Add a small set of tests that construct a real `ServiceProvider` with one
fake command, one handler and one deliberately failing validator, and dispatch through
`IDispatcher` end to end, asserting both the success path and the validation-failure `Result`.

**[AMENDED 6b-iii, 2026-09-11 -- STATUS: `MASTER-020` is CLOSED BY EVIDENCE, with NO code work.**
This entry's "Suggested direction" was implemented on 2026-08-18 and nobody noticed, which is the
whole finding. 6b-iii wrote **no test and changed no production code** for it; the close is a record
correction and the evidence is the five files named in CORRECTION 1, counted from this session's
`measurement.md` at `1407090`.

**Two residuals were found while establishing this, and they are NOT part of this entry.** They were
recorded as explicitly not to be folded into it, because a close that quietly absorbs adjacent gaps
is how a gap stops being tracked: `Dispatcher`'s own `CancellationToken` propagation is unasserted,
and `Dispatcher`'s missing-handler failure mode is untested. They are filed as **`MASTER-172`** and
**`MASTER-173`** in `docs/audit/00-master-index.md`. `MASTER-020`'s closure does not claim either,
and neither does `MASTER-070`'s -- see `TEST-019`.**]**

**[AMENDED 6b-x, 2026-09-18 -- BOTH RESIDUALS ARE NOW CLOSED, and the decision above is vindicated.**
`MASTER-172` and `MASTER-173` were both closed by session 6b-x: commit `b0fa532`, merged `98326d4`.
Two new files in `src/DungeonMaster.Tests/Application/Common/Dispatch/` -- `DispatcherCancellationTests.cs`
(6 `[Fact]`) and `DispatcherMissingHandlerTests.cs` (3) -- with **no production code change**
(`Dispatcher.cs` byte-identical at `sha256 e748d9ea…0375`). Solution suite 960 -> 969, 0 failed,
0 warnings, verified in the main checkout after the merge.

**6b-iii was right to refuse to fold these into `MASTER-020`'s closure, and 6b-x measured why.** Both
residuals were re-measured against `506cbba` before this session decomposed anything, and **both still
held** -- the first inherited claims in six sessions to survive re-measurement unchanged. Had they been
absorbed in 2026-09-11, the propagation gap would have been closed by a claim nobody could check: at
`506cbba` the dispatch tests carried 6 `[Fact]`, **none** of which passed a token, and across all three
test projects there were 12 `CancellationTokenSource` sites, **none** in `Application/Common/Dispatch/`.

**The `MASTER-173` closure carries a distinction this note is the right place to keep.** It was a
**contract-coverage gap, not a reachable defect**. The startup self-check at
`ApplicationServiceCollectionExtensions.ValidateHandlerRegistrations` makes the Dispatcher's runtime
missing-handler path unreachable for every real command in the real container -- proven by
`ApplicationServiceRegistrationTests.cs:29`, `:129` and `:169`. A reader who finds
`ApplicationServiceRegistrationTests.cs:46`
(`ValidateHandlerRegistrations_WithAMissingHandler_ThrowsNamingTheCommand`) and concludes `MASTER-173`
was already covered has found the **near-miss**, not the entry: that test exercises the self-check and
never constructs or invokes `Dispatcher`. 6b-x read it in full before concluding the gap was real, and
the distinction is why the entry stayed `S3` rather than being re-rated in either direction.

**Coverage went wider than `MASTER-172`'s wording.** The entry names only "the handler it resolves";
the session also asserted the token reaching the **pipeline behaviour** on all three paths, because the
three invoker classes share no code on that axis and a dropped token is silent rather than throwing.
**One residual remains and is in `.dir-sdlc/backlog.md`, deliberately not folded in here** -- the same
discipline this block exists to record: no fact covers the *handler's* token when a behaviour is also
registered, so a defect dropping it only on that path would pass all nine new facts.**]**

### TEST-002 -- Every persistence test builds its schema with EnsureCreated, so no test exercises a migration

```yaml
id: TEST-002
layer: TEST
severity: S2
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/managing-schemas/ensure-created
locations:  # AMENDED 6b-ii -- seven, not four; line anchors re-resolved at 850a10b
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:28
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:25
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs:31
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:31
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/UnitOfWorkTests.cs:31
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Configurations/AreaOutlineChangeTrackingTests.cs:32
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Configurations/PlacedObjectConfigurationTests.cs:34
effort: S
risk: low
independent: true
```

**What.** All seven Infrastructure test classes create their schema with
`context.Database.EnsureCreated()`. Production creates its schema with `db.Database.Migrate()`
(`src/DungeonMaster.Infrastructure/DependencyInjection.cs:50`). `EnsureCreated` builds the schema
from the current model, bypassing the three migrations under
`src/DungeonMaster.Infrastructure/Migrations/` entirely.

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 1, the class count. Classification: DRIFT.**
Previous wording, verbatim: *"All four Infrastructure test classes create their schema with
`context.Database.EnsureCreated()`"*, over a `locations` list of four files. The measured figure is
**seven call sites in seven distinct classes**:

```
git grep -n "EnsureCreated" 850a10b -- src/DungeonMaster.Tests
```

-> 7 lines, `| wc -l` -> `7`; `git grep -l ... | wc -l` -> `7`. Three of the seven appear in **neither**
this entry's nor `TEST-009`'s list -- `Repositories/UnitOfWorkTests.cs`,
`Configurations/AreaOutlineChangeTrackingTests.cs`, `Configurations/PlacedObjectConfigurationTests.cs`
-- and two of those three live under `Configurations/` rather than `Repositories/`, which is why a
directory-scoped reading of either entry misses them. All three are added to both lists.

**Classification evidence.** The same command at the authoring commit,
`git grep -n "EnsureCreated" 2603170 -- src/DungeonMaster.Tests`, returns exactly **four**, and they are
exactly the four listed. `git log --diff-filter=A --format="%h %ad %s" --date=short -- <file>` on each
missed file returns `ef0b0b6 2026-08-18` for `UnitOfWorkTests.cs` and `1453cbd 2026-08-18` for both
`Configurations/` files. **None of the three existed when this entry was written on 2026-08-17.** The
entry was right on the day; the repository grew under it the following day. This is drift, not a
counting error, and the same finding applies unchanged to `TEST-009` and `TEST-026`.**]**

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 2, the migration count. Classification: DRIFT.**
Previous wording, verbatim: *"bypassing the two migrations under
`src/DungeonMaster.Infrastructure/Migrations/` entirely"*. The measured figure is **three**:

```
git ls-tree -r --name-only 850a10b -- src/DungeonMaster.Infrastructure/Migrations/   | grep '\.cs$' | grep -v Designer | grep -v Snapshot | wc -l
```

-> `3`. Named, so the count is checkable rather than merely restated:
`20260805014736_InitialCreate`, `20260805184106_AddFloorDescription`,
`20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault`.

**Classification evidence.** The same command at `2603170` lists **two** -- `InitialCreate` and
`AddFloorDescription`. The third migration's own timestamp prefix is `20260818`, one day after this
document was authored. The entry was correct when written.**]**

**Why it matters.** The cited page states plainly that `EnsureCreated` and Migrations "don't work
well together" and that a project intending to use Migrations should not use `EnsureCreated` to
initialize the schema. The consequence here is a specific and reachable one: a model change shipped
without its migration -- a new property, a changed column type, a renamed table -- produces a green
suite, because the tests build the schema from the model that already contains the change, and then
fails on the first real user's existing database, where the schema comes from the migration chain
that does not.

**Suggested direction.** Switch the four fixtures to `Database.Migrate()` against the SQLite
in-memory connection so the migration chain is what is under test, and consider one additional test
that asserts the model and the migrations have not diverged.

### TEST-003 -- RepoPaths accepts any ancestor with a .git entry without checking the App project is beneath it

```yaml
id: TEST-003
layer: TEST
severity: S2
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/RepoPaths.cs:51-76
  - src/DungeonMaster.Tests/App/Theme/RepoPaths.cs:19-23
  - src/DungeonMaster.Tests/App/Theme/RepoPaths.cs:8
effort: XS
risk: low
independent: true
```

**What.** `FindRoot` walks up from `AppContext.BaseDirectory` and returns the first directory
containing a `.git` file or directory. It never checks that `src/DungeonMaster.App` exists under
what it returned. `AppRoot` is then composed from that result and handed straight to
`Directory.EnumerateFiles`. The result is cached in a static `Lazy<string>`, which caches the
exception too if the walk runs off the top of the drive.

**Why it matters.** I believe this makes the entire twelve-class theme sub-suite dependent on an
ambient property of the machine it runs on rather than on anything in the repository. Two layouts
appear to break it: a checkout with no `.git` at all -- a source archive, or a CI job that fetches a
tarball rather than cloning -- where the walk throws `InvalidOperationException` from a static
initialiser and every theme fact fails with a message about a missing repository root; and a
checkout nested inside an outer repository, where the walk can stop at the wrong root and
`EnumerateFiles` throws `DirectoryNotFoundException` from within a test body. In this session's
linked worktree it resolves correctly -- the `.git`-as-a-file case is handled and commented at
`RepoPaths.cs:57-61`, and the 292-test run confirms it -- so the defect I am describing is the
absence of a check, not a failure observed today.

**Suggested direction.** Make the loop's acceptance condition "this directory has a `.git` entry
**and** contains `src/DungeonMaster.App`", keep walking otherwise, and have the failure message name
the directory it started from and what it was looking for.

### TEST-004 -- Eleven of the thirteen App files assert over XAML as text; a twelfth parses it as XML

```yaml
id: TEST-004
layer: TEST
severity: S2
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:18-56
  - src/DungeonMaster.Tests/App/Theme/ThemeDictionaryTests.cs:15-18
  - src/DungeonMaster.Tests/App/Theme/ThemeTokenParityTests.cs:17-35
  - src/DungeonMaster.Tests/App/Theme/ImplicitStyleTests.cs:18-26
  - src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs:31-34
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:378
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:492-494
effort: L
risk: medium
independent: true
```

**What.** Eleven of the thirteen files under `App/` read `.xaml` files as strings and match regular
expressions against them. The twelfth, `TokenTypeMatchesTargetPropertyTests`, reads the same files
with `XDocument.Parse`, walks
`Descendants()`, reads attributes by `XName`, and gets element identity, namespace qualification,
ancestor context and accurate line numbers from `LoadOptions.SetLineInfo` for free. The thirteenth,
`App/Theme/RepoPaths.cs`, is a path helper that makes no XAML assertion at all, so the comparison is
between eleven regex classes and one XML one.

**Why it matters.** I believe the regex approach produces a class of silent false pass that the XML
approach cannot. `ControlTag` at `SemanticDescriptionTests.cs:33` matches `[^>]*?` for a tag's
attributes, so an attribute value containing `>` truncates the tag and the control is never
examined; `StylePattern` at `ImplicitStyleTests.cs:20` closes on the first `</Style>` with
`.*?`, so a nested style would mis-terminate the body and its setters would be attributed to the
wrong target; `SemanticDescriptionTests` only sees double-quoted attributes, so a single-quoted
`Text='...'` is invisible. None of those failures announce themselves -- the scan simply reaches
fewer controls and reports green. The suite's own authors clearly know this: nine classes carry
explicit anti-vacuity assertions whose only purpose is to detect a scan that has stopped reaching
anything, which is a symptom of the parsing strategy rather than a fix for it. That
`TokenTypeMatchesTargetPropertyTests` does the harder job with `System.Xml.Linq` in fewer lines
than several of the regex classes is the strongest evidence available that the alternative is
affordable.

**Suggested direction.** Move the shared scan in `ThemeSource` onto `XDocument`, expose keyed
elements and attributes as objects, and let each fact query that model. The anti-vacuity guards
stay useful but stop being the only thing standing between the suite and silence.

### TEST-005 -- Compiled Regex objects are constructed inside per-item loops rather than reused

```yaml
id: TEST-005
layer: TEST
severity: S3
category: performance
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/base-types/compilation-and-reuse-in-regular-expressions
locations:
  - src/DungeonMaster.Tests/App/Theme/ThemeDictionaryTests.cs:304-310
  - src/DungeonMaster.Tests/App/Theme/ImplicitStyleTests.cs:214-217
  - src/DungeonMaster.Tests/App/Theme/BindablePropertyDefaultsMatchTokensTests.cs:84-86
  - src/DungeonMaster.Tests/App/Theme/BindablePropertyDefaultsMatchTokensTests.cs:129-131
effort: S
risk: low
independent: true
```

**What.** `NoOrphanedStyleKeys` constructs two `new Regex(..., RegexOptions.Compiled)` instances on
every iteration of its per-style loop. `ImplicitStyleTests.Names` builds an interpolated pattern and
passes it to the static `Regex.Matches` on every call. The two `BindablePropertyDefaults` theories
construct a compiled `Regex` per theory case.

**Why it matters.** The cited page states that `RegexOptions.Compiled` raises construction cost in
exchange for lower match cost, and that "creating and rapidly destroying numerous `Regex` objects is
an expensive process". Constructing a compiled regex inside a loop pays the JIT cost per iteration
and collects none of the benefit, which is the exact inversion of what the option is for. The same
page notes the static-method cache holds only 15 patterns by default, so `ImplicitStyleTests.Names`
is also at risk of cache eviction if the interpolated element names ever grow.

**Suggested direction.** Hoist the two loop-constructed patterns out of the `foreach` (or replace
the per-key regex with a single pass that collects all `{StaticResource ...}` keys into a set once
and tests membership), and drop `RegexOptions.Compiled` from any pattern that is genuinely
short-lived.

### TEST-006 -- The accessibility scan truncates supplementary-plane character entities with a char cast

```yaml
id: TEST-006
layer: TEST
severity: S3
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/base-types/character-encoding-introduction
locations:
  - src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs:222-235
  - src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs:243-259
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:205-220
effort: XS
risk: low
independent: true
```

**What.** `SemanticDescriptionTests.Decode` converts a `&#xNNNN;` entity with
`(char)int.Parse(...)`. `IsIconOnly` then rejects anything whose `Length != 1`.
`IconFontCoverageTests.Decode`, doing the same job three files away, uses
`char.ConvertFromUtf32`.

**Why it matters.** The cited page explains that a code point above `U+FFFF` requires a surrogate
pair and cannot be held in one `char`. An entity such as `&#x1F6E1;` therefore either wraps to an
unrelated BMP character or, at `int.Parse` boundaries, is silently mis-decoded; either way the
control is measured against the wrong codepoint, and the surrogate-pair form is rejected outright by
the `Length != 1` test. The consequence is an icon-only control in the supplementary plane passing
the accessibility ratchet without a `SemanticProperties.Description`, which is precisely the state
the class exists to prevent. That the sibling class already does this correctly makes the
divergence, rather than the technique, the finding.

**Suggested direction.** Replace the cast with `char.ConvertFromUtf32`, change `IsIconOnly` to
enumerate runes and require exactly one, and consider lifting the single correct `Decode` into a
shared helper so the two cannot drift again.

### TEST-007 -- The four validator test classes are identical except for the command type

```yaml
id: TEST-007
layer: TEST
severity: S3
category: duplication
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Duplicated Code"
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs:6-55
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/CreateDungeonCommandValidatorTests.cs:6-55
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateAreaCommandValidatorTests.cs:6-55
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateDungeonCommandValidatorTests.cs:6-55
effort: S
risk: low
independent: true
```

**What.** All four validator test classes are 55 lines with the same three tests -- valid command
passes, a three-case `[Theory]` on empty name, and a 101-character name -- differing only in the
command record they construct. The four validators they test are themselves identical: a single
`RuleFor(command => command.Name).NotEmpty().MaximumLength(100)`.

**Why it matters.** Fowler's treatment of duplicated code applies directly: the cost is not the
lines, it is that the next rule added to one validator must be remembered in four places, and that
the four files hide rather than expose the fact that four commands share one rule. A reader
comparing the four cannot tell whether the sameness is deliberate or accidental.

**Suggested direction.** Collapse the four into one generic abstract base parameterised by command
type with an abstract factory for "command with this name", or a single `[Theory]` over the four
validators, so a new rule has one place to be tested and a divergence between the four validators
becomes visible.

### TEST-008 -- Sixteen handler test classes repeat an identical fixture constructor and an identical not-found test

```yaml
id: TEST-008
layer: TEST
severity: S3
category: duplication
authority: conventions
citation: backend/dotnet-format.md#12. Testing in C#
locations:  # AMENDED 6b-ii -- all sixteen ranges re-resolved at 850a10b; see CORRECTION 4(a)
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs:13-22
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddFloorCommandHandlerTests.cs:13-22
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddPlacedObjectCommandHandlerTests.cs:18-30
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/CreateDungeonCommandHandlerTests.cs:15-27
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/DeleteDungeonCommandHandlerTests.cs:15-27
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemoveAreaCommandHandlerTests.cs:13-22
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemoveFloorCommandHandlerTests.cs:13-22
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemovePlacedObjectCommandHandlerTests.cs:18-30
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/ReorderFloorCommandHandlerTests.cs:13-22
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateAreaCommandHandlerTests.cs:13-22
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateAreaOutlineCommandHandlerTests.cs:14-23
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateDungeonCommandHandlerTests.cs:13-22
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateFloorCommandHandlerTests.cs:13-22
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs:18-30
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonDetailQueryHandlerTests.cs:10-17
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonsQueryHandlerTests.cs:13-23
effort: M
risk: low
independent: true
```

**What.** Sixteen handler test classes declare the same readonly fields and the same constructor
body. The **fourteen** command classes declare `new UnitOfWorkSubstitute()`,
`_unitOfWork.Dungeons` and `new XHandler(_unitOfWork.Factory)`; the **two** query classes declare
`Substitute.For<IDungeonReadRepository>()` and `new XHandler(_readRepository)`. Twelve of the
fourteen command classes then carry a not-found test whose body is the same four statements with a
different command record. Twelve is the count on the behavioural reading used throughout this
record: a test that asserts `ErrorCategory.NotFound`. Only nine of those twelve are named
`HandleAsync_WithNonExistent...`; the other three state the same case under a different name.
`CreateDungeonCommandHandlerTests` is the only command class with no not-found test.

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 4(a), the constructor this entry describes no longer
exists. Classification: DRIFT.** Previous wording, verbatim: *"Sixteen handler test classes declare
the same two readonly fields and the same constructor body -- `Substitute.For<IDungeonWriteRepository>()`
(or `IDungeonReadRepository`) and `new XHandler(_writeRepository)`"*, with the sixteen `locations`
line ranges above. **The fourteen command classes no longer have that constructor.** At `850a10b` all
fourteen wire through the extracted `Application/Common/Handlers/UnitOfWorkSubstitute.cs` -- *"The one
place the substituted unit-of-work chain is wired"* -- as `new UnitOfWorkSubstitute()` plus
`new HANDLER(_unitOfWork.Factory)`:

```
git grep -c "new UnitOfWorkSubstitute()" 850a10b -- 'src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/*.cs' | wc -l
git grep -n "_unitOfWork.Factory)" 850a10b -- 'src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/*.cs' | wc -l
```

-> `14` and `14`. The two `Queries/` classes still use the described shape. The duplication this entry
records is therefore already partly consolidated -- what remains is the per-class constructor body,
not the wiring. **The live `**What.**` paragraph above is corrected in place accordingly**, so that a
reader meeting it before this block is not told something false; its previous wording is preserved
verbatim at the head of this amendment.

**All sixteen `locations` ranges were stale to match, and all sixteen are re-resolved above.** They
were measured, not estimated, with

```
git show 850a10b:<file> | awk '/^\tprivate readonly/ && !s {s=NR} /^\t\}$/ && s {print s"-"NR; exit}'
```

run over each of the sixteen files, which returns the fields-plus-constructor block bounded by the
first `private readonly` line and the constructor's closing brace. Twelve of the fourteen command
ranges moved by one line, the four `PlacedObject`/`CreateDungeon`/`DeleteDungeon` shapes by more, and
**both `Queries/` ranges were already correct and are unchanged** -- `:10-17` and `:13-23` resolve at
`850a10b` exactly as cited, which is consistent with 4(a): the two query classes never took the
`UnitOfWorkSubstitute` rewrite.

**Classification: DRIFT**, on the same evidence as the rest of this block -- the three sampled ranges
reproduce the described constructor exactly at `2603170`, so they were accurate when written.

**Method note.** This re-resolution was an advisory finding from this task's review, not part of the
briefed correction set: five of the six corrected entries had their line anchors re-resolved and this
one did not, which was an inconsistency in how the method was applied rather than a defect the brief
named. It is recorded here so that the asymmetry is visible rather than silently repaired.

**Classification evidence.** `git show 2603170:<file>` piped to `sed -n '<range>p'` reproduces the
described constructor exactly at the cited ranges -- checked on `AddAreaCommandHandlerTests.cs:12-19`,
`AddPlacedObjectCommandHandlerTests.cs:17-27` and `GetDungeonsQueryHandlerTests.cs:13-23`, each
returning the two readonly fields and the `Substitute.For<...>` / `new XHandler(...)` body. The entry
was accurate when written. `UnitOfWorkSubstitute.cs` was added by
`git log --diff-filter=A --format="%h %ad %s" --date=short -- src/DungeonMaster.Tests/Application/Common/Handlers/UnitOfWorkSubstitute.cs`
-> `ef0b0b6 2026-08-18`, the day after.**]**

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 4(b), the stated exclusions are wrong. Classification:
DRIFT.** This is a separate defect from 4(a) and is deliberately not folded into that block: a stale
description and a false exclusion are different kinds of error and a reader needs to see which is
which. Previous wording, verbatim: *"`CreateDungeonCommandHandlerTests` and
`DeleteDungeonCommandHandlerTests` are the two classes with no not-found test."* **DeleteDungeon has
one:**

```
git grep -n "NotFound" 850a10b -- src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/DeleteDungeonCommandHandlerTests.cs
```

-> `65: public async Task HandleAsync_WithUnknownId_ReturnsNotFound()` and
`85: result.ErrorCategory.Should().Be(ErrorCategory.NotFound);`. It is reached through
`RemoveAsync(...).Returns(false)` rather than a null load, which is why it does not match the
dungeon-missing shape the rest of the entry counts, and it additionally asserts that the error text
contains the id and that `CommitAsync` did not happen. **`CreateDungeonCommandHandlerTests` is the
only command class with no `NotFound` assertion at all.** Both classes are still correctly excluded
from the twelve-way extraction this entry proposes -- but for two different reasons, not one.

**Classification evidence.** The same command at `2603170` returns **nothing**: that file carried no
`NotFound` assertion when this document was written. `git log -S"HandleAsync_WithUnknownId_ReturnsNotFound"
--format="%h %ad %s" --date=short -- <that file>` -> `bcb49dc 2026-08-18`. The exclusion was true on
the day and became false the following day.**]**

**Why it matters.** `dotnet-format.md` section 12 prescribes exactly this constructor shape, so the
duplication is convention-compliant and not itself a deviation -- which is why this is recorded once
rather than sixteen times. The cost is that the not-found path, which is the single most repeated
behaviour in the layer, has twelve independent statements of the same contract; changing how a
missing aggregate is reported means editing twelve files, and a handler that quietly stops returning
`ErrorCategory.NotFound` fails twelve tests that all say the same thing.

**Suggested direction.** Keep the per-class constructor the convention asks for, but lift the
not-found case into one parameterised test that dispatches each command type through a repository
stubbed to return `null` and asserts `ErrorCategory.NotFound` once.

### TEST-009 -- The SQLite in-memory fixture is copy-pasted verbatim into seven Infrastructure test classes

```yaml
id: TEST-009
layer: TEST
severity: S3
category: duplication
authority: practice
citation: Meszaros, xUnit Test Patterns, ch. 19 "Fixture Setup Patterns"
locations:  # AMENDED 6b-ii -- seven, not four; ranges re-resolved at 850a10b over the shared block
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:18-34
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:12-28
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:15-31
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs:18-34
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/UnitOfWorkTests.cs:15-35
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Configurations/AreaOutlineChangeTrackingTests.cs:19-35
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Configurations/PlacedObjectConfigurationTests.cs:21-37
effort: S
risk: low
independent: false
blocked-by:
  - TEST-002
```

**What.** Each of the seven classes declares the same `_connection` and `_options` fields, the same
constructor opening a `Data Source=:memory:` connection and calling `EnsureCreated`, and the same
one-line `Dispose`. Nothing is shared: there is no base class, no `IClassFixture`, no
`ICollectionFixture`. The `CreateContext()` helper is **not** part of the common block -- see
CORRECTION 3.

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 1 applies here identically. Classification: DRIFT.**
Previous wording, verbatim, in the title: *"copy-pasted verbatim into four Infrastructure test
classes"*, and in the body: *"Each of the four classes declares ... the same `CreateContext()`
helper"*. The measured figure is **seven**, by
`git grep -n "EnsureCreated" 850a10b -- src/DungeonMaster.Tests` -> 7 call sites in 7 distinct
classes. The three files this entry misses are the three `TEST-002` misses, for the same reason, and
the full evidence and classification are recorded once under `TEST-002`'s CORRECTION 1 rather than
restated here: all three were added on 2026-08-18, the day after this document was written, so the
four this entry names were all four that existed.**]**

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 3, the block length. Classification: DRIFT -- and the
premise this correction was briefed on was itself wrong; see the recorded discrepancy below.**
Previous wording, verbatim: *"here the same nineteen lines exist four times"*. **Nineteen was correct
when written.** Each of the four `locations` ranges as cited resolves at `2603170` to exactly 19
lines, and the four are byte-identical modulo the constructor's own name -- md5
`e61c65123b15ae41c5de23e362ab41fa` for all four, over
`git show 2603170:<file> | sed -n '<range>p'` with the constructor name normalised. Those nineteen
lines included `CreateContext()`, and at `2603170` all four cited classes had it.

What is true at `850a10b`, over the corrected set of **seven** classes, is a smaller block:

```
git show 850a10b:<file> | sed -n '/private readonly SqliteConnection/,/public void Dispose()/p'
```

-> **17 lines** in six of the seven, and **21** in `UnitOfWorkTests.cs`, which interleaves four XML
doc-comment lines the others do not carry. Piping that through
`| grep -v '^[[:space:]]*///' | grep '[^[:space:]]'` yields **13 lines, byte-identical across all
seven** (md5 `5be5c19f761758c9211a53a50825f15f`, constructor name normalised). **Thirteen non-blank,
non-doc lines is the figure that survives the scope correction. Seventeen holds for six of the seven
and not for the seventh.**

`CreateContext()` cannot be part of the common block at `850a10b`. Measured with
`git show 850a10b:<file> | grep -c "AppDbContext CreateContext()"` and the same with
`grep -c "CreateFactory()"`, across the seven: **5 of 7 declare `CreateContext`, 6 of 7 declare
`CreateFactory`, 4 of 7 declare both** -- `AreaOutlineChangeTrackingTests` and
`PlacedObjectConfigurationTests` have no `CreateContext`, `DungeonReadRepositoryTests` has no
`CreateFactory`. A shared base must provide both unconditionally: offering only the intersection
breaks leaves, and offering only what each leaf already had is not a base.

**Classification evidence.** The claim held at `2603170`, verified by re-running the range extraction
at that commit as above, so this is drift produced by the same 2026-08-18 growth as CORRECTION 1 and
not a counting error.

**Recorded discrepancy.** Session 6b-ii's own manifest asserted that nineteen was *"reachable only by
including `CreateContext()`, which is NOT present in all seven"* and directed a straight 19 -> 17
swap. That reasoning compares the four files this entry named against the seven that exist now.
Nineteen was right for four; seventeen is right for six of seven; thirteen is the only figure right
for all seven. The bare swap would have replaced a true statement with a false one, which is why this
block states three figures and the scope each belongs to.**]**

**Why it matters.** The fixture-setup guidance is that a fixture shared by several test classes
belongs in one place; here the same thirteen non-blank, non-doc lines exist seven times, so the
change proposed in `TEST-002` -- moving from `EnsureCreated` to `Migrate` -- has to be made seven
times and can be made inconsistently. It also means the seven classes cannot be given a shared
seeding vocabulary without adding an eighth copy of the plumbing.

**Suggested direction.** Extract a `SqliteFixture` implementing `IDisposable` and have the seven
classes take it via `IClassFixture<SqliteFixture>`, or a shared abstract base if per-test isolation
of the database is wanted (it currently is -- see the layer verdict). Land it with `TEST-002` so the
schema strategy is changed once.

### TEST-010 -- Four tests assert Received(1) on a save that happens on a path where nothing changed

```yaml
id: TEST-010
layer: TEST
severity: S3
category: testability
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemovePlacedObjectCommandHandlerTests.cs:115-117
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemovePlacedObjectCommandHandlerTests.cs:147-149
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs:123-125
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs:165-167
effort: S
risk: low
independent: true
```

**What.** `HandleAsync_FloorNotFound_ReturnsSuccessWithNoChange` and
`HandleAsync_ObjectNotFound_ReturnsSuccessWithNoChange`, in both the Remove and Update placed-object
classes, assert `await _writeRepository.Received(1).UpdateAsync(dungeon, Arg.Any<CancellationToken>())`
on paths whose own names say the aggregate was not modified.

**Why it matters.** The cited page's guidance is to "focus on verifying the behavior over the
implementation". The observable behaviour on these paths is "returns success and changes nothing";
whether the handler nonetheless issues a redundant `UpdateAsync` is an implementation choice, and
these four assertions promote it to a contract. A correct optimisation -- skipping the save when the
domain call reported no change -- would break four tests without changing any behaviour a caller can
see. This is the clearest instance in the suite of a test coupled to implementation.

**Suggested direction.** Drop the `Received(1)` assertion from the two no-change tests in each class
and keep it on the paths where the save is the point. If "always persists" really is the intended
contract, say so in the test name rather than in a call-count.

### TEST-011 -- Task-returning substitutes are given explicit Returns(Task.CompletedTask) stubs

```yaml
id: TEST-011
layer: TEST
severity: S3
category: testability
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs:30-32
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddFloorCommandHandlerTests.cs:27
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/CreateDungeonCommandHandlerTests.cs:34-38
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/DeleteDungeonCommandHandlerTests.cs:34-38
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemoveAreaCommandHandlerTests.cs:31-33
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/ReorderFloorCommandHandlerTests.cs:31-33
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateAreaOutlineCommandHandlerTests.cs:32-34
effort: XS
risk: low
independent: true
```

**What.** Several handler tests configure `_writeRepository.UpdateAsync(...)` or `AddAsync(...)`
with `.Returns(Task.CompletedTask)` even though the return value is never inspected. The four
placed-object and query classes do not do this, so the setup is inconsistent as well as inert.

**Why it matters.** I could not confirm against NSubstitute's published documentation that a
`Task`-returning member is auto-substituted with a completed task, so I state this as belief rather
than fact: my reading is that these stubs configure a return NSubstitute would supply anyway, which
makes them arrange noise that a reader has to check before concluding it does nothing. Whether or
not that belief holds, the inconsistency between the classes is visible in the source and costs a
reader time on every one of them.

**Suggested direction.** Confirm the framework's default for `Task`-returning members against the
version in use, then either remove the redundant stubs everywhere or add them everywhere, so the
pattern reads as a decision.

### TEST-012 -- AreaPersistenceDiagnosticTests is a committed debugging session with one subsumed test

```yaml
id: TEST-012
layer: TEST
severity: S3
category: dead-code
authority: judgment
citation: n/a
locations:  # AMENDED 6b-ii -- re-resolved at 850a10b to the four method anchors
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:11-16
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:55
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:75
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:96
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:116
effort: XS
risk: low
independent: true
```

**What.** The class is named `AreaPersistenceDiagnosticTests` and its summary says it was "added
while investigating a report that both operations appeared not to persist from the designer UI".
`RemoveArea_WithEmptyOutline_PersistsAcrossReload` seeds through the same `SeedAsync` as
`RemoveArea_PersistsAcrossReload` -- which always produces an area with an empty outline -- and
differs from it only by an extra assertion at line 79 that the outline is empty. Similarly
`UpdateArea_SecondRenamePersistsAcrossReload` performs the first rename and then a second -- but it
does **not** contain `UpdateArea_RenamePersistsAcrossReload`; see CORRECTION 6.

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 6, and this one is a reasoning defect, not a stale
citation. Classification: MEASUREMENT ERROR.** Previous wording, verbatim: *"Similarly
`UpdateArea_SecondRenamePersistsAcrossReload` performs the first rename and then a second, so it
strictly contains `UpdateArea_RenamePersistsAcrossReload`."* -- and, in the Suggested direction,
*"drop the two subsumed tests"*. **Only one of the two claimed subsumptions holds.**

`RemoveArea_PersistsAcrossReload` (`:55` at `850a10b`) is genuinely subsumed by
`RemoveArea_WithEmptyOutline_PersistsAcrossReload` (`:75`): the two are line-for-line identical except
that the wider one adds an assertion that the outline is empty. Deleting the narrow one loses nothing.
**It is the narrow test that is deletable, not the one carrying the extra assertion** -- the entry's
prose above names the wider test first and describes it by its extra assertion, which reads as though
it were the redundant one. It is not.

`UpdateArea_RenamePersistsAcrossReload` (`:96`) is **not** subsumed, and deleting it would destroy
coverage. `UpdateArea_SecondRenamePersistsAcrossReload` (`:116`) renames and commits, then **reloads
in a second unit of work** and renames again, asserting only the final name. The second unit of work
reads the aggregate back from the database and overwrites the name **regardless of whether the first
commit persisted**, so the first rename's persistence is never observed by that test. Read both
bodies with
`git show 850a10b:src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs | sed -n '96,140p'`.

Session 6b-ii demonstrated this by injection with a passing control before acting on it: removing the
commit from the first rename block leaves the `SecondRename` assertion **passing** and makes the
`RenamePersists` assertion **fail**. That demonstration was run by the session's test tasks, not by
this documentation pass; this pass records it and states its own independent static evidence above.

**This class holds the only persistence round-trip coverage of area removal and area rename.** Every
other `RemoveArea`/`UpdateArea` hit in the suite is either an in-memory Domain aggregate test or a
handler test against a substituted repository, and `AreaOutlineChangeTrackingTests` covers
`UpdateAreaOutline`, a different method. **Following this entry as written -- dropping two tests --
would have removed real coverage. It is one deletion.**

**Classification evidence.** Unlike every other correction in this document, this one is not drift.
`git grep -n "public async Task" 2603170 -- <that file>` returns the same four tests at `:51`, `:71`,
`:92` and `:112`, and
`git show 2603170:<that file> | sed -n '92,137p'` shows `SecondRename` already performing the
reload-in-a-second-unit-of-work that defeats the subsumption. **The code the entry reasoned about was
the code that exists now; the reasoning was wrong on the day it was written.

**The `**Why it matters.**` paragraph below is corrected in place by this block**, because it rested
on the same false premise and sits *after* the amendment, where a reader has no signal that it is
superseded. Previous wording, verbatim: *"and the two subsumed pairs support that reading"*. It now
reads *"and the **one** subsumed pair -- plus a second, near-duplicate pair that turns out **not** to
be one (CORRECTION 6) -- supports that reading"*. **The paragraph's argument survives the correction:**
one genuine subsumption plus one near-duplicate that is not a subsumption is still the signature of a
bisecting session committed as it stood, and the paragraph's real point -- that this class holds the
only round-trip coverage of area removal and rename while wearing a label that invites deletion --
is unaffected and independently confirmed above.**]**

**Why it matters.** I believe the name and the summary together mark this as investigation scaffold
that was never converted into permanent coverage, and the **one** subsumed pair -- plus a second,
near-duplicate pair that turns out **not** to be one (CORRECTION 6) -- supports that reading:
someone bisecting a persistence bug wrote the narrower case, then the wider one, and committed
both. The cost is a class whose title tells a future reader it is provisional -- so nobody will
maintain it -- while it nonetheless holds the only round-trip coverage of area removal and rename
that exists. That is the worst of both: real coverage wearing a label that invites deletion.

**Suggested direction.** Decide what it is. If the round-trip coverage is wanted, rename to
`AreaPersistenceTests`, drop the **one** subsumed test -- `RemoveArea_PersistsAcrossReload`, keeping
`UpdateArea_RenamePersistsAcrossReload` per CORRECTION 6 -- and rewrite the summary as a statement of
what is guaranteed rather than of what was being investigated. If it is not wanted, delete it and check
that nothing else covers area removal (nothing does today).

### TEST-013 -- Four geometry facts differ only in their data (the five snap-zone facts are deleted)

```yaml
id: TEST-013
layer: TEST
severity: S3
category: duplication
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:  # AMENDED 6b-ii -- re-resolved at 850a10b; entry 4 deleted in 1678d12, entry 5 moved
  - src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:436-457   # NoBareSpacingValues
  - src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:555-577   # NoBareMarginValues
  - src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:591-614   # NoBareStrokeValues
  - src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:629-652   # NoBareMinimumSizeValues -- missed by the original entry
  - src/DungeonMaster.Tests/AppCore/Map/TileCoordinateHelperTests.cs:11-22
effort: S
risk: low
independent: true
```

**What.** `NoBareSpacingValues`, `NoBareMarginValues`, `NoBareStrokeValues` **and
`NoBareMinimumSizeValues`** in `GeometryScaleTests` are structurally identical: `Scan` over XAML,
`Scan` over C#, filter out zero, `Describe`, order, assert empty. They vary only by pattern list,
allow-list and message. The `SnapZoneCalculatorTests` half of this entry no longer exists; see
CORRECTION 5(a).

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 5(a), `locations` entry 4 refers to deleted code.
Classification: DRIFT.** Previous wording, verbatim, in the title: *"and five snap-zone facts"*, and
in the body: *"Separately, `SnapZoneCalculatorTests` states five `[Fact]`s that each call
`SnapZoneCalculator.ToTile` ..."*, with
`src/DungeonMaster.Tests/Application/Common/Map/SnapZoneCalculatorTests.cs:11-69` as `locations`
entry 4.

```
git ls-tree -r --name-only 850a10b | grep -i snapzone
```

-> **nothing** (exit 1). `SnapZoneCalculator` and its five facts were deleted in `1678d12`
(*"refactor(app): extract Map types to App.Core layer"*). **Location 4 does not exist and no work
should be done for it.**

**Classification evidence.** The same command at `2603170` returns
`src/DungeonMaster.Application/Common/Map/SnapZoneCalculator.cs` and
`src/DungeonMaster.Tests/Application/Common/Map/SnapZoneCalculatorTests.cs`, and
`git grep -c "\[Fact\]" 2603170 -- <that test file>` -> `5`. The entry was exactly right when
written. `git log -1 --format="%h %ad" --date=short 1678d12` -> `1678d12 2026-08-26`, nine days
after.**]**

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 5(b), `locations` entry 5's path is dead too.
Classification: DRIFT.** Previous path, verbatim:
`src/DungeonMaster.Tests/Application/Common/Map/TileCoordinateHelperTests.cs:11-22`. The file moved
to `src/DungeonMaster.Tests/AppCore/Map/TileCoordinateHelperTests.cs` in the same commit `1678d12`.
**The path is corrected rather than the location deleted: the cited content survives at the same
line numbers.** `git show 850a10b:src/DungeonMaster.Tests/AppCore/Map/TileCoordinateHelperTests.cs | sed -n '11,22p'`
returns the `[Theory]` with five `[InlineData]` rows and `SnapToGrid_ReturnsNearestTilePoint`, which
is what entry 5 was citing. The comparison the entry draws still stands; only the address changed.**]**

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 5(c), the uniform-fact count is four, not three.
Classification: DRIFT.** Previous wording, verbatim: *"`NoBareSpacingValues`, `NoBareMarginValues` and
`NoBareStrokeValues` in `GeometryScaleTests` are structurally identical"*, and in the Suggested
direction, *"for the three uniform scales"*.

```
git grep -n "NoBare" 850a10b -- src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs
```

-> six fact declarations, at `:436` `NoBareSpacingValues`, `:469` `NoBarePaddingValues`, `:514`
`NoBareRadiusValues`, `:555` `NoBareMarginValues`, `:591` `NoBareStrokeValues`, `:629`
`NoBareMinimumSizeValues`. **Four share the one pipeline** -- Spacing, Margin, Stroke and MinimumSize.
Padding and Radius are correctly excluded, and the reason is worth stating because it is what makes
the other four a set: Padding adds a `PaddingScaleExceptions` filter and an `IsShapingPadding`
projection, Radius adds a `RadiusGeometryExceptions` filter, and neither shares the pipeline.
**A `[Theory]` sized for three under-covers by one whole ratchet.**

**Classification evidence.** The same command at `2603170` returns **five** facts and
`NoBareMinimumSizeValues` is not among them -- it did not exist. The body naming three uniform facts
was correct when written.

All three cited geometry line ranges (`300-324`, `428-453`, `466-492`) are stale by roughly 130 lines
and have been re-resolved above; at `2603170` they resolved exactly, `sed -n '300p;324p'` and its two
siblings returning `[Fact]` and the fact's closing brace. That too is drift.**]**

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 5(d), the title contradicted the entry's own body.
Classification: MEASUREMENT ERROR.** Previous title, verbatim: *"Five geometry facts and five
snap-zone facts differ only in their data"*, while the body named **three** geometry facts as
structurally identical. Five geometry facts existed at `2603170`, but two of them --
`NoBarePaddingValues` and `NoBareRadiusValues` -- did not "differ only in their data" then either:
`git grep -n "PaddingScaleExceptions\|RadiusGeometryExceptions\|IsShapingPadding" 2603170 -- src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs`
returns both exception tables (`:214`, `:233`) and their use inside those two facts (`:350`, `:357`,
`:361`, `:400`) at the authoring commit. **The title overstated its own body's finding on the day it
was written, and a reader could have caught it without leaving the document.** Recording that is more
useful to the next auditor than the corrected number alone.**]**

**Why it matters.** The cited page's "Avoid multiple Act tasks" section recommends parameterised
tests precisely for this shape, and the .NET guidance on avoiding logic in tests points the same
way. The cost in `GeometryScaleTests` is that adding a sixth scale means copying twenty-five lines
and remembering all five steps; the cost in `SnapZoneCalculatorTests` is five test methods where a
five-row `[InlineData]` table would show the boundary progression -- mid-tile, origin, exact
boundary, negative partial, negative whole, in the order the file states them -- as one readable
set. That the file beside it already
does this makes the inconsistency the finding.

**Suggested direction.** Give `GeometryScaleTests` a single `[Theory]` over
`(name, xamlPatterns, cSharpPatterns, allowList, message)` for the **four** uniform scales, leaving
padding and radius as facts because they carry extra filtering. The `SnapZoneCalculatorTests` half of
this direction is **struck**: that file was deleted in `1678d12` (CORRECTION 5(a)).

### TEST-014 -- MAUI property types and the conversion matrix are hand-transcribed into the test at a fixed date

```yaml
id: TEST-014
layer: TEST
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:72-81
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:92-137
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:170-180
effort: M
risk: medium
independent: true
```

**What.** Three tables hold facts about `Microsoft.Maui.Controls`: which element-plus-property pairs
have which CLR type, which property names have an element-independent type, and which conversions
`BindableProperty.TryConvert` performs. Their documentation states they were "read off
Microsoft.Maui.Controls.dll 10.0.41 by reflection on 2026-08-07" and measured by probing `SetValue`.

**Why it matters.** I believe this is the most valuable guard in the App sub-suite -- it is the one
that would have caught the `Button.CornerRadius` defect its summary describes -- and also the one
most likely to become quietly wrong. The tables are a snapshot of a third-party assembly held in
source with nothing tying them to that assembly; a MAUI upgrade that widens a conversion or retypes
a property leaves the tables stating the old truth, and the failure mode is a false pass, not a
compile error. `EveryDmTokenBindingTargetsAPropertyOfAKnownType` protects the coverage of the tables
but not their accuracy.

**Suggested direction.** Since the test project does not reference the App project, it cannot
reflect over MAUI directly today. Consider generating the tables from reflection in a small tool
run against the resolved `Microsoft.Maui.Controls` assembly and committing the output, so a version
bump produces a visible diff; failing that, pin the MAUI version the tables were read from as a
constant in the file and assert it against the resolved package version.

### TEST-015 -- A TrueType cmap parser lives in a test class and three of its four subtable branches are unexercised

```yaml
id: TEST-015
layer: TEST
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:276-320
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:365-379
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:452-482
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:484-515
effort: M
risk: medium
independent: true
```

**What.** `IconFontCoverageTests` contains a nested `TrueTypeCmap` class of roughly 300 lines that
parses a font's table directory and four `cmap` subtable formats (0, 4, 6, 12) out of a byte array.
The only font it is ever given is `rpgawesome-webfont.ttf`, which exercises one of those four
formats; `TheIconFontCmapParsesToPrivateUseGlyphs` is the sole check on the parser and it is a smoke
test of the whole pipeline rather than of any format.

**Why it matters.** I believe this inverts the usual relationship: the most intricate code in the
test project is the code with the least coverage, and a defect in it produces a wrong answer rather
than an error. Format 4 is the branch that runs, and its segment arithmetic -- `endCodes`,
`startCodes`, `deltas`, `rangeOffsets` at lines 390-393 -- has several off-by-one opportunities that
would return a plausible but wrong codepoint set, which the icon coverage fact would then use as
ground truth. The other three branches are dead in practice, so they carry cost without protection.

**Suggested direction.** Either narrow the parser to format 4 with an explicit failure on anything
else, so the code that exists is the code that runs; or keep the four formats and add byte-array
fixtures exercising each, at which point the parser deserves to be a named type of its own rather
than a nested class in a fact about icons.

### TEST-016 -- Assertions live inside shared scan helpers, so failures are reported against the helper

```yaml
id: TEST-016
layer: TEST
severity: S3
category: testability
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:86-88
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:143-146
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:176-179
  - src/DungeonMaster.Tests/App/Theme/ThemeHygieneTests.cs:197-200
  - src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:640-643
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:354-356
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:437-441
effort: M
risk: medium
independent: true
```

**What.** `ThemeSource.ThemeFiles`, `ThemeSource.DoubleToken`, `ThemeSource.Int32Token`,
`ThemeHygieneTests.Offenders`, `GeometryScaleTests.Scan`, `TokenTypeMatchesTargetPropertyTests
.XamlFiles` and `.AppPropertyTypes` all call `.Should()` from inside a non-test helper that other
tests use to arrange their subject.

**Why it matters.** These are the anti-vacuity guards, and I think their intent is sound -- a scan
that reaches nothing must be a failure, not a pass. My concern is where the assertion sits. Because
the helper is called during Arrange, a broken scan reports the helper's `because` string against
whichever test happened to call it first, and the same underlying fault surfaces as an apparently
unrelated failure in five or six different tests. A reader has to work out which of them is the
cause. It also means no single test owns "the scan reaches files", so it cannot be named, skipped or
reasoned about on its own.

**Suggested direction.** Promote each anti-vacuity guard to its own `[Fact]` -- one per scan, named
for what it guards -- and let the helpers return their result without asserting. The protection is
identical and the failure names itself.

### TEST-017 -- Every test in the type-matching class recomputes the full disk scan from scratch

```yaml
id: TEST-017
layer: TEST
severity: S3
category: performance
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:212-214
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:254
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:288
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:323
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:82
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:154
effort: S
risk: low
independent: true
```

**What.** All four tests in `TokenTypeMatchesTargetPropertyTests` call `Bindings()`, which reads and
`XDocument.Parse`s every `.xaml` file in the App project and regex-scans every `.cs` file, from
disk, each time. Both tests in `IconFontCoverageTests` call `IconFontCodepoints()`, which re-reads
and re-parses the TTF. Nothing is cached across tests, and neither class uses `IClassFixture`.

**Why it matters.** Microsoft's characteristics of a good unit test list "Fast" first, and the whole
suite currently runs in two seconds, so this is not a problem today -- I raise it as a structural
observation rather than a measured cost. What I believe does matter is that the repetition is
invisible: the scan is expressed as a private static method, so it reads like a cheap accessor, and
a sixth or seventh fact added to either class silently multiplies the I/O again.

**Suggested direction.** Move the scan behind an `IClassFixture` (or a `Lazy<T>` static, given the
inputs are immutable within a run) so each class parses the tree once. This pairs naturally with
`TEST-016`, since a fixture is also the natural home for the anti-vacuity assertions.

### TEST-018 -- The two query-handler tests assert only that the stubbed DTO came back

```yaml
id: TEST-018
layer: TEST
severity: S3
category: testability
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonDetailQueryHandlerTests.cs:19-38
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonsQueryHandlerTests.cs:28-52
effort: S
risk: low
independent: true
```

**What.** `HandleAsync_WithExistingId_ReturnsDtoFromRepository` stubs
`_readRepository.GetDetailAsync` to return a `DungeonDetailDto` it constructed, then asserts that
the returned object's `Id` and `Name` equal the values it put in. `HandleAsync_WithDungeons_
ReturnsSummaries` does the same with a two-element list.

**Why it matters.** The cited page's distinction between a stub and a mock is the relevant one: here
the substitute is a stub supplying data, and the assertions are against that data, so the test can
only fail if the handler transforms its input -- which it does not. The tests are true by
construction and will remain true after any refactor of the handler that preserves pass-through,
which means they neither document a behaviour nor guard one. The four assertions on
`result[0].Name`, `result[1].Name` and `result[1].Description` cost more to read than they protect.

**Suggested direction.** Reduce each to one assertion that the repository result is returned
unchanged (`result.Should().BeSameAs(dto)` for the detail case), or, if the handler is expected to
grow projection logic, test that logic when it arrives rather than pinning the current identity
mapping in four places.

### TEST-019 -- No test in the suite passes a cancellable or cancelled CancellationToken (FALSIFIED -- two do, from 2026-08-18; the real gap is restated in CORRECTION 2)

```yaml
id: TEST-019
layer: TEST
severity: S3
category: async
authority: judgment
citation: n/a
locations:  # AMENDED 6b-iii -- the two :37 anchors were dead and are replaced; see CORRECTION 4
  - src/DungeonMaster.Tests/Application/Common/Handlers/CommandHandlerTestBase.cs:62   # AMENDED 6b-iii -- replaces AddAreaCommandHandlerTests.cs:37 (dead)
  - src/DungeonMaster.Tests/Application/Common/Handlers/CommandHandlerTestBase.cs:69   # AMENDED 6b-iii -- replaces UpdateFloorCommandHandlerTests.cs:37 (dead)
  - src/DungeonMaster.Tests/Application/Common/Handlers/DungeonNotFoundHandlerTestBase.cs:34   # AMENDED 6b-iii
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonDetailQueryHandlerTests.cs:32
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:42
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs:44
effort: S
risk: low
independent: true
```

**What.** Every asynchronous call site in the suite passes `CancellationToken.None`, and every
NSubstitute setup matches the token with `Arg.Any<CancellationToken>()`. No test constructs a
`CancellationTokenSource`, cancels one, or asserts that a token reaches the repository.

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 1, the headline claim is FALSIFIED, not merely stale. Classification: DRIFT.**
Previous wording, verbatim, as the record's title and its first paragraph: *"No test in the suite
passes a cancellable or cancelled CancellationToken"* / *"No test constructs a
`CancellationTokenSource`, cancels one, or asserts that a token reaches the repository."*
**Counterexamples, measured at `1407090`:** `ValidationBehaviorTests.cs:222` and `:366` each
construct a `CancellationTokenSource`, pass the token into production code, and capture the exact
instance that arrived -- `Arg.Do<CancellationToken>` captures at `:233`, `:242`, `:377` and `:386`.
So a test did construct one, and did assert the identity of the token that reached the code under
test.

**Classification evidence.** Those two facts were added by `f9a665b` / `09f4360` on **2026-08-18**,
one day after this document was written. Drift, not an authorship error.**]**

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 2, the narrower gap that WAS real, stated correctly. Classification: not a correction -- the surviving finding, re-measured.**
Falsifying the headline does not empty the entry. Measured at `1407090`: **0 of 16 handlers** (14
command + 2 query) and **0 of 6 repository/unit-of-work async methods** were proven to forward their
token, against **1 of 1 pipeline behavior** proven. Also still true at `1407090`: no test cancelled a
token, no test asserted `OperationCanceledException`, and `Arg.Is<CancellationToken>` appeared
nowhere. Those figures are counted from this session's `measurement.md`. That -- and not the title --
is what `MASTER-070` was closed against.**]**

**Why it matters.** I believe this leaves an entire dimension of the async contract unstated. Every
handler and repository method takes a `CancellationToken`; nothing proves any of them forwards it.
A handler that dropped the token and called `LoadAsync(id, default)` would pass all 292 tests, and
the symptom in the running application -- work continuing after a page is navigated away from --
is exactly the kind of thing a suite is supposed to catch and a manual pass is not.

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 3, this was a COVERAGE GAP AND NOT A DEFECT. Classification: not a correction -- a qualification the entry does not make.**
Previous wording, verbatim: *"A handler that dropped the token and called `LoadAsync(id, default)`
would pass all 292 tests, and the symptom in the running application -- work continuing after a page
is navigated away from -- is exactly the kind of thing a suite is supposed to catch and a manual pass
is not."* The hypothetical was accurate about the *suite* and is now false (see the closure below:
that exact defect, injected, fails a named test). But the sentence reads as though the symptom were
live. It was not. The re-measurement checked the production side across the handlers, the
`DungeonMutationExtensions` overloads and Infrastructure, and found the **token threaded correctly
throughout**. Nothing was dropping a token; nothing proved it was not. Recorded so a later reader
does not treat this entry as a shipped bug report.**]**

**Suggested direction.** Add one test per repository-facing handler shape that stubs with
`Arg.Is<CancellationToken>(token => token == expected)` and asserts the call was received, plus one
Infrastructure test that cancels a token before a repository call and asserts
`OperationCanceledException`.

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 4, two dead per-file anchors replaced, and the closure. Classification: DRIFT (anchors).**
Previous wording, verbatim, as the first two entries of `locations`:
*`.../Commands/AddAreaCommandHandlerTests.cs:37`* and *`.../Commands/UpdateFloorCommandHandlerTests.cs:37`*.
Both are dead: `8784326` (6b-ii) extracted `CommandHandlerTestBase` and
`DungeonNotFoundHandlerTestBase`, and all **14** command-handler test classes now derive from one of
them -- the 14 counted from this session's `measurement.md` at `1407090`, and confirmed by T1's own
`--filter "FullyQualifiedName~PassesThatSameToken"` run, in which the base-level fact executes
exactly 14 times -- and `UnitOfWorkSubstitute.cs:24` is the single `Arg.Any<CancellationToken>()` chokepoint the
entry's second sentence was describing per-file. They are replaced above by live anchors handed over
by 6b-iii's T1.

**Closure -- `MASTER-070`, T1, commit `6871900`.** One new seam,
`CommandHandlerTestBase<THandler>.InvokeWithTokenAsync`, signature
`protected abstract Task InvokeWithTokenAsync(CancellationToken ct)`, at
`src/DungeonMaster.Tests/Application/Common/Handlers/CommandHandlerTestBase.cs:62`, and six new
facts, one per async port method. No assertion uses `Arg.Any<CancellationToken>()`; each names the
token value.

| Port method | Fact | Declared at | Executions |
|---|---|---|---|
| `IUnitOfWorkFactory.CreateAsync` | `CommandHandlerTestBase<THandler>.HandleAsync_WithACancellationToken_PassesThatSameTokenToUnitOfWorkFactoryCreateAsync` | `CommandHandlerTestBase.cs:69` | 14 |
| `IDungeonWriteRepository.LoadAsync` | `DungeonNotFoundHandlerTestBase<THandler>.HandleAsync_WithACancellationToken_PassesThatSameTokenToWriteRepositoryLoadAsync` | `DungeonNotFoundHandlerTestBase.cs:34` | 12 |
| `IUnitOfWork.CommitAsync` | `CreateDungeonCommandHandlerTests.HandleAsync_WithACancellationToken_PassesThatSameTokenToUnitOfWorkCommitAsync` | `CreateDungeonCommandHandlerTests.cs:109` | 1 |
| `IDungeonWriteRepository.RemoveAsync` | `DeleteDungeonCommandHandlerTests.HandleAsync_WithACancellationToken_PassesThatSameTokenToWriteRepositoryRemoveAsync` | `DeleteDungeonCommandHandlerTests.cs:115` | 1 |
| `IDungeonReadRepository.GetSummariesAsync` | `GetDungeonsQueryHandlerTests.HandleAsync_WithACancellationToken_PassesThatSameTokenToGetSummariesAsync` | `GetDungeonsQueryHandlerTests.cs:83` | 1 |
| `IDungeonReadRepository.GetDetailAsync` | `GetDungeonDetailQueryHandlerTests.HandleAsync_WithACancellationToken_PassesThatSameTokenToGetDetailAsync` | `GetDungeonDetailQueryHandlerTests.cs:61` | 1 |

**30 executions** (14 + 12 + 1 + 1 + 1 + 1), by
`dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj --filter "FullyQualifiedName~PassesThatSameToken"`
-> `Passed: 30, Failed: 0, Total: 30`.

The entry's second suggested direction is also implemented: **four** Infrastructure tests over
`SqliteInMemoryTestBase`, each seeding its row first, then `await cts.CancelAsync()` before the call,
each asserting `OperationCanceledException` --
`DungeonReadRepositoryTests.GetSummariesAsync_WithAnAlreadyCancelledToken_ThrowsOperationCanceledException`
(`:72`), `...GetDetailAsync_...` (`:98`),
`DungeonWriteRepositoryTests.LoadAsync_...` (`:231`), and
`UnitOfWorkTests.CommitAsync_WithAnAlreadyCancelledToken_ThrowsOperationCanceledExceptionAndWritesNothing`
(`:115`). Seeding first is what makes the cancellation, rather than a missing row, the discriminator.

**Shown able to fail.** `CreateDungeonCommandHandler.cs:20`, `CreateAsync(ct)` changed to
`CreateAsync(CancellationToken.None)` -- the precise defect the "Why it matters" paragraph
hypothesises. `dotnet test ... --filter "FullyQualifiedName~...Features.Dungeons.Commands"` ->
`Failed: 1, Passed: 74, Total: 75`, the sole failure being
`CreateDungeonCommandHandlerTests.HandleAsync_WithACancellationToken_PassesThatSameTokenToUnitOfWorkFactoryCreateAsync`,
with two named controls green in the same run
(`CreateDungeonCommandHandlerTests...PassesThatSameTokenToUnitOfWorkCommitAsync` and
`AddAreaCommandHandlerTests...PassesThatSameTokenToUnitOfWorkFactoryCreateAsync`). Reverted;
production byte-identical. The reviewer reproduced the injection independently: `Failed: 1,
Passed: 29`, then `Failed: 0, Passed: 30`.

**Post-revert filter totals**, all `Failed: 0`, by
`dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj --filter "FullyQualifiedName~<X>"`:
`...Application.Common.Handlers` **14**, `...Features.Dungeons.Commands` **75**,
`...Features.Dungeons.Queries` **6**, `...Infrastructure.Persistence.Repositories` **23**. Full leaf
override proven by `dotnet build src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` ->
`0 Warning(s), 0 Error(s)`, a missed abstract override being CS0534.

**What this does NOT cover.** The `Dispatcher` itself. T1 proves forwarding at the **handler and
repository** boundaries only; the dispatcher's token propagation is unasserted and is filed
separately as **`MASTER-172`**. `MASTER-070`'s closure does not claim it, and a later reader must not
take one for the other.**]**

**[AMENDED 6b-x, 2026-09-18 -- the clause above is now out of date and is corrected here rather than
rewritten.** "The dispatcher's token propagation is unasserted" was true when written and is **false as
of `b0fa532`, merged `98326d4`**: `MASTER-172` is closed, and `DispatcherCancellationTests.cs` asserts
propagation to the handler **and** to the pipeline behaviour on all three request paths. The rest of the
paragraph stands unchanged -- `MASTER-070`'s closure still does not claim the dispatcher boundary, and a
later reader still must not take one for the other. See the amended block at `:321` and the
`MASTER-172` row in `docs/audit/00-master-index.md`.**]**

### TEST-020 -- Domain coverage is happy paths and unknown-id no-ops, with no constructor or boundary cases

```yaml
id: TEST-020
layer: TEST
severity: S3
category: testability
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:8-18
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:58-72
  - src/DungeonMaster.Tests/Domain/Entities/Area/AreaOutlineTests.cs:34-59
  - src/DungeonMaster.Tests/Domain/Entities/Floor/FloorObjectMethodsTests.cs:17-33
  - src/DungeonMaster.Tests/Domain/Entities/Floor/PlacedObjectTests.cs:74-90
effort: M
risk: low
independent: true
```

**What.** The five Domain classes cover the success path of each operation and, for most, the
unknown-id no-op. `PlacedObjectTests` is the exception: it has two `[Theory]` blocks covering `NaN`,
both infinities, and values below and above the scale range. No equivalent exists elsewhere -- there
is no test for `new Dungeon(null!)` or an empty name, no test of `ReorderFloor` with an index of
zero, a negative index or one past the end, no test of `AddFloor` on an aggregate at any boundary,
and no test that `UpdateAreaOutline` rejects or accepts a degenerate one- or two-segment outline.

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 1, "the five Domain classes". Classification: DRIFT.**
Previous wording, verbatim: *"The five Domain classes cover the success path of each operation"*,
over a `locations` list of five files. There are **six** Domain test files at `1407090` --
counted from this session's `measurement.md`, which measured them at that commit --
`FloorMethodsTests.cs` being the sixth, added by `781f73b`.

**Classification evidence.** `781f73b` is dated **2026-08-18** ("report mutation outcomes instead of
silently no-opping"), the day after this document was written. The entry was right on the day.**]**

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 2, "no test of `AddFloor` on an aggregate at any boundary". Classification: MEASUREMENT ERROR.**
Previous wording, verbatim: *"no test of `AddFloor` on an aggregate at any boundary"*. **This was
never true.** `DungeonAggregateTests.cs:12` and `:24` were **both present at the audit commit
`413be78`** (2026-08-17), and `:12` falls inside this entry's **own first cited location**,
`src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:8-18`. The entry cited the
tests that refute its own clause.

**Classification evidence.** Presence at `413be78` is what settles it: the clause did not become
false, it was false when written. This is a different failure from drift, and re-measurement is the
only thing that catches it -- which is the reason this pass exists.**]**

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 3, "`PlacedObjectTests` is the exception". Classification: MEASUREMENT ERROR.**
Previous wording, verbatim: *"`PlacedObjectTests` is the exception: it has two `[Theory]` blocks
covering `NaN`, both infinities, and values below and above the scale range. No equivalent exists
elsewhere"*. **Never true.** `ObjectScaleTests.cs:8` and `:16` are boundary `[Theory]` blocks that
**predate the audit by three months**, so `PlacedObjectTests` was not the only one and not the
exception.

**Classification evidence.** The three-month lead means no post-audit commit can account for it; the
clause was false on 2026-08-17. Authorship error, not drift. Note that this clause and CORRECTION 2
together carry the entry's rhetorical argument -- "the author knows how to write these, which makes
the absence elsewhere look like an omission" -- and both halves of that argument were wrong when
made.**]**

**Why it matters.** I believe `PlacedObjectTests` shows the author knows how to write these -- its
invalid-scale theories are the best boundary coverage in the project -- which makes the absence
elsewhere look like an omission rather than a decision. The floor renumbering in `RemoveFloor` and
`ReorderFloor` is arithmetic over an ordered collection, which is the classic home of off-by-one
defects, and it is currently covered by exactly one three-floor happy-path case each.

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 4, "exactly one three-floor happy-path case each". Classification: DRIFT.**
Previous wording, verbatim: *"The floor renumbering in `RemoveFloor` and `ReorderFloor` ... is
currently covered by exactly one three-floor happy-path case each."* At `1407090` **`RemoveFloor` has
4 tests and `ReorderFloor` has 6**, counted from this session's `measurement.md`.

**Classification evidence.** `781f73b` (2026-08-18) grew
`src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs` **from 104 to 336 lines
and from 8 to 21 tests** -- precisely the boundary coverage this entry asked for, landing the day
after it was written. The entry was accurate on the day and obsolete the next.**]**

**Suggested direction.** Extend `DungeonAggregateTests` with `[Theory]` blocks over out-of-range
reorder targets and over the empty and single-element floor collections, and state explicitly (in a
test, not a comment) what the aggregate does with a null or blank name.

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 5, clause by clause, and the closure. Classification: mixed, stated per clause.**
The entry is a list of clauses with different fates, and a single verdict on it would be wrong. As
re-measured at `1407090`:

| Clause, as written | Status at `1407090` |
|---|---|
| *"no test for `new Dungeon(null!)` or an empty name"* | **was open; closed by this session** -- T2, below |
| *"no test of `ReorderFloor` with an index of zero"* | **closed before this session** -- `DungeonAggregateTests.cs:161` |
| *"... or one past the end"* | **closed before this session** -- `:179` |
| *"... a negative index"* | open at the Domain level; covered at the Application level, and closed at Domain by T2 below |
| *"no test of `AddFloor` on an aggregate at any boundary"* | **never true** -- CORRECTION 2 |
| *"no test that `UpdateAreaOutline` rejects or accepts a degenerate one- or two-segment outline"* | open, but as a **missing production invariant**, not a missing test -- filed as `MASTER-168` |
| *"`PlacedObjectTests` is the exception"* | **never true** -- CORRECTION 3 |

**Closure -- `MASTER-086`, T2, commit `aeac7b7`.** Four `[Theory]`, three `InlineData` each (`null`,
`""`, `"   "`), each asserting `ArgumentException`, one per name guard:
`Constructor_WithNullOrWhitespaceName_ThrowsArgumentException` (`DungeonAggregateTests.cs:341`,
proving `Dungeon.cs:25`), `Update_WithNullOrWhitespaceName_...` (`:354`, proving
`DungeonMethods.cs:24`), `AddArea_WithNullOrWhitespaceNameOnExistingFloor_...` (`:373`, proving
`Area.cs:31`), `UpdateArea_WithNullOrWhitespaceNameOnExistingArea_...` (`:393`, proving
`AreaMethods.cs:24`). The last two pass a **real** `floor.Id` and a **real** `area.Id`, because
`DungeonMethods.AddArea` returns `null` at its floor lookup (`:208-214`) and `UpdateArea` returns
`NotFound` at its floor lookup *before* the name guard is reached -- so the guard is what throws.

Plus five `[Fact]`: `ReorderFloor_WithNegativeNumber_ReturnsOutOfRangeAndLeavesNumberUnchanged`
(`:413`); `AddPlacedObject_WithoutScale_ReturnsObjectWithDefaultScaleAndGivenKindPositionAndRotation`
(`:431`); `AddPlacedObject_WithoutScaleOnUnknownFloorId_ReturnsNull` (`:455`);
`UpdateObject_WithoutScaleOnExistingObject_ReturnsSuccessAndAppliesPositionAndRotation`
(`FloorObjectMethodsTests.cs:129`); `UpdateObject_WithoutScaleOnUnknownId_ReturnsNotFound` (`:152`).
These reach the previously untested **overloads**, not the ones already covered at
`FloorObjectMethodsTests.cs:76`/`:98`: binding is by arity with no optional parameters --
`DungeonMethods.cs:371` (4-arg) against `:404`, and `FloorObjectMethods.cs:96` (3-arg) against
`:134`.

**Shown able to fail.** `ArgumentException.ThrowIfNullOrWhiteSpace(name);` removed from
`Area.cs:31`; `dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj --filter
"FullyQualifiedName~DungeonMaster.Tests.Domain"` -> `Failed: 3, Passed: 66, Total: 69`, the three
failures being the three cases of `AddArea_WithNullOrWhitespaceNameOnExistingFloor_...` with
*"Expected a <System.ArgumentException> to be thrown, but no exception was thrown."* Named green
control in the same run: `UpdateArea_WithNullOrWhitespaceNameOnExistingArea_...`, which routes
through a **different** guard (`AreaMethods.cs:24`) and so legitimately stays green. Restored; the
reviewer verified `Area.cs` byte-identical to base. Post-restore total, reproduced by the reviewer:
`Failed: 0, Passed: 69, Skipped: 0, Total: 69`.

**Three things were deliberately NOT tested, per escalation E2 -- record this as a decision, not an
omission.** A test cannot assert behaviour that does not exist, and deciding where a missing
invariant should live is a design decision outside a coverage session. They are filed instead:
**`MASTER-168`** (`Area.SetOutline` stores a degenerate or null outline -- this entry's clause (d)),
**`MASTER-169`** (`Floor` accepts a zero or negative floor number), **`MASTER-170`** (`PathSegment`
permits a `CubicBezier` with null control points).**]**

### TEST-021 -- App facts assert against the lexical form of application C#, not its behaviour

```yaml
id: TEST-021
layer: TEST
severity: S3
category: testability
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/BindablePropertyDefaultsMatchTokensTests.cs:84-88
  - src/DungeonMaster.Tests/App/Theme/BindablePropertyDefaultsMatchTokensTests.cs:129-133
  - src/DungeonMaster.Tests/App/Theme/DmThemeKeysResolveTests.cs:13-16
  - src/DungeonMaster.Tests/App/Theme/DmThemeKeysResolveTests.cs:136-141
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:188-194
effort: L
risk: high
independent: true
```

**What.** Several facts match regular expressions against the text of application C#.
`BindableDefault_EqualsTheTokenItMirrors` requires the literal sequence
`nameof(X), typeof(double), typeof(Y), <number>`. `ResolveCallPattern` requires
`Resolve<T>("DmKey")` written inline. `NoEagerDmThemeRead` matches a three-alternative pattern
against static declarations. `AppBindablePropertyPattern` requires
`BindableProperty.Create(nameof(...), typeof(...), typeof(...)`.

**Why it matters.** This is the sharpest form of the behaviour-versus-implementation problem in this
suite, and I state it as belief because whether it is acceptable depends on what these facts are
for. My reading: several behaviour-preserving edits break them. Extracting the bindable property's
type to an alias, wrapping a `Resolve<T>` call in a local helper, or introducing a `const double`
for a default all change nothing a user or the runtime can observe, and all cause the pattern to
stop matching. Critically, most of them fail *open* -- `ResolveCalls` and `AppPropertyTypes` carry
`NotBeEmpty` guards, but `BindableDefault_EqualsTheTokenItMirrors` fails closed with a clear message
while `NoEagerDmThemeRead` simply reports no offenders. So a refactor either produces a red test for
a green change, or silently retires the guard.

**Suggested direction.** Where the goal is a value (a default matching a token), a Roslyn-based
check or a small reflection probe over the built assembly would read the real declaration rather
than its spelling. Where the goal is a prohibition (`NoEagerDmThemeRead`), accept that it is a
lint rule and consider expressing it as a Roslyn analyzer, which is what it is.

### TEST-022 -- Arrange/Act/Assert structure is used in one sub-tree and absent from three

```yaml
id: TEST-022
layer: TEST
severity: S4
category: style
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs:23-43
  - src/DungeonMaster.Tests/Application/Common/Map/MapDrawingSessionTests.cs:10-17
  - src/DungeonMaster.Tests/Application/Common/Map/TileCoordinateHelperTests.cs:17-22
  - src/DungeonMaster.Tests/Domain/Entities/Area/AreaOutlineTests.cs:10-32
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:34-49
effort: S
risk: low
independent: true
```

**What.** The sixteen handler test classes carry explicit `// Arrange`, `// Act`, `// Assert`
comments and blank-line separation. `Application/Common/Map/`, `Domain/` and `Infrastructure/` carry
neither: `MapDrawingSessionTests.PlaceVertex_FirstVertex_ReturnsAdded` is six consecutive statements
with no separation at all, and `TileCoordinateHelperTests` puts the Act and the first Assert on
adjacent lines.

**Why it matters.** The cited page's "Arrange your tests" section is explicit that separating the
three phases "clearly highlights the dependencies required to call your code, how your code is
called, and what you're trying to assert", and that "readability is one of the most important
aspects when writing a unit test". The concrete cost here is a reader who has learned the convention from the handler tests and
then loses it in three of the four sub-trees; in a run of six undifferentiated statements it takes a
second read to find which line is the Act.

**Suggested direction.** Apply the same convention across all four sub-trees. At minimum, insert
blank lines between the phases even where the comments are judged noise -- the separation carries
most of the benefit.

### TEST-023 -- Single-letter lambda parameters appear in thirteen places

```yaml
id: TEST-023
layer: TEST
severity: S4
category: naming
authority: conventions
citation: backend/dotnet-format.md#Lambda Variable Names
locations:
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:29
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:43
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:68
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:51
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:52
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs:37
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs:52
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/CreateDungeonCommandValidatorTests.cs:37
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/CreateDungeonCommandValidatorTests.cs:52
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateAreaCommandValidatorTests.cs:37
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateAreaCommandValidatorTests.cs:52
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateDungeonCommandValidatorTests.cs:37
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateDungeonCommandValidatorTests.cs:52
effort: XS
risk: low
independent: true
```

**What.** `DungeonAggregateTests` uses `f` for a floor in three `Select`/`OrderBy` calls,
`DungeonReadRepositoryTests` uses `s` for a summary in two `First` calls, and all four validator
test classes use `e` for a validation failure in `ContainSingle` -- twice each, at lines 37 and 52,
which is eight of the thirteen sites. The rest of the project uses
descriptive lambda names throughout -- `dungeonFloor`, `placedObject`, `character`, `command` --
including in the same files.

**Why it matters.** `dotnet-format.md` states the rule without qualification: "Use descriptive names
in lambda expressions -- never single-letter variables", with `users.Where(u => u.IsActive)` given
as the incorrect form. Thirteen sites deviate. With no `.editorconfig` and no analyzers in the
repository, nothing but review enforces this, so the count will grow.

**Suggested direction.** Rename to `floor`, `summary` and `failure` respectively. Consider whether
this is one of the rules worth encoding once an `.editorconfig` exists (T6 owns that question).

### TEST-024 -- ObjectScaleTests is the only file in the project indented with tabs

```yaml
id: TEST-024
layer: TEST
severity: S4
category: style
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/Domain/Entities/Floor/ObjectScaleTests.cs:8-28
effort: XS
risk: low
independent: true
```

**What.** A scan for leading tab characters across the 45 files returns exactly one:
`Domain/Entities/Floor/ObjectScaleTests.cs`. The other 44 use four spaces.

**Why it matters.** I state this as belief because the repository has no `.editorconfig` and
`dotnet-format.md` states no *dedicated* indentation-character rule. It is not silent on the
question, though, and the signal it does give points the other way: every indentation instruction it
gives is denominated in tabs -- section 3 measures declaration layout in "+1 tab" steps
(lines 240-247), section 6's layout table repeats that in every row (lines 286-291), and section 9
requires XML tag descriptions "indented with **two tabs** from the `///` marker" (line 375) -- and
299 of the document's sample lines are tab-led. So `ObjectScaleTests.cs` is the outlier within its
own project while being the one file that matches the convention document's only signal, and the
other 44 are the outliers against that signal. My belief is that the inconsistency is the finding
either way: a mixed-indentation project produces spurious whitespace diffs and inconsistent
rendering, and nothing in the repository *decides* the question, which is what would have to happen
first.

**Suggested direction.** Do not convert this file on the strength of majority alone -- the majority
runs against `dotnet-format.md`'s tab-denominated instructions, so converting it to spaces would
move the project further from its own convention document, not closer. Settle the question first:
which indentation character this repository uses, and where that is written down. T6 owns the absent
`.editorconfig` and is the right place to decide it; whichever character wins, one file or 44 then
change to match.

### TEST-025 -- XML documentation is present on 21 of 45 files and absent from 24

```yaml
id: TEST-025
layer: TEST
severity: S4
category: docs
authority: conventions
citation: backend/dotnet-format.md#9. XML Documentation Comments
locations:
  - src/DungeonMaster.Tests/Domain/Entities/Area/AreaOutlineTests.cs:8
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:6
  - src/DungeonMaster.Tests/Domain/Entities/Floor/PlacedObjectTests.cs:7
  - src/DungeonMaster.Tests/Application/Common/Map/MapDrawingSessionTests.cs:7
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs:10
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/CreateDungeonCommandValidatorTests.cs:6
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:12
effort: M
risk: low
independent: true
```

**What.** All thirteen `App/` files, six `Application/` files and two `Infrastructure/` files carry
XML documentation on their public types and members. The remaining 24 -- all five `Domain/` files,
all three `Common/Map/` files, all four validator classes, nine of the fourteen command-handler
classes, one query class and two `Infrastructure/` classes -- carry none, not even a type summary.
Every one of these is a `public` type with `public` members.

**Why it matters.** `dotnet-format.md` section 9 opens with "All public types, members, and
parameters must have XML documentation comments", without exempting test code. The split is not
random -- it tracks which sub-tree the file is in -- so the effect is that a reader learns to expect
documentation in `App/` and stops looking for it elsewhere, and a maintainer adding a file has no
signal about which side of the line their file falls on.

**Suggested direction.** Decide whether the rule applies to test classes and record the decision.
If it does, the gap is 24 files of type summaries plus per-method summaries; if it does not, note
the exemption in `dm-conventions` and consider trimming the 21 that have them, since the `App/`
documentation is genuinely load-bearing (much of it explains why a fact exists) and should survive
either way.

### TEST-026 -- Seven non-sealed test classes implement IDisposable with no Dispose(bool) overload

```yaml
id: TEST-026
layer: TEST
severity: S4
category: disposal
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
locations:  # AMENDED 6b-ii -- seven classes; each cited at its declaration and its Dispose, at 850a10b
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:16,34
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:10,28
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:13,31
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs:16,34
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/UnitOfWorkTests.cs:13,35
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Configurations/AreaOutlineChangeTrackingTests.cs:17,35
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Configurations/PlacedObjectConfigurationTests.cs:19,37
effort: XS
risk: low
independent: false
blocked-by:
  - TEST-009
```

**What.** All seven Infrastructure test classes are declared `public class ... : IDisposable` -- not
`sealed` -- and implement disposal as a single expression-bodied
`public void Dispose() => _connection.Dispose();`. There is no `Dispose(bool)` overload and no
guard against a second call.

**[AMENDED 6b-ii, 2026-09-08 -- CORRECTION 7, the class count. Classification: DRIFT.** Previous
wording, verbatim, in the title: *"Four non-sealed test classes implement IDisposable"*, and in the
body: *"All four Infrastructure test classes are declared `public class ... : IDisposable`"*. The
measured figure is **seven**:

```
git grep -n "class .*: *IDisposable\|class .*, *IDisposable" 850a10b -- src/DungeonMaster.Tests | grep -v sealed | wc -l
```

-> `7`, and without the `wc` the seven names are exactly the same seven classes that call
`EnsureCreated` in `TEST-002`. The three this entry misses are the same three `TEST-002` and
`TEST-009` miss.

**Classification evidence.** The same command at `2603170` -> `4`, listing precisely the four this
entry names. Per `TEST-002`'s CORRECTION 1, all three additional classes were added on 2026-08-18,
the day after this document was written. The entry was correct on the day.

**Recorded discrepancy -- a self-inconsistency this entry was accused of and does not have.** Session
6b-ii's manifest directed that this entry *"under-counts against its own `locations` list"*, on the
grounds that the list carried five entries across four files while the body said four. Measured at
`2603170`, the fifth entry is not a fifth class: `AreaPersistenceDiagnosticTests.cs:15` is the class
declaration and `:33` is its `Dispose`, while the other three files were cited at their `Dispose` line
only. `git show 2603170:<file> | sed -n '<n>p'` on all five confirms it. **The list was internally
consistent -- five citations, four classes -- and inconsistent only in how much it cited per file.**
The list is now regularised to cite both anchors for all seven. This is recorded rather than quietly
dropped, because an alleged defect that measurement does not support is exactly the kind of claim this
pass exists to stop propagating.**]**

**Why it matters.** The cited page states that "any non-sealed class should have a `Dispose(bool)`
overload method" and that "a `Dispose` method should be idempotent, such that it's callable multiple
times without throwing an exception". Neither holds here. The practical exposure is small -- xUnit
calls `Dispose` once per instance and `SqliteConnection.Dispose` is itself idempotent -- but the
declaration invites a derived fixture that would have no correct way to add its own cleanup, which
is exactly the direction `TEST-009` proposes.

**Suggested direction.** The simplest fix is `sealed` on all seven, which removes the requirement
outright and is correct for a leaf test class. If the shared base class from `TEST-009` is built
instead, that base is the one type that needs the full pattern.

### TEST-027 -- Two incompatible test-naming schemes are in use

```yaml
id: TEST-027
layer: TEST
severity: S4
category: naming
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs:22
  - src/DungeonMaster.Tests/Domain/Entities/Floor/ObjectScaleTests.cs:11
  - src/DungeonMaster.Tests/App/Theme/ThemeHygieneTests.cs:71
  - src/DungeonMaster.Tests/App/Theme/ThemeDictionaryTests.cs:121
  - src/DungeonMaster.Tests/App/Theme/DmThemeKeysResolveTests.cs:51
  - src/DungeonMaster.Tests/App/Theme/TokenNamingTests.cs:179
effort: M
risk: low
independent: true
```

**What.** The `Application/`, `Domain/` and `Infrastructure/` sub-trees use
`Method_Scenario_ExpectedBehaviour` -- `HandleAsync_WithExistingDungeonAndFloor_AddsAreaAndReturnsId`,
`StepUp_ReturnsExpected`. The `App/` sub-tree uses declarative sentences with no method under test
and no separators -- `NoRawColourLiteralsOutsideTheThemeDictionaries`,
`TokensIsTheOnlyThemeFileContainingColourLiterals`,
`EveryColourTokenObeysTheNamingRule`. A minority in `App/` splits the difference
(`EveryKeyDmThemeResolves_IsDefinedInTheThemeDictionaries`).

**Why it matters.** The cited page prescribes the three-part form -- method, scenario, expected
behaviour -- so that "just by looking at the suite of unit tests, you should be able to infer the
behavior of your code". The `App/` names are readable in isolation but do not name a method, which
is defensible given there is no method under test: these facts assert over files. So the deviation
is partly justified by what the sub-suite does. What is not justified is the third scheme mixed into
the same directory, which leaves a reader with no rule to follow.

**Suggested direction.** Keep the sentence form in `App/` -- it fits its subject -- but make it
uniform within that directory by removing the underscore-separated hybrids, and state the two
schemes and where each applies somewhere a new file's author will see it.

### TEST-028 -- Comment-stripping regexes and a line-number helper are re-declared instead of shared

```yaml
id: TEST-028
layer: TEST
severity: S4
category: duplication
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Duplicated Code"
locations:
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:53-56
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:220-229
  - src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:720-723
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:70-73
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:247-253
  - src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs:112-115
  - src/DungeonMaster.Tests/App/Theme/ThemeTokenParityTests.cs:32-35
  - src/DungeonMaster.Tests/App/Theme/ThemeDictionaryTests.cs:20-23
effort: S
risk: low
independent: true
```

**What.** `ThemeSource` exists, by its own summary, because "the patterns and file scans here were
duplicated across five test classes". An XML-comment pattern is nonetheless declared three times
(`ThemeSource`, `GeometryScaleTests`, `IconFontCoverageTests`), a C#-comment pattern twice
(`ThemeDictionaryTests`, `ThemeTokenParityTests`), and the identical
`text.Take(index).Count(c => c == '\n') + 1` line-number computation three times
(`ThemeSource.LineNumber`, `IconFontCoverageTests.LineNumber`, and inline in
`SemanticDescriptionTests`).

**Why it matters.** This is the divergence `ThemeSource`'s own documentation says the class was
created to prevent, reappearing after it. Two of the three comment patterns differ deliberately --
`GeometryScaleTests` needs a `body` capture group so it can blank the interior and preserve offsets
-- so the fix is not a blind merge; but the three identical line-number copies have no such excuse,
and the two comment-line patterns in `ThemeDictionaryTests` and `ThemeTokenParityTests` differ only
by an anchor that is arguably a bug in one of them.

**Suggested direction.** Move the blanking helper and the line-number helper into `ThemeSource` as
the single home the class was created to be, and have the two C#-comment strippers agree on whether
the pattern is anchored (`GeometryScaleTests.cs:727-730` documents why it must be).

### TEST-029 -- Several tests carry two or more independent reasons to fail

```yaml
id: TEST-029
layer: TEST
severity: S4
category: style
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs:137-154
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:128-144
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:156-171
  - src/DungeonMaster.Tests/App/Theme/TokenNamingTests.cs:263-272
  - src/DungeonMaster.Tests/Application/Common/Map/MapDrawingSessionTests.cs:13-17
effort: S
risk: low
independent: true
```

**What.** `EveryIconOnlyControlHasASemanticDescription` asserts offenders are empty and then that
the scan reached something. `EveryDmFontIconTextIsInTheIconFontsCmap` does the same.
`TheIconFontCmapParsesToPrivateUseGlyphs` makes four unrelated assertions.
`EveryRegisteredExceptionStillExistsAndStillOffends` asserts that every reason string is non-blank
and separately that the complaint list is empty. `PlaceVertex_FirstVertex_ReturnsAdded` asserts the
result, the vertex count and the session state.

**Why it matters.** The cited page's "Avoid multiple Act tasks" section notes that once an assertion
fails, subsequent ones do not execute, so "some working functionality might be interpreted as
failing". In the anti-vacuity cases the ordering also inverts the diagnosis: because the offender
assertion runs first, a suite that has silently stopped scanning anything is only reported when the
offender list happens to be empty, which is precisely the state it is supposed to distinguish from
success.

**Suggested direction.** Split the anti-vacuity assertions into their own facts (this is the same
change `TEST-016` proposes and should land with it), and split
`TheIconFontCmapParsesToPrivateUseGlyphs` into one fact per property of the parse.

### TEST-030 -- Assertions appear inside Arrange blocks

```yaml
id: TEST-030
layer: TEST
severity: S4
category: style
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs:145
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs:186
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:79
effort: XS
risk: low
independent: true
```

**What.** Two tests in `UpdatePlacedObjectCommandHandlerTests` obtain the object under test with
`floor.Objects.Should().ContainSingle().Which` under an explicit `// Arrange` comment:
`HandleAsync_ObjectNotFound_ReturnsSuccessWithNoChange` binds it as `theObj` at line 145, and
`HandleAsync_WithCustomScale_StoresScaleOnObject` binds it as `obj` at line 186.
`AreaPersistenceDiagnosticTests.RemoveArea_WithEmptyOutline_PersistsAcrossReload` asserts
`Outline.Should().BeEmpty()` in the middle of the block that performs the removal.

**Why it matters.** The cited page's "Arrange your tests" section says the pattern "helps to reduce
the opportunity for assertions to intermix with code in the Act task", and separating the phases is
what makes a failure legible. Here a failure in the Arrange assertion is reported as a failure of
the test's stated behaviour, so `HandleAsync_ObjectNotFound_ReturnsSuccessWithNoChange` can go red
for a reason that has nothing to do with the object not being found.

**Suggested direction.** Replace the fluent extraction with `floor.Objects.Single()` in Arrange, and
move the outline assertion in the diagnostic test into its own fact -- or delete it, since
`TEST-012` proposes removing that test entirely.

### TEST-031 -- One theme ratchet is structurally incapable of failing in its current state

```yaml
id: TEST-031
layer: TEST
severity: S5
category: dead-code
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/ThemeHygieneTests.cs:65
  - src/DungeonMaster.Tests/App/Theme/ThemeHygieneTests.cs:131-151
effort: XS
risk: low
independent: true
```

**What.** `EveryAllowListedFileExistsAndStillOffends` iterates `PendingMigration`, which is
initialised empty at line 65. The `Where` therefore produces nothing, and the test asserts that an
empty list is empty. The class documents this at length, calls it "vacuous today and that is the
point", and argues it is a self-arming ratchet rather than dead code.

**Why it matters.** I find the author's reasoning persuasive and I am recording this as an
observation rather than a deviation. The argument -- that the guard is needed at the exact moment
someone adds the first allow-list entry, which is also the moment nobody would think to write it --
is sound, and the equivalent guards in `GeometryScaleTests` and `SemanticDescriptionTests` do bite
today, so the shape is proven. My only note is that a maintainer running the suite has no way to
distinguish this test from one that has silently stopped reaching anything, which is the failure
mode the rest of the sub-suite works hard to make visible.

**Suggested direction.** If it is kept -- and I believe it should be -- consider marking it in a way
that says "intentionally inert while the list is empty" to the person reading a test report rather
than only to the person reading the source; a `[Fact(DisplayName = ...)]` naming the condition would
do it.

### TEST-032 -- coverlet is referenced but there is no runsettings, no collection configuration and no CI invocation

```yaml
id: TEST-032
layer: TEST
severity: S5
category: dependency
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:11
effort: S
risk: low
independent: true
```

**What.** `coverlet.collector` 6.0.4 is referenced by the test project. There is no `.runsettings`
file anywhere in the repository, no `[assembly: CollectionBehavior]` attribute, no
`xunit.runner.json`, and no CI configuration visible. `dotnet test` therefore runs with xUnit's
defaults and produces no coverage output unless someone passes
`--collect:"XPlat Code Coverage"` by hand.

**Why it matters.** I believe the absent `.runsettings` matters less for coverage than it first
appears -- the interesting question in this suite is which behaviours are untested (`TEST-001`,
`TEST-019`, `TEST-020`), and a percentage would not surface any of those; the Microsoft guidance is
itself explicit that a coverage number does not imply quality. What the absent configuration does
mean is that the package is present and unused, and that nothing pins the parallelism settings the
suite currently relies on. The suite is safe under the defaults today (see the layer verdict), but
that safety is a property of how the fixtures happen to be written, not something the repository
states. The project-file record itself belongs to T6.

**Suggested direction.** Either remove `coverlet.collector` or add the `.runsettings` and the CI
step that uses it. Separately, consider an explicit `[assembly: CollectionBehavior]` declaration so
the parallelism the fixtures depend on is written down rather than inherited.

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 1, the scope is triple today, and the cited line was wrong when written. Classification: MEASUREMENT ERROR (the cited line); DRIFT (the singular scope).**
Previous wording, verbatim: *"`coverlet.collector` 6.0.4 is referenced by the test project"*, over a
`locations` list of one entry, `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:11`. **Three test
projects referenced it, not one**, with the version held centrally. Measured at `1407090` by this
session's T4, which confirmed every line below before editing it:
`src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:9`,
`src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj:26`,
`src/DungeonMaster.App.UnitTests/DungeonMaster.App.UnitTests.csproj:20`, and
`Directory.Packages.props:20`.

**Classification evidence, the cited line: MEASUREMENT ERROR.** `:11` was **wrong when written**
rather than overtaken. At the audit commit `413be78` (2026-08-17):

```
$ git show 413be78:src/DungeonMaster.Tests/DungeonMaster.Tests.csproj | grep -n "PackageReference"
9:    <PackageReference Include="coverlet.collector" />
10:    <PackageReference Include="FluentAssertions" />
11:    <PackageReference Include="Microsoft.NET.Test.Sdk" />
12:    <PackageReference Include="NSubstitute" />
13:    <PackageReference Include="xunit" />
14:    <PackageReference Include="xunit.runner.visualstudio" />
```

coverlet stood at line **9** on the audit date -- line 11 was `Microsoft.NET.Test.Sdk` -- and 9 is the
same line it still stood at when T4 removed it, so it did not move in between. Authorship error, not
drift.

**Classification evidence, the singular scope: DRIFT.** The entry's *"the test project"*, singular,
was **correct when written**. Neither App-side test project existed at `413be78`:

```
$ git ls-tree -r --name-only 413be78 -- src/DungeonMaster.App.Tests      | wc -l
0
$ git ls-tree -r --name-only 413be78 -- src/DungeonMaster.App.UnitTests  | wc -l
0
```

`DungeonMaster.Tests` was the only test project there was. The scope tripled afterwards, when later
sessions added the two App-side projects: the record did not under-count, it was overtaken.

Both commands were run read-only by this session's orchestrator, which is not barred from git as its
tasks are, and are recorded verbatim in
`.dir-sdlc/sessions/2026-09-11-measure-then-close-the-test-gaps/handover-addendum.md`. Round 1 of this
pass cited `8bb2d79` for the line; that commit is dated **2026-08-18**, one day *after* the audit, and
so cannot establish what was true when the record was written. The citation is replaced above; the
verdict it supported is unchanged.**]**

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 2, the suggested remedy is a MEASURED REGRESSION. DO NOT IMPLEMENT IT. Classification: UNDETERMINED as to the audit date; measured harmful today.**
Previous wording, verbatim: *"Separately, consider an explicit `[assembly: CollectionBehavior]`
declaration so the parallelism the fixtures depend on is written down rather than inherited."*
Session 6a measured exactly this: `DisableTestParallelization` serialises collections **without
ordering them**, and the spike recorded **3 crashes in 11 runs, ~27% nondeterministic**
(`.dir-sdlc/sessions/2026-09-02-app-sub-suite-rebuild/spike-parallelism-report.md:204-205` and
`:407` -- the artifact those two figures are counted from), and `manifest.json:293` of that session
states it **must not be adopted as a mitigation**.

**This correction is the non-optional part of this entry's closure.** Left uncorrected, the entry
reads as a standing recommendation and a future reader implements it in good faith, buying a ~27%
flaky suite in exchange for writing down a property the repository already states (CORRECTION 3).
This session's T4 added **no** such attribute anywhere.

**Classification evidence.** *Not settled for the audit date.* The spike ran on 2026-09-02 against
the App sub-suite that sessions 6a and 6b-i built, which did not exist on 2026-08-17, so this pass
cannot say the suggestion was harmful when it was made. What is measured is that it is harmful
now.**]**

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 3, the parallelism the entry calls unpinned IS pinned today. Classification: DRIFT.**
Previous wording, verbatim: *"that safety is a property of how the fixtures happen to be written, not
something the repository states."* The repository does state it, in the form that works --
collection declarations rather than an assembly-level behaviour switch: `MauiHostCollection.cs:7-8`
and `AppTestCollection.cs:27-28`, with **nine classes declaring membership**, plus a runtime latch at
`MauiHostFixture.cs:78-84` that **throws, naming the required collection**, if a test runs outside
it. Counted from this session's `measurement.md` at `1407090`. The claim is falsified twice over: by
the declarations, and by the latch that enforces them at run time rather than by convention.

**Classification evidence.** *Measured at the audit commit.* The entry was **right when it was
written**: none of the three files that pin the parallelism today existed at `413be78` (2026-08-17).

```
$ git ls-tree -r --name-only 413be78 | grep -ci "MauiHostCollection\|AppTestCollection\|MauiHostFixture"
0
```

Nothing pinned the parallelism on the audit date, so the claim was accurate and the repository moved
underneath it -- the pinning arrived later, with the App sub-suite rebuilt by sessions 6a and 6b-i,
and in the form the 6a spike settled on (`[CollectionDefinition]` plus explicit membership) rather
than the `[assembly: CollectionBehavior]` this entry suggested, which is why that suggestion was never
the thing that fixed it (CORRECTION 2). Command run read-only by this session's orchestrator and
recorded in `.dir-sdlc/sessions/2026-09-11-measure-then-close-the-test-gaps/handover-addendum.md`.**]**

**[AMENDED 6b-iii, 2026-09-11 -- CORRECTION 4, what the entry got right, and the closure. Classification: not a correction -- confirmation.**
All four absences the entry lists were **still literally true** at `1407090`: coverlet present and
unused, no `.runsettings`, no `xunit.runner.json`, no `[assembly: CollectionBehavior]`, and no CI --
the last **verified rather than assumed** (zero tracked `.yml`/`.yaml` files; no `.github/`
directory -- recorded in this session's `measurement.md` at `1407090`). The entry's own reasoning about coverage percentages is also left standing.

**Closure -- `MASTER-155`, T4, commit `5347813`, per escalation E1: the package is removed rather
than configured.** The four lines in CORRECTION 1 were deleted.
`dotnet restore src/DungeonMaster.sln --force-evaluate` regenerated the lock files **by tooling** --
no lock file was hand-edited or deleted, and no mismatch was reported. Of nine `packages.lock.json`,
**3 content-changed** (`DungeonMaster.Tests`, `App.Tests`, `App.UnitTests`) and **6 unchanged**; each
changed file lost exactly one six-line `coverlet.collector` `Direct` block and nothing else, with **no
other `resolved` version moving**. `App.UiTests` never referenced coverlet and is excluded from the
solution, so it needed no separate restore. The reviewer re-ran with `--locked-mode
--force-evaluate`, which fails if the committed locks disagree with the tooling: it restored clean.
`dotnet build src/DungeonMaster.sln` -> `Build succeeded. 0 Warning(s), 0 Error(s)` across all eight
projects. Execution unaffected:
`dotnet test src/DungeonMaster.App.UnitTests/DungeonMaster.App.UnitTests.csproj` -> `Failed: 0,
Passed: 23, Total: 23`, and `dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj --filter
"FullyQualifiedName~DungeonMaster.Tests.Application.Common.Results"` -> `Failed: 0, Passed: 22,
Total: 22`. Central Package Management intact:
`grep -rn 'PackageReference[^>]*Version=' --include=*.csproj --include=*.props --include=*.targets`
returns no hits, and `Directory.Packages.props` now holds **15** `PackageVersion` entries, down from
16 -- T4's own count at `5347813`.**]**

## Layer verdict

The suite holds its shape in Domain and Application and loses it in `App/`. The Domain tests drive
behaviour through the aggregate root -- `AreaOutlineTests` goes through `Dungeon`, never `Area` --
so they survive a refactor of entity internals: the right seam. The Application handler tests follow
`dotnet-format.md` section 12 exactly, and NSubstitute use is sound rather than over-mocked: one
substituted repository interface per class, nothing the test owns mocked. FluentAssertions is
specific enough that most failures diagnose themselves; `Should().BeEquivalentTo` appears once
(`ThemeDictionaryTests.cs:156`), correctly. Verification style is the one real
weakness: `Received(1)` on paths whose own names say nothing changed (`TEST-010`) is the suite's
clearest implementation coupling, and a contained one.

xUnit usage is correct, and I checked shared state explicitly: the suite is **sound under xUnit's
default parallelism** -- each test method gets a private database, the only static state is immutable
after initialisation; the working is in `## Method`. `[Theory]` and `[MemberData]` fit their uses,
though four geometry facts should join them (`TEST-013`).

**[AMENDED 6b-ii, 2026-09-08 -- cross-reference corrected, and it was wrong on both halves.
Classification: DRIFT.** Previous wording, verbatim: *"though three geometry and five snap-zone facts
should join them (`TEST-013`)."* It is **four** geometry facts, not three -- `NoBareMinimumSizeValues`
is the fourth, per `TEST-013` CORRECTION 5(c) -- and the five snap-zone facts **no longer exist**, per
CORRECTION 5(a). Left uncorrected, this sentence would have contradicted the corrected entry it cites.
Both halves were accurate at `2603170`; the evidence and dating are recorded once under `TEST-013`
rather than repeated here.

The full sweep for cross-references outside the seven records was
`git grep -n "snap-zone\|snap zone\|nineteen\|four Infrastructure" 850a10b -- docs/audit/05-tests.md`,
which returned six hits: `:143` and `:1072` (the `TEST-002` and `TEST-026` bodies, corrected in
place), `:405` (the `TEST-009` title, corrected), `:432` (the `TEST-009` body, corrected), `:552` (the
`TEST-013` title, corrected) and `:1328` (this sentence). **No hit was left alone.** The `## Summary`
severity counts are deliberately **not** changed: this pass corrected entries and added none, removed
none and re-severitied none.**]**

My verdict on `App/`: **reading source files off disk is a defensible way to enforce theme and
accessibility hygiene, and these are not tests -- they are a linter wearing a test's clothes, and
the disguise is costing them.** The guards are excellent; the icon-font cmap and token-type checks
each caught a real shipped defect. The mechanism is wrong in three ways. Path resolution is ambient
and unverified, so the sub-suite depends on a `.git` entry in an ancestor directory (`TEST-003`).
Eleven of the thirteen files assert over XAML by regex while a twelfth proves `XDocument` does it
better in fewer lines (`TEST-004`); the anti-vacuity guards exist because regex scans fail silently.
And several facts assert against the lexical spelling of C# (`TEST-021`), so a behaviour-preserving
refactor either reddens a green change or silently retires the guard. The alternative -- Roslyn
analyzers for the prohibitions, a project reference for the value checks -- costs real work and is
not obviously worth it; but at 4,690 lines and growing, it deserves weighing.

Two seams sit wrong regardless. The theme guards share a `dotnet test` invocation with the unit
tests, so a lint failure and a regression are indistinguishable; and `ThemeSource`, the
intended one home for shared scanning, is already bypassed (`TEST-028`). The absent
`DungeonMaster.App` project reference is a boundary record T6 owns.

## Attestation

- No file under `src/` was created, modified, or deleted by this task: yes
- Every record cites at least one file path with a line number: yes
- Every record names its authority, and every non-`judgment` record carries a real citation: yes
