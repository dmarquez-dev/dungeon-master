# Contract — Core type identity (session 2026-09-17-core-type-identity)

**Frozen at Gate 2.** Two tasks build to this file. A task that needs it changed stops and escalates; it
does not edit this file.

Everything below pins **observable behaviour**: member names and numbering, exception types, `ParamName`
strings, and the equality/hash answers a caller gets. It deliberately pins **no construction form** — how
the guard is written, which helper throws it, and how `GetHashCode` combines its inputs are the
implementer's decisions, provided the observable answers in the tables below are the ones a caller sees.

## Why this contract exists

The two entries look independent and mostly are: T1 owns `ObjectKind.cs`, `PlacedObject.cs` and
`AddPlacedObjectCommandValidator.cs`; T2 owns `Entity.cs`. They are serialised anyway, because they share
one worktree and therefore one `obj/` and one `bin/`, and both must build. `dependsOn` records that, not a
logical dependency.

The one substantive coupling is §3: **`PlacedObject` derives from `Entity`**, so T1's guard tests
construct instances whose equality semantics T2 then changes. §3.4 tells T1 what it may and may not assume
about entity equality while T2 has not landed.

---

## 1. `ObjectKind` gains a `None` member at zero — MASTER-073

File: `src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs`

### 1.1 The decision

**A member named `None` is added as the first member of the enumeration, declared as a bare `None,` with
no explicit numeric value**, above the existing `// ── Doors (wall-snap) ──` group comment and carrying an
XML doc comment saying what it means: no kind has been specified, and it is never a legal kind for a
`PlacedObject`.

- **No explicit numeric value is introduced anywhere in the file.** `None` takes `0` implicitly, and the
  37 existing members keep their declaration order and take `1..37` implicitly. Across the 16 production
  enum declarations in this repository, no member carries an explicit numeric value; this change must not
  introduce the first one.
- The five group comments are preserved. No existing member is renamed, reordered or removed.
- **Member count goes 37 → 38**, by
  `grep -cE "^[[:space:]]+[A-Za-z][A-Za-z0-9]*," src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs`
  at commit `9c2bff6` → `37`. Use that CRLF-safe form: tracked files are CRLF, so the `$`-anchored form
  returns `0` on this repository and must not be used for any count in this session.
- The ordinals of the 37 existing members each shift up by one. That shift is invisible: §1.4.

This is `docs/audit/01-domain-application.md` CORE-017's stated suggested direction, adopted. One clause of
CORE-017 is nonetheless false and must be struck when MASTER-073 is closed — §1.4.

### 1.2 The alternative that was considered and rejected, so nobody re-runs this argument

**Option A — number the members from 1 (`DoorWood = 1`), leaving `default(ObjectKind)` outside the
enumeration — was proposed by the architect, argued, and then withdrawn by the same architect after the
counter-case was built.** Escalation E1; the user ruled for `None`. The reasoning is recorded here in full
because a future reader who sees `None` sitting unused in a 38-member enum will otherwise re-open it.

**Why `None` was chosen:**

1. **The Microsoft enum guideline offers two compliant shapes and A is neither.** The guideline
   (https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum) reads: *"DO provide a value
   of zero on simple enums. Consider calling the value something like 'None.' If such a value is not
   appropriate for this particular enum, the most common default value for the enum should be assigned
   the underlying value of zero."* The escape clause does not license starting at 1 — it prescribes a
   second way to comply. A is outside the guideline, not inside its exception.
2. **CA1008 does not fire either way**, verified by building `DungeonMaster.Domain` with `DoorWood = 1`
   applied at `9c2bff6`: `0 Warning(s), 0 Error(s)` at `AnalysisMode=Recommended`, `AnalysisLevel=10.0`,
   `TreatWarningsAsErrors=true`. Under A that silence is a cost: nothing in the toolchain would ever tell
   a future reader why the enum starts at 1, and the justification would live only in this file and a
   closed audit row. Under `None` the member's own name is the explanation.
3. **A does not fix the `ObjectKindInfo` fallbacks, which was the architect's own strongest argument
   against `None` and was wrong.** Those three arms are `_ =>`, not member-matched, so
   `GetObjectSize((ObjectKind)0)` returns `(1.00, 1.00)` and `GetCategory((ObjectKind)0)` returns
   `"Decoration"` under A exactly as they do for `None`. A makes the absorbed value unnamed rather than
   named; it does not stop the absorption. The residual is real under either option and is recorded as
   §2.2.
4. **The obligation `None` imposes is one line, once.** Exactly one boundary in this codebase accepts an
   `ObjectKind` from outside — `AddPlacedObjectCommand.Kind`, validated at
   `AddPlacedObjectCommandValidator.cs:27`. `UpdatePlacedObjectCommand` carries no `Kind`. That one line
   is pinned in §2 and tested. A's obligation, by contrast, falls on every future reader of the type,
   unassisted and forever.
5. **The precedent reaches further than this entry.** Fourteen of the repository's sixteen production
   enums have a concrete member at zero, `MutationOutcome { Success, NotFound, OutOfRange }` among them —
   so `default(MutationOutcome)` is `Success`, the same defect class, unfiled. Whatever is chosen here is
   the template for those. "Add a `None` member" is mechanical; "renumber from 1" is not.
6. **`None` is the smaller departure in form.** A would introduce the repository's first explicit enum
   value. A bare `None,` declared first does not.

**And A's central claim — that it makes the invalid state unrepresentable — is only half true.** The
architect ran the round trip on SDK `10.0.400` before withdrawing A. `EnumToStringConverter` writes
`value.ToString()`, and `Enum.Parse` accepts numeric strings:

| Probe | Observed |
|---|---|
| `((K)0).ToString()` where `K` starts at 1 | `0` |
| `Enum.TryParse<K>("0", out var back)` | `True`, `back` = `0`, `Enum.IsDefined(back)` = `False` |
| `Enum.Parse<K>("0")` throws? | no |
| `N.None.ToString()` round trip where `N` starts at `None` | `None` → `True`, value `None`, `IsDefined` = `True` |

So under A an undefined `(ObjectKind)0` would persist as the string `0` and materialise back silently
through the private EF constructor that no guard covers. A and `None` are equally exposed at that
boundary; A's only surviving edge was detectability afterwards, and the Domain has exactly one
construction path (§2) at which both are rejected identically.

**Two arguments the architect originally made for A are withdrawn as not evidence.** The `ObjectKind?`
sites at `ObjectPaletteView.xaml.cs:13` and `FloorMapDrawable.cs:458` are UI *selection* state — "the user
has not picked a tool" — a different concept from "this command arrived without a kind". And `Scale` has
no sentinel because `float` offers none, so MASTER-075's range-guard remedy was forced, not chosen, and
says nothing about enum design.

### 1.3 Observable consequences a test may assert

| Expression | Required answer |
|---|---|
| `(int)ObjectKind.None` | `0` |
| `default(ObjectKind)` | `ObjectKind.None` |
| `Enum.IsDefined(ObjectKind.None)` | **`true`** — this is why §2's guard needs its own `None` condition and why the validator needs `.NotEqual` |
| `(int)ObjectKind.DoorWood` | `1` |
| `(int)ObjectKind.DoorIron` | `2` |
| `Enum.GetValues<ObjectKind>().Length` | `38` |
| number of members casting to `0` | exactly one, and it is `None` |
| `Enum.Parse<ObjectKind>("DoorWood")` | `ObjectKind.DoorWood` (unchanged — parsing is by name) |

### 1.4 Why the ordinal shift is invisible, and the CORE-017 clause it refutes

CORE-017 states *"the underlying numeric values of every existing member shift by one, so any persisted
integer column has to be migrated -- hence medium risk; T2 owns the persistence side."* **There is no
persisted integer column.** `src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs`
maps `builder.Property(obj => obj.Kind).IsRequired().HasConversion<string>()`, so the column stores the
member *name*. `ObjectPackRegistry.cs:24` and `FakeAssetSource.cs:77` both parse by name. There is no
numeric cast over `ObjectKind` anywhere in the repository and no `Enum.GetValues` or `Enum.IsDefined` site
that depends on ordinal order. No migration is entailed and no Infrastructure file is touched by this
session. Whoever closes MASTER-073 must strike that clause rather than leave it standing.

## 2. The single chokepoint rejects `None` and any undefined value — MASTER-073

Files: `src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs`,
`src/DungeonMaster.Application/Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommandValidator.cs`

### 2.1 The domain guard

The five-parameter `internal PlacedObject(Guid floorId, ObjectKind kind, CanvasPoint position,
ObjectRotation rotation, float scale)` keeps its signature, parameter names and order unchanged, and gains
**one guard with two conditions**:

| Condition | Exception, exactly this type | `ParamName` | `ActualValue` |
|---|---|---|---|
| `kind` is `ObjectKind.None` | `ArgumentOutOfRangeException` | `kind` | `ObjectKind.None` |
| `kind` is not a defined member of `ObjectKind` | `ArgumentOutOfRangeException` | `kind` | the offending `kind` value |

Binding details:

- **Both conditions are required.** `Enum.IsDefined(ObjectKind.None)` is `true`, so the second condition
  does not imply the first. A guard carrying only the `IsDefined` test would admit `None`, which is the
  whole defect; a guard carrying only the `None` test would admit a cast from a numeric value.
- **Ordering is part of the contract.** The `kind` guard runs **before** the existing `scale` rejection
  and **before** the first field assignment. A call that is bad in both `kind` and `scale` therefore
  throws with `ParamName` `kind`, deterministically.
- The exception type and the three-argument shape (`paramName`, `actualValue`, message) match the existing
  `scale` precedent at `PlacedObject.cs:30` and `:82`. This is repository precedent, not a .NET-wide rule;
  `InvalidEnumArgumentException` is deliberately **not** used, because nothing in this repository uses it
  and the Domain does not reference `System.ComponentModel`.
- **The four-parameter overload is not separately guarded.** It delegates to the five-parameter one, so the
  guard covers both. Do not add a second copy.
- **The `private PlacedObject()` constructor stays ungated.** EF materialises through it; a guard there
  would break loading. Note the consequence honestly: a stored row whose `Kind` column reads `None` would
  materialise cleanly. §2.4 is the criterion that keeps such a row from ever being written.
- **`Floor.AddObject` is not changed.** Both overloads in `FloorObjectMethods.cs` pass `kind` straight into
  the constructor, so the throw propagates without an aggregate-edge check. That file is outside T1's
  ownership set, and it is the only caller of the constructor — which is what makes this a single
  chokepoint.
- **No `Update` overload gains a kind guard.** `PlacedObject.Update` does not take an `ObjectKind`.

### 2.2 The validator

`AddPlacedObjectCommandValidator.cs` keeps its existing `RuleFor(command => command.Kind).IsInEnum()` at
`:25-27` and **adds a `.NotEqual(ObjectKind.None)` rule to the same property**. Both rules are required,
for the reason in §2.1: `IsInEnum()` is `Enum.IsDefined` underneath and `None` is defined, so `IsInEnum()`
alone accepts it.

- `UpdatePlacedObjectCommandValidator.cs` is **not** changed. Its command carries no `Kind`
  (`UpdatePlacedObjectCommand.cs`), so its `IsInEnum()` at `:35` is over `NewRotation` and is unrelated.
- No other validator in the Application layer takes an `ObjectKind`. Any future one must carry both rules.

### 2.3 Vacuity notes, binding on the tests

Two distinct traps here, and both must be closed:

1. **Exception-type collision.** This same constructor already throws `ArgumentOutOfRangeException` for an
   out-of-range `scale`, so a test asserting only `ThrowExactly<ArgumentOutOfRangeException>()` could pass
   for the wrong reason. **Every rejection test asserts `.Which.ParamName` is `kind`**, and asserts
   `.Which.ActualValue` is the value passed in, which is also what distinguishes the two rejection
   conditions from each other.
2. **Condition collapse.** Because the guard has two conditions, a test suite covering only one of them
   would pass against a guard missing the other. The `None` case and the undefined-cast case each need
   their own rejection test, and the ordering clause needs its own test too — see the manifest's
   fact-by-fact specification.

Neither condition duplicates a downstream rejection: with the guard removed,
`new PlacedObject(floorId, ObjectKind.None, position, rotation)` constructs successfully and
`_objects.Add(obj)` accepts it, so the rejection tests genuinely fail without the guard.

### 2.4 Accepted residual — `ObjectKindInfo` absorbs `None`

**Named here so it cannot be mistaken for an oversight.** `src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs`
ends `GetObjectSize` (`:67`), `GetDisplayName` (`:109`) and `GetCategory` (`:129`) with `_ =>` arms, and
routes `GetDefaultAnchor` through two `HashSet` misses to `SnapAnchor.Center`. An `ObjectKind.None` that
reached them would render as a 1.00 × 1.00 centre-anchored `"None"` in category `"Decoration"` rather than
failing.

- This is **not fixed in this session**. `ObjectKindInfo.cs` is in the App layer; the Domain is this
  session's whole production surface, and no task owns that file.
- It is also **not** a regression introduced by `None`: the same arms absorb any undefined `ObjectKind`
  identically today, so the gap predates this change (§1.2, point 3).
- Ruled at Gate 2: **carried to the Gate 3 backlog, not filed as a new audit entry this session.**
- No task may edit `ObjectKindInfo.cs`. A task that concludes it must, stops and escalates.

## 3. `Entity` has identity equality — MASTER-074

File: `src/DungeonMaster.Domain/Entities/Entity.cs`

### 3.1 Members

`Entity` implements `IEquatable<Entity>` and overrides `Equals(object?)` and `GetHashCode()`. `Id`,
`CreatedAt`, `LastModified` and `Touch()` keep their current declarations, accessibility and XML doc
comments; `Id` keeps its `private set`.

**No `==` or `!=` operator is declared.** This is a frozen decision, not an oversight, and it matches
CORE-018's own suggested direction (*"Implement identity equality on `Entity` … with `GetHashCode` over
`Id`"*), which names neither operator. Declaring them would silently change the meaning of every existing
`==` over an entity-typed expression in the repository — a surface this session has not measured — while
`Equals`/`GetHashCode` alone are what `List<T>.Remove`, `List<T>.Contains`, `Distinct`, `Except` and
hash-based collections consult, which is the whole of the entry. A task that believes the operators are
needed stops and escalates.

### 3.2 Observable behaviour

Let `a` and `b` be `Entity` instances.

| Case | `a.Equals(b)` — both the `Entity?` and the `object?` overload | `a.GetHashCode() == b.GetHashCode()` |
|---|---|---|
| `ReferenceEquals(a, b)` | `true` | `true` |
| `a.GetType() == b.GetType()` and `a.Id == b.Id`, distinct instances | **`true`** | **`true`** |
| `a.GetType() == b.GetType()` and `a.Id != b.Id` | `false` | not specified |
| `a.GetType() != b.GetType()` (e.g. `Area` vs `Floor`), same `Id` | **`false`** | not specified |
| `b` is `null` | `false` | — |

- Runtime type participates via `GetType()`, not via a pattern match on a named type. All four subclasses
  are `sealed` today (`Area`, `Dungeon`, `Floor`, `PlacedObject`, by
  `grep -rn --include=*.cs -E "class [A-Za-z]+ *: *Entity\b" src/` at `9c2bff6`), so the two readings agree
  now; `GetType()` keeps agreeing if one is unsealed later.
- `Equals` is reflexive, symmetric and transitive over the cases above, and never throws.

### 3.3 Hash stability — the constraint that outranks distribution quality

`GetHashCode()` is computed from `Id`, optionally combined with the runtime type. It **must not**
incorporate `CreatedAt`, `LastModified`, or any other mutable state. `Touch()` mutates `LastModified` on a
live instance, so a hash that read it would change while the instance sat in a hash-based collection.

There are no entity-keyed `HashSet<T>` or `Dictionary<T, …>` in production today — the measurement's
census records zero — so this constraint is forward-looking. It is binding regardless.

`Id` is assigned at construction (`Guid.NewGuid()`) and again by EF during materialisation through the
private parameterless constructor, so no entity is ever transiently `Guid.Empty` in the constructed-object
path, and two independently constructed entities never collide.

### 3.4 What must not change

- **EF Core identity resolution is unaffected.** The change tracker keys on the primary key, not on
  `Equals`/`GetHashCode`. None of the four `IEntityTypeConfiguration` files is edited, and
  `DungeonWriteRepository.cs:59`'s `dbContext.Dungeons.Remove(dungeon)` continues to resolve by key.
- **The three Domain `Remove` sites are not edited.** `DungeonMethods.cs:74`, `FloorMethods.cs:98` and
  `FloorObjectMethods.cs:73` each retrieve the instance from the same list by `Id` immediately before
  removing it, so reference equality was already exact there. Their behaviour must be identical after the
  change; they are outside T2's ownership set.
- **T1 may assume nothing about entity equality.** T1 runs first. Its tests must not assert equality or
  inequality of two `PlacedObject` instances, and must not rely on `Contains`, `Distinct` or a
  `HashSet<PlacedObject>`. Comparing `obj.Kind`, `obj.Id` and the other properties directly is what T1
  needs and is unaffected by T2.

### 3.5 How a test obtains two distinct instances sharing an `Id`

`Id` has a `private set`, so no public API produces this state. The mechanism is reflection on the
declaring type's property:

> `typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(instance, someGuid)`

**This was executed by the architect before freezing**, on SDK `10.0.400`, against a shape equivalent to
`Entity` (abstract base, `public Guid Id { get; private set; } = Guid.NewGuid();`, two sealed subclasses).
Observed: `CanWrite=True`, setter `set_Id` non-public, `SetValue` succeeded on both a same-type and a
cross-type instance, and the two instances then compared `Id`-equal and reference-unequal. It is recorded
here so the implementer does not have to guess and does not have to invent a production seam.

**No production seam is added to make this testable.** `Id` does not become `internal set`, no test-only
constructor or factory is introduced, and `AssemblyInfo.cs` is not edited — `[assembly:
InternalsVisibleTo("DungeonMaster.Tests")]` already exists on the Domain and is enough to reach the
internal constructors.

### 3.6 Vacuity note, binding on the tests

Today `Entity` uses reference equality, so **three of the natural assertions already pass against
unchanged code** and prove nothing on their own:

| Assertion | Passes today? | Role |
|---|---|---|
| two instances with **different** `Id` are not equal | **yes** | control only |
| an `Area` and a `Floor` with the **same** `Id` are not equal | **yes** | control only |
| `x.Equals(null)` is `false`; `x.Equals(x)` is `true` | **yes** | control only |
| two instances of the same type with the **same** `Id` are equal | no | **discriminating** |
| their `GetHashCode()` values are equal | no | **discriminating** |
| `dungeon.Floors.Contains(clone)` where `clone` shares the tracked floor's `Id` | no | **discriminating** |

The discrimination evidence required by §4 must come from the discriminating rows. A run in which only the
control rows were observed failing, or in which the controls alone were cited, does not satisfy it.

### 3.7 Namespace shadowing trap

A test file placed at `src/DungeonMaster.Tests/Domain/Entities/` sits in namespace
`DungeonMaster.Tests.Domain.Entities`, which **contains the child namespaces `.Area`, `.Dungeon` and
`.Floor`** (the sibling folders). A bare `Floor`, `Area` or `Dungeon` in that file therefore binds to a
*namespace*, not to the domain type, and the file will not compile. Alias the domain types — for example
`using DomainFloor = DungeonMaster.Domain.Entities.Floors.Floor;` — exactly as the files under
`Domain/Entities/Floor/` already alias `Dungeon`.

## 4. Rules binding on both tasks

- **Discrimination is run, not asserted.** Each task removes or weakens its own change, runs a named
  filtered test command, records which tests failed **and which named positive control stayed green**,
  then restores and re-runs to green. A control that also fails means the run was broken by something else
  and the evidence does not count.
- **A census over a C# type covers the target-typed form.** `grep "new TypeName("` misses every `new(...)`
  inside a collection expression or a target-typed assignment. Any construction census in this session
  greps both forms.
- **Every count uses the CRLF-safe grep form.** Tracked files are CRLF; a `$`-anchored pattern silently
  returns `0`. This is not hypothetical — it returned `0` for this enum's member count during
  decomposition.
- **`IDE0055` fails the build.** `Directory.Build.props:5` sets `TreatWarningsAsErrors=true` and `:16` sets
  `EnforceCodeStyleInBuild=true`, so a formatting slip is a build error even though `.editorconfig:64`
  reads `severity = warning`. `.editorconfig` sets `indent_style = tab`; every edit goes through the edit
  tools and `sed -i` is never used on a tracked file.
- **No absolute suite total is a criterion.** Each task runs the suite before its own edits and after, and
  reports both numbers with the delta. A total anchored to a commit cannot be verified by a task that did
  not measure it.
- **Close `DungeonMaster.App` before a full-solution build**, or the Windows TFM fails with MSB3026 /
  MSB3027. `src/DungeonMaster.App.UiTests` is deliberately outside the solution; do not add it.
