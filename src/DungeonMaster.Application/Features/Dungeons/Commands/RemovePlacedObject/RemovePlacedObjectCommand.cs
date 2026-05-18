using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemovePlacedObject;

/// <summary>
///		Represents a request to remove a placed object from the specified area within a dungeon floor.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor that contains the area.
/// </param>
/// <param name="AreaId">
///		The unique identifier of the area that owns the object.
/// </param>
/// <param name="ObjectId">
///		The unique identifier of the placed object to remove.
/// </param>
public record RemovePlacedObjectCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid AreaId,
	Guid ObjectId)
		: ICommand<bool>;
