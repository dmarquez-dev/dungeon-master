# Decided test rows

Clear the seven `.sir/backlog.md` rows whose remedy a 20261118-0114-backlog-triage gate already chose, all inside `src/DungeonMaster.Tests` and `src/DungeonMaster.App.Tests`.

## Context

The 20261118-0114-backlog-triage session re-measured every open row and settled remedies at its Gate 2 (D1-D21) and Gate 3. This session takes the seven of those rows whose change stays inside the two test projects and needs no Appium server. The rows are named here by their originating session and opening words (line numbers move when Gate 3 edits the file):

- 2026-09-17-core-type-identity, "Drop the two `await Task.Delay(20)`" (D1).
- 2026-09-02-app-sub-suite-rebuild, "As one contract change to `ThemeSource`" (D6).
- 2026-09-02-app-sub-suite-rebuild, "Keep the `Assert.NotEmpty(refilled)` guard" (D7).
- 2026-09-02-app-sub-suite-rebuild, "Widen `ThemeHygieneTests`" (D8).
- 2026-08-28-the-control-kit, "Record in the remark on `FloorMapDrawableAllocationTests`" (the triage's Gate 3).
- 2026-09-14-pluralise-entity-namespaces, "Rename `src/DungeonMaster.Tests/Domain/Entities/{Area,Dungeon,Floor}/`" (D9).
- 2026-09-30-canvaspoint-finiteness-guard, "Reword the `NonFiniteScaleRejectionTests.cs` remark" (D10).

Every row's cited sites still hold at `f2aead0`. Both the architect and the orchestrator re-ran each check in the worktree.

## Definition of done

Each item maps to criteria in `manifest.json`. `coverage.md` holds the mapping.

- [ ] D1 `grep -rn --exclude-dir=bin --exclude-dir=obj Task.Delay <worktree>/src/DungeonMaster.Tests` prints nothing (2 lines at `f2aead0`).
- [ ] D2 The two LastModified tests backdate `LastModified` through reflection on `Entity`'s private setter to a fixed literal instant, as `ForceId` does for `Id`.
- [ ] D3 Each of those two tests fails when `Entity.Touch` stops updating `LastModified`. This is shown by an injected defect at `Entity.cs:33` with the tests held fixed, then reverted.
- [ ] D4 No production clock seam is added. `Entity.cs` is byte-identical to `f2aead0` at T1's end.
- [ ] D5 `src/DungeonMaster.Tests/Domain/Entities` holds only the `Areas/`, `Dungeons/` and `Floors/` folders, with the same 11 file names and no singular folder left.
- [ ] D6 The namespaces follow the plural folders.
- [ ] D7 Nothing in `src` or `docs` references `DungeonMaster.Tests.Domain.Entities.{Area,Dungeon,Floor}`.
- [ ] D8 The 11 `using Domain<Type> =` aliases that the singular segment forced are dropped, along with their 86 use lines.
- [ ] D9 The moved files' content is unchanged except for the namespace, using and alias lines and the D1 change.
- [ ] D10 `ThemeFileText`, `KeyDefinitionPattern` and `LegacyTolerantKeyDefinitionPattern` are gone, along with their doc comments and the crefs at `ThemeSource.cs:98` and `:648`.
- [ ] D11 `ThemeDirectory` and `StaticResourceKeys` are `private static`.
- [ ] D12 The pin test `TheLegacyTolerantKeyPatternStillMatchesThePrefixCasingTheStrictPatternRejects` is deleted, so DungeonMaster.Tests has exactly one fewer test.
- [ ] D13 `IsLegacyTolerantKey`'s doc comment still states the reason for the `DM` tolerance.
- [ ] D14 `ThemeTokenNotFoundException` is kept, with one `<remarks>` naming `DoubleToken`, `Int32Token` and `StringToken` as the lookups that throw it.
- [ ] D15 A `//` comment sits directly above `Assert.NotEmpty(refilled);`.
- [ ] D16 That comment says the guard stops the following `Assert.All` passing vacuously if the count assertion is weakened.
- [ ] D17 `ThemeHygieneTests` reports a `<Setter>` outside `Resources/Theme` whose `Property` is `FontSize` or `FontFamily` with a bare `Value`.
- [ ] D18 A synthetic positive control is reported through the same probe method the facts call.
- [ ] D19 A negative control (a `{StaticResource}` Value, and `IconFontSize`) is not reported.
- [ ] D20 The positive control is shown able to fail by an injection into the Setter branch, then reverted.
- [ ] D21 `StillOffends` recognises the Setter form, so the allow-list self-check stays consistent.
- [ ] D22 Doc comments describing the probe as attribute-only are corrected.
- [ ] D23 No current XAML fails the widened facts. At `f2aead0` all 9 FontSize and FontFamily Setters are inside `Resources/Theme`.
- [ ] D24 `grep -n unconditional` on `FloorMapDrawableAllocationTests.cs` prints only lines inside the remarks block of `MovedAreas_AllocateNoMorePerFrameThanUnmovedOnes`.
- [ ] D25 The `NonFiniteScaleRejectionTests` remark says the `ComparableComparer` behaviour was observed in FluentValidation 12.1.1, the version `Directory.Packages.props` pins.
- [ ] D26 Every touched file keeps CRLF-only line endings, has 0 lone CR and keeps its no-BOM state, by Python byte count.
- [ ] D27 Each task's `dotnet build src/DungeonMaster.sln` reports 0 Warning(s) and 0 Error(s), and its `dotnet test --no-build` reports 0 failed, with the stated test-count delta.
- [ ] D28 T6 runs the final full-solution build and test at the tip: App.UnitTests 217, App.Tests 146, and DungeonMaster.Tests 1007 plus the cases T4 adds.
- [ ] O1 The seven rows are removed from `.sir/backlog.md` at Gate 3 (orchestrator).
- [ ] O2 No production file changes: `git diff --name-only f2aead0 <tip> -- src/DungeonMaster.Domain/ src/DungeonMaster.Application/ src/DungeonMaster.Infrastructure/ src/DungeonMaster.App/ src/DungeonMaster.App.Core/` prints nothing (orchestrator).
- [ ] O3 `git diff -M --name-status f2aead0 <T1 commit> -- src/DungeonMaster.Tests/Domain/Entities` shows 9 `R` lines and `M` for `EntityEqualityTests.cs` (orchestrator).
- [ ] O4 `git ls-files src/DungeonMaster.Tests/Domain/Entities` at the tip lists only the plural folders and the 2 root files (orchestrator).
- [ ] O5 At the tip, the rows' literal checks `git grep -n Task.Delay -- 'src/DungeonMaster.Tests*'` and `git grep -n -w -e ThemeFileText -e KeyDefinitionPattern -e LegacyTolerantKeyDefinitionPattern -- src` both print nothing (orchestrator).
- [ ] O6 At Gate 3, two rows are appended to `.sir/backlog.md`, per Gate 2 D2 (orchestrator). One extends `ThemeHygieneTests` to the `<Setter.Value>` element form. The other re-anchors the 18 `docs/audit/` citations of the singular `Domain/Entities/{Area,Dungeon,Floor}/` test paths to the plural ones (17 in `05-tests.md`, 1 in `00-master-index.md`, by `grep -rnoE 'Domain/Entities/(Area|Dungeon|Floor)/[A-Za-z]+Tests\.cs' docs` at `f2aead0`). Each row is written as the action it holds.

## Scope

Six tasks in a linear chain, T1 → T6, all `sir-test-engineer`. T1 merges the Task.Delay row and the rename row because they share two files.

### Not in scope

- The ten other triage-decided rows (App code, Appium, build config, the AAA rule, renames in App, XAML re-indent). They stay open in `.sir/backlog.md`.
- Re-anchoring the 18 `docs/audit/` citations of the singular test paths (17 in `05-tests.md`, 1 in `00-master-index.md`). This is deferred work: Gate 2 D2 carries it to `.sir/backlog.md` at Gate 3 (O6).
- Detecting the `<Setter.Value>` element form in `ThemeHygieneTests`. The row asks only for a bare `Value` attribute. This is deferred work: Gate 2 D2 carries it to `.sir/backlog.md` at Gate 3 (O6).
- The `CollectionWrapperCachingTests.cs:6` alias. The singular segment does not force it (it compiles without the alias at `f2aead0`), and it lies outside T1's ownership.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| How are the Task.Delay row and the rename row split, given they share files? | One task, T1, with the Task.Delay half done first. Ownership must be disjoint, and both rows edit `Floor/FloorMethodsTests.cs` and `EntityEqualityTests.cs`. | 1 | `git grep -n Task.Delay f2aead0`; `using Domain<Type> =` lines in both files |
| What injection shows "fails when LastModified is not updated"? | At `Entity.cs:33`, replace `LastModified = DateTime.UtcNow;` with `_ = DateTime.UtcNow;`. The self-assignment does not compile (CA2245 as an error). | 1 | architect scratch build at `f2aead0`: 0/0 with this injection, 2 of 2 failing |
| Where does the backdate helper live? | A private static helper in each file that uses it, following `ForceId`. | 1 | `EntityEqualityTests.cs:192-195` at `f2aead0` |
| Which value is backdated to? | A fixed literal instant, never `Now` or `UtcNow`. | 1 | session discipline `disciplines/test.md`, Isolation |
| Which aliases did the singular segment force? | All 11 lines in 7 files. Dropping them without the rename gives CS0118, and with the rename plain usings build 0/0. | 1 | architect scratch compile at `f2aead0` |
| How is a rename done without git? | A Python byte copy to the new path, then deletion of the source. The orchestrator supplies a per-file content diff to the reviewer, and checks rename detection after the commit (O3). | 4 | implementers have no git (sir-core) |
| Where does the ThemeTokenNotFoundException remark go? | On the class's own doc comment, naming the three token lookups. | 4 | `ThemeSource.cs:327`, `:358`, `:382` `<exception cref>` lines at `f2aead0` |
| Does `StillOffends` widen with the probe? | Yes. Otherwise an allow-listed file whose only offence is a Setter would be reported stale. | 1 | `ThemeHygieneTests.cs:259-275`, `:203-207` at `f2aead0` |
| Is the `<Setter.Value>` element form in scope? | No. Only the attribute form is covered, because the row says "bare `Value`". | 1 | the row text |
| Does dotnet-format §9 apply to existing wrapped paragraphs? | Only to sentences a task writes or rewrites. Existing paragraphs are not reflowed. | 1 | `dm-conventions/backend/dotnet-format.md` §9; existing remarks wrap mid-sentence |
| Does the allocation remark restate "all 30 tests green"? | No, unless it is measured and scoped so the comment cannot change the result. | 1 | `.sir/lessons.md` FORM 17 |
| Can two tasks build at once? | No. Every build writes the shared `obj/` and `bin/` trees of the referenced projects, so the tasks form a linear chain. | 1 | `DungeonMaster.Tests.csproj:21-24`, `App.Tests.csproj:38`, `App.csproj:135-137` |
| Which tasks inject defects? | T1 (production `Entity.cs`, reverted and hash-checked) and T4 (its own file). The chain means no peer builds while either injection is live. | 1 | manifest `dependsOn` |
| What are the test-count deltas? | T2 removes 1 (one `[Fact]`, no data rows). T4 adds k, reported from its diff. Every other task changes 0. | 1 | `ThemeSourceTests.cs:446-447` at `f2aead0` |
| Do the audit citations of singular paths change in this session? | No. They are outside the test projects. Gate 2 D2 carries the re-anchoring to the backlog (O6). | 1 | `grep -rnoE 'Domain/Entities/(Area\|Dungeon\|Floor)/[A-Za-z]+Tests\.cs' docs \| wc -l` = 18 |

## Contracts

None. No task consumes another task's interface.

## Open questions

None. The Gate 2 decisions are in the gate.
