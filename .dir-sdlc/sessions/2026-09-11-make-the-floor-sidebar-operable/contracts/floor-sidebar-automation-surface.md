# Contract C2 — the floor sidebar's automation surface

Frozen at Gate 2. Produced by `T2` and `T3`. Consumed by `T5` (the Appium harness) and, for its
citations, by `T6`.

`T5` runs out of process against the built `.exe` and can address an element only by the
`AutomationId` the XAML declares. `T5` is dispatched **after** `T2` and `T3` have run, but it is
written against this table and not against whatever it finds — so this table is what `T2` and `T3`
are required to leave behind.

`src/DungeonMaster.App.UiTests/README.md` records that that project "adds none and edits no `.xaml`
file". **That stays true.** `T5` owns no file under `src/DungeonMaster.App/`. If `T5` needs an
`AutomationId` that is not in this table, it escalates; it does not add one.

## 1. Required AutomationIds — FROZEN

Every id below must resolve by `AccessibilityId` on a `DungeonDetailPage` with at least one floor.

| AutomationId | Element | Required after this session | Owner |
| --- | --- | --- | --- |
| `FloorCard` | the card-body `VerticalStackLayout` | must exist; still the tap target that expands/collapses | `T2` |
| `FloorExpandToggle` | the expand chevron | must exist, **and must be a `Button`** after `T2` (it is a `Label` at `b3b1641`) | `T2` |
| `FloorNotebookButton` | per-floor notebook `Button` | unchanged | — |
| `FloorDeleteButton` | per-floor delete `Button` | unchanged | — |
| `FloorDragHandle` | the reorder handle `Label` | must exist; still a `Label` hosting the `DragGestureRecognizer` (contract C1 §3) | `T3` |
| `AddAreaButton` | the "+ Add area" `Button` | unchanged; still `IsVisible`-gated on `IsExpanded` | — |
| `AreaRow` | `AreaSidebarItem`'s root `Border x:Name="ItemBorder"` (`AreaSidebarItem.xaml:13`) | must exist and keep this id; `T3` attaches a `HandCursorBehavior` to this `Border` and changes nothing else about it | `T3` |

**Renaming, removing, or duplicating any id in this table is a contract change and requires an
escalation.** Duplicates matter as much as removals: `RebuildFloorList` adds one
`FloorSidebarItem` per floor, so every id above appears once per floor in the UIA tree and `T5` must
address them positionally within a card, never by expecting a unique match.

## 2. Semantic properties — FROZEN, with one platform trap

| Element | Property | Value |
| --- | --- | --- |
| `FloorCard` | `SemanticProperties.Description` | `"Floor card"` — static, kept |
| `FloorExpandToggle` | `SemanticProperties.Description` | `"Collapsed"` / `"Expanded"`, rewritten in `ApplyExpanded` alongside the glyph |
| `FloorDragHandle` | `SemanticProperties.Description` | `"Reorder floor"` |

**Trap, and it is load-bearing:** the card's root `Border` (`x:Name="FloorBorder"`) must **not**
acquire a `SemanticProperties.Description`. Setting a description on a control with children makes
iOS/Mac Catalyst screen readers unable to reach those children, which would remove the three header
buttons from VoiceOver. Documented in `measurement.md` §3.

`SemanticProperties` exposes only `Description`, `Hint` and `HeadingLevel` in MAUI. There is **no**
cross-platform UIA `ExpandCollapse` surface. No task is to invent one, and no criterion asserts one.

## 3. What `T5` may assert — FROZEN as the harness's scope

`T5`'s instrument is unproven for one thing and proven for the rest. The split is frozen here so it
is decided at Gate 2 rather than discovered at review (`measurement.md` §5).

**Assertable by Appium, and in scope for `T5`:**

- an element resolves by `AccessibilityId`, and its `IsOffscreen` / `Displayed` state
- an element's bounding rectangle (`Location`, `Size`), and therefore its width, height, and its
  right edge relative to the scroll viewport's right edge
- a keyboard tab-order walk: send `Tab` N times, read the focused element's `AutomationId` after
  each, and report the sequence
- `Click()` on an element and the state change that follows

**NOT assertable, and explicitly out of `T5`'s scope:**

- that a floor drag-and-drop reorder succeeds. Synthetic `mouse_event` input was measured on
  2026-09-11 **not** to initiate WinUI drag-and-drop at all (`measurement.md` §5: the source card
  stayed at full opacity, so `OnDragStarting` never fired). Whether Appium's W3C pointer actions can
  is **unproven**. `T5` may *attempt* it, but no `T5` acceptance criterion depends on it succeeding,
  and the reorder's verification falls to human observation under `T1`.
- the mouse cursor shape. `HandCursorBehavior` sets WinUI's non-public `ProtectedCursor`, which no
  other process can read, and screenshots do not capture the Windows cursor. Already recorded at
  `.dir-sdlc/sessions/2026-08-28-the-control-kit/HANDOFF-session-6.md:104`. This is why `T3`'s
  `MASTER-175` fix on `AreaRow` is verified by an orchestrator-run human observation and not here:
  `T5` may assert that `AreaRow` resolves and where it sits, and nothing at all about its cursor.

## 4. Prerequisites the harness cannot supply itself — FROZEN

`T5`'s tests cannot pass without an Appium server on `127.0.0.1:4723`, Windows Developer Mode, and a
built `DungeonMaster.App.exe`. Versions proven 2026-09-04 are tabulated in that project's
`README.md` and `T5` must read it before writing a line.

The project is deliberately **outside** `src/DungeonMaster.sln`. `T5` must not add it to the
solution: doing so makes a solution-wide `dotnet test` fail on every machine without the tooling.

Consequence for acceptance: a `T5` criterion may require the tests to **build** and to be
**runnable**, and may require a green run **when the prerequisites are present**, with an explicit
stated fallback naming who runs them. It must not require a green run unconditionally.
