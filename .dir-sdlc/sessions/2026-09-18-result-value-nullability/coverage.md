# Coverage mapping -- 2026-09-18-result-value-nullability

Written by the orchestrator at Gate 2, before the approval that freezes `manifest.json`. The `Gate 2`
column is complete; the `End state` column is written at Gate 3.

## Definition of done

Items are `spec.md`'s numbered definition-of-done list.

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. `Value` doc no longer claims `null` on failure | T1 (A1) | covered | met (A1) |
| 2. Doc states the failure value is `default(T)`, as `<see langword="default"/>` | T1 (A2) | covered | met (A2) |
| 3. Doc names the `Result<Guid>` consequence -- `Guid.Empty` | T1 (A3) | covered | met (A3) |
| 4. Doc names `IsSuccess`/`IsFailure` as the only failure signal | T1 (A4) | covered | met (A4) |
| 5. `MapCanvasViewModel.cs` has no `Value == Guid.Empty` comparison | T2 (B1) | covered | met (B1) |
| 6. The legitimate `addResult.Value` read into `UpdateAreaOutlineCommand` survives, and is the only one | T2 (B4) | covered | met (B4) |
| 7. No test name or summary in `ResultOfTTests.cs` claims a failed result carries no value | T3 (C1, C2) | covered | met (C1, C2) |
| 8. A `[Fact]` names the default behaviour and asserts `default(Guid)` literally, with a corrected summary | T3 (C3, C4) | covered | met (C3, C4) |
| 9. A new source-scanning fact pins the sentinel's absence, flags the must-match, spares three near-misses, and is vacuity-guarded | T3 (C5-C10) | covered | met (C5-C10) |
| 10. `MASTER-044`'s status cell marks the entry done | T4 (D1) | covered | met (D1) |
| 11. `CORE-005` carries an amendment recording the six falsified or superseded claims | T4 (D2-D8) | covered | met (D2-D8) |
| 12. No regression; `DungeonMaster.Tests` strictly above its 850 baseline | T1 (A7, A8), T2 (B6, B7), T3 (C12) | covered | met -- 855 / 23 / 102 = 980, exit 0 |

**Every definition-of-done item has a covering task. No uncovered rows.**

## Uncovered rows

None.

## Rows that are NOT definition-of-done items, recorded so they are not mistaken for coverage

**U1 -- `measurement.md` §3 fact 2 is deliberately not acted on.** `ResultOfT.cs:29` annotates `T?`
on an unconstrained `T`. No task changes it; `A5` pins it as preserved and `D8` records the decision
in the audit. It is stated as a live fact in the measurement, so it is listed here rather than
silently dropped. **Presented at Gate 2 as the session's one open question. THE USER'S DECISION, recorded in their terms: leave `T?` unconstrained and document it -- the annotation is correct and the prose was the defect.** `T1` stands as drafted; `A5` and `D8` carry the reasoning so it is not re-opened blind. The two alternatives were presented and declined: an accessor that cannot be read on a failed result (`TryGetValue`/`Match`, which repository evidence opposes at `n = 0`), and constraining `T : notnull` (which the architect reported as ineffective rather than merely unattractive, since `notnull` does not make `T?` mean `null` for `Guid`).

**U2 -- nothing in this session protects the register's own accuracy going forward.** T4 corrects six
claims in `CORE-005`. It does not, and no task does, prevent the next `CORE-00x` fix from falsifying a
neighbouring entry the same way `CORE-006` falsified this one. That is the mechanism that made this
session necessary, and it remains unaddressed. Carried to Gate 3 as a backlog candidate, not a task.

**U3 -- the amendment's contents are checked for presence, not for truth.** `D3`-`D8` each falsify on
a string appearing in the amendment block. A block containing the right strings in a wrong sentence
would pass. The strings were chosen to be specific enough that this is unlikely (`ResultOfT.cs`,
`private init`, `MapCanvasViewModel.cs:213`, `TryGetValue`, the figures `4` and `16`), but the limit
is real and is stated rather than hidden. The reviewer reads the prose; no command does.

## Contract operations

**None.** `manifest.json` records `"contracts": []`. This session declares no frozen interface: task
ownership sets are disjoint and nothing crosses a task boundary as an interface. There is therefore
no contract-operation table to fill and **no contract-operation search runs at Gate 3**. This is a
declared outcome, not a missing artifact.

## Gate 3 -- end state of the three non-item rows

**U1 -- resolved at Gate 2 and honoured.** The user decided `T?` stays unconstrained and documented.
`A5` was met as a preserve criterion (declaration byte-identical, `grep -c "where T"` -> 0) and `D8`
recorded the reasoning in the audit. **No task changed the annotation.**

**U2 -- still open, and the session produced evidence for it rather than closing it.** Nothing here
prevents the next `CORE-00x` fix from falsifying a neighbouring entry. This session is the proof of the
mechanism: `CORE-006`'s landed fix falsified four of `CORE-005`'s seven claims with nobody touching
`MASTER-044`. **Carried to Gate 3 as a backlog candidate.**

**U3 -- the limit was real and the reviewer is what caught it.** `D3`-`D8` are satisfied by string
presence, and a block with the right strings in a wrong sentence would have passed every command. The
T4 reviewer read the prose and found **two incidental line citations that do not resolve to what they
claim** (`01-domain-application.md:318` and `:366`) -- the exact defect class the amendment exists to
correct. No command in the session could have caught either. **Carried to Gate 3 as a decision.**

## Gate 3 -- register survey, re-run after the index edit

`179` entry rows: **130 closed, 46 open, 3 part done.** Zero `UNCLASSIFIED`. Open S3 falls **5 -> 4**;
`CORE` falls **16 -> 15**. `MASTER-044` no longer appears among the open rows. Command in the 6b-xii
handoff, section 6.
