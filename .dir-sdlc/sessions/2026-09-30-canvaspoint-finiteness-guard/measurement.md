# Measurement — session 6b-xv

Entry under measurement: **`MASTER-081`** (`docs/audit/00-master-index.md:269`) / **`CORE-016`**
(`docs/audit/01-domain-application.md:861-899`).

**Fixed commit: `75e80ee`**, branch `chore/repo-audit`, measured in the **main checkout** at
`C:\Users\dmarq\Development\Repos\dungeon-master`, working tree clean.

```
git rev-parse --short chore/repo-audit        -> 75e80ee
git status --porcelain                        -> (no output)
git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit -> 0  0
```

**Convention of this file, and it is the one 6b-xiv's §0 rule is about: every figure below carries the
command that produced it, on the same row or the row beneath it.** A figure with no command beside it in
this document is a defect in this document, not a shorthand. 6b-xiv's single false claim was its only row
with a figure and no command, and it was false in both directions at once.

---

## 1. The six cited locations

`CORE-016`'s `locations:` block lists **6** ranges — counted by reading `01-domain-application.md:870-876`,
which is the whole block. (Not 8. Eight is `CORE-015`'s count, and conflating the two is how 6b-xiv's
first false claim started.)

| # | Cited | Verdict | What is actually there |
|---|---|---|---|
| 1 | `Entities/Floors/ObjectScale.cs:3-12` | **correct** | `public static class ObjectScale` opens at `:3` and closes at `:12`; the file is 12 lines |
| 2 | `Entities/Floors/PlacedObject.cs:28-34` | **correct** | the constructor's scale guard, `if` at `:28`, closing brace at `:34` |
| 3 | `Entities/Floors/PlacedObject.cs:80-86` | **STALE, −4** | the `Update` guard is at **`:76-82`** |
| 4 | `ValueObjects/CanvasPoint.cs:9` | **correct** | `public record CanvasPoint(float X, float Y);` |
| 5 | `ValueObjects/TilePoint.cs:12` | **correct** | `public record TilePoint(int Col, int Row);` |
| 6 | `ValueObjects/PathSegment.cs:18-22` | **STALE** | now the `<param name="Ctrl2">` doc block; the record declaration is `:25-29` and its validation body `:35-79` |

**4 of 6 correct, 2 stale.** Every range above was read with `cat -n <file>` over the whole file and
compared line by line — not with a line-range `sed`, which cannot show that a range has moved.

```
wc -l src/DungeonMaster.Domain/Entities/Floors/ObjectScale.cs   ->  12
wc -l src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs  ->  89
wc -l src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs      ->   9
wc -l src/DungeonMaster.Domain/ValueObjects/TilePoint.cs        ->  12
wc -l src/DungeonMaster.Domain/ValueObjects/PathSegment.cs      ->  80
```

**Row 3's drift has a named cause**: the `CORE-019` scale-less overload removal (session
`2026-09-29-scaleless-overload-removal`) deleted the four-argument constructor and the two-argument
`Update` from this file. `PlacedObject.cs` is **89** lines now.

---

## 2. The six assertions

### Holds (4)

**(a) "There is no `Policies/` directory."** True.

```
find src/DungeonMaster.Domain -type d | grep -v '/obj/\|/bin/'
  -> src/DungeonMaster.Domain
     src/DungeonMaster.Domain/Entities
     src/DungeonMaster.Domain/Entities/Areas
     src/DungeonMaster.Domain/Entities/Dungeons
     src/DungeonMaster.Domain/Entities/Floors
     src/DungeonMaster.Domain/ValueObjects
```

Six source directories, none named `Policies`.

**(b) "`ObjectScale` holds the bounds as a static class of `const float`s plus two clamping helpers."**
True, with one precision: it holds **four** `const float`s, not only the bounds — `Min`, `Max`, `Step`,
`Default` at `:5-8` — plus `StepUp` and `StepDown` at `:10-11`. Read from `cat -n`, whole file.

**(c) "written twice ... as an inline `if` in the `PlacedObject` constructor and again in
`PlacedObject.Update`, each throwing `ArgumentOutOfRangeException` with the same interpolated message."**
True in substance. **"Verbatim" is imprecise and the imprecision is worth recording**: the two conditions
differ in the identifier —

```
:28   if (float.IsNaN(scale) || float.IsInfinity(scale) || scale < ObjectScale.Min || scale > ObjectScale.Max)
:76   if (float.IsNaN(newScale) || float.IsInfinity(newScale) || newScale < ObjectScale.Min || newScale > ObjectScale.Max)
```

— while the message string **is** byte-identical at `:33` and `:81`:
`$"Scale must be between {ObjectScale.Min} and {ObjectScale.Max}."`

**(d) "`CanvasPoint` ... accept[s] any input, including `float.NaN`."** True, and §3 shows it is worse
than the entry says.

### Falsified (2) — and these are the two the entry's argument rests on

**(e) "because `AddPlacedObjectCommand` has no validator (CORE-009) an out-of-range scale reaches the
entity and surfaces as an exception rather than a validation result." — FALSE.**

`MASTER-079` / `CORE-009` is closed:

```
grep -n "CORE-009" docs/audit/00-master-index.md
  -> 267:| 79 | MASTER-079 | S3 | CORE | structure | M | low | Ten of the fourteen commands have no
        validator, making ValidationBehavior a no-op for them | CORE-009 | **done** |
```

Both validators exist and both bound scale, read in full with `cat -n`:

- `Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommandValidator.cs:38-42`
- `Features/Dungeons/Commands/UpdatePlacedObject/UpdatePlacedObjectCommandValidator.cs:37-41`

both `.InclusiveBetween(ObjectScale.Min, ObjectScale.Max)`, and each carries a doc comment at `:13`
stating the bounds reference `ObjectScale` *"so that this validator cannot drift from the domain guard."*

The dispatch path is pinned by tests: `CommandValidatorCoverageTests.cs:155` (a scale above `Max`) and
`:187` (a scale below `Min`), both asserting rejection **before the handler**.

> **`InclusiveBetween` also rejects `NaN` and `±Infinity`.** As first written, this document justified that
> by "every ordered comparison against `NaN` is false and `+Infinity > Max`", and flagged it in §5 as
> reasoned rather than run.
>
> **CORRECTED after T2 ran it and T2's reviewer measured the mechanism. The conclusion stands; the
> justification was underspecified and is withdrawn.**
>
> The verdict is **rejected, all three values, on both validators** — six rejections, each a single failure
> with error code `InclusiveBetweenValidator`, plus `ObjectScale.Default` valid on both as a control
> (`NonFiniteScaleRejectionTests.cs`, and reproduced in the reviewer's own full-solution run).
>
> **The mechanism is not IEEE-754 comparison semantics.** `InclusiveBetween` compares through
> **`FluentValidation.Validators.ComparableComparer<float>.Instance`**, whose total order sorts `NaN` below
> everything. Established by loading the shipped `FluentValidation.dll` and invoking the comparer:
> `Compare(NaN, 0.25f) = -1`, `Compare(NaN, 4.0f) = -1`, `Compare(+Infinity, 4.0f) = 1`,
> `Compare(-Infinity, 0.25f) = -1`.
>
> **Why the original justification was worth nothing, in either direction.** T2's first report said the
> reasoning above would have produced *the opposite* verdict. Both statements were reasoning from a
> different implicit form of a range check, and **neither named its form**:
>
> | Form | On `NaN` |
> |---|---|
> | positive — `if (!(v >= from && v <= to)) fail` | **rejects**, because `NaN >= Min` is `false` |
> | negative — `if (v < from \|\| v > to) fail` | **accepts**, because both comparisons are `false` |
>
> This document's conclusion was right for the positive form, which it did not name. T2's correction of it
> was right for the negative form and overbroad as stated; T2's reviewer caught that too, and T2's doc
> comment was narrowed to the negative form in revision round 1. **The lesson is not that one party was
> right — it is that a mechanism nobody ran produced two confident, incompatible readings from the same
> two true sub-facts.** §5 declining to claim it was the only thing that worked here.

**(f) "The three value objects under `ValueObjects/` are plain positional records that accept any input."
— FALSE for `PathSegment`. Two of three, not three.**

```
grep -n "MASTER-170" docs/audit/00-master-index.md
  -> 358: ... | **done** (6b-vii, 2026-09-16) ...
```

`PathSegment.cs:35` redeclares `Kind` with an initializer calling `ValidateControlPoints` (`:40-79`),
which throws `ArgumentException` in all four directions. It is not a plain positional record.

**`ValueObjects/` holds four files, three of which are records**: `CanvasPoint.cs`, `TilePoint.cs`,
`PathSegment.cs`, and `PathSegmentKind.cs` — the fourth is the enum. `ls src/DungeonMaster.Domain/ValueObjects/`.
So the entry's "three" is the right population; it is the predicate "plain, accepts any input" that is now
false for one of them.

---

## 3. What the measurement found that the entry does not address

**`CanvasPoint` has no finiteness guard, and neither does anything upstream of it.**

```
grep -rn "IsNaN\|IsInfinity\|IsFinite" --include=*.cs src/ | grep -v '/obj/\|/bin/'
  -> src/DungeonMaster.App.Core/Editor/FloorReorderNearest.cs:155:   ... !double.IsFinite(y)
     src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:28:    float.IsNaN(scale) || float.IsInfinity(scale) ...
     src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:76:    float.IsNaN(newScale) || float.IsInfinity(newScale) ...
  count -> 3
```

**Three non-finite guards in the entire source**, two of them the duplicated pair under measurement and
the third unrelated (a `double` in the floor-reorder hit test).

**Predicate check, per the census rule — a census validates its predicate, not only its total.**
A case that must match: `PlacedObject.cs:28`, matched. A near-miss that must not: `ObjectScale.StepUp`'s
`MathF.Min(current + Step, Max)` at `:10`, which clamps rather than guards — correctly not matched. The
population is 3 and was read in full rather than counted, so a wrong predicate cannot hide in a tail.

**Position is guarded by `.NotNull()` and nothing else.** Read, not grepped — both validator files in
full: `AddPlacedObjectCommandValidator.cs:30-32` is `RuleFor(command => command.Position).NotNull();`
and there is no second rule on `Position` anywhere in either file.

**It is persisted as two required columns.** `PlacedObjectConfiguration.cs:43-54` maps `Position` with
`OwnsOne` into `Position_X` and `Position_Y`, both `.HasColumnType("REAL")` and both `.IsRequired()`.

So `new CanvasPoint(float.NaN, float.NaN)` is constructible, passes both validators, reaches the entity,
and is handed to EF. **What SQLite then does with it is NOT measured here and is not claimed.** See §5.

---

## 4. Pricing the entry's suggested direction

`CORE-016` cites `backend/backend-conventions.md#5. Policy Conventions` in the separate `dm-conventions`
repository at `C:\Users\dmarq\Development\Repos\dm-conventions` (present; `ls -d` confirms).

§5 is at `backend/backend-conventions.md:165`, `grep -n "^#.*[Pp]olicy"`. It requires two classes per
concept — a policy that **returns a structured result and never throws**, and a value object that **owns
state, delegates to the policy, and throws a domain exception**.

| Ingredient the direction needs | Present in `src/`? | Command |
|---|---|---|
| `PolicyResult` | **no — 0 hits** | `grep -rn "PolicyResult" --include=*.cs src/ \| grep -v '/obj/\|/bin/'` |
| `DomainException` | **no — 0 hits** | `grep -rn "DomainException" --include=*.cs src/ \| grep -v '/obj/\|/bin/'` |
| `Policies/` directory | **no** | the `find` in §2(a) |
| An unmapped `Scale` to convert freely | **no** | `PlacedObjectConfiguration.cs:34-35` maps it `.HasColumnType("REAL")` |

**Three findings about the cited authority itself:**

1. **§5's own stated benefit is already delivered here by another mechanism.** It gives pre-validation as
   *"command validators call the policy directly to surface user-facing field errors before attempting to
   construct the value object"* — which the two validators do, referencing the constants instead of a
   policy.
2. **§5's cross-reference is broken.** It says *"For C# implementation shapes, see dotnet-format.md § 9"*.
   `grep -n "^## " backend/dotnet-format.md` shows **§9 is "XML Documentation Comments"**; the content is
   **§11, "Domain Building Blocks in C#"**, at `:552`. Off by two sections.
3. **The repository has already answered this question once, in the opposite direction.** `PathSegment`'s
   rule was implemented by throwing inline in the record — the shape §5 assigns to the value object and
   forbids for the policy. Whatever is decided here, it is a second answer to a question with a standing
   first answer, and that is worth saying out loud rather than discovering later.

`dm-conventions` is a **separate repository and is not modified by this session** — consistent with every
prior session in this stream.

---

## 5. Owed to a task, not claimed here

Three things this document deliberately did not assert, because they were reasoned rather than run. **All
three have now been run. Every one of them came back other than as reasoned, and the third is still not
fully closed.**

1. **What SQLite does with a `NaN` written to a `REAL NOT NULL` column** — throw on save, store `NULL`, or
   round-trip. **RUN by T1 (`3cef4dd`), reproduced and deepened by its reviewer. The answer is none of the
   three offered: it is a split.** `float.NaN` **throws** — `DbUpdateException` wrapping
   `InvalidOperationException("Cannot store 'NaN' values.")`, from **`Microsoft.Data.Sqlite`'s ADO layer
   refusing to bind the parameter**, not from SQLite and not from `IsRequired()`. Both **infinities
   round-trip intact**, the stored column's `typeof()` being `real` — read directly with
   `SELECT typeof(Position_X), Position_X, Position_Y FROM PlacedObjects`, which is stronger than the
   reload assertion and rules out an EF identity-cache artifact. **The persistence layer is therefore not a
   guard**: it stops one of three cases, at commit time rather than at the boundary that accepted the value.
2. **That `InclusiveBetween` rejects `NaN` and `±Infinity`** (§2(e)). **RUN by T2, mechanism measured by its
   reviewer. Conclusion confirmed, justification withdrawn** — see the corrected block in §2(e). It rejects
   all three, via `FluentValidation.Validators.ComparableComparer<float>.Instance`'s total order, not via
   IEEE-754 comparison semantics.
3. **Whether any production path can actually produce a non-finite `CanvasPoint`** — as opposed to the type
   merely permitting one. **RUN by T2. Answered for `ZoomLevel`; NOT closed for `PanOffset`, and deliberately
   left open.**
   - **`ZoomLevel`: no.** All six production writes to `ZoomLevel`/`PanOffset` were enumerated
     (`FloorMapDrawable.cs:439`, `:442`; `DungeonDetailPage.xaml.cs:500`, `:507`, `:508`, `:1632-1634`),
     confirmed three independent ways including a grep *restricted* to `obj/`|`bin/` returning zero, which
     closed the generated-code route T2's own census had filtered out. Every Windows caller supplies
     `ZoomLevel ± 0.1f` from a `1.0f` start.
   - **A premise of the orchestrator's own prediction was refuted here.** `Math.Clamp` does **not** clamp
     `NaN` — it returns `NaN` unchanged while clamping `±Infinity` to the bounds, and the snap guard at
     `DungeonDetailPage.xaml.cs:498` is also `false` for `NaN`. **The Windows conclusion holds because of the
     `#elif MACCATALYST` fence at `:600-616`, not because of the clamp**, and the MacCatalyst pinch path at
     `:612` is a live latent hole on that TFM.
   - **`PanOffset`: undecidable from this repository's source, and recorded as such by user decision
     (escalation E2).** `DungeonDetailPage.xaml.cs:1632-1634` accumulates raw `e.Touches[0].X/.Y` with no
     finiteness test, and only `ResetViewport` (`:507-508`) ever sanitizes it. One non-finite touch
     coordinate would make `PanOffset` permanently `NaN`, every later `ScreenToCanvas` `NaN`, and `:1442`
     persist it. **Whether the MAUI Windows pointer pipeline can deliver such a coordinate is a fact about
     an external framework's runtime, not about this repository**, and nothing here asserts it either way.

> **The scale path is closed; the position path is not.** `InclusiveBetween` now provably rejects every
> non-finite scale, while `Position` carries only `.NotNull()` — and `CanvasPoint` is a `record`, a reference
> type, so `.NotNull()` is not vacuous but checks null alone and never finiteness. That asymmetry, combined
> with item 1's result that infinities persist, is the live gap this session's guard closes.

---

## 6. Out of scope, recorded rather than fixed

- **The `Floor` lookup-and-guard repetition** carried forward by 6b-xiv (`FloorMethods.cs:90`, `:128`,
  `:162`; `FloorObjectMethods.cs:56`, `:95`) — untouched, still open.
- **The 8 lone CR characters in `.dir-sdlc/backlog.md`**, dating to 6b-iv. Measured again at the top of
  this session: `python -c "b=open('.dir-sdlc/backlog.md','rb').read(); print(b.count(b'\r')-b.count(b'\r\n'))"`
  -> **8**, unchanged. Ninth consecutive session leaving them alone.
- **`dm-conventions`' broken §5 cross-reference** and the singular-`Dungeon` namespace at
  `backend/dotnet-format.md:567` and `:588`, which this repository replaced with the plural form.
  Separate repository, deliberately not modified.
- **The policy layer itself.** Not built by this session, by decision at Gate 1. `MASTER-081` therefore
  cannot close as `done`.

---

## 7. Arithmetic for anyone citing a line number after this session

`.dir-sdlc/backlog.md` held **441** newlines at `75e80ee`
(`python -c "print(open('.dir-sdlc/backlog.md','rb').read().count(b'\n'))"`).

> The 6b-xv handoff's §5 says **439**. The difference is `75e80ee` itself, which appended the git-notes
> item after §5 was written. Not a defect — but it is the ninth session running where a handoff figure had
> drifted by the time it was read, which is why this file re-derives rather than quotes.
