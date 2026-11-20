# Handoff to session 6b-xiii

Written at the close of 6b-xii, 2026-09-29. Self-contained: everything 6b-xiii needs is here or at a
path named here. Nothing in it depends on the conversation that produced it.

**Every number below was re-derived from the repository immediately before writing, not carried from
the session that produced it.** Section 6 gives you the commands to re-run the survey rather than
trust the counts.

**A note on the slug.** This session's artifacts live under `2026-09-18-result-value-nullability` and
its branch carried that date, because Gate 1 ran on the 18th. The implementation and Gate 3 ran on the
**28th and 29th**. The slug is the session's identity and was not renamed; **do not date this work
from the slug.**

---

## 0. The one thing to carry forward, if you read nothing else

**Seven claims about what a command or an analyser catches were false this session, and every single
one was caught by RUNNING the thing rather than reading it.** Five were in frozen acceptance criteria,
one was in a criterion's supporting rationale, one was in an implementer's report. The orchestrator
verified each cited command at Gate 2 and **still missed five of them**, because the commands passed
while measuring the wrong property.

| # | The claim | What is actually true | Who caught it |
|---|---|---|---|
| 1 | A7: "a malformed `<see cref/>` raises CS1574, and `TreatWarningsAsErrors` fails the build" | **`GenerateDocumentationFile` is unset in every project**, so Roslyn never processes doc comments and CS1574 cannot fire. `dotnet build` cannot diagnose malformed XML doc markup anywhere in this repository | T1's implementer, by injection **plus a positive control** (`!IsSuccessNoSuchMember` -> `CS0103`, proving the build was not stale) |
| 2 | T2's report: "`IDE0055` would have failed B6 otherwise" (on blank lines) | **The Roslyn formatter has no rule about consecutive blank lines.** Inserting one leaves `0 Warning(s)`, exit 0. The config is not the gap -- `IDE0055` IS `severity = warning` with `EnforceCodeStyleInBuild` and `TreatWarningsAsErrors` on; there is simply nothing to hit | T2's reviewer, by reproducing it in its own scratch copy |
| 3 | C10's `grep "HaveCount(4)"` | Constrains **formatting, not behaviour**: this repo splits multi-argument calls one per line, so the natural form prints nothing. **Satisfying the grep cost the FluentAssertions reason string** the criterion existed to encourage | T3's implementer |
| 4 | C3's `grep -c "default(Guid)" >= 1` | Returns **2**, and one hit is **XML doc prose**, not the assertion. The grep cannot tell documentation from a test assertion | T3's implementer |
| 5 | C10's `HaveCount(4)` assertion itself | **Cannot fail.** `DetailViewModelSources()` projects a 4-element literal array and returns 4 or throws. And C10's cited vacuity mode (`AppSourceFiles` returning empty) **does not apply** -- the test never calls it | T3's reviewer |
| 6 | D8's rationale: "the only `Try` in the tree is the BCL call `Enum.TryParse`" | **False by 29.** `grep -rn "Try[A-Za-z]*("` returns **30** lines, and `DmInlineEditor.xaml.cs:275` declares `public void TryCommit()` -- a repo-declared `Try` member. D8's strict predicate (`bool Try…(`) is genuinely 0, so the decision survives; the universal wrapped around it did not | T4, which then **refused to carry the false universal into the audit record** |
| 7 | The 6b-xii handoff's baseline check, "all three TFMs appear" | `dotnet test`'s `Passed!` line prints `(net10.0)` for **all three** assemblies, including the one that demonstrably ran from `bin/Debug/net10.0-windows10.0.19041.0/`. **The check reads a string that cannot discriminate.** The TFM that ran is on the `Test run for ...` line | The orchestrator, measuring its own baseline |

**The pattern is one sentence: a criterion's prose asserts which tool catches what, and the assertion
is never itself measured.** Gate 2's consistency check cannot catch this class -- it asks whether a
criterion is satisfied at `startRef`, which is a different question from whether its cited command
measures the property the prose claims. **Defect 5 is the sharpest case: an assertion that cannot
fail, which no `startRef` check can see.**

**What to do about it in 6b-xiii:** when a criterion's prose names an analyser, a diagnostic ID, or a
tool behaviour, **require the dispatch to run it once before the freeze** -- not the falsifying
command, the *claim about the tool*. Six of the seven above cost nothing to check and every one was
wrong.

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`7a16852`**, the 6b-xii merge, plus the follow-on commits described
  below. **Re-derive the push state yourself** with
  `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit`; the 6b-ix handoff
  had to be corrected twice for exactly this, because it was true when written and false twenty
  minutes later.
- **`main` is untouched** at `45495c1`, local and remote identical. Nothing in 6b-xii went near it.
- 6b-xii merged as **`7a16852`**, a `--no-ff` merge with **exactly two parents**, `HEAD^1 = 42ec7e9`
  and `HEAD^2 = 6771204`, matching the shape of the thirteen session merges before it. **Parent count
  verified with `git cat-file -p HEAD | grep -c "^parent"`, not taken from `dir-git`'s report** --
  see section 4.
- The session branch carried **4 task commits**: `99c05c4` (T1, documentation), `4f66fe1` (T2,
  sentinel removed), `08d31cd` (T3, tests), `6771204` (T4, audit closure). Change surface over
  `42ec7e9..6771204`: **6 files changed, 337 insertions, 10 deletions**, from `git diff --numstat`.
- 6b-xii artifacts: `.dir-sdlc/sessions/2026-09-18-result-value-nullability/` -- `spec.md`,
  **`measurement.md`**, `manifest.json`, **`coverage.md`**, `disciplines/` (4), `reviews/T1.md`
  through `reviews/T4.md`, `log.md`, `state.json`, and this handoff. **`contracts/` is empty and
  absent from git** -- the session declared none, recorded as `"contracts": []` with the reason in
  `coverage.md`. That is not a missing artifact.
- **All 14 session artifacts verified `crlf == lf` with a final newline, in binary.** Zero
  non-uniform. That is an improvement on 6b-xi's ten-of-ten and 6b-x's five-of-eleven.

### Index arithmetic -- read this before citing any line number

| File | Lines at `42ec7e9` | Now | Offset rule |
|---|---|---|---|
| `docs/audit/00-master-index.md` | 687 | 687 | **Unchanged.** 6b-xii rewrote one status cell in place; no line added or removed (`1 insertion / 1 deletion`) |
| `docs/audit/01-domain-application.md` | 1604 | 1722 | **+115** inserted as one block after `:298`, then **+3** from the post-merge citation fix inside it. A citation at or below `:298` is unaffected; above it, add 118 |
| `.dir-sdlc/backlog.md` | 415 | 423 | **+8**, all appended at the end |
| `src/DungeonMaster.Application/Common/Results/ResultOfT.cs` | 131 | 145 | **+14**, all `///` lines above the `Value` declaration. **`Value` moved from `:29` to `:43`** |
| `src/DungeonMaster.App/ViewModels/MapCanvasViewModel.cs` | 462 | 457 | **-5** at `:213`. The surviving `addResult.Value` read moved from `:222` to `:217` |
| `docs/audit/05-tests.md` | 2229 | 2229 | Untouched by 6b-xii |

Counted with `b.count(b'\n')` in binary. **Do not apply the table and consider the citation
verified** -- it tells you where a line *moved*, not whether the claim at that line is still true.

---

## 2. What is open

179 entry rows: **130 closed, 46 open, 3 part done.** Zero rows return `UNCLASSIFIED`.

Open by layer -- `CORE` **15**, `UI` 9, `UIKIT` 9, `INFRA` 6, `TEST` 6, plus 1 spanning `UI, XCUT`.
By severity -- **S1 0, S2 0**, S3 **4**, S4 32, S5 10.

> **There are still no open S1 or S2 entries.** The severity argument remains spent; 6b-vii through
> 6b-xii all picked on coherence and layer concentration instead, and it has now worked six times.

The three `part done` entries, which **must not be marked done**: `MASTER-094` (`:282`),
`MASTER-098` (`:286`), `MASTER-108` (`:296`). Both inherited warnings still stand -- `MASTER-098` was
*measured* by 6b-vi and not closed, and **`MASTER-108`'s prose carries a wrong number**: it says
`DungeonDetailPage.xaml.cs` is "2,226 lines", which 6b-xi measured at **2,594**. **Neither entry was
touched by 6b-xii.**

**The four open S3s:**

| Line | Entry | Layer | Title (truncated) |
|---|---|---|---|
| `:235` | `MASTER-047` | CORE | Five Domain overloads have no production caller |
| `:239` | `MASTER-051` | UI | Entry and Editor handler mappers repeat eight WinUI resets |
| `:269` | `MASTER-081` | CORE | The Domain has no policy layer |
| `:295` | `MASTER-107` | CORE | `Dungeon` is a 453-line delegation facade |

`MASTER-044` is gone from this list -- closed by 6b-xii. **The S3 band fell from 5 to 4 and the CORE
layer from 16 to 15.**

---

## 3. The recommended session

This is a recommendation, not a decision -- 6b-xiii's Gate 1 owns the scope.

**Recommended: `MASTER-047` -- five Domain overloads with no production caller.** CORE, `dead-code`,
never re-measured by any session. It is the last S3 whose scope is plausibly small.

**The argument against it, at full strength.** Its "no production caller" claim is exactly the shape
this project's instruments get wrong. The backlog records that `dir-ect`'s own Gate 3
contract-operation search produced **two false `not called` verdicts in 6b-ix**, and 6b-xii declared
no contracts at all, so that search remains unexercised. **Re-measure by hand**, and note that four of
the five overloads are reachable from `DungeonMaster.Tests` via `InternalsVisibleTo`, so deletion
touches the test project and the suite is the blast radius.

**A specific warning, earned this session.** `MASTER-047`'s entry (`CORE-019`) cites six locations
with line numbers. **6b-xii found that four of `CORE-005`'s seven claims had gone stale without anyone
touching the entry** -- see section 4. `CORE-019` cites `PlacedObject.cs`, `FloorObjectMethods.cs` and
`DungeonMethods.cs`, none of which 6b-xii modified, but the ObjectScale work did touch that area.
**Measure before decomposing.**

### The alternatives, ranked

1. **The UI latency spike** (`backlog.md`, 2026-09-18). **A user-observed defect, not a code-reading
   one**, and still the only item in the register or backlog that came from running the application.
   Wants a profiler and a Release-build comparison; the user asked that a session decide whether it
   warrants its own `MASTER` entry. **Untouched by 6b-xii.**
2. **The four items 6b-xii filed**, section 5 -- particularly the neighbour-falsification process
   item, which is the only one that predicts future defects rather than describing a present one.
3. **`MASTER-107` -- `Dungeon` is a 453-line delegation facade.** Large, structural, wants its own
   session. **Re-measure the line count first**; its sibling's claim is known wrong by 368 lines.
4. **The `PathSegmentKind` migration, from `CORE-035`.** Still **the only item in the register
   requiring a data migration on persisted rows.** Scope it as a migration or not at all.

---

## 4. The inherited discipline -- this is the part that matters

**Re-measure every entry against a fixed commit before decomposing, and decompose from the
measurement.** 6b-xii is the strongest evidence yet for this rule, and it added a second one.

### The new rule: a fix to one entry silently falsifies its neighbours

`MASTER-044` had **never been measured by any session**. Four of its seven claims were stale or false
at `42ec7e9`:

| Claim | Reality |
|---|---|
| `Result<T>` is at `Result.cs:9` | It is at `ResultOfT.cs:9`; `Result.cs` holds the non-generic `Result` |
| `public T? Value { get; init; }` | Already `private init`, on both types |
| "fourteen of the sixteen dispatchable operations" | **4 of 16.** The two queries never used `Result<T>` at all -- `QueryAsync<TResult>` returns `Task<TResult>` |
| The sentinel is at `DungeonDetailViewModel.cs:302` | That file is **202 lines**. The survivor was `MapCanvasViewModel.cs:213` |

**None of that drift was caused by work on `MASTER-044`.** It was caused by **`CORE-006`, the entry
immediately below it**, whose own suggested direction -- a non-generic `Result` and `private init`
accessors -- was implemented. **A fix aimed at one entry falsified its neighbour, in prose, at lines
that never moved.** No survey, no line-offset table and no index arithmetic would flag it.

**This is now a filed backlog item, and it is the most valuable thing 6b-xii produced.** When you
close an entry, re-measure the entries that cite the same files or types.

### The session refused three chances to close on a weaker footing

1. **Gate 1 measured the entry before the user chose it**, and reported that half the complaint was
   already fixed. Closing it on the register's wording would have produced an amendment full of
   numbers that are no longer true.
2. **The fix shape was derived from repository evidence, not nominated.** `CORE-005`'s own headline
   suggestion -- a `TryGetValue`/`Match` accessor -- was rejected because **`n = 0`**: this repository
   declares no `Try`-pattern method anywhere. The architect reported three further counts **as single
   instances rather than conventions** (`n = 1` throwing members, `n = 1` generic constraints, `n = 1`
   for the `<see langword="default"/>` markup), which is the discipline 6b-xi's wrong survey bought.
3. **T4 inherited a false universal from its own dispatch, caught it, and refused to propagate it**
   into a permanent document -- writing the precise predicate and two narrowing facts instead.

### What the reviewers did beyond their briefs, for the sixth session running

- **T1's reviewer re-surveyed a claim rather than confirming it**, per project, found three apparent
  counter-examples and correctly discarded all three (`Task.FromResult<Stream>`, `IPopupResult<bool>`
  in a string literal, `Result<int>` in test fixtures only).
- **T2's reviewer disproved the implementer's `IDE0055` claim** by reproducing it, and correctly
  diagnosed that the config was not the gap.
- **T3's reviewer reproduced a red state WITHOUT mutating the tree**, running the detector regex over
  the pre-T2 blob. It also stress-tested that regex over **23 constructed forms** and listed **8 it
  misses**, plus one false-positive shape.
- **T4's reviewer proved D9 by reconstruction rather than by its grep**: removing the inserted lines
  yields a file byte-identical to `08d31cd`, which proves the entry body untouched -- the grep only
  proved one line survived.

### The irony worth remembering

**The session convened to fix stale line citations introduced two stale line citations into its own
correction record** -- `01-domain-application.md:318` said `Result.cs:16-19` holds `IsSuccess` /
`IsFailure` (they are at `:15` and `:20-21`; `:16-19` is a blank line and a doc comment), and `:366`
attributed the `new Areas.Area(...)` construction to `FloorMethods.cs:64` (it is at `:68-71`; `:64` is
the signature). **No criterion governed either**, because D3-D8 are satisfied by *string presence* --
the orchestrator declared that limit at Gate 2 as coverage row U3, and the reviewer is what caught it.
**Both were fixed post-merge by the user's decision, and the amendment now says so in its own text.**

### The environment findings

**`sed -i` remains suspended, and the harness RE-ISSUES the instruction mid-session.** Three
implementers and one reviewer independently reported a mid-run auto-mode reminder telling them to edit
with `sed`. All four refused, citing the dispatch. **This is not a one-time setup instruction -- it
must be restated in every dispatch, with the reason, because the harness will contradict you while the
agent is working.**

**No git-object read can witness a CRLF in this repository, and the mechanism is now measured.**
`.gitattributes` declares `* text=auto` and `core.autocrlf` is `true`, so blobs are stored **LF** and
converted on checkout. The base blob of `MapCanvasViewModel.cs` is **462 LF / 0 CRLF** while the
working file is uniformly CRLF. **The standing wording -- that `git show` "strips CR" -- is wrong and
should be retired: the CR was never in the object.** For line-*ending* questions these commands are
structurally incapable of a true answer; for line-*counting* they are sound, which is how 462 was
established.

**`GenerateDocumentationFile` is unset repo-wide**, so XML doc markup is unchecked by the build. The
user's Gate 3 decision was to record this and cite the reviewer's XML well-formedness parse as the
substitute evidence, **not** to enable it -- turning it on bare surfaces CS1591 across every
undocumented public member, and nobody has measured that blast radius.

**`AnalysisMode` is `Recommended` (`Directory.Build.props:15`), `TreatWarningsAsErrors=true` (`:5`),
`EnforceCodeStyleInBuild=true` (`:16`), `IDE0055` is `severity = warning` (`.editorconfig:64`).** All
re-verified. **But `IDE0055` does not enforce consecutive blank lines** -- measured this session.

**The `Write` tool still emits LF**, and this session's own edit scripts introduced one stray LF into
`log.md` -- a `\n` inside a non-raw Python string that became a real newline instead of the literal
text it was meant to display. **Caught by the same binary check everything else is caught by.** Also
worth knowing: a write-then-read inside one Python script can read a stale buffer; verify in a
**separate process**.

### `dir-git` -- six dispatches, and the errors are in its REPORTS again

Six dispatches: `init-session`, four `commit-task`, `merge-session`. **Every repository write was
correct. Every SHA was correct. No forbidden trailer was ever added. The merge has exactly two
parents.**

**Its reports were wrong twice, and its commit prose once:**

1. **The merge report INVENTED A PATH** -- `src/DungeonMaster.Core/Common/Results/ResultOfT.cs`. **No
   `DungeonMaster.Core` project exists** (there is `DungeonMaster.App.Core`); the file is in
   `DungeonMaster.Application`. A plausible-looking path that resolves to nothing.
2. **The merge report read `--stat`'s graph column as a file stat again** -- "2 changes", "16
   changes", "12 changes" against real numstats of `1/1`, `15/1`, `9/3`. **This is the identical error
   6b-xi caught.**
3. **T3's commit body carries a false claim**: that the detector "was shown to fail against the real
   tree before applying this change and **remains red after restoration**". It is **green**
   (`Failed: 0, Passed: 5`), and the real evidence is the opposite shape -- it reddened when the
   sentinel was *re-inserted*.

**The actionable finding, and it is new: the warning works.** In all four `commit-task` dispatches,
where the orchestrator explicitly warned against `--stat`'s graph column, **every number was right**.
The merge dispatch omitted that warning and it reverted immediately. **Put the `--numstat` warning in
every dispatch, not just the commit ones.**

**Its commit-message convention also had to be measured before it complied.** The first two commits
were subject-only and scoped by effort (`audit`), against a measured convention of **14 of 17
`src/`-touching commits carrying a body, median 14 lines**, all scoped by subsystem. Told that as a
*measurement*, it produced a 17-line body and `test(results):` on the next commit and kept it. **Its
brief already says to infer convention from history; it needed the history quantified.**

### The structural limits, unchanged

- **Tasks cannot invoke git.** 6b-xii made the audit closure a *task* (T4) rather than orchestrator
  work, which worked -- but T4 could not classify whether `DungeonDetailViewModel.cs:302` was wrong at
  authorship or the file shrank, because that needs history. **It said so in the record rather than
  guessing**, which is the correct behaviour.
- **No criterion may constrain what a task *reports*** -- only what the repository *contains*.
- **Do not write a criterion whose check requires git.** D11 originally did; it was caught at Gate 2
  and the clause removed pre-freeze.
- **`git diff` cannot see an untracked file.** T3's new test file was invisible; it was **named
  explicitly in the reviewer's dispatch**. `git add -N` is **not** the fix and must not be used.
- **A task may own a file solely as a temporary probe.** T3 injected into T2's committed file to prove
  its new fact load-bearing, and restored it byte-for-byte -- `sha256` and line count both verified,
  and `git status` no longer listed it. **Revert by re-editing content, never by restoring a copy.**
- **Re-supply the full criterion text on every re-review dispatch.**

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **423 lines** by `b.count(b'\n')`. 6b-xii appended **8 lines / 4 items**.

> **The 8 lone CR characters are still there and are still not from this session.** Measured before
> and after the append in binary: **8 both times**, and the append script fails hard if that number
> moves. They date to 6b-iv. **State which method you used** -- Python's text-mode iteration gives
> 431, because universal-newline handling splits on a lone CR too. 6b-xii deliberately did not repair
> them -- outside the approved scope, for the seventh session running.

**6b-xii's four items, all carried by explicit user decision at Gate 3:**

1. **Nothing re-measures an entry when its NEIGHBOUR changes** -- the process gap this session is the
   proof of. The highest-value item here: it predicts future stale entries rather than describing one.
2. **A vacuity guard that cannot fail** (`ResultSentinelIsAbsentTests.cs:82`), plus the same always-4
   shape in the pre-existing `FailureIsSurfacedTests.cs:210-217`. Worth one decision, not a per-test
   fix.
3. **A residual the new guard does not close** -- an emptied source file still counts 4 and flags
   nothing. Cross-covered today only incidentally.
4. **A universal in a durable artifact with nothing enforcing it** -- `ResultOfT.cs:38-39` says
   `Result<Guid>` is "the only closed construction this application uses". Measured true three ways
   today; nothing will say so when it stops being true, and the compiler never reads it.

**Still unscheduled from earlier sessions:** the UI latency spike; the discipline-versus-contract
sleep conflict and the clock seam; the `ObjectKindInfo` `None` absorption; the drop-placeholder
indicator design question; `AreaSidebarItem`'s two `PointerGestureRecognizer`s; the truncated floor
name with no recovery affordance; the Appium DIU-versus-pixel ratchets; the EF instrument that cannot
see a consistent namespace rename; the three 6b-iii items on `docs/audit/05-tests.md`; the
`with`-expression gap on `PathSegment`; the bare `else` in `ValidateControlPoints`; the count-only
assertion in `AreaOutlineTests`; the fifteen items 6b-ix filed; 6b-x's four; and 6b-xi's three.

**`dm-conventions` still teaches the pattern 6b-v removed** -- `backend/dotnet-format.md:567` and
`:588`. Separate repository, deliberately not modified.

**`.dir-sdlc/log.md` does not exist and never has**, across all 21 sessions. This project keeps
session-level logs only. Do not create it without deciding to.

---

## 6. Commands to re-derive section 2 yourself

Unchanged, and it still classifies every row. Re-run rather than trusting section 2.

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

**The column index is `[10]`**, because splitting a row on `|` yields a leading and a trailing empty
cell. **Re-run the survey after you edit the index, and again after the closure commit** -- 6b-xii did
both, and the post-edit run is what confirmed `130 / 46 / 3`.

---

## 7. Mechanics

- **Push status must be re-derived, not read here.** The push question is asked separately from
  Gate 3, as the framework requires.
- **Worktree root must be short.** Use `C:/dm-wt/6b-xiii`. This is a MAUI/WinUI solution and a long
  path breaks the WinUI XAML compiler with `MAX_PATH` errors; the scratchpad path is too long.
  `C:/dm-wt` is empty again after 6b-xii's cleanup.
- **Tracked files are CRLF and a formatting slip fails the build.** **Suspend the harness's `sed`
  instruction in every dispatch** -- and know that the harness will re-issue it mid-run.
- **Close any running `DungeonMaster.App` before building the Windows TFM**, or `MSB3026`/`MSB3027`.
- **Verify in the main checkout after merge**, not only in the worktree.
- **Tasks share one worktree and one `obj/`/`bin/`.** Two tasks that both build must be serialised.
  6b-xii ran **fully serial** (`T1 -> T2 -> T3 -> T4`), which was mandatory for T2/T3 on both shared
  build output and substance -- T3's new fact scans the file T2 edits.
- **The zero-warning check is command-specific.** `grep -icE "warning|error"` returns 0 over
  `dotnet test` output but **always 2** over `dotnet build`, because MSBuild prints `0 Warning(s)` and
  `0 Error(s)` literally. Over a build, use `grep -cE "(warning|error) [A-Z]+[0-9]+"`.
- **`dotnet test`'s `Passed!` line cannot tell you which TFM ran.** It prints `(net10.0)` for all
  three assemblies. Read the `Test run for ...` line instead.
- **To run the app for an observation pass:**
  `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`,
  then launch
  `src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`.
  **Check the binary is newer than every source file before observing.** **The app opens the user's
  REAL database** -- say so before asking for an observation pass.
- **This session's model tiers:** architect, implementers and reviewers on `claude-opus-5`;
  `dir-git` on `claude-haiku-4-5`. There is still no `.dir-sdlc/config.json`; those are the documented
  defaults, and each composed discipline is therefore its bundled base alone.
- **Gate 3 has no slot for orchestrator-authored repo content.** 6b-xii used the shape 6b-v through
  6b-xi used: merge (`7a16852`), then the `docs/audit/` citation fix, then `.dir-sdlc/`, then this
  handoff as its own commit.
- **Do the `.dir-sdlc/` appends BEFORE dispatching the commit that stages them.** 6b-xii did.
- **Metrics for 6b-xii:** `userInteractions` **3**, `escalationsRaised` **0**, `reviewRounds` **0**,
  `redispatches` **0**. The three are Gate 1, Gate 2 and Gate 3; **the push question makes four and is
  asked after the commit that stores this number**, the same ordering 6b-x and 6b-xi recorded.
  `reviewRounds` **0** is earned: **four tasks, four first-pass reviews, zero blocking findings**, and
  `state.json`'s per-task `revisionRounds` sums to it. `escalationsRaised` **0** is also real -- no
  question arose that the evidence ladder could not settle.
