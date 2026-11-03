# CORE doc-comment corrections

Make six comments and XML doc blocks in the CORE layer (and one in `DungeonMaster.App.Core`) true and in house style. Only comment and doc text changes. No executable code changes.

## Context

Gate 1 chose six `.sir/backlog.md` rows, D1-D6 (`discussion.md`, selected rows table). The spec phase re-measured each one on the base commit `d2b84a2` (`chore/repo-audit`). Instruments, fixtures and baselines are in `contracts/C1-measurement.md`.

| # | Backlog | Measured at base | Verdict |
|---|---|---|---|
| D1 | `:17` (MASTER-180 / CORE-036) | CORE-036's predicate (C1 P1) reproduces its enumerated list line for line: `CanvasPoint.cs` 4 9-12 16-18 20 21 23 24 28-30 41-43 60-64 92, `PlacedObject.cs` 50 97 98 102, `ObjectKind.cs` 10. P2, the same predicate without P1's exemption for lines ending `>`, adds `CanvasPoint.cs:19`, which breaks a sentence after `<see cref="float.PositiveInfinity"/>`. CORE-036 (`01-domain-application.md:2801-2803`) says `:19` conforms. That is false. | live |
| D2 | `:41` | `ResultOfT.cs:36-41` says `Result<Guid>` is "the only closed construction this application uses". C1 §7 S1: outside the `src/*Tests/` projects the only closed form is `Result<Guid>` (10 sites). S2 (DI and type-argument closures, including `DependencyInjection.cs:67`): Guid only. The test project closes it over `int` (`DispatcherCancellationTests.cs:155`). So the claim holds for production code only, and the comment does not say so. | live |
| D3 | `:42` | `CanvasPoint.cs:18` still says a `float.NaN` "throws on save — a `DbUpdateException` wrapping…", and `:25` still says "So the store stops one of three cases, at commit time." The finiteness guard rejects NaN at construction, so that EF path is unreachable. `CanvasPointNonFinitePersistenceTests.cs:55-62` already records that the EF wrapper "can no longer be observed". | live |
| D4 | `:46` | `Area.cs:24` "it is still built at most once per Area". `??=` (`Area.cs:76`) is a non-atomic check-then-assign. The architect found the same claim at `:12-13` ("built once and reused, so Outline hands back the same instance on every read"). | live; `:12-13` added to scope at Gate 2 |
| D5 | `:48` | The source row (`.dir-sdlc/backlog.md:517`) named `src`-wide `HasCheckConstraint` and `MaximumLength` annotations. Neither survives: `grep -rn "HasCheckConstraint" src/DungeonMaster.Infrastructure/Persistence/Configurations` finds none. `git log -S"HasCheckConstraint' src"` on that directory shows them removed by `4be3fc0`, `34f9feb` and `375037d`. The one surviving annotation, `` `grep -rn 'CHECK' src/DungeonMaster.Infrastructure/Migrations` returns 0 ``, still returns 0 when run. Control: the same command over `.../Configurations` returns 6. Its population excludes the commenting files, so FORM 17 does not apply. The source row itself calls this clause valid. | **stale**: closed with evidence, no task |
| D6 | `:199` | `AreaOutlineGeometry.Translate`'s doc block has `<exception>` for `ArgumentNullException` (`:25`) and `ArgumentException` (`:29`) only. It rebuilds each segment via `new PathSegment(segment.Kind, …)` (`:46-47`), and `PathSegment.cs:80-85` throws `ArgumentOutOfRangeException(nameof(Kind), …)` for an undefined kind. | live |

Every edited file is 100% CRLF with no lone CR at base (C1 §6). The build does not validate `cref` targets: no project sets `GenerateDocumentationFile`. So `cref` correctness is a reading criterion.

## Definition of done

- [ ] **D1.** P2 prints nothing on `CanvasPoint.cs`, `PlacedObject.cs` and `ObjectKind.cs`, and the doc word stream (C1 W) of each is unchanged apart from D3's paragraph.
- [ ] **D2.** `Value`'s second `<para>` scopes the closed-construction claim to production code or names its survey. Every statement it makes holds under C1 S1/S2, it does not falsify itself (FORM 17), its technical content is kept, and it obeys `dotnet-format.md:378`.
- [ ] **D3.** `CanvasPoint`'s second remarks `<para>` no longer describes a commit-time `DbUpdateException`. It states the guard. It keeps the ADO-provider attribution, the REAL/TEXT point and the infinity/NaN asymmetry, and it obeys `:376`/`:378`.
- [ ] **D4.** `Area`'s `_outlineView` comment makes no unqualified once-only or same-instance claim, adds no unverified threading claim, and keeps its MASTER-114 rationale.
- [ ] **D5.** Closed as stale at Gate 3 by the orchestrator, with the evidence in Context.
- [ ] **D6.** `Translate`'s doc block carries a true `<exception cref="ArgumentOutOfRangeException">` entry in house form, and the diff is insertion-only.
- [ ] **Register (T3, kept at Gate 2).** MASTER-180 is marked **done** in `docs/audit/00-master-index.md`. An `AMENDED` block on CORE-036 records the closure and corrects the `:19 conforms` claim.
- [ ] **No executable code change.** C1 N's non-comment-line hash is unchanged for every edited `.cs` file.
- [ ] **Line endings.** C1 B prints CR = LF = CRLF, loneCR = 0, for every edited file.
- [ ] **Build and tests.** `dotnet build` of `src/DungeonMaster.sln` prints `0 Warning(s)` and `0 Error(s)`. `dotnet test --no-build` prints 0 failed, at 187 / 943 / 139.
- [ ] **Backlog.** D1-D6 removed from `.sir/backlog.md` at Gate 3 (orchestrator). The out-of-scope sentence breaks below are added to it.

## Scope

D1-D6 as above, with D4 covering both `Area.cs:12-13` and `:24`. The edits touch six `.cs` files (T1, T2) and two `docs/audit/` markdown files (T3).

### Not in scope

- Other `:378` sentence breaks P2 finds in files this session edits: `ResultOfT.cs:27, :32, :33` and `AreaOutlineGeometry.cs:6, :12, :22, :26, :73-76, :138` (line numbers at `d2b84a2`). Routed to `.sir/backlog.md` at Gate 3, named explicitly in the review phase's ask (lesson of 20261102-2225).
- Backlog `:45` (guard or pin `Area`'s `_outline` invariant), which is a code and test change.
- The App-layer `///` reflow rows (backlog `:77`, `:78`) and the remaining test-strength rows.
- Any executable code. A row that turns out to need code goes to the backlog.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Is CORE-036's line list still current? | Yes. P1 reproduces it line for line at `d2b84a2`, and P2 adds `CanvasPoint.cs:19`. | 1 | C1 §2 table; run in this phase with the fixture1/fixture2 controls |
| What does "re-break at a sentence boundary" require? | No sentence spans two `///` lines. A line may hold several whole sentences. No word changes (C1 W hash). | 1 | `dm-conventions/backend/dotnet-format.md:376-378` |
| Does D2's universal hold anywhere? | In production code only: `Result<Guid>` directly and through `ICommand<Guid>`/`ICommandHandler<_, Guid>`/`SendAsync<Guid>`. Tests close over `int`. | 1 | C1 §7 S1 (controls: `FakeAssetSource.cs` → 0, `ResultOfT.cs` → 5) and S2 |
| Is D5 live? | No. Stale, closed with evidence at Gate 3. | 1 | Context row D5 |
| Does closing D1 update the audit register? | Yes, as task T3, which the user kept at Gate 2. DIR precedent sets the status cell and appends an `AMENDED` block (4178924, bbb3718, c7f1b96, 65acd5a). No SIR session has yet closed a MASTER-sourced row. | 1 + judgment | `git log -- docs/audit/00-master-index.md`; `20261029-1708-dir-backlog-to-sir/spec.md:49` (migration did not edit the register) |
| Worktree root | `C:/dm-wt/<id>`, not the `.claude-worktrees/` default. `.sir/config.json` has no `worktreeRoot`. | 1 | `.sir/lessons.md` worktree-root lesson; Gate 1 decision |
| Can T1 and T2 build at once? | No. They share one `obj/`/`bin/` tree, so T2 `dependsOn` T1 (applied at Gate 2). | judgment | C1 §10 |
| Is a commit hash cited in the register? | No. Tasks run without git. DIR form "No commit hash". | 1 | `00-master-index.md:368` (existing cell) |

The table was read against `contracts/C1-measurement.md`. No row contradicts it. C1 defines instruments, not behaviour, so the rows are mostly about scope.

## Contracts

- `contracts/C1-measurement.md`: instruments P1, P2, W, I1, N, B, S1/S2, X, R, with fixtures, controls and baselines at `d2b84a2`. It declares no API operation.

## Open questions

None outside the Gate 2 decisions recorded in `log.md`.
