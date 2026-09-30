# Coverage mapping — session 6b-xv

`Gate 2` column written at Gate 2, before the approval that froze `manifest.json`. `End state` column filled
at Gate 3 by joining each item to its covering task's per-criterion verdicts in `reviews/<task-id>.md`.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. `CanvasPoint` rejects a non-finite coordinate in both coordinates, for `NaN` and both infinities, naming the offending coordinate as `ParamName`, and through a `with` expression as well as the constructor | T3 (c1–c6) | covered | **met** |
| 2. The `PlacedObject` scale guard exists once, not twice; both `ParamName`s and the `ActualValue` survive the extraction | T4 (c1–c6) | covered | **met** |
| 3. `MASTER-081`'s Status cell records `part done` in the unbolded shape used at `:282` and `:296`, naming what was and was not built | T5 (c1–c2) | covered | **met** |
| 4. `CORE-016`'s falsified assertions and stale ranges corrected in place, every range re-read from the file | T5 (c3–c11) | covered | **met** |
| 5. What SQLite does with a non-finite value in a `REAL NOT NULL` column — **run**, not reasoned | T1 (c1–c5); **evidence preserved by T6 after amendment A2** | covered | **met** |
| 6. Whether `InclusiveBetween` rejects `NaN` and both infinities — **run**, not reasoned | T2 (c1–c3) | covered | **met** |
| 7. Whether a production path can produce a non-finite `CanvasPoint`, every assignment site checked and named | T2 (c4–c7) | covered; **predicted `no reachable path` on the shipping Windows TFM** | **met in substance, per escalation E2** — c7's letter unmet; see below |
| 8. Suite green per-assembly against the dispatch baseline, and `dotnet build` clean by `grep -cE "(warning\|error) [A-Z]+[0-9]+"` | T1 c7, T2 c10, T3 c13, T4 c11 | covered | **met** |

**No uncovered rows at Gate 2, and no `not met` end state.** One row is `met in substance` by explicit user
decision rather than on its letter, and it is rendered in full below because that is an exception.

### Item 7 — the letter, and the prediction

**Criterion 7's letter is unmet and that is recorded rather than smoothed over.** It asked whether *any*
input reachable from the persistence path can leave `ZoomLevel` or `PanOffset` non-finite. T2 resolved it
**NO for `ZoomLevel`** and **"not decidable from this repository's source" for `PanOffset`**, which does not
resolve the disjunction. The reviewer marked it `not met` and **declined to rule**, escalating instead,
because satisfying the letter would require asserting whether the MAUI Windows pointer pipeline can deliver a
non-finite touch coordinate — a fact about an external framework's runtime, not about this repository.

**The user accepted it as met-in-substance at escalation E2**, and `PanOffset` is recorded as an open question
in the audit register rather than as a closed finding.

### The prediction's verdict — partly confirmed, one premise refuted, scope incomplete

The `Gate 2` cell predicted `no reachable path` on the shipping Windows TFM. **The honest verdict is three
things at once, and none of them is a clean `confirmed`:**

| | |
|---|---|
| **`ZoomLevel`** | **CONFIRMED.** All six production writes enumerated; the generated-code route closed by a grep *restricted* to `obj/`\|`bin/` returning zero; every Windows caller supplies `ZoomLevel ± 0.1f` from a `1.0f` start |
| **A premise of the reasoning** | **REFUTED.** `Math.Clamp` does **not** clamp `NaN` — it returns `NaN` unchanged while clamping `±Infinity` to the bounds, and the snap guard at `DungeonDetailPage.xaml.cs:498` is also `false` for `NaN`. **The Windows conclusion holds because of the `#elif MACCATALYST` fence at `:600-616`, not because of the clamp** — and the MacCatalyst pinch path at `:612` is a live latent hole on that TFM |
| **`PanOffset`** | **NOT COVERED by the prediction at all.** The prediction's reasoning was entirely about `ApplyZoom`. Site 6 (`DungeonDetailPage.xaml.cs:1632-1634`) accumulates raw touch coordinates with no finiteness test and only `ResetViewport` sanitizes it. This is a **scope gap in the prediction**, not a refutation of it |

**The search's answer stands over the prediction.** The prediction was written into this file before T2 ran
anything, precisely so that a disagreement would survive into the artifact instead of being quietly
overwritten by whichever half turned out right. It did.

## Contract operations

**Empty. This session declared no contracts.** `manifest.json` carries `"contracts": []`.

No task consumed an interface another task produced — T3 and T5 consumed T1's and T2's *findings*, which
landed as assertions and class-level XML doc comments inside those tasks' own test files. **No contract was
invented to fill the slot**, so the end-state search has nothing to run and the sharpest row this table can
produce — an operation with an implementer and no caller — cannot arise.

## Amendment A2 added a task, and that does not add a definition-of-done row

**T6 was added mid-session by amendment `A2`, authorized by escalation `E4`.** It carries no row of its own.
The definition-of-done items come from `spec.md`, which is signed rather than frozen and is not amendable in
that sense; and T6 exists to **preserve item 5's evidence** after T3's guard made T1's route to it
unreachable, not to cover a new outcome. Item 5's `Covering task` cell names T6 for that reason.

**Amendment `A1` changed T3's criterion 9 rather than any contract**, so it re-derives nothing here.

## One correction to the architect's report, recorded at Gate 2 and still standing

The architect's decomposition report cited `ApplyZoom` at **`FloorMapDrawable.cs:496-503`**. `ApplyZoom` is at
**`DungeonDetailPage.xaml.cs:496`** — the right line number attached to the wrong file.
`FloorMapDrawable.cs:496-503` is `canvas.RestoreState()`, `RaiseRedrawRequested` and the opening of
`DrawGrid`.

**No acceptance criterion inherited the error**, so nothing in the manifest carried it. T2 independently
confirmed the correction and called the citation "the wrong **file** but the right line range."
