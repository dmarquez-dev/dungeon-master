# C1 -- Measurement instruments, startRef baselines, build rules and editing hazards

This contract declares no callable operation. It exists so that every instrument a criterion names is
available verbatim to every later phase; scratch copies made in the spec phase do not survive it.

Every baseline below was measured by the architect in the spec phase on the session worktree
`C:/dm-wt/20261109-1641-part-done-rows` at `91bbbf4d323286e912c2239137e0444159646980` (startRef for T1).
Paths are relative to the worktree root. Every grep runs **from the worktree root** with
`--exclude-dir=obj --exclude-dir=bin` (`--exclude-dir=obj` excludes build directories named `obj`; it does not
hide the identifier `obj` inside a `.cs` file -- the fixture control in section 4 shows both).

## 0. How to run these

- Save any script into **your own scratch directory** and run it from there; never save one into the worktree.
  Run Python as `python -I` and hand it `C:/...` paths, never `/c/...` (`.sir/lessons.md:52`).
- Fixture controls run on files in your own scratch directory, never in the worktree.
- `sha256sum` prints the digest followed by ` *-` or ` *<path>`; compare the 64 hex digits only.
- Before trusting an empty, zero or unchanged result, re-run the instrument's control and confirm the stated
  output. A result from an instrument whose control was not re-run is not evidence.
- **`sed`, `awk` and `grep -E` with a file argument strip CR on this machine** (`.sir/lessons.md:22`). Every
  content digest below therefore starts with `tr -d '\r' <`, so CR handling is explicit and identical for every
  reader. Line endings are judged only by B (section 1).
- Never combine `grep -i` with `-F` (`.sir/lessons.md:33`); never use `grep -P` (it can print 0 while erroring).

## 1. B -- line-ending byte census

```
od -An -tx1 -v <file> | tr -s ' ' '\n' | awk 'NF{ if($1=="0d")cr++; if($1=="0a"){lf++; if(p=="0d")pair++} p=$1 } END{printf "CR=%d LF=%d CRLF=%d loneCR=%d\n",cr,lf,pair,cr-pair}'
```

Control (run in the spec phase): `printf 'a\r\nb\rc\n' > <scratch>/b.txt`, then B prints
`CR=2 LF=2 CRLF=1 loneCR=1`. A file passes when CR = LF = CRLF and loneCR = 0. BOM check:
`head -c 3 <file> | od -An -tx1` (` 75 73 69` = `using`, ` 23 20 44` = `# D`).

startRef: every file below is CRLF with **loneCR=0** and CR = LF = CRLF = the line count shown. A rename changes
no line count, so each value is also the required post-state.

| File | lines | first 3 bytes |
| --- | --- | --- |
| `src/DungeonMaster.App/Assets/ObjectAssets/ObjectPackRegistry.cs` | 58 | `75 73 69` |
| `src/DungeonMaster.Tests/Application/Common/Dispatch/DispatcherCancellationTests.cs` | 478 | `75 73 69` |
| `src/DungeonMaster.Tests/Application/Common/Dispatch/DispatcherTests.cs` | 402 | `75 73 69` |
| `src/DungeonMaster.Tests/Application/Common/Handlers/CommandHandlerTestBase.cs` | 94 | `75 73 69` |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddAreaCommandHandlerTests.cs` | 96 | `75 73 69` |
| `.../Commands/AddFloorCommandHandlerTests.cs` | 59 | `75 73 69` |
| `.../Commands/AddPlacedObjectCommandHandlerTests.cs` | 201 | `75 73 69` |
| `.../Commands/CreateDungeonCommandHandlerTests.cs` | 143 | `75 73 69` |
| `.../Commands/DeleteDungeonCommandHandlerTests.cs` | 155 | `75 73 69` |
| `.../Commands/RemoveAreaCommandHandlerTests.cs` | 103 | `75 73 69` |
| `.../Commands/RemoveFloorCommandHandlerTests.cs` | 96 | `75 73 69` |
| `.../Commands/RemovePlacedObjectCommandHandlerTests.cs` | 174 | `75 73 69` |
| `.../Commands/ReorderFloorCommandHandlerTests.cs` | 145 | `75 73 69` |
| `.../Commands/UpdateAreaCommandHandlerTests.cs` | 108 | `75 73 69` |
| `.../Commands/UpdateAreaOutlineCommandHandlerTests.cs` | 121 | `75 73 69` |
| `.../Commands/UpdateDungeonCommandHandlerTests.cs` | 62 | `75 73 69` |
| `.../Commands/UpdateFloorCommandHandlerTests.cs` | 132 | `75 73 69` |
| `.../Commands/UpdatePlacedObjectCommandHandlerTests.cs` | 236 | `75 73 69` |
| `src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs` (not edited) | 499 | `75 73 69` |
| `src/DungeonMaster.Infrastructure/Persistence/Configurations/FloorConfiguration.cs` | 141 | `75 73 69` |
| `src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs` | 74 | `75 73 69` |
| `src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonReadRepository.cs` | 146 | `75 73 69` |
| `docs/audit/00-master-index.md` | 691 | `23 20 44` |
| `docs/audit/01-domain-application.md` | 3214 | `23 20 44` |

`.../Commands/` abbreviates `src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/`.

## 2. Editing hazards (binding on every task)

1. **Every target file is CRLF** (section 1). After your last edit, B on each file you touched prints
   CR = LF = CRLF = its section 1 line count (for the two audit files: plus the lines T3 inserted) and loneCR=0,
   and its first 3 bytes are unchanged.
2. **Never use `sed -i`, and never use the Write tool on an existing file.** Both rewrite a whole CRLF file to LF
   (`.sir/lessons.md:35-36`). Use the Edit tool. No task in this session creates a file in the worktree.
3. **The Edit tool strips trailing whitespace from `new_string`**, spaces as well as tabs (`.sir/lessons.md:38`,
   `:51`). Key every edit on a line's first non-whitespace text and never end a `new_string` on whitespace;
   leading tabs survive.
4. **The Edit tool normalises every embedded lone CR anywhere in the file** (`.sir/lessons.md:28`). Run B before
   your first Edit of a file; if loneCR is not 0, stop and report rather than edit. Every file in section 1 has
   loneCR=0 at startRef.
5. **Indent `.cs` files with tabs.** A rename changes no indentation; do not re-indent or reflow anything.
6. **No commit hash and no placeholder for one** in any register cell or block (`MASTER-051`,
   `docs/audit/00-master-index.md:239`). Tasks have no git; the orchestrator commits.
7. **Anchor every citation on the line and what is on it**, never on a derived count.

## 3. Build and test rules

- **This toolchain cannot build concurrently in one worktree.** Every project writes its `obj/` and `bin/` inside
  the worktree, which no `owns` glob covers; two `dotnet build`s would contend for them. The manifest therefore
  orders the two tasks that run `dotnet` (T1, then T2) in one chain. T3 runs no `dotnet`.
- Solution build: `dotnet build C:/dm-wt/20261109-1641-part-done-rows/src/DungeonMaster.sln`. startRef:
  `Build succeeded.` `0 Warning(s)` `0 Error(s)` (orchestrator probe, `state.json` `verification[0]`). Judge from
  those summary lines, never from `grep -c warning` over the output, which is always 2 for `dotnet build`
  (`.sir/lessons.md:23`).
- Full suite: `dotnet test C:/dm-wt/20261109-1641-part-done-rows/src/DungeonMaster.sln --no-build` after the
  build. startRef (orchestrator probe, `state.json` `verification[1]`): `DungeonMaster.App.UnitTests` Passed 217,
  `DungeonMaster.Tests` Passed 976, `DungeonMaster.App.Tests` Passed 137, each Failed 0 and Skipped 0. A
  rename adds and removes no test, so these three totals are also the required post-state. A run that prints no
  test count is not a pass (`.sir/lessons.md:45`).
- **No stale `<param>` tag can fail the build.** `GenerateDocumentationFile` is set nowhere:
  `grep -rn GenerateDocumentationFile --include=*.props --include=*.csproj --include=*.targets .` from the
  worktree root, filtered of `/obj/` paths, prints 0 lines at startRef (`.sir/lessons.md:61` records the
  consequence). TAG (section 4.2) is therefore the only instrument for tags; the build guards code uses only.
- EF model: `dotnet ef migrations has-pending-model-changes --project C:/dm-wt/20261109-1641-part-done-rows/src/DungeonMaster.Infrastructure --no-build`
  after a build of the Infrastructure project. startRef (orchestrator probe, `state.json` `verification[2]`):
  prints `No changes have been made to the model since the last migration.` and exits 0. It has not been seen
  failing; T2's J1 is its must-fail control.
- Narrower commands for an injection: `dotnet build C:/dm-wt/20261109-1641-part-done-rows/src/DungeonMaster.Tests/DungeonMaster.Tests.csproj`
  and `dotnet build C:/dm-wt/20261109-1641-part-done-rows/src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj`.

## 4. Source instruments

### 4.0 Fixture control (run in the spec phase; recreate it in your scratch directory to re-run)

A scratch `Fixture.cs` (CRLF) holding these ten lines, plus an identical copy under a scratch subdirectory
named `obj/`:

```
	Task X(CancellationToken ct)
			ct);
	F(s, cancellationToken: ct);
	Task Y(CancellationToken cancellationToken)
	using var cts = new();
	var ctx = 1;
	// actual
	var o = obj => obj.Id;
	var p = placedObject => placedObject.Id;
	var q = $"{s}obj{s}";
```

Results over the fixture directory with the section 4.1 / 4.3 flags: CT-WORD prints lines 1, 2, 3 only (the
near-misses `cancellationToken`, `cts`, `ctx`, `actual` do not match, and the `obj/` copy is not counted);
OBJ-WORD prints lines 8 and 10 (line 10 is a string literal, which is why OBJ-WORD is never run over a
population holding path strings -- section 4.3); OBJ-HEAD prints line 8 only. Add the line
`	/// <param name="ct">` to re-run TAG's control: it then prints 1, and `<param name="cancellationToken">` lines
are not matched.

### 4.1 `ct` -- parameter declarations and every word use

- **CT-DECL:** `grep -rnwE "CancellationToken ct" src/DungeonMaster.Tests src/DungeonMaster.App --include=*.cs --exclude-dir=obj --exclude-dir=bin`
  **startRef: 28 lines in 18 files** (`grep -rlwE` with the same arguments prints 18; 17 under
  `src/DungeonMaster.Tests`, 1 `ObjectPackRegistry.cs`). Required after T1: **0**.
- **CT-WORD:** `grep -rnw ct src --include=*.cs --include=*.xaml --exclude-dir=obj --exclude-dir=bin`
  **startRef: 63 lines in 19 files**: the 28 CT-DECL lines; the 15 TAG lines; `ReceivedToken = ct;` at
  `DispatcherCancellationTests.cs:394`, `:419`, `:444`, `:473`; one `ct);` call-argument line in each of the
  fourteen `*CommandHandlerTests.cs` files; `ObjectPackRegistry.cs:17`
  (`... DeserializeAsync<PackManifest>(stream, cancellationToken: ct);`, a named argument whose label is already
  `cancellationToken`); and `src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs:151`,
  `// Created once and restarted on every keystroke instead of a Task.Delay(ct) per keystroke:`. No line
  outside `src/DungeonMaster.Tests` and `src/DungeonMaster.App` matches. **Required after T1: exactly 1 line,
  `NotebookView.xaml.cs:151`.**
- **NotebookView is out of scope, deliberately.** Its `ct` is prose naming the argument of a rejected
  `Task.Delay` alternative in a comment, not a parameter, a local or any identifier; MASTER-130's EXCLUSION 2
  counts declaration sites, and this line is not one. It is the one permitted residue. The file is in no task's
  ownership: after T1, `tr -d '\r' < src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs | sha256sum`
  prints its startRef value `cfc65e9c2e1f97172b3c671490530657246230f504fbfe7346d13b1db0725131`.
- **Clash check (startRef):** of the 18 files T1 owns, only `ObjectPackRegistry.cs` contains the word
  `cancellationToken` (1 line, the named-argument label at `:17`); `grep -cw cancellationToken` prints 0 on the
  other 17. No rename can collide with an existing identifier.
- **CT-NEW:** `grep -rnwE "CancellationToken cancellationToken" src/DungeonMaster.Tests src/DungeonMaster.App --include=*.cs --exclude-dir=obj --exclude-dir=bin`
  **startRef: 0.** Required after T1: **28**, one per startRef CT-DECL line, in the same 18 files.

### 4.2 TAG -- `<param>` tags naming the token

- **TAG:** `grep -rnF '<param name="ct">' src --include=*.cs --exclude-dir=obj --exclude-dir=bin` **startRef: 15
  lines**, one in `CommandHandlerTestBase.cs` (`:56`, documenting the abstract `InvokeWithTokenAsync`) and one in
  each of the fourteen `*CommandHandlerTests.cs` files (documenting its `InvokeWithTokenAsync` override). Required
  after T1: **0**.
- **TAG-NEW:** `grep -rnF '<param name="cancellationToken">' src/DungeonMaster.Tests src/DungeonMaster.App --include=*.cs --exclude-dir=obj --exclude-dir=bin`
  **startRef: 0.** Required after T1: **15**, in the same 15 files.
- Second predicate, by pairing: in each of those 15 files, after T1, `grep -cF '<param name="cancellationToken">'`
  prints 1 and `grep -cwE "CancellationToken cancellationToken"` prints 1, so each tag documents a live parameter
  of that name in the same file.

### 4.3 `obj` -- the recorded population and its lambda heads

- **OBJ-WORD:** `grep -rnw obj src/DungeonMaster.Application src/DungeonMaster.Infrastructure src/DungeonMaster.Domain --include=*.cs --exclude-dir=obj --exclude-dir=bin`
  This is the instrument of `MASTER-130`'s EXCLUSION 3 (`docs/audit/00-master-index.md:318`, previous wording),
  which counted **lines**. **startRef: 32 lines in 4 files:** `PlacedObjectConfiguration.cs` 18,
  `DungeonReadRepository.cs` 8, `FloorConfiguration.cs` 2, and the exempt `src/DungeonMaster.Domain/Entities/Entity.cs`
  4 (`:65` `/// <param name="obj">`, `:69` its `<paramref>`, `:71` `public override bool Equals(object? obj) =>`,
  `:72` `Equals(obj as Entity);` -- the framework override's parameter name). 32 - 4 = 28 non-exempt lines.
  Required after T2: **exactly the 4 `Entity.cs` lines.** `src/DungeonMaster.Application` holds 0 lines at
  startRef. `src/DungeonMaster.Infrastructure/Migrations` holds 0 files matching (`grep -rlw obj` over it prints
  nothing).
- **Unit settlement for the record's "17 lines across 3 files"** (told: orchestrator measurement, spec phase).
  The record's 17 was OBJ-WORD's lines minus the 4 exempt ones, at 6b-xvii: 9 + 6 + 2. The same instrument now
  gives 18 + 8 + 2 = 28. The difference is DRIFT, not error: PlacedObjectConfiguration's nine lambdas were each
  reformatted from one line to two (`builder.Property(obj =>` / `obj.FloorId)`), still nine lambdas;
  DungeonReadRepository's six projection lines are still six (`:41`, `:43-47`, the `Select(obj =>` head and its
  five uses) and the file gained two **comment** lines naming the parameter, `:91`
  (`// query above. The projection below reaches Position: obj.Position, and`) and `:133`
  (`// FloorProjection reaches Position: obj.Position, an owned CanvasPoint.`). Those two comments name the
  lambda parameter and go stale if it is renamed without them, so T2 renames them with it.
- **OBJ-HEAD:** `grep -rnwE "obj =>" src --include=*.cs --exclude-dir=obj --exclude-dir=bin` -- the second
  predicate, counting lambdas rather than lines. **startRef: 11 lines**, all in the three files:
  `PlacedObjectConfiguration.cs:23`, `:26`, `:30`, `:34`, `:39`, `:44`, `:48`, `:52`, `:57`;
  `FloorConfiguration.cs:132`; `DungeonReadRepository.cs:41`. None anywhere else in `src`. Required after T2: **0**.
- **The replacement name.** Every one of the eleven lambdas takes a `PlacedObject` (the nine in
  `PlacedObjectConfiguration.Configure`, over `EntityTypeBuilder<PlacedObject>`; the `HasForeignKey` over
  `floor.Objects` in `FloorConfiguration.cs:129-134`; the `Select` over `floor.Objects` in
  `DungeonReadRepository.cs:40-48`), so the subject-describing name is `placedObject` -- the name `CORE-024`'s
  `Suggested direction.` gives (`docs/audit/01-domain-application.md:1685`, "rename `obj` to `placedObject`")
  and the one the Domain already uses for the same subject (`src/DungeonMaster.Domain/Entities/Floors/FloorMethods.cs:226`,
  `:265`). Clash check (startRef): `grep -rnw placedObject src/DungeonMaster.Infrastructure --include=*.cs --exclude-dir=obj --exclude-dir=bin`
  prints nothing; in `PlacedObjectConfiguration.cs:56-59` the sibling `OwnsOne` lambda takes `position =>`, and in
  `DungeonReadRepository.cs` the enclosing lambda is `floor =>` and the sibling is `area =>`.
- **Out of the recorded population, measured and not renamed.** OBJ-WORD's scope is the record's (Application,
  Infrastructure, Domain). Over the other projects, at startRef, `grep -rnw obj <dir> --include=*.cs --exclude-dir=obj --exclude-dir=bin`
  prints: `src/DungeonMaster.App` 32 lines in 2 files (`Graphics/FloorMapDrawable.cs`,
  `Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`, locals and a parameter named `obj`);
  `src/DungeonMaster.Tests` 46 lines in 7 files (locals named `obj` in five test files, and two lines in
  `App/Theme/RepoPaths.cs:67` and `RepoPathsTests.cs:195` where `obj` is a directory name inside a string);
  `src/DungeonMaster.App.Tests` 1 line (a doc comment quoting a grep pattern); `src/DungeonMaster.App.Core`,
  `src/DungeonMaster.App.UnitTests` and `src/DungeonMaster.App.UiTests` 0. These are outside `MASTER-130`'s
  recorded population and outside this session's scope (spec Assumptions).

### 4.4 RENAME -- the post-state is exactly the word rename (expected digests)

For each file a task owns, the required post-state content is the startRef content with every whole-word
occurrence of the old name replaced by the new one, and nothing else changed. The expected digests were computed
in the spec phase two independent ways that agreed on every file: Python
`re.sub(r'(?<![A-Za-z0-9_])<old>(?![A-Za-z0-9_])', '<new>', text)` over the CR-stripped bytes, and
`tr -d '\r' < <file> | sed -E 's/(^|[^A-Za-z0-9_])<old>([^A-Za-z0-9_]|$)/\1<new>\2/g'`; after the replacement,
`grep -cw <old>` on the expected text printed 0 for every file, so no occurrence was missed.

**Check after the task:** `tr -d '\r' < <file> | sha256sum` prints the expected digest below. Together with B
(line count and CRLF unchanged) this proves the change is the rename and only the rename.

`ct` -> `cancellationToken` (T1). Occurrences replaced per file in parentheses; total 62 (CT-WORD's 63 minus
NotebookView's 1).

| File | expected digest | n |
| --- | --- | --- |
| `src/DungeonMaster.App/Assets/ObjectAssets/ObjectPackRegistry.cs` | `201441c44008fbf85e4b446d0d19c477fa65014e1d91f0c166e94f94e3f5ef0d` | 2 |
| `src/DungeonMaster.Tests/Application/Common/Dispatch/DispatcherCancellationTests.cs` | `6692df79e7c1d55fa9133a3ec6a2857add91adcde3a9535ed903982cb6aa64c1` | 8 |
| `src/DungeonMaster.Tests/Application/Common/Dispatch/DispatcherTests.cs` | `83fc00d35a84f1b2f9fe325fe2a807c18aa86cf840bdbcb6b0f4df6901a76b68` | 8 |
| `src/DungeonMaster.Tests/Application/Common/Handlers/CommandHandlerTestBase.cs` | `d4a31aee04696ad41beb6b3585672e90b8cadbf368e4420e8a55c6553ae61f12` | 2 |
| `.../Commands/AddAreaCommandHandlerTests.cs` | `91311a4994d3378c18ebaa1d0996525090ed93a2f344a2eb18b525333f93f81d` | 3 |
| `.../Commands/AddFloorCommandHandlerTests.cs` | `1b34ca2e75508e8a1c63b56f8b7a0fba6b327ca98df21550b04c3396dd2f92ae` | 3 |
| `.../Commands/AddPlacedObjectCommandHandlerTests.cs` | `607db9e169116b3dba756c719b19754da845ed20f79d3186b079800d3d66fb93` | 3 |
| `.../Commands/CreateDungeonCommandHandlerTests.cs` | `a0ff5996e89c2a34d5687d5a7743a2cba46bd7d15c1e11f290e3b13ac7568f27` | 3 |
| `.../Commands/DeleteDungeonCommandHandlerTests.cs` | `df3ccdba44530f3686b1ac50d0605ffea13ef25fa9ff12a8f902e310f2744af0` | 3 |
| `.../Commands/RemoveAreaCommandHandlerTests.cs` | `67d4f8db2378e78693d2bc303fa5972903d7ac2f438212a71ce9561edfaf7283` | 3 |
| `.../Commands/RemoveFloorCommandHandlerTests.cs` | `33bbdc334995b3ee8ff9d15ebde1a937326fb191070f74153001a6ed6f201aa4` | 3 |
| `.../Commands/RemovePlacedObjectCommandHandlerTests.cs` | `ade1f95a811464b47857b09c25352c3e6098a7692ee6478d46bb605616e0c408` | 3 |
| `.../Commands/ReorderFloorCommandHandlerTests.cs` | `11f3580f53fc49759693dd85eabe41756117a5b18f722572a0c4698470fb5b18` | 3 |
| `.../Commands/UpdateAreaCommandHandlerTests.cs` | `89496dbb1852be0886896becef525d5f6dfdcbb89d6b08a18ab5d39b2a7bb064` | 3 |
| `.../Commands/UpdateAreaOutlineCommandHandlerTests.cs` | `4264b9c80d44c946e666e108a9b77c6792e6683703fc7c8ae3302abb1c37fe16` | 3 |
| `.../Commands/UpdateDungeonCommandHandlerTests.cs` | `6d3db66bba745df97171bf3d46a7b0be9e405d1f8e7fcc06912287975a82da55` | 3 |
| `.../Commands/UpdateFloorCommandHandlerTests.cs` | `9f4bda9f252a3ddfb6ceb282a96c0ab50ac45964f5af8d5cefae87aac86212a9` | 3 |
| `.../Commands/UpdatePlacedObjectCommandHandlerTests.cs` | `6e1c5cfd9ae15922f01e859d78f78c71b7dd84ca61f64be48e9811361167a210` | 3 |

`obj` -> `placedObject` (T2). Total 28 occurrences, one per line, equal to OBJ-WORD's 28 non-exempt lines.

| File | startRef digest | expected digest | n |
| --- | --- | --- | --- |
| `src/DungeonMaster.Infrastructure/Persistence/Configurations/FloorConfiguration.cs` | `5167cf2890787e1cb72a38e4a8924509ccadc2be8c9afc316ccd6c39fbf7e52d` | `f8c4434b9c9148b94bcb9e76ff9af55be3a871d0999351f575c636d739550c41` | 2 |
| `src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs` | `da25e7968ff7f18c8fc79eddec7858f230534459aa8e2a4102e414b28defe2a9` | `32007d8e63a4d225c47b8a7997ec9c92a0757261b6e36d42ff1c53f500790f4e` | 18 |
| `src/DungeonMaster.Infrastructure/Persistence/Repositories/DungeonReadRepository.cs` | `9611761231ccf460080019e89442cab6289bd9cb0e3de6bc3b10015c5aa0cc29` | `7aeb30633dac4bad5936f6ac73c27f2a9682e7faeea3982418a522d93f23e682` | 8 |

Control for RENAME (each task runs it before its first edit): `tr -d '\r' < <file> | sha256sum` on each owned
file prints **neither** the expected digest (startRef differs from the post-state) and, for T2's three files, the
startRef digest above. A file whose startRef digest is not the tabled one has changed under you: stop, copy it
into your scratch directory, and report.

### 4.5 QOBJ -- audit text quoting the `obj` lambda code (T3, read-only)

`grep -rnE 'obj =>|obj\.(Id|FloorId|Kind|Rotation|Scale|CreatedAt|LastModified|Position)' docs/audit --include=*.md`
**startRef: 15 lines**: `00-master-index.md` 4 (rows 303, 304, 318, 335), `01-domain-application.md` 5
(`:1105`, `:1269`, `:1706`, `:1811`, `:1812`), `02-infrastructure.md` 6. These are records of the code as it
stood when each was written; after T2 they quote text the source no longer holds. **None is edited** (house rule:
nothing deletes previous wording); T3's cell and block say so with this figure, cited by this section.

## 5. Register instruments (T3)

- **R -- unescaped pipes on a register line:**
  `tr -d '\r' < docs/audit/00-master-index.md | sed -n '<N>p' | sed 's/[\\][|]//g' | tr -cd '|' | wc -c`.
  Control (run in the spec phase): `printf '| a | b \\| c |\n'` through the same tail prints 3; `printf '| a | b | c |\n'`
  prints 4. The form `sed 's/\\|//g'` fails that control and must not be used. **startRef: rows 269 and 318 each
  print 11** (rows 333, 345, 366 also print 11). Required after T3: 11 on each of 269 and 318.
- **OC -- old status cell:** `tr -d '\r' < docs/audit/00-master-index.md | sed -n '<N>p' | sed 's/^.*| <ID> | //; s/ |$//'`,
  then `printf '%s' "$cell" | sha256sum` and `printf '%s' "$cell" | wc -c`. `| <ID> | ` occurs once on each
  line at startRef. **startRef:** row 269 (`<ID>` = `CORE-016`)
  `50f83e900e5b785f27764b42e84127a58d82c8d5145ea82111a152134d5f8e7e`, 303 bytes, beginning
  `part done (6b-xv, 2026-09-30) -- the duplicated scale rule`; row 318 (`CORE-024`)
  `e91744ef0830fac6a0d7b544627bd59102514bcfa1540af6db02d116e5520823`, 11846 bytes, beginning
  `part done (20261105-1907-core-heavy-rows, 2026-11-05) -- `. Row 318's cell already holds one
  `**Previous status wording, retained unchanged below.**` marker; row 269's holds none.
- **V -- the retained previous wording, from the FIRST marker.** The greedy `sed 's/^.*\*\*Previous ...//'` form
  of earlier sessions strips to the **last** marker, and after T3 row 318 holds two (the new one and the one
  inside the retained cell), so that form would return only the 6b-xvii text and pass on a cell missing the
  20261105 wording. Use this script, saved in your scratch directory (its sha256 in the spec phase was
  `9e138ed53a7e0358aeaa2e59d681694a555106447e8b4fbbe5326467ccb3777e`):

  ```
  import sys, hashlib
  path, n = sys.argv[1], int(sys.argv[2])
  line = open(path, 'rb').read().split(b'\r\n')[n - 1].decode('utf-8')
  marker = '**Previous status wording, retained unchanged below.** '
  i = line.find(marker)
  if i < 0 or not line.endswith(' |'):
      print('NO-MARKER'); sys.exit(1)
  cell = line[i + len(marker):-2]
  b = cell.encode('utf-8')
  print(hashlib.sha256(b).hexdigest(), len(b), 'markers=%d' % line.count(marker))
  ```

  Run as `python -I <scratch>/v1.py C:/dm-wt/20261109-1641-part-done-rows/docs/audit/00-master-index.md <N>`.
  Controls (run in the spec phase): row 366 prints `7e26bc181ceac74267a14b9cf218a83fb2447a68e7750d326886ebc613866fe8 1577 markers=1`
  (the digest the `20261105-1907-core-heavy-rows` contract C1 recorded for that row); row 345 prints `NO-MARKER`;
  a scratch fixture line `| x | new **Previous status wording, retained unchanged below.** old A **Previous status wording, retained unchanged below.** older B |`
  with CRLF prints `cb9571892acd5cf2196542774f8c89a1bb4f5baadb0fb854f593a8000105a0eb 68 markers=2`, the digest
  of `old A **Previous status wording, retained unchanged below.** older B` (the greedy sed form returns
  `older B` instead). **Required after T3:** row 269 prints `50f83e90... 303 markers=1`; row 318 prints
  `e91744ef... 11846 markers=2`.
- **U -- nothing else in the index changes:** `tr -d '\r' < docs/audit/00-master-index.md | sed '269d;318d' | sha256sum`.
  **startRef: `97a020fcbd248fd0373b3fdbc86a9a4b7b6020d4a713dd7c66da69821bdff6cc`**; the file has 691 lines
  (whole-file `tr -d '\r' | sha256sum` `636d0a04e356060bf720640384ca5cd09e09dbb0318bf7365ac616382992fb86`).
  Required after T3: the same U digest and 691 lines.
- **AO -- amendment-block openers:** `tr -d '\r' < docs/audit/01-domain-application.md | grep -c '^\*\*\[AMENDED'`.
  **startRef: 18.** Must-match: `:1770` (`**[AMENDED 20261105-1907-core-heavy-rows, 2026-11-05 -- the ...`).
  Required after T3: **20**. Closers in this file end `.**]**` (e.g. `:1823`, `:3053`); judged by reading.
- **ID01 -- nothing in `01-domain-application.md` but the two blocks changes:**
  `tr -d '\r' < docs/audit/01-domain-application.md | sha256sum` **startRef:
  `183cf7221a8fcc5799d0876ca25768eb8fb08a8d59c00bf21a61e16f8b13a53e`** (3214 lines). After T3, the same command
  with the inserted line ranges deleted (`| sed '<a>,<b>d;<c>,<d>d'` before `sha256sum`, the ranges as numbered in
  the post-T3 file) prints that digest. Control: the same pipeline with no deletion prints a different digest
  after T3, and `wc -l` on the post-T3 file equals 3214 plus the inserted line count.

- **UNOWNED -- audit files no task edits**, `tr -d '\r' < <file> | sha256sum`, startRef (required unchanged after
  T3): `docs/audit/02-infrastructure.md` `877841f8953629a7bcc57930986f0be5511485ee96e91519269cd243baaa640c`;
  `docs/audit/03-app-shell-and-views.md` `db061f1ef68f571885a4ecdc5a91b14f6d815242934f1366b07f3fa63cb577a0`;
  `docs/audit/04-app-controls-and-theme.md` `627ed33bb7754dc0cc1716e3338c8bd0cf11295067143afc38d1fe700bb9d000`.
- **AppData:** `grep -c AppData <file>` prints 0 on both `00-master-index.md` and `01-domain-application.md` at
  startRef. Required after T3: 0 on each.

## 6. CITE -- line citations into `01-domain-application.md` that an insertion moves (T3)

`01-domain-application.md:1050-1056` (the `AMENDED 6b-xv` block on `CORE-016`) warns that inserting paragraphs
in `CORE-016` shifts every line below it. Both of this session's insertions land above live citations.

Instruments, both run before the first edit and after the last:

- **CITE-IN** (in-file), with a literal backtick character on each side of the pattern, inside single quotes:

  ```
  tr -d '\r' < docs/audit/01-domain-application.md | grep -noE '`:[0-9]{4}(-[0-9]+)?`'
  ```

  **The spelling `\x60` for the backtick prints 0 under `grep -E` (measured in the spec phase) and must not be
  used.** Second predicate: `tr -d '\r' < docs/audit/01-domain-application.md | grep -noE '(^|[^A-Za-z0-9_.])(:[0-9]{4})'`.
  **startRef: both print 24 matches**; every match of the second is backticked.
- **CITE-X** (cross-file): `grep -rnoE '01-domain-application\.md:[0-9]+(-[0-9]+)?' docs` **startRef: 18 matches**
  -- `:1293` 9 times (`03-app-shell-and-views.md:208`; `04-app-controls-and-theme.md:1017`, `:1133`, `:1272`,
  `:1373`, `:1465`, `:1555`, `:1686`, `:1798`), `:2156-2216` 8 times (`04-app-controls-and-theme.md:1016`,
  `:1132`, `:1271`, `:1372`, `:1464`, `:1554`, `:1685`, `:1797`), `:2313` once (`00-master-index.md:307`).
  `.dir-sdlc/` and `.sir/` hold historical records and are out of population.

The architect's reading at startRef of every in-file citation whose target is below `:1059` (the last line of
`CORE-016`'s `AMENDED 6b-xv` block), **told -- T3 re-reads each target before relying on it**:

| Cited at | Cites | Reading at startRef |
| --- | --- | --- |
| `:1054` | `:1162-1167` | live, correct: the `CORE-019` `locations:` window |
| `:1598` | `:1567` | live, correct: the quoted `Floor.cs at 73 lines` sentence |
| `:1612` | `:1561-1563` | live, correct: the quoted `PlacedObject is not partial at all` sentence |
| `:1696`, `:1867`, `:1997`, `:2123`, `:2263`, `:2393`, `:2661` | `:1293` | live, correct: the `AMENDED 6b-xiv` opener on `CORE-019` |
| `:1794` | `:1712-1714` | live, correct: the quoted blocker-class sentence in `CORE-024`'s 6b-xvii block |
| `:1608`, `:1620` | `:2940`, `:2940-2941` | dated ("as at `11f9562`") |
| `:1618` | `:2302` | dated ("as at `11f9562`") |
| `:2122` | `:2090-2125` | dated ("it was ... before") |
| `:2531` | `:2447` | dated ("at this session's startRef") |
| `:3157` | `:2801-2803` | dated ("at `d2b84a2`") |
| `:2121` | `:2156-2216` | live, **already stale**: `CORE-028`'s `AMENDED 6b-xvii` block opens at `:2258` and closes at `:2318` |

Cross-file: the nine `:1293` citations are live and correct; the eight `:2156-2216` are live and already stale,
as above; `00-master-index.md:307`'s `:2313` was not adjudicated by the architect -- T3 reads it.
