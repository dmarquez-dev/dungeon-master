# Coverage mapping — Session 6b-ii

`Item`, `Covering task`, `Consuming task` and `Gate 2` written at Gate 2.
`Implementing task` and `End state` written at Gate 3.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. `MASTER-014` — no `EnsureCreated`; base uses `Database.Migrate()` | T1 | covered | **met** — `EnsureCreated` call sites 7 → 0 (the one HEAD hit is prose in an XML summary). Reviewer re-measured both figures |
| 2. `MASTER-065` — fixture exists once as a new non-sealed base; 7 classes inherit | T1 | covered | **met** — `new SqliteConnection` 7 files → 1. No leaf declares any of the six removed members |
| 3. `MASTER-127` — 7 sealed leaves; exactly one non-sealed type with `Dispose(bool)` | T1 | covered | **met** — non-sealed `IDisposable` test classes 7 → 1, and that one is the base |
| 4. `MASTER-042` — renamed; **exactly one** test deleted; rename test survives | T1 | covered | **met** — 4 facts → 3, the absent one `RemoveArea_PersistsAcrossReload`. Non-subsumption proven by injection with a **passing control**, reproduced by the reviewer |
| 5a. `MASTER-085` — the **14** command classes share one base | T2 | covered — added at Gate 2 | **met** — `new UnitOfWorkSubstitute()` 14 files → 0; two-level hierarchy, 14 leaves |
| 5. `MASTER-085` — the **12** dungeon-missing not-found tests expressed once | T2 | covered | **met** — 12 leaves derive from `DungeonNotFoundHandlerTestBase`; `--filter` on the inherited fact → **`Passed: 12`**, not 1, so the base runs per leaf |
| 5b. `MASTER-085` — the 2-member query equivalence class | T3 | covered — settled, threshold 3 | **met** — no base introduced; the rule **and** the measured count of 2 both recorded, so the decision is distinguishable from an omission |
| 6. `MASTER-064` — four name-validator classes expressed once | T4 | covered | **met** — four leaves on `NameOnlyValidatorTestBase<TCommand>`; per-leaf discrimination proven by injection, reviewer used a **different** validator |
| 6a. `MASTER-064` — the other ten validator classes untouched | T4 | covered — added at Gate 2 | **met** — `git diff --stat` names exactly the four leaves; no eleventh file |
| 7. `MASTER-067` — four uniform geometry facts data-driven **plus a cardinality `[Fact]`** | T5 | covered | **met** — reviewer dropped a **different** row than the implementer and got one failure, the guard, while `Total` silently fell 10 → 9 |
| 8. `MASTER-069` — fails on a different instance **and** on unnamed-field corruption | T3 | covered | **met** — 4 of 6 mutations invisible before, 0 of 6 after; reviewer ran four **different** injections |
| 9. `MASTER-066` / `MASTER-041` — marked done vs `bcb49dc`, evidence recorded, no code change | T6 | covered | **met** — both rows marked, `git merge-base --is-ancestor bcb49dc HEAD` → 0; `UpdateAsync` 45 → 0, `Task.CompletedTask` 13 → 0 |
| 10. Addition 1 — commit-boundary assertion, **demonstrated before relied on** | T2 | covered | **met** — under injection **24 failures, all naming `CommitAsync`, none naming `Update`**; the pre-task suite would have caught it zero times |
| 11. Addition 2 — `InternalsVisibleTo` with a demonstration | T2 | covered | **met** — `red step` reproduced: without it, `ArgumentException`; with it, pass. Fact name states its own cause |
| 12. Master index records all ten outcomes **and warns 6b-iii** | T6 | covered | **met** — and it **corrected the session's own framing**; warning names all five 6b-iii entries as *suspected, not known* |
| 13. Suite green, build clean, each task re-measuring | T1–T7 | covered | **met** — serial run at `e55eb85` with **empty** `git status`: build `0 Warning(s) 0 Error(s)`; `699 / 77 / 23` |
| 13a. Every session-touched `.cs` CRLF in the **main checkout** after merge | orchestrator, Gate 3 | covered — assigned at Gate 2 | **pending the merge** — verified per-file in the worktree by every task; the main-checkout check runs after `merge-session` |
| 14. The seven mis-citations in `docs/audit/05-tests.md` corrected in the entry bodies | **T7** | covered — became a task at Gate 2 | **met, and it corrected the premise** — five of the seven are real; **two were not defects at all** |

## Contract operations

Searched over the **37 files** in `git diff --name-only 850a10b..HEAD`, matching each operation as a
whole word, discarding the definition site.

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `SqliteInMemoryTestBase.Options` | T1 | T1 | covered | **called** — 2 files / 3 hits (`AreaOutlineChangeTrackingTests`, `PlacedObjectConfigurationTests`) |
| `.CreateContext()` | T1 | T1 | covered | **called** — 6 files / 33 hits |
| `.CreateDbContextFactory()` | T1 | T1 | covered | **called** — 3 files / 6 hits |
| `.CreateFactory()` | T1 | T1 | covered | **called** — 7 files / 24 hits |
| `Dispose()` / `protected virtual Dispose(bool)` | T1 | xUnit, per leaf | covered | **called by the framework, not by a file in the diff** — 0 in-diff call sites outside the definition. The base is `IDisposable` and every leaf inherits it, so xUnit's disposal path reaches it. **The search cannot see framework invocation; this is stated rather than scored as `not called`** |
| `CommandHandlerTestBase<THandler>` | T2 | T2 | covered | **called** — 3 files (the derived base + the 2 direct leaves) |
| `DungeonNotFoundHandlerTestBase` | T2 | T2 | covered | **called** — 13 files / 13 hits |
| `InvokeWithMissingDungeonAsync` | T2 | T2 | covered | **called** — 12 files / 12 hits, one per leaf |
| `NameOnlyValidatorTestBase<TCommand>` | T4 | T4 | covered | **called** — 4 files |
| `NamePropertyName` | T4 | T4 | covered | **called** — 4 files, each via `nameof` on its own command type |
| `CreateCommandWithName` | T4 | T4 | covered | **called** — 4 files |
| `UnitOfWorkSubstitute.Factory` / `.UnitOfWork` / `.Dungeons` (pre-existing, frozen) | prior session | T2, T4, **and `ApplicationServiceRegistrationTests.cs` owned by no task** | covered | **called, and the unowned consumer verified green** — `ApplicationServiceRegistrationTests` → `Passed: 11`, `CommandValidatorCoverageTests` → `Passed: 6`, identical before and after |

### A stated deviation from the search's discard rule

`dir`'s contract-operation search discards hits inside test files, because *"an operation exercised
only by its own tests is not reached by the application."*

**Every file in this session is a test file.** The deliverable *is* test infrastructure, and the
consumers of a test-fixture contract are test classes by design. **Applying that exclusion literally
would mark all eleven operations `not called` and produce a false alarm on every one.** The exclusion
is written for production operations; it does not fit a test-infrastructure contract, so it was not
applied, and this paragraph records that rather than leaving a reader to infer why the rule was
skipped.

The definition-site and same-owning-task exclusions **were** applied. Note this weakens the check
here relative to a production session: for `T1` and `T4` the consumers are files the same task owns,
which the rule normally discards as insufficient evidence. What the search establishes here is that
the extraction is genuinely used by every class it was extracted from — not that an application path
reaches it, which for test infrastructure is not a meaningful question.

**No operation shipped with an implementer and no consumer.** That is the finding this artifact exists
to produce, and this session does not have one — unlike 6b-i, which shipped six.
