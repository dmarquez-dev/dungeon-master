# Pluralise the entity namespaces

Close `MASTER-022` by renaming the three colliding Domain entity namespaces to their plural form and renaming the matching folders, so that no namespace shares the name of a type it contains.

## Context

`namespace DungeonMaster.Domain.Entities.Dungeon` contains a type named `Dungeon`; the same holds for `Floor` and `Area`. Microsoft's namespace-naming guideline forbids this exactly because compilers then require full qualification, and that prediction has come true in this repository: 76 qualified references across `src/`, concentrated in `DungeonMethods.cs` and the Infrastructure configuration files, written as `Floor.Floor`, `Floor.PlacedObject`, `Area.Area`.

`MASTER-022` (`docs/audit/00-master-index.md:210`, source record `CORE-004` at `docs/audit/01-domain-application.md:197-234`) is one of only two open S2 entries. `CORE` is the largest open layer at 21 entries and no session since 6b-ii has touched it.

**The direction was decided at Gate 1 and is not re-openable by a task.** `CORE-004` offers two: flatten to `DungeonMaster.Domain.Entities`, or pluralise the leaf. Flattening was rejected because it breaks `dm-conventions/backend/dotnet-format.md:30` ("Namespace ... matches folder structure") and `:58` ("Folder structure mirrors the namespace hierarchy"). Pluralising satisfies both, provided the folders are renamed to match — which is why the folder rename is part of the work rather than a tidy-up.

`CORE-004`'s own prose argues for flattening on the grounds that this is "which the `dm-conventions` folder layout does not require". That parenthetical is false against the two lines above, and correcting it is in scope.

## Definition of done

1. `DungeonMaster.Domain.Entities.Dungeon`, `.Floor` and `.Area` no longer exist anywhere in `src/`; `.Dungeons`, `.Floors` and `.Areas` exist in their place. 14 namespace declarations, 70 `using` lines and 76 qualified references are converted.
2. `src/DungeonMaster.Domain/Entities/{Dungeon,Floor,Area}/` are renamed to `{Dungeons,Floors,Areas}/`, so folder structure still mirrors the namespace hierarchy. Verified by directory listing — `IDE0130` sits at default severity and the build does not check this.
3. `dotnet build src/DungeonMaster.sln` reports `0 Warning(s), 0 Error(s)`.
4. All six doc-comment `cref`s that name these namespaces are updated. The compiler cannot see them: `GenerateDocumentationFile` is set in neither `Directory.Build.props` nor any `src/*/*.csproj`, so CS1574 never fires. Keyed by enclosing member, not line number.
5. The 48 namespace strings in `src/DungeonMaster.Infrastructure/Migrations/` are handled per the disposition chosen at Gate 2, and `dotnet ef migrations has-pending-model-changes` is run — with a control run first — and its verbatim output recorded in a session artifact.
6. The `Dungeons` namespace-segment collision does not fire. `Dungeons` already names 19 namespaces under `Features.Dungeons.*`; the 14 files at risk compile clean.
7. The test baseline is preserved: 908 passed / 0 failed across `DungeonMaster.Tests` (788), `DungeonMaster.App.Tests` (97) and `DungeonMaster.App.UnitTests` (23). No test's assertions are rewritten — this is a pure rename, and a task that needs to change what a test asserts has found a real problem and escalates.
8. No `.xaml` file is edited. Measured at zero `clr-namespace` references to these namespaces.
9. `CORE-004`'s false parenthetical in `docs/audit/01-domain-application.md` is corrected.
10. `MASTER-022` is closed in `docs/audit/00-master-index.md`, citing the commits that did it. **Orchestrator work** — it needs commit hashes, which no task can obtain.
11. The 78 stale folder-path citations across `docs/audit/` (77 lines: 75 in `01-domain-application.md`, 2 in `00-master-index.md`, plus `04-app-controls-and-theme.md:261`) point at the renamed folders. `CORE-004`'s own six-line `locations:` block stays singular as historical record; `05-tests.md`'s 17 test-folder citations are unaffected and untouched.

## Scope

Seven tasks, decomposed **by project**. Ownership disjointness is structural rather than negotiated: a project's `using` lines live only in that project. The reference graph forces a mostly serial chain with two concurrent pairs — `{T3, T4}` and `{T5, T6}` — and even those may be serialized, because all tasks share one worktree and one `obj/`/`bin/`.

### Not in scope

- **Renaming the test-project mirror folders.** `src/DungeonMaster.Tests/Domain/Entities/{Area,Dungeon,Floor}/` keep singular names. The guideline does not apply there — `DungeonMaster.Tests.Domain.Entities.Floor` contains no type named `Floor` — and those namespaces already match their folders. Newly inconsistent, and flagged as coverage gap G2.
- **Enabling `GenerateDocumentationFile`.** It would make CS1574 real and ratchet the next stale `cref`, but it also enables CS1591 for every undocumented public member under `TreatWarningsAsErrors`, and that blast radius is unmeasured. Its own session.
- **Amending `dm-conventions`.** A separate repository. Its `dotnet-format.md:567` and `:588` teach the very pattern this session removes; that is filed as a follow-on, not fixed here.
- **`src/DungeonMaster.App.UiTests`.** Deliberately outside `src/DungeonMaster.sln`, and not added to it.
- **Editing `.dir-sdlc/sessions/2026-08-26-settle-the-app-boundaries/contracts/app-core-presentation.cs`**, which contains an 85th occurrence of the old namespace. It is a frozen contract from sub-session 5a and declares itself uncompiled at its lines 7-8; amending it would falsify the August record.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Pluralise or flatten? | Pluralise, and rename the folders to match | user decision | Gate 1; flatten breaks `dotnet-format.md:30` and `:58` |
| Is the 85th file part of the edit set? | No — frozen, uncompiled contract | repo evidence | `app-core-presentation.cs` header lines 7-8; set difference `85 − 84` is exactly this file, `84 − 85 = ∅` |
| Can the `Dungeons` collision fire from a mechanical rename? | No, for the three reference shapes present | measurement | four-case compiled probe; cases A/B/C compile clean, case D fails `CS0234` as the positive control |
| Does editing the `.Designer.cs` snapshots alter any DDL? | No | repo evidence | 0 namespace strings in all three non-Designer migration files; `Up`/`Down` absent from all three Designers; all four `ToTable` names unchanged |
| Does the build verify the folder rename? | No | repo evidence | `IDE0130` absent from the `.editorconfig` analyzer backlog block at `:63-75`, so default severity |
| Are the test mirror folders renamed? | No | repo evidence | no type named `Floor` in `DungeonMaster.Tests.Domain.Entities.Floor`; namespace already matches folder |
| Do the migration files get reformatted? | No | repo evidence | `.editorconfig:49-50` marks them `generated_code = true` |
| Who proves the full-solution build and the 908 total? | The orchestrator, between T5/T6 and Gate 3 | judgment | every task's evidence is scoped to its own project; 6b-iv established this shape |
| Is `has-pending-model-changes` a usable instrument here? | Yes, but the control had not been run at decomposition time | repo evidence | `.dir-sdlc/backlog.md:40` records it running successfully in an earlier session; the architect could not run it (`NETSDK1004`, no `obj/project.assets.json` in a worktree it was forbidden to write to), so T3 runs the control first |

## Contracts

- **`contracts/target-namespaces.md`** — the three target strings and folder paths, the 84-file inventory and the 85th-file exclusion, the three edit shapes, the prohibition on simplification, CRLF/tabs/`sed -i` mechanics, and the behavioural floor.
- **`contracts/namespace-collision.md`** — the `Dungeons` collision, the 14 files at risk, and the four-case compiled probe that settles it.
- **`contracts/migration-snapshot.md`** — the 48 strings per file, the tension with the database discipline, the EF protocol with its control run, and the artifact that carries the result to T6 and the reviewer.
- **`contracts/doc-comment-crefs.md`** — the six `cref`s, keyed by enclosing member, and why the build cannot see them.

## Open questions

Both were resolved by the user at Gate 2 on 2026-09-16, before any task was dispatched. Neither remains open.

- **E1 — does updating the three `*.Designer.cs` snapshots count as editing a released migration?** **Resolved: Disposition U** — update all 48, the snapshot and all three Designers. The evidence is that these files contain no DDL, no `ToTable` name and no MigrationId, only model metadata, so the discipline's purpose is not engaged. Recorded in `contracts/migration-snapshot.md` §3 with the rejected alternative kept in view. **The decision does not make the EF outcome known** — it rests on what the files contain, not on an observed EF run, so T3 still runs the control first and is forbidden from predicting the result.
- **G1 — 78 stale folder-path citations across `docs/audit/`.** **Resolved: folded into T7**, which widens to own `01-domain-application.md`, `00-master-index.md` and `04-app-controls-and-theme.md`. See definition-of-done item 11.

One item remains genuinely unresolved but is deliberately **not** this session's to settle:

- **G2 — the test-project mirror folders** keep singular names while Domain goes plural. No collision exists there and the namespaces still match their folders, so nothing is broken; it is newly inconsistent. Carried to the backlog at Gate 3 unless the user says otherwise.
