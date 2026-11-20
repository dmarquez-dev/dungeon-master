sir-sdlc 1.0.12 — Gate 3 report — 20261102-1907-domain-invariants

**Decisions needed:** none — the one blocking finding (T2 B1) closed in its first revision round, and no escalation was raised.

## 2. Acceptance criteria

| Task | Domain | Criteria | Met |
|---|---|---|---|
| T1 | Dungeon validators, DungeonConfiguration comment, NameOnlyValidatorTestBase | 21 | 21 |
| T2 | Floor validator, FloorConfiguration comment | 17 | 17 |
| T3 | Area validators, AreaConfiguration comment | 20 | 20 |
| T4 | PathSegment undefined kind | 9 | 9 |
| T5 | AreaOutlineGeometry.Translate | 10 | 10 |
| T6 | ObjectKindInfo guards + real-kind table | 20 | 20 |
| T7 | Dungeon.LastModified on inner NotFound | 14 | 14 |
| T8 | Three-segment outline by value | 9 | 9 |

No criterion is `not met`, and none was amended. Criteria resting on a demonstration:

**T1**
| # | Criterion | Tag | Evidence |
|---|---|---|---|
| 1 | Create rejects Description 2001 | red step | `ContainSingle().Which.PropertyName` == Description; pre-rule "Expected result.IsValid to be False, but found True." |
| 2 | Create accepts Description 2000 | injected defect | C1 (1999) → `Failed: 1`; 2001 test green |
| 3 | Create accepts null Description | injected defect | C2 (`.NotEmpty()`) → null test red; 2001 green |
| 4 | Update rejects Description 2001 | red step | Notes null, `ContainSingle()`, PropertyName Description |
| 5 | Update accepts Description 2000 | injected defect | U1 (1999) → `Failed: 1`; 2001 green |
| 6 | Update accepts null Description | injected defect | U2 → null test red; Description 2001 not among failures |
| 7 | Update rejects Notes 4001 | red step | Description null, `ContainSingle()`, PropertyName Notes |
| 8 | Update accepts Notes 4000 | injected defect | U3 (3999) → `Failed: 1`; Notes 4001 green |
| 9 | Update accepts null Notes | injected defect | U4 → null test red; Notes 4001 not among failures |
| 18 | Build clean | reviewer-reproduced | `0 Warning(s)` / `0 Error(s)` |
| 19 | Tests pass | reviewer-reproduced | 30 / 909 / 108, `Failed: 0` |
| 20 | CRLF | reviewer-reproduced | both counts 0 on 6 files; control 1 / 1 |

**T2** (round 2 verdicts)
| # | Criterion | Tag | Evidence |
|---|---|---|---|
| C1 | Description 2001 rejected | injected defect (round-0 red step) | I1 deleted rule → target red; at-2000 control green |
| C2 | Description 2000 accepted | injected defect | I2 (1999) → only this test red |
| C3 | null Description accepted | injected defect | I3 (`.NotEmpty()`) → red; 2001 test passed |
| C4 | Notes 2001 rejected | injected defect | I4 deleted rule → target red; at-2000 green |
| C5 | Notes 2000 accepted | injected defect | I5 (1999) → only this test red |
| C6 | null Notes accepted | injected defect | I6 → red; Notes 2001 passed |
| C11 | counts true and self-safe | reviewer-reproduced | Migrations CHECK count 0; control 1 |
| C13 | injections run, reverted, hashed | reviewer-reproduced (final hash) | I1–I6 quoted; validator hash equal before/after |
| C14 | build after every revert | reviewer-reproduced (final state) | `0 Warning(s)` / `0 Error(s)` after each of six reverts |
| C15 | test after every revert | reviewer-reproduced (final state) | three `Passed!` (30/915/108) after each of six reverts |

**T3**
| # | Criterion | Tag | Evidence |
|---|---|---|---|
| C1 | AddArea Description 2001 rejected | red step, injected defect | red-step hash matches fd429e3; A1 red, at-2000 green |
| C2 | AddArea 2000 accepted | injected defect | A2 (1999) → `Failed: 1`, this test |
| C3 | AddArea null accepted | injected defect | A3 → this + base valid-command test; 2001 not among them |
| C4 | UpdateArea Description 2001 rejected | red step, injected defect | red-step hash matches; U1 red, at-2000 green |
| C5 | UpdateArea Description 2000 accepted | injected defect | U2 → `Failed: 1`, this test |
| C6 | UpdateArea null Description accepted | injected defect | U3 → `Failed: 5`; Description 2001 not among them |
| C7 | UpdateArea Notes 2001 rejected | red step, injected defect | U4 red, Notes at-2000 green |
| C8 | UpdateArea Notes 2000 accepted | injected defect | U5 → `Failed: 1` |
| C9 | UpdateArea null Notes accepted | injected defect | U6 → `Failed: 5`; Notes 2001 not among them |
| C14 | counts true and self-safe | reviewer-reproduced | Migrations CHECK 0; controls 1 and 3 |
| C16 | injections run, reverted, hashed | reviewer-reproduced (final hash) | 9 injections quoted; validator hashes equal before |
| C17 | build after every revert | reviewer-reproduced (final state) | post-revert.ps1 solution build each revert, `0 Warning(s)` / `0 Error(s)` |
| C18 | test after every revert | reviewer-reproduced (final state) | three `Passed!` (30/924/108) |

**T4**
| # | Criterion | Tag | Evidence |
|---|---|---|---|
| 1 | Undefined kind throws AOORE, ParamName `Kind` | red step | `ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("Kind")`; baseline hash = fd429e3; "no exception was thrown" |
| 2 | Line/CubicBezier rules unchanged | reviewer-reproduced | test diff one insertion; source narrows `else` only; 925 pass |
| 6 | Build | reviewer-reproduced | `0 Warning(s)` / `0 Error(s)` |
| 7 | Tests | reviewer-reproduced | 30 / 925 / 108, no `Failed!` |
| 8 | CRLF | reviewer-reproduced | 0 / 0 both files; control 1 / 1 |

**T5**
| # | Criterion | Tag | Evidence |
|---|---|---|---|
| 1 | Constructor, no `with` | reviewer-reproduced | `grep -nw with` prints nothing; control on fd429e3 prints `42: translated.Add(segment with` |
| 2 | Line + Ctrl1 makes Translate throw, ParamName `Ctrl1` | red step | `Failed: 1`, "no exception was thrown"; baseline hash = fd429e3 |
| 3 | Eight existing Translate tests unmodified | reviewer-reproduced | same 8 methods, single all-additions hunk; 926/926 |
| 7 | Build | reviewer-reproduced | `0 Warning(s)` / `0 Error(s)` |
| 8 | Test | reviewer-reproduced | 30 / 926 / 108 |
| 9 | CRLF | reviewer-reproduced | 0 / 0; control 1 / 1 |

**T6**
| # | Criterion | Tag | Evidence |
|---|---|---|---|
| 1–8 | Each of four methods throws for None and for 999, ParamName `kind` | red step | 8 failures in red.txt against unchanged file, hash = fd429e3 |
| 9 | GetDefaultAnchor preserved | injected defect | Center→North → 28 anchor-row failures only |
| 10 | GetObjectSize preserved | injected defect | default arm changed → 8 failures |
| 11 | GetDisplayName preserved | injected defect | "Weapon Rack"→"WeaponRack" → 1 failure |
| 12 | GetCategory preserved | injected defect | default "Decorations" → 7 failures |
| 13 | Table covers whole enum | injected defect | `BeEquivalentTo(Enum.GetValues…)`; deleting Fountain row → 1 failure |
| 16 | Injections reverted | reviewer-reproduced (final hash) | both hashes = pre-injection |
| 17 | Build | reviewer-reproduced | build after each of 5 reverts, `0 Warning(s)` / `0 Error(s)` |
| 18 | Test | reviewer-reproduced | 187 / 926 / 108 after each revert |
| 19 | CRLF | reviewer-reproduced | 0 / 0; control 1 / 1 |

**T7**
| # | Criterion | Tag | Evidence |
|---|---|---|---|
| 1 | RemoveArea | injected defect | red on LastModified; floor-not-found return never reaches TouchOnSuccess |
| 2 | UpdateArea | injected defect | red on LastModified |
| 3 | UpdateAreaOutline | injected defect | red on LastModified |
| 4 | RemovePlacedObject | injected defect | red on LastModified |
| 5 | UpdatePlacedObject | injected defect | red on LastModified |
| 6 | Control | injected defect | Success + `BeAfter(captured)` passes unmodified and under injection |
| 7 | No sleep | reviewer-reproduced | `Task.Delay\|Thread.Sleep` 0 (control 2); `SpinWait.SpinUntil` asserted |
| 8 | DungeonMethods.cs byte-identical | reviewer-reproduced | SHA256 worktree = fd429e3 |
| 11 | Build | reviewer-reproduced | `0 Warning(s)` / `0 Error(s)` |
| 12 | Test | reviewer-reproduced | 932 / 187 / 108 |
| 13 | CRLF | reviewer-reproduced | 0 / 0, control 1 / 1, no space indent |

**T8**
| # | Criterion | Tag | Evidence |
|---|---|---|---|
| 1 | Equal by value and order; red under reversal | injected defect | `HaveCount(3)` → `Should().Equal(outline)`; red "differs at index 0" |
| 2 | Control: stored outline reversed | injected defect | expected (1,1),(5,1),(5,5), actual reversed |
| 3 | Every other fact passes, unmodified | reviewer-reproduced | one hunk inside R6 test; 932/932 |
| 4 | AreaMethods.cs byte-identical | reviewer-reproduced | SHA256 worktree = fd429e3 |
| 5 | Injection run and reverted | injected defect | `Failed: 4, Passed: 928` verbatim; revert by hash |
| 6 | Build | reviewer-reproduced | `0 Warning(s)` / `0 Error(s)` |
| 7 | Test | reviewer-reproduced | 187 / 932 / 108 |
| 8 | CRLF | reviewer-reproduced | 0 / 0, control 1 / 1 |

Every row: `.sir/sessions/20261102-1907-domain-invariants/reviews/T1.md` … `T8.md`.

**Coverage** (mapped at Gate 2, checked at Gate 3): all 12 definition-of-done rows `met`; D11 was amended at Gate 2 to admit a red step. No contracts, so no contract-operation rows.

**Limits:** none — every verification command ran in the worktree.

## 3. What ran

| Task | Agent | Model | rounds | redispatches | Commit |
|---|---|---|---|---|---|
| — | sir-architect (decomposition) | opus | — | — | — |
| T1 | sir-backend-engineer | opus | 0 | 0 | `4be3fc0` |
| T2 | sir-backend-engineer | opus | 1 | 0 | `34f9feb` |
| T3 | sir-backend-engineer | opus | 0 | 0 | `375037d` |
| T4 | sir-backend-engineer | opus | 0 | 0 | `ce871e2` |
| T5 | sir-backend-engineer | opus | 0 | 0 | `5ecfca8` |
| T6 | sir-frontend-engineer | opus | 0 | 0 | `eb53bd6` |
| T7 | sir-test-engineer | opus | 0 | 0 | `5d64494` |
| T8 | sir-test-engineer | opus | 0 | 0 | `e76de04` |

Each task reviewed by `sir-reviewer` (opus): 9 reviewer dispatches, one of them T2's re-review. `rounds` sums to 1, equal to `reviewRounds`.

**Change surface:** `fd429e3..e76de04` on `sir/20261102-1907-domain-invariants` — 8 commits, 22 files changed, 1114 insertions, 203 deletions. Full diff: `git diff fd429e3..e76de04`.

The worktree `C:/dm-wt/domain-invariants` also holds one uncommitted change outside every task's owns: `src/DungeonMaster.App/packages.lock.json` (3+/3-), not part of this range.

## 4. Advisory findings

**Contract gaps** (1)
- T5 A1 — `AreaOutlineGeometry.cs:25-31`: `Translate` can now throw `ArgumentOutOfRangeException` (ParamName `Kind`) for an undefined kind via T4's constructor; its XML doc does not list it.

**Test and CI concerns** (10) — see `reviews/T1.md`, `T2.md`, `T3.md`, `T7.md`, `T8.md`.

**Cosmetics** (7) — see `reviews/T1.md`, `T2.md`, `T3.md`, `T4.md`, `T6.md`.

## 5. Metrics

- `userInteractions`: 3 (Gates 1, 2, 3) — the push question, asked after the merge, is not yet counted
- `escalationsRaised`: 0
- `reviewRounds`: 1
