using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddStructureElement;

public record AddStructureElementCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid AreaId,
	VertexPoint? From,
	VertexPoint? To,
	StructureKind Kind,
	EdgePoint? EdgeAnchor = null)
		: ICommand<Guid>;
