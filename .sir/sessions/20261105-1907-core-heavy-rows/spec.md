# core-heavy-rows

Rename `ObjectRotation`'s `CW90`/`CW180`/`CW270` with a real data migration. Then close MASTER-157 and MASTER-178
as deliberately unchanged on measured evidence, and correct the audit record's PathSegmentKind claim.

## Context

This is the backlog's "CORE heavy rows" unit (`.sir/backlog.md:9-13`, plus the CW90 clause of `:25`). Gate 1
decided its shape (`discussion.md`, Decisions). Everything below was re-measured in the spec phase on the session
worktree at `62ba566`.

- **ObjectRotation** is persisted by member name (`PlacedObjectConfiguration.cs:39-42`,
  `.HasConversion<string>()`).
  - So the rename has to rewrite the stored `PlacedObjects.Rotation` strings.
  - P-CW (C1 section 2) prints 28 lines: 3 in `ObjectRotation.cs`, 3 in `FloorMapDrawable.cs`, and 22 in 10 test
    files.
  - Rotation is cycled by ordinal (`% 4`), and the ordinals do not move.
- **PathSegmentKind** is persisted by ordinal inside the `Areas.Outline` JSON. Its members `Line` and `CubicBezier`
  are not abbreviated, so no rename is owed.
  - The audit record's "same blocker class" is imprecise, and its "a backlog item covers both renames" is false:
    `00-master-index.md:318`, and `01-domain-application.md:1712-1714`.
- **MASTER-157 / CORE-031.** The register row's status cell is empty.
  - The "233 lines" was exact at audit commit `16d594b`: 7 files.
  - It has drifted since: `Common/Dispatch` is now 8 files and 437 lines, with `Dispatcher.cs` at 209.
  - The three costs the entry names (MASTER-080/089/090) show in code:
    - a cached invoker map;
    - assembly-scan registration;
    - queries going through `BuildPipeline`.
  - One residue remains by design: handlers are still resolved from `IServiceProvider` at dispatch time.
- **MASTER-178 / CORE-035.** A validated census gives 17 production enums: 5 with `None` at zero and 12
  concrete-at-zero (C3).
  - The census strips doc comments and string literals, honours explicit `= n`, and sees nested `private` enums.
  - Its fixture control passes, and a second, line-based predicate also counts 17.
  - The reachability triage (C3 section 4) found **no concrete-at-zero default that is both reachable and a
    defect**. So no fix task follows, and all 12 close as deliberately unchanged, each with its own reason.

## Definition of done

- [ ] **D1** No `CW90`, `CW180` or `CW270` remains under `src`. Two files are excluded because they deliberately
  name the old stored strings: the new migration and its test. P-CW prints 0. The members are `Clockwise90`,
  `Clockwise180` and `Clockwise270`, at the same ordinals, with accurate `///` summaries.
- [ ] **D2** A new migration `<ID>_RenameObjectRotationClockwiseMembers` rewrites the stored `Rotation` strings:
  - `Up` changes them to the new names, and `Down` reverses that.
  - It uses SQL only and is idempotent.
  - The snapshot is regenerated, and `has-pending-model-changes` reports no change.
- [ ] **D3** A test proves the migration against a database that holds the old values (O1, O2 and O4), and proves
  the reversal (O3 and O5). It shows a red step and two injected defects (J1 and J2).
- [ ] **D4** The solution builds with 0 warnings and 0 errors, and every test passes. The migration-count ratchet
  is now 4.
- [ ] **D5** The MASTER-130 register cell and CORE-024 record the cleared CW90 family. The row stays part done.
- [ ] **D6** The PathSegmentKind persistence claim is corrected in the MASTER-130 cell and in CORE-024's
  `AMENDED 6b-xvii` block.
- [ ] **D7** MASTER-157 / CORE-031 is closed **DELIBERATELY UNCHANGED**. The close cites the re-measured size, the
  audit-commit figure (DRIFT), and the three maintenance costs verified in code.
- [ ] **D8** MASTER-178 / CORE-035 is closed **DELIBERATELY UNCHANGED**. The close records the validated census
  with its controls, and a reasoned disposition for each of the 12 enums.
- [ ] **D9** Every file a task writes keeps CRLF line endings, and no lone CR is introduced.

## Scope

- **T1:** the rename, the migration, the proof test and the ratchet.
- **T2, T3, T4:** the register and audit edits for MASTER-157, MASTER-178 and MASTER-130. These are each one status
  cell plus one inserted `AMENDED` block, and they leave all previous wording intact.
- **At Gate 3, the orchestrator** trims the closed backlog rows: `:11`, `:12`, `:13`, and the CW90 clause of
  `:25`. The `:9` heading is kept.

### Not in scope

- The `ct` and `obj` residues of MASTER-130 (EXCLUSIONS 2 and 3). The row stays part done.
- Any change to `PathSegmentKind`, or a `JsonStringEnumConverter`.
- Adding a zero member to any enum.
- Updating the global `dotnet-ef` tool.
- Editing `.dir-sdlc/`, which still carries the original false claim at `.dir-sdlc/backlog.md:457`.
- Two items go to the backlog at Gate 3:
  - **`BtnCW` and `BtnCCW`** (`ControlsPaletteView.xaml.cs:27-28`) are abbreviations outside the CW90 family.
  - **`ValidateHandlerRegistrations` is not called at app startup.** This is already a backlog row
    (`.sir/backlog.md`, "Call the `ValidateHandlerRegistrations` self-check..."), and T2's cell only states it.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Does any concrete-at-zero enum have a reachable default that is a defect? | No. All 12 are deliberately unchanged. `DrawState` is the one whose default *is* reached, at construction, but `Idle` is the correct initial state. | 1 | C3 section 4, one row per enum with `file:line`. Orchestrator and architect triaged independently. |
| Does the `dotnet-ef` version skew block scaffolding? | No. So the tool is left alone, per Gate 1. It is a **global** tool at 10.0.3, not a local one. | 1 | `dotnet tool list --local` is empty. `has-pending-model-changes` ran in the worktree and printed the skew warning, then "No changes have been made to the model since the last migration." |
| Is the rename split from the migration? | No, it is one task. A rename commit without the migration would leave a tree that cannot load stored `'CW90'` rows. | 4 | C2 section 2; `PlacedObjectConfiguration.cs:39-42` |
| What layout do hand edits to the migration use? | The scaffolded four-space layout, with a `<summary>` in the sibling's form. | 1 | `.editorconfig:49-50` marks `Migrations/*.cs` as `generated_code`; `20260818232300_*.cs` |
| Do the new files carry a BOM? | Migration files: yes, like all 7 siblings. The test file: no, like its siblings. | 1 | `head -c 3 \| od` on each (C1 section 1) |
| Was CORE-031's "233 lines" wrong when it was written? | No. It is DRIFT: 7 files and 233 lines at `16d594b`. | 1 | C4 section 1.3, by `git show 16d594b:<path> \| wc -l`, with a HEAD control that prints 437 |
| Can tasks build concurrently? | Not in one worktree, but only T1 runs `dotnet`. So no builder pair needs ordering. | 1 | C1 section 9; the four tasks' criteria |
| Do injected defects collide with peers? | No. J1 and J2 live only in T1's migration file. T2 and T3 read only the Application layer and enum declarations. | 4 | C2 section 5 |
| How do T2, T3 and T4 share the two audit files? | Serially, T2 → T3 → T4, through `dependsOn`. T4 also waits for T1 so it can cite the migration ID. | 1 | `manifest.json`; the ownership intersection lists only these three pairs |

## Contracts

- [C1 — measurement instruments](contracts/C1-measurement.md)
- [C2 — rotation rename and migration](contracts/C2-rotation-migration.md)
- [C3 — enum census and dispositions](contracts/C3-enum-census.md)
- [C4 — register and audit updates](contracts/C4-register.md)

## Open questions

None.
