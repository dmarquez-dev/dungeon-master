# Coverage — session 6b-x, `2026-09-18-dispatcher-test-gaps`

Written at Gate 2, before the approval that freezes `manifest.json`. The `Gate 2` column is complete
here; `End state` is empty and is filled at Gate 3.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. Token reaches the **handler** on all three request paths | T1 / A1, A2, A3 | covered | **met** — A1, A2, A3 all met; A2 `reviewer-reproduced` |
| 2. Token reaches the **behaviour pipeline** on all three paths | T1 / A4, A5, A6 | covered | **met** — A4, A5, A6 all met |
| 3. Missing-handler failure observed on all three paths, message naming the handler service | T1 / A7, A8, A9 | covered | **met** — A7, A8, A9 all met, all `reviewer-reproduced`. Prediction **confirmed**: the message string held, and the reviewer verified the facts assert on the message rather than on the bare throw |
| 4. Each of the three defect classes shown able to fail | T1 / A10, A11, A12 | covered | **met** — A10, A11, A12 all met; A12 `reviewer-reproduced` |
| 5. `Dispatcher.cs` byte-identical to `startRef` | T1 / A14 | covered | **met** — `reviewer-reproduced`, and re-verified by the orchestrator after the commit: `git diff --stat 506cbba..b0fa532 -- src/DungeonMaster.Application/` is empty |
| 6. New files CRLF with final newline, every `[Fact]` and type XML-documented | T1 / A13, A15 | covered | **met** — A13 met; **A15 met only after amendment A1**, having returned `not met` on the original text |
| 7. `DungeonMaster.Tests` green, exit 0, with `Dispatcher.cs` restored | T1 / A16 | covered | **met** — A16 met, `Failed: 0, Passed: 849, Total: 849`, exit 0 |
| 8. Full solution suite green and warning-free, verified in the **main checkout after the merge** | — | **uncovered** — accepted at Gate 2 as orchestrator work, not out of scope. No task can perform it: it happens after every task is done and after the merge | **not yet performed** — by construction it cannot be, at Gate 3. The merge has not happened. Predicted `met`; this row is filled after the post-merge run, and the prediction is not evidence |
| 9. `MASTER-172` and `MASTER-173` closed in `docs/audit/00-master-index.md` with commit evidence, and the residual note at `docs/audit/05-tests.md:317` updated to record what was closed and what was not | — | **uncovered** — accepted at Gate 2 as orchestrator work, not out of scope. Tasks cannot invoke git, so a commit hash cannot reach an index cell from inside one | **not yet performed** — same reason. The closure cites `b0fa532` and its merge commit, and the merge commit does not exist until approval |

Rows 8 and 9 are **not** retired as out of scope. They are real definition-of-done items that no task is
structurally able to perform, and recording them as `covered` would make the session look decomposed
when it is not. The 6b-ix session recorded that two of its three new index rows returned `UNCLASSIFIED`
from the project's own survey until the post-merge closure commit set them — a session dying in that
window leaves rows invisible to every future survey, which is why row 9 is tracked here rather than
treated as bookkeeping.

**Predicted end states**, written before the Gate 3 fill so a surprise reads as a surprise:
rows 1–7 `met`; rows 8 and 9 `met` by orchestrator action. Row 3 is the one to watch — it is the only
row whose evidence rests on a library message string rather than on this repository's own code, and
although that string was probed against the pinned version (`measurement.md` §7), a package bump between
now and Gate 3 would refute it.

**How the predictions resolved.** Row 3's prediction is **confirmed** — the message held and the
reviewer additionally verified that the facts assert on the message rather than on the bare throw, so
the row is met for the reason predicted rather than by luck. Rows 1, 2, 4, 5 and 7 are met as predicted.

**Row 6's prediction is refuted, and the search's answer stands.** It was predicted `met`; on the text
signed at Gate 2 it was **not met** — A15 returned `not met` from the reviewer, and the row is met only
because escalation E1 produced amendment A1. The prediction was wrong because it assumed the criterion
described the repository, and the criterion did not. Recorded as refuted rather than quietly overwritten
by the row that turned out right, which is the entire purpose of writing predictions down.

**Rows 8 and 9 are not yet performed**, and their prediction is therefore neither confirmed nor refuted
at Gate 3. Stating them as `met` here would report an intention as a measurement.

## Contract operations

**Empty, and not a gap.** The session declares no contracts: `manifest.json` carries `"contracts": []`
and `contracts/` holds no files. With a single task there is no inter-task boundary to freeze. An
operation declared here would have an implementer and no consumer, which is the precise failure this
table exists to surface.
