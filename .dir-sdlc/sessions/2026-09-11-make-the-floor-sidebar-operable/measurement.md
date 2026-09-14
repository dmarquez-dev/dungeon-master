# Measurement — session 6b-iv, the floor sidebar

All measurement below was taken against `chore/repo-audit` at **`b3b1641`** (full:
`b3b164103c63de5176c30690c1e5fe52fd34e4da`), before any task was dispatched and before any
implementation edit existed in the worktree.

The discipline this file exists to serve: **decompose from the measurement, never from the entry's
prose.** Of the five inherited entries, measurement changed the disposition of four.

---

## 1. Source measurement

Three read-only agents measured the five entries independently. Their findings, condensed to the
claims that decide task shape:

### `MASTER-164` — STILL TRUE AS WRITTEN

The only one of the five whose text survives measurement intact.

- Card body is `VerticalStackLayout` + `TapGestureRecognizer` — `FloorSidebarItem.xaml:79-86`
- Expand toggle is still a `Label` — `FloorSidebarItem.xaml:97-107`
- Zero `IsTabStop`, `Focusable`, `TabIndex`, `Focus()`, `Focused`/`Unfocused` in either file. The
  sole hit for the substring `Focus` is `FocusNameEditor` (`FloorSidebarItem.xaml.cs:261`), which
  focuses nothing — it sets `IsExpanded` and raises `SelectRequested`.
- The only tab stops in a collapsed floor card are the notebook and delete `Button`s. `AddAreaButton`
  (`:226-241`) is `IsVisible`-gated on `IsExpanded` (`:236`), so it is unreachable until expansion —
  and expansion is what cannot be reached by keyboard.

### `MASTER-163` — PARTIALLY TRUE; the "at all" clause is FALSE (see §2)

Source premises all hold:

- Handle is `Grid.Column="4"`, last of five — `FloorSidebarItem.xaml:164-179`
- An 18pt glyph `Label` (`DmIconSizeLarge`, `Resources/Theme/Typography.xaml:76`), no
  `WidthRequest`/`HeightRequest`/`MinimumWidthRequest` anywhere on it
- Border padding `DmPaddingCompact` = `8,4` (`Resources/Theme/Tokens.xaml:682`), `Margin="0,0,0,4"` —
  **zero right margin**, so the handle sits ~8px from the scroll content's right edge
- Host is an unpadded `ScrollView` with no reserved gutter — `DungeonDetailPage.xaml:666-669`
- WinUI scrollbar overrides are **colour-only** — `Platforms/Windows/App.xaml.cs:124-136`
- The `DragGestureRecognizer` is on that `Label` and nowhere else (`:177`), and both
  `ReorderFloorCommand` call sites are inside drop handlers (`DungeonDetailPage.xaml.cs:860`,
  `:2290`). `OnKeyDown` (`:361-430`) has no reorder key. **So the handle is the only reorder
  affordance in the source** — that part of the entry is correct and load-bearing.

### `MASTER-161` — NO LONGER TRUE as written

Entry text: "card body is tappable but carries no `HandCursorBehavior`". **It carries one**, at
`FloorSidebarItem.xaml:88`, with its own comment at `:76-78` naming MASTER-161. The remedy shipped in
5c. See §2 for whether it works.

### `MASTER-165` — PARTIALLY TRUE, and over-generalised from one data point

- The root cause is **not** a platform-type cast. `SetCursor` casts to `Microsoft.UI.Xaml.UIElement`
  (`Behaviors/HandCursorBehavior.cs:157`), which `Button`, `Label`, `Grid`, `Border` and
  `VerticalStackLayout` all satisfy on Windows; `BindingFlags.NonPublic` (`:116-122`) defeats
  `ProtectedCursor`'s protection with no subclass. Planning a fix around "the cast excludes
  non-Buttons" would be planning against a cause that does not exist.
- 25 XAML attachment sites across 8 files plus 5 C# construction sites were enumerated. Nine are
  non-`Button`.
- **A contradicting data point already existed:** `DmCard` attaches the behavior to itself, a
  `ContentView` root (`Controls/DmCard/DmCard.xaml.cs:116`), and that was human-observed working in
  Session 4 (`docs/audit/00-master-index.md:441`).
- The rest of the 2026-09-01 observation is explained by **absence, not inertness**:
  `AreaSidebarItem`'s root `Border` (`AreaSidebarItem.xaml:12`) carries `TapGestureRecognizer` and
  `HoverOrLongPressBehavior` (`:138`, `:141-145`) but **no `HandCursorBehavior` at all** — the file's
  only two attachments are the `Button`s at `:111` and `:131`. The floor name label
  (`FloorSidebarItem.xaml:111-118`) and area name label (`AreaSidebarItem.xaml:82-83`) likewise carry
  none of their own.

### `MASTER-166` — PARTIALLY TRUE; the wiring half is false

- Fully wired, no missing link: `AddGhostArea` defined `Graphics/FloorMapDrawable.cs:378-390`; one
  production call site `DungeonDetailPage.xaml.cs:1754`; `RaiseRedrawRequested` (`:499-500`)
  subscribed at `DungeonDetailPage.xaml.cs:77-78` calling `MapCanvas.Invalidate()`; plus a redundant
  direct `Invalidate()` at `:1755`; drawn from the same `_ghostAreas` field it writes
  (`FloorMapDrawable.cs:549-552` reading, `:388` writing, field at `:117`).
- **Four tests already exist** — `FloorMapDrawableStateTests.cs:69-74`, `:79-84`, `:148-160`;
  `FloorMapDrawableAreaAlphaTests.cs:96-110`. Any reading of this entry as "untested" is wrong.
- The open question is timing only: `ClearGhostAreas()` (`DungeonDetailPage.xaml.cs:1767`) runs after
  an awaited local-database write, and if that write returns inside one frame interval the ghost is
  composited away before it rasterizes.
- The audit record concedes its own evidence is negative: `00-master-index.md:572` records a 45-frame
  burst at 60ms and states "a negative capture is not proof". 60ms is ~4x too slow to sample a
  single-frame event on a 60Hz display.
- **Same pattern at a second site:** optimistic area move, `DungeonDetailPage.xaml.cs:1716-1727`.
  Not the same as the Place-tool "ghost object" (`FloorMapDrawable.cs:457-467`), which is a live
  cursor-follow preview and must not be folded in.

---

## 2. The running-application probe

`HandCursorBehavior` sets WinUI's non-public `ProtectedCursor`, which no other process can read. The
repository had already reached this conclusion and recorded it —
`.dir-sdlc/sessions/2026-08-28-the-control-kit/HANDOFF-session-6.md:104`: *"A `GetCursorInfo` probe
measures the apparatus, not the change. Cursor claims need a human."* Screenshots do not capture the
Windows cursor either. **So this was measured by a human observer, and no other instrument was
substituted for one.**

### Apparatus

A temporary probe varied the attach strategy by floor number so all three configurations were
hoverable in **one** launch rather than three rebuild-and-relaunch cycles, removing run-to-run
variation as an explanation for any difference:

- **A** (`Number % 3 == 1`) — as shipped: behavior on the inner `VerticalStackLayout`
- **B** (`Number % 3 == 2`) — behavior moved to the root `Border`, inner attachment removed. This is
  the remedy `00-master-index.md:571` prescribes
- **C** (`Number % 3 == 0`) — as shipped, plus an explicit transparent background on the card body

The floor name was prefixed with its configuration so the observer could tell the cards apart.
Built clean (`0 Warning(s), 0 Error(s)`) and run against the real local database, read-only:
"Test Dungeon #1", **11 floors**, which is also enough to make the sidebar scroll and put the overlay
scrollbar in play. The probe was reverted immediately afterwards; `git status` confirmed the worktree
clean at `b3b1641` before any task was dispatched.

### Result — cursor

**All three configurations showed the hand cursor, including A, the untouched as-shipped build.**

Consequences, and they are large:

- **`MASTER-161` is `done`, not `not done`.** The 5c remedy works at `b3b1641`. Its recorded status
  is wrong and the record must be corrected rather than the code re-fixed.
- **`MASTER-165`'s stated claim is false.** Its evidence rested on exactly one non-`Button` surface —
  this card body — and that surface works. Combined with `DmCard`'s `ContentView` root already
  observed working, there is no surviving evidence that the behavior fails on non-`Button` elements.
- The 2026-09-01 observation that produced both entries is now fully accounted for **without**
  needing 165: the floor card body works, and the area card body shows no hand because it has no
  behavior attached at all. That second half is settled by source and needs no further observation.

### Result — drag handle

Observed: **the drag started; the scrollbar did not block it, but made the handle very difficult to
grab; and no reorder happened.**

Two distinct findings, and they are not the same defect:

1. **`MASTER-163`'s "cannot be performed at all" is FALSE.** The real defect is an ergonomics /
   hit-target one: an 18pt glyph with zero right margin, competing with the overlay scrollbar. The
   remedy shape named at `00-master-index.md:569` — move the handle out of the gutter, or reserve the
   gutter — still addresses it; the severity claim does not survive.
2. **The drop does not reorder.** This is a live functional defect, it is *not* what `MASTER-163`
   describes, and no index entry records it. It is the more serious of the two.

Both findings bear on `MASTER-098`, whose status reads "the DragOver state is unverifiable while the
drag handle is unreachable (MASTER-163)". **The handle is reachable.** That premise is gone and 098's
deferral no longer rests on anything.

---

## 3. Documentation research — `MASTER-164`'s remedy

Escalation E1 asked whether to re-take the remedy the user declined at 5c's escalation E5. Researched
against official documentation, per the user's stated priorities: (1) best practice and common
convention, (2) most platform-agnostic solution.

- **Making the card body or root `Border` focusable is not viable.** `IsTabStop`, `TabIndex` and
  `ITabStopElement` do not exist in .NET MAUI 10's public API — the candidate doc URLs return 404 and
  the full `VisualElement` API page has zero matches. More decisive, **.NET MAUI 9 deliberately
  removed the loophole**: the what's-new page's own example is `<VerticalStackLayout Focus="...">`
  and states the handler "isn't executed because only input controls can gain focus". At the platform
  layer a `Border`/`Grid`/`StackPanel` is a `UIElement` but not a `Control`, and WinUI's keyboard
  accessibility guidance states only `Control`-derived classes support focus and tab navigation.
- **The glyph `Button` is what Microsoft explicitly prescribes for this defect.** Their keyboard
  accessibility page shows a pointer-only `Image` as "Don't do this" and the same element wrapped in
  a `Button` as "Do this instead" — which is this control's `VerticalStackLayout` +
  `TapGestureRecognizer` one-to-one. It is also the structure WinUI's own `Expander` uses
  (`PART_ExpanderToggleButton`, a real `ToggleButton`).
- **The objection that sank it at E5 is resolvable.** A 16x23 target fails the general 23x23 minimum,
  but Microsoft's touch guidance carves out this exact widget: *"It's acceptable for click targets to
  be smaller than the minimum size if that control has redundant functionality. For example, the
  progressive disclosure triangles used by the tree view control are only 6x9 pixels."* The chevron is
  redundant with the card-body tap. Keep the 16x23 **visual**, give it >=23x23 of **hit area** via
  `Padding`, and the header row keeps its height.
- **Two things documentation does not settle, treated as must-run rather than assumed:** whether
  MAUI's `MinimumWidthRequest`/`MinimumHeightRequest` actually override the WinUI default `Button`
  minimum on Windows (no documented number, no documented handler mapping), and whether the MAUI
  Community Toolkit `Expander` is keyboard-focusable at all — it is a tap-driven `ContentView` with
  no keyboard or UIA documentation on any of its pages, and it explicitly warns against interactive
  controls in its header, which this card's three buttons violate.
- **No cross-platform UIA `ExpandCollapse` surface exists in MAUI.** `SemanticProperties` exposes
  `Description`, `Hint` and `HeadingLevel` and nothing else. Expressing the pattern requires a
  Windows handler. A real `Button` at least inherits an `Invoke` peer for free.
- **Mac Catalyst caveat:** setting `SemanticProperties.Description` on a control with children makes
  iOS/Catalyst screen readers unable to reach the children. The card `Border` must therefore **not**
  acquire a `Description`, or the three header buttons disappear from VoiceOver.

---

## 4. Citation drift found while measuring

Recorded for the backlog sweep the handoff already carries as item 1.

| Citation | Says | Actually |
|---|---|---|
| `.dir-sdlc/backlog.md:225` | `RebuildFloorList`'s "imperative loop at `DungeonDetailPage.xaml.cs:717`" | declared at `DungeonDetailPage.xaml.cs:752` |
| `00-master-index.md:567` | recogniser at `FloorSidebarItem.xaml:33-35` | now `:84-86`; `:33-35` is a comment block |
| `00-master-index.md:567` | `DmCard.xaml.cs:93` | now `:116` |
| `00-master-index.md:569` | handle at `FloorSidebarItem.xaml:164-171` | now `:164-179` (gesture recognizers added) |
| `04-app-controls-and-theme.md:376`, `:481` | `GhostAreas` public `List<>`, separate `DrawGhostAreas(canvas)` | neither exists; private `_ghostAreas`, one merged `DrawAreas` loop at `FloorMapDrawable.cs:535-553` |

`MASTER-163` and `MASTER-164` have **no detailed source record** in `01-`..`06-`; they exist only in
`00-master-index.md` (`:286`, `:351`, `:352`, `:569`, `:570`, `:590`). The index says so itself at
`:590`. Same for `MASTER-161`/`165` (`:349`, `:353`, `:363`, `:461`, `:567`, `:571`, `:590`).

---

## 5. The synthetic drag probe — INCONCLUSIVE, apparatus limitation

After the reorder trace returned four candidates, the orchestrator attempted to settle candidate #1
("`DragOver` never runs on the target, so `Drop` never fires") by driving a real drag with synthetic
mouse input and capturing the target card mid-drag. The `DragOver` visual state is a gold stroke
(`FloorSidebarItem.xaml:59-63`) and, unlike a cursor shape, **is** visible in a screenshot.

Conditions: as-shipped build at `b3b1641`, rebuilt after the probe revert (`0 Warning(s), 0 Error(s)`),
window maximized at 1920x1080. **All 11 floors fit with no scrollbar**, which isolates the reorder
question from `MASTER-163`'s scrollbar question entirely.

Observed while the button was held over the target: target card shows **no** `DragOver` highlight, no
placeholder gap appears, and after release the order is unchanged with no error line.

**This does not settle candidate #1, and must not be recorded as if it did.** `DragStarted` sets
`item.Opacity = 0` (`DungeonDetailPage.xaml.cs:866-870`), and the source card remained at **full
opacity** for the whole sequence. So `OnDragStarting` (`FloorSidebarItem.xaml.cs:373`) never fired and
no MAUI drag session ever began: synthetic `mouse_event` input does not initiate WinUI
drag-and-drop. The probe measured the apparatus, not the application — the same failure mode
`HANDOFF-session-6.md:104` already recorded for `GetCursorInfo`.

Two consequences that are real:

1. It is evidence for E2's choice of the Appium harness. WinAppDriver injects input at a level native
   drag-and-drop accepts, which synthetic `mouse_event` does not.
2. **It is also a risk against that choice**, and the risk must be named before any criterion is
   frozen: if Appium's W3C pointer actions also fail to drive native WinUI DnD, then "an automated
   test proves the reorder works" is an unmeetable criterion. Any task owning it needs a stated
   fallback to human observation, decided at Gate 2 rather than discovered at review.

The human observation from §2 remains the only evidence about the real behaviour: the drag starts,
the scrollbar impedes grabbing rather than blocking it, and no reorder happens.

### Correction to §4, made during decomposition

The first row of the §4 table was itself misattributed when first written, by this session, in the
file whose subject is misattributed citations. It read "`HANDOFF-session-6b-iv.md` §4 —
`RebuildFloorList` at ~`:717`". That handoff contains no such citation:

```
grep -n "RebuildFloorList\|717" .dir-sdlc/sessions/2026-09-11-measure-then-close-the-test-gaps/HANDOFF-session-6b-iv.md
```

returns nothing at `b3b1641`. The real home is **`.dir-sdlc/backlog.md:225`**, inside the carried item
about `MASTER-094`/`MASTER-108`, which says "still the imperative loop at
`DungeonDetailPage.xaml.cs:717`". Found by `dir-architect` during decomposition, verified by the
orchestrator re-running both commands, and corrected in the table above.

**This is the fourth consecutive session to produce a citation error while handling citation errors,
and the mechanism was the same every time: the claim was restated from working memory instead of
being re-derived from the file.** A sweep that had trusted the original row would have edited a
handoff that never contained the defect and left `backlog.md:225` untouched.

---

## 6. The four reorder root-cause candidates, enumerated

Recorded here because T1 raised it as a contract gap and was right to: §2 and the frozen contract both
referred to "four candidates" while only candidate #1 was written down anywhere a task could read. The
omission is the orchestrator's, not the architect's — the trace produced all four and only the first
was carried into the artifacts. A task asked to discriminate between four candidates it cannot read
has been handed an unmeetable criterion, which is this project's recorded failure mode.

The four, as the read-only trace ranked them at `b3b1641`:

1. **`Drop` never reaches either handler, because `AcceptedOperation` is never set on the element under
   the pointer at mouse-up.** In WinUI `Drop` fires only if the last `DragOver` left
   `AcceptedOperation != None`. `ShowDragPlaceholder` mutates `FloorListContainer.Children` from
   *inside* the `DragOver` handler (`DungeonDetailPage.xaml.cs:2239-2243` remove, `:2309` insert),
   moving the target 48px in the `afterTarget == false` branch. The code documents the resulting
   instability itself at `FloorSidebarItem.xaml.cs:395-397` ("DragOver then DragLeave oscillation =
   stutter").
2. **`Drop` fires, but the `"FloorId"` property does not survive the round-trip**, and the guard at
   `FloorSidebarItem.xaml.cs:415-418` discards it in silence. The repo's key matches at both ends
   (`:377` writes, `:415` reads), so the failure would be in MAUI's `_XFPropertes_DONTUSE` transport.
3. **The drop lands on neither handler's element** — the inner `VerticalStackLayout` or empty
   container space. The `DropGestureRecognizer` is on the outer `Border`
   (`FloorSidebarItem.xaml:249-252`); `FloorListContainer` (`DungeonDetailPage.xaml:668`) has no drop
   recognizer, so anything landing between cards is discarded with no handler and no trace.
4. **(low) The placeholder's `Drop` handler should fire but the placeholder was removed before
   mouse-up.** `ClearDragPlaceholder` (`:2312-2323`) is reachable from `DragEnded` (`:876-882`) via
   `OnDropCompleted` (`FloorSidebarItem.xaml.cs:382`).

T1, reading MAUI 10.0.41's own source, eliminated two theory *classes* by evidence (the drop target is
wired — `AllowDrop` defaults true; and the payload has a transport —
`e.Data.Properties["_XFPropertes_DONTUSE"]`), and added a candidate the trace did not have:

- **A. Windows never delivers `DragOver`/`Drop` at all** for an unpackaged app
  (`WindowsPackageType=None`, `DungeonMaster.App.csproj:74`), per dotnet/maui#26887. **Weakened** — a
  Microsoft verifier commented "Cannot repro on Windows platform," and the report concerns drags
  originating outside the app.
- **C. The platform hands the app an empty package**, which C1 §1 row 1 requires be treated as a
  silent return.

T1 also corrected a claim of its own before it reached code: its first draft asserted
`DropEventArgs.Data` can be null and that `OnDrop`'s dereference was the defect. `DropEventArgs.cs:29-33`
reads `Data = view ?? new DataPackageView(new DataPackage())`, so it cannot. The null-conditional stays
in the code documented as guarding a contract rather than a live path.

---

## 7. Correction to §3 — BOTH figures at `00-master-index.md:570` are wrong

`docs/audit/00-master-index.md:570` states `MASTER-164`'s remedy cost as *"a header-row height change,
since those siblings render **55x55** against the `Label`'s **16x23**"*. T2 measured both in the
running application, at a maximized 1920x1080 window across all 11 floors of Test Dungeon #1:

| | index claims | measured by T2 |
|---|---|---|
| sibling `Button`s (notebook, delete) | 55x55 | **44x44** |
| `ExpandToggle` `Label` | 16x23 | **13x19** |
| header row height | implied to grow | **44, unchanged before and after** |

**§3 of this file repeated the `16x23` figure**, inherited from the index without re-measuring it, in
the sentence beginning "The objection that sank it at E5 is resolvable." That reasoning still holds —
a sub-minimum disclosure target is sanctioned by the redundancy carve-out, and the shipped hit area is
44x44 either way — but the number quoted in it was the index's, not a measured one.

**§3 does NOT contain `55x55`.** `contracts/audit-record-allocation.md` correction 11 asserts that it
does; that assertion is wrong, and is itself an instance of the pattern this session keeps recording —
a claim about a file, restated rather than re-derived. Verified by
`grep -n "55x55" .dir-sdlc/sessions/2026-09-11-make-the-floor-sidebar-operable/measurement.md`
returning nothing.

**Correction 11 must therefore cover both figures, not one.** The load-bearing point is unchanged and
still the important half: `55x55` is what the user's sub-session 5c escalation-E5 refusal rested on,
and the remedy was deferred on a measurement nobody re-took until this session.
