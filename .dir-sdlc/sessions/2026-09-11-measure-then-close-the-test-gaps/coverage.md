# Coverage mapping — 2026-09-11-measure-then-close-the-test-gaps

Gate 2 column written before the approval that froze `manifest.json`. End state filled at Gate 3 from
`reviews/T1.md`–`T5.md` and from the session diff `1407090..7f3ecc5`.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. Invocation seam on `CommandHandlerTestBase`, every leaf overrides it | T1 c1 | covered | **met** — clean `dotnet build` proves no CS0534 |
| 2. Base `[Fact]` proves token reaches `IUnitOfWorkFactory.CreateAsync` | T1 c2 | covered | **met** — `Received(1).CreateAsync(token)`, 14 executions |
| 3. `IDungeonWriteRepository.LoadAsync` proven | T1 c3 | covered | **met** — 12 executions; reviewer confirmed `LoadAsync` precedes the null short-circuit |
| 4. `IUnitOfWork.CommitAsync` and `RemoveAsync` proven | T1 c4 | covered | **met** — per-file facts in `CreateDungeon`/`DeleteDungeon` tests |
| 5. `GetSummariesAsync` and `GetDetailAsync` proven | T1 c5 | covered | **met** — per-file facts in the two query test classes |
| 6. Two Infrastructure tests with an already-cancelled token | T1 c6 | covered | **met** — four delivered; reviewer confirmed each seeds its row first, so cancellation is what discriminates |
| 7. Domain boundary tests against existing guards | T2 c1–c4 | covered | **met** — 4 `[Theory]` + 5 `[Fact]`; reviewer confirmed overload binding by arity |
| 8. No Domain production behaviour changes | T2 c5 | covered | **met** — `Area.cs` verified byte-identical to base |
| 9. Converter rejects any non-`int` above the `Invert` block | T3 c1 | covered | **met** — guard is the first statement in `Convert`; `Invert` block at `:53` |
| 10. Named converter behaviours preserved | T3 c2 | covered | **met** — all 11 no-change rows asserted, incl. `Convert(0, …, "Invert") == true` |
| 11. Red-first converter test | T3 c5 | covered | **met** — `red step`; 5 failures against the unmodified converter, control green, `red.txt` preserved |
| 12. `coverlet.collector` removed, lock files regenerate | T4 c1, c2 | covered | **met** — 4 sites removed, 3 of 9 locks changed, coverlet block only |
| 13. No runsettings, CI, xunit.runner.json, CollectionBehavior | T4 c5 | covered | **met** — all four verified absent |
| 14. Five `Status` cells read `**done** (6b-iii)` with evidence | T5 c1 | covered | **met** — ranks 20, 70, 86, 155, 167 |
| 15. `MASTER-020` closed by evidence; `TEST-001`'s seven corrections classified | T5 c2 | covered | **met** — all seven now `DRIFT` after round 2 |
| 16. Dispatcher residuals not folded into `MASTER-020` | T5 c3 | covered | **met** — filed as `MASTER-172`/`173`, closure disclaims them |
| 17. `TEST-019` falsification recorded, dead anchors replaced | T5 c4 | covered | **met** — two dead `:37` anchors replaced by three live ones |
| 18. `TEST-020`'s two never-true clauses classified `MEASUREMENT ERROR` | T5 c5 | covered | **met** |
| 19. `TEST-032` records both inversions incl. the measured regression | T5 c6 | covered | **met** — flagged **DO NOT IMPLEMENT IT** |
| 20. `MASTER-167` corrected; `UIKIT-004` not falsified | T5 c7, c8 | covered | **met** — `04-app-controls-and-theme.md` untouched |
| 21. Six new entries filed | T5 c9, c10 | covered | **met** — ranks 168–173, fields verbatim from the frozen contract |
| 22. 6b-iii preamble paragraph naming the four re-measured records | T5 c12 | covered | **met** — states the rest are "unchecked, which is not the same as correct" |
| 23. Every count carries its command; none anchored at `HEAD` | T5 c11 + T1 c8 / T2 c8 / T3 c7 / T4 c4 | covered | **met** — `git diff \| grep "^+" \| grep -c "HEAD"` → 0 |
| — Full solution suite green after all five tasks land | **none** | **uncovered — accepted: orchestrator between T5 and Gate 3** | **met** — orchestrator ran it at `7f3ecc5`: build `0 Warning(s), 0 Error(s)`; `dotnet test src/DungeonMaster.sln --no-build` → **866 passed, 0 failed** across three projects (`App.UnitTests` 23, `DungeonMaster.Tests` 750, `App.Tests` 93) |

23 of 23 covered items met, plus the uncovered row discharged by the orchestrator as planned.

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `Convert(value, targetType, parameter, culture)` | T3 | T3 | covered | **called** — `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:151` (and five further XAML sites). See the note below |
| `ConvertBack(...)` | T3 | T3 c4 | covered | **called** — asserted at `GreaterThanZeroConverterTests.cs:281`, `:297`; no production caller, which is correct for a one-way binding converter |
| `T1.evidence(MASTER-070)` | T1 | T5 c4 | covered | **called** — cited in `TEST-019` CORRECTION 4 |
| `T2.evidence(MASTER-086)` | T2 | T5 c5 | covered | **called** — cited in `TEST-020` CORRECTIONS 1–4 |
| `T3.evidence(MASTER-167)` | T3 | T5 c7 | covered | **called** — cited in the `MASTER-167` narrative amendment |
| `T4.evidence(MASTER-155)` | T4 | T5 c6 | covered | **called** — cited in `TEST-032` CORRECTIONS 1–4 |
| `T5.close(entry)` × 5 | T5 | **none in-session** | **uncovered — accepted: consumer is Gate 3's report and later index readers** | **called** — the five `Status` cells at `00-master-index.md` ranks 20, 70, 86, 155, 167 |
| file `MASTER-168`–`171` | T5 | T5 c9, c10 | covered | **called** — appended at ranks 168–171 |
| file `MASTER-172`, `173` | T5 | T5 c3, c9 | covered | **called** — appended at ranks 172–173 |

### Note on `Convert`'s call sites

The skill's mechanical check searches the session's touched files for the operation name. Run that way,
`Convert`'s only hits are its definition site and its tests, which would read `not called`. **That reading
would be wrong here, and recording it would be misleading.** The six binding call sites are pre-existing
XAML that this session deliberately did not touch — criterion 8 of T3 required exactly that. They were
enumerated and verified present this session:

`App.xaml:21` (resource declaration), `DungeonDetailPage.xaml:125`, `:131`, `:151`,
`DungeonListPage.xaml:100`, `:106`, `:115` — seven XAML lines, of which `:151` and `:115` pass
`ConverterParameter=Invert`.

Recorded as `called` on that evidence. The check exists to catch an operation with an implementer and no
consumer; that failure mode does not apply to a converter whose consumers predate the change to it.

**What a call site does not prove:** that a user reaches it. Both `Invert` consumers were measured this
session as **latent traps** — their bound paths resolve today, and both pages use compiled bindings, so the
defect was never firing in the running application. The guard prevents a future break; it does not fix a
visible one.

## Session change surface

`git diff --shortstat 1407090..7f3ecc5` → **34 files changed, 1445 insertions(+), 38 deletions(-)**,
across **5 task commits** (`6871900`, `aeac7b7`, `925fb55`, `5347813`, `7f3ecc5`).
