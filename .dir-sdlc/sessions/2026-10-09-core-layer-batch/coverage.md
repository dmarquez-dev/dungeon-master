# Coverage mapping -- session 6b-xvii

Produced by `dir-architect` with the draft manifest; written here by the orchestrator, who holds the
Gate 2 decisions. The `Gate 2` column was complete before the freeze; `End state` was filled at Gate 3.

Items are numbered as `spec.md`'s **Definition of done** numbers them.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. `RotateDirection.cs` renamed to `RotationDirection.cs`, git-visible, referencing App files untouched | T1 | covered | **met** -- T1 criteria 1-4 all met; git recorded `R100`, an exact rename |
| 2. Doc-comment form and all 14 single-line tags in the three named files | T3 | covered | **met** -- T3 criteria 1-9 all met; both census predicates return 0 |
| 3. Five `AsReadOnly()` sites return a cached wrapper, no public property type changes | T4 | covered | **met** -- T4 criteria 1-13 all met, criteria 6 and 8 under amendment A2, which carved out `Area.Outline` |
| 4. `ct` spelled `cancellationToken` at all 51 Application + Infrastructure sites; the 28 Tests and App sites untouched | T5 | covered | **met** -- T5 criteria 1, 6 met; criterion 2 met under amendment A1 |
| 5. `obj` renamed at all nine occurrences, distinct from the lambda parameter at the two shadowing sites | T5 | covered | **met** -- T5 criteria 4 and 5 met |
| 6. XML documentation on the six Floors types, `ObjectKind` type-level only | T2 | covered | **met** -- T2 criteria 1-14 all met |
| 7. Solution builds `0 Warning(s)` / `0 Error(s)`; suite 0 failed, 0 skipped | T6 | covered -- T6 alone, per the architect's E7 | **met** -- T6 criteria 1 and 2 met, reviewer-reproduced; 897 + 23 + 108 = 1028 passed |
| 8. `CORE-028` records the falsification with its positive control; `MASTER-113` closes | T6 | covered | **met** -- T6 criteria 6 and 12 met |
| 9. `CORE-032` records `MASTER-145` closed deliberately unchanged | T6 | covered | **met** -- T6 criterion 10 met |
| 10. `CORE-033` records the corrected count; `MASTER-146` closes | T6 | covered | **met** -- T6 criteria 11 and 17 met, after blocking finding B1 was resolved in revision round 1 |
| 11. Every stale citation corrected in its audit entry, `AMENDED 6b-xvii` blocks on seven entries | T6 | covered -- **but see the anchor-staleness finding in the Gate 2 consistency report** | **met** -- T6 criteria 12-17 and 22 met. Six `AMENDED 6b-xvii` blocks plus the `CORE-025` block the implementer found unprompted. Blocking findings B1 and B2 were false universals inside two of these blocks and were resolved in revision round 1 |
| 12. `MASTER-171` closes as a duplicate naming `MASTER-111`; `MASTER-130` closes part done | T6 | covered | **met** -- T6 criteria 4 and 8 met; blocking finding B3 against the `MASTER-130` cell resolved in revision round 1 |
| 13. The E1, E2 and E3 backlog follow-ups are filed | **none** | **uncovered** -- no task may touch `.dir-sdlc/backlog.md`. Decision: the **orchestrator** appends all three at Gate 3 after approval, which is what `dir`'s Backlog section prescribes in any case. Not a decomposition gap to fill with a task. | **pending approval** -- the append is Gate 3's own action and has not run at render time. This row is the one End state that is not a join over `reviews/`, because no task's criteria cover it |

Row 13 was raised by `dir-architect` as escalation E6 rather than closed by it: the architect is
forbidden from retiring an uncovered row by declaring it out of scope, and it did not.

**No definition-of-done item ended `not met`.** Every row above is a join over the per-criterion verdicts
in `reviews/<task-id>.md`; no new evidence was gathered to fill this column, per the Coverage rule.

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|

**No rows, and this is a decision rather than an omission.** `manifest.json` declares `"contracts": []`.
Two independent reasons, both measured:

1. **T5 owns both sides of every interface it changes.** The eight interfaces (`IQueryHandler`,
   `ICommandHandler`, `IDispatcher`, `IPipelineBehavior`, `IUnitOfWork`, `IUnitOfWorkFactory`,
   `IDungeonReadRepository`, `IDungeonWriteRepository`) and every Infrastructure implementation sit in one
   task's ownership set, so the parameter spelling is not a cross-task boundary.
2. **Even split across tasks, nothing would break.** `grep -rnE '\bct:' src --include=*.cs`, with `bin`
   and `obj` excluded, returns **0** -- no call site anywhere passes the parameter by name -- and C# does
   not require an implementation's parameter names to match its interface's. The predicate was
   controlled: `grep -rnE '\bcancellationToken:' src --include=*.cs` over the same tree returns **1**, so
   the predicate can see a named argument of exactly this shape.

**The Gate 3 end-state search did not run, because there is no operation to run it against.** That is a
consequence of there being no new interface in this session, not a check that was skipped. No `not called`
row is possible, and none is claimed.

**No amendment touched a file under `contracts/`.** All three amendments (A1, A2, A3) are against
`manifest.json`, so no contract row needed re-deriving here.
