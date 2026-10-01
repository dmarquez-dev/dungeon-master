# Handoff to session 6b-xvi

Written at the close of 6b-xv, **2026-10-01** (the session began 2026-09-30; Gate 3, the merge and this
handoff ran past midnight — **the slug's date is the session's start**, per the same convention 6b-xiv used).

Self-contained: everything 6b-xvi needs is here or at a path named here. Nothing in it depends on the
conversation that produced it.

**Every number below was re-derived from the repository immediately before writing.** Section 6 gives you the
command for section 2. **Re-run it rather than trusting section 2** — and read section 0 first.

---

## 0. The one thing to carry forward, if you read nothing else

**Nine errors were caught this session. EIGHT were the orchestrator's — six in its own prose and dispatches
(rows 1, 3, 4, 5, 6, 7) and two in criteria its own Gate 2 consistency check passed (rows 2, 8) — and one was
`dir-git`'s (row 9). The two that cost real work were both PREDICATE failures, not arithmetic failures — and
in both cases the total was correct.**

> **CORRECTED before the push: this paragraph first read "Five were the orchestrator's", and that figure was
> false.** It was carried over from an earlier tally whose population was different — one that counted T2's
> wrong comparer name, T3's stale `DbUpdateException` framing and T4's `137`-versus-`142` line-count slip as
> rows, and did not list rows 2, 6, 7 and 8 separately. **The table was rewritten and the count was not
> re-derived from it.** Counting the nine rows below by attribution gives eight and one.
>
> **That is this section's own corollary, committed inside the document that asserts it** — a figure carried
> from a population it no longer describes, where the arithmetic was never wrong because it was never
> redone. Found by auditing the committed file against its own table, which is the only reason it is not
> still there. **Re-derive a count when you rewrite what it counts.**

| # | The claim | What is true | Who caught it |
|---|---|---|---|
| 1 | **"No existing call site constructs a non-finite `CanvasPoint`, and your guard should break nothing"** — cited to a census of 78 sites / 5 production / **0** non-finite, handed to T3 at dispatch | **T3's guard broke 7 tests.** T1 supplies its non-finite values as xUnit **`[InlineData]` theory parameters**, so the construction line reads `new CanvasPoint(x, y)` with no `NaN` or `Infinity` token on it. `grep "new CanvasPoint(" \| grep -E "NaN\|Infinity"` **can never see those rows** — re-running it returns 0 forever | **T3**, which diagnosed both the stale anchor *and* the predicate blindness, and counted a second way: `grep "InlineData" \| grep -cE "NaN\|Infinity"` -> **24** rows, 7 of them T1's |
| 2 | T3 criterion 9's two branches, keyed on T1 observing *either* storage *or* a throw | **T1 observed BOTH.** Both branches fired and required mutually exclusive outcomes, and branch 2's required comment ("materialization cannot encounter a non-finite value") was by then **known false** | The orchestrator, reading T1's result against the frozen text — but **after** the freeze, so it cost amendment `A1` |
| 3 | `measurement.md` §2(e): `InclusiveBetween` rejects `NaN` "because every ordered comparison against `NaN` is false" | **Conclusion right, mechanism wrong.** It compares through `FluentValidation.Validators.ComparableComparer<float>.Instance`, a total order sorting `NaN` below everything | **T2's reviewer**, by loading the shipped DLL and invoking the comparer |
| 4 | `coverage.md`'s prediction credited `Math.Clamp` with bounding the zoom | **`Math.Clamp` does not clamp `NaN`** — returns it unchanged. The Windows conclusion holds because of a `#elif MACCATALYST` fence, not the clamp | **T2**, by running it |
| 5 | T3's dispatch: "every agent that wrote a file this session hit" the LF-vs-CRLF problem | **A universal the orchestrator never measured.** T4's Edit-tool writes came out `bareLF=0` | **T4**, which measured rather than assuming, and noted that normalizing an already-CRLF file is the risk, not the remedy |
| 6 | T5's criterion 5: "both validators bound **`Scale`**" | `UpdatePlacedObjectCommandValidator.cs:37-41` binds **`NewScale`**. Grepping `Scale` alone misses the second validator | **T5** — and the orchestrator had read both files earlier in the session and still relayed the imprecise wording |
| 7 | T3's dispatch: "two stale ranges" in `CORE-016`'s `locations:` block | **Four needed correcting.** `CanvasPoint.cs:9` was correct at the opening commit and **stopped being correct during the session**, because T3 grew the file from 9 lines to 110 | **T5**, which re-read all six rather than the two it was told about |
| 8 | T4 criterion 9: inject by "replacing the helper's `paramName` with a single hard-coded name" | **That cannot compile here.** `CA1507` under `TreatWarningsAsErrors` (`Directory.Build.props:5`) makes it a **build error, not a test failure** — so the criterion as worded is unsatisfiable | **T4**, which ran the literal injection, recognised it was inert, **discarded it rather than reporting it as evidence**, and substituted `nameof(scale)` inside the helper |
| 9 | `f886737`'s body: "15 paths staged", "14 new session-artifact files" | **16 and 15.** And **the 15 was handed to `dir-git` verbatim in its dispatch** | The orchestrator's post-commit read-back — **after** the commit existed, which is the structural limit |

> **The rule you inherited was: a census validates its predicate, not only its total. This session is the
> proof of its cost, and the corollary is sharper than the rule.** Error 1 had a *correct total* measured
> with a *correct command* at a *correct anchor* — and was still useless, because the predicate could not see
> the population under test. **Ask what form the thing you are counting could take that your pattern cannot
> match.** `[InlineData]` is one such form; so is a `with`-expression, a target-typed `new(...)`, and a value
> arriving through a theory parameter. Counting a second way is what surfaces it: T3's second count
> disagreed, T2's second count disagreed (4 vs 6, missing `{ get; set; } = ` initializers), and in both cases
> **the disagreement was the finding.**

**And the counterweight, which is the actual result: six tasks, eight review dispatches, ZERO blocking
findings that were an implementer's fault.** Every defect above was found by someone *re-running* a claim
rather than re-reading it. The single revision round charged was spent by explicit user decision on advisory
findings.

---

## 1. Where the repository stands

- **`chore/repo-audit`** carries the session. **Re-derive the tip and the push state yourself** with
  `git rev-parse --short chore/repo-audit` and
  `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit`.
- **`main` is untouched at `45495c1`**, local and remote identical. Nothing in 6b-xv went near it.
- **The session's commits**, in order:

| Commit | Task | What |
|---|---|---|
| `3cef4dd` | T1 | store behaviour pinned — 8 tests |
| `4031fc8` | T2 | validator and viewport behaviour pinned — 10 tests |
| `93a4c90` | T3 | **the `CanvasPoint` finiteness guard** — the session's only production change |
| `f62b1c7` | T6 | T1's tests rewritten beneath the domain |
| `ba5666d` | T4 | the `PlacedObject` scale-guard extraction |
| `3ca623b` | T5 | the register correction |
| **`b3081ed`** | — | **the merge**, `--no-ff`, **exactly two parents** `75e80ee` and `3ca623b`, verified with `git cat-file -p` rather than taken from `dir-git`'s report |
| `f886737` | — | session artifacts + 5 backlog items. **Its body carries two false figures — see §5 item 6** |

- **Change surface over `75e80ee..3ca623b`: 10 files, 1120 insertions, 24 deletions**, from `git diff --stat`.
- **Suite: 1021 passed — `DungeonMaster.Tests` 890, `DungeonMaster.App.UnitTests` 23,
  `DungeonMaster.App.Tests` 108 — 0 failed, 0 skipped, exit 0. Measured in the MAIN CHECKOUT after the
  merge**, not only in the worktree.
- **All 10 merged files verified `crlf == lf == cr` in the main checkout.** Zero mismatches.

### Index arithmetic — read this before citing any line number

| File | At `75e80ee` | Now | Offset rule |
|---|---|---|---|
| `docs/audit/00-master-index.md` | 687 | **687** | **Unchanged.** One Status cell replaced in place |
| `docs/audit/01-domain-application.md` | 2061 | **2221** | **+160**, all in one block inserted inside `CORE-016` at `:861`. **A citation at or above `:860` is unaffected** |
| `src/.../ValueObjects/CanvasPoint.cs` | 9 | **110** | **+101.** The record declaration moved from `:9` to `:42` |
| `src/.../Entities/Floors/PlacedObject.cs` | 89 | **101** | **+12.** The two guards at `:28-34` and `:76-82` became one helper at `:91-100` with call sites at `:28` and `:70` |
| `src/.../Domain/Entities/Floor/PlacedObjectTests.cs` | 101 | **142** | **+41** |
| `.../CanvasPointNonFinitePersistenceTests.cs` | — | **223** | created `3cef4dd` at 134 lines, **rewritten by `f62b1c7`** |
| `.dir-sdlc/backlog.md` | 441 | **453** | **+12**, all appended at the end: 6 items |

**A measured instance of this table's own limit.** `CORE-016` cited `CanvasPoint.cs:9`, which was **correct**
when 6b-xv measured it and **false by the end of the same session**, because T3 grew the file. T5 caught it;
the dispatch had told it only two ranges were stale. **A range verified at the session's opening commit is not
verified at its closing commit.**

---

## 2. What is open

179 entry rows: **132 closed, 43 open, 4 part done.** Zero rows return `UNCLASSIFIED`.

Open by layer — `CORE` **12**, `UI` 9, `UIKIT` 9, `INFRA` 6, `TEST` 6, plus 1 spanning `UI, XCUT`.
By severity — **S1 0, S2 0**, S3 **1**, S4 32, S5 10.

> **Still no open S1 or S2.** The severity argument has now worked nine times, 6b-vii through 6b-xv.

**`MASTER-081` moved to `part done`. The S3 band fell from 2 to 1 and the CORE layer from 13 to 12.**

**There is exactly one open S3 left in the whole register:**

| Line | Entry | Layer | Effort / risk | Title |
|---|---|---|---|---|
| `:239` | **`MASTER-051`** | UI | S / low | Entry and Editor handler mappers repeat the same eight WinUI resource assignments verbatim |

The **four** `part done` entries, which **must not be marked done**: `MASTER-081` (`:269`, new this session),
`MASTER-094` (`:282`), `MASTER-098` (`:286`), `MASTER-108` (`:296`). Both inherited warnings still stand —
`MASTER-098` was *measured* by 6b-vi and not closed, and **`MASTER-108`'s prose carries a wrong number**: it
says `DungeonDetailPage.xaml.cs` is "2,226 lines", which 6b-xi measured at **2,594**. Untouched by 6b-xv.

---

## 3. The recommended session

A recommendation, not a decision — 6b-xvi's Gate 1 owns the scope.

**Recommended: `MASTER-051` / `UI-003` — the Entry and Editor handler mappers repeat eight WinUI resource
assignments verbatim.** UI, **S3, `effort: S`, `risk: low`**, and **the last open S3 in the register.** Closing
it retires the severity band entirely, which is the argument that has selected nine of the last ten sessions.

**Re-measure first, and expect the entry to be wrong somewhere.** Every entry re-measured in this stream has
had at least one false or stale specific, and 6b-xv falsified **two of two** load-bearing premises on its
recommended entry. `MASTER-051` makes checkable claims — "eight", "verbatim", "Entry and Editor" — and each is
a count or a universal. **`DungeonDetailPage.xaml.cs` is 2,594 lines and the handler mappers live in the App
layer this stream has touched least.**

**The argument against it:** it is the only remaining S3, so closing it removes the selection criterion that
has made nine sessions easy to scope. After `MASTER-051` the register is 32 S4 and 10 S5 and the next session
has to pick on coherence alone.

### The alternatives, ranked

1. **The `git notes` convention — and it is materially stronger than when it was deferred.** It now has
   **three** known candidates rather than two (`a116b2c`, `66a0b73`, **`f886737`**), and 6b-xv produced the
   finding that makes it urgent: **the "hand over every figure" remedy failed** (§5 item 6). Four consecutive
   sessions have put a false figure in a permanent commit body while every repository write was correct.
   `git notes list` -> **0** and `git ls-remote origin 'refs/notes/*'` -> **0**, so the convention must be
   decided before any annotating. **Scope: decide the convention, then ONE sweep.**
2. **`B11` has no mechanical guard, carried from 6b-xiv and still open.** T2's I3 injection proved the naive
   collapse passes all 980 tests and would satisfy six other criteria. **One test closes it.** Still the
   cheapest high-value item in the file, and it has now waited two sessions.
3. **The UI latency spike** (`backlog.md`, 2026-09-18). **Still the only item in the register or backlog that
   came from running the application**, and now untouched by 6b-xii through 6b-xv. Wants a profiler and a
   Release-build comparison; the user asked that a session decide whether it warrants its own `MASTER` entry.
4. **The `PathSegmentKind` migration, from `CORE-035`.** Still **the only item in the register requiring a data
   migration on persisted rows.** Scope it as a migration or not at all.
5. **The five 6b-xv advisories** (§5 items 1–5). All small; item 2 (`Contain("5")`) and item 3 (the
   unattributed figure) are one-line fixes.

---

## 4. The inherited discipline

**Re-measure every entry against a fixed commit before decomposing, and decompose from the measurement.**
Nine sessions of evidence, and 6b-xv is the strongest case yet: **the recommended entry's two load-bearing
premises were both false**, and implementing it as written would have built a policy layer for a problem that
no longer existed. 6b-xv adds six refinements.

### A criterion can be unsatisfiable by its own instrument

**T4 criterion 9 named an injection that cannot compile** — a hard-coded `paramName` literal trips `CA1507`
under `TreatWarningsAsErrors`, producing a build error where the criterion asked for a test failure. **T4 ran
it, recognised it was inert, discarded it rather than reporting it as evidence, and substituted the only
compiling spelling of the same defect.** Its reviewer applied the literal form itself and confirmed.

> **This project has now recorded ten inert injections across three sessions. This is the FIRST caught by the
> implementer that wrote it**, rather than downstream. **When a criterion prescribes an injection, the author
> owes it a check that the injection compiles and fails a test rather than the build.**

### An amendment can be needed because a finding was a SPLIT

`A1` existed because T1 observed `NaN` refused *and* both infinities stored. **T3 c9's two branches both
fired and required mutually exclusive outcomes, and branch 2's required comment was by then known false.**
A criterion keyed on "either X or Y" is unsatisfiable the moment the measurement returns both.

> **When a criterion branches on an unknown a task will measure, ask what happens if the answer is "both".**

### Guarding a type can make a sibling task's evidence inexpressible

T3's guard broke 7 of T1's 8 tests, because the guard works. **That collision was latent in the ordering from
the start and no ordering avoids it**: T1 had to run first to tell T3 what to guard against, and once the
guard exists, T1's route to that evidence is gone. What the framework got right is that **T3 escalated instead
of editing a file it did not own**, which turned a silent weakening of someone else's test into amendment `A2`
and task T6.

**T6's rewrite was better than what it replaced.** T1 *inferred* the ADO attribution from a message resource
and an exception type; T6 demonstrates it on a call path with **no EF frame on the stack**. And **T6 was
required to record what was LOST** — EF's wrapping of the refusal is no longer observable at all — rather than
letting coverage quietly shrink.

### A reviewer's best work was the injection nobody asked for

- **T3's reviewer added Injection C** (field initializers neutered, `init` guards kept) -> exactly the 8
  constructor cases failed. With T3's own Injection B (the inverse) -> exactly the 6 `with` cases. **Together
  they prove the two mechanisms guard disjoint paths and neither is redundant** — stronger than T3's evidence.
- **T6's reviewer injected a text sentinel `'Infinity'`** which `Convert.ToDouble` parses back to
  `Double.PositiveInfinity`, **so the value assertion still passed and only `typeof()` caught it.** That is
  what established the `typeof()` assertion is the load-bearing one.
- **T4's reviewer found an assertion that cannot fail** — `Contain("5")` against a message rendering
  `0.25` — in a test no criterion told it to examine.
- **T2's reviewer closed a route T2 structurally could not check**: T2's census filtered `obj/`|`bin/` out, so
  the reviewer ran the identifier grep **restricted** to them, returning zero.
- **T1's reviewer read the raw column** — `SELECT typeof(Position_X), ...` -> `type=real` — which rules out an
  EF identity-cache artifact that a reload assertion cannot.

### A criterion's letter can conflict with a repository convention, and the convention can win

**T5's criteria 5–7 said three paragraphs must "no longer assert" false claims. The register's own documented
convention forbids deleting previous wording** — stated twice with three cross-references, with `locations:`
correction in place as the **single** documented exception. T5 followed the convention, struck the prose in an
`AMENDED 6b-xv` block quoting each claim verbatim, **reported the conflict as "content met, letter NOT met",
and handed the ruling to review.**

Its reviewer ruled **met**, on grounds worth reusing: *"No longer asserts" is a claim about what the document
asserts, not about which bytes moved… The document cannot simultaneously follow the convention and satisfy a
deletion reading* — and criteria 3 and 4 asking for the in-place exception is **evidence the criteria were
written inside the convention rather than against it.**

### `dir-git` — the split is four sessions old and the remedy just failed

**Every repository write was correct across six `commit-task` dispatches, the merge and the artifact commit**:
every path list, every SHA, every `--numstat`, one parent on each task commit, exactly two on the merge, no
forbidden trailer anywhere, and it correctly **refused to stage a fourth changed path** that belonged to an
unreviewed task when T3's commit was made.

**But `f886737`'s body miscounted two figures, and one of them was handed over verbatim.** 6b-xiv concluded
the handed-figure remedy held and located the exposure in diff-derived claims only. **That conclusion is now
false.** See §5 item 6 for what would actually close it.

### The structural limits, unchanged and re-confirmed

- **Tasks cannot invoke `git`.** T5 needed six commit SHAs and was handed every one; T1, T2, T4 and T6 could
  not verify their own ownership criteria and said so.
- **A reviewer can run `git` where its implementer could not**, and that asymmetry caught real things again:
  T6's ownership criterion was verified **by content** (`git diff --exit-code` against the committed guard)
  where T6 could offer only mtime.
- **No criterion may constrain what a task *reports*** — only what the repository *contains*.
- **A criterion needing a figure a task cannot measure must have that figure handed over at dispatch**, and
  **a handed figure can still be corrupted in prose** — new this session.
- **Revert by re-editing content, never by restoring a copy.** Every injection this session was reverted and
  digest-verified; `CanvasPoint.cs` returned byte-identical after two separate reviewer injections.
- **`sed -i` remains suspended, and the harness re-issued it to EVERY agent. All 21 refused**, across both
  sessions. Restate it in every dispatch, reviewers included.
- **The Write/Edit tools emit LF while `.editorconfig:8` sets `end_of_line = crlf`** — but **not always.**
  T2 and T3 hit bare-LF counts of 105–223; T4's edits came out clean. **Measure; do not assume in either
  direction, and never normalize a file that is already clean.**

### The environment findings

**New instrument traps, each measured:**

- **`grep -c $'\r$'` returns a confident `0` on a fully-CRLF file.** Found by T6's reviewer validating its own
  predicate against a synthetic mixed buffer. Use a byte scan for `0x0A` not preceded by `0x0D`.
- **`grep -v '/obj/|/bin/'` without `-E` filters NOTHING** — the alternation is literal. Two frozen criteria
  carried this form and were fixed before the freeze; proved against a two-line fixture rather than asserted.
- **`CA1507` under `TreatWarningsAsErrors`** turns a hard-coded-string injection into a build error. See §4.

### An inherited finding is WRONG: storage is not uniformly LF

6b-xiv recorded *"Storage: always LF. Every artifact blob is stored LF."* **That is false for one file, and it
is the file every session appends to.** Measured with `git cat-file -p <commit>:<path>` piped through a byte
census:

| Blob at `53c00d2` | crlf | lf | lone CR |
|---|---|---|---|
| `HANDOFF-session-6b-xvi.md` | 0 | 428 | 0 |
| `state.json` | 0 | 116 | 0 |
| `src/.../CanvasPoint.cs` — **tracked source** | 0 | 110 | 0 |
| **`.dir-sdlc/backlog.md`** | **453** | 453 | **8** |

**`backlog.md` is stored CRLF and nothing else is.** All three paths report identically under
`git check-attr text eol` (`text: auto`, `eol: unspecified`), `core.autocrlf` is `true`, and `backlog.md` is
treated as text — `git diff --numstat` returns `2 0` rather than `-`. **So the difference is not
configuration.**

**Its history, measured:** at creation (`bd63eb9`) the blob was **LF**, 7 lines, 0 lone CR. By `e5d1398` it was
**CRLF** with 8 lone CRs, and it has been CRLF at every commit since — 431, 439, 439, 441, and now 453.

> **The practical rule, which is what matters: APPEND TO `backlog.md` AS CRLF.** Both of 6b-xv's appends did,
> and both asserted the lone-CR count before and after and would have failed hard if it moved. **An LF append
> would make the file mixed; a normalization pass over it would rewrite every line and destroy the 8 lone
> CRs** — which are 6b-iv's, not yours, and which ten sessions have now deliberately preserved.
>
> **The mechanism is inferred and NOT run:** most likely git's `text=auto` declines to re-normalize a path
> whose indexed blob already contains CRLF. Stated as a hypothesis, because this session did not test it — and
> the practical rule above holds either way.

**Confirmed again, and these are stable rather than intermittent:**

- **`dotnet test`'s `Passed!` line cannot tell you which TFM ran** — it printed `(net10.0)` for
  `App.Tests` in every one of this session's runs while the assembly ran from
  `net10.0-windows10.0.19041.0`. **Read the `Test run for ...` line.** Reproduced eight times.
- **The zero-warning check is command-specific.** `grep -icE "warning|error"` returns **0** over
  `dotnet test` output but **always 2** over `dotnet build` — and this session confirmed the 2 are literally
  the `0 Warning(s)` and `0 Error(s)` summary lines. Over a build use
  `grep -cE "(warning|error) [A-Z]+[0-9]+"`, which returned **0** at baseline and after T4.
- **The solution is at `src/DungeonMaster.sln`**, not the repository root.

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **453 lines** by `b.count(b'\n')`. 6b-xv appended **12 lines / 6 items**.

> **The 8 lone CR characters are still there and are still not from this session.** Measured in binary before
> and after both appends: **8 every time**, and both append scripts asserted on it and would have failed hard
> if it moved. They date to 6b-iv. **State which method you used** — Python's text-mode iteration gives a
> different total, because universal-newline handling splits on a lone CR too. 6b-xv deliberately did not
> repair them — outside the approved scope, for the **tenth** session running.

**6b-xv's six items, all carried by explicit user decision:**

1. **`CanvasPoint.cs:16-25` frames the `NaN` refusal as a `DbUpdateException` wrapper at commit time** —
   unreachable since T3's guard, and the cited file now asserts the unwrapped ADO refusal. **The defect is
   narrower than it reads**: `:21-22` already carries the correct attribution, so only the wrapper and the
   timing are stale. **The register is already right** (option 1 applied in `3ca623b`); this is the source
   comment agreeing with the tests beside it. Filed as a `contradicts` finding by T6's reviewer and
   independently as an advisory by T3's.
2. **`PlacedObjectTests.cs:95`'s `Contain("5")` cannot fail** — the message renders `0.25`, which contains a
   `5`. It passes even under the two-argument overload that drops `ActualValue`. One line fixes it.
3. **An unattributed figure in the register**, `01-domain-application.md:1022` — "all six production writes …
   were enumerated", with no command, in a block where every other figure carries one. **The count is
   correct**, reproduced three ways; the defect is that nothing in the document says how. **This is this
   session's own block failing the rule this session enforced on fourteen agents.**
4. **`NonFiniteScaleRejectionTests.cs:20` names `ComparableComparer<float>`, which is `internal` to
   FluentValidation** — true of v12.0.0.0 and measured, but upgrade-fragile with no public-API signal.
   Records a **strictly better state** than the false name it replaced.
5. **The MacCatalyst pinch path is a live latent hole.** `Math.Clamp` does not clamp `NaN`;
   `DungeonDetailPage.xaml.cs:612` is the only unbounded `ApplyZoom` caller and is gated solely by the
   `#elif MACCATALYST` fence at `:600-616`. **Matters the day the project targets MacCatalyst.**
6. **`f886737`'s body miscounts two figures, one of them handed over verbatim — and this disproves 6b-xiv's
   conclusion.** Body says "15 paths staged" / "14 new session-artifact files"; the commit holds **16** and
   **15**. 6b-xiv recorded that the handed-figure remedy held and the exposure was diff-derived claims only.
   **It is not.** Fourth consecutive session with correct writes and false prose. **What would actually close
   it:** require the committing agent to **re-derive the body's figures from the staged index and report them
   back as separate labelled values**, so the read-back compares two independent derivations rather than a
   claim against a dispatch. The read-back caught this one — **after** the commit, which is the structural
   limit.

**Still unscheduled from earlier sessions:** `B11`'s missing mechanical guard (6b-xiv, measured not suspected);
the UI latency spike; the five misleadingly named `WithUnknown{Area,Object}Id` tests; the `A3` test with no
non-mutation assertion; the discipline-versus-contract sleep conflict and the clock seam; the `ObjectKindInfo`
`None` absorption; the drop-placeholder indicator design question; `AreaSidebarItem`'s two
`PointerGestureRecognizer`s; the truncated floor name with no recovery affordance; the Appium DIU-versus-pixel
ratchets; the EF instrument that cannot see a consistent namespace rename; the three 6b-iii items on
`docs/audit/05-tests.md`; the `with`-expression gap on `PathSegment` (**which 6b-xv deliberately did not fix —
`CanvasPoint` avoids the shape, `PathSegment` still has it**); the bare `else` in `ValidateControlPoints`; the
count-only assertion in `AreaOutlineTests`; the fifteen items 6b-ix filed; 6b-x's four; 6b-xi's three;
6b-xii's four; 6b-xiii's four; and 6b-xiv's five.

**`dm-conventions` has TWO defects recorded and neither is modified** — separate repository, deliberately
untouched. `backend/dotnet-format.md:567` and `:588` still teach the singular-`Dungeon` namespace this
repository replaced with the plural form; and **`backend/backend-conventions.md` §5's cross-reference is
broken** — it says "see `dotnet-format.md` § 9", but §9 is *XML Documentation Comments* and the content is
**§11, Domain Building Blocks in C#**, at `:552`. Off by two sections. Found by 6b-xv.

**`.dir-sdlc/log.md` does not exist and never has**, across all 24 sessions. This project keeps session-level
logs only, and 6b-xv did not create it. **The framework prescribes appending a line there; this project's
convention overrides it. Do not create it without deciding to.**

**`Floor` carries the same lookup-and-guard pattern five more times** (`FloorMethods.cs:90`, `:128`, `:162`;
`FloorObjectMethods.cs:56`, `:95`). The Domain holds 15 instances; 6b-xiv addressed `Dungeon`'s 10. Untouched
by 6b-xv.

---

## 6. Command to re-derive section 2 yourself

Unchanged, and it still classifies every row. **Re-run rather than trusting section 2.**

```bash
python - <<'PY'
import re
from collections import Counter
rows=[]
for ln,line in enumerate(open("docs/audit/00-master-index.md",encoding="utf-8"),1):
    if not re.match(r'^\| *\d+ *\| *MASTER-', line): continue
    c=[x.strip() for x in line.split("|")]
    rows.append(dict(line=ln,id=c[2],sev=c[3],layers=c[4],title=c[8],status=c[10]))
def state(st):
    s=st.lower()
    if st=="": return "open"
    if "part done" in s: return "part done"
    if "not an open item" in s or "falsified" in s: return "closed"
    if re.search(r'\*\*open', s) or s.startswith("open"): return "open"
    if "**done**" in s: return "closed"
    if "not done" in s: return "not done"
    return "UNCLASSIFIED: "+st[:70]          # never silently bucket
for r in rows: r["state"]=state(r["status"])
print("total entry rows:", len(rows))
for k,v in Counter(r["state"] for r in rows).most_common(): print(f"  {v:4}  {k}")
op=[r for r in rows if r["state"]=="open"]
print("\nby layer:");    [print(f"  {v:4}  {k}") for k,v in Counter(r["layers"] for r in op).most_common()]
print("\nby severity:"); [print(f"  {v:4}  {k}") for k,v in sorted(Counter(r["sev"] for r in op).items())]
print("\nopen S3:");     [print(f"  :{r['line']}  {r['id']}  {r['layers']}") for r in op if r["sev"]=="S3"]
print("\npart done:");   [print(f"  :{r['line']}  {r['id']}") for r in rows if r["state"]=="part done"]
PY
```

**The column index is `[10]`**, because splitting a row on `|` yields a leading and a trailing empty cell.
**Re-run the survey after you edit the index, and again after the closure commit.**

---

## 7. Mechanics

- **Push status must be re-derived, not read here.** The push question is asked separately from Gate 3.
- **Worktree root must be short.** Use `C:/dm-wt/6b-xvi`. This is a MAUI/WinUI solution and a long path
  breaks the WinUI XAML compiler with `MAX_PATH` errors; the scratchpad path is too long. **`C:/dm-wt` is
  empty again after 6b-xv's cleanup.**
- **Tracked files are CRLF and a formatting slip fails the build.** **Suspend the harness's `sed`
  instruction in every dispatch** — it will be re-issued mid-run, to every agent.
- **Close any running `DungeonMaster.App` before building the Windows TFM**, or `MSB3026`/`MSB3027`. That is a
  file lock, not a test failure.
- **Verify in the main checkout after merge**, not only in the worktree. 6b-xv did, on 10 files.
- **Tasks share one worktree and one `obj/`/`bin/`.** 6b-xv ran **all six serially**, on the architect's
  reasoning that two concurrent `dotnet` builds collide on MSBuild output regardless of source disjointness.
  `owns` alone would have permitted T1∥T2 and T3∥T4. **The serial chain also happened to be load-bearing**:
  T3 genuinely needed T1's SQLite observation before it could choose a guard.
- **Suite baseline for 6b-xvi: 1021 passed** — `DungeonMaster.Tests` **890**,
  `DungeonMaster.App.UnitTests` **23**, `DungeonMaster.App.Tests` **108** — 0 failed, 0 skipped, measured in
  the **main checkout** after the merge. Up from 980 at `75e80ee`; the difference is **41 new test cases**
  across five files.
- **To run the app for an observation pass:**
  `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`, then
  launch `src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`.
  **Check the binary is newer than every source file before observing.** **The app opens the user's REAL
  database** — say so before asking for an observation pass.
- **This session's model tiers:** architect, implementers and reviewers on `claude-opus-5`; `dir-git` on
  `claude-haiku-4-5`. There is still **no `.dir-sdlc/config.json`**; those are the documented defaults, and
  each composed discipline is its bundled base alone — **`backend` 42, `test` 37, `database` 35,
  `frontend` 34 lines** by `wc -l`, all far under the 500 cap.
- **There is still no composed discipline for the `architecture` domain.** T5 was an `architecture` task and
  took its conventions from the register's own row formats, which its criteria cited by line.
- **`contracts/` was empty and git cannot track an empty directory**, so the session directory committed **15**
  files and no `contracts/`. The absence is recorded in `manifest.json`'s `"contracts": []` and in
  `coverage.md` — **not** by a placeholder file, and do not create one.
- **Gate 3 has no slot for orchestrator-authored repo content.** 6b-xv used the shape 6b-v through 6b-xiv
  used: merge (`b3081ed`), then `.dir-sdlc/` (`f886737`), then this handoff with the sixth backlog item.
- **Do the `.dir-sdlc/` appends BEFORE dispatching the commit that stages them.** 6b-xv did, twice.
- **Metrics for 6b-xv:** `userInteractions` **6**, `escalationsRaised` **4**, `reviewRounds` **1**,
  `redispatches` **0**. **`reviewRounds` 1 equals the sum of per-task `revisionRounds`, so the identity
  holds** — unlike 6b-xiv, which had to explain why it did not.
- **`state.json` records `userInteractions` 6, which is one more than the value committed in `f886737`.**
  The artifact commit was made before the push question and the item-6 decision were asked, and both are
  counted interactions. **The file was updated and committed with this handoff rather than left understated.**
- **The six interactions:** Gate 1 (which fired **two** prompts — the scope choice, then a re-scope after the
  measurement falsified the entry's premises — and is **one** interaction, because a gate is one interaction
  however many prompts it fires), Gate 2, the batched `E1`/`E2`/`E3` ask, `E4`, Gate 3, and the final
  combined push-and-item-6 ask.
- **4 escalations, 2 amendments.** `E1` -> `A1` (T3's criterion 9, rewritten for a split finding). `E2`
  settled T2's criterion 7 as met-in-substance with no amendment. `E3` spent a revision round on advisory
  findings by user decision. `E4` -> `A2` (added task T6). **Both amendments repaired an orchestrator
  oversight, and neither repaired the other's repair** — unlike 6b-xiv.
- **69 acceptance criteria across 6 tasks: 68 met as written, 1 met in substance.** Eight review dispatches,
  **zero blocking findings that were an implementer's fault.**
