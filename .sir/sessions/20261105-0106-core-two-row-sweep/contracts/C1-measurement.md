<!-- frozen: 20261105-0106-core-two-row-sweep, Gate 2 -->

# C1 -- Measurement instruments and startRef baselines

This contract declares no callable operation. No task consumes code another task defines; the session
moves members between files and re-breaks comment lines. C1 exists so every instrument a criterion
names is available verbatim to every later phase. Scratch copies made during the spec phase do not
survive it.

Every baseline below was measured by the architect in the spec phase, on the session worktree
`C:/dm-wt/20261105-0106-core-two-row-sweep` at `11f9562ce0f60577abc41911984a5dbeb6cb4c10` with a
clean tree (the orchestrator's `git status --short`, handed to the architect). Paths are relative to
the worktree root.

## 0. How to run these

- Save each awk program below into **your own scratch directory** and run it as
  `awk -f <that file> <target>`. Never save one into the worktree.
- Every control that needs a fixture or a perturbed copy runs on files in **your own scratch
  directory**. No control is ever injected into a worktree file: peers share that tree.
- `sha256sum` here prints the digest followed by ` *-`. Compare the 64 hex digits only.
- Before you trust an empty or unchanged result from an instrument, recreate its fixture and confirm
  the stated output. A result from an instrument whose control was not re-run is not evidence.
- awk in this shell may drop CR on input. Every instrument here is CR-insensitive by construction
  (it strips CR itself), and none is used to judge line endings. Section 7 judges line endings.

## 1. P2 -- a `///` line that ends mid-sentence

Verbatim from section 2 of `.sir/sessions/20261103-1947-core-doc-comments/contracts/C1-measurement.md`.

```
function txt(l,  t){ t=l; sub(/\r$/,"",t); if (t !~ /^[ \t]*\/\/\/($|[^\/])/) return "\001"; sub(/^[ \t]*\/\/\/[ \t]*/,"",t); sub(/[ \t]+$/,"",t); return t }
function tagonly(t){ return t ~ /^<[^<>]*>$/ }
{ L[NR]=$0 }
END { for (i=1;i<NR;i++){ a=txt(L[i]); b=txt(L[i+1]);
  if (a=="\001"||a==""||tagonly(a)) continue
  if (a ~ /[.!?]$/) continue
  if (b=="\001"||b==""||tagonly(b)) continue
  print FILENAME ":" i } }
```

It prints the first line of each pair of consecutive description lines (non-empty, not tag-only)
where the first does not end in `.`, `!` or `?`. Blind spot: a break placed after a period that does
not end the sentence (`e.g.`). Section 2 covers it.

Fixtures (CRLF; `printf` writes them, `\t` is a real tab):

- fixture1 = `printf '/// <summary>\r\n///\t\tThis sentence is broken\r\n///\t\tmid clause here.\r\n/// </summary>\r\n/// <remarks>\r\n///\t\tA whole sentence.\r\n///\t\tAnother one.\r\n/// </remarks>\r\n'` -- P2 prints only line 2.
- fixture2 = `printf '///\t\tA thing <see cref="X"/>\r\n///\t\tcontinues here.\r\n/// </summary>\r\n'` -- P2 prints only line 1.
- fixture3 = `printf '///\t\tSee the guide, e.g.\r\n///\t\tthe second chapter.\r\n///\t\tA new sentence.\r\n'` -- P2 prints **nothing** (its blind spot).

All three were run in the spec phase and gave exactly these outputs.

## 2. S -- a description line that continues a sentence (P2's second predicate)

```
function txt(l,  t){ t=l; sub(/\r$/,"",t); if (t !~ /^[ \t]*\/\/\/($|[^\/])/) return "\001"; sub(/^[ \t]*\/\/\/[ \t]*/,"",t); sub(/[ \t]+$/,"",t); return t }
function tagonly(t){ return t ~ /^<[^<>]*>$/ }
{ L[NR]=$0 }
END { for (i=2;i<=NR;i++){ a=txt(L[i-1]); b=txt(L[i]);
  if (a=="\001"||a==""||tagonly(a)) continue
  if (b=="\001"||b==""||tagonly(b)) continue
  if (b ~ /^[a-z]/) print FILENAME ":" i } }
```

It prints the second line of each pair of consecutive description lines whose text begins with a
lowercase letter, which a new sentence never does. It shares no test with P2: P2 reads the end of the
first line, S the start of the second. Blind spot: a continuation that begins with a tag or a capital
(`<see cref=...>`, a proper noun). P2 covers the first of those. Controls, run in the spec phase:
fixture1 -- S prints only line 3; fixture2 -- S prints only line 2; fixture3 -- S prints only line 2.

## 3. W -- doc-comment word stream ("no words changed")

```
{ t=$0; sub(/\r$/,"",t)
  if (t !~ /^[ \t]*\/\/\/($|[^\/])/) next
  sub(/^[ \t]*\/\/\/[ \t]*/,"",t); sub(/[ \t]+$/,"",t)
  n=split(t,w,/[ \t]+/); for (i=1;i<=n;i++) if (w[i]!="") print w[i] }
```

Run: `awk -f W.awk <file> | sha256sum`, and `awk -f W.awk <file> | wc -l` for the token count. It is
the precedent's W with the `<para>`-skip removed (this session skips nothing).

Controls, run in the spec phase on scratch copies of `src/DungeonMaster.App/Services/Navigation/ShellRoutes.cs`:
- must pass: join `:4` and `:5` into one `///` line (a reflow, no word changed) -- the digest is
  unchanged (`56623335...`), and P2 no longer prints `4`;
- must fire: change `declared` to `declard` on `:5` -- the digest changes (`ec38fd41...`).

## 4. K -- every line that is not a `///` line ("nothing but `///` lines changed")

```
tr -d '\r' < <file> | grep -v '^[[:space:]]*///' | sha256sum
```

It hashes, in order, every line that is not a `///` line: code, blank lines and `//` comments. It is
stronger than the precedent's N, which also dropped `//` comments.

Controls, run in the spec phase on scratch copies of `src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs`:
- must pass: join `:49` and `:50` (a `///` reflow) -- unchanged (`77277b53...`);
- must fire (comment): on `:303`, change `bare 3` to `bare 4` -- changes (`0f0faba9...`). The
  precedent's N (`grep -v '^[[:space:]]*//'`) is **blind** to this: it printed `8eb46771...` both before
  and after;
- must fire (code): on `:305`, change `Spacing = DmTheme.SpaceXS` to `Spacing = DmTheme.SpaceS` --
  changes (`957be918...`).

## 5. I1 -- description-line indentation (`dotnet-format.md:375`)

Verbatim from section 4 of the precedent contract.

```
{ t=$0; sub(/\r$/,"",t)
  if (t !~ /^[ \t]*\/\/\/($|[^\/])/) next
  sub(/^[ \t]*\/\/\//,"",t)
  body=t; sub(/^[ \t]+/,"",body); sub(/[ \t]+$/,"",body)
  if (body=="" || body ~ /^<[^<>]*>$/) next
  pre=t; sub(/[^ \t].*$/,"",pre)
  if (pre !~ /^\t\t+$/) print FILENAME ":" NR }
```

Fixture: `printf '///\t\tGood line.\r\n/// Bad line with space.\r\n///    \tMixed.\r\n/// <summary>\r\n///\t\t<see cref="X"/> starts with tag.\r\n'`
-- prints only lines 2 and 3 (run in the spec phase).

## 6. L -- indentation before `///`

```
{ t=$0; sub(/\r$/,"",t); isdoc = (t ~ /^[ \t]*\/\/\/($|[^\/])/)
  pre=t; sub(/[^ \t].*$/,"",pre)
  if (isdoc) { n++; P[n]=pre; N[n]=NR; next }
  for (i=1;i<=n;i++) if (P[i]!=pre) print FILENAME ":" N[i]
  n=0 }
END { for (i=1;i<=n;i++) print FILENAME ":" N[i] }
```

It prints each `///` line whose leading whitespace differs from that of the first non-`///` line after
its run (the declaration it documents). Fixture:
`printf '\t/// <summary>\r\n\t///\t\tFine.\r\n///\t\tShifted.\r\n\t/// </summary>\r\n\tpublic int X;\r\n'`
-- prints only line 3 (run in the spec phase).

## 7. B -- line-ending byte census (FORMs 1, 5, 7, 13, 15)

```
od -An -tx1 -v <file> | tr -s ' ' '\n' | awk 'NF{ if($1=="0d")cr++; if($1=="0a"){lf++; if(p=="0d")pair++} p=$1 } END{printf "CR=%d LF=%d CRLF=%d loneCR=%d\n",cr,lf,pair,cr-pair}'
```

Control: `printf 'a\r\nb\rc\n'` prints `CR=2 LF=2 CRLF=1 loneCR=1` (run in the spec phase). A file passes
when CR = LF = CRLF and loneCR = 0. Never judge line endings with `grep $'\r'`, `sed -n`, `grep -E` with a
file argument, or `git show` output.

BOM check: `head -c 3 <file> | od -An -tx1`. A file starting `using` prints ` 75 73 69`; one starting
`namespace` prints ` 6e 61 6d`. A UTF-8 BOM would print ` ef bb bf`.

## 8. M -- member-line multiset across an entity's partial files

```
cat <files> | tr -d '\r' | grep -vE '^(using |namespace |public sealed (partial )?class |[{}]$|$)' | LC_ALL=C sort | sha256sum
```

Replace `| LC_ALL=C sort | sha256sum` with `| wc -l` for the line count. It drops each file's header and
type-shell lines (`using`, `namespace`, the column-0 type declaration, column-0 braces) and blank lines,
then hashes the sorted multiset of everything else -- every member line and every doc-comment line.
Moving members between an entity's partial files leaves it unchanged; adding, deleting or altering any
member line changes it. **Blind spot, by design:** reordering lines leaves it unchanged. Section 9
covers order.

Regex control (FORM 3, every branch): the fixture
`printf '\t{\r\n\tpublic Guid FloorId { get; private set; }\r\npublic sealed partial class Floor : Entity\r\n{\r\n}\r\nusing X;\r\nnamespace Y;\r\n\r\npublic sealed class PlacedObject : Entity\r\n'`
piped through `tr -d '\r' | grep -nvE '<the pattern above>'` prints only lines 1 (`\t{`) and 2
(`\tpublic Guid FloorId ...`), and the `wc -l` form prints 2. Lines 3-9 exercise every branch,
including the declaration with and without `partial`. Run in the spec phase.

| Population | files, in this order | lines | sha256 |
| --- | --- | --- | --- |
| Floor, at startRef | `Floor.cs` `FloorMethods.cs` `FloorObjectMethods.cs` | 320 | `f3b9a065d6206de9afc57d873e888cc69fc7b629210dd514ffad4808a02d5e4d` |
| Floor, required after T1 | `Floor.cs` `FloorMethods.cs` | 320 | `f3b9a065d6206de9afc57d873e888cc69fc7b629210dd514ffad4808a02d5e4d` |
| PlacedObject, at startRef | `PlacedObject.cs` | 94 | `ace0efdfcdba2bf95e9b45eb0a42d0b06c5c34c1641119029f6ab70e1ab9d0ea` |
| PlacedObject, required after T1 | `PlacedObject.cs` `PlacedObjectMethods.cs` | 94 | `ace0efdfcdba2bf95e9b45eb0a42d0b06c5c34c1641119029f6ab70e1ab9d0ea` |

All files are under `src/DungeonMaster.Domain/Entities/Floors/`. The "required after T1" rows were
measured on the C2 target files the architect built in scratch, and they agree with section 9.

Controls, run in the spec phase on scratch copies of the C2 target files:
- must fire (deletion): delete the `Touch();` line of `RemoveObject` (target `FloorMethods.cs:235`) --
  changes (`2549334b...`);
- must fire (accessibility): change `private static void ValidateScale` to `internal static void
  ValidateScale` in the target `PlacedObjectMethods.cs` -- changes (`ba4d0e7b...`);
- blind, by design: swap `_objects.Add(placedObject);` and `Touch();` in `AddObject` (target `:208-209`)
  -- **unchanged** (`f3b9a065...`). Section 9 fires on it.

## 9. E -- exact content, line endings stripped

```
tr -d '\r' < <file> | sha256sum
```

Required values after T1, for the C2 target composition (all under `src/DungeonMaster.Domain/Entities/Floors/`):

| File | lines (`wc -l`) | sha256 |
| --- | --- | --- |
| `FloorMethods.cs` | 282 | `d92063f00c74c5ca8448b9cf15c9385282a51fd7e6cec82f142bd44209d9f499` |
| `PlacedObject.cs` | 63 | `6f324b807ac0f5216a3490cec2c42a898f0c95f5976b9d8781d9a96bed2ca788` |
| `PlacedObjectMethods.cs` | 55 | `a6c0697ede8e61b5736a66bc3542afd6ab4035343aceba6886d08d5a6a305c9f` |

At startRef the values differ: `FloorMethods.cs` `9b9cc01a...` (176 lines), `PlacedObject.cs`
`9ab84b7a...` (112 lines), and `PlacedObjectMethods.cs` does not exist. `Floor.cs` is `0efd28d8...` (94 lines)
and stays so; no task owns it.

Controls, run in the spec phase: the swap that M cannot see changes E (`a57b756f...`); the deletion
changes E (`12c63a92...`). The architect also compiled the three target files: a scratch copy of
`src/DungeonMaster.Domain` (with the root `.editorconfig`, `Directory.Build.props`,
`Directory.Packages.props`, `global.json`, `nuget.config`), `FloorObjectMethods.cs` removed and the
three target files installed CRLF, built with `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`. Its must-fire
control, the target `PlacedObjectMethods.cs` with `partial` removed, failed with `CS0260`.

## 10. PC -- partial-declaration census

```
grep -n partial src/DungeonMaster.Domain/Entities/*/*.cs
```

At startRef it prints exactly 7 lines (control: `Floor.cs:8` must appear; `PlacedObject.cs` must not):
`Areas/Area.cs:8`, `Areas/AreaMethods.cs:5`, `Dungeons/Dungeon.cs:6`, `Dungeons/DungeonMethods.cs:5`,
`Floors/Floor.cs:8`, `Floors/FloorMethods.cs:5`, `Floors/FloorObjectMethods.cs:5`. Required after T1:
exactly these 8 lines, in this order (all under `src/DungeonMaster.Domain/Entities/`), measured on the
scratch build of section 9:

```
Areas/Area.cs:8:public sealed partial class Area : Entity
Areas/AreaMethods.cs:5:public sealed partial class Area
Dungeons/Dungeon.cs:6:public sealed partial class Dungeon : Entity
Dungeons/DungeonMethods.cs:5:public sealed partial class Dungeon
Floors/Floor.cs:8:public sealed partial class Floor : Entity
Floors/FloorMethods.cs:5:public sealed partial class Floor
Floors/PlacedObject.cs:8:public sealed partial class PlacedObject : Entity
Floors/PlacedObjectMethods.cs:5:public sealed partial class PlacedObject
```

## 11. Doc-reflow baselines at startRef

| File | P2 | S | W tokens | W sha256 | K sha256 | I1 | L | B |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `src/DungeonMaster.Application/Common/Results/ResultOfT.cs` | 27 32 33 | 34 | 406 | `fb6eccd476aa7e06db746e5f621497604e104e5767bf4b9c5d56e97d52b29551` | `7fd75c7f1120a28645204e2d892f48f96b73ad198171f13518dd14202e177962` | none | none | 144 each, loneCR 0 |
| `src/DungeonMaster.App.Core/Map/AreaOutlineGeometry.cs` | 6 12 22 26 76 77 78 79 141 | 7 13 23 77 78 79 80 142 | 440 | `52b288989b1e3f532beb7ff44a0596cb3baa81054872f55eb68762f2504d1863` | `fcfd92b71446f6ae7ea0f32450cff2b83265fc8d3825d30b5907f465fd2861e1` | none | none | 205 each, loneCR 0 |
| `src/DungeonMaster.App/Services/Navigation/ShellRoutes.cs` | 4 10 16 | 5 11 17 | 74 | `56623335833fe17cd996158f6040b4c0d29193d57fd72db0e8a6ba857fc4bd38` | `7a8b8ec45ca0b4cd15850777b3b7affb289315ebb2a1036bb0965c715f15d111` | none | none | 20 each, loneCR 0 |
| `src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs` | 49 50 309 310 | 50 311 | 302 | `b05485a3a81397cfc352f80e19e4aa02b9a27028e0d3fe230d3632925659fbc1` | `77277b534d14f2406f711f6fd45e0ba8d640cfd4e54599286d441220e2836669` | none | none | 412 each, loneCR 0 |

None of the four files starts with a BOM (section 7). Reachability: the architect built, in scratch, one
conforming reflow of each file (every sentence-splitting run joined into one line). On all four, P2, S,
I1 and L printed nothing, and W and K printed exactly the digests above.

P2, S, I1 and L print nothing on `Floor.cs`, `FloorObjectMethods.cs` and `PlacedObject.cs` at
startRef, and P2 prints nothing on `FloorMethods.cs`. I1 prints `Floor.cs:38` and `FloorMethods.cs:13`:
single-line `<exception>` elements, pre-existing and out of scope. C2 preserves them byte for byte.

## 12. R -- register-row cell count, and the amendment censuses

```
tr -d '\r' < docs/audit/00-master-index.md | awk -v row=<N> 'NR==row{ s=$0; n=0; for(i=1;i<=length(s);i++){ c=substr(s,i,1); if(c=="|" && substr(s,i-1,1)!="\\") n++ } print n }'
```

It counts the `|` characters on line N that do not follow a `\`. Control: `printf '| a | b \\| c |\n'`
at row 1 prints 3. At startRef it prints 11 on lines 297, 298, 299 and 368. Line 298 begins
`| 110 | MASTER-110 |` and ends `| CORE-022 | |` (an empty status cell).

For `docs/audit/01-domain-application.md`:
- opener census `tr -d '\r' < <file> | grep -c '^\*\*\[AMENDED'` prints 14 at startRef. It matches `:2885`
  (`**[AMENDED 20261103-1947-core-doc-comments, ...`). It does not match the near-misses `:1093` and
  `:1101` (`**AMENDED 6b-viii`, `**AMENDED 6b-ix`, no bracket).
- closer census `tr -d '\r' < <file> | grep -c '\]\*\*$'` prints 14 at startRef. It matches `:2916`
  (ends `` `T1`.**]** ``).

Anchors at startRef: `### CORE-022` heading `:1537`; its `**Suggested direction.**` paragraph `:1572-1575`;
blank `:1576`; `### CORE-023` heading `:1577`. The stale figures are on `:1567` ("`Floor.cs` at 73 lines is
genuinely readable next to 305 lines of behaviour") and again in the layer verdict at `:2940`.
`## Layer verdict` is `:2918`.

B at startRef: `docs/audit/00-master-index.md` CR=691 LF=691 CRLF=691 loneCR=0;
`docs/audit/01-domain-application.md` CR=2950 LF=2950 CRLF=2950 loneCR=0.

## 13. Build and test (FORM 8; lesson 20261029-1708)

- `dotnet build C:/dm-wt/20261105-0106-core-two-row-sweep/src/DungeonMaster.sln`. Quote its three summary
  lines: `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`. Never use a `grep -c` over "warning|error".
- `dotnet test C:/dm-wt/20261105-0106-core-two-row-sweep/src/DungeonMaster.sln --no-build`, run after
  that build. Quote each assembly's summary line. The orchestrator probed it runnable at startRef with
  `DungeonMaster.App.UnitTests.dll` 187, `DungeonMaster.Tests.dll` 953 and `DungeonMaster.App.Tests.dll` 123
  passed, 0 failed. Output that names no test count is reported as not runnable, not as a pass.
- Both commands write one shared `obj/` and `bin/` tree, so **two tasks never run either at the same
  time**. The manifest orders every building task (T1, T2, T3) in sequence through `dependsOn`.
- A build may rewrite a tracked `packages.lock.json` (lessons 2026-11-02, 2026-11-04). No task owns one.
  Never modify or revert one; report it if `ls -l` or the build output shows it changed.
- No build file sets `GenerateDocumentationFile`, so the build checks no `cref`. Every `cref` here
  is moved or re-broken, never added or changed.

## 14. Editing hazards (bind every task, and every reviewer who touches a file)

- No `sed -i` and no `perl -i`, ever. Each rewrites CRLF to LF.
- No `Write` on any file in the worktree. It emits LF, and an LF `.cs` file fails the build under
  `IDE0055`.
- `Edit` keeps CRLF terminators. It strips trailing whitespace (spaces and tabs) from `new_string`, so
  never end a `new_string` on whitespace, and key each edit on a line's first text character.
- `Edit` normalises embedded lone CRs. Every file this session edits has loneCR = 0 at startRef
  (sections 11 and 12), so this is safe here. Confirm with B after the last edit.
- A new `.cs` file is made CRLF from birth. One safe route: `cp` a CRLF sibling to the new path, then
  shape it with `Edit` (C2 section 3). Prove the result with B and the BOM check.
- Indent with tabs, never spaces.
