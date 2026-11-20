using DungeonMaster.Domain.Entities.Floors;

namespace DungeonMaster.App.Assets.ObjectAssets;

/// <summary>
///		Holds the asset paths for the currently active object pack.
///		Updated by <see cref="DungeonMaster.App.Views.DungeonDetail.ObjectPaletteView"/> when the user selects a pack.
/// </summary>
public static class ObjectAssetCatalogue
{
	private static IReadOnlyDictionary<ObjectKind, string> _paths =
		new Dictionary<ObjectKind, string>();

	/// <summary>
	///		Gets the asset path for each object kind in the currently active object pack.
	/// </summary>
	public static IReadOnlyDictionary<ObjectKind, string> Paths => _paths;

	internal static void SetPaths(IReadOnlyDictionary<ObjectKind, string> paths) =>
		_paths = paths;
}
