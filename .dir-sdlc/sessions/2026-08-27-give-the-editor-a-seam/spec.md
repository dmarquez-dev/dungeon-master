# Give the editor a seam

Extract the dungeon editor's interaction logic out of the page and view model into types a test can reach, and clear the correctness, platform and accessibility items sitting on the same three files.

## Context

Sub-session 5b of Session 5, following 5a ("Settle the App boundaries"), which is merged. 5a created
`DungeonMaster.App.Core` (`net10.0`, MAUI-free, referenced by `DungeonMaster.Tests`) and
`DungeonMaster.App.Tests` (`net10.0-windows10.0.19041.0`, references App). That is the seam this session
spends.

The editor's behaviour currently lives where nothing can execute it. Measured in the worktree on
2026-08-27:

| File | Lines | Command |
|---|---|---|
| `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` | 2415 | `wc -l` |
| `src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs` | 678 | `wc -l` |

The page declares 28 private fields, holds every transition of the draw/move/pan/selection state machine
across five gesture handlers and a keyboard handler, and performs three pieces of pure geometry as private
members of a `ContentPage`. The view model carries 11 `[RelayCommand]` methods plus four loose public async
methods across four unrelated responsibilities.

**The audit's own numbers for this work are unreliable.** Eleven recorded figures were re-measured during
decomposition and ten were wrong — see `## Assumptions`. Every criterion in `manifest.json` cites a figure
measured this session with the command that produced it.

## Definition of done

Each item is checkable by someone who did not write the code.

1. `AreaOutlineGeometry` and `MapHitTester` exist in `DungeonMaster.App.Core`, contain the outline
   translation, point-in-polygon test, polygon area and object bounding-box hit test, and reference no MAUI
   type. (MASTER-095 / UI-017)
2. A `MapEditorState` type in `DungeonMaster.App.Core` owns the draw, move-drag, pan and object-selection
   transitions, with `MapTool` and `DrawState` beside it. (MASTER-108 / UI-016, kernel half)
3. `DungeonDetailPage.xaml.cs` no longer declares the 18 interaction-state fields the manifest names, and
   retains the 11 it names as genuine view state. (MASTER-108 / UI-016, page half)
4. `DungeonDetailViewModel` is a composition root over `DungeonHeaderViewModel`, `FloorListViewModel` and
   `MapCanvasViewModel`, sharing state through `IDungeonEditingContext`. (MASTER-106 / UI-006)
5. Every mutating operation on the detail view models follows one stated reload policy, and the operations
   that deviate are documented as deliberate. (MASTER-094 / UI-007, uniform half)
6. The four loose public async methods are `[RelayCommand]` members and none takes a `dungeonId` parameter.
   (MASTER-052 / UI-008)
7. The canvas cursor is set behind a platform seam; when the Windows member cannot be resolved the failure
   is reported rather than swallowed. (MASTER-096 / UI-019)
8. `GetAsyncKeyState` no longer appears in `DungeonDetailPage.xaml.cs`; the secondary-button query sits
   behind a platform seam. (MASTER-056 / UI-020)
9. `DungeonListViewModel` takes an `INavigationService` and names no `Shell.Current`. (MASTER-053 / UI-009)
10. `DungeonListPage` takes a `Func<CreateDungeonPopup>` and no `IServiceProvider`, and the popup is still
    produced per click. (MASTER-054 / UI-011)
11. `CreateDungeonViewModel` reports its outcome through one channel, and the popup's subscriptions are
    either removed or replaced. (MASTER-076 / UI-012)
12. `CreateDungeonPopup.xaml` declares `x:DataType`, or records the exact diagnostic that prevents it.
    (MASTER-083 / UI-030)
13. The map `GraphicsView` carries an `AutomationId`, a semantic description and a hint; the FLOORS caption
    and each floor row carry heading semantics. (MASTER-084 / UI-031)
14. The back action from the detail page is explicitly owned by the page rather than inferred from platform
    chrome. (MASTER-058 / UI-033, reachable half)
15. The 48×48 palette chip is constructed in exactly one place, used by both palettes. (MASTER-057 / UI-023)
16. `src/DungeonMaster.Tests` contains executing tests for the extracted geometry, hit testing and editor
    state, and `dotnet test` reports a per-assembly pass count above the 506 baseline for that assembly.
17. The dead `DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail` import is removed from
    `DungeonDetailPage.xaml.cs`, having first been confirmed unused. (5a backlog 1)
18. 5a's two false "4-space indented" criterion texts are corrected where they actually live. (5a backlog 2)
19. The unchecked-`int` limit of the distance guard at `FloorMapDrawable.cs:553` is recorded in the code.
    (5a backlog 3)
20. The throwing behaviour introduced at `DungeonDetailViewModel.cs:179` is documented, not reverted.
    (5a backlog 4)
21. `dotnet build DungeonMaster.sln` from `<worktree>\src` reports 0 Warning(s), 0 Error(s), and
    `net10.0-maccatalyst` still compiles.

## Scope

The 14 audit entries MASTER-052, 053, 054, 056, 057, 058, 076, 083, 084, 094, 095, 096, 106 and 108, plus
the four items sub-session 5a left on `.dir-sdlc/backlog.md`.

### Not in scope

- **Anything requiring observed macCatalyst behaviour.** `net10.0-maccatalyst` is parked under ADR-001: it
  builds but cannot be run. This retires the first half of UI-033's suggested direction ("verify the
  affordance on macCatalyst") and bounds MASTER-056 and MASTER-096 to extraction behind a seam. Verification
  is compilation of that TFM plus inspection.
- **Constructing a page, view or `Application` in a test.** `DungeonMaster.App.Tests` runs outside a MAUI
  host. Whether that is fixable is Session 6's question.
- **The 13 entries held for sub-session 5c**, plus MASTER-159 and MASTER-161.
- Two remedy halves are proposed for exclusion and are a Gate 2 decision, not a settled matter — see
  `## Open questions`.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Is MASTER-094 still blocked by UI-004? | No. Its recorded blocker is closed. | repo evidence | `docs/audit/00-master-index.md:189` marks MASTER-001 (UI-004) **done** |
| Is the audit's "2,226-line" page figure current? | No — 2415. | repo evidence | `wc -l DungeonDetailPage.xaml.cs` |
| Is the audit's "397-line" view model figure current? | No — 678. | repo evidence | `wc -l DungeonDetailViewModel.cs` |
| UI-006 says nine `[RelayCommand]` methods. | 11. | repo evidence | `grep -c "\[RelayCommand\]"` |
| UI-008 says five public async methods. | 4 methods; the 5th grep hit is the `LoadTask` **property** at `:89`. | repo evidence | `grep -nE "^\s*public\s+(async\s+)?(Task\|ValueTask)"` |
| UI-007 says 11 of 12 mutations reload, exactly one does not. | 11 reload sites against 14 `SendAsync` members; **three** do not reload. Never 12/1. | repo evidence | `grep -n "await LoadDungeonAsync"` → 11 hits |
| UI-019 puts the cursor code at `:911-934`. | `SetCanvasCursor` is at `:1029`. | repo evidence | `grep -n "private void SetCanvasCursor"` |
| UI-020 puts the P/Invoke at `:71-74` and its use at `:1186-1203`. | Declaration `:74`, call site `:1310`. | repo evidence | `grep -n "GetAsyncKeyState"` |
| UI-009 puts `Shell.Current` at `DungeonListViewModel.cs:101-103`. | `:104`. A second hit at `DungeonDetailPage.xaml.cs:346` is a different concern and out of scope. | repo evidence | `grep -rn "Shell\.Current" src/DungeonMaster.App/` |
| UI-012 puts the events at `:35` and `:40`. | `:36` and `:41`. | repo evidence | `grep -n "public event"` |
| UI-030 says six reflection-based bindings. | 7 `{Binding}` expressions; six target the VM, the seventh sets `Source={x:Reference Chrome}`. | repo evidence | `grep -n "{Binding "` |
| Backlog 1 puts the dead using at page `:11`. | `:12`, and it is the file's only hit for that namespace — genuinely dead. | repo evidence | `sed -n 12p`; `grep -c "GetDungeonDetail\|Dto"` → 1 |
| Backlog 2 says the false indent text is in 5a's `contracts/`. | It is not. It is in 5a's `manifest.json:82` and `:120`. `contracts/` contains no such claim. | repo evidence | `grep -rn "four spaces\|4-space" .dir-sdlc/sessions/2026-08-26-.../` |
| Can `Microsoft.Maui.Graphics` types enter App.Core? | No — it would need `Directory.Packages.props` and a lock file no task owns. The kernel uses `CanvasPoint`, float pairs and a new `CanvasRect`. | repo evidence | no such `PackageVersion`; `TileCoordinateHelper.ToPixel` already returns a tuple |
| Where does `ObjectKindInfo` live? | Stays in App; injected into `MapHitTester` as `Func<ObjectKind,(float,float)>`. | repo evidence | type is MAUI-free but referenced by files no task owns |
| Are the platform seams DI-registered? | No — static/partial, so `MauiProgram.cs` stays T4's alone. | repo evidence | `App.csproj` sets `SingleProject=true`; MAUI auto-includes `Platforms/**` per TFM |
| Does `DungeonDetailViewModel` resolve its children from the container? | No — it constructs them, so T3 never touches `MauiProgram.cs`. | judgment | also satisfies the backend discipline's "resolves nothing at runtime from a global container" |
| Verified build/test baseline at `d33ec23`? | Build 0 Warning(s)/0 Error(s); **506** passed in `DungeonMaster.Tests`, **1** in `DungeonMaster.App.Tests`. Two per-assembly lines, **no aggregate**. | repo evidence | `dotnet build`/`dotnet test DungeonMaster.sln`, run in the main checkout |

## Contracts

- `contracts/app-core-editor.cs` — geometry, hit testing, `MapEditorState`, `MapTool`, `DrawState`,
  `CanvasRect`. Owner T1; consumers T2, T5.
- `contracts/app-detail-view-models.cs` — the four-way view-model split, `IDungeonEditingContext`, the
  argument records. Owner T3; consumers T5, T6.
- `contracts/app-navigation.cs` — `INavigationService` and the `Func<CreateDungeonPopup>` factory.
  Owner T4; consumer T3.

## Open questions

| Question | Who resolves it | When | Outcome |
|---|---|---|---|
| Two remedy halves are uncovered: UI-016's `RebuildFloorList` → `BindableLayout` conversion (page `:729-940`, 212 lines) and UI-007's "then narrow it" projection-patching half. Take them, defer them to 5c, or take one? | User | Gate 2 | **Resolved** — both deferred to sub-session 5c, appended to `.dir-sdlc/backlog.md` at Gate 3. MASTER-108 and MASTER-094 therefore close **partially** in 5b. |
| `src/DungeonMaster.Tests/App/**` holds five source-scanning tests that T3, T5 and T7 can each break, and the path belongs to no task. Leave it as an escalation trigger, or assign it? | User | Gate 2 | **Resolved** — left unowned as an escalation trigger. T3 c.16, T5 c.20 and T7 c.11 require escalation rather than editing, so a cross-task write cannot happen silently. Expected to fire at least once. |
| Whether `$(MauiEnableXamlCBindingWithSourceCompilation)` lifts the `MAUIG2045` blocker on the popup's `Source`-bearing binding is not knowable without running the build. T4's criteria are written as an attempt plus an explicit accept-the-recorded-result. | T4, at implementation | During dispatch | Open |
