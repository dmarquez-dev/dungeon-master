namespace DungeonMaster.App.UnitTests.Fixtures;

/// <summary>
///		The name of the xunit collection that shares one <see cref="AppFixture"/>.
/// </summary>
/// <remarks>
///		Every test class in this assembly that constructs or touches a
///		<c>Microsoft.Maui.Controls</c> type carries <c>[Collection(AppTestCollection.Name)]</c>.
///		<c>Application.Current</c> and <c>DispatcherProvider.Current</c> are process-global mutable
///		statics; two classes racing to set them is a family of hazard whose failure mode is an
///		intermittent green, which is the most expensive kind. The collection makes those classes
///		sequential.
/// </remarks>
public static class AppTestCollection
{
	/// <summary>
	///		The collection name, used by both <c>[CollectionDefinition]</c> and every
	///		<c>[Collection]</c> attribute in this assembly.
	/// </summary>
	public const string Name = "app-unit-tests";
}

/// <summary>
///		Declares <see cref="AppFixture"/> as the collection fixture for
///		<see cref="AppTestCollection.Name"/>.
/// </summary>
[CollectionDefinition(AppTestCollection.Name)]
public sealed class AppTestCollectionDefinition : ICollectionFixture<AppFixture>;
