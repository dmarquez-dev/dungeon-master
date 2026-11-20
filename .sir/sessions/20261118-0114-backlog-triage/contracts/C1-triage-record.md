<!-- frozen at Gate 2 of 20261118-0114-backlog-triage -->

# Contract C1: triage record

The interface between the six triage tasks (producers: T1-T6) and the apply task (consumer: T7).
T7 learns nothing about a backlog row except what its record block says, or what
`contracts/C2-gate2-decisions.md` says where that contract names the row. There is no language form
for this, so it is declared in markdown, and its operative definition is the checker
`contracts/C1-check.py` beside this file: where this prose and the checker disagree, the checker is
what the session enforces and the disagreement is reported as a contract defect.

All paths below are relative to the repository root unless they are absolute. The session directory
in the main checkout is `C:/Users/dmarq/Development/Repos/dungeon-master/.sir/sessions/20261118-0114-backlog-triage/`
(written `<S>` below). The worktree is `C:/dm-wt/20261118-0114-backlog-triage` (written `<W>`), at
`4d874be48dae2c853e8f564eed5f065cc00cc9f2` (written `4d874be`) when the triage tasks run.

## 1. The reference files

| Name | File | sha256 at 4d874be |
|---|---|---|
| ORIGINAL BACKLOG | `C:/Users/dmarq/Development/Repos/dungeon-master/.sir/backlog.md` (main checkout) | `5cfbfb9d8e76ed0dc26182f7b9772adc4c27794dd6a3592092905e24f0a3fe84` |
| ORIGINAL LESSONS | `C:/Users/dmarq/Development/Repos/dungeon-master/.sir/lessons.md` (main checkout) | `6d185c70424d4b7dfbe60b6eff571447f80ac08bb81b849235e431664420adbe` |

The main checkout's two files are byte-identical to `<W>/.sir/backlog.md` and `<W>/.sir/lessons.md`
at 4d874be (both hashes measured by Python `hashlib` and by `sha256sum`, which agree). They are the
pre-edit reference because the worktree copies are the files T7 changes, and no task may run git.
`C1-check.py` refuses to run if either hash differs.

**Line N** of a file means element N (1-based) of its bytes decoded as UTF-8 and split on `\r\n`,
after dropping the one empty element the final `\r\n` leaves. A **row** is a line of ORIGINAL BACKLOG
matching `^\d{4}-\d{2}-\d{2} \S+ ` (Python `re.match`). Its **heading** is the nearest preceding line
beginning `## `. The **open rows** are the 132 rows whose heading is not `## Permanently deferred`;
line 141 (MASTER-158) is the one row excluded and no record names it. A row's **date** and **session**
are its first two space-separated fields; its **action** is the text between the session and the
row's single ` -- source: `; its **tail** is the text from ` -- source: ` (inclusive) to the end of the
line. Every row contains ` -- source: ` exactly once at 4d874be (Python count over all 133 rows), so
the split is unambiguous even on lines 130 and 158, which contain ` -- ` twice.

## 2. Files and populations

Each record file is owned by exactly one producer, is a new file in the worktree, and may be LF or
CRLF (record files are session records, not CRLF-governed; the checker accepts either).

| Producer | File (in `<W>`) | Range | Open rows |
|---|---|---|---|
| T1 | `.sir/sessions/20261118-0114-backlog-triage/triage/rows-0007-0030.md` | lines 7-30 | 21 |
| T2 | `.sir/sessions/20261118-0114-backlog-triage/triage/rows-0034-0058.md` | lines 34-58 | 25 |
| T3 | `.sir/sessions/20261118-0114-backlog-triage/triage/rows-0062-0087.md` | lines 62-87 | 23 |
| T4 | `.sir/sessions/20261118-0114-backlog-triage/triage/rows-0091-0109.md` | lines 91-109 | 19 |
| T5 | `.sir/sessions/20261118-0114-backlog-triage/triage/rows-0110-0137.md` | lines 110-137 | 25 |
| T6 | `.sir/sessions/20261118-0114-backlog-triage/triage/rows-0145-0163.md` | lines 145-163 | 19 |

A file's **population** is every open row whose line number lies in its range: 132 in all, each in
exactly one file (counts printed by `python -I <S>/contracts/C1-check.py populations`).

## 3. File layout

1. Line 1, exactly: `# Triage: .sir/backlog.md lines <first>-<last> at 4d874be`, with `<first>` and
   `<last>` the range of section 2 written without padding.
2. Zero or more prose lines, none beginning `### `.
3. One **block** per row of the population, in ascending line order, and nothing else but blank
   lines between them. No line follows the last block except blank lines.

A block, line by line (`<...>` is a value; every other character is literal):

```
### row <N>
- date: <date>
- session: <session>
- disposition: <remove|keep|rewrite|lesson|decided>
- decision: <D<n>|->
- lessons-line: <M|->
- xref: <-|N1, N2, ...>
- summary: <one sentence>
- row:
~~~~
<the row's line, verbatim>
~~~~
- evidence:
~~~~
<one or more lines>
~~~~
- new-text: -
```

where the last field is either the single line `- new-text: -` or the three-part form

```
- new-text:
~~~~
<exactly one line>
~~~~
```

**Fences and escaping.** A fence line is exactly `~~~~`. Inside a fence every line is literal text up
to its line terminator: `|`, `\`, backticks and `\|` stand for themselves and nothing is escaped,
unescaped or trimmed. A field line's value (`- key: value`) is likewise literal to the end of the
line, with one space after the colon. The only text a record cannot carry is a fence-content line
that is exactly `~~~~` (0 occurrences of `~~~~` in ORIGINAL BACKLOG or ORIGINAL LESSONS at 4d874be,
Python `str.count`). There is therefore no escape for the escape to define: the format has no escape.
Rows that exercise this at 4d874be: line 128 carries a backslash; lines 87, 137 and 154 carry `|`
(Python, `chr(92) in line` and `'|' in line` over the open rows). `C1-check.py self-test` round-trips
lines 87 and 128 through a block and back.

## 4. Fields

| Field | Rule |
|---|---|
| `N` | The row's line number at 4d874be, decimal, no padding. |
| `date`, `session` | Equal the row's date and session (section 1). |
| `disposition` | `remove`: the row's work is done, or its claim is stale, and the evidence shows it. `keep`: still true and still worded as work; T7 leaves the line byte-identical. `rewrite`: true but mis-worded; T7 replaces the line with `new-text`. `lesson`: a standing rule rather than work (discussion.md E2); T7 removes the row and, when `new-text` is given, appends it to `.sir/lessons.md`. `decided`: the row begins `Decide` and is answered by C2; permitted only for a row C2 lists. |
| `decision` | `D<n>` exactly when `disposition` is `decided`, naming the C2 entry whose `rows` include N; otherwise `-`. |
| `lessons-line` | Only with `lesson`: the line M (16-149) of ORIGINAL LESSONS that already states the rule, as read with `grep -n` at 4d874be; then `new-text` is `-`. Otherwise `-`. |
| `xref` | `-`, or the open-row line numbers at 4d874be of rows that ask related or overlapping work, `, `-separated, ascending, never N itself. Not required to be symmetric. |
| `summary` | One sentence giving the reason for the disposition, legible without opening the evidence. For `decided`, what the re-measurement found, and nothing that decides the row. |
| `row` | The row's line verbatim (fence content equals line N of ORIGINAL BACKLOG exactly). |
| `evidence` | At least one line beginning `$ ` (a command, run in `<W>` at 4d874be with paths relative to `<W>` or absolute), each followed by the output it printed, verbatim or with elided runs marked by a line `[... K lines elided ...]` stating K. A `file:line` read is cited as the `grep -n` command that printed it. A command that could not run is recorded as `$ <command>` followed by `NOT RUN: <reason>`. A rule found already stated in a framework skill file may be cited by that file's absolute path with `grep -n` output. |
| `new-text` | Required (three-part form) for `rewrite`, and for `lesson` with `lessons-line` `-`; the single line `- new-text: -` otherwise. Its one line is the exact text T7 writes, shaped per section 5. |

## 5. Shapes of new text

**Rewritten row** (`rewrite`). The line is `<date> <session> <action> <tail>` where date and session
are the original row's, `<action>` is the new wording (non-empty, different from the original
action, containing no ` -- source: `), and `<tail>` is the original tail, either verbatim or with its
first `records undetermined; ` removed. It names other backlog rows by session and subject, never by
line: its action matches no `(?<!dir-sdlc/)backlog\.md:\d|\b(?:row|line)s? :?\d+\b(?!-)`, the
LINE-REF pattern (`.sir/lessons.md:95` at 4d874be; no original action matches it, Python over the
133 rows; it must match `row :147` and `.sir/backlog.md:12` and must not match
`row 2026-09-14-make-the-floor-sidebar-operable`, checked by `C1-check.py self-test`). A row whose work is a human or Appium check is written as numbered
steps `(1) ... (2) ...`, a pass condition introduced by `Pass:`, and at least one `<path>:<line>`
anchor stated `at <commit>` with a 7-40 character hex commit (the 20261111-0156 lesson,
`.sir/lessons.md:100` at 4d874be). Lines 25, 52 and 159 are such rows (discussion.md, Decisions):
`C1-check.py record` requires each to be `rewrite` with new text carrying `(1) `, `(2) `, `Pass:` and
an anchor matching `[\w./-]+:\d+(?:-\d+)?`? at `?[0-9a-f]{7,40}\b` before its tail; any other
human-check row is held to the same form by the reviewer. Cited `.dir-sdlc/backlog.md:N` and `MASTER-NNN` references are
stable and may stay.

**Lesson line** (`lesson` with new text). Matches, as one line,

`^(\d{4}-\d{2}-\d{2}) (\S+) \[(project|framework|harness)\] (.+) \(source: \.sir/backlog\.md row (\S+) (.+), routed by 20261118-0114-backlog-triage\)$`

with group 1 the row's date, groups 2 and 5 the row's session, and group 6 a few words naming the
row's subject (for example `` `ResultSentinelIsAbsentTests` empty-file assertion ``). The whole
line matches no LINE-REF. The date and
session are the original row's filing date and session, so appended lines are not in date order;
the file's FORM lines (16-43) already are not.

## 6. What T7 produces from the records

**Resolution.** Row N's final disposition and new text are those of the `#### row N` sub-block of
C2 when C2 has one (C2 wins, for a Decide row and for any other row an answer reaches), and
otherwise those of N's record block. A record `decided` with no C2 sub-block for N is an error.

**`.sir/backlog.md`.** Starting from ORIGINAL BACKLOG's lines:

1. Every `remove` and `lesson` row is deleted; every `keep` row is unchanged; every `rewrite` row is
   replaced in place by its new text. Line 141 is unchanged. No other line is added or changed except
   in steps 2 and 3.
2. Line 3 gains, after its last character and one space, exactly:
   `` Their re-measurement at `4d874be`, with each open entry's disposition and evidence, lives in `.sir/sessions/20261118-0114-backlog-triage/triage/`. ``
3. A `## ` heading with no row beneath it before the next heading or end of file is deleted with
   the blank line after it; then each run of consecutive blank lines is reduced to one, and blank
   lines at end of file are deleted.
4. Every line ends `\r\n`, including the last; the file has CRLF = LF = CR (lone CR 0) by Python
   byte counts.

**`.sir/lessons.md`.** ORIGINAL LESSONS' bytes, unchanged, followed by one line per resolved `lesson`
disposition that carries new text, in ascending N, each terminated `\r\n`; CRLF = LF = CR.

Neither file is written with `Write` or `sed -i`. An `Edit` `new_string` never ends on whitespace
(`.sir/lessons.md:51` at 4d874be). A Python script that reads and writes bytes is the recommended
route (`.sir/lessons.md:53`); a script containing backslashes is written to a file with the `Write`
tool rather than passed inline through Bash (`.sir/lessons.md:91`).

## 7. Error cases

| Case | Required behaviour |
|---|---|
| The evidence cannot settle whether the work is done without a build, a test run or a git command | No triage task builds, runs tests or runs git; it reads files in `<W>`, whose content outside `.sir/sessions/` is 4d874be's until T7 runs, since T1-T6 commit only their record files. `C1-check.py record` rejects an evidence line beginning `$ git` or `$ dotnet` unless the next line begins `NOT RUN:`. The row is `keep` (or `rewrite` where the wording is also wrong), its evidence ends with the `NOT RUN:` line naming the command that would settle it, and the task names the row in its report. |
| A row other than a C2 row looks like it needs the user's decision | `rewrite` it as a row whose action begins `Decide`, with the options stated, and name it in the report. The task never decides it. |
| A `decided` row's re-measurement contradicts the premise C2 records for it | The evidence records what was measured; the disposition stays `decided`; the task reports the contradiction as a contract-change escalation naming the C2 entry. |
| A row fits two dispositions | The one earlier in this order wins: `remove`, `lesson`, `rewrite`, `keep`; the summary names the other. |
| Two rows ask the same work | Each keeps its own block; each names the other in `xref`. Deleting one as a duplicate is a `remove` whose evidence quotes the surviving row's line with `grep -n`. |
| Two rows in one file state one standing rule | The lower-numbered row is `lesson` and carries the new line; the other is `remove`, its evidence quoting the first row's line with `grep -n` and its summary naming that row's session and subject. Two files cannot coordinate, so a rule routed from two ranges may reach T7 twice; T7 applies both and its reviewer reports the pair as an advisory. |
| A producer finds its population differs from section 2 | It stops and escalates. |
| T7 finds `C1-check.py union` reporting anything other than 132 keys with 0 missing, 0 extra, 0 duplicated, or `C1-check.py c2` reporting any pending entry or error | T7 stops and escalates, and edits no file. It never edits a record file or C2. |
