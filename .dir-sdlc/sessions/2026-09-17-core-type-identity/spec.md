# Core type identity — MASTER-073 and MASTER-074

Establish two invariants the Domain's own design assumes but never states: that `ObjectKind` names its
own absence instead of silently meaning *wooden door*, and that an `Entity` is equal to another by identity
rather than by reference.

## Context

Audit session 6b-viii. `chore/repo-audit` at `9c2bff6`; 176 entry rows, 123 closed, 50 open, 3 part done;
no open S1 or S2. CORE is the largest open layer at 17, and these are two of its nine open S3s —
`docs/audit/00-master-index.md:261` and `:262`, both `api-design`, both compiler- and test-verifiable with
no human observation required.

`measurement.md` re-derived both against `9c2bff6` before this spec was written. **Neither is a live bug.**
`default(ObjectKind)` cannot be reached from the UI: the place path runs only inside
`ObjectPalette.ActiveObject is ObjectKind placedKind` (`DungeonDetailPage.xaml.cs:1433`), and the UI types
absence as `ObjectKind?` throughout. All three Domain `List.Remove(entity)` sites retrieve the instance from
the same list by `Id` first, so reference equality is exact at each. Both entries are therefore **latent**:
the type fails to establish something its design assumes, and every caller compensates by hand today.

The deliverable for each is accordingly **the invariant plus a test that fails without it** — the shape
6b-vii ran three times. No criterion asserts a failure reachable today, because the measurement refutes that
for both, and such a criterion would be unsatisfiable by correct code.

## Definition of done

1. `ObjectKind` gains a member `None`, declared **first** as a bare `None,` with **no explicit numeric
   value**, so it is implicitly `0` and `default(ObjectKind) == ObjectKind.None`. The file introduces no
   explicit enum value — the repository has none anywhere and must not gain its first here.
2. The 37 existing members are unchanged in name and declaration order; the count is 38.
3. `PlacedObject`'s five-parameter constructor rejects **both** `ObjectKind.None` **and** any value that is
   not a defined member — one guard, two conditions, each with `ArgumentOutOfRangeException`, `ParamName`
   `kind` and the offending value as `ActualValue`, ordered before the existing `scale` rejection. Neither
   condition implies the other: `Enum.IsDefined(ObjectKind.None)` is `true`.
4. `AddPlacedObjectCommandValidator` keeps `IsInEnum()` and gains `.NotEqual(ObjectKind.None)`, **with a
   test proving a validator carrying `IsInEnum()` alone still accepts `None`** — so the second rule's
   reason lives in a test rather than in prose.
5. The ordinal shift is shown not to disturb persistence or the asset packs, by existing tests passing
   unmodified; and no seed, fixture or test datum stores `"None"` as a `Kind`, a string that becomes
   persistable under this design and was not before.
6. `Entity` compares by runtime type and `Id`, through `Equals(object?)`, `Equals(Entity?)` and
   `IEquatable<Entity>`.
7. `GetHashCode()` agrees with `Equals`, derives from `Id`, and is stable across a `Touch()` that provably
   changes `LastModified`.
8. No `==` / `!=` operator is declared on `Entity`.
9. The equality change is shown not to disturb EF or the three Domain `Remove` sites, by existing tests
   passing unmodified.
10. Each change is shown able to fail: a run injection per task, with a **named positive control that stays
    green** in each.
11. `dotnet build src/DungeonMaster.sln` reports 0 warnings and 0 errors across all three App TFMs.
12. The suite is green, each task measuring its own before/after baseline rather than citing a prior
    session's total.
13. `MASTER-073` and `MASTER-074` are marked done at `docs/audit/00-master-index.md:261-262`. MASTER-073
    closes **with** CORE-017's recommended direction but **against** its migration clause, which is struck as
    false: there is no integer column, because `Kind` is mapped `.HasConversion<string>()`.

## Scope

`ObjectKind.cs`, `PlacedObject.cs` and `Entity.cs` in the Domain, `AddPlacedObjectCommandValidator.cs` in
the Application layer, plus one new test file per task and two added facts in the existing
`AddPlacedObjectCommandValidatorTests.cs`.

### Not in scope

- **`ObjectKindInfo.cs` is not edited.** Its three `_ =>` arms (`:67`, `:109`, `:129`) and
  `GetDefaultAnchor`'s fallback will absorb `None` as a 1.00 × 1.00 centre-anchored "Decoration". This is a
  **named, accepted residual**, not an oversight: the same arms absorb any undefined value today, so it is
  not a regression. It is carried to the Gate 3 backlog by the user's ruling, and recorded in contract §2.4.
- **No `==` / `!=` operators on `Entity`** — declaring them changes the meaning of every existing `==` over
  an entity-typed expression, a surface this session has not measured.
- **No subclass gains its own equality override**; the base class is the whole of the change.
- **No production seam for testability** — `Id` does not become `internal set`, and no `WithId` helper is
  added. Same-`Id` instances come from reflection.
- `FloorObjectMethods.cs`, the three `Remove` sites, `src/DungeonMaster.Infrastructure/**`, and the
  asset-pack manifests are untouched; a task concluding otherwise stops and escalates.
- The seven other open S3s, the three `part done` entries, and the carried-forward backlog.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Does renumbering `ObjectKind` break persisted data? | No. The column stores the member **name**. | repo | `PlacedObjectConfiguration.cs` `.HasConversion<string>()`; no `(ObjectKind)` numeric cast, no `Enum.GetValues`, no JSON carrying the ordinal |
| Does CA1008 ("enums should have a zero value") fail the build under the rejected option A? | **No** — which is itself a cost of A, since nothing in the toolchain would ever explain a 1-based enum to a later reader | measured | Orchestrator probe at `9c2bff6`: applied `DoorWood = 1` in the worktree, `dotnet build …/DungeonMaster.Domain.csproj` → `0 Warning(s), 0 Error(s)`; file restored and worktree verified clean |
| Does a standing project convention forbid sentinel members in domain enums? | **No** — no enum-design guidance exists | repo | Searched `dm-conventions/backend/dotnet-guidelines.md` and `dotnet-format.md`; only incidental `IEnumerable` matches |
| Is a 1-based enum even inside the Microsoft guideline's exception? | **No.** The guideline permits `None = 0` or `most-common-default = 0`; a 1-based enum is neither | external | The guideline as quoted at `docs/audit/01-domain-application.md:735-739` |
| Did option A make an invalid kind unpersistable? | **No.** `EnumToStringConverter` writes `value.ToString()` and `Enum.Parse` accepts numeric strings, so `(ObjectKind)0` would persist as `"0"` and materialise back through the ungated private constructor | measured | Architect probe, recorded in contract §1.2 |
| Is `ArgumentOutOfRangeException(paramName, actualValue, message)` this repo's form for a domain rejection? | Yes, not `InvalidEnumArgumentException` | repo | `PlacedObject.cs:30`, `:82`; the Domain does not reference `System.ComponentModel` |
| Can reflection set `Entity.Id` through its private setter? | Yes | external / measured | Architect probe on SDK `10.0.400`: `CanWrite=True`, non-public `set_Id`, `SetValue` succeeded same-type and cross-type; recorded in contract §3.5 |
| Does overriding `Equals`/`GetHashCode` affect EF change tracking? | No — the tracker keys on the primary key | repo | The four configurations; `DungeonWriteRepository.cs:59` resolves by key |
| Is the audit-index closure a task or orchestrator work? | Orchestrator, at Gate 3 | repo | Prior session's manifest has no docs task; `eca0cd8` was made outside the tasks. **Reported as an uncovered row regardless — the gate decides.** |
| Are all four `Entity` subclasses `sealed`? | Yes, which makes the `GetType()` comparison unambiguous | repo | `grep -rn -E "class [A-Za-z]+ *: *Entity\b" src/` |

## Contracts

- [`contracts/core-type-identity.md`](contracts/core-type-identity.md) — §1 `ObjectKind` numbering and the
  design decision; §2 the `PlacedObject` kind guard (exception type, `ParamName`, `ActualValue`, ordering);
  §3 `Entity` equality (behaviour table, hash stability, no operators, EF non-impact, the reflection
  mechanism, the vacuity table, the namespace trap); §4 rules binding on both tasks.

## Open questions

| Question | Who resolves it | When |
|---|---|---|
| ~~Option A departs from CORE-017's recommendation. Ratify?~~ | The user | **Resolved at Gate 2 (escalation E1): option B, member named `None`.** The architect's steelman reversed its own recommendation, retracting three of its four original arguments; the ruling followed that reversal, not deference to the audit record |
| ~~Definition-of-done item 13 (the audit-index closure) is covered by no task.~~ | The user | **Resolved at Gate 2:** the orchestrator writes it after the merge, in a separate docs commit on the base branch, matching `eca0cd8` |
| `Entity.GetHashCode` has no production consumer — nothing in the product is keyed by an entity today. | The user | **Gate 2**, for visibility; it ships regardless, because it must agree with `Equals` |
