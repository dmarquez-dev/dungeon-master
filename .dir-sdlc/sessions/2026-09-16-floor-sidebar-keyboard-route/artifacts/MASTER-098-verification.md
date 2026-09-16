# `MASTER-098` / `UIKIT-011` — what is true of the code now

Written by DIR task `T2` of session `2026-09-16-floor-sidebar-keyboard-route`, under contract C3.
Its reader is the orchestrator at Gate 3. It is written to be read on its own: nothing below depends
on having `measurement.md` open beside it.

**What this document is.** `UIKIT-011` is a record of five or six specific statements about two
control files. This document takes those statements one at a time, re-reads the code they describe,
and writes down what the code says now, the command that produced that answer, and the file and line
it was read from.

**What this document is not.** It states no disposition for `MASTER-098`. Whether the entry is
closed, and on the strength of which commits, is Gate 3's decision and not this document's. See
also §5, which is the limit of what a source read can establish.

---

## 0. How to reproduce anything below

Every command in this file was run from the worktree root **`C:/dm-wt/6b-vi`**, and every
`path:line` citation is relative to that root. A citation can be checked one at a time with

```
sed -n '<line>p' <path>
```

Call-site counts use a **bare member name** grep over `src/`, excluding `obj/`, `bin/` and the file
that declares the member. A qualified `Type.Member` single-line grep is not used anywhere in this
file: this codebase formats many calls across two lines, and that form silently misses them
(contract C2 §5).

The source record being re-measured is `docs/audit/04-app-controls-and-theme.md:504-538`; line 504
is its heading:

```
$ sed -n '504p' docs/audit/04-app-controls-and-theme.md
### UIKIT-011 -- Sidebar items expose visual state through imperative methods, not bindable properties
```

The two files under test are
`src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml{,.cs}` and
`src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml{,.cs}`.

---

## 1. Claim-by-claim

Ten rows. The record's prose carries nine distinct assertions plus a suggested direction; all are
taken, not only the five that contract C3 §3.2 names as the minimum.

### 1.1 — `AreaSidebarItem` carries `public void SetEditing(bool)` backed by a private `_isSelected`, and paints stroke, thickness and background inline

*Claimed at* `docs/audit/04-app-controls-and-theme.md:521-522`.

| Field | Content |
| --- | --- |
| **Claim** | `AreaSidebarItem` carries `public void SetEditing(bool)`, backed by a private `_isSelected` field, and paints stroke, thickness and background inline. |
| **True now** | **False on all three parts.** No member named `SetEditing` and no field named `_isSelected` exists anywhere under `src/`. The state is the bindable property `IsSelectedProperty` / `IsSelected`. The only private field left on the control is `_isHovered`. No colour, stroke or thickness is assigned in the code-behind at all: the sole appearance statement is one `VisualStateManager.GoToState` call in `UpdateVisualState`. |
| **Command** | `grep -rn "SetEditing\|_isSelected" src --include=*.cs --include=*.xaml \| grep -v "/obj/" \| grep -v "/bin/"` — every surviving hit is a comment or doc comment describing the removal (`AreaSidebarItem.xaml:22`, `:29`, `AreaSidebarItem.xaml.cs:32`, `src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:351`). No declaration and no call site. |
| **File and line** | `src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml.cs:37` — `public static readonly BindableProperty IsSelectedProperty =`; `:111` — `public bool IsSelected`; `:96` — `private bool _isHovered;` (the one remaining private field); `:245` — `private void UpdateVisualState() =>`. |

### 1.2 — `FloorSidebarItem` carries `FocusNameEditor()`, `Deselect()`, `Select()` and `SetAreaEditing(Guid, bool)` over a private `_isExpanded`, with `SetExpanded` painting surface and border directly

*Claimed at* `docs/audit/04-app-controls-and-theme.md:522-524`.

| Field | Content |
| --- | --- |
| **Claim** | Four public methods over a private `_isExpanded`, with `SetExpanded` painting surface and border directly. |
| **True now** | **Mostly false.** `Deselect()`, `Select()`, `SetExpanded` and the field `_isExpanded` do not exist. Two of the four methods survive — `FocusNameEditor()` and `SetAreaEditing(Guid, bool)` — and §2 takes each of them separately. Expansion is the bindable property `IsExpandedProperty` / `IsExpanded`. Nothing in the code-behind writes a colour: the sole appearance statement is one `GoToState` call in `UpdateVisualState`. |
| **Command** | `grep -rn "SetEditing\|Deselect\|_isExpanded\|SetExpanded" src --include=*.cs --include=*.xaml \| grep -v "/obj/" \| grep -v "/bin/"` — the `_isExpanded` and `SetExpanded` hits are comments recording the removal (`FloorSidebarItem.xaml:25`, `:32`, `:33`, `FloorSidebarItem.xaml.cs:36`); the two `Deselect` hits are in `DungeonDetailPage.xaml.cs:1316` and `:1819` and are about deselecting a **palette item**, unrelated to this control. No declaration of any of the four names. |
| **File and line** | `src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:41` — `public static readonly BindableProperty IsExpandedProperty =`; `:163` — `public bool IsExpanded`; `:319` — `private void UpdateVisualState() =>`. |

### 1.3 — Neither selection nor expansion is a bindable property

*Claimed at* `docs/audit/04-app-controls-and-theme.md:524-525`.

| Field | Content |
| --- | --- |
| **Claim** | Neither selection nor expansion is a bindable property. |
| **True now** | **False.** Both are `BindableProperty` declarations with CLR accessors, and `IsExpanded` is additionally consumed as a live binding source inside its own markup (three `IsVisible` bindings). |
| **Command** | `grep -rn "IsExpanded\|IsSelected" src/DungeonMaster.App --include=*.xaml --include=*.cs \| grep -v "/obj/" \| grep -v "/bin/"`, then reading the declaring lines directly with `sed -n`. |
| **File and line** | `AreaSidebarItem.xaml.cs:37` and `:111`; `FloorSidebarItem.xaml.cs:41` and `:163`; `FloorSidebarItem.xaml:264` — `IsVisible="{Binding IsExpanded, Source={x:Reference Root}}"` (one of three identical bindings, the others at `:273` and `:312`). |

### 1.4 — Neither control declares a `VisualStateManager` group

*Claimed at* `docs/audit/04-app-controls-and-theme.md:525`.

| Field | Content |
| --- | --- |
| **Claim** | Neither control declares a `VisualStateManager` group. |
| **True now** | **False.** Both declare one. `AreaSidebarItem` declares `SelectionStates` (`Normal` / `Hovered` / `Selected`); `FloorSidebarItem` declares `FloorStates` (`Normal` / `Hovered` / `Expanded` / `DragOver`). Both are custom-named rather than `CommonStates`, deliberately and with the reason stated in the markup: MAUI drives `CommonStates` itself and its automatic `PointerOver` would outrank the selected/expanded state whenever the pointer rested on the row. |
| **Command** | `grep -rln "VisualStateGroupList" src/DungeonMaster.App/Controls --include=*.xaml` — a single bare token, so nothing depends on how a line is wrapped. |
| **File and line** | `src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml:41` — `<VisualStateGroup x:Name="SelectionStates">`; `src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml:40` — `<VisualStateGroup x:Name="FloorStates">`. The naming rationale is at `AreaSidebarItem.xaml:25` and `FloorSidebarItem.xaml:28`. |

### 1.5 — `DmCard` is the only control in the kit that does (`Controls/DmCard/DmCard.xaml:18-35`)

*Claimed at* `docs/audit/04-app-controls-and-theme.md:526`. This claim is not in contract C3 §3.2's
list of five; it is added here because reading the record turned it up.

| Field | Content |
| --- | --- |
| **Claim** | `DmCard` is the only control in the kit declaring a `VisualStateManager` group, at `DmCard.xaml:18-35`. |
| **True now** | **The exclusivity is false; the citation still resolves.** Three of the ten XAML controls under `Controls/` declare a group: `DmCard`, `AreaSidebarItem`, `FloorSidebarItem`. `DmCard`'s group is still exactly where the record says — `:18` opens `<VisualStateManager.VisualStateGroups>`, `:35` closes it, and `:20` declares `CommonStates` with `Normal` and `PointerOver`. |
| **Command** | `grep -rln "VisualStateGroupList" src/DungeonMaster.App/Controls --include=*.xaml \| wc -l` → **3**; `ls src/DungeonMaster.App/Controls/*/*.xaml \| wc -l` → **10** (`ls src/DungeonMaster.App/Controls \| wc -l` returns **12**, but two of those entries are loose `.cs` files, not controls). |
| **File and line** | `src/DungeonMaster.App/Controls/DmCard/DmCard.xaml:18` — `<VisualStateManager.VisualStateGroups>`; `:20` — `<VisualStateGroup x:Name="CommonStates">`. Plus the two from §1.4. |

### 1.6 — The selected and expanded appearance cannot be restyled from the theme dictionaries at all — it is hard-wired in code-behind

*Claimed at* `docs/audit/04-app-controls-and-theme.md:530-532`. This is the record's "why it
matters", and the sharpest of its claims.

| Field | Content |
| --- | --- |
| **Claim** | The selected and expanded appearance cannot be restyled from the theme dictionaries at all; it is hard-wired in code-behind. |
| **True now** | **False.** Every setter in the `Selected` state and every setter in the `Expanded` state is a `StaticResource` reference into `Resources/Theme/Tokens.xaml`, and each of those keys is defined there. Nothing in either code-behind assigns a colour, a brush or a thickness. Changing `DmAreaSelectedTint` in the token dictionary restyles the selected area row; changing `DmSurfaceTavernRaisedActive` restyles the expanded floor card. |
| **Command** | `grep -n "VisualState\|Setter Property\|StaticResource Dm" <each control>.xaml`, then `grep -rn "x:Key=\"DmAreaSelectedTint\"\|x:Key=\"DmAreaSelectedBorder\"\|x:Key=\"DmStrokeHairline\"\|x:Key=\"DmSurfaceTavernRaisedActive\"" src/DungeonMaster.App/Resources` to confirm each key resolves. |
| **File and line** | Selected row: `AreaSidebarItem.xaml:56` (`<VisualState x:Name="Selected">`), `:58` `BackgroundColor` = `{StaticResource DmAreaSelectedTint}`, `:59` `Stroke` = `{StaticResource DmAreaSelectedBorder}`, `:60` `StrokeThickness` = `{StaticResource DmStrokeHairline}`. Expanded card: `FloorSidebarItem.xaml:53` (`<VisualState x:Name="Expanded">`), `:55` `BackgroundColor` = `{StaticResource DmSurfaceTavernRaisedActive}`, `:56` `Stroke` = `{StaticResource DmBorderTavernHoverBrush}`. Token definitions: `src/DungeonMaster.App/Resources/Theme/Tokens.xaml:257` (`DmAreaSelectedBorder` = `#C8A040`), `:258` (`DmAreaSelectedTint` = `#18C8A040`), `:186` (`DmSurfaceTavernRaisedActive` = `#6A8B5E1A`), `:574` (`DmStrokeHairline` = `1`). |

### 1.7 — It forces the host page to track selection itself and push it back in

*Claimed at* `docs/audit/04-app-controls-and-theme.md:533`. **This is the one claim in the record
that survives**, and it is flagged here rather than buried, because it is the only place where the
re-measurement does not simply falsify the record.

| Field | Content |
| --- | --- |
| **Claim** | The design forces the host page to track selection itself and push it back in. |
| **True now** | **Still true in substance.** The mechanism changed; the ownership did not. `DungeonDetailPage` still decides which floor is expanded and writes `IsExpanded` directly on the child control at three sites, and still pushes area selection in through `SetAreaEditing`. What changed is that the push now lands on a bindable property (settable through a style, monitorable, a legal binding target) rather than on an imperative setter that painted six colours. Nothing in the tree binds `IsExpanded` or `IsSelected` to a view-model property; the page remains the owner of the fact. |
| **Command** | `grep -rn "IsExpanded\|IsSelected" src/DungeonMaster.App --include=*.xaml --include=*.cs \| grep -v "/obj/" \| grep -v "/bin/"`, excluding the two declaring files — every remaining C# hit is a page-side write. |
| **File and line** | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:832` — `item.IsExpanded = false;`; `:965` — `item.IsExpanded = true;`; `:2204` — `other.IsExpanded = false;`; `:2191` — `floorItem.SetAreaEditing(area.Id, area.Id == _editor.FocusedAreaId);`. |

### 1.8 — `docs/TODO.md` already reports the sidebar-rebuild cost

*Claimed at* `docs/audit/04-app-controls-and-theme.md:534`. Also not in C3 §3.2's list; added on
reading.

| Field | Content |
| --- | --- |
| **Claim** | `docs/TODO.md` already reports the sidebar-rebuild cost. |
| **True now** | **True, unchanged.** `docs/TODO.md` carries a numbered item describing `LoadDungeonAsync` rebuilding the sidebar O(n) times per load, tearing down and recreating every `FloorSidebarItem` and every nested `AreaSidebarItem`. |
| **Command** | `grep -n "sidebar\|rebuild" docs/TODO.md` |
| **File and line** | `docs/TODO.md:201` — ``2. **`LoadDungeonAsync` rebuilds the sidebar O(n) times per load.** …`` |

### 1.9 — The record's two `locations:` citations

*Claimed at* `docs/audit/04-app-controls-and-theme.md:514-515`.

| Field | Content |
| --- | --- |
| **Claim** | The defect lives at `AreaSidebarItem.xaml.cs:137-153` and `FloorSidebarItem.xaml.cs:185-221`. |
| **True now** | **Both ranges are stale.** Neither now contains any imperative appearance code. `AreaSidebarItem.xaml.cs:137-153` is the `HoverEndedCommand` and `DeleteCommand` bindable-property accessors and their doc comments. `FloorSidebarItem.xaml.cs:185-221` is the `AreaHoverStartedCommand` / `AreaHoverEndedCommand` / `AreaDeleteCommand` / `AreaNotebookCommand` accessors and the start of the constructor's doc comment. A reader following either citation today lands on unrelated code. |
| **Command** | `sed -n '137,153p' src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml.cs` and `sed -n '185,221p' src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs` |
| **File and line** | `AreaSidebarItem.xaml.cs:141` — `public ICommand? HoverEndedCommand`; `FloorSidebarItem.xaml.cs:186` — `public ICommand? AreaHoverStartedCommand`. |

### 1.10 — The suggested direction, clause by clause

*Claimed at* `docs/audit/04-app-controls-and-theme.md:536-538`.

| Field | Content |
| --- | --- |
| **Claim** | Three clauses: (a) introduce `IsSelected` / `IsExpanded` bindable properties; (b) express the appearance as `CommonStates` / custom visual states in each control's XAML pointing at theme tokens; (c) reduce the public methods to thin wrappers or delete them. |
| **True now** | **(a) satisfied** — both properties exist under exactly those names (§1.3). **(b) satisfied** — both controls carry a custom visual-state group whose setters are theme-token `StaticResource` references (§1.4, §1.6); the clause's own wording permits custom states, which is what was chosen, for the `PointerOver` precedence reason the markup states. **(c) partly satisfied** — of the five public members the record names across the two controls, three are deleted outright (`SetEditing`, `Deselect`, `Select`) and two survive; §2 takes each survivor on its own terms. |
| **Command** | The commands of §1.3, §1.4, §1.6 and §2. |
| **File and line** | `AreaSidebarItem.xaml.cs:37`, `:111`; `FloorSidebarItem.xaml.cs:41`, `:163`, `:262`, `:274`; `AreaSidebarItem.xaml:41`, `:58-60`; `FloorSidebarItem.xaml:40`, `:55-56`. |

---

## 2. The two surviving imperative methods

Both are on `FloorSidebarItem`. `AreaSidebarItem` has no public methods left.

### 2.1 `FloorSidebarItem.FocusNameEditor()` — `FloorSidebarItem.xaml.cs:262`

| Field | Content |
| --- | --- |
| **What it does now** | Two statements: sets `IsExpanded = true`, then raises `SelectRequested`. Its name is now a misnomer — it focuses nothing and opens no editor; naming a floor happens in the notebook, which the doc comment at `:257-261` says in place. It writes no colour and touches no visual. |
| **Thin wrapper over a bindable property?** | **Half.** Statement one is a bare write to the `IsExpanded` bindable property. Statement two is an event raise, which a property write cannot express — the page uses `SelectRequested` to deselect every other floor, so the method is a property write **plus** a notification. |
| **External call-site count** | **1.** |
| **Count command** | `grep -rn "FocusNameEditor" src --include=*.cs --include=*.xaml \| grep -v "/obj/" \| grep -v "/bin/" \| grep -v "Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs" \| wc -l` → `1`. Bare member name, `src/` only, `obj/` and `bin/` and the declaring file excluded. |
| **Why it cannot be a binding** | It can be, in part — and that is worth saying plainly rather than defending. The `IsExpanded` half is already a property write the caller could make directly (`DungeonDetailPage.xaml.cs:832`, `:965` and `:2204` do exactly that). What survives the reduction is the `SelectRequested` raise, which is a notification and not state. This method is therefore the weakest of the two survivors against clause (c): it is a two-line convenience over one property write and one event, not an irreducible one. |
| **File and line** | Declaration `FloorSidebarItem.xaml.cs:262`; body `:264` — `IsExpanded = true;`. Call site `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:2478` — `lastItem.FocusNameEditor();`, inside a dispatched delay after `AddFloorCommand`. |

### 2.2 `FloorSidebarItem.SetAreaEditing(Guid, bool)` — `FloorSidebarItem.xaml.cs:274`

| Field | Content |
| --- | --- |
| **What it does now** | Walks `AreaContainer.Children`, finds the `AreaSidebarItem` whose `Area?.Id` matches the argument, and sets that child's `IsSelected` to the second argument. It writes no colour; the one assignment it makes is to a bindable property on the child, and the child's own `SelectionStates` group does the painting. |
| **Thin wrapper over a bindable property?** | **Yes** — a lookup plus a single property write, and nothing else. This is exactly the "thin wrapper" shape clause (c) asks for. |
| **External call-site count** | **1.** |
| **Count command** | `grep -rn "SetAreaEditing" src --include=*.cs --include=*.xaml \| grep -v "/obj/" \| grep -v "/bin/" \| grep -v "Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs" \| wc -l` → `1`. Same bare-member-name form. |
| **Why it cannot be a binding** | The area rows are generated by the area list's item template, so the page holds no reference to any of them. Reaching a row by area ID requires walking `AreaContainer.Children`, and only this control can do that. A binding would need the row's identity to be addressable from outside the template, which it is not. |
| **File and line** | Declaration `FloorSidebarItem.xaml.cs:274`; the single write `:280` — `item.IsSelected = editing;`. Call site `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:2191` — `floorItem.SetAreaEditing(area.Id, area.Id == _editor.FocusedAreaId);`, inside a loop over the selected floor's areas. |

---

## 3. What this session's own contract C1 adds to `FloorSidebarItem`

Disclosed here so the orchestrator does not have to discover it after the fact. This section is a
**design position taken from contract C1**, not a measurement: at the time this artifact was
written, neither member existed in the tree, and this document makes no claim that they do.

Contract C1 adds two members to `FloorSidebarItem`:

- **`IsDragHandleFocused`** (C1 §3) — `public bool IsDragHandleFocused => DragHandle.IsFocused;`. A
  get-only computed expression with no backing field. It is a read, not a setter, and it stores
  nothing, so it creates no second owner of any fact. It is outside `UIKIT-011`'s subject entirely.
- **`FocusDragHandle()`** (C1 §4.3) — a parameterless `void` method that moves keyboard focus to
  this floor's reorder handle.

**Why `FocusDragHandle` does not reopen the entry.** `UIKIT-011`'s subject, stated in its own
heading at `docs/audit/04-app-controls-and-theme.md:504`, is *visual state* exposed through
imperative methods — appearance driven by imperative setters instead of being bindable and
themeable. **Focus is an action, not appearance state.** Calling `FocusDragHandle()` moves the
keyboard focus ring; it does not set a value that the control then paints from, and there is no
"focused appearance" it configures. It has the same justified shape as the surviving
`SetAreaEditing` (§2.2): the `Button` lives inside this control's template, so the page has no
other handle on it, and a bindable property cannot express "do this now" in any case.

Nothing in C1 adds an imperative appearance setter to either control.

---

## 4. Two residues — noted, not defects

In `AreaSidebarItem`'s `SelectionStates` group, five setters carry literal values rather than theme
tokens:

| Where | Setters | Verified with |
| --- | --- | --- |
| `Normal` state | `AreaSidebarItem.xaml:44` `BackgroundColor="Transparent"`, `:45` `Stroke="Transparent"`, `:46` `StrokeThickness="0"` | `sed -n '44,46p' src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml` |
| `Hovered` state | `AreaSidebarItem.xaml:52` `Stroke="Transparent"`, `:53` `StrokeThickness="0"` | `sed -n '52,53p' src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml` |

**These are noted, and they are not defects.** Transparency is the absence of a colour, not a
themeable value: there is no token a theme could supply for "no background", and a
`DmTransparent` key would be an alias for nothing that every theme would have to define
identically. The same holds for a zero stroke thickness — it is the absence of a stroke.

The markup states in place, at `AreaSidebarItem.xaml:36-38`, why every state names all three
properties rather than only the ones it changes: a state naming a subset lets MAUI restore the rest
to the `Border`'s own attributes on entry, which is the class of bug that two mechanisms driving one
property used to produce. The literals in the `Normal` and `Hovered` states exist to satisfy that
completeness rule, not because a token was overlooked.

The `Hovered` state's one non-absent value **is** tokenised: `:51` sets
`BackgroundColor="{StaticResource DmGoldHoverTint}"`. So is every value in `Selected` (§1.6).

**This is not a reason to reopen the entry.**

---

## 5. The limit of this document

**Every claim above is a static source read.** Each was produced by opening a file and reading a
line, or by a `grep` over the source tree — never by running the application and never by observing
a pixel.

It follows that this document **does not establish**:

- that the `Selected`, `Expanded`, `Hovered` or `DragOver` visual states actually paint at runtime;
- that `VisualStateManager.GoToState` reaches the intended element and returns `true` rather than
  silently doing nothing on an unknown state name;
- that the theme tokens resolve to the colours their hex values suggest on screen;
- that the bindable properties propagate to the visual tree in the running app.

Those are observation-pass findings, and nothing here substitutes for them. Specifically, the
observations that would settle the runtime half are: a floor card selected and expanded, and an area
row selected, showing the `FloorStates` / `SelectionStates` colours rather than the `Normal` ones;
and the `Hovered` state on both controls, which is the one whose `AreaSidebarItem` setters are
literal rather than tokenised (§4).

A source read is evidence about what the code says. It is not evidence about what the application
does.
