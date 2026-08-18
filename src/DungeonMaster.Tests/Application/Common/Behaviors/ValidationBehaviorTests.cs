using DungeonMaster.Application.Common.Behaviors;
using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Common.Behaviors;

/// <summary>
///		Unit tests for the value-producing arity of <see cref="ValidationBehavior{TCommand, TResult}"/>.
/// </summary>
public class ValidationBehaviorTests
{
	private readonly TestCommand _command = new("Goblin Warren");

	/// <summary>
	///		Verifies that a command with no registered validators reaches the next stage of the pipeline.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithNoValidators_CallsNext()
	{
		// Arrange
		var nextValue = Guid.NewGuid();
		var nextCalled = false;

		var behavior = new ValidationBehavior<TestCommand, Guid>([]);

		// Act
		var result = await behavior.HandleAsync(
			_command,
			() =>
			{
				nextCalled = true;

				return Task.FromResult(Result<Guid>.Success(nextValue));
			},
			CancellationToken.None);

		// Assert
		nextCalled.Should().BeTrue();
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be(nextValue);
	}

	/// <summary>
	///		Verifies that a command whose validators all pass reaches the next stage of the pipeline.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithPassingValidator_CallsNext()
	{
		// Arrange
		var nextValue = Guid.NewGuid();
		var nextCalled = false;

		var behavior = new ValidationBehavior<TestCommand, Guid>([ValidatorReturning()]);

		// Act
		var result = await behavior.HandleAsync(
			_command,
			() =>
			{
				nextCalled = true;

				return Task.FromResult(Result<Guid>.Success(nextValue));
			},
			CancellationToken.None);

		// Assert
		nextCalled.Should().BeTrue();
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be(nextValue);
	}

	/// <summary>
	///		Verifies that a single validation failure short-circuits the pipeline and is reported as a validation failure.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithOneFailure_ReturnsValidationFailureAndDoesNotCallNext()
	{
		// Arrange
		var nextCalled = false;

		var validator = ValidatorReturning(
			new ValidationFailure(
				"Name",
				"Name is required."));

		var behavior = new ValidationBehavior<TestCommand, Guid>([validator]);

		// Act
		var result = await behavior.HandleAsync(
			_command,
			() =>
			{
				nextCalled = true;

				return Task.FromResult(Result<Guid>.Success(Guid.NewGuid()));
			},
			CancellationToken.None);

		// Assert
		nextCalled.Should().BeFalse();
		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.Validation);
		result.ValidationErrors.Should().HaveCount(1);
		result.Error.Should().Contain("Name is required.");
	}

	/// <summary>
	///		Verifies that every validation failure survives rather than only the first one.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithThreeFailures_CarriesAllThreeValidationErrors()
	{
		// Arrange
		var validator = ValidatorReturning(
			new ValidationFailure(
				"Name",
				"Name is required."),
			new ValidationFailure(
				"Name",
				"Name must be shorter than 64 characters."),
			new ValidationFailure(
				"DungeonId",
				"DungeonId must not be empty."));

		var behavior = new ValidationBehavior<TestCommand, Guid>([validator]);

		// Act
		var result = await behavior.HandleAsync(
			_command,
			() =>
				Task.FromResult(Result<Guid>.Success(Guid.NewGuid())),
			CancellationToken.None);

		// Assert
		result.ValidationErrors.Should().HaveCount(3);
		result.Error.Should().Contain("Name is required.");
		result.Error.Should().Contain("Name must be shorter than 64 characters.");
		result.Error.Should().Contain("DungeonId must not be empty.");
	}

	/// <summary>
	///		Verifies that failures reported by two separate validators are aggregated into one result.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithTwoValidators_CarriesFailuresFromBoth()
	{
		// Arrange
		var first = ValidatorReturning(
			new ValidationFailure(
				"Name",
				"Name is required."));

		var second = ValidatorReturning(
			new ValidationFailure(
				"DungeonId",
				"DungeonId must not be empty."));

		var behavior = new ValidationBehavior<TestCommand, Guid>([first, second]);

		// Act
		var result = await behavior.HandleAsync(
			_command,
			() =>
				Task.FromResult(Result<Guid>.Success(Guid.NewGuid())),
			CancellationToken.None);

		// Assert
		result.ValidationErrors.Should().HaveCount(2);
		result.Error.Should().Contain("Name is required.");
		result.Error.Should().Contain("DungeonId must not be empty.");
	}

	/// <summary>
	///		Verifies that the property name reported by each validation failure survives into the result.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithFailures_PreservesPropertyName()
	{
		// Arrange
		var validator = ValidatorReturning(
			new ValidationFailure(
				"Name",
				"Name is required."),
			new ValidationFailure(
				"DungeonId",
				"DungeonId must not be empty."));

		var behavior = new ValidationBehavior<TestCommand, Guid>([validator]);

		// Act
		var result = await behavior.HandleAsync(
			_command,
			() =>
				Task.FromResult(Result<Guid>.Success(Guid.NewGuid())),
			CancellationToken.None);

		// Assert
		result.ValidationErrors.Should().SatisfyRespectively(
			first =>
			{
				first.PropertyName.Should().Be("Name");
				first.Message.Should().Be("Name is required.");
			},
			second =>
			{
				second.PropertyName.Should().Be("DungeonId");
				second.Message.Should().Be("DungeonId must not be empty.");
			});
	}

	/// <summary>
	///		Verifies that the cancellation token the behavior was called with is handed to every validator.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WhenCalled_PassesTheTokenToEveryValidator()
	{
		// Arrange
		using var source = new CancellationTokenSource();

		var token = source.Token;
		var firstReceived = CancellationToken.None;
		var secondReceived = CancellationToken.None;

		var first = Substitute.For<IValidator<TestCommand>>();

		first
			.ValidateAsync(
				Arg.Any<ValidationContext<TestCommand>>(),
				Arg.Do<CancellationToken>(received =>
					firstReceived = received))
			.Returns(Task.FromResult(new ValidationResult()));

		var second = Substitute.For<IValidator<TestCommand>>();

		second
			.ValidateAsync(
				Arg.Any<ValidationContext<TestCommand>>(),
				Arg.Do<CancellationToken>(received =>
					secondReceived = received))
			.Returns(Task.FromResult(new ValidationResult()));

		var behavior = new ValidationBehavior<TestCommand, Guid>([first, second]);

		// Act
		await behavior.HandleAsync(
			_command,
			() =>
				Task.FromResult(Result<Guid>.Success(Guid.NewGuid())),
			token);

		// Assert
		firstReceived.Should().Be(token);
		secondReceived.Should().Be(token);
	}

	/// <summary>
	///		Verifies that a validator carrying an asynchronous rule runs without throwing.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithAsyncRule_DoesNotThrow()
	{
		// Arrange
		var behavior = new ValidationBehavior<TestCommand, Guid>([new AsyncRuleValidator()]);

		// Act
		var act = async () =>
			await behavior.HandleAsync(
				_command,
				() =>
					Task.FromResult(Result<Guid>.Success(Guid.NewGuid())),
				CancellationToken.None);

		// Assert
		await act.Should().NotThrowAsync();
	}

	private static IValidator<TestCommand> ValidatorReturning(
		params ValidationFailure[] failures)
	{
		var validator = Substitute.For<IValidator<TestCommand>>();

		validator
			.ValidateAsync(
				Arg.Any<ValidationContext<TestCommand>>(),
				Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(new ValidationResult(failures)));

		return validator;
	}

	/// <summary>
	///		A command defined solely for these tests, so that the production command set can change without breaking them.
	/// </summary>
	/// <param name="Name">
	///		The name carried by the command.
	/// </param>
	public sealed record TestCommand(string Name)
		: ICommand<Guid>;

	/// <summary>
	///		A real validator carrying an asynchronous rule, used to prove the behavior never invokes a validator synchronously.
	/// </summary>
	public sealed class AsyncRuleValidator
		: AbstractValidator<TestCommand>
	{
		/// <summary>
		///		Initializes a new instance of the <see cref="AsyncRuleValidator"/> class.
		/// </summary>
		public AsyncRuleValidator()
		{
			RuleFor(command =>
				command.Name)
				.MustAsync(async (name, token) =>
				{
					await Task.Yield();

					return !string.IsNullOrWhiteSpace(name);
				})
				.WithMessage("Name must not be empty.");
		}
	}
}
