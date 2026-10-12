namespace DungeonMaster.App.Services.Navigation;

/// <summary>
///		Holds the Shell route names and query-parameter keys used for navigation, so that a
///		route string is declared once and matched by every producer and consumer.
/// </summary>
internal static class ShellRoutes
{
	/// <summary>
	///		The Shell route registered for the dungeon detail page. Matched by exact string, so
	///		the value must not be re-cased.
	/// </summary>
	internal const string DungeonDetail = "dungeondetail";

	/// <summary>
	///		The query-parameter key carrying the dungeon identifier on the detail route. Matched
	///		by exact string, so the value must not be re-cased.
	/// </summary>
	internal const string IdQueryKey = "id";
}
