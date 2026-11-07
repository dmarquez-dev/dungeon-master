<!-- frozen at Gate 2: 20261106-2050-detail-page-trio, 2026-11-06 -->

# C1 -- Measurement instruments, startRef baselines, build rules and editing hazards

This contract declares no callable operation. It exists so every instrument a criterion names is available
verbatim to every later phase; scratch copies made during the spec phase do not survive it.

Every baseline below was measured by the architect in the spec phase on the session worktree
`C:/dm-wt/20261106-2050-detail-page-trio` at `442b6f587e866d3ad07a33632b44225f21452d15` (startRef for T1).
Paths are relative to the worktree root. Every grep runs **from the worktree root**, over the stated scope,
with `--exclude-dir=obj --exclude-dir=bin` (the App's `bin/` holds copied sources). Bare member names are used
throughout because this codebase splits calls across lines; a qualified single-line grep silently misses them.

## 0. How to run these

- Save any script into **your own scratch directory** and run it from there. Never save one into the worktree.
  Run Python as `python -I` with `C:/...` paths.
- Controls that need a fixture run on files in **your own scratch directory**, never in the worktree.
- `sha256sum` prints the digest followed by ` *-` or ` *<path>`. Compare the 64 hex digits only.
- Before trusting an empty, zero or unchanged result, re-run the instrument's control and confirm the stated
  output. A result from an instrument whose control was not re-run is not evidence.
- **`sed` and `awk` with a file argument strip CR on this machine** (measured: `sed -n 15p .sir/backlog.md` and
  `tr -d '\r' < .sir/backlog.md | sed -n 15p` hash identically). Every line-level instrument below therefore
  starts with `tr -d '\r' <` so the CR handling is explicit and identical for every reader.

## 1. B -- line-ending byte census

```
od -An -tx1 -v <file> | tr -s ' ' '\n' | awk 'NF{ if($1=="0d")cr++; if($1=="0a"){lf++; if(p=="0d")pair++} p=$1 } END{printf "CR=%d LF=%d CRLF=%d loneCR=%d\n",cr,lf,pair,cr-pair}'
```

Control (run): `printf 'a\r\nb\rc\n' > <scratch>/b.txt`, then B prints `CR=2 LF=2 CRLF=1 loneCR=1`. A file passes
when CR = LF = CRLF and loneCR = 0. BOM check: `head -c 3 <file> | od -An -tx1` (` 75 73 69` = `using`,
` ef bb bf` = UTF-8 BOM, ` 3c 3f 78` = `<?x`).

startRef values -- every file listed is CRLF with **loneCR=0**, CR = LF = CRLF = the line count shown:

| File | lines | first 3 bytes |
| --- | --- | --- |
| `src/DungeonMaster.Application/Common/Interfaces/IDungeonReadRepository.cs` | 37 | `75 73 69` |
| `src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonReadRepository.cs` | 110 | `75 73 69` |
| `src/DungeonMaster.App.Core/Editor/MapEditorState.cs` | 310 | `75 73 69` |
| `src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs` | 224 | `75 73 69` |
| `src/DungeonMaster.App/ViewModels/IDungeonEditingContext.cs` | 64 | `75 73 69` |
| `src/DungeonMaster.App/ViewModels/FloorListViewModel.cs` | 208 | `75 73 69` |
| `src/DungeonMaster.App/ViewModels/MapCanvasViewModel.cs` | 388 | `75 73 69` |
| `src/DungeonMaster.App/ViewModels/DungeonHeaderViewModel.cs` | 98 | `75 73 69` |
| `src/DungeonMaster.Tests/AppCore/Editor/MapEditorStateTests.cs` | 476 | `75 73 69` |
| `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` | 2636 | `75 73 69` |
| `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml` | 748 | `3c 3f 78` |
| `src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs` | 554 | `75 73 69` |
| `src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml` | 364 | `ef bb bf` |
| `src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml` | 157 | `ef bb bf` |
| `src/DungeonMaster.App.UiTests/AppSession.cs` | 230 | `75 73 69` |
| `docs/audit/00-master-index.md` | 691 | -- |
| `docs/audit/03-app-shell-and-views.md` | 2578 | -- |
| `docs/audit/04-app-controls-and-theme.md` | 1870 | -- |
| `.sir/backlog.md` | 199 | -- |

**`.sir/backlog.md` carries 0 lone CRs at startRef** (B above), not the 8 it historically carried.

Sibling precedent for NEW files: every new `.cs` file starts with `using` or `namespace` and carries **no BOM**,
like `GetDungeonDetailQuery.cs`, `GetDungeonDetailQueryHandler.cs`, `DungeonReadRepositoryTests.cs`,
`GetDungeonDetailQueryHandlerTests.cs`, `FloorReorderCommandArgsTests.cs`, `AppFixture.cs` and
`NotNullConverter.cs` (all ` 75 73 69`). New files are CRLF with loneCR=0.

## 2. Editing hazards (binding on every task)

1. **Every target file is CRLF** (section 1). After your last edit, B on each file you touched must print
   CR = LF = CRLF and loneCR=0, and the first 3 bytes must be unchanged.
2. **Never use `sed -i`, and never use the Write tool on an existing file.** Both rewrite a whole CRLF file to
   LF. Use the Edit tool for existing files. The Write tool is permitted only to create a new file, and the new
   file must then pass B (Write emits LF: if it does, convert it before you finish, by a method you name).
3. **The Edit tool strips trailing whitespace from `new_string`.** An edit that ends a line in tabs reports
   success and produces no indent. Key every edit on a line's first non-whitespace text; leading tabs survive.
4. **The Edit tool normalises any embedded lone CR anywhere in the file.** Run B before your first Edit of a
   file; if loneCR is not 0, stop and report rather than edit.
5. **Indent `.cs` files with tabs** (`.editorconfig:12-13`). `.editorconfig:38-39` also says tab for `*.xaml`,
   but `DungeonDetailPage.xaml`, `FloorSidebarItem.xaml` and `AreaSidebarItem.xaml` are indented with spaces
   (read: e.g. `FloorSidebarItem.xaml:304` opens with 12 spaces): **match the surrounding lines of the file you are
   editing** and do not re-indent anything you did not otherwise change. C# formatting authority:
   `C:/Users/dmarq/Development/Repos/dm-conventions/backend/dotnet-format.md`.

## 3. Build and test rules

- **This toolchain cannot build concurrently in one worktree.** Every project writes its `obj/` and `bin/`
  inside the worktree, which no `owns` glob covers; two `dotnet build`s of the solution would contend for them.
  The manifest therefore orders every task that runs `dotnet` (T1, T2, T3) in one chain. T4 runs no `dotnet`.
- Build: `dotnet build C:/dm-wt/20261106-2050-detail-page-trio/src/DungeonMaster.sln`. startRef:
  `Build succeeded.` `0 Warning(s)` `0 Error(s)` (orchestrator probe, `state.json`). Judge warnings and errors
  from those two summary lines, never from `grep -c warning` over the output (that count is always 2 for
  `dotnet build`).
- Test: `dotnet test C:/dm-wt/20261106-2050-detail-page-trio/src/DungeonMaster.sln --no-build` after the build.
  startRef: `DungeonMaster.App.UnitTests` 187 passed, `DungeonMaster.Tests` 958 passed,
  `DungeonMaster.App.Tests` 123 passed, 0 failed and 0 skipped in each (orchestrator probe, `state.json`).
  `DungeonMaster.App.UiTests` is not in the solution and is not run.
- A narrower command for a red step or an injection: `dotnet test <one test .csproj> --filter <expr>` is
  permitted and preferred. Report the exact command.

### 3.1 TA -- test-attribute census (no test deleted)

```
grep -rhoE --include=*.cs --exclude-dir=obj --exclude-dir=bin '^\s*\[Fact(\(|\])' src/<project> | wc -l
grep -rhoE --include=*.cs --exclude-dir=obj --exclude-dir=bin '^\s*\[Theory(\(|\])' src/<project> | wc -l
```

Control (run on a scratch fixture holding `\t[Fact]`, `\t[Fact(DisplayName = "x")]`, `\t// [Fact] commented`,
`\t[FactAttribute]`, `\t[Theory]`, `\t[InlineData(1)]`, CRLF): the Fact form prints 2 and the Theory form 1 --
the commented and `[FactAttribute]` lines must not match.

startRef: `DungeonMaster.Tests` Fact 635, Theory 44; `DungeonMaster.App.UnitTests` Fact 30, Theory 8;
`DungeonMaster.App.Tests` Fact 113, Theory 3. `grep -rhoE 'Skip\s*='` over each project prints 0 at startRef.

## 4. Source instruments (startRef values; re-run the control before trusting a result)

Fixture control for 4.1-4.4, run in the spec phase: a scratch `Fixture.cs` (CRLF) holding the lines
`\t\tawait context.ReloadAsync();`, `\tTask ReloadAsync();`, `\tpublic Task ReloadAsync() =>`,
`\t///\t<see cref="ReloadAsync"/>`, `\t\titem.IsExpanded = false;`, `\t\tIsExpanded = true;`,
`\t\tvar expanded = IsExpanded;`, `\t\tif (x.IsExpanded == true)`,
`\t\tIsVisible="{Binding IsExpanded, Source={x:Reference Root}}"`, `\t\tFloorListContainer.Children.Clear();`,
`\t\tFloorListContainer.Children.Count;`, `\t\tfloorItem.SetAreaEditing(area.Id, true);`, `\t\tfloorItem` and
`\t\t\t.SetAreaEditing(a, b);`, plus an identical copy under `obj/`. Results: P-RELOAD 1, P-EXPW 2 (the two
assignments only), P-CHILDMUT 1, and a bare `SetAreaEditing` grep 2 (the split call is caught; the `obj/` copy is
not counted). Recreate it in your scratch directory to re-run the control.

### 4.1 P-RELOAD -- full-detail reload call sites in the App

```
grep -rn --include=*.cs --exclude-dir=obj --exclude-dir=bin '\.ReloadAsync()' src/DungeonMaster.App
```

Matches an invocation through a receiver; does not match the declarations `Task ReloadAsync();` and
`public Task ReloadAsync() =>` or a `cref`. **startRef: 14 lines** -- `DungeonHeaderViewModel.cs:96`;
`FloorListViewModel.cs:96`, `:131`, `:170`, `:206`; `MapCanvasViewModel.cs:94`, `:130`, `:170`, `:231`, `:269`,
`:309`, `:345`, `:386`; `DungeonDetailPage.xaml.cs:1335`. Required after T2: 5 (the four kept in C3 section 3
plus the page's `:1335`). Required after T3: **4**.

### 4.2 P-REFRESH -- floor-scoped refresh call sites in the view models

```
grep -rn --include=*.cs --exclude-dir=obj --exclude-dir=bin '\.RefreshFloorAsync(' src/DungeonMaster.App/ViewModels
```

Scoped to `ViewModels/` so the page's own private `RefreshFloorAsync()` calls (which have no receiver) are out of
population. **startRef: 0.** Required after T2: **9** -- eight in `MapCanvasViewModel.cs`, one in
`FloorListViewModel.cs` (C3 section 3).

### 4.3 P-EXPW -- writes to `IsExpanded`

```
grep -rnE --include=*.cs --exclude-dir=obj --exclude-dir=bin 'IsExpanded\s*=([^=]|$)' src/DungeonMaster.App
```

**startRef: 6 lines** -- `FloorSidebarItem.xaml.cs:286`, `:431`, `:449` (each `IsExpanded = true;`) and
`DungeonDetailPage.xaml.cs:873` (`item.IsExpanded = false;`), `:1006` (`item.IsExpanded = true;`), `:2346`
(`other.IsExpanded = false;`). Required after T3: **0**.

P-SELW, the `IsSelected` sibling: `grep -rnE --include=*.cs --exclude-dir=obj --exclude-dir=bin 'IsSelected\s*=([^=]|$)' src/DungeonMaster.App`.
**startRef: 1** -- `FloorSidebarItem.xaml.cs:308` (`item.IsSelected = editing;`). Required after T3: **0**.

### 4.4 P-CHILDMUT -- imperative mutations of the floor list container

```
grep -rnE --include=*.cs --exclude-dir=obj --exclude-dir=bin 'FloorListContainer\.Children\.(Clear|Add|Insert|Remove)\(' src/DungeonMaster.App
```

**startRef: 5** -- `DungeonDetailPage.xaml.cs:796` (`Clear`), `:1002` (`Add`), `:2529` and `:2605`
(`Remove(_dragPlaceholder)`), `:2593` (`Insert(insertIdx, _dragPlaceholder)`). Required after T3: only the
placeholder's `Insert` and `Remove` lines remain; no `Clear(` and no `Add(` line.

### 4.5 Member censuses (bare name, whole `src`, `obj`/`bin` excluded)

`grep -rn --exclude-dir=obj --exclude-dir=bin '<Name>' src`, startRef:

| Name | startRef lines | Where |
| --- | --- | --- |
| `RebuildFloorList` | 4 | `DungeonDetailPage.xaml.cs:792`, `:794`; `FloorSidebarItem.xaml.cs:323` (doc comment); `src/DungeonMaster.App.UiTests/AppSession.cs:112` (doc comment) |
| `OnFloorsChanged` | 3 | `DungeonDetailPage.xaml.cs:229`, `:259`, `:789` |
| `SetAreaEditing` | 3 | `FloorSidebarItem.xaml.cs:302` (declaration), `:319` (`cref`); `DungeonDetailPage.xaml.cs:2249` (the one call) |
| `FocusNameEditor` | 2 | `FloorSidebarItem.xaml.cs:284` (declaration); `DungeonDetailPage.xaml.cs:2626` (the one call) |
| `CollapseAllExcept` | 2 | `DungeonDetailPage.xaml.cs:890`, `:2340` |
| `UpdateActiveAreaSidebarHighlight` | 5 | `DungeonDetailPage.xaml.cs:1011`, `:1078`, `:1089`, `:1836`, `:2238` |
| `new FloorSidebarItem` | 7 | `DungeonDetailPage.xaml.cs:800`; six in `src/DungeonMaster.App.Tests/Views/FloorReorder*Tests.cs`, out of every task's ownership and unchanged |

`grep -c 'public void' src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs` prints **3** at
startRef (`:284` `FocusNameEditor`, `:302` `SetAreaEditing`, `:335` `FocusReorderAffordance`); the near-miss
`private void ApplyExpanded()` (`:380`) does not match. The same count on `AreaSidebarItem.xaml.cs` prints 0.

`BindableLayout` in `DungeonDetailPage.xaml`: `grep -c 'BindableLayout' src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml`
prints **0** at startRef (its only uses in `src/DungeonMaster.App` are `FloorSidebarItem.xaml:309`, `:321`, `:331`,
the area list). P-BIND, `IsExpanded`/`IsSelected` set in XAML in attribute or property-element form:
`grep -rnE --include=*.xaml --exclude-dir=obj --exclude-dir=bin '(IsExpanded|IsSelected)(=|>)' src/DungeonMaster.App`
prints nothing at startRef: **nothing binds either property to view-model state** (the 6b-vi artifact's section 1.7
survivor, re-measured and still true). Control (run on a scratch `F.xaml`): `<a IsExpanded="{Binding X}" />`,
`<controls:FloorSidebarItem.IsExpanded>` and `<c IsSelected="{MultiBinding}" />` match; the near-miss
`<b IsVisible="{Binding IsExpanded, Source={x:Reference Root}}" />` (the form at `FloorSidebarItem.xaml:297`, `:306`,
`:345`) does not.

### 4.6 P-PHRASE -- stale reload-policy prose

```
grep -c -e 'detail reloaded' -e 'reloads the detail' <file>
```

startRef: `MapCanvasViewModel.cs` 16, `FloorListViewModel.cs` 8 (`:68`, `:71`, `:100`, `:106`, `:135`, `:142`,
`:174`, `:180`), `DungeonHeaderViewModel.cs` 1 (it says `reloads the dungeon detail`, matched by neither -- its
own phrases are below). Single-phrase counts, `grep -c '<phrase>' <file>`, startRef:
`every other mutation` in `DungeonHeaderViewModel.cs` 1 (`:64`); `There are no exceptions` in
`IDungeonEditingContext.cs` 1 (`:57`); `like every other` in `DungeonDetailPage.xaml.cs` 1 (`:707`);
`replaced every floor projection` in `DungeonDetailPage.xaml.cs` 1 (`:777`).

### 4.7 P-HANDLER -- Application command handler signatures (preserved)

```
grep -rho --include=*.cs --exclude-dir=obj --exclude-dir=bin 'ICommandHandler<[A-Za-z]*Command\(, [A-Za-z]*\)\?>' src/DungeonMaster.Application/Features | sort | sha256sum
```

**startRef: 14 lines** (`| wc -l` in place of `| sha256sum`), digest
`2628445fc6c50e001c12594c2c8d7af20d559724088162d5a4f0d78838c857c8`. Second method, same population:
`ls src/DungeonMaster.Application/Features/Dungeons/Commands` lists 14 folders. Must-match case:
`ICommandHandler<AddAreaCommand, Guid>` (`AddAreaCommandHandler.cs`); near-miss: the generic
`ICommandHandler<TCommand>` declarations in `Common/Dispatch/`, outside the scope. Under the decomposition this
manifest drafts, the digest is **unchanged** after every task: no command result type changes.

Queries: `grep -rn --include=*.cs --exclude-dir=obj --exclude-dir=bin ': IQueryHandler<' src/DungeonMaster.Application/Features`
prints 2 lines at startRef (`GetDungeonDetailQueryHandler.cs:11`, `GetDungeonsQueryHandler.cs:11`); 3 after T1.

## 5. Register instruments (T4; startRef values)

- **R -- unescaped pipes on a register line:**
  `tr -d '\r' < docs/audit/00-master-index.md | sed -n '<N>p' | sed 's/[\\][|]//g' | tr -cd '|' | wc -c`.
  Control (run): `printf '| a | b \\| c |\n'` through the same tail prints 3, `printf '| a | b | c |\n'` prints 4.
  **The form `sed 's/\\|//g'` FAILS that control** (prints 4 for both) and must not be used. startRef: rows 282,
  286 and 296 each print 11. Required after T4: 11 on each.
- **OC -- old status cell:** `tr -d '\r' < docs/audit/00-master-index.md | sed -n '<N>p' | sed 's/^.*| <ID> | //; s/ |$//'`,
  then `printf '%s' "$cell" | sha256sum` and `printf '%s' "$cell" | wc -c`. `| <ID> | ` occurs once on each line.
  startRef: row 282 (`<ID>` = `UI-007`) `ab0c2cc240eb0aa2ed41311aeca9dc7ed6422454eae768c08fe743f86c5c9989`, 131 bytes;
  row 286 (`UIKIT-011`) `884672f8782fcf6b95e03ec24154839d0604219e8916c5ce971c6ea3629c33e7`, 3495 bytes; row 296
  (`UI-016`) `e1c15f6a434ea9d4b02650cb845a6f80501c058a3c1f11ef0197f97e9ecb278c`, 128 bytes. None of the three
  contains the phrase `Previous status wording` (`grep -c` on the three lines prints 0).
- **V -- the retained previous wording:**
  `tr -d '\r' < docs/audit/00-master-index.md | sed -n '<N>p' | sed 's/^.*\*\*Previous status wording, retained unchanged below\.\*\* //; s/ |$//'`,
  hashed as OC. Must-match control (run): row 366 gives `7e26bc181ceac74267a14b9cf218a83fb2447a68e7750d326886ebc613866fe8`,
  the hash the `20261105-1907-core-heavy-rows` contract C1 recorded for that row's retained wording. Near-miss
  (run): row 345, which carries no retained wording, gives the whole line (3962 bytes, a different digest).
- **U -- nothing else in the index changes:** `tr -d '\r' < docs/audit/00-master-index.md | sed '282d;286d;296d' | sha256sum`.
  startRef: `7565604d62d1c06877b726aaced1ba296fb5c727b462c9fc63b540596e56374f`; the file has 691 lines.
- **AO -- amendment-block openers:** `tr -d '\r' < <file> | grep -c '^\*\*\[AMENDED'`. startRef: `03-app-shell-and-views.md`
  12, `04-app-controls-and-theme.md` 12. Must-match: `03-app-shell-and-views.md:2241` (`**[AMENDED 20261106-0120-shipping-identity, ...`).
  Closer forms differ by file and are judged by reading, not counted: the newest `03` block ends at `:2271` with
  `.]**`; `04` blocks end with `**]**` (e.g. `:366`).

## 6. Backlog instruments (orchestrator, Gate 3)

All with `tr -d '\r' < .sir/backlog.md | ...`. startRef line hashes (`sed -n '<N>p' | sha256sum`):
`:15` `706799b1dfd29158e0b175090bf07bb8fad9e52722c9551bbd637efcdb4556d1` (MASTER-081),
`:19` `0bbcfebb5488b975df2ccbca1f1125474ee75d8aecaedfb5647065e23b639e3e` (MASTER-130),
`:25` `50fef1e3e297f83f527777e5ded6ab630aaec80996ccbf1e9afb177855abfe1b`,
`:27` `b39ea2ceb2c1682d15fe1ec080ca847894a83bb78f71561dab6af7eee6e10730`.
Lines to remove: `:16` (MASTER-094), `:17` (MASTER-098), `:18` (MASTER-108), `:38` (the
`2026-08-27-give-the-editor-a-seam` duplicate naming both `MASTER-108` and `MASTER-094`). Post-removal digest
of the whole file, `sed '16d;17d;18d;38d' | sha256sum`: `620481ca4feea6dcb06b00308905a43b14c758c65d59e69e5345c2e841f36bc6`.
