# C1 — Citations and claims in FloorMapDrawableNonFiniteTests.cs

Contract for task T1. One file changes, and only its `///` lines:
`src/DungeonMaster.App.Tests/Graphics/FloorMapDrawableNonFiniteTests.cs` (below, "the test file").
Every figure here was measured by the architect in the worktree
`C:/dm-wt/20261116-2027-nonfinite-test-citations` at `40dddd1` (HEAD
`40dddd19dcf494430190a310e392fe198b1bffd6`), with the command named beside it. Every
`file:line` came from `grep -n` at `40dddd1`. Production paths below are relative to the
worktree root. Abbreviations: DDP = `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`,
FMD = `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs`.

## 1. Editing rules (binding on T1)

1. Only lines whose text after leading whitespace starts with `///` may change. Lines may be
   rewritten in place; a `///` line may be added or removed. No other line of the test file and
   no other file changes.
2. The test file is tracked CRLF. Never `sed -i` and never the Write tool on it. Use `Edit` only.
   Never end a `new_string` on whitespace (Edit strips trailing spaces and tabs). Key each edit on
   text that starts at the line's first text character after `///` and its two tabs, or include
   the whole line from `///` (leading tabs survive Edit).
3. Every rewritten `///` content line keeps the project format (dm-conventions
   `backend/dotnet-format.md` §9, rules at lines 375 and 378): description lines are indented
   `///` + two tabs, and a line ends at a sentence boundary — it ends with `.`, or `.</item>` for
   a list item. Never break a sentence across `///` lines.
4. No `.sir/` path, no backlog line number, and no session id in the test file. The only
   provenance text allowed is the commit label `as numbered at <c>40dddd1</c>`.
5. The citation label is the literal text `as numbered at <c>40dddd1</c>` (XML doc comments have
   no backtick markup). Every `///` line that carries a citation token (§2) also carries this
   label; one label may cover several tokens on its line ("both ...", "all three ...").

## 2. Citation census (P-CITE), counted in citations, not lines

**Predicate.** On each line whose text after leading whitespace starts with `///`, every match of
the Python regex

```
(?:[A-Za-z0-9_./]+\.(?:cs|xaml))?:[0-9]+(?:-[0-9]+)?
```

is one citation token: an optional file name ending `.cs` or `.xaml` (with any path), a colon,
digits, and an optional `-digits` range. Bare continuations such as `<c>:513</c>` are citations.
A `<see cref="..."/>` is not a citation and does not match (it carries no `:digits`). A file name
without a line number (`FloorMapDrawableExecutionTests.cs` at test-file line 9) does not match.

**Controls** (program in §6, mode `controls` of census.py there; output observed by the architect):
must-match `<c>:513</c>`, `<c>Foo.cs:629-630</c>`, `<c>x/Y.xaml:263</c>` all matched; near-misses
`[0.25, 4.0]`, `Foo.cs` (no number) and `10:x` (no digit after the colon) did not:
`CONTROL [':513', 'Foo.cs:629-630', 'x/Y.xaml:263']`.

**Second predicate (P-COLON)**, no regex: count each `:` immediately followed by an ASCII digit
on `///` lines, by character walk. It cannot share P-CITE's file-name mistakes.

**Base count at `40dddd1`:** P-CITE **26** citations on **14** lines (8, 13, 31, 40, 41, 42, 43,
44, 45, 47, 51, 52, 64, 118); P-COLON **26**. The two agree. The discussion's
`grep -n -E '[A-Za-z]+\.(cs|xaml)?:[0-9]+|:[0-9]{2,}'` printed 14 lines, which is the same line set.

No prose line-reference without a colon exists: `grep -n -o -E '(line|lines|item|items) [0-9]+'`
on the test file printed only `38:items 3` (an in-file reference to list items, §4 F4).

## 3. Citation table (26 rows)

Columns: row; test-file line at `40dddd1`; old token; what it points to at `40dddd1` and the
evidence; required new token; needles that must appear on the same `///` line as the new token
(besides the label). "Points to" names the member and statement per decision E1. The needles and
tokens are checked by P-ANCHOR (§6). Every new token is in the form given (path form unchanged
from the old token: full path stays full path, file name stays file name, bare stays bare).

| Row | Line | Old token | Points to at `40dddd1` (evidence) | New token | Needles |
|---|---|---|---|---|---|
| R1 | 8 | `FloorMapDrawable.cs:629-630` | `ScreenToCanvas`'s expression body `new((screen.X - PanOffset.X) / ZoomLevel,` / `(screen.Y - PanOffset.Y) / ZoomLevel);` (FMD :647-648, `grep -n -E 'ScreenToCanvas' FMD`: decl :646) | `FloorMapDrawable.cs:647-648` | `ScreenToCanvas`, `subtract` |
| R2 | 8 | `DungeonDetailPage.xaml.cs:1437` | `var placeCanvas = _drawable.ScreenToCanvas(placeRaw);` in `OnMapStartInteraction`'s Place-tool branch (DDP :1595; method decl :1483) | `DungeonDetailPage.xaml.cs:1595` | `OnMapStartInteraction`, `placeCanvas` |
| R3 | 8 | `DungeonDetailPage.xaml.cs:1442` | `new CanvasPoint(placeCanvas.X, placeCanvas.Y),`, the position argument to `PlaceObjectAsync` (DDP :1600) | `DungeonDetailPage.xaml.cs:1600` | `PlaceObjectAsync`, `new CanvasPoint(placeCanvas.X, placeCanvas.Y)` |
| R4 | 13 | `DungeonDetailPage.xaml.cs:1437` | same site as R2 | `DungeonDetailPage.xaml.cs:1595` | `OnMapStartInteraction` |
| R5 | 31 | `DungeonDetailPage.xaml.cs:1442` | same site as R3; claim corrected per F2 | `DungeonDetailPage.xaml.cs:1600` | `PlaceObjectAsync`, `ArgumentOutOfRangeException`, `ErrorMessage` |
| R6 | 40 | `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:439` | `public float ZoomLevel { get; set; } = 1.0f;` (FMD :455) | `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:455` | `ZoomLevel`, `= 1.0f` |
| R7 | 41 | `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:442` | `public PointF PanOffset { get; set; } = PointF.Zero;` (FMD :458) | `src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:458` | `PanOffset`, `= PointF.Zero` |
| R8 | 42 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:520` | `_drawable.ZoomLevel = ZoomLevelGuard.Next(_drawable.ZoomLevel, raw);` in `ApplyZoom` (DDP :578) | DDP + `:578` (full path) | `ApplyZoom`, `ZoomLevelGuard.Next` |
| R9 | 42 | `:518` | `private void ApplyZoom(float raw)` (DDP :576) | `:576` | `ApplyZoom` |
| R10 | 42 | `:513` | `ApplyZoom(_drawable.ZoomLevel + ZoomStep);` in `OnZoomInClicked` (decl :570), unfenced (DDP :571) | `:571` | `OnZoomInClicked` |
| R11 | 42 | `:516` | `ApplyZoom(_drawable.ZoomLevel - ZoomStep);` in `OnZoomOutClicked` (decl :573), unfenced (DDP :574) | `:574` | `OnZoomOutClicked` |
| R12 | 42 | `:528` | `ApplyZoom(1.0f);` in `ResetViewport`, unfenced (DDP :586) | `:586` | `ResetViewport` |
| R13 | 42 | `:617` | `ApplyZoom(_drawable.ZoomLevel + direction * ZoomStep);` in `OnMapScrollWheel` (decl :669) (DDP :675) | `:675` | `OnMapScrollWheel` |
| R14 | 42 | `:606-619` | the class-level `#if WINDOWS` region (directive :664) up to its `#elif MACCATALYST` (:677), which declares `_scrollZoomSubscription` and `OnMapScrollWheel`; it sits between `DetachScrollZoom` and `OnViewModelPropertyChanged` and is not inside any member | `:664-677` | `#if WINDOWS` |
| R15 | 42 | `:631` | `ApplyZoom(_pinchStartZoom * (float)e.Scale);` in `OnMapPinchUpdated` (decl :681) (DDP :689) | `:689` | `OnMapPinchUpdated` |
| R16 | 42 | `:619-635` | the class-level `#elif MACCATALYST` arm (:677) up to `#endif` (:693), which declares `_pinchGesture`, `_pinchStartZoom` and `OnMapPinchUpdated` | `:677-693` | `#elif MACCATALYST` |
| R17 | 43 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:528` | `ApplyZoom(1.0f);` in `ResetViewport` (DDP :586) | DDP + `:586` (full path) | `ResetViewport`, `ApplyZoom(1.0f);` |
| R18 | 43 | `:525` | `private void ResetViewport()` (DDP :583) | `:583` | `ResetViewport` |
| R19 | 44 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:508` | `_drawable.PanOffset = PointF.Zero;` in `ResetViewport` (DDP :585; `grep -n -F` prints it once). Note: :508 was right when written — at `4031fc8`, the commit that added the comment (`git log -S'xaml.cs:1437'` on the test file), DDP :508 is `_drawable.PanOffset = PointF.Zero;` inside `ResetViewport` (:505); the remarks' later `ZoomLevel` correction renumbered list items 3 and 4 (`:500` and `:507` at `4031fc8`, per `git show 4031fc8:<test file> \| grep -n -o -E 'xaml.cs:5[0-9][0-9]'`) but not item 5, which then drifted (orchestrator, `git show 4031fc8:<DDP> \| grep -n`) | DDP + `:585` (full path) | `ResetViewport`, `_drawable.PanOffset = PointF.Zero;` |
| R20 | 45 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1632-1634` | `_drawable.PanOffset = new PointF(` / `_drawable.PanOffset.X + delta.X,` / `_drawable.PanOffset.Y + delta.Y);` in `OnMapDragInteraction`'s Pan branch (decl :1754) (DDP :1790-1792) | DDP + `:1790-1792` (full path) | `OnMapDragInteraction` |
| R21 | 47 | `FloorMapDrawableExecutionTests.cs:39-40` | unchanged: `PanOffset = new PointF(10f, 20f),` / `ZoomLevel = 2f,` in the object initializer of `ScreenToCanvasInvertsPanAndZoom` (decl :35) | `FloorMapDrawableExecutionTests.cs:39-40` | `ScreenToCanvasInvertsPanAndZoom` |
| R22 | 51 | `FloorMapDrawable.cs:98` | unchanged: `public sealed class FloorMapDrawable : IDrawable, IDisposable` | `FloorMapDrawable.cs:98` | `sealed class FloorMapDrawable : IDrawable, IDisposable` |
| R23 | 52 | `Resources/Theme/Tokens.xaml:263` | unchanged: `Feature-specific: FloorMapDrawable`, the section-banner XML comment at `src/DungeonMaster.App/Resources/Theme/Tokens.xaml:262-264` | `Resources/Theme/Tokens.xaml:263` | `Feature-specific: FloorMapDrawable` |
| R24 | 52 | `:267` | unchanged: prose in the separate XML comment at :266-271 above `<Color x:Key="DmMapAreaStroke">` (:272) | `:267` | `DmMapAreaStroke` |
| R25 | 64 | `FloorMapDrawable.cs:629-630` | same site as R1 | `FloorMapDrawable.cs:647-648` | `ScreenToCanvas` |
| R26 | 118 | `DungeonDetailPage.xaml.cs:1442` | same site as R3; claim corrected per F3 | `DungeonDetailPage.xaml.cs:1600` | `PlaceObjectAsync`, `ArgumentOutOfRangeException` |

Evidence commands (all run at `40dddd1` from the worktree root):
`grep -n -E 'ZoomLevel|PanOffset|ScreenToCanvas|sealed class|BindableProperty' FMD`;
`grep -n -E 'ScreenToCanvas|CanvasPoint|ZoomLevel|PanOffset|#if|#elif|#else|#endif|void [A-Za-z]+\(' DDP`
(plus the `private ... (` alternative for method headers);
`grep -n -E 'var placeCanvas|new CanvasPoint\(placeCanvas|_editor.PanAnchorScreen = new CanvasPoint\(current|var touchPoint = new CanvasPoint' DDP`;
`grep -n -E 'ScreenToCanvasInvertsPanAndZoom|PanOffset = new PointF\(10f, 20f\)|ZoomLevel = 2f' src/DungeonMaster.App.Tests/Graphics/FloorMapDrawableExecutionTests.cs`;
`grep -n -E '<!--|-->'` and `grep -n 'x:Key="DmMapAreaStroke"'` on `src/DungeonMaster.App/Resources/Theme/Tokens.xaml`.

**How the old :1437 and :1442 were resolved** (the comment never quoted their text). Confirmed
from history by the orchestrator: at `4031fc8`, the commit that wrote the comment, DDP :1437 is
`var placeCanvas = _drawable.ScreenToCanvas(placeRaw);` and :1442 is
`new CanvasPoint(placeCanvas.X, placeCanvas.Y),` (`git show 4031fc8:<DDP> | grep -n -E 'ScreenToCanvas|new CanvasPoint\('`).
The architect reached the same answer from three facts, without git: (a) the comment says the call at :1437 "feeds the conversion" at :1442 that "persists as a
`CanvasPoint`", so :1437 is a `ScreenToCanvas` call whose result is wrapped in a `new CanvasPoint`;
of the 8 `ScreenToCanvas(` calls in DDP (:1545 :1595 :1661 :1693 :1776 :1802 :1814 :2070), only
:1595 (→ :1600) and :1776 (→ :1779) feed a `new CanvasPoint(`; (b) the gap 1442 − 1437 = 5 matches
:1595→:1600 and not :1776→:1779 (3); (c) the pan accumulator in the same handler region moved by
the same +158 (:1632 → :1790), and 1437 + 158 = 1595, 1442 + 158 = 1600. The `#if` spans and
`ApplyZoom` sites likewise moved by a common +58, and the FMD properties by +16, the
`ScreenToCanvas` body by +18; every row was mapped by content first, the offsets only corroborate.

## 4. Claims re-checked

### 4.1 Claims found true at `40dddd1` and kept (K). Reviewer re-runs each command and gets the stated output.

In this table every `\|` is an escaped `|` (markdown cell delimiter); type it as a bare `|` when
running the command. The commands run from the worktree root in Git Bash.

| Id | Test-file line(s) | Claim | Command (worktree root) and output at `40dddd1` |
|---|---|---|---|
| K1 | 32 | MAUI Windows pointer delivery of a non-finite coordinate is undecidable from source | Not decidable from source; stays stated as undecided. Needles `is not decidable from this repository` and `is NOT asserted here` each count 1 at base (P-CLAIM) and must remain ≥ 1 |
| K2 | 17, 42 | `ApplyZoom` holds the only production `ZoomLevel` write; callers :571 :574 :586 unfenced, :675 in the `#if WINDOWS` region, :689 in the `#elif MACCATALYST` arm | `grep -rn "ZoomLevel" src/ --include=*.cs --include=*.xaml \| grep -Ev "[\\/](obj\|bin)[\\/]" \| grep -v FloorMapDrawableNonFiniteTests.cs` prints writes only at FMD :455 (initializer), DDP :578, and test-only `FloorMapDrawableExecutionTests.cs:40`; every other hit is a read or the `ZoomLevelGuard` type. `grep -c -F 'ApplyZoom(' DDP` prints `6` (decl :576 + 5 callers) |
| K3 | 29, 30, 45 | the pan-drag accumulator is the one non-constant `PanOffset` writer; only `ResetViewport` resets it | same grep with `PanOffset`: writes only at FMD :458 (initializer), DDP :585 (`PointF.Zero`), DDP :1790-1792, and test-only `FloorMapDrawableExecutionTests.cs:39` |
| K4 | 51 | `FloorMapDrawable` is not a `BindableObject`; `grep -c BindableProperty` is 0 | `grep -c BindableProperty FMD` prints `0`; FMD :98 `public sealed class FloorMapDrawable : IDrawable, IDisposable` |
| K5 | 52 | the `.xaml` occurrences of `FloorMapDrawable` are prose, not bindings | `grep -rn "FloorMapDrawable" src/ --include=*.xaml \| grep -Ev "[\\/](obj\|bin)[\\/]"` prints exactly Tokens.xaml :263 and :267, each inside an XML comment (F5 corrects "a comment block") |
| K6 | 53 | no string literal or `nameof` of either name under `src/` outside this comment | `grep -rn -E '"(ZoomLevel\|PanOffset)"\|nameof\(([A-Za-z_.]*[.])?(ZoomLevel\|PanOffset)\)' src/ --include=*.cs --include=*.xaml \| grep -Ev "[\\/](obj\|bin)[\\/]"` prints one line, the test file's own line 37 (the grep method sentence). Control: on a fixture holding `nameof(FloorMapDrawable.ZoomLevel);`, `"PanOffset";` and `PanOffset.X;` it printed the first two and not the third |
| K7 | 37 | the grep method sentence | re-run as K2/K3; it reproduces the six list items' population plus the two test-only writes (line 47) |
| K8 | 18, 24 | `ZoomLevelGuard.Next` returns the current level for a non-finite raw, then snaps and clamps into [0.25, 4.0] | `src/DungeonMaster.App/Views/DungeonDetailPage/ZoomLevelGuard.cs`: `if (!float.IsFinite(raw))` at :43, `Math.Clamp(raw, MinimumZoom, MaximumZoom)` at :53, `MinimumZoom = 0.25f` :17, `MaximumZoom = 4.0f` :22 (`grep -n -E 'IsFinite\|Math.Clamp\|MinimumZoom =\|MaximumZoom ='`) |
| K9 | 33 | accumulation alone cannot practically reach ±Infinity, each `delta` bounded by the canvas extent | DDP :1789 `var delta = new PointF(current.X - anchor.X, current.Y - anchor.Y);` and :1793 resets the anchor to `current` each move; kept |
| K10 | 9 | constructed with `new FloorMapDrawable { ... }` and no MAUI host, as `FloorMapDrawableExecutionTests.cs` does | `FloorMapDrawableExecutionTests.cs` :37-41 object initializer; kept (no line number in the claim) |

K-row claims stay in substance; their line numbers are re-anchored per §3.

### 4.2 Claims false at `40dddd1`, or made false by this task (F). The comment must state what the code does.

- **F1 (line 8).** "The division ... is the one arithmetic operation in the chain that ends in a
  persisted `CanvasPoint`." False twice: `ScreenToCanvas` subtracts `PanOffset` before it divides
  (FMD :647-648), and the test file's own line 100 rests on that subtraction; and the chain does not
  end in a persisted point for a non-finite result (F2). Required: the R1 line names the
  subtraction (needle `subtract`); `the one arithmetic operation` and `ends in a persisted` count 0.
  Within the Place chain (touch → `placeRaw` cast-only construction at DDP :1594 → `ScreenToCanvas`
  → `new CanvasPoint`) the subtraction and division are the only arithmetic; `DrawGrid`'s divisions
  by `ZoomLevel` (FMD :525-528) are drawing, outside that chain, and need no mention.
- **F2 (line 31).** "... which `DungeonDetailPage.xaml.cs:1442` then persists as a `CanvasPoint`."
  False: `CanvasPoint`'s constructor runs `Finite` (`src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:89`),
  whose `if (!float.IsFinite(value))` (:94) throws `ArgumentOutOfRangeException` (:96); the throw at
  DDP :1600 is inside `OnMapStartInteraction`'s `try` (:1498), whose `catch (Exception exception)`
  (:1652) sets `_viewModel.ErrorMessage` (:1654). A non-finite result is never persisted through
  this path. Required: the R5 line names `ArgumentOutOfRangeException` and `ErrorMessage`;
  `persists as a <c>CanvasPoint</c>` counts 0.
- **F3 (line 118).** "... becomes a fully non-finite persisted `CanvasPoint`." False for the same
  reason. Required: the R26 line names `ArgumentOutOfRangeException`; `becomes a fully non-finite
  persisted` counts 0. The test name, body and assertions do not change.
- **F4 (line 38).** "... every line-number citation elsewhere in this file was not re-measured by
  that correction." True at base, made false by T1 itself. Required: `was not re-measured` counts 0.
  The sentence's "list items 3 and 4" reference may be dropped; if kept it must still name the
  `ApplyZoom` and `ResetViewport` items, which requires the list to keep its six items in order.
- **F5 (line 52).** "two prose lines inside a comment block". False: they sit in two separate XML
  comments, the banner at Tokens.xaml :262-264 and the comment at :266-271 (`grep -n -E '<!--|-->'`).
  Required: `inside a comment block` counts 0 and `two separate XML comments` counts ≥ 1.

### 4.3 Observations for Gate 3 (no change required by T1; no production change in this session)

- **O1.** In `OnMapDragInteraction`'s Pan branch the statement after the accumulator,
  `_editor.PanAnchorScreen = new CanvasPoint(current.X, current.Y);` (DDP :1793), throws
  `ArgumentOutOfRangeException` for the same non-finite coordinate, after the `PanOffset` write at
  :1790-1792 has landed. `OnMapDragInteraction` (:1754-1818) has no `try`. So the claim that
  `PanOffset` becomes permanently `NaN` still holds, and the handler then throws unhandled; whether
  the app survives that is not decidable from source. The comment may say so (the extra tokens
  `DungeonDetailPage.xaml.cs:1793` / `:1793` are allowed).
- **O2.** `OnMapStartInteraction` builds `new CanvasPoint((float)e.Touches[0].X, (float)e.Touches[0].Y)`
  at DDP :1500, inside its `try`, so a non-finite touch at interaction start is caught and reported
  before any `ScreenToCanvas` call.
- **O3.** The Draw branch passes the same `ScreenToCanvas` result to
  `TileCoordinateHelper.SnapToGrid` (`src/DungeonMaster.App.Core/Map/TileCoordinateHelper.cs:31-32`,
  `(int)MathF.Floor(x / tileSize + 0.5f)`), which casts a possibly-`NaN` float to `int`; what that
  cast yields is runtime-defined (judgment, not checked here) and no `CanvasPoint` guard sits on
  that path. Unchecked; a candidate backlog row.
- **O4.** R14/R16: decision E1 names an `#if` span "by its enclosing member and its directive";
  these two spans are class-level and have no enclosing member, so the anchor names the directive
  and the member the region declares (`OnMapScrollWheel`, `OnMapPinchUpdated`). Applied by analogy.

## 5. Allowed citation tokens after T1

Exactly the 26 row tokens of §3 (repeats allowed) plus these optional extras, each only on a line
carrying the label: `CanvasPoint.cs:89`, `CanvasPoint.cs:94`, `DungeonDetailPage.xaml.cs:1652`,
`:1652`, `DungeonDetailPage.xaml.cs:1793`, `:1793`. Any other P-CITE token is a stray.

## 6. Instruments

The reviewer and T1 write each program below to their own scratch directory **with the Write tool**
(Bash halves backslashes in heredocs) and run it with `python -I <script> <args>`, passing the test
file as `C:/dm-wt/20261116-2027-nonfinite-test-citations/src/DungeonMaster.App.Tests/Graphics/FloorMapDrawableNonFiniteTests.cs`.

### 6.1 census.py (P-CITE, P-COLON)

```python
import re, sys

CITE = re.compile(r'(?:[A-Za-z0-9_./]+\.(?:cs|xaml))?:[0-9]+(?:-[0-9]+)?')

def cites(path):
    data = open(path, 'rb').read().decode('utf-8-sig')
    out = []
    for n, line in enumerate(data.split('\n'), 1):
        if not line.lstrip().startswith('///'):
            continue
        for m in CITE.finditer(line):
            out.append((n, m.group(0)))
    return out

def second(path):
    data = open(path, 'rb').read().decode('utf-8-sig')
    total = 0
    for line in data.split('\n'):
        if not line.lstrip().startswith('///'):
            continue
        for i in range(len(line) - 1):
            if line[i] == ':' and line[i + 1] in '0123456789':
                total += 1
    return total

if sys.argv[1] == 'control':
    sample = '/// a <c>:513</c> b <c>Foo.cs:629-630</c> c <c>x/Y.xaml:263</c> d [0.25, 4.0] e Foo.cs f 10:x\n'
    print('CONTROL', [m.group(0) for m in CITE.finditer(sample)])
    sys.exit(0)

c = cites(sys.argv[1])
for n, t in c:
    print(f'{n}\t{t}')
print('P-CITE', len(c))
print('P-COLON', second(sys.argv[1]))
```

Base output at `40dddd1`: `P-CITE 26`, `P-COLON 26` (the 26 rows of §3). On the architect's
reference post-state fixture (§6.3): `P-CITE 28`, `P-COLON 28` (26 rows plus `CanvasPoint.cs:94`
and `DungeonDetailPage.xaml.cs:1652`).

### 6.2 check.py (P-CLAIM, P-ANCHOR, P-CODE, P-FORMAT, P-BYTES)

Indentation inside this program is tabs.

```python
import hashlib, re, sys

LABEL = 'as numbered at <c>40dddd1</c>'
CITE = re.compile(r'(?:[A-Za-z0-9_./]+\.(?:cs|xaml))?:[0-9]+(?:-[0-9]+)?')
DDP = 'src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs'
FMD = 'src/DungeonMaster.App/Graphics/FloorMapDrawable.cs'

# Table rows: (row id, citation token required, needles that must share its /// line).
ROWS = [
	('R1', 'FloorMapDrawable.cs:647-648', ['ScreenToCanvas', 'subtract']),
	('R2', 'DungeonDetailPage.xaml.cs:1595', ['OnMapStartInteraction', 'placeCanvas']),
	('R3', 'DungeonDetailPage.xaml.cs:1600', ['PlaceObjectAsync', 'new CanvasPoint(placeCanvas.X, placeCanvas.Y)']),
	('R4', 'DungeonDetailPage.xaml.cs:1595', ['OnMapStartInteraction']),
	('R5', 'DungeonDetailPage.xaml.cs:1600', ['PlaceObjectAsync', 'ArgumentOutOfRangeException', 'ErrorMessage']),
	('R6', FMD + ':455', ['ZoomLevel', '= 1.0f']),
	('R7', FMD + ':458', ['PanOffset', '= PointF.Zero']),
	('R8', DDP + ':578', ['ApplyZoom', 'ZoomLevelGuard.Next']),
	('R9', ':576', ['ApplyZoom']),
	('R10', ':571', ['OnZoomInClicked']),
	('R11', ':574', ['OnZoomOutClicked']),
	('R12', ':586', ['ResetViewport']),
	('R13', ':675', ['OnMapScrollWheel']),
	('R14', ':664-677', ['#if WINDOWS']),
	('R15', ':689', ['OnMapPinchUpdated']),
	('R16', ':677-693', ['#elif MACCATALYST']),
	('R17', DDP + ':586', ['ResetViewport', 'ApplyZoom(1.0f);']),
	('R18', ':583', ['ResetViewport']),
	('R19', DDP + ':585', ['ResetViewport', '_drawable.PanOffset = PointF.Zero;']),
	('R20', DDP + ':1790-1792', ['OnMapDragInteraction']),
	('R21', 'FloorMapDrawableExecutionTests.cs:39-40', ['ScreenToCanvasInvertsPanAndZoom']),
	('R22', 'FloorMapDrawable.cs:98', ['sealed class FloorMapDrawable : IDrawable, IDisposable']),
	('R23', 'Resources/Theme/Tokens.xaml:263', ['Feature-specific: FloorMapDrawable']),
	('R24', ':267', ['DmMapAreaStroke']),
	('R25', 'FloorMapDrawable.cs:647-648', ['ScreenToCanvas']),
	('R26', 'DungeonDetailPage.xaml.cs:1600', ['PlaceObjectAsync', 'ArgumentOutOfRangeException']),
]

# Every citation token the file may carry after the task: the 26 rows' tokens plus the optional extras.
EXTRAS = ['CanvasPoint.cs:89', 'CanvasPoint.cs:94', 'DungeonDetailPage.xaml.cs:1652', ':1652', 'DungeonDetailPage.xaml.cs:1793', ':1793']
ALLOWED = set(t for _, t, _ in ROWS) | set(EXTRAS)

def doc_lines(b):
	text = b.decode('utf-8')
	return [(n, l.rstrip('\r')) for n, l in enumerate(text.split('\n'), 1) if l.lstrip(' \t').startswith('///')]

def anchors(b):
	docs = doc_lines(b)
	fails = []
	for rid, tok, needles in ROWS:
		ok = False
		for _, l in docs:
			if tok in [m.group(0) for m in CITE.finditer(l)] and LABEL in l and all(nd in l for nd in needles):
				ok = True
		if not ok:
			fails.append(rid)
	stray = [(n, m.group(0)) for n, l in docs for m in CITE.finditer(l) if m.group(0) not in ALLOWED]
	unlabelled = [n for n, l in docs if CITE.search(l) and LABEL not in l]
	return fails, stray, unlabelled

def code_digest(b):
	lines = [l for l in b.split(b'\n') if not l.lstrip(b' \t').startswith(b'///')]
	return len(lines), hashlib.sha256(b'\n'.join(lines)).hexdigest()

TAGONLY = re.compile(r'^\s*///\s*</?(summary|remarks|para|list)( type="number")?>\s*$')

def fmt(b):
	bad = []
	for n, l in doc_lines(b):
		if TAGONLY.match(l):
			continue
		if not l.lstrip(' \t').startswith('///' + chr(9) + chr(9)):
			bad.append((n, 'indent'))
		elif not (l.endswith('.') or l.endswith('.</item>')):
			bad.append((n, 'end'))
	return bad

def bytes_report(b):
	cr, lf, crlf = b.count(b'\r'), b.count(b'\n'), b.count(b'\r\n')
	return 'LF %d CRLF %d loneCR %d' % (lf, crlf, cr - crlf)

# Claim needles over the whole file text: (id, needle, required count after the task).
CLAIMS = [
	('F1', 'the one arithmetic operation', 0),
	('F1', 'ends in a persisted', 0),
	('F2', 'persists as a <c>CanvasPoint</c>', 0),
	('F3', 'becomes a fully non-finite persisted', 0),
	('F4', 'was not re-measured', 0),
	('F5', 'inside a comment block', 0),
	('F5', 'two separate XML comments', '>=1'),
	('K1', 'is not decidable from this repository', '>=1'),
	('K1', 'is NOT asserted here', '>=1'),
]

def claims(b):
	t = b.decode('utf-8')
	return [(cid, nd, t.count(nd), want) for cid, nd, want in CLAIMS]

def report(b):
	for cid, nd, got, want in claims(b):
		print('P-CLAIM', cid, repr(nd), 'count', got, 'required', want, 'OK' if (got >= 1 if want == '>=1' else got == want) else 'NOT MET')
	fails, stray, unlabelled = anchors(b)
	print('P-ANCHOR missing rows', len(fails), fails)
	print('P-ANCHOR stray tokens', len(stray), stray)
	print('P-ANCHOR unlabelled citation lines', len(unlabelled), unlabelled)
	print('P-CODE lines %d sha256 %s' % code_digest(b))
	print('P-FORMAT bad lines', len(fmt(b)), fmt(b))
	print('P-BYTES', bytes_report(b))

if sys.argv[1] == 'controls':
	# Fixture lines built in memory: one that satisfies R22 and one near-miss that must not.
	good = ('///' + chr(9) + chr(9) + 'Declared <c>sealed class FloorMapDrawable : IDrawable, IDisposable</c> at <c>FloorMapDrawable.cs:98</c>, ' + LABEL + '.\r\n').encode()
	near = ('///' + chr(9) + chr(9) + 'Declared <c>sealed class FloorMapDrawable : IDrawable, IDisposable</c> at <c>FloorMapDrawable.cs:98</c>.\r\n').encode()
	stale = ('///' + chr(9) + chr(9) + 'At <c>FloorMapDrawable.cs:629-630</c>, ' + LABEL + '.\r\n').encode()
	print('CONTROL R22 good passes:', 'R22' not in anchors(good)[0])
	print('CONTROL R22 near-miss (no label) fails:', 'R22' in anchors(near)[0])
	print('CONTROL stale token flagged:', anchors(stale)[1])
	print('CONTROL unlabelled line flagged:', anchors(near)[2])
	print('CONTROL format: missing period flagged:', fmt(('///' + chr(9) + chr(9) + 'No period here\r\n').encode()))
	print('CONTROL format: single-tab indent flagged:', fmt(('///' + chr(9) + 'One tab.\r\n').encode()))
	print('CONTROL bytes fixture:', bytes_report(b'a' + b'\r\n' + b'b' + b'\r' + b'c' + b'\n'))
	sys.exit(0)

report(open(sys.argv[1], 'rb').read())
```

**`controls` output observed by the architect** (all must print as shown):

```
CONTROL R22 good passes: True
CONTROL R22 near-miss (no label) fails: True
CONTROL stale token flagged: [(1, 'FloorMapDrawable.cs:629-630')]
CONTROL unlabelled line flagged: [1]
CONTROL format: missing period flagged: [(1, 'end')]
CONTROL format: single-tab indent flagged: [(1, 'indent')]
CONTROL bytes fixture: LF 2 CRLF 1 loneCR 1
```

P-CODE has two further controls, run in memory on the base file: changing `Assert.Equal(60f` to
`Assert.Equal(61f` changes the digest (True), and changing one character of a `///` line leaves it
unchanged (True).

**Base output at `40dddd1`** (the stray list printed 22 tokens, the 26 less the 4 unchanged rows
R21–R24):

```
P-CLAIM F1 'the one arithmetic operation' count 1 required 0 NOT MET
P-CLAIM F1 'ends in a persisted' count 1 required 0 NOT MET
P-CLAIM F2 'persists as a <c>CanvasPoint</c>' count 1 required 0 NOT MET
P-CLAIM F3 'becomes a fully non-finite persisted' count 1 required 0 NOT MET
P-CLAIM F4 'was not re-measured' count 1 required 0 NOT MET
P-CLAIM F5 'inside a comment block' count 1 required 0 NOT MET
P-CLAIM F5 'two separate XML comments' count 0 required >=1 NOT MET
P-CLAIM K1 'is not decidable from this repository' count 1 required >=1 OK
P-CLAIM K1 'is NOT asserted here' count 1 required >=1 OK
P-ANCHOR missing rows 26 [R1 ... R26]
P-ANCHOR stray tokens 22 [...]
P-ANCHOR unlabelled citation lines 14 [8, 13, 31, 40, 41, 42, 43, 44, 45, 47, 51, 52, 64, 118]
P-CODE lines 95 sha256 c77569a96091b5efda08e7e84e8d62ae17d985f3b50551818993bd2a24ff058a
P-FORMAT bad lines 0 []
P-BYTES LF 169 CRLF 169 loneCR 0
```

### 6.3 Satisfiability (architect only, not a criterion)

The architect rewrote base lines 8, 13, 31, 38, 40-45, 47, 51, 52, 64 and 118 of a scratch copy
(CRLF kept) and ran check.py on it: every P-CLAIM `OK`, `missing rows 0`, `stray tokens 0`,
`unlabelled citation lines 0`, P-CODE digest identical to base, `P-FORMAT bad lines 0`,
`P-BYTES LF 169 CRLF 169 loneCR 0`. So all criteria can hold together. That wording is the
architect's and is not prescribed; T1 writes its own.

## 7. Build and test baseline

Measured by the orchestrator in this worktree at `40dddd1`:
`dotnet build C:/dm-wt/20261116-2027-nonfinite-test-citations/src/DungeonMaster.sln` printed
`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`; `dotnet test
C:/dm-wt/20261116-2027-nonfinite-test-citations/src/DungeonMaster.sln --no-build` printed
App.UnitTests 217/217, DungeonMaster.Tests 1004/1004, App.Tests 146/146. Prove a clean build by
quoting its three summary lines, never by `grep -icE "warning|error"`. Save the full test output to
scratch before filtering it.
