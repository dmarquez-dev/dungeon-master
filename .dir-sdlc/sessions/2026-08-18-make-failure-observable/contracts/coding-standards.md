# Contract: coding standards binding on every task in this session

Frozen at Gate 2. Every task in this session is bound by this file. It exists so that no
implementer has to re-derive the formatting rules and no reviewer has to argue them.

## 1. Indentation — TABS, non-negotiable

`.editorconfig` at the repository root sets `indent_style = tab` for `[*]`, `[*.cs]` and
`[*.xaml]` (lines 4, 13, 36, 39). `Directory.Build.props` sets `TreatWarningsAsErrors` and
Session 1 raised `dotnet_diagnostic.IDE0055.severity` to `warning`.

**A space-indented `.cs` file does not compile.** Every new file and every edited line in this
session is tab-indented. Continuation indent is one further tab.

## 2. Line endings — CRLF, and the failure is invisible to git

`.editorconfig` line 8 declares `end_of_line = crlf`; line 9 declares `insert_final_newline = true`.
`IDE0055` enforces both.

`.gitattributes` carries `* text=auto`, so an LF-ended file is normalised on the way into the
index and `git status` reports the tree **clean** while `dotnet build` fails with an IDE0055
error. Session 1 was bitten by exactly this and it cost a round.

**Every new file must be written with CRLF line endings.** Verify before reporting done — the
command to run, and to quote in the task report:

```
file <path>                      # must report "with CRLF line terminators"
```

## 3. XML documentation comments

`dm-conventions/backend/dotnet-format.md` §9 "XML Documentation Comments":

> All public types, members, and parameters must have XML documentation comments.
>
> - Tag descriptions go on a **new line**, indented with **two tabs** from the `///` marker
> - All descriptions are complete sentences ending with a period
> - **Line breaks occur only at sentence boundaries.** Never break a sentence across multiple
>   `///` lines.

This binds every public member added by this session — `Result` and `Result<T>`'s factories,
`ErrorCategory` members, `MutationOutcome` members, `IRequest<TResponse>`, the mutation helper, the
ten new validators, and the view models' `ErrorMessage` property. Where a member throws, document it
with `<exception cref="..."/>` (§11 precedent).

## 4. Call-site and declaration wrapping

`dotnet-format.md` §3 "Method Call Arguments":

> The 2+ parameter wrapping rule applies at call sites as well — not just method and constructor
> declarations. Any method call or object construction invoked with 2 or more arguments wraps each
> argument onto its own line.

`dotnet-format.md` §3 "Primary Constructors":

> **Primary constructor parameters**: always one per line, regardless of count.
> **Regular (body) constructor and method parameters**: one per line when there are 2 or more;
> inline when exactly 1.

**Decision, because the corpus contradicts itself.** `dotnet-format.md` §12's own test sample
writes `new RegisterUserCommandHandler(_userRepository, _passwordHasher)` and
`.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())` unwrapped, violating its own
§3 rule. **The §3 rule wins over the §12 sample**, in production code and in test code alike. New
and rewritten test files wrap 2-or-more-argument NSubstitute setups. Existing untouched lines are
not reformatted — this session is not a formatting sweep and gratuitous reflow is a review finding.

## 5. Expression bodies and lambdas

`dotnet-format.md` §7: "Everything after `=>` goes on a new line, indented one level."
`dotnet-format.md` §1: "Use descriptive names in lambda expressions — never single-letter
variables." The existing Domain code already obeys this (`floor =>`, `placedObject =>`); match it.

FluentValidation chains follow `dotnet-format.md` §8 verbatim:

```csharp
RuleFor(command =>
	command.Email)
	.NotEmpty()
	.EmailAddress()
	.MaximumLength(256);
```

## 6. Braces

`dotnet-format.md` §4: all `if`, `else`, `for`, `foreach`, `while`, `do`, `using`, `lock` bodies use
full braces, even for a single statement. No exceptions.

## 7. File organisation

`dotnet-format.md` §2:

> - One type per file (class, record, interface, enum)
> - File name matches the type name exactly
> - Folder structure mirrors the namespace hierarchy

Consequence binding on this session: `Result` (`Result.cs`), `Result<T>` (`ResultOfT.cs`),
`ErrorCategory`, `ValidationError`, `ResultMessage`, `MutationOutcome` and `IRequest<TResponse>` are
**seven separate files**.

Two deliberate deviations, both forced by C# arity overloading and both recorded in `spec.md`:

- `Result` and `Result<T>` share a type name, so they cannot both be `Result.cs`. The generic one
  takes `ResultOfT.cs` (`contracts/result-and-error-surface.md` §2.3).
- `ICommand` / `ICommand<TResult>` and `ICommandHandler<TCommand>` /
  `ICommandHandler<TCommand, TResult>` are two arities of one name each, and they **share** their
  existing files `ICommand.cs` and `ICommandHandler.cs`
  (`contracts/dispatch-and-pipeline.md` §1, §1.2). These are the only files in this session holding
  two types, and `ValidationBehavior.cs` likewise holds both behaviour arities (§3.1 there).

A reviewer should treat those five files as pre-approved and raise no finding against them; any
*other* file holding two types is a finding.

Namespaces are file-scoped (`.editorconfig` sets `csharp_style_namespace_declarations = file_scoped`).

## 8. Test conventions

`dotnet-format.md` §12 and `backend-conventions.md` §9.

- **Framework:** xUnit + FluentAssertions + NSubstitute. Never `Assert.*`.
- **Class shape:** `public class XTests`, not sealed. `private readonly` fields for substitutes and
  the system under test, constructed in the constructor. No `IClassFixture`, no shared state.
- **Naming:** `Method_Scenario_ExpectedResult`. **This repository keeps the `Async` suffix on the
  method segment** (`HandleAsync_WithExistingFloor_RemovesFloorAndReturnsSuccess`) where
  `dotnet-format.md` §12's sample writes `Handle_...`. **Repository precedent wins** — the 14
  existing handler test files are unanimous. The command that shows it, and that a reviewer can
  re-run:

  ```
  grep -rho 'public async Task [A-Za-z_]*' \
    src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/ | sort -u
  ```

- **Arrangement:** literal `// Arrange`, `// Act`, `// Assert` comments, with a blank line before
  `// Act` and before `// Assert`.
- **One test file per source file** — `backend-conventions.md` §9 "Test Organization": "Unit tests
  mirror the source structure: a test file per source file, grouped by layer". Every new public type
  this session introduces gets a test file.
- **Infrastructure tests** use the fixture pattern already established in
  `src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs`
  — a `SqliteConnection("Data Source=:memory:")` held open for the class lifetime, `EnsureCreated`,
  a private `CreateContext()` factory, `IDisposable`. Do not invent a second fixture.

## 9. The baseline this session must not lower

Measured on `chore/repo-audit` before this session:

- `dotnet build src/DungeonMaster.sln` — **0 warnings, 0 errors** under `TreatWarningsAsErrors`,
  building both App target frameworks (`net10.0-windows10.0.19041.0` and `net10.0-maccatalyst`).
- `dotnet test src/DungeonMaster.sln` — **292 passed, 0 failed**.

Every task ends with both commands run for real and their output quoted in the task report. A task
that leaves the build red, raises the warning count above zero, or loses a passing test has failed
regardless of what else it achieved.

The total test count **rises** across this session — every task adds tests. A task reports the count
it started from and the count it ended with. It must never lose a test it did not deliberately
replace, and any replacement is named in the report with its reason.

## 10. What the conventions do NOT cover — declared here, not cited

`dm-conventions` is silent on the following. They are decided here so that review does not re-open
them. A reviewer who wants a different answer should raise it as advisory, not blocking.

| Question | Decision |
|---|---|
| Name of the error enum — the corpus uses `ResultError` (dotnet-format §10), `ErrorCategory` (dotnet-format §3 samples) and `ResultErrorType` (dotnet-guidelines §6.1) for one concept | **`ErrorCategory`** — the name already in this repository, and the one `dotnet-format.md` §3's sample writes. Renaming is churn with no benefit. |
| Member ordering | No rule exists anywhere. Follow the file's existing order. For new files: fields, constructor, properties, then methods, with static factories last. |
| `var` usage | No rule exists. Use `var` for locals with an obvious initialiser, matching every existing file. |
| `Handle` vs `HandleAsync` | **`HandleAsync`** — the repository's custom interfaces already use it, and `backend-conventions.md` §6 exempts framework-interface method names from the `Async`-suffix rule either way. |
| Validator and handler registration mechanism | Assembly scan. See `contracts/dispatch-and-pipeline.md` §4. Nothing in the corpus addresses it, so nothing forbids it. |
| MAUI, XAML and view-model shape | The conventions repo has **no** MAUI, XAML or view-model document at all; `frontend/` is React only. `contracts/app-failure-channel.md` declares the shape in full, derived from this repository's own two working examples. |

## 11. Stale cross-references, so nobody chases a broken link

`backend-conventions.md` points at "dotnet-format.md § 8" for CQRS shapes, "§ 9" for domain building
blocks and "§ 10" for testing. The real numbering in `dotnet-format.md` is **§10 CQRS in C#, §11
Domain Building Blocks in C#, §12 Testing in C#**. Cite the real numbers. The anchors in
`backend-conventions.md` (`#8-cqrs-in-c` and friends) are dead links.
