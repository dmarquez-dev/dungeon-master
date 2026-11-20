# Observation transcript — after the revert

Run by the orchestrator with the user on 2026-09-16, after commit `4c79364`. This is the evidence that
escalation **E1**'s remedy worked, and it is the **only** evidence behind the card-scoped focus gate, which
had neither a test nor an observation before this pass.

## Conditions

- Session branch at **`4c79364`**, all six commits landed, `git status --porcelain` in `C:/dm-wt/6b-vi`
  **clean**.
- `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`
  → **`0 Warning(s), 0 Error(s)`**, elapsed 10.13s. No `DungeonMaster.App` process running beforehand.
- Launched from the worktree build. **Test Dungeon #1**, maximized, focus reset by a click on empty page
  background before the Tab walk.

## What the observer reported

| | Observation | C1 §6 | Result |
|---|---|---|---|
| ① | `Ctrl`+`↑`/`↓` reorder from a focused **card**, repeatedly | **blocking** | **YES** |
| ② | Pointer drag-reorder works again | **blocking** | **YES** |
| ③ | The floor name is no longer squeezed | report | **YES** — reads `MAIN TAVE…` |
| ④ | Tab no longer stops on the handle glyph | report | **YES** — tabbing skips it |

### ① The card-scoped gate works, and works from every focusable child — D1 MET

> *"Works when starting with the chevron focused, as well as other icons (with the chevron then taking
> focus)."*

**This is the observation the session most needed, and it tested more than it was asked to.** The gate is
`ContainsKeyboardFocus`, a recursive walk over the card's visual tree. Had the walk been wrong — reaching
only direct children, say — the route would have worked from the chevron and silently failed from the
notebook and delete buttons. **The observer checked exactly that and it works from all of them**, which is
direct evidence the recursion reaches descendants at depth rather than just the first level.

That is the half no test covers. T4's reviewer established the recursion-at-depth case is the part most
likely to break under a later template change and the part no reviewer can eyeball; this observation is
currently the only thing standing behind it.

**The clause in parentheses also confirms focus restoration:** *"with the chevron then taking focus"*. After
a reorder, `RebuildFloorList()` destroys the focused element, and `FocusReorderAffordance()` deliberately
restores focus to the card's `ExpandToggle` rather than to whatever held it before. The observer saw focus
land on the chevron after reordering from a different button — **the disclosed behaviour change from
amendment A1 §3A, observed working as designed.** And the route repeats, which is what that restoration
exists for.

### ② The pointer drag is restored — D4 MET

> *"Works."*

The regression that escalation E1 existed to undo is gone. The handle is byte-for-byte the element 6b-iv
observed starting a drag on 2026-09-11, with its `DragGestureRecognizer` and both handler names intact.

**Both blocking observations from C1 §6 are now satisfied**, which they were not after O1.

### ③ The floor name recovered — D7 MET, with one detail recorded rather than smoothed

> *"Yes, the name is back to MAIN TAVE…"*

**The two reports do not use the same string, and that is recorded rather than reconciled.** O1 reported the
squeeze as *"MAIN TAV... is now MAIN TA..."*. O2 reports the recovered state as *"MAIN TAVE…"* — **one
character longer than O1's own pre-revert baseline of `MAIN TAV…`**.

Three readings are consistent with the evidence and **this session did not measure which**:

1. The observer approximated in one or both reports — both are informal transcriptions of a truncated label.
2. The revert recovered slightly more width than the `Button` had taken, e.g. because T3 dropped
   `BorderWidth` and `Margin="0"` from the restored `Label` per A1 §1A.
3. The pre-session baseline was always `MAIN TAVE…` and O1's `MAIN TAV…` was the approximation.

**No DIU measurement was taken in either pass**, so the supported claim is "the name is visibly longer than
after the `Button` and no longer reads as squeezed" — not "the column returned to exactly its `7483b57`
width". Recorded because the difference is one character and the temptation to call it identical is exactly
the restated-claim habit this project keeps catching.

### ④ Tab skips the handle — the disclosed cost, observed

> *"Yes, tabbing skips the glyph."*

**This is the accepted cost of E1, confirmed in the running application rather than assumed.** The handle is
a `Label` again, takes no keyboard focus on WinUI, and is therefore no longer its own tab stop. A
screen-reader user meets its `Hint` — which still names the `Ctrl`+arrow keys, per A1 §5A — as static text
rather than as a focusable control.

`MASTER-176`'s requirement is that *a keyboard-only user can reorder floors*, and ① establishes that they
can. The weaker discoverability is a real, disclosed reduction against what the `Button` briefly offered, and
the user accepted it at E1 knowing the trade.

## Disposition

- **D1 — MET.** Proven in the running application, including the repeat case and from multiple focusable
  children of the card.
- **D4 — MET.** The pointer drag is restored.
- **D7 — MET** in substance; see ③ for the measurement limit.
- **C1 §6 observations 1 and 2, both blocking — both now satisfied.**
- **The card-scoped gate has an observation behind it.** It still has **no test** — see T4's recommendation
  and its reviewer's correction, carried to the backlog.
