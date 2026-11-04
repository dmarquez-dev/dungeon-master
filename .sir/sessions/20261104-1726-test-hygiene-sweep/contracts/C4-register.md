<!-- frozen: 20261104-1726-test-hygiene-sweep, Gate 2 -->

# C4 -- Register updates: six status cells and six amendment blocks

No callable operations. Owned by T8, which runs last.

## 1. Status cells, `docs/audit/00-master-index.md`

Rows at startRef (each begins `| <n> | MASTER-<n> |`, each has 11 unescaped `|`, C1 section 7):
`:314` MASTER-126, `:316` MASTER-128, `:325` MASTER-137, `:327` MASTER-139, `:331` MASTER-143,
`:332` MASTER-144. Each last (status) cell is empty at startRef (the row ends `| |`).

Each status cell becomes:

```
**done** (20261104-1726-test-hygiene-sweep, 2026-11-04) -- <body>
```

so that, with `*` stripped, the cell's first token is `done` (register-classifier lesson,
`.sir/lessons.md:43`). The body states, for its row:

- what changed, bounded to the population this session worked (no claim beyond it);
- every figure with the command that produced it, measured by T8 on the post-change worktree, or
  attributed by name where T8 cannot measure it (git history);
- a pointer to its `AMENDED 20261104-1726-test-hygiene-sweep` block in `docs/audit/05-tests.md`;
- the sentence: no commit hash is cited, because tasks run without git and the orchestrator makes the
  commit.

Per row, the body must carry:

| Row | Must state |
| --- | --- |
| MASTER-126 | the row's "thirteen" is **7** at startRef (C1 section 1), DRIFT: four validator classes were consolidated into `NameOnlyValidatorTestBase`; P1 and P2 (C1 section 1) each print nothing over `src/DungeonMaster.Tests` post-change; names used `failure`, `floor`, `summary`. |
| MASTER-128 | the `UpdatePlacedObjectCommandHandlerTests` half is done (PA, C1 section 2, prints nothing post-change); the diagnostic-test half is **STALE**: `AreaPersistenceDiagnosticTests.cs` was deleted in `cf32061` (2026-09-08), attributed to the architect's `git log --diff-filter=D` at spec time. |
| MASTER-137 | the population was the 205 methods `aaa.pl` (C1 section 4) flagged in `AppCore/`, `Domain/`, `Infrastructure/` at startRef; post-change it prints nothing over each of the three; `Application/Common/Map/` is now `AppCore/Map/` (DRIFT); `App/` was excluded and why; the README states the rule. |
| MASTER-139 | the population was the five facts the row names (C5 section 1), with their current locations; the split each received; the suite grew by 9 (943 -> 952); and the explicit statement that no claim is made about any other fact. |
| MASTER-143 | closed by **decision**: test classes are exempt from `dotnet-format.md` section 9; the exemption is recorded in `src/DungeonMaster.Tests/README.md`; no doc comment was added or removed to close this row (amendment A5, escalation E3: the MASTER-139 split facts did add `/// <summary>` blocks, disclosed with their count and predicate); the row's "24 of 45" is now **18 of 109** by DOC (C1 section 5), re-measured by T8. |
| MASTER-144 | the two schemes and their directories; that every underscore name in `App/` (58 at startRef) was renamed (Option A, chosen at Gate 2) and that the three sentence names outside `App/` were renamed (C2 section 5); N1 and N2 (C1 section 3) post-change; the README states the rule; the comment-only reference updates outside `src/DungeonMaster.Tests` (C1 section 10). |

**Escaping.** A literal `|` inside a cell is written `\|`. A cell **cites** an instrument by its C1
section and does not quote it, so no instrument text (several contain `|`) enters a cell. The
sequence `\|` meaning a literal backslash-pipe cannot occur in these cells; T8's criterion checks the
cell count R (C1 section 7) is still 11 on each edited row. A cell may cite
`.sir/sessions/20261104-1726-test-hygiene-sweep/contracts/C1-measurement.md` by path (precedent:
MASTER-180 at `:368`); the README may not.

No other line of `00-master-index.md` changes; the file keeps 691 lines.

## 2. Amendment blocks, `docs/audit/05-tests.md`

One block per entry, inserted after the entry's `**Suggested direction.**` paragraph and before the
next `### TEST-0` heading. Anchors at startRef (`grep -n`):

| Entry | Heading | Suggested direction | Next heading |
| --- | --- | --- | --- |
| TEST-022 (MASTER-137) | `:1608` | `:1641` | `:1645` TEST-023 |
| TEST-023 (MASTER-126) | `:1645` | `:1685` | `:1688` TEST-024 |
| TEST-025 (MASTER-143) | `:1727` | `:1761` | `:1767` TEST-026 |
| TEST-027 (MASTER-144) | `:1835` | `:1871` | `:1875` TEST-028 |
| TEST-029 (MASTER-139) | `:1917` | `:1951` | `:1955` TEST-030 |
| TEST-030 (MASTER-128) | `:1955` | `:1986` | `:1990` TEST-031 |

Form (house form, e.g. `:88`, `:308`): the first line begins
`**[AMENDED 20261104-1726-test-hygiene-sweep, 2026-11-04 -- ` and the last line ends `**]**`. The
block states that the row is CLOSED and how, and corrects each stale claim of its entry with a
**Classification** (`DRIFT` where the code moved after the audit, `MEASUREMENT ERROR` where the claim
was wrong when written, `UNDETERMINED` where the audit-date state cannot be known without git), quoting
the corrected words verbatim. Corrections each block must carry at minimum:

- TEST-022: `Application/Common/Map/` -> `AppCore/Map/` (DRIFT); the population worked (C1 section 4).
- TEST-023: "thirteen" -> 7 at startRef, and the four validator `:37`/`:52` locations ->
  `NameOnlyValidatorTestBase.cs:65`/`:80` (DRIFT); `DungeonAggregateTests.cs:29/43/68` -> `:32/46/71`,
  `DungeonReadRepositoryTests.cs:51/52` -> `:29/30` at startRef (DRIFT -- or UNDETERMINED if T8 cannot
  establish the audit-date state; say which).
- TEST-025: "21 of 45 / 24" -> 18 of 109 at T8's measurement; the decision (exempt) and where it is
  recorded.
- TEST-027: the decision (C2 section 1) and that Gate 2 chose Option A (all 58 `App/` underscore names renamed).
- TEST-029: `Application/Common/Map/MapDrawingSessionTests.cs` -> `AppCore/Map/` (DRIFT); that
  `EveryDmFontIconTextIsInTheIconFontsCmap` already ran its guard before its offenders assertion at
  startRef, so the "ordering inverts the diagnosis" clause no longer described it (DRIFT), while
  `EveryIconOnlyControlHasASemanticDescription` still ran it after (true at startRef).
- TEST-030: `AreaPersistenceDiagnosticTests.cs` deleted in `cf32061` (attributed); `:186` -> `:195`
  for the second site and the method names as they stood at startRef
  (`HandleAsync_WithUnknownObjectId_ReturnsNotFoundAndDoesNotSave`,
  `HandleAsync_WithCustomScale_StoresScaleOnObject`) (DRIFT or UNDETERMINED, stated).

Nothing in any entry is deleted: the diff to `05-tests.md` is insertions only. Opener and closer
censuses (C1 section 7) each print 44 (38 at startRef).
