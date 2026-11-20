# Coverage — session 2026-09-16-core-domain-invariants

Written at Gate 2, **before** the approval that freezes `manifest.json`. The `Gate 2` column is complete here;
the `End state` column is filled at Gate 3 from `reviews/<task-id>.md` and from the contract-operation search.

The mapping was produced by `dir-architect` with the draft manifest and re-checked by the orchestrator against
`spec.md`'s numbered definition of done. The architect flagged that no `spec.md` existed when it mapped, so it
mapped against the dispatch text instead; `spec.md` was then written from the same decisions, and every row
below was re-derived against the spec as written.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. `SetOutline` rejects null with `ArgumentNullException` | T1 | covered | met |
| 2. `SetOutline` rejects fewer than 3 segments with exact `ArgumentException` | T1 | covered | met |
| 3. Both rejections precede any `_outline` mutation, proven on segment values | T1 | covered | met |
| 4. No `ClearOutline` affordance added | T1 | covered | met |
| 5. `Floor..ctor` throws `ArgumentOutOfRangeException` below 1, before any assignment | T2 | covered | met |
| 6. `UpdateNumber` throws the same, before `Number` and before `Touch()` | T2 | covered | met |
| 7. Private parameterless `Floor()` unchanged and ungated | T2 | covered | met |
| 8. `Dungeon.ReorderFloor` still returns `MutationOutcome.OutOfRange` | T2 | covered | met |
| 9. No `FloorNumber` value object | T2 | covered | met |
| 10. `PathSegment` rejects `CubicBezier` with either control point null | T3 | covered | met |
| 11. `PathSegment` rejects `Line` carrying either control point | T3 | covered | met |
| 12. Guard runs on JSON deserialization, with a positive control and a corrupt-row case | T3 | covered | met |
| 13. `Ctrl1` / `Ctrl2` doc comments corrected | T3 | covered | met |
| 14. Construction census covers both the explicit and target-typed forms | T3 | covered | met |
| 15. `Translate_SegmentWithOnlyCtrl2Null_…` deleted, not repaired | T3 | covered | met |
| 16. `AreaOutlineChangeTrackingTests`' two single-segment outlines raised to ≥3 | T1 | covered | met |
| 17. Each guard shown able to fail, with a named positive control still passing | T1, T2, T3 | covered | met |
| 18. `dotnet build src/DungeonMaster.sln` — 0 warnings, 0 errors, all three App TFMs | T3 | covered | met |
| 19. All three test projects green, totals reported against the 796/97/23 baseline | T3 | covered | met |

**No uncovered rows.** Item 17 is the one row covering three tasks: each of T1, T2 and T3 carries its own
discrimination criterion (T1 §7, T2 §7, T3 §8), each removing or weakening only its own guard and naming the
positive control that must survive. It is one definition-of-done item because it is one standard, and it maps
cleanly onto three separate criteria rather than being a bundle the gate cannot see inside.

Items 4 and 9 are each covered twice over: by the owning task's criteria and by `contracts/domain-invariants.md`
§4, which forbids them repository-wide.

## Contract operations

`contracts/domain-invariants.md` declares behaviour on constructors and setters rather than a call-shaped API,
so "consuming task" reads as *the task that calls it and would notice it broken*. This is recorded here because
it is the reading the Gate 3 search will be run under.

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| §1 `Area.SetOutline` null rejection | T1 (`AreaMethods.cs:40`) | T1, via `dungeon.UpdateAreaOutline` | covered | **called** — `src/DungeonMaster.Domain/Entities/Floors/FloorMethods.cs:170` |
| §1 `Area.SetOutline` count rejection | T1 (`AreaMethods.cs:40`) | T1 | covered | **called** — same site, `FloorMethods.cs:170` |
| §2 `Floor..ctor` range guard | T2 (`Floor.cs:33`) | T2, direct via `InternalsVisibleTo` | covered | not called |
| §2 `Floor.UpdateNumber` range guard | T2 (`FloorMethods.cs:14`) | T2, direct | covered | not called |
| §3 `PathSegment` `CubicBezier` rule | T3 (`PathSegment.cs:35`) | T3 (`PathSegmentTests`); exercised indirectly by T1's and T2's segments under §3.2 | covered | not called |
| §3 `PathSegment` `Line` rule | T3 (`PathSegment.cs:35`) | T3 | covered | not called |
| §3.1 validation on JSON deserialize | T3 (a consequence, not new code) | T3 (`PathSegmentJsonRoundTripTests`) | covered | not called |

**No uncovered rows.**

### The Gate 2 prediction was wrong, and the search is what corrected it

At Gate 2 this file predicted, in writing and before dispatch, that **all seven operations would search as
`not called`** — on the reasoning that every consumer is a test inside the implementing task, which is exactly
what the search discards. **Two of the seven came back `called`, so that prediction was wrong.** Recording the
refutation rather than quietly overwriting it, because a prediction made to pre-empt a suspicious-looking
result is worth exactly as much as its accuracy.

`Area.SetOutline` has a genuine call site at `FloorMethods.cs:170`, inside `Floor.UpdateAreaOutline`. It
survives all three discard rules: it is not the definition site (`AreaMethods.cs:40`), it is not in a file owned
by the task that owns the definition (T1 owns `AreaMethods.cs`; `FloorMethods.cs` is owned by **T2**), and it is
not a test file. It is the real application path into the guard.

**What the five `not called` rows do and do not mean.** The search is scoped to the session's touched files, per
its definition. `Floor.UpdateNumber` genuinely has four production callers — `DungeonMethods.cs:83`, `:132`,
`:140`, `:144`, verified in this session's `measurement.md` — but `DungeonMethods.cs` is deliberately outside the
change surface, so the instrument cannot see them. Likewise `PathSegment` is constructed in production at
`MapDrawingSession.cs:122` and `:125`, also untouched. So `not called` here reports *"no caller inside the change
surface"*, which for a session that changes only the guarded members and none of their callers is the expected
reading — not evidence the product cannot reach them.

**The evidence that these invariants are real is the per-guard discrimination runs of definition-of-done item
17, not this search.** Each guard was removed or weakened and the specific tests that must fail were observed
failing while a named positive control stayed green — for T1 and T2 the reviewer additionally ran the mirror
case, proving the two `Floor` guards are independently covered rather than one test standing in for both.
