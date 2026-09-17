# Contract — Domain invariants (session 2026-09-16-core-domain-invariants)

**Frozen at Gate 2.** Three tasks build to this file. A task that needs it changed stops and escalates;
it does not edit this file.

## Why this contract exists

The three guards live in three different files owned by three different tasks, so at first reading they
look independent. They are not. **T1 and T2 write new `PathSegment` values in their tests, and T3 changes
what a legal `PathSegment` is.** T1 and T2 run first. Without this file they would write segments that are
legal today and illegal after T3 lands, and T3 would inherit failures that are not its own. §3 below is
therefore binding on T1 and T2 from the moment they start, before the code that enforces it exists.

## 1. `Area.SetOutline` — MASTER-168

File: `src/DungeonMaster.Domain/Entities/Areas/AreaMethods.cs`

```csharp
internal void SetOutline(IReadOnlyList<PathSegment> outline)
```

Signature is unchanged. Two rejections, both **before** any mutation of `_outline`:

| Condition | Exception, exactly this type | `ParamName` |
|---|---|---|
| `outline` is null | `ArgumentNullException` | `outline` |
| `outline.Count < 3` | `ArgumentException` | `outline` |

- `ArgumentNullException` derives from `ArgumentException`, so tests must discriminate with
  `ThrowExactly<T>()`, never `Throw<T>()`.
- Empty is **included** in the count rejection. There is no separate empty case and **no `ClearOutline`
  affordance is added in this session**.
- The guard lives in `Area.SetOutline` and **nowhere else**. `Dungeon.UpdateAreaOutline` and
  `Floor.UpdateAreaOutline` are unchanged: their `MutationOutcome.NotFound` checks run first, so an
  unknown floor or area with an empty outline still returns `NotFound` and does not throw. Three existing
  tests depend on exactly that ordering and must keep passing unmodified:
  `AreaOutlineTests.UpdateAreaOutline_WithUnknownFloor_IsNoOp`,
  `AreaOutlineTests.UpdateAreaOutline_WithUnknownArea_IsNoOp`, and
  `DungeonAggregateTests.UpdateAreaOutline_WithUnknownAreaId_ReturnsNotFound`.
- Persistence does not route through `SetOutline`. `AreaConfiguration` maps the outline with
  `.HasField("_outline")` and `PropertyAccessMode.PreferFieldDuringConstruction`, so EF materialises into
  the backing field. This guard cannot affect loading.

## 2. `Floor` number — MASTER-169

Files: `src/DungeonMaster.Domain/Entities/Floors/Floor.cs`,
`src/DungeonMaster.Domain/Entities/Floors/FloorMethods.cs`

Both members reject `number < 1` identically, matching the `PlacedObject.cs:30` / `:82` precedent:

```csharp
throw new ArgumentOutOfRangeException(nameof(number), number, "<message>");
```

| Member | Guard |
|---|---|
| `internal Floor(Guid dungeonId, int number, string? name)` | `number < 1` → `ArgumentOutOfRangeException`, `ParamName` `number` |
| `internal void UpdateNumber(int number)` | `number < 1` → `ArgumentOutOfRangeException`, `ParamName` `number` |

- In `UpdateNumber` the guard runs **before** `Number` is assigned and before `Touch()`.
- In the constructor the guard runs **before** any field assignment.
- The private parameterless `Floor()` constructor that EF uses is **unchanged and ungated**. EF
  materialisation must keep working.
- `Dungeon.ReorderFloor` is **not changed**. It keeps returning `MutationOutcome.OutOfRange` for
  `newNumber < 1 || newNumber > _floors.Count` at the aggregate's public edge, and that check runs before
  any `UpdateNumber` call, so the new throw is not reachable from it.
- A `FloorNumber` value object was considered and rejected at Gate 1. Do not introduce one.

## 3. `PathSegment` — MASTER-170

File: `src/DungeonMaster.Domain/ValueObjects/PathSegment.cs`

The positional record keeps its four parameters and their order. Validation goes in the **primary
constructor**, bidirectionally:

| Condition | Exception, exactly this type | `ParamName` |
|---|---|---|
| `Kind == PathSegmentKind.CubicBezier` and `Ctrl1` is null or `Ctrl2` is null | `ArgumentException` | `Ctrl1` or `Ctrl2` — whichever is null; `Ctrl1` when both are |
| `Kind == PathSegmentKind.Line` and `Ctrl1` is not null or `Ctrl2` is not null | `ArgumentException` | `Ctrl1` or `Ctrl2` — whichever is non-null; `Ctrl1` when both are |

`ArgumentException` exactly, not `ArgumentNullException`: the invalid thing is the combination of
parameters, not one missing parameter, and pinning the exact type gives the tests discriminating power.

**The construction form that satisfies this.** C# has no statement body for a positional record's primary
constructor. Redeclare one member with an initializer that validates and returns:

```csharp
public record PathSegment(
	PathSegmentKind Kind,
	TilePoint End,
	TilePoint? Ctrl1,
	TilePoint? Ctrl2)
{
	public PathSegmentKind Kind { get; init; } = ValidateControlPoints(Kind, Ctrl1, Ctrl2);

	private static PathSegmentKind ValidateControlPoints(
		PathSegmentKind kind,
		TilePoint? ctrl1,
		TilePoint? ctrl2) { /* throw per the table, else return kind */ }
}
```

Keep the existing XML doc comments, and update the `Ctrl1` / `Ctrl2` comments so they state the
requirement rather than the current "used only when `CubicBezier` is specified", which is the wording the
audit entry calls out as contradicted.

### 3.1 Two consequences that are accepted, not accidents

- **Validation runs on every database read.** `AreaConfiguration.cs:82-97` persists the outline as `TEXT`
  and deserialises with `JsonSerializer.Deserialize<List<PathSegment>>`. `System.Text.Json` constructs a
  positional record **through its constructor**, so a corrupt or hand-edited row now throws on load rather
  than rendering silently wrong. The user accepted this explicitly at Gate 1. The risk is bounded:
  production has only ever written `Line` segments with null control points
  (`MapDrawingSession.cs:122`, `:125`), so no persisted row can violate the rule.
- **`with` expressions are not validated.** A record's copy constructor does not re-run property
  initializers, so `segment with { … }` bypasses this guard. There is exactly one production `with` site,
  `AreaOutlineGeometry.Translate` (`src/DungeonMaster.App.Core/Map/AreaOutlineGeometry.cs:42`), and it
  translates coordinates while preserving both `Kind` and the null-ness of `Ctrl1` / `Ctrl2` — so it
  cannot turn a valid segment invalid. **Do not change `AreaOutlineGeometry`**; the Domain project is this
  session's whole production surface. This gap is reported to the user as a finding, not closed here.

### 3.2 Binding on every task, including T1 and T2

From the start of T1, every `PathSegment` written anywhere in the repository must already satisfy §3:

- A `PathSegmentKind.Line` segment is constructed with `null, null`.
- A `PathSegmentKind.CubicBezier` segment is constructed with **both** control points non-null.

**Both construction forms count.** The explicit `new PathSegment(…)` and the target-typed `new(…)` inside a
collection expression or array initializer are the same constructor. A census that greps only
`new PathSegment(` undercounts — in this repository it finds 26 sites, and
`grep -rn "new PathSegment(\|new(PathSegmentKind\." src --include=*.cs`, source files only, finds 42.

## 4. What no task may do

- Change any file under `src/DungeonMaster.App`, `src/DungeonMaster.App.Core`, or
  `src/DungeonMaster.Application`. The Domain project is the entire production change surface for this
  session. If a task believes an App-layer or Application-layer change is entailed, that is an escalation.
- Add `src/DungeonMaster.App.UiTests` to `src/DungeonMaster.sln`, or assign work in it.
- Change `Dungeon.ReorderFloor`, `Dungeon.UpdateAreaOutline`, or `Floor.UpdateAreaOutline`.
- Introduce a `FloorNumber` or `Outline` value object.
- Use `sed -i` on any tracked file. Tracked files are CRLF and `IDE0055` is a build error; `sed -i`
  rewrites the file to LF and fails the build on every line. Edit through the edit tools.
