# core-two-row-sweep

Close MASTER-110 by giving `Floor` and `PlacedObject` the two-file entity split, and finish MASTER-180's two
`///` re-break follow-ups, with no behaviour change.

## Context

Base: `chore/repo-audit` at `11f9562`, worktree `C:/dm-wt/20261105-0106-core-two-row-sweep`, clean.

- **MASTER-110 / CORE-022** (`docs/audit/00-master-index.md:298`, `docs/audit/01-domain-application.md:1537`).
  `dotnet-format.md` §11 (`:552-563`) says an entity has two files: `{Entity}.cs` for properties and
  constructors, and `{Entity}Methods.cs` for behaviour. `grep -n partial src/DungeonMaster.Domain/Entities/*/*.cs`
  shows `Area` and `Dungeon` with two partials each. `Floor` has three, because of `FloorObjectMethods.cs`.
  `PlacedObject.cs` is not partial: `Update` and `ValidateScale` sit beside its properties. Gate 1 chose
  **conform both**, with no change to dm-conventions.
- **MASTER-180 follow-ups.** `dotnet-format.md:378` says "Line breaks occur only at sentence boundaries."
  The P2 predicate from `20261103-1947-core-doc-comments` C1 §2 was re-run this phase, after its two fixtures
  printed the expected lines. It flags:
  - `ResultOfT.cs` :27 :32 :33, and `AreaOutlineGeometry.cs` :6 :12 :22 :26 :76-79 :141. This matches
    `.sir/backlog.md:191` exactly.
  - `ShellRoutes.cs` :4 :10 :16, and `ObjectPaletteView.xaml.cs` :49 :50 :309 :310. This covers `.sir/backlog.md:65`.
    `:309-310` is beyond the `LoadFailed` block that row names; see Assumptions.
- All seven target files are all-CRLF, with zero lone CR and no BOM. That was measured by a byte census;
  the control `printf 'a\r\nb\rc\n'` printed CR=2 LF=2 CRLF=1.
- Verification probe in the worktree: `dotnet build …/src/DungeonMaster.sln` printed `Build succeeded. 0 Warning(s) 0 Error(s)`.
  `dotnet test --no-build` passed App.UnitTests 187, Tests 953 and App.Tests 123, with 0 failed. `git status`
  was clean after the build.

## Definition of done

- [ ] **D1** `src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs` no longer exists.
  `grep -n partial src/DungeonMaster.Domain/Entities/*/*.cs` prints exactly the eight lines in C1 §10, two per
  entity, for `Area`, `Dungeon`, `Floor` and `PlacedObject`.
- [ ] **D2** `FloorMethods.cs`, `PlacedObject.cs` and the new `PlacedObjectMethods.cs` each equal the C2
  composition. Instrument E (C1 §9) prints the three digests C1 §9 lists.
- [ ] **D3** No member line of `Floor` or `PlacedObject` was added, removed or altered. Instrument M (C1 §8)
  over each entity's partial files prints its base-ref digest (`f3b9a065…` and `ace0efdf…`).
- [ ] **D4** `ResultOfT.cs` and `AreaOutlineGeometry.cs` have no `///` line that ends mid-sentence or continues
  one: P2 and S print nothing. No word or non-`///` line changed, and indentation is unchanged: W, K, I1 and L
  equal their base values.
- [ ] **D5** The same holds for `ShellRoutes.cs` and `ObjectPaletteView.xaml.cs`, `:309-310` included.
- [ ] **D6** Every changed or new `.cs` file is CRLF throughout: B (C1 §7) gives CR = LF = CRLF and loneCR 0.
  `PlacedObjectMethods.cs` has no BOM.
- [ ] **D7** After each code task, the solution builds with 0 warnings and 0 errors. The tests pass 187, 953 and 123,
  with 0 failed.
- [ ] **D8** The MASTER-110 status cell (`00-master-index.md:298`) reads **done** and carries closure evidence the
  writer measured itself. The row still has 11 unescaped `|`, and no other line of the file changes.
- [ ] **D9** CORE-022 carries an `AMENDED 20261105-0106-core-two-row-sweep` block in house form. It records the
  closure and classifies the stale "73 / 305 lines" figures as DRIFT, with the `16d594b` evidence. Nothing in
  the file is deleted.
- [ ] **D10** The session's commits touch only the ten owned paths. There is no `.csproj`, test-file, `Floor.cs` or
  namespace change.
- [ ] **D11** `.sir/backlog.md` loses the MASTER-110 rows (`:9`, `:34`) and the follow-up rows (`:65`, `:191`).
  The `:7` heading is kept.

## Scope

| Task | Agent | Owns | After |
|---|---|---|---|
| T1 | backend | `Floors/{FloorMethods,FloorObjectMethods,PlacedObject,PlacedObjectMethods}.cs` | — |
| T2 | backend | `ResultOfT.cs`, `AreaOutlineGeometry.cs` | T1 |
| T3 | frontend | `ShellRoutes.cs`, `ObjectPaletteView.xaml.cs` | T2 |
| T4 | architect (docs) | `docs/audit/00-master-index.md`, `docs/audit/01-domain-application.md` | T1 |

T1, T2 and T3 are chained because they share one `obj/` (C1 §13). T4 never builds, so it may run beside T2 and T3.
The orchestrator does D10 and D11 at Gate 3.

### Not in scope

- Test files. `FloorObjectMethodsTests.cs` keeps its name; test classes do not mirror source files one to one.
- The MASTER-180 register cell and CORE-036. Both are already closed.
- The audit's historical mentions of CORE-022 at `01-domain-application.md:2302` and `:2940-2941`. The new
  block supersedes them without editing them.
- Two existing I1 deviations: single-line `<exception>` text at `Floor.cs:38` and `FloorMethods.cs:13`, which
  C2 keeps byte for byte. Offered for the backlog at Gate 3.
- The backlog row edits are the orchestrator's, made at Gate 3. No task makes them.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Does `PlacedObjectMethods.cs` go in a new `Entities/PlacedObjects/` folder? | No. It stays in `Entities/Floors/` with the same namespace. A namespace move would rewrite EF snapshot type-name strings. | 1 | `PlacedObject.cs:3`; backlog `:85` (48 snapshot type-name strings) |
| Where do the folded object methods go in `FloorMethods.cs`? | After `UpdateAreaOutline`, in their original order, with one blank line between. | 4 | C2 §2; keeps source order, gives E one answer |
| Does `PlacedObjectMethods.cs` carry a base type or type summary? | Neither. | 1 | `FloorMethods.cs:5`, `AreaMethods.cs:5`, `DungeonMethods.cs:5` |
| Does backlog `:65` cover `ObjectPaletteView.xaml.cs:309-310` (`BuildThumbButton`)? | Yes. The row's goal is "every `///` line is a whole sentence", so the whole file is in scope. | 4 | `.sir/backlog.md:65`; P2 output above |
| Were CORE-022's "73 lines" and "305 lines" wrong when written? | No, so they are DRIFT. At `16d594b` (the commit that wrote them, by `git log -S`), `git show 16d594b:…/Floor/<f> \| wc -l` gives `Floor.cs` 73, `FloorMethods.cs` 148 and `FloorObjectMethods.cs` 157 (148 + 157 = 305). The control, at `11f9562`, gives `Floor.cs` 94. | 1 | orchestrator measurement, recorded in C3 §2 item 2 |
| May two tasks build at once? | No. They share `obj/` and `bin/`, so the code tasks are chained. | 1 | C1 §13 |
| Must test files change with the fold? | No. | 1 | `ls src/DungeonMaster.Tests/Domain/Entities/*/`: e.g. `Dungeon/DungeonAggregateTests.cs`, `Floor/FloorNumberGuardTests.cs` mirror no source file |
| Is MASTER-180's cell edited? | No. It is already done. | 1 | `00-master-index.md:368` |

## Contracts

- [C1-measurement.md](contracts/C1-measurement.md): the instruments P2, S, W, K, I1, L, B, M, E, PC and R, with
  their fixtures, controls and base values, plus the editing hazards (§14).
- [C2-entity-split.md](contracts/C2-entity-split.md): the exact target composition of the three entity files.
- [C3-register.md](contracts/C3-register.md): the MASTER-110 status cell and the CORE-022 amendment block.

## Open questions

None.
