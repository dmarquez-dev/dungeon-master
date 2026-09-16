# Pre-decomposition measurement — session 6b-vi

Taken by the orchestrator in the **main checkout** at commit `7483b57`, on 2026-09-16, before the architect
was dispatched. Every claim below names the command that produced it. Nothing here is restated from
`HANDOFF-session-6b-vi.md`, from the audit index, or from a prior session's record.

The handoff's §4 rule is the reason this file exists: **decompose from the measurement, never from the
entry's prose.** 6b-iv found four of five inherited entries had changed disposition; 6b-v found its single
entry's prose false. This session measured two entries and **one changed disposition outright.**

---

## 1. Repository state

`git worktree list`, `git rev-parse`, `git status --porcelain`, run in the main checkout:

- `chore/repo-audit` at `7483b57f91cb89e8f89eef742d540c15932fec82`, working tree clean.
- `main` at `45495c1`, local and `origin/main` identical.
- Session worktree `C:/dm-wt/6b-vi` on branch `dir/2026-09-16-floor-sidebar-keyboard-route`, at the same
  commit, clean. Independently verified by the orchestrator rather than read from `dir-git`'s report.

Index survey, re-derived with `HANDOFF-session-6b-vi.md` §6's script against
`docs/audit/00-master-index.md`: **176 entry rows — 119 closed, 54 open, 3 part done.** Open by severity:
S2 1, S3 12, S4 31, S5 10. Open by layer: CORE 20, UI 9, UIKIT 9, TEST 8, INFRA 6, plus 2 spanning two
layers. This reproduces the handoff's §2 exactly.

## 2. File sizes, measured with `b.count(b'\n')`

Not `content.split('\n')` — that method is off by one on every newline-terminated file, which is 6b-v's
backlog item 3.

| File | Lines |
|---|---|
| `src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml` | 331 |
| `src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs` | 458 |
| `src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml` | 157 |
| `src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml.cs` | 261 |
| `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` | 2488 |

**Note for whoever cites `MASTER-108`:** that entry describes `DungeonDetailPage.xaml.cs` as "2,226 lines".
It is **2488**. `MASTER-108` is not in this session's scope and is not amended here; the number is recorded
so nobody re-states 2,226 from the entry.

---

## 3. `MASTER-176` — the floor drag handle has no keyboard route

**Disposition unchanged: the defect is real and present.** Every load-bearing claim in the entry at
`docs/audit/00-master-index.md:658` was re-verified against the code and holds.

**The handle.** `FloorSidebarItem.xaml:237-255` — a `Label`, `Grid.Column="4"`,
`AutomationId="FloorDragHandle"`, glyph `&#x22EE;`, carrying exactly two gesture recognizers:

```
<TapGestureRecognizer Tapped="OnDragHandleTapped" />
<DragGestureRecognizer DragStarting="OnDragStarting" DropCompleted="OnDropCompleted" />
```

`OnDragHandleTapped` (`FloorSidebarItem.xaml.cs:363-364`) does one thing: `_suppressCardTap = true`. It is
not a reorder route — it suppresses the card's own tap so pressing the handle does not also select the
floor.

**Why a keyboard user cannot reach it, stated by the file itself.** `FloorSidebarItem.xaml:100-108`, written
by 6b-iv when it made `ExpandToggle` a `Button` for `MASTER-164`:

> As a Label it took no keyboard focus on WinUI — only Control-derived classes do — and the card body it
> points at is a VerticalStackLayout, which takes no focus either and cannot be made to:
> IsTabStop/TabIndex/ITabStopElement are absent from .NET MAUI 10's public API and MAUI 9 removed layout
> focus deliberately.

The drag handle is a `Label`. **The same sentence that justified converting `ExpandToggle` applies to it
unchanged.** `ApplyExpanded` (`FloorSidebarItem.xaml.cs:290`) additionally carries a written guard
against reverting `ExpandToggle` to a `Label`, annotating the type so a revert fails as `CS0029` rather than
regressing silently — which is a precedent for how this codebase defends a focus-bearing element.

**Reorder call sites.** `grep -rn "ReorderFloor" --include=*.cs --include=*.xaml src/`, excluding `obj/` and
`bin/`. In production App code `ReorderFloorCommand.ExecuteAsync` is reached from exactly **three** sites,
all in `DungeonDetailPage.xaml.cs` — `:861`, `:2306`, `:2426` — and all three are inside drop handlers. The
entry says "both `ReorderFloorCommand` call sites"; there are **three**. The entry's conclusion is
unaffected — all three are pointer-driven — but the count in its prose is wrong, and that is recorded here
rather than restated.

**Below the App layer the operation is fully built and tested**, so a keyboard route needs no new
Application or Domain work: `ReorderFloorCommand` / `Handler` / `Validator` exist under
`src/DungeonMaster.Application/Features/Dungeons/Commands/ReorderFloor/`, `Dungeon.ReorderFloor` at
`src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:103`, with handler, validator and aggregate
tests already present.

**The page's key handler is global, not focus-scoped.** `OnKeyDown`
(`DungeonDetailPage.xaml.cs:362`) is fed by `Plugin.Maui.KeyListener`'s `KeyboardBehavior`, added to the
page's `Behaviors` at `:238-243` under the comment *"fires regardless of which element has focus, solving
the GraphicsView focus-sink problem on Windows"*. It switches on `char.ToLower(e.KeyChar)` over the tool
shortcuts `d m h p s e r [ ]` and declares **no reorder key**. Modifiers are available on this event and are
already used once — `e.Modifiers.HasFlag(KeyboardModifiers.Control)` at `:446`.

**The consequence for design, and it is the session's open question.** Because that handler is
focus-independent, putting reorder keys into it would make them fire whenever the detail page is up. A
keyboard reorder route therefore needs either a focus-bearing host on the handle itself, or an explicit
"the floor sidebar holds focus" gate. `Plugin.Maui.KeyListener` is pinned at `1.0.0-preview4`
(`Directory.Packages.props:34`) and `KeyboardBehavior` is a `Behavior` that can attach to any
`VisualElement`, but **whether attaching it to a control scopes its events to that control is not
established by anything in this repository** — the only attachment site is the page. That is the one
unknown the decomposition must settle by measurement rather than assumption.

**One frozen-contract note, now expired but worth knowing.** `FloorSidebarItem.xaml:235-236` records
*"The DragGestureRecognizer stays on THIS Label and its two handler names are unchanged — contract C1
section 3 freezes both."* That was 6b-iv's contract and 6b-iv is closed, so it binds nothing now. It does
record a real decision, and any change that rehosts the drag gesture is changing something a previous
session deliberately pinned.

---

## 4. `MASTER-098` / `UIKIT-011` — DISPOSITION CHANGED

**The entry's remedy appears substantially complete, and no record says so.** `MASTER-098` is carried as
`part done` with its work outstanding. Measured against the code, **every specific defect `UIKIT-011` names
is now false, and all three clauses of its suggested direction are satisfied.**

> **[AMENDED 2026-09-16, by the orchestrator, after T2 re-derived this section independently.
> Classification: MEASUREMENT ERROR — this paragraph OVERSTATED.]**
>
> **"Every specific defect is now false" is wrong. One claim survives.**
> `docs/audit/04-app-controls-and-theme.md:533` — *"it forces the host page to track selection itself and
> push it back in"* — **is still true in substance.** `DungeonDetailPage` still owns the fact and still
> writes it into the child control: `item.IsExpanded` at `:832`, `:965` and `:2204`, and area selection
> through `SetAreaEditing` at `:2191`. **Nothing binds `IsExpanded` or `IsSelected` to a view-model
> property** — `grep -rn "IsExpanded=\"{Binding\|IsSelected=\"{Binding" --include=*.xaml src/` returns
> **nothing**. What changed is the *mechanism* — the push now lands on a bindable property instead of an
> imperative setter that painted six colours — **not the ownership**, and ownership is what that clause
> asserts.
>
> Two softer corrections from the same re-derivation: `UIKIT-011`'s suggested-direction clause (c) is only
> **partly** satisfied (three of five named public members deleted, two survive), and `FocusNameEditor()` is
> the **weak** survivor — its first statement `IsExpanded = true` (`:264`) is a bare property write the
> caller already makes directly at three other sites, so only the `SelectRequested` raise resists reduction.
> `SetAreaEditing` is genuinely irreducible; `FocusNameEditor` is not.
>
> Also found: the record's own `locations:` citations at `:514-515` are **both stale** — they now land on
> `ICommand` bindable-property accessors in both files.
>
> **Consequence for scope, and it is a Gate 3 decision rather than a measurement.** `MASTER-098` is
> **not** cleanly closeable on this evidence. The remedy is substantially but not wholly landed, which is
> what `part done` already says. Verified independently by the orchestrator before accepting it: the three
> write sites and the empty binding grep above were re-run in the worktree.
>
> **The mechanism of the error is the one this project keeps recording.** T2 was dispatched with an explicit
> instruction not to take this section as its source and to say so if it disagreed. It disagreed, and it was
> right. Had it been handed this paragraph as established fact, the overstatement would have propagated into
> the closure artifact and from there into the audit index.

`UIKIT-011`'s source record is `docs/audit/04-app-controls-and-theme.md:504-538`. Taking its claims one at
a time, against `grep -n` over each control:

| `UIKIT-011` claim | Measured now | Evidence |
|---|---|---|
| `AreaSidebarItem` carries `public void SetEditing(bool)` backed by a private `_isSelected` | **False.** No `SetEditing`, no `_isSelected`. | `AreaSidebarItem.xaml.cs` — `grep -n "SetEditing\|_isSelected"` returns nothing |
| `FloorSidebarItem` carries `Deselect()`, `Select()`, `SetExpanded` over a private `_isExpanded` | **False.** None of the three exists. | `FloorSidebarItem.xaml.cs` — absent from the full `public`/`private void` listing |
| Neither selection nor expansion is a bindable property | **False.** Both are. | `AreaSidebarItem.xaml.cs:37` `IsSelectedProperty`; `FloorSidebarItem.xaml.cs:41` `IsExpandedProperty` |
| Neither control declares a `VisualStateManager` group | **False.** Both do. | `AreaSidebarItem.xaml:41` group `SelectionStates` (`Normal`/`Hovered`/`Selected`); `FloorSidebarItem.xaml:40` group `FloorStates` (`Normal`/`Hovered`/`Expanded`/`DragOver`) |
| The selected and expanded appearance "cannot be restyled from the theme dictionaries at all" | **False.** The state setters resolve theme tokens. | `FloorSidebarItem.xaml:42-64` — `DmSurfaceTavernRaised`, `DmSurfaceTavernRaisedHover`, `DmSurfaceTavernRaisedActive`, `DmBorderTavernBrush`, `DmBorderTavernHoverBrush`; `AreaSidebarItem.xaml:42-62` — `DmGoldHoverTint`, `DmAreaSelectedTint`, `DmAreaSelectedBorder`, `DmStrokeHairline` |

`AreaSidebarItem.xaml.cs:32-36` states the migration in its own words — the `IsSelected` property
*"replaces the private `_isSelected` field the appearance used to hang off"*.

**The two surviving imperative methods, and why they do not reopen the entry.** `UIKIT-011`'s suggested
direction asks to *"reduce the public methods to thin wrappers or delete them"*. Two remain on
`FloorSidebarItem`, each with exactly one external caller, and both are already thin wrappers over the new
bindable properties:

- `FocusNameEditor()` (`:262-267`) — sets `IsExpanded = true` and raises `SelectRequested`. Sole external
  caller `DungeonDetailPage.xaml.cs:2478`.
- `SetAreaEditing(Guid, bool)` (`:274-286`) — finds the child row by area id and sets `item.IsSelected`. Its
  own doc comment calls it *"a thin wrapper over the child's own `AreaSidebarItem.IsSelected`"* and gives
  the reason it cannot be a binding: the rows come from an item template, so the page has no handle on one
  and this control is the only thing that can reach a row by area id. Sole external caller
  `DungeonDetailPage.xaml.cs:2191`.

Both call-site counts from `grep -rn "FocusNameEditor\|SetAreaEditing" --include=*.cs --include=*.xaml src/`
excluding `obj/` and the declaring file.

**Two residues, neither of them `UIKIT-011`'s subject.** `AreaSidebarItem.xaml:44-46` and `:52-53` set
literal `Transparent` and `0` rather than tokens in the `Normal` and `Hovered` states. Transparency is
absence of colour rather than a themeable value, so this is noted, not claimed as a defect.

**What this means for scope, and it is a decision rather than a measurement.** On this evidence the correct
action for `MASTER-098` is **verify and close with commit evidence**, not implement. Closing it is
orchestrator work — entry closures cite commit hashes and a DIR task cannot invoke git. The remedy landed
across 5c and 6b-iv without either session reconciling the entry, which is the same
shipped-but-never-closed pattern `MASTER-163`, `MASTER-164` and `MASTER-174` are recorded as having at
`docs/audit/00-master-index.md:667`.

**This has not been proven by a running application.** Every claim above is a static read. Whether the
bindable properties and visual states actually paint correctly at runtime is exactly the kind of claim this
audit has twice recorded as substituting a source read for an observation. The observation pass this
session has available should cover it.

---

## 5. The drop placeholder — out of scope by decision

Cut at Gate 1. Its design question is unsettled: making the indicator always truthful means either mutating
the visual tree under the pointer mid-`DragOver` — the mechanism 6b-iv investigated and rejected, recorded
at `FloorSidebarItem.xaml.cs` against *"DragOver then DragLeave oscillation = stutter"* — or finding a
presentation that indicates position without reparenting anything. It stays in `.dir-sdlc/backlog.md`.
