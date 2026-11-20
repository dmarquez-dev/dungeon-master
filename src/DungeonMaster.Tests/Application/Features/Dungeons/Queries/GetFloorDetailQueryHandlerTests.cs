using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Application.Features.Dungeons.Queries.GetFloorDetail;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Queries;

public class GetFloorDetailQueryHandlerTests
{
	private readonly IDungeonReadRepository _readRepository;
	private readonly GetFloorDetailQueryHandler _handler;

	public GetFloorDetailQueryHandlerTests()
	{
		_readRepository = Substitute.For<IDungeonReadRepository>();
		_handler = new GetFloorDetailQueryHandler(_readRepository);
	}

	/// <summary>
	///		Verifies that the floor projection the read repository returns is handed back as the very same instance.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WhenTheRepositoryReturnsAFloor_ReturnsThatSameInstance()
	{
		// Arrange
		var dungeonId = Guid.NewGuid();
		var floorId = Guid.NewGuid();
		var dto = new FloorDto(floorId, 2, "Undercroft", null, null, [], []);
		_readRepository
			.GetFloorDetailAsync(dungeonId, floorId, Arg.Any<CancellationToken>())
			.Returns(dto);

		var query = new GetFloorDetailQuery(dungeonId, floorId);

		// Act
		var result = await _handler.HandleAsync(query, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(dto);
	}

	/// <summary>
	///		Verifies that a <see langword="null"/> from the read repository, the not-found value, is returned as <see langword="null"/>.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WhenTheRepositoryReturnsNull_ReturnsNull()
	{
		// Arrange
		_readRepository
			.GetFloorDetailAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((FloorDto?)null);

		var query = new GetFloorDetailQuery(Guid.NewGuid(), Guid.NewGuid());

		// Act
		var result = await _handler.HandleAsync(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	/// <summary>
	///		Verifies that the read repository is asked for the floor with the query's dungeon and floor identifiers and the exact token handed to the handler.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithACancellationToken_PassesThatSameTokenAndBothIdentifiersToGetFloorDetailAsync()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var dungeonId = Guid.NewGuid();
		var floorId = Guid.NewGuid();

		_readRepository
			.GetFloorDetailAsync(
				Arg.Any<Guid>(),
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>())
			.Returns((FloorDto?)null);

		// Act
		await _handler.HandleAsync(
			new GetFloorDetailQuery(dungeonId, floorId),
			token);

		// Assert
		await _readRepository
			.Received(1)
			.GetFloorDetailAsync(
				dungeonId,
				floorId,
				token);
	}
}
