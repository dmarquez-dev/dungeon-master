# DungeonMaster.App.UiTests

An Appium end-to-end harness that drives the real, unpackaged `DungeonMaster.App.exe` on Windows.

**This project is deliberately NOT in `src/DungeonMaster.sln`.** Its tests cannot pass without an
Appium server running and the app built, so including it would make a solution-wide `dotnet test`
fail by default on every machine that has not installed the tooling below. This README is the
compensating control: it carries the commands the solution file would otherwise imply.

## Prerequisites, and the versions this was proven against

| Component | Version | Proven on | How to check |
| --- | --- | --- | --- |
| Appium server | `3.7.0` | 2026-09-04, re-proven 2026-09-14 | `appium --version` |
| Appium Windows driver | `windows@6.1.1` | 2026-09-04, re-proven 2026-09-14 | `appium driver list --installed` |
| WinAppDriver | `1.2.200902003-release` | 2026-09-04, re-proven 2026-09-14 | `(Get-Item 'C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe').VersionInfo.ProductVersion` |
| `Appium.WebDriver` client | `8.3.2`, pinned in `Directory.Packages.props` | 2026-09-04, re-proven 2026-09-14 | see that file's comment |

Windows must be in **Developer Mode** for WinAppDriver to attach to an application. Check it with
`(Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock').AllowDevelopmentWithoutDevLicense`,
which must answer `1`. It answered `1` on 2026-09-14.

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

`FloorSidebarAutomationTests` drives the floor sidebar on `DungeonDetailPage`, reached by clicking
`EditDungeonButton` on the start route. Its four facts were proven on **2026-09-14**. Each names the
control that stays green when it reddens, and the first bullet is the apparatus control all four
share:

- `ControlASessionOpensAndTheFirstDungeonDetailPageShowsAtLeastOneFloorCard` — the apparatus
  control for all four. If this reddens, the session never reached a page with floors and every
  other failure in the class is apparatus rather than defect.
- `TabbingFromTheDetailPageEntryReachesFloorExpandToggle` — presses `Tab`, reports the focused
  `AutomationId` after each press, and asserts `FloorExpandToggle` appears in the sequence. It is
  reached at press 4: `StartSessionButton`, `DungeonNotebookButton`, `AddFloorButton`,
  `FloorExpandToggle`. Control:
  `ControlOneTabPressFromPageEntryMovesFocusOffTheEntryElement`, which establishes only that a
  `Tab` reaches the app at all — so a red walk beside a green control means the chevron is
  genuinely out of the tab ring, which is what it was while it was a `Label`.
- `FloorExpandToggleMeasuresAtLeast23By23` — the chevron's hit footprint, measured 55x55 on
  2026-09-14. Control: `ControlFloorNotebookButtonMeasuresAtLeast23By23`, an untouched button of
  the same header row read through the same rectangle channel.
- `FloorDragHandleRightEdgeClearsTheScrollViewportRightEdgeByAtLeast14` — the `MASTER-163`
  ratchet. Measured clearance 30, threshold 14. Control:
  `ControlTheSidebarScrollViewportResolvesAsAScrollViewerWiderThanAFloorCard`, which proves the
  element the clearance is measured against really is the scroll viewport.
- `AddAreaButtonIsAbsentWhileTheFloorIsCollapsedAndPresentAfterTheExpandToggleIsActivated` —
  0 before the click, 1 after. Control:
  `ControlFloorDeleteButtonIsPresentWhileTheFloorIsCollapsed`, which distinguishes "the add-area
  row is not shown" from "this session is looking at nothing". Removing the `Click()` was run on
  2026-09-14 and produced 1 failed, 2 passed — the fact red, both controls green.

### What this harness cannot assert, and why it is written down rather than attempted

- **That a floor reorder succeeds.** Synthetic `mouse_event` input was measured on 2026-09-11 not
  to initiate WinUI drag-and-drop at all: the source card stayed at full opacity, so
  `OnDragStarting` never fired. **W3C pointer actions were attempted on 2026-09-14 and cannot
  reach it either**: a `mouse` pointer input source is refused by this server/driver pair with
  `unsupported operation: Currently only pen and touch pointer input source types are
  supported`, so the instrument has no mouse to press, let alone to drag. The sibling `key`
  input source is refused the same way — `Currently key input source type is not supported`,
  which is why `Tab` is sent to an element instead. Contract C2 section 3 recorded the pointer
  route as UNPROVEN; it is now measured, and it is closed. The reorder is verified by human
  observation instead.
- **The mouse cursor shape.** `HandCursorBehavior` sets WinUI's non-public `ProtectedCursor`,
  which no other process can read, and screenshots do not capture the Windows cursor.
- **The focused element via the active-element endpoint.** Both `GET` and `POST` on
  `/session/{id}/element/active` answer 405 and 404 through this proxy (measured 2026-09-14), so
  focus is read out of the page source instead. The host `InputSiteWindowClass` pane reports
  `HasKeyboardFocus="True"` alongside the real control, so the reader takes the LAST match, not
  the first — taking the first reports a featureless pane forever and makes a working tab walk
  look broken.

Every `AutomationId` addressed here is already declared in the app's XAML. **This project adds none
and edits no `.xaml` file.**

## Notes

- **The driver reports rectangles in physical pixels, and the thresholds are written in
  device-independent units on purpose.** This display reports 120 `LOGPIXELSX`, a 1.25 scale: a
  44-DIU button measured 55 pixels on 2026-09-14. At 100% scale the two units coincide; above it
  a DIU threshold applied to a pixel measurement is the *weaker* claim, so a display change
  cannot redden these tests without the defect having genuinely returned. Same reason the
  clearance threshold is 14 and not the 23 that was measured.
- **Test classes do not run in parallel** (`AssemblyInfo.cs`). Every test owns its session, but
  the desktop's foreground keyboard focus is shared, so two collections at once would have one
  test's `Tab` land in another test's window.

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
