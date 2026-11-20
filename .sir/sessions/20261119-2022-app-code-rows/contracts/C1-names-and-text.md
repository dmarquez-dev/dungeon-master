# C1 - names and text (session 20261119-2022-app-code-rows)

Frozen at Gate 2. Every line number below is as numbered at `ba5937f`, read with `grep -n` in the worktree `/c/dm-wt/20261119-2022-app-code-rows` (HEAD `ba5937fc3c14f244592b479419c2b29a62884b86`). No task changes behaviour; every change below is a comment, a doc remark, an identifier rename, or the removal of code nothing reaches.

No task writes `.sir/backlog.md`, and no file any task writes cites a `.sir/backlog.md` line number or a `.sir/` path.

## 1. Edit rules (every task)

1. Existing files are changed with `Edit` only. `Write` and `sed -i` are forbidden on an existing file, because both rewrite a CRLF file as LF.
2. `Edit` strips trailing whitespace from `new_string` and still reports success. Never end a `new_string` on whitespace; key each edit on the first text character of a line.
3. Every touched file stays CRLF with no lone CR and no bare LF, and keeps its BOM state. At `ba5937f`, only `DmFlagBanner.xaml` and `ControlsPaletteView.xaml` begin with a UTF-8 BOM among the files this session touches.
4. Indentation is tabs in C#. In XAML, keep the spaces the file already uses. A new comment line takes the indentation of the code line it sits above.
5. A new comment goes on its own line or lines. It never trails a code line, and no blank line is added or removed next to it.

## 2. Row 14 - FocusArea ordering comment (T1)

File: `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`.

- At `:1180-1183`, `if (_editor.DrawState == DrawState.Drawing) { AbortInProgressDraw(); }`. `AbortInProgressDraw` (`:1226-1230`) calls `_editor.ResetDrawState(...)`, and `MapEditorState.ResetDrawState` (`src/DungeonMaster.App.Core/Editor/MapEditorState.cs:239`) sets `IsMoveDragging = false` at `:245`.
- At `:1185` the comment `// Abort any in-progress move drag` sits above `if (_editor.IsMoveDragging)` at `:1186`.
- Change: the run of `//` lines ending directly above `if (_editor.IsMoveDragging)` names the ordering. It says that `AbortInProgressDraw` has already run `ResetDrawState`, that this clears `IsMoveDragging`, and that the guard reads the flag after that. It contains the three literals `AbortInProgressDraw`, `ResetDrawState` and `IsMoveDragging`. The existing line may be kept as part of the run, or replaced. The guard and its body are not moved.

## 3. Row 16 - `n < 3` intent (T3 source, T4 tests)

- Source: `src/DungeonMaster.App.Core/Map/AreaOutlineGeometry.cs`, `var n = polygon.Count;` at `:145`, `if (n < 3)` at `:146`. The run of `//` lines ending directly above `if (n < 3)` (between `:145` and `:146`) records the guard as documented intent. It says that the filing found that replacing the guard with `n < 0` changes no answer, and it contains the literal `n < 0`. The guard is neither changed nor deleted.
- Tests: `src/DungeonMaster.Tests/AppCore/Map/AreaOutlineGeometryTests.cs`, methods `ContainsPoint_EmptyPolygon_ReturnsFalse` (`:243`), `ContainsPoint_PolygonOfOneVertex_ReturnsFalse` (`:249`) and `ContainsPoint_PolygonOfTwoVertices_ReturnsFalse` (`:255`). Inside each method's body, a `//` remark at two tabs, containing `documented intent`, says that the test pins the guard's documented result rather than covering live logic. No other method of the file gains the phrase. No test is renamed, added, removed, or has a code line changed.

## 4. Row 17 - DmFlagBanner title editor removal (T2)

Folder: `src/DungeonMaster.App/Controls/DmFlagBanner/`.

Removed:

- `DmFlagBanner.xaml:38-49`: the `<!-- Inline editor ... -->` comment and the `controls:DmInlineEditor` element `x:Name="TitleEditor"`, including its `TextCommitted="OnTitleCommitted"`.
- `DmFlagBanner.xaml:6`: `xmlns:controls="clr-namespace:DungeonMaster.App.Controls"`, which nothing else in the file uses once the editor is gone.
- `DmFlagBanner.xaml.cs:17-25`: `TitleProperty`'s `propertyChanged` callback. Its only effect is the `TitleEditor.Text` write at `:24`, and its comment at `:19-22` describes that write. `TitleProperty` stays, created by `BindableProperty.Create(nameof(Title), typeof(string), typeof(DmFlagBanner), string.Empty)`.
- `DmFlagBanner.xaml.cs:27-37`: `IsTitleEditableProperty` and its summary.
- `DmFlagBanner.xaml.cs:39-42`: the banner's `TextCommitted` event and its summary.
- `DmFlagBanner.xaml.cs:53-60`: the `IsTitleEditable` property and its summary.
- `DmFlagBanner.xaml.cs:70-74`: `ApplyEditability`.
- `DmFlagBanner.xaml.cs:76-83`: `OnTitleCommitted`.

Reworded: the class summary at `DmFlagBanner.xaml.cs:4` stops claiming editing support. It describes a display-only banner title, as one complete sentence ending in a period, at `///` plus two tabs.

Kept: the Grid at `DmFlagBanner.xaml:10`, whose `Margin="0,0,0,8"` keeps the file an offending entry of `MarginPendingMigration` (`src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:318`); the `TitleLabel` element (`:27-36`) unchanged; `Title`, its getter and setter, and the constructor. `DmInlineEditor`'s own `TextCommitted` (`DmInlineEditor.xaml.cs:140`, raised at `:468`) and `NotebookView.xaml:99` are untouched.

## 5. Row 85 - rotate control names (T1)

| Site at `ba5937f` | Old | New |
|---|---|---|
| `ControlsPaletteView.xaml:66` `x:Name` | `BtnCCW` | `RotateCounterClockwiseButton` |
| `ControlsPaletteView.xaml:90` `x:Name` | `BtnCW` | `RotateClockwiseButton` |
| `ControlsPaletteView.xaml.cs:27` | `BtnCCW` | `RotateCounterClockwiseButton` |
| `ControlsPaletteView.xaml.cs:28` | `BtnCW` | `RotateClockwiseButton` |
| `DungeonDetailPage.xaml:470` `x:Name` | `PopupBtnCCW` | `PopupRotateCounterClockwise` |
| `DungeonDetailPage.xaml:472` `AutomationId` | `PopupBtnCCW` | `PopupRotateCounterClockwise` |
| `DungeonDetailPage.xaml:491` `x:Name` | `PopupBtnCW` | `PopupRotateClockwise` |
| `DungeonDetailPage.xaml:493` `AutomationId` | `PopupBtnCW` | `PopupRotateClockwise` |
| `DungeonDetailPage.xaml.cs:167` | `PopupBtnCCW` | `PopupRotateCounterClockwise` |
| `DungeonDetailPage.xaml.cs:172` | `PopupBtnCW` | `PopupRotateClockwise` |

The palette `AutomationId`s `PaletteRotateCounterClockwise` (`ControlsPaletteView.xaml:69`) and `PaletteRotateClockwise` (`:93`) stay. The other popup buttons (`PopupBtnScaleDown`, `PopupBtnScaleUp`, `PopupBtnDelete`) and the `PopupBtn*` colour members (`DungeonDetailPage.xaml.cs:2133-2142`) are outside this row and stay. At `ba5937f` none of the four new names occurs under `src`.

## 6. Row 86 - `obj` renames (T1 App, T4 Tests)

The rule: a local bound to the result of `floor.AddObject(...)` is `addedObject`. A local bound to `new PlacedObject(...)`, and the loop variable and parameter that carry a placed object item through `FloorMapDrawable`, are `placedObject`. A local bound to the lookup of the selected object is `selectedObject`. Only the identifier `obj` is renamed. The `obj` directory-name strings at `src/DungeonMaster.Tests/App/Theme/RepoPaths.cs:67` and `src/DungeonMaster.Tests/App/Theme/RepoPathsTests.cs:195` stay. The lambda parameters `placedObj` (`DungeonDetailPage.xaml.cs:2105`, `:2194`, `:2230-2231`, `:2266`, `:2296`) and the local `loadedObj` (`DungeonWriteRepositoryTests.cs:237-242`) are not `obj` and stay.

| File | Declaration at `ba5937f` | New name | Uses renamed with it |
|---|---|---|---|
| `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs` | `:670` `foreach (var obj in Floor.Objects)` in `DrawPlacedObjects` | `placedObject` | `:672` |
| same | `:676` parameter `PlacedObjectItem obj` of `DrawPlacedObject` | `placedObject` | `:678-729`, 9 lines |
| `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` | `:2105` in `UpdateObjectPopup` | `selectedObject` | `:2107`, `:2115`, `:2116` |
| same | `:2194` in `RotateObjectAsync` | `selectedObject` | `:2196`, `:2202`, `:2203`, `:2209`, `:2211` |
| same | `:2230` in `ScaleObjectAsync` | `selectedObject` | `:2233`, `:2239`, `:2240`, `:2246`, `:2247` |
| same | `:2266` in `MoveObjectAsync` (looked up by `objectId`, not the selection) | `placedObject` | `:2268` (twice), `:2282`, `:2283` |
| `src/DungeonMaster.Tests/Domain/Entities/Areas/AreaOutlineTests.cs` | `:77` | `addedObject` | `:94` |
| `src/DungeonMaster.Tests/Domain/Entities/Floors/FloorObjectMethodsTests.cs` | `:25`, `:46`, `:62`, `:94`, `:139` | `addedObject` | the other 12 lines. This also avoids sharing a name with the lambda parameter `placedObject` at `:36`, `:54` and `:150` |
| `src/DungeonMaster.Tests/Domain/Entities/Floors/ObjectKindGuardTests.cs` | `:96` | `placedObject` | `:104-107` |
| `src/DungeonMaster.Tests/Domain/Entities/Floors/PlacedObjectTests.cs` | `:17`, `:37`, `:52`, `:121`, `:145` | `placedObject` | the other 13 lines |
| `src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs` | `:218` | `addedObject` | `:238` |

Population check at `ba5937f`: `grep -rnw obj src/DungeonMaster.App src/DungeonMaster.Tests --include=*.cs --exclude-dir=obj --exclude-dir=bin` prints 78 lines. That is 32 App lines (FloorMapDrawable 12, DungeonDetailPage 20, which hold 33 tokens because `:2268` has two), 44 Tests lines in the five files above, and the 2 RepoPaths strings. At `ba5937f` none of `selectedObject`, `placedObject` or `addedObject` occurs as a whole word in any of these seven files, except the lambda parameter `placedObject` in `FloorObjectMethodsTests.cs` (6 tokens).

## 7. Row 90 - FloorDropLanding remark (T3)

File: `src/DungeonMaster.App.Core/Editor/FloorDropLanding.cs`. The type's doc comment gains one `<remarks>` element. It goes after the last `/// </param>` (`:18`) and before `public readonly record struct FloorDropLanding(` (`:19`). The opener is `/// <remarks>` and the closer is `/// </remarks>`, both at column 0. Each description line is `///` plus two tabs and one complete sentence ending in a period (dm-conventions `backend/dotnet-format.md` section 9). The remark says that a `default` value reports `HasLanding` as `true`, because `default` sets `TargetIndex` to 0 and not to `FloorReorderNearest.NoBand` (-1, `FloorReorderNearest.cs:128`). It also says that `FloorDropLanding.None`, not `default`, means no landing. It contains the literals `default`, `HasLanding`, `true` and `None`. Any `cref` it carries names only `None`, `HasLanding`, `TargetIndex` or `FloorReorderNearest.NoBand`, because a solution build does not diagnose an unresolved cref (no build file sets `GenerateDocumentationFile`).

## 8. Operations

This contract declares names and text only. It declares no runtime operation, type, or event that one task provides and another calls.
