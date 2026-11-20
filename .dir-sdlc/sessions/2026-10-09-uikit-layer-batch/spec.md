# Session 6b-xviii — UIKIT layer batch

Close or correct the nine open `UIKIT` register rows selected at Gate 1, reconcile the
`MASTER-122`/`MASTER-181` duplicate, and leave the build and suite no worse than the measured
baseline.

## Context

The repo audit register holds 181 rows — 141 closed, 35 open, 5 part done, measured this session at
`57b7eba`. `UIKIT` is the largest open layer at 9 rows and was untouched until now. Session 6b-xvii
closed nine `CORE` rows against a recent cadence of about 1.4 per session, which is the evidence that
**layer concentration beats severity** as a selection criterion; this session tests that again on
`UIKIT`.

Gate 1 selected eight of the nine, excluding `MASTER-158` (`L`, `risk=high`, "no bindable surface at
all") as two consecutive briefs advised.

**Re-measurement changed the work.** Of the eight rows, **five carry a false or stale specific** and
**two are falsified outright** — `MASTER-125`'s stated remedy ("Delete the line") would break the
build and six tests. The register's specifics are leads here, not evidence. `measurement.md` in this
directory is the evidence base, with every figure's command beside it.

The baseline is this session's own measurement, not an inheritance: `dotnet build
src/DungeonMaster.sln` → `Build succeeded.`, 0 Warning(s), 0 Error(s) across three TFMs;
`dotnet test src/DungeonMaster.sln` → **1028 passed**, 0 failed, 0 skipped.

## Definition of done

**Code changes**

- **D1** `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs` has 0 braceless control-flow sites, down from 8, measured by a detector shown non-blind against a nested-parenthesis header. (`MASTER-121`)
- **D2** `src/DungeonMaster.App/Behaviors/HandCursorBehavior.cs` has 0 braceless control-flow sites, down from 2 — `:67` inside `#if MACCATALYST` and `:154` inside `#if WINDOWS`, both compiled here. (`MASTER-121`)
- **D3** `grep -cE '^[[:space:]]*/// {2,}'` returns 0 on each of the 5 in-scope files, clearing all **124** space-indented doc continuation lines, each replaced by `///` plus exactly two tabs. (`MASTER-122` ⊕ `MASTER-181`)
- **D4** `public enum BehaviorCursorShape` is declared in `src/DungeonMaster.App/Behaviors/BehaviorCursorShape.cs` and nowhere else; `HandCursorBehavior.cs` declares only `HandCursorBehavior`. (`MASTER-123`)
- **D5** `NotebookKind`, `NotebookTarget` and `NotebookSaveArgs` each have their own file under `Controls/NotebookView/`, each named for its type; `NotebookView.xaml.cs` declares only `NotebookView`. (`MASTER-123`)
- **D6** `HandCursorBehavior.Shape` carries an XML doc comment that states what it selects and its default. (`MASTER-136`)
- **D7** `FloorSidebarItem.SetAreaEditing` carries `<param name="areaId">` and `<param name="editing">`, its existing `<summary>` unchanged. (`MASTER-136`)
- **D8** `BookmarkTabShape` computes the notch as `Math.Min(NotchDepth, bounds.Height * 0.5)`, and its comment states the half-height bound. (`MASTER-148`)
- **D9** A named test fact fails against the old clamp and passes against the new one, demonstrated by injection **with a positive control that passes in the same run**. (`MASTER-148`)
- **D10** A named test fact in `src/DungeonMaster.Tests/App/Theme/` asserts `DmConfirmButton`'s `PointerOver` and `Pressed` opacity equal `DmButtonBase`'s by comparing **parsed values**, with a positive-control fact proving the scan reaches both styles. `Disabled` and `Normal` are excluded and the file says why. (`MASTER-151`)
- **D11** No structural change to either theme dictionary; both are left byte-identical, proven by `sha256sum`. (`MASTER-151`)

**Register changes**

- **D12** `MASTER-121` closed, with its population corrected to 10 sites / 2 files, its eight stale citations recorded, the two already-clean files recorded, and the uncited `:154` recorded.
- **D13** `MASTER-122` closed, with its false file-indentation claim struck (0 leading-space against 706 leading-tab, of 882) and its population widened to the measured 124 lines / 5 files.
- **D14** `MASTER-181` closed as a duplicate of `MASTER-122`, in `00-master-index.md` **and** in its `CORE-026` entry in `01-domain-application.md`, recording that its layer `UI, INFRA, TEST` is wrong in its first element.
- **D15** `MASTER-123` closed, recording that the brief's `git mv`/`R100` mechanism does not apply because these are extractions, not renames.
- **D16** `MASTER-124` closed **FALSIFIED**, recording that both instances and the defect class are gone and that the row misread §5.
- **D17** `MASTER-125` closed **FALSIFIED**, recording all four false clauses and that its stated remedy would break the build and six tests.
- **D18** `MASTER-136` closed, effort corrected `S`→`XS`, with all six dead locations accounted for including `AreaSidebarItem.SetEditing`, which no longer exists.
- **D19** `MASTER-148` closed, recording the chosen clamp, why `bounds.Height - 1` was rejected, and that its citation was the only one of the eight that survived unchanged.
- **D20** `MASTER-151` closed as deliberately unchanged with a parity guard added, recording that its named owner "T5" is a stale task id.
- **D21** Every `AMENDED 6b-xviii` block follows the established six-element form, deletes no previous wording, and carries a positive control beside any count resting on an absence.

**Session-wide**

- **D22** Every touched file remains pure CRLF, proven three ways per file; `Write` and `sed -i` were used on no pre-existing file.
- **D23** `dotnet build src/DungeonMaster.sln` reports `Build succeeded.`, 0 Warning(s), 0 Error(s), with all three `DungeonMaster.App` TFMs in the output.
- **D24** `dotnet test src/DungeonMaster.sln` reports 0 failed and 0 skipped, and the passed total reconciles to 1028 plus the new facts added.
- **D25** Every count written into the register names the command that produced it on the post-change tree, or is labelled with whose measurement it is.

## Scope

Eight register rows, all layer `UIKIT`: `MASTER-121`, `122`, `123`, `124`, `125`, `136`, `148`, `151`
— plus `MASTER-181`, closed as a duplicate under the Gate 1 merge decision. Nine rows total.

Nine source files are edited, three created, one or two test files created, and three register files
amended.

### Not in scope

- **`MASTER-158`** (`UIKIT-027`, "`DmConfirmPopup` and `NotebookView` expose no bindable surface at all") — `L`, `risk=high`. Excluded at Gate 1. `UIKIT` therefore ends this session with 1 open row, not 0.
- **The EF migration's 35 doc lines.** `src/DungeonMaster.Infrastructure/Migrations/20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs` carries 35 lines of the same space form and is excluded — see Assumption 2. `124 remedied + 35 excluded = 159`, the full measured population.
- **Three layer-wide measurements this session paid for.** 37 braceless-or-same-line sites across all 58 `DungeonMaster.App` `.cs` files, 21 of them in `Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` (UI layer); 14 type-vs-filename mismatches, 10 outside `UIKIT` and 8 in `ViewModels/`; and a `<param>`-gap upper bound of 14. **No row and no task records these** — see Open question 2.
- **No `.editorconfig`.** `UIKIT-022`'s suggested direction floats one; it is repository-wide and belongs to whoever owns repository configuration.
- **Any structural change to the theme dictionaries.** `MASTER-151`'s duplication is deliberate and correct; the remedy is a test, not a refactor.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Is the branch ahead of origin, as the brief and handoff both state? | **No — 0 ahead / 0 behind.** Both commits were pushed. | 1 | `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit` → `0 0`; `git reflog show origin/chore/repo-audit` → `57b7eba … update by push` |
| Are the EF migration's 35 doc lines in `MASTER-122`'s remedy? | **No, excluded.** The repository leaves scaffolder output in the generator's style. | 1 | All **7** files under `Migrations/` are space-indented with **0** leading tabs. `MASTER-181`'s own row text already flags it: "the migration's 35 lines also need a judgment call this row does not make, since migrations are generated artefacts and may be exempt." Reinforced by `disciplines/database.md:15`: "A released migration is never edited." |
| Which clamp for `MASTER-148` — `bounds.Height * 0.5` or `bounds.Height - 1`? | **`* 0.5`.** | 4 | `- 1` yields a **negative** notch for `Height < 1`, so `LineTo(center.X, bottom - notch)` puts the apex below the bottom edge and inverts the notch into a spike outside bounds — worse than the degeneracy. `1` is also an absolute floor in device-independent units that does not scale. Architect read `BookmarkTabShape.cs:27-47`. |
| Where do the extracted `Notebook*` types go? | **`Controls/NotebookView/`**, per `UIKIT-023`'s own suggested direction. | 1 (mixed) | **Counter-evidence stated plainly:** every other standalone type in this project sits *flat* in `Controls/` (`BookmarkTabShape.cs`, `NotebookAffordance.cs`), and each per-control subfolder holds only that control's `.xaml`/`.xaml.cs`. Namespace is `DungeonMaster.App.Controls` either way, so the choice is organizational and free to reverse. |
| Is the doc conversion one width or two? | **One — exactly 5 spaces → 2 tabs**, no nested width in scope. | 1 | `grep -oE '/// {2,}' \| awk '{print length($0)-3}' \| sort -n \| uniq -c` over the 5 in-scope files returns `124 5` and nothing else. **All 21** of the 9-space nested lines are in the excluded migration. |
| Is the target form two tabs or three for a `<para>` body? | **Two, uniformly.** | 1 | `NotebookAffordance.cs` under `sed -n '9,24p' \| cat -A` renders every continuation line `///^I^I`, `<para>` bodies included. |
| Where does the `BookmarkTabShape` test live? | **`App.UnitTests`**, with `App.Tests` as a declared fallback. | 1 | `App.UnitTests` is plain `net10.0`, no `UseMaui`, references `DungeonMaster.App`; its csproj comment records that it exists so `DungeonMaster.Tests` need not reference App. `BookmarkTabShape` is a plain `IShape` with no `Element` involvement. |
| Does `MASTER-151`'s remedy have a home? | **Yes — `src/DungeonMaster.Tests/App/Theme/`.** | 1 | 12 files there read the theme XAML off disk, `ThemeSource.cs` the shared reader. The row's named owner "T5" is a stale task id from an earlier session. |
| Does `DungeonMaster.Tests` compile `DungeonMaster.App`? | **No.** It references Domain, Application, Infrastructure, App.Core and reads App's XAML off disk as data. | 1 | `grep -nE 'ProjectReference' src/DungeonMaster.Tests/DungeonMaster.Tests.csproj`. This is what makes T8's `dependsOn: [T5, T6]` correct rather than under-specified. |
| Does `#if MACCATALYST` compile on this machine? | **Yes.** | 1 | `DungeonMaster.App.csproj:9` declares three TFMs; the baseline build emitted a **530944-byte** `net10.0-maccatalyst` assembly. **This corrects a claim the orchestrator made from the `#if` guard alone without building, and retracted** — see `measurement.md` Correction 5. |
| Where should the first build signal sit, given T1–T6 carry no build criterion? | **The orchestrator runs `dotnet build src/DungeonMaster.sln` itself between waves 1 and 2.** | 4 | Six concurrent builds in one worktree collide on `obj/`/`bin/`, which is why T1–T6 verify by file-content measurement only. One serial build keeps the six-way parallelism and catches a broken brace or a mangled extraction before T7/T8 spend a dispatch. The orchestrator already owns and has run that command (54.38s). Raised by the architect as an escalation; resolved here at rung 4 rather than put to the user, because it is the orchestrator's own operation. |

## Contracts

**None.** `contracts/` is empty and that is the architect's considered conclusion, not an omission.
No task consumes an operation a peer implements: T2 and T3 relocate types inside one unchanged
namespace with zero consumer changes, T7 and T8 test code already present at the base, and T9 reads
prose reports. The only cross-task agreement is the 5-spaces→2-tabs conversion rule shared by T1,
T4, T5 and T6 — a convention with no declaration to freeze, encoded as an identical acceptance
criterion in all four.

## Open questions

**All four resolved at Gate 2. None remains open.**

| # | Question | Decision |
|---|---|---|
| 1 | After this session no open row covers the EF migration's 35 doc lines | **Standing exemption.** Generated EF scaffolder output is exempt from §9's two-tab rule as a convention of this repository, recorded on `MASTER-122`'s entry with its evidence. No row stays open because nothing is owed. This answers the question `MASTER-181`'s own row text raised and declined to settle. |
| 2 | Three layer-wide measurements have no home | **All three go to `.dir-sdlc/backlog.md`** at Gate 3, each with its figure, its command and its known limits — including that the `<param>` count of 14 is an upper bound with at least two identified false positives. |
| 3 | T1's injection has no revert proof | **Criterion added to T1** before the freeze, identical in form to T7's and T8's: `sha256sum` after the fix, after the injection (must differ), after the restore (must match). T1 now carries 9 criteria. |
| 4 | T7 and T8 share a wave and both build | **T8 serialized behind T7.** `T8.dependsOn` is now `["T5", "T6", "T7"]`, giving four strictly sequential waves. Costs one wave of latency on two small tasks; removes a race that would have surfaced as a non-reproducible failure attributed to the wrong task. |

## Waves, as frozen

| Wave | Tasks | Gate |
|---|---|---|
| 1 | T1, T2, T3, T4, T5, T6 — six-way parallel, disjoint `owns`, no build criterion | Orchestrator runs `dotnet build src/DungeonMaster.sln` before wave 2 |
| 2 | T7 | |
| 3 | T8 | |
| 4 | T9 | |
