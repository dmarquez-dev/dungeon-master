# App comment fixes

Close five deferred comment and doc-comment rows in `DungeonMaster.App` and `DungeonMaster.App.Tests`, taken from the unscheduled sections of `.sir/backlog.md` after every scheduled heading was cleared by 20261109-1641-part-done-rows.

## Context

The five scheduled headings at `.sir/backlog.md:5-13` are all empty, so this session picked from the unscheduled sections, grouping rows from one layer as recent sessions have. Each row's target was spot-checked to still exist at discuss time (HEAD `3b6e7ef`), using `grep -rn "using MapTool"`, `grep -n Paths ObjectAssetCatalogue.cs`, `find -name INavigationService.cs`, `grep -n DrawString RecordingCanvas.cs` and `sed -n '36,39p' FloorMapDrawableNonFiniteTests.cs` run from `src/`. These were existence checks, not re-measurements. Every row still has to be re-measured in spec before a criterion is written on it.

- `.sir/backlog.md:44`: `ObjectAssetCatalogue.Paths` is public (`src/DungeonMaster.App/Assets/ObjectAssets/ObjectAssetCatalogue.cs:14`) and reported as having no `<summary>`.
- `.sir/backlog.md:47`: `src/DungeonMaster.App/ViewModels/MapCanvasViewModel.cs:19` holds `using MapTool = DungeonMaster.App.Core.Editor.MapTool;`, and the row reports a false comment beside it.
- `.sir/backlog.md:48`: `src/DungeonMaster.App/Services/Navigation/INavigationService.cs:11` is reported as the only place the route literal is written out instead of being read from `ShellRoutes.DungeonDetail`.
- `.sir/backlog.md:176`: `src/DungeonMaster.App.Tests/Graphics/FloorMapDrawableNonFiniteTests.cs:38` says the `ZoomLevel` text "was corrected when `ZoomLevelGuard` was introduced". The row says the guard landed at `02fc0dd` and the correction at `1e978a4`.
- `.sir/backlog.md:177`: `src/DungeonMaster.App.Tests/Graphics/RecordingCanvas.cs:39` crefs `DrawString`, which has two overloads (`:103`, `:119`), so the reference is ambiguous (CS0419). Only a documentation-generating build reports it, because no build file sets `GenerateDocumentationFile` (lesson 2026-11-04 20261103-2239-app-follow-ups).

## Decisions

- Track: Standard. There are five files across two projects, and each fix is a claim that can be checked on its own.
- Session name: `app-comment-fixes`.
- In scope: the five rows at `.sir/backlog.md:44`, `:47`, `:48`, `:176` and `:177`, and nothing else.
- Out of scope: register and audit-doc corrections (`docs/audit/`), the `obj` rename population (`.sir/backlog.md:194`), and anything that needs a running app or a human observation.
- `:47` changes name binding, so its task includes a solution build.
- `:48` may be met either by rewording the comment to describe the route's shape or by a `cref` to `ShellRoutes.DungeonDetail`. Spec picks one from repository evidence.
- `:177` is proven only by a documentation-generating build (for example `-p:GenerateDocumentationFile=true`) over `DungeonMaster.App.Tests`. The ordinary 0-warning build is not evidence for a cref.
- `:176` is verified against git history (`git log` on `02fc0dd` and `1e978a4`) before the wording is fixed.
- Each closed row is removed from `.sir/backlog.md` at Gate 3. Any blank line left under an emptied heading is handled as lesson 2026-11-09 records. `.sir/backlog.md` must not be written with `Edit` if it carries lone CR, so it is measured first.
- Base branch is `chore/repo-audit`, not `main` (lesson 2026-11-09 20261109-1641-part-done-rows).
