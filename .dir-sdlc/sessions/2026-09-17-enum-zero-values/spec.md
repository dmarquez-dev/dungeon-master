# Session 6b-ix — Enum zero values

**Goal.** Close the `MutationOutcome` zero-value defect on `MASTER-073`'s template, and file the triage of all
fourteen concrete-at-zero production enums in the audit record so the next session inherits a measurement
instead of a claim.

## Context

`src/DungeonMaster.Domain/Entities/MutationOutcome.cs` declares `{ Success, NotFound, OutOfRange }` with no
explicit values, so `default(MutationOutcome)` is `Success` — a CLR default that means *the operation worked*.
`MutationOutcomeExtensions.ToResult` (`:60-77`) closes its switch with
`_ => throw new ArgumentOutOfRangeException(...)`. That arm catches `(MutationOutcome)99`; it cannot catch the
CLR default, because the default **is** a recognised member and is mapped straight to `Result.Success()`. This
is the same defect class as `MASTER-073` with the sign reversed: there the zero value was absorbed by a `_ =>`
arm, here it is passed through by one.

`MASTER-073` was closed by session 6b-viii by adding `ObjectKind.None` at zero. The user ruled for a `None`
member over a 1-based renumbering partly because "add a `None`" was believed to generalise mechanically across
the other enums. **This session measured that premise and it is false in at least one case** — see
`### Not in scope`.

None of the fourteen is filed in the audit record today.

## Definition of done

1. `default(MutationOutcome)` is `MutationOutcome.None`, not `Success`.
2. `MutationOutcome` declares `None, Success, NotFound, OutOfRange` in that order with no explicit `= n`, and
   the three pre-existing members keep their relative order at ordinals 1, 2, 3.
3. `ToResult` rejects a defaulted `MutationOutcome` with `ArgumentOutOfRangeException`, asserting both
   `ParamName` `"outcome"` and `ActualValue` `MutationOutcome.None`.
4. `ToResult`'s rejection of a value outside the declared members still holds, and is proven by an injection
   separate from the one that proves item 3 — each condition demonstrated on its own.
5. The XML `<exception>` documentation on both `ToResult` overloads no longer says "not a recognised member",
   which is false of `None`.
6. `MutationOutcome`'s existing mapping behaviour is preserved: `Success` to `Result.Success()`, `NotFound` to
   `ErrorCategory.NotFound`, `OutOfRange` to `ErrorCategory.Validation`.
7. `DrawSessionResult` declares `None, Added, Closed, Ignored` in that order with no explicit `= n`, in
   `App.Core` only, and `PlaceVertex` never returns the zero member.
8. The two production comparison sites at `DungeonDetailPage.xaml.cs:1473` and `:1489` are byte-for-byte
   unchanged, so a leaked `None` takes neither branch and commits no vertex.
9. The audit record gains `CORE-034` (the anchor), `CORE-035` (the full triage), and `UI-037`
   (`DrawSessionResult` and its untaken follow-up), with three new index rows numbered 177, 178, 179.
10. The triage names all fourteen enums with their zero member and class, and gives the denominator as **17**
    production enums, not 16.
11. `CORE-017`'s 6b-viii amendment is corrected beside its existing text — not rewritten — to record that
    "renumbering is free" is specific to `ObjectKind` and does not generalise.
12. No new index row claims `done` and none carries a commit hash; a task cannot invoke git.
13. Every file created or modified under `src/` is CRLF on every line, verified by counting `0x0D 0x0A` pairs
    in binary.
14. `dotnet build` reports `0 Warning(s)` and `0 Error(s)` for every project touched, and
    `dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` reports `0 failed`.

## Scope

Two enums change in code — `MutationOutcome` and `DrawSessionResult`, the two the measurement places in
class A. All fourteen are filed as analysis.

### Not in scope

- **`PathSegmentKind`, and this is a measured exclusion rather than a judgment.** `AreaConfiguration.cs:16`
  builds `new JsonSerializerOptions(JsonSerializerDefaults.General)` and `src/` contains **zero**
  `JsonStringEnumConverter` or `JsonConverter` declarations, so `System.Text.Json` writes the **ordinal** into
  the `Areas.Outline` TEXT column — and `PathSegmentJsonRoundTripTests.cs:26-28` already pins `"Kind": 0` as
  `Line` and `"Kind": 1` as `CubicBezier`. Inserting `None = 0` reinterprets every persisted outline
  (`Line` to `None`, `CubicBezier` to `Line`). That is a **data migration on persisted rows**, not a
  renumbering, and nobody proposing the sweep has proposed a migration.
- **`RotationDirection` and `ScaleDirection`.** Their consumers are boolean comparisons —
  `dir == RotationDirection.Clockwise` at `DungeonDetailPage.xaml.cs:140` and `:151`, and
  `var down = dir == ScaleDirection.Down;` at `:167` — whose else-branch would absorb a new zero member and
  **still perform the operation in the opposite direction**. A `None` member there renames the silent
  behaviour without detecting it.
- **The eight class-C enums**, judged compliant under the guideline's most-common-default branch.
- **Any change under `src/DungeonMaster.App/`**, which also keeps the session off the Windows TFM.
- **Closing the new entries.** They are filed `open`; closure with a commit hash is orchestrator work.
- **The 8 lone CR characters in `.dir-sdlc/backlog.md`**, unrepaired for the fourth session running and still
  outside an approved scope.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| How many production enums carry a concrete member at zero? | 14 of 17 | 1 — repo | `measurement.md` §1; predicate validated against `MutationOutcome` (must flag) and `ObjectKind` (must not) |
| Is the inherited "14 of 16" usable? | No — the denominator is wrong | 1 — repo | `measurement.md` §1; reconciles as 15 of 17 before 6b-viii added `ObjectKind.None` |
| Is `default(MutationOutcome)` reachable in production? | Not by any found path — the defect is latent | 1 — repo | 55 production refs; no `default(...)`, `= default`, array allocation or uninitialised field found. `MASTER-073` was closed on the same latency |
| Is a `None` member the project's chosen resolution? | Yes | 1 — repo | `CORE-017`, `docs/audit/01-domain-application.md:741`; `MASTER-073` implements it |
| Will an analyzer catch what this session leaves? | No | 1 — repo | `CA1008` does not fire at `AnalysisMode=Recommended`; build probe, 6b-viii |
| Does `MutationOutcome` reach Infrastructure? | No | 1 — repo | `grep -rln "MutationOutcome" src/DungeonMaster.Infrastructure/` returns nothing |
| Is any `MutationOutcome` ordinal cast in the tree? | One, and it is a test literal | 1 — repo | `(MutationOutcome)99` at `MutationOutcomeExtensionsTests.cs:109` |
| Next free audit ids? | `CORE-034`, `CORE-035`, `UI-037`; rows 177–179 | 1 — repo | max `CORE-033`, max `UI-036`; 176 rows, `#` contiguous 1–176, max `MASTER-176` |
| Can a task write the closure hash? | No | 1 — repo | Tasks cannot invoke git; every prior closure came from an orchestrator docs commit, e.g. `d3b4cbd` |

## Contracts

- `contracts/enum-zero-values.md` — C1 `MutationOutcome` shape, C2 `DrawSessionResult` shape, C3 `ToResult`
  zero-member behaviour.

## Open questions

| Question | Who resolves it | When |
|---|---|---|
| `DrawSessionResult.None` has no production consumer — accept the fall-through, or require explicit rejection at `DungeonDetailPage.xaml.cs:1473` and `:1489`? | User | Gate 2 |
| T3's C3 and C6 disagree about `SnapAnchor`'s class, in the same entry | User | Gate 2 |
| `PlaceVertex` has three return paths and the manifest specifies two injections | User | Gate 2 |
