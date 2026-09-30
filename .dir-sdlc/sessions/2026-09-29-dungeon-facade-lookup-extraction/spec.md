# Collapse `Dungeon`'s repeated floor lookup, and close `MASTER-107`

Extract the ten-fold floor lookup and five-fold outcome re-check out of `DungeonMethods.cs` without
changing one byte of `Dungeon`'s public surface, then close `MASTER-107` / `CORE-015` against a
measurement rather than against the entry's own prose.

## Context

`MASTER-107` (`docs/audit/00-master-index.md:295`) / `CORE-015`
(`docs/audit/01-domain-application.md:751`) is the last open `S3` in `CORE`, the layer that has absorbed
most of this audit stream's work. Every figure below is from `measurement.md`, taken at `20b0b09`.

`DungeonMethods.cs` is **489 lines**, the largest file in the Domain by a factor of 2.8 over the next.
Of its **12** public methods, **8** are lookup-and-forward wrappers; **10** carry a byte-identical
`_floors.FirstOrDefault(floor => floor.Id == floorId)` plus null-guard, and **5** repeat an
`if (outcome is not MutationOutcome.Success) { return outcome; }` block the entry never mentions.

The entry's substance is right and three of its specifics are not. **"453 lines" was exact at the
authoring commit `16d594b`** and has drifted to 489. **"eight of its eleven methods" was wrong when
written** — there were 13 public methods at `16d594b` and are 12 now; "eight" is right. **"forward to
the identically named `Floor` method" is true for 4 of 8** — the other four rename. And its second
suggested direction, letting callers work against `Floor` directly, **cannot be done as written**:
every `Floor` mutator is `internal` and was `internal` at authorship, so that option is a
Domain-accessibility change rather than a refactor.

The entry's `risk: high` rests on the wrappers being "the aggregate's whole public surface." They have
exactly **8** external call sites, one Application command handler each. **Preserve the 12 signatures
and zero call sites change** — which is what makes this session small enough to finish.

This session also repairs the `AMENDED 6b-xiii` block in `01-domain-application.md`, because it is in
the same file and `measurement.md` §6 found that **one of the two backlog items against it is itself
false**.

## Definition of done

1. The repeated floor lookup appears **once** in `DungeonMethods.cs`, reached by all 10 sites that hold
   a copy of it today. **Checked with `grep -c "FirstOrDefault"`, which returns 10 at `20b0b09` and must
   return 1.** Not with the single-line literal `_floors.FirstOrDefault(floor => floor.Id == floorId)`,
   which this item first named and which **occurs zero times** — the expression is split across two
   tab-indented lines, so `grep -cF` on the single-line form returns 0 and a criterion counting it would
   be vacuous. Caught by `dir-architect`.
2. The `if (outcome is not MutationOutcome.Success) { return outcome; }` block appears **at most once**
   in `DungeonMethods.cs`.
3. All **12** public method signatures on `Dungeon` — name, parameter types, parameter names, order and
   return type — are unchanged from `20b0b09`.
4. The **8** Application command-handler files that call those methods are unmodified by this session.
5. `dotnet build src/DungeonMaster.sln` reports zero errors and zero warnings, checked with
   `grep -cE "(warning|error) [A-Z]+[0-9]+"`.
6. `dotnet test src/DungeonMaster.sln --nologo` reports **0 failed and 0 skipped**, with a total of
   **975 plus any test this session adds**.
7. Each of the **8** lookup sites returning `MutationOutcome.NotFound` for an unknown `floorId` has a
   test asserting that, and each is shown discriminating by **injected defect** rather than by a red
   step.
8. Each of the **2** lookup sites returning `null` for an unknown `floorId` (`AddArea`,
   `AddPlacedObject`) has a test asserting that, shown discriminating the same way.
9. `MASTER-107`'s Status cell reads `**done** (6b-xiv)` and states the corrected line count, the
   corrected method count, and that 4 of 8 forwarders rename. It carries **no commit hash** — the task
   cannot invoke git, per the convention at `00-master-index.md:623`.
10. `CORE-015` carries an `AMENDED 6b-xiv` block recording: `453` was exact at `16d594b`; "eleven" was
    wrong when written; the lookup duplication is 10-fold not 8; the outcome re-check is a second
    duplication; and the second suggested direction was never viable because `Floor`'s mutators are
    `internal`.
11. The false claim at `01-domain-application.md:993` — *"no `Update` overload of either arity was
    within ten lines of `:62-63`"* — is corrected, stating that the two-parameter `Update` sat at `:70`
    at `b39a65d`, **7** lines below `:63`.
12. The `:1000` "48 lines downward" sentence is recorded as **sound**, and the backlog item asserting
    it false is recorded as **falsified**, naming the proof: `:323-333` at `16d594b` is the
    four-parameter `AddPlacedObject` declaration exactly, and that declaration moved `:323` → `:371`
    across `bb7405c` → `781f73b`.
13. The citation-drift property is recorded once: all 8 of `CORE-015`'s `locations:` ranges (`:761-768`)
    and **the two `CORE-019` citations 6b-xiii corrected** — not both of that entry's `locations:`, of
    which it has **6**, at `:933-938` — were **exact at `16d594b`** and have drifted since, so the audit
    document's citations were accurate when authored and decay with the code.
14. The `AMENDED 6b-xiii` block's **causal** claim is corrected. It says both stale citations were
    "caused by the **ObjectScale feature work**" (`:995-997`). There are **two** causes, neither of them
    that: `DungeonMethods.cs:323-333` moved `:323`→`:371` at **`781f73b`** (2026-08-18, one day after
    authorship, `+99/-33`), and `PlacedObject.cs:62-63` moved `:62`→`:70` at **`b4e2a74`** (2026-09-17).
    The ObjectScale commit `ec6c239` is **2026-05-21**, three months *before* the entry was authored, so
    it cannot have moved a line the entry cited afterwards — what it created was the overload pairs
    `CORE-019` is about, not the drift. `781f73b` never touched `PlacedObject.cs`, so no single commit
    moved both. 6b-xiii's arithmetic was right (93→101 is the +8 it reconstructed) and its attribution
    was not.

## Scope

`src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs`; new or amended tests under
`src/DungeonMaster.Tests/`; and `docs/audit/00-master-index.md` plus
`docs/audit/01-domain-application.md`.

### Not in scope

- **`Floor`'s own 5 instances of the same lookup-and-guard pattern** (`FloorMethods.cs:90`, `:128`,
  `:162`; `FloorObjectMethods.cs:56`, `:95`). `CORE-015`'s `locations:` name only `DungeonMethods.cs`.
  The Domain holds 15 instances; this session addresses 10.
- **Any change to `Dungeon`'s public surface**, including making a member `internal`, renaming a
  parameter, or reordering one. Item 3 forbids it.
- **Any change to `Floor`'s accessibility.** This is the entry's non-viable second direction.
- **The other two open `S3`s**, `MASTER-051` and `MASTER-081`.
- **The 8 lone `CR` characters in `.dir-sdlc/backlog.md`.** Outside approved scope for the ninth
  session running.
- **Repairing `MASTER-108`'s wrong line count** ("2,226 lines", measured at 2,594 by 6b-xi). It is a
  `part done` row and is not this session's entry.
- **`a116b2c`'s false commit body.** History is not rewritten here.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Which file does `MASTER-107` cover? | `DungeonMethods.cs` only | 1 | All 8 of `CORE-015`'s `locations:` cite it and nothing else |
| Was "453 lines" ever correct? | Yes, exactly, at `16d594b` | 1 | `git show 16d594b:.../Dungeon/DungeonMethods.cs \| wc -l` → `453` |
| Which of the two suggested directions? | The lookup extraction. The second is out of scope | 1 | Every `Floor` mutator is `internal` at `20b0b09` and at `16d594b`; the second option needs a Domain-accessibility change |
| Does the public surface have to change? | No | 1 | 8 external call sites, all Application handlers; preserving signatures leaves them untouched |
| How many lookup sites are there? | 10, not the entry's 8 | 1 | `if (floor is null)` → 10; controls: `Update` and `AddFloor` have none, 10 + 2 = 12 methods |
| Does a closure row carry a commit hash? | No — the orchestrator adds it after the merge | 1 | `00-master-index.md:623`: *"`T6` cannot invoke git; the orchestrator writes the closure with commit evidence at Gate 3"* |
| What does a closed Status cell read? | `**done** (<session>)` | 1 | `MASTER-047:235`, `MASTER-177:365`, `MASTER-179:367` |
| Suite baseline? | 975 — 850 / 23 / 102, all **passed** counts | 1 | `dotnet test src/DungeonMaster.sln --nologo` in the worktree at `20b0b09` |
| Register baseline? | 179 rows, 131 closed / 45 open / 3 part done, 0 `UNCLASSIFIED` | 1 | The handoff's §6 survey script, re-run at `20b0b09` |
| Line endings for tracked files? | CRLF, and a slip fails the build | 1 | `TreatWarningsAsErrors=true` (`Directory.Build.props:5`), `EnforceCodeStyleInBuild=true` (`:16`), `IDE0055` warning (`.editorconfig:64`) |
| Is the solution at the repository root? | No — `src/DungeonMaster.sln` | 1 | `find -maxdepth 2 -name "*.sln*"` returns that one path |

## Contracts

**One: `contracts/dungeon-public-surface.md`.**

This section first read **"None"**, on the reasoning that `Dungeon`'s 12 signatures are required by item
3 to be *unchanged* and so constitute no new boundary. `dir-architect` authored one anyway and escalated
it (**E3**), and the argument for it is decisive rather than stylistic: **items 3, 4 and the disposition
table are all "unchanged from `20b0b09`", and a task cannot invoke `git`.** Without a handed-over
baseline, the three criteria resting on them are uncheckable by the very task they judge — the same
failure the handoff records as "a criterion needing a figure a task cannot measure must have that figure
handed over at dispatch."

The contract **is** that baseline, and it holds declarations only, per `dir-contracts`: the 12
signatures verbatim (§1), the ten floor-absent dispositions (§2), the outcome re-check (§3), the eight
forwarders and the four that rename (§4), **SHA-256 digests of the eight external call-site files**
(§5), the tested/untested split of the ten paths (§6), and the suite baseline (§7).

**All eight digests were re-verified independently by the orchestrator** with `sha256sum` in the
worktree; all eight match to the character.

## Open questions

None. Every unknown resolved at rung 1 above.

## A note for whoever authors the acceptance criteria

Items 7 and 8 assert behaviour that **already holds at `startRef`** — that is deliberate, and it is the
preserved-behaviour carve-out rather than vacuity. A consequence follows for the evidence: those tests
**cannot be shown red**, because they pass before the refactor by design. Their demonstration route is
**injected defect** — remove a null guard, run the filter, observe the failure — and a criterion
demanding a red step for them would be unsatisfiable.

Item 6's total is stated as "975 plus any test this session adds" on purpose. The exact post-change
figure is not knowable until the test task lands, so **any criterion resting on the final number needs
that number handed over at dispatch** — a task cannot measure a peer's contribution, and a criterion
naming a fixed total would be `UNDETERMINED` by construction.
