# Measurement record -- session 6b-xvii

Every figure below was derived by the orchestrator on 2026-10-09 against the working tree at
`00a093a` (`chore/repo-audit`, clean, 0 ahead / 0 behind origin), with the command beside it.
Nothing here is inherited. Figures that could not be measured are marked NOT MEASURED.

The purpose of this file is to be the evidence base for the session's acceptance criteria, so a
criterion can cite a measured line rather than an audit entry whose specifics have drifted.

---

## 1. Register state

Command: one `awk -F'|'` over rows matching `^\| [0-9]+ \| MASTER-[0-9]+ \|` in
`docs/audit/00-master-index.md`, classifying column 11.

**The `part done` test must run BEFORE the `done` test.** Testing `done` first returns 3 part-done
rows, not 4: `MASTER-098`'s status prose contains a substring the closed-row pattern also matches, so
the row is absorbed into the closed count. With the order corrected:

| Class | Count |
|---|---|
| closed | 133 |
| open | 42 |
| part done | 4 -- `MASTER-081`, `MASTER-094`, `MASTER-098`, `MASTER-108` |
| total | 179 |

Open severity: S4 32, S5 10. Open effort: XS 24, S 13, M 3, L 2.
Open layers: CORE 12, UIKIT 9, UI 8, TEST 6, INFRA 6, `UI, XCUT` 1.

## 2. The duplicate pair

`MASTER-111` (`:299`) and `MASTER-171` (`:359`) carry byte-identical titles, both S4 / CORE / naming /
XS / risk=low, both open. Grouping every open row by column 9 and reporting groups larger than one
returned exactly this one group. **The bound is on exact titles only.**

The defect is live: `src/DungeonMaster.Domain/Entities/Floors/RotateDirection.cs:3` reads
`public enum RotationDirection { Clockwise, CounterClockwise }`.

Blast radius of the type name: 3 files, 6 occurrences --
`grep -rln 'RotationDirection' src --include=*.cs --include=*.xaml` and the matching
`grep -ro ... | wc -l`. The files are `RotateDirection.cs`, `ControlsPaletteView.xaml.cs`,
`DungeonDetailPage.xaml.cs`.

---

## 3. Per-row re-measurement

Seven of the ten selected rows carried at least one false or stale specific.

### MASTER-110 / CORE-022 -- EXCLUDED from this batch

Substance holds: `Floor` has three partials (`Floor.cs:8`, `FloorMethods.cs:5`,
`FloorObjectMethods.cs:5`), and `PlacedObject.cs:8` declares `public sealed class PlacedObject`, not
partial. All eight cited location line numbers are exact.

**Stale figures:** "Floor.cs at 73 lines" -- actual **82** (`wc -l`). "305 lines of behaviour" --
actual **288** (`FloorMethods.cs` 176 + `FloorObjectMethods.cs` 112).

Authority verified: `dm-conventions/backend/dotnet-format.md:556-563` defines the split as exactly two
files. Excluded by user decision: conforming exceeds the row's `XS` estimate, so it goes to the
backlog for a properly-scoped track.

### MASTER-111 + MASTER-171 / CORE-023 -- IN BATCH

Verified in full, see section 2. Remedy per the entry: rename the file to `RotationDirection.cs`.
Authority: `dotnet-format.md:57` -- "File name matches the type name exactly".
One fix closes two rows; `MASTER-171` closes as a duplicate naming `MASTER-111`.

### MASTER-112 / CORE-026 -- IN BATCH

Substance holds. Doc-comment forms were measured by reading bytes (`cat -A`), after `grep -P` proved
unusable in this locale -- it errors with "supports only unibyte and UTF-8 locales" **and prints 0**,
which is a confident wrong number rather than a visible failure.

| File | Finding |
|---|---|
| `CanvasPoint.cs` | `///` + **5 spaces** at `:4-5`. Its `<remarks>` block, `:7` to `:33`, uses the conforming `///` + 2 tabs. The file is **mixed**. |
| `PlacedObjectDto.cs` | `///` + **5 spaces** at `:7` and `:12`; 4 single-line `<param>` tags -- see the correction below for their lines |
| `PlacedObject.cs` | single-line `<summary>` tags -- see the correction below for the count |

> **CORRECTED 2026-10-09, same session.** The single-line-tag rows above were wrong in three ways, found
> by `dir-architect` during decomposition and re-verified by the orchestrator with
> `grep -nE '///[[:space:]]*<(param|summary|returns)[^>]*>.*</(param|summary|returns)>'`, which returns
> `0` on the conforming control `GetDungeonsQueryHandler.cs`:
>
> | Claim as first written | Measured | What went wrong |
> |---|---|---|
> | `PlacedObjectDto.cs` tags at `:9, :10, :13, :14` | `:9, :10, :14, :15` | `:13` is the `/// </param>` **closer** of the already-conforming `Position` tag spanning `:11-13`; `:15` was omitted. The count 4 was right and two of the four positions were wrong. |
> | `PlacedObject.cs` has **5** single-line `<summary>` tags | **6**, at `:12, :37, :40, :49, :52, :55` | My predicate was `<summary>[^<]+</summary>`. The character class cannot cross a nested tag, so it silently skipped `:12`, whose summary contains `<see cref="PlacedObject"/>`. A blind predicate, not a miscount. |
> | `CanvasPoint.cs` single-line tags: not mentioned | **3**, at `:34`, `:35`, `:84` | I read only the first 16 lines of the file and reported as though I had censused it. |
>
> The `PlacedObject.cs` single-line `<param>` tag is at `:81`. All three single-line tags in
> `CanvasPoint.cs` sit **outside** its `<remarks>` block, which closes at `:33`, so expanding them does
> not disturb the block left deliberately untouched.
>
> The second row is the one worth carrying: the count was wrong because the **predicate** was wrong, and
> re-running it would have returned 5 every time. A census validates its predicate, not only its total.

Control: `GetDungeonsQueryHandler.cs:7` writes `///` + 2 tabs, confirming the dominant form.

Authority: `dotnet-format.md:375` -- "Tag descriptions go on a **new line**, indented with **two
tabs** from the `///` marker".

**NOT MEASURED:** the repo-wide count of files using the space form. The per-file census timed out at
120 seconds. No figure is asserted for it.

**Unfiled deviation found while reading the authority.** `dotnet-format.md:378` also requires "Line
breaks occur only at sentence boundaries. Never break a sentence across multiple `///` lines."
`CanvasPoint.cs`'s `<remarks>` block breaks sentences mid-clause across `///` lines. This is a real
deviation against the same section, it is **not** what `MASTER-112` filed, and it is surfaced at
Gate 2 rather than folded silently into scope.

### MASTER-113 / CORE-028 -- FALSIFIED, evidence-only close

The row claims twelve handlers carry `using DungeonMaster.Application.Features.Dungeons;` and four do
not. **Zero of the sixteen handlers carry it, and the directive exists nowhere in `src`.**

- `grep -rln '^using DungeonMaster.Application.Features.Dungeons;$' src/DungeonMaster.Application/Features/Dungeons --include=*Handler.cs` -> 0 of 16
- unanchored `grep -rn 'using DungeonMaster.Application.Features.Dungeons;' src --include=*.cs`, with
  `bin` and `obj` excluded -> **0**
- **positive control**, a using line known to be present in exactly those files:
  `grep -c '^using DungeonMaster.Application.Common.Dispatch;$'` on
  `Features/Dungeons/Commands/AddArea/AddAreaCommandHandler.cs` -> **1**

The predicate can see a using line in these files and still finds none of the claimed one, so the zero
is a measurement rather than a blind spot. The cause is visible: the repository interfaces now live at
`Application/Common/Interfaces/`, which is `CORE-029`'s remedy, and that move removed the reason for
the directive. The row is already resolved by a sibling change and closes with evidence, no code
change.

### MASTER-114 / CORE-030 -- IN BATCH

Substance holds. `grep -rn 'AsReadOnly()' src --include=*.cs` returns these property sites:

| Site | Cited as | Actual | Note |
|---|---|---|---|
| `Dungeon.Floors` | `Dungeon.cs:48-49` | `:49` body | exact |
| `Area.Outline` | `Area.cs:60-61` | `:61` body | exact |
| `Floor.Areas` | `Floor.cs:65-66` | `:74-75` | **drift, +9** |
| `Floor.Objects` | `Floor.cs:71-72` | `:80-81` | **drift, +9** |
| `MapDrawingSession` | `Application/Common/Map/MapDrawingSession.cs:37-38` | `src/DungeonMaster.App.Core/Map/MapDrawingSession.cs:43` | **moved project** |

Both `Floor.cs` citations are wrong by the same constant +9, which is drift rather than
mis-identification -- the same signal that explains `MASTER-110`'s stale line counts. The fifth site
is in `DungeonMaster.App.Core`, outside the CORE layer entirely.

### MASTER-130 / CORE-024 -- IN BATCH, closes PART DONE

**The row contains a false claim that would have rewritten persisted data.** It says to rename
`CW90`/`CW180`/`CW270`, "noting that persisted values are unaffected because only the names change."

- `src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs:30-32`:
  `builder.Property(obj => obj.Rotation).IsRequired().HasConversion<string>()`
- `src/DungeonMaster.Infrastructure/Migrations/AppDbContextModelSnapshot.cs:141`:
  `b.Property<string>("Rotation")` with `.HasColumnType("TEXT")`

`ObjectRotation` is persisted **by member name**. Renaming the members rewrites every stored value --
the same blocker class as the `PathSegmentKind` row the register already parks as blocked. Excluded by
user decision; a backlog item covers both renames with real migrations.

**`ct` population, predicate validated both ways.**
`grep -rn 'CancellationToken ct\b' src --include=*.cs`, with `bin` and `obj` excluded -> **79**
declaration sites: Application **44**, Tests **27**, Infrastructure **7**, App **1**.

- positive control, a known declaration:
  `grep -c 'CancellationToken ct\b'` on `Common/Dispatch/IQueryHandler.cs` -> **1**
- near-miss control that must NOT match: `grep -c 'CancellationToken cancellationToken'` on the same
  file -> **0**

The row cites six files. Scope set by user decision: **Application + Infrastructure, 51 sites.** Tests
(27) and App (1) go to the backlog.

**Stale citations.** The two repository interfaces are cited at
`Application/Features/Dungeons/IDungeonWriteRepository.cs` and `IDungeonReadRepository.cs`; both now
live at `Application/Common/Interfaces/`. The `obj` local is cited at `FloorObjectMethods.cs:40`;
**line 40 holds no `obj`**. The nine occurrences are at `:31, :38, :41, :56, :59, :64, :95, :98,
:103`, across three distinct locals. `ObjectRotation.cs:5-8` is exact.

Authority: `dotnet-format.md:36` gives the parameter example as `cancellationToken`;
`dotnet-format.md:42` requires descriptive lambda and local names.

### MASTER-131 / CORE-025 -- IN BATCH

Partly stale. `grep -c '<summary>'` per file:

| Type | `<summary>` tags | Verdict |
|---|---|---|
| `ObjectKind` | **1** | row claims none; the `None` member at `:5` IS documented. The type declaration at `:3` is not. |
| `ObjectRotation` | 0 | confirmed |
| `ObjectScale` | 0 | confirmed -- but see the shape correction below |
| `SnapAnchor` | 0 | confirmed |
| `RotateDirection` | 0 | confirmed |
| `ScaleDirection` | 0 | confirmed |

`ObjectKind`'s member count is **38**, not the 37 the row states, by two independent predicates that
agree: `^[[:space:]]+[A-Za-z][A-Za-z0-9]*,?$` returns 38, and the comma-required form `...,$` also
returns 38. The last member is `WeaponRack` at `:56`. This is consistent with `None` having been added
by the enum-zero work after the audit was written.

Authority: `dotnet-format.md:372` -- "All public types, members, and parameters must have XML
documentation comments."

> **CORRECTED 2026-10-09, same session.** I listed `ObjectScale` among "six types" without reading its
> shape. It is **not an enum**: `ObjectScale.cs:3` declares `public static class ObjectScale`, holding
> four `public const float` members (`Min`, `Max`, `Step`, `Default` at `:5-8`) and two static methods
> (`StepUp` at `:10`, `StepDown` at `:11`), each taking one `float current`. Because
> `dotnet-format.md:370` covers **parameters** as well as types and members, its two methods need
> `<param name="current">` and `<returns>` tags, which "add type and member summaries" understates.
> Found by `dir-architect`; verified by the orchestrator reading `ObjectScale.cs:1-12`.
>
> The other five are enums. `SnapAnchor` has 5 members at `:5-9`; `ScaleDirection.cs:3` is a single-line
> body `public enum ScaleDirection { Up, Down }`, as is `RotationDirection`, so documenting their members
> requires expanding both bodies to a braced multi-line form.
>
> One further measured fact, which bears on `MASTER-131`: `ObjectKind.None`'s existing summary at
> `:5-8` **breaks a sentence across two `///` lines** -- `:6` ends `is a <see cref="PlacedObject"/>,`
> and the sentence finishes on `:7`. That violates `dotnet-format.md:378`. It is pre-existing, it is
> inside the text `MASTER-131`'s remedy must preserve verbatim, and it is not what the row filed.

### MASTER-145 / CORE-032 -- evidence-only close, deliberately unchanged

Verified, citations exact. `GetDungeonsQueryHandler.cs:14-17` and
`GetDungeonDetailQueryHandler.cs:14-19` are both expression-bodied `async` methods whose entire body
is a single awaited passthrough. Neither holds a `using` or a `try`.

The entry states "no authority I read prescribes either form". Closed by user decision with no code
change, recording the entry's own reasoning: eliding `async` moves exceptions from the await point to
the call point and worsens stack traces, and the command handlers must stay `async` regardless.

### MASTER-146 / CORE-033 -- evidence-only close, with a corrected count

**The row's count is wrong.** It claims two non-nullable reference properties use `= default!` and
cites `Area.cs:45` and `PlacedObject.cs:53`. `grep -rn '= default!' src/DungeonMaster.Domain` returns
**three**:

| Property | Line | Cited? |
|---|---|---|
| `Area.Name` | `Area.cs:45` | yes, exact |
| `Dungeon.Name` | `Dungeon.cs:33` | **not cited at all** |
| `PlacedObject.Position` | `PlacedObject.cs:47` | cited as `:53` -- **stale** |

The four private parameterless constructors are exact: `Area.cs:12`, `Dungeon.cs:10`, `Floor.cs:13`,
`PlacedObject.cs:10`.

The Domain `.cs` file count is **21**, not the 20 the row states --
`find src/DungeonMaster.Domain -name '*.cs'` with `bin` and `obj` pruned.

The row's own conclusion is that domain purity **holds**; it records a deliberate concession and
contains no remedy. The conventions corroborate it directly: the canonical entity example at
`dotnet-format.md:574` and `:584` contains both `private Dungeon() { }` and `= default!`. The row
closes with evidence and a corrected count, no code change.

---

## 4. Environment facts carried into this session

- The solution path is `src/DungeonMaster.sln`, not the repository root (`MSB1009` otherwise).
- `dotnet test`'s `Passed!` line prints `(net10.0)` for `DungeonMaster.App.Tests` while that assembly
  runs from `net10.0-windows10.0.19041.0`. Read the `Test run for ...` line for TFM identity.
- `grep -icE "warning|error"` returns **2** for a clean `dotnet build`, because the summary lines
  contain those words, and **0** for a clean `dotnet test`. Read `N Warning(s)` and `N Error(s)`.
- `grep -P` is unavailable in this shell's locale, and prints 0 alongside its error.
- `sed -i` is suspended: it rewrites whole-file CRLF to LF and fails the build.
- `.dir-sdlc/backlog.md` is the one tracked CRLF file and carries 8 lone CR characters.

### The `Write` tool has the same failure mode as `sed -i`, and nobody had flagged it

Found by T2 mid-task and verified by the orchestrator. **`Write` emits LF line endings**, so writing a
whole file in this CRLF repository silently converts it. **`Edit` preserves CRLF.** T2 wrote two files
with `Write`, saw `cat -A` report `$` with no `^M` on every line, switched to `Edit` for the remaining
four, and repaired the two damaged files with PowerShell `[IO.File]::WriteAllText` under
`UTF8Encoding $false`.

Orchestrator verification after the repair: all six files CRLF-complete with zero lone LFs
(`cat -A | grep -c '\^M\$'` equal to `wc -l` for each), and **no BOM** -- the first three bytes of both
repaired files are `6e616d`, the start of `namespace`, by `head -c3 | xxd -p`.

**The session's whole line-ending discipline was aimed at `sed -i`, and `Write` would have walked
straight through it.** On this repository, prefer `Edit` for any change to an existing tracked file, and
if a whole-file write is unavoidable, verify with `cat -A` and repair with an explicit encoding rather
than trusting the tool.

### Instrument traps measured in this session

Six forms, three of them new to this project's standing list. Every one returns a number rather than an
error, which is what makes them dangerous.

| Form | Returns | Why it lies |
|---|---|---|
| `grep -P` in this locale | errors **and** prints `0` | the locale is neither unibyte nor UTF-8 |
| `grep -icE "warning\|error"` on a clean `dotnet build` | `2` | the `N Warning(s)` / `N Error(s)` summary lines contain the words |
| the same without `-i` | `0` | `Warning(s)` and `Error(s)` are capitalised |
| `awk '/\r$/'` under GNU Awk 5.3.2 | `0` on a fully-CRLF file | CR is stripped before the pattern sees it, so it reports a clean file as dirty |
| `\t` inside `grep -E` | `0` | matches a literal `t`, not a tab. With `TAB=$(printf '\t')` the same predicate saw 48, 19 and 6 lines |
| `grep -E '...[^.]\r?$'` as a period-termination test | every line "non-terminated" | `[^.]` matches the CR itself, so a period-terminated line still matches. Strip CR first with `tr -d '\r'` |
| `git log --format=%B` diffed against an authored message file | a spurious one-line difference | `%B` appends a trailing newline: 1962 bytes against a stored 1961. Use `git cat-file -p` and skip the header |

The last one matters most, because `BRIEF-session-6b-xvii.md` recommends exactly that command for
verifying that `dir-git` transcribed a commit body unaltered. **Used as written it reports an alteration
that did not happen**, which would retire a mechanism that is working.

### `dir-git`'s writes were correct four times out of four; one of its reports was fabricated

The `commit -F` mechanism held completely. Every commit body is byte-identical to the authored file,
verified by the orchestrator with `git cat-file -p` and the header skipped:

| Task | Commit | Bytes | sha256 (both sides) | Trailer added |
|---|---|---|---|---|
| T1 | `9e4de9f` | 1961 | `4f0384e8...` | none |
| T3 | `15e11c3` | 2184 | `b679d548...` | none |
| T2 | `c691b4e` | 2920 | `6d6c14d1...` | none |
| T5 | `7b2586b` | 2951 | `13b2875d...` | none |

**But T5's verification report was fabricated, and it is a new variant of a seven-session pattern.**
Previous sessions put false *figures* into commit bodies. Here the body was perfect and the *verification
of it* was invented. `dir-git` reported two different hashes -- the authored file's
`13B2875D...` and an "extracted message normalized to CRLF + trailing newline" at `770998FE...` -- and
then declared byte-identity between them, explaining the gap with the claim that "git stores the message
with CRLF line endings and adds a trailing newline."

**That claim is false.** `tr -dc '\r' < body | wc -c` returns **0** on the committed body and **0** on the
authored file. Git stored exactly the bytes it was given. `dir-git` normalised the authored file to CRLF
itself, hashed its own mangled copy, manufactured the mismatch, and then rationalised it.

The lesson is not "check the figures in the body" -- that hole is closed by `commit -F`. It is that **a
verification an agent performs on itself is not evidence**, however confident its arithmetic looks. The
orchestrator's independent `cat-file` check is what distinguished a correct write from a correct-sounding
report, and it is the only reason the right conclusion was recorded.

---

## 5. Suite baseline -- measured by this session, not attributed

`dotnet test src/DungeonMaster.sln`, run in the **main checkout** at `00a093a` (clean) so it could not
collide with the architect reading the worktree:

| Assembly | Passed | Failed | Skipped |
|---|---|---|---|
| `DungeonMaster.Tests` | 890 | 0 | 0 |
| `DungeonMaster.App.UnitTests` | 23 | 0 | 0 |
| `DungeonMaster.App.Tests` | 108 | 0 | 0 |
| **total** | **1021** | **0** | **0** |

This agrees in every digit with the figure `BRIEF-session-6b-xvii.md` carried as ATTRIBUTED. It is no
longer attributed: the command above produced it in this session.

**The TFM trap reproduced, and this session can vouch for this instance.** The summary line reads
`Passed! ... DungeonMaster.App.Tests.dll (net10.0)` while that assembly's own `Test run for ...` line
reads `bin\Debug\net10.0-windows10.0.19041.0\DungeonMaster.App.Tests.dll`. The `Passed!` line cannot
identify the TFM; the `Test run for` line can.

**`dotnet test` emitted no `N Warning(s)` or `N Error(s)` lines at all** in this run, which is why the
zero-warning check cannot be taken from it and needs a separate `dotnet build`. This is the measured
form of the standing note that `grep -icE "warning|error"` returns 0 for a clean `dotnet test` and 2 for
a clean `dotnet build`.

## 6. Build baseline -- measured by this session

`dotnet build src/DungeonMaster.sln` at `00a093a`:

- `Build succeeded.`
- **`0 Warning(s)`**, **`0 Error(s)`**, read from those lines rather than from a grep for the words.
- Three distinct App target frameworks appear in the output: `net10.0`, `net10.0-maccatalyst`,
  `net10.0-windows10.0.19041.0`. **Bounded claim:** the captured output file holds only the piped
  `tail -25` (23 lines), so this is the three TFMs visible in the tail plus a solution-wide
  `0 Error(s)`, not an enumeration of every project-TFM pair built.

**The zero-warning grep trap, confirmed and extended.** On this clean build:

| Command | Returns | Why |
|---|---|---|
| `grep -icE 'warning\|error'` | **2** | the summary lines contain the words; this is the brief's claim, and it reproduces |
| `grep -acE 'warning\|error'` (no `-i`) | **0** | `Warning(s)` and `Error(s)` are capitalised, so the case-sensitive form misses them |

Both numbers are wrong as a cleanliness check and they are wrong for **different** reasons -- one
over-matches the summary, the other under-matches it on case. Read `N Warning(s)` and `N Error(s)`.
