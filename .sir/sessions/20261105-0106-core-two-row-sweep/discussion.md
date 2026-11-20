# core-two-row-sweep

Close MASTER-110 by bringing `Floor` and `PlacedObject` to the two-file entity split in
`dm-conventions/backend/dotnet-format.md:556-563`. In the same session, take the two remaining
MASTER-180 follow-ups: re-breaking sentence-splitting `///` lines at `.sir/backlog.md:191` (CORE
side) and `:65` (UI side).

## Context

This is the backlog's next headed unit, "CORE two-row sweep: MASTER-110, 180" at `.sir/backlog.md:7`.
The discuss phase re-measured it at 2026-11-05T01:07Z on `chore/repo-audit` at `11f9562`, with a
clean tree:

- **MASTER-110 is open.** The register row is at `docs/audit/00-master-index.md:298` and the finding
  is `CORE-022` at `docs/audit/01-domain-application.md:1537`. `grep -n partial` over
  `src/DungeonMaster.Domain/Entities/*/*.cs` shows `Floor` with three partials: `Floor.cs:8`,
  `FloorMethods.cs:5` and `FloorObjectMethods.cs:5`. `Area` and `Dungeon` each have two.
  `PlacedObject.cs` is not partial. `wc -l` gives `Floor.cs` 94, `FloorMethods.cs` 176,
  `FloorObjectMethods.cs` 112 and `PlacedObject.cs` 112. The backlog carries this row twice, at
  `.sir/backlog.md:9` and `:34`. Both close with it.
- **MASTER-180 is already done** (20261103-1947-core-doc-comments; register cell at
  `docs/audit/00-master-index.md:368`). Two follow-ups are left. `.sir/backlog.md:191` covers
  `ResultOfT.cs:27, :32, :33` (`src/DungeonMaster.Application/Common/Results/`) and
  `AreaOutlineGeometry.cs:6, :12, :22, :26, :76-79, :141` (`src/DungeonMaster.App.Core/Map/`), at
  `6e1493f`. `.sir/backlog.md:65` covers `ShellRoutes.cs` and the preserved `LoadFailed` block in
  `ObjectPaletteView.xaml.cs`, per `dotnet-format.md:378`. These line numbers were not re-measured
  in discuss. The spec phase must re-measure them at its own base.

## Decisions

- Track: Standard. Name: `core-two-row-sweep`.
- MASTER-110 remedy is **conform both**. Fold `FloorObjectMethods.cs` into `FloorMethods.cs` and
  delete the third partial. Make `PlacedObject` a `partial` class split into `PlacedObject.cs`
  (properties and constructors) and `PlacedObjectMethods.cs` (behaviour). There is no behaviour
  change and no change to dm-conventions.
- Take `.sir/backlog.md:191` (ResultOfT.cs, AreaOutlineGeometry.cs `///` re-break) in scope.
- Take `.sir/backlog.md:65` (ShellRoutes.cs, ObjectPaletteView.xaml.cs `LoadFailed` block reflow)
  in scope. The user accepted the widening into the App/UI layer.
- Close the MASTER-110 register cell and amend `CORE-022` with closure evidence. Remove
  `.sir/backlog.md:9`, `:34`, `:65` and `:191` at Gate 3, and keep the `:7` heading as earlier
  sessions did.
- Read `dm-conventions/backend/dotnet-format.md` before any formatting work. Respect the CRLF and
  lone-CR lessons in `.sir/lessons.md` (no `sed -i`, no `Write` on existing CRLF files, and no
  `Edit` new_string ending on whitespace).
