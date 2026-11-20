# TEST hygiene sweep

Close register rows MASTER-126, 128, 137, 139, 143 and 144 by cleaning up the structure of `src/DungeonMaster.Tests`, without changing any production behaviour.

## Context

The six rows are the first grouped section of `.sir/backlog.md` (lines 5-12). The 2026-08-17 code-quality audit filed all six, and every one said "re-measure before scheduling". The spec phase re-measured them at `9c931b4`. Every figure below has its command in `contracts/C1-measurement.md`.

- **MASTER-126:** 7 single-letter lambda sites, not 13. The four validator test classes were merged into `NameOnlyValidatorTestBase`, which took 8 sites down to 2.
- **MASTER-128:** the 2 assertions inside Arrange blocks in `UpdatePlacedObjectCommandHandlerTests` (`:145`, `:195`) are still there. The diagnostic-test half is **stale**: `AreaPersistenceDiagnosticTests.cs` was deleted in `cf32061`.
- **MASTER-137:** the sub-trees have moved since the audit (`Application/Common/Map` is now `AppCore/Map`). By the `aaa.pl` checker, 205 methods lack Arrange/Act/Assert structure: AppCore 126, Domain 63, Infrastructure 16.
- **MASTER-139:** all five named facts still have more than one reason to fail. In one of them (`EveryDmFontIconTextIsInTheIconFontsCmap`), the guard already runs first, so the audit's claim that the diagnosis is inverted no longer holds there.
- **MASTER-143:** 18 of 109 `*Tests.cs` files have no `/// <summary>`, not 24 of 45.
- **MASTER-144:** `App/` holds 58 test names with an underscore. Of the rest, 88 are in sentence form. Outside `App/`, 3 names are in sentence form.

The baseline in the worktree is a solution build with 0 warnings and 0 errors, and these passing tests: DungeonMaster.Tests 943, App.UnitTests 187, App.Tests 123.

## Definition of done

- [ ] **MASTER-126:** P1 and P2 (C1 §1) print nothing over `src/DungeonMaster.Tests`. The 7 parameters are renamed to `failure`, `floor` and `summary`, or equivalent descriptive names.
- [ ] **MASTER-128:** PA (C1 §2) prints nothing over `src/DungeonMaster.Tests`. Both Arrange blocks use `floor.Objects.Single()`. The `obj`/`theObj` locals are renamed. The diagnostic half is closed as stale, with `cf32061` as evidence.
- [ ] **MASTER-137:** `aaa.pl` (C1 §4) prints nothing over `AppCore/`, `Domain/` and `Infrastructure/`, and no assertion is removed, weakened or reordered.
- [ ] **MASTER-139:** the five named facts are split as C5 §2 sets out (+9 facts), and each new fact is shown able to fail by the injections in C5 §4.
- [ ] **MASTER-143:** the exemption of test classes from the XML-documentation rule is recorded in the README. No doc comments are added or removed.
- [ ] **MASTER-144:** N1 (C1 §3) prints 0, so `App/` is uniformly in sentence form. N2 prints 0, so outside `App/` everything is `Method_Scenario_Expected`. No old name survives anywhere under `src` (REF, C1 §10).
- [ ] **README:** `src/DungeonMaster.Tests/README.md` states both naming schemes and where each applies, the XML-doc exemption, and the AAA rule, as C3 requires.
- [ ] **Register:** the six status cells in `docs/audit/00-master-index.md` are closed, and the six `AMENDED` blocks in `docs/audit/05-tests.md` are in place, as C4 requires.
- [ ] **Whole solution:** the build prints 0 warnings and 0 errors. DungeonMaster.Tests passes 952 (943 + 2 + 7), App.UnitTests 187 and App.Tests 123, with 0 failed.

## Scope

All six rows. There are eight tasks: T1 to T6 and T8 run as a chain, and T7 (the README) runs alongside them. The tasks are split by file and sub-tree, not by row, so that no two tasks own the same file. Every change is inside `src/DungeonMaster.Tests/` and `docs/audit/{00-master-index,05-tests}.md`, with two exceptions:

- **Four comment lines outside the test project** that name a test this session renames. T6 updates them, and in each of those files the named line must be the only change:
  - `src/DungeonMaster.App.UnitTests/Theme/DmThemeResolutionTests.cs:92`
  - `src/DungeonMaster.App/Resources/Theme/Controls.Base.xaml:114`
  - `src/DungeonMaster.App/Resources/Theme/Tokens.xaml:511`
  - `src/DungeonMaster.App/Services/Platform/PlatformSeams.Unsupported.cs:22`
- **Files owned only for injection:** `src/DungeonMaster.App.Core/Map/MapDrawingSession.cs` (T2), and `IconFontFixture.cs` and `TrueTypeCmap.cs` (T5). These files must be byte-identical at the end of each task.

### Not in scope

- The other test projects: App.Tests, App.UnitTests (apart from the one comment line above) and App.UiTests.
- `dm-conventions`. It is external and read-only, and nothing is written there.
- The `ct` → `cancellationToken` rename (MASTER-130 / `backlog.md:39`).
- Arrange/Act/Assert in `App/`, whose tests are source scans, and the 3 Application methods that deviate from it (C1 §4). These are reported and not worked.
- Facts with multiple reasons to fail beyond the five MASTER-139 names. An unvalidated census found 15 candidates, and that figure is reported only.
- Hybrid underscore names outside `App/`, such as `ObjectKindGuardTests.cs:29`. These are already `Method_…` form by N2.
- Adding doc comments to test files, and trimming the existing ones.
- Removing the rows from `.sir/backlog.md`. The orchestrator does that at Gate 3.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Where are the MASTER-143/144 conventions recorded? | In a new `src/DungeonMaster.Tests/README.md`, not in `dm-conventions` | repo | `src/DungeonMaster.App.UiTests/README.md` is the only convention doc kept inside a test project. 6b-xviii recorded its migration exemption inside this repo (the UIKIT-022 amendment). `dm-conventions` is a separate repository. |
| Which test classes does the exemption cover? | Test classes in DungeonMaster.Tests. Helper types (fixtures, scanners) are not covered. | judgment | C3 §3 |
| Is MASTER-137's rule "blank lines only" or markers plus blank lines? | `// Arrange`/`// Act`/`// Assert` markers with blank lines between phases. A single-statement body is exempt, and Arrange is omitted when there is no setup. | repo | dotnet-format.md §12. The handler tests: Application has 204 of 207 methods conformant. A rule of blank lines alone cannot be checked mechanically. |
| Which sub-trees does MASTER-137 cover? | AppCore (the relocated `Common/Map`, plus its siblings), Domain and Infrastructure. `App/` is excluded. | repo | TEST-022 names those three sub-trees, and `App/` facts are scans (C1 §4) |
| Are the 3 sentence-form names outside `App/` renamed? | Yes, to `Method_Scenario_Expected` | judgment | Gate 1: "Keep `Method_When_Then` elsewhere" (C2 §5) |
| Is `obj` at `UpdatePlacedObjectCommandHandlerTests.cs:39` renamed with the row's two sites? | Yes, so the whole file is consistent | judgment | `grep -cwE 'theObj\|obj'` prints 13 at startRef |
| MASTER-139 splits the guard out of the fact, but a 2026-09-02 DIR contract says the guard belongs in the same fact. Which wins? | Gate 1's decision. That contract governed only its own session. The comment at `IconFontCoverageTests.cs:62-66` that cites it is removed or rewritten. | repo | C5 §3, discussion.md Decisions |
| Can a task temporarily break a production file to show that a split fact can fail? | Yes, as long as the file ends byte-identical. The chain means no other task is building during the injection. | repo | C5 §4; dependsOn chain T1→T6→T8 |
| Can tasks build concurrently? | No. They all share one `obj/`/`bin/` tree. Every builder is chained, and T7 does not build. | repo | C1 §8 |
| README line endings | LF in the worktree (CR = 0). Git normalises it on commit. | repo | `.gitattributes` `* text=auto`; C3 §1 |

## Contracts

- [C1-measurement.md](contracts/C1-measurement.md): every predicate, its controls, the startRef figures, build and test commands, and editing hazards.
- [C2-naming.md](contracts/C2-naming.md): the two naming schemes, Options A and B, and the classification of the 58 names.
- [C3-readme.md](contracts/C3-readme.md): what the README must contain.
- [C4-register.md](contracts/C4-register.md): the six register cells and the six AMENDED blocks.
- [C5-splits.md](contracts/C5-splits.md): the MASTER-139 population, how each fact is split, and injections J1–J10.

## Open questions

None. At Gate 2 (2026-11-04) the user chose Option A for MASTER-144 (all 58 `App/` underscore names renamed), which is recorded in C2 §2.
