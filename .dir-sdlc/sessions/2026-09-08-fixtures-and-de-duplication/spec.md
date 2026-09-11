# Session 6b-ii — Fixtures and de-duplication

Close the ten `TEST`-layer audit entries assigned to sub-session 6b-ii, all in
`src/DungeonMaster.Tests`, all host-free.

## Context

Session 6b holds 26 audit entries. 6b-i closed eleven and 6b-iii holds five; this session holds the
remaining ten — verified by set arithmetic, not by reading: `11 + 5 + 10 = 26`, no overlap, no gap.

**The audit does not describe this repository any more.** `docs/audit/05-tests.md` was committed
**2026-08-17** (`git log --diff-filter=A` → `2603170`), and **28 commits have touched
`src/DungeonMaster.Tests` since `bcb49dc^`**. Two measurement spikes ran before decomposition, at the
user's direction, and found that **eight of the ten entries are live and two are already remediated**;
of the eight live, **six carried a materially wrong count, path, or premise**. `MASTER-064` is the only
entry whose text survived checking intact.

All measurements are in `spike-findings.md`, which is the **governing artifact for this session**:
where it and the audit disagree, it wins. The decomposition was built against it, not against the
audit.

## Definition of done

Each item is checkable by someone who did not write the code.

1. **`MASTER-014`** — no test in `src/DungeonMaster.Tests` builds its schema with `EnsureCreated`;
   the shared base uses `Database.Migrate()`. Checked by `grep -rn "EnsureCreated"` over the test
   project returning **0** outside the base's own history, and by the suite passing.
2. **`MASTER-065`** — the SQLite in-memory fixture exists **once**, as a new non-sealed base type, and
   the **seven** classes that duplicated it inherit from it. Checked by the fixture block appearing in
   exactly one file.
3. **`MASTER-127`** — all seven leaf classes are `sealed`; exactly one non-sealed type in the chain
   carries `protected virtual Dispose(bool)`. Checked by a grep for non-sealed `IDisposable` test
   classes returning only the base.
4. **`MASTER-042`** — `AreaPersistenceDiagnosticTests` is renamed to a non-diagnostic name with a
   rewritten summary, and **exactly one** test is deleted: `RemoveArea_PersistsAcrossReload`.
   `UpdateArea_RenamePersistsAcrossReload` **survives**.
5. **`MASTER-085`** — the fourteen command handler test classes share one base; the twelve
   dungeon-missing not-found tests are expressed once. Checked by the duplicated constructor body
   appearing in one file.
6. **`MASTER-064`** — the four name-validator test classes are expressed once, parameterised by
   command type. The other ten validator test classes are untouched.
7. **`MASTER-067`** — the four uniform geometry facts (`Spacing`, `Margin`, `Stroke`,
   **`MinimumSize`**) are one data-driven test, and a **new named `[Fact]` asserts the row set's
   cardinality against the expected named set.** `Padding` and `Radius` stay separate.
8. **`MASTER-069`** — each query-handler test fails when the handler returns a different instance
   **and** when it corrupts a field the old assertions did not name. Demonstrated, not asserted.
9. **`MASTER-066` and `MASTER-041`** — marked done in the master index against commit `bcb49dc`, with
   the evidence recorded and **no code change**, because the fix each prescribes is respectively a
   regression and a no-op.
10. **Addition 1** — the commit-boundary assertion is added where it is strictly more observable than
    the repository-boundary one, **and the task demonstrates that before relying on it.**
11. **Addition 2** — `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` exists in
    `DungeonMaster.Tests`, with a demonstration that it enables what it claims to.
12. **The master index** records all ten entries' outcomes, and records that the audit is stale so
    **6b-iii inherits the warning** rather than rediscovering it.
13. **The suite is green and the build is clean** — every task re-measures the baseline rather than
    inheriting `695`, and every session-touched `.cs` file is CRLF in the main checkout after merge.

## Scope

The ten entries above, plus the two additions the user scoped in at the re-scope gate.

### Not in scope

- 6b-iii's five entries: `MASTER-020`, `070`, `086`, `155`, `167`.
- `MASTER-161`, `163`, `164`, `165`, `166` — dropped from Session 6 at its own Gate 1 as sidebar and
  UI defects. A suggested-home line in the audit must not draw them in.
- Reconciling `.dir-sdlc/backlog.md` for the four items 6a left, three of which 6b-i closed in
  substance. Not this session's, as the handoff states.
- Marking any prior session's index rows. This session marks only its own.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Which entries belong to 6b-ii? | The ten named | 1 — repo | `docs/audit/00-master-index.md` Session 6b line; partition proved by set arithmetic |
| Is the audit's account of these entries current? | **No** — stale by 28 commits | 2 — measured | `git log --oneline bcb49dc^..HEAD -- src/DungeonMaster.Tests \| wc -l` → 28 |
| How many files call `EnsureCreated`? | **7**, not the audit's 4 | 2 — measured | `grep -rn "EnsureCreated" --include=*.cs src/DungeonMaster.Tests` |
| How many migrations exist? | **3**, not the audit's 2 | 2 — measured | `ls Migrations/*.cs \| grep -v Designer \| grep -v Snapshot \| wc -l` |
| Does `Migrate()` work on SQLite in-memory? | Yes | 2 — measured | Scratch probe: `APPLIED=3 PENDING=0 TABLES=6` |
| Will `MASTER-014` arrive red? | No — migrations are in sync and the two schemas are set-identical | 2 — measured | `dotnet ef migrations has-pending-model-changes`; normalised schema diff |
| How many non-sealed `IDisposable` test classes? | **7**, not the audit's 4 | 2 — measured | `grep -rn "class .*IDisposable" \| grep -v sealed \| wc -l` → 7 |
| Does `DeleteDungeonCommandHandlerTests` lack a not-found test? | **No, it has one** — the audit is wrong | 2 — measured | `DeleteDungeonCommandHandlerTests.cs:65,85` |
| Are `MASTER-042`'s two subsumptions both real? | **No, one is** | 2 — measured | Injection with a passing control: `SecondRename` stays green under a defect `RenamePersists` catches |
| Does `SnapZoneCalculatorTests` still exist? | **No** — deleted in `1678d12` | 2 — measured | `git ls-tree -r --name-only HEAD \| grep -i snapzone` → nothing |
| How many uniform geometry facts? | **4**, not the audit's 3 | 2 — measured | `grep -n "NoBare" GeometryScaleTests.cs` → 6 facts, 4 uniform |
| Which fixture route for `MASTER-065`? | Abstract base class | 5 — user | Re-scope gate decision; chosen over `IClassFixture` to preserve per-test isolation and keep `MASTER-127` real |
| How to dispose of `MASTER-066`/`041`? | Mark done vs `bcb49dc`, plus act on two spike-exposed gaps | 5 — user | Re-scope gate decision |
| Can the 7 Infrastructure files be split across tasks? | **No** — one task | 2 — measured | Four entries touch them simultaneously and one renames a file; DIR forbids intersecting concurrent ownership |
| How many `TestDbContextFactory` use sites must keep working? | **10 across 7 files** | 2 — measured | `grep -rl` excluding the declaration → 7 files, 10 sites |
| Is there a common base for `Result` and `Result<T>`? | **No** — two unrelated sealed records | 2 — measured | `grep -rn "record Result"` → both `public sealed record`, no shared interface |
| Does any `internal interface` exist to justify Addition 2? | **No** — the demonstration must declare its own | 2 — measured | `grep -rn "internal interface"` over product code → 0 |
| Which assertion library? | FluentAssertions 8.9.0; **no xunit `Assert` anywhere** | 2 — measured | `Directory.Packages.props:21`; 83 of 92 test files use FluentAssertions |

## Contracts

- `contracts/sqlite-fixture-base.md` — the shared fixture base's surface: `Options`,
  `CreateContext()`, `CreateDbContextFactory()`, `CreateFactory()`, and the `Dispose` shape. **T1
  implements it; its consumers are the seven leaves and `TestDbContextFactory`'s 10 sites.**
- `contracts/shared-test-base-convention.md` — the single convention for abstract generic test bases,
  so `MASTER-085` and `MASTER-064` do not invent two shapes for one idea. Includes the extraction
  threshold and the leaf-count-asserted rule.

Also binding, from a prior session:
`.dir-sdlc/sessions/2026-09-02-app-sub-suite-rebuild/contracts/anti-vacuity.md` §2 and §5 govern
`MASTER-067`'s conversion.

## Open questions

| Question | Who resolves | When |
|---|---|---|
| Should `docs/audit/05-tests.md`'s seven measured mis-citations be corrected in place, or only noted in the master index? | User | Gate 2 — it is the one uncovered coverage row |
| Should `MASTER-085`'s two-member query equivalence class be extracted despite falling below the threshold of three? | User | Gate 2 |
