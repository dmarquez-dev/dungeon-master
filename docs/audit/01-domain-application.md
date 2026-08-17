# Domain and Application -- code quality audit

## Scope

| Directory / glob | Files | Lines |
|---|---|---|
| `src/DungeonMaster.Domain/*.cs` (root) | 1 | 3 |
| `src/DungeonMaster.Domain/Entities/Entity.cs` | 1 | 28 |
| `src/DungeonMaster.Domain/Entities/Area/**` | 2 | 107 |
| `src/DungeonMaster.Domain/Entities/Dungeon/**` | 2 | 503 |
| `src/DungeonMaster.Domain/Entities/Floor/**` | 10 | 559 |
| `src/DungeonMaster.Domain/ValueObjects/**` | 4 | 60 |
| `src/DungeonMaster.Application/Common/Behaviors/**` | 1 | 48 |
| `src/DungeonMaster.Application/Common/Dispatch/**` | 7 | 233 |
| `src/DungeonMaster.Application/Common/Map/**` | 3 | 224 |
| `src/DungeonMaster.Application/Common/Results/**` | 2 | 86 |
| `src/DungeonMaster.Application/DependencyInjection.cs` | 1 | 105 |
| `src/DungeonMaster.Application/Features/Dungeons/*.cs` (root) | 2 | 101 |
| `src/DungeonMaster.Application/Features/Dungeons/Commands/**` | 32 | 984 |
| `src/DungeonMaster.Application/Features/Dungeons/Queries/**` | 9 | 211 |
| **Total** | **77** | **3,252** |

Files read: **77**. Files cited in at least one record: **62**.

No hand-written file in either layer was excluded from reading. Neither project's source tree contains
generated code -- there are no EF Core migrations, no `*.Designer.cs` and no XAML source-generator
output -- so every one of the 77 files was audited against the same hand-written standard. One
category was excluded:

- `src/DungeonMaster.Domain/obj/**` and `src/DungeonMaster.Application/obj/**` (6 `.cs` files:
  `*.AssemblyInfo.cs`, `*.GlobalUsings.g.cs`, `.NETCoreApp,Version=v10.0.AssemblyAttributes.cs`) --
  MSBuild intermediate output, regenerated on every build rather than authored, and therefore not
  counted in the table above.

The following 15 files were read and found sound; they are not cited by any record:

- `src/DungeonMaster.Domain/ValueObjects/PathSegmentKind.cs` -- fully documented enum, both members carry XML docs.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommand.cs` -- conforms to the documented command shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommandValidator.cs` -- conforms to the documented validator shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/CreateDungeon/CreateDungeonCommand.cs` -- conforms to the documented command shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/CreateDungeon/CreateDungeonCommandValidator.cs` -- conforms to the documented validator shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateArea/UpdateAreaCommand.cs` -- conforms to the documented command shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateArea/UpdateAreaCommandValidator.cs` -- conforms to the documented validator shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateDungeon/UpdateDungeonCommand.cs` -- conforms to the documented command shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateDungeon/UpdateDungeonCommandValidator.cs` -- conforms to the documented validator shape.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/AreaDto.cs` -- plain projection record, fully documented.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/DungeonDetailDto.cs` -- plain projection record, fully documented.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/FloorDto.cs` -- plain projection record, fully documented.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/GetDungeonDetailQuery.cs` -- conforms to the documented query shape.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeons/DungeonSummaryDto.cs` -- plain projection record, fully documented.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeons/GetDungeonsQuery.cs` -- conforms to the documented query shape.

## Method

- Base commit: `45495c1bd39a52dbc942eceab897b3c1ac553ee9`.
- `dm-conventions` documents read:
  - `backend/dotnet-format.md` -- 794 lines, **read in full**.
  - `backend/backend-conventions.md` -- 251 lines, read in full.
  - `backend/dotnet-guidelines.md` -- 532 lines; sections 1 (Async / Await), 4 (Memory & Allocations)
    and 6 (Exception Handling) read in full, the HttpContext and HTTP-client sections skimmed and
    judged inapplicable to a MAUI-hosted stack with no ASP.NET Core surface.
- Microsoft Learn pages consulted (each fetched during this audit):
  - https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces
  - https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions
  - https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum
  - https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
  - https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-reference-types
  - https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/
  - https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/incompatibilities
- Tooling, all read-only: `git rev-parse` for the base SHA; `find` and `wc -l` for the scope counts;
  `grep` across `src/` to establish which Domain and Application members have production callers as
  opposed to test-only callers. No build, no test run, no formatter, no analyzer, and no write of any
  kind under `src/`.

## Summary

| Severity | Count |
|---|---|
| S1 | 0 |
| S2 | 5 |
| S3 | 16 |
| S4 | 9 |
| S5 | 3 |
| **Total** | **33** |

## Records

### CORE-001 -- Aggregate mutators silently no-op on an unknown child id and the command still reports success

```yaml
id: CORE-001
layer: CORE
severity: S2
category: correctness
authority: conventions
citation: backend/backend-conventions.md#2. CQRS Patterns
locations:
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:61-84
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:95-138
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:155-171
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:215-229
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:249-271
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:286-303
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:392-408
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:429-452
  - src/DungeonMaster.Domain/Entities/Floor/FloorMethods.cs:72-84
  - src/DungeonMaster.Domain/Entities/Floor/FloorMethods.cs:101-120
effort: M
risk: medium
independent: true
```

**What.** Every `Dungeon` mutator that takes a `floorId`, `areaId` or `objectId` returns `void` and
returns early when the lookup misses -- `RemoveFloor`, `UpdateFloor`, `RemoveArea`, `UpdateArea`,
`UpdateAreaOutline`, `RemovePlacedObject`, `UpdatePlacedObject` and the two `Floor` counterparts all do
`if (floor is null) { return; }`. `ReorderFloor` additionally returns early when `newNumber` is outside
`1..Count` (`DungeonMethods.cs:114-117`). The corresponding handlers cannot observe the miss and
unconditionally return `Result<bool>.Success(true)`.

**Why it matters.** `backend-conventions.md` section 2 states that a command "returns a typed result
wrapper indicating success or failure with an error type". Here the wrapper reports success for an
operation that did not happen. A stale floor or area id -- from a view model held across a delete, or a
reorder to an out-of-range number -- produces a green result, a `Touch()`-free aggregate and a pointless
`UpdateAsync` round-trip. Today the UI does not go on to show stale state: every dispatch call site in
`DungeonDetailViewModel.cs` discards the `Result` and immediately re-reads the aggregate via
`LoadDungeonAsync(_dungeonId)` (lines 159-163, 176-185, 197-204), so the no-op is corrected on the next
render. That is a property of the caller, not of the contract: the layer's own success signal is
unreliable, and the first caller that trusts it -- or stops reloading -- inherits the defect.

**Suggested direction.** Give the aggregate mutators a `bool` or small outcome return value, as
`Floor.RemoveObject` and `Floor.UpdateObject` already do, and have each handler map a miss to
`ErrorCategory.NotFound`. `ReorderFloor`'s range rejection is a distinct outcome from "floor not
found" and is worth distinguishing.

### CORE-002 -- AddArea throws InvalidOperationException for a missing floor while siblings return a Result

```yaml
id: CORE-002
layer: CORE
severity: S2
category: correctness
authority: conventions
citation: backend/dotnet-guidelines.md#6.1 Do Not Use Exceptions for Normal Control Flow
locations:
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:196-198
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommandHandler.cs:31-34
effort: S
risk: low
independent: true
```

**What.** `Dungeon.AddArea` resolves the floor with `FirstOrDefault(...) ?? throw new
InvalidOperationException($"Floor {floorId} not found.")`. `AddAreaCommandHandler` calls it without a
`try`/`catch`, so the exception propagates out of the handler, out of `Dispatcher.SendAsync` and into
the caller. The neighbouring `AddPlacedObject` path returns `null` for the same condition and
`RemoveArea` / `UpdateArea` ignore it entirely.

**Why it matters.** `dotnet-guidelines.md` section 6.1 states that a predictable "not found" condition
must be expressed through `Result<T>`, not an exception. There is no dispatcher-level or
composition-root-level exception boundary in this layer, so a bad `FloorId` on `AddArea` escapes the
Result contract that all fourteen commands otherwise advertise. Three different reactions to one
condition also means no caller can write a single, correct error path.

**Suggested direction.** Make `AddArea` return `Area?` -- matching `AddPlacedObject` -- and have the
handler map `null` to `ErrorCategory.NotFound` with the floor id in the message. Then settle on one
convention for "child not found" across the whole aggregate.

### CORE-003 -- ValidationBehavior validates synchronously and never passes the cancellation token

```yaml
id: CORE-003
layer: CORE
severity: S2
category: async
authority: conventions
citation: backend/dotnet-guidelines.md#1.4 Pass CancellationToken Through the Call Stack
locations:
  - src/DungeonMaster.Application/Common/Behaviors/ValidationBehavior.cs:23-37
effort: XS
risk: low
independent: true
```

**What.** `HandleAsync` accepts a `CancellationToken ct` and then calls
`validator.Validate(context)` -- FluentValidation's synchronous entry point, which takes no token. The
token is used nowhere in the method; it is only forwarded implicitly by `await next()`.

**Why it matters.** `dotnet-guidelines.md` section 1.4 requires that a `CancellationToken` accepted by
an async method be propagated to every awaited call. Beyond the rule, the synchronous overload throws
if any registered validator ever declares an async rule (`MustAsync`, `CustomAsync`), so the first
async validation rule added to this codebase fails at runtime inside a method whose signature already
promised async execution.

**Suggested direction.** Call `await validator.ValidateAsync(context, ct)` and aggregate the results,
or iterate the validators with `await` rather than composing them through a synchronous LINQ chain.

### CORE-004 -- Entity namespaces share the name of the type they contain, forcing Floor.Floor qualification

```yaml
id: CORE-004
layer: CORE
severity: S2
category: naming
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces
locations:
  - src/DungeonMaster.Domain/Entities/Dungeon/Dungeon.cs:1-6
  - src/DungeonMaster.Domain/Entities/Floor/Floor.cs:3-8
  - src/DungeonMaster.Domain/Entities/Area/Area.cs:3-8
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:44-47
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:323-333
  - src/DungeonMaster.Domain/Entities/Floor/FloorMethods.cs:51-58
effort: M
risk: medium
independent: true
```

**What.** `namespace DungeonMaster.Domain.Entities.Dungeon` contains a type named `Dungeon`; the same
holds for `Floor` and `Area`. As a direct consequence, `DungeonMethods.cs` must write
`Floor.Floor`, `Floor.PlacedObject`, `Floor.ObjectKind`, `Floor.ObjectRotation`, `Floor.ObjectScale`
and `Area.Area` throughout, and `FloorMethods.cs` must write `Area.Area`.

**Why it matters.** The Microsoft namespace-naming guideline is explicit: "DO NOT use the same name
for a namespace and a type in that namespace... Several compilers require such types to be fully
qualified." That prediction is exactly what happened here -- roughly forty qualified references in
`DungeonMethods.cs` alone. The cost compounds: every new Domain type inside these folders inherits the
qualification tax, and `Floor.ObjectScale` reads as though `ObjectScale` were nested inside the `Floor`
entity when it is a sibling static class.

**Suggested direction.** Either flatten to `DungeonMaster.Domain.Entities` (keeping the per-entity
folders but not the per-entity namespace, which the `dm-conventions` folder layout does not require),
or pluralise the namespace leaf (`Entities.Floors`) so that folder structure and namespace can both
survive. Either change touches the Application and App layers' `using` lists, hence the medium risk.

### CORE-005 -- Result<T>.Value is T? on an unconstrained T, so null on failure is unreachable for value types

```yaml
id: CORE-005
layer: CORE
severity: S3
category: api-design
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-reference-types
locations:
  - src/DungeonMaster.Application/Common/Results/Result.cs:9
  - src/DungeonMaster.Application/Common/Results/Result.cs:16-19
effort: S
risk: low
independent: true
```

**What.** `Result<T>` declares `public T? Value { get; init; }` with no constraint on `T`, and its XML
summary says the property is "the value returned by the operation, or `null` when the operation
failed". Every command in this layer closes `T` as `Guid` or `bool`, both value types.

**Why it matters.** The Microsoft nullable-reference-types reference states that for an unconstrained
type parameter with a value-type argument, "`T` is `int` and `T?` is also `int` -- the annotation has
no effect on value types unless the type parameter has the `struct` constraint". So on failure
`Value` is `Guid.Empty` / `false`, never `null`, and the documentation misdescribes the type for
fourteen of the sixteen dispatchable operations. Callers are pushed into sentinel checks; the App layer
already does exactly that (`ViewModels/DungeonDetailViewModel.cs:302` tests
`result.Value == Guid.Empty`), which is a null-check surrogate that would silently mis-fire the day a
command legitimately returns an empty `Guid`.

**Suggested direction.** Either constrain the story -- expose `TryGetValue(out T value)` or a
`Match`-style accessor that cannot be read on a failed result -- or, at minimum, correct the XML
summary to state that `Value` holds `default(T)` on failure and stop callers from treating the default
as a failure signal.

### CORE-006 -- Result<T> permits invalid states and every state-changing command returns an always-true bool

```yaml
id: CORE-006
layer: CORE
severity: S3
category: api-design
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Results/Result.cs:11-29
  - src/DungeonMaster.Application/Features/Dungeons/Commands/DeleteDungeon/DeleteDungeonCommandHandler.cs:19-23
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveFloor/RemoveFloorCommandHandler.cs:31-37
effort: M
risk: medium
independent: true
```

**What.** All four `Result<T>` members are `public ... { get; init; }`, so
`new Result<Guid> { IsSuccess = true, Error = "boom" }` is constructible alongside the two intended
factories. Separately, nine of the fourteen commands are declared `ICommand<bool>` and their handlers
end in `return Result<bool>.Success(true);` with no path that ever yields `Success(false)`;
`DeleteDungeonCommandHandler` returns success even when `DeleteAsync` deleted nothing.

**Why it matters.** I believe the `bool` payload is carrying no information -- it appears to be a
stand-in for "no return value", and a reader has to check each handler to discover that `false` is
unreachable. The public `init` accessors appear to leave the invariant "success implies no error"
enforced only by convention rather than by the type. Both make the error model weaker than the
discipline the rest of the layer shows.

**Suggested direction.** Consider adding a non-generic `Result` (or a `Unit` payload) for commands with
no return value, and making the properties `private init` / positional so the static factories are the
only way to construct one.

### CORE-007 -- Two ErrorCategory members are never produced and no production caller reads the category

```yaml
id: CORE-007
layer: CORE
severity: S3
category: api-design
authority: conventions
citation: backend/backend-conventions.md#2. CQRS Patterns
locations:
  - src/DungeonMaster.Application/Common/Results/ErrorCategory.cs:6-27
  - src/DungeonMaster.Application/Common/Results/Result.cs:26-29
effort: S
risk: low
independent: true
```

**What.** `ErrorCategory` declares `Validation`, `NotFound`, `Conflict` and `Unauthorized`. A
repository-wide search finds `Conflict` and `Unauthorized` produced nowhere, `Forbidden` -- listed in
the conventions' error-category set -- absent, and `Result.ErrorCategory` read only by the test
project; the three App view models that consume results read `IsSuccess` and `Error` only.

**Why it matters.** `backend-conventions.md` section 2 defines the category set as
`(Validation, NotFound, Conflict, Unauthorized, Forbidden)` and gives the category a job: the calling
layer maps it to a status. In this application nothing maps it, so the category is a write-only field
that costs a decision at every failure site and buys nothing today. Two of its four values have never
had a producer, which makes the enum look larger and more considered than it is.

**Suggested direction.** Either give the category a consumer -- a single result-to-message mapping the
view models share -- or reduce the enum to the values that are actually produced and grow it again
when a caller needs the distinction. Align the member set with the conventions either way.

### CORE-008 -- ValidationBehavior discards every validation failure except the first

```yaml
id: CORE-008
layer: CORE
severity: S3
category: api-design
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Behaviors/ValidationBehavior.cs:30-44
effort: S
risk: low
independent: false
blocked-by:
  - CORE-006
```

**What.** The behavior materialises every failure from every registered validator into a `List`, then
returns `Result<TResult>.Failure(failures[0].ErrorMessage, ErrorCategory.Validation)` -- a single
string built from the first element. The remaining failures, and the `PropertyName` of every failure,
are dropped.

**Why it matters.** I believe this makes per-field error display impossible for any caller: the UI
receives one sentence with no indication of which field it belongs to, so a form with three invalid
fields must be corrected and resubmitted three times. The cost is currently masked because every
validator in the layer has exactly one rule chain, but it will surface the first time a command
carries two validated fields.

**Suggested direction.** Carry the failures as structured data on the failed result -- a
property-name-to-messages map alongside the summary string -- so the single-message path stays
available for simple callers while richer callers can bind per field.

### CORE-009 -- Ten of the fourteen commands have no validator, making ValidationBehavior a no-op for them

```yaml
id: CORE-009
layer: CORE
severity: S3
category: structure
authority: conventions
citation: backend/backend-conventions.md#2. CQRS Patterns
locations:
  - src/DungeonMaster.Application/DependencyInjection.cs:45-49
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddFloor/AddFloorCommand.cs:14-17
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommand.cs:28-35
  - src/DungeonMaster.Application/Features/Dungeons/Commands/DeleteDungeon/DeleteDungeonCommand.cs:11-13
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveArea/RemoveAreaCommand.cs:17-21
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveFloor/RemoveFloorCommand.cs:14-17
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemovePlacedObject/RemovePlacedObjectCommand.cs:17-21
  - src/DungeonMaster.Application/Features/Dungeons/Commands/ReorderFloor/ReorderFloorCommand.cs:17-21
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateAreaOutline/UpdateAreaOutlineCommand.cs:21-26
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateFloor/UpdateFloorCommand.cs:23-29
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdatePlacedObject/UpdatePlacedObjectCommand.cs:28-35
effort: M
risk: low
independent: true
```

**What.** `DependencyInjection.cs` registers four validators -- for `CreateDungeon`, `UpdateDungeon`,
`AddArea` and `UpdateArea`. The other ten commands dispatch through `ValidationBehavior` with an empty
`IEnumerable<IValidator<TCommand>>`, so the behavior resolves, allocates a `ValidationContext`, finds
zero failures, and calls `next()`.

**Why it matters.** `backend-conventions.md` section 2 states that a command "has a corresponding
validator that runs before the handler". The gaps are not cosmetic: `ReorderFloorCommand.NewNumber` is
an unbounded `int` whose out-of-range values are then silently swallowed by the aggregate (CORE-001),
`AddPlacedObjectCommand.Scale` is an unbounded `float` whose only guard is an
`ArgumentOutOfRangeException` thrown deep inside `PlacedObject`, and `UpdateAreaOutlineCommand.Outline`
is an unchecked `IReadOnlyList` that may be empty or null.

**Suggested direction.** Add validators for the commands that carry real input -- at minimum the
numeric ranges on `ReorderFloor`, `AddPlacedObject` and `UpdatePlacedObject`, and non-empty checks on
`UpdateAreaOutline` -- and consider registering validators by assembly scan so a new command cannot be
added without one being noticed.

### CORE-010 -- Dispatcher is a service locator over a hand-maintained handler registration list

```yaml
id: CORE-010
layer: CORE
severity: S3
category: structure
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
locations:
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:9-11
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:18-34
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:59-64
  - src/DungeonMaster.Application/DependencyInjection.cs:51-101
effort: M
risk: medium
independent: true
```

**What.** `Dispatcher` takes `IServiceProvider` and calls `GetRequiredService(handlerType)` /
`GetServices(behaviorType)` on a `Type` built at run time via `MakeGenericType`. The sixteen
handler registrations it depends on are written out by hand in `DependencyInjection.cs`, each one
repeating the closed generic pair.

**Why it matters.** The Microsoft DI guidelines list "Avoid using the *service locator pattern*" among
their recommendations. The concrete cost here is that the dispatcher's dependencies are invisible: a
command whose handler registration is forgotten compiles cleanly, passes every unit test that
constructs the handler directly, and throws `InvalidOperationException` only when a user triggers that
one screen. The hand-maintained list makes the omission easy -- adding a command means editing three
files in the right order.

**Suggested direction.** Register handlers by scanning the Application assembly for closed
`ICommandHandler<,>` / `IQueryHandler<,>` implementations, and add a startup self-check that asserts
every `ICommand<>` in the assembly has exactly one registered handler. That keeps the locator inside
`Dispatcher` -- which is unavoidable for a runtime-dispatched design -- while removing the class of bug
it enables.

### CORE-011 -- Every dispatch performs reflection and dynamic binding with no caching of the closed handler type

```yaml
id: CORE-011
layer: CORE
severity: S3
category: performance
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:18-28
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:36-49
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:59-68
effort: M
risk: medium
independent: false
blocked-by:
  - CORE-010
```

**What.** `SendAsync` calls `MakeGenericType` twice per dispatch, resolves the handler into a
`dynamic` local, casts the command to `dynamic` at each invocation, materialises the behavior list with
`GetServices(...).Cast<dynamic>().Reverse().ToList()`, and allocates one closure per behavior. Nothing
is memoised across calls. `QueryAsync` repeats the pattern without the behavior loop.

**Why it matters.** I believe this is the main cost the hand-rolled dispatcher is paying, and it lands
on a path that the map editor drives interactively -- `UpdatePlacedObjectCommand` is dispatched while
the user drags an object. Each dispatch pays reflection, a DLR call-site setup on first use per
call-site shape, and several allocations that a compiled generic path would not need. I also believe
`dynamic` is a portability risk worth verifying for the `net10.0-maccatalyst` target, since the Native
AOT and trimming documentation both treat runtime code generation as unsupported; I could not confirm
from an official page that the C# runtime binder is affected in this specific configuration, so I state
it as a concern rather than a fact.

**Suggested direction.** Cache the closed handler and behavior types in a
`ConcurrentDictionary<Type, ...>` keyed by the command's runtime type, and store a compiled delegate
rather than re-binding through `dynamic` on every call. If AOT compatibility turns out to matter,
a source-generated dispatch table removes both reflection and `dynamic` at once.

### CORE-012 -- Queries bypass the pipeline entirely; IPipelineBehavior applies only to commands

```yaml
id: CORE-012
layer: CORE
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:55-69
  - src/DungeonMaster.Application/Common/Dispatch/IPipelineBehavior.cs:14-16
  - src/DungeonMaster.Application/Common/Dispatch/IQueryHandler.cs:12-14
effort: M
risk: low
independent: true
```

**What.** `IPipelineBehavior<TCommand, TResult>` is constrained `where TCommand : ICommand<TResult>`,
and `QueryAsync` resolves the query handler and awaits it directly with no behavior lookup. There is no
query-side equivalent of the behavior interface.

**Why it matters.** The composition itself is correct -- I checked the ordering, and reversing the
behavior list before folding it into the pipeline does produce first-registered-outermost, with the
per-iteration `next` capture handled properly. The structural gap is elsewhere: any cross-cutting
concern that should cover reads as well as writes (logging, timing, caching, tenant filtering) has
nowhere to attach, so the first such requirement will either be duplicated into every query handler or
force a second pipeline to be built.

**Suggested direction.** I believe the cheapest correction is to generalise the behavior interface over
a request abstraction shared by `ICommand<T>` and `IQuery<T>`, keeping the `Result<T>` wrapper on the
command path only. That is a larger change than it looks, so it is worth deferring until a concern
actually needs to span both sides.

### CORE-013 -- Fourteen command handlers repeat one load / null-check / mutate / save skeleton

```yaml
id: CORE-013
layer: CORE
severity: S3
category: duplication
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Duplicated Code"
locations:
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommandHandler.cs:16-41
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddFloor/AddFloorCommandHandler.cs:16-38
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommandHandler.cs:16-50
  - src/DungeonMaster.Application/Features/Dungeons/Commands/CreateDungeon/CreateDungeonCommandHandler.cs:16-29
  - src/DungeonMaster.Application/Features/Dungeons/Commands/DeleteDungeon/DeleteDungeonCommandHandler.cs:15-24
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveArea/RemoveAreaCommandHandler.cs:16-40
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveFloor/RemoveFloorCommandHandler.cs:16-38
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemovePlacedObject/RemovePlacedObjectCommandHandler.cs:17-41
  - src/DungeonMaster.Application/Features/Dungeons/Commands/ReorderFloor/ReorderFloorCommandHandler.cs:16-40
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateArea/UpdateAreaCommandHandler.cs:16-43
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateAreaOutline/UpdateAreaOutlineCommandHandler.cs:15-40
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateDungeon/UpdateDungeonCommandHandler.cs:16-41
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateFloor/UpdateFloorCommandHandler.cs:16-42
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdatePlacedObject/UpdatePlacedObjectCommandHandler.cs:17-44
effort: M
risk: medium
independent: false
blocked-by:
  - CORE-001
```

**What.** Twelve of the fourteen handlers repeat the same skeleton, varying only in the aggregate call
in the middle: `LoadAsync(id, ct)`, `if (dungeon is null) return Failure($"Dungeon {id} not found.",
NotFound)`, one mutator call, `UpdateAsync(dungeon, ct)`, `Success(...)`. The not-found message string
is written out twelve times. Eleven of the twelve are identical outside the mutator call;
`AddPlacedObjectCommandHandler` adds one further guard on the returned object
(`AddPlacedObjectCommandHandler.cs:38-43`). Only `CreateDungeon` and `DeleteDungeon` differ in shape.

**Why it matters.** This is the "Duplicated Code" smell at its most literal: the load-and-guard
prologue is a policy, and it currently has twelve copies. Any change to it -- adding concurrency
handling, a domain-event dispatch after `UpdateAsync`, a transaction scope, or the outcome-aware
handling that CORE-001 requires -- has to be applied twelve times with no compiler assistance if one is
missed.

**Suggested direction.** Introduce a small abstract base or a helper (for example
`writeRepository.MutateAsync(id, dungeon => ..., ct)`) that owns load, guard, mutate and save, leaving
each handler with just its aggregate call. Land it together with CORE-001 so the shared prologue is
written once against the new outcome-returning mutators rather than twice.

### CORE-014 -- AddPlacedObject reports a missing floor as a Validation failure with an opaque message

```yaml
id: CORE-014
layer: CORE
severity: S3
category: api-design
authority: conventions
citation: backend/backend-conventions.md#2. CQRS Patterns
locations:
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommandHandler.cs:6-10
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommandHandler.cs:38-43
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:363-369
effort: XS
risk: low
independent: false
blocked-by:
  - CORE-001
```

**What.** `Dungeon.AddPlacedObject` returns `null` when the floor id does not match, and the handler
turns that into `Result<Guid>.Failure("Failed to add placed object.", ErrorCategory.Validation)`. The
handler's own XML summary documents this as "a validation failure if the floor is not found". The
message names neither the floor nor the reason.

**Why it matters.** `backend-conventions.md` section 2 gives the error category a routing job -- the
calling layer maps each category onto an outcome. A not-found condition labelled `Validation` routes to
the wrong outcome and tells the user their input was malformed when in fact a referenced entity is
gone. The message carries no id, so a support report built from it is not actionable.

**Suggested direction.** Return `ErrorCategory.NotFound` with `$"Floor {command.FloorId} not found."`,
matching the dungeon-level message already used in the same method, and correct the class summary.

### CORE-015 -- Dungeon is a 453-line delegation facade that re-implements Floor's API rather than exposing it

```yaml
id: CORE-015
layer: CORE
severity: S3
category: structure
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Middle Man"
locations:
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:155-171
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:191-204
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:215-229
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:249-271
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:286-303
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:356-380
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:392-408
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:429-452
effort: L
risk: high
independent: true
```

**What.** `DungeonMethods.cs` is the largest file in the Domain at 453 lines. Eight of its eleven
methods consist of the same three steps: `FirstOrDefault` the floor by id, return or throw if absent,
forward to the identically named `Floor` method, then `Touch()`. The forwarded methods
(`UpdateArea`, `UpdateAreaOutline`, `RemoveArea`, `AddArea`, `AddObject`, `RemoveObject`,
`UpdateObject`) already exist on `Floor` with the same parameter lists minus `floorId`.

**Why it matters.** This is the Middle Man smell: the aggregate root's public surface is mostly
pass-through, and every new `Floor` or `Area` behaviour must be mirrored upward with a hand-written
lookup-and-forward wrapper. The XML documentation is mirrored too, which is why a large share of the
file is duplicated prose. Aggregate-root gatekeeping is a legitimate reason to route through
`Dungeon`, but the current form pays the full cost of the indirection without concentrating any
invariant in it -- the wrappers add only a `Touch()` and a null guard.

**Suggested direction.** Extract the repeated `floorId` lookup into a single private
`RequireFloor(Guid floorId)` helper so each wrapper collapses to two lines, or expose a narrow
floor-scoped accessor from the root and let callers work against `Floor` directly for behaviour that
carries no cross-floor invariant. High risk because this is the aggregate's whole public surface.

### CORE-016 -- The Domain has no policy layer and scale validation is inline and duplicated

```yaml
id: CORE-016
layer: CORE
severity: S3
category: structure
authority: conventions
citation: backend/backend-conventions.md#5. Policy Conventions
locations:
  - src/DungeonMaster.Domain/Entities/Floor/ObjectScale.cs:3-12
  - src/DungeonMaster.Domain/Entities/Floor/PlacedObject.cs:28-34
  - src/DungeonMaster.Domain/Entities/Floor/PlacedObject.cs:80-86
  - src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:9
  - src/DungeonMaster.Domain/ValueObjects/TilePoint.cs:12
  - src/DungeonMaster.Domain/ValueObjects/PathSegment.cs:18-22
effort: M
risk: low
independent: true
```

**What.** There is no `Policies/` directory. The one non-trivial domain rule -- object scale must be a
finite value in `[Min, Max]` -- is written twice, verbatim, as an inline `if` in the `PlacedObject`
constructor and again in `PlacedObject.Update`, each throwing `ArgumentOutOfRangeException` with the
same interpolated message. `ObjectScale` holds the bounds as a static class of `const float`s plus two
clamping helpers. The three value objects under `ValueObjects/` are plain positional records that
accept any input, including `float.NaN` for `CanvasPoint`.

**Why it matters.** `backend-conventions.md` section 5 requires a policy class for each domain concept
with non-trivial validation, returning a structured result and never throwing, with the value object
delegating to it. Scale is exactly that concept and has neither a policy nor a value object. The
duplication is already load-bearing: the two copies must be kept in step, and because
`AddPlacedObjectCommand` has no validator (CORE-009) an out-of-range scale reaches the entity and
surfaces as an exception rather than a validation result.

**Suggested direction.** Introduce an `ObjectScale` value object with an `ObjectScalePolicy` beside it,
have `PlacedObject` store the value object, and let the command validators call the policy directly to
produce field-level errors before construction. `PathSegment`'s `Ctrl1`/`Ctrl2` nullability rule
(control points only meaningful for `CubicBezier`) is a second candidate for the same treatment.

### CORE-017 -- ObjectKind has no zero value, so default(ObjectKind) silently means DoorWood

```yaml
id: CORE-017
layer: CORE
severity: S3
category: api-design
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum
locations:
  - src/DungeonMaster.Domain/Entities/Floor/ObjectKind.cs:3-51
effort: S
risk: medium
independent: true
```

**What.** `ObjectKind` starts at `DoorWood` with no `None` member, so the enum's zero value is a
concrete door. `ObjectRotation` and `SnapAnchor`, by contrast, do start at a neutral value
(`None`, `Center`).

**Why it matters.** The Microsoft enum design guideline is explicit: "DO provide a value of zero on
simple enums. Consider calling the value something like 'None.' If such a value is not appropriate for
this particular enum, the most common default value for the enum should be assigned the underlying
value of zero." Neither branch is satisfied here -- `DoorWood` is not a meaningful default and not the
most common kind. A default-constructed `AddPlacedObjectCommand`, a column that fails to bind, or a
deserialisation gap therefore produces a wooden door rather than an obvious error.

**Suggested direction.** Add `None = 0` as the first member and reject it at the boundary
(validator plus entity constructor). Note that the underlying numeric values of every existing member
shift by one, so any persisted integer column has to be migrated -- hence medium risk; T2 owns the
persistence side.

### CORE-018 -- Entity provides no identity-based equality, so two instances of the same entity compare unequal

```yaml
id: CORE-018
layer: CORE
severity: S3
category: api-design
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Domain/Entities/Entity.cs:6-27
effort: S
risk: medium
independent: true
```

**What.** `Entity` exposes `Id`, `CreatedAt` and `LastModified` and a protected `Touch()`, but does not
override `Equals`, `GetHashCode` or `==`. All four entities (`Dungeon`, `Floor`, `Area`,
`PlacedObject`) therefore use reference equality, while the three value objects are `record`s with
structural equality.

**Why it matters.** I believe this is a latent trap rather than a present defect: the aggregate's
internal lookups all compare `entity.Id == someGuid` explicitly, so nothing misbehaves today. But the
model presents two entity-shaped things -- a re-loaded `Dungeon` and an in-memory one with the same
`Id` -- as different objects, which makes `Contains`, `Distinct`, `Except` and dictionary keys behave
in a way most readers of a DDD-shaped domain would not expect. The asymmetry with the value-object
records makes the surprise more likely, not less.

**Suggested direction.** Implement identity equality on `Entity` (equal when the runtime types match
and `Id` is equal), with `GetHashCode` over `Id`. Medium risk because any existing code that relies on
reference semantics for entities would change behaviour silently.

### CORE-019 -- Five Domain overloads have no production caller and are reachable only from the test assembly

```yaml
id: CORE-019
layer: CORE
severity: S3
category: dead-code
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Dead Code"
locations:
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:323-333
  - src/DungeonMaster.Domain/Entities/Floor/FloorObjectMethods.cs:10-14
  - src/DungeonMaster.Domain/Entities/Floor/FloorObjectMethods.cs:96-113
  - src/DungeonMaster.Domain/Entities/Floor/PlacedObject.cs:13-18
  - src/DungeonMaster.Domain/Entities/Floor/PlacedObject.cs:62-63
  - src/DungeonMaster.Domain/AssemblyInfo.cs:3
effort: S
risk: low
independent: true
```

**What.** A repository-wide search finds no reachable production entry point for the scale-less
overloads: `Dungeon.AddPlacedObject(floorId, kind, position, rotation)`, `Floor.AddObject(kind,
position, rotation)`, `Floor.UpdateObject(objectId, newPosition, newRotation)`,
`PlacedObject.Update(newPosition, newRotation)` and the four-argument `PlacedObject` constructor. Four
of the five have no caller at all outside `DungeonMaster.Tests`. The fifth,
`PlacedObject.Update(newPosition, newRotation)`, does have one production caller --
`Floor.UpdateObject(objectId, newPosition, newRotation)` at `FloorObjectMethods.cs:109` -- but that
caller is itself one of the dead overloads, so the cluster is dead as a whole rather than individually.
Every Application-layer call site passes an explicit scale. The remaining callers are in
`DungeonMaster.Tests`, reachable because `AssemblyInfo.cs` grants
`InternalsVisibleTo("DungeonMaster.Tests")`.

**Why it matters.** These overloads are kept alive by their own tests, which is the classic way dead
code survives a coverage check. Each one is a second entry point into the scale invariant discussed in
CORE-016, so any change to scale handling has to be made in twice as many places as the production
paths require, and a reader cannot tell from the code which overload is the intended one.

**Suggested direction.** Delete the scale-less overloads and update the tests to pass
`ObjectScale.Default` explicitly -- the tests then document the default rather than depending on an
otherwise unused API. If the convenience overloads are wanted for readability, keep exactly one level
of defaulting (an optional parameter) rather than a parallel overload chain at three types.

### CORE-020 -- Common/Map holds three geometry types that no command, query or handler in the layer uses

```yaml
id: CORE-020
layer: CORE
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Map/MapDrawingSession.cs:30-38
  - src/DungeonMaster.Application/Common/Map/SnapZoneCalculator.cs:8-33
  - src/DungeonMaster.Application/Common/Map/TileCoordinateHelper.cs:9-52
effort: M
risk: low
independent: true
```

**What.** `MapDrawingSession`, `SnapZoneCalculator` and `TileCoordinateHelper` have no caller inside
`DungeonMaster.Application`. Their production callers are all in `DungeonMaster.App`
(`ViewModels/DungeonDetailViewModel.cs`, `Graphics/FloorMapDrawable.cs`,
`Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`), which reach past the dispatcher and call them
directly. `MapDrawingSession` is stateful: the view model holds one as a mutable property.

**Why it matters.** I believe these are presentation-interaction concerns -- pixel-to-tile snapping and
in-progress drawing state -- that have been placed in the Application layer because they are
"not UI framework code", rather than because the application layer needs them. The consequence is that
the Application assembly's public surface is not the command/query contract it otherwise presents:
part of it is a utility library that the App layer consumes as a library. It also means a stateful,
non-thread-safe session object lives in a layer whose other types are all stateless per-dispatch
services.

**Suggested direction.** I believe the cleanest resolution is to move the coordinate helpers into the
App layer next to their only callers, and to decide explicitly whether `MapDrawingSession` is
application state (in which case a command should consume it) or view state (in which case it belongs
to the view model). The layer boundary itself is T6's record; this record covers only the fact that the
Application layer exposes types it does not use.

### CORE-021 -- Floor.UpdateNumber is the only entity mutator that does not call Touch

```yaml
id: CORE-021
layer: CORE
severity: S2
category: structure
authority: conventions
citation: backend/backend-conventions.md#3. Entity Conventions
locations:
  - src/DungeonMaster.Domain/Entities/Floor/FloorMethods.cs:13-14
  - src/DungeonMaster.Domain/Entities/Area/AreaMethods.cs:19-29
  - src/DungeonMaster.Domain/Entities/Entity.cs:23-27
effort: XS
risk: low
independent: true
```

**What.** `internal void UpdateNumber(int number) => Number = number;` writes entity state without
calling `Touch()`. Every other mutator in the Domain -- `Area.Update`, `Area.SetOutline`,
`Floor.UpdateDetails`, `Floor.AddArea`, `Floor.RemoveArea`, `Floor.UpdateAreaOutline`, the three
`Floor` object methods, `PlacedObject.Update` and all of `Dungeon`'s -- does.

**Why it matters.** `backend-conventions.md` section 3 states that a mutable entity records a
last-modified timestamp updated whenever state changes, and this is the one place that rule is not
followed. There is no user-visible consequence today because `FloorDto` projects no timestamps, so the
floor's stale `LastModified` is never displayed. The inconsistency is the cost: a reader cannot rely on
"mutator implies `Touch`", and the moment a floor timestamp is surfaced, reordering will appear not to
have modified the floors it renumbered.

**Suggested direction.** Add `Touch()` to `UpdateNumber`, or -- if the omission is deliberate because
renumbering is bookkeeping rather than a user edit -- say so in the XML summary so the exception is
documented rather than inferred.

### CORE-022 -- Floor is split across three partial files where the conventions define a two-file split

```yaml
id: CORE-022
layer: CORE
severity: S4
category: structure
authority: conventions
citation: backend/dotnet-format.md#11. Domain Building Blocks in C#
locations:
  - src/DungeonMaster.Domain/Entities/Floor/Floor.cs:8
  - src/DungeonMaster.Domain/Entities/Floor/FloorMethods.cs:5
  - src/DungeonMaster.Domain/Entities/Floor/FloorObjectMethods.cs:5
  - src/DungeonMaster.Domain/Entities/Area/Area.cs:8
  - src/DungeonMaster.Domain/Entities/Area/AreaMethods.cs:5
  - src/DungeonMaster.Domain/Entities/Dungeon/Dungeon.cs:6
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:5
  - src/DungeonMaster.Domain/Entities/Floor/PlacedObject.cs:8
effort: XS
risk: low
independent: true
```

**What.** The conventions define the entity split as exactly two files: `{Entity}.cs` for properties
and constructors, `{Entity}Methods.cs` for behaviour. `Dungeon` and `Area` follow this. `Floor` adds a
third partial, `FloorObjectMethods.cs`, holding the placed-object behaviour. `PlacedObject` is not
partial at all -- its two behaviour methods sit in `PlacedObject.cs` alongside its properties.

**Why it matters.** `dotnet-format.md` section 11 gives the split as a table of two files and states
its purpose: keeping the data shape readable at a glance while isolating behaviour. The split itself is
working as intended -- `Floor.cs` at 73 lines is genuinely readable next to 305 lines of behaviour, and
`DungeonMethods.cs` proves behaviour does grow independently. The deviation is the file count and the
inconsistency: three entities follow three different arrangements, so a reader has to look before
knowing where a method lives.

**Suggested direction.** Either accept the third file and record it in the conventions as a permitted
extension when behaviour clusters (which `FloorObjectMethods.cs` is a fair example of), or fold it back
into `FloorMethods.cs`. Whichever is chosen, apply it to `PlacedObject` too so all four entities read
the same way.

### CORE-023 -- RotateDirection.cs declares a type named RotationDirection

```yaml
id: CORE-023
layer: CORE
severity: S4
category: naming
authority: conventions
citation: backend/dotnet-format.md#2. File Organization
locations:
  - src/DungeonMaster.Domain/Entities/Floor/RotateDirection.cs:1-3
effort: XS
risk: low
independent: true
```

**What.** The file `RotateDirection.cs` contains `public enum RotationDirection { Clockwise,
CounterClockwise }`. The sibling `ScaleDirection.cs` declares `ScaleDirection`, matching.

**Why it matters.** `dotnet-format.md` section 2 requires that the file name match the type name
exactly. A grep or a "go to file" for `RotationDirection` misses this file, and the mismatch is the
kind that survives for years because nothing enforces it.

**Suggested direction.** Rename the file to `RotationDirection.cs`. Both callers are in
`DungeonMaster.App`, which references the type and not the file, so nothing else changes.

### CORE-024 -- Abbreviated identifiers ct, obj and CW90 are used throughout both layers

```yaml
id: CORE-024
layer: CORE
severity: S4
category: naming
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions
locations:
  - src/DungeonMaster.Application/Common/Dispatch/IDispatcher.cs:25-27
  - src/DungeonMaster.Application/Common/Dispatch/ICommandHandler.cs:29-31
  - src/DungeonMaster.Application/Common/Dispatch/IQueryHandler.cs:27-29
  - src/DungeonMaster.Application/Common/Behaviors/ValidationBehavior.cs:26
  - src/DungeonMaster.Application/Features/Dungeons/IDungeonWriteRepository.cs:22-24
  - src/DungeonMaster.Application/Features/Dungeons/IDungeonReadRepository.cs:20
  - src/DungeonMaster.Domain/Entities/Floor/FloorObjectMethods.cs:40
  - src/DungeonMaster.Domain/Entities/Floor/FloorObjectMethods.cs:65-73
  - src/DungeonMaster.Domain/Entities/Floor/ObjectRotation.cs:5-8
effort: S
risk: low
independent: true
```

**What.** The cancellation-token parameter is named `ct` on every interface, handler and repository
method in the Application layer. `FloorObjectMethods` names its local `obj`. `ObjectRotation`'s members
are `None`, `CW90`, `CW180`, `CW270`.

**Why it matters.** The Microsoft general naming conventions state "DO NOT use abbreviations or
contractions as part of identifier names" and "DO favor readability over brevity". `dotnet-format.md`
reinforces this for this specific parameter: every handler signature in sections 3, 9 and 10 spells it
`cancellationToken`, and the naming table's parameter example is `cancellationToken`. `ct` is the most
visible instance because it appears in every public signature the layer exposes.

**Suggested direction.** Rename `ct` to `cancellationToken` across the Dispatch interfaces, behaviors,
handlers and repository interfaces; rename `obj` to `placedObject` (which the surrounding lambdas
already use); and consider `Clockwise90` / `Clockwise180` / `Clockwise270` for the rotation members,
noting that persisted values are unaffected because only the names change.

### CORE-025 -- Six public Domain types carry no XML documentation

```yaml
id: CORE-025
layer: CORE
severity: S4
category: docs
authority: conventions
citation: backend/dotnet-format.md#9. XML Documentation Comments
locations:
  - src/DungeonMaster.Domain/Entities/Floor/ObjectKind.cs:3
  - src/DungeonMaster.Domain/Entities/Floor/ObjectRotation.cs:3
  - src/DungeonMaster.Domain/Entities/Floor/ObjectScale.cs:3-11
  - src/DungeonMaster.Domain/Entities/Floor/SnapAnchor.cs:3
  - src/DungeonMaster.Domain/Entities/Floor/RotateDirection.cs:3
  - src/DungeonMaster.Domain/Entities/Floor/ScaleDirection.cs:3
effort: S
risk: low
independent: true
```

**What.** `ObjectKind` (37 members), `ObjectRotation`, `SnapAnchor`, `RotationDirection` and
`ScaleDirection` have no `<summary>` on the type or on any member. `ObjectScale` documents neither the
class nor its four constants nor its two methods. Everything else in both layers is documented, often
thoroughly -- `PathSegmentKind` documents both of its members.

**Why it matters.** `dotnet-format.md` section 9 states that all public types, members and parameters
must have XML documentation comments. The gap is concentrated in exactly the types where a reader most
needs prose: `ObjectKind`'s semantics are carried entirely by section comments drawn with box-drawing
characters, `SnapAnchor`'s members do not explain what a "North" anchor snaps to, and `ObjectScale`'s
`StepUp`/`StepDown` clamping behaviour is discoverable only by reading the expression body.

**Suggested direction.** Add type-level summaries to all six, member summaries to `SnapAnchor`,
`ObjectRotation`, `RotationDirection` and `ScaleDirection`, and constant/method summaries to
`ObjectScale`. `ObjectKind`'s 37 members can reasonably keep their grouping comments with a type-level
summary that points at the grouping.

### CORE-026 -- Two files indent XML documentation continuation lines with spaces instead of two tabs

```yaml
id: CORE-026
layer: CORE
severity: S4
category: docs
authority: conventions
citation: backend/dotnet-format.md#9. XML Documentation Comments
locations:
  - src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:3-8
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/PlacedObjectDto.cs:6-15
  - src/DungeonMaster.Domain/Entities/Floor/PlacedObject.cs:12
  - src/DungeonMaster.Domain/Entities/Floor/PlacedObject.cs:43-59
effort: XS
risk: low
independent: true
```

**What.** `CanvasPoint.cs` and `PlacedObjectDto.cs` write `///     Represents...` with five spaces
where every other file writes `///` followed by two tabs. Both files also put `<param>` descriptions on
the same line as the tag (`/// <param name="X">Horizontal pixel offset from the area origin.</param>`),
as does `PlacedObject.cs` for several of its summaries.

**Why it matters.** `dotnet-format.md` section 9 requires that tag descriptions go on a new line
indented with two tabs from the `///` marker. With no `.editorconfig` in the repository -- T6 owns that
record -- nothing normalises this, so the two styles will keep interleaving as files are edited in
different editors.

**Suggested direction.** Re-indent the affected comment blocks to the two-tab form and expand the
single-line `<summary>` and `<param>` tags in `PlacedObject.cs` and `PlacedObjectDto.cs` to the
multi-line shape the rest of both layers uses.

### CORE-027 -- Five files indent with four spaces and two column-align assignments where the rest use tabs

```yaml
id: CORE-027
layer: CORE
severity: S4
category: style
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Domain/Entities/Floor/ObjectKind.cs:5-50
  - src/DungeonMaster.Domain/Entities/Floor/ObjectRotation.cs:5-8
  - src/DungeonMaster.Domain/Entities/Floor/ObjectScale.cs:5-11
  - src/DungeonMaster.Domain/Entities/Floor/SnapAnchor.cs:5-9
  - src/DungeonMaster.Domain/Entities/Floor/PlacedObject.cs:36-40
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/PlacedObjectDto.cs:17-21
effort: XS
risk: low
independent: true
```

**What.** Sixty-five of the seventy-seven files in scope indent with tabs and seven contain no indented
line at all. The remaining five -- `ObjectKind.cs`, `ObjectRotation.cs`, `ObjectScale.cs`,
`SnapAnchor.cs` and `PlacedObjectDto.cs` -- indent with four spaces. Two files additionally pad
assignments into columns: `public const float Min     = 0.25f;` in `ObjectScale.cs`, and
`FloorId  = floorId; Kind     = kind;` in `PlacedObject.cs`, which is tab-indented and is cited above
for the alignment alone.

**Why it matters.** `dotnet-format.md` does not state an indent-character rule, so I am not asserting a
violation -- I believe the dominant convention in this repository is tabs, and these five files depart
from it. The practical cost is diff noise: a single-character edit inside a column-aligned block
re-aligns the whole block, and mixed indentation makes whitespace-only churn hard to distinguish from
real change in review.

**Suggested direction.** Normalise the five files to tabs and drop the column alignment in both. This
is one of the things an `.editorconfig` would settle permanently; that file's absence is T6's record.

### CORE-028 -- Twelve handlers carry a redundant using for their own enclosing namespace, and four do not

```yaml
id: CORE-028
layer: CORE
severity: S4
category: style
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddFloor/AddFloorCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/CreateDungeon/CreateDungeonCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/DeleteDungeon/DeleteDungeonCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveArea/RemoveAreaCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveFloor/RemoveFloorCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/ReorderFloor/ReorderFloorCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateArea/UpdateAreaCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateDungeon/UpdateDungeonCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateFloor/UpdateFloorCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/GetDungeonDetailQueryHandler.cs:2
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeons/GetDungeonsQueryHandler.cs:2
effort: XS
risk: low
independent: true
```

**What.** Twelve handlers declare `using DungeonMaster.Application.Features.Dungeons;` even though
their own file-scoped namespace is nested beneath it, so the repository interfaces are already in
scope. The four object-related handlers (`AddPlacedObject`, `RemovePlacedObject`, `UpdatePlacedObject`,
`UpdateAreaOutline`) omit it and compile identically.

**Why it matters.** I believe this is dead syntax rather than a defect -- the build is clean, and no
analyzer is configured to flag it (T6 owns the analyzer record). The cost is that the two conventions
sit side by side in the same folder tree, so a reader adding a handler has no way to tell which form is
intended, and the redundant line implies a dependency that does not exist.

**Suggested direction.** Remove the twelve redundant directives so all sixteen handlers read the same
way.

### CORE-029 -- Repository interfaces sit under Features/Dungeons rather than the conventions' Common/Interfaces

```yaml
id: CORE-029
layer: CORE
severity: S4
category: structure
authority: conventions
citation: backend/backend-conventions.md#1. Project Structure
locations:
  - src/DungeonMaster.Application/Features/Dungeons/IDungeonReadRepository.cs:9
  - src/DungeonMaster.Application/Features/Dungeons/IDungeonWriteRepository.cs:8
effort: S
risk: low
independent: true
```

**What.** `IDungeonReadRepository` and `IDungeonWriteRepository` live at the root of
`Features/Dungeons/`. The conventions' Application layout places application service interfaces in
`Common/Interfaces/` and shows `Features/{Feature}/` containing only `Commands/`, `Queries/` and
`DTOs/`. There is no `Common/Interfaces/` directory in the project.

**Why it matters.** `backend-conventions.md` section 1 gives the layer layout explicitly, and this is a
departure from it. The cost is small and mostly navigational, but it is the kind of drift that makes
the documented structure stop being a reliable map of the code.

**Suggested direction.** Either move the two interfaces to `Common/Interfaces/`, or -- since
feature-owned repository contracts are a defensible choice that keeps the read and write models beside
the feature that defines them -- amend the conventions to permit feature-local interfaces and say why.
The deviation being undocumented is the part worth fixing, not necessarily the placement.

### CORE-030 -- Every collection property allocates a fresh ReadOnlyCollection wrapper on each access

```yaml
id: CORE-030
layer: CORE
severity: S4
category: performance
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Domain/Entities/Dungeon/Dungeon.cs:48-49
  - src/DungeonMaster.Domain/Entities/Floor/Floor.cs:65-66
  - src/DungeonMaster.Domain/Entities/Floor/Floor.cs:71-72
  - src/DungeonMaster.Domain/Entities/Area/Area.cs:60-61
  - src/DungeonMaster.Application/Common/Map/MapDrawingSession.cs:37-38
effort: XS
risk: low
independent: true
```

**What.** `Floors`, `Areas`, `Objects`, `Outline` and `Vertices` are all expression-bodied properties of
the form `_backing.AsReadOnly()`, so each read constructs a new `ReadOnlyCollection<T>` around the same
list rather than returning a cached wrapper.

**Why it matters.** I believe the allocation is measurable only because of who reads these properties:
the map renderer in the App layer walks floors, areas, outlines and objects on every draw pass, so a
per-frame allocation per collection per entity accumulates in gen-0. The nearest stated rule,
`dotnet-guidelines.md` section 4.2 "Avoid Frequent Large Object Allocations on Hot Paths", does not
reach this: these wrappers are small objects, not Large Object Heap allocations, and the section is
written about request handling rather than rendering. So I state the concern as a belief rather than a
rule violation.

**Suggested direction.** Cache the wrapper in a `readonly` field initialised beside the backing list
(`private readonly ReadOnlyCollection<Floor> _floorsView;`) and return it, or expose the property as
`IReadOnlyList<T>` returning `_floors` directly, which is already read-only through the interface.

### CORE-031 -- The hand-rolled dispatcher is 233 lines replacing a mediator library and earns its keep

```yaml
id: CORE-031
layer: CORE
severity: S5
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:9-69
  - src/DungeonMaster.Application/Common/Dispatch/IDispatcher.cs:8-46
  - src/DungeonMaster.Application/Common/Dispatch/ICommand.cs:9
  - src/DungeonMaster.Application/Common/Dispatch/IQuery.cs:9
effort: L
risk: high
independent: true
```

**What.** The `Common/Dispatch` folder is seven files and 233 lines: two marker interfaces, two handler
interfaces, a behavior interface, the dispatcher facade and its implementation. It provides
command/query separation, one open-generic behavior pipeline, and nothing else -- no notifications, no
streaming, no request pre/post processors, no ordering control beyond registration order.

**Why it matters.** I believe the hand-rolled dispatcher is currently carrying its weight, and it is
worth saying so explicitly rather than treating "no MediatR" as a defect. The interfaces are small and
correct, the behavior composition order is right, the `Result<T>` return type is baked into the command
contract in a way a general-purpose library would not give for free, and there is no licensing or
version-coupling exposure. Its real costs are already filed separately: the service-locator
registration surface (CORE-010), the uncached reflection and `dynamic` (CORE-011), and the missing
query-side pipeline (CORE-012). If any of those three grows -- particularly a second or third behavior
with ordering requirements -- the balance shifts.

**Suggested direction.** Keep it, and treat CORE-010 through CORE-012 as the maintenance price of
keeping it. Revisit only if the pipeline needs ordered behaviors, notifications, or query-side
cross-cutting concerns, at which point a library becomes cheaper than growing this one.

### CORE-032 -- Both query handlers wrap a single passthrough call in an async state machine

```yaml
id: CORE-032
layer: CORE
severity: S5
category: async
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeons/GetDungeonsQueryHandler.cs:14-17
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/GetDungeonDetailQueryHandler.cs:14-19
effort: XS
risk: low
independent: true
```

**What.** Both query handlers are expression-bodied `async` methods whose entire body is
`await readRepository.Get...Async(...)` -- the awaited task is returned unchanged, with no work before
or after the await.

**Why it matters.** I believe returning the task directly would remove the state machine and one
allocation per query with no behavioural difference here, since neither method holds a `using`, a
`try`, or anything else that depends on the continuation running inside the method. I also believe the
current form is defensible: eliding `async` moves exceptions from the await point to the call point and
makes stack traces harder to read, which is why many teams keep the `await` deliberately. This is a
note, not a deviation -- no authority I read prescribes either form.

**Suggested direction.** If the elision is wanted, drop `async`/`await` and return the task; otherwise
leave it and be consistent, since the command handlers all have genuine post-await work and must stay
`async` regardless.

### CORE-033 -- The only persistence-shaped concession in the Domain is the private parameterless constructors

```yaml
id: CORE-033
layer: CORE
severity: S5
category: boundary
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Domain/Entities/Area/Area.cs:12
  - src/DungeonMaster.Domain/Entities/Dungeon/Dungeon.cs:10
  - src/DungeonMaster.Domain/Entities/Floor/Floor.cs:13
  - src/DungeonMaster.Domain/Entities/Floor/PlacedObject.cs:10
  - src/DungeonMaster.Domain/Entities/Area/Area.cs:45
  - src/DungeonMaster.Domain/Entities/Floor/PlacedObject.cs:53
effort: XS
risk: low
independent: true
```

**What.** Each of the four entities declares an unused `private Entity() { }` constructor, and two
non-nullable reference properties are initialised with `= default!` (`Area.Name`,
`PlacedObject.Position`). Nothing else in the 20 Domain files references a framework type beyond the
BCL: no attributes, no `DbContext`, no `virtual` navigation properties, no interfaces borrowed from
Application, and the `.csproj` has zero package and zero project references.

**Why it matters.** I checked domain purity specifically and believe it holds. These six lines exist to
let an ORM materialise the entities, which is an inward-facing accommodation rather than a leak -- the
Domain names nothing about persistence and would compile unchanged if the Infrastructure project were
deleted. I record it so the concession is visible rather than accidental, and because `= default!`
silences the nullable analyser at exactly the two places where a materialisation bug would be hardest
to see. The model is behaviour-rich, not anaemic: the aggregate owns renumbering, outline replacement
and scale bounds rather than exposing setters.

**Suggested direction.** Nothing required. If the concession is ever worth removing, EF Core can
construct via the full constructor for these shapes; a one-line comment on each private constructor
naming its purpose would be cheaper and would stop a future reader deleting them as dead code.

## Layer verdict

Both layers hold their shape. The Domain has no project or package references and, on reading all
twenty files, no conceptual leak either: no framework types, no persistence vocabulary, and a
behaviour-rich model where the aggregate owns floor renumbering, outline replacement and scale bounds
rather than exposing setters. The four private parameterless constructors are the only ORM
accommodation and they name nothing about persistence. The Application layer likewise depends only on
Domain, keeps commands, queries, handlers and validators in the shape the conventions prescribe, and
its thirty-odd triplets are strikingly consistent -- consistent enough that their shared skeleton is
now duplication rather than convention (CORE-013).

The internal seams are where the layers are weakest, and the fault line is the same in both: failure
information is discarded at the seam. `Dungeon`'s mutators return `void` and no-op on an unknown child
id, so a handler cannot tell success from silent nothing and returns `Success(true)` regardless
(CORE-001); one path throws instead (CORE-002); one miscategorises (CORE-014); and `ErrorCategory`,
the type that exists to carry this distinction, has no production consumer at all (CORE-007). Fixing
the aggregate's return types is the single change that unlocks most of the rest.

Three named areas are otherwise sound. The dispatcher composes its pipeline correctly -- reversal and
per-iteration capture both check out -- and I judge the hand-rolled design as earning its keep today
(CORE-031), with its costs filed as CORE-010 through CORE-012. `Result`/`ErrorCategory` is used
consistently and never unwrapped carelessly; its problems are design-level, not discipline-level. The
partial-entity split works: `Floor.cs` at 73 lines stays readable beside 305 lines of behaviour. Its
only issue is that three entities arrange it three ways (CORE-022).

The boundary that does need attention is Application-to-App: `Common/Map` holds three geometry types
with no caller inside the layer (CORE-020). T6 owns the boundary record itself.

## Attestation

- No file under `src/` was created, modified, or deleted by this task: yes
- Every record cites at least one file path with a line number: yes
- Every record names its authority, and every non-`judgment` record carries a real citation: yes
