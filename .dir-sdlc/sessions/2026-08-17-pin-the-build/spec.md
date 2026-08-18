# Session 1 — Pin the build

Make the DungeonMaster solution's build reproducible and self-enforcing, changing no line of `src/` behaviour.

## Context

The code-quality audit in `docs/audit/` read all 205 source files and produced 167 records. Its verdict on the build was that it is "clean at zero warnings under `TreatWarningsAsErrors`, which is a demanding baseline honestly met — but nothing about the build is pinned, and no analyzer above the SDK default floor is running, which is why all 40 style records had to be found by people reading files."

`docs/audit/00-master-index.md` orders the remedy into seven fix sessions. This is the first, and it goes first for two reasons the index states: it is the only session that changes no `src/` behaviour, so it lands with zero regression risk; and it is enabling — `MASTER-132` decides the indent character Session 7 depends on, and `MASTER-078` turns on the enforcement that stops Session 7 from having to happen again.

Baseline on `chore/repo-audit` at `48a91f1`, measured at session start: `dotnet build src/DungeonMaster.sln` succeeds with **0 warnings, 0 errors**, building both `net10.0-windows10.0.19041.0` and `net10.0-maccatalyst`; `dotnet test` reports **292 passed, 0 failed**. Every "no regression" item below is measured against exactly that.

## Definition of done

Environment and restore pinned:

- [ ] `global.json` exists at the repository root; `dotnet --version` from the repository root prints `10.0.303`, proving the pin is honoured rather than merely present. *(MASTER-009)*
- [ ] `dotnet nuget list source` lists exactly one enabled source — `nuget.org` at `https://api.nuget.org/v3/index.json` — with no machine- or profile-level source leaking in. *(MASTER-072)*
- [ ] A `packages.lock.json` exists in each of the five project directories under `src/`, the App project's carries one section per target framework, and none of them is git-ignored. *(MASTER-072)*

Analyzer set pinned and style enforced:

- [ ] `AnalysisLevel` is `10.0`, `AnalysisMode` is `Recommended`, `EnforceCodeStyleInBuild` is `true`, and `LangVersion` is `14.0` rather than `latest` — all four verified by `dotnet msbuild -getProperty:`, not by reading the file. *(MASTER-009, MASTER-078)*
- [ ] Every rule the raised analysis mode fires is suppressed to `suggestion` inside a banner-delimited `ANALYZER BACKLOG` block in `.editorconfig`, each line carrying its occurrence count, and no rule appears there that did not actually fire. Fixing them is Session 7's job. *(MASTER-078)*

Packages centralised:

- [ ] No `PackageReference` in any of the five project files carries a `Version` or `VersionOverride` attribute; every version lives in `Directory.Packages.props` with `ManagePackageVersionsCentrally` and `CentralPackageTransitivePinningEnabled` set. *(MASTER-071)*
- [ ] Both EF Core packages are at `10.0.11`, and the `SQLitePCLRaw` security pin has been explicitly re-checked against `dotnet list package --include-transitive` — with the outcome stated either way. A silent carry-forward does not count. *(MASTER-043)*
- [ ] `Plugin.Maui.KeyListener` sits in `Directory.Packages.props` at `1.0.0-preview4` with a comment recording that no stable release exists and naming the removal condition. It is neither replaced nor removed. *(MASTER-104)*

Duplication and residue removed:

- [ ] No `.csproj` re-declares `Nullable` or `ImplicitUsings`; the effective values are unchanged, verified by `dotnet msbuild -getProperty:`. *(MASTER-129)*
- [ ] `src/DungeonMaster.sln` declares no `x64` or `x86` platform, and its configuration block is 20 lines rather than 60. *(MASTER-140)*
- [ ] No build file begins with a UTF-8 byte-order mark; `.gitattributes` exists at the repository root. *(MASTER-150)*

Indentation settled:

- [ ] `.editorconfig` exists at the repository root with `root = true` and `indent_style = tab` for `[*]` and `[*.cs]`. *(MASTER-132)*
- [ ] No `.cs` file under `src/`, outside the exempted EF migrations, carries a leading space. *(MASTER-132)*
- [ ] The reformat is whitespace-only: `git diff -w` over the 58 reformatted files is empty — no reflow, no brace insertion, no reordering, no line-ending change. *(MASTER-132)*
- [ ] `IDE0055` is at `warning`, and the build is clean with it there. This, rather than a reviewer's eye, is what proves the reformat is complete. *(MASTER-132)*

Nothing broke:

- [ ] `dotnet build src/DungeonMaster.sln` completes with **0 warnings and 0 errors** under `TreatWarningsAsErrors=true`, for both App target frameworks, at the end of every task.
- [ ] `dotnet test src/DungeonMaster.sln` reports **292 passed, 0 failed** at the end of every task.
- [ ] No `.cs` or `.xaml` file is changed by T1 or T2, and the only `.cs` changes anywhere in the session are whitespace.

## Scope

The ten entries the master index assigns to Session 1: `MASTER-009`, `MASTER-043`, `MASTER-071`, `MASTER-072`, `MASTER-078`, `MASTER-104`, `MASTER-129`, `MASTER-132`, `MASTER-140`, `MASTER-150`.

One addition outside the audit: `.gitignore` gains a `.claude-worktrees/` line, because the DIR worktree this session runs in otherwise shows as untracked in the main checkout.

Three tasks, fully serial — `T1 → T2 → T3`. The architect found no genuine parallelism and did not invent any: five of the ten entries touch `Directory.Build.props` and four touch the same five `.csproj`, so those merge into one task rather than passing six files back and forth through five contracts; and because every task's acceptance rests on a whole-solution build in one shared worktree, even disjoint globs would not make two tasks safely concurrent.

### Not in scope

- **Fixing the diagnostics `AnalysisMode=Recommended` surfaces.** They are suppressed to `suggestion` and left legible as backlog. Session 7 fixes them.
- **Replacing `Plugin.Maui.KeyListener`.** No stable release exists, so the audit's suggested direction has no path. The replacement question carries to Session 5, where the keyboard work and the macCatalyst decision already live.
- **Version bumps beyond EF Core and its two forced companions.** `CommunityToolkit.Maui` 14→15, `Microsoft.Maui.Controls` 10.0.41→10.0.90, `coverlet.collector` 6→10, `NSubstitute` 5→6 and `xunit.runner.visualstudio` 3→4 are all available and none is named by any Session 1 entry. Major bumps carry behaviour risk this session exists to avoid.
- **Lifting `TargetFramework` into `Directory.Build.props`.** See assumption 3 — the mechanism `XCUT-013` describes does not work as described.
- **Reformatting `.xaml` files or the five EF migrations.** `XCUT-012` censused `.cs` only and explicitly permits exempting generated migrations.
- **Converting the solution to `.slnx`.** `XCUT-014` raises it and says it is a separate call.
- **Any change to `src/` behaviour.** This is the property that earned the session its position.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Tabs or spaces? | Tabs | 1 | `dm-conventions/backend/dotnet-format.md` — every indentation instruction is denominated in tabs (§6 "+1 tab", lines 240-247 and 286-291; §9 "two tabs" at line 375); 299 of its 794 lines are tab-led. Microsoft's C# conventions page says four spaces but declines the authority role, calling itself "not an authoritative list". Argued in full at `docs/audit/06-cross-cutting.md#XCUT-012`. |
| Which SDK to pin? | `10.0.303` | 1 | `dotnet --list-sdks` — it is the only SDK installed. |
| `rollForward: latestPatch` or `disable`? | `latestPatch` | 4 | `XCUT-009` suggests `disable` once lock files are adopted. Not taken: `disable` makes the repository unbuildable on any other 10.0.3xx patch install, and `packages.lock.json` pins package versions, not the SDK — the two are not coupled. Counter-position recorded in `contracts/repository-root-config.md` §1. |
| Add the BOM everywhere, or strip it? | Strip — `charset = utf-8`, no BOM | 4 | `XCUT-017` offers both and chooses neither. Uniform, diff-friendly, and matches the two most recently hand-edited files. `contracts/repository-root-config.md` §6. |
| Should `TargetFramework` move to `Directory.Build.props`? | No | 1 | `XCUT-013` suggests it "leaving the App project's `TargetFrameworks` to override it", but `Directory.Build.props` is imported *before* the project body, so both properties end up non-empty and the multi-target build depends on evaluation order. The duplication the record is actually about — `Nullable`, `ImplicitUsings` — is removed in full. |
| Is any version bump beyond EF Core forced? | Two are | 1 | `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Logging.Debug` move `10.0.0 → 10.0.11`: EF Core 10.0.11 resolves them at 10.0.11 transitively, and under `CentralPackageTransitivePinningEnabled` a lower central version is an `NU1605` downgrade error. Both patch-level, same major. |
| How many `PackageReference` entries are there? | 16 | 1 | Direct count across the four project files — App 5, Tests 6, Infrastructure 3, Application 2. `XCUT-008` says fourteen; the discrepancy is in the audit. |
| Which files does the reformat touch? | 58 | 1 | Census in the worktree, listed file-by-file in `contracts/editorconfig.md` §6. The audit counted 62 space-indented `.cs` files; the architect's census found 63, and 5 EF migrations are exempted. The one-file difference is a classification edge case, not material. |
| Does a stable `Plugin.Maui.KeyListener` exist? | No | 3 | `dotnet package search Plugin.Maui.KeyListener --exact-match --prerelease` returns only `1.0.0-preview1` through `preview4`. |
| Is there CI to keep green? | No | 1 | No `.github/` directory and no CI configuration anywhere in the repository. Verification is local `dotnet` commands. |

## Contracts

- [`contracts/repository-root-config.md`](contracts/repository-root-config.md) — exact content of `global.json`, `nuget.config` and `.gitattributes`; the `.gitignore` edit; the solution reduction from 60 configuration lines to 20, with the five project GUIDs; and the repository-wide BOM/encoding decision, binding on all three tasks.
- [`contracts/msbuild-properties-and-packages.md`](contracts/msbuild-properties-and-packages.md) — the final `Directory.Build.props`; the `Directory.Packages.props` shape; the frozen 16-package version table with a disposition per package; the `SQLitePCLRaw` re-check procedure; the required `.csproj` shape after central package management; and the mandatory internal ordering of T2's edits (dedupe → BOM → CPM → analyzers → lock files), which is not optional because those edits interact.
- [`contracts/editorconfig.md`](contracts/editorconfig.md) — the eight `.editorconfig` sections in fixed order and which task writes which; the analyzer-backlog block format; the explicit list of the 58 files T3 reformats; and the single verification command T3's acceptance rests on.

`.editorconfig` deliberately appears in both T2's and T3's `owns`. They are strictly serialised and the contract fixes the boundary to one line: T2 authors the whole file and parks `IDE0055` at `suggestion`; T3 changes that one line to `warning` and nothing else.

## Open questions

| Question | Who resolves it | When |
|---|---|---|
| Does `CentralPackageTransitivePinningEnabled` survive the MAUI workload packs? It is known to interact badly with them, and `XCUT-008` names it as the mechanism the hand-rolled `SQLitePCLRaw` pin substitutes for. | T2's implementer, by running the restore. The contract **forbids** dropping the property quietly; if the App project will not restore, it escalates, with the reduced form (`ManagePackageVersionsCentrally` alone) as the ranked fallback. | During T2 |
| Is the `SQLitePCLRaw` 2.1.12 pin still needed once EF Core is at 10.0.11? The existing comment names exactly this as its removal condition. | T2's implementer, from `dotnet list package --include-transitive`. Either outcome is acceptable; leaving it undetermined is not. | During T2 |
| How large is the analyzer backlog `AnalysisMode=Recommended` produces? Unknown until it runs, and the build currently has no warning margin. | T2's implementer. Volume does not change the approach — it is suppressed to `suggestion` and counted — but a very large number is worth knowing at Gate 3. | During T2 |
