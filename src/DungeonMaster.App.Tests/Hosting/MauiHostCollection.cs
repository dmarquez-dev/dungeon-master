namespace DungeonMaster.App.Tests.Hosting;

/// <summary>
///		The one xunit collection every host-requiring test class joins. Membership is what
///		serialises those classes against the single dispatcher.
/// </summary>
[CollectionDefinition(MauiHostCollection.Name)]
public sealed class MauiHostCollection : ICollectionFixture<MauiHostFixture>
{
	/// <summary>The collection name to pass to <c>[Collection(...)]</c>.</summary>
	public const string Name = "maui-host";
}
