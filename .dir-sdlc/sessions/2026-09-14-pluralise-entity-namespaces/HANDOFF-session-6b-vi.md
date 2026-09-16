# Handoff to session 6b-vi

Written at the close of 6b-v, 2026-09-16. Self-contained: everything 6b-vi needs is here or at a path named
here. Nothing in it depends on the conversation that produced it.

**Every number below was re-derived from the repository at `12211d4` immediately before writing, not carried
from the session that produced it.** §6 gives you the commands to re-run the survey rather than trust the
counts. 6b-iv produced twelve instances of a claim being restated instead of re-derived; 6b-v produced two
more of its own, both caught by somebody running a command and neither by its author — see §4.

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`12211d4`**, pushed; local and `origin` matched, working tree clean.
- **`main` is untouched** at `45495c1`, local and remote identical. Nothing in 6b-v went near it.
- One worktree — the main checkout. 6b-v's worktree was removed; its branch
  `dir/2026-09-14-pluralise-entity-namespaces` is retained locally and **deliberately not pushed**, as this
  project does with all nine merged session branches.
- Full solution suite: **908 passed, 0 failed** — `DungeonMaster.Tests` 788, `DungeonMaster.App.Tests` 97,
  `DungeonMaster.App.UnitTests` 23. Build **0 Warning(s), 0 Error(s)** across every TFM. Verified **in the
  main checkout after the merge**, not only in the session worktree. The total is unchanged from `af4c65f`:
  6b-v was a pure rename and added no test.
- 6b-v merged as **`c0542fd`**, a `--no-ff` merge commit with two parents and `HEAD^2 = d69601c`, matching the
  shape of the six session merges before it. Two follow-on commits on the base branch: `bdfea77` (the entry
  closure) and `12211d4` (session artifacts).
- 6b-v artifacts: `.dir-sdlc/sessions/2026-09-14-pluralise-entity-namespaces/` — `spec.md`, `manifest.json`,
  `coverage.md`, `contracts/` (4), `disciplines/` (4), `reviews/T1`-`T7`, **`artifacts/T3-ef-model-check.md`**,
  `log.md`, `state.json`.

### Index arithmetic — read this before citing any line number

**This session's offsets are unusually simple, and that is itself a hazard.**

| File | Lines at `af4c65f` | Now | Offset rule |
|---|---|---|---|
| `docs/audit/00-master-index.md` | 667 | 667 | **none — no line moved** |
| `docs/audit/01-domain-application.md` | 1323 | 1352 | **a citation at line ≥ 234 is now low by 29**; below 234, unchanged |
| `docs/audit/04-app-controls-and-theme.md` | 1226 | 1226 | none |
| `docs/audit/05-tests.md` | 2189 | 2189 | **untouched entirely** |
| `.dir-sdlc/backlog.md` | 274 | 294 | +20, all appended at the end |

Derived with `difflib` against `af4c65f`, not read off a diff summary. The whole of `01`'s growth is one
29-line amendment inserted at a single point; there is no second insertion anywhere in the file.

**Two warnings, and the second is the one that will bite.**

**Do not apply the table and consider the citation verified.** It tells you where a line *moved*, not whether
the claim at that line is still true. 6b-iv corrected eleven claims whose line numbers were fine and whose
content was wrong — including, this session, `CORE-004`'s own suggested direction, which was false when
written and had a perfectly good line number.

**`00-master-index.md` having a zero offset does not mean its citations are stable in general.** It grew
590 → 667 during 6b-iv. A citation written before 6b-iv is still off by up to 77 lines; a citation written
during or after 6b-iv is exact. The offset you need depends on when the citation was written, not on this
table alone.

---

## 2. What is open

176 entry rows in the index: **119 closed, 54 open, 3 part done.**

Open entries by layer — `CORE` **20**, `UI` 9, `UIKIT` 9, `TEST` 8, `INFRA` 6, plus 2 spanning two layers.
By severity — **S2 1**, S3 12, S4 31, S5 10.

**There is now exactly one open S2: `MASTER-176` (`:364`, UI/UIKIT)** — *the floor drag handle has no keyboard
route at all, so floor reordering is pointer-only.* 6b-v closed `MASTER-022`, which was the other one.

The three `part done` entries, which **must not be marked done**: `MASTER-094` (`:282`), `MASTER-098` (`:286`),
`MASTER-108` (`:296`).

**`MASTER-098` has been unblocked since 6b-iv and nothing has picked it up.** Its deferral premise — *"the
DragOver state is unverifiable while the drag handle is unreachable (MASTER-163)"* — was struck and recorded
void in 6b-iv, because the handle is now reachable and was human-observed taking a press. It is still
`part done` and still open. It has been deferred twice on a premise it no longer has, and 6b-v passed over it
a third time.

**The twelve open S3s, because the recommendation below turns on them:**

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

**Counted with the commands in §6.** Those commands are unchanged from the ones 6b-v used and still classify
correctly; nothing 6b-v wrote into a Status cell broke them.

---

## 3. The recommended session: finish the floor sidebar

This is a recommendation, not a decision — 6b-vi's Gate 1 owns the scope.

**`MASTER-176` + `MASTER-098` + the drop-placeholder backlog item.**

**Why now:**

- **It is the last open S2.** Every other open entry is S3 or below. The audit's remaining severity mass is
  gone after this one.
- **`MASTER-098` is unblocked and has been passed over three times.** It is `part done`, its blocker is void
  and recorded void, and it sits in the same four files.
- **The churn argument has weakened.** The reason 6b-v declined this cluster was that
  `FloorSidebarItem.xaml(.cs)` and `DungeonDetailPage.xaml(.cs)` had changed substantially in four
  consecutive sessions. 6b-v touched none of them, so it is now one session removed.
- **`MASTER-164` already gave a floor's areas a keyboard route.** Reordering is what remains unreachable
  without a pointer, and the frontend discipline requires every interactive element be keyboard-operable.

**The risk, stated plainly, because it is the reason this keeps being deferred.** The placeholder item
**cannot be built the obvious way.** Making the drop indicator always truthful means either mutating the
visual tree under the pointer mid-`DragOver` — `contracts/floor-reorder.md` §5 candidate #1 in 6b-iv's
session directory, the mechanism 6b-iv spent an entire task investigating and rejecting — or finding a
presentation that indicates position **without reparenting anything**. **That is a design decision, and it
belongs at Gate 1, not at implementation.** If Gate 1 cannot answer it, scope the session to `MASTER-176` and
`MASTER-098` and leave the placeholder in the backlog rather than discovering the problem in a task.

**The second risk: this cluster needs human observation.** 6b-iv ran three separate observation passes with
the user for work of this shape. 6b-v's evidence was a build log and cost nothing; this will not be that.
Budget for it at Gate 1 and read §4's trap 2 about observation configuration before writing any criterion
that depends on one.

### The legitimate alternative: the missing production invariants

**`MASTER-168`, `169`, `170`** — `Area.SetOutline` accepting a degenerate or null outline, `Floor` accepting a
zero or negative number, `PathSegment` permitting a `CubicBezier` with null control points. All CORE, all S3,
all filed by 6b-iii and unscheduled since.

- **They are one coherent cluster**: three missing invariants in the same layer, all about a constructor or
  setter accepting a value the domain should reject.
- **CORE is still the largest open layer at 20**, and 6b-v only took one entry out of it.
- **They are compiler- and test-verifiable**, with no human observation at all.

**The argument against, and it is the reason they were filed rather than fixed:** 6b-iii's escalation E2
recorded that **these are missing production invariants, not test gaps** — a test cannot assert behaviour that
does not exist, so each needs a design decision about what the behaviour *should be* before any test can
assert it. Three design decisions at one Gate 1 is a real load, and getting one wrong writes a wrong invariant
into the domain. Expect Gate 1 to be long, and expect to need the user on all three.

---

## 4. The inherited discipline — this is the part that matters

**Re-measure every entry against a fixed commit before decomposing. Decompose from the measurement, never from
the entry's prose.** 6b-iv did this and four of its five inherited entries changed disposition. **6b-v had one
entry and its prose was still wrong**: `CORE-004`'s "Suggested direction" argued for flattening the namespaces
on the grounds that per-entity namespaces are *"not required"* by the `dm-conventions` folder layout. That is
false — `dm-conventions/backend/dotnet-format.md:30` requires namespaces to match folder structure and `:58`
says folder structure mirrors the namespace hierarchy. **Had the session decomposed from the entry text it
would have taken the rejected direction and broken two convention rules.** The paragraph now carries an
in-place amendment classified MEASUREMENT ERROR.

### The finding 6b-v actually produced

**A positive control must fail the way the real defect would fail. Establishing that an instrument works is
not the same as establishing it can see the defect class under test.**

T3's criterion required injecting a defect into an EF Core model snapshot and confirming
`dotnet ef migrations has-pending-model-changes` noticed. The prescribed **single-site** injection **was
detected** — the criterion was satisfied, and a confident green was available and compliant. T3 instead
observed that changing one of three sites naming an entity type leaves `HasOne(…)`/`Navigation(…)` implicitly
defining a second, table-less entity type, so detection was **overdetermined by a structural split** rather
than by the name change. It probed the real edit's shape — all three sites renamed consistently — and got
**not detected**.

**`MigrationsModelDiffer` pairs entity types by mapped table, not by CLR name, so a consistent namespace
rename is invisible to it.** Everything about the setup was right and still misleading: the contract
established the instrument, the repo's own backlog recorded it working here, and the criterion mandated a
positive control. The control passed and proved nothing.

**The BEFORE control had already demonstrated the blindness at full scale and it was misread.** At BEFORE the
live model was fully plural while all 48 snapshot strings were still singular, and the check reported no
changes — a 48-occurrence, four-file demonstration, read at the time as "the command runs".

**Nothing automated covers those 48 strings** — not the build (string literals), not the differ, and not the
tests: `SqliteInMemoryTestBase.cs:33` uses `Database.Migrate()`, which since EF Core 9 (this repo is on
10.0.11) *does* load and diff the snapshot, but runs **that same differ**. What closed the question was static
and unplanned: the 48 occurrences are **four distinct strings**, each verifiable against a type that exists —
including `PlacedObject`, which belongs to `Floors` rather than to any `PlacedObjects` namespace, and is the
one spelling a mechanical "append an s" would have broken. **A reviewer found that while refusing to accept
the implementer's proposed remedy at face value.**

Full protocol: `artifacts/T3-ef-model-check.md` §3a–3d. Carried to the backlog as a proposal for a check that
compares entity types by CLR name.

### Two more restated-claim instances, both caught by a command

The pattern 6b-iv catalogued twelve times fired twice more, and **neither was a reasoning error**:

1. **A gap between two real numbers was filled by the nearest plausible item.** `05-tests.md` has 17 matches
   under a broad pattern and 16 under a strict one. The architect explained the delta with a short-form
   citation at `:1665` — but that line is matched by *neither* pattern, so it could not have caused the delta;
   the real 17th is a **glob in a table at `:13`** whose next path segment is `**`. The arithmetic
   `16 + 1 = 17` happened to work, which is why it survived. True total: **18**.
2. **A frozen contract asserted something about the manifest that the manifest did not say.** The architect
   wrote an `owns` entry and a contract sentence describing it in the same pass; when the `owns` entry was
   corrected, the sentence stayed and was false. It took its truth from intent rather than from the file.

**And one the orchestrator got wrong and a reviewer corrected.** A task reported two line counts that were
each exactly one too high. The orchestrator filed this as the restated-claim pattern. It is not:
`1353 = 1352 + 1` and `1227 = 1226 + 1` is the signature of `content.split('\n')` on a newline-terminated
file — a **second, independent measurement with a broken method**, a different defect class with a different
fix. **The fix is the method (`b.count(b'\n')`), not the discipline.** Mis-filing it would have put a wrong
category into the record the next session reads.

### Five specific traps, all of which fired in 6b-v

1. **`git diff` cannot see a rename-by-move, and the reviewer's diff is built from it.**
   `git diff <startRef> -- <owns>` after T1 showed **14 deletions and zero additions**, because the renamed
   directories were untracked. A reviewer handed that would correctly read it as the task deleting the
   `Entities` tree. **`git add -N` is not the fix** — a read must not grow into a write, and staging would
   corrupt the commit that follows. Tell the reviewer what its diff omits and point it at
   `git show <ref>:<old path>` for true before/after.
2. **Three criteria constrained what a task *reported* rather than what the repository contains.** "Show the
   arithmetic", "report all three commands", "derived from its own scan, not from the contract's table". A
   review dispatch cannot see a task report. All three reviewers correctly declined to attest and handed it to
   the orchestrator, who settled it from the held report and wrote the attestation into the review file.
   **The rule is easy to honour in its obvious form and easy to violate in the subtle one** — "verify the task
   showed its arithmetic" reads as a quality bar, not a cross-task dependency.
3. **Time-based attribution instruments do not work in a shared worktree.** A wall-clock `find -newermt`
   window could not separate one task's writes from five earlier tasks'; a file-mtime sweep returned 87 files
   and could attribute none. One replacement — `find -newer <own first edited file>` — *looked* sound but
   worked only because every peer's writes were already **committed**. **The decisive instrument is the git
   dirty set, which tasks cannot run**, so attribution is orchestrator work and should be stated as such in
   criteria rather than asked of a task.
4. **`dir-git`'s errors are in its reports, not its writes.** Nine dispatches, nine correct commits — exactly
   the right paths, exactly the supplied messages, including one made while a peer held uncommitted work and
   one while a peer had a deliberately corrupted file on disk. **Four wrong numbers in its reports**: 15 files
   for 14, a command described with literal `\n` that the commit did not contain, a `git status` with a
   duplicate and three omissions, and 23 distinct paths where its own stat said 22. **Give it verbatim
   instructions and verify the outcome with your own commands; do not read its summary as evidence.** Tell it
   to count distinct file paths, not stat lines — git's 50% rename threshold renders a small renamed file as a
   delete plus an add, which is what produced the 15-for-14.
5. **Defect injection is unsafe exactly when concurrency is in use.** Injection is the strongest evidence a
   reviewer can produce, and it is what made T1's and T5's reviews convincing. But the worktree is shared, and
   a deliberately broken intermediate state can corrupt a peer mid-build. Two reviewers reached this
   independently. **The workable forms are a scratch copy, or a positive control already run inside a frozen
   contract — with the substitution disclosed either way.**

### The structural one, still unfixed

**DIR tasks cannot invoke git, and a task report is not reachable from another task.** Both halves fired
again. The first is familiar: entry closures citing commit hashes are orchestrator work, and 6b-v's T7 was
explicitly forbidden from writing them even though it owned the index file.

The second cost nothing this session **only because the fix from 6b-iv was applied up front**: T3 wrote its EF
protocol into `artifacts/T3-ef-model-check.md` at an absolute main-checkout path, and T6 read that exact path.
**Persist the value into a session artifact the moment the task returns it**, and name the absolute path in
both tasks' criteria — a worktree-relative path resolves to a directory that does not exist.

**One new structural limit, found at Gate 3.** The contract-operations coverage check is a **call-site
search**, and it is inapplicable to contracts that declare rules rather than callable operations. 6b-v's four
contracts fix target strings, forbid edit shapes and record mechanics; none is an operation whose name could
appear at a call site. Running the search would have produced a meaningless `not called` on all 16 rows, or
incidental text matches dressed as call sites. It was **deliberately not run**, `Implementing task` left
empty by design, and the substitute evidence — per-criterion review verdicts — recorded as **weaker in kind**.
Expect the same if 6b-vi's contracts are rule-shaped.

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **294 lines** by `wc -l`. 6b-v appended **nine items**, in a new section at the end.

> **Before you count anything in that file: it contains 8 lone `\r` characters and they are not from 6b-v.**
> Byte offsets 60544–64238, all `\r\r\n` doubled-carriage-return sequences inside the
> `### For a later session -- from sub-session 6b-iv` header and its first items — introduced by 6b-iv's
> backlog append (`b8d6b2a`/`d2d9d3c`). Measured at `af4c65f`: **8 lone CRs before 6b-v, 8 after**; 6b-v's
> append added 20 `\r\n` pairs and zero lone CRs.
>
> **The consequence is that two reasonable line-count methods disagree by 8.** `wc -l` and `b.count(b'\n')`
> both give **294**; Python's text-mode iteration gives **302**, because universal-newline handling splits on
> a lone `\r` as well. Neither is wrong — they are counting different things. **State which one you used**, and
> do not treat a disagreement as a restated-claim error; this is the *broken-method* class from §4, in a third
> variant. Fixing the eight sequences is a one-line repair nobody has scheduled.

The nine items:

1. **Give the next namespace rename an instrument that can actually see it** — a test comparing the last
   migration's `TargetModel` entity-type names to `context.Model.GetEntityTypes()` **by CLR name rather than
   by table**. The check the differ structurally cannot perform.
2. **No automated check covers the 48 EF snapshot strings**, measured rather than assumed. A future session
   editing them must redo the four-distinct-string cross-check by hand until item 1 exists.
3. **`content.split('\n')` is off by one on every newline-terminated file** — a broken method, not a restated
   claim.
4. **File-mtime and `find -newer` cannot attribute writes in a shared worktree** — attribution is orchestrator
   work.
5. **Three criteria constrained what a task reported** — sharpen the rule's wording; encode orchestrator
   attestation as expected procedure.
6. **Criteria cite contract paths as if worktree-relative** — fix in the criteria, not just in dispatch
   prompts.
7. **The Gate 3 contract-operations check does not apply to rule-shaped contracts.**
8. **The harness `sed` instruction contradicts the contract's `sed -i` prohibition** — orchestrator-owned; a
   known harness/contract conflict should be named in every dispatch. T5 found it at implementation time and
   disclosed it; six earlier tasks met it silently.
9. **The test-project mirror folders now differ from Domain, deliberately** — `Tests/Domain/Entities/{Area,
   Dungeon,Floor}/` stay singular. Correct for 6b-v, but the two trees now read differently for a reason
   visible from neither.

**From 6b-iv, still unscheduled:** the drop-placeholder indicator (the user's own observation — see §3),
`AreaSidebarItem`'s two `PointerGestureRecognizer`s needing one hover re-observation, the truncated floor name
with no recovery affordance, and the Appium size ratchets asserting DIU thresholds against physical-pixel
measurements.

**Six entries filed by 6b-iii at ranks 168-173 remain unscheduled**, and **168-170 are the missing production
invariants** — see §3's alternative.

**Three 6b-iii backlog items also remain**: the elided command at `docs/audit/05-tests.md:273`, the inference
stated as a measurement at `:2040-2041`, and `IUnitOfWorkFactory.CreateAsync`'s documented
`OperationCanceledException` being unproven by any test.

**`dm-conventions` still teaches the pattern 6b-v removed** — `backend/dotnet-format.md:567` and `:588` use
`namespace DmProject.Domain.Entities.Dungeon;` containing `class Dungeon`. It is a separate repository and was
deliberately not modified. Filed, not scheduled.

---

## 6. Commands to re-derive §2 yourself

These are **unchanged from the ones 6b-v used** and still classify every row; nothing 6b-v wrote into a Status
cell broke them. Re-run rather than trusting §2.

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
print("\nopen S2:");     [print(f"  :{r['line']}  {r['id']}  [{r['layers']}]  {r['title'][:70]}") for r in op if r["sev"]=="S2"]
print("\npart done:");   [print(f"  :{r['line']}  {r['id']}") for r in rows if r["state"]=="part done"]
PY
```

**The column index is `[10]`, and this is worth knowing rather than copying.** Splitting a row on `|` yields a
leading empty cell and a trailing empty cell; `Status` is `[10]` and `[11]` is the trailing empty. An
`awk -F'|'` doing the same job uses `$11`, because awk is 1-based. Writing the 6b-v handoff produced a wrong
survey from exactly that off-by-one — it reported all 176 rows open.

If any row prints `UNCLASSIFIED`, **read the cell before deciding what it means.** That is the whole point of
the bucket.

---

## 7. Mechanics

- **Worktree root must be short.** Use `C:/dm-wt/6b-vi`. This is a MAUI/WinUI solution and a long path breaks
  the WinUI XAML compiler with `MAX_PATH` errors; the scratchpad path is too long. `C:/dm-wt` exists and is
  empty.
- **Tracked files are CRLF and `IDE0055` is a build error.** Editing a tracked file with `sed -i` rewrites it
  to LF and fails the build on every line. It fired twice in 6b-iv. **The session's own harness instruction
  tells agents to edit via Bash `sed`** — that conflict is real, it is backlog item 8, and **you should name
  it in every dispatch** rather than relying on each criterion to re-forbid it. 6b-v's T5 found it at
  implementation time and disclosed it; six earlier tasks met it silently.
- **`grep -c $'\r$'` returns a confident `0` on a correctly-CRLF file in this environment.** Count in binary —
  `b.count(b'\r\n')` versus `b.count(b'\n')`.
- **The folder move must be a filesystem rename (`mv`), not a write-out at the new path** — a hand-written
  file re-encodes line endings. Edit contents in place first, then move.
- **Close any running `DungeonMaster.App` before building the Windows TFM.** A stale instance holds
  `DungeonMaster.App.Core.dll` and produces `MSB3026`/`MSB3027`. It bit three agents in 6b-iv; it bit none in
  6b-v because every dispatch checked first.
- **Verify in the main checkout after merge**, not only in the worktree. 6b-v checked 358 files and found zero
  inconsistent line endings, but a clean worktree proves nothing — in 6b-iv `.dir-sdlc/backlog.md` was CRLF in
  the worktree and LF-only in the main checkout.
- **Tasks share one worktree and one `obj/`/`bin/`.** 6b-v ran a strict serial chain except for one genuinely
  concurrent pair (T3 ∥ T4 was *not* run concurrently; T2 ∥ T7 and the T3-review ∥ T4 pair were, and T7 ran no
  `dotnet` command at all). **Two tasks that both build share four upstream projects through one `obj/`** —
  serialize them; the parallelism is on paper.
- **The Appium harness works and all its prerequisites are installed** — Appium 3.7.0, `windows@6.1.1`,
  WinAppDriver `1.2.200902003-release`, `Appium.WebDriver` 8.3.2 pinned at `Directory.Packages.props:17`,
  Developer Mode on. **The server is not running**; start it with `appium --address 127.0.0.1 --port 4723`.
  `src/DungeonMaster.App.UiTests` is deliberately **outside** `src/DungeonMaster.sln`; do not add it.
- **This session's model tiers:** implementers, reviewer and architect on `claude-opus-5`; `dir-git` on
  `claude-haiku-4-5`. There is still no `.dir-sdlc/config.json`; those are the documented defaults.
- **Gate 3 has no slot for orchestrator-authored repo content.** Entry closures need commit hashes, so no task
  can write them, but the session branch carries task commits only. 6b-v resolved this as two commits on the
  base branch after the merge — `docs/audit/` first (`bdfea77`), then `.dir-sdlc/` (`12211d4`). Reuse that
  shape.
- **There is no project-level `.dir-sdlc/log.md`** and 6b-v did not create one. Each session's `log.md` lives
  in its own session directory, which is the convention all seven sessions follow.
