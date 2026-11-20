# Measurement — session 6b-ix, enum zero values

Every number here was derived in this session, at `80beda93a689e47ed899d64fe7ad41fb8e99a677`, by the command
named beside it. Nothing is carried from the 6b-ix handoff or from 6b-viii's steelman round.

## 1. The census, and its correction to the inherited record

Command — a Python walk of `src/`, skipping `bin`/`obj`/`.vs`, stripping block and line comments from the
whole file **before** parsing, then matching `(public|internal|private|protected) enum <Name> [: base] { body }`
and assigning values left to right with explicit `= n` honoured:

| Quantity | Measured | Inherited claim |
|---|---|---|
| Enum declarations under `src/` | **18** | 17 |
| Production enum declarations | **17** | 16 |
| Test-project enum declarations | **1** (`AsyncVoidUnitKind`) | 1 |
| Production enums with a concrete member at zero | **14** | 14 |
| Production enums with `None` at zero | **3** | 2 |

The inherited `14 of 16` is wrong in its denominator and right in its numerator. It reconciles exactly:
`ObjectKind.None` was added by 6b-viii, moving `ObjectKind` out of the concrete-at-zero bucket, so the census
that produced `14 of 16` was taken over a tree where the true figure was `15 of 17`. **The architect's census
missed one concrete-at-zero enum.** Which one is not recoverable from the claim, and does not need to be: the
figure to use is `14 of 17`, measured above.

### Predicate validation

A total re-run cannot catch a wrong predicate, so the predicate was validated against two named cases before
the count was used:

- **Must flag** — `MutationOutcome` → zero member `Success`. Flagged.
- **Must not flag** — `ObjectKind` → zero member `None`. Not flagged.
- Unparsed members across all 18 declarations: **0**.

The first run of this census was **wrong and was discarded**. It split each enum body on `,` before stripping
doc comments, so every enum whose `<summary>` contains a comma was corrupted — it reported `DrawState`'s zero
member as `Drawing` (it is `Idle`) and `NotebookKind`'s as `Floor` (it is `Dungeon`). The corrected script
strips comments from the whole file first. This is recorded because the discarded run's numbers were
confident, reproducible, and false.

## 2. The anchor defect, confirmed from source

`src/DungeonMaster.Domain/Entities/MutationOutcome.cs` declares, in this order and with no explicit values:

    MutationOutcome { Success, NotFound, OutOfRange }

so `default(MutationOutcome)` is `Success`. A mutation nobody performed reports that it worked.

**The guard that exists for this enum cannot catch it.** `MutationOutcomeExtensions.ToResult`
(`src/DungeonMaster.Application/Common/Handlers/MutationOutcomeExtensions.cs:60-77`) switches on the outcome
and closes with:

    _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unrecognised mutation outcome.")

That arm catches an undefined value such as `(MutationOutcome)99`. It cannot catch the CLR default, because
the CLR default **is** a recognised member — so the default maps to `Result.Success()` and the failure is
reported to the Application layer as a success. This is the same defect class as `MASTER-073`, with one
difference that makes it sharper: `MASTER-073`'s zero value was absorbed by a `_ =>` arm, and this one is
passed through by it.

**The defect is latent.** Every production site that produces a `MutationOutcome` returns an explicit member;
counted by the same walk, `MutationOutcome` has 55 production references and 30 test references, and no
`default(MutationOutcome)`, `= default`, `new MutationOutcome[]`, or uninitialised field or property of that
type was found. `MASTER-073` was latent on the same terms and was closed anyway.

> **The default-reachability search is a weak instrument and its silence is not evidence.** It matches source
> patterns only. It cannot see reachability through a generic `default(T)`, a `FirstOrDefault`, a struct field,
> a mocking framework's unconfigured return, or a persistence round-trip. Where this measurement says no
> pattern matched, that is what it says and no more.

## 3. Triage of the 14 — three classes, not one defect

Not all fourteen are defects, and a criterion asserting they are would be false. The Microsoft guideline admits
two compliant shapes — `None = 0`, or *most-common-default* `= 0` — and most of these fourteen sit in the
second branch legitimately.

### Class A — the zero value asserts a consequential outcome

| Enum | Zero member | Evidence |
|---|---|---|
| `MutationOutcome` | `Success` | §2. Domain to Application; the `_ =>` guard passes the default through as success |
| `DrawSessionResult` | `Added` | `MapDrawingSession.cs:8`; consumed by equality at `DungeonDetailPage.xaml.cs:1473` and `:1489`, so a leaked default reads as "a vertex was added" |

### Class B — the zero value silently picks one of several equally valid options

| Enum | Zero member | Note |
|---|---|---|
| `RotationDirection` | `Clockwise` | `{ Clockwise, CounterClockwise }` — no member means "no rotation asked for" |
| `ScaleDirection` | `Up` | `{ Up, Down }` — same shape |
| `PathSegmentKind` | `Line` | `PathSegment.Kind` is init-only with no default at `PathSegment.cs:35` |
| `SnapAnchor` | `Center` | `Center` is arguably the genuine default anchor; weakest of the four |

### Class C — most-common-default, compliant under the guideline's second branch

`MapTool.Draw`, `DrawState.Idle`, `NotebookKind.Dungeon`, `NotebookPane.Description`,
`CanvasCursorShape.Arrow`, `BehaviorCursorShape.Hand`, `FloorListBandKind.FloorCard`, `ErrorCategory.Validation`.

**`ErrorCategory` carries measured evidence and is not merely a judgment call.** It is stored as
`ErrorCategory?` at `Result.cs:31` and `ResultOfT.cs:39`, so absence is already modelled by `null` rather than
by a zero member. Its zero value is not reachable as "no category supplied".

Class C is a judgment about intent supported by usage, not a measurement. Class A is measured.

## 4. What this measurement does not settle

- Whether Class B is in scope. Four enums, four separate design decisions, and none has a named defect the way
  `MutationOutcome` does.
- Whether the resolution is a `None` member, as `MASTER-073` resolved, or something else per enum.
- The audit entry or entries this becomes. None of the fourteen is filed anywhere today.
