# Tests -- code quality audit

## Scope

| Path or glob | Files | Lines |
|---|---|---|
| `src/DungeonMaster.Tests/App/Accessibility/**` | 1 | 260 |
| `src/DungeonMaster.Tests/App/Theme/**` | 12 | 4,430 |
| `src/DungeonMaster.Tests/Application/Common/Map/**` | 3 | 223 |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/**` | 14 | 1,324 |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/**` | 2 | 129 |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/**` | 4 | 220 |
| `src/DungeonMaster.Tests/Domain/Entities/**` | 5 | 478 |
| `src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/**` | 4 | 484 |
| **Total** | **45** | **7,548** |

Rolled up by sub-tree: `App/` 13 files / 4,690 lines; `Application/` 23 files / 1,896 lines;
`Domain/` 5 files / 478 lines; `Infrastructure/` 4 files / 484 lines.

**Files read: 45. Files cited in at least one record: 45.** Every `.cs` file in the scope table is
cited by at least one record below; no file was read and left unaccounted for.

Not audited, with the reason for each:

- `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` -- the contract assigns all five `.csproj`
  files to `XCUT` (T6). One record here (`TEST-032`) cites a single line of it because the
  observation is about the test suite's coverage tooling rather than about the project file's own
  quality; the file-level project record belongs to T6.
- `src/DungeonMaster.Tests/bin/**` and `src/DungeonMaster.Tests/obj/**` -- build output, not source.
- There is **no generated code in this project**: no EF Core migrations, no `*.Designer.cs`, no
  `*.g.cs`, and no XAML source-generator output. The test project does not reference
  `DungeonMaster.App`, so `MauiXamlInflator=SourceGen` produces nothing here. Every one of the 45
  files is hand-written and was audited against the same standard.

## Method

- **Base commit:** `45495c1bd39a52dbc942eceab897b3c1ac553ee9`, audited in the worktree
  `dungeon-master-worktrees/2026-08-17-code-quality-audit`.
- **`dm-conventions` documents read:**
  - `backend/dotnet-format.md` -- 794 lines, **read in full**, before any style, naming, formatting
    or XML-documentation record in this document was written. Its section 12, "Testing in C#", is
    the only C# testing guidance in that repository and is cited where it applies. Its
    `testing/` documents are Vitest and Playwright and are **not** cited anywhere here.
- **Microsoft Learn pages consulted (all fetched, not recalled):**
  - https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
  - https://learn.microsoft.com/en-us/ef/core/testing/
  - https://learn.microsoft.com/en-us/ef/core/managing-schemas/ensure-created
  - https://learn.microsoft.com/en-us/dotnet/standard/base-types/compilation-and-reuse-in-regular-expressions
  - https://learn.microsoft.com/en-us/dotnet/standard/base-types/best-practices-regex
  - https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
  - https://learn.microsoft.com/en-us/dotnet/standard/base-types/character-encoding-introduction
- **Other sources consulted:** https://xunit.net/docs/running-tests-in-parallel (fetched, for the
  default collection and parallelism behaviour used in the shared-state analysis).
- **Tooling run (read-only):**
  - `dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` from the worktree.
    Result: `Failed: 0, Passed: 292, Skipped: 0, Total: 292, Duration: 2 s`. Writes went only to
    `bin/` and `obj/`. The green run is reported as a fact about the suite's current state, not as
    evidence of its quality.
  - The same command re-run with `--filter FullyQualifiedName~<namespace>` once per sub-tree, to
    obtain test-case counts rather than method counts: `App/` 155, `Application/` 85, `Domain/` 38,
    `Infrastructure/` 14, summing to the 292 of the full run.
- **Shared-state and parallelism analysis** (the conclusion is stated in `## Layer verdict`; this is
  the working behind it). xUnit v2 defaults to one collection per test class and runs different
  classes in parallel -- confirmed against the fetched xunit.net page above. Every Infrastructure
  fixture opens its own `Data Source=:memory:` connection in its constructor, so each test method
  gets a private database, and there is no file-backed SQLite anywhere in the project. The only
  static state in the project is `RepoPaths.LazyRoot`, the compiled regexes, and the never-mutated
  allow-list `HashSet`s, all of which are immutable after initialisation. No test mutates state
  another test reads, so the default parallelism is safe here.
  - `git status --porcelain` after the run: no entry under `src/`.
  - `grep`/`find` inventories of indentation characters, XML-doc presence, and the
    `src/DungeonMaster.Application` and `src/DungeonMaster.Infrastructure` type inventories, to
    establish which production types have no test at all.

## Summary

| Severity | Count |
|---|---|
| S1 | 0 |
| S2 | 4 |
| S3 | 17 |
| S4 | 9 |
| S5 | 2 |
| **Total** | 32 |

## Records

### TEST-001 -- The dispatcher, the validation pipeline and the Result type have no tests at all

```yaml
id: TEST-001
layer: TEST
severity: S2
category: testability
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:1
  - src/DungeonMaster.Application/Common/Behaviors/ValidationBehavior.cs:1
  - src/DungeonMaster.Application/Common/Results/Result.cs:1
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs:17
effort: M
risk: low
independent: true
```

**What.** `src/DungeonMaster.Tests/Application/` contains `Common/Map/` and `Features/Dungeons/`
and nothing else: there is no test anywhere in the project for `Dispatcher`, for
`ValidationBehavior`, or for `Result`. The four validator test classes invoke
`_validator.Validate(command)` directly, so they prove a rule object's rules and never that the
rule object is reached by the pipeline that is supposed to run it.

**Why it matters.** The Microsoft guidance is that tests exist to protect against regression in the
code most likely to change, and the dispatcher is the highest-risk code in the Application layer: it
resolves handlers by reflection over closed generic types and composes behaviours through `dynamic`,
so a mistake there is invisible to the compiler in a way none of the tested handlers are. Today a
validator could be deleted from dependency injection, or `ValidationBehavior` could stop short-
circuiting on failure, and all 292 tests would still pass.

**Suggested direction.** Add a small set of tests that construct a real `ServiceProvider` with one
fake command, one handler and one deliberately failing validator, and dispatch through
`IDispatcher` end to end, asserting both the success path and the validation-failure `Result`.

### TEST-002 -- Every persistence test builds its schema with EnsureCreated, so no test exercises a migration

```yaml
id: TEST-002
layer: TEST
severity: S2
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/ef/core/managing-schemas/ensure-created
locations:
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:26-27
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:24-25
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs:29-30
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:29-30
effort: S
risk: low
independent: true
```

**What.** All four Infrastructure test classes create their schema with
`context.Database.EnsureCreated()`. Production creates its schema with `db.Database.Migrate()`
(`src/DungeonMaster.Infrastructure/DependencyInjection.cs:50`). `EnsureCreated` builds the schema
from the current model, bypassing the two migrations under
`src/DungeonMaster.Infrastructure/Migrations/` entirely.

**Why it matters.** The cited page states plainly that `EnsureCreated` and Migrations "don't work
well together" and that a project intending to use Migrations should not use `EnsureCreated` to
initialize the schema. The consequence here is a specific and reachable one: a model change shipped
without its migration -- a new property, a changed column type, a renamed table -- produces a green
suite, because the tests build the schema from the model that already contains the change, and then
fails on the first real user's existing database, where the schema comes from the migration chain
that does not.

**Suggested direction.** Switch the four fixtures to `Database.Migrate()` against the SQLite
in-memory connection so the migration chain is what is under test, and consider one additional test
that asserts the model and the migrations have not diverged.

### TEST-003 -- RepoPaths accepts any ancestor with a .git entry without checking the App project is beneath it

```yaml
id: TEST-003
layer: TEST
severity: S2
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/RepoPaths.cs:51-76
  - src/DungeonMaster.Tests/App/Theme/RepoPaths.cs:19-23
  - src/DungeonMaster.Tests/App/Theme/RepoPaths.cs:8
effort: XS
risk: low
independent: true
```

**What.** `FindRoot` walks up from `AppContext.BaseDirectory` and returns the first directory
containing a `.git` file or directory. It never checks that `src/DungeonMaster.App` exists under
what it returned. `AppRoot` is then composed from that result and handed straight to
`Directory.EnumerateFiles`. The result is cached in a static `Lazy<string>`, which caches the
exception too if the walk runs off the top of the drive.

**Why it matters.** I believe this makes the entire twelve-class theme sub-suite dependent on an
ambient property of the machine it runs on rather than on anything in the repository. Two layouts
appear to break it: a checkout with no `.git` at all -- a source archive, or a CI job that fetches a
tarball rather than cloning -- where the walk throws `InvalidOperationException` from a static
initialiser and every theme fact fails with a message about a missing repository root; and a
checkout nested inside an outer repository, where the walk can stop at the wrong root and
`EnumerateFiles` throws `DirectoryNotFoundException` from within a test body. In this session's
linked worktree it resolves correctly -- the `.git`-as-a-file case is handled and commented at
`RepoPaths.cs:57-61`, and the 292-test run confirms it -- so the defect I am describing is the
absence of a check, not a failure observed today.

**Suggested direction.** Make the loop's acceptance condition "this directory has a `.git` entry
**and** contains `src/DungeonMaster.App`", keep walking otherwise, and have the failure message name
the directory it started from and what it was looking for.

### TEST-004 -- Eleven of the thirteen App files assert over XAML as text; a twelfth parses it as XML

```yaml
id: TEST-004
layer: TEST
severity: S2
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:18-56
  - src/DungeonMaster.Tests/App/Theme/ThemeDictionaryTests.cs:15-18
  - src/DungeonMaster.Tests/App/Theme/ThemeTokenParityTests.cs:17-35
  - src/DungeonMaster.Tests/App/Theme/ImplicitStyleTests.cs:18-26
  - src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs:31-34
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:378
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:492-494
effort: L
risk: medium
independent: true
```

**What.** Eleven of the thirteen files under `App/` read `.xaml` files as strings and match regular
expressions against them. The twelfth, `TokenTypeMatchesTargetPropertyTests`, reads the same files
with `XDocument.Parse`, walks
`Descendants()`, reads attributes by `XName`, and gets element identity, namespace qualification,
ancestor context and accurate line numbers from `LoadOptions.SetLineInfo` for free. The thirteenth,
`App/Theme/RepoPaths.cs`, is a path helper that makes no XAML assertion at all, so the comparison is
between eleven regex classes and one XML one.

**Why it matters.** I believe the regex approach produces a class of silent false pass that the XML
approach cannot. `ControlTag` at `SemanticDescriptionTests.cs:33` matches `[^>]*?` for a tag's
attributes, so an attribute value containing `>` truncates the tag and the control is never
examined; `StylePattern` at `ImplicitStyleTests.cs:20` closes on the first `</Style>` with
`.*?`, so a nested style would mis-terminate the body and its setters would be attributed to the
wrong target; `SemanticDescriptionTests` only sees double-quoted attributes, so a single-quoted
`Text='...'` is invisible. None of those failures announce themselves -- the scan simply reaches
fewer controls and reports green. The suite's own authors clearly know this: nine classes carry
explicit anti-vacuity assertions whose only purpose is to detect a scan that has stopped reaching
anything, which is a symptom of the parsing strategy rather than a fix for it. That
`TokenTypeMatchesTargetPropertyTests` does the harder job with `System.Xml.Linq` in fewer lines
than several of the regex classes is the strongest evidence available that the alternative is
affordable.

**Suggested direction.** Move the shared scan in `ThemeSource` onto `XDocument`, expose keyed
elements and attributes as objects, and let each fact query that model. The anti-vacuity guards
stay useful but stop being the only thing standing between the suite and silence.

### TEST-005 -- Compiled Regex objects are constructed inside per-item loops rather than reused

```yaml
id: TEST-005
layer: TEST
severity: S3
category: performance
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/base-types/compilation-and-reuse-in-regular-expressions
locations:
  - src/DungeonMaster.Tests/App/Theme/ThemeDictionaryTests.cs:304-310
  - src/DungeonMaster.Tests/App/Theme/ImplicitStyleTests.cs:214-217
  - src/DungeonMaster.Tests/App/Theme/BindablePropertyDefaultsMatchTokensTests.cs:84-86
  - src/DungeonMaster.Tests/App/Theme/BindablePropertyDefaultsMatchTokensTests.cs:129-131
effort: S
risk: low
independent: true
```

**What.** `NoOrphanedStyleKeys` constructs two `new Regex(..., RegexOptions.Compiled)` instances on
every iteration of its per-style loop. `ImplicitStyleTests.Names` builds an interpolated pattern and
passes it to the static `Regex.Matches` on every call. The two `BindablePropertyDefaults` theories
construct a compiled `Regex` per theory case.

**Why it matters.** The cited page states that `RegexOptions.Compiled` raises construction cost in
exchange for lower match cost, and that "creating and rapidly destroying numerous `Regex` objects is
an expensive process". Constructing a compiled regex inside a loop pays the JIT cost per iteration
and collects none of the benefit, which is the exact inversion of what the option is for. The same
page notes the static-method cache holds only 15 patterns by default, so `ImplicitStyleTests.Names`
is also at risk of cache eviction if the interpolated element names ever grow.

**Suggested direction.** Hoist the two loop-constructed patterns out of the `foreach` (or replace
the per-key regex with a single pass that collects all `{StaticResource ...}` keys into a set once
and tests membership), and drop `RegexOptions.Compiled` from any pattern that is genuinely
short-lived.

### TEST-006 -- The accessibility scan truncates supplementary-plane character entities with a char cast

```yaml
id: TEST-006
layer: TEST
severity: S3
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/base-types/character-encoding-introduction
locations:
  - src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs:222-235
  - src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs:243-259
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:205-220
effort: XS
risk: low
independent: true
```

**What.** `SemanticDescriptionTests.Decode` converts a `&#xNNNN;` entity with
`(char)int.Parse(...)`. `IsIconOnly` then rejects anything whose `Length != 1`.
`IconFontCoverageTests.Decode`, doing the same job three files away, uses
`char.ConvertFromUtf32`.

**Why it matters.** The cited page explains that a code point above `U+FFFF` requires a surrogate
pair and cannot be held in one `char`. An entity such as `&#x1F6E1;` therefore either wraps to an
unrelated BMP character or, at `int.Parse` boundaries, is silently mis-decoded; either way the
control is measured against the wrong codepoint, and the surrogate-pair form is rejected outright by
the `Length != 1` test. The consequence is an icon-only control in the supplementary plane passing
the accessibility ratchet without a `SemanticProperties.Description`, which is precisely the state
the class exists to prevent. That the sibling class already does this correctly makes the
divergence, rather than the technique, the finding.

**Suggested direction.** Replace the cast with `char.ConvertFromUtf32`, change `IsIconOnly` to
enumerate runes and require exactly one, and consider lifting the single correct `Decode` into a
shared helper so the two cannot drift again.

### TEST-007 -- The four validator test classes are identical except for the command type

```yaml
id: TEST-007
layer: TEST
severity: S3
category: duplication
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Duplicated Code"
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs:6-55
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/CreateDungeonCommandValidatorTests.cs:6-55
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateAreaCommandValidatorTests.cs:6-55
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateDungeonCommandValidatorTests.cs:6-55
effort: S
risk: low
independent: true
```

**What.** All four validator test classes are 55 lines with the same three tests -- valid command
passes, a three-case `[Theory]` on empty name, and a 101-character name -- differing only in the
command record they construct. The four validators they test are themselves identical: a single
`RuleFor(command => command.Name).NotEmpty().MaximumLength(100)`.

**Why it matters.** Fowler's treatment of duplicated code applies directly: the cost is not the
lines, it is that the next rule added to one validator must be remembered in four places, and that
the four files hide rather than expose the fact that four commands share one rule. A reader
comparing the four cannot tell whether the sameness is deliberate or accidental.

**Suggested direction.** Collapse the four into one generic abstract base parameterised by command
type with an abstract factory for "command with this name", or a single `[Theory]` over the four
validators, so a new rule has one place to be tested and a divergence between the four validators
becomes visible.

### TEST-008 -- Sixteen handler test classes repeat an identical fixture constructor and an identical not-found test

```yaml
id: TEST-008
layer: TEST
severity: S3
category: duplication
authority: conventions
citation: backend/dotnet-format.md#12. Testing in C#
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs:12-19
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddFloorCommandHandlerTests.cs:12-19
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddPlacedObjectCommandHandlerTests.cs:17-27
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/CreateDungeonCommandHandlerTests.cs:14-24
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/DeleteDungeonCommandHandlerTests.cs:13-23
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemoveAreaCommandHandlerTests.cs:12-19
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemoveFloorCommandHandlerTests.cs:12-19
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemovePlacedObjectCommandHandlerTests.cs:17-27
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/ReorderFloorCommandHandlerTests.cs:12-19
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateAreaCommandHandlerTests.cs:12-19
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateAreaOutlineCommandHandlerTests.cs:13-20
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateDungeonCommandHandlerTests.cs:12-19
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateFloorCommandHandlerTests.cs:12-19
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs:17-27
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonDetailQueryHandlerTests.cs:10-17
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonsQueryHandlerTests.cs:13-23
effort: M
risk: low
independent: true
```

**What.** Sixteen handler test classes declare the same two readonly fields and the same
constructor body -- `Substitute.For<IDungeonWriteRepository>()` (or `IDungeonReadRepository`) and
`new XHandler(_writeRepository)` -- and twelve of the fourteen command classes then carry a
not-found test whose body is the same four statements with a different command record. Twelve is
the count on the behavioural reading used throughout this record: a test that asserts
`ErrorCategory.NotFound`. Only nine of those twelve are named `HandleAsync_WithNonExistent...`;
the other three state the same case under a different name. `CreateDungeonCommandHandlerTests` and
`DeleteDungeonCommandHandlerTests` are the two classes with no not-found test.

**Why it matters.** `dotnet-format.md` section 12 prescribes exactly this constructor shape, so the
duplication is convention-compliant and not itself a deviation -- which is why this is recorded once
rather than sixteen times. The cost is that the not-found path, which is the single most repeated
behaviour in the layer, has twelve independent statements of the same contract; changing how a
missing aggregate is reported means editing twelve files, and a handler that quietly stops returning
`ErrorCategory.NotFound` fails twelve tests that all say the same thing.

**Suggested direction.** Keep the per-class constructor the convention asks for, but lift the
not-found case into one parameterised test that dispatches each command type through a repository
stubbed to return `null` and asserts `ErrorCategory.NotFound` once.

### TEST-009 -- The SQLite in-memory fixture is copy-pasted verbatim into four Infrastructure test classes

```yaml
id: TEST-009
layer: TEST
severity: S3
category: duplication
authority: practice
citation: Meszaros, xUnit Test Patterns, ch. 19 "Fixture Setup Patterns"
locations:
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:17-35
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:12-30
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:14-32
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs:17-35
effort: S
risk: low
independent: false
blocked-by:
  - TEST-002
```

**What.** Each of the four classes declares the same `_connection` and `_options` fields, the same
constructor opening a `Data Source=:memory:` connection and calling `EnsureCreated`, the same
one-line `Dispose`, and the same `CreateContext()` helper. Nothing is shared: there is no base
class, no `IClassFixture`, no `ICollectionFixture`.

**Why it matters.** The fixture-setup guidance is that a fixture shared by several test classes
belongs in one place; here the same nineteen lines exist four times, so the change proposed in
`TEST-002` -- moving from `EnsureCreated` to `Migrate` -- has to be made four times and can be made
inconsistently. It also means the four classes cannot be given a shared seeding vocabulary without
adding a fifth copy of the plumbing.

**Suggested direction.** Extract a `SqliteFixture` implementing `IDisposable` and have the four
classes take it via `IClassFixture<SqliteFixture>`, or a shared abstract base if per-test isolation
of the database is wanted (it currently is -- see the layer verdict). Land it with `TEST-002` so the
schema strategy is changed once.

### TEST-010 -- Four tests assert Received(1) on a save that happens on a path where nothing changed

```yaml
id: TEST-010
layer: TEST
severity: S3
category: testability
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemovePlacedObjectCommandHandlerTests.cs:115-117
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemovePlacedObjectCommandHandlerTests.cs:147-149
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs:123-125
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs:165-167
effort: S
risk: low
independent: true
```

**What.** `HandleAsync_FloorNotFound_ReturnsSuccessWithNoChange` and
`HandleAsync_ObjectNotFound_ReturnsSuccessWithNoChange`, in both the Remove and Update placed-object
classes, assert `await _writeRepository.Received(1).UpdateAsync(dungeon, Arg.Any<CancellationToken>())`
on paths whose own names say the aggregate was not modified.

**Why it matters.** The cited page's guidance is to "focus on verifying the behavior over the
implementation". The observable behaviour on these paths is "returns success and changes nothing";
whether the handler nonetheless issues a redundant `UpdateAsync` is an implementation choice, and
these four assertions promote it to a contract. A correct optimisation -- skipping the save when the
domain call reported no change -- would break four tests without changing any behaviour a caller can
see. This is the clearest instance in the suite of a test coupled to implementation.

**Suggested direction.** Drop the `Received(1)` assertion from the two no-change tests in each class
and keep it on the paths where the save is the point. If "always persists" really is the intended
contract, say so in the test name rather than in a call-count.

### TEST-011 -- Task-returning substitutes are given explicit Returns(Task.CompletedTask) stubs

```yaml
id: TEST-011
layer: TEST
severity: S3
category: testability
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs:30-32
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddFloorCommandHandlerTests.cs:27
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/CreateDungeonCommandHandlerTests.cs:34-38
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/DeleteDungeonCommandHandlerTests.cs:34-38
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemoveAreaCommandHandlerTests.cs:31-33
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/ReorderFloorCommandHandlerTests.cs:31-33
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateAreaOutlineCommandHandlerTests.cs:32-34
effort: XS
risk: low
independent: true
```

**What.** Several handler tests configure `_writeRepository.UpdateAsync(...)` or `AddAsync(...)`
with `.Returns(Task.CompletedTask)` even though the return value is never inspected. The four
placed-object and query classes do not do this, so the setup is inconsistent as well as inert.

**Why it matters.** I could not confirm against NSubstitute's published documentation that a
`Task`-returning member is auto-substituted with a completed task, so I state this as belief rather
than fact: my reading is that these stubs configure a return NSubstitute would supply anyway, which
makes them arrange noise that a reader has to check before concluding it does nothing. Whether or
not that belief holds, the inconsistency between the classes is visible in the source and costs a
reader time on every one of them.

**Suggested direction.** Confirm the framework's default for `Task`-returning members against the
version in use, then either remove the redundant stubs everywhere or add them everywhere, so the
pattern reads as a decision.

### TEST-012 -- AreaPersistenceDiagnosticTests is a committed debugging session with two subsumed tests

```yaml
id: TEST-012
layer: TEST
severity: S3
category: dead-code
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:10-15
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:50-89
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:91-137
effort: XS
risk: low
independent: true
```

**What.** The class is named `AreaPersistenceDiagnosticTests` and its summary says it was "added
while investigating a report that both operations appeared not to persist from the designer UI".
`RemoveArea_WithEmptyOutline_PersistsAcrossReload` seeds through the same `SeedAsync` as
`RemoveArea_PersistsAcrossReload` -- which always produces an area with an empty outline -- and
differs from it only by an extra assertion at line 79 that the outline is empty. Similarly
`UpdateArea_SecondRenamePersistsAcrossReload` performs the first rename and then a second, so it
strictly contains `UpdateArea_RenamePersistsAcrossReload`.

**Why it matters.** I believe the name and the summary together mark this as investigation scaffold
that was never converted into permanent coverage, and the two subsumed pairs support that reading:
someone bisecting a persistence bug wrote the narrower case, then the wider one, and committed
both. The cost is a class whose title tells a future reader it is provisional -- so nobody will
maintain it -- while it nonetheless holds the only round-trip coverage of area removal and rename
that exists. That is the worst of both: real coverage wearing a label that invites deletion.

**Suggested direction.** Decide what it is. If the round-trip coverage is wanted, rename to
`AreaPersistenceTests`, drop the two subsumed tests, and rewrite the summary as a statement of what
is guaranteed rather than of what was being investigated. If it is not wanted, delete it and check
that nothing else covers area removal (nothing does today).

### TEST-013 -- Five geometry facts and five snap-zone facts differ only in their data

```yaml
id: TEST-013
layer: TEST
severity: S3
category: duplication
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:300-324
  - src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:428-453
  - src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:466-492
  - src/DungeonMaster.Tests/Application/Common/Map/SnapZoneCalculatorTests.cs:11-69
  - src/DungeonMaster.Tests/Application/Common/Map/TileCoordinateHelperTests.cs:11-22
effort: S
risk: low
independent: true
```

**What.** `NoBareSpacingValues`, `NoBareMarginValues` and `NoBareStrokeValues` in
`GeometryScaleTests` are structurally identical: `Scan` over XAML, `Scan` over C#, filter out zero,
`Describe`, order, assert empty. They vary only by pattern list, allow-list and message.
Separately, `SnapZoneCalculatorTests` states five `[Fact]`s that each call
`SnapZoneCalculator.ToTile` with a different pair of floats and assert a different `TilePoint`,
while `TileCoordinateHelperTests` next to it expresses the same shape as two `[Theory]`
declarations with `[InlineData]`.

**Why it matters.** The cited page's "Avoid multiple Act tasks" section recommends parameterised
tests precisely for this shape, and the .NET guidance on avoiding logic in tests points the same
way. The cost in `GeometryScaleTests` is that adding a sixth scale means copying twenty-five lines
and remembering all five steps; the cost in `SnapZoneCalculatorTests` is five test methods where a
five-row `[InlineData]` table would show the boundary progression -- mid-tile, origin, exact
boundary, negative partial, negative whole, in the order the file states them -- as one readable
set. That the file beside it already
does this makes the inconsistency the finding.

**Suggested direction.** Give `GeometryScaleTests` a single `[Theory]` over
`(name, xamlPatterns, cSharpPatterns, allowList, message)` for the three uniform scales, leaving
padding and radius as facts because they carry extra filtering. Convert `SnapZoneCalculatorTests`
to a `[Theory]` matching its neighbour.

### TEST-014 -- MAUI property types and the conversion matrix are hand-transcribed into the test at a fixed date

```yaml
id: TEST-014
layer: TEST
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:72-81
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:92-137
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:170-180
effort: M
risk: medium
independent: true
```

**What.** Three tables hold facts about `Microsoft.Maui.Controls`: which element-plus-property pairs
have which CLR type, which property names have an element-independent type, and which conversions
`BindableProperty.TryConvert` performs. Their documentation states they were "read off
Microsoft.Maui.Controls.dll 10.0.41 by reflection on 2026-08-07" and measured by probing `SetValue`.

**Why it matters.** I believe this is the most valuable guard in the App sub-suite -- it is the one
that would have caught the `Button.CornerRadius` defect its summary describes -- and also the one
most likely to become quietly wrong. The tables are a snapshot of a third-party assembly held in
source with nothing tying them to that assembly; a MAUI upgrade that widens a conversion or retypes
a property leaves the tables stating the old truth, and the failure mode is a false pass, not a
compile error. `EveryDmTokenBindingTargetsAPropertyOfAKnownType` protects the coverage of the tables
but not their accuracy.

**Suggested direction.** Since the test project does not reference the App project, it cannot
reflect over MAUI directly today. Consider generating the tables from reflection in a small tool
run against the resolved `Microsoft.Maui.Controls` assembly and committing the output, so a version
bump produces a visible diff; failing that, pin the MAUI version the tables were read from as a
constant in the file and assert it against the resolved package version.

### TEST-015 -- A TrueType cmap parser lives in a test class and three of its four subtable branches are unexercised

```yaml
id: TEST-015
layer: TEST
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:276-320
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:365-379
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:452-482
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:484-515
effort: M
risk: medium
independent: true
```

**What.** `IconFontCoverageTests` contains a nested `TrueTypeCmap` class of roughly 300 lines that
parses a font's table directory and four `cmap` subtable formats (0, 4, 6, 12) out of a byte array.
The only font it is ever given is `rpgawesome-webfont.ttf`, which exercises one of those four
formats; `TheIconFontCmapParsesToPrivateUseGlyphs` is the sole check on the parser and it is a smoke
test of the whole pipeline rather than of any format.

**Why it matters.** I believe this inverts the usual relationship: the most intricate code in the
test project is the code with the least coverage, and a defect in it produces a wrong answer rather
than an error. Format 4 is the branch that runs, and its segment arithmetic -- `endCodes`,
`startCodes`, `deltas`, `rangeOffsets` at lines 390-393 -- has several off-by-one opportunities that
would return a plausible but wrong codepoint set, which the icon coverage fact would then use as
ground truth. The other three branches are dead in practice, so they carry cost without protection.

**Suggested direction.** Either narrow the parser to format 4 with an explicit failure on anything
else, so the code that exists is the code that runs; or keep the four formats and add byte-array
fixtures exercising each, at which point the parser deserves to be a named type of its own rather
than a nested class in a fact about icons.

### TEST-016 -- Assertions live inside shared scan helpers, so failures are reported against the helper

```yaml
id: TEST-016
layer: TEST
severity: S3
category: testability
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:86-88
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:143-146
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:176-179
  - src/DungeonMaster.Tests/App/Theme/ThemeHygieneTests.cs:197-200
  - src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:640-643
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:354-356
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:437-441
effort: M
risk: medium
independent: true
```

**What.** `ThemeSource.ThemeFiles`, `ThemeSource.DoubleToken`, `ThemeSource.Int32Token`,
`ThemeHygieneTests.Offenders`, `GeometryScaleTests.Scan`, `TokenTypeMatchesTargetPropertyTests
.XamlFiles` and `.AppPropertyTypes` all call `.Should()` from inside a non-test helper that other
tests use to arrange their subject.

**Why it matters.** These are the anti-vacuity guards, and I think their intent is sound -- a scan
that reaches nothing must be a failure, not a pass. My concern is where the assertion sits. Because
the helper is called during Arrange, a broken scan reports the helper's `because` string against
whichever test happened to call it first, and the same underlying fault surfaces as an apparently
unrelated failure in five or six different tests. A reader has to work out which of them is the
cause. It also means no single test owns "the scan reaches files", so it cannot be named, skipped or
reasoned about on its own.

**Suggested direction.** Promote each anti-vacuity guard to its own `[Fact]` -- one per scan, named
for what it guards -- and let the helpers return their result without asserting. The protection is
identical and the failure names itself.

### TEST-017 -- Every test in the type-matching class recomputes the full disk scan from scratch

```yaml
id: TEST-017
layer: TEST
severity: S3
category: performance
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:212-214
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:254
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:288
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:323
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:82
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:154
effort: S
risk: low
independent: true
```

**What.** All four tests in `TokenTypeMatchesTargetPropertyTests` call `Bindings()`, which reads and
`XDocument.Parse`s every `.xaml` file in the App project and regex-scans every `.cs` file, from
disk, each time. Both tests in `IconFontCoverageTests` call `IconFontCodepoints()`, which re-reads
and re-parses the TTF. Nothing is cached across tests, and neither class uses `IClassFixture`.

**Why it matters.** Microsoft's characteristics of a good unit test list "Fast" first, and the whole
suite currently runs in two seconds, so this is not a problem today -- I raise it as a structural
observation rather than a measured cost. What I believe does matter is that the repetition is
invisible: the scan is expressed as a private static method, so it reads like a cheap accessor, and
a sixth or seventh fact added to either class silently multiplies the I/O again.

**Suggested direction.** Move the scan behind an `IClassFixture` (or a `Lazy<T>` static, given the
inputs are immutable within a run) so each class parses the tree once. This pairs naturally with
`TEST-016`, since a fixture is also the natural home for the anti-vacuity assertions.

### TEST-018 -- The two query-handler tests assert only that the stubbed DTO came back

```yaml
id: TEST-018
layer: TEST
severity: S3
category: testability
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonDetailQueryHandlerTests.cs:19-38
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonsQueryHandlerTests.cs:28-52
effort: S
risk: low
independent: true
```

**What.** `HandleAsync_WithExistingId_ReturnsDtoFromRepository` stubs
`_readRepository.GetDetailAsync` to return a `DungeonDetailDto` it constructed, then asserts that
the returned object's `Id` and `Name` equal the values it put in. `HandleAsync_WithDungeons_
ReturnsSummaries` does the same with a two-element list.

**Why it matters.** The cited page's distinction between a stub and a mock is the relevant one: here
the substitute is a stub supplying data, and the assertions are against that data, so the test can
only fail if the handler transforms its input -- which it does not. The tests are true by
construction and will remain true after any refactor of the handler that preserves pass-through,
which means they neither document a behaviour nor guard one. The four assertions on
`result[0].Name`, `result[1].Name` and `result[1].Description` cost more to read than they protect.

**Suggested direction.** Reduce each to one assertion that the repository result is returned
unchanged (`result.Should().BeSameAs(dto)` for the detail case), or, if the handler is expected to
grow projection logic, test that logic when it arrives rather than pinning the current identity
mapping in four places.

### TEST-019 -- No test in the suite passes a cancellable or cancelled CancellationToken

```yaml
id: TEST-019
layer: TEST
severity: S3
category: async
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs:37
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateFloorCommandHandlerTests.cs:37
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonDetailQueryHandlerTests.cs:32
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:42
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs:44
effort: S
risk: low
independent: true
```

**What.** Every asynchronous call site in the suite passes `CancellationToken.None`, and every
NSubstitute setup matches the token with `Arg.Any<CancellationToken>()`. No test constructs a
`CancellationTokenSource`, cancels one, or asserts that a token reaches the repository.

**Why it matters.** I believe this leaves an entire dimension of the async contract unstated. Every
handler and repository method takes a `CancellationToken`; nothing proves any of them forwards it.
A handler that dropped the token and called `LoadAsync(id, default)` would pass all 292 tests, and
the symptom in the running application -- work continuing after a page is navigated away from --
is exactly the kind of thing a suite is supposed to catch and a manual pass is not.

**Suggested direction.** Add one test per repository-facing handler shape that stubs with
`Arg.Is<CancellationToken>(token => token == expected)` and asserts the call was received, plus one
Infrastructure test that cancels a token before a repository call and asserts
`OperationCanceledException`.

### TEST-020 -- Domain coverage is happy paths and unknown-id no-ops, with no constructor or boundary cases

```yaml
id: TEST-020
layer: TEST
severity: S3
category: testability
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:8-18
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:58-72
  - src/DungeonMaster.Tests/Domain/Entities/Area/AreaOutlineTests.cs:34-59
  - src/DungeonMaster.Tests/Domain/Entities/Floor/FloorObjectMethodsTests.cs:17-33
  - src/DungeonMaster.Tests/Domain/Entities/Floor/PlacedObjectTests.cs:74-90
effort: M
risk: low
independent: true
```

**What.** The five Domain classes cover the success path of each operation and, for most, the
unknown-id no-op. `PlacedObjectTests` is the exception: it has two `[Theory]` blocks covering `NaN`,
both infinities, and values below and above the scale range. No equivalent exists elsewhere -- there
is no test for `new Dungeon(null!)` or an empty name, no test of `ReorderFloor` with an index of
zero, a negative index or one past the end, no test of `AddFloor` on an aggregate at any boundary,
and no test that `UpdateAreaOutline` rejects or accepts a degenerate one- or two-segment outline.

**Why it matters.** I believe `PlacedObjectTests` shows the author knows how to write these -- its
invalid-scale theories are the best boundary coverage in the project -- which makes the absence
elsewhere look like an omission rather than a decision. The floor renumbering in `RemoveFloor` and
`ReorderFloor` is arithmetic over an ordered collection, which is the classic home of off-by-one
defects, and it is currently covered by exactly one three-floor happy-path case each.

**Suggested direction.** Extend `DungeonAggregateTests` with `[Theory]` blocks over out-of-range
reorder targets and over the empty and single-element floor collections, and state explicitly (in a
test, not a comment) what the aggregate does with a null or blank name.

### TEST-021 -- App facts assert against the lexical form of application C#, not its behaviour

```yaml
id: TEST-021
layer: TEST
severity: S3
category: testability
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/BindablePropertyDefaultsMatchTokensTests.cs:84-88
  - src/DungeonMaster.Tests/App/Theme/BindablePropertyDefaultsMatchTokensTests.cs:129-133
  - src/DungeonMaster.Tests/App/Theme/DmThemeKeysResolveTests.cs:13-16
  - src/DungeonMaster.Tests/App/Theme/DmThemeKeysResolveTests.cs:136-141
  - src/DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs:188-194
effort: L
risk: high
independent: true
```

**What.** Several facts match regular expressions against the text of application C#.
`BindableDefault_EqualsTheTokenItMirrors` requires the literal sequence
`nameof(X), typeof(double), typeof(Y), <number>`. `ResolveCallPattern` requires
`Resolve<T>("DmKey")` written inline. `NoEagerDmThemeRead` matches a three-alternative pattern
against static declarations. `AppBindablePropertyPattern` requires
`BindableProperty.Create(nameof(...), typeof(...), typeof(...)`.

**Why it matters.** This is the sharpest form of the behaviour-versus-implementation problem in this
suite, and I state it as belief because whether it is acceptable depends on what these facts are
for. My reading: several behaviour-preserving edits break them. Extracting the bindable property's
type to an alias, wrapping a `Resolve<T>` call in a local helper, or introducing a `const double`
for a default all change nothing a user or the runtime can observe, and all cause the pattern to
stop matching. Critically, most of them fail *open* -- `ResolveCalls` and `AppPropertyTypes` carry
`NotBeEmpty` guards, but `BindableDefault_EqualsTheTokenItMirrors` fails closed with a clear message
while `NoEagerDmThemeRead` simply reports no offenders. So a refactor either produces a red test for
a green change, or silently retires the guard.

**Suggested direction.** Where the goal is a value (a default matching a token), a Roslyn-based
check or a small reflection probe over the built assembly would read the real declaration rather
than its spelling. Where the goal is a prohibition (`NoEagerDmThemeRead`), accept that it is a
lint rule and consider expressing it as a Roslyn analyzer, which is what it is.

### TEST-022 -- Arrange/Act/Assert structure is used in one sub-tree and absent from three

```yaml
id: TEST-022
layer: TEST
severity: S4
category: style
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs:23-43
  - src/DungeonMaster.Tests/Application/Common/Map/MapDrawingSessionTests.cs:10-17
  - src/DungeonMaster.Tests/Application/Common/Map/TileCoordinateHelperTests.cs:17-22
  - src/DungeonMaster.Tests/Domain/Entities/Area/AreaOutlineTests.cs:10-32
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:34-49
effort: S
risk: low
independent: true
```

**What.** The sixteen handler test classes carry explicit `// Arrange`, `// Act`, `// Assert`
comments and blank-line separation. `Application/Common/Map/`, `Domain/` and `Infrastructure/` carry
neither: `MapDrawingSessionTests.PlaceVertex_FirstVertex_ReturnsAdded` is six consecutive statements
with no separation at all, and `TileCoordinateHelperTests` puts the Act and the first Assert on
adjacent lines.

**Why it matters.** The cited page's "Arrange your tests" section is explicit that separating the
three phases "clearly highlights the dependencies required to call your code, how your code is
called, and what you're trying to assert", and that "readability is one of the most important
aspects when writing a unit test". The concrete cost here is a reader who has learned the convention from the handler tests and
then loses it in three of the four sub-trees; in a run of six undifferentiated statements it takes a
second read to find which line is the Act.

**Suggested direction.** Apply the same convention across all four sub-trees. At minimum, insert
blank lines between the phases even where the comments are judged noise -- the separation carries
most of the benefit.

### TEST-023 -- Single-letter lambda parameters appear in thirteen places

```yaml
id: TEST-023
layer: TEST
severity: S4
category: naming
authority: conventions
citation: backend/dotnet-format.md#Lambda Variable Names
locations:
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:29
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:43
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:68
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:51
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:52
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs:37
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs:52
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/CreateDungeonCommandValidatorTests.cs:37
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/CreateDungeonCommandValidatorTests.cs:52
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateAreaCommandValidatorTests.cs:37
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateAreaCommandValidatorTests.cs:52
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateDungeonCommandValidatorTests.cs:37
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateDungeonCommandValidatorTests.cs:52
effort: XS
risk: low
independent: true
```

**What.** `DungeonAggregateTests` uses `f` for a floor in three `Select`/`OrderBy` calls,
`DungeonReadRepositoryTests` uses `s` for a summary in two `First` calls, and all four validator
test classes use `e` for a validation failure in `ContainSingle` -- twice each, at lines 37 and 52,
which is eight of the thirteen sites. The rest of the project uses
descriptive lambda names throughout -- `dungeonFloor`, `placedObject`, `character`, `command` --
including in the same files.

**Why it matters.** `dotnet-format.md` states the rule without qualification: "Use descriptive names
in lambda expressions -- never single-letter variables", with `users.Where(u => u.IsActive)` given
as the incorrect form. Thirteen sites deviate. With no `.editorconfig` and no analyzers in the
repository, nothing but review enforces this, so the count will grow.

**Suggested direction.** Rename to `floor`, `summary` and `failure` respectively. Consider whether
this is one of the rules worth encoding once an `.editorconfig` exists (T6 owns that question).

### TEST-024 -- ObjectScaleTests is the only file in the project indented with tabs

```yaml
id: TEST-024
layer: TEST
severity: S4
category: style
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/Domain/Entities/Floor/ObjectScaleTests.cs:8-28
effort: XS
risk: low
independent: true
```

**What.** A scan for leading tab characters across the 45 files returns exactly one:
`Domain/Entities/Floor/ObjectScaleTests.cs`. The other 44 use four spaces.

**Why it matters.** I state this as belief because the repository has no `.editorconfig` and
`dotnet-format.md` states no *dedicated* indentation-character rule. It is not silent on the
question, though, and the signal it does give points the other way: every indentation instruction it
gives is denominated in tabs -- section 3 measures declaration layout in "+1 tab" steps
(lines 240-247), section 6's layout table repeats that in every row (lines 286-291), and section 9
requires XML tag descriptions "indented with **two tabs** from the `///` marker" (line 375) -- and
299 of the document's sample lines are tab-led. So `ObjectScaleTests.cs` is the outlier within its
own project while being the one file that matches the convention document's only signal, and the
other 44 are the outliers against that signal. My belief is that the inconsistency is the finding
either way: a mixed-indentation project produces spurious whitespace diffs and inconsistent
rendering, and nothing in the repository *decides* the question, which is what would have to happen
first.

**Suggested direction.** Do not convert this file on the strength of majority alone -- the majority
runs against `dotnet-format.md`'s tab-denominated instructions, so converting it to spaces would
move the project further from its own convention document, not closer. Settle the question first:
which indentation character this repository uses, and where that is written down. T6 owns the absent
`.editorconfig` and is the right place to decide it; whichever character wins, one file or 44 then
change to match.

### TEST-025 -- XML documentation is present on 21 of 45 files and absent from 24

```yaml
id: TEST-025
layer: TEST
severity: S4
category: docs
authority: conventions
citation: backend/dotnet-format.md#9. XML Documentation Comments
locations:
  - src/DungeonMaster.Tests/Domain/Entities/Area/AreaOutlineTests.cs:8
  - src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs:6
  - src/DungeonMaster.Tests/Domain/Entities/Floor/PlacedObjectTests.cs:7
  - src/DungeonMaster.Tests/Application/Common/Map/MapDrawingSessionTests.cs:7
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs:10
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/CreateDungeonCommandValidatorTests.cs:6
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:12
effort: M
risk: low
independent: true
```

**What.** All thirteen `App/` files, six `Application/` files and two `Infrastructure/` files carry
XML documentation on their public types and members. The remaining 24 -- all five `Domain/` files,
all three `Common/Map/` files, all four validator classes, nine of the fourteen command-handler
classes, one query class and two `Infrastructure/` classes -- carry none, not even a type summary.
Every one of these is a `public` type with `public` members.

**Why it matters.** `dotnet-format.md` section 9 opens with "All public types, members, and
parameters must have XML documentation comments", without exempting test code. The split is not
random -- it tracks which sub-tree the file is in -- so the effect is that a reader learns to expect
documentation in `App/` and stops looking for it elsewhere, and a maintainer adding a file has no
signal about which side of the line their file falls on.

**Suggested direction.** Decide whether the rule applies to test classes and record the decision.
If it does, the gap is 24 files of type summaries plus per-method summaries; if it does not, note
the exemption in `dm-conventions` and consider trimming the 21 that have them, since the `App/`
documentation is genuinely load-bearing (much of it explains why a fact exists) and should survive
either way.

### TEST-026 -- Four non-sealed test classes implement IDisposable with no Dispose(bool) overload

```yaml
id: TEST-026
layer: TEST
severity: S4
category: disposal
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
locations:
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:15
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:33
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:28
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs:30
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs:33
effort: XS
risk: low
independent: false
blocked-by:
  - TEST-009
```

**What.** All four Infrastructure test classes are declared `public class ... : IDisposable` -- not
`sealed` -- and implement disposal as a single expression-bodied
`public void Dispose() => _connection.Dispose();`. There is no `Dispose(bool)` overload and no
guard against a second call.

**Why it matters.** The cited page states that "any non-sealed class should have a `Dispose(bool)`
overload method" and that "a `Dispose` method should be idempotent, such that it's callable multiple
times without throwing an exception". Neither holds here. The practical exposure is small -- xUnit
calls `Dispose` once per instance and `SqliteConnection.Dispose` is itself idempotent -- but the
declaration invites a derived fixture that would have no correct way to add its own cleanup, which
is exactly the direction `TEST-009` proposes.

**Suggested direction.** The simplest fix is `sealed`, which removes the requirement outright and
is correct for a leaf test class. If the shared base class from `TEST-009` is built instead, that
base is the one type that needs the full pattern.

### TEST-027 -- Two incompatible test-naming schemes are in use

```yaml
id: TEST-027
layer: TEST
severity: S4
category: naming
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs:22
  - src/DungeonMaster.Tests/Domain/Entities/Floor/ObjectScaleTests.cs:11
  - src/DungeonMaster.Tests/App/Theme/ThemeHygieneTests.cs:71
  - src/DungeonMaster.Tests/App/Theme/ThemeDictionaryTests.cs:121
  - src/DungeonMaster.Tests/App/Theme/DmThemeKeysResolveTests.cs:51
  - src/DungeonMaster.Tests/App/Theme/TokenNamingTests.cs:179
effort: M
risk: low
independent: true
```

**What.** The `Application/`, `Domain/` and `Infrastructure/` sub-trees use
`Method_Scenario_ExpectedBehaviour` -- `HandleAsync_WithExistingDungeonAndFloor_AddsAreaAndReturnsId`,
`StepUp_ReturnsExpected`. The `App/` sub-tree uses declarative sentences with no method under test
and no separators -- `NoRawColourLiteralsOutsideTheThemeDictionaries`,
`TokensIsTheOnlyThemeFileContainingColourLiterals`,
`EveryColourTokenObeysTheNamingRule`. A minority in `App/` splits the difference
(`EveryKeyDmThemeResolves_IsDefinedInTheThemeDictionaries`).

**Why it matters.** The cited page prescribes the three-part form -- method, scenario, expected
behaviour -- so that "just by looking at the suite of unit tests, you should be able to infer the
behavior of your code". The `App/` names are readable in isolation but do not name a method, which
is defensible given there is no method under test: these facts assert over files. So the deviation
is partly justified by what the sub-suite does. What is not justified is the third scheme mixed into
the same directory, which leaves a reader with no rule to follow.

**Suggested direction.** Keep the sentence form in `App/` -- it fits its subject -- but make it
uniform within that directory by removing the underscore-separated hybrids, and state the two
schemes and where each applies somewhere a new file's author will see it.

### TEST-028 -- Comment-stripping regexes and a line-number helper are re-declared instead of shared

```yaml
id: TEST-028
layer: TEST
severity: S4
category: duplication
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Duplicated Code"
locations:
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:53-56
  - src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:220-229
  - src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:720-723
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:70-73
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:247-253
  - src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs:112-115
  - src/DungeonMaster.Tests/App/Theme/ThemeTokenParityTests.cs:32-35
  - src/DungeonMaster.Tests/App/Theme/ThemeDictionaryTests.cs:20-23
effort: S
risk: low
independent: true
```

**What.** `ThemeSource` exists, by its own summary, because "the patterns and file scans here were
duplicated across five test classes". An XML-comment pattern is nonetheless declared three times
(`ThemeSource`, `GeometryScaleTests`, `IconFontCoverageTests`), a C#-comment pattern twice
(`ThemeDictionaryTests`, `ThemeTokenParityTests`), and the identical
`text.Take(index).Count(c => c == '\n') + 1` line-number computation three times
(`ThemeSource.LineNumber`, `IconFontCoverageTests.LineNumber`, and inline in
`SemanticDescriptionTests`).

**Why it matters.** This is the divergence `ThemeSource`'s own documentation says the class was
created to prevent, reappearing after it. Two of the three comment patterns differ deliberately --
`GeometryScaleTests` needs a `body` capture group so it can blank the interior and preserve offsets
-- so the fix is not a blind merge; but the three identical line-number copies have no such excuse,
and the two comment-line patterns in `ThemeDictionaryTests` and `ThemeTokenParityTests` differ only
by an anchor that is arguably a bug in one of them.

**Suggested direction.** Move the blanking helper and the line-number helper into `ThemeSource` as
the single home the class was created to be, and have the two C#-comment strippers agree on whether
the pattern is anchored (`GeometryScaleTests.cs:727-730` documents why it must be).

### TEST-029 -- Several tests carry two or more independent reasons to fail

```yaml
id: TEST-029
layer: TEST
severity: S4
category: style
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs:137-154
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:128-144
  - src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:156-171
  - src/DungeonMaster.Tests/App/Theme/TokenNamingTests.cs:263-272
  - src/DungeonMaster.Tests/Application/Common/Map/MapDrawingSessionTests.cs:13-17
effort: S
risk: low
independent: true
```

**What.** `EveryIconOnlyControlHasASemanticDescription` asserts offenders are empty and then that
the scan reached something. `EveryDmFontIconTextIsInTheIconFontsCmap` does the same.
`TheIconFontCmapParsesToPrivateUseGlyphs` makes four unrelated assertions.
`EveryRegisteredExceptionStillExistsAndStillOffends` asserts that every reason string is non-blank
and separately that the complaint list is empty. `PlaceVertex_FirstVertex_ReturnsAdded` asserts the
result, the vertex count and the session state.

**Why it matters.** The cited page's "Avoid multiple Act tasks" section notes that once an assertion
fails, subsequent ones do not execute, so "some working functionality might be interpreted as
failing". In the anti-vacuity cases the ordering also inverts the diagnosis: because the offender
assertion runs first, a suite that has silently stopped scanning anything is only reported when the
offender list happens to be empty, which is precisely the state it is supposed to distinguish from
success.

**Suggested direction.** Split the anti-vacuity assertions into their own facts (this is the same
change `TEST-016` proposes and should land with it), and split
`TheIconFontCmapParsesToPrivateUseGlyphs` into one fact per property of the parse.

### TEST-030 -- Assertions appear inside Arrange blocks

```yaml
id: TEST-030
layer: TEST
severity: S4
category: style
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
locations:
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs:145
  - src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs:186
  - src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs:79
effort: XS
risk: low
independent: true
```

**What.** Two tests in `UpdatePlacedObjectCommandHandlerTests` obtain the object under test with
`floor.Objects.Should().ContainSingle().Which` under an explicit `// Arrange` comment:
`HandleAsync_ObjectNotFound_ReturnsSuccessWithNoChange` binds it as `theObj` at line 145, and
`HandleAsync_WithCustomScale_StoresScaleOnObject` binds it as `obj` at line 186.
`AreaPersistenceDiagnosticTests.RemoveArea_WithEmptyOutline_PersistsAcrossReload` asserts
`Outline.Should().BeEmpty()` in the middle of the block that performs the removal.

**Why it matters.** The cited page's "Arrange your tests" section says the pattern "helps to reduce
the opportunity for assertions to intermix with code in the Act task", and separating the phases is
what makes a failure legible. Here a failure in the Arrange assertion is reported as a failure of
the test's stated behaviour, so `HandleAsync_ObjectNotFound_ReturnsSuccessWithNoChange` can go red
for a reason that has nothing to do with the object not being found.

**Suggested direction.** Replace the fluent extraction with `floor.Objects.Single()` in Arrange, and
move the outline assertion in the diagnostic test into its own fact -- or delete it, since
`TEST-012` proposes removing that test entirely.

### TEST-031 -- One theme ratchet is structurally incapable of failing in its current state

```yaml
id: TEST-031
layer: TEST
severity: S5
category: dead-code
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/App/Theme/ThemeHygieneTests.cs:65
  - src/DungeonMaster.Tests/App/Theme/ThemeHygieneTests.cs:131-151
effort: XS
risk: low
independent: true
```

**What.** `EveryAllowListedFileExistsAndStillOffends` iterates `PendingMigration`, which is
initialised empty at line 65. The `Where` therefore produces nothing, and the test asserts that an
empty list is empty. The class documents this at length, calls it "vacuous today and that is the
point", and argues it is a self-arming ratchet rather than dead code.

**Why it matters.** I find the author's reasoning persuasive and I am recording this as an
observation rather than a deviation. The argument -- that the guard is needed at the exact moment
someone adds the first allow-list entry, which is also the moment nobody would think to write it --
is sound, and the equivalent guards in `GeometryScaleTests` and `SemanticDescriptionTests` do bite
today, so the shape is proven. My only note is that a maintainer running the suite has no way to
distinguish this test from one that has silently stopped reaching anything, which is the failure
mode the rest of the sub-suite works hard to make visible.

**Suggested direction.** If it is kept -- and I believe it should be -- consider marking it in a way
that says "intentionally inert while the list is empty" to the person reading a test report rather
than only to the person reading the source; a `[Fact(DisplayName = ...)]` naming the condition would
do it.

### TEST-032 -- coverlet is referenced but there is no runsettings, no collection configuration and no CI invocation

```yaml
id: TEST-032
layer: TEST
severity: S5
category: dependency
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:11
effort: S
risk: low
independent: true
```

**What.** `coverlet.collector` 6.0.4 is referenced by the test project. There is no `.runsettings`
file anywhere in the repository, no `[assembly: CollectionBehavior]` attribute, no
`xunit.runner.json`, and no CI configuration visible. `dotnet test` therefore runs with xUnit's
defaults and produces no coverage output unless someone passes
`--collect:"XPlat Code Coverage"` by hand.

**Why it matters.** I believe the absent `.runsettings` matters less for coverage than it first
appears -- the interesting question in this suite is which behaviours are untested (`TEST-001`,
`TEST-019`, `TEST-020`), and a percentage would not surface any of those; the Microsoft guidance is
itself explicit that a coverage number does not imply quality. What the absent configuration does
mean is that the package is present and unused, and that nothing pins the parallelism settings the
suite currently relies on. The suite is safe under the defaults today (see the layer verdict), but
that safety is a property of how the fixtures happen to be written, not something the repository
states. The project-file record itself belongs to T6.

**Suggested direction.** Either remove `coverlet.collector` or add the `.runsettings` and the CI
step that uses it. Separately, consider an explicit `[assembly: CollectionBehavior]` declaration so
the parallelism the fixtures depend on is written down rather than inherited.

## Layer verdict

The suite holds its shape in Domain and Application and loses it in `App/`. The Domain tests drive
behaviour through the aggregate root -- `AreaOutlineTests` goes through `Dungeon`, never `Area` --
so they survive a refactor of entity internals: the right seam. The Application handler tests follow
`dotnet-format.md` section 12 exactly, and NSubstitute use is sound rather than over-mocked: one
substituted repository interface per class, nothing the test owns mocked. FluentAssertions is
specific enough that most failures diagnose themselves; `Should().BeEquivalentTo` appears once
(`ThemeDictionaryTests.cs:156`), correctly. Verification style is the one real
weakness: `Received(1)` on paths whose own names say nothing changed (`TEST-010`) is the suite's
clearest implementation coupling, and a contained one.

xUnit usage is correct, and I checked shared state explicitly: the suite is **sound under xUnit's
default parallelism** -- each test method gets a private database, the only static state is immutable
after initialisation; the working is in `## Method`. `[Theory]` and `[MemberData]` fit their uses,
though three geometry and five snap-zone facts should join them (`TEST-013`).

My verdict on `App/`: **reading source files off disk is a defensible way to enforce theme and
accessibility hygiene, and these are not tests -- they are a linter wearing a test's clothes, and
the disguise is costing them.** The guards are excellent; the icon-font cmap and token-type checks
each caught a real shipped defect. The mechanism is wrong in three ways. Path resolution is ambient
and unverified, so the sub-suite depends on a `.git` entry in an ancestor directory (`TEST-003`).
Eleven of the thirteen files assert over XAML by regex while a twelfth proves `XDocument` does it
better in fewer lines (`TEST-004`); the anti-vacuity guards exist because regex scans fail silently.
And several facts assert against the lexical spelling of C# (`TEST-021`), so a behaviour-preserving
refactor either reddens a green change or silently retires the guard. The alternative -- Roslyn
analyzers for the prohibitions, a project reference for the value checks -- costs real work and is
not obviously worth it; but at 4,690 lines and growing, it deserves weighing.

Two seams sit wrong regardless. The theme guards share a `dotnet test` invocation with the unit
tests, so a lint failure and a regression are indistinguishable; and `ThemeSource`, the
intended one home for shared scanning, is already bypassed (`TEST-028`). The absent
`DungeonMaster.App` project reference is a boundary record T6 owns.

## Attestation

- No file under `src/` was created, modified, or deleted by this task: yes
- Every record cites at least one file path with a line number: yes
- Every record names its authority, and every non-`judgment` record carries a real citation: yes
