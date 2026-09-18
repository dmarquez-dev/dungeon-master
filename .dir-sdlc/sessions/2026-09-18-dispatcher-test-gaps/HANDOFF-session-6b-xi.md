# Handoff to session 6b-xi

Written at the close of 6b-x, 2026-09-18. Self-contained: everything 6b-xi needs is here or at a path
named here. Nothing in it depends on the conversation that produced it.

**Every number below was re-derived from the repository immediately before writing, not carried from the
session that produced it.** Section 6 gives you the commands to re-run the survey rather than trust the
counts.

That discipline has now corrected its inherited record in five of the last six sessions. **6b-x is the
exception, and the exception is itself the finding:** both inherited entries were re-measured before
decomposition and **both still held**, unchanged. Re-measuring is not only a way to catch errors; it is
also the only way to know that an entry is still worth spending a session on. What 6b-x corrected
instead was **its own** artifact — a frozen acceptance criterion that turned out to be false about the
repository — and a stale present-tense clause in `05-tests.md` that no survey would ever have flagged.

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`8f6ffc3`**, **four commits ahead of `origin/chore/repo-audit`, not
  pushed** unless the push question was answered yes after this document was written. **Re-derive this
  yourself** with `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit`; the
  6b-ix handoff had to be corrected twice for exactly this, because it was true when written and false
  twenty minutes later.
- **`main` is untouched** at `45495c1`, local and remote identical. Nothing in 6b-x went near it.
- One worktree — the main checkout. `C:/dm-wt/6b-x` was removed at session end and `C:/dm-wt` is empty.
  6b-x's branch `dir/2026-09-18-dispatcher-test-gaps` is retained locally at `b0fa532` and **deliberately
  not pushed**. There are now **fourteen** such branches and `git ls-remote --heads origin "dir/*"`
  returns **nothing**.
- Full solution suite: **969 passed, 0 failed** — `DungeonMaster.Tests` **849**, `DungeonMaster.App.Tests`
  **97**, `DungeonMaster.App.UnitTests` **23**. `grep -icE "warning|error"` over the entire captured
  output returns **0** — no warning or error line of any kind, not merely a zero in a summary. All three
  App TFMs appear in the output. Verified **in the main checkout after the merge**. The total is the 960
  baseline plus 6b-x's 9 new facts.
- 6b-x merged as **`98326d4`**, a `--no-ff` merge with two parents, `HEAD^1 = 506cbba` and
  `HEAD^2 = b0fa532`, matching the shape of the eleven session merges before it. Three follow-on commits
  on the base branch: **`d434cbd`** (the closures), **`8f6ffc3`** (session artifacts and the backlog
  append), and this handoff's commit.
- 6b-x artifacts: `.dir-sdlc/sessions/2026-09-18-dispatcher-test-gaps/` — `spec.md`, **`measurement.md`**,
  `manifest.json`, **`coverage.md`**, `disciplines/` (4), `reviews/T1.md`, `log.md`, `state.json`.
  **`contracts/` is empty and absent from git** — the session declared none, and git does not track empty
  directories. That is recorded in `manifest.json` as `"contracts": []` and is not a missing artifact.

### Index arithmetic — read this before citing any line number

| File | Lines at `506cbba` | Now | Offset rule |
|---|---|---|---|
| `docs/audit/00-master-index.md` | 687 | 687 | **Unchanged.** 6b-x rewrote two status cells in place; no line was added or removed. Every citation is unaffected |
| `docs/audit/05-tests.md` | 2189 | 2229 | +32 after `:320`, a further +8 after `:1425`; **net +40** for a citation above `:1425`, **+32** between `:320` and `:1425`, unaffected at or below `:316` |
| `.dir-sdlc/backlog.md` | 401 | 409 | +8, all appended at the end |
| `docs/audit/01-domain-application.md` | 1604 | 1604 | Untouched by 6b-x |
| `docs/audit/03-app-shell-and-views.md` | 1410 | 1410 | Untouched by 6b-x |

Counted with `b.count(b'\n')` in binary, not `content.split('\n')` — that method is off by one on every
newline-terminated file and is a standing backlog item.

**Do not apply the table and consider the citation verified.** It tells you where a line *moved*, not
whether the claim at that line is still true. **6b-x proved that directly**: `05-tests.md:1422` had not
moved at all since it was written, and its claim had become false anyway — see section 4.

---

## 2. What is open

179 entry rows: **128 closed, 48 open, 3 part done.** Zero rows return `UNCLASSIFIED`.

Open by layer — `CORE` **16**, `UI` 10, `UIKIT` 9, `INFRA` 6, `TEST` **6**, plus 1 spanning `UI, XCUT`.
By severity — **S1 0, S2 0**, S3 **6**, S4 32, S5 10.

> **There are still no open S1 or S2 entries.** The severity argument remains spent; 6b-vii through 6b-x
> all picked on coherence and layer concentration instead, and it has now worked four times.

The three `part done` entries, which **must not be marked done**: `MASTER-094` (`:282`), `MASTER-098`
(`:286`), `MASTER-108` (`:296`). Both inherited warnings still stand — `MASTER-098` was *measured* by
6b-vi and not closed (`artifacts/MASTER-098-verification.md` at `553493f`; its `locations:` citations at
`04-app-controls-and-theme.md:514-515` are both stale, the survivor is `:533`), and **`MASTER-108`'s
prose carries a wrong number** — it says `DungeonDetailPage.xaml.cs` is "2,226 lines"; it was 2488 at
`7483b57` and is larger now. Re-measure before citing. **Neither was touched by 6b-x.**

**The six open S3s:**

| Line | Entry | Layer | Title (truncated) |
|---|---|---|---|
| `:232` | `MASTER-044` | CORE | `Result<T>.Value` is `T?` on an unconstrained `T` |
| `:235` | `MASTER-047` | CORE | Five Domain overloads have no production caller |
| `:239` | `MASTER-051` | UI | Entry and Editor handler mappers repeat eight WinUI resets |
| `:269` | `MASTER-081` | CORE | The Domain has no policy layer |
| `:295` | `MASTER-107` | CORE | `Dungeon` is a 453-line delegation facade |
| `:367` | `MASTER-179` | UI | `DrawSessionResult`'s two consumer sites still do not reject the zero member |

`MASTER-172` and `MASTER-173` are gone from this list — closed by 6b-x. **The TEST layer fell from 8 open
to 6, and the S3 band from 8 to 6.**

---

## 3. The recommended session

This is a recommendation, not a decision — 6b-xi's Gate 1 owns the scope.

**The cheap work is now genuinely exhausted, and 6b-ix predicted this exactly.** Its handoff warned that
taking `MASTER-172` + `MASTER-173` would leave the next session with no gentle option. That has come true
and should be treated as a known, accepted cost rather than a surprise: every remaining S3 is structural
(`MASTER-107`, `MASTER-081`), a design question (`MASTER-044`), a UI repetition sweep (`MASTER-051`), a
dead-code question (`MASTER-047`), or the remainder of an entry a previous session deliberately left open
(`MASTER-179`).

**Recommended: `MASTER-179` — finish what 6b-ix started.** The two equality sites at
`DungeonDetailPage.xaml.cs:1473` and `:1489` still carry no explicit rejection of `DrawSessionResult.None`.
6b-ix deliberately did not take this and the user ratified that at Gate 2: leaving them unchanged is what
makes a leaked default *inert*. The argument for taking it now is that **"inert" and "detected" are
different properties**, and the entry is open on exactly that gap. It is the smallest remaining item and
the only one whose scope is already known.

### The argument against it, at full strength

**It puts the session on the Windows TFM**, which 6b-ix and 6b-x both avoided entirely. That means
`MSB3026`/`MSB3027` if the app is running, a slower build, and — if the change is to be *proven* rather
than merely asserted — an observation pass in the running application, which no automated check replaces.
Both recent sessions were fast partly because they stayed on the plain TFM, and that advantage is spent
the moment this is taken.

**And it is a two-site change with a design question attached.** Rejecting `None` at a consumer site
means deciding what the consumer does instead — throw, log, no-op — and that is a user decision at Gate 1,
not a mechanical edit. Do not let its `XS` sizing suggest otherwise.

### The alternatives, ranked

1. **`MASTER-044` — `Result<T>.Value` is `T?` on an unconstrained `T`.** CORE, `api-design`, the same
   family 6b-viii and 6b-ix closed and probably the same latent shape. **Still not measured by anyone**,
   so Gate 1 is longer and the scope is unknowable until it is.
2. **`MASTER-047` — five Domain overloads with no production caller.** Never re-measured. Note that
   `dir-ect`'s own Gate 3 contract-operation search is the tool for exactly this shape, and the backlog
   already records that it produced two false `not called` verdicts in 6b-ix — **re-measure by hand.**
3. **`MASTER-107` — `Dungeon` is a 453-line delegation facade.** Large, structural, wants its own
   session. **Re-measure the line count first**; its sibling `MASTER-108`'s claim is already known wrong.
4. **The `PathSegmentKind` migration, from `CORE-035`.** Real, filed, and **the only item in the register
   requiring a data migration on persisted rows**. Do not take it as a side-quest; it wants a session
   scoped as a migration, with a plan for existing `Areas.Outline` JSON.

---

## 4. The inherited discipline — this is the part that matters

**Re-measure every entry against a fixed commit before decomposing, and decompose from the measurement.**

### What 6b-x produced, in order of what it saved

**1. A frozen acceptance criterion was false about the repository, and the census is what proved it.**
Criterion A15 required every `[Fact]` *and every declared type* in the new files to be immediately
preceded by a doc comment ending `/// </summary>`, "matching the convention every one of the six `[Fact]`
in `DispatcherTests.cs` already follows". The reviewer returned it `not met` and found **one**
counterexample. Rather than resolve an ambiguity on one instance, the orchestrator censused the
population: **20 type declarations across 13 files are immediately preceded by `</typeparam>` or
`</param>`, 16 of them pre-existing — including `DispatcherTests.cs:231-243`, the very declaration A15
cited as its authority.** The criterion was not ambiguous; it was **wrong**. Its own falsifiability
command, scoped to `[Fact]` lines, had encoded the true convention all along. Escalation E1, amendment
A1. **The failure mode is this project's most familiar one: a universal is a count without a digit in
it.** The architect validated "every one of the six `[Fact]`" — true — and then extended the requirement
to a second population it had never counted.

**2. The reviewer's best finding was an injection nobody specified, for the third session running.** The
three specified rounds each redden three facts at once, which **cannot distinguish three path-specific
facts from three facts sharing one mechanism.** The reviewer noticed and ran a **single-site** injection
at `Dispatcher.cs:172` alone: exactly one fact reddened. Under that same defect, all six pre-existing
`DispatcherTests` facts stayed green — which confirmed the gap `MASTER-172` describes by measurement
rather than by a reading of the file. **Tell every reviewer where to be sceptical, then let it exceed the
brief.** It has now paid three sessions in a row.

**3. A `judgment`-rung assumption in a frozen criterion was settled by a 90-second probe.** The architect
recorded that it *believed* `GetRequiredService`'s message contains the rendered service type, and stated
that if it did not, three of sixteen criteria would be **unsatisfiable as written**. Rather than dispatch
on a belief, the orchestrator wrote a throwaway file-based app in scratch space and ran it — first
against the 9.x line, then **again against 10.0.11**, the version `Directory.Packages.props:24` actually
pins, because the first run would otherwise have been evidence about a version this repository does not
use. `No service for type 'ICommandHandler`1[FakeCommand]' has been registered.` **A rung-4 assumption
that gates three criteria is worth one measurement.**

**4. A stale claim can sit at a line that never moved.** `05-tests.md:1422` read "the dispatcher's token
propagation is unasserted" — true when written, false the moment `b0fa532` landed. **No survey would
flag it**: it is prose in a document body, not an index status cell, and its line number had not changed,
so the index-arithmetic table would have "verified" the citation. It was found only by grepping the
register for every mention of the entries being closed, **after** the closure commit had already been
made — which is why this correction is in the handoff commit rather than in `d434cbd`. **When you close
an entry, grep the whole `docs/audit/` tree for its id before you commit, not just its index row.**

**4b. And this handoff's own arithmetic table was wrong on its first draft.** It stated `05-tests.md` at
**2232** lines with a `+11` offset. The measured values are **2229** and `+8`. The figure had been
written from the shape of the edit rather than from `b.count(b'\n')`, caught by measuring before the
commit, and is recorded here rather than silently corrected — because the table exists to be trusted by
the next session, and a handoff that misreports its own offsets is worse than one that omits them.

**5. The injections were checked against this project's own analyser traps before being specified.**
`new CancellationToken(false)` was chosen over the obvious `default` precisely because **`CA2016` would
have converted that injection into a build failure that reads like a demonstration**; and
`GetService<…>()!` carries the null-forgiving operator `CS8602` would otherwise demand under
`TreatWarningsAsErrors`. Neither round touches `ParamName`, so the `CA2208`-as-error trap 6b-ix measured
was not in play. **An injection that cannot redden its test is worse than none, because it reads as a
demonstration.** Nine inert injections are on record across two earlier sessions.

**6. Two definition-of-done items were declared uncoverable at Gate 2 rather than discovered at Gate 3.**
Tasks cannot invoke git, so an index closure citing a commit hash cannot come from one, and the
post-merge main-checkout suite run happens after every task is done. Both were written into `coverage.md`
as **uncovered, accepted as orchestrator work** — not retired as out of scope, which would have made the
session look decomposed when it was not.

**7. A Gate 2 prediction was refuted and the refutation was kept.** `coverage.md` predicted row 6 `met`;
on the signed text it was **not met**, and it is met only via amendment A1. Recorded as refuted rather
than overwritten by the row that turned out right — which is the entire reason predictions are written
down before the search.

### The environment findings

**`sed -i` remains destructive here, and the suspension held everywhere it was issued.** 6b-x suspended
the harness's `sed` instruction explicitly in **every** dispatch — implementers, reviewer, and all five
`dir-git` dispatches — and no agent hit the problem. 6b-ix's reviewer hit it only because that session
issued the suspension to implementers alone. **Keep issuing it to every agent, without exception.**

**The `Write` and `Edit` tools emit LF, and 6b-x did not escape this — the orchestrator's own artifacts
did.** Six of this session's eleven artifacts are **LF**: `spec.md`, `measurement.md`, `coverage.md`,
`log.md`, `state.json`, `reviews/T1.md`. The four disciplines and `manifest.json` are CRLF because they
were written through a script. **No build impact** — nothing compiles `.dir-sdlc/` — and the repository
is itself inconsistent here: across all committed session artifacts, **250 are LF, 109 CRLF, 21 mixed**,
though 6b-ix was uniformly CRLF. The two new `.cs` files **are** correct CRLF, verified in binary, because
criterion A13 required it. **If you want CRLF artifacts, write them through a script; the tools will not
do it for you.**

**The standing list of instruments that lie about line endings is unchanged**, and all of them still lie:
`git show <rev>:<path> > file` and `perl -pe` strip CR; `sed -n 'Np' file | cat -A` shows no `^M` on a
genuinely CRLF file; `grep -c $'\r$'` returns a confident `0` on a correct file. **Count line endings in
binary. Nothing else in this environment is trustworthy about them.**

**`CA2016` is a live trap for any cancellation-token injection.** It fires on a syntactic `default` or
`CancellationToken.None` argument when a token parameter is in scope, and under `TreatWarningsAsErrors`
that is a compile failure rather than a red test. Use `new CancellationToken(false)`.

### `dir-git` — five dispatches, all five reports accurate

**This is the first session in six to record no `dir-git` reporting error at all.** Five dispatches —
`init-session`, `commit-task`, `merge-session`, two more `commit-task`, and `cleanup` — every repository
write correct, every commit message body factually clean, and **every SHA it printed verified correct**,
including on `cleanup`, the exact operation that invented a 41-character hash in 6b-ix.

What produced that is not a change in the agent. **Every dispatch supplied the message's factual content
verbatim and forbade additions; every dispatch named the specific prior failure it was guarding against;
and every hash and every commit body was read back by the orchestrator with its own `git rev-parse`,
`git show --stat` and `git log -1 --format=%B` before the result was treated as final.** Keep doing all
three. One session of clean reports is not evidence the failure mode is gone.

### The structural limits, unchanged

- **Tasks cannot invoke git.** Entry closures cite commit hashes, so they are orchestrator work after the
  merge. Declare it at Gate 2 as an uncovered coverage row rather than discovering it at Gate 3.
- **The closure commit is not optional bookkeeping.** A session dying between the merge and the closure
  commit leaves rows invisible to every future survey.
- **No criterion may constrain what a task *reports*** — only what the repository *contains*.
- **`git diff` cannot see an untracked file.** Both new test files were invisible to the reviewer's diff
  and had to be named explicitly in its dispatch. `git add -N` is **not** the fix.
- **A task may own a production file solely to inject into it**, provided a criterion pins the file's
  `sha256` at `startRef`. 6b-x did this with `Dispatcher.cs` (A14) and it worked cleanly — the file ended
  byte-identical, and `git diff --stat 506cbba..b0fa532 -- src/DungeonMaster.Application/` is empty.
- **Revert an injection by re-editing content, never by restoring a copy.** `Copy-Item` preserves
  `LastWriteTime`, MSBuild does not rebuild, and the next run reports a **stale DLL**.
- **Re-supply the full criterion text on every re-review dispatch**, or the reviewer's labels drift.

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **409 lines** by both `wc -l` and `b.count(b'\n')`. 6b-x appended **8 lines /
4 items** at the end.

> **The 8 lone CR characters are still there and are still not from this session.** Measured before and
> after 6b-x's append: 8 both times, by `b.count(b'\r') - b.count(b'\r\n')` in binary. They date to
> 6b-iv. `wc -l` and `b.count(b'\n')` both give **409**; Python's text-mode iteration gives **417**,
> because universal-newline handling splits on a lone CR too. **State which method you used.** 6b-x
> deliberately did not repair them — outside the approved scope, for the fifth session running.

6b-x's four items: the handler-token gap under a registered behaviour; amended A15's type clauses having
no cited command; an injection filter selecting 26 tests where the criterion enumerated 15; and the
unnameable `NullReferenceException` behind `MASTER-173`.

**Still unscheduled from earlier sessions:** the discipline-versus-contract sleep conflict and the clock
seam no session has taken; the `ObjectKindInfo` `None` absorption; the drop-placeholder indicator design
question; `AreaSidebarItem`'s two `PointerGestureRecognizer`s; the truncated floor name with no recovery
affordance; the Appium DIU-versus-pixel ratchets and the clearance ratchet nobody has run; the EF
instrument that cannot see a consistent namespace rename; the three 6b-iii items on `docs/audit/05-tests.md`;
the `with`-expression gap on `PathSegment`; the bare `else` in `ValidateControlPoints`; the count-only
assertion in `AreaOutlineTests`; and the fifteen items 6b-ix filed.

**A standing environment fact.** `IDE0055` is **not** set to `error` in `.editorconfig` — `:64` reads
`severity = warning`. It still fails the build, because `Directory.Build.props:5` sets
`TreatWarningsAsErrors=true` with `EnforceCodeStyleInBuild=true` at `:16`. `AnalysisMode` is
`Recommended` at `Directory.Build.props:15`, and **`CA1008` does not fire under it** — measured by build
probe in 6b-viii. **`CA2208` does fire, as an error** — measured in 6b-ix. **`CA2016` does not fire on
`new CancellationToken(false)`** — measured in 6b-x.

**`dm-conventions` still teaches the pattern 6b-v removed** — `backend/dotnet-format.md:567` and `:588`.
It is a separate repository and was deliberately not modified.

**`.dir-sdlc/log.md` does not exist and never has**, across all 19 sessions. The dir skill refers to a
project-level log; this project keeps session-level logs only. 6b-x did not create it. Do not create it
without deciding to.

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
cell. An `awk -F'|'` doing the same job uses `$11`. Writing the 6b-v handoff produced a wrong survey from
exactly that off-by-one.

**If any row prints `UNCLASSIFIED`, read the cell before deciding what it means.** **Re-run the survey
after you edit the index, and again after the closure commit** — 6b-x did both.

---

## 7. Mechanics

- **Push status must be re-derived, not read here.** At the time of writing, `chore/repo-audit` is **4
  ahead / 0 behind** `origin/chore/repo-audit`. The push question is asked separately from Gate 3, as the
  framework requires, and its answer is not knowable to this document.
- **Worktree root must be short.** Use `C:/dm-wt/6b-xi`. This is a MAUI/WinUI solution and a long path
  breaks the WinUI XAML compiler with `MAX_PATH` errors; the scratchpad path is too long. `C:/dm-wt`
  exists and is empty again.
- **Tracked files are CRLF and a formatting slip fails the build** — via `TreatWarningsAsErrors`.
  **Suspend the harness's `sed` instruction in every dispatch**, implementers, reviewers and `dir-git`
  alike, and say why. Verify new files in binary; the `Write` tool emits LF.
- **Close any running `DungeonMaster.App` before building the Windows TFM**, or `MSB3026`/`MSB3027`.
  **A session taking `MASTER-179` or `MASTER-051` will be on that TFM** — neither 6b-ix nor 6b-x was.
- **Verify in the main checkout after merge**, not only in the worktree.
- **Tasks share one worktree and one `obj/`/`bin/`.** Two tasks that both build must be serialised.
  6b-x ran a single task and said so rather than inventing parallelism.
- **To run the app for an observation pass:**
  `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`,
  then launch `src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`.
  **In Git Bash, give the path with forward slashes or quote it** — backslashes are eaten.
- **The Appium harness prerequisites are installed** but the server is not running:
  `appium --address 127.0.0.1 --port 4723`. `src/DungeonMaster.App.UiTests` is deliberately **outside**
  `src/DungeonMaster.sln`; do not add it. It has not been compiled in any session since 6b-v.
- **This session's model tiers:** implementers, reviewer and architect on `claude-opus-5`; `dir-git` on
  `claude-haiku-4-5`. There is still no `.dir-sdlc/config.json`; those are the documented defaults, and
  each composed discipline is therefore its bundled base alone.
- **Gate 3 has no slot for orchestrator-authored repo content.** 6b-x used the shape 6b-v through 6b-ix
  used: merge (`98326d4`), then `docs/audit/` closures (`d434cbd`), then `.dir-sdlc/` (`8f6ffc3`), then
  this handoff as its own commit.
- **Do the `.dir-sdlc/` appends BEFORE dispatching the commit that stages them.** 6b-x did.
- **Do not edit a tracked file in the main checkout while `dir-git` is running a merge there.** 6b-x held
  all its index and backlog edits until the merge returned.
- **Metrics for 6b-x:** `userInteractions` 4, `escalationsRaised` 1, `reviewRounds` 0. The re-review after
  amendment A1 charged neither a revision round nor a review round, per **Precedence after an amendment** —
  the task built correctly against what it was handed and the artifact moved underneath it. **That `0` is
  earned, not incidental**, and `state.json`'s per-task `revisionRounds` sums to it.
