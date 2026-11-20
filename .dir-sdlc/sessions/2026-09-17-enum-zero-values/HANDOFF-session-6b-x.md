# Handoff to session 6b-x

Written at the close of 6b-ix, 2026-09-17. Self-contained: everything 6b-x needs is here or at a path named
here. Nothing in it depends on the conversation that produced it.

**Every number below was re-derived from the repository immediately before writing, not carried from the
session that produced it.** Section 6 gives you the commands to re-run the survey rather than trust the counts.

That discipline has now corrected its inherited record **five sessions running**: 6b-iv found four of five
inherited entries had changed disposition, 6b-v found its single entry's prose false, 6b-vi found its own
orchestrator's measurement overstated, 6b-vii had its measurement corrected twice, 6b-viii had its measurement
corrected by the first agent it dispatched — and **6b-ix corrected both an inherited count and the stated
rationale of a user decision.** It also corrected *its own* first measurement before any agent saw it.

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`8c2ac20`**, **pushed**; local and `origin` at **0/0 parity**, working tree
  clean. That tip is two commits past this handoff's own (`63a15c5`): the push question was asked separately
  from Gate 3 and answered yes, and `8c2ac20` corrects this document's `dir-git` section — see section 4.
- **`main` is untouched** at `45495c1`, local and remote identical. Nothing in 6b-ix went near it.
- One worktree — the main checkout. `C:/dm-wt/6b-ix` was removed at session end. 6b-ix's branch
  `dir/2026-09-17-enum-zero-values` is retained locally at `476b510` and **deliberately not pushed**. There are
  now **thirteen** such branches and `git ls-remote --heads origin "dir/*"` returns **nothing**.
- Full solution suite: **960 passed, 0 failed** — `DungeonMaster.Tests` **840**, `DungeonMaster.App.Tests`
  **97**, `DungeonMaster.App.UnitTests` **23**. Build **0 Warning(s), 0 Error(s)** across all three App TFMs.
  Verified **in the main checkout after the merge**, not only in the session worktree; both runs agreed
  exactly. The total is the 953 baseline plus 6b-ix's 7 new tests.
- 6b-ix merged as **`02178be`**, a `--no-ff` merge with two parents and `HEAD^2 = 476b510`, matching the shape
  of the ten session merges before it. Three follow-on commits on the base branch: **`147fa13`** (the closure
  annotations), **`911527f`** (session artifacts and the backlog append), and this handoff's commit.
- 6b-ix artifacts: `.dir-sdlc/sessions/2026-09-17-enum-zero-values/` — `spec.md`, **`measurement.md`**,
  `manifest.json`, **`coverage.md`**, `contracts/enum-zero-values.md`, `disciplines/` (4), `reviews/T1`–`T3`,
  `log.md`, `state.json`.

### Index arithmetic — read this before citing any line number

| File | Lines at `80beda9` | Now | Offset rule |
|---|---|---|---|
| `docs/audit/00-master-index.md` | 667 | 687 | +3 after `:364`, a further +17 after `:376`; **net +20** for a citation above `:376`. A citation at or below `:361` is unaffected |
| `docs/audit/01-domain-application.md` | 1360 | 1604 | +19 after `:756`, a further +231 after `:1330`; **net +244** above `:1330`. At or below `:750` unaffected |
| `docs/audit/03-app-shell-and-views.md` | 1346 | 1410 | +64 after `:1317`; at or below `:1311` unaffected |
| `.dir-sdlc/backlog.md` | 367 | 401 | +34, all appended at the end |

Counted with `b.count(b'\n')` in binary, not `content.split('\n')` — that method is off by one on every
newline-terminated file and is a standing backlog item.

**Do not apply the table and consider the citation verified.** It tells you where a line *moved*, not whether
the claim at that line is still true. And note that `00-master-index.md` has now moved in two of the last three
sessions after being stable for one, so "it did not move last time" is not evidence about this time.

---

## 2. What is open

179 entry rows: **126 closed, 50 open, 3 part done.** Zero rows return `UNCLASSIFIED`.

Open by layer — `CORE` **16**, `UI` 10, `UIKIT` 9, `TEST` 8, `INFRA` 6, plus 1 spanning `UI, XCUT`.
By severity — **S1 0, S2 0**, S3 **8**, S4 32, S5 10.

> **There are still no open S1 or S2 entries.** 6b-ix filed `MASTER-177` as S2 and closed it in the same
> session, so the property 6b-vi established survives. The severity argument remains spent; 6b-vii, 6b-viii and
> 6b-ix all picked on coherence and layer concentration instead, and it worked all three times.

The three `part done` entries, which **must not be marked done**: `MASTER-094` (`:282`), `MASTER-098` (`:286`),
`MASTER-108` (`:296`). Both inherited warnings still stand — `MASTER-098` was *measured* by 6b-vi and not
closed (`artifacts/MASTER-098-verification.md` at `553493f` holds the breakdown; its `locations:` citations at
`04-app-controls-and-theme.md:514-515` are both stale, the survivor is `:533`), and **`MASTER-108`'s prose
carries a wrong number** — it says `DungeonDetailPage.xaml.cs` is "2,226 lines"; it was 2488 at `7483b57` and
is larger now. Re-measure before citing.

**The eight open S3s:**

| Line | Entry | Layer | Title (truncated) |
|---|---|---|---|
| `:232` | `MASTER-044` | CORE | `Result<T>.Value` is `T?` on an unconstrained `T` |
| `:235` | `MASTER-047` | CORE | Five Domain overloads have no production caller |
| `:239` | `MASTER-051` | UI | Entry and Editor handler mappers repeat eight WinUI resets |
| `:269` | `MASTER-081` | CORE | The Domain has no policy layer |
| `:295` | `MASTER-107` | CORE | `Dungeon` is a 453-line delegation facade |
| `:360` | `MASTER-172` | TEST | No test asserts `Dispatcher` propagates its `CancellationToken` |
| `:361` | `MASTER-173` | TEST | `Dispatcher`'s missing-handler failure mode is untested |
| `:367` | `MASTER-179` | UI | `DrawSessionResult`'s two consumer sites still do not reject the zero member |

---

## 3. The recommended session

This is a recommendation, not a decision — 6b-x's Gate 1 owns the scope.

**`MASTER-172` + `MASTER-173` — the two `Dispatcher` test gaps.** They are now the *only* remaining cheap
coherent pair, and they have been the ranked alternative in two consecutive handoffs without being taken. Both
are genuinely test gaps rather than missing production behaviour, so no design decision is needed and Gate 1 is
short. Filed by 6b-iii as dispatcher residuals.

### The argument against it, at full strength

**Two small test additions is the least ambitious session available, and the register's cheap work is now
exhausted.** After this pair there is no small coherent item left: every remaining S3 is either structural
(`MASTER-107`, `MASTER-081`), a design question (`MASTER-044`), a UI repetition sweep (`MASTER-051`), or the
remainder of an entry this session deliberately left open (`MASTER-179`). Taking the cheap pair now means the
*next* session has no gentle option at all. There is a real argument for spending a session on something
structural while the register still has easy work banked against a hard session going badly.

**And nobody has verified the two entries are still accurate.** They were filed by 6b-iii and neither has been
re-measured since. Five consecutive sessions have found an inherited claim wrong. Re-measure before decomposing
— that is not boilerplate here, it is the single most reliable finding this project produces.

### The alternatives, ranked

1. **`MASTER-179` — finish what 6b-ix started.** The two equality sites at `DungeonDetailPage.xaml.cs:1473`
   and `:1489` still carry no explicit rejection of `DrawSessionResult.None`. 6b-ix deliberately did not take
   this, and the user ratified that at Gate 2: leaving them unchanged is what makes a leaked default inert. The
   argument for taking it now is that "inert" and "detected" are different properties, and the entry is open on
   exactly that gap. **It puts the session on the Windows TFM**, which 6b-ix avoided entirely.
2. **`MASTER-044` — `Result<T>.Value` is `T?` on an unconstrained `T`.** CORE, `api-design`, the same family
   as the entries 6b-viii and 6b-ix closed, and probably the same latent shape. Still not measured by anyone.
3. **`MASTER-107` — `Dungeon` is a 453-line delegation facade.** Large, structural, wants its own session.
   **Re-measure the line count first**; `MASTER-108`'s sibling claim is already known wrong.
4. **The `PathSegmentKind` migration, from `CORE-035`.** Real, filed, and **the only item in the register that
   requires a data migration on persisted rows**. Do not take it as a side-quest; it wants a session scoped as a
   migration, with a plan for existing `Areas.Outline` JSON.

---

## 4. The inherited discipline — this is the part that matters

**Re-measure every entry against a fixed commit before decomposing, and decompose from the measurement.**

### What 6b-ix produced, in order of what it saved

**1. A user decision's stated rationale can be false, and measuring it is how you find out.** 6b-viii's user
ruling chose "add a `None` member" over "renumber from 1" partly because *"add a `None` generalises
mechanically across fourteen enums and renumber-from-1 does not."* **It does not generalise.**
`AreaConfiguration.cs:16` builds `new(JsonSerializerDefaults.General)`, `src/` contains **zero**
`JsonStringEnumConverter` or `JsonConverter` declarations, so `System.Text.Json` writes `PathSegmentKind` as
its **ordinal** into the `Areas.Outline` TEXT column — and `PathSegmentJsonRoundTripTests.cs:26-28` is an
*existing* test pinning `"Kind": 0` as `Line`. Inserting `None = 0` there reinterprets every persisted outline.
That is a **data migration**, not a renumbering. Found by the architect at decomposition, verified independently
by the orchestrator, and recorded as an amendment beside `CORE-017`'s 6b-viii text rather than as a rewrite of
it. **The premise being wrong did not make the decision wrong** — `None` remains right for the two enums this
session changed — but a generalisation nobody measured had entered the record as a reason.

**2. A criterion can become reachable-without-the-work only *after* a sibling criterion lands.** T1's A6
required `ToResult(default(MutationOutcome))` to throw with `ParamName` and `ActualValue`. Genuinely not
satisfied at the base tree — but the moment A1 added `None`, the **pre-existing** `_ =>` arm rejected it
correctly, so A6 passed before `ToResult` was touched. **The Gate 2 vacuity check cannot catch this**: it tests
each criterion against `startRef`, and at `startRef` A6 is genuinely unsatisfied. The implementer found it and
reported it rather than letting the criterion look cleanly earned. **Add "is this criterion still discriminating
once its siblings land?" to the Gate 2 check.**

**3. Strengthening a specified injection was the best finding for the third session running.** T1's reviewer
ran a third injection nobody asked for — deleting the dedicated `MutationOutcome.None` switch arm outright — and
all 11 tests stayed green. The arm is **behaviourally decorative**, load-bearing only for the contract's
separate-removability requirement, and no test protects it. Both previous sessions produced their sharpest
finding the same way. **Tell every reviewer where to be sceptical, then let it exceed the brief.**

**4. Adding an injection at Gate 2 closed a real hole.** The draft manifest asserted a universal over
`PlaceVertex`'s *three* return paths while specifying *two* injections. The user added injection E at Gate 2.
All three then reddened a **different** pre-existing control while the other two stayed green — without E, the
`Closed` path would have shipped asserted but never demonstrated. **A universal is a count without a digit in
it, and the consistency check is where to catch it.**

**5. The orchestrator's own first census was confidently wrong.** It split enum bodies on `,` before stripping
doc comments, corrupting every enum whose `<summary>` contains a comma — it reported `DrawState`'s zero member
as `Drawing` (it is `Idle`). Reproducible, fast, false. Caught by validating the predicate against a must-flag
and a must-not-flag case **before** the count was used. Recorded in `measurement.md` §1 rather than replaced
quietly.

**6. Three independent census instruments agreed and all three shared one unstated assumption.** The
orchestrator's Python walk, T3's re-implementation and the reviewer's grep all returned 17/14. Each would have
been wrong identically if any enum carried an explicit `= n`, because all three read "first member" as "zero
member". **Only the third reviewer checked.** Agreement between instruments of the same design is weaker
evidence than it looks.

**7. The entry that corrects an uncited count contained uncited counts of its own.** T3's first draft of
`CORE-035` said "the other **seven** class C entries" (the table has nine; eight excluding `ErrorCategory`) and
"Class B is **four** enums" (three, after the reclassification the same entry performs, which it concedes two
lines later). That was the session's one blocking finding. The revision then found **four more exposures the
reviewer had missed**, all *universals* rather than digits — "no guard, analyser **or test** observed it", "the
**one case in the fourteen**", "**no analyser** will flag anything", "**zero warnings**". **A wrong number is
discoverable by recounting; an unmeasured universal reads as measured forever.**

### The environment findings

**`sed -i` is DESTRUCTIVE here, and this is no longer a tension — it is a measurement.** The T2 reviewer ran
`sed -i` on one file and it **silently rewrote all 134 line endings from CRLF to LF across the whole file**,
breaking the build with `IDE0055` on every line. `sed -i` does not edit in place in this environment; it
rewrites the file. Every handoff before this one described the harness's "edit via Bash `sed`" instruction as a
*conflict* with the CRLF requirement. **6b-ix suspended that instruction explicitly in every implementer
dispatch and no task hit it; the reviewer, dispatched without that suspension, did. Suspend it in review
dispatches too.**

**`Copy-Item` preserves `LastWriteTime`**, so a byte-correct restore can be measured against a **stale DLL**.
T1's reviewer restored an injected file by copy, MSBuild did not rebuild, and the next run reported two
failures that were not real. **A restore verified by blob hash can still produce a false test result.** T2's
implementer, told this in its dispatch, reverted by re-editing content instead and hit nothing.

**The standing list of instruments that lie about line endings is unchanged**, and all of them still lie:
`git show <rev>:<path> > file` and `perl -pe` strip CR; `sed -n 'Np' file | cat -A` shows no `^M` on a genuinely
CRLF file; `grep -c $'\r$'` returns a confident `0` on a correct file. **The Write tool emits LF** — it did so
on both new `.cs` files this session and on two artifacts. **Count line endings in binary. Nothing else in this
environment is trustworthy about them.**

**`CA2208` is escalated to an error in this solution.** A wrong-`ParamName` injection **fails to compile**, so
the `ParamName` half of a rejection test is guarded by the build, not by the test. Any future criterion
asserting `ParamName` should know the test is not the control it appears to be.

### `dir-git` — commit messages clean for the second session running; one fabricated hash in a report

**Eight dispatches, every repository write correct, and every commit message body factually clean.** What
changed is not the agent: **every dispatch supplied the message's factual content verbatim and forbade
additions**, and each body was **read back against `git show --stat` before the commit was treated as final**.
The merge dispatch again carried an explicit prohibition on claiming to carry the `.dir-sdlc` artifacts, because
`e22f37a` falsely claimed exactly that. **Keep doing both.** The failure mode is in its *reports and prose*, not
its writes.

**And it produced one wrong report anyway, on the ninth dispatch.** Its `cleanup` report printed the base branch's SHA as `63a15c5579d70410fe7b224bd66e2bb3e7eb91d78` — an extra digit inserted into `63a15c579d70410fe7b224bd66e2bb3e7eb91d78`. **The write was correct**: the worktree is genuinely removed, the branch genuinely retained, the tree genuinely clean. Only the hash in the prose was invented, and it was caught because the orchestrator re-ran `git rev-parse` rather than reading the report. **This is the fifth consecutive session recording the same split — correct writes, unreliable reports — and the first to catch it outside a commit message.** Read back every hash it prints, not only the ones in commit bodies.

### The structural limits, unchanged

- **Tasks cannot invoke git.** Entry closures cite commit hashes, so they are orchestrator work after the merge.
  6b-ix encoded this as criterion C10 and made it falsifiable by inspection — **and the task validated that
  predicate against a positive control**, confirming the same regex finds row 176's six real hashes.
- **The closure commit is not optional bookkeeping.** Because C10 forbids a task writing a status, two of the
  three new rows returned `UNCLASSIFIED` from the project's own survey until the post-merge closure commit set
  them. A session dying in that window leaves rows invisible to every future survey.
- **No criterion may constrain what a task *reports*** — only what the repository *contains*.
- **`git diff` cannot see an untracked file.** Both new test files were invisible to their reviewers' diffs and
  had to be named explicitly in the dispatch. `git add -N` is **not** the fix.
- **Reviewers inject into files they do not own**, and both verified restores **by blob hash**. The orchestrator
  re-verified every restore independently before committing.
- **Re-supply the full criterion text on every re-review dispatch.** T3's final reviewer was not given it,
  identified criteria by content, and its `C8`/`C9`/`C10` labels drifted onto that round's three changes. Every
  verdict was substantively right; the labels were not.

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **401 lines** by both `wc -l` and `b.count(b'\n')`. 6b-ix appended **34 lines /
15 items** in a new section at the end.

> **The 8 lone CR characters are still there and are still not from this session.** Measured before and after
> 6b-ix's append: 8 both times, by `b.count(b'\r') - b.count(b'\r\n')` in binary. They date to 6b-iv. `wc -l`
> and `b.count(b'\n')` both give **401**; Python's text-mode iteration gives **409**, because universal-newline
> handling splits on a lone CR too. **State which method you used.** 6b-ix deliberately did not repair them —
> outside the approved scope, for the fourth session running.

6b-ix's fifteen items cover: the decorative `None` switch arm; `CA2208` guarding `ParamName` instead of the
tests; the two distinct twelves in `CORE-035`; `sed -i` measured as destructive; the `Copy-Item` stale-DLL trap;
the corrected `14 of 16` census; the orchestrator's own discarded first census; three instruments sharing one
blind spot; the sibling-criterion vacuity gap; the Write tool's LF twice; a reviewer catching its own wrong
predicate; the criterion-label drift; the index status-cell convention; **the Gate 3 contract-operation search
marking two reachable operations `not called`**; and the missing cardinality guard on `DrawSessionResult`.

**Still unscheduled from earlier sessions:** the discipline-versus-contract sleep conflict and the clock seam no
session has taken; the `ObjectKindInfo` `None` absorption; the drop-placeholder indicator design question;
`AreaSidebarItem`'s two `PointerGestureRecognizer`s; the truncated floor name with no recovery affordance; the
Appium DIU-versus-pixel ratchets and the clearance ratchet nobody has run; the EF instrument that cannot see a
consistent namespace rename; the three 6b-iii items on `docs/audit/05-tests.md`; the `with`-expression gap on
`PathSegment`; the bare `else` in `ValidateControlPoints`; and the count-only assertion in `AreaOutlineTests`.

**A standing environment fact.** `IDE0055` is **not** set to `error` in `.editorconfig` — `:64` reads
`severity = warning`. It still fails the build, because `Directory.Build.props:5` sets
`TreatWarningsAsErrors=true` with `EnforceCodeStyleInBuild=true` at `:16`. `AnalysisMode` is `Recommended` at
`Directory.Build.props:15`, and **`CA1008` does not fire under it** — measured by build probe in 6b-viii,
re-cited in `CORE-035`. **`CA2208` does fire, as an error** — measured in 6b-ix.

**`dm-conventions` still teaches the pattern 6b-v removed** — `backend/dotnet-format.md:567` and `:588`. It is a
separate repository and was deliberately not modified. 6b-viii searched it for enum-design guidance and found
none, which is why `CORE-017` was the only standing authority.

**`.dir-sdlc/log.md` does not exist and never has**, across all 18 sessions. The dir skill refers to a
project-level log; this project keeps session-level logs only. 6b-ix did not create it. Do not create it without
deciding to.

---

## 6. Commands to re-derive section 2 yourself

Unchanged, and they still classify every row — including 6b-ix's three new ones. Re-run rather than trusting
section 2.

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

**The column index is `[10]`**, because splitting a row on `|` yields a leading and a trailing empty cell. An
`awk -F'|'` doing the same job uses `$11`. Writing the 6b-v handoff produced a wrong survey from exactly that
off-by-one.

**If any row prints `UNCLASSIFIED`, read the cell before deciding what it means.** That is the whole point of
the bucket. In 6b-vi it caught a malformed closure marker; in 6b-ix it caught two rows that were correctly
awaiting their closure commit. **Re-run the survey after you edit the index, and again after the closure
commit** — 6b-ix did both, and the second run is the one that proved the rows readable.

---

## 7. Mechanics

- **Push is done, and 6b-ix is the first session since 6b-vii to end at parity.** `chore/repo-audit` and
  `origin/chore/repo-audit` are both at `8c2ac20`; `main` is untouched at `45495c1` on both sides; and
  `git ls-remote --heads origin "dir/*"` still returns **nothing** against **thirteen** local session
  branches. The push question was asked separately from Gate 3, as the framework requires. **Re-derive this
  yourself** — an earlier draft of this handoff said "not pushed", which was true when written and false
  twenty minutes later.
- **Worktree root must be short.** Use `C:/dm-wt/6b-x`. This is a MAUI/WinUI solution and a long path breaks
  the WinUI XAML compiler with `MAX_PATH` errors; the scratchpad path is too long. `C:/dm-wt` exists and is
  empty again.
- **Tracked files are CRLF and a formatting slip fails the build** — via `TreatWarningsAsErrors`, not an
  `IDE0055` severity of `error`. **Suspend the harness's `sed` instruction in every dispatch, implementers and
  reviewers alike**, and say why: `sed -i` rewrites the whole file's line endings here. Verify new files in
  binary; the `Write` tool emits LF.
- **Close any running `DungeonMaster.App` before building the Windows TFM**, or `MSB3026`/`MSB3027`. 6b-ix
  avoided the Windows TFM entirely by scoping T2 to `App.Core`; a session touching `MASTER-179` or `MASTER-051`
  will not have that luxury.
- **Verify in the main checkout after merge**, not only in the worktree. 6b-ix did; both runs agreed.
- **Tasks share one worktree and one `obj/`/`bin/`.** Two tasks that both build must be serialised. 6b-ix ran a
  fully serial DAG and declared `dependsOn` as build serialisation rather than claiming false parallelism.
- **To run the app for an observation pass:**
  `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`, then
  launch `src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`.
  **In Git Bash, give the path with forward slashes or quote it** — backslashes are eaten.
- **The Appium harness prerequisites are installed** but the server is not running:
  `appium --address 127.0.0.1 --port 4723`. `src/DungeonMaster.App.UiTests` is deliberately **outside**
  `src/DungeonMaster.sln`; do not add it. It has not been compiled in any session since 6b-v.
- **This session's model tiers:** implementers, reviewer and architect on `claude-opus-5`; `dir-git` on
  `claude-haiku-4-5`. There is still no `.dir-sdlc/config.json`; those are the documented defaults.
- **Gate 3 has no slot for orchestrator-authored repo content.** 6b-ix used the shape 6b-v through 6b-viii
  used, with one addition: merge (`02178be`), then `docs/audit/` closures (`147fa13`), then `.dir-sdlc/`
  (`911527f`), then the handoff as its own commit.
- **Do the `.dir-sdlc/` appends BEFORE dispatching the commit that stages them.** 6b-vii got this wrong and it
  resolved benignly; 6b-viii and 6b-ix both did it in the right order.
- **Do not edit a tracked file in the main checkout while `dir-git` is running a merge there.** 6b-ix held all
  its index and backlog edits until the merge returned.
