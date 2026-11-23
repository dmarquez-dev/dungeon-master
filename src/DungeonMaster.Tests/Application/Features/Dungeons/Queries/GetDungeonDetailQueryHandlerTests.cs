using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Queries;

public class GetDungeonDetailQueryHandlerTests
{
	private readonly IDungeonReadRepository _readRepository;
	private readonly GetDungeonDetailQueryHandler _handler;

	public GetDungeonDetailQueryHandlerTests()
	{
		_readRepository = Substitute.For<IDungeonReadRepository>();
		_handler = new GetDungeonDetailQueryHandler(_readRepository);
	}

	[Fact]
	public async Task HandleAsync_WithExistingId_ReturnsDtoFromRepository()
	{
		// Arrange
		var id = Guid.NewGuid();
		var firstFloor = new FloorDto(Guid.NewGuid(), 1, "Upper Hall", null, null, [], []);
		var secondFloor = new FloorDto(Guid.NewGuid(), 2, "Lower Crypt", null, null, [], []);
		var floors = new List<FloorDto> { firstFloor, secondFloor };
		var dto = new DungeonDetailDto(id, "The Vault", null, null, 2, DateTime.UtcNow, DateTime.UtcNow, floors);
		_readRepository
			.GetDetailAsync(id, Arg.Any<CancellationToken>())
			.Returns(dto);

		var query = new GetDungeonDetailQuery(id);

		// Act
		var result = await _handler.HandleAsync(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeSameAs(dto);
		result!.Id.Should().Be(id);
		result.Name.Should().Be("The Vault");
		result.Floors.Should().SatisfyRespectively(
			floor => floor.Should().BeSameAs(firstFloor),
			floor => floor.Should().BeSameAs(secondFloor));
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentId_ReturnsNull()
	{
		// Arrange
		_readRepository
			.GetDetailAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((DungeonDetailDto?)null);

		var query = new GetDungeonDetailQuery(Guid.NewGuid());

		// Act
		var result = await _handler.HandleAsync(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}
	/// <summary>
	///		Verifies that the exact token handed to the handler is the token the read repository is asked for the detail projection with.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithACancellationToken_PassesThatSameTokenToGetDetailAsync()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var id = Guid.NewGuid();

		_readRepository
			.GetDetailAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>())
			.Returns((DungeonDetailDto?)null);

		// Act
		await _handler.HandleAsync(
			new GetDungeonDetailQuery(id),
			token);

		// Assert
		await _readRepository
			.Received(1)
			.GetDetailAsync(
				id,
				token);
	}

}
