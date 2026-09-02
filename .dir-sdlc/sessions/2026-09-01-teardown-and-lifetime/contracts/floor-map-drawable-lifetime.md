# Contract — `FloorMapDrawable` lifetime and load seams

**Producer:** T2. **Consumers:** T5 (`DungeonDetailPage` disposes it at teardown and observes that it did).
Frozen at Gate 2. An implementer needing a change here stops and escalates.

Covers `MASTER-013` (`UIKIT-001`) drawable half and `MASTER-160`.
File: `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs`.

---

## Public surface

```csharp
namespace DungeonMaster.App.Graphics;

public sealed class FloorMapDrawable : IDrawable, IDisposable
{
	/// <summary>Reads colours from the live theme; loads assets from the app package.</summary>
	public FloorMapDrawable();

	/// <summary>Existing seam, unchanged.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="paletteSource"/> is null.</exception>
	public FloorMapDrawable(Func<FloorMapPalette> paletteSource);

	/// <summary>
	///     Full seam. <paramref name="streamSource"/> takes an app-package-relative asset path and
	///     yields the stream to decode; <paramref name="imageFactory"/> decodes it. The two together
	///     replace the direct <c>FileSystem.OpenAppPackageFileAsync</c> /
	///     <c>PlatformImage.FromStream</c> calls in <see cref="PreloadObjectImagesAsync"/>, which is
	///     what makes disposal observable without a MAUI host.
	/// </summary>
	/// <exception cref="ArgumentNullException">Any argument is null.</exception>
	public FloorMapDrawable(
		Func<FloorMapPalette> paletteSource,
		Func<string, Task<Stream>> streamSource,
		Func<Stream, Microsoft.Maui.Graphics.IImage> imageFactory);

	/// <summary>True once <see cref="Dispose"/> has run. Never returns to false.</summary>
	public bool IsDisposed { get; }

	public void Dispose();

	public void ClearObjectImageCache();

	public Task PreloadObjectImagesAsync();
}
```

The parameterless and single-argument constructors delegate to the three-argument one with
`FileSystem.OpenAppPackageFileAsync` and `PlatformImage.FromStream` as the defaults, so the
production path and the test path run the same code.

---

## Guarantees

### `Dispose()`

- Disposes every cached `IImage` — including the ones a caller never drew — then clears the cache.
- Idempotent. A second call disposes nothing further and does not throw.
- Sets `IsDisposed` to true. This property exists so a consumer that does not own the drawable
  (T5's page test) can observe that teardown ran; it is the *only* disposal signal this contract
  offers, and no consumer may reach for a private field instead.
- Per-entry failures are swallowed exactly as `ClearObjectImageCache()` already swallows them: one
  failing native release must not strand the remaining handles. `Dispose()` itself never throws.

### `ClearObjectImageCache()`

- Unchanged from what Session 4 landed: disposes each entry, swallowing per entry, then clears.
- Does **not** set `IsDisposed`. The cache may be refilled afterwards.
- Calling it after `Dispose()` is a no-op and does not throw.

### `PreloadObjectImagesAsync()`

- Per kind in `ObjectAssetCatalogue.Paths`: a cached non-null entry is skipped; otherwise a stream
  is obtained from `streamSource`, decoded by `imageFactory` **into a local**, the stream is closed,
  and only then is the local assigned into the cache.
- **`MASTER-160`:** if anything after a successful decode throws — the stream's `Dispose()` above all
  — the decoded local is disposed before the entry is set to `null`. A non-null cache entry is never
  replaced by `null` while still undisposed. This is the whole of the entry.
- A per-kind failure stores `null` and continues to the next kind, as today.
- After `Dispose()`, returns a completed task without loading anything. It does **not** throw
  `ObjectDisposedException`: the framework may drive the drawable after page teardown, and turning
  that into an exception would convert a leak into a crash.
- Threading is unchanged: every write happens on the calling thread, no `ConfigureAwait(false)`,
  no offload. The existing remarks on the method stay true.

### `Draw`

- After `Dispose()`, `Draw` renders the missing-object placeholder for every kind rather than
  throwing or touching a disposed handle.

---

## Error cases summary

| Situation | Surface |
|---|---|
| Null delegate to any constructor | `ArgumentNullException` naming the parameter |
| `streamSource` throws for a kind | Entry stored as `null`; loop continues |
| `imageFactory` throws for a kind | Entry stored as `null`; loop continues; no image was decoded so nothing to dispose |
| Stream `Dispose()` throws after a successful decode | Decoded image disposed, entry stored as `null`, loop continues (`MASTER-160`) |
| `IImage.Dispose()` throws during `Dispose()` / `ClearObjectImageCache()` | Swallowed per entry; remaining entries still released; caller sees no exception |
| Any public member called after `Dispose()` | No throw. Loads become no-ops, `Draw` paints placeholders |
