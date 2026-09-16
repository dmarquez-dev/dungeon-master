# Contract — the EF Core migration snapshots

Frozen at Gate 2. Session `2026-09-14-pluralise-entity-namespaces` (sub-session 6b-v).

Authored by `dir-architect` at decomposition. **Consumed by T3 (Infrastructure)**, which owns all
four files, and by T6 (Tests), which owns the only tests that exercise the model at runtime.

This is the one part of the session the compiler cannot check. The 48 occurrences are **string
literals**. They compile identically whether they are right or wrong, and a stale snapshot is
invisible to `dotnet build` and invisible to `TreatWarningsAsErrors`.

---

## 1. The 48 occurrences — measured, per file

`src/DungeonMaster.Infrastructure/Migrations/`, matching
`DungeonMaster\.Domain\.Entities\.(Dungeon|Floor|Area)\b`:

| File | Occurrences |
|---|---|
| `20260805014736_InitialCreate.Designer.cs` | 12 |
| `20260805184106_AddFloorDescription.Designer.cs` | 12 |
| `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.Designer.cs` | 12 |
| `AppDbContextModelSnapshot.cs` | 12 |
| **TOTAL** | **48** |

They take two forms, both inside `BuildTargetModel` / `BuildModel`:

```csharp
modelBuilder.Entity("DungeonMaster.Domain.Entities.Floor.Floor", b =>
b.HasOne("DungeonMaster.Domain.Entities.Dungeon.Dungeon", null)
```

---

## 2. The DDL is not in these files — measured

The three sibling **non-Designer** migration files, which hold the `Up()` and `Down()` methods and
therefore all the applied SQL, contain **zero** occurrences of the pattern:

| File | Occurrences |
|---|---|
| `20260805014736_InitialCreate.cs` | 0 |
| `20260805184106_AddFloorDescription.cs` | 0 |
| `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs` | 0 |

Each `.Designer.cs` contains exactly `[Migration("…")]` and `protected override void
BuildTargetModel(ModelBuilder modelBuilder)` — verified by searching all three for `protected
override void Up`, `protected override void Down`, `BuildTargetModel` and `[Migration(`.

**Consequence, and it is the load-bearing one: editing these four files changes CLR type identity
inside a model snapshot and changes no DDL whatsoever.** No table, column, key, index or constraint
name appears in the diff. All four entity types are mapped with an explicit `ToTable(...)` —
`Areas`, `Dungeons`, `Floors`, `PlacedObjects` at `AreaConfiguration.cs:48`,
`DungeonConfiguration.cs:21`, `FloorConfiguration.cs:21`, `PlacedObjectConfiguration.cs:16` — and
none of those four strings is touched by this session.

`__EFMigrationsHistory` records a `MigrationId` and a `ProductVersion`. It stores no content hash, so
no already-applied migration's recorded identity changes.

---

## 3. The database discipline, and how this sits against it

`disciplines/database.md` is binding and states:

> Migrations are forward-only. A released migration is never edited; a mistake is corrected by a new
> migration.

**T3 must not edit any `Up()` or `Down()` method, must not alter any DDL, and must not add,
renumber, reorder or delete a migration.** The three `MigrationId` values
(`20260805014736_InitialCreate`, `20260805184106_AddFloorDescription`,
`20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault`) are frozen exactly as they stand,
in both the file names and the `[Migration("…")]` attributes.

### DISPOSITION U — decided by the user at Gate 2 on 2026-09-16

Whether updating the three `.Designer.cs` snapshots nevertheless counts as "editing a released
migration" was raised as an escalation and **is now settled. The user chose Disposition U.**

> **Disposition U — update all four files. All 48 strings are pluralised**, in
> `AppDbContextModelSnapshot.cs` and in all three `*.Designer.cs` files.

**T3 implements U and does not re-open it.** After the edit, `src/DungeonMaster.Infrastructure/Migrations/`
contains **0** singular occurrences and **48** plural ones.

**The evidence the decision rests on**, all of it about what the files contain rather than about any
observed EF run:

- The `Up()`/`Down()` methods — every line of applied SQL — live in the three **non-Designer**
  migration files, which contain **0** occurrences of the pattern (§2). Each `.Designer.cs` holds only
  `[Migration("…")]` and `BuildTargetModel`.
- No table, column, key, index or constraint name appears in the diff. All four `ToTable(...)` names
  — `Areas`, `Dungeons`, `Floors`, `PlacedObjects` — are untouched.
- `__EFMigrationsHistory` stores a `MigrationId` and a `ProductVersion`, not a content hash, so no
  applied migration's recorded identity changes.

So the edit changes CLR type identity inside model metadata and changes **no DDL and no migration
identity**, which is what the discipline's forward-only rule exists to protect.

### Disposition S — considered and rejected at the same gate

Recorded because a future reader needs to see the decision was made with the alternative in view, and
**not** as a fallback T3 may reach for.

> **Disposition S — snapshot only.** Only the 12 occurrences in `AppDbContextModelSnapshot.cs` would
> be pluralised; the 36 in the three `.Designer.cs` files would be left naming the old CLR
> namespaces, with a short comment block added at the top of each — inside the existing
> generated-code region — recording that the names are historical and why they were not updated.

Its case was the strict reading: those three files are literally part of released migrations, and the
rule says a released migration is never edited. Its cost, and the reason it was rejected: three
historical snapshots would be left naming CLR types that no longer exist anywhere in the solution,
which is misleading to exactly the reader the rule is meant to protect, while protecting no DDL that
was ever at risk.

**T3 must not implement S, in whole or in part, and must not fall back to it.** If T3 believes U
cannot be carried out, it escalates rather than substituting S.

**Deciding the disposition does not make the EF outcome known.** U was chosen on evidence about file
*contents*, not on an observed `has-pending-model-changes` run. §5's protocol is unchanged and
binding in full: the control runs first, both outputs are reported verbatim, and T3 is forbidden from
predicting or assuming the AFTER result.

---

## 4. Formatting — do not reformat these files

- `.editorconfig:49-50` marks `src/DungeonMaster.Infrastructure/Migrations/*.cs` as
  `generated_code = true`, so `IDE0055` and the style analyzers do not fire there.
- **These four files are space-indented.** That is correct for generated EF output and must stay. The
  tabs rule in `contracts/target-namespaces.md` §5 **does not apply to this directory.** A task that
  re-indents them to tabs has produced a several-hundred-line diff that hides the change being
  reviewed, and it must not.
- The edit is a **substring replacement inside string literals** and nothing else. Every other byte
  of all four files is unchanged. The files are tracked and CRLF: use the edit tools, never
  `sed -i`.

---

## 5. The instrument, and the protocol T3 must run

The check is **`dotnet ef migrations has-pending-model-changes`**, invoked against
`src/DungeonMaster.Infrastructure` as both project and startup project. A design-time factory exists
at `src/DungeonMaster.Infrastructure/Persistence/AppDbContextFactory.cs`, so no other startup project
is needed.

**The instrument is established in this repository.** `.dir-sdlc/backlog.md:40` records that it was
run successfully here by both an implementer and a reviewer in session 3 — *"The check itself passes
today: it was run by both the implementer and the reviewer against the session's migration."* The
same line records there is **no `.github/workflows` directory**, so this is a manual check and not
CI, and **no task may create a CI pipeline to host it**.

`.dir-sdlc/backlog.md:46` records the expected noise: every invocation prints *"The Entity Framework
tools version '10.0.3' is older than that of the runtime '10.0.11'."* **That is a known tooling-
version warning, not a failure**, and T3 must report it as observed rather than treat it as a result.

### The protocol — a control, then the measurement

T3 runs the command **twice** and reports both, verbatim:

1. **BEFORE** any edit to the four files, with the rest of the rename already landed upstream (T1,
   T2) and Infrastructure's own `using`/`cref` edits not yet made, or made — either is acceptable so
   long as it is stated. This is the **control**: it establishes the instrument runs to completion in
   this worktree and reports what it reports on a model that has not yet moved.
2. **AFTER** the edit, in the final state T3 hands over.

The architect could not run the control: the session worktree `C:/dm-wt/6b-v` has no
`obj/project.assets.json`, so the command fails with `NETSDK1004` before reaching EF, and restoring
would have meant writing into a worktree the architect is forbidden to write to. **T3 runs the
restore as part of its own build and is therefore the first agent that can run this.** The control is
T3's to produce, not something it inherits.

### The outcome is not predicted, and T3 must not assume it

Whether the differ treats a changed CLR type name as a model change when `ToTable(...)` is unchanged
is **exactly the kind of question that must be run rather than reasoned about**, and this contract
deliberately does not assert the answer.

- If the AFTER run reports **no pending changes**: that is the expected and desired outcome. T3
  records both outputs and is done.
- If the AFTER run reports **pending model changes**: **T3 stops and escalates.** It must **not**
  scaffold a new migration, must **not** hand-edit the snapshot further to make the check quiet, and
  must **not** weaken the criterion. A new migration generated at this point would carry no DDL and
  would exist only to re-record a namespace, which is a decision for the gate. T3 reports the exact
  output, the two outputs side by side, and stops.

### The artifact — how this reaches a reviewer

Task reports live only in the orchestrator's context and are unreadable by a reviewer and by
downstream tasks. **T3 writes both outputs into a session artifact file**, at the absolute path

```
C:/Users/dmarq/Development/Repos/dungeon-master/.dir-sdlc/sessions/2026-09-14-pluralise-entity-namespaces/artifacts/T3-ef-model-check.md
```

which is in the **main checkout**, deliberately outside the session worktree, so it is never swept
into a task commit. The file records, in this order: the exact command line; the BEFORE output
verbatim, including the tooling-version warning; the state of the worktree when BEFORE was run; the
AFTER output verbatim; the per-file occurrence counts after the edit, with the command that produced
them; and a statement that the gate chose Disposition U on 2026-09-16 and that U is what was implemented.

**That path is deliberately NOT in T3's `owns` set, and that is not an oversight.** `owns` declares
what a task may edit **inside the worktree** `C:/dm-wt/6b-v`, because that is where both mechanisms
consuming it run: `dir-git` stages exactly the task's `owns` globs, and the reviewer's diff is
`git diff <startRef> -- <owns paths>`. This artifact is placed outside the worktree precisely so it
can never be swept into a task commit, so it must never appear in either. A reviewer opens it by the
**absolute path above**, which T3's criterion c8 states and T6's criterion names in full — never by a
worktree-relative path, which resolves to a directory that does not exist.

---

## 6. What T3 cannot prove, and who proves it

**T3 cannot run a single test.** `DungeonMaster.Tests` is T6's, it does not compile until T6 lands
its `using` edits, and it is the only project holding tests that exercise the EF model against a real
provider. T3's evidence is therefore a green `dotnet build` of
`src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj` plus the two EF outputs, and
nothing more. That is the whole of what is reachable from the files T3 owns, and T3's criteria ask
for nothing beyond it.

**The runtime half is T6's.** The persistence tests under
`src/DungeonMaster.Tests/Infrastructure/Persistence/` build a real model over SQLite and are what
would actually fail if the rename had broken entity-to-table mapping. T6's criteria require that
selection to pass, and T6 depends on T3.

Neither task owns the full-solution suite. Confirming **908 passed, 0 failed** across all three test
projects after every task has landed is **orchestrator work between the last task and Gate 3**, as it
was in 6b-iv. No task's criteria claim it.

---

## 7. Commands

- **48 occurrences, per file:** `DungeonMaster\.Domain\.Entities\.(Dungeon|Floor|Area)\b` over each
  `.cs` file in `src/DungeonMaster.Infrastructure/Migrations/`. Script preserved at
  `…/scratchpad/dir/2026-09-14-pluralise-entity-namespaces/architect/hazards.py`.
- **0 occurrences in the three non-Designer files:** the same pattern, counted per file.
- **`ToTable` names:** `grep -rn "ToTable\|HasColumnName" src/DungeonMaster.Infrastructure/Persistence/Configurations/`.
- **The EF check:** `dotnet ef migrations has-pending-model-changes --project src/DungeonMaster.Infrastructure --startup-project src/DungeonMaster.Infrastructure`, run from the worktree root.
