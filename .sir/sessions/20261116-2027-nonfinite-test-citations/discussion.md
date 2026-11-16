# nonfinite-test-citations

Make every source-location citation in the doc comments of
`src/DungeonMaster.App.Tests/Graphics/FloorMapDrawableNonFiniteTests.cs` point at the code it
describes as of the session base, anchored so that a reader can find each one again after the
line numbers move.

## Context

- Backlog row `.sir/backlog.md:148` (2026-11-04, 20261103-2239-app-follow-ups, source
  `state.json E1; reviews/T5.md A-3`) asks for the class-remark line citations outside the
  `ZoomLevel` text that session corrected to be re-measured. It starts with item 5's `:508`
  and line 13's unchecked `:1437`.
- Re-measured at `40dddd1` (the base, `chore/repo-audit` HEAD) with `grep -n` in the main
  checkout. The drift is wider than the row says, and the `ZoomLevel` text the 20261103-2239
  session corrected has drifted again too:
  - `DungeonDetailPage.xaml.cs`: `_drawable.PanOffset = PointF.Zero;` is at `:585`. The
    comment says `:508` and the row says `:527`.
  - `_drawable.ZoomLevel = ZoomLevelGuard.Next(...)` is at `:578`, against the comment's
    `:520`.
  - `private void ApplyZoom` is at `:576` (comment `:518`), `private void ResetViewport` at
    `:583` (comment `:525`), and `ApplyZoom(1.0f);` at `:586` (comment `:528`).
  - The `ApplyZoom(` callers are at `:571`, `:574`, `:586`, `:675` and `:689`. The comment
    names `:513`, `:516`, `:528`, `:617` and `:631`.
  - There are `#if WINDOWS` lines at `:389`, `:621`, `:650` and `:664`, and `#elif MACCATALYST`
    lines at `:398`, `:632`, `:654` and `:677`. The comment's spans are `:606-619` and
    `:619-635`.
  - The pan-drag accumulator `_drawable.PanOffset = new PointF(` starts at `:1790`, against
    the comment's `:1632-1634`.
  - `ScreenToCanvas(` calls are at `:1545`, `:1595`, `:1661`, `:1693`, `:1776`, `:1802`,
    `:1814` and `:2070`. Which of them the comment's `:1437` and `:1442` meant still has to
    be worked out.
  - `FloorMapDrawable.cs`: `ZoomLevel { get; set; } = 1.0f;` is at `:455` and
    `PanOffset ... = PointF.Zero;` at `:458` (comment `:439`, `:442`). The sealed class
    declaration is still at `:98`. The comment puts the `ScreenToCanvas` division at
    `:629-630`, while `grep -n` puts `new((screen.X - PanOffset.X) / ZoomLevel,` at `:647`.
  - `Resources/Theme/Tokens.xaml`: the two `FloorMapDrawable` prose lines are still at
    `:263` and `:267`.
  - Within the test file itself, `grep -n -E '[A-Za-z]+\.(cs|xaml)?:[0-9]+|:[0-9]{2,}'` prints
    14 lines, several of which carry more than one citation. The spec phase must count the
    citations themselves, not these lines, and validate that predicate with a control.
    Bare `:NNN` continuations and `FloorMapDrawableExecutionTests.cs:39-40` are in the
    population too.

## Decisions

- **Track: Standard** (Gate 1). The change touches one file, but its result is a set of
  separately checkable citations and claims, and the spec must record each one.
- **E1: anchor each site on its member and statement, and keep the line number only as
  "as numbered at `<base>`"** (Gate 1, recommended option, chosen by the user). Each citation
  names the enclosing member and the statement or construct (for example `ApplyZoom`'s
  `ZoomLevelGuard.Next` write), so that `grep` finds it again after the numbers move. The line
  number stays, labelled with the commit it was measured at. An `#if` span, which has no
  member of its own, is named by its enclosing member and its directive.
- **The claims are re-checked as well as the numbers** (Gate 1 work item 3, accepted). The
  spec must check that `ApplyZoom` is still the only production `ZoomLevel` write, which
  callers it has, which `#if` arm each caller sits in, that the pan-drag accumulator is still
  the one unbounded `PanOffset` writer, that `FloorMapDrawable` is still not a
  `BindableObject` (`grep -c BindableProperty` 0), and that the `.xaml` occurrences are still
  prose only.
- **Default, let stand at Gate 1: only doc comments change.** No test body, assertion or
  production file is edited, and App.Tests still passes 146 of 146 (the count recorded by
  20261116-1932-objectkind-ordinal-test; re-measure it at the base before relying on it).
- **Default, let stand at Gate 1:** where a re-measured claim is no longer true, the comment
  is corrected to what the code does and the finding is reported at Gate 3. No production
  fix is made in this session.
- **Default, let stand at Gate 1:** backlog row `:148` is removed at Gate 3 once its
  citations are settled.
- **Line endings:** the test file is a tracked CRLF `.cs` file. The lessons file forbids
  `sed -i` and `Write` on it, says `Edit` must not end `new_string` on whitespace, and says
  line endings are measured with a Python byte count.
- **Base:** `chore/repo-audit` at `40dddd1`. The worktree root is `C:/dm-wt/`, per
  `.sir/config.json`, and the branch prefix is `agent/`.
