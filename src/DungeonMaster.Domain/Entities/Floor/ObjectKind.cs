namespace DungeonMaster.Domain.Entities.Floor;

public enum ObjectKind
{
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
