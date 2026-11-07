using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DungeonMaster.App.Core.Editor;
using DungeonMaster.App.Services.Navigation;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Application.Features.Dungeons.Queries.GetFloorDetail;
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Composition root for the dungeon detail page.
/// </summary>
/// <remarks>
///		This type owns four facts and nothing else: which dungeon is loaded, how to refresh one
///		floor of it or reload all of it, where a failure is shown, and the editor's selection
///		state (<see cref="Editor"/>). The editing surface lives on the three children it
///		constructs — <see cref="Header"/>, <see cref="FloorList"/> and <see cref="Canvas"/> — and
///		they reach back through <see cref="IDungeonEditingContext"/>, which is the seam that makes
///		the refresh policy two members instead of a line copied into every command.
/// </remarks>
public partial class DungeonDetailViewModel : ObservableObject, IQueryAttributable, IDungeonEditingContext
{
	private static readonly Result UnusableNavigationIdResult = Result.Failure(
		"This dungeon could not be opened because the navigation request carried no usable dungeon identifier.",
		ErrorCategory.NotFound);

	private static readonly Result DungeonNotFoundResult = Result.Failure(
		"This dungeon could not be found. It may have been deleted. Return to the dungeon list and open a dungeon.",
		ErrorCategory.NotFound);

	private static readonly Result NoDungeonLoadedResult = Result.Failure(
		"No dungeon is loaded, so there is nothing to save. Return to the dungeon list and open a dungeon.",
		ErrorCategory.NotFound);

	private readonly AppDispatcher _dispatcher;

	private readonly INavigationService _navigation;

	private Guid _dungeonId;

	/// <summary>
	///		Initialises the composition root and constructs its three children.
	/// </summary>
	/// <param name="dispatcher">
	///		The application dispatcher, handed to each child so every command in the page's
	///		editing surface goes through one pipeline.
	/// </param>
	/// <param name="navigation">
	///		Route navigation behind an interface, so this type can be exercised without a live
	///		navigation host and never reads a static ambient accessor.
	/// </param>
	public DungeonDetailViewModel(AppDispatcher dispatcher, INavigationService navigation)
	{
		ArgumentNullException.ThrowIfNull(dispatcher);
		ArgumentNullException.ThrowIfNull(navigation);

		_dispatcher = dispatcher;
		_navigation = navigation;

		// The children are CONSTRUCTED here, not resolved from the container: they exist only in
		// service of this page and have no independent lifetime, so registering them would put
		// three types in the container that nothing else can legitimately ask for.
		Header = new DungeonHeaderViewModel(dispatcher, this);
		FloorList = new FloorListViewModel(dispatcher, this);
		Canvas = new MapCanvasViewModel(dispatcher, this);

		Editor = new MapEditorState();
	}

	/// <summary>
	///		Gets the editor's interaction state, including the observable selection facts the
	///		sidebar rows bind to: which floor is selected and which area is focused.
	/// </summary>
	/// <remarks>
	///		This is the one instance of the page's lifetime. It is constructed here, is never null
	///		and is never replaced, and loading, reloading or refreshing does not reset its
	///		selection; clearing the selection stays the page's responsibility.
	/// </remarks>
	public MapEditorState Editor { get; }

	/// <summary>
	///		Gets or sets the full detail DTO for the currently loaded dungeon.
	/// </summary>
	[ObservableProperty]
	public partial DungeonDetailDto? Dungeon { get; set; }

	/// <summary>
	///		Gets or sets the message describing the most recent failed operation, or
	///		<see langword="null"/> when the most recent operation succeeded.
	/// </summary>
	[ObservableProperty]
	public partial string? ErrorMessage { get; set; }

	/// <summary>
	///		Gets the dungeon metadata editor: name, description, notes, and save.
	/// </summary>
	public DungeonHeaderViewModel Header { get; }

	/// <summary>
	///		Gets the floor lifecycle and the floor collection the page's sidebar renders.
	/// </summary>
	public FloorListViewModel FloorList { get; }

	/// <summary>
	///		Gets the canvas content: active tool, drawing session, and the area and placed-object commands.
	/// </summary>
	public MapCanvasViewModel Canvas { get; }

	/// <inheritdoc/>
	public Guid DungeonId =>
		_dungeonId;

	/// <summary>
	///		Gets the task representing the load started by the most recent call to <see cref="ApplyQueryAttributes"/>.
	///		The page awaits this rather than letting the started load be discarded, so a load that faults is observed instead of being captured on a <see cref="Task"/> nobody holds.
	///		This is <see cref="Task.CompletedTask"/> before any navigation has been applied and when the navigation carried no usable identifier.
	/// </summary>
	public Task LoadTask { get; private set; } = Task.CompletedTask;

	/// <summary>
	///		Applies query-string attributes from route navigation to load the correct dungeon.
	/// </summary>
	/// <param name="query">
	///		The query attributes provided by route navigation.
	///		An <c>id</c> key holding a valid <see cref="Guid"/> string starts a load.
	///		When that identifier names no dungeon, the load clears the identifier, reports the not-found failure on <see cref="ErrorMessage"/>, and leaves every editing command refusing, as <see cref="LoadDungeonAsync"/> describes.
	///		A missing, empty, or unparsable <c>id</c> is not silently ignored: the view model clears any dungeon it was showing, leaves <see cref="LoadTask"/> completed, and reports the failure on <see cref="ErrorMessage"/>.
	///		Every editing command is refused while that state holds, so no write is aimed at a dungeon that does not exist.
	/// </param>
	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		ClearError();

		if (!query.TryGetValue(ShellRoutes.IdQueryKey, out var val) || !Guid.TryParse(val?.ToString(), out var id))
		{
			_dungeonId = Guid.Empty;

			Repopulate(null);

			LoadTask = Task.CompletedTask;
			ErrorMessage = ResultMessage.ForFailure(UnusableNavigationIdResult);

			return;
		}

		LoadTask = LoadDungeonCommand.ExecuteAsync(id);
	}

	/// <inheritdoc/>
	public bool NoDungeonIsLoaded()
	{
		if (_dungeonId != Guid.Empty)
		{
			return false;
		}

		ErrorMessage = ResultMessage.ForFailure(NoDungeonLoadedResult);

		return true;
	}

	/// <inheritdoc/>
	public void ReportFailure(Result result)
	{
		ArgumentNullException.ThrowIfNull(result);

		ErrorMessage = ResultMessage.ForFailure(result);
	}

	/// <inheritdoc/>
	public void ClearError()
	{
		ErrorMessage = null;
	}

	/// <inheritdoc/>
	public Task ReloadAsync() =>
		LoadDungeonAsync(_dungeonId);

	/// <inheritdoc/>
	/// <remarks>
	///		Every fallback condition is judged after the floor-scoped read completes, against the
	///		state as it stands then, so a load that landed while the read was in flight is never
	///		patched with an index taken before it. The floor list is replaced first and the
	///		projection second, the same order <see cref="Repopulate"/> keeps, so a handler reacting
	///		to <see cref="Dungeon"/> changing reads a floor list that already carries the new floor.
	/// </remarks>
	public async Task RefreshFloorAsync(Guid floorId)
	{
		var floor = await _dispatcher.QueryAsync<FloorDto?>(
			new GetFloorDetailQuery(_dungeonId, floorId),
			CancellationToken.None);

		var dungeon = Dungeon;
		var index = FloorList.IndexOf(floorId);

		// Any of these means the floor cannot be replaced in place: the floor or the dungeon was
		// removed between the write and the read, the floor list does not show the floor, or no
		// projection is loaded to keep consistent with it. The full reload is the correct answer
		// to each, and it carries the not-found handling LoadDungeonAsync already owns.
		if (floor is null
			|| dungeon is null
			|| index < 0
			|| index >= dungeon.Floors.Count
			|| dungeon.Floors[index].Id != floorId)
		{
			await ReloadAsync();

			return;
		}

		FloorList.ReplaceAt(
			index,
			floor);

		var floors = dungeon.Floors.ToArray();
		floors[index] = floor;

		Dungeon = dungeon with
		{
			Floors = floors,
		};
	}

	/// <summary>
	///		Loads the dungeon detail for the specified <paramref name="id"/> and repopulates the
	///		three children.
	/// </summary>
	/// <remarks>
	///		When no dungeon has that identifier, the load does not fall through to an empty page.
	///		The loaded identifier is cleared to <see cref="Guid.Empty"/>, the children are repopulated empty, and the not-found failure is reported on <see cref="ErrorMessage"/>.
	///		Every editing command then refuses through <see cref="NoDungeonIsLoaded"/>, so no write is aimed at the missing dungeon.
	///		The same outcome follows when <see cref="ReloadAsync"/> finds that the loaded dungeon was deleted between commands.
	/// </remarks>
	/// <param name="id">
	///		The unique identifier of the dungeon to load.
	/// </param>
	/// <returns>
	///		A task that completes when the dungeon has been read and the children repopulated, or when the not-found failure has been reported.
	/// </returns>
	[RelayCommand]
	private async Task LoadDungeonAsync(Guid id)
	{
		_dungeonId = id;

		var dto = await _dispatcher.QueryAsync<DungeonDetailDto?>(
			new GetDungeonDetailQuery(id),
			CancellationToken.None);

		if (dto is null)
		{
			_dungeonId = Guid.Empty;

			Repopulate(null);

			ErrorMessage = ResultMessage.ForFailure(DungeonNotFoundResult);

			return;
		}

		Repopulate(dto);
	}

	/// <summary>
	///		Returns to the dungeon list.
	/// </summary>
	/// <returns>
	///		A task that completes when the page has been popped.
	/// </returns>
	[RelayCommand]
	private Task GoBackAsync() =>
		_navigation.GoBackAsync();

	// Children first, then the projection. A handler reacting to Dungeon changing reads the
	// children's current values, so the page never repaints from a half-updated composition.
	private void Repopulate(DungeonDetailDto? dto)
	{
		Header.Populate(dto);
		FloorList.Populate(dto);

		Dungeon = dto;
	}
}
