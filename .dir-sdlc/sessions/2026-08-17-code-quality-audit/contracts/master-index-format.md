# Contract: master index format

**Status:** frozen at Gate 2. Exactly one document conforms to this contract:
`docs/audit/00-master-index.md`, produced by T6. It is the document the user reads first and the
one from which follow-on fix sessions are scoped.

Its inputs are the six per-layer documents, each conforming to `audit-record-format.md`. This
contract defines how those are merged. Terminology is inherited from that contract: a **record** is
one audited observation; `CORE`, `INFRA`, `UI`, `UIKIT`, `TEST`, `XCUT` are the layer codes;
`S1`-`S5` is the severity scale; `effort` and `risk` carry the same meanings.

---

## 1. Master entry identity

Merged entries are numbered `MASTER-001`, `MASTER-002`, ... zero-padded to three digits, gapless,
assigned in final rank order so that `MASTER-001` is rank 1.

## 2. Merge rule

Two or more source records collapse into one master entry when, and only when, **the same fix
resolves all of them**. Same category and same authority is not sufficient; same file is not
sufficient. The test is the fix.

When records collapse:

- The master entry takes the **highest** severity among its sources (`S1` beats `S2`).
- It takes the **largest** effort among its sources.
- It takes the **highest** risk among its sources.
- Its `Layers` cell lists every distinct source layer code, comma-separated, in the section-1 order
  of `audit-record-format.md`.
- Its title is rewritten to describe the shared fix, not any one instance.

Records that do not collapse become one master entry each, inheriting all fields unchanged.

## 3. Ranking

Sort ascending by severity (`S1` first), then ascending by effort (`XS`, `S`, `M`, `L`), then
ascending by risk (`low`, `medium`, `high`), then ascending by first source ID. Cheap, safe fixes
therefore rise within their severity band -- the ordering is deliberately biased toward what a
maintainer can act on immediately.

## 4. Document skeleton

Sections appear in this order, with these exact headings. None may be omitted.

```markdown
# DungeonMaster solution -- code quality audit, master index

## Verdict
## Per-layer verdicts
## Boundaries
## Source documents
## Master index
## Merged groups
## Suggested fix sessions
## Coverage attestation
```

### `## Verdict`

300-600 words. The solution-level answer to both audit axes: is the codebase well built, and does
the architecture hold. State a position in the first sentence. This is the only section written for
a reader who will read nothing else.

### `## Per-layer verdicts`

One paragraph per layer, in section-1 code order, each 60-120 words, each headed by a bolded layer
name. Each paragraph must be T6's own judgment informed by the source document -- not a quotation of
that document's `## Layer verdict`.

### `## Boundaries`

The five layers' *boundaries*, as distinct from the layers themselves: Domain to Application,
Application to Infrastructure, Application/Infrastructure to App, and Tests' reach into each. State
for each pair whether the dependency direction, the surface area, and the types crossing it are
right. Boundary defects are recorded as `XCUT` records in `docs/audit/06-cross-cutting.md` and
appear in the master index like any other; this section is the prose assessment, not a second list.

### `## Source documents`

Exactly one table, these exact columns and this row order:

| Document | Layer | S1 | S2 | S3 | S4 | S5 | Total |
|---|---|---|---|---|---|---|---|
| `docs/audit/01-domain-application.md` | CORE | n | n | n | n | n | n |
| `docs/audit/02-infrastructure.md` | INFRA | n | n | n | n | n | n |
| `docs/audit/03-app-shell-and-views.md` | UI | n | n | n | n | n | n |
| `docs/audit/04-app-controls-and-theme.md` | UIKIT | n | n | n | n | n | n |
| `docs/audit/05-tests.md` | TEST | n | n | n | n | n | n |
| `docs/audit/06-cross-cutting.md` | XCUT | n | n | n | n | n | n |
| **All** | | n | n | n | n | n | n |

Each row's counts must equal that document's own `## Summary` table. Where they do not, T6 states
the discrepancy in `## Coverage attestation` rather than silently adjusting either number.

### `## Master index`

Exactly one table, these exact columns in this order:

| Rank | ID | Sev | Layers | Category | Effort | Risk | Title | Sources |
|---|---|---|---|---|---|---|---|---|

- `Rank` is `1..N`, gapless, matching the `ID` suffix.
- `Sources` lists every source record ID collapsed into the entry, comma-separated, in section-1
  layer order then numeric order. At least one.
- `Title` is a single line, no trailing period, max 100 characters.

This table is the whole of the section. Do not restate record prose here -- the source documents
hold it, and the `Sources` column is the link.

### `## Merged groups`

One bullet per master entry whose `Sources` has more than one ID:

```markdown
- **MASTER-014** (`CORE-003`, `UIKIT-021`, `TEST-009`) -- one sentence naming the shared fix and
  why the three are the same fix rather than three similar ones.
```

If nothing merged, the section says so in one line.

### `## Suggested fix sessions`

Proposed follow-on DIR sessions, in recommended execution order, each as:

```markdown
### Session <n> -- <name>

- **Entries:** MASTER-002, MASTER-005, MASTER-011
- **Combined effort:** <sum, expressed in the XS/S/M/L vocabulary or in days>
- **Highest risk:** <low | medium | high>
- **Rationale:** one or two sentences on why these belong together and why this session comes at
  this point in the order.
```

Rules:

- Every `S1` and `S2` master entry appears in exactly one session. `S3`-`S5` entries may be left
  unassigned; if any are, a closing bullet lists them and says why.
- A session groups entries by *shared blast radius* -- same files, same layer, or one fix that
  unblocks the others -- not by severity. Batching unrelated cheap fixes into a "quick wins"
  session is acceptable only when they are genuinely independent (`independent: true` on every
  source record).
- Respect `blocked-by`: a blocked entry never precedes its blocker's session.
- Between three and eight sessions. More than eight is not a plan; fewer than three is not a
  decomposition.

### `## Coverage attestation`

Verbatim wording, each line answered `yes` or with a stated exception:

```markdown
- Every record ID in every source document appears in exactly one master entry: yes
- Source record count <n> reconciles to master entry count <m> with <k> merges: yes
- Every S1 and S2 master entry is assigned to a suggested fix session: yes
- No file under `src/` was created, modified, or deleted by this task: yes
```

The second line must show the arithmetic with real numbers, and `n - k = m` must hold.

## 5. Encoding and mechanics

As `audit-record-format.md` section 7: UTF-8, LF, no BOM, no emoji, repo-relative forward-slash
paths, fenced blocks declare a language.
