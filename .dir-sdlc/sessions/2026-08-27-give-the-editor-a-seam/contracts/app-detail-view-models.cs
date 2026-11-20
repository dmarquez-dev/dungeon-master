// ============================================================================================
// FROZEN CONTRACT — the dungeon-detail view-model composition
// Session: 2026-08-27-give-the-editor-a-seam (sub-session 5b)
// Owner:    T3 (dir-frontend-engineer)
// Consumers: T5 (DungeonDetailPage.xaml.cs), T6 (DungeonDetailPage.xaml)
//
// Everything here lives in src/DungeonMaster.App, namespace DungeonMaster.App.ViewModels,
// target frameworks net10.0-windows10.0.19041.0 and net10.0-maccatalyst (App.csproj:4 —
// note the trailing `.0` on the Windows moniker and the absence of a version on maccatalyst;
// `net10.0-windows10.0.19041` and `net10.0-maccatalyst26.5` are NOT this repository's monikers
// and fail with NETSDK1005).
//
// House style: tabs, CRLF, UTF-8 no BOM, final newline, XML doc on every public member.
// ============================================================================================

namespace DungeonMaster.App.ViewModels;

using MapTool = DungeonMaster.App.Core.Editor.MapTool;   // step 2 of the MapTool migration; see contracts/app-core-editor.cs

// --------------------------------------------------------------------------------------------
// SECTION 1 — The editing context (new file: ViewModels/IDungeonEditingContext.cs, owned by T3)
//
// This is the seam that makes UI-007's reload policy explicit instead of implicit. Today the
// policy is a copy of `await LoadDungeonAsync(_dungeonId);` pasted at the end of some mutations
// and omitted from others. Measured 2026-08-27 in DungeonDetailViewModel.cs
// (`grep -n "await LoadDungeonAsync" src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs`
// -> 11 call sites, at :241 :273 :309 :342 :376 :409 :448 :482 :598 :625 :675). Fourteen
// members send a mutating command; THREE of them do not reload — SaveDungeonAsync (:190),
// AddAreaWithOutlineAsync (:490) and SaveNewAreaOutlineAsync (:540). UI-007's own text says
// "eleven of the twelve" and names ONE exception; that is stale and the tree wins.
// --------------------------------------------------------------------------------------------

/// <summary>
///		What a child view model needs from the dungeon-detail composition root: which dungeon is
///		loaded, how to reload it, and where to put a failure.
/// </summary>
public interface IDungeonEditingContext
{
	/// <summary>
	///		The identifier of the loaded dungeon, or <see cref="Guid.Empty"/> when none is
	///		loaded. This is the SINGLE source of the dungeon id: no command argument record
	///		declared below carries a dungeonId, which is what closes UI-008's "three re-taking an
	///		id the type holds".
	/// </summary>
	Guid DungeonId { get; }

	/// <summary>
	///		Whether a dungeon is loaded. When it returns false the caller must abandon the
	///		operation; <see cref="ReportNoDungeonLoaded"/> has already set the error message.
	/// </summary>
	bool NoDungeonIsLoaded();

	/// <summary>
	///		Sets ErrorMessage from a failed <see cref="Result"/> via ResultMessage.ForFailure.
	/// </summary>
	void ReportFailure(Result result);

	/// <summary>Clears ErrorMessage.</summary>
	void ClearError();

	/// <summary>
	///		Re-runs GetDungeonDetailQuery for <see cref="DungeonId"/> and repopulates every child
	///		view model. THE RELOAD POLICY IS UNIFORM: every member that sends a mutating command
	///		and succeeds awaits this before returning. There are no exceptions, and a member that
	///		wants one must be given a documented reason at its own declaration.
	/// </summary>
	Task ReloadAsync();
}

// --------------------------------------------------------------------------------------------
// SECTION 2 — DungeonDetailViewModel (existing file, retyped by T3)
//
// It stops being a 678-line grab bag (`wc -l` on 2026-08-27 -> 678; UI-006's heading says 397
// and its location says :27-397 — both stale) and becomes a composition root.
// --------------------------------------------------------------------------------------------

/// <summary>
///		Composition root for the dungeon detail page.
/// </summary>
public partial class DungeonDetailViewModel : ObservableObject, IQueryAttributable, IDungeonEditingContext
{
	/// <param name="dispatcher">DungeonMaster.Application.Common.Dispatch.IDispatcher.</param>
	/// <param name="navigation">See contracts/app-navigation.cs. Registered by T4 in MauiProgram.cs.</param>
	public DungeonDetailViewModel(AppDispatcher dispatcher, INavigationService navigation);

	// -- Bound directly by DungeonDetailPage.xaml. THESE THREE MEMBERS MUST KEEP THEIR NAMES ---
	//    and stay on THIS type, because the page's root sets x:DataType="vm:DungeonDetailViewModel"
	//    (DungeonDetailPage.xaml:10) and binds `Dungeon.FloorCount` at :111, `ErrorMessage` at
	//    :120 and `Dungeon.Name` at :130 — the only three {Binding} expressions in that file,
	//    measured 2026-08-27 by `grep -o "{Binding [A-Za-z.]*" DungeonDetailPage.xaml`.
	//    Moving them would silently break bindings T6 is not asked to rewrite.

	[ObservableProperty] public partial DungeonDetailDto? Dungeon { get; set; }
	[ObservableProperty] public partial string? ErrorMessage { get; set; }

	// -- The three children. Constructed by this type, NOT resolved from the container, so
	//    MauiProgram.cs (T4's file) needs no new registration and this task does not touch it. --

	/// <summary>Dungeon metadata editing: name, description, notes, save.</summary>
	public DungeonHeaderViewModel Header { get; }

	/// <summary>Floor lifecycle and the floor collection the page's sidebar renders.</summary>
	public FloorListViewModel FloorList { get; }

	/// <summary>Canvas content: active tool, drawing session, area and placed-object commands.</summary>
	public MapCanvasViewModel Canvas { get; }

	// -- Load and navigation -----------------------------------------------------------------

	/// <inheritdoc cref="IDungeonEditingContext.DungeonId"/>
	public Guid DungeonId { get; }

	/// <summary>
	///		The load started by the most recent <see cref="ApplyQueryAttributes"/>. Unchanged in
	///		meaning from today's member; the page still awaits it.
	/// </summary>
	public Task LoadTask { get; }

	/// <inheritdoc/>
	public void ApplyQueryAttributes(IDictionary<string, object> query);

	/// <summary>Loads a dungeon by id and repopulates the three children.</summary>
	[RelayCommand] private Task LoadDungeonAsync(Guid id);

	/// <summary>
	///		Returns to the dungeon list. Bound by T6 from DungeonDetailPage.xaml as
	///		`Command="{Binding GoBackCommand}"`; see the MASTER-058 criteria on T6. Implemented
	///		as `navigation.GoBackAsync()`; it does NOT touch Shell.Current directly.
	/// </summary>
	[RelayCommand] private Task GoBackAsync();

	// IDungeonEditingContext members are implemented explicitly or publicly, T3's choice.
}

// --------------------------------------------------------------------------------------------
// SECTION 3 — DungeonHeaderViewModel (new file: ViewModels/DungeonHeaderViewModel.cs)
// --------------------------------------------------------------------------------------------

/// <summary>Dungeon metadata editing.</summary>
public partial class DungeonHeaderViewModel(AppDispatcher dispatcher, IDungeonEditingContext context) : ObservableObject
{
	[ObservableProperty] public partial string DungeonName { get; set; }
	[ObservableProperty] public partial string? DungeonDescription { get; set; }
	[ObservableProperty] public partial string? DungeonNotes { get; set; }

	/// <summary>
	///		Sends UpdateDungeonCommand and, on success, awaits <see cref="IDungeonEditingContext.ReloadAsync"/>.
	///		THE RELOAD IS NEW: SaveDungeonAsync is one of the three mutations that does not
	///		reload today (DungeonDetailViewModel.cs:190-210, measured 2026-08-27). Adding it is
	///		what makes the policy uniform, and it is a deliberate behaviour change.
	/// </summary>
	[RelayCommand] private Task SaveAsync();

	/// <summary>Sets the three editable fields from a loaded DTO. Called by the composition root during load.</summary>
	public void Populate(DungeonDetailDto? dto);
}

// --------------------------------------------------------------------------------------------
// SECTION 4 — FloorListViewModel (new file: ViewModels/FloorListViewModel.cs)
// --------------------------------------------------------------------------------------------

/// <summary>Floor lifecycle and the collection the sidebar renders.</summary>
public partial class FloorListViewModel(AppDispatcher dispatcher, IDungeonEditingContext context) : ObservableObject
{
	/// <summary>
	///		The floors of the loaded dungeon. This is the collection that today lives at
	///		DungeonDetailViewModel.cs:82 as `public ObservableCollection&lt;FloorItem&gt; Floors`.
	///		T5 reads it as `_viewModel.FloorList.Items` and subscribes to
	///		`Items.CollectionChanged`.
	/// </summary>
	public ObservableCollection<FloorItem> Items { get; }

	/// <summary>
	///		Clears and refills <see cref="Items"/> from a loaded DTO via
	///		DungeonDetailPresentationMapper.ToItems. Called by the composition root during load.
	/// </summary>
	/// <remarks>
	///		RECORDED BEHAVIOUR CHANGE, carried from sub-session 5a and NOT to be reverted:
	///		DungeonDetailPresentationMapper.ToItems throws ArgumentNullException on a null
	///		collection and ArgumentException on a null element, where the loop it replaced
	///		(before 5a) would have thrown NullReferenceException or silently added a null. The
	///		call site is DungeonDetailViewModel.cs:179 today and moves here. T3 must carry a
	///		comment at the new call site saying so; see T3's acceptance criteria.
	/// </remarks>
	public void Populate(DungeonDetailDto? dto);

	[RelayCommand] private Task AddFloorAsync();
	[RelayCommand] private Task RemoveFloorAsync(Guid floorId);
	[RelayCommand] private Task UpdateFloorAsync(UpdateFloorArgs args);
	[RelayCommand] private Task ReorderFloorAsync(ReorderFloorArgs args);
}

/// <summary>Arguments for renaming or re-describing a floor.</summary>
public sealed record UpdateFloorArgs(Guid FloorId, string? Name, string? Description, string? Notes);

/// <summary>Arguments for moving a floor to a new position in the stack.</summary>
public sealed record ReorderFloorArgs(Guid FloorId, int NewNumber);

// --------------------------------------------------------------------------------------------
// SECTION 5 — MapCanvasViewModel (new file: ViewModels/MapCanvasViewModel.cs)
//
// This is where UI-008 is closed. TODAY the type carries four public Task-returning METHODS
// outside the command surface — MoveAreaOutlineAsync (:455), RemovePlacedObjectAsync (:604) and
// the two UpdatePlacedObjectAsync overloads (:631, :648), measured 2026-08-27 by
// `grep -nE "^\s*public\s+(async\s+)?(Task|ValueTask)" DungeonDetailViewModel.cs`, which returns
// five lines only because `public Task LoadTask { get; private set; }` at :89 is a PROPERTY, not
// a method. UI-008's heading says five and its own body names four; four is the measured figure.
// --------------------------------------------------------------------------------------------

/// <summary>Canvas content and the tool state the page's gestures drive.</summary>
public partial class MapCanvasViewModel(AppDispatcher dispatcher, IDungeonEditingContext context) : ObservableObject
{
	/// <summary>
	///		The active map tool. THIS IS THE SINGLE OWNER of that fact — MapEditorState
	///		deliberately does not store it (see contracts/app-core-editor.cs, section 3) and the
	///		page must not shadow it in a field.
	/// </summary>
	[ObservableProperty] public partial MapTool ActiveTool { get; set; }

	/// <summary>The area currently being edited in draw mode. Was DungeonDetailViewModel.EditingAreaId.</summary>
	[ObservableProperty] public partial Guid? EditingAreaId { get; set; }

	/// <summary>
	///		The in-progress vertex list. Moved as-is from DungeonDetailViewModel.cs:77;
	///		DungeonMaster.App.Core.Map.MapDrawingSession itself is NOT edited this session.
	/// </summary>
	public MapDrawingSession DrawingSession { get; }

	[RelayCommand] private Task AddAreaAsync(Guid floorId);
	[RelayCommand] private Task RemoveAreaAsync(RemoveAreaArgs args);
	[RelayCommand] private Task UpdateAreaAsync(UpdateAreaArgs args);
	[RelayCommand] private Task AddAreaWithOutlineAsync(AddAreaWithOutlineArgs args);
	[RelayCommand] private Task MoveAreaOutlineAsync(MoveAreaOutlineArgs args);
	[RelayCommand] private Task AddPlacedObjectAsync(PlaceObjectArgs args);
	[RelayCommand] private Task RemovePlacedObjectAsync(RemovePlacedObjectArgs args);
	[RelayCommand] private Task UpdatePlacedObjectAsync(UpdatePlacedObjectArgs args);
}

/// <summary>Arguments for removing an area from a floor.</summary>
public sealed record RemoveAreaArgs(Guid FloorId, Guid AreaId);

/// <summary>Arguments for renaming or re-describing an area.</summary>
public sealed record UpdateAreaArgs(Guid FloorId, Guid AreaId, string Name, string? Description, string? Notes);

/// <summary>Arguments for creating an area and writing the outline the user just drew.</summary>
public sealed record AddAreaWithOutlineArgs(Guid FloorId, IReadOnlyList<PathSegment> Outline);

/// <summary>Arguments for writing a moved area's translated outline.</summary>
public sealed record MoveAreaOutlineArgs(Guid FloorId, Guid AreaId, IReadOnlyList<PathSegment> Outline);

/// <summary>Arguments for placing a new object. Kept from DungeonDetailViewModel.PlaceObjectArgs (:566), now top-level.</summary>
public sealed record PlaceObjectArgs(Guid FloorId, ObjectKind Kind, CanvasPoint Position, ObjectRotation Rotation, float Scale);

/// <summary>Arguments for removing a placed object. NO dungeonId: the context holds it.</summary>
public sealed record RemovePlacedObjectArgs(Guid FloorId, Guid ObjectId);

/// <summary>
///		Arguments for moving, rotating or rescaling a placed object. NO dungeonId. The two
///		overloads this replaces differed only in whether a scale was supplied, so
///		<paramref name="Scale"/> defaults to ObjectScale.Default and the second overload is
///		deleted rather than reproduced.
/// </summary>
public sealed record UpdatePlacedObjectArgs(
	Guid FloorId,
	Guid ObjectId,
	CanvasPoint Position,
	ObjectRotation Rotation,
	float Scale = ObjectScale.Default);

// --------------------------------------------------------------------------------------------
// SECTION 6 — Rules binding every member above
//
// R1. Every command follows the shape the file already uses and that shape is frozen:
//       ClearError -> if (context.NoDungeonIsLoaded()) return; -> await dispatcher.SendAsync(...)
//       -> if (result.IsFailure) { context.ReportFailure(result); return; }
//       -> await context.ReloadAsync();
//     The trailing reload has NO exceptions (see section 1).
//
// R2. No member declared here takes a dungeonId parameter. The page must never again pass
//     `_viewModel.Dungeon!.Id`; the null-forgiving read is the correctness hazard UI-008 names.
//
// R3. No member of any type here is a public Task-returning METHOD outside [RelayCommand],
//     with two named exceptions that are not commands and not async: the `Populate` methods in
//     sections 3 and 4, which are `void`.
//
// R4. No type here references Shell, Application.Current, or any static ambient accessor.
//     Navigation goes through INavigationService (contracts/app-navigation.cs).
//
// R5. NOT IN SCOPE, and a task must not attempt it: replacing the whole-aggregate reload with a
//     patched collection. UI-007's remedy has a second half — "have the mutating commands return
//     or apply the changed projection rather than re-querying the aggregate, so the observable
//     collection is patched instead of rebuilt" — which changes the Application layer's command
//     results and is reported as an UNCOVERED row in this session's coverage mapping rather than
//     silently absorbed here.
// --------------------------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// CORRECTION APPENDED 2026-08-27 AT GATE 3 (contradicts finding C2, reviews/T6.md)
//
// This contract assumed that the page's root x:DataType makes every view-model
// binding compile. That is FALSE for one class of member, and the assumption
// cost the session a build failure.
//
// The discriminator, reproduced this session on both TFMs:
//
//   * [ObservableProperty] public partial members - e.g. Dungeon, ErrorMessage -
//     HAVE a user-source declaration, so MAUI's binding generator sees them and
//     the binding compiles.
//
//   * [RelayCommand]-generated members - e.g. GoBackCommand, CreateCommand -
//     have NO user-source declaration at all. MAUI's binding generator cannot
//     see another generator's output and emits MAUIG2045, which fails the build
//     under TreatWarningsAsErrors (Directory.Build.props:5).
//
// The remedy is a PER-BINDING opt-out, not dropping the root annotation:
//     Command="{Binding GoBackCommand, x:DataType={x:Null}}"
// See DungeonDetailPage.xaml:58. Dropping the root x:DataType - what
// CreateDungeonPopup.xaml:15-47 did - decompiles every binding on the page.
//
// CAVEAT, so this is not over-trusted: it does not explain why CancelCommand,
// from the same generator, went UNREPORTED on CreateDungeonPopup. The rule is a
// strong heuristic, not proven complete.
// ---------------------------------------------------------------------------
