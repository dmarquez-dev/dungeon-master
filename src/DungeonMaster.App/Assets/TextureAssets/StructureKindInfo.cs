using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.App.Assets.TextureAssets;

public static class StructureKindInfo
{
	public static string GetCategory(StructureKind kind) => kind switch
	{
		StructureKind.RoundColumn or StructureKind.SquareColumn => "Point",
		StructureKind.OpenArchway or StructureKind.WoodDoor
			or StructureKind.IronDoor or StructureKind.Portcullis => "Door",
		_ => "Edge",
	};

	public static bool IsEdgeCenterStructure(StructureKind kind) => kind switch
	{
		StructureKind.OpenArchway or StructureKind.WoodDoor
			or StructureKind.IronDoor or StructureKind.Portcullis => true,
		_ => false,
	};

	public static string GetDisplayName(StructureKind kind) => kind switch
	{
		StructureKind.RoundColumn  => "Round Column",
		StructureKind.SquareColumn => "Square Column",
		StructureKind.WallStub     => "Wall Stub",
		StructureKind.IronBars     => "Iron Bars",
		StructureKind.OpenArchway  => "Open Archway",
		StructureKind.WoodDoor     => "Wood Door",
		StructureKind.IronDoor     => "Iron Door",
		StructureKind.Portcullis   => "Portcullis",
		_                          => kind.ToString(),
	};

	/// <summary>
	///     For vertex-pair point structures: render size as a fraction of tile size (square).
	///     For edge-center structures: render thickness (the thin dimension; span is always TileSize).
	/// </summary>
	public static float GetRenderSize(StructureKind kind) => kind switch
	{
		StructureKind.RoundColumn or StructureKind.SquareColumn               => 0.4f,
		StructureKind.WallStub or StructureKind.IronBars                      => 0.25f,
		StructureKind.OpenArchway or StructureKind.WoodDoor
			or StructureKind.IronDoor or StructureKind.Portcullis             => 0.2f,
		_                                                                     => 0.4f,
	};

	/// <summary>
	///     CW rotation in degrees to apply to an edge-center structure image based on which edge it sits on.
	///     Asset convention: 0° = door faces south (opening downward).
	///     N=0°, E=90°, S=180°, W=270°.
	/// </summary>
	public static float GetEdgeSideRotation(EdgeSide side) => side switch
	{
		EdgeSide.N => 0f,
		EdgeSide.E => 90f,
		EdgeSide.S => 180f,
		EdgeSide.W => 270f,
		_          => 0f,
	};
}
