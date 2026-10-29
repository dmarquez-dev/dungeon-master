# Convert the DIR backlog and the open audit register into SIR backlog items

Goal: one triaged `.sir/backlog.md` and a seeded `.sir/lessons.md`, built from recorded evidence only, so later
sir-sdlc sessions can read the remaining audit and backlog work at rung 1.

## Context

- The repo-audit arc ran under DIR through 6b-xx. Its deferred work sits in `.dir-sdlc/backlog.md` and the
  unfinished rows of `docs/audit/00-master-index.md`; its next-session guidance sits in
  `.dir-sdlc/sessions/2026-10-16-infra-layer-batch/BRIEF-session-6b-xxi.md` (314 lines). `.sir/` holds only this
  session's directory.
- `.dir-sdlc/backlog.md`: **181 items** (`grep -c '^- 20' .dir-sdlc/backlog.md`), 525 lines (`wc -l`), spread
  over **21** headed groups (awk over `^#` headings counting `^- 20` lines under each; the 21 per-group counts
  sum to 181). Each item opens on one `- 20` line; some continue on indented lines (e.g. :95-99 continue :94),
  which are never keys. Bytes: **533 CR, 525 CRLF, so 8 lone CR** (Python `bytes.count`, worktree, `03fa5b3`).
- Instrument catalogue locations, corrected from `discussion.md:13`: forms 1-10 exist only as an unnumbered
  10-row table at `.dir-sdlc/sessions/2026-10-09-uikit-layer-batch/BRIEF-session-6b-xix.md:179-188`; forms 11-14
  are in 6b-xix's `measurement.md` §12 (:848, :867) and §14 (:950, :971); forms 15-18 in 6b-xx's
  `measurement.md` (:15, :281, :342, :385).
- Register: **181 rows** — **164 closed** (156 `done`, 4 `FALSIFIED`, 3 `deliberately unchanged`,
  1 `filed found-and-fixed`), **12 open** and **5 part done**. Open = `MASTER-110`, `126`, `128`, `137`, `139`,
  `142`, `143`, `144`, `157`, `158` (empty status cell) plus `178`, `180` (status begins `open`). Part done =
  `MASTER-081`, `094`, `098`, `108`, `130`. Measured with a Python classifier splitting `^| <rank> | MASTER-NNN |`
  rows on unescaped `|`, reading cell 10, stripping `*`, prefix-matching `deliberately unchanged` before
  `part done` before `done`. Controls: prefixing `MASTER-001`'s status with a junk token raised `UNCLASSIFIED`
  from 10 to 11 (must-fire); `MASTER-145`/`151`/`154` classify `deliberately unchanged` (near-miss). The figure
  agrees with discuss's exclusion-based predicate — two predicates, one answer.
- Session branch `sir/20261029-1708-dir-backlog-to-sir` at `03fa5b3`, worktree `C:/dm-wt/sir-backlog`.

## Definition of done

- [ ] D1. Triage record: every one of the 181 `.dir-sdlc/backlog.md` items, keyed by its line number, appears
  exactly once across the triage files, with one disposition from `carry`, `drop`, `defer-permanent`.
- [ ] D2. Triage record: every one of the 17 open/part-done register rows appears exactly once, keyed by
  `MASTER-NNN`, with one disposition.
- [ ] D3. Every `drop` cites recorded evidence (a register status cell, a later backlog line, or a DIR session
  record) as a path with line number; an item the records cannot decide is `carry`, never `drop`.
- [ ] D4. `.sir/backlog.md` exists, opens with `# Backlog`, and holds exactly one entry line per `carry` and
  per `defer-permanent` triage row — no more, no fewer.
- [ ] D5. Every `.sir/backlog.md` entry line preserves the source item's original date and DIR session id,
  carries its back-pointer (`.dir-sdlc/backlog.md:<line>` or `MASTER-NNN`), and carries the marker
  `re-measure before scheduling`.
- [ ] D6. `.sir/backlog.md` groups entries under the 13 session-bundle headings of `contracts/bundles.md` — the
  TEST six-row sweep (`126`, `128`, `137`, `139`, `143`, `144`), the CORE two-row sweep (`110`, `180`), CORE
  heavy rows (`157`, `178`, and the `ObjectRotation`/`PathSegmentKind` rename at `.dir-sdlc/backlog.md:457`),
  `MASTER-142` alone, the five part-done rows, seven non-register clusters, and `Permanently deferred` — a
  heading appearing exactly when at least one entry sits under it, and every entry line under exactly one.
- [ ] D7. `MASTER-158` is `defer-permanent` with its reason (excluded at Gate 1 five consecutive sessions)
  and states that the register row itself was not edited.
- [ ] D8. `.sir/lessons.md` exists, opens with `# Lessons`, and has one line per lesson, dated and attributed
  to the DIR session that learned it, covering all 18 instrument-catalogue forms, each named by its number.
- [ ] D9. `.sir/lessons.md` carries one line for each of the nine process lessons named in discussion
  decision 7: `sed -i` CRLF loss, `Write` CRLF loss, `Edit` normalising lone CR, `Edit` stripping trailing
  tabs, worktrees under `C:\dm-wt`, verify in the main checkout, `git cat-file -p` rather than `%B`,
  control-must-match-defect-class with the second-predicate rule, and the register classifier ordering with
  `|` escaping.
- [ ] D10. The session branch changes nothing outside `.sir/`: `git diff --name-only
  chore/repo-audit...sir/20261029-1708-dir-backlog-to-sir` lists only paths beginning `.sir/`.

## Scope

All 181 DIR backlog items and all 17 open/part-done register rows, triaged from the register, later backlog
entries and DIR session records. Deliverables: `.sir/backlog.md`, `.sir/lessons.md`, and the triage record under
`.sir/sessions/20261029-1708-dir-backlog-to-sir/triage/`.

### Not in scope

- Fixing any backlog item or register row; reading or re-measuring source code.
- Editing anything under `.dir-sdlc/` or `docs/audit/`, including the `MASTER-158` row.
- Pushing.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Where does the triage record live? | Under `.sir/sessions/20261029-1708-dir-backlog-to-sir/triage/` on the session branch, so it merges into this session's directory | 1 | `discussion.md:70-71` (decision 9: "the triage record in the session directory") |
| What counts as an open register row? | Status cell empty, or beginning `open` after stripping `*`; part done = beginning `part done` | 1 | `docs/audit/00-master-index.md:188-370`; classifier and controls in Context |
| Date and session id for a register row, which has no date column | The filing session and date its own status cell or source names; otherwise the audit session `2026-08-17-code-quality-audit`, 2026-08-17 | 4 | No date column at `docs/audit/00-master-index.md:186`; `.dir-sdlc/sessions/2026-08-17-code-quality-audit/` is the session that wrote the register |
| A backlog item and a register row describing the same work | Both kept as separate entries, never merged; a producer cross-references the other wherever it notices, best-effort, symmetry not checked (Gate 2 decision) | 4 | Decision 2 requires every item considered and nothing to disappear silently (`discussion.md:36-39`); `contracts/triage-file.md` §3 column 9, §4 |
| What may justify `drop` | Only a register status, a later backlog line, or a DIR session record; never source code | 1 | `discussion.md:40-43` (decision 3) |
| Is the arc's push state a constraint? | No: 0 ahead / 0 behind at discuss time | 1 | `discussion.md:16-18` |
| Does a failing `dotnet build`/`dotnet test` probe matter? | No: the session writes prose only; Python and read-only git are the instruments, and both ran | 1 | `state.json` `verification` |
| Line endings of the new `.sir/` files | Whatever the edit tools emit; they are new files, and `* text=auto` normalises on commit | 1 | `.gitattributes:1`; `discussion.md:66-68` |
| How are forms 1-10 numbered, when no record numbers them? | Row order of the only enumerated record: `BRIEF-session-6b-xix.md:179` = FORM 1 … `:188` = FORM 10; plus one lesson recording that 6b-xx's reviews number two of them differently | 1 | `BRIEF-session-6b-xix.md:177-188`; agrees with `BRIEF-session-6b-xxi.md:46` (form 1) and 6b-xx `reviews/T1.md:60` (form 7); conflicts with 6b-xx `reviews/T3.md:57`, `reviews/T2.md:51` |
| A DIR-process item that is obsolete rather than closed (e.g. "before reusing Session N's contracts") | `carry` under `process-and-criteria`; no `obsolete` disposition | 1 | `discussion.md:36-43` (decisions 2-3: drop only what later work closed; never guess closed) |
| Does the review phase's own `.sir/backlog.md` append, or wrap's `.sir/lessons.md` append, block merging a branch that adds those files? | No: review merges first and appends after; wrap runs after the merge | 1 | `sir-phase-review/SKILL.md:108-109` |
| Which agent implements prose-only tasks? | `sir-architect`, domain `architecture`, for all eight: there is no docs agent, and the four composed disciplines are code conventions none of which applies | 4 | `manifest.schema.json` agent/domain enums; `disciplines/*.md` |

## Contracts

- [`contracts/triage-file.md`](contracts/triage-file.md) — the triage-row format: files and populations, layout,
  the nine columns, error cases. Producers T1-T6, consumer T8.
- [`contracts/bundles.md`](contracts/bundles.md) — the 13 bundle ids, their heading lines and order, membership
  rules, and T8's rendering guarantees. Producers T1-T6, consumer T8.

The `.sir/backlog.md` entry-line shape is frozen in T8's criterion 3, not in a contract: T8 is its only producer
and no task consumes it.

## Open questions

None.
