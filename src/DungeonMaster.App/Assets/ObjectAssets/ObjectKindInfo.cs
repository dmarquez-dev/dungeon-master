using DungeonMaster.Domain.Entities.Floors;

namespace DungeonMaster.App.Assets.ObjectAssets;

public static class ObjectKindInfo
{
	private static readonly HashSet<ObjectKind> WallObjects =
	[
		ObjectKind.DoorWood,
		ObjectKind.DoorIron,
		ObjectKind.DoorPortcullis,
		ObjectKind.Arch,
		ObjectKind.Window,
	];

	private static readonly HashSet<ObjectKind> WallAdjacentObjects =
	[
		ObjectKind.Bookshelf,
		ObjectKind.Dresser,
		ObjectKind.Shelf,
		ObjectKind.WeaponRack,
	];

	public static SnapAnchor GetDefaultAnchor(ObjectKind kind)
	{
		if (WallObjects.Contains(kind))
		{
			return SnapAnchor.East;
		}
		if (WallAdjacentObjects.Contains(kind))
		{
			return SnapAnchor.South;
		}
		return SnapAnchor.Center;
	}

	/// <summary>
	///		Logical bounding size of the object in tile units (W × H).
	///		Used to scale assets proportionally at appropriate dungeon-map scale.
	/// </summary>
	public static (float W, float H) GetObjectSize(ObjectKind kind) => kind switch
	{
		// Tiny decorative / lighting
		ObjectKind.Candle or ObjectKind.Torch or ObjectKind.Lantern => (0.35f, 0.35f),
		ObjectKind.Vase => (0.35f, 0.50f),
		ObjectKind.WickerBasket or ObjectKind.Brazier => (0.50f, 0.50f),

		// Small storage
		ObjectKind.Chest or ObjectKind.Barrel or ObjectKind.Crate => (0.65f, 0.65f),

		// Small seating
		ObjectKind.Stool => (0.55f, 0.55f),
		ObjectKind.Chair or ObjectKind.Armchair => (0.75f, 0.75f),

		// Medium / single-tile
		ObjectKind.Throne or ObjectKind.Cauldron or ObjectKind.Oven => (0.90f, 0.90f),
		ObjectKind.Chandelier => (0.80f, 0.80f),
		ObjectKind.Altar or ObjectKind.Statue => (0.90f, 0.90f),
		ObjectKind.Dresser or ObjectKind.TableRound => (1.00f, 1.00f),
		ObjectKind.Rug => (1.80f, 1.20f),

		// Wall-adjacent storage (tall/wide)
		ObjectKind.Bookshelf or ObjectKind.WeaponRack => (0.45f, 1.10f),
		ObjectKind.Shelf => (1.10f, 0.45f),

		// Large furniture spanning multiple tiles
		ObjectKind.Bed => (1.00f, 2.00f),
		ObjectKind.TableRect => (2.00f, 1.00f),
		ObjectKind.Desk => (1.80f, 0.90f),
		ObjectKind.Bench => (0.45f, 1.50f),
		ObjectKind.TableEnd => (0.70f, 0.70f),

		_ => (1.00f, 1.00f),
	};

	public static string GetDisplayName(ObjectKind kind) => kind switch
	{
		ObjectKind.DoorWood => "Wood Door",
		ObjectKind.DoorIron => "Iron Door",
		ObjectKind.DoorPortcullis => "Portcullis",
		ObjectKind.Arch => "Arch",
		ObjectKind.Window => "Window",
		ObjectKind.TableRound => "Round Table",
		ObjectKind.TableRect => "Rect Table",
		ObjectKind.TableEnd => "Side Table",
		ObjectKind.Desk => "Writing Desk",
		ObjectKind.Chair => "Chair",
		ObjectKind.Armchair => "Armchair",
		ObjectKind.Bench => "Bench",
		ObjectKind.Stool => "Stool",
		ObjectKind.Throne => "Throne",
		ObjectKind.Bed => "Bed",
		ObjectKind.StairsUp => "Stairs Up",
		ObjectKind.StairsDown => "Stairs Down",
		ObjectKind.Chest => "Chest",
		ObjectKind.Barrel => "Barrel",
		ObjectKind.Crate => "Crate",
		ObjectKind.Bookshelf => "Bookshelf",
		ObjectKind.Dresser => "Dresser",
		ObjectKind.Shelf => "Shelf",
		ObjectKind.Vase => "Vase",
		ObjectKind.WickerBasket => "Basket",
		ObjectKind.Torch => "Torch",
		ObjectKind.Brazier => "Brazier",
		ObjectKind.Lantern => "Lantern",
		ObjectKind.Chandelier => "Chandelier",
		ObjectKind.Candle => "Candle",
		ObjectKind.Fountain => "Fountain",
		ObjectKind.Altar => "Altar",
		ObjectKind.Statue => "Statue",
		ObjectKind.Rug => "Rug",
		ObjectKind.Cauldron => "Cauldron",
		ObjectKind.Oven => "Oven",
		ObjectKind.WeaponRack => "Weapon Rack",
		_ => kind.ToString()
	};

	public static string GetCategory(ObjectKind kind)
	{
		if (WallObjects.Contains(kind))
		{
			return "Doors";
		}
		return kind switch
		{
			ObjectKind.TableRound or ObjectKind.TableRect or ObjectKind.TableEnd
				or ObjectKind.Desk or ObjectKind.Chair or ObjectKind.Armchair
				or ObjectKind.Bench or ObjectKind.Stool or ObjectKind.Throne
				or ObjectKind.Bed or ObjectKind.StairsUp or ObjectKind.StairsDown => "Furniture",

			ObjectKind.Chest or ObjectKind.Barrel or ObjectKind.Crate
				or ObjectKind.Bookshelf or ObjectKind.Dresser or ObjectKind.Shelf
				or ObjectKind.Vase or ObjectKind.WickerBasket => "Storage",

			ObjectKind.Torch or ObjectKind.Brazier or ObjectKind.Lantern
				or ObjectKind.Chandelier or ObjectKind.Candle => "Lighting",

			_ => "Decoration"
		};
	}
}
