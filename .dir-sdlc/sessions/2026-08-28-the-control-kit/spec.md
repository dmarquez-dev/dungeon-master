# The control kit — repo audit, sub-session 5c

Clear Session 5's fifteen remaining audit entries across the App control kit and theme, then reconcile the audit index for the whole of Session 5.

## Context

Session 5 of the repository audit was split into three sub-sessions at Gate 1 on 2026-08-26, at 32 entries and ~11.5 days. **5a** ("Settle the App boundaries") and **5b** ("Give the editor a seam") are complete and merged. **5c is the last**, and it owns two things nobody else can now do: the control-kit entries, and marking Session 5's rows in `docs/audit/00-master-index.md`.

Scope was derived rather than inherited: Session 5's declared 32 entries (`docs/audit/00-master-index.md:422`) minus the 5 that 5a closed (`MASTER-026`, `027`, `030`, `082`, `087`) minus the 14 that 5b closed (`052`-`058`, `076`, `083`, `084`, `094`-`096`, `106`, `108` — two of them only partially) leaves **13**, plus the **2** post-audit entries `MASTER-159` and `MASTER-161`, which were appended by Session 4 and suggested for Session 5.

Baseline, measured 2026-08-28 in the session worktree, `dotnet build DungeonMaster.sln` and `dotnet test DungeonMaster.sln --no-build` from `src/`:

```
Build succeeded.   0 Warning(s)   0 Error(s)      both TFMs
Passed!  - Failed: 0, Passed: 639, Total: 639 - DungeonMaster.Tests.dll
Passed!  - Failed: 0, Passed:   1, Total:   1 - DungeonMaster.App.Tests.dll
```

There is no aggregate test count. A solution-wide `dotnet test` prints two per-assembly lines; any criterion citing a count cites it per assembly with the command that produced it.

**The binding constraint on this session is evidence, not difficulty.** `DungeonMaster.App.Tests` runs outside a MAUI application host: `CreateMauiApp()` is never called, there is no `Application.Current`, and nothing deriving from `ContentView`/`ContentPage`/`Shell` can be constructed. Almost every target here is a `ContentView` subclass. So each task below names its evidence kind explicitly, and no criterion demands proof that the owning task cannot produce.

## Definition of done

Each item is one audit entry unless stated, and each is checkable by someone who did not write the code.

1. `NotNullConverter.Convert` returns `false` for the unresolved-binding sentinel, proven by an executed test. (`MASTER-007`, S2)
2. Both converters return `null` from `ConvertBack` instead of throwing, proven by an executed test. (`MASTER-034`)
3. A dungeon-list card's Edit and Delete buttons are reachable in the running app, with the mechanism of the original failure stated and demonstrated before the fix. (`MASTER-159`, S2)
4. The 44-unit touch-target minimum resolves from a single declared token; no `Minimum{Height,Width}Request="44"` literal remains in App XAML, and a ratchet test guards the absence. (`MASTER-036`)
5. `DmTheme.Gold` and `DmTheme.ScrollGoldLight` resolve distinct token keys, with `ScrollGoldLight`'s three call sites reading the new one. (`MASTER-037`)
6. `PaletteThumb.CreateShell` reads `DmTheme.StrokeHairline` instead of the literal `1`; `PaletteThumb.cs` leaves `StrokePendingMigration`; that list's doc comment is corrected to match its remaining membership. (backlog item, 5b escalation E1)
7. `DmSearchBar` expresses its focus state through visual states, with no imperative focus painting in code-behind. (`MASTER-060`)
8. `NotebookView` resolves its theme colour through a `DmTheme` accessor rather than a raw dictionary indexer and a null-forgiving operator, and a missing key fails with a named, diagnosable error. (`MASTER-061`)
9. `AreaSidebarItem` and `FloorSidebarItem` expose visual state through bindable properties rather than imperative methods. (`MASTER-098`)
10. `FloorSidebarItem` no longer re-declares and hand-forwards `AreaSidebarItem`'s events. (`MASTER-099`)
11. `FloorSidebarItem`'s tappable card body carries `HandCursorBehavior`, matching `DmCard`. (`MASTER-161`)
12. `DrawAreasViewMode` and `DrawAreasEditMode` are collapsed to one method parameterised by the alpha rule. (`MASTER-035`)
13. `FloorMapDrawable` exposes read-only collection interfaces rather than concrete mutable collections. (`MASTER-059`)
14. `Draw` no longer allocates per area, per segment and per frame, with a measured before/after allocation count. (`MASTER-097`)
15. `DmInlineEditor`'s five responsibilities are separated and `ClearFocusGlow`'s hidden state change is made explicit. (`MASTER-105`, high risk)
16. No `propertyChanged` handler imperatively writes a property the XAML already binds. (`MASTER-077`)
17. `docs/audit/00-master-index.md` records an outcome for every Session 5 row, attributed to the sub-session that closed it, with **`MASTER-094` and `MASTER-108` recorded as partial and never as done**.
18. Build stays at `0 Warning(s) 0 Error(s)` for both TFMs under `TreatWarningsAsErrors`, and both test assemblies stay green, reported per assembly.

## Scope

The fifteen entries above, the `DmTheme.StrokeHairline` backlog item, and the index reconciliation. Nine tasks, fully serialized.

### Not in scope

- **macCatalyst runtime behaviour.** The target is parked under `ADR-001`: it builds here but cannot be run. Verification for that TFM is compilation plus inspection, and no criterion demands observed macCatalyst behaviour.
- **Retabbing XAML.** Every XAML file violates `.editorconfig:38-39` (`[*.xaml]` → `indent_style = tab`) and `IDE0055` does not reach XAML. Implementers match the file they are editing and change no other line's indentation.
- **`MASTER-160`**, the undisposed path in `PreloadObjectImagesAsync` — the index suggests Session 6, beside `MASTER-013`.
- **The second halves of `MASTER-094` and `MASTER-108`** — `UI-016`'s `BindableLayout` conversion and `UI-007`'s projection narrowing. Both are L-effort rewrites; the latter changes Application-layer command results. Deferred by the user at 5b's Gate 2. 5c records them as partial, and does not implement them.
- **Five of the seven inherited backlog items** — pending the Gate 2 decision recorded as escalation E1 below.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| What is the definition of done, given no prior spec file? | The scope statement in the orchestrator's decomposition brief, decomposed above | dispatch instruction | Session directory holds no `spec.md` before this one |
| Which sentinel does `MASTER-007` concern? | `BindableProperty.UnsetValue` | project docs — **not repo-verified** | `UIKIT-004`; the identifier has zero occurrences in the tree, so T1 must verify it empirically or escalate |
| What should the `NotebookView` accessor be named? | `DmTheme.ColorByKey(string)`, not the record's `Color(string)` | judgment + repo | `Resolve<T>` precedent in `DmTheme.cs`; `Color` would shadow the type ~40 members return |
| How is `MASTER-037` closed? | By adding a distinct token, never by deleting the accessor | repo | `DmTheme.cs:129-131` prescribes it; `DmMapObjectPlaceholder` is the precedent; one of the three call sites sits in another task's ownership set |
| Where does `MASTER-077` land? | Wholly in T8, including its `DmCard` clause | judgment | One entry, one producing task, so T9 has a single source to attribute |
| Can any task run concurrently? | No — fully serial | dispatch constraint + repo | 5b serialized T5/T6 for the same reason: shared XAML compilation confounds clean-build evidence |
| What is the build/test command? | `dotnet build\|test DungeonMaster.sln` from the worktree's `src/` | project docs | 5b handoff baseline, reproduced this session |
| What may T9 change in the audit records? | Numbers and citations only, never a finding's reasoning | judgment | The index is a record; correcting its arithmetic is reconciliation, rewriting its conclusions is not |

## Contracts

- `contracts/dm-theme-accessors.cs` — produced by T3, consumed by T5. Freezes `StrokeHairline`, `ColorByKey(string)` and its three `InvalidOperationException` cases, the `DmTouchTargetMinimum` token type, and what must not be added.
- `contracts/dm-card-trailing-content.md` — produced by T2, must not be regressed by T8. States the invariants the trailing region must satisfy and deliberately does **not** state the defect's mechanism, which T2 derives.
- `contracts/audit-status-vocabulary.md` — produced by T1–T8 as `AUDIT OUTCOMES` blocks, consumed by T9. Five permitted status strings; forbids marking `MASTER-094` or `MASTER-108` done.

## Open questions

- **E1 — are the five unscoped backlog items in or out?** Resolved by the user at Gate 2, below. The architect may not retire them itself.
- **T2's keyboard reachability may prove to be a product decision.** If the Edit and Delete buttons cannot be made keyboard-reachable without changing `DmCard`'s hover-reveal design, that is a design change, not a defect fix. Encoded as an in-task stop-and-escalate rather than a Gate 2 question, because the answer depends on a reproduction T2 has not yet run.
