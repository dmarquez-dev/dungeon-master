# Coverage mapping — 2026-09-14-pluralise-entity-namespaces (6b-v)

Written at Gate 2, before the approval that freezes `manifest.json`. The `End state` column is filled at Gate 3.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. The three namespaces pluralised throughout `src/` — 14 decls, 70 usings, 76 qualified refs | T1, T2, T3, T4, T5, T6 | covered |  **met** — 0 singular occurrences remain in `src/`; 14 plural declarations; 44+13+8+6+3 usings converted |
| 2. The three Domain folders renamed to match `dotnet-format.md:30`/`:58` | T1 | covered |  **met** — `Entities/` lists `Areas`, `Dungeons`, `Floors`, `Entity.cs`, `MutationOutcome.cs` (T1 c2) |
| 3. `dotnet build src/DungeonMaster.sln` reports `0 Warning(s), 0 Error(s)` | T1–T6, each scoped to its own project | uncovered — see G3 |  **met** — orchestrator ran it: `Build succeeded. 0 Warning(s), 0 Error(s)` across all TFMs |
| 4. All six doc-comment `cref`s updated | T1 (5), T3 (1) | covered |  **met** — T1 c5 (5 crefs), T3 c2 (1 cref), both scans 0 singular |
| 5. The 48 EF snapshot strings handled and the instrument run with a control | T3 | covered |  **met** — 0 singular / 48 plural (T3 c3); instrument found blind, closed statically instead |
| 6. The `Dungeons` collision does not fire | T2 (1 file), T6 (13 files) | covered |  **met** — T2 c2, T6 c3; green builds, no `CS0234` |
| 7. Baseline 908/0 preserved, no assertion rewritten | T5 (97 + 23), T6 (788) | uncovered for the 908 total — see G3 |  **met** — 788 + 97 + 23 = 908, summed by the orchestrator |
| 8. No `.xaml` edited | T5 | covered |  **met** — T5 c3; 0 `.xaml` paths in the session diff |
| 9. `CORE-004`'s false parenthetical corrected | T7 | covered |  **met** — T7 c1/c2, classified MEASUREMENT ERROR |
| 10. `MASTER-022` closed in `00-master-index.md` | none | uncovered — see G4 |  pending — orchestrator work after the merge |
| 11. The 78 stale folder-path citations across `docs/audit/` corrected | T7 | uncovered at decomposition — **folded into T7 by user decision at Gate 2**, see G1 |  **met** — T7 c5/c6; residual exactly 6, all `CORE-004` `locations:` |

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `target-namespaces` §1 — the three target strings | | T2, T3, T4, T5, T6 | covered | consumed — see note |
| `target-namespaces` §2 — what does not move; no type or file renamed | | T1; test-folder clause T6 | covered | consumed — see note |
| `target-namespaces` §3 — per-project file inventory | | each task, its own row | covered | consumed — see note |
| `target-namespaces` §4(a) — namespace declaration shape | | T1 | covered | consumed — see note |
| `target-namespaces` §4(b) — plain vs alias `using`, two counting commands | | T2, T3, T4, T5, T6 | covered | consumed — see note |
| `target-namespaces` §4(c) — qualified-reference shape | | T1, T3, T6 | covered | consumed — see note |
| `target-namespaces` §4(d) — XAML measured at zero | | T5 | covered | consumed — see note |
| `target-namespaces` §4 — simplification is forbidden | | T1, T2, T3, T4, T5, T6 | covered | consumed — see note |
| `target-namespaces` §5 — CRLF / tabs / `sed -i` / `mv` / MSB3026 | | all seven | covered | consumed — see note |
| `target-namespaces` §6 — behavioural floor, escalate don't proceed | | all seven | covered | consumed — see note |
| `namespace-collision` §2 — the 14 files at risk | | T2 (1), T6 (13) | covered | consumed — see note |
| `namespace-collision` §4 — rules 1–4 | | T1, T2, T6 | covered | consumed — see note |
| `migration-snapshot` §1/§3/§4/§5 — the 48, disposition, no-reformat, protocol | | T3 | covered | consumed — see note |
| `migration-snapshot` §5 — artifact `T3-ef-model-check.md` | | T6 and the reviewer | covered | consumed — see note |
| `migration-snapshot` §6 — the runtime half | | T6 | covered | consumed — see note |
| `doc-comment-crefs` §2 — the six | | T1 (5), T3 (1) | covered | consumed — see note |

## Uncovered rows — presented at Gate 2

**G1 — 78 stale folder-path citations across `docs/audit/`.** Re-derived at `af4c65f`: 78 occurrences on 77 lines — 75 lines in `01-domain-application.md`, 3 occurrences on 2 lines in `00-master-index.md` — plus one namespace-form reference at `04-app-controls-and-theme.md:261`. Of the 75, six lines are `CORE-004`'s own `locations:` block, arguably historical record. The other ~69 are live navigational citations that would lead a future session to a directory that no longer exists. The 17 `src/DungeonMaster.Tests/Domain/Entities/…` citations in `05-tests.md` are unaffected, since the test folders are not renamed.

**Resolved at Gate 2 (2026-09-16): folded into T7.** T7 widens from the single `CORE-004` correction to own `docs/audit/01-domain-application.md`, `docs/audit/00-master-index.md` and `docs/audit/04-app-controls-and-theme.md`, still disjoint from every other task. `CORE-004`'s own six-line `locations:` block stays singular as historical record, and `05-tests.md` stays out of T7's ownership. T7 remains forbidden from writing `MASTER-022`'s closure into the index it now owns — that cites commit hashes and stays orchestrator work.

**G2 — test-project mirror folders keep singular names.** `src/DungeonMaster.Tests/Domain/Entities/{Area,Dungeon,Floor}/`. Defensible — no type-name collision there and the namespaces already match their folders — but newly inconsistent with Domain. **Not scoped.**

**G3 — no task owns the full-solution build or the 908 total.** Every task's build and test evidence is scoped to its own project, deliberately, because nothing wider is reachable from its ownership. Confirming `dotnet build src/DungeonMaster.sln` clean and 908 passed / 0 failed across all three test projects is orchestrator work between T5/T6 and Gate 3, the shape 6b-iv established.

**G4 — `MASTER-022`'s closure in `00-master-index.md`.** Orchestrator work by rule: it cites commit hashes, and no task may run git.

## Note on the contract-operations table — why the call-site search does not apply

`dir`'s Gate 3 procedure for contract operations is a **search**: locate each declared operation's definition site in the session's touched files, discard hits in the defining file, in files owned by the same task, and in tests, and record `called` naming a surviving call site or `not called` otherwise. That procedure presumes contracts which declare **callable operations** — functions, endpoints, methods with names that can appear at a call site.

**This session's four contracts declare none.** They fix target strings (`target-namespaces.md` §1), state what must not move (§2), enumerate a file inventory (§3), define permitted edit shapes and prohibitions (§4), record mechanics (§5) and set a behavioural floor (§6); `namespace-collision.md` records a measured compiler behaviour and the rules following from it; `migration-snapshot.md` records a disposition and a protocol; `doc-comment-crefs.md` enumerates six sites. **None of these is an operation with a name that could appear at a call site**, so running the prescribed search would produce either zero hits everywhere — a meaningless `not called` on every row — or, worse, incidental text matches presented as call sites.

**`Implementing task` is therefore left empty by design, not by omission.** No artifact records an implementing task for a rule, and the search that would identify one is inapplicable.

**What `consumed` means in the End state column, and what evidences it:** each consuming task carried the relevant contract clause in its acceptance criteria, and its reviewer judged that criterion met against a command's output. So consumption is evidenced by the per-criterion verdicts in `reviews/T1.md` … `reviews/T7.md`, which is the same evidence §2 of the Gate 3 report rests on — not by a call-site search that cannot be run here.

**This is a real limitation of the check for a rename session, and is carried to Gate 3.** The check exists to catch an operation with an implementer and no consumer — a product that passes every criterion and still cannot do the thing. That failure mode does not arise when the contracts govern *how an edit is made* rather than *what capability is added*, and the substitute evidence above is weaker in kind: it confirms each rule was applied, not that anything calls anything.
