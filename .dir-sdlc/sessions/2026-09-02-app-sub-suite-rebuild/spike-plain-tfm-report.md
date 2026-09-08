# Spike: plain `net10.0` TFM on `DungeonMaster.App`

Measurement spike. Every change described here was reverted; the worktree ended clean.

- **Worktree:** `C:\dm-wt\2026-09-02-app-sub-suite-rebuild`, branch `dir/2026-09-02-app-sub-suite-rebuild`, at `83ba80c`.
- **Solution:** `C:\dm-wt\2026-09-02-app-sub-suite-rebuild\src\DungeonMaster.sln`
- **Date:** 2026-09-02

## Corrections to the brief

Everything in the brief that I re-checked held, with two additions:

1. The WinUI grep list is **correct as source files**, but the same grep also matches one build artifact,
   `src/DungeonMaster.App/obj/Debug/net10.0-windows10.0.19041.0/win-x64/XamlTypeInfo.g.cs`. Six source files, as stated.
   Command (run in the worktree):

   ```
   grep -rln "Microsoft\.UI\.\|WinRT\|Windows\.Foundation\|Microsoft\.Windows\.\|#if WINDOWS" src/DungeonMaster.App --include=*.cs | grep -v "/Platforms/"
   ```

2. **None of those six files needed a single guard.** They are already inside `#if WINDOWS`. The compile
   failures on the bare TFM came from somewhere the brief did not anticipate — see Q1/Q2.

`Directory.Build.props:21` is confirmed as `RestorePackagesWithLockFile`. Both csproj files matched the brief's
description exactly. All files involved are CRLF, verified with `tr -cd '\r' | wc -c` against `tr -cd '\n' | wc -c`
(110/110 for `DungeonMaster.App.csproj`, 28/28 for `DungeonMaster.Tests.csproj`) — never with grep.

---

## Q1 — Does the App project build on a plain `net10.0` TFM?

**Yes, after three csproj lines and one new 26-line file.** Out of the box it fails with two errors, then one more.

The change, exactly as the brief proposed:

```xml
<TargetFrameworks>net10.0;net10.0-windows10.0.19041.0;net10.0-maccatalyst</TargetFrameworks>
<OutputType Condition="'$(TargetFramework)' != 'net10.0'">Exe</OutputType>
```

Command:

```
dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0 -v m
```

### Failure round 1 — 2 errors, both `CS8795`

```
Services\Platform\PlatformCanvasCursor.cs(32,31): error CS8795: Partial method
  'PlatformCanvasCursor.SetShape(object?, CanvasCursorShape)' must have an implementation part
  because it has accessibility modifiers.
Services\Platform\PlatformPointer.cs(27,31): error CS8795: Partial method
  'PlatformPointer.IsSecondaryButtonPressed()' must have an implementation part
  because it has accessibility modifiers.
    0 Warning(s)
    2 Error(s)
```

Both come from the same designed-in tripwire. `PlatformCanvasCursor.cs:14-17` says so in its own words:

> Both target frameworks supply an implementing declaration, which is what an extended partial method
> requires. Adding a third target framework without adding a third implementation is a compile error
> rather than a silent no-op, which is the point.

This is the seam working as documented, not a defect.

### Failure round 2 — 1 error, `CA1416`

After adding the third implementation:

```
MauiProgram.cs(42,3): error CA1416: This call site is reachable on all platforms.
  'AppBuilderExtensions.UseMauiCommunityToolkit(MauiAppBuilder, Action<Options>?)' is only supported on:
  'Android' 21.0 and later, 'iOS' 15.0 and later, 'maccatalyst' 15.0 and later,
  'Tizen' 6.5 and later, 'Windows' 10.0.17763 and later.
    0 Warning(s)
    1 Error(s)
```

One call site. `CommunityToolkit.Maui` ships a `net10.0` lib (as the brief said), but its members carry
`SupportedOSPlatform` attributes, so a platform-neutral TFM cannot call them unguarded. Under
`TreatWarningsAsErrors` this is an error.

### Green

Suppressing `CA1416` for the bare TFM only:

```xml
<NoWarn Condition="'$(TargetFramework)' == 'net10.0'">$(NoWarn);CA1416</NoWarn>
```

```
DungeonMaster.App -> ...\src\DungeonMaster.App\bin\Debug\net10.0\DungeonMaster.App.dll
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Finding.** The bare TFM builds. No failure was spurious — each reproduced deterministically and each was
resolved by a specific change rather than a retry.

---

## Q2 — Exactly what breaks, and how much guarding does it need?

**Not six WinUI guards, and not a structural rewrite. Three edited lines and one new file.**

| Failure | Kind | Fix | Files | Lines |
|---|---|---|---|---|
| `CS8795` ×2 | Missing implementing declaration for an extended partial method (the `Platforms/` seam) | New `Services/Platform/PlatformSeams.Unsupported.cs` under `#if !WINDOWS && !MACCATALYST` | 1 new | 26 |
| `CA1416` ×1 | Platform-annotated package member called from a platform-neutral TFM | `NoWarn` scoped to `'$(TargetFramework)' == 'net10.0'` | 0 (csproj only) | 1 |
| TFM + OutputType | The Microsoft-documented change itself | `DungeonMaster.App.csproj` | 1 modified | 2 |

Counts from `git diff --stat` and `wc -l` on the new file:

```
 src/DungeonMaster.App/DungeonMaster.App.csproj     |   5 +-      (3 insertions, 2 deletions)
 src/DungeonMaster.Tests/DungeonMaster.Tests.csproj |   2 +
 src/DungeonMaster.App/packages.lock.json           | 378 +++++    (0 deletions)
 src/DungeonMaster.Tests/packages.lock.json         | 168 +++++    (0 deletions)
```

`wc -l < src/DungeonMaster.App/Services/Platform/PlatformSeams.Unsupported.cs` → `26`.

**Zero `#if WINDOWS` guards were added to any existing source file.** No `SingleProject`/resizetizer asset
problem appeared. No missing package asset appeared. No XAML problem appeared.

### One trap that only shows up when you run the suite

My first version of the seam file used `_ = platformView;` to consume unused parameters. That made
`DungeonMaster.Tests.App.AsyncDiscipline.DiscardedTasksAreAbsentTests.AppSource_NoLineStatesADiscardedTask`
fail — the hygiene test scans App source text and bans the discard token:

```
Expected violations to be empty because G2 bans the discard token in this layer, but these lines state one:
  ...\PlatformSeams.Unsupported.cs:13: _ = platformView; | ...:14: _ = shape;
```

Replacing the discards with an empty body and a comment cleared it. Anyone adopting this must write the no-op
seam without a discard.

---

## Q3 — Does XAML work on the bare TFM?

**Yes, fully. This is not the "reflection over non-XAML types only" world — it is the real one.**

Generated files are not written to disk by default, so I re-ran the build emitting them into scratch:

```
dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0 -t:Rebuild \
  -p:EmitCompilerGeneratedFiles=true -p:CompilerGeneratedFilesOutputPath=<scratch>/gen-net10
```

Counting the output:

```
find <scratch>/gen-net10 -name "*.xsg.cs" | wc -l          -> 26
grep -l "InitializeComponent" <scratch>/gen-net10/Microsoft.Maui.Controls.SourceGen/*/*.xsg.cs | wc -l  -> 26
find src/DungeonMaster.App -name "*.xaml" -not -path "*/obj/*" -not -path "*/bin/*" | wc -l   -> 27
find src/DungeonMaster.App -name "*.xaml" ... -path "*Platforms*"  -> Platforms/Windows/App.xaml
```

27 XAML files exist; 1 is `Platforms/Windows/App.xaml`, which is correctly not compiled for the bare TFM.
**All 26 shared XAML files produced a `.xsg.cs`, and all 26 contain `InitializeComponent`.**

Generators that ran for `net10.0`: `Microsoft.Maui.Controls.SourceGen`, `Microsoft.Maui.Controls.BindingSourceGen`,
`CommunityToolkit.Maui.SourceGenerators`, `CommunityToolkit.Mvvm.SourceGenerators`.

**Finding.** The XAML source generator runs on the bare TFM, the generated partial classes compile, and
`InitializeComponent` exists. Q4 below shows it also *executes correctly at run time*.

---

## Q4 — Can `DungeonMaster.Tests` see and use App types?

Added to `DungeonMaster.Tests.csproj`:

```xml
<ProjectReference Include="..\DungeonMaster.App\DungeonMaster.App.csproj" />
```

### First: a real collision, 31 errors in 3 files

```
dotnet build src/DungeonMaster.Tests/DungeonMaster.Tests.csproj -v m
    31 Error(s)   — all CS0104
```

All of the form:

```
error CS0104: 'IDispatcher' is an ambiguous reference between
  'DungeonMaster.Application.Common.Dispatch.IDispatcher' and 'Microsoft.Maui.Dispatching.IDispatcher'
```

Distinct files affected (`... | grep "error CS0104" | sed 's/(.*//' | sort -u | wc -l` → **3**):

- `src/DungeonMaster.Tests/Application/Common/Dispatch/DispatcherTests.cs`
- `src/DungeonMaster.Tests/Application/Common/Dispatch/ApplicationServiceRegistrationTests.cs`
- `src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/CommandValidatorCoverageTests.cs`

Cause is *not* the project reference directly. The transitive `Microsoft.Maui.Controls` package injects
**18 MAUI global usings** into the referencing project. Read from the generated `GlobalUsings.g.cs`, which
gained `Microsoft.Maui`, `Microsoft.Maui.Controls`, `Microsoft.Maui.Dispatching`, `Microsoft.Maui.Graphics`,
and 14 more. Only `Microsoft.Maui.Dispatching` actually collided, with the project's own
`Dispatcher`/`IDispatcher`.

**One line fixes it**, with no source file touched:

```xml
<Using Remove="Microsoft.Maui.Dispatching" />
```

```
DungeonMaster.Tests -> ...\bin\Debug\net10.0\DungeonMaster.Tests.dll
Build succeeded.
    0 Warning(s)  0 Error(s)
```

### (a) Naming and calling a plain App class — **YES**

`Q4a_PlainAppClass_IsNameableAndCallable`: constructed `DungeonMaster.App.Converters.GreaterThanZeroConverter`,
called `Convert(5, typeof(bool), null, CultureInfo.InvariantCulture)`, asserted `true`. Passed.

### (b) Reading a `BindableProperty` by reflection — **YES.** This is the MASTER-109 answer.

`Q4b_BindableProperty_IsReadableByReflection` and `Q4b2_BindableProperty_NullDefault_IsReadable`, both passed:

```csharp
var field = typeof(DmCard).GetField("TitleProperty", BindingFlags.Public | BindingFlags.Static);
var property = (BindableProperty)field!.GetValue(null)!;
Assert.Equal("Title",         property.PropertyName);
Assert.Equal(typeof(string),  property.ReturnType);
Assert.Equal(typeof(DmCard),  property.DeclaringType);
Assert.Equal(string.Empty,    property.DefaultValue);
```

`SubtitleProperty` was checked separately because it is declared with a `null` default, proving `DefaultValue`
is genuinely read rather than coincidentally matching.

`PropertyName`, `ReturnType`, `DeclaringType` and `DefaultValue` are all readable from the live
`BindableProperty` object, with no MAUI host and no `Application` instance. Everything
`BindableProperty.Create(nameof(...), typeof(...), typeof(...), <default>)` states in source is available as
data. Regex-matching the source text is not necessary.

### (c) Constructing an `Element`-derived object — **YES, it just works**

`Q4c_ElementDerived_CanBeConstructed`: `new BoxView()` — **passed in 242 ms, no exception**.

No `TypeInitializationException`, no `COMException`. The hypothesis in the brief is confirmed by measurement:
on the platform-less TFM the WinUI handler assembly is not in play, and the `Element` static constructor that
walls off `net10.0-windows` does not fire.

### (d) Constructing a XAML-declared control — **YES, with two prerequisites**

This took three rounds and each round moved the wall, so the intermediate results matter.

**Round 1 — bare `new DmCard()` throws:**

```
Microsoft.Maui.Controls.Xaml.XamlParseException: Position 11:9.
  StaticResource not found for key DmStrokeHairline
   at Microsoft.Maui.Controls.Xaml.StaticResourceExtension.ProvideValue(IServiceProvider)
   at DungeonMaster.App.Controls.DmCard.InitializeComponent() in Controls_DmCard_DmCard.xaml.xsg.cs:line 192
   at DungeonMaster.App.Controls.DmCard..ctor() in DmCard.xaml.cs:line 98
```

Note *where* it threw: **inside the generated `InitializeComponent`, at line 192.** The generator's output ran.
The failure is a missing resource dictionary, because nothing had merged `Resources/Theme/*.xaml`.

**Round 2 — construct the `Application` first.** `Q4e_AppConstructs_AndMergesThemeDictionaries` **passed**:
`new DungeonMaster.App.App()` constructs on the plain TFM (its `#if WINDOWS` block is excluded),
`Application.Current` becomes non-null, and `Application.Current.Resources.TryGetValue("DmStrokeHairline", out _)`
returns true — all seven merged theme dictionaries load.

`Q4g_XamlPage_Constructs` **passed** in the same run: `new DungeonMaster.App.AppShell()` — a whole XAML Shell —
constructs.

`Q4f` still failed, but on a new and different wall:

```
System.InvalidOperationException: BindableObject was not instantiated on a thread with a dispatcher
  nor does the current application have a dispatcher.
   at Microsoft.Maui.Controls.DispatcherExtensions.FindDispatcher(BindableObject)
   at DungeonMaster.App.Behaviors.HoverOrLongPressBehavior...OnAttachedTo(View) in HoverOrLongPressBehavior.cs:90
   at DungeonMaster.App.Controls.DmCard.WireHover() in DmCard.xaml.cs:line 121
```

The XAML was fine by then; a *behaviour* attached by the control's own code-behind needs a dispatcher.

**Round 3 — supply a dispatcher.** A ~40-line fake (`IDispatcher`, `IDispatcherTimer`, `IDispatcherProvider`)
plus `DispatcherProvider.SetCurrent(...)`. `Q4h_XamlControl_ConstructsWithDispatcher` **passed**:

```
Passed!  - Failed: 0, Passed: 1, Skipped: 0, Total: 1, Duration: 261 ms - DungeonMaster.Tests.dll (net10.0)
```

It asserted all of: `new DmCard()` does not throw; `card.Content` is non-null (so `InitializeComponent`
populated the visual tree from `DmCard.xaml`); the `x:Name`d `TrailingPresenter` field exists and is assigned;
and `card.Title = "Spike"` round-trips through `card.GetValue(DmCard.TitleProperty)`.

**Finding.** A XAML-declared control constructs, its generated `InitializeComponent` runs and populates its
content and named fields, and its bindable properties work — on the plain `net10.0` TFM, with no MAUI host and
no WinUI. The two prerequisites are a constructed `App` (for the theme dictionaries) and a dispatcher provider.
Both are ordinary test-fixture work, not framework limits.

---

## Q5 — Does the change harm the shipping app?

No.

| Command | Result |
|---|---|
| `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -v m` | `Build succeeded. 0 Warning(s) 0 Error(s)` |
| `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-maccatalyst -v m` | `Build succeeded. 0 Warning(s) 0 Error(s)` |
| `dotnet build src/DungeonMaster.sln -v m` | `Build succeeded. 0 Warning(s) 0 Error(s)` |
| `dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` | `Passed! - Failed: 0, Passed: 640, Skipped: 0, Total: 640` |
| `dotnet test src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj` | `Passed! - Failed: 0, Passed: 77, Skipped: 0, Total: 77` |

Per assembly, matching the baselines exactly: **`DungeonMaster.Tests` 640 passed**, **`DungeonMaster.App.Tests`
77 passed**. No aggregate figure is quoted because none exists.

Build output is unchanged for the shipping app and correctly differentiated for the new TFM:

```
bin/Debug/net10.0-windows10.0.19041.0/win-x64/   ->  DungeonMaster.App.exe, .dll, .pri, .deps.json, .runtimeconfig.json
bin/Debug/net10.0/                               ->  DungeonMaster.App.dll only (no .exe, no .runtimeconfig.json)
```

The conditional `OutputType` does exactly what it claims: the Windows TFM still produces an executable, the bare
TFM produces a library.

`DungeonMaster.App.Tests` continued to resolve `DungeonMaster.App` to the **Windows** TFM
(`bin\Debug\net10.0-windows10.0.19041.0\win-x64\DungeonMaster.App.dll`), so adding a bare TFM did not
redirect the existing platform test project.

**`net10.0-maccatalyst` was built only.** No runtime claim is made for it (`ADR-001`).

---

## Q6 — Total cost

**Hand-written changes: 3 files, 5 edited lines, 1 new 26-line file.**

| File | Change |
|---|---|
| `src/DungeonMaster.App/DungeonMaster.App.csproj` | 3 lines: TFM list, conditional `OutputType`, TFM-scoped `NoWarn` for `CA1416` |
| `src/DungeonMaster.App/Services/Platform/PlatformSeams.Unsupported.cs` | **new**, 26 lines, `#if !WINDOWS && !MACCATALYST` |
| `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` | 2 lines: `ProjectReference` to App, `<Using Remove="Microsoft.Maui.Dispatching" />` |

**`packages.lock.json` moved, as predicted.** Both files, purely additively, with zero deletions:
`src/DungeonMaster.App/packages.lock.json` **+378 lines** (a whole new `"net10.0"` dependency section) and
`src/DungeonMaster.Tests/packages.lock.json` **+168 lines**. Counts from `git diff --stat`. Restore rewrites
these without being asked; they must be committed alongside.

**Shipping app output: unchanged.** See Q5.

Things a reader needs to know before adopting:

- The `Platforms/` partial-method seam is a hard tripwire. A third TFM needs a third implementation, and the
  no-op body must not use the discard token or `DiscardedTasksAreAbsentTests` fails.
- `CA1416` needs handling. `NoWarn` scoped to the bare TFM is the cheapest option but suppresses the rule
  project-wide for that TFM; a `#if`/`SupportedOSPlatform` guard at the one `MauiProgram.cs` call site is the
  narrower alternative and was not measured.
- Referencing App imports 18 MAUI global usings into the test project. One collided today. Another could
  collide later as either side grows.
- A useful test fixture needs `new App()` (theme dictionaries) and a dispatcher provider. Neither is provided
  by anything in the repo today; both are small.

---

## Cleanup

All four modified files were restored from byte copies in scratch and verified with `cmp` — `DungeonMaster.App.csproj`,
`DungeonMaster.Tests.csproj`, and both `packages.lock.json` files all reported OK. The new seam file and the
`src/DungeonMaster.Tests/Spike/` throwaway tests were deleted.

`git status --porcelain` in the worktree:

```
```

Empty. The solution was then rebuilt from the restored state — `Build succeeded. 0 Warning(s) 0 Error(s)` — and
`git status --porcelain` was still empty afterwards, confirming the restore did not leave a lock file that the
next restore would rewrite.

---

## What this costs and what it buys

**The plain `net10.0` TFM builds here.** Cost is three edited lines in `DungeonMaster.App.csproj` and one new
26-line file supplying no-op implementations for the two `Platforms/` partial-method seams. Not one existing
source file needed an `#if WINDOWS` guard — the six WinUI-referencing files are already guarded. The only
genuinely unexpected failure was `CA1416` on the single `UseMauiCommunityToolkit()` call, which the packages'
platform annotations make unreachable from a platform-neutral TFM; a TFM-scoped `NoWarn` clears it, and a
narrower per-call-site guard was not measured.

**XAML compiles on it, fully.** The `SourceGen` inflator ran for `net10.0` and produced all 26 `.xsg.cs` files
for the 26 shared XAML files, every one containing `InitializeComponent`. This is not the degraded
"reflection over non-XAML types only" world. Pages and XAML controls are reachable from a plain test project:
a XAML `ContentView` constructs, `InitializeComponent` executes and populates both `Content` and its `x:Name`d
fields, and `AppShell` constructs too — provided the fixture first constructs `new App()` (which merges the seven
theme dictionaries, without which XAML throws `StaticResourceExtension` failures) and registers a dispatcher
provider (without which behaviour attachment throws). Both are ordinary fixture work of a few dozen lines.

**Reflection over `BindableProperty` declarations works — this is a clean yes for MASTER-109.** From a plain
`net10.0` xUnit test, with no MAUI host and no `Application` instance, the static `BindableProperty` field reads
back with correct `PropertyName`, `ReturnType`, `DeclaringType` and `DefaultValue`, verified against both an
empty-string default and a `null` default. Regex-matching `BindableProperty.Create(nameof(...), typeof(...), ...)`
in source text can be replaced with real reflection over real types.

**The shipping app is unaffected.** Both platform TFMs and the whole solution build `0 Warning(s) 0 Error(s)`;
`DungeonMaster.Tests` runs 640 passed and `DungeonMaster.App.Tests` runs 77 passed, both matching baseline. The
Windows TFM still emits `DungeonMaster.App.exe` while the bare TFM emits a library only, and `App.Tests` still
resolves App to the Windows TFM. `net10.0-maccatalyst` was built only; no runtime claim is made for it.

**What only becomes visible once you try it**, and what would make this a bad idea:

1. Adding the reference imports **18 MAUI global usings** into `DungeonMaster.Tests` via the transitive
   `Microsoft.Maui.Controls` package. Today exactly one collides — `Microsoft.Maui.Dispatching` against the
   project's own `Dispatcher`/`IDispatcher` — producing **31 `CS0104` errors across 3 existing test files**,
   fixed by one `<Using Remove="..." />` line. But this is a standing namespace-pollution hazard: `Microsoft.Maui`,
   `Microsoft.Maui.Controls` and `Microsoft.Maui.Graphics` are now ambient in the unit test project, and any future
   `Application`, `Element`, `Color`, `Page`, or `View` type on either side collides silently until it doesn't compile.
   I hit this myself mid-spike when `Application.Current` bound to `DungeonMaster.Tests.Application`.
2. The `Platforms/` seam is a **deliberate compile-time tripwire**, documented as such in
   `PlatformCanvasCursor.cs`. Every future platform seam added to the app must now grow a third, platform-less
   implementation, or the bare TFM breaks. That is a permanent, recurring tax on a pattern the project uses.
3. The no-op seam body **cannot use the discard token** — `_ = param;` makes
   `DiscardedTasksAreAbsentTests.AppSource_NoLineStatesADiscardedTask` fail, since that hygiene test scans App
   source text. Found only by running the suite.
4. `packages.lock.json` grows by **546 lines across two files** (+378 App, +168 Tests), purely additive. Every
   package-graph change now resolves for a third TFM.
5. Build time: the App project now compiles a third time on every full build.
