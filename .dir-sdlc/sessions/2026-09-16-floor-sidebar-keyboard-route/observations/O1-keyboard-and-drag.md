# Observation transcript — the keyboard route and the pointer drag

Run by the orchestrator with the user on 2026-09-16, after all four tasks landed. This is the evidence
Gate 3's report rests on for definition-of-done items **D1**, **D4** and **D7**, and it settles the four
claims contract C1 §6 forbids any criterion from asserting from a source read.

Format follows 6b-iv's `observations/` transcripts, which are this project's precedent.

## Conditions

- Session branch `dir/2026-09-16-floor-sidebar-keyboard-route` at **`52e6213`**, all four task commits
  landed, `git status --porcelain` in `C:/dm-wt/6b-vi` **clean**.
- `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`
  → **`0 Warning(s), 0 Error(s)`**, elapsed 10.21s. No `DungeonMaster.App` process was running beforehand
  (`tasklist | grep -i dungeon` → no match), so no `MSB3026`/`MSB3027` lock.
- Launched from the **worktree** build, not the main checkout:
  `C:/dm-wt/6b-vi/src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`
- **Test Dungeon #1**, maximized. Focus reset by a click on empty page background before the Tab walk, so
  the walk starts from the page rather than from a clicked control.

## What the observer reported

| | Observation | C1 §6 | Result |
|---|---|---|---|
| ① | `Tab` reaches the drag handle, and `Ctrl`+`↑`/`Ctrl`+`↓` move the floor | **blocking** | **YES — and repeatedly** |
| ② | Pointer drag-reorder still works after the gesture rehosts onto a `Button` | **blocking** | **NO — BROKEN** |
| ③ | The header row's height is unchanged | report for decision | No height change reported; the focus border on the handle *"looks identical (visually) to other focused items"* |
| ④ | The floor card's width and the floor name are not squeezed | report for decision | **SQUEEZED** — `MAIN TAV…` became `MAIN TA…` |

### ① The keyboard route works, including on repeat — D1 MET

`Tab` reaches the drag handle `Button` and `Ctrl`+`↑`/`Ctrl`+`↓` reorder the floor. **The observer
confirmed it works repeatedly, not once.**

The repeat is the load-bearing half and it was tested deliberately. `RebuildFloorList()` clears
`FloorListContainer.Children` and destroys the `Button` that held focus, so without T3's
`FocusDragHandle()` restoration the route would have worked exactly one time and then gone dead — a defect
that would have looked like success in any single-press test. T3's reviewer had separately verified the
ordering this depends on (`OnFloorsChanged` calls `RebuildFloorList()` inline, synchronously, so the new
children exist when the `await` returns). **Observation confirms the mechanism end-to-end in the running
application.**

This also settles C1 §0's open question in the affirmative: `Plugin.Maui.KeyListener` **does** deliver a
`Ctrl`+arrow `KeyDown` carrying `e.Key` to the page handler while a `Button` holds focus. That was assumption
1 of the spec, rung `repo/package`, explicitly **not proven at runtime** until now.

### ② The pointer drag is BROKEN — D4 NOT MET

> *"Clicking and holding to drag-to-reorder floors does not produce any behavior."*

**This is the session's accepted risk materialising.** It was identified before implementation, named as the
largest risk at Gate 2, offered to the user as a choice against a lower-risk design, accepted knowingly, and
routed here rather than asserted either way. Every party declined to guess it:

- `measurement.md` recorded that the handle carries the repository's **only** `DragGestureRecognizer`, so
  there is no precedent for a `Button` host anywhere in this codebase.
- The spec's Assumptions table recorded it as **API-legal but behaviourally unestablished** — Microsoft's
  documentation confirms `GestureRecognizers` is a `View` member and `Button` derives from `View`, and says
  nothing about a WinUI `Button` capturing the pointer press.
- T3 disclosed it rather than claiming the conversion safe.
- T3's reviewer **found nothing that settled it and explicitly declined to treat its static read as proof**,
  offering only that the handle now carries a single gesture recognizer with the tap handler gone, so nothing
  within the element competes for the press — *"which removes one confound from the observation without
  answering it."*

The confound removal did not save it. A WinUI `Button` captures the pointer press, and MAUI's
`DragGestureRecognizer` does not start a drag from one.

**C1 §6 governs what happens next, and it is unambiguous:** observation 2 is blocking, and the session
**escalates rather than shipping something that reads as applied.**

### ③ The header row height — no regression reported

The observer reported no height change and noted the focus border on the handle *"looks identical
(visually) to other focused items"*. That also disposes of T3's reviewer's advisory **A6** — `DragHandle`
being the only one of the file's five `Button`s without a `CornerRadius` — as **not visible in practice**.

**Stated as a limit:** this is a visual report, not a measured one. No DIU measurement was taken, so the
44-DIU invariant 6b-iv recorded is **unmeasured here**, not re-established. The claim supported is "no
observer-visible height change", which is what C1 §6.3 asks for but weaker than a measurement.

### ④ The floor name is squeezed — D7 NOT MET

> *"it looks like floor name text is shortened (MAIN TAV... is now MAIN TA...)"*

**Exactly the consequence C1 §6.4 predicted and T3 disclosed.** The old `Label` measured 23 DIU wide;
`ExpandToggle`, the comparable `Button`, measures 44×44. A `Button`'s control template applies its own
measure that `Padding`, `Margin="0"` and `BorderWidth="0"` cannot pin away, so the handle widened its `Auto`
column and took the width from the floor-name column beside it.

This compounds a cost 6b-iv already recorded: the name column went 160 → 129 → 99 DIU across its T2 and T3,
and the name already carries `LineBreakMode="TailTruncation"` with `MaxLines="1"`, so a truncated floor name
**has no recovery affordance anywhere** — that is an open backlog item from 6b-iv, unfixed.

**The observer proposed a remedy**, recorded here verbatim as their own suggestion rather than as a finding:

> *"maybe reducing the space between the expand/collapse icon and the floor card edge would help? there is
> more space than is probably needed, so we can move elements closer to the card edges (not including
> areas… that spacing looks solid as-is)"*

**② and ④ are coupled, and that matters for the decision.** Both are consequences of one change — the
`Button`'s larger rendered footprint. Reverting the conversion resolves both at once; keeping it requires two
separate remedies.

## Disposition

- **D1 — MET.** Proven in the running application, including the repeat case.
- **D4 — NOT MET.** Blocking. Escalated.
- **D7 — NOT MET** in its width half. Reported for decision, per C1 §6.
- **C1 §0's open question — SETTLED YES.** The keys reach the handler while a `Button` holds focus.
- **T3 reviewer advisory A6 — DISPOSED.** The focus visual is not visually distinguishable from its siblings.
