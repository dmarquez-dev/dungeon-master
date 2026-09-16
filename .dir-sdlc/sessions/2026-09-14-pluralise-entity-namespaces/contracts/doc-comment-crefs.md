# Contract — the six doc-comment `cref`s

Frozen at Gate 2. Session `2026-09-14-pluralise-entity-namespaces` (sub-session 6b-v).

Authored by `dir-architect` at decomposition. **Consumed by T1 (Domain, five of them) and T3
(Infrastructure, one).** It crosses a task boundary, which is why it is a contract and not a note in
one task's criteria.

---

## 1. Why these need a contract at all: nothing in the build sees them

`Directory.Build.props:5` sets `TreatWarningsAsErrors`. **`GenerateDocumentationFile` is not set
anywhere** — verified by searching `Directory.Build.props` and all eight `src/*/*.csproj` files for
the property; there are zero matches. Without it the compiler does not emit **CS1574** (`cref`
attribute could not be resolved), so a `cref` naming a namespace that no longer exists is **silently
wrong**. It compiles. It is green. `dotnet build` reports `0 Warning(s), 0 Error(s)` either way.

These six are therefore the only part of the rename with **no automatic detector at all**, and they
are verified by a reviewer reading the file.

### `GenerateDocumentationFile` is deliberately NOT turned on by this session

Enabling it would also enable **CS1591** (missing XML comment on a publicly visible type or member)
for every undocumented public member in the solution, which under `TreatWarningsAsErrors` could turn
the whole build red. Whether it would, and how widely, has **not been measured** — so it is an
unestablished instrument, and no criterion in this session rests on it. A criterion whose instrument
has not been shown able to see the thing is the failure mode 6b-iv burned a session on.

It is a good ratchet and it is recorded as a follow-on, not scheduled here.

---

## 2. The six — by enclosing member, not by line number

**Line numbers move the moment any file above them is edited, and five of these six sit in files this
session edits heavily. No criterion may cite them.** The line numbers below are given once, as they
stood at `af4c65f`, purely so a reader can find the site; the *identity* of each `cref` is its
enclosing member plus its exact text.

### T1 — `DungeonMaster.Domain`, five

| File | at `af4c65f` | From | To |
|---|---|---|---|
| `Entities/Dungeon/DungeonMethods.cs` | `:40` | `cref="Floor.Floor"` | `cref="Floors.Floor"` |
| `Entities/Dungeon/DungeonMethods.cs` | `:201` | `cref="Area.Area"` | `cref="Areas.Area"` |
| `Entities/Dungeon/DungeonMethods.cs` | `:369` | `cref="Floor.PlacedObject"` | `cref="Floors.PlacedObject"` |
| `Entities/Dungeon/DungeonMethods.cs` | `:402` | `cref="Floor.PlacedObject"` | `cref="Floors.PlacedObject"` |
| `Entities/Floor/FloorMethods.cs` | `:53` | `cref="Area.Area"` | `cref="Areas.Area"` |

After T1, these live at `Entities/Dungeons/DungeonMethods.cs` and `Entities/Floors/FloorMethods.cs`.

### T3 — `DungeonMaster.Infrastructure`, one

| File | at `af4c65f` | From | To |
|---|---|---|---|
| `Persistence/Repositories/DungeonReadRepository.cs` | `:9` | `cref="DungeonMaster.Domain.Entities.Dungeon.Dungeon"` | `cref="DungeonMaster.Domain.Entities.Dungeons.Dungeon"` |

Note the shape difference: T3's is **fully qualified** and stays fully qualified. T1's five are
**leaf-qualified** and stay leaf-qualified. Neither is converted to the other form — see
`contracts/target-namespaces.md` §4, "Simplification is forbidden".

`Floors.` and `Areas.` leaf-qualified `cref`s are safe: neither segment exists elsewhere in `src/`
(`contracts/namespace-collision.md` §1).

---

## 3. How a task proves it did this

The count is **6**, and it is the count of `cref` values matching
`(^|\.)(Dungeon|Floor|Area)\.(Dungeon|Floor|Area|PlacedObject|ObjectKind|ObjectRotation|ObjectScale)`
over every `.cs` file in `src/`.

**After the session, that count must be 0**, and the count of the corresponding plural forms must be
6. Both are re-derivable with the script preserved at
`…/scratchpad/dir/2026-09-14-pluralise-entity-namespaces/architect/hazards.py`, which prints each
match with its file and line so the search is not a bare number.

Each owning task runs that scan **scoped to its own project** and reports the matches it printed —
T1 expects 0 singular / 5 plural under `src/DungeonMaster.Domain/`, T3 expects 0 singular / 1 plural
under `src/DungeonMaster.Infrastructure/`. Neither task scans the other's project; a project-wide
count is not something a task running beside peers can honestly assert.

**No other `cref` in the solution is edited.** The scan is the definition of the change surface, and
it returned exactly these six.
