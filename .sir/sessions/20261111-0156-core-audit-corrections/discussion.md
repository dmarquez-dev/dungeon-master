# CORE audit corrections

Correct the three open audit-record backlog rows that target `docs/audit/01-domain-application.md`
(CORE-035, CORE-019, CORE-015), and remove those three rows from `.sir/backlog.md` at merge.

## Context

All fifteen earlier SIR sessions are WRAPPED and the five scheduled headings at the top of
`.sir/backlog.md` are empty, so the orchestrator chose this unit. Layer concentration has beaten
severity ordering in recent sessions (`20261110-1618-audit-record-corrections`,
`20261110-1857-tests-audit-corrections`), and these are the only three rows under
"Corrections to the audit record" whose target is the CORE audit file alone.

The three rows, as numbered in `.sir/backlog.md` at `9f9ca69`:

- `:96` -- Add a parenthetical to `CORE-035` in `docs/audit/01-domain-application.md` naming each of
  its two distinct "twelve" figures (the post-change `12 of 17` concrete-at-zero census and the
  14 - 2 = 12 enums left unchanged) so neither can be quoted as the other.
- `:97` -- Add a one-clause qualifier to the exception sentence of the `AMENDED 6b-xiv` preamble on
  `CORE-019` saying the eight-row drift table was handed over with a method rather than a command,
  since the preamble's "every figure ... with the command" universal still reaches it.
- `:98` -- Restate the `20b0b09` anchor inside `CORE-015`'s Correction 4 paragraph, whose
  `grep -c "if (outcome is not MutationOutcome.Success)"` returns 5 there but 1 at `HEAD` after
  6b-xiv's own extraction.

Presence checked at discuss time with `grep -n` on the working tree at `9f9ca69`: `CORE-015`
heading `:751`, `CORE-019` heading `:1261`, `CORE-035` heading `:3045`, `CORE-015` Correction 4 at
`:837`, `AMENDED 6b-xiv` opener on `CORE-019` at `:1402`. `grep -c twelve` on the file printed 12.
None of the rows' claims has been re-measured yet; every one is "re-measure before scheduling".

## Decisions

- Track: Standard (one file, but several independently checkable claims).
- Scope is exactly backlog rows `:96`, `:97`, `:98`. Nothing else in `01-domain-application.md`
  and no other audit file.
- Excluded: the `01-domain-application.md:2245` bold-parity item, because it is one of six in
  backlog row `:184` and taking it alone would split that row; and every other audit-correction
  row (files 00, 02-06).
- Form of the corrections: follow the precedent of the two previous audit-correction sessions --
  dated, attributed insertions (AMENDED blocks or inline qualifiers) that leave earlier text
  readable; no earlier text is deleted.
- Each row's claim is re-measured in the spec phase before a criterion is written; a row found
  false or stale is reported at Gate 2, not silently closed.
- At merge, the three rows are removed from `.sir/backlog.md`.
