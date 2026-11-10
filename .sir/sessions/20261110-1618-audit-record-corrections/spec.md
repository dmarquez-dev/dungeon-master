# Audit record corrections

Correct six stale or inaccurate statements in the audit register (`docs/audit/00-master-index.md`,
`docs/audit/01-domain-application.md`) by insertion only, one per unscheduled backlog row at `.sir/backlog.md:179`,
`:182`, `:183`, `:184`, `:190` and `:191`, and remove those six rows from the backlog at Gate 3.

## Context

The five scheduled backlog headings are empty; these six rows were appended at Gate 3 by
20261105-0106-core-two-row-sweep, 20261105-1907-core-heavy-rows and 20261109-1641-part-done-rows, each carrying
its own instrument. Every row was re-measured at the base `c90da08` (`chore/repo-audit`); four had drifted lines,
and the facts the corrections state are frozen in `contracts/C1-register-corrections.md` sections 4-6 with the
command beside each. Re-measurement changed three rows' substance:

- Row 179: the `C1 and C2 contracts` sentence is at 01 `:1677-1678`; `C1-measurement.md:203-204` does state the 176
  and 94, so only the 112 and the 288 are C3-only. The row's own `grep -nw -e 112 -e 288` over-matches (C1 section 5.1).
- Row 184: the population is five sites, not four: 01 `:3038` (`the blocker is a data migration rather than a
  renumbering`) was omitted by the row (C1 section 4.1).
- Row 191: Correction 2 ends at `:2567`, not `:2564`; the cited `:2313` was inside it when written (OM-2, DRIFT).

The register's house rule is that no previous wording is deleted (00 cells: `Previous status wording, retained
unchanged below.`; 01 blocks: `Nothing below deletes any previous wording.`), so every correction is a new
dated span (00) or a new `AMENDED` block at the end of its entry (01).

## Definition of done

- [ ] D1. Row 179: a `CORE-022` block corrects the "C1 and C2 contracts" attribution (C1 section 5.1), ERROR (attribution).
- [ ] D2. Row 182: a `CORE-035` block records the `ObjectKindInfo.cs:24-28` -> `:40` drift (C1 section 5.2), DRIFT per OM-1.
- [ ] D3. Row 183: the `CORE-035` block restates the `RotationDirection` command with its filters and its 6-line result, and the `:139`/`:150` -> `:196`/`:207` drift (C1 section 5.3), DRIFT per OM-3; and the same for the `ScaleDirection` row (5 lines, `:166` -> `:223`), decided at Gate 2.
- [ ] D4. Row 184: all five zero-member sites carry a note stating R184 verbatim (C1 section 4.1).
- [ ] D5. Row 190: the `MASTER-081` span and a `CORE-016` block correct the attribution to `discussion.md:26` plus `spec.md:22-24` (C1 section 4.2), ERROR (attribution).
- [ ] D6. Row 191: the `MASTER-119` span corrects `01-domain-application.md:2313` to `:2560-2567` at `c90da08` (C1 section 5.6), DRIFT per OM-2.
- [ ] D7. Both files: `LONE_CR 0`, `CR == CRLF`, and CRLF equals the startRef count plus ARC's `ADDED_LINES`.
- [ ] D8. Both files: ARC prints `BASE_MATCH yes` (no existing byte changed; insertions only).
- [ ] D9. Only the two audit files change across the session's task commits.
- [ ] D10. The six rows are removed from `.sir/backlog.md` at Gate 3, with nothing else in the file changed except Gate 3's own appends.
- [ ] D11. After the merge, ARC on both main-checkout files prints the D7 census and `BASE_MATCH yes`.

## Scope

Insertions into `docs/audit/00-master-index.md` (T1: four inline spans, in `MASTER-081`, `MASTER-119`, `MASTER-130`,
`MASTER-178`) and `docs/audit/01-domain-application.md` (T2: four blocks, at the end of `CORE-016`, `CORE-022`,
`CORE-024`, `CORE-035`), in the forms of C1 section 2; the backlog removal at Gate 3.

### Not in scope

- The older audit-correction rows at `.sir/backlog.md:88-108`; `docs/audit/05-tests.md` (`:177`); the session-path-citation questions (`:170`, `:178`); the `obj` rename population (`:189`). These stay in the backlog unchanged.
- Rewriting or deleting any existing register text. Enforced by D8 (`BASE_MATCH yes`, criteria C1-4 and C2-3).
- Any source or test code change. Enforced by D9 (orchestrator, `git diff --name-only`).
- The bare `grep -rn "RotationDirection" src/` in the `MASTER-171` narrative at `00-master-index.md:398`, found by the architect: deferred work, to be appended to `.sir/backlog.md` at Gate 3 (decided at Gate 2).

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Rewrite or annotate stale wording? | Annotate: insertions only, quoting the old words verbatim | repo | 00 `:318`, `:366` `retained unchanged`; 01 `Nothing below deletes any previous wording.`; 01 `:1370` (a block correcting an earlier block) |
| Where do 01 corrections go? | A new block at the end of each entry, after its last `**]**` line | repo | `.sir/lessons.md:72`; C1 section 2.2 table |
| Agent and domain | `sir-architect` / `architecture`; no discipline applies | repo | register tasks in 20261105-0106 (T4), 20261109-1641 (T3), 20261105-1907 (T2-T4) manifests |
| Can tasks run concurrently? | Yes: neither builds, tests or injects into a tracked file; ownership is disjoint | repo | manifest `owns`; must-fail controls run on scratch copies (C1-4, C2-3) |
| Shared row-184 phrasing | R184: `adding a zero member renumbers the persisted ordinals, so it is a renumbering that needs a data migration` | repo | `PathSegmentKind.cs:11`, `:16`, no `=`; `AreaConfiguration.cs:16`; `PathSegmentJsonRoundTripTests.cs:28-29` (C1 section 4.1) |
| Classifications needing git history (rows 182, 183, 191, 179) | DRIFT, DRIFT, DRIFT; 179 ERROR (attribution) | repo | orchestrator OM-1..OM-4, C1 section 6.1, commands stated |
| Are `.sir/` paths acceptable in register prose? | Yes: the register is process documentation and already cites session paths | repo | 00 `:269`, `:318` |
| Are Edit-tool edits safe on these files? | Yes: 0 lone CR in both (Python byte census; fixture `a\r\nb\rc\n` printed `CR 2 CRLF 1`) | repo | C1 section 1 |

## Contracts

- [`contracts/C1-register-corrections.md`](contracts/C1-register-corrections.md): universals U1-U3, base digests and census, insertion forms, the ARC instrument (sha256 `42f21f81...`), shared wordings R184 and R190, per-row facts, orchestrator measurements and results.

## Open questions

None. Gate 2 decided both open items: `ScaleDirection` is recorded in the `CORE-035` block (C2-8), and the
`MASTER-171` bare grep at `00-master-index.md:398` goes to the backlog at Gate 3.
