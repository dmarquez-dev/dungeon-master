# Contract — the async, disposal and threading rules, and the guard tests that encode them

Frozen at Gate 2. This is the session's single discipline stated once, in the mechanically checkable form
T1 encodes as tests and T2, T3, T4 and T5 must satisfy.

- **Producer:** T1 — owns `src/DungeonMaster.Tests/App/AsyncDiscipline/`
- **Consumers:** T2 (`Graphics/FloorMapDrawable.cs`), T3 (`Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`,
  `Views/DungeonDetailPage/ObjectPaletteView.xaml.cs`), T4 (`Behaviors/*.cs`), T5 (`Controls/DmConfirmPopup/DmConfirmPopup.xaml.cs`,
  `Views/CreateDungeonPopup/CreateDungeonPopup.xaml.cs`, `Views/DungeonListPage/DungeonListPage.xaml.cs`)

T1 runs first and alone. T2–T5 then run concurrently. T1's tests are red when T1 finishes and green when
T2–T5 finish; that red is this session's red step, and it is the only route to one available in this
repository (see "Why these are source scans" below).

## Why these are source scans, and what they therefore do not prove

`src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` declares `<TargetFramework>net10.0</TargetFramework>`
and references only Domain, Application and Infrastructure — verified by
`grep -n "ProjectReference\|TargetFramework" src/DungeonMaster.Tests/DungeonMaster.Tests.csproj`, which
returns four lines and no reference to `DungeonMaster.App`. The App project is multi-targeted
(`net10.0-windows10.0.19041` / `net10.0-maccatalyst26.5`) and a `net10.0` project cannot reference it.
That is audit entry XCUT-002 / MASTER-026 and it belongs to Session 6. It is not fixed here and not
assumed away.

Every test T1 writes therefore **reads application source text from disk and never executes application
code**, exactly as `src/DungeonMaster.Tests/App/Theme/` and
`src/DungeonMaster.Tests/App/ViewModels/NavigationLoadIsObservedTests.cs` already do, through
`src/DungeonMaster.Tests/App/Theme/RepoPaths.cs`.

Two consequences bind every acceptance criterion in this session:

1. **No T1 test proves runtime behaviour.** A criterion resting on a T1 test says "the source states X",
   never "at run time Y happens". Any criterion worded otherwise is wrong and must be reported, not
   silently reinterpreted.
2. **This is why MASTER-006, MASTER-013, MASTER-055 and MASTER-062 are not in this session.** Three
   leaks and a race cannot be proven fixed by any executing test here, and at Gate 2 the user judged
   structural assertion insufficient for that defect class and moved all four to Session 6, where
   MASTER-026 lands. What remains in Session 4 is the set whose evidence is either a source fact that is
   genuinely the whole fix (G1, G2, G3, G4) or a behaviour a human can see in a running app.

These scans are implementation-coupled, which the test discipline's "behavior over implementation" rule
warns against. That tension is accepted deliberately and for a stated reason: source text is the only
observable this test project has of the App layer. `NavigationLoadIsObservedTests` records the same
reasoning in its own class summary and is the precedent being followed.

## The scanned set

`RepoPaths.AppSourceFiles("*.cs")` — every `*.cs` under
`C:\Users\dmarq\Development\Repos\dungeon-master\.claude-worktrees\dir\2026-08-24-async-disposal-and-threading-discipline\src\DungeonMaster.App`,
recursively, excluding any path containing an `obj` or `bin` segment. That method already exists and
already applies those exclusions (`src/DungeonMaster.Tests/App/Theme/RepoPaths.cs`, lines 30-48).

A **comment line** throughout these rules means a line whose trimmed form starts with `//` or `///`.
Comment lines are excluded from every scan. This matters: `FloorMapDrawable.cs:291` is a `// TODO(macOS)`
comment containing the literal text `GetAwaiter().GetResult()`, and it would otherwise be a false positive
for G4.

## The rules

### G1 — every async void unit is guarded

An **async void unit** is either:

- **(a) a method declaration** — a non-comment line matching the regular expression `\basync void\b`; or
- **(b) an async event-handler lambda** — a non-comment line matching `\+= async\b`.

Its **body** runs from the declaration line to the first following line whose leading-tab count is less
than or equal to the declaration line's leading-tab count and whose trimmed form is `}` or `};`.

**The rule:** within every async void unit's body, a non-comment line whose trimmed form starts with `try`
must occur at a line number strictly lower than the line number of the first non-comment line in that body
containing `await `.

Counted today by T1's own scanner, run against the unfixed tree, and reported in T1's report. The
underlying populations, verified now:

- `grep -rn "async void" --include=*.cs src/DungeonMaster.App | grep -v "///" | wc -l` returns **13**
  method declarations. One of them — `DungeonListPage.OnAppearing` at
  `src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml.cs:55` — already satisfies G1 and is
  the pattern the other twelve must adopt; its `<remarks>` block at lines 39-54 states why the `try` is
  load-bearing.
- `grep -rn "+= async" --include=*.cs src/DungeonMaster.App | wc -l` returns **6**, all in
  `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`, at lines 659, 736, 761, 789,
  1871 and 2155. **None of these six appears in audit entry UI-014 / MASTER-019**, which listed only named
  methods. They are the same defect in the same file and T3 fixes them under G1.

**The guard's shape.** A `try` around the awaited work whose `catch (Exception exception)` routes the
message to that file's error channel, named per file below. Not a bare `catch { }`: the backend
discipline forbids swallowing an error to keep a path quiet, and every channel below is a place the
failure becomes visible or recorded.

| File (relative to the worktree root) | Error channel | Evidence it exists |
|---|---|---|
| `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` | `_viewModel.ErrorMessage` | `src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:71` — `public partial string? ErrorMessage { get; set; }` |
| `src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs` | a new `public event EventHandler<string>? LoadFailed;` on the view, which `DungeonDetailPage` subscribes to `_viewModel.ErrorMessage` | The view has no view model; it declares five events today at lines 15-19 and this is the sixth. Both files are owned by T3, so the event and its subscriber land together. |
| `src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml.cs` | `_viewModel.ErrorMessage` | `src/DungeonMaster.App/ViewModels/DungeonListViewModel.cs:33` |
| `src/DungeonMaster.App/Views/CreateDungeonPopup/CreateDungeonPopup.xaml.cs` | `viewModel.ErrorMessage` (the constructor parameter, captured by the lambda) | `src/DungeonMaster.App/ViewModels/CreateDungeonViewModel.cs:31` |
| `src/DungeonMaster.App/Controls/DmConfirmPopup/DmConfirmPopup.xaml.cs` | `System.Diagnostics.Debug.WriteLine` plus a comment at the catch site | The control has no view model and `Popup<bool>` exposes no error channel. `System.Diagnostics.Debug.WriteLine` is the App layer's established last-resort channel, used at `src/DungeonMaster.App/MauiProgram.cs:166` and `src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:109`. The comment must state that a failed close leaves the popup on screen and the awaiting caller's `Task<bool>` incomplete. |

### G2 — no discarded task

**The rule:** no non-comment line in the scanned set has a trimmed form that starts with `_ = `
(token followed by a space), **excluding the switch-expression discard arm form `_ =>`**, which is
not a discarded task and must not be reported.

`grep -rn "_ = " --include=*.cs src/DungeonMaster.App` returns **16** lines today, at:
`Controls/DmConfirmPopup/DmConfirmPopup.xaml.cs:59,62`;
`Views/CreateDungeonPopup/CreateDungeonPopup.xaml.cs:25,28`;
`Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:143,163,351,369,388,1215,1217,1219,1221,1223,1522,1662`.
Every one of the sixteen discards a `Task` — I read each; they are `CloseAsync`, `RotateObjectAsync`,
`ScaleObjectAsync`, `DeleteSelectedObjectAsync` and `MoveObjectAsync`.

> **Corrected at Gate 3, 2026-08-26, by the user's decision.** This paragraph previously read "There is
> no legitimate non-task `_ =` discard in the layer today, which is why the rule can be stated as a flat
> ban on the token." **That claim was false**, and T1's reviewer filed it as a `contradicts` finding.
> The layer holds **six** switch-expression discard arms of the form `_ => value`, none of which
> discards a task and none of which can be rewritten — removing one produces `CS8509` under
> `TreatWarningsAsErrors`. They are at `Assets/ObjectAssets/ObjectKindInfo.cs:67,109,129`,
> `Behaviors/HandCursorBehavior.cs:119`, `Graphics/FloorMapDrawable.cs:411`, and
> `Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:924` (now `:1041`).
>
> Under the literal flat ban, G2 could never go green and T3's acceptance criterion 5 was
> unsatisfiable by any amount of correct work. T1 implemented the contract's own grep, count and
> enumerated line list — all three of which already agreed on the narrower token — and pinned the
> exclusion with `ScanG2_WhenALineIsASwitchExpressionDiscardArm_ReportsNothing`. The orchestrator ruled
> `ScanG2` the operative definition for the rest of the session and carried that into every dispatch
> and reviewer brief.
>
> **A residual gap the corrected rule still has**, from T1's advisory A1: a discard written `_=Foo();`
> with no space, or mid-line as in `if (x) _ = Foo();`, is invisible to both the prose and the scanner.
> Zero such lines exist today. A type-aware check is what would close it, and a source scanner cannot
> perform one.

Audit entry UI-015 / MASTER-024 names "eight call sites" and collapses lines 1209-1224 into one location.
The actual count in `DungeonDetailPage.xaml.cs` is **twelve** lines, and the two popup files carry four
more that no audit entry mentions.

### G3 — async void only where the signature forces it

**The rule:** every async void **method declaration** (G1 case (a)) either contains the token ` override `
on the declaration line, or declares a method whose name matches `^On[A-Z]`.

Violated today at exactly three sites, all in `DungeonDetailPage.xaml.cs`:
`private async void EraseAreaAsync(...)` (line 1038), `private async void PlaceObjectAsync(` (line 1050),
`private async void FinishDrawing()` (line 1602). These are MASTER-018 / UI-013. Each becomes
`private async Task ...` and is awaited by its callers, which are themselves event handlers and therefore
become guarded async void units under G1. Their call sites, per UI-013 and to be re-verified by T3 against
the current file: `OnMapStartInteraction` (lines 1277, 1289, 1336) and `OnMapDoubleTapped` (line 1645).

### G4 — no sync-over-async in the App layer

**The rule:** no non-comment line in the scanned set contains `GetAwaiter().GetResult()` or `.Wait()`.

`grep -rn "GetAwaiter()\.GetResult()\|\.Wait()" --include=*.cs src/DungeonMaster.App` returns **two** lines
today: `Graphics/FloorMapDrawable.cs:291` (a comment, excluded by the rule) and
`Graphics/FloorMapDrawable.cs:304` (the real one, MASTER-025). After T2 the rule holds with zero matches.

`.Result` is deliberately **not** part of this rule. `src/DungeonMaster.App/Extensions/NavigationExtensions.cs:56`
reads `return result.Result;`, where `result` is an `IPopupResult<T>` and `.Result` is an ordinary property,
not `Task.Result`. A scanner cannot tell the two apart from source text, so banning the token would fail on
correct code.

### G5, G6, G7, G8 — withdrawn at Gate 2

An earlier draft of this contract defined four further rules: G5 (the drawable is disposed by its owner,
MASTER-013), G6 (no plain bool crosses a thread in the long-press paths, MASTER-062), G7 (the pan
recogniser is detached, MASTER-006) and G8 (the scroll-zoom unsubscribe uses the element it subscribed to,
MASTER-055).

**All four are withdrawn.** At Gate 2 the user rescheduled MASTER-006, MASTER-013, MASTER-055 and
MASTER-062 to Session 6. They are three leaks and a race; no test in this repository executes App code,
and structural assertion was judged insufficient evidence for that defect class. Session 6 lands
MASTER-026 and can demonstrate them rather than assert them.

T1 MUST NOT write these four guards. A guard asserting a fix nobody in this session is making would be
permanently red against a tree nobody is fixing, which would leave the suite red at Gate 3 and teach
every later reader that a red suite is normal here.

The rule identifiers G5, G6, G7 and G8 are retired rather than renumbered, so that this document, the
manifest and the task reports all keep referring to the same four things. G1, G2, G3 and G4 keep their
numbers and their meanings unchanged.

## What each consumer task must do to satisfy these rules

Stated once here so no task has to derive it, and so the reviewer can check the same list.

- **T2** — `FloorMapDrawable.cs`: G4 (remove the blocking load), plus the full surface in
  `floor-map-drawable.md`. No `IDisposable` and no `Dispose()` — see that contract's section 1.
- **T3** — `DungeonDetailPage.xaml.cs` and `ObjectPaletteView.xaml.cs`: G1 (guard 5 named async void
  handlers — `DungeonDetailPage.xaml.cs:554`, `1502`, `2211` and `ObjectPaletteView.xaml.cs:143`, `154` —
  plus the 6 existing async lambdas at `659`, `736`, `761`, `789`, `1871`, `2155`, plus the 2 new ones the
  drawable contract creates, plus the 3 handlers that G3's conversion turns into awaiting handlers);
  G2 (12 discards); G3 (3 conversions); and the two call-site changes in `floor-map-drawable.md`. No
  `_drawable.Dispose()` call, and no `AttachScrollZoom`/`DetachScrollZoom` change — both withdrawn above.
- **T4** — `Behaviors/HandCursorBehavior.cs` only: MASTER-039, and no G rule at all (see below — it is a
  caching change with no source-scannable shape worth freezing). `CanvasLongPressDetector.cs` and
  `HoverOrLongPressBehavior.cs` are no longer owned by any task in this session and MUST NOT be edited.
- **T5** — `Controls/DmConfirmPopup/DmConfirmPopup.xaml.cs`, `Views/CreateDungeonPopup/CreateDungeonPopup.xaml.cs`,
  `Views/DungeonListPage/DungeonListPage.xaml.cs`: G1 (guard `DungeonListPage.xaml.cs:69`, `100`, `114`,
  `124`), G2 (4 discards).

MASTER-039 / UIKIT-020 (`HandCursorBehavior.SetCursor` reflecting on every pointer event) is T4's and has
no guard rule: the fix is to hoist the `typeof(Microsoft.UI.Xaml.UIElement).GetProperty("ProtectedCursor", ...)`
lookup into a `static readonly` field and cache one `InputSystemCursor` per `BehaviorCursorShape`. The
whole block is inside `#if WINDOWS` (`Behaviors/HandCursorBehavior.cs:109-132`), so a source scan would
have to reason about preprocessor branches to say anything useful, and the change is verified by build and
by the manual cursor observation instead.

## T1's obligation to show its scanner can fail

Every rule above — G1, G2, G3 and G4 — must be implemented as a function taking source text (or a line list) as a parameter, so
that T1 can prove discrimination without editing a file it does not own. For each of G1 through G4, T1
writes at least one test over an **inline fixture string** that violates the rule and asserts the scanner
reports it, alongside the `[Fact]` that applies the scanner to the real files. This is the
"demonstrated discrimination" route, and it is available to T1 entirely within
`src/DungeonMaster.Tests/App/AsyncDiscipline/`.

The red step is additionally available for free: T1 runs before T2–T5, so the real-file facts fail when
T1 finishes. T1 reports the exact failing test names and counts it observed.

## Escalation

A consumer that cannot satisfy a rule without editing a file it does not own must stop and report it. In
particular: no task may edit `src/DungeonMaster.Tests/App/AsyncDiscipline/` except T1, and no task may
relax a rule here to make its own work pass.
