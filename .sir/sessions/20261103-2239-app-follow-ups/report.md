sir-sdlc 1.0.18 — Gate 3 report — 20261103-2239-app-follow-ups

## 1. Decisions needed

Two items proposed for `.sir/backlog.md` came from resolved escalations. Each carries that escalation's options.

**B1 (from E1, T5).** The non-`ZoomLevel` line citations in the `FloorMapDrawableNonFiniteTests` class remarks were left unmeasured on purpose. T5 added a sentence saying so. The review names item 5's `:508` (now `:527`) and line 13's `:1437` (not checked).
1. *As written (E1 option 1, chosen at Gate 2):* append "re-measure the remaining line citations in the remarks block". Rationale: the narrow fix landed, and the rest stays bounded. Evidence: repo, `FloorMapDrawableNonFiniteTests.cs:16-19, :22-26, :42-43`.
2. *Widened (E1 option 3):* append "fully refresh every citation and universal claim in the block". Rationale: the complete fix, at the cost of re-running a reachability audit. Evidence: judgment.

**B2 (from E2, T5 C7).** A normal solution build does not diagnose `<see cref>`: no build file sets `GenerateDocumentationFile`. Only a doc-mode build exposes it, including the pre-existing `RecordingCanvas.cs(39,41)` CS0419.
1. *As written (E2 option 1, chosen):* append "future criteria about crefs must cite the doc-mode build command", and carry it to lessons. Rationale: the property was measured and holds; the record keeps the instrument gap. Evidence: repo, `FloorMapDrawableNonFiniteTests.cs`, `RecordingCanvas.cs`.
2. *Fix the instrument (E2 option 3):* append "enable `GenerateDocumentationFile` repo-wide". Rationale: normal builds would check crefs, but it fails on CS0419 and likely surfaces CS1591 everywhere. Evidence: judgment.

## 2. Acceptance criteria

| Task | Domain | Criteria | Met |
|---|---|---|---|
| T1 | frontend | 6 | 6 |
| T2 | frontend | 9 | 9 |
| T3 | frontend | 10 | 10 |
| T4 | frontend | 4 | 4 |
| T5 | test | 7 | 7 |

No criterion is `not met`, and none was amended. The 18 criteria that rest on a demonstration are listed below.

- T1 C3 — reviewer-reproduced — the overlay BoxView keeps "Sessions are not available yet" (:117) and `InputTransparent="False"` (:118).
- T1 C4 — reviewer-reproduced — one `-` line and no `+`; the `ToolTipProperties` count is 4.
- T2 C1 — reviewer-reproduced — the census hits once, at `DungeonDetailPage.xaml.cs:520` (`ZoomLevelGuard.Next`). The startRef :527 hit is gone, and the near miss stays unmatched.
- T2 C5 — injected defect — `|| !float.IsFinite(current)` at `ZoomLevelGuard.cs:43` gave Failed 1 (Expected 1, Actual NaN), Passed 8. Reverted.
- T2 C6 — injected defect — under the injection, the control and the 7 existing tests passed.
- T2 C7 — reviewer-reproduced — `ZoomLevelGuard.cs` has a matching sha256 before and after, and no diff.
- T2 C8 — reviewer-reproduced — perl CRLF census equal on all three files.
- T3 C1 — reviewer-reproduced — the census gives the same 5 file:line pairs, the case-insensitive count is 8, and the control matched.
- T3 C2 — reviewer-reproduced — both deleted files fail `test -e`.
- T3 C4 — reviewer-reproduced — after deletion, predicate 1 is empty and predicate 2 finds only the 3 TileSize lines.
- T3 C5 — reviewer-reproduced — build 0/0. Tests are 187 / 943 / 123 with Failed 0, which is 18 fewer than 141.
- T3 C9 — reviewer-reproduced — opener/closer form on all three blocks; `**]**` counts 12 and 15.
- T3 C10 — reviewer-reproduced — perl CRLF census equal on all three edited files.
- T4 C4 — reviewer-reproduced — perl census `173 173 173`.
- T5 C1 — reviewer-reproduced — the only non-test `ZoomLevel` write is `:520`, and the remarks now say every write goes through `ZoomLevelGuard.Next`.
- T5 C4 — reviewer-reproduced — 22/22 changed lines are `///`, and the `[Fact]` count is 6 before and 6 after.
- T5 C6 — reviewer-reproduced — perl census `169 169 169`.
- T5 C7 — injected defect — a `NextX` cref gave CS1574 in a doc-mode build (implementer's run). Met on the E2 resolution.

Every row is in `reviews/T1.md` through `reviews/T5.md`.

**Coverage**, as mapped at Gate 2 and checked at Gate 3. D1, D4, D5 and D7-D14 are met by the join. D3's handler half is met (T2 C3). Orchestrator rows still open:
- D2 (uncovered at Gate 2: a human hover observation, asked at this gate). It is pending your answer.
- D6 (uncovered at Gate 2: a human Ctrl+0 observation, asked at this gate). It is pending your answer.
- D15 (uncovered at Gate 2, orchestrator's) and D3's disposition half. Backlog rows 194-198 are removed after approval.

No contracts are declared, so there are no operation rows.

**Limits:** none. Every verification command ran in the worktree.

**Human checks**
- Run the app, open the dungeon list, and hover the disabled start-session icon on a card. Only "Sessions are not available yet" should appear, never "Start session" (D2).
- On the dungeon detail page, zoom in and pan away, then press Ctrl+0. The map should return to its origin at 100%, with the zoom label reading "100%" (D6).

## 3. What ran

| Task | Agent | Domain | Model | Reviews | Rounds | Redispatches | Commit |
|---|---|---|---|---|---|---|---|
| T1 | sir-frontend-engineer | frontend | opus | 1 | 0 | 0 | b27f661 |
| T2 | sir-frontend-engineer | frontend | opus | 1 | 0 | 0 | 754020a |
| T3 | sir-frontend-engineer | frontend | opus | 1 | 0 | 0 | b50645a |
| T4 | sir-frontend-engineer | frontend | opus | 1 | 0 | 0 | 5361708 |
| T5 | sir-test-engineer | test | opus | 1 | 0 | 0 | 1e978a4 |

The reviewers were `sir-reviewer` on opus, once per task. Five commits went through `sir-git` on haiku.

Change surface: `2a7d27b..1e978a4`, 5 commits. 10 files changed, 101 insertions(+), 402 deletions(-). The full diff is `git diff 2a7d27b..1e978a4`.

## 4. Advisory findings

**Correctness risks (3)**
- `FloorMapDrawableNonFiniteTests.cs:38`: the timing in "was corrected when `ZoomLevelGuard` was introduced" is false. The guard came in at `02fc0dd`, and the correction is this session's. Suggested wording: "corrected after". (T5 A-1)
- `RecordingCanvas.cs:39`: a pre-existing ambiguous cref `DrawString` (CS0419), visible only in a doc-mode build. (T5 A-4)
- `FloorMapDrawableNonFiniteTests.cs`: item 5's `:508` and line 13's `:1437` are stale citations left by E1. This is the same work as B1 in section 1 and is decided there. (T5 A-3)

**Session conduct (1)**
- The T4 dispatch brief said the README "already cites" the MS Learn URL, but at startRef it did not. The implementer caught it, and the false premise was in the brief, not in the frozen criterion. (T4 A-3)

**Test and CI concerns:** 3. See `reviews/T3.md` and `reviews/T4.md`.

**Cosmetics:** 2. See `reviews/T1.md` and `reviews/T5.md`.

## 5. Metrics

- userInteractions: 4 (this gate included; the push question comes after the merge and is not yet counted)
- escalationsRaised: 2
- reviewRounds: 0
