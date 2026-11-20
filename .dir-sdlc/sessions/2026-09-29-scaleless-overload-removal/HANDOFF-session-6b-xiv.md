# Handoff to session 6b-xiv

Written at the close of 6b-xiii, 2026-09-29. Self-contained: everything 6b-xiv needs is here or at a
path named here. Nothing in it depends on the conversation that produced it.

**Every number below was re-derived from the repository immediately before writing, not carried from
the session that produced it.** Section 6 gives you the commands to re-run the survey rather than
trust the counts.

**A note on the slug.** This session's artifacts live under `2026-09-29-scaleless-overload-removal`
and Gate 1, implementation and Gate 3 all ran on the 29th, so the slug's date is honest for once.

---

## 0. The one thing to carry forward, if you read nothing else

**The rule you inherited is too narrow, and this session is the proof.** 6b-xii's rule was: *when a
criterion's prose names an analyser, a diagnostic ID, or a tool behaviour, run it before the freeze.*
Both of the sharpest false claims this session produced **name no tool at all**. One is a claim about
where code sits in a file; the other about how EF Core materializes an entity.

> **It is not "run the claim about the tool". It is: run ANY claim about a mechanism.**
> A line range is a claim about a file. A materialization path is a claim about a library. A
> convention is a claim about a document. None of them is a tool, and all three were false.

**And the second half, which is sharper and cost more:** *verifying a command ran is not verifying it
measured the property the prose claimed.* I ran every cited command at Gate 2 and still froze four
false claims, because each command returned output and I never asked what the output was about.

| # | The claim | What is actually true | Who caught it |
|---|---|---|---|
| 1 | `CORE-019` cites `DungeonMethods.cs:323-333` for `AddPlacedObject` | Holds `UpdateAreaOutline`. **Pre-existing drift**, caused by the ObjectScale work, not by anything done to `MASTER-047` | Gate 1's measurement, before the user chose the scope |
| 2 | `CORE-019` cites `PlacedObject.cs:62-63` for `Update` | `:62` is blank, `:63` the `Rotation` doc comment, `:64` the declaration. Also pre-existing drift | Gate 1's measurement; refined by T3, which derived it without git and **said it was a derivation** |
| 3 | A3's `grep -rE "<5 names>" src/DungeonMaster.Tests/ \| wc -l` returns 0 when done | Returns **9** at base — xunit method names live in compiled assembly metadata, so it matches four DLLs under `bin/` and `obj/`. **A5 and A6 force a build in the same worktree, so it could never have returned 0** however correct the edit | The orchestrator at Gate 2, by running it in the main checkout rather than the fresh worktree |
| 4 | A6: "grep returns 6 assembly-attribute lines" | **5** attribute lines; the grep returns 8 because 3 hits are inside XML doc comments | The orchestrator at Gate 2 |
| 5 | `measurement.md` §5.2: two deleted tests are "exact duplicates" | One is a strict **subset** — its twin asserts `Scale` as well | `dir-architect`, reading the files |
| 6 | `measurement.md` §5.3: "the `None` rejection survives" | The *rejection* survives; **one assertion does not**. Deleting the test loses the only `ActualValue == ObjectKind.None` assertion in the tree | `dir-architect` — became escalation E1 |
| 7 | `measurement.md` §5.4: `Constructor_SetsAllProperties` asserts five properties | **Six** | `dir-architect` |
| 8 | B1 and C2 locate the four-parameter `AddPlacedObject` at `:371-377` | **`:371-381`.** `:371-377` stops mid-expression, after `=> AddPlacedObject(` and `floorId,`. **I produced it by inferring a span's end from two grep hits instead of reading the range**, and C2 would have written it into the register | **T2's implementer**, which measured the range before deleting it — escalation E2 |
| 9 | B6: removing `PlacedObject`'s `Scale` field initializer "would silently give every entity materialized from the database a scale of 0" | **Completely inert.** EF invokes the parameterless ctor and **then assigns `Scale` from its mapped column.** The disproof sat in the repo the whole time at `PlacedObjectConfigurationTests.cs:48`, a `[Theory]` whose rows are `0.25f`/`2.5f`/`4.0f` — none of them the default | **T2's reviewer, by injection**: initializer removed, 975 passed — escalation E3 |
| 10 | C8: the no-hash convention is "stated explicitly in the MASTER-098 and MASTER-163 amendments" | **Neither states it.** `:286` mentions neither Gate 3 nor commit evidence; `:351` **carries** a hash (`c519781`) and states no convention. Its real home is `MASTER-164`'s narrative at `:623`. **The architect authored this and I relayed it into T3's dispatch unchecked** | **T3's reviewer** — and T3 had already written the false pointer into the register |
| 11 | A3's rationale: one deletion is "a literal duplicate ... once converted" | Loose rather than false. The survivor needed no conversion, and "literal" overstates a token difference (`ObjectScale.Default` vs `1.0f`, the same value) | T1's reviewer — **and I partly corrected the reviewer back**, because it read "once converted" as applying to the survivor |
| 12 | C1 and C3 each declare their full-path grep "returns exactly 1" | **Returns 1 at base and 2 when the work is done**, because sibling criterion C5 compels the strings to reappear verbatim. The criteria are unsatisfiable as literally written | **T3**, which flagged it and edited no criterion |
| 13 | The amendment block: "no `Update` overload of either arity was within ten lines of `:62-63`" | The two-parameter `Update` sat at `:70` — **7 lines** below `:63`. **In the block whose subject is unmeasured line-range claims, closing with a generalisation about exactly this error** | T3's reviewer. Carried to the backlog by user decision, so **it is in `chore/repo-audit` now** |
| 14 | The amendment block: `AddPlacedObject` "drifted **downward** by 48 lines, from the cited `:323` to the measured `:371`" | Arithmetic on a stale citation, not a measured displacement — `:323` was never a measured position, and nobody read the authorship commit | T3's reviewer. Also carried to the backlog |
| 15 | My own survey, written into `MASTER-047`'s cell: "21 carry a hash and 49 do not" | **22 / 48 — because writing that row's own hash moved it.** A figure about the register, recorded in the register, falsified by its own recording | **The orchestrator, immediately after writing it**; corrected in place with the self-reference stated |
| 16 | The 6b-xiii handoff's "the shape of the **thirteen** session merges before it" — which I had begun copying into this handoff as "fourteen" | **`chore/repo-audit` carries 29 merge commits, all 29 with exactly two parents; 22 merge a DIR session branch, so 21 precede 6b-xiii's.** The other 7 are pre-audit PR merges. Whatever "thirteen" counted, it was not this | **The orchestrator, while drafting this handoff** — by measuring a number it was about to inherit |

**Nine of these sixteen were mine**, a tenth I relayed without checking, and **the sixteenth I
inherited and nearly propagated.** That last one is the reason this table ends where it does: a
handoff is the highest-leverage place in this process for a wrong number to survive, because the next
session is *told* it was re-derived. **Item 16 was caught only because I ran the count instead of
copying the sentence around it.**

That is the number worth carrying, not the total: the orchestrator authors the measurement every task
decomposes from, so its unchecked claims propagate into frozen criteria and from there into permanent
documents. **Every one of the nine was caught by somebody running the thing.** None was caught by
re-reading.

**So the rule has a third clause, and it is aimed at this document:** a number you inherit is a claim
like any other. The header of this handoff promises every figure was re-derived. **Make that true by
re-deriving them, including the ones that look like background.**

**What to do about it in 6b-xiv:** before a claim about *any* mechanism enters `measurement.md` or a
criterion, run it once. And when you verify a criterion's cited command at Gate 2, **ask what
property the output describes**, not merely whether the command succeeds. Item 3 is the cheap
illustration: the command ran, returned a number, and the number was about `bin/` and `obj/`.

---

## 1. Where the repository stands

- Branch `chore/repo-audit` at **`8c08abc`**. **Re-derive the push state yourself** with
  `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit`; the 6b-ix handoff
  had to be corrected twice for exactly this.
- **`main` is untouched** at `45495c1`, local and remote identical. Nothing in 6b-xiii went near it.
- 6b-xiii merged as **`65542c6`**, a `--no-ff` merge with **exactly two parents**, `b39a65d` and
  `18db889`. **Parent count verified with `git cat-file -p`, not taken from `dir-git`'s report.**
  **The merge shape, measured rather than inherited:** `chore/repo-audit` carries **29 merge
  commits, and all 29 have exactly two parents** — checked one at a time, not sampled. **22 of them
  merge a DIR session branch, so 21 precede this one**; the other 7 are pre-audit
  `Merge pull request #1`–`#7`. See item 16 in section 0 — the number I first wrote here was
  inherited and wrong.
- The session branch carried **3 task commits**: `a116b2c` (T1, the 22 test call sites), `a9a5935`
  (T2, the five deletions), `18db889` (T3, the audit closure). Change surface over
  `b39a65d..18db889`: **12 files, 136 insertions, 193 deletions**, from `git diff --numstat`.
- Two post-merge commits followed, both mine: **`6cd9db0`** (the `MASTER-047` Status-cell
  correction, per `contradicts` CONTRA-2, plus its commit evidence) and **`8c08abc`** (the 13
  session artifacts and the 4-item backlog append).
- 6b-xiii artifacts: `.dir-sdlc/sessions/2026-09-29-scaleless-overload-removal/` — `spec.md`,
  **`measurement.md`**, `manifest.json`, **`coverage.md`**, `disciplines/` (4), `reviews/T1.md`
  through `reviews/T3.md`, `log.md`, `state.json`, and this handoff. **`contracts/` is empty and
  absent from git** — the session declared none, recorded as `"contracts": []` with the reason in
  `coverage.md`. That is not a missing artifact.
- **All 13 session artifacts verified `crlf == lf`, zero lone CR, with a final newline, in binary.**

### Index arithmetic — read this before citing any line number

| File | Lines at `b39a65d` | Now | Offset rule |
|---|---|---|---|
| `docs/audit/00-master-index.md` | 687 | 687 | **Unchanged.** Two Status-cell rewrites in place, no line added or removed |
| `docs/audit/01-domain-application.md` | 1722 | 1820 | **+98**, appended as one block after `:965`. A citation at or below `:965` is unaffected; above it, add 98 |
| `.dir-sdlc/backlog.md` | 423 | 431 | **+8**, all appended at the end |
| `src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs` | 519 | 489 | **−30** at `:353-381`. A citation above `:353` is unaffected |
| `src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs` | 157 | 112 | **−45**, in two blocks: `:7-14` and `:79-113` |
| `src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs` | 101 | 89 | **−12**, at `:12-18` and `:69-71`. **`Scale` moved from `:67` to `:59`** |
| `docs/audit/05-tests.md` | 2229 | 2229 | Untouched by 6b-xiii |

Counted with `b.count(b'\n')` in binary. **Do not apply the table and consider the citation
verified** — it tells you where a line *moved*, not whether the claim at that line is still true.
That distinction is what items 1, 2 and 8 of section 0 are all instances of.

---

## 2. What is open

179 entry rows: **131 closed, 45 open, 3 part done.** Zero rows return `UNCLASSIFIED`.

Open by layer — `CORE` **14**, `UI` 9, `UIKIT` 9, `INFRA` 6, `TEST` 6, plus 1 spanning `UI, XCUT`.
By severity — **S1 0, S2 0**, S3 **3**, S4 32, S5 10.

> **There are still no open S1 or S2 entries.** The severity argument remains spent; 6b-vii through
> 6b-xiii all picked on coherence and layer concentration instead, and it has now worked seven times.

The three `part done` entries, which **must not be marked done**: `MASTER-094` (`:282`),
`MASTER-098` (`:286`), `MASTER-108` (`:296`). Both inherited warnings still stand — `MASTER-098` was
*measured* by 6b-vi and not closed, and **`MASTER-108`'s prose carries a wrong number**: it says
`DungeonDetailPage.xaml.cs` is "2,226 lines", which 6b-xi measured at **2,594**. **Neither entry was
touched by 6b-xiii.**

**The three open S3s:**

| Line | Entry | Layer | Title (truncated) |
|---|---|---|---|
| `:239` | `MASTER-051` | UI | Entry and Editor handler mappers repeat eight WinUI resets |
| `:269` | `MASTER-081` | CORE | The Domain has no policy layer |
| `:295` | `MASTER-107` | CORE | `Dungeon` is a 453-line delegation facade |

`MASTER-047` is gone from this list — closed by 6b-xiii. **The S3 band fell from 4 to 3 and the CORE
layer from 15 to 14.**

---

## 3. The recommended session

This is a recommendation, not a decision — 6b-xiv's Gate 1 owns the scope.

**Recommended: `MASTER-107` — `Dungeon` is a 453-line delegation facade.** CORE, structural, and the
last S3 in the layer that has absorbed most of this stream's work.

**Re-measure the line count first, and expect it to be wrong.** Its sibling `MASTER-108`'s claim is
known wrong by 368 lines, and `DungeonMethods.cs` just lost 30 lines to 6b-xiii — so any figure in
that entry predates this session. **`DungeonMethods.cs` is 489 lines now**; whether the entry's "453"
ever described `Dungeon.cs`, `DungeonMethods.cs`, or the pair is itself unmeasured.

**The argument against it, at full strength.** It is the largest remaining structural entry, it
probably wants its own session rather than a slot in this stream, and a facade-decomposition touches
the aggregate every Application handler dispatches through. If Gate 1's measurement says the scope is
large, **say so and pick something else** rather than decomposing a session you cannot finish.

### The alternatives, ranked

1. **The UI latency spike** (`backlog.md`, 2026-09-18). **A user-observed defect, not a code-reading
   one**, and still the only item in the register or backlog that came from running the application.
   Wants a profiler and a Release-build comparison; the user asked that a session decide whether it
   warrants its own `MASTER` entry. **Untouched by 6b-xii and 6b-xiii.**
2. **The two false claims 6b-xiii left in `docs/audit/01-domain-application.md`** — `:993` and
   `:1000`, items 13 and 14 above. They are **in `chore/repo-audit` now**, they are small, and fixing
   them is the cheapest possible session. Fix them with an amendment note, not a silent edit.
3. **`MASTER-051`** — the Entry and Editor handler mappers repeating eight WinUI resets. UI, S3, and
   the only open S3 outside CORE.
4. **The `PathSegmentKind` migration, from `CORE-035`.** Still **the only item in the register
   requiring a data migration on persisted rows.** Scope it as a migration or not at all.

---

## 4. The inherited discipline — this is the part that matters

**Re-measure every entry against a fixed commit before decomposing, and decompose from the
measurement.** Seven sessions of evidence. 6b-xiii adds three refinements.

### Gate 2's consistency check has a blind spot, and it is now measured

The check reads each task's criteria as a set and asks whether any pair can both hold. **It does not
simulate the interaction between "this string must be ABSENT from the file" and "this string must be
QUOTED VERBATIM in the file".** C1/C3 versus C5 is exactly that pair, it reached dispatch, and I had
reported the check clean.

**A pair can contradict through the artifact they both address rather than through their prose.**
T3's reviewer named the shape better than I did: **sibling-criterion vacuity inverted.** The known
pattern is a sibling's landing making a criterion *satisfiable without the work*; this is a sibling's
landing making one **unsatisfiable**. Add the second direction to the check.

Its reasoning for ruling `met` anyway is worth reusing verbatim: *ruling `not met` would trigger a
revision round whose only available action is forbidden by a sibling criterion — a round that cannot
converge.*

### A count about a document, written into that document, can falsify itself

Item 15. I surveyed the register's hash convention, wrote the figure into a register row, and **the
act of writing that row's own hash moved the figure.** Nothing external changed. If you record a
measurement about an artifact *inside* that artifact, re-measure after writing and state the count
inclusive of your own edit.

### An entry's own suggested direction can be the least reliable thing in it

`CORE-019` said: "update the tests to pass `ObjectScale.Default` explicitly". Taken literally that
was **wrong for 7 of the 22 call sites** — seven tests were *named after the API being deleted*, and
appending an argument would have left them asserting that passing `2.0f` yields `2.0f`. **Five had to
be deleted and two converted and renamed.** The entry anticipated no deletions at all.

Gate 1 found this by reading the enclosing test of every call site rather than the call sites alone.
**That scan is cheap and it is what turned a mechanical edit into a three-way disposition.**

### What the reviewers did beyond their briefs, for the seventh session running

- **T1's reviewer distinguished test-side injection from production-side proof.** All four of T1's
  injections mutated *test* code — correctly, since the Domain files were T2's ownership — which
  proves an assertion reads the value it names but **not** that it would catch the production
  regression. It closed the gap by inspection on the two that mattered.
- **T2's reviewer falsified a frozen criterion's rationale by injection** (item 9), and its evidence
  is a single number: 975 passed with the initializer gone.
- **T3's reviewer found the false convention attribution** (item 10) by searching for the convention
  rather than confirming the rows exist — which is the distinction I failed to make at Gate 2.
- **T3's reviewer also corrected its implementer**, which cut the other way from every other finding:
  T3's C8/C11 "tension" was overstated, and the two criteria are compatible as written.

### Three grep forms that returned confident wrong numbers, all measured this session

- **`grep -r` over a test project matches compiled assemblies.** xunit method names live in DLL
  metadata. Scope with `--include=*.cs`. This one made a frozen criterion unsatisfiable (item 3).
- **`grep -c "` + "`" + `[Fact]` + "`" + `"` returns 0** against text literally containing that
  string, because `[Fact]` parses as a **character class**. Use `grep -cF`. It read as a missing
  criterion clause until measured.
- **`grep -c $'\r$'` returns 0 on a file whose last bytes are `0d0a`**, and the `^\t` leading-tab form
  returns 0 too. Both reconfirmed. **Use a byte-exact Python count for line endings.**

### The environment findings

**`sed -i` remains suspended, and the harness RE-ISSUES the instruction mid-session.** **Nine agents
in 6b-xiii independently reported it and all nine refused, citing their dispatch.** This is not a
one-time setup instruction — it must be restated in every dispatch, reviewers included, with the
reason.

**`GenerateDocumentationFile` is unset repo-wide**, so XML doc markup is unchecked by the build and
**CS1574 cannot fire**. Unchanged, and re-verified.

**`AnalysisMode` is `Recommended` (`Directory.Build.props:15`), `TreatWarningsAsErrors=true` (`:5`),
`EnforceCodeStyleInBuild=true` (`:16`), `IDE0055` is `severity = warning` (`.editorconfig:64`)** — and
**`IDE0055` does not enforce consecutive blank lines.** All re-verified.

**A missed call site after an overload deletion is `CS7036`, not `CS1501`.** Measured by the architect
compiling a scratch project rather than guessing. That is what makes a clean build the proof a
migration was complete.

**`DungeonMaster.App.Tests` can shed tests silently.** Under one of T2's reviewer's injections it
reported `Failed: 0, Passed: 81, Skipped: 0, Total: 81` — **21 short of 102, nothing failed or
skipped.** Rest a criterion on `Failed: 0` and treat that assembly's total as a prediction.

**The zero-warning check is command-specific.** `grep -icE "warning|error"` returns 0 over
`dotnet test` output but **always 2** over `dotnet build`. Over a build use
`grep -cE "(warning|error) [A-Z]+[0-9]+"`.

**`dotnet test`'s `Passed!` line cannot tell you which TFM ran.** It prints `(net10.0)` for all three
assemblies. Read the `Test run for ...` line. Reproduced again this session.

### `dir-git` — five dispatches, and the remedies that work are now two

Five dispatches: `init-session`, three `commit-task`, `merge-session`, plus a two-commit post-merge
dispatch. **Every repository write was correct. Every SHA was correct. No forbidden trailer was ever
added. The merge has exactly two parents.**

**Only one report or body was wrong, against two of six last session.** `a116b2c`'s body stated
"23 skipped, 102 inconclusive" where those are two assemblies' **passed** counts; nothing was skipped
and "inconclusive" is not a category the output reports. It was handed all three figures correctly.

**Two remedies, both now demonstrated:**

1. **The `--numstat` warning.** Carried in every dispatch this session, including the merge — and
   **the `--stat` graph-column error that recurred in 6b-xi and 6b-xii did not recur.**
2. **Name the figures and forbid relabelling.** After `a116b2c`, both later dispatches were told to
   read the numbers exactly as given; **both bodies came back accurate**, including the same suite
   figures it had mangled.

**Its commit-message convention still needs quantifying**, and quantifying it worked first time: told
that **39 of 40** `src/`-touching commits use `type(scope):`, **37 of 40** carry a body of **median 13
non-blank lines**, scopes are **subsystems**, and **only 1 of 40** carries `Co-Authored-By` /
`Claude-Session`, it produced conforming subjects and bodies on all three task commits — one of them
exactly 13 lines.

### The structural limits, unchanged

- **Tasks cannot invoke git.** T3 needed `b39a65d` to confirm a citation and could not read it; it
  **derived the answer two independent ways and recorded that it was a derivation.** That is the
  correct behaviour and it is now precedent.
- **No criterion may constrain what a task *reports*** — only what the repository *contains*.
- **Do not write a criterion whose check requires git.**
- **`git diff` cannot see an untracked file.** No task created one this session.
- **A criterion needing a figure a task cannot measure must have that figure handed over at
  dispatch.** C11 needed the post-change suite total; `coverage.md` recorded the obligation at Gate 2
  and the dispatch honoured it. Without that, C11 was UNDETERMINED by construction.
- **Revert by re-editing content, never by restoring a copy.** T2's reviewer made two injections and
  restored byte-identically — `sha256` and line count both verified.

---

## 5. Carried-forward work

`.dir-sdlc/backlog.md` holds **431 lines** by `b.count(b'\n')`. 6b-xiii appended **8 lines / 4 items**.

> **The 8 lone CR characters are still there and are still not from this session.** Measured before
> and after the append in binary: **8 both times**, and the append script fails hard if the number
> moves. They date to 6b-iv. **State which method you used** — Python's text-mode iteration gives a
> different total, because universal-newline handling splits on a lone CR too. 6b-xiii deliberately
> did not repair them — outside the approved scope, for the eighth session running.

**6b-xiii's four items, all carried by explicit user decision at Gate 3:**

1. **A false claim inside the correction record this session wrote**, at
   `01-domain-application.md:993` — "no `Update` overload of either arity was within ten lines of
   `:62-63`", when one sat 7 lines away. **In `chore/repo-audit` now**; whoever picks it up is editing
   a merged record. The highest-value item here, because it is the session's own subject turned on
   itself.
2. **An unmarked derivation presented as a measurement**, at `:1000` — the "48 lines downward" figure.
3. **A converted test's `Scale` assertion that cannot fail**
   (`DungeonAggregateTests.cs:451`) — and **the frozen criterion is what foreclosed the stronger
   form**, so this is not the implementer's error.
4. **`a116b2c`'s false body claim**, recorded because history is not rewritten here. Its actionable
   half is already proven — see the `dir-git` remedies above.

**Still unscheduled from earlier sessions:** the UI latency spike; the discipline-versus-contract
sleep conflict and the clock seam; the `ObjectKindInfo` `None` absorption; the drop-placeholder
indicator design question; `AreaSidebarItem`'s two `PointerGestureRecognizer`s; the truncated floor
name with no recovery affordance; the Appium DIU-versus-pixel ratchets; the EF instrument that cannot
see a consistent namespace rename; the three 6b-iii items on `docs/audit/05-tests.md`; the
`with`-expression gap on `PathSegment`; the bare `else` in `ValidateControlPoints`; the count-only
assertion in `AreaOutlineTests`; the fifteen items 6b-ix filed; 6b-x's four; 6b-xi's three; and
6b-xii's four — including **the neighbour-falsification process item**, which 6b-xiii is now the
second consecutive proof of.

**`dm-conventions` still teaches the pattern 6b-v removed** — `backend/dotnet-format.md:567` and
`:588`. Separate repository, deliberately not modified.

**`.dir-sdlc/log.md` does not exist and never has**, across all 22 sessions. This project keeps
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
cell. **Re-run the survey after you edit the index, and again after the closure commit** — 6b-xiii
did both, and the post-edit run is what caught item 15.

---

## 7. Mechanics

- **Push status must be re-derived, not read here.** The push question is asked separately from
  Gate 3, as the framework requires.
- **Worktree root must be short.** Use `C:/dm-wt/6b-xiv`. This is a MAUI/WinUI solution and a long
  path breaks the WinUI XAML compiler with `MAX_PATH` errors; the scratchpad path is too long.
  `C:/dm-wt` is empty again after 6b-xiii's cleanup.
- **Tracked files are CRLF and a formatting slip fails the build.** **Suspend the harness's `sed`
  instruction in every dispatch** — and know that the harness will re-issue it mid-run. Nine agents
  refused it this session.
- **Close any running `DungeonMaster.App` before building the Windows TFM**, or `MSB3026`/`MSB3027`.
  That is a file lock, not a test failure.
- **Verify in the main checkout after merge**, not only in the worktree. 6b-xiii did, and also
  byte-compared one file between the two after a harness note reported a line number that turned out
  to be a rendering artifact rather than the file's state.
- **Tasks share one worktree and one `obj/`/`bin/`.** 6b-xiii ran **fully serial**
  (`T1 -> T2 -> T3`), which was mandatory: T1 had to move the call sites before T2 could delete the
  overloads, and T3 needed T2's measured suite figures.
- **Suite baseline for 6b-xiv: 975 passed** — `DungeonMaster.Tests` **850**,
  `DungeonMaster.App.UnitTests` **23**, `DungeonMaster.App.Tests` **102** — 0 failed, 0 skipped,
  measured in the **main checkout** after the merge. Down from 980 at `b39a65d`; the difference is the
  five `[Fact]` methods T1 deleted, which is a deliberate reduction and not a regression.
- **To run the app for an observation pass:**
  `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0 -c Debug`,
  then launch
  `src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`.
  **Check the binary is newer than every source file before observing.** **The app opens the user's
  REAL database** — say so before asking for an observation pass.
- **This session's model tiers:** architect, implementers and reviewers on `claude-opus-5`;
  `dir-git` on `claude-haiku-4-5`. There is still no `.dir-sdlc/config.json`; those are the documented
  defaults, and each composed discipline is therefore its bundled base alone (42 / 35 / 34 / 37 lines).
- **Gate 3 has no slot for orchestrator-authored repo content.** 6b-xiii used the shape 6b-v through
  6b-xii used: merge (`65542c6`), then the `docs/audit` correction, then `.dir-sdlc/`, then this
  handoff as its own commit.
- **Do the `.dir-sdlc/` appends BEFORE dispatching the commit that stages them.** 6b-xiii did.
- **Metrics for 6b-xiii:** `userInteractions` **5**, `escalationsRaised` **3**, `reviewRounds` **0**,
  `redispatches` **0**. The five are Gate 1, Gate 2, escalation E2, escalation E3 and Gate 3; **the
  push question makes six and is asked after the commit that stores this number.** `reviewRounds` **0**
  is earned: **three tasks, three first-pass reviews, zero blocking findings**, and `state.json`'s
  per-task `revisionRounds` sums to it. **`escalationsRaised` 3 is the honest cost of the section 0
  table** — E1, E2 and E3 each exist because a claim in a frozen artifact turned out to be false, and
  two of the three were raised by the task that found mine.
- **4 amendments** to the frozen manifest, `A1` through `A4`, each authorised by an escalation and
  each performed by the orchestrator rather than an agent. `A4`'s `inFlight` is **`[]`** — the honest
  value at that instant, since T2's review was complete and T3 had never been dispatched.
