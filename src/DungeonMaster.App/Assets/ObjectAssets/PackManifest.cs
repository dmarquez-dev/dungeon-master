using System.Text.Json.Serialization;

namespace DungeonMaster.App.Assets.ObjectAssets;

internal sealed record PackManifest(
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("categoryOrder")] IReadOnlyList<string> CategoryOrder,
    [property: JsonPropertyName("objects")] IReadOnlyList<PackObjectEntry> Objects);

internal sealed record PackObjectEntry(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("path")] string Path);
