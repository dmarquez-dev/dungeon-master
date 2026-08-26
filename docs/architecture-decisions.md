# Architecture Decision Records

This document records the architectural decisions taken in this repository, their context, and
their rationale. Records are numbered, appended in order, and separated by `---`. A record is not
rewritten once accepted; a decision that changes is superseded by a later record.

---

## ADR-001: macCatalyst Stays as a Build-Verified, Not Run-Verified, Target

**Status:** Accepted

**Context:**
`src/DungeonMaster.App/DungeonMaster.App.csproj:4` declares two target frameworks:

```
<TargetFrameworks>net10.0-windows10.0.19041.0;net10.0-maccatalyst</TargetFrameworks>
```

The repository audit raised the second one three times — `UI-021` (macCatalyst input handling is
declared unimplemented in five places while remaining a shipped target), `UI-036` (the app declares
only a `Windows Machine` run profile), and `XCUT-004` (macCatalyst is a shipped target framework
with no solution platform, run profile or input), grouped as `MASTER-030`. `XCUT-004` states the
problem precisely: "I believe the ambiguity is the defect, more than either resolution of it would
be." A reader of the repository today cannot tell whether five unimplemented input paths are a bug
backlog or dead weight, because nothing says whether the target is real.

The development loop for this project is Windows-only. There is no macOS machine in it, and there
is therefore no way here to launch the app on macCatalyst, drive its input paths, or observe any of
its behaviour at runtime. That constraint — not a judgment about whether a Mac build is desirable —
is what this record has to be honest about.

**What is verified, and what is not.** These two claims are different, and this record keeps them
apart in every sentence that follows.

*Build-verified.* The macCatalyst target compiles. Observed on 2026-08-26 from
`dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-maccatalyst --no-incremental`:

```
DungeonMaster.App -> ...\src\DungeonMaster.App\bin\Debug\net10.0-maccatalyst\maccatalyst-x64\DungeonMaster.App.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

The toolchain that produces it is present. From `dotnet workload list` on the same day:

```
Installed Workload Id      Manifest Version         Installation Source
maccatalyst                26.5.10301/10.0.100      VS 18.9.12112.369
```

Because `TreatWarningsAsErrors` is on, a clean build is a real signal: the macCatalyst half of every
conditionally compiled branch is syntactically valid, type-checks, and does not regress silently
when the Windows half is edited. `XCUT-004`'s statement that "nothing here can build the Mac one"
is not accurate as the tree stands, and correcting it is part of what this record contributes.

*Not run-verified.* No macCatalyst binary produced here has ever been launched, and none can be. No
statement about how this application behaves at runtime on macCatalyst appears anywhere in this
record, because no such behaviour has been observed. Every gap listed below is known from reading
the source, not from running it.

**The five input paths that are declared unimplemented.** Located with
`grep -rn "TODO(macOS)" src`, which returns five occurrences in `src/DungeonMaster.App` and one
further match — `src/DungeonMaster.Tests/App/AsyncDiscipline/SyncOverAsyncIsAbsentTests.cs:54` —
that is a string literal inside a test fixture and not a code path:

| # | Location | What is unimplemented |
|---|---|---|
| 1 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:329` | UIKit exposes no global `FocusManager` equivalent, so `IsTextInputActive()` falls through to `return false` unconditionally on macCatalyst and keyboard shortcuts are not suppressed while the user is typing. |
| 2 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:516` | MAUI's `PinchGestureRecognizer` exposes no pinch centroid, so pinch zoom scales around the canvas centre rather than the cursor. |
| 3 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1056` | `NSCursor` has no shape mapping 1:1 to WinUI's `SizeAll`; the closest available cursors are substituted. |
| 4 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1313` | There is no `GetAsyncKeyState` equivalent, so the right-click suppression guard that exists on Windows has no counterpart. |
| 5 | `src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:46` | The macCatalyst `Picker` popup (`UIPickerView`) styling is untested and may be illegible against the parchment surface. |

**This gap is not cosmetic, and accepting it is not the same as calling it minor.** `UI-021` assesses
the first of the five as reachable, user-visible data loss on that platform, and the reasoning is
visible in the code rather than inferred. `OnKeyPressed` guards its shortcut switch with
`if (IsTextInputActive()) return;` at `DungeonDetailPage.xaml.cs:354`. On macCatalyst that guard is
always false, and the switch it fails to gate is not read-only: `d`, `m`, `h`, `p`, `s` and `e`
switch the active map tool, and `r` — with the Place tool active — rotates the selected or hovered
placed object through `RotateObjectAsync`, a persisted mutation. Typing a name containing any of
those letters into the notebook or the flag banner, neither of which reports focus to the page,
would therefore also drive the map. The fourth entry carries the second risk `UI-021` names: if the
native gesture recogniser raises `StartInteraction` before `SecondaryTapped`, a right-click may
execute draw or place logic before the secondary handler runs. Both risks are asserted as reachable
because nothing gates the macCatalyst build behind a feature flag — not because either has been
seen happening, which would require running the app.

**Decision:**
`net10.0-maccatalyst` stays in `TargetFrameworks`. The target is not deleted, and the five input
paths above are not implemented — not by the session that writes this record, and not by the two
sessions that follow it. The target's status is hereby explicit: **macCatalyst is maintained as a
build-verified target and is not claimed to be run-verified.** That is the whole of the commitment
being made. It is not a statement that the application works on macCatalyst, and no one should read
it as one.

Three further things are accepted exactly as they stand, and are recorded here so that each becomes
a decision rather than an open ambiguity:

- **`UI-036` — the single run profile — is accepted, and `src/DungeonMaster.App/Properties/launchSettings.json` is not changed.** It declares one profile, `Windows Machine`. `UI-036` itself concludes "No action needed as a file change"; macCatalyst launch profiles are supplied by the IDE on macOS and are not conventionally checked in.
- **`XCUT-004`'s missing solution platform is accepted, and `src/DungeonMaster.sln` gains no Mac platform entry.** Its `SolutionConfigurationPlatforms` block contains `Debug|Any CPU` and `Release|Any CPU` and nothing else. `XCUT-004` describes that list as "`Any CPU`, `x64` and `x86`"; that does not match the file as it stands, and this record describes the file. Adding a platform nobody here can build against would add the appearance of support without any of it.
- **There is no macCatalyst test host, and one cannot be built here.** Multi-targeting a test project to `net10.0-maccatalyst` was attempted and failed twice: first `NU1201 — Project DungeonMaster.App is not compatible with net10.0-maccatalyst26.0`, then, after pinning the platform version to 26.5, `Empty ResolveFrameworkReference.RuntimePackPath while trying to read runtime components manifest`. The `DungeonMaster.App.Tests` project introduced alongside this record is Windows-only for that reason.

**Consequences:**
- The conditionally compiled branches are maintained twice, compiled once per build, and exercised on one platform only. `grep -rn "MACCATALYST" src/DungeonMaster.App --include=*.cs` finds **ten** preprocessor directives across **three** files: three `#if MACCATALYST` in `Behaviors/HandCursorBehavior.cs` (`:29`, `:48`, `:62`), two `#elif MACCATALYST` in `MauiProgram.cs` (`:75`, `:102`), and five `#elif MACCATALYST` in `Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` (`:328`, `:512`, `:533`, `:552`, `:1052`). Two further matches — `Controls/DmCard/DmCard.xaml.cs:91` and `Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:47` — are mentions inside comments, not directives. The macCatalyst half of each branch is accepted as unverified at runtime for as long as this decision stands.
- The compiler is the only thing checking that half. It catches breakage in signatures and types when the Windows half is edited; it cannot catch behavioural drift, and nothing else here will.
- The five paths above stay open and unscheduled. They are a known and sized gap rather than a hidden one, which is the difference this record is meant to make.
- Anyone who builds and ships this application to macCatalyst ships the input behaviour described above. This record is the disclosure; it is not a clearance.
- `MASTER-030`'s three audit entries (`UI-021`, `UI-036`, `XCUT-004`) are resolved as *decided*, not as *fixed*. The code they describe is unchanged.
- Keeping the target settles an input that another backlog item was parked on. `.dir-sdlc/backlog.md` defers evaluating a replacement for `Plugin.Maui.KeyListener` — pinned at `1.0.0-preview4` in `Directory.Packages.props`, `MASTER-104` / `XCUT-011` — explicitly because "dropping macCatalyst would change the answer". That dependency now resolves in favour of keeping the target, so the evaluation is unblocked. **It has not been performed.** No package change is in scope here, and the item stays in the backlog.

**Revisit condition:**
This decision is revisited when **a macOS machine enters the development loop**. At that point the
five paths above become a sized worklist rather than an accepted gap, and the run-verification gap
closes — the target can be launched, driven and tested, and the "build-verified, not run-verified"
distinction this record rests on stops applying. It is also revisited if the macCatalyst build stops
succeeding and cannot be repaired without running it, since a target that no longer compiles has
lost the only verification it had left, and the choice then is genuinely between repairing it and
dropping it. Until one of those happens, no work is scheduled against the five paths.

**Alternatives Considered:**
- **Drop `net10.0-maccatalyst` from `TargetFrameworks`:** Rejected — this is the cheapest resolution and it closes the same audit records, since removing the target would delete the five input paths, the ten conditional branches and `UI-036` along with it. It was rejected because macCatalyst support is still wanted and the target builds cleanly today with the workload installed. Deleting a working build target to close a documentation gap discards real optionality to buy tidiness, and re-adding it later would mean rewriting every branch removed here from nothing.
- **Implement the target fully — the four input guards, a solution platform and a run profile:** Rejected — this is the resolution `XCUT-004` and `UI-021` prefer, and it is the correct one eventually. It was rejected now because it cannot be done honestly from here. A `UIResponder.IsFirstResponder` walk, a native `UIPinchGestureRecognizer` for the pinch centroid, and an explicit secondary-button check are all guards whose entire purpose is a runtime behaviour, and no runtime behaviour can be observed in this environment. Writing them unverified would convert five honestly declared gaps into five untested implementations that look finished — a strictly worse position than the one recorded here, and one the revisit condition above is designed to reach properly instead.
- **Keep the target but add a solution platform and a macCatalyst run profile:** Rejected — it would make the repository look as though the target can be built and launched from the solution, which is exactly the appearance-of-support `XCUT-004` objects to. A run profile nobody here can run is another comment pretending to be a commitment.
- **Leave the ambiguity in place and revisit it when a Mac appears:** Rejected — this is the status quo, and `XCUT-004` identifies it as the defect itself. A parked decision with no written trigger is the same ambiguity wearing a different hat, which is why the revisit condition above is stated rather than implied.
