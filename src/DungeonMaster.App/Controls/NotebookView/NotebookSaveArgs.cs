namespace DungeonMaster.App.Controls;

/// <summary>
///		The payload of a notebook autosave, describing which entity to update and with what text.
/// </summary>
/// <param name="Id">The identifier of the entity to update.</param>
/// <param name="FloorId">
///		The owning floor identifier, or <see langword="null"/> for a dungeon-level notebook.
/// </param>
/// <param name="Kind">The kind of entity to update.</param>
/// <param name="Name">The entity's name as edited.</param>
/// <param name="Description">The read-aloud text, or <see langword="null"/> when blank.</param>
/// <param name="Notes">The DM-only notes, or <see langword="null"/> when blank.</param>
public record NotebookSaveArgs(
	Guid Id,
	Guid? FloorId,
	NotebookKind Kind,
	string Name,
	string? Description,
	string? Notes);
