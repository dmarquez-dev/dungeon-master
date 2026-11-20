namespace DungeonMaster.App.Controls;

/// <summary>
///		The entity a notebook has been opened on, together with the text it should display.
/// </summary>
/// <param name="Id">The identifier of the entity being authored.</param>
/// <param name="FloorId">
///		The owning floor identifier, or <see langword="null"/> for a dungeon-level notebook.
///		An area needs this to address the update command that saves it.
/// </param>
/// <param name="Kind">The kind of entity being authored.</param>
/// <param name="Name">The entity's display name.</param>
/// <param name="Description">The player-facing read-aloud text, or <see langword="null"/> when unset.</param>
/// <param name="Notes">The DM-only notes, or <see langword="null"/> when unset.</param>
public record NotebookTarget(
	Guid Id,
	Guid? FloorId,
	NotebookKind Kind,
	string Name,
	string? Description,
	string? Notes);
