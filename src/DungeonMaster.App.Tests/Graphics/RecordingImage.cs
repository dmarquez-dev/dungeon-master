using Microsoft.Maui.Graphics;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace DungeonMaster.App.Tests.Graphics;

/// <summary>
///		An <see cref="IImage"/> that does nothing but count the times it was disposed.
/// </summary>
/// <remarks>
///		A real <c>PlatformImage</c> cannot answer the question these facts ask. It exposes no
///		"was I disposed" signal, and it cannot even be decoded outside a MAUI host — Win2D throws a
///		bare <c>COMException</c>. Handing this in through the drawable's image-factory seam is what
///		makes the cached-handle lifetime (UIKIT-001 / MASTER-013, MASTER-160) observable at all.
/// </remarks>
internal sealed class RecordingImage : IImage
{
	/// <summary>
	///		The label this image was created under, so a fact can say WHICH image was disposed
	///		rather than only how many were.
	/// </summary>
	public required string Label { get; init; }

	/// <summary>
	///		How many times <see cref="Dispose"/> has been called. A fact asserts 1 for a released
	///		handle and 0 for a stranded one; more than 1 would be a double release.
	/// </summary>
	public int DisposeCount { get; private set; }

	/// <summary>
	///		Whether <see cref="Dispose"/> should throw. Set on one image to prove that a failing
	///		native release does not strand the remaining handles.
	/// </summary>
	public bool ThrowOnDispose { get; set; }

	/// <inheritdoc/>
	public float Width => 10f;

	/// <inheritdoc/>
	public float Height => 10f;

	/// <inheritdoc/>
	public void Dispose()
	{
		DisposeCount++;

		if (ThrowOnDispose)
			throw new InvalidOperationException($"native release of {Label} failed");
	}

	/// <inheritdoc/>
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
	}

	/// <inheritdoc/>
	public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false) => this;

	/// <inheritdoc/>
	public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false) => this;

	/// <inheritdoc/>
	public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false) =>
		this;

	/// <inheritdoc/>
	public IImage ToPlatformImage() => this;

	/// <inheritdoc/>
	public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1) =>
		throw new NotSupportedException();

	/// <inheritdoc/>
	public Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1) =>
		throw new NotSupportedException();
}
