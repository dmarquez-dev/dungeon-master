# Session 4 — Async, disposal and threading discipline in the presentation layers

**Goal.** Apply one discipline across the App and Infrastructure layers — no `async void` outside a
guarded handler, no discarded task, no sync-over-async — closing seven of the eleven audit entries the
master index batches as Session 4. The other four were rescheduled at Gate 2; see `## Not in scope`.

## Context

`docs/audit/00-master-index.md` plans seven fix sessions against a 2026-08-17 audit of this solution.
Sessions 1, 2 and 3 are complete and merged on `chore/repo-audit`; this is Session 4, sequenced after
Session 3 because a shorter-lived `DbContext` changes what an unobserved task can corrupt, and that
lifetime is now settled.

**The audit text predates Sessions 1–3 and is treated as a set of claims to re-verify, not facts.**
Decomposition found five places where it did not survive contact with the code — recorded in
`## Assumptions` below and carried into the acceptance criteria with the commands that produce the
corrected numbers.

**The constraint that shapes this session.** `src/DungeonMaster.Tests` targets `net10.0` and cannot
reference the multi-targeted `DungeonMaster.App` project — audit entry `XCUT-002` / `MASTER-026`,
which is Session 6's work and explicitly out of scope here. No test in this repository executes App
code. App-layer tests therefore use the `RepoPaths` source-scanning idiom
(`src/DungeonMaster.Tests/App/Theme/RepoPaths.cs`), which reads source text from disk.

**That constraint is what resized the session.** Four entries — `MASTER-006`, `MASTER-013`,
`MASTER-055`, `MASTER-062` — are three leaks and a memory-visibility race. Nothing here executes App
code and none of the four is visible in a running app, so structural assertion was the only evidence
available for them. At Gate 2 that was judged insufficient for that defect class and all four moved to
Session 6, where `MASTER-026` lands and they can be demonstrated rather than asserted.

## Definition of done

Entry-level outcomes:

1. **MASTER-018** — the three `async void` methods that are not event handlers (`EraseAreaAsync`,
   `PlaceObjectAsync`, `FinishDrawing`) are `async Task` and every caller awaits them.
2. **MASTER-019** — every `async void` unit in the App layer is guarded: a `try` before the first
   `await` whose `catch` routes the failure to an error channel. This covers the named handlers the
   audit lists **and** the six `async` event lambdas no audit entry records.
3. **MASTER-024** — no line in `src/DungeonMaster.App` has a trimmed form starting with `_ =`.
4. **MASTER-025** — `FloorMapDrawable`'s `Draw` path performs no file I/O and no blocking wait;
   images are loaded by an awaited `PreloadObjectImagesAsync()` the page invokes, and
   `LoadOrGetImage` is a pure cache read.
5. **MASTER-013, the `ClearObjectImageCache()` half only** — that method disposes each non-null cached
   `IImage` before clearing. `IDisposable`, `Dispose()` and its call site remain **Session 6's**. This
   half lands here solely because item 4 would otherwise amplify the leak it defers; see
   `## Assumptions`. Nothing in this session may describe `MASTER-013` as closed.
6. **MASTER-038** — `DmConfirmPopup`'s `OnConfirm` and `OnCancel` await `CloseAsync` under a guard
   instead of discarding it.
7. **MASTER-039** — `HandCursorBehavior` resolves the `ProtectedCursor` `PropertyInfo` once into a
   `static readonly` field and caches one `InputSystemCursor` per shape, rather than reflecting per
   pointer event.
8. **MASTER-153** — every `await` in the four Infrastructure files carries `.ConfigureAwait(false)`,
   and a source-scanning guard test asserts it. Nothing in the build enforces this — CA2007 is not
   enabled by default in .NET 10 — so that test is the only enforcement and must say so.

Session-level outcomes:

9. `src/DungeonMaster.Tests/App/AsyncDiscipline/` encodes rules **G1–G4** of
   `contracts/async-discipline-rules.md` as source-scanning guards, each demonstrated to discriminate
   by an inline fixture that violates the rule and one that satisfies it, and each applied to real App
   source through `RepoPaths`. G5–G8 were withdrawn with the four rescheduled entries.
10. Every guard test class states in its XML `<summary>` that it reads source text and never executes
    App code, and no test name or summary claims a leak, race or crash path is absent.
11. **No regression.** `dotnet test` passes with the arithmetic stated: 448 pre-existing passing tests
    (measured at `e795365`) plus the tests this session adds equals the observed total.
12. **Build clean at 0 warnings for both App target frameworks** (`net10.0-windows10.0.19041` and
    `net10.0-maccatalyst26.5`) under `TreatWarningsAsErrors`. New `.cs` files are tab-indented and
    CRLF-terminated.
13. `docs/audit/00-master-index.md` and `docs/TODO.md` are reconciled against what actually landed —
    Status cells set from task reports rather than from this plan, the `## Progress` row for Session 4
    updated with real commit shorthands, and the four-entry reassignment to Session 6 recorded in both
    session blocks and both row counts.

## Scope

`src/DungeonMaster.App` — `Graphics/FloorMapDrawable.cs`, `Behaviors/HandCursorBehavior.cs`,
`Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`, `Views/DungeonDetailPage/ObjectPaletteView.xaml.cs`,
`Views/CreateDungeonPopup/CreateDungeonPopup.xaml.cs`, `Views/DungeonListPage/DungeonListPage.xaml.cs`,
`Controls/DmConfirmPopup/DmConfirmPopup.xaml.cs`.

`src/DungeonMaster.Infrastructure` — `DependencyInjection.cs`, `Persistence/UnitOfWorkFactory.cs`, and
the two repositories.

`src/DungeonMaster.Tests` — two new directories, `App/AsyncDiscipline/` and `Infrastructure/Async/`.

`docs/audit/00-master-index.md` and `docs/TODO.md`, for reconciliation only.

### Not in scope

- **`MASTER-006`, `MASTER-013` (beyond item 5), `MASTER-055` and `MASTER-062`** — rescheduled to
  Session 6 at Gate 2. `src/DungeonMaster.App/Behaviors/CanvasLongPressDetector.cs` and
  `HoverOrLongPressBehavior.cs` are consequently owned by **no task** and must not be edited.
- **`XCUT-002` / `MASTER-026`** — making the test project able to reference the App project. Session 6.
  This is the constraint the session is designed around, not a problem it solves.
- **The stale-state half of `docs/TODO.md`'s "Designer async-lifecycle hardening"** — continuations
  writing into a floor that is no longer selected. The `async void` half is this session's; the
  re-validation half stays open and must be left explicitly open.
- **Any change to entity property types.** `docs/persistence-contract.md` governs those, and Session 3
  left the UTC timestamp invariant Domain-only and undefended — a `.ToLocalTime()` introduced in a
  view model would now shift silently. No task touches this.
- **View models.** `CreateDungeonViewModel`, `DungeonListViewModel` and `DungeonDetailViewModel` are
  read for their `ErrorMessage` channel and not edited.
- Behaviour change of any kind beyond the seven entries in scope.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| How many discarded-task sites does `DungeonDetailPage.xaml.cs` have? | **12**, not the eight `UI-015` records — the audit collapses the hit-zone block at 1209–1224 into one location | 1 | `grep -c "_ = " src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` → 12; sites at 143, 163, 351, 369, 388, 1215, 1217, 1219, 1221, 1223, 1522, 1662 |
| Are there discard sites no audit entry records? | **Yes, two** — `CreateDungeonPopup.xaml.cs:25` and `:28` | 1 | `grep -rn "_ = " --include=*.cs src/DungeonMaster.App` → 16 total; `UI-015` records only `DmConfirmPopup`'s two |
| Are there unguarded `async void` units no audit entry records? | **Yes, six** `async` event lambdas, all in `DungeonDetailPage.xaml.cs` at 659, 736, 761, 789, 1871, 2155. `UI-014` enumerated named methods only | 1 | `grep -rn "+= async" --include=*.cs src/DungeonMaster.App` → exactly 6 |
| How many `await`s does Infrastructure have, and in how many files? | **9 awaits across 4 files**, not the two files `INFRA-015` names. `UnitOfWorkFactory.cs` did not exist when the audit was written | 1 | `grep -rn "await " --include=*.cs src/DungeonMaster.Infrastructure --exclude-dir=obj --exclude-dir=Migrations \| wc -l` → 9 |
| Do `ObjectPaletteView`'s handler line numbers still match `UI-014`? | **No** — 143 and 154, not the 137 and 148 recorded | 1 | `sed -n '143p;154p' src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs` → both `async void` handlers |
| Can a rule simply ban `.Result`? | **No.** `NavigationExtensions.cs:56` is `return result.Result;` on an `IPopupResult<T>` — an ordinary property. G4 bans `GetAwaiter().GetResult()` and `.Wait()` only | 1 | `sed -n '56p' src/DungeonMaster.App/Extensions/NavigationExtensions.cs` |
| Must the scanners exclude comments? | **Yes, as a hard requirement.** `FloorMapDrawable.cs:291` is a comment containing the literal banned token | 1 | `grep -n "GetAwaiter" src/DungeonMaster.App/Graphics/FloorMapDrawable.cs` → 291 (comment) and 304 (the real call) |
| **Does fixing `MASTER-025` make `MASTER-013`'s leak worse?** | **Yes, materially.** `LoadOrGetImage` populates `_imageCache` lazily — one entry per kind actually drawn. `PreloadObjectImagesAsync()` populates an entry for every kind the active pack declares, and `ObjectKind` has **37** members. Images abandoned per `ClearObjectImageCache()` call go from "the kinds the user drew" to up to the whole pack, and that method runs on every pack change. This is why item 5 lands despite `MASTER-013` being rescheduled | 1 | `grep -cE "^\s+[A-Z][A-Za-z]*\s*(=\|,)" src/DungeonMaster.Domain/Entities/Floor/ObjectKind.cs` → 37; `ObjectAssetCatalogue.cs:14` keys `Paths` by `ObjectKind`; `LoadOrGetImage` called only from `FloorMapDrawable.cs:354,447`; `ClearObjectImageCache()` called from `DungeonDetailPage.xaml.cs:100` |
| **Is there a best-practice reason not to apply `ConfigureAwait(false)` in this Infrastructure project?** | **No.** CA2007's documented hazard is *application* code — it names "button click event handlers in a WinForms or WPF project" — which is `DungeonMaster.App`, not `DungeonMaster.Infrastructure`. The guidance for a non-UI class library is to use it on every await unless the code needs the captured context, and Infrastructure does not. CA2007 even ships an `output_kind` option separating `DynamicallyLinkedLibrary` from UI apps | 3 | [CA2007](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2007); [ExecutionContext and SynchronizationContext](https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/executioncontext-synchronizationcontext) |
| Will the build enforce the new `ConfigureAwait` convention? | **No.** CA2007 is not enabled by default in .NET 10 and is not in Session 1's `AnalysisMode: Recommended` set. T6's guard test is the only enforcement; delete it and the convention silently stops being one | 3 | [CA2007](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2007) — "Enabled by default in .NET 10: No" |
| What is the error channel where a control has no view model? | `Debug.WriteLine` — the App layer's established last-resort channel | 1 | `src/DungeonMaster.App/MauiProgram.cs:166`, `Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:109` |
| Is the no-regression floor still 448? | **Yes**, as of `e795365` | 1 | Session 3's Gate 3 record, `.dir-sdlc/sessions/2026-08-18-settle-the-data-access-lifetime/reviews/T1.md`; `docs/audit/00-master-index.md` `## Progress` |
| Can an implementer agent launch the MAUI app on Windows and observe it? | **Yes.** Session 3's T3 ran the app, observed the detail page and screenshotted it. A manual-observation criterion is reachable, provided the observing task owns the file that drives the observation | 1 | `.dir-sdlc/sessions/2026-08-18-settle-the-data-access-lifetime/reviews/T3.md` criterion 4 and the B1 resolution |
| Is there a project discipline config or fragment set? | **No.** `.dir-sdlc/config.json` and `.dir-sdlc/disciplines/` do not exist, so documented defaults apply and each discipline is dir-ect's bundled base alone | 1 | `ls -a .dir-sdlc/` → `backlog.md`, `sessions` only |

## Contracts

- `contracts/async-discipline-rules.md` — rules G1–G4, the session's single discipline, stated so a
  source scanner can check each one. G5–G8 are recorded there as withdrawn. Consumed by T1 (encodes
  them), T2, T3, T4, T5 (satisfy them).
- `contracts/floor-map-drawable.md` — freezes `FloorMapDrawable`'s public surface
  (`ClearObjectImageCache()`, `PreloadObjectImagesAsync()`) so the drawable's fix and its call sites
  can be written against one agreed shape. `IDisposable`/`Dispose()` are recorded as Session 6's.
- `contracts/infrastructure-configureawait.md` — the `MASTER-153` rule, the Gate 2 decision and the
  evidence it rests on, and the record that nothing in the build enforces it.

## Open questions

None. Both Gate 2 escalations and the one follow-on escalation are resolved:

| Escalation | Resolution |
|---|---|
| E1 — apply `ConfigureAwait(false)` or decline? | **Apply.** Settled on first-party evidence rather than preference; see the Assumptions rows above. T6 runs. |
| E2 — accept structural-only evidence for four entries, or reschedule? | **Reschedule** `MASTER-006`, `MASTER-013`, `MASTER-055`, `MASTER-062` to Session 6. |
| E3 — the preload amplifies the leak E2 just deferred | **Dispose on clear, keep the rest in Session 6.** Three lines inside T2's own file; `MASTER-013` is partly landed and T7 records exactly which half. |

Residual risk, not a question: if a manual-observation run fails for an environmental reason, that
criterion returns unmet by design and the observation falls to the orchestrator at Gate 3.
