# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 row 179, CORE-022 attribution | T2 (C2-2, C2-3, C2-4, C2-5) | covered | met (reviews/T2.md: C2-2, C2-3, C2-4 met; C2-5 met under A2) |
| D2 row 182, GetDefaultAnchor drift | T2 (C2-6) | covered | met (reviews/T2.md: C2-6) |
| D3 row 183, RotationDirection command and drift | T2 (C2-7, C2-8) | covered | met (reviews/T2.md: C2-7 met under A3; C2-8 met) |
| D4 row 184, five zero-member sites carry R184 | T1 (C1-7, C1-8, C1-9), T2 (C2-9) | covered | met (reviews/T1.md: C1-7, C1-8 met under A1, C1-9 met; reviews/T2.md: C2-9 met) |
| D5 row 190, both attributions | T1 (C1-6), T2 (C2-10) | covered | met (reviews/T1.md: C1-6 met under A1; reviews/T2.md: C2-10 met under A2) |
| D6 row 191, MASTER-119 citation | T1 (C1-5) | covered | met (reviews/T1.md: C1-5 met under A1) |
| D7 line endings, both files | T1 (C1-1), T2 (C2-1) | covered | met (reviews/T1.md: C1-1; reviews/T2.md: C2-1) |
| D8 no existing byte changed | T1 (C1-4), T2 (C2-3) | covered | met (reviews/T1.md: C1-4; reviews/T2.md: C2-3) |
| D9 only the two audit files in task commits | orchestrator: no task has git | uncovered; orchestrator, evidence `git diff --name-only <merge-base chore/repo-audit>..<branch tip>` read before the merge | met, orchestrator row: `git diff --name-only c90da08..agent/20261110-1618-audit-record-corrections` (merge-base c90da08, tip 291e3d1) printed `docs/audit/00-master-index.md` and `docs/audit/01-domain-application.md` only |
| D10 six backlog rows removed at Gate 3 | orchestrator: `.sir/backlog.md` is owned by no task | uncovered; orchestrator, evidence a byte-level edit with a pre-edit lone-CR census and a prefix/suffix byte comparison against the pre-edit copy | met, orchestrator row: a Python byte-level edit (`backlog_edit.py`) removed lines 179, 182, 183, 184, 190 and 191, each matched on its own prefix, and appended 1 row (the spec-routed MASTER-171 `:398` item). Pre-edit census `CR 191 CRLF 191 LONE_CR 0`; post-edit `CR 186 CRLF 186 LONE_CR 0`. `PREFIX_MATCH yes` for the 53537 bytes before line 179, and `EXPECTED_MATCH yes` for the whole file against the pre-edit copy with the six lines removed and the row appended. Must-fail control, one byte flipped at offset 100: `PREFIX_MATCH no`, `EXPECTED_MATCH no`. `git diff --numstat -- .sir/backlog.md` printed `1 6` |
| D11 main-checkout ARC after merge | orchestrator: post-merge, no task exists | uncovered; orchestrator, evidence ARC (C1 section 3) run on both main-checkout paths after `merge-session` | met, orchestrator row, after merge 30c1af3: ARC on main-checkout 00 printed `CR 691 CRLF 691 LONE_CR 0`, `INSERTIONS 4 ADDED_LINES 0`, `BASE_MATCH yes`; on main-checkout 01 `CR 3552 CRLF 3552 LONE_CR 0`, `INSERTIONS 4 ADDED_LINES 189` (3363 + 189), `BASE_MATCH yes`; `git diff --quiet` against the branch tips printed no difference for either file |

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| C1 section 2 insertion forms | T1, T2 | T1, T2 | covered | applied: T1's four spans in 00 (9f1211f), T2's four blocks in 01 (291e3d1) |
| C1 section 3 ARC instrument | T1, T2 | T1, T2 (and each reviewer) | covered | applied: run by T1, T2 and both reviewers; re-run by the orchestrator at the tip (sha256 42f21f81...7b86c) |
| C1 section 4.1 R184 | T1, T2 | T1, T2 | covered | applied: MASTER-130, MASTER-178 spans; CORE-024, CORE-035 blocks |
| C1 section 4.2 R190 | T1, T2 | T1, T2 | covered | applied: MASTER-081 span; CORE-016 block |
| C1 section 4.3 cross-references | T1 | T1 | covered | applied: MASTER-081, MASTER-130, MASTER-178 spans |
| C1 section 5 per-row facts | T1, T2 | T1 (5.6), T2 (5.1-5.3) | covered | applied: MASTER-119 span; CORE-022, CORE-035 blocks |
| C1 section 6.1 OM results | T1, T2 | T1 (OM-2), T2 (OM-1, OM-3, OM-4) | covered | applied: MASTER-119 span; CORE-022, CORE-035 blocks |
