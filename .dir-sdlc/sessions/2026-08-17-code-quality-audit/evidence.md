# Resolved evidence — 2026-08-17-code-quality-audit

Gathered by the orchestrator at rungs 1–2 of the dir-inference ladder. Do not re-derive these.

## Gate 1 decisions (from the user)

- **Deliverable: findings only.** No source file in `src/` may be modified. The audit produces
  written findings documents. Fixes become follow-on sessions.
- **Depth: everything, including style.** Full style/formatting/naming pass measured against
  `dm-conventions` and Microsoft .NET naming guidelines, *in addition to* substance
  (correctness risk, API design, disposal/async/threading, layer leakage, testability,
  dead/duplicated code).
- Yardstick priority: **Microsoft/.NET official guidance first**, then general software
  engineering practice, then the project's own documented conventions.
- Core functionality is out of scope. This is a quality audit, not a bug hunt — though a
  correctness risk found while reading is a legitimate finding.

## Repository facts

| Fact | Evidence |
|---|---|
| 5 projects, clean-architecture layering | `src/DungeonMaster.sln` |
| Domain: 20 `.cs`, ~1,260 lines, zero package refs, zero project refs | `src/DungeonMaster.Domain/DungeonMaster.Domain.csproj` |
| Application: 57 `.cs`, ~1,992 lines; refs Domain only; FluentValidation 12.1.1, MS.Extensions.DI.Abstractions 10.0.0 | `src/DungeonMaster.Application/DungeonMaster.Application.csproj` |
| Infrastructure: 14 `.cs`, ~1,366 lines; refs Domain + Application; EF Core 10.0.7 Sqlite, transitive pin on SQLitePCLRaw.lib.e_sqlite3 2.1.12 for GHSA-2m69-gcr7-jv3q | `src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj` |
| App: .NET MAUI 10, `net10.0-windows10.0.19041.0;net10.0-maccatalyst`; 42 `.cs` + 27 `.xaml`, ~11,723 lines; refs Application + Infrastructure | `src/DungeonMaster.App/DungeonMaster.App.csproj` |
| App packages: CommunityToolkit.Maui 14.1.0, CommunityToolkit.Mvvm 8.4.2, Microsoft.Maui.Controls 10.0.41, Microsoft.Extensions.Logging.Debug 10.0.0, Plugin.Maui.KeyListener 1.0.0-preview4 | same |
| `MauiXamlInflator=SourceGen` is enabled | same |
| Tests: 45 `.cs`, ~7,548 lines; xUnit 2.9.3, FluentAssertions 8.9.0, NSubstitute 5.3.0, coverlet 6.0.4; refs Domain + Application + Infrastructure | `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` |
| Tests do **not** reference App; App-layer tests read XAML/CS as text from disk | `src/DungeonMaster.Tests/App/Theme/RepoPaths.cs`, `ThemeSource.cs` |
| `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`, `LangVersion=latest` solution-wide | `Directory.Build.props` |
| **No `.editorconfig` anywhere in the repo** | `find . -name .editorconfig` returns nothing |
| **No analyzer package references** in any project; no `AnalysisLevel`/`EnableNETAnalyzers` property set | all five `.csproj`, `Directory.Build.props` |
| Build is clean: `Build succeeded. 0 Warning(s) 0 Error(s)` under `TreatWarningsAsErrors` | `build_output.txt` |
| Architecture is CQRS with a **hand-rolled dispatcher** (no MediatR): `Common/Dispatch/{IDispatcher,Dispatcher,ICommand,ICommandHandler,IQuery,IQueryHandler,IPipelineBehavior}` | `src/DungeonMaster.Application/Common/Dispatch/` |
| Error handling is a `Result` type with `ErrorCategory`, not exceptions | `src/DungeonMaster.Application/Common/Results/` |
| Validation is a `ValidationBehavior` pipeline behavior over FluentValidation | `src/DungeonMaster.Application/Common/Behaviors/ValidationBehavior.cs` |
| Entities use a partial split: `X.cs` + `XMethods.cs` | `Domain/Entities/{Area,Dungeon,Floor}/` |
| Base branch | `chore/repo-audit`, HEAD `45495c1` |

## Project conventions — MUST be read by every auditor

Located at `C:\Users\dmarq\Development\Repos\dm-conventions`. Applicable docs:

- `backend/dotnet-format.md` (794 lines) — C# style: naming, parameter formatting, inheritance
  indentation, XML docs, CQRS shapes. **Read this in full before making any style or formatting
  finding.** A style finding that contradicts this document is a defect, not a finding.
- `backend/backend-conventions.md` (251 lines) — CQRS, entities, value objects, policies, DI,
  error handling.
- `backend/dotnet-guidelines.md` (532 lines) — async/await, EF Core, performance. Written for
  ASP.NET Core; the async/EF Core/performance sections apply, the HttpContext sections do not.
- `architecture/architecture-decisions.md` (237 lines) — ADR format.

**Convention gap, already established — do not re-investigate:** `dm-conventions` has no MAUI/XAML
document (its frontend docs are React/TypeScript) and no xUnit document (its testing docs are
Vitest/Playwright). For the App and Tests layers, the yardstick is therefore Microsoft's own
guidance (.NET MAUI docs, .NET naming/framework design guidelines, xUnit guidance) plus general
practice. Do not apply React or Vitest conventions to MAUI or xUnit.

## Standing constraints for every task in this session

1. **Read-only on `src/`.** No agent modifies, creates, or deletes any file under `src/`.
   The only writes are the findings documents each task owns.
2. Every finding cites **file path + line number** and names its **authority** — a Microsoft
   docs URL, a `dm-conventions` file and section, or explicit `judgment`.
3. An uncited claim stated as fact is a defect. Demote to `judgment` and word it as belief.
4. Findings are **severity-ranked** and **deduplicated**. Volume is not the goal; a ranked,
   actionable list is.
5. The build is already clean under `TreatWarningsAsErrors`. "The compiler would catch this"
   is not available as a finding — if it were catchable, it is already caught.
