# Coverage

<!-- Definition of done: one row per spec.md item. `Item`, `Covering task`, and
     `Gate 2` are written at Gate 2; `End state` is written at Gate 3. -->

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 nine single-floor mutations refresh only their floor, in place, with no whole-dungeon query | T2 (on T1's query) | covered | met (reviews/T2.md criteria 11, 14-21; the query: reviews/T1.md criteria 1-16) |
| D2 the four reloading mutations still reload, each with its reason documented | T2 | covered | met (reviews/T2.md criteria 12, 22, 25-29) |
| D3 the page no longer re-queries after an edit (P-REFRESH/P-RELOAD: P-RELOAD prints 4) | T3 | covered | met (reviews/T3.md criterion 11: P-RELOAD 4 lines, none in the page) |
| D4 no Application command result type changes (P-HANDLER digest preserved) | T1 | covered (Gate 2: the floor-scoped read; command results unchanged) | met (reviews/T1.md criterion 8, reviewer-reproduced digest 2628445f...; reviews/T4.md criterion 2) |
| D5 floor list is a templated `BindableLayout`; `RebuildFloorList` gone; replacing one floor rebuilds only its row | T3 | covered | met (reviews/T3.md criteria 1, 3, 17; the replaced row is re-bound, not rebuilt -- E2, A3) |
| D6 row wiring lives in the template; heading level and keyboard-reorder focus restoration preserved | T3 | covered | met (reviews/T3.md criteria 13, 14, 16, 24) |
| D7 page and control write neither `IsExpanded` nor `IsSelected`; both bound to `Editor`; `SetAreaEditing` and `FocusNameEditor` deleted | T2, T3 | covered | met (reviews/T2.md criterion 9; reviews/T3.md criteria 5, 6, 7, 10) |
| D8 solution builds 0 warnings / 0 errors; full test run has no failures and no removed tests | T3 | covered | met (reviews/T3.md criteria 34-36, reviewer-reproduced) |
| D9 three register cells (previous wording retained verbatim) and three amendment blocks | T4 | covered | met (reviews/T4.md criteria 1-14) |
| D10 backlog trim: lines 16, 17, 18, 38 removed; 15, 19, 25, 27 byte-identical; heading :13 renamed | orchestrator -- tasks may not touch `.sir/` | uncovered -> Gate 2: orchestrator at Gate 3; evidence C1 section 6 line hashes for :15/:19/:25/:27, `wc -l` down by 4, a read of :13 | met, orchestrator-run at Gate 3 before the report (main checkout, `.sir/backlog.md`): pre-trim the four kept-line hashes matched C1 section 6 and `sed '16d;17d;18d;38d'` digested 620481ca...; post-trim `tr -d '\r' < .sir/backlog.md \| sed -n '<N>p' \| sha256sum` on :15, :16, :22, :24 printed 706799b1..., 0bbcfebb..., 50fef1e3..., b39ea2ce... (the startRef :15, :19, :25, :27); `wc -l` 199 -> 195; :13 reads `## Part-done register rows: MASTER-081, 130`; B prints `CR=195 LF=195 CRLF=195 loneCR=0` |
| D11 runtime UI behaviour (spec, Human observation list, 10 items) | orchestrator -- no command can see it | uncovered -> Gate 2: the user observes at Gate 3; evidence the user's verdict per item | not met -- 9 of 10: items 1-5 and 7-10 observed OK by the user on the worktree build (`dotnet run --project C:/dm-wt/20261106-2050-detail-page-trio/src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0`); item 6 FAILED (a self-drop leaves the dragged card hidden), still failed after the T3 round-2 revision, which was discarded; carried to `.sir/backlog.md` (Appended at Gate 3) |
| D12 `docs/TODO.md` Known redundancies items 1-3 carry dated notes; the item lines are byte-identical | T4 | covered (added at Gate 2) | met (reviews/T4.md criteria 18, 19) |

<!-- Contract operations: one row per declared contract operation. `Operation`,
     `Consuming task`, and `Gate 2` are written at Gate 2; `Implementing task`
     and `End state` are written at Gate 3. -->

<!-- Gate 3 search: `git grep -nw <name>` over the session range 442b6f5..agent/20261106-2050-detail-page-trio,
     discarding the definition site, same-task files (unless present at base 442b6f5) and test files. -->

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| C2 `GetFloorDetailQuery` / `GetFloorDetailQueryHandler` | T1 | T2 | covered (predicted: called) | called -- `DungeonDetailViewModel.cs:193` (T2); prediction confirmed |
| C3 section 1 `MapEditorState.PropertyChanged` for `SelectedFloorId` / `FocusedAreaId` | T2 | T3 | covered | called -- bound in `DungeonDetailPage.xaml:759` (`Editor.FocusedAreaId`) and `:776` (`Editor.SelectedFloorId`, MultiBinding), T3; the subscriber is the MAUI binding engine |
| C3 section 2 `DungeonDetailViewModel.Editor` | T2 | T3 | covered (predicted: called) | called -- `DungeonDetailPage.xaml.cs:102` and `DungeonDetailPage.xaml:759` (T3); prediction confirmed |
| C3 section 3.1 replace-in-place semantics of the nine commands | T2 | T3 | covered | called -- `DungeonDetailPage.xaml.cs` executes all nine (e.g. `:1018` AddAreaCommand, `:789` UpdateFloorCommand); every one of these page call sites is present at base 442b6f5 |
| C3 section 3.2 full-reload semantics of the four commands | T2 | T3 | covered | called -- `DungeonDetailPage.xaml.cs:2683` AddFloor, `:892` RemoveFloor, `:965`/`:2387`/`:2513`/`:2639` ReorderFloor, `:768` header Save; present at base 442b6f5 |
