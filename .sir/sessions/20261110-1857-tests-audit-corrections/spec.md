# Tests audit corrections

Correct four stale or inaccurate statements in `docs/audit/05-tests.md` and `docs/audit/06-cross-cutting.md`, one
per backlog row (the elided Validators path, the coverlet "did not move in between" clause, the
`DependencyInjection.cs` rename, and the TEST-023/029/030 reclassifications and closer), plus two unmatched-bold
defects found by this session's own census; then remove the four rows from `.sir/backlog.md` at Gate 3.

## Context

Every row was re-measured at the base `07f0512` (`chore/repo-audit`) by the orchestrator, with git, before
decomposition; the record is `measurement.md` (M0-M5) and the frozen facts are in
`contracts/C1-tests-audit-corrections.md` section 5. Re-measurement changed the substance of three rows:

- Coverlet clause (M2): the clause is **true**. Coverlet stood at line 9 in all 105 commits from `413be78` to the
  removal's parent, with a control. What it lacked was evidence, so the correction supplies the census rather than
  narrowing the wording.
- `DependencyInjection.cs` (M3): all five citations were exact when written (blob `e2c9267` at both authoring
  commits), and the file's **content** changed as well as its name, so a filename swap would point at different code.
  Each entry gets a block giving the rename, the as-written verdict, and the labelled counterpart at `07f0512`.
- TEST-023 and TEST-030 (M4.1): P1 and PA, run over the `2603170` tree, show every undetermined line and method name
  exact on the day: DRIFT. The TEST-029 closer defect is confirmed by a bold-parity census (BP), which also found two
  more unmatched-bold blocks that no backlog row names (`TEST-012`, `XCUT-015`).

The register keeps original wording standing and corrects by dated `AMENDED` blocks (M5). Four mechanical defects
(an unpasteable command and three rendering defects) cannot be cured by a later block, so they are fixed in place
as exact whole-line replacements R1-R4, each recorded verbatim in its entry's new block.

## Definition of done

- [ ] D1. Elided Validators path: `05:274`'s command carries the full path (R1), recorded in a new `TEST-001` block (C1 section 5.1).
- [ ] D2. Coverlet clause: a new `TEST-032` block records the 105-commit census as the clause's evidence, classified `not a correction -- confirmation (measured)`, the clause left standing (C1 section 5.7).
- [ ] D3. `DependencyInjection.cs`: new blocks on `TEST-002` (D3a), `XCUT-001` (D3b) and `XCUT-006` (D3c) give the rename, DRIFT, and the labelled counterparts; the five citations stand as written (D3d) (C1 sections 5.2, 5.8, 5.9).
- [ ] D4. TEST-023 (D4a) and TEST-030 (D4b) Correction 2 reclassified UNDETERMINED -> DRIFT in new blocks; TEST-029's closer fixed in place (R2) and recorded (D4c) (C1 sections 5.4-5.6).
- [ ] D5. The two unmatched-bold blocks fixed in place and recorded: `TEST-012` (R3, D5a), `XCUT-015` (R4, D5b) (C1 sections 5.3, 5.10).
- [ ] D6. Both files: `LONE_CR 0`, `CR == CRLF`, CRLF equals the base count plus TAC's `ADDED_LINES`.
- [ ] D7. Both files: TAC prints `BASE_MATCH yes` with every listed `REPL` at `NEW 1 OLD 0` (no other byte changed).
- [ ] D8. House form in every block: `as numbered at 07f0512`, the ATTR literal on history blocks, no `.sir/backlog.md` line labels, no scratch paths (C1 U2-U5).
- [ ] D9. BP prints `odd 0` for both files.
- [ ] D10. Only the two audit files change across the session's task commits.
- [ ] D11. After the merge, TAC (census, `BASE_MATCH yes`) and BP (`odd 0`) re-run on both main-checkout files.
- [ ] D12. The four rows are removed from `.sir/backlog.md` at Gate 3, with nothing else changed except Gate 3's own appends.

## Scope

T1: seven blocks and three replacements (R1-R3) in `docs/audit/05-tests.md`. T2: three blocks and one replacement
(R4) in `docs/audit/06-cross-cutting.md`. Forms, anchors and content in C1 sections 2 and 5. The backlog edit at Gate 3.

### Not in scope

- The other rows under `## Corrections to the audit record` and the other `## Appended at Gate 3` rows, including the
  session-path-citation questions on `MASTER-180` and `MASTER-110`. They stay in the backlog unchanged.
- Rewriting any entry's original text or front matter, including the five `DependencyInjection.cs` citations
  themselves. Enforced by D7 (`BASE_MATCH yes`) and C2-8.
- Any source, test or build change. Enforced by D10 (orchestrator, `git diff --name-only`).
- Deferred work, to be appended to `.sir/backlog.md` at Gate 3:
  - The other Infrastructure-`DependencyInjection.cs` citations outside these two files: `00-master-index.md:33`,
    `02-infrastructure.md:7`, `:41`, `:1011`, `:1107`, `:1149`, `03-app-shell-and-views.md:89`, as numbered at `07f0512`
    (M3). Instrument: `grep -n DependencyInjection docs/audit/*.md`.
  - BP flags six blocks in other audit files, not yet read by eye: `01-domain-application.md:2245`,
    `02-infrastructure.md:159`, `03-app-shell-and-views.md:411`, `:840`, `:2314`, `04-app-controls-and-theme.md:1366`,
    as numbered at `07f0512`. BP was controlled on 05's `**]**` convention only; 02 closes with a plain `]`.
    Instrument: `python -I .sir/sessions/20261110-1857-tests-audit-corrections/bp.py docs/audit/0[1-4]*.md`.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Rewrite or annotate stale wording? | Annotate: new dated `AMENDED` blocks at the end of each entry; original prose and front matter stand, including the five `DependencyInjection.cs` citations, although that row's wording says "instead of" | repo | `02-infrastructure.md:1067-1068`; `01-domain-application.md:907`; `05-tests.md:117-118` (M5) |
| Mechanical defects inside earlier amendment blocks (R1-R4) | Fixed in place as exact whole-line replacements, each recorded verbatim by the new block. No file precedent edits inside an amendment block; a later block cannot make a command pasteable or a delimiter render | judgment | in-place-with-record precedent for entry prose: `05:682`, `:982`; counter-precedent `05:117-118`; decided at Gate 2 (Q2: in place, recorded) |
| Closer form for new blocks, both files | `**]**`, with every block's bold markers paired | repo | 44 of 44 closers in 05 end `**]**` (C1 section 4 `closers`); 06's one block is itself the defect |
| TEST-029 fix form | `these five.** **]**`: keeps the bold sentence and the `**]**` token | judgment | the alternative `five.****]**` fails TAC's `REPL` control (C1 section 3) |
| Label for the coverlet clause | `Classification: not a correction -- confirmation (measured)` | repo | `05:2305`, `:1334`, `:1348` |
| "As written" commits | `2603170` for 05, `676a8f3` for 06 (each file's authoring commit) | repo | M0 (`git log --diff-filter=A`) |
| History facts | Orchestrator-measured at spec time and stated as told (ATTR); tasks run without git | repo | `measurement.md` M1-M4; C1 U4 |
| Agent and domain | `sir-architect` / `architecture`; no composed discipline applies to audit prose | repo | `20261110-1618-audit-record-corrections/manifest.json` |
| Can tasks build concurrently? | Nothing to order: no task builds, tests or runs git | repo | manifest `owns`; C1 section 1 |
| Injected defects into tracked files? | None: every must-fail control runs on a scratch copy | repo | C1 sections 3-4; criteria C1-3, C1-4, C2-3, C2-4, C2-9 |
| Is `Edit` safe on these files? | Yes: 0 lone CR in both (Python byte census) | repo | M0; C1 section 1 |
| May register prose cite `.sir/` paths? | Yes: the register is process documentation and already cites session paths | repo | `05:284` cites `.dir-sdlc/sessions/2026-09-11-measure-then-close-the-test-gaps/handover-addendum.md`; `05:1721`, `:2124` cite a session's `C1-measurement.md` |

## Contracts

- [`contracts/C1-tests-audit-corrections.md`](contracts/C1-tests-audit-corrections.md): universals U1-U6, base digests
  and census, block form and replacements R1-R4, the TAC instrument (sha256 `2e6a3a93...`), BP by path and digest
  (`01ecfd8a...`), shared literals, per-block content, and the needle cross-check.

## Open questions

None. Gate 2 decided all three as drafted: Q1, the coverlet clause is recorded with its census and not narrowed
(D2); Q2, R1-R4 are fixed in place and recorded verbatim; Q3, the `TEST-012` and `XCUT-015` defects are fixed in
this session (D5). The orchestrator owns D10-D12 on the evidence named in `coverage.md`.
