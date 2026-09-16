# Contract — the `Dungeons` namespace-segment collision

Frozen at Gate 2. Session `2026-09-14-pluralise-entity-namespaces` (sub-session 6b-v).

Authored by `dir-architect` at decomposition. **Consumed by T2 (Application) and T6 (Tests)**, which
own the 14 files where the collision could fire, and by T1, which creates the segment that collides.

This contract exists so that no implementer has to *discover* this. It was measured, and then it was
**run** — see §3, which reports a real compiler, not an argument about one.

---

## 1. The collision

After the rename, `Dungeons` exists as a namespace segment in **two** places:

- `DungeonMaster.Domain.Entities.Dungeons` — created by this session (T1).
- `…Features.Dungeons` — **19 distinct namespaces already use it**, and have since long before this
  session: **16** under `DungeonMaster.Application.Features.Dungeons.*` and **3** under
  `DungeonMaster.Tests.Application.Features.Dungeons.*`.

Inside any of those 19, an **unqualified `Dungeons`** binds to the nearer enclosing
`Features.Dungeons`, not to `Domain.Entities.Dungeons`.

**`Floors` and `Areas` have no existing segment anywhere in `src/`** — measured at 0 distinct
namespaces each. They are clean. This contract is about `Dungeons` and nothing else.

---

## 2. The 14 files at risk

Files whose **own** namespace contains the segment `Dungeons` **and** which reference
`DungeonMaster.Domain.Entities.Dungeon`:

| File | Own namespace |
|---|---|
| `src/DungeonMaster.Application/Features/Dungeons/Commands/CreateDungeon/CreateDungeonCommandHandler.cs` | `…Application.Features.Dungeons.Commands.CreateDungeon` |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs` | `DungeonMaster.Tests.Application.Features.Dungeons.Commands` |
| `…/AddFloorCommandHandlerTests.cs` | same |
| `…/AddPlacedObjectCommandHandlerTests.cs` | same |
| `…/CreateDungeonCommandHandlerTests.cs` | same |
| `…/RemoveAreaCommandHandlerTests.cs` | same |
| `…/RemoveFloorCommandHandlerTests.cs` | same |
| `…/RemovePlacedObjectCommandHandlerTests.cs` | same |
| `…/ReorderFloorCommandHandlerTests.cs` | same |
| `…/UpdateAreaCommandHandlerTests.cs` | same |
| `…/UpdateAreaOutlineCommandHandlerTests.cs` | same |
| `…/UpdateDungeonCommandHandlerTests.cs` | same |
| `…/UpdateFloorCommandHandlerTests.cs` | same |
| `…/UpdatePlacedObjectCommandHandlerTests.cs` | same |

1 in `DungeonMaster.Application` (T2's) and 13 in `DungeonMaster.Tests` (T6's).

**All 14 carry the reference as a plain `using` directive and zero qualified references.** That is
the measured fact that makes the next section decisive.

---

## 3. Measured, not reasoned — four cases built against a real compiler

A four-file probe was compiled with the same SDK band this repository pins
(`global.json` → `10.0.400`), `net10.0`, `LangVersion 14.0`, `Nullable enable`,
`ImplicitUsings enable`. It declares `namespace DungeonMaster.Domain.Entities.Dungeons; class Dungeon`
alongside consumers in `DungeonMaster.Application.Features.Dungeons.Commands.*` and
`DungeonMaster.Tests.Application.Features.Dungeons.Commands`. Preserved at
`…/scratchpad/dir/2026-09-14-pluralise-entity-namespaces/architect/probe/`.

| Case | Shape | Result |
|---|---|---|
| **A** | `using DungeonMaster.Domain.Entities.Dungeons;` at file top, above the file-scoped `namespace`, consumer in `…Features.Dungeons.Commands.CreateDungeon` | **compiles**, 0 warnings, 0 errors |
| **B** | fully-qualified `DungeonMaster.Domain.Entities.Dungeons.Dungeon` from inside `…Features.Dungeons.Commands.OtherThing` | **compiles** |
| **C** | `using DomainDungeon = DungeonMaster.Domain.Entities.Dungeons.Dungeon;` at file top, consumer in `DungeonMaster.Tests.Application.Features.Dungeons.Commands` | **compiles** |
| **D** | leaf-qualified `Dungeons.Dungeon` from inside `…Features.Dungeons.Commands.LeafQualified` | **FAILS** — `error CS0234: The type or namespace name 'Dungeon' does not exist in the namespace 'DungeonMaster.Application.Features.Dungeons'` |

Case D is the positive control: it proves the probe can see the collision, so cases A, B and C
passing is evidence and not an empty apparatus. Run in that order, D was added to a green build and
turned it red, then is the only thing that is red.

### What this settles

**A, B and C are the only three shapes that exist in this repository** (see
`contracts/target-namespaces.md` §4). All three compile. **The mechanical rename, performed exactly
as §4 of that contract requires, does not fire the collision in any of the 14 files.**

**Shape D does not exist in the repository today and no task may create it.** This is the one way the
session can break: an implementer "tidying" a fully-qualified reference down to its leaf, or writing
a new qualified reference by hand.

---

## 4. The rules this contract imposes

1. **Every one of the 14 files in §2 is edited by changing the `using` line's leaf segment and
   nothing else.** No alias is introduced. No `using` is removed. No reference inside the file body
   is touched — there are none to touch.
2. **No task writes a leaf-qualified `Dungeons.` reference anywhere.** Not in `DungeonMaster.Domain`,
   where the leaf-qualified form *is* the existing idiom (`Dungeon.Dungeon` → `Dungeons.Dungeon` is
   correct and safe *there*, because `DungeonMaster.Domain` has no `Features.Dungeons`), and
   emphatically not in `DungeonMaster.Application` or `DungeonMaster.Tests`, where it is CS0234.
3. **`Floors.` and `Areas.` leaf-qualified references are safe everywhere**, because no other
   namespace in `src/` carries either segment. T1's 22 in-Domain qualified references are the
   established idiom and stay leaf-qualified.
4. **The collision fails loudly or not at all.** It is `CS0234` at compile time. There is no silent
   misbehaviour and no runtime failure mode. `TreatWarningsAsErrors` is irrelevant here — CS0234 is
   an error already. **A green `dotnet build` of the owning project is sufficient evidence that this
   contract was honoured**, and that is exactly how T2's and T6's criteria verify it.

---

## 5. Commands

- **19 `Dungeons` namespaces / 0 `Floors` / 0 `Areas`:** collect every
  `^\s*namespace\s+([A-Za-z0-9_.]+)\s*[;{]` match over `src/**/*.cs` (skipping `obj/`, `bin/`), split
  each on `.`, and count the distinct namespaces having the segment. Script preserved at
  `…/architect/hazards.py`.
- **The 14 at-risk files:** files whose own namespace has segment `Dungeons` and whose text matches
  `DungeonMaster\.Domain\.Entities\.Dungeon\b`. Script preserved at `…/architect/detail.py`.
- **The probe:** `dotnet build probe.csproj -v:m` in `…/architect/probe/`.
