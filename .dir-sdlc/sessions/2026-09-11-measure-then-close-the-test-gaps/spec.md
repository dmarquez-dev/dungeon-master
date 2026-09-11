# Measure, then close the test gaps — audit session 6b-iii

Close the five master-index entries 6b-iii inherited, doing the work the measurement supports rather
than the work the entries describe.

## Context

6b-ii handed forward five entries — `MASTER-020`, `MASTER-070`, `MASTER-086`, `MASTER-155`,
`MASTER-167` — with an explicit warning at `docs/audit/00-master-index.md:522`: none had been
re-measured, staleness was suspected rather than known, and 26 of `05-tests.md`'s 32 records remain
unchecked rather than confirmed.

Five read-only agents measured all five against the fixed commit `1407090` before anything was
decomposed. The results are in `measurement.md` and they are the brief for this session; the entries
themselves are its subject, not its instructions.

**Only one of the five entries describes work that still needs doing as written.** Three were
overtaken by commits landing 2026-08-18, the day after the audit was authored. Two contain clauses
that were false when written — including `TEST-020` naming as absent a test that its own first cited
location points at. One, `TEST-032`, prescribes a remedy that a later session measured at a ~27%
crash rate.

So the session's largest output is corrections. That is the point of it, not a detour from it.

## Definition of done

**Code**

1. `CommandHandlerTestBase` carries a leaf-supplied invocation seam accepting a `CancellationToken`,
   and every command-handler test class overrides it — proven by a clean
   `dotnet build src/DungeonMaster.Tests/DungeonMaster.Tests.csproj`, not by a grep.
2. A `[Fact]` on `CommandHandlerTestBase` proves the exact token instance reaches
   `IUnitOfWorkFactory.CreateAsync`, executing once per derived leaf. `Arg.Any<CancellationToken>()`
   does not satisfy it.
3. `IDungeonWriteRepository.LoadAsync` is proven to receive that same token instance.
4. `IUnitOfWork.CommitAsync` and `IDungeonWriteRepository.RemoveAsync` are each proven to receive it.
5. `IDungeonReadRepository.GetSummariesAsync` and `GetDetailAsync` are each proven to receive it.
6. At least two Infrastructure tests pass an **already-cancelled** token into a real repository over
   `SqliteInMemoryTestBase` and assert `OperationCanceledException`, covering one read-side and one
   write-side method. Zero such tests exist today anywhere.
7. Domain boundary tests exist for the guards that already exist: `Dungeon` and `Area` name guards,
   `ReorderFloor` with negative targets, and the two members with no Domain test at all
   (`Dungeon.AddPlacedObject` both overloads, 3-arg `Floor.UpdateObject`).
8. No Domain production behaviour changes.
9. `GreaterThanZeroConverter.Convert` returns `false` for any non-`int` value **before** the `Invert`
   block can negate it.
10. The named converter behaviours are preserved: `(5, null)`→true, `(0, null)`→false,
    `(0, "Invert")`→true, `(5, "Invert")`→false, case-insensitive `"invert"`, `ConvertBack`→null.
11. A converter test fails before the guard and passes after it. The existing
    `ConvertReturnsFalseForTheUnresolvedBindingSentinel` passes both before and after, so it is not
    evidence.
12. `coverlet.collector` is removed from all three test projects and from
    `Directory.Packages.props`, and the `packages.lock.json` files regenerate.
13. No `.runsettings`, no CI file, no `xunit.runner.json`, and no `[assembly: CollectionBehavior]` is
    added.

**Records**

14. All five entries' `Status` cells in `00-master-index.md` read `**done** (6b-iii)` with the
    evidence the owning task handed over.
15. `MASTER-020` is closed **by evidence with no code work**, and `TEST-001`'s seven contradicted
    assertions are corrected, each classified `DRIFT`, `MEASUREMENT ERROR` or `UNDETERMINED`.
16. The two dispatcher residuals are **not** folded into `MASTER-020`'s closure.
17. `TEST-019`'s headline is recorded as falsified, with the narrower real gap stated as measured,
    and its two dead anchors replaced.
18. `TEST-020`'s two never-true clauses are classified `MEASUREMENT ERROR`, not `DRIFT`.
19. `TEST-032` records both inversions, including that its own suggested `[assembly:
    CollectionBehavior]` is a measured regression.
20. `MASTER-167`'s two inaccuracies are corrected, and `UIKIT-004` is **not** recorded as falsified.
21. Six new entries are filed: `MASTER-168`–`MASTER-171` from the production findings, `MASTER-172`
    and `MASTER-173` from the dispatcher residuals.
22. A 6b-iii paragraph is appended to the `05-tests.md` preamble naming exactly which four records
    this pass re-measured and stating that the rest remain unchecked rather than confirmed.
23. Every count is stated with the command that produced it, anchored at `1407090` or a named
    historical commit. No count is anchored at `HEAD`.

## Scope

The five inherited entries, their source records in `docs/audit/05-tests.md`, their `Status` cells
and narrative entries in `docs/audit/00-master-index.md`, and the six new entries this session's
measurement justifies filing.

### Not in scope

- **Fixing the three unguarded production inputs** (`Area.SetOutline` degenerate/null outline,
  `Floor` number zero/negative, `PathSegment` CubicBezier null control points). Filed as
  `MASTER-168`–`170` per escalation E2. A test cannot assert behaviour that does not exist, and
  deciding what that behaviour should be is a design decision, not coverage work.
- **`.runsettings`, CI, `xunit.runner.json`, `[assembly: CollectionBehavior]`** — per escalation E1.
  The last is not merely declined but recorded as harmful.
- **The two dispatcher residuals** — token propagation through `Dispatcher`, and its missing-handler
  failure mode. Filed as `MASTER-172`/`173`. Folding them into `MASTER-020` would let a closed entry
  claim coverage nobody wrote.
- **`ReorderFloor` with a negative target at the Application layer** — already covered at
  `ReorderFloorCommandValidatorTests.cs:65-69`. Only the Domain-level gap is in scope.
- **The other 26 unchecked records in `05-tests.md`.** This session re-measured four. Saying so
  plainly is item 22; silently implying more were confirmed is the failure mode.
- **`docs/audit/04-app-controls-and-theme.md`** — not edited. The bad citation lives in the index.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Are the five entries stale? | Three overtaken by drift, two carry clauses false when written, one live verbatim | repo evidence | `measurement.md`, five independent measurements at `1407090` |
| Where do converter tests live? | `App.Tests`, alongside the existing class; not split across two projects | repo evidence | `App.Tests/Converters/GreaterThanZeroConverterTests.cs`; no collection attribute, so no membership change needed |
| Does one base `[Fact]` really cover all 14 command handlers? | Yes — `IUnitOfWorkFactory.CreateAsync` is the universal chokepoint | repo evidence | `UnitOfWorkSubstitute.cs:22-25` |
| Why do `CommitAsync`/`RemoveAsync` need per-file tests? | The not-found base path reaches neither | repo evidence | `DungeonNotFoundHandlerTestBase.cs:47-53` asserts `DidNotReceive()` on both |
| Is the token gap a production defect? | No — production threads it correctly throughout. Coverage gap only | repo evidence | handlers, `DungeonMutationExtensions`, Infrastructure all verified |
| Is `MASTER-167` currently firing? | No — a latent trap at both sites; compiled bindings make a rename a build failure | repo evidence | `x:DataType` at both pages `:10`; `MauiXamlInflator=SourceGen` at `DungeonMaster.App.csproj:45` |
| Severity and effort for the six new entries | This session's judgment, recorded as judgment | judgment | — |
| Can the code tasks run concurrently? | Ownership is disjoint, but they share `obj/`/`bin/` in one worktree, so they are serialized | judgment | architect's report; intermittent build contention is the unmeetable-criterion class this repo has already burned three sessions on |

## Contracts

- `contracts/converter-behaviour.md` — the `Convert`/`ConvertBack` truth table T3 implements and
  tests against.
- `contracts/closure-evidence.md` — the evidence package each code task hands to T5, so a closure
  cites the owning task rather than re-deriving it.
- `contracts/new-entries.md` — the fixed ids, severities, layers and titles for `MASTER-168`–`173`.

## Open questions

| Question | Who resolves | When |
|---|---|---|
| E4 — should `MASTER-171` be filed as a file-name/type-name mismatch, or dropped with only the falsification recorded in the `MASTER-086` amendment? | User | Gate 2 |
| Full-suite green after all five tasks land — no task can honestly own this, since every task's evidence is a scoped `--filter` run | Orchestrator | Between T5 and Gate 3 |
