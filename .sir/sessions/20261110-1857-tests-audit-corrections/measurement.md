# Orchestrator measurements, spec phase

Taken by the orchestrator in the main checkout at base `07f0512` (`chore/repo-audit`), 2026-11-10, before
decomposition. Tasks cannot run git; every fact below that needs history was measured here, with its command,
and a task quotes it as attributed rather than re-measuring it. Commands are Git Bash from the repository root.

## M0. Target files at 07f0512

Python byte census (`b.count(b'\r')`, `b.count(b'\r\n')`, `b.count(b'\n')`):

| File | bytes | CR | CRLF | LF | lone CR | sha256 |
|---|---|---|---|---|---|---|
| `docs/audit/05-tests.md` | 153224 | 2384 | 2384 | 2384 | 0 | `99ac05a316cf5007e69d068bf73d4a72ad58e0371db960da2fca0f5759e807c0` |
| `docs/audit/06-cross-cutting.md` | 50311 | 881 | 881 | 881 | 0 | `495dee996a0f841aa026e6f3988c03442972c7fac07bbec24ef5ddd528e74f7c` |
| `.sir/backlog.md` | 58548 | 186 | 186 | 186 | 0 | (Gate 3 only) |

Zero lone CR in all three, so `Edit` is safe on them (FORM 13 does not bite). `Write` is not (it emits LF).

Authoring commits (`git log --diff-filter=A --format='%h %ad' --date=iso -- <file>`): `05-tests.md` at
`2603170` (2026-08-17 14:42:41 -0700); `06-cross-cutting.md` at `676a8f3` (2026-08-17 16:55:21 -0700).
`2603170` is an ancestor of `413be78` (`git merge-base --is-ancestor`), 8 commits earlier.

## M1. Row :95 -- the elided path at 05-tests.md:274

`grep -n 'src/\.\.\./Validators' docs/audit/05-tests.md` prints one line, `:274`, inside the fenced block of the
`AMENDED 6b-iii ... CORRECTION 6, the dead location anchor` block (`:262-284`, entry `TEST-001`, heading `:163`):

    $ git show 413be78:src/.../Validators/AddAreaCommandValidatorTests.cs | sed -n '15,19p'

`grep -n 'src/\.\.\.' docs/audit/*.md` prints that line and no other (census of the defect class across the
audit set: n = 1).

Full path: `git ls-tree -r --name-only 413be78 | grep AddAreaCommandValidatorTests` prints exactly
`src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs`
(the same path the block itself quotes at `:264`). The pasteable command

    git show 413be78:src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs | sed -n '15,19p'

prints (`cat -A`, tabs shown as `^I`): `$` / `^I^I// Act$` / `^I^Ivar result = _validator.Validate(command);$` /
`$` / `^I^I// Assert$` -- the five lines the block's fence shows at `:275-279` (fences at `:273` and `:280`;
corrected from `:276-280` after the architect's CRLF-split read, confirmed by the orchestrator the same way). (Content only; `sed -n` strips
nothing that matters here, FORM 7, and is never used to judge line endings.)

## M2. Row :96 -- "it did not move in between", 05-tests.md:2239

`grep -n 'did not move in between' docs/audit/05-tests.md` prints `:2239`, in the `AMENDED 6b-iii ... CORRECTION
1, the scope is triple today` block (`:2215-2260`, entry `TEST-032`, heading `:2180`). The sentence, `:2238-2240`:
*"coverlet stood at line **9** on the audit date -- line 11 was `Microsoft.NET.Test.Sdk` -- and 9 is the same line
it still stood at when T4 removed it, so it did not move in between. Authorship error, not drift."*

The row asks to narrow the clause because two endpoints do not establish the path. **The path was measured**:

- `git log --full-history --format='%h %p %ad %s' --date=short 413be78..5347813 -- src/DungeonMaster.Tests/DungeonMaster.Tests.csproj`
  prints three commits: `5347813` (2026-09-11, `build(deps): remove unused coverlet.collector package`, the T4
  removal), `dfa49d7` (2026-08-26, a merge), `828327a` (2026-08-26).
- Per-commit census, every commit from `413be78` through `5347813^` (`413be78` plus
  `git rev-list 413be78..5347813^`, 105 commits; `git rev-list --count 413be78..5347813` prints 105, which
  includes `5347813` itself and not `413be78`):
  `git show $c:src/DungeonMaster.Tests/DungeonMaster.Tests.csproj | grep -n coverlet | cut -d: -f1` aggregated
  with `sort | uniq -c` prints exactly `105 L9`.
- Control (the census can report a line other than 9): `git show 828327a:<same file> | grep -n Microsoft.NET.Test.Sdk`
  prints `11:`.
- At `5347813` the same grep prints nothing (removed). At `2603170`, `5347813^` and `828327a^` it prints `9:`.

Result: coverlet stood at line 9 in **every one of the 105 commits** from the audit-era commit `413be78` to the
removal's parent. The clause is true at commit granularity; what it lacked was the evidence, not accuracy. (No
claim is possible about uncommitted working-tree states; the record's unit is the commit.)

## M3. Row :108 -- `DependencyInjection.cs`, renamed

`grep -n DependencyInjection.cs docs/audit/05-tests.md docs/audit/06-cross-cutting.md` prints five lines:

| Site | Entry | Text |
|---|---|---|
| `05-tests.md:377` | `TEST-002` (heading `:353`), `What.` paragraph | `` (`src/DungeonMaster.Infrastructure/DependencyInjection.cs:50`) `` -- for `db.Database.Migrate()` |
| `06-cross-cutting.md:117` | `XCUT-001` (heading `:107`), `locations:` | `  - src/DungeonMaster.Infrastructure/DependencyInjection.cs:30-34` |
| `06-cross-cutting.md:118` | `XCUT-001`, `locations:` | `  - src/DungeonMaster.Infrastructure/DependencyInjection.cs:45-51` |
| `06-cross-cutting.md:130` | `XCUT-001`, `What.` paragraph | `` `DependencyInjection.cs:47`, inside `MigrateDatabase` `` -- the one `CreateScope` hit |
| `06-cross-cutting.md:334` | `XCUT-006` (heading `:321`), `locations:` | `  - src/DungeonMaster.Infrastructure/DependencyInjection.cs:33-34` |

All five name the **Infrastructure** file. A second, unrenamed `src/DungeonMaster.Application/DependencyInjection.cs`
exists at `07f0512` (`git ls-files | grep DependencyInjection`), so a bare filename search over-reports; the
Infrastructure path is what distinguishes them.

Rename: `git log --diff-filter=R -M --name-status -- '*DependencyInjection.cs' '*InfrastructureServiceCollectionExtensions.cs'`
prints `69e3c14 2026-10-16 refactor(infra): rename DependencyInjection.cs to match the type it declares`,
`R100 src/DungeonMaster.Infrastructure/DependencyInjection.cs src/DungeonMaster.Infrastructure/InfrastructureServiceCollectionExtensions.cs`.

**Live when written (DRIFT, all five).** `git rev-parse <c>:src/DungeonMaster.Infrastructure/DependencyInjection.cs`
prints the same blob `e2c9267002f6d7da21cb6f0f434c4836e06fe0f8` at `2603170`, `676a8f3` and `413be78`. In that
blob: `:30-31` `services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));`, `:33`
`services.AddScoped<IDungeonWriteRepository, DungeonWriteRepository>();`, `:34`
`services.AddScoped<IDungeonReadRepository, DungeonReadRepository>();`, `:45`
`public static void MigrateDatabase(this IServiceProvider services)` through `:51` `}`, `:47`
`using var scope = services.CreateScope();`, `:50` `db.Database.Migrate();`. Every citation was exact.
`git grep -n -e CreateScope -e CreateAsyncScope 676a8f3 -- 'src/*.cs'` prints one line,
`src/DungeonMaster.Infrastructure/DependencyInjection.cs:47` -- the "exactly one hit" `06:130` reports.

**The content changed as well as the name.** Commits touching the file after `676a8f3`
(`git log --format='%h %ad %s' --date=short 676a8f3..07f0512 -- <old> <new>`): `ef0b0b6` 2026-08-18
`refactor(persistence): establish unit of work lifetime boundaries`; `33da60f` 2026-08-24 (ConfigureAwait);
`7b2586b` 2026-10-09 (cancellationToken); `69e3c14` 2026-10-16 (the rename).
At `07f0512`, `git show 07f0512:src/DungeonMaster.Infrastructure/InfrastructureServiceCollectionExtensions.cs | grep -n -e AddDbContextFactory -e AddSingleton -e MigrateDatabaseAsync -e MigrateAsync -e CreateScope` prints:
`30: services.AddDbContextFactory<AppDbContext>(options =>`, `33: services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();`,
`34: services.AddSingleton<IDungeonReadRepository, DungeonReadRepository>();`,
`51: public static async Task MigrateDatabaseAsync(`, `59: await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);`
(the method's closing `}` is `:60`; the class's is `:61`). No `CreateScope` in the file.

Counterparts at `07f0512`, by site (a counterpart is the code now at the cited role, **not** the cited code):

| Site | Cited at the audit commit | At `07f0512` in `InfrastructureServiceCollectionExtensions.cs` |
|---|---|---|
| `05:377` `:50` | `db.Database.Migrate();` | `:59` `await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);` |
| `06:117` `:30-34` | `AddDbContext` + two `AddScoped` | `:30-34`, same span, now `AddDbContextFactory<AppDbContext>` (`:30-31`) and two `AddSingleton` (`:33` `IUnitOfWorkFactory`, `:34` `IDungeonReadRepository`) |
| `06:118` `:45-51` | `MigrateDatabase` (sync, opens a scope) | `MigrateDatabaseAsync` at `:51-60` (resolves `IDbContextFactory<AppDbContext>`, no scope) |
| `06:130` `:47` | `services.CreateScope()` | none: no `CreateScope` in the file. `git grep -n -e CreateScope -e CreateAsyncScope 07f0512 -- 'src/*.cs'` prints 10 lines, every one under `src/DungeonMaster.Tests/` (`Application/Common/Dispatch/ApplicationServiceRegistrationTests.cs` 6, `Application/Features/Dungeons/Validators/CommandValidatorCoverageTests.cs` 4) |
| `06:334` `:33-34` | `AddScoped<IDungeonWriteRepository,...>`, `AddScoped<IDungeonReadRepository,...>` | `:33` `AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>`, `:34` `AddSingleton<IDungeonReadRepository, DungeonReadRepository>`; `IDungeonWriteRepository` is no longer registered in this file -- `src/DungeonMaster.Infrastructure/Persistence/UnitOfWork.cs:15` constructs `DungeonWriteRepository` (`git grep -n IDungeonWriteRepository 07f0512 -- 'src/*.cs'`, non-test lines) |

**Register precedent for the front matter.** `02-infrastructure.md:1067-1068` (the INFRA-017 rename closure; `grep -n -F 'The front matter is deliberately'` prints `:1067`, and `:423` for an unrelated entry):
*"The front matter is deliberately left as written; this block is the correction, per this arc's practice of never
rewriting original metadata."* `02-infrastructure.md:1092-1094` names `05-tests.md` and `06-cross-cutting.md` as
still citing the old name, "reported rather than edited".

**Outside this session's scope, found by the same census** (`grep -n DependencyInjection docs/audit/*.md`): other
Infrastructure-file citations at `00-master-index.md:33`, `02-infrastructure.md:7`, `:41`, `:1011`, `:1107`,
`:1149`, `03-app-shell-and-views.md:89`. Most of 02's are INFRA-017's own historical record of the rename.

## M4. Row :177 -- TEST-023, TEST-029, TEST-030

### M4.1 Classifications, settled at the authoring commit 2603170

The test tree at `2603170` was extracted with `git archive 2603170 src/DungeonMaster.Tests | tar -x -C <scratch>`
(45 `.cs` files, `find . -name '*.cs' | wc -l` -- agreeing with `TEST-025`'s "21 of 45 files"). P1 and PA were run
byte-for-byte as stated in `.sir/sessions/20261104-1726-test-hygiene-sweep/contracts/C1-measurement.md` sections 1-2,
over `$(find . -name '*.cs' | sort)`, output piped through `tr -d '\r'`.

**P1 prints 13 lines**: the eight validator sites (`AddArea`, `CreateDungeon`, `UpdateArea`, `UpdateDungeon`
`CommandValidatorTests.cs:37` and `:52`, `e =>`), `Domain/Entities/Dungeon/DungeonAggregateTests.cs:29`, `:43`,
`:68` (`f =>`), `Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:51`, `:52` (`s =>`). The five
lines `TEST-023`'s Correction 2 (`05:1728-1732`) left UNDETERMINED are **exact** at the audit commit:
**DRIFT**. The title's "thirteen" was also exact on the day.

**PA prints 2 lines**: `Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs:145`
`var theObj = floor.Objects.Should().ContainSingle().Which;` and `:186`
`var obj = floor.Objects.Should().ContainSingle().Which;`. Method headers at `2603170`
(`git show 2603170:<that file> | tr -d '\r' | grep -n 'public async Task'`): `:133`
`HandleAsync_ObjectNotFound_ReturnsSuccessWithNoChange`, `:174` `HandleAsync_WithCustomScale_StoresScaleOnObject`
(next header after `:133` is `:174`; none after `:174`). So `:145` lies in `HandleAsync_ObjectNotFound_...` and
`:186` in `HandleAsync_WithCustomScale_...`: the method name and `:186` that `TEST-030`'s Correction 2
(`05:2129-2136`) left UNDETERMINED are **exact** at the audit commit: **DRIFT**.

### M4.2 Bold-marker parity (BP) of every amendment block

Instrument: `.sir/sessions/20261110-1857-tests-audit-corrections/bp.py` (sha256
`01ecfd8aefc048ee8cbf66b06c79cf1ad923521f4933405eb340a206dbc111ee`), run as `python <bp.py> <file>...`. It finds each
`**[AMENDED` block, skips fenced code, replaces each inline code span with a placeholder, counts `**` from opener to
closer line, and prints each block whose count is odd -- an odd count is an unmatched bold delimiter, which renders
as literal asterisks.

Controls (CRLF fixtures, printf): `**[AMENDED x -- h.**` / ``body **b** `**c**`.**]**`` prints `odd 0`
(must pass; it holds `**` inside a code span); `**[AMENDED x -- h.**` / `body **b.**]**` prints `odd 1` (must fail;
the TEST-029 shape).

At `07f0512`: `05-tests.md` -- `blocks 44 odd 2`; `06-cross-cutting.md` -- `blocks 1 odd 1`.
`grep -c '^\*\*\[AMENDED' docs/audit/05-tests.md` prints 44, agreeing with BP's block count. Every one of the 44
closers in 05 ends `**]**` (`tr -d '\r' < 05-tests.md | grep -n '\]\**$'`, 44 lines, each read).

| Block | Entry | Defect, read by eye | In a row? |
|---|---|---|---|
| `05:2052-2077` | `TEST-029` | `:2076` opens `**No claim is`, `:2077` `made about any fact other than these five.**]**` -- the `**` after `five.` closes that bold, so the final `**` is unmatched and renders as a literal `]**` | yes, `:177` |
| `05:943-990` | `TEST-012` (6b-ii, CORRECTION 6) | `:979` opens `**The code the entry reasoned about was`; the sentence ends `:980` `...wrong on the day it was written.` and the paragraph ends with no closing `**` | **no** |
| `06:740-771` | `XCUT-015` (20261106-0120-shipping-identity) | opener `:740-742` closes its bold at `same change.**`; the body has no open bold; the last line ends `` `:19-23`.]** `` -- the closing `**` is unmatched | **no** |

An instrument artifact seen and fixed on the way: a first version that stripped inline code spans by deletion
reported `05:262-284` odd, because `` *`path`* `` (italic around code, `:264`) collapses to `**`. The placeholder
substitution in bp.py removes it; `05:262-284` is even under bp.py.

## M5. In-place versus insertion, the register's precedent

- Original entry prose stays standing: `05:88-125` (6b-ii/6b-iii file-level blocks), `05:1715` and peers
  ("Nothing above is deleted"), `01-domain-application.md:907` ("Nothing below deletes any previous wording").
- `locations:` is the documented in-place exception: `01-domain-application.md:911-914`, `:1309-1311`; marked in
  05 by `locations:  # AMENDED 6b-ii -- ...` at `05:362`, `:760`, `:1850`. But `02-infrastructure.md:1067-1068`
  left a rename's front matter as written.
- A correction made in place to other prose is recorded by a block quoting the previous wording verbatim:
  `05:682` ("The live `**What.**` paragraph above is corrected in place accordingly"), `05:982-985`.
- Earlier amendment blocks are left as they stood and corrected by a later block: `05:117-118` ("The block above
  is 6b-ii's and is left exactly as it stood"; `grep -n -F 'is left exactly as it stood'` prints `:118`).
