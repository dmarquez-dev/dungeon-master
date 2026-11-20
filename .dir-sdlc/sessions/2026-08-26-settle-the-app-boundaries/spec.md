# Settle the App boundaries

Give the App layer a testable non-UI core, give the control kit a contract that is its own, and turn
the macCatalyst ambiguity into a written decision.

## Context

This is **sub-session 5a** of Session 5 of the repository audit. Session 5 — "Give the editor a seam
and the control kit its own contract" — is 32 audit entries, the largest remaining, and was split at
Gate 1 into three sub-sessions: **5a settles the boundary questions**, 5b takes the editor seam, 5c
takes the control kit. 5a goes first because its two decisions constrain the other two sub-sessions
and Session 6.

Five audit entries are in scope, and no others:

| Entry | Source record | What it says |
|---|---|---|
| `MASTER-026` | `XCUT-002` | `DungeonMaster.Tests` targets `net10.0`; `DungeonMaster.App` targets `net10.0-windows10.0.19041.0` and `net10.0-maccatalyst`. They share no framework, so no test can reference App. |
| `MASTER-030` | `UI-021`, `UI-036`, `XCUT-004` | `net10.0-maccatalyst` is a shipped target framework with no run profile, no solution platform, and five `TODO(macOS)` input paths. |
| `MASTER-027` | `UIKIT-005` | Three control-kit types name `AreaDto`, `FloorDto` and `PlacedObjectDto`, so the kit's public API is one query's projection. |
| `MASTER-082` | `CORE-020` | `Common/Map`'s three geometry types have no caller inside `DungeonMaster.Application`. |
| `MASTER-087` | `XCUT-005` | The Application assembly's public surface is three unrelated things behind one project reference, and nothing states what App may touch. |

Four prior sessions have landed on `chore/repo-audit`. Session 1 pinned the build; Session 2 made
failure observable; Session 3 settled the `DbContext` lifetime; Session 4 took async, disposal and
threading discipline in the presentation layers. Session 4 rescheduled four entries to Session 6 —
`MASTER-006`, `MASTER-013`, `MASTER-055`, `MASTER-062` — for one reason: **no test in this repository
can execute App code**, so the only available evidence was a structural assertion over source text,
which the user judged insufficient for that defect class. `MASTER-159`, a user-facing defect found by
that session, is in the same position. This session builds the mechanism those five are waiting on.
It fixes none of them.

## Definition of done

| # | Item |
|---|---|
| D1 | `TileCoordinateHelper`, `SnapZoneCalculator` and `MapDrawingSession` no longer live in `DungeonMaster.Application`. (`MASTER-082`) |
| D2 | The Application layer's public surface is stated in a committed document, and what is not that surface has been moved. (`MASTER-087`) |
| D3 | A `net10.0` class library `DungeonMaster.App.Core` exists, is referenced by `DungeonMaster.App` and by `DungeonMaster.Tests`, and references no MAUI package. (`MASTER-026`, half a) |
| D4 | A test project referencing `DungeonMaster.App` exists and **executes** MAUI-typed App code: at least one test constructs a type whose compilation requires a MAUI type, calls a member, and asserts on the result — demonstrated by a passing run *and* by an injected wrong value producing a named failure. (`MASTER-026`, half b) |
| D5 | `AreaSidebarItem`, `FloorSidebarItem` and `FloorMapDrawable` no longer name `AreaDto`, `FloorDto` or `PlacedObjectDto`; the kit binds its own presentation contracts, mapped from the query DTO at one boundary; the feet-per-tile arithmetic leaves the render method. (`MASTER-027`) |
| D6 | `docs/architecture-decisions.md` holds `ADR-001`, recording macCatalyst as build-verified and not run-verified, naming the five `TODO(macOS)` paths as an accepted gap, recording `UI-036` and the missing solution platform as accepted unchanged, and stating a revisit condition. (`MASTER-030`) |
| D7 | No regression: `src/DungeonMaster.sln` builds at 0 warnings, both App target frameworks build at 0 warnings under `TreatWarningsAsErrors`, and `DungeonMaster.Tests` still reports `Failed: 0`. |

## Scope

Four tasks. T4 is disjoint from the rest and runs concurrently; T1 → T2 → T3 is a strict chain,
because all three write into `src/DungeonMaster.App.Core/**` and the last two share three App files.
**That ordering is load-bearing, not a preference** — relaxing it makes the decomposition wrong.

| Task | Agent | Does |
|---|---|---|
| T1 | `dir-architect` | Stands up both new projects, registers them in the solution, wires the project references, and writes the first test that executes MAUI-typed App code |
| T2 | `dir-backend-engineer` | Moves the three geometry types out of Application into `App.Core`, moves the feet-per-tile arithmetic out of the render method, writes `docs/app-application-seam.md` |
| T3 | `dir-frontend-engineer` | Gives the kit `AreaItem` / `FloorItem` / `PlacedObjectItem` and a mapper, and retypes the three controls onto them |
| T4 | `dir-architect` | Writes `ADR-001` |

### Not in scope

- **Implementing any of the five `TODO(macOS)` input paths.** Escalation E1 parked macCatalyst
  explicitly. Nothing here can run a macCatalyst build, so a criterion demanding observed macCatalyst
  runtime behaviour would be unsatisfiable, and none is written.
- **Deleting the `net10.0-maccatalyst` target framework.** Same decision.
- **Fixing anything the new test mechanism makes testable.** `MASTER-006`, `MASTER-013`,
  `MASTER-055`, `MASTER-062` belong to Session 6; `MASTER-159` and `MASTER-161` to sub-session 5c.
  T1 modifies no file under `Controls/`, `Behaviors/` or `Graphics/`.
- **The `Plugin.Maui.KeyListener` evaluation** parked in `.dir-sdlc/backlog.md`. E1's answer unblocks
  it — keeping the target settles the input the evaluation was waiting on — but performing it is not
  in these five entries, and it stays in the backlog.
- **Correcting `docs/superpowers/plans/2026-08-05-theme-systemisation.md:22`**, which claims the
  macCatalyst TFM cannot build on this machine and is stale. `git check-ignore -v` reports
  `.gitignore:40 docs/superpowers/` — the file is gitignored, so a correction could not appear in
  this session's diff or commit range.
- The editor seam (5b) and the rest of the control kit (5c).

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Does `net10.0-maccatalyst` actually build on this machine? | Yes, clean | 1 | `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-maccatalyst` → `Build succeeded. 0 Warning(s). 0 Error(s).`, 14.32s; `dotnet workload list` shows `maccatalyst 26.5.10301/10.0.100` |
| Where does the ADR go? | `docs/architecture-decisions.md`, one numbered document | 1 | No ADR directory exists (`find . -iname "*adr*"` returns three unrelated `…ReadRepository…` files); `dm-conventions/architecture/architecture-decisions.md` is itself one file holding `ADR-001`–`ADR-009`; `docs/persistence-contract.md` is the precedent for a decision doc directly under `docs/` |
| Can `DungeonMaster.App.Tests` be multi-targeted to both App frameworks? | No — Windows only | 3 | Measured in a throwaway spike: adding `net10.0-maccatalyst` → `NU1201: Project DungeonMaster.App is not compatible with net10.0-maccatalyst26.0`; pinning to 26.5 → `RuntimeComponentManifest.targets(3,5): error : Empty ResolveFrameworkReference.RuntimePackPath`. No macCatalyst test runtime pack exists here |
| Where does the DTO → Item mapping happen? | `DungeonDetailViewModel` | 1 | It is the single site receiving `DungeonDetailDto` from the dispatcher, so there is one call site to audit |
| Do `UI-036` and `XCUT-004`'s missing solution platform change any file? | No — both become things the ADR records | 1 | `UI-036` states "No action needed as a file change"; under a parked decision a solution platform nobody can build against would add the appearance of support |
| Does `MASTER-087`'s "state what the surface is" half need an artifact? | Yes — `docs/app-application-seam.md` | 1 | `docs/persistence-contract.md` is the existing precedent for exactly this |

## Contracts

- `contracts/new-projects.md` — both new projects' exact build shape, each non-obvious property
  paired with the build failure it defeats
- `contracts/app-core-map.cs` — the `DungeonMaster.App.Core.Map` surface after the move, the
  behaviour-preserving rule, and the new `TileDistance.InFeet`
- `contracts/app-core-presentation.cs` — `AreaItem` / `FloorItem` / `PlacedObjectItem` /
  `DungeonDetailPresentationMapper`
- `contracts/maccatalyst-decision.md` — where `ADR-001` goes, its structure, and every fact it carries

**One conflict between two audit records is resolved here rather than by an implementer.**
`UIKIT-005` suggests moving the feet-per-tile arithmetic "to Application alongside the other map
helpers". `CORE-020` moves those helpers *out* of Application. `MASTER-082` wins: the helpers leave
Application, and the feet arithmetic follows them into `App.Core` as `TileDistance.InFeet`.

## Open questions

None. Both escalations (E1 macCatalyst, E2 the `MASTER-026` shape) were raised and answered before
decomposition and are recorded in `state.json`.
