# TEST hygiene sweep

Close the six TEST rows in `.sir/backlog.md` section "TEST six-row sweep: MASTER-126, 128, 137,
139, 143, 144" (backlog.md:5-12). This is structural cleanup of `src/DungeonMaster.Tests`, and
production behaviour does not change.

## Context

TEST is the layer with the most open rows, and this is the first grouped section of the backlog.
Each row was filed on 2026-08-17 by the code-quality audit and carries "re-measure before
scheduling". The discuss phase measured three things roughly at 2026-11-04T17:27Z. None of the
predicates had a control, so the spec phase re-measures all of them:

- `TheIconFontCmapParsesToPrivateUseGlyphs` is still one fact at
  `src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs:104` (MASTER-139).
- Run from `src/DungeonMaster.Tests`, `for f in $(git ls-files '*Tests.cs'); do grep -q "/// <summary>" "$f" || echo "$f"; done | wc -l` printed 18, out of 109 `*Tests.cs` files. The row cites 24 of 45 (MASTER-143).
- In `App/`, `git grep -nE "public (async Task|void) [A-Za-z0-9]+_[A-Za-z0-9_]+\(" -- 'App/*.cs' | wc -l` printed 58 underscore-named methods, and the same predicate without the underscore printed 90 (MASTER-144). Neither predicate is limited to `[Fact]`/`[Theory]` methods.
- `UpdatePlacedObjectCommandHandlerTests.cs` has 2 lines that match `floor.Objects.Single()\|\.Objects\.Should()` (MASTER-128).

Overlap that stays out of scope: the `ct` → `cancellationToken` rename in `DungeonMaster.Tests` belongs to MASTER-130 and backlog.md:39, not to MASTER-126's lambda-parameter rename.

## Decisions

- Scope: all six rows, in one session on the Standard track. A row the spec's re-measure shows to be stale or false is closed as such with its evidence, not worked.
- MASTER-126: rename the single-letter lambda parameters in the test project to descriptive names. Count the population with two independent predicates before deciding which sites to rename.
- MASTER-128: move the assertions out of Arrange blocks, as the row describes (`floor.Objects.Single()` in Arrange for `UpdatePlacedObjectCommandHandlerTests`, and a fact of its own for the diagnostic test's outline assertion).
- MASTER-137: apply the handler tests' Arrange/Act/Assert structure across the three test sub-trees that lack it. At minimum, blank lines separate the phases.
- MASTER-139: split the tests that have two or more independent reasons to fail. Move anti-vacuity assertions into facts of their own, and split `TheIconFontCmapParsesToPrivateUseGlyphs` into one fact per property of the parse.
- MASTER-143 (user decision at Gate 1): test classes are **exempt** from the XML-documentation rule. Record the exemption where the conventions live. Add no doc comments to test files for this row.
- MASTER-144 (user decision at Gate 1): reconcile **as the row says**. Keep the sentence form in `App/` and rename its underscore-separated hybrids to sentence form. Keep `Method_When_Then` elsewhere. State both schemes, and where each applies, somewhere a new test file's author will see it.
- Open for the spec phase: where "the conventions" are recorded for MASTER-143 and MASTER-144. The candidates are a repo-local doc and the external `dm-conventions` repository. Resolve this through `sir-inference`. A change outside this repository is an external-contract escalation.
- Each closed row is also updated in the audit register (`docs/audit/00-master-index.md`) and removed from `.sir/backlog.md` at Gate 3, as earlier sessions did.
