using DungeonMaster.App.Graphics;
using DungeonMaster.App.Tests.Hosting;
using DungeonMaster.App.Views;

namespace DungeonMaster.App.Tests.Views;

/// <summary>
///		Pins the page half of UIKIT-001 / MASTER-013: the page releases the map drawable's native
///		image handles when its handler is torn down.
/// </summary>
/// <remarks>
///		<para>
///		The evidence is the drawable's own <c>IsDisposed</c> after a real teardown of a real page
///		resolved from the running application's container - not an assertion over source text.
///		<c>IsDisposed</c> is frozen in contracts/floor-map-drawable-lifetime.md precisely so a
///		consumer that does not own the drawable can observe that teardown ran.
///		</para>
///		<para>
///		A host is required: <c>DungeonDetailPage</c> is a <c>ContentPage</c> and nothing deriving
///		from <c>Microsoft.Maui.Controls.Element</c> constructs without a started WinUI application
///		(spike-host-report.md section 3).
///		</para>
/// </remarks>
[Collection(MauiHostCollection.Name)]
public sealed class DungeonDetailPageTeardownTests(MauiHostFixture host)
{
	/// <summary>
	///		Disconnecting the page's handler disposes the drawable the page created, releasing every
	///		decoded object image it still holds.
	/// </summary>
	[Fact]
	public void DisconnectingThePageHandlerDisposesTheMapDrawable()
	{
		host.Invoke(() =>
		{
			var page = host.Resolve<DungeonDetailPage>();

			var canvas = page.FindByName<GraphicsView>("MapCanvas");

			Assert.NotNull(canvas);

			var drawable = Assert.IsType<FloorMapDrawable>(canvas.Drawable);

			Assert.False(drawable.IsDisposed);

			// A handler has to be connected before it can be disconnected: Element.Handler returns
			// early when the value it is given is the one it already holds, so assigning null to a
			// page that never had a handler raises no OnHandlerChanged at all.
			page.Handler = new StubElementHandler();
			page.Handler = null;

			Assert.True(drawable.IsDisposed);
		});
	}

	/// <summary>
	///		The teardown is a real handler disconnect, and nothing before it disposes the drawable:
	///		connecting a handler leaves the page fully usable.
	/// </summary>
	[Fact]
	public void ConnectingThePageHandlerLeavesTheMapDrawableUsable()
	{
		host.Invoke(() =>
		{
			var page = host.Resolve<DungeonDetailPage>();

			var drawable = Assert.IsType<FloorMapDrawable>(page.FindByName<GraphicsView>("MapCanvas")!.Drawable);

			page.Handler = new StubElementHandler();

			Assert.False(drawable.IsDisposed);

			page.Handler = null;
		});
	}

	/// <summary>
	///		The smallest handler <c>Element.Handler</c> will accept. It renders nothing: its only job
	///		is to be a non-null handler that can then be taken away, which is the teardown the page
	///		reacts to.
	/// </summary>
	private sealed class StubElementHandler : IViewHandler
	{
		public object? PlatformView => null;

		public object? ContainerView => null;

		public bool HasContainer { get; set; }

		public IView? VirtualView { get; private set; }

		public IMauiContext? MauiContext => null;

		IElement? IElementHandler.VirtualView => VirtualView;

		public void DisconnectHandler() => VirtualView = null;

		public Size GetDesiredSize(double widthConstraint, double heightConstraint) => Size.Zero;

		public void Invoke(string command, object? args)
		{
		}

		public void PlatformArrange(Rect frame)
		{
		}

		public void SetMauiContext(IMauiContext mauiContext)
		{
		}

		public void SetVirtualView(IElement view) => VirtualView = view as IView;

		public void UpdateValue(string property)
		{
		}
	}
}
