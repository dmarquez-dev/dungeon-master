// Contract — DungeonMaster.App.Core.Presentation
//
// Frozen at Gate 2. Session 2026-08-26-settle-the-app-boundaries (sub-session 5a).
// Authored by T3. Consumed by DungeonMaster.App's control kit, by DungeonMaster.Tests, and by
// sub-sessions 5b and 5c — 5c owns the rest of the control kit and inherits these shapes.
//
// This file is a CONTRACT, not source. It is not compiled. The real files carry full XML doc
// comments per dm-conventions/backend/dotnet-format.md section 9; members are written
// signature-only here for brevity.
//
// ---------------------------------------------------------------------------------------------
// WHAT THIS CLOSES
// ---------------------------------------------------------------------------------------------
//
// MASTER-027 / UIKIT-005: the reusable control kit binds directly to Application query DTOs.
// AreaSidebarItem.Area is typed AreaDto, FloorSidebarItem.Floor is typed FloorDto, and
// FloorMapDrawable takes FloorDto, AreaDto and PlacedObjectDto — all from
// DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail. The kit's public API is
// therefore one query's projection, and adding a field to that query changes the surface of three
// presentation types.
//
// Verified in this session, in the worktree, so the worklist is a measurement and not an estimate.
// `grep -rn "using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;"` over
// src/DungeonMaster.App finds that import in exactly FIVE files:
//
//   Controls/AreaSidebarItem/AreaSidebarItem.xaml.cs:1
//   Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:1
//   Graphics/FloorMapDrawable.cs:4
//   ViewModels/DungeonDetailViewModel.cs:18
//   Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:11
//
// and `grep -c "AreaDto\|FloorDto\|PlacedObjectDto"` over those same five files reports
// 10, 13, 4, 1 and 7 matching lines respectively. Those counts include the import line itself.
//
// THREE of the five stop naming the DTOs entirely: the two sidebar controls and FloorMapDrawable.
// TWO keep naming them, and that is correct rather than an omission — DungeonDetailViewModel is
// what calls the dispatcher and receives DungeonDetailDto, and DungeonDetailPage is the page
// boundary UIKIT-005 names as the place to map. A criterion demanding zero DTO references across
// all five would be demanding that the query result arrive from nowhere.
//
// ---------------------------------------------------------------------------------------------
// THE SHAPES ARE DELIBERATELY CLOSE TO THE DTOs, AND THAT IS NOT A FAILURE
// ---------------------------------------------------------------------------------------------
//
// AreaItem carries the same five values AreaDto carries. The point of MASTER-027 is not that the
// kit needs fewer fields; it is that the kit needs its OWN type, so that a change to
// GetDungeonDetail's projection is no longer a change to three presentation types' public API. A
// reviewer reading "these look identical" is reading the intended result.
//
// One thing does change shape: the derived value. Both sidebar controls currently recompute
//   !string.IsNullOrWhiteSpace(x.Description) || !string.IsNullOrWhiteSpace(x.Notes)
// at AreaSidebarItem.xaml.cs:80-81 and FloorSidebarItem.xaml.cs:153-154. Two copies of one
// derivation is exactly what the frontend discipline forbids ("derived values are computed, never
// stored" and "two components holding the same fact will disagree"). It is computed once, here.
//
// ---------------------------------------------------------------------------------------------

namespace DungeonMaster.App.Core.Presentation;

using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.ValueObjects;

/// The control kit's own shape for an area. Consumed by AreaSidebarItem, FloorSidebarItem and
/// FloorMapDrawable. Never by a handler.
///
/// Referencing Domain value objects (PathSegment) is intended and is not the coupling MASTER-027
/// describes: the Dependency Rule permits an outer layer to name an inner one, and PathSegment is
/// the vocabulary an outline is expressed in. The coupling being removed is to
/// GetDungeonDetail's PROJECTION, not to the domain.
public sealed record AreaItem(
	Guid Id,
	string Name,
	string? Description,
	string? Notes,
	IReadOnlyList<PathSegment> Outline)
{
	/// True when either Description or Notes holds non-whitespace text.
	/// Computed, never stored. Replaces the duplicated expression at AreaSidebarItem.xaml.cs:80-81.
	public bool HasNotebookContent { get; }
}

/// The control kit's own shape for a floor. Consumed by FloorSidebarItem and FloorMapDrawable.
public sealed record FloorItem(
	Guid Id,
	int Number,
	string? Name,
	string? Description,
	string? Notes,
	IReadOnlyList<AreaItem> Areas,
	IReadOnlyList<PlacedObjectItem> Objects)
{
	/// True when either Description or Notes holds non-whitespace text.
	/// Computed, never stored. Replaces the duplicated expression at FloorSidebarItem.xaml.cs:153-154.
	public bool HasNotebookContent { get; }
}

/// The control kit's own shape for an object placed on a floor. Consumed by FloorMapDrawable.
///
/// Position is the object's bounding-box CENTRE in floor-absolute canvas pixels, not its top-left.
/// That is what PlacedObjectDto.Position means today (documented at FloorMapDrawable.cs:160) and
/// the meaning does not change here. Restating it is not redundant: this is the one field in the
/// three shapes whose name does not tell a reader which corner it refers to.
public sealed record PlacedObjectItem(
	Guid Id,
	ObjectKind Kind,
	CanvasPoint Position,
	ObjectRotation Rotation,
	float Scale);

// ---------------------------------------------------------------------------------------------
// THE TRANSLATION POINT
// ---------------------------------------------------------------------------------------------
//
// This is the ONLY type in DungeonMaster.App.Core permitted to name a type from
// DungeonMaster.Application.Features.*. See contracts/new-projects.md section 2.
//
// It lives in DungeonMaster.App.Core rather than in DungeonMaster.App for one reason: there, a test
// in the net10.0 DungeonMaster.Tests project can reach it. A mapper placed in the App assembly
// would be unreachable by every existing test, which is the defect XCUT-002 records and this
// session exists to stop reproducing.

public static class DungeonDetailPresentationMapper
{
	// Ordering guarantee consumers may rely on: every collection is mapped index-for-index, so
	// Areas and Objects come out in the order the query produced them. FloorSidebarItem renders
	// areas in list order and DungeonDetailPage hit-tests areas in list order, so a reordering
	// here would be user-visible.
	//
	// Purity guarantee: these methods are pure. They read no ambient state, hold none, and calling
	// one twice with the same input yields equal results. They must not be given a cache.
	//
	// ERROR CASES — the part implementers most often guess differently. All four methods:
	//
	//   - A null argument throws ArgumentNullException whose ParamName is that parameter's name.
	//     Nothing returns null and nothing returns an empty result to paper over a null input.
	//   - A null ELEMENT inside floors, floor.Areas or floor.Objects throws ArgumentException whose
	//     message names the collection and the zero-based index of the null element, and whose
	//     ParamName is the collection parameter's name. It must not throw
	//     NullReferenceException, and it must not silently skip the element: a floor that quietly
	//     loses an area renders a map missing a room, which is the class of silent failure the
	//     whole audit is about.
	//   - No other exception is thrown. There is no validation of Name, Scale, Rotation or Outline;
	//     an empty Outline, a negative Scale and a whitespace Name all map through unchanged,
	//     exactly as they reach the renderer today.

	// ACCESSIBILITY — decided by the user at Gate 2, on coverage gap 3.
	//
	// ToItems is the ONE operation an external caller uses: DungeonDetailViewModel calls it once,
	// where the dispatcher's DungeonDetailDto arrives. It is public.
	//
	// The three ToItem overloads had, in the coverage table this contract shipped with, an
	// implementer and no consumer outside this file. They are internal. An operation nothing
	// outside the assembly calls does not belong on the assembly's public surface, and leaving it
	// public invites sub-sessions 5b and 5c to bind to a shape that was never meant to be a
	// boundary. Do not widen them back to public to make something compile — if an App file turns
	// out to need an individual ToItem, the mapping boundary is drawn in the wrong place and that
	// is a contract-change escalation, not a keyword edit.
	//
	// The class itself stays `public static class`, because ToItems is public.
	//
	// HOW THE INTERNAL OVERLOADS STAY TESTABLE. internal members of DungeonMaster.App.Core are
	// invisible to DungeonMaster.Tests unless the assembly grants it, so the grant is made
	// explicitly rather than left for whoever hits the compile error:
	//
	//   src/DungeonMaster.App.Core/AssemblyInfo.cs
	//       using System.Runtime.CompilerServices;
	//
	//       [assembly: InternalsVisibleTo("DungeonMaster.Tests")]
	//
	// This is the repository's own precedent, not an invention: src/DungeonMaster.Application/
	// AssemblyInfo.cs and src/DungeonMaster.Domain/AssemblyInfo.cs carry that identical attribute
	// today — `grep -rn "InternalsVisibleTo" src --include=*.cs --include=*.csproj --include=*.props`
	// over the worktree on 2026-08-26 returned exactly those TWO lines and nothing else.
	//
	// The file lives at the App.Core project root, which is T1's ownership set, so T1 creates it.
	// T3 owns only src/DungeonMaster.App.Core/Presentation/** and must not create or edit it; T3
	// verifies it is present before writing its first test against an internal overload, and
	// reports a blocked dependency if it is not. The error cases above are asserted against each
	// overload BY NAME, which is possible only with the grant in place.

	internal static AreaItem ToItem(AreaDto area);

	internal static PlacedObjectItem ToItem(PlacedObjectDto placedObject);

	internal static FloorItem ToItem(FloorDto floor);

	public static IReadOnlyList<FloorItem> ToItems(IReadOnlyList<FloorDto> floors);
}

// ---------------------------------------------------------------------------------------------
// WHAT THE CONSUMERS BECOME
// ---------------------------------------------------------------------------------------------
//
//   AreaSidebarItem.AreaProperty      typeof(AreaDto)  -> typeof(AreaItem)
//   AreaSidebarItem.Area              AreaDto?         -> AreaItem?
//   AreaSidebarItem's five events     EventHandler<AreaDto>  -> EventHandler<AreaItem>
//                                     (SelectRequested, HoverStarted, HoverEnded,
//                                      DeleteRequested, NotebookRequested)
//
//   FloorSidebarItem.FloorProperty    typeof(FloorDto) -> typeof(FloorItem)
//   FloorSidebarItem.Floor            FloorDto?        -> FloorItem?
//   FloorSidebarItem's five AreaDto-carrying events and its DeleteRequested/NotebookRequested
//                                     -> the corresponding Item type
//
//   FloorMapDrawable.Floor            FloorDto?        -> FloorItem?
//   FloorMapDrawable.GetEffectiveOutline(AreaDto)      -> (AreaItem)
//   FloorMapDrawable.DrawPlacedObject(ICanvas, PlacedObjectDto) -> (ICanvas, PlacedObjectItem)
//
//   DungeonDetailViewModel.Floors     ObservableCollection<FloorDto> -> ObservableCollection<FloorItem>
//   DungeonDetailPage's four AreaDto-typed members (DeleteAreaAsync, FocusArea, EraseAreaAsync,
//                                     HitTestArea) and its PlacedObjectDto local -> Item types
//
// The bindable-property type argument matters: BindableProperty.Create's second argument is the
// declared property type and a mismatch between it and the CLR property is the classic control-kit
// defect. This repository does not have that defect today (the audit checked all nine controls) and
// must not acquire it here.
//
// WHERE THE MAPPING HAPPENS. DungeonDetailViewModel receives DungeonDetailDto from the dispatcher
// and calls DungeonDetailPresentationMapper.ToItems once, so that Floors holds FloorItem. That is
// "map from the query DTO at the page boundary" as UIKIT-005 asks, with the boundary drawn at the
// view model rather than in the page's event handlers — the page reads the view model's collection
// and there is exactly one call site to audit. No control, and no code under
// src/DungeonMaster.App/Controls/, may call the mapper.
//
// ---------------------------------------------------------------------------------------------
// EXPLICITLY OUT OF SCOPE FOR THIS SESSION
// ---------------------------------------------------------------------------------------------
//
// The rest of the control kit is sub-session 5c's, and MASTER-159 (DmCard's trailing presenter
// never renders, so Edit and Delete are unreachable on every dungeon-list card) is 5c's too. This
// session retypes three controls and changes no rendering behaviour. If a retyped control's
// behaviour changes at all, that is a defect in this task, not a bonus fix.
