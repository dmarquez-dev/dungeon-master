using DungeonMaster.App.Assets.ObjectAssets;
using DungeonMaster.Domain.Entities.Floors;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace DungeonMaster.App.Tests.Graphics;

/// <summary>
///		Stands in for the app package that <c>FloorMapDrawable</c> loads object images from: it opens
///		the streams and decodes them, and it can be told to fail either half for one named kind.
/// </summary>
/// <remarks>
///		This is the test side of the drawable's two load seams. The real ones —
///		<c>FileSystem.OpenAppPackageFileAsync</c> and <c>PlatformImage.FromStream</c> — need a MAUI
///		host and yield a handle whose release cannot be observed, so neither the teardown release
///		(UIKIT-001 / MASTER-013) nor the overwrite-without-release (MASTER-160) is checkable through
///		them. Every image it hands out is a <see cref="RecordingImage"/>.
/// </remarks>
internal sealed class FakeAssetSource
{
	/// <summary>
	///		Every image this source decoded, in decode order.
	/// </summary>
	public List<RecordingImage> Decoded { get; } = [];

	/// <summary>
	///		The kind whose stream cannot be opened at all, or null for none. Stands for an asset
	///		missing from the pack.
	/// </summary>
	public ObjectKind? StreamFailsFor { get; set; }

	/// <summary>
	///		The kind whose stream decodes successfully and then throws when it is closed, or null for
	///		none. This is MASTER-160's trigger, and the only one.
	/// </summary>
	public ObjectKind? StreamThrowsOnCloseFor { get; set; }

	/// <summary>
	///		Opens the stream for one asset path. Wired to the drawable's <c>streamSource</c> seam.
	/// </summary>
	public Task<Stream> OpenAsync(string path)
	{
		var kind = KindOf(path);

		if (StreamFailsFor == kind)
			throw new FileNotFoundException($"no asset for {kind}", path);

		return Task.FromResult<Stream>(
			new LabelledStream(path, throwOnDispose: StreamThrowsOnCloseFor == kind));
	}

	/// <summary>
	///		Decodes an open stream into a recording image. Wired to the drawable's
	///		<c>imageFactory</c> seam.
	/// </summary>
	public IImage Decode(Stream stream)
	{
		var image = new RecordingImage { Label = ((LabelledStream)stream).Path };
		Decoded.Add(image);

		return image;
	}

	/// <summary>
	///		The image this source decoded for one kind, or null if it never decoded one.
	/// </summary>
	public RecordingImage? ImageFor(ObjectKind kind) =>
		Decoded.SingleOrDefault(image => image.Label == PathFor(kind));

	/// <summary>
	///		The asset path this source uses for a kind. Its shape does not matter to anything under
	///		test; only that kind and path map onto each other one to one.
	/// </summary>
	public static string PathFor(ObjectKind kind) =>
		$"fake-pack/Objects/{kind}.png";

	private static ObjectKind KindOf(string path) =>
		Enum.Parse<ObjectKind>(path["fake-pack/Objects/".Length..^".png".Length]);

	/// <summary>
	///		A stream that knows which asset it is, and that can be told to throw when it is closed.
	/// </summary>
	/// <remarks>
	///		A stream throwing from <c>Dispose</c> after handing out a decoded image is exactly the
	///		MASTER-160 window: the load's catch then had to store <c>null</c> over a live handle.
	/// </remarks>
	private sealed class LabelledStream(string path, bool throwOnDispose) : MemoryStream
	{
		public string Path { get; } = path;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (throwOnDispose)
				throw new IOException($"close of {Path} failed");
		}
	}
}

/// <summary>
///		Points <see cref="ObjectAssetCatalogue"/> at a chosen set of kinds for the duration of one
///		fact, and puts back whatever was there before.
/// </summary>
/// <remarks>
///		<para>
///		The catalogue is a static that only <c>ObjectPaletteView</c> fills, through an
///		<c>internal</c> setter, after loading a pack under a MAUI host. Outside a host it is EMPTY,
///		so <c>PreloadObjectImagesAsync</c> would iterate nothing and every disposal fact would pass
///		vacuously. Calling the setter is what makes the load path reachable at all from a host-free
///		fact; it arranges state and asserts nothing, so it is not the assertion-over-source evidence
///		class this session exists to replace. The setter is reachable because the App assembly grants
///		<c>InternalsVisibleTo("DungeonMaster.App.Tests")</c>, as the three other projects already do
///		for <c>DungeonMaster.Tests</c>.
///		</para>
///		<para>
///		The catalogue is process-wide state, so every fact that uses this scope must live in ONE
///		test class: xunit runs the facts of one class one at a time, and of different classes
///		concurrently.
///		</para>
/// </remarks>
internal sealed class ObjectCatalogueScope : IDisposable
{
	private readonly IReadOnlyDictionary<ObjectKind, string> _previous = ObjectAssetCatalogue.Paths;

	public ObjectCatalogueScope(IReadOnlyList<ObjectKind> kinds) =>
		ObjectAssetCatalogue.SetPaths(kinds.ToDictionary(kind => kind, FakeAssetSource.PathFor));

	/// <summary>
	///		The kinds the shipped pack declares, which is what the catalogue holds in the running app.
	/// </summary>
	/// <remarks>
	///		Counted from <c>src/DungeonMaster.App/Resources/Raw/packs/2_Minute_Tabletop/pack.json</c>,
	///		the one pack <c>ObjectPackRegistry.AvailablePacks</c> declares, and every one of its
	///		entries parses to an <see cref="ObjectKind"/>, so the catalogue holds one path per kind
	///		below. This is NOT the 37 members of the <see cref="ObjectKind"/> enum: the enum is the
	///		ceiling, the pack is what actually loads.
	/// </remarks>
	public static IReadOnlyList<ObjectKind> ShippedPackKinds { get; } =
	[
		ObjectKind.Altar,
		ObjectKind.Armchair,
		ObjectKind.Barrel,
		ObjectKind.Bed,
		ObjectKind.Bench,
		ObjectKind.Bookshelf,
		ObjectKind.Brazier,
		ObjectKind.Candle,
		ObjectKind.Cauldron,
		ObjectKind.Chair,
		ObjectKind.Chandelier,
		ObjectKind.Chest,
		ObjectKind.Crate,
		ObjectKind.Desk,
		ObjectKind.Dresser,
		ObjectKind.Lantern,
		ObjectKind.Oven,
		ObjectKind.Rug,
		ObjectKind.Shelf,
		ObjectKind.Statue,
		ObjectKind.Stool,
		ObjectKind.TableEnd,
		ObjectKind.TableRect,
		ObjectKind.TableRound,
		ObjectKind.Throne,
		ObjectKind.Torch,
		ObjectKind.Vase,
		ObjectKind.WeaponRack,
		ObjectKind.WickerBasket,
	];

	public void Dispose() =>
		ObjectAssetCatalogue.SetPaths(_previous);
}
