using DungeonMaster.Domain.Entities.Area;

namespace DungeonMaster.App.Assets.TextureAssets;

public static class TextureKindInfo
{
	public static string GetCategory(TextureKind kind) => kind switch
	{
		TextureKind.StoneFloor or TextureKind.WoodPlanks
			or TextureKind.DirtFloor or TextureKind.CobblestoneFloor => "Floor",
		_ => "Wall",
	};

	public static string GetDisplayName(TextureKind kind) => kind switch
	{
		TextureKind.StoneFloor       => "Stone Floor",
		TextureKind.WoodPlanks       => "Wood Planks",
		TextureKind.DirtFloor        => "Dirt Floor",
		TextureKind.CobblestoneFloor => "Cobblestone",
		TextureKind.StoneBrick       => "Stone Brick",
		TextureKind.CaveWall         => "Cave Wall",
		TextureKind.WoodWall         => "Wood Wall",
		TextureKind.BrickWall        => "Brick Wall",
		_                            => kind.ToString(),
	};
}
