# Contract — MSBuild properties, central package management, project files

**Producer:** T2. **Consumers:** T3.
**Frozen at Gate 2.** Depends on `repository-root-config.md`, which T1 has already landed.

T2 owns every MSBuild-evaluated file: `Directory.Build.props`, the new `Directory.Packages.props`,
all five `.csproj`, and the generated `packages.lock.json` files. It carries `MASTER-009` (the
`AnalysisLevel` half), `MASTER-078`, `MASTER-071`, `MASTER-043`, `MASTER-104`, `MASTER-129`, the
`RestorePackagesWithLockFile` half of `MASTER-072`, and the `.csproj` half of `MASTER-150`.

---

## 1. Ordering inside T2 — not optional

These edits interact, and doing them out of order produces failures that look like defects in the
wrong change. Perform them in this order, building after each:

1. `MASTER-129` — delete the duplicated properties from the five `.csproj`. Build must stay clean.
2. `MASTER-150` — strip the BOM from the four `.csproj` that carry one. Build must stay clean.
3. `MASTER-071` + `MASTER-043` + `MASTER-104` — introduce `Directory.Packages.props`, move all 16
   versions in, bump EF Core. Restore and build must be clean.
4. `MASTER-009` + `MASTER-078` — pin `AnalysisLevel`/`LangVersion`, raise `AnalysisMode`, enable
   `EnforceCodeStyleInBuild`, then work the diagnostics down to `suggestion` until the build is
   green again. (`.editorconfig` must already exist — see `editorconfig.md`; T2 authors it as part
   of this step.)
5. `RestorePackagesWithLockFile` — **last**. Enabling lock files before the version set has settled
   means fighting stale lock files through every step above.

## 2. `Directory.Build.props` — final content

```xml
<Project>
	<PropertyGroup>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
		<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
		<LangVersion>14.0</LangVersion>
	</PropertyGroup>

	<PropertyGroup>
		<!-- MASTER-009 / XCUT-003. Pinned numerically so the analyzer rule set moves when this
		     line moves, not when the installed SDK moves. global.json pins the SDK band; this
		     pins what that band's analyzers are allowed to demand. -->
		<AnalysisLevel>10.0</AnalysisLevel>
		<!-- MASTER-078 / XCUT-010. -->
		<AnalysisMode>Recommended</AnalysisMode>
		<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
	</PropertyGroup>

	<PropertyGroup>
		<!-- MASTER-072 / XCUT-009. -->
		<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
	</PropertyGroup>
</Project>
```

Notes binding on the implementer:

- `LangVersion` moves from `latest` to `14.0`, the language version that ships with .NET 10, closing
  the third floating input `XCUT-003` names. If the build reports that a construct in `src/`
  requires a higher version, that is new information: do not raise the pin silently — report it.
- `EnableNETAnalyzers` is deliberately **not** set. `XCUT-010` establishes that analysis is already
  on by default for .NET 5+; setting it adds a line that asserts nothing.
- `NuGetAudit` is deliberately **not** set. `XCUT-009` establishes it is on by default and already
  running.
- Tab-indented, UTF-8 no BOM, CRLF, trailing newline. (It is space-indented today; T2 converts it,
  because `.editorconfig` now says tabs for `*.props` and T2 owns this file. This is the one build
  file whose whitespace T2 rather than T3 changes.)

## 3. `Directory.Packages.props` — new, exact shape

```xml
<Project>
	<PropertyGroup>
		<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
		<CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
	</PropertyGroup>

	<ItemGroup>
		<!-- one PackageVersion per row of the table below, alphabetical by Include -->
	</ItemGroup>
</Project>
```

Every entry is a `<PackageVersion Include="..." Version="..." />`. Ordering is alphabetical by
`Include`, in a single `ItemGroup`, so a reader can find a package without knowing which project
uses it.

### 3.1 The version table — this is the frozen part

Sixteen packages. (`XCUT-008` says "fourteen"; the actual count of `PackageReference` entries across
the four project files is sixteen. The discrepancy is in the audit, not here.)

| Package | Version | Disposition |
|---|---|---|
| `CommunityToolkit.Maui` | `14.1.0` | hold — 15.0.0 is a major bump, not in any Session 1 entry |
| `CommunityToolkit.Mvvm` | `8.4.2` | hold |
| `coverlet.collector` | `6.0.4` | hold — 10.0.1 is a major bump |
| `FluentAssertions` | `8.9.0` | hold |
| `FluentValidation` | `12.1.1` | hold |
| `Microsoft.EntityFrameworkCore.Design` | `10.0.11` | **bump** from 10.0.7 — `MASTER-043` |
| `Microsoft.EntityFrameworkCore.Sqlite` | `10.0.11` | **bump** from 10.0.7 — `MASTER-043` |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | `10.0.11` | **bump** from 10.0.0 — forced, see §3.2 |
| `Microsoft.Extensions.Logging.Debug` | `10.0.11` | **bump** from 10.0.0 — forced, see §3.2 |
| `Microsoft.Maui.Controls` | `10.0.41` | hold — 10.0.90 is not in any Session 1 entry |
| `Microsoft.NET.Test.Sdk` | `18.5.1` | hold |
| `NSubstitute` | `5.3.0` | hold — 6.2.0 is a major bump |
| `Plugin.Maui.KeyListener` | `1.0.0-preview4` | hold — `MASTER-104`, see §3.3 |
| `SQLitePCLRaw.lib.e_sqlite3` | `2.1.12` | conditional — see §3.4 |
| `xunit` | `2.9.3` | hold |
| `xunit.runner.visualstudio` | `3.1.4` | hold — 4.0.0 is a major bump |

**Scope rule behind the "hold" column.** Only `MASTER-043` asks this session for a version bump, and
it names EF Core. Major-version bumps of the test and MAUI packages carry behaviour risk and appear
in no Session 1 entry, so taking them here would break the session's defining property — that no
`src/` behaviour changes. They stay where they are and remain visible for a later session now that
they are all in one file.

### 3.2 Why the two `Microsoft.Extensions.*` packages are not a scope violation

They are declared at `10.0.0` today. EF Core `10.0.11` resolves `Microsoft.Extensions.*` at
`10.0.11` transitively. Under `CentralPackageTransitivePinningEnabled`, a central version *lower*
than a transitively required one is a package downgrade and restore fails with **NU1605**. Raising
these two to `10.0.11` is therefore not a discretionary bump — it is what makes §3.1 restore at all,
and both are patch-level moves inside the same major version.

**General rule for any further NU1605 the implementer hits:** raise the central version to the
lowest version that satisfies the transitive requirement, never higher, and record each such raise
in the T2 report with the package that forced it. Do not resolve a downgrade by removing the entry.

### 3.3 `Plugin.Maui.KeyListener` — `MASTER-104`, decided

Held at `1.0.0-preview4`. No stable release exists; `dotnet package search --exact-match
--prerelease` returns only `preview1` through `preview4`. The entry carries a comment in the same
style as the `SQLitePCLRaw` pin — advisory-shaped: what the situation is, why the version is held,
and the condition under which it is removed:

```xml
		<!-- Held at a prerelease deliberately (MASTER-104 / XCUT-011). No stable release of
		     Plugin.Maui.KeyListener exists: the feed offers only 1.0.0-preview1..preview4.
		     MauiProgram.cs binds .UseKeyListener() to it. Move to the first stable 1.0.0 the
		     moment one ships. Whether to keep the dependency at all is a Session 5 question
		     (XCUT-011's replacement analysis), not this session's. -->
		<PackageVersion Include="Plugin.Maui.KeyListener" Version="1.0.0-preview4" />
```

Replacing or removing the package is explicitly **out of scope** for this session.

### 3.4 `SQLitePCLRaw.lib.e_sqlite3` — conditional, `MASTER-043` re-check

The existing three-line comment in `DungeonMaster.Infrastructure.csproj` states the removal
condition in its own words: *"Remove once EF ships a version that resolves 2.1.12 or later on its
own (10.0.10 still does not)."* `MASTER-043` requires that condition be re-tested against the new EF
version, not assumed.

Required procedure:

1. Move the pin and its comment into `Directory.Packages.props` unchanged, alongside the EF bump.
2. Temporarily remove **both** the `PackageReference` from `DungeonMaster.Infrastructure.csproj`
   **and** the `PackageVersion` from `Directory.Packages.props`, restore, and run
   `dotnet list src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj package
   --include-transitive` to read what EF `10.0.11` actually resolves.

   > **Amended 2026-08-17, after T2's review.** This step previously said to remove only the
   > `PackageReference`. That reading is meaningless under
   > `CentralPackageTransitivePinningEnabled=true` (§3): a surviving `PackageVersion` pins the
   > transitive dependency regardless of whether any project references it directly, so the
   > `--include-transitive` listing would report `2.1.12` because the central pin put it there —
   > not because EF resolved it. Both entries must be absent for the re-check to measure what
   > `MASTER-043` is asking about. Restore whichever entries step 4 requires afterwards.
3. **If it resolves `2.1.12` or later on its own:** delete the `PackageReference` and the
   `PackageVersion` entirely, and say so in the T2 report — the pin has served its purpose.
4. **If it still resolves `2.1.11` or earlier:** restore the `PackageReference`, keep the
   `PackageVersion` at `2.1.12`, and update only the parenthesis in the comment from
   `(10.0.10 still does not)` to `(10.0.11 still does not)`. The rest of the comment stands.

Either way the outcome is stated explicitly in the T2 report. A silent carry-forward is a failure of
this criterion.

### 3.5 Known risk — transitive pinning and the MAUI workload

`CentralPackageTransitivePinningEnabled` interacts badly with workload-provided packs in some MAUI
configurations. If the App project cannot be made to restore cleanly with it on, **do not silently
drop the property** — `XCUT-008` names it as the mechanism the hand-rolled `SQLitePCLRaw` pin is
substituting for. Escalate to the orchestrator with the restore error, offering the reduced form
(`ManagePackageVersionsCentrally` alone) as the ranked fallback.

## 4. `.csproj` shape after T2

### 4.1 Every project — `MASTER-129` / `XCUT-013`

Delete these two lines from all five project files. They restate `Directory.Build.props` verbatim
and are the "five shadows of one setting" the record describes:

```xml
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
```

`TreatWarningsAsErrors` and `LangVersion` are not re-declared anywhere and need no action.

**Non-goal, deliberately: lifting `TargetFramework` into `Directory.Build.props`.** `XCUT-013`
suggests it "leaving the App project's `TargetFrameworks` to override it". That override does not
work as stated. `Directory.Build.props` is imported before the project body, so a
`<TargetFramework>net10.0</TargetFramework>` there is already set when `DungeonMaster.App.csproj`
later declares `<TargetFrameworks>`; MSBuild then has both properties non-empty and the outer build
loop's behaviour depends on evaluation order rather than on intent. Expressing "default unless
multi-targeted" needs a `Directory.Build.targets` import-order trick that costs more than it
removes. And `net10.0` in four project files is a declaration, not a shadow of a props value — the
duplication `XCUT-013` is actually about is `Nullable` and `ImplicitUsings`, which §4.1 removes in
full. `TargetFramework` and `TargetFrameworks` stay exactly where they are.

Everything genuinely per-project — `IsPackable`, `UseMaui`, `OutputType`, `SingleProject`,
`RootNamespace`, the App's identity, icon, splash, font and asset item groups, all `ProjectReference`
entries, and every existing comment — is untouched.

### 4.2 Every `PackageReference` — `MASTER-071`

Strip the `Version` attribute; the `Include` stays. Metadata children stay. So
`DungeonMaster.Infrastructure.csproj` reads:

```xml
		<PackageReference Include="Microsoft.EntityFrameworkCore.Design">
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
			<PrivateAssets>all</PrivateAssets>
		</PackageReference>
		<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
```

A `Version` attribute surviving on any `PackageReference` under CPM is a restore error (NU1008), so
this is self-enforcing — but the reviewer should still confirm none was replaced by a
`VersionOverride`, which would reintroduce exactly the drift CPM removes.

### 4.3 `DungeonMaster.Tests.csproj` line 13

`<PackageReference Include="Microsoft.NET.Test.Sdk" ... />` currently sits at column zero between
five indented siblings (`XCUT-012` cites this). T2 indents it to match. The file also lacks a
trailing newline; T2 adds one.

### 4.4 Encoding

Per `repository-root-config.md` §6: T2 strips the BOM from the four `.csproj` that carry one
(Domain, Application, Infrastructure, Tests). App's `.csproj` and `Directory.Build.props` have none
already. All files stay CRLF with a trailing newline. All build files T2 writes are tab-indented.

## 5. `packages.lock.json`

`RestorePackagesWithLockFile=true` produces one `packages.lock.json` per project directory under
`src/`. These are committed — that is the whole point of the property (`XCUT-009`: "the resolved
graph is committed and reviewable"). T2 owns `src/**/packages.lock.json`.

The App project multi-targets, so its lock file carries a section per target framework. Both are
expected and both are committed.

Operational note for every later session, and for T3: once lock files exist, changing any version
requires `dotnet restore --force-evaluate`. A plain `dotnet restore` will refuse with NU1004 rather
than silently re-resolve — which is the intended behaviour.

## 6. What T3 may rely on

After T2 is signed off, T3 may assume, without re-verifying:

- `Directory.Build.props` has exactly the content in §2, including `EnforceCodeStyleInBuild=true`.
- `.editorconfig` exists with the sections `editorconfig.md` specifies, and
  `dotnet_diagnostic.IDE0055.severity` is set to `suggestion` inside the analyzer-backlog block.
- `dotnet build src/DungeonMaster.sln` completes at 0 warnings, 0 errors for both App target
  frameworks, and `dotnet test` reports 292 passed.
- No file under `src/` with a `.cs` extension has been modified by T2.
