using DungeonMaster.Domain.Entities.Area;

namespace DungeonMaster.App.Assets.TextureAssets;

/// <summary>
///		Holds the asset paths for the currently active texture pack.
///		Updated by <see cref="TexturePaletteView"/> when the user selects a pack.
/// </summary>
public static class TextureAssetCatalogue
{
	private static IReadOnlyDictionary<TextureKind, string> _paths =
		new Dictionary<TextureKind, string>();

	public static IReadOnlyDictionary<TextureKind, string> Paths => _paths;

	internal static void SetPaths(IReadOnlyDictionary<TextureKind, string> paths) =>
		_paths = paths;
}
