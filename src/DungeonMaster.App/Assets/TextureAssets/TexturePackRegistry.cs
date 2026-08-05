using System.Text.Json;
using DungeonMaster.App.Assets.ObjectAssets;
using DungeonMaster.Domain.Entities.Area;
using Microsoft.Maui.Storage;

namespace DungeonMaster.App.Assets.TextureAssets;

public static class TexturePackRegistry
{
	public static IReadOnlyList<(string Id, string DisplayName)> AvailablePacks { get; } =
		[("2_Minute_Tabletop", "2 Minute Tabletop")];

	public static async Task<TexturePackLoadResult?> LoadPackAsync(
		string packId,
		CancellationToken ct = default)
	{
		var manifestPath = $"packs/{packId}/pack.json";
		try
		{
			await using var stream = await FileSystem.OpenAppPackageFileAsync(manifestPath);
			var manifest = await JsonSerializer.DeserializeAsync<PackManifest>(
				stream,
				PackManifestJsonOptions.Default,
				ct);
			if (manifest is null)
				return null;

			var texturePaths = new Dictionary<TextureKind, string>();
			foreach (var entry in manifest.Textures ?? [])
			{
				if (Enum.TryParse<TextureKind>(entry.Kind, out var kind))
					texturePaths[kind] = $"{packId}/{entry.Path}";
			}

			var structurePaths = new Dictionary<StructureKind, string>();
			foreach (var entry in manifest.Structures ?? [])
			{
				if (Enum.TryParse<StructureKind>(entry.Kind, out var kind))
					structurePaths[kind] = $"{packId}/{entry.Path}";
			}

			return new TexturePackLoadResult(
				manifest.DisplayName,
				texturePaths,
				structurePaths);
		}
		catch
		{
			return null;
		}
	}
}

public sealed record TexturePackLoadResult(
	string DisplayName,
	IReadOnlyDictionary<TextureKind, string> TexturePaths,
	IReadOnlyDictionary<StructureKind, string> StructurePaths);
