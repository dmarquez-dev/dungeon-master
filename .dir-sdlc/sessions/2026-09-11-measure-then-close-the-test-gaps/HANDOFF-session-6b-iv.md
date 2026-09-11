# Handoff to session 6b-iv

Written at the close of 6b-iii, 2026-09-11. Self-contained: everything 6b-iv needs is here or at a path
named here. Nothing in it depends on the conversation that produced it.

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`d88b566`**, local and `origin` matched, working tree clean.
- One worktree — the main checkout at `C:/Users/dmarq/Development/Repos/dungeon-master`. 6b-iii's worktree
  was removed; its branch `dir/2026-09-11-measure-then-close-the-test-gaps` is retained locally, unpushed.
- Full solution suite: **866 passed, 0 failed**. Build **0 Warning(s), 0 Error(s)**. Verified in the main
  checkout after merge, not only in the session worktree.
- 6b-iii artifacts: `.dir-sdlc/sessions/2026-09-11-measure-then-close-the-test-gaps/`. Its `measurement.md`,
  `handover.md` and `handover-addendum.md` are the evidence base for everything it claimed.

**Index arithmetic, read this before citing any line number.** 6b-iii grew `docs/audit/00-master-index.md`
from **568 to 590 lines**. Any citation of the form `00-master-index.md:NNN` written before 6b-iii and
pointing at **line 355 or later is low by 14**; at **558 or later, low by 22**. The inheritance note that
previous handoffs cite as `:522` is now at **`:536`**. A sweep for that citation form across `docs/audit/`
and `.dir-sdlc/sessions/` is the first backlog item.

## 2. What is open

173 entry rows in the index: **111 done, 58 open, 3 part done, 1 not done.**

Open entries by layer — `CORE` 21, `UIKIT` 10, `UI` 10, `TEST` 8, `INFRA` 6, plus 3 spanning two layers.
By severity — **S2 3**, S3 14, S4 31, S5 10.

Counted with:

```
awk -F'|' '/^\| *[0-9]+ *\| *MASTER-/ {s=$11; gsub(/^ +| +$/,"",s); ...}' docs/audit/00-master-index.md
```

(full commands in §6 below, runnable as written).

**The three open S2 entries are `MASTER-022`, `MASTER-163` and `MASTER-164`.** Two of the three are in the
cluster proposed next.

## 3. The recommended session: the floor sidebar is not operable

This is a recommendation, not a decision — 6b-iv's Gate 1 owns the scope. But it is the strongest theme
available, and it is the only one that **unblocks an entry another session had to defer**.

Five entries were deliberately excluded from Session 6 at its Gate 1, on the stated ground that folding
sidebar/UI defects into a test-suite session is the theme collision that split Session 5. They have never
been scheduled since. They are coherent as a group:

| Entry | Sev | Layer | Effort | What |
|---|---|---|---|---|
| `MASTER-163` | **S2** | UI, UIKIT | XS | The floor drag handle sits under WinUI's overlay scrollbar, so floor reordering **cannot be performed at all** |
| `MASTER-164` | **S2** | UI, UIKIT | S | The floor card body and its expand toggle take no focus, so **no floor's areas can be opened by keyboard** |
| `MASTER-161` | S3 | UIKIT | XS | `FloorSidebarItem`'s card body is tappable but carries no `HandCursorBehavior`. Status **`not done` (5c)** — the behavior was attached and had **no observed effect** |
| `MASTER-165` | S3 | UIKIT | XS | `HandCursorBehavior` takes effect on `Button` elements and **not** on the layouts and labels it is also attached to |
| `MASTER-166` | S3 | UI | S | The optimistic ghost frame `AddGhostArea` exists to paint was **never observed reaching the screen** |

**Closing `MASTER-163` unblocks `MASTER-098`.** That entry's status reads, verbatim:

> `part done (5c) -- the DragOver state is unverifiable while the drag handle is unreachable (MASTER-163), deferred to a later session`

So a sixth entry moves as a consequence, without being scheduled.

**One hypothesis worth testing early, stated as a hypothesis.** `MASTER-165` says `HandCursorBehavior` works
on `Button` and not on layouts or labels. `MASTER-161` says that behavior was attached to a card body and had
no observed effect. **`165` may be the root cause of `161`** — in which case `161` is not a separate fix and
should close as a consequence of `165`, not as its own task. This was **not** verified by 6b-iii; it is a
reading of two entry texts. Measure it before decomposing, and if it holds, say so in the decomposition
rather than writing two tasks that are one.

### Why this cluster and not the others

`CORE` is the largest open layer at 21, and `MASTER-022` is the third open S2 — entity namespaces sharing
their type's name, forcing `Floor.Floor` qualification. It is a legitimate alternative theme. It is also a
wide mechanical rename touching every layer at once, which is a different kind of session; the sidebar
cluster is bounded, user-visible, and has two S2 entries plus an unblock.

## 4. The inherited discipline — this is the part that matters

**Re-measure every entry against a fixed commit before decomposing. Decompose from the measurement, never
from the entry's prose.** 6b-iii did this and the result was stark: of its five inherited entries, **only one
described work that still needed doing as written**. One was already closed. One's headline claim was false.
One had two clauses that were never true — including one naming as absent a test that the record's *own first
cited location* points at. One prescribed a remedy that a later session had measured as a regression.

The sidebar cluster is **more** exposed to this than 6b-iii's set was, for a specific reason: `MASTER-161` is
marked `not done` because a remedy shipped and the user observed it ineffective, and `MASTER-166` and
`MASTER-098` rest on things "never observed" or "unverifiable". **Those are claims about the running
application, and no amount of source reading settles them.** Plan for that at Gate 1 — either a real run, or
an explicit statement in the spec that a criterion rests on user observation and cannot be self-verified.

**Three specific traps, all of which fired in 6b-iii:**

1. **Grep shapes lie.** A measurement agent ran `grep -rn "RotateDirection" src/` → 0 lines and concluded the
   type was dead. The file is `RotateDirection.cs`; the type it declares is `RotationDirection`, with five live
   references. Never put a bare grep count in a criterion; prefer a command whose failure is a build error, or
   enumerate `file:line` and cite the list.
2. **Counts swing with build state.** A frozen contract cited `grep -rn "GreaterThanZeroConverter" src/DungeonMaster.App/`
   as "7 lines". It returns **15** bare, 10 excluding `obj`, 8 excluding `obj` and `bin`, 7 for XAML only.
   The substance was right and the instrument was not.
3. **`dir-git` substitutes plausible commit hashes.** Handed `413be78`, it wrote `850a10b` — a different
   session's reference commit that happened to be circulating. Give `dir-git` the identifiers verbatim with an
   explicit "do not substitute any hash you have seen elsewhere", and read its commit messages before moving on.
   Also: it **fast-forwards** `merge-session`. The last five session merges are `--no-ff` merge commits
   (`06c36ba`, `7684c5e`, `ce4113d`, `45bab59`, `a56e062`); check `git log --merges` after merging and reshape
   while nothing is pushed.

**And the structural one.** DIR tasks cannot invoke git. Any criterion whose honest answer needs history —
*was this true at commit X* — is unanswerable by the task that owns it. 6b-iii's record-correction task wrote
`UNDETERMINED` five times and was right to; the orchestrator then settled all five with six read-only commands
in under a minute. **Resolve history questions yourself before dispatch and put the commands and outputs in the
evidence package.**

## 5. Carried-forward work

Four items were appended to `.dir-sdlc/backlog.md` at 6b-iii's Gate 3. Read the file for the full text; in
brief:

1. **Sweep for stale `00-master-index.md:NNN` citations** — see §1. Affects 6b-iv's own reading directly.
2. **`docs/audit/05-tests.md:273`** quotes a command with an elided path (`src/.../Validators/`) that cannot be
   pasted, against the document's own standard at `:88-92`.
3. **`docs/audit/05-tests.md:2040-2041`** states an inference as a measurement — "it did not move in between",
   from two measured endpoints.
4. **`IUnitOfWorkFactory.CreateAsync`'s documented `OperationCanceledException` is unproven by any test.**
   Recommended to become a numbered entry alongside `MASTER-172`/`173` rather than stay a backlog line.

**Six entries were filed by 6b-iii at ranks 168–173 and none is scheduled:** `168` `Area.SetOutline` accepts a
degenerate or null outline; `169` `Floor` number accepts zero and negatives; `170` `PathSegment` permits
`CubicBezier` with null control points; `171` the `RotateDirection.cs` / `RotationDirection` name mismatch;
`172` and `173` the two dispatcher residuals. **168–170 are missing production invariants, not test gaps** —
each needs a design decision about what the behaviour *should* be before any test can assert it, which is
exactly why 6b-iii filed rather than fixed them.

**Three other entries are part done and must not be marked done:** `MASTER-094` and `MASTER-108` from 5b,
`MASTER-098` from 5c (the one the sidebar cluster would unblock).

**`docs/audit/05-tests.md` remains 22-of-32 unchecked.** 6b-ii checked six records, 6b-iii checked four
(`TEST-001`, `TEST-019`, `TEST-020`, `TEST-032`). The file's own preamble now states this. Unchecked is not
confirmed.

## 6. Commands to re-derive §2 yourself

Do not trust the counts above without re-running these — that is the whole discipline.

```bash
# total entry rows
grep -cE '^\| *[0-9]+ *\| *MASTER-' docs/audit/00-master-index.md

# status breakdown
awk -F'|' '/^\| *[0-9]+ *\| *MASTER-/ {s=$11; gsub(/^ +| +$/,"",s);
  if (s=="") print "OPEN"; else if (s ~ /\*\*done\*\*/) print "done";
  else if (s ~ /part done/) print "part done"; else if (s ~ /not done/) print "not done";
  else print "other"}' docs/audit/00-master-index.md | sort | uniq -c | sort -rn

# open entries by layer
awk -F'|' '/^\| *[0-9]+ *\| *MASTER-/ {s=$11; gsub(/^ +| +$/,"",s);
  if (s=="") {l=$5; gsub(/^ +| +$/,"",l); print l}}' docs/audit/00-master-index.md | sort | uniq -c | sort -rn

# open S2 entries
awk -F'|' '/^\| *[0-9]+ *\| *MASTER-/ {s=$11; v=$4; gsub(/^ +| +$/,"",s); gsub(/^ +| +$/,"",v);
  if (s=="" && v=="S2") {id=$3; t=$9; gsub(/^ +| +$/,"",id); gsub(/^ +| +$/,"",t); print id": "t}}' \
  docs/audit/00-master-index.md
```

## 7. Mechanics

- **Worktree root must be short.** Use `C:/dm-wt/6b-iv`. This is a MAUI/WinUI solution and a long path breaks
  the WinUI XAML compiler with MAX_PATH errors; the scratchpad path is too long.
- **Tracked files are CRLF and `IDE0055` is a build error.** Editing a tracked file with `sed -i` rewrites it
  to LF and fails the build on every line of that file. Use the editing tools. This cost two tasks time in
  6b-iii.
- **Tasks share one worktree and one `obj/`/`bin/`.** 6b-iii serialized its five tasks for that reason —
  concurrent `dotnet test` runs contend and fail intermittently, which is the unmeetable-criterion class this
  repo has lost sessions to. Ownership disjointness is necessary but not sufficient.
- **Verify in the main checkout after merge**, not only in the worktree. Line-ending state diverges invisibly.
- **This session's model tiers:** implementers, reviewer and architect on `claude-opus-5`; `dir-git` on
  `claude-haiku-4-5`. There is no `.dir-sdlc/config.json`; those are the documented defaults.
