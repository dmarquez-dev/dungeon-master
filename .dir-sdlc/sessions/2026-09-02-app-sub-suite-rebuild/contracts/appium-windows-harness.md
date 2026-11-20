# Contract — the Windows Appium end-to-end harness

Producer: **T11**. Consumers: **none**. Nothing in the session depends on this task and it produces no
surface anyone else compiles against.

It exists as a contract for one reason only: T11 is the sole owner of `Directory.Packages.props`, and
it is the only task in the session that adds a package the repository has never carried.

It closes **no audit entry**. It is new coverage the user scoped in deliberately at escalation E2.

## 1. Prerequisites — installed, verified on this machine

```
appium --version                  -> 3.7.0
appium driver list --installed    -> windows@6.1.1 [installed (npm)]
(Get-Item 'C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe').VersionInfo
                                  -> ProductVersion 1.2.200902003-release   (the v1.2.1 build)
```

Measured by the coordinator on 2026-09-02. The task is **not** blocked.

## 2. The version question T11 must settle by measurement, not by reading

**The installed server is Appium 3.7.0.** Microsoft's page
(https://learn.microsoft.com/dotnet/maui/deployment/ui-testing) pins `Appium.WebDriver` **8.0.0**,
which is the Appium 2.x-era .NET client. **That version must not be carried into this project on the
strength of that page.**

The failure mode this guards against is specific and is the exact shape this session exists to
eliminate: a wrong client version yields a harness that **compiles cleanly and never connects**.
Green, and proving nothing.

What is established from evidence, and what is not:

| Claim | Evidence |
|---|---|
| The latest `Appium.WebDriver` on nuget.org is **8.3.2** | `curl -s https://api.nuget.org/v3-flatcontainer/appium.webdriver/index.json` — the version list ends `8.3.0, 8.3.1, 8.3.2`. `external` |
| 8.3.2 depends on `Selenium.WebDriver` 4.46.0 and `System.Drawing.Common` 10.0.9, targets `.NETStandard2.0`, and carries a 2014–2026 copyright | its `.nuspec`, fetched from the same flat-container endpoint. `external` |
| Nothing Appium-, Selenium- or NUnit-related exists in the repository or the local NuGet cache | `grep -n "Appium\|Selenium\|NUnit" Directory.Packages.props` → no matches; `ls ~/.nuget/packages/appium.webdriver` → not present |
| **That 8.3.2 speaks to an Appium 3.7.0 server** | **Not established, and Gate 2 resolved that it must not be assumed.** No measurement was run. Believed likely, because both sides speak W3C WebDriver over HTTP and 8.3.2 is current, but this is `judgment`, stated as belief and never as fact. It is a CANDIDATE, not a pin. |

**Resolved at Gate 2 (escalation E4): no version is pinned anywhere in this decomposition.** Not in
this contract, not in the manifest, not in a criterion. **8.3.2 is the candidate to try first and is
explicitly UNPROVEN against a 3.x server.** It is named here so that T11 does not start from the doc
page's 8.0.0, and for no other reason.

**T11 pins a version by opening a session against the running 3.7.0 server, not by reading a doc
page.** The version it pins goes into `Directory.Packages.props` with a comment naming the server
version it was proven against and the date. If the first choice does not connect, T11 tries another
and records what failed — a rejected version with its error is more useful to the next reader than a
clean file. **If T11 cannot establish a working client version at all, it ESCALATES.** It does not
pin a version it has not seen connect, and it does not fall back to a build-only harness: the user
chose a driven interaction over that explicitly at E2.

## 3. Test framework — xunit, deliberately, not the sample's NUnit

The repository standardises on xunit: both `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` and
`src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj` reference `xunit` and
`xunit.runner.visualstudio`, and the session's composed test discipline is written against that
vocabulary. `Directory.Packages.props` has no NUnit entry.

`Appium.WebDriver` is a Selenium-derived **client library**, not a test framework. It imposes no
runner. Adopting NUnit here would put a second test framework and a second adapter into the
repository to gain nothing.

**T11 uses xunit.** Two consequences that must be carried:

1. The Microsoft sample's most-quoted trap — NUnit's `[SetUpFixture]` being namespace-scoped, so a
   namespace mismatch silently leaves the driver uninitialised — **does not apply** and must not
   appear in this project's code, comments, or acceptance criteria as though it did. Driver lifetime
   is an xunit `IClassFixture`/`IAsyncLifetime` concern here.
2. Any code copied from that page needs translating, not transcribing. A transcription that carries
   NUnit attributes will not compile; one that carries NUnit's *assumptions* will compile and mislead.

## 4. The app is unpackaged and is addressed by executable path

`src/DungeonMaster.App/DungeonMaster.App.csproj` sets `<WindowsPackageType>None</WindowsPackageType>`.
The app therefore has **no package identity and no AUMID**. The sample's

```csharp
App = "com.companyname.appname_9zz4h110yvjzm!App"
```

form is for a packaged (MSIX) app and **will not work here**. The unpackaged app is addressed by the
absolute path of `DungeonMaster.App.exe` under
`src/DungeonMaster.App/bin/<Configuration>/net10.0-windows10.0.19041.0/win-x64/`.

T11 verifies the exact capability spelling the installed `windows@6.1.1` driver expects rather than
copying the sample's. It must not invent one: if it cannot confirm the spelling from the driver's own
documentation or from a session that actually opened, it says so and reports the failure rather than
shipping a harness that cannot launch.

## 5. Package and lock-file ownership — the `5a` glob trap in a new place

T11 is the **sole** owner of `Directory.Packages.props` (see `build-file-ownership.md` §2). It adds
`PackageVersion` entries for the Appium client and whatever it transitively requires, keeps the
existing alphabetical order, and **re-versions nothing that is already there**. Re-versioning
`Microsoft.Maui.Controls` in particular would move the ground under T8, which reads that assembly's
real property types by reflection.

`Directory.Build.props:21` sets `RestorePackagesWithLockFile=true`, and
`Directory.Packages.props:4` sets `CentralPackageTransitivePinningEnabled=true`. So:

- T11 owns `src/DungeonMaster.App.UiTests/packages.lock.json` and commits it with the csproj.
- **T11 must check whether adding the new `PackageVersion` entries rewrote any *other* project's lock
  file.** `git status --porcelain` after a solution-wide restore is the check. If another lock file
  changed, T11 **escalates** — it does not commit a file it does not own, and it does not revert the
  change and hope. `Selenium.WebDriver` and `System.Drawing.Common` are not referenced anywhere else
  today, so no other lock file is expected to move; "expected" is why the check is a criterion.

## 6. Not in the solution — deliberately

`src/DungeonMaster.App.UiTests` is **not** added to `src/DungeonMaster.sln`. **Resolved at Gate 2
(escalation E3): the user took this option.** The reasoning is in `build-file-ownership.md` §3. In short: its tests
cannot pass without an Appium server running and the app built, so a solution-wide `dotnet test` would
fail by default on every machine; and the solution file has one owner (T1), so a second writer would
either race it or need a dependency the isolation argument spends.

T11 pays for that invisibility by writing `src/DungeonMaster.App.UiTests/README.md` naming, verbatim,
the commands to build the app, start the server, and run the harness — and by putting those same
commands in its acceptance criteria, so the reviewer runs them rather than trusting them.

## 7. Build settings inherited from the root, which are not optional

`Directory.Build.props` applies to this project too: `Nullable=enable`, `ImplicitUsings=enable`,
`TreatWarningsAsErrors=true`, `LangVersion=14.0`, `AnalysisLevel=10.0`, `AnalysisMode=Recommended`,
`EnforceCodeStyleInBuild=true`.

One known hazard to check rather than assume: `System.Drawing.Common` carries
`SupportedOSPlatform("windows")` annotations, and under `TreatWarningsAsErrors` a `CA1416` on a
platform-neutral TFM is an error — this is the same failure the App project hit at
`MauiProgram.cs(42,3)` and had to `NoWarn` (`app-unit-tests-project.md` §1.1). Targeting
`net10.0-windows10.0.19041.0` rather than plain `net10.0` avoids it at the source and is also honest
about what the project is: a harness that drives a Windows application. T11 may choose otherwise, but
it reports what it measured, and it does not reach for `NoWarn` before trying the TFM.

C# in this project is **tab-indented, CRLF**, like the rest of the repository. Verify with
`tr -cd '\r' | wc -c` against `tr -cd '\n' | wc -c` — never with `grep`. See
`build-file-ownership.md` §5.

## 8. What "proves one interaction end to end" means

The user chose a driven interaction over a build-only harness explicitly. A criterion is met only
when **all** of the following hold in one run, with the console output quoted:

1. A session is opened against the running Appium 3.7.0 server and the unpackaged
   `DungeonMaster.App.exe`, and the driver returns a non-null session.
2. An element is located by `AccessibilityId` — resolving an `AutomationId` set in the app's XAML.
   **The app already carries them and T11 adds none.** Measured in the worktree on 2026-09-02:

   ```
   grep -rn "AutomationId" src/DungeonMaster.App --include=*.xaml | grep -v /obj/ | grep -v /bin/ | wc -l
   ```
   → **52** occurrences across the control and view XAML, including clickable ones such as
   `ConfirmPopupConfirmButton`, `FloorNotebookButton`, `AddAreaButton` and `NotebookCloseButton`.
   (`AutomationId` appears **0** times in App `.cs`.)

   T11 therefore **owns no `.xaml` file and edits none**. If it finds it genuinely cannot reach any
   existing id and needs a new one, that is a change to a file outside its ownership set and it
   **escalates** rather than editing.
3. `Click()` is invoked on it.
4. A **different** observable property of the application changes as a result, and the test asserts
   on that change — not on the click having been dispatched, and not on the element still existing.
5. **The test is shown able to fail.** Either a red step (the assertion observed failing before the
   interaction was wired) or a demonstrated discrimination (assert the post-click state *without*
   performing the click, and watch it fail). Naming the command and quoting the failure is the whole
   of the requirement. A UI test never seen failing is indistinguishable from one that clicked
   nothing.

A screenshot is not evidence. A passing run with no failing counterpart is not evidence.
