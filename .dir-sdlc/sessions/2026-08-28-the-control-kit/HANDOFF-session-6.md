# Handoff — Session 6, "Rebuild the App sub-suite and de-duplicate the fixtures"

Written 2026-09-01, after sub-session 5c merged and `chore/repo-audit` was pushed. **Every figure below was measured on 2026-09-01 with the command shown, in the main checkout at `e31d9f1`.** Where something is inherited rather than measured, it says so.

> **Read this first.** The 5b→5c handoff carried two claims that did not survive measurement, and one of its *corrections* was itself wrong. This session then found **six defects in its own criteria** and **four in briefs the orchestrator wrote** — every one caught by an agent opening the cited source instead of trusting the citing claim. Treat this document the same way. It is a starting point for measurement, not a substitute for it.

## Where things stand

`chore/repo-audit` at **`e31d9f1`**, **pushed** to `origin` (`https://github.com/dmarquez-dev/dungeon-master`). Main checkout clean. Session 5's worktree removed; branch `dir/2026-08-28-the-control-kit` kept, since the review files cite it.

**Sessions 1–5 are complete.** Session 5 ran as three sub-sessions (5a, 5b, 5c); 5c reconciled the index for all of Session 5, including 5b's fourteen rows, which 5b never marked.

**Note on a claim the previous handoff got wrong:** it said "nothing is pushed — the whole of Sessions 1–5b is local". `chore/repo-audit` was in fact already on the remote at `702ff76` ("chore(build): pin .NET SDK version to 10.0.400"), an ancestor of Session 5's base. The 2026-09-01 push was a clean fast-forward.

Baseline, `dotnet build` / `dotnet test src/DungeonMaster.sln --no-build` from the repository root:

```
Build succeeded.   0 Warning(s)   0 Error(s)      both App TFMs
Passed!  - Failed: 0, Passed: 640, Total: 640 - DungeonMaster.Tests.dll (net10.0)
Passed!  - Failed: 0, Passed:  30, Total:  30 - DungeonMaster.App.Tests.dll (net10.0)
```

**There is still no aggregate test count.** A solution-wide `dotnet test` prints two per-assembly lines. Any criterion citing a count must cite it per assembly with the command that produced it.

## Session 6's declared scope — 29 entries, all still open

From `docs/audit/00-master-index.md:445-449`. Measured status of each, 2026-09-01:

| Status | Entries |
|---|---|
| **empty** (23) | `008`, `014`, `020`, `028`, `040`, `041`, `042`, `063`, `064`, `065`, `066`, `067`, `068`, `069`, `070`, `085`, `086`, `100`, `101`, `102`, `109`, `127`, `138`, `149`, `155` |
| `rescheduled to Session 6` (3) | `006`, `055`, `062` |
| `part done -- rescheduled to Session 6` (1) | `013` |

Severity spread: **6 × S2** (`006`, `008`, `013`, `014`, `020`, `028`), 19 × S3, 2 × S4, 2 × S5.

**`MASTER-013` is half-done.** Session 4 landed `ClearObjectImageCache()` disposing each cached image before clearing; the page-teardown half — `IDisposable`, `Dispose()`, and the call site — is Session 6's. Do not read its row as untouched.

**Why the four rescheduled entries are here:** Session 4 could produce no evidence for them. `DungeonMaster.Tests` targets `net10.0` and could not reference the multi-targeted App project, so the only available evidence was a structural assertion over source text, which the user judged insufficient for three leaks and a memory-visibility race. **That constraint is now partly lifted** — see the next section.

## The evidence situation, which is the whole reason this session is sequenced here

Session 5a built two things that change what Session 6 can prove:

- **`DungeonMaster.App.Core`** — `net10.0`, MAUI-free, referenced by both App and `DungeonMaster.Tests`. Anything extractable to it is testable by the main suite.
- **`DungeonMaster.App.Tests`** — single-targeted `net10.0-windows10.0.19041.0`, so it *can* reference the App project and execute App code. It has **30 tests** as of 5c, up from 1 at the start of 5c.

**The limit that still stands, and it shaped every task in 5c:** `DungeonMaster.App.Tests` runs **outside a MAUI application host**. `MauiProgram.CreateMauiApp()` is never called, there is no `Application.Current`, and **nothing deriving from `ContentView`/`ContentPage`/`Shell` can be constructed**. 5c's T7 measured the exact failure when it tried:

```
System.TypeInitializationException : The type initializer for 'Microsoft.Maui.Controls.Element' threw an exception.
---- The type initializer for 'Microsoft.Maui.Handlers.ViewHandler' threw an exception.
-------- System.Runtime.InteropServices.COMException
   at Microsoft.UI.Xaml.Input.FocusManager.add_GotFocus(...)
```

**Whether that half is reachable is explicitly Session 6's question** — the index says so at the Session 5a outcome. If Session 6 can stand up a MAUI host in the test process, it unlocks the four rescheduled entries and a great deal else. **Establish that early**, because the answer determines what the rest of the session can even claim.

**What 5c proved is possible without a host:** plain classes construct fine (converters — T1), classes that only touch non-theme code construct fine (`FloorMapDrawable.ScreenToCanvas`), and a class whose theme dependency is behind a seam becomes constructible (T7 added a `Func<FloorMapPalette>` constructor overload precisely to make an allocation measurement reachable). That last one is a pattern, not a one-off: **an injected seam is how you make App code testable without a host.**

## Scope decisions waiting at Gate 1

**1. Five of the six entries 5c filed suggest Session 6 as their home, and three of them do not fit Session 6's theme.**

| Entry | Sev | Suggested home says | Fits "rebuild the test suite"? |
|---|---|---|---|
| `MASTER-163` drag handle under the overlay scrollbar | S2 | "Session 6, with the rest of the sidebar's reachability work" | **No** — a UI defect |
| `MASTER-164` floor card body takes no keyboard focus | S2 | "Session 6, with `MASTER-163`" | **No** — a UI defect |
| `MASTER-165` `HandCursorBehavior` inert on non-`Button` elements | S3 | "Session 6, with `MASTER-161`" | **No** — a UI defect |
| `MASTER-166` optimistic ghost frame never observed | S3 | "Session 6" | Partly — needs a reproduction first |
| `MASTER-167` `GreaterThanZeroConverter` inverted sentinel | S2 | "Session 6, with the rest of the converter work" | **Yes** — converters are plain classes, directly testable |

T9 grouped these **by subject**, which is reasonable in isolation but conflicts with Session 6's own rationale — "the whole test suite in one pass, after the code it tests has settled". Three sidebar/UI defects inside a test-suite session is a theme collision, and this repo has already split one session (Session 5) for being too broad. **Decide deliberately at Gate 1**; do not let five entries drift in because a suggested-home line named the next session number.

**`MASTER-161` is `not done` and unassigned**, and `MASTER-165`'s home line says "with `MASTER-161`" — so wherever 165 goes, 161 should follow. `MASTER-162` carries **no** suggested home; it was filed and closed by 5c.

**2. `MASTER-160` is not in Session 6's declared list but names it.** Its bullet suggests Session 6 "alongside `MASTER-013`, whose teardown disposal touches the same cache". It is a narrow undisposed path in `PreloadObjectImagesAsync`. Including it is probably right; it is a decision, not a given.

## What Session 6 inherits from the backlog

`.dir-sdlc/backlog.md` holds **seven items dated 2026-09-01** (four filed by T9, three carried at Gate 3) plus five older ones from 5b. **None is specifically test-suite work**, so the backlog does not add scope here — but two are worth reading before writing criteria:

- **The `FloorMapDrawableAllocationTests` blind spot.** Its permanent fact compares moved areas against unmoved, so it catches only *offset-conditional* regressions. A reviewer injected an unconditional per-frame allocation and **all 30 tests stayed green**. If Session 6 audits test quality, this is a worked example of a passing test that guards less than its name suggests.
- **`DmFlagBanner.IsTitleEditable` is dead**, and with it `TitleEditor`, `ApplyEditability`, `OnTitleCommitted` and the `TextCommitted` event. Relevant if Session 6 measures coverage: those are lines no test can meaningfully cover because no consumer reaches them.

## Environment traps — all measured, all cost time

**Create the worktree under a short root: `C:\dm-wt\<slug>`.** The scratchpad path is ~184 chars, which pushes `src/DungeonMaster.App/obj/.../win-x64/input.json` past `MAX_PATH` (260); the net472 WinUI `XamlCompiler.exe` then fails with a **misleading** `input.json doesn't exist!`. `LongPathsEnabled` is already `1` and does not help. 5c used `C:\dm-wt\2026-08-28-the-control-kit` and never hit it.

**Two grep forms return confident wrong numbers instead of failing.** Both have appeared in acceptance criteria.
- `grep -c '^\t'` returns **0** on a fully tab-indented file. Use `TAB=$(printf '\t'); grep -c "^$TAB" <file>`.
- **`grep -cv $'\r$'` varies by shell.** On files proven all-CRLF by `od -c` and by CR-count = LF-count, it returned the **full line count** in one shell and **0** in two others — same grep 3.0, same files. **Never check line endings with grep in any form.** Use `file`, or compare `tr -cd '\r' | wc -c` against `tr -cd '\n' | wc -c`.

**`sed -i` rewrites files as LF**, tripping `IDE0055` under `TreatWarningsAsErrors`. Nine agents hit or avoided this in 5c; one hit it *without* `sed -i`, by pasting an LF block into a CRLF file.

**`git checkout -- <file>` restores to HEAD, which predates uncommitted task work.** A reviewer clobbered an implementer's file this way and recovered only because it had saved a byte copy first and checked with `cmp`. **Save a byte copy before any temporary edit**, and restore from that.

**Every XAML file violates its own `.editorconfig`** (`:38-39` declares `[*.xaml]` → `indent_style = tab`; the files are space-indented). `IDE0055` does not reach XAML, so nothing catches it. **Match the file, never retab.**

**macCatalyst is parked (`ADR-001`)** — it builds but cannot be run here. No criterion may demand observed macCatalyst behaviour.

**Probes lie in both directions.** A DPI-unaware `SetCursorPos` probe reproduced a defect that did not exist (`MASTER-159`); an element already focused at startup made a working control read as broken; synthetic `SendInput` does not drive WinUI's OLE drag loop, and failed identically on changed and unchanged builds. A working DPI-aware UIA probe with a `cursor==target`/`foreground==app` assertion is preserved in 5c's scratch — **but scratch is disposable and may be gone.** If it is, rebuild it and keep the assertion; a run without it is not trusted.

**`HandCursorBehavior` sets WinUI's non-public `ProtectedCursor` per element**, which another process cannot read. A `GetCursorInfo` probe measures the apparatus, not the change. Cursor claims need a human — that is how `MASTER-039` and `MASTER-161` were both settled.

## Process lessons from 5c worth carrying verbatim

**1. Tree wins over criterion, and the criteria were wrong six times.** A self-contradicting count; a drafting error generalising one doc comment to five sites; a false reachability premise; a repo-wide claim falsified by a sibling task three commits later; and two commands returning wrong numbers. Dispatch with verbatim criteria plus a standing rule: *implement what is correct and report the discrepancy with the command that showed it.*

**2. Hand agents the artifact, not your summary of it.** The orchestrator retyped criteria into prompts and introduced errors; it later pointed agents at `manifest.json` and told them to read their own `acceptance` array verbatim. **Four defects came from orchestrator briefs**, including one that omitted an entire audit entry a review file had handed over. The fix that worked: cite the file and the task id, not the content.

**3. Two would-be record corrections were WRONG, and both were caught by reading the cited source.** One claimed a criterion mis-cited a filename (it did not); one claimed the audit record overstated a harm (the record was precise; a session paraphrase had overstated it). **A wrong correction in the index is worse than no correction** — the next reader has no reason to doubt it. Before writing any correction, open the cited document and read what it actually says.

**4. Make deletions and green tests prove themselves.** When a task removed an allow-list entry and turned the suite green, it injected a defect into the newly-unexempted file and watched the guard catch it — proving the deletion removed an *exemption*, not coverage. Any change that makes a test stop failing deserves that treatment.

**5. Distinguish "wrong when written" from "stale".** T9 re-measured three corrected counts at the audit-era commit and found them wrong *then*; a fourth was right then and only looks wrong now because 5c fixed it. Errors and drift carry different lessons and the record now says which is which.

**6. Strike line numbers; cite the symbol.** A contract **frozen at Gate 2** had its own line citations go stale inside the same session. File-plus-symbol is the only durable address.

**7. Ask the user to look.** Three of 5c's six new entries exist because the user opened the app. Two negative observations — no hand cursor, unreachable drag handle — changed three index rows and retroactively explained a null result an agent had measured and could not account for.

## Suggested shape

Announce **Standard**, unless Gate 1 decides to fold in the five suggested-home entries — at 34 entries and ~6.6 days *before* any additions, and with "highest risk: high" declared, this is close to the size at which Session 5 was split. **The split decision belongs at Gate 1, before decomposition, not after.**

Sequence the host question first: whether `DungeonMaster.App.Tests` can stand up a MAUI application host determines whether the four rescheduled entries are provable at all, and therefore what the rest of the session may claim. `MASTER-065` and `MASTER-014` are named in the index as one unit — the shared fixture and the `EnsureCreated` schema are the same four files.

Finally: this session **owns no index reconciliation for a prior session** — 5c closed Session 5's rows, including 5b's. Session 6 marks only its own.
