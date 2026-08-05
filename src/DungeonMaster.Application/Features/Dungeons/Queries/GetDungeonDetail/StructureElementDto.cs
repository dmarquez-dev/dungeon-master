using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

public record StructureElementDto(
	Guid Id,
	VertexPoint? From,
	VertexPoint? To,
	EdgePoint? EdgeAnchor,
	StructureKind Kind);
