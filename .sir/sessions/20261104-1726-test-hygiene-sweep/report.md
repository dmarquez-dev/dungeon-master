sir-sdlc 1.0.20 — Gate 3 report — 20261104-1726-test-hygiene-sweep

**Decisions needed:** none — all three escalations (E1-E3) are resolved with no deferred action, and no blocking finding was raised.

## 2. Acceptance criteria

| Task | Domain | Criteria | Met |
|---|---|---|---|
| T1 | test | 12 | 12 |
| T2 | test | 14 | 14 |
| T3 | test | 12 | 12 |
| T4 | test | 12 | 12 |
| T5 | test | 24 | 24 |
| T6 | test | 14 | 14 |
| T7 | test | 10 | 10 |
| T8 | architecture | 28 | 28 |

No criterion is `not met`. Of the 126 criteria, 37 are listed below: 31 have a demonstration tag and 6 were changed by an amendment.

- T2 c2 (amended A1): the diff holds only markers, blank lines, Act-value extractions, the S5 facts, the E1 guard fact and the A1 removals. The reviewer read every `-` line.
- T2 c3 (amended A1): tests.pl goes from 152 to 155 rows, adding exactly the three named facts.
- T2 c4 (amended A1): PA prints 0 lines over AppCore.
- T2 c5 [red step]: `DrawState_OnANewState_IsIdle` failed with the expected value set to Drawing and passed with Idle.
- T2 c9 [injected defect]: J1 (`> 0` changed to `> 1`) failed ActivatesSession only.
- T2 c10 [injected defect]: J2 (`_vertices.Add` commented out) failed HoldsOneVertex and ActivatesSession, and ReturnsAdded passed.
- T2 c12 [reviewer-reproduced]: the build reports 0 warnings and 0 errors.
- T2 c13 [reviewer-reproduced]: tests pass at 946 / 187 / 123, with 0 failed.
- T3 c7 (amended A3/A4): each renamed method claims only what it asserts. Its body changes are markers plus one extraction, which the amendment allows.
- T3 c10 [reviewer-reproduced]: the build reports 0 warnings and 0 errors.
- T3 c11 [reviewer-reproduced]: tests pass at 946 / 187 / 123, so the delta is 0.
- T4 c4 [reviewer-reproduced]: P1 and P2 print nothing on DungeonReadRepositoryTests.cs.
- T4 c6 [reviewer-reproduced]: the awk count is 0. It was 1 at startRef.
- T4 c7 (amended A3): `ScopedFiles_EveryAwait_IsConfiguredFalse`. Its body change is three marker lines.
- T4 c10 [reviewer-reproduced]: the build reports 0 warnings and 0 errors.
- T4 c11 [reviewer-reproduced]: tests pass at 946 / 187 / 123, so the delta is 0.
- T5 c2 [injected defect J3]: the S1 guard asserts only that the scan is non-empty, with its `because` text verbatim.
- T5 c4 [injected defect J4]: the S2 guard asserts only that `IconTexts` is non-empty, with its `because` text verbatim.
- T5 c8 [injected defect J9]: the reason fact holds only its `OnlyContain` assertion.
- T5 c9 [injected defect J10]: the colour guard asserts only that `ColourTokens()` is non-empty.
- T5 c12 [injected defect]: J3 failed the S1 guard and passed the offender fact.
- T5 c13 [injected defect]: J4 failed the S2 guard and passed the offender fact. The revert was confirmed by `cmp` and sha256.
- T5 c14 [injected defect, reviewer-reproduced]: J5 failed S3(iii) only.
- T5 c15 [injected defect]: J6 failed S3(iv) only.
- T5 c16 [injected defect]: J7 failed S3(ii) and passed S3(i).
- T5 c17 [injected defect]: J8 failed S3(i).
- T5 c18 [injected defect]: J9 failed the reason fact and passed the register fact.
- T5 c19 [injected defect]: J10 failed the colour guard and passed the reason fact.
- T5 c22 [reviewer-reproduced]: the build reports 0 warnings and 0 errors.
- T5 c23 [reviewer-reproduced]: DungeonMaster.Tests went from 946 to 953 (+7), with 0 failed.
- T6 c11 [reviewer-reproduced]: the build reports 0 warnings and 0 errors.
- T6 c12 [reviewer-reproduced]: DungeonMaster.Tests passes 953, so the delta is 0.
- T6 c13 [reviewer-reproduced]: App.UnitTests passes 187 and App.Tests 123, with 0 failed.
- T8 c7 [reviewer-reproduced]: the build reports 0 warnings and 0 errors.
- T8 c8 [reviewer-reproduced]: DungeonMaster.Tests passes 953 with 0 failed. The guard facts number T2=1, T3=0 and T4=0, matching the commit bodies.
- T8 c9 [reviewer-reproduced]: App.UnitTests passes 187 and App.Tests 123, with 0 failed.
- T8 c14 (amended A7, E3): the MASTER-143 cell carries the scoped claim "No doc comment was added or removed to close this row". It discloses the 7 summaries added this session (660 → 667).

Every row is in `reviews/T1.md` through `reviews/T8.md`.

**Coverage, as mapped at Gate 2, amended where an amendment changed a contract, and checked at Gate 3:** all 9 definition-of-done rows are `met`, and there are no exceptions. The two rows that name 952 were met at 953 under A2, which adds the E1 guard fact. No contract operations were declared, because C1-C5 are documents.

**Limits:** none. Every verification command ran in the worktree.

## 3. What ran

| Task | Agent | Domain | Model | Reviews | Rounds | Redispatches | Commit |
|---|---|---|---|---|---|---|---|
| T1 | sir-test-engineer | test | opus | 1 | 0 | 0 | 38d7a5a |
| T2 | sir-test-engineer | test | opus | 1 | 0 | 0 | c7e2878 |
| T3 | sir-test-engineer | test | opus | 1 | 0 | 0 | b254b0b |
| T4 | sir-test-engineer | test | opus | 1 | 0 | 0 | 5c39ec6 |
| T5 | sir-test-engineer | test | opus | 1 | 0 | 0 | 28359e5 |
| T6 | sir-test-engineer | test | opus | 1 | 0 | 0 | 2577caa |
| T7 | sir-test-engineer | test | opus | 1 | 0 | 0 | 4dc3488 |
| T8 | sir-architect | architecture | opus | 1 | 0 | 0 | a0bb22a |

Other dispatches:

- `init-session` (sir-git, haiku)
- `decomposition` (sir-architect, opus)
- 8 reviews (sir-reviewer, opus)
- 8 `commit-T<n>` (sir-git, haiku)

T2 and T3 each have a second `DISPATCH` line after their amendment. The review files record each one as a continuation of the same task, not a crash re-dispatch.

**Change surface:** the range is `9c931b4..a0bb22a`, from `git merge-base chore/repo-audit agent/20261104-1726-test-hygiene-sweep` to the branch tip. It holds 8 commits, and `git diff --stat` reports 53 files changed, 1119 insertions and 165 deletions. `git diff 9c931b4..a0bb22a` shows the full diff.

**Amendments (7):**

- A1 (E1) `manifest.json`: changed from "no assertion removed or reordered; T2 +2; T8 952" to "a precondition left under `// Arrange` may be removed, covered by a named fact or moved to a guard fact; T2 +3; T8 952 + guard facts".
- A2 (E1) `contracts/C1-measurement.md` §8: changed from "943 + 2 + 7 = 952" to "952 plus the E1 guard facts (T2 adds 1)".
- A3 (E2) `manifest.json`: in T3 c7 and T4 c7, "renamed body unchanged" now reads "unchanged apart from markers, blank lines and Act-value extraction".
- A4 (E2) `contracts/C2-naming.md` §6: the same allowance applies to renamed Domain and Infrastructure methods that aaa.pl flags.
- A5 (E3) `contracts/C4-register.md` §1: the MASTER-143 line "no doc comment was added or removed" now ends "…to close this row", and the session-added summaries are disclosed.
- A6 (E3) `contracts/C1-measurement.md` §5: "no task adds or removes a doc comment" now reads "…to close MASTER-143; T5 added 7 summaries".
- A7 (E3) `manifest.json`: T8 criterion 14 now uses the scoped "to close this row" claim plus the disclosure.

## 4. Advisory findings

**Contract gaps (2)**

- `src/DungeonMaster.Tests/Infrastructure/AreaPersistenceTests.cs:38` and `ConfigureAwaitDisciplineTests.cs:51-53`: these two assertions sit under `// Act`. PA and A1 check only the span from Arrange to Act, so no predicate catches an assertion inside Act. (T4, T8)
- `src/DungeonMaster.App.UnitTests/Theme/DmThemeResolutionTests.cs:93`: this line still holds an elided fragment of an old name, `…_IsDeclaredAsTheTypeItIsResolvedAs`. REF matches full names only, so it misses the fragment. (T6, T8)

**Correctness risks (2)**

- `SemanticDescriptionTests.cs:124-125`, `IconFontCoverageTests.cs:75-76` and `TokenNamingTests.cs:345-346`: these carried `because` strings say "this fact", which now refers to the sibling offender fact rather than the guard that prints the message. The criteria required the text verbatim. (T5)
- `docs/audit/05-tests.md`, TEST-023 and TEST-030 blocks: both are classified UNDETERMINED, but git at `2603170` settles them as DRIFT. (T8)

**Session conduct (3)**

- Amendment A1 cited first-round worktree line numbers as startRef lines in T2 criteria 3 and 4. Every fact was also named, so nothing was ambiguous. (T2)
- The orchestrator's condensed T5 review brief paraphrased the injected text for J5-J8. The implementer's report gave the literal lines. (T5)
- C1 §5:311-312 stated that no task adds or removes a doc comment. T5 falsified this frozen universal, which led to E3 and A6. (T8)

**Test and CI concerns (1):** see `reviews/T2.md`, `T6.md` and `T8.md`.

**Cosmetics (9):** see `reviews/T1.md`, `T2.md`, `T4.md`, `T7.md` and `T8.md`.

## 5. Metrics

- `userInteractions`: 6. This includes this gate. The push question is asked after the merge and is not yet counted.
- `escalationsRaised`: 3
- `reviewRounds`: 0. This equals the sum of the Rounds column.
