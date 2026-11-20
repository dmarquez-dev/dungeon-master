# T2 observation transcript — keyboard reachability of a floor's areas

Run by the orchestrator with the user, per Gate 2's resolution of escalation E5. This is the evidence
`dir-reviewer` judges T2's criterion c6 against; without it that criterion is UNDETERMINED.

Protocol executed as T2 specified it.

## Conditions

- Branch at `5d4d0b7febff8f9ed4cb59cd111f61f935ca3d3d` (T1's commit) plus T2's uncommitted changes.
- Worktree contained exactly T2's three owned files; the temporary measurement probe was verified
  reverted (`grep -c "DM_PROBE_PATH\|Probe"` on the code-behind → **0**).
- `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`
  → `0 Warning(s), 0 Error(s)`.
- App launched, maximized on the 1920x1080 primary display, **Test Dungeon #1** open, 11 floor cards,
  all collapsed. Focus reset by a click on empty page background so the walk starts from the page and
  not from a clicked control.

## What the observer reported

| | Observation | Result |
|---|---|---|
| (a) | Focus ring reaches a floor card's chevron | **YES** |
| (b) | Space on the chevron expands the floor | **YES** |
| (c) | `+ Add area` becomes reachable by further Tab | **YES** |
| (d) | Card-body click still expands after the chevron has been used | **YES** |

**Criterion c6 is MET** — its bar is (a), (b) and (c) all yes.

Baseline for comparison, from T2's report: at `b3b1641` a 40-press Tab walk reached the chevron
**zero** times, because `ExpandToggle` was a `Label` and a `Label` cannot take focus on WinUI.

(d) was the optional regression check and it matters: T2 found and fixed a real defect mid-task. Its
first handler set `_suppressCardTap` following the delete/notebook convention, but `Button.Clicked`
does not reach the card's `TapGestureRecognizer`, so the flag leaked and swallowed the *next*
card-body tap. T2 discriminated that with a control run rather than reasoning about it, then made the
handler delegate exactly as the card tap does and let the page's `SelectRequested` handler own the
collapse. (d) confirms the fix holds in the shipped build.

## Independent corroboration of the height invariant

Not part of the protocol, and recorded because it comes from outside T2's own instrument.

Comparing the pre-T2 screenshot (`t1-obs-before.png`) with the staged post-T2 screenshot
(`t2-obs-staged.png`), the eleven floor cards occupy **identical y positions** in both — same first
card, same last card, same 58px pitch. The only visible difference is horizontal: the chevron column
widened and the floor names shifted right.

That is consistent with T2's instrumented measurement of header Grid `282x44` → `282x44` and floor
card `300x54` → `300x54`, and it corroborates criterion c4 by a different route.

## Escalation E7, resolved at this stop

T2 shipped the chevron at **44x44** rather than the contract's pinned ~23x23, because
`MinimumWidthRequest="23"` is a bare literal that fails the repository's own
`GeometryScaleTests.NoBareGeometryValues` ratchet, and the legal remedy — a token in
`Resources/Theme/Tokens.xaml` — lives in a file T2 does not own. T2 ran the ratchet rather than
assuming it, and quoted the failure.

**User's decision: keep 44x44 as shipped.** It matches the three sibling `Button`s in the same header
row exactly, clears the touch-target minimum outright instead of relying on the progressive-disclosure
redundancy carve-out, and introduces no new theme token. The accepted cost is horizontal: the chevron
column widens 13 → 44 and the floor-name label narrows 160 → 129. Height is unchanged either way,
which is the invariant that mattered.

The option of adding a `DmDisclosureTargetMinimum` token and pinning 29x27 was declined, as was
recording the file in `MinimumSizePendingMigration` — both T2 and the orchestrator recommended against
the latter, since that census documents undecided legacy cases and using it to wave through a
deliberate new value would degrade the ratchet for every later reader.

## A correction this task forced on the audit record

T2 measured the sibling buttons at **44x44**, not 55x55, and the header row at 44 high.

`docs/audit/00-master-index.md:570` states the remedy's cost as "a header-row height change, since
those siblings render 55x55 against the `Label`'s 16x23". `measurement.md` §3 repeated it. **Both are
wrong**, and the wrong number is the one the user's 5c escalation-E5 refusal rested on. The invariant
still held; only the reasoning behind it was off.

This is a correction T6 must make, and it is not in the ten corrections `audit-record-allocation.md`
§3 currently enumerates.
