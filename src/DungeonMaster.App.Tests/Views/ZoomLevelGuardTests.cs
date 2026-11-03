using DungeonMaster.App.Views.DungeonDetail;

namespace DungeonMaster.App.Tests.Views;

/// <summary>
///		Executes <see cref="ZoomLevelGuard"/> directly. The guard derives from nothing and touches no MAUI type, so it runs without an application host, as <see cref="KeyChordTests"/> already does.
/// </summary>
/// <remarks>
///		<see cref="CurrentLevel"/> is neither 1.0, <see cref="ZoomLevelGuard.MinimumZoom"/> nor <see cref="ZoomLevelGuard.MaximumZoom"/>, so a non-finite request that slipped past the finiteness check and reached the snap or the clamp could not coincide with it.
/// </remarks>
public class ZoomLevelGuardTests
{
	private const float CurrentLevel = 1.5f;

	/// <summary>
	///		A NaN request leaves the level unchanged. Unguarded, NaN fails the snap comparison and passes through the clamp as NaN.
	/// </summary>
	[Fact]
	public void Next_WithNaN_ReturnsTheCurrentLevel()
	{
		var next = ZoomLevelGuard.Next(CurrentLevel, float.NaN);

		Assert.Equal(CurrentLevel, next);
	}

	/// <summary>
	///		A positive-infinity request leaves the level unchanged. Unguarded, it clamps to the maximum.
	/// </summary>
	[Fact]
	public void Next_WithPositiveInfinity_ReturnsTheCurrentLevel()
	{
		var next = ZoomLevelGuard.Next(CurrentLevel, float.PositiveInfinity);

		Assert.Equal(CurrentLevel, next);
	}

	/// <summary>
	///		A negative-infinity request leaves the level unchanged. Unguarded, it clamps to the minimum.
	/// </summary>
	[Fact]
	public void Next_WithNegativeInfinity_ReturnsTheCurrentLevel()
	{
		var next = ZoomLevelGuard.Next(CurrentLevel, float.NegativeInfinity);

		Assert.Equal(CurrentLevel, next);
	}

	/// <summary>
	///		Control: a finite in-range request outside the snap band is returned unchanged.
	/// </summary>
	[Fact]
	public void Next_WithAFiniteInRangeLevelOutsideTheSnapBand_ReturnsThatLevel()
	{
		var next = ZoomLevelGuard.Next(CurrentLevel, 2.0f);

		Assert.Equal(2.0f, next);
	}

	/// <summary>
	///		Control: a finite request within the snap threshold of 100% snaps to exactly 1.0.
	/// </summary>
	[Fact]
	public void Next_WithAFiniteLevelInsideTheSnapBand_ReturnsOne()
	{
		var next = ZoomLevelGuard.Next(CurrentLevel, 1.03f);

		Assert.Equal(1.0f, next);
	}

	/// <summary>
	///		Control: a finite request above the maximum clamps to the maximum.
	/// </summary>
	[Fact]
	public void Next_WithAFiniteLevelAboveTheMaximum_ReturnsTheMaximum()
	{
		var next = ZoomLevelGuard.Next(CurrentLevel, 5.0f);

		Assert.Equal(4.0f, next);
	}

	/// <summary>
	///		Control: a finite request below the minimum clamps to the minimum.
	/// </summary>
	[Fact]
	public void Next_WithAFiniteLevelBelowTheMinimum_ReturnsTheMinimum()
	{
		var next = ZoomLevelGuard.Next(CurrentLevel, 0.1f);

		Assert.Equal(0.25f, next);
	}
}
