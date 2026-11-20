using DungeonMaster.App.Behaviors;
using DungeonMaster.Domain.Entities.Floors;

namespace DungeonMaster.App.Views.DungeonDetail;

/// <summary>
///		The controls palette, whose rotate and scale buttons ask the host to transform an object, and which shows the current scale factor as a readout.
/// </summary>
public partial class ControlsPaletteView : ContentView
{
	/// <summary>
	///		Raised when one of the two rotate buttons is tapped, carrying the direction to rotate in.
	/// </summary>
	public event EventHandler<RotationDirection>? RotateRequested;

	/// <summary>
	///		Raised when one of the two scale buttons is tapped, carrying the direction to scale in.
	/// </summary>
	public event EventHandler<ScaleDirection>? ScaleRequested;

	/// <summary>
	///		Initialises a new instance of <see cref="ControlsPaletteView"/>, wiring each of the four buttons to the event and direction it raises.
	/// </summary>
	public ControlsPaletteView()
	{
		InitializeComponent();
		AttachButtonBehavior(RotateCounterClockwiseButton, () => RotateRequested?.Invoke(this, RotationDirection.CounterClockwise));
		AttachButtonBehavior(RotateClockwiseButton, () => RotateRequested?.Invoke(this, RotationDirection.Clockwise));
		AttachButtonBehavior(BtnScaleDown, () => ScaleRequested?.Invoke(this, ScaleDirection.Down));
		AttachButtonBehavior(BtnScaleUp, () => ScaleRequested?.Invoke(this, ScaleDirection.Up));
	}

	/// <summary>
	///		Sets the scale readout to the given factor, shown to at most two decimal places and suffixed with a multiplication sign.
	/// </summary>
	/// <param name="scale">
	///		The scale factor to display.
	/// </param>
	public void UpdateScale(float scale) => ScaleReadout.Text = $"{scale:0.##}×";

	/// <summary>
	///		Sets the scale readout back to the unscaled factor of one.
	/// </summary>
	public void ResetScale() => ScaleReadout.Text = "1.0×";

	private static void AttachButtonBehavior(Border btn, Action action)
	{
		btn.Behaviors.Add(new HandCursorBehavior());

		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => action();
		btn.GestureRecognizers.Add(tap);
	}
}
