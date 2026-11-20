# Code quality audit — DungeonMaster

Produce an evidence-cited, severity-ranked quality audit of all five projects, on two axes — per-file craft and per-layer architecture — measured against Microsoft/.NET guidance first, then general practice, then the project's own conventions.

## Context

The solution is a five-project clean-architecture .NET 10 MAUI application: `Domain` (20 files), `Application` (57), `Infrastructure` (14), `App` (42 `.cs` + 27 `.xaml`), `Tests` (45). CQRS runs on a hand-rolled dispatcher rather than MediatR, errors flow through a `Result`/`ErrorCategory` type rather than exceptions, validation is a FluentValidation pipeline behavior, and persistence is EF Core 10 over SQLite.

The baseline is already unusually strict: `Directory.Build.props` sets `Nullable`, `ImplicitUsings`, `LangVersion latest`, and `TreatWarningsAsErrors`, and the solution builds with 0 warnings. So the compiler has already taken everything it can take. What remains is the class of problem a compiler cannot see — design, boundaries, disposal and lifetime, testability, duplication, and consistency — which is exactly what this session is for.

Core functionality is not under review. A correctness risk noticed while reading is still a legitimate record; hunting for bugs is not the task.

## Definition of done

1. Seven documents exist under `docs/audit/`: `00-master-index.md`, `01-domain-application.md`, `02-infrastructure.md`, `03-app-shell-and-views.md`, `04-app-controls-and-theme.md`, `05-tests.md`, `06-cross-cutting.md`.
2. Every per-layer document conforms to `contracts/audit-record-format.md` — six sections in fixed order, every record a YAML block with eleven required keys plus **What. / Why it matters. / Suggested direction.**
3. Every record cites at least one `path:line` (or `path:start-end`) that resolves to a real file at the base commit. No record cites a file without a line number.
4. Every record names an authority from `microsoft` | `conventions` | `practice` | `judgment`, with the citation form that token requires — a fetched `learn.microsoft.com` URL, a `dm-conventions` path plus verbatim heading, a named source, or `n/a` with the record worded as belief.
5. Every record carries a severity (S1–S5, defined by consequence), an `effort` (XS/S/M/L, of the fix), and a `risk`, so follow-on sessions can be scoped from the document rather than by re-reading the code.
6. Every document's `## Method` states which `dm-conventions` documents were read, with line counts, and states explicitly that `backend/dotnet-format.md` was read **in full** before any style, naming, formatting, or XML-doc record was written. No style record contradicts a rule in that document.
7. Coverage is attested, not assumed: every document's `## Scope` reports the directories audited with file and line counts, and names anything excluded with a reason.
8. `00-master-index.md` conforms to `contracts/master-index-format.md`: all source records merged into a single ranked list, with `source record count − merges = master entry count` shown arithmetically, and every source record ID appearing in exactly one master entry.
9. `00-master-index.md` proposes 3–8 follow-on fix sessions in execution order, with every S1 and S2 entry assigned to exactly one session and no session ordered before one it is blocked by.
10. **Nothing under `src/` is created, modified, or deleted.** `git status --porcelain` shows no entry under `src/` on the session branch, and each task attests to this verbatim.

## Scope

All of `src/`, read-only, across six tasks:

| Task | Agent | Covers |
|---|---|---|
| T1 | backend | `Domain` + `Application` — entities, value objects, CQRS dispatcher, `Result` model, validation behavior |
| T2 | database | `Infrastructure` — `AppDbContext`, entity configurations, read/write repositories, factory, DI composition root, migrations |
| T3 | frontend | `App` root, `Views/`, `ViewModels/`, `Extensions/`, `Platforms/` — MVVM discipline, code-behind, Shell navigation, service lifetimes |
| T4 | frontend | `App` `Controls/`, `Converters/`, `Behaviors/`, `Graphics/`, `Theme/`, `Resources/Theme/` — bindable properties, converter contracts, leak safety, theme tokens |
| T5 | test | `Tests` — naming, xUnit patterns, FluentAssertions and NSubstitute usage, behaviour-vs-implementation, and the on-disk XAML hygiene sub-suite |
| T6 | architecture | `Directory.Build.props`, the five `.csproj`, the `.sln`, the absent `.editorconfig` and analyzer config, the layer *boundaries*, and the master synthesis |

T1–T5 run as one concurrent wave. T6 depends on all five.

### Not in scope

- **Any change to `src/`.** This session produces documents. Fixes are follow-on sessions, scoped from `00-master-index.md`.
- Functional correctness as a hunting ground — the user has stated core functionality is fine.
- Findings the compiler already enforces. The build is clean under `TreatWarningsAsErrors`; "the compiler would catch this" is unavailable as a record.
- Applying `dm-conventions`' React/TypeScript or Vitest/Playwright documents to MAUI or xUnit. They do not govern those stacks.
- Performance profiling, security penetration testing, and dependency CVE scanning beyond what is visible in the `.csproj` files.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Which conventions govern this repo? | `dm-conventions` `backend/dotnet-format.md`, `backend/backend-conventions.md`, `backend/dotnet-guidelines.md`, `architecture/architecture-decisions.md` | 1 | `C:\Users\dmarq\Development\Repos\dm-conventions\CONVENTIONS-INDEX.md` — tag rows `backend,dotnet,csharp` |
| Do the frontend/testing conventions apply? | No. Its frontend docs are React/TypeScript and its testing docs are Vitest/Playwright; this is MAUI and xUnit. For those layers the yardstick is Microsoft guidance plus general practice. | 1 | Same index — `frontend/react-format.md` tags `react,typescript`; `testing/vitest-guidelines.md` tags `vitest` |
| Which parts of `dotnet-guidelines.md` apply? | The async/await, EF Core, and performance sections. The ASP.NET Core `HttpContext` sections do not — there is no web host. | 1 | `CONVENTIONS-INDEX.md` description; no ASP.NET package reference in any `.csproj` |
| Is the build clean, and under what strictness? | Yes — 0 warnings, 0 errors, with `TreatWarningsAsErrors=true` solution-wide | 1 | `build_output.txt` final block; `Directory.Build.props` |
| Is there an `.editorconfig` or analyzer configuration? | No `.editorconfig` anywhere; no analyzer package reference and no `AnalysisLevel`/`EnableNETAnalyzers` property in any project | 1 | `find . -name .editorconfig` → empty; all five `.csproj` |
| Where do the audit documents live? | `docs/audit/`, numbered `00`–`06` for reading order | 1 | `docs/` already holds `TODO.md`, `icon-references.md`, `mockups/`; no competing convention in the repo |
| Does the App layer fit one task? | No — split on the seam between pages/MVVM/navigation (~5.5k lines) and the reusable presentation kit (~6.0k lines). Near-equal halves, and each half has a distinct yardstick. | 1 | Measured line counts per directory under `src/DungeonMaster.App` |
| Should Domain and Application split? | No — 77 files but tightly coupled through the `Result` type and the CQRS shapes; splitting would force one auditor to judge the other's contract | 1 | `Application/Common/Results/Result.cs` referenced throughout `Features/**/Handlers` |
| Which agent audits Infrastructure? | `dir-database-engineer` — 12 of 14 `.cs` files are EF Core persistence or migrations | 1 | File listing of `src/DungeonMaster.Infrastructure` |
| How is generated migration code judged? | Explicitly scoped in `## Scope` — either audited against a stated different standard for generated code, or excluded with the reason given. `DependencyInjection.cs` is judged as hand-written. | 4 | No repo policy on generated code exists; recorded as an assumption, enforced by T2 acceptance criterion 7 |
| Is the word "findings" usable in the deliverable? | No. The harness blocks writing `.md` files whose names read as report/summary/findings/analysis files. Documents are named by subject and the record section is `## Records`. | 1 | The architect's first `Write` of `contracts/findings-document.md` was rejected on that basis |
| What is the base branch? | `chore/repo-audit` @ `45495c1` | 1 | `git rev-parse --abbrev-ref HEAD` in the main checkout |

## Contracts

- **`contracts/audit-record-format.md`** — the per-layer document format shared by T1–T6. Fixes six layer codes (`CORE`, `INFRA`, `UI`, `UIKIT`, `TEST`, `XCUT`); gapless `<LAYER>-<NNN>` identity; a five-point severity scale defined by *consequence* rather than adjective, where S5 is forced to `authority: judgment`; a four-token authority enum with a citation form constrained per token; an eleven-key YAML block per record including `effort` and `risk` and a conditional `blocked-by`; and a six-section skeleton ending in a verbatim attestation.
- **`contracts/master-index-format.md`** — T6's synthesis format. Defines the merge test (two records collapse only when *the same fix* resolves both — not the same category, not the same file), field promotion on merge, rank ordering (severity, then effort, then risk), an eight-section skeleton, and a coverage attestation whose arithmetic must reconcile.

Both are frozen at Gate 2. Changing either afterwards requires an escalation.

## Open questions

None. Every unknown resolved at rungs 1–4; the escalation rule did not fire.
