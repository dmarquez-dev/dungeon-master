# Handoff — Session 6b, "Rebuild the App sub-suite and de-duplicate the fixtures"

Written 2026-09-02, after Session 6a merged into `chore/repo-audit`. **Every figure below was measured on 2026-09-02 with the command shown, in the main checkout at `8167b84`.** Where something is inherited rather than measured, it says so.

> **Read this first.** Session 6a found **five defects in its own acceptance criteria** and **three tests that could pass while asserting nothing**. Not one was caught by reading or reasoning — every single one was caught by an agent running the check and looking at what came back. Treat this document the same way. It is a starting point for measurement, not a substitute for it.

## Where things stand

`chore/repo-audit` at **`8167b84`** — the 6a merge is `45bab59`, and `8167b84` is the artifact commit on top of it. **Pushed** to `origin` on 2026-09-02: `origin/chore/repo-audit` is at `8167b84`, range `e31d9f1..8167b84`, nine commits. The session worktree was removed; branch `dir/2026-09-01-teardown-and-lifetime` was kept, since the review files cite it. There is **no project-level `.dir-sdlc/log.md`** in this repository — the session log is `.dir-sdlc/sessions/2026-09-01-teardown-and-lifetime/log.md`, and it records every gate, dispatch and escalation. Main checkout clean and verified there, not only in the worktree:

```
dotnet build src/DungeonMaster.sln        Build succeeded.   0 Warning(s)   0 Error(s)
dotnet test src/DungeonMaster.sln --no-build
  Passed!  - Failed: 0, Passed: 640, Total: 640 - DungeonMaster.Tests.dll (net10.0)
  Passed!  - Failed: 0, Passed:  77, Total:  77 - DungeonMaster.App.Tests.dll (net10.0)
```

**There is still no aggregate test count.** A solution-wide `dotnet test` prints two per-assembly lines. Any criterion citing a count must cite it per assembly with the command that produced it.

## What 6a changed about what 6b can do

**A full MAUI application host runs in-process in `DungeonMaster.App.Tests` under `dotnet test`.** This is the single biggest change to your evidence surface, and it is already built and committed:

- `src/DungeonMaster.App.Tests/Hosting/MauiHostFixture.cs` — assembly-wide xunit fixture. Join with `[Collection(MauiHostCollection.Name)]`; do all MAUI work inside `host.Invoke(...)`. `host.Resolve<T>()` reaches the real DI container.
- It required two build changes, each **proved necessary by removing it and watching the host die**: `WindowsAppSdkBootstrapInitialize=true`, and an MSBuild target copying the app PRI to `testhost.pri` with a matching delete-on-clean target.
- **`MauiProgram.DatabasePathOverride`** redirects the database so the host does not migrate the user's real one. The fixture sets it before the UI thread starts. The application never sets it, and a production call site would be the defect.

**Two limits, both real:**

1. **This recipe is off Microsoft's documented path.** Microsoft specifies an `OutputType=Exe` self-hosted app under MTP and states "VSTest doesn't support this unpackaged WinUI configuration" (https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-winui#configure-an-unpackaged-test-app). What was measured here contradicts that note for this configuration — it works under VSTest with xunit 2.9.3. Know that you are off-path, and that xunit 2.9.3 cannot run under MTP if the recipe ever proves fragile.
2. **One host per process.** `Microsoft.UI.Xaml.Application.Start` can be called once, so the fixture is assembly-wide and every host-requiring test funnels through it. xunit parallelism against it has **not** been measured, nor whether two test classes dispatching concurrently onto the one dispatcher behave. **6b will add far more host tests than 6a did — this is the thing most likely to bite you.**

**The construction boundary, corrected.** The previous handoff attributed the no-host failure to `ContentView`. It is one level lower: **`Microsoft.Maui.Controls.Element`'s static constructor**. `BoxView` and even `Element.AutomationIdProperty` fail identically without a started WinUI `Application`. Full record: `spike-host-report.md` in this directory.

## Your scope — 26 entries

From `docs/audit/00-master-index.md`, the Session 6b section. **Count it yourself before quoting it** — the first draft of this handoff said 25, and was wrong. The arithmetic: Session 6's declared list held **29**; 6a closed five entries but only **four of them were on that list** (`MASTER-006`, `013`, `055`, `062`), because `MASTER-160` was never in it — it came in at Gate 1 as an accepted addition. So 29 − 4 = 25, plus `MASTER-167` assigned here at Session 6's Gate 1 = **26**, confirmed by extracting every `MASTER-\d+` from the Entries line and counting: 26 ids, no duplicates.

`MASTER-008`, `014`, `020`, `028`, `040`, `041`, `042`, `063`, `064`, `065`, `066`, `067`, `068`, `069`, `070`, `085`, `086`, `100`, `101`, `102`, `109`, `127`, `138`, `149`, `155`, `167`.

**Explicitly not yours, decided at Session 6's Gate 1:** `MASTER-161`, `163`, `164`, `165`, `166` were left out of Session 6 entirely — they are sidebar and UI defects, and folding them into a test-suite session is the theme collision that got Session 5 split. `MASTER-166` additionally needs a reproduction before it can be scoped at all. Do not let them drift in because a suggested-home line named a session number.

**`MASTER-065` and `MASTER-014` are named in the index as one unit** — the shared fixture and the `EnsureCreated` schema are the same four files.

## The five criterion defects 6a found, and the one rule that would have caught two of them

This is the most transferable thing 6a produced. All five are recorded with evidence in `reviews/T*.md`, and all four substantive ones were **amended in the frozen `manifest.json` at Gate 3**, each marked inline with the finding and the evidence.

1. **An injection that does not discriminate.** Two criteria said: remove a guard flag, watch the named test go red. The test stayed **green** — the same code arm also called `_timer.Stop()`, and a stopped `FakeLongPressTimer` ignores `Tick()`, so the guard was never reached. This happened **twice**, in two different files, and both tasks independently had to write a re-armed-timer test to isolate the flag.

   > **The rule, adopted at Gate 3: before writing a criterion that prescribes an injection, run that injection against the test you are about to name.** If the named test stays green, the criterion certifies nothing. Where the code has two mechanisms that suppress the same outcome, disabling one proves nothing about the other.

2. **A criterion naming a type that does not exist.** `Microsoft.Maui.Controls.Internals.IPanGestureController`. The interface is public at `Microsoft.Maui.Controls.IPanGestureController`; no `Internals` type of that name exists in `Microsoft.Maui.Controls.dll`. Reflection over the built assembly settled it.
3. **An arrangement that cannot produce its own evidence.** A criterion said "set `page.Handler = null` and assert the drawable is disposed". MAUI's `Element` handler setter **returns early when assigned the value it already holds**, and a DI-resolved page's handler is already null — so that assignment raises no `OnHandlerChanged` at all. A minimal `IViewHandler` must be connected first, then nulled. (`IElementHandler` does not compile: `VisualElement.Handler` is typed `IViewHandler`.) A reviewer proved the connect step load-bearing by deleting it against correct code and watching the test fail.
4. **Evidence in a form the tool does not print.** A criterion asked for a per-TFM `0 Warning(s) / 0 Error(s)` pair. MSBuild emits **one aggregate** figure for a multi-TFM build. Both TFM assemblies are emitted and the aggregate zero covers both, so the substance held — but the criterion asked for something that does not exist.
5. **A vacuity trap the criteria never anticipated.** `ObjectAssetCatalogue.Paths` is **empty in the test process**: `SetPaths` was `internal` and its only production caller needs a MAUI host. Four disposal criteria would have passed over an empty cache. 6a fixed the access (`InternalsVisibleTo`, commit `ff45a45`) and the arrangement helper is `ObjectCatalogueScope` in `src/DungeonMaster.App.Tests/Graphics/FakeAssetSource.cs`. **If you construct a `FloorMapDrawable` and expect it to hold images, you need that arrangement.**

## Three tests that pass while asserting nothing — the shape to hunt

All the same trap: **an enumeration-based assertion over an empty collection**. `Assert.All(...)` and a bare `foreach` wrapping the assertions both no-op on empty input.

- `MauiHostFixtureTests.EveryExceptionTheHostMarksHandledIsRecordedWithATypeAndAMessage` — both assertions inside a `foreach` over a collection that is always empty today. It executes no assertion at all.
- `FloorMapDrawableDisposalTests.ClearObjectImageCache_...` and `.Dispose_CalledTwice_...` — both rest their real claim on `Assert.All(harness.Assets.Decoded, ...)`, leaving only a trivial `IsDisposed` check when the catalogue is empty.

**6b is a test-suite session. This shape is your primary quarry**, alongside the `FloorMapDrawableAllocationTests` blind spot already in `.dir-sdlc/backlog.md`, whose permanent fact is relative-only and passes an unconditional per-frame allocation regression.

## What 6b inherits in the backlog

`.dir-sdlc/backlog.md` carries **four items dated 2026-09-02** from 6a, all four carried at Gate 3 by the user's decision (`grep -c '^- 2026-09-02' .dir-sdlc/backlog.md` → 4). Twelve older items sit in the two other `### For a later session` sections — five carried from 5b and re-deferred by 5c, seven from 5c. The file holds **45 dated items in total** across all its sections, most of them repository work and audit-document corrections rather than deferred session work, so do not quote 45 as "the backlog" without saying which section you mean. The four from 6a:

- **`MauiHostFixture.InvokeAsync` has an implementer and no caller** — both overloads. Kept at Gate 2 as surface provisioned for a task that turned out not to need them. **6b either gives them a caller or removes them.**
- **The enumeration-assertion trap**, with its three sites and the general rule.
- **`HandledExceptions`' capture path is unexercised** — nothing has shown the recorder fires. Related and unexplained: 6a measured that the startup mask is **not** load-bearing under this fixture, contradicting what the spike measured for its own probe. Nobody has explained the divergence, and 6a deliberately claimed nothing about the spike's unidentified `InvalidOperationException`.
- **`ShippedPackKinds` duplicates `pack.json`** as a hand-maintained 29-entry literal that no test compares against the manifest.

**Two different numbers that look like the same number:** **29** is the loadable kind count from the single declared pack (`pack.json`); **37** is the `ObjectKind` enum member count. Session 4's outcome cites 37, correctly, for a different quantity. Do not carry either forward unrecounted — recount with the command and name it.

## Environment traps — all measured, all cost time

**Create the worktree under a short root: `C:\dm-wt\<slug>`.** The scratchpad path is ~184 chars, which pushes `input.json` past `MAX_PATH` (260); the net472 WinUI `XamlCompiler.exe` then fails with a **misleading** `input.json doesn't exist!`. `LongPathsEnabled` is already `1` and does not help. 6a used `C:\dm-wt\2026-09-01-teardown-and-lifetime` and never hit it.

**Never check line endings with `grep`, in any form.** `grep -c '^\t'` returns **0** on a fully tab-indented file. `grep -cv $'\r$'` returned the full line count in one shell and **0** in two others, on the same files proven all-CRLF. Compare `tr -cd '\r' | wc -c` against `tr -cd '\n' | wc -c`, or use `file`. **This fired again in 6a**, twice: once when an agent hit the leading-tab form live, and once when the orchestrator inserted an LF-only block into the CRLF master index and caught it only because it ran the CR/LF comparison afterwards.

**`sed -i` rewrites files as LF**, tripping `IDE0055` under `TreatWarningsAsErrors`. Pasting an LF block into a CRLF file does the same thing.

**`git checkout -- <file>` restores to HEAD, which predates uncommitted task work.** Every 6a agent was told to save a byte copy before any temporary edit and restore from that copy with `cmp`. All of them did, and several made three or four injections each. Keep the rule.

**Concurrent peer builds in one worktree produce spurious failures.** 6a measured five distinct ones — `Package.appxmanifest` not found, `input.json` in use, `CS0006`, `CS2012` on a locked `intermediatexaml` DLL, and an aborted run missing `testhost.dll`. All cleared on a plain retry. **6a therefore serialized its last two tasks** even though their ownership globs were disjoint: disjoint ownership prevents file conflicts, not build collisions, and the framework's concurrency rule does not cover this. A reviewer was also standing-ordered never to report a build failure without retrying it once.

**Every XAML file violates its own `.editorconfig`** (`[*.xaml]` → `indent_style = tab`; the files are space-indented). `IDE0055` does not reach XAML, so nothing catches it. **Match the file, never retab.**

**macCatalyst is parked (`ADR-001`)** — it builds but cannot be run here. No criterion may demand observed macCatalyst behaviour.

**Probes lie in both directions**, and `HandCursorBehavior` sets WinUI's non-public `ProtectedCursor` per element, which another process cannot read — cursor claims need a human. Both lessons carry unchanged from the 6a handoff.

## Process lessons worth carrying verbatim

1. **Hand agents the artifact, not your summary of it.** Every 6a dispatch pointed the agent at `manifest.json` and told it to read its own `acceptance` array verbatim, with a standing line that the manifest wins over the brief and any conflict is to be reported. **Zero defects came from orchestrator briefs in 6a**, against four in the previous sub-session, which is the only change that was made.
2. **Give every agent the standing rule: implement what is correct, and report the discrepancy with the command that showed it.** All five criterion defects surfaced this way. None was found by anyone reasoning about a criterion.
3. **Have the reviewer reproduce the demonstration rather than accept it**, and prefer a *different* injection. 6a's T3 reviewer wrapped a tick in `Task.Run` where the implementer had swapped the timer type; both produced the same thread-id divergence, which is much stronger than one of them alone.
4. **Distinguish a correction from a non-correction.** One 6a agent reported a "path correction" for a path no brief contained. It was recorded as **not** a record correction. Two sub-sessions before this one produced corrections that were themselves wrong; a wrong correction in the record is worse than none, because the next reader has no reason to doubt it.
5. **Verify in the main checkout after the merge, not only in the worktree.** A clean worktree does not prove a clean main checkout, and line-ending state diverges invisibly between them.
6. **Ask the user to look.** Unchanged from the previous handoff, and still the cheapest source of ground truth this project has.

## Suggested shape

26 entries is close to the size at which both Session 5 and Session 6 were split. **Take the split decision at Gate 1, before decomposition, not after.** A natural seam is the `MASTER-014` + `MASTER-065` fixture/schema unit against the broader `App/` sub-suite rebuild, with `MASTER-167` — an inverted `GreaterThanZeroConverter` sentinel, a plain class, host-free and directly testable — as its own small piece.

Sequence the xunit-parallelism question early, the way 6a sequenced the host question: **whether two host-requiring test classes can coexist under one assembly-wide fixture determines how much of 6b's suite can use the host at all**, and 6a did not measure it.

Finally: this session **owns no index reconciliation for a prior session.** 6a marked its own five rows and wrote its own outcome record. Session 6b marks only its own.
