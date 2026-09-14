# T3 observation transcript — drag handle, area cursor, missed drop

Run by the orchestrator with the user, per Gate 2's resolution of escalation E5. This is the evidence
`dir-reviewer` judges T3's criterion c5 against; without it that criterion is UNDETERMINED.

Protocol executed as T3 specified it, with the **amended step 3** T3 supplied after shipping c7.

## Conditions — and why the window size is the point

- Branch at `5146e6d` (T2's commit) plus T3's uncommitted changes.
- `dotnet build … -f net10.0-windows10.0.19041.0` → `0 Warning(s), 0 Error(s)`.
- App launched **restored, not maximized**, window set to **900x640** at (120, 80).
- **The sidebar scrolls**: only floors 1-6 fit, floor 11 is below the fold, so the WinUI overlay
  scrollbar is genuinely rendered over the handle column. Confirmed by screenshot
  `t3-obs-detail.png`.

Every earlier observation in this session was maximized at 1920x1080, where all 11 floors fit and **no
scrollbar renders at all** — the wrong configuration for testing a scrollbar-gutter remedy. T3 named
this risk in its protocol and it is the single thing this observation exists to get right.

Incidental confirmation visible in the same screenshot: the second card reads "Main Tave…", truncated
rather than wrapped, which is the `LineBreakMode="TailTruncation"` / `MaxLines="1"` T3 added to keep
the row height pinned for names longer than the narrowed column.

## What the observer reported

| Step | Observation | Result |
|---|---|---|
| 1 | Press the `⋮` handle on the third floor card, once, with the scrollbar present | **Took the press first time, cleanly.** A drag began; no fighting with the scrollbar |
| 2 | Hover an area row's body inside an expanded floor (not its notebook/delete buttons) | **Hand cursor appeared** |
| 3 | Release the drag on the thin seam between two cards | **Reordered, landing on the UPPER card** — exactly T3's prediction |
| extra | Did it reorder twice, or jump two positions? | **No.** No double-reorder observed |

**Criterion c5 is MET on all three steps.**

Consequences:

- **`MASTER-163` is fixed.** The remedy was reserving the 14-DIU gutter as right padding on
  `FloorListContainer` and widening the handle to 23x44 with `Padding="8,0"` +
  `VerticalOptions="Fill"` + `BackgroundColor="Transparent"`. Measured before the fix: the handle
  occupied X=284..291 of a 300-wide viewport while the overlay band begins at 286, so the scrollbar
  covered **five of its seven units**.
- **`MASTER-175` is fixed.** `AreaSidebarItem`'s root `Border` now carries `HandCursorBehavior`,
  matching the floor card body. This was the surviving half of the 2026-09-01 observation, and it was
  always **absence** rather than the inertness `MASTER-165` alleged.
- **The silent discard is fixed.** A drop on the seam now reorders instead of vanishing. The
  tie-break resolved to the upper card as designed.
- **The `ExistingTarget` guard held.** T3 could not verify from outside the app whether WinUI bubbles
  a drop from a card up to the container, so it wrote code correct under either answer and named
  double-reordering as the symptom if it were wrong. No double-reorder was observed, so the guard is
  doing its job — though note this is one trial, not a proof.

## New finding from the observer — placeholder indication is hit-or-miss

Reported unprompted during step 3:

> the placeholder highlight-effect drop indicator didn't indicate where the card would go, and a
> general scenario of placing the card back in its original spot doesn't show the placeholder
> highlight indicator in the eventual end position … the drag and drop behavior looks correct, but
> visually the drop placeholder indicator could be refined so that it always shows where the card will
> go, rather than the hit-or-miss visual behavior of right now

**This is not an oversight in T3's work; it is the disclosed cost of a risk T3 declined to take.** Its
report states plainly:

> **No placeholder on a gap hover.** `OnFloorListDragOver` only accepts the operation; it does not show
> a placeholder for gap hovers. That would mutate the visual tree under the pointer mid-`DragOver`,
> which is exactly C1 §5's candidate #1, and I was not willing to introduce it for polish.

C1 §5's candidate #1 is the mechanism a whole task (T1) was dispatched to investigate: `ShowDragPlaceholder`
mutating `FloorListContainer.Children` from *inside* the `DragOver` handler, with the code documenting
its own instability at `FloorSidebarItem.xaml.cs:395-397` ("DragOver then DragLeave oscillation =
stutter"). So the placeholder is shown only while hovering a **card**, never while hovering the seam
the new third drop target now accepts — and the destination the resolver picks can differ from the
card last hovered.

Making the indicator always show the true destination therefore means either mutating the tree under
the pointer — the thing this session has the most evidence against — or finding a presentation that
indicates position **without** reparenting anything, for example drawing an insertion line rather than
inserting a sized placeholder element. That is a design question, not a defect fix.

**Carried to `.dir-sdlc/backlog.md` at Gate 3**, not filed as an audit entry: the behaviour is correct
and the reorder lands where the resolver says; what is wanted is a better affordance.

## Side effect on user data, disclosed

Two reorders were performed against the real local database during this observation — step 1's drag
and step 3's seam drop. The floor order in `Test Dungeon #1` has changed accordingly. The user was
told before the earlier T1 observation that a successful reorder writes, and chose then to leave the
result rather than restore it.

---

## Handoff to T5 — the durable fact, recorded here so a task can reach it

T3's criterion c9 required it to state, in one sentence with numbers in it, the fact T5 is to freeze
as an Appium assertion. T3 stated it in its report, but **a report lives in the orchestrator's context
and not in any file a task can open** — the same reachability problem the architect raised about the
`13x19` figure. It is therefore recorded here, verbatim, as the artifact T5 reads:

> **`FloorDragHandle`'s right edge is at least 14 device-independent units left of the sidebar scroll
> viewport's right edge.**

T3's derivation, verbatim:

> Derivation: 14 is the measured overlay band. Actual measured clearance after the fix is **23** DIU
> (handle right edge at absolute X=277 of a 300 viewport); the band begins at 286, so the handle
> clears the bar itself by 9. T5 should assert 14 — the durable fact is "clears the gutter", and 23
> has slack a DPI change could eat.

**T5 asserts 14, not 23.** The margin between them is deliberate slack, not an error: asserting the
measured 23 would make the test fail on a DPI change that had not actually reintroduced the defect.

### The severity statement, also recorded for reachability

T3's c6 required its report to correct `MASTER-163`'s severity direction. T6 writes the index
amendment from this, so it is recorded here rather than left in a report:

> **`MASTER-163` is this session's primary defect, and its severity is corrected UPWARD, not
> downward.** The reorder feature itself works end to end when the drop lands on a target … What
> fails is **reaching** the drop target, and when the drop misses it fails **silently**, which is why
> the feature reads as broken. The entry's literal "cannot be performed at all" clause remains false —
> but the correction is that the real defect is **worse** than ergonomics, not milder than a blocked
> feature.
