# Brief -- start here for 6b-xix

**Written 2026-10-12 by the orchestrator of 6b-xviii. Every figure below was measured on that date,
in the main checkout at `0cc4799`, with the command beside it.** Where a figure is inherited rather
than re-derived it is tagged **ATTRIBUTED** and says whose it is. Things this session did not measure
are marked **NOT MEASURED** and are not guessed.

This brief sits in 6b-xviii's own session directory, so the full record is beside it: `spec.md`,
`manifest.json` (frozen, then amended twice at Gate 3), `measurement.md`, `coverage.md`, `state.json`,
`log.md`, and nine `reviews/` files. **`measurement.md` is the one to read first** -- it is the
evidence base every criterion rested on, it carries **eight numbered corrections**, and five of those
are corrections to the orchestrator's own figures.

---

## Where the arc stands

| | | Command |
|---|---|---|
| Branch | `chore/repo-audit` at **`0cc4799`**, **0 ahead / 0 behind** origin | `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit` |
| | **Everything is PUSHED.** 12 commits went to origin on 2026-10-12; `git reflog show origin/chore/repo-audit` records `57b7eba..0cc4799 update by push`. All 12 confirmed published with `git merge-base --is-ancestor <c> origin/chore/repo-audit` | |
| `main` | **`45495c1`**, local and remote identical, untouched by the whole 6b arc | `git rev-parse --short main`, then `origin/main` **separately** -- the two-revision form fails |
| Working tree | clean | `git status --porcelain` |
| **`C:/dm-wt` is NOT empty** | **1 entry, `6b-xviii`**, at `77bbaae` on `dir/2026-10-09-uikit-layer-batch` | `git worktree list` |
| Session dirs | **26** | `ls .dir-sdlc/sessions \| wc -l` |
| Register | **181 rows -- 150 closed, 26 open, 5 part done**, and **0 unclassified** | the survey below |
| Open severity | **S4 20, S5 6** | same survey |
| Open effort | **XS 11, S 10, M 3, L 2** | same survey |
| Open risk | **low 20, medium 3, high 3** | same survey |
| Open layers | **UI 8, INFRA 6, TEST 6, CORE 4, UIKIT 1, `UI, XCUT` 1** | same survey |
| Part done | **5** -- `MASTER-081`, `094`, `098`, `108`, `130`. Unchanged by this session | same survey |
| `.dir-sdlc/backlog.md` | **164** dated line items, up from 158; still CRLF with **8** lone CR | `grep -c '^- 20'`; lone CR as total CR minus CRLF |
| Suite | **1038 passed** -- `DungeonMaster.Tests` **900**, `App.UnitTests` **30**, `App.Tests` **108** -- 0 failed, 0 skipped | `dotnet test src/DungeonMaster.sln`, run by this orchestrator **in the main checkout after the merge** |
| Build | `Build succeeded.`, **0 Warning(s)**, **0 Error(s)**, all three `DungeonMaster.App` TFMs | `dotnet build src/DungeonMaster.sln`, same run |

**The survey command**, so you can re-run it rather than trust the table: one `awk -F'|'` over rows
matching `^\| [0-9]+ \| MASTER-[0-9]+ \|` in `docs/audit/00-master-index.md`, classifying column 11.

**Two things about that classifier, and both cost 6b-xviii time.** First, **test `part done` BEFORE
`done`**, or a part-done row is absorbed into the closed count. Second -- and this is the one the
previous brief did not carry -- **never classify these cells by substring.** The status cells are
multi-sentence prose and three open rows contain the words "done" or "fixed" inside that prose; a
substring classifier returns `closed=143 open=33` where the truth is `141/35`. Anchor on the
**leading token** of the cell, and carry an `unclassified` counter that must read **0** -- that
counter is what makes the totals mean anything, because it proves the classifier read every cell.
The statuses it must recognise as closed now include `FALSIFIED`, `DELIBERATELY UNCHANGED`,
`AS A DUPLICATE`, and the pre-existing `filed found-and-fixed`.

**UIKIT went from 9 open rows to 1.** Only `MASTER-158` remains -- `L`, `risk=high`, "DmConfirmPopup
and NotebookView expose no bindable surface at all" -- excluded at Gate 1 for the third consecutive
session. **This is the second session in a row where layer concentration beat severity as a selection
criterion**: nine rows closed against a recent cadence of about 1.4 per session, matching 6b-xvii's
nine. Two sessions is not a law, but it is now the best-evidenced heuristic the arc has.

---

## Do this first, and it is not optional

**Re-measure every row you select, before you plan around it.** This is the single
highest-value action available and the evidence is now overwhelming:

| Session | Rows selected | Carried a false or stale specific | Falsified outright |
|---|---|---|---|
| 6b-xvii | 10 | **7** | 1 |
| 6b-xviii | 8 | **5** | **2** |

**`MASTER-125`'s recorded remedy was "Delete the line", and following it would have broken the build
and six tests.** Every clause of that entry was false: the constructor was not empty (it chains
`: this(FloorMapPalette.FromTheme)`), it was documented, the class had **three** constructors rather
than none, and it had **7** live call sites. **`MASTER-124` was falsified twice over** -- both cited
instances gone, the defect class absent from both files, *and* the entry misread the convention
section it cited, so the same entry filed today would still be wrong.

**The register's specifics are leads. They are not evidence.** A row's line numbers, its population
counts, and its suggested remedy are each independently capable of being wrong, and the remedy is the
one that can do damage.

**The free win is spent again, and the same caveat applies.** Grouping all 26 open rows by exact
title returns **no group larger than one**, so there is no second `MASTER-111`/`171` or `122`/`181`
pair waiting. **That bound is on exact titles only.** Four clusters still share layer, category and
effort without sharing a title, and the `122`/`181` overlap proved a non-identical duplicate is the
real risk:

| Cluster | Rows |
|---|---|
| `UI/style/S` | `MASTER-134`, `135`, `141` |
| `INFRA/naming/XS` | `MASTER-116`, `117` |
| `INFRA/correctness/S` | `MASTER-152`, `156` |
| `TEST/style/S` | `MASTER-137`, `139` |

---

## The recommendation: UI, with the ratio stated rather than hidden

**Primary: UI.** Eight open rows, the largest open layer, and the layer both previous briefs named as
the strong alternative without ever selecting it.

| Row | Sev | Category | Effort | Risk |
|---|---|---|---|---|
| `MASTER-118` | S4 | structure | XS | low |
| `MASTER-119` | S4 | docs | XS | low |
| `MASTER-120` | S4 | style | XS | low |
| `MASTER-133` | S4 | docs | S | low |
| `MASTER-134` | S4 | style | S | low |
| `MASTER-135` | S4 | style | S | low |
| `MASTER-141` | S4 | style | S | **medium** |
| `MASTER-154` | S5 | dead-code | S | low |

**The tradeoff, not resolved for you.** UI is the largest layer and draining it is the strategy that
has now worked twice -- but its effort ratio is **XS 3 / S 5**, materially worse than the **XS 7 / S 1**
that made UIKIT a nine-row session. One row is `risk=medium`. And **`MASTER-154` is a dead-code row
about an unwired "Start Session" button in two views**, which is the shape that turns out to be a
feature decision rather than a cleanup; both previous briefs flagged it and neither tested the claim.
Consider scoping it out at Gate 1 the way `MASTER-158` has been, or resolving it first.

**Strong alternative: INFRA.** Six rows at **XS 4 / S 2** -- the best effort ratio on the board -- and
the `INFRA/naming/XS` pair (`116`, `117`) is plausibly one task. The cost: `MASTER-156` is
`risk=high` and `correctness`, which is a different class of work from a formatting sweep, and it
clusters with `MASTER-152` so the two want resolving together.

**Honest third: TEST.** Six rows, and **all six are `risk=low`** -- the only layer where that is
true. It touches only test files, so production blast radius is zero. The cost is two `M` rows
(`MASTER-143` docs, `MASTER-144` naming) and that a test-only session closes rows without improving
the product.

**What is NOT a candidate.** UIKIT has 1 row left and it is the one three briefs have advised
excluding. CORE has 4 but holds `MASTER-157` (`L`, `risk=high`) and `MASTER-178` (`M`,
`risk=medium`), so its ratio is the worst available.

---

## The five things that will bite on day one

1. **`C:/dm-wt` is not empty.** 6b-xviii's worktree is still at `C:/dm-wt/6b-xviii`. Use a sibling
   path -- `C:/dm-wt/6b-xix` -- or remove the old one first with `dir-git cleanup` from the **main
   repository root**. Do **not** nest under the scratchpad path: this is a MAUI/WinUI solution and a
   long path breaks the WinUI XAML compiler with `MAX_PATH`.
2. **The branch is 0 ahead / 0 behind, and that is the real state.** 6b-xviii inherited a brief
   claiming 1 ahead and a handoff note claiming 2 ahead with "both unpushed commits are yours to
   send"; both were wrong, the commits had already been pushed, and the reflog is what settled it.
   **Check `git reflog show origin/chore/repo-audit` rather than trusting any prose about ahead-ness,
   this brief included.**
3. **Three write tools are now suspended, not two.** `sed -i` rewrites whole-file CRLF to LF and
   fails the build. **`Write` does the same** -- it emits LF and silently converts an entire file.
   **And new in 6b-xviii: `Edit` strips TRAILING whitespace from `new_string`.** Replacing `///` with
   `///` plus two tabs reports **success** and produces **no indent at all**; leading whitespace
   survives. When trailing whitespace is the only difference the tool answers *"No changes to make:
   old_string and new_string are exactly the same"*. **`Edit` is still the only CRLF-preserving
   writer and remains the right tool** -- it just cannot carry trailing whitespace, so key any
   whitespace-bearing edit on the line's first text character. One task hit this on 116 lines and
   only its tab-form positive control caught it. **Restate all three in every dispatch**: the harness
   re-issued its `sed`/heredoc/`Write` preference to **all nine implementers and several reviewers**,
   and all nine refused and reported it. Nine for nine is not luck; it is the dispatch wording.
4. **Appends before commits, at Gate 3.** Write the `.dir-sdlc/backlog.md` lines and the `log.md`
   entries **first**, then commit. 6b-xviii did this correctly and ended with a clean tree and
   nothing dirty; 6b-xvii got it wrong for its own log tail and left an unpushed commit.
5. **`.dir-sdlc/backlog.md` is the one tracked file whose blob is CRLF**, carrying exactly **8** lone
   CR since 6b-iv. Append explicit `\r\n` bytes with `>>` so no existing byte is rewritten, and
   assert the lone-CR count on both sides. 6b-xviii went 473→487 lines, 481→495 total CR, **8 lone CR
   before and 8 after**. **`tr -dc '\r' | wc -c` counts TOTAL CR, not lone CR** -- the correct form is
   total CR minus CRLF-line count.

Two that cost time rather than breaking things: **the solution is `src/DungeonMaster.sln`**, not the
repository root (`MSB1009` otherwise); and **`dotnet test`'s `Passed!` line cannot tell you which TFM
ran** -- it prints `(net10.0)` for `DungeonMaster.App.Tests` while that assembly runs from
`net10.0-windows10.0.19041.0`. Read the `Test run for ...` line. That trap reproduced throughout
6b-xviii and is now deliberately reproduced in the register as evidence.

---

## The instrument list, and it has grown

**Ten forms that return a plausible number instead of an error.** None fails loudly, which is why **a
zero means nothing until a control has been shown to return non-zero.** Four of these were found in
6b-xviii, and two of the four were in the orchestrator's own evidence base.

| Form | Returns | Why it lies |
|---|---|---|
| `grep -c $'\r$'` | **the line count in two shells, constant 0 in a third** | **ENVIRONMENT-DEPENDENT.** Measured 3/3/3 on CRLF/LF/mixed by the orchestrator and one reviewer; 0/0/0 by two implementers. Useless under either mode -- it discriminates nothing. **Do not assert which mode it shows**; the only portable conclusion is that it must not be used |
| `cat -A f \| grep -c "\^M\$"` **in DOUBLE quotes** | **0** on a known-CRLF file | **QUOTING-dependent, NOT a property of the spelling.** Measured on a 54-line CRLF file: `'\^M\$$'` → 54, `'\^M\$'` → **54**, `"\^M\$"` → **0**, `"\^M\$$"` → 54. In double quotes the shell collapses `\$` to a bare `$` anchor, which can never match the literal `$` that `cat -A` emits after `^M`. **Use the single-quoted `'\^M\$$'`** -- robust under both quotings. Two implementers lost time to this |
| `grep -rln 'a\|b\|c'` **without `-E`** | **0** | The `\|` is a literal, so the pattern matches one impossible string. **This was in `measurement.md` beside a figure of 12 that was correct** -- the figure was right and its command was inert. Corrected as `CORRECTION 8` |
| `\b` inside a shell-quoted regex | **a literal backspace byte** (`^H`, visible under `cat -A`) | Shell escape processing eats it, so a guard clause never matches. **A "fixed" predicate stayed inert while still reporting a number.** Rewrite without backslashes |
| `git show <ref>:<path>` as a line-ending base | **0 CR, always** | Blobs are **LF-normalised** by `core.autocrlf=true` with `* text=auto`. It **cannot** establish line endings, and advising it as a comparison base was an orchestrator error a reviewer corrected. For content it is fine; for CRLF compare against a clean checkout, or use `git status` to prove byte identity |
| `\t` inside `grep -E` | **0** | Matches a literal `t`. With `TAB=$(printf '\t')` the same predicate returned 21 where the broken form returned 0 |
| `sed -n 'N,Mp'` and `grep -E` **with a file argument** | CR absent | Both strip CR, so neither can inspect line endings. **This produced a false "mixed line endings" finding** in 6b-xviii that had to be withdrawn |
| `grep -icE "warning\|error"` on a clean `dotnet build` | **2** | The `N Warning(s)` / `N Error(s)` summary lines contain the words. Quote the three summary lines instead |
| A brace detector without the **matching** close paren | **a false negative on a nested-paren header** | **TWO independent routes were found in 6b-xviii**: paren-balance arithmetic, and a same-line-statement heuristic matching the `)` *inside* the header. Two different naive implementations, two mechanisms, the same confident miss |
| `git log --format=%B` vs an authored file | a spurious one-line difference | `%B` appends a newline. **Use `git cat-file -p <sha>` and skip the header** |

**`cat -A` piped into `grep` remains the only line-ending instrument that holds up** -- in single
quotes, with the `'\^M\$$'` spelling.

---

## Mechanisms 6b-xviii proved, and they are worth reusing wholesale

**`git commit -F` is now 11 for 11 byte-identical**, across bodies from 1222 to 4116 bytes. Write the
full message to a file in `dir-git`'s scratch directory, dispatch `git commit -F <path>`, and forbid
rewording, reflowing, trailers, and "recompute a figure then adjust the text" -- with the instruction
that a figure it believes wrong means **commit nothing and report**. **Verify yourself afterwards with
`git cat-file -p`, never `%B`.** Treat the paraphrase hole as closed.

**But verify the report, not just the commit.** Twice in 6b-xviii a `dir-git` report was not evidence
of what it claimed: one substituted `[message body as staged -- 51 lines, 3034 bytes, verbatim from
file]` for the actual body in its own `git show` output, and one **omitted an entire operation** from
a two-operation dispatch. Both commits were correct; neither report proved it. **Add to every
`dir-git` dispatch: print `git show --stat` verbatim, no placeholder, and report every operation.**

**File-scoped ownership, when rows collide on files.** 6b-xviii's rows overlapped heavily --
`HandCursorBehavior.cs` was touched by three rows and `FloorMapDrawable.cs` by two -- so tasks were
scoped to **disjoint file sets**, each task doing every row touching its files, rather than one task
per row. 21 `owns` globs, all disjoint, **zero concurrency conflicts**. Row-scoped ownership would
have been impossible: every row touches `00-master-index.md`, so any row-wise split of the register
work creates intersecting ownership.

**The orchestrator runs the build between waves.** Six parallel tasks cannot each carry a build
criterion -- concurrent `dotnet build` invocations collide on `obj/`/`bin/` -- so wave 1 verified by
file-content measurement only and the orchestrator ran one serial build before wave 2. It cost 44
seconds and it is what made "wave 1 regressed nothing" an attributable fact.

**The ANCHOR RULE, and it earned its keep repeatedly.** Append to any criterion citing a line: *every
line number here was measured at `<base>` and this session's own edits move it; locate each cited
construct by content search in the post-change tree, name the command, and write the re-derived line.*
**Never derive a line by subtraction from a span.** 6b-xviii had three off-by-one drifts, all from
arithmetic over base figures rather than reading a command's output -- and one task caught its own
drift because it cited a line in the file it was editing.

**Injection plus a positive control in the same run.** A red step is impossible for a preservation
fact, so the evidence is injection -- but **the control passing in the same run is what distinguishes
a test that catches the defect from a test that fails for any reason.** Two instances worth copying:
one task's clamp injection failed 1 of 3 cases and **it disclosed that plainly** rather than letting
"the fact failed" stand; another ran **four** injections where two were asked, the extra two aimed at
its own control, and found that blinding **one** side of a scan fails both facts while blinding
**both** makes the parity fact pass **vacuously** -- `null` equal to `null`, the exact failure mode
the control exists for, observed rather than argued.

**A positive control beside every load-bearing zero.** T9 listed nine such pairings: four braceless
zeros against 21 on another file, five space-form zeros against 35 on the exempt migration, a
`SetEditing` zero against 3 for `SetAreaEditing`, and so on. **Five zeros with no control are
indistinguishable from a blind predicate.**

**Partition a raw census when it differs from the figure you mean.** `grep -rn 'new FloorMapDrawable'`
returns **13**; the real call-site count is **7**. T9 wrote both with the six named near-misses, so
`7 + 6 = 13` -- because "7" beside that command with no partition looks fabricated to anyone who
reproduces 13.

**Reverse-reconstruction for "behaviour unchanged".** One task proved it by undoing both
transformations and matching the base file's `sha256` byte-for-byte. Its reviewer then **declined to
rely on that** -- a reversal written by the forward transformation's author shares its assumptions --
and established the same thing independently by alignment: 116 deleted lines all `///`, 132 added
being 116 `///` plus 16 brace-only, **636 == 636** identical sequences after stripping whitespace,
which excludes a reordering the script would not have known to undo. **Both are good; the second is
better, and the difference is who wrote the instrument.**

---

## What 6b-xviii got wrong about itself

Recorded because the next session inherits the habits, not just the register.

- **Three of the orchestrator's own figures were corrected by the agents it dispatched.** (a) It
  claimed `HandCursorBehavior.cs:67` could not be build-verified because it sits inside
  `#if MACCATALYST` -- inferred from a `grep` for the guard, never built. The baseline build emitted a
  530944-byte maccatalyst assembly. **A grep for a preprocessor guard establishes that a guard exists
  and nothing about whether the toolchain defines the symbol.** (b) It advised comparing against
  `git show <ref>:<path>` as a "sounder base" for line endings; blobs are LF-normalised, so that
  comparison cannot establish line endings at all -- its own `core.autocrlf` finding, documented in
  `measurement.md` §3 and not applied three sections later. (c) It recorded a comment block as
  `:10-30`; it is `:6-29`, wrong at both ends.
- **One false universal existed in three places and was caught by the task it was given to.**
  `measurement.md` recorded `MASTER-151`'s span as `:31-65` -- a *different* span from the `:35-66` the
  row actually cites -- so the row's own citation was never compared against the tree, and the
  orchestrator concluded `MASTER-148`'s was the only citation of eight that survived. **Two did.**
  The universal reached the frozen `manifest.json` through the architect and `coverage.md`'s own
  definition-of-done item through the orchestrator. T9 measured it, **refused to write the criterion's
  letter**, and recorded "one of only two". All three copies are now corrected, the manifest by
  amendment A1 at Gate 3. **A criterion that asserts a measurable universal cannot override the
  measurement**, and a task that says so is doing its job.
- **A census command in the evidence base was inert.** `grep -rln` with `|` and no `-E` returns 0; the
  figure of 12 beside it was correct. **Written into the very file whose instrument section warns
  about exactly this shape.**
- **An indent-width split summed to the artefact figure the same section rejected** -- "146 at 5
  spaces and 21 at 9" sums to 167, where the real population is 159. Correct is 138 and 21. **A
  figure that does not add up to its own total is a signal, and nobody checked the addition.**
- **A known-false sentence was committed deliberately, then fixed at Gate 3.** A new test file's XML
  summary claimed the unclamped notch "would reach or overshoot the top edge" at heights 10, 15 or 19
  -- true only at 10. Review classified it **advisory rather than blocking, and correctly so: no
  acceptance criterion covered the accuracy of a doc comment.** That is a real gap in the criteria,
  not a severity call to overturn. **If your session writes new prose into code, give some criterion
  authority over whether it is true.**
- **`escalationsRaised` was 0, and one escalation was arguably owed.** The architect raised a
  scheduling question -- where the first build signal should sit -- and the orchestrator resolved it
  at inference rung 4 because it was the orchestrator's own operation. That was defensible, and it is
  worth noticing that a session can reach 0 escalations partly by absorbing decisions.

**The pattern across all of it, unchanged from 6b-xvii and now better evidenced: the dominant failure
was never a wrong answer, it was a right-looking answer from an instrument nobody had shown could
fail.** Ten such instruments are catalogued above. In 6b-xviii, **four were found by the author of the
instrument** -- which is new, and encouraging -- but the three that mattered most were found by a
second party re-running the command. **A control only fully counts when someone other than its author
runs it.**

---

## Where to look for the rest

| You need | Go to |
|---|---|
| The evidence base, its eight corrections, and the instrument list with commands | `measurement.md` in this directory -- read sections 3, 4 and 7 |
| Per-criterion verdicts, all 31 advisory findings, and the three `contradicts` findings with their ranked options | `reviews/T1.md` through `reviews/T9.md` |
| What the decomposition promised and what it delivered | `spec.md`, `manifest.json`, `coverage.md` |
| The two Gate 3 amendments, with before and after | `state.json` -- the `amendments` array |
| The six items 6b-xviii carried forward | `.dir-sdlc/backlog.md`, under `## Sub-session 6b-xviii` |
| The `AMENDED 6b-xviii` block form, nine worked examples | `docs/audit/04-app-controls-and-theme.md` (8 blocks) and `01-domain-application.md` (1, on `CORE-026`). **Copy the FORM, not the contents** -- and carry the caveat that `AMENDED 6b-xiv` at `01-domain-application.md:1293` records that the block *it* was modelled on "carries three defects of its own" |
| What 6b-xviii actually did, commit by commit | `git log --oneline 57b7eba..0cc4799`. **Two ranges, two different surfaces -- see the note below** |

**The change surface has two correct answers and the wrong command gives you the wrong one.** Measured
with `git diff --shortstat`:

| Range | Surface | What it covers |
|---|---|---|
| `57b7eba..77bbaae` | **18 files, 1303 insertions, 192 deletions** | the ten task commits -- code and register only |
| `57b7eba..0cc4799` | **38 files, 3279 insertions, 192 deletions** | the above plus the merge and the `.dir-sdlc/` artifact commit |

The artifact commit alone is 20 files and 1976 insertions, and `1303 + 1976 = 3279` reconciles. **Ask
for the task surface and you want `..77bbaae`**, not the branch tip.

**Two of this orchestrator's own figures were wrong here, and both are recorded rather than quietly
fixed.** The first draft of this brief cited `57b7eba..0cc4799` as the source of the 18-file figure;
that range returns 38 files. And **the artifact commit `0cc4799`'s own body states "18 files, 1300
insertions, 192 deletions" -- the insertion count is stale by 3.** The 1300 was measured at `bbb3718`,
before the doc-comment fix `77bbaae` existed, and the body was written after it landed without
re-deriving the range. History is not rewritten for this, so **`0cc4799`'s body carries a figure three
too low and this brief is the correction.** It is the same defect class the session catalogued ten
times in other people's work: a figure carried across a boundary without recomputation.

**The six backlog items are unusually actionable** -- each names its measured evidence, its command,
and its known limits, so the next session need not re-derive them. Three are layer-wide populations
no row covers: **37** braceless control-flow sites across `DungeonMaster.App` (21 of them in the
UI-layer `DungeonDetailPage.xaml.cs`), **14** type-vs-filename mismatches, and a `<param>`-gap
**upper bound** of 14 that is explicitly not a count. Three came from Gate 3: **8 pre-existing
register rows whose status prose contains a literal `|` and breaks the markdown table they sit in**
(`MASTER-051`, `112`, `113`, `114`, `130`, `131`, `146`, `180` -- confirmed identical at base and
after), `ObjectAssetCatalogue.Paths` being public and undocumented (**bounded to exactly one member**,
which makes it a cheap close), and two stale citations.

**Unchanged and still unscheduled:** `B11`'s missing mechanical guard, **the UI latency spike** --
still the only item in the register or backlog that came from *running the application* -- and the
`ObjectRotation`/`PathSegmentKind` enum renames, which need real EF migrations because the enums are
persisted by member name.

**One standing exemption is now on the register and must not be re-litigated as a regression.**
Generated EF scaffolder output is exempt from `dotnet-format.md` §9's two-tab rule: all 7 files under
`src/DungeonMaster.Infrastructure/Migrations/` are space-indented with 0 leading tabs, and
`disciplines/database.md:15` makes editing a released migration prohibited rather than merely out of
scope. **A future census of the space-indent form returning 35 is reading the exemption, not finding
a defect.** `124 remedied + 35 exempt = 159`, the full measured population.
