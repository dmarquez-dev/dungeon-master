# Contract — ConfigureAwait in the Infrastructure layer

Frozen at Gate 2. Covers MASTER-153 / INFRA-015, severity **S5**, the only entry in this session whose
blast radius is not the App project.

- **Producer and sole consumer:** T6 — owns `src/DungeonMaster.Infrastructure/**/*.cs` (excluding
  `Migrations/` and `obj/`) and `src/DungeonMaster.Tests/Infrastructure/Async/`

T6 shares no file with any other task and has no dependency on any of them.

## The decision, taken by the user at Gate 2: APPLY

The audit entry ends: "Doing nothing is also defensible for an app this size -- the point is that the
decision should be deliberate." It is `authority: judgment`, `citation: n/a`, and severity S5 — by the
audit's own definition, an S5 deviates from no authority. The architect did not settle it; **the user did,
at Gate 2, and the answer is to apply `ConfigureAwait(false)`.**

The user's condition was to decline if best practice gives a reason not to, and to implement if the
practice merely distinguishes shipped libraries from internal ones without applying against it. The
evidence taken at Gate 2 is the second case:

- **CA2007, "When to suppress warnings"** — the rule "is intended for libraries", and suppression is
  appropriate for "projects that represent application code", because running it on application code,
  "for example, button click event handlers in a WinForms or WPF project", "is likely to lead to the
  wrong actions being taken." The hazard the guidance names is the **UI app project**. That is
  `DungeonMaster.App`, which this task does not touch — not `DungeonMaster.Infrastructure`, which it does.
  <https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2007>
- **ExecutionContext and SynchronizationContext** — "Library authors should use `ConfigureAwait(false)`
  on every await unless the code specifically needs to resume on the captured context." Infrastructure
  touches no UI state, so it does not need the captured context.
  <https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/consuming-the-task-based-asynchronous-pattern>
- CA2007's `dotnet_code_quality.CA2007.output_kind` option separates `DynamicallyLinkedLibrary` from UI
  apps, which is the documented mechanism for scoping the rule to exactly this split.

### Nothing in the build enforces this

**CA2007 is not enabled by default in .NET 10.** `AnalysisMode` is `Recommended` in this repository
(Session 1), and CA2007 is not in that set. No analyzer, no warning, and no `TreatWarningsAsErrors`
failure will catch a future `await` written without `.ConfigureAwait(false)` in these four files.

The source-scanning guard test T6 writes is therefore **the only enforcement that will exist**. T6's
criteria must not imply otherwise, and T6 must not report the build as evidence for this rule. If the
guard test is ever deleted, the convention silently stops being a convention.

## Verified facts

Run in `C:\Users\dmarq\Development\Repos\dungeon-master\.claude-worktrees\dir\2026-08-24-async-disposal-and-threading-discipline`:

- `grep -rn "ConfigureAwait" --include=*.cs src | wc -l` returns **0**. There is no `ConfigureAwait` call
  anywhere in the solution, not merely absent from Infrastructure.
- `grep -rn "await " --include=*.cs src/DungeonMaster.Infrastructure --exclude-dir=obj --exclude-dir=Migrations | wc -l`
  returns **9**, across **four** files:

  | File | Lines |
  |---|---|
  | `src/DungeonMaster.Infrastructure/DependencyInjection.cs` | 56, 58 |
  | `src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonReadRepository.cs` | 19, 21, 39, 41 |
  | `src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonWriteRepository.cs` | 17, 48 |
  | `src/DungeonMaster.Infrastructure/Persistence/UnitOfWorkFactory.cs` | 16 |

  **The audit names only the two repository files.** `DependencyInjection.cs` and `UnitOfWorkFactory.cs`
  carry three further awaits and are in scope for T6 on the same reasoning: they are library code with no
  UI state to resume onto.

- Three of the nine awaits are `await using` declarations
  (`DependencyInjection.cs:56`, `DungeonReadRepository.cs:19`, `DungeonReadRepository.cs:39`), each of the
  shape `await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);`. These carry two
  awaits on one line: the `await using` disposal and the inner `await` on the factory call.

## The rule

Every `await` expression in the scoped files carries `.ConfigureAwait(false)`.

For an `await using` declaration this means both awaits — the inner factory call and the disposal — which
requires the `ConfiguredAsyncDisposable` form. **T6 must establish the exact syntax by compiling it, and
must not take a predicted form on trust.** I have not compiled it and therefore do not state one here, and
T6 must not state a compiler error code it has not observed. If the disposal await cannot carry
`ConfigureAwait(false)` without a shape T6 judges to hurt readability, T6 applies it to the inner await
only and says so explicitly in its report, naming the line.

Nothing else in the scoped files changes. No signature, no lifetime, no behaviour. `src/DungeonMaster.App`
is untouched, so the view models' final continuations still land on the UI thread — which is the whole
point, and is the audit's own stated intent.

`docs/persistence-contract.md` exists and governs entity property types; T6 changes no property type and
no entity, so it is not in scope, but T6 must not introduce one.

## Evidence

T6's evidence is genuinely executable, unlike every App-layer task in this session:
`src/DungeonMaster.Tests` already references `src/DungeonMaster.Infrastructure`, verified by
`grep -n "ProjectReference" src/DungeonMaster.Tests/DungeonMaster.Tests.csproj`, and
`src/DungeonMaster.Tests/Infrastructure/Persistence/` already exists.

T6 nonetheless writes a **source-scanning** guard, not a behavioural one, because `ConfigureAwait` has no
observable effect a test can assert: the test host has no `SynchronizationContext`, so a continuation
resumes on the thread pool either way. The scan asserts the token is present on every `await` in the
scoped files. T6's red step is self-contained: it runs the scan against the unmodified tree, observes it
fail naming the 9 sites, then applies the change and observes it pass.
