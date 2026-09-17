# Coverage — 2026-09-17-core-type-identity

Produced by `dir-architect` in decomposition, written here by the orchestrator before the Gate 2 approval
that freezes `manifest.json`. The `End state` columns are written at Gate 3.

Item numbers follow `spec.md`'s **Definition of done**. The architect derived its mapping before `spec.md`
existed and said so; the rows below are re-stated against the spec as written, and the mapping is unchanged
by that re-derivation except that the architect's items 1–14 collapse to the spec's 1–13 (its item 3, "the
A-vs-B choice is made and pinned", is a decomposition output rather than a definition-of-done item, and is
carried as an Open question in the spec instead).

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1 — `None` declared first, implicitly zero, no explicit numeric value in the file | T1 | covered | met |
| 2 — the 37 existing members unchanged in name and order; count 38 | T1 | covered | met |
| 3 — `PlacedObject` rejects **both** `None` and any undefined value, one guard, `ParamName` `kind`, with `ActualValue`, ordered before the `scale` rejection | T1 | covered | met |
| 4 — validator keeps `IsInEnum()` and gains `.NotEqual(ObjectKind.None)`, with a test proving `IsInEnum()` alone accepts `None` | T1 | covered | met |
| 5 — the ordinal shift disturbs neither persistence nor asset packs; no seed or fixture stores `"None"` as a `Kind` | T1 | covered | met |
| 6 — `Entity` compares by runtime type + `Id`, all three surfaces | T2 | covered | met |
| 7 — `GetHashCode` agrees with `Equals`, stable under a proven `Touch()` | T2 | covered | met |
| 8 — no `==` / `!=` declared | T2 | covered | met |
| 9 — EF and the three `Remove` sites undisturbed | T2 | covered | met |
| 10 — each change shown able to fail, with a named control staying green | T1, T2 | covered | met |
| 11 — full solution build, 0 warnings / 0 errors, three App TFMs | T1, T2 | covered | met |
| 12 — suite green, each task against its own baseline | T1, T2 | covered | met |
| 13 — `MASTER-073` / `MASTER-074` marked done at `00-master-index.md:261-262` | — (orchestrator) | **uncovered at decomposition; Gate 2 decision: the orchestrator writes it at Gate 3, in a separate docs commit on the base branch after the merge, matching `eca0cd8`. Not added to the manifest — a task cannot cite commit hashes that do not exist until after the merge.** | **pending** — orchestrator writes it after the merge, per the Gate 2 decision |

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| §1 `ObjectKind.None` at zero | **T1** (`ObjectKind.cs`) | T1 | covered | **not called** — all 15 hits are in T1-owned files or test files |
| §2 `PlacedObject` `None`-and-undefined rejection | **T1** (`PlacedObject.cs`) | T1 | covered | **not called** — definition site plus T1-owned test hits only |
| §3.1–3.2 `Entity.Equals` (both overloads, `IEquatable<Entity>`) | **T2** (`Entity.cs`) | T2 | covered | **not called** — 13 hits, all in the T2-owned test file |
| §3.1 / §3.3 `Entity.GetHashCode` | **T2** (`Entity.cs`) | T2 — tests only | covered, with the caveat below | **not called** — definition site plus T2-owned test hits only |

**`Implementing task` is empty at Gate 2 by design** — it is derivable only at Gate 3, once the
definition-site search has run.

**Two things the architect reported rather than tidied away, and neither is hidden here:**

- **`Entity.GetHashCode` has no production consumer.** The measurement's census found zero entity-keyed
  `HashSet<T>` or `Dictionary<T, …>` in production and the architect reproduced that. It ships anyway,
  because a `GetHashCode` that disagrees with `Equals` is a defect in itself and CORE-018 asks for it — but
  its only caller in the product is `EqualityComparer<T>.Default` inside collections nothing currently uses.
- **The production consumers of §1, §2 and §3.1–3.2 are all owned by no task this session** —
  `Floor.AddObject` (`FloorObjectMethods.cs:41`), `ObjectKindInfo`, the palette views, and the three
  `Remove` sites at `DungeonMethods.cs:74`, `FloorMethods.cs:98`, `FloorObjectMethods.cs:73`. The Gate 3
  call-site search is scoped to the session's change surface, so **these operations are expected to read
  `not called` at Gate 3**. That is a property of a session that deliberately changes a base type without
  touching its callers, not a coverage hole — recorded now, before the fact, so the Gate 3 result is read
  against a prediction rather than explained after it.

The prediction above is pre-registered in the sense 6b-vii used the term, and 6b-vii's equivalent prediction
was **refuted** — two of seven operations came back `called`. It is written to be checked, not to be trusted.

## Amended after the Gate 2 escalation

Escalation E1 resolved to **option B** (`ObjectKind.None`), reversing the design the mapping above was first
drafted against. Rows 1–5 and the first two contract operations were re-stated accordingly; **the mapping
itself did not change** — the same task covers each item, and no row moved between covered and uncovered.
The residual the switch created — `ObjectKindInfo`'s `_ =>` arms absorbing `None` — is **not** a
definition-of-done item: it is a named accepted gap in contract §2.4, carried to the Gate 3 backlog by the
user's ruling. It is recorded here so that a reader of this file alone cannot mistake its absence from the
table for an omission.

## Gate 3 — the pre-registered prediction held

The Gate 2 block above predicted, in writing and before dispatch, that **all four contract operations would
read `not called`** at Gate 3, because this session deliberately changes base types without touching their
callers. **The search confirmed it: four of four `not called`.**

Method, run in the session worktree over the seven files in
`git diff --name-only 9c2bff6..dir/2026-09-17-core-type-identity`, with the three discards applied in order —
the definition site, every hit in a file owned by the task owning that definition site, and every hit in a
test file:

| Operation | Raw hits in the change surface | Surviving all three discards |
|---|---|---|
| `ObjectKind.None` | 15 across 4 files | 0 |
| `PlacedObject(` construction | 7 across 2 files | 0 |
| `.Equals(` | 13 in 1 file | 0 |
| `GetHashCode` | 9 across 2 files | 0 |

**This is the expected result here and it is not a coverage hole** — it was predicted before the fact for a
stated structural reason. The production consumers exist and are untouched: `Floor.AddObject`
(`FloorObjectMethods.cs:41`), `ObjectKindInfo`, the palette views, and the three `Remove` sites at
`DungeonMethods.cs:74`, `FloorMethods.cs:98`, `FloorObjectMethods.cs:73`. They are outside the change surface
by design, which is exactly why the search cannot see them.

**What it does not prove.** `not called` here means *no call site inside this session's change surface*, not
*nothing calls it*. The check as defined cannot distinguish those two, and this session is the case where the
difference matters — so the row is read as confirming a prediction, never as evidence that the product does
not reach these operations.

6b-vii's equivalent prediction was refuted two ways out of seven. This one survived; that is a result about
this session's shape, not a reason to trust the next one.
