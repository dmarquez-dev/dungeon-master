# The App-to-Application seam

`DungeonMaster.App` references `DungeonMaster.Application`, and until now the seam between them was
whatever happened to be `public`. This file states what `DungeonMaster.App` is *permitted* to consume
from `DungeonMaster.Application`, so that the next reference across the boundary is made with the
statement rather than against silence.

It is the document `XCUT-005` asks for and `MASTER-087` tracks. `XCUT-005`'s finding is that "there is
no statement anywhere of what App is permitted to touch, so the seam is whatever is public".

**Nothing enforces this document.** There is no architecture test, no analyzer, no `InternalsVisibleTo`
boundary and no build step that fails when a type outside the permitted surface is referenced from
`DungeonMaster.App`. The seam is a convention that a reader or a reviewer applies by hand. Treating it
as enforced would be a false confidence this repository's audit already has too much of.

## 1. What App may consume

| Permitted | Namespace as written | Why it is on the list |
|---|---|---|
| The dispatcher contract | `DungeonMaster.Application.Common.Dispatch.IDispatcher` | It is the single entry point through which the presentation layer asks the Application layer to do anything. It is imported under the alias `AppDispatcher`, because `Microsoft.Maui.Dispatching.IDispatcher` occupies the unqualified name. |
| The commands and the queries | `DungeonMaster.Application.Features.*.Commands.*`, `DungeonMaster.Application.Features.*.Queries.*` | These are the request shapes the dispatcher accepts. Naming one is how App states an intent. |
| Their DTOs | The result types declared beside each command and query, in the same namespace | A query's answer has to be readable by the caller that asked for it. |
| The result envelope | `DungeonMaster.Application.Common.Results` | `Result`, `Result<T>`, `ResultMessage`, `ErrorCategory` and `ValidationError` are how an expected failure crosses the boundary instead of an exception. |
| The ports | `DungeonMaster.Application.Common.Interfaces` | On the list because a composition root may legitimately have to name a port when registering its implementation. See the measured note in section 2: no file in `DungeonMaster.App` names one today. |
| The registration entry point | `DungeonMaster.Application.DependencyInjection` | `MauiProgram` calls it to register the layer. |

**And nothing else.** In particular, App does not reach into `Common.Dispatch` beyond `IDispatcher`, does
not name a handler, does not name a pipeline behavior, and does not reach `DungeonMaster.Application`
for geometry, formatting or any other helper that is not part of a request, a response or a port.
A helper that App wants and Application does not use belongs in `DungeonMaster.App.Core`, not here —
which is exactly what section 3 records happening to `Common/Map`.

## 2. What App actually names today, measured

Measured in the worktree on 2026-08-26, after the `Common/Map` move described in section 3, with:

```
grep -rh "^using DungeonMaster.Application" src/DungeonMaster.App --include=*.cs
grep -rh "using AppDispatcher = DungeonMaster.Application" src/DungeonMaster.App --include=*.cs
```

| | Observed |
|---|---|
| Files in `src/DungeonMaster.App/` naming an `Application` namespace | 9 |
| Plain `using DungeonMaster.Application…` lines | 25 |
| Aliased `using AppDispatcher = …IDispatcher;` lines | 3 |
| Distinct namespaces named, the alias included | 19 |
| Of those, `Common.Results` | 3 lines |
| Of those, `Common.Interfaces` (the ports) | 0 lines |

Every one of the 19 falls inside section 1's table. The ports row is permitted and unused: registration
of their implementations happens in `DungeonMaster.Infrastructure`, so `MauiProgram` never has to name
one. That is the current state, not a rule — the row stays permitted.

## 3. What left the Application assembly in this session, and why

Session `2026-08-26-settle-the-app-boundaries` moved `DungeonMaster.Application/Common/Map/` out of the
Application layer. The directory no longer exists.

`MASTER-082` / `CORE-020` found that the three geometry types in it — `TileCoordinateHelper`,
`MapDrawingSession` and `SnapZoneCalculator` — had no caller inside `DungeonMaster.Application`: no
command, no query and no handler in the layer used any of them. They were in the Application assembly
because the map editor needed them, which is a reason for them to be *reachable* from App, not a reason
for them to *live* in Application. Under section 1 they were never part of the permitted surface —
they are not a request, a response or a port.

| Type | Where it went | Why |
|---|---|---|
| `TileCoordinateHelper` | `DungeonMaster.App.Core/Map/`, namespace `DungeonMaster.App.Core.Map` | Presentation geometry with a real caller in `DungeonMaster.App`. |
| `MapDrawingSession`, `DrawSessionResult` | `DungeonMaster.App.Core/Map/`, namespace `DungeonMaster.App.Core.Map` | Same. Its `LastVertex` property had no production caller and was deleted; `Vertices[^1]` reads the same fact. |
| `SnapZoneCalculator` | Deleted | It had no caller anywhere in production — only its own tests. A type with no caller is not answered by giving it a new address. `TileCoordinateHelper.SnapToGrid` already performs canvas-to-tile conversion and survives. |

`DungeonMaster.App.Core` is a `net10.0` class library, so it is reachable from `DungeonMaster.Tests`,
which `DungeonMaster.App` is not. That is the second reason the code went there rather than into
`DungeonMaster.App` itself.

One behaviour moved in the same direction at the same time: the feet-per-tile distance arithmetic that
was inline in `DungeonMaster.App/Graphics/FloorMapDrawable.cs`'s draw method is now
`DungeonMaster.App.Core.Map.TileDistance.InFeet`. `MASTER-027` / `UIKIT-005` suggested moving that
calculation "to Application alongside the other map helpers". That suggestion conflicts with
`MASTER-082`, which moves the helpers *out* of Application, and the conflict was resolved in favour of
`MASTER-082`. **Nothing in this session added a type to `DungeonMaster.Application`.**

## 4. The rule for the next reference across the seam

Before adding a `using DungeonMaster.Application…` to a file in `DungeonMaster.App`, ask which row of
section 1 it falls under. If it falls under none:

1. If the type is a request, a response or a port, it belongs in the Application layer and the row is
   already there — proceed.
2. If App needs it but no command, query or handler in the Application layer uses it, it belongs in
   `DungeonMaster.App.Core`. This is the `Common/Map` case, and section 3 is the worked example.
3. If it is genuinely Application-internal — a handler, a pipeline behavior, a validator — App does not
   name it. Ask for the behaviour through the dispatcher instead.

Nothing checks this at build time. It is checked by whoever reads the diff.

## 5. Where this is pointed at from

- `docs/audit/` — `MASTER-087`, `XCUT-005`, `MASTER-082` and `CORE-020`.
- `docs/persistence-contract.md` is the precedent this file follows: a boundary stated in prose,
  with what is measured marked as measured.
