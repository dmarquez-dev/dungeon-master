namespace DungeonMaster.App.Behaviors;

public sealed class CanvasLongPressDetector : IDisposable
{
	private const int LongPressMs = 450;

	private System.Timers.Timer? _timer;
	private bool _active;

	public event EventHandler? LongPressed;

	public void TouchStarted()
	{
		_active = true;
		_timer?.Stop();
		_timer?.Dispose();
		_timer = new System.Timers.Timer(LongPressMs) { AutoReset = false };
		_timer.Elapsed += (_, _) =>
		{
			if (_active)
				MainThread.BeginInvokeOnMainThread(() =>
					LongPressed?.Invoke(this, EventArgs.Empty));
		};
		_timer.Start();
	}

	public void TouchEnded()
	{
		_active = false;
		_timer?.Stop();
		_timer?.Dispose();
		_timer = null;
	}

	public void Dispose()
	{
		_active = false;
		_timer?.Stop();
		_timer?.Dispose();
		_timer = null;
	}
}
