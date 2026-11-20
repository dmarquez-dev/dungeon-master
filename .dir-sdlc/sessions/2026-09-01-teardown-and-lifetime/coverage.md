# Coverage — 2026-09-01-teardown-and-lifetime

Gate 2 columns written before the approval that froze `manifest.json`. End state written at Gate 3, from `reviews/T*.md` and from a call-site search over the session's touched files (`git diff --name-only 55fc7da..dir/2026-09-01-teardown-and-lifetime`, 21 files).

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. `MASTER-006` — pan recogniser removed on detach, demonstrated | T4 (criteria 1–4) | covered | **met** — attach 2 / detach 0 under the host; reviewer reproduced `Actual: 1` with the teardown reverted |
| 2. `MASTER-013` drawable half — `FloorMapDrawable` is `IDisposable`, disposal observable | T2 (criteria 1–3) | covered | **met** — all 29 cached images disposed including never-drawn kinds; second `Dispose()` a no-op |
| 3. `MASTER-013` page half — page disposes the drawable at handler teardown | T5 (criteria 1–3) | covered | **met** — reviewer reproduced the red step, and separately proved the arrangement non-vacuous (see C1 in `reviews/T5.md`) |
| 4. `MASTER-055` — scroll-zoom unsubscribe uses a captured reference | T5 (criteria 4–5) | covered | **met** — `DetachScrollZoom` reads no `Handler`/`PlatformView`; reviewer reproduced the red step by making `Detach()` re-resolve |
| 5. `MASTER-062` — no cross-thread read of the long-press flag | T3, T4 (criteria 5–7), T5 (criteria 6–7) | covered | **met** — `System.Timers.Timer` and the `MainThread` hop are gone from both consumers; tick delivered on the calling thread, proven by thread-id equality |
| 6. `MASTER-062` — the race is **not** claimed as demonstrated | T3 crit. 6, T4 crit. 8, T5 crit. 8 | covered | **met** — stated in three reports and in code comments at both changed sites; no artifact claims reproduction |
| 7. `MASTER-160` — no undisposed image overwritten by `null` | T2 crit. 4 | covered | **met** — reviewer restored the old shape and reproduced the single-test failure |
| 8. E2 — the test run does not write the user's real database | T1 crit. 6 | covered | **met** — SHA-256 and mtime of `%LOCALAPPDATA%\DungeonMaster\dungeon-master.db` unchanged across a reviewer-run suite; per-run temp databases confirmed created instead |
| 9. Host fixture reproducible; `testhost.pri` is a build step | T1 crit. 1 | covered | **met** — reviewer ran a third clean/build cycle; absence after clean is attributable to `RemoveTestHostPriOnClean` |
| 10. Build clean, 0 warnings, both App TFMs | T1 c8, T2 c8, T3 c7, T4 c9, T5 c9 | covered | **met** — both TFMs, `0 Warning(s) / 0 Error(s)`, reproduced by reviewers at T2, T3, T4 and T5 |
| 11. Both suites green, quoted per assembly | T1 c9, T2 c8, T3 c7, T4 c9, T5 c10 | covered | **met** — orchestrator's own final run: `Failed: 0, Passed: 640, Total: 640 - DungeonMaster.Tests.dll (net10.0)` and `Failed: 0, Passed: 77, Total: 77 - DungeonMaster.App.Tests.dll (net10.0)` |
| 12. Tab indentation and CRLF preserved | T1 c10, T2 c9, T3 c8, T4 c10, T5 c11 | covered | **met** — CR/LF counts equal per file across all 21 touched files; no `.xaml` file was modified |

No uncovered definition-of-done rows at Gate 2; **all twelve met at Gate 3.**

## Contract operations

`Implementing task` filled from the definition site the Gate 3 search located.

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `MauiHostCollection.Name` | T1 | T3, T4, T5 | covered | **called** — `LongPressTimerTests.cs`, `HoverOrLongPressBehaviorTests.cs`, `DungeonDetailPageTeardownTests.cs` |
| `MauiHostFixture.Invoke(Action)` / `Invoke<T>` | T1 | T3, T4, T5 | covered | **called** — same three files |
| `MauiHostFixture.Resolve<T>()` | T1 | T1, T5 | covered | **called** — `DungeonDetailPageTeardownTests.cs` (T5, a different task than the definition owner) |
| `MauiProgram.DatabasePathOverride` | T1 | T1 | covered | **called by the fixture only** — see the note below; the application never sets it, which is the intent |
| `MauiHostFixture.InvokeAsync(Func<Task>)` | T1 | — | uncovered — kept at Gate 2, provisioned for T5's asynchronous preload path | **not called** |
| `MauiHostFixture.InvokeAsync<T>(Func<Task<T>>)` | T1 | — | uncovered — kept at Gate 2, provisioned for T5's asynchronous preload path | **not called** |
| `FloorMapDrawable()` (parameterless) | T2 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs` |
| `FloorMapDrawable(Func<FloorMapPalette>)` | T2 | T2, existing Graphics suite | covered | **called** — pre-existing `App.Tests/Graphics` suite |
| `FloorMapDrawable(palette, streamSource, imageFactory)` | T2 | T2 | covered | **called** — `FloorMapDrawableDisposalTests.cs` |
| `FloorMapDrawable.IsDisposed` | T2 | T2, T5 | covered | **called** — `DungeonDetailPageTeardownTests.cs` (T5) |
| `FloorMapDrawable.Dispose()` | T2 | T2, T5 | covered | **called** — `DungeonDetailPage.xaml.cs`, production |
| `FloorMapDrawable.ClearObjectImageCache()` | T2 | T2, T5 | covered | **called** — `DungeonDetailPage.xaml.cs`, production |
| `FloorMapDrawable.PreloadObjectImagesAsync()` | T2 | T2, T5 | covered | **called** — `DungeonDetailPage.xaml.cs`, production |
| `ILongPressTimer.Start` / `Stop` / `Elapsed` / `Dispose` | T3 | T4, T5 | covered | **called** — `HoverOrLongPressBehavior.cs`, `CanvasLongPressDetector.cs`, production |
| `DispatcherLongPressTimer(IDispatcher)` | T3 | T4, T5 | covered | **called** — `HoverOrLongPressBehavior.cs`, `CanvasLongPressDetector.cs`, `DungeonDetailPage.xaml.cs`, production |
| `FakeLongPressTimer.Tick()` / `IsRunning` | T3 | T4, T5 | covered | **called** — `HoverOrLongPressBehaviorTests.cs`, `CanvasLongPressDetectorTests.cs` (both different tasks than the definition owner) |
| `CapturedSubscription<T>.Attach()` / `Detach()` | T5 | T5 | — | **called** — `DungeonDetailPage.xaml.cs`, production |

### Two honest notes on how this table was filled

**The test-file exclusion does not mean here what it usually means.** The standing rule discards hits inside test files, because an operation exercised only by its own tests is not reached by the application. **This session's deliverable is partly test infrastructure by design** — the host fixture, `ILongPressTimer`'s fake, and `FloorMapDrawable.IsDisposed` exist to be consumed by tests, and Gate 2 approved them on that basis. For those operations the table records the consuming test file and names the task, so a reader can see the consumer is a different task than the definition owner rather than the same task marking its own homework. Where an operation is reached from production code, the table says `production` explicitly. Six operations are reached from `DungeonDetailPage.xaml.cs`, `HoverOrLongPressBehavior.cs` or `CanvasLongPressDetector.cs` — genuine application call sites.

**`MauiProgram.DatabasePathOverride`'s only consumer is a file owned by the same task that defines it** (both `MauiProgram.cs` and `MauiHostFixture.cs` are T1's). Under the strict rule that reads as `not called`. **That is the correct and intended state**: the application must never set this property, and a production call site would be the defect. Recorded as it is rather than as a clean `called`.

### The one genuine gap

**Both `MauiHostFixture.InvokeAsync` overloads are `not called`.** They were the surface kept at Gate 2 on the expectation that T5's asynchronous page-preload path would need them; it did not. A whole-word search across the session's 21 touched files finds `InvokeAsync` only at its own definition site in `MauiHostFixture.cs` (the other hits in the repository are an unrelated `Dispatcher.InvokeAsync` in the Application layer). No task calls either overload.

This is exactly the finding the artifact exists to produce: every task passed every criterion, and this operation has an implementer and no caller. It is not a defect in anything shipped — the fixture works and is exercised — but the two overloads are unproven code, and the Gate 2 judgement that T5 would need them was wrong.
