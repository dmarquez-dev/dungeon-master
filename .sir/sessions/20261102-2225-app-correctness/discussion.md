# App correctness

Close six user-visible correctness gaps the backlog records in `DungeonMaster.App`: its view
models and the `DungeonDetailPage` code-behind. These are states a user can reach where the app
shows something false, silently accepts the wrong input, or leaves a handler half-guarded.

## Context

The repo-audit arc on `chore/repo-audit` schedules its work from `.sir/backlog.md`. At Gate 1
that file held 168 entries, by `grep -c '^20' .sir/backlog.md`. The previous session,
`20261102-1907-domain-invariants`, closed the Domain and Application correctness rows. Gate 1
chose the App layer next, ahead of the TEST six-row sweep and a CORE doc/comment sweep, because
the user asked for work that improves the product.

The selected rows, by their line in `.sir/backlog.md` as read at Gate 1 (`grep -n`, listed below):

| # | Backlog line | Source | Row |
|---|---|---|---|
| A1 | 56 | `.dir-sdlc/backlog.md:44` (xref MASTER-017) | Make `DungeonDetailViewModel.LoadDungeonAsync` report an error and stop saves when a well-formed `Guid` names no dungeon. Consider filing it as its own audit entry, since MASTER-017 covered only absent or unparsable ids. |
| A2 | 64 | `.dir-sdlc/backlog.md:233` | Reset `CreateDungeonViewModel.ErrorMessage` to null when a new create starts or succeeds. |
| A3 | 68 | `.dir-sdlc/backlog.md:243` (xref MASTER-154) | Disable `StartSessionButton` until sessions exist, or wire it up. On a dungeon with floors it is an enabled tab stop in `DmCard`'s trailing region, and pressing it does nothing. |
| A4 | 75 | `.dir-sdlc/backlog.md:318` | Decide whether the floor reorder keystroke should require `e.Modifiers == KeyboardModifiers.Control` instead of `HasFlag(KeyboardModifiers.Control)`. `HasFlag` also admits Ctrl+Shift+arrow and Ctrl+Alt+arrow. |
| A5 | 77 | `.dir-sdlc/backlog.md:451` | Guard the MacCatalyst pinch-zoom path, where `ApplyZoom(_pinchStartZoom * (float)e.Scale)` can pass `NaN`, which `Math.Clamp` returns unchanged. |
| A6 | 57 | `.dir-sdlc/backlog.md:52` (xref MASTER-055) | Fold the synchronous `DetachScrollZoom()`/`AttachScrollZoom()` calls into the `try` in `OnMapCanvasHandlerChanged` so the handler is guarded uniformly. |

**Line-number correction.** The Gate 1 question cited these rows as lines 52, 53, 60, 64, 71 and
73. Those figures came off a `sed -n '5,132p'` excerpt and are 4 too low. The rows the user
approved are the ones named above. `grep -n` on the file gives 56, 57, 64, 68, 75 and 77.

**Log ordering.** The `GATE 1 presented` line in `log.md` was written after the gate was
answered. The session directory's name depended on the answer, so no `log.md` existed to write
it into beforehand. Its timestamp is the clock read taken just before the gate was shown.

These were spot-checks by the orchestrator at Gate 1, not measurements. Each is a lead for the
spec phase to re-measure:

- `src/DungeonMaster.App/ViewModels/CreateDungeonViewModel.cs:70` assigns `ErrorMessage` on
  failure. It was the only assignment found in that file (`grep -n ErrorMessage` hit `:33`, the
  declaration, and `:70`), so A2 looks live.
- `src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:172-181`: `LoadDungeonAsync`
  passes the query's nullable DTO straight to `Repopulate`, with no null branch, so A1 looks
  live. Spec must establish what "stop saves" means here: which commands save, and what gates
  them today.
- `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`: `HasFlag(KeyboardModifiers.Control)`
  appears at `:396` **and** `:480`. The row names one site (`:390` at filing time). Spec must
  establish which is the floor reorder keystroke, and whether the second has the same defect.
- The same file has `ApplyZoom(_pinchStartZoom * (float)e.Scale)` at `:632` (A5 looks live).
- `StartSessionButton` appears in `DungeonDetailPage.xaml:118` and in `DungeonListPage.xaml:96`.
  `Theme/Implicit.xaml:222-223` mentions a `FloorCount == 0` DataTrigger for both. The row names
  only the detail page. Spec must decide whether A3 covers both buttons.
- A6 was not spot-checked.

## Decisions

- Track: **Standard**. The work spans several App files and makes many separately checkable
  claims.
- Session name: `app-correctness`.
- Scope is A1-A6 above and nothing else. The neighbouring mixed-indent row at backlog line 58
  (`DungeonDetailPage.xaml.cs:481`, in the same file as A4-A6) is **not** selected at Gate 1.
  If the spec finds it is still live and lies inside a hunk a task already owns, it may raise it
  at Gate 2 as an optional add. It is not quietly absorbed.
- **Re-measure every row before planning around it.** The arc's record is that most selected
  rows carry a false or stale specific. A row found already fixed is closed with its evidence and
  gets no work. A row whose premise is false is reported at Gate 2, not quietly re-scoped.
- A1's design (how the error is surfaced, how saves are stopped) is settled in spec from repo
  evidence. Start with how the view model already surfaces a failed `Result` (`ResultMessage`,
  and the absent/unparsable-id handling MASTER-017 added). Whether to file a new audit entry is
  a Gate 3 note, not work.
- A3 offers two remedies, "disable until sessions exist" or "wire it". Sessions do not exist as a
  feature, so the default is **disable**, and wiring it is out of scope. Settle the mechanism
  (`IsEnabled`, a `CanExecute`, or the existing DataTrigger) from how the file's sibling buttons
  do it.
- A4 is phrased as a decision. Default: **require exact `Control`** for the reorder chord, since
  `HasFlag` would collide silently with any later Ctrl+Shift/Ctrl+Alt binding. If spec finds a
  reason in the repo the chord must accept extra modifiers, report it at Gate 2.
- A5's guard should reject a non-finite scale product, leaving zoom unchanged, rather than clamp
  it. Spec settles where the guard lives (the pinch handler or `ApplyZoom`), with the
  `CanvasPoint` finiteness guard as precedent. The path is MacCatalyst-only, so its test must
  exercise the guard logic on the Windows/net10.0 test host. Don't rely on a MacCatalyst run that
  this machine cannot do.
- Every changed guard needs a test that **fails against the unguarded code**. Each such
  criterion names an injection that is actually run, with a control that must pass. A test
  whose green state is reachable without the change under test does not count. Where a property
  can only be observed in the running UI (A3's tab stop, A6's guarding), name it at Gate 2 and
  route it to human observation. Never close a row on a static proxy.
- At Gate 3, the closed rows are recorded against their `.sir/backlog.md` entries.

### Standing constraints carried from `.sir/lessons.md` and memory

- Never run `sed -i` on a tracked file, and never use `Write` on an existing CRLF file. Both
  silently convert the whole file to LF, and under `IDE0055` that fails the build. State this
  in every dispatch, reviewers included.
- `Edit` strips trailing whitespace from `new_string`, so key whitespace-bearing edits on the
  line's first text character.
- Create the worktree under `C:\dm-wt\` (for example `C:\dm-wt\app-correctness`). A
  scratchpad-length path breaks the WinUI XAML compiler with a misleading `input.json`
  MAX_PATH error.
- The solution is `src/DungeonMaster.sln`. Prove a clean build by quoting its summary lines,
  never with `grep -icE "warning|error"`, which returns 2 on a clean build. Treat
  `dotnet test --no-build` in an unrestored worktree as not run unless the output names a
  test count.
- A solution build after an SDK update can rewrite `src/DungeonMaster.App/packages.lock.json`,
  a file no task owns. Check `git status` in both trees before removing the worktree.
- For any "build and test after every revert" criterion, script it, save the logs, and cite
  them. Final-state evidence alone has been ruled insufficient.
- After the merge, verify build, tests and line endings in the **main checkout**, not only
  the worktree.
- Verify every commit body with `git cat-file -p` against its message file, plus a
  tampered-copy control. Never use `git log --format=%B`.
- Commits on the session branch are authorised through the SIR flow. **Never push.**
