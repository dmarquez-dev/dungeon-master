# Handoff — sub-session 5c, "The control kit"

Written 2026-08-28, after sub-session 5b merged. Every figure below was measured on 2026-08-28 with the
command shown. **Nothing here is carried from an audit record without re-measurement**, because the record has
been wrong 14 times in the last session alone.

## Where things stand

Session 5 was split into three sub-sessions at Gate 1. **5a** ("Settle the App boundaries") and **5b** ("Give
the editor a seam") are both complete and merged. **5c is the last.**

Branch `chore/repo-audit`, clean, at **`f5bee10`**. Nothing pushed — the whole of Sessions 1–5b is local.

Baseline, `dotnet build` / `dotnet test DungeonMaster.sln` from `src/`:

```
Build succeeded.   0 Warning(s)   0 Error(s)
Passed!  - Failed: 0, Passed: 639, Total: 639 - DungeonMaster.Tests.dll (net10.0)
Passed!  - Failed: 0, Passed:   1, Total:   1 - DungeonMaster.App.Tests.dll (net10.0)
```

**There is no aggregate test count.** A solution-wide `dotnet test` prints two per-assembly lines. There is no
combined 640. Any criterion citing a count must cite it per assembly with the command that produced it.

5b closed 12 entries fully and 2 partially across 9 tasks, 9 commits, **zero revision rounds and zero blocking
findings**. Artifacts in `.dir-sdlc/sessions/2026-08-27-give-the-editor-a-seam/`.

## 5c's scope — 15 entries

Derived by taking Session 5's declared 32-entry list from `docs/audit/00-master-index.md:422` and subtracting
what 5a and 5b actually closed. **13 remain**, plus the 2 appended post-audit entries.

| Entry | Sev | Effort/Risk | Title |
|---|---|---|---|
| MASTER-105 | S3 | M / **high** | `DmInlineEditor` concentrates five responsibilities and hides a state change in `ClearFocusGlow` |
| MASTER-097 | S3 | M / medium | `Draw` allocates per area, per segment and per frame |
| MASTER-098 | S3 | M / medium | Sidebar items expose visual state through imperative methods, not bindable properties |
| MASTER-099 | S3 | M / medium | `FloorSidebarItem` re-declares and hand-forwards five of `AreaSidebarItem`'s events |
| MASTER-077 | S3 | S / medium | `propertyChanged` handlers imperatively write properties the XAML already binds |
| MASTER-059 | S3 | S / low | `FloorMapDrawable` exposes concrete mutable collections as public API |
| MASTER-060 | S3 | S / low | `DmSearchBar` paints its focus state imperatively rather than through visual states |
| MASTER-061 | S3 | S / low | `NotebookView` resolves a theme colour through a raw indexer and a null-forgiving operator |
| MASTER-007 | **S2** | XS / low | `NotNullConverter` reports an unresolved binding as a non-null value |
| MASTER-034 | S3 | XS / low | One-way converters throw from `ConvertBack` where the guidance is to return null |
| MASTER-035 | S3 | XS / low | `DrawAreasViewMode` and `DrawAreasEditMode` differ only in the alpha rule |
| MASTER-036 | S3 | XS / low | The 44-unit touch-target minimum is a bare literal repeated ten times |
| MASTER-037 | S3 | XS / low | `DmTheme.Gold` and `DmTheme.ScrollGoldLight` resolve the identical token key |
| MASTER-159 | **S2** | XS / low | `DmCard` never assigns its trailing presenter's content |
| MASTER-161 | S3 | XS / low | `FloorSidebarItem`'s card body is tappable but carries no `HandCursorBehavior` |

**MASTER-007 and MASTER-159 are the only S2s and should lead.** Everything else is S3.

## Read before writing any criterion — three record claims I checked, two are wrong

**MASTER-159's headline is FALSE as the tree stands.** The record says `DmCard` "never assigns its trailing
presenter's content, so a list card's Edit and Delete buttons never render". It does assign it —
`Controls/DmCard/DmCard.xaml.cs:46`, inside `TrailingContentProperty`'s `propertyChanged`:

```csharp
card.TrailingPresenter.Content = newValue as View;
```

Note `TrailingPresenter.IsVisible` is set `false` at `:84` and `:110` and `true` at `:104`, so if a real defect
survives it is a **visibility** problem, not an assignment one. **Re-derive the actual defect before scoping
this**, and if there is none, close the entry as already-fixed rather than inventing work. `git log` on that
file shows four commits since the audit, any of which could have closed it.

**MASTER-036's "ten times" is not supported.** Measured across `Controls/` and `Views/` XAML: **7** total
occurrences of `44`, of which only **4** are `HeightRequest`/`WidthRequest`/`MinimumHeightRequest="44"`.
Re-count before writing a criterion against a number.

**MASTER-161's claim survives verification.** `FloorSidebarItem.xaml` has 3 `HandCursorBehavior` — at `:86`,
`:106` and `:158`, all on child buttons. The **card body's** `TapGestureRecognizer` is at `:34` inside
`FloorBorder` (`:16`) and has none. The entry is valid as written.

**MASTER-037 is confirmed and the file already documents it.** `DmTheme.cs:39-40` has `Gold =>
Resolve<Color>("DmAccentGoldLight")`, and the doc comments at `:33`, `:56` and `:102` already say the value is
shared. The duplication is real and self-described.

Measured sizes of 5c's main targets (`wc -l`): `NotebookView.xaml.cs` **496**, `FloorMapDrawable.cs` **576**,
`DmTheme.cs` **523**, `DmInlineEditor.xaml.cs` **431**, `FloorSidebarItem.xaml.cs` **357**,
`AreaSidebarItem.xaml.cs` **154**, `DmCard.xaml.cs` **115**, `DmSearchBar.xaml.cs` **47**.

## Seven backlog items 5c inherits

All in `.dir-sdlc/backlog.md` under "For sub-session 5c -- from sub-session 5b". Three interact directly with
5c's scope:

1. **`DmTheme.StrokeHairline`** — retire `PaletteThumb.cs`'s `StrokeThickness = 1` literal via a new accessor.
   This lands in `DmTheme.cs`, which **MASTER-037 also touches**. Do them together. Doing it also removes
   `PaletteThumb.cs` from `StrokePendingMigration`, at which point correct that list's doc comment at
   `GeometryScaleTests.cs:280-284`, which says "The four C# entries" but now describes three.
2. **`CreateDungeonPopup.xaml` per-binding fix** — 5b's T6 proved a per-binding `x:DataType={x:Null}` closes
   the `MAUIG2045` gap that T4 had concluded was unclosable. That page has 7 bindings, all currently
   reflection-based; the technique leaves only `CreateCommand` uncompiled. **This falsifies a comment at
   `CreateDungeonPopup.xaml:15-47` that must be corrected with it.**
3. **The dead-by-coupling guard** at `DungeonDetailPage.xaml.cs:984-992` — `AbortInProgressDraw` now clears
   `IsMoveDragging`, so the very next `if (_editor.IsMoveDragging)` can never fire. Not currently reachable;
   a future change makes it a silent bug.

The other four: the silent `Guid.Empty` exit at `MapCanvasViewModel.cs:213-216`; the lost ``Was `_field` ``
provenance in `contracts/app-core-editor.cs`; the unreachable `n < 3` guard at `AreaOutlineGeometry.cs:148`
(**do not "fix" it and do not delete it**); and the two remedy halves below.

## MASTER-108 and MASTER-094 closed PARTIALLY — do not mark them done

5b closed the first half of each. The second halves were deferred to 5c by the user's decision at Gate 2:

- **UI-016's `BindableLayout` conversion.** `RebuildFloorList` is still the imperative loop at
  `DungeonDetailPage.xaml.cs:717`, wiring handlers per row.
- **UI-007's "then narrow it".** 5b made the reload policy uniform — 13 mutating members, 13 reload sites,
  zero deviations — but did not patch the projection instead of re-querying. This one **changes
  Application-layer command results**, a layer no 5b task owned.

Their rows in `docs/audit/00-master-index.md` must not carry a done marker, and reconciling the index is
5c's job.

## Environment traps that cost 5b real time

**Create the worktree under a short root — `C:\dm-wt\<slug>`.** The scratchpad worktree path is ~184 chars,
which pushes `src/DungeonMaster.App/obj/.../win-x64/input.json` to **263** chars, past `MAX_PATH` (260). The
net472 WinUI `XamlCompiler.exe` then fails with a **misleading** `input.json doesn't exist!`. `LongPathsEnabled`
is already `1` and does not help. This masked four tasks' true state and produced entirely phantom figures
(92 `IDE0055`, 2 `MAUIG2045`) before it was diagnosed. 5a's path was 185 chars and never hit it.

**`grep -c '^\t'` silently returns 0** in this Git Bash — BRE does not expand `\t`, and `grep -P` errors on the
locale. Use `TAB=$(printf '\t'); grep -c "^$TAB" <file>`. Never write the broken form into a criterion: it
returns a plausible wrong answer instead of failing, so a criterion can be reported met with no evidence.

**`sed -i` rewrites files as LF here**, which trips `IDE0055` under `TreatWarningsAsErrors`.

**Every XAML file violates its own `.editorconfig`.** `:38-39` declares `[*.xaml]` → `indent_style = tab`;
`DungeonDetailPage.xaml` is 0 tab-leading and 643 four-space-leading. `IDE0055` does not reach XAML so nothing
catches it. **Match the file, never retab** — and note `.editorconfig:42` is the `[*.{json,yml,yaml}]` section,
not the XAML one. Two 5b criteria cited `:42` wrongly; see the process note below.

**macCatalyst is parked (ADR-001)** — it builds but cannot be run. No criterion may demand observed macCatalyst
behaviour. Verification is compilation of that TFM plus inspection.

**`DungeonMaster.App.Tests` runs outside a MAUI host** — no `CreateMauiApp()`, no `Application.Current`, and
nothing deriving from `ContentView`/`ContentPage`/`Shell` can be constructed. **5c is a control-kit session, so
this bites harder than it did in 5b**: most of these entries are about `ContentView` subclasses. Plan the test
story around what can actually be executed — `DungeonMaster.App.Core` (`net10.0`, MAUI-free, referenced by
`DungeonMaster.Tests`) is the home for anything extractable.

## Process lessons worth carrying

**The criteria-defect failure mode changed in 5b, and 5c should expect the new kind.** Sessions 2–4 produced
only wrong numbers and wrong strings, none of which changed what was built. 5b produced **14 defects**,
including several that were *semantic*:

- a criterion **contradicting its own frozen contract** (T4's "`Shell.Current` exactly one hit", where the
  contract requires the service to read it);
- an **over-broad grep pattern that forced worse code** (T1's `using DungeonMaster\.App\.` also matching
  App.Core's own sibling namespace, forcing using-aliases);
- a criterion **asserting a false fact about the tree** (T5's "the page still awaits `LoadTask`" — it never
  did);
- a criterion whose claim **made the build fail** (T6's "so the binding compiles" — `MAUIG2045`);
- and one false `.editorconfig:42` citation that appeared **identically in two independently-dispatched
  tasks**, which makes it a **template defect, not two mistakes**.

Four rules that earned their keep, all four of which I would carry verbatim:

1. **Tree wins over criterion.** Dispatch with verbatim criteria plus a standing rule: implement what is
   correct and report the discrepancy with the command that showed it. All 14 defects were caught this way and
   none changed what shipped.
2. **Check sibling criteria against each other, not only against the tree.** Two of 5b's defects were only
   visible by comparing a criterion to its own contract or to a peer criterion.
3. **Hand the reviewer the implementer's report, not just the diff.** A criterion demanding observed output
   cannot be judged without it.
4. **Ask the reviewer to reproduce the strongest claim, not accept it.** This paid off repeatedly — reviewers
   independently reproduced injected defects, generator output, and a `MAUIG2045` failure, and one found a
   coarsening the implementer had missed.

**Serialize tasks that share a build artifact.** 5b ran T5 and T6 separately because both participate in the
same XAML compilation and T5's criteria demanded a clean build; concurrent edits would have confounded its own
evidence. Ownership disjointness is necessary but not sufficient when two tasks feed one compiler.

**A `git status --porcelain` scope check is unsound in a shared worktree.** Two reviewers found this
independently. Always scope it: `git status --porcelain -- <owns globs>`.

**An ownership glob naming a `.csproj` must also name its `packages.lock.json`** (`Directory.Build.props:21`
sets `RestorePackagesWithLockFile=true`). And a glob pointing at `.dir-sdlc/` can never be satisfied by
`commit-task` on the session branch, because `.dir-sdlc/` is not carried there — 5b's T8 hit this and the split
had to be recorded deliberately.

## Suggested shape

Announce **Standard** — the Large decomposition already happened at Session 5's Gate 1.

Suggested order: the two S2s first (**MASTER-007**, then **MASTER-159** once its real defect is established, or
closed as already-fixed) → the four XS theme/converter entries, which are cheap and mostly independent
(**034, 035, 036, 037** + the `DmTheme.StrokeHairline` backlog item, all touching `DmTheme.cs`/`Converters/`) →
the sidebar cluster (**098, 099, 161**, which share `FloorSidebarItem`/`AreaSidebarItem` and should be one
ownership set) → **060, 061, 077, 059** → **097** (drawable allocation) → **105** (`DmInlineEditor`, the only
high-risk entry) → the two deferred remedy halves if there is room.

Watch ownership: `DmTheme.cs` is touched by 037 and the backlog accessor; `FloorSidebarItem` by 098, 099 and
161; `FloorMapDrawable.cs` by 059 and 097. Those must not run concurrently.

Finally: **reconcile `docs/audit/00-master-index.md`** at the end — 5c is the last sub-session of Session 5, so
it owns marking Session 5's rows, including the two partial ones that must not be marked done.
