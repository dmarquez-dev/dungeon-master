# Handoff to session 6b-xv

Written at the close of 6b-xiv, 2026-09-30. Self-contained: everything 6b-xv needs is here or at a path
named here. Nothing in it depends on the conversation that produced it.

**Every number below was re-derived from the repository immediately before writing.** Section 6 gives
you the commands. **Re-run them rather than trusting section 2** — and read section 0 first, because
this session's own inherited rule was about exactly this.

**A note on the slug.** The artifacts live under `2026-09-29-dungeon-facade-lookup-extraction`. Gate 1
and implementation ran on the 29th; Gate 3 and the merge ran on the 30th. The slug's date is the
session's start.

---

## 0. The one thing to carry forward, if you read nothing else

**Six false line-range or count claims this session. Three were mine. One of the three was written
inside the correction of another of the three.** That nesting is the finding, not the total.

| # | The claim | What is true | Who caught it |
|---|---|---|---|
| 1 | `measurement.md` §1 claim 4: `CORE-015`'s 8 locations drifted **`+13` to `+51`** | **`+12` to `+40`** at opening lines. `+51` is `323 → 374`, the **four-parameter** `AddPlacedObject` — a `CORE-019` citation, not one of `CORE-015`'s eight, which cites `356-380` and moved `+18`. The low end was wrong independently. **Neither endpoint was measured** | **T3's reviewer**, as a blocking finding — after it was frozen into C15 and written into a permanent record |
| 2 | The repair for #1 said that row is **the only** row in §1 with no command beside it | **One of three.** §1 has 6 claims against 3 command bullets; claims 4, 5 and 6 all lack one | **T3's round-2 reviewer**, as a `contradicts` finding |
| 3 | C17 located the false ObjectScale passage at `:995-997` | Its quoted tail runs onto **`:998`** | **T3 itself**, which wrote the accurate range and flagged the deviation rather than conforming |
| 4 | `dir-architect`'s C13: the sentence spans `:999-1000`, and `:1000` "is where `:371` appears" | Spans **`:999-1001`**; both `:323` and `:371` sit on **`:1001`** | The orchestrator, before the freeze |
| 5 | `dir-architect`'s C15: `CORE-015`'s locations are at `:759-766` | **`:761-768`.** Its range starts two lines early on `citation:`/`locations:` and would have counted 6 of the 8 | The orchestrator, before the freeze |
| 6 | T3's round-1 block: `:761-768` and `:933-938` "have already moved" | **`:761-768` never moved** — it sits *above* the insertion point. Only `:933-938` moved | **T3's reviewer** — a false line-range claim sitting inside the block about false line-range claims |

> **The rule you inherited was: run any claim about a mechanism, not just about a tool. This session
> supplies its corollary, and it is cheap: _a figure asserted where the document's own convention names
> a command is not an oversight, it is a signal._** `measurement.md` §1 had six claims and three
> commands. The one row that carried a figure with no command is the row that was false, and it was
> false in both directions at once.

**Four of my own instruments returned confident wrong answers, all caught by checking rather than
re-reading:**

1. **Python text mode silently translates CRLF.** `open(p, encoding="utf-8-sig").read()` then
   `t.split("\r\n")` returned **1 line** for a 489-line file and made every `\r\n` regex return 0. Use
   `newline=''` and **assert `"\r\n" in t` before counting.**
2. **A pipe through `tee` masks the exit code.** `dotnet test <bad path> | tee log | tail` reported
   **exit 0** over `MSBUILD : error MSB1009`. Redirect and echo `$?`.
3. **`grep -cF` on a phrase that wraps across lines returns a confident zero.** This bit me twice, both
   times on verbatim quotations in the audit document. **Collapse whitespace first**:
   `" ".join(open(p,newline='').read().split())`.
4. **`git rev-parse --short main origin/main` fails** with *"Needed a single revision"* — it does not
   take two revisions that way. For a second that read as `origin/main` having vanished.

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`50b627d`** (the session-artifact commit) plus this handoff's own
  commit after it. **Re-derive the push state yourself** with
  `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit`.
- **`main` is untouched** at `45495c1`, local and remote identical. Nothing in 6b-xiv went near it.
- 6b-xiv merged as **`e5d1398`**, a `--no-ff` merge with **exactly two parents**, `20b0b09` and
  `a70fc95`. **Parent count verified with `git cat-file -p`**, not taken from `dir-git`'s report.
- The session branch carried **four** commits — one more than the three-task shape, because the last is
  a post-Gate-3 correction:
  - `aa5d7ac` (T1, five new tests)
  - `66a0b73` (T2, the two private helpers)
  - `e2315e3` (T3, the closure and two amendment blocks)
  - `a70fc95` (T3, a one-sentence correction inside one of those blocks)
- Change surface over `20b0b09..a70fc95`: **4 files, 395 insertions, 65 deletions**, from
  `git diff --stat`.
- **Verified in the MAIN checkout after the merge**, not only in the worktree: all four merged files
  `crlf == lf == cr`, and two of them `sha256`-identical between worktree and checkout.

### Index arithmetic — read this before citing any line number

| File | At `20b0b09` | Now | Offset rule |
|---|---|---|---|
| `docs/audit/00-master-index.md` | 687 | 687 | **Unchanged.** One Status cell replaced in place |
| `docs/audit/01-domain-application.md` | 1820 | **2061** | **+241**, in two blocks: `CORE-015` at `:792` and `CORE-019` at `:1133`. A citation at or above `:791` is unaffected |
| `src/.../Dungeons/DungeonMethods.cs` | 489 | **471** | **−18** |
| `src/.../Dungeon/DungeonAggregateTests.cs` | 473 | **580** | **+107**, insertions only |
| `.dir-sdlc/backlog.md` | 431 | **439** | **+8**, all appended at the end |

**Do not apply the table and consider the citation verified.** It tells you where a line *moved*, not
whether the claim at that line is still true. Items 1, 4, 5 and 6 of section 0 are all instances.

---

## 2. What is open

179 entry rows: **132 closed, 44 open, 3 part done.** Zero rows return `UNCLASSIFIED`.

Open by layer — `CORE` **13**, `UI` 9, `UIKIT` 9, `INFRA` 6, `TEST` 6, plus 1 spanning `UI, XCUT`.
By severity — **S1 0, S2 0**, S3 **2**, S4 32, S5 10.

> **Still no open S1 or S2.** The severity argument remains spent; 6b-vii through 6b-xiv all picked on
> coherence and layer concentration, and it has now worked eight times.

`MASTER-107` is closed. **The S3 band fell from 3 to 2 and the CORE layer from 14 to 13.**

The three `part done` entries, which **must not be marked done**: `MASTER-094` (`:282`), `MASTER-098`
(`:286`), `MASTER-108` (`:296`). Both inherited warnings still stand — `MASTER-098` was *measured* by
6b-vi and not closed, and **`MASTER-108`'s prose carries a wrong number**: it says
`DungeonDetailPage.xaml.cs` is "2,226 lines", which 6b-xi measured at **2,594**. Untouched by 6b-xiv.

**The two remaining open S3s:**

| Line | Entry | Layer | Title (truncated) |
|---|---|---|---|
| `:239` | `MASTER-051` | UI | Entry and Editor handler mappers repeat eight WinUI resets |
| `:269` | `MASTER-081` | CORE | The Domain has no policy layer |

---

## 3. The recommended session

A recommendation, not a decision — 6b-xv's Gate 1 owns the scope.

**Recommended: `MASTER-081` / `CORE-016` — the Domain has no policy layer and scale validation is
inline and duplicated.** CORE, S3, `effort: M`, `risk: low`, and the last CORE S3.

**Why it is the strongest candidate.** It is `M`/`low` where `MASTER-107` was `L`/`high`, its subject
sits in files this session just worked in, and **it has a standing connection to work already deferred**:
its suggested direction is an `ObjectScale` value object, and an `ObjectScale` value object is already
a deferred review item from the Object Scale feature. Check that the two agree before scoping.

**Re-measure first, and expect the entry to be wrong somewhere.** Every entry re-measured in this stream
has had at least one false or stale specific, and `CORE-016` makes checkable claims: that scale
validation is "written twice, verbatim" in `PlacedObject`, that `ObjectScale` is "a static class of
`const float`s plus two clamping helpers", and that the three `ValueObjects/` records "accept any input,
including `float.NaN` for `CanvasPoint`". **`PlacedObject.cs` is 89 lines now and its cited line ranges
predate three sessions of change.**

**The argument against it, at full strength.** `CORE-016` is one of two entries whose suggested
direction the Domain's current shape does not obviously admit — a policy layer is a new architectural
seam, and `backend-conventions.md` section 5 is an external document this repository does not currently
follow anywhere. If Gate 1's measurement says the entry is really asking for a new layer rather than a
value object, **say so and pick something else.**

### The alternatives, ranked

1. **The four items 6b-xiv left in the backlog**, the first of which is the strongest single item in the
   file: **`B11` has no mechanical guard and the gap is measured, not suspected** — T2's I3 injection
   proved the naive collapse passes all 980 tests. One test closes it. **Cheapest high-value work
   available.**
2. **The UI latency spike** (`backlog.md`, 2026-09-18). **Still the only item in the register or backlog
   that came from running the application**, and still untouched by 6b-xii, 6b-xiii and 6b-xiv. Wants a
   profiler and a Release-build comparison; the user asked that a session decide whether it warrants its
   own `MASTER` entry.
3. **`MASTER-051`** — the Entry and Editor handler mappers repeating eight WinUI resets. UI, S3, and now
   the only open S3 outside CORE.
4. **The `PathSegmentKind` migration, from `CORE-035`.** Still **the only item in the register requiring
   a data migration on persisted rows.** Scope it as a migration or not at all.

---

## 4. The inherited discipline

**Re-measure every entry against a fixed commit before decomposing, and decompose from the
measurement.** Eight sessions of evidence. 6b-xiv adds four refinements.

### The Gate 2 consistency check earned its keep, and this time it is measured

6b-xiii recorded that the check has a blind spot. 6b-xiv found a different shape it *does* catch, and
then **measured what catching it was worth**.

`B3` permitted the outcome re-check to reach zero "provided B5 and B7 still hold" — and neither reached
the thing at risk: `B5` covered only floor-absent dispositions, `B7` compiles, `B8`'s suite does not
cover it. Collapsing the re-check naively advances `LastModified` on a *failed* inner operation, which
is persisted in all four EF configurations and surfaced to the UI.

**The user added `B11` at Gate 2. T2 then injected the exact collapse `B11` forbids and the entire suite
passed — 855 / 23 / 102, exit 0.** It would also have satisfied `B1`, `B3`, `B4`, `B5`, `B7` and `B8`.

> **`B11` was the only thing in the manifest standing between this session and a silent, persisted,
> user-visible regression, and a null result is what proved it.** The finding is not "the check works" —
> it is that **a criterion whose permission is broader than its guard reads exactly like a clean
> criterion**, and the only way to tell is to ask what each stated safeguard actually constrains.

### An amendment can repair an error and introduce another

Two amendments, `A1` and `A2`, **both repairing an orchestrator error, and `A2` repaired a false
universal written inside `A1`'s own correction.** Both rounds were amendment-caused and charged neither
`revisionRounds` nor `reviewRounds`, which is the rule working correctly — T3 committed no error in
either round.

**The lesson for the amendment text itself: a correction is new prose and gets no exemption.** `A1`
prescribed a sentence verbatim, and that sentence was false. Apply the same measurement standard to a
repair that you apply to the thing being repaired.

### Reviewers answered questions nobody asked, for the eighth session running

- **T1's reviewer closed T1's own stated evidence gap.** T1 said plainly that flipping an expected value
  proves an assertion is live but **not** that the `NotFound` comes from `Dungeon`'s guard rather than
  `Floor`'s. The reviewer added five temporary control facts with the **real** `floor.Id` — `Failed: 0,
  Passed: 46` — and settled it. **The implementer naming its own evidence's limit is what made the
  reviewer's control obvious.**
- **T2's reviewer verified `B11` by a whole-file `Touch()` census** rather than by reading five method
  bodies: 8 call sites, none inside the span covering the five re-check methods, so the only reachable
  route is the guarded one.
- **T3's reviewer ran git where T3 could not.** T3 was forbidden `git` and required to take
  `measurement.md` as given, so **the reviewer checking whether what T3 was handed was true is the only
  reason the drift range was caught.** Dispatch reviewers with that asymmetry stated.
- **T3 corrected two defects its own edits created**, unprompted, including a universal that its round-1
  insertion had falsified.

### `dir-git` — the split is now three sessions old and its shape is exact

Six dispatches: `init-session`, four `commit-task`, `merge-session`. **Every repository write was
correct: every path list, every SHA, every `--numstat`, one parent on each task commit, exactly two on
the merge, no forbidden trailer anywhere.**

**One body was wrong, and the way it was wrong is the useful part.** `66a0b73` said the helper replaced
the lookup in *"eight removal/mutation methods"* (it is **ten**, and its own next line says 10) and named
*"RemoveObject, and UpdateObject"* — **`Floor` members, not `Dungeon`'s**.

> **Both remedies held for every figure `dir-git` was HANDED.** The `--numstat` warning worked; naming
> the figures and forbidding relabelling worked — the suite counts came back correctly labelled in all
> four commits. **The two wrong claims were ones it derived from the diff itself, which is where it has
> no guard.** The third remedy is therefore: **hand over every count and identifier the body will need,
> and tell it to take names from the diff or from the dispatch and nowhere else.** The T3 dispatches did
> exactly that and both bodies came back accurate.

And the irony worth keeping: **`RemoveObject`/`UpdateObject` is precisely the rename confusion
`CORE-015`'s own "identically named" claim got wrong — the claim this session corrected in C4 —
reproduced one layer down in the commit that fixes it.**

**`a70fc95` is subject-only**, against the 37-of-40 body convention. It was a one-sentence fix and the
dispatch asked for a short body; it wrote none.

### The structural limits, unchanged and re-confirmed

- **Tasks cannot invoke git.** T3 needed six historical commits and was handed every figure.
- **No criterion may constrain what a task *reports*** — only what the repository *contains*. This bit
  twice: coverage row 7a had no criterion for that reason (escalation E1), and `B9` had to be worded as
  the clean final state with the demonstration owed to the report.
- **A criterion needing a figure a task cannot measure must have that figure handed over at dispatch.**
  `B8` needed the post-T1 suite total; the dispatch carried it.
- **Revert by re-editing content, never by restoring a copy.** T1, T1's reviewer and T2 all injected and
  all reverted; every revert was verified by digest.
- **`sed -i` remains suspended, and the harness re-issued it to EVERY agent this session — all eleven
  refused**, including three separate refusals from T3 across three rounds. Restate it in every dispatch,
  reviewers included.

### The environment findings

**The line-ending mechanism, measured rather than inherited — and the inherited version was wrong.**
`.gitattributes` is `* text=auto` **and `core.autocrlf` is `true`**. 6b-xiv's architect predicted that
LF artifacts "normalize on the first commit and the main checkout will show CRLF". **Half right, and
the wrong half matters.**

- **Storage: always LF.** Every artifact blob is stored LF — verified byte-exactly over `git cat-file -p`
  output for this session's and the previous session's `spec.md` and `measurement.md`, and for a
  discipline file written as CRLF. **All five returned `crlf=0, cr=0`.**
- **Working tree: CRLF only after an actual checkout.** A file an agent *writes* is never checked out,
  so it stays LF on disk even after being committed. Immediately after `50b627d`, **11 of this session's
  15 artifacts were LF in the working tree** while the four disciplines (written as CRLF by hand) were
  CRLF.
- **Nothing is wrong, and the proof is one command.** Deleting a committed artifact and running
  `git checkout -- <path>` restored it at `crlf == lf == cr == 156`, with `git status` clean. The
  repository content is identical to every previous session's; only the un-checked-out working copy
  differs, and it converges on any fresh checkout.

> **Do not "fix" an LF artifact in the working tree, and do not report it as a defect.** The measurement
> that matters is the stored blob, not the file on disk — and the two disagree by design here. A prior
> handoff's "all 13 session artifacts verified `crlf == lf`" was measuring a working tree, which answers
> a different question than the one it sounds like it answers.

**Source files are a different matter and the check there IS load-bearing:** all four files the merge
touched came back `crlf == lf == cr` in the main checkout, and two were `sha256`-identical to the
worktree. **That is why verifying in the main checkout after the merge is not optional.**

**The zero-warning check is command-specific.** `grep -icE "warning|error"` returns 0 over `dotnet test`
output but **always 2** over `dotnet build`. Over a build use `grep -cE "(warning|error) [A-Z]+[0-9]+"`.

**`dotnet test`'s `Passed!` line cannot tell you which TFM ran.** It prints `(net10.0)` for all three
assemblies while `App.Tests` runs from `net10.0-windows10.0.19041.0`. Read the `Test run for ...` line.
**Reproduced four times this session** and now recorded as stable rather than intermittent.

**The solution is at `src/DungeonMaster.sln`, not the repository root.** A `dotnet test` against the
root path fails `MSB1009` — and through a `tee` pipe it reports exit 0.

**`grep -cE "^\tpublic "` inside double quotes returns 0 in this shell** — the shell does not expand
`\t`. Use `T=$'\t'` and `"^${T}public "`.

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **439 lines** by `b.count(b'\n')`. 6b-xiv appended **8 lines / 4 items**.

> **The 8 lone CR characters are still there and are still not from this session.** Measured in binary
> before and after the append: **8 both times**, and the append script asserted on it and would have
> failed hard if it moved. They date to 6b-iv. **State which method you used** — Python's text-mode
> iteration gives a different total, because universal-newline handling splits on a lone CR too. 6b-xiv
> deliberately did not repair them — outside the approved scope, for the ninth session running.

**6b-xiv's four items, all carried by explicit user decision at Gate 3:**

1. **`B11` has no mechanical guard, and I3 measured it** — the naive collapse passes all 980 tests and
   would satisfy six other criteria. **The strongest single item in this file**: the hole is measured,
   not suspected, and one test closes it.
2. **`66a0b73`'s two false body claims**, permanent in history — including `CORE-015`'s own rename error
   reproduced in the commit that fixes it.
3. **A universal in the `AMENDED 6b-xiv` preamble still reaches the `A1` table**, which was handed over
   with a method rather than a command. One clause closes it. **No criterion constrains that preamble**,
   which is why it survived three review passes.
4. **Correction 4's cited `grep -c` decays inside its own commit** — it says 5, and T2's landing makes it
   return 1. Not false (the anchor is in the paragraph above) but a mismatch inside a block whose thesis
   is that unanchored positions decay.

**Still unscheduled from earlier sessions:** the UI latency spike; the five misleadingly named
`WithUnknown{Area,Object}Id` tests, whose confusion risk **rose** this session now that ten
similarly-named tests sit in one class across two different guards; the `A3` test with no non-mutation
assertion; the discipline-versus-contract sleep conflict and the clock seam; the `ObjectKindInfo` `None`
absorption; the drop-placeholder indicator design question; `AreaSidebarItem`'s two
`PointerGestureRecognizer`s; the truncated floor name with no recovery affordance; the Appium
DIU-versus-pixel ratchets; the EF instrument that cannot see a consistent namespace rename; the three
6b-iii items on `docs/audit/05-tests.md`; the `with`-expression gap on `PathSegment`; the bare `else` in
`ValidateControlPoints`; the count-only assertion in `AreaOutlineTests`; the fifteen items 6b-ix filed;
6b-x's four; 6b-xi's three; 6b-xii's four; and 6b-xiii's four.

**`dm-conventions` still teaches the pattern 6b-v removed** — `backend/dotnet-format.md:567` and `:588`.
Separate repository, deliberately not modified.

**`.dir-sdlc/log.md` does not exist and never has**, across all 23 sessions. This project keeps
session-level logs only. Do not create it without deciding to.

**`Floor` carries the same lookup-and-guard pattern five more times** (`FloorMethods.cs:90`, `:128`,
`:162`; `FloorObjectMethods.cs:56`, `:95`). The Domain holds 15 instances; 6b-xiv addressed `Dungeon`'s
10. Declared out of scope at Gate 2 and recorded in `measurement.md` §5.

---

## 6. Commands to re-derive section 2 yourself

Unchanged, and it still classifies every row. Re-run rather than trusting section 2.

```bash
python - <<'PY'
import re
from collections import Counter
rows=[]
for ln,line in enumerate(open("docs/audit/00-master-index.md",encoding="utf-8"),1):
    if not re.match(r'^\| *\d+ *\| *MASTER-', line): continue
    c=[x.strip() for x in line.split("|")]
    rows.append(dict(line=ln,id=c[2],sev=c[3],layers=c[4],title=c[8],status=c[10]))
def state(st):
    s=st.lower()
    if st=="": return "open"
    if "part done" in s: return "part done"
    if "not an open item" in s or "falsified" in s: return "closed"
    if re.search(r'\*\*open', s) or s.startswith("open"): return "open"
    if "**done**" in s: return "closed"
    if "not done" in s: return "not done"
    return "UNCLASSIFIED: "+st[:70]          # never silently bucket
for r in rows: r["state"]=state(r["status"])
print("total entry rows:", len(rows))
for k,v in Counter(r["state"] for r in rows).most_common(): print(f"  {v:4}  {k}")
op=[r for r in rows if r["state"]=="open"]
print("\nby layer:");    [print(f"  {v:4}  {k}") for k,v in Counter(r["layers"] for r in op).most_common()]
print("\nby severity:"); [print(f"  {v:4}  {k}") for k,v in sorted(Counter(r["sev"] for r in op).items())]
print("\nopen S3:");     [print(f"  :{r['line']}  {r['id']}  {r['layers']}") for r in op if r["sev"]=="S3"]
print("\npart done:");   [print(f"  :{r['line']}  {r['id']}") for r in rows if r["state"]=="part done"]
PY
```

**The column index is `[10]`**, because splitting a row on `|` yields a leading and a trailing empty
cell. **Re-run the survey after you edit the index, and again after the closure commit.**

---

## 7. Mechanics

- **Push status must be re-derived, not read here.** The push question is asked separately from Gate 3.
- **Worktree root must be short.** Use `C:/dm-wt/6b-xv`. This is a MAUI/WinUI solution and a long path
  breaks the WinUI XAML compiler with `MAX_PATH` errors; the scratchpad path is too long. `C:/dm-wt` is
  empty again after 6b-xiv's cleanup.
- **Tracked files are CRLF and a formatting slip fails the build.** **Suspend the harness's `sed`
  instruction in every dispatch** — it will be re-issued mid-run, to every agent. Eleven refused it this
  session.
- **Close any running `DungeonMaster.App` before building the Windows TFM**, or `MSB3026`/`MSB3027`.
  That is a file lock, not a test failure.
- **Verify in the main checkout after merge**, not only in the worktree. `.gitattributes` is
  `* text=auto` and agent-written artifacts are LF.
- **Tasks share one worktree and one `obj/`/`bin/`.** 6b-xiv ran **T1 ∥ T3, then T2**. The `T1 → T2`
  dependency was load-bearing and not a preference: the ten floor-absent tests cannot be shown red, so
  injection is their only evidence route, and the defect must go in `DungeonMethods.cs`, which **T2 owns
  and T1 does not**.
- **Suite baseline for 6b-xv: 980 passed** — `DungeonMaster.Tests` **855**,
  `DungeonMaster.App.UnitTests` **23**, `DungeonMaster.App.Tests` **102** — 0 failed, 0 skipped,
  measured in the **main checkout** after the merge. Up from 975 at `20b0b09`; the difference is T1's
  five new `[Fact]` methods.
- **To run the app for an observation pass:**
  `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`,
  then launch
  `src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`.
  **Check the binary is newer than every source file before observing.** **The app opens the user's REAL
  database** — say so before asking for an observation pass.
- **This session's model tiers:** architect, implementers and reviewers on `claude-opus-5`; `dir-git` on
  `claude-haiku-4-5`. There is still no `.dir-sdlc/config.json`; those are the documented defaults, and
  each composed discipline is its bundled base alone (43 / 35 / 36 / 38 lines).
- **There is no composed discipline for the `architecture` domain.** T3 had 17 criteria and encoded none
  from a discipline, because none exists for it. Its conventions came from the register itself.
- **Gate 3 has no slot for orchestrator-authored repo content.** 6b-xiv used the shape 6b-v through
  6b-xiii used: merge (`e5d1398`), then `.dir-sdlc/`, then this handoff as its own commit.
- **Do the `.dir-sdlc/` appends BEFORE dispatching the commit that stages them.** 6b-xiv did.
- **Metrics for 6b-xiv:** `userInteractions` **4**, `escalationsRaised` **5**, `reviewRounds` **0**,
  `redispatches` **0**. The four are Gate 1, Gate 2, Gate 3 and the push question. **`reviewRounds` 0
  against T3's `revisionRounds` 2 does not sum, and that is the rule working**: both of T3's rounds were
  `amendment-caused`, which charges neither counter. **Say so in the report rather than reconciling it** —
  a session that quietly recomputed one of them would hide the two orchestrator errors that caused them.
- **5 escalations, 2 amendments.** E1 and E3 were settled by the orchestrator at rung 1 with no user
  stop; E2 at Gate 2; E4 and E5 produced the two amendments. **Both amendments repaired an orchestrator
  error, and the second repaired the first's repair.**
- **38 acceptance criteria, 38 met.** Three tasks, five review dispatches, zero blocking findings that
  were the implementers' fault.
