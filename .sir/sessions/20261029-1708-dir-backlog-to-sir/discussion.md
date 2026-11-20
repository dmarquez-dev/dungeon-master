# Convert the DIR backlog and the open audit register into SIR backlog items

Turn the DIR-era deferred work (`.dir-sdlc/backlog.md`), the unfinished rows of the audit register
(`docs/audit/00-master-index.md`), and the 6b-xxi orientation brief into a `.sir/backlog.md` (plus a seeded
`.sir/lessons.md`), so later sir-sdlc sessions can read them at rung 1 and finish the repo audit and the
backlog work.

## Context

- The repo-audit arc ran under DIR (dir-sdlc) through sub-session **6b-xx**
  (`.dir-sdlc/sessions/2026-10-16-infra-layer-batch/`), whose orientation brief for the next session is
  `.dir-sdlc/sessions/2026-10-16-infra-layer-batch/BRIEF-session-6b-xxi.md` (314 lines). That brief is the
  primary source for the recommended next work and the instrument catalogue (18 forms; the first 14 live in
  6b-xix's `measurement.md` §12, forms 15-18 in 6b-xx's `measurement.md`).
- Measured at discuss time (2026-10-29, main checkout, `chore/repo-audit` at `03fa5b3`):
  - Push state: **0 ahead / 0 behind** `origin/chore/repo-audit`
    (`git rev-list --left-right --count origin/chore/repo-audit...chore/repo-audit` → `0 0`). The brief's
    "seven commits UNPUSHED" warning is stale; the branch was pushed 2026-10-27.
  - Working tree clean; only the main checkout in `git worktree list`.
  - `.dir-sdlc/backlog.md`: **181 items** (`grep -c '^- 20' .dir-sdlc/backlog.md`), 525 lines, ~20 section
    headings (`grep -n '^#'`), sections run from Session 2 through 6b-xix plus a 6b-xx append. The brief
    records it as carrying **8 lone CR** bytes (ATTRIBUTED, not re-measured here).
  - Register: 181 rows. Rows still needing work, from a Python split of `00-master-index.md:188-370` on
    unescaped `|`, excluding statuses beginning `done`, `deliberately unchanged`, `FALSIFIED` and
    `filed found-and-fixed`: **12 open** — `MASTER-110`, `126`, `128`, `137`, `139`, `142`, `143`, `144`,
    `157`, `158`, `178`, `180` — and **5 part done** — `MASTER-081`, `094`, `098`, `108`, `130`. This agrees
    with the brief's 12 / 5. The predicate has no near-miss control; the spec phase must re-validate it
    (the brief warns the classifier needed fixing in two prior sessions).
  - `.sir/` did not exist before this session. There is no `.sir/config.json`; defaults apply.
- Open-layer shape (from the brief, ATTRIBUTED): TEST 6, CORE 4, UIKIT 1, `UI, XCUT` 1; severity S4 10, S5 2.

## Decisions

1. **Track: Standard.** One output surface (`.sir/`), but ~200 items need parallel reading and a reviewable
   triage, which a single Small-track agent cannot give.
2. **Scope: everything, triaged.** Every one of the 181 `.dir-sdlc/backlog.md` items and all 17
   open/part-done register rows are considered. An item already closed by later work is **dropped from the
   SIR backlog, and the drop is recorded with its evidence** (the register status, the later backlog note,
   or the session record that closed it) in a session artifact, so nothing disappears silently.
3. **Triage depth: recorded evidence only.** Status is decided from the register, later backlog entries and
   DIR session records — **no source code is re-measured**. Every carried entry is marked
   *re-measure before scheduling*, because the arc measured 6 of 6 selected rows false-or-stale in 6b-xx.
   Where the records cannot decide whether an item is closed, carry it and say so; do not guess closed.
4. **Format.** `.sir/backlog.md` follows `<plugin root>/templates/backlog.md`: one line per item — date,
   source session id, the action in a sentence. Each line carries a back-pointer (the `.dir-sdlc/backlog.md`
   line, or the `MASTER-NNN` id) so the full DIR detail stays one hop away. The original item's date and
   DIR session id are preserved, not replaced with today's.
5. **Session bundles.** The SIR backlog also groups entries into proposed session-sized clusters, following
   the 6b-xxi brief's recommendation: TEST six-row sweep (`126`, `128`, `137`, `139`, `143`, `144`; style
   and naming sub-clusters), the CORE two-row sweep (`110`, `180`), CORE heavy rows (`157` L/high, `178`
   M/medium — note its neighbour, the `ObjectRotation`/`PathSegmentKind` enum renames needing EF
   migrations), `MASTER-142` on its own blast radius, the five part-done rows, and the non-register
   backlog clusters the triage finds.
6. **`MASTER-158` is permanently deferred.** It has been excluded at Gate 1 for five consecutive sessions;
   the SIR backlog records it as permanently deferred, with that reason, rather than as a candidate. This
   session **does not edit the register row** (it keeps all writes under `.sir/`); the entry says so, so a
   reader of the register knows where the decision lives.
7. **Seed `.sir/lessons.md`.** Carry, one line each per the lessons template, the instrument catalogue's
   18 forms and the arc's process lessons that the SIR framework will otherwise relearn: the CRLF / lone-CR
   tool traps (`sed -i`, `Write`, `Edit` normalising lone CR, trailing-tab stripping), worktrees under
   `C:\dm-wt` (MAX_PATH breaks the WinUI XAML compiler), verify in the main checkout, verifying commit
   bodies with `git cat-file -p` rather than `%B`, control-must-match-defect-class / second-predicate rule,
   the classifier's `deliberately unchanged` / `part done` ordering, and escaping `|` in register cells.
   Sources: the 6b-xxi brief, 6b-xix and 6b-xx `measurement.md`, and the DIR session records. Lessons are
   dated and attributed to the DIR session that learned them.
8. **`.dir-sdlc/` is read-only for this session.** Nothing under `.dir-sdlc/` or `docs/audit/` is edited;
   in particular `.dir-sdlc/backlog.md`'s lone CR bytes are never at risk. New `.sir/` files are new files,
   so the CRLF-destruction traps do not apply to them, but no existing tracked file is rewritten.
9. **Not in scope:** fixing any backlog item or register row, re-measuring code, pushing, or editing the
   register. The session's deliverables are `.sir/backlog.md`, `.sir/lessons.md`, and the triage record
   in the session directory.
