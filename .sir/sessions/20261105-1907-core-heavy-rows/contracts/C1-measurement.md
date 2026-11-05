<!-- frozen: 20261105-1907-core-heavy-rows, Gate 2 -->

# C1 -- Measurement instruments, startRef baselines, build rules and editing hazards

This contract declares no callable operation. It exists so every instrument a criterion names is available
verbatim to every later phase: scratch copies made during the spec phase do not survive it.

Every baseline below was measured by the architect in the spec phase on the session worktree
`C:/dm-wt/20261105-1907-core-heavy-rows` at `62ba566b3bee6a6819846cb8667e59857fe31a24` (the clean tree the
orchestrator recorded in `discussion.md`). Paths are relative to the worktree root unless absolute.

## 0. How to run these

- Save each script below into **your own scratch directory** and run it from there. Never save one into the
  worktree. Hand Python the `C:/...` path form (lesson 2026-11-03) and run it as `python -I`.
- Every control that needs a fixture or a perturbed copy runs on files in **your own scratch directory**.
  The only injection any task makes into a worktree file is the one C2 section 5 prescribes for T1.
- `sha256sum` prints the digest followed by ` *-` or ` *<path>`. Compare the 64 hex digits only.
- Before you trust an empty or unchanged result, re-run the instrument's control and confirm the stated
  output. A result from an instrument whose control was not re-run is not evidence.

## 1. B -- line-ending byte census (lesson FORMs 1, 5, 7, 13, 15)

```
od -An -tx1 -v <file> | tr -s ' ' '\n' | awk 'NF{ if($1=="0d")cr++; if($1=="0a"){lf++; if(p=="0d")pair++} p=$1 } END{printf "CR=%d LF=%d CRLF=%d loneCR=%d\n",cr,lf,pair,cr-pair}'
```

Control: `printf 'a\r\nb\rc\n' > <scratch>/b.txt` then B prints `CR=2 LF=2 CRLF=1 loneCR=1`. A file passes when
CR = LF = CRLF and loneCR = 0. Never judge line endings with `grep $'\r'`, `sed -n`, `grep -E` with a file
argument, or `git show` output.

BOM check: `head -c 3 <file> | od -An -tx1`. A file starting `using` prints ` 75 73 69`; one starting
`namespace` prints ` 6e 61 6d`; a UTF-8 BOM prints ` ef bb bf`.

startRef values (all loneCR=0, CR = LF = CRLF = the line count shown):

| File | lines | first 3 bytes |
| --- | --- | --- |
| `src/DungeonMaster.Domain/Entities/Floors/ObjectRotation.cs` | 27 | `6e 61 6d` |
| `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs` | 898 | `75 73 69` |
| `src/DungeonMaster.Tests/AppCore/Map/MapHitTesterTests.cs` | 371 | `75 73 69` |
| `src/DungeonMaster.Tests/AppCore/Presentation/DungeonDetailPresentationMapperTests.cs` | 333 | `75 73 69` |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/AddPlacedObjectCommandHandlerTests.cs` | 201 | `75 73 69` |
| `src/DungeonMaster.Tests/Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs` | 236 | `75 73 69` |
| `src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonAggregateTests.cs` | 604 | `75 73 69` |
| `src/DungeonMaster.Tests/Domain/Entities/Dungeon/DungeonLastModifiedOnFailedMutationTests.cs` | 188 | `75 73 69` |
| `src/DungeonMaster.Tests/Domain/Entities/Floor/FloorObjectMethodsTests.cs` | 153 | `75 73 69` |
| `src/DungeonMaster.Tests/Domain/Entities/Floor/ObjectKindGuardTests.cs` | 120 | `75 73 69` |
| `src/DungeonMaster.Tests/Domain/Entities/Floor/PlacedObjectTests.cs` | 160 | `75 73 69` |
| `src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/DungeonWriteRepositoryTests.cs` | 272 | `75 73 69` |
| `src/DungeonMaster.Tests/Infrastructure/Persistence/SqliteInMemoryTestBaseTests.cs` | 85 | `75 73 69` |
| `src/DungeonMaster.Infrastructure/Migrations/AppDbContextModelSnapshot.cs` | 220 | `ef bb bf` |
| `docs/audit/00-master-index.md` | 691 | -- |
| `docs/audit/01-domain-application.md` | 2997 | -- |

Every one of the seven files under `src/DungeonMaster.Infrastructure/Migrations/` is CRLF with loneCR=0 and
starts with the BOM ` ef bb bf` (B and the BOM check, run on each).

## 2. P-CW -- the old rotation member names (DoD D1)

```
grep -rnE 'CW(90|180|270)' C:/dm-wt/20261105-1907-core-heavy-rows/src --exclude-dir=obj --exclude-dir=bin | grep -v '_RenameObjectRotationClockwiseMembers\.cs:' | grep -v 'RenameObjectRotationClockwiseMembersMigrationTests\.cs:'
```

It scans every file under `src` (not only `*.cs`), skipping `obj/` and `bin/` (the App's `bin/` holds copied
`.cs` sources), and drops the two files that deliberately name the old stored strings: the new migration
(C2 section 1; the pattern matches `<timestamp>_RenameObjectRotationClockwiseMembers.cs` and not its
`.Designer.cs`) and the new migration test file (C2 section 3). `wc -l` on its output:

- **startRef: 28** -- `FloorMapDrawable.cs:744-746` (3), `ObjectRotation.cs:16, :21, :26` (3), and 22 lines in
  the 10 test files of section 1's table (the `src/DungeonMaster.Tests/` rows other than
  `SqliteInMemoryTestBaseTests.cs`). Every alternation branch fires (FORM 3): `| grep -c CW180` gives 5 and
  `| grep -c CW270` gives 3 at startRef. Over non-`.cs` files it gives 0 at startRef.
- **required after T1: 0.**

Control for the two exclusions (FORM 3, must keep exactly one line):
`printf 'x_RenameObjectRotationClockwiseMembers.cs:5: CW90\nRenameObjectRotationClockwiseMembersMigrationTests.cs:7: CW90\nA.cs:1: CW90\n' | grep -v '_RenameObjectRotationClockwiseMembers\.cs:' | grep -v 'RenameObjectRotationClockwiseMembersMigrationTests\.cs:'`
prints only `A.cs:1: CW90` (run in the spec phase).

Companion: `grep -rn 'Clockwise90\|Clockwise180\|Clockwise270' C:/dm-wt/20261105-1907-core-heavy-rows/src --exclude-dir=obj --exclude-dir=bin | wc -l`
prints **0 at startRef**.

The C# compiler is the strong half of D1: once the members are renamed, any surviving `ObjectRotation.CW90`
fails the build. P-CW covers what the compiler cannot see: comments, strings and non-C# files.

## 3. JC -- a JSON enum converter anywhere in `src` (PathSegmentKind persistence)

```
grep -rnE 'JsonConverter|JsonStringEnum' C:/dm-wt/20261105-1907-core-heavy-rows/src --include=*.cs --exclude-dir=obj --exclude-dir=bin
```

Prints nothing at startRef. Control: `printf 'x JsonStringEnumConverter\ny [JsonConverter(typeof(Z))]\nz JsonSerializer\n' | grep -cE 'JsonConverter|JsonStringEnum'`
prints 2 (both branches fire, the near-miss `JsonSerializer` does not).

## 4. DYN -- `dynamic` in the Application layer (MASTER-090)

```
grep -rnw dynamic C:/dm-wt/20261105-1907-core-heavy-rows/src/DungeonMaster.Application --include=*.cs --exclude-dir=obj --exclude-dir=bin
```

Prints nothing at startRef. Control: `printf 'dynamic handler = x;\n// dynamically resolved\n' | grep -nw dynamic`
prints only `1:dynamic handler = x;` (the must-fire line fires, the near-miss `dynamically` does not).

## 5. R -- register-row cell count

```
tr -d '\r' < docs/audit/00-master-index.md | awk -v row=<N> 'NR==row{ s=$0; n=0; for(i=1;i<=length(s);i++){ c=substr(s,i,1); if(c=="|" && substr(s,i-1,1)!="\\") n++ } print n }'
```

Counts the `|` on line N that do not follow a `\`. Control: `printf '| a | b \\| c |\n'` at row 1 prints 3.
At startRef it prints **11** on lines 318, 345 and 366.

## 6. V -- "previous status wording retained": hash of a status cell's last N characters

```python
import sys, hashlib
path, row, anchor, n = sys.argv[1], int(sys.argv[2]), sys.argv[3], int(sys.argv[4])
line = open(path, 'rb').read().decode('utf-8').split('\r\n')[row - 1]
start = line.index(anchor) + len(anchor)
cell = line[start:-2] if line.endswith(' |') else None
if cell is None or len(cell) < n:
    print('NO-CELL-OR-TOO-SHORT')
else:
    print(len(cell), hashlib.sha256(cell[-n:].encode('utf-8')).hexdigest())
```

Run: `python -I V.py C:/dm-wt/20261105-1907-core-heavy-rows/docs/audit/00-master-index.md <row> '<anchor>' <N>`.
It prints the status cell's length and the sha256 of its last N characters. A cell that keeps its previous
wording verbatim at its end prints the startRef digest below.

| Row | anchor (exact, trailing space included) | N (startRef cell length) | startRef digest |
| --- | --- | --- | --- |
| 318 | `\| CORE-024 \| ` | 7319 | `cdf4794226968e5a830acc5f1826fe3aeda7c3eefd86802ab868e115960ee120` |
| 366 | `\| CORE-035 \| ` | 1576 | `7e26bc181ceac74267a14b9cf218a83fb2447a68e7750d326886ebc613866fe8` |

(In this table `\|` is the markdown escape for a literal `|`; the anchor argument is `| CORE-024 | `.)
Line 345's status cell is empty at startRef (length 0), so there is no previous wording to retain on it.

Controls, run in the spec phase on scratch copies of `00-master-index.md`: must pass -- prepend
`NEW TEXT. **Previous status wording, retained unchanged below.** ` to row 318's cell: prints length 7384 and
`cdf47942...`; must fire -- additionally change `EXCLUSION 2 --` to `EXCLUSION 2 -` inside the retained text:
prints `9fe304bb...`.

## 7. U -- every master-index line except the three rows this session edits

```
tr -d '\r' < docs/audit/00-master-index.md | awk 'NR!=318 && NR!=345 && NR!=366' | sha256sum
```

startRef: `21286673f9bf98a1f51bab307d686e2f326aac28f8d5b8a5aef24dd9a7b4a88a` over 688 lines. Required after each of
T2, T3 and T4: **the same digest**, and `wc -l` on the file still 691. Control:
`printf 'a\nb\nc\n' | awk 'NR!=2' | sha256sum` and the same with `b` changed to `X` print the same digest
(`b72cf6d7...`); with `c` changed to `d` it prints `17fd9b11...`.

## 8. AO / AC -- amendment-block censuses in `docs/audit/01-domain-application.md`

- opener census AO: `tr -d '\r' < docs/audit/01-domain-application.md | grep -c '^\*\*\[AMENDED'` prints **15** at
  startRef. It matches `:1577` (`**[AMENDED 20261105-0106-core-two-row-sweep, ...`) and not the near-misses
  `:1093` / `:1101` (`**AMENDED 6b-viii`, `**AMENDED 6b-ix`, no bracket).
- closer census AC: `tr -d '\r' < docs/audit/01-domain-application.md | grep -c '\]\*\*$'` prints **15** at startRef.
  It matches `:1768` (ends `unchanged.**]**`).
- Each of T2, T3 and T4 adds one block, in that order (manifest `dependsOn`), so both censuses print **16 after
  T2, 17 after T3 and 18 after T4**.

Anchors at startRef (each task locates its own by heading text, since earlier tasks shift line numbers):

| Entry | last paragraph before the insertion point | blank | next heading |
| --- | --- | --- | --- |
| CORE-024 | the `AMENDED 6b-xvii` block, `:1689-1768` | `:1769` | `### CORE-025` `:1770` |
| CORE-031 | `**Suggested direction.**` `:2461-2463` | `:2464` | `### CORE-032` `:2465` |
| CORE-035 | `**Suggested direction.**` `:2829-2836` | `:2837` | `### CORE-036` `:2838` |

## 9. Build, test and EF commands (FORM 8; lesson 20261029-1708)

- `dotnet build C:/dm-wt/20261105-1907-core-heavy-rows/src/DungeonMaster.sln`. Quote its three summary lines:
  `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`. Never use a `grep -c` over "warning|error".
- `dotnet test C:/dm-wt/20261105-1907-core-heavy-rows/src/DungeonMaster.sln --no-build`, run after that build.
  Quote each assembly's summary line. The orchestrator probed it runnable at startRef with
  `DungeonMaster.App.UnitTests.dll` 187, `DungeonMaster.Tests.dll` 953 and `DungeonMaster.App.Tests.dll` 123
  passed, 0 failed. Output that names no test count is reported as not runnable, not as a pass.
- `dotnet ef migrations has-pending-model-changes --project C:/dm-wt/20261105-1907-core-heavy-rows/src/DungeonMaster.Infrastructure --startup-project C:/dm-wt/20261105-1907-core-heavy-rows/src/DungeonMaster.Infrastructure --no-build`,
  run after a build. At startRef (orchestrator's probe) it printed a tool/runtime version-skew warning (global
  `dotnet-ef` 10.0.3, runtime 10.0.11) and then `No changes have been made to the model since the last migration.`
  The skew does not block, and per Gate 1 the tool is left alone: no task installs, updates or adds a local
  tool manifest (`C:/dm-wt/20261105-1907-core-heavy-rows/.config` does not exist at startRef).
- **This toolchain cannot build concurrently in one worktree.** `dotnet build`, `dotnet test` and every `dotnet ef`
  command that builds write one shared `obj/` and `bin/` tree. In this manifest **only T1 runs any `dotnet`
  command**; T2, T3 and T4 run none. A reviewer of T1 runs them only while no other `dotnet` command runs.
- A build may rewrite a tracked `packages.lock.json` (lessons 2026-11-02, 2026-11-04). No task owns one. Never
  modify or revert one; report it if the build output or `ls -l` shows it changed.

## 10. Editing hazards (bind every task, and every reviewer who touches a file)

- No `sed -i` and no `perl -i`, ever: each rewrites CRLF to LF.
- No `Write` on any existing file in the worktree: it emits LF, and an LF `.cs` file fails the build under `IDE0055`.
- `Edit` keeps CRLF terminators but strips trailing whitespace (spaces and tabs) from `new_string`, and reports
  success. Never end a `new_string` on whitespace; key each edit on a line's first text character.
- `Edit` normalises embedded lone CRs anywhere in the file it writes. Every file this session edits has
  loneCR = 0 at startRef (section 1), so this is safe here. Confirm with B after the last edit.
- A new hand-made `.cs` file is CRLF from birth. One safe route: `cp` a CRLF sibling to the new path, then shape
  it with `Edit`. Prove the result with B and the BOM check before anything builds it.
- Files the EF scaffolder writes (C2 section 1) are checked with B and the BOM check as soon as it exits.
- Indent `.cs` files outside `Migrations/` with tabs, never spaces (`dotnet-format.md`; `IDE0055` enforces it).
  Files under `src/DungeonMaster.Infrastructure/Migrations/` are `generated_code = true` (`.editorconfig:49-50`),
  so analyzers skip them: hand edits there follow the scaffolded layout (four-space indentation, block
  namespace), as the hand-written `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs` does.
- Never write a `.sir/` session path into a file under `src/` (lesson 2026-11-03, `.sir/lessons.md:54`).
  `docs/audit/` cells and blocks may cite session paths, as the register already does.
- Never edit anything under `.dir-sdlc/` or `.sir/backlog.md`; the orchestrator trims the backlog at Gate 3.
