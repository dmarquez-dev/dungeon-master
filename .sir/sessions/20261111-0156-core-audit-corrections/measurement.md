# Spec-phase re-measurement of backlog rows :96, :97, :98

Measured by the spec-phase orchestrator on 2026-11-11, in the main checkout at `9f9ca69`
(`chore/repo-audit`, identical to the session worktree's HEAD). File under measurement:
`docs/audit/01-domain-application.md` (abbreviated `01`). Line numbers are at `9f9ca69`.

`01` byte census (Python `open(f,'rb')`): CR 3552, CRLF 3552, LF 3552 -- all CRLF, no lone CR.
`.sir/backlog.md`: CR 187, CRLF 187, LF 187 -- all CRLF, no lone CR.

## M1 -- row :98, `CORE-015` Correction 4 (VERDICT: TRUE)

Correction 4 sits at `01:837-842`, inside the `AMENDED 6b-xiv` block that opens at `01:792`. Its
sentence reads "`grep -c "if (outcome is not MutationOutcome.Success)"` over the same file returns
**5**" (`01:838`) and names no commit; Corrections 1-3 name `20b0b09` explicitly (`01:807`, `:812-815`,
`:824`).

```
P=src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs
git show 20b0b09:$P | grep -c 'if (outcome is not MutationOutcome.Success)'   # 5
git show 9f9ca69:$P | grep -c 'if (outcome is not MutationOutcome.Success)'   # 1
git show 9f9ca69:$P | grep -c 'MutationOutcome'                               # 33 (control: file is the right file, token present)
git log --oneline 20b0b09..9f9ca69 -- $P
# 66a0b73 refactor(domain): extract floor lookup and outcome-touch helpers from DungeonMethods
```

The control that must match is the `5` at `20b0b09` (the figure the paragraph records). The single
commit touching the file since is 6b-xiv's own extraction, `66a0b73`. The row's claim holds exactly.

## M2 -- row :97, `CORE-019` `AMENDED 6b-xiv` preamble (VERDICT: TRUE)

The block opens at `01:1402`. Its universal, `01:1411-1413`: "Every figure attributed to a historical
commit was measured by this session's orchestrator and handed over with the command that produced it."
Its exception sentence, `01:1414-1418`: "**The one exception is the eight-row drift table below, with
the closing-line range beside it**: those were measured at Gate 3 under amendment A1 of that session's
manifest, and they *replace* a `measurement.md` figure this block goes on to show was false."

The exception sentence excepts the table from *where* figures are recorded (Gate 3, not
`measurement.md`), not from *with the command*. The table is at `01:1535-1544` (header `:1535`, rows
`:1537-1544`); the closing-line range `+15` to `+48` is at `01:1529`. The text above the table,
`01:1532-1533`, gives a method and no command: "Each row was produced by resolving the cited range
against both commits, and re-measured independently."

Source of the hand-over, the 6b-xiv session's manifest criterion C15
(`.dir-sdlc/sessions/2026-09-29-dungeon-facade-lookup-extraction/manifest.json:82`): "The eight rows,
measured by the orchestrator at Gate 3 by resolving each cited range against both commits and
re-measured independently by T3's reviewer, are: ..." -- a method, no command. The row's claim holds.

## M3 -- row :96, `CORE-035`'s two twelves (VERDICT: TRUE; one fact added)

`CORE-035` heading `01:3045`; next entry `CORE-036` at `01:3393`. `grep -c twelve 01` prints 12
(case-sensitive, the discuss-phase figure); `grep -c -i twelve 01` prints 16. Unit: matching lines.

The two figures, both inside `CORE-035`:

- **(a) the post-change census, `12 of 17`** -- `01:3080`, "After this session's two class A changes
  the same census returns `12 of 17`"; restated `:3086` ("should expect 12, not 14") and re-measured by
  the `AMENDED 20261105-1907-core-heavy-rows` block at `01:3244-3245` (`TOTAL 17 NONE_AT_ZERO 5
  CONCRETE_AT_ZERO 12 NO_ZERO 0`) and `:3257` ("The 12 of 17 agrees ...").
- **(b) the table census, `14 - 2 = 12`** -- `01:3070-3071`, "the twelve this session does not change
  -- the fourteen in the table below less its two class A rows, 14 - 2 = 12 (table census, defined under
  that table)"; restated `01:3199-3200`.

The source DIR backlog item (`.dir-sdlc/backlog.md:377`) says both are "derived and labelled ... so
nothing is wrong -- but a numerical collision ... is a conflation risk". It cites `:1439` and `:1430`,
which have drifted to `:3080` and `:3071`. `git log -S'table census' -- 01` returns only `476b510`
(the authoring commit), so the `(table census)` label predates the row and the row asks for more than
that label.

**Added fact: the two twelves name the same twelve enums.** The triage table (`01:3105-3120`, 14 rows)
less its two class A rows (`MutationOutcome` `:3107`, `DrawSessionResult` `:3108`) leaves
`RotationDirection`, `ScaleDirection`, `PathSegmentKind`, `SnapAnchor`, `MapTool`, `DrawState`,
`NotebookKind`, `NotebookPane`, `CanvasCursorShape`, `BehaviorCursorShape`, `FloorListBandKind`,
`ErrorCategory`. The `AMENDED 20261105-1907` dispositions table (`01:3267-3278`, rows numbered 1-12)
lists exactly those twelve names, and `01:3248` states "the twelve with a concrete member at zero are the
twelve rows of the table below". Commands:

```
awk 'NR>=3095 && NR<=3130' docs/audit/01-domain-application.md | grep '^|'        # triage table
awk 'NR>=3264 && NR<=3310 && /^\| [0-9]/' docs/audit/01-domain-application.md    # dispositions, 12 rows
```

So the figures are of two different instruments (a Python census over a tree; arithmetic over table
rows) that agree on one set. The `AMENDED 20261105-1907` block's "the twelve enums this entry leaves
with a concrete member at zero" (`01:3214`) and "No enum of the twelve" (`:3223`) use both readings at
once, which is correct only because the sets coincide -- the parenthetical should say so.

## Insertion-point context

Prior audit-correction sessions inserted dated, attributed blocks headed
`**[AMENDED <session-id>, <date> -- ...` (e.g. `01:1733`, `01:3311`) and left earlier text in place.
