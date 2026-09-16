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
