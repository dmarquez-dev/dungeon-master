# Handoff to session 6b-viii

Written at the close of 6b-vii, 2026-09-17. Self-contained: everything 6b-viii needs is here or at a path
named here. Nothing in it depends on the conversation that produced it.

**Every number below was re-derived from the repository at `bcef1e4` immediately before writing, not carried
from the session that produced it.** §6 gives you the commands to re-run the survey rather than trust the
counts. That discipline keeps earning: 6b-iv found four of five inherited entries had changed disposition,
6b-v found its single entry's prose false, 6b-vi found its own orchestrator's measurement overstated, and
**6b-vii had its own measurement corrected twice — once by itself and once by an agent it dispatched.**

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`bcef1e4`**, pushed; local and `origin` at **0/0 parity**, working tree clean.
- **`main` is untouched** at `45495c1`, local and remote identical. Nothing in 6b-vii went near it.
- One worktree — the main checkout. `C:/dm-wt` is empty again. 6b-vii's branch
  `dir/2026-09-16-core-domain-invariants` is retained locally at `2014e09` and **deliberately not pushed**.
  There are now **eleven** such branches and `git ls-remote --heads origin "dir/*"` returns **nothing**.
- Full solution suite: **935 passed, 0 failed** — `DungeonMaster.Tests` **815**, `DungeonMaster.App.Tests`
  **97**, `DungeonMaster.App.UnitTests` **23**. Build **0 Warning(s), 0 Error(s)** across all three App TFMs.
  Verified **in the main checkout after the merge**, not only in the session worktree. The total is the 916
  baseline plus 6b-vii's 20 new tests minus 1 deleted.
- 6b-vii merged as **`9fc02af`**, a `--no-ff` merge with two parents and `HEAD^2 = 2014e09`, matching the
  shape of the eight session merges before it. Two follow-on commits on the base branch: `eca0cd8` (the entry
  closures) and `bcef1e4` (session artifacts and the backlog append).
- 6b-vii artifacts: `.dir-sdlc/sessions/2026-09-16-core-domain-invariants/` — `spec.md`, **`measurement.md`**,
  `manifest.json`, `coverage.md`, `contracts/domain-invariants.md`, `disciplines/` (4), `reviews/T1`–`T3`,
  `log.md`, `state.json`.

### Index arithmetic — read this before citing any line number

| File | Lines at `dff976d` | Now | Offset rule |
|---|---|---|---|
| `docs/audit/00-master-index.md` | 667 | 667 | **none — no line moved.** All three edits were in-place within existing rows |
| `.dir-sdlc/backlog.md` | 322 | 345 | +23, all appended at the end |

Counted with `b.count(b'\n')`, not `content.split('\n')` — that method is off by one on every
newline-terminated file and is a standing backlog item.

**Do not apply the table and consider the citation verified.** It tells you where a line *moved*, not whether
the claim at that line is still true. And `00-master-index.md`'s zero offset does not mean its citations are
stable in general: it grew 590 → 667 during 6b-iv, so a citation written before 6b-iv is still off by up to
77 lines.

---

## 2. What is open

176 entry rows: **123 closed, 50 open, 3 part done.**

Open by layer — `CORE` **17**, `UI` 9, `UIKIT` 9, `TEST` 8, `INFRA` 6, plus 1 spanning `UI, XCUT`.
By severity — **S1 0, S2 0**, S3 **9**, S4 31, S5 10.

> **There are still no open S1 or S2 entries.** 6b-vi closed the last one. The severity argument remains
> spent, and 6b-vii picked on **coherence and layer concentration** instead — three entries in one layer, each
> the same shape. That worked well enough to recommend repeating; see §3.

The three `part done` entries, which **must not be marked done**: `MASTER-094` (`:282`), `MASTER-098`
(`:286`), `MASTER-108` (`:296`).

**`MASTER-098` was measured by 6b-vi, not closed.** `artifacts/MASTER-098-verification.md` (`553493f`)
re-derives every claim in `UIKIT-011`'s source record: four of five named defects gone, one survivor, and the
record's own `locations:` citations at `04-app-controls-and-theme.md:514-515` are **both stale**. The survivor
is `:533` — the host page still owns selection and pushes it in. Whoever picks it up inherits a breakdown and
should not re-derive it from the entry's prose.

**`MASTER-108`'s prose carries a wrong number.** It describes `DungeonDetailPage.xaml.cs` as "2,226 lines".
It was 2488 at `7483b57` and is larger now. Re-measure before citing.

**The nine open S3s:**

| Line | Entry | Layer | Title (truncated) |
|---|---|---|---|
| `:232` | `MASTER-044` | CORE | `Result<T>.Value` is `T?` on an unconstrained `T` |
| `:235` | `MASTER-047` | CORE | Five Domain overloads have no production caller |
| `:239` | `MASTER-051` | UI | Entry and Editor handler mappers repeat eight WinUI resets |
| `:261` | `MASTER-073` | CORE | `ObjectKind` has no zero value |
| `:262` | `MASTER-074` | CORE | `Entity` provides no identity-based equality |
| `:269` | `MASTER-081` | CORE | The Domain has no policy layer |
| `:295` | `MASTER-107` | CORE | `Dungeon` is a 453-line delegation facade |
| `:360` | `MASTER-172` | TEST | No test asserts `Dispatcher` propagates its `CancellationToken` |
| `:361` | `MASTER-173` | TEST | `Dispatcher`'s missing-handler failure mode is untested |

---

## 3. The recommended session

This is a recommendation, not a decision — 6b-viii's Gate 1 owns the scope.

**`MASTER-073` + `MASTER-074` — the two remaining CORE type-level defects.** They are the closest thing left
to the cluster 6b-vii just closed: both are about a Domain type failing to establish something its own design
assumes, both are compiler- and test-verifiable with no human observation, and CORE remains the largest open
layer at 17.

- **`MASTER-073` — `ObjectKind` has no zero value.** A C# enum's `default` is `0`; if no member is `0`, then
  `default(ObjectKind)` is a value outside the enumeration, and every `PlacedObject` materialised before its
  kind is assigned holds one. Cheap to check, and the fix is a design decision (add an explicit `None = 0`, or
  guard construction) that Gate 1 must make before any test can assert it — **the same shape as 6b-vii's
  three**, so the same playbook applies.
- **`MASTER-074` — `Entity` provides no identity-based equality.** Two instances of the same persisted entity
  compare by reference, so `Contains`, `Distinct` and any set operation over aggregates behave by object
  identity rather than by `Id`. Check whether anything relies on the current reference semantics **before**
  proposing the fix; EF change tracking has its own identity map and may make some call sites indifferent.

**The argument against:** `MASTER-074` in particular may be wider than it reads. `Entity` is the base of every
aggregate, so equality semantics touch every collection operation in the Domain and possibly the EF
configuration. Measure the blast radius before decomposing, exactly as 6b-v did for the namespace rename.

### The alternatives, ranked

1. **`MASTER-172` + `MASTER-173` — the two `Dispatcher` test gaps.** The only remaining pair that are *genuinely*
   test gaps rather than missing production behaviour, so no design decision is needed and Gate 1 is short.
   Both were filed by 6b-iii as dispatcher residuals. Smallest, safest, least ambitious.
2. **`MASTER-107` — `Dungeon` is a 453-line delegation facade.** Large, structural, and the kind of change that
   wants its own session. Re-measure the line count first; `MASTER-108`'s sibling claim is already known wrong.
3. **The `with`-expression gap from 6b-vii** (first backlog item) — a one-line App.Core change plus its test,
   closing the one construction path that bypasses `PathSegment`'s new guard. Too small to be a session on its
   own; fold it into whatever runs next.

---

## 4. The inherited discipline — this is the part that matters

**Re-measure every entry against a fixed commit before decomposing, and decompose from the measurement.**
6b-vii ran that rule and it caught **the orchestrator twice** — once by the orchestrator itself, once by an
agent it had dispatched.

### The finding 6b-vii actually produced

**A test that duplicates a rejection something downstream already performs cannot be verified by exception
type alone.**

T1's null-outline test asserted `ThrowExactly<ArgumentNullException>()`. That **passes against unguarded
code**: `List<T>.AddRange(null)` throws that exact type on its own, with `ParamName` `"collection"`. The
criterion — orchestrator-authored — did not mandate a parameter-name assertion, so a literal reading of it
produces a test that cannot fail. The implementer noticed while writing it and added
`.WithParameterName("outline")`; the discrimination run then failed with *"Expected exception with parameter
name 'outline', but found 'collection'"*, which is the only reason the case is discriminating at all.

**Checked and found NOT to generalise**, which is the other half of the lesson — a rule stated without its
boundary gets applied where it does not belong. `new Floor(id, 0, name)` and
`new PathSegment(CubicBezier, end, null, null)` throw **nothing** without their guards, so T2's and T3's tests
were never exposed. The trap is specific to **a new guard that rejects what a downstream call already
rejects**.

### Two instrument failures, both the orchestrator's

1. **`grep "new PathSegment("` returns 26 sites; both forms return 42.** The target-typed `new(...)` form
   inside a collection expression never spells the type. The 16 missed sites concealed
   `AreaOutlineGeometryTests.cs:63` — a `CubicBezier` with `Ctrl2` null, illegal under this session's own
   decision. `dir-architect` caught it during decomposition. **A census over a C# type must cover the
   target-typed form.** Verified at three commits: 42 at `dff976d`, 51 at `4011662`, 59 at the tip.
2. **The measurement's first pass claimed a degenerate outline was reachable from the UI.** It reasoned from
   the Application validator alone — which constrains null and empty but not segment count — and concluded the
   gap was live. **Tracing the producer refuted it**: `MapDrawingSession.BuildOutline` returns `[]` below three
   vertices and otherwise emits one segment per vertex plus a closing segment, so 0 or ≥3, never 1 or 2. **A
   validator tells you what is rejected; only the producer tells you what is reachable.**

### The contract defect, and why it did not cost anything

**A contract must pin observable behaviour, never a worked code form.** `contracts/domain-invariants.md` §3
sketched the `PathSegment` validator with camelCase helper parameters while its binding table demanded
`ParamName` of exactly `Ctrl1`/`Ctrl2`. That combination **does not compile**: `CA2208` fires four times
demanding the exact casing. The sketch was rung-4 judgment presented in the authoritative shape of worked
code, and it was frozen that way.

What contained it: the implementer diagnosed the build error, judged the deviation to be form rather than
substance, **did not silently build something else**, and said so in its report. The reviewer then **verified
the `CA2208` claim by restoring the camelCase form and building it** before ruling. Pin the exact exception
type and the exact `ParamName`; leave the construction form to the implementer.

### What the reviews did that is worth repeating

- **The mirror injection.** T2's own discrimination removed only the `UpdateNumber` guard and watched its two
  tests fail — which proves `UpdateNumber` is covered but says nothing about whether the constructor tests pass
  *for the constructor guard's sake*. The reviewer removed the constructor guard alone and got the exact
  complementary failure. **When one change adds two guards, a one-sided injection proves half of what it
  appears to.**
- **Reaching the branch you think you are reaching.** T2's `ReorderFloor(floorId, 0)` test could have returned
  `OutOfRange` via the `oldNumber == newNumber` short-circuit rather than the range check. The reviewer
  weakened `newNumber < 1` to `< 0` and watched the test go red, proving it reaches `DungeonMethods.cs:122`.
- **Declining to manufacture a finding.** T2's implementer flagged two additive changes no criterion mandated
  and asked for a ruling. The reviewer checked the contract, found the exception message was a `<message>`
  placeholder the implementer was *obliged* to fill, and said so rather than writing it up as scope creep.
- **The unpinned unknown.** Nobody knew whether `System.Text.Json` surfaces a constructor `ArgumentException`
  directly or wraps it in a `JsonException`. Rather than guess it into a criterion a correct implementation
  could fail, T3's criterion **required the task to run it, observe, and pin what it saw** — then the reviewer
  reproduced it independently. It surfaces **directly, unwrapped**.

### The Gate 2 prediction that was wrong

`coverage.md` predicted in writing, before dispatch, that all seven contract operations would search as
`not called`. **Two came back `called`** — `Area.SetOutline` has a real call site at `FloorMethods.cs:170`,
which survives all three discard rules because that file is owned by T2 while the definition is owned by T1.
The prediction was made in good faith, to stop a suspicious-looking result being read as a late excuse. It was
still wrong, and `coverage.md` records it as refuted rather than overwritten. **A pre-registered prediction is
worth exactly its accuracy.**

### `dir-git` — the failure mode has escalated

Nine dispatches, **every repository write correct**. **Two commit-message bodies were false**, and the second
was not a miscount but an **invention**: it asserted the session flagged deferred items for *"object-value-object
scale work and hit-zone refactoring"*. No such finding exists — zero matches in the backlog, raised by no
review. The merge message separately claimed to carry the `.dir-sdlc` artifacts (it carried ten files, none
under `.dir-sdlc/`) and reported the suite as 800 → 807, one task's local delta.

Both were caught **pre-push** by reading the message body against `git status` and `git show --stat`, and both
were amended with the user's approval, preserving tree and parents (`e22f37a` → `9fc02af`,
`38f23f1` → `bcef1e4`). **Supply the message's factual content verbatim, forbid additions — and then read the
body back and match each claim to an artifact before treating the commit as final.** Everyone checks the staged
paths; nobody re-reads the prose.

### The structural limits, unchanged

- **Tasks cannot invoke git.** Entry closures cite commit hashes, so they are orchestrator work after the
  merge. "No stray edits" is also orchestrator-verified — a task cannot prove it, and 6b-vii's T3 said so
  plainly rather than asserting it.
- **No criterion may constrain what a task *reports*** — only what the repository *contains*.
- **`git diff` cannot see an untracked file**, so a task that only adds files shows an empty diff. Tell the
  reviewer; `git add -N` is **not** the fix.
- **Reviewers inject into files outside the task's ownership.** 6b-vii's T2 reviewer injected into
  `DungeonMethods.cs`, owned by nobody in that task, and restored it. **Verify the restore yourself before
  committing** — an unrestored injection would have been swept into T2's commit under T2's name.

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **345 lines** by both `wc -l` and `b.count(b'\n')`. 6b-vii appended **23 lines /
10 items** in a new section at the end.

> **The 8 lone `\r` characters are still there and are still not from this session.** Measured before and
> after 6b-vii's append: 8 both times. They date to 6b-iv's append. `wc -l` and `b.count(b'\n')` both give
> **345**; Python's text-mode iteration gives **353**, because universal-newline handling splits on a lone
> `\r` too. **State which method you used.** 6b-vii deliberately did not repair them — outside the approved
> scope, and fixing it silently is exactly the drift these sessions exist to remove.

6b-vii's ten items cover: the `with`-expression gap on `PathSegment`; the bare `else` in
`ValidateControlPoints` that a third `PathSegmentKind` would silently inherit; the count-only assertion in
`AreaOutlineTests`; the duplicate-rejection criterion trap; the target-typed `new(...)` census defect; the
contract-sketch-that-does-not-compile; **the `IDE0055` mechanism correction** (see below); `dir-git`'s
fabricated commit message; the refuted Gate 2 coverage prediction; and the eight lone `\r`.

**A standing environment fact every prior handoff got wrong.** `IDE0055` is **not** set to `error` in
`.editorconfig` — `:64` reads `severity = warning`. It still fails the build, because `Directory.Build.props:5`
sets `TreatWarningsAsErrors=true` with `EnforceCodeStyleInBuild=true` at `:16`. **The hazard is unchanged; the
mechanism is different** — and a future session checking the `.editorconfig` line alone would see `warning` and
wrongly conclude the constraint had been lifted. The `sed -i` prohibition stands for exactly the reason it
always did.

**Still unscheduled from earlier sessions:** the drop-placeholder indicator design question;
`AreaSidebarItem`'s two `PointerGestureRecognizer`s needing one hover re-observation; the truncated floor name
with no recovery affordance; the Appium DIU-versus-pixel ratchets and the clearance ratchet nobody has been
able to run; the EF instrument that cannot see a consistent namespace rename; the three 6b-iii items on
`docs/audit/05-tests.md`; and `FloorMethodsTests.UpdateNumber_WhenCalled_UpdatesLastModified`'s
`await Task.Delay(20)`, which violates the composed test discipline's no-sleeping rule (noted by 6b-vii's
architect, out of its surface).

**`dm-conventions` still teaches the pattern 6b-v removed** — `backend/dotnet-format.md:567` and `:588`. It is
a separate repository and was deliberately not modified.

---

## 6. Commands to re-derive §2 yourself

Unchanged, and they still classify every row — including 6b-vii's three closures. Re-run rather than trusting §2.

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
after you edit the index.** 6b-vii did, and all three closures classified correctly.

---

## 7. Mechanics

- **Worktree root must be short.** Use `C:/dm-wt/6b-viii`. This is a MAUI/WinUI solution and a long path breaks
  the WinUI XAML compiler with `MAX_PATH` errors; the scratchpad path is too long. `C:/dm-wt` exists and is
  empty.
- **Tracked files are CRLF and a formatting slip fails the build** — via `TreatWarningsAsErrors`, not an
  `IDE0055` severity of `error`; see §5. **The session's own harness instruction tells agents to edit via Bash
  `sed`** — that conflict is real and standing. **Name it in every dispatch.** 6b-vii did, and no task hit it.
  Verify new files' line endings by counting bytes in binary, and note the `Write` tool emits LF.
- **`grep -c $'\r$'` returns a confident `0` on a correctly-CRLF file in this environment.** Count in binary.
- **Close any running `DungeonMaster.App` before building the Windows TFM**, or `MSB3026`/`MSB3027`.
- **Verify in the main checkout after merge**, not only in the worktree.
- **Tasks share one worktree and one `obj/`/`bin/`.** Two tasks that both build must be serialised. 6b-vii ran
  a fully serial DAG for exactly this reason and said so at Gate 2 rather than claiming false parallelism.
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
  can write them. 6b-vii reused the shape 6b-v and 6b-vi used: merge, then `docs/audit/` (`eca0cd8`), then
  `.dir-sdlc/` (`bcef1e4`).
- **Do the `.dir-sdlc/` appends BEFORE dispatching the commit that stages them.** 6b-vii wrote `state.json` and
  `log.md` *after* dispatching that commit. It resolved benignly — the session directory was still untracked, so
  the writes were picked up — but it was a race that did not need to exist.
