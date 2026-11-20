# Contract C3 — the `MASTER-098` / `UIKIT-011` closure evidence artifact

Draft for Gate 2. Written by `dir-architect` in decomposition mode. Produced by `T2`; consumed by the
**orchestrator** at Gate 3, which is the only party that can close an index entry.

This file's absolute main-checkout path is
`C:/Users/dmarq/Development/Repos/dungeon-master/.dir-sdlc/sessions/2026-09-16-floor-sidebar-keyboard-route/contracts/C3-master-098-closure-evidence.md`.

---

## 1. Why this is an artifact and not an index edit

`measurement.md` §4 finds `MASTER-098`'s remedy **already substantially complete**: every specific
defect `UIKIT-011` names is now false, and all three clauses of its suggested direction are
satisfied. The remedy landed across sub-sessions 5c and 6b-iv without either reconciling the entry —
the same shipped-but-never-closed pattern `MASTER-163`, `MASTER-164` and `MASTER-174` are recorded as
having at `docs/audit/00-master-index.md:667`.

Closing the entry cites the commits that closed it, and **a DIR task cannot invoke git** (contract
C2 §4). So the split is:

| Work | Who |
| --- | --- |
| Re-measure each `UIKIT-011` claim against the code and write it down with its evidence | `T2`, into the artifact |
| Write the status, the disposition and the commit hashes into `docs/audit/00-master-index.md` | the **orchestrator**, at Gate 3, citing the artifact |

**`T2` must not touch `docs/audit/00-master-index.md` or `docs/audit/04-app-controls-and-theme.md`.**
Neither file is in its ownership set.

---

## 2. The artifact — path and audience — FROZEN

Exactly one file:

```
.dir-sdlc/sessions/2026-09-16-floor-sidebar-keyboard-route/artifacts/MASTER-098-verification.md
```

relative to the worktree root `C:/dm-wt/6b-vi`. Its absolute worktree path is
`C:/dm-wt/6b-vi/.dir-sdlc/sessions/2026-09-16-floor-sidebar-keyboard-route/artifacts/MASTER-098-verification.md`.
The `artifacts/` directory does not yet exist in the worktree; creating it is part of the task.

Its reader is the orchestrator at Gate 3, deciding whether to close `MASTER-098`. It must be legible
to that reader **without** `measurement.md` open beside it.

---

## 3. What it must contain — FROZEN

### 3.1 Every claim re-derived from the file, not restated

**This is the load-bearing rule of this contract.** This project has a documented history of one
specific failure: a claim about a file restated from another claim about that file instead of
re-derived from the file. 6b-iv produced twelve instances; 6b-v produced two more.

`measurement.md` §4 tells `T2` **which lines to look at**. It does not tell `T2` what they say.
Every row of the artifact is written by opening the cited file and reading the cited line, with the
command that produced it named beside it. A row whose evidence is "`measurement.md` §4 says so" is a
failed row.

### 3.2 A row per `UIKIT-011` claim

`UIKIT-011`'s source record is `docs/audit/04-app-controls-and-theme.md:504-538`. `T2` reads that
record itself and takes its claims one at a time. At minimum these five, each carrying **Claim /
Measured now / Command / File and line**:

1. `AreaSidebarItem` carries `public void SetEditing(bool)` backed by a private `_isSelected`.
2. `FloorSidebarItem` carries `Deselect()`, `Select()` and `SetExpanded` over a private `_isExpanded`.
3. Neither selection nor expansion is a bindable property.
4. Neither control declares a `VisualStateManager` group.
5. The selected and expanded appearance cannot be restyled from the theme dictionaries at all.

If reading `04-app-controls-and-theme.md:504-538` turns up a claim this list omits, **`T2` adds a row
for it** rather than treating the list as exhaustive.

### 3.3 The two surviving imperative methods

`UIKIT-011`'s suggested direction asks to *"reduce the public methods to thin wrappers or delete
them"*. `FloorSidebarItem.FocusNameEditor()` and `FloorSidebarItem.SetAreaEditing(Guid, bool)`
survive. For each, the artifact records: what it does now, whether it is a thin wrapper over a
bindable property, its external call-site count **with the command that produced it**, and why it
cannot be a binding.

Count call sites with a **bare member name** grep over `src/`, excluding `obj/`, `bin/` and the
declaring file — never a qualified single-line `Type.Member` form, which misses this codebase's
two-line call formatting (contract C2 §5).

### 3.4 Contract C1's own additions, disclosed

Contract C1 adds `FloorSidebarItem.IsDragHandleFocused` (a computed property) and
`FloorSidebarItem.FocusDragHandle()` (a method) in the same session. The artifact must say so and
say why `FocusDragHandle` does not reopen the entry: `UIKIT-011`'s subject is *appearance state*
being driven by imperative setters, and focus is an action, not appearance state.

`T2` writes this from contract C1 §4.3, and states it as a design position, not as a measurement.

### 3.5 The two residues, stated as residues

`AreaSidebarItem.xaml:44-46` and `:52-53` set literal `Transparent` and `0` rather than tokens in the
`Normal` and `Hovered` states. Transparency is the absence of a colour rather than a themeable value.
Record it as **noted, not a defect**, and say that. Do not let it become a reason to reopen.

### 3.6 The line that keeps the orchestrator honest

The artifact must end with an explicit statement that **every claim in it is a static source read**,
and that whether the bindable properties and visual states actually paint correctly at runtime is
**not** established by it. That is the observation pass's job (§4). This audit has twice recorded a
source read substituted for an observation; the artifact must not be the third.

### 3.7 What it must NOT contain

- No commit hash, and no sentence of the form "closed by …" or "fixed in …". Those are the
  orchestrator's (§1).
- No status word for the entry — not `done`, not `closed`, not `part done`. The artifact states what
  is **true of the code**; the disposition is Gate 3's.
- No claim about a line of a file it did not open.

---

## 4. The runtime half, which this artifact cannot supply

Two things the human observation pass settles for `MASTER-098`, named here so the orchestrator asks
for them rather than inferring them:

1. **The selected and expanded appearances actually paint.** A floor card selected and expanded, and
   an area row selected, showing the `FloorStates` / `SelectionStates` colours rather than the
   `Normal` ones.
2. **Hover paints too.** The `Hovered` state on both controls, which is the one whose setters are
   literal rather than tokenised on `AreaSidebarItem` (§3.5).

Neither is a criterion `T2` can meet, and no criterion may ask it to.

---

## 5. Ordering — `T2` runs before `T3`

`T2`'s rows cite line numbers in `FloorSidebarItem.xaml` and `FloorSidebarItem.xaml.cs`. Contract C1
§1 has `T3` rewriting a block of that same XAML, which shifts every line number below it.

The manifest therefore makes `T3` depend on `T2`. This is not a data dependency — it is line-number
stability, and it removes an entire class of silently-wrong citation. `T2` builds nothing and owns
one new file, so it costs the critical path nothing.
