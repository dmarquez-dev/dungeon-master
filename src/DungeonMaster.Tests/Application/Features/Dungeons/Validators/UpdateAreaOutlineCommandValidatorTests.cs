using DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Tests for <see cref="UpdateAreaOutlineCommandValidator"/>.
/// </summary>
public class UpdateAreaOutlineCommandValidatorTests
{
	private readonly UpdateAreaOutlineCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = new UpdateAreaOutlineCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Outline());

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithEmptyDungeonId_ReturnsViolation()
	{
		// Arrange
		var command = new UpdateAreaOutlineCommand(
			Guid.Empty,
			Guid.NewGuid(),
			Guid.NewGuid(),
			Outline());

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdateAreaOutlineCommand.DungeonId));
	}

	[Fact]
	public void Validate_WithEmptyFloorId_ReturnsViolation()
	{
		// Arrange
		var command = new UpdateAreaOutlineCommand(
			Guid.NewGuid(),
			Guid.Empty,
			Guid.NewGuid(),
			Outline());

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdateAreaOutlineCommand.FloorId));
	}

	[Fact]
	public void Validate_WithEmptyAreaId_ReturnsViolation()
	{
		// Arrange
		var command = new UpdateAreaOutlineCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.Empty,
			Outline());

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdateAreaOutlineCommand.AreaId));
	}

	[Fact]
	public void Validate_WithEmptyOutline_ReturnsViolation()
	{
		// Arrange
		var command = new UpdateAreaOutlineCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			[]);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdateAreaOutlineCommand.Outline));
	}

	[Fact]
	public void Validate_WithNullOutline_ReturnsViolation()
	{
		// Arrange
		var command = new UpdateAreaOutlineCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			null!);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(error =>
			error.PropertyName == nameof(UpdateAreaOutlineCommand.Outline));
	}

	private static IReadOnlyList<PathSegment> Outline() =>
		[
			new PathSegment(
				PathSegmentKind.Line,
				new TilePoint(0, 0),
				null,
				null),
			new PathSegment(
				PathSegmentKind.Line,
				new TilePoint(1, 0),
				null,
				null),
			new PathSegment(
				PathSegmentKind.Line,
				new TilePoint(1, 1),
				null,
				null)
		];
}
