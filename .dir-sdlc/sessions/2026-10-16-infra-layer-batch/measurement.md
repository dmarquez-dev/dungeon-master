# Measurement — 2026-10-16-infra-layer-batch

Every figure here was produced by a command run in this session, in the tree named beside it. Figures
inherited rather than re-derived are tagged **ATTRIBUTED**. Anything not measured says **NOT MEASURED**.

Two trees are in play and every line number below names which:

- **main checkout** — `C:/Users/dmarq/Development/Repos/dungeon-master` at `chore/repo-audit` = `9b33961`
- **worktree** — `C:/dm-wt/6b-xx` at `dir/2026-10-16-infra-layer-batch` = `9b3396163281e6208b0e98e9dd8d11bd20a3e1f4`

Both are the same commit at session start, so a line number is unambiguous until the first task commit.

---

## 1. Instrument form 15 — `grep -c $'\r$'` is context-dependent within one shell

**This supersedes form 1 in the 6b-xx brief**, which called the form "environment-dependent". It is not
environment-dependent; it is *context*-dependent inside a single shell invocation, and the two contexts fail
in **opposite directions**. Measured on purpose-built fixtures in session scratch:

| Fixture | Truth | `grep -c $'\r$'` standalone | same inside `$( )` |
|---|---|---|---|
| `lf.txt` — 3 LF lines | 0 CR | **0** correct | **3** wrong |
| `crlf.txt` — 3 CRLF lines | 3 CR | **0** wrong | **3** correct |
| `lonecr.txt` — 5 CRLF lines, lines 2 and 4 terminating `\r\r\n` | 2 lone CR | **0** wrong | **5** wrong |
| `midcr.txt` — 3 CRLF lines, one CR mid-line | 1 lone CR | **0** wrong | **3** wrong |

Standalone, the pattern never matches CR and the form returns **0 always**. Inside a command substitution the
`\r` is lost and the pattern collapses to `$`, which matches every line, so the form returns **the total line
count always**. Each is correct in exactly one of the four cases, which is why both read as trustworthy.

**This session was misled by it before catching it.** The same file — the prior session's composed
`backend.md` — returned `crlf=0` standalone and `crlf=48` inside `$( )`, two commands apart. The first
reading produced a false claim, stated to the user, that this project's composed disciplines are LF.

**The replacement** is a byte census: flatten the file with `od -An -tx1 -v`, count `0d` bytes for total CR,
count `0d` immediately followed by `0a` for CRLF, and take lone CR as the difference. It is `crcensus.sh` in
session scratch. It passes all four fixtures above, **including both lone-CR shapes** — terminator-adjacent
and mid-line. The two LF/CRLF fixtures alone would not have been a control matching the defect class, since
neither contains a lone CR; they were added for that reason.

## 2. Line endings, measured on the controlled census

| File | Lines | CR | CRLF | Lone CR |
|---|---|---|---|---|
| `.dir-sdlc/backlog.md` | 513 | 521 | 513 | **8** |
| `.dir-sdlc/sessions/2026-10-12-ui-layer-batch/state.json` | 129 | 0 | 0 | 0 |
| `.dir-sdlc/sessions/2026-10-12-ui-layer-batch/coverage.md` | 53 | 0 | 0 | 0 |
| `.dir-sdlc/sessions/2026-10-12-ui-layer-batch/disciplines/backend.md` | 48 | 45 | 45 | 0 |
| bundled `dir-ect 0.15.0 disciplines/backend.md` | 45 | 45 | 45 | 0 |
| this session's composed `disciplines/backend.md` | 48 | 45 | 45 | 0 |

**The backlog's 8 lone CR are confirmed**, now on an instrument with a matching control rather than by a
predicate that happened to agree. **ATTRIBUTED** from the brief, not re-derived here: that they sit at lines
257-264 as four two-line units, and that `backlog.md` is the only tracked file carrying them.

**Composed disciplines are mixed-ending, not LF.** The bundled sources are fully CRLF, and composition
prepends three LF lines (two provenance comments and a blank), giving 48 lines of which 45 are CRLF. This
session's four composed files reproduce the prior session's profile exactly, because both were built with
`printf` for the headers and `cat` for the body. They must not be rewritten with a whole-file writer.

## 3. The six rows, re-measured. 6 of 6 carried a false or stale specific

Re-measurement is not optional, per the brief; the arc's three prior sessions ran 7-of-10, 5-of-8 and 8-of-8.
This session makes it **6 of 6**, with two rows materially changed rather than merely drifted.

| Row | Substantive claim | Measured |
|---|---|---|
| `MASTER-115` / INFRA-006 | "indented with four spaces" | **FALSE.** `grep -c '^    '` = **0**; `grep -c $'^\t'` = **34**. Already tab-indented. The inline-lambda and bare-`<inheritdoc/>` halves survive |
| `MASTER-116` / INFRA-007 | two single-letter lambdas | **TRUE**, at lines **45** and **50**, not the cited 46/51 |
| `MASTER-117` / INFRA-017 | file/type mismatch | **TRUE**, citation **exact** at line 12. Out-of-scope Application cite says `:27`; actual is **13** |
| `MASTER-147` / INFRA-009 | `AsNoTracking` is a no-op on the summary query | **TRUE.** Cited `:17-18` is the method signature; the call is line **23** |
| `MASTER-152` / INFRA-002 | "eleven string columns"; limits "enforced today by exactly one thing -- the FluentValidation validators" | **Count FALSE: nine.** **Premise FALSE: three of nine.** See §4 |
| `MASTER-156` / INFRA-005 | index non-unique, deliberately | **TRUE**, citation **exact** at lines 58-59. Both halves of the history claim verified. See §5 |

**Two of the six citations were exact** — `MASTER-117` at `:12` and `MASTER-156` at `:58-59`. The pattern the
brief named holds: the diagnoses survive and the coordinates do not.

**CORRECTED under escalation E6.** This line originally read *"Two exact citations in six is the best ratio
the arc has recorded."* **That superlative was unestablished and it was this orchestrator's own invention.**
The brief's table at `BRIEF-session-6b-xx.md:89` has the columns `Rows selected | Carried a false or stale
specific | Falsified outright` — **there is no citation-exactness column, so the arc has never recorded the
metric the superlative compared against.** And on the only reading that makes the comparison possible the
claim is false in the other direction: the brief's 6b-xviii row records 8 rows selected with 5 carrying a
false or stale specific, leaving **at least 3 of 8** with none — a better ratio than 2 of 6.

**The propagation is the part that matters.** The superlative did not stay in this file. It was written into
`manifest.json` as T5 criterion 9, which **mandated the clause verbatim**, and from there into the permanent
register at `00-master-index.md:344`. Found by T5's reviewer as a `contradicts` finding on its second pass,
after the orchestrator had frozen it and after a task had been obliged to reproduce it. `02-infrastructure.md`
carries **0** occurrences, so T6 did not inherit it.

**This is the defect class the brief described as "intra-session drift propagated by the party whose job was
to prevent it", and the standing rule it breaks is already in the arc's record: a universal is a count
without a digit in it, and it is measured when the artifact carrying it is frozen.** The figure that survives
is the one with a digit: two of six, both named.

### MASTER-115's remaining scope, with a control

**AMENDED: the count is 11, not 10.** The original predicate here,
`grep -cE '\([a-zA-Z]+ => [^)]*\)'`, returns **10** — and it under-counts, because it requires a closing
paren on the **same line** and so cannot match line **43**:

```
                builder.OwnsOne(obj => obj.Position, position =>
```

Counted a second way with a predicate that could not share that mistake,
`grep -cE '=>[[:space:]]+[^[:space:]]'` returns **11** on `PlacedObjectConfiguration.cs` — lines 18, 20, 23,
26, 30, 34, 37, 40, **43**, 45, 50 — and **0** on each of `AreaConfiguration.cs`, `DungeonConfiguration.cs`
and `FloorConfiguration.cs`. The control is the three files the row itself names as compliant, so it matches
the defect class under test. Found by `dir-architect`, verified by the orchestrator. The bare
`/// <inheritdoc/>` is at line **13**.

**Note what the control did not do: it passed under the wrong predicate too.** All three siblings returned 0
for the 10-form as well, because the defect the 10-form is blind to — a call site spanning lines — does not
occur in the compliant files either. A control can match the defect class under test and still be silent
about a predicate's blind spot. See §10, form 16.

**Lines 45 and 50 are simultaneously MASTER-115's and MASTER-116's sites** — on those two lines the
continuation reformat and the `p` to `point` rename are one edit. The two rows cannot be separate concurrent
tasks.

## 4. MASTER-152's premise is false, and this is the session's most consequential finding

The row states the length limits are "enforced today by exactly one thing -- the FluentValidation validators
in the Application layer." Measured in the worktree:

| Entity | Property | EF `HasMaxLength` | Validator rule |
|---|---|---|---|
| `Dungeon` | `Name` | 100 (`DungeonConfiguration.cs:33`) | `CreateDungeon` + `UpdateDungeon`, `MaximumLength(100)` |
| `Dungeon` | `Description` | 2000 (`:37`) | **none** |
| `Dungeon` | `Notes` | 4000 (`:41`) | **none** |
| `Floor` | `Name` | 100 (`FloorConfiguration.cs:40`) | `AddFloor` + `UpdateFloor` |
| `Floor` | `Description` | 2000 (`:44`) | **none** |
| `Floor` | `Notes` | 2000 (`:48`) | **none** |
| `Area` | `Name` | 100 (`AreaConfiguration.cs:78`) | `AddArea` + `UpdateArea` |
| `Area` | `Description` | 2000 (`:82`) | **none** |
| `Area` | `Notes` | 2000 (`:86`) | **none** |

- **Nine** `HasMaxLength` calls, not eleven: 3 + 3 + 3 + 0 across the four configurations.
- They reconcile against the released migrations as **8 + 1 + 0** — eight `maxLength` in
  `20260805014736_InitialCreate.cs`, one in `20260805184106_AddFloorDescription.cs`, zero in
  `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs`. Counting the population a second way
  is what validates the nine.
- **All six `MaximumLength` rules in the Application layer are `MaximumLength(100)`** and all six guard a
  `Name`. There is no `MaximumLength(2000)` or `MaximumLength(4000)` anywhere in the solution.
- `grep -rn 'Description\|Notes' --include=*Validator.cs` over all **14** validator files returns **zero
  hits**.
- `grep -rn 'Length'` over `DungeonMaster.Domain`, excluding the geometry types, returns **NONE**. The Domain
  assigns `Description` and `Notes` by raw assignment at eight sites.

**So three of the nine columns are bounded by a validator and six are bounded by nothing in any layer** —
not the database, not the Application layer, not the Domain. A comment asserting the row's premise would be
false for two thirds of the columns it describes, and this session's only available defect is a false comment.
Carried to Gate 2 as a decision rather than resolved here.

## 5. MASTER-156's history claim, both halves verified

- `git log --all -S'RemoveFloorNumberUniqueIndex'` returns **5** commits. The two that add and delete the
  file, from `git log --all --name-status --diff-filter=AD`, are the ones that matter:

| Commit | Status | What it did |
|---|---|---|
| `e402ed8 feat(app) Implement DungeonDetailPage with floor/area management and drag-and-drop reordering` | **A** | Added `20260511120000_RemoveFloorNumberUniqueIndex.cs` and its designer |
| `7a4bb2d chore(db): squash migrations to a single floor-level baseline`, 2026-08-04 18:49:20 -0700 | **D** | Deleted both in the baseline squash |

**The unique index was dropped by the same commit that implemented drag-and-drop floor reordering.** That
converts the register's hedge — "I believe the reason was the floor-reorder command" — into a fact, and it is
the strongest evidence the comment can cite.

The migration body, read with `git show e402ed8:<path>`, is unambiguous. `Up` drops
`IX_Floors_DungeonId_Number` and recreates it on `{ "DungeonId", "Number" }` with **no** `unique` argument;
`Down` recreates the same index with `unique: true`. So the index *was* unique and was deliberately made
non-unique, rather than having simply never been declared unique.

**This is pre-resolved history evidence and the orchestrator owns it.** Tasks cannot run git, so
`MASTER-156`'s criteria cite these figures from this file rather than requiring a task to re-derive them.
- `Migrations/` holds **7** files — three migrations plus their designers, and `AppDbContextModelSnapshot.cs`.
  No `RemoveFloorNumberUniqueIndex` survives the squash, consistent with the row.
- `DungeonMethods.cs:125-142` shifts a block of floors and *then* sets the moved floor. Moving floor 3 to 1
  passes through numbering `{2,3,3}` inside one `SaveChanges`. SQLite has no deferrable unique index, so a
  unique index rejects that mid-transaction.

**Therefore making the index unique is not an `S` row.** It requires renumbering through a disjoint range,
which is a redesign of the reorder command. The row's comment branch is the only remedy its stated effort
supports. **No EF migration is needed by either correctness row** — this settles the brief's open Gate 1
question in the negative.

## 6. MASTER-117 is NOT a pure rename — AMENDED

**This section originally claimed the rename needs "no code reference changes". That was FALSE, and it was
the orchestrator's error, not the register's.** `MASTER-117`'s own register citation is exact; the false
claim was added here. Found by `dir-architect` during decomposition and verified by the orchestrator before
amendment.

What is still true:

- `DungeonMaster.Infrastructure.csproj` contains **no** `Compile` item — SDK-style globbing, so no project
  edit is needed.
- The type is already named `InfrastructureServiceCollectionExtensions`, so the target filename matches the
  existing type and **no reference to the type changes**. The only consumer,
  `DungeonMaster.App/MauiProgram.cs:132`, calls `AddInfrastructureServices`, a member name the rename does
  not touch.

**What the original claim missed.** One test references the file **by path, as a string literal**:

```
src/DungeonMaster.Tests/Infrastructure/Async/ConfigureAwaitDisciplineTests.cs:30
                "DungeonMaster.Infrastructure/DependencyInjection.cs",
```

It is the first of four entries in the `ScopedFiles` array at `:28-34`, and the test asserts
`File.Exists(fullPath).Should().BeTrue(...)` for every entry, with the message "the scoped file '{0}' must
exist for this guard to mean anything". **Renaming the file without editing that literal fails the test
outright.** `grep -rn 'DependencyInjection\.cs'` over `src`, excluding `obj` and `bin`, returns **exactly 1**
hit — that line.

**Why the original survey could not see it.** It searched for the *type* name
(`InfrastructureServiceCollectionExtensions|AddInfrastructure`) and for csproj `Compile` items. Neither
population can contain a filename inside a string literal. The defect class was "references to the file
path" and that population was never searched. **A survey answers the population it is scoped to**; the check
that was owed here was whether the surveyed population could contain the case under test, and it could not.

**Second-order consequence for MASTER-147.** `DungeonReadRepository.cs` is entry **3** of that same
`ScopedFiles` array, so the same guard scans the file `MASTER-147` adds a comment to. Its `IsComment` helper
at `:87-93` classifies a line as a comment only via `TrimStart().StartsWith("//")`, `StartsWith("/*")` or
`StartsWith('*')` — so **a comment appended after code on the same line is scanned as code.** A trailing
comment mentioning `await` would therefore be read as an unconfigured await and fail the guard. Every comment
that task adds must be a **whole-line** `//` comment.

## 7. Standing exemptions, restated so they are not re-litigated as regressions

**ATTRIBUTED** from the brief and not re-derived: the 7 files under
`src/DungeonMaster.Infrastructure/Migrations/` are space-indented and exempt from `dotnet-format.md` §9's
two-tab rule, and `disciplines/database.md:15` makes editing a released migration prohibited. A census of the
space-indent form returning 35 is reading the exemption, not finding a defect. **This session touches no
migration**, per §5.

## 8. Hazards this session must route around

- **`Edit` normalises every embedded lone CR in a file**, however far from the edit site, while leaving CRLF
  terminators intact. `backlog.md` is the only tracked file carrying them. Any append to it is a byte-append
  with explicit `\r\n`, verified by the pre-append file being a byte-identical **prefix** of the post-append
  file — not by two counts agreeing.
- **`Edit` strips trailing whitespace from `new_string`.** `dotnet-format.md` §9 requires tag descriptions on
  a new line indented **two tabs** from `///`, so `MASTER-115`'s doc block is exactly the shape this hazard
  bites: an edit producing `///` followed by two tabs and nothing else reports success and yields no indent.
  Every such edit must write the full line including its sentence text.
- **`sed -i` rewrites whole-file CRLF to LF** and fails the build. No task may use it.
- **The solution is `src/DungeonMaster.sln`**, not the repository root.
- **`dotnet test`'s `Passed!` line cannot identify the TFM** — read the `Test run for` line instead.
- **`grep -icE "warning|error"` returns 2 on a clean `dotnet build`** and 0 on a clean `dotnet test`. The
  zero-warning check is command-specific.

## 9. Baseline, measured in the worktree at session start

Measured at `C:/dm-wt/6b-xx`, commit `9b3396163281e6208b0e98e9dd8d11bd20a3e1f4`, before any task was
dispatched. The orchestrator runs these serially between waves, because concurrent `dotnet build` collides on
`obj/` and `bin/`; **no task may carry a build or suite criterion.**

`dotnet build src/DungeonMaster.sln`:

- `Build succeeded.`, **0 Warning(s)**, **0 Error(s)**
- Three target frameworks present: `net10.0`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`

`dotnet test src/DungeonMaster.sln`: **1038 passed, 0 failed, 0 skipped**

| Assembly | Passed | Ran from, per the `Test run for` line |
|---|---|---|
| `DungeonMaster.Tests` | 900 | `bin/Debug/net10.0` |
| `DungeonMaster.App.UnitTests` | 30 | `bin/Debug/net10.0` |
| `DungeonMaster.App.Tests` | 108 | `bin/Debug/net10.0-windows10.0.19041.0` |

This matches the brief's figures for `d58d699`, so nothing moved between that commit and `9b33961`.

**The TFM hazard reproduced live.** `App.Tests`'s `Passed!` line reads `DungeonMaster.App.Tests.dll
(net10.0)` while its `Test run for` line names `net10.0-windows10.0.19041.0`. The `Passed!` line cannot
identify the framework that ran; read `Test run for`.

## 10. Instrument form 16 — a predicate requiring a closing delimiter on the same line

**Mechanism.** A census pattern that demands its closing delimiter on the same line silently skips every
call site that spans lines. It **under-counts**, and it returns a clean figure with no error.

**It produced two defects in this session, in two different shapes.** Both were the orchestrator's:

| Defect | Predicate | Returned | Truth | What it could not see |
|---|---|---|---|---|
| MASTER-115's population, §3 | `\([a-zA-Z]+ => [^)]*\)` | 10 | **11** | line 43, whose `)` is on a later line |
| MASTER-117's references, §6 | type-name and csproj-`Compile` greps | 0 refs | **1** | a filename inside a string literal |

The second is the same failure at one remove: not a delimiter on the wrong line, but a **population that
could not contain the case under test**. Both were caught the same way — by counting the same population a
second time with a predicate that could not share the first one's mistake.

**And the controls did not help.** In the first case all three control files returned 0 under the wrong
predicate as well, because the blind spot — a multi-line call site — does not occur in them either. **This
sharpens the arc's existing rule.** "A control must match the defect class" is necessary and is not
sufficient: a control confirms a predicate sees the defect it was built for, and says nothing about a
defect the predicate cannot express. The only check that caught either of these was a **second predicate**,
not a second control.

## 11. The orchestrator's own corrections, counted

The arc's standing lesson is that the orchestrator's artifacts need correcting independently of the register
being wrong, and those are different failure modes. Keeping them separate:

**Register rows carrying a false or stale specific: 6 of 6.** Unchanged by anything in this section.
`MASTER-117`'s register citation is **exact**; the false claim about it was introduced here, by this
orchestrator, and is not a register defect.

**Corrections to this file so far: three, all found by a dispatched agent or by the orchestrator's own
re-measurement, none by the register.**

| # | Claim as written | Truth | Found by |
|---|---|---|---|
| 1 | composed disciplines are LF | mixed — 3 LF header lines over a 45-line CRLF body | orchestrator, re-measuring after the instrument failure in §1 |
| 2 | `MASTER-117` needs "no code reference changes" | one test references the path as a string literal | `dir-architect` |
| 3 | 10 inline-lambda sites | **11** | `dir-architect` |

Corrections 1 and 3 share a mechanism with the defects they describe: each was a measurement stated with a
command beside it, where the command was sound and the **predicate** was not. A command that ran is not a
command that measured the claimed property.

## 12. Corrections to the register's detail prose, beyond its coordinates

Facts the detail entries assert that are wrong, and that a comment written from them would inherit:

- **INFRA-009** describes `DungeonSummaryDto`'s members as "`Guid`, `string`, `int`, `DateTimeOffset`".
  Measured at `DungeonSummaryDto.cs:24-30`, the two timestamps are **`DateTime`**, not `DateTimeOffset`. The
  substantive claim — that every member is a scalar and the projection contains no entity type — **holds**,
  so the row's conclusion survives and only its type name is wrong. A comment must not say
  `DateTimeOffset`.
- **INFRA-002**'s premise is false as recorded in §4.

**ATTRIBUTED** to `dir-architect`, not re-derived by the orchestrator: that a 7th `MaximumLength` grep hit
exists at `NameOnlyValidatorTestBase.cs:7` and is documentation prose rather than a rule. It falls outside
the Application-layer scope the §4 census used, so it does not change the 6 figure there — but a census
scoped to the whole solution would count it and report 7 guarded columns.

## 13. Instrument form 17 — a command annotation that falsifies itself by being written down

**Found by T4's reviewer, confirmed by the orchestrator.** This is the third contamination finding of the
session and the most pointed, because **the contaminating text is the command annotation a criterion
required.**

T4's criterion 11 mandates the register of `AreaConfiguration.cs:18-36`: a measurement stated **together with
the command that produced it**. T4 complied. The comments now read, in all three files:

> `// No CHECK constraint is generated either, and that half IS measured here: `grep -o 'CHECK' *.cs` over`
> `// Migrations/ returns 0, and `grep -rn 'HasCheckConstraint' src` returns 0.`

Re-run after the commit:

| Annotation, as the comment states it | Re-run returns | Why |
|---|---|---|
| `grep -rn 'HasCheckConstraint' src` → 0 | **3** | all three hits are the comment's own text, one per file |
| `grep -rn 'MaximumLength' src --include=*.cs` → 7 (6 rules + 1 doc prose) | **10** | the extra three are the comments themselves |
| `grep -o 'CHECK'` over `Migrations/` → 0 | **0** | the population excludes the files the comments live in |

**The substance is intact and was verified both ways.** Real `HasCheckConstraint` call sites, excluding
comment lines, is **0**; the six `MaximumLength` rules are all `(100)` and all on a `Name`. The claim "no
CHECK constraint is generated" rests on the `Migrations/`-scoped command, which still returns 0, so
criterion 4's requirement is met by the surviving clause and the finding is **advisory, not blocking.**

**How this differs from form 14.** Form 14 is a detector whose pattern appears in prose it happens to scan,
inflating a count. Form 17 is narrower and worse: **the prose is the command annotation itself, and the
annotation exists because a criterion demanded reproducibility.** The requirement is what destroys the
property it was written to guarantee. Form 14 bites a measurement that happens to run over documentation;
form 17 bites every measurement that documents itself in the population it measures.

**The rule it yields.** An annotation stating a command and its result must either **scope the command to a
population excluding the file the annotation lives in** — which is why the `Migrations/`-scoped clause
survived and the `src`-scoped clauses did not — or state the result as of a named commit rather than as a
reproducible present-tense fact. The surviving clause and the falsified ones are in the same sentence of the
same comment, which makes this an unusually clean natural experiment.

**Three contamination findings, one session, three venues:** T3's criterion counting `AsNoTracking` (amended,
A1); T4's criterion counting `HasMaxLength` (resolved by rewording, escalation E5, accepted); and this one,
which no criterion anticipated and which the deliverable carries into the permanent record. The first two
were caught before commit by the implementers themselves. **This one was caught after the work was final, by
a reviewer re-running a command it was not asked to re-run.**

## 14. Instrument form 18 — `grep -oiF` / `grep -icF` CRASHES, and the crash reads as zero hits

**Found by T5's reviewer. Reproduced by the orchestrator on a two-word fixture.** This is the most dangerous
form in the catalogue, because the tool does not return a wrong number — it **dies**, and the pipeline
converts its death into the single most plausible wrong answer.

Measured on a fixture containing the one line `HELLO hello`:

| Command | Result |
|---|---|
| `grep -oiF 'hello' <file>` | **Aborted, signal 6, exit 134**, no output at all |
| `grep -oiF 'hello' <file> 2>/dev/null \| wc -l` | **0** — a confident, silent zero |
| `grep -oF 'hello' <file>` | `hello`, exit 0 — **sound without `-i`** |

**Combining `-i` with `-F` is what breaks it.** `-oF` and `-cF` without `-i` are reliable; the reviewer's
escaped-pipe counts used `-oF` and returned non-zero correctly. Case-insensitive literal matching needs a
`tr 'A-Z' 'a-z'` copy piped into `grep -cF`.

**Why it matters more than an ordinary wrong predicate.** Every other catalogued form returns a number that
is wrong. This one returns **nothing**, and `wc -l` turns nothing into `0` — which, for a scan asking "does
this file contain any forbidden phrasing?", is exactly the answer that means *clean*. A reviewer scanning 18
phrasings for unestablished universals got 0 on all 18 and would have reported criterion 12 met on no
evidence whatsoever.

**It was caught only by a positive control that had to fire and didn't.** The reviewer built a fixture
designed to trip 8 of the 18 phrasings; that control also returned 0, which is the only reason the crash
surfaced. **A scan returning all zeros is indistinguishable from a crashing scan without a control that must
match.** The arc already holds "run the injection, and include a control that must pass"; this is the case
where the control was the whole of the detection.

**Related, same session:** `grep -o '\|'` returns 0 on every row because GNU BRE reads `\|` as alternation —
the implementer hit this independently and replaced it, and the reviewer reproduced it. A literal pipe count
requires `-oF`.
