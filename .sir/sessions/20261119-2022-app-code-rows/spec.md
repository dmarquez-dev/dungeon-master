# App code rows

Close six App-layer rows of `.sir/backlog.md` whose action and check an earlier gate or spec already fixed, with no behaviour change, and remove each row at Gate 3.

## Context

At `ba5937f` (branch `chore/repo-audit`) six backlog rows are still open, re-measured in `discussion.md`: the FocusArea ordering comment, the `AreaOutlineGeometry` `n < 3` intent comment, the `DmFlagBanner` title editor nobody uses, the `BtnCW`/`BtnCCW` names, the `obj` locals, and the `FloorDropLanding` `default` remark. Every name and text the work must produce is fixed in `contracts/C1-names-and-text.md`. The worktree is `C:/dm-wt/20261119-2022-app-code-rows` on `agent/20261119-2022-app-code-rows`.

## Definition of done

1. **Row 14.** The run of `//` lines directly above `if (_editor.IsMoveDragging)` in `DungeonDetailPage.FocusArea` says that `AbortInProgressDraw` has already run `ResetDrawState`, that this clears `IsMoveDragging`, and that the guard reads the flag afterwards. The guard is not moved.
2. **Row 16, source.** A `//` comment directly above `if (n < 3)` in `AreaOutlineGeometry.ContainsPoint` records the guard as documented intent. It says the filing found that replacing the guard with `n < 0` changes no answer. The guard is unchanged.
3. **Row 16, tests.** Each of the three small-polygon tests in `AreaOutlineGeometryTests.cs` carries a `//` remark containing `documented intent`, so the lowercased count in the file is at least 3. No other method gains the phrase.
4. **Row 17.** `IsTitleEditable`, `ApplyEditability`, `OnTitleCommitted`, the banner's `TextCommitted`, `TitleEditor`, `TitleProperty`'s editor-only callback and the unused `xmlns:controls` are gone from `DmFlagBanner`. `grep -rn -w TextCommitted` under `src` (excluding `obj` and `bin`) then prints only `DmInlineEditor`'s two lines and `NotebookView.xaml`'s one. The banner title binding and `AutomationId="FlagBannerTitle"` remain.
5. **Row 85.** `grep -rn -e BtnCW -e BtnCCW src --include=*.xaml --include=*.cs` prints nothing. The 10 sites carry `RotateClockwiseButton` and `RotateCounterClockwiseButton` (palette x:Names), and `PopupRotateClockwise` and `PopupRotateCounterClockwise` (popup x:Names and AutomationIds).
6. **Row 86, App.** `grep -rnw obj src/DungeonMaster.App --include=*.cs --exclude-dir=obj --exclude-dir=bin` prints nothing. Each site has the C1 section 6 name.
7. **Row 86, Tests.** The same grep over `src/DungeonMaster.Tests` prints only the two `RepoPaths` directory strings, `RepoPaths.cs:67` and `RepoPathsTests.cs:195`.
8. **Row 86, whole row.** After the merge, in the main checkout, `grep -rnw obj src/DungeonMaster.App src/DungeonMaster.Tests --include=*.cs --exclude-dir=obj --exclude-dir=bin` prints exactly those two lines.
9. **Row 90.** `FloorDropLanding` gains one `<remarks>` saying that a `default` value reports `HasLanding` as true and that `FloorDropLanding.None`, not `default`, means no landing. `grep -n -w default` on the file prints only `///` lines.
10. **No behaviour change.** Each touched code file hashes the same as at base once the declared renames are mapped back and `//` lines are dropped. `dotnet build src/DungeonMaster.sln` prints `0 Warning(s)` and `0 Error(s)`. `dotnet test src/DungeonMaster.sln --no-build` passes 217, 1012 and 146 (App.UnitTests, DungeonMaster.Tests, App.Tests).
11. **Line endings.** Every touched file stays CRLF with CR = CRLF = LF and keeps its BOM state.
12. **Backlog.** The six rows whose keys C1 lists are removed from `.sir/backlog.md` at Gate 3. Each key then appears 0 times, and the file has no lone CR. This bounds only those six removals. Whatever else Gate 3 carries onto the file is not counted here.
13. **Human check, at Gate 3.** Build, then run the built `win-x64/DungeonMaster.App.exe` from the worktree, with a dungeon whose floor holds at least one placed object. Expected:
    - The list page banner still reads "Dungeons", and the detail page banner still shows the dungeon name, as before.
    - Selecting the object and clicking each popup rotate button turns it a quarter turn each way, as before.
    - Each controls-palette rotate button does the same to the selected object, and to the ghost while a palette entry is active.

## Scope

The six rows above, in the 15 files the manifest's four tasks own, and the six row removals from `.sir/backlog.md` at Gate 3.

### Not in scope

- Fixing or deleting the `n < 3` guard, or moving the FocusArea cleanup: both rows took the comment route at Gate 1.
- Other `PopupBtn*` names (`PopupBtnScaleDown`, `PopupBtnScaleUp`, `PopupBtnDelete`, the `PopupBtn*` colour members) and the palette AutomationIds: outside row 85.
- The lambda parameter `placedObj` and the local `loadedObj`: they are not `obj`.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Can two tasks build at once in one worktree? | No, so the tasks form the strict chain T1, T2, T3, T4. | repo | Every project shares one `obj/` and `bin/`. Lesson 2026-11-12 records MSB3021/MSB3027 lock failures. |
| Does any criterion inject a defect? | No. Nothing needs ordering for it. | repo | `manifest.json` criteria |
| Which name does each DungeonDetailPage `obj` take? | `selectedObject` where the lookup is by `_editor.SelectedObjectId` (`UpdateObjectPopup`, `RotateObjectAsync`, `ScaleObjectAsync`). `placedObject` in `MoveObjectAsync`, which looks up by `objectId`. | repo | `DungeonDetailPage.xaml.cs:2105`, `:2266` |
| What do `floor.AddObject(...)` results take? | `addedObject`. That avoids clashing with the lambda `placedObject`. | repo | `FloorObjectMethodsTests.cs:36` |
| Does row 17's removal set include `TitleProperty`'s callback and `xmlns:controls`? | Yes, since both are dead without the editor. | repo | The row names the `TitleEditor.Text` write. `DmFlagBanner.xaml.cs:24`, `DmFlagBanner.xaml:6` |
| Is the banner class summary reworded? | Yes, to a display-only description. It currently claims "optional inline editing support". | judgment | `DmFlagBanner.xaml.cs:4` |
| Does the banner Grid's `Margin` stay? | Yes, because `EveryGeometryAllowListEntryStillOffends` requires the file to keep offending. | repo | `GeometryScaleTests.cs:318` |
| Which crefs may the row-90 remark use? | Only `None`, `HasLanding`, `TargetIndex` or `FloorReorderNearest.NoBand`, because a build does not diagnose crefs. | repo | Lesson 2026-11-04 (no `GenerateDocumentationFile`) |
| What is each task's test baseline? | 217 / 1012 / 146 at its own startRef, with delta 0 through the chain. | repo | Probe of `dotnet test --no-build` at `ba5937f` |
| Does a build rewrite a tracked file? | Not at base. `git status --short` was empty after the probe build. | repo | Spec-phase probe |
| Where is the worktree? | `C:/dm-wt/<id>`, from `git.worktreeRoot`. | repo | `.sir/config.json` |

## Contracts

- [`contracts/C1-names-and-text.md`](contracts/C1-names-and-text.md): the edit rules, the row-14, row-16 and row-90 text, the row-17 removal set, and the row-85 and row-86 name tables. It declares no runtime operation.

## Open questions

None.
