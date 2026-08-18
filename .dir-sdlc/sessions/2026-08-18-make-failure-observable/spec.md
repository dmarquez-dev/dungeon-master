# Session 2 -- Make failure observable in the Application core

Make a failed write impossible to miss, from the aggregate mutator that refuses it to the label the
user reads.

## Context

The audit's verdict is that this codebase's one systemic weakness is that failure information is
discarded at almost every seam it crosses. Three unrelated mechanisms in three assemblies compose
into one user-visible behaviour -- **an edit that appears to succeed and did not**:

- Domain mutators return `void` and quietly no-op on an unknown child id, so the handler returns
  `Success(true)` for an operation that never happened (`CORE-001` / MASTER-021).
- `DungeonWriteRepository.UpdateAsync` attaches a detached aggregate, which marks it `Unchanged`, so
  `SaveChangesAsync` writes nothing (`INFRA-012` / MASTER-016).
- `DungeonDetailViewModel` throws away every `Result` the dispatcher hands it, so even a correctly
  reported failure is invisible (`UI-004` / MASTER-001 -- the audit's only S1).

The master index considered merging these three and deliberately did not: no single change resolves
more than one of them. They are **ordered** rather than merged, which is why they share this session.

Around them sit the failure plumbing they travel through -- `Result<T>` permitting invalid states and
every state-changing command returning an always-true `bool`, `ErrorCategory` with no production
consumer, validation that runs synchronously and keeps only the first failure, and a dispatcher whose
hand-maintained registry and uncached reflection make the fourteen-way duplicated handler skeleton
expensive to change.

**Session 1 ("Pin the build") completed on 2026-08-18** and changed the ground rules: `.editorconfig`
fixes tabs, `IDE0055` is at `warning`, `AnalysisMode` is `Recommended`, and `TreatWarningsAsErrors`
is on -- so style regressions now break the build rather than surviving to review. Session 1 also had
a "no `src/` behaviour change" property. **This session does not.** It changes behaviour, so every
task ends in a real test run.

**Baseline measured in the worktree before any change** (not inherited from the Session 1 report):

- `dotnet test src/DungeonMaster.sln` -- **292 passed, 0 failed, 0 skipped**
- `dotnet build src/DungeonMaster.sln` -- **0 warnings, 0 errors** under `TreatWarningsAsErrors`,
  building both `net10.0-windows10.0.19041.0` and `net10.0-maccatalyst`

## Definition of done

**The three silent-success mechanisms**

- [ ] A mutator called with an unknown child id reports that it did nothing, and the handler that
      called it returns a `NotFound` failure rather than `Success(true)`. Covered by a test per
      affected command that fails on the parent commit.
- [ ] `UpdateAsync` given a detached aggregate persists the change.
      `UpdateAsync_WithDetachedAggregate_PersistsTheChange` fails on the parent commit and passes after.
- [ ] A rejected write in the dungeon editor is visible to the user as text on screen, verified by
      running the app on Windows and observing it -- not only by a source-scanning test.

**The failure plumbing**

- [ ] `Result<T>` cannot be constructed in an invalid state: `sealed`, private parameterless
      constructor, no `init` accessor reachable from outside.
- [ ] `grep -rn 'ICommand<bool>' src/DungeonMaster.Application` returns nothing.
- [ ] `ErrorCategory` has a production consumer -- all three view models route through
      `ResultMessage.ForFailure`, and `grep -rn 'ErrorMessage = result.Error' src/DungeonMaster.App`
      returns nothing.
- [ ] `ValidationBehavior` awaits `ValidateAsync`, passes the `CancellationToken`, and carries every
      failure rather than `failures[0]`.
- [ ] Every `ICommand<>` implementation in the Application assembly resolves at least one
      `IValidator<>` -- asserted by reflection, so the criterion cannot rot as commands are added.
- [ ] `grep -rn 'dynamic' src/DungeonMaster.Application` returns nothing, and the closed handler type
      is cached across dispatches.
- [ ] `AddScoped<` in `DependencyInjection.cs` drops from **21** to at most 3; handler coverage is
      asserted by enumerating command and query types rather than by a hard-coded count.
- [ ] Queries run through the pipeline, and still return bare DTOs rather than `Result<TResult>`.
- [ ] The duplicated `"Dungeon {id} not found."` construction appears once, not twelve times.

**Session-wide, every task**

- [ ] `dotnet build src/DungeonMaster.sln` -- 0 warnings, 0 errors, both App target frameworks.
- [ ] `dotnet test src/DungeonMaster.sln` -- 0 failures, total at or above 292.
- [ ] Every new file is tab-indented and **CRLF**-terminated. An LF-ended `.cs` file fails `IDE0055`
      while `git status` reports it clean -- Session 1 was bitten by exactly this.

## Scope

Sixteen master entries, closed across nine tasks:

| Task | Closes |
|---|---|
| T1 Domain mutators report outcomes | MASTER-021 (signal), 010, 003 |
| T2 `Result` + `Result<T>` / `ErrorCategory` / `ResultMessage` | MASTER-088 (Result), 045 (enum) |
| T3 `UpdateAsync` / `DeleteAsync` | MASTER-016, 088 (delete) |
| T4 One handler skeleton, `bool` to non-generic `Result`, minimal non-generic dispatch path | MASTER-091, 021 (report), 031, 088 (bool) |
| T5 `ValidationBehavior` async, token, all failures | MASTER-002, 046 |
| T6 Assembly scan, cached dispatch over three request paths, query pipeline | MASTER-089, 090, 080 |
| T7 Validators for the ten unvalidated commands | MASTER-079 |
| T8 `ErrorMessage` in the editor | MASTER-001, 045 (consumer) |
| T9 Narrow the palette's catch | MASTER-005 |

### Not in scope

- **The `DbContext` and repository lifetime** (MASTER-023), and the rest of the persistence contract
  (MASTER-015, MASTER-092). Session 3 owns those. T3 fixes **only** the `Attach`-marks-it-`Unchanged`
  tracking defect and must not touch DI registration.
- **`NotNullConverter`'s own defect** (MASTER-007 -- it reports an unresolved binding as non-null).
  Assigned to Session 5. T8 uses the converter as-is and notes the interaction.
- **The five parked design questions** -- MASTER-044, 073, 074, 081, 107. The index parks them
  pending MASTER-088's answer; they are re-asked after this session lands, not during it.
- **Giving the App project a testable seam** (MASTER-026, Session 5). This constrains T8 and T9 --
  see Open questions.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| How does a dispatched command's failure reach the user? | The existing `ErrorMessage` + `NotNullConverter` binding pattern; do not invent one | 1 | `CreateDungeonViewModel.cs:30,62`; `DungeonListViewModel.cs:32,69,91`; `CreateDungeonPopup.xaml:38,42`; `DungeonListPage.xaml:22,26` |
| Non-generic `Result`, or `Result<Unit>`? | **A genuinely non-generic `Result` alongside `Result<T>`.** No `Unit` type exists in this session | 4 | **Judgment, not a citable standard.** FluentResults and CSharpFunctionalExtensions, the two most widely used dedicated .NET Result libraries, both ship a non-generic `Result` alongside `Result<T>`; ErrorOr instead uses a placeholder (`ErrorOr<Success>`), which is the `Unit` shape. The weighted signal is MediatR -- the closest analogue to this repository's hand-rolled dispatcher -- which removed `Unit` in v12 in favour of a non-generic request path after years of shipping the `Unit` approach. Three of four favour non-generic, and the CQRS-specific precedent migrated off `Unit`. The lookup budget was spent and returned no citable answer on this specific question; **recorded as belief, not fact.** Decided by the user at Gate 2, who declined to state a preference and asked for the more accepted practice, and who accepted the resulting increase in T2, T4 and T6 effort explicitly |
| Does `Result<T>` derive from `Result`? | **No -- independent sealed records.** No shared base, no shared interface | 4 | Judgment: `sealed` is the invariant MASTER-088 exists to establish, and a base class cannot be sealed. The duplication (five members, three factories) is accepted knowingly; `ResultMessage` keeps its switch in one place via two thin overloads. Frozen in `contracts/result-and-error-surface.md` §2.1 |
| Where do two same-named types live, given one type per file? | `Result.cs` + `ResultOfT.cs`; but `ICommand.cs`, `ICommandHandler.cs` and `ValidationBehavior.cs` each hold both of their arities | 4 | Judgment: `dotnet-format.md` §2 asks for one type per file AND a filename matching the type name, and two `Result`s make those unsatisfiable together. The record types get separate files because each is substantial; the marker interfaces and the behaviours stay together because each pair is two arities of one two-to-twenty-line declaration. Recorded so a reviewer does not read either as an oversight -- `contracts/coding-standards.md` §2 lists the five files |
| Does the enum keep its name? | Yes, `ErrorCategory` | 1 | Repo already uses it; `dotnet-format.md` §3's own sample writes it |
| Do queries get validators too? | Pipeline yes, validators no -- the validation behavior is registered closed per command, so a query cannot acquire one by construction | 1 | ADR-001 ("uniformly to all commands and queries") reconciled with `backend-conventions.md` §2 ("has no validator") |
| Do queries start returning `Result<TResult>`? | No, bare DTOs | 1 | `backend-conventions.md` §2; `CORE-012` says keep the wrapper "on the command path only" |
| `UpdateAsync`: call `Update`, or throw on `Detached`? | Call `Update` | 1 | `INFRA-012`'s first suggested direction; throwing converts silent data loss into a crash on a path Session 3 is about to make reachable |
| Where does `ResultMessage` live? | Application layer, not App | 4 | Judgment: it is a presentation concern, but the App project cannot be unit-tested at all (MASTER-026) and this session's criteria must be behavioural |
| How many commands are `ICommand<bool>`? | **Ten**, not the nine `CORE-006` states | 1 | Measured: `ICommand<bool>` 10 files, `ICommand<Guid>` 4, sum 14. The record is wrong; contracts state ten and name the error |

Three further counts asserted from memory during decomposition were measured and corrected before
freezing: `AddScoped<` is **21** (not 20), `Grid.Row=` in `DungeonDetailPage.xaml` is **12** (not 11),
and `ObjectScale.Min`/`Max` are public constants to be referenced rather than literals to be copied.
All were re-verified independently by the orchestrator.

## Contracts

| Contract | What it freezes |
|---|---|
| `contracts/coding-standards.md` | Tabs, CRLF, XML documentation form, call-site wrapping |
| `contracts/result-and-error-surface.md` | `Result`, `Result<T>`, `ValidationError`, `ErrorCategory`, `ResultMessage` |
| `contracts/domain-mutation-outcome.md` | `MutationOutcome` and the per-mutator return-type table |
| `contracts/persistence-write-contract.md` | `IDungeonWriteRepository` after `DeleteAsync` returns `bool` |
| `contracts/handler-skeleton.md` | The single load / check / mutate / save shape all 14 handlers adopt |
| `contracts/dispatch-and-pipeline.md` | `IRequest<TResponse>`, the three request paths, the single cached invoker, registration by scan |
| `contracts/app-failure-surfacing.md` | The `ErrorMessage` channel and the frozen XAML edit |

## Open questions

Both were resolved by the user at Gate 2 and are recorded here as decided, not open.

1. **`ErrorCategory` member set** -- **Resolved: keep five**, matching `backend-conventions.md` §2
   and adding `Forbidden`, with `ResultMessage` as the real consumer. The drafts already said this,
   so nothing was amended. Rung 1: the conventions document names the set verbatim.
2. **`Unit` vs a genuinely non-generic `Result`** -- **Resolved: non-generic `Result`, reversing the
   draft.** Rung 4, `judgment`; the reasoning and the fact that it is belief rather than a cited
   standard are in the Assumptions table above. The user declined to state a preference, asked for
   the more accepted practice, and accepted the increase in T2, T4 and T6 effort. Contracts
   `result-and-error-surface.md`, `handler-skeleton.md`, `dispatch-and-pipeline.md`,
   `app-failure-surfacing.md` and `coding-standards.md` and the criteria of T2, T4, T5, T6, T7 and
   T8 were amended before the freeze. T1, T3 and T9 were already dispatched and were not touched;
   none of their criteria references the result shape.

One consequence of decision 2 that a reviewer should read as intended rather than accidental: **T4
adds a minimal, deliberately un-improved non-generic dispatch path** so that its ten converted
commands compile, and **T6 deletes it** in the rewrite. `dispatch-and-pipeline.md` §1.5 tabulates
exactly what T4 may add. This is ordinary sequencing cost, not scope creep, and it is the reason
T4 gained four files in its ownership set.

One limitation that is **not** an open question, but must be stated before sign-off: **MASTER-001 and
MASTER-005 cannot be closed by `dotnet test` alone.** `DungeonMaster.Tests` targets `net10.0` and
cannot reference the multi-targeted App project (MASTER-026, Session 5). T8's evidence is therefore
three-layered -- a real unit test of the decision logic, a source-scanning test in the repo's existing
`Tests/App/` idiom, and **one manual observation on Windows** the implementer performs and reports.
T9 has the same limitation and less mitigation: its code is `#if WINDOWS`, in a `Loaded` handler,
constructing WinUI types, so a manual check is the only observable evidence available.
