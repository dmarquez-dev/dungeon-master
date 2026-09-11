using DungeonMaster.Application.Common.Interfaces;

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
