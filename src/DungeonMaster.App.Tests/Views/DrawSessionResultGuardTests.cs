using DungeonMaster.App.Core.Map;
using DungeonMaster.App.Views.DungeonDetail;

namespace DungeonMaster.App.Tests.Views;

/// <summary>
///		Executes <see cref="DrawSessionResultGuard"/> directly. The guard derives from nothing and
///		touches no MAUI type, so it runs without an application host, as
///		<see cref="Converters.NotNullConverterTests"/> already does (MASTER-179).
/// </summary>
/// <remarks>
///		<c>ParamName</c> is deliberately not asserted anywhere below. <c>CA2208</c> is an error in
///		this solution, so a wrong parameter name fails the build rather than reddening a test, and
///		asserting it would look like cover it is not. The <c>ActualValue</c> and <c>Message</c>
///		halves are what these tests actually protect.
/// </remarks>
public class DrawSessionResultGuardTests
{
	/// <summary>
	///		<see cref="DrawSessionResult.None"/> is a declared member that
	///		<see cref="MapDrawingSession.PlaceVertex"/> never returns. Its dedicated arm must reject
	///		it with its own wording, so deleting that arm and letting the catch-all absorb the value
	///		reddens this test.
	/// </summary>
	[Fact]
	public void EnsurePlacementOutcome_WithNone_ThrowsNamingTheNoPlacementAttemptCase()
	{
		var exception = Assert.Throws<ArgumentOutOfRangeException>(
			() => DrawSessionResult.None.EnsurePlacementOutcome());

		Assert.Equal(DrawSessionResult.None, exception.ActualValue);
		Assert.Contains("No placement attempt was made.", exception.Message);
	}

	/// <summary>
	///		A value outside the declared members is rejected with the catch-all's own wording,
	///		distinct from the <see cref="DrawSessionResult.None"/> message.
	/// </summary>
	[Fact]
	public void EnsurePlacementOutcome_WithAValueOutsideTheDeclaredMembers_ThrowsNamingTheUnrecognisedCase()
	{
		var exception = Assert.Throws<ArgumentOutOfRangeException>(
			() => ((DrawSessionResult)999).EnsurePlacementOutcome());

		Assert.Equal((DrawSessionResult)999, exception.ActualValue);
		Assert.Contains("Unrecognised draw session result.", exception.Message);
	}

	/// <summary>
	///		Positive control: every value <see cref="MapDrawingSession.PlaceVertex"/> does return
	///		passes through unchanged and throws nothing.
	/// </summary>
	[Theory]
	[InlineData(DrawSessionResult.Added)]
	[InlineData(DrawSessionResult.Closed)]
	[InlineData(DrawSessionResult.Ignored)]
	public void EnsurePlacementOutcome_WithAPlacementOutcome_ReturnsItUnchanged(
		DrawSessionResult result)
	{
		var returned = result.EnsurePlacementOutcome();

		Assert.Equal(result, returned);
	}
}
