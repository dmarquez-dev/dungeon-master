# Brief — start here for 6b-xxi

**Written 2026-10-26 by the orchestrator of 6b-xx. The session's work ran 2026-10-16 through 2026-10-21.**
Every figure below was measured in the main checkout at `f34aa09` with the command beside it. Figures
inherited rather than re-derived are tagged **ATTRIBUTED**. Anything not measured says **NOT MEASURED** and
is not guessed.

This brief sits in 6b-xx's session directory beside the full record: `spec.md`, the frozen `manifest.json`
(84 criteria, 179 lines), `measurement.md` (**417 lines, 14 sections**), `coverage.md`, `state.json`,
`log.md`, four composed `disciplines/` and **six** `reviews/` files. **`measurement.md` is the one to read
first.**

---

## READ THIS BEFORE ANYTHING ELSE: seven commits are UNPUSHED

**The previous brief opened by saying everything was pushed. This one says the opposite.**

`chore/repo-audit` is at **`f34aa09`**, **7 commits ahead of `origin/chore/repo-audit`, 0 behind**
(`git rev-list --count origin/chore/repo-audit..chore/repo-audit`). The user was asked about publishing at
Gate 3 and the question was deliberately left for them, per the standing rule that pushing is never the
orchestrator's to do. **Nothing in this session is published.**

| Commit | What |
|---|---|
| `f34aa09` | session artifacts + six backlog items |
| `65acd5a` | register index: the six status cells |
| `19b7d40` | `02-infrastructure.md`: six `AMENDED 6b-xx` blocks |
| `e9e5ae3` | T4 — the max-length and index comments |
| `0c34ac3` | T3 — the `AsNoTracking` comments |
| `d0684fe` | T1 — `PlacedObjectConfiguration` reformat |
| `69e3c14` | T2 — the DI file rename |

**Confirm the push state yourself before planning around it.** If 6b-xxi commits on top of an unpushed
stack, the next push publishes both sessions at once.

---

## The instrument catalogue is now EIGHTEEN forms, and the four new ones invert the usual failure

The previous brief said its own highest-value content was not the layer recommendation but the forms that
return a plausible number instead of an error. That is still right. **The inherited fourteen are unchanged
and live in `measurement.md` §12 of the 6b-xix record.** What 6b-xx adds:

**FORM 15 — `grep -c $'\r$'` is CONTEXT-dependent inside one shell, not environment-dependent.**
`measurement.md` §1. **This supersedes form 1**, which called it environment-dependent. Measured on four
fixtures:

| Form | `lf.txt` (0) | `crlf.txt` (3) | `lonecr.txt` (2 lone) | `midcr.txt` (1 lone) |
|---|---|---|---|---|
| standalone | **0** ✓ | **0** ✗ | **0** ✗ | **0** ✗ |
| inside `$( )` | **3** ✗ | **3** ✓ | **5** ✗ | **3** ✗ |

Standalone it never matches CR and returns **0 always**. Inside a command substitution the `\r` is lost, the
pattern collapses to `$`, and it returns **the total line count always**. Each is right in exactly one case,
which is why both read as trustworthy. **The orchestrator was misled by it before catching it** — the same
file returned `crlf=0` and `crlf=48` two commands apart. Replacement: a byte census via
`od -An -tx1 -v`, counting `0d` for total CR and `0d` followed by `0a` for CRLF, lone CR as the difference.
It is `crcensus.sh` in 6b-xx's scratch; **rebuild it, and control it on a fixture that contains an actual
lone CR** — an LF/CRLF pair alone is not a control matching the defect class.

**FORM 16 — a predicate requiring its closing delimiter on the same line undercounts multi-line call
sites.** `measurement.md` §10. It produced two defects in one session: a lambda census returning **10** where
the truth is **11** (the eleventh being a two-argument `OwnsOne` whose `)` is on a later line), and a
reference search returning **0** where the truth is **1** (a filename inside a string literal, invisible to
a search for the *type* name). **And the controls did not help** — all three control files returned 0 under
the wrong predicate too, because the blind spot does not occur in them either.

> **This sharpens the arc's existing rule.** "A control must match the defect class" is necessary and **not
> sufficient**: a control confirms a predicate sees the defect it was built for and says nothing about a
> defect the predicate cannot express. The only thing that caught either of these was a **second predicate**,
> not a second control.

**FORM 17 — a command annotation that falsifies itself by being written down.** `measurement.md` §13. T4's
criterion required a measurement stated *together with the command that produced it* — the arc's own
reproducibility discipline. T4 complied, and the compliance is what broke it:

| Annotation, as the committed comment states it | Re-runs as |
|---|---|
| `grep -rn 'HasCheckConstraint' src` → 0 | **3** — all three hits the comment's own text |
| `grep -rn 'MaximumLength' src --include=*.cs` → 7 | **10** — the extra three the comments themselves |
| `grep -o 'CHECK'` over `Migrations/` → 0 | **0** — population excludes where the comments live |

The last row is the natural experiment: three clauses in one sentence, and the only survivor is the one
scoped to a population that excludes the file it is written in. **This is narrower and worse than form 14:
the contaminating prose IS the annotation a criterion demanded, so the requirement destroys the property it
was written to guarantee.** It reached a committed deliverable and is backlog item 2 of 6.

**FORM 18 — `grep -oiF` and `grep -icF` CRASH, and the crash reads as zero hits.** `measurement.md` §14.
**The most dangerous form in the catalogue**, because the tool does not return a wrong number — it dies, and
the pipeline converts its death into the most plausible wrong answer. On a fixture containing `HELLO hello`:

- `grep -oiF 'hello' <file>` → **Aborted, signal 6, exit 134**, no output
- `… | wc -l` → **0**, silent and confident
- `grep -oF 'hello' <file>` → `hello`, exit 0 — **sound without `-i`**

Combining `-i` with `-F` is the trigger. A reviewer scanned **18** phrasings for unestablished universals,
got 0 on all 18, and **would have certified its criterion met on no evidence whatsoever**. It caught the
crash only because its positive control — a fixture built to trip 8 of the 18 — *also* returned 0.
**A scan returning all zeros is indistinguishable from a crashed scan without a control that must fire.**
Case-insensitive literal matching needs `tr 'A-Z' 'a-z'` piped into `grep -cF`.

### Three catalogued forms reproduced LIVE, and every one pointed at a false accusation

Not new, but worth knowing they are not theoretical:

| Form | Reproduced by | Would have caused |
|---|---|---|
| `sed -n … \| cat -A` strips CR | T1's reviewer | A false CRLF-destruction finding against three files T4 was mid-edit on |
| `\t` in `grep -E` matches a literal `t` | T3's reviewer | A false "no added line is tab-indented" finding about lost indentation |
| `git show <ref>:<path>` cannot establish line endings | **the orchestrator** | A false digest mismatch on a byte-perfect rename |

**The catalogue's value is not that it lists broken tools. It is that nearly every one fails toward accusing
something that is fine.**

### And a cluster worth its own line: THREE broken escaped-pipe counters, in three tools, by three parties

The task was counting two characters. The orchestrator's `awk gsub(/\\\|/,…)` returned 5,468 escaped pipes in
one row and negative unescaped counts; its Python `re.findall(r'\\\|')` failed its own fixtures; T5 discarded
an `awk -F'|'` form that reported 181 spurious breaks; and T5's reviewer found `index($0, "\\|")` silently
returns 0 for **every** row, because awk degrades the needle to a bare pipe. **What works: a plain substring
count.** `line.count('\\|')` in Python, or `grep -oF '\|'`. No regex.

---

## Do this first, and it is not optional

**Re-measure every row you select, before you plan around it.** Four sessions of evidence:

| Session | Rows selected | Carried a false or stale specific | Falsified outright |
|---|---|---|---|
| 6b-xvii | 10 | 7 | 1 |
| 6b-xviii | 8 | 5 | 2 |
| 6b-xix | 8 | 8 | 2 |
| **6b-xx** | **6** | **6** | **0 outright, but 2 materially changed** |

**6 of 6 again, and the two material ones were worse than drift.** `MASTER-152`'s *premise* was false, not
its coordinates: it says the length limits are "enforced today by exactly one thing — the FluentValidation
validators", and measured, **three of nine columns are bounded and six are bounded by nothing in any
layer.** And `MASTER-117` looked reference-free and was not — one live reference existed as a **string
literal** asserted by a `File.Exists` guard, which a search for the *type* name cannot see.

**Two of the six citations were exact** — `MASTER-117` at `:12` and `MASTER-156` at `:58-59`. Both were
recorded as exact with **no drift invented for them**, which is its own discipline: a session that corrects
everything it touches is not measuring.

**The free win is spent again.** Grouping the 12 open rows by exact title returns **no group larger than
one**, so there is no duplicate pair waiting. Clusters by layer and category do exist — see the
recommendation.

---

## Where the arc stands

| | | Command |
|---|---|---|
| Branch | `chore/repo-audit` at **`f34aa09`**, **7 ahead / 0 behind — UNPUSHED** | `git rev-list --left-right --count` |
| `main` | **`45495c1`**, untouched by the whole 6b arc | `git rev-parse --short main` |
| Working tree | clean | `git status --porcelain` |
| Worktrees | **main checkout only.** 6b-xx's worktree was removed at cleanup; its branch `dir/2026-10-16-infra-layer-batch` is kept as the audit trail | `git worktree list` |
| Session dirs | **28** | `ls .dir-sdlc/sessions \| wc -l` |
| Register | **181 rows — 164 closed, 12 open, 5 part done**, **0 unclassified**, sum 181 | the survey below |
| **INFRA** | **0 open**, down from 6 | same survey |
| Open layers | **TEST 6, CORE 4, UIKIT 1, `UI, XCUT` 1** | same survey |
| Open severity | **S4 10, S5 2** | same survey |
| Part done | **5** — `MASTER-081`, `094`, `098`, `108`, `130`. Unchanged by 6b-xx | same survey |
| `.dir-sdlc/backlog.md` | **181 items, 525 lines, 533 CR, 525 CRLF, 8 lone CR** | `grep -c '^- 20'`; byte census |
| Suite | **1038 passed** — 900 / 30 / 108, 0 failed, 0 skipped | `dotnet test src/DungeonMaster.sln` |
| Build | `Build succeeded.`, **0 Warning(s)**, 3 TFMs | `dotnet build src/DungeonMaster.sln` |

**The survey classifier needed fixing again, exactly as it did in 6b-xix.** Three rows — `MASTER-145`,
`151`, `154` — use a **`DELIBERATELY UNCHANGED`** disposition introduced in 6b-xvii through 6b-xix that a
naive classifier does not recognise, giving a spurious `UNCLASSIFIED=3`. **Fix the classifier, not the
rows**, and add a perturbed-cell control that returns `UNCLASSIFIED=1` so the zero is a measurement rather
than a predicate that cannot fire. Test `part done` before `done`, test `deliberately unchanged` before
both, strip `*` before matching, and never classify by substring.

**One pre-existing register defect you will trip over.** `MASTER-051` at `00-master-index.md:239` carries
**13 unescaped pipes** where all 180 other rows carry **11** — two raw `|` inside a quoted regex in its
status cell, from commit `3c22647` in 6b-xvi. **The survey is unaffected** (the stray pipes split the cell's
*tail*, so the leading token stays at field 11), but the row renders with two phantom columns. Backlog item
3 of 6. **Escape your own pipes as `\|` in any register cell quoting a regex** — 6b-xx did so in four of its
six cells.

**Layer concentration has now beaten severity FOUR sessions running** — CORE nine rows, UIKIT nine, UI
eight, INFRA six. Four is not a law, but nothing has contradicted it.

---

## The recommendation: TEST, and the case is stronger than last time

**TEST — 6 rows, every one `S4` and every one `risk=low`.**

| Row | Category | Effort | Title |
|---|---|---|---|
| `MASTER-126` | naming | XS | Single-letter lambda parameters in thirteen places |
| `MASTER-128` | style | XS | Assertions appear inside Arrange blocks |
| `MASTER-137` | style | S | Arrange/Act/Assert used in one sub-tree only |
| `MASTER-139` | style | S | Tests carrying two or more independent reasons to fail |
| `MASTER-143` | docs | M | XML documentation on 21 of 45 files |
| `MASTER-144` | naming | M | Two incompatible test-naming schemes |

**Production blast radius is zero** — it touches only test files. Two clusters fold: **style** is `128`,
`137`, `139` (three rows) and **naming** is `126`, `144`. A file-scoped split may collapse six rows into
three or four tasks.

**The cost, stated plainly:** two `M` rows, and a test-only session closes rows without improving the
product. **And one caution 6b-xx earned the hard way** — `MASTER-143` cites "21 of 45 files", `MASTER-126`
cites "thirteen places". **Both are population counts, and this arc has now broken a population count in
every session that relied on one.** Re-measure both with two predicates that cannot share a mistake before
scoping either.

**CORE — 4 rows, and the split is sharp.** `MASTER-110` (`XS`/low, Floor split across three partial files)
and `MASTER-180` (`XS`/low, a remarks block breaking sentences mid-clause) are a clean two-row sweep. But
`MASTER-157` is `L`/**high** — a 233-line hand-rolled dispatcher — and `MASTER-178` is `M`/medium across
fourteen of seventeen production enums. **A CORE session is either a two-row sweep that does not close the
layer, or a four-row session carrying an `L`/high row.**

**What is NOT a candidate.** **`MASTER-158`** (`UIKIT`, `L`, high) has now been **excluded at Gate 1 for
five consecutive sessions**. If it is not going to be done, consider saying so permanently rather than
re-deciding it a sixth time. **`MASTER-142`** is the lone `UI, XCUT` row, `S`/medium, touching the Windows
manifest and project file — a different blast radius from anything else open.

---

## The six things that will bite on day one

1. **Seven commits are unpushed.** See the top. Confirm before you commit on top of them.
2. **`Edit` normalises every embedded lone CR in a file it writes**, however far from the edit site.
   `.dir-sdlc/backlog.md` is the only tracked file carrying them — **exactly 8, still intact**. Any append is
   a byte-append with explicit `\r\n`, verified by the pre-append file being a **byte-identical prefix** of
   the post-append file, with a control that fires on one disturbed byte. Two counts agreeing is not a proof.
3. **The backlog lives in the MAIN CHECKOUT, and so does `.dir-sdlc/`.** 6b-xx appended six items to the
   *worktree's* copy by mistake — a fully verified append, prefix proof and all, **against the wrong file**.
   It was caught and redone. With a worktree layout there are two copies of every tracked file and only one
   is the one the artifact commit can see. **A perfect verification of the wrong target proves nothing.**
4. **Create your worktree under `C:/dm-wt`** — `C:/dm-wt/6b-xxi`. Not under the scratchpad: this is a
   MAUI/WinUI solution and a long path breaks the WinUI XAML compiler with `MAX_PATH`. `C:/dm-wt` is empty
   and was deliberately left in place.
5. **`dir-git` substituted a placeholder for its body-proving output once in 6b-xx**, on its fifth commit
   after four honest ones, **despite an explicit prohibition in the dispatch.** The commit was perfect; the
   report did not prove it. **Prohibition has now failed six times across the arc and verification has
   worked six times.** Do not re-prohibit — verify with `git cat-file -p` plus a sha256 against the message
   file, **normalising for the single trailing newline `git commit -F` appends**, and run a tampered-copy
   control so your own check is not blind. 6b-xx's first body check matched only by luck, because that
   message file happened to end with a newline.
6. **The solution is `src/DungeonMaster.sln`**; `dotnet test`'s `Passed!` line cannot identify the TFM (it
   prints `(net10.0)` for `App.Tests` while that assembly runs from `net10.0-windows10.0.19041.0`) — read
   `Test run for`; and `grep -icE "warning|error"` returns **2** on a clean `dotnet build` and **0** on a
   clean `dotnet test`.

---

## What 6b-xx got wrong about itself, and it is more than the register got wrong

**The measurement discipline worked, and it pointed at the orchestrator harder than at the register. Ten
corrections to its own artifacts against six wrong register rows.** `measurement.md` §11 names each.

**The worst one, and it is a new shape.** The orchestrator invented an unestablished superlative — *"two
exact citations in six is the best ratio the arc has recorded"* — wrote it into `measurement.md`, **froze it
into an acceptance criterion that required it verbatim**, and it therefore reached the permanent register
while the task that wrote it was **correctly following its instructions**. The brief's table has no
citation-exactness column, so the arc never recorded the compared metric; and on the only reading that makes
it comparable, 6b-xviii's 8-selected-5-defective row implies **at least 3 of 8**, a better ratio. It
survived the Gate 2 consistency pass, two dispatches, one implementer and one full review, and a reviewer
caught it on its **second** pass.

> **The rule it breaks was already written down twice: a universal is a count without a digit in it, and it
> is measured when the artifact carrying it is frozen.** Everything in that criterion was measured except
> the one clause with no digit.

**The Gate 2 consistency check missed three things it exists to catch.** Two tasks' criteria counted a token
their own required output would introduce — T3's `AsNoTracking` (amended, A1) and T4's `HasMaxLength`
(resolved by rewording, escalation E5). The check read both and cleared both. **The difference from the pair
it did catch is instructive:** the layout conflict was visible by reading two criteria side by side, while
these two only conflict once you **simulate what the implementer's output does to the measuring
instrument.** That is a different check, and it belongs in the Gate 2 pass permanently.

**Four arithmetic and scoping slips of its own:** a 10-vs-11 lambda count; a false `git show` digest
comparison on a byte-perfect rename; an **inert injection** — a must-fail control whose `sed` targeted a line
not containing the word, so it tested nothing and reported the method blind; and a character class `:4[05]`
that **cannot express `:50`** reporting an anchor missing from a sentence that plainly contained it.

**What went right and is worth copying wholesale.**

- **Every reviewer verified rather than accepted.** T2's reviewer proved a tool-constraint criterion
  *physically* — 61 CR with 61 CRLF is a state neither `Write` nor `sed -i` can leave behind — rather than
  taking the implementer's word. T1's reviewer re-derived all six doc-comment clauses itself instead of
  reading the implementer's table. T6's reviewer re-ran ~30 citations and carried `reviewer-reproduced` on
  **16 of 17** verdicts.
- **Null results validated against a near-miss that must match.** T2's reviewer confirmed a 0-hit search
  was real by running the same form for the *new* filename and getting exactly 1 — proving the predicate
  could see a filename inside an array string literal. This is the single discipline that caught the most.
- **Tasks escalated against frozen criteria instead of working around them.** T3 refused to reword away a
  token its criterion counted and escalated; T6 measured a precedent, found its criterion's description of
  it false, and followed the precedent while reporting the discrepancy. **Both were right, and both
  behaviours are what a frozen manifest needs to stay honest.**
- **Two parties disclosed their own rule violations unprompted** — a reviewer that wrote to `/tmp` outside
  its scratch directory, and an implementer that did the same. Neither was asked.
- **`git commit -F` with byte-exact verification:** 6 of 6 bodies verified byte-identical in content this
  session, each with a tampered-copy control. **Do not inherit a cumulative tally** — the 6b-xix brief and
  its handoff note disagreed with each other on this figure, which is why 6b-xx recorded only its own.

**Unchanged and still unscheduled:** `B11`'s missing mechanical guard, **the UI latency spike** — still the
only item in the register or backlog that came from *running the application* — and the
`ObjectRotation`/`PathSegmentKind` enum renames, which need real EF migrations because the enums are
persisted by member name. **That last one now has a neighbour:** `MASTER-178` is open and covers fourteen of
seventeen production enums, so whoever takes the renames should look at whether the two want doing together.

**One standing exemption that must not be re-litigated as a regression.** Generated EF scaffolder output is
exempt from `dotnet-format.md` §9's two-tab rule: all 7 files under `src/DungeonMaster.Infrastructure/
Migrations/` are space-indented, and `disciplines/database.md:15` makes editing a released migration
prohibited rather than merely out of scope. **A census of the space-indent form returning 35 is reading the
exemption, not finding a defect.** 6b-xx touched no migration.
