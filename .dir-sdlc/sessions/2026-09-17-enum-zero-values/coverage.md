# Coverage mapping — session 6b-ix, enum zero values

Written at Gate 2, before the approval that freezes `manifest.json`. The `Gate 2` columns are complete; the
`End state` columns and the `Implementing task` column are filled at Gate 3.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. `default(MutationOutcome)` is `None` | T1 (A1, A3) | covered |  met — T1/A1, A3 |
| 2. Member order `None, Success, NotFound, OutOfRange`, no explicit `= n`, ordinals 1/2/3 preserved | T1 (A1, A4) | covered |  met — T1/A1, A4 |
| 3. `ToResult` rejects the defaulted value, asserting `ParamName` and `ActualValue` | T1 (A6) | covered |  met — T1/A6, injected defect |
| 4. Undefined-value rejection still holds, proven by a separate injection | T1 (A7, A8, A9) | covered |  met — T1/A7, A8, A9, two separate injections |
| 5. XML `<exception>` wording no longer says "not a recognised member" | T1 (A10) | covered |  met — T1/A10 |
| 6. Existing `MutationOutcome` mapping behaviour preserved | T1 (A11) | covered |  met — T1/A11 |
| 7. `DrawSessionResult` shape, App.Core only, `PlaceVertex` never returns the zero member | T2 (B1, B4, B5, B12) | covered |  met — T2/B1, B4, B5, B12 |
| 8. The two comparison sites at `:1473` and `:1489` byte-for-byte unchanged | T2 (B8) | covered |  met — T2/B8 |
| 9. `CORE-034`, `CORE-035`, `UI-037` filed with index rows 177–179 | T3 (C1, C3, C8, C9) | covered |  met — T3/C1, C3, C8, C9 |
| 10. All fourteen named with zero member and class; denominator 17 | T3 (C3) | covered |  met — T3/C3 |
| 11. `CORE-017`'s amendment corrected beside its existing text | T3 (C11) | covered |  met — T3/C11 |
| 12. No new index row claims `done` or carries a commit hash | T3 (C10) | covered |  met — T3/C10, positive control |
| 13. CRLF on every file touched under `src/`, verified in binary | T1 (A12); T2 (B9) | covered |  met — T1/A12, T2/B9, both reviewer-reproduced |
| 14. Clean build and `0 failed` for every project touched | T1 (A13, A14); T2 (B10, B11) | covered |  met — T1/A13, A14, T2/B10, B11 |

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| C1 `MutationOutcome.None` | T1 | T1 — `ToResult`'s rejection, `MutationOutcomeExtensions.cs` | covered — **predicted `not called`** | **not called** — prediction **confirmed** |
| C1 ordinals 1/2/3 preserved | T1 | T1 — test assertion (A4) | covered — **predicted `not called`** | **not called** — prediction **confirmed** |
| C3 `ToResult` zero-member rejection | T1 | The existing Application command handlers, unchanged | covered | **called** — `MutationOutcomeExtensions.cs:37`, base-ref exception; see the deviation below |
| C3 undefined-value rejection | T1 | The existing Application command handlers, unchanged | covered | **called** — same call site and same exception |
| C2 `DrawSessionResult.None` | T2 | **none** | **uncovered** — user accepted at Gate 2: the inert fall-through is the intended effect; the untaken follow-up is filed in `UI-037` (C8) | **not called** — as accepted |
| C2 `PlaceVertex` never returns `None` | T2 | T2 — `MapDrawingSession.PlaceVertex`, and the two unchanged equality sites | covered | **called** — `DungeonDetailPage.xaml.cs:1471`; see the deviation below |

### The predictions, pre-registered

Both `C1` rows are predicted `not called`, and the reason is the discard rule rather than a hole in the work.
`MutationOutcome.None`'s definition site is `MutationOutcome.cs` and its only consumer is
`MutationOutcomeExtensions.cs` — **both owned by T1** — so the sibling-file discard removes the call, and the
base-ref exception does not rescue it because `None` does not exist at `80beda9`. The prediction is written
here, before the search, so that a `not called` result at Gate 3 reads as the rule working rather than as a
late excuse. If either comes back `called`, the prediction is recorded as refuted and the search's answer
stands.

### The uncovered row

`C2 DrawSessionResult.None` has **no production consumer**, and this is real rather than an artifact of the
discard rules. The only two production sites that read the enum are the equality comparisons at
`DungeonDetailPage.xaml.cs:1473` and `:1489`, and `B8` requires both to be left byte-for-byte unchanged. The
member's entire effect is that a defaulted value takes *neither* branch and no vertex is committed — a change
in what does not happen, which is not a call.

This is the sharpest row on the table and it is presented as a decision at Gate 2, not as an advisory note.

## Gate 2 decisions applied to the draft

Three edits were made to `manifest.json` before the freeze, all ordinary draft edits requiring no amendment record:

- The uncovered `C2` row was **accepted as uncovered**, with the omission filed by T3's `C8`.
- T3's `C3` and `C6` contradicted each other on `SnapAnchor`'s class; `C6` survives and `C3`'s list now reads
  class C, naming `C6` as the carrier of the evidence.
- T2 gained **`B12`, injection E**, covering `PlaceVertex`'s third return path (`Closed`, `MapDrawingSession.cs:70`).
  `B7`'s rationale asserted a universal over three return paths while only two injections were specified.

## The Gate 3 end-state fill

Range: `git merge-base chore/repo-audit dir/2026-09-17-enum-zero-values` = `80beda9`, to tip `476b510`.
Nine files touched, 447 insertions, 3 deletions.

### The predictions, resolved

Both `C1` rows were predicted `not called` at Gate 2, before the search, and both came back **`not called`**.
**Confirmed.** The reason is the one written down in advance: `MutationOutcome.None`'s only non-test consumer
is `MutationOutcomeExtensions.cs`, owned by the same task that owns the definition site, so the sibling-file
discard removes it, and the base-ref exception cannot rescue it because `None` does not exist at `80beda9`.
Nothing outside the range references the member at all — it is new.

`C2 DrawSessionResult.None` came back `not called` as the user accepted it at Gate 2. That is the decision
working as taken, not a hole the session left.

### Two rows record a deviation from the search rule, and here is why

**The rule scopes the search to files the session touched.** For two operations the genuine consumers are in
files this session deliberately did not touch, so a literal application would mark both `not called` — the
sharpest row on the table — against operations the application demonstrably reaches. Recorded by hand rather
than reported falsely:

- **`C3 ToResult`.** Eight Application command handlers call it — `RemoveAreaCommandHandler.cs:27`,
  `RemoveFloorCommandHandler.cs:25`, `RemovePlacedObjectCommandHandler.cs:27`, `ReorderFloorCommandHandler.cs:28`,
  `UpdateAreaCommandHandler.cs:30`, `UpdateAreaOutlineCommandHandler.cs:28`, `UpdateFloorCommandHandler.cs:29`,
  `UpdatePlacedObjectCommandHandler.cs:30` — and **none of those files is in the range**, because the contract
  left `ToResult`'s signatures unchanged precisely so they would not need to be. Within the range the call site
  is `MutationOutcomeExtensions.cs:37`, the two-argument overload forwarding to the three-argument one; it is
  owned by the definition site's task, so it survives only by the **base-ref exception** — verified with
  `git show 80beda9:src/DungeonMaster.Application/Common/Handlers/MutationOutcomeExtensions.cs`, where that
  forwarding call is present verbatim.
- **`C2 PlaceVertex`.** Called at `DungeonDetailPage.xaml.cs:1471`, a pre-existing caller that criterion B8
  required be left byte-for-byte unchanged — so the very criterion that protects the call site is what keeps
  the file out of the range.

**A call site still does not prove a user can reach it**, and neither row claims that. It upgrades "nobody was
assigned to call this" to "something calls this", which is all this check was ever designed to do.
