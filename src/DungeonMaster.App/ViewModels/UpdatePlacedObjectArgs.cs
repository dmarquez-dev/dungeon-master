using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Arguments for moving, rotating, or rescaling a placed object. There is no dungeon
///		identifier: the editing context holds the one identifier that is correct.
/// </summary>
/// <remarks>
///		The two overloads this replaces differed only in whether a scale was supplied, so
///		<paramref name="Scale"/> defaults to <see cref="ObjectScale.Default"/> and the second
///		overload is deleted rather than reproduced.
/// </remarks>
/// <param name="FloorId">The unique identifier of the floor that owns the object.</param>
/// <param name="ObjectId">The unique identifier of the object to update.</param>
/// <param name="Position">The object's new position, in canvas coordinates.</param>
/// <param name="Rotation">The object's new rotation.</param>
/// <param name="Scale">The object's new scale; defaults to <see cref="ObjectScale.Default"/>.</param>
public sealed record UpdatePlacedObjectArgs(
	Guid FloorId,
	Guid ObjectId,
	CanvasPoint Position,
	ObjectRotation Rotation,
	float Scale = ObjectScale.Default);
