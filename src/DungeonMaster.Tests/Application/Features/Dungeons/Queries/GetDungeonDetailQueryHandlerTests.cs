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
		var dto = new DungeonDetailDto(id, "The Vault", null, null, 0, DateTime.UtcNow, DateTime.UtcNow, []);
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
}
