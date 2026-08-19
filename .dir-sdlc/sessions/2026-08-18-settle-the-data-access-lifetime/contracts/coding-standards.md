# Contract: coding standards binding on every task in this session

Frozen at Gate 2. Every task in this session is bound by this file. It carries forward
`.dir-sdlc/sessions/2026-08-18-make-failure-observable/contracts/coding-standards.md` unchanged in
substance; the two things that changed are the test baseline (§9) and the list of pre-approved
multi-type files (§7).

Paths in this file are relative to the **worktree root**, which is
`C:\Users\dmarq\Development\Repos\dungeon-master\.claude-worktrees\2026-08-18-settle-the-data-access-lifetime\`.
Paths to session artifacts are given absolute and in full.

## 1. Indentation — TABS, non-negotiable

`.editorconfig` at the repository root sets `indent_style = tab` for `[*]`, `[*.cs]` and `[*.xaml]`.
`Directory.Build.props` sets `TreatWarningsAsErrors` and Session 1 raised
`dotnet_diagnostic.IDE0055.severity` to `warning`.

**A space-indented `.cs` file does not compile.** Every new file and every edited line in this
session is tab-indented. Continuation indent is one further tab.

`src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs` is
space-indented today (INFRA-006, `MASTER-134`, **not in this session**). A task that edits lines in
that file writes its new and edited lines with **tabs**, leaving the surrounding space-indented lines
alone. Reindenting the whole file is out of scope and is a review finding, not a bonus.

## 2. Line endings — CRLF, and the failure is invisible to git

`.editorconfig` declares `end_of_line = crlf` and `insert_final_newline = true`; `IDE0055` enforces
both. `.gitattributes` carries `* text=auto`, so an LF-ended file is normalised on the way into the
index and `git status` reports the tree **clean** while `dotnet build` fails with an IDE0055 error.

**Every new file must be written with CRLF line endings.** The command to run, and to quote in the
task report:

```
file <path>                      # must report "with CRLF line terminators"
```

## 3. XML documentation comments

`dm-conventions/backend/dotnet-format.md` §9:

> All public types, members, and parameters must have XML documentation comments.
>
> - Tag descriptions go on a **new line**, indented with **two tabs** from the `///` marker
> - All descriptions are complete sentences ending with a period
> - **Line breaks occur only at sentence boundaries.**

This binds every public member added by this session: `IUnitOfWork`, `IUnitOfWorkFactory`, every
member of the two relocated repository ports, `MigrateDatabaseAsync`, and every new test-visible
type. Where a member throws, document it with `<exception cref="..."/>`.

## 4. Call-site and declaration wrapping

`dotnet-format.md` §3: any method call or object construction with **2 or more arguments** wraps each
argument onto its own line. Primary-constructor parameters are always one per line regardless of
count. The §3 rule wins over §12's own contradicting sample, in production and test code alike.

Existing untouched lines are **not** reformatted. This session is not a formatting sweep and
gratuitous reflow is a review finding.

## 5. Expression bodies and lambdas

`dotnet-format.md` §7: "Everything after `=>` goes on a new line, indented one level."
`dotnet-format.md` §1: "Use descriptive names in lambda expressions — never single-letter variables."

This bites in the EF configuration files, which are the session's densest lambda code. The three
tab-indented configurations already obey it (`dungeon =>`, `floor =>`, `area =>`); match them.

## 6. Braces

`dotnet-format.md` §4: all `if`, `else`, `for`, `foreach`, `while`, `do`, `using`, `lock` bodies use
full braces, even for a single statement. No exceptions.

## 7. File organisation

`dotnet-format.md` §2: one type per file; file name matches the type name exactly; folder structure
mirrors the namespace hierarchy. Namespaces are file-scoped.

Deviations carried in from Session 2 and **still pre-approved** — a reviewer raises no finding
against them: `Result.cs` / `ResultOfT.cs`, `ICommand.cs` (two arities), `ICommandHandler.cs` (two
arities), `ValidationBehavior.cs` (two arities).

One new deviation, declared here so it is not rediscovered in review:
`src/DungeonMaster.Application/Common/Interfaces/IUnitOfWork.cs` holds **`IUnitOfWork` only**.
`IUnitOfWorkFactory` gets its own file `IUnitOfWorkFactory.cs`. There is no arity collision here, so
the ordinary one-type-per-file rule applies with no exception.

Because §2 also requires the folder to mirror the namespace, `MASTER-048`'s relocation is a **move of
both the file and the namespace**: `DungeonMaster.Application.Features.Dungeons` to
`DungeonMaster.Application.Common.Interfaces`. Moving the file while leaving the namespace behind
would satisfy CORE-029's letter and break §2, and is a blocking finding.

## 8. Test conventions

`dotnet-format.md` §12 and `backend-conventions.md` §9.

- **Framework:** xUnit + FluentAssertions + NSubstitute. Never `Assert.*`.
- **Class shape:** `public class XTests`, not sealed. `private readonly` fields for substitutes and
  the system under test, constructed in the constructor.
- **Naming:** `Method_Scenario_ExpectedResult`, keeping the `Async` suffix on the method segment
  (`CommitAsync_WithNoChanges_WritesNothing`). Repository precedent wins over §12's sample.
- **Arrangement:** literal `// Arrange`, `// Act`, `// Assert` comments, blank line before `// Act`
  and before `// Assert`.
- **One test file per source file** (`backend-conventions.md` §9).
- **Infrastructure tests** use the fixture already established in
  `src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs`
  lines 12–32 — a `SqliteConnection("Data Source=:memory:")` held open for the class lifetime,
  `EnsureCreated()`, a private `CreateContext()` factory, `IDisposable`. **Do not invent a second
  fixture.** Where a test now needs an `IDbContextFactory<AppDbContext>` rather than a context, wrap
  the existing `DbContextOptions<AppDbContext>` field in a small test-local factory as
  `contracts/persistence-ports-and-unit-of-work.md` §7 specifies, rather than replacing the fixture.

## 9. The baseline this session must not lower

Measured in the worktree on the session's base commit `9903f405dc919f782ba9048cae9623bcf1d79297`:

- `dotnet test src/DungeonMaster.sln --nologo` — **`Failed: 0, Passed: 434, Skipped: 0, Total: 434`**.
- The same command builds all five projects including both App target frameworks
  (`net10.0-windows10.0.19041.0` and `net10.0-maccatalyst`) with **0 warnings, 0 errors** under
  `TreatWarningsAsErrors`, and exits 0.

**434 is the floor, not 292.** The audit-era documents in `docs/audit/` predate Session 2 and cite
292; that number is stale and no criterion in this session may use it.

Every task ends with `dotnet build src/DungeonMaster.sln` and `dotnet test src/DungeonMaster.sln`
run for real and their output quoted in the task report — the count it started from and the count it
ended with. A task that leaves the build red, raises the warning count above zero, or loses a passing
test has failed regardless of what else it achieved. Any test deliberately replaced is named in the
report with its reason.

## 10. Decisions the conventions do not cover

`dm-conventions` is silent on the following. They are decided here so review does not re-open them.

| Question | Decision |
|---|---|
| Service lifetime vocabulary in a host with no request scope | Registrations are **`AddSingleton`** for types that hold only thread-safe collaborators, and nothing in `src/` is registered `AddScoped` by our own code after T1. See `contracts/persistence-ports-and-unit-of-work.md` §5, including the one scoped registration EF adds on our behalf that we do not control. |
| `Async` suffix on a method that is no longer asynchronous | Drop it. `AddAsync` becoming synchronous becomes `Add`, per `backend-conventions.md` §6. |
| Where a repository-shaped port lives | `Application/Common/Interfaces/` (`backend-conventions.md` §1). This session establishes that directory; it does not exist on the base commit. |
| Member ordering | No rule exists. Follow the file's existing order. New files: fields, constructor, properties, then methods. |
| `var` usage | No rule exists. Use `var` for locals with an obvious initialiser, matching every existing file. |

## 11. Escalation, not improvisation

An implementer that finds a frozen contract wrong **stops and reports a contract-change escalation
to the orchestrator**. It does not edit a file under
`C:\Users\dmarq\Development\Repos\dungeon-master\.dir-sdlc\sessions\2026-08-18-settle-the-data-access-lifetime\contracts\`,
because every other task in flight is building against the version it was handed.
