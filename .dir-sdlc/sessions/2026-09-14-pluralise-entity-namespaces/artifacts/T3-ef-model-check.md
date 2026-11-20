# T3 — EF model check record

Session `2026-09-14-pluralise-entity-namespaces` (sub-session 6b-v), task **T3** (`database`,
owns `src/DungeonMaster.Infrastructure/**`). Worktree `C:/dm-wt/6b-v`. Written 2026-09-16.

This file exists because task reports live only in the orchestrator's context and are unreadable by
a reviewer and by T6. T6 reads this exact absolute path.

---

## 1. The exact command line

Run from the worktree root `C:/dm-wt/6b-v`:

```
dotnet ef migrations has-pending-model-changes --project src/DungeonMaster.Infrastructure --startup-project src/DungeonMaster.Infrastructure
```

The design-time factory at `src/DungeonMaster.Infrastructure/Persistence/AppDbContextFactory.cs`
makes this invocation sufficient; no other startup project is needed.

---

## 2. BEFORE — the control

Verbatim, including the tooling-version warning:

```
Build started...
Build succeeded.
The Entity Framework tools version '10.0.3' is older than that of the runtime '10.0.11'. Update the tools for the latest features and bug fixes. See https://aka.ms/AAc1fbw for more information.
No changes have been made to the model since the last migration.
EXITCODE=0
```

The tooling-version line is the expected noise recorded at `.dir-sdlc/backlog.md:46`. It is reported
as **observed**, not as a result.

### State of the worktree when BEFORE ran

The control ran **before any of the four migration files had been edited** and after everything else
listed here. Specifically, all of the following had already changed relative to `af4c65f`:

- **T1 (Domain)** had landed: the three entity folders renamed to `Areas/`, `Dungeons/`, `Floors/`
  and their namespace declarations pluralised. Committed upstream.
- **T2 (Application)** had landed: its 8 `using` lines pluralised. Committed upstream (`f8d599f`).
- **T3's own non-migration edits had already been made**, and this was necessary rather than
  incidental: `src/DungeonMaster.Infrastructure/` held 6 stale singular `using` lines, so the
  project did **not** compile and `dotnet ef` could not run at all until they were fixed. The
  6 `using` lines and the 1 `cref` were therefore pluralised first, and
  `dotnet build src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj` was run
  (`0 Warning(s), 0 Error(s)`), which also performed the restore the architect could not.
- **The four files in `Migrations/` were still entirely singular** at this moment: 48 singular
  occurrences, 0 plural, verified by the §6 command immediately beforehand.

The architect could not produce this control: the worktree had no `obj/project.assets.json` and the
command failed `NETSDK1004` before reaching EF. T3's own build supplied the restore.

---

## 3. The injection — demonstrated discrimination

Required before any result from this check is trusted.

### 3a. Single-site injection (the criterion's literal form)

In `Migrations/AppDbContextModelSnapshot.cs`, one `modelBuilder.Entity("...")` CLR type name was
changed to a name that does not exist — the entity definition block for `Dungeon`:

```
modelBuilder.Entity("DungeonMaster.Domain.Entities.Dungeon.Dungeon", b =>
  ->
modelBuilder.Entity("DungeonMaster.Domain.Entities.Dungeon.DungeonXYZZY", b =>
```

Output with the injection in place:

```
Build started...
Build succeeded.
The Entity Framework tools version '10.0.3' is older than that of the runtime '10.0.11'. Update the tools for the latest features and bug fixes. See https://aka.ms/AAc1fbw for more information.
Changes have been made to the model since the last migration. Add a new migration.
EXITCODE=1
```

**Detected.** Exit code 1.

### 3b. Reverted

The injection was reverted and the same command re-run:

```
Build started...
Build succeeded.
The Entity Framework tools version '10.0.3' is older than that of the runtime '10.0.11'. Update the tools for the latest features and bug fixes. See https://aka.ms/AAc1fbw for more information.
No changes have been made to the model since the last migration.
EXITCODE=0
```

`grep -c XYZZY` on the file returned `0`, and the file's byte length returned to its baseline
`7825` bytes with `220` CRLF, confirming the revert was exact.

The result differs between injected and reverted, so the run is not inert.

### 3c. Supplementary probe — and it is the load-bearing one

The single-site injection changes **one** of the three sites naming that type in the snapshot. The
other two (`b.HasOne("…Dungeon.Dungeon", null)` and the `Navigation("Floors")` block) then
implicitly create a *second*, bare entity type. So the single-site injection introduces a
**structural split**, and it is not established that the name alone caused the detection.

The real edit is a **consistent** rename of every site. That case was therefore probed directly:
all 3 occurrences of `DungeonMaster.Domain.Entities.Dungeon.Dungeon` in the snapshot were changed
to `…Dungeon.DungeonXYZZY` together (`grep -c XYZZY` returned `3`):

```
Build started...
Build succeeded.
The Entity Framework tools version '10.0.3' is older than that of the runtime '10.0.11'. Update the tools for the latest features and bug fixes. See https://aka.ms/AAc1fbw for more information.
No changes have been made to the model since the last migration.
EXITCODE=0
```

**NOT detected.** A consistent CLR type name change across all its sites is invisible to this check.
The probe was then reverted; `grep -c XYZZY` returned `0` and the file returned to `7825` bytes,
`220` CRLF, `0` tabs — byte-identical to baseline.

### 3d. What this means for the AFTER run

The BEFORE control says the same thing independently: at BEFORE the snapshot was consistently
**singular** while the live model was consistently **plural**, and the check reported *no changes*.

So `has-pending-model-changes` pairs entity types by their mapped table and is **blind to a
consistent namespace rename inside the snapshot**. The AFTER result below is a genuine, correctly
obtained *null* result: it confirms **no DDL drift was introduced** by the edit, which is real and
worth having. It does **not**, and cannot, confirm that the 48 renamed strings are correct.

**The 48 strings are verified by the occurrence counts in §6, not by this command.** The runtime
half — a real model built over SQLite — is T6's, per `migration-snapshot.md` §6.

This was raised to the orchestrator as an evidence-quality escalation. It is **not** a
`c7`/pending-changes escalation: the AFTER run is clean.

---

## 4. AFTER — the measurement

The final state T3 hands over, with all four migration files edited:

```
Build started...
Build succeeded.
The Entity Framework tools version '10.0.3' is older than that of the runtime '10.0.11'. Update the tools for the latest features and bug fixes. See https://aka.ms/AAc1fbw for more information.
No changes have been made to the model since the last migration.
EXITCODE=0
```

**No pending model changes.** The `c7` stop-and-escalate condition did not fire. No migration was
scaffolded, no snapshot was hand-edited to quiet the check, and no criterion was weakened.

### The two, side by side

| Run | Output | Exit |
|---|---|---|
| BEFORE (migrations singular) | `No changes have been made to the model since the last migration.` | 0 |
| Single-site injection | `Changes have been made to the model since the last migration. Add a new migration.` | 1 |
| Consistent-rename probe | `No changes have been made to the model since the last migration.` | 0 |
| AFTER (migrations plural) | `No changes have been made to the model since the last migration.` | 0 |

---

## 5. Build

```
dotnet build src/DungeonMaster.Infrastructure/DungeonMaster.Infrastructure.csproj
```

Final state:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

The build **cannot** verify the 48 strings: they are string literals and compile identically whether
right or wrong. That is the entire reason §3 and §4 exist.

---

## 6. Per-file occurrence counts after the edit

Command — the pattern from `migration-snapshot.md` §7, counted per file over each `.cs` file in
`src/DungeonMaster.Infrastructure/Migrations/`:

- singular: `DungeonMaster\.Domain\.Entities\.(Dungeon|Floor|Area)\b`
- plural:   `DungeonMaster\.Domain\.Entities\.(Dungeons|Floors|Areas)\b`

Script preserved at
`…/scratchpad/dir/2026-09-14-pluralise-entity-namespaces/T3/t3scan.py` (adapted from the
architect's `hazards.py`), run from `C:/dm-wt/6b-v`.

### Before the edit

| File | singular | plural |
|---|---|---|
| `20260805014736_InitialCreate.Designer.cs` | 12 | 0 |
| `20260805014736_InitialCreate.cs` | 0 | 0 |
| `20260805184106_AddFloorDescription.Designer.cs` | 12 | 0 |
| `20260805184106_AddFloorDescription.cs` | 0 | 0 |
| `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.Designer.cs` | 12 | 0 |
| `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs` | 0 | 0 |
| `AppDbContextModelSnapshot.cs` | 12 | 0 |
| **TOTAL** | **48** | **0** |

### After the edit

| File | singular | plural |
|---|---|---|
| `20260805014736_InitialCreate.Designer.cs` | 0 | 12 |
| `20260805014736_InitialCreate.cs` | 0 | 0 |
| `20260805184106_AddFloorDescription.Designer.cs` | 0 | 12 |
| `20260805184106_AddFloorDescription.cs` | 0 | 0 |
| `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.Designer.cs` | 0 | 12 |
| `20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs` | 0 | 0 |
| `AppDbContextModelSnapshot.cs` | 0 | 12 |
| **TOTAL** | **0** | **48** |

All four files changed. The three **non-Designer** files — which hold every line of applied DDL in
`Up()`/`Down()` — were 0 before and 0 after, and are byte-for-byte unchanged (see §7).

### MigrationIds, unchanged

```
20260805014736_InitialCreate.Designer.cs: [Migration("20260805014736_InitialCreate")]
20260805184106_AddFloorDescription.Designer.cs: [Migration("20260805184106_AddFloorDescription")]
20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.Designer.cs: [Migration("20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault")]
```

### `ToTable` names, unchanged

```
Persistence/Configurations/AreaConfiguration.cs:48: builder.ToTable("Areas");
Persistence/Configurations/DungeonConfiguration.cs:21: builder.ToTable("Dungeons");
Persistence/Configurations/FloorConfiguration.cs:21: builder.ToTable("Floors");
Persistence/Configurations/PlacedObjectConfiguration.cs:16: builder.ToTable("PlacedObjects");
```

---

## 7. The files were not reformatted

The edit was a substring replacement inside string literals and nothing else, made with the edit
tools. `sed -i` was never used on any tracked file.

Binary counts, `b.count(b'\r\n')` versus `b.count(b'\n')`, before and after:

| File | bytes before | bytes after | delta | CRLF | LF | tabs |
|---|---|---|---|---|---|---|
| `20260805014736_InitialCreate.Designer.cs` | 7919 | 7931 | +12 | 221 → 221 | 221 | 0 |
| `20260805184106_AddFloorDescription.Designer.cs` | 8082 | 8094 | +12 | 225 → 225 | 225 | 0 |
| `20260818232300_…WithoutStoreDefault.Designer.cs` | 7999 | 8011 | +12 | 223 → 223 | 223 | 0 |
| `AppDbContextModelSnapshot.cs` | 7825 | 7837 | +12 | 220 → 220 | 220 | 0 |
| `20260805014736_InitialCreate.cs` | 6180 | 6180 | 0 | 134 → 134 | 134 | 0 |
| `20260805184106_AddFloorDescription.cs` | 809 | 809 | 0 | 29 → 29 | 29 | 0 |
| `20260818232300_…WithoutStoreDefault.cs` | 5634 | 5634 | 0 | 122 → 122 | 122 | 0 |

Each edited file grew by **exactly 12 bytes** — one byte per replacement, the appended `s`, for 12
replacements. CRLF counts are unchanged, `CRLF == LF` throughout (no bare LF), and the tab count is
**0** in all seven files: they remain space-indented generated EF output, as
`.editorconfig:49-50` (`generated_code = true`) intends. The tabs rule in `target-namespaces.md` §5
does not apply to this directory.

---

## 8. The gate's decision

**The gate chose Disposition U on 2026-09-16**, recorded in
`contracts/migration-snapshot.md` §3: *update all four files; all 48 strings are pluralised*, in
`AppDbContextModelSnapshot.cs` and in all three `*.Designer.cs` files.

**Disposition U is what was implemented.** All four files were edited; the result is 0 singular and
48 plural.

**Disposition S — snapshot only, leaving the three Designer files singular — was considered and
rejected at that same gate, and was not implemented, in whole or in part, and was not used as a
fallback.**

No `Up()` or `Down()` method was edited, no DDL changed, no table/column/key/index/constraint name
changed, and no migration was added, renumbered, reordered or deleted.
