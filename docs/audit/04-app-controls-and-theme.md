# App control kit and theme system -- code quality audit

## Scope

| Directory / glob | Files | Lines |
|---|---|---|
| `src/DungeonMaster.App/Controls/**` | 22 | 3100 |
| `src/DungeonMaster.App/Converters/**` | 2 | 143 |
| `src/DungeonMaster.App/Behaviors/**` | 3 | 251 |
| `src/DungeonMaster.App/Graphics/**` | 1 | 534 |
| `src/DungeonMaster.App/Theme/**` | 1 | 523 |
| `src/DungeonMaster.App/Resources/Theme/**` | 7 | 1417 |
| **Total** | **36** | **5968** |

**Files read: 36. Files cited in a record: 21.** The files-read count equals the scope table's
total. `.xaml` files are counted, read, and cited exactly as `.cs` files are.

Read and found sound -- no record raised against them (15 files):

- `Controls/AreaSidebarItem/AreaSidebarItem.xaml`, `Controls/DmCard/DmCard.xaml`,
  `Controls/DmConfirmPopup/DmConfirmPopup.xaml`, `Controls/DmDivider/DmDivider.xaml`,
  `Controls/DmDivider/DmDivider.xaml.cs`, `Controls/DmPopup/DmPopup.xaml`,
  `Controls/DmPopup/DmPopup.xaml.cs`, `Controls/DmSearchBar/DmSearchBar.xaml`,
  `Controls/FloorSidebarItem/FloorSidebarItem.xaml`, `Controls/NotebookAffordance.cs`,
  `Controls/NotebookView/NotebookView.xaml`.
- `Resources/Theme/Tokens.xaml`, `Resources/Theme/Typography.xaml`,
  `Resources/Theme/Controls.Parchment.xaml`, `Resources/Theme/Controls.Tavern.xaml`.

Not audited, with the reason for each:

- **The App root files (`App.xaml`, `App.xaml.cs`, `AppShell.xaml`, `AppShell.xaml.cs`,
  `MauiProgram.cs`), `Views/`, `ViewModels/`, `Extensions/`, `Platforms/`, `Properties/`** --
  out of scope: **T3 owns them** under layer code `UI`. `Views/DungeonDetailPage/PaletteThumb.cs`
  sits under `Views/` and is therefore T3's file; it is not recorded here.
  Where a record's consequence reaches a T3 file, this document names it as context only and does
  not cite it as a location.
- **Generated code.** `MauiXamlInflator=SourceGen` is enabled, so every `.xaml` in scope has a
  generated `*.xaml.g.cs` partial carrying `InitializeComponent` and the `x:Name` fields. That
  generated output is **excluded** -- it is not hand-written, is not under source control, and is
  not judged against the hand-written standard. The hand-written `.xaml` and `.xaml.cs` halves are
  both audited in full.
- **`src/DungeonMaster.Tests/App/Theme/`** -- T5 owns it. It is read here only to establish what
  the theme suite already guards, so that this document does not record something already
  enforced. Nothing in it is cited as a location.

## Method

- **Base commit:** `45495c1bd39a52dbc942eceab897b3c1ac553ee9` (branch
  `feat/phosphor-pack-integration`, worktree
  `dungeon-master-worktrees/2026-08-17-code-quality-audit`).
- **`dm-conventions` documents read:**
  - `backend/dotnet-format.md` -- 794 lines, **read in full**, before any style, naming,
    formatting, or XML-documentation record in this document was written. Every `S4` here names
    the numbered section of that document it rests on.
  - No other `dm-conventions` document was relied on. Its `frontend/` documents are
    React/TypeScript and are **not cited anywhere in this document**, per the session's settled
    convention gap: `dm-conventions` has no MAUI or XAML document.
- **Microsoft Learn pages consulted (all fetched, not recalled):**
  - https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/converters
  - https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/bindable-properties
  - https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/behaviors
  - https://learn.microsoft.com/en-us/dotnet/maui/user-interface/graphics/draw
  - https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/guidelines-for-collections
  - https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
  - https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.graphics.iimage
- **Tooling (all read-only):** `git log` for the base SHA; `ripgrep` over the scope for citation
  anchors, converter call sites, `VisualStateManager` usage and `x:Reference` bindings; PowerShell
  line counts and a leading-whitespace census across the layer's 19 `.cs` files. No build was run,
  the app was not launched, and no file under `src/` was written.
- **Prior-art check.** `src/DungeonMaster.Tests/App/Theme/` and `docs/TODO.md` were read before
  writing any theme record. Findings already enforced by that suite -- the token naming rule
  (`TokenNamingTests`), C#-to-XAML key parity (`ThemeTokenParityTests`), merge order and orphaned
  style keys (`ThemeDictionaryTests`), colour/font/geometry literals outside the dictionaries
  (`ThemeHygieneTests`, `GeometryScaleTests`), bindable-property defaults against tokens
  (`BindablePropertyDefaultsMatchTokensTests`), and token-to-target-property typing
  (`TokenTypeMatchesTargetPropertyTests`) -- are **not** recorded here. Items already scheduled in
  `docs/TODO.md`, notably `FloorSidebarItem`'s `_suppressCardTap` latch and the sidebar rebuild
  cost, are likewise not recorded as fresh findings.

## Summary

| Severity | Count |
|---|---|
| S1 | 0 |
| S2 | 5 |
| S3 | 15 |
| S4 | 6 |
| S5 | 3 |
| **Total** | **29** |

## Records

### UIKIT-001 -- FloorMapDrawable caches IDisposable images and never disposes them

```yaml
id: UIKIT-001
layer: UIKIT
severity: S2
category: disposal
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
locations:
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:21
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:26-27
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:299-318
effort: S
risk: low
independent: true
```

**What.** `_imageCache` holds `Microsoft.Maui.Graphics.IImage` instances created by
`PlatformImage.FromStream`. `IImage` derives from `IDisposable`. `FloorMapDrawable` creates and
owns every entry, does not implement `IDisposable` itself, and `ClearObjectImageCache()` calls
`_imageCache.Clear()` -- dropping the references without disposing them.

**Why it matters.** The Microsoft dispose guidance states that "if your class owns an instance of
another type that implements `IDisposable`, the containing class itself should also implement
`IDisposable`", because a class that instantiates a disposable and stores it as an instance member
is responsible for its cleanup. On Windows a `PlatformImage` wraps a Win2D bitmap holding GPU
memory, so each `ClearObjectImageCache()` abandons the whole decoded set to non-deterministic
finalisation. The TODO note at line 291 says the cache is deliberately cleared once the
`GraphicsView` handler is ready, so this path is reached in normal use, not only at shutdown.

**Suggested direction.** Make `FloorMapDrawable` implement `IDisposable`, dispose each cached
image in `ClearObjectImageCache()` before clearing, and dispose the remainder from `Dispose()`.
The owning page would then dispose the drawable when the page's handler is torn down.

### UIKIT-002 -- Image loading blocks on async file I/O from inside the Draw call path

```yaml
id: UIKIT-002
layer: UIKIT
severity: S2
category: async
authority: practice
citation: Stephen Cleary, Concurrency in C# Cookbook 2e, ch. 1 "Async Basics" -- "Avoid blocking on async code"
locations:
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:304
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:354
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:447
effort: M
risk: medium
independent: true
```

**What.** `LoadOrGetImage` calls
`FileSystem.OpenAppPackageFileAsync(path).GetAwaiter().GetResult()` and then decodes the stream
with `PlatformImage.FromStream`. It is reached from `DrawPlacedObject` (line 354) and
`DrawGhostObject` (line 447), both of which run inside `Draw`, which the `GraphicsView` invokes on
the UI thread for every frame.

**Why it matters.** Blocking a synchronous UI-thread call on an asynchronous operation is the
pattern Cleary names as the standard deadlock and responsiveness hazard: the render pass cannot
complete until package-file I/O and image decoding have finished, so a cold cache stalls the frame
and, on any platform whose `FileSystem` implementation resumes on a thread-pool continuation, can
deadlock outright. The file's own TODO at line 291 states the same conclusion and describes the
fix as pre-loading asynchronously.

**Suggested direction.** Pre-populate `_imageCache` from an asynchronous load started after the
`GraphicsView` `Loaded` event, and have `LoadOrGetImage` become a pure cache read that returns
`null` on a miss, so `Draw` never touches the file system.

### UIKIT-003 -- HoverOrLongPressBehavior never detaches the pan recogniser it adds

```yaml
id: UIKIT-003
layer: UIKIT
severity: S2
category: disposal
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/behaviors
locations:
  - src/DungeonMaster.App/Behaviors/HoverOrLongPressBehavior.cs:23-25
  - src/DungeonMaster.App/Behaviors/HoverOrLongPressBehavior.cs:28-43
effort: XS
risk: low
independent: true
```

**What.** `OnAttachedTo` constructs a `PanGestureRecognizer`, subscribes `OnPanUpdated` to its
`PanUpdated` event, and adds it to `bindable.GestureRecognizers`. `OnDetachingFrom` unsubscribes
and removes the `PointerGestureRecognizer` but does neither for the pan recogniser: the local
`pan` variable is never stored in a field, so the handler stays subscribed and the recogniser stays
in the view's collection.

**Why it matters.** The Microsoft behaviors guidance is explicit that `OnDetachingFrom` "is used to
perform any required cleanup. For example, you could unsubscribe from an event on a control to
prevent memory leaks", and the same page warns that behaviors "aren't implicitly removed from
controls when pages are popped from the navigation stack". The behavior therefore keeps the
detached view alive through the recogniser's handler chain, and a re-attach adds a second pan
recogniser on top of the first -- both would arm the long-press timer for one gesture. This
behavior is attached by `DmCard` for every card, so the surface is broad.

**Suggested direction.** Hold the `PanGestureRecognizer` in a field beside `_pointer`, and mirror
the existing pointer teardown for it in `OnDetachingFrom`.

### UIKIT-004 -- NotNullConverter reports an unresolved binding as a non-null value

```yaml
id: UIKIT-004
layer: UIKIT
severity: S2
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/converters
locations:
  - src/DungeonMaster.App/Converters/NotNullConverter.cs:30-37
effort: XS
risk: low
independent: true
```

**What.** `Convert` is `return value is not null;`. When a binding path fails to resolve, the
binding engine supplies the sentinel `BindableProperty.UnsetValue` rather than `null`. That
sentinel is a non-null object, so the converter returns `true`.

**Why it matters.** The Microsoft converters guidance directs converters to inspect the incoming
value with `as`, `is`, or `GetType` rather than assume its shape, precisely because a converter is
handed values it did not anticipate. Three consumers bind this converter to a visibility target
for an error message -- `Views/CreateDungeonPopup/CreateDungeonPopup.xaml`,
`Views/DungeonDetailPage/DungeonDetailPage.xaml` and `Views/DungeonListPage/DungeonListPage.xaml`
(named as context only) -- plus the resource declaration at `App.xaml:20`. A typo or
rename in the bound path therefore renders a permanently visible, empty error banner instead of
failing quietly. `GreaterThanZeroConverter` does not share the defect: its `value is int n` test
rejects the sentinel.

**Count corrected 2026-09-01 (sub-session 5c, T1).** This record said "Both consumers" and named
two. There are three, measured with
`grep -rn 'NotNullConverter' src/ --include=*.xaml`: the popup, the detail page and the list page.
The `DungeonDetailPage` omission was an error when this record was written, not later drift. The
per-site line numbers are dropped rather than renumbered -- `CreateDungeonPopup.xaml`'s binding has
moved twice since the audit -- because the file plus the converter name is the durable address.

**Suggested direction.** Test the sentinel explicitly -- `value is not null && value !=
BindableProperty.UnsetValue` -- or narrow the check to the type the converter actually expects.

### UIKIT-005 -- The reusable control kit binds directly to Application query DTOs

```yaml
id: UIKIT-005
layer: UIKIT
severity: S2
category: boundary
authority: practice
citation: Robert C. Martin, Clean Architecture (2017), ch. 22 "The Clean Architecture" -- the Dependency Rule
locations:
  - src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml.cs:1
  - src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml.cs:14-21
  - src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:1
  - src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:32-39
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:1-9
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:32
effort: L
risk: medium
independent: true
```

**What.** `AreaSidebarItem.Area` is typed `AreaDto`, `FloorSidebarItem.Floor` is typed `FloorDto`,
and `FloorMapDrawable` takes `FloorDto`, `AreaDto` and `PlacedObjectDto` -- all imported from
`DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail`. `FloorMapDrawable`
additionally imports `DungeonMaster.Domain.Entities.Floors`, `DungeonMaster.Domain.ValueObjects`
and `DungeonMaster.Application.Common.Map`, and computes distance in feet from tile deltas at
`Graphics/FloorMapDrawable.cs:519-524`.

**Why it matters.** The Dependency Rule holds that inner-layer types may be referenced by outer
layers, but that a presentational component becomes reusable only when its contract is its own.
These are the response shape of one specific query, so the control kit's public API is pinned to
that query's projection: adding a field to `GetDungeonDetail`'s DTO is a change to the surface of
three presentation types, and no sidebar item can be reused against any other source of area or
floor data. The map drawable further mixes a domain calculation into a render method. T6 owns the
cross-layer boundary record for the App-to-Application seam as a whole; this record is the
in-layer consequence -- the kit does not hold together as reusable while its own controls name
Application types.

**Suggested direction.** Give the kit its own presentation contracts (a small `AreaItem` /
`FloorItem` shape, or individual bindable properties for the fields each control actually renders)
and map from the query DTO at the page boundary. Move the feet-per-tile arithmetic to Application
alongside the other map helpers.

### UIKIT-006 -- One-way converters throw from ConvertBack where the guidance is to return null

```yaml
id: UIKIT-006
layer: UIKIT
severity: S3
category: api-design
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/converters
locations:
  - src/DungeonMaster.App/Converters/NotNullConverter.cs:60-67
  - src/DungeonMaster.App/Converters/GreaterThanZeroConverter.cs:69-74
effort: XS
risk: low
independent: true
```

**What.** Both converters implement `ConvertBack` as an unconditional
`throw new NotSupportedException(...)`, each with roughly twenty lines of XML documentation
describing the throw.

**Why it matters.** The Microsoft converters page states directly: "If you know that a value
converter will only be used in `OneWay` bindings, then the `ConvertBack` method can simply return
`null`." Throwing turns a future author's mistake -- attaching one of these to a `TwoWay` binding,
which XAML will accept without complaint -- into a runtime exception on the UI thread rather than
an inert no-op. Every call site today is a `OneWay` `IsVisible` or `IsEnabled` target, so nothing
misbehaves now; the cost is the exception surface and the documentation weight carried to describe
it.

**Suggested direction.** Return `null` (or `BindableProperty.UnsetValue`) from both `ConvertBack`
implementations and collapse the accompanying documentation to a single sentence.

### UIKIT-007 -- propertyChanged handlers imperatively write properties the XAML already binds

```yaml
id: UIKIT-007
layer: UIKIT
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Controls/DmFlagBanner/DmFlagBanner.xaml:30
  - src/DungeonMaster.App/Controls/DmFlagBanner/DmFlagBanner.xaml.cs:17-22
  - src/DungeonMaster.App/Controls/DmFlagBanner/DmFlagBanner.xaml.cs:73-78
  - src/DungeonMaster.App/Controls/DmInlineEditor/DmInlineEditor.xaml:31
  - src/DungeonMaster.App/Controls/DmInlineEditor/DmInlineEditor.xaml.cs:312-318
  - src/DungeonMaster.App/Controls/DmCard/DmCard.xaml.cs:24-31
  - src/DungeonMaster.App/Controls/DmCard/DmCard.xaml.cs:113-114
effort: S
risk: medium
independent: true
```

**What.** `DmFlagBanner.xaml:30` binds `TitleLabel.Text` to the `Title` bindable property with
`{Binding Title, Source={x:Reference Root}}`, and the `TitleProperty` `propertyChanged` handler
*also* assigns `banner.TitleLabel.Text` directly; `OnTitleCommitted` assigns it a third time.
`DmInlineEditor.xaml:31` binds `DisplayLabel.TextColor` to the `TextColor` bindable property, and
`SyncDisplay` assigns `DisplayLabel.TextColor` imperatively. `DmCard`'s `SubtitleProperty` handler
calls `UpdateSubtitleVisibility()` to toggle `SubtitleLabel.IsVisible`, a rule the layer's own
`NotNullConverter` or a `DataTrigger` could express in markup.

**Why it matters.** I believe the same target property being driven from two mechanisms is a
maintenance hazard rather than merely redundant: my reading of MAUI's value precedence is that a
locally assigned value outranks a binding on the same property, so the imperative write appears to
be the one that reaches the pixel and the binding in the markup appears to be decorative -- which
would make the XAML misdescribe what renders. I could not confirm that precedence behaviour
against a fetched Microsoft page, so I state it as belief. Independently of the precedence
question, the duplication is real: a reader must check both halves of the control to know what
sets a property.

**Suggested direction.** Pick one mechanism per property. Where a binding already exists, delete
the imperative assignment from the change handler; where imperative state is genuinely needed
(`DmFlagBanner`'s post-commit resync), remove the binding and say so at the site. `DmCard`'s
subtitle visibility is a good candidate for the existing converter.

### UIKIT-008 -- FloorMapDrawable exposes concrete mutable collections as public API

```yaml
id: UIKIT-008
layer: UIKIT
severity: S3
category: api-design
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/guidelines-for-collections
locations:
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:48
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:63
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:68
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:73
effort: S
risk: low
independent: true
```

**What.** Four public properties expose concrete collection types: `PendingMoveOffsets` and
`PendingOutlines` as `Dictionary<,>`, `GhostAreas` as `List<>`, and `InProgressVertices` as a
`List<>` with a public setter.

**Public-surface removal, 2026-09-01 (sub-session 5c, T7, closing `MASTER-059`).** One of the four
was not just badly typed but **dead**: no write to `PendingOutlines` existed anywhere in the
repository, so `GetEffectiveOutline` always fell through to `area.Outline`. Both were removed rather
than retyped, and `grep -rn "PendingOutlines\|GetEffectiveOutline" src/` now exits 1 with no hits.
The other three were replaced by six `Set`/`Add`/`Clear` members that raise a `RedrawRequested`
event, which is the "mutation points raise a redraw request" half of the suggested direction below;
the page subscribes once. Recorded here because a public member disappearing from a shipped assembly
is worth stating explicitly, even when nothing consumed it.

**Why it matters.** The Framework Design Guidelines state "DO NOT use `ArrayList` or `List<T>` in
public APIs" and "DO NOT use `Hashtable` or `Dictionary<TKey,TValue>` in public APIs" -- both are
implementation data structures -- and separately "DO NOT provide settable collection properties".
The concrete result here is that the drawable cannot observe its own state: the page mutates these
collections directly and must remember to call `Invalidate()` afterwards, so a forgotten
invalidation is a stale frame with nothing to catch it, and the drawable has no seam at which to
recompute anything derived.

**Suggested direction.** Expose `IDictionary<,>` / `IList<>` (or narrower read-only surfaces with
explicit `Set...` methods), remove the setter on `InProgressVertices`, and consider having the
mutation points raise a redraw request so the caller cannot forget.

**Line references, corrected 2026-09-01.** The line numbers in this record's `locations` and prose
address the file as it stood at the 2026-08-17 audit and predate Sessions 4 and 5, which grew it;
they are left unchanged rather than renumbered, because renumbering goes stale again at the next
session. **The durable address is the symbol name.** Measured by sub-session 5c's T7 at `7a0b506`,
where `FloorMapDrawable.cs` was 576 lines: the four
collection properties were at `:108`, `:123`, `:128` and `:133`, not the `:48`/`:63`/`:68`/`:73`
recorded above. The file is 776 lines as of 2026-09-01, so those numbers have moved again.

**[AMENDED 6b-iv, 2026-09-11 -- `GhostAreas` is not a public `List<>` and has not been one since sub-session 5c. Classification: DRIFT.** Previous wording, verbatim: *"Four public properties expose concrete collection types: `PendingMoveOffsets` and `PendingOutlines` as `Dictionary<,>`, `GhostAreas` as `List<>`, and `InProgressVertices` as a `List<>` with a public setter."* There is no `GhostAreas` member of any kind today. The ghost outlines live in a **private readonly field**, `private readonly List<IReadOnlyList<PathSegment>> _ghostAreas = [];` at `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:117`, reached only through the two public methods `AddGhostArea` (`:384`) and `ClearGhostAreas` (`:396`), each of which raises `RedrawRequested`. This is **DRIFT, not a measurement error**: the sentence was true when written, and the "Public-surface removal, 2026-09-01" paragraph above already records the replacement in general terms -- this amendment states it for the one member a later reader is most likely to go looking for, because searching this file for `GhostAreas` finds the stale prose before it finds that paragraph. Verified by opening the file in the 6b-iv worktree: `sed -n '115,119p'` for the field and `grep -rn "GhostAreas\|DrawGhostAreas" src/DungeonMaster.App/Graphics/FloorMapDrawable.cs`, which returns exactly one line, `396: public void ClearGhostAreas()`. `FloorMapDrawable.cs` was **not** edited by any task in this session, so this citation is current rather than qualified. The record's severity, category and suggested direction are unchanged.**]**

### UIKIT-009 -- Draw allocates per area, per segment and per frame

```yaml
id: UIKIT-009
layer: UIKIT
severity: S3
category: performance
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:236
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:243
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:245
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:251
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:262-267
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:485
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:529
effort: M
risk: medium
independent: true
```

**What.** Inside the render path, `DrawAreaOutline` allocates a fresh `PathF` per area (line 245),
a `Color` per area via `WithAlpha` (line 236), and a `TilePoint` per segment and per Bezier control
point (lines 243, 251, 262-267). `DrawInProgressOverlay` allocates another `PathF` (line 485) and
an interpolated string per frame while a phantom segment is live (line 529). Every `DmTheme.*`
read in this file is a resource-dictionary lookup plus a type test, and there are exactly twelve
of them across one pass.

**Why it matters.** I believe this is a measurable cost rather than a theoretical one: at a plain
60fps with a dozen areas of a dozen segments each, this is on the order of thousands of
short-lived allocations per second on the UI thread, which is exactly the shape that produces
gen-0 pressure and visible hitching during a pan or zoom. I did not profile the running app and
cannot state the magnitude as fact. The geometry per area is stable between frames unless the
outline or its offset changes, so most of it appears recomputable once rather than every frame.

**Suggested direction.** Cache the built `PathF` per area, keyed on the outline plus effective
offset, and invalidate on change; hoist the per-frame `DmTheme` reads into locals at the top of
`Draw`; and cache the distance label string on the snapped value rather than reformatting it each
frame. Measure before and after -- this is the kind of change that should be justified by a
number.

**Count corrected 2026-09-01 (sub-session 5c, T7).** "More than a dozen" `DmTheme` reads was
exactly twelve, measured with `grep -o 'DmTheme\.' <file> | wc -l` at `7a0b506` (lines 210, 296,
419, 421, 458, 461, 498, 514, 530, 546, 569, 570) and equally twelve at the audit-era commit, so
the claim was wrong when written rather than overtaken. The judgement is unchanged, and the number
is now seven, T7 having hoisted the reads into one per-frame palette.

**Line references, corrected 2026-09-01.** The line numbers in this record's `locations` and prose
address the file as it stood at the 2026-08-17 audit and predate Sessions 4 and 5, which grew it;
they are left unchanged rather than renumbered, because renumbering goes stale again at the next
session. **The durable address is the symbol name.** The `locations` numbers above address the
pre-Session-4 file.

### UIKIT-010 -- DrawAreasViewMode and DrawAreasEditMode differ only in the alpha rule

```yaml
id: UIKIT-010
layer: UIKIT
severity: S3
category: duplication
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Duplicated Code"
locations:
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:171-181
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:183-197
effort: XS
risk: low
independent: true
```

**What.** The two methods share an identical loop over `Floor!.Areas` -- same
`GetEffectiveOutline`, same `GetEffectiveOffset`, same `DrawAreaOutline` call, same trailing
`DrawGhostAreas(canvas)`. The only difference is how `alpha` is chosen: view mode dims everything
but the hovered area, edit mode falls back to the active area when nothing is hovered.

**Why it matters.** Fowler's canonical case: a change to how an outline is drawn has to be made in
two places, and the shorter view-mode form is the one likely to be missed. The two are already
selected by a single `if` at line 131, so nothing but the alpha rule needs two code paths.

**Suggested direction.** Collapse to one loop and extract the alpha decision into a small helper
that takes the edit/view distinction, or compute it inline from `ActiveAreaId is not null`.

**Line references, corrected 2026-09-01.** The line numbers in this record's `locations` and prose
address the file as it stood at the 2026-08-17 audit and predate Sessions 4 and 5, which grew it;
they are left unchanged rather than renumbered, because renumbering goes stale again at the next
session. **The durable address is the symbol name.** Measured by sub-session 5c's T7 at `7a0b506`:
`DrawAreasViewMode` was at `:231-241` and
`DrawAreasEditMode` at `:243-257`, not the `:171-181` and `:183-197` recorded above, and the
selecting `if` was at `:191-198`, not line 131. Both methods have since been collapsed into one
`DrawAreas` loop under `MASTER-035`, so the two names no longer exist.

**[AMENDED 6b-iv, 2026-09-11 -- there is no separate `DrawGhostAreas(canvas)` call, and no such method. Classification: DRIFT.** Previous wording, verbatim: *"The two methods share an identical loop over `Floor!.Areas` -- same `GetEffectiveOutline`, same `GetEffectiveOffset`, same `DrawAreaOutline` call, same trailing `DrawGhostAreas(canvas)`."* No method named `DrawGhostAreas` exists in the file. The ghosts are painted by a second `foreach` **inside** the single surviving `DrawAreas`, which runs over the private `_ghostAreas` field after the floor's own areas: `DrawAreas` is `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:535-553`, and the ghost loop is its last five lines, `foreach (var outline in _ghostAreas) { DrawAreaOutline(canvas, outline, palette.AreaOutline); }`. **DRIFT, not a measurement error**: the paragraph above already records that `MASTER-035` collapsed the two methods into one, which is the same edit that inlined this call, so the sentence was accurate when written. `GetEffectiveOutline` in the same sentence is likewise gone -- `UIKIT-008`'s own "Public-surface removal" paragraph records it as removed as dead. The record is **closed in substance by `MASTER-035`** and this amendment changes nothing about that; it exists so a reader searching for `DrawGhostAreas` learns it never survived rather than concluding the record is describing a different file. Verified by opening the file in the 6b-iv worktree: `sed -n '535,553p'` and `grep -rn "GhostAreas\|DrawGhostAreas" src/DungeonMaster.App/Graphics/FloorMapDrawable.cs`, whose only hit is `ClearGhostAreas` at `:396`. `FloorMapDrawable.cs` was **not** edited by any task in this session, so this citation is current rather than qualified.**]**

### UIKIT-011 -- Sidebar items expose visual state through imperative methods, not bindable properties

```yaml
id: UIKIT-011
layer: UIKIT
severity: S3
category: api-design
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/bindable-properties
locations:
  - src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml.cs:137-153
  - src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:185-221
effort: M
risk: medium
independent: true
```

**What.** `AreaSidebarItem` carries `public void SetEditing(bool)` backed by a private
`_isSelected` field, and paints stroke, thickness and background inline. `FloorSidebarItem` carries
`FocusNameEditor()`, `Deselect()`, `Select()` and `SetAreaEditing(Guid, bool)` over a private
`_isExpanded`, with `SetExpanded` painting surface and border directly. Neither selection nor
expansion is a bindable property, and neither control declares a `VisualStateManager` group --
`DmCard` is the only control in the kit that does (`Controls/DmCard/DmCard.xaml:18-35`).

**Why it matters.** The Microsoft guidance lists the cases where a property "should be implemented
as a bindable property", and these hit three of them: acting as a valid binding target, being
settable through a style, and monitoring property changes. As written, the selected and expanded
appearance cannot be restyled from the theme dictionaries at all -- it is hard-wired in
code-behind -- so the one part of the kit the theme system cannot reach is the part that changes
most. It also forces the host page to track selection itself and push it back in, which is where
`docs/TODO.md` already reports the sidebar-rebuild cost.

**Suggested direction.** Introduce `IsSelected` / `IsExpanded` bindable properties, express the
appearance as `CommonStates` / custom visual states in each control's XAML pointing at theme
tokens, and reduce the public methods to thin wrappers or delete them.

### UIKIT-012 -- FloorSidebarItem re-declares and hand-forwards five of AreaSidebarItem's events

```yaml
id: UIKIT-012
layer: UIKIT
severity: S3
category: structure
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Middle Man"
locations:
  - src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:86-109
  - src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:159-178
effort: M
risk: medium
independent: true
```

**What.** `FloorSidebarItem` declares `AreaSelectRequested`, `AreaHoverStarted`, `AreaHoverEnded`,
`AreaDeleteRequested` and `AreaNotebookRequested`, which exist only to republish the identically
shaped events of its child `AreaSidebarItem`s. `RebuildAreas` wires five closures per child on
every `Floor` assignment, having first cleared and reconstructed the whole child collection.
`FloorSidebarItem` exposes 14 public events in total.

**Count corrected 2026-09-01 (sub-session 5c, T6).** This record said 12. It was 14, measured with
`TAB=$(printf '\t'); grep -c "^${TAB}public event"` over
`src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs` -- 14 at the audit-era
commit as well as at `0c8f98f`, so the claim was wrong when written rather than overtaken by later
work. It is 9 as of 2026-09-01, the five forwarded area events having been replaced by commands on
a `BindableLayout` template under `MASTER-099`.

**Why it matters.** This is Fowler's Middle Man: five members of the type's public surface add no
behaviour, and adding a sixth area-level event means edits in three files. The forwarding closures
also mean each rebuild allocates a new set of delegates whose only unsubscribe path is the child
becoming unreachable. `docs/TODO.md` already schedules the *cost* of the sidebar rebuild under
"Evaluate the dungeon-detail data refresh path"; this record is about the API shape that makes the
rebuild the only way to re-bind, which that item does not cover.

**Suggested direction.** Bind the area list with `BindableLayout` and an `ItemTemplate` so children
are created by the framework rather than in code, and replace the forwarded events with commands
on a single bindable property, so a child's gesture reaches the page without the floor item
mediating it.

### UIKIT-013 -- DmSearchBar paints its focus state imperatively rather than through visual states

```yaml
id: UIKIT-013
layer: UIKIT
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Controls/DmSearchBar/DmSearchBar.xaml.cs:38-46
effort: S
risk: low
independent: true
```

**What.** `OnSearchFocused` and `OnSearchUnfocused` assign `SearchBorder.Stroke` from
`DmTheme.FloorBorderActive` and `DmTheme.FloorBorderNormal` in code-behind. The control's XAML
declares no visual states.

**Why it matters.** I believe this is the wrong seam for the kit: focus is exactly the kind of
state MAUI's `VisualStateManager` exists to express, `DmCard` already expresses hover that way, and
the theme dictionaries can reach a visual state but cannot reach a code-behind assignment. It also
borrows two `Floor*` accessors on `DmTheme` whose names describe the floor card rather than a
search field, so the token intent reads wrong at this call site. I did not find a Microsoft page
stating a preference between the two mechanisms for focus specifically, so I state this as belief
rather than as a rule.

**Suggested direction.** Move the two colours into a `CommonStates` group (or a `Focused` state) on
the border in `DmSearchBar.xaml` referencing the border-brush tokens directly, and delete both
handlers. If a distinct token is warranted for a focused input, add one rather than reusing the
floor-card pair.

### UIKIT-014 -- The 44-unit touch-target minimum is a bare literal repeated ten times

**The count of ten is CORRECT and must not be "corrected".** Re-measured 2026-09-01 from
`src/DungeonMaster.App` with `grep -rn '"44"' Resources/Theme/` against the pre-fix tree: exactly
10 occurrences, 2 in `Controls.Base.xaml` and 8 in `Implicit.xaml`, which are this record's own
named locations. An earlier handoff called the count wrong after searching `Controls/` and
`Views/` -- a different population, and not the one this record names. The literals number 0 as of
2026-09-01, `MASTER-036` having replaced them with `{StaticResource DmTouchTargetMinimum}`; the ten
describes the state at audit time and is right about it.

```yaml
id: UIKIT-014
layer: UIKIT
severity: S3
category: duplication
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Resources/Theme/Implicit.xaml:196-197
  - src/DungeonMaster.App/Resources/Theme/Implicit.xaml:282-283
  - src/DungeonMaster.App/Resources/Theme/Implicit.xaml:287-288
  - src/DungeonMaster.App/Resources/Theme/Implicit.xaml:292-293
  - src/DungeonMaster.App/Resources/Theme/Controls.Base.xaml:50-51
effort: XS
risk: low
independent: true
```

**What.** `MinimumHeightRequest="44"` and `MinimumWidthRequest="44"` appear as literals ten times
across five style declarations in two dictionaries. `Tokens.xaml` declares no token for the value.
The `GeometryScaleTests` ratchets cover spacing, padding, radius, margin and stroke, and explicitly
allow-list `Controls.Base.xaml`'s radius `3` and both files' padding `14,10` -- but nothing scans
`MinimumHeightRequest` or `MinimumWidthRequest`, so this class of literal is unguarded.

**Why it matters.** I believe this is the token layer's own argument applied to a value it has not
yet reached. `Implicit.xaml:189-190` states the reasoning -- "the platform touch target, not a
spacing step" -- which is a statement that the value has a *name and a meaning*, and that is
precisely what a token expresses. The 14,10 padding is a defensible exception because it is a
measured button box specific to one control; a platform accessibility minimum applying uniformly to
Button, Entry, Editor and Picker is not the same case. Today a change to that minimum is ten edits
with no guard that they stay in step. I could not confirm a Microsoft page prescribing 44 for MAUI
specifically, so I do not assert the number itself as a standard.

**Suggested direction.** Add a `DmTouchTargetMinimum` token to `Tokens.xaml`, bind the ten setters
to it, and extend the geometry ratchet to scan `MinimumHeightRequest` / `MinimumWidthRequest` so
the next literal is caught.

### UIKIT-015 -- DmTheme.Gold and DmTheme.ScrollGoldLight resolve the identical token key

```yaml
id: UIKIT-015
layer: UIKIT
severity: S3
category: api-design
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Theme/DmTheme.cs:39-40
  - src/DungeonMaster.App/Theme/DmTheme.cs:135-136
effort: XS
risk: low
independent: true
```

**What.** Both accessors call `Resolve<Color>("DmAccentGoldLight")`. The XML documentation on
`ScrollGoldLight` (lines 122-134) states this is deliberate -- the two names read differently at
their call sites -- and records that separating them requires adding a token.

**Why it matters.** I believe an alias that is documented as nominal is still an alias: a reader
who changes `Gold` has no way to discover from the change site that `ScrollGoldLight` moved with
it, and a reader who wants only one of the two must first read 13 lines of documentation to learn
that the seam does not exist yet. `ThemeTokenParityTests` guarantees both keys exist but cannot see
that they are the same key. The documentation is unusually good here, so this is a judgement about
whether the second name is earning its keep rather than a defect claim -- and the file's own
precedent (`DmMapObjectPlaceholder` was added to make exactly this kind of seam real) points the
other way.

**Suggested direction.** Either add the distinct token now, so the seam is real and the doc comment
collapses to one line, or delete `ScrollGoldLight` and let its three call sites read `Gold`. The
same question applies to the several other members the file documents as sharing a value; those
at least resolve distinct keys.

**Count corrected 2026-09-01 (sub-session 5c, T3).** This record said "four call sites". There are
three, measured with
`grep -rn 'ScrollGoldLight' src/ --include=*.cs --include=*.xaml | grep -v '/obj/\|/bin/'` and
counting only reads of the accessor: `Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs`,
`Controls/NotebookAffordance.cs` and `Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`. It was
three at the audit-era commit too, so the claim was wrong when written. Line numbers are omitted
deliberately: all three files were edited in Session 5, and the numbers this session first measured
were already two revisions old by the time it finished.

### UIKIT-016 -- NotebookView resolves a theme colour through a raw indexer and a null-forgiving operator

```yaml
id: UIKIT-016
layer: UIKIT
severity: S3
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs:415-430
effort: S
risk: low
independent: true
```

**What.** `RibbonBrush` reads `(Color)MauiApplication.Current!.Resources[colorKey]` with a runtime
key chosen from a ternary. The comment at lines 417-418 explains that `DmTheme` exposes one member
per fixed key and so has nothing to route a runtime key through.

**Why it matters.** I believe this loses the three diagnostics `DmTheme.Resolve<T>` was written to
provide: the null-forgiving `!` suppresses the "read before `Application.Current.Resources`
existed" case that the class's own remarks call out as the hardest to diagnose; the raw indexer
throws a bare `KeyNotFoundException` naming neither the file nor the reason; and the cast throws
`InvalidCastException` rather than naming the actual type. `ThemeTokenParityTests` guards the key
strings themselves -- its comment records that `NotebookView.RibbonBrush` once carried
`"DMRibbonRead"` undetected -- so the key is safe; the failure *reporting* is what is missing. It
also builds a new `LinearGradientBrush` and two `GradientStop`s on every ribbon selection.

**Suggested direction.** Add a `DmTheme.Resolve`-backed helper that accepts a key at runtime --
`internal static Color Color(string key)` -- so this call site keeps its diagnostics, and cache the
two brushes since there are exactly two.

### UIKIT-017 -- Long-press state is written on the UI thread and read on a timer thread with no synchronisation

```yaml
id: UIKIT-017
layer: UIKIT
severity: S3
category: threading
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Behaviors/CanvasLongPressDetector.cs:8
  - src/DungeonMaster.App/Behaviors/CanvasLongPressDetector.cs:18-23
  - src/DungeonMaster.App/Behaviors/HoverOrLongPressBehavior.cs:9
  - src/DungeonMaster.App/Behaviors/HoverOrLongPressBehavior.cs:57-63
effort: S
risk: low
independent: true
```

**What.** `CanvasLongPressDetector._active` and `HoverOrLongPressBehavior._touching` are plain
`bool` fields, set from gesture callbacks on the UI thread and read inside a
`System.Timers.Timer.Elapsed` handler, which runs on a thread-pool thread. Neither field is
`volatile` and neither access is guarded.

**Why it matters.** I believe this is a real if narrow race rather than a theoretical one: the
memory model permits the timer thread to observe a stale `true` after the gesture has ended, which
would fire `LongPressed` for a press the user already released. Both handlers also subscribe to
`Elapsed` with an anonymous lambda, so the subscription cannot be removed independently of
disposing the timer -- correct as written, since both dispose, but fragile. I have not observed the
misfire and cannot state it as fact.

**Suggested direction.** Mark both fields `volatile`, or replace `System.Timers.Timer` with
`IDispatcherTimer` from the view's `Dispatcher` -- which ticks on the UI thread and would remove
both the race and the `MainThread.BeginInvokeOnMainThread` hop. `NotebookView` already uses
`Dispatcher.CreateTimer()` for its debounce, so the pattern is established in the layer.

### UIKIT-018 -- DmConfirmPopup discards the CloseAsync task without observing it

```yaml
id: UIKIT-018
layer: UIKIT
severity: S3
category: async
authority: practice
citation: Stephen Cleary, Concurrency in C# Cookbook 2e, ch. 2 "Async Basics" -- observing task exceptions
locations:
  - src/DungeonMaster.App/Controls/DmConfirmPopup/DmConfirmPopup.xaml.cs:58-62
effort: XS
risk: low
independent: true
```

**What.** Both button handlers are `_ = CloseAsync(true);` and `_ = CloseAsync(false);`. The
returned `Task` is discarded, so a fault inside the close sequence is never observed and never
surfaces.

**Why it matters.** Discarding a task is the fire-and-forget shape Cleary warns about: the
exception is swallowed until finalisation and there is no path by which the caller, or the user,
learns the popup failed to close. The popup is `Popup<bool>`, so the awaiting caller would see the
result task never complete. `docs/TODO.md` already collects a family of fire-and-forget handlers
under "Designer async-lifecycle hardening", but those are all `DungeonDetailPage` methods; this one
is inside the control and is not listed there.

**Suggested direction.** Make the handlers `async void` with a try/catch that logs, or route both
through a small `SafeFireAndForget`-style helper shared by the layer so the failure is at least
recorded.

### UIKIT-019 -- DmInlineEditor concentrates five responsibilities and hides a state change in ClearFocusGlow

```yaml
id: UIKIT-019
layer: UIKIT
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Controls/DmInlineEditor/DmInlineEditor.xaml.cs:1-431
  - src/DungeonMaster.App/Controls/DmInlineEditor/DmInlineEditor.xaml.cs:352-366
  - src/DungeonMaster.App/Controls/DmInlineEditor/DmInlineEditor.xaml.cs:415-430
effort: M
risk: high
independent: true
```

**What.** At 431 lines the file is the largest control in the kit. It owns display/edit mode
switching by opacity and `InputTransparent`, two parallel input controls chosen by `AcceptsReturn`,
a commit protocol with a `_suppressUnfocusCommit` latch, focus-glow painting, and placeholder
rendering. `ClearFocusGlow` -- named for the visual effect -- sets `_isEditing = false` as its first
statement (line 424). `Commit` assigns `Text`, which fires `TextProperty`'s `propertyChanged`,
which calls `SyncDisplay`, and then calls `SyncDisplay(value)` again directly.

**Why it matters.** I believe the `ClearFocusGlow` naming is the more serious half: a reader
scanning for where edit mode ends will not look inside a method named for painting, and
`OnPointerExited` calls it on a plain hover-out. Today that is harmless because the call is guarded
by `!_isEditing`, but the guard and the mutation are in different methods. The double `SyncDisplay`
is redundant rather than wrong. The single-file concentration is the reason both are easy to miss.

**Suggested direction.** Split the glow painting from the edit-state transition into two methods
with honest names, drop the redundant `SyncDisplay` call in `Commit`, and consider extracting the
commit/suppression protocol into a small collaborator so the control's remaining job is display.
High risk: the commit ordering is carefully reasoned in the existing comments and must be preserved
exactly.

**Line references, corrected 2026-09-01.** The line numbers in this record's `locations` and prose
address the file as it stood at the 2026-08-17 audit and predate Sessions 4 and 5, which grew it;
they are left unchanged rather than renumbered, because renumbering goes stale again at the next
session. **The durable address is the symbol name.** Measured by sub-session 5c's T8 on the pre-T8
file (431 lines): `ClearFocusGlow` was `:422-430`,
not the `:415-430` recorded above; the range recorded above as `Commit`, `:352-366`, is
`ApplyFocusGlow`, a different method, and `Commit` was `:415-420`; `SyncDisplay` was `:312-318`.
Those two are an identification error rather than drift. `MASTER-105` has since split the painting
from the edit-state transition, so `ClearFocusGlow` no longer writes `_isEditing`.

**The `OnPointerExited` sentence above is accurate and was NOT amended.** Sub-session 5c reported it
as overstating the harm; a reviewer read this record and found it already says the call is guarded
by `!_isEditing` and that the guard and the mutation sit in different methods, which is exactly the
correct statement. The overstatement was in that session's own task criterion, not here. Recorded so
the point is not re-litigated.

### UIKIT-020 -- HandCursorBehavior reflects for ProtectedCursor on every pointer event

```yaml
id: UIKIT-020
layer: UIKIT
severity: S3
category: performance
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Behaviors/HandCursorBehavior.cs:109-132
effort: XS
risk: low
independent: true
```

**What.** `SetCursor` calls `typeof(Microsoft.UI.Xaml.UIElement).GetProperty("ProtectedCursor",
...)` and `InputSystemCursor.Create(shape)` on each invocation. It runs on every `PointerEntered`
and every `PointerExited`, on every view carrying the behavior.

**Why it matters.** I believe the reflection lookup is cheap enough per call not to be visible, but
it is unnecessary work repeated on a high-frequency input path, and the `PropertyInfo` and the four
possible `InputSystemCursor` instances are all invariant for the process. There is also no cached
failure path: if the property lookup ever returns `null` the code silently does nothing and repeats
the lookup forever. I did not measure the cost and do not claim it is perceptible.

**Suggested direction.** Cache the `PropertyInfo` in a `static readonly` field and cache the
`InputSystemCursor` per `BehaviorCursorShape` in a small static map, so each pointer event is a
dictionary read and a property set.

### UIKIT-021 -- Single-statement control flow omits braces at twelve sites

```yaml
id: UIKIT-021
layer: UIKIT
severity: S4
category: style
authority: conventions
citation: backend/dotnet-format.md#4. Control Flow Braces
locations:
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:188-191
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:210-211
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:219-220
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:233-234
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:288-289
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:323-327
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:434-435
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:475-476
  - src/DungeonMaster.App/Behaviors/CanvasLongPressDetector.cs:20-22
  - src/DungeonMaster.App/Behaviors/HoverOrLongPressBehavior.cs:60-62
  - src/DungeonMaster.App/Behaviors/HandCursorBehavior.cs:67-68
effort: XS
risk: low
independent: true
```

**What.** Eleven `if` bodies and one `foreach` body are written without braces. `dotnet-format.md`
section 4 states: "All control flow statements (`if`, `else`, `for`, `foreach`, `while`, `do`,
`using`, `lock`) must use full brace notation `{ }`, even when the body is a single statement.
Never omit braces."

**Why it matters.** The convention exists to make adding a second statement safe. Two of these are
already the riskier form: `FloorMapDrawable.cs:188-191` is a braceless `if`/`else` pair assigning
`alpha`, and `CanvasLongPressDetector.cs:20-22` and `HoverOrLongPressBehavior.cs:60-62` wrap
multi-line lambda invocations in a braceless `if`, where the extent of the body is not visually
obvious.

**Suggested direction.** Add braces at all twelve sites. Mechanical, no behavioural change.

**[AMENDED 6b-xviii, 2026-10-09 -- this entry's POPULATION is overstated by two sites and one file, all
eight of its `FloorMapDrawable.cs` citations are stale, two of the three files it names were already
clean when it was filed, and a real site it never cited was found. Classification: the ENTRY's
substance stands -- the convention is real and was being violated -- but its TITLE, its `locations:`
block and its count are all FALSE as stated; NARROWED, not falsified.** The register row is
`MASTER-121` at `00-master-index.md:309`, closed **done**. This block follows the form of the
`AMENDED 6b-xvii` block on `CORE-028` at `01-domain-application.md:2156-2216`, cited **for its form
only** -- `AMENDED 6b-xiv` at `01-domain-application.md:1293` records that the block that one was in
turn modelled on *"carries three defects of its own"*. **Nothing below deletes any previous wording.**

*Correction 1 -- the population, in the title and in the `What.` paragraph.* Previous wording, verbatim,
the title: *"Single-statement control flow omits braces at twelve sites"*, and from `What.`: *"Eleven
`if` bodies and one `foreach` body are written without braces."* **The live population was 10 braceless
sites across 2 files, not twelve across three.** Measured on the post-change tree with a detector that
resolves the **matching** close parenthesis of a control-flow header rather than the first one:

| File | Braceless sites, post-change | Note |
|---|---|---|
| `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs` | **0** | 8 remedied, re-derived at `:208`, `:274`, `:283`, `:597`, `:655`, `:660`, `:770`, `:813` |
| `src/DungeonMaster.App/Behaviors/HandCursorBehavior.cs` | **0** | 2 remedied, re-derived at `:69`, `:158` |
| `src/DungeonMaster.App/Behaviors/CanvasLongPressDetector.cs` | **0** | cited `:20-22`; **already clean at 0 when filed** |
| `src/DungeonMaster.App/Behaviors/HoverOrLongPressBehavior.cs` | **0** | cited `:60-62`; **already clean at 0 when filed** |

**POSITIVE CONTROL beside those four zeros**, without which they are indistinguishable from a predicate
reading nothing: the identical detector, in the same run, returns **21** on
`src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`. **And the detector was
validated against the defect class that defeats naive versions of it**, in both directions: exactly
**3** on a fixture holding three braceless forms, two of which have nested-parenthesis headers
(`if (a.Foo(b) && c.Bar(d)) return;` and a `foreach` over a `.Where(i => i.Ok(i.Id))`), beside three
**braced** nested-paren forms that must NOT be flagged and are not; and **0** on an all-braced fixture
including a braced `else`. This class has now defeated three independently written detectors in this
session's record -- one by paren-balance arithmetic, one by a `)` + non-space tail test matching the
inner `) &&`, and the braced-nested-paren direction is why the control file must contain both.

*Correction 2 -- the eight `FloorMapDrawable.cs` citations.* Previous wording, verbatim, the
`locations:` block's first eight entries: `:188-191`, `:210-211`, `:219-220`, `:233-234`, `:288-289`,
`:323-327`, `:434-435`, `:475-476`. **Every one is stale and none of them holds braceless control flow.**
They assume a **534**-line file. The file measured **882** lines at the session base and **898**
post-change (`wc -l`), the +16 being the eight brace pairs this session added -- the leading-tab count
moves **706 -> 722** across the same edit, so the two figures reconcile rather than being asserted
separately. Anchored on the line and on what is on it, not derived by subtraction from any span.

*Correction 3 -- the two files that were already clean.* Previous wording, verbatim, from
`Why it matters.`: *"`CanvasLongPressDetector.cs:20-22` and `HoverOrLongPressBehavior.cs:60-62` wrap
multi-line lambda invocations in a braceless `if`, where the extent of the body is not visually
obvious."* **Both files return 0 braceless sites and did so before this session touched anything** --
no task owns either file. So the entry's two "riskier form" examples are, between them, one real site
(`FloorMapDrawable.cs:188-191`, itself a stale citation) and two that had already been fixed.

*Correction 4 -- a real site this entry never cited.* **`HandCursorBehavior.cs:154`, inside
`#if WINDOWS`, was a braceless site and appears nowhere in the `locations:` block**, which cites only
`:67-68` for that file. It is now braced, re-derived at `:158`. An entry that overstates its count by
two while omitting a member is not merely imprecise: the count and the enumeration disagreed, and only
the enumeration was ever checked.

*Correction 5 -- A CLAIM MADE IN THIS SESSION, NOT AN ORIGINAL ERROR IN THIS ENTRY, recorded because a
correction is new prose and gets no exemption from measurement.* The orchestrator initially recorded,
and told the architect inside a decomposition dispatch, that `HandCursorBehavior.cs:67` *"does not
compile on this machine, so an edit there cannot be verified by a build"*. **That is false, and it was
inferred from the `#if` guard without running a build.** Measured here, post-change:

1. **The guards.** `grep -nE '^#(if|elif|else|endif)'` on `HandCursorBehavior.cs` returns **three**
   `#if MACCATALYST` regions, at `:31-36`, `:50-52` and `:64-111`, and **two** `#if WINDOWS` regions, at
   `:113-151` and `:155-167`. The braced site at `:69` is in the **third** MACCATALYST region and the one
   at `:158` is in the **second** WINDOWS region. Earlier spans reported in this session for these two
   regions were each off by a constant `+1`, the signature of arithmetic over base figures rather than a
   read of the command's output; these are read from the output above.
2. **All three target frameworks are declared.** `DungeonMaster.App.csproj:9` reads
   `<TargetFrameworks>net10.0;net10.0-windows10.0.19041.0;net10.0-maccatalyst</TargetFrameworks>`.
3. **The maccatalyst target is actually built here.** `dotnet build src/DungeonMaster.sln` in this
   worktree wrote `src/DungeonMaster.App/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/DungeonMaster.App.dll`,
   **530944 bytes** by `ls -l`, alongside the `net10.0` and `net10.0-windows10.0.19041.0` outputs.

**So both sites are build-verifiable and this entry's `risk: low` rating stands.** The general lesson is
the transferable part: **a `grep` for a preprocessor guard establishes that a guard exists and
establishes nothing about whether the toolchain defines the symbol.** It was caught only because the
baseline build printed the maccatalyst target. The record's severity, category and risk are unchanged;
effort is unchanged at `XS`.**]**

### UIKIT-022 -- FloorMapDrawable is space-indented, and four files indent doc-comment bodies with spaces

```yaml
id: UIKIT-022
layer: UIKIT
severity: S4
category: style
authority: conventions
citation: backend/dotnet-format.md#9. XML Documentation Comments
locations:
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:1-534
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:13-16
  - src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml.cs:28-36
  - src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:91-109
effort: XS
risk: low
independent: true
```

**What.** A leading-whitespace census across the layer's 19 `.cs` files returns 429 space-indented
lines and 0 tab-indented lines in `FloorMapDrawable.cs`; every other file in the layer is
tab-indented with zero space-indented lines. Its XML doc bodies use five spaces after `///` rather
than the two tabs section 9 prescribes ("Tag descriptions go on a new line, indented with two tabs
from the `///` marker"). `AreaSidebarItem.xaml.cs:28-36` and four doc comments in
`FloorSidebarItem.xaml.cs:91-109` use the same space form while the rest of each file uses tabs.

**Why it matters.** Section 9's two-tab rule is explicit for doc comments, and the file-level
indentation is a straight inconsistency: with no `.editorconfig` anywhere in the repository
(established in the session evidence), nothing normalises this, so a diff touching
`FloorMapDrawable.cs` from an editor configured for the rest of the codebase will churn whitespace
across unrelated lines.

**Suggested direction.** Convert `FloorMapDrawable.cs` to tabs in one isolated commit, fix the six
space-indented doc bodies, and consider adding an `.editorconfig` -- though that is repository-wide
and belongs to T6.

**[AMENDED 6b-xviii, 2026-10-09 -- this entry's FILE-INDENTATION CLAIM IS FALSE, its title is wrong in
its first clause, its own census figures describe a file that no longer exists in that form, two of its
four cited locations were already clean, and its doc-comment population was four times what it names.
Classification: the FILE-LEVEL half is FALSIFIED; the DOC-COMMENT half's substance stands and was acted
on in full, with its population WIDENED; `MASTER-181` is merged into this row.** The register row is
`MASTER-122` at `00-master-index.md:310`, closed **done**, with `MASTER-181` at
`00-master-index.md:369` closed **AS A DUPLICATE** of it under the user's Gate 1 decision. This block
follows the form of the `AMENDED 6b-xvii` block on `CORE-028` at `01-domain-application.md:2156-2216`,
cited **for its form only** -- `AMENDED 6b-xiv` at `01-domain-application.md:1293` records that the
block that one was in turn modelled on *"carries three defects of its own"*. **Nothing below deletes any
previous wording.**

*Correction 1 -- the title's first clause and the file-level census, both FALSE.* Previous wording,
verbatim, the title: *"FloorMapDrawable is space-indented, and four files indent doc-comment bodies with
spaces"*, and from `What.`: *"A leading-whitespace census across the layer's 19 `.cs` files returns 429
space-indented lines and 0 tab-indented lines in `FloorMapDrawable.cs`; every other file in the layer is
tab-indented with zero space-indented lines."* **The file is tab-indented, and the census is inverted.**
Measured on the post-change tree:

| | Measured | Command |
|---|---|---|
| Total lines | **898** | `wc -l` |
| Leading-SPACE lines | **0** | `grep -c '^ '` |
| Leading-TAB lines | **722** | `grep -c "^$TAB"` with `TAB=$(printf '\t')` |

The orchestrator measured **0 / 706 / 882** on the same three commands at the session base, so the
post-change figures differ by exactly the **+16** brace lines this session added for `MASTER-121` and the
two measurements reconcile rather than standing as two separate assertions. **POSITIVE CONTROL for the
leading-space predicate**, so the `0` is a measurement and not a blind predicate: the identical
`grep -c '^ '` returns **110** on
`src/DungeonMaster.Infrastructure/Migrations/20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs`.
**NOT MEASURED BY THIS SESSION: which session converted the file from spaces to tabs.** That needs git
archaeology nobody ran, and no row claims it; it is stated here rather than guessed.

*Correction 2 -- the two cited locations that were already clean, and both cited at the wrong path.*
Previous wording, verbatim, from the `locations:` block:
*"src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml.cs:28-36"* and
*"src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:91-109"*, and from `What.`:
*"`AreaSidebarItem.xaml.cs:28-36` and four doc comments in `FloorSidebarItem.xaml.cs:91-109` use the same
space form while the rest of each file uses tabs."* **Both files return 0 for the space predicate and
needed no `MASTER-122` change**, confirmed on both sides of the session rather than only after. Both also
live one directory deeper than `What.`'s prose implies -- `Controls/AreaSidebarItem/` and
`Controls/FloorSidebarItem/`, not `Controls/` -- which the `locations:` block happens to get right while
the prose does not.

*Correction 3 -- the remedied population, with its command and its controls.* Previous wording, verbatim,
from `Suggested direction.`: *"fix the six space-indented doc bodies"*. **The population was 124 lines
across 5 files, not six bodies.** Per-file, `grep -cE '^[[:space:]]*/// {2,}'`, measured by the architect
at the session base and re-run on the post-change tree:

| File | Base | Post-change | Tab form, post-change |
|---|---|---|---|
| `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs` | 116 | **0** | **116** |
| `src/DungeonMaster.App/Assets/ObjectAssets/ObjectAssetCatalogue.cs` | 2 | **0** | **2** |
| `src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs` | 2 | **0** | **2** |
| `src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs` | 1 | **0** | **1** |
| `src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs` | 3 | **0** | **3** |
| **Total** | **124** | **0** | **124** |

The tab column is `grep -cE "^[[:space:]]*///${TAB}"` with `TAB=$(printf '\t')`, and it is what makes the
five zeros a **conversion** rather than a deletion: the same 124 lines are present in the conforming
form. **THREE CONTROLS accompany those zeros.** (1) **POSITIVE CONTROL on the space predicate:** it
returns **35** on the excluded EF migration named in Correction 4, a file known to still hold the form.
(2) **POSITIVE CONTROL on the tab predicate, against a known-lying spelling:** the `TAB` form returns
**21** on `src/DungeonMaster.App/Controls/NotebookAffordance.cs` while
`grep -cE '^[[:space:]]*///\t'` returns a false **0** on that same file, because `\t` inside `grep -E`
matches a literal `t`. (3) **PREDICATE VALIDATED IN BOTH DIRECTIONS** on a purpose-built CRLF fixture:
a line with ONE space after `///` must not match and does not; lines with two and with five must and do,
2 of 4. **And one whole-tree pass closes the arithmetic:**
`grep -rnE '^[[:space:]]*/// {2,}' src --include=*.cs --exclude-dir=bin --exclude-dir=obj` returns
**35 matching lines in 1 file** post-change against **159 lines in 6 files** at the base, and
`159 - 124 = 35`. Note the `--exclude-dir` form: applying the `bin`/`obj` filter to the matched TEXT
instead of to PATHS is what produced this session's discarded artefact figure of 167.

*Correction 4 -- THE EF MIGRATION IS A STANDING EXEMPTION OF THIS REPOSITORY, NOT WORK THIS SESSION
SKIPPED. This is a convention this register now carries, and it is the answer to a question
`MASTER-181`'s own row text raised and declined to settle.* Previous wording, verbatim, from
`MASTER-181` at `00-master-index.md:369`: *"the migration's 35 lines also need a judgment call this row
does not make, since migrations are generated artefacts and may be exempt"*. **The user decided at
Gate 2 that generated EF scaffolder output is EXEMPT from `dotnet-format.md` section 9's two-tab rule, as
a standing convention of this repository.** The evidence it rests on, measured here:

- **`src/DungeonMaster.Infrastructure/Migrations/20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs`
  carries 35 lines of the same space form** -- `grep -cE '^[[:space:]]*/// {2,}'` returns **35** -- and
  of those, **14 are at 5 spaces and 21 at 9 spaces**, the nested `<para>` width. **ALL 21 of the
  9-space lines in the whole measured population of 159 sit in this one file**, which is why the
  in-scope 124 were uniformly 5-space and one conversion rule covered them. The distribution is
  `grep -ohE '/// {2,}'` piped through an indent-width count: `14 5` and `21 9`.
- **The repository leaves scaffolder output in the generator's style, measured across the whole
  directory and not inferred from one file.** All **7** files under `Migrations/` are space-indented with
  **0** leading tabs, by `grep -c '^ '` and `grep -c "^$TAB"` per file with `TAB=$(printf '\t')`:
  `InitialCreate.Designer.cs` 154/0, `InitialCreate.cs` 116/0, `AddFloorDescription.Designer.cs` 157/0,
  `AddFloorDescription.cs` 21/0, this migration's `.Designer.cs` 155/0, this migration 110/0, and
  `AppDbContextModelSnapshot.cs` 153/0. Every space figure is non-zero, so the uniform `0` in the tab
  column is a measurement rather than a blind predicate.
- **Editing it is PROHIBITED, not merely out of scope.** `disciplines/database.md:15` states:
  *"Migrations are forward-only. A released migration is never edited; a mistake is corrected by a new
  migration."* A whitespace reflow of a released migration is exactly the edit that rule forbids, and no
  new migration can "correct" a doc-comment indent.
- **This resolved at inference rung 1 and was never escalated**, because the repository's own uniform
  practice and a discipline rule both answered it directly.

**The arithmetic, stated explicitly: 124 remedied + 35 exempt = 159, the full measured population.**
**NO OPEN ROW COVERS THE 35 LINES AFTER THIS SESSION, AND THAT IS DELIBERATE** -- nothing is owed, so no
row stays open for them. A future census that returns 35 for this predicate is reading the exemption,
not a regression. The record's severity, category, effort and risk are unchanged.**]**

### UIKIT-023 -- Four public types are declared in files named for a different type

```yaml
id: UIKIT-023
layer: UIKIT
severity: S4
category: structure
authority: conventions
citation: backend/dotnet-format.md#2. File Organization
locations:
  - src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs:9-19
  - src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs:33-39
  - src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs:52-58
  - src/DungeonMaster.App/Behaviors/HandCursorBehavior.cs:3-4
effort: XS
risk: low
independent: true
```

**What.** `NotebookView.xaml.cs` declares the public `NotebookKind` enum and the public
`NotebookTarget` and `NotebookSaveArgs` records ahead of `NotebookView` itself.
`HandCursorBehavior.cs` declares the public `BehaviorCursorShape` enum ahead of the behavior.
Section 2 states: "One type per file (class, record, interface, enum)" and "File name matches the
type name exactly".

**Why it matters.** All four are part of the kit's public contract -- `NotebookTarget` and
`NotebookSaveArgs` are what the host page constructs and receives -- yet a consumer looking for
them has to know they live inside a control's code-behind. `NotebookView.xaml.cs` is a XAML
code-behind partial, which makes it the least discoverable place in the layer to put a public DTO.

**Suggested direction.** Move each to its own file beside the type that uses it:
`Controls/NotebookView/NotebookKind.cs`, `NotebookTarget.cs`, `NotebookSaveArgs.cs`, and
`Behaviors/BehaviorCursorShape.cs`.

**[AMENDED 6b-xviii, 2026-10-09 -- this entry is CONFIRMED and acted on in full, its four destinations
are recorded as they actually exist, and the mechanism the session brief recommended for it DOES NOT
APPLY. Classification: CONFIRMED, the only one of this session's eight selected rows whose substance
and count were both correct as filed; the `locations:` line numbers were nonetheless stale.** The
register row is `MASTER-123` at `00-master-index.md:311`, closed **done**. This block follows the form
of the `AMENDED 6b-xvii` block on `CORE-028` at `01-domain-application.md:2156-2216`, cited **for its
form only** -- `AMENDED 6b-xiv` at `01-domain-application.md:1293` records that the block that one was
in turn modelled on *"carries three defects of its own"*. **Nothing below deletes any previous
wording.**

*Correction 1 -- the destinations, recorded as they exist rather than as proposed.* Previous wording,
verbatim, from `Suggested direction.`: *"Move each to its own file beside the type that uses it:
`Controls/NotebookView/NotebookKind.cs`, `NotebookTarget.cs`, `NotebookSaveArgs.cs`, and
`Behaviors/BehaviorCursorShape.cs`."* **All four were created at exactly those paths**, re-derived by
content search on the post-change tree with
`grep -nE '^[[:space:]]*public ' <file>`, which returns exactly one line for each:

| Type | File, as it exists | Declared at |
|---|---|---|
| `BehaviorCursorShape` | `src/DungeonMaster.App/Behaviors/BehaviorCursorShape.cs` | `:4` |
| `NotebookKind` | `src/DungeonMaster.App/Controls/NotebookView/NotebookKind.cs` | `:6` |
| `NotebookTarget` | `src/DungeonMaster.App/Controls/NotebookView/NotebookTarget.cs` | `:15` |
| `NotebookSaveArgs` | `src/DungeonMaster.App/Controls/NotebookView/NotebookSaveArgs.cs` | `:14` |

Each source file now declares only the type it is named for: `HandCursorBehavior.cs:9` and
`NotebookView.xaml.cs:14`, by the same predicate. `grep -rn 'enum BehaviorCursorShape' src --include=*.cs`
with `bin` and `obj` excluded **by path** returns exactly **1** line, so the enum is declared nowhere
else. **The predicate was validated against the near-misses it must exclude**, since a one-type-per-file
census is worthless if it cannot distinguish access modifiers: a widened form admitting any run of
modifiers before the type keyword returns the same single `public` declaration per source file, and the
two private declarations that must NOT match are present in `NotebookView.xaml.cs` at `:17`
(`private enum NotebookPane`) and `:80` (`private sealed class RibbonBrushes`). A narrower spelling used
earlier in this session matched only `public [sealed] [partial] (class|record|enum|struct|interface)` and
would miss a `public static` or `public abstract` form; it is adequate here and **must not be reused as
an "any public type" census elsewhere.**

*Correction 2 -- the `locations:` line numbers.* Previous wording, verbatim, the `locations:` block:
`NotebookView.xaml.cs:9-19`, `:33-39`, `:52-58` and `HandCursorBehavior.cs:3-4`. **All four were stale
at the session base**, where the orchestrator located the declarations at `NotebookView.xaml.cs:8`,
`:32`, `:51` and `HandCursorBehavior.cs:4`. They are now stale in a second and permanent way: the types
no longer live in those files at all. The citations are superseded by the table above rather than
corrected in place.

*Correction 3 -- THE RECOMMENDED MECHANISM DOES NOT APPLY TO THIS ROW, and no rename was recorded.* The
session brief advised reusing 6b-xvii's fix for `MASTER-111` -- *"a byte-identical `git mv` committed
alone so git records `R100`"*. **That works for a file whose single type is misnamed; it does not
describe this row.** Here each source file keeps its own correctly-named type and the extra types are
**extracted** into new files, so there is no rename for `git mv` to express and **no `R100` to produce**.
Stating it plainly because the alternative -- an entry that leaves a recommended mechanism unmentioned --
invites the next reader to assume it was used and to look for a rename in the history that is not there.

*On controls: NO correction in this block rests on an absence or a zero, which is why none carries a
POSITIVE CONTROL.* Every figure above is a positive count -- one `public` declaration per file, one
tree-wide declaration of the enum, four destinations that exist -- and **a command that returns a hit
has demonstrated its own non-blindness.** What a count-based predicate needs instead is proof it does
not over-match, and that is supplied above by the two private near-misses at `NotebookView.xaml.cs:17`
and `:80`, which are present in the file and correctly excluded. The record's severity, category,
effort and risk are unchanged.**]**

### UIKIT-024 -- Public types, members and parameters across the behaviors lack XML documentation

```yaml
id: UIKIT-024
layer: UIKIT
severity: S4
category: docs
authority: conventions
citation: backend/dotnet-format.md#9. XML Documentation Comments
locations:
  - src/DungeonMaster.App/Behaviors/HoverOrLongPressBehavior.cs:3-13
  - src/DungeonMaster.App/Behaviors/HoverOrLongPressBehavior.cs:15-43
  - src/DungeonMaster.App/Behaviors/CanvasLongPressDetector.cs:3-10
  - src/DungeonMaster.App/Behaviors/CanvasLongPressDetector.cs:12-41
  - src/DungeonMaster.App/Behaviors/HandCursorBehavior.cs:14
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:23
  - src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml.cs:134-137
  - src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:208-211
effort: S
risk: low
independent: true
```

**What.** Section 9 requires that "All public types, members, and parameters must have XML
documentation comments". `HoverOrLongPressBehavior` documents nothing at all -- not the type, its
three public events, or its two overrides. `CanvasLongPressDetector` documents nothing -- not the
type, the `LongPressed` event, `TouchStarted`, `TouchEnded` or `Dispose`.
`HandCursorBehavior.Shape` is public and undocumented. `FloorMapDrawable`'s public constructor is
undocumented. `AreaSidebarItem.SetEditing(bool editing)` and
`FloorSidebarItem.SetAreaEditing(Guid areaId, bool editing)` have `<summary>` but no `<param>`
tags for their three parameters.

**Why it matters.** The rest of the layer is documented to an unusually high standard -- `DmPopup`,
`DmTheme` and `NotebookAffordance` carry substantial reasoning in their remarks -- so the gaps read
as oversights rather than as a different policy. `CanvasLongPressDetector` is the case where it
costs most: it is the only `IDisposable` in the layer, and nothing states who owns the disposal or
what thread `LongPressed` is raised on.

**Suggested direction.** Add the missing `<summary>` and `<param>` tags, following section 9's
two-tab body indentation and complete-sentence rules.

**[AMENDED 6b-xviii, 2026-10-09 -- SIX of this entry's EIGHT locations were already dead when this
session opened, one of them because the member it names NO LONGER EXISTS, and the entry's effort rating
no longer described the work. Classification: OVERTAKEN -- the entry's substance was correct when filed
and four of its gaps were closed by other sessions without being recorded here; the two that survived
are remedied. Effort corrected `S` -> `XS`.** The register row is `MASTER-136` at
`00-master-index.md:324`, closed **done**, and that row's effort cell now reads `XS`. This block follows
the form of the `AMENDED 6b-xvii` block on `CORE-028` at `01-domain-application.md:2156-2216`, cited
**for its form only** -- `AMENDED 6b-xiv` at `01-domain-application.md:1293` records that the block that
one was in turn modelled on *"carries three defects of its own"*. **Nothing below deletes any previous
wording.**

*Correction 1 -- the live scope was 2 of 8, and the effort rating.* Previous wording, verbatim, the
`effort:` key: *"effort: S"*, on eight `locations:` entries. **Two locations were live.** Both are
remedied, re-derived by content on the post-change tree:

- **`HandCursorBehavior.Shape`** now carries a `<summary>` at `:11-15` above the member at `:16`, with
  every continuation line in the `///` + two-tabs form section 9 prescribes. The entry cited it at
  `:14`, which was **exact** at the session base -- the one citation in this entry that was not stale.
- **`FloorSidebarItem.SetAreaEditing`** now carries `<param name="areaId">` at `:296-298` and
  `<param name="editing">` at `:299-301`, above the signature at `:302`. Both `name=` values match the
  declared `(Guid areaId, bool editing)`. **The pre-existing `<summary>` at `:290-295` is unchanged.**

*Correction 2 -- the four locations that "document nothing at all" and in fact document everything.*
Previous wording, verbatim, from `What.`: *"`HoverOrLongPressBehavior` documents nothing at all -- not
the type, its three public events, or its two overrides. `CanvasLongPressDetector` documents nothing --
not the type, the `LongPressed` event, `TouchStarted`, `TouchEnded` or `Dispose`."* **Both files are
fully documented and were before this session began** -- no task owns either. Measured post-change:
`grep -c '///'` returns **42** on `HoverOrLongPressBehavior.cs` (159 lines) and **38** on
`CanvasLongPressDetector.cs` (101 lines), and a detector for a `public` declaration whose nearest
preceding non-blank line is not a `///` line returns **0 undocumented public members** on each.
**POSITIVE CONTROL beside those zeros:** the identical detector returns **1** on
`src/DungeonMaster.App/Assets/ObjectAssets/ObjectAssetCatalogue.cs`, flagging
`public static IReadOnlyDictionary<ObjectKind, string> Paths => _paths;` at `:14` -- and the detector was
controlled to flag an undocumented member and NOT a documented one on a purpose-built fixture.

*Correction 3 -- the `FloorMapDrawable.cs:23` citation, stale twice over.* Previous wording, verbatim,
from `What.`: *"`FloorMapDrawable`'s public constructor is undocumented."* **The constructor is at
`:132` and IS documented**, by a `<summary>` at `:129-131`; the same detector returns **0** undocumented
public members on that file. And `:23` is not a constructor: it now holds `///` + two tabs + `<para>`,
a doc-comment line inside the type-level `<remarks>` block.

*Correction 4 -- a cited member that NO LONGER EXISTS ANYWHERE.* Previous wording, verbatim, from
`What.`: *"`AreaSidebarItem.SetEditing(bool editing)` and
`FloorSidebarItem.SetAreaEditing(Guid areaId, bool editing)` have `<summary>` but no `<param>` tags for
their three parameters."* **`SetEditing` does not exist anywhere in `DungeonMaster.App`.**
`grep -rn 'SetEditing' src/DungeonMaster.App --include=*.cs`, with `bin` and `obj` excluded by path,
returns **0**. **POSITIVE CONTROL beside that zero, because an absence reported by an uncontrolled grep
is not a measurement:** the same command shape for `SetAreaEditing` returns **3** --
`FloorSidebarItem.xaml.cs:302` (the declaration), `FloorSidebarItem.xaml.cs:319` (a `<see cref=>` in a
neighbouring doc comment) and `DungeonDetailPage.xaml.cs:2213` (the single call site). Note also that
`SetEditing` is not a substring of `SetAreaEditing`, so the zero is not an artefact of the longer name
shadowing the shorter. The entry's *"their three parameters"* is therefore a count over a population of
two members where one was already gone.

*The layer-wide `<param>` figure, recorded AS A BOUND and not as a population.* This session's
`<param>`-coverage detector reports **14** public members across all 58 `.cs` files of
`DungeonMaster.App` that take parameters and carry a doc block with no `<param>` tag. **That figure is
an UPPER BOUND, not the genuine population, and it has at least two known false positives:**
`FloorSidebarItem.ContainsKeyboardFocus => HasFocusedDescendant(this)`, where an expression body's
parentheses are mistaken for a parameter list, and `EventHandler<(Guid, int)>? ReorderRequested`, where a
generic event type resembles one. **THE EXACT GENUINE FIGURE WAS NOT MEASURED BY THIS SESSION.** It is
recorded here only so the bound is not mistaken for a count by a later reader who finds it in this
session's artifacts; **only `SetAreaEditing` was ever inside this row's scope**, either way. The
record's severity, category and risk are unchanged; **effort is corrected from `S` to `XS`.**]**

### UIKIT-025 -- Blank-line and trailing-line formatting deviations

```yaml
id: UIKIT-025
layer: UIKIT
severity: S4
category: style
authority: conventions
citation: backend/dotnet-format.md#5. Blank Lines Within Method Bodies
locations:
  - src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:255-259
  - src/DungeonMaster.App/Controls/AreaSidebarItem/AreaSidebarItem.xaml.cs:153-155
effort: XS
risk: low
independent: true
```

**What.** In `OnCardTapped`, the `SelectRequested?.Invoke(...)` at line 259 directly follows the
closing brace of a multi-line `if` block with no blank line between them. Section 5 requires a
blank line "before a statement when ... the preceding statement spans multiple lines". Separately,
`AreaSidebarItem.xaml.cs` carries a stray blank line at 154 before the type's closing brace.

**Why it matters.** No behavioural consequence; both are straightforward deviations from a rule the
rest of the layer follows consistently.

**Suggested direction.** Insert the blank line at `FloorSidebarItem.xaml.cs:259`, delete the blank
line at `AreaSidebarItem.xaml.cs:154`.

**[AMENDED 6b-xviii, 2026-10-09 -- this entry is FALSIFIED. Both of its locations are clean, the defect
CLASS is absent from both files, and the entry MISREADS the section it cites -- so its premise did not
follow from its authority even on the day it was filed. Classification: FALSIFIED, on two independent
grounds, one of them an ORIGINAL ERROR rather than drift. NO CODE CHANGED for this row.** The register
row is `MASTER-124` at `00-master-index.md:312`, closed **FALSIFIED**. This block follows the form of
the `AMENDED 6b-xvii` block on `CORE-028` at `01-domain-application.md:2156-2216`, cited **for its form
only** -- `AMENDED 6b-xiv` at `01-domain-application.md:1293` records that the block that one was in
turn modelled on *"carries three defects of its own"*. **Nothing below deletes any previous wording.**

*Correction 1 -- location 1, where the blank line this entry asks for is already present.* Previous
wording, verbatim, from `What.`: *"In `OnCardTapped`, the `SelectRequested?.Invoke(...)` at line 259
directly follows the closing brace of a multi-line `if` block with no blank line between them."*
**It does not.** Re-derived by content on the post-change tree -- `grep -n 'OnCardTapped'` puts the
method at `:423` of a **554**-line file, not at `:255-259` of the file this entry assumed -- the body
reads:

```
:425  if (_suppressCardTap)
:426  {
:427      _suppressCardTap = false;
:428      return;
:429  }
:430  (blank)
:431  IsExpanded = true;
:432  SelectRequested?.Invoke(this, EventArgs.Empty);
```

**The blank line is at `:430`**, between the `if` block's closing brace at `:429` and the statement at
`:431`. Anchored on the lines and on what is on them, not derived by offset from the entry's `:255-259`.

*Correction 2 -- location 2, where the stray blank line does not exist.* Previous wording, verbatim,
from `What.`: *"Separately, `AreaSidebarItem.xaml.cs` carries a stray blank line at 154 before the
type's closing brace."* **It does not.** The file is **261** lines and its final four are
`:258 command.Execute(area);`, `:259 }`, `:260 }`, `:261 }` -- **no blank line before the type's closing
brace.** The entry's `:153-155` citation now lands inside a documented property, so the citation is
stale as well as the claim being false. No task in this session owns this file, so this is its state at
the session base too.

*Correction 3 -- THE DEFECT CLASS IS ABSENT FROM BOTH FILES, not merely the two instances.* Closing a
formatting row on two re-read locations would leave open whether the form simply moved elsewhere in the
same files, so the class was censused rather than the instances. A detector for **a line that is only a
closing brace, immediately followed at the same indentation by a statement, with no blank line
between** returns **0** on both files. **The detector was controlled in three directions**, and the
third one mattered: exactly **1** on a fixture with one such case; **0** on a fixture where the same
pair is blank-separated; and on a fixture containing a legal `}`-then-`else` continuation it returns
**1**, flagging only the genuine case and skipping the `else`. **That third control caught a real false
positive in this detector's first version**, which flagged `FloorSidebarItem.xaml.cs:266` -- a `}`
followed by `else` at the same indent, which is correct C# and not the defect class. Without it this
block would have reported a defect that is not one. **POSITIVE CONTROL beside the two zeros:** the
corrected detector returns **13** on
`src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` in the same tree, so the zeros
are measurements and not a blind predicate.

*Correction 4 -- THE ENTRY MISREADS ITS OWN AUTHORITY, and this is an ORIGINAL ERROR, not drift.*
Previous wording, verbatim, from `What.`: *"Section 5 requires a blank line "before a statement when ...
the preceding statement spans multiple lines"."* **Section 5 requires one when the preceding statement
spans multiple lines DUE TO METHOD CHAINING OR 2+ WRAPPED ARGUMENTS.** The closing brace of an `if`
block is neither of those. **So even with the blank line genuinely absent, section 5 would not have
required it**, and this entry's premise did not follow from its citation when it was filed. This is the
half worth carrying: the two locations becoming clean makes the row closable, but the misread
authority is what makes it FALSIFIED rather than merely overtaken -- the same entry filed today against
the same code would still be wrong. The record's severity, category, effort and risk are
unchanged.**]**

### UIKIT-026 -- FloorMapDrawable declares a redundant empty public constructor

```yaml
id: UIKIT-026
layer: UIKIT
severity: S4
category: dead-code
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:23
effort: XS
risk: low
independent: true
```

**What.** `public FloorMapDrawable() { }` declares a parameterless constructor identical to the
implicit one the compiler would emit, on a class with no other constructor.

**Why it matters.** I believe it is simply dead: it carries no initialisation, no documentation and
no XAML requirement I could identify, since the drawable is constructed from code-behind rather
than from markup. It also sits between the field declaration and the properties, interrupting the
file's member ordering.

**Suggested direction.** Delete the line.

**[AMENDED 6b-xviii, 2026-10-09 -- EVERY CLAUSE of this entry is now false, and ITS STATED REMEDY IS
DESTRUCTIVE: following "Delete the line" WOULD BREAK THE BUILD AND SIX TESTS. Classification:
FALSIFIED, and flagged as a HARMFUL SUGGESTED DIRECTION rather than merely an inaccurate one. NO CODE
CHANGED for this row.** The register row is `MASTER-125` at `00-master-index.md:313`, closed
**FALSIFIED**. This block follows the form of the `AMENDED 6b-xvii` block on `CORE-028` at
`01-domain-application.md:2156-2216`, cited **for its form only** -- `AMENDED 6b-xiv` at
`01-domain-application.md:1293` records that the block that one was in turn modelled on *"carries three
defects of its own"*. **Nothing below deletes any previous wording.**

*Correction 1 -- all four clauses of the `What.` and `Why it matters.` paragraphs.* Previous wording,
verbatim, from `What.`: *"`public FloorMapDrawable() { }` declares a parameterless constructor identical
to the implicit one the compiler would emit, on a class with no other constructor."* And from
`Why it matters.`: *"I believe it is simply dead: it carries no initialisation, no documentation and no
XAML requirement I could identify"*. **Four separate claims, each false, each re-derived by content on
the post-change tree:**

| Claim | Measured | Command |
|---|---|---|
| The constructor is empty | **FALSE** -- it chains `: this(FloorMapPalette.FromTheme)` at `:133` | `grep -n -B6 -A3 'public FloorMapDrawable()'` |
| It is undocumented | **FALSE** -- `:129-131` is a `<summary>`: "Initializes a new instance that reads its colours from `DmTheme`." | same |
| The class has no other constructor | **FALSE** -- **three** constructors, at `:132`, `:148`, `:175` | `grep -n 'public FloorMapDrawable'` |
| It is dead | **FALSE** -- **7 live call sites** | the census below |
| It is at `:23` | **stale** -- it is at `:132`; `:23` is a doc-comment line inside the type-level `<remarks>` | same |

*Correction 2 -- the call-site census, with the predicate stated so the figure is reproducible.*
**7 call sites require a parameterless constructor.** The raw predicate returns a larger number and
that is recorded deliberately, so a later reader who reproduces it does not conclude the 7 was
invented: `grep -rn 'new FloorMapDrawable' src --include=*.cs`, with `bin` and `obj` excluded **by
path**, returns **13 lines**. Of those, **7 require the parameterless constructor**:

- `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:63` --
  `_drawable = new FloorMapDrawable();`, **production code**;
- `src/DungeonMaster.App.Tests/Graphics/FloorMapDrawableExecutionTests.cs:37`, and
  `src/DungeonMaster.App.Tests/Graphics/FloorMapDrawableNonFiniteTests.cs:69`, `:87`, `:105`, `:123`,
  `:141` -- six object-initializer uses, each confirmed by reading the following line, which is `{` in
  every case. **The `new FloorMapDrawable { ... }` form requires a parameterless constructor.**

**And 6 are NEAR-MISSES that must NOT count**, named because a census that cannot exclude them is not a
measurement: five are parameterised constructions --
`FloorMapDrawableDisposalTests.cs:43`, `:88`, `:102`, `:116` and `FloorMapDrawableStateTests.cs:187` --
and one is **not a call site at all**, `FloorMapDrawableNonFiniteTests.cs:9`, a doc-comment line that
names the form in `<c>` tags. `7 + 6 = 13`, so the filtered figure and the raw predicate reconcile.

*Correction 3 -- THE SUGGESTED DIRECTION IS DESTRUCTIVE AND MUST NOT BE FOLLOWED.* Previous wording,
verbatim: *"Delete the line."* **Deleting the parameterless constructor would remove the only
constructor that `new FloorMapDrawable()` and `new FloorMapDrawable { ... }` can bind to, breaking one
production call site and six test call sites.** The class's two remaining constructors both take
parameters, so no implicit parameterless constructor would be emitted to take its place -- the C#
compiler emits one only for a class that declares no constructor at all, which this class does not. The
build in this worktree reports `Build succeeded.` with **0 Warning(s)** and **0 Error(s)** across all
three `DungeonMaster.App` target frameworks, and the suite reports **1038 passed, 0 failed, 0
skipped**, with this row's code untouched. **This is the entry worth reading twice:** it is not merely
stale, it is an instruction that would have been carried out mechanically by a session trusting the
register's specifics, and the only thing standing between it and a broken build was re-measurement.

*On controls: NO correction in this block rests on an absence or a zero, which is why none carries a
POSITIVE CONTROL.* Every clause above is falsified by something **present** -- a chained constructor
initializer at `:133`, a `<summary>` at `:129-131`, three declarations, thirteen matched lines -- rather
than by something missing, and **a command that returns a hit has demonstrated its own non-blindness.**
The one risk a positive census carries is over-counting, and that is why the 13 matched lines are
partitioned above into 7 that count and 6 named near-misses that must not, rather than being reported
as a single number. The record's severity, category, effort and risk are unchanged.**]**

### UIKIT-027 -- DmConfirmPopup and NotebookView expose no bindable surface at all

```yaml
id: UIKIT-027
layer: UIKIT
severity: S5
category: api-design
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Controls/DmConfirmPopup/DmConfirmPopup.xaml.cs:30-56
  - src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs:142
  - src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs:171-256
effort: L
risk: high
independent: true
```

**What.** Of the ten types under `Controls/`, eight declare bindable properties. `DmConfirmPopup`
declares none -- its five inputs are constructor parameters with defaults. `NotebookView` declares
none either: its API is `Open(NotebookTarget)`, `Close()`, `SyncName(string)`, a plain CLR
`CurrentId` property, and a `SaveRequested` event.

**Why it matters.** I believe both choices are defensible on their own terms and I record this as
an observation, not a deviation. A popup constructed once per invocation has little to gain from
bindability, and `NotebookView`'s open/close protocol carries ordering guarantees -- flush the name
edit, commit the previous target, then repoint -- that a property setter would make it easy to
bypass. The note worth having is that the two are the only controls that cannot participate in a
data-bound page, which is a thing to know before anyone tries to move the designer page toward
MVVM: these two would have to be reshaped first.

**Suggested direction.** No action recommended today. If the host page later moves to declarative
binding, revisit `NotebookView` first -- `Open`'s ordering could be preserved behind a single
`Target` bindable property whose `propertyChanged` runs the same sequence.

### UIKIT-028 -- DmConfirmButton restates the full visual-state group its base already defines

```yaml
id: UIKIT-028
layer: UIKIT
severity: S5
category: duplication
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Resources/Theme/Controls.Semantic.xaml:35-66
effort: XS
risk: high
independent: true
```

**What.** `DmConfirmButton` derives from `DmButtonBase` via `BasedOn` and then declares its own
complete `CommonStates` group, repeating the base group's four states and restating
`{StaticResource DmConfirm}` and `{StaticResource DmConfirmText}` three times each.

**Why it matters.** I believe this is a genuine constraint of MAUI's style system rather than an
avoidable duplication: a derived style replaces the base group wholesale rather than merging into
it, so inheriting would drop the `Disabled` treatment the confirm button needs. The file's comment
at lines 10-16 says exactly this and records the regression that proved it. The observation worth
recording is only that the duplication *exists* and is invisible to `ThemeDictionaryTests` -- if
`DmButtonBase`'s hover or press opacity is ever retuned, this copy will silently not move, and
nothing would report it.

**Suggested direction.** No structural change; the current form is correct. A parity fact asserting
that `DmConfirmButton`'s `PointerOver` and `Pressed` opacity values match `DmButtonBase`'s would
close the silent-drift gap. T5 owns the theme suite that would house it.

**[AMENDED 6b-xviii, 2026-10-09 -- this entry is CONFIRMED and its judgment is upheld: the duplication
is DELIBERATE and no structural change was made. Its citation of the comment block that justifies the
duplication is wrong at both ends, and its named owner is a STALE TASK ID from an earlier session.
Classification: CONFIRMED and closed DELIBERATELY UNCHANGED; the `locations:` span is EXACT, the
in-prose citation is DRIFT, the owner is an ORIGINAL ERROR.** The register row is `MASTER-151` at
`00-master-index.md:339`, closed **DELIBERATELY UNCHANGED**. This block follows the form of the
`AMENDED 6b-xvii` block on `CORE-028` at `01-domain-application.md:2156-2216`, cited **for its form
only** -- `AMENDED 6b-xiv` at `01-domain-application.md:1293` records that the block that one was in
turn modelled on *"carries three defects of its own"*. **Nothing below deletes any previous wording.**

*Correction 1 -- the comment block's span, WRONG AT BOTH ENDS, and corrected at review rather than
first measurement.* Previous wording, verbatim, from `Why it matters.`: *"The file's comment at lines
10-16 says exactly this and records the regression that proved it."* **The block is
`src/DungeonMaster.App/Resources/Theme/Controls.Semantic.xaml:6-29`.** Re-derived by content: a
`grep -n` for the XML comment delimiters returns the opener at **`:6`** and the closer at **`:29`**;
`:5` is blank, `:10` is a prose line *inside* the block, `:30` is blank, and `:31` opens the
`DmConfirmButton` style. The file is **81** lines, 81 CRLF, and is byte-identical to its state at the
session base. **The span was ALSO wrong in this session's own evidence file**, which recorded `:10-30`
-- likewise wrong at both ends -- and from there the figure reached `manifest.json`'s frozen `T8`
acceptance criteria in two places before being caught at `T8`'s review and corrected in
`measurement.md` as its `CORRECTION 6`. **No verdict anywhere depended on the bounds**, and every
substantive claim attributed to the block is present at `:6-29`. It is recorded here with its
provenance so the next session's criteria do not inherit it a third time; what caught it was the frozen
criteria's own instruction to re-derive rather than carry forward.

*Correction 2 -- the entry's judgment, UPHELD, with the block's load-bearing sentences located.*
Previous wording, verbatim: *"I believe this is a genuine constraint of MAUI's style system rather than
an avoidable duplication: a derived style replaces the base group wholesale rather than merging into it,
so inheriting would drop the `Disabled` treatment the confirm button needs."* **Confirmed, and the file
says so itself.** At `:13-14` the block states that a derived style replaces the base group wholesale;
at `:15-16` it records the regression that proved it -- the disabled CREATE button in
`CreateDungeonPopup` turning from grey-brown back to green at 0.45 opacity; and at `:25-28` it warns in
capitals that **`DmDestructiveButton` MUST NOT GAIN A `CommonStates` GROUP OF ITS OWN**. The structure
is as the entry describes: `DmConfirmButton` at `:31` with `BasedOn="{StaticResource DmButtonBase}"`, its
own `CommonStates` group under a `Setter` spanning `:35-66` with states `Normal` `:38`, `PointerOver`
`:45`, `Pressed` `:50`, `Disabled` `:57`; and `DmDestructiveButton` at `:69` and `DmNeutralButton` at
`:75`, both `BasedOn` with no group of their own. **This entry's `locations:` span of `:35-66` is EXACT**
-- it is the `Setter` element, opening at `:35` and closing at `:66`. It is therefore one of only **two**
of this session's eight selected rows whose `locations:` survived unchanged, the other being
`MASTER-148`'s; an earlier figure in this session's artifacts called `MASTER-148`'s the only one, and
that is corrected here by measurement.

*Correction 3 -- the named owner is a STALE TASK ID.* Previous wording, verbatim, from
`Suggested direction.`: *"T5 owns the theme suite that would house it."* **"T5" is a task id from an
earlier session and names nothing in this one.** The suite is **`src/DungeonMaster.Tests/App/Theme/`**,
which holds **21** files, of which **11** read the theme XAML off disk post-change, with
`ThemeSource.cs` as the shared reader. Across all four test projects the same predicate returns **13**
files post-change and therefore **12** at the base. **A caution on that figure's command**, because the
spelling matters: the form recorded in this session's evidence file uses `grep -rln` with a `|`
alternation and **no `-E`**, in which `|` is a literal and which returns **0** here -- the figure is
right but that spelling could not have produced it. The `-E` form is what measures it.

*Correction 4 -- the gap was real, and it was unguarded because nothing in the suite named either
style.* Previous wording, verbatim: *"The observation worth recording is only that the duplication
*exists* and is invisible to `ThemeDictionaryTests` -- if `DmButtonBase`'s hover or press opacity is
ever retuned, this copy will silently not move, and nothing would report it."* **Confirmed, and now
closed by a test rather than a refactor.** Before this session, **no file in any of the four test
projects named `Controls.Semantic` or `DmConfirmButton`**; the count is now **1**, the new
`src/DungeonMaster.Tests/App/Theme/ConfirmButtonStateParityTests.cs`. **POSITIVE CONTROL beside that
prior zero:** the same grep shape for `DmButtonBase` returns **4** files, so the grep is not silently
empty and the 0 was a measurement. The guard is **3 facts**, at `:86`, `:123` and `:148`, and it is
scoped deliberately:

- **`PointerOver` and `Pressed` only**, each comparing the semantic style's parsed opacity to the base
  style's parsed opacity -- **no literal appears in either assertion.** Exactly one literal `0.82`
  appears anywhere in the file, at **`:117`**, inside a doc comment explaining why it is *not* asserted.
  The precise true statement is **"no literal appears in any assertion"**, not "no literal appears in
  the file": an earlier universal of the latter form was recorded in this session and is false.
- **`Disabled` is EXCLUDED ON PURPOSE because it diverges on purpose** -- measured from the XAML,
  `Opacity` is **`0.6`** at `Controls.Semantic.xaml:61` against the base's **`0.45`** at
  `Controls.Base.xaml:85`. A parity fact over `Disabled` would be wrong, not merely redundant.
- **`Normal` is EXCLUDED** because it sets `BackgroundColor` and `TextColor` rather than opacity alone,
  which is the mechanism the block at `:18-23` explains.
- A **positive-control fact** at `:86` proves the scan reaches both styles in both compared states, so a
  parity assertion cannot pass by resolving against nothing on both sides.

**Neither theme dictionary was modified**: `Controls.Base.xaml` is 157 lines and `Controls.Semantic.xaml`
81, both byte-identical to the session base. The record's severity, category, effort and risk are
unchanged.**]**

### UIKIT-029 -- BookmarkTabShape's notch clamp permits the degenerate case its comment describes

```yaml
id: UIKIT-029
layer: UIKIT
severity: S5
category: correctness
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.App/Controls/BookmarkTabShape.cs:34-36
effort: XS
risk: low
independent: true
```

**What.** The comment at lines 34-35 states "Never let the notch swallow the whole ribbon -- a
degenerate path renders as nothing at all". The clamp is
`Math.Min(NotchDepth, bounds.Height)`, which permits `notch == bounds.Height` exactly: at that
value the notch apex sits on the top edge and the shape collapses to two triangles meeting at a
point.

**Why it matters.** I believe this is unreachable in practice rather than a live defect: the only
consumer is `NotebookView.xaml`, whose ribbons are 40 or 60 units tall against the default
`NotchDepth` of 10, and `NotchDepth` is a plain CLR property with no binding on it. The record is
that the guard is one comparison weaker than the invariant its own comment states, which is the
kind of gap that matters if the shape is ever reused at a smaller size.

**Suggested direction.** If touched for another reason, tighten to
`Math.Min(NotchDepth, bounds.Height * 0.5)` or clamp against `bounds.Height - 1`. Not worth a
dedicated change.

**[AMENDED 6b-xviii, 2026-10-09 -- this entry is CONFIRMED with its citation EXACT, the first of its two
suggested clamps was adopted and THE SECOND IS REJECTED AS WORSE THAN THE DEFECT IT FIXES, and the
unreachability argument it rests on is NOT CONFIRMED because the figures it cites are not in the file it
cites. Classification: CONFIRMED and remedied; the `locations:` span was EXACT at the session base; the
"40 or 60 units tall" premise is MIS-LOCATED.** The register row is `MASTER-148` at
`00-master-index.md:336`, closed **done**. This block follows the form of the `AMENDED 6b-xvii` block on
`CORE-028` at `01-domain-application.md:2156-2216`, cited **for its form only** -- `AMENDED 6b-xiv` at
`01-domain-application.md:1293` records that the block that one was in turn modelled on *"carries three
defects of its own"*. **Nothing below deletes any previous wording.**

*Correction 1 -- the clamp, and the citation that was exact.* Previous wording, verbatim, from `What.`:
*"The clamp is `Math.Min(NotchDepth, bounds.Height)`, which permits `notch == bounds.Height` exactly"*.
**The clamp is now `Math.Min(NotchDepth, bounds.Height * 0.5)`**, re-derived by content at
`src/DungeonMaster.App/Controls/BookmarkTabShape.cs:42`, with its explanatory comment at `:34-41` in a
**54**-line file. **This entry's `locations:` citation of `:34-36` was EXACT at the session base** --
comment `:34-35`, clamp `:36` -- and it is one of only **two** of this session's eight selected rows
whose line numbers survived unchanged there, the other being `MASTER-151`'s `:35-66`. It is now stale by
this session's own edit, the comment having grown by six lines, which is why the positions above are
re-derived rather than carried.

*Correction 2 -- THE SECOND SUGGESTED CLAMP IS REJECTED, and the reason is not cosmetic.* Previous
wording, verbatim, from `Suggested direction.`: *"tighten to `Math.Min(NotchDepth, bounds.Height * 0.5)`
or clamp against `bounds.Height - 1`."* **`bounds.Height - 1` was rejected and is absent from the file**
(`grep -c 'Height - 1'` returns **0**). **POSITIVE CONTROL beside that zero:** the identical
`grep -c` form returns **1** for `Height \* 0.5` and **2** for `bounds.Height` on the same file, so the
predicate demonstrably sees text that is present. Two independent reasons for the rejection:

1. **For `bounds.Height < 1` it yields a NEGATIVE notch.** At `Height = 0.5`, `Height - 1 = -0.5`, so
   `bottom - notch` places the notch apex **0.5 units BELOW `bounds.Bottom`** -- outside the bounds
   entirely, inverting the notch into a spike. **That is a worse artefact than the degeneracy it is
   meant to fix:** the degenerate case renders as nothing, which reads as a missing control, while the
   inverted case renders geometry outside the shape's own rectangle.
2. **`1` is an absolute floor in device-independent units and does not scale.** `* 0.5` keeps a strictly
   positive band of ribbon above the apex at every positive height and scales with the ribbon, which is
   the invariant the comment at `:34-35` was always claiming.

*Correction 3 -- the change is invisible to today's rendering, stated as arithmetic rather than as a
claim about the running app.* Both consumers pass `NotchDepth="10"`, re-derived at
`src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml:195` and `:222`, and
`Math.Min(10, H * 0.5) == 10` for every `H >= 20` -- so for any ribbon at least 20 units tall the
computed notch is bit-identical under the old clamp and the new one. `NotchDepth` is a plain CLR
property at `:16` (`public double NotchDepth { get; set; } = 10;`) and `grep -c 'BindableProperty'`
returns **0** on the file, so nothing can bind it to a smaller value at runtime. **POSITIVE CONTROL
beside that second zero, and the first candidate for it FAILED, which is why two were tried:** the
identical command returns **6** on `src/DungeonMaster.App/Controls/DmCard/DmCard.xaml.cs`, a control
that works -- but it returns **0** on `src/DungeonMaster.App/Controls/NotebookAffordance.cs`, which was
tried first and is therefore **no control at all**. A control file has to be one the predicate is known
to fire on, verified rather than assumed. **The guard added is
3 test facts comprising 7 cases**, at `src/DungeonMaster.App.UnitTests/Controls/BookmarkTabShapeNotchTests.cs`:
a default-depth premise fact at `:59`, `ApexStaysStrictlyBelowTheTopEdge_AtHeightsUnderTwiceTheNotchDepth`
at `:79` over heights 10, 15 and 19, and
`NotchIsExactlyTheNotchDepth_AtHeightsOfAtLeastTwiceTheNotchDepth` at `:109` over 20, 40 and 60 -- the
second of which is a positive control proving the production geometry did not move.

*Correction 4 -- THE UNREACHABILITY ARGUMENT IS NOT CONFIRMED, AND ITS FIGURES ARE NOT WHERE THIS ENTRY
PUTS THEM.* Previous wording, verbatim, from `Why it matters.`: *"the only consumer is
`NotebookView.xaml`, whose ribbons are 40 or 60 units tall against the default `NotchDepth` of 10"*.
**`NotebookView.xaml` declares no height on either ribbon.** The file says so itself, at `:181-186`:
*"NO HeightRequest HERE, ON EITHER RIBBON. Both heights are owned by SelectedRibbonHeight /
UnselectedRibbonHeight in the code-behind ... The XAML used to declare 60 and 40 as well -- values that
never survived to the screen and could drift from the consts without any symptom until someone edited
the wrong one."* The figures live in the **code-behind**: `private const double SelectedRibbonHeight =
60;` at `NotebookView.xaml.cs:27` and `private const double UnselectedRibbonHeight = 40;` at `:30`,
assigned at `:363` and `:367`. **So the 40 and 60 are right in value and wrong in location**, and this
session's standing "ribbon heights NOT MEASURED" item is answered in a different file from the one it
names. **What remains genuinely unconfirmed is the step from the consts to the pixels:** these are
*requested* heights, and no command run in this session observes a rendered ribbon. The clamp binds only
below `H = 20`, and both consts are far above it, so the argument is sound on the evidence available --
but it rests on a static proxy for a layout property, and **it was NOT confirmed by observation.** The
record's severity, category, effort and risk are unchanged.**]**

## Layer verdict

This layer holds its shape better than any other presentation code I would expect to find at this
size, and the reason is the theme system. `Theme/DmTheme.cs` and the seven dictionaries under
`Resources/Theme/` are a genuine three-tier arrangement -- token, then base metrics, then semantic
and surface variants -- with the merge order, the token naming rule, C#-to-XAML key parity, style
orphaning and literal hygiene all enforced by facts that read the files off disk. The `DmTheme`
lifetime hazard is understood, documented and guarded. The classic copy-paste defect I looked for
across all nine bindable-property-bearing controls -- a `BindableProperty` whose `nameof` string or
declaring type does not match its own property -- **does not occur once**: every declaration uses
`nameof` and names its own type, defaults are sane, and `DmPopup` and `DmInlineEditor` correctly
use `defaultValueCreator` where a positional default would read resources too early. `DmPopup` is
the best-designed control in the kit.

The seams are where it weakens, and they weaken in one consistent direction: state that should be
declarative is imperative, and the boundary that should be the kit's own is the Application layer's.
Three of the ten control types name `AreaDto`/`FloorDto` directly (UIKIT-005), so the kit is not
reusable outside one query's projection. Selection and expansion live in private fields painted from
code-behind (UIKIT-011), which is the one part of the UI the theme dictionaries cannot reach --
exactly inverted. And in three controls a `propertyChanged` handler writes a property the XAML next
to it already binds (UIKIT-007). The converters are contract-compliant apart from the
`UnsetValue` gap (UIKIT-004). The behaviors are the highest-yield leak surface as expected, and one
real leak is there: `HoverOrLongPressBehavior` adds a pan recogniser it never removes (UIKIT-003).
`FloorMapDrawable` is the weakest file -- it blocks on async I/O inside the render path, never
disposes the images it owns, and is the only space-indented file in the layer.

Verdict: a sound, well-governed theme layer carrying a control kit that is not yet reusable. Fix the
DTO coupling and the imperative visual state and the two halves would match. T6 owns the App-to-
Application boundary record itself.

## Attestation

- No file under `src/` was created, modified, or deleted by this task: yes
- Every record cites at least one file path with a line number: yes
- Every record names its authority, and every non-`judgment` record carries a real citation: yes
