# Gate 3: icon-button-padding (Small, sir 1.0.34)

Session `20261112-2120-icon-button-padding`, base `a4c3c90` on `chore/repo-audit`, worked in the main checkout.

This render is the final one. It went through two Gate 3 stops: render 1 (after review round 1) asked the human checks and the advisory dispositions; render 2 (after the fix-now re-review) asked the H2 re-check, the re-review advisories and the approval.

## 1. Decisions

- E1 (routing): a new keyed `DmIconButton` style rather than per-site overrides or dropping the width minimum app-wide; all four surfaces (floor cards, dungeon cards, area cards, detail page).
- E2 (implementation): without a Disabled state, `StartSessionButtonTests.ListPageStartSessionButton_WithOneFloor_HasDisabledOpacity` failed (expected 0.35, actual 1), because the keyed style's Normal-state Opacity 1 outranked the list StartSessionButton's local 0.35. The user chose a Disabled state at 0.35. A1 amended C1, A2 amended C4.
- E3 (Gate 3, render 1): advisory 1 (comment said Padding narrows the buttons) fixed now; advisory 2 (parity test sees only Opacity setters inside states) carried; advisory 3 (dead local Opacity 0.35) fixed now, A3 amended C2. One re-review round followed.
- Render 2: re-review advisories R1 and R2 carried; approved and committed.

## 2. Criteria (final verdicts, re-review round)

| | Verdict | Evidence |
|---|---|---|
| C1 | met | `DmIconButton` declared once in Controls.Base.xaml, no BasedOn; implicit setters restated, MinimumWidthRequest 0, Disabled 0.35; comment true after the E3 rewording |
| C2 | met (as amended by A3) | 1 declaration + 8 uses; each site gained only `Style=`; the list StartSessionButton lost only `Opacity="0.35"`; ExpandToggle untouched |
| C3 | met | Implicit.xaml census "9 of the app's 21" Buttons without Style=, recounted by the reviewer two ways (regex over comment-stripped text, ElementTree); Disabled passage true after E3 |
| C4 | met (`injected defect`; (d) also `reviewer-reproduced`) | injections (a) width 44, (b) Disabled removed, (c) PointerOver 0.9, (d) Disabled 0.45 each failed 1 of 4 facts; restores sha256-identical, control fired on a one-byte change |
| C5 | met | `dotnet build src/DungeonMaster.sln` 0 Warning(s) 0 Error(s); `dotnet test` App.UnitTests 217, Tests 1004, App.Tests 146, all passed (re-run by the reviewer in the re-review round) |
| C6 | met | Python byte counts: every touched XAML file all-CRLF, BOM state unchanged; new test file 408/408 CRLF, no BOM |
| C7 | decided by human checks H1-H5 | all passed (below) |

Superseded verdicts: in review round 1 the reviewer recorded C2 met against its original wording (every other attribute unchanged, the local Opacity kept) and listed advisories 1-3; the E3 fix superseded those, as A3 records.

## 3. Change surface

`git diff --stat a4c3c90` before the records commit: 6 XAML files, 77 insertions, 7 deletions at render 1 (Controls.Base.xaml, Implicit.xaml, AreaSidebarItem.xaml, FloorSidebarItem.xaml, DungeonDetailPage.xaml, DungeonListPage.xaml), plus the new `src/DungeonMaster.Tests/App/Theme/IconButtonStyleParityTests.cs`; the E3 round edited Controls.Base.xaml, Implicit.xaml and DungeonListPage.xaml again.

## 4. Advisories and backlog

- Carried: advisory 2 (parity test, Opacity setters only), R1 (test doc comments describe the removed local 0.35 as history), R2 (ragged DmIconButton header wrap).
- Removed: the 20261112-0153-floor-name-tooltip row (card icon buttons look over-padded), done here.

## 5. Human checks and metrics

Against the main checkout working tree, launched with `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -t:Run -f net10.0-windows10.0.19041.0`:

- H1 floor card notebook and delete narrower (predicted about 24 DIU), tighter gap, full height: passed.
- H2 dungeon card edit, start session, delete narrower (predicted about 36 DIU), start session still dimmed: passed; re-observed after the local Opacity was removed: passed.
- H3 area card notebook and delete narrower (predicted about 22 DIU): passed.
- H4 detail page dungeon notebook button narrower (predicted about 22 DIU): passed.
- H5 hover dims, click works: passed.

Metrics: userInteractions 4 (routing E1, E2, Gate 3 render 1, render 2 with approval), escalationsRaised 3, reviewRounds 1.
