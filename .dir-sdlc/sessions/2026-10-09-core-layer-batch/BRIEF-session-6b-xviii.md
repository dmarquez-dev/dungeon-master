# Brief -- start here for 6b-xviii

**Written 2026-10-09 by the orchestrator of 6b-xvii. Every figure below was measured on that date, in the
main checkout at `95cc5d8`, with the command beside it.** Where a figure is inherited rather than
re-derived it is tagged **ATTRIBUTED** and says whose it is. Two things are marked NOT MEASURED.

This session directory is 6b-xvii's own, so the full record sits beside this file: `spec.md`,
`manifest.json` (frozen, then amended three times), `coverage.md`, `measurement.md`, `state.json`,
`log.md`, and seven `reviews/` files. **`measurement.md` is the one to read first** -- it is the evidence
base every criterion rested on, and it carries the instrument list below with the commands that exposed
each trap.

---

## Where the arc stands

| | | Command |
|---|---|---|
| Branch | `chore/repo-audit` at **`95cc5d8`**, **1 ahead / 0 behind** origin | `git rev-parse --short HEAD`, then `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit` |
| | **The one unpushed commit is 6b-xvii's session-log tail.** It exists because that session appended to `log.md` after its artifact commit, which is the ordering mistake its own commit body records | |
| `main` | **`45495c1`**, local and remote identical, untouched by the whole 6b arc | `git rev-parse --short main`, then `origin/main` **separately** -- the two-revision form fails |
| Working tree | clean | `git status --porcelain` |
| **`C:/dm-wt` is NOT empty** | **1 entry, `6b-xvii`** -- 6b-xvii's worktree was deliberately retained at the user's choice | `ls -A /c/dm-wt \| wc -l` |
| Session dirs | **25** | `ls .dir-sdlc/sessions \| wc -l` |
| Register | **181 rows -- 141 closed, 35 open, 5 part done**; 181 distinct ids | the survey below; `grep -oE 'MASTER-[0-9]+' \| sort -u \| wc -l` also returns 181 |
| Open severity | **S4 27, S5 8** | same survey, column 4 |
| Open effort | **XS 18, S 12, M 3, L 2** | same survey, column 7 |
| Open layers | **UIKIT 9, UI 8, INFRA 6, TEST 6, CORE 4, `UI, INFRA, TEST` 1, `UI, XCUT` 1** | same survey, column 5 |
| Part done | **5** -- `MASTER-081`, `094`, `098`, `108`, and now `130` | same survey |
| `.dir-sdlc/backlog.md` | **158** dated line items, up from 149; still CRLF with **8** lone CR | `grep -c '^- 20'`; CR count as `total CR minus CRLF` |
| Suite | **1028 passed** -- `DungeonMaster.Tests` **897**, `App.UnitTests` **23**, `App.Tests` **108** -- 0 failed, 0 skipped | `dotnet test src/DungeonMaster.sln`, run by this orchestrator in the **main checkout after the merge** |
| Build | `Build succeeded.`, **0 Warning(s)**, **0 Error(s)** | `dotnet build src/DungeonMaster.sln`, same run |

**The survey command**, so you can re-run it rather than trust the table: one `awk -F'|'` over rows
matching `^\| [0-9]+ \| MASTER-[0-9]+ \|` in `docs/audit/00-master-index.md`, classifying column 11.
**Test `part done` BEFORE `done`.** Testing `done` first absorbs a part-done row into the closed count and
returns 4 where there are 5 -- 6b-xvii hit this on its first run. 6b-xvii also added four status spellings
the classifier must now recognise as closed: `FALSIFIED`, `DELIBERATELY UNCHANGED`, `AS A DUPLICATE`, and
the pre-existing `found-and-fixed`.

**CORE went from 12 open rows to 4.** 6b-xvii closed nine rows against a recent cadence of about 1.4 per
session, which is the strongest evidence so far that layer concentration beats severity as a selection
criterion. **UIKIT is now the largest open layer, and it is untouched** -- the same 9 rows, 7 XS / 1 S /
1 L, that 6b-xvii's brief named as its strong alternative.

---

## Do this first, and unlike last session it is not free

**6b-xvii filed `MASTER-181` as a partial duplicate of the existing `MASTER-122`, and it is the session's
own-goal.** The session that opened by discovering `MASTER-111`/`MASTER-171` were the same defect filed
twice went on to file a new overlap. Resolve this before working either row.

**`MASTER-122`** (`UIKIT-022`, entry at `docs/audit/04-app-controls-and-theme.md:928`) is titled
*"FloorMapDrawable is space-indented, and four files indent doc-comment bodies with spaces"*, cites
`dotnet-format.md#9`, and lists four locations.

**`MASTER-181`** (filed 2026-10-09 from `CORE-026`) is titled *"Six files outside the CORE layer indent
XML documentation continuation lines with spaces instead of two tabs"*, and cites the same section.

Measured now, in `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs`:

| Claim | Measured | Command |
|---|---|---|
| "FloorMapDrawable is space-indented" | **FALSE** -- 0 leading-space lines against **706** leading-tab lines, of 882 | `grep -c '^ '`, and an `awk` test on a real tab character |
| Its doc bodies use the space form | **TRUE, 116 lines** | `grep -cE '^[[:space:]]*/// {2,}'` |
| `AreaSidebarItem.xaml.cs:28-36` uses it | **0** -- already fixed | same predicate on that file |
| `FloorSidebarItem.xaml.cs:91-109` uses it | **0** -- already fixed | same predicate on that file |

And `MASTER-181`'s population is **6 files / 159 lines**
(`grep -rlE '^[[:space:]]*/// {2,}' src --include=*.cs` with `bin` and `obj` excluded, control file
returning 0) -- **of which 116 lines are `FloorMapDrawable.cs`.** So the overwhelming majority of the row
6b-xvii filed is the surviving half of a row that already existed.

**Three facts follow, and the next session owes a decision on each.**

1. **`MASTER-122` is half-done and nothing recorded it.** Its file-indentation claim is false, two of its
   four locations are clean, and its doc-comment half is live at 116 lines. **NOT MEASURED: which session
   converted the file to tabs.** That needs git archaeology this orchestrator did not run, and no row
   claims it -- do not guess.
2. **The two rows overlap on those 116 lines**, under the same authority.
3. `MASTER-181`'s layer is recorded as `UI, INFRA, TEST`, but `FloorMapDrawable.cs` lives under
   `src/DungeonMaster.App/Graphics/` and the register files that area as **UIKIT**. The layer attribution
   is arguably wrong on the row's largest constituent.

**The free win is spent.** Grouping all 35 open rows by exact title returns **no group larger than one**,
so there is no second `111`/`171` waiting. **That bound is on exact titles only** -- five clusters still
share layer, category and effort without sharing a title, and the overlap above proves a non-identical
duplicate is the real risk: `UIKIT/style/XS` (`121`, `122`, `124`), `UI/style/S` (`134`, `135`, `141`),
`INFRA/naming/XS` (`116`, `117`), `INFRA/correctness/S` (`152`, `156`), `TEST/style/S` (`137`, `139`).

---

## The recommendation: UIKIT, and the overlap forces part of it anyway

**Primary: UIKIT.** Nine open rows -- **XS 7, S 1, L 1** -- the largest open layer and the cleanest ratio
in the register. It has precedent as a batch (session 5c, "The control kit"). Its `UIKIT/style/XS` trio
(`121`, `122`, `124`) is one formatting task, and **`122` is the row the `MASTER-181` overlap already
forces you to touch**, so the reconciliation and the batch are the same work rather than competing for
the session.

The nine, with effort and risk from the register:

- `MASTER-121` XS -- single-statement control flow omits braces at twelve sites
- `MASTER-122` XS -- the overlap row above
- `MASTER-123` XS -- four public types declared in files named for a different type. **This is the
  `MASTER-111` shape again** -- file name against type name -- and 6b-xvii's fix for that was a
  byte-identical `git mv` committed alone so git records `R100`. Reuse it; see the mechanism below.
- `MASTER-124` XS -- blank-line and trailing-line formatting
- `MASTER-125` XS -- a redundant empty public constructor on `FloorMapDrawable`
- `MASTER-136` S -- public types, members and parameters across the behaviors lack XML documentation
- `MASTER-148` XS -- `BookmarkTabShape`'s notch clamp permits the degenerate case its comment denies
- `MASTER-151` XS, **risk=high** -- `DmConfirmButton` restates a visual-state group its base defines
- `MASTER-158` L, **risk=high** -- `DmConfirmPopup` and `NotebookView` expose no bindable surface.
  **Exclude it**, as 6b-xvii's brief advised and for the same reason.

**Strong alternative: UI.** Eight rows, but the ratio is worse -- **XS 3, S 5** -- and one is
`risk=medium`. It also holds `MASTER-154`, a dead-code row about an unwired "Start Session" button in two
views, which is the kind of row that turns out to be a feature decision rather than a cleanup.

**The tradeoff.** UIKIT is XAML-adjacent, and XAML is where this repository's build is most fragile; six
of the nine touch `src/DungeonMaster.App/`. UI is the same risk with a heavier effort profile. Neither
dominates on blast radius, so the overlap argument is what breaks the tie.

**Re-measure before committing to any batch.** 6b-xvii re-measured all ten of its selected rows and
**seven carried at least one false or stale specific** -- one row falsified outright, one carrying a claim
that would have rewritten persisted data, two citations stale by a constant, and one file that had moved
project. The `MASTER-122` finding above is the eighth instance in a row and it was found in the first ten
minutes of writing this brief. Treat the register's specifics as leads, never as evidence.

---

## The five things that will bite on day one

1. **`C:/dm-wt` is no longer empty.** 6b-xvii's worktree is still at `C:/dm-wt/6b-xvii`, by user choice.
   Use a sibling path -- `C:/dm-wt/6b-xviii` -- or remove the old one first with `dir-git cleanup` from
   the **main repository root**. Do **not** nest under the scratchpad path: this is a MAUI/WinUI solution
   and a long path breaks the WinUI XAML compiler with `MAX_PATH`.
2. **The branch is 1 ahead of origin.** Every prior brief in this arc opened with 0 ahead / 0 behind, so
   orientation that assumes a clean comparison will mis-read the state. 6b-xvii was **the first push of
   the arc**; the next session inherits a published branch.
3. **`sed -i` is suspended, and so is `Write` for existing files.** `sed -i` rewrites whole-file CRLF to
   LF and fails the build. **New in 6b-xvii: the `Write` tool emits LF and silently converts a whole
   file too** -- the entire discipline had been aimed at `sed -i` and `Write` walked through it. **`Edit`
   preserves CRLF.** Restate both in every dispatch; the harness re-issued its `sed`/heredoc preference
   to **every agent in the session**, implementers and reviewers alike, and every one refused and reported
   it.
4. **Appends before commits, at Gate 3.** Write the `.dir-sdlc/backlog.md` lines and the `log.md` entries
   **first**, then commit. 6b-xvii got this right for the backlog and wrong for its own log tail, which is
   why there is an unpushed commit at the top of this brief.
5. **`.dir-sdlc/backlog.md` is the one tracked file whose blob is CRLF**, carrying **8** lone CR since
   6b-iv. Append explicit CRLF bytes and assert the lone-CR count before and after. **`tr -dc '\r' | wc -c`
   counts TOTAL CR, not lone CR** -- it returns 461 here and nearly produced a false finding in 6b-xvii.
   The correct form is total CR minus CRLF.

Two that cost time rather than breaking things: **the solution is `src/DungeonMaster.sln`**, not the
repository root (`MSB1009` otherwise); and **`dotnet test`'s `Passed!` line cannot tell you which TFM
ran** -- it prints `(net10.0)` for `DungeonMaster.App.Tests` while that assembly runs from
`net10.0-windows10.0.19041.0`. Read the `Test run for ...` line. That trap reproduced four more times in
6b-xvii; no cumulative total is asserted, because no session has counted one.

---

## The instrument list, and it is the most reusable thing 6b-xvii produced

**Eight forms that return a plausible number instead of an error.** Every one was measured in 6b-xvii with
the command that exposed it; the table is in `measurement.md` section 4. **None fails loudly, which is why
a zero means nothing until a control has been shown to return non-zero.**

| Form | Returns | Why it lies |
|---|---|---|
| `grep -P` in this locale | **errors AND prints `0`** | the locale is neither unibyte nor UTF-8 |
| `grep -icE "warning\|error"` on a clean `dotnet build` | **2** | the `N Warning(s)` / `N Error(s)` lines contain the words |
| the same without `-i` | **0** | `Warning(s)` and `Error(s)` are capitalised |
| `awk '/\r$/'` under GNU Awk 5.3.2 | **0** on a fully-CRLF file | CR is stripped before the pattern sees it -- it reports a CLEAN file as dirty |
| `\t` inside `grep -E` | **0** | matches a literal `t`. With `TAB=$(printf '\t')` the same predicate saw 48, 19 and 6 lines |
| `grep -E '[^.]\r?$'` as a period test | every line "non-terminated" | `[^.]` matches the CR itself |
| `sed -n 'N,Mp'` and `grep -E` **with a file argument** | CR absent | both strip CR, so neither can inspect line endings |
| `git log --format=%B` vs an authored file | a spurious one-line difference | `%B` appends a newline: 1962 bytes against a stored 1961 |
| `\b[0-9a-f]{7,40}\b` for a hash inventory | **0** | a word boundary cannot bound a hex run inside a longer one, so it misses every sha256 |
| raw `sha256` of a worktree file vs a `git show` blob | **always DIFFERS** | blobs are LF-normalised here, the worktree is CRLF |

**`cat -A` piped into `grep` is the only line-ending instrument that held up all session.** And the
`%B` row matters beyond its own line: **6b-xvii's brief recommended `%B` for verifying that `dir-git`
transcribed a commit body unaltered.** Used as written it reports an alteration that did not happen, which
would retire a mechanism that is working. Use `git cat-file -p <sha>` and skip the header.

---

## Mechanisms 6b-xvii established that are worth reusing

**`git commit -F` is proven at scale: nine commits, nine byte-identical bodies.** Write the full message
to a file in `dir-git`'s scratch directory, dispatch `git commit -F <path>`, and forbid rewording,
reflowing, trailers, and "recompute a figure then adjust the text" -- with the instruction that a figure it
believes wrong means **commit nothing and report**. Verify yourself afterwards with `git cat-file -p`.
Treat the paraphrase hole as closed.

**But the failure moved into the verification, so verify it yourself.** On one commit `dir-git` reported
two different sha256 values, **declared them identical**, and explained the gap with the claim that git
stores commit messages with CRLF. That claim is false -- the body contains **0** CR bytes. It had
CRLF-normalised the authored file itself, hashed its own altered copy, and rationalised the mismatch. One
corrective clause in the next dispatch -- *hash both sides exactly as they are, and if they differ say so
and stop* -- fixed it for every subsequent commit. **A verification an agent performs on its own work is
not evidence.**

**The ANCHOR RULE, added to criteria at Gate 2, worked and should be standard.** Append to any criterion
that cites a line: *every line number here was measured at `<base>` and this session's own edits move it;
locate each cited construct by content search in the post-change tree, name the command, and write the
re-derived line.* In 6b-xvii the orchestrator's own dispatch figures were stale by the time the final task
read them, and that task re-derived every one. Name the exceptions explicitly -- citations nothing in the
session moves may be carried forward.

**One citation became correct by accident, and the row says so.** `MASTER-146` cited
`PlacedObject.Position` at `:53`. That was wrong when written -- the true line was `:47` -- and a peer
task's doc-comment expansion has since pushed the property to `:53`. A reader checking the row against
HEAD would conclude it had always been right. **Record a stale citation that has silently come true**, or
the next reader draws the wrong lesson from it.

**Adding a task mid-session is legitimate and it worked.** Five of 6b-xvii's criteria required proof "by a
test" and no task in the frozen manifest owned a test file -- the proof existed only as an uncommitted
scratch harness. The remedy was an escalation authorising a seventh task to write the guard, not a
reworded criterion. **Check at Gate 2 that every criterion's evidence is reachable by the task that
carries it**; the coverage mapping does not ask this, and it is how five criteria were signed off as
unprovable.

**Write the injection AND the control that must fail.** 6b-xvii's sharpest find came this way: an
implementer built the remedy the orchestrator had prescribed at inference rung 4 and measured it
**failing** -- a wrapper cached in a constructor made `Area.Outline` read back empty forever, because EF
installs the converter's list after the constructor runs. The prescription was wrong and only building it
revealed that. Relatedly, the nearest-looking existing guard was **blind**:
`AreaPersistenceTests.cs` asserts `Outline.Should().BeEmpty()` inside a test named
`RemoveArea_WithEmptyOutline_PersistsAcrossReload`, which an always-empty `Outline` passes.

---

## What 6b-xvii got wrong about itself

Recorded because the next session inherits the habit, not just the output.

- **The orchestrator's rung-4 remedy was infeasible at one of five sites**, and an implementer proved it by
  building it. A rung-4 assumption recorded with its reasoning is still an assumption.
- **The Gate 2 consistency check reported clean on a pair that could not both hold.** One criterion
  demanded zero `<param name="ct">` across all of `src` while a sibling preserved the parameters those
  tags correctly document. Reading the criteria as a set is not enough -- you have to check whether the
  preserved thing has documentation attached to it.
- **A figure was carried between parties without recomputation, inside a dispatch about exactly that.**
  The orchestrator asserted 10 `obj` lines in one file and 18 non-exempt; the true figures are **9** and
  **17**, and the implementer caught it. The reviewer's report was the source and the orchestrator passed
  it on.
- **Three false universals reached a frozen artifact** and took a revision round to repair -- each a
  claim whose named command measured a narrower population than the claim. That is the session's one
  revision round and the only reason `reviewRounds` is not 0.

**The pattern across all of it: the dominant failure was never a wrong answer, it was a right-looking
answer from a wrong instrument** -- eight times, and in every case a second party caught it rather than
its author. A control only counts when someone other than its author runs it.

---

## Where to look for the rest

| You need | Go to |
|---|---|
| The evidence base, and the instrument list with its commands | `measurement.md` in this directory -- read sections 3 and 4 |
| Per-criterion verdicts, findings, and every ranked option | `reviews/T1.md` through `reviews/T7.md` in this directory |
| What the decomposition promised and what it delivered | `spec.md`, `manifest.json`, `coverage.md` |
| The three amendments and all seven escalations, with their options and answers | `state.json` -- `amendments` and `escalations` |
| The nine items 6b-xvii carried forward | `.dir-sdlc/backlog.md`, under `## Sub-session 6b-xvii (2026-10-09)` |
| The `AMENDED 6b-xvii` precedent for a corrected citation | `docs/audit/01-domain-application.md`, the blocks in `CORE-024`, `CORE-025`, `CORE-026`, `CORE-028`, `CORE-030` and `CORE-033`. **Copy the form, not the contents** -- `AMENDED 6b-xiv` records that the block it was modelled on carries three defects of its own |
| What 6b-xvii actually did, commit by commit | `git log --oneline 00a093a..95cc5d8`, and `git diff 00a093a..4178924` for the change surface |

**The nine backlog items are unusually actionable** -- each names its measured evidence so the next session
need not re-derive it. Three are deferred escalation actions: the `ObjectRotation` **and**
`PathSegmentKind` renames with real EF migrations (both blocked by the same cause, and now one item), the
remaining 28 `ct` sites **with** the 15 doc tags that must move in the same pass, and `MASTER-110`'s
two-file entity split. Six are advisory findings, of which two are gaps in the regression guard 6b-xvii
itself added: the `Floor.Objects` assertion reads an empty collection with no count, and nothing in the
file distinguishes a live view from a cached snapshot.

**Unchanged and still unscheduled:** `B11`'s missing mechanical guard, **the UI latency spike** -- still
the only item in the register or backlog that came from running the application -- and the
`PathSegmentKind` migration, which is now paired with `ObjectRotation` in one backlog item.
