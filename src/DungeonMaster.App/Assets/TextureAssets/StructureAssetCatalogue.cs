using DungeonMaster.Domain.Entities.Area;

namespace DungeonMaster.App.Assets.TextureAssets;

/// <summary>
///		Holds the asset paths for the currently active structure pack.
/// </summary>
public static class StructureAssetCatalogue
{
	private static IReadOnlyDictionary<StructureKind, string> _paths =
		new Dictionary<StructureKind, string>();

	public static IReadOnlyDictionary<StructureKind, string> Paths => _paths;

	internal static void SetPaths(IReadOnlyDictionary<StructureKind, string> paths) =>
		_paths = paths;
}
