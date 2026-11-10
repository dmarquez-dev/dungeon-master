<!-- frozen: 20261110-1618-audit-record-corrections, Gate 2 -->

# C1 -- Register corrections: forms, shared wordings, measured facts, and the ARC instrument

Consumers: `T1` (owns `docs/audit/00-master-index.md`) and `T2` (owns `docs/audit/01-domain-application.md`).
Every figure below was measured by the architect on the session worktree `C:/dm-wt/20261110-1618-audit-record-corrections`
at `c90da08` (startRef for both tasks), with the command stated beside it. Commands run from the worktree root.
The main checkout and the worktree held byte-identical copies of both audit files at `c90da08` (`sha256sum` on
both paths printed the two digests of section 1 for each).

## 0. Universals in this contract, each checked against every instruction below

- **U1. No task rewrites or deletes any existing byte of either audit file.** Every change is an insertion of
  the forms in section 2. Checked against sections 4-5: every correction there is written as new text that quotes
  the corrected words; none instructs an in-place edit. ARC (section 3) is the operational definition.
- **U2. No commit hash for this session's own commits appears** in any insertion (tasks run without git; the
  orchestrator commits). Other hashes are permitted and some are required: `c90da08`, `11f9562`, `16d594b`, and
  any hash an OM result in section 6 supplies. Checked against sections 4-6.
- **U3. No `|` and no backslash in any 00 span** (section 2.1). Checked: no literal in sections 4-5 that T1 must
  write contains either character. This is how the cell escape scheme's own escape is avoided: the sequence
  cannot occur, and T1's criterion checks `PIPES 0 BACKSLASHES 0`.

## 1. Files, base digests, line endings, tools

| File | owner | sha256 at `c90da08` (raw bytes) | CR | CRLF | lone CR |
| --- | --- | --- | --- | --- | --- |
| `docs/audit/00-master-index.md` | T1 | `d6caf2f99d3667352eb51a7aed4b36c008971e4829fba222806328ac385e1037` | 691 | 691 | 0 |
| `docs/audit/01-domain-application.md` | T2 | `2e5ed635cf536402e2aedd80f749c61a4313610943468fe49ec5c04ba771f1d4` | 3363 | 3363 | 0 |

Census by ARC (section 3), which counts bytes `0d` and pairs `0d 0a` in Python. Control: a fixture holding
`a\r\nb\rc\n` printed `CR 2 CRLF 1 LONE_CR 1`.

- Edit tool: permitted (0 lone CR in both files). `Write` and `sed -i` on either file: forbidden.
- Never end an `Edit` `new_string` on whitespace (it is stripped). Key each edit on non-whitespace text.
- Insertions use no tab character.
- After editing: `LONE_CR 0`, `CR == CRLF`, and CRLF equals the startRef CRLF plus ARC's `ADDED_LINES`.

## 2. Insertion forms

The opener literal, shared by both files (note the trailing space):

    **[AMENDED 20261110-1618-audit-record-corrections, 2026-11-10 -- 

The closer literal is `**]**`. The headline after the opener ends with `.**`, then the body follows.

### 2.1 In `00-master-index.md`: one inline span per cell

- A span is `<opener><headline>.** <body>**]**`, on the cell's own line, inserted together with one following
  space **immediately before an anchor**:
  - for `MASTER-081` (`:269`), `MASTER-130` (`:318`) and `MASTER-178` (`:366`): the line's **first** occurrence of
    `**Previous status wording, retained unchanged below.**`;
  - for `MASTER-119` (`:307`), which has no such marker: the line's final `|` character.
- The body must not contain the sequence `**]** ` (closer plus space) before its end, and contains no `|` and no
  backslash (U3).
- Line numbers in 00 do not move (insertions add no line).

### 2.2 In `01-domain-application.md`: one block per entry

- A block is whole lines: a blank line, then the opener line (the opener at column 0), then wrapped body lines,
  the last line ending in `**]**`. No earlier line of the block ends in `**]**`.
- It is inserted **immediately after the last line of the entry's last existing block** (the line ending
  `**]**`), so the entry's existing blank line and the next `### ` heading follow it.

| Entry | insert after the startRef line ending `**]**` | next heading at startRef | register row named in the block |
| --- | --- | --- | --- |
| `CORE-016` | `:1136` | `:1138` `### CORE-017` | `MASTER-081` at `00-master-index.md:269` |
| `CORE-022` | `:1699` | `:1701` `### CORE-023` | `MASTER-110` at `00-master-index.md:298` |
| `CORE-024` | `:1972` | `:1974` `### CORE-025` | `MASTER-130` at `00-master-index.md:318` |
| `CORE-035` | `:3202` | `:3204` `### CORE-036` | `MASTER-178` at `00-master-index.md:366` |

Located by `grep -n '^### CORE-0' docs/audit/01-domain-application.md` and by the closer on the line above each
heading's preceding blank line. Each block carries, after its headline, `The register row is ...` as in the table,
and the bold sentence `**Nothing below deletes any previous wording.**`. Body lines wrap at a space, never inside
a word or at a hyphen, at no more than 112 characters where the text allows.

### 2.3 Citations in new prose (both files)

- Every line number cited in an insertion is **as numbered at `c90da08`**, and the insertion says so in those
  words at least once (the house precedent: `(numbered as at 11f9562, before this block was inserted)` in the
  `CORE-022` block). T2's own blocks move every 01 line below `:1136`.
- Every figure or line citation carries the command that re-derives it at `c90da08`, anchored on the content of
  the line it cites. A command whose population includes either audit file is stated with its result at `c90da08`,
  never as a present-tense result (FORM 17: the insertion's own quotations would change it).
- Quoted previous wording is verbatim and, where it spans a line break, says `joined across its line break`.

## 3. ARC -- the insertion checker (instrument, not deliverable)

Recreate it byte for byte with the `Write` tool in your own scratch directory as `arc_check.py` (Write keeps the
backslash-free text intact; the script contains no backslash by design, lessons.md:91). Its `sha256sum` was
`42f21f811f59064383601eb875d0ea5444d074a0c5afc26665577214b7a7b86c` (LF, trailing newline); if yours differs,
compare against this text before trusting it. Run: `python -I <scratch>/arc_check.py <file> [needle ...]`, with
the file as a `C:/...` path or an MSYS path in argv. Pass needles in single quotes; no needle in this contract
or the manifest contains a single quote. In the manifest's criteria a needle is delimited by backticks, or by
single quotes where the needle itself contains a backtick; the delimiters are not part of the needle.

```python
import hashlib
import os
import sys

OPEN = "**[AMENDED 20261110-1618-audit-record-corrections, 2026-11-10 -- "
CLOSE = "**]**"
CR = bytes([13])
CRLF = bytes([13, 10])
SEP = chr(13) + chr(10)
BASE = {
    "00-master-index.md": "d6caf2f99d3667352eb51a7aed4b36c008971e4829fba222806328ac385e1037",
    "01-domain-application.md": "2e5ed635cf536402e2aedd80f749c61a4313610943468fe49ec5c04ba771f1d4",
}


def fail(message):
    print("ERROR " + message)
    sys.exit(2)


def strip_spans(text):
    lines = text.split(SEP)
    found = []
    for number, line in enumerate(lines, start=1):
        while OPEN in line:
            start = line.find(OPEN)
            end = line.find(CLOSE + " ", start)
            if end < 0:
                fail("unclosed span on line %d" % number)
            span = line[start:end + len(CLOSE)]
            cells = line.split("|")
            where = cells[2].strip() if len(cells) > 2 else "?"
            after = line[end + len(CLOSE) + 1:end + len(CLOSE) + 41]
            found.append((where, number, span, after, 0))
            line = line[:start] + line[end + len(CLOSE) + 1:]
        lines[number - 1] = line
    return SEP.join(lines), found


def strip_blocks(text):
    lines = text.split(SEP)
    kept = []
    found = []
    heading = "?"
    index = 0
    while index < len(lines):
        line = lines[index]
        if line.startswith("### "):
            heading = line[4:].split(" -- ")[0]
        if OPEN in line and not line.startswith(OPEN):
            fail("opener not at line start on line %d" % (index + 1))
        if line.startswith(OPEN):
            if not kept or kept[-1] != "":
                fail("block on line %d not preceded by a blank line" % (index + 1))
            kept.pop()
            last = index
            while last < len(lines) and not lines[last].endswith(CLOSE):
                last += 1
            if last == len(lines):
                fail("unclosed block on line %d" % (index + 1))
            following = last + 1
            while following < len(lines) and lines[following] == "":
                following += 1
            after = lines[following][:40] if following < len(lines) else "EOF"
            found.append((heading, index + 1, " ".join(lines[index:last + 1]), after, last - index + 2))
            index = last + 1
            continue
        kept.append(line)
        index += 1
    return SEP.join(kept), found


def main():
    path = sys.argv[1]
    needles = sys.argv[2:]
    name = os.path.basename(path)
    if name not in BASE:
        fail("unknown file " + name)
    data = open(path, "rb").read()
    cr = data.count(CR)
    crlf = data.count(CRLF)
    print("FILE " + name)
    print("CR %d CRLF %d LONE_CR %d" % (cr, crlf, cr - crlf))
    text = data.decode("utf-8")
    if name.startswith("00"):
        stripped, found = strip_spans(text)
    else:
        stripped, found = strip_blocks(text)
    print("INSERTIONS %d ADDED_LINES %d" % (len(found), sum(item[4] for item in found)))
    for where, number, body, after, _ in found:
        print("  AT %s line %d PIPES %d BACKSLASHES %d NEXT [%s]" % (where, number, body.count("|"), body.count(chr(92)), after))
    digest = hashlib.sha256(stripped.encode("utf-8")).hexdigest()
    print("SHA_AFTER_REMOVAL " + digest)
    print("BASE_MATCH " + ("yes" if digest == BASE[name] else "no"))
    for needle in needles:
        hits = [item[0] for item in found if needle in item[2]]
        outside = stripped.count(needle)
        print("NEEDLE [%s] IN %s OUTSIDE %d" % (needle, ",".join(hits) if hits else "NONE", outside))


main()
```

What it reports: the byte census; each insertion's location (`AT` the cell's `MASTER-NNN` for 00, the
enclosing `### CORE-NNN` heading for 01), its `PIPES`/`BACKSLASHES` counts and the text that follows it
(`NEXT`); the sha256 of the file **with every insertion removed** (for 01, each block plus the blank line before
it) and whether that equals the startRef digest; and, per needle, which insertions contain it (block lines
joined with one space, so a needle spanning a line break at a space is found) and how often it occurs outside
all insertions.

Validated by the architect at `c90da08`:
- startRef files: `INSERTIONS 0 ADDED_LINES 0`, `BASE_MATCH yes`, census as in section 1.
- must-match fixture (a span inserted before 00 `:269`'s first previous-status marker; a three-line block after
  01 `:1136`): `AT MASTER-081 line 269 ... NEXT [**Previous status wording, retained unch]`, `AT CORE-016 line 1138
  ... NEXT [### CORE-017 -- ObjectKind has no zero v]`, `ADDED_LINES 4`, CRLF 3367, `BASE_MATCH yes` for both; a
  needle wrapped across the block's line break at a space reported `IN CORE-016`.
- must-fail fixture (the same plus one byte appended to line 10): `BASE_MATCH no` for both files.
- unclosed 00 span: `ERROR unclosed span on line 269`. An unclosed 01 block swallows text up to the next
  existing `**]**` line and reports `BASE_MATCH no` (blind spot of the `ERROR` path, caught by the digest).
- pipe control: a span holding one bare `|` and one `\|` printed `PIPES 2 BACKSLASHES 1`.

## 4. Shared wordings (both tasks consume these)

### 4.1 Row 184 -- the zero-member reading

The population is the zero-member wordings only: the sentences classifying the addition of a zero member to
`PathSegmentKind` as, or as not, "a renumbering". Five sites at `c90da08`, none wrapping a line:

| Site | words, verbatim | owner |
| --- | --- | --- |
| `00-master-index.md:318` (`MASTER-130`, inside its retained 20261105-1907 status wording) | `a renumbering, not a rename` | T1 |
| `00-master-index.md:366` (`MASTER-178`, inside its retained 6b-ix status wording) | `a data migration, not a renumbering` | T1 |
| `01-domain-application.md:1882` (`CORE-024`, `AMENDED 20261105-1907-core-heavy-rows` block) | `a renumbering, not a rename` | T2 |
| `01-domain-application.md:3038` (`CORE-035` body) | `the blocker is a data migration rather than a renumbering` | T2 |
| `01-domain-application.md:3058` (`CORE-035` body) | `data migration on a JSON column, not a renumbering` | T2 |

Census, two predicates that cannot share a mistake:
- per line: `grep -nE 'a renumbering, not a rename|(rather than|not) a renumbering' docs/audit/00-master-index.md
  docs/audit/01-domain-application.md` prints exactly the five lines above.
- joined text: a Python scan of each file with its lines joined by one space, for the three phrases
  `a renumbering, not a rename`, `rather than a renumbering`, `not a renumbering`, prints the same five sites,
  each spanning one line; its wrap control (`a renumbering,` and `not a rename` on consecutive CRLF lines) was
  reported as spanning lines 1-2.
- must-not-match, checked by eye: 01 `:1183` (`renumbering it was free`, `ObjectKind`), 00 `:344` (a rank
  renumbering) and 01 `:1881` (`renumbering them would`) contain `renumber` and are matched by neither predicate.
  The other `renumber` lines (00 `:82`, `:282`, `:295`, `:344`, `:371`, `:373`, `:375`, `:377`, `:384`; 01 `:832`,
  `:1183`, `:1184`, `:1194`, `:1608`, `:1611`, `:1881`, `:2797`, `:3039`, `:3335`) are floor, rank, row or
  per-enum freedom-to-renumber prose and are not in the population.

**Decision: annotate, do not rewrite.** Both 00 sites sit inside text the cell itself declares
`Previous status wording, retained unchanged below.`, and the 01 blocks declare `Nothing below deletes any previous
wording.` (U1). Each site gets a note in the insertion of its cell or entry.

**The shared reading (literal R184), which every one of the five notes states verbatim:**

    adding a zero member renumbers the persisted ordinals, so it is a renumbering that needs a data migration

Each note quotes its site's words verbatim, says the two wordings agree in substance (in "not a renumbering", the
word means a code-only renumbering), and states R184. Repository evidence, to be cited with its commands:
`src/DungeonMaster.Domain/ValueObjects/PathSegmentKind.cs` declares `Line` at `:11` (`grep -n 'Line,'`) and
`CubicBezier` at `:16` (`grep -n CubicBezier`), with no explicit values (`grep -c '='` on that file prints 0), so
inserting `None` first moves `Line` from 0 to 1 and `CubicBezier` from 1 to 2; the ordinal is persisted, because
`AreaConfiguration.cs:16` (`grep -n JsonSerializerDefaults
src/DungeonMaster.Infrastructure/Persistence/Configurations/AreaConfiguration.cs`) builds
`new(JsonSerializerDefaults.General)` with no converter, and
`src/DungeonMaster.Tests/Infrastructure/Persistence/Configurations/PathSegmentJsonRoundTripTests.cs:28-29`
(`grep -n '"Kind"'` on that file prints `:28`, `:29` and `:52`) stores `"Kind": 0` and `"Kind": 1`.

### 4.2 Row 190 -- the declined list's attribution

Words corrected, verbatim at `c90da08`:
- `00-master-index.md:269`: `(told: Gate 1, the same `discussion.md` Decisions)` attributing
  `a policy layer, `PolicyResult`, `DomainException` and an `ObjectScale` value object`.
- `01-domain-application.md:1075-1076`: `(told: Gate 1, the same Decisions)`, followed by the same list, which wraps
  after `a` on `:1075`.

**The corrected attribution (literal R190), which both notes state:** `PolicyResult`, `DomainException` and an
`ObjectScale` value object are named as considered and declined in
`.sir/sessions/20261109-1641-part-done-rows/discussion.md:26` (Decisions), located by
`grep -n 'were considered and declined'` on that file, which prints `:26` alone; the **policy layer** is not named
there, and is named, with all three others, in that session's `spec.md:22-24` (Context, the bullet that opens
`- **MASTER-081 / CORE-016**` at `:22`, `grep -n 'MASTER-081 / CORE-016'`, and ends `Gate 1 declined them.` at
`:24`, `grep -n 'Gate 1 declined them'`). In `spec.md` the words `The policy` and `layer` wrap across `:23-24`, so
`grep -n 'policy layer'` on `spec.md` prints only `:82` (Not in scope); on `discussion.md` it prints only `:16`
(Context, describing the 6b-xv status cell, not the decision). Classification: **ERROR (attribution)** -- both
files were in that session's record when the attribution was written.

### 4.3 Cross-references

A 00 span refers to 01 by entry and block name, never by 01 line number other than the `c90da08` citations
listed in section 5, in the house form, backticks included:

    the `AMENDED 20261110-1618-audit-record-corrections` block on `CORE-0NN` in `01-domain-application.md`

with `CORE-016` in the `MASTER-081` span, `CORE-024` in the `MASTER-130` span and `CORE-035` in the `MASTER-178`
span. The `MASTER-119` span has no 01 block to name (row 191 changes 00 only). A 01 block refers to its register
row as in section 2.2's table, in the form `The register row is `MASTER-NNN` at `00-master-index.md:NNN`.`

## 5. Per-row facts (measured at `c90da08`)

### 5.1 Row 179 -- `CORE-022` block (T2)

- Corrected words, verbatim, joined across the line break at `01-domain-application.md:1677-1678`: `Those figures
  are told: measured by the architect and stated in this session's C1 and C2 contracts.`
- Truth, in `.sir/sessions/20261105-0106-core-two-row-sweep/contracts/`:
  - `C1-measurement.md:203-204` states `FloorMethods.cs` 176 and `Floor.cs` 94 lines at startRef:
    `grep -n '(176 lines)'` prints `C1-measurement.md:203`; `grep -n '(94 lines)'` prints `C1-measurement.md:204`.
  - `FloorObjectMethods.cs` 112 and the 288 total are stated **only in C3**, `C3-register.md:64-65`:
    `grep -n 'FloorObjectMethods.cs. 112' .sir/sessions/20261105-0106-core-two-row-sweep/contracts/C*.md` prints
    `C3-register.md:64` alone, and `grep -nw 288 .sir/sessions/20261105-0106-core-two-row-sweep/contracts/C*.md`
    prints `C3-register.md:65` alone.
  - `C2-entity-split.md:34` cites the range `FloorObjectMethods.cs:7-112` ending at the type's closing `}`, which
    implies a 112-line file but states no count.
  - The backlog row's instrument `grep -nw -e 112 -e 288` over `C*.md` over-matches: it also prints `C1:204`
    (`PlacedObject.cs` 112 lines, digest `9ab84b7a...`), `C2:34` and `C2:67` (line ranges).
  - Origin: `C3-register.md:65-66` itself says `stated in C1` / `and C2.`; the block copied C3 section 2 item 2.
- Classification: **ERROR (attribution)**. The figures themselves are unaffected.

### 5.2 Row 182 -- `CORE-035` block (T2)

- The entry cites `src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs:24-28` for `GetDefaultAnchor` at
  `01-domain-application.md:2951` (`locations:`) and `:3033` (the `SnapAnchor` class C reclassification
  paragraph): `grep -n 'ObjectKindInfo.cs:24-28' docs/audit/01-domain-application.md` printed exactly those two.
- Now: `grep -n GetDefaultAnchor src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs` prints `40:` alone
  (the declaration); `grep -n 'return SnapAnchor.Center' src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs`
  prints `58:` alone (the terminal fallback the paragraph describes).
- The `AMENDED 20261105-1907-core-heavy-rows` block's `*Drifted citations,*` paragraph (`:3192`) omits this
  drift, while its dispositions row 4 (`:3163`) already cites `ObjectKindInfo.cs:40`.
- The block records the drift; `locations:` and the paragraph stay as written. Classification: per OM-1.

### 5.3 Row 183 -- `CORE-035` block (T2)

- Corrected words, verbatim, from dispositions row 2 (`:3161`): ``grep -rn RotationDirection` over production
  `src/` prints only the event declaration (`ControlsPaletteView.xaml.cs:14`), those four lines and the enum's own
  declaration.``
- Restated command, with the filters its figure depends on:
  `grep -rn RotationDirection src --include=*.cs --exclude-dir=obj --exclude-dir=bin | grep -v 'Tests/'`
  prints **6 lines** at `c90da08`: `ControlsPaletteView.xaml.cs:14` (event), `:27` and `:28` (producers),
  `DungeonDetailPage.xaml.cs:196` and `:207` (the `==` reads), `RotationDirection.cs:6` (declaration). The figure
  stands (1 + 4 + 1); the two `==` reads the row cites as `:139` and `:150` are now `:196` and `:207`. The
  `grep -v 'Tests/'` stage removes nothing at `c90da08` (the same command without it also prints 6 lines); it keeps
  the scope to production code. The bare command also reports `Binary file ... matches` for build output under
  `bin/` and `obj/` in a built tree (it did in this worktree at `c90da08`; no count is stated, it depends on what
  was built).
- **ScaleDirection (decided at Gate 2: recorded in the same block).** Dispositions row 3 (`:3162`) says `the same
  grep for `ScaleDirection``. The same filtered command for `ScaleDirection` prints **5 lines**:
  `ControlsPaletteView.xaml.cs:19`, `:29`, `:30`, `DungeonDetailPage.xaml.cs:223` (row 3 cites `:166`),
  `ScaleDirection.cs:6`. The `CORE-035` block records this too.
- Classification of the `==`-line moves: per OM-3.

### 5.4 Row 184 -- see section 4.1 (T1: 00 `:318`, `:366`; T2: `CORE-024` block for `:1882`, `CORE-035` block for `:3038` and `:3058`).

### 5.5 Row 190 -- see section 4.2 (T1: `MASTER-081` span; T2: `CORE-016` block).

### 5.6 Row 191 -- `MASTER-119` span (T1)

- Corrected words, verbatim, `00-master-index.md:307`: ``MASTER-114` records a citation stale by project into
  `App.Core` at `01-domain-application.md:2313``.
- At `c90da08`, `01-domain-application.md:2313` lies inside `CORE-026` (`### CORE-026` at `:2105` is the last heading
  above it), in a *Correction 3* about `MASTER-181`.
- The text meant is `CORE-030`'s `AMENDED 6b-xvii` block, *Correction 2*, at `01-domain-application.md:2560-2567`
  as numbered at `c90da08`: `:2560` by `grep -n 'the fifth citation, stale by PROJECT'
  docs/audit/01-domain-application.md` (prints `:2560` alone), and `:2567`, the line ending `caller inside the
  Application layer.`, by `grep -n 'caller inside the Application layer' docs/audit/01-domain-application.md`
  (prints `:2567` alone); `:2568` is empty. The backlog row's `2560-2564` is short by three lines.
- `CORE-030` is `MASTER-114`'s entry (`00-master-index.md:302`; the block says `The register row is `MASTER-114``).
- Classification: per OM-2.

## 6. Orchestrator measurements required before freeze (tasks run without git)

The architect does not run git. Each result is quoted into this section before Gate 2, and the insertion states
it as told.

- **OM-1 (row 182).** `git log -S 'ObjectKindInfo.cs:24-28' --format=%h -- docs/audit/01-domain-application.md`
  (the commit that wrote the citation), then `git show <that>:<ObjectKindInfo.cs path at that commit> | grep -n
  GetDefaultAnchor`. DRIFT if the declaration lay within `:24-28` there; otherwise ERROR.
- **OM-2 (row 191).** `git log -S '01-domain-application.md:2313' --format=%h -- docs/audit/00-master-index.md`,
  then `git show <that>:docs/audit/01-domain-application.md | grep -n 'the fifth citation, stale by PROJECT'`.
  DRIFT if it printed `2313` (or a span containing it); otherwise ERROR.
- **OM-3 (row 183).** `git log -S 'DungeonDetailPage.xaml.cs:139' --format=%h -- docs/audit/01-domain-application.md`,
  then `git show <that>:src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs | grep -n
  'RotationDirection.Clockwise'`. DRIFT if it printed `:139` and `:150`.
- **OM-4 (row 179).** `git log --format=%h -- .sir/sessions/20261105-0106-core-two-row-sweep/contracts/` -- if one
  commit, the contracts were never amended after they were committed, which supports ERROR over DRIFT.

### 6.1 Results (run by the orchestrator in the main checkout at `c90da08`, 2026-11-10; told to both tasks)

Each insertion states its classification as **told: orchestrator measurement, 20261110-1618-audit-record-corrections**.
Line numbers from `git show <h>:<path> | grep -n` (lessons.md:84: `grep -n`, not `sed -n`).

- **OM-1 -> DRIFT.** `git log -S` printed `476b510` alone (2026-09-17, `docs(audit): record CORE-034, CORE-035, and
  UI-037`). `git show 476b510:src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs | grep -n GetDefaultAnchor`
  printed `24:` -- the declaration lay inside `:24-28` when the citation was written.
- **OM-2 -> DRIFT.** `git log -S` printed `c7f1b96` alone (2026-10-12, `docs(audit): close the eight UI rows in the
  register index`). At `c7f1b96`, `grep -n 'the fifth citation, stale by PROJECT'` printed `2309:` and
  `grep -n 'caller inside the Application layer'` printed `2316:`, so Correction 2 spanned `:2309-2316`, which
  contains the cited `:2313`.
- **OM-3 -> DRIFT.** `git log -S` printed `6b3bd62` alone (2026-11-05, `docs(audit): close MASTER-178 deliberately
  unchanged and amend CORE-035`). `git show 6b3bd62:src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs
  | grep -n 'RotationDirection.Clockwise'` printed `139:` and `150:`; the same file with `grep -n 'ScaleDirection'`
  printed `166:` (row 3's `ScaleDirection` citation drifted the same way).
- **OM-4 -> supports ERROR (row 179).** `git log --format=%h -- .sir/sessions/20261105-0106-core-two-row-sweep/contracts/`
  printed `d097ff6` alone (2026-11-05, `chore(dir): record SIR session 20261105-0106-core-two-row-sweep`): the
  contracts were committed once and never amended, so C3 never said otherwise.
