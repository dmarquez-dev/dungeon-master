# App comment fixes

Close five deferred comment and doc-comment rows in `DungeonMaster.App` and `DungeonMaster.App.Tests`: `.sir/backlog.md:44`, `:47`, `:48`, `:176` and `:177`.

## Context

All five scheduled backlog headings are empty, so this session takes five unscheduled rows from one layer pair. The orchestrator re-measured each row in the worktree at `3b6e7ef` (base `chore/repo-audit`), and every claim still holds:

- `:44`: `ObjectAssetCatalogue.cs:14` `Paths` is public and has no `<summary>`. The doc build reports `ObjectAssetCatalogue.cs(14,56): warning CS1591`.
- `:47`: `MapCanvasViewModel.cs:15-18` claims an App-layer `DungeonMaster.App.ViewModels.MapTool` still exists. `grep -rn "enum MapTool" --include=*.cs src` (obj excluded) finds one declaration only, `DungeonMaster.App.Core/Editor/MapTool.cs:6`, so the comment is false and the `:19` alias is unneeded.
- `:48`: `INavigationService.cs:11` writes `"dungeondetail?id={guid}"`. `grep -rn dungeondetail --include=*.cs --include=*.xaml src` (obj and bin excluded) finds 2 lines: that one and the declaration `ShellRoutes.cs:12`.
- `:176`: `FloorMapDrawableNonFiniteTests.cs:38` says "corrected when `ZoomLevelGuard` was introduced". `02fc0dd` introduced the guard and did not touch the file. `1e978a4` made the correction (11+/11-). `git merge-base --is-ancestor 02fc0dd 1e978a4` succeeds.
- `:177`: `RecordingCanvas.cs:39` `<see cref="DrawString"/>` matches two overloads (`:103`, `:119`). The doc build reports `RecordingCanvas.cs(39,41): warning CS0419`.

## Definition of done

- [ ] `:44`: `Paths` carries a `<summary>` and the doc build reports no CS1591 for `ObjectAssetCatalogue.cs`.
- [ ] Gate 2 D1: `ObjectAssetCatalogue.cs:7` crefs `DungeonMaster.App.Views.DungeonDetail.ObjectPaletteView`, and the doc build reports no CS1574 for the file.
- [ ] `:47`: the false comment and the `MapTool` alias are gone from `MapCanvasViewModel.cs`, replaced by `using DungeonMaster.App.Core.Editor;` in sorted position.
- [ ] `:48`: `INavigationService.cs` names the route and query key by `<see cref>` to `ShellRoutes.DungeonDetail` and `ShellRoutes.IdQueryKey`. The `dungeondetail` census prints only `ShellRoutes.cs:12`, and the doc build reports nothing for the file.
- [ ] `:176`: line 38 reads "corrected after", and no other byte of the file changes.
- [ ] `:177`: the `DrawnStrings` summary crefs both `DrawString` overloads by parameter list, and the doc build reports no warning for `RecordingCanvas.cs`.
- [ ] The solution build prints `0 Warning(s)` and `0 Error(s)`, and the suite passes 217 / 976 / 137 with 0 failed, after each task.
- [ ] Every edited file stays all-CRLF with no lone CR.
- [ ] At Gate 3, exactly the five rows at `.sir/backlog.md:44`, `:47`, `:48`, `:176` and `:177` (line numbers at `3b6e7ef`) are removed. No heading is emptied: `## App, UI and UIKIT code` (`:22`) and `## Appended at Gate 3` (`:167`) both keep other rows. This item bounds the removals only, not the file's total.

## Scope

The five rows above, in five source files, across two tasks. Gate 2 (D1) also folded the neighbouring `ObjectAssetCatalogue.cs:7` CS1574 into T1, because T1 already edits that file.

### Not in scope

- Other doc-build warnings: the many pre-existing CS1591 in `DungeonMaster.App`. They are deferred to the existing `.sir/backlog.md:43` and `:46` rows.
- The other stale line citations in `FloorMapDrawableNonFiniteTests.cs`. They are deferred to the existing `.sir/backlog.md:174` row.
- Register and audit-doc corrections (`docs/audit/`), the `obj` rename population (`.sir/backlog.md:194`), and anything that needs a running app.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Base branch | `chore/repo-audit` at `3b6e7ef` | repo | `git rev-parse --verify chore/repo-audit`; lesson 2026-11-09 (sessions base on `chore/repo-audit`, not `main`) |
| Worktree root | `C:/dm-wt/` | repo | `.sir/config.json` `git.worktreeRoot`; lesson worktree-root (MAX_PATH) |
| `:48`: reword or cref? | Cref both `ShellRoutes` members. A doc build then diagnoses drift, and a reworded literal would not be diagnosed. | repo | `ShellRoutes.cs:4` ("declared once and matched by every producer and consumer"); the one producer `DungeonListViewModel.cs:107` composes `{ShellRoutes.DungeonDetail}?{ShellRoutes.IdQueryKey}=` |
| Is a public interface citing `internal` members acceptable? | Yes. `DungeonMaster.App` is an executable with no external API consumer. | judgment | `ShellRoutes.cs:6` `internal static class`; recorded in T1 criterion 12 |
| `:47`: does a plain `using DungeonMaster.App.Core.Editor;` collide with anything? | No. None of the 8 `Core/Editor` type names is declared anywhere else in App or App.Core. | repo | predicate `(class\|enum\|struct\|record\|interface) <Name>([^A-Za-z0-9_]\|$)` per name, control `MapEditorState` matched; 3 App files already use the plain using |
| `:177`: which overloads? | Both. Both add to `DrawnStrings`. | repo | `RecordingCanvas.cs:115`, `:122`; parameter-list cref precedent `MauiHostFixture.cs:70` |
| Evidence for a cref or a summary | The doc build only: `-p:GenerateDocumentationFile=true -p:TreatWarningsAsErrors=false --no-incremental` over App.Tests, which builds App too | repo | lesson 2026-11-04; probe at `3b6e7ef` printed the CS0419 and CS1591 controls |
| Doc-comment shape | Two tabs after `///`, one sentence per line, ending in `.` | repo | dm-conventions `backend/dotnet-format.md:374-378` (section 9) |
| Can builds run concurrently? | No. App.Tests references App, so they share App's `obj/`. T2 depends on T1. | repo | `DungeonMaster.App.Tests.csproj` project reference; architect report |
| May `Edit` write these files? | Yes. Each has 0 lone CR at `3b6e7ef`. `.sir/backlog.md` is 196 CRLF with 0 lone CR, so its Gate 3 removal may use `Edit` too. | repo | P-EOL per file (T1 criterion 20, T2 criterion 12); Python byte count on `.sir/backlog.md`, fixture control `a\r\nb\rc\n` printed 1 CRLF, 1 lone CR |
| `:176` git facts | `02fc0dd` (2026-11-02) did not touch the file. `1e978a4` (2026-11-03) did. | repo | `git show --stat --format= <sha> -- <file>`; `git merge-base --is-ancestor`. Pre-resolved, because tasks run no git |

## Contracts

None. No interface crosses between T1 and T2. The shared instruments (P-SLICE, P-SHOW, P-EOL, P-SHA, and the doc build) are inlined in each task's criteria in `manifest.json`.

## Open questions

None outside Gate 2's section 1.
