<!-- frozen at Gate 2: 20261110-1857-tests-audit-corrections, 2026-11-10 -->

# C1 -- Tests-audit corrections: block form, in-place replacements, per-block content, and the TAC and BP instruments

Consumers: `T1` (owns `docs/audit/05-tests.md`) and `T2` (owns `docs/audit/06-cross-cutting.md`). Both tasks
start at `07f0512` (`chore/repo-audit`). Every figure about the two audit files below was measured by the
architect on the session worktree `C:/dm-wt/20261110-1857-tests-audit-corrections` at `07f0512`, with the command
beside it; `sha256sum` printed the same digest for the worktree copy and the main-checkout copy of each file
(section 1). Every **git-history** fact is the orchestrator's spec-time measurement, quoted from
`.sir/sessions/20261110-1857-tests-audit-corrections/measurement.md` (sections M0-M5) and stated by the tasks as
told; tasks run without git and must not re-derive history.

Decided at Gate 2: the coverlet clause is not narrowed; a block records the census as its evidence (section
5.7). The two unmatched-bold blocks found by BP outside any backlog row are fixed in this session (sections 5.3 and
5.10, replacements R3 and R4). Mechanical defects inside earlier amendment blocks are fixed in place as R1-R4, each
recorded verbatim by its entry's new block.

## 0. Universals, each checked against every instruction in this contract

- **U1. Every change is a block insertion of section 2's form or one of the in-place replacements R1-R4 (section
  2.2), and nothing else.** Operational definition: TAC (section 3) prints `BASE_MATCH yes` with each listed
  `REPL` at `NEW 1 OLD 0`. Checked against sections 5.1-5.10: each instructs one block, and only 5.1 (R1), 5.3
  (R3), 5.5 (R2) and 5.10 (R4) instruct a replacement.
- **U2. No block labels anything by a `.sir/backlog.md` line number** (lessons.md:95: Gate 3 removes those rows,
  so the label would name another row). A backlog row is named by its originating session and its subject (the
  table in section 5.0). Operational definition: TAC needles `backlog.md:` and `row :` print `IN NONE`. Checked:
  no literal this contract requires in a block contains either string.
- **U3. Every block says `as numbered at 07f0512` at least once**, and every audit-file line number it cites is
  numbered at `07f0512` (each task's own insertions move the lines below them). Checked: every block in section 5
  is instructed to cite at least one audit-file line.
- **U4. Every block stating a git-history fact carries the literal ATTR** (section 5.0) -- blocks 5.1, 5.2, 5.4, 5.6,
  5.7, 5.8, 5.9. Blocks 5.3, 5.5 and 5.10 state only facts about the audit files at `07f0512`, which the task
  re-measures itself; they need not carry it.
- **U5. No tab character in any block, no body line other than the last ending in `]` outside a fence, and no
  path under a scratch or temp directory.** Operational definition: TAC prints `TABS 0` and `EARLY_CLOSE 0` for
  every block; the needle `AppData` prints `IN NONE`. Checked: no quoted text this contract requires contains a
  tab (5.1 cites the tab-bearing fenced lines `:275-279` by range and must not reproduce them), and every quoted
  line ending in `]` or `]**` is required inside a fence (section 2.1).
- **U6. A block makes no claim about whether an entry's substance still holds** beyond what section 5 instructs.
  The three rename blocks (5.2, 5.8, 5.9) say so in the literal NOCLAIM (section 5.0). This universal has no
  scanner; the reviewer reads it.

## 1. Files, base digests, line endings, tools

| File | owner | sha256 at `07f0512` (raw bytes) | CR | CRLF | lone CR | lines |
| --- | --- | --- | --- | --- | --- | --- |
| `docs/audit/05-tests.md` | T1 | `99ac05a316cf5007e69d068bf73d4a72ad58e0371db960da2fca0f5759e807c0` | 2384 | 2384 | 0 | 2384 |
| `docs/audit/06-cross-cutting.md` | T2 | `495dee996a0f841aa026e6f3988c03442972c7fac07bbec24ef5ddd528e74f7c` | 881 | 881 | 0 | 881 |

Census: TAC (section 3), Python byte counts of `0d` and `0d 0a`; agrees with measurement.md M0. Digests:
`sha256sum` on both the worktree and main-checkout paths.

- `Edit` is the only permitted writer (0 lone CR in both files, FORM 13 does not bite). `Write` and `sed -i` on
  either file are forbidden: both emit LF (lessons.md:35-36).
- Never end an `Edit` `new_string` on whitespace (it is stripped, lessons.md:38 and :51). Key every edit on
  non-whitespace text. Each replacement R1-R4 is one `Edit` whose `old_string` is the whole previous line.
- After editing: `LONE_CR 0`, `CR == CRLF`, and CRLF equals the startRef CRLF plus TAC's `ADDED_LINES`
  (replacements add no line).
- Tasks do not build, test, or run git. Nothing in this work needs any of them.

## 2. Forms

### 2.1 The block

The opener literal (note the trailing space):

    **[AMENDED 20261110-1857-tests-audit-corrections, 2026-11-10 -- 

- A block is whole lines: a blank line, then the opener line (opener at column 0) carrying a headline that ends
  `.**` (it may wrap), then body lines, the **last** line ending `**]**`. The house closer is `**]**` in both files
  (05's 44 closers all end `**]**`: `closers` census in section 4).
- It is inserted **immediately after the last line of its entry** (the anchor in section 5), so the entry's existing
  blank line and the next heading follow it. TAC's `NEXT` field proves the placement.
- **Previous wording quoted verbatim when it is a whole line that ends in `]` or `]**`, or that holds backticks,
  goes inside a fence**: a line that is exactly three backticks, the quoted line, a line that is exactly three
  backticks. That keeps it out of BP's count (BP skips fences) and out of TAC's closer search, and is the house
  form already used at `05:273-280`. Shorter verbatim quotes may be inline code spans.
- Any `**` meant literally (a quoted closer, a quoted bold opener) is inside a code span or a fence. BP parity of
  every block is even.
- Body lines wrap at a space, at no more than 112 characters where the text allows. No tab. No line other than the
  last ends in `]` outside a fence.

### 2.2 In-place replacements (exact whole lines)

Each is the only line in its file equal to its previous form at `07f0512` (a Python whole-line equality count
over the file split on CRLF, run by the architect, printed `1` for each; TAC's `REPL ... OLD 1` at startRef is the
same fact).

**R1** (T1, `05:274`, inside the fence of the `AMENDED 6b-iii` *CORRECTION 6, the dead location anchor* block on
`TEST-001`). Previous, then new:

````
$ git show 413be78:src/.../Validators/AddAreaCommandValidatorTests.cs | sed -n '15,19p'
$ git show 413be78:src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs | sed -n '15,19p'
````

**R2** (T1, `05:2077`, the closer of the `AMENDED 20261104-1726-test-hygiene-sweep` block on `TEST-029`).
Previous, then new:

````
made about any fact other than these five.**]**
made about any fact other than these five.** **]**
````

**R3** (T1, `05:980`, inside the `AMENDED 6b-ii` *CORRECTION 6* block on `TEST-012`). Previous, then new:

````
the code that exists now; the reasoning was wrong on the day it was written.
the code that exists now; the reasoning was wrong on the day it was written.**
````

**R4** (T2, `06:771`, the closer of the `AMENDED 20261106-0120-shipping-identity` block on `XCUT-015`).
Previous, then new:

````
`Package.appxmanifest:15`, and `<Properties>`, cited `:13-17`, is now `:19-23`.]**
`Package.appxmanifest:15`, and `<Properties>`, cited `:13-17`, is now `:19-23`.**]**
````

## 3. TAC -- the insertion-and-replacement checker (instrument, not deliverable)

Recreate it byte for byte with the `Write` tool in your own scratch directory as `tac_check.py`. It contains no
backslash by design (lessons.md:91). Its `sha256sum` was
`2e6a3a93d3d46c4ce57f23a65c114c990569fdf56ed3a76e109980df24025610` (LF, trailing newline, 111 lines); if yours
differs, compare against this text before trusting it. Run: `python -I <scratch>/tac_check.py <file> [needle ...]`
(a `C:/...` path, or an MSYS path in argv). Pass needles in single quotes; no needle in this contract or the
manifest contains a single quote. In the manifest a needle is delimited by backticks, or by single quotes where it
contains a backtick; delimiters are not part of the needle.

````python
import hashlib
import os
import sys

OPEN = "**[AMENDED 20261110-1857-tests-audit-corrections, 2026-11-10 -- "
CLOSE = "**]**"
FENCE = "```"
CR = bytes([13])
CRLF = bytes([13, 10])
SEP = chr(13) + chr(10)
TAB = chr(9)
BASE = {
    "05-tests.md": "99ac05a316cf5007e69d068bf73d4a72ad58e0371db960da2fca0f5759e807c0",
    "06-cross-cutting.md": "495dee996a0f841aa026e6f3988c03442972c7fac07bbec24ef5ddd528e74f7c",
}
# In-place replacements: id -> (line as the task leaves it, line as it stood at 07f0512).
REPL = {
    "05-tests.md": [
        ("R1", "$ git show 413be78:src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs | sed -n '15,19p'",
               "$ git show 413be78:src/.../Validators/AddAreaCommandValidatorTests.cs | sed -n '15,19p'"),
        ("R2", "made about any fact other than these five.** **]**",
               "made about any fact other than these five.**]**"),
        ("R3", "the code that exists now; the reasoning was wrong on the day it was written.**",
               "the code that exists now; the reasoning was wrong on the day it was written."),
    ],
    "06-cross-cutting.md": [
        ("R4", "`Package.appxmanifest:15`, and `<Properties>`, cited `:13-17`, is now `:19-23`.**]**",
               "`Package.appxmanifest:15`, and `<Properties>`, cited `:13-17`, is now `:19-23`.]**"),
    ],
}


def fail(message):
    print("ERROR " + message)
    sys.exit(2)


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
            fence = False
            early = 0
            while last < len(lines):
                body = lines[last]
                if body.startswith(FENCE):
                    fence = not fence
                elif not fence:
                    if body.endswith(CLOSE):
                        break
                    if body.rstrip().endswith("]"):
                        early += 1
                last += 1
            if last == len(lines):
                fail("unclosed block on line %d" % (index + 1))
            following = last + 1
            while following < len(lines) and lines[following] == "":
                following += 1
            after = lines[following][:40] if following < len(lines) else "EOF"
            block = lines[index:last + 1]
            tabs = sum(item.count(TAB) for item in block)
            found.append((heading, index + 1, " ".join(block), after, last - index + 2, early, tabs, fence))
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
    live, found = strip_blocks(text)
    print("INSERTIONS %d ADDED_LINES %d" % (len(found), sum(item[4] for item in found)))
    for where, number, body, after, _, early, tabs, fence in found:
        print("  AT %s line %d EARLY_CLOSE %d TABS %d NEXT [%s]" % (where, number, early, tabs, after))
    lines = live.split(SEP)
    for rid, new, old in REPL[name]:
        print("REPL %s NEW %d OLD %d" % (rid, lines.count(new), lines.count(old)))
        lines = [old if item == new else item for item in lines]
    digest = hashlib.sha256(SEP.join(lines).encode("utf-8")).hexdigest()
    print("SHA_AFTER_REVERSAL " + digest)
    print("BASE_MATCH " + ("yes" if digest == BASE[name] else "no"))
    for needle in needles:
        hits = [item[0] for item in found if needle in item[2]]
        print("NEEDLE [%s] IN %s OUTSIDE %d" % (needle, ",".join(hits) if hits else "NONE", live.count(needle)))


main()
````

What it reports: the byte census; each block's location (`AT` the enclosing `### ` entry id), its `EARLY_CLOSE`
count (body lines outside a fence, before the closer, ending in `]` -- such a line would end the block early for
BP) and `TABS` count, and the first 40 characters of the next non-blank line (`NEXT`); for each listed replacement,
how many whole lines of the text-outside-blocks equal its new form and its previous form (`REPL`); the sha256 of the
file with every block (and the blank line before it) removed and every replacement reverted, and whether it equals
the `07f0512` digest (`BASE_MATCH`); and, per needle, which blocks contain it (block lines joined with one space, so
a needle wrapped at a space is found) and how often it occurs **outside all blocks, before reversal** (`OUTSIDE`,
i.e. in the live text the reader sees outside this session's blocks).

Validated by the architect at `07f0512`, all fixtures built in the architect's scratch directory by a Python
script from the worktree files:
- startRef files: `05` prints `CR 2384 CRLF 2384 LONE_CR 0`, `INSERTIONS 0 ADDED_LINES 0`, `REPL R1 NEW 0 OLD 1`,
  `REPL R2 NEW 0 OLD 1`, `REPL R3 NEW 0 OLD 1`, `BASE_MATCH yes`; `06` prints `CR 881 CRLF 881 LONE_CR 0`,
  `INSERTIONS 0 ADDED_LINES 0`, `REPL R4 NEW 0 OLD 1`, `BASE_MATCH yes`.
- must-match fixture (R1-R4 applied; one dummy block at each anchor of section 5, the 5.1, 5.3, 5.5 and 5.10
  dummies quoting their previous line inside a fence): `05` printed `CR 2417 CRLF 2417 LONE_CR 0`,
  `INSERTIONS 7 ADDED_LINES 33`, the seven AT lines of T1's criterion in order with `EARLY_CLOSE 0 TABS 0`,
  `REPL R1/R2/R3 NEW 1 OLD 0`, `BASE_MATCH yes`; `06` printed `CR 893 CRLF 893`, `INSERTIONS 3 ADDED_LINES 12`,
  the three AT lines, `REPL R4 NEW 1 OLD 0`, `BASE_MATCH yes`. The 5.5 dummy's fenced quote ends `**]**` and was
  not taken as the closer (the fence-aware search), or `BASE_MATCH` would have printed `no`.
- must-fail controls on copies of that fixture: one byte appended to line 10 -> `BASE_MATCH no`; R2 written
  `these five.****]**` -> `REPL R2 NEW 0 OLD 0`, `BASE_MATCH no` (BP prints `odd 0` on that copy, so TAC is the
  instrument that catches a wrong replacement); a body line `see [this]` added to a block -> `EARLY_CLOSE 1`
  on that block (BP still prints `odd 0`, so TAC is the instrument that catches it); R2 left unapplied ->
  `REPL R2 NEW 0 OLD 1` (and BP `odd 1`). On copies of the `06` fixture: one byte appended to line 10 ->
  `BASE_MATCH no`; R4 left unapplied -> `REPL R4 NEW 0 OLD 1` and BP `odd 1`; R4 written `` `:19-23`.****]** `` ->
  `REPL R4 NEW 0 OLD 0`, `BASE_MATCH no`.

## 4. BP -- bold-marker parity, and the `closers` census

BP is `.sir/sessions/20261110-1857-tests-audit-corrections/bp.py` in the **main checkout** (sha256
`01ecfd8aefc048ee8cbf66b06c79cf1ad923521f4933405eb340a206dbc111ee`, re-checked by the architect with `sha256sum`).
Verify the digest, then run it by path, never a retyped copy (it holds a backslash, lessons.md:91):
`python -I C:/Users/dmarq/Development/Repos/dungeon-master/.sir/sessions/20261110-1857-tests-audit-corrections/bp.py <file>`.
It prints `<file> blocks N odd M` and one `ODD (start, end, count, tail)` per odd block.

At `07f0512` (architect, worktree): `05-tests.md blocks 44 odd 2` (`ODD (943, 990, ...)`, `ODD (2052, 2077, ...)`);
`06-cross-cutting.md blocks 1 odd 1` (`ODD (740, 771, ...)`). On the must-match fixture of section 3:
`05-tests.md blocks 51 odd 0`, `06-cross-cutting.md blocks 4 odd 0`. Controls of its own are in measurement.md M4.2
(`odd 0` must-pass, `odd 1` must-fail).

`closers` (the architect's second predicate for 05's closer form, which cannot share BP's closer rule): over lines
outside fences, count lines starting `**[AMENDED` and lines ending `**]**`. At `07f0512` it printed
`05-tests.md OPENERS 44 ENDS_FULL_CLOSER 44` and, for `06-cross-cutting.md`, `OPENERS 1 ENDS_FULL_CLOSER 0` with the
single line ending `]**` at `:771` -- agreeing with measurement.md M4.2's 44 and with the XCUT-015 defect.

## 5. Per-block content

### 5.0 Shared literals and the backlog rows

- **ATTR** (U4): `told: orchestrator measurement, 20261110-1857-tests-audit-corrections` -- each history block
  states it and cites `.sir/sessions/20261110-1857-tests-audit-corrections/measurement.md` by section (`M1`...`M4`),
  never by line.
- **NOCLAIM** (U6): `This block makes no claim about whether the finding of this entry still holds.` (no
  apostrophe, so it can be passed as a single-quoted needle).
- Reclassification literal (5.4, 5.6): `Reclassified: UNDETERMINED -> DRIFT.`
- The backlog rows, named in blocks by session and subject only (U2):

| Block(s) | backlog row's originating session | subject |
| --- | --- | --- |
| 5.1 | `2026-09-11-measure-then-close-the-test-gaps` | the elided `src/.../Validators/` path in the quoted `git show 413be78:` command |
| 5.7 | `2026-09-11-measure-then-close-the-test-gaps` | the coverlet "did not move in between" clause |
| 5.2, 5.8, 5.9 | `2026-10-16-infra-layer-batch` | the `DependencyInjection.cs` rename |
| 5.4, 5.5, 5.6 | `20261104-1726-test-hygiene-sweep` | reclassifying TEST-023 and TEST-030; the TEST-029 closer |
| 5.3, 5.10 | none: found by this session's BP census | unmatched bold delimiter |

### 5.1 `TEST-001` (T1; R1)

Anchor: the line ending `registered, so a defect dropping it only on that path would pass all nine new facts.**]**`
(`05:351`, closer of the `AMENDED 6b-x` block). Next: `### TEST-002`.
The block states: that the command in the fence of the `AMENDED 6b-iii` *CORRECTION 6* block (`:274`, as numbered at
`07f0512`) is **corrected in place**, and that block's DRIFT classification and its finding are unaffected; the
previous line verbatim, in a fence (the same form as 5.3, 5.5 and 5.10);
the full path `src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/AddAreaCommandValidatorTests.cs`,
derived by `git ls-tree -r --name-only 413be78 | grep AddAreaCommandValidatorTests` (M1, ATTR), the same path the
block itself quotes at `:264`; and that the corrected command prints the five lines the fence shows at `:275-279`
(M1, ATTR; `:274` is the command and `:280` the closing fence). It does not reproduce those five lines (they hold
tabs, U5). It names its backlog row per 5.0.

### 5.2 `TEST-002` (T1)

Anchor: the line `that asserts the model and the migrations have not diverged.` (`05:430`). Next: `### TEST-003`.
The block states, all from M3 with ATTR:
- the citation, verbatim: `src/DungeonMaster.Infrastructure/DependencyInjection.cs:50`, in the `**What.**`
  paragraph at `:377` (as numbered at `07f0512`), for `db.Database.Migrate()`;
- the rename: `69e3c14` (2026-10-16), `R100`, to `src/DungeonMaster.Infrastructure/InfrastructureServiceCollectionExtensions.cs`;
- exact when written: `git rev-parse <c>:src/DungeonMaster.Infrastructure/DependencyInjection.cs` printed blob
  `e2c9267` at `2603170` (this file's authoring commit), `676a8f3` and `413be78`, and `:50` of that blob is
  `db.Database.Migrate();` -- `Classification: DRIFT`;
- the content changed as well as the name (commits `ef0b0b6`, `33da60f`, `7b2586b`, `69e3c14` after `676a8f3`);
- the **counterpart** at `07f0512`, labelled a counterpart and not the cited code: `:59`
  `await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);` inside `MigrateDatabaseAsync`
  (re-measurable by the task in the worktree: `grep -n MigrateAsync src/DungeonMaster.Infrastructure/InfrastructureServiceCollectionExtensions.cs` printed `59:` alone);
- the disambiguation: the citation names the Infrastructure file; `src/DungeonMaster.Application/DependencyInjection.cs`
  is a different file that exists unrenamed;
- the front matter and the prose are left as written, per `02-infrastructure.md:1067-1068`;
- NOCLAIM; the backlog row per 5.0.

### 5.3 `TEST-012` (T1; R3)

Anchor: the line `that nothing else covers area removal (nothing does today).` (`05:1004`). Next: `### TEST-013`.
The block states: BP (section 4, by path and digest) printed `ODD (943, 990, ...)` at `07f0512` for the
`AMENDED 6b-ii` *CORRECTION 6* block; the bold opened at `:979` (`` `**The code the entry reasoned about was` ``) is
never closed, so the paragraph ending `:980` renders literal asterisks; `:980` is **corrected in place** by
appending `**` after `written.`; the previous line verbatim in a fence; that it is found by this session's census
and is **in no backlog row** (the literal `in no backlog row`). Line numbers as numbered at `07f0512`.

### 5.4 `TEST-023` (T1)

Anchor: the line `` `src/DungeonMaster.Tests`.**]** `` that closes the `AMENDED 20261104-1726-test-hygiene-sweep`
block (`05:1737`; the only line in 05 equal to it, by the same whole-line count). Next:
`### TEST-024`.
The block states, from M4.1 with ATTR: the heading of that block's *Correction 2* verbatim,
`*Correction 2 -- the other five line numbers. Classification: UNDETERMINED.*` (`:1728`, the correction spanning
`:1728-1732`, as numbered at `07f0512`); `Reclassified: UNDETERMINED -> DRIFT.`; the evidence: the test tree at
`2603170` (the authoring commit) extracted with `git archive`, 45 `.cs` files, and P1 as defined in
`.sir/sessions/20261104-1726-test-hygiene-sweep/contracts/C1-measurement.md` section 1, run over it: `P1 prints 13
lines`, among them `DungeonAggregateTests.cs:29`, `:43`, `:68` and `DungeonReadRepositoryTests.cs:51`, `:52`, the
five lines the correction left undetermined, each exact on the day; and that the title's "thirteen" was exact on
the day. The *Correction 2* text above is left standing. The backlog row per 5.0.

### 5.5 `TEST-029` (T1; R2)

Anchor: the R2 line as the task leaves it, `made about any fact other than these five.** **]**` (`05:2077`).
Next: `### TEST-030`.
The block states: the closer of the `AMENDED 20261104-1726-test-hygiene-sweep` block, at `:2077`, is **corrected in
place** (write the phrase on one line or wrapped at its space); the
previous line verbatim in a fence; why: the `**` after `five.` closes the bold opened at `:2076`
(`` `**No claim is` ``), so the final `**` was unmatched and rendered a literal `]**`; the new form keeps the bold
sentence and the file's closer token, which `all 44 closers` in this file used at `07f0512` (section 4 `closers`,
and measurement.md M4.2); BP printed `ODD (2052, 2077, ...)` at `07f0512` (name `bp.py`). Line numbers as
numbered at `07f0512`. The backlog row per 5.0.

### 5.6 `TEST-030` (T1)

Anchor: the line `` their own, recorded by task and file on `MASTER-128`.**]** `` (`05:2143`). Next: `### TEST-031`.
The block states, from M4.1 with ATTR: the heading of the `AMENDED 20261104-1726-test-hygiene-sweep` block's
*Correction 2* verbatim (it is the whole of `:2129`, one line; the correction spans `:2129-2136`, as numbered at
`07f0512`; the literal is given here on one line):

    *Correction 2 -- the second line number and the first method name. Classification: UNDETERMINED.*

then `Reclassified: UNDETERMINED -> DRIFT.`; the evidence: PA
(`C1-measurement.md` section 2 of that session) over the `2603170` tree: `PA prints 2 lines`,
`UpdatePlacedObjectCommandHandlerTests.cs:145` `var theObj = floor.Objects.Should().ContainSingle().Which;` and
`:186` `var obj = floor.Objects.Should().ContainSingle().Which;`; the method headers at `2603170`, `:133`
`HandleAsync_ObjectNotFound_ReturnsSuccessWithNoChange` and `:174` `HandleAsync_WithCustomScale_StoresScaleOnObject`,
with the command M4.1 names; so the entry's method name and `:186` were exact on the day. The backlog row per 5.0.

### 5.7 `TEST-032` (T1)

Anchor: the line `` 16 -- T4's own count at `5347813`.**]** `` (`05:2328`). Next: `## Layer verdict`.
The block states, from M2 with ATTR: the clause verbatim, joined across its line break at `:2238-2239` (as numbered
at `07f0512`): `9 is the same line it still stood at when T4 removed it, so it did not move in between`; that the
clause is **left standing**; `Classification: not a correction -- confirmation (measured)` (the register's label
for a claim found true, `05:2305`) [amended at Gate 3, A1: `:1334` and `:1348` removed]; the evidence the clause lacked: the per-commit
census over `413be78` plus `git rev-list 413be78..5347813^`, `105 commits`, of
`git show $c:src/DungeonMaster.Tests/DungeonMaster.Tests.csproj | grep -n coverlet`, which put coverlet at line 9 in
every one; the control `828327a` (`grep -n Microsoft.NET.Test.Sdk` printed `11:`), and that at `5347813` the grep
prints nothing; the three commits that touched the csproj in that range (`5347813`, `dfa49d7`, `828327a`); and the
limit: the census is at **commit granularity** and makes no claim about uncommitted working-tree states. The backlog
row per 5.0.

### 5.8 `XCUT-001` (T2)

Anchor: the line `rather than leaving it to be found by reading two files in two projects.` (`06:151`). Next:
`### XCUT-002`.
The block states, from M3 with ATTR, for the three citations (as numbered at `07f0512`): `:117`
`src/DungeonMaster.Infrastructure/DependencyInjection.cs:30-34` and `:118` `...DependencyInjection.cs:45-51`
(`locations:`), and `:130` `` `DependencyInjection.cs:47` `` (the `**What.**` paragraph; the bare name is the
Infrastructure file, inside `MigrateDatabase`):
- the rename `69e3c14`, `R100`, to `InfrastructureServiceCollectionExtensions.cs`;
- exact when written: blob `e2c9267` at `676a8f3` (this file's authoring commit), `2603170` and `413be78`; in it
  `:30-31` `AddDbContext`, `:33`-`:34` the two `AddScoped`, `:45`-`:51` `MigrateDatabase`, `:47`
  `using var scope = services.CreateScope();`; and `git grep -n -e CreateScope -e CreateAsyncScope 676a8f3 -- 'src/*.cs'`
  printed that one line -- the "exactly one hit" of `:130` was exact -- `Classification: DRIFT`;
- the content changed as well as the name (`ef0b0b6`, `33da60f`, `7b2586b`, `69e3c14`);
- the **counterparts** at `07f0512`, labelled counterparts and not the cited code: `:30-34` now
  `AddDbContextFactory<AppDbContext>` (`:30-31`) and two `AddSingleton` (`:33` `IUnitOfWorkFactory`, `:34`
  `IDungeonReadRepository`); `MigrateDatabaseAsync` at `:51-60`, which resolves `IDbContextFactory<AppDbContext>` and
  opens no scope; for `:47`, none -- no `CreateScope` in the file, and the repository-wide search over `src/`
  prints `10 lines`, all under `src/DungeonMaster.Tests/` (task re-measurable in the worktree:
  `grep -rn -e CreateScope -e CreateAsyncScope src --include=*.cs --exclude-dir=obj --exclude-dir=bin` printed 10
  lines in two test files, 6 and 4; and `grep -n -e AddDbContextFactory -e AddSingleton -e MigrateDatabaseAsync -e MigrateAsync -e CreateScope src/DungeonMaster.Infrastructure/InfrastructureServiceCollectionExtensions.cs`
  printed `30:`, `33:`, `34:`, `51:`, `59:`);
- the disambiguation from `src/DungeonMaster.Application/DependencyInjection.cs`; front matter and prose left as
  written per `02-infrastructure.md:1067-1068`; NOCLAIM; the backlog row per 5.0.

### 5.9 `XCUT-006` (T2)

Anchor: the line `place to land.` (`06:356`). Next: `### XCUT-007`.
The block states, from M3 with ATTR: the citation `:334` (as numbered at `07f0512`)
`src/DungeonMaster.Infrastructure/DependencyInjection.cs:33-34`; the rename (`69e3c14`, `R100`,
`InfrastructureServiceCollectionExtensions.cs`); exact when written: blob `e2c9267` at `676a8f3`, `:33`
`services.AddScoped<IDungeonWriteRepository, DungeonWriteRepository>();` and `:34`
`services.AddScoped<IDungeonReadRepository, DungeonReadRepository>();` -- `Classification: DRIFT`; the
**counterpart** at `07f0512`: `:33` `AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>`, `:34`
`AddSingleton<IDungeonReadRepository, DungeonReadRepository>`, `IDungeonWriteRepository` no longer registered in that
file, `src/DungeonMaster.Infrastructure/Persistence/UnitOfWork.cs:15` constructing `DungeonWriteRepository`
(task re-measurable: `grep -n IDungeonWriteRepository src/DungeonMaster.Infrastructure/Persistence/UnitOfWork.cs`
printed `15:`); the disambiguation; front matter left as written per `02-infrastructure.md:1067-1068`; NOCLAIM; the
backlog row per 5.0.

### 5.10 `XCUT-015` (T2; R4)

Anchor: the R4 line as the task leaves it (`06:771`). Next: `### XCUT-016`.
The block states: BP printed `ODD (740, 771, ...)` at `07f0512` (name `bp.py`); the opener's bold closes at
`same change.**` (`:742`), the body opens no bold, so the final `**` of `:771` was unmatched and rendered a
literal `]**`; `:771` is **corrected in place** to end `.**]**`, the closer 05 uses throughout; the previous line
verbatim in a fence; `in no backlog row`. Line numbers as numbered at `07f0512`.

## 6. Shared needles, checked against each other

- `corrected in place`: required in 5.1, 5.3, 5.5 (05) and 5.10 (06); criteria use containment.
- `Classification: DRIFT`: required in 5.2, 5.8, 5.9; `Reclassified: UNDETERMINED -> DRIFT` in 5.4, 5.6 (does not
  contain `Classification: DRIFT`); criteria use containment.
- NOCLAIM: 5.2, 5.8, 5.9; it does not contain, and is not contained in, `No claim is` (TEST-029's own bold, quoted
  in 5.5).
- `five.**]**` is not a substring of `five.** **]**` (space). `src/.../Validators` is not a substring of the full
  path. `wrong on the day it was written.**` is a substring of R3's new line only (TAC at `07f0512`: `OUTSIDE 0`).
- `in no backlog row`: 5.3, 5.10 only by instruction; criteria use containment.
