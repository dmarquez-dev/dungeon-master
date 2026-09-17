namespace DungeonMaster.Domain.Entities.Floors;

public enum ObjectKind
{
	/// <summary>
	///		No kind has been specified. This is never a legal kind for a <see cref="PlacedObject"/>,
	///		and the construction chokepoint rejects it.
	/// </summary>
	None,

	// ── Doors (wall-snap) ────────────────────────────────────────────────────
	DoorWood,
	DoorIron,
	DoorPortcullis,
	Arch,
	Window,

	// ── Furniture ────────────────────────────────────────────────────────────
	TableRound,
	TableRect,
	TableEnd,
	Desk,
	Chair,
	Armchair,
	Bench,
	Stool,
	Throne,
	Bed,
	StairsUp,
	StairsDown,

	// ── Storage ──────────────────────────────────────────────────────────────
	Chest,
	Barrel,
	Crate,
	Bookshelf,
	Dresser,
	Shelf,
	Vase,
	WickerBasket,

	// ── Lighting ─────────────────────────────────────────────────────────────
	Torch,
	Brazier,
	Lantern,
	Chandelier,
	Candle,

	// ── Decoration ───────────────────────────────────────────────────────────
	Fountain,
	Altar,
	Statue,
	Rug,
	Cauldron,
	Oven,
	WeaponRack,
}
