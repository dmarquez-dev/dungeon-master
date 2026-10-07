# Brief -- start here for 6b-xvii

**One page, and this time it is the only document.** 6b-xvi ran on the **Small** track -- one implementer,
one reviewer, one commit, no spec, no manifest, no worktree -- so it produced no session directory and
there is no 6b-xvii handoff to pair with this. `HANDOFF-session-6b-xvi.md` beside this file is still the
reference for **inherited discipline, instrument traps and structural limits** (its sections 4 and 7); its
section 3 alternatives list is now partly spent and its state figures are superseded by the table below.

**Written 2026-10-07. Every figure was derived on that date by the orchestrator of 6b-xvi**, with the
command beside it, from the register blob at `3c22647` rather than from the working tree -- the previous
session was editing `00-master-index.md` while the first survey ran, so the count would have shifted
underneath it. Two figures are **attributed, not re-derived here**, and are labelled where they appear.

---

## Where the arc stands

| | | Command |
|---|---|---|
| Branch | `chore/repo-audit` at **`3c22647`**, **0 ahead / 0 behind** origin | `git rev-parse --short HEAD`, then `origin/chore/repo-audit`, then `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit` |
| `main` | **`45495c1`**, local and remote identical, untouched by the whole 6b arc | `git rev-parse --short main`, then `origin/main` **separately** -- the two-revision form fails |
| Working tree | clean | `git status --porcelain` |
| `C:/dm-wt` | **empty**, ready for a worktree | `ls -A /c/dm-wt | wc -l` |
| Session dirs | **24** | `ls .dir-sdlc/sessions | wc -l` |
| Register | **179 rows -- 133 closed, 42 open, 4 part done**, zero `UNCLASSIFIED`; 179 distinct ids | the survey below; its class tally sums to 179, and `grep -oE 'MASTER-[0-9]+' | sort -u | wc -l` also returns 179 |
| Open severity | **S1 0, S2 0, S3 0, S4 32, S5 10** | same survey, column 4 |
| Open effort | **XS 24, S 13, M 3, L 2** | same survey, column 7 |
| Open layers | **CORE 12, UIKIT 9, UI 8, TEST 6, INFRA 6, `UI, XCUT` 1** | same survey, column 5 |
| `.dir-sdlc/backlog.md` | **453** newlines, **8** lone CR, in the working tree **and** in the blob at `3c22647` | byte counts of `\n` and of `\r` minus `\r\n`; the second over `git show 3c22647:...` |
| Backlog items | **149** dated line items across 20 headings; liveness **unaudited** | 149 bullet-leading lines and 149 date-leading lines, two patterns agreeing |
| Suite | **1021 passed** -- `DungeonMaster.Tests` **890**, `App.UnitTests` **23**, `App.Tests` **108** -- 0 failed, 0 skipped | **ATTRIBUTED**: measured twice in 6b-xvi, independently, by its implementer and its reviewer, agreeing in every digit. Not re-run for this brief. `dotnet test src/DungeonMaster.sln` |
| Build | 0 Warning(s), 0 Error(s), all three App TFMs | **ATTRIBUTED** to the same two runs |

**The severity criterion is spent.** `MASTER-051` was the last open S3 and 6b-xvi closed it, so severity
can choose no more: what remains is 32 S4 and 10 S5, measured above. (`BRIEF-session-6b-xvi.md` says
severity chose nine of the previous ten sessions. **That figure is attributed, not re-derived** -- it is
not recomputed here, and nothing in this brief depends on it.)

**But "no open S3" does not mean S3 is finished, and the state table is the wrong place to look for the
heaviest work left.** The **4 part-done rows are all S3** -- `MASTER-081`, `MASTER-094`, `MASTER-098`,
`MASTER-108`, three at effort `M` and one at `L`. That is more residual effort than the entire open S4/S5
tail, parked in rows no severity query counts as open. 6b-xv already falsified both premises of
`MASTER-081`, so re-measure before assuming any of the four still describes live work.

---

## Do this first, because it is free

**`MASTER-111` and `MASTER-171` are the same defect filed twice.** Byte-identical titles, both `S4`, both
`CORE`, both `naming`, both `XS`, both open:

> `RotateDirection.cs declares a type named RotationDirection`

`MASTER-111` came from `CORE-023`; `MASTER-171` was filed by **Session 6b-iii** from its own findings,
apparently without checking whether the register already held the row. **The defect is real and still
live** -- `src/DungeonMaster.Domain/Entities/Floors/RotateDirection.cs:3` declares
`public enum RotationDirection { Clockwise, CounterClockwise }`.

So **there are 42 open rows but 41 distinct open defects.** One fix closes two rows, and the second should
close as a duplicate naming the first rather than as independently done.

**This is the only exact-title duplicate among the 42** -- checked by grouping every open row by title and
reporting each group larger than one; `MASTER-111`/`MASTER-171` was the only group returned. **That bound
is on exact titles only.** Six clusters share layer, category and effort without sharing a title, and any
of them could hide a second overlap: `INFRA/correctness/S` (`152`, `156`), `INFRA/naming/XS` (`116`,
`117`), `TEST/style/S` (`137`, `139`), `UI/style/S` (`134`, `135`, `141`), `UIKIT/style/XS` (`121`, `122`,
`124`), plus the pair above.

---

## The recommendation: select by LAYER CONCENTRATION, and batch

**The cadence collapsed and the work did not get harder.** Rows closed per session: 6b-i **11**, 6b-ii
**10**, 6b-iii **5**, 6b-iv **5** -- then 6b-v through 6b-xiv closed **14 between all ten of them**, and
6b-xv closed **none** fully. Recent cadence is about **1.4 rows per session**. At that rate the remaining
42 rows plus four residuals is **thirty-plus more sessions**; at the early-6b rate it is a handful.

The difference is not difficulty. **Thirty-seven of the 42 open rows are XS or S.** The early sessions
batched one layer's related rows into a single Standard session with parallel tasks; the recent ones took
one row each **because severity was the selection criterion and the severe rows were isolated**. That
constraint is now gone.

**Primary: CORE.** Twelve open rows -- **XS 8, S 2, M 1, L 1** -- so ten at XS/S, the largest small-row
block in the register. It also holds both cheap wins: the `111`/`171` duplicate above, and
**`MASTER-157`**, `S5`/`L`/`risk=high`, whose title already reads as a judgment -- *"The hand-rolled
dispatcher is 233 lines replacing a mediator library **and earns its keep**"*. **That is a hypothesis, not
a measurement**: it is phrased as though it may close with no code change, and nobody has re-measured it.
Treat it as a candidate for an evidence-only close, not as one.

The ten small CORE rows cluster into three plausible tasks: **naming** (`111`, `171`, `130`), **docs**
(`112`, `131`), and **structure / style / performance** (`110`, `113`, `114`, `145`, `146`). Exclude `157`
and **`MASTER-178`** (`M`, `risk=medium`, the enum-zero row, which is substantive and partly worked
already -- see `.dir-sdlc/sessions/2026-09-17-enum-zero-values/`).

**Strong alternative: UIKIT.** Nine open rows -- **XS 7, S 1, L 1** -- the cleanest ratio in the register,
eight of nine small, and a coherent layer with precedent (session 5c, "The control kit"). Its
`UIKIT/style/XS` trio (`121`, `122`, `124`) is one formatting task on its own. Exclude **`MASTER-158`**
(`L`, `risk=high`, no bindable surface on two controls).

**The tradeoff, stated rather than resolved.** CORE closes more rows and holds both cheap wins, but it is
the domain layer and changes there ripple into an 890-test assembly. UIKIT has a smaller blast radius per
row but touches XAML, which is where this repository's build is most fragile. Neither dominates; pick on
appetite for suite churn.

**Re-measure a sample before committing to any batch.** `XS` and `S` are the audit's own estimates, never
measurements, and **every entry re-measured in this stream has had at least one false or stale specific**.
6b-xvi is one more instance of it: `UI-003`'s substance was right, one of its four checkable claims was
false, one had to be recorded as unmeasured, and **both of its `locations:` citations were stale**. The
universal in the previous sentence is quoted from `BRIEF-session-6b-xvi.md:51` and is **not re-counted
here** -- no session has enumerated the entries behind it, so treat it as a standing expectation rather
than a figure.

---

## The three things that will bite on day one

1. **Use a short worktree root -- `C:/dm-wt/6b-xvii`, never the scratchpad path.** This is a MAUI/WinUI
   solution and a long path breaks the WinUI XAML compiler with `MAX_PATH` errors. `C:/dm-wt` is empty and
   exists for exactly this. 6b-xvi avoided the trap only because the Small track creates no worktree.
2. **`sed -i` is suspended, and the harness WILL re-issue its `sed`/heredoc preference mid-run to every
   agent, reviewers included.** It rewrites whole-file CRLF to LF and fails the build. **Restate the
   suspension in every dispatch.** **ATTRIBUTED:** 21 consecutive agents refused it across 6b-xiv and
   6b-xv. In 6b-xvi the harness re-issued it to the implementer in both rounds and it refused both times,
   reporting each refusal. Those are the refusals this session can vouch for; the running total is not
   re-derived here.
3. **If you append to `.dir-sdlc/backlog.md`, append CRLF and assert the lone-CR count before and after.**
   It is the **one** tracked file in this repository stored CRLF -- confirmed again at `3c22647` by reading
   the blob rather than the working tree. It carries **8** lone CR characters from 6b-iv that eleven
   sessions have preserved.

Two that cost time rather than breaking things: **the solution is at `src/DungeonMaster.sln`**, not the
repository root (`MSB1009` otherwise); and **`dotnet test`'s `Passed!` line cannot tell you which TFM
ran** -- it prints `(net10.0)` for `DungeonMaster.App.Tests` while that assembly runs from
`net10.0-windows10.0.19041.0`. Read the `Test run for ...` line. **ATTRIBUTED:** 6b-xvi's orchestrator
recorded this as a ninth reproduction before that session began; inside it the trap then appeared **twice
more**, once for the implementer and once for the reviewer, each of which read the identity from the
`Test run for ...` line and neither of which was fooled. No cumulative total is asserted, because no
session has counted one.

One measurement trap worth adding to the standing list: **`grep -icE "warning|error"` returns 2 for a
perfectly clean `dotnet build`**, because the summary lines contain those words. Read `N Warning(s)` and
`N Error(s)` instead. For `dotnet test` the same grep returns 0 when clean. The two commands need
different checks, and conflating them reports a clean build as dirty.

---

## The habit worth carrying from 6b-xvi

**Cite the line and what is on it. Never a count you got by subtracting line numbers.**

6b-xvi produced **three** off-by-one errors in prose and every one came from arithmetic: the orientation
brief's own (`:171-199` where the entry ends at `:198`, caught by printing `:199` and finding it blank);
the implementer's, inside the correction block, which was the session's one **blocking** finding -- "stopping
two assignments into the body", where three fall inside the range and the range ends *on* the third; and
the orchestrator's, repeating that same claim to the user while presenting the finding.

The remedy adopted was to replace the derived count with a named anchor -- not *"two assignments in"* but
*"its last line, `:84`, is the `TextControlBorderBrush` assignment"*. An anchor cannot be miscounted, and
when the file drifts it goes visibly stale instead of staying plausible.

**Caveat found the same session:** two anchors can be textually identical. `:86` and `:113` were both
`TextControlBorderBrushFocused`, one per copy of the duplicated block -- so an anchor sometimes needs its
provenance stated, and a content check cannot verify provenance. Derive which block a line falls in from
the enclosing `#if` / `#elif` boundaries, never from the report claiming it.

**And the cheap signal for a stale citation:** if a cited span's **length** is right while its position is
wrong by a **constant**, that is drift, not mis-identification. `UI-003` cited `:59-84` and `:86-113` --
26 and 28 lines, exactly the lengths of the true `:75-100` and `:102-129`, both shifted `+16`. Compute the
length and the offset before concluding an entry pointed at something else entirely.

---

## Two mechanisms 6b-xvi established that are worth reusing

**`dir-git`'s paraphrase hole is closed, and `git commit -F` is what closes it.** Five consecutive sessions
put a false figure into a permanent commit body while every repository write was correct. 6b-xvi wrote the
full message to a file in that agent's scratch directory and dispatched `git commit -F <path>`, explicitly
forbidding rewording, reflowing, trailers, and "recompute a figure then adjust the text" -- with the
instruction that a figure it believes wrong means **commit nothing and report**. The committed body came
back **byte-identical**: `diff` clean, equal sha256, no trailer added. First provably unaltered body in six
sessions. Still verify both ways -- have it re-derive the staged set from the index
(`git diff --cached --name-status` and `--numstat`), and afterwards diff `git log --format=%B -n 1 HEAD`
against the authored file. Transcription has no failure mode that summarising lacks, and it removes the one
summarising has.

**An injection falsified the orchestrator's own dispatch premise, and the control is why the result was
usable.** The 6b-xvi dispatch asserted that a private method left visible on the non-Windows TFMs would be
a build *error* under `TreatWarningsAsErrors` plus `AnalysisMode=Recommended`. An added, genuinely uncalled
probe method built at **0 warnings, 0 errors**: IDE0051 is not enforced here. The real mechanism was then
found with a positive control -- `#if WINDOWS` replaced by `#if true` gives `Build FAILED` with two
`CS0234` errors (`Microsoft.UI` unresolvable), one per non-Windows TFM. **The guard is load-bearing on type
resolution, not on dead-code analysis.** Write the injection *and* the control that must fail: the
injection alone would have concluded the guard was decorative.

---

## Where to look for the rest

| You need | Go to |
|---|---|
| Inherited discipline, instrument traps, structural limits | `HANDOFF-session-6b-xvi.md` **sections 4 and 7** |
| Worktree, TFM, model tiers, Gate-3 shape | same, **section 7** |
| Carried-forward items and everything unscheduled | same, **section 5**, then `.dir-sdlc/backlog.md` (149 items, liveness unaudited) |
| What 6b-xvi actually did | `3c22647` -- its body carries every figure with the command that produced it |
| The `AMENDED 6b-xvi` precedent for recording a corrected citation | `docs/audit/03-app-shell-and-views.md`, `UI-003` at `:171-266` |
| The register survey behind this brief's figures | an `awk -F'|'` over rows matching `^\| [0-9]+ \| MASTER-[0-9]+ \|`, classifying column 11 and grouping columns 4, 5 and 7 |

**Section 3 of the 6b-xvi handoff ranked five alternatives, and two have changed.** The `git notes`
convention is superseded by the `commit -F` mechanism above, which solves the same problem more cheaply;
and the hashless-closure question was settled at 6b-xvi's Gate 3 by user decision -- row 51 closes with no
hash and states why, and the `MASTER-047` route of the orchestrator amending the row after the merge was
deliberately not taken. **`B11`'s missing mechanical guard, the UI latency spike** (still the only item in
the register or backlog that came from running the application) **and the `PathSegmentKind` migration**
(still the only item requiring a data migration on persisted rows) are unchanged and unscheduled.
