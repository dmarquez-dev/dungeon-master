using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Commands.FloodFillTileTextures;

public sealed class FloodFillTileTexturesCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<FloodFillTileTexturesCommand, int>
{
	public async Task<Result<int>> HandleAsync(FloodFillTileTexturesCommand command, CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(command.DungeonId, ct);

		if (dungeon is null)
			return Result<int>.Failure($"Dungeon {command.DungeonId} not found.", ErrorCategory.NotFound);

		var floor = dungeon.Floors.FirstOrDefault(f => f.Id == command.FloorId);
		if (floor is null)
			return Result<int>.Failure("Floor not found.", ErrorCategory.Validation);

		var area = floor.Areas.FirstOrDefault(a => a.Id == command.AreaId);
		if (area is null)
			return Result<int>.Failure("Area not found.", ErrorCategory.Validation);

		var tilesToFill = FloodFill(command.SeedPosition, area);

		foreach (var tile in tilesToFill)
			dungeon.SetTileTexture(command.FloorId, command.AreaId, tile, command.Kind);

		if (tilesToFill.Count > 0)
			await writeRepository.UpdateAsync(dungeon, ct);

		return Result<int>.Success(tilesToFill.Count);
	}

	private static List<TilePoint> FloodFill(
		TilePoint seed,
		Area area)
	{
		var origin = area.Origin;
		var verts = BuildPolygon(origin, area.Outline);

		var result = new List<TilePoint>();
		var visited = new HashSet<TilePoint>();
		var queue = new Queue<TilePoint>();

		var absSeed = new TilePoint(origin.Col + seed.Col, origin.Row + seed.Row);
		if (!IsTileInsidePolygon(absSeed.Col, absSeed.Row, verts))
			return result;

		queue.Enqueue(seed);
		visited.Add(seed);

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			result.Add(current);

			foreach (var neighbor in Neighbors(current))
			{
				if (visited.Contains(neighbor))
					continue;

				visited.Add(neighbor);

				var absNeighbor = new TilePoint(origin.Col + neighbor.Col, origin.Row + neighbor.Row);
				if (!IsTileInsidePolygon(absNeighbor.Col, absNeighbor.Row, verts))
					continue;

				queue.Enqueue(neighbor);
			}
		}

		return result;
	}

	private static IEnumerable<TilePoint> Neighbors(TilePoint p) =>
	[
		new TilePoint(p.Col - 1, p.Row),
		new TilePoint(p.Col + 1, p.Row),
		new TilePoint(p.Col, p.Row - 1),
		new TilePoint(p.Col, p.Row + 1),
	];

	private static List<(float X, float Y)> BuildPolygon(TilePoint origin, IReadOnlyList<PathSegment> outline)
	{
		var verts = new List<(float, float)>(outline.Count + 1);
		verts.Add((origin.Col, origin.Row));
		foreach (var seg in outline)
			verts.Add((seg.End.Col, seg.End.Row));
		if (verts.Count > 1)
			verts.RemoveAt(verts.Count - 1);
		return verts;
	}

	// A tile overlaps the area if any of its 3×3 interior sample points is inside the polygon.
	// Using only the center fails for diagonal-bisected tiles where the center is outside
	// but a meaningful portion of the tile is inside the area.
	private static bool IsTileInsidePolygon(int col, int row, List<(float X, float Y)> polygon)
	{
		float[] d = [0.25f, 0.5f, 0.75f];
		foreach (var dx in d)
			foreach (var dy in d)
				if (IsInsidePolygon(col + dx, row + dy, polygon))
					return true;
		return false;
	}

	private static bool IsInsidePolygon(float px, float py, List<(float X, float Y)> polygon)
	{
		var n = polygon.Count;
		var inside = false;
		for (int i = 0, j = n - 1; i < n; j = i++)
		{
			var xi = polygon[i].X; var yi = polygon[i].Y;
			var xj = polygon[j].X; var yj = polygon[j].Y;
			if (((yi > py) != (yj > py)) && px < (xj - xi) * (py - yi) / (yj - yi) + xi)
				inside = !inside;
		}
		return inside;
	}
}
