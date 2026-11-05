<!-- frozen: 20261105-0106-core-two-row-sweep, Gate 2 -->

# C2 -- Entity split target: Floor and PlacedObject

No callable operation. T1 owns every file named here. C2 fixes the target composition exactly, so
the move has one correct result and E (C1 section 9) can check it byte for byte, with line endings
stripped.

Authority: `dm-conventions/backend/dotnet-format.md` section 11, "Entity File Split" (`:554-563`).
Entities are split across two files in the same `Entities/{Entity}/` directory: `{Entity}.cs` holds
properties and constructors only, and `{Entity}Methods.cs` holds domain behaviour. Both declare
`public [sealed] partial class {Entity}`. Gate 1 chose "conform both", with no change to dm-conventions.

All paths are under `src/DungeonMaster.Domain/Entities/Floors/`. Line numbers are at startRef
`11f9562`.

## 1. Invariants

- No namespace changes. Every file keeps `namespace DungeonMaster.Domain.Entities.Floors;`. A namespace
  move would rewrite EF model-snapshot type-name strings.
- No member's text, accessibility, modifiers, signature, body or doc comment changes. Only file placement
  changes, plus `partial` on `PlacedObject`'s declaration.
- No other file changes: no `.csproj` (the projects are SDK-style and glob `*.cs`), no test file, and
  not `Floor.cs`. At startRef no `.csproj`, `.props` or `.targets` under `src/` names
  `FloorObjectMethods`, `FloorMethods` or `PlacedObject` (`grep -rln` over those extensions printed nothing).
  `src/DungeonMaster.Tests/Domain/Entities/Floor/FloorObjectMethodsTests.cs` keeps its name and content.
  Test classes there do not mirror source files one to one.

## 2. `FloorMethods.cs` after T1 (282 lines)

In order:
1. startRef `FloorMethods.cs:1-175`, unchanged. That runs through the closing `	}` of `UpdateAreaOutline`.
2. One empty line.
3. startRef `FloorObjectMethods.cs:7-112`, unchanged. That covers `AddObject`, `RemoveObject` and
   `UpdateObject` with their doc comments, in that order, then the type's closing `}`.

startRef `FloorObjectMethods.cs:1-6` duplicates `FloorMethods.cs:1-6` (`using DungeonMaster.Domain.ValueObjects;`,
blank, the namespace, blank, `public sealed partial class Floor`, `{`), so it is dropped.

`FloorObjectMethods.cs` is deleted.

## 3. `PlacedObject.cs` after T1 (63 lines)

1. startRef `PlacedObject.cs:1-7`, unchanged (the using, the namespace and the type summary).
2. Line 8 becomes exactly `public sealed partial class PlacedObject : Entity`. Only `partial ` is inserted.
3. startRef `PlacedObject.cs:9-62`, unchanged (`{`, both constructors at `:10-37`, the five properties at `:39-62`).
4. `}`.

startRef `:63` (the blank line before `Update`'s summary) and `:64-111` move or go, as section 4 says.

## 4. `PlacedObjectMethods.cs`, new (55 lines)

1. These six lines, exactly, none of them indented:

   ```
   using DungeonMaster.Domain.ValueObjects;

   namespace DungeonMaster.Domain.Entities.Floors;

   public sealed partial class PlacedObject
   {
   ```

   The using is required, because `Update` takes a `CanvasPoint`. The declaration carries no base type,
   following `FloorMethods.cs:5`, `AreaMethods.cs:5` and `DungeonMethods.cs:5`. Like those three, it
   carries no type-level doc comment.
2. startRef `PlacedObject.cs:64-112`, unchanged: `Update` (`:64-87`), the blank `:88`, `ValidateScale`
   (`:89-111`) and the closing `}`.

The file is CRLF throughout, ends with CRLF after its `}`, and has no BOM (C1 section 7). One safe route:
`cp` `PlacedObject.cs` to `PlacedObjectMethods.cs` before editing either file, then use `Edit` on the copy
to replace startRef `:5-63` with the declaration and `{`.

## 5. Required measurements after T1

- E (C1 section 9) on each of the three files prints the digest in C1 section 9's table.
- M (C1 section 8) prints the startRef digest for each entity's partial files.
- PC (C1 section 10) prints exactly the eight lines listed there.
- B (C1 section 7) on each of the three files prints CR = LF = CRLF = its line count, loneCR=0. The BOM
  check prints ` 75 73 69`.
