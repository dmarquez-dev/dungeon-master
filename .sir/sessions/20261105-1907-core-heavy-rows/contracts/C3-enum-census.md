<!-- frozen: 20261105-1907-core-heavy-rows, Gate 2 -->

# C3 -- MASTER-178 / CORE-035: the enum census and the per-enum dispositions

No callable operation. T3 consumes this contract and owns no source file: every disposition below is
"deliberately unchanged", so no code change follows from it. Baselines are at startRef `62ba566` (C1 header).

## 1. The census instrument (verbatim; the orchestrator wrote it in the spec phase)

It walks `<root>/src`, skips `obj/` and `bin/` and the four test projects (`DungeonMaster.Tests`,
`DungeonMaster.App.Tests`, `DungeonMaster.App.UnitTests`, `DungeonMaster.App.UiTests`), strips block comments,
line and doc comments and string literals from each `.cs` file, then matches `enum Name [: type] { ... }`,
drops attributes from the body, honours explicit `= n`, and reports which member sits at zero.

Recreate it byte for byte as `census.py` in your scratch directory: the code block below, from `import os` to
the final `"NO_ZERO", ...))` line, with tab indentation and a trailing newline. Check
`sha256sum census.py` prints `2a10830e5ac1569f5f542c99ae17802a805290e4f4e1e1fae5500d3aadb5b05a` before using it.
Run: `python -I census.py C:/dm-wt/20261105-1907-core-heavy-rows`.

```python
import os, re, sys
root = sys.argv[1]
TESTS = ('DungeonMaster.Tests', 'DungeonMaster.App.Tests', 'DungeonMaster.App.UnitTests', 'DungeonMaster.App.UiTests')


def strip(s):
	s = re.sub(r'/\*.*?\*/', '', s, flags=re.S)
	s = re.sub(r'//[^\n]*', '', s)
	s = re.sub(r'"(?:\\.|[^"\\])*"', '""', s)
	return s


rows = []
for dp, dn, fn in os.walk(os.path.join(root, 'src')):
	dn[:] = [d for d in dn if d not in ('obj', 'bin')]
	parts = os.path.normpath(dp).split(os.sep)
	if any(t in parts for t in TESTS):
		continue
	for f in fn:
		if not f.endswith('.cs'):
			continue
		p = os.path.join(dp, f)
		t = strip(open(p, encoding='utf-8-sig').read())
		for m in re.finditer(r'\benum\s+(\w+)\s*(?::\s*\w+)?\s*\{([^}]*)\}', t):
			name, body = m.group(1), m.group(2)
			body = re.sub(r'\[[^\]]*\]', '', body)
			members = [x.strip() for x in body.split(',') if x.strip()]
			val = -1
			zero = None
			explicit = False
			for mem in members:
				if '=' in mem:
					n, v = [y.strip() for y in mem.split('=', 1)]
					explicit = True
					try:
						val = int(v, 0)
					except ValueError:
						val = None
				else:
					n = mem
					val = (val + 1) if val is not None else None
				if val == 0 and zero is None:
					zero = n
			rel = os.path.relpath(p, root).replace(os.sep, '/')
			rows.append((name, zero, explicit, rel))
for r in sorted(rows):
	print(f"{r[0]}\tzero={r[1]}\texplicit={r[2]}\t{r[3]}")
print("TOTAL", len(rows),
	"NONE_AT_ZERO", sum(1 for r in rows if r[1] == 'None'),
	"CONCRETE_AT_ZERO", sum(1 for r in rows if r[1] not in ('None', None)),
	"NO_ZERO", sum(1 for r in rows if r[1] is None))
```

**Reading its rows.** Each row prints `zero=<member>`. The script prints Python's `None` for an enum with no
member at zero, so a row reading `zero=None` is ambiguous between "a member named `None` sits at zero" and "no
member is zero". The TOTAL line is not ambiguous (it compares against the string `'None'`), and on this tree it
prints `NO_ZERO 0`, so every `zero=None` row below is a member named `None`. The fixture's `Explicit` row is the
other case.

## 2. Census control fixture

Recreate it with this script (`python -I make_fixture.py <scratch>/fx`); it writes LF files:

```python
import os, sys
root = sys.argv[1]
files = {
    'src/Prod/F.cs': (
        '/// <summary>A, B, and C.</summary>\n'
        'public enum MustFlag { /// <summary>x, y</summary>\n'
        ' Alpha, Beta }\n'
        'internal enum MustNotFlag { None, Some }\n'
        'class K { private enum Hidden : byte { [Obsolete("a,b")] First = 0, Second } }\n'
        'enum Explicit { A = 1, B = 2 }\n'
        'enum ExplicitZeroLater { A = 1, B = 0 }\n'
        '// enum InComment { X }\n'
    ),
    'src/DungeonMaster.Tests/T.cs': 'public enum TestOnly { A }\n',
}
for rel, text in files.items():
    p = os.path.join(root, rel)
    os.makedirs(os.path.dirname(p), exist_ok=True)
    with open(p, 'wb') as f:
        f.write(text.encode('utf-8'))
```

`sha256sum` of the two files: `src/Prod/F.cs` `3c22f24e85b700d99d83b46ae2846162b6f31e0365b3f59baad125b8c15302cc`,
`src/DungeonMaster.Tests/T.cs` `3a97ab43e0e59f26c8e35bc5c508ddfa901d34b53d325493bdbf5d53572cfbed`.

What each case controls: a comma inside a doc comment (`MustFlag`, must still be split into exactly two members);
a `None` at zero (`MustNotFlag`); a nested `private` enum with a `: byte` base and an attribute containing a comma
(`Hidden`); explicit values with no zero (`Explicit`); explicit values with zero not first (`ExplicitZeroLater`);
an enum only inside a comment (`InComment`, must not appear); a test-project enum (`TestOnly`, must not appear).

`python -I census.py <scratch>/fx` prints exactly (tab-separated). Python on Windows writes CRLF to a pipe, so
hash it CR-stripped: `python -I census.py <scratch>/fx | tr -d '\r' | sha256sum` prints
`db60df1b3f0d35513572960c642ee4a555a21ac2c7becad99f20586059864758`:

```
Explicit	zero=None	explicit=True	src/Prod/F.cs
ExplicitZeroLater	zero=B	explicit=True	src/Prod/F.cs
Hidden	zero=First	explicit=True	src/Prod/F.cs
MustFlag	zero=Alpha	explicit=False	src/Prod/F.cs
MustNotFlag	zero=None	explicit=False	src/Prod/F.cs
TOTAL 5 NONE_AT_ZERO 1 CONCRETE_AT_ZERO 3 NO_ZERO 1
```

## 3. Census output on the worktree at startRef

`python -I census.py C:/dm-wt/20261105-1907-core-heavy-rows` prints exactly this (CR-stripped digest, as above,
`8d1d455164802c1cc7495043f0983bf0b7c00de2e4d7734d03a5b9a26470d929`; the architect reproduced the orchestrator's
run):

```
BehaviorCursorShape	zero=Hand	explicit=False	src/DungeonMaster.App/Behaviors/BehaviorCursorShape.cs
CanvasCursorShape	zero=Arrow	explicit=False	src/DungeonMaster.App/Services/Platform/CanvasCursorShape.cs
DrawSessionResult	zero=None	explicit=False	src/DungeonMaster.App.Core/Map/MapDrawingSession.cs
DrawState	zero=Idle	explicit=False	src/DungeonMaster.App.Core/Editor/DrawState.cs
ErrorCategory	zero=Validation	explicit=False	src/DungeonMaster.Application/Common/Results/ErrorCategory.cs
FloorDropOwner	zero=None	explicit=False	src/DungeonMaster.App.Core/Editor/FloorReorderNearest.cs
FloorListBandKind	zero=FloorCard	explicit=False	src/DungeonMaster.App.Core/Editor/FloorReorderNearest.cs
MapTool	zero=Draw	explicit=False	src/DungeonMaster.App.Core/Editor/MapTool.cs
MutationOutcome	zero=None	explicit=False	src/DungeonMaster.Domain/Entities/MutationOutcome.cs
NotebookKind	zero=Dungeon	explicit=False	src/DungeonMaster.App/Controls/NotebookView/NotebookKind.cs
NotebookPane	zero=Description	explicit=False	src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs
ObjectKind	zero=None	explicit=False	src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs
ObjectRotation	zero=None	explicit=False	src/DungeonMaster.Domain/Entities/Floors/ObjectRotation.cs
PathSegmentKind	zero=Line	explicit=False	src/DungeonMaster.Domain/ValueObjects/PathSegmentKind.cs
RotationDirection	zero=Clockwise	explicit=False	src/DungeonMaster.Domain/Entities/Floors/RotationDirection.cs
ScaleDirection	zero=Up	explicit=False	src/DungeonMaster.Domain/Entities/Floors/ScaleDirection.cs
SnapAnchor	zero=Center	explicit=False	src/DungeonMaster.Domain/Entities/Floors/SnapAnchor.cs
TOTAL 17 NONE_AT_ZERO 5 CONCRETE_AT_ZERO 12 NO_ZERO 0
```

T1 renames three `ObjectRotation` members during this session and leaves `None` at zero, so this output is the
same before and after T1. Predicate check on the real tree: `ObjectRotation` must fall in the `None` bucket and
does; `PathSegmentKind` must fall in the concrete bucket (`Line`) and does; the nested `private enum NotebookPane`
must be found and is.

**Second predicate** (line-based, cannot share the parser's mistakes, FORM 16 / lesson 2026-10-16):

```
grep -rnE '^[[:space:]]*(public|internal|private|protected)?[[:space:]]*(sealed[[:space:]]+)?enum[[:space:]]+[A-Za-z]' C:/dm-wt/20261105-1907-core-heavy-rows/src --include=*.cs --exclude-dir=obj --exclude-dir=bin | grep -v '/src/DungeonMaster.Tests/\|/src/DungeonMaster.App.Tests/\|/src/DungeonMaster.App.UnitTests/\|/src/DungeonMaster.App.UiTests/'
```

prints **17** lines (`| wc -l`) at startRef, one per enum name in the table above, `NotebookPane` included. The two
counts agree. FORM 3 control for the `\|` exclusion: the same pipeline with `grep -v` replaced by `grep` prints
exactly one line, `.../src/DungeonMaster.Tests/App/AsyncDiscipline/AsyncDisciplineScanner.cs:288:public enum AsyncVoidUnitKind`
(the test-project enum CORE-035 names), so the exclusion fires and removes only that line (run in the spec phase).

## 4. Disposition of the twelve concrete-at-zero enums

The guideline (`https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum`, CORE-035's citation)
admits `None = 0` or "the most common default value" at zero. Gate 1's rule: if `default(T)` is actually reachable
**and** is a defect, add a `None` member (with a data migration if persisted); otherwise close the enum deliberately
unchanged with a stated reason. The architect read each enum's producers and stored fields at startRef; every
citation below was re-read in the spec phase and is to be re-read by T3. **Every one of the twelve is DELIBERATELY
UNCHANGED; none needs a fix task.**

| # | Enum (zero member) | Where `default` could arise | Evidence at startRef | Disposition and reason |
| --- | --- | --- | --- | --- |
| 1 | `PathSegmentKind` (`Line`) | a deserialised outline segment | `PathSegment` is a `record` class (`PathSegment.cs:28`) whose `Kind` initializer validates every value (`:38`, `:43-88`, throwing for an undefined one at `:82-85` [corrected from `:83-85` by 20261118-0052-c3-census-citation, 2026-11-18]). It is persisted by ordinal in the `Areas.Outline` JSON column (`AreaConfiguration.cs:16`, `new(JsonSerializerDefaults.General)`; JC, C1 section 3, prints nothing). `PathSegmentJsonRoundTripTests.cs` pins `"Kind": 0` as `Line` (the raw-JSON lines `:28-29`). | Unchanged. `Line` is the most common segment and a valid one, so a `0` read from storage is correct data, not a leaked default. Adding `None = 0` would reinterpret every stored outline (a JSON data migration) to fix no defect. |
| 2 | `RotationDirection` (`Clockwise`) | the `RotateRequested` event argument | Raised only at `ControlsPaletteView.xaml.cs:27` (`CounterClockwise`) and `:28` (`Clockwise`), both explicit; consumed by `==` at `DungeonDetailPage.xaml.cs:139` and `:150`. No field or property stores the type. | Unchanged. `default` is never produced; every value comes from an explicit producer. |
| 3 | `ScaleDirection` (`Up`) | the `ScaleRequested` event argument | Raised only at `ControlsPaletteView.xaml.cs:29` (`Down`) and `:30` (`Up`); consumed by `==` at `DungeonDetailPage.xaml.cs:166`. No stored field. | Unchanged, for the same reason as row 2. |
| 4 | `SnapAnchor` (`Center`) | a computed anchor | The only production producer is `ObjectKindInfo.GetDefaultAnchor` (`ObjectKindInfo.cs:40`), whose every path returns an explicit member (`:52` `East`, `:56` `South`, `:58` `Center` as the terminal fallback). `grep -rn 'GetDefaultAnchor(' src --include=*.cs --exclude-dir=obj --exclude-dir=bin` outside `DungeonMaster.App.UnitTests/` prints only that declaration: it has no production caller. | Unchanged. `Center` is the repository's own stated fallback (CORE-035's class C reclassification) and no `default` is produced. |
| 5 | `MapTool` (`Draw`) | the active tool | The only stored `MapTool` is `MapCanvasViewModel.ActiveTool`, initialised `= MapTool.Pan` (`MapCanvasViewModel.cs:47`); `MapEditorState.SetActiveTool` takes it as a parameter (`MapEditorState.cs:148`) and stores none. | Unchanged. The default is never observed: the one stored field is initialised explicitly. |
| 6 | `DrawState` (`Idle`) | `MapEditorState.DrawState` | `public DrawState DrawState { get; private set; }` has no initializer (`MapEditorState.cs:42`), so `default` **is** reached at construction; it is then set explicitly (`:154-156`, `:162`, `:188-190`). | Unchanged, deliberately: the reachable default is `Idle`, which is the correct initial state, so it is the guideline's "most common default" branch, not a defect. |
| 7 | `NotebookKind` (`Dungeon`) | `NotebookTarget.Kind`, `NotebookSaveArgs.Kind` | Both carriers are `record` classes (`NotebookTarget.cs:15`, `NotebookSaveArgs.cs:14`), so their default is `null`, not a zero `Kind`; every producer passes an explicit kind (`DungeonDetailPage.xaml.cs:673` `Dungeon`, `:988` `Floor`, `:997` `Area`). | Unchanged. `default` is never produced. |
| 8 | `NotebookPane` (`Description`) | the selected pane | `private enum` (`NotebookView.xaml.cs:17`); every call passes an explicit pane (`:159`, `:203`, `:346` `Description`; `:349` `Notes`); no stored field of the type. | Unchanged. Private, never defaulted, and `Description` is the pane the view opens on (`:159`, `:203`). |
| 9 | `CanvasCursorShape` (`Arrow`) | a cursor request | No stored field. `DungeonDetailPage.xaml.cs:1129` maps every unlisted tool to `CanvasCursorShape.Arrow` (`_ =>` arm), and the Windows mapping falls back to the system arrow (`PlatformCanvasCursor.Windows.cs`, the `_ =>` arm of `ToSystemShape`). | Unchanged. `Arrow` is the ordinary cursor and the code's own fallback, so a zero `Arrow` is the most common default. |
| 10 | `BehaviorCursorShape` (`Hand`) | `HandCursorBehavior.Shape` | Initialised `= BehaviorCursorShape.Hand` (`HandCursorBehavior.cs:16`). | Unchanged. The property is initialised explicitly, and `Hand` is the behaviour's namesake default. |
| 11 | `FloorListBandKind` (`FloorCard`) | `default(FloorListBand)` | `FloorListBand` is a `readonly record struct` (`FloorReorderNearest.cs:88`), so `default` is constructible; it has `Height` 0, and `Resolve` skips every band with `Height <= 0` in both passes (`:166`, `:187`). The one producer sets the kind explicitly (`DungeonDetailPage.xaml.cs:2412-2413`). | Unchanged. A defaulted band is ignored by construction, so its zero kind is never acted on. |
| 12 | `ErrorCategory` (`Validation`) | a failed result's category | Modelled as `ErrorCategory?` (`Result.cs:31`, `ResultOfT.cs:49`), so "no category" is `null`, not zero; failures are built with an explicit category. | Unchanged. Absence is `null`, so the zero value never stands for "unset". |

Citations that drifted since CORE-035 was written, measured in the spec phase: `ResultOfT.cs:39` (CORE-035's
`ErrorCategory` citation) is now `:49`; `PathSegmentJsonRoundTripTests.cs:26-28` is now `:28-29` for the two raw-JSON
`"Kind"` lines; CORE-035's `DungeonDetailPage.xaml.cs:140`, `:151`, `:167` are now `:139`, `:150`, `:166`; and its
`ControlsPaletteView.xaml.cs:14-17` producers are now `:27-30` (`:14` and `:19` are the event declarations).

**Superseded framings.** CORE-035 records `PathSegmentKind` as **blocked** and `RotationDirection` / `ScaleDirection`
as an open **class B design question**. Gate 1 decided the question by reachability: a zero member is owed only
where the default is reachable and wrong, and row 1-3 above show it is not. The blocker was real for *adding* a
zero member; with no zero member owed, nothing is blocked.

If T3's re-read finds any citation in the table false, or a default reachable and defective, T3 stops and reports
a contract-change escalation; it does not change a disposition on its own.
