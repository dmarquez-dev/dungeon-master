# Measurement — MASTER-073 and MASTER-074

**Fixed commit: `9c2bff6`.** Every claim below was re-derived against that commit in the main checkout
before decomposition, per the discipline inherited from 6b-vii. Commands are named beside their results.
Paths are repository-relative; `src/**/bin/` and `src/**/obj/` are excluded from every census.

---

## Correction to the inherited handoff

`HANDOFF-session-6b-viii.md` §3 states: *"A C# enum's `default` is `0`; if no member is `0`, then
`default(ObjectKind)` is a value outside the enumeration."*

**That is false for this enum.** `src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs` declares 37 members
with **no explicit numeric values**, so C# assigns them `0..36` in declaration order and `DoorWood` **is** `0`.
`default(ObjectKind)` is therefore a *fully valid* member that silently means *wooden door* — which is exactly
what the index row at `docs/audit/00-master-index.md:261` says, verbatim:

> `ObjectKind has no zero value, so default(ObjectKind) silently means DoorWood`

The index row is right and the handoff's gloss on it is wrong. The distinction is not cosmetic: it decides the
fix space, because "unrepresentable value" and "wrong representable value" are not repaired the same way, and
because a value outside the enumeration is already rejected by the validation that exists today, while `DoorWood`
is not.

---

## MASTER-073 — `ObjectKind` has no zero value

### The type

`src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs` — 37 members in 5 commented groups, no explicit values,
no `None`. `DoorWood` is the first member and therefore `0`.

### Census

`grep -rn --include=*.cs "ObjectKind" src/` with `bin/` and `obj/` filtered: **234 references across 34 files.**

Targeted search for the forms that would make renumbering dangerous —
`grep -rn --include=*.cs -E "\(ObjectKind\)|default\(ObjectKind\)|Enum\.(IsDefined|TryParse|Parse)"`:

| Form | Hits in production |
|---|---|
| `(ObjectKind)` cast from a numeric value | **0** |
| `default(ObjectKind)` written literally | **0** |
| `Enum.GetValues` / `Enum.IsDefined` over `ObjectKind` | **0** anywhere in the repository |
| `Enum.TryParse<ObjectKind>` / `Enum.Parse<ObjectKind>` | 2 — `ObjectPackRegistry.cs:24` (production), `FakeAssetSource.cs:77` (test). Both parse **by name**. |

### Is the ordinal persisted or serialised anywhere?

**No.** Three independent checks:

- `src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs` maps
  `builder.Property(obj => obj.Kind).IsRequired().HasConversion<string>()` — the column stores the **member
  name**, not the ordinal.
- The only `System.Text.Json` usage over domain types is `AreaConfiguration.cs:87,93`, and it serialises
  `List<PathSegment>` — not `ObjectKind`. The other JSON site, `ObjectPackRegistry.cs`, reads a pack manifest
  whose `Kind` is a **string** parsed by name.
- No numeric cast or `Enum.GetValues` site exists to depend on ordinal order (table above).

**Consequence: renumbering `ObjectKind` is invisible to the database, to the asset packs, and to every call
site.** The concern raised at Gate 1 — that inserting a zero member would renumber all 37 and break persisted
data — **is refuted by the string conversion.** Member *names* are the persistence contract; member *numbers*
are not.

### What validation exists today

`AddPlacedObjectCommandValidator.cs` declares `RuleFor(command => command.Kind).IsInEnum()`.

**That rule cannot reject `default` today**, because `default` is `DoorWood` and `DoorWood` is a defined member.
The rule is present, looks like it guards the field, and guards nothing about an unspecified kind.

### Reachability — the producer, not the validator

Per 6b-vii's finding that *a validator tells you what is rejected and only the producer tells you what is
reachable*, the production chain was traced end to end. There is exactly one of each link:

| Link | Site | Count of production construction sites |
|---|---|---|
| UI event | `DungeonDetailPage.xaml.cs:1433` — guarded by `ObjectPalette.ActiveObject is ObjectKind placedKind` | — |
| | `DungeonDetailPage.xaml.cs:1206` — `new PlaceObjectArgs(floorId, kind, …)` | **1** |
| Command | `MapCanvasViewModel.cs:295` — `new AddPlacedObjectCommand(…, args.Kind, …)` | **1** |
| Entity | `PlacedObject`'s two `internal` constructors; the parameterless one is `private` and exists for EF | — |

`ObjectPalette.ActiveObject` is typed **`ObjectKind?`** (`ObjectPaletteView.xaml.cs:13`), and so is
`FloorMapDrawable.GhostObjectKind` (`:458`). **The UI already models "nothing selected" as `null`, never as a
sentinel value**, and the place path runs only inside a pattern match that has proved the value non-null.

**Therefore `default(ObjectKind)` is not reachable from the UI today.** MASTER-073 is a latent API-design defect,
not a live bug: the type cannot express "unspecified", so any future caller that omits the kind silently gets a
wooden door, and the `IsInEnum()` rule already in place cannot catch it.

### The fix space, with measured consequences

Two candidate designs. Both are cheap; they differ in what they oblige every future caller to do.

**A — number the members from 1 (`DoorWood = 1`, the rest follow implicitly).** `default(ObjectKind)` becomes
`0`, which is then **outside** the enumeration, so the existing `IsInEnum()` rule starts rejecting it with no
validator change, and a domain guard can reject it by the same test. One line changes. Adds no member that
every `switch`, palette and asset map must then handle. Free with respect to persistence, per the section above.

**B — add an explicit `None = 0` member.** Makes absence nameable in the type. But `None` is a *defined* member,
so `IsInEnum()` would accept it and every validator would additionally need `.NotEqual(ObjectKind.None)`; and
`ObjectKindInfo`'s `WallObjects`/`WallAdjacentObjects` sets and `GetObjectSize` switch gain a member that means
nothing to them.

The measurement does not pick between them — the architect proposes and Gate 2 signs off — but the evidence for
A being the smaller obligation is the two rows above, and the case against B is that it reintroduces, as a named
member, exactly the "valid value that means nothing" the entry is about.

---

## MASTER-074 — `Entity` provides no identity-based equality

### The type

`src/DungeonMaster.Domain/Entities/Entity.cs`, 39 lines. It declares `Id` (`Guid`, `private set`, initialised
`Guid.NewGuid()`), `CreatedAt`, `LastModified`, and `protected void Touch()`. It **overrides neither `Equals`
nor `GetHashCode`**, implements **no `IEquatable<>`**, and declares no equality operators. Two instances
therefore compare by reference.

### Blast radius

`grep -rn --include=*.cs -E "class [A-Za-z]+ *: *Entity\b" src/` — **four subclasses, all in the Domain**:
`Area`, `Dungeon`, `Floor`, `PlacedObject`.

Entity-typed collections in production —
`grep -rn --include=*.cs -E "(List|IReadOnlyList|ICollection|IEnumerable|HashSet|Dictionary)<[^>]*(Area|Dungeon|Floor|PlacedObject)\b"`:

- `Dungeon.cs:8` `private readonly List<Floor> _floors = []` (exposed `:48` as `IReadOnlyList`)
- `Floor.cs:10` `private readonly List<Area> _areas = []` (exposed `:74`)
- `Floor.cs:11` `private readonly List<PlacedObject> _objects = []` (exposed `:80`)

**No `HashSet<T>` and no `Dictionary<T, …>` is keyed by an entity anywhere in production.**

Set operations over entities in the Domain, Application and Infrastructure layers —
`grep -rn --include=*.cs -E "\.(Remove|Contains|IndexOf|Distinct|Except|Union|Intersect)\("` over those three
projects returns **four** hits, and only four:

| Site | Call | Does it depend on `Equals`? |
|---|---|---|
| `DungeonMethods.cs:74` | `_floors.Remove(floor)` | Yes — `List<T>.Remove` uses `EqualityComparer<T>.Default` |
| `FloorMethods.cs:98` | `_areas.Remove(area)` | Yes |
| `FloorObjectMethods.cs:73` | `_objects.Remove(obj)` | Yes |
| `DungeonWriteRepository.cs:59` | `dbContext.Dungeons.Remove(dungeon)` | No — EF resolves by primary key through its change tracker, not by `Equals` |

### Every one of the three is already correct

All three follow the identical shape, read at the fixed commit:

```csharp
var floor = _floors.FirstOrDefault(floor => floor.Id == floorId);
if (floor is null) { return MutationOutcome.NotFound; }
_floors.Remove(floor);
```

The instance handed to `Remove` was retrieved **from the same list**, by `Id`, moments earlier. Reference
equality is not merely adequate there — it is exact. Every identity comparison in production already goes
through `.Id` explicitly.

**Therefore MASTER-074 has no live call site either.** It is the same class of defect as MASTER-073: the type
fails to establish something its own design assumes, and today every caller compensates by hand.

**The handoff's stated fear is refuted.** §3 warned that equality semantics *"touch every collection operation
in the Domain and possibly the EF configuration"*. The measured surface is four subclasses, three private lists,
three `Remove` calls, and an EF call that does not consult `Equals` at all.

### What the fix must not break

- **EF Core does not use `Equals`/`GetHashCode` for identity resolution** — it keys the change tracker on the
  primary key — so overriding them does not change tracking, fixups, or the four configurations.
- `Id` is assigned at construction (`Guid.NewGuid()`) and by EF during materialisation via the `private`
  parameterless constructor. A hash computed from `Id` is stable for the lifetime of any instance that is in a
  hash-based collection, and the census above shows there are none today.
- Equality must not treat two different entity **types** with a colliding `Id` as equal; the runtime type
  belongs in the comparison.

---

## What this measurement changes about the session

1. **Neither entry is a live bug.** Both are latent invariants the types fail to establish. The session's
   deliverable is the invariant plus a test that fails without it — the shape 6b-vii ran three times.
2. **MASTER-073 is harder than the index row suggests and easier than Gate 1 feared.** Harder, because
   `default` is a valid member rather than an out-of-range value and the existing `IsInEnum()` rule is inert
   against it. Easier, because the string conversion makes renumbering free.
3. **MASTER-074 is much smaller than the handoff feared** — four subclasses and three call sites, all of which
   already compare by `Id`.
4. **Any acceptance criterion asserting a *live* defect today would be unsatisfiable.** A test written to prove
   a user can reach `default(ObjectKind)` through the UI would fail against correct code, because the producer
   refutes it. Criteria must assert the invariant and the guard, never a reachable-today failure.
