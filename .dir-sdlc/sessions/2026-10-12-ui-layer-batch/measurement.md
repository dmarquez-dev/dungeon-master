# measurement.md -- session 6b-xix (UI layer batch)

**Base commit `c9d9b11`, worktree `C:/dm-wt/6b-xix`. Every figure carries the command that produced it.**
Figures inherited rather than re-derived are tagged **ATTRIBUTED** with their source. Things not measured are
marked **NOT MEASURED** and are not guessed.

This file is the evidence base the acceptance criteria rest on. Its organising rule, inherited from 6b-xvii
and 6b-xviii and re-earned in this session's first measurement wave: **the register's specifics are leads,
not evidence.**

---

## 1. Inherited state, re-measured before Gate 1

| | Measured | Command |
|---|---|---|
| `chore/repo-audit` | `c9d9b11`, 0 ahead / 0 behind origin | `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit` |
| Publication | `c9d9b11 ... update by push` at `@{0}` | `git reflog show origin/chore/repo-audit` |
| `main` | `45495c1`, local and remote identical | `git rev-parse --short main`, then `origin/main` separately |
| Worktrees | 2 before this session: main checkout, and `C:/dm-wt/6b-xviii` at `77bbaae` | `git worktree list` |
| Session worktree | `C:/dm-wt/6b-xix` at `c9d9b11`, clean, created as a sibling | `git worktree list`; `git -C C:/dm-wt/6b-xix status --porcelain` empty |
| Register | 181 rows -- 150 closed, 26 open, 5 part done, **0 unclassified**, sum reconciles to 181 | one `awk -F'\|'` over `^\| [0-9]+ \| MASTER-[0-9]+ \|`, classifying column 11 on its **leading token** |
| Open layers | UI 8, INFRA 6, TEST 6, CORE 4, UIKIT 1, `UI, XCUT` 1 | same survey |
| Open severity / effort / risk | S4 20, S5 6 / XS 11, S 10, M 3, L 2 / low 20, medium 3, high 3 | same survey |
| `.dir-sdlc/backlog.md` | 487 lines, 495 total CR, 487 CRLF lines, **8 lone CR** | `wc -l`; `tr -dc '\r' \| wc -c`; `cat -A f \| grep -c '\^M\$$'`; lone CR = total CR minus CRLF lines |

**The CRLF instrument was controlled in both directions before being trusted**, in this environment:
a known-CRLF 2-line fixture returns **2** and a known-LF fixture returns **0** under the single-quoted
`cat -A f | grep -c '\^M\$$'`. The documented double-quoted form returns **0** on the same known-CRLF
fixture, reproducing the quoting hazard here rather than inheriting it as folklore.

---

## 2. Layer taxonomy: does `App.Core` have a layer of its own?

**No, and this is resolved from repository evidence at inference rung 1 -- it is not an escalation.**

The register uses 13 distinct layer strings and **none of them is `App.Core`**:
`awk -F'|'` over the row pattern, printing column 5, gives CORE 38, UIKIT 34, UI 34, TEST 33, INFRA 19,
XCUT 12, plus seven multi-layer combinations.

Precedent exists in both directions and both point the same way -- record the move, keep the filed layer:

- `docs/audit/03-app-shell-and-views.md:1393`, inside a **UI**-layer entry, already cites
  `src/DungeonMaster.App.Core/Map/MapDrawingSession.cs`.
- `MASTER-114`, filed under **CORE**, records one of its own citations as stale **by project** into
  `App.Core` and states at `docs/audit/01-domain-application.md:2313` that this is "outside the CORE layer
  this entry is" filed under -- without refiling the row.

**Consequence for this session:** `MASTER-119`'s file citation is stale **by project**, in the arc's own
established sense, and the row remains a UI row.

---

## 3. MASTER-118 / UI-010 -- the detail route literal

**Verdict: CONFIRMED in substance, one citation stale, remedy sound as written.**

| Clause | Verdict | Evidence |
|---|---|---|
| `AppShell.xaml.cs:16-18` | CONFIRMED | 16 `Routing.RegisterRoute(`, 17 `"dungeondetail",`, 18 `typeof(DungeonDetailPage));` |
| `DungeonListViewModel.cs:103` | **STALE -> `:107`** | cited line 103 holds the `<param>` description `The unique identifier of the dungeon to open.`; the construct is at `:107` |
| `AppShell.xaml:12-15` | CONFIRMED | `Route="DungeonListPage"` on `:15` |
| written twice | **STALE -- three textual occurrences, two functional** | the third is a doc comment, `Services/Navigation/INavigationService.cs:11` |
| once in `Routing.RegisterRoute`, once lower-cased and interpolated with a query string | CONFIRMED | `:17`; `await navigation.GoToAsync($"dungeondetail?id={id}")` at `:107` |
| `Route="DungeonListPage"` is a third name in a third place, different casing | CONFIRMED | `grep -rn 'Route="' src/` returns exactly one hit |
| `authority: judgment`, `citation: n/a` | CONFIRMED | **stated explicitly: no convention text supports or contradicts this row** |

**Re-derived by the orchestrator independently of the measuring agent**, with
`grep -rn '"dungeondetail' src --include=*.cs --include=*.xaml` excluding `/obj/` and `/bin/`:

```
src/DungeonMaster.App/AppShell.xaml.cs:17:                      "dungeondetail",
src/DungeonMaster.App/Services/Navigation/INavigationService.cs:11:     ///             a query, e.g. <c>"dungeondetail?id={guid}"</c>.
src/DungeonMaster.App/ViewModels/DungeonListViewModel.cs:107:           await navigation.GoToAsync($"dungeondetail?id={id}");
```

**Predicate partition, so the figure of 3 is reproducible.** The case-insensitive superset
`grep -rni "dungeondetail" src/` returns a much larger census dominated by the `DungeonDetailPage` and
`DungeonDetailViewModel` identifiers; the leading `"` is what excludes that near-miss class. Must-match
case `AppShell.xaml.cs:17` matches; near-miss `MauiProgram.cs:148 AddTransient<DungeonDetailViewModel>()`
does not.

**A population the entry's "What" never mentions, and which its remedy nonetheless covers.** The
query-parameter key `"id"` is duplicated across the same producer/consumer pair as the route name: produced
uninterpolated inside `:107`, consumed at `ViewModels/DungeonDetailViewModel.cs:115` in
`query.TryGetValue("id", out var val)`. Raw census for the quoted `"id"` over the App project is 7;
**n = 1** key literal, with six near-misses that are all `<param>` / `<paramref>` doc text
(`DungeonDetailViewModel.cs:162,165`, `DungeonListViewModel.cs:77,79,100,102`). The remedy's clause about
query-parameter keys is therefore independently validated rather than decorative.

**Remedy assessment.** Sound as written, and the only one of this wave's three that is. Three cautions:
`internal` is viable because both live sites are in `DungeonMaster.App`; XAML `Route="..."` **cannot** read a
C# constant without `{x:Static}` plus a namespace mapping, so a literal reading of "both the registration and
the navigation call" leaves `AppShell.xaml:15` untouched and the third-place clause surviving the fix; and
"settle a single casing convention" has **no authority behind it** -- Shell route matching is by exact string,
so changing the literal's value is a behavioural change to navigation rather than a formatting change.

---

## 4. MASTER-119 / UI-025 -- MapTool single-line summary comments

**Verdict: core defect CONFIRMED 6 of 6; file path stale by project; one authority gloss overstated.**

| Clause | Verdict | Evidence |
|---|---|---|
| `src/DungeonMaster.App/ViewModels/MapTool.cs` | **STALE BY PROJECT -> `src/DungeonMaster.App.Core/Editor/MapTool.cs`** | cited path does not exist; `find src -name 'MapTool*'` returns the App.Core path as sole hit. Verified by the orchestrator independently. Moved by `3d34a69 refactor(app): extract editor logic to App.Core layer`; originally added at the cited path by `3b52377` |
| span `8-24` | CONFIRMED at the new path | `:8` first member doc comment, `:24` the `Erase` member |
| all six members on a single line, one `///` marker carrying open tag, description and close | CONFIRMED, n = 6 | the interpolated-tab predicate returns 6, and its `-n` listing shows the six are the six members |
| `Draw` at `:8` representative, with an em dash | CONFIRMED | `cat -A` renders `M-bM-^@M-^T` = U+2014 |
| type-level summary at `3-5` already in the three-line form | CONFIRMED | `:4` is `///` followed by exactly two tabs |
| section 9 says descriptions go on a new line indented two tabs from the `///` marker | **CONFIRMED VERBATIM** | `dotnet-format.md:375` |
| "every worked example ... on three separate lines" | **STALE/overstated** | two of the three examples are three lines; the example at `:399-402` is **four**, because `:378` requires a break at each sentence boundary. Does not weaken the row -- all six MapTool descriptions are single sentences |

**The tab-in-`grep -E` hazard was reproduced live on this very file**, which is why every tab predicate above
uses the interpolated form: with `TAB=$(printf '\t')`, the interpolated predicate returns the type-summary
line, while the backslash-escaped form returns **0**. Same file, same intended predicate, 1 vs 0.

**Scope the row does not address.** The same validated predicate over all of `src/`, with `obj` and `bin`
excluded, returns **76** single-line summary comments across four projects -- `DungeonMaster.App` 35,
`App.Core` 9, `App.Tests` 7, `App.UiTests` 25. MapTool's 6 are a fraction of App.Core's 9. The row scopes
itself to one file; the convention it cites is project-wide. **Recorded as a population, not folded into the
row.**

**Remedy assessment.** Correct in shape, misleading in one mechanical detail that will bite. The remedy says
to use "the three-line form the file's own type comment already uses" -- but the type comment sits at column 0
(`///` plus 2 tabs) while the members sit one tab inside the enum body, so each member needs **three tabs of
leading whitespace before the description**, not two. Anyone pattern-matching the type comment under-indents
all six. Substitute the explicit tab count for the "form the type comment uses" wording.

---

## 5. MASTER-120 / UI-027 -- inline primary constructors: **FALSIFIED**

**This row cannot be executed as written, and following its remedy verbatim breaks the build.** It is the
second row in two sessions with that property, by the same mechanism as 6b-xviii's `MASTER-125`: the entry
quotes a declaration line that **does not exist in the tree** and then instructs an implementer to reformat it.

| Clause | Verdict | Evidence (orchestrator-verified) |
|---|---|---|
| `DungeonDetailViewModel.cs:27` | **FALSIFIED** | `:27` is the string argument `"No dungeon is loaded, so there is nothing to save. ..."` inside a static field initialiser |
| `DungeonDetailViewModel` has a primary constructor | **FALSIFIED** | `:20` is `public partial class DungeonDetailViewModel : ObservableObject, IQueryAttributable, IDungeonEditingContext` -- no parameter list. Its constructor is a **regular** one at `:47`, taking `AppDispatcher dispatcher, INavigationService navigation` |
| the entry's quoted declaration, with `(AppDispatcher dispatcher)` and two interfaces | **FALSIFIED -- this line is not in the tree** | real `:20` has no parameter list and carries a third interface |
| `DungeonListViewModel.cs:13` | **FALSIFIED** | `:13` is a doc comment |
| `DungeonListViewModel` is declared inline on one line | **FALSIFIED -- already wrapped** | `:15` opens the primary constructor, `:16` carries `AppDispatcher dispatcher,`, `:17` carries `INavigationService navigation) : ObservableObject` |
| `CreateDungeonViewModel.cs:11` | **STALE -> `:12`** | `:11` is the closing `</summary>`; declaration at `:12` |
| `CreateDungeonViewModel` is inline | **CONFIRMED** | `:12` -- **the only one of the three cited locations where the defect exists** |
| "all three view models" | **FALSIFIED** | 1 of 3 cited files exhibits it; real n = 4, three of them uncited |
| "the correct form for these three is a three-line declaration" | **FALSIFIED as stated** | true of `CreateDungeonViewModel` only; `DungeonListViewModel` is already correct; `DungeonDetailViewModel` has no primary constructor, so the form has no referent |
| section 3: primary constructor parameters always one per line regardless of count, with a one-parameter worked example | CONFIRMED VERBATIM | `dotnet-format.md:68`; example at `:73-76` |
| section 6: inheritance list on its own line at +1 tab from the parameters | CONFIRMED | `dotnet-format.md:240`, `:246`, summary table `:290` |
| **universal**: "every handler in the Application layer follows this shape" | **CONFIRMED, 16 of 16** | all 16 handler declarations in `src/DungeonMaster.Application` are in the wrapped form; corroborated by the opposite predicate returning 0 for that project |
| "so the App layer's view models are the outliers" | STALE/misleading | directionally true, but it misidentifies which: two cited "outliers" are not outliers, and two real outliers go unnamed |

**The real population, derived twice by two different predicates that agree.** The measuring agent used an
alternation-and-anchor form; the orchestrator independently used "declaration line containing both an open
paren and a close-paren-colon". Both return the same four sites in `DungeonMaster.App`:

```
ViewModels/CreateDungeonViewModel.cs:12   CreateDungeonViewModel(AppDispatcher dispatcher) : ObservableObject
ViewModels/DungeonHeaderViewModel.cs:19   DungeonHeaderViewModel(AppDispatcher dispatcher, IDungeonEditingContext context) : ObservableObject
ViewModels/FloorListViewModel.cs:24       FloorListViewModel(AppDispatcher dispatcher, IDungeonEditingContext context) : ObservableObject
ViewModels/MapCanvasViewModel.cs:34       MapCanvasViewModel(AppDispatcher dispatcher, IDungeonEditingContext context) : ObservableObject
```

plus **n = 1 outside the App project**, `src/DungeonMaster.App.Tests/Graphics/FakeAssetSource.cs:86`, a
private nested test helper with the same deviation.

**Three controls, run by the orchestrator, which is what licenses the zeros.** Positive: the predicate
returns 1 on `CreateDungeonViewModel.cs`, a known inline site. Negative 1: **0** on
`DungeonListViewModel.cs`, the wrapped-primary-constructor shape. Negative 2: **0** on
`DungeonDetailViewModel.cs`, the inline-base-list-without-primary-constructor shape. Both confusable shapes
that actually occur in this tree are discriminated, in both directions.

**Three of the four real sites are absent from the entry, and they are the worse ones** -- all three take two
parameters, which is where section 3's "regardless of count" bites hardest.

**Why following the remedy breaks the build.** An implementer who trusts the entry's quoted line and
reformats *it* into three lines writes a primary constructor that does not exist, drops the third interface
`IDungeonEditingContext`, and introduces a parameter `dispatcher` colliding with the private readonly
`_dispatcher` field at `:30` and the real constructor at `:47`. **NOT MEASURED as a compiler error** -- this
is a static argument from the name collision, not an observed build failure, because this wave was read-only.

**Deviations in the two falsified files that are real but belong to different rules**, recorded so they are
not lost and not silently folded into this row:

- `DungeonDetailViewModel.cs:20` -- base/interface list inline with three interfaces. Violates section 6
  `:240`, and is not a primary-constructor defect.
- `DungeonDetailViewModel.cs:47` -- two-parameter **regular** constructor inline. Violates section 3 `:70`,
  "one per line when there are 2 or more", a rule the entry never cites.
- `DungeonListViewModel.cs:17` -- base list shares the parameter line rather than sitting on its own at
  +2 tabs. Violates section 6 `:240`.

**Pattern note for this wave.** All three rows' citations into `dotnet-format.md` were accurate, while
**5 of the 7 cited source line numbers were wrong.** In this batch the register was reliable about authority
and unreliable about location. `MASTER-120` also inverts the arc's usual universal-failure pattern: its
universal is the one clause that survived testing while every specific collapsed.

---

## 6. MASTER-133 / UI-024 -- XML docs on three view types

**Verdict: the defect is real, but the entry mis-states it in four ways, one cited location does not exist,
and the layer-wide universal is false by 12 sites.**

| Clause | Verdict | Evidence (orchestrator-verified where marked) |
|---|---|---|
| `ObjectPaletteView.xaml.cs:7-21` | STALE | type declaration at `:7` is right; the span ends on the summary **opener** of the one documented event. Public surface runs `:7` to `:358` |
| `ObjectPaletteView.xaml.cs:372-382` | **FALSIFIED -- the lines do not exist** | **orchestrator-verified: the file is 370 lines** |
| `SecondaryPaletteView.xaml.cs:7-20` | STALE | type declaration at `:5`, not inside the span; public members `:7, :8, :10, :18`. Correct span `:5-18` |
| `ControlsPaletteView.xaml.cs:6-21` | CONFIRMED exact | type `:6`, last public member `ResetScale` `:21` |
| `ObjectPaletteView` has one public property | CONFIRMED | `ActiveObject` `:13` |
| ...five public events | **STALE -> six** | `:15, :16, :17, :18, :19` plus `LoadFailed` `:26`. The sixth is the one that **is** documented -- the entry appears to have counted the *undocumented* events and reported it as the event total |
| ...two public methods | CONFIRMED | `SelectObject` `:352`, `ClearActiveObject` `:358` |
| `SecondaryPaletteView`: two events, one method | CONFIRMED | `:7, :8`; `:18` |
| `ControlsPaletteView`: two events, two methods | CONFIRMED | `:8, :9`; `:20, :21` |
| all three carry **no doc comments at all** | **FALSIFIED for `ObjectPaletteView`**, CONFIRMED for the other two | **orchestrator-verified: a `grep -c` for the triple-slash marker returns 10 on `ObjectPaletteView.xaml.cs`**, 0 on the other two. Its two blocks are `:21-25` (`LoadFailed`) and `:275-279` (a private member) |
| universal: every **other** public type in this scope is documented | CONFIRMED for the folder | n = 5 public types in `Views/DungeonDetailPage/`; `DungeonDetailPage` and `ToolTileView` each return 0 undocumented sites from a predicate that returns 6 on `ControlsPaletteView` |
| "the only public API surfaces **in the layer** that do not comply" | **FALSIFIED** | **33** undocumented public sites across **9** files in `src/DungeonMaster.App`; **12 are outside the three cited views** |
| section 9 "opens with" the quoted sentence | CONFIRMED, byte-exact | heading `:370`, blank `:371`, sentence `:372` |

**The two zeros are controlled.** The doc-marker count returns 0 on `ControlsPaletteView.xaml.cs` and
`SecondaryPaletteView.xaml.cs`, while the same predicate returns 10 / 20 / 45 / 57 / 103 on the five other
files in the same folder. A zero with no control is indistinguishable from a blind predicate; these are not.

**The 12 sites that falsify the layer-wide universal** include `ObjectKindInfo` and `ObjectPackRegistry`,
both **`public static`** -- exactly the modifier form the carried backlog item warns its own predicate drops.
Also `ObjectAssetCatalogue.Paths:14`, which is itself a separate carried backlog item, and
`NotebookView.xaml.cs:85`.

**The compiler cannot catch any of this, re-confirmed at this base.** A recursive search for
`GenerateDocumentationFile` across `*.csproj`, `*.props` and `*.targets` exits 1 with no output, while the
control search for `TargetFramework` over the same file set returns **9** of the 10 build files that exist.
So the search reaches the build files and the absence is real: **a missing tag raises no warning, and a
malformed `cref` raises none either.**

**Two prose premises the row's remedy depends on, both tested.**

- **`ActiveObjectChanged` carrying `null` to mean cleared: TRUE, and it arrives by two paths, not one.**
  `:355` invokes with `kind`; `:361` (`ClearActiveObject`) and `:367` (`OnClosePalette`) both invoke with
  `null`, each after setting `ActiveObject = null`. The consumer reads it as cleared at
  `DungeonDetailPage.xaml.cs:1219`, `ControlsPalette.IsVisible = kind is not null`.
- **The `Populate` call-order requirement: REAL BUT SOFT, and this is the trap.** There is no throw and no
  guard. Skipping it renders a blank, untitled panel. **A doc comment saying `Populate` "must" be called, in
  a way implying an exception, would be false.** `Populate` also clears the grid's children and row
  definitions on entry, so it is re-entrant across repeat opens -- true, useful, and absent from the entry.
  Its sole caller is `ObjectPaletteView.xaml.cs:347`, two lines before the `SecondaryPaletteRequested`
  invoke at `:349`.

**This is the session's highest-exposure row, and the reason is structural.** 6b-xviii committed a
known-false sentence in a new XML summary and had to repair it at Gate 3, and review classified it
**advisory rather than blocking -- correctly, because no acceptance criterion covered whether a doc comment
was true.** This row's entire deliverable is new prose. **No command can verify that prose is true**, so a
criterion must give a named party authority over it, or the gap reproduces by construction.

**Remedy assessment.** Buildable, nothing breaks, but four corrections are needed before dispatch: the work
is **10** members not 11 and the event count is **six** not five; `ObjectPaletteView` is **not**
undocumented, so a fresh block per member risks clobbering the existing `LoadFailed` summary; the `Populate`
requirement must be worded as consequence rather than obligation; and the null-payload prose must name both
`:361` and `:367`. Section 9's rules `:376` and `:378` also bind the new prose -- complete sentences ending
in a period, and **never a sentence broken across two comment lines.**

---

## 7. MASTER-134 / UI-026 -- nested types: **FALSIFIED**

**Three of four factual claims are false, two of four cited locations do not exist, and one clause of the
remedy is impossible and would break the build and 26 tests.** Both constructs were moved out from under the
entry by `c5b5ccf refactor(app): split DungeonDetailViewModel into a composition root` and
`3d34a69 refactor(app): extract editor logic to App.Core layer`.

| Clause | Verdict | Evidence (orchestrator-verified where marked) |
|---|---|---|
| `DungeonDetailViewModel.cs:317` | **FALSIFIED -- line does not exist** | **orchestrator-verified: the file is 202 lines** |
| `DungeonDetailPage.xaml.cs:40-41` as an enum site | **FALSIFIED** | `:39` is a floor-drag comment, `:41` and `:42` are drag fields. No enum |
| `ObjectPaletteView.xaml.cs:333-335` | **STALE -> `:313-315`** | the three fields are at `:313, :314, :315`; `:333-335` is inside `OpenSecondaryPalette`'s body |
| `DungeonDetailViewModel.cs:75` | **FALSIFIED** | **orchestrator-verified: `_dungeonId` is declared at `:34`**; `:75` is inside the `ErrorMessage` property region |
| `PlaceObjectArgs` is a public record **nested inside `DungeonDetailViewModel`** | **FALSIFIED** | **orchestrator-verified: `src/DungeonMaster.App/ViewModels/MapCanvasViewModel.cs:428`, top-level** -- `cat -A` shows the line begins `public` with **no leading tab** -- in a different file. Nested-type census of `DungeonDetailViewModel.cs` returns 0 |
| ...referenced as `DungeonDetailViewModel.PlaceObjectArgs` | **FALSIFIED** | the page's real reference is **unqualified**, `DungeonDetailPage.xaml.cs:1206` |
| `DrawState` is an enum declared **mid-class in the page** | **FALSIFIED** | **orchestrator-verified: `src/DungeonMaster.App.Core/Editor/DrawState.cs:6`, a top-level `public enum` in its own correctly-named file in a different project**, fully documented. Nested-type census of the page returns 0 |
| three fields at `:333`, between two methods | count CONFIRMED, line STALE | three fields, `:313-315`, between `BuildExpanderButton`'s close `:311` and `OpenSecondaryPalette` `:317` |
| "320 lines below its other fields" | **not reproduced -- it is a subtraction** | both positions measured directly instead: other fields `:9` and `:10-11`; this block `:313-315` |
| `_dungeonId` declared **after all its properties** | **FALSIFIED** | fields at `:22, :26, :30, :32, :34`; constructor `:47`; first property `:67`. **The field block is already gathered at the top -- the state the remedy asks for already exists** |
| section 2 quotes | CONFIRMED byte-exact | `:56` one type per file, `:57` file name matches the type |

**The nested-type zeros are controlled.** The predicate returns 0 on both cited files and **non-zero** where
nested types genuinely exist -- `NotebookView.xaml.cs:17` `private enum NotebookPane` and `:80`
`private sealed class RibbonBrushes`.

**Which of the four locations section 2 actually covers: one.** `PlaceObjectArgs` breaches both `:56` and
`:57` -- though **not for the stated reason**: it is co-located in a differently-named file, not nested.
`DrawState` is **already compliant** on all three of section 2's rules. The `ObjectPaletteView` field
ordering and `_dungeonId` are **not section 2 matters at all** -- the entry's own hedge, "a readability cost
rather than a rule breach," is correct, and the section 2 citation does not belong on them.

**Blast radius, measured, with the near-miss partition.** `PlaceObjectArgs`: **6 hits, 3 in `src`** --
declaration `:428`, consumer `MapCanvasViewModel.cs:280`, and `DungeonDetailPage.xaml.cs:1206`. The other 3
are in a frozen 2026-08-27 session contract, not compiled. **Zero test references**, and that zero is
controlled: the same command finds 26 `DrawState` hits in
`src/DungeonMaster.Tests/AppCore/Editor/MapEditorStateTests.cs`, so it does reach test code.
**Extraction cost: 1 new file, 0 call-site edits** -- declaration and both usages share the namespace.

`DrawState`: **69 hits, 57 in `src`** -- 26 in `MapEditorStateTests.cs`, 19 in the page, 11 in
`MapEditorState.cs`, 1 at the declaration. **The 19 page hits are not 19 enum references**: several are the
*method* `ResetDrawState` or comment prose; actual qualified member references are 8 lines.

**Why one clause of the remedy breaks the build.** "Move `DrawState` to its own file **or leave it private**"
-- it is already in its own file, and `private` **is not expressible for a top-level type**. Narrowing it to
`internal` would break `DungeonMaster.App` and `DungeonMaster.Tests`, because it is consumed from a different
project and asserted on in **26** places. **This is the same shape as 6b-xviii's "Delete the line": an
instruction that reads as a one-liner and breaks the build and the tests.** The correct action is to strike
the clause as already satisfied.

**And extracting only `PlaceObjectArgs` makes the file less consistent, not more.** It sits with **six
sibling records** in `MapCanvasViewModel.cs` -- `RemoveAreaArgs:392`, `UpdateAreaArgs:403`,
`AddAreaWithOutlineArgs:410`, `MoveAreaOutlineArgs:418`, `RemovePlacedObjectArgs:436`,
`UpdatePlacedObjectArgs:452`. Extracting one leaves six identical section 2 breaches in the same file.
Either extract all seven or none.

**Three corrections to the carried type-vs-filename backlog item**, which this row overlaps:

- the population at this base is **11** by a modifier-agnostic predicate and **10** by the backlog's own
  gapped one -- not 14. The item's 14 was measured before `MASTER-123` closed the 4 UIKIT rows.
- **"8 are in `ViewModels/`" is stale -> 9.**
- **the gap the item names is not the gap that bites.** It warns about `public static` and `public abstract`;
  both `public static` types in the layer match their filenames and contribute **zero** mismatches, and there
  are no `public abstract` types. The modifier that actually hides a mismatch is **`readonly`**, in
  `FloorMapDrawable.cs:40` `public readonly record struct FloorMapPalette(`.

`PlaceObjectArgs` **is** inside that population; `DrawState` **is not** and never could be, because its
filename matches its type.

**The measuring agent corrected one of its own figures in place and disclosed it**: its first census took
`basename -s .cs`, so `Foo.xaml.cs` became `Foo.xaml` and every XAML code-behind partial was a false
positive, giving 30 and 31 instead of 10 and 11.

---

## 8. MASTER-141 / UI-032 -- layout literals in token-rich markup (risk=medium)

**Verdict: the diagnosis survives measurement intact in every particular. Its coordinates and its dependency
do not.**

**Authority, stated explicitly:** `judgment` / `citation: n/a`. There is no convention text to verify, so
**every clause rests on the entry author's judgment rather than a rule**, and nothing here can be adjudicated
as a convention violation.

| Clause | Verdict | Evidence |
|---|---|---|
| `Padding="16,12,16,0"` exists | CONFIRMED, single hit solution-wide | `DungeonDetailPage.xaml:64` |
| ...at `:34` | **STALE -- drift, offset +30** | `:34` is prose inside a comment about the back-button behaviour |
| a `300` sidebar column exists | CONFIRMED | `ColumnDefinitions="*,300"` at `:186` |
| ...at `:137` | **STALE -- drift, offset +49** | |
| `Margin="32,158,0,0"` on the notebook | CONFIRMED | `:746`, last attribute of the `NotebookView` |
| ...at `:638` | **STALE -- drift, offset +108** | |
| `ObjectPaletteView.xaml:12` = `220`, `:52` = `280`, `ControlsPaletteView.xaml:34` = `220`, `SecondaryPaletteView.xaml:10` = `272` | **all four CONFIRMED exactly** | the four palette citations are the accurate ones |
| all are bare numbers, not `StaticResource` | CONFIRMED | each sits among four `StaticResource` attributes |
| universal: colours from tokens | CONFIRMED 4/4 | 26 / 6 / 16 / 4 |
| universal: spacings from tokens | CONFIRMED 4/4 | `DmSpace` 8 / 2 / 4 / 3 |
| universal: radii from tokens | CONFIRMED 4/4 | `DmRadius` 9 / 1 / 5 / 1 |
| universal: stroke weights from tokens | **CONFIRMED with a named qualification** | `DmStroke` 4 / 1 / 5 / 1. The only bare non-zero values are `StrokeThickness="1.25"` `:393` and `"1.45"` `:426` -- **both inside the entry's own zoom carve-out**. Every other bare value is `StrokeThickness="0"`, which is "no stroke", not a weight choice |
| universal: font sizes from tokens | CONFIRMED 4/4 | `DmFontSize` 8 / 2 / 3 / 1, and **zero** bare numeric `FontSize` in all four files |
| zoom carve-out span `333-378` | **STALE -- start correct, end short** | `:333` opens the stepper stack correctly; `:378` is mid-attribute. The real end is **`:431`**, cited by content, the closing tag of that stack |
| a derivation comment is actually there | CONFIRMED | `:340-372`, a 33-line block |
| the notebook margin's comment calls it an approximation | CONFIRMED | `:735-737`, the word is "approximates" |
| the two `220`s are coincidence, nothing links them | CONFIRMED | exactly two text hits for `220` in the folder and the theme directory, plus three binary false hits in a `.png` and two `.ttf` files. No shared token, no cross-reference, no `Style` |

**The counter-census is what makes the five-part universal meaningful**, since a universal can only be
falsified by finding a bare literal. Raw census of bare numeric layout attributes and hex colours across the
four files returns **16 lines; the figure that matters is 2.** The fourteen near-misses, enumerated rather
than asserted: six `StrokeThickness="0"`, five `CharacterSpacing` (letter-spacing, not one of the five named
categories), two zero-valued `Spacing`/`RowSpacing`, and `:64` itself matching on its own `RowSpacing="0"`.
**Zero hex colours anywhere.** The two real exceptions are the carved-out zoom thicknesses.

**The three stale citations were correct when written, and this is drift rather than carelessness.** Walking
the file's history, at `50e3572` ("chore(theme): close out the design-system cleanup", 2026-08-12) all three
read exactly **34 / 137 / 638**. **But the three offsets differ -- +30, +49, +108 -- because insertions
landed between the anchors, so this is NOT a constant-offset drift and cannot be corrected by one shift.**

**The token-set finding, which decides whether this row is workable -- orchestrator-verified.**

The `Dm*` token set exists and is substantial: **127 unique `x:Key="Dm*"` definitions** across
`src/DungeonMaster.App/Resources/Theme/`, verified at their definition sites rather than by markup
references. Layout-related families: `DmRadius*` 3, `DmStroke*` 3, `DmSpace*` 6, `DmPadding*` 6 (Thickness),
`DmFontSize*` 12, `DmIconSize*` 6 (12-22), and `DmTouchTargetMinimum` (44).

**But there is no width, column, panel, sidebar or palette token of any kind.** Filtering the 127 keys for
those name shapes returns **nothing**, and that zero is controlled: the same pipeline returns `DmSpace` 6,
`DmRadius` 3, `DmStroke` 3, `DmFontSize` 12, `DmIconSize` 6.

So the conclusion is neither "dimension tokens are absent" nor "the names exist". **Dimension tokens exist as
a glyph-scale family of 12-22 plus one accessibility floor of 44, and nothing covers the values this row
needs -- 220, 220, 272, 280, 300, and the composite `32,158`.** Those belong to a different semantic family,
panel and container geometry, which does not exist. Promoting a width means **opening a new token family**:
choosing its naming scheme and deciding whether `220`/`272` are one token with an override or two. **That is
materially more than editing seven attributes, and it is a plausible explanation for `risk=medium` against
an `S` effort.** The register's `T4` dependency is therefore **real but misdescribed**: the token *set*
exists; the token *names this row needs* do not. `T4` belongs to an earlier, already-closed session of this
arc, so there is no live counterparty to agree names with and the naming decision falls to this session.

**Remedy assessment: would not produce a correct result as written**, for three independent reasons -- the
token names do not exist; three of seven citations are stale by non-uniform offsets, so an implementer
following them literally edits comment prose at `:34` and `:137`; and the carve-out span ends 53 lines early,
so an implementer treating `:379-431` as fair game would see `StrokeThickness="1.25"`/`"1.45"` and a
`42`/`36` stepper cell as unprotected -- when the comment at `:364-371` says in terms that "correcting" those
weights is wrong.

---

## 9. MASTER-154 / UI-035 -- the unwired Start Session button (S5, dead-code)

**Verdict: every substantive claim about the LIST page is confirmed, including the exact line 95 and the
Hint's wording. Three of six clauses about the DETAIL page are overstated, and its cited span is the wrong
region. The "no action needed" judgment is corroborated by a FROZEN CONTRACT from an earlier session of this
same programme.**

**The row's own "Suggested direction" is "No action needed."** Two prior briefs flagged this row as "possibly
a feature decision rather than a cleanup" without either testing the claim. It has now been tested.

| Clause | Verdict | Evidence (orchestrator-verified where marked) |
|---|---|---|
| detail span `DungeonDetailPage.xaml:69-96` | **FALSIFIED -- wrong region, not drift** | **orchestrator-verified: `:69` is back-button prose and `:96` is a `<Label`.** The Start Session construct runs **`:106-154`**, with `Text="Start Session"` at `:117` and the `AutomationId` at `:118`. Start +37, end +58, so not a constant offset |
| list span `DungeonListPage.xaml:72-111` | CONFIRMED | comment `:72-85`, `Grid` `:86`, `Button` `:87-111`. It stops one line before the disabled-tooltip overlay `:113-117` |
| detail button has an `AutomationId` | CONFIRMED | `:118` |
| detail button has a tooltip | **FALSIFIED on the button** | the only tooltip is on the overlay at `:152`, visible *only when disabled* |
| detail button has semantic properties | **FALSIFIED, and deliberately so** | the comment at `:111-114` states the omission is intentional |
| detail button has a `CommandParameter` binding | **FALSIFIED** | **orchestrator-verified: `CommandParameter` appears on exactly 4 XAML lines solution-wide, ALL in `DungeonListPage.xaml`** -- `:61` Edit, `:77` prose, `:95` Start Session, `:129` Delete. **The detail page has none** |
| detail button has a `DataTrigger` dimming it | CONFIRMED | `:130-135`, `Opacity` to `0.35` on `FloorCount` |
| detail button has a disabled-state tooltip overlay | CONFIRMED | `:149-153` |
| list button: `AutomationId`, tooltip, semantic properties, `CommandParameter`, `DataTrigger`, overlay | **all six CONFIRMED** | `:96`, `:99`, `:97-98`, `:95`, `:104-109`, `:113-117` |
| **neither carries `Clicked` or `Command`** | **CONFIRMED, controlled** | see below |
| both annotated as deliberately unwired | CONFIRMED | detail `:106-115`, list `:72-85`, both quoted in the agent report |
| both cross-reference each other | CONFIRMED, **plus a third the entry does not mention** | `Implicit.xaml:222-226` names both sites |
| list `SemanticProperties.Hint` says the button performs no action | **CONFIRMED verbatim** | `:98` -- "Sessions are not available yet; this button performs no action." |
| `CommandParameter="{Binding Id}"` is on line **95** | **CONFIRMED exactly** | raw census is 3; the two near-misses are Edit `:61` and Delete `:129` |
| the binding path `Id` resolves against the item type | CONFIRMED | `x:DataType="dto:DungeonSummaryDto"` at `:42`, `ObservableCollection<DungeonSummaryDto>`, and `Id` is the record's first positional member -- a **compiled** binding, so it must and does resolve |

**The load-bearing zero, controlled by the orchestrator.** Using a predicate that distinguishes `Command=`
from `CommandParameter=` without `\b` -- a shell-quoted `\b` becomes a literal backspace byte and would have
returned 0 while still reporting a number:

| Run | Result |
|---|---|
| detail button span `:116-136` | **0** |
| list button span `:87-111` | **0** |
| **control**, whole `DungeonDetailPage.xaml` | **10** |
| **control**, whole `DungeonListPage.xaml` | **3** |

The control is decisive because it finds `Clicked` on the sibling Edit and Delete buttons **in the same
`DataTemplate`, one line below their own `CommandParameter`s** -- so the predicate is reading the right
region and discriminating the right token. A second controlled pair of zeros rules out wiring by gesture
recognizer on a wrapper: 0 inside both spans against 13 and 6 on the whole files.

**Solution-wide search: this is an UNBUILT feature, not "wired elsewhere and these two sites are stale."**
A case-insensitive recursive search for `start session` in any spelling across `*.cs`, `*.xaml`, `*.csproj`
and `*.json`, excluding `obj` and `bin`, returns **13 lines and zero executable wiring** -- 7 in the two
markup files under test, 2 in an `Implicit.xaml` comment, 1 in a `DmCard.xaml.cs` comment, and 3 in a prior
DIR session record. **Positive control:** the same machinery with the pattern `dungeon` returns 362 files, so
the search reaches the solution and the zero is not a path or include-filter artifact. Corroborating
negatives, each run separately: **no session route** (the only registered route is `dungeondetail`), **no
feature-flag infrastructure anywhere in the solution**, and **no test asserts on the `AutomationId`**.

**The finding that settles the disposition -- orchestrator-verified against the artifact.**
`.dir-sdlc/sessions/2026-08-28-the-control-kit/manifest.json`, task `T1`, acceptance criterion 5, states as
a **frozen contract invariant**:

> "`AutomationId="EditDungeonButton"`, `"StartSessionButton"` and `"DeleteDungeonButton"` keep those exact
> strings; and the Start-Session button stays unwired with the comment at `DungeonListPage.xaml:72-85`
> intact and unsoftened. If the fix makes that button visible for the first time, its
> `SemanticProperties.Hint` -- which says sessions are not available yet -- becomes user-visible and must
> still be true."

**That span is still byte-exact**: `:72` opens "Start Session button." and `:85` closes "the same time as the
handler, not before. -->". So the deliberate-unwired state is not merely annotated in a comment -- **it was
ratified as a frozen invariant by an earlier session of this same audit programme**, and the entry's judgment
that the handling is "genuinely good" is independently corroborated by that record.

**Remedy assessment.** The suggested direction is "No action needed", so the conditional narrow action is
what gets assessed: *"consider dropping the two `CommandParameter` bindings."*

**That action is not executable as written: there is one, not two.** The detail page has no
`CommandParameter` at all, so an implementer would find nothing to delete there. Dropping `:95` alone is
safe in behaviour -- nothing reads it, the `DataTrigger` and the overlay both bind `FloorCount`, and
`Implicit.xaml` references the `DataTrigger` rather than the parameter -- but **NOT MEASURED as a
compilation fact**, because the binding is compiled (`x:DataType` is set) and no build was run.

**Three findings neither prior brief surfaced:**

1. **`:61` and `:129` are the same defect and the entry does not mention them.** The Edit and Delete buttons
   each carry `CommandParameter="{Binding Id}"` *and* a `Clicked=` handler one line below. A `Clicked`
   handler receives no `CommandParameter`, so **all three `CommandParameter`s in this `DataTemplate` are
   inert.** By the entry's own stated rationale -- that the parameter misleads a reader into assuming a
   `Command` exists -- `:61` and `:129` mislead **more**, because their buttons visibly work and carry no
   warning comment. Scoping the action to `:95` alone would leave the two more misleading instances three
   lines above and below.
2. **The `AutomationId` string is contractually frozen**, per the invariant quoted above.
3. **The `DataTrigger`s are load-bearing and must not be swept up.** `Implicit.xaml:217-226` deliberately
   omits a `Disabled` `VisualState` *because* three unstyled buttons set their own `Opacity 0.35`, and a
   `VisualState` setter would outrank both a `DataTrigger` and a local value. Removing either `DataTrigger`
   as "part of the dead markup" would silently lose the disabled dimming with no compensating style.

**NOT MEASURED, and not inferred.** Both comments assert that clicking was "confirmed at runtime" to do
nothing. **That is not re-measured here** -- no build and no app were run. The source evidence is strong and
four-way controlled, but **the absence of four source constructs is not an observation of runtime
behaviour.** This is the same discipline 6b-xviii's maccatalyst error violated: a grep for a preprocessor
guard established that a guard existed and nothing about whether the toolchain defined the symbol.

---

## 10. MASTER-135 / UI-028 -- braceless control flow: **EVERY CLAUSE STALE, AND THE ROW IS THE SESSION'S BEST OPPORTUNITY**

**Verdict: the entry was exact when written and is now stale in every single clause. Of its 15 cited spans,
ZERO point at a braceless site at `c9d9b11`.** Ten have drifted by a **non-constant** offset ranging **+47 to
+354**, and five were eliminated by refactors. **And yet the row's substantive claim -- "roughly twenty
places" -- is CONFIRMED at 21, and the population reconciles exactly.**

**Why every number moved.** The entry was authored at `b2a7383`. Running the detector against that commit's
blobs reproduces **every cited line number almost exactly**. `DungeonDetailPage.xaml.cs` was **2226** lines
then and is **2594** now; `DungeonDetailViewModel.cs` was **397** then and is **202** now. `git show` is used
for **content only** here -- blobs are LF-normalised, so nothing in this section claims anything about line
endings.

### The 15 cited spans

| Cited span | Verdict at `c9d9b11` | Real location / evidence |
|---|---|---|
| `DungeonDetailViewModel.cs:302-303` | **FALSIFIED** | **the file is 202 lines.** Exact at `b2a7383`; the construct is gone from `src/` entirely. **Zero braceless sites in this file now** |
| `DungeonDetailPage.xaml.cs:308-321` | **STALE, drift +47** | held 3 sites when written; now **`:355/356`, `:364/365`, `:367/368`**. **Orchestrator-verified: `:308` today is `protected override void OnHandlerChanged()` followed by a braced `if`, and `:355-356` is the braceless `if (Navigation?.ModalStack.Count > 0) / return false;`** |
| `:326-329` | **STALE, drift +74** | now **`:400-403`**. Today's `:326-329` is a closing brace, a divider comment and a method signature -- **no control flow at all** |
| `:393-395` | **STALE, drift +74** | now **`:468-469`**. The span also started one line early when written |
| `:401-406` | **STALE, drift +79** | now **`:482-485`**. Also started two lines early when written |
| `:419-420` | **STALE, drift +79** | now **`:498-499`** |
| `:443-444` | **FALSIFIED -- site eliminated** | removed by `3c22647 refactor(app): extract the shared WinUI handler-mapper body to one method` |
| `:464-465` | **FALSIFIED -- site eliminated** | same construct, same refactor |
| `:1159` | **STALE, drift +153** | now **`:1312`** |
| `:1213-1223` | **STALE, drift +150** | now **`:1364-1373`** |
| `:1741-1757` | **FALSIFIED -- both sites eliminated** | held 2 sites when written, including the `PolygonArea` loop |
| `:1765-1766` | **FALSIFIED -- site eliminated** | the five surviving guards of that shape at `:1930, :2021, :2057, :2093, :2123` are **all braced** |
| `:2037-2038` | **STALE, drift +135** | now **`:2172-2173`** |
| `:2073-2074` | **STALE, drift +135** | now **`:2208-2209`** |
| `:2109-2117` | **STALE, drift +354** | held 3 sites; now **`:2463-2470`** |

**The drift is not uniform: +47, +74, +74, +79, +79, +153, +150, +135, +135, +354. A single constant-offset
remap would be wrong at 9 of the 10 surviving spans.** This is the anchor rule's whole point -- every site
must be relocated by content search, never by arithmetic.

**The mapping reconciles arithmetically.** The surviving cited sites total **20**, plus **one site the
register never cited** -- audit-era `:350/351`, now `:424/425` -- gives **21**, exactly the measured count in
the file. So the 15 spans covered 25 of the 26 sites present when written, and one was **missed at authoring
time**, falling between two spans.

### The remaining clauses

| Clause | Verdict | Evidence |
|---|---|---|
| "single-statement `if`, `else` and `for` bodies" | **CONFIRMED for `if` and `else`; FALSIFIED for `for`** | all 21 current sites are `if` (18) or `else` (3). **There is no unbraced `for` anywhere in `DungeonMaster.App`** |
| "roughly twenty places" | **CONFIRMED** | 21 in the file, 27 project-wide |
| the densest site is a **five-branch** `if/else if` chain | **CONFIRMED** | **orchestrator-verified at `:1364-1373`: five branches, every body unbraced** |
| ...in which every branch is an unbraced **`_ =` discard** | **FALSIFIED at `c9d9b11`** (true at `b2a7383`) | **orchestrator-verified: every body is now `await ...`, not `_ = ...`.** The two remaining `_ =` occurrences in the file are both lambda parameters, neither in the chain |
| `PolygonArea` at `:1756-1757` with an unbraced `for` | **FALSIFIED** | **orchestrator-verified: `PolygonArea` does not exist in `src/`** -- the sole hit is a *test method name*. Its successors live in `App.Core/Map/AreaOutlineGeometry.cs` and **both relocated `for` loops are braced**, with the census as positive control: the detector sees 8 control-flow sites in that file and finds all 8 braced |
| universal: "the same file uses full braces everywhere else" | **CONFIRMED as a ratio for this file; FALSIFIED as a project-wide generalisation** | `DungeonDetailPage.xaml.cs` is **155 braced of 176 (88%)** -- genuinely local. But `ObjectKindInfo.cs` is **3 of 3, 100% braceless**, and is outside the row's cited locations |
| "mechanical and behaviour-preserving" | **CONFIRMED for all 27 sites** | see below |
| authority, section 4 | **CONFIRMED verbatim, and NO carve-out exists** | `dotnet-format.md:154` is the entire normative text and contains no "unless", "except", "may", or `.editorconfig` escape. The prohibition is unconditional |

### The detector, and why it had to be built rather than grepped

The previous session found **two independent routes** to the same false negative on nested-paren headers.
The detector written for this pass **masks** strings, chars and comments, then finds the **matching** close
paren by a depth walk across newlines -- neither route's shortcut. It was shown non-blind **in both
directions**:

- On a fixture with a multi-line nested-paren braceless header beside an **identical correctly-braced**
  header: the detector reports exactly the one defect and **not** its braced twin, while **both naive routes
  return zero.**
- On real code: `ObjectPaletteView.xaml.cs:33` is itself a live instance of the defect class --
  `foreach (var (_, displayName) in ObjectPackRegistry.AvailablePacks)`. Naive route 2 reports it with the
  wrong class **and false-positives line 217**.
- On the row's main file the three implementations disagree: **mine 21, route 1 fifteen, route 2 thirty-two.**

**The agent corrected one of its own figures in place and named the mechanism correctly rather than letting
the bigger number stand on a wrong cause**: route 1's 15-vs-21 gap is **not** its arithmetic but six
`else`/`else if` forms its keyword regex never covered; its arithmetic failure is isolated separately on the
fixture, where it returns 0 against a true 1.

**The figure is a TOTAL, not a floor and not an upper bound.** Every form the detector does not handle was
measured to have **zero** occurrences in the layer: raw string literals 0, and `do(`, `while(`, `lock(`,
`fixed(`, `using(` each 0. `switch`/`catch` are excluded because the grammar already forces a block.

### The population, verified twice

**`.cs` file count: 62**, agreed by `find` on the working tree and `git ls-tree -r` on `c9d9b11`
-- **orchestrator-verified both ways.** The backlog's "now 62" is confirmed.

**Total: 27 sites. Orchestrator-verified by TWO independently-built predicates that agree exactly**, which is
the standard this arc holds a load-bearing count to:

| File | Sites |
|---|---|
| `Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` | **21** |
| `Assets/ObjectAssets/ObjectKindInfo.cs` | **3** |
| `Assets/ObjectAssets/ObjectPackRegistry.cs` | **2** |
| `Views/DungeonDetailPage/ObjectPaletteView.xaml.cs` | **1** |

The orchestrator's own predicate was built differently -- "control-flow header whose next line does not open
a brace" -- and **skipped 17 multi-line headers by construction, yet still returned 27 with the identical
per-file split.** That agreement independently establishes that **none of the 17 multi-line headers is
braceless**, which is the one thing a next-line predicate cannot see and the detector can.

**The reconciliation against the backlog is exact.** Running the same detector against 6b-xviii's base
`57b7eba` returns **58 files and 37 sites**, with 21 in `DungeonDetailPage.xaml.cs`, 8 in `FloorMapDrawable.cs`
and 2 in `HandCursorBehavior.cs`. **37 - 10 = 27**, and the 10 `MASTER-121` closed are exactly those two
files, **both now at zero with the census as positive control** -- the detector sees 35 and 6 control-flow
sites in them respectively and finds all 41 braced. **`MASTER-121`'s closure is independently verified.** The
four files 6b-xviii added contribute 0 sites, so the 58-to-62 growth does not move the figure.

**Every zero is controlled.** Of the 58 files reporting no braceless site, **24 contain no control-flow
statement at all** (enums, interfaces, records, platform shims) and their zero is trivially correct; the other
**34 report a positive braced count with zero braceless**. The control that matters most is
`DungeonDetailViewModel.cs`, the file the row cites and where the count is now zero: the detector finds **two**
control-flow statements there -- one of them `:115`, itself a **nested-paren header**, the exact defect class
-- and reports both braced. **The zero is a measured negative, not a blind predicate.**

### Row versus population -- the session's most consequential figure

**Taken literally, ZERO of the 27 population sites fall inside any of the row's 15 cited spans.** Every span
points either at compliant or non-control-flow code (10) or at a site that no longer exists (5).

**Taken by file, which is the figure a task can act on:**

| Population file | Sites | Cited by the row? |
|---|---|---|
| `DungeonDetailPage.xaml.cs` | **21** | yes, by 14 of its 15 spans |
| `DungeonDetailViewModel.cs` | **0** | cited by 1 span, and **empty** |
| `ObjectKindInfo.cs` | **3** | **no -- no row covers these** |
| `ObjectPackRegistry.cs` | **2** | **no -- no row covers these** |
| `ObjectPaletteView.xaml.cs` | **1** | **no -- no row covers these** |

**21 of 27 (78%) are inside the row's intended file; 6 of 27 (22%) are in three files the row never names.**

So: **a task scoped to the measured 27 closes `MASTER-135` and the carried backlog item together.** Closing
the row by its own text -- even with all 15 spans corrected -- leaves 6 sites open and the backlog item
surviving the session. The 6 extra sites are two small `Assets/ObjectAssets` files and one line of
`ObjectPaletteView.xaml.cs`: trivial work beside the 21. **This is the synergy the UI layer was selected for,
and it is now measured rather than assumed.**

One caveat that must travel with the scope extension: `ObjectKindInfo.cs` is **3 braceless of 3**, so
closing it takes the file from 100% unbraced to 100% braced. That is the right outcome, but the row's "the
deviation is local rather than a house style" framing is a `DungeonDetailPage.xaml.cs` observation and
**does not transfer** to it.

### Remedy assessment: "mechanical and behaviour-preserving" is TRUE at all 27 sites

Six checks were run, each establishing a distinct hazard is absent:

- **No unbraced body is a declaration.** Every body line was extracted **by content**, never by offset. All
  27 are: `return;`/`return <expr>;` (8), `continue;` (2), a method call (6), an `await` of a call (6), an
  assignment to an **existing** variable or indexer (4), or a nested one-line `if...return` (1). **Zero
  declarations** -- the one case that genuinely changes scope when wrapped.
- **No comment sits between a header and its body** -- all 27 report `-` in the detector's
  comment-between column, and the detector was **proven able to see that case** on a fixture. The one nearby
  comment is *trailing* on `:401` and is unaffected.
- **No `else if` chain where bracing changes association.** The one chain's five bodies are each a single
  `await` statement; none is itself an `if`. **The one thing an implementer must not do** is rewrite
  `else if (cond)` as `else { if (cond) ... }` -- semantically identical but a gratuitous nesting change.
- **No dangling-else hazard.** The only `if`-bodied site is `:1312`, whose body is `return;` and which has
  no `else`. **No site has a nested unbraced `if`.**
- **`out var` and pattern variables in conditions are safe** -- they scope to the enclosing block regardless
  of bracing, so bracing cannot break the later reference.
- **One real hazard, and it is whitespace rather than semantics.** `:33` of `ObjectPaletteView.xaml.cs`
  indents with **tabs**, measured with `cat -A` (`^I^I`), matching the rest of the layer -- which means
  `UI-029`'s claim that this file indents with four spaces is **at least partly stale**, flagged for whoever
  owns that row and **NOT MEASURED as a whole-file property.** Because `Edit` strips trailing whitespace,
  each brace insertion must be keyed on the line's first text character.

**The two real risks are not semantic.** They are (i) **acting on the register's line numbers**, which point
at compliant or nonexistent code at all 15 spans, and (ii) **verifying with a naive detector**, which
undercounts this file by 6 or overcounts it by 11. **Verification of this remedy must use a matching-paren
detector, not a grep** -- and `ObjectPaletteView.xaml.cs:33` is precisely the site a naive verifier gets
wrong.

---

## 11. Corrections, four of them, two to the orchestrator's own figures

**All four were found by `dir-architect` re-deriving figures it had been handed.** This is the mechanism the
arc keeps re-learning: **a control only fully counts when someone other than its author runs it.** Recorded
here rather than quietly fixed, because the next session inherits the habits and not just the register.

### CORRECTION 1 -- the orchestrator's MapTool indent instruction was wrong and would have broken all six edits

**What the dispatch said:** each `MapTool` member description needs "**three tabs of leading whitespace**
before the description".

**What is true, measured with `cat -A`:** the byte form is **one leading tab, then the `///` marker, then two
tabs.** Section 9 `:375` indents the description two tabs **from the marker**, and inside a type body the
marker itself sits one tab in.

Confirmed against two independent anchors, `^I` being a tab:

```
MapTool.cs:4                   ///^I^IIdentifies the active drawing tool on the m...   (type-level, marker at column 0)
MapTool.cs:8                   ^I/// <summary>Draw tool M-bM-^@M-^T click to plac...  (member, marker one tab in)
CreateDungeonViewModel.cs:15   ^I///^I^IGets or sets the dungeon name entered by the user.
```

`CreateDungeonViewModel.cs:14-16` is the in-repo worked example of the correct three-line member form
**at the session base `c9d9b11` ONLY. It is `:16-18` in the post-wave-1 tree**, because T3's own
primary-constructor wrap added two lines above it — `:13` `AppDispatcher dispatcher)` and `:14`
`: ObservableObject`. **This base-era span was handed to both register tasks and BOTH wrote it into the
permanent record**, where their reviewers caught it as a blocking finding each. It is the most-propagated
defect of this session and it is the orchestrator's: sibling-induced drift inside a single session, carried
forward by the party whose job was to prevent exactly that. **Always cite which tree a line number belongs
to.**

**Whose error this is, precisely.** The measuring agent wrote "**1 tab + `///` + 2 tabs** = three tabs of
leading whitespace before the description text". **The arithmetic was correct and the summary phrase was the
wrong compression**; the orchestrator propagated the phrase into section 4 of this file and into the
architect dispatch **without re-deriving it**. "Three tabs of leading whitespace" reads as three tabs then
the marker, and an implementer pattern-matching it gets all six members wrong.

**This is the same defect class this session catalogued in the register ten times over: a figure carried
across a boundary without recomputation.** Section 4's remedy paragraph is superseded by this correction;
the authority is the byte form above, not the phrase.

### CORRECTION 2 -- section 6's `ObjectPaletteView` member census omits the public constructor

**What section 6 said:** "one public property, six public events, two public methods" -- nine members.

**What the census returns**, `grep -nE '^[[:space:]]*public[[:space:]]'` on the file, with the control that
the private field at `:9` is correctly **not** matched:

```
7:   public partial class ObjectPaletteView : ContentView
13:  public ObjectKind? ActiveObject { get; private set; }
15,16,17,18,19,26:  six public events
28:  public ObjectPaletteView()          <-- OMITTED from section 6's enumeration
352: public void SelectObject(ObjectKind kind)
358: public void ClearActiveObject()
```

**Ten public members plus the public type, eleven public sites. Exactly one -- `LoadFailed` at `:26` -- is
already documented, so the undocumented population is 10.**

**The orchestrator's dispatch figure of "10" was right by coincidence and wrong in its wording.** It said
"the work there is **10 public members**"; the truth is **10 undocumented sites, comprising 9 undocumented
members and the undocumented type.** The two reconcile only because the omitted constructor and the already
documented event cancel.

**The omission is internal inconsistency rather than a different convention**: section 6's own
`SecondaryPaletteView` enumeration (`:7, :8, :10, :18`) **does** include that type's constructor at `:10`.

### CORRECTION 3 -- a SIXTH detector hazard, absent from the instrument list: a brace inside the header

`DungeonDetailPage.xaml.cs:2208`, one of the 27 sites in the population, is:

```
if (child is not FloorSidebarItem floorItem || floorItem.Floor is not { } floor || floor.Id != _editor.SelectedFloorId)
```

`is not { } floor` is an **empty property pattern**, which puts a literal open brace **inside the header
text** -- a brace that opens no block. **Every hazard in the instrument list concerns nested PARENS; this is
a brace in the header, a distinct route to the same confident wrong answer**, and it sits inside the
population the bracing task must verify. A detector keying on "the first open brace after the header" reads
this site as already braced and silently drops it.

The detector used in section 10 handles it correctly -- it locates the **matching close paren** first and
only then looks for the body -- which is why its 27 and the orchestrator's independently-built 27 agree. But
nothing in the instrument list would have warned a third implementation, and that is now fixed.

### CORRECTION 4 -- a stale comment asserting a false fact, inside a file this session edits

`MapCanvasViewModel.cs:15-18` reads:

> "Step 2 of the MapTool migration. The App-layer `DungeonMaster.App.ViewModels.MapTool` **still exists** and
> is deleted by the task that owns it; until then a plain `using DungeonMaster.App.Core.Editor;` would be
> shadowed by same-namespace lookup and this file would silently keep binding the old enum. The alias is what
> actually rebinds the name."

**It does not still exist.** `find src -name 'MapTool*'` with `obj` and `bin` pruned returns exactly one
path, `src/DungeonMaster.App.Core/Editor/MapTool.cs` -- the same measurement that made `MASTER-119`'s
citation stale by project. So the stated justification for the `using MapTool = ...` alias at `:19` is no
longer load-bearing.

**Routed to report-and-backlog, not action.** Removing the alias is a name-binding change that no selected
row covers and that no task can build-verify, and `MapCanvasViewModel.cs` is already owned by the task doing
the primary-constructor and record-extraction work. Widening that task to a name-binding change would put an
unverifiable edit inside a diff whose other criteria are all content measurements.

**Why this one matters beyond itself:** it is a comment that was true when written and became false through a
sibling change -- the identical mechanism behind every stale citation in this session's eight rows. **The
register is not uniquely bad at this. The code has it too, and nothing in this repository can catch it:**
`GenerateDocumentationFile` appears in no build file, so no comment, tag or `cref` is compiler-verified in
either direction.

### CORRECTION 5 -- the naive-route-2 figure of 32 is ATTRIBUTED AND UNREPRODUCED

Section 10 records that three implementations disagreed on `DungeonDetailPage.xaml.cs` at **21, 15 and 32** --
the matching-paren detector, naive route 1, and naive route 2. The 21 and the 15 are independently
reproduced. **The 32 is not.**

T5's implementer built its own route-2 and reported that it returns **0** everywhere rather than 32, and
**said so rather than reporting the inherited number**. T5's reviewer then read that implementation's source
and established *why*: it appends a hit only when no `{` exists **anywhere later in the file**, so it is
**structurally inert** on essentially any input, and its zero on the fixture demonstrates nothing. The
reviewer separately recorded that it could not reproduce the 32 either.

**So two independent parties failed to reproduce it, and one of them explained why its own attempt could
not.** That does not falsify the original 32 -- M3's route-2 was a different implementation, a same-line
heuristic matching the **first** `)` rather than the matching one, and on this file it plausibly over-reports.
But the figure now stands **attributed to M3 alone and unreproduced in this session**, and section 10's "three
implementations disagreed ... at 21, 15 and 32" must be read that way.

**The conclusion section 10 draws is unaffected**, and is in fact strengthened: a grep cannot verify this
work. Route 1 was independently shown wrong in **both** directions -- 4 misses plus a genuine false positive
at base `:390`, where a greedy `.*\)` matched the inner `HasFlag(...)` paren of a multi-line header that is
in fact braced. **What is weakened is only the specific number 32**, and the lesson is the one this file
keeps relearning: a figure that travels without its instrument is a claim, and "three implementations
disagreed" is only as good as the third implementation nobody re-ran.

### CORRECTION 6 -- the orchestrator relayed summaries where it should have relayed reports, twice

Two reviewers independently reported that criteria discharged **by report text** could not be judged, because
the review dispatch carried an orchestrator-authored **summary** of the implementer's report rather than the
report itself.

- **T1** produced a **blocking** finding, B1, against criteria 13 and 14. The reviewer diagnosed the cause
  correctly and ranked "check the full report before spending a revision round" first; the elided sections
  were supplied and **B1 was withdrawn**. The implementer's report had carried both records in full all along.
- **T5** produced advisory **A3** on criteria 1 and 15, for the same reason. The reviewer marked both met --
  correctly, because 100% CRLF intact across all three files positively excludes both forbidden writers,
  which is **stronger evidence than the assertion itself** -- and asked the orchestrator to confirm the
  statements exist. **They do**, verbatim, in T5's report.
- **T2** produced a related advisory: the stored report file was a **reformat**, which labelled two content
  searches instead of quoting them and demonstrably dropped a `cat -A` paste the report said was present.

**One defect, three manifestations, and the fix is mechanical:** a reviewer judging a criterion that is
discharged by report text must receive the report **verbatim as a file**, never a summary and never a
reformat. The orchestrator changed practice for wave 2 on this basis. The cost of the summary was one
spurious blocking finding and two advisories that cost reviewer effort to resolve.

---

## 12. Two NEW instrument hazards, bringing the register to twelve forms

Both were found in this session by the party whose own predicate was wrong, which is the encouraging half of
the pattern. Neither fails loudly.

### FORM 11 -- a trailing character class that the stripped CR leaves nothing to match

**Found by T3's reviewer in its own work.** The predicate
`(record|class|struct|enum)[[:space:]]+<TypeName>[^A-Za-z0-9_]`, intended to match a type declaration as a
whole word, returned **0** for `ObjectKind`, `ObjectRotation` and `ObjectScale` -- **not an absence.** The
declaration lines **end at the type name**, and `grep -E` had already stripped the CR, so the trailing
`[^A-Za-z0-9_]` had **no character left to match**. Corrected to `([^A-Za-z0-9_]|$)`, all five types return 1.

**This is the exact MIRROR of the known `[^.]`-matches-the-CR trap**, and the pair is worth holding together:

| Form | What the character class does to the CR | Failure |
|---|---|---|
| `[^.]` on a CRLF line | **consumes** it as "a character that is not a dot" | matches where it should not |
| `[^A-Za-z0-9_]` as a trailing guard | **requires** a character the CR-stripping removed | fails to match at all |

Both are silent, both return a plausible number, and they fail in **opposite directions** -- which means a
reader who has internalised one is not protected against the other. **Any end-anchored predicate over this
repository's CRLF files needs `|$` in its final class.**

### FORM 12 -- a non-ASCII census whose matches are tabs

**Found by T3's implementer in its own work, and disclosed unprompted.** `grep -c '[^ -~]'`, used to test
criterion 9's claim that the doc blocks contain typographic apostrophes, returned **non-zero per file** and
**looked like confirmation**. The matches were **tab** lines: the per-file counts equalled each file's count
of `///` + two tabs and leading-tab lines exactly. The tab is outside the printable-ASCII range ` ` to `~`,
so a predicate for "non-ASCII" catches every indented line in a tab-indented repository.

**The correct instrument is a high-byte census** -- `od -An -tx1 -v` piped to a count of bytes `>= 0x80` --
and it must be controlled: it returns **0** on both files under test and **18** on
`App.Core/Editor/MapTool.cs`, which genuinely holds six em dashes at three bytes each.

**Why this one matters beyond itself.** It is the first instrument in this register whose failure mode is a
**false positive that confirms a false criterion.** Every other catalogued form returns a spurious zero or a
spurious difference; this one returned a spurious *presence*, and had its author trusted it, criterion 9's
fabricated clause would have been recorded as verified rather than falsified.

---

## 13. What the five implementers found in the frozen criteria

Recorded because the next session inherits the authoring habits, not just the register.

**Two frozen criteria carried false clauses, and both were the orchestrator's.**

- **T3 criterion 9 asserted a byte-level property that does not exist** -- typographic apostrophes in the
  doc blocks. There are no non-ASCII bytes in either host file. The clause demanded preservation of
  characters that are not there; byte-for-byte preservation was achieved anyway, which is strictly stronger.
  **A criterion asserting a byte-level property with no `od` beside it is the signal**, in the reviewer's
  own words.
- **T3 criterion 8 generalised from a population of one to a population of nine.** "ZERO test references" was
  measured on `PlaceObjectArgs` and stated of all nine; `ReorderFloorArgs` has four references across **two**
  test projects, one of them compiled. **This is the arc's own catalogued "a survey answers the population
  you scope it to" failure, reproduced inside a frozen artifact rather than in the register.**

**One criterion under-cited its own authority**, asymmetrically against a sibling. T1 criterion 6 required
"the section 9 form" and enumerated `:372`, `:375`, `:376`, omitting **`:378`**; T4's equivalent cited it.
All three of T1's new doc blocks break a sentence across two `///` lines. Review refuted it as blocking --
correctly, since a finding that cannot be pinned to a criterion's letter is never blocking, and the
implementer satisfied what it was given.

**One criterion's own cited anchor went stale during the session.** T4 criterion 11 names
`DungeonDetailPage.xaml.cs:2208`; a peer grew that file under T4, which found the header at `:2241` and
**nearly filed it as a detector false positive** before diagnosing it. The orchestrator measured it at
`:2243`. T4's criterion 8 anchor `:1219` likewise drifted to `:1239`. **Both are warnings for the wave-2
register tasks**, whose criteria already require content anchoring for that file.

**One criterion recorded a figure as NOT MEASURED that an implementer then measured.** T5 criterion 14 noted
the architect verified four sites as same-line and did not classify the remaining 22; T5 measured all 26 and
its reviewer independently confirmed exactly four same-line and 22 not.

**And one attributed figure could not be reproduced by anyone.** See CORRECTION 5: the naive-route-2 count of
32 stands attributed to its original author alone.

---

## 14. Wave 2's corrections to this file, and two more instrument forms

The two register tasks and their reviewers found **eleven defects in this evidence base**. Three were found
**independently by two parties**, which is the strongest corroboration available here. All are corrected
below rather than in place, except CORRECTION 1's span, which is fixed inline because it propagated.

| # | Section | What this file said | What is true | Found by |
|---|---|---|---|---|
| 1 | §11 CORRECTION 1 | `CreateDungeonViewModel.cs:14-16` | `:14-16` **at base only**; `:16-18` post-wave-1, moved +2 by T3's own wrap. **Propagated into both register tasks and caught as a blocking finding in each** | T6 review, T7 review |
| 2 | §6 | the two zeros "controlled by the same predicate returning **10 / 20 / 45 / 57 / 103** on the five other files in the same folder" | **four** genuine controls at 20 / 45 / 57 / 103. **The 10 is `ObjectPaletteView.xaml.cs`'s own count — the file under test, not a control on it** | T6, confirmed by its reviewer |
| 3 | §6 | `GenerateDocumentationFile` absent, control `TargetFramework` 9 of **10** build files | **11** build files. The two missed are the root `Directory.Build.props` and `Directory.Packages.props` — **exactly where that property would live, so the finding is STRONGER** | **T6 and T7 independently** |
| 4 | §8 | counter-census **16** lines, fourteen near-misses | **17 / 15**; `CharacterSpacing` is **6**, not 5 | **T6 and T7 independently** |
| 5 | §8 | filtering the 127 `Dm*` keys for width/column/panel/sidebar/palette name shapes "returns **nothing**" | returns **three** — `DmSurfaceToolPanel`, `DmTextToolPanel`, `DmPaddingPanel`, **none a dimension token**. The decisive test is **value coverage**, not name shape: no key carries 220/272/280/300 and none ≥ 100, controlled at `DmTouchTargetMinimum` 44 | **T6 and T7 independently**; T7 substituted the better predicate |
| 6 | §9 | "**7** in the two markup files under test", summing to 13 | those two files hold **10** (5 + 5). **The §9 subtotal is understated by 3 and its total of 13 matches the true `src` total of 13 only by coincidence** — agreement by accident, which reads as corroboration | T6's reviewer |
| 7 | §9 | the per-file split of the 13 `start session` hits as six and four | **5 and 5**, plus 2 and 1 | T6, reproduced by its reviewer |
| 8 | §9 | gesture-recognizer control 13; solution-wide search 13 hits with a 362-file control | **12**; and **10 hits with a 407-file control** — and **407 is scope-sensitive, being 467 without `obj`/`bin` excluded**. The criterion that mandated 362 **named no scope at all** | T7; T6 on the 407/467 |
| 9 | §10 | three implementations disagreed at 21, 15 and **32** | the 32 is **attributed and unreproduced** by three further parties. See CORRECTION 5 | T5, its reviewer, T7's reviewer |
| 10 | §7 | the type-vs-filename population | corrected to **11** modifier-agnostic / 10 gapped at base, with `readonly` as the modifier that actually hides a mismatch; **five** `public static` types in the layer, not two | T7, reproduced by its reviewer |
| 11 | §6 | 33 undocumented public sites across 9 files **at base** | **correct as a base figure and correctly attributed.** But the *post-change* census is **12 across 6 files**, not the 10 or 11 successively reported — the last site, `Platforms/MacCatalyst/AppDelegate.cs:6`, is skipped by a predicate that stops at a complete attribute **without walking above the attribute run** | T7's reviewer, on the third iteration of the same predicate |

**Entry 11 is the one worth dwelling on.** The same predicate was corrected twice and remained wrong both
times, and each iteration's validation cases **exercised the opposite case** — a documented member above its
attributes — rather than the defect under test. **Three rounds of control-does-not-match-the-defect-class on
one predicate**, each round's author believing the control adequate. The lesson the arc already holds is that
a control must match the defect class; what this adds is that **a corrected predicate needs a NEW control, not
the old one re-run.**

### FORM 13 — a WRITER that silently changes unnamed bytes

Every form 1 through 12 is a **predicate** returning a wrong number. This one is categorically different:
**`Edit` preserves CRLF line terminators and normalises every EMBEDDED lone CR in the whole file, however far
from the edit site.** Measured on `.dir-sdlc/backlog.md`: eight lone CR at lines 257-264, an edit at line 477,
result **zero**. Reproduced by the orchestrator on a purpose-built fixture — 10 CRLF lines with `\r\r\n` at
lines 3 and 4, one `Edit` at line 9, lone CR **2 → 0**, all ten terminators intact. **And reproduced a second
time live**, when T7's revision round destroyed them again on a file whose own task had just discovered the
hazard and was explicitly watching for it.

It is the **fourth** destructive write mechanism and the **second** specific to `Edit`. It **supersedes the
arc's standing rule that "`Edit` is the CRLF-safe writer"** — the accurate statement is that it preserves
line *terminators* only. **A 100%-CRLF check passes throughout the loss**, so the only detection is a lone-CR
count, derived as total CR minus CRLF-line count.

**The consequence for criteria: a criterion mandating `Edit` while also requiring embedded lone CR preserved
is mutually unsatisfiable.** That fired live here, was ratified by the user as escalation E5, and the repair
was a byte-surgical binary write verified two ways — **the first byte divergence between the pre-change copy
and the final file is at the edit site itself**, so every byte before it, including all eight lone CR, is
identical.

### FORM 14 — contamination that INFLATES a count while every real hit survives

**A detector whose pattern appears in the prose it scans.** On `backlog.md`, where the truth is 8 lone CR:

| Form | Returns | What it is |
|---|---|---|
| `grep -c '\^M\^M\$' file` without `cat -A` | **2**, all prose, zero genuine | the **already-known** filter-before-`cat -A` error |
| `cat -A file \| grep -c '\^M\^M\$'` | **10** — all 8 genuine **plus 2 prose contaminations** | **the new form** |
| `cat -A file \| grep -c '^\^M\^M\$$'` | **4** | anchoring changes it again |

**Every catalogued form until now under-counts; this one over-counts, and every real hit is still present** —
same sign, nothing looks broken. **It bites hardest in exactly this file, because the backlog's purpose is to
record patterns in prose: documenting a hazard disarms grep for that hazard in the document that records it.**
The figure to register is **10**. T7 reported 2, which was the older error wearing this one's clothes — its
reviewer separated the two.

### A correction to the orchestrator's own instruction, caught by both re-reviewers

**`git show ecb01e7:<path>` does not read the session base, and the failure mode differs by file.** For
**source** files `ecb01e7` is **post-wave-1** and identical to worktree HEAD, so measuring a base-era claim
there makes it look false — this **nearly produced a false blocking finding**. The source-file base is
**`c9d9b11`**. For the **two register documents**, `ecb01e7` effectively *is* the base, with the status cells
empty, so a round-1 output is **not recoverable from git at all**, wave-2 work being uncommitted until Gate 3.
**One wrong instruction, two different failure modes, both caught by the party it was given to.**
