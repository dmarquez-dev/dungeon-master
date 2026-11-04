<!-- frozen: 20261104-1726-test-hygiene-sweep, Gate 2 -->

# C3 -- `src/DungeonMaster.Tests/README.md`: content requirements

No callable operations. The README is new, owned by T7, and is where a new test file's author reads
this project's test conventions (MASTER-143, MASTER-144, MASTER-137).

Placement evidence (orchestrator, `sir-inference` repo-evidence rung): the only test-project-local
convention document in the repository is `src/DungeonMaster.App.UiTests/README.md`; the external
`dm-conventions` repository is read-only to this session.

## 1. Form

- Plain Markdown, first line `# DungeonMaster.Tests`.
- Line endings LF throughout in the worktree: `tr -cd '\r' < src/DungeonMaster.Tests/README.md | wc -c`
  prints `0`. (The `Write` tool emits LF; `.gitattributes` is `* text=auto`, so git normalises on
  commit and a later checkout may present CRLF. That is git's doing and is out of this task's view.)
- No path under `.sir/` and no session id anywhere in the file
  (`grep -c '\.sir/' src/DungeonMaster.Tests/README.md` prints `0`). Register ids such as
  `MASTER-143` and audit ids such as `TEST-025` may be cited.
- **No count** of files, tests, methods or names, and no universal about the existing code ("every
  test in X already ..."). The README states rules, not measurements, so it cannot go stale by a
  file being added.

## 2. Required content -- naming (MASTER-144)

A section stating both schemes and the directory each applies to, consistent with C2 section 1:

- `App/` -- sentence form: a PascalCase declarative sentence stating the condition and the outcome,
  **with no underscore**. Say why: these facts assert over source files and the theme, and usually
  have no single method under test. One example name declared verbatim in `App/` at startRef with no
  underscore, so that no task renames it (for example
  `NoRawColourLiteralsOutsideTheThemeDictionaries` at `App/Theme/ThemeHygieneTests.cs:126`). T7 runs
  beside T1-T6, so an example taken from a renamed name would depend on a sibling.
- `AppCore/`, `Application/`, `Domain/`, `Infrastructure/` -- `Method_Scenario_ExpectedBehaviour`;
  the first part names the member or type under test; the scenario part is omitted when there is no
  distinguishing condition. One example name taken verbatim from one of those sub-trees.
- The rule is by directory: a new file follows the scheme of the directory it is created in.

Gate 2 chose Option A (C2 section 2): the `App/` rule has no exception for helper-method tests.

## 3. Required content -- XML documentation (MASTER-143)

A section stating that **test classes in this project are exempt** from `dm-conventions`
`backend/dotnet-format.md` section 9 ("All public types, members, and parameters must have XML
documentation comments."), quoting that sentence verbatim, and naming `MASTER-143` as the register
row that recorded the decision. It also states:

- documentation that already exists is kept and is welcome, especially a comment that explains why a
  fact exists or what it guards;
- non-test helper types in the project (fixtures, scanners, readers) are not covered by "test class"
  -- write the exemption so that it names test classes, and say nothing broader.

## 4. Required content -- Arrange/Act/Assert (MASTER-137)

A section stating the structure the handler tests use and this session applies to `AppCore/`,
`Domain/` and `Infrastructure/` (C1 section 4, rules 1-4), in prose:

- `// Arrange`, `// Act`, `// Assert` comment markers, in that order, each after the first preceded
  by a blank line;
- `// Arrange` is omitted when there is no setup;
- a body that is a single statement needs no markers;
- the `App/` scan facts are not bound by this section.

It may cite `dm-conventions` `backend/dotnet-format.md` section 12 as the source of the shape.

## 5. Nothing else is required

Further sections (how to run the tests, the anti-vacuity guard-fact naming used in `App/`) are
permitted provided they obey section 1.
