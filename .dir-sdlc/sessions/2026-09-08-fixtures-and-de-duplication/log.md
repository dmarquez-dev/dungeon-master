# Session log -- 2026-09-08-fixtures-and-de-duplication

2026-09-08 -- Gate 1 presented and approved. Track: Standard. Scope: ten audit entries
  (MASTER-014, 041, 042, 064, 065, 066, 067, 069, 085, 127), all in `src/DungeonMaster.Tests`.
  User chose spike-before-decomposition over decomposing against the audit's prose.
2026-09-08 -- Session setup: disciplines composed (backend, database, frontend, test; bundled bases
  only, no project fragments, all under the 500-line cap). Worktree `C:\dm-wt\6b-ii`, branch
  `dir/2026-09-08-fixtures-and-de-duplication` from `chore/repo-audit` at `850a10b`.
2026-09-08 -- Spike dispatched, two agents in parallel, disjoint write sets. Spike A (dir-test-engineer,
  opus): MASTER-066, 069, 041 -- injection-based, may edit Commands/Queries test files and must restore.
  Spike B (dir-test-engineer, opus): MASTER-014, 065, 085, 064, 067, 042, 127 -- read-only measurement,
  forbidden from modifying the worktree, and tasked with naming the collisions between entries.
2026-09-08 -- Spike A returned. Central finding: the audit document was committed 2026-08-17
  (`git log --diff-filter=A -- docs/audit/05-tests.md` -> `2603170`), and commit `bcb49dc`
  (2026-08-18, "fix(handlers): extract mutation skeleton and map outcomes to results") already
  remediated MASTER-066/TEST-010 and MASTER-041/TEST-011. Verified independently by the orchestrator:
  `grep -rn UpdateAsync --include=*.cs src | grep -v /bin/ | wc -l` -> 0 (the API the entry cites does
  not exist); `grep -rn "Task\.CompletedTask" src/DungeonMaster.Tests | wc -l` -> 0.
  `git merge-base --is-ancestor bcb49dc HEAD` -> yes. 28 commits have touched src/DungeonMaster.Tests
  since `bcb49dc^`; `bcb49dc` alone rewrote 12 handler test files.
  MASTER-069/TEST-018 is live, and its blind spot is LARGER than the entry states: measured, the two
  query tests are blind both to identity (M1) and to every DTO field the assertions do not name (M3).
  MASTER-041 settled as fact, not belief: NSubstitute 5.3.0 auto-substitutes Task-returning members
  with a completed task; probe passed with two controls, one of which (a deliberately hanging Task)
  fired, proving the green was a real negative.
  Orchestrator error, recorded: the Spike A brief said Application source was read-only and then asked
  for injections into handlers. The agent resolved in favour of the questions and reported the
  contradiction rather than complying silently. That was the right call and the brief was mine.
2026-09-08 -- Spike B returned. Audit found systematically stale and under-counted. Orchestrator
  re-verified independently, each with its own command: SnapZoneCalculatorTests absent at HEAD
  (`git ls-tree -r --name-only HEAD | grep -i snapzone` -> nothing); 3 migrations, not the 2 TEST-002
  cites; 7 non-sealed IDisposable test classes, not TEST-026's 4; 6 `NoBareX` geometry facts including
  `NoBareMinimumSizeValues` which TEST-013 misses; `DeleteDungeonCommandHandlerTests:65` HAS the
  not-found test TEST-008 says it lacks. Also proven by spike: `Database.Migrate()` works against
  SQLite in-memory, migrations have no pending model changes, and the EnsureCreated and Migrate
  schemas are set-identical -- so MASTER-014 can land without turning the suite red.
  TEST-012's second claimed subsumption is FALSE, demonstrated by injection with a passing control:
  `UpdateArea_SecondRenamePersistsAcrossReload` stays green under a defect that
  `UpdateArea_RenamePersistsAcrossReload` catches. Following the entry literally destroys coverage.
  Net: 8 of 10 entries live, 2 already remediated by `bcb49dc`, and only MASTER-064 is independent.
  Gate 1's premise has changed materially; taking the re-scope to the user before decomposition.
2026-09-08 -- Re-scope gate presented and answered (userInteractions 2). Decisions:
  (1) MASTER-065 takes the abstract base class route, not IClassFixture -- preserves per-test database
      isolation and keeps MASTER-127 real work rather than a no-op closure. A NEW base type; promoting
      one of the existing 7 is prohibited, since sealing it would contradict MASTER-127.
  (2) MASTER-066 and MASTER-041 marked done against `bcb49dc` with evidence, and the session
      additionally acts on two gaps the spike exposed that no entry covers: the commit-boundary
      assertion (flagged as reasoning, not measurement -- the owning task must demonstrate it), and the
      absent `InternalsVisibleTo("DynamicProxyGenAssembly2")`.
2026-09-08 -- spike-findings.md written to the session directory (321 lines, CRLF verified) as the
  governing artifact, so the architect reads measurements rather than the orchestrator's summary.
2026-09-08 -- dir-architect dispatched (opus) for decomposition. phase -> spec.
2026-09-08 -- Gate 2 decisions taken (this gate is ONE userInteraction however many prompts it fired):
  (1) Coverage row 14 becomes a task. T7 added, owning `docs/audit/05-tests.md`, correcting the seven
      measured mis-citations in the entry bodies so 6b-iii does not re-derive them. Chosen over
      accepting it out of scope, which repo evidence supported (no prior session has edited an audit
      record body) but which would have cost the next session the same measurements.
  (2) MASTER-085's query equivalence class: threshold of 3 holds, 2 members not extracted. The
      architect's judgment confirmed; contract unamended, so no escalation.
  Orchestrator errors corrected at this gate: spike-findings.md said TestDbContextFactory had 10 use
  sites across 6 files; it is 7. Caught by dir-architect, re-verified, artifact corrected.
  spec.md did not exist -- the architect flagged it rather than authoring its own acceptance standard,
  which was the right call. Written now, with a 19-row Assumptions table, every row citing its command.
  Ownership disjointness verified by expanding every `owns` glob against the real file tree, not by
  comparing glob strings: T1 8 files, T2 18, T3 11, T4 16, T5 1, T6 1, DISJOINT, and all seven
  EnsureCreated files owned solely by T1.
2026-09-08 -- Gate 2 approved. manifest.json and contracts/ FROZEN. 7 tasks, 90 acceptance criteria,
  ownership DISJOINT across all 21 pairs (verified by the orchestrator by expanding globs against the
  real file tree, not by comparing glob strings). Two gaps in the orchestrator's own spec.md were
  caught by dir-architect and closed: DoD item 5 needed all FOURTEEN command classes to share a base,
  not just the 12-class not-found half, because UnitOfWorkSubstitute consolidates the wiring and not
  the constructor; DoD item 6's "other ten validator classes untouched" was asserted nowhere.
  DoD 13a assigned to the orchestrator at Gate 3: CRLF verified in the MAIN CHECKOUT after merge, since
  no task can prove it from the worktree. dir-architect raised it and correctly refused to retire it.
  startRef for every dispatched task: 850a10b6fa0c820a5e6aa73fc0b5859fd0dc3892.
  T1-T5 and T7 dispatched concurrently (all dependsOn []); T6 waits on T1-T5. phase -> implement.
2026-09-08 -- T3 reported done; reviewer dispatched (opus). T3 -> in_review.
  T3 overturned this session's own governing artifact and appears to be right: spike-findings.md framed
  MASTER-069 as identity being "the smaller half", with unasserted-field corruption the larger. T3
  measured that `BeSameAs` alone ALSO catches unasserted-field corruption, because the DTOs are
  positional records with init-only members -- any field change forces a new instance and so breaks
  reference identity. The halves separate only for the LIST, which is why per-element identity rather
  than list identity was the right criterion. Flagged to the reviewer to settle independently.
  T3 also reports its full-suite figure as CONTAMINATED and declines to attribute it: 39 paths dirty,
  none of them its own, including T1's live `InjectedProbeT1` migration and T2's in-flight edit to
  DungeonMutationExtensions.cs. It reported and attributed rather than retrying it away, which is the
  standing rule working. Whether a criterion demanding a clean full-suite figure can be met by a task
  that does not control the worktree is referred to the reviewer as a possible criterion defect.
2026-09-08 -- T5 reported done; reviewer dispatched (opus). T5 -> in_review.
  T5's dropped-row injection is the cleanest demonstration of the session: deleting the "Stroke" row
  produced EXACTLY ONE failure -- the new cardinality guard -- while the suite Total silently fell from
  10 to 9 and the other 8 tests stayed green. That is the silent-ratchet-retirement the guard exists to
  catch, measured rather than argued. Control: row restored -> Failed: 0, Passed: 10, Total: 10.
  The guard's expected-name array is deliberately NOT derived from the ratchet list -- deriving it would
  make the guard agree with whatever the list says, i.e. a guard that cannot fail. Reviewer told to
  verify that independence and to drop a DIFFERENT row.
2026-09-08 -- SECOND task independently hits the same wall: a criterion demanding a clean full-suite
  figure cannot be satisfied by a task that does not control the worktree. T3 and T5 both declined to
  attribute a contaminated number and both attributed the dirt to named peers instead of retrying it
  away. This now looks like a criterion defect affecting all six parallel tasks rather than two
  implementation failures. Both reviewers have been asked to rule on it as a possible `contradicts`
  finding. ORCHESTRATOR ACTION: re-measure the full suite serially, in a clean worktree, after all
  tasks land and before Gate 3 -- and reconcile against each task's established per-class delta
  (T5: +1; T3: 0).
2026-09-08 -- T4 reported done; reviewer dispatched (opus). T4 -> in_review.
  T4's per-leaf discrimination is the right shape and was RUN, not reasoned: deleting `.NotEmpty()`
  from AddAreaCommandValidator failed 3 of 5 cases on the AddArea leaf ONLY, while the other three
  leaves stayed 15/15 green with the injection still live. That is the property proving the base binds
  each leaf to its OWN validator rather than running one validator four times -- the exact way a
  generic base silently stops covering a leaf. Control: restored -> 20/20.
  T4 found a defect in its OWN frozen criterion and reported it rather than working around it:
  criterion 2 says the four files differ on 6 lines; measured, they differ on 8. The criterion omits
  the two `nameof(...)` assertion lines -- which is precisely why NamePropertyName had to be an
  abstract member rather than a base-side literal. Referred to the reviewer to rule on as a possible
  `contradicts` finding. A supporting claim is also false: two other validator test files are both 82
  lines, so "every other file at a different length" does not hold; the load-bearing part (55 unique
  to the four in scope) does.
2026-09-08 -- THIRD independent task hits the full-suite contamination wall (T3, T5, now T4). T4
  measured 698 pre-task and 699 post-task, both contaminated by 32 peer paths, and declined to offer
  either as its own figure. Its delta is 0 on both filtered measures. The criterion defect is now
  established across three of six parallel tasks.
  Positive cross-task signal: T4 reports CommandValidatorCoverageTests passing 6/6 before AND after,
  which is the check that peer T2's UnitOfWorkSubstitute surface held -- the coverage.md concern about
  an unowned consumer, measured green from the other side.
2026-09-08 -- T2 and T1 reported done; reviewers dispatched (opus). Both -> in_review.
  T2 found TWO frozen criteria that contradict each other and reported rather than working around:
  (A) criterion 3 mandates a two-level hierarchy AND requires `grep -ln ": CommandHandlerTestBase"`
      to return 14 -- impossible, because C# permits one base class, so the twelve not-found leaves
      name the DERIVED base and never the root. Measured: 2. A flat 14-way hierarchy would satisfy the
      grep but would put the not-found fact on CreateDungeon, which has NO not-found path. The grep is
      wrong, not the shape.
  (B) criterion 7's literal assertion spelling depends on the `_unitOfWork` field that criterion 3
      requires be deleted.
  T2's commit-boundary demonstration settles the addition that was scoped in on reasoning alone:
  under injection, 24 failures, ALL naming CommitAsync and NONE naming Update -- so the pre-task suite
  would have caught this defect ZERO times. The addition is justified by measurement now, not argument.
  T2's InternalsVisibleTo red step produced the real ArgumentException naming DynamicProxyGenAssembly2.
  T1 captured THE ONLY CLEAN BASELINE of the session: Passed: 695, Total: 695 with
  `git status --porcelain` EMPTY at that moment, re-measured rather than inherited.
  T1's MASTER-042 demonstration is the one that stopped this session destroying coverage: under an
  identical injected defect, the retained UpdateArea_RenamePersistsAcrossReload FAILED while
  UpdateArea_SecondRenamePersistsAcrossReload PASSED -- the passing control proving non-subsumption.
  Exactly one test deleted, not the two the audit prescribed.
  T1 flagged a criterion no implementer can satisfy: AC10 requires `git ls-files` to stop printing the
  old path, but that reads the INDEX and implementers are forbidden from writing git. It closes only
  when dir-git stages at commit. Referred to the reviewer as a possible criterion defect.
  T1 also confirmed independently that spike-findings.md's "6 files" for TestDbContextFactory is wrong
  -- it is 7. That is the second independent confirmation of the orchestrator's error.
  FIFTH task to hit the clean-full-suite-status criterion defect. All five closed their arithmetic on
  filtered, per-class measures instead. Deltas: T1 +2, T2 +1, T3 0, T4 0, T5 +1.
2026-09-08 -- CORRECTION, orchestrator error, third of the session and the worst.
  `spike-findings.md` NEVER CONTAINED A MASTER-069 SECTION. Verified: `grep -c "MASTER-069"
  spike-findings.md` -> 0. Spike A measured the entry in full and the orchestrator wrote the result
  into this log (line 21) and into the Gate-1 report to the user, but never into the governing
  artifact. Two frozen criteria (manifest.json:83 and :84) then cited that non-existent section, and
  the log entry above at lines 78-84 told the user T3 had "overturned this session's own governing
  artifact". THAT IS FALSE -- the artifact held no position to overturn. `grep -rn "smaller half"`
  over the session directory returns only manifest.json:83 and log.md:79, both attributing the phrase
  to a document that does not say it. Found by T3's REVIEWER, not by the orchestrator, not by reading.
  spike-findings.md now carries an [OMITTED ... AMENDED] block recording the omission. It is
  deliberately NOT backfilled with the measurement: writing a dated measurement record retroactively
  would make the citations resolve while destroying the evidence that they ever failed to.
  What T3 actually did, per its reviewer's independent reproduction: confirmed log.md:21 by run
  demonstration and established the mechanism nobody had measured -- both DTOs are positional records
  with init-only members, so reference identity implies field equality, and the halves separate only
  for the LIST. A real result, and not a reversal. The Gate 3 report must say so.
2026-09-08 -- T7 review returned the session's FIRST BLOCKING FINDING. T7 -> revising, revisionRounds 1,
  metrics.reviewRounds 1. B1: docs/audit/05-tests.md:756 still asserts "the two subsumed pairs support
  that reading" twelve lines below T7's own amendment headed "Only one of the two claimed subsumptions
  holds." Live prose, not quoted previous wording, so a reader has no signal it is superseded.
  Verified by the orchestrator at :750-760. Swept the class rather than the instance
  (`grep -n "subsumed\|subsumes"` -> :688 :720 :722 :729 :756 :763); :756 is the ONLY live
  contradiction. T7 re-dispatched with the finding and told to re-run the sweep itself rather than
  adopt the orchestrator's result.
  T7's headline reversal CONFIRMED by its reviewer, which re-ran 15 of T7's recorded commands and
  reproduced every figure including both md5s: six of seven corrections are DRIFT, not under-counting.
  The audit named every file that existed on 2026-08-17. Only two are genuine authorship errors.
  This overturns the orchestrator's framing throughout this session.
2026-09-08 -- CORRECTION, orchestrator error, FOURTH of the session. spike-findings.md claimed the
  fixture block is "byte-identical across all 7 files, md5 af38458e...". FALSE: 17 lines in six files,
  21 in UnitOfWorkTests.cs, which interleaves four XML doc lines -- differing line counts make
  byte-identity impossible. Identity holds only at 13 non-blank/non-doc lines,
  md5 5be5c19f761758c9211a53a50825f15f. SPIKE B REPORTED THE CAVEAT AND THE ORCHESTRATOR DROPPED IT
  when composing the artifact. The spike measured correctly; the summary lost it. Consequence checked:
  the manifest's directed 19->17 swap would have replaced a true statement with a false one; T7 refused
  it. T1 is unaffected -- it measured the helper asymmetry itself and its reviewer verified the result
  empirically. Artifact amended in place, previous wording kept verbatim.
2026-09-08 -- T1 (cf32061) and T2 (8784326) committed. Five tasks done, five commits on the branch:
  71972eb T5, a36074d T3, 8b74f54 T4, cf32061 T1, 8784326 T2.
  T6's dependencies (T1-T5) are all done, so T6 dispatched (dir-architect, opus, implementer mode).
2026-09-08 -- T6's criterion 8 is STALE and the orchestrator caused it. It asserts that
  docs/audit/05-tests.md "is NOT edited" and that correcting it "is covered by no task in this
  manifest, and the coverage mapping reports it as an OPEN GAP". That was true when T6's criteria were
  written and became false at Gate 2, when the user made coverage row 14 a task and T7 was added to
  cover exactly it. T6's criteria were not updated when T7 was added.
  NOT raised as an escalation, deliberately. The established mechanism has handled nine criterion
  defects this session without one: the implementer implements what is correct and reports the
  discrepancy, the reviewer rules on it as a `contradicts` finding, and Gate 3 presents it. Amending a
  frozen manifest would need an escalation; recording the true outcome does not. T6 was dispatched with
  the staleness named explicitly and told not to assert the open gap and not to edit 05-tests.md to
  make the criterion true.
  T6 also carries the framing correction: the session's "systematically under-counting" description of
  the audit is FALSE, per T7 and confirmed by T7's reviewer. Six of seven corrections are DRIFT. T6 is
  instructed not to flatten the distinction, because 6b-iii inherits five TEST-* entries and needs the
  accurate version rather than the alarming one.
2026-09-08 -- T7 revision round 1 complete; re-review dispatched to the SAME reviewer (context kept).
  T7 -> in_review. B1 fixed by rewording :756 in place, previous wording preserved verbatim, plus a
  paragraph in CORRECTION 6 stating explicitly WHY that sentence needed correcting in place rather than
  quoting: it sits AFTER the amendment, where the reader has no signal. T7 ran the reviewer's option 3
  (sweep the class) before its option 1 (fix the instance), and its sweep matched the orchestrator's
  exactly on all six hits and on the triage.
  Advisory 1 taken and it is the largest part of the revision: all sixteen TEST-008 `locations` ranges
  re-resolved at 850a10b. T7 reports both Queries/ ranges unchanged and argues that is an internal
  consistency check, since the two query classes never took the UnitOfWorkSubstitute rewrite and so
  should not have moved. Reviewer asked to verify that specifically -- it is the kind of argument that
  is either a real cross-check or a coincidence being read as one.
  Advisory 3 partly declined: six over-wide lines left unwrapped because they are inline backticked
  commands and wrapping them would break criterion 12's pasteability. Reviewer asked to rule; the
  orchestrator agrees but it is the reviewer's advisory and its criterion.
  Diff grew 387/60 -> 448/83, so the reviewer was told to re-derive line numbers rather than reuse its
  own, and to re-verify ALL 15 criteria because a revision can break one that passed.
2026-09-08 -- T7 re-review PASS, B1 closed. Committed e55eb85. T7 -> done, revisionRounds 1.
  Strongest verification of the session: the reviewer ran the `awk` command EXACTLY AS RENDERED in the
  document over all sixteen TEST-008 files at 850a10b, and all sixteen reproduced the claimed range
  character for character. It also verified T7's Queries/ cross-check CAUSALLY rather than accepting
  the correlation: `git grep "UnitOfWorkSubstitute" 850a10b -- Queries/*.cs` -> nothing; both ranges
  byte-identical between 2603170 and 850a10b modulo a space-to-tab conversion; 14 command files carry
  the shape the query files lack. The ranges did not move because the code did not change, and the
  code did not change because the rewrite never reached those two files.
  The reviewer closed its OWN advisory against itself: T7 declined to wrap six over-wide lines because
  they are inline commands and wrapping inserts a newline the next reader would paste. Criterion 12
  makes pasteability blocking; column width is enforced nowhere, and 156 lines already exceed 100
  columns in the baseline. "A cosmetic convention does not get to degrade a criterion the task is
  graded on."
  One new advisory carried to Gate 3, not fixed: unclosed bold span at 05-tests.md:779-780, which will
  run bold across the following paragraph. Verified independently by the orchestrator (:779 has one
  `**`, :780 has none). One-character fix, but advisory findings never trigger a revision round and the
  reviewer said so explicitly. Reopening a passed task for a rendering detail is the nit loop the
  severity gate exists to stop.
2026-09-08 -- SERIAL FULL-SUITE MEASUREMENT, the orchestrator's obligation under DoD 13 and the
  resolution of the criterion defect five tasks hit. Worktree `git status --porcelain` EMPTY at
  e55eb85; T6 touches only a markdown file and cannot affect the build.
    dotnet build src/DungeonMaster.sln                       Build succeeded. 0 Warning(s) 0 Error(s)
    dotnet test src/DungeonMaster.Tests                      Passed: 699, Failed: 0, Total: 699
    dotnet test src/DungeonMaster.App.Tests                  Passed:  77, Failed: 0, Total:  77
    dotnet test src/DungeonMaster.App.UnitTests              Passed:  23, Failed: 0, Total:  23
  RECONCILIATION EXACT: T1 +2, T2 +1, T3 0, T4 0, T5 +1 = +4 against T1's clean 695 baseline -> 699.
  Predicted before measuring, and measured to the test. No leaf silently stopped being discovered.
2026-09-08 -- T6 reported done; reviewer dispatched with a SECOND job: adjudicate a direct conflict
  between T6 and the already-committed T7. T6 -> in_review.
  T7 (committed e55eb85) says six of seven corrections are DRIFT and two are MEASUREMENT ERROR.
  T6 re-measured independently and says FOUR drift, TWO authorship errors, and TWO claimed defects that
  measurement does not support at all.
2026-09-08 -- CORRECTION, orchestrator error, FIFTH of the session, and it propagated furthest.
  Measured directly: all four of TEST-009's cited ranges at 2603170 (14-32, 12-30, 17-35, 17-35) run
  from `private readonly SqliteConnection` THROUGH `private AppDbContext CreateContext()` and are
  NINETEEN LINES EACH. The competing "17" comes from a sed delimiter stopping at `Dispose()`, which
  EXCLUDES CreateContext(). The two figures measure different spans; the delimiter, not the commit, is
  the variable.
  So TEST-009's "nineteen lines" WAS NEVER A DEFECT. It is correct for the span the entry itself cites.
  spike-findings.md's original claim -- "It is 17 lines ... not the 'nineteen' TEST-009 claims" -- was
  wrong, and the orchestrator's FIRST amendment to it repeated the error by classifying the item as
  drift rather than as no defect at all. The wrong claim propagated into the manifest's directed
  19->17 swap, into T7's committed classification, and into the orchestrator's own amendment.
  T7 refused the bare swap and recorded three scoped figures, which was the right instinct and limited
  the damage -- but it still classified the item DRIFT.
  Referred to T6's reviewer to rule with its own commands rather than being decided here. If it holds,
  the committed document carries a wrong classification and that is a Gate 3 decision.
  NOTE: this is a separate matter from the 17-vs-21 finding, which is real -- UnitOfWorkTests.cs
  interleaves four XML doc lines. Two distinct discrepancies were being conflated under one heading.
2026-09-08 -- T6 review: PASS, 0 blocking, 3 contradicts, 5 advisory. ADJUDICATION RESOLVED and the
  orchestrator's alarm was PARTLY WRONG -- the reviewer was told to say so if it was, and it did.
  T6 is correct on both disputes, but `e55eb85` contains NO wrong classification. Its CORRECTION 3
  header reads `Classification: DRIFT` while the block's first substantive sentence is "Nineteen was
  correct when written." The DRIFT label attaches to the SCOPE CHANGE (four classes -> seven, 19 lines
  -> 13 common lines), not to any claim the auditor miscounted. No correction to the committed document
  is required. The residual is a header/body tension a reader resolves in one sentence.
  The reviewer also found a THIRD dispute the orchestrator had not flagged: manifest.json:190 asserts
  TEST-012's prose "presents WithEmptyOutline as the redundant one when it is the NARROW test" -- both
  halves wrong. It is the WIDER test and the prose does not call it redundant. T7 handled it without
  restating the error.
  TEST-026's non-defect is now settled by FOUR independent measurements: T7's reviewer, T7, T6, and
  T6's reviewer.
  C1 records a criterion defect worth carrying: T6's criterion 5 anchored an expected count to HEAD,
  which returned 33 rather than the stated 28 because six peer commits landed during the session. The
  same manifest FORBADE working-tree anchors for T7 (manifest.json:172) and did not apply the rule to
  T6.
2026-09-11 -- T6 reviewed PASS and committed d40d1b1. All seven tasks done, seven commits.
2026-09-11 -- Gate 3 presented (userInteractions 3 -- the gate counts once however many prompts it
  fired). Twelve `contradicts` findings across seven reviews deduped to five decisions. EVERY ONE
  falsifies a claim the orchestrator wrote into a frozen artifact; none was an implementation failure.
  User decisions: carry D1, D3, D4 to the backlog as process items; carry all three section-4
  advisories; approve the merge. D2 and D5 judged sufficiently recorded in-session.
2026-09-11 -- MERGED. `merge-session` -> 7684c5e, --no-ff, into chore/repo-audit. Nothing pushed:
  origin/chore/repo-audit still at 850a10b, verified. Worktree retained for cleanup.
2026-09-11 -- DoD 13a VERIFIED, the one definition-of-done row no task could own. Every session-touched
  .cs file checked in the MAIN CHECKOUT after the merge, not in the worktree: 35 of 35 CRLF-clean
  (CR == LF == CRLF count, none zero), 0 not clean. Both touched markdown files likewise
  (00-master-index.md 568/568, 05-tests.md 1716/1716). A clean worktree does not prove a clean main
  checkout, which is why this was assigned to the orchestrator at Gate 2 rather than left to be assumed.
2026-09-11 -- Backlog appended: five items, 51 insertions 0 deletions, purely additive. Section
  placement verified: the IReadOnlyList identity-guard gap under repository work; the Progress-row
  commit-list gap under audit-document corrections; and the three criterion-authoring defects under DIR
  process artifacts. The Progress row was deliberately NOT fixed in-session -- the user chose at Gate 3
  to track it rather than have the orchestrator edit a committed task's file, so no merge SHA was
  inserted into the index either.
