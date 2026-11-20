# 6b-xv — Guard `CanvasPoint`, de-duplicate the `PlacedObject` scale guard, and correct `MASTER-081`

**Goal.** Close the one reachable, unguarded non-finite path the `MASTER-081` re-measurement found, remove
the duplicated scale guard beside it, and correct the register entry whose two load-bearing premises the
re-measurement falsified.

## Context

`MASTER-081` / `CORE-016` (`docs/audit/00-master-index.md:269`, `docs/audit/01-domain-application.md:861-899`)
was the last open CORE S3 and the 6b-xv handoff's recommended scope. It was re-measured against `75e80ee`
in the main checkout before any decomposition; the full measurement is `measurement.md` beside this file.

**Four of its six cited locations are correct; two are stale. Two of its six assertions are false, and
they are the two its argument rests on:**

- *"because `AddPlacedObjectCommand` has no validator (CORE-009)"* — `MASTER-079` / `CORE-009` is
  `**done**`, and both `AddPlacedObjectCommandValidator` and `UpdatePlacedObjectCommandValidator` bound
  `Scale` with `InclusiveBetween(ObjectScale.Min, ObjectScale.Max)`.
- *"The three value objects under `ValueObjects/` are plain positional records that accept any input"* —
  false for `PathSegment`, which `MASTER-170` closed in 6b-vii.

The entry's *suggested direction* — a `Policies/` layer with `PolicyResult` and `DomainException` and an
`ObjectScale` value object — would introduce two types this repository has nowhere (`grep` returns **0**
hits for each) and change the EF mapping of a persisted `REAL` column, to obtain a benefit
(`backend-conventions.md` §5's pre-validation) that the two validators already deliver. **The user ruled
it out at Gate 1.**

What the measurement found instead is a live gap the entry mentions only in passing: there are **3**
non-finite guards in the whole of `src/`, two of them the duplicated pair under measurement, and
**`CanvasPoint` has none**. `Position` is constrained by `.NotNull()` and nothing else in either
validator, and persists to two `IsRequired()` `REAL` columns.

## Definition of done

1. **`CanvasPoint` rejects a non-finite coordinate**, in both coordinates, for `NaN` and both infinities,
   naming the offending coordinate as the exception's `ParamName` — and it rejects one set through a
   `with` expression too, not only through the constructor.
2. **The `PlacedObject` scale guard exists once, not twice.** `grep -c -F "float.IsNaN("` and
   `grep -c -F "Scale must be between"` over `PlacedObject.cs` each return **1**; both return **2** at
   `75e80ee`. The two distinct `ParamName`s (`scale`, `newScale`) and the `ActualValue` survive the
   extraction.
3. **`MASTER-081`'s Status cell records the entry as `part done`**, in the unbolded shape the table
   already uses at `:282` and `:296`, naming what was and was not built.
4. **`CORE-016`'s falsified assertions and stale ranges are corrected in place**, with every line range
   re-read from the file rather than carried from `measurement.md`.
5. **What SQLite does with a non-finite value in a `REAL NOT NULL` column is run and pinned**, not
   reasoned.
6. **Whether `InclusiveBetween` rejects `NaN` and both infinities is run and pinned**, not reasoned.
7. **Whether a production path can actually produce a non-finite `CanvasPoint` is run and pinned**, with
   every assignment site checked and named.
8. **The suite stays green**: `dotnet test src/DungeonMaster.sln` passes, per-assembly figures matching the
   dispatch baseline plus the tests each task adds, and `dotnet build` emits no warning or error measured
   with `grep -cE "(warning|error) [A-Z]+[0-9]+"`.

Items 5–7 are the three things `measurement.md` §5 deliberately declined to claim. They are
definition-of-done items rather than notes because the answer to item 5 may change what the right guard
in item 1 is.

## Scope

`src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs`,
`src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs`, their tests, three new test files pinning
items 5–7, and the two audit documents.

### Not in scope

- **A `Policies/` directory, a `PolicyResult` type, a `DomainException` type, an `ObjectScale` value
  object, or any change to the EF mapping of `PlacedObject.Scale`.** Ruled out by the user at Gate 1.
  `MASTER-081` therefore closes **`part done`**, never `done`. Two tasks carry an explicit scope-fence
  criterion because `CORE-016`'s own *suggested direction* proposes exactly these, and an implementer
  reading the entry is being actively pulled across the line.
- **`PathSegment`'s `with`-expression bypass** — a record's copy constructor does not re-run property
  initializers, so `MASTER-170`'s guard is reachable around. Already a known, carried backlog item; this
  session's item 1 avoids repeating the shape in `CanvasPoint` but does not fix `PathSegment`.
- **The `Floor` lookup-and-guard repetition** (`FloorMethods.cs:90`, `:128`, `:162`;
  `FloorObjectMethods.cs:56`, `:95`), carried forward by 6b-xiv and still open.
- **`dm-conventions`** — a separate repository. Its §5 cross-reference is broken (it points at
  `dotnet-format.md` §9, *XML Documentation Comments*; the content is §11 at `:552`) and recorded here,
  not fixed.
- **The 8 lone CR characters in `.dir-sdlc/backlog.md`**, dating to 6b-iv. Re-measured at **8**,
  unchanged. Ninth consecutive session leaving them alone.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Is `MASTER-081`'s cited authority reachable? | Yes, but its cross-reference is broken | repo evidence | `dm-conventions` present; §5 at `backend/backend-conventions.md:165`; it cites `dotnet-format.md` §9 but the shape is at §11, `:552` — `grep -n "^## "` |
| Does the repository have `PolicyResult` or `DomainException`? | No — 0 hits each | repo evidence | `grep -rn "PolicyResult" --include=*.cs src/`, same for `DomainException`, both excluding `bin`/`obj` |
| Is `CanvasPoint` only a persistence type? | No — 5 of 78 construction sites are production, all in `DungeonDetailPage.xaml.cs`, and only `:1442` is persisted | repo evidence | `grep -rn "new CanvasPoint(" --include=*.cs src/`; the other four are screen-space touch, drag and pan-anchor paths |
| Does any existing site construct a non-finite `CanvasPoint`? | No — 0 | repo evidence | the same census filtered `grep -cE "NaN\|Infinity"` |
| Does `ScreenToCanvas` return a `CanvasPoint`, so that guarding the type would break T2's tests? | **No — it returns `PointF`** | repo evidence | `FloorMapDrawable.cs:628-630`; `PanOffset` is `PointF` at `:442`. The `CanvasPoint` boundary is `DungeonDetailPage.xaml.cs:1442`, downstream |
| What is the suite baseline? | 980 passed, 0 failed, 0 skipped | repo evidence | measured in the worktree at `75e80ee`: `DungeonMaster.Tests` **855** (`net10.0`), `App.UnitTests` **23** (`net10.0`), `App.Tests` **102** (**`net10.0-windows10.0.19041.0`**, read from the `Test run for` line, not the `Passed!` line) |
| Does `PlacedObjectTests.cs` already assert a `ParamName`? | No — 0 | repo evidence | `grep -c "ParamName" src/DungeonMaster.Tests/Domain/Entities/Floor/PlacedObjectTests.cs` |
| Is the `Update_InvalidScale_Throws` theory symmetric with the constructor's? | No — it lacks `NegativeInfinity` (4 cases against 5) | repo evidence | read at `PlacedObjectTests.cs:62-67` and `:80-84` |
| What shape does a partial close take in the index? | `part done (<session>) -- <what was and was not done>`, unbolded | repo evidence | `:282` and `:296`, Status cell read directly |
| Will marking `MASTER-081` `part done` change the open counts? | Yes — open 44→43, part done 3→4, open S3 2→**1**, open CORE 13→**12** | repo evidence | the handoff §6 survey script classifies `part done` into its own bucket; to be re-run after T5 |

## Contracts

**None.** No task consumes an interface another task produces. T3 and T5 consume T1's and T2's *findings*,
which land as assertions and class-level XML doc comments inside those tasks' own test files — prose and
tests in the repository, not a declared surface. `contracts/` is empty and the contract-operations half of
the coverage mapping is therefore empty. **No contract was invented to fill the slot.**

## Open questions

None outstanding. Items 5–7 of the definition of done are the three questions the measurement left open,
and each is assigned to a task that must **run** it rather than reason about it.
