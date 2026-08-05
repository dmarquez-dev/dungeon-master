using System.Text.Json;
using System.Text.Json.Serialization;

namespace DungeonMaster.App.Assets.ObjectAssets;

internal static class PackManifestJsonOptions
{
	internal static readonly JsonSerializerOptions Default = new()
	{
		AllowTrailingCommas         = true,
		PropertyNameCaseInsensitive = true,
	};
}

internal sealed record PackManifest(
    [property: JsonPropertyName("displayName")]   string DisplayName,
    [property: JsonPropertyName("categoryOrder")] IReadOnlyList<string> CategoryOrder,
    [property: JsonPropertyName("objects")]       IReadOnlyList<PackObjectEntry> Objects,
    [property: JsonPropertyName("textures")]      IReadOnlyList<PackTextureEntry>? Textures,
    [property: JsonPropertyName("structures")]    IReadOnlyList<PackStructureEntry>? Structures);

internal sealed record PackObjectEntry(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("path")] string Path);

internal sealed record PackTextureEntry(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("path")] string Path);

internal sealed record PackStructureEntry(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("path")] string Path);
