using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Text;

namespace DungeonMaster.App.Tests.Graphics;

/// <summary>
///		A no-op <see cref="ICanvas"/> that records only what the map facts assert about, and that
///		can be told to record nothing at all.
/// </summary>
/// <remarks>
///		It is hand-written rather than substituted because
///		<c>FloorMapDrawableAllocationTests</c> measures the bytes a frame allocates: a mocking
///		framework records every call it receives, and those allocations would swamp the drawable's
///		own by orders of magnitude. With <see cref="Recording"/> false this type allocates nothing
///		per call, so the measured bytes are the drawable's alone.
/// </remarks>
internal sealed class RecordingCanvas : ICanvas
{
	private Color _strokeColor = Colors.Transparent;

	/// <summary>
	///		Whether to record. False makes every member a true no-op, for allocation measurement.
	/// </summary>
	public bool Recording { get; set; } = true;

	/// <summary>
	///		The stroke colour in force at each <see cref="DrawPath"/> call, in call order. The map
	///		draws one path per area and one per ghost outline, so this is the per-area alpha the
	///		frame actually painted.
	/// </summary>
	public List<Color> StrokeColorAtDrawPath { get; } = [];

	/// <summary>
	///		The points of each path handed to <see cref="DrawPath"/>, in call order.
	/// </summary>
	public List<PointF[]> DrawnPaths { get; } = [];

	/// <summary>
	///		Every string handed to <see cref="DrawString(string, float, float, float, float, HorizontalAlignment, VerticalAlignment, TextFlow, float)"/> or <see cref="DrawString(string, float, float, HorizontalAlignment)"/>, in call order.
	///		Kept as the instances passed in, so a fact can assert two frames reused one string rather than formatting two.
	/// </summary>
	public List<string> DrawnStrings { get; } = [];

	/// <inheritdoc/>
	public float DisplayScale { get; set; } = 1f;

	/// <inheritdoc/>
	public Color StrokeColor
	{
		set => _strokeColor = value;
	}

	/// <inheritdoc/>
	public Color FillColor { set { } }

	/// <inheritdoc/>
	public Color FontColor { set { } }

	/// <inheritdoc/>
	public IFont Font { set { } }

	/// <inheritdoc/>
	public float FontSize { set { } }

	/// <inheritdoc/>
	public float Alpha { set { } }

	/// <inheritdoc/>
	public bool Antialias { set { } }

	/// <inheritdoc/>
	public BlendMode BlendMode { set { } }

	/// <inheritdoc/>
	public float MiterLimit { set { } }

	/// <inheritdoc/>
	public LineCap StrokeLineCap { set { } }

	/// <inheritdoc/>
	public LineJoin StrokeLineJoin { set { } }

	/// <inheritdoc/>
	public float StrokeSize { set { } }

	/// <inheritdoc/>
	public float StrokeDashOffset { set { } }

	/// <inheritdoc/>
	public float[] StrokeDashPattern { set { } }

	/// <inheritdoc/>
	public void DrawPath(PathF path)
	{
		if (!Recording)
			return;

		StrokeColorAtDrawPath.Add(_strokeColor);
		DrawnPaths.Add([.. path.Points]);
	}

	/// <inheritdoc/>
	public void DrawString(
		string value,
		float x,
		float y,
		float width,
		float height,
		HorizontalAlignment horizontalAlignment,
		VerticalAlignment verticalAlignment,
		TextFlow textFlow = TextFlow.ClipBounds,
		float lineSpacingAdjustment = 0)
	{
		if (Recording)
			DrawnStrings.Add(value);
	}

	/// <inheritdoc/>
	public void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment)
	{
		if (Recording)
			DrawnStrings.Add(value);
	}

	/// <inheritdoc/>
	public void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero) { }

	/// <inheritdoc/>
	public void ClipRectangle(float x, float y, float width, float height) { }

	/// <inheritdoc/>
	public void ConcatenateTransform(System.Numerics.Matrix3x2 transform) { }

	/// <inheritdoc/>
	public void DrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed) { }

	/// <inheritdoc/>
	public void DrawEllipse(float x, float y, float width, float height) { }

	/// <inheritdoc/>
	public void DrawImage(Microsoft.Maui.Graphics.IImage image, float x, float y, float width, float height) { }

	/// <inheritdoc/>
	public void DrawLine(float x1, float y1, float x2, float y2) { }

	/// <inheritdoc/>
	public void DrawRectangle(float x, float y, float width, float height) { }

	/// <inheritdoc/>
	public void DrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius) { }

	/// <inheritdoc/>
	public void DrawText(IAttributedText value, float x, float y, float width, float height) { }

	/// <inheritdoc/>
	public void FillArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise) { }

	/// <inheritdoc/>
	public void FillEllipse(float x, float y, float width, float height) { }

	/// <inheritdoc/>
	public void FillPath(PathF path, WindingMode windingMode) { }

	/// <inheritdoc/>
	public void FillRectangle(float x, float y, float width, float height) { }

	/// <inheritdoc/>
	public void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius) { }

	/// <inheritdoc/>
	public SizeF GetStringSize(string value, IFont font, float fontSize) => SizeF.Zero;

	/// <inheritdoc/>
	public SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment) => SizeF.Zero;

	/// <inheritdoc/>
	public void ResetState() { }

	/// <inheritdoc/>
	public bool RestoreState() => true;

	/// <inheritdoc/>
	public void Rotate(float degrees) { }

	/// <inheritdoc/>
	public void Rotate(float degrees, float x, float y) { }

	/// <inheritdoc/>
	public void SaveState() { }

	/// <inheritdoc/>
	public void Scale(float sx, float sy) { }

	/// <inheritdoc/>
	public void SetFillPaint(Paint paint, RectF rectangle) { }

	/// <inheritdoc/>
	public void SetShadow(SizeF offset, float blur, Color color) { }

	/// <inheritdoc/>
	public void SubtractFromClip(float x, float y, float width, float height) { }

	/// <inheritdoc/>
	public void Translate(float tx, float ty) { }
}
