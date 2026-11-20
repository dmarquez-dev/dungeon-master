# CORE audit corrections

Insert three dated, attributed corrections into `docs/audit/01-domain-application.md`, one per backlog row
(`CORE-015` Correction 4's missing `20b0b09` anchor, the `CORE-019` `AMENDED 6b-xiv` preamble's command
universal, and `CORE-035`'s two "twelve" figures), then remove those three rows from `.sir/backlog.md` at Gate 3.

## Context

All three rows were re-measured at the base `9f9ca69` (`chore/repo-audit`) by the orchestrator, with git,
before decomposition; the record is `measurement.md` (M1-M3). All three claims are **true**:

- `CORE-015` Correction 4 (M1): its `grep -c "if (outcome is not MutationOutcome.Success)"` prints 5 at
  `20b0b09` and 1 at `9f9ca69`. The one commit in between, `66a0b73`, is 6b-xiv's own helper extraction. The
  paragraph names no commit, while Corrections 1-3 name `20b0b09`.
- `CORE-019` preamble (M2): the universal says every historical figure was "handed over with the command that
  produced it". The exception sentence excepts the drift table and the closing-line range only from where they
  are recorded. The source criterion (6b-xiv manifest C15) handed the table over with a method and the range
  with nothing.
- `CORE-035` (M3): the `12 of 17` census and the `14 - 2 = 12` table census are both labelled. M3 adds one
  fact: they name the **same** twelve enums. The triage table less its class A rows equals the
  `AMENDED 20261105-1907` dispositions table. That is why that block's unqualified "the twelve" is correct.

The architect froze the exact text of each insertion (I1-I3) in `contracts/C1-core-audit-corrections.md`.
The contract also embeds ICC, the checker. The orchestrator reproduced ICC's must-match output on an
independently spliced fixture.

## Definition of done

- [ ] D1. `CORE-015` Correction 4: I1 is inserted after `:842`, inside the `AMENDED 6b-xiv` block. It restates
  the anchor (5 at `20b0b09`, 1 at `9f9ca69`, `66a0b73` between) with its measured and told figures labelled
  (C1 section 2.1).
- [ ] D2. `CORE-019` preamble: I2 is inserted after `:1418`. It says the drift table and the closing-line range
  were handed over with no command, the rows with a method and the range bare (C1 section 2.2).
- [ ] D3. `CORE-035`: I3 is inserted after `:3391`, as a new `AMENDED` block at the entry's end. It names the
  census twelve and the table twelve apart, with their units, and records that they name the same twelve
  enums (C1 section 2.3).
- [ ] D4. The audit file is CRLF throughout after the change: ICC prints `CR`, `LF` and `CRLF` equal at 3608,
  and `LONE_CR 0`.
- [ ] D5. No pre-existing line is deleted or changed: ICC prints `BASE_MATCH yes`, and each insertion's `AT`
  line ends `ok`.
- [ ] D6. Only `docs/audit/01-domain-application.md` changes across the session's task commits.
- [ ] D7. After the merge, ICC re-run on the **main-checkout** file prints `BASE_MATCH yes`, three `AT ... ok`
  lines and `CR 3608 LF 3608 CRLF 3608 LONE_CR 0`.
- [ ] D8. At Gate 3, the three rows are removed from `.sir/backlog.md`. They are the
  `2026-09-17-enum-zero-values` `CORE-035` parenthetical row and the two `2026-09-29-dungeon-facade-lookup-extraction`
  rows (the `CORE-019` qualifier and the `20b0b09` anchor). Each row's text counts 1 before and 0 after. The
  file stays CRLF with no lone CR. Nothing else changes except Gate 3's own appends.

## Scope

T1 makes three whole-line insertions into `docs/audit/01-domain-application.md`. The exact text and anchors are
in C1 section 2. The orchestrator does the backlog edit at Gate 3.

### Not in scope

- Every other row under "Corrections to the audit record", including the `01-domain-application.md:2245`
  bold-parity item (the `20261110-1857-tests-audit-corrections` BP row). BP still prints `odd 1` afterwards,
  with that block moved to `:2270`. Enforced by C1-5 (`BASE_MATCH yes`): no existing line changes.
- Any other audit file, and any source, test or build file. Enforced by D6.
- Re-numbering this file's existing in-document line citations. The insertions move every line below `:842`
  (by 15) and `:1418` (by 25), as earlier sessions' insertions did. The document already says a cited line is
  a historical position (`:1559-1562`). The insertions qualify their own citations "as numbered at
  `9f9ca69`" (C1 U3). Nothing here is deferred: this is the register's standing convention, not a ban.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Are the three rows still true? | Yes, all three, re-measured with git at `9f9ca69` | repo | `measurement.md` M1-M3 |
| Annotate or rewrite? | Insert only, with dated, attributed text. No existing line changes | repo | Gate 1 decision (`discussion.md`); C1 U1 |
| Where does I3 go? | It is a block after the `AMENDED 20261110-1618` closer at `:3391`, not a parenthetical in the body. That block says the body and the `1907` block "stay exactly as written" | repo | `01:3387-3388` |
| Where do I1 and I2 go? | Each is a parenthetical continuing its paragraph inside the 6b-xiv block (no blank line before it). No preservation claim covers either 6b-xiv block | repo | a whitespace-joined multi-line search of `docs/audit/*.md` for `6b-xiv` within 260 characters of `exactly as written`, `unchanged`, `verbatim` or `untouched`, run by the orchestrator: five hits, none a claim about those blocks |
| I1/I2 openers | `(AMENDED ...` closed by `)`, with no `[` or `]`. A `]` would close the enclosing 6b-xiv block early for BP | repo | C1 U4; section 5's BP spans |
| Who composes the text? | Nobody at implement time: the text is frozen exactly in C1 section 2 | judgment | lessons 93 and 97 (composed corrections carried defects) |
| How are history facts handled? | The orchestrator measured them at spec time. The text states them as told, with the ATTR literal. T1 runs without git | repo | `measurement.md`; C1 U3 |
| Agent and domain | `sir-architect` / `architecture`. No composed discipline applies to audit prose | repo | `20261110-1857-tests-audit-corrections/manifest.json`; the four composed disciplines name no audit or markdown rule |
| Can tasks build concurrently? | There is nothing to order: one task, and it does not build, test or run git | repo | manifest (`T1` only); C1 section 1 |
| Injected defects into tracked files? | None. Every must-fail control runs on a scratch copy | repo | C1-1, C1-5, C1-8 |
| Is `Edit` safe on this file? | Yes: CR 3552 = CRLF 3552 = LF 3552, no lone CR (Python byte census) | repo | `measurement.md` header; C1 section 1 |
| May audit prose cite `.sir/` paths? | Yes. The register is process documentation and already cites session paths | repo | `01:800-801`, `01:3218` |
| ICC instrument | Recreated from C1 section 3, sha256 `71f0e4e7...f874`. The orchestrator re-extracted it, got the same digest, and reproduced the base and must-match outputs | repo | C1 sections 3 and 5 |

## Contracts

- [`contracts/C1-core-audit-corrections.md`](contracts/C1-core-audit-corrections.md) contains universals
  U1-U5, the base digest and census, the exact insertions I1-I3 with their anchors, the ICC instrument, BP by
  path, and the validation record.

## Open questions

None outside Gate 2.
