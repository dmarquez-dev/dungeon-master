namespace DungeonMaster.Domain.Entities.Area;

public enum ObjectKind
{
    // Wall category (default snap: edge)
    DoorWood,
    DoorIron,
    DoorPortcullis,
    Arch,
    Window,

    // Floor category (default snap: center)
    TableRound,
    TableRect,
    Chair,
    Chest,
    Barrel,
    Crate,
    Torch,
    Brazier,
    Fountain,
    Altar,
    Throne,
    Bed,
    StairsUp,
    StairsDown
}
