using System.Text.Json;
using DungeonMaster.Domain.Entities.Area;

namespace DungeonMaster.App.Assets.ObjectAssets;

public static class ObjectPackRegistry
{
    public static IReadOnlyList<(string Id, string DisplayName)> AvailablePacks { get; } =
        [("2_Minute_Tabletop", "2 Minute Tabletop")];

    public static async Task<PackLoadResult?> LoadPackAsync(string packId, CancellationToken ct = default)
    {
        var manifestPath = $"packs/{packId}/pack.json";
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(manifestPath);
            var manifest = await JsonSerializer.DeserializeAsync<PackManifest>(stream, cancellationToken: ct);
            if (manifest is null)
                return null;

            var paths = new Dictionary<ObjectKind, string>();
            foreach (var entry in manifest.Objects)
            {
                if (Enum.TryParse<ObjectKind>(entry.Kind, out var kind))
                    paths[kind] = $"{packId}/Objects/{entry.Path}";
            }

            // Build category → kinds map from paths (category = third component: packId/Objects/category/file).
            var byCat = paths
                .GroupBy(kv => kv.Value.Split('/')[2])
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<ObjectKind>)g.Select(kv => kv.Key).ToList());

            // Respect pack-defined category order; append any unlisted categories alphabetically.
            var ordered = manifest.CategoryOrder
                .Where(c => byCat.ContainsKey(c))
                .Concat(byCat.Keys.Except(manifest.CategoryOrder).OrderBy(c => c))
                .ToList();

            return new PackLoadResult(manifest.DisplayName, paths, ordered, byCat);
        }
        catch
        {
            return null;
        }
    }
}

public sealed record PackLoadResult(
    string DisplayName,
    IReadOnlyDictionary<ObjectKind, string> Paths,
    IReadOnlyList<string> CategoryOrder,
    IReadOnlyDictionary<string, IReadOnlyList<ObjectKind>> Categories);
