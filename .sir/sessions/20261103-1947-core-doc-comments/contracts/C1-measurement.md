# C1 -- Measurement instruments and startRef baselines

Session `20261103-1947-core-doc-comments`. Frozen at Gate 2. This contract declares no API
operation: the session changes comment text only, and no task consumes anything another task
defines. It exists so every later phase has the instruments verbatim. Scratch copies made during
the spec phase do not survive it.

All baselines below were measured by the architect on the worktree at
`d2b84a249baf8d3e12c16e7cadbd6c96d16a4564` (the session's base commit, tree clean by
`git -C C:/dm-wt/20261103-1947-core-doc-comments status --short`, which printed nothing).
Paths are relative to the worktree root unless absolute.

## 0. How to run these

- Save each awk program below into **your own scratch directory** as a file and run it as
  `awk -f <that file> <target>`. Never save one into the worktree.
- Every control below that needs a fixture or an injected copy is run on files in **your own
  scratch directory**. No control is ever injected into a file in the worktree. Peers share
  that tree.
- `sha256sum` on this machine prints the digest followed by ` *-`. Compare the 64 hex digits only.
- Before you trust a zero from P1, P2 or I1, recreate that instrument's fixture and confirm the
  stated output. A zero from an instrument whose control was not re-run is not evidence.

## 1. P1 -- mid-sentence `///` break, `>` exempt (CORE-036's predicate)

```
function txt(l,  t){ t=l; sub(/\r$/,"",t); if (t !~ /^[ \t]*\/\/\/($|[^\/])/) return "\001"; sub(/^[ \t]*\/\/\/[ \t]*/,"",t); sub(/[ \t]+$/,"",t); return t }
function tagonly(t){ return t ~ /^<[^<>]*>$/ }
{ L[NR]=$0 }
END { for (i=1;i<NR;i++){ a=txt(L[i]); b=txt(L[i+1]);
  if (a=="\001"||a==""||tagonly(a)) continue
  if (a ~ /[.!?>]$/) continue
  if (b=="\001"||b==""||tagonly(b)) continue
  print FILENAME ":" i } }
```

## 2. P2 -- the same, with no `>` exemption

Identical to P1 except that the line `if (a ~ /[.!?>]$/) continue` becomes
`if (a ~ /[.!?]$/) continue`:

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

P1 is blind to a sentence broken after an inline tag such as `<see cref="x"/>`. P2 sees it. P2 also
flags any description line ending in a character other than `.`, `!` or `?`, such as `:` or a closing
quote. A sentence whose period sits inside a closing quote (`."`) is therefore a P2 hit. End such
sentences with the period outside the quote, as the startRef text already does.

### P1/P2 fixtures (CRLF; write them with `printf`, `\t` is a real tab, `\r\n` the terminator)

fixture1 = `printf '/// <summary>\r\n///\t\tThis sentence is broken\r\n///\t\tmid clause here.\r\n/// </summary>\r\n/// <remarks>\r\n///\t\tA whole sentence.\r\n///\t\tAnother one.\r\n/// </remarks>\r\n'`
-- P1 prints only line 2. P2 prints only line 2.

fixture2 = `printf '///\t\tA thing <see cref="X"/>\r\n///\t\tcontinues here.\r\n/// </summary>\r\n'`
-- P1 prints nothing. P2 prints only line 1.

Both were run in the spec phase and gave exactly these outputs.

### P1/P2 at startRef (line numbers only)

| File | P1 | P2 |
|---|---|---|
| `src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs` | 4 9 10 11 12 16 17 18 20 21 23 24 28 29 30 41 42 43 60 61 62 63 64 92 | P1's list plus 19 |
| `src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs` | 50 97 98 102 | 50 97 98 102 |
| `src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs` | 10 | 10 |
| `src/DungeonMaster.Application/Common/Results/ResultOfT.cs` | 27 32 33 37 38 39 | 27 32 33 37 38 39 |
| `src/DungeonMaster.Domain/Entities/Areas/Area.cs` | none | none |
| `src/DungeonMaster.App.Core/Map/AreaOutlineGeometry.cs` | 6 12 22 26 73 74 75 76 138 | 6 12 22 26 73 74 75 76 138 |

`CanvasPoint.cs:19` ends `while <see cref="float.PositiveInfinity"/>`, and its sentence continues on
`:20`. It is a true positive that P1 misses. CORE-036's claim that `:19` conforms is false.

## 3. W -- doc-comment word stream (proves "no words changed")

Prints every whitespace-separated token of every `///` line's text in order. With `-v skip=N`, it
drops the Nth line whose text is exactly `<para>` through the next line whose text is exactly
`</para>`, both included. `skip=0` drops nothing.

```
{ t=$0; sub(/\r$/,"",t)
  if (t !~ /^[ \t]*\/\/\/($|[^\/])/) next
  sub(/^[ \t]*\/\/\/[ \t]*/,"",t); sub(/[ \t]+$/,"",t)
  if (t=="<para>") { pc++; if (skip>0 && pc==skip) { skipping=1; next } }
  if (skipping) { if (t=="</para>") skipping=0; next }
  n=split(t,w,/[ \t]+/); for (i=1;i<=n;i++) if (w[i]!="") print w[i] }
```

Run: `awk -v skip=N -f W.awk <file> | sha256sum`, and `| wc -l` for the token count.

Baselines at startRef:

| File | skip | tokens | sha256 |
|---|---|---|---|
| `CanvasPoint.cs` | 2 | 364 | `369908f9deb957076ee746477736ba2d543bed6a55ba24675cdb94f8ad60b8e7` |
| `PlacedObject.cs` | 0 | 183 | `a8b33b0483cd5646adbf7eea0b9e2213a0c5c337bd60f9ba26ec1799ebe85d46` |
| `ObjectKind.cs` | 0 | 49 | `3620232a9195b0ebdbdabdbf8e898e9316f59e485b80230e3b41eb056f6ca10d` |
| `ResultOfT.cs` | 2 | 333 | `24a636802bdee9b01c85205e4c022f704f7beac51503c335bfe5de0e75cbdc80` |

`CanvasPoint.cs` skip=2 drops the type remarks' second `<para>`, at startRef `:15-26`, which is D3's
rewrite. `ResultOfT.cs` skip=2 drops `Value`'s second `<para>`, at startRef `:36-41`, which is D2's
rewrite. At startRef, `<para>` lines are `CanvasPoint.cs:8, :15, :27` and `ResultOfT.cs:31, :36`.

Controls, run in the spec phase on copies of `PlacedObject.cs`:
- Join `:50` and `:51` into one `///` line, which is a reflow with no word changed. The hash is
  unchanged (`a8b33b04...`), and P1 no longer prints `50`.
- Change `expressed` to `expresssed` on `:51`. The hash changes (`97bb116d...`).

## 4. I1 -- description-line indentation (encodes `dotnet-format.md:375`)

Prints each `///` line whose text is not empty and not a single tag, and where the run between
`///` and the text is anything other than two or more tabs.

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
-- prints only lines 2 and 3. At startRef I1 prints nothing over `CanvasPoint.cs`, `PlacedObject.cs`,
`ObjectKind.cs`, `ResultOfT.cs` and `AreaOutlineGeometry.cs`.

## 5. N -- non-comment lines unchanged (proves "no executable code changed")

```
tr -d '\r' < <file> | grep -v '^[[:space:]]*//' | sha256sum
```

This drops every line whose first non-space characters are `//`, which covers both `//` and `///`.
It hashes everything else, including blank lines and code. Line endings are stripped first, so this
hash says nothing about CRLF. Section 6 measures that.

Baselines at startRef:

| File | non-comment lines | sha256 |
|---|---|---|
| `src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs` | 32 | `9867de75592e3048725aea9bf74400371754fbe2f0fa8ea531e02211a006715f` |
| `src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs` | 66 | `d155f683f2dee2899f7916a003bf6cd619d8d4ead96a70ef228d574b06b5c2f0` |
| `src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs` | 48 | `3ad22314b05778aed4c47ba38054f9b91bdc68569d657b8564021d9912e27f09` |
| `src/DungeonMaster.Application/Common/Results/ResultOfT.cs` | 68 | `7fd75c7f1120a28645204e2d892f48f96b73ad198171f13518dd14202e177962` |
| `src/DungeonMaster.Domain/Entities/Areas/Area.cs` | 34 | `e9fbfd5cdd8cf5655d353499264a7d52059f4f82ab6ebf8c8d325ef0620a6e42` |
| `src/DungeonMaster.App.Core/Map/AreaOutlineGeometry.cs` | 116 | `17bde18ebebad37a9ef46550d990b31f3fb16196084998c0e789c5b0d7b1950f` |

Controls, run in the spec phase on copies of `Area.cs` in scratch:
- Must pass: on `:24`, change `at most once per Area` to `at most once per Area under single-threaded
  access`. The hash is unchanged (`e9fbfd5c...`).
- Must fire: on `:25`, change `_outlineView;` to `_outlineViewX;`. The hash changes (`9902b7e3...`).

## 6. B -- line-ending byte census (FORM 1, 7, 13 and 15)

```
od -An -tx1 -v <file> | tr -s ' ' '\n' | awk 'NF{ if($1=="0d")cr++; if($1=="0a"){lf++; if(p=="0d")pair++} p=$1 } END{printf "CR=%d LF=%d CRLF=%d loneCR=%d\n",cr,lf,pair,cr-pair}'
```

Control: `printf 'a\r\nb\rc\n'` prints `CR=2 LF=2 CRLF=1 loneCR=1`. This was run in the spec phase.

A file passes when CR = LF = CRLF and loneCR = 0. Startref values (every file 100% CRLF, loneCR 0):
`CanvasPoint.cs` 116, `PlacedObject.cs` 115, `ObjectKind.cs` 61, `ResultOfT.cs` 145, `Area.cs` 77,
`AreaOutlineGeometry.cs` 202, `docs/audit/00-master-index.md` 689, `docs/audit/01-domain-application.md`
2917.

## 7. D2 survey -- which closed `Result<T>` constructions production code makes

Production means `src/` excluding `/obj/` and the four `src/*Tests/` projects.

S1, the orchestrator's predicate:
```
grep -rnoE '(^|[^A-Za-z0-9_])Result<[A-Za-z_.?]+' src --include=*.cs | grep -v '/obj/' | grep -vE '^src/[^/]*Tests/'
```
At startRef it prints `Result<Guid` (10), `Result<T` (6) and `Result<TResult` (14), tallied with
`sed -E 's/.*(Result<[A-Za-z_.?]+)/\1/' | sort | uniq -c`. Over the test projects
(`grep -E '^src/[^/]*Tests/'`) the same predicate prints `Result<Guid` (36) and `Result<int` (14). The
orchestrator's controls returned 0 on `src/DungeonMaster.App.Tests/Graphics/FakeAssetSource.cs`, which
holds the near-miss `Task.FromResult<Stream>`, and 5 on `ResultOfT.cs`.

S2 is a second census that S1 cannot share a mistake with. It counts closures made through type
arguments, which S1 cannot see. `DependencyInjection.cs:67` closes `Result<>` at runtime over each
`ICommandHandler<,>`'s second type argument.
```
grep -rnoE "ICommandHandler<[A-Za-z_]+, *[A-Za-z_.?<>]+>" src --include=*.cs | grep -v /obj/ | grep -vE '^src/[^/]*Tests/'
grep -rnoE "ICommand<[A-Za-z_.?]+>" src --include=*.cs | grep -v /obj/ | grep -vE '^src/[^/]*Tests/'
grep -rnoE "(Send|Dispatch)Async<[^>(]*>" src --include=*.cs | grep -v /obj/ | grep -vE '^src/[^/]*Tests/'
```
At startRef, every closed argument these print is `Guid`. The commands print:
- 4 `ICommandHandler<XCommand, Guid>` lines plus the open `ICommandHandler<TCommand, TResult>` in
  `Dispatcher.cs`.
- 4 `ICommand<Guid>` and 8 open `ICommand<TResult>`.
- 5 `SendAsync<Guid>` and 2 open `SendAsync<TResult>`.

`IQueryHandler<,>` returns a bare `TResult`, not a `Result<T>` (`IQueryHandler.cs:27`). All `Result<`
tokens in production number 31 (`grep -rnoF 'Result<'`, same exclusions): S1's 30 plus one open
`typeof(Result<>)`.

## 8. X -- `<exception>` elements in `Translate`'s doc block

```
awk '/Translate\(/{exit} /<exception cref=/{print NR": "$0}' src/DungeonMaster.App.Core/Map/AreaOutlineGeometry.cs
```
It prints every `<exception cref=` line before the first `Translate(`, which at startRef is the
declaration at `:32`. The only doc block above that line holding `<exception>` is `Translate`'s. At
startRef it prints `:25` (`ArgumentNullException`) and `:29` (`ArgumentException`). The file holds one
other `ArgumentOutOfRangeException` `<exception>`, at `:82`, which belongs to another member and is
outside this scan.

## 9. R -- register-row cell count

```
tr -d '\r' < docs/audit/00-master-index.md | awk 'NR==368{ s=$0; n=0; for(i=1;i<=length(s);i++){ c=substr(s,i,1); if(c=="|" && substr(s,i-1,1)!="\\") n++ } print n }'
```
This counts the `|` characters on line 368 that do not follow a `\`. At startRef it prints 11 on lines
367, 368 and 369 alike, which is 10 cells. Line 368 begins `| 180 | MASTER-180 |`.

Amendment-block census for `docs/audit/01-domain-application.md`:
- `tr -d '\r' < <file> | grep -c '^\*\*\[AMENDED'` prints 13 at startRef. It matches `:235`
  (`**[AMENDED 6b-v, ...`). It does not match the near-miss `:1093` (`**AMENDED 6b-viii, ...`, no
  bracket).
- `tr -d '\r' < <file> | grep -c '\]\*\*$'` prints 13 at startRef. Closers include `:262` and `:415`.

CORE-036 starts at `:2791`. Its `**Suggested direction.**` paragraph starts at `:2878`, and
`## Layer verdict` is at `:2885`.

## 10. Build and test (FORM 8; lesson of 20261029-1708)

- `dotnet build C:/dm-wt/20261103-1947-core-doc-comments/src/DungeonMaster.sln`. Quote its three
  summary lines: `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`. Never use a `grep -c` over
  "warning|error".
- `dotnet test C:/dm-wt/20261103-1947-core-doc-comments/src/DungeonMaster.sln --no-build`. Quote each
  assembly's summary line. A run whose output names no test count is not runnable, not a pass. The
  orchestrator probed this command runnable at dispatch, with per-assembly totals 187, 943 and 139.
- The solution has no `GenerateDocumentationFile` (`Directory.Build.props` and every `*.csproj`, by
  `grep -rn GenerateDocumentationFile`), so the build does not validate `cref` targets. Any `cref` a task
  adds is checked by reading.
- Both commands build one shared `obj/` and `bin/` tree, so two tasks must never run them at the same
  time.

## 11. Editing hazards (bind every task, and every reviewer who touches a file)

- No `sed -i`, ever. It rewrites CRLF to LF.
- No `Write` on an existing file. It emits LF.
- `Edit` keeps CRLF terminators. It strips trailing whitespace, spaces and tabs alike, from
  `new_string`. Never end a `new_string` on whitespace. Key the edit on a line's first text character.
- `Edit` normalises embedded lone CRs. Every target file has loneCR = 0 at startRef (section 6), so this
  is safe here. Confirm with B after editing.
- Indent with tabs, never spaces. A description line is `///` followed by tabs, matching its siblings
  (I1).
