using DungeonMaster.App.Assets.ObjectAssets;
using DungeonMaster.App.Behaviors;
using DungeonMaster.App.Theme;
using DungeonMaster.Domain.Entities.Floor;

namespace DungeonMaster.App.Views.DungeonDetail;

/// <summary>
///		The 48x48 object thumbnail rendered by both <see cref="ObjectPaletteView"/> and
///		<see cref="SecondaryPaletteView"/>. The two palettes show the same chip in two places, so
///		its frame, its image-or-initial content, its two text alternatives and its hover treatment
///		live here rather than being restated per view, where they had already drifted apart twice
///		— once in the hover colours, and once in a one-unit image inset.
/// </summary>
internal static class PaletteThumb
{
	// Properties and methods, not static readonly fields: see DmTheme's remarks and the guard
	// test NoEagerDmThemeRead.
	private static Color SurfaceNormal =>
		DmTheme.SurfaceToolChip;

	private static Color SurfaceHover =>
		DmTheme.SurfaceToolChipHover;

	// A brush per pointer event rather than one shared instance: a SolidColorBrush handed to
	// several views at once is a MAUI anti-pattern, and no brush token exists for either colour.
	private static SolidColorBrush StrokeNormal() =>
		new(DmTheme.BorderToolChip);

	private static SolidColorBrush StrokeHover() =>
		new(DmTheme.Gold);

	/// <summary>
	///		Adds the hand cursor and the pointer-enter/exit surface and stroke swap to a
	///		thumbnail chip.
	/// </summary>
	/// <param name="border">
	///		The chip to attach the hover treatment to.
	/// </param>
	internal static void AttachHover(Border border)
	{
		border.Behaviors.Add(new HandCursorBehavior());
		var ptr = new PointerGestureRecognizer();
		ptr.PointerEntered += (_, _) =>
		{
			border.BackgroundColor = SurfaceHover;
			border.Stroke = StrokeHover();
		};
		ptr.PointerExited += (_, _) =>
		{
			border.BackgroundColor = SurfaceNormal;
			border.Stroke = StrokeNormal();
		};
		border.GestureRecognizers.Add(ptr);
	}

	/// <summary>
	///		The inset between a thumbnail's image and the chip edge that frames it.
	/// </summary>
	/// <remarks>
	///		The two palettes used to disagree here by one device-independent unit — 2 in
	///		<see cref="ObjectPaletteView"/>, 3 in <see cref="SecondaryPaletteView"/> — which is the
	///		same drift, on the same chip, that moved the hover colours into this class. 2 wins:
	///		Tokens.xaml's spacing scale has a step at 2 (DmSpaceXXS) and none at 3, and its
	///		snapping table sends 3 to 4, which is a visibly wider inset on a 48x48 chip. It stays a
	///		literal rather than a token read because DmTheme deliberately exposes only the five
	///		geometry accessors code-behind actually asks for, and adding a sixth is not this
	///		file's call.
	/// </remarks>
	private const double ImageInset = 2;

	/// <summary>
	///		Builds the empty 48x48 chip both palettes frame their content with: the surface, the
	///		hairline stroke and the tight corner radius, and nothing that depends on what is
	///		inside it.
	/// </summary>
	/// <param name="content">
	///		The view to place inside the chip.
	/// </param>
	/// <remarks>
	///		Separate from <see cref="Build"/> because the primary palette's category expander is
	///		this same chip with an ellipsis in it, a different tap target and a semantic hint of
	///		its own. It shares the frame, which is what drifted; it does not share the object
	///		chip's tooltip, description or selection callback, none of which mean anything for it.
	/// </remarks>
	internal static Border CreateShell(View content)
	{
		return new Border
		{
			WidthRequest = 48,
			HeightRequest = 48,
			Padding = 0,
			BackgroundColor = SurfaceNormal,
			Stroke = StrokeNormal(),
			StrokeThickness = 1,
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
			{
				CornerRadius = DmTheme.RadiusTight
			},
			Content = content
		};
	}

	/// <summary>
	///		Builds the object thumbnail chip: the shared frame, the object's image or its initial,
	///		both text alternatives, the tap target and the hover treatment.
	/// </summary>
	/// <param name="kind">
	///		The object the chip stands for.
	/// </param>
	/// <param name="onSelected">
	///		Invoked with <paramref name="kind"/> when the chip is tapped. The two palettes
	///		legitimately differ here and only here — the primary palette selects the object
	///		directly, the secondary one raises an event for its owner — so the callback is a
	///		parameter rather than something this factory decides.
	/// </param>
	internal static Border Build(ObjectKind kind, Action<ObjectKind> onSelected)
	{
		var displayName = ObjectKindInfo.GetDisplayName(kind);
		var border = CreateShell(BuildContent(kind, displayName));

		// The tooltip is a hover affordance and reaches nobody who is not holding a mouse; the
		// chip's only other content is an image, so the description is its one text alternative.
		ToolTipProperties.SetText(border, displayName);
		SemanticProperties.SetDescription(border, displayName);

		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) =>
			onSelected(kind);
		border.GestureRecognizers.Add(tap);

		AttachHover(border);

		return border;
	}

	/// <summary>
	///		The object's artwork when the loaded pack has some, and its initial otherwise.
	/// </summary>
	private static View BuildContent(ObjectKind kind, string displayName)
	{
		if (ObjectAssetCatalogue.Paths.TryGetValue(kind, out var path))
		{
			return new Image
			{
				Source = path,
				Aspect = Aspect.AspectFit,
				Margin = ImageInset
			};
		}

		return new Label
		{
			Text = displayName.Length > 0 ? displayName[..1] : "?",
			FontSize = DmTheme.FontSizeMeta,
			TextColor = DmTheme.TextParchment,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}
