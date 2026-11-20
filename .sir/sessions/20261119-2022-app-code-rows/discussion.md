# App code rows

Close six App-layer rows of `.sir/backlog.md` whose action and check were already fixed by an earlier gate or spec, and remove each row at Gate 3.

## Context

The user asked for the next batch of backlog work. At `ba5937f` (branch `chore/repo-audit`) the orchestrator re-measured six rows and found all six still open. Line numbers below are `.sir/backlog.md` lines at `ba5937f`. File citations are as numbered at `ba5937f`.

1. **backlog.md:14, FocusArea ordering** (2026-08-27-give-the-editor-a-seam). In `DungeonDetailPage.FocusArea`, `AbortInProgressDraw();` at `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1182` runs `MapEditorState.ResetDrawState`, which sets `IsMoveDragging = false` (`src/DungeonMaster.App.Core/Editor/MapEditorState.cs:245`). The `if (_editor.IsMoveDragging)` cleanup at `:1186` reads that flag afterwards. The comment at `:1185` reads `// Abort any in-progress move drag` and does not name the ordering. The row's check is `grep -n -B3 -F "if (_editor.IsMoveDragging)" <file>`: it should print the new comment, or show the guard ahead of the abort call.
2. **backlog.md:16, AreaOutlineGeometry `n < 3` guard** (2026-08-27-give-the-editor-a-seam). The guard is `if (n < 3)` at `src/DungeonMaster.App.Core/Map/AreaOutlineGeometry.cs:146` and has no intent comment. The three small-polygon tests are `ContainsPoint_EmptyPolygon_ReturnsFalse`, `ContainsPoint_PolygonOfOneVertex_ReturnsFalse` and `ContainsPoint_PolygonOfTwoVertices_ReturnsFalse` at `src/DungeonMaster.Tests/AppCore/Map/AreaOutlineGeometryTests.cs:243`, `:249` and `:255`. The lowercased count of `documented intent` in that test file prints 0 (`python -I -c "print(open('src/DungeonMaster.Tests/AppCore/Map/AreaOutlineGeometryTests.cs',encoding='utf-8-sig').read().lower().count('documented intent'))"`). The row's checks are: the `grep -n -B3 -F "if (n < 3)"` output shows a comment, and the count is at least 3. The comment must say that the filing found that replacing the guard with `n < 0` changes no answer. The guard itself is neither fixed nor deleted.
3. **backlog.md:17, DmFlagBanner title editor removal** (2026-08-28-the-control-kit, Gate 2 D3 of 20261118-0114-backlog-triage). The following appear only inside `src/DungeonMaster.App/Controls/DmFlagBanner/`: `IsTitleEditable` with its property, `ApplyEditability`, `OnTitleCommitted`, the banner's `TextCommitted` event and the `TitleEditor` element. The sites are `DmFlagBanner.xaml:38`, `:49` and `DmFlagBanner.xaml.cs:28-82`. `git grep -n -e IsTitleEditable -e TitleEditor -- 'src/*.xaml' 'src/*.cs'`, with the banner folder filtered out, prints nothing. `TextCommitted` also names `DmInlineEditor`'s own event (`DmInlineEditor.xaml.cs:140`) and `NotebookView.xaml:99`, and both of those stay. The row's check: `git grep -n -e IsTitleEditable -e ApplyEditability -e OnTitleCommitted -- src` prints nothing, and the solution builds with 0 warnings.
4. **backlog.md:85, BtnCW/BtnCCW renames** (20261105-1907-core-heavy-rows, spec Not in scope). The row lists ten sites, which are exactly the sites `git grep -n -e BtnCW -e BtnCCW -- src` prints:
   - `ControlsPaletteView.xaml:66`, `:90`
   - `ControlsPaletteView.xaml.cs:27-28`
   - `DungeonDetailPage.xaml:470`, `:472`, `:491`, `:493`
   - `DungeonDetailPage.xaml.cs:167`, `:172`

   The row's check: `grep -rn -e BtnCW -e BtnCCW src --include=*.xaml --include=*.cs` prints nothing, and the solution builds with 0 warnings.
5. **backlog.md:86, `obj` renames** (20261109-1641-part-done-rows, Gate 2 D19 of 20261118-0114-backlog-triage). `grep -rnw obj src/DungeonMaster.App src/DungeonMaster.Tests --include=*.cs --exclude-dir=obj --exclude-dir=bin` prints these lines per file:

   | File | Lines |
   |---|---|
   | `App/Graphics/FloorMapDrawable.cs` | 12 |
   | `App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` | 20 |
   | `Tests/Domain/Entities/Areas/AreaOutlineTests.cs` | 2 |
   | `Tests/Domain/Entities/Floors/FloorObjectMethodsTests.cs` | 17 |
   | `Tests/Domain/Entities/Floors/ObjectKindGuardTests.cs` | 5 |
   | `Tests/Domain/Entities/Floors/PlacedObjectTests.cs` | 18 |
   | `Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs` | 2 |
   | `Tests/App/Theme/RepoPaths.cs` | 1 |
   | `Tests/App/Theme/RepoPathsTests.cs` | 1 |

   That is 32 App lines and 44 Tests lines in 5 files. The last two rows of the table hold `obj` directory-name strings and stay. The row's check: the same grep prints only those two strings.
6. **backlog.md:90, FloorDropLanding default remark** (20261111-1831-floor-drag-placeholder, Gate 2 D20 of 20261118-0114-backlog-triage). `grep -n -w default src/DungeonMaster.App.Core/Editor/FloorDropLanding.cs` prints nothing. `None` is at `:27` and `HasLanding` at `:36`. The remark should say that a `default` value reports `HasLanding = true`, and that `FloorDropLanding.None`, not `default`, means no landing. The row's check: that grep prints the remark.

## Decisions

- Track: Standard, confirmed at Gate 1 (user chose Standard).
- Scope: these six rows and nothing else. Each is removed from `.sir/backlog.md` at Gate 3 once its check passes.
- Row 14 takes the comment route. A comment above the `if (_editor.IsMoveDragging)` guard names the ordering, and the cleanup is not moved, so behaviour is unchanged.
- Row 85's names follow the palette's existing AutomationId spelling `PaletteRotateClockwise` (`ControlsPaletteView.xaml:93`):
  - the palette's x:Names become `RotateClockwiseButton` and `RotateCounterClockwiseButton`
  - the popup's x:Name and AutomationId become `PopupRotateClockwise` and `PopupRotateCounterClockwise`
- Row 86's new names are chosen per site from what each `obj` holds, for example `placedObject`. The two `RepoPaths` directory strings stay.
- `DungeonDetailPage.xaml.cs` is touched by rows 14, 85 and 86, so the tasks that own it are serialised.
- Behaviour change: none intended.
- Human check (1), to be routed to Gate 3: in the running app, the flag banner title still shows, and both rotate controls (palette and popup) still rotate a placed object. A static check cannot fully see x:Name renames or a XAML removal.
- Models: implementers and reviewers on opus, sir-git on haiku.
- Lessons to carry into the spec:
  - Check `tasklist` for a running `DungeonMaster.App.exe` before dispatching. A running copy locks `bin`.
  - Line endings: never use `Write` or `sed -i` on existing CRLF files.
  - `Edit` strips trailing whitespace from `new_string`.
