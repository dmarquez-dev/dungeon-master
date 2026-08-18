# Contract — repository root and solution configuration

**Producer:** T1. **Consumers:** T2, T3.
**Frozen at Gate 2.** Paths are relative to the repository root of the session worktree.

T1 creates four files that do not exist today and edits two that do. Nothing T1 writes changes an
MSBuild *property* — every property lives in `Directory.Build.props`, which T2 owns. The split is
deliberate: T1 is the restore/VCS environment, T2 is the build.

---

## 1. `global.json` (new)

Exact content:

```json
{
	"sdk": {
		"version": "10.0.303",
		"rollForward": "latestPatch"
	}
}
```

- `10.0.303` is the only SDK installed on the verification machine (`dotnet --list-sdks`).
- `rollForward: latestPatch` keeps the SDK inside the `10.0.3xx` feature band. The analyzer rule set
  moves with the feature band, so pinning the band is what makes `AnalysisLevel` (contract 2) and
  `TreatWarningsAsErrors=true` mean the same thing on two machines.
- **Why not `rollForward: disable`.** `XCUT-009` suggests `disable` on the grounds that lock files
  and the SDK should move in step. It is not taken: `disable` makes the repository unbuildable the
  moment the developer installs any 10.0.3xx patch, and `packages.lock.json` pins package versions,
  not SDK versions, so the two are not actually coupled. If a future SDK patch does move the
  analyzer set, `AnalysisLevel` (pinned numerically) absorbs it.
- Tab-indented, UTF-8, **no BOM**, CRLF, trailing newline.

## 2. `nuget.config` (new, lowercase filename)

Exact content:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<packageSources>
		<clear />
		<add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
	</packageSources>
	<packageSourceMapping>
		<packageSource key="nuget.org">
			<package pattern="*" />
		</packageSource>
	</packageSourceMapping>
</configuration>
```

- `<clear />` is the point of the file (`XCUT-009`): it drops every machine- and user-profile source
  so restore cannot silently reach a feed the repository never named. This is the dependency-confusion
  defence the cited NuGet guidance asks for.
- The `packageSourceMapping` block is a single-source mapping. It is a no-op today and becomes the
  enforcement point the moment a second source is ever added, which is when it is expensive to
  retrofit.
- **`RestorePackagesWithLockFile` does not go here.** It is an MSBuild property; T2 sets it in
  `Directory.Build.props`. See contract 2 §3.
- Tab-indented, UTF-8, **no BOM**, CRLF, trailing newline.

## 3. `.gitattributes` (new)

Exact content:

```
* text=auto

*.png binary
*.jpg binary
*.ico binary
*.ttf binary
*.otf binary
*.db binary
```

- `* text=auto` is the line named verbatim in `XCUT-017`.
- **T1 must not run `git add --renormalize`.** Renormalising would rewrite the line endings of every
  tracked file in one commit — exactly the whitespace churn this session exists to keep separable.
  Existing files keep their stored line endings until they are next staged for another reason.
- The binary block is added beyond what `XCUT-017` asks for because the repository ships PNG object
  packs and TTF icon fonts under `src/DungeonMaster.App/Resources/`, and `text=auto` heuristics on
  binaries are the one place this file can cause damage.
- UTF-8, no BOM, CRLF, trailing newline.

## 4. `.gitignore` (edit)

Append one entry under a new trailing section. The DIR worktree currently shows as untracked in the
main checkout.

```
# DIR session worktrees
.claude-worktrees/
```

No other line of `.gitignore` changes. In particular `packages.lock.json` must **not** be ignored —
T2 commits those files, and the existing `packages/` and `.nuget/` patterns do not match them.

## 5. `src/DungeonMaster.sln` (edit)

Two changes, and nothing else.

**5a — remove the inert platform axis (`MASTER-140` / `XCUT-014`).**

`GlobalSection(SolutionConfigurationPlatforms)` reduces from six entries to two:

```
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
```

`GlobalSection(ProjectConfigurationPlatforms)` reduces from 60 lines to 20 — for each of the five
project GUIDs, exactly these four lines survive, and every `|x64` and `|x86` line is deleted:

```
		{GUID}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{GUID}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{GUID}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{GUID}.Release|Any CPU.Build.0 = Release|Any CPU
```

The five GUIDs, unchanged:

| Project | GUID |
|---|---|
| DungeonMaster.Domain | `{110CCDEA-E21C-4E3D-978A-058E88D2F5B7}` |
| DungeonMaster.Application | `{AE3DA7DE-2053-4525-AC91-B86DC046D6A6}` |
| DungeonMaster.Infrastructure | `{3A6900DA-A352-4F9B-A470-385528A4AB35}` |
| DungeonMaster.App | `{55FB89CA-C058-4BE6-A86C-5FF24A0E42C2}` |
| DungeonMaster.Tests | `{6EF4E72D-7AD6-4C57-BC8B-3F3228E78BEE}` |

Every deleted entry resolved to `Any CPU` already, so no project's build changes. `Project(...)`
declarations, `SolutionProperties` and `ExtensibilityGlobals` are untouched.

**Non-goal:** converting to `.slnx`. `XCUT-014` explicitly calls that a separate decision.

**5b — strip the UTF-8 BOM (`MASTER-150` / `XCUT-017`).**

`src/DungeonMaster.sln` currently begins `ef bb bf`. It must not after T1. See §6.

## 6. Byte-order mark and encoding decision — binding on T1, T2 and T3

`XCUT-017` offers `utf-8-bom` or `utf-8` and does not choose. **This session chooses `utf-8`, no
BOM, for every text file it touches.** Reasons: it is uniform, it matches the two files that were
most recently hand-edited (`Directory.Build.props`, `DungeonMaster.App.csproj`), MSBuild and Roslyn
read BOM-less UTF-8 without complaint, and a BOM is invisible churn in a diff.

Files whose BOM is stripped, and by whom:

| File | Has BOM today | Stripped by |
|---|---|---|
| `src/DungeonMaster.sln` | yes | T1 |
| `src/DungeonMaster.Domain/DungeonMaster.Domain.csproj` | yes | T2 |
| `src/DungeonMaster.Application/DungeonMaster.Application.csproj` | yes | T2 |
| `src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj` | yes | T2 |
| `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` | yes | T2 |
| `Directory.Build.props` | no | — |
| `src/DungeonMaster.App/DungeonMaster.App.csproj` | no | — |

**T3 strips no BOM from any `.cs` file.** T3's diff must be leading-whitespace-only; a BOM change on
line 1 would show up under `git diff --ignore-all-space` and defeat its acceptance criterion. Any
`.cs` BOM in the repository is left exactly as it is and is out of this session's scope.

## 7. What T2 and T3 may rely on

After T1 is signed off, T2 and T3 may assume, without re-verifying:

- `global.json` exists at the repository root and pins the SDK feature band.
- `nuget.config` exists at the repository root, has cleared inherited sources, and nuget.org is the
  only source restore will reach.
- `.gitattributes` exists; no renormalisation has been performed.
- The solution declares `Debug|Any CPU` and `Release|Any CPU` and nothing else. Any build or test
  command written against `-c Debug` or `-c Release` with no `-p:Platform` continues to work.
- No `.csproj`, no `Directory.Build.props`, and no file under `src/` other than `DungeonMaster.sln`
  has been modified by T1.
