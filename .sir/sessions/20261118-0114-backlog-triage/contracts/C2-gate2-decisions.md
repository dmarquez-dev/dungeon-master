<!-- frozen at Gate 2 of 20261118-0114-backlog-triage; answers applied by the orchestrator -->

# Contract C2: Gate 2 decisions

The user's Gate 2 answers to the 23 open backlog rows whose action begins `Decide` at 4d874be (lines
7, 8, 20, 22, 23, 36, 40, 41, 50, 58, 65, 69, 101, 134, 135, 136, 147, 150, 151, 153, 157, 158, 161;
Python `re.match(r'^\S+ \S+ Decide ', line)` over the open rows of `C1-triage-record.md` section 1,
control: line 7 matches, line 9 does not). Lines 147, 151 and 157 ask one question and are one entry
(discussion.md, Decisions), so there are 21 entries, D1-D21.

Producer: the orchestrator, from the user's answers, before the freeze. Consumers: T1-T6 cite an
entry by id in a record's `decision` field and never decide its rows; T7 applies each entry's row
sub-blocks (`C1-triage-record.md` section 6, Resolution). Line numbers are as at 4d874be.

## Entry grammar

Each entry is a `### D<n>` heading followed by these field lines, then one `#### row <N>` sub-block
for every line in `rows` and for any further open row the answer reaches (a non-Decide row named
here is overridden by this entry, and no row may appear in two entries):

```
### D<n>
- rows: <N>[, <N>...]
- subject: <session and subject of the row(s)>
- premise: <the orchestrator's re-measurement, with its command>  |  (pending)
- answer: <the user's choice, verbatim from the Gate 2 dialog>  |  (pending)

#### row <N>
- disposition: <remove|keep|rewrite|lesson>  |  (pending)
- lessons-line: <M|->
- new-text: -
```

`new-text` takes the three-part fenced form of `C1-triage-record.md` section 3 for `rewrite`, and for
`lesson` with `lessons-line` `-`; its line is shaped per that contract's section 5. `(pending)` is
the only placeholder; `C1-check.py c2` counts the sub-blocks still holding one, and T7 does not start
while that count is above 0.

## Entries

### D1
- rows: 7
- subject: 2026-09-17-core-type-identity, clock seam for `LastModified` tests without `Task.Delay`
- premise: Re-measured at 4d874be by the orchestrator: `git grep -n "Task.Delay" -- 'src/DungeonMaster.Tests*'` prints `EntityEqualityTests.cs:175` and `Floor/FloorMethodsTests.cs:18`, both `await Task.Delay(20);`; `git grep -n -E "TimeProvider|IClock|ISystemClock" -- src` prints nothing.
- answer: Backdate by reflection (Recommended)

#### row 7
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-09-17 2026-09-17-core-type-identity Drop the two `await Task.Delay(20)` calls in `EntityEqualityTests` and `FloorMethodsTests.UpdateNumber_WhenCalled_UpdatesLastModified` by backdating `LastModified` through reflection on its private setter, as `ForceId` does for `Id`, with no production clock seam (Gate 2 D1 of 20261118-0114-backlog-triage); check: `git grep -n Task.Delay -- 'src/DungeonMaster.Tests*'` prints nothing and each test still fails when `LastModified` is not updated. -- source: .dir-sdlc/backlog.md:351; xref: MASTER-074; re-measure before scheduling.
~~~~

### D2
- rows: 8
- subject: 2026-09-18-dispatcher-test-gaps, register entry for `Dispatcher`'s `GetRequiredService` barrier
- premise: Re-measured at 4d874be: MASTER-173 (`docs/audit/00-master-index.md:361`) already records the `GetRequiredService` barrier as "One residual carried to the backlog", and `DispatcherMissingHandlerTests` asserts the handler-naming messages a `GetService` swap turns red.
- answer: Retire: already noted (Recommended)

#### row 8
- disposition: remove
- lessons-line: -
- new-text: -

### D3
- rows: 20
- subject: 2026-08-28-the-control-kit, `DmFlagBanner` title editing unbuilt or abandoned
- premise: Re-measured at 4d874be: every `IsTitleEditable`, `TitleEditor`, `ApplyEditability`, `OnTitleCommitted` hit is inside `Controls/DmFlagBanner/`; no consumer sets `IsTitleEditable`; `git log -S IsTitleEditable` reaches `a283f70` "make the top banner a display-only title".
- answer: Remove the dead editor (Recommended)

#### row 20
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-09-01 2026-08-28-the-control-kit Remove `DmFlagBanner`'s abandoned title editor (`IsTitleEditable`, `TitleEditor`, `ApplyEditability`, `OnTitleCommitted`, the `TextCommitted` event and the `TitleEditor.Text` write), unset since `a283f70` made the banner display-only (Gate 2 D3 of 20261118-0114-backlog-triage); check: `git grep -n -e IsTitleEditable -e ApplyEditability -e OnTitleCommitted -- src` prints nothing and the solution builds with 0 warnings. -- source: .dir-sdlc/backlog.md:231; re-measure before scheduling.
~~~~

### D4
- rows: 22
- subject: 2026-08-28-the-control-kit, per-frame `StrokeDashPattern` allocations in `FloorMapDrawable`
- premise: Re-measured at 4d874be: `git grep -n StrokeDashPattern -- src` prints `FloorMapDrawable.cs:813` `[4f, 3f]` (ghost placeholder branch) and `:867` `[4f, 4f]` (phantom segment only); MASTER-097 is done.
- answer: Retire: negligible (Recommended)

#### row 22
- disposition: remove
- lessons-line: -
- new-text: -

### D5
- rows: 23
- subject: 2026-08-28-the-control-kit, accessibility state for `DmCard`'s opacity-hidden trailing region
- premise: Re-measured at 4d874be: `DmCard.xaml.cs` hides the trailing region by Opacity and InputTransparent with no AutomationProperties; each DungeonListPage card carries 3 trailing buttons (`EditDungeonButton`, `StartSessionButton`, `DeleteDungeonButton`, `grep -n AutomationId` on `DungeonListPage.xaml`), not six; MASTER-162 chose opacity so the buttons stay reachable.
- answer: Accept as intended (Recommended)

#### row 23
- disposition: remove
- lessons-line: -
- new-text: -

### D6
- rows: 36
- subject: 2026-09-02-app-sub-suite-rebuild, the six `ThemeSource` members with no external consumer
- premise: Re-measured at 4d874be with `git grep -n -w <member> -- src`: `ThemeFileText` 0 callers; `ThemeDirectory` and `StaticResourceKeys` used only inside `ThemeSource.cs`; `KeyDefinitionPattern` and `LegacyTolerantKeyDefinitionPattern` referenced only by one `ThemeSourceTests` pin test; `ThemeTokenNotFoundException` thrown and documented by the lookups.
- answer: One contract-change pass (Recommended)

#### row 36
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-09-08 2026-09-02-app-sub-suite-rebuild As one contract change to `ThemeSource` (Gate 2 D6 of 20261118-0114-backlog-triage): delete `ThemeFileText` (0 callers at `4d874be`), make `ThemeDirectory` and `StaticResourceKeys` private (used only inside `ThemeSource.cs`), delete `KeyDefinitionPattern` and `LegacyTolerantKeyDefinitionPattern` with their `ThemeSourceTests` pin test (no scan uses them), and keep `ThemeTokenNotFoundException` with a remark that it is the documented throw of the token lookups; check: `git grep -n -w -e ThemeFileText -e KeyDefinitionPattern -e LegacyTolerantKeyDefinitionPattern -- src` prints nothing and `DungeonMaster.Tests` passes. -- source: .dir-sdlc/backlog.md:80; xref: .dir-sdlc/backlog.md:92; re-measure before scheduling.
~~~~

### D7
- rows: 40
- subject: 2026-09-02-app-sub-suite-rebuild, the unfailable guard in `FloorMapDrawableDisposalTests`
- premise: Re-measured at 4d874be: `FloorMapDrawableDisposalTests.cs:238` `Assert.NotEmpty(refilled);` cannot fail while `:229` and `:230` hold; it guards the following `Assert.All` against passing on an empty list.
- answer: Keep it, add a comment (Recommended)

#### row 40
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-09-08 2026-09-02-app-sub-suite-rebuild Keep the `Assert.NotEmpty(refilled)` guard in `FloorMapDrawableDisposalTests.PreloadObjectImagesAsync_AfterClearObjectImageCache_RefillsTheCache` (`FloorMapDrawableDisposalTests.cs:238` at `4d874be`) and add one comment above it saying it stops the following `Assert.All` passing vacuously if the count assertion is weakened (Gate 2 D7 of 20261118-0114-backlog-triage); check: the comment sits directly above the guard and `DungeonMaster.App.Tests` passes. -- source: .dir-sdlc/backlog.md:88; re-measure before scheduling.
~~~~

### D8
- rows: 41
- subject: 2026-09-02-app-sub-suite-rebuild, `ThemeHygieneTests`' Setter-form hole
- premise: Re-measured at 4d874be: `ThemeHygieneTests` matches `FontSize` attributes only; a Python census over `git ls-files '*.xaml'` found 9 FontSize/FontFamily Setters, all under `src/DungeonMaster.App/Resources/Theme`, so 0 would surface outside it.
- answer: Close the hole (Recommended)

#### row 41
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-09-08 2026-09-02-app-sub-suite-rebuild Widen `ThemeHygieneTests` so a `<Setter>` whose `Property` is `FontSize` or `FontFamily` with a bare `Value` outside `Resources/Theme` is reported as an offender, with a synthetic positive control (Gate 2 D8 of 20261118-0114-backlog-triage); a census at `4d874be` found all 9 such Setters inside `Resources/Theme`, so no current file should fail; check: the control is reported by the widened probe and `DungeonMaster.Tests` passes. -- source: .dir-sdlc/backlog.md:90; re-measure before scheduling.
~~~~

### D9
- rows: 50
- subject: 2026-09-14-pluralise-entity-namespaces, singular test-project mirror folders
- premise: Re-measured at 4d874be: Domain uses `Entities/{Areas,Dungeons,Floors}`; the test tree uses `Entities/{Area,Dungeon,Floor}` with 9 `.cs` files (`find ... -name *.cs | wc -l`) and no outside references to those namespaces.
- answer: Rename to plural (Recommended)

#### row 50
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-09-16 2026-09-14-pluralise-entity-namespaces Rename `src/DungeonMaster.Tests/Domain/Entities/{Area,Dungeon,Floor}/` and their namespaces to the plural `Areas`, `Dungeons` and `Floors` that `src/DungeonMaster.Domain/Entities/` uses (9 files at `4d874be`), dropping any `using Domain<Type> =` alias the singular segment forced (Gate 2 D9 of 20261118-0114-backlog-triage); check: `git ls-files src/DungeonMaster.Tests/Domain/Entities` lists only the plural folders and `DungeonMaster.Tests` passes. -- source: .dir-sdlc/backlog.md:294; re-measure before scheduling.
~~~~

### D10
- rows: 58
- subject: 2026-09-30-canvaspoint-finiteness-guard, `NonFiniteScaleRejectionTests` doc comment naming an internal FluentValidation type
- premise: Re-measured at 4d874be: `NonFiniteScaleRejectionTests.cs:20` names `ComparableComparer<float>.Instance`; `Directory.Packages.props:21` pins FluentValidation 12.1.1; a reflection probe on that DLL printed `public=False`.
- answer: Version-stamp the remark (Recommended)

#### row 58
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-09-30 2026-09-30-canvaspoint-finiteness-guard Reword the `NonFiniteScaleRejectionTests.cs` remark naming FluentValidation's `internal` `ComparableComparer<float>.Instance` (`:20` at `4d874be`) to say it was observed in FluentValidation 12.1.1, the version `Directory.Packages.props` pins (Gate 2 D10 of 20261118-0114-backlog-triage); check: `grep -n 12.1.1 src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/NonFiniteScaleRejectionTests.cs` prints the remark. -- source: .dir-sdlc/backlog.md:449; re-measure before scheduling.
~~~~

### D11
- rows: 65
- subject: 2026-08-17-pin-the-build, `.gitattributes` `eol` directive versus dropping `end_of_line` from `.editorconfig`
- premise: Re-measured at 4d874be: `.editorconfig:8` `end_of_line = crlf`; `.gitattributes` is `* text=auto` with no `eol`; `git ls-files --eol` shows every text file `i/lf` and `grep -c "i/lf.*w/lf"` prints 0.
- answer: Drop end_of_line (Recommended)

#### row 65
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-08-17 2026-08-17-pin-the-build Drop `end_of_line = crlf` from `.editorconfig` (`:8` at `4d874be`), since under `* text=auto` the index is LF for every text file and the setting only turns an LF working-tree write into an `IDE0055` build failure that `git status` cannot see (Gate 2 D11 of 20261118-0114-backlog-triage); check: `grep -n end_of_line .editorconfig` prints nothing and a solution build with one LF-converted `.cs` file passes. -- source: .dir-sdlc/backlog.md:70; xref: MASTER-132, MASTER-150; re-measure before scheduling.
~~~~

### D12
- rows: 69
- subject: 2026-09-18-drawsessionresult-none-rejection, static guard for `PlaceVertex` without `EnsurePlacementOutcome`
- premise: Re-measured at 4d874be: one production `PlaceVertex` consumer, `DungeonDetailPage.xaml.cs:1629`, already chained to `.EnsurePlacementOutcome()`; no analyser project exists; `ConfigureAwaitDisciplineTests` is a source-scan precedent. The paired row on protecting that call asks for the same check and is closed by this rewrite.
- answer: Source-scan test (Recommended)

#### row 69
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-09-18 2026-09-18-drawsessionresult-none-rejection Add a source-scan test, modelled on `ConfigureAwaitDisciplineTests` and scoped to `src/DungeonMaster.App`, that fails when a `PlaceVertex(` call is not followed by `.EnsurePlacementOutcome()` (one consumer at `4d874be`, `DungeonDetailPage.xaml.cs:1629`, already guarded), with a must-fail fixture and no analyser project (Gate 2 D12 of 20261118-0114-backlog-triage); check: deleting `.EnsurePlacementOutcome()` at the consumer turns the test red and restoring it turns it green. -- source: .dir-sdlc/backlog.md:413; xref: MASTER-179, .dir-sdlc/backlog.md:411; re-measure before scheduling.
~~~~

#### row 55
- disposition: remove
- lessons-line: -
- new-text: -

### D13
- rows: 101
- subject: 2026-09-11-make-the-floor-sidebar-operable, mechanical check binding each claim to its command
- premise: Re-measured at 4d874be: no test or script reads `docs/audit/`; two citation predicates over `docs/audit/*.md` count 1619 and 3465; `.sir/lessons.md:55`, `:56` and `:101` state the citation rule.
- answer: Retire, cite lessons (Recommended)

#### row 101
- disposition: lesson
- lessons-line: 55
- new-text: -

### D14
- rows: 134
- subject: 2026-09-17-enum-zero-values, lesson: strip doc comments before splitting enum bodies
- premise: Re-measured at 4d874be: `.sir/lessons.md:42` states the must-match control rule; the comment-splitting detail served one closed census (MASTER-178).
- answer: Remove, cite lessons:42 (Recommended)

#### row 134
- disposition: lesson
- lessons-line: 42
- new-text: -

### D15
- rows: 135
- subject: 2026-09-17-enum-zero-values, lesson: agreeing measurements sharing one method share its blind spot
- premise: Re-measured at 4d874be: `.sir/lessons.md:42` ("count the population again with a second predicate that cannot share the first's mistake") and `:31` (FORM 16) state the rule.
- answer: Remove, cite lessons:42 (Recommended)

#### row 135
- disposition: lesson
- lessons-line: 42
- new-text: -

### D16
- rows: 136
- subject: 2026-09-17-enum-zero-values, lesson: a disagreeing reviewer checks its own predicate first
- premise: Re-measured at 4d874be: a Python search of `.sir/lessons.md` finds no line telling a reviewer to suspect its own predicate first; the source (`.dir-sdlc/backlog.md:393`) records the `private enum NotebookPane` miss, 17 declarations against 18.
- answer: Add a [framework] line (Recommended)

#### row 136
- disposition: lesson
- lessons-line: -
- new-text:
~~~~
2026-09-17 2026-09-17-enum-zero-values [framework] When a reviewer's or orchestrator's count disagrees with an artifact, check its own predicate before reporting a discrepancy, then validate the corrected count by its complement: a regex that missed `private enum NotebookPane` gave 17 declarations against the entry's 18. (source: .sir/backlog.md row 2026-09-17-enum-zero-values reviewer predicate check, routed by 20261118-0114-backlog-triage)
~~~~

### D17
- rows: 147, 151, 157
- subject: 20261103-1947-core-doc-comments (MASTER-180 cell, CORE-036 block), 20261105-0106-core-two-row-sweep (MASTER-110 cell) and 20261111-0156-core-audit-corrections (CORE-015, CORE-019, CORE-035 blocks): audit citations of `.sir/sessions/` paths
- premise: Re-measured at 4d874be: Python regex `\.sir/sessions/\d{8}-\d{4}-[a-z0-9-]+/` over `docs/audit/*.md` counts 50 citations on 45 lines across 9 sessions (control count 50); `git ls-files .sir/sessions | wc -l` prints 337, so the cited directories are tracked.
- answer: Keep, lesson, drop 3 rows (Recommended)

#### row 147
- disposition: lesson
- lessons-line: -
- new-text:
~~~~
2026-11-03 20261103-1947-core-doc-comments [project] `.sir/sessions/<id>/` directories are tracked in git (337 paths at `4d874be`), so a `docs/audit/` citation into one resolves for as long as the directory is kept: keep session directories, and cite a session file by path rather than inlining it. (source: .sir/backlog.md row 20261103-1947-core-doc-comments audit citations of session paths, routed by 20261118-0114-backlog-triage)
~~~~

#### row 151
- disposition: remove
- lessons-line: -
- new-text: -

#### row 157
- disposition: remove
- lessons-line: -
- new-text: -

### D18
- rows: 150
- subject: 20261104-1726-test-hygiene-sweep, an assertion under `// Act` and the AAA rule
- premise: Re-measured at 4d874be: assertions under `// Act` sit at `Infrastructure/Persistence/Repositories/AreaPersistenceTests.cs:38`, `Infrastructure/Async/ConfigureAwaitDisciplineTests.cs:51-53` (both files moved from the row's paths) and `Infrastructure/Persistence/Configurations/CanvasPointGuardMaterializationTests.cs:60-61`.
- answer: Breach: file a work row (Recommended)

#### row 150
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-11-04 20261104-1726-test-hygiene-sweep Extend the AAA rule, its PA predicate and the A1 precondition rule from Arrange to Act blocks, then move the assertions that sit under `// Act` at `4d874be` into the Assert section or a guard fact: `src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceTests.cs:38`, `src/DungeonMaster.Tests/Infrastructure/Async/ConfigureAwaitDisciplineTests.cs:51-53` and `src/DungeonMaster.Tests/Infrastructure/Persistence/Configurations/CanvasPointGuardMaterializationTests.cs:60-61` (Gate 2 D18 of 20261118-0114-backlog-triage); check: the extended predicate prints nothing over the test projects and prints a fixture holding an assertion under `// Act`. -- source: reviews/T4.md advisory 1, reviews/T8.md; re-measure before scheduling.
~~~~

### D19
- rows: 153
- subject: 20261109-1641-part-done-rows, `obj` locals outside `MASTER-130`'s population
- premise: Re-measured at 4d874be with `grep -rnw obj <dir> --include=*.cs --exclude-dir=obj --exclude-dir=bin | wc -l`: App 32 lines in 2 files; Tests 46 lines in 7 files, of which the `RepoPaths.cs` and `RepoPathsTests.cs` directory-name strings are not candidates (44 lines in 5 files); none is a lambda.
- answer: One row, App and Tests (Recommended)

#### row 153
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-11-09 20261109-1641-part-done-rows Rename the `obj` locals and parameters outside MASTER-130's recorded population to descriptive names: `src/DungeonMaster.App` (32 lines in `Graphics/FloorMapDrawable.cs` and `Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`) and `src/DungeonMaster.Tests` (44 lines in 5 files, leaving the `obj` directory-name strings in `App/Theme/RepoPaths.cs` and `App/Theme/RepoPathsTests.cs`), as counted at `4d874be` (Gate 2 D19 of 20261118-0114-backlog-triage); check: `grep -rnw obj src/DungeonMaster.App src/DungeonMaster.Tests --include=*.cs --exclude-dir=obj --exclude-dir=bin` prints only those two strings. -- source: MASTER-130 (Gate 2 Q2); re-measure before scheduling.
~~~~

### D20
- rows: 158
- subject: 20261111-1831-floor-drag-placeholder, whether `default(FloorDropLanding)` means no landing
- premise: Re-measured at 4d874be: `FloorDropLanding.cs:36` `HasLanding` compares `TargetIndex` to `NoBand = -1`, so a default value has a landing; `git grep -n 'default(FloorDropLanding)' -- src` prints nothing.
- answer: Add a doc remark (Recommended)

#### row 158
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-11-11 20261111-1831-floor-drag-placeholder Add a remark on `FloorDropLanding` saying a `default` value reports `HasLanding = true` and that `FloorDropLanding.None`, not `default`, means no landing (`src/DungeonMaster.App.Core/Editor/FloorDropLanding.cs:36` at `4d874be`; nothing constructs a default today) (Gate 2 D20 of 20261118-0114-backlog-triage); check: `grep -n -w default src/DungeonMaster.App.Core/Editor/FloorDropLanding.cs` prints the remark. -- source: reviews/T1.md A2; re-measure before scheduling.
~~~~

### D21
- rows: 161
- subject: 20261112-0153-floor-name-tooltip, display-scaled `MaximumToggleWidth` bound
- premise: Re-measured at 4d874be: `FloorSidebarAutomationTests.cs:118` `MaximumToggleWidth = 40` holds for an 18-DIU chevron only up to 40/18 scale; the remark at `:55` says a display change can never redden the tests. The 2026-09-14-make-the-floor-sidebar-operable size-ratchet row proposes the scale derivation that settles both.
- answer: Merge into scale row (Recommended)

#### row 161
- disposition: remove
- lessons-line: -
- new-text: -

#### row 47
- disposition: rewrite
- lessons-line: -
- new-text:
~~~~
2026-09-14 2026-09-11-make-the-floor-sidebar-operable Make the Appium size ratchets compare like units by deriving the display scale from a known fixed-size element (the notebook button's declared DIU size against its measured pixels), since asserting DIU thresholds against physical-pixel measurements lets a regression to 20 DIU pass at 1.25 scale; apply the same derivation to the fixed `MaximumToggleWidth = 40` pixel bound in `src/DungeonMaster.App.UiTests/FloorSidebarAutomationTests.cs` (`:118` at `4d874be`), which a correct 18-DIU chevron exceeds above 40/18 scale, and correct that class's remark that a display change can never redden its tests (`:55`), folded in from the 20261112-0153-floor-name-tooltip toggle-width row (Gate 2 D21 of 20261118-0114-backlog-triage); check: the suite logs the derived scale and every size assertion compares DIU with DIU. -- source: .dir-sdlc/backlog.md:265; xref: .dir-sdlc/backlog.md:320; re-measure before scheduling.
~~~~
