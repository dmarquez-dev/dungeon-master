namespace DungeonMaster.Domain.Entities.Floors;

/// <summary>
///		Defines the permitted scale range for a placed object and the steps between its values.
/// </summary>
public static class ObjectScale
{
	/// <summary>
	///		The smallest scale a placed object may take.
	/// </summary>
	public const float Min = 0.25f;

	/// <summary>
	///		The largest scale a placed object may take.
	/// </summary>
	public const float Max = 4.0f;

	/// <summary>
	///		The amount by which a scale changes in a single step.
	/// </summary>
	public const float Step = 0.25f;

	/// <summary>
	///		The scale a placed object takes when none is specified.
	/// </summary>
	public const float Default = 1.0f;

	/// <summary>
	///		Returns the next scale above the supplied one, clamped to <see cref="Max"/>.
	/// </summary>
	/// <param name="current">
	///		The scale to step up from.
	/// </param>
	/// <returns>
	///		The stepped-up scale, never greater than <see cref="Max"/>.
	/// </returns>
	public static float StepUp(float current) => MathF.Min(current + Step, Max);

	/// <summary>
	///		Returns the next scale below the supplied one, clamped to <see cref="Min"/>.
	/// </summary>
	/// <param name="current">
	///		The scale to step down from.
	/// </param>
	/// <returns>
	///		The stepped-down scale, never less than <see cref="Min"/>.
	/// </returns>
	public static float StepDown(float current) => MathF.Max(current - Step, Min);
}
