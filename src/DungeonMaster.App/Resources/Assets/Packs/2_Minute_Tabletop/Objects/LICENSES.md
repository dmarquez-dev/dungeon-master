# Asset Licenses

## 2-Minute Tabletop — Furniture Map Assets
- Source: https://2minutetabletop.com/product/furniture-map-assets/
- License: Creative Commons Attribution-NonCommercial 4.0 International (CC BY-NC 4.0)
- Attribution: Ross McConnell / 2-Minute Tabletop (2minutetabletop.com)
- Attribution display: "Map assets by 2-Minute Tabletop — 2minutetabletop.com"
- Notes: Non-commercial use only. Assets are hand-drawn top-down dungeon furniture.
  Place downloaded PNGs into the category subfolders below and update
  ObjectAssetCatalogue.cs with the corresponding paths.

## 2-Minute Tabletop — Dungeon Wall & Floor Assets (reserved — texture phase)
- Source: https://2minutetabletop.com/dungeon-wall-floor-assets/
- License: Creative Commons Attribution-NonCommercial 4.0 International (CC BY-NC 4.0)
- Attribution: Ross McConnell / 2-Minute Tabletop (2minutetabletop.com)
- Notes: Walls, floors, doors, and dungeon essentials. Reserved for the floor/wall
  texture system. Door and prop PNGs from this pack may also be placed here once
  that system is implemented.

## 2-Minute Tabletop — Dungeon Map Tiles (reserved — texture phase)
- Source: https://2minutetabletop.com/dungeon-builder-map-tiles/
- License: Creative Commons Attribution-NonCommercial 4.0 International (CC BY-NC 4.0)
- Attribution: Ross McConnell / 2-Minute Tabletop (2minutetabletop.com)
- Notes: 131 modular floor/wall/stair/pillar tiles. Reserved for the floor/wall
  texture system.

## Directory Structure

Each PNG maps to an ObjectKind enum member via ObjectAssetCatalogue.cs:

```
objects/
  doors/          DoorWood, DoorIron, DoorPortcullis, Arch, Window
  furniture/      TableRound, TableRect, Chair, Throne, Bed, StairsUp, StairsDown
  storage/        Chest, Barrel, Crate
  lighting/       Torch, Brazier
  decoration/     Fountain, Altar
```

New ObjectKind entries added to cover 2MT objects beyond the original 19 should
follow the same folder/naming convention and be documented here.
