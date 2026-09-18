# Measurement - session 6b-xi

Every figure here was produced by the command printed beside it, in the **main checkout** at
`061b28b`, before any task was dispatched. Nothing is carried from the handoff or from a prior
session. Where a count feeds a criterion, its predicate is validated by a must-match and a
must-not-match case, per the standing project rule.

`<PROD>` throughout means the five production projects -- `src/DungeonMaster.Domain`,
`src/DungeonMaster.Application`, `src/DungeonMaster.App`, `src/DungeonMaster.App.Core`,
`src/DungeonMaster.Infrastructure` -- with `/obj/` and `/bin/` hits filtered out.
`src/DungeonMaster.App.UiTests` is outside `src/DungeonMaster.sln` and is not production code.

---

## 1. The entry under measurement still holds

`MASTER-179` at `docs/audit/00-master-index.md:367`, status `**open** -- partially addressed`.

The two cited consumer sites are at the line numbers the entry gives, **unmoved**:

    grep -rn "DrawSessionResult" --include=*.cs src/

- `DungeonDetailPage.xaml.cs:1473` -- `if (result == DrawSessionResult.Added)`
- `DungeonDetailPage.xaml.cs:1489` -- `else if (result == DrawSessionResult.Closed)`

Neither carries any rejection of `DrawSessionResult.None`. The entry is **still worth a session**;
re-measuring is what establishes that, not only what catches errors.

**The producer's contract.** `MapDrawingSession.cs:11` documents `None` as "No placement attempt
was made. Never returned by `MapDrawingSession.PlaceVertex`." The value at `:1471` comes directly
from `PlaceVertex`, so **`None` is unreachable at both sites through the real call path**. This is
measured, not assumed: `PlaceVertex` has exactly three `return` statements, at
`MapDrawingSession.cs:70`, `:75` and `:79`, returning `Ignored`, `Closed` and `Added`
respectively, and none returns `None`.

    grep -n "return DrawSessionResult" src/DungeonMaster.App.Core/Map/MapDrawingSession.cs

**Consequence for the definition of done, stated before decomposition rather than discovered at
review:** no test routed through `PlaceVertex` can reach a rejection added at these sites. A
criterion demanding one would be unsatisfiable as written. `backlog.md:355` and `:389` are both
records of criteria that looked earned and were not.

---

## 2. The convention survey - what this repository does with an enum member that cannot occur

This is the question Gate 1 deferred to evidence. The user declined to pick a shape and asked for
the repository's own answer, counted rather than asserted.

### 2.1 The first scoping was wrong, and it is recorded rather than quietly replaced

The survey was first scoped as *"what do App-layer catch-all arms do?"* That produced a decisive
and **false** answer: 7 App-layer catch-alls, all absorbing, 0 throwing -- apparently establishing
that the layer's convention is to absorb, and that `MASTER-179` should be closed with no code
change at all.

That scoping was wrong, and the error is the project's signature one: **the population had never
been checked against the case under test.** See 2.3.

### 2.2 The raw counts

Catch-all arms in switch **expressions**, separated from lambda discards by a second method that
agrees with the first (9 + 7 = 16, matching the raw total of 16):

    grep -rn --include=*.cs '_ =>' <PROD>              # 16 total

| Kind | Count |
|---|---|
| Switch-expression catch-all arms | 9 |
| Lambda discards (not switch arms) | 7 |

**The shared-spelling blind spot was closed.** Both methods key on the literal `_ =>`, so both
would miss a named catch-all. There are none:

    grep -rnE '^\s*var \w+ =>' --include=*.cs <PROD>   # 0

Switch **statement** `default:` arms in production:

    grep -rn --include=*.cs 'default:' <PROD>          # 2 -- both absorb (set a cursor)

`Debug.Assert` and `Trace.Assert` in production:

    grep -rn --include=*.cs 'Debug.Assert' <PROD>      # 0
    grep -rn --include=*.cs 'Trace.Assert' <PROD>      # 0

**There is no assertion-based precedent in this repository at all.** Any criterion prescribing
`Debug.Assert` would be introducing a construct that appears nowhere in production.

### 2.3 The correction - the seven absorbing arms are not our population

Of the 9 switch-expression catch-alls, 8 absorb and 1 throws. Seven of the 8 absorbers are App
layer. But **every one of those seven is a value-producing switch expression**, where C# requires
a catch-all or exhaustiveness -- the arm exists because the language demands a value, not because
the author chose to absorb:

| Site | Catch-all yields |
|---|---|
| `ObjectKindInfo.cs:67` | `(1.00f, 1.00f)` |
| `ObjectKindInfo.cs:109` | `kind.ToString()` |
| `ObjectKindInfo.cs:129` | `"Decoration"` |
| `HandCursorBehavior.cs:145` | `HandCursor` |
| `FloorMapDrawable.cs:735` | `0f` |
| `PlatformCanvasCursor.Windows.cs:71` | `InputSystemCursorShape.Arrow` |
| `DungeonDetailPage.xaml.cs:1110` | `CanvasCursorShape.Arrow` |

**Our site is not that shape.** It is a statement-level `if`/`else if` chain that performs actions
and produces no value, so nothing is syntactically required of it.

The right comparison population is statement-level enum dispatch in the App layer. There are four
such sites, and **not one of them is an instance of our case**:

    grep -rnE '\bswitch\s*\(' --include=*.cs <PROD>

| Site | Switches on | Why it has no `default:` |
|---|---|---|
| `HoverOrLongPressBehavior.cs:133` | `GestureStatus` | Ignores `Running` -- a value that **legitimately occurs** and the site does not care about |
| `NotebookView.xaml.cs:512` | `GestureStatus` | Ignores `Completed`/`Canceled` -- same |
| `DungeonDetailPage.xaml.cs:606` | `GestureStatus` | Ignores `Completed`/`Canceled` -- same |
| `DungeonDetailPage.xaml.cs:676` | `NotebookKind` | **Exhaustive** -- covers `Dungeon`, `Floor` and `Area`, all three declared members |

So the App layer contains **zero** sites that silently ignore a value which *cannot* occur. Three
ignore values that can, and one is exhaustive. The apparent 7-to-1 split against rejection was an
artifact of a population that contains no member of the case under test.

### 2.4 The answer, correctly scoped: n = 1, and it throws

Exactly one production site handles an enum member documented as never-occurring:
**`MutationOutcomeExtensions.ToResult`**,
`src/DungeonMaster.Application/Common/Handlers/MutationOutcomeExtensions.cs`.

Its shape, at `:77-86`:

- a **dedicated arm for the never-returned member** -- `MutationOutcome.None =>` throwing
  `ArgumentOutOfRangeException(nameof(outcome), outcome, "No mutation outcome was reported.")`
- a **separate `_ =>` arm** for values outside the declared members, with a different message,
  `"Unrecognised mutation outcome."`
- an **`<exception>` doc block** at `:56-60` naming both cases distinctly

It is the direct sibling of the member this session is about: the same programme (6b-viii/6b-ix)
added `MutationOutcome.None` and `DrawSessionResult.None`, and `MASTER-073`'s resolution is
recorded at `backlog.md:361` as the project's template for these.

**Predicate validation.** A must-match and a must-not-match, per the standing rule:

- must match -- `MutationOutcomeExtensions.cs:77`, a dedicated never-occurring-member arm that
  throws. Counted. Correct.
- must **not** match -- `DispatcherLongPressTimer.cs:60`,
  `ArgumentOutOfRangeException.ThrowIfLessThan(delay, TimeSpan.Zero)`. A numeric argument guard on
  a `TimeSpan`, not an enum rejection. Not counted. Correct.
- near miss -- `ResultMessage.cs:68`, `_ => error ?? "The operation failed."`. A catch-all over an
  enum that absorbs. Counted as an absorber, not a rejecter. Correct.

**A count of 1 is a weak convention and is reported as one.** It is not "the repository always
does this"; it is "the repository has done this exactly once, in the closest analogue that exists,
and nowhere has it done anything else with a never-occurring member." Both halves belong in the
record. `backlog.md:383` and `:387` are the standing warnings about what a confidently-stated
number does in this project.

---

## 3. Layer and TFM facts that constrain the work

`DungeonDetailPage.xaml.cs` is in `src/DungeonMaster.App`, which builds for the Windows TFM
`net10.0-windows10.0.19041.0`. Neither 6b-ix nor 6b-x was on that TFM.

`DrawSessionResult` and `MapDrawingSession` are in `src/DungeonMaster.App.Core`, which is
plain-TFM and is exercised by `src/DungeonMaster.Tests`.

**This split is what makes the cardinality fact cheap and the rejection expensive**: the fact
added under item 2 of the scope lands in `DungeonMaster.Tests` against `App.Core` on the plain
TFM, while the consumer change forces the Windows build.

---

## 4. The adjacent item taken into scope at Gate 1

`backlog.md:401`, filed by 6b-ix: no test guards a future reordering of `DrawSessionResult` beyond
the three ordinals pinned today. Measured:

`DrawSessionResultZeroValueTests.cs:16-21` pins `Added`=1, `Closed`=2, `Ignored`=3. That is total
coverage of the enum as it stands, and **an inserted or appended member renumbers silently**. The
enum has **4** declared members -- `None`, `Added`, `Closed`, `Ignored` -- counted from
`MapDrawingSession.cs:8-29`. The same shape as the 38-member cardinality guard 6b-viii wrote for
`ObjectKind`.
