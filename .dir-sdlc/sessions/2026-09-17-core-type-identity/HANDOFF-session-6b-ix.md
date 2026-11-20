# Handoff to session 6b-ix

Written at the close of 6b-viii, 2026-09-17. Self-contained: everything 6b-ix needs is here or at a path
named here. Nothing in it depends on the conversation that produced it.

**Every number below was re-derived from the repository at `c327a7d` immediately before writing, not carried
from the session that produced it.** §6 gives you the commands to re-run the survey rather than trust the
counts. That discipline keeps earning: 6b-iv found four of five inherited entries had changed disposition,
6b-v found its single entry's prose false, 6b-vi found its own orchestrator's measurement overstated, 6b-vii
had its measurement corrected twice, and **6b-viii had its measurement corrected by the first agent it
dispatched — the third consecutive session in which that happened.**

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`c327a7d`**, pushed; local and `origin` at **0/0 parity**, working tree clean.
- **`main` is untouched** at `45495c1`, local and remote identical. Nothing in 6b-viii went near it.
- One worktree — the main checkout. `C:/dm-wt` is empty again. 6b-viii's branch
  `dir/2026-09-17-core-type-identity` is retained locally at `3ef82c8` and **deliberately not pushed**.
  There are now **twelve** such branches and `git ls-remote --heads origin "dir/*"` returns **nothing**.
- Full solution suite: **953 passed, 0 failed** — `DungeonMaster.Tests` **833**, `DungeonMaster.App.Tests`
  **97**, `DungeonMaster.App.UnitTests` **23**. Build **0 Warning(s), 0 Error(s)** across all three App TFMs.
  Verified **in the main checkout after the merge**, not only in the session worktree. The total is the 935
  baseline plus 6b-viii's 18 new tests.
- 6b-viii merged as **`4564210`**, a `--no-ff` merge with two parents and `HEAD^2 = 3ef82c8`, matching the
  shape of the nine session merges before it. Two follow-on commits on the base branch: `d3b4cbd` (the entry
  closures and the CORE-017 amendment) and `c327a7d` (session artifacts and the backlog append).
- 6b-viii artifacts: `.dir-sdlc/sessions/2026-09-17-core-type-identity/` — `spec.md`, **`measurement.md`**,
  `manifest.json`, `coverage.md`, `contracts/core-type-identity.md`, `disciplines/` (4), `reviews/T1`–`T2`,
  `log.md`, `state.json`.

### Index arithmetic — read this before citing any line number

| File | Lines at `9c2bff6` | Now | Offset rule |
|---|---|---|---|
| `docs/audit/00-master-index.md` | 667 | 667 | **none — no line moved.** Both edits were in-place within existing rows |
| `docs/audit/01-domain-application.md` | 1360 | 1368 | +8 after `:744`; a citation **above** `:744` is unaffected |
| `.dir-sdlc/backlog.md` | 345 | 367 | +22, all appended at the end |

Counted with `b.count(b'\n')`, not `content.split('\n')` — that method is off by one on every
newline-terminated file and is a standing backlog item.

**Do not apply the table and consider the citation verified.** It tells you where a line *moved*, not whether
the claim at that line is still true. And `00-master-index.md`'s zero offset does not mean its citations are
stable in general: it grew 590 → 667 during 6b-iv, so a citation written before 6b-iv is still off by up to
77 lines.

---

## 2. What is open

176 entry rows: **125 closed, 48 open, 3 part done.**

Open by layer — `CORE` **15**, `UI` 9, `UIKIT` 9, `TEST` 8, `INFRA` 6, plus 1 spanning `UI, XCUT`.
By severity — **S1 0, S2 0**, S3 **7**, S4 31, S5 10.

> **There are still no open S1 or S2 entries.** 6b-vi closed the last one. The severity argument remains
> spent. 6b-vii picked on coherence and layer concentration; 6b-viii repeated that and it worked again —
> two CORE `api-design` entries, both latent, both closed in one session with no revision round.

The three `part done` entries, which **must not be marked done**: `MASTER-094` (`:282`), `MASTER-098`
(`:286`), `MASTER-108` (`:296`).

**`MASTER-098` was measured by 6b-vi, not closed.** `artifacts/MASTER-098-verification.md` (`553493f`)
re-derives every claim in `UIKIT-011`'s source record: four of five named defects gone, one survivor, and the
record's own `locations:` citations at `04-app-controls-and-theme.md:514-515` are **both stale**. The survivor
is `:533`. Whoever picks it up inherits a breakdown and should not re-derive it from the entry's prose.

**`MASTER-108`'s prose carries a wrong number.** It describes `DungeonDetailPage.xaml.cs` as "2,226 lines".
It was 2488 at `7483b57` and is larger now. Re-measure before citing.

**The seven open S3s:**

| Line | Entry | Layer | Title (truncated) |
|---|---|---|---|
| `:232` | `MASTER-044` | CORE | `Result<T>.Value` is `T?` on an unconstrained `T` |
| `:235` | `MASTER-047` | CORE | Five Domain overloads have no production caller |
| `:239` | `MASTER-051` | UI | Entry and Editor handler mappers repeat eight WinUI resets |
| `:269` | `MASTER-081` | CORE | The Domain has no policy layer |
| `:295` | `MASTER-107` | CORE | `Dungeon` is a 453-line delegation facade |
| `:360` | `MASTER-172` | TEST | No test asserts `Dispatcher` propagates its `CancellationToken` |
| `:361` | `MASTER-173` | TEST | `Dispatcher`'s missing-handler failure mode is untested |

---

## 3. The recommended session

This is a recommendation, not a decision — 6b-ix's Gate 1 owns the scope.

**The enum zero-value sweep, filed from 6b-viii's backlog.** `MASTER-073` closed by adding `ObjectKind.None`
at zero, and the steelman that produced that decision censused **16 enum declarations in production** (17
counting `AsyncVoidUnitKind` in the test project) and reported that **14 carry a concrete member at zero** —
including **`MutationOutcome { Success, NotFound, OutOfRange }`, where `default(MutationOutcome)` is
`Success`**, a CLR default that silently means *the operation worked*. That is the same defect class as
`MASTER-073` and **it is filed nowhere**.

- **Measure first, and re-derive the 14.** The orchestrator verified the census total and `MutationOutcome`
  directly and **did not** individually enumerate all fourteen, so treat that number as the architect's claim,
  not as measured. The command is in the backlog item.
- **`MASTER-073`'s resolution is the template**, which is part of why the user ruled for a `None` member over
  a 1-based enum: "add a `None`" generalises mechanically across fourteen enums and "renumber from 1" does not.
- Not every one of the fourteen is a defect. An enum whose zero member is genuinely its most common default
  satisfies the guideline's second branch. **The measurement's job is to separate those from the
  `MutationOutcome` shape**, and a criterion asserting all fourteen are defects would be false.
- This wants filing as one or more audit entries before or during the session; it has no `MASTER-` id yet.

### The alternatives, ranked

1. **`MASTER-172` + `MASTER-173` — the two `Dispatcher` test gaps.** The only remaining pair that are
   *genuinely* test gaps rather than missing production behaviour, so no design decision is needed and Gate 1
   is short. Filed by 6b-iii as dispatcher residuals. Smallest, safest, least ambitious — and now the
   **only** cheap coherent pair left, since 6b-viii took the other one.
2. **`MASTER-107` — `Dungeon` is a 453-line delegation facade.** Large, structural, wants its own session.
   **Re-measure the line count first**; `MASTER-108`'s sibling claim is already known wrong.
3. **`MASTER-044` — `Result<T>.Value` is `T?` on an unconstrained `T`.** CORE, `api-design`, the same family
   as the two 6b-viii just closed, and probably the same latent shape. Not yet measured by anyone.

---

## 4. The inherited discipline — this is the part that matters

**Re-measure every entry against a fixed commit before decomposing, and decompose from the measurement.**
6b-viii ran that rule and it caught the orchestrator again.

### What 6b-viii actually produced, in order of what it saved

**1. An asymmetric recommendation is evidence about its own reliability.** `dir-architect` recommended a
1-based `ObjectKind` in four numbered points and gave the alternative one paragraph of costs. The user
declined to ratify and asked it to argue the other side as strongly. It **reversed its own recommendation**,
retracting three of its four arguments:

- **One was factually wrong.** It had credited the 1-based option with preventing `ObjectKindInfo`'s `_ =>`
  arms from absorbing the value. Those arms absorb `(ObjectKind)0` *identically*. The argument was for
  improving `ObjectKindInfo`, presented as an argument against the other option.
- **Two were overstated**, including "the existing `IsInEnum()` rule starts working for free" — which saves
  one line in one file, once, while the domain guard the contract already required catches the case under
  either design.
- **One was not evidence at all** — the repo's `ObjectKind?` usages are *UI selection state*, a different
  concept from "this command arrived without a kind".

It also refuted its own headline claim with a probe: `EnumToStringConverter` writes `value.ToString()` and
`Enum.Parse` accepts numeric strings, so a 1-based enum never made an invalid kind unpersistable.
**A steelman round is cheap when nothing has been dispatched** — this one cost one agent round trip and zero
implementer rework.

**2. A one-sided injection proves half of what it appears to.** T1's criterion specified deleting the whole
two-condition `kind` guard. The reviewer judged that insufficient and **deleted each condition separately**:
removing `kind == ObjectKind.None` reddened facts (5) and (7) with (6) green; removing `!Enum.IsDefined(kind)`
reddened only (6). Each half has its own covering fact. The whole-guard deletion would have reddened
everything at once and proved only that *something* was load-bearing. **This is the second session running in
which strengthening a specified injection was the best finding**, and both times the reviewer did it unprompted
after being told where to be sceptical.

**3. A criterion can prescribe a construction that is vacuous.** T1's criterion required a throwaway validator
carrying only `IsInEnum()`, to prove `.NotEqual(ObjectKind.None)` is load-bearing. **An empty
`AbstractValidator` accepts everything**, so a green result cannot distinguish *"`IsInEnum` accepts `None`"*
from *"no rule ran"*. The claim survives on independent evidence, but **the defect is in the criterion, and
prescribing the construction froze it in.** Same family as 6b-vii's duplicate-rejection trap: *a test whose
green state is reachable without the thing under test proves nothing.*

**4. The orchestrator's measurement omitted the decisive context.** `measurement.md` presented the two designs
as an open choice without recording that one of them is **the project's own recorded recommendation**
(`CORE-017`, `docs/audit/01-domain-application.md:741`). Every *number* in that file held under independent
re-derivation; the omission was framing, and it would have let a departure from the audit record pass a gate
unremarked. The architect caught it.

**5. A guideline's escape clause is not a licence.** The Microsoft enum guideline offers **two** compliant
shapes — `None = 0`, or *most-common-default* `= 0`. A 1-based enum is **neither**; it is outside the
guideline, not inside its exception. The architect misread this in its first report and corrected it itself.

### The environment findings — three new instruments that lie

All three cost real work this session, and they join `grep -c $'\r$'` returning a confident `0` on a
correctly-CRLF file:

1. **LF line endings alone are an `IDE0055` build error here** — roughly thirty `Fix formatting` errors at odd
   columns, **with no mention of line endings**. This is what a file gets when it acquires LF by accident.
2. **`git show <rev>:<path> > file` and `perl -pe` strip CR** in this Git Bash. That is *how* a file acquires
   LF by accident. `head -N file > out` preserves CRLF.
3. **`sed -n 'Np' file | cat -A` displays lines without `^M` even when the file is genuinely CRLF** — a false
   negative that nearly made a reviewer report a line-ending regression that did not exist.
   `head -N | tail -1 | cat -A` shows the truth.

**Count line endings in binary. Nothing else in this environment is trustworthy about them.**

### The standing conflict — do not resolve this silently

**Three binding rules cannot all hold**, and 6b-viii's T2 hit it:

- the acceptance criterion required proving `LastModified` actually changed, or the hash-stability test is
  vacuous;
- `disciplines/test.md:33` forbids sleeping in tests, outright;
- contract §3 forbids adding a production seam for testability, which is what a clock injection is.

`LastModified` is `DateTime.UtcNow`, so two calls can return the same value. T2 used `await Task.Delay(20)`
and kept the test non-vacuous — **the right call, made silently rather than reported.** `dir-reviewer` cannot
catch this: it is handed no discipline by design and saw only a flake risk. The pre-existing instance in
`FloorMethodsTests.UpdateNumber_WhenCalled_UpdatesLastModified` is the same shape. **Fixing it needs a clock
seam, which is a design decision no session has taken.** It is in the backlog with both rules named.

### `dir-git` — clean this session, and the reason is repeatable

**Six dispatches, every repository write correct, and — for the first time in four sessions — every commit
message body factually clean.** What changed is not the agent. It is that **every dispatch supplied the
message's factual content verbatim and forbade additions**, and each body was then **read back against
`git show --stat` before the commit was treated as final**. The merge dispatch additionally carried an
explicit prohibition on claiming to carry the `.dir-sdlc` artifacts, because `e22f37a` falsely claimed exactly
that. **Keep doing both.** The failure mode is in its *reports and its prose*, not its writes.

### The structural limits, unchanged

- **Tasks cannot invoke git.** Entry closures cite commit hashes, so they are orchestrator work after the
  merge. "No stray edits" is also orchestrator-verified — a task cannot prove it.
- **No criterion may constrain what a task *reports*** — only what the repository *contains*.
- **`git diff` cannot see an untracked file**, so a task that only adds files shows an empty diff. Both of
  6b-viii's new test files were invisible to the reviewer's diff and had to be named explicitly in the
  dispatch. `git add -N` is **not** the fix.
- **Reviewers inject into files they do not own.** Both 6b-viii reviewers did, and both verified their
  restores **by blob hash** rather than by inspection. The orchestrator re-verified both independently before
  committing. An unrestored injection lands in the task's commit under the task's name.

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **367 lines** by both `wc -l` and `b.count(b'\n')`. 6b-viii appended **22 lines /
9 items** in a new section at the end.

> **The 8 lone `\r` characters are still there and are still not from this session.** Measured before and
> after 6b-viii's append: 8 both times. They date to 6b-iv. `wc -l` and `b.count(b'\n')` both give **367**;
> Python's text-mode iteration gives **375**, because universal-newline handling splits on a lone `\r` too.
> **State which method you used.** 6b-viii deliberately did not repair them — outside the approved scope, for
> the third session running, and fixing it silently is exactly the drift these sessions exist to remove.

6b-viii's nine items cover: the discipline-versus-contract sleep conflict; the `ObjectKindInfo` `None`
absorption; the vacuous prescribed validator construction; T1's ordering test asserting `ParamName` but not
`ActualValue`; T1's ordinal test checking only the first two members; **`MutationOutcome.Success` and the
enum-wide sweep**; the three new lying instruments; the architect's asymmetric-then-reversed recommendation;
and the orchestrator's own framing omission.

**Still unscheduled from earlier sessions:** the drop-placeholder indicator design question;
`AreaSidebarItem`'s two `PointerGestureRecognizer`s needing one hover re-observation; the truncated floor name
with no recovery affordance; the Appium DIU-versus-pixel ratchets and the clearance ratchet nobody has been
able to run; the EF instrument that cannot see a consistent namespace rename; the three 6b-iii items on
`docs/audit/05-tests.md`; the `with`-expression gap on `PathSegment` (`AreaOutlineGeometry.Translate`); the
bare `else` in `ValidateControlPoints`; and the count-only assertion in `AreaOutlineTests`.

**A standing environment fact every handoff before 6b-vii got wrong.** `IDE0055` is **not** set to `error` in
`.editorconfig` — `:64` reads `severity = warning`. It still fails the build, because
`Directory.Build.props:5` sets `TreatWarningsAsErrors=true` with `EnforceCodeStyleInBuild=true` at `:16`.
**The hazard is unchanged; the mechanism is different.** `AnalysisMode` is `Recommended` at `AnalysisLevel`
`10.0` — and **`CA1008` does not fire under it**, measured by build probe in 6b-viii.

**`dm-conventions` still teaches the pattern 6b-v removed** — `backend/dotnet-format.md:567` and `:588`. It is
a separate repository and was deliberately not modified. 6b-viii also searched it for enum-design guidance and
found **none**, which is why `CORE-017` was the only standing authority on the `MASTER-073` decision.

---

## 6. Commands to re-derive §2 yourself

Unchanged, and they still classify every row — including 6b-viii's two closures. Re-run rather than trusting §2.

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
the bucket, and in 6b-vi it caught the orchestrator's own malformed closure marker (`**DONE**` where the
convention is `**done**` — it would have made the entry invisible to every future survey). **Re-run the survey
after you edit the index.** 6b-viii did, and both closures classified correctly.

---

## 7. Mechanics

- **Worktree root must be short.** Use `C:/dm-wt/6b-ix`. This is a MAUI/WinUI solution and a long path breaks
  the WinUI XAML compiler with `MAX_PATH` errors; the scratchpad path is too long. `C:/dm-wt` exists and is
  empty.
- **Tracked files are CRLF and a formatting slip fails the build** — via `TreatWarningsAsErrors`, not an
  `IDE0055` severity of `error`; see §5. **The session's own harness instruction tells agents to edit via Bash
  `sed`** — that conflict is real and standing. **Name it in every dispatch.** 6b-vii and 6b-viii both did, and
  no task hit it. Verify new files' line endings by counting bytes in binary; the `Write` tool emits LF.
- **Three instruments lie about line endings in this environment.** See §4. Count in binary.
- **Close any running `DungeonMaster.App` before building the Windows TFM**, or `MSB3026`/`MSB3027`.
- **Verify in the main checkout after merge**, not only in the worktree.
- **Tasks share one worktree and one `obj/`/`bin/`.** Two tasks that both build must be serialised. 6b-viii ran
  a serial DAG for exactly this reason and declared `dependsOn` as build serialisation rather than claiming
  false parallelism.
- **To run the app for an observation pass:**
  `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`, then
  launch `src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`.
  **In Git Bash, give the path with forward slashes or quote it** — backslashes are eaten.
- **The Appium harness prerequisites are installed** but the server is not running:
  `appium --address 127.0.0.1 --port 4723`. `src/DungeonMaster.App.UiTests` is deliberately **outside**
  `src/DungeonMaster.sln`; do not add it. It has not been compiled in any session since 6b-v.
- **This session's model tiers:** implementers, reviewer and architect on `claude-opus-5`; `dir-git` on
  `claude-haiku-4-5`. There is still no `.dir-sdlc/config.json`; those are the documented defaults.
- **Gate 3 has no slot for orchestrator-authored repo content.** Entry closures need commit hashes, so no task
  can write them. 6b-viii reused the shape 6b-v, 6b-vi and 6b-vii used: merge, then `docs/audit/` (`d3b4cbd`),
  then `.dir-sdlc/` (`c327a7d`).
- **Do the `.dir-sdlc/` appends BEFORE dispatching the commit that stages them.** 6b-vii got this wrong and it
  resolved benignly; 6b-viii did it in the right order.
- **Do not edit a tracked file in the main checkout while `dir-git` is running a merge there.** 6b-viii held
  its backlog and index edits until the merge returned, for exactly this reason.
