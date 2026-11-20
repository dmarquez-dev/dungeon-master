# Contract — who owns the shared build files

Producer: none singly. This contract exists because five of the eleven tasks would otherwise each
have a reason to edit the same four kinds of file, and the harness has no lock.

Consumers: **every task in the session**.

## 0. Why this is a contract and not a note

`Directory.Build.props:21` (verified in the worktree, `sed -n '19,23p' Directory.Build.props`) sets
`RestorePackagesWithLockFile=true`, and `Directory.Packages.props:3` sets
`ManagePackageVersionsCentrally=true`. Together those mean:

- **Every `.csproj` a task owns, it also owns that project's `packages.lock.json`.** Restore rewrites
  the lock file without being asked. The Option-C spike measured **+546 lines across two lock files**
  (+378 `src/DungeonMaster.App/packages.lock.json`, +168 `src/DungeonMaster.Tests/packages.lock.json`)
  from the TFM change alone (`git diff --stat`, recorded in `spike-plain-tfm-report.md` §Q6). A task
  whose `owns` glob names a `.csproj` and not its lock file leaves the branch unrestorable on a fresh
  clone.
- **A new `PackageReference` requires a new `PackageVersion` in the root `Directory.Packages.props`.**
  That file is shared, and only one task in this session may touch it.

Verified in the worktree on 2026-09-02:

```
find src -name '*.csproj' -not -path '*/obj/*' | wc -l              -> 7
find src -name 'packages.lock.json' -not -path '*/obj/*' -not -path '*/bin/*' | wc -l  -> 7
grep -c "Project(" src/DungeonMaster.sln                            -> 7
```

Seven projects, seven lock files, seven solution entries. The pairing is 1:1 today and must stay so.

## 1. Exclusive owners

| File | Sole owner | Nobody else may edit it |
|---|---|---|
| `src/DungeonMaster.App/DungeonMaster.App.csproj` | **T1** | including to add a package |
| `src/DungeonMaster.App/packages.lock.json` | **T1** | |
| `src/DungeonMaster.App.UnitTests/DungeonMaster.App.UnitTests.csproj` | **T1** | T8 and T9 add `.cs` files to the project; the SDK globs them, so no csproj edit is needed or permitted |
| `src/DungeonMaster.App.UnitTests/packages.lock.json` | **T1** | |
| `src/DungeonMaster.sln` | **T1** | see §3 |
| `Directory.Packages.props` (repo root) | **T11** | see §2 |
| `src/DungeonMaster.App.UiTests/**` | **T11** | |
| `Directory.Build.props` (repo root) | **nobody** | no task in this session has a reason to change it. A task that believes it does escalates. |
| `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` and its `packages.lock.json` | **nobody** | see §4 — this is an invariant, not an omission |
| `src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj` and its `packages.lock.json` | **nobody** | T10 edits `.cs` files in that project only |

## 2. `Directory.Packages.props` — T11 only

T1's new `DungeonMaster.App.UnitTests` needs **no** new package version. Every package it references
already has a `PackageVersion` entry:

```
grep -n "PackageVersion Include" Directory.Packages.props
```

→ `coverlet.collector`, `FluentAssertions`, `Microsoft.NET.Test.Sdk`, `NSubstitute`, `xunit`,
`xunit.runner.visualstudio` are all present, and `Microsoft.Maui.Controls` arrives transitively
through the `DungeonMaster.App` project reference. T1 therefore does not touch this file.

T11 (Appium) needs package versions that do not exist in the repository today — an NUnit runner and
`Appium.WebDriver`. T11 adds them, and **T11 is the only task permitted to**, precisely so that no
two tasks race on the one file.

T11 must keep the existing entries alphabetically ordered as the file already is, and must not
re-version any existing entry. Re-versioning `Microsoft.Maui.Controls` in particular would move the
ground under T8, which reads that assembly's real property types by reflection.

`Directory.Packages.props:4` also sets `CentralPackageTransitivePinningEnabled=true`, so a new
`PackageVersion` entry can rewrite a lock file in a project T11 does not own. **T11 runs a
solution-wide restore and checks `git status --porcelain` afterwards.** Nothing Appium- or
Selenium-related exists in the repository today (`grep -n "Appium\|Selenium\|NUnit"
Directory.Packages.props` → no matches), so no other lock file is expected to move. If one does, T11
**escalates** rather than committing a file outside its ownership set. This is the glob trap that bit
sub-session 5a, in a new place.

## 3. `src/DungeonMaster.sln` — T1 only, and T11 is deliberately absent from it

T1 adds exactly one `Project(...)` entry, for `DungeonMaster.App.UnitTests`, with its configuration
rows in `GlobalSection(ProjectConfigurationPlatforms)`. After T1 the counts are 8 and 8.

**T11's `DungeonMaster.App.UiTests` is deliberately NOT added to `src/DungeonMaster.sln`.** This is a
considered departure and the reasons are recorded here rather than left to be rediscovered:

1. **The project's tests cannot pass without an Appium server running and the app built.** A project
   inside the solution whose tests fail by default on every machine that has not installed the
   `appium` CLI, the `windows` driver and WinAppDriver is a worse default than a project outside it
   whose build and run commands are written down. This is the load-bearing reason.
2. Ownership cannot be transferred mid-session. If T1 owns the solution file and T11 needs an entry
   in it, either two tasks write one file, or the entry lands before the project does and
   `dotnet build src/DungeonMaster.sln` fails for everyone until T11 finishes.
3. T11 depends on nothing but T1, and nothing depends on T11, so it is the one task that may be
   dropped at Gate 2 or run last without disturbing anything. A solution entry would remove that
   property.

(The prerequisites are now installed — `appium` 3.7.0, `windows@6.1.1`, WinAppDriver
`1.2.200902003-release` — so "blocked on an install" is no longer among the reasons. It was, when
this decomposition began.)

The cost is that `DungeonMaster.App.UiTests` is invisible to a solution-wide build. T11 pays that
cost by naming its own build and run commands in its acceptance criteria and in a `README.md` inside
its own directory. **Resolved at Gate 2 (escalation E3): the user took this option**, for the reason given in item 1 —
its tests cannot pass without a running Appium server, so including it would make a solution-wide
`dotnet test` fail by default for everyone. The `README.md` and the criteria-level build and run
commands are the agreed compensating control.

## 4. `DungeonMaster.Tests` stays free of MAUI — this is an invariant

`src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` must not gain a `ProjectReference` to
`DungeonMaster.App`, and must not gain a `Using Include="Microsoft.Maui..."` or any MAUI
`PackageReference`.

This is the reason the user chose a new project. Measured in `spike-plain-tfm-report.md` §Q4: adding
the reference imports **18 MAUI global usings** transitively from `Microsoft.Maui.Controls`, of which
one collides today — `Microsoft.Maui.Dispatching.IDispatcher` against the project's own
`DungeonMaster.Application.Common.Dispatch.IDispatcher` — producing **31 `CS0104` errors across 3
files**:

```
src/DungeonMaster.Tests/Application/Common/Dispatch/DispatcherTests.cs
src/DungeonMaster.Tests/Application/Common/Dispatch/ApplicationServiceRegistrationTests.cs
src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/CommandValidatorCoverageTests.cs
```

and leaving `Application`, `Element`, `Color`, `Page` and `View` ambient in a 640-test project
thereafter.

**Any task that finds itself wanting App types inside `DungeonMaster.Tests` has found work that
belongs in `DungeonMaster.App.UnitTests`.** T8 and T9 are the two tasks this applies to and both are
scoped for it. Any other task hitting it escalates rather than adding the reference.

The invariant is checkable and T1 carries the criterion for it.

## 5. Formatting, because `TreatWarningsAsErrors` is on

`Directory.Build.props` sets `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`,
`AnalysisLevel=10.0` and `AnalysisMode=Recommended` for every project in the repository, the two new
ones included.

- **C# and `.csproj` and `.props`: tab-indented, CRLF.** `.editorconfig` `[*]` sets
  `indent_style = tab` and `end_of_line = crlf`; `[*.cs]` repeats the tab rule.
- **Never verify line endings with `grep`.** `grep -c '^\t'` returns 0 on a fully tab-indented file
  and `grep -cv $'\r$'` has returned both the full line count and 0 on the same all-CRLF file in
  different shells. Use `tr -cd '\r' | wc -c` against `tr -cd '\n' | wc -c`; equal counts mean CRLF
  throughout. Verified this way on `Directory.Packages.props` (29/29),
  `src/DungeonMaster.Tests/App/Theme/ThemeSource.cs` (230/230) and `src/DungeonMaster.sln` (57/57).
- `sed -i` rewrites a file as LF and trips `IDE0055`. Pasting an LF block into a CRLF file does the
  same. Use the editing tools.
- **XAML is space-indented and violates its own `.editorconfig`** (`[*.xaml]` says
  `indent_style = tab`; `IDE0055` does not reach XAML). Match the file. **No task in this session
  reformats a `.xaml` file**, and no criterion may require it.
- `packages.lock.json` is generated. `.editorconfig` `[*.{json,yml,yaml}]` sets
  `indent_style = space`; restore produces that. Commit what restore writes; do not hand-edit.

## 6. Evidence rules for build and test output

Binding on every criterion in this session.

- **MSBuild prints one aggregate `0 Warning(s) / 0 Error(s)` for a multi-TFM build, not one pair per
  TFM.** T1 adds a **third** TFM to `DungeonMaster.App`. A criterion that asks for a per-TFM pair
  asks for output the tool does not produce. Where per-TFM evidence is genuinely wanted, run
  `dotnet build ... -f <tfm>` once per TFM and quote each aggregate separately.
- **There is no aggregate test figure.** A solution-wide `dotnet test` prints one line per assembly.
  Every count is stated per assembly with the command that produced it. After T1 there is a **third**
  such line.
- **Baselines to hold:** `DungeonMaster.Tests` **640 passed**, `DungeonMaster.App.Tests` **77
  passed**, `dotnet build src/DungeonMaster.sln` clean. Both figures are from
  `spike-plain-tfm-report.md` §Q5 and `spike-parallelism-report.md` §0, each with its command. A task
  that changes a count states the new count with its command and says which tests account for the
  difference.
- **The solution is `<worktree>\src\DungeonMaster.sln`.** Note the `src\` segment. A previous session
  wrote `<worktree>\DungeonMaster.sln` into five criteria and no such file exists.
- **Cite a member by NAME, not by line.** A line number in a contract is stale the moment the file
  shifts, and this session has already shipped one wrong set: `GeometryScaleTests.cs:720-723` and
  `:727-730` pointed into a method body, while the real declarations are `XmlCommentPattern` at
  `:838` and `WholeLineCommentPattern` at `:845` (E5). Where a line number is genuinely useful, give
  the member name first and the line second, and mark the line as frozen at a date. This is the form
  T3 adopted in its own doc comments and it is now the house rule.
- **Never assert that a file is unmodified without naming the command that shows it.** An
  unverifiable claim in a criterion is worse than no criterion: the reviewer has nothing to run.
  (E7, T9's criterion 12.)
- **`git status --porcelain -- <globs>` is the file-set evidence command in this session, never
  `git diff --name-only`.** `git diff` omits untracked files entirely — three of T3's four
  deliverables were new files and it listed none of them — and unscoped in this shared worktree it
  reports peer tasks' work as if it were yours. Always scope it to the task's own `owns` globs. A
  deletion shows as ` D`, so the scoped form covers that case too. (E7)

- **Concurrent peer builds in one worktree produce spurious failures.** 6a measured five distinct
  ones, all cleared on a plain retry. Disjoint ownership prevents file conflicts, not build
  collisions. A build failure that names a locked or partially-written file in a project the task
  does not own is retried once before it is reported.
