# Contract — FloorMapDrawable public surface

Frozen at Gate 2, amended at Gate 2 after the user rescheduled MASTER-013.

**Scope: closes MASTER-025 (UIKIT-002, sync-over-async in the Draw path) only.**
**MASTER-013 (UIKIT-001, undisposed cached images) moved to Session 6** at Gate 2, together with
MASTER-006, MASTER-055 and MASTER-062, because those four are three leaks and a race, no test in this
repository executes App code, and structural assertion was judged insufficient evidence for that defect
class. Session 6 lands MASTER-026 and can demonstrate them rather than assert them.

One half of MASTER-013 nevertheless lands here, and the reason is in "The leak this task would otherwise
make worse" below. Read that section before implementing.

- **Producer:** T2 — owns `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs`
- **Consumer:** T3 — owns `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`
- **Asserted by:** T1 — owns `src/DungeonMaster.Tests/App/AsyncDiscipline/`

T2 and T3 edit different files and run concurrently. Neither may wait on the other; this document is the
only thing they share.

## Current shape, verified in the worktree at `e795365`

Verified by `grep -nE "^\t(public|private|internal|protected)" src/DungeonMaster.App/Graphics/FloorMapDrawable.cs`
run in `C:\Users\dmarq\Development\Repos\dungeon-master\.claude-worktrees\dir\2026-08-24-async-disposal-and-threading-discipline`:

- `public sealed class FloorMapDrawable : IDrawable` (line 18) — implements `IDrawable` only.
- `private readonly Dictionary<ObjectKind, Microsoft.Maui.Graphics.IImage?> _imageCache = new();` (line 21).
- `public void ClearObjectImageCache() => _imageCache.Clear();` (lines 26-27) — drops `IImage` references without disposing them.
- `private Microsoft.Maui.Graphics.IImage? LoadOrGetImage(ObjectKind kind)` (line 282) — on a cache miss calls
  `FileSystem.OpenAppPackageFileAsync(path).GetAwaiter().GetResult()` (line 304) and decodes with `PlatformImage.FromStream`.
- `LoadOrGetImage` is called from `DrawPlacedObject` (line 354) and `DrawGhostObject` (line 447), both reached from
  `public void Draw(ICanvas canvas, RectF dirtyRect)` (line 121), which `GraphicsView` invokes on the UI thread.

Consumer side, verified by
`grep -rn "FloorMapDrawable|ClearObjectImageCache" --include=*.cs src/` in the same worktree — the only
production consumer is `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`:

- line 27 — `private readonly FloorMapDrawable _drawable;`
- line 87 — `_drawable = new FloorMapDrawable();`
- line 100 — `_drawable.ClearObjectImageCache();` inside the `ObjectPalette.PackChanged += (_, _) => { ... }` lambda.

No test references `FloorMapDrawable`; the same grep returns no hit under `src/DungeonMaster.Tests`.

## The frozen surface after this session

T2 MUST produce exactly these members with exactly these signatures. T3 MUST call exactly these and no others.

### 1. The type does NOT implement `IDisposable` in this session

`FloorMapDrawable` keeps its current declaration:

```csharp
public sealed class FloorMapDrawable : IDrawable
```

No `Dispose()` method is added, and T3 adds no `_drawable.Dispose();` call. That is the page-teardown
half of MASTER-013 and it belongs to Session 6, because proving that the drawable's images are released
when the page is torn down needs a test that can execute App code, which does not exist until MASTER-026
lands.

### 2. `ClearObjectImageCache` disposes before it clears

```csharp
/// <summary>
///		Disposes every cached object image and clears the cache so assets reload from the active pack.
/// </summary>
public void ClearObjectImageCache()
```

The signature is unchanged from today. The behaviour changes: each non-null cached `IImage` is disposed
before `_imageCache.Clear()`. T3's existing call site at line 100 keeps compiling unchanged, and T3 needs
no new call.

Each disposal is wrapped so that one failing `IImage.Dispose()` does not prevent the remaining handles
from being released, with a comment at the catch site stating exactly that. (Backend discipline, "Never
swallow an error to keep a code path quiet"; the comment is the stated reason.)

#### The leak this task would otherwise make worse

This one method is carved out of MASTER-013 and kept in Session 4 deliberately, because **the preload in
section 3 amplifies the very leak that MASTER-013 describes, and it is this task that introduces the
amplification.**

Today `_imageCache` populates lazily: `LoadOrGetImage` decodes an image only for an `ObjectKind` actually
drawn on the current floor or held as the placement ghost. `ClearObjectImageCache()` then abandons that
partial set undisposed.

After section 3, `PreloadObjectImagesAsync()` populates an entry for **every** `ObjectKind` the active
pack declares in `ObjectAssetCatalogue.Paths`, drawn or not. `Paths` is
`IReadOnlyDictionary<ObjectKind, string>` (`src/DungeonMaster.App/Assets/ObjectAssets/ObjectAssetCatalogue.cs:14`)
and `ObjectKind` has **37 members**, verified by
`grep -cE "^\s+[A-Z][A-Za-z]*\s*(=|,)" src/DungeonMaster.Domain/Entities/Floor/ObjectKind.cs` in the
worktree. So the number of decoded, GPU-backed images abandoned per `ClearObjectImageCache()` call rises
from "the kinds the user actually drew" to "up to every kind in the pack", and that method is called on
**every pack change**.

Leaving the disposal out would therefore have made a known leak strictly worse as a side effect of fixing
an unrelated defect. Disposing inside `ClearObjectImageCache()` is three lines, needs no change in any
file T2 does not own, and holds the leak at or below where it is today. The page-teardown half stays in
Session 6 precisely because it *does* need a call site in another task's file.

### 3. A new asynchronous preload

```csharp
/// <summary>
///		Loads and decodes every image named by <see cref="ObjectAssetCatalogue.Paths"/> into the cache.
///		Must be called on the UI thread. Safe to call repeatedly; entries already cached are not reloaded.
/// </summary>
public async Task PreloadObjectImagesAsync()
```

Behavioural terms, all binding:

- It reads `DungeonMaster.App.Assets.ObjectAssets.ObjectAssetCatalogue.Paths`
  (`public static IReadOnlyDictionary<ObjectKind, string> Paths`, verified at
  `src/DungeonMaster.App/Assets/ObjectAssets/ObjectAssetCatalogue.cs:14`) and, for every entry whose
  `ObjectKind` is not already present in `_imageCache` with a non-null value, awaits
  `FileSystem.OpenAppPackageFileAsync(path)` and decodes the stream with `PlatformImage.FromStream`.
- A per-entry failure is caught, stores `null` for that `ObjectKind`, and does not abort the remaining
  entries. The catch site carries a comment saying the entry renders as the missing-object placeholder —
  the same reason the existing catch at line 310 already gives.
- **Threading:** every write to `_imageCache` happens on the thread that called the method. T2 MUST NOT
  introduce a `Task.Run`, a background thread, or any other write to `_imageCache` off the calling
  thread. `_imageCache` is a plain `Dictionary` read by `Draw` on the UI thread, and this session's whole
  premise is that no field is written on one thread and read on another.
- It never blocks. `GetAwaiter().GetResult()`, `.Result` and `.Wait()` MUST NOT appear anywhere in
  `FloorMapDrawable.cs` after this task. Checkable by running, in the worktree,
  `grep -n "GetAwaiter().GetResult()\|\.Result\b\|\.Wait()" src/DungeonMaster.App/Graphics/FloorMapDrawable.cs`
  and observing no matching line. That command returns exactly one line today
  (`304:				using var stream = FileSystem.OpenAppPackageFileAsync(path).GetAwaiter().GetResult();`).

### 4. `LoadOrGetImage` becomes a pure cache read

```csharp
private Microsoft.Maui.Graphics.IImage? LoadOrGetImage(ObjectKind kind)
```

Signature unchanged, and private, so it is not part of the surface T3 sees — but its behaviour is frozen
because T3's runtime observation depends on it: it performs a dictionary lookup and returns the value or
`null`. It touches no file system and starts no task. `Draw` renders the existing missing-object
placeholder (`DrawMissingObjectFallback`, line 422) on a miss, exactly as it does today for a failed load.

## What T3 must do with this surface

T3 makes exactly these two changes in `DungeonDetailPage.xaml.cs`:

1. The `ObjectPalette.PackChanged` lambda, which today reads (lines 98-102):

   ```csharp
   ObjectPalette.PackChanged += (_, _) =>
   {
   	_drawable.ClearObjectImageCache();
   	MapCanvas.Invalidate();
   };
   ```

   becomes a guarded asynchronous handler that clears, awaits `PreloadObjectImagesAsync()`, then
   invalidates. Because it is a lambda assigned to a `void`-returning event, it becomes `async void` and
   therefore falls under rule G1 of `async-discipline-rules.md`: it carries a `try` before its first
   `await` and routes the failure to `_viewModel.ErrorMessage`.

2. Preload once when the canvas handler first becomes available, from `OnMapCanvasHandlerChanged`
   (line 435) — the point at which the Win2D resource creator exists on Windows, which is the reason the
   current code clears the cache on pack change in the first place. This too is a guarded `async void`
   handler under G1.

**T3 adds no `_drawable.Dispose();` call.** An earlier draft of this contract required one in the
`if (Handler is null)` block of `OnHandlerChanged` (line 487). That requirement is withdrawn: MASTER-013's
page-teardown half moved to Session 6 at Gate 2, and `FloorMapDrawable` has no `Dispose()` to call. If you
find yourself writing that line, re-read section 1.

## Escalation

If T2 finds that `PreloadObjectImagesAsync` cannot populate the cache on Windows because the Win2D
resource creator is still unavailable at every point T3 can call it, that is a contract change: stop and
report it. Do not resolve it by reinstating a synchronous load inside `Draw`, and do not resolve it by
editing `DungeonDetailPage.xaml.cs` — T3 owns that file.

Neither task may re-add `IDisposable`, `Dispose()`, or a `_drawable.Dispose()` call on its own judgment.
That is Session 6's work, and a partial version of it landing here would leave Session 6 reviewing a
defect that is neither open nor closed.
