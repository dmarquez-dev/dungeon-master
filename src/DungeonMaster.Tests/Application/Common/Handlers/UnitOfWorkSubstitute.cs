using DungeonMaster.Application.Common.Interfaces;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Common.Handlers;

/// <summary>
///		The one place the substituted unit-of-work chain is wired.
///		A substituted <see cref="IUnitOfWorkFactory"/> hands back a substituted <see cref="IUnitOfWork"/> whose <see cref="IUnitOfWork.Dungeons"/> is a substituted <see cref="IDungeonWriteRepository"/>, so a handler test can assert against the repository and against the commit boundary without repeating the wiring.
/// </summary>
internal sealed class UnitOfWorkSubstitute
{
	/// <summary>
	///		Initializes the factory, the unit of work and the write repository, and connects them.
	/// </summary>
	public UnitOfWorkSubstitute()
	{
		Dungeons = Substitute.For<IDungeonWriteRepository>();

		UnitOfWork = Substitute.For<IUnitOfWork>();
		UnitOfWork.Dungeons.Returns(Dungeons);

		Factory = Substitute.For<IUnitOfWorkFactory>();
		Factory
			.CreateAsync(Arg.Any<CancellationToken>())
			.Returns(UnitOfWork);
	}

	/// <summary>
	///		Gets the substituted factory to hand to the system under test.
	/// </summary>
	public IUnitOfWorkFactory Factory { get; }

	/// <summary>
	///		Gets the substituted unit of work, used to assert whether the commit boundary was crossed.
	/// </summary>
	public IUnitOfWork UnitOfWork { get; }

	/// <summary>
	///		Gets the substituted write repository the unit of work exposes.
	/// </summary>
	public IDungeonWriteRepository Dungeons { get; }
}
