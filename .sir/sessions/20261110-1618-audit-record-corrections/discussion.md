# Audit record corrections

Correct six stale or inaccurate statements in the audit register (`docs/audit/00-master-index.md`, `docs/audit/01-domain-application.md`), taken from the unscheduled Gate-3-appended rows of `.sir/backlog.md`, and remove those six rows from the backlog at Gate 3.

## Context

The five scheduled headings at `.sir/backlog.md:5-13` are all empty, and the previous session (20261109-2239-app-comment-fixes) closed the App comment rows and declared `docs/audit/` corrections out of its scope. This session takes the audit-record rows that were filed most recently at Gate 3 advisories, because each one already carries the instrument that checks it.

Each row's target was spot-checked to still exist at discuss time (HEAD `c90da08`, `chore/repo-audit`, clean tree). These were existence checks, not re-measurements; several line numbers have drifted since the rows were filed, and every row must be re-measured in spec before a criterion is written on it.

- `.sir/backlog.md:179`: the CORE-022 AMENDED block (row cites `docs/audit/01-domain-application.md:1600-1601`) says the 11f9562 figures are "stated in this session's C1 and C2 contracts", but `FloorObjectMethods.cs`'s 112 lines and the 288 total are stated only in C3 (`.sir/sessions/20261105-0106-core-two-row-sweep/contracts/C3-register.md:64-65`). At discuss time `sed -n '1600,1601p'` showed text about `Floor` methods, so the block has moved and must be located by content.
- `.sir/backlog.md:182`: the CORE-035 entry cites `ObjectKindInfo.cs:24-28` for `GetDefaultAnchor`; `grep -n GetDefaultAnchor src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs` printed `:40`. `grep -n 'ObjectKindInfo.cs:24-28' docs/audit/01-domain-application.md` printed `:2951` and `:3033` (the row said `:2802` and `:2884`). The row asks for the drift to be recorded in the CORE-035 AMENDED block's drift paragraph.
- `.sir/backlog.md:183`: the CORE-035 AMENDED block's `RotationDirection` row (row says `:3012`; `grep -n 'grep -rn RotationDirection'` printed `:3161`) states a bare `grep -rn RotationDirection` whose figure depends on filters; the row gives the filtered instrument.
- `.sir/backlog.md:184`: "a renumbering, not a rename" (`00-master-index.md:318`, `01-domain-application.md:~1882`) and "a data migration, not a renumbering" (`00-master-index.md:366`, `01-domain-application.md:~3058`) are opposite wordings of the same substance. The row's grep instrument also matches many unrelated "renumbering" lines (floor renumbering, rank renumbering), so the population to reconcile is the zero-member wordings only.
- `.sir/backlog.md:190`: the "considered and declined" attribution in `MASTER-081`'s cell (`00-master-index.md:269`) and the CORE-016 amendment block (`01-domain-application.md:~1063`) pins the list to `.sir/sessions/20261109-1641-part-done-rows/discussion.md` Decisions, which does not name the policy layer; that session's `spec.md:24` does.
- `.sir/backlog.md:191`: `MASTER-119` (`00-master-index.md:307`) cites `01-domain-application.md:2313`; the text it means is CORE-030 Correction 2, which `grep -n 'the fifth citation, stale by PROJECT'` put at `:2560`.

## Decisions

- Track: Standard. Two files, but six independently checkable claims; per the "count the judgments" rule this is not Small.
- Session name: `audit-record-corrections`.
- In scope: the six rows at `.sir/backlog.md:179`, `:182`, `:183`, `:184`, `:190` and `:191`, and nothing else.
- Out of scope: the older audit-correction rows at `.sir/backlog.md:88-108`; `docs/audit/05-tests.md` (`:177`); the session-path-citation questions (`:170`, `:178`); the `obj` rename population (`:189`); any source-code change; anything needing a running app or a human observation.
- No production or test code changes; the deliverable is register prose only, so no build is needed for evidence, but the main-checkout line-ending state of each edited file must be proven by byte counts (lone CR = total `0d` minus `0d 0a`, measured before and after; lessons FORM 13 and edit-lone-cr), and a file carrying lone CR must not be written with `Edit`.
- Every corrected citation is re-derived by a command at the session's base commit and the command is stated with it (lesson: a correction is new prose and gets no exemption).
- `:184` reconciles only the zero-member wordings; spec decides the shared phrasing from repository evidence and lists every site it changes, counted by a predicate with a must-match and a must-not-match control.
- Line citations the corrections add should be anchored on a quoted line and its content, not derived by subtraction.
- Each closed row is removed from `.sir/backlog.md` at Gate 3; `.sir/backlog.md` is measured for lone CR first, and any blank line left under an emptied heading is handled as lesson 2026-11-09 records (no heading is expected to empty: all six sit under `## Appended at Gate 3`).
- Base branch is `chore/repo-audit`, not `main` (lesson 2026-11-09 20261109-1641-part-done-rows).
