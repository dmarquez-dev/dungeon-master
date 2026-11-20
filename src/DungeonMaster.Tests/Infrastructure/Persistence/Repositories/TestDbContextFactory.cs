using DungeonMaster.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

/// <summary>
///		Wraps the <see cref="DbContextOptions{TContext}"/> the existing in-memory SQLite fixture already builds, so a test can hand a real <c>IDbContextFactory</c> to production code without inventing a second fixture.
///		Only <see cref="CreateDbContext"/> is implemented: <c>IDbContextFactory&lt;TContext&gt;.CreateDbContextAsync</c> is a virtual interface member with a default implementation.
/// </summary>
internal sealed class TestDbContextFactory(
	DbContextOptions<AppDbContext> options)
		: IDbContextFactory<AppDbContext>
{
	/// <summary>
	///		Creates a context over the shared options, and therefore over the one open connection the fixture holds.
	/// </summary>
	/// <returns>
	///		A new <see cref="AppDbContext"/>.
	/// </returns>
	public AppDbContext CreateDbContext() =>
		new(options);
}
