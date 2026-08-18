# Contract: the Result and error surface

Frozen at Gate 2. **Authored by T2. Consumed by T1, T3, T4, T5, T6, T7 and T8.** No task other
than T2 may change any signature in this file; a task that believes it must, escalates.

Closes: **MASTER-088** (the `Result<T>`-permits-invalid-states half), **MASTER-045**, and supplies
the failure vocabulary that **MASTER-046** and **MASTER-001** consume.

Location: `src/DungeonMaster.Application/Common/Results/`, which
`backend-conventions.md` §1 annotates as "Result and error types".

---

## 1. The defect being closed

`Result.cs` today is a `public record Result<T>` with four `public { get; init; }` members and two
static factories. Nothing prevents

```csharp
new Result<Guid> { IsSuccess = true, Error = "boom" }
```

Repository-wide check (run it; it must return **nothing** after T2, and it returns nothing today
too, because no caller currently abuses the initialiser — the point is that the *type* stops
permitting it):

```
grep -rn 'new Result<' --include=*.cs src | grep -v 'Common/Results'
```

`ErrorCategory` declares four members. `Conflict` and `Unauthorized` have no producer anywhere, and
`Forbidden` — named by `backend-conventions.md` §2 — is missing. Verified:

```
grep -rn 'ErrorCategory.Conflict\|ErrorCategory.Unauthorized' --include=*.cs src   # no output
```

Ten of the fourteen commands are `ICommand<bool>` whose handlers can only ever return
`Success(true)`. **The audit record CORE-006 says "nine of the fourteen"; the measured number is
ten.** The command that produces it, for the reviewer:

```
grep -rl 'ICommand<bool>' src/DungeonMaster.Application/Features/Dungeons/Commands/ | wc -l   # 10
grep -rl 'ICommand<Guid>' src/DungeonMaster.Application/Features/Dungeons/Commands/ | wc -l   # 4
```

The ten: `DeleteDungeon`, `RemoveArea`, `RemoveFloor`, `RemovePlacedObject`, `ReorderFloor`,
`UpdateArea`, `UpdateAreaOutline`, `UpdateDungeon`, `UpdateFloor`, `UpdatePlacedObject`.
The four: `AddArea`, `AddFloor`, `AddPlacedObject`, `CreateDungeon`.

---

## 2. `Result` — the non-generic sibling

**New file** `Common/Results/Result.cs`, holding the non-generic `Result`. `Result<T>` moves out of
today's `Result.cs` into **`Common/Results/ResultOfT.cs`** (see §2.3 for why the filename is not
`Result.cs`).

```csharp
namespace DungeonMaster.Application.Common.Results;

/// <summary>
///		Represents the outcome of an operation that produces no value.
/// </summary>
public sealed record Result
{
	private Result() { }

	/// <summary>
	///		Gets a value indicating whether the operation succeeded.
	/// </summary>
	public bool IsSuccess { get; private init; }

	/// <summary>
	///		Gets a value indicating whether the operation failed.
	/// </summary>
	public bool IsFailure =>
		!IsSuccess;

	/// <summary>
	///		Gets the summary failure message, or <see langword="null"/> on success.
	/// </summary>
	public string? Error { get; private init; }

	/// <summary>
	///		Gets the category of the failure, or <see langword="null"/> on success.
	/// </summary>
	public ErrorCategory? ErrorCategory { get; private init; }

	/// <summary>
	///		Gets the individual validation failures. Never <see langword="null"/>; empty unless the
	///		operation failed validation.
	/// </summary>
	public IReadOnlyList<ValidationError> ValidationErrors { get; private init; } = [];

	/// <summary>
	///		Creates a successful result.
	/// </summary>
	public static Result Success() =>
		new()
		{
			IsSuccess = true,
		};

	/// <summary>
	///		Creates a failed result carrying a message and a category.
	/// </summary>
	public static Result Failure(
		string error,
		ErrorCategory category) =>
			new()
			{
				IsSuccess = false,
				Error = error,
				ErrorCategory = category,
			};

	/// <summary>
	///		Creates a failed result carrying every validation failure.
	/// </summary>
	public static Result ValidationFailure(
		IReadOnlyList<ValidationError> errors) =>
			new()
			{
				IsSuccess = false,
				Error = string.Join(
					" ",
					errors.Select(error =>
						error.Message)),
				ErrorCategory = Results.ErrorCategory.Validation,
				ValidationErrors = errors,
			};
}
```

The invariants are **identical to `Result<T>`'s** and binding for the same reasons: `sealed`, a
`private` parameterless constructor, `private init` on every member so that neither an object
initialiser nor a `with` expression is reachable from outside the type, and `ValidationErrors`
defaulting to `[]` so a caller may iterate it unconditionally. `Success()` takes no argument
because there is no value; there is deliberately no `Failure()` overload that carries one.

### 2.1 The frozen relationship: siblings, not a hierarchy

**`Result<T>` does NOT derive from `Result`. The two are independent sealed records.** An
implementer must not add an inheritance edge, a shared base record, or a shared public interface.

The reasoning, so that a reviewer does not re-open it:

- **`sealed` is the load-bearing invariant of this whole entry.** MASTER-088 is "the type permits
  invalid states". A base class cannot be `sealed`, and its constructor would have to be at least
  `private protected` for `Result<T>` to call it, which reopens derivation to anything in the
  Application assembly. Trading the strongest available guarantee for the avoidance of about forty
  lines of factory methods is the wrong trade in the one entry whose subject is that guarantee.
- **Records inherit awkwardly.** A derived record's synthesised `Equals` compares
  `EqualityContract`, so `Result<T>` and `Result` would never be equal even when both are a
  `NotFound` failure — a surprise with no upside here.
- **Nothing in this session needs the polymorphism.** The only place that would have consumed a
  common base is `ResultMessage.ForFailure`, and §6 handles both shapes with two thin overloads
  over one shared body, so the switch is still written exactly once.

The cost is accepted knowingly: the two records duplicate five members and three factories. **A
reviewer must not raise that duplication as a finding** — it is the deliberate consequence of a
frozen decision.

### 2.2 Why non-generic and not a `Unit` payload

This reverses the draft that went into Gate 2. It is a **judgment call, not a citable standard**,
and it is recorded as such in `spec.md`'s Assumptions table at Rung 4.

FluentResults and CSharpFunctionalExtensions, the two most widely used dedicated .NET Result
libraries, both ship a non-generic `Result` alongside `Result<T>`; ErrorOr instead uses a
placeholder (`ErrorOr<Success>`), which is the `Unit` shape. The weighted signal is MediatR — the
closest analogue to this repository's hand-rolled dispatcher — which removed `Unit` in v12 in
favour of a non-generic request path after years of shipping the `Unit` approach. Three of four
favour non-generic, and the CQRS-specific precedent migrated off `Unit`. **Not confirmed by
citation; recorded as belief.**

The user accepted the resulting increase in T2 and T6 effort explicitly. CORE-006's suggested
direction permits either: "Consider adding a non-generic `Result` (or a `Unit` payload) for
commands with no return value".

**There is no `Unit` type in this session.** Reviewer check, after T2:

```
grep -rn 'Unit' --include=*.cs src/DungeonMaster.Application src/DungeonMaster.App   # no output
```

### 2.3 File naming

`dotnet-format.md` §2 requires one type per file with the file name matching the type name exactly.
Two types both named `Result` cannot both occupy `Result.cs`, so the second rule is unsatisfiable
and the first is kept: **`Result.cs` holds `Result`, `ResultOfT.cs` holds `Result<T>`.** The `OfT`
suffix is the conventional .NET workaround and is preferred here over `Result{T}.cs` (brace
characters in paths are hostile to shell tooling) and over putting both records in one file.
This is a Rung 4 judgment; it is recorded in `spec.md` and a reviewer should not treat the
deviation as unnoticed.

The corresponding test files are `ResultTests.cs` and `ResultOfTTests.cs` (§7).

---

## 3. `Result<T>` — invalid states unrepresentable

**Rewritten file, renamed** `Common/Results/ResultOfT.cs` (§2.3). The shape is frozen exactly as
below; only the XML documentation text may differ. It is a **sibling** of the non-generic `Result`,
not a derived type — §2.1.

```csharp
namespace DungeonMaster.Application.Common.Results;

public sealed record Result<T>
{
	private Result() { }

	public bool IsSuccess { get; private init; }

	public bool IsFailure =>
		!IsSuccess;

	public T? Value { get; private init; }

	public string? Error { get; private init; }

	public ErrorCategory? ErrorCategory { get; private init; }

	public IReadOnlyList<ValidationError> ValidationErrors { get; private init; } = [];

	public static Result<T> Success(T value) =>
		new()
		{
			IsSuccess = true,
			Value = value,
		};

	public static Result<T> Failure(
		string error,
		ErrorCategory category) =>
			new()
			{
				IsSuccess = false,
				Error = error,
				ErrorCategory = category,
			};

	public static Result<T> ValidationFailure(
		IReadOnlyList<ValidationError> errors) =>
			new()
			{
				IsSuccess = false,
				Error = string.Join(
					" ",
					errors.Select(error =>
						error.Message)),
				ErrorCategory = Results.ErrorCategory.Validation,
				ValidationErrors = errors,
			};
}
```

Binding points:

- **`sealed` + `private` parameterless constructor + `private init` on every member.** Together
  these make `new Result<T> { ... }` and `existing with { ... }` uncompilable outside the type,
  which is the whole of MASTER-088's first half. The record's generated copy constructor is
  `private` because the record is `sealed`.
- **`IsFailure`** is added because thirteen call sites in T4 and T8 read the negation, and
  `!result.IsSuccess` at every one of them is noise.
- **`Value` nullability is deliberately unchanged** (`T?`, no `[MemberNotNullWhen]`). That question
  is `MASTER-044`, which the master index parks explicitly pending this session. Do not answer it
  here; do not add annotations that pre-empt it.
- **`ValidationErrors` defaults to `[]`, never null.** A caller may iterate it unconditionally.
  On a non-validation failure and on success it is empty.
- `ValidationFailure` joins the messages with a single space so that the summary `Error` string
  carries **every** message, not the first — that is MASTER-046's observable half.

### 3.1 Failure factories must not accept a success

There is deliberately no `Failure` overload that takes a value and no `Success` overload that takes
an error. That is the invariant. A test asserts the type refuses the states — see §7.

---

## 4. `ValidationError`

**New file** `Common/Results/ValidationError.cs`.

```csharp
namespace DungeonMaster.Application.Common.Results;

/// <summary>
///		A single validation failure, naming the property that failed and why.
/// </summary>
/// <param name="PropertyName">
///		The name of the property that failed validation. Empty when the rule is not property-scoped.
/// </param>
/// <param name="Message">
///		The human-readable description of the failure.
/// </param>
public sealed record ValidationError(
	string PropertyName,
	string Message);
```

This is what CORE-008 asks for: "Carry the failures as structured data on the failed result — a
property-name-to-messages map alongside the summary string — so the single-message path stays
available for simple callers while richer callers can bind per field." A flat list preserves both
ordering and duplicate property names, which a dictionary does not; callers that want a per-field
view group by `PropertyName`.

---

## 5. `ErrorCategory` — five members, aligned to the conventions

**Rewritten file** `Common/Results/ErrorCategory.cs`. Members, in this order:

| Member | Produced by, after this session |
|---|---|
| `Validation` | `ValidationBehavior` (T5); `Dungeon.ReorderFloor`'s out-of-range outcome mapped in T4 |
| `NotFound` | every handler's dungeon guard; every `MutationOutcome.NotFound`; `DeleteDungeon` when nothing was deleted |
| `Conflict` | no producer yet |
| `Unauthorized` | no producer yet |
| `Forbidden` | **new member**; no producer yet |

**Decision, and it is a judgment call the reviewer should read before objecting.** CORE-007's
suggested direction offers two routes and closes with "Align the member set with the conventions
either way." `backend-conventions.md` §2 "Result Type" names the set explicitly:

> - An error category (Validation, NotFound, Conflict, Unauthorized, Forbidden)

The conventions document is a rung-1 authority and it names five. Deleting members would deviate
from it; adding `Forbidden` complies with it. The half of CORE-007 that actually bites — "no
production caller reads the category" — is closed by §6 instead, which gives every member a
consumer. An enum member with a consumer and no producer yet is a documented extension point; an
enum with neither is the defect the record describes.

The alternative (reduce to `Validation` and `NotFound`) is recorded in the architect's Gate 2
report as a ranked option. If the user prefers it, this contract is amended before Gate 2, not
after.

---

## 6. `ResultMessage` — the consumer the category never had

**New file** `Common/Results/ResultMessage.cs`. It serves **both** result shapes through two thin
public overloads over one private body, so the five-way switch is written exactly once. This is the
concrete discharge of §2.1's third bullet.

```csharp
namespace DungeonMaster.Application.Common.Results;

/// <summary>
///		Turns a failed result into the single string a presentation layer displays.
/// </summary>
public static class ResultMessage
{
	/// <summary>
	///		Returns the message to display for the given result, or <see langword="null"/> when the
	///		result succeeded.
	/// </summary>
	public static string? ForFailure(Result result) =>
		Format(
			result.IsSuccess,
			result.Error,
			result.ErrorCategory,
			result.ValidationErrors);

	/// <summary>
	///		Returns the message to display for the given result, or <see langword="null"/> when the
	///		result succeeded.
	/// </summary>
	public static string? ForFailure<T>(Result<T> result) =>
		Format(
			result.IsSuccess,
			result.Error,
			result.ErrorCategory,
			result.ValidationErrors);

	private static string? Format(
		bool isSuccess,
		string? error,
		ErrorCategory? category,
		IReadOnlyList<ValidationError> validationErrors)
	{
		if (isSuccess)
		{
			return null;
		}

		return category switch
		{
			Results.ErrorCategory.Validation =>
				FormatValidation(
					error,
					validationErrors),
			Results.ErrorCategory.NotFound =>
				error,
			Results.ErrorCategory.Conflict =>
				error,
			Results.ErrorCategory.Unauthorized =>
				"You are not signed in.",
			Results.ErrorCategory.Forbidden =>
				"You do not have permission to do that.",
			_ =>
				error ?? "The operation failed.",
		};
	}

	private static string FormatValidation(
		string? error,
		IReadOnlyList<ValidationError> validationErrors) =>
			validationErrors.Count == 0
				? error ?? "The operation failed."
				: string.Join(
					Environment.NewLine,
					validationErrors.Select(validationError =>
						string.IsNullOrEmpty(validationError.PropertyName)
							? validationError.Message
							: $"{validationError.PropertyName}: {validationError.Message}"));
}
```

Binding points:

- **Both overloads are required and both must be exercised by tests.** A call site holding a
  non-generic `Result` and a call site holding a `Result<Guid>` both write
  `ResultMessage.ForFailure(result)` with no type argument and no cast; overload resolution picks
  the right one. That is what keeps T8's fourteen call sites uniform even though ten of them now
  dispatch a non-generic command and four do not.
- **The switch body is written once**, in `Format`. An implementer who copies the switch into both
  overloads has produced two places for a sixth `ErrorCategory` member to be forgotten, and that is
  a blocking finding.
- This is "a single result-to-message mapping the view models share", verbatim from CORE-007's
  first suggested direction. **All three view models route through it** (T8), which is what makes
  `ErrorCategory` a read field in production rather than a write-only one.
- It lives in the Application layer rather than the App project **so that it is testable**. The
  test project (`src/DungeonMaster.Tests/DungeonMaster.Tests.csproj`) references Domain,
  Application and Infrastructure and **not** the App project, which is multi-targeted to
  `net10.0-windows10.0.19041.0` and `net10.0-maccatalyst` and cannot be referenced from a `net10.0`
  test assembly. A formatter placed in the App project could not be unit-tested at all, and this
  session's acceptance criteria must be behavioural.
- The `switch` must be exhaustive over all five members. Adding a member later without a case is
  then caught by the discard arm rather than by an exception — deliberate; a missing case must
  never crash the UI.

---

## 7. Tests T2 owns

`src/DungeonMaster.Tests/Application/Common/Results/` — new directory, one file per new source
file.

- `ResultOfTTests.cs` — the generic record
  - `Success_WithValue_IsSuccessAndCarriesNoError`
  - `Failure_WithCategory_IsNotSuccessAndCarriesNoValue`
  - `ValidationFailure_WithThreeErrors_ErrorContainsAllThreeMessages` — this is MASTER-046's
    unit-level proof at the type level; T5 proves it end-to-end through the behaviour.
  - `ValidationFailure_WithNoErrors_ProducesEmptyValidationErrors`
- `ResultTests.cs` — the non-generic record, **the same four cases**, minus the value assertions:
  - `Success_IsSuccessAndCarriesNoError`
  - `Failure_WithCategory_IsNotSuccessAndCarriesTheCategory`
  - `ValidationFailure_WithThreeErrors_ErrorContainsAllThreeMessages`
  - `ValidationFailure_WithNoErrors_ProducesEmptyValidationErrors`
  - `Success_HasEmptyValidationErrors` — the never-null invariant on the success path
- `ResultMessageTests.cs` — one test per `ErrorCategory` member (five), plus
  `ForFailure_WithSuccess_ReturnsNull` and
  `ForFailure_WithValidationErrors_JoinsEveryMessageOnItsOwnLine`, **plus two that pin the
  dual-overload behaviour**: `ForFailure_WithNonGenericResult_ReturnsTheSameMessageAsTheGeneric`
  (construct the same failure both ways, assert the two messages are equal) and
  `ForFailure_WithSucceedingNonGenericResult_ReturnsNull`. Nine minimum.

**The invalid-state criterion is a compile-time property, not a runtime one**, so it is verified
by the negative-compilation check in T2's acceptance criteria, not by a test method. That check now
covers **both** records: `new Result { IsSuccess = true, Error = "boom" }` must fail to compile for
the same reason `new Result<Guid> { ... }` does.
