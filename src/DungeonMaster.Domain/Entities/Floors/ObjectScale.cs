namespace DungeonMaster.Domain.Entities.Floors;

public static class ObjectScale
{
	public const float Min = 0.25f;
	public const float Max = 4.0f;
	public const float Step = 0.25f;
	public const float Default = 1.0f;

	public static float StepUp(float current) => MathF.Min(current + Step, Max);
	public static float StepDown(float current) => MathF.Max(current - Step, Min);
}
