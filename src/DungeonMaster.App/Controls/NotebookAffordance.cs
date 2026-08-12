using DungeonMaster.App.Theme;

namespace DungeonMaster.App.Controls;

/// <summary>
///		Paints a sidebar row's notebook button to show whether the entity behind it has authored
///		content, and writes the matching screen-reader description.
/// </summary>
/// <remarks>
///		<para>
///		This existed as two character-identical copies, one in <see cref="AreaSidebarItem"/> and
///		one in <see cref="FloorSidebarItem"/>. The duplication mattered more than its five lines
///		suggest: the gold/muted swap is the ONLY sighted cue that a floor or an area has been
///		written up, and a colour is not a text alternative, so the description has to change with
///		it or the affordance exists for sighted users alone. Held by copy-paste, that contract
///		survives exactly until someone edits one copy — and the two copies are in different files,
///		so nothing would have reported the drift.
///		</para>
///		<para>
///		It is one method rather than a base class on purpose. The two controls share this one
///		rule and nothing else — different DTOs, different layouts, different gestures — and a
///		shared base would couple everything else about them in order to share five lines.
///		</para>
/// </remarks>
internal static class NotebookAffordance
{
	/// <summary>
	///		Applies the has-content state to a sidebar row's notebook button.
	/// </summary>
	/// <param name="notebookButton">
	///		The notebook button to paint and describe.
	/// </param>
	/// <param name="hasContent">
	///		Whether the entity has a description or notes authored against it.
	/// </param>
	/// <remarks>
	///		Both halves are applied here rather than in XAML for the same reason: they depend on
	///		the data, and the XAML values are only the unpopulated defaults.
	/// </remarks>
	public static void Apply(Button notebookButton, bool hasContent)
	{
		ArgumentNullException.ThrowIfNull(notebookButton);

		notebookButton.TextColor = hasContent
			? DmTheme.ScrollGoldLight
			: DmTheme.InkMuted;

		SemanticProperties.SetDescription(
			notebookButton,
			hasContent
				? "Notebook, has content"
				: "Notebook, empty");
	}
}
