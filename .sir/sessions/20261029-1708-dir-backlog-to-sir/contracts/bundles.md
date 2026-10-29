<!-- frozen: 20261029-1708-dir-backlog-to-sir, Gate 2 -->

# Contract: bundle vocabulary

The interface between the triage producers (T1-T6), which assign a bundle id to every triage row, and the
fan-in task (T8), which renders each id as a heading of `.sir/backlog.md`. Declared in markdown; there is
no language form.

## 1. Bundle ids, headings, and heading order

The heading column is the exact text of the line T8 writes, including the leading `## `. Rows are in the
order the headings appear in `.sir/backlog.md`.

| Order | Bundle id | Kind | Heading line |
|---|---|---|---|
| 1 | `test-six-row-sweep` | register | `## TEST six-row sweep: MASTER-126, 128, 137, 139, 143, 144` |
| 2 | `core-two-row-sweep` | register | `## CORE two-row sweep: MASTER-110, 180` |
| 3 | `core-heavy-rows` | register | `## CORE heavy rows: MASTER-157, 178, and the ObjectRotation/PathSegmentKind rename` |
| 4 | `master-142-alone` | register | `## MASTER-142 alone: the Windows shipping identity` |
| 5 | `part-done-rows` | register | `## Part-done register rows: MASTER-081, 094, 098, 108, 130` |
| 6 | `core-domain-application` | non-register | `## CORE domain and application code` |
| 7 | `app-ui` | non-register | `## App, UI and UIKIT code` |
| 8 | `test-coverage` | non-register | `## Test coverage and test strength` |
| 9 | `build-tooling` | non-register | `## Build, tooling, line endings and CI` |
| 10 | `audit-record-corrections` | non-register | `## Corrections to the audit record` |
| 11 | `process-and-criteria` | non-register | `## Process: criteria, contracts and orchestration` |
| 12 | `measurement-instruments` | non-register | `## Measurement instruments` |
| 13 | `permanently-deferred` | register | `## Permanently deferred` |

No other bundle id exists. A producer never creates one.

## 2. Membership rules

| Key | Bundle id |
|---|---|
| `MASTER-126`, `MASTER-128`, `MASTER-137`, `MASTER-139`, `MASTER-143`, `MASTER-144` | `test-six-row-sweep` |
| `MASTER-110`, `MASTER-180` | `core-two-row-sweep` |
| `MASTER-157`, `MASTER-178`, `.dir-sdlc/backlog.md:457` | `core-heavy-rows` |
| `MASTER-142` | `master-142-alone` |
| `MASTER-081`, `MASTER-094`, `MASTER-098`, `MASTER-108`, `MASTER-130` | `part-done-rows` |
| `MASTER-158` | `permanently-deferred` |
| every other `.dir-sdlc/backlog.md:<N>` key | one of the non-register ids (orders 6-12), chosen by its producer |

A register-kind id (orders 1-5 and 13) is used by the keys this table assigns to it and by no other key.

## 3. Rendering guarantees T8 provides

- A heading is written if and only if at least one `carry` or `defer-permanent` row carries its id.
- Each heading appears at most once, in the order of section 1.
- Every entry line sits after exactly one heading line: the heading of its row's bundle id, with no
  other `## ` line between them.
- `drop` rows produce no entry line under any heading.
