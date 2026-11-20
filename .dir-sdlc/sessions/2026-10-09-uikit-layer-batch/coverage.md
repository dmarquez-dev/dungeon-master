# Coverage — session 6b-xviii

`Item`, `Covering task` and `Gate 2` written at Gate 2. `End state` written at Gate 3.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 `FloorMapDrawable.cs` 0 braceless sites, from 8 | T1 | covered | **met** — T1 C1, detector 0 with a positive control of 21 |
| D2 `HandCursorBehavior.cs` 0 braceless sites, from 2 | T2 | covered | **met** — T2 C1, both sites braced at `:69` and `:158` |
| D3 124 space-form doc lines cleared across 5 files | T1 (116), T4 (4), T5 (1), T6 (3) | covered | **met** — all four tasks, tab form summing 116+2+2+1+3 = 124 |
| D4 `BehaviorCursorShape` in its own file, declared once | T2 | covered | **met** — T2 C3, `grep -rn` returns exactly 1 |
| D5 Three `Notebook*` types in their own files | T3 | covered | **met** — T3 C1/C2, one public declaration per file |
| D6 `HandCursorBehavior.Shape` documented | T2 | covered | **met** — T2 C6, two-tab continuation lines |
| D7 `SetAreaEditing` has both `<param>` tags | T4 | covered | **met** — T4 C1, `name=` matching both parameters |
| D8 Notch clamp is `Math.Min(NotchDepth, bounds.Height * 0.5)` | T7 | covered | **met** — T7 C1, at `:42`, rejected alternative absent |
| D9 Clamp fact shown able to fail, with a passing positive control | T7 | covered | **met** — T7 C5, `injected defect`; only the Height-10 case discriminates, disclosed |
| D10 Opacity parity facts on parsed values, plus a scan-reach control | T8 | covered | **met** — T8 C2/C3/C5, `injected defect`; vacuity reproduced by blinding both sides |
| D11 Both theme dictionaries left byte-identical, `sha256sum`-proven | T8 | covered | **met** — T8 C7, and `git status` shows neither modified |
| D12 `MASTER-121` closed, population and citations corrected | T9 | covered | **met** — T9 C2/C3 |
| D13 `MASTER-122` closed, false claim struck, population widened | T9 | covered | **met** — T9 C4/C5 |
| D14 `MASTER-181` closed as a duplicate, in both files | T9 | covered | **met** — T9 C7 |
| D15 `MASTER-123` closed, `git mv`/`R100` recorded as inapplicable | T9 | covered | **met** — T9 C8 |
| D16 `MASTER-124` closed FALSIFIED, §5 misreading recorded | T9 | covered | **met** — T9 C9, recorded as an original error |
| D17 `MASTER-125` closed FALSIFIED, remedy recorded as destructive | T9 | covered | **met** — T9 C10, 7 call sites partitioned from a raw 13 |
| D18 `MASTER-136` closed, effort `S`→`XS`, six dead locations accounted | T9 | covered | **met** — T9 C11/C12 |
| D19 `MASTER-148` closed, clamp choice and rejection recorded | T9 | covered | **met in substance; THIS ITEM'S OWN WORDING WAS FALSE** — see below |
| D20 `MASTER-151` closed deliberately-unchanged with a guard added | T9 | covered | **met** — T9 C14, comment block re-derived to `:6-29` |
| D21 Every `AMENDED 6b-xviii` block follows the six-element form | T9 | covered | **met** — T9 C15, all nine conform; two vacuously satisfy element six and say so |
| D22 Every touched file still pure CRLF, proven three ways | T1–T9 (all nine) | covered | **met** — every task, three instruments agreeing per file |
| D23 Build clean across all three TFMs | T9; orchestrator also between waves 1 and 2 | covered | **met** — T9 C18, reproduced by the reviewer |
| D24 Suite green, passed total reconciles to 1028 + new facts | T9 | covered | **met** — T9 C19, 1028 + 7 + 3 = 1038, reconciling per project |
| D25 Every register count carries its command or its attribution | T9 | covered | **met** — T9 C17, with one stale incidental figure (58 → 62) as advisory |

**25 items, 25 covered, 0 uncovered, 25 met.**

**D19 is the one exception worth stating in full, and the defect is in this document.** As written at
Gate 2 the item read: *"`MASTER-148` closed, recording the chosen clamp, why `bounds.Height - 1` was
rejected, and that its citation was the only one of the eight that survived unchanged."* **That final
clause is a false universal.** `MASTER-151`'s citation `Controls.Semantic.xaml:35-66` is also exact,
on a file the session left byte-identical. So the item's first two clauses are met and its third was
unmeetable as written, because it asked for a falsehood.

The universal originated in `measurement.md` (corrected there as `CORRECTION 7`), reached the frozen
`manifest.json` through the architect, **and reached this document through the orchestrator's own
definition of done.** T9 measured it, refused to write it, and recorded "one of only two". That is
the correct outcome; the register says the true thing. It reaches Gate 3 as `contradicts` finding X1
with the reviewer's options, because the last uncorrected copy is in the frozen manifest.

## Contract operations

**No rows — `contracts/` is empty.** This is the architect's considered conclusion rather than an
omission: no task consumes an operation a peer implements, so there is no operation that could be
left with an implementer and no caller. The reasoning is in `spec.md`'s `## Contracts`.

The sharpest row this table exists to produce — `not called` — therefore cannot arise this session.

## Decisions still open at the point of writing

These are not uncovered definition-of-done items; they are questions about whether the definition of
done should be **wider**. Recorded here because `coverage.md` is written before the approval that
freezes `manifest.json`, and a session that dies between sign-off and dispatch should find them on
disk.

| # | Question | Outcome |
|---|---|---|
| 1 | After this session no open row covers the EF migration's 35 doc lines | **Standing exemption**, recorded on `MASTER-122`'s entry by T9. Nothing owed, no row left open. |
| 2 | Three layer-wide measurements (37 braceless sites, 14 filename mismatches, `<param>` bound of 14) have no home | **All three to `.dir-sdlc/backlog.md`** at Gate 3, with figures, commands and limits. Orchestrator-owned; no task covers it. |
| 3 | T1's brace-removal injection has no `sha256sum` revert proof, unlike T7's and T8's | **Criterion added to T1** before the freeze. T1 now carries 9 criteria. |
| 4 | T7 and T8 share a wave and both build and test in one worktree | **T8 serialized behind T7** — `dependsOn` now `["T5","T6","T7"]`. Four sequential waves. |

All four were decided before the approval that froze `manifest.json`.
