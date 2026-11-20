# Contract: dispatch, registration and the pipeline

Frozen at Gate 2. **Authored by T6, except §3 which T5 and T6 split (see §3's opening) and §1.5's
minimal additions which T4 makes first.** Consumed by T4 and T7.

Closes: **MASTER-089** (service locator over a hand-maintained list), **MASTER-090** (uncached
reflection and `dynamic`), **MASTER-080** (queries bypass the pipeline), **MASTER-002** and
**MASTER-046** (via §3).

---

## 1. The request abstraction — MASTER-080

CORE-012's suggested direction, verbatim: "generalise the behavior interface over a request
abstraction shared by `ICommand<T>` and `IQuery<T>`, keeping the `Result<T>` wrapper on the command
path only." That is what this section does, and it is what makes a behaviour attachable to a read.

**There are three request paths after this session**, because `contracts/result-and-error-surface.md`
§2 freezes a genuinely non-generic `Result`:

| Path | Marker | Handler | Response |
|---|---|---|---|
| No-payload command (10) | `ICommand` | `ICommandHandler<TCommand>` | `Result` |
| Value command (4) | `ICommand<TResult>` | `ICommandHandler<TCommand, TResult>` | `Result<TResult>` |
| Query (2) | `IQuery<TResult>` | `IQueryHandler<TQuery, TResult>` | `TResult` |

**New file** `Common/Dispatch/IRequest.cs`:

```csharp
namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Marker interface for anything the dispatcher routes to a handler.
/// </summary>
/// <typeparam name="TResponse">
///		The type the handler returns, including any result wrapper.
/// </typeparam>
public interface IRequest<TResponse> { }
```

The existing `Common/Dispatch/ICommand.cs` holds **both** arities of `ICommand`. `dotnet-format.md`
§2 wants the file named for the type, the type is named `ICommand` in both cases, and the filesystem
admits one `ICommand.cs` — so the two arities share it. (The `Result` / `Result<T>` pair took the
other route, `Result.cs` plus `ResultOfT.cs`, because there the generic form is a hundred-line
record rather than a two-line marker. Both deviations are recorded in `spec.md`.)

`Common/Dispatch/ICommand.cs` becomes:

```csharp
/// <summary>
///		Marker interface for commands that produce no value.
/// </summary>
public interface ICommand
	: IRequest<Result> { }

/// <summary>
///		Marker interface for commands that return a result of type <typeparamref name="TResult"/>.
/// </summary>
public interface ICommand<TResult>
	: IRequest<Result<TResult>> { }
```

`Common/Dispatch/IQuery.cs` becomes:

```csharp
public interface IQuery<TResult>
	: IRequest<TResult> { }
```

Note the asymmetry, and that it is deliberate: a command's response is `Result` or `Result<TResult>`,
a query's response is `TResult`. `backend-conventions.md` §2 "Queries" is explicit — a query
"Returns a DTO directly — no result wrapper". This session does **not** wrap queries.

**`ICommand` does not derive from `ICommand<T>` for any `T`, and `ICommand<T>` does not derive from
`ICommand`.** The three marker interfaces are mutually exclusive and §2.4 makes that invariant
testable rather than merely stated.

### 1.1 `IPipelineBehavior` — one arity, not two

`Common/Dispatch/IPipelineBehavior.cs` becomes:

```csharp
public interface IPipelineBehavior<in TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	Task<TResponse> HandleAsync(
		TRequest request,
		Func<Task<TResponse>> next,
		CancellationToken ct);
}
```

**This is the finding that makes the amendment cheaper than it looks: the non-generic command path
needs no new behaviour interface.** Once the behaviour is generalised over `(TRequest, TResponse)`
rather than `(TCommand, TResult)` — which §1 does for MASTER-080 anyway — the no-payload path is
just the closure `IPipelineBehavior<TCommand, Result>`, exactly as the value path is
`IPipelineBehavior<TCommand, Result<TResult>>` and the query path is
`IPipelineBehavior<TQuery, TResult>`. **An implementer who adds a single-arity
`IPipelineBehavior<TRequest>` has misread this contract.** Reviewer check:

```
grep -c 'interface IPipelineBehavior' src/DungeonMaster.Application/Common/Dispatch/IPipelineBehavior.cs   # 1
```

### 1.2 `ICommandHandler` — the one genuinely new interface

`Common/Dispatch/ICommandHandler.cs` holds both arities, for the same file-naming reason as §1:

```csharp
/// <summary>
///		Defines the contract for handling a command of type <typeparamref name="TCommand"/> that
///		produces no value.
/// </summary>
public interface ICommandHandler<in TCommand>
	where TCommand : ICommand
{
	Task<Result> HandleAsync(
		TCommand command,
		CancellationToken ct);
}

/// <summary>
///		Defines the contract for handling a command of type <typeparamref name="TCommand"/> that
///		returns a value of type <typeparamref name="TResult"/>.
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
	where TCommand : ICommand<TResult>
{
	Task<Result<TResult>> HandleAsync(
		TCommand command,
		CancellationToken ct);
}
```

`IQueryHandler<TQuery, TResult>` keeps its existing signature unchanged.

### 1.3 `IDispatcher` — a third entry point

```csharp
public interface IDispatcher
{
	Task<Result> SendAsync(
		ICommand command,
		CancellationToken ct = default);

	Task<Result<TResult>> SendAsync<TResult>(
		ICommand<TResult> command,
		CancellationToken ct = default);

	Task<TResult> QueryAsync<TResult>(
		IQuery<TResult> query,
		CancellationToken ct = default);
}
```

A concrete command implements exactly one marker (§1, §2.4), so a call site writes
`await dispatcher.SendAsync(command)` in both command cases and overload resolution is unambiguous
with no type argument and no cast. That is what keeps T4's App ripple mechanical — see
`contracts/handler-skeleton.md` §6.

### 1.5 Sequencing — what T4 adds before T6 arrives

This is the amendment's sharpest edge and it must not be discovered during implementation. **T4 runs
before T6** (`manifest.json` dependencies), and T4 converts ten commands to the non-generic path.
Those ten handlers, their registrations and their ten App call sites cannot compile unless a
non-generic path already exists. So T4 adds a **minimal, throwaway** one, and T6 replaces it
wholesale.

**T4 adds, in `Common/Dispatch/`, exactly this and nothing else:**

| File | T4 adds | T6 then does |
|---|---|---|
| `ICommand.cs` | `public interface ICommand { }` — **no base interface**, because `IRequest<TResponse>` does not exist yet | adds `: IRequest<Result>` |
| `ICommandHandler.cs` | `ICommandHandler<in TCommand> where TCommand : ICommand`, returning `Task<Result>` | unchanged |
| `IDispatcher.cs` | the `Task<Result> SendAsync(ICommand, CancellationToken ct = default)` overload | unchanged |
| `Dispatcher.cs` | an implementation of that overload **copied from the existing `dynamic` one**, differing only in returning `Task<Result>` | deletes it; §2 replaces the whole file |
| `DependencyInjection.cs` (T4-owned already) | the ten hand-written `AddScoped<ICommandHandler<XCommand>, XCommandHandler>()` lines, replacing the ten `AddScoped<ICommandHandler<XCommand, bool>, ...>` lines they supersede | deletes all of them; §4 replaces them with the scan |

**T4 must not** introduce `IRequest`, must not touch `IQuery.cs`, `IQueryHandler.cs` or
`IPipelineBehavior.cs`, and must not attempt the invoker cache. Its new `Dispatcher` overload is
deliberately as `dynamic` and as uncached as the two beside it — MASTER-090 is T6's entry, and a
half-done cache in T4 would collide with T6's rewrite. A T4 diff that improves the dispatcher is a
blocking finding, not a bonus.

**Consequence for T6's before/after counts.** T6's `grep -c 'AddScoped<'` still reads **21** on the
tree T6 starts from, because T4 replaced ten registrations rather than adding any. Measured on the
parent commit: 21. T6's own criterion says at most 1 after.

### 1.4 Queries get a pipeline but not a validator

`architecture-decisions.md` ADR-001 says "MediatR pipeline behaviors (validation, logging) apply
uniformly to all commands and queries". `backend-conventions.md` §2 says a query "Has no validator
(input is validated at the HTTP layer via model binding)". These are reconcilable and this contract
reconciles them explicitly, so neither can be cited against the implementation:

> **Queries run through the behaviour pipeline. No validation behaviour is registered against a
> query.** The seam exists so that logging, timing and caching have somewhere to attach; validation
> stays on the command path where the result wrapper exists to carry a rejection.

§4.2 enforces this in the registration itself, not by convention.

---

## 2. `Dispatcher` — no `dynamic`, one reflection per request type, one cache

**Rewritten file** `Common/Dispatch/Dispatcher.cs`. The defect: `MakeGenericType` twice per
dispatch, `GetRequiredService(Type)`, `Cast<dynamic>()`, `(dynamic)command` at every invocation, no
memoisation. `UpdatePlacedObjectCommand` is dispatched on every frame of an object drag.

The frozen design is a **type-erasing invoker cached per concrete request type**. Reflection runs
once per request type for the life of the process; every dispatch after the first is statically
typed calls through a virtual method.

### 2.1 One cache for all three paths — the frozen answer

```csharp
private static readonly ConcurrentDictionary<Type, object> Invokers = new();
```

**There is exactly one dictionary, keyed on the concrete request type, storing `object`.** It is not
three dictionaries and not a composite key. This is sound because of a property that §1 already
guarantees and §4.3 tests: **a concrete request class implements exactly one of `ICommand`,
`ICommand<T>` and `IQuery<T>`**, so the concrete type alone determines which invoker family its
entry belongs to. Each entry point casts the retrieved `object` to the abstract invoker its own path
expects, and that cast can never be wrong, because no other path can ever have created an entry
under that key.

The invariant is load-bearing, so it is enforced rather than assumed: §4.1's self-check fails, and
names the offending type, if any request type implements more than one marker. If someone later
writes `record X : ICommand, IQuery<int>`, the test suite says so before the cast can throw at
runtime.

**Do not "fix" this by keying on a `(requestType, responseType)` tuple.** The response type is
derivable from the request type, so it buys nothing but an allocation on the hot path this entry
exists to make cheap.

### 2.2 The invoker families

Three abstract bases, three sealed closed implementations. The names are frozen, because the obvious
naming collides: an abstract `CommandInvoker<TResult>` and a closed `CommandInvoker<TCommand>` are
the same arity and cannot coexist.

| Path | Abstract base | Closed implementation |
|---|---|---|
| No-payload command | `CommandInvoker` | `TypedCommandInvoker<TCommand>` |
| Value command | `CommandInvoker<TResult>` | `TypedCommandInvoker<TCommand, TResult>` |
| Query | `QueryInvoker<TResult>` | `TypedQueryInvoker<TQuery, TResult>` |

All six are `private` nested types of `Dispatcher`.

```csharp
public sealed class Dispatcher(
	IServiceProvider serviceProvider)
		: IDispatcher
{
	private static readonly ConcurrentDictionary<Type, object> Invokers = new();

	/// <inheritdoc/>
	public Task<Result> SendAsync(
		ICommand command,
		CancellationToken ct = default)
	{
		var invoker = (CommandInvoker)Invokers.GetOrAdd(
			command.GetType(),
			static commandType =>
				Activator.CreateInstance(
					typeof(TypedCommandInvoker<>).MakeGenericType(commandType))!);

		return invoker.InvokeAsync(
			serviceProvider,
			command,
			ct);
	}

	/// <inheritdoc/>
	public Task<Result<TResult>> SendAsync<TResult>(
		ICommand<TResult> command,
		CancellationToken ct = default)
	{
		var invoker = (CommandInvoker<TResult>)Invokers.GetOrAdd(
			command.GetType(),
			static commandType =>
				Activator.CreateInstance(
					typeof(TypedCommandInvoker<,>).MakeGenericType(
						commandType,
						typeof(TResult)))!);

		return invoker.InvokeAsync(
			serviceProvider,
			command,
			ct);
	}

	// QueryAsync is the same shape over QueryInvoker<TResult> / TypedQueryInvoker<,>.

	private abstract class CommandInvoker
	{
		public abstract Task<Result> InvokeAsync(
			IServiceProvider serviceProvider,
			ICommand command,
			CancellationToken ct);
	}

	private sealed class TypedCommandInvoker<TCommand>
		: CommandInvoker
			where TCommand : ICommand
	{
		public override Task<Result> InvokeAsync(
			IServiceProvider serviceProvider,
			ICommand command,
			CancellationToken ct)
		{
			var typed = (TCommand)command;

			var handler = serviceProvider
				.GetRequiredService<ICommandHandler<TCommand>>();

			return BuildPipeline<TCommand, Result>(
				serviceProvider,
				typed,
				() =>
					handler.HandleAsync(
						typed,
						ct),
				ct)();
		}
	}

	private abstract class CommandInvoker<TResult>
	{
		public abstract Task<Result<TResult>> InvokeAsync(
			IServiceProvider serviceProvider,
			ICommand<TResult> command,
			CancellationToken ct);
	}

	private sealed class TypedCommandInvoker<TCommand, TResult>
		: CommandInvoker<TResult>
			where TCommand : ICommand<TResult>
	{
		public override Task<Result<TResult>> InvokeAsync(
			IServiceProvider serviceProvider,
			ICommand<TResult> command,
			CancellationToken ct)
		{
			var typed = (TCommand)command;

			var handler = serviceProvider
				.GetRequiredService<ICommandHandler<TCommand, TResult>>();

			return BuildPipeline<TCommand, Result<TResult>>(
				serviceProvider,
				typed,
				() =>
					handler.HandleAsync(
						typed,
						ct),
				ct)();
		}
	}
}
```

The query path is the same shape: `TypedQueryInvoker<TQuery, TResult>` resolves
`IQueryHandler<TQuery, TResult>` and calls `BuildPipeline<TQuery, TResult>` — note the response type
is the bare `TResult`, no wrapper, per §1.

### 2.3 One fold, so ordering cannot diverge — the frozen answer

**The behaviour chain is built in exactly one place**, a single `private static` generic method on
`Dispatcher` that all three invoker families call:

```csharp
	private static Func<Task<TResponse>> BuildPipeline<TRequest, TResponse>(
		IServiceProvider serviceProvider,
		TRequest request,
		Func<Task<TResponse>> handler,
		CancellationToken ct)
			where TRequest : IRequest<TResponse>
	{
		var behaviors = serviceProvider
			.GetServices<IPipelineBehavior<TRequest, TResponse>>()
			.Reverse()
			.ToList();

		var pipeline = handler;

		foreach (var behavior in behaviors)
		{
			var next = pipeline;
			pipeline = () =>
				behavior.HandleAsync(
					request,
					next,
					ct);
		}

		return pipeline;
	}
```

This is the answer to "how does behaviour ordering stay identical on both paths": **the ordering
logic is not replicated across the paths, so it cannot differ between them.** There is one
`.Reverse()`, one fold, one closure shape, in one method that the no-payload, value and query
invokers all call with different type arguments. `.Reverse()` before the fold produces
first-registered-outermost, which CORE-012 confirms is correct today; changing it would reverse
behaviour order on all three paths at once, which is far easier to catch in review than reversing it
on one path only.

An implementer who inlines the fold into the three invokers has removed the property this section
exists to guarantee. That is a **blocking** finding even if every test still passes, because what is
frozen here is the absence of a second copy.

### 2.4 The mutual-exclusivity invariant

Stated in §1, enforced in §4.1, tested in §4.3. It is what licences §2.1's single cache. Restated
here so a reviewer reading only §2 sees it: **no concrete class may implement more than one of
`ICommand`, `ICommand<T>` and `IQuery<T>`.** All fourteen commands and both queries satisfy it by
construction after T4, and the self-check keeps it true.

Remaining binding points, unchanged from the pre-amendment design:

- **`dynamic` disappears entirely.** Reviewer check:
  `grep -rn 'dynamic' src/DungeonMaster.Application` returns nothing. This also removes the
  `net10.0-maccatalyst` runtime-binder concern CORE-011 flagged as unconfirmed.
- **The invoker instance is cached; the handler and behaviours are not.** They are scoped services
  and must be resolved per dispatch — which is why `BuildPipeline` takes the provider and resolves
  inside it, rather than the invoker caching a behaviour list. Caching them would leak a scope, a
  bug worse than the one being fixed. `backend-conventions.md` §7: "Services that hold
  request-scoped state ... use a scoped lifetime."
- `Invokers` is `static`, so the cache is process-wide. That is safe: every invoker is stateless and
  takes the provider as a parameter. The `GetOrAdd` factories are `static` lambdas for the same
  reason — they must not close over the `Dispatcher` instance.
- **`Dispatcher` stays a service locator internally, and that is fine.** CORE-010's suggested
  direction says so: "That keeps the locator inside `Dispatcher` — which is unavoidable for a
  runtime-dispatched design — while removing the class of bug it enables." The class of bug is the
  hand-maintained list, removed in §4.

---

## 3. `ValidationBehavior` — async, tokened, complete, and on both command paths

**Split between T5 and T6, and the split follows §1.5's sequencing.**

- **T5 authors** `ValidationFailureCollector` (§3.1) and the **value arity**
  `ValidationBehavior<TCommand, TResult>`, against today's `IPipelineBehavior<in TCommand, TResult>`.
  That is all T5 can write: the no-payload arity needs `IPipelineBehavior` generalised over
  `(TRequest, TResponse)`, which is T6's §1.1, and T5 runs concurrently with T4 and must not touch
  `Common/Dispatch/`.
- **T6 re-types** the value arity to `IPipelineBehavior<TCommand, Result<TResult>>` when §1.1 lands
  — a base-list change only, body byte-identical — and **adds** the no-payload arity
  `ValidationBehavior<TCommand>` plus its three tests (§3.2). That class is about twenty lines,
  because the loop is already in the collector.

The collector is written by T5 with both callers in view, which is why it returns
`IReadOnlyList<ValidationError>` rather than a `Result` of either shape.

Closes MASTER-002 (`Validate` is synchronous and `ct` is never passed) and MASTER-046 (only
`failures[0]` survives).

### 3.1 One collector, two thin behaviours

The failure-gathering loop is written **once**, in an `internal static` helper, because `Result` and
`Result<T>` are frozen as siblings (`contracts/result-and-error-surface.md` §2.1) and the only thing
the two behaviours cannot share is the final factory call.

**New file** `Common/Behaviors/ValidationFailureCollector.cs`:

```csharp
namespace DungeonMaster.Application.Common.Behaviors;

/// <summary>
///		Runs every validator for a command and gathers the failures.
/// </summary>
internal static class ValidationFailureCollector
{
	/// <summary>
	///		Runs each validator in turn and returns every failure, in validator order then rule
	///		order. An empty list means the command is valid.
	/// </summary>
	public static async Task<IReadOnlyList<ValidationError>> CollectAsync<TCommand>(
		IEnumerable<IValidator<TCommand>> validators,
		TCommand command,
		CancellationToken ct)
	{
		var context = new ValidationContext<TCommand>(command);
		var errors = new List<ValidationError>();

		foreach (var validator in validators)
		{
			var validationResult = await validator.ValidateAsync(
				context,
				ct);

			errors.AddRange(
				validationResult.Errors.Select(failure =>
					new ValidationError(
						failure.PropertyName,
						failure.ErrorMessage)));
		}

		return errors;
	}
}
```

**New file** `Common/Behaviors/ValidationBehavior.cs` — both arities, one file, per the naming rule
in §1:

```csharp
/// <summary>
///		Rejects a no-payload command that fails validation, before the handler runs.
/// </summary>
public sealed class ValidationBehavior<TCommand>(
	IEnumerable<IValidator<TCommand>> validators)
		: IPipelineBehavior<TCommand, Result>
			where TCommand : ICommand
{
	/// <inheritdoc/>
	public async Task<Result> HandleAsync(
		TCommand command,
		Func<Task<Result>> next,
		CancellationToken ct)
	{
		var errors = await ValidationFailureCollector.CollectAsync(
			validators,
			command,
			ct);

		if (errors.Count > 0)
		{
			return Result.ValidationFailure(errors);
		}

		return await next();
	}
}

/// <summary>
///		Rejects a value-producing command that fails validation, before the handler runs.
/// </summary>
public sealed class ValidationBehavior<TCommand, TResult>(
	IEnumerable<IValidator<TCommand>> validators)
		: IPipelineBehavior<TCommand, Result<TResult>>
			where TCommand : ICommand<TResult>
{
	/// <inheritdoc/>
	public async Task<Result<TResult>> HandleAsync(
		TCommand command,
		Func<Task<Result<TResult>>> next,
		CancellationToken ct)
	{
		var errors = await ValidationFailureCollector.CollectAsync(
			validators,
			command,
			ct);

		if (errors.Count > 0)
		{
			return Result<TResult>.ValidationFailure(errors);
		}

		return await next();
	}
}
```

Binding points:

- **`ValidateAsync(context, ct)`**, awaited in a `foreach`, in the collector. Not `Select(...)` over
  a synchronous call, and not `Task.WhenAll` — validators may share a scoped dependency and running
  them concurrently is a different change than the one this entry asks for.
- **Every failure survives**, in validator order then rule order, with its `PropertyName`.
- **Neither behaviour re-implements the loop.** The whole of MASTER-002 and MASTER-046 lives in
  `ValidationFailureCollector`; each behaviour is a null check and a factory call. An implementer who
  copies the loop into both classes has created two places for the async bug to come back, and that
  is a blocking finding.
- The behaviour never throws. A validator that throws is a programming error and propagates —
  `dotnet-guidelines.md` §6.2.
- The collector is `internal`, not `public`: it is an implementation seam, not part of the surface
  other tasks consume. **T5 must not add an `InternalsVisibleTo` for it** — T5's tests reach the
  collector only through the two public behaviours, which is the right level to test it at anyway,
  and `src/DungeonMaster.Application/AssemblyInfo.cs` belongs to T6 (§4.3). A T5 diff that creates
  that file is out of ownership.

### 3.2 Tests T5 owns

`src/DungeonMaster.Tests/Application/Common/Behaviors/ValidationBehaviorTests.cs` — **a new file;
there is no test for this class today.**

| Test | Proves |
|---|---|
| `HandleAsync_WithNoValidators_CallsNext` | the empty case ten commands hit today |
| `HandleAsync_WithPassingValidator_CallsNext` | |
| `HandleAsync_WithOneFailure_ReturnsValidationFailureAndDoesNotCallNext` | |
| `HandleAsync_WithThreeFailures_CarriesAllThreeValidationErrors` | **MASTER-046.** `result.ValidationErrors.Should().HaveCount(3)` and `result.Error.Should().Contain(...)` for each of the three messages |
| `HandleAsync_WithTwoValidators_CarriesFailuresFromBoth` | aggregation across validators |
| `HandleAsync_WithFailures_PreservesPropertyName` | the half CORE-008 says is dropped |
| `HandleAsync_WhenCalled_PassesTheTokenToEveryValidator` | **MASTER-002.** Substitute an `IValidator<T>`, capture the `CancellationToken` argument of `ValidateAsync`, assert it equals the token passed in. A cancelled-token variant that asserts `ValidateAsync` was reached with `IsCancellationRequested` true is acceptable as a second case, not as a replacement. |
| `HandleAsync_WithAsyncRule_DoesNotThrow` | the runtime failure CORE-003 predicts: build a real `AbstractValidator<T>` with a `MustAsync` rule and dispatch through the behaviour. **This test throws `AsyncValidatorInvokedSynchronouslyException` before the change and passes after.** It is the strongest single criterion for MASTER-002. |

**All eight are written by T5 against `ValidationBehavior<TCommand, TResult>`, the value arity** —
the only arity that exists when T5 runs (§1.5). They carry the whole of MASTER-002 and MASTER-046,
because the logic they exercise lives in the shared collector.

**T6 adds three more**, against the no-payload arity `ValidationBehavior<TCommand>` it authors, in
the same file:

| Test | Proves |
|---|---|
| `HandleAsync_WithFailures_ReturnsNonGenericValidationFailure` | `Result.ValidationFailure` is the factory reached, and `ValidationErrors` has the same count the value arity produces from the same validators |
| `HandleAsync_WithPassingValidator_CallsNext` | the success path reaches the handler |
| `HandleAsync_WhenCalled_PassesTheTokenToEveryValidator` | the token half holds on this arity too |

Eleven tests in the file when T6 is done: eight from T5, three from T6. Both sets define their own
test-local command and validator types (see T5's acceptance criteria) so that T4's concurrent
migration cannot break them.

---

## 4. Registration by assembly scan — MASTER-089

**Rewritten file** `src/DungeonMaster.Application/DependencyInjection.cs`. Today it hand-writes
sixteen closed-generic registrations (fourteen commands, two queries) plus four validators and the
dispatcher — **21 `AddScoped<` calls, measured**:

```
grep -c 'AddScoped<' src/DungeonMaster.Application/DependencyInjection.cs   # 21 before, at most 1 after
```

A command whose handler registration is forgotten compiles, passes every unit test, and throws only
when a user reaches that screen.

`AddApplicationServices` becomes, in order:

1. `services.AddScoped<IDispatcher, Dispatcher>();`
2. **Handlers.** Scan `typeof(ApplicationServiceCollectionExtensions).Assembly` for every
   non-abstract, non-generic class, and for each interface it implements that is a closed
   `ICommandHandler<>`, `ICommandHandler<,>` or `IQueryHandler<,>`, register it scoped against that
   closed interface. **Three open-generic shapes to match now, not two** — an implementer who
   matches only the two-parameter forms silently drops all ten no-payload handlers, and §4.1's
   self-check is what catches it.
3. **Validators.** The same scan for closed `IValidator<>`.
4. **Validation behaviour, closed per command, on both arities.**
   - For every closed `ICommandHandler<TCommand>` found in step 2, register
     `IPipelineBehavior<TCommand, Result>` implemented by `ValidationBehavior<TCommand>`, scoped.
   - For every closed `ICommandHandler<TCommand, TResult>` found in step 2, register
     `IPipelineBehavior<TCommand, Result<TResult>>` implemented by
     `ValidationBehavior<TCommand, TResult>`, scoped.

   Ten registrations from the first bullet and four from the second, fourteen in total — one per
   command, matching the fourteen handlers.

### 4.1 The self-check

**New public method** on `ApplicationServiceCollectionExtensions`:

```csharp
/// <summary>
///		Asserts that every command and query type in the Application assembly implements exactly one
///		request marker and has exactly one registered handler.
/// </summary>
/// <exception cref="InvalidOperationException">
///		Thrown when any request type implements more than one of <c>ICommand</c>,
///		<c>ICommand&lt;T&gt;</c> and <c>IQuery&lt;T&gt;</c>, or has no handler, or has more than
///		one. The message names every offending type.
/// </exception>
public static void ValidateHandlerRegistrations(IServiceProvider serviceProvider)
```

It performs **two** checks, and both are required:

1. **Exactly one marker per request type.** This is §2.4's mutual-exclusivity invariant, and it is
   what licences the single invoker cache in §2.1. Without this check the cache's cast is an
   unproven assumption.
2. **Exactly one registered handler per request type**, across all three handler shapes. This is
   CORE-010's "add a startup self-check that asserts every `ICommand<>` in the assembly has exactly
   one registered handler".

It is a public method rather than an unconditional call inside `AddApplicationServices` because
building a provider mid-registration is itself an anti-pattern. A composition root may call it;
**a test must call it**, and that test is the acceptance criterion.

### 4.2 Why the validation behaviour is closed per command

Step 4 is deliberately **not** an open-generic registration of
`typeof(IPipelineBehavior<,>) → typeof(ValidationBehavior<,>)`. Two reasons, and both are binding:

- An open-generic registration would be a candidate for a **query** closure
  `IPipelineBehavior<GetDungeonsQuery, IReadOnlyList<DungeonSummaryDto>>`, whose closure of
  `ValidationBehavior<,>` violates its `where TCommand : ICommand<TResult>` constraint. Whether the
  container silently skips it or throws depends on the container's constraint handling, which is
  not something this session should be betting on. The amendment makes this sharper, not softer:
  with `IPipelineBehavior` now constrained on `IRequest<TResponse>` rather than on `ICommand`, the
  query closure is *legal at the interface* and only the behaviour's own constraint rejects it.
- Closing it per command is what makes §1.4 true by construction: **a query cannot acquire a
  validation behaviour because none is ever registered for one.**

### 4.3 Tests T6 owns

`src/DungeonMaster.Tests/Application/Common/Dispatch/` — a new directory.

| Test | Proves |
|---|---|
| `ValidateHandlerRegistrations_OnTheRealContainer_DoesNotThrow` | **MASTER-089.** Build a `ServiceCollection`, call `AddApplicationServices`, substitute the two repository ports, build the provider, call the self-check. |
| `ValidateHandlerRegistrations_WithAMissingHandler_ThrowsNamingTheCommand` | the check detects the class of bug CORE-010 describes, not merely that it runs. Remove one handler registration from a container and assert the throw names that command type. |
| `EveryRequestType_ImplementsExactlyOneMarker` | **§2.4.** Enumerate every non-abstract class in the Application assembly, count how many of `ICommand`, `ICommand<>`, `IQuery<>` it implements, assert every count is 0 or 1. This is the test that licences the single cache in §2.1; without it the cast there is unproven. |
| `AddApplicationServices_RegistersAHandlerForEveryCommand` | enumerate every `ICommand` **and** `ICommand<>` implementation in the assembly by reflection and resolve its handler. Asserts **14** across both arities — **10 no-payload and 4 value-producing** — with the counts computed from the assembly, not hard-coded, so adding a fifteenth command cannot pass silently. |
| `AddApplicationServices_RegistersAHandlerForEveryQuery` | asserts **2**, same way. |
| `AddApplicationServices_RegistersNoValidationBehaviourForAQuery` | **§1.4.** Resolving `IEnumerable<IPipelineBehavior<GetDungeonsQuery, IReadOnlyList<DungeonSummaryDto>>>` returns empty and does not throw. |
| `SendAsync_WithTwoBehaviours_RunsThemFirstRegisteredOutermost` | the ordering CORE-012 confirmed correct is preserved through the rewrite. Register two recording behaviours on a **no-payload** command, assert the recorded sequence. |
| `SendAsyncOfT_WithTwoBehaviours_RunsThemInTheSameOrder` | **§2.3.** The same two recording behaviours on a **value** command produce the **same** recorded sequence. The two tests together are what prove ordering did not diverge between the arities; either one alone proves nothing about the other. |
| `SendAsync_DispatchedTwice_ReflectsTypeOnlyOnce` | **MASTER-090.** Not a timing test. Dispatch the same command type twice and assert the cached invoker instance is the same object — expose the cache through an `internal` member and `InternalsVisibleTo`, or assert on a static counter incremented in the invoker factory. The implementer picks one and states which; a benchmark is not acceptable evidence. |
| `SendAsync_WithBothCommandArities_SharesOneInvokerCache` | **§2.1.** Dispatch a no-payload command and a value command, and assert the single `Invokers` dictionary holds one entry per concrete command type and that each retrieves as its own invoker family. This is the test that the one-cache decision is actually implemented as one cache. |
| `QueryAsync_WithABehaviour_RunsItAroundTheHandler` | **MASTER-080.** Register a recording `IPipelineBehavior<GetDungeonsQuery, IReadOnlyList<DungeonSummaryDto>>`, dispatch the query, assert the behaviour ran before and after the handler. **This test cannot even be written before the change** — there is no query behaviour interface today — which is exactly the failing-before/passing-after property this session requires. |
| `QueryAsync_WithNoBehaviour_ReturnsTheHandlerResult` | no regression on the two existing queries |

`src/DungeonMaster.Domain/AssemblyInfo.cs` already exists as precedent for `InternalsVisibleTo`. The
Application project has no such file today; adding one is permitted under T6's ownership and is
needed regardless, because §3.1's `ValidationFailureCollector` is `internal`.

---

## 5. Validators — MASTER-079

**Authored by T7.** Ten commands have no validator today. The command that proves it, and that a
reviewer re-runs:

```
for d in src/DungeonMaster.Application/Features/Dungeons/Commands/*/; do
  ls "$d"*Validator.cs >/dev/null 2>&1 || basename "$d"
done
```

**Before: 10 lines** — `AddFloor`, `AddPlacedObject`, `DeleteDungeon`, `RemoveArea`, `RemoveFloor`,
`RemovePlacedObject`, `ReorderFloor`, `UpdateAreaOutline`, `UpdateFloor`, `UpdatePlacedObject`.
**After: no output.**

Each new file is `Features/Dungeons/Commands/{Name}/{Name}CommandValidator.cs`, a
`public sealed class {Name}CommandValidator : AbstractValidator<{Name}Command>` with a documented
parameterless constructor, per `backend-conventions.md` §1's directory tree and `dotnet-format.md`
§10. They need no DI edit: §4 step 3 picks them up by scan.

**Minimum rule set per command.** Every `Guid` field gets `.NotEmpty()`. Beyond that, the three
gaps CORE-009 names by hand are mandatory:

| Command | Mandatory rules |
|---|---|
| `ReorderFloorCommand` | `NewNumber` `.GreaterThanOrEqualTo(1)`. The upper bound depends on the dungeon's floor count and belongs to the aggregate, which now reports `OutOfRange` — do not attempt it in the validator. |
| `AddPlacedObjectCommand` | `Scale` `.InclusiveBetween(ObjectScale.Min, ObjectScale.Max)`. `src/DungeonMaster.Domain/Entities/Floor/ObjectScale.cs:5-6` declares `Min = 0.25f` and `Max = 4.0f` as public constants — **reference the constants, do not copy the literals**, so the validator cannot drift from the constructor guard. A pre-validation rule that disagrees with the guard is worse than no rule. |
| `UpdatePlacedObjectCommand` | the same `ObjectScale.Min`/`ObjectScale.Max` bounds on `NewScale` where present |
| `UpdateAreaOutlineCommand` | `Outline` `.NotNull()` and `.NotEmpty()` |
| `UpdateFloorCommand`, `AddFloorCommand` | name length bounds matching the existing `AddAreaCommandValidator` and `UpdateAreaCommandValidator` — read them and match, do not invent new limits |

`backend-conventions.md` §5 applies where a field is policy-backed: a validator calls the policy, it
does not re-express the policy's rules. This repository has no policy types today, so the clause is
inert here — noted so a reviewer does not raise it.

### 5.1 Tests T7 owns

One file per validator in
`src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/`, matching the four that already
exist. Naming per `backend-conventions.md` §9: `Validate_WithShortPassword_ReturnsViolation` is the
cited shape, so `Validate_WithEmptyDungeonId_ReturnsViolation`.

One additional cross-cutting test, in the same directory:

- `EveryCommand_HasARegisteredValidator` — enumerate every implementation of **`ICommand` or
  `ICommand<>`** in the Application assembly by reflection, resolve `IEnumerable<IValidator<TCommand>>`
  from a real container, assert each is non-empty. Asserts **14 of 14** — ten no-payload plus four
  value-producing — computed from the assembly rather than hard-coded. **Enumerating only
  `ICommand<>` would silently skip ten of the fourteen**, which are exactly the ten this task adds
  validators for, so the test would pass while proving nothing. This is MASTER-079's closing
  criterion and it is the one that cannot rot.
