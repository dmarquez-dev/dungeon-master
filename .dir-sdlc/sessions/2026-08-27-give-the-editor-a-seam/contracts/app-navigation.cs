// ============================================================================================
// FROZEN CONTRACT — navigation and popup construction seams
// Session: 2026-08-27-give-the-editor-a-seam (sub-session 5b)
// Owner:    T4 (dir-frontend-engineer)
// Consumers: T3 (DungeonDetailViewModel.GoBackCommand)
//
// Closes MASTER-053 / UI-009 (view model navigates through the static Shell.Current) and
// MASTER-054 / UI-011 (DungeonListPage injects IServiceProvider and resolves the popup from it).
//
// Files: src/DungeonMaster.App/Services/Navigation/**, all new, all owned by T4.
// House style: tabs, CRLF, UTF-8 no BOM, final newline, XML doc on every public member.
// ============================================================================================

namespace DungeonMaster.App.Services.Navigation;

// --------------------------------------------------------------------------------------------
// SECTION 1 — INavigationService
//
// Measured 2026-08-27: `grep -rn "Shell\.Current" src/DungeonMaster.App/` returns exactly TWO
// hits — DungeonListViewModel.cs:104 (`await Shell.Current.GoToAsync($"dungeondetail?id={id}")`,
// UI-009 records it at :101-103) and DungeonDetailPage.xaml.cs:346 (`Shell.Current?.CurrentPage`
// inside IsTextInputActive). ONLY THE FIRST IS IN SCOPE HERE. The second is a page reading its
// own chrome to decide whether a text input has focus; it is view work in a view, it is not
// what UI-009 describes, and no task in this manifest is asked to remove it.
// --------------------------------------------------------------------------------------------

/// <summary>
///		Shell navigation, behind an interface so a view model can be exercised without a live
///		Shell.
/// </summary>
public interface INavigationService
{
	/// <summary>
	///		Navigates to a route. <paramref name="route"/> is a Shell route string and may carry
	///		a query, e.g. <c>"dungeondetail?id={guid}"</c>.
	/// </summary>
	/// <exception cref="ArgumentException"><paramref name="route"/> is null, empty or whitespace.</exception>
	Task GoToAsync(string route);

	/// <summary>
	///		Pops the current page. Implemented as <c>GoToAsync("..")</c>.
	/// </summary>
	Task GoBackAsync();
}

/// <summary>
///		The one place in the application that reads <c>Shell.Current</c> for navigation.
/// </summary>
/// <remarks>
///		A null <c>Shell.Current</c> is a defect, not an expected failure: it means navigation was
///		requested before the Shell existed. Throw <see cref="InvalidOperationException"/> naming
///		the route rather than returning silently — the backend discipline's "never swallow an
///		error to keep a code path quiet" applies, and a silently-ignored navigation is exactly
///		the failure mode UI-019 is being fixed for elsewhere in this session.
/// </remarks>
public sealed class ShellNavigationService : INavigationService;

// --------------------------------------------------------------------------------------------
// SECTION 2 — the create-dungeon popup factory
//
// UI-011's remedy: "Inject a `Func<CreateDungeonPopup>` or a small popup-factory abstraction so
// the dependency is declared, and keep the container out of the page. Note that the popup being
// resolved per click is correct and should be preserved."
//
// FROZEN CHOICE: the `Func<CreateDungeonPopup>` form. It is the smaller of the two the record
// offers, it keeps per-click resolution by construction, and it needs no new type.
//
// Registration in MauiProgram.cs (T4's file; today CreateDungeonPopup is registered transient at
// :128 and DungeonListPage at :126):
//
//     builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
//     builder.Services.AddTransient<Func<CreateDungeonPopup>>(
//         sp => sp.GetRequiredService<CreateDungeonPopup>);
//
// DungeonListPage's constructor becomes
//     public DungeonListPage(DungeonListViewModel viewModel, Func<CreateDungeonPopup> createPopup)
// and OnCreateClicked calls `createPopup()` where it calls
// `_services.GetRequiredService<CreateDungeonPopup>()` today (DungeonListPage.xaml.cs:73).
// The page must then hold NO IServiceProvider field: measured 2026-08-27,
// `grep -c "IServiceProvider" src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml.cs`
// returns 2 (the field at :17 and the parameter at :31); it must return 0.
// --------------------------------------------------------------------------------------------

// --------------------------------------------------------------------------------------------
// SECTION 3 — what does NOT change
//
// N1. DungeonListViewModel's primary constructor gains INavigationService as its SECOND
//     parameter, after the dispatcher. It is registered transient at MauiProgram.cs:125 and
//     resolves without any other edit.
// N2. Route strings are unchanged. `"dungeondetail?id={id}"` stays exactly as it is; this
//     session moves where the call is made, not what it navigates to.
// N3. No route is registered or unregistered. `Routing.RegisterRoute` does not appear in
//     MauiProgram.cs today (`grep -c "Routing.RegisterRoute" src/DungeonMaster.App/MauiProgram.cs`
//     -> 0) and must not be added.
// --------------------------------------------------------------------------------------------
