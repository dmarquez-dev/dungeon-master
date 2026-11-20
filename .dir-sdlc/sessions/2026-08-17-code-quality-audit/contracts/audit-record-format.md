# Contract: audit record format

**Status:** frozen at Gate 2. Every per-layer audit task in session `2026-08-17-code-quality-audit`
produces exactly one document conforming to this contract. The synthesis task (T6) parses these
documents mechanically, so the structure below is normative, not advisory. Where this document says
"exactly", it means byte-for-byte on the literal parts.

This is a *format* contract. It constrains the shape of the deliverable, not the content of any
audit record.

Throughout, "record" means one audited observation -- one entry in the document's `## Records`
section.

---

## 1. Layer codes

Each task is assigned exactly one layer code. It is fixed by this contract; do not invent one.

| Code | Task | Files in scope |
|---|---|---|
| `CORE` | T1 | `src/DungeonMaster.Domain/**`, `src/DungeonMaster.Application/**` |
| `INFRA` | T2 | `src/DungeonMaster.Infrastructure/**` |
| `UI` | T3 | `src/DungeonMaster.App/` root files, `Views/**`, `ViewModels/**`, `Extensions/**`, `Platforms/**`, `Properties/**` |
| `UIKIT` | T4 | `src/DungeonMaster.App/Controls/**`, `Converters/**`, `Behaviors/**`, `Graphics/**`, `Theme/**`, `Resources/Theme/**` |
| `TEST` | T5 | `src/DungeonMaster.Tests/**` |
| `XCUT` | T6 | `src/DungeonMaster.sln`, `Directory.Build.props`, all five `.csproj`, the absent `.editorconfig`, the absent analyzer configuration, and the *boundaries between* layers |

## 2. Record identity

Every record has an ID of the exact form:

```text
<LAYER>-<NNN>
```

`<LAYER>` is the code from section 1. `<NNN>` is a zero-padded three-digit decimal, starting at
`001`, incrementing by one, with **no gaps and no reuse**. IDs are unique within the document and,
because the layer code prefixes them, unique across the session.

IDs are stable: once written, a record keeps its ID. If a record is withdrawn during authoring,
renumber so the sequence stays gapless -- the document as delivered is what freezes.

## 3. Severity scale

Severity is one of the five tokens below. The definition governs, not the adjective.

| Token | Name | Definition |
|---|---|---|
| `S1` | Critical | The code can produce incorrect user-visible behaviour, data loss or corruption, an unhandled crash, or a security / dependency vulnerability, on a path reachable in normal use. |
| `S2` | Major | No misbehaviour observed today, but the code violates an explicit Microsoft guideline or a documented `dm-conventions` rule in a way that will generate defects the next time the code is changed or extended. Also: layer-boundary violations, and disposal / async / threading misuse not currently reachable. |
| `S3` | Moderate | Design or structural weakness with no correctness consequence: duplication, dead code, an untestable seam, a missing or misplaced abstraction, a responsibility in the wrong type. Correct today, expensive tomorrow. |
| `S4` | Minor | Style, naming, formatting, XML-documentation, or file-organisation deviation from `dm-conventions` or the Microsoft naming / framework-design guidelines, with no behavioural consequence. |
| `S5` | Observation | No deviation from any authority. A note the maintainer may wish to act on. `S5` records **must** carry `authority: judgment`. |

The build is already clean under `TreatWarningsAsErrors=true`. "The compiler would catch this" is
therefore not available as a record at any severity.

## 4. Authority and citation

`authority` is exactly one of four tokens, and `citation` is constrained by it:

| `authority` | `citation` must be | Example |
|---|---|---|
| `microsoft` | A full `https://learn.microsoft.com/...` URL that was actually fetched. | `https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines` |
| `conventions` | `<path relative to the dm-conventions repo root>#<section heading, verbatim>` | `backend/dotnet-format.md#Parameter formatting` |
| `practice` | A named, checkable source: `<Author, Title>, <chapter or section>`, or a URL. | `Fowler, Refactoring 2e, ch. 3 "Duplicated Code"` |
| `judgment` | The literal string `n/a`. | `n/a` |

Rules:

1. A claim you could not confirm against a fetched Microsoft page, a read `dm-conventions` section,
   or a named source is **`judgment`**. Demote it and word the prose as belief ("this appears
   to...", "I believe...") -- never as fact. An uncited claim stated as fact is a defect in the
   document.
2. Priority order when more than one authority applies: Microsoft official guidance, then general
   software engineering practice, then `dm-conventions`. Cite the highest-priority authority that
   actually supports the record.
3. `dm-conventions` has **no MAUI/XAML document and no xUnit document** -- its frontend docs are
   React/TypeScript and its testing docs are Vitest/Playwright. For `UI`, `UIKIT`, and `TEST`, do
   not cite those; use `microsoft` or `practice`. This is a settled fact of the session, not
   something to re-investigate.
4. A style record that contradicts `backend/dotnet-format.md` is a defect in the document, not a
   record. Read that document in full before writing any `S4`.

## 5. Record block

Each record is a level-3 heading, immediately followed by a fenced `yaml` block, immediately
followed by three bolded paragraphs in fixed order. Nothing else goes between them. Worked example:

    ### CORE-007 -- Dispatcher resolves handlers per call without caching the closed generic type

    ```yaml
    id: CORE-007
    layer: CORE
    severity: S3
    category: performance
    authority: microsoft
    citation: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
    locations:
      - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:34-51
    effort: S
    risk: low
    independent: true
    ```

    **What.** One to three sentences stating the observation, factually, with no recommendation.

    **Why it matters.** The consequence, tied to the cited authority. Name the concrete cost.

    **Suggested direction.** The shape of a fix, one to three sentences. Not a patch -- this session
    produces written records only, and no follow-on session is bound by what you write here.

### YAML keys

All eleven keys are required, in the order shown, with no extras.

| Key | Type | Constraint |
|---|---|---|
| `id` | string | Matches section 2 and the heading. |
| `layer` | string | The task's layer code from section 1. Identical on every record in the document. |
| `severity` | string | `S1`, `S2`, `S3`, `S4`, or `S5`. |
| `category` | string | One of: `correctness`, `api-design`, `async`, `disposal`, `threading`, `boundary`, `testability`, `duplication`, `dead-code`, `dependency`, `structure`, `performance`, `security`, `accessibility`, `style`, `naming`, `docs`. |
| `authority` | string | `microsoft`, `conventions`, `practice`, or `judgment`, per section 4. |
| `citation` | string | Constrained by `authority`, per section 4. |
| `locations` | list of strings | At least one. Each is `<repo-relative path>:<line>` or `<repo-relative path>:<start>-<end>`. Paths are relative to the repository root and begin with `src/`, `docs/`, or a root file name. A record with no line number is not deliverable -- if it is genuinely file-wide, cite `:1`. |
| `effort` | string | `XS` (under 30 min), `S` (under 2 h), `M` (under 1 day), `L` (over 1 day). Estimates the *fix*, not the audit. |
| `risk` | string | `low`, `medium`, or `high` -- the risk that applying the fix changes behaviour unintentionally. |
| `independent` | boolean | `true` if the fix can land alone; `false` if it must land together with other fixes. |
| `blocked-by` | list of strings | Required **only when `independent: false`**; omit the key entirely otherwise. Lists the record IDs (any layer) the fix must land with. |

`blocked-by` is the single conditional key; when present it follows `independent`.

### Heading

```text
### <ID> -- <title>
```

The separator is a double hyphen surrounded by single spaces. The title is a single line, no
trailing period, stating the observation -- not the fix. Maximum 100 characters.

## 6. Document skeleton

Sections appear in this order, with these exact headings. No section may be omitted; a section with
nothing to report says so in one line.

```markdown
# <Layer name> -- code quality audit

## Scope
## Method
## Summary
## Records
## Layer verdict
## Attestation
```

### `## Scope`

A table of every directory or glob audited, with file count and line count, followed by a bulleted
list of anything inside the task's layer that was **not** audited, each with one clause of why.
Generated code (EF Core migrations, `*.Designer.cs`, `*.g.cs`, XAML source-generator output) is
either audited against a different standard than hand-written code, or excluded -- either way, say
which here.

### `## Method`

- The base commit SHA the audit was performed against.
- The `dm-conventions` documents read, by repo-relative path, each with the line count read. State
  explicitly whether `backend/dotnet-format.md` was read **in full**.
- The Microsoft Learn pages consulted, as a bulleted list of URLs.
- Any tooling run (all of it read-only).

### `## Summary`

Exactly one table, with these exact columns and this row order:

| Severity | Count |
|---|---|
| S1 | n |
| S2 | n |
| S3 | n |
| S4 | n |
| S5 | n |
| **Total** | n |

The total must equal the number of record blocks in `## Records`.

### `## Records`

Record blocks per section 5, in ascending ID order. Ascending ID order only -- do not group by
severity; T6 does the ranking.

### `## Layer verdict`

Prose, 150-400 words, answering both audit axes for this layer: does the layer hold its shape, and
are its internal seams in the right places. State a verdict, not a recap of the records. If the
layer's boundary with an adjacent layer is the problem, say so and note that T6 owns the boundary
record itself.

### `## Attestation`

Three lines, verbatim wording, each answered `yes` or with a statement of the exception:

```markdown
- No file under `src/` was created, modified, or deleted by this task: yes
- Every record cites at least one file path with a line number: yes
- Every record names its authority, and every non-`judgment` record carries a real citation: yes
```

## 7. Encoding and mechanics

- UTF-8, LF line endings, no BOM.
- Fenced code blocks declare a language (`yaml`, `csharp`, `xml`, `text`).
- Code excerpts are permitted inside the three prose paragraphs, but keep them under 15 lines. The
  reader has the repository; quote only what is load-bearing.
- No emoji.
- Paths are always repo-relative and always use forward slashes, on every platform.

## 8. What this contract does not constrain

The number of records, their content, the depth of any audit, and the wording of prose. Volume is
explicitly not the goal -- a ranked, actionable, deduplicated list is.
