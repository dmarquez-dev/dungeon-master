# Contract — the two new projects

Frozen at Gate 2. Session `2026-08-26-settle-the-app-boundaries` (sub-session 5a).
Authored by `T1`. Consumed by `T2`, `T3`, by sub-sessions 5b and 5c, and by Session 6.

Every property in section 2 and section 3 was verified in this session by building and running a
throwaway copy of the project outside the repository. Where a property is present only to defeat a
specific build failure, the failure it defeats is quoted. **Do not drop a property because it looks
redundant** — each one below was added in response to an observed error, in the order shown.

---

## 1. Verified baseline this session starts from

Re-run these before claiming a regression. All four were run by `dir-architect` on 2026-08-26 in the
worktree
`C:\Users\dmarq\Development\Repos\dungeon-master\.claude-worktrees\dir\2026-08-26-settle-the-app-boundaries`.

| Command (run from the worktree root) | Observed |
|---|---|
| `dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` | `Failed: 0, Passed: 469, Skipped: 0, Total: 469` |
| `dotnet build src/DungeonMaster.sln` | `Build succeeded. 0 Warning(s). 0 Error(s).` |
| `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 --no-incremental` | `Build succeeded. 0 Warning(s). 0 Error(s).` |
| `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-maccatalyst --no-incremental` | `Build succeeded. 0 Warning(s). 0 Error(s).` |

**Exact strings that have been got wrong in this repository before.** The solution is at
`src/DungeonMaster.sln`, **not** at the repository root. The target framework monikers as written in
`src/DungeonMaster.App/DungeonMaster.App.csproj:4` are `net10.0-windows10.0.19041.0` and
`net10.0-maccatalyst`. The forms `net10.0-windows10.0.19041` and `net10.0-maccatalyst26.5` are what
NuGet *resolves* those to and are what appear in `NU1201` messages; they are **not** valid `-f`
arguments and fail with `NETSDK1005`.

---

## 2. `DungeonMaster.App.Core` — a `net10.0` class library

Path: `src/DungeonMaster.App.Core/DungeonMaster.App.Core.csproj`
Root namespace: `DungeonMaster.App.Core`

```xml
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
	</PropertyGroup>

	<ItemGroup>
		<ProjectReference Include="..\DungeonMaster.Domain\DungeonMaster.Domain.csproj" />
		<ProjectReference Include="..\DungeonMaster.Application\DungeonMaster.Application.csproj" />
	</ItemGroup>

</Project>
```

**Why it references `DungeonMaster.Application`.** Exactly one file in the assembly is permitted to
name an Application type: the mapper declared in `contracts/app-core-presentation.cs`, which is the
designated translation point from the `GetDungeonDetail` query DTOs to the control kit's own shapes.
No other file in `DungeonMaster.App.Core` may name a type from `DungeonMaster.Application.Features.*`.
That restriction is the whole point of `MASTER-027`: if the presentation contracts themselves named
the DTOs, nothing would have changed.

**Why it must stay `net10.0` and must never gain a platform TFM.** `DungeonMaster.Tests` targets
`net10.0` and reaches `DungeonMaster.App.Core` by an ordinary project reference. Giving
`DungeonMaster.App.Core` a platform TFM would reproduce `XCUT-002` one project further out. It must
not reference `Microsoft.Maui.*` in any form.

**Reference direction after this session:**

- `DungeonMaster.App` → `DungeonMaster.App.Core` (new)
- `DungeonMaster.Tests` → `DungeonMaster.App.Core` (new)
- `DungeonMaster.App.Tests` → `DungeonMaster.App` (new)
- `DungeonMaster.App.Core` → `DungeonMaster.Application` → `DungeonMaster.Domain` (existing chain)

No existing reference is removed. `DungeonMaster.App` keeps its reference to
`DungeonMaster.Application`, because `IDispatcher` and the query DTOs are still consumed through it.

---

## 3. `DungeonMaster.App.Tests` — a Windows-only MAUI-referencing test project

Path: `src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj`
Root namespace: `DungeonMaster.App.Tests`

This project exists so that App code can be **executed** by a test rather than asserted about as
source text. It is the mechanism that `MASTER-006`, `MASTER-013`, `MASTER-055` and `MASTER-062`
(Session 6) and `MASTER-159` (sub-session 5c) are waiting on. **This session fixes none of those
five.** Building the mechanism is the deliverable; using it is not.

```xml
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
		<UseMaui>true</UseMaui>
		<SingleProject>false</SingleProject>
		<OutputType>Library</OutputType>
		<IsPackable>false</IsPackable>
		<WindowsPackageType>None</WindowsPackageType>
		<EnableMsixTooling>false</EnableMsixTooling>
		<DisableResizetizer>true</DisableResizetizer>
		<AppxGeneratePriEnabled>false</AppxGeneratePriEnabled>
	</PropertyGroup>

	<ItemGroup>
		<PackageReference Include="coverlet.collector" />
		<PackageReference Include="FluentAssertions" />
		<PackageReference Include="Microsoft.NET.Test.Sdk" />
		<PackageReference Include="NSubstitute" />
		<PackageReference Include="xunit" />
		<PackageReference Include="xunit.runner.visualstudio" />
	</ItemGroup>

	<ItemGroup>
		<Using Include="Xunit" />
	</ItemGroup>

	<ItemGroup>
		<ProjectReference Include="..\DungeonMaster.App\DungeonMaster.App.csproj" />
	</ItemGroup>

</Project>
```

Every package above is already declared in `Directory.Packages.props`, so no `PackageVersion` entry
is added by this session. Central Package Management forbids a `Version` attribute on the
`PackageReference` elements.

### 3.1 Why each non-obvious property is there — with the failure it defeats

Each failure below was observed in this session, in this order, against a copy of this project
referencing the real `DungeonMaster.App`.

| Property | Failure observed without it |
|---|---|
| `DisableResizetizer=true` | `Microsoft.Maui.Resizetizer.After.targets(633,9): error : One or more duplicate file names were detected.  All image output filenames must be unique: appicon`. The `ResizetizeCollectItems` target pulls `MauiIcon` out of every referenced project, and an icon carrying a `ForegroundFile` — this repository's does, `DungeonMaster.App.csproj:56` — is exported twice. |
| `AppxGeneratePriEnabled=false` | `PRI175: 0x80070003 - MakePri failed with error: The system cannot find the path specified.`, then `APPX0002: Task 'WinAppSdkExpandPriContent' failed. Could not find file '…\obj\Debug\net10.0-windows10.0.19041.0\DungeonMaster.App.pri.xml'`. The MSIX PRI-expansion target tries to expand the referenced app's `.pri` into *this* project's intermediate directory. |
| `EnableMsixTooling=false` | Setting it `true` did **not** avoid the PRI failure above. It is `false` because a test library is never packaged. |
| `SingleProject=false` | Not load-bearing on its own — `true` produced the same resizetizer failure — but a test library has no platform head, so `false` is the honest value. |
| `UseMaui=true` | Required. Without it the project cannot consume a `UseMaui` project reference's MAUI types. |

### 3.2 The target framework is single, and it is Windows

`<TargetFramework>` — singular — is `net10.0-windows10.0.19041.0`. **Do not multi-target this
project.** Two failures were observed in this session when `net10.0-maccatalyst` was added:

1. `error NU1201: Project DungeonMaster.App is not compatible with net10.0-maccatalyst26.0
   (.NETCoreApp,Version=v10.0). Project DungeonMaster.App supports: - net10.0-maccatalyst26.5
   (.NETCoreApp,Version=v10.0) - net10.0-windows10.0.19041 (.NETCoreApp,Version=v10.0)` — the App
   project's macCatalyst platform version resolves to 26.5, and a bare `net10.0-maccatalyst` in
   another project resolves to 26.0.
2. Pinning the platform version to 26.5 moved the failure to
   `RuntimeComponentManifest.targets(3,5): error : Empty ResolveFrameworkReference.RuntimePackPath
   while trying to read runtime components manifest.` This machine has no macCatalyst test runtime
   pack.

This is consistent with escalation **E1**, in which the user parked macCatalyst explicitly: the
target is build-verified and not run-verified, so there is nothing for a macCatalyst test host to
verify here. The absence of one is recorded in the ADR — see `contracts/maccatalyst-decision.md`.

### 3.3 What a test in this project may and may not assume

- It runs **outside a MAUI application host**. `MauiProgram.CreateMauiApp()` is not called; there is
  no `Application.Current`, no window, no handler registry and no MAUI `IDispatcher`.
- Therefore types whose construction requires a handler — anything deriving from `ContentView`,
  `ContentPage` or `Shell`, which is every control under `src/DungeonMaster.App/Controls/` — are
  **out of reach** in this project as it is contracted here. No criterion and no test may assume
  otherwise.
- **In reach, and demonstrated in this session:** MAUI-typed non-visual code. `FloorMapDrawable` was
  constructed and `ScreenToCanvas(PointF)` executed and asserted against, passing, in a copy of this
  project.
- Whether the host-dependent half can be reached — by constructing a `MauiApp` in a fixture, or by a
  UI-thread test host — is **Session 6's question**. Nothing produced here may claim it has been
  answered.

### 3.4 Solution registration and lock files

- Both new projects are added to `src/DungeonMaster.sln` with `Debug|Any CPU` and `Release|Any CPU`
  entries matching the pattern the five existing projects use.
- `Directory.Build.props:23` sets `RestorePackagesWithLockFile=true`, so restoring each new project
  writes a `packages.lock.json` beside its `.csproj`. Both must be committed. Five lock files are
  committed today; a sixth and seventh that are not would silently unpin the new projects.
- `.editorconfig` declares `indent_style = tab` for `*.cs` and `*.csproj`, `end_of_line = crlf`,
  `charset = utf-8` and `insert_final_newline = true`, and `IDE0055` is at `warning` under
  `TreatWarningsAsErrors`. A space-indented or LF-terminated new file fails the build.
