# Handoff to session 6b-v

Written at the close of 6b-iv, 2026-09-14. Self-contained: everything 6b-v needs is here or at a path
named here. Nothing in it depends on the conversation that produced it.

**Every number below was re-derived from the repository at `d2d9d3c` immediately before writing, not
carried from the session that produced it.** That is not ceremony. 6b-iv produced **twelve** instances
of a claim being restated instead of re-derived, from twelve different authors, five of them the
orchestrator's — see §4. A handoff asking to be believed would be the wrong artifact, and §6 gives you
the commands to re-run the survey rather than trust the counts.

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`d2d9d3c`**, local and `origin` matched, working tree clean.
- **`main` is untouched** at `45495c1`, local and remote identical. Nothing in 6b-iv went near it.
- One worktree — the main checkout at `C:/Users/dmarq/Development/Repos/dungeon-master`. 6b-iv's
  worktree was removed; its branch `dir/2026-09-11-make-the-floor-sidebar-operable` is retained
  locally and **deliberately not pushed**, as this project does with merged session branches.
- Full solution suite: **908 passed, 0 failed** — `DungeonMaster.Tests` 788, `DungeonMaster.App.Tests`
  97, `DungeonMaster.App.UnitTests` 23. Build **0 Warning(s), 0 Error(s)**. Verified **in the main
  checkout after the merge**, not only in the session worktree. It was 866 at `b3b1641`, so 6b-iv added
  42 tests, all on paths that had none.
- 6b-iv merged as **`f88bbe8`**, a `--no-ff` merge commit with two parents and `HEAD^2 = 3448ecd`,
  matching the shape of the five session merges before it.
- 6b-iv artifacts: `.dir-sdlc/sessions/2026-09-11-make-the-floor-sidebar-operable/` — `spec.md`,
  `measurement.md`, `coverage.md`, `manifest.json`, `contracts/`, `disciplines/`, `reviews/T1`-`T6`,
  **`observations/`** (three human-observation transcripts), **`artifacts/`** (the preserved latency
  probe), `log.md`, `state.json`.

### Index arithmetic — read this before citing any line number

6b-iv grew `docs/audit/00-master-index.md` from **590 to 667 lines**. The growth is **not** uniform:
almost all of it lands in the `MASTER-161`-to-`MASTER-166` detail block. Derived with `difflib` against
`b3b1641`, not read off a diff summary:

| An old citation at line ≥ | is now low by |
|---|---|
| 362 | 3 |
| 368 | 5 |
| 568 | 13 |
| 570 | 17 |
| 571 | 35 |
| 572 | 47 |
| 573 | 71 |
| 583 | **77** |

`docs/audit/04-app-controls-and-theme.md` grew 1223 → 1227: **≥408 add 2, ≥499 add 4.**

No other `docs/audit/` file was touched. **`docs/audit/05-tests.md` is untouched** — including its
preamble at `:129` still recording that 22 of its 32 records are unchecked.

**Do not apply this table and consider the citation verified.** It tells you where a line *moved*, not
whether the claim at that line is still true. 6b-iv corrected eleven claims whose line numbers were
fine and whose content was wrong.

---

## 2. What is open

176 entry rows in the index: **118 closed, 55 open, 3 part done.**

Open entries by layer — `CORE` **21**, `UI` 9, `UIKIT` 9, `TEST` 8, `INFRA` 6, plus 2 spanning two
layers. By severity — **S2 2**, S3 12, S4 31, S5 10.

**The two open S2 entries are `MASTER-022` (`:210`, CORE) and `MASTER-176` (`:364`, UI/UIKIT).**
6b-iv closed `MASTER-163` and `MASTER-164`, the other two S2s it inherited, and filed `MASTER-176`
itself.

The three `part done` entries, which **must not be marked done**: `MASTER-094` (`:282`),
`MASTER-098` (`:286`), `MASTER-108` (`:296`).

**`MASTER-098` changed status in a way that matters.** Its deferral premise — *"the DragOver state is
unverifiable while the drag handle is unreachable (MASTER-163)"* — is struck through and recorded void,
because the handle is now reachable and was human-observed taking a press first time. **It is still
`part done` and still open**; only the premise was struck. It has been deferred twice on that premise
and no longer has it.

**Counted with the commands in §6, which are NOT the ones the previous handoff carried** — see that
section for why the old ones now mis-report.

---

## 3. The recommended session: `MASTER-022`, the `Floor.Floor` namespaces

This is a recommendation, not a decision — 6b-v's Gate 1 owns the scope.

`MASTER-022` (`:210`, S2, CORE, effort M, risk medium): *entity namespaces share the name of the type
they contain, forcing `Floor.Floor` qualification.* It has been named as the alternative in at least
the last two handoffs and passed over both times.

**Why now, and why this rather than more sidebar work:**

- **`CORE` is by far the largest open layer at 21**, and nothing in 6b-iii or 6b-iv touched it. The
  audit's open mass is drifting into one layer while adjacent layers get worked.
- **It is compiler-verifiable end to end.** A namespace rename either builds or does not. After 6b-iv,
  where three of six tasks carried human-observation criteria and the orchestrator ran three separate
  observation passes with the user, a session whose evidence is `0 Warning(s), 0 Error(s)` is a
  genuinely different and much cheaper rhythm.
- It is one of only two open S2s, and the other one is the sidebar work you would otherwise be
  continuing.

**The risk, stated plainly, because it is the reason this keeps being deferred.** A namespace rename
touches every layer at once, which makes **ownership disjointness the whole problem**. DIR forbids two
concurrent tasks whose `owns` globs intersect, and a rename that sweeps `Domain`, `Application`,
`Infrastructure`, `App.Core`, `App` and four test projects will not decompose into disjoint sets by
feature. Expect to decompose **by project or by directory**, and expect a mostly serial chain. Do not
let an architect hand you two concurrent tasks that both touch `using` lines in the same project.

### The legitimate alternative: finish the floor sidebar

`MASTER-176` + `MASTER-098` + the placeholder-indicator backlog item is a coherent cluster, and a
defensible choice:

- `MASTER-176` is the **last pointer-only operation in the sidebar**. 6b-iv's `MASTER-164` gave
  keyboard access to a floor's areas; reordering is what remains unreachable without a pointer, and the
  frontend discipline requires every interactive element be keyboard-operable.
- `MASTER-098` is unblocked for the first time and is `part done`.
- The placeholder item is the user's own finding from 6b-iv's observation pass.

**Two arguments against taking it now.** First, `FloorSidebarItem.xaml`, `FloorSidebarItem.xaml.cs`,
`DungeonDetailPage.xaml` and `DungeonDetailPage.xaml.cs` all changed substantially in 6b-iv — four
consecutive sessions in the same four files invites churn, and every line citation in the entries you
would be working from is now stale (§1). Second, **the placeholder item cannot be built the obvious
way**: making the indicator always truthful means either mutating the visual tree under the pointer
mid-`DragOver` — `contracts/floor-reorder.md` §5 candidate #1, the mechanism 6b-iv spent a whole task
investigating — or finding a presentation that indicates position **without reparenting anything**.
That is a design decision that wants making at Gate 1, not discovered at implementation.

---

## 4. The inherited discipline — this is the part that matters

**Re-measure every entry against a fixed commit before decomposing. Decompose from the measurement,
never from the entry's prose.** 6b-iv did this and **four of its five inherited entries changed
disposition**: one had already been fixed and only its record was wrong; one's central claim was false;
one was *worse* than recorded rather than milder; one's wiring half was false and four tests it was
described as lacking already existed. **Only one of five survived its own prose.** Decomposing from the
entry text would have produced tasks that fixed working code.

### The finding 6b-iv actually produced

**Twelve instances of one failure: a claim about a file restated from another statement about that
file, instead of re-derived from the file.** Twelve different authors — an Explore agent, `dir-git`, a
frozen contract, an implementer, a reviewer, and the orchestrator five times. Full enumeration in
`log.md`.

**None was a reasoning error. None was caught by its author. Every one was caught by somebody running
a command.**

The mechanism, diagnosed by the task that committed instance eight and verified by its reviewer:

> **A narrow window plus recall is not a read** — the recalled feature may be true of the window and
> false of the line, or may be imported onto a line the window never showed.

That author had run two real commands against the right file and got the load-bearing answer right both
times. The failure was that **the span read and the span cited were different spans, and nothing forced
them to be the same.** That is why it survives careful agents.

**The practice that caught all twelve: name the command and the span beside the claim.** The 6b-iv
amendments in the index do this and are the worked example. `.dir-sdlc/backlog.md` carries a proposal
to make it mechanical, with three ranked implementations — and records that the older manifest-lint
proposal, useful as it is, would have caught **2 of the 12**.

The numeric twin, from instance ten: **add the column up.** A count asserted instead of summed is the
same failure wearing different clothes.

### Five specific traps, all of which fired in 6b-iv

1. **A criterion can be unsatisfiable from the files its own task owns — twice.** T2 needed a theme
   token in `Tokens.xaml`; T3 needed `DungeonDetailPage.xaml.cs` because **XAML event handlers resolve
   against the code-behind**, and its criterion asserted in so many words that owning the `.xaml`
   sufficed. Both escalated instead of writing outside their set, which was right both times. **At
   decomposition, check that each criterion's remedy is *reachable* from the ownership it grants — not
   merely that the glob sets are disjoint.**
2. **The observation configuration can invalidate the observation.** Every early 6b-iv observation ran
   maximized at 1920x1080, where all 11 floors fit and **no scrollbar renders at all** — the wrong
   configuration for testing a scrollbar-gutter remedy. The one that mattered ran at 900x640 where the
   sidebar actually scrolls. **State the window configuration in the protocol, and state what makes it
   the right one.**
3. **Instruments measure the apparatus.** Synthetic `mouse_event` does **not** initiate WinUI
   drag-and-drop (the source card stays opaque, proving `OnDragStarting` never fired). Appium
   **refuses** a W3C mouse pointer source outright — *"Currently only pen and touch pointer input
   source types are supported"* — so it can never drive this reorder; that is now measured, in
   `contracts/floor-sidebar-automation-surface.md` §3. Cursor shape is unreadable cross-process. And a
   first-match UIA focus reader reported a featureless pane for 32 consecutive tabs, making a **working**
   keyboard walk look dead. **Before freezing a criterion on an instrument, establish the instrument
   can see the thing.**
4. **`dir-git` invents plausible detail.** On T1's commit it wrote an invented member name
   (`_dropTarget_Drop`, which exists nowhere), the wrong handler name, and a wrong file count — in one
   message. **Supply commit messages verbatim and instruct it not to re-infer them**; every later 6b-iv
   commit did this and none had a defect. Also check the merge shape: it has fast-forwarded session
   merges in this project before.
5. **Serialize `dir-git` in time, not just through the orchestrator.** 6b-iv dispatched `cleanup` and a
   `commit` whose lifetimes overlapped. No collision occurred — `git fsck` clean, no `index.lock` — but
   the rule exists for a race that is intermittent by nature, and routing both through the orchestrator
   is not serialization if their dispatches overlap.

### The structural one, still unfixed

**DIR tasks cannot invoke git, and a task report is not reachable from another task.** The first half is
familiar: entry closures citing commit hashes are orchestrator work, and 6b-iv's `T6` was explicitly
forbidden from writing them.

The second half cost 6b-iv a review round and two `UNDETERMINED` criteria. **Any criterion of the form
"the report states X" is unverifiable by the reviewer and unreadable by a downstream task**, because
task reports live in the orchestrator's context and only findings are persisted. It bit three times:
`13x19` existed only in a report; T3's handoff sentence existed only in a report; and T3's `c6`/`c9`
went undetermined because the orchestrator did not supply the report to the reviewer.

**The fix that worked: persist the sentence into a session artifact the moment the task returns it.**
6b-iv's `observations/T3-sidebar.md` has a "Handoff to T5" section holding exactly the number T5 was
required to assert, for exactly this reason.

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **67 items**. 6b-iv appended **five**, in a new section at `:257`:

1. **The drop placeholder does not always indicate where the card will land.** The user's own
   observation. Behaviour is correct; the affordance is not. **The naive fix reintroduces the one
   pattern this session has the most evidence against** — see §3.
2. **`AreaSidebarItem`'s `ItemBorder` now hosts two `PointerGestureRecognizer`s** and the hover
   highlight was not re-observed after `HandCursorBehavior` joined `HoverOrLongPressBehavior`. One
   glance settles it.
3. **A truncated floor name has no recovery affordance.** The name column narrowed 160 → 99 across
   6b-iv; the file's own `Button`s already use `ToolTipProperties.Text`.
4. **The Appium size ratchets assert DIU thresholds against physical-pixel measurements**, which at
   1.25 scale means a regression to 20 DIU would pass where the contract pins 23.
5. **The restated-claim proposal, widened to fit its evidence** — see §4.

**Six entries filed by 6b-iii at ranks 168-173 remain unscheduled**, and **168-170 are missing
production invariants, not test gaps** — each needs a design decision about what the behaviour *should*
be before any test can assert it.

**Three 6b-iii backlog items also remain**: the elided command at `docs/audit/05-tests.md:273`, the
inference stated as a measurement at `:2040-2041`, and `IUnitOfWorkFactory.CreateAsync`'s documented
`OperationCanceledException` being unproven by any test.

---

## 6. Commands to re-derive §2 yourself — and why the old ones no longer work

**The survey commands the previous handoff carried now mis-report.** 6b-iv's `T6` wrote rich status
text into several cells — `**FALSIFIED** (6b-iv) …`, `filed found-and-fixed (6b-iv); **not an open
item.**`, `**OPEN.** Filed, not fixed …` — and the old `awk` classifier drops all three into an
`other` bucket, so it under-counts closed entries and mis-states which are open. Re-running it verbatim
will give you three unexplained rows and a wrong total.

Use this instead. It classifies on the text rather than on emptiness alone, and it prints anything it
cannot classify rather than silently bucketing it:

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

**The column index is `[10]`, and this is worth knowing rather than copying.** Splitting a row on `|`
yields a leading empty cell and a trailing empty cell; `Status` is `[10]` and `[11]` is the trailing
empty. An `awk -F'|'` doing the same job uses `$11`, because awk is 1-based. **Writing this handoff
produced a wrong survey from exactly that off-by-one** — it reported all 176 rows open — which is
instance thirteen of §4's pattern and was caught by printing the cells instead of assuming the offsets.

If any row prints `UNCLASSIFIED`, **read the cell before deciding what it means.** That is the whole
point of the bucket.

---

## 7. Mechanics

- **Worktree root must be short.** Use `C:/dm-wt/6b-v`. This is a MAUI/WinUI solution and a long path
  breaks the WinUI XAML compiler with MAX_PATH errors; the scratchpad path is too long. `C:/dm-wt`
  already exists.
- **Tracked files are CRLF and `IDE0055` is a build error.** Editing a tracked file with `sed -i`
  rewrites it to LF and fails the build on every line. This fired **twice** in 6b-iv — once on a
  reviewer, once on an implementer whose new file came out LF-only and produced a wall of `IDE0055`.
  Use the edit tools.
- **`grep -c $'\r$'` returns a confident `0` on a correctly-CRLF file in this environment.** A 6b-iv
  reviewer found this while auditing line endings. **Count in binary** — `b.count(b'\r\n')` versus
  `b.count(b'\n')`.
- **Close any running `DungeonMaster.App` before building the Windows TFM.** A stale instance holds
  `DungeonMaster.App.Core.dll` and produces `MSB3026`/`MSB3027`. This bit three agents in 6b-iv.
- **Verify in the main checkout after merge**, not only in the worktree. In 6b-iv `.dir-sdlc/backlog.md`
  was CRLF in the worktree and **LF-only in the main checkout** before the merge; line-ending state
  diverges invisibly and a clean worktree proves nothing.
- **Tasks share one worktree and one `obj/`/`bin/`.** Concurrent `dotnet test` runs contend. 6b-iv ran
  a strict serial chain T1→T2→T3→T4 then T5∥T6, and the only concurrent pair had disjoint globs with
  one of them running no `dotnet` command at all.
- **The Appium harness works and all its prerequisites are installed on this machine** — Appium 3.7.0,
  `windows@6.1.1`, WinAppDriver `1.2.200902003-release`, `Appium.WebDriver` 8.3.2 pinned at
  `Directory.Packages.props:17`, Developer Mode on. **The server is not running**; start it with
  `appium --address 127.0.0.1 --port 4723` and stop it when done. `src/DungeonMaster.App.UiTests` is
  deliberately **outside** `src/DungeonMaster.sln`; do not add it.
- **This session's model tiers:** implementers, reviewer and architect on `claude-opus-5`; `dir-git` on
  `claude-haiku-4-5`. There is still no `.dir-sdlc/config.json`; those are the documented defaults.
- **Gate 3 has no slot for orchestrator-authored repo content.** Entry closures need commit hashes, so
  no task can write them, but the session branch is meant to carry task commits only and the artifact
  commit is meant to stage only `.dir-sdlc/` paths. 6b-iv resolved this as two commits on the base
  branch after the merge — `docs/audit/` first, then `.dir-sdlc/`. Reuse that shape.
