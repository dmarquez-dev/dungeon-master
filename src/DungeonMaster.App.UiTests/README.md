# DungeonMaster.App.UiTests

An Appium end-to-end harness that drives the real, unpackaged `DungeonMaster.App.exe` on Windows.

**This project is deliberately NOT in `src/DungeonMaster.sln`.** Its tests cannot pass without an
Appium server running and the app built, so including it would make a solution-wide `dotnet test`
fail by default on every machine that has not installed the tooling below. This README is the
compensating control: it carries the commands the solution file would otherwise imply.

## Prerequisites, and the versions this was proven against

| Component | Version proven on 2026-09-04 | How to check |
| --- | --- | --- |
| Appium server | `3.7.0` | `appium --version` |
| Appium Windows driver | `windows@6.1.1` | `appium driver list --installed` |
| WinAppDriver | `1.2.200902003-release` | `(Get-Item 'C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe').VersionInfo.ProductVersion` |
| `Appium.WebDriver` client | `8.3.2`, pinned in `Directory.Packages.props` | see that file's comment |

Windows must be in **Developer Mode** for WinAppDriver to attach to an application.

The client version is pinned to one that was **observed opening a session against this server**, not
to the `8.0.0` the Microsoft MAUI UI-testing page shows. That page targets the Appium 2.x-era client.

Install the server and driver with:

```
npm install -g appium
appium driver install --source=npm appium-windows-driver
```

## 1. Build the app under test

```
dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug
```

This produces `src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`,
which is the path the harness launches. The app is unpackaged (`WindowsPackageType` is `None`), so it
has no package identity and no AUMID: it is addressed by **executable path**, through the
`appium:app` capability the installed `windows@6.1.1` driver documents for a classic app.

## 2. Start the Appium server

In its own terminal, and leave it running:

```
appium --address 127.0.0.1 --port 4723
```

`http://127.0.0.1:4723/status` should answer `"ready":true`.

## 3. Build and run the harness

```
dotnet build src/DungeonMaster.App.UiTests/DungeonMaster.App.UiTests.csproj -v m
dotnet test src/DungeonMaster.App.UiTests/DungeonMaster.App.UiTests.csproj --logger "console;verbosity=detailed"
```

`verbosity=detailed` is not decoration: the per-step console output (session id, the element
located, the click, the resulting popup title) is what makes a passing run readable as something
other than a green tick.

Stop the server when finished. Each test quits its own driver, which normally closes the app window
it launched. That is not a guarantee: after a run in which a test FAILED, one
`DungeonMaster.App.exe` was measured still running with the driver already quit (2026-09-04). Check
with `Get-Process DungeonMaster.App` after a red run and stop what is left.

## Overrides

| Variable | Effect |
| --- | --- |
| `DM_UITESTS_APPIUM_URL` | Appium server endpoint. Default `http://127.0.0.1:4723/`. |
| `DM_UITESTS_APP_PATH` | Absolute path to the executable under test. Default is the `Debug` path above. |

## What the tests do

`DungeonListPageInteractionTests` drives the app's start route as three facts that form one
experiment:

- `ASessionOpensAndTheDungeonListPageIsReachable` — control. A session opens and returns a session
  id, and `CreateDungeonButton` resolves by `AccessibilityId`.
- `TheCreateDungeonPopupIsAbsentBeforeTheButtonIsClicked` — control. `PopupTitle` is absent while
  the button is reachable, so "not open" is distinguishable from "looking at nothing".
- `ClickingCreateDungeonOpensTheCreateDungeonPopup` — the driven interaction. `Click()` on
  `CreateDungeonButton`, then `PopupTitle` appears reading `New Dungeon`.

The controls exist because a UI experiment in which everything fails cannot tell a detected defect
apart from a broken apparatus. Removing the `Click()` reddens the third fact and leaves the first
two green; that asymmetry is the evidence the click is what the assertion reads.

Every `AutomationId` addressed here is already declared in the app's XAML. **This project adds none
and edits no `.xaml` file.**

## Notes

- The app under test uses the **real** local database under
  `%LOCALAPPDATA%\DungeonMaster\dungeon-master.db`. These tests only read it: the create popup is
  opened and never confirmed, so no dungeon is written. A test that confirms a create would need a
  database seam the app does not currently expose to an out-of-process runner.
- The project targets plain `net10.0`. The `System.Drawing.Common` platform annotations that
  `Appium.WebDriver` drags in produce no `CA1416` here, measured on 2026-09-04: nothing in this
  project touches a `System.Drawing` type. If a future test does, move the project to
  `net10.0-windows10.0.19041.0` rather than adding `NoWarn`.
- This project uses **xunit**, like the rest of the repository — `Appium.WebDriver` is a client
  library and imposes no runner, so no second test framework is introduced. Driver lifetime is an
  ordinary `IDisposable` owned by each test, not a framework-scoped fixture holding a shared driver.
