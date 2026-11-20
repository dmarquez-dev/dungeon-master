# Handoff to session 6b-vii

Written at the close of 6b-vi, 2026-09-16. Self-contained: everything 6b-vii needs is here or at a path named
here. Nothing in it depends on the conversation that produced it.

**Every number below was re-derived from the repository at `527ff39` immediately before writing, not carried
from the session that produced it.** §6 gives you the commands to re-run the survey rather than trust the
counts. That discipline is not ceremony: 6b-iv found four of five inherited entries had changed disposition,
6b-v found its single entry's prose false, and **6b-vi found its own orchestrator's measurement overstated** —
see §4.

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`527ff39`**, pushed; local and `origin` at **0/0 parity**, working tree clean.
- **`main` is untouched** at `45495c1`, local and remote identical. Nothing in 6b-vi went near it.
- One worktree — the main checkout. 6b-vi's worktree was removed and `C:/dm-wt` is empty again; its branch
  `dir/2026-09-16-floor-sidebar-keyboard-route` is retained locally at `4c79364` and **deliberately not
  pushed**. There are now **ten** such branches and `git ls-remote --heads origin "dir/*"` returns **nothing**.
- Full solution suite: **916 passed, 0 failed** — `DungeonMaster.Tests` **796**, `DungeonMaster.App.Tests`
  **97**, `DungeonMaster.App.UnitTests` **23**. Build **0 Warning(s), 0 Error(s)** across all three App TFMs.
  Verified **in the main checkout after the merge**, not only in the session worktree. The total is the
  908 baseline plus 6b-vi's 8 new tests (7 decision tests, 1 hint test).
- 6b-vi merged as **`2c3a47e`**, a `--no-ff` merge with two parents and `HEAD^2 = 4c79364`, matching the shape
  of the seven session merges before it. Two follow-on commits on the base branch: `28bd8bd` (the entry
  closures) and `527ff39` (session artifacts and the backlog append).
- 6b-vi artifacts: `.dir-sdlc/sessions/2026-09-16-floor-sidebar-keyboard-route/` — `spec.md`,
  `manifest.json`, `coverage.md`, `measurement.md`, `contracts/` (3), `disciplines/` (4), `reviews/T1`-`T4`,
  **`observations/O1-keyboard-and-drag.md`** and **`observations/O2-after-the-revert.md`**,
  **`artifacts/MASTER-098-verification.md`**, `log.md`, `state.json`.

### Index arithmetic — read this before citing any line number

| File | Lines at `7483b57` | Now | Offset rule |
|---|---|---|---|
| `docs/audit/00-master-index.md` | 667 | 667 | **none — no line moved.** Both edits were in-place within existing rows |
| `docs/audit/01-domain-application.md` | 1352 | 1352 | untouched |
| `docs/audit/04-app-controls-and-theme.md` | 1226 | 1226 | untouched |
| `docs/audit/05-tests.md` | 2189 | 2189 | untouched |
| `.dir-sdlc/backlog.md` | 294 | 322 | +28, all appended at the end |

Counted with `b.count(b'\n')`, not `content.split('\n')` — that method is off by one on every
newline-terminated file and is a standing backlog item.

**Do not apply the table and consider the citation verified.** It tells you where a line *moved*, not whether
the claim at that line is still true. And `00-master-index.md`'s zero offset does not mean its citations are
stable in general: it grew 590 → 667 during 6b-iv, so a citation written before 6b-iv is still off by up to
77 lines. The offset you need depends on when the citation was written.

---

## 2. What is open

176 entry rows: **120 closed, 53 open, 3 part done.**

Open by layer — `CORE` **20**, `UI` 9, `UIKIT` 9, `TEST` 8, `INFRA` 6, plus 1 spanning `UI, XCUT`.
By severity — **S2 0**, S3 12, S4 31, S5 10.

> **THERE ARE NO OPEN S1 OR S2 ENTRIES LEFT IN THE AUDIT.** 6b-vi closed `MASTER-176`, the last one. Every
> remaining open entry is S3 or below. **This changes how you should choose scope**: the severity argument
> that has driven the last several sessions is spent, and the next session picks on coherence, layer
> concentration, or unblocking value instead.

The three `part done` entries, which **must not be marked done**: `MASTER-094` (`:282`), `MASTER-098`
(`:286`), `MASTER-108` (`:296`).

**`MASTER-098` is now measured rather than merely deferred.** 6b-vi committed
`artifacts/MASTER-098-verification.md` (`553493f`), which re-derives every claim in `UIKIT-011`'s source
record from the controls themselves: **four of five named defects are gone**, one survives, and the record's
own `locations:` citations at `04-app-controls-and-theme.md:514-515` are **both stale**. The survivor is
`:533` — *"it forces the host page to track selection itself and push it back in"* — which still holds:
`DungeonDetailPage` writes `item.IsExpanded` at `:832`, `:965`, `:2204` and pushes area selection through
`SetAreaEditing` at `:2191`, and **nothing binds `IsSelected` or `IsExpanded` to a view-model property**.
Whoever picks it up inherits a breakdown and should not re-derive it from the entry's prose.

**`MASTER-108`'s prose carries a wrong number.** It describes `DungeonDetailPage.xaml.cs` as "2,226 lines".
It was **2488** at `7483b57` and is larger now after 6b-vi's edits. Re-measure before citing.

**The twelve open S3s:**

| Line | Entry | Layer | Title (truncated) |
|---|---|---|---|
| `:232` | `MASTER-044` | CORE | `Result<T>.Value` is `T?` on an unconstrained `T` |
| `:235` | `MASTER-047` | CORE | Five Domain overloads have no production caller |
| `:239` | `MASTER-051` | UI | Entry and Editor handler mappers repeat eight WinUI resets |
| `:261` | `MASTER-073` | CORE | `ObjectKind` has no zero value |
| `:262` | `MASTER-074` | CORE | `Entity` provides no identity-based equality |
| `:269` | `MASTER-081` | CORE | The Domain has no policy layer |
| `:295` | `MASTER-107` | CORE | `Dungeon` is a 453-line delegation facade |
| `:356` | `MASTER-168` | CORE | `Area.SetOutline` stores a degenerate or null outline |
| `:357` | `MASTER-169` | CORE | `Floor` accepts a zero or negative floor number |
| `:358` | `MASTER-170` | CORE | `PathSegment` permits a `CubicBezier` with null control points |
| `:360` | `MASTER-172` | TEST | No test asserts `Dispatcher` propagates its `CancellationToken` |
| `:361` | `MASTER-173` | TEST | `Dispatcher`'s missing-handler failure mode is untested |

---

## 3. The recommended session

This is a recommendation, not a decision — 6b-vii's Gate 1 owns the scope.

**`MASTER-168` + `MASTER-169` + `MASTER-170` — the three missing production invariants.**

They were 6b-vi's own declined alternative and the argument for them is now stronger, because the S2 that
outranked them is gone.

- **They are one coherent cluster**: three missing invariants in the same layer, each about a constructor or
  setter accepting a value the domain should reject.
- **CORE is by far the largest open layer at 20** of 53, and 6b-vi took nothing out of it.
- **They are compiler- and test-verifiable with no human observation at all** — which matters more than
  usual after 6b-vi, whose two blocking checks could only be settled by a person watching the app.

**The argument against, which is why they were filed rather than fixed:** 6b-iii's escalation E2 recorded
that these are **missing production invariants, not test gaps** — a test cannot assert behaviour that does
not exist, so each needs a design decision about what the behaviour *should be* before any test can assert
it. Three design decisions at one Gate 1 is a real load, and getting one wrong writes a wrong invariant into
the domain. Expect Gate 1 to be long and expect to need the user on all three.

### The alternative: close the gate 6b-vi left untested

**One task, small, and it removes the only real hole 6b-vi shipped.** The keyboard reorder route added by
6b-vi gates entirely on `FloorSidebarItem.ContainsKeyboardFocus`, and **nothing tests it** — the route's
correctness rests on observation O2 alone. `FloorDragHandleTests` is a markup-shape test and structurally
cannot observe focus.

**The objection that makes it look expensive has already been disproved, and that is why this is worth
doing now**: a task warned the test would need reflection against a private property key; its reviewer
reflected over `Microsoft.Maui.Controls.dll` and found `VisualElement.IsFocusedPropertyKey` is **public** and
`BindableObject.SetValue(BindablePropertyKey, object)` is a **public overload**. So
`descendant.SetValue(VisualElement.IsFocusedPropertyKey, true)` is public API with no internals coupling, and
it is exactly the input `HasFocusedDescendant` (`FloorSidebarItem.xaml.cs:342`) consumes. Home:
`DungeonMaster.App.Tests/Views/`, precedent `FloorReorderDropRoundTripTests`. **Test the recursion at depth** —
a focused descendant in the area list rather than a direct child — because that is the half a later template
change would break invisibly.

---

## 4. The inherited discipline — this is the part that matters

**Re-measure every entry against a fixed commit before decomposing, and decompose from the measurement.**
6b-vi ran that rule and it caught **its own orchestrator**.

### The finding 6b-vi actually produced

**Some claims can only be settled by a person using the software, and a criterion that cannot see them will
pass anyway.**

6b-vi's first implementation converted the floor drag handle from a `Label` to a `Button` so it could take
keyboard focus — the remedy its own file already argued for, with a worked precedent two sessions old. It
passed **17 acceptance criteria and a review with zero blocking findings**. Build clean, suite green.

**It had silently destroyed drag-and-drop.** A WinUI `Button` consumes the pointer press, so MAUI's
`DragGestureRecognizer` never starts a drag. Nothing static could see it: the markup was correct, the handler
names were unchanged, the gesture was present and countable, and Microsoft's documentation confirms the API
is legal on a `Button`. **A human pressed and held for ten seconds and the session reversed.**

Three things made that recoverable rather than shipped:

1. **The risk was named before implementation.** `measurement.md` recorded that this handle carries the
   repository's **only** `DragGestureRecognizer`, so no precedent existed. The spec's Assumptions table
   recorded it as **API-legal but behaviourally unestablished**.
2. **It was put to the user at Gate 2 as a choice**, against a lower-risk design, and accepted knowingly.
3. **Two definition-of-done items were marked observation-only at Gate 2** rather than dressed up as
   verifiable — and **both turned out to be the ones that mattered**. `coverage.md` said so before dispatch:
   *"a task can assert that the drag handler names are unchanged and that a size ratchet still holds; neither
   is evidence that a drag still starts."*

**The transferable rule: when a criterion cannot see the thing it is about, say so at Gate 2 and route it to
observation. Do not let a static proxy stand in for it.** The proxy will pass.

### Four instrument failures, all by the orchestrator

Every one returned a **confident** answer, and every one was caught by running a different command:

1. **SHA-256 of a worktree file against `git show` output**, to verify a defect injection was reverted.
   Those digests can *never* match on a CRLF working tree, because git stores blobs with LF. Would have
   reported a clean revert as corruption. **`git diff HEAD -- <path>` is the instrument.**
2. **`git diff` against a base predating both revision rounds**, to prove round 2 touched only comments.
   Structurally cannot isolate one round. The reviewer closed it by reconstructing the byte delta per
   replacement (+14, −9, −2, −2 = +1) — **ask for the per-edit reconstruction, not the aggregate**.
3. **`grep -c '\\n'`** to check a commit message for literal escapes. Matched every line. `grep -cF '\n'`
   returns 0; the message was always well-formed. A correction was issued to the user on a claim that was
   never true.
4. **Writing an index closure as `**DONE (6b-vi...)**`**, which the survey classified **UNCLASSIFIED** —
   117 existing entries use `**done**`. Left unfixed, `MASTER-176` would have been **invisible to every
   future survey**: neither closed nor open. Caught because the survey was re-run after the edit.

**Re-run the survey after you edit the index.** Number 4 is the one that would have done lasting damage, and
it cost one command to catch.

### Three defective criteria, all orchestrator-authored, all caught by tasks

- **A criterion predicted one failing test; two failed.** The orchestrator wrote contract amendment A1 §5A,
  which changed a `Hint` string, *and* the criterion asserting that changing it would break nothing — while a
  test pinned that string verbatim. **When an amendment changes a value a test asserts, derive the
  failure-count prediction from a search, not from recollection.**
- **Two criteria in one dispatch directly contradicted each other** — one requiring false doc comments
  corrected, one requiring that file unchanged. **Resolution rule: resolve toward the criterion whose purpose
  the other was serving, and disclose.**
- **A `grep -c` over a bare type name counts comment prose**, so two criteria could fail a correct file.
  `grep -c "throw"` also leaves `ThrowIfNull` and implicit-throw operators uncounted. **Pin the declaration
  form** — `grep -c "<DragGestureRecognizer"`.

### Injections: one was inert, and its author caught it

A task proving a `CS0029` revert guard fires changed only an element's start tag. `MAUIG1001` plus a cascade
of `CS0103` **masked `CS0029` entirely** — a loud red build for unrelated reasons, which read carelessly is a
successful demonstration. **It checked the error codes, corrected the injection, re-ran, and only then
claimed the result.** The handoff you are replacing catalogued nine inert injections across two sessions,
one caught.

Its reviewer then reproduced it and found more: **removing the `Clicked` handler produces no diagnostic at
all**, so `CS0029` is the *sole* barrier to a silent `MASTER-164` regression.

**And on which side to inject:** a task showed its markup assertions non-vacuous by mutating its own
**expected** values. Two reviewers ruled that the weaker instrument — it proves expected ≠ actual, not that
the actual side is read from the file, since a hard-coded reader behaves identically. The second reviewer
**rejected it and injected into the production XAML instead**, and only then recorded `reviewer-reproduced`.
**Vary the input; hold the test fixed.**

### The structural limits, unchanged

- **Tasks cannot invoke git.** Entry closures cite commit hashes, so they are orchestrator work after the
  merge. Attribution of "which files did this task write" is also orchestrator work — file-mtime and
  `find -newer` provably cannot do it in a shared worktree.
- **No criterion may constrain what a task *reports*** — only what the repository *contains*. A reviewer
  cannot see another task's report.
- **`git diff` cannot see an untracked file**, so a task that only adds files shows an empty diff. Tell the
  reviewer; `git add -N` is **not** the fix.
- **The Gate 3 contract-operations check is a call-site search and does not apply to rule-shaped contracts.**
  6b-vi's C2 and C3 declare rules, not callable operations; the search was deliberately not run over them.
  Two of C1's operations also searched as `not called` because implementer and consumer were the same task —
  **`coverage.md` predicted that before dispatch**, which is the right time to say it.

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **322 lines** by both `wc -l` and `b.count(b'\n')`. 6b-vi appended **28 lines /
13 items** in a new section at the end.

> **The 8 lone `\r` characters are still there and are still not from this session.** Measured at `7483b57`:
> 8 before, 8 after. They date to 6b-iv's append. `wc -l` and `b.count(b'\n')` both give **322**; Python's
> text-mode iteration gives **330**, because universal-newline handling splits on a lone `\r` too. **State
> which method you used.** Fixing the eight sequences is a one-line repair nobody has scheduled.

6b-vi's thirteen items, in the file's new final section, cover: the untested `ContainsKeyboardFocus` gate
(**with the public-API disproof that makes it cheap**); the `grep -c` prose-counting defect; the
span-read-versus-span-cited trap recurring, this time committed by the orchestrator; the failure-count
criterion defect; the contradictory-criteria resolution rule; the SHA-256-versus-`git show` wrong instrument;
the expectation-side-versus-input-side injection rule; **the drag handle no longer being a tab stop**, which
is a deliberate accessibility reduction and names the conditions for revisiting it; the user's own
floor-name spacing suggestion, attached to 6b-iv's truncation item rather than actioned; the two observation
transcripts disagreeing by one character on the floor name with **no DIU measurement in either pass**;
`HasFlag(Control)` admitting `Ctrl`+`Shift`+arrow; the Appium clearance ratchet possibly having shifted with
**nobody able to tell, because that suite is outside the solution**; and `QualifiedElementName` returning a
bare local name, making element-type assertions weaker than they read.

**Still unscheduled from earlier sessions:** the drop-placeholder indicator design question (cut from 6b-vi
at Gate 1 and still genuinely unsettled); `AreaSidebarItem`'s two `PointerGestureRecognizer`s needing one
hover re-observation; the truncated floor name with no recovery affordance; the Appium DIU-versus-pixel
ratchets; the EF instrument that cannot see a consistent namespace rename; and the three 6b-iii items on
`docs/audit/05-tests.md`.

**`dm-conventions` still teaches the pattern 6b-v removed** — `backend/dotnet-format.md:567` and `:588`. It
is a separate repository and was deliberately not modified.

---

## 6. Commands to re-derive §2 yourself

Unchanged from the ones 6b-vi used, and they still classify every row — including 6b-vi's own two edits,
**after** the closure marker was corrected to `**done**`. Re-run rather than trusting §2.

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
print("\nopen S2:");     [print(f"  :{r['line']}  {r['id']}") for r in op if r["sev"]=="S2"] or print("  NONE")
print("\npart done:");   [print(f"  :{r['line']}  {r['id']}") for r in rows if r["state"]=="part done"]
PY
```

**The column index is `[10]`**, because splitting a row on `|` yields a leading and a trailing empty cell.
An `awk -F'|'` doing the same job uses `$11`. Writing the 6b-v handoff produced a wrong survey from exactly
that off-by-one.

**If any row prints `UNCLASSIFIED`, read the cell before deciding what it means.** That is the whole point of
the bucket, and in 6b-vi it caught the orchestrator's own malformed closure marker.

---

## 7. Mechanics

- **Worktree root must be short.** Use `C:/dm-wt/6b-vii`. This is a MAUI/WinUI solution and a long path
  breaks the WinUI XAML compiler with `MAX_PATH` errors; the scratchpad path is too long. `C:/dm-wt` exists
  and is empty.
- **Tracked files are CRLF and `IDE0055` is a build error.** Editing a tracked file with `sed -i` rewrites it
  to LF and fails the build on every line. **The session's own harness instruction tells agents to edit via
  Bash `sed`** — that conflict is real and standing. **Name it in every dispatch.** 6b-vi did, and no task
  hit it.
- **`grep -c $'\r$'` returns a confident `0` on a correctly-CRLF file in this environment.** Count in binary.
- **Close any running `DungeonMaster.App` before building the Windows TFM**, or `MSB3026`/`MSB3027`.
- **Verify in the main checkout after merge**, not only in the worktree.
- **Tasks share one worktree and one `obj/`/`bin/`.** Two tasks that both build share four upstream projects
  through it — serialize them. 6b-vi ran only one genuinely concurrent pair, where one task built nothing.
- **Defect injection is safe only when no peer is live.** 6b-vi's final task injected into another task's
  file, sanctioned explicitly because it was last and the worktree was its own — the orchestrator verified
  that precondition before dispatching rather than assuming it.
- **To run the app for an observation pass:**
  `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`,
  then launch
  `src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`.
  **In Git Bash, give the path with forward slashes or quote it** — backslashes are eaten and the launch
  fails with `command not found`.
- **The Appium harness prerequisites are installed** but the server is not running:
  `appium --address 127.0.0.1 --port 4723`. `src/DungeonMaster.App.UiTests` is deliberately **outside**
  `src/DungeonMaster.sln`; do not add it. **It was never compiled at any point during 6b-vi** — see the
  backlog item about the clearance ratchet.
- **This session's model tiers:** implementers, reviewer and architect on `claude-opus-5`; `dir-git` on
  `claude-haiku-4-5`. There is still no `.dir-sdlc/config.json`; those are the documented defaults.
- **Gate 3 has no slot for orchestrator-authored repo content.** Entry closures need commit hashes, so no
  task can write them. 6b-vi reused 6b-v's shape: merge, then `docs/audit/` (`28bd8bd`), then `.dir-sdlc/`
  (`527ff39`). Reuse it.
- **`dir-git` performed 9 operations in 6b-vi with no wrong writes and no wrong numbers in its reports** —
  better than its record in 6b-v. **Verify its outcomes with your own commands anyway.** Every commit in this
  session was independently re-counted by the orchestrator, which is why that claim can be made at all.
