<!-- frozen: 20261029-1708-dir-backlog-to-sir, Gate 2 -->

# Contract: triage file

The interface between the six triage tasks (producers: T1, T2, T3, T4, T5, T6) and the fan-in task
(consumer: T8). T8 reads nothing about an item except what its triage row says. There is no language
form for this, so it is declared in markdown.

## 1. Files

All paths are relative to the repository root. Each file is owned by exactly one producer.

| Producer | File | Population it covers |
|---|---|---|
| T1 | `.sir/sessions/20261029-1708-dir-backlog-to-sir/triage/backlog-0001-0209.md` | `.dir-sdlc/backlog.md` lines 1-209 |
| T2 | `.sir/sessions/20261029-1708-dir-backlog-to-sir/triage/backlog-0210-0323.md` | `.dir-sdlc/backlog.md` lines 210-323 |
| T3 | `.sir/sessions/20261029-1708-dir-backlog-to-sir/triage/backlog-0324-0368.md` | `.dir-sdlc/backlog.md` lines 324-368 |
| T4 | `.sir/sessions/20261029-1708-dir-backlog-to-sir/triage/backlog-0369-0454.md` | `.dir-sdlc/backlog.md` lines 369-454 |
| T5 | `.sir/sessions/20261029-1708-dir-backlog-to-sir/triage/backlog-0455-0525.md` | `.dir-sdlc/backlog.md` lines 455-525 |
| T6 | `.sir/sessions/20261029-1708-dir-backlog-to-sir/triage/register.md` | `docs/audit/00-master-index.md` open and part-done rows |

"Line N" of `.dir-sdlc/backlog.md` means element N (1-based) of the file's bytes split on `\n`. An
**item line** is a line whose first four bytes are `- 20`. Only item lines are keys. An item's text may
continue on indented lines after its item line; those lines are read as part of the item and are never
keys.

## 2. File layout

In order:

1. Line 1: `# Triage: <population>`, where `<population>` is the third column of section 1.
2. Zero or more prose lines, none of which begins with `|`.
3. The header row, exactly: `| key | date | session | disposition | certainty | evidence | action | bundle | xref |`
4. The separator row, exactly: `|---|---|---|---|---|---|---|---|---|`
5. One data row per key in the file's population, and nothing else. No line after the table.

**Cell splitting.** A row is split on every `|` not immediately preceded by `\`, after removing one
trailing `\r` if present. The leading and trailing empty fields are discarded; exactly 9 cells remain,
each stripped of surrounding spaces. A literal `|` inside any cell is written `\|`.

**Ordering guarantee.** Data rows are in ascending key order: backlog keys by line number; register keys
by the numeric part of the id. Each key appears exactly once in its file and in no other triage file.

## 3. Columns

| # | Column | Type | Nullable | Rule |
|---|---|---|---|---|
| 1 | `key` | `.dir-sdlc/backlog.md:<N>` (N decimal, no padding) or `MASTER-<NNN>` (three digits) | no | Backlog files: N is an item line within the file's range. `register.md`: exactly `MASTER-081`, `094`, `098`, `108`, `110`, `126`, `128`, `130`, `137`, `139`, `142`, `143`, `144`, `157`, `158`, `178`, `180`. |
| 2 | `date` | `YYYY-MM-DD` | no | Backlog key: the date the item line opens with. Register key: the filing date its status cell or source names; otherwise `2026-08-17`. |
| 3 | `session` | a directory name under `.dir-sdlc/sessions/` | no | Backlog key: the backticked session id on the item line. Register key: the filing session its status cell or source names, as its directory name; otherwise `2026-08-17-code-quality-audit`. |
| 4 | `disposition` | `carry` \| `drop` \| `defer-permanent` | no | `defer-permanent` is permitted for `MASTER-158` only, and `MASTER-158` is always `defer-permanent`. |
| 5 | `certainty` | `decided` \| `undetermined` | no | `undetermined` means the recorded evidence cannot decide whether the item is closed. Permitted only with `carry`. |
| 6 | `evidence` | `-`, or one or more `<path>:<line>` or `<path>:<first>-<last>` tokens separated by `, ` | yes (`-`) | Every path begins `.dir-sdlc/` or `docs/audit/`, exists, and the cited lines exist. No path begins `src/`. Required (not `-`) for `drop` and `defer-permanent`. For a register key, always includes that row's own line in `docs/audit/00-master-index.md`. |
| 7 | `action` | one sentence of prose | no | `carry` / `defer-permanent`: the work that remains, stated so it can be scheduled without opening the source. `drop`: what the item asked and what closed it. No newline; `\|` for a literal pipe. |
| 8 | `bundle` | a bundle id from `contracts/bundles.md` section 1 | no | Subject to the membership rules of `contracts/bundles.md` section 2. Required for `drop` rows too, though T8 places no entry for them. |
| 9 | `xref` | `-`, or keys separated by `, ` | yes (`-`) | Each key names an item line of `.dir-sdlc/backlog.md` or a row `MASTER-NNN` present in `docs/audit/00-master-index.md`, other than the row's own key. Not required to be symmetric across files. |

## 4. Error cases and how they surface

| Case | Required behaviour |
|---|---|
| The records cannot decide whether an item is closed | `carry`, `certainty` = `undetermined`. Never `drop`. |
| Closure is inferable only by reading `src/` | Treated as undecidable: `carry`, `undetermined`. Source code is not evidence. |
| A producer believes an item other than `MASTER-158` should be permanently deferred | `carry`; the producer says so in its report. The disposition is not available to it. |
| An item line's date or session id cannot be parsed | The producer stops and escalates. It does not invent either value. |
| An item fits no bundle id | The nearest bundle id is used and the producer names the misfit in its report. No new id is created. |
| A backlog item and a register row describe the same work | Both rows exist and are never merged. A producer names the other key in `xref` wherever it notices the overlap; symmetry across files is not required and not checked. |
| A producer finds its population differs from section 1 (an item line outside its range, or a missing one) | The producer stops and escalates. |
| T8 finds the union of keys across the six files is not exactly the 181 item lines plus the 17 register ids, each once | T8 stops and escalates. It never edits a triage file. |
