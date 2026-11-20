# Measurement — session 6b-xviii (UIKIT layer batch)

**Every figure below was measured on 2026-10-09 in the main checkout at `57b7eba`, with the command
beside it.** Figures inherited from the 6b-xviii brief rather than re-derived are tagged
**ATTRIBUTED**. Where a brief or register figure was found wrong, the correction is stated with the
command that produced it.

---

## 1. Repository state at session start

| | Measured | Command |
|---|---|---|
| Branch | `chore/repo-audit` at `57b7eba` | `git rev-parse --short HEAD` |
| Ahead / behind origin | **0 / 0** | `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit` |
| `origin/chore/repo-audit` | `57b7eba` | `git rev-parse --short origin/chore/repo-audit` |
| `main` local / remote | `45495c1` / `45495c1` | `git rev-parse --short main`, then `origin/main`, run separately |
| Working tree | clean | `git status --porcelain` |
| `C:/dm-wt` | 1 entry, `6b-xvii`, at `4178924` on `dir/2026-10-09-core-layer-batch` | `git worktree list` |
| Session dirs (before this one) | 25 | `ls .dir-sdlc/sessions \| wc -l` |
| `.dir-sdlc/config.json` | absent — documented defaults apply | `ls .dir-sdlc/` |
| `.dir-sdlc/disciplines/` | absent — no project fragments | `ls .dir-sdlc/disciplines/` |
| Project-level `.dir-sdlc/log.md` | **absent.** Only `backlog.md` and `sessions/` exist | `ls .dir-sdlc/` |

### Correction 1 — the inherited ahead-of-origin figure was wrong in both handoff artifacts

`BRIEF-session-6b-xviii.md` states the branch is **1 ahead** of origin; the session handoff notes
state **2 ahead** and that "both unpushed commits are yours to send". Measured: **0 ahead / 0
behind**, and

```
git reflog show origin/chore/repo-audit | head -3
57b7eba refs/remotes/origin/chore/repo-audit@{0}: update by push
6acbb73 refs/remotes/origin/chore/repo-audit@{1}: update by push
00a093a refs/remotes/origin/chore/repo-audit@{2}: update by push
```

Both commits were pushed. There is nothing to send. Note the local `origin/*` refs were last
fetched 2026-10-07 (`ls -l .git/FETCH_HEAD`), so this is the state of the local remote-tracking ref;
it is the push entries in its reflog, not a fetch, that establish the commits left this machine.

---

## 2. The register, re-surveyed

| | Measured | Command |
|---|---|---|
| Total rows | **181** | `grep -cE '^\| [0-9]+ \| MASTER-[0-9]+ \|' docs/audit/00-master-index.md` |
| Closed | **141** | the survey below |
| Open | **35** | same |
| Part done | **5** — `MASTER-081`, `094`, `098`, `108`, `130` | same |
| Open severity | S4 **27**, S5 **8** | same, column 4 |
| Open effort | XS **18**, S **12**, M **3**, L **2** | same, column 7 |
| Open layers | UIKIT **9**, UI **8**, INFRA **6**, TEST **6**, CORE **4**, `UI, INFRA, TEST` **1**, `UI, XCUT` **1** | same, column 5 |
| Unclassified rows | **0** | the survey's own counter |

Every one of these reproduces the brief's figures exactly. The survey is one `awk -F'|'` over rows
matching `^\| [0-9]+ \| MASTER-[0-9]+ \|`, classifying column 11, and it carries an `unclassified`
counter that must read 0 for the totals to mean anything.

### Correction 2 — a substring classifier returns 143/33 instead of 141/35

A first survey using `s ~ /done|falsified|...|fixed/` returned **closed=143, open=33**. The status
cells carry multi-sentence prose, and three open rows contain the words "done" or "fixed" inside
that prose. Anchoring the predicate on the **leading token** of the cell fixes it:

```
if (s ~ /^part done/)                         -> part done   # test this FIRST
if (s ~ /^(\*\*)?open/ || s == "")            -> open
if (s ~ /^(\*\*)?(done|FALSIFIED|DELIBERATELY UNCHANGED|AS A DUPLICATE|filed found-and-fixed)/) -> closed
else                                          -> UNCLASSIFIED, printed
```

The three rows the substring form absorbed are the ones whose cells begin `open.`, `open.` and
`**open**`. **Test `part done` before `done`**, as the brief warns — and additionally, never
classify these cells by substring at all.

---

## 3. Line endings — the mechanism, and three instruments that lie about it

### The mechanism

| | Measured | Command |
|---|---|---|
| `core.autocrlf` | **true** | `git config core.autocrlf` |
| `.gitattributes` | present, `* text=auto` | `cat .gitattributes` |
| Working-tree `.cs` files in `DungeonMaster.App` | **58 pure-CRLF, 0 pure-LF, 0 mixed** | the census below |
| A sample blob vs its working-tree file | worktree **48** CR, blob **0** CR | `tr -dc '\r' < f \| wc -c` vs `git show HEAD:f \| tr -dc '\r' \| wc -c` |
| `.dir-sdlc/backlog.md` blob | **481** CR — the one tracked blob that is CRLF | `git show HEAD:.dir-sdlc/backlog.md \| tr -dc '\r' \| wc -c` |

So: **the entire working tree is CRLF and the blobs are LF**, by checkout conversion, with
`backlog.md` the single blob that escaped normalisation. This explains the inherited note that a raw
`sha256` of a worktree file always differs from its `git show` blob — it is not a quirk, it is
`core.autocrlf`. **`Edit` preserves CRLF; `Write` and `sed -i` emit LF and silently convert a whole
file.** That is the session's governing constraint.

### Correction 3 — `grep -c $'\r$' FILE` reports every line as CRLF

This is a **ninth** instrument for the inherited register of forms that return a plausible number
instead of an error, and it is the sharpest one yet because the brief's own backlog procedure
depends on measuring CRLF lines. Controlled against three files built for the purpose:

```
printf 'alpha\r\nbeta\r\ngamma\r\n' > pure_crlf.txt   # 20 bytes
printf 'alpha\nbeta\ngamma\n'       > pure_lf.txt     # 17 bytes
printf 'alpha\r\nbeta\ngamma\r\n'   > mixed.txt       # 19 bytes
```

| Instrument | pure CRLF | pure LF | mixed | Verdict |
|---|---|---|---|---|
| `grep -c $'\r$' FILE` | 3 | **3** | **3** | **WORTHLESS** — returns the line count |
| `grep -c $'\r' FILE` | 3 | **3** | **3** | **WORTHLESS** — same |
| `cat FILE \| grep -c $'\r$'` | 3 | **3** | **3** | **WORTHLESS** — piping does not save it |
| `tr -dc '\r' < FILE \| wc -c` | 3 | 0 | 2 | **CORRECT** — total CR |
| `cat -A FILE \| grep -c '\^M\$$'` | 3 | 0 | 2 | **CORRECT** — CRLF line count |

Expected is 3 / 0 / 2. The first three forms match the file's line count regardless of content,
which is consistent with the CR being stripped from both input and pattern, leaving `$` to match
every line. The same invocation also returned **473** for `backlog.md` in one call and **0** for it
in another, so it is not even self-consistent.

**It survived into the inherited procedure because it is right by luck on the one file that
procedure is about:** `backlog.md` genuinely has 473 lines and 473 CRLF lines, so returning the line
count returns the right answer there.

### Two false findings of my own, both from a lying instrument

- **"`AreaSidebarItem.xaml.cs` has mixed line endings."** WITHDRAWN. It arose from
  `sed -n '148,158p' FILE | cat -A`, and `sed -n` with a file argument strips CR before `cat -A`
  sees it — an instrument already in the inherited register. Measured correctly, the file is
  261 lines / 261 CRLF / 261 total CR: **fully CRLF**.
- **"Every file is 100% CRLF" from a 58-file census.** The census used
  `grep -c $'\r$'` and therefore reported `CRLF == lines` for every file — the signature of the bug
  above. Re-run with `cat -A | grep -c '\^M\$$'`, the conclusion happens to hold, but the first
  census measured nothing.

### Correction 4 — the brief's total-CR figure for `backlog.md`

The brief states `tr -dc '\r' | wc -c` "returns 461 here". Measured: **481**, both in the working
tree and in the blob at `95cc5d8`. The lone-CR figure the brief derives is nonetheless **correct**:

| | Measured | Command |
|---|---|---|
| Lines | 473 | `wc -l < .dir-sdlc/backlog.md` |
| CRLF lines | 473 | `cat -A .dir-sdlc/backlog.md \| grep -c '\^M\$$'` |
| Total CR | 481 | `tr -dc '\r' < .dir-sdlc/backlog.md \| wc -c` |
| **Lone CR** | **8** | 481 − 473 |
| Dated items | **158** | `grep -c '^- 20' .dir-sdlc/backlog.md` |

---

## 4. The eight selected rows, re-measured

The brief warns that 6b-xvii re-measured ten selected rows and seven carried at least one false or
stale specific. **Of these eight, five do** — two falsified outright, one half false, one with a
changed population, one overtaken to a fifth of its stated scope.

### Summary

| Row | Register claim | Measured | Verdict |
|---|---|---|---|
| `MASTER-121` | 12 braceless sites across 3 files | **10 sites across 2 files**; all 8 citations stale; one site uncited; one site uncompilable here | NARROWED |
| `MASTER-122` | `FloorMapDrawable` is space-indented + 4 doc locations | file is **0 space / 706 tab**; doc form live at **159 lines / 6 files**; 2 of 4 locations clean | **HALF FALSE** |
| `MASTER-123` | 4 public types in files named for another type | all 4 present, destinations absent | CONFIRMED |
| `MASTER-124` | blank-line deviations at 2 locations | **0** — both gone, defect class absent, and the row misreads its own authority | **FALSIFIED** |
| `MASTER-125` | redundant **empty** ctor, class has no other ctor, delete it | ctor is **not empty**, class has **3** ctors, **7** live call sites | **FALSIFIED** |
| `MASTER-136` | 8 locations lacking XML docs, effort **S** | **2 of 8** live; one cited member no longer exists | OVERTAKEN |
| `MASTER-148` | notch clamp permits the degenerate case, `:34-36` | exact, at `:34-36` | **CONFIRMED, citation exact** |
| `MASTER-151` | `DmConfirmButton` restates the base's visual-state group | confirmed at `:31-65` | CONFIRMED |

### `MASTER-121` — single-statement control flow omits braces

**Authority.** `dotnet-format.md` §4, read this session at `dm-conventions/backend/dotnet-format.md:152-180`:
"All control flow statements (`if`, `else`, `for`, `foreach`, `while`, `do`, `using`, `lock`) must
use full brace notation `{ }`, even when the body is a single statement. Never omit braces." The
"Incorrect" example is the braceless next-line form, so both the next-line form and the
same-line `if (x) return;` form violate it.

**All eight cited locations in `FloorMapDrawable.cs` are stale.** The row's `locations` block
assumes a 534-line file; the file is now **882** lines (`wc -l`), and not one of `:188-191`,
`:210-211`, `:219-220`, `:233-234`, `:288-289`, `:323-327`, `:434-435`, `:475-476` holds braceless
control flow. Re-located by content with a purpose-built detector:

| File | Braceless sites | Note |
|---|---|---|
| `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs` | **8** — `:208`, `:272`, `:279`, `:591`, `:647`, `:650`, `:758`, `:799` | |
| `src/DungeonMaster.App/Behaviors/HandCursorBehavior.cs` | **2** — `:67`, `:154` | see below |
| `src/DungeonMaster.App/Behaviors/CanvasLongPressDetector.cs` | **0** | row cites `:20-22`; **already clean** |
| `src/DungeonMaster.App/Behaviors/HoverOrLongPressBehavior.cs` | **0** | row cites `:60-62`; **already clean** |

**Two facts about `HandCursorBehavior.cs` that the row does not carry**, both from
`grep -nE '^#(if|elif|else|endif)'` on that file:

- **`:67` sits inside `#if MACCATALYST`** (the block runs `:62-107`).
- **`:154` sits inside `#if WINDOWS`** (`:151-161`), and **the row does not cite it.**

**CORRECTION 5 — my own, and it reached a dispatch before I caught it.** I initially recorded, and
told `dir-architect` in its decomposition dispatch, that `:67` "does not compile on this machine, so
an edit there cannot be verified by a build". **That is false.** I inferred it from the `#if` guard
and never ran the build. Measured:

| | Measured | Command |
|---|---|---|
| App target frameworks | `net10.0;net10.0-windows10.0.19041.0;net10.0-maccatalyst` | `grep -nE 'TargetFramework' src/DungeonMaster.App/DungeonMaster.App.csproj:9` |
| maccatalyst assembly produced | **yes**, 530944 bytes, written by this build | the build's own `DungeonMaster.App -> ...\net10.0-maccatalyst\maccatalyst-x64\DungeonMaster.App.dll` line, then `ls -l` |
| the guarded block's symbols | `UIKit.UIHoverGestureRecognizer` at `:63`, `:78`, `:82`, compiled clean | `grep -n 'UIHoverGestureRecognizer'` |

**`#if MACCATALYST` is compiled here and both sites are build-verifiable.** The correction was sent
to the architect mid-dispatch, withdrawing the constraint in full. This is the brief's named failure
mode — a figure carried between parties without recomputation — committed by the orchestrator inside
a dispatch that warns against it, and caught only because the baseline build printed the
maccatalyst target. **A `grep` for a preprocessor guard establishes that a guard exists; it
establishes nothing about whether the toolchain defines the symbol.**

One consequence worth keeping: because a solution build covers three TFMs for `DungeonMaster.App`,
"the build succeeds" is a stronger claim here than on a single-TFM project, and an edit that breaks
only the maccatalyst target will surface.

**Layer context, not part of the row:** the detector finds **37** braceless-or-same-line sites
across all 58 `.cs` files of `DungeonMaster.App`, of which **21 are in
`Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`** — the UI layer, outside this row's scope.

**The detector, and why its first version was wrong.** First version flagged
`if (context.NoDungeonIsLoaded())` as a same-line body: its tail test `\)[ \t]*[^ \t{]` matched the
**inner** closing paren of a nested call. The control file had no nested parentheses, so it passed
while being blind to the defect class — the instrument passed its control and was still wrong.
Corrected to anchor the tail test at end-of-header after stripping trailing `//` comments, and
re-controlled against a file containing three braced nested-paren forms that must **not** be
flagged and three braceless forms that must be: it returns exactly the three expected, and **0** on
an all-braced file including a braced `else`.

### `MASTER-122` — doc-comment body indentation, and the `MASTER-181` overlap

**Authority.** `dotnet-format.md` §9, `dm-conventions/backend/dotnet-format.md:370-418`: "Tag
descriptions go on a **new line**, indented with **two tabs** from the `///` marker", and "All
public types, members, and parameters must have XML documentation comments."

**The row's file-level indentation claim is false.** It states a census returning "429
space-indented lines and 0 tab-indented lines in `FloorMapDrawable.cs`". Measured:

| | Measured | Command |
|---|---|---|
| Total lines | 882 | `wc -l` |
| Leading-space lines | **0** | `grep -c '^ '` |
| Leading-tab lines | **706** | `grep -c "^$TAB"` with `TAB=$(printf '\t')` |

The file was converted to tabs at some point after the row was filed. **NOT MEASURED: which session
converted it.** That needs git archaeology this orchestrator did not run, and no row claims it.

**The doc-comment half is live, and it is the whole of `MASTER-181`.** Population of the space form,
`bin` and `obj` excluded **by path**:

| Lines | File | Layer |
|---|---|---|
| **116** | `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs` | UIKIT |
| 2 | `src/DungeonMaster.App/Assets/ObjectAssets/ObjectAssetCatalogue.cs` | UIKIT |
| 2 | `src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs` | UIKIT |
| **35** | `src/DungeonMaster.Infrastructure/Migrations/20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs` | INFRA — **a generated EF migration** |
| 1 | `src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs` | INFRA |
| 3 | `src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs` | TEST |
| **159** | **6 files** | **120 UIKIT / 36 INFRA / 3 TEST** |

`grep -cE '^[[:space:]]*/// {2,}'` per file over `find src -name '*.cs' | grep -vE '/(bin|obj)/'`.
A whole-tree `grep -rh` form returns **167** because the `bin`/`obj` filter was applied to the
matched text rather than to paths — 159 is the figure, 167 is the artefact.

**Predicate controlled in both directions.** A line with **one** space after `///` must not match
and does not; a line with **two** must and does. Two indent widths occur: **146** lines at 5 spaces
and **21** at 9 spaces (the nested form inside `<para>`).

**Two of the row's four cited locations are already clean**, and both live one directory deeper than
the row cites (`find` located them at `Controls/AreaSidebarItem/` and `Controls/FloorSidebarItem/`,
not `Controls/`): `AreaSidebarItem.xaml.cs` and `FloorSidebarItem.xaml.cs` return **0** for the
space predicate and **27** for the tab predicate (`^[[:space:]]*///${TAB}`) — the compliant form.

**The overlap.** `MASTER-181` (filed 2026-10-09 by 6b-xvii from `CORE-026`) names the same defect
under the same authority, and **116 of its 159 lines — 73% — are `FloorMapDrawable.cs`**, which is
`MASTER-122`'s own largest location. The user's Gate 1 decision is to **merge into `MASTER-122`**:
correct the false file-indentation claim, widen to the measured 6 files / 159 lines, and close
`MASTER-181` as a duplicate. The merged row spans **UIKIT, INFRA and TEST**, which is why
`MASTER-181`'s recorded layer of `UI, INFRA, TEST` is wrong in its first element rather than wholly
wrong.

**The tab predicate also needed a control.** `grep -nE '^[[:space:]]*///\t'` returned **0** on a
file known to contain `///<TAB><TAB>Gets or sets` — `\t` inside `grep -E` matches a literal `t`, an
instrument already in the inherited register. With `TAB=$(printf '\t')` the same predicate returns
**27**.

### `MASTER-123` — four public types declared in files named for a different type

**Authority.** `dotnet-format.md` §2, `:54-62`: "One type per file (class, record, interface,
enum)", "File name matches the type name exactly".

All four confirmed present, by
`grep -nE '^[[:space:]]*public (sealed )?(partial )?(class|record|enum|struct|interface)'`:

| Type | Declared at | Row cites |
|---|---|---|
| `NotebookKind` | `Controls/NotebookView/NotebookView.xaml.cs:8` | `:9-19` |
| `NotebookTarget` | same file `:32` | `:33-39` |
| `NotebookSaveArgs` | same file `:51` | `:52-58` |
| `BehaviorCursorShape` | `Behaviors/HandCursorBehavior.cs:4` | `:3-4` |

All four suggested destination files are **absent** (`NotebookKind.cs`, `NotebookTarget.cs`,
`NotebookSaveArgs.cs`, `BehaviorCursorShape.cs`), so none of this has been started.

**The brief's recommended mechanism does not apply to this row.** It advises reusing 6b-xvii's fix
for `MASTER-111` — "a byte-identical `git mv` committed alone so git records `R100`". That works for
a file whose single type is misnamed. Here each file keeps its own correctly-named type and the
extra types are **extracted** into new files. There is no rename for `git mv` to record and no
`R100` to produce.

**Layer context, not part of the row:** the same census across all 58 `.cs` files finds **14**
type-vs-filename mismatches. The other ten are in `ViewModels/` (8), `Assets/ObjectAssets/` (1) and
are outside UIKIT. The row's "four" is correct **for the two files it names**.

### `MASTER-124` — blank-line and trailing-line formatting — **FALSIFIED**

Both of the row's two locations are clean, and the row additionally misreads its authority.

**Location 1, `FloorSidebarItem.xaml.cs:255-259`.** Stale: `OnCardTapped` is now at **`:417`**
(`grep -n 'OnCardTapped'`), the file is 548 lines. Re-read by content, the method is:

```
:419  if (_suppressCardTap)
:420  {
:421      _suppressCardTap = false;
:422      return;
:423  }
:424  (blank)
:425  IsExpanded = true;
:426  SelectRequested?.Invoke(this, EventArgs.Empty);
```

**The blank line the row asks for is present at `:424`.** The defect is gone.

**Location 2, `AreaSidebarItem.xaml.cs:153-155`.** Stale: the file is now **261** lines and `:148-158`
is a documented property. Its final four lines are `:258 command.Execute(area);`, `:259 }`,
`:260 }`, `:261 }` — **no stray blank line before the type's closing brace.** The defect is gone.

**The defect class is absent from both files, not merely the two instances.** A detector for "a
line that is only a closing brace, immediately followed at the same indentation by a statement with
no blank line between" — controlled to return exactly 1 on a file with one such case and to skip a
blank-separated one — returns **0** on both files.

**And the row misreads §5.** It paraphrases the rule as requiring a blank line "before a statement
when ... the preceding statement spans multiple lines". §5 at `:180-238` actually requires one when
"the preceding statement spans multiple lines **due to method chaining or 2+ wrapped arguments**".
The closing brace of an `if` block is neither. The row's premise did not follow from its citation
even when it was filed.

### `MASTER-125` — redundant empty public constructor — **FALSIFIED, and its remedy is destructive**

The row states: "`public FloorMapDrawable() { }` declares a parameterless constructor identical to
the implicit one the compiler would emit, on a class with no other constructor", and directs
"Delete the line." Every clause is false now.

| Claim | Measured | Command |
|---|---|---|
| The ctor is empty | **FALSE** — it chains `: this(FloorMapPalette.FromTheme)` at `:133` | `grep -n -B6 -A3 'public FloorMapDrawable()'` |
| It is undocumented | **FALSE** — `:129-131` is a `<summary>`: "Initializes a new instance that reads its colours from `DmTheme`." | same |
| The class has no other constructor | **FALSE** — **three** ctors, at `:132`, `:148`, `:175` | `grep -n 'public FloorMapDrawable'` |
| It is dead | **FALSE** — **7 live call sites** | below |
| It is at `:23` | stale — it is at `:132` | same |

Call sites, from `grep -rn 'new FloorMapDrawable'` with `bin`/`obj` excluded: production at
`src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:63`, plus six
object-initializer uses in `DungeonMaster.App.Tests` (`FloorMapDrawableExecutionTests.cs:37` and
`FloorMapDrawableNonFiniteTests.cs:69, 87, 105, 123, 141`) — the `new FloorMapDrawable { ... }`
form requires a parameterless constructor.

**Following the row's suggested direction would break the build and six tests.** The row should be
closed as falsified, not implemented.

### `MASTER-136` — public types, members and parameters across the behaviors lack XML documentation

Rated effort **S** on 8 locations. **Two survive.** Measured with a detector for a `public` member
whose nearest preceding non-blank line is not a `///` line, controlled to flag an undocumented
member and not a documented one:

| Row's location | Measured | Verdict |
|---|---|---|
| `HoverOrLongPressBehavior.cs:3-13` — "documents nothing at all" | **42** `///` lines, **0** undocumented public members | documented since |
| `HoverOrLongPressBehavior.cs:15-43` | 0 undocumented | documented since |
| `CanvasLongPressDetector.cs:3-10` — "documents nothing" | **38** `///` lines, **0** undocumented | documented since |
| `CanvasLongPressDetector.cs:12-41` | 0 undocumented | documented since |
| `HandCursorBehavior.cs:14` — `Shape` is public and undocumented | **LIVE**, at exactly `:14` | **confirmed** |
| `FloorMapDrawable.cs:23` — public ctor undocumented | ctor is at `:132` and **is** documented | documented since; citation stale |
| `AreaSidebarItem.xaml.cs:134-137` — `SetEditing(bool)` has no `<param>` | **the method does not exist anywhere in `DungeonMaster.App`** (`grep -rn 'SetEditing'`) | gone |
| `FloorSidebarItem.xaml.cs:208-211` — `SetAreaEditing` has no `<param>` | **LIVE**, now at `:296`; `<summary>` at `:290-295`, no `<param>` tag | **confirmed** |

**Live scope: 2 of 8 locations** — `HandCursorBehavior.Shape` at `:14`, and
`FloorSidebarItem.SetAreaEditing` at `:296`. The row's effort rating of **S** no longer matches;
the remaining work is **XS**.

**Layer context, not part of the row:** a `<param>`-coverage detector over all 58 `.cs` files
reports **14** public members that take parameters and carry a doc block with no `<param>` tag.
**That figure is an upper bound and is NOT the genuine population:** at least two are false
positives where an expression body contains parentheses
(`FloorSidebarItem.ContainsKeyboardFocus => HasFocusedDescendant(this)`) or a generic event type
resembles a parameter list (`EventHandler<(Guid, int)>? ReorderRequested`). **NOT MEASURED: the
exact genuine figure.** Only `SetAreaEditing` is inside this row's scope either way.

### `MASTER-148` — `BookmarkTabShape`'s notch clamp — **CONFIRMED, citation exact**

**CORRECTION 7 — this line said "the only row of the eight whose line numbers survived unchanged",
and that is a FALSE UNIVERSAL. There are two.** `MASTER-151`'s sole `locations:` entry is
`Controls.Semantic.xaml:35-66`, and it is **exact**: `:35` is
`<Setter Property="VisualStateManager.VisualStateGroups">` and `:66` is its closing `</Setter>`,
measured on a file T8 left byte-identical to the base. **This file is why the exactness went
unnoticed** — section 4's `MASTER-151` entry recorded the group as `:31-65`, the Style element plus
the group, which is a different span from the one the row actually cites, so the row's own citation
was never compared against the tree. The false universal propagated into `manifest.json`'s frozen T9
criteria, which direct the register to record `MASTER-148`'s as "the ONLY one of the eight".
**T9 measured it, refused to write it, and recorded "one of only two" naming the other** — the
correct call, since a criterion asserting a measurable universal cannot override the measurement.

`MASTER-148` is one of **two** rows of the eight whose line numbers survived unchanged.

| | Measured | Command |
|---|---|---|
| Comment | `:34-35`, "Never let the notch swallow the whole ribbon" | `grep -n -B4 -A6 'Math.Min(NotchDepth'` |
| Clamp | `:36`, `var notch = (float)Math.Min(NotchDepth, bounds.Height);` | same |
| `NotchDepth` | `:16`, `public double NotchDepth { get; set; } = 10;` — a plain CLR property, **not** a `BindableProperty` | `grep -n 'NotchDepth'` |
| Consumers | **2**, both `NotebookView.xaml:195` and `:222`, both `NotchDepth="10"` | `grep -rn 'BookmarkTabShape\|NotchDepth' --include=*.xaml` |
| File | 48 lines | `wc -l` |

`Math.Min(NotchDepth, bounds.Height)` permits `notch == bounds.Height` exactly, which is the
degenerate case the comment at `:34-35` says it prevents. The row's own unreachability argument
holds: both consumers pass 10, and the property carries no binding.

**NOT MEASURED: the ribbon heights.** The row asserts they are "40 or 60 units tall". This
orchestrator did not confirm that, and the unreachability claim rests on it.

### `MASTER-151` — `DmConfirmButton` restates the visual-state group its base defines

| | Measured | Command |
|---|---|---|
| `DmConfirmButton` style | `Resources/Theme/Controls.Semantic.xaml:31`, `BasedOn="{StaticResource DmButtonBase}"` | `grep -n` over `:1-81` |
| Its own `CommonStates` group | `:35-65` — `Normal` `:38`, `PointerOver` `:45`, `Pressed` `:50`, `Disabled` `:57` | same |
| File | 81 lines, 81 CRLF | `wc -l`, `cat -A \| grep -c` |
| The file's own explanation | **`:6-29`** — the row cites `:10-16`. **CORRECTED at T8's review; this table said `:10-30`, which is wrong at both ends** | `grep -n '<!--\|-->'`, then `sed -n '5,7p;28,32p' \| cat -A` |
| `DmDestructiveButton`, `DmNeutralButton` | `:69`, `:75`, both `BasedOn` with **no** group of their own | same |

**CORRECTION 6 — this section's span for the comment block was wrong, and it propagated.** This table
originally recorded the block as `:10-30`. Measured at T8's review and re-measured by the
orchestrator: `<!--` opens at **`:6`** and `-->` closes at **`:29`**; `:10` is a prose line *inside*
the block and `:30` is blank, with the `DmConfirmButton` style opening at `:31`. The error reached
`manifest.json`'s frozen T8 criteria, which cite `:10-30` in `acceptance[0]` and `acceptance[3]` —
the architect carried this file's figure forward in good faith. **No verdict changed**, because no
criterion depends on the span's bounds and every substantive claim attributed to the block is present
at `:6-29`. It is corrected here as well as in the register so a future session's criteria do not
inherit it a second time. The frozen criteria's own ANCHOR RULE is what caught it: T9's `MASTER-151`
criterion says "re-derive it", and re-derivation is what produced `:6-29`.

Confirmed, and the duplication is **deliberate**: the file's comment block at `:6-29` states that a
derived style replaces the base group wholesale rather than merging, records the regression that
proved it, and warns in capitals that `DmDestructiveButton` must not gain a group of its own. The
row agrees. The finding is only that the duplication is invisible to the test suite.

**The remedy has a home, and the row names a stale owner.** The row says "T5 owns the theme suite" —
a task id from an earlier session. The suite is **`src/DungeonMaster.Tests/App/Theme/`**, where
**12** files read the theme XAML off disk, including `ThemeSource.cs` as the shared reader and
`ThemeDictionaryTests.cs`.

**CORRECTION 8 — the command this paragraph named cannot produce the figure beside it, and it is a
TENTH lying instrument, this one the orchestrator's own.** As originally written the command was
`grep -rln 'Controls.Semantic|Resources/Theme|ThemeDictionaries|VisualStateGroup'` — **with no
`-E`**. Without `-E`, `|` is a literal character, so the pattern matches one impossible string and
the command **returns 0**. T9 measured that, and measured the corrected form: with `-E` across the
four test projects it returns **13** post-change, hence **12** at the base. **So the figure of 12 is
correct and its stated command was inert** — the exact shape this file's own section 4 warns about,
committed in the act of warning about it. Verified by the orchestrator: as spelled **0**, with `-E`
**13**. The correct form is `grep -rlnE`
across the four test projects. **No test file currently references `Controls.Semantic` or
`DmConfirmButton`** by name, which is consistent with the row's claim that the duplication is
unguarded.

---

## 5. Instruments this session built, with their controls

Each lives in the session scratch directory and each passed a control that must match and a
near-miss that must not.

| Instrument | Finds | Control |
|---|---|---|
| `braceless.awk` | control-flow headers with no braced body, next-line or same-line | 3 expected hits on a mixed file containing 3 **braced nested-paren** forms that must not be flagged; **0** on an all-braced file including `else` |
| `undoc.awk` | `public` members whose nearest preceding non-blank line is not `///` | flags `Undoc()`, not `Doc()` |
| `noparam.awk` | parameterised public members whose doc block has no `<param>` | over-reports; at least 2 known false positives, see `MASTER-136` |
| `blankline.awk` | a lone closing brace followed at the same indent by a statement | exactly 1 on a file with one case, skipping a blank-separated one; **0** on both target files |
| `cat -A \| grep -c '\^M\$$'` | CRLF line count | 3 / 0 / 2 on purpose-built CRLF, LF and **mixed** files |
| `tr -dc '\r' < f \| wc -c` | total CR | 3 / 0 / 2 on the same three |

**The mixed file is what makes the last two trustworthy.** A two-file control of pure CRLF and pure
LF cannot distinguish a correct instrument from one that returns the line count for CRLF and zero
for LF. Three of the five line-ending forms tested pass a two-file control and fail the three-file
one.

---

## 6. Figures carried but NOT measured by this orchestrator

Stated here so nothing downstream reads them as this session's evidence.

- **Which session converted `FloorMapDrawable.cs` to tabs.** Needs git archaeology. No row claims it.
- **The exact genuine count of public members missing a `<param>` tag** across the layer. The
  detector's 14 is an upper bound with at least 2 known false positives.
- **The ribbon heights in `NotebookView.xaml`** that `MASTER-148`'s unreachability argument rests on.
- **The ribbon heights** — repeated here because `MASTER-148`'s unreachability argument rests on
  them and nothing in this session has confirmed them.

## 7. The baseline — MEASURED, no longer attributed

Both run by this orchestrator in the session worktree `C:/dm-wt/6b-xviii` at `57b7eba`, before any
task was dispatched.

| | Measured | Command |
|---|---|---|
| Build | **`Build succeeded.`, 0 Warning(s), 0 Error(s)**, 54.38s | `dotnet build src/DungeonMaster.sln` |
| TFMs built for `DungeonMaster.App` | **3** — `net10.0`, `net10.0-windows10.0.19041.0`, `net10.0-maccatalyst` | the build's own output lines |
| `DungeonMaster.Tests` | **897** passed, 0 failed, 0 skipped | `dotnet test src/DungeonMaster.sln` |
| `App.UnitTests` | **23** passed, 0 failed, 0 skipped | same run |
| `App.Tests` | **108** passed, 0 failed, 0 skipped | same run |
| **Total** | **1028 passed, 0 failed, 0 skipped** | same run |

These reproduce the brief's attributed figures exactly, and they are now this session's own
measurement rather than an inheritance. **The solution is `src/DungeonMaster.sln`** — the repository
root gives `MSB1009`.

**The TFM trap reproduced again.** `App.Tests` prints
`Passed! ... - DungeonMaster.App.Tests.dll (net10.0)` while its `Test run for` line reads
`...\bin\Debug\net10.0-windows10.0.19041.0\DungeonMaster.App.Tests.dll`. The `Passed!` line cannot
tell you which TFM ran; read the `Test run for` line. No cumulative count of this trap's occurrences
is asserted, because no session has kept one.
