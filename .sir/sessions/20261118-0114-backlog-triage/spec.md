# Backlog triage

Re-measure every open row of `.sir/backlog.md` at `4d874be`, give each a recorded disposition, and shrink the backlog to rows that are real work worded as work, with every `Decide` row answered by the user at Gate 2.

## Context

- At `4d874be`, `.sir/backlog.md` holds 133 dated rows: 132 open, plus MASTER-158 under `## Permanently deferred`. `C1-check.py populations` printed `132 open rows, 132 in the six ranges`. The file is 163/163/163 CRLF/LF/CR by Python byte count, and `.sir/lessons.md` is 149/149/149.
- The file's header says every entry must be re-measured before it is scheduled. Removing rows one Small session at a time would take on the order of a hundred sessions (discussion.md, Context).
- 23 rows begin `Decide`. Lines 147, 151 and 157 ask one question, so there are 21 decisions. The orchestrator re-measured all 23 during this phase, and the premises are recorded in `contracts/C2-gate2-decisions.md`.
- Precedent: `20261029-1708-dir-backlog-to-sir` triaged the DIR backlog into this file with one record file per range and a final task that assembled the backlog.

## Definition of done

1. Every one of the 132 open rows has exactly one record block across the six record files under `.sir/sessions/20261118-0114-backlog-triage/triage/`. `C1-check.py union` prints `132 keys, 0 missing, 0 extra, 0 duplicated, 0 errors`.
2. Every record file passes `C1-check.py record` with 0 errors.
3. Every `remove` carries a command and output that the reviewer reproduces in the worktree.
4. Every `keep`, `rewrite` and `lesson` is justified by its evidence and worded as C1 section 5 requires.
5. No triage task decides a `Decide` row. Each such row is recorded `decided` and cites its C2 entry.
6. Every `Decide` row is asked at Gate 2 with ranked options and a recommendation. C2 is filled with the answers before the freeze, and `C1-check.py c2` prints `0 pending, 0 errors`.
7. The human/Appium-check rows (the floor-name DIU measurement, the drag-handle Appium run and the expand-toggle Appium run) are rewritten with numbered steps, a `Pass:` condition and a `<path>:<line>` anchor at a named commit.
8. No rewritten row or appended lesson line names a backlog row by line number. The checker's LINE-REF pattern enforces this.
9. `.sir/backlog.md` equals the C1 section 6 construction from the records and C2. `C1-check.py apply` prints `backlog 0 differing lines`.
10. A heading the triage empties is removed with its blank line. No doubled blank line and no trailing blank line remain (`layout 0 violations`).
11. `.sir/lessons.md` is its original 61480 bytes followed only by appended lines in the C1 section 5 shape.
12. Both files keep CRLF = LF = CR by Python byte count.
13. MASTER-158 is unchanged under `## Permanently deferred`.
14. No `src/`, test or `docs/` change: the session's diff against `chore/repo-audit` touches only `.sir/` paths.

## Scope

All 132 open rows (E1, Gate 1). The work is the six range record files (T1-T6) and the application of their dispositions and of C2's answers to `.sir/backlog.md` and `.sir/lessons.md` (T7). Standing-rule rows are routed to `.sir/lessons.md` (E2, Gate 1).

### Not in scope

- Any change to `src/`, tests or `docs/`, including one-line fixes that a row names. Such a row is kept or rewritten as work. This is enforced by the SCOPE criteria of T1-T7 and by definition-of-done item 14.
- MASTER-158 (`## Permanently deferred`), which stays byte-identical. This is enforced by T7 criterion 9.
- Deciding a row that is not a `Decide` row at `4d874be`. If a triage task finds one that needs the user, it rewrites it as a `Decide` row (C1 section 7), and that row is named at Gate 3.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Where does the session worktree live? | `C:/dm-wt/20261118-0114-backlog-triage`, branch `agent/20261118-0114-backlog-triage` | 1 (repo) | `.sir/config.json` `git.worktreeRoot`, `git.branchPrefix`; `.sir/lessons.md:39` (MAX_PATH) |
| Can two tasks build at once? | No: the solution shares `obj/`. No task in this session builds or runs tests at all, so T1-T6 run in parallel. | 1 (repo) | `.sir/lessons.md:116-117`; C1 section 7 |
| Does any task inject a defect into a live file? | No. Every must-fail demonstration runs in-memory in `C1-check.py self-test` (20 passed, 0 failed). | 1 (repo) | `contracts/C1-check.py` |
| How does a triage task re-measure without git? | It reads the worktree, whose content outside `.sir/sessions/` stays `4d874be`'s until T7. A row that only git, a build or a test could settle is `keep`, with a `NOT RUN:` line naming the command. | 1 (repo) | implementer hard rule; C1 section 7 |
| May a row outside the Process and Measurement sections be routed as `lesson`? | Yes. discussion.md applies "route per E2" to every row. | 3 (judgment) | discussion.md, Decisions |
| Does a rule already stated in a framework skill count as `remove`? | Yes, citing the skill file's absolute path with `grep -n` output. | 3 (judgment) | C1 section 4, `evidence` |
| Is the checker part of the frozen contract? | Yes: `contracts/C1-check.py` is the operative definition of C1 sections 3-6, so every reviewer reads the same instrument. | 3 (judgment) | architect report |
| Does `.sir/backlog.md` line 3 point at the new record? | Yes: one sentence naming `.sir/sessions/20261118-0114-backlog-triage/triage/` (C1 section 6, step 2), so a reader can find the evidence. | 3 (judgment) | `.sir/backlog.md:3` names the 20261029-1708 triage the same way |
| Which date and session does an appended lesson line carry? | The original row's, with `(source: .sir/backlog.md row <session> <subject>, routed by 20261118-0114-backlog-triage)`. Appended lines are therefore not in date order, as `.sir/lessons.md:16-43` already are not. | 1 (repo) | `.sir/lessons.md:16-43` |
| How are the record files committed? | Each triage task's record file is committed from the worktree under `.sir/sessions/20261118-0114-backlog-triage/triage/`, as the precedent did. | 1 (repo) | `.sir/sessions/20261029-1708-dir-backlog-to-sir/state.json` (T1-T6 commits) |
| How are the 132 rows split? | Six ranges: 7-30 (21), 34-58 (25), 62-87 (23), 91-109 (19), 110-137 (25), 145-163 (19). | 1 (repo) | `C1-check.py populations` |

## Contracts

- `contracts/C1-triage-record.md`: the record format, new-text shapes, T7's construction and the error cases.
- `contracts/C1-check.py`: the checker (`populations`, `self-test`, `record`, `c2`, `union`, `apply`).
- `contracts/C2-gate2-decisions.md`: the Gate 2 decisions D1-D21, with premises, answers and per-row dispositions.

## Open questions

None. The 21 decisions were answered at Gate 2, every one on its recommended option, and the answers are frozen in `contracts/C2-gate2-decisions.md`. D22 assigned definition-of-done items 6 and 14 (the session-level half) to the orchestrator.
