using DungeonMaster.App.Behaviors;
using DungeonMaster.Domain.Entities.Floors;

namespace DungeonMaster.App.Views.DungeonDetail;

public partial class ControlsPaletteView : ContentView
{
	public event EventHandler<RotationDirection>? RotateRequested;
	public event EventHandler<ScaleDirection>? ScaleRequested;

	public ControlsPaletteView()
	{
		InitializeComponent();
		AttachButtonBehavior(BtnCCW, () => RotateRequested?.Invoke(this, RotationDirection.CounterClockwise));
		AttachButtonBehavior(BtnCW, () => RotateRequested?.Invoke(this, RotationDirection.Clockwise));
		AttachButtonBehavior(BtnScaleDown, () => ScaleRequested?.Invoke(this, ScaleDirection.Down));
		AttachButtonBehavior(BtnScaleUp, () => ScaleRequested?.Invoke(this, ScaleDirection.Up));
	}

	public void UpdateScale(float scale) => ScaleReadout.Text = $"{scale:0.##}×";
	public void ResetScale() => ScaleReadout.Text = "1.0×";

	private static void AttachButtonBehavior(Border btn, Action action)
	{
		btn.Behaviors.Add(new HandCursorBehavior());

		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => action();
		btn.GestureRecognizers.Add(tap);
	}
}
