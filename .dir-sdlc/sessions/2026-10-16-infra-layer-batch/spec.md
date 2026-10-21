# Session 6b-xx — INFRA layer batch

Close all six open `INFRA` rows of the code-quality audit register, taking the documentation branch on both
S5 correctness rows, so the layer finishes at zero open.

## Context

The `chore/repo-audit` arc has closed 158 of 181 register rows across 27 sessions. `INFRA` is one of five
layers with open rows and has the best effort ratio on the board: four `XS` rows plus two `S` rows. The 6b-xx
orientation brief left one question open and marked it **NOT MEASURED** — whether either correctness row
needs a new EF migration, which decides whether `INFRA` is a four-row sweep or a schema session.

**That question is now settled: neither row needs a migration.** `MASTER-152` has a documentation branch its
own entry prefers; `MASTER-156` cannot be unique-ified at `effort: S` at all, because the floor-reorder
command passes through a duplicate numbering state inside one `SaveChanges` and SQLite has no deferrable
unique index. So `INFRA` is a six-row sweep with no schema work, which is better than the brief's best case.

Layer concentration has beaten severity as a selection criterion three sessions running — CORE nine rows,
UIKIT nine, UI eight. This is the fourth.

**Three of the six rows produce no behaviour change at all.** `MASTER-147`, `152` and `156` ship comments.
That makes prose accuracy this session's primary review surface and a false comment its only available
defect — which is why every factual assertion a new comment makes carries a blocking criterion with
`dir-reviewer` named as the adjudicating party.

## Definition of done

1. `MASTER-115` / `INFRA-006` — `PlacedObjectConfiguration.cs` has nothing following a `=>` on its own line,
   and its bare `/// <inheritdoc/>` is replaced by a `<summary>`/`<param>` block matching its three siblings.
2. `MASTER-116` / `INFRA-007` — no single-letter lambda parameter remains in that file.
3. `MASTER-117` / `INFRA-017` — `DependencyInjection.cs` is renamed to
   `InfrastructureServiceCollectionExtensions.cs`, byte-identically, and the one live path reference is
   updated so the ConfigureAwait source guard still passes.
4. `MASTER-147` / `INFRA-009` — `DungeonReadRepository.cs` records that the detail query's `AsNoTracking` is
   load-bearing and the summary query's is a no-op. Both calls remain.
5. `MASTER-152` / `INFRA-002` — the three configurations carrying `HasMaxLength` record that it is model
   metadata the store does not enforce, and **enumerate the six columns no layer bounds**.
6. `MASTER-156` / `INFRA-005` — `FloorConfiguration.cs` records why the `(DungeonId, Number)` index is
   deliberately non-unique, and the index is unchanged.
7. All six rows are closed in `docs/audit/00-master-index.md`, status cell field 11, leading token `**done**`.
8. All six detail entries in `docs/audit/02-infrastructure.md` carry an `AMENDED 6b-xx` block classifying
   each clause and quoting every corrected wording verbatim.
9. No file under `src/DungeonMaster.Infrastructure/Migrations/` is created or modified.
10. Every target file remains 100% CRLF with 0 lone CR.
11. **Orchestrator-held:** the build succeeds with 0 warnings across three TFMs and the suite stays at 1038
    passed, measured serially between waves.
12. **Orchestrator-held:** carried-forward items are appended to `.dir-sdlc/backlog.md` by byte-append with
    explicit `\r\n`, verified by prefix byte-identity.
13. **Orchestrator-held:** the register's open/closed survey is recomputed at Gate 3 from the committed tree.

Items 11 through 13 are deliberately not covered by any task. No task may run a build, a test or a git
command, and no task may touch `backlog.md` — see Assumptions.

## Scope

Six register rows, six source files, two audit documents. Six tasks in two waves: T1 through T4 on source
concurrently, then T5 and T6 on the audit documents once the source has landed.

### Not in scope

- **Any EF migration.** Measured as unnecessary; the 7 files under `Migrations/` also carry a standing
  exemption from `dotnet-format.md` §9's two-tab rule, and a census of the space-indent form returning 35 is
  reading that exemption rather than finding a defect.
- **Closing the six-column enforcement gap** `MASTER-152` exposes — no validator rule, no CHECK constraint,
  no Domain guard. Carried to Gate 2 as a decision.
- **Making the `(DungeonId, Number)` index unique.** It requires renumbering through a disjoint range, a
  redesign of `ReorderFloor`, which `effort: S` does not cover.
- **`src/DungeonMaster.Application/DependencyInjection.cs`**, which carries the identical file/type mismatch
  (type at `:13`, not the `:27` the entry cites). Deliberately left.
- **The two bare `/// <inheritdoc/>` in `DungeonReadRepository.cs`** at base lines 16 and 36. `MASTER-115`'s
  pattern is scoped to `PlacedObjectConfiguration.cs` only.
- **`docs/audit/05-tests.md` and `06-cross-cutting.md`**, which both name `DependencyInjection.cs` and go
  stale when T2 lands. Reported, not edited.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Does either correctness row need a new EF migration? | No. Both have a documentation branch; `156`'s unique branch is not `S`-effort | repo | `DungeonMethods.cs:125-142` shift-then-set; single `SaveChangesAsync` at `UnitOfWork.cs:19`; `measurement.md` §5 |
| Was the index's non-uniqueness deliberate? | Yes, and by the reordering work itself | repo | `e402ed8` added `20260511120000_RemoveFloorNumberUniqueIndex`; its `Up` drops `unique`, its `Down` restores it; `7a4bb2d` deleted it in the squash |
| How many `HasMaxLength` columns are there? | **Nine**, not the eleven the row states | repo | 3+3+3+0 across four configurations, cross-checked against released migrations as 8+1+0 |
| What enforces those nine limits today? | **Three** by an Application validator; **six** by nothing in any layer | repo | 6 `MaximumLength` rules, all `MaximumLength(100)` on a `Name`; `Description\|Notes` over 14 validator files returns 0; `Length` over Domain outside geometry returns none |
| Is `MASTER-117` a pure rename? | **No.** One test references the file by path in a string literal | repo | `ConfigureAwaitDisciplineTests.cs:30` inside `ScopedFiles`, asserted by `File.Exists(...).Should().BeTrue(...)` at `:49-51`; exactly 1 hit under `src` |
| Is `MASTER-115`'s "four spaces" clause true? | **No.** Four-space census 0, leading-tab census 34 | repo | the pre-change file; the conversion half was fixed before this session |
| How many inline-lambda sites are there? | **11**, not 10 | repo | `=>[[:space:]]+[^[:space:]]` returns 11 on the target and 0 on all three compliant siblings; the 10-form cannot match line 43 |
| Can a task append to `.dir-sdlc/backlog.md`? | No. `Edit` normalises embedded lone CR, and that file carries the only 8 in the repository | repo | `measurement.md` §1, §2, §8; "use `Edit`" and "preserve the 8" are mutually unsatisfiable |
| Can a task carry a build or suite criterion? | No. Concurrent `dotnet build` collides on `obj/` and `bin/` | repo | arc practice; the orchestrator runs both serially between waves |
| Is `grep -c $'\r$'` usable for CRLF evidence? | No. It is context-dependent within one shell and measures CR in neither form | repo | `measurement.md` §1, four fixtures; replaced by a byte census |
| What form do amendment blocks take? | `**[AMENDED 6b-xx, <date> -- <verdict>.**` … `**]**`, with per-clause `Classification:` and verbatim previous wording | repo | the ten `AMENDED 6b-xix` blocks in `03-app-shell-and-views.md`; the bracket-less form in `01-domain-application.md` is older |
| Which field is the register status cell? | Field 11 of 12, empty at base on all six rows | repo | `awk -F'\|'` over rows 303, 304, 305, 335, 340, 344: `NF=12`, field 11 is a single space |

## Contracts

**None.** This session declares no interface, so `contracts/` is absent and `manifest.json` records
`"contracts": []`. The contract-operation half of the coverage mapping has nothing to check — there is no
operation that could have an implementer and no caller.

## Open questions

| Question | Who resolves it | When |
|---|---|---|
| Do the six unbounded `Description`/`Notes` columns get validator rules this session, a backlog carry, or nothing? | The user | Gate 2 |
| T1's criteria 3 and 4 prescribe incompatible layouts for line 43 — which governs? | The user | Gate 2, before the freeze |
| Is `T2`'s `domain` right as `backend` when its ownership spans a source file and a test file? | The user | Gate 2 |
| Do the three orchestrator-held definition-of-done items (11-13) stay with the orchestrator? | The user | Gate 2 |

## Waves, as frozen

| Wave | Tasks | Why |
|---|---|---|
| 1 | T1, T2, T3, T4 | Disjoint ownership, no dependencies. Build and suite run after, serially |
| 2 | T5, T6 | Both depend on T1-T4 so every line number they record is re-derived post-change |
