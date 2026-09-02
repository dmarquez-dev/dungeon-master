# Spike report — MAUI application host feasibility in `DungeonMaster.App.Tests`

Run 2026-09-01 in the session worktree `C:\dm-wt\2026-09-01-teardown-and-lifetime`, clean at `55fc7da`.
Verbatim record of the pre-decomposition spike. Nothing here was inferred by the orchestrator.

## 1. Verdict

**Yes.** A full MAUI application host — DI container, `Application.Current`, `Application.Current.Resources`, one `Window`, `AppShell` as root page — runs in-process in `DungeonMaster.App.Tests` under `dotnet test` with the existing xunit 2.9.3 / `xunit.runner.visualstudio` 3.1.4 (VSTest) setup, and `ContentView`/`ContentPage`/`DmCard` construct on it. It requires three things, each measured necessary by removing it and watching the host die.

## 2. Evidence

All commands run from `C:\dm-wt\2026-09-01-teardown-and-lifetime`. Probes lived in a temporary `src/DungeonMaster.App.Tests/SpikeProbes.cs` (deleted; final copy at the spike scratch directory as `SpikeProbes.cs.final`). Each probe was run in its own process — `dotnet test src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj --no-build --nologo --filter "FullyQualifiedName~SpikeProbes.<Pnn>"` — because a failed type initializer is cached per process and poisons every later probe.

### 2a. The documented failure, reproduced verbatim (baseline csproj, unmodified)

`P01`, `new PanGestureRecognizer()`:

```
System.TypeInitializationException : The type initializer for 'Microsoft.Maui.Controls.Element' threw an exception.
---- System.TypeInitializationException : The type initializer for 'Microsoft.Maui.Handlers.ViewHandler' threw an exception.
-------- System.Runtime.InteropServices.COMException :
   at Microsoft.Maui.Controls.Element.RemapForControls()
   at Microsoft.Maui.Handlers.RemappingHelper.RemapIfNeeded(Type handlerType, Action remapForControls)
   at Microsoft.Maui.Controls.Element.RemapIfNeeded()
   at Microsoft.Maui.Controls.Element..cctor()
   at WinRT.ExceptionHelpers.ThrowExceptionForHR(Int32 hr)
   at ABI.WinRT.Interop.EventSource`1.Subscribe(TDelegate handler)
   at Microsoft.UI.Xaml.Input.FocusManager.add_GotFocus(EventHandler`1 value)
   at Microsoft.Maui.Handlers.ViewHandler..cctor()
```

Same harness as the handoff describes. **The handoff attributes it to `ContentView`; the failure is actually one level lower** — see the boundary below. `P09` (bare STA thread) gave the HRESULT: `COMException (0x8001010E)` = `RPC_E_WRONG_THREAD`. `P08` measured the xunit test thread: `APARTMENT=MTA`.

### 2b. What the two blockers actually were

`P13` (`new HoverOrLongPressBehavior()`) failed differently, and that was the tell:

```
System.Runtime.InteropServices.COMException : ClassFactory cannot supply requested class
   at WinRT.ActivationFactory.Get(String typeName, Guid iid)
   at Microsoft.UI.Dispatching.DispatcherQueue.get__objRef_global__Microsoft_UI_Dispatching_IDispatcherQueueStatics()
   at Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()
```

That is the Windows App SDK runtime not being loaded into the process. Per Microsoft, the SDK injects its bootstrap initializer only when `OutputType` is `Exe`/`WinExe`; a test **library** loaded by a non-WinAppSDK host must opt in explicitly with `<WindowsAppSdkBootstrapInitialize>true</WindowsAppSdkBootstrapInitialize>`:

- https://learn.microsoft.com/windows/apps/windows-app-sdk/use-windows-app-sdk-run-time#behind-the-scenes,-and-opting-out-of-auto-initializers
- https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-winui#configure-an-unpackaged-test-app

With that property set (temporary edit; byte copy taken first, restored and `cmp`-verified):

- `P13` → `Passed! - Failed: 0, Passed: 1`.
- `P15` (`Microsoft.UI.Xaml.Application.Start` on an STA thread) stopped crashing the test host and ran a message loop for the full 20 s timeout: `WINUI START RESULT: no exception thrown / thread still running`. Before the property it was `The active test run was aborted. Reason: Test host process crashed`.
- `P01` still failed identically — bootstrap alone does not reach `Element`.

`P19` (WinUI `Application.Start` + bare `new Microsoft.UI.Xaml.Application()`, then MAUI types on its dispatcher) is where `ContentView` first worked; the failure had moved on to app resources:

```
P19: MAUI FAILED: Microsoft.Maui.Controls.Xaml.XamlParseException: Position 11:9.
StaticResource not found for key DmStrokeHairline
   at DungeonMaster.App.Controls.DmCard.InitializeComponent()
```

`P20` (`MauiProgram.CreateMauiApp()` on that dispatcher) failed inside the builder:

```
P20: CreateMauiApp FAILED: System.Reflection.TargetInvocationException
 ---> System.Runtime.InteropServices.COMException (0x80004005): Unspecified error
Cannot find a resource with the given key: AcrylicBackgroundFillColorDefaultBrush.
   at Microsoft.UI.Xaml.Controls.XamlControlsResources..ctor()
   at Microsoft.Maui.Platform.ResourceDictionaryExtensions.AddLibraryResources[T](ResourceDictionary resources)
   at Microsoft.Maui.Hosting.MauiAppBuilder.Build()
```

That is a PRI-resolution failure. The test output directory had only `DungeonMaster.App.pri`; WinUI resolves the app PRI by the **process** executable name, and the process is `testhost.exe`. Copying `DungeonMaster.App.pri` to `testhost.pri` next to `testhost.exe` fixes it. Copying the three framework PRIs (`Microsoft.UI.pri`, `Microsoft.UI.Xaml.Controls.pri`, `Microsoft.WindowsAppRuntime.pri`) does **not** — measured both ways, see 2d.

### 2c. The host, working

`P21` starts the application's real entry point, `DungeonMaster.App.WinUI.App : MauiWinUIApplication`, on a background STA thread, waits for `Microsoft.Maui.Controls.Application.Current`, then dispatches onto it:

```
P21: MAUI host UP. card=True page=True resources=2
```

`card` is a real `DmCard` (a `ContentView` with XAML and `StaticResource` lookups), `page` a `ContentPage`.

`P22`, the same host, exercising the entries' actual subjects:

```
P22: A: recognizers after attach=2; after detach=1;
     B: pack=True paths=29 decoded 2_Minute_Tabletop/Objects/furniture/bed.png=True;
     C: services=True detailPage=True;
     D: windows=1 rootPage=AppShell
```

- **A** is `MASTER-006` measured as behaviour, not asserted over text: attaching `HoverOrLongPressBehavior` to a `BoxView` adds 2 gesture recognizers; removing the behavior leaves 1 behind. That is the leak, observed.
- **B**: `PlatformImage.FromStream` decodes a real pack asset under the host (it throws bare `COMException` without one — `P10`).
- **C**: `IPlatformApplication.Current.Services` resolves `DungeonMaster.App.Views.DungeonDetailPage`.
- **D**: the shell is genuinely up.

Working host code, fixture-shaped, with the three requirements documented inline, is at `MauiHostFixture.cs` in the spike scratch directory.

A fourth non-obvious requirement is in there too: `app.UnhandledException += (_, e) => e.Handled = true`. Something raises an `InvalidOperationException` on the WinUI dispatcher during startup; without the handler it fail-fasts and VSTest reports only `Test host process crashed` with no diagnostic at all. The sequence was found by writing a trace file from inside the callback (`p21-trace.log`).

### 2d. The three necessity A/Bs

| Removed | `P21` result | Log |
|---|---|---|
| all 4 PRI copies | `Test host process crashed` | `P21_nopri.log` |
| `testhost.pri` (3 framework PRIs kept) | `Test host process crashed` | `P21_frameworkpri.log` |
| the 3 framework PRIs (`testhost.pri` kept) | `MAUI host UP. card=True page=True resources=2` | `P21_onlytesthostpri.log` |
| `WindowsAppSdkBootstrapInitialize` (PRI kept) | `Test host process crashed` | `P21_nobootstrap.log` |

So: `WindowsAppSdkBootstrapInitialize=true` **and** `testhost.pri` **and** an STA thread. The framework PRIs are not needed.

## 3. The boundary

| Without any host (repo as it stands) | |
|---|---|
| Plain classes, MAUI-typed signatures | **Yes** — 30 existing tests; `FloorMapDrawable` with the `Func<FloorMapPalette>` seam constructs, `PreloadObjectImagesAsync()` and `ClearObjectImageCache()` run (`P11` passed) |
| `CanvasLongPressDetector` (a plain `IDisposable`, no MAUI base) | **Yes** (`P14` passed) |
| `MauiApp.CreateBuilder(useDefaults: false).Build()` | **Yes** (`P07` passed) |
| Anything deriving from **`Microsoft.Maui.Controls.Element`** | **No.** The wall is `Element`'s static constructor, not `ContentView`. `PanGestureRecognizer` (`P01`), `Element.AutomationIdProperty` (`P02`), `BoxView` (`P03`), `ContentView` (`P04`), `DmCard` (`P05`) all fail identically |
| `Microsoft.UI.Xaml.Controls.Grid` | **No** (`P16`) |
| `PlatformImage.FromStream` | **No** — bare `COMException` (`P10`) |
| `MauiProgram.CreateMauiApp()` | **No** — dies in `UseMauiApp<App>()` at `Element..cctor` (`P06`) |
| A bare STA thread, with or without `DispatcherQueueController.CreateOnCurrentThread()` | **Does not help** — `RPC_E_WRONG_THREAD` at `Element..cctor` (`P09`, `P18`) |

| With bootstrap only, still no WinUI `Application` | |
|---|---|
| `HoverOrLongPressBehavior` (a `BindableObject`, not an `Element`) | **Yes** (`P13`) — the dispatcher lookup succeeds; the `Element` wall is unchanged (`P01` still fails) |

| With the full host (`P21`/`P22`) | |
|---|---|
| `ContentView`, `ContentPage`, `DmCard`, gesture recognizers, `View.Behaviors` attach/detach | **Yes** |
| `PlatformImage.FromStream` on a real asset | **Yes** |
| DI container, `DungeonDetailPage` resolution, `Application.Current.Resources`, `Windows[0].Page` | **Yes** |

Boundary sentence: *without a started WinUI `Application`, nothing deriving from `Microsoft.Maui.Controls.Element` can be constructed, and no Win2D-backed graphics call succeeds; with one, everything tried worked.*

## 4. Per-entry tiers

Tier 1 = executable without a host (possibly behind an injected seam). Tier 2 = executable only with the host. Tier 3 = not executable here; structural assertion only.

| Entry | Subject | Tier | Strongest demonstration available | Seam needed (tier 1) and cost |
|---|---|---|---|---|
| `MASTER-006` (`UIKIT-003`) | `HoverOrLongPressBehavior` never detaches the pan recogniser | **2** | Already produced: `view.GestureRecognizers.Count` is 2 after attach and **1** after `Behaviors.Remove` (`P22`, section A). The fixed version must leave 0. No seam can avoid it — the defect is a mutation of `View.GestureRecognizers`, and `View` is an `Element` | n/a |
| `MASTER-013` (`UIKIT-001`) — drawable half | `FloorMapDrawable` owns `IImage` and is not `IDisposable` | **1** | Inject a recording `IImage` and assert `Dispose()` is called by `Dispose()` and by `ClearObjectImageCache()`. `FloorMapDrawable` already constructs host-free (`P11`) | An image-factory seam, e.g. a `Func<Stream, IImage>` constructor overload beside the existing `Func<FloorMapPalette>` one. **Low** — the identical precedent is in the same file |
| `MASTER-013` — page-teardown half | `DungeonDetailPage` must dispose the drawable at teardown | **2** | `DungeonDetailPage` resolves from the live DI container under the host (`P22`, section C), so it can be constructed and its teardown driven | n/a |
| `MASTER-055` (`UI-018`) | Scroll-zoom unsubscribe re-resolves `MapCanvas.Handler?.PlatformView` | **2, unconfirmed** — see §5 | Under the host, capture-vs-re-resolve can be tested if the canvas gets a real handler. The spike did **not** measure that a `GraphicsView` handler materializes for a page here | Alternatively **1**: put subscribe/unsubscribe behind a small interface over the native element so a fake can report a stranded delegate. **Medium** — it changes `DungeonDetailPage`'s attach/detach shape, which is the code under audit |
| `MASTER-062` (`UIKIT-017`) | `_active` / `_touching` written on UI thread, read on timer thread | **3 for the race itself; 1–2 for the fix's behaviour** | No test can force a memory-visibility reordering, so the stale-`true` misfire is not demonstrable — the source record itself says "I have not observed the misfire". What *is* demonstrable: `CanvasLongPressDetector` is a plain class and constructs host-free (`P14`), so its `TouchStarted`/`TouchEnded`/`LongPressed` contract is tier 1 today; if the fix is `IDispatcherTimer` from the view's `Dispatcher`, asserting the tick arrives on the UI thread is tier 2 under the host. A `volatile`-keyword assertion by reflection would be the structural class the user already rejected | For the tier-1 half, none — `CanvasLongPressDetector` already needs no seam |
| `MASTER-160` | `PreloadObjectImagesAsync` overwrites a decoded image with `null`, undisposed, if the stream throws on close | **1** | The trigger is `stream.Dispose()` throwing. Inject a stream whose `Dispose` throws plus a recording `IImage`, then assert the entry was disposed before being replaced. Whole path lives in `FloorMapDrawable`, which is host-free | A stream-source seam (`Func<string, Task<Stream>>`) alongside the image-factory seam above — same constructor-overload pattern. **Low**, and it is the same edit as `MASTER-013`'s drawable half, so the two share one seam |

Note on the alternative for `MASTER-013`/`160`: under the host a real asset decodes (`P22`, section B), but a real `PlatformImage`'s `Dispose()` is not observable from the test, so the seam is the better route for both regardless of the host.

## 5. What the spike could not settle

- **Whether `MapCanvas` gets a real handler and native `FrameworkElement` under this host.** That is the exact thing `MASTER-055` is about, and it needs the Shell navigated to `DungeonDetailPage` in-process. The page resolves from DI and a Window with `AppShell` exists; navigation was not attempted. One further probe: `Shell.Current.GoToAsync` to the detail route, then read `MapCanvas.Handler?.PlatformView`.
- **`MauiProgram.CreateMauiApp()` runs the real EF migration against the user's real database** — `Environment.SpecialFolder.LocalApplicationData\DungeonMaster\dungeon-master.db`. The probe runs did that. Any fixture that ships must redirect it, and `MauiProgram` currently offers no seam for the connection string.
- **One host per process.** `Microsoft.UI.Xaml.Application.Start` can be called once. A fixture must therefore be a single xunit *assembly*-wide fixture, and every host-requiring test must funnel through it, serialized. xunit parallelism against it was not measured, nor whether a second test class dispatching concurrently onto the one dispatcher behaves.
- **The swallowed `InvalidOperationException`** during startup (visible only as `FIRSTCHANCE` entries in `p21-trace.log`). Masking it is what keeps the process alive; it was not identified. It should be identified before a fixture depends on `Handled = true` as a blanket.
- **`testhost.pri` as a build step.** The spike copied it by hand each run. Making it an MSBuild target that copies `$(OutDir)DungeonMaster.App.pri` to `testhost.pri` is straightforward but untested, and it is brittle against the runner's executable name — `dotnet test` under a different runner (or MTP) would need a different name. Microsoft's documented supported shape is different: an `OutputType=Exe` self-hosted WinUI test app run under MTP, with the note "VSTest doesn't support this unpackaged WinUI configuration" (https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-winui#configure-an-unpackaged-test-app). **What the spike measured contradicts that note for this configuration** — it works under VSTest — but the session should know it is off the documented path, and that xunit 2.9.3 cannot run under MTP if the recipe ever proves fragile.

## 6. Worktree state

`git status --porcelain` returned empty. The one temporary edit (`WindowsAppSdkBootstrapInitialize` in `DungeonMaster.App.Tests.csproj`) was byte-copied first, restored from that copy and verified with `cmp`; the temporary `SpikeProbes.cs` was deleted, as were the PRI copies in `bin/`.

Post-cleanup: `dotnet build src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj` → `Build succeeded. 0 Warning(s) 0 Error(s)`; `dotnet test src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj --no-build` → `Passed! - Failed: 0, Passed: 30, Total: 30 - DungeonMaster.App.Tests.dll (net10.0)`.

## 7. Decisions taken on this report

- **E1 — evidence strategy: hybrid.** Constructor seams for `MASTER-013`'s drawable half and `MASTER-160`; the host fixture only for `MASTER-006`, `MASTER-013`'s page-teardown half, and `MASTER-055`.
- **E2 — test database: add a database-location seam to `MauiProgram`** so the fixture points at a temporary file. Accepted as new scope for this sub-session; `MauiProgram` is named by none of the five entries.
