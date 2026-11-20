# Coverage mapping -- session 6b-xiii

Written at Gate 2, **before** the approval that freezes `manifest.json`. The `Gate 2` column is
complete here; the `End state` column is filled at Gate 3.

The architect produced this mapping in its report; the orchestrator writes it down, and the
orchestrator is also the party holding the user's Gate 2 decisions.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 -- the 13 mechanical call sites pass an explicit scale | T1 (A1) | covered | **met** |
| D2 -- the 2 `AddPlacedObject` tests converted, surviving, renamed off `WithoutScale` | T1 (A2) | covered | **met** |
| D3 -- the 5 subject-removed `[Fact]` methods gone from `*.cs` sources | T1 (A3) | covered | **met** |
| D4 -- the 4 sole-cover survivors still exist with their assertions | T1 (A4) | covered | **met** |
| D5 -- the five scale-less members gone, doc comments included, one declaration each remaining | T2 (B1-B5) | covered | **met** |
| D6 -- `PlacedObject`'s private parameterless ctor and `Scale` field initializer unchanged | T2 (B6, B7) | covered | **met** -- the text is unchanged, which is what B6 requires. **Its RATIONALE was falsified** by T2's reviewer (`contradicts` C-1): removing the initializer is inert, not a scale-0 regression. The row is `met`; the reason the criterion gave for caring was wrong |
| D7 -- `Domain/AssemblyInfo.cs` unchanged | T2 (B8) | covered | **met** |
| D8 -- solution builds clean, coded-diagnostic grep returns 0 | T1 (A5), T2 (B9) | covered | **met** |
| D9 -- solution suite reports `Failed: 0`, totals reported not rationalised | T1 (A6), T2 (B10) | covered | **met** -- 975 (850 / 23 / 102), zero failures, zero skipped. The prediction agreed in every digit and the agreement was checked, not assumed |
| D10 -- `CORE-019`'s two stale citations replaced, corrected ranges qualified to `b39a65d` | T3 (C1-C4) | covered | **met** -- C1 and C3 on their stated substance, evidenced by a locations-scoped instrument returning 0. **Their attached file-scoped commands return 1** (`contradicts` CONTRA-1): C5 compels the strings to reappear verbatim, so the instrument cannot discriminate its own requirement |
| D11 -- `AMENDED 6b-xiii` block: DRIFT classification, verbatim previous wording, cause, all three ranges historical | T3 (C5-C7) | covered | **met** |
| D12 -- `MASTER-047` `Status` cell: `**done** (6b-xiii)`, disposition, stale-citation count, measured suite total, no commit hash | T3 (C8-C11) | covered | **met** -- cell verified hash-free (zero 7+-char hex tokens, zero `b39a65d`). **C8's authority citation is false** (`contradicts` CONTRA-2) and T3 relayed it into the cell; the convention lives at `00-master-index.md:623` under `MASTER-164`, not in `MASTER-098`/`MASTER-163` |
| D13 -- nothing outside the ownership sets modified; `01-domain-application.md:211` untouched | T3 (C13), T2 (B11) | covered | **met** -- `:211` byte-identical with its singular `Dungeon/` intact; B11 proved by all 87 deleted lines falling in exactly 5 hunks, one per dead member |
| D14 -- `KindGuardRunsBeforeTheScaleRejection` asserts `ActualValue == ObjectKind.None` as well as `ParamName`, added before D3 deletes its current carrier | T1 (A8) | covered -- added at Gate 2 by escalation E1, before the freeze | **met** -- and demonstrated live rather than merely present: T1's injection of `(ObjectKind)999` in place of `None` reddened it |
| **`MASTER-047`'s commit hash in the `Status` cell** | **none** | **uncovered -- accepted as orchestrator work at Gate 3, not a task's** | **written, post-merge, by the orchestrator** -- `a116b2c` (T1), `a9a5935` (T2), `18db889` (T3), merged as `65542c6`. Out of scope for every task by design; C8 required its absence from T3's output and that was verified. The same commit corrected C8's false attribution of the convention, per `contradicts` CONTRA-2 |

**One uncovered row, and it is uncovered by design rather than by omission.** The register's
convention -- visible in `MASTER-163` (`**done** (6b-iv) -- c519781`) and `MASTER-167`
(`**done** (6b-iii) -- T3, 925fb55`) -- is that a closure's commit evidence is written by the
orchestrator after the merge, because a DIR task cannot invoke git and a guessed hash is worse than an
absent one. D12 and C8 therefore *require* the hash's absence from the task's output. The row is
recorded as uncovered rather than quietly folded into D12 so that a Gate 3 reader can see the hash was
somebody's job and whose.

**Every other row is covered.** 14 of 15 items map to a task; no item maps to none by accident.

D14 entered the mapping **at Gate 2**, from escalation E1, and its `Gate 2` cell says so. It is
recorded as `covered` rather than `amended` because E1 resolved before the approval that froze
`manifest.json` -- nothing had been signed when A8 was written, so this is a draft edit and no
`amendments` entry exists for it. A row added after the freeze would have read `amended` instead.

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|

**Empty, and declared empty rather than left blank.** `manifest.json` records `"contracts": []`.

The only interface between the three tasks is the set of C# overload signatures T1 converts its call
sites onto, and those already exist in the repository at `b39a65d` -- they are not something to author
and freeze. There is no new type, no new boundary, and T1 -> T2 -> T3 is a strictly serial chain over
disjoint ownership sets, so there is no declared operation that could reach an implementer without a
consumer. The sharpest row this table exists to produce -- an operation with an implementer and no
caller -- cannot arise where nothing is declared.

Consequently there is **no contract-operation search at Gate 3**, and no `not called` verdict is
possible for this session. That is the same position 6b-xii recorded, and it is stated here so a Gate 3
reader does not read the empty table as a search that was skipped.

### End state, filled at Gate 3

**Every definition-of-done row is `met`.** 34 acceptance criteria across three tasks, all met on
first review, zero blocking findings, zero revision rounds. No row is `not met` and none needed the
`attributed` label -- every verdict is a join over evidence this session produced, and the
environment did not move underneath it.

**Two rows are `met` while carrying a falsified claim, and both are flagged in place rather than
smoothed.** D6's criterion required a text be unchanged and it is; the *reason* B6 gave for caring
was measured false. D10's criteria are met in substance while their own attached commands return a
number contradicting that verdict. Recording these as plain `met` without the qualification would
make this table the reassuring artifact it exists not to be.

**The one uncovered row stayed uncovered, as predicted at Gate 2.** No task acquired it and none
should have.

## A dispatch obligation this mapping depends on

**C11 has an input no task can produce.** T3 must record the suite total measured *after* the change,
and it cannot measure it within its ownership set -- it owns two documents. The figure comes from T2's
own B10 output and **the orchestrator must hand it to T3 at dispatch**. If that does not happen, C11 is
UNDETERMINED by construction, and T3 is instructed to report it as such rather than fill it from
`measurement.md` section 5.5, whose 975 is explicitly a prediction.

Recorded here, in the artifact, because it is a cross-task dependency that no `dependsOn` edge can
express: T3 depends on T2's *measurement*, not merely on T2's completion.
