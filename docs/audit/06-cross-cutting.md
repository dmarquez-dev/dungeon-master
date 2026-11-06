# Cross-cutting -- code quality audit

## Scope

This task audits everything that belongs to no single layer: the solution and build configuration,
the repository-level configuration files that exist and the ones that do not, and the *boundaries
between* the five layers as distinct from the layers themselves.

| Path or glob | Files | Lines |
|---|---|---|
| `Directory.Build.props` | 1 | 8 |
| `src/DungeonMaster.sln` | 1 | 90 |
| `src/DungeonMaster.Domain/DungeonMaster.Domain.csproj` | 1 | 9 |
| `src/DungeonMaster.Application/DungeonMaster.Application.csproj` | 1 | 18 |
| `src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj` | 1 | 26 |
| `src/DungeonMaster.App/DungeonMaster.App.csproj` | 1 | 104 |
| `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` | 1 | 29 |
| `src/DungeonMaster.App/Platforms/Windows/Package.appxmanifest` | 1 | 46 |
| `src/DungeonMaster.App/Properties/launchSettings.json` | 1 | 8 |
| `.gitignore` | 1 | 43 |
| **Total** | **10** | **381** |

Absent files searched for and confirmed missing across the whole repository (`find` at the base
commit, excluding `.git/`): `.editorconfig`, `global.json`, `nuget.config` / `NuGet.config`,
`Directory.Packages.props`, `Directory.Build.targets`, `.gitattributes`, `packages.lock.json`. Each
absence that carries a consequence is a record below, and every such record cites the file where the
configuration *would* belong -- `Directory.Build.props:1` or the relevant `.csproj` -- never a path
that does not exist.

Boundaries audited, as pairs rather than as layers: Domain to Application; Application to
Infrastructure; Application and Infrastructure to App; and the Tests project's reach into each of
the four. Where a per-layer audit reported a defect as belonging to a boundary rather than to its
own layer, that defect appears here as an `XCUT` record: `01-domain-application.md` handed over
`CORE-020` (Application-to-App), `02-infrastructure.md` handed over the App-side half of
`INFRA-018` and the Application-owned interface behind `INFRA-014`,
`03-app-shell-and-views.md` handed over the DI lifetime disagreement, the absent Tests-to-App
reference behind `UI-017`, the macCatalyst target and the package identity placeholders,
`04-app-controls-and-theme.md` handed over the App-to-Application DTO coupling, and `05-tests.md`
handed over the absent `DungeonMaster.App` project reference.

Not audited, with reasons:

- Source files under `src/` other than the two named above. `T1`-`T5` own them; this task reads
  them only as evidence for a boundary claim and cites them where it does.
- `.gitignore` was read to confirm that no configuration file is merely ignored rather than absent.
  Its 43 lines cover build output, IDE state, runtime `*.db` files, `packages/`, `.nuget/`, and
  built mockup output; none of the seven files named above matches any pattern in it, so each of
  those absences is real rather than an untracked file. It carries no record.
- Generated output under `bin/` and `obj/`. Build artefacts, not source.
- CI configuration. There is none; the absence is recorded by `TEST-032` in `05-tests.md`, which
  reached it from the coverlet reference, and is not duplicated here.

Files read: 10. Files cited in at least one record: 9. `.gitignore` is the one file read and not
cited; it is listed above as read and found sound.

## Method

Base commit: `00313ebd72f5c80698d6143e85c90770bbc61e4b` (`00313eb`), on the session branch cut from
`chore/repo-audit`.

`dm-conventions` documents read:

- `backend/dotnet-format.md` -- **read in full, all 794 lines**, before any style or formatting
  record was written. No record below contradicts a rule stated in that document; where this
  document reaches a conclusion the convention document does not state outright, `XCUT-012` says so
  explicitly and shows the working.
- `backend/backend-conventions.md` -- 251 lines, read in full. Sections 1 (`Layer Organization`,
  `Layer Dependencies`) and 7 (`Dependency Injection`) are the load-bearing ones here.
- `architecture/architecture-decisions.md` -- read for the ADR format referenced in
  `XCUT-004`'s suggested direction. No record cites it as authority.

Microsoft Learn pages consulted, all fetched during this audit:

- https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview
- https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management
- https://learn.microsoft.com/en-us/nuget/concepts/security-best-practices
- https://learn.microsoft.com/en-us/dotnet/core/tools/global-json
- https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

Tooling run, all read-only:

- `find` over the repository for each absent configuration file named in `## Scope`.
- `git log`, `git rev-parse`, `git status --porcelain` for the base commit and the attestation.
- A byte-order-mark and line-ending check (`od -An -tx1` on the first three bytes; `file`) across
  the seven build files.
- A leading-whitespace census over every `.cs` file under `src/`, excluding `bin/` and `obj/`,
  classifying each file as tab-indented, space-indented, or containing no indented line. The
  per-project result is quoted in `XCUT-012`.
- A count of tab-led lines in `backend/dotnet-format.md` (299 of 794), quoted in `XCUT-012`.

No file was built, formatted, or modified.

## Summary

| Severity | Count |
|---|---|
| S1 | 0 |
| S2 | 4 |
| S3 | 8 |
| S4 | 4 |
| S5 | 1 |
| **Total** | **17** |

## Records

### XCUT-001 -- The composition root is split across two projects and neither of them opens a service scope

```yaml
id: XCUT-001
layer: XCUT
severity: S2
category: dependency
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
locations:
  - src/DungeonMaster.Infrastructure/DependencyInjection.cs:30-34
  - src/DungeonMaster.Infrastructure/DependencyInjection.cs:45-51
  - src/DungeonMaster.App/MauiProgram.cs:123-131
  - src/DungeonMaster.App/MauiProgram.cs:137-141
effort: M
risk: medium
independent: true
```

**What.** `AddInfrastructureServices` registers `AppDbContext` (scoped by `AddDbContext`'s default)
and both repositories `AddScoped`. `MauiProgram` registers all three view models and all three
page/popup types `AddTransient` and then resolves them through Shell and through an injected
`IServiceProvider`. A repository-wide search for `CreateScope` or `CreateAsyncScope` over `src/`
returns exactly one hit outside build output: `DependencyInjection.cs:47`, inside `MigrateDatabase`,
which opens a scope, migrates, and disposes it before the first page is ever resolved. No scope
exists at any point where a view model, handler or repository is resolved. Neither project states a
lifetime contract for the other, and neither one is wrong on its own terms.

**Why it matters.** The cited page names this shape as the "Scoped service as singleton"
anti-pattern -- "if you're not creating a scope or within an existing scope, the service becomes a
singleton" -- and states separately, under general `IDisposable` guidelines, "Don't resolve
`IDisposable` instances with a transient or scoped lifetime in the root scope", noting that
disposables resolved from the top-level container are held until the container is disposed. The
concrete cost is one `AppDbContext` for the process: a change tracker that never resets, identity
resolution that keeps returning first-loaded instances to later screens, and no protection against
two overlapping operations on one context. This is the boundary record for `INFRA-018` and `UI-001`,
which reached the same conclusion from the two sides; the defect belongs to neither project because
each registration is defensible in isolation and only the pair is wrong.

**Suggested direction.** Decide the lifetime contract once, in one place, and make both projects
state it. `AddDbContextFactory<AppDbContext>` with repositories taking `IDbContextFactory<T>` puts
the boundary at the operation and needs no host cooperation; a dispatcher-level `IServiceScope` per
command or query puts it at the dispatch and keeps the repositories unchanged. Whichever is chosen,
building the provider with scope validation enabled would have surfaced the mismatch at startup
rather than leaving it to be found by reading two files in two projects.

### XCUT-002 -- Tests targets net10.0 and App targets platform frameworks, so no test can reference App

```yaml
id: XCUT-002
layer: XCUT
severity: S2
category: boundary
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:4
  - src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:23-27
  - src/DungeonMaster.App/DungeonMaster.App.csproj:4
effort: M
risk: medium
independent: true
```

**What.** `DungeonMaster.Tests` declares `<TargetFramework>net10.0</TargetFramework>` and references
Domain, Application and Infrastructure. `DungeonMaster.App` declares
`<TargetFrameworks>net10.0-windows10.0.19041.0;net10.0-maccatalyst</TargetFrameworks>` and is
referenced by nothing. The thirteen files under `src/DungeonMaster.Tests/App/` therefore assert
about App code by reading `.xaml` and `.cs` off disk as text through `App/Theme/RepoPaths.cs`.

**Why it matters.** `03-app-shell-and-views.md` (`UI-017`) and `05-tests.md` (`TEST-004`,
`TEST-021`) both treat the missing reference as an omission that could be corrected by adding one.
I believe it cannot: a project targeting `net10.0` has no target framework in common with one
targeting `net10.0-windows10.0.19041.0` or `net10.0-maccatalyst`, so I believe adding the
`ProjectReference` would fail to restore rather than work. If that is right, then every consequence
downstream of it -- the regex-over-source sub-suite, the unreachable geometry and hit-test helpers in
`UI-017`, the lexical assertions in `TEST-021` -- is structural rather than a matter of discipline,
and the cheap fix those records imply is not available. That is the reason this is a boundary record
and not a Tests-layer one: the constraint lives in the pair of target frameworks, in two project
files neither audit owned.

**Suggested direction.** Two shapes resolve it and they are not equivalent. Extract the code worth
testing -- the outline translation, the two hit tests, the tile arithmetic -- into a `net10.0`
library the Tests project can already reach, which is `DungeonMaster.Application` or a new
`DungeonMaster.App.Core`; this is cheap and covers `UI-017` but nothing XAML-shaped. Or multi-target
the Tests project to the App's frameworks and run it on Windows only, which reaches everything and
costs a second test host, a longer build, and a suite that cannot run on a machine without the MAUI
workload. I believe the first is the better trade at this size, and that the second is what
`TEST-021`'s Roslyn suggestion is really an argument against.

### XCUT-003 -- No global.json and no AnalysisLevel pin, so the analyzer set floats under TreatWarningsAsErrors

```yaml
id: XCUT-003
layer: XCUT
severity: S2
category: dependency
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/tools/global-json
locations:
  - Directory.Build.props:1
  - Directory.Build.props:5
  - Directory.Build.props:6
effort: XS
risk: low
independent: true
```

**What.** `Directory.Build.props` sets `TreatWarningsAsErrors=true` and `LangVersion=latest`. No
`global.json` exists anywhere in the repository, and no project sets `AnalysisLevel`. The .NET
source-code-analysis documentation states that `AnalysisLevel` defaults to `latest`, so "you always
get the latest code analysis rules as you move to newer versions of the .NET SDK".

**Why it matters.** The cited page states that when no `global.json` is found "the highest installed
SDK version is used", and that outside Visual Studio "prerelease versions are considered". Three
floating inputs therefore compound: the SDK version, the analyzer rule set that SDK ships, and
`LangVersion=latest`. Any newly enabled rule arrives as a warning, and `TreatWarningsAsErrors=true`
turns it into a build error. The build is clean today at 0 warnings, which means there is no margin:
one rule added by an SDK update breaks the build on a machine where not one repository file changed,
and the same repository can build differently on two developers' machines. The cited page's own
advice is that "in CI scenarios ... you typically want to specify an acceptable range for the SDK
version".

**Suggested direction.** Add a `global.json` pinning the SDK feature band with `rollForward` set to
`latestPatch` or `latestFeature`, and set `AnalysisLevel` to the current numeric version in
`Directory.Build.props` so the rule set moves when the team moves it. Pinning `LangVersion` to the
same major version rather than `latest` closes the third input at no cost.

### XCUT-004 -- macCatalyst is a shipped target framework with no solution platform, run profile or input

```yaml
id: XCUT-004
layer: XCUT
severity: S2
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/DungeonMaster.App.csproj:4
  - src/DungeonMaster.App/DungeonMaster.App.csproj:50
  - src/DungeonMaster.App/Properties/launchSettings.json:2-7
  - src/DungeonMaster.sln:17-24
effort: L
risk: high
independent: true
```

**What.** `net10.0-maccatalyst` is a declared target framework with `SupportedOSPlatformVersion`
15.0. Around it: `launchSettings.json` declares one profile, "Windows Machine"; the solution's
platform list is `Any CPU`, `x64` and `x86` with no Mac entry; `MauiProgram.cs:75-83` and `102-112`
carry `#elif MACCATALYST` handler branches; and `UI-021` reports macCatalyst input handling declared
unimplemented in five places. Nothing in the repository builds, runs or tests this target.

**Why it matters.** I believe a target framework in `TargetFrameworks` is a shipping commitment, and
that this one is not being kept. The costs are real and separate from whether anyone wants a Mac
build. Every `#if WINDOWS` / `#elif MACCATALYST` pair is maintained twice and verified once, so the
Mac half rots silently; the `TreatWarningsAsErrors=true` guarantee that makes this session's other
records trustworthy has only been demonstrated for the Windows target, since nothing here can build
the Mac one; and a reader of `UI-021` cannot tell whether five unimplemented input paths are a bug
backlog or dead weight, because the repository does not say whether the target is real. I believe
the ambiguity is the defect, more than either resolution of it would be.

**Suggested direction.** Decide, and write the decision down -- an ADR in the format
`architecture/architecture-decisions.md` prescribes is the natural home. If macCatalyst ships, it
needs a run profile, a solution platform, and `UI-021`'s five paths implemented; if it does not,
removing the target framework deletes those five paths, the `#elif MACCATALYST` branches, and
`UI-036` along with it. Both resolutions close the same set of records, which is why they are one
piece of work and not several.

### XCUT-005 -- The Application assembly's public surface is a dispatcher contract, a DTO set and a utility library

```yaml
id: XCUT-005
layer: XCUT
severity: S3
category: boundary
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Map/TileCoordinateHelper.cs:9-52
  - src/DungeonMaster.Application/Common/Map/MapDrawingSession.cs:30-38
  - src/DungeonMaster.Application/Common/Map/SnapZoneCalculator.cs:8-33
  - src/DungeonMaster.App/DungeonMaster.App.csproj:100
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:1-9
effort: M
risk: low
independent: true
```

**What.** `DungeonMaster.App` references `DungeonMaster.Application` and consumes it in three
different ways. Through `IDispatcher`, which is the intended contract. Through the query DTOs, which
`UIKIT-005` reports as the declared types of three control kit bindable properties. And through
`Common/Map`, whose three types `CORE-020` reports as having no caller inside Application at all --
their production callers are `DungeonDetailViewModel`, `FloorMapDrawable` and
`DungeonDetailPage.xaml.cs`. `MapDrawingSession` is mutable and the view model holds one as a
property.

**Why it matters.** The dependency direction is correct at all three sites -- App depends on
Application, never the reverse -- so this is a surface-area finding, not a direction one. I believe
the cost is that the boundary cannot be reasoned about: there is no statement anywhere of what App
is permitted to touch, so the seam is whatever is `public`, and three unrelated kinds of coupling
have accumulated behind one project reference. `CORE-020` and `UIKIT-005` each recorded one instance
from inside their own layer; neither could see that the pattern is the boundary rather than two
local slips. I also believe the stateful `MapDrawingSession` is the sharpest instance: an object
whose lifetime is a user's in-progress gesture is living in a layer whose every other type is a
stateless per-dispatch service.

**Suggested direction.** State what the Application layer's public surface is -- commands, queries,
their DTOs, the dispatcher, and the ports -- and then move what is not that. `Common/Map` is either
application-layer behaviour, in which case a command should consume it and the App should reach it
through the dispatcher, or it is presentation arithmetic, in which case it belongs in a project the
App owns. `XCUT-002` constrains that choice, because a helper moved into `DungeonMaster.App` becomes
untestable in the same move.

### XCUT-006 -- Persistence ports are declared inside an Application feature folder rather than at a port surface

```yaml
id: XCUT-006
layer: XCUT
severity: S3
category: boundary
authority: conventions
citation: backend/backend-conventions.md#Layer Organization
locations:
  - src/DungeonMaster.Application/Features/Dungeons/IDungeonReadRepository.cs:1
  - src/DungeonMaster.Application/Features/Dungeons/IDungeonWriteRepository.cs:1
  - src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs:32
  - src/DungeonMaster.Infrastructure/DependencyInjection.cs:33-34
effort: S
risk: low
independent: true
```

**What.** `IDungeonReadRepository` and `IDungeonWriteRepository` are declared in
`Application/Features/Dungeons/`, alongside that feature's commands and queries. The layout in the
cited section places application service interfaces at `Application/Common/Interfaces/` and reserves
`Features/{Feature}/` for commands, queries, handlers and validators.

**Why it matters.** The dependency inversion is correct -- Infrastructure implements interfaces
Application owns, so the arrow points inward -- but the port has no address. `02-infrastructure.md`
records this from the other side: `INFRA-014`'s fix, moving the transaction boundary out of the
repository, "touches `IDungeonWriteRepository`, which lives in Application", so a persistence-driven
change edits a file sitting inside a feature folder. The practical cost is that a second feature
needing persistence has no obvious place to put its port, and that the one seam between two
assemblies is not visible as a seam to anyone reading either directory.

**Suggested direction.** Move both interfaces to `Application/Common/Interfaces/` as the cited
section lays out. This is the same move `CORE-029` proposes from inside the Application layer; the
boundary reason and the layout reason are one edit, and doing it before `INFRA-014` gives that fix a
place to land.

### XCUT-007 -- Domain CLR type choices set Infrastructure's storage cost with no port between the two

```yaml
id: XCUT-007
layer: XCUT
severity: S3
category: boundary
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/DungeonConfiguration.cs:43-49
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/FloorConfiguration.cs:50-56
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/AreaConfiguration.cs:51-57
  - src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs:38-42
effort: M
risk: medium
independent: true
```

**What.** The Domain chooses `DateTimeOffset` for `CreatedAt` and `LastModified` on all four entity
types, and the Infrastructure configurations map them with no value converter. `INFRA-003` reports
that the SQLite provider does not natively support the type and rates the fix `L` / `high`. The same
shape appears in `INFRA-008`, where the CLR default `0f` for `Scale` collides with a store default,
and `INFRA-002`, where eleven `HasMaxLength` calls describe constraints SQLite does not enforce.

**Why it matters.** The Domain has no reference to Infrastructure, which is right, and it therefore
has no way of knowing which provider it is being stored in -- which is also right. What is missing is
anything in between: no port, no documented persistence contract, no test that fails when a Domain
type choice becomes expensive downstream. I believe that is why `INFRA-003` is rated `L` / `high`
rather than `S` / `low`: the fix is not in the layer that found it. A converter in the
configurations is Infrastructure-local but leaves the Domain's chosen type as the one clients see;
changing to `DateTime` is a Domain edit that touches every entity and every test that constructs
one.

**Suggested direction.** I believe the cheapest durable answer is to write down that SQLite is the
persistence target and which CLR types are cheap in it, next to the entities rather than in the
configurations, so the next type choice is made with that information. The converter-versus-Domain
decision then belongs to `INFRA-003` and should be taken once for all four timestamp columns rather
than per configuration file.

### XCUT-008 -- Package versions and the security pin are declared per project with no central management

```yaml
id: XCUT-008
layer: XCUT
severity: S3
category: dependency
authority: microsoft
citation: https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management
locations:
  - src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj:14-17
  - src/DungeonMaster.Application/DungeonMaster.Application.csproj:7-10
  - src/DungeonMaster.App/DungeonMaster.App.csproj:91-97
  - src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:10-17
  - Directory.Build.props:1
effort: S
risk: low
independent: true
```

**What.** Fourteen `PackageReference` entries carry inline `Version` attributes across four project
files. No `Directory.Packages.props` exists and `ManagePackageVersionsCentrally` is not set. Among
those fourteen is the deliberate pin
`<PackageReference Include="SQLitePCLRaw.lib.e_sqlite3" Version="2.1.12" />`, added as a top-level
reference in the Infrastructure project solely to lift a transitive dependency past
GHSA-2m69-gcr7-jv3q, and carrying a three-line comment that states the advisory, the patched
version, and the condition for removal.

**Why it matters.** The pin itself is correct and unusually well documented -- it does exactly what
the NuGet supply-chain guidance asks, which is to take action promptly on a known vulnerability, and
it records why so the next reader can decide when to remove it. The problem is where it lives. It
appears in the Infrastructure project as an ordinary dependency, indistinguishable in shape from
`Microsoft.EntityFrameworkCore.Sqlite` next to it, so nothing at the repository root shows that a
security decision is being held in place. The cited page describes exactly this case:
`CentralPackageTransitivePinningEnabled` "promotes a transitive dependency to a top-level dependency
implicitly on your behalf when necessary", which is the mechanism this pin is hand-rolling. The
version-drift cost is the ordinary one -- `Microsoft.Extensions.*` at 10.0.0 is declared in two
projects today and nothing keeps them together.

**Suggested direction.** Add a `Directory.Packages.props` with `ManagePackageVersionsCentrally` and
`CentralPackageTransitivePinningEnabled` set, move all fourteen versions into it, and keep the
existing comment on the `SQLitePCLRaw` entry so the reason survives the move. The pin then reads as
what it is: a repository-level security decision.

### XCUT-009 -- No nuget.config, no package source mapping and no lock file at the repository root

```yaml
id: XCUT-009
layer: XCUT
severity: S3
category: security
authority: microsoft
citation: https://learn.microsoft.com/en-us/nuget/concepts/security-best-practices
locations:
  - Directory.Build.props:1
  - src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj:14-17
effort: S
risk: low
independent: true
```

**What.** No `nuget.config` exists in the repository, so restore uses whatever sources the machine
or user profile supplies. `RestorePackagesWithLockFile` is not set and no `packages.lock.json` is
committed. `NuGetAudit` is not configured, though the cited page states it is on by default from
.NET 8 and warns on transitive packages by default from .NET 9, so auditing is running.

**Why it matters.** The cited page states that adding a `nuget.config` at the repository root "is
considered a best practice as it promotes repeatability and ensures that different users have the
same NuGet configuration", and recommends `<clear />` elements so no machine-specific source leaks
in; it names dependency confusion as the attack this defends against. Lock files it describes as
storing "the hash of your package's content", ensuring package repeatability. This repository has
already demonstrated that it takes advisories seriously -- `XCUT-008`'s pin exists because someone
acted on one -- which is what makes the gap worth recording: the response to a known vulnerability
is in place while the controls that would make the restore itself reproducible are not. Combined
with `XCUT-003`, nothing about a build here is pinned: not the SDK, not the analyzer set, not the
package graph.

**Suggested direction.** Add a root `nuget.config` with `<clear />` and nuget.org as the single
source, and enable `RestorePackagesWithLockFile` so the resolved graph is committed and reviewable.
The `global.json` from `XCUT-003` should use `rollForward: disable` if lock files are adopted, which
the cited SDK documentation notes is needed to keep the SDK and the dependency graph in step.

### XCUT-010 -- Analysis mode is left at Default and code-style rules are not enforced on build

```yaml
id: XCUT-010
layer: XCUT
severity: S3
category: dependency
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview
locations:
  - Directory.Build.props:2-7
  - src/DungeonMaster.Domain/DungeonMaster.Domain.csproj:3-7
effort: S
risk: medium
independent: true
```

**What.** No project sets `AnalysisMode`, `AnalysisLevel`, `EnforceCodeStyleInBuild` or
`EnableNETAnalyzers`, and no analyzer package is referenced anywhere. The cited page states that for
projects targeting .NET 5 or later "code analysis is enabled by default", so the CA analyzers are
running; that the default analysis mode is `Default`, in which "only a small number of rules are
enabled as build warnings"; and that code-style analysis is "*disabled*, by default, for all .NET
projects on command-line builds".

**Why it matters.** It would be wrong to say this repository has no analysis -- it has the twenty-odd
rules the .NET 10 SDK enables by default, and `TreatWarningsAsErrors=true` makes each of them a hard
stop, which is a genuinely strong baseline. What it has none of is anything above that floor. The
`CAxxxx` design, performance and usage rules that sit at suggestion severity would have flagged
several findings this session produced by hand: disposal ownership in `UIKIT-001`, `async void` in
`UI-013`, the unused private members behind `CORE-019`. The `IDExxxx` rules are not running at all on
a command-line build, which is why every style record in this session -- braces in `UI-028` and
`UIKIT-021`, indentation in `XCUT-012`, documentation in `CORE-025` -- had to be found by a person
reading files. Style rules that only a reviewer enforces are style rules that drift between reviews.

**Suggested direction.** Set `AnalysisMode` to `Recommended` and `EnforceCodeStyleInBuild` to `true`
in `Directory.Build.props`, together with the `AnalysisLevel` pin from `XCUT-003` so the escalation
happens once rather than on every SDK update. Expect a first run to produce a large number of
diagnostics under `TreatWarningsAsErrors`; the usual approach is to raise the mode and drop
individual rules to `suggestion` in the `.editorconfig` from `XCUT-012` until the backlog is worked
off, rather than to raise the mode last.

### XCUT-011 -- A prerelease package is referenced by the only project that ships

```yaml
id: XCUT-011
layer: XCUT
severity: S3
category: dependency
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/DungeonMaster.App.csproj:96
  - src/DungeonMaster.App/MauiProgram.cs:2
  - src/DungeonMaster.App/MauiProgram.cs:29
effort: M
risk: medium
independent: true
```

**What.** `Plugin.Maui.KeyListener` is referenced at version `1.0.0-preview4` and wired in through
`.UseKeyListener()`. It is the only prerelease dependency in the solution, and the only third-party
package outside the CommunityToolkit and Microsoft sets. The other four packages in the App project
are stable releases.

**Why it matters.** I believe a `-preview4` package in the one project that ships carries a class of
risk the other thirteen references do not: no compatibility commitment between previews, no
guarantee that a `1.0.0` final ever arrives, and no assurance the API surface `MauiProgram` binds to
survives. The functionality it provides is keyboard input, which `UI-021` and `XCUT-004` already
identify as the least-finished part of the App. I state this as belief because I did not evaluate
the package's release history or its maintenance status, and a preview that has been stable for a
long time is a different proposition from a fresh one.

**Suggested direction.** Check whether a stable release exists and move to it. If none does, the
question worth answering is how much of the keyboard handling the plugin is actually doing, and
whether the Windows-only subset this app currently uses is small enough to take directly against the
platform, which would remove the dependency and the macCatalyst gap in `XCUT-004` at the same time.

### XCUT-012 -- No .editorconfig, so the indent character dotnet-format.md denominates its rules in is unenforced

```yaml
id: XCUT-012
layer: XCUT
severity: S4
category: style
authority: conventions
citation: backend/dotnet-format.md#6. Inheritance, Interfaces, and Generic Constraints
locations:
  - Directory.Build.props:1
  - src/DungeonMaster.App/DungeonMaster.App.csproj:3
  - src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:3
  - src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:13
  - src/DungeonMaster.sln:17
effort: S
risk: low
independent: true
```

**What.** No `.editorconfig` exists in the repository. A census of every `.cs` file under `src/`,
excluding `bin/` and `obj/`, finds 109 tab-indented files, 62 space-indented and 7 with no indented
line, and no file mixing the two. The split is not random -- it is per project:

```text
DungeonMaster.App            tab= 35  space=  7  none= 0
DungeonMaster.Application    tab= 54  space=  1  none= 2
DungeonMaster.Domain         tab= 11  space=  4  none= 5
DungeonMaster.Infrastructure tab=  8  space=  1  none= 0
  (of which Migrations)      tab=  0  space=  5  none= 0
DungeonMaster.Tests          tab=  1  space= 44  none= 0
```

The build files disagree the same way: `DungeonMaster.App.csproj` and `DungeonMaster.sln` are
tab-indented, the other four `.csproj` and `Directory.Build.props` are space-indented, and
`DungeonMaster.Tests.csproj:13` carries a `PackageReference` at column zero between five indented
siblings.

**Why it matters.** `dotnet-format.md` states no *dedicated* indent-character rule, which is why
`CORE-027` and `TEST-024` both stopped at `judgment` and deferred the question here. It is not
silent, though. Every indentation instruction it gives is denominated in tabs: section 6 specifies
"+1 tab" for each successive element type and repeats that in all four rows of its summary table
(lines 240-247, 286-291); section 9 requires XML tag descriptions "indented with **two tabs** from
the `///` marker" (line 375); section 3's worked examples measure declaration layout the same way.
299 of the document's 794 lines are tab-led. An instruction to indent "+1 tab" is not executable
under a spaces regime without a conversion the document never supplies, so the convention document
does answer the question -- indirectly, and only once.

The counter-authority is real and should be stated. Microsoft's own C# coding conventions page says
"Use four spaces for indentation. Don't use tab characters." That page also states, of itself, that
its guidelines "are used by Microsoft to develop samples and documentation", that they were adopted
from the runtime and Roslyn repositories, and that they are "meant to be an example of common C#
conventions, and not an authoritative list", inviting teams to "modify them to suit your team's
needs". `dm-conventions` is that modification. So the session's Microsoft-first yardstick does not
decide this one: the Microsoft page in question declines to be the authority, and the project's own
document, which does claim that role, points to tabs. **My position is tabs**, and the practical
consequence is that the Tests project is the outlier at 44 files, not `ObjectScaleTests.cs` at one.
`TEST-024` reached this and correctly refused to convert the single file on majority alone.

**Suggested direction.** Add an `.editorconfig` at the repository root with `indent_style = tab` for
`[*.cs]` and for `[*.{csproj,props,targets,sln}]`, plus `charset` and `end_of_line`, and reformat the
space-indented minority in one commit that touches nothing else so the whitespace churn is
reviewable as whitespace churn. Generated migrations can be excluded with their own section rather
than reformatted. Once `EnforceCodeStyleInBuild` from `XCUT-010` is on, this file is what makes every
other style record in this session enforceable rather than advisory.

### XCUT-013 -- Directory.Build.props holds four properties that the five project files then re-declare

```yaml
id: XCUT-013
layer: XCUT
severity: S4
category: duplication
authority: judgment
citation: n/a
locations:
  - Directory.Build.props:3-6
  - src/DungeonMaster.Domain/DungeonMaster.Domain.csproj:4-6
  - src/DungeonMaster.Application/DungeonMaster.Application.csproj:13-15
  - src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj:21-23
  - src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:4-6
  - src/DungeonMaster.App/DungeonMaster.App.csproj:17-18
effort: XS
risk: low
independent: true
```

**What.** `Directory.Build.props` sets `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors` and
`LangVersion` for every project. All five project files then set `Nullable` and `ImplicitUsings`
again to the same values, and four of them declare `<TargetFramework>net10.0</TargetFramework>`
individually.

**Why it matters.** I believe the duplication is worse than merely redundant, because it is
misleading in a specific way: a reader looking at `DungeonMaster.Domain.csproj` sees `Nullable` set
locally and reasonably concludes that is where the setting lives, so a future change made there
silently disagrees with the four other projects rather than changing the solution. The `props` file
exists precisely so there is one place; leaving the per-project copies in means there are six, five
of which are shadows. `TargetFramework` is the same shape, with the wrinkle that the App project
genuinely differs and would need to keep its own `TargetFrameworks` line.

**Suggested direction.** Delete `Nullable` and `ImplicitUsings` from all five project files, and lift
`net10.0` into `Directory.Build.props` as the default `TargetFramework`, leaving the App project's
`TargetFrameworks` to override it. Everything that is genuinely per-project -- `IsPackable`,
`UseMaui`, `OutputType` -- stays where it is.

### XCUT-014 -- The solution declares x64 and x86 platforms that every project maps back to Any CPU

```yaml
id: XCUT-014
layer: XCUT
severity: S4
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.sln:17-24
  - src/DungeonMaster.sln:26-37
  - src/DungeonMaster.sln:62-73
effort: S
risk: low
independent: true
```

**What.** `SolutionConfigurationPlatforms` declares six configurations: Debug and Release across
`Any CPU`, `x64` and `x86`. In `ProjectConfigurationPlatforms`, every one of the thirty entries for
all five projects resolves to `Any CPU` -- for example
`{110CCDEA-...}.Debug|x64.ActiveCfg = Debug|Any CPU`. No project has an `x64` or `x86` build.

**Why it matters.** I believe the platform axis is inert: selecting `x64` in an IDE produces a build
identical to `Any CPU`, so the choice communicates something the solution does not deliver. That is
harmless while it stays inert and stops being harmless the moment someone needs a real architecture
split -- and this solution has a MAUI app whose actual publish identifiers are `win10-x64` and,
per the commented block at `DungeonMaster.App.csproj:6-11`, `maccatalyst-x64` and
`maccatalyst-arm64`, none of which the platform list represents. I also believe this is template
residue rather than a decision: the six-platform list is what the Visual Studio solution template
emits.

**Suggested direction.** Reduce the solution to `Any CPU` unless a real per-architecture build is
wanted, in which case the App project needs actual `x64`/`arm64` configurations behind those
entries. The `.NET 10` SDK also supports the `.slnx` format, which expresses the same thing in a
fraction of the ninety lines; that is a separate call and not required by this record.

### XCUT-015 -- App identity is template-shaped in the manifest and flagged as undecided in the project file

```yaml
id: XCUT-015
layer: XCUT
severity: S4
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Platforms/Windows/Package.appxmanifest:9
  - src/DungeonMaster.App/Platforms/Windows/Package.appxmanifest:13-17
  - src/DungeonMaster.App/DungeonMaster.App.csproj:29
  - src/DungeonMaster.App/DungeonMaster.App.csproj:31-45
effort: S
risk: medium
independent: true
```

**What.** `Package.appxmanifest` carries `Name="maui-package-name-placeholder"`,
`Publisher="CN=User Name"`, `Version="0.0.0.0"`, `<DisplayName>$placeholder$</DisplayName>`,
`<PublisherDisplayName>User Name</PublisherDisplayName>` and `<Logo>$placeholder$.png</Logo>`. In the
project file, `ApplicationTitle` is `DungeonMaster.App` -- the project name, not a product name --
and `ApplicationDisplayVersion` and `ApplicationVersion` are the template's `1.0` and `1`. The
`ApplicationId` has been deliberately changed away from the template default and carries a comment
stating, in the file's own capitals, that it "IS AN IDENTITY DECISION AND MAY WANT TO BE SOMETHING
ELSE".

**Why it matters.** This is one decision recorded in two files, half-taken. The comment already
states the cost better than I can: changing the identifier after an install makes the next build a
different application to Windows, with its own local app-data folder, and the SQLite database lives
there. `WindowsPackageType` is `None` today, so the manifest's placeholders are not currently
reaching a user -- which is exactly why this is cheap now and stops being cheap on the day packaging
is turned on. `UI-034` records the manifest half from inside the App layer; the project-file half and
the fact that the two must agree are why it sits here.

**Suggested direction.** Settle the shipping identity once -- product name, publisher, version scheme
-- and write it into both files in the same change, including the `ApplicationTitle` a user actually
sees. If the identity is genuinely undecided, the honest form is an ADR saying so with the migration
cost the comment already describes, rather than a placeholder that reads as finished.

**[AMENDED 20261106-0120-shipping-identity, 2026-11-06 (UTC; local 2026-11-05) -- this entry was acted
on in full as `MASTER-142`, together with `UI-034`: the user settled the shipping identity and BOTH halves now carry
it in the same change.** The decision: user-visible name `Dungeon Master`, publisher `CN=Dungeon Master`
with display name `Dungeon Master`, `ApplicationId` kept as `com.dmarquezdev.dungeonmaster`, and version
`0.1.0` (`ApplicationDisplayVersion`) with build `1` (`ApplicationVersion`).

*Project-file half.* `ApplicationTitle` is `Dungeon Master`, `ApplicationDisplayVersion` is `0.1.0` and
`ApplicationVersion` is `1`. The `ApplicationId` comment no longer calls the identity undecided: it
records the id as settled (MASTER-142, 2026-11-06) and keeps the warning, now scoped to packaged
(MSIX) builds: once the app ships packaged, changing the id makes it a different app to Windows with
its own package-virtualised app-data folder, so an existing install would stop seeing its dungeons.
Today's unpackaged build stores the SQLite database at a fixed
`LocalApplicationData\DungeonMaster\dungeon-master.db` that does not depend on the id
(`src/DungeonMaster.App/MauiProgram.cs:121-125`), so the comment's original unscoped form overstated the
present risk. No ADR was needed, since the identity is decided rather than open.

*Manifest half.* `Publisher` is `CN=Dungeon Master` and `PublisherDisplayName` is `Dungeon Master`;
these were the only identity and display values the manifest ships as literals. `Name`, `Version` and the `$placeholder$` tokens are
build-substituted from the project file (see the `AMENDED 20261106-0120-shipping-identity` block on
`UI-034` in `03-app-shell-and-views.md`, which corrects that entry on this point), so they were left as
tokens with an XML comment naming the csproj as their single source. After the change, `dotnet build
src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug` ended `Build
succeeded.`, `0 Warning(s)`, `0 Error(s)`, and the generated
`obj/Debug/net10.0-windows10.0.19041.0/win-x64/resizetizer/m/Package.appxmanifest` reads
`Name="com.dmarquezdev.dungeonmaster" Publisher="CN=Dungeon Master" Version="0.1.0.1"`,
`<DisplayName>Dungeon Master</DisplayName>` and `<PublisherDisplayName>Dungeon Master</PublisherDisplayName>`.

*Citation drift.* The csproj citations `:29` and `:31-45` no longer point at the identity: by content,
`<ApplicationTitle>` is at `DungeonMaster.App.csproj:55`, the `App Identifier` comment opens at `:57`,
`<ApplicationId>` is at `:69`, and `<ApplicationDisplayVersion>`/`<ApplicationVersion>` are at `:72-73`.
The manifest citations moved by the new six-line comment: `<Identity`, cited `:9`, is now at
`Package.appxmanifest:15`, and `<Properties>`, cited `:13-17`, is now `:19-23`.]**

### XCUT-016 -- EF Core is pinned three patch releases behind a version the pin comment shows was evaluated

```yaml
id: XCUT-016
layer: XCUT
severity: S3
category: dependency
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj:9
  - src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj:13
  - src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj:14-16
effort: XS
risk: low
independent: true
```

**What.** `Microsoft.EntityFrameworkCore.Design` and `Microsoft.EntityFrameworkCore.Sqlite` are both
at `10.0.7`. The comment immediately below them, explaining the `SQLitePCLRaw` pin, ends
"Remove once EF ships a version that resolves 2.1.12 or later on its own (10.0.10 still does not)."

**Why it matters.** I read that parenthesis as evidence that `10.0.10` exists and was examined, which
means the project is knowingly three patch releases behind on its data-access dependency. I state
this as belief rather than fact because I checked the comment and the version attributes and did not
query the package feed, so I do not know what the current latest patch is or what those releases
changed. The reason it is worth a record at all is the interaction: `INFRA-020` reports the migration
history was squashed onto a new baseline, and `INFRA-003`, `INFRA-004` and `INFRA-008` are all
provider-behaviour findings. Provider patch releases are where that class of behaviour changes, so
the gap between the pinned version and the current one is the gap in which those four records were
written.

**Suggested direction.** Move to the current 10.0.x patch and re-check whether the `SQLitePCLRaw`
pin is still needed, since the comment names exactly that as the removal condition. This is small
enough to fold into whichever session takes `XCUT-008`, and the check costs one `dotnet list package
--outdated` run.

### XCUT-017 -- Build files disagree on byte-order mark and the repository has no .gitattributes

```yaml
id: XCUT-017
layer: XCUT
severity: S5
category: style
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.sln:1
  - src/DungeonMaster.Domain/DungeonMaster.Domain.csproj:1
  - src/DungeonMaster.App/DungeonMaster.App.csproj:1
  - Directory.Build.props:1
effort: XS
risk: low
independent: true
```

**What.** `DungeonMaster.sln` and four of the five `.csproj` files begin with a UTF-8 byte-order mark
(`ef bb bf`). `DungeonMaster.App.csproj` and `Directory.Build.props` do not. All seven use CRLF line
endings. No `.gitattributes` exists, so line endings are whatever each contributor's
`core.autocrlf` produces.

**Why it matters.** Nothing is wrong today: MSBuild reads all of these, the build is clean, and a
single-contributor repository on one platform will not notice. I believe it is worth noting only
because the split is arbitrary -- the two files without a mark are the two most recently hand-edited,
which suggests the difference records editor history rather than any intent -- and because a
`.gitattributes` is the kind of thing that is free to add now and awkward to add after a second
contributor on a second platform has produced a whole-file diff. This is an observation, not a
deviation from any authority I can cite.

**Suggested direction.** If `XCUT-012`'s `.editorconfig` is adopted, give it `charset = utf-8-bom`
or `utf-8` for the build files and let a format pass normalise them. Adding a `.gitattributes` with
`* text=auto` at the same time costs one line and settles line endings before it can matter.

## Layer verdict

The build configuration is deliberate where someone has touched it and untended everywhere else --
and the untended parts are exactly the ones that would have caught the findings in the other five
documents. `Directory.Build.props` is the strongest thing here: `TreatWarningsAsErrors=true`
alongside `Nullable=enable`, solution-wide, is a demanding baseline that this codebase meets at zero
warnings. `src/DungeonMaster.sln` is correct apart from an inert platform axis. `DungeonMaster.Domain.csproj`
is the cleanest file in the solution and carries no record -- nine lines, no package reference, no
project reference -- and is the structural fact behind T1's verdict that the Domain holds its shape.
The Application, Infrastructure and Tests project files are sound in what they declare; their records
are all about what is declared elsewhere or not at all. `DungeonMaster.App.csproj` carries the real
judgment, in comments that are the best in-repository documentation I read this session, and also the
most unfinished business.

What is absent matters more. There is no `.editorconfig`, no `global.json`, no
`Directory.Packages.props`, no `nuget.config` and no lock file, so nothing about a build here is
pinned: not the SDK, not the analyzer rule set, not the package graph, not the indent character. The
analyzers that do run are the SDK defaults at `Default` mode with code-style analysis off, which is
why every one of the 40 `S4` records this session produced had to be found by a person reading a
file. The most consequential line is the one not written: `AnalysisLevel` is unpinned while
`TreatWarningsAsErrors` is on, so the build's greenness depends on which SDK happens to be installed.

The boundaries are in better shape than the configuration. All four dependency directions are correct
and none is violated anywhere. Domain-to-Application is sound and has no record: T1 read all twenty
Domain files and found no framework type and no persistence vocabulary, the four private
parameterless constructors being the only concession. Where the boundaries are weak it is surface
area and lifetime, never direction -- the composition root is split across two projects that never
agreed a lifetime (`XCUT-001`), the Application assembly is consumed as three different kinds of
thing (`XCUT-005`), the persistence ports have no address (`XCUT-006`), and the Tests project's
target framework puts App out of reach in a way no per-layer audit could see (`XCUT-002`).

## Attestation

- No file under `src/` was created, modified, or deleted by this task: yes
- Every record cites at least one file path with a line number: yes
- Every record names its authority, and every non-`judgment` record carries a real citation: yes
