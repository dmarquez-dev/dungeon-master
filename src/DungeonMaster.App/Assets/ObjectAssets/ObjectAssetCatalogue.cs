using DungeonMaster.Domain.Entities.Floors;

namespace DungeonMaster.App.Assets.ObjectAssets;

/// <summary>
///     Holds the asset paths for the currently active object pack.
///     Updated by <see cref="ObjectPaletteView"/> when the user selects a pack.
/// </summary>
public static class ObjectAssetCatalogue
{
	private static IReadOnlyDictionary<ObjectKind, string> _paths =
		new Dictionary<ObjectKind, string>();

	public static IReadOnlyDictionary<ObjectKind, string> Paths => _paths;

	internal static void SetPaths(IReadOnlyDictionary<ObjectKind, string> paths) =>
		_paths = paths;
}
