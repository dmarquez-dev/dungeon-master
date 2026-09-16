# Contract C2 — editing, building and counting in this repository

Draft for Gate 2. Written by `dir-architect` in decomposition mode. **Binds every task in this
session, and every reviewer.** It declares rules, not callable operations.

This file's absolute main-checkout path is
`C:/Users/dmarq/Development/Repos/dungeon-master/.dir-sdlc/sessions/2026-09-16-floor-sidebar-keyboard-route/contracts/C2-editing-build-and-counting-rules.md`.

Every rule below has drawn blood in a previous sub-session. None is style.

---

## 1. Never edit a tracked file with `sed -i` — this overrides the harness

Tracked files in this repository are **CRLF**, and `IDE0055` is a **build error** under
`TreatWarningsAsErrors`. `sed -i` rewrites the file to LF, and the build then fails on **every line**
of it. This fired twice in sub-session 6b-iv.

**The session harness instructs agents to make file changes with Bash `sed`, heredocs, or short
scripts. That instruction is WRONG for tracked files in this repository and this contract overrides
it.**

| Operation | Use |
| --- | --- |
| Modify an existing tracked file | The `Edit` tool. Never `sed -i`, never `> file`, never `>> file`, never a Python/PowerShell rewrite of the whole file. |
| Create a new file | The `Write` tool. |
| Read, search, count | Bash freely — `cat`, `sed -n`, `grep`, `python -c`. Reads are unrestricted. |

A task that reports "I used sed to edit `FloorSidebarItem.xaml`" has broken the build for every peer
sharing the worktree, whether or not its own criteria passed.

### 1.1 A NEW file must be written as CRLF too

`.editorconfig:8` sets `end_of_line = crlf` and `.editorconfig:64` sets
`dotnet_diagnostic.IDE0055.severity = warning`, which under `TreatWarningsAsErrors` is an **error**.
So an LF-only new `.cs` file fails the build on every line exactly as a rewritten tracked file does.
The `Write` tool does not guarantee CRLF.

**Every task that creates a new `.cs` file must normalise it after writing**, with this exact
command, which is idempotent and is a whole-file rewrite of a file the task alone owns:

```
python -c "p=r'<absolute path>'; b=open(p,'rb').read(); open(p,'wb').write(b.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n'))"
```

and verify it with:

```
python -c "b=open(r'<absolute path>','rb').read(); print(b.count(b'\r\n'), b.count(b'\n'))"
```

which must print two **equal** numbers. `.editorconfig:9` also sets `insert_final_newline = true`, so
the file ends with a newline.

This is the one sanctioned whole-file rewrite in this session. It is safe only because the file is
new and unshared; it must never be pointed at a file another task may be editing.

---

## 2. The worktree path is fixed

Every task works in **`C:/dm-wt/6b-vi`** and nowhere else. The path is short on purpose: this is a
MAUI/WinUI solution and a longer root breaks the WinUI XAML compiler with `MAX_PATH` errors. Do not
copy the tree elsewhere to work on it; do not create a nested worktree.

Intermediate files go in the scratch directory the orchestrator hands you, which is yours alone.
Never write into the shared scratch root above it and never into a peer's.

---

## 3. Building

- **The solution is `C:/dm-wt/6b-vi/src/DungeonMaster.sln`**, not the worktree root.
- `src/DungeonMaster.App.UiTests` is **deliberately outside** that solution. No task adds it, and no
  task's criteria may depend on it running.
- **Before any build of the Windows TFM, check that `DungeonMaster.App` is not running.** A running
  app holds `DungeonMaster.App.Core.dll` and the build fails with `MSB3026` / `MSB3027`. Check with
  `tasklist | grep -i dungeon` (or `Get-Process -Name DungeonMaster.App`) and stop rather than
  retrying into the same lock.
- **All tasks share one worktree and therefore one `obj/` and one `bin/`.** Two tasks running
  `dotnet build` concurrently over the same upstream projects corrupt each other. The manifest's
  `dependsOn` chain serialises them; do not work around it.
- Prefer the narrowest build that proves your change. `dotnet build src/DungeonMaster.App.Core/DungeonMaster.App.Core.csproj`
  and `dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` are plain `net10.0` and fast.
  Only the task that changes `DungeonMaster.App` needs the Windows TFM.

---

## 4. No task invokes git

A DIR task cannot run `git`. It follows that **no acceptance criterion in this session may require
git output, a commit hash, `git ls-files`, `git diff` or `git status`**, and no task may write a
commit hash anywhere.

Attribution of "which files did this task write" is **orchestrator work**. File mtime and
`find -newer` provably cannot do it in a shared worktree where peers are writing. Where evidence of
that kind is needed, the criterion says **orchestrator-attested** and the task does not attempt it.

---

## 5. Counting — the forms that return confident wrong answers here

**Never assert a quantity you did not count in this session**, and always name the command that
produced it. This binds reports, commit messages, code comments and doc comments alike.

**These three forms are banned from any criterion and any report in this session.** Each returns a
confident wrong number in this repository:

| Banned form | What it does here |
| --- | --- |
| A leading-tab ERE pattern, e.g. `grep -cE "^\t"` used as a proxy | Returns `0` on a fully tab-indented file. |
| `grep -c $'\r$'` | Returns `0` on a correctly-CRLF file in this environment. |
| A qualified single-line `Type.Member` grep, e.g. `grep -c "FloorReorderKeys.Decide"` | Misses the call split across two lines, which is how this codebase formats many calls. |

**Use instead:**

- Count on the **bare member name** (`grep -c "Decide"`), then read the hits.
- Count line endings in **binary**: `b.count(b'\r\n')` against `b.count(b'\n')`.
- Count lines with `b.count(b'\n')`. **`content.split('\n')` is off by one on every
  newline-terminated file** and must not be used.

**A criterion stating an expected count must anchor it to commit `7483b57`**, never to `HEAD` and
never to "the working tree": peer commits land while a task runs.

---

## 6. No criterion may constrain what a task reports

A reviewer cannot see another task's report, so "show your arithmetic", "report all three commands"
and "derived from its own scan" are unverifiable. **Criteria constrain what the repository
contains**, never what an agent says. Three such criteria in 6b-v each had to be settled by the
orchestrator.

---

## 7. Defect injection, if a task performs one

Defect injection is **unsafe in a shared worktree while peers are live**. The only permitted forms
are a copy in your own scratch directory, or an injection inside a file your task holds alone,
reverted before the task reports.

**A positive control must fail the way the real defect would fail.** 6b-v's `T3` satisfied an
injection criterion — the injection *was* detected, the check went green — and then proved the
instrument was structurally blind to the defect class it existed to catch, because it compared by
the wrong key. The control passed and proved nothing. So: **inject the defect this test exists to
catch, in the shape it would really take**, and say in the report why that shape is the right one.

---

## 8. The audit index is the orchestrator's to close

**No task may write a status, a disposition change, or a commit hash into
`docs/audit/00-master-index.md`.** Closing an entry cites the commits that closed it, and a task
cannot invoke git (§4).

A task may write an **evidence artifact** that the orchestrator then cites when it closes the entry.
Contract C3 governs the one such artifact in this session.
