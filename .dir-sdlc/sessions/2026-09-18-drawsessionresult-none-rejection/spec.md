# 6b-xi - Reject DrawSessionResult.None at its consumer, and guard the enum's cardinality

**Goal.** Close `MASTER-179` by making a leaked `DrawSessionResult.None` *detected* rather than
merely inert, in the shape this repository's own code already uses for a never-occurring enum
member -- and guard the enum against a silent renumbering while we are here.

## Context

6b-ix added `None` at zero to `DrawSessionResult` in `App.Core`, so a leaked default now commits no
vertex instead of fabricating one. It deliberately did **not** touch the two consumer sites, and the
user ratified that at Gate 2: leaving them unchanged is what makes a leaked default *inert*.

`MASTER-179` stays open on the gap between **inert** and **detected**. Those are different
properties, and the entry is open on exactly that difference.

Re-measured at `061b28b` before decomposition (`measurement.md`): both cited sites are at the line
numbers the entry gives, `DungeonDetailPage.xaml.cs:1473` and `:1489`, unmoved, and neither rejects
`None`. The entry is still worth a session.

**The design question was routed to a counted survey**, because the user declined to pick a shape
and asked the repository's convention to decide. The survey's first scoping was decisive and
**false** -- it found 7 App-layer catch-alls all absorbing and concluded the layer's convention was
to absorb, which would have closed `MASTER-179` with no code change at all. Every one of those seven
is a *value-producing* switch expression where C# requires a catch-all; our site is a statement
chain that is required to do nothing. Re-scoped, the App layer contains **zero** sites that ignore a
value which cannot occur. Correctly scoped the count is **n = 1** and it throws:
`MutationOutcomeExtensions.ToResult`. That first wrong scoping is recorded in `measurement.md`
section 2.1 rather than replaced, because it is the same failure -- a count extended to a population
it was never validated against -- that froze a false criterion in each of the last two sessions.

**One count is a weak convention and is reported as one.** It is not "the repository always does
this"; it is "the repository has done this once, in the closest analogue that exists, and has never
done anything else with a never-occurring member."

## Definition of done

1. `DrawSessionResult.None` can no longer reach either consumer comparison at
   `DungeonDetailPage.xaml.cs:1473` / `:1489` -- it is rejected before them.
2. The rejection follows the shape the survey counted: a dedicated arm for the never-returned
   member, a separate catch-all arm with a **different** message, and an `<exception>` doc block
   naming both cases distinctly.
3. The dedicated `None` arm is **detectably** load-bearing -- deleting it reddens exactly one fact.
4. A value outside the declared members is rejected with its own distinct message.
5. `Added`, `Closed` and `Ignored` still flow through unchanged (positive control).
6. Nothing else in `DungeonDetailPage.xaml.cs` changes.
7. A future insertion or append to `DrawSessionResult` fails loudly rather than renumbering
   silently (`backlog.md:401`).
8. That new cardinality guard is shown able to fail, by an injection that reddens it alone.
9. `MapDrawingSession.cs` carries no permanent change -- the injection is fully reverted.
10. `src/DungeonMaster.App` builds clean on `net10.0-windows10.0.19041.0`.
11. `DungeonMaster.Tests` green.
12. `DungeonMaster.App.Tests` green.
13. Every new and edited file is CRLF with a final newline, verified in binary.
14. The full solution suite is green in the **main checkout after the merge**, and `MASTER-179`'s
    index row is closed citing the merge commit.

Items 1-13 decompose into task acceptance criteria. Item 14 is orchestrator work and is recorded as
uncovered in `coverage.md` -- tasks cannot invoke git, and the post-merge run happens after every
task is done.

## Scope

- `src/DungeonMaster.App/Views/DungeonDetailPage/` -- the guard and the one changed statement.
- `src/DungeonMaster.App.Tests/Views/` -- the guard's facts.
- `src/DungeonMaster.Tests/AppCore/Map/DrawSessionResultZeroValueTests.cs` -- the cardinality fact.
- `src/DungeonMaster.App.Core/Map/MapDrawingSession.cs` -- **injection only**, pinned byte-identical
  by sha256 at `startRef`.
- `docs/audit/00-master-index.md` and `.dir-sdlc/` -- orchestrator, after the merge.

### Not in scope

- **`ObjectKindInfo`'s `None` absorption** (`backlog.md:353`). Different file, different layer,
  already recorded as a deliberate residual by 6b-ix's Gate 3.
- **Any change to `MapDrawingSession.PlaceVertex` or to `DrawSessionResult`'s members.** The
  producer's contract is what makes `None` unreachable; changing it would change the thing being
  guarded.
- **The remaining five open S3 entries.** `MASTER-044`, `-047`, `-051`, `-081`, `-107` stay open.
- **The 8 lone CR characters in `.dir-sdlc/backlog.md`.** Outside the approved scope for the sixth
  session running; still not from this session.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| What does this repository do with an enum member documented as never occurring? | Throws `ArgumentOutOfRangeException` from a dedicated arm, with a separate catch-all arm carrying a different message and an `<exception>` block naming both | 1 (repo) | `measurement.md` section 2.4; `MutationOutcomeExtensions.cs:56-86`. **n = 1** -- reported as a weak convention |
| Is `Debug.Assert` an option the repository would recognise? | No | 1 (repo) | `measurement.md` section 2.2 -- **0** `Debug.Assert` and **0** `Trace.Assert` in production |
| Can a rejection at the consumer be reached by a test? | **No.** `None` is unreachable through `PlaceVertex` | 1 (repo) | `MapDrawingSession.cs:11`, and its three `return` statements at `:70`, `:75`, `:79` |
| Does the literal inline transplant of that shape compile here? | **No.** `CA2208` is an error when `nameof()` names a *local* rather than a parameter | measured this session | Build probe reproduced independently by the orchestrator under `AnalysisLevel 10.0` / `AnalysisMode Recommended` / `TreatWarningsAsErrors=true` / **`EnforceCodeStyleInBuild=true`**: `error CA2208`. The extension-method form built clean, `0 Warning(s) 0 Error(s)` |
| Is an `internal static` non-MAUI class in `App` callable from `App.Tests` without a host? | Yes | 1 (repo) | `src/DungeonMaster.App/AssemblyInfo.cs:3` grants `InternalsVisibleTo("DungeonMaster.App.Tests")`; `App.Tests/Converters/NotNullConverterTests.cs` is the existing hostless precedent |
| Does the page already import the guard's namespace? | Yes -- no `using` need be added | 1 (repo) | `PaletteThumb.cs:6` declares `DungeonMaster.App.Views.DungeonDetail`; `DungeonDetailPage.xaml.cs:13` imports it |
| Can appending a fifth enum member fail the build? | No -- there is no `switch` over this enum anywhere | 1 (repo) | `grep -rn "DrawSessionResult" --include=*.cs src/`, `/obj/` and `/bin/` filtered |
| Is the suite green at `startRef`? | Yes -- **969 passed / 0 failed** | measured this session | `dotnet test src/DungeonMaster.sln -c Debug` in the **main checkout** at `061b28b`: `DungeonMaster.Tests` 849, `App.UnitTests` 23, `App.Tests` 97; `grep -icE "warning|error"` over the whole output = **0** |
| Is `DungeonMaster.App` running (MSB3026/MSB3027 risk)? | No | measured this session | `tasklist` -- no matching process |

## Contracts

**None.** The two tasks own disjoint files in disjoint projects and neither calls anything the other
writes. `manifest.json` records `"contracts": []`; git does not track empty directories, so no
`contracts/` directory appears in the commit. This matches 6b-x, which declared none for the same
reason. Recorded as a finding rather than an omission -- see `coverage.md` section 2.

## Open questions

| Question | Who resolves it | When |
|---|---|---|
| **E1.** Where does the rejection live -- a new `internal static` guard class, an extracted private method on the page, or inline with a non-convention exception? `CA2208` makes this a build-forced choice the Gate 1 evidence did not consider | **User** | Gate 2 |
| **B9's falsifiability command is unsatisfiable as written** and needs rewording before the freeze | **User ratifies; orchestrator edits the draft** | Gate 2 |
| Does drawing still work in the running application? | **Human observation** -- no automated check replaces it | Before Gate 3 approval; recorded as `coverage.md` U5 |
