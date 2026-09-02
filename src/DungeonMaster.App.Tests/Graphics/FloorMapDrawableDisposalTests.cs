using DungeonMaster.App.Core.Presentation;
using DungeonMaster.App.Graphics;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.Maui.Graphics;

namespace DungeonMaster.App.Tests.Graphics;

/// <summary>
///		Pins the lifetime of the object images <see cref="FloorMapDrawable"/> decodes and owns:
///		who releases them, when, and what happens to a handle whose load fails half way.
/// </summary>
/// <remarks>
///		<para>
///		UIKIT-001 / MASTER-013's drawable half: the drawable decoded <c>IImage</c> handles and had no
///		way to release them, so nothing did when the owning page was torn down. MASTER-160: a stream
///		that threw when it was closed, after a successful decode, left the load's catch storing
///		<c>null</c> over a live handle.
///		</para>
///		<para>
///		Every fact here runs the real load path. The two platform calls it used to make inline —
///		<c>FileSystem.OpenAppPackageFileAsync</c> and <c>PlatformImage.FromStream</c> — are now the
///		defaults behind two constructor seams, and a <see cref="FakeAssetSource"/> is passed in
///		instead, so what is exercised is the same code the app runs.
///		</para>
///		<para>
///		All of them live in ONE class deliberately: they arrange <c>ObjectAssetCatalogue</c>, which
///		is process-wide, and xunit runs the facts of one class one at a time.
///		</para>
/// </remarks>
public class FloorMapDrawableDisposalTests
{
	/// <summary>
	///		A drawable wired to a fake pack, together with the pack, for the life of one fact.
	/// </summary>
	private sealed class Harness : IDisposable
	{
		private readonly ObjectCatalogueScope _catalogue =
			new(ObjectCatalogueScope.ShippedPackKinds);

		public FakeAssetSource Assets { get; } = new();

		public FloorMapDrawable Drawable => _drawable ??= new FloorMapDrawable(
			() => MapTestScene.Palette,
			Assets.OpenAsync,
			Assets.Decode);

		private FloorMapDrawable? _drawable;

		public void Dispose()
		{
			_drawable?.Dispose();
			_catalogue.Dispose();
		}
	}

	/// <summary>
	///		A floor carrying one placed object of a kind the fake pack declares, so the draw path
	///		reaches the image cache rather than only the grid.
	/// </summary>
	private static FloorItem FloorWithOneBed() =>
		new(
			Guid.Parse("33333333-3333-3333-3333-333333333333"),
			1,
			"Disposal floor",
			null,
			null,
			[],
			[
				new PlacedObjectItem(
					Guid.Parse("44444444-4444-4444-4444-444444444444"),
					ObjectKind.Bed,
					new CanvasPoint(40f, 40f),
					ObjectRotation.None,
					ObjectScale.Default),
			]);

	/// <summary>
	///		A null palette source is rejected by name, so a caller that forgot one is told which
	///		argument it forgot.
	/// </summary>
	[Fact]
	public void Constructor_WithANullPaletteSource_ThrowsNamingThatParameter()
	{
		var assets = new FakeAssetSource();

		var thrown = Assert.Throws<ArgumentNullException>(() =>
			new FloorMapDrawable(null!, assets.OpenAsync, assets.Decode));

		Assert.Equal("paletteSource", thrown.ParamName);
	}

	/// <summary>
	///		A null stream source is rejected by name.
	/// </summary>
	[Fact]
	public void Constructor_WithANullStreamSource_ThrowsNamingThatParameter()
	{
		var assets = new FakeAssetSource();

		var thrown = Assert.Throws<ArgumentNullException>(() =>
			new FloorMapDrawable(() => MapTestScene.Palette, null!, assets.Decode));

		Assert.Equal("streamSource", thrown.ParamName);
	}

	/// <summary>
	///		A null image factory is rejected by name.
	/// </summary>
	[Fact]
	public void Constructor_WithANullImageFactory_ThrowsNamingThatParameter()
	{
		var assets = new FakeAssetSource();

		var thrown = Assert.Throws<ArgumentNullException>(() =>
			new FloorMapDrawable(() => MapTestScene.Palette, assets.OpenAsync, null!));

		Assert.Equal("imageFactory", thrown.ParamName);
	}

	/// <summary>
	///		A drawable that has not been disposed says so, which is the signal the owning page's
	///		teardown fact reads.
	/// </summary>
	[Fact]
	public void IsDisposed_BeforeDispose_IsFalse()
	{
		using var harness = new Harness();

		Assert.False(harness.Drawable.IsDisposed);
	}

	/// <summary>
	///		Disposing the drawable releases EVERY image it decoded, including the kinds no frame ever
	///		drew, and reports itself disposed. Preload fills one entry per kind the pack declares,
	///		not one per kind drawn, so the never-drawn ones are the bulk of what leaked.
	/// </summary>
	[Fact]
	public async Task Dispose_ReleasesEveryCachedImageIncludingKindsNeverDrawn()
	{
		using var harness = new Harness();
		await harness.Drawable.PreloadObjectImagesAsync();

		Assert.NotEmpty(harness.Assets.Decoded);
		Assert.Equal(ObjectCatalogueScope.ShippedPackKinds.Count, harness.Assets.Decoded.Count);
		Assert.All(harness.Assets.Decoded, image =>
			Assert.Equal(0, image.DisposeCount));

		harness.Drawable.Dispose();

		Assert.All(harness.Assets.Decoded, image =>
			Assert.Equal(1, image.DisposeCount));
		Assert.True(harness.Drawable.IsDisposed);
	}

	/// <summary>
	///		A second Dispose releases nothing further and does not throw, so a page torn down twice
	///		cannot double-release a native handle.
	/// </summary>
	[Fact]
	public async Task Dispose_CalledTwice_ReleasesNothingFurtherAndDoesNotThrow()
	{
		using var harness = new Harness();
		await harness.Drawable.PreloadObjectImagesAsync();
		harness.Drawable.Dispose();

		Assert.NotEmpty(harness.Assets.Decoded);

		harness.Drawable.Dispose();

		Assert.All(harness.Assets.Decoded, image =>
			Assert.Equal(1, image.DisposeCount));
		Assert.True(harness.Drawable.IsDisposed);
	}

	/// <summary>
	///		One image failing to release does not strand the others, and the failure does not reach
	///		the caller: teardown has nowhere to report it and one dead handle must not cost the rest.
	/// </summary>
	[Fact]
	public async Task Dispose_WhenOneImageThrowsOnRelease_StillReleasesTheOthersAndDoesNotThrow()
	{
		using var harness = new Harness();
		await harness.Drawable.PreloadObjectImagesAsync();

		Assert.NotEmpty(harness.Assets.Decoded);

		var failing = harness.Assets.Decoded[0];
		failing.ThrowOnDispose = true;

		harness.Drawable.Dispose();

		Assert.All(harness.Assets.Decoded, image =>
			Assert.Equal(1, image.DisposeCount));
	}

	/// <summary>
	///		Clearing the cache on a pack change releases each cached image but leaves the drawable
	///		alive, because the next pack's images are about to be loaded into it.
	/// </summary>
	[Fact]
	public async Task ClearObjectImageCache_ReleasesEachCachedImageAndLeavesTheDrawableNotDisposed()
	{
		using var harness = new Harness();
		await harness.Drawable.PreloadObjectImagesAsync();

		Assert.NotEmpty(harness.Assets.Decoded);

		harness.Drawable.ClearObjectImageCache();

		Assert.All(harness.Assets.Decoded, image =>
			Assert.Equal(1, image.DisposeCount));
		Assert.False(harness.Drawable.IsDisposed);
	}

	/// <summary>
	///		A preload after a cache clear loads every kind again, which is what a pack change is: the
	///		clear released the old images and left an empty cache to refill.
	/// </summary>
	[Fact]
	public async Task PreloadObjectImagesAsync_AfterClearObjectImageCache_RefillsTheCache()
	{
		using var harness = new Harness();
		await harness.Drawable.PreloadObjectImagesAsync();
		harness.Drawable.ClearObjectImageCache();

		await harness.Drawable.PreloadObjectImagesAsync();

		Assert.NotEmpty(harness.Assets.Decoded);
		Assert.Equal(
			ObjectCatalogueScope.ShippedPackKinds.Count * 2,
			harness.Assets.Decoded.Count);

		var refilled = harness.Assets.Decoded
			.Skip(ObjectCatalogueScope.ShippedPackKinds.Count)
			.ToList();

		Assert.NotEmpty(refilled);
		Assert.All(
			refilled,
			image => Assert.Equal(0, image.DisposeCount));
	}

	/// <summary>
	///		MASTER-160. When a stream throws as it is closed, after it has already handed back a
	///		decoded image, that image is released before the entry becomes null — so no live handle
	///		is ever overwritten. The later Dispose finds nothing to release for that kind, which is
	///		how a fact can tell "released and dropped" from "still held".
	/// </summary>
	[Fact]
	public async Task PreloadObjectImagesAsync_WhenTheStreamThrowsOnCloseAfterDecoding_ReleasesTheDecodedImageRatherThanStrandingIt()
	{
		using var harness = new Harness();
		harness.Assets.StreamThrowsOnCloseFor = ObjectKind.Bed;

		await harness.Drawable.PreloadObjectImagesAsync();

		var stranded = harness.Assets.ImageFor(ObjectKind.Bed);
		Assert.NotNull(stranded);
		Assert.Equal(1, stranded.DisposeCount);

		harness.Drawable.Dispose();

		Assert.Equal(1, stranded.DisposeCount);
	}

	/// <summary>
	///		A kind whose asset cannot be opened at all costs only that kind: every other kind the
	///		pack declares still loads. The shipped pack declares 29 kinds, so 28 load.
	/// </summary>
	[Fact]
	public async Task PreloadObjectImagesAsync_WhenOneKindsStreamCannotBeOpened_StillLoadsEveryOtherKind()
	{
		using var harness = new Harness();
		harness.Assets.StreamFailsFor = ObjectKind.Bed;

		await harness.Drawable.PreloadObjectImagesAsync();

		Assert.Equal(ObjectCatalogueScope.ShippedPackKinds.Count - 1, harness.Assets.Decoded.Count);
		Assert.Null(harness.Assets.ImageFor(ObjectKind.Bed));
	}

	/// <summary>
	///		A preload after teardown loads nothing and does not throw. The framework can drive a
	///		drawable after the page that owned it is gone, and an ObjectDisposedException there would
	///		turn a leak into a crash.
	/// </summary>
	[Fact]
	public async Task PreloadObjectImagesAsync_AfterDispose_LoadsNothingAndDoesNotThrow()
	{
		using var harness = new Harness();
		harness.Drawable.Dispose();

		await harness.Drawable.PreloadObjectImagesAsync();

		Assert.Empty(harness.Assets.Decoded);
	}

	/// <summary>
	///		A frame drawn after teardown paints the missing-object placeholder rather than throwing
	///		or reaching for a released handle, for the same reason.
	/// </summary>
	[Fact]
	public async Task Draw_AfterDispose_PaintsTheFrameWithoutThrowing()
	{
		using var harness = new Harness();
		harness.Drawable.Floor = FloorWithOneBed();
		await harness.Drawable.PreloadObjectImagesAsync();
		harness.Drawable.Dispose();

		var canvas = new RecordingCanvas();
		harness.Drawable.Draw(canvas, new RectF(0f, 0f, 200f, 200f));

		Assert.Contains("?", canvas.DrawnStrings);
	}
}
