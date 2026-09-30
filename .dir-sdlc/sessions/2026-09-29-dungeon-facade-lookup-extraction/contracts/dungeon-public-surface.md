# Contract — `Dungeon`'s public surface and its ten floor-lookup sites

**Frozen at Gate 2.** Declarations only. No task may change anything this file declares.

**Why this contract exists, given that `spec.md` declares none.** Three criteria across two tasks rest
on the phrase *"unchanged from `20b0b09`"*. **A task cannot invoke `git`**, so a task handed only that
phrase cannot read the baseline it is measured against. This file is the baseline, handed over as an
artifact rather than as a commit reference. It is also a real boundary between tasks: `T2` must not
change this surface, and `T1`'s tests call every member of it.

Every figure below was measured by the architect in the worktree `C:/dm-wt/6b-xiv` at `20b0b09`.
Commands are named per claim.

---

## 1. The twelve public methods of `Dungeon`, verbatim

All twelve are declared in `src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs`.
Enumerated by `grep -nE "^\tpublic " <file>`, which returns exactly 12 lines. Indentation is a **tab**
per level; the file is **CRLF** throughout (`b.count(b'\r\n')` → 489, `b.count(b'\n')` → 489).

Line numbers are the declaration's first line at `20b0b09` and **will move**. Locate a member by name,
never by line.

```csharp
// :19
public void Update(
    string name,
    string? description,
    string? notes)

// :42
public Floors.Floor AddFloor(string? name)

// :64
public MutationOutcome RemoveFloor(Guid floorId)

// :103
public MutationOutcome ReorderFloor(
    Guid floorId,
    int newNumber)

// :168
public MutationOutcome UpdateFloor(
    Guid floorId,
    string? name,
    string? description,
    string? notes)

// :203
public Areas.Area? AddArea(
    Guid floorId,
    string name,
    string? description)

// :234
public MutationOutcome RemoveArea(
    Guid floorId,
    Guid areaId)

// :279
public MutationOutcome UpdateArea(
    Guid floorId,
    Guid areaId,
    string name,
    string? description,
    string? notes)

// :326
public MutationOutcome UpdateAreaOutline(
    Guid floorId,
    Guid areaId,
    IReadOnlyList<PathSegment> outline)

// :374
public Floors.PlacedObject? AddPlacedObject(
    Guid floorId,
    Floors.ObjectKind kind,
    CanvasPoint position,
    Floors.ObjectRotation rotation,
    float scale)

// :413
public MutationOutcome RemovePlacedObject(
    Guid floorId,
    Guid objectId)

// :459
public MutationOutcome UpdatePlacedObject(
    Guid floorId,
    Guid objectId,
    CanvasPoint newPosition,
    Floors.ObjectRotation newRotation,
    float newScale)
```

**Name, parameter types, parameter names, order and return type are all frozen.** The indentation shown
above is normalised to four spaces for readability; **the file uses tabs** and a tab-to-space conversion
is an `IDE0055` build error here (`.editorconfig:64`, `EnforceCodeStyleInBuild=true` at
`Directory.Build.props:16`, `TreatWarningsAsErrors=true` at `:5`).

**Also frozen, and not in this file.** `src/DungeonMaster.Domain/Entities/Dungeons/Dungeon.cs` (50
lines) declares the rest of the public surface: the `public Dungeon(...)` constructor at `:21`, and the
properties `Name` `:33`, `Description` `:38`, `Notes` `:43`, `Floors` `:48`. No task owns that file, so
no task may change it.

---

## 2. The ten floor-lookup sites, and what each returns when the floor is absent

Every one of the ten carries the byte-identical two-line expression

```
\tvar floor = _floors.FirstOrDefault(floor =>\r\n\t\t\tfloor.Id == floorId);\r\n
```

followed by `if (floor is null)`. Counted with a Python read using `newline=''` after asserting
`"\r\n" in t`: `t.count('_floors.FirstOrDefault(floor =>\r\n\t\t\tfloor.Id == floorId);')` → **10**,
`t.count('if (floor is null)')` → **10**. Cross-checked a second way with a command that could not
share that mistake: `grep -c "FirstOrDefault" <file>` → **10** and
`grep -c "if (floor is null)" <file>` → **10**.

**The indentation is load-bearing and is tabs.** A grep or Python literal written with spaces returns
**0** — measured: the space-indented form of the same expression returns 0 against this file.

**A single-line literal also returns 0.** `grep -cF '_floors.FirstOrDefault(floor => floor.Id == floorId)'`
→ **0**, because the expression is split across two lines. `spec.md` states the pattern in that
single-line form; no criterion may count it that way.

| # | Method | Lookup at `20b0b09` | Guard at | Returns when floor is absent |
|---|---|---|---|---|
| 1 | `RemoveFloor` | `:66-67` | `:69` | `MutationOutcome.NotFound` (`:71`) |
| 2 | `ReorderFloor` | `:107-108` | `:110` | `MutationOutcome.NotFound` (`:112`) |
| 3 | `UpdateFloor` | `:174-175` | `:177` | `MutationOutcome.NotFound` (`:179`) |
| 4 | `AddArea` | `:208-209` | `:211` | **`null`** (`:213`) |
| 5 | `RemoveArea` | `:238-239` | `:241` | `MutationOutcome.NotFound` (`:243`) |
| 6 | `UpdateArea` | `:286-287` | `:289` | `MutationOutcome.NotFound` (`:291`) |
| 7 | `UpdateAreaOutline` | `:331-332` | `:334` | `MutationOutcome.NotFound` (`:336`) |
| 8 | `AddPlacedObject` | `:381-382` | `:384` | **`null`** (`:386`) |
| 9 | `RemovePlacedObject` | `:417-418` | `:420` | `MutationOutcome.NotFound` (`:422`) |
| 10 | `UpdatePlacedObject` | `:466-467` | `:469` | `MutationOutcome.NotFound` (`:471`) |

**8 `NotFound` and 2 `null`.** Counted by `grep -n -A2 "if (floor is null)" <file> | grep -E "return"`,
which returns 10 lines: 8 `return MutationOutcome.NotFound;` and 2 `return null;`.

**Predicate control, so the total is not trusted on its own.** The two methods with **no** lookup are
`Update` (`:19`) and `AddFloor` (`:42`) — confirmed by reading `:19-53`, where neither touches
`_floors.FirstOrDefault`. `10 + 2 = 12`, and 12 is the method count measured independently in §1. The
**must-match** case is `RemoveFloor`; the **must-not-match** near-miss is `AddFloor`, which does touch
`_floors` (`_floors.Count + 1` at `:46`, `_floors.Add(floor)` at `:49`) and must not be counted as a
lookup site. It is not.

**Every one of these ten dispositions is behaviour that must survive this session unchanged.**

---

## 3. The second duplication: the outcome re-check

`grep -c "if (outcome is not MutationOutcome.Success)" <file>` → **5**. `CORE-015` does not mention it.

---

## 4. The eight forwarders, and the four that rename

`grep -nE "floor\.[A-Z][A-Za-z]*\(" <file>` returns **9** lines. Eight are the forwarders; the ninth,
`floor.UpdateNumber(newNumber)` at `:144`, sits inside `ReorderFloor`, which is **not** a forwarder —
it runs its own renumbering after the lookup.

| `Dungeon` method | forwards to | line | identically named? |
|---|---|---|---|
| `UpdateFloor` | `floor.UpdateDetails` | `:182` | **no** |
| `AddArea` | `floor.AddArea` | `:216` | yes |
| `RemoveArea` | `floor.RemoveArea` | `:246` | yes |
| `UpdateArea` | `floor.UpdateArea` | `:294` | yes |
| `UpdateAreaOutline` | `floor.UpdateAreaOutline` | `:339` | yes |
| `AddPlacedObject` | `floor.AddObject` | `:389` | **no** |
| `RemovePlacedObject` | `floor.RemoveObject` | `:425` | **no** |
| `UpdatePlacedObject` | `floor.UpdateObject` | `:474` | **no** |

**4 of 8 rename.** `CORE-015`'s claim that the forwarders "forward to the identically named `Floor`
method" is true for 4 and false for 4. Independently re-derived by the architect from the grep above;
it agrees with `measurement.md` §1 in every row.

---

## 5. The eight external call sites — frozen, and checkable without `git`

Each is one Application command handler, and every one is at line **`:24`** of its file. Command:

```
grep -rn --include=*.cs -E "\.(UpdateFloor|AddArea|RemoveArea|UpdateArea|UpdateAreaOutline|AddPlacedObject|RemovePlacedObject|UpdatePlacedObject)\(" src/ --exclude-dir=obj --exclude-dir=bin
```

filtered to exclude `DungeonMaster.Tests/` and `DungeonMaster.Domain/`. Re-run by the architect: **8
hits, each printed and read individually.** Predicate validated two ways, because a uniform count of 1
across 8 different names is the shape of a broken filter — must-match control `.AddFloor(` → 1 external
caller; must-not-match control `.Touch(` → 0.

**No task owns these files, and no task may modify them.** A task can verify that for itself with
`sha256sum`, which needs no `git`. All eight, at `20b0b09`, under
`src/DungeonMaster.Application/Features/Dungeons/Commands/`:

```
a5466beeeb16353d90d38cdf7129a9c0fa76099016f1543f3c464ad0a131c45e  AddArea/AddAreaCommandHandler.cs
390e0fcef16d13ec697e01777a9aa851f92b2f43a3a9c43be13635eef1347161  AddPlacedObject/AddPlacedObjectCommandHandler.cs
053a280c9a315f3fa204a273d649f896879588eaedeb4ecfa0490bbca93aab89  RemoveArea/RemoveAreaCommandHandler.cs
a8c251ec9f5e371f68e0773fd71414f86a64f1f6f1ce72d023b2b337fdf4fb6d  RemovePlacedObject/RemovePlacedObjectCommandHandler.cs
184075a051499dfc6cae11b04d838949972afdd9d65b9e9c85716e1a61e29a65  UpdateArea/UpdateAreaCommandHandler.cs
f0310b3f36d4a2b5838aea134cf06b1620c614ed5ab80fc86f267dcfc9c82a55  UpdateAreaOutline/UpdateAreaOutlineCommandHandler.cs
061c245d981ed17c3c2b2363d2b58ba79928a25ee880622040a8e9ec63038073  UpdateFloor/UpdateFloorCommandHandler.cs
54ce97bbd3cd7854f9b93c24d77b124f23f40bb9445d04cb300f65337f102406  UpdatePlacedObject/UpdatePlacedObjectCommandHandler.cs
```

---

## 6. Which of the ten not-found paths is already tested, and which is not

`src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs` — **note the directory is
`Dungeon/`, singular**, while the source lives under `Dungeons/`, plural. 473 lines; `grep -cF '[Fact]'`
→ **24**, `grep -cF '[Theory]'` → **4**. `-F` is required: `grep -c "[Fact]"` parses as a character
class.

**Each of the ten test bodies below was read in full, not matched by line.** That read is what produced
the finding: five test names contain `WithUnknown…_ReturnsNotFound` and **none of the five exercises an
unknown `floorId`** — each passes a **valid** `floor.Id` from `dungeon.AddFloor("Entry")` together with
an unknown *child* id, so the assertion is satisfied by the child lookup inside `Floor`, one level below
the guard this contract is about.

| # | Method | Floor-absent path covered at `20b0b09`? | By |
|---|---|---|---|
| 1 | `RemoveFloor` | **yes** | `RemoveFloor_WithUnknownFloorId_ReturnsNotFound` (`:115`) |
| 2 | `ReorderFloor` | **yes** | `ReorderFloor_WithUnknownFloorId_ReturnsNotFound` (`:145`) |
| 3 | `UpdateFloor` | **yes** | `UpdateFloor_WithUnknownFloorId_ReturnsNotFound` (`:215`) |
| 4 | `AddArea` | **yes** | `AddArea_WithUnknownFloorId_ReturnsNull` (`:233`) |
| 5 | `RemoveArea` | **NO** | `RemoveArea_WithUnknownAreaId_ReturnsNotFound` (`:251`) passes `floor.Id` |
| 6 | `UpdateArea` | **NO** | `UpdateArea_WithUnknownAreaId_ReturnsNotFound` (`:267`) passes `floor.Id` |
| 7 | `UpdateAreaOutline` | **NO** | `UpdateAreaOutline_WithUnknownAreaId_ReturnsNotFound` (`:286`) passes `floor.Id` |
| 8 | `AddPlacedObject` | **yes** | `AddPlacedObject_OnUnknownFloorId_ReturnsNull` (`:456`) |
| 9 | `RemovePlacedObject` | **NO** | `RemovePlacedObject_WithUnknownObjectId_ReturnsNotFound` (`:303`) passes `floor.Id` |
| 10 | `UpdatePlacedObject` | **NO** | `UpdatePlacedObject_WithUnknownObjectId_ReturnsNotFound` (`:319`) passes `floor.Id` |

**5 of the 10 floor-absent paths are covered; 5 are not.** The five uncovered are `RemoveArea`,
`UpdateArea`, `UpdateAreaOutline`, `RemovePlacedObject`, `UpdatePlacedObject` — all five in the
`NotFound` group. Both `null` sites are already covered.

**This corrects the premise of `spec.md` items 7 and 8**, which read as though the coverage gap were
uniform across all ten. It is not: the work is exactly five new test cases, and the existing five are a
preservation obligation rather than work. The five misleading names are also a finding in their own
right against `test.md`'s Naming rule — *"The name must not claim more than the test checks"* — but
renaming them is not in `spec.md`'s scope and is not commissioned here.

---

## 7. Suite baseline — re-derived, not inherited

`dotnet test src/DungeonMaster.sln --nologo` in `C:/dm-wt/6b-xiv` at `20b0b09`, exit `0`:

| Assembly | Passed | Failed | Skipped | Total |
|---|---|---|---|---|
| `DungeonMaster.Tests` | 850 | 0 | 0 | 850 |
| `DungeonMaster.App.UnitTests` | 23 | 0 | 0 | 23 |
| `DungeonMaster.App.Tests` | 102 | 0 | 0 | 102 |
| **Sum** | **975** | **0** | **0** | **975** |

**All three figures are `passed` counts.** `850 / 23 / 102` are three assemblies' passed totals and
nothing else — **not** "850 passed, 23 skipped, 102 inconclusive". Any dispatch or commit body carrying
these figures carries both number and label verbatim.

**Read each assembly's identity from its `Test run for …` line, never from `Passed!`.** Reproduced
again by the architect: all three `Passed!` lines print `(net10.0)`, while `DungeonMaster.App.Tests` ran
from `bin\Debug\net10.0-windows10.0.19041.0\`.

`DungeonMaster.App.Tests` has silently shed tests before (`81` where `102` was expected, nothing failed
or skipped). Treat its **102** as a prediction to check, not a guarantee.

**Build baseline.** `dotnet build src/DungeonMaster.sln`, exit `0`, `Build succeeded`,
`0 Warning(s)` / `0 Error(s)`. `grep -cE "(warning|error) [A-Z]+[0-9]+"` over that log → **0**. The
plain form `grep -icE "warning|error"` → **2**, because MSBuild prints those two summary lines
literally. Over a `dotnet test` log the plain form returns **0**. **The qualified form is the only one
any criterion may use over a build log.**

**The solution is `src/DungeonMaster.sln`, not the repository root.**
