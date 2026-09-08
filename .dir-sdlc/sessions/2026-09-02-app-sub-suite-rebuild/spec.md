# Rebuild the App sub-suite

Replace the `App/` test sub-suite's regex-over-source-text assertions with assertions that read
real declarations, real resolved resources and real types -- and add the end-to-end layer the
repository has never had.

Sub-session **6b-i** of Session 6b, of Session 6. Scope fixed at Gate 1 as 11 of Session 6b's 26
audit entries, plus one accepted addition.

## Context

`src/DungeonMaster.Tests/App/` asserts things about the application by reading its `.xaml` and
`.cs` files off disk as strings and matching regular expressions against them. That is not a
stylistic preference -- it is forced. `DungeonMaster.Tests` targets plain `net10.0`, does not set
`UseMaui`, and references Domain, Application, Infrastructure and App.Core but **not**
`DungeonMaster.App`. It has no compiled application to look at, so `App/Theme/RepoPaths.cs` exists
solely to find source files on disk.

The audit's eleven entries describe the consequences: truncating tag matchers that silently scan
fewer controls, a `.*?` style body that mis-terminates on nesting, assertions buried in shared
helpers so failures name the helper, a hand-transcribed MAUI type table frozen at a date, a
TrueType `cmap` parser living in a test class with three of its four branches unexercised, and a
ratchet structurally incapable of failing.

Research before decomposition established two things that shaped this session. First, **Microsoft
documents no supported way to unit-test XAML or views out of process** -- its answers are to test
view models and libraries, or to drive the real app with Appium -- so asserting over the
declaration is not itself the defect; the regex implementation is. Second, Microsoft *does*
document a way to test code inside a MAUI app project: add the plain platform-less TFM to the app's
`TargetFrameworks` and condition `OutputType`. That was measured here and works, giving real types,
real reflection and real resource resolution with no MAUI host and no WinUI.

## Definition of done

Each item is checkable by someone who did not write the code.

1. `DungeonMaster.App` builds for a plain `net10.0` TFM alongside its two platform TFMs, and emits a
   library for the bare TFM while still emitting `DungeonMaster.App.exe` for Windows.
2. A new `DungeonMaster.App.UnitTests` project exists, targets plain `net10.0`, references
   `DungeonMaster.App`, and passes.
3. `DungeonMaster.Tests` still has **no** `DungeonMaster.App` project reference and **no** ambient
   MAUI global usings, demonstrated by command.
4. The shipping app is unaffected: solution build reports `0 Warning(s) 0 Error(s)`,
   `DungeonMaster.Tests` and `DungeonMaster.App.Tests` each pass at or above their baselines of 640
   and 77, and `net10.0-maccatalyst` builds (no runtime claim -- `ADR-001`).
5. `RepoPaths` rejects a candidate root that has a `.git` entry but no `src/DungeonMaster.App`
   beneath it, and a test demonstrates the rejection. *(MASTER-008)*
6. The shared theme/XAML scan exposes a parsed `XDocument` model -- keyed elements, attributes,
   namespace-qualified names, line info -- rather than raw text, and its consumers query that model.
   *(MASTER-028)*
7. Character-entity decoding is rune-correct: a supplementary-plane entity survives decoding intact
   rather than being truncated by a `char` cast, demonstrated by a test over such an entity.
   *(MASTER-040)*
8. No compiled `Regex` is constructed inside a per-item loop anywhere in the sub-suite.
   *(MASTER-063)*
9. The type-matching and icon-font disk scans are computed once per process rather than per test.
   *(MASTER-068)*
10. MAUI property types and the conversion matrix are obtained by reflection over the real types
    instead of a hand-transcribed table carrying a fixed date. *(MASTER-100)*
11. The TrueType `cmap` parser is extracted from its test class into a named unit with its own
    tests, and its previously unexercised subtable branches are exercised. *(MASTER-101)*
12. No assertion executes inside a shared scan helper; every assertion is in the `[Fact]` or
    `[Theory]` that owns it, so a failure names the fact. *(MASTER-102)*
13. Assertions about application C# read real declarations -- `BindableProperty` metadata by
    reflection, theme keys by real resolution against the merged dictionaries -- not the lexical
    form of the source. *(MASTER-109)*
14. Comment-stripping patterns and the line-number helper are declared once and shared.
    *(MASTER-138)*
15. The inert theme ratchet either fails on a real regression or is legible as a documented
    tripwire; it is not left structurally incapable of failing while reading as a guard.
    *(MASTER-149)*
16. Every assertion that enumerates a collection is guarded against vacuity, so an empty collection
    fails rather than passes. *(6a backlog item; 6 live sites measured in `DungeonMaster.App.Tests`)*
17. `MauiHostFixture.InvokeAsync` either has a caller or is removed. *(6a backlog item)*
18. A Windows Appium harness drives **one real interaction** against the unpackaged app and asserts
    its effect. *(accepted addition, not an audit entry)*

## Scope

The 11 entries: `MASTER-008`, `028`, `040`, `063`, `068`, `100`, `101`, `102`, `109`, `138`, `149`.
Plus the accepted Appium addition, and three of the four Session 6a backlog items.

### Not in scope

- **Session 6b-ii's 10 entries and 6b-iii's 5** -- `014`, `020`, `041`, `042`, `064`, `065`, `066`,
  `067`, `069`, `070`, `085`, `086`, `127`, `155`, `167`. Follow-on sessions.
- **`MASTER-161`, `163`, `164`, `165`, `166`** -- sidebar and UI defects, excluded from Session 6
  entirely at its Gate 1.
- **The `ShippedPackKinds` / `pack.json` backlog item** -- graphics and catalogue work, not the App
  sub-suite. Explicitly declined rather than silently dropped.
- **Migrating or extending `MauiHostFixture`.** No entry in the eleven needs the WinUI host. T10
  makes the only `App.Tests` changes and neither migrates nor builds on the host.
- **Reformatting XAML.** Every `.xaml` file violates its own `.editorconfig` (`[*.xaml]` declares
  tabs; the files are space-indented) and `IDE0055` does not reach XAML. Match the file, never
  retab.
- **Any macCatalyst runtime claim.** It builds; it cannot be run here (`ADR-001`).

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Why does the App sub-suite assert over source text? | It is forced to: `DungeonMaster.Tests` cannot see the compiled application | 1 | `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` -- plain `net10.0`, no `UseMaui`, no `DungeonMaster.App` reference |
| Is asserting over the declaration itself the defect? | No. Microsoft documents no supported out-of-process way to test XAML or views; the regex implementation is the defect | 3 | `learn.microsoft.com/dotnet/maui/deployment/unit-testing` and `.../ui-testing`; WinUI guidance to refactor testable code into a library |
| Can the App project build on a plain `net10.0` TFM? | Yes -- 3 edited csproj lines plus one ~26-line platform-less seam file; none of the six WinUI-referencing files needed a guard | 1 | `spike-plain-tfm-report.md` Q1, Q2 |
| Does XAML compile on the bare TFM? | Yes -- `SourceGen` produced all 26 `.xsg.cs` files, each with `InitializeComponent` | 1 | `spike-plain-tfm-report.md` Q3 |
| Can an `Element` be constructed without a MAUI host? | Yes on the platform-less TFM: `new BoxView()` succeeds in 242 ms. The static-constructor wall is specific to `net10.0-windows` | 1 | `spike-plain-tfm-report.md` Q4c |
| What does constructing a XAML control require? | A constructed `App` (for the seven merged theme dictionaries) and a dispatcher provider; ~40 lines of fixture | 1 | `spike-plain-tfm-report.md` Q4d, rounds 1-3 |
| Can `BindableProperty` metadata be read by reflection? | Yes -- `PropertyName`, `ReturnType`, `DeclaringType`, `DefaultValue`, verified against an empty-string and a `null` default | 1 | `spike-plain-tfm-report.md` Q4b |
| Reference App from `DungeonMaster.Tests`, or a new project? | A new `DungeonMaster.App.UnitTests`. The reference imports 18 MAUI global usings and produces 31 `CS0104` errors across 3 files today, leaving `Application`, `Element`, `Color`, `Page`, `View` ambient in a 640-test project | 5 | Escalation E1; `spike-plain-tfm-report.md` Q4 |
| Does any of the eleven need the WinUI host? | No | 1 | Architect's per-entry analysis; `spike-parallelism-report.md` for what the host would have cost |
| Test framework for the Appium harness? | xunit, matching both existing test projects. The NUnit `[SetUpFixture]` namespace trap therefore does not apply and must not appear in evidence | 1 | `DungeonMaster.Tests.csproj`, `DungeonMaster.App.Tests.csproj` |
| How is the app addressed by the Windows driver? | By executable path -- it is unpackaged, not the packaged AUMID the Microsoft sample shows | 1 | `DungeonMaster.App.csproj` sets `WindowsPackageType=None` |
| Does the app already carry `AutomationId`s? | Yes -- 52 in App XAML, 0 in App C#. No XAML edit is needed for the harness | 1 | `grep -rn "AutomationId" src/DungeonMaster.App --include=*.xaml` |
| Are package versions managed centrally? | Yes -- `ManagePackageVersionsCentrally` and `CentralPackageTransitivePinningEnabled`, both at the repository root, with `RestorePackagesWithLockFile=true` | 1 | `Directory.Packages.props:3-4`; `Directory.Build.props:21` |
| Are the audit's counts current? | No. `TEST-004` says 11 of 13 App files; there are 20 `.cs` files, 14 naming `Regex`, 1 naming `XDocument`. `TEST-016` lists 7 helper-assertion sites; there are 14, and one cited location is wrong | 1 | `find`/`grep` commands recorded in `coverage.md` and the architect's report |

## Contracts

- `contracts/build-file-ownership.md` -- who may write the solution file, `Directory.Packages.props`
  and each `packages.lock.json`, and the escalation path when a task must touch one it does not own.
- `contracts/app-unit-tests-project.md` -- the new project's shape, and `AppFixture`: the
  constructed `App`, the dispatcher provider, `FindResource`, `Construct<T>`, `AppTestCollection`.
- `contracts/repo-paths.md` -- `Root`, `AppRoot`, `AppSourceFiles`, and the hardened root check.
- `contracts/theme-source.md` -- the `XDocument` model and the shared text helpers, with every
  member's external call sites named.
- `contracts/anti-vacuity.md` -- the enumeration-assertion rule, the helper-assertion inventory, and
  the prohibition on resting any criterion on `MauiHostFixture.HandledExceptions`.
- `contracts/appium-windows-harness.md` -- the harness shape, the unpackaged addressing, and the
  requirement that the client version be established rather than assumed.

## Open questions

Both are Gate 2 decisions, raised by `dir-architect` and carried here rather than answered by it.

1. **Is `src/DungeonMaster.App.UiTests` added to `src/DungeonMaster.sln`?** The architect encoded
   "no", because its tests cannot pass without an Appium server and a built app, so a solution-wide
   `dotnet test` would fail by default everywhere. That departs from the convention every other
   project follows. *Resolved by: the user, at Gate 2.*
2. **Which `Appium.WebDriver` version speaks to the installed Appium 3.7.0 server?** No version is
   named in any criterion. The latest is 8.3.2; Microsoft's page pins 8.0.0 but assumes an Appium
   2.x server. That a W3C-WebDriver client of either version reaches a 3.7.0 server is believed, not
   established. *Resolved by: T11, which must establish it before pinning; escalates if it cannot.*
