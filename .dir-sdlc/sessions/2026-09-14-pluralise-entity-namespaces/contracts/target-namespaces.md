# Contract — target namespaces and folder paths

Frozen at Gate 2. Session `2026-09-14-pluralise-entity-namespaces` (sub-session 6b-v), closing
`MASTER-022` / `CORE-004`.

Authored by `dir-architect` at decomposition. **Consumed by every task in this manifest.** The whole
session depends on all seven tasks writing the same three strings; a task that invents its own
spelling produces a build that fails in someone else's project, not its own.

Every count and every path in this file was re-derived at `af4c65f` with the commands named in §7.

---

## 1. The three renames — these strings are the contract

| Old namespace | New namespace | Old folder | New folder |
|---|---|---|---|
| `DungeonMaster.Domain.Entities.Area` | `DungeonMaster.Domain.Entities.Areas` | `src/DungeonMaster.Domain/Entities/Area/` | `src/DungeonMaster.Domain/Entities/Areas/` |
| `DungeonMaster.Domain.Entities.Dungeon` | `DungeonMaster.Domain.Entities.Dungeons` | `src/DungeonMaster.Domain/Entities/Dungeon/` | `src/DungeonMaster.Domain/Entities/Dungeons/` |
| `DungeonMaster.Domain.Entities.Floor` | `DungeonMaster.Domain.Entities.Floors` | `src/DungeonMaster.Domain/Entities/Floor/` | `src/DungeonMaster.Domain/Entities/Floors/` |

The transformation is exactly: **append one `s` to the leaf segment, and to the matching folder
name.** Nothing else. No segment is added, removed, reordered or re-cased.

**The folder rename is not cosmetic and is not optional.**
`dm-conventions/backend/dotnet-format.md:30` requires the namespace to match the folder structure and
`:58` states *"Folder structure mirrors the namespace hierarchy."* Renaming the namespace without the
folder violates both. This is also why the alternative direction `CORE-004` suggests — flattening to
`DungeonMaster.Domain.Entities` — was rejected at Gate 1.

`dm-conventions` is a separate repository at `C:/Users/dmarq/Development/Repos/dm-conventions` and
**must not be modified by this session.**

---

## 2. What does NOT move

These stay exactly where they are, with their namespace unchanged at
`DungeonMaster.Domain.Entities`:

- `src/DungeonMaster.Domain/Entities/Entity.cs`
- `src/DungeonMaster.Domain/Entities/MutationOutcome.cs`

**No type is renamed.** `Dungeon`, `Floor`, `Area`, `PlacedObject`, `ObjectKind`, `ObjectRotation`,
`ObjectScale`, `RotationDirection`, `ScaleDirection`, `SnapAnchor`, `Entity`, `MutationOutcome` all
keep their names. **No file is renamed.** Only the three *directories* in §1 are renamed.

**The test projects' mirror folders are NOT renamed by this session.**
`src/DungeonMaster.Tests/Domain/Entities/{Area,Dungeon,Floor}/` and their namespaces
`DungeonMaster.Tests.Domain.Entities.{Area,Dungeon,Floor}` stay as they are. Those namespaces contain
no type of the same name, so the Microsoft guideline `CORE-004` cites does not apply to them, and
their namespaces already match their folders. See the coverage mapping for the open question this
leaves.

---

## 3. The file inventory — 14 files move, 84 files are edited

**14 files change folder** (all under `src/DungeonMaster.Domain/Entities/`):

| From | To | Files |
|---|---|---|
| `Area/` | `Areas/` | `Area.cs`, `AreaMethods.cs` |
| `Dungeon/` | `Dungeons/` | `Dungeon.cs`, `DungeonMethods.cs` |
| `Floor/` | `Floors/` | `Floor.cs`, `FloorMethods.cs`, `FloorObjectMethods.cs`, `ObjectKind.cs`, `ObjectRotation.cs`, `ObjectScale.cs`, `PlacedObject.cs`, `RotateDirection.cs`, `ScaleDirection.cs`, `SnapAnchor.cs` |

2 + 2 + 10 = **14**, which is also the count of `namespace ...Entities.(Dungeon|Floor|Area)`
declarations in `src/` (§7 command A).

**84 `.cs` files under `src/` contain at least one of the three patterns** and must be edited. By
project, and by which task owns them in this manifest:

| Project | Task | ns decls | `using` lines | qualified refs | files |
|---|---|---|---|---|---|
| `DungeonMaster.Domain` | T1 | 14 | 1 | 22 | 14 |
| `DungeonMaster.Application` | T2 | 0 | 8 | 0 | 8 |
| `DungeonMaster.Infrastructure` | T3 | 0 | 6 | 49 | 11 |
| `DungeonMaster.App.Core` | T4 | 0 | 3 | 0 | 3 |
| `DungeonMaster.App` | T5 | 0 | 10 | 0 | 10 |
| `DungeonMaster.App.Tests` | T5 | 0 | 3 | 0 | 3 |
| `DungeonMaster.Tests` | T6 | 0 | 39 | 5 | 35 |
| **TOTAL** | | **14** | **70** | **76** | **84** |

`src/DungeonMaster.App.UnitTests` and `src/DungeonMaster.App.UiTests` contain **zero** occurrences of
any of the three patterns and are not edited by any task.

### The 85th file — measured, and deliberately excluded

A separate scan for the fully-qualified string `DungeonMaster.Domain.Entities.(Dungeon|Floor|Area)`
across **every** `.cs` file in the repository returns **85** files (§7 command D). The one file in
that set and not in the 84 is:

```
.dir-sdlc/sessions/2026-08-26-settle-the-app-boundaries/contracts/app-core-presentation.cs
```

at line 60, `using DungeonMaster.Domain.Entities.Floor;`.

**It is a frozen contract from sub-session 5a. It is not compiled, it is not source, and it must not
be edited.** Its own header states so: *"This file is a CONTRACT, not source. It is not compiled."*
It is a historical record of what was frozen in August, and amending it would falsify that record.

**84 is therefore the correct number for every criterion in this session that counts edited files.**
85 is the correct number only for the phrase "files in the repository that mention these namespaces",
and no criterion uses it.

---

## 4. The three edit shapes — and the one that does not exist here

Every occurrence in `src/` is one of exactly three shapes. There is no fourth.

**(a) Namespace declaration** — 14, all in `DungeonMaster.Domain`, all file-scoped:

```csharp
namespace DungeonMaster.Domain.Entities.Floor;   →   namespace DungeonMaster.Domain.Entities.Floors;
```

**(b) `using` directive** — all at file top **above** the file-scoped `namespace` declaration. Two
sub-forms, and **the two are counted by different commands — do not conflate them:**

```csharp
using DungeonMaster.Domain.Entities.Floor;                           // PLAIN — 70, command B
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeon.Dungeon; // ALIAS — 5, command C
```

**The 70 in the §3 table is the plain form only.** Command B's pattern ends at the namespace leaf
followed by `;`, so it does **not** match the alias form, which continues into a type name. The 5
aliases fall into the **qualified-reference** column instead, and all 5 are in
`DungeonMaster.Tests`. A task counting "using lines it edited" with command B alone will
under-report by exactly the number of aliases in its project — 5 for T6, 0 for everyone else.

**(c) Qualified reference** — 76, of which 22 are in `DungeonMaster.Domain` (leaf-qualified, e.g.
`Floor.Floor`), 49 in `DungeonMaster.Infrastructure` (48 fully-qualified strings in `Migrations/` —
see `contracts/migration-snapshot.md` — plus 1 fully-qualified `cref`), and 5 in
`DungeonMaster.Tests` (all of them the *alias* form above, at
`Domain/Entities/Area/AreaOutlineTests.cs:4`, `Domain/Entities/Dungeon/DungeonAggregateTests.cs:5`,
`Domain/Entities/Floor/FloorMethodsTests.cs:3`, and
`Domain/Entities/Floor/FloorObjectMethodsTests.cs:5` and `:6`).

**Worked example of the arithmetic, because it is easy to get wrong.** In
`DungeonMaster.Tests`: 39 plain `using` lines across 34 files, plus 5 alias lines across 4 files,
= **44 lines across 35 files**. It is 35 and not 38 because three of the four alias files also carry
a plain `using`; the 35th file is `Domain/Entities/Floor/FloorMethodsTests.cs`, which carries an
alias and no plain `using`. Five files carry two plain `using` lines each, which is why 39 lines sit
in 34 files.

**(d) XAML `clr-namespace` — measured at zero.** A scan of every `.xaml` file in the repository for
`clr-namespace:DungeonMaster.Domain.Entities.(Dungeon|Floor|Area)` returns **0 files**, and a wider
scan for any `clr-namespace:DungeonMaster.Domain...` returns 0 matches (§7 command E). **No task
edits XAML.** A task that finds itself editing a `.xaml` file has gone outside this contract and must
escalate.

### Simplification is forbidden

A task may change **only** the leaf segment, from singular to plural. It must **not**:

- shorten a fully-qualified reference to a leaf-qualified one (this is how the collision in
  `contracts/namespace-collision.md` fires);
- introduce, remove or retarget a `using` alias;
- reorder, add or remove `using` directives — including "tidying" them into alphabetical order. The
  existing order is not alphabetical (see
  `src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddPlacedObjectCommandHandlerTests.cs:4-5`,
  where `Entities.Floor` precedes `Entities.Dungeon`) and nothing in the build enforces it;
- reformat, re-indent, or re-wrap any line it is not changing;
- collapse `Floor.PlacedObject` to `PlacedObject` even where a `using` would make that legal. That is
  a readability improvement, it is not this session, and it turns a 1-character diff into a review
  surface. Every qualified reference keeps its qualification, pluralised.

---

## 5. Mechanics — how to make the edit without breaking the build on formatting

These are not style preferences. Each one is a measured build failure in this repository.

- **`.cs` files are indented with TABS.** `.editorconfig:64` sets
  `dotnet_diagnostic.IDE0055.severity = warning` and `Directory.Build.props:5` sets
  `TreatWarningsAsErrors`, so a space-indented or LF-indented line is a **build error**, not a
  warning. Any new file must be written with tabs. (This session creates no new `.cs` file; the rule
  is stated because the folder rename writes files at new paths.)
- **Tracked files are CRLF.** `.gitattributes` is `* text=auto`. **Never use `sed -i`** on a tracked
  file: it rewrites the whole file to LF and fails the build on every line. This fired twice in
  6b-iv. Use the edit tools.
- **`grep -c $'\r$'` returns a confident `0` on a correctly-CRLF file in this environment.** Do not
  use it to check line endings. Count in binary: `b.count(b'\r\n')` versus `b.count(b'\n')`.
- **Recommended order for the folder rename (T1 only): edit contents first, then move the
  directory.** Editing in place keeps the edit tools' read-tracking valid, and a directory move
  performed with `mv` is a rename that does not rewrite bytes, so CRLF survives. Moving first and
  editing after also works; writing the file out at the new path by hand does not, because it
  re-encodes the line endings.
- **The interim state does not need to build.** Between the namespace edit and the folder move, the
  namespace and the folder disagree. `IDE0130` is not in the `.editorconfig` analyzer backlog block
  (`:63-75`) and is therefore at its default severity, which does not fail the build. Only the
  **final** state is judged.
- **Close any running `DungeonMaster.App` before building the Windows TFM.** A live instance holds
  `DungeonMaster.App.Core.dll` and produces `MSB3026`/`MSB3027`. This bit three agents in 6b-iv.
- **All tasks share one worktree and one `obj/`/`bin/`.** Even where the dependency graph permits two
  tasks to run at once, the orchestrator may serialize them. No task's correctness may depend on
  running concurrently with another.

---

## 6. This is a pure rename — the behavioural floor

Nothing in this session changes behaviour. The baseline to preserve:

- Full solution suite **908 passed, 0 failed** — `DungeonMaster.Tests` 788,
  `DungeonMaster.App.Tests` 97, `DungeonMaster.App.UnitTests` 23.
- Build **0 Warning(s), 0 Error(s)**.

**The test count must not change and no test's assertions may be rewritten.** A task that finds
itself changing *what* a test asserts — as opposed to which namespace the test imports — has found a
real problem. It must **escalate to the orchestrator and stop**, not proceed. The same applies to any
task that finds itself adding a `using`, deleting a type, or changing a method signature.

---

## 7. Commands — every number above, re-derivable

All run against the session worktree `C:/dm-wt/6b-v` at `af4c65f`, walking `src/` and skipping
`obj/` and `bin/`. The three patterns are matched **separately**, which is why the per-project columns
in §3 do not sum to the file count.

- **A — namespace declarations (14):** `^\s*namespace\s+DungeonMaster\.Domain\.Entities\.(Dungeon|Floor|Area)\b` over `src/**/*.cs`, multiline.
- **B — `using` lines (70):** `^\s*using\s+(?:\w+\s*=\s*)?DungeonMaster\.Domain\.Entities\.(Dungeon|Floor|Area)\s*;` over `src/**/*.cs`, multiline.
- **C — qualified references (76):** `\b(Dungeon|Floor|Area)\.(Dungeon|Floor|Area|PlacedObject|ObjectKind|ObjectRotation|ObjectScale)\b` over `src/**/*.cs`.
- **D — fully-qualified, whole repo (85 files):** `DungeonMaster\.Domain\.Entities\.(Dungeon|Floor|Area)\b` over every `.cs` file under the repository root, skipping `obj/`, `bin/`, `.git/`.
- **E — XAML (0 files):** `clr-namespace:DungeonMaster\.Domain\.Entities\.(Dungeon|Floor|Area)\b` over every `.xaml` file under the repository root.
- **F — file inventory (14 files in 3 folders):** `ls` of `src/DungeonMaster.Domain/Entities/` and its three subdirectories.

The scripts that produced these are preserved at
`…/scratchpad/dir/2026-09-14-pluralise-entity-namespaces/architect/scan.py` and `hazards.py`.
**Re-run them; do not restate their output from this file.** A count restated instead of re-derived
is the single failure 6b-iv produced twelve times.
