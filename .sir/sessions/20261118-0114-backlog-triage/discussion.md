# Backlog triage

Re-measure every open row of `.sir/backlog.md` at 4d874be, give each a recorded disposition, shrink the backlog to rows that are real work worded as work, and put every Decide row to the user with ranked options and a recommendation at Gate 2.

## Context

- At 4d874be `.sir/backlog.md` holds 133 dated rows, 132 open and 1 under `## Permanently deferred` (MASTER-158). Counted by a Python script matching `^\d{4}-\d{2}-\d{2} \S+ ` per CRLF-split line, grouped by the preceding `## ` heading; the file is 163 CRLF, 163 LF, 163 CR (no lone CR) by a Python byte count.
- Rows per heading at 4d874be: CORE domain and application code 3; App, UI and UIKIT code 18; Test coverage and test strength 25; Build, tooling, line endings and CI 8; Corrections to the audit record 15; Process: criteria, contracts and orchestration 34; Measurement instruments 10; Permanently deferred 1; Appended at Gate 3 19.
- 23 rows have an action beginning `Decide` (same script; control: line :7 matches, line :9 does not). Their lines at 4d874be: :7, :8, :20, :22, :23, :36, :40, :41, :50, :58, :65, :69, :101, :134, :135, :136, :147, :150, :151, :153, :157, :158, :161. 44 rows sit under the Process and Measurement instruments headings; 4 rows are in both sets, 63 in their union.
- The file's own header says every entry must be re-measured before it is scheduled: its claim was true when filed and may not be now.
- Why now: at the recent pace (the last 10 `.sir/log.md` session lines: 14 rows removed, 6 appended) one-row Small sessions would take on the order of a hundred sessions to clear the file; a triage first lets later sessions group by section.
- Precedent: `20261029-1708-dir-backlog-to-sir` triaged the DIR backlog into this file; its manifest split the work as T1-T6 owning one record file each, T7 owning `.sir/lessons.md`, T8 owning `.sir/backlog.md`. Its triage files under `.sir/sessions/20261029-1708-dir-backlog-to-sir/triage/` hold the original disposition and evidence for each carried row.

## Decisions

- Track: Standard (Gate 1, user confirmed the recommendation).
- E1 (ambiguous, resolved at Gate 1): scope is all 132 open rows. The user chose this over the 63 Decide, Process and Instrument rows or the 44 Process and Instrument rows alone.
- E2 (ambiguous, resolved at Gate 1): a Process or Measurement instruments row the triage finds to be a standing rule rather than work becomes one tagged line in `.sir/lessons.md` (`[project]`, `[framework]` or `[harness]`), citing its backlog source, and is removed from the backlog; a row whose rule an existing lessons.md line already states is removed with that line cited. The user chose this over keeping the rules under one backlog heading or removing only duplicates.
- Dispositions per row: already done or stale (remove, with the command and output that show it); still true as written (keep); true but mis-worded (rewrite as work with its check); a standing rule (route per E2); a user decision (ranked options and a recommendation, asked at Gate 2).
- Decide rows are asked at Gate 2 in batched choice dialogs, recommended option first. Rows asking the same question are asked as one: the three citation-path rows from 20261103-1947-core-doc-comments, 20261105-0106-core-two-row-sweep and 20261111-0156-core-audit-corrections (:147, :151, :157 at 4d874be, by `awk 'NR==147||NR==151||NR==157'` on the file).
- No source, test or `docs/audit/` file changes in this session: a row whose fix is a one-liner is kept, worded as work, not fixed here.
- The Permanently deferred row (MASTER-158) is out of scope and stays unchanged.
- Rows that ask for a human or Appium check (for example the 2026-09-16-floor-sidebar-keyboard-route DIU-measurement and drag-handle rows, :25 and :52, and the 20261112-0153-floor-name-tooltip expand-toggle Appium row, :159, at 4d874be) are kept and rewritten as numbered steps with a pass condition and a line anchor at a named commit, per the 20261111-0156 lesson in `.sir/lessons.md`, not removed.
- Task split: parallel triage tasks by backlog section, each owning only its own record file in the session directory; one final task owns `.sir/backlog.md` and `.sir/lessons.md` and applies the dispositions after Gate 2's answers. `.sir/backlog.md` and `.sir/lessons.md` are CRLF with no lone CR, so they must keep that (no `Write`, no `sed -i`; never end an `Edit` new_string on whitespace).
- Every row's line number is cited as numbered at 4d874be, and a rewritten row names its rows by session and subject, not by line, since this session's own edit moves them.
