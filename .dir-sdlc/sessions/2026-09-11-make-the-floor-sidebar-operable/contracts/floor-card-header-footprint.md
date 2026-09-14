# Contract C3 — the floor card header row's footprint

Frozen at Gate 2. **AMENDED 2026-09-12 after freeze**, on the authority of T2 running measurement the Gate 2 text had assumed: the 55x55 figure below is wrong and is marked so in place. Produced by `T2`, constrained further by `T3`, observed by `T5`.

This contract exists because of one decision and its history. At sub-session 5c's escalation **E5**
the user **declined** `MASTER-164`'s remedy, and the reason they declined it was the header-row
height change that came with it: a MAUI `Button` was believed to render at WinUI's default **55x55**
minimum against the `Label`'s 16x23.

> **AMENDED after `T2` ran — the 55x55 figure is WRONG.** `T2` measured the sibling buttons in a
> running application at **44x44**, with the header row **44** high, and shipped the conversion with
> the row height unchanged at 44; the reviewer corroborated it by two independent routes. See
> `observations/T2-keyboard.md`. This is not a typo being fixed: **55x55 is the figure the user's 5c
> escalation-E5 refusal rested on**, so the remedy was deferred on a measurement nobody re-took. The
> index carries the same wrong figure at `docs/audit/00-master-index.md:570` and `T6` corrects it as
> correction 11 (contract C4 §3). **The 16x23 figure in this contract is wrong too** — `T2` measured
> the `Label` at **13x19** — and `measurement.md` §3 repeated that **16x23**, not the 55x55; the
> orchestrator owns that file's correction (§7 there), not `T6`. Escalation **E1** re-took that decision this session on the strength of a
documentation round (`measurement.md` §3), and it resolved to the **pinned-footprint** `Button` —
the remedy *with the cost removed*, not the remedy the user declined.

**So an unchanged header-row height is not a nicety here. It is the condition on which the remedy was
re-accepted.** A `T2` that delivers keyboard focus and a taller header row has delivered the thing
the user said no to.

## 1. The pinned footprint — FROZEN

`FloorExpandToggle` after `T2`:

| Property | Required value |
| --- | --- |
| element type | `Button` (was `Label`) |
| `BackgroundColor` | `Transparent` |
| `BorderWidth` | `0` |
| glyph | `▸` collapsed / `▾` expanded — the same two characters, still flipped in `ApplyExpanded` |
| **visual glyph box** | ~16x23 — unchanged from the `Label` — **SUPERSEDED, see §1a** |
| **hit area** | **>= 23x23**, reached by `Padding`, not by growing the glyph — **SUPERSEDED, see §1a** |
| `Margin` | `0` — padding is the hit-area mechanism, margin is not |

### 1a. SUPERSEDED BY WHAT SHIPPED — the chevron is 44x44, by the user's decision

Two things in §1 are no longer true, and `T3` and `T5` read this file, so they are corrected here
rather than left for a reader to trip over.

1. **The 16x23 baseline is wrong.** `T2` measured the `Label` at **13x19**. 16x23 came from
   `00-master-index.md:570` and was never re-measured; `T6`'s correction 11 fixes it in the index.
2. **`T2` shipped the chevron at 44x44, not at the pinned ~16x23 visual with a >=23x23 padded hit
   area**, and **the user decided to keep it** at the T2 observation
   (`observations/T2-keyboard.md:54-60`): 44x44 matches the three sibling `Button`s in the same header
   row, and **the header row stayed 44 high**, so §2's invariant — the thing the whole remedy was
   re-accepted on — **held anyway**.

So the *constraint* §2 states survives intact and is what matters; the *mechanism* §1 prescribed did
not survive contact with the platform. `reviews/T2.md:45-46` records the open thread: the 44x44
footprint is **not** demonstrably produced by the `Padding` this contract names as the mechanism. That
is a live finding for Gate 3, not something any task should now "fix" by shrinking the chevron back.

**Consequences for the two tasks still reading this file.** `T5`'s bounding-rect fact asserts
`FloorExpandToggle` is **at least** 23x23, which 44x44 satisfies — no change needed. `T3` must not
resize the chevron; its footprint is settled.

The redundancy argument below is retained because it is still the reason a sub-minimum chevron *would*
have been acceptable, and because it is the citation `T6` needs for correction 11's context.

The 16x23 visual is smaller than Microsoft's general 23x23 minimum touch target and is sanctioned
anyway, by the redundancy carve-out their own touch guidance states: *"It's acceptable for click
targets to be smaller than the minimum size if that control has redundant functionality. For example,
the progressive disclosure triangles used by the tree view control are only 6x9 pixels."* The chevron
is redundant with the card-body tap, which stays. Cited in `measurement.md` §3.

## 2. The height invariant — FROZEN, and this is the one that decides `T2`

> **The floor card header row's rendered height, and the floor card's rendered height, are the same
> after `T2` and `T3` as they are at `b3b1641`.**

"Same" means: no observable change in the running application, and no change to the number of floor
cards that fit in the sidebar viewport at a 1920x1080 maximized window with 11 floors — the
configuration `measurement.md` §5 already used and recorded.

The 23x23 hit area fits inside a row whose other three children are **44x44** `Button`s (measured by
`T2`, not the 55x55 this contract originally asserted), so the row's
height is set by those siblings and the padded chevron does not raise it. **That is the reasoning,
not the measurement.** `T2` must *measure* it, because documentation does not settle it.

## 3. The two things documentation could not settle — MUST BE RUN, NOT ASSUMED

`measurement.md` §3 names these explicitly. `T2` must produce an observation for each, not an
argument:

1. **Does MAUI's `MinimumWidthRequest` / `MinimumHeightRequest` actually override the WinUI default
   `Button` minimum on Windows?** There is no documented number and no documented handler mapping.
   If it does not, the pinned footprint needs a different mechanism (a Windows handler mapping, or a
   different container) and that is an **escalation**, not a silent substitution.
2. **Does the header row height change?** See §2. Must be observed in the running application.

A `T2` report that asserts either from the API surface rather than from a run has not met this
contract.

## 4. What `T2` must NOT do — FROZEN

- **Do not make the card body, the root `Border`, or any layout focusable.** `IsTabStop`, `TabIndex`
  and `ITabStopElement` are absent from .NET MAUI 10's public API, and MAUI 9 removed the loophole
  deliberately — its own what's-new example is `<VerticalStackLayout Focus="..."/>` stating the
  handler "isn't executed because only input controls can gain focus". At the platform layer a
  `Border`/`Grid`/`StackPanel` is a `UIElement` but not a `Control`, and WinUI's keyboard
  accessibility guidance is that only `Control`-derived classes take focus. A task that tries this
  will burn its budget on an API that does not exist.
- **Do not introduce the MAUI Community Toolkit `Expander`.** It is a tap-driven `ContentView` with
  no keyboard or UIA documentation on any of its pages, and it explicitly warns against interactive
  controls in its header — which this card's three header buttons are.
- **Do not remove the card-body `TapGestureRecognizer`.** The chevron's small visual is sanctioned
  *because* the card tap is redundant with it. Removing the card tap removes the sanction.
- **Do not add `SemanticProperties.Description` to `FloorBorder`** — see contract C2 §2.

## 5. What `T3` inherits

`T3` moves `FloorDragHandle` out of the WinUI overlay-scrollbar gutter, or reserves that gutter.
Either way `T3` is bound by §2: the header row's height does not change. A `T3` remedy that reserves
the gutter by padding the `ScrollView` or by adding a right margin changes **width**, which is
permitted; one that adds a row or wraps the handle below the row is not.

`T3` also owns `AreaSidebarItem.xaml` from Gate 2 onward, for `MASTER-175`. **That is a behavior
attachment and nothing else** — a `HandCursorBehavior` added inside the existing `Border.Behaviors`
block on `ItemBorder`. It changes no size, no padding, no margin and no gesture, so nothing in this
contract's footprint rules is engaged by it. A `T3` that takes the opportunity to restyle the area
row has exceeded its task.
