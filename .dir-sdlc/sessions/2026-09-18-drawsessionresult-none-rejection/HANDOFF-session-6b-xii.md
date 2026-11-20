# Handoff to session 6b-xii

Written at the close of 6b-xi, 2026-09-18. Self-contained: everything 6b-xii needs is here or at a
path named here. Nothing in it depends on the conversation that produced it.

**Every number below was re-derived from the repository immediately before writing, not carried from
the session that produced it.** Section 6 gives you the commands to re-run the survey rather than
trust the counts.

That discipline has now corrected its inherited record in five of the last seven sessions. **6b-xi
is, like 6b-x, one of the exceptions -- and for the second time the exception is the finding:** the
`MASTER-179` entry was re-measured before decomposition and **still held**, its two cited line
numbers unmoved. Re-measuring is not only how errors are caught; it is the only way to know an entry
is still worth a session.

What 6b-xi corrected instead was **three claims in another document that its own fix falsified**,
and **one of its own frozen-criteria-to-be** before the freeze. Both are described below, because
both are reproducible failure modes rather than incidents.

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`8339c72`** plus this handoff's commit. **Re-derive the push state
  yourself** with `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit`; the
  6b-ix handoff had to be corrected twice for exactly this, because it was true when written and
  false twenty minutes later.
- **`main` is untouched** at `45495c1`, local and remote identical. Nothing in 6b-xi went near it.
- 6b-xi merged as **`6d45515`**, a `--no-ff` merge with two parents, `HEAD^1 = 061b28b` and
  `HEAD^2 = 4df98c2`, matching the shape of the twelve session merges before it. Follow-on commits on
  the base branch: **`8339c72`** (the closures), then the `.dir-sdlc/` artifacts, then this handoff.
- Full solution suite: **975 passed, 0 failed** -- `DungeonMaster.Tests` **850**,
  `DungeonMaster.App.Tests` **102**, `DungeonMaster.App.UnitTests` **23**.
  `grep -icE "warning|error"` over the entire captured output returns **0**. All three TFMs appear.
  Verified **in the main checkout after the merge**, at `6d45515`. The total is the 969 baseline --
  itself **measured this session at `061b28b`**, not carried from the 6b-x handoff -- plus 6b-xi's 6
  new facts.
- 6b-xi artifacts: `.dir-sdlc/sessions/2026-09-18-drawsessionresult-none-rejection/` -- `spec.md`,
  **`measurement.md`**, `manifest.json`, **`coverage.md`**, `disciplines/` (4), `reviews/T1.md`,
  `reviews/T2.md`, `log.md`, `state.json`. **`contracts/` is empty and absent from git** -- the
  session declared none, recorded as `"contracts": []` with the reason in `coverage.md` section 2.
  That is not a missing artifact.

### Index arithmetic -- read this before citing any line number

| File | Lines at `061b28b` | Now | Offset rule |
|---|---|---|---|
| `docs/audit/00-master-index.md` | 687 | 687 | **Unchanged.** 6b-xi rewrote one status cell in place; no line was added or removed. Every citation is unaffected |
| `docs/audit/03-app-shell-and-views.md` | 1410 | 1445 | **+2** after `:1334`, a further **+33** after `:1376`; **net +35** for a citation above `:1376`, **+2** between `:1334` and `:1376`, unaffected at or below `:1334` |
| `.dir-sdlc/backlog.md` | 409 | 415 | **+6**, all appended at the end |
| `docs/audit/01-domain-application.md` | 1604 | 1604 | Untouched by 6b-xi |
| `docs/audit/05-tests.md` | 2229 | 2229 | Untouched by 6b-xi |

Counted with `b.count(b'\n')` in binary, not `content.split('\n')` -- that method is off by one on
every newline-terminated file and is a standing backlog item.

**Do not apply the table and consider the citation verified.** It tells you where a line *moved*, not
whether the claim at that line is still true. **6b-xi proved that harder than 6b-x did**: three
claims in `03-app-shell-and-views.md` became false at lines that had **not moved at all** -- see
section 4.

---

## 2. What is open

179 entry rows: **129 closed, 47 open, 3 part done.** Zero rows return `UNCLASSIFIED`.

Open by layer -- `CORE` **16**, `UI` 9, `UIKIT` 9, `INFRA` 6, `TEST` 6, plus 1 spanning `UI, XCUT`.
By severity -- **S1 0, S2 0**, S3 **5**, S4 32, S5 10.

> **There are still no open S1 or S2 entries.** The severity argument remains spent; 6b-vii through
> 6b-xi all picked on coherence and layer concentration instead, and it has now worked five times.

The three `part done` entries, which **must not be marked done**: `MASTER-094` (`:282`),
`MASTER-098` (`:286`), `MASTER-108` (`:296`). Both inherited warnings still stand -- `MASTER-098` was
*measured* by 6b-vi and not closed (`artifacts/MASTER-098-verification.md` at `553493f`; its
`locations:` citations at `04-app-controls-and-theme.md:514-515` are both stale, the survivor is
`:533`), and **`MASTER-108`'s prose carries a wrong number** -- it says
`DungeonDetailPage.xaml.cs` is "2,226 lines". **6b-xi measured that file at 2,594 lines**
(`b.count(b'\n')`, binary) while working in it. The claim was already wrong by 262 lines at
`7483b57`'s 2488, and it is now wrong by 368. **Neither entry was touched by 6b-xi.**

**The five open S3s:**

| Line | Entry | Layer | Title (truncated) |
|---|---|---|---|
| `:232` | `MASTER-044` | CORE | `Result<T>.Value` is `T?` on an unconstrained `T` |
| `:235` | `MASTER-047` | CORE | Five Domain overloads have no production caller |
| `:239` | `MASTER-051` | UI | Entry and Editor handler mappers repeat eight WinUI resets |
| `:269` | `MASTER-081` | CORE | The Domain has no policy layer |
| `:295` | `MASTER-107` | CORE | `Dungeon` is a 453-line delegation facade |

`MASTER-179` is gone from this list -- closed by 6b-xi. **The S3 band fell from 6 to 5 and the UI
layer from 10 to 9.**

---

## 3. The recommended session

This is a recommendation, not a decision -- 6b-xii's Gate 1 owns the scope.

**The cheap work stayed exhausted, exactly as the 6b-xi handoff said.** 6b-xi took the one item whose
scope was already known, and there is no longer any such item. Every remaining S3 is structural
(`MASTER-107`, `MASTER-081`), a design question (`MASTER-044`), a UI repetition sweep (`MASTER-051`),
or a dead-code question (`MASTER-047`). **There is no small item left in the S3 band. Plan for a
session that costs more than the last two.**

**Recommended: `MASTER-044` -- `Result<T>.Value` is `T?` on an unconstrained `T`.** CORE,
`api-design`, the same family 6b-viii, 6b-ix and 6b-xi closed, and probably the same latent shape.
**It has still never been measured by anyone**, so Gate 1 is a measurement pass and the scope is
unknowable until it finishes. It stays on the plain TFM, which is worth real time -- see the argument
against below.

### The argument against it, at full strength

**Its scope is genuinely unknown, and "unmeasured" has bitten this project before.** `MASTER-107`'s
sibling `MASTER-108` carries a line count wrong by 368. An unmeasured entry can be an afternoon or a
week, and Gate 1 cannot tell you which until it has run.

**`Result<T>` is load-bearing across the whole Application layer.** Unlike `DrawSessionResult`, which
had exactly one production consumer, a change to `Result<T>.Value`'s nullability touches every
handler. This is not an XS item wearing a CORE label; it is plausibly a session that produces a
frozen contract and three tasks.

### The alternatives, ranked

1. **`MASTER-047` -- five Domain overloads with no production caller.** Never re-measured. Note that
   `dir-ect`'s own Gate 3 contract-operation search is the tool for this shape, and the backlog
   records that it produced two false `not called` verdicts in 6b-ix -- **re-measure by hand.** 6b-xi
   declared no contracts at all, so that search did not run this session and remains unexercised.
2. **The UI latency spike 6b-xi filed** (`backlog.md`, appended 2026-09-18). **A user-observed
   defect, not a code-reading one**, and the only item in the register or backlog that came from
   running the application. See section 5; it wants a profiler and a Release-build comparison, and
   the user asked that a session decide whether it warrants its own `MASTER` entry.
3. **`MASTER-107` -- `Dungeon` is a 453-line delegation facade.** Large, structural, wants its own
   session. **Re-measure the line count first**; its sibling's claim is known wrong by 368 lines.
4. **The `PathSegmentKind` migration, from `CORE-035`.** Real, filed, and **the only item in the
   register requiring a data migration on persisted rows**. Do not take it as a side-quest; it wants
   a session scoped as a migration, with a plan for existing `Areas.Outline` JSON.

---

## 4. The inherited discipline -- this is the part that matters

**Re-measure every entry against a fixed commit before decomposing, and decompose from the
measurement.**

### What 6b-xi produced, in order of what it saved

**1. The orchestrator's own convention survey was decisive and WRONG on its first scoping, and the
correction is the session's central lesson.** The user declined to pick a rejection shape and asked
the repository's convention to decide, counted rather than asserted. Scoped as *"what do App-layer
catch-all arms do?"*, the survey returned **7 absorbing, 0 throwing** -- an answer that would have
closed `MASTER-179` with **no code change at all**. That answer was false. **Every one of those seven
is a value-producing `switch` expression where C# requires a catch-all**; the target site is a
statement chain that is required to do nothing. Re-scoped to statement-level enum dispatch, the App
layer has four such sites and **not one is an instance of the case under test** -- three ignore
`GestureStatus` values that legitimately occur, and the `NotebookKind` switch is exhaustive across
all three members. Correctly scoped the count is **`n = 1`**, and it throws. **The failure mode is
this project's most familiar one, and this time the orchestrator committed it against itself: a
count extended to a population it was never validated against.** Recorded in `measurement.md`
section 2.1 rather than replaced.

**2. `n = 1` was reported as a weak convention, in every artifact that carries it.** One instance is
not "the repository always does this"; it is "the repository has done this once, in the closest
analogue that exists, and has never done anything else with a never-occurring member." Both halves
are in `measurement.md` section 2.4, in `spec.md`'s Assumptions table, and in the closed index row.
**A count of one that is reported as a rule is how the next false universal gets written.**

**3. A criterion was caught UNSATISFIABLE at Gate 2, before the freeze, by the orchestrator's
consistency check.** B9 required `grep -icE "warning|error"` over a `dotnet build` output to return
`0`. **MSBuild always prints the literal lines `0 Warning(s)` and `0 Error(s)`, so that command
returns 2 on a perfectly clean build.** The standard is correct for `dotnet test` -- it returned 0
over the full 969-test baseline -- and had been carried across command types without being measured.
Reworded before the freeze as an ordinary draft edit, no amendment, and ratified by the user.
**Pre-freeze this cost one edit; post-freeze it would have cost a review round and an escalation.**

**4. The architect found that the obvious implementation DOES NOT COMPILE, and the orchestrator
reproduced it rather than taking it on report.** `CA2208` is an error in this solution and fires when
`nameof()` names a **local** rather than a parameter of the enclosing method. The value at the call
site is a local, so the literal transplant of the `MutationOutcomeExtensions` shape fails the build.
The orchestrator re-ran the probe **with `EnforceCodeStyleInBuild=true`, which the architect's probe
had switched off**, and got `error CA2208` on the inline form and `0 Warning(s) 0 Error(s)` on the
extension-method form. **That constraint is what made the rejection testable**: moving it into a
method taking the value as a parameter is what gave the session criteria B4-B7 at all.

**5. The reviewers exceeded their briefs, for the fifth session running, and both findings were the
best output of their tasks.**
- **T1's reviewer injected an *inserted* enum member when the criteria only specified an *appended*
  one**, and produced the discriminating table: an append reddens the new fact **alone**; an
  insertion reddens it **and** the pre-existing order fact. That establishes the new fact is
  load-bearing and is not the `backlog.md:355` failure mode.
- **T2's reviewer deleted the guard's call site** -- a probe nobody specified, suggested in the
  dispatch -- and measured the residual the session had only argued for: `Failed: 0, Passed: 102`,
  **nothing reddens**. See item 7.

**6. Both implementers raised the same criterion-wording defect rather than reshaping their runs to
satisfy it.** A2 and B7 each say the injected command "builds with exit code 0 and reports exactly
one test failed"; `dotnet test` necessarily exits **1** when a test fails. Both implementers reported
the observed exit code and flagged the discrepancy. Both reviewers ruled the same way -- *"builds
with exit code 0"* scopes to the **build step**, whose purpose in the clause is to prove no analyser
rejects the injected code -- and recorded which reading they applied. **Two tasks, two implementers,
the same defect surfaced twice by the parties best placed to hide it.** If you reuse this injection
wording, fix it: say *"the build step succeeds"* and state the process exit separately.

**7. A residual was converted from an argument into a measurement, and it is the honest cost of this
closure.** `coverage.md` U6 declared at Gate 2 that nothing would protect the guard's *call site*.
At Gate 3 the reviewer measured it: **deleting `.EnsurePlacementOutcome()` from
`DungeonDetailPage.xaml.cs:1471` leaves `App.Tests` green at 102/102 with zero diagnostics, and no
analyser complains either.** Because `None` is unreachable through `PlaceVertex`, **no runtime test
can distinguish a guarded call site from an unguarded one.** Carried to the backlog with the only
remedy that would work -- a static check. **This is a permanent property of closing `MASTER-179`,
not a defect in the decomposition**, and it is recorded in the closed index row so nobody has to
rediscover it.

**8. A definition-of-done item was closed by HUMAN OBSERVATION and never on its proxies.**
`coverage.md` U5 -- that drawing still works -- was named at Gate 2 as unprovable by any automated
check, with B6 and B11 identified as **static proxies** for it. The orchestrator rebuilt the app from
the committed state, **verified the binary was newer than every source file** so no stale build was
observed, and launched it directly with `Start-Process` (**no Visual Studio, no debugger attached**).
The user performed the pass and confirmed all three behaviours **and the absence of the failure
signature** -- a misfiring guard would surface as "Could not complete that map action: ..." via the
enclosing `try`/`catch`. **A static proxy can pass while the real property is broken; this row was
created to prevent exactly that and was closed on observation.**

### The environment findings

**`sed -i` remains destructive here, and the suspension held everywhere it was issued.** 6b-xi
suspended the harness's `sed` instruction explicitly in **every** dispatch -- architect, both
implementers, both reviewers, and all five `dir-git` dispatches -- and no agent hit the problem.
**Keep issuing it to every agent, without exception.** The stakes rose this session: T2's file is
**2,594 lines**, and a `sed -i` on it would have been the worst instance the project has seen.

**The `Write` tool emitted LF on BOTH new `.cs` files, again.** T2 caught both and rewrote them in
binary. That is now four consecutive `.cs` files across three sessions where `Write` produced LF and
only a binary count caught it. **The orchestrator's artifacts were all written through a script and
are uniformly CRLF** -- all ten session artifacts verified `crlf == lf` in binary, an improvement on
6b-x, where six of eleven were LF.

**The orchestrator's own edit script hit the line-ending trap mid-session.** A guarded
search-and-replace over `coverage.md` failed its exact-match assertion because the search text was
`\n`-joined and the file holds `\r\n`. **The guard is what caught it** -- the script refused to write
rather than silently mangling the artifact. **Normalise to LF before matching, and convert back to
CRLF on write.** Every replacement in this session's edit scripts asserts an exact occurrence count
for this reason.

**The standing list of instruments that lie about line endings is unchanged**, and all of them still
lie: `git show <rev>:<path> > file` and `perl -pe` strip CR; `sed -n 'Np' file | cat -A` shows no
`^M` on a genuinely CRLF file; `grep -c $'\r$'` returns a confident `0` on a correct file. **Count
line endings in binary. Nothing else in this environment is trustworthy about them.**

**`CA2208` is a live trap for any `ArgumentOutOfRangeException` whose `paramName` is a local.**
Measured twice this session, independently. It also means **a test asserting `ParamName` is guarded
by the BUILD, not by the test** -- the standing 6b-ix finding, and the reason B4 and B5 assert
`ActualValue` and `Message` instead.

**`AnalysisMode` is `Recommended` (`Directory.Build.props:15`), `TreatWarningsAsErrors=true` (`:5`),
`EnforceCodeStyleInBuild=true` (`:16`), and `IDE0055` is `severity = warning` (`.editorconfig:64`)
and still fails the build through that pair.** All four re-verified this session.
**`CA1008` does not fire** under this mode (6b-viii). **`CA2016` does not fire on
`new CancellationToken(false)`** (6b-x).

### `dir-git` -- five dispatches, four accurate reports, ONE WRONG

**The clean streak ended at two sessions, and the 6b-x handoff's warning was right: one clean session
was never evidence the failure mode had gone.**

Five dispatches -- `init-session`, two `commit-task`, `merge-session`, the closure `commit-task`.
**Every repository write was correct. Every SHA was correct. Every commit message body was stored
byte-identical to what it was handed, with zero forbidden trailers.**

**But the closure commit's report misstated its own stat.** It claimed
`00-master-index.md` had **2 insertions** and `03-app-shell-and-views.md` **41 insertions**. The
truth, from `git show --numstat`, is **1 insertion / 1 deletion** and **38 insertions / 3 deletions**.
**It read `--stat`'s total-lines-changed graph column (`2 +-`, `41 ++---`) as an insertion count.**
The commit is entirely correct; only the report is wrong.

**This is the documented pattern exactly -- its errors are in its REPORTS, not its writes -- and it
was caught only because every number it printed was re-read with `git rev-parse`, `git show
--numstat` and `git log -1 --format=%B` before being treated as final.** Had the orchestrator
transcribed that stat into this handoff, the next session would have inherited two wrong numbers with
a plausible provenance. **Keep re-reading every one. `--numstat` is the reliable form; `--stat`'s
graph column is what misleads.**

### The structural limits, unchanged

- **Tasks cannot invoke git.** Entry closures cite commit hashes, so they are orchestrator work after
  the merge. Declare it at Gate 2 as an uncovered coverage row. **6b-xi also hit the inverse:** B11's
  check was *"checkable from the task diff"* -- a git measurement the owning task could not run. The
  implementer **flagged it rather than asserting a measurement it had no access to**, and the
  orchestrator verified it. **Do not write a criterion whose check requires git.**
- **No criterion may constrain what a task *reports*** -- only what the repository *contains*.
- **`git diff` cannot see an untracked file.** Both of T2's new files were untracked and invisible;
  they were **named explicitly in the reviewer's dispatch**. `git add -N` is **not** the fix and must
  not be used -- it mutates the index that these read-only Gate 3 commands are safe for not touching.
- **A task may own a production file solely to inject into it**, provided a criterion pins the file's
  `sha256` at `startRef`. 6b-xi did this with `MapDrawingSession.cs` (A4) and it worked cleanly --
  the file ended byte-identical and appears in neither commit.
- **Revert an injection by re-editing content, never by restoring a copy.** `Copy-Item` preserves
  `LastWriteTime`, MSBuild does not rebuild, and the next run reports a **stale DLL**.
- **Verify the worktree before every commit when injections have run in it.** 6b-xi ran **four**
  injection rounds across two files. Before each commit the orchestrator re-verified every sha256
  **and the presence of each injection site** -- the guard call, the dedicated `None` arm, the
  pass-through arms returning `result` rather than a constant. A leftover injection committed into a
  production file is the one failure here that is expensive to undo.
- **Re-supply the full criterion text on every re-review dispatch**, or the reviewer's labels drift.

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **415 lines** by both `wc -l` and `b.count(b'\n')`. 6b-xi appended
**6 lines / 3 items** at the end.

> **The 8 lone CR characters are still there and are still not from this session.** Measured before
> and after 6b-xi's append by `b.count(b'\r') - b.count(b'\r\n')` in binary: **8 both times**, and
> the append script fails hard if that number moves. They date to 6b-iv. `wc -l` and
> `b.count(b'\n')` both give **415**; Python's text-mode iteration gives **423**, because
> universal-newline handling splits on a lone CR too. **State which method you used.** 6b-xi
> deliberately did not repair them -- outside the approved scope, for the sixth session running.

**6b-xi's three items:**

1. **`MASTER-179`'s guard is applied at a call site that nothing protects** -- measured at 102/102
   green with the call deleted.
2. **The only durable protection is a static check** -- paired with (1), with the analyser-versus-
   grep decision and the `TreatWarningsAsErrors` consequence named.
3. **SPIKE: the running app stalls 1-2 seconds on ordinary interactions.** **User-observed during the
   U5 pass**, and the only item in the register or backlog that came from running the application.
   Three named instances: closing a polygon, closing the floor-rename popup, saving a rename with
   enter. **Ruled out:** Visual Studio hosting (launched directly via `Start-Process`, no debugger)
   and this session's change (two of the three interactions never touch the drawing path). **Not
   ruled out:** that this was a `Debug` build -- measure a Release build first. **Why it surfaced
   now:** 6b-ix and 6b-x both avoided the Windows TFM, so this was the first time in three sessions
   anyone ran the app. **The user asked that a future session decide whether it warrants its own
   `MASTER` entry.**

**Still unscheduled from earlier sessions:** the discipline-versus-contract sleep conflict and the
clock seam no session has taken; the `ObjectKindInfo` `None` absorption; the drop-placeholder
indicator design question; `AreaSidebarItem`'s two `PointerGestureRecognizer`s; the truncated floor
name with no recovery affordance; the Appium DIU-versus-pixel ratchets and the clearance ratchet
nobody has run; the EF instrument that cannot see a consistent namespace rename; the three 6b-iii
items on `docs/audit/05-tests.md`; the `with`-expression gap on `PathSegment`; the bare `else` in
`ValidateControlPoints`; the count-only assertion in `AreaOutlineTests`; the fifteen items 6b-ix
filed; and 6b-x's four.

**`dm-conventions` still teaches the pattern 6b-v removed** -- `backend/dotnet-format.md:567` and
`:588`. It is a separate repository and was deliberately not modified.

**`.dir-sdlc/log.md` does not exist and never has**, across all 20 sessions. The dir skill refers to
a project-level log; this project keeps session-level logs only. 6b-xi did not create it. Do not
create it without deciding to.

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
cell. An `awk -F'|'` doing the same job uses `$11`. Writing the 6b-v handoff produced a wrong survey
from exactly that off-by-one.

**If any row prints `UNCLASSIFIED`, read the cell before deciding what it means.** **Re-run the
survey after you edit the index, and again after the closure commit** -- 6b-xi did both.

---

## 7. Mechanics

- **Push status must be re-derived, not read here.** The push question is asked separately from
  Gate 3, as the framework requires, and its answer is not knowable to this document.
- **Worktree root must be short.** Use `C:/dm-wt/6b-xii`. This is a MAUI/WinUI solution and a long
  path breaks the WinUI XAML compiler with `MAX_PATH` errors; the scratchpad path is too long.
  `C:/dm-wt` is empty again after 6b-xi's cleanup.
- **Tracked files are CRLF and a formatting slip fails the build.** **Suspend the harness's `sed`
  instruction in every dispatch**, implementers, reviewers and `dir-git` alike, and say why. Verify
  new files in binary; the `Write` tool emits LF.
- **Close any running `DungeonMaster.App` before building the Windows TFM**, or `MSB3026`/`MSB3027`.
  6b-xi confirmed no process was running before it built, and confirmed the app was closed again
  before the post-merge suite.
- **Verify in the main checkout after merge**, not only in the worktree.
- **Tasks share one worktree and one `obj/`/`bin/`.** Two tasks that both build must be serialised.
  6b-xi's `T2 dependsOn T1` was **mandatory, not stylistic**: T1 temporarily appends a member to an
  enum `DungeonMaster.App` compiles against.
- **To run the app for an observation pass:**
  `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`,
  then launch
  `src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`.
  **Check the binary is newer than every source file before observing** -- MSBuild's incremental
  build can leave a stale exe, and this project has already measured one stale-DLL false result.
  **In Git Bash, give the path with forward slashes or quote it** -- backslashes are eaten.
  **The app opens the user's REAL database.** An observation pass that draws anything creates real
  data; say so before asking for one.
- **The Appium harness prerequisites are installed** but the server is not running:
  `appium --address 127.0.0.1 --port 4723`. `src/DungeonMaster.App.UiTests` is deliberately
  **outside** `src/DungeonMaster.sln`; do not add it. It has not been compiled in any session since
  6b-v.
- **This session's model tiers:** architect, implementers and reviewers on `claude-opus-5`;
  `dir-git` on `claude-haiku-4-5`. There is still no `.dir-sdlc/config.json`; those are the
  documented defaults, and each composed discipline is therefore its bundled base alone.
- **Gate 3 has no slot for orchestrator-authored repo content.** 6b-xi used the shape 6b-v through
  6b-x used: merge (`6d45515`), then `docs/audit/` closures (`8339c72`), then `.dir-sdlc/`, then this
  handoff as its own commit.
- **Do the `.dir-sdlc/` appends BEFORE dispatching the commit that stages them.** 6b-xi did.
- **Do not edit a tracked file in the main checkout while `dir-git` is running a merge there.**
  6b-xi held every index and backlog edit until the merge returned.
- **Metrics for 6b-xi:** `userInteractions` **4**, `escalationsRaised` **1**, `reviewRounds` **0**.
  The four are Gate 1, Gate 2, Gate 3, and the push question -- **the fourth is counted here because
  it is asked before the session ends, but it is asked AFTER the commit that stores this number**,
  which is the same ordering 6b-x recorded. `reviewRounds` **0** is earned: both tasks passed their
  first review with zero blocking findings, and `state.json`'s per-task `revisionRounds` sums to it.
  **Two questions preceded Gate 1** -- the scope choice and an attempt to settle the rejection shape
  -- and are **not** counted, because they were asked before the session was invoked. The second was
  answered *"let Gate 1 decide from evidence,"* which is what produced the survey in section 4.
