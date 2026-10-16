# Brief — start here for 6b-xx

**Written 2026-10-15 by the orchestrator of 6b-xix. The session's work was done 2026-10-12 and pushed; the
three-day gap is the author's, not the repository's — nothing moved in it, confirmed by
`git log -1 --format=%ci` and the reflog.** Every figure below was measured in the main checkout at
`d58d699` with the command beside it. Figures inherited rather than re-derived are tagged **ATTRIBUTED**.
Anything not measured says **NOT MEASURED** and is not guessed.

This brief sits in 6b-xix's session directory, so the full record is beside it: `spec.md`, the frozen
`manifest.json`, `measurement.md`, `coverage.md`, `state.json`, `log.md`, four composed `disciplines/`, and
**seven** `reviews/` files. **`measurement.md` is the one to read first** — 994 lines, 14 sections, and it
carries **eleven corrections to itself**, every one of them found by an agent the orchestrator dispatched.

---

## Read this section before the recommendation

**The instrument list is now FOURTEEN forms, and two of them are categorically new.** The previous brief said
its own highest-value content was not the layer recommendation but the forms that return a plausible number
instead of an error. That was right, and it is more right now.

**FORM 13 — `Edit` is not the CRLF-safe writer.** Every form 1 through 12 is a *predicate* returning a wrong
number. This one is a **writer silently changing bytes you did not name.** `Edit` preserves CRLF line
**terminators** and **normalises every EMBEDDED lone CR in the whole file, however far from the edit site.**

Measured on `.dir-sdlc/backlog.md`: eight lone CR at lines 257-264, an edit at **line 477**, result **zero**.
Reproduced on a purpose-built fixture — 10 CRLF lines with `\r\r\n` at lines 3 and 4, one `Edit` at line 9 —
**lone CR 2 → 0, all ten terminators intact.** And reproduced **a third time live**, when the revision round
destroyed them again on a file whose own task had just discovered the hazard and was explicitly watching.

- **A 100%-CRLF check passes throughout the loss.** `cat -A f | grep -c '\^M\$$'` equals `wc -l` the whole
  time. The only detection is **lone CR = total CR minus CRLF-line count.**
- **`.dir-sdlc/backlog.md` is the only tracked file carrying them: exactly 8, since 6b-iv**, as four
  two-line units (a content line plus the blank after it, both terminating `\r\r\n`) at byte offsets
  60544/60547, 62183/62186, 63333/63336, 64235/64238.
- **To append to that file, use a byte-append with explicit `\r\n` — never `Edit`.** Then verify at the byte
  level, not by count: **the post-append file's prefix must be byte-identical to the pre-append file.** That
  is a proof; two counts agreeing is not.
- **A criterion mandating `Edit` while also requiring the 8 preserved is mutually unsatisfiable.** That fired
  live in 6b-xix and was ratified as escalation E5. Do not write one.

**FORM 14 — a detector whose pattern appears in the prose it scans, INFLATING a count while every real hit
survives.** On `backlog.md`, where the truth is 8:

| Form | Returns |
|---|---|
| `grep -c '\^M\^M\$' file` without `cat -A` | **2**, all prose, zero genuine — the *already-known* filter-before-`cat -A` error |
| `cat -A file \| grep -c '\^M\^M\$'` | **10** — all 8 genuine **plus 2 prose contaminations** |
| `cat -A file \| grep -c '^\^M\^M\$$'` | **4** |

**Every other catalogued form undercounts. This one overcounts, and the error runs in the same direction a
real finding would** — so nothing looks broken and you get no cue to check. **It bites hardest in the backlog
itself, because a file whose purpose is recording patterns in prose disarms grep for those patterns in
itself.** Detect by binary scan, not text filter.

**Two forms from 6b-xix worth holding as a pair, because they fail in OPPOSITE directions:**

| Form | What the character class does to the CR | Failure |
|---|---|---|
| `[^.]` on a CRLF line | **consumes** it as "a character that is not a dot" | matches where it should not |
| a trailing `[^A-Za-z0-9_]` guard | **requires** a character CR-stripping removed | fails to match at all |

**Knowing one gives no protection against the other.** Any end-anchored predicate over this repository needs
`|$` in its final class.

**And one that produced a false positive confirming a false criterion** — the only form in the register that
does: `grep -c '[^ -~]'` for "non-ASCII" **counts every tab**, because a tab is outside printable ASCII in a
tab-indented repository. Use a high-byte `od -An -tx1 -v` census, controlled: it returns **0** on a file with
no non-ASCII and **18** on `App.Core/Editor/MapTool.cs`, which holds six em dashes.

**The eight older forms are unchanged** and are in `measurement.md` §12 and the previous brief: `grep -c $'\r$'`
environment-dependent; `cat -A` piped to a **double-quoted** `"\^M\$"` returning 0 on a known-CRLF file;
`grep -rln 'a\|b\|c'` without `-E`; `\b` in a shell-quoted regex becoming a literal backspace byte; `\t`
inside `grep -E` matching a literal `t`; `sed -n 'N,Mp'` and `grep -E` with a file argument both stripping
CR; `git show <ref>:<path>` unable to establish line endings; `grep -icE "warning\|error"` returning 2 on a
clean build; and a brace detector lacking the **matching** close paren.

**One refinement measured in 6b-xix:** the double-quoted hazard **did not reproduce** in two separate agents'
shells, both returning the same figure as the single-quoted form. It remains environment-dependent. **Use
the single-quoted `'\^M\$$'` and do not assert what the double-quoted form shows.**

---

## Do this first, and it is not optional

**Re-measure every row you select, before you plan around it.** The evidence is now three sessions deep and
it got worse, not better:

| Session | Rows selected | Carried a false or stale specific | Falsified outright |
|---|---|---|---|
| 6b-xvii | 10 | 7 | 1 |
| 6b-xviii | 8 | 5 | 2 |
| **6b-xix** | **8** | **8** | **2** |

**8 of 8.** `MASTER-120` quoted a declaration line **that does not exist in the tree**, and following its
remedy would have invented a primary constructor, dropped an interface and collided a parameter with an
existing field. `MASTER-134`'s clause "move it to its own file or leave it private" is **impossible** for a
top-level type and would have broken a project and 26 test assertions. **`MASTER-135` had ZERO of its 15
cited spans pointing at a braceless site** — ten drifted +47 to +354 **non-uniformly**, so no single shift
corrects them, and five named sites that refactors had eliminated.

**And yet `MASTER-135`'s substantive claim was confirmed at 21 sites.** That is the pattern in one row: **the
register's diagnoses survive and its coordinates do not.** Treat a row's line numbers, population counts and
suggested remedy as three independently unreliable things, and the remedy as the one that can do damage.

**The free win is spent again and the caveat holds.** Grouping all 18 open rows by exact title returns **no
group larger than one**, so there is no duplicate pair waiting. **That bound is on exact titles only** —
three clusters still share layer, category and effort:

| Cluster | Rows |
|---|---|
| `INFRA/naming/XS` | `MASTER-116`, `117` |
| `INFRA/correctness/S` | `MASTER-152`, `156` |
| `TEST/style/S` | `MASTER-137`, `139` |
| `TEST/naming/XS+M` | `MASTER-126`, `144` |

---

## Where the arc stands

| | | Command |
|---|---|---|
| Branch | `chore/repo-audit` at **`d58d699`**, **0 ahead / 0 behind** | `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit` |
| | **Everything is PUSHED.** Nine commits went to origin; `git reflog show origin/chore/repo-audit` records `c9d9b11..d58d699 update by push`. **All nine confirmed published individually** with `git merge-base --is-ancestor <c> origin/chore/repo-audit` | |
| `main` | **`45495c1`**, local and remote identical, untouched by the whole 6b arc | `git rev-parse --short main`, then `origin/main` **separately** — the two-revision form fails |
| Working tree | clean | `git status --porcelain` |
| **`C:/dm-wt` has ONE entry** | **`6b-xviii`** at `77bbaae`. **6b-xix's worktree was removed.** `6b-xviii` is STALE — its session merged and published on 2026-10-12 — and was left only because the user's cleanup instruction named one worktree. **Consider removing it from the main repository root before you start** | `git worktree list` |
| Session dirs | **27** | `ls .dir-sdlc/sessions \| wc -l` |
| Register | **181 rows — 158 closed, 18 open, 5 part done**, and **0 unclassified** | the survey below |
| Open severity | **S4 13, S5 5** | same survey |
| Open effort | **XS 8, S 5, M 3, L 2** | same survey |
| Open risk | **low 13, medium 2, high 3** | same survey |
| Open layers | **INFRA 6, TEST 6, CORE 4, UIKIT 1, `UI, XCUT` 1** | same survey |
| Part done | **5** — `MASTER-081`, `094`, `098`, `108`, `130`. Unchanged by 6b-xix | same survey |
| `.dir-sdlc/backlog.md` | **175** dated items, up from 164; **513 lines, 521 total CR, 513 CRLF, 8 lone CR** | `grep -c '^- 20'`; lone CR as total CR minus CRLF lines |
| Suite | **1038 passed** — `DungeonMaster.Tests` **900**, `App.UnitTests` **30**, `App.Tests` **108** — 0 failed, 0 skipped | `dotnet test src/DungeonMaster.sln`, run three times serially |
| Build | `Build succeeded.`, **0 Warning(s)**, **0 Error(s)**, all three `DungeonMaster.App` TFMs | `dotnet build src/DungeonMaster.sln` |

**The survey command**, so you re-run it rather than trust the table: one `awk -F'|'` over rows matching
`^\| [0-9]+ \| MASTER-[0-9]+ \|`, classifying **column 11** on its **leading token**, with markdown emphasis
stripped and an `unclassified` counter that must read **0**.

**Three things about that classifier.** Test `part done` **before** `done`. **Never classify by substring** —
the cells are multi-sentence prose and several contain "done" or "fixed" inside it. **And strip `*` before
matching:** 6b-xix's own register task first returned `UNCLASSIFIED=8` because its regex required bold, and
eight pre-existing rows use unbolded `part done` / `open` / `filed found-and-fixed` forms plus one
`done AS A DUPLICATE`. **It fixed the classifier rather than a token, which was correct only because all
eight were rows it had never touched** — verified by the diff showing 8 changed lines, all its own.

**UI went from 8 open rows to 0.** Layer concentration has now beaten severity as a selection criterion
**three sessions running** — CORE nine rows, UIKIT nine, UI eight. **Three is not a law, but it is the
best-evidenced heuristic the arc has**, and nothing has yet contradicted it.

---

## The recommendation: INFRA or TEST, and the tradeoff is real

**INFRA — 6 rows, the best effort ratio on the board, and a genuine split inside it.**

| Row | Sev | Category | Effort | Risk |
|---|---|---|---|---|
| `MASTER-115` | S4 | style | XS | low |
| `MASTER-116` | S4 | naming | XS | low |
| `MASTER-117` | S4 | naming | XS | low |
| `MASTER-147` | S5 | performance | XS | low |
| `MASTER-152` | S5 | correctness | S | low |
| `MASTER-156` | S5 | correctness | S | **high** |

**The split matters more than the ratio.** The four `XS` rows are formatting, naming and one dead-code
removal — a clean sweep. **But `MASTER-152` and `MASTER-156` are both database-guarantee correctness and both
plausibly need a real EF migration**: `156` makes a non-unique `(DungeonId, Number)` index unique, and
`disciplines/database.md:15` makes editing a *released* migration prohibited, so it needs a **new** one.
**NOT MEASURED: whether either row can be satisfied without a migration.** Settle that at Gate 1 before
scoping, because it decides whether INFRA is a four-row sweep or a schema session.

**TEST — 6 rows, and the only layer where every open row is `risk=low`.**

| Row | Sev | Category | Effort | Risk |
|---|---|---|---|---|
| `MASTER-126` | S4 | naming | XS | low |
| `MASTER-128` | S4 | style | XS | low |
| `MASTER-137` | S4 | style | S | low |
| `MASTER-139` | S4 | style | S | low |
| `MASTER-143` | S4 | docs | M | low |
| `MASTER-144` | S4 | naming | M | low |

Production blast radius is **zero** — it touches only test files. The cost is two `M` rows and that a
test-only session closes rows without improving the product. **`137` and `139` cluster, and `126` and `144`
are both naming**, so a file-scoped split may fold them.

**What is NOT a candidate.** **CORE** has 4 but holds `MASTER-157` (`L`, `risk=high`, a 233-line hand-rolled
dispatcher the row itself says "earns its keep") and `MASTER-178` (`M`, `risk=medium`), so its ratio is the
worst available. **UIKIT** has 1 row, `MASTER-158` (`L`, `risk=high`), **excluded at Gate 1 for four
consecutive sessions** — if you are not going to do it, consider saying so permanently rather than
re-deciding. **`MASTER-142`** is the lone `UI, XCUT` row, `S`/`medium`, touching the Windows manifest and
project file — a different blast radius from anything else open.

---

## The five things that will bite on day one

1. **`Edit` destroys embedded lone CR.** See FORM 13. It fired three times in one session, including on a
   task that knew about it. If you touch `.dir-sdlc/backlog.md` at all, byte-append and verify the prefix.
2. **`C:/dm-wt` holds a stale `6b-xviii`.** Create yours as a sibling — `C:/dm-wt/6b-xx` — or remove the old
   one first from the **main repository root**. **Do not nest under the scratchpad path:** this is a
   MAUI/WinUI solution and a long path breaks the WinUI XAML compiler with `MAX_PATH`.
3. **`git show <ref>:<path>` is NOT the session base for everything, and the failure mode differs by file.**
   Both of 6b-xix's re-reviewers corrected the orchestrator on this. For **source** files a post-wave-1
   commit is identical to worktree HEAD, so a base-era claim measured there **looks false** — it nearly
   produced a false blocking finding. For files a **task** first wrote, that same commit effectively *is* the
   base, so round-1 output is **not recoverable from git at all**, task work being uncommitted until Gate 3.
   **Name which tree every line number belongs to.**
4. **Appends before commits, at Gate 3.** Write the `backlog.md` items and the `log.md` entries first, then
   commit. 6b-xix did this correctly and ended with a clean tree.
5. **The solution is `src/DungeonMaster.sln`**, not the repository root (`MSB1009` otherwise); and
   **`dotnet test`'s `Passed!` line cannot tell you which TFM ran** — it prints `(net10.0)` for
   `App.Tests` while that assembly runs from `net10.0-windows10.0.19041.0`. Read the `Test run for` line.

---

## What 6b-xix got wrong about itself, and it is a lot

**The measurement discipline worked. It pointed at the orchestrator more than at the register.**

- **Eleven corrections to `measurement.md`**, all found by dispatched agents, **three of them found
  independently by two parties** — the folder control that counted the file under test as a control on
  itself, the build-file denominator of 10 that is 11, and a "returns nothing" filter that returns three.
- **Six frozen acceptance criteria carried false factual premises**, corrected in the permanent prose rather
  than propagated: typographic apostrophes that do not exist; "zero test references" generalised from a
  population of **one** to a population of **nine**; "two projects break" where an `InternalsVisibleTo` grant
  means one does; a direction to copy amendment blocks **that are not in the file the task owned**; "ends
  mid-attribute" where the attribute is complete and the element truncates; and a frozen invariant attributed
  to the wrong task's wrong criterion.
- **One orchestrator defect reached the permanent record TWICE.** A worked-example citation was correct at
  the session base and moved two lines when a sibling task wrapped a constructor above it **mid-session**.
  The base-era span was handed to **both** register tasks and both wrote it into their cells, where their
  reviewers caught it as a blocking finding **each**. **That is intra-session drift propagated by the party
  whose job was to prevent it** — the same mechanism the session spent its measurement pass cataloguing.
- **Four arithmetic slips of the orchestrator's own**: "wrong at 9 of the 10" where three offsets repeat so
  it is 8; a "362-file control" that is 407 **and scope-sensitive, being 467 without `obj`/`bin` excluded**,
  in a criterion that named no scope; an option label saying "7 records" where its own description
  enumerated nine; and a predicted artifact-commit file count of 15 that was 18.
- **Four relays of a summary where a report was required**, producing one spurious blocking finding and three
  advisories. **Three reviewers diagnosed it before practice changed** — the first diagnosis should have been
  enough.

**The sharpest process lesson, and it is new.** One census predicate was corrected **twice and stayed wrong
both times** — 10, then 11, and the truth is 12 — because **each round's validation cases exercised the
opposite case**, a documented member above its attributes rather than an undocumented one. The arc already
holds that a control must match the defect class. **What this adds: a CORRECTED predicate needs a NEW
control, not the old one re-run.**

**What went right and is worth copying wholesale.**

- **The prose-truth criterion.** 6b-xviii committed a known-false XML summary and review correctly called it
  *advisory*, because no criterion covered doc-comment accuracy. 6b-xix made it **blocking** and **named the
  reviewer as the adjudicating party**: 30 assertions tabulated, each against the post-change line
  establishing it. **The reviewer declined to accept the table and verified every row against sources it
  located itself. None failed.** If your session writes new prose into code, do this.
- **`git commit -F` went 8 for 8 in this session**, and that is the only figure here the orchestrator
  verified. **Do not inherit a running total.** The prior session's brief claims "11 for 11" while its own
  handoff note claims "12 of 12" — **the two disagree and this orchestrator did not resolve them**, so the
  cumulative figure is **NOT MEASURED**. 6b-xix's eight are the seven task commits plus the artifact commit;
  the merge used `-m` with a subject only and is not in the count. Write the full message to a file, dispatch
  `git commit -F <path>`, forbid rewording and trailers. **Verify yourself with `git cat-file -p` and a
  sha256 against the source file — never `%B`, which appends a newline.** Worth knowing: `dir-git` reports
  substituted a **placeholder** for the body-proving output **twice in 6b-xix alone**, after three prior
  instances. The commits were right every time and **no report proved it.** Verify; do not re-prohibit.
- **Criteria as byte-exact files, not retyped prose.** Each dispatch received its criteria written straight
  from the frozen manifest, so what the implementer was handed and what it was judged against could not drift.
- **File-scoped disjoint ownership**, 0 overlaps across 21 task pairs, and **the orchestrator running the
  build serially between waves** — no task can carry a build criterion, because concurrent `dotnet build`
  collides on `obj/`/`bin/`.

**Unchanged and still unscheduled:** `B11`'s missing mechanical guard, **the UI latency spike** — still the
only item in the register or backlog that came from *running the application* — and the
`ObjectRotation`/`PathSegmentKind` enum renames, which need real EF migrations because the enums are
persisted by member name. **That last one now rhymes with `MASTER-156`**: if you take INFRA and it needs a
migration anyway, consider whether the two schema jobs want doing together.

**One standing exemption that must not be re-litigated as a regression.** Generated EF scaffolder output is
exempt from `dotnet-format.md` §9's two-tab rule: all 7 files under
`src/DungeonMaster.Infrastructure/Migrations/` are space-indented with 0 leading tabs, and
`disciplines/database.md:15` makes editing a released migration prohibited rather than merely out of scope.
**A future census of the space-indent form returning 35 is reading the exemption, not finding a defect.**
**This matters directly if you select INFRA.**
