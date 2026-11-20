<!-- frozen at Gate 2: 20261106-2050-detail-page-trio, 2026-11-06 -->

# C4 -- Register and audit updates: MASTER-094 / UI-007, MASTER-098 / UIKIT-011, MASTER-108 / UI-016

No callable operation. One task, **T4**, edits three status cells in `docs/audit/00-master-index.md` (rows 282,
286, 296), inserts two amendment blocks in `docs/audit/03-app-shell-and-views.md` (on `UI-007` and `UI-016`) and
one in `docs/audit/04-app-controls-and-theme.md` (on `UIKIT-011`). T4 runs after T3, so every figure it writes
describes the finished tree. **The backlog (`.sir/backlog.md`) is trimmed by the orchestrator at Gate 3** (section
5); no task touches `.sir/` or `.dir-sdlc/`. T4 runs no `dotnet` command; where a cell states a build or test
outcome, it is labelled **told** and attributed to the review record of the task that ran it.

Editing hazards: C1 section 2, binding. All three audit files are CRLF with loneCR=0 at startRef (C1 section 1).

## 0. Rules for every cell and block

- **Cite what you measured; label what you were told.** "Measured" means T4 ran the command on the worktree
  during its own run and the text gives the command, or its C1 section, beside the figure. "Told" means the value
  comes from a contract, a review record or the orchestrator, and the text says so.
- **Status token:** with `*` stripped, each edited cell's leading token is `done`. For row 282 this was decided
  at Gate 2: MASTER-094 closes `done` on the floor-scoped design, and its item 1 names the four mutations that
  keep the full reload (AddFloor, RemoveFloor, ReorderFloor, the header Save) and why (C3 section 3.2).
- **Escaping:** every literal `|` inside a cell is written `\|`. R (C1 section 5) prints 11 on each edited line.
  Cite instruments by their contract section rather than quoting one that contains `|`.
- **Previous wording:** each of the three cells ends with the sentence
  `**Previous status wording, retained unchanged below.**` followed by one space and the whole startRef cell,
  verbatim, then ` |`. V (C1 section 5) on the edited line reproduces OC's startRef digest and byte count for that
  row (282: `ab0c2cc2...`, 131; 286: `884672f8...`, 3495; 296: `e1c15f6a...`, 128).
- **Nothing else in the index changes:** U (C1 section 5) prints `7565604d...` after the edits and the file keeps
  691 lines.
- **Blocks:** in `03`, the house form of the newest block, `:2241-2271` (`AMENDED 20261106-0120-shipping-identity`):
  first line begins `**[AMENDED 20261106-2050-detail-page-trio, 2026-11-06 -- `, last line ends `]**`, and the
  block says `Form follows the \`AMENDED 6b-xvi\` block on \`UI-003\` in this file at \`:201\`.` as that block does.
  In `04`, the form of `:353-366` (`AMENDED 20261103-2239-app-follow-ups`): same opener shape, last line ends
  `**]**`. Every block has one empty line before and after it, consists of inserted lines only (nothing in the
  entry is deleted or edited, `locations:` included), and states that the record's severity, category, effort
  and risk are unchanged.
- **No hash of this session's own commit** appears anywhere, and each cell says why: tasks run without git and
  the orchestrator makes the commit (precedent: `MASTER-142` at `00-master-index.md:330`, "No commit hash is
  cited: the orchestrator makes the commit.").
- A cell or block may cite `.sir/sessions/20261106-2050-detail-page-trio/contracts/<file>` by path. It never cites
  a scratch path.
- After the last edit, B (C1 section 1): `00-master-index.md` CR=LF=CRLF=691, loneCR=0; each of `03` and `04`
  CR=LF=CRLF=(startRef line count from C1 section 1 + the lines T4 inserted), loneCR=0.
- AO (C1 section 5) after the edits: `03` prints **14** (startRef 12), `04` prints **13** (startRef 12).

## 1. MASTER-094 / UI-007

### 1.1 Status cell, `00-master-index.md:282`

startRef line: `| 94 | MASTER-094 | S3 | UI | performance | M | medium | Every mutation reloads the whole dungeon
aggregate, and exactly one does not | UI-007 | part done (5b) -- UI-007's second remedy, patching the projection
instead of re-querying the aggregate, deferred to a later session |`. Only the status cell changes. It begins
`**done** (20261106-2050-detail-page-trio, 2026-11-06) -- ` and states:

1. **What changed, measured by T4:** the nine floor-scoped view-model mutations (C3 section 3.1, named) now refresh
   only their own floor through `GetFloorDetailQuery` (C2) and replace that one `FloorItem` in place; P-REFRESH
   (C1 section 4.2) prints 9 lines. The four membership- and header-shaped mutations (C3 section 3.2, named, each
   with its reason) still reload the whole detail; P-RELOAD (C1 section 4.1) prints 4 lines, against 14 at
   startRef.
2. **The page's second query is gone:** at startRef every floor mutation the page drove ran the whole-detail query
   **twice** -- once in the view-model command and again in the page's `RefreshFloorAsync`
   (`DungeonDetailPage.xaml.cs:1335` at startRef, the fourteenth P-RELOAD line); that method now re-resolves the
   drawable's floor and queries nothing (T4 reads its body and cites its current line).
3. **No Application command result type changed**, measured by T4 with P-HANDLER (C1 section 4.7): 14 lines,
   digest `2628445f...`, identical to startRef. The backlog row's expectation that the remedy "also changes
   Application-layer command results" did not hold for the design signed at Gate 2 (**told**: Gate 2 decision).
4. A pointer to the `AMENDED 20261106-2050-detail-page-trio` block on `UI-007` in `docs/audit/03-app-shell-and-views.md`.
5. Test outcomes, if stated, **told** from T1's, T2's and T3's review records.
6. The no-commit-hash sentence.
7. `**Previous status wording, retained unchanged below.**` and the startRef cell.

### 1.2 Amendment block on UI-007

Inserted after `UI-007`'s `**Suggested direction.**` paragraph (startRef `03-app-shell-and-views.md:407-409`) and
before `### UI-008` (startRef `:411`). Anchor on the heading text, not the line number, because section 2.2 inserts
lower in the same file. It carries:

1. **Closure:** `MASTER-094` (`00-master-index.md:282`) is closed by this session, with the section 1.1 item 1
   figures and the design (C2, C3 section 3) by path.
2. **Correction of `What.`:** quote verbatim "Eleven of the twelve mutating operations end with `await
   LoadDungeonAsync(_dungeonId)`" and "`AddAreaWithOutlineAsync` (line 296) is the single exception and does not
   reload". At this session's startRef both were stale: the reload is `context.ReloadAsync()`, it ended **all**
   thirteen view-model mutations (P-RELOAD, C1 section 4.1: 13 view-model lines plus the page's 1), and
   `AddAreaWithOutlineAsync` reloaded once after its second write (`MapCanvasViewModel.cs:231` at startRef; its doc
   comment at `:175-189` records the change). No classification word is given: tasks have no git, so whether this
   is drift or error is not established.
3. **The "Why it matters." mechanism, re-stated as measured:** the page's `Floors.Clear()` rebuild is now
   `FloorList.Items` (`FloorListViewModel.cs:32`); a floor-scoped mutation raises one `Replace`, not a `Clear` and n
   `Add`s (C3 section 3.1). The page-comment citations `DungeonDetailPage.xaml.cs:545-546` and `:587` in that
   paragraph are not re-derived; `locations:` is kept unchanged as a record of the audited state.
4. **What remains, stated plainly:** the four section 3.2 mutations still re-query the whole dungeon, by design,
   each with its reason.
5. The severity/category/effort/risk sentence.

## 2. MASTER-108 / UI-016

### 2.1 Status cell, `00-master-index.md:296`

startRef line: `| 108 | MASTER-108 | S3 | UI | structure | L | high | The detail page's code-behind is 2,226 lines
and owns the editor's entire interaction state machine | UI-016 | part done (5b) -- UI-016's second remedy,
converting RebuildFloorList to a templated BindableLayout, deferred to a later session |`. The cell begins
`**done** (20261106-2050-detail-page-trio, 2026-11-06) -- ` and states, each figure measured by T4:

1. **The floor list is a templated `BindableLayout`:** `FloorListContainer` in `DungeonDetailPage.xaml` carries
   `BindableLayout.ItemsSource` bound to `FloorList.Items` and a `BindableLayout.ItemTemplate` whose root is
   `controls:FloorSidebarItem` (T4 quotes the two attribute lines with their line numbers). The row's "over `Floors`"
   names a collection that has been `FloorList.Items` since 5b split the view model.
2. **The hand-rolled loop is gone:** the C1 section 4.5 census for `RebuildFloorList` prints 0 lines in
   `src/DungeonMaster.App` and `src/DungeonMaster.App.UiTests` (startRef 4), `OnFloorsChanged` 0 (startRef 3),
   `new FloorSidebarItem` 0 in `src/DungeonMaster.App` (startRef 1); P-CHILDMUT (C1 section 4.4) lists only the drag
   placeholder's `Insert` and `Remove` lines.
3. **Row wiring is declarative:** the number of event and command attributes the template declares on
   `FloorSidebarItem` (T4 counts them by reading the template and lists them), against the per-row wiring in
   `RebuildFloorList` at startRef (`DungeonDetailPage.xaml.cs:794-1012`: nine event subscriptions and five
   `Area*Command` assignments, measured by the architect with
   `tr -d '\r' < <page.xaml.cs> | sed -n '794,1012p' | grep -cE '^\s*item\.[A-Za-z]+ \+= '` (9) and the same with
   `'^\s*item\.Area[A-Za-z]+Command = '` (5), read line by line to confirm: `DeleteRequested`, `SelectRequested`,
   `ReorderRequested`, `DragStarted`, `DragEnded`, `PlaceholderRequested`, `PlaceholderCleared`, `AddAreaRequested`,
   `NotebookRequested`; `AreaSelectCommand`, `AreaHoverStartedCommand`, `AreaHoverEndedCommand`, `AreaDeleteCommand`,
   `AreaNotebookCommand`. The range is gone after T3, so T4 quotes this figure as **told**, from this contract).
4. **The size the title states:** `wc -l` on `DungeonDetailPage.xaml.cs` now (T4 measures), against 2636 at
   startRef (C1 section 1) and the title's "2,226". The title is a record and is not edited.
5. The first remedy (5b) is untouched by this session and is not re-assessed here.
6. A pointer to the `AMENDED 20261106-2050-detail-page-trio` block on `UI-016`; the no-commit-hash sentence.
7. `**Previous status wording, retained unchanged below.**` and the startRef cell.

### 2.2 Amendment block on UI-016

Inserted after `UI-016`'s `**Suggested direction.**` paragraph (startRef `03-app-shell-and-views.md:795-797`) and
before `### UI-017` (startRef `:799`). Carries: the closure (section 2.1 items 1-3, by figure); that `What.`'s
"wiring twelve event handlers per row" is stale at startRef (nine events plus five commands, read from
`DungeonDetailPage.xaml.cs:807-1000` at startRef, **told** from this contract) with no classification word; that
the placeholder `Border` insertion the entry calls "a genuine visual concern" still lives in code-behind, as the
entry says it should; the `locations:` list kept unchanged; the severity/category/effort/risk sentence.

## 3. MASTER-098 / UIKIT-011

### 3.1 Status cell, `00-master-index.md:286`

The startRef cell is the long `part done (5c) -- ...` cell carrying the `AMENDED 6b-iv` and `AMENDED 6b-vi`
text (OC: 3495 bytes). The new cell begins `**done** (20261106-2050-detail-page-trio, 2026-11-06) -- ` and
states, each figure measured by T4:

1. **The page no longer owns the fact:** P-EXPW (C1 section 4.3) prints 0 lines (startRef 6: three page writes and
   three in the control itself); P-SELW prints 0 (startRef 1); the `SetAreaEditing`, `FocusNameEditor` and
   `CollapseAllExcept` censuses (C1 section 4.5) print 0.
2. **Both properties are bound to one owner:** `FloorSidebarItem.IsExpanded` is bound to whether the row's floor is
   `DungeonDetailViewModel.Editor.SelectedFloorId`, and `AreaSidebarItem.IsSelected` to whether the row's area is
   `Editor.FocusedAreaId` (C3 sections 1-2); T4 quotes each binding line with its line number.
3. **UIKIT-011's clause (c) is now met in full:** `grep -c 'public void'` on `FloorSidebarItem.xaml.cs` prints 1
   (startRef 3) -- `FocusReorderAffordance`, which moves keyboard focus and sets no appearance state, the case the
   6b-vi artifact's section 3 records as outside this entry's subject.
4. **The 6b-vi artifact's surviving claim is now false, and its line numbers had drifted before this session:**
   `.dir-sdlc/sessions/2026-09-16-floor-sidebar-keyboard-route/artifacts/MASTER-098-verification.md` section 1.7
   cites `:832`, `:965`, `:2204` and `:2191`; at this session's startRef the same statements stood at `:873`,
   `:1006`, `:2346` and `:2249` (C1 sections 4.3 and 4.5), and section 2 cites `FocusNameEditor` at `:262` and
   `SetAreaEditing` at `:274` / `:280`, which stood at `:284` and `:302` / `:308`. No classification word (no git).
   The artifact is a committed record and is not edited.
5. A pointer to the `AMENDED 20261106-2050-detail-page-trio` block on `UIKIT-011`; the no-commit-hash sentence.
6. `**Previous status wording, retained unchanged below.**` and the startRef cell.

### 3.2 Amendment block on UIKIT-011

Inserted after `UIKIT-011`'s `**Suggested direction.**` paragraph (startRef `04-app-controls-and-theme.md:592-594`)
and before `### UIKIT-012` (startRef `:596`). Carries: the closure (section 3.1 items 1-3); that "It also forces
the host page to track selection itself and push it back in" (startRef `:589`) is now false, with the evidence;
that the record's other claims stand as the 6b-vi artifact measured them (cited by path, not re-derived); the
`locations:` list kept unchanged; the severity/category/effort/risk sentence.

## 4. `docs/TODO.md` -- added to T4 at Gate 2

`docs/TODO.md`'s `### Known redundancies` (startRef `:197`) items 1, 2 and 3 (`:199`, `:201`, `:203`) describe
exactly what this session removes. Gate 2 decided to add the file to T4. The rules are in section 6.

## 5. Backlog, by the orchestrator at Gate 3

Following the closing pattern of previous sessions (C4 of `20261105-1907-core-heavy-rows`: "The backlog rows ...
are trimmed by the orchestrator at Gate 3"): remove `.sir/backlog.md:16`, `:17`, `:18` and `:38` (C1 section 6
gives each line's identity and the post-removal digest `620481ca...`). Lines `:15` (MASTER-081) and `:19`
(MASTER-130), and the related `:25` and `:27`, hash to their startRef values afterwards. The heading at `:13`,
`## Part-done register rows: MASTER-081, 094, 098, 108, 130`, would name three rows it no longer holds. The orchestrator
renames it to `## Part-done register rows: MASTER-081, 130` (spec Assumptions). There is no precedent: previous
sessions closed whole headings and left them empty, as `:5`, `:7`, `:9`, `:11` show. Because of the rename, the
`620481ca...` digest in C1 section 6 describes the trim **without** it, so the Gate 3 check is the four kept-line
hashes, `wc -l` down by 4, and a read of `:13`. The file has 0 lone CRs at startRef (C1 section 1), so the Edit tool's
lone-CR normalisation is not a hazard for this edit; B must still print loneCR=0 afterwards.

## 6. `docs/TODO.md` notes (T4, added at Gate 2)

- **Inserted lines only.** After each of items 1, 2 and 3 of `### Known redundancies` (startRef `:199`, `:201`,
  `:203`) insert one empty line and then one note line, which begins with three spaces and
  `**Note (2026-11-06, 20261106-2050-detail-page-trio):**`. Six lines are inserted in all; no existing line is
  edited or deleted, so the items' original text stays as the record of what was true.
- **Identity check.** `tr -d '\r' < docs/TODO.md | sha256sum` prints
  `758528758e33e33141b55d5d64b53847515921ffcb958d86285b2425e972918d` at startRef (227 lines), and that is what the
  file prints with the six inserted lines deleted. `grep -c "^   \*\*Note (2026-11-06"` prints 0 at startRef.
  On a fixture holding one note line and one unindented near-miss it prints 1 (run by the orchestrator at Gate 2).
- **What each note says, measured:** see the T4 criteria. Each note cites its figure with the command or the C1
  section behind it, and labels anything taken from a review record or contract as told.
- **Bytes.** The file is CRLF, its first 3 bytes are `23 20 54` (no BOM), and loneCR=0 at startRef. The notes are
  inserted with the Edit tool, keyed on the item line's first characters (the tool strips trailing whitespace).
  B prints CR = LF = CRLF = 233 and loneCR=0 afterwards.
