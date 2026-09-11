using DungeonMaster.Application.Common.Interfaces;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Common.Handlers;

/// <summary>
///		The one place a command handler test class is wired to a substituted unit-of-work chain.
///		Every command handler test class under <c>Application/Features/Dungeons/Commands/</c> derives from this, directly or through <see cref="DungeonNotFoundHandlerTestBase{THandler}"/>, and supplies only the one line that differs between them: the handler construction.
/// </summary>
/// <typeparam name="THandler">
///		The command handler under test.
/// </typeparam>
public abstract class CommandHandlerTestBase<THandler>
	where THandler : class
{
	private readonly UnitOfWorkSubstitute _substitute;

	private THandler? _handler;

	/// <summary>
	///		Initializes the substituted unit-of-work chain. xUnit constructs one instance per test, so no test observes another's substitute.
	/// </summary>
	protected CommandHandlerTestBase()
	{
		_substitute = new UnitOfWorkSubstitute();
	}

	/// <summary>
	///		Gets the substituted write repository the unit of work exposes, used to arrange loads and to assert against the repository boundary.
	/// </summary>
	protected IDungeonWriteRepository WriteRepository =>
		_substitute.Dungeons;

	/// <summary>
	///		Gets the substituted unit of work, used to assert whether the commit boundary was crossed.
	/// </summary>
	protected IUnitOfWork UnitOfWork =>
		_substitute.UnitOfWork;

	/// <summary>
	///		Gets the handler under test, built once per test instance from the substituted factory.
	/// </summary>
	protected THandler Handler =>
		_handler ??= CreateHandler(_substitute.Factory);

	/// <summary>
	///		Gets the substituted unit-of-work factory the handler was built over, used to assert against the factory boundary.
	/// </summary>
	protected IUnitOfWorkFactory UnitOfWorkFactory =>
		_substitute.Factory;

	/// <summary>
	///		Dispatches this leaf's command with the supplied token, so a base-level test can assert which token crossed each port.
	///		The command's arguments are irrelevant to that question; only the token this method threads through is.
	/// </summary>
	/// <param name="ct">
	///		The token to hand to the handler.
	/// </param>
	/// <returns>
	///		A task that completes when the handler returns. The result is discarded.
	/// </returns>
	protected abstract Task InvokeWithTokenAsync(CancellationToken ct);

	/// <summary>
	///		Verifies that the exact token handed to the handler is the token the handler opens its unit of work with.
	///		Declared on the base, so it runs once for every command handler test class that derives from it.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithACancellationToken_PassesThatSameTokenToUnitOfWorkFactoryCreateAsync()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		// Act
		await InvokeWithTokenAsync(token);

		// Assert
		await UnitOfWorkFactory
			.Received(1)
			.CreateAsync(token);
	}

	/// <summary>
	///		Creates the handler under test.
	/// </summary>
	/// <param name="factory">
	///		The substituted unit-of-work factory to hand to the handler's constructor.
	/// </param>
	/// <returns>
	///		The handler this leaf exercises.
	/// </returns>
	protected abstract THandler CreateHandler(IUnitOfWorkFactory factory);
}
