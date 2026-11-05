# core-heavy-rows

Take the backlog's "CORE heavy rows" unit (`.sir/backlog.md:9`). First, rename `ObjectRotation`'s
`CW90`, `CW180` and `CW270` members, with an EF data migration. Second, triage MASTER-178's
concrete-at-zero enums and fix the ones where it matters. Third, close MASTER-157 as a decision to
keep the dispatcher.

## Context

The discuss phase re-measured this unit at 2026-11-05T19:08Z on `chore/repo-audit` at `62ba566`.
The tree was clean (`git status --short` printed nothing).

- **The ObjectRotation rename (MASTER-130 EXCLUSION, `.sir/backlog.md:11` and `:25`).**
  - The enum is at `src/DungeonMaster.Domain/Entities/Floors/ObjectRotation.cs`. Its members are
    `None`, `CW90`, `CW180` and `CW270`, each with a `///` summary.
  - It is persisted by member name. `PlacedObjectConfiguration.cs:40-42` maps `obj.Rotation` with
    `.HasConversion<string>()`, so renaming a member rewrites the stored strings.
  - `grep -rn 'CW90\|CW180\|CW270' src --include=*.cs | grep -v '/obj/\|Migrations' | wc -l`
    printed 28. The same grep over `*.xaml` printed 0.
  - The newest migration is `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault`.
- **The PathSegmentKind half of the row rests on a false premise.**
  - Its members are `Line` and `CubicBezier` (`src/DungeonMaster.Domain/ValueObjects/PathSegmentKind.cs`).
    Neither name is abbreviated.
  - It is not persisted by name. It is serialized inside the `Areas.Outline` JSON column through
    `JsonSerializer` with `new(JsonSerializerDefaults.General)` (`AreaConfiguration.cs:16`, `:116-131`).
  - `grep -rn 'JsonConverter\|JsonStringEnum' src --include=*.cs` printed nothing, so the enum
    serializes as an ordinal.
  - The claim that "both enums are persisted by member name" (`.sir/backlog.md:11`, from
    `.dir-sdlc/backlog.md:457`) is therefore false for PathSegmentKind. That row's own measurement
    covered only `PlacedObjectConfiguration`. `.sir/backlog.md:13` already states the ordinal fact
    correctly.
- **MASTER-157 / CORE-031.**
  - The register row is at `docs/audit/00-master-index.md:345`, and its status cell is empty.
  - `wc -l src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs` printed 209. The row says
    233.
  - The backlog row (`.sir/backlog.md:12`) says the three maintenance costs the finding names are
    done. That claim is still unverified, so the spec must re-measure it.
- **MASTER-178 / CORE-035.**
  - The register row is at `docs/audit/00-master-index.md:366`. It is open as a standing record:
    14 of 17 production enums were concrete-at-zero, and 12 were deliberately left unchanged.
  - `grep -rlE '^\s*(public|internal)? *enum ' src --include=*.cs`, excluding `/obj/` and
    `Tests`, printed 15. That counts files, not enums, and its predicate has not been validated.
    The spec must re-count the enums with a validated census. Per `.sir/backlog.md:172-174`, the
    census must strip doc comments, must see explicit `= n` values and non-public enums, and must
    be controlled.

## Decisions

- Track: Standard. Name: `core-heavy-rows`.
- **ObjectRotation:** rename `CW90`, `CW180` and `CW270` to `Clockwise90`, `Clockwise180` and
  `Clockwise270`.
  - Update every reference and the `///` summaries.
  - Add a real EF migration whose `Up` rewrites the stored `Rotation` strings (`'CW90'` →
    `'Clockwise90'`, and so on) and whose `Down` reverses them. Regenerate the model snapshot.
  - Prove the migration against a database that holds the old values.
  - Update the local `dotnet-ef` tool first if the version skew (`.sir/backlog.md:95`) blocks
    scaffolding. Otherwise leave it.
- **PathSegmentKind:** no code change.
  - Correct the false "persisted by member name" claim in the audit record wherever MASTER-130 or
    CORE-024 repeats it, and in the backlog rows at Gate 3.
  - Consider PathSegmentKind only under the MASTER-178 triage below.
- **MASTER-157:** keep the hand-rolled dispatcher and close CORE-031 as **deliberately unchanged**.
  This is a register and audit edit only, with no code change.
  - The status cell must cite the re-measured line count.
  - It must verify the backlog's claim that the three named maintenance costs are done before
    relying on it.
- **MASTER-178:** re-measure all production enums with a validated census, then classify each
  concrete-at-zero enum.
  - If `default(T)` is actually reachable and is a defect, add `None = 0` (or the equivalent). If
    the enum is persisted, ship a data migration with it. This applies to PathSegmentKind's
    ordinal in `Areas.Outline` JSON, and to any enum stored by ordinal.
  - Otherwise, close the enum as deliberately unchanged with a stated reason per enum.
  - The row closes when every enum has a disposition.
  - If the triage finds a fix that is large or risky, the spec raises it at Gate 2 rather than
    widening silently.
- Update the MASTER-130 register cell for the cleared `CW90` family. The `ct` and `obj` residues
  are out of scope, so the row stays part done.
- At Gate 3, remove or trim the closed backlog rows: `:11` rename, `:12` MASTER-157, `:13`
  MASTER-178, and the CW90 clause of `:25` MASTER-130. Keep the `:9` heading, as earlier sessions
  did.
- Read `dm-conventions/backend/dotnet-format.md` before any formatting work.
- Respect the CRLF and lone-CR lessons in `.sir/lessons.md`:
  - No `sed -i`.
  - No `Write` on existing CRLF files.
  - No `Edit` new_string that ends on whitespace.
  - Create session worktrees under `C:/dm-wt/` (`.sir/config.json`).
