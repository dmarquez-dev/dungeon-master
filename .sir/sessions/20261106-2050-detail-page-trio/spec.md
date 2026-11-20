# detail-page-trio

Close MASTER-094, MASTER-098 and MASTER-108 on `DungeonDetailPage`. Single-floor edits refresh one floor in place.
The sidebar's selection and expansion state moves to the view model. The floor list becomes a templated
`BindableLayout`.

## Context

This is the backlog unit "Part-done register rows" (`.sir/backlog.md:13-19`), narrowed at Gate 1 to the three rows
that share the page (`discussion.md`, Decisions). Every figure below was re-measured at `442b6f5` on the session
worktree, with the command and its control in C1.

- **MASTER-094 / UI-007.** All 13 view-model mutations end in the whole-dungeon reload. That is 14 call sites of
  `.ReloadAsync()` (C1 section 4.1): 13 in the view models and one in the page at `DungeonDetailPage.xaml.cs:1335`.
  - Every floor edit the page drives therefore runs `GetDungeonDetailQuery` **twice**: once in the command, then
    again in the page's `RefreshFloorAsync`.
  - UI-007's "eleven of the twelve" is stale.
  - The commands already load the aggregate in order to write it (`MutateAsync`), and the projection exists only in
    `DungeonReadRepository.cs:48-109`.
- **MASTER-098 / UIKIT-011.** The four defects that 6b-vi removed are still gone, and its one surviving claim still
  holds: nothing binds `IsExpanded` or `IsSelected` (the XAML grep prints nothing).
  - There are 6 writes to `IsExpanded` (C1 section 4.3): 3 in the page (`:873`, `:1006`, `:2346`) and 3 in
    `FloorSidebarItem` itself (`:286`, `:431`, `:449`).
  - There is 1 imperative `IsSelected` write (`FloorSidebarItem.xaml.cs:308`, inside `SetAreaEditing`).
  - The page owns its own `MapEditorState` (`:37`).
  - The line numbers in 6b-vi's breakdown have drifted.
- **MASTER-108 / UI-016.** `RebuildFloorList` (`:794`) clears the container and re-adds every row.
  - It runs on every `CollectionChanged`, and each row is wired by hand: 9 events and 5 commands, where UI-016
    says twelve.
  - The page is 2,636 lines; the register title says 2,226.
- **Line endings.** Every target file is CRLF with 0 lone CRs, measured byte by byte (C1 section 1).
  `.sir/backlog.md` now holds 0 lone CRs, not the 8 recorded earlier.

## Definition of done

- [ ] **D1** The nine single-floor commands (C3 section 3.1) refresh only their floor, replacing that one
  `FloorItem` in place with one `Replace` notification, and dispatch no `GetDungeonDetailQuery`.
  - A null read, an unknown floor or a null `Dungeon` falls back to the full reload.
- [ ] **D2** AddFloor, RemoveFloor, ReorderFloor and the header Save still reload the whole detail, each with its
  reason documented at its declaration.
- [ ] **D3** The page no longer re-queries after an edit: P-RELOAD prints 4 lines, none of them in the page.
- [ ] **D4** No Application command result type changes: the P-HANDLER digest equals its startRef value.
- [ ] **D5** `FloorListContainer` is a templated `BindableLayout` over `FloorList.Items`.
  - `RebuildFloorList` and `OnFloorsChanged` are gone.
  - Replacing one floor leaves the other rows as the same instances.
- [ ] **D6** The row events and commands are wired in the template. The heading level and the keyboard-reorder
  focus restoration are preserved.
- [ ] **D7** No `IsExpanded` or `IsSelected` write remains under `src/DungeonMaster.App` (P-EXPW and P-SELW each
  print 0).
  - Both properties are bound one-way to `DungeonDetailViewModel.Editor`.
  - `SetAreaEditing`, `FocusNameEditor` and `CollapseAllExcept` are deleted.
- [ ] **D8** The solution builds with 0 warnings and 0 errors. `dotnet test` on the solution reports 0 failed and
  0 skipped in all three assemblies, and no startRef test is removed.
- [ ] **D9** Register rows 282, 286 and 296 carry new status cells, each retaining its previous wording verbatim.
  UI-007, UI-016 and UIKIT-011 each carry an amendment block (C4).
- [ ] **D10** Backlog lines `:16`, `:17`, `:18` and `:38` are removed, and the heading at `:13` is renamed to name
  081 and 130 only. Lines `:15`, `:19`, `:25` and `:27` stay byte-identical (C1 section 6). This item is the
  orchestrator's, at Gate 3.
- [ ] **D11** Each runtime behaviour in **Human observation** below is observed by the user at Gate 3.
- [ ] **D12** Items 1, 2 and 3 of `docs/TODO.md`'s `### Known redundancies` each carry a dated note stating what
  is now true, with a measured figure. The items' own lines stay byte-identical (C4 section 6). Added at Gate 2.

## Scope

The four tasks run as a serial chain: T1 → T2 → T3 → T4.

| Task | What it does |
|---|---|
| T1 | `GetFloorDetailQuery → FloorDto?`, with its repository read and tests (C2) |
| T2 | The floor-scoped refresh, and `MapEditorState` made observable and owned by the view model (C3) |
| T3 | The templated floor list, the `IsExpanded`/`IsSelected` bindings, and the page's re-query removed |
| T4 | The register, plus the `docs/TODO.md` notes added at Gate 2 (C4) |

### Not in scope

- MASTER-081 and MASTER-130. Their backlog lines stay byte-identical.
- Any part of `docs/TODO.md` other than the three Known-redundancies notes. Gate 2 added those notes to T4.
- Changing command result types. Gate 2 chose the floor-scoped read, and row 282 closes `done`.
- The drag placeholder's index-based insertion into the now-bound container. It stays in code-behind, and the
  observation list covers it.
- `MultiBinding`. The repo has no precedent for it, and the criteria judge observable state, not the binding
  mechanism.

### Human observation (D11, at Gate 3)

1. Floor cards: expand and collapse visuals, only one card expanded at a time, and a re-tap collapses.
2. Expanding from the keyboard through the chevron, with focus staying visible on it.
3. The area highlight after a notebook rename, after drawing an area and after nudging an object.
4. Ctrl+arrow reorder repeated several times, with focus restored each time.
5. The scroll position after an edit on a low floor, and after a reorder.
6. Drag reorder: the placeholder appears; a drop on a card, a seam or the placeholder reorders once; no placeholder
   is left behind.
7. Add floor: the new floor ends up selected and expanded, with its map shown.
8. The click-only row events and commands: delete floor, reorder, drag started/ended, placeholder shown/cleared, add
   area, open a floor's notebook, delete an area, area hover dimming the map, and open an area's notebook.
9. Where keyboard focus lands when the row holding it is replaced.
10. The selected, expanded, hovered and drag-over visual states paint.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Can tasks build concurrently? | No. One worktree means a shared `obj/` and `bin/`. T1, T2 and T3 all run `dotnet`, so they are serial; T4 runs none. | 1 | C1 section 3; `dependsOn` |
| Do injected defects collide with peers? | No. The chain is fully serial. J1-J3 are in T1's files, J4-J8 in T2's and J9-J13 in T3's. | 4 | manifest |
| Does 094 change command results? | No. Gate 2 chose the floor-scoped read and closes row 282 `done`, naming the four full reloads. The P-HANDLER digest `2628445f...` is preserved, and T1 re-measures it. | 1 | C1 section 4.7. The orchestrator re-ran it and got the same digest. |
| Who owns selection and expansion? | `DungeonDetailViewModel.Editor`, a `MapEditorState` that now raises change notifications. The page's `_editor` is deleted. | 4 | C3 sections 1-2; frontend.md, State ownership |
| Which mutations still reload the whole dungeon? | AddFloor, RemoveFloor and ReorderFloor (membership or renumbering), and the header Save (the dungeon-level projection). | 4 | C3 section 3.2; `IDungeonEditingContext.cs:56-58` requires a documented reason |
| Does any test outside the tasks' ownership implement `IDungeonEditingContext` or read a deleted member? | No. The one test use is `Substitute.For` (`FloorReorderCommandArgsTests.cs:102`). The two hits on `FloorListContainer` read the XAML, whose container markup T3 #2 preserves. | 1 | Orchestrator grep over the three test projects |
| Where do the tests run? | View-model tests run in `App.UnitTests` (NSubstitute, plain net10.0). Page and control tests run in `App.Tests` under the MAUI host fixture. `App.UiTests` is not in the solution. | 1 | `.csproj` references; `DungeonDetailPageTeardownTests.cs`; `grep -c UiTests src/DungeonMaster.sln` prints 0 |
| Is the 098 breakdown still current? | Yes in substance, but its line numbers have drifted. | 1 | C4 section 3.1 item 4 |
| How does the backlog heading at `:13` read after the trim? | It is renamed to `Part-done register rows: MASTER-081, 130`, because both of those rows remain under it. There is no precedent, since earlier sessions emptied whole headings. | 4 | `.sir/backlog.md:13`; D10 |
| Who trims the backlog? | The orchestrator, at Gate 3. Tasks never touch `.sir/`. | 1 | C1 section 6; the precedent of `20261105-1907-core-heavy-rows` |

## Contracts

- [C1 -- measurement, build rules, editing hazards](contracts/C1-measurement.md)
- [C2 -- `GetFloorDetailQuery` (T1 → T2)](contracts/C2-floor-detail-query.md)
- [C3 -- editor state owner and floor-scoped refresh (T2 → T3)](contracts/C3-editor-state-and-floor-refresh.md)
- [C4 -- register cells and blocks (T4), plus the backlog trim](contracts/C4-register.md)

## Open questions

None. Gate 2 decided three things:

- MASTER-094 uses the floor-scoped read, and row 282 closes `done`.
- `docs/TODO.md` items 1-3 get dated notes, written by T4.
- The user observes the ten runtime behaviours at Gate 3.
