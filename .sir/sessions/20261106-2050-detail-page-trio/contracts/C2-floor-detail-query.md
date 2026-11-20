<!-- frozen at Gate 2: 20261106-2050-detail-page-trio, 2026-11-06 -->

# C2 -- The floor-scoped read: `GetFloorDetailQuery` (defined by T1, consumed by T2)

MASTER-094 / UI-007's second remedy, in the design Gate 2 chose (over changing the command results): a mutation that changes **one floor** refreshes **that floor's** projection instead of
re-running `GetDungeonDetailQuery` for the whole aggregate. This contract is the Application-layer read that
makes that possible. **No command result type changes** (C1 section 4.7 stays at its startRef digest).

Editing hazards and build rules: C1 sections 2 and 3, binding.

## 1. Declarations

Both live in the source tree, which is where the consumer imports them from; nothing in this contract file is
importable and no consumer re-declares either.

| Declaration | Owning file (T1) |
| --- | --- |
| `public record GetFloorDetailQuery(Guid DungeonId, Guid FloorId) : IQuery<FloorDto?>;` | `src/DungeonMaster.Application/Features/Dungeons/Queries/GetFloorDetail/GetFloorDetailQuery.cs` |
| `public sealed class GetFloorDetailQueryHandler(IDungeonReadRepository readRepository) : IQueryHandler<GetFloorDetailQuery, FloorDto?>` | `src/DungeonMaster.Application/Features/Dungeons/Queries/GetFloorDetail/GetFloorDetailQueryHandler.cs` |

- Namespace: `DungeonMaster.Application.Features.Dungeons.Queries.GetFloorDetail`.
- `FloorDto` is the **existing** record in `DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail`
  (`FloorDto.cs`). It is reused, not re-declared, so a refreshed floor and a floor from the full detail are the
  same type and map through the same `DungeonDetailPresentationMapper` path.
- Registration: none by hand. `DependencyInjection.cs` registers every handler by assembly scan
  (`ScannedTypes`, `DependencyInjection.cs:29`, `:137`), and `ValidateHandlerRegistrations` (`:93`) asserts exactly one handler per request type. Queries
  acquire no validation behaviour (the comment at `DependencyInjection.cs:48-49`).

## 2. Behaviour, as the consumer may rely on it

Dispatched as `dispatcher.QueryAsync<FloorDto?>(new GetFloorDetailQuery(dungeonId, floorId), cancellationToken)`.

| Case | Result |
| --- | --- |
| A dungeon `DungeonId` exists and owns a floor `FloorId` | A `FloorDto` **equivalent** to the element with `Id == FloorId` of `GetDungeonDetailQuery(DungeonId)`'s `Floors`: same `Id`, `Number`, `Name`, `Description`, `Notes`, and the same `Areas` (`Id`, `Name`, `Description`, `Notes`, `Outline`) and `Objects` (`Id`, `Kind`, `Position`, `Rotation`, `Scale`) |
| No dungeon has `DungeonId` | `null` |
| The dungeon exists but owns no floor `FloorId` | `null` |
| A floor `FloorId` exists but belongs to a **different** dungeon | `null` -- the floor is never returned through another dungeon's identifier |
| The token is cancelled | The repository's cancellation propagates (`OperationCanceledException`); the handler does not swallow it |

`null` is the **expected-failure value**, exactly as `GetDungeonDetailQuery` returns `null` for a missing dungeon
(`IDungeonReadRepository.cs:21-36`, the `GetDetailAsync` declaration and its doc comment). The consumer decides what a `null` means (C3 section 3: it falls back to the
full reload). No exception is thrown for a missing dungeon or floor.

The read is **one unit of work**: it creates a context, projects with no tracking, and disposes, as
`GetDetailAsync` does (`DungeonReadRepository.cs:48-109`). It reads one floor's rows, not the dungeon's other
floors.

## 3. What T1 may shape freely (not part of the boundary)

The repository method that backs the handler is T1-internal: its only caller is `GetFloorDetailQueryHandler`, so
it is not a boundary between tasks and is not frozen here. The criteria require that it exists on
`IDungeonReadRepository` (so the handler stays a thin delegation, as `GetDungeonDetailQueryHandler.cs:13-20` is)
and that the floor projection cannot drift from `GetDetailAsync`'s: T1 either shares one projection between the
two queries or proves equivalence with a test (manifest T1).
