sir-sdlc 1.0.22 — Gate 3 report — 20261105-1907-core-heavy-rows

**Decisions needed:** none. The one escalation, E1, was resolved by amendment A1 and deferred nothing.

## 2. Acceptance criteria

| Task | Domain | Criteria | Met |
|---|---|---|---|
| T1 | database | 32 | 32 |
| T2 | architecture | 19 | 19 |
| T3 | architecture | 31 | 31 |
| T4 | architecture | 18 | 18 |

No criterion is `not met`, and the amendment did not change any criterion's text. The 37 criteria below carry a demonstration tag, and each is listed in full:

- **T1.1** (reviewer-reproduced): `cat -A` shows `Clockwise90/180/270` at :16/:21/:26, with no `= n`. At startRef these lines held CW*.
- **T1.3** (reviewer-reproduced): P-CW `| wc -l` prints 0. At startRef, `git grep` gives 28.
- **T1.6** (reviewer-reproduced): the migration count prints 4. At startRef it gives 3.
- **T1.7** (reviewer-reproduced): on the new migration, `migrationBuilder.` counts 2 and `migrationBuilder.Sql(` counts 2. The two controls give 4/2 and 2/0.
- **T1.10** (reviewer-reproduced): `has-pending-model-changes` prints "No changes have been made to the model since the last migration". The snapshot's sha256 equals startRef's.
- **T1.21** (red step): with Up and Down emptied, the narrow run gives `Failed: 3, Passed: 2`. O1 shows `"CW90"` where it expects `"Clockwise90"`.
- **T1.22** (injected defect J1): removing the `CW180` arm in Up makes O1 fail and leaves O3 passing (`Failed: 2, Passed: 3`). The revert digest is equal.
- **T1.23** (injected defect J2): removing the `Clockwise270` arm in Down makes O3 fail and the other four pass (`Failed: 1, Passed: 4`). The revert digest is equal.
- **T1.24** (reviewer-reproduced): build prints "0 Warning(s)" / "0 Error(s)", including with `--no-incremental`.
- **T1.25** (reviewer-reproduced): the suites give 187/187, 958/958 and 123/123. 953 + 5 = 958.
- **T1.26** (reviewer-reproduced): B prints CR = LF = CRLF and loneCR=0 for all twelve edited files.
- **T2.1** (reviewer-reproduced): a full-prefix `grep -cF` on line 345 prints 1. The cell was empty at startRef and now leads with `DELIBERATELY UNCHANGED`.
- **T2.3** (reviewer-reproduced): `wc -l Common/Dispatch/*.cs` gives 8 files, `437 total`, and Dispatcher.cs at 209.
- **T2.4** (reviewer-reproduced): DYN prints nothing. `MakeGenericType` occurs only inside the `static` value factories (Dispatcher.cs:28-75).
- **T2.5** (reviewer-reproduced): `AddScoped<` matches only at :27, and the App grep for ValidateHandlerRegistrations prints nothing.
- **T2.6** (reviewer-reproduced): `IQuery<TResult> : IRequest<TResult>`, and BuildPipeline is at :199. ValidationBehavior is registered for commands only.
- **T2.8** (reviewer-reproduced): the D grep prints :92/:131/:164/:197. The cell says the residue is kept by design.
- **T2.10** (reviewer-reproduced): R's control prints 3, and R prints 11 on line 345.
- **T2.11** (reviewer-reproduced): the U digest is `21286673…a88a`, `wc -l` is 691, and the file is all CRLF.
- **T2.13** (reviewer-reproduced): AO and AC print 16 each. At startRef they print 15.
- **T2.18** (reviewer-reproduced): numstat is `64 0`, with no removed lines.
- **T2.19** (reviewer-reproduced): B gives `CR=3061 LF=3061 CRLF=3061 loneCR=0`, which is 2997 + 64.
- **T3.2** (reviewer-reproduced): the CR-stripped fixture census sha256 is `db60df1b…4758`.
- **T3.3** (reviewer-reproduced): the worktree census gives `TOTAL 17 NONE_AT_ZERO 5 CONCRETE_AT_ZERO 12 NO_ZERO 0`.
- **T3.4** (reviewer-reproduced): the second predicate prints 17. The FORM 3 control prints only the AsyncVoidUnitKind line.
- **T3.9** (reviewer-reproduced): V's controls pass and fire as expected. Row 366 prints `3578 7e26bc18…6fe8`.
- **T3.11** (reviewer-reproduced): the U digest, the 691 line count and all-CRLF hold, each checked after its control.
- **T3.13** (reviewer-reproduced): AO and AC print 17 each. At 0fd802e they print 16.
- **T3.31** (reviewer-reproduced): B gives `CR=3159 … loneCR=0`, which is 3061 + 98.
- **T4.2** (reviewer-reproduced): Clockwise90/180/270 are at :16/:21/:26, the migration and its Designer are listed, and P-CW `| wc -l` prints 0.
- **T4.5** (reviewer-reproduced): the PathSegmentKind quotes are verbatim. `Line` and `CubicBezier` are confirmed, and JC prints nothing after its control.
- **T4.7** (reviewer-reproduced): a grep for this session's own commit hashes in the new prefix prints 0.
- **T4.8** (reviewer-reproduced): row 318 prints `11846 cdf47942…e120`, and V's controls pass and fire.
- **T4.9** (reviewer-reproduced): R's control prints 3, and R prints 11 on line 318.
- **T4.10** (reviewer-reproduced): the U digest, the 691 line count and all-CRLF hold, each checked after its control.
- **T4.12** (reviewer-reproduced): AO and AC print 18 each. At 6b3bd62 they print 17.
- **T4.18** (reviewer-reproduced): B gives `CR=3214 … loneCR=0`, which is 3159 + 55.

Every row lives in `.sir/sessions/20261105-1907-core-heavy-rows/reviews/T1.md` … `T4.md`.

**Coverage:** this is coverage as mapped at Gate 2, amended where an amendment changed a contract, and checked at Gate 3. All 9 definition-of-done rows are `covered` and `met`. No contract declares a callable operation, so the operations table has no rows.

**Limits:** none. Every verification command ran in the worktree.

## 3. What ran

| Task | Agent | Domain | Model | Reviews | Rounds | Redispatches | Commit |
|---|---|---|---|---|---|---|---|
| T1 | sir-database-engineer | database | opus | 1 | 0 | 0 | 2a04271 |
| T2 | sir-architect | architecture | opus | 1 | 0 | 0 | 0fd802e |
| T3 | sir-architect | architecture | opus | 1 | 0 | 0 | 6b3bd62 |
| T4 | sir-architect | architecture | opus | 1 | 0 | 0 | 9f2d990 |

Each task also had one `sir-reviewer` dispatch on opus. The `sir-git` dispatches (`init-session` and four `commit-T*`) ran on haiku.

- **Range:** `62ba566..9f2d990` (`git merge-base chore/repo-audit agent/20261105-1907-core-heavy-rows` gives `62ba566`). It holds 4 commits.
- **Stat:** 18 files changed, 799 insertions(+), 33 deletions(-).
- **Full diff:** `git diff 62ba566..9f2d990`.

**Amendments (1):**

- **A1** changed `contracts/C4-register.md` §0, authorised by E1.
  - Before: "No commit hash appears in any cell or block".
  - After: "No hash of this session's own commit appears in any cell or block". A historical hash copied from an orchestrator-supplied figure may appear, labelled told.

## 4. Advisory findings

**Contract gaps (4)**

- `contracts/C3-enum-census.md:182` gives the PathSegment throw at `PathSegment.cs:83-85`, but the statement spans :82-85 inside the else arm at :80-86. No disposition depends on it. (T3)
- `docs/audit/01-domain-application.md:2802` and `:2884` (CORE-035's body) cite `ObjectKindInfo.cs:24-28` for GetDefaultAnchor, which is now at :40. C3 §4's drift list omits this, so the new block's drift paragraph does too. (T3)
- `docs/audit/01-domain-application.md:3012` cites "`grep -rn RotationDirection` over production `src/`" without the filters its figure needs. Unfiltered, it also matches bin/obj binaries. Reproduced with `--include=*.cs --exclude-dir=obj --exclude-dir=bin | grep -v 'Tests/'`. (T3)
- The audit uses opposite wording for a zero member:
  - `docs/audit/00-master-index.md:318` and `docs/audit/01-domain-application.md:1805` call adding one "a renumbering, not a rename".
  - Row 366 and `01-domain-application.md:2909` call it "a data migration, not a renumbering".
  - The substance is consistent; the wording is C4 §3.1 item 4's, verbatim. (T4)

**Test and CI concerns:** 2. See `reviews/T1.md` and `reviews/T2.md`.

**Cosmetics:** 1. See `reviews/T4.md`.

## 5. Metrics

- `userInteractions`: 4. This counts every stop so far, including this gate. The push question has not been asked yet, so it is not in the count.
- `escalationsRaised`: 1
- `reviewRounds`: 0 (the sum of the Rounds column: 0)
