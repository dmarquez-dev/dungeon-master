<!-- frozen at Gate 2: 20261111-0156-core-audit-corrections, 2026-11-11 -->

# C1 -- CORE-audit corrections: the three frozen insertions, their anchors, and the ICC instrument

Consumer: `T1` (owns `docs/audit/01-domain-application.md`). startRef `9f9ca69` (`chore/repo-audit`). Every figure
about the audit file below was measured by the architect on the session worktree
`C:/dm-wt/20261111-0156-core-audit-corrections` at `9f9ca69`, with the command beside it; `sha256sum` printed the
same digest for the worktree copy and the main-checkout copy (section 1). Every **git-history** fact is the
orchestrator's spec-time measurement, quoted from `.sir/sessions/20261111-0156-core-audit-corrections/measurement.md`
(M1-M3) and stated in the text as told; T1 runs without git and must not re-derive history.

The text of each insertion is frozen **exactly** in section 2. T1 composes nothing; it applies three insertions and
measures. Exactness is the point here: the text composed in both earlier audit-correction sessions carried defects
of its own (lessons.md, the 20261110-1618 backlog-line labels and the 20261110-1857 `05:2305` citation), so every
sentence below was checked by the architect against the file, the source and the told measurements before freezing
(section 5).

## 0. Universals, each checked against every instruction in this contract

- **U1. Every change is one of the three whole-line insertions I1-I3 of section 2, at its anchor, and nothing
  else.** No pre-existing line is deleted or changed. Operational definition: ICC (section 3) prints
  `OCCURRENCES I1 1`, `OCCURRENCES I2 1`, `OCCURRENCES I3 1`, three `AT ... ok` lines and `BASE_MATCH yes`.
  Checked against section 2: each instruction is an insertion of whole lines after an existing line; none edits a
  line in place.
- **U2. No insertion labels anything by a `.sir/backlog.md` line number, mentions `HEAD`, names a scratch path, or
  holds a `|`.** Operational definition: ICC's `FORBIDDEN NONE` for each insertion (needles `backlog.md:`, `row :`,
  `AppData`, `|`, `HEAD`). Checked: the I1-I3 texts were scanned by ICC on the must-match fixture (section 5).
- **U3. Every insertion carries the attribution literal** `told: orchestrator measurement,
  20261111-0156-core-audit-corrections` (ICC `ATTR yes`), and **every insertion citing a line of this file says**
  ``as numbered at `9f9ca69` `` (ICC `NUMBERED yes` for I1 and I3; I2 cites no line number of the audit file and
  prints `NUMBERED no` by design). The three insertions move every line below `:842`, so a bare number would drift
  inside the session that wrote it.
- **U4. No insertion line holds a tab or trailing whitespace; no I1/I2 line ends in `]`; no I3 line other than the
  last ends in `]`; I3's last line ends `**]**`; every insertion's `**` count outside code spans is even.**
  Operational definition: ICC `TABS 0 TRAILING_WS 0 EARLY_CLOSE 0`, even `BOLD`, and I3 `LAST_ENDS ...**]**`.
  I1 and I2 sit inside existing `AMENDED 6b-xiv` blocks, so a line of theirs ending in `]` would close that block
  early for BP (section 4); they are parentheticals opened by `(AMENDED` (no `[`) and closed by `)`.
- **U5. The insertions' claims about preserved text are true after the change.** I1 and I2 each end "Nothing
  above is changed" / "nothing above is changed"; I3 says "This entry's body and its earlier blocks stay exactly as
  written". All three are U1 restated, and `BASE_MATCH yes` is their measurement. I3 sits after the
  `AMENDED 20261110-1618-audit-record-corrections` block whose `:3387-3388` says the body and the
  `AMENDED 20261105-1907-core-heavy-rows` block "stay exactly as written"; that is why I3 is a block at the end of
  the entry and not a parenthetical in the body (section 2.3).

## 1. File, base digest, line endings, tools

| File | owner | sha256 at `9f9ca69` (raw bytes) | CR | LF | CRLF | lone CR |
| --- | --- | --- | --- | --- | --- | --- |
| `docs/audit/01-domain-application.md` | T1 | `58acb6b634b6cf4b9db16a327a686d209e9a76104517cc08204411cfb945c467` | 3552 | 3552 | 3552 | 0 |

Census: ICC (Python byte counts of `0d`, `0a`, `0d 0a`); agrees with measurement.md's census. Digest: `sha256sum` on
both the worktree and main-checkout paths, same value.

- `Edit` is the only permitted writer. `Write` and `sed -i` on the file are forbidden: both emit LF. The file has no
  lone CR, so Edit's lone-CR normalisation does not bite.
- Edit strips trailing whitespace from `new_string`: every `new_string` below ends on non-whitespace text.
- Suggested `old_string` for each insertion (each measured unique at `9f9ca69` with `grep -c -F`, printing 1):
  I1 `address this duplication and not only the lookup.`; I2 `and no line position here is inferred from a
  neighbouring one.`; I3 the two lines `**DELIBERATELY UNCHANGED**.**]**` + blank + `### CORE-036 -- CanvasPoint's`
  (the first line alone occurs 3 times, as a substring at `:1136` and `:1168`, so it is **not** unique alone). The
  `new_string` is the `old_string` with the insertion's lines added on the line(s) after the anchor (for I3, before
  the blank line that precedes `### CORE-036`). ICC judges the result, not the route.
- Apply I1, I2, I3 in any order; each anchor is unique and outside the others' text.
- T1 does not build, test or run git. No task in this session builds.

## 2. The insertions (exact; ICC reads them from this file)

Each fence below holds the exact lines inserted, one source line per file line, joined in the file by CRLF. Each
marker names the base line (numbered at `9f9ca69`) the insertion follows. ICC parses these markers and fences; do
not reformat them.

### 2.1 I1 -- `CORE-015`, Correction 4 (the `2026-09-29-dungeon-facade-lookup-extraction` follow-up)

After `:842` (`address this duplication and not only the lookup.`), the last line of Correction 4's paragraph,
inside the `AMENDED 6b-xiv` block on `CORE-015` that opens at `:792`; `:843` (blank) and `:844`
(`*Correction 5 ...`) follow it. The parenthetical restates the anchor the paragraph left out, and says its figures
are not covered by that block's head-of-block provenance (`:798-803`), which is otherwise the block's universal.

<!-- INSERT I1 AFTER BASE LINE 842 -->
````text
(AMENDED 20261111-0156-core-audit-corrections, 2026-11-11 -- the anchor of the figure above, restated. The
**5** is a figure at `20b0b09`, the baseline Corrections 1-3 name (`:807`, `:813`, `:824`, as numbered at
`9f9ca69`); Correction 4 itself names no commit. At `9f9ca69` the same command over the same file returns
**1**: the one remaining match is the guard in the private helper `TouchOnSuccess`
(`DungeonMethods.cs:462`), and each of the five methods named above now returns through it. Measured by this
session's task on the session worktree, over `src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs`:
`grep -c -F 'if (outcome is not MutationOutcome.Success)'` printed 1, and
`grep -n -F 'return TouchOnSuccess('` printed 5 lines, at `DungeonMethods.cs:241`, `:279`, `:314`, `:389`
and `:428`, one in each of the five. The rest is told: orchestrator measurement,
20261111-0156-core-audit-corrections, `.sir/sessions/20261111-0156-core-audit-corrections/measurement.md`
section M1: the same `grep -c` over
`git show 20b0b09:src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs` printed 5, and
`git log --oneline 20b0b09..9f9ca69` on that path printed one commit, `66a0b73`, 6b-xiv's own extraction of
the floor lookup and outcome-touch helpers. These figures are this session's, not 6b-xiv's, so the provenance
stated at the head of this block does not cover them, and nothing above is changed.)
````

### 2.2 I2 -- `CORE-019`, the `AMENDED 6b-xiv` preamble (the `2026-09-29-dungeon-facade-lookup-extraction` follow-up)

After `:1418` (`and no line position here is inferred from a neighbouring one.`), the last line of the preamble
paragraph of the `AMENDED 6b-xiv` block on `CORE-019` that opens at `:1402`; `:1419` (blank) and `:1420`
(`*Correction A ...`) follow it. The exception sentence is `:1414-1417`; the universal it fails to except the table
from is `:1411-1413`; the table is `:1535-1544`, the closing-line range `:1529`, the method `:1532-1533`. I2 adds
the qualifier as the paragraph's last sentence rather than mid-line, because the exception sentence ends inside
`:1417` and U1 forbids changing a line. It cites the block's own sentences by quotation, not by line number.

<!-- INSERT I2 AFTER BASE LINE 1418 -->
````text
(AMENDED 20261111-0156-core-audit-corrections, 2026-11-11 -- a qualifier to the exception above. The
eight-row drift table and the closing-line range are excepted from "handed over with the command that
produced it" as well (joined across its line break): they were handed over with no command. The rows came
with a method, the one the sentence above the table states, "resolving the cited range against both commits,
and re-measured independently" (joined across its line break), and the range came bare. The rest is told:
orchestrator measurement, 20261111-0156-core-audit-corrections,
`.sir/sessions/20261111-0156-core-audit-corrections/measurement.md` section M2, which reads the hand-over in
criterion C15 of `.dir-sdlc/sessions/2026-09-29-dungeon-facade-lookup-extraction/manifest.json`. This
session's task re-read that criterion: from "The eight rows, measured" to "(+15 to +48)" it names no `git`,
`grep`, `awk`, `sed` or `python` command. Nothing above is changed.)
````

### 2.3 I3 -- `CORE-035`, the two twelves (the `2026-09-17-enum-zero-values` follow-up)

After `:3391` (`**DELIBERATELY UNCHANGED**.**]**`), the closer of the `AMENDED 20261110-1618-audit-record-corrections`
block, the last block of `CORE-035`; the existing `:3392` (blank) and `:3393` (`### CORE-036 ...`) follow it. The
fence's first line is empty: it is the blank line that precedes the opener. A block, not a parenthetical in the
body, because `:3387-3388` records the body and the `AMENDED 20261105-1907-core-heavy-rows` block as staying
"exactly as written" (U5).

<!-- INSERT I3 AFTER BASE LINE 3391 -->
````text

**[AMENDED 20261111-0156-core-audit-corrections, 2026-11-11 -- this entry's two "twelve" figures are named
apart, and found to name the same twelve enums.** The register row is `MASTER-178`. **Nothing below deletes
any previous wording.** Every line number of this file cited in this block is as numbered at `9f9ca69`, before
this session's insertions, and each was re-read by this session's task on the session worktree; no census is
re-run here.

*The census twelve.* `12 of 17` is the post-change census: production enum declarations under `src/` with a
concrete member at zero, counted by this entry's Python census re-run on the tree after its two class A changes
(`:3080`, restated `:3086`), and counted again by the `AMENDED 20261105-1907-core-heavy-rows` block, whose
census printed `CONCRETE_AT_ZERO 12` (`:3245`; `The 12 of 17` at `:3257`). Its unit is enum declarations in a
source tree, so it moves when an enum gains or loses a concrete member at zero.

*The table twelve.* `14 - 2 = 12` is the table census: the fourteen rows of the triage table (`:3107-3120`)
less its two class A rows, `MutationOutcome` (`:3107`) and `DrawSessionResult` (`:3108`), giving the twelve
this entry leaves unchanged (`:3070-3071`, restated `:3199-3200`). Its unit is rows of a table in this file,
so it does not move with the code.

*The same twelve.* The two figures come from different instruments and agree on one set: the twelve enum
names the triage table holds after its two class A rows are the twelve names of the dispositions table in the
`AMENDED 20261105-1907-core-heavy-rows` block (`:3267-3278`, rows 1 to 12), which says so at `:3248-3249`. So
that block's "the twelve enums this entry leaves with a concrete member at zero" (`:3214`) and "No enum of the
twelve" (`:3223`) hold under either reading. Measured by this session's task, comparing as sets the
backticked names in the first cell of `:3109-3120` with those in the second cell of `:3267-3278`; the same
finding is told: orchestrator measurement, 20261111-0156-core-audit-corrections,
`.sir/sessions/20261111-0156-core-audit-corrections/measurement.md` section M3. Quote either figure with its
label, `12 of 17` (census) or `14 - 2 = 12` (table census): they coincide on the tree the dispositions were
made against, and the census figure moves with the code while the table figure does not. This entry's body
and its earlier blocks stay exactly as written. **No hash for this session's commit appears in this block.**
The record's severity, category, effort and risk are unchanged, and `MASTER-178` / `CORE-035` stays CLOSED
**DELIBERATELY UNCHANGED**.**]**
````

## 3. ICC -- the insertion-and-citation checker (instrument, not deliverable)

Recreate it byte for byte with the `Write` tool in your own scratch directory as `icc.py`. It contains no backslash
by design (lessons.md, 20261109-2239 harness entry). Its `sha256sum` is given in section 5; if yours differs, compare
against this text before trusting it. Run, as four positional arguments (`C:/...` paths):

    python -I <scratch>/icc.py <session dir>/contracts/C1-core-audit-corrections.md <worktree>/docs/audit/01-domain-application.md <worktree>/src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs <worktree>/.dir-sdlc/sessions/2026-09-29-dungeon-facade-lookup-extraction/manifest.json

where `<session dir>` is `C:/Users/dmarq/Development/Repos/dungeon-master/.sir/sessions/20261111-0156-core-audit-corrections`
(this frozen contract, in the main checkout) and `<worktree>` is `C:/dm-wt/20261111-0156-core-audit-corrections`.

````python
import hashlib
import sys

BASE_SHA = "58acb6b634b6cf4b9db16a327a686d209e9a76104517cc08204411cfb945c467"
CR = bytes([13])
LF = bytes([10])
CRLF = bytes([13, 10])
SEP = chr(13) + chr(10)
TAB = chr(9)
TICK = chr(96)
FENCE = TICK * 4
MARK = "<!-- INSERT "
ATTR = "told: orchestrator measurement, 20261111-0156-core-audit-corrections"
NUMBERED = "as numbered at " + TICK + "9f9ca69" + TICK
FORBIDDEN = ["backlog.md:", "row :", "AppData", "|", "HEAD"]
GUARD = "if (outcome is not MutationOutcome.Success)"
RETURN = "return TouchOnSuccess("
SPAN_FROM = "The eight rows, measured"
SPAN_TO = "(+15 to +48)"
COMMANDS = ["git ", "grep", "awk", "sed ", "python"]
# (insertion, base line, text that base line must contain). Base = the audit file with the insertions removed.
CITES = [
    ("I1", 807, "baseline `20b0b09`"),
    ("I1", 813, "at `20b0b09`"),
    ("I1", 824, "at `20b0b09`"),
    ("I1", 837, "*Correction 4 -- a SECOND duplication"),
    ("I1", 838, "returns **5**"),
    ("I1", 842, "address this duplication and not only the lookup."),
    ("I2", 1412, "handed over with"),
    ("I2", 1413, "the command that produced it."),
    ("I2", 1414, "**The one"),
    ("I2", 1418, "and no line position here is inferred from a neighbouring one."),
    ("I2", 1529, "`+15` to `+48`"),
    ("I2", 1532, "by resolving the cited range against both commits, and"),
    ("I2", 1533, "re-measured independently."),
    ("I2", 1535, "| Member |"),
    ("I3", 3070, "the twelve this session does not change"),
    ("I3", 3071, "14 - 2 = 12 (table census"),
    ("I3", 3080, "returns `12 of 17`"),
    ("I3", 3086, "should expect 12"),
    ("I3", 3107, "| `MutationOutcome` |"),
    ("I3", 3108, "| `DrawSessionResult` |"),
    ("I3", 3120, "| `ErrorCategory` |"),
    ("I3", 3199, "The **twelve** enums left alone here"),
    ("I3", 3200, "14 - 2 = 12 (table census)"),
    ("I3", 3214, "the twelve enums this entry leaves with a concrete member at zero"),
    ("I3", 3223, "No enum of the twelve"),
    ("I3", 3245, "CONCRETE_AT_ZERO 12"),
    ("I3", 3248, "the twelve with a concrete member at zero are the twelve rows of the table"),
    ("I3", 3249, "below."),
    ("I3", 3257, "The 12 of 17"),
    ("I3", 3267, "| 1 | `PathSegmentKind` |"),
    ("I3", 3278, "| 12 | `ErrorCategory` |"),
    ("I3", 3391, "**DELIBERATELY UNCHANGED**.**]**"),
    ("I3", 3393, "### CORE-036 "),
]


def fail(message):
    print("ERROR " + message)
    sys.exit(2)


def read_insertions(path):
    lines = open(path, "rb").read().decode("utf-8").replace(SEP, chr(10)).split(chr(10))
    found = []
    index = 0
    while index < len(lines):
        line = lines[index]
        if line.startswith(MARK):
            words = line.split()
            name = words[2]
            anchor = int(words[6])
            if lines[index + 1] != FENCE + "text":
                fail("no fence after marker " + name)
            end = index + 2
            while lines[end] != FENCE:
                end += 1
            found.append((name, anchor, lines[index + 2:end]))
            index = end
        index += 1
    return found


def unbold_count(line):
    parts = line.split(TICK)
    outside = "".join(parts[0::2])
    return outside.count("**")


def find_all(lines, block):
    hits = []
    width = len(block)
    for start in range(len(lines) - width + 1):
        if lines[start:start + width] == block:
            hits.append(start)
    return hits


def owner_of(src, number):
    for back in range(number - 1, 0, -1):
        text = src[back - 1]
        if (text.startswith(TAB + "public ") or text.startswith(TAB + "private ")) and "(" in text:
            return text.split()[0] + ":" + text.split("(")[0].split()[-1]
    return "?"


def hashes(line):
    spans = line.split(TICK)[1::2]
    return sum(1 for item in spans if 7 <= len(item) <= 40 and all(ch in "0123456789abcdef" for ch in item))


def cells(line, column):
    parts = [item.strip() for item in line.split("|")]
    return parts[column].strip(TICK) if column < len(parts) else ""


def main():
    contract, audit, source, manifest = sys.argv[1:5]
    inserts = read_insertions(contract)
    print("INSERT_SPECS %d" % len(inserts))
    data = open(audit, "rb").read()
    cr = data.count(CR)
    lf = data.count(LF)
    crlf = data.count(CRLF)
    print("CR %d LF %d CRLF %d LONE_CR %d" % (cr, lf, crlf, cr - crlf))
    print("RAW_SHA " + hashlib.sha256(data).hexdigest())
    lines = data.decode("utf-8").split(SEP)
    starts = []
    added = 0
    for name, anchor, block in inserts:
        hits = find_all(lines, block)
        added += len(block)
        starts.append((hits[0] if len(hits) == 1 else -1, name, anchor, block))
        print("OCCURRENCES %s %d" % (name, len(hits)))
    print("SPEC_ADDED_LINES %d" % added)
    if any(item[0] < 0 for item in starts):
        print("BASE_MATCH no (an insertion is missing or repeated)")
        return
    starts.sort()
    removed = 0
    base = list(lines)
    for start, name, anchor, block in starts:
        after = start - removed
        print("AT %s after base line %d want %d %s" % (name, after, anchor, "ok" if after == anchor else "WRONG"))
        removed += len(block)
    for start, name, anchor, block in sorted(starts, reverse=True):
        del base[start:start + len(block)]
    digest = hashlib.sha256(SEP.join(base).encode("utf-8")).hexdigest()
    print("BASE_LINES %d" % (len(base) - 1))
    print("BASE_MATCH " + ("yes" if digest == BASE_SHA else "no"))
    for start, name, anchor, block in sorted(starts, key=lambda item: item[1]):
        joined = " ".join(block)
        body = [item for item in block if item != ""]
        longest = max(len(item) for item in block)
        tabs = sum(item.count(TAB) for item in block)
        trailing = sum(1 for item in block if item != item.rstrip())
        early = sum(1 for item in body[:-1] if item.rstrip().endswith("]"))
        last = body[-1]
        bold = sum(unbold_count(item) for item in block)
        bad = [word for word in FORBIDDEN if word in joined]
        print("FORM %s LINES %d MAXLEN %d TABS %d TRAILING_WS %d EARLY_CLOSE %d BOLD %d LAST_ENDS %s" % (
            name, len(block), longest, tabs, trailing, early, bold, last[-6:]))
        print("HOUSE %s ATTR %s NUMBERED %s FORBIDDEN %s" % (
            name, "yes" if ATTR in joined else "no", "yes" if NUMBERED in joined else "no",
            ",".join(bad) if bad else "NONE"))
    bad_cites = 0
    for name, number, text in CITES:
        ok = text in base[number - 1]
        if not ok:
            bad_cites += 1
            print("CITE FAIL %s :%d [%s]" % (name, number, text))
    print("CITES %d FAILED %d" % (len(CITES), bad_cites))
    triage = set(cells(base[number - 1], 1) for number in range(3109, 3121))
    disp = set(cells(base[number - 1], 2) for number in range(3267, 3279))
    print("SETS triage_less_A %d dispositions %d EQUAL %s" % (len(triage), len(disp), "yes" if triage == disp else "no"))
    whole = set(cells(base[number - 1], 1) for number in range(3107, 3121))
    print("SETS_CONTROL with_class_A %d EQUAL %s" % (len(whole), "yes" if whole == disp else "no"))
    src = open(source, "rb").read().decode("utf-8").split(SEP)
    print("C4 HASHES %d CONTROL_807 %d" % (
        sum(hashes(base[number - 1]) for number in range(837, 843)), hashes(base[806])))
    guards = ["%d:%s" % (number, owner_of(src, number)) for number, item in enumerate(src, 1) if GUARD in item]
    print("SRC GUARD %d %s" % (len(guards), " ".join(guards)))
    print("SRC TOKEN_CONTROL %d" % sum(1 for item in src if "MutationOutcome" in item))
    owners = ["%d:%s" % (number, owner_of(src, number)) for number, item in enumerate(src, 1) if RETURN in item]
    print("SRC RETURNS %d %s" % (len(owners), " ".join(owners)))
    row = open(manifest, "rb").read().decode("utf-8").replace(SEP, chr(10)).split(chr(10))[81]
    head = row.find(SPAN_FROM)
    tail = row.find(SPAN_TO)
    span = row[head:tail + len(SPAN_TO)] if head >= 0 and tail > head else ""
    print("C15 SPAN %d IN_SPAN %d WHOLE_LINE %d" % (
        len(span), sum(span.count(word) for word in COMMANDS), sum(row.count(word) for word in COMMANDS)))


main()
````

What it reports: `INSERT_SPECS` (fences parsed from this contract; must be 3); the byte census and `RAW_SHA`; per
insertion `OCCURRENCES` (exact contiguous whole-line matches in the file); `ADDED_LINES`; per insertion `AT ... after
base line N want M ok|WRONG` (the base line it follows once all insertions are removed); `BASE_MATCH` (the sha256 of
the file with all three removed, against the `9f9ca69` digest); per insertion `FORM` (lines, longest line, tabs,
trailing whitespace, early `]` closers, `**` count outside code spans, last characters) and `HOUSE` (attribution
literal, `as numbered at` literal, forbidden needles); `CITES ... FAILED n` (each line the three texts cite, read on
the reconstructed base and required to contain the text the insertion relies on; a `CITE FAIL` line names any
miss); `SETS` (the triage table's names at base `:3109-3120` against the dispositions table's at `:3267-3278`);
`SRC GUARD` and `SRC RETURNS` (I1's two source counts, each `return TouchOnSuccess(` line with the `public` method
it sits in); and `C15 SPAN ... IN_SPAN ... WHOLE_LINE ...` (I2's command census over the C15 hand-over span, with
the whole line as its must-match control: the line carries an `awk` command for an unrelated count).

## 4. BP -- bold-marker parity

BP is `.sir/sessions/20261110-1857-tests-audit-corrections/bp.py` in the **main checkout** (sha256
`01ecfd8aefc048ee8cbf66b06c79cf1ad923521f4933405eb340a206dbc111ee`, re-checked by the architect). Verify the
digest, then run it by path, never a retyped copy (it holds a backslash):
`python -I C:/Users/dmarq/Development/Repos/dungeon-master/.sir/sessions/20261110-1857-tests-audit-corrections/bp.py <file>`.

At `9f9ca69` (architect, worktree): `01-domain-application.md blocks 24 odd 1`, the one `ODD (2245, 2369, 103, ...)`
being the bold-parity defect another backlog row owns (out of scope; this session leaves it alone). I3 adds a
block; I1 and I2 add none (their openers have no `[`), so after the change BP prints `blocks 25 odd 1`, its one ODD
line starting at 2245 + the lengths of I1 and I2 (section 5 states the number). A line of I1 or I2 ending in `]`
would close its enclosing 6b-xiv block early and move that block's parity, which is why U4 forbids it.

## 5. Validation record

All by the architect, 2026-11-11, fixtures built in the architect's scratch directory by a Python script that
splices section 2's fences into a copy of the worktree file at their anchors (the result an exact Edit must give).

- `icc.py`: `sha256sum` `71f0e4e7e98e02c9a813dde6bf39b518794b936bb47d8a1d8ea5f23c0337f874`, 195 lines, LF, trailing
  newline, 0 backslash, 0 tab (Python byte counts).
- **startRef file** (`9f9ca69`): `INSERT_SPECS 3`, `CR 3552 LF 3552 CRLF 3552 LONE_CR 0`, `RAW_SHA 58acb6b6...c467`
  (the base digest), `OCCURRENCES I1 0`, `I2 0`, `I3 0`, `BASE_MATCH no (an insertion is missing or repeated)`.
- **must-match fixture**: `CR 3608 LF 3608 CRLF 3608 LONE_CR 0`;
  `RAW_SHA f67d81a78a4b473f633a751367cef7ced445ddc863f024cfe86f2f2cb63fe715`; `OCCURRENCES` 1, 1, 1;
  `SPEC_ADDED_LINES 56` (I1 15, I2 10, I3 31; 3552 + 56 = 3608); `AT I1 after base line 842 want 842 ok`,
  `AT I2 ... 1418 ... ok`, `AT I3 ... 3391 ... ok`; `BASE_LINES 3552`; `BASE_MATCH yes`;
  `FORM I1 LINES 15 MAXLEN 109 TABS 0 TRAILING_WS 0 EARLY_CLOSE 0 BOLD 4 LAST_ENDS nged.)`;
  `FORM I2 LINES 10 MAXLEN 108 TABS 0 TRAILING_WS 0 EARLY_CLOSE 0 BOLD 0 LAST_ENDS nged.)`;
  `FORM I3 LINES 31 MAXLEN 111 TABS 0 TRAILING_WS 0 EARLY_CLOSE 0 BOLD 10 LAST_ENDS .**]**`;
  `HOUSE I1 ATTR yes NUMBERED yes FORBIDDEN NONE`, `HOUSE I2 ATTR yes NUMBERED no FORBIDDEN NONE`,
  `HOUSE I3 ATTR yes NUMBERED yes FORBIDDEN NONE`; `CITES 33 FAILED 0`;
  `SETS triage_less_A 12 dispositions 12 EQUAL yes`; `SETS_CONTROL with_class_A 14 EQUAL no` (near-miss: the
  fourteen with class A are not the dispositions' twelve); `C4 HASHES 0 CONTROL_807 1` (Correction 4's
  `:837-842` carry no backticked hash; must-match control `:807` carries `20b0b09`);
  `SRC GUARD 1 462:private:TouchOnSuccess`; `SRC TOKEN_CONTROL 33` (the file is the right file: measurement.md M1's
  `grep -c 'MutationOutcome'` also printed 33);
  `SRC RETURNS 5 241:public:RemoveArea 279:public:UpdateArea 314:public:UpdateAreaOutline 389:public:RemovePlacedObject 428:public:UpdatePlacedObject`
  (the five Correction 4 names at `:838-840`); `C15 SPAN 574 IN_SPAN 0 WHOLE_LINE 1` (must-match control: the whole
  C15 line carries `awk 'NR>=751 ...'`).
- **BP** on the must-match fixture: `blocks 25 odd 1`, `ODD (2270, 2394, 103, ...)` = the base's `(2245, 2369)` moved
  by I1 + I2 = 25 lines. Block spans, base -> fixture: `CORE-015` 6b-xiv `(792, 859, 50)` -> `(792, 874, 54)`;
  `CORE-019` 6b-xiv `(1402, 1572, 148)` -> `(1417, 1597, 148)`; `1618` block on `CORE-035` `(3311, 3391, 10)` ->
  `(3336, 3416, 10)`; I3 `(3418, 3447, 10)`. So neither 6b-xiv block closes early.
- **must-fail controls**, each on a copy of the must-match fixture:
  - one byte appended to line 10 -> `BASE_MATCH no` (all else as the fixture);
  - I1's last line ending `changed.]` instead of `changed.)` -> `OCCURRENCES I1 0`, `BASE_MATCH no (...)`. BP on
    that copy still prints `blocks 25 odd 1` with the same ODD line, so **ICC, not BP, is the instrument that
    catches a changed insertion**;
  - one CRLF (line 5) turned into LF -> `CR 3607 LF 3608 CRLF 3607 LONE_CR 0`: `LONE_CR 0` alone does not catch it,
    which is why the criterion requires `CR`, `LF` and `CRLF` equal; also `BASE_MATCH no`, `CITES 33 FAILED 32`;
  - I1 moved one line late (after the blank `:843`) -> `AT I1 after base line 843 want 842 WRONG` with
    `BASE_MATCH yes`: **`AT` is the only line that catches a misplaced insertion**;
  - one line added at the top of the file -> every `AT` `WRONG`, `BASE_MATCH no`, `CITES 33 FAILED 33`,
    `SETS ... EQUAL no` (the citation and set checks are position-sensitive, as intended).
