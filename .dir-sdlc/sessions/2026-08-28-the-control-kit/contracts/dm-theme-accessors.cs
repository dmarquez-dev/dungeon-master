// CONTRACT — frozen at Gate 2, session 2026-08-28-the-control-kit.
//
// The DmTheme surface that T3 ("Theme tokens and accessors") ADDS and that other tasks CONSUME.
// This file is not compiled. It is the shape T3 must produce and T5/T7 may rely on.
//
// PRODUCER:  T3 — owns src/DungeonMaster.App/Theme/DmTheme.cs and src/DungeonMaster.App/Resources/Theme/**
// CONSUMERS: T3 (StrokeHairline, from Views/DungeonDetailPage/PaletteThumb.cs)
//            T5 (ColorByKey, from Controls/NotebookView/NotebookView.xaml.cs)
//
// An implementer needing this changed STOPS and escalates. It never edits this file.

namespace DungeonMaster.App.Theme;

public static partial class DmTheme
{
	// ─────────────────────────────────────────────────────────────────────────
	// 1. StrokeHairline  —  closes the 5b backlog item
	// ─────────────────────────────────────────────────────────────────────────
	//
	// Resolves the EXISTING token declared at Resources/Theme/Tokens.xaml:566:
	//     <x:Double x:Key="DmStrokeHairline">1</x:Double>
	// No new token is declared for this member. The token is x:Double, and both
	// Border.StrokeThickness and Button.BorderWidth are System.Double, so the type
	// argument is `double` and not `int`. Using `Resolve<int>` here would throw the
	// third InvalidOperationException below at runtime, not fail at compile time.
	//
	// Sole call site required by this session:
	//     src/DungeonMaster.App/Views/DungeonDetailPage/PaletteThumb.cs, inside
	//     `internal static Border CreateShell(View content)` (declared at :86), replacing
	//     the literal `StrokeThickness = 1,` at :95.
	//
	// Consequence T3 must carry through, and the reason this member exists at all:
	// PaletteThumb.cs is currently one of the four entries in `StrokePendingMigration`
	// (src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs:286-292). That set's own
	// doc says each entry is "a conversion still to do rather than an exemption", and
	// `EveryGeometryAllowListEntryStillOffends` makes an entry that no longer offends a
	// FAILURE. So removing the literal REQUIRES removing the entry in the same change.

	public static double StrokeHairline { get; }   // => Resolve<double>("DmStrokeHairline")

	// ─────────────────────────────────────────────────────────────────────────
	// 2. ColorByKey  —  closes MASTER-061 / UIKIT-016 for T5
	// ─────────────────────────────────────────────────────────────────────────
	//
	// A runtime-keyed colour lookup that routes through the SAME private Resolve<T>
	// (DmTheme.cs:496) every fixed-key accessor uses, so a call site choosing its key at
	// runtime keeps the three diagnostics the raw indexer discards.
	//
	// NAME: `ColorByKey`, deliberately NOT the `Color(string)` that UIKIT-016 suggests at
	// 04-app-controls-and-theme.md:661-663. `Color` is the name of the type this class
	// returns from ~40 other members; a method of that name in the same class is legal C#
	// but makes every `public static Color Xxx =>` line in the file read ambiguously to a
	// human. `ColorByKey` names the operation and keeps the type name unshadowed.
	//
	// VISIBILITY: `internal`. NotebookView is in the same assembly (DungeonMaster.App);
	// nothing outside it needs a runtime-keyed lookup, and `public` would invite one.

	internal static Color ColorByKey(string key);

	// ERROR CASES — all three are InvalidOperationException, all three come from the
	// existing private Resolve<T> at DmTheme.cs:496-522, and ColorByKey must not catch,
	// wrap or suppress any of them. This is the entire point of the member: the raw
	// indexer at NotebookView.xaml.cs:420 loses all three.
	//
	//   (a) Application.Current?.Resources is null
	//       -> InvalidOperationException:
	//          "Theme resource '{key}' was read before Application.Current.Resources
	//           existed. See the remarks on DmTheme."
	//       This is the case the null-forgiving `!` at NotebookView.xaml.cs:420 currently
	//       converts into a bare NullReferenceException.
	//
	//   (b) key absent from the merged dictionaries
	//       -> InvalidOperationException:
	//          "Theme resource '{key}' is not defined under Resources/Theme."
	//       Currently a bare KeyNotFoundException naming neither the file nor the reason.
	//
	//   (c) key present but bound to a value that is not a Color
	//       -> InvalidOperationException:
	//          "Theme resource '{key}' is a {actualTypeName}, not a Color."
	//       Currently an InvalidCastException that does not name the actual type.
	//
	// No null return, and no sentinel. There is no "colour not found" that a caller may
	// handle: a missing theme key is a defect in the theme, not an expected failure.
	// (Backend discipline, "Error handling": expected failure is a value the caller must
	// handle; a defect terminates the operation. This is a defect.)

	// ─────────────────────────────────────────────────────────────────────────
	// 3. What is NOT added
	// ─────────────────────────────────────────────────────────────────────────
	//
	// NO DmTheme accessor is added for the touch-target minimum. MASTER-036's token
	// (`DmTouchTargetMinimum`, see §4) is consumed from XAML only. DmTheme.cs's own
	// standing rule is to add an accessor "at the moment a C# call site needs it", and
	// this session creates no such call site: the one C# `44` in the App project is
	// `HeightRequest = 44` at Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:2265,
	// which is a fixed height and not a minimum, and is outside T3's ownership.
	// (Measured 2026-08-28 with:
	//    grep -rn '\b44\b' --include=*.cs Controls/ Views/ Theme/ Graphics/ Resources/
	//  from src/DungeonMaster.App — one hit.)
	//
	// NO existing DmTheme member is renamed, and NO existing member is deleted.
	// In particular `ScrollGoldLight` (DmTheme.cs:135-136) KEEPS its name. UIKIT-015
	// offers deleting it as an alternative; that route is FORBIDDEN here, because its
	// three call sites are
	//     Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:146
	//     Controls/NotebookAffordance.cs:45
	//     Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:582
	// (measured 2026-08-28 with `grep -rn 'ScrollGoldLight' src/ --include=*.cs
	//  --include=*.xaml | grep -v '/obj/\|/bin/'`), and the first of those is inside T6's
	// ownership set. Deleting the accessor would put T3 inside another task's files.
	// Note the record says "four call sites"; there are three.
	//
	// ─────────────────────────────────────────────────────────────────────────
	// 4. Token keys T3 declares in Resources/Theme/Tokens.xaml
	// ─────────────────────────────────────────────────────────────────────────
	//
	//   DmTouchTargetMinimum   x:Double   44
	//       The platform touch-target floor. x:Double because every target property is
	//       MinimumHeightRequest / MinimumWidthRequest, both System.Double —
	//       TokenTypeMatchesTargetPropertyTests enforces this pairing.
	//
	//   <a distinct gold token for ScrollGoldLight>   Color
	//       Key name is T3's to choose and is NOT frozen here, because no other task
	//       reads it. It must obey Tokens.xaml's stated naming rule (Tokens.xaml:594):
	//       the theme never names a token after its value. Its value must equal
	//       DmAccentGoldLight's current value, so that this change moves no pixel.
	//       DmMapObjectPlaceholder (Tokens.xaml:274) is the precedent DmTheme.cs:129-131
	//       explicitly points at.
}
