<!-- frozen: 20261105-1907-core-heavy-rows, Gate 2 -->

# C4 -- Register and audit updates: MASTER-157 / CORE-031, MASTER-178 / CORE-035, MASTER-130 / CORE-024

No callable operation. Three tasks each edit **one** status cell in `docs/audit/00-master-index.md` and insert
**one** amendment block in `docs/audit/01-domain-application.md`. They share both files, so the manifest orders
them **T2, then T3, then T4** through `dependsOn`, and T4 also waits for T1. No two of them run at once. The
backlog rows (`.sir/backlog.md:11`, `:12`, `:13`, and the CW90 clause of `:25`) are trimmed by the orchestrator at
Gate 3; no task touches `.sir/` or `.dir-sdlc/`.

## 0. Rules for every cell and block

- **Cite what you measured; label what you were told** (lesson 2026-11-03, `.sir/lessons.md:56`). "Measured"
  means the writing task ran the command on the worktree during its own run and the text gives the command or
  its C1/C3 section beside the figure. "Told" means the value comes from a contract, a review record or the
  orchestrator, and the text says so.
- **Status token** (register-classifier lesson, `.sir/lessons.md:43`): with `*` stripped, the cell's leading
  token is `DELIBERATELY UNCHANGED` (rows 345, 366) or `part done` (row 318).
- **Escaping:** every literal `|` inside a cell is written `\|`. R (C1 section 5) prints 11 on the edited line.
  Cite instruments by their contract section rather than quoting one that contains `|`.
- **Previous wording:** rows 318 and 366 end with the sentence `**Previous status wording, retained unchanged
  below.**` followed by the whole startRef cell, verbatim; V (C1 section 6) proves it. Row 345 is empty at startRef.
- **Nothing else in the master index changes:** U (C1 section 7) prints the startRef digest and the file keeps 691
  lines.
- **Blocks:** house form, as at `01-domain-application.md:1577` and `:2932`. First line begins
  `**[AMENDED 20261105-1907-core-heavy-rows, 2026-11-05 -- `; last line ends `**]**`; the block contains
  **Nothing below deletes any previous wording.**; one empty line before and after it; inserted lines only, nothing
  in the entry deleted or edited. Each block states that the record's severity, category, effort and risk are
  unchanged.
- **No hash of this session's own commit** appears in any cell or block, and each cell says why: tasks run without
  git and the orchestrator makes the commit (precedent `MASTER-051` at `00-master-index.md:239`). A historical hash
  copied from an orchestrator-supplied figure (such as section 1.3's `16d594b` and `62ba566`) may appear, labelled
  told. (Amended A1, by E1.)
- A cell or block may cite `.sir/sessions/20261105-1907-core-heavy-rows/contracts/<file>` by path.
- B (C1 section 1) after the last edit: `00-master-index.md` CR=LF=CRLF=691, loneCR=0; `01-domain-application.md`
  CR=LF=CRLF=(the file's line count before the task + the lines it inserted), loneCR=0.

## 1. T2 -- MASTER-157 / CORE-031: the dispatcher, deliberately unchanged

### 1.1 Status cell, `00-master-index.md:345`

At startRef the line is `| 157 | MASTER-157 | S5 | CORE | structure | L | high | The hand-rolled dispatcher is 233
lines replacing a mediator library and earns its keep | CORE-031 | |` (one line in the file). Only the status cell
changes. It begins:

```
**DELIBERATELY UNCHANGED** (20261105-1907-core-heavy-rows, 2026-11-05) -- 
```

modelled on `MASTER-145` at `:333`, and states:

1. **The decision:** closed with no code change by the user's Gate 1 decision to keep the hand-rolled dispatcher,
   which is CORE-031's own `Suggested direction.` ("Keep it").
2. **Re-measured size**, measured by T2: `wc -l src/DungeonMaster.Application/Common/Dispatch/*.cs` lists 8 files and
   437 lines in total, `Dispatcher.cs` 209 (startRef values; T2 re-runs it and quotes its own output). The title's
   "233 lines" and CORE-031's "seven files and 233 lines" do not describe the folder today; the eighth file is
   `IRequest.cs` (9 lines). Classification of the 233 figure: per section 1.3.
3. **The three maintenance costs, verified in code by T2 rather than taken from their register cells.** Each
   register cell (`MASTER-080` at `:268`, `MASTER-089` at `:277`, `MASTER-090` at `:278`) reads `**done**` at
   startRef; the cell labels that as what the register says, then gives T2's own measurement (section 1.2).
4. **What remains, stated plainly:** the dispatcher still resolves handlers and behaviours from `IServiceProvider`
   at dispatch time (section 1.2, item D). That is the service-location shape `MASTER-089`'s title names; its
   *hand-maintained registration list* half is gone, its service-location half is inherent in the design Gate 1
   kept.
5. A pointer to the `AMENDED 20261105-1907-core-heavy-rows` block on `CORE-031` in `01-domain-application.md`.
6. The no-commit-hash sentence.

### 1.2 Code evidence for the three costs (startRef values; T2 re-runs every command)

All paths under `src/DungeonMaster.Application/`.

- **A. MASTER-090 / CORE-011, uncached reflection and `dynamic`.**
  - DYN (C1 section 4) prints nothing; re-run its control first.
  - `grep -n 'ConcurrentDictionary<Type, object> Invokers' Common/Dispatch/Dispatcher.cs` prints `:18`.
  - `grep -n 'Invokers.GetOrAdd' Common/Dispatch/Dispatcher.cs` prints `:31`, `:48`, `:67` (two `SendAsync`
    overloads and `QueryAsync`), and `grep -n 'MakeGenericType' Common/Dispatch/Dispatcher.cs` prints `:35`, `:52`,
    `:71`, each inside the `static ... =>` value factory of the `GetOrAdd` above it: the closed invoker type is
    built once per request type and cached.
- **B. MASTER-089 / CORE-010, the hand-maintained registration list.**
  - `grep -n 'AddScoped<' DependencyInjection.cs` prints only `:27` (`services.AddScoped<IDispatcher, Dispatcher>();`):
    no handler is registered by name.
  - `grep -n 'ScannedTypes' DependencyInjection.cs` prints `:29` (the registration loop), `:99` (the
    self-check loop) and `:137` (the property, an assembly scan of the Application assembly).
  - `ValidateHandlerRegistrations` (`DependencyInjection.cs:93`) asserts exactly one marker and exactly one
    handler per request type. `grep -rn 'ValidateHandlerRegistrations' src/DungeonMaster.App --include=*.cs
    --exclude-dir=obj --exclude-dir=bin` (run from the worktree root) prints nothing: it is exercised by
    `ApplicationServiceRegistrationTests.cs` (`:29`, `:46`), not called at app startup. Say so; do not claim a
    startup check.
- **C. MASTER-080 / CORE-012, queries bypassing the pipeline.**
  - `Common/Dispatch/IQuery.cs:9-10` declares `public interface IQuery<TResult>` `: IRequest<TResult>`.
  - `grep -n 'BuildPipeline<' Common/Dispatch/Dispatcher.cs` prints `:84` (the definition) and `:133`, `:166`, `:199`;
    `:199` is inside `TypedQueryInvoker`, so a query passes through the same `IPipelineBehavior` composition as a
    command.
  - No behaviour is registered for queries today, by design: `ValidationBehavior` is registered only in the two
    command branches (`DependencyInjection.cs:50-54`, `:64-70`), and the comment at `:48-49` says so ("so that no
    query can ever acquire a validation behavior"). The pipeline is reachable by queries; it is empty for them.
- **D. The residue.** `grep -n 'GetRequiredService\|GetServices' Common/Dispatch/Dispatcher.cs` prints `:92`,
  `:131`, `:164`, `:197`: handlers and behaviours are resolved from the container at dispatch time.

### 1.3 The audit-commit figure for "233 lines" -- supplied by the orchestrator

Tasks have no git. Before Gate 2 the orchestrator measures, at the commit that wrote CORE-031's "seven files and
233 lines" (found with `git log -S` over `docs/audit/01-domain-application.md`), `git show <sha>:<path> | wc -l` for
each file then in `src/DungeonMaster.Application/Common/Dispatch/` (path as it was then), and records here: the
sha, the per-file counts, the total, and the classification -- **DRIFT** if the total was 233 then (true at its
commit, moved since), **ERROR** if it was not.

> ORCHESTRATOR-SUPPLIED, measured in the spec phase on 2026-11-05. sha `16d594b` ("docs: add code-quality audit
> for domain and application", 2026-08-17), the only commit `git log -S 'seven files and 233 lines' --
> docs/audit/01-domain-application.md` prints. Files and counts, by `git show 16d594b:<path> | wc -l` over
> `git ls-tree --name-only 16d594b src/DungeonMaster.Application/Common/Dispatch/`: `Dispatcher.cs` 70,
> `ICommand.cs` 9, `ICommandHandler.cs` 32, `IDispatcher.cs` 47, `IPipelineBehavior.cs` 36, `IQuery.cs` 9,
> `IQueryHandler.cs` 30 -- seven files. Total `233`. Control: the same method at `HEAD` (`62ba566`) prints 8 files
> and 437 lines, matching `wc -l` on the worktree. Classification **DRIFT**: exact at its commit, moved since.

T2 copies this into the block **labelled told** (measured by the orchestrator in the spec phase).

### 1.4 Amendment block on CORE-031

Inserted after CORE-031's `**Suggested direction.**` paragraph (startRef `:2461-2463`) and before
`### CORE-032` (startRef `:2465`). It carries:

1. **Closure.** `MASTER-157` (`00-master-index.md:345`) is CLOSED **DELIBERATELY UNCHANGED** by user decision at
   Gate 1; no code changed.
2. **The stale size.** Quote verbatim from `What.` (startRef `:2447`): "The `Common/Dispatch` folder is seven files
   and 233 lines". State T2's measurement (section 1.1 item 2) and the told audit-commit figure and classification
   (section 1.3).
3. **The three costs** the `Why it matters.` paragraph names (CORE-010, CORE-011, CORE-012), each with the
   evidence of section 1.2 A-C (commands or this section's path), and the residue (section 1.2 D).
4. **`locations:` is kept unchanged** as a record of the audited state; the block does not re-derive those lines.
5. AO and AC (C1 section 8) each print **16** after the block, where they print 15 at startRef.

## 2. T3 -- MASTER-178 / CORE-035: every concrete-at-zero enum given a disposition

### 2.1 Status cell, `00-master-index.md:366`

Begins `**DELIBERATELY UNCHANGED** (20261105-1907-core-heavy-rows, 2026-11-05) -- ` and states:

1. **The census**, measured by T3 with C3's instrument (cited by path, `C3-enum-census.md` section 1): `TOTAL 17
   NONE_AT_ZERO 5 CONCRETE_AT_ZERO 12 NO_ZERO 0`; that T3 re-ran the fixture control (C3 section 2) and the second
   predicate (C3 section 3, 17 lines) first; and that 12 of 17 agrees with the post-change figure this row
   already records for 6b-ix.
2. **Every one of the twelve is deliberately unchanged**, each with its reason in the CORE-035 block (C3 section 4);
   no default of the twelve is both reachable and a defect, so no `None` member and no data migration was added.
3. The row's previous framing is **superseded**: `PathSegmentKind` is no longer "blocked" (no zero member is owed)
   and the `RotationDirection` / `ScaleDirection` class B design question is decided (their defaults are never
   produced).
4. A pointer to the `AMENDED 20261105-1907-core-heavy-rows` block on `CORE-035`.
5. The no-commit-hash sentence (no code changed for this row).
6. `**Previous status wording, retained unchanged below.**` and the startRef cell (V, C1 section 6: 1576, `7e26bc18...`).

### 2.2 Amendment block on CORE-035

Inserted after CORE-035's `**Suggested direction.**` paragraph (startRef `:2829-2836`) and before `### CORE-036`
(startRef `:2838`). It carries:

1. **Closure.** `MASTER-178` (`00-master-index.md:366`) is CLOSED **DELIBERATELY UNCHANGED** by this session.
2. **The validated census:** the instrument by path (C3 section 1, with its sha256), the fixture control and its
   output (C3 section 2), the worktree output totals (C3 section 3), and the second predicate's 17. The
   `zero=None` display ambiguity is stated (C3 section 1).
3. **A disposition table with one row per enum**, twelve rows: enum, zero member, disposition `deliberately
   unchanged`, and the reason with its `file:line` citations as C3 section 4 gives them, re-read by T3. Pipes
   inside this markdown table are ordinary table delimiters (this file is not the register).
4. **Superseded framings** (C3 section 4, last paragraph): the class B design question and the `PathSegmentKind`
   "blocked" note in this entry's body and its `Suggested direction.`.
5. **Drifted citations** (C3 section 4): `ResultOfT.cs:39` -> `:49`, `PathSegmentJsonRoundTripTests.cs:26-28` ->
   `:28-29`, `DungeonDetailPage.xaml.cs:140`/`:151`/`:167` -> `:139`/`:150`/`:166`, `ControlsPaletteView.xaml.cs:14-17`
   -> producers at `:27-30`, each re-measured by T3. The entry's `locations:` list is kept unchanged as a record.
6. AO and AC each print **17** after the block.

## 3. T4 -- MASTER-130 / CORE-024: the CW90 family cleared, and the PathSegmentKind correction

### 3.1 Status cell, `00-master-index.md:318`

Begins `part done (20261105-1907-core-heavy-rows, 2026-11-05) -- ` and states:

1. **The CW90 family is cleared:** `ObjectRotation`'s members are now `Clockwise90`, `Clockwise180`, `Clockwise270`
   (T4 measures their declaration lines with `grep -n` on `ObjectRotation.cs`); the migration
   `<ID>_RenameObjectRotationClockwiseMembers` (T4 reads the ID from `ls src/DungeonMaster.Infrastructure/Migrations/`)
   rewrites the stored `PlacedObjects.Rotation` strings `Up` and reverses them `Down` (mapping per C2 section 2);
   the proof is `RenameObjectRotationClockwiseMembersMigrationTests` (C2 section 3), whose pass is **told** from
   T1's review record. P-CW (C1 section 2) prints 0 lines, measured by T4 after re-running its control.
2. **EXCLUSION 1 is superseded:** its blocker -- persistence by member name -- is exactly what the migration handles.
3. **The row stays part done:** the `ct` residue (EXCLUSION 2) and the `obj` residue (EXCLUSION 3) are unchanged and
   out of this session's scope; their figures are carried as recorded, not re-measured.
4. **Correction -- PathSegmentKind.** Quote verbatim the two phrases of the startRef cell: "which is the same blocker
   class this register already parks `PathSegmentKind` on" and "A backlog item covers both renames with real
   migrations." Then the facts, measured by T4: `PathSegmentKind`'s members are `Line` and `CubicBezier`
   (`PathSegmentKind.cs`, `grep -n`), neither abbreviated, so no rename is owed; it is persisted by **ordinal**
   inside the `Areas.Outline` JSON column (`AreaConfiguration.cs:16`, `new(JsonSerializerDefaults.General)`; JC,
   C1 section 3, prints nothing), so renaming its members would rewrite nothing stored while renumbering them would;
   the blocker this register parks it on (`MASTER-178`) concerned adding a zero member, a renumbering, not a rename.
   "Same blocker class" is therefore imprecise and "both renames" is false.
5. **Citation drift:** the cell's `PlacedObjectConfiguration.cs:30-32` names lines that no longer hold the
   `Rotation` chain; at this session's startRef it occupies `:39-42` (`:39` `builder.Property(obj =>`, `:40`
   `obj.Rotation)`, `:41` `.IsRequired()`, `:42` `.HasConversion<string>();`), measured by T4 with `grep -n` and a read.
   No classification word is given (tasks have no git).
6. A pointer to the `AMENDED 20261105-1907-core-heavy-rows` block on `CORE-024`; build and test outcomes either
   omitted or attributed to T1's review record (told); the no-commit-hash sentence.
7. `**Previous status wording, retained unchanged below.**` and the startRef cell (V, C1 section 6: 7319, `cdf47942...`).

### 3.2 Amendment block on CORE-024

Inserted after the `AMENDED 6b-xvii` block that closes at startRef `:1768` and before `### CORE-025` (startRef
`:1770`). It carries:

1. **The CW90 family closed** as in section 3.1 item 1, and that the `Suggested direction.`'s rename is now done
   with the data migration the 6b-xvii block said it needed.
2. **Correction of the 6b-xvii block** (startRef `:1712-1714`). Quote verbatim, joined across its line breaks and
   with its bold markers: "**The rename is therefore the same blocker class as the `PathSegmentKind` row this
   register already parks as blocked**, and it was excluded by user decision; a backlog item covers both renames
   with real migrations." State the facts of section 3.1 item 4, measured by T4.
3. **Line drift in the 6b-xvii block:** its `:30`, `:31`, `:32` for the `Rotation` chain and `:28` for the `Kind`
   chain's `.HasConversion<string>();` now read `:39-42` and `:37` (T4 re-measures with `grep -n` on
   `PlacedObjectConfiguration.cs`); its `ObjectRotation.cs` member lines `:16`, `:21`, `:26` now hold
   `Clockwise90`, `Clockwise180`, `Clockwise270`.
4. **The entry stays part done** because of the `ct` and `obj` residues (`MASTER-130`, EXCLUSIONS 2 and 3).
5. AO and AC each print **18** after the block.
