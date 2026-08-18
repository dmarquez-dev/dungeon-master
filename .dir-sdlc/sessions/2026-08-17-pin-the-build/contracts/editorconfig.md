# Contract — `.editorconfig` structure, section ownership, and the analyzer backlog

**Producers:** T2 (creates the file, all sections). T3 (changes exactly one line — §5).
**Consumer:** T3.
**Frozen at Gate 2.**

`.editorconfig` is the one file two tasks write. T2 and T3 are strictly serialised by `dependsOn`,
so they never hold it at the same time, but the boundary still has to be exact: this section says
which lines belong to which task and forbids everything else.

The indent character is **TAB**. That is settled — `XCUT-012` argues it out in full and the
authority is `dm-conventions/backend/dotnet-format.md`, every indentation instruction in which is
denominated in tabs (§6's "+1 tab" per element type and its four-row summary table; §9's "two tabs"
for XML doc tag descriptions; §3's worked examples). Do not reopen it.

---

## 1. Section order — fixed

`.editorconfig` at the repository root, UTF-8 no BOM, CRLF, trailing newline, tab-indented where it
indents at all. Sections appear in exactly this order, and no section may be added or reordered. **The headers in
this table are the authoritative literal text** — §2 describes each section's keys and never
restates a header.

> **Amended 2026-08-17, after T2's review.** §1 and §2 previously gave two different literal
> headers for section 3: the table said `[*.{csproj,props,targets,sln,slnx,config}]` while §2's
> `global.json` discussion said it "becomes"
> `[{*.csproj,*.props,*.targets,*.sln,*.slnx,*.config,global.json}]`. The second was the intended
> one. The table row now carries it, §2 no longer restates it, and the `global.json`-inclusive glob
> appears exactly once in this contract.

| # | Header | Written by |
|---|---|---|
| 0 | `root = true` (before any section) | T2 |
| 1 | `[*]` | T2 |
| 2 | `[*.cs]` | T2 |
| 3 | `[{*.csproj,*.props,*.targets,*.sln,*.slnx,*.config,global.json}]` | T2 |
| 4 | `[*.xaml]` | T2 |
| 5 | `[*.{json,yml,yaml}]` | T2 |
| 6 | `[*.md]` | T2 |
| 7 | `[src/DungeonMaster.Infrastructure/Migrations/*.cs]` | T2 |
| 8 | the analyzer-backlog block (see §4) | T2 authors; T3 edits one line |

## 2. Sections 0–7 — required keys

**Section 1 `[*]`**

```ini
indent_style = tab
indent_size = 4
tab_width = 4
charset = utf-8
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true
```

`charset = utf-8` (not `utf-8-bom`) is the encoding decision recorded in
`repository-root-config.md` §6. `end_of_line = crlf` matches every file in the repository today and
matches `.gitattributes`'s `text=auto` on a Windows checkout.

**Section 2 `[*.cs]`** — tabs, plus the style preferences that are *statements of the convention
document* rather than severity escalations:

```ini
indent_style = tab
indent_size = 4
tab_width = 4
csharp_prefer_braces = true:silent
dotnet_style_qualification_for_field = false:silent
dotnet_style_qualification_for_property = false:silent
dotnet_style_qualification_for_method = false:silent
csharp_style_namespace_declarations = file_scoped:silent
dotnet_naming_rule.private_fields_underscore_camel_case.severity = suggestion
```

Naming rule detail (`dotnet-format.md` §1, private field `_camelCase`) is written out in full with
its `symbols` and `style` triplet. **Every style key in this section carries `:silent` or
`suggestion`, never `warning` or `error`.** The reason is `MASTER-078`'s ground rule: this session
turns enforcement *on*; Session 7 is what fixes the diagnostics. `csharp_prefer_braces` in
particular has known violations already recorded (`UI-028`, `UIKIT-021`), so escalating it here
would break the build for work this session has explicitly excluded.

**Section 3** (header per §1) — `indent_style = tab`. This is the section `XCUT-012` asks for by
name, extended to cover `nuget.config`, the new `Directory.Packages.props`, and `global.json`.

**Section 4 `[*.xaml]`** — `indent_style = tab`. Declared so editors behave, but note:
`.xaml` whitespace was never censused by `XCUT-012` and **no `.xaml` file is reformatted in this
session**. `EnforceCodeStyleInBuild` does not apply IDE0055 to XAML, so this section has no build
consequence.

**Sections 5 and 6** — `indent_style = space`, `indent_size = 2`. Tabs are illegal in YAML and
conventionally wrong in JSON; `global.json` and `nuget.config` are the two files here that this
overrides for, and `global.json` matching `[*.{json,...}]` means it is space-indented in the editor
even though `repository-root-config.md` §1 shows it tab-indented. **Resolve this in favour of the
contract file: `global.json` is tab-indented, which is why section 3's glob in §1 names it
explicitly and `global.json` is therefore matched by section 3 rather than section 5.** Section 5
then covers the remaining JSON (`launchSettings.json`, `packages.lock.json`) at two spaces, which is what the
tooling that generates them emits.

**Section 7 `[src/DungeonMaster.Infrastructure/Migrations/*.cs]`**

```ini
generated_code = true
```

`XCUT-012`: "Generated migrations can be excluded with their own section rather than reformatted."
This is that exclusion, and it is what keeps T3's file list at 62 rather than 67. No `indent_style`
is declared here — the five files keep their EF-emitted spaces, and `generated_code = true`
suppresses IDE and CA diagnostics on them so they cannot break the build under `MASTER-078`.

## 3. What must NOT go in `.editorconfig`

- No `dotnet_diagnostic.*` line outside the block in §4.
- No `[*]`-level `dotnet_analyzer_diagnostic.category-*.severity` blanket. `AnalysisMode=Recommended`
  in `Directory.Build.props` is the single lever; a second lever here would make the effective rule
  set depend on which file the reader opened.
- No `# ReSharper` or IDE-vendor sections.

## 4. The analyzer backlog block — `MASTER-078`

Everything `AnalysisMode=Recommended` and `EnforceCodeStyleInBuild=true` newly flag gets dropped to
`suggestion` here, rule by rule, until the build is green. Fixing those diagnostics is Session 7's
job. The requirement from the session brief is that the suppression list be **legible as backlog
rather than as a permanent decision**, so the block has a fixed shape:

```ini
# ============================================================================
# ANALYZER BACKLOG — MASTER-078 / XCUT-010 — NOT A PERMANENT DECISION
#
# Session 1 raised AnalysisMode to Recommended and turned EnforceCodeStyleInBuild
# on. Every rule below fired on that first run and is parked at `suggestion` so
# the build stays green under TreatWarningsAsErrors. Each one is Session 7 work:
# fix the occurrences, then delete the line so the rule returns to its default
# severity. A line deleted from this block is a rule that is now enforced.
#
# Nothing may be added to this block after Session 1 without a new record.
# ============================================================================
[*.cs]
dotnet_diagnostic.IDE0055.severity = suggestion   # formatting; 62 files — owned by T3, see below
dotnet_diagnostic.CAxxxx.severity  = suggestion   # <n> occurrences — <one-line description>
# ...
# ============================================================================
# END ANALYZER BACKLOG
# ============================================================================
```

Rules binding on T2:

- Each line carries a trailing comment with the **occurrence count** and a short description. A bare
  severity line is not acceptable — the count is what tells Session 7 how big the item is.
- Only rules that **actually fired** may appear. Do not pre-emptively suppress rules that might fire.
- The block sits at the end of the file so a `grep` for `dotnet_diagnostic` finds nothing above it.
- T2 reports the full list of suppressed rules and their counts in its report, so Gate 3 records the
  size of the backlog this session created.

## 5. The one line T3 changes

`IDE0055` is the formatting rule. With `indent_style = tab` declared, it fires on 62 `.cs` files
still in the tree, and under `TreatWarningsAsErrors` at its default `warning` severity that is a
broken build. So T2 parks it at `suggestion` like the rest.

> **Amended 2026-08-17, after T2's review.** The counts in this section said 58; the measured
> figure is **62 files and roughly 14,160 locations**. See the amendment note in §6 for why the
> original number was both miscounted and derived from the wrong definition.

Once T3 has reformatted those 62 files, the reason for parking it is gone. **T3's final edit is to
change that single line**, from:

```ini
dotnet_diagnostic.IDE0055.severity = suggestion   # formatting; 62 files — owned by T3, see below
```

to:

```ini
dotnet_diagnostic.IDE0055.severity = warning      # raised by T3 once the 62 files were reformatted
```

and then to build. A clean build at 0 warnings with `IDE0055` at `warning` is the proof that the
reformat is complete and that `.editorconfig` is now enforced rather than advisory — which is the
outcome `XCUT-012` is actually after.

**T3 may change no other line of `.editorconfig`.** If reformatting reveals that some other rule
must move, that is an escalation, not an edit.

## 6. T3's file list — the 62 files

> **Amended 2026-08-17, after T2's review.** This section previously named 58 files and defined
> them as "every `.cs` file with at least one space-indented line and no tab-indented line". Both
> were wrong. The enumeration actually held **57** paths, not 58 — the count was arithmetic nobody
> checked, and the Tests sub-header said 45 over a list of 44. And the definition was the wrong
> one: `IDE0055` also fires on **intra-line column alignment**, which occurs in five files that are
> already 100% tab-indented and so were excluded by the old rule. Leaving them out made T3's
> acceptance criteria jointly unsatisfiable — the flip to `warning` in §5 would have left ~79
> diagnostics live in files T3 was forbidden to edit. The list below is the **measured** set:
> every file reporting at least one `IDE0055` diagnostic with the rule raised to `warning`.
> Unfrozen and widened on the user's decision at the escalation of 2026-08-17.

Every `.cs` file under `src/` that reports an `IDE0055` diagnostic. Excludes `bin/`, `obj/`, and
the five generated files under `src/DungeonMaster.Infrastructure/Migrations/`, which
`generated_code = true` exempts — verified empirically, not assumed. Paths relative to `src/`.

**DungeonMaster.App (10)**

```
DungeonMaster.App/Assets/ObjectAssets/ObjectAssetCatalogue.cs
DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs
DungeonMaster.App/Assets/ObjectAssets/ObjectPackRegistry.cs
DungeonMaster.App/Assets/ObjectAssets/PackManifest.cs
DungeonMaster.App/Behaviors/HandCursorBehavior.cs
DungeonMaster.App/Graphics/FloorMapDrawable.cs
DungeonMaster.App/Views/DungeonDetailPage/ControlsPaletteView.xaml.cs
DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs
DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs
DungeonMaster.App/Views/DungeonDetailPage/SecondaryPaletteView.xaml.cs
```

**DungeonMaster.Application (1)**

```
DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/PlacedObjectDto.cs
```

**DungeonMaster.Domain (5)**

```
DungeonMaster.Domain/Entities/Floor/ObjectKind.cs
DungeonMaster.Domain/Entities/Floor/ObjectRotation.cs
DungeonMaster.Domain/Entities/Floor/ObjectScale.cs
DungeonMaster.Domain/Entities/Floor/PlacedObject.cs
DungeonMaster.Domain/Entities/Floor/SnapAnchor.cs
```

**DungeonMaster.Infrastructure (2)**

```
DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs
DungeonMaster.Infrastructure/Persistence/Repositories/DungeonReadRepository.cs
```

**DungeonMaster.Tests (44)**

```
DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs
DungeonMaster.Tests/App/Theme/BindablePropertyDefaultsMatchTokensTests.cs
DungeonMaster.Tests/App/Theme/DmThemeKeysResolveTests.cs
DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs
DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs
DungeonMaster.Tests/App/Theme/ImplicitStyleTests.cs
DungeonMaster.Tests/App/Theme/RepoPaths.cs
DungeonMaster.Tests/App/Theme/ThemeDictionaryTests.cs
DungeonMaster.Tests/App/Theme/ThemeHygieneTests.cs
DungeonMaster.Tests/App/Theme/ThemeSource.cs
DungeonMaster.Tests/App/Theme/ThemeTokenParityTests.cs
DungeonMaster.Tests/App/Theme/TokenNamingTests.cs
DungeonMaster.Tests/App/Theme/TokenTypeMatchesTargetPropertyTests.cs
DungeonMaster.Tests/Application/Common/Map/MapDrawingSessionTests.cs
DungeonMaster.Tests/Application/Common/Map/SnapZoneCalculatorTests.cs
DungeonMaster.Tests/Application/Common/Map/TileCoordinateHelperTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddFloorCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddPlacedObjectCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/CreateDungeonCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/DeleteDungeonCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemoveAreaCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemoveFloorCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/RemovePlacedObjectCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/ReorderFloorCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateAreaCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateAreaOutlineCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateDungeonCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdateFloorCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonDetailQueryHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Queries/GetDungeonsQueryHandlerTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Validators/CreateDungeonCommandValidatorTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateAreaCommandValidatorTests.cs
DungeonMaster.Tests/Application/Features/Dungeons/Validators/UpdateDungeonCommandValidatorTests.cs
DungeonMaster.Tests/Domain/Entities/Area/AreaOutlineTests.cs
DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs
DungeonMaster.Tests/Domain/Entities/Floor/FloorObjectMethodsTests.cs
DungeonMaster.Tests/Domain/Entities/Floor/PlacedObjectTests.cs
DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs
DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs
DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs
DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs
```

The five files that carry alignment violations rather than space indentation, and so were
absent from the original list, are: `DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`
(68 locations), `DungeonMaster.App/Views/DungeonDetailPage/ControlsPaletteView.xaml.cs` (3),
`DungeonMaster.Domain/Entities/Floor/PlacedObject.cs` (4),
`DungeonMaster.Infrastructure/Persistence/Repositories/DungeonReadRepository.cs` (3), and
`DungeonMaster.App/Behaviors/HandCursorBehavior.cs` (1).

## 7. Rules T3 works under

- **Whitespace only.** *(Amended 2026-08-17 — this rule previously said "leading whitespace only",
  which excluded the intra-line column alignment `IDE0055` also flags. Widened on the user's
  decision at the escalation of 2026-08-17.)* Leading runs of 4 spaces become one tab, and
  intra-line alignment padding is normalised as `IDE0055` requires. Change nothing else: no line
  reflow, no brace insertion, no reordering, no line-ending change, no edit to any
  non-whitespace character. **BOM removal is permitted and expected** where `repository-root-config.md`
  §6's `utf-8`, no-BOM decision requires it — that decision is binding on T3, and the earlier
  "no BOM change" wording in this bullet contradicted it. Three files in the 62 carry a BOM and will
  lose it. A BOM is never *added*. The commit must remain semantically inert — that is the property this
  task exists to protect, and it is unchanged by the widening.
- **Do not run bare `dotnet format`.** It applies every style rule in `.editorconfig` and would mix
  semantic style changes into a commit whose entire value is that it is whitespace-only. Use
  `dotnet format whitespace`, which applies exactly the `IDE0055` fixes and nothing else, and
  verify with §8.
- **Scope the invocation to the 62 files with `--include`.** *(Added 2026-08-17.)* Unscoped over the
  solution, `dotnet format whitespace` touches **64** files: the 62 in §6 plus
  `DungeonMaster.App/Platforms/MacCatalyst/AppDelegate.cs` and
  `DungeonMaster.App/Platforms/MacCatalyst/Program.cs`, whose **only** change is a BOM strip. Those
  two are outside T3's ownership, so an unscoped run is a scope violation. Passing the 62 paths to
  `--include` confines the footprint exactly; measured against a `--verify-no-changes --report`
  dry-run, the 62 are covered completely, none is missed, and no file under `Migrations/` is touched.
- **The five migration files are not touched.** Section 7 of `.editorconfig` exempts them.
- **No `.xaml` file is touched.**
- Alignment inside multi-line block comments (` *` continuation lines) is left alone; the check in
  §8 excludes it deliberately.

> **Measured facts behind the two amendments above, from a read-only
> `dotnet format whitespace src/DungeonMaster.sln --verify-no-changes --report` dry-run on
> 2026-08-17.** Recorded because a round-2 review reached the opposite conclusion on the first of
> them from a `grep`-based measurement that MSYS line-ending normalisation had corrupted.
>
> - **Line endings do not change.** Every `.cs` file under `src/` is CRLF-only — 178 of 178,
>   counted on raw bytes — so `.editorconfig`'s `end_of_line = crlf` already matches the tree and
>   every replacement the report proposes emits `
`. The "no line-ending change" rule in this
>   section stands as written and is satisfied.
> - **Five files get a BOM stripped**, three of them inside the 62:
>   `DungeonDetailPage.xaml.cs`, `ThemeDictionaryTests.cs`, `ThemeTokenParityTests.cs`. The other
>   two are the MacCatalyst pair excluded by `--include`.
> - **Ten `.cs` files carry a BOM in total.** The formatter only rewrites files it is otherwise
>   changing, so five BOM-bearing files survive T3 untouched. That is a residual inconsistency with
>   `repository-root-config.md` §6, not a T3 defect: `XCUT-017` and `MASTER-150` scope the BOM
>   question to *build* files, all of which are already clean. Carried as an observation.

## 8. The verification command T3's acceptance rests on

Run from the repository root of the worktree:

```sh
grep -rEn "^ +[^ *]" --include=*.cs --exclude-dir=bin --exclude-dir=obj src \
  | grep -v "/Migrations/"
```

Expected output: nothing, exit status 1. Any line printed is a `.cs` file still carrying leading
spaces outside a block comment and outside the exempted migrations.

> **Amended 2026-08-17.** This grep is **necessary but no longer sufficient**: it detects leading
> spaces only, and the widened scope in §6 includes files whose violation is intra-line alignment,
> which this command cannot see. The authoritative check is the one in §5 — `IDE0055` at `warning`
> with `TreatWarningsAsErrors=true` and a build that reports 0 warnings and 0 errors. Run both; the
> grep localises a failure fast, the build is what proves completeness.
