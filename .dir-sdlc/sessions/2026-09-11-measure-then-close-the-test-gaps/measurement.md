# Re-measurement of the five inherited entries, against `1407090`

Required by the inheritance note at `docs/audit/00-master-index.md:522`: none of these five was
re-measured by 6b-ii, so staleness was suspected but not known. Five read-only agents measured them
independently against a single fixed commit, `1407090`, before any decomposition. Every count below
was produced by a named command in this session.

## Verdicts

| Entry | Source | Verdict | Consequence |
|---|---|---|---|
| MASTER-020 | TEST-001 | **CLOSED** | No code work. Record correction only. |
| MASTER-070 | TEST-019 | **FALSIFIED as written; narrower gap real** | Code work, re-scoped. |
| MASTER-086 | TEST-020 | **PARTLY CLOSED; two clauses never true** | Code work, re-scoped; three design decisions surfaced. |
| MASTER-155 | TEST-032 | **WRONG SHAPE; prescribed fix is a known regression** | Decision required; record correction is the non-optional part. |
| MASTER-167 | (no source record) | **STILL LIVE, verbatim** | Code work as described, plus two amplifications. |

## MASTER-020 / TEST-001 — CLOSED

All three types have behaviour-level tests. Closed on 2026-08-18, the day after the audit was
written, by `09f4360`, `f9a665b`, `3b44d91`.

- `DispatcherTests.cs` — 6 facts, real `ServiceCollection`, asserts behaviour ordering
  (`first:enter, second:enter, handler, second:exit, first:exit`) and invoker-cache identity.
- `ValidationBehaviorTests.cs` — 11 facts, including
  `HandleAsync_WithOneFailure_ReturnsValidationFailureAndDoesNotCallNext` asserting `nextCalled == false`.
- `ResultTests.cs` (5), `ResultOfTTests.cs` (8), `ResultMessageTests.cs` (9).
- `Result<T>` moved to its own file `Common/Results/ResultOfT.cs` in `3b44d91`.

Contradicted assertions in TEST-001: the title; "`Application/` contains `Common/Map/` and
`Features/Dungeons/` and nothing else" (there is no `Common/Map/` at all); "no test anywhere";
"the four validator test classes" (there are 14); "never that the rule object is reached by the
pipeline" (three `SendAsync_...IsRejectedBy...` facts prove exactly that); the cited anchor
`AddAreaCommandValidatorTests.cs:17`, which now lands on `protected override string NamePropertyName`;
and "all 292 tests would still pass", which is the premise of the S2 rating.

Residuals found but **not** part of MASTER-020, not to be smuggled into it: `Dispatcher` token
propagation is unasserted, and `Dispatcher`'s missing-handler failure mode is untested.

## MASTER-070 / TEST-019 — FALSIFIED, real gap re-scoped

Counterexamples: `ValidationBehaviorTests.cs:222` and `:366` each construct a
`CancellationTokenSource`, pass the token into production code, and assert the exact instance
arrived (`Arg.Do<CancellationToken>` capture at `:233`, `:242`, `:377`, `:386`). Added `f9a665b` /
`09f4360`, 2026-08-18 — one day after the audit.

What survives, stated correctly: **no handler and no repository is proven to forward its token.**
0 of 16 handlers (14 command + 2 query); 0 of 6 repository/unit-of-work async methods; 1 of 1
pipeline behavior is proven. Still true: no test cancels a token, no test asserts
`OperationCanceledException`, no `Arg.Is<CancellationToken>` anywhere.

Production code threads the token correctly throughout — verified across handlers, the
`DungeonMutationExtensions` overloads, and Infrastructure. **This is a coverage gap, not a defect.**

Shape change that matters: `8784326` (6b-ii) extracted `CommandHandlerTestBase` and
`DungeonNotFoundHandlerTestBase`; all 14 command-handler test classes now derive from one of them,
and `UnitOfWorkSubstitute.cs:24` is the single `Arg.Any<CancellationToken>()` chokepoint. The
record's per-file anchors `AddAreaCommandHandlerTests.cs:37` and `UpdateFloorCommandHandlerTests.cs:37`
are dead. One base-level test with a new invocation seam covers all 14 leaves.

## MASTER-086 / TEST-020 — PARTLY CLOSED; two clauses never true

`781f73b` (2026-08-18, "report mutation outcomes instead of silently no-opping") grew
`DungeonAggregateTests.cs` from 104 to 336 lines and from 8 to 21 tests — precisely the boundary
coverage the record asked for.

Clause by clause:

| Clause | Status |
|---|---|
| `new Dungeon(null!)` / empty name | **still open** |
| `ReorderFloor` index zero | **closed** — `DungeonAggregateTests.cs:161` |
| `ReorderFloor` one past the end | **closed** — `:179` |
| `ReorderFloor` negative | open at Domain level; covered at Application level |
| `AddFloor` at a boundary | **never true** — `:12` and `:24`, both present at the audit commit `413be78`; `:12` is the record's own first cited location |
| `UpdateAreaOutline` degenerate outline | open, but as a **missing production invariant**, not a missing test |
| "`PlacedObjectTests` is the only boundary `[Theory]`" | **never true** — `ObjectScaleTests.cs:8` and `:16` predate the audit by three months |

Also contradicted: "the five Domain classes" (six test files now; `FloorMethodsTests.cs` added by
`781f73b`), and "covered by exactly one three-floor happy-path case each" (`RemoveFloor` has 4 tests,
`ReorderFloor` has 6).

Production findings, distinct from test gaps, each requiring a decision before any test can assert
anything:

- **P1** `Area.SetOutline` (`AreaMethods.cs:38-44`) does `Clear()` then `AddRange()` with no
  segment-count and no null check. A 0-, 1-, or 2-segment "polygon" is stored silently. The
  Application validator rejects empty/null, so the aggregate is protected only by a caller it does
  not control.
- **P2** `Floor..ctor` and `Floor.UpdateNumber` accept any `int`, including 0 and negatives.
- **P3** `PathSegment` permits `CubicBezier` with null control points, contradicting its own doc comment.
- **P4** ~~`RotateDirection` is dead — zero references in `src/` outside its declaring file.~~
  **FALSIFIED during decomposition, 2026-09-11.** This row was wrong, and it was wrong in this
  session rather than inherited. The measuring agent grepped the *file* name. The file
  `src/DungeonMaster.Domain/Entities/Floor/RotateDirection.cs:3` declares
  `public enum RotationDirection`, and `grep -rn "RotationDirection" src/` returns six lines in three
  files: the declaration plus **five live references** at
  `src/DungeonMaster.App/Views/DungeonDetailPage/ControlsPaletteView.xaml.cs:8`, `:14`, `:15` and
  `DungeonDetailPage.xaml.cs:139`, `:150`. `grep -rn "RotateDirection" src/` returns 0 because no
  identifier by that name exists anywhere. The real finding is a **file-name/type-name mismatch**,
  not dead code. Caught by `dir-architect` while decomposing; confirmed by the orchestrator with both
  commands. This is the same grep-shape failure the session exists to correct, one layer further in.

## MASTER-155 / TEST-032 — WRONG SHAPE; the prescribed fix is a known regression

All four absences are literally still true: coverlet unused, no `.runsettings`, no
`xunit.runner.json`, no `[assembly: CollectionBehavior]`, and no CI — the last verified, not assumed
(zero tracked `.yml`/`.yaml` files; no `.github/` directory).

Two inversions:

1. **Scope tripled.** coverlet is referenced by three test projects, not one:
   `DungeonMaster.Tests.csproj:9`, `App.Tests.csproj:26`, `App.UnitTests.csproj:20`, with the version
   central at `Directory.Packages.props:20`. The cited line `:11` was wrong when written — `git show
   413be78:src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` shows coverlet at line 9 on the audit date.
   **Corrected 2026-09-11:** this row originally cited `8bb2d79`, which is dated 2026-08-18 — one day *after*
   the audit, on the wrong side of the boundary the DRIFT / MEASUREMENT ERROR split turns on, so it could not
   establish what was true when the record was written. Caught by T5's reviewer. The conclusion is unchanged;
   the anchor now is the audit commit itself. See `handover-addendum.md` §1.
2. **The suggested remedy is a measured regression.** The record proposes `[assembly:
   CollectionBehavior]` "so the parallelism the fixtures depend on is written down". Session 6a's
   spike measured it: 3 crashes in 11 runs, ~27% nondeterministic
   (`.dir-sdlc/sessions/2026-09-02-app-sub-suite-rebuild/spike-parallelism-report.md:204-205`, `:407`),
   because `DisableTestParallelization` serialises collections without ordering them.
   `manifest.json:293` of that session states it "must not be adopted as a mitigation".

And the thing the record says is unpinned **is** pinned, in the form that works:
`MauiHostCollection.cs:7-8` and `AppTestCollection.cs:27-28`, nine classes declaring membership, plus
a runtime latch at `MauiHostFixture.cs:78-84` that throws naming the required collection. So
"that safety is a property of how the fixtures happen to be written, not something the repository
states" is falsified twice over.

Shared-state audit found nothing parallel-unsafe: every persistence test gets its own
`Data Source=:memory:` connection; the only real-DB writer sets a per-run GUID temp path before
WinUI starts; temp directories are per-call; the one mutable static is an `Interlocked` latch.

## MASTER-167 — STILL LIVE, verbatim

`GreaterThanZeroConverter.cs:38` computes `value is int n && n > 0` — correctly `false` for the
sentinel — and `:42` negates it under `Invert`. No guard anywhere in the file. Unmodified since
`fda46b7` (2026-08-28), which touched only `ConvertBack`. Match at `:40` is
`s.Equals("Invert", StringComparison.OrdinalIgnoreCase)`, case-insensitive, string-typed.

Consumers: seven `GreaterThanZeroConverter` lines in XAML — one resource declaration at
`App.xaml:21` and six binding sites. Exactly two pass `Invert`, and they are the two the entry names,
at the exact cited lines: `DungeonDetailPage.xaml:151` and `DungeonListPage.xaml:115`. Both are
transparent `BoxView` overlays carrying the identical tooltip — the entry implies only the list page
is.

Two amplifications:

- The defect covers **every non-`int` value**, not only the sentinel: `null`, `double`, `long`,
  `string` all return `true` under `Invert`.
- Both consumers use compiled bindings (`x:DataType`, `MauiXamlInflator=SourceGen`), so the entry's
  stated trigger — "a typo or a rename in either bound path" — would be a **build failure**, not a
  runtime sentinel. The defect is a **latent trap** at both sites, with a type change or a future
  consumer lacking `x:DataType` as the realistic trigger. The entry's user-visible consequence does
  not follow for these two sites as they stand.

Existing coverage: `App.Tests/Converters/GreaterThanZeroConverterTests.cs` has
`ConvertReturnsFalseForTheUnresolvedBindingSentinel`, which passes `parameter = null`. It passes
today **and** after the fix, so an `Invert` + `UnsetValue` test is a genuine red-first step.

`UIKIT-004` is **not** falsified, exactly as the entry instructs. `04-app-controls-and-theme.md:224-225`
claims only that the bare `value is int n` test rejects the sentinel, which is true. (The entry cites
the range as `:225-226`; the sentence is at `:224-225` and `:226` is blank.)

Precedent to mirror: `NotNullConverter.cs:41` —
`value is not null && !ReferenceEquals(value, BindableProperty.UnsetValue)`.

## Cross-cutting observation

Three of the five entries were overtaken by commits landing on **2026-08-18**, the day after the
audit was authored: `09f4360`, `f9a665b`, `3b44d91`, `781f73b`. This is the same staleness-is-drift
pattern 6b-ii recorded, and it is now measured rather than suspected for these five.

Two of TEST-020's clauses, and TEST-032's cited line, were **wrong when written** rather than
overtaken — a distinct failure from drift, and one that re-measurement is the only way to catch.
