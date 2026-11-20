#if !WINDOWS && !MACCATALYST
namespace DungeonMaster.App.Services.Platform;

/// <content>
///		The implementing declaration of the canvas-cursor seam for target frameworks that carry no
///		platform — today the plain <c>net10.0</c> TFM, which exists so
///		<c>DungeonMaster.App.UnitTests</c> can reference this project without a WinUI host.
/// </content>
/// <remarks>
///		<para>
///		EVERY FUTURE SEAM UNDER <c>Platforms/</c> NEEDS A THIRD BODY IN THIS FILE. An extended
///		partial method with no implementing declaration is <c>CS8795</c>, so adding a
///		<c>Platforms/Windows</c> and a <c>Platforms/MacCatalyst</c> implementation and stopping
///		there breaks the plain TFM and, with it, the whole unit-test project. That tripwire is
///		deliberate — <see cref="PlatformCanvasCursor"/> documents it as the point of the pattern —
///		and this file is the permanent tax it charges. A future author adding a seam adds its
///		platform-less body here in the same change.
///		</para>
///		<para>
///		NO BODY IN THIS FILE MAY USE THE DISCARD TOKEN — the underscore-assignment form, deliberately
///		not spelled out here. Using it to consume an unused parameter makes
///		<c>DungeonMaster.Tests.App.AsyncDiscipline.DiscardedTasksAreAbsentTests.NoAppSourceLineStatesADiscardedTask</c>
///		fail: that hygiene test scans application source TEXT and bans the token outright. An empty
///		body with an explaining comment is the shape that passes. Measured, and found only by
///		running the suite.
///		</para>
///		<para>
///		No body here throws. A platform-less seam that threw would fail every future unit test that
///		happened to reach it, for a reason unrelated to what that test was about.
///		</para>
/// </remarks>
internal static partial class PlatformCanvasCursor
{
	/// <inheritdoc cref="SetShape(object?, CanvasCursorShape)"/>
	internal static partial void SetShape(object? platformView, CanvasCursorShape shape)
	{
		// Deliberately empty, and deliberately without a discard for the two parameters.
		// PlatformCanvasCursor.SetShape already documents that a view the platform does not
		// recognise "leaves the cursor alone". A target framework with no platform recognises
		// nothing, so doing nothing IS that documented behaviour rather than a stub standing in
		// for one.
	}
}

/// <content>
///		The implementing declaration of the secondary-button seam for target frameworks that carry
///		no platform. The two rules in <see cref="PlatformCanvasCursor"/>'s remarks in this file —
///		a third body per seam, and never the discard token — govern this type too.
/// </content>
internal static partial class PlatformPointer
{
	/// <inheritdoc cref="IsSecondaryButtonPressed()"/>
	internal static partial bool IsSecondaryButtonPressed() =>
		// PlatformPointer.IsSecondaryButtonPressed documents false as "the answer that leaves the
		// caller's behaviour unchanged" for a platform that cannot answer the question. A target
		// framework with no platform cannot answer it, so false is that documented answer and not
		// a guess.
		false;
}
#endif
