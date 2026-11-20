# Contract — the plain `net10.0` TFM, the `DungeonMaster.App.UnitTests` project, and the XAML fixture

Producer: **T1**. Consumers: **T8, T9**, and every task later added to `DungeonMaster.App.UnitTests`.

This is the foundation the session's two heaviest entries stand on (`MASTER-100`/`TEST-014` and
`MASTER-109`/`TEST-021`). Everything in §1–§3 was measured in this worktree and reverted; the record
is `spike-plain-tfm-report.md`. Nothing here is a design guess except where it says so.

## 1. The third target framework

`src/DungeonMaster.App/DungeonMaster.App.csproj` gains a plain `net10.0` TFM. Three edited lines,
exactly as measured:

```xml
<TargetFrameworks>net10.0;net10.0-windows10.0.19041.0;net10.0-maccatalyst</TargetFrameworks>
<OutputType Condition="'$(TargetFramework)' != 'net10.0'">Exe</OutputType>
<NoWarn Condition="'$(TargetFramework)' == 'net10.0'">$(NoWarn);CA1416</NoWarn>
```

Guarantees T1 delivers and every consumer may rely on:

1. `net10.0-windows10.0.19041.0` still emits `DungeonMaster.App.exe`; `net10.0` emits a library only
   (no `.exe`, no `.runtimeconfig.json`). Measured.
2. `src/DungeonMaster.App.Tests` continues to resolve `DungeonMaster.App` to the **Windows** TFM
   (`bin\Debug\net10.0-windows10.0.19041.0\win-x64\DungeonMaster.App.dll`). Adding a bare TFM does
   not redirect it. Measured.
3. `MauiXamlInflator=SourceGen` runs for `net10.0`. All **26** shared `.xaml` files produce a
   `.xsg.cs` and every one contains `InitializeComponent`. (27 `.xaml` files exist; the 27th is
   `Platforms/Windows/App.xaml`, correctly not compiled for the bare TFM.) Measured, with the
   generated-file counts recorded in `spike-plain-tfm-report.md` §Q3.

### 1.1 `CA1416` — why `NoWarn` and what its cost is

The bare TFM's only genuinely unexpected failure was one `CA1416` at `MauiProgram.cs(42,3)`, on the
single `UseMauiCommunityToolkit()` call: `CommunityToolkit.Maui` ships a `net10.0` lib but its
members carry `SupportedOSPlatform` attributes, so a platform-neutral TFM cannot call them unguarded,
and `TreatWarningsAsErrors` turns that into an error.

`NoWarn` scoped to `'$(TargetFramework)' == 'net10.0'` clears it and was measured green. **Its cost
is that `CA1416` is suppressed project-wide for that TFM**, so a future unguarded platform call on
the bare TFM will not be reported. The narrower alternative — an `#if`/`SupportedOSPlatform` guard at
the one `MauiProgram.cs` call site — **was not measured**. T1 may take either; if it takes the
narrower one it must report the measurement, because the spike did not.

## 2. The platform-less seam — a permanent tax, stated so nobody rediscovers it

`src/DungeonMaster.App/Services/Platform/` declares two extended partial methods:

- `PlatformCanvasCursor.SetShape(object? platformView, CanvasCursorShape shape)`
- `PlatformPointer.IsSecondaryButtonPressed()`

Implementing declarations live under `Platforms/Windows/` and `Platforms/MacCatalyst/`, one compiled
per TFM. A third TFM with no third implementation is `CS8795` ×2 — and that is **the seam working as
designed**, not a defect. `PlatformCanvasCursor.cs` says so in its own words:

> Adding a third target framework without adding a third implementation is a compile error rather
> than a silent no-op, which is the point.

T1 adds `src/DungeonMaster.App/Services/Platform/PlatformSeams.Unsupported.cs`, guarded
`#if !WINDOWS && !MACCATALYST`, supplying no-op bodies for both.

**Two rules, both binding:**

1. **The no-op body must not use the discard token.** `_ = platformView;` makes
   `DungeonMaster.Tests.App.AsyncDiscipline.DiscardedTasksAreAbsentTests.AppSource_NoLineStatesADiscardedTask`
   fail — that hygiene test scans App source text and bans the token. Measured; found only by running
   the suite. An empty body with an explaining comment is the shape that passed.
2. **Every future `Platforms/` seam now needs a third body.** This is a permanent, recurring tax on a
   pattern the app uses, and the bare TFM breaks the moment somebody forgets. T1 records it in the
   XML doc comment on `PlatformSeams.Unsupported.cs` — in the file a future author will be editing,
   not only here.

`PlatformPointer.IsSecondaryButtonPressed()` documents `false` as "the answer that leaves the
caller's behaviour unchanged"; the platform-less body returns `false` for that stated reason.
`SetShape` documents that an unrecognised view "leaves the cursor alone"; the platform-less body does
nothing, for that stated reason. Neither body throws. A seam that throws on the bare TFM would make
every future App.UnitTests test that happens to reach it fail for an unrelated reason.

**Zero `#if WINDOWS` guards are added to any existing source file.** The six WinUI-referencing files
outside `Platforms/` are already guarded; the brief that preceded the spike anticipated the wrong
problem and the spike said so. A task adding such a guard has found something the spike did not and
must report it rather than absorb it.

## 3. `DungeonMaster.App.UnitTests` — the new project

Path: `src/DungeonMaster.App.UnitTests/DungeonMaster.App.UnitTests.csproj`
Root namespace: `DungeonMaster.App.UnitTests`

Shape, mirroring `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` plus one project reference:

```xml
<TargetFramework>net10.0</TargetFramework>
<IsPackable>false</IsPackable>
<!-- packages: coverlet.collector, FluentAssertions, Microsoft.NET.Test.Sdk, NSubstitute,
     xunit, xunit.runner.visualstudio — every one already versioned in Directory.Packages.props -->
<Using Include="Xunit" />
<ProjectReference Include="..\DungeonMaster.App\DungeonMaster.App.csproj" />
```

It sets **no** `UseMaui`. `DungeonMaster.Tests` with the same shape plus that reference is exactly
what the spike ran every Q4 measurement in, so this is the measured configuration and not an
extrapolation from it.

Guarantees:

1. The project resolves `DungeonMaster.App` to the **`net10.0`** TFM. This is what makes
   `Element` construction work: on a platform-less TFM the WinUI handler assembly is not in play and
   the `Element` static constructor that walls off `net10.0-windows` does not fire.
   (`new BoxView()` passed in 242 ms with no exception. Measured.)
2. It gains the **18 MAUI global usings** that `Microsoft.Maui.Controls` injects transitively. That is
   accepted here and is the whole reason the project is separate. `Microsoft.Maui.Dispatching.IDispatcher`
   is therefore ambient in it — a test that also wants
   `DungeonMaster.Application.Common.Dispatch.IDispatcher` must qualify one of them. The fixture in §4
   uses the MAUI one.
3. It appears in `src/DungeonMaster.sln` (T1 adds the entry; see `build-file-ownership.md` §3).
4. It carries a `packages.lock.json`, committed with the csproj.

### 3.1 The invariant this project exists to protect

`src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` **must remain free of a `DungeonMaster.App`
reference and free of MAUI global usings**, and `src/DungeonMaster.Tests/**` must remain free of
`using Microsoft.Maui`. T1 carries a criterion for it. `build-file-ownership.md` §4 records why, with
the measured cost of the alternative.

## 4. `AppFixture` — the frozen fixture surface

Namespace: `DungeonMaster.App.UnitTests.Fixtures`
File: `src/DungeonMaster.App.UnitTests/Fixtures/AppFixture.cs` (T1 owns; T8 and T9 do not edit it)

```csharp
/// Ambient MAUI state a XAML-declared control needs in order to construct, with no MAUI host
/// and no WinUI. Constructed once per test collection.
public sealed class AppFixture : IDisposable
{
	/// The constructed DungeonMaster.App.App. Non-null for the fixture's whole lifetime.
	/// Application.Current is this instance while the fixture is alive.
	public Microsoft.Maui.Controls.Application Application { get; }

	/// Looks a merged theme resource up by key. Returns null when the key is not defined.
	/// Never throws for a missing key — absence is a fact a consumer reports, not an exception.
	public object? FindResource(string key);

	/// Invokes the factory with the ambient state guaranteed ready, translating a
	/// XamlParseException into a failure naming the key or element that could not be resolved.
	/// Returns exactly what the factory returned.
	public T Construct<T>(Func<T> factory) where T : Microsoft.Maui.Controls.Element;
}

public static class AppTestCollection
{
	public const string Name = "app-unit-tests";
}

[CollectionDefinition(AppTestCollection.Name)]
public sealed class AppTestCollectionDefinition : ICollectionFixture<AppFixture>;
```

### 4.1 What the fixture guarantees, and what each guarantee was measured against

| Guarantee | Measured as |
|---|---|
| `new DungeonMaster.App.App()` constructs on the bare TFM (its `#if WINDOWS` block is excluded) and `Application.Current` becomes non-null | spike `Q4e`, passed |
| All **seven** merged theme dictionaries load, so `Application.Current.Resources.TryGetValue("DmStrokeHairline", out _)` returns `true` | spike `Q4e`, passed. Without this, `new DmCard()` throws `XamlParseException: StaticResource not found for key DmStrokeHairline` from inside the generated `InitializeComponent` at line 192 — spike `Q4` round 1 |
| A dispatcher provider is registered, so a `View`'s attached behaviour can call `DispatcherExtensions.FindDispatcher` | spike `Q4h`, passed. Without it, `new DmCard()` throws `InvalidOperationException: BindableObject was not instantiated on a thread with a dispatcher nor does the current application have a dispatcher` from `HoverOrLongPressBehavior.OnAttachedTo` — spike `Q4` round 2 |
| With both, `new DmCard()` constructs, `Content` is non-null, the `x:Name`d `TrailingPresenter` field is assigned, and `Title` round-trips through `GetValue(DmCard.TitleProperty)` | spike `Q4h`, passed |
| `new AppShell()` — a whole XAML `Shell` — constructs | spike `Q4g`, passed |

The dispatcher is a fake: `IDispatcher`, `IDispatcherTimer` and `IDispatcherProvider`, roughly 40
lines, registered with `Microsoft.Maui.Dispatching.DispatcherProvider.SetCurrent(...)`. T1 owns its
implementation; it is not part of the frozen surface, and no consumer names its type.

### 4.1.1 A measured MAUI landmine: `ResourceDictionary` will not enumerate

**[ADDED E12, 2026-09-02. Measured by T9. Read this before writing any code that walks the live
merged theme resources.]**

**A `ResourceDictionary` loaded from a `Source` reports its entries through `Count` and `Keys` but
yields NOTHING from its `IEnumerable<KeyValuePair<string, object>>` enumerator.** On the first merged
theme dictionary in this application:

| probe | result |
|---|---|
| `Count` | **89** |
| `Keys.Count` | **89** |
| `foreach (var pair in dictionary)` | **0 iterations** |
| `MergedDictionaries.Count` | 0 |
| runtime type | `Microsoft.Maui.Controls.ResourceDictionary` |

**The remedy is `Keys` plus `TryGetValue`.** Never `foreach` a `ResourceDictionary` in this project.

Two things make this worth a contract section rather than a comment in one file:

1. **It fails silently and in the passing direction.** A `foreach` collects nothing, so every
   downstream assertion runs over an empty collection and reports green. That is the exact vacuity
   shape `anti-vacuity.md` exists for, arriving through a framework behaviour nobody would predict
   from the type's public surface — it implements `IEnumerable<KeyValuePair<string, object>>` and
   answers `Count` correctly while enumerating to nothing.
2. **It was caught by a guard fact, on the first run.** T9's first draft enumerated with `foreach`
   and collected nothing; its own non-emptiness guard went red immediately. That is a genuine red
   step, not a constructed one, and it is the clearest evidence this session has produced that the
   §2/§3 guard rules pay for themselves.

**A future task reaching for `foreach` here will silently examine nothing and its facts will pass.**
Any task that reads the live merged dictionaries — for a key list, a value sweep, or a parity check
— uses `Keys` and `TryGetValue`, and carries a non-emptiness guard on what it collected.

### 4.2 The collection rule

**Every test class in `DungeonMaster.App.UnitTests` that constructs or touches a
`Microsoft.Maui.Controls` type carries `[Collection(AppTestCollection.Name)]`.**

Reason, stated as judgment because it was not measured here: `Application.Current` and
`DispatcherProvider.Current` are process-global static state. Two classes racing to set them is the
same family of hazard as the one the parallelism spike *did* measure in `DungeonMaster.App.Tests`,
and its failure mode there was an intermittent green, which is the most expensive kind. The
collection makes the classes sequential, and the parallelism spike measured that joining a collection
costs 10–46 ms per test regardless of how many classes join.

**The `App.Tests` process-crash hazard does not apply here.** In `DungeonMaster.App.Tests`, a class
outside `[Collection("maui-host")]` that touches an `Element`-derived type before the WinUI host boots
crashes the whole test host process (measured, 3 runs, `Total tests: Unknown`, ~53 of 82 results
lost). That wall is the `Element` static constructor reaching WinUI's `FocusManager`, and on the
platform-less TFM it does not fire — `new BoxView()` just works. The collection rule above is about
shared mutable statics, not about that crash, and the two must not be conflated in any criterion.

### 4.3 Two things no criterion in this session may rest on

- **`MauiHostFixture.HandledExceptions` firing.** The parallelism spike measured that an exception
  thrown from a `Dispatcher.Dispatch` callback fail-fasts the test host process (3/3 runs) and never
  reaches the recorder. It still has no demonstrated capture path.
- **Observed macCatalyst behaviour.** `ADR-001` parks macCatalyst: it builds here and cannot be run
  here. A criterion may require `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f
  net10.0-maccatalyst` to succeed. It may not require anything about running it.

### 4.4 Quoting 6a's boundary sentence

Wherever a task quotes it, "nothing deriving from `Element` constructs without a started WinUI
`Application`" needs the qualifier **"in a cold `net10.0-windows` process"**. The wall is a
once-per-process type initializer, not a per-access apartment check: once the host has run it on its
STA thread, the same touch succeeds on the MTA xunit thread (81/81, three runs). The unqualified form
will mislead whoever later tries to explain why an out-of-collection test passed.

## 5. Commands

Per-TFM builds, one aggregate `0 Warning(s) 0 Error(s)` each — **there is no per-TFM pair in a
multi-TFM build's output**:

```
dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0 -v m
dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -v m
dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-maccatalyst -v m
dotnet build src/DungeonMaster.sln -v m
```

Per-assembly test runs — **no aggregate figure exists**:

```
dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj          -> baseline 640 passed
dotnet test src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj  -> baseline 77 passed
dotnet test src/DungeonMaster.App.UnitTests/DungeonMaster.App.UnitTests.csproj  -> new, third line
```
