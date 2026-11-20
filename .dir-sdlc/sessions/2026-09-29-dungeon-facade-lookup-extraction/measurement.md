# Measurement — session 6b-xiv

**Fixed commit for every figure below: `20b0b09`** (`chore/repo-audit`, 0 ahead / 0 behind `origin`).
Where a figure is taken at another commit, that commit is named inline.

**Every claim in this document names the command that produced it.** A claim with no command is marked
`DERIVED` or `UNMEASURED`. This document is what the architect decomposes from, so an unchecked claim
here becomes a frozen criterion — the failure mode the last two sessions each recorded.

---

## 0. Instrument warnings established in this session

Two of my own instruments were wrong before they were right. Both are recorded because the wrong
version was reproducible and confident.

1. **Python text mode silently translates CRLF.** `open(p, encoding="utf-8-sig").read()` then
   `t.split("\r\n")` returned **1 line** for a 489-line file, and every `\r\n`-anchored regex returned
   **0**. Use `newline=''` and assert `"\r\n" in t` before counting. The first run of the duplication
   census reported `0 occurrences` of a block that occurs 10 times.

2. **A pipe through `tee` masks the exit code.** `dotnet test <bad path> | tee log | tail` reported
   **exit 0** while the log held `MSBUILD : error MSB1009: Project file does not exist.` Redirect
   instead and echo `$?`, or read the log.

3. **The solution is at `src/DungeonMaster.sln`, not the repository root.** `find /c/dm-wt/6b-xiv
   -maxdepth 2 -name "*.sln*"` returns exactly that one path.

Carried in from the handoff and not re-measured this session: `grep -r` over a test project matches
compiled assemblies (scope with `--include=*.cs`); `grep -c "[Fact]"` parses as a character class (use
`-cF`); line-ending counts need a byte-exact Python count.

---

## 1. `MASTER-107` / `CORE-015`, claim by claim

The entry is at `docs/audit/01-domain-application.md:751`; its index row is
`docs/audit/00-master-index.md:295`.

**The authoring commit is `16d594b`, "docs: add code-quality audit for domain and application",
2026-08-17** — `git log --oneline --reverse -S "Dungeon is a 453-line delegation facade" --
docs/audit/01-domain-application.md`. Every claim below is checked against *both* `16d594b` and
`20b0b09`, because that is what separates a stale citation from a wrong one.

| # | Claim | At `16d594b` | At `20b0b09` | Verdict |
|---|---|---|---|---|
| 1 | "453-line" | **453** | **489** | **Stale.** Correct when written |
| 2 | "largest file in the Domain" | not checked | **489**, next is `FloorMethods.cs` at **176** | True |
| 3 | "eight of its eleven methods" | **13** public methods | **12** public methods | **"eleven" was wrong when written.** "eight" is right |
| 4 | The 8 `locations:` ranges | all 8 are exact declaration sites | all 8 drifted — **`+12` to `+40`**, see below | Exact at authorship, stale now |
| 5 | "forward to the **identically named** `Floor` method" | — | true for **4 of 8** | **Half false** |
| 6 | Its list of forwarded names | — | 7 names; omits `UpdateDetails` | Incomplete |

Commands:

- Line counts: `b.count(b'\n')` in binary on the file, and
  `git show 16d594b:src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs | wc -l` → `453`.
  **Note the path differs at `16d594b`** — `Entities/Dungeon/`, singular, before the namespace
  pluralisation at `070d535`.
- Largest-in-Domain: `os.walk("src/DungeonMaster.Domain")` excluding `obj`/`bin`, 21 `.cs` files,
  ranked by `b.count(b'\n')`.
- Method census: regex over `^\tpublic <ret> <Name>(` excluding the constructor, run against both
  commits. **Predicate validated by eye against the full file read** (490 lines) — the 12 are `Update`,
  `AddFloor`, `RemoveFloor`, `ReorderFloor`, `UpdateFloor`, `AddArea`, `RemoveArea`, `UpdateArea`,
  `UpdateAreaOutline`, `AddPlacedObject`, `RemovePlacedObject`, `UpdatePlacedObject`. The 13th at
  `16d594b` is the second `AddPlacedObject` overload, deleted by 6b-xiii at `a9a5935`.

### CORRECTED AT GATE 3 — claim 4's drift range was false, and it is the only FIGURE here with no command

**This row originally read `+13` to `+51`. Both endpoints were wrong.** It was prescribed verbatim into
frozen criterion C15, and T3 — forbidden `git` and required to use this document as given — wrote it
into a permanent audit record. **Found by T3's reviewer as a blocking finding, confirmed by the
orchestrator, and repaired through escalation E4 → amendment A1.**

Measured by resolving each cited range against both commits, and re-measured independently by T3's
reviewer:

| member | `CORE-015` cites | declaration at `20b0b09` | opening drift | closing drift |
|---|---|---|---|---|
| `UpdateFloor` | 155-171 | 168 | +13 | +15 |
| `AddArea` | 191-204 | 203 | **+12** | +16 |
| `RemoveArea` | 215-229 | 234 | +19 | +27 |
| `UpdateArea` | 249-271 | 279 | +30 | +37 |
| `UpdateAreaOutline` | 286-303 | 326 | **+40** | **+48** |
| `AddPlacedObject` | 356-380 | 374 | +18 | +18 |
| `RemovePlacedObject` | 392-408 | 413 | +21 | +27 |
| `UpdatePlacedObject` | 429-452 | 459 | +30 | +36 |

**Opening-line drift is `+12` to `+40`; closing-line is `+15` to `+48`.** Neither endpoint of the
original figure is right.

**How it went wrong, stated plainly because it is the defect class this session is documenting.**
`+51` is `323 → 374` — the drift of the **four-parameter** `AddPlacedObject`. That is a `CORE-019`
citation, **not one of `CORE-015`'s eight**: `CORE-015` cites `356-380`, the five-parameter overload,
which moved `+18`. A figure was carried in from a neighbouring entry's citation. The low end was wrong
independently — the true minimum is `AddArea`'s `+12`, not `UpdateFloor`'s `+13`. **Neither endpoint was
measured.**

And the bare range was ambiguous even in principle, because it never said whether it measured opening or
closing lines. That ambiguity is half of why it went unchallenged: a reader checking one endpoint against
the other convention would find a near-miss rather than an error.

**This row was one of three in §1 whose `Commands:` list names no command for it, and it is the only
one of the three that states a figure** — claims 5 and 6 state qualitative verdicts. §1 carries **6**
claims against **3** command bullets, which cover claims 1, 2 and 3.

> **This paragraph originally read "the only row in §1 with no command". That was false**, and it was a
> false universal written inside the correction of a false figure — caught by T3's round-2 reviewer as a
> `contradicts` finding, repaired by escalation E5 / amendment A2. **Two orchestrator errors, the second
> nested inside the first's repair.** Predicate validated both directions: claim 3 is the near-miss that
> must not match, because the `Commands:` list does name its method census; claim 6 is the case that
> must, naming no command anywhere in §1.

The rule this session inherited — *run any claim about a mechanism, not just about a tool* — has a
corollary this row supplies, and the correction sharpens it rather than weakening it: **a figure
asserted where the document's own convention names a command is not an oversight, it is a signal.** The
distinguishing property was never uniqueness; it is that claims 5 and 6 are verdicts a reader can check
by reading, and claim 4 was a number nobody could.

### The four renames behind claim 5

Four of the eight forwarders do **not** call an identically named `Floor` method:

| `Dungeon` method | forwards to |
|---|---|
| `UpdateFloor` | `floor.UpdateDetails` |
| `AddPlacedObject` | `floor.AddObject` |
| `RemovePlacedObject` | `floor.RemoveObject` |
| `UpdatePlacedObject` | `floor.UpdateObject` |

The four that do match are `AddArea`, `RemoveArea`, `UpdateArea`, `UpdateAreaOutline`.

---

## 2. The duplication census

Run with `newline=''` against
`src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs` at `20b0b09`.

| Pattern | Count |
|---|---|
| `_floors.FirstOrDefault(floor =>` + newline + `floor.Id == floorId);` | **10** |
| `if (floor is null)` | **10** |
| … guarding `return MutationOutcome.NotFound;` | **8** |
| … guarding `return null;` | **2** |
| `if (outcome is not MutationOutcome.Success) { return outcome; }` | **5** |
| `Touch();` | **12** |

**File composition of 489 lines: 180 doc-comment, 66 blank, 243 code.**

**Two corrections to the entry's framing follow from this.**

- **The lookup duplication is 10-fold, not 8.** `RemoveFloor` and `ReorderFloor` carry the identical
  `FirstOrDefault` + null-guard but then run their own renumbering/shifting logic rather than
  forwarding, so the entry correctly excludes them from its **eight forwarders** and thereby
  undercounts the **ten** sites an extraction would serve.
  **Predicate control:** `Update` and `AddFloor` are the two methods with no lookup — confirmed by
  reading `:19-53`. 10 + 2 = 12 methods.
- **A second duplication the entry never mentions:** the `outcome` re-check, **5 times**. Any design
  aiming at the entry's stated "each wrapper collapses to two lines" has to address this, not only the
  lookup.

`Touch();` occurring 12 times against 12 methods means **every** public method touches — relevant to
any helper that wants to absorb it.

---

## 3. The entry's second suggested direction is not viable as written

The entry proposes either a `RequireFloor` extraction **or** "expose a narrow floor-scoped accessor
from the root and let callers work against `Floor` directly."

**Every `Floor` mutator is `internal`, and was `internal` at the authoring commit.**

`grep -nE "^[[:space:]]+(public|internal) [A-Za-z]"` over `FloorMethods.cs`,
`FloorObjectMethods.cs`, `Floor.cs` at `20b0b09`, and the same over the `Entities/Floor/` paths at
`16d594b`:

- `20b0b09` — `internal`: `UpdateNumber`, `UpdateDetails`, `AddArea`, `RemoveArea`, `UpdateArea`,
  `UpdateAreaOutline`, `AddObject`, `RemoveObject`, `UpdateObject`. `public`: only the 5 scalar
  properties plus the `Areas` and `Objects` read-only projections.
- `16d594b` — the same 6 + 5 members, all `internal`.

So the second option is a **Domain-accessibility change**, not a refactor. It is not a smaller
alternative to the first option; it is a larger one. **Scope this session to the extraction.**

---

## 4. Blast radius — why the entry's "high risk" does not survive narrowing

The entry rates `risk: high` because the wrappers are "the aggregate's whole public surface."

**The 8 forwarders have exactly 8 external call sites.** Each is one Application command handler, and
every one of the 8 is at line **`:24`** of its handler:

```
Features/Dungeons/Commands/AddArea/AddAreaCommandHandler.cs:24
Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommandHandler.cs:24
Features/Dungeons/Commands/RemoveArea/RemoveAreaCommandHandler.cs:24
Features/Dungeons/Commands/RemovePlacedObject/RemovePlacedObjectCommandHandler.cs:24
Features/Dungeons/Commands/UpdateArea/UpdateAreaCommandHandler.cs:24
Features/Dungeons/Commands/UpdateAreaOutline/UpdateAreaOutlineCommandHandler.cs:24
Features/Dungeons/Commands/UpdateFloor/UpdateFloorCommandHandler.cs:24
Features/Dungeons/Commands/UpdatePlacedObject/UpdatePlacedObjectCommandHandler.cs:24
```

Command: `grep -rn --include=*.cs -E "\.(UpdateFloor|AddArea|RemoveArea|UpdateArea|UpdateAreaOutline|AddPlacedObject|RemovePlacedObject|UpdatePlacedObject)\(" src/ --exclude-dir=obj --exclude-dir=bin`
filtered to exclude `DungeonMaster.Tests/` and `DungeonMaster.Domain/`.

**Predicate validated two ways**, because a uniform count of 1 across 8 different names is exactly the
shape of a broken filter:
- **Must-match control:** `.AddFloor(` → **1** external caller, consistent with the 1:1 handler design.
- **Must-not-match control:** `.Touch(` → **0** external callers, correct for a Domain-internal member.
- The 8 hits were then printed and read individually rather than counted.

**Consequence: preserve the 12 public signatures and zero call sites change.** That is the property a
criterion should rest on, and it is checkable by a clean build — a missed call site after a signature
change is `CS7036` in this solution, per the handoff's architect measurement.

**15 test files exercise these 8 members** —
`grep -rln --include=*.cs -E "\.(UpdateFloor|AddArea|…)\(" src/DungeonMaster.Tests/`.

### CORRECTED — the floor-absent paths are half uncovered, and a lookalike hides it

This paragraph first read *"the gap to check is the null / `NotFound` path at each of the 10 lookup
sites"*, which implied all ten needed work. **That was wrong for 5 of the 10, and wrong in the direction
that would have commissioned redundant work.** Found independently by the orchestrator and by
`dir-architect`, both by **reading the enclosing test body** rather than matching its name.

`DungeonAggregateTests.cs` holds five methods matching `WithUnknown.*Id_ReturnsNotFound` —
`RemoveArea…:251`, `UpdateArea…:267`, `UpdateAreaOutline…:286`, `RemovePlacedObject…:303`,
`UpdatePlacedObject…:319`. **Every one passes a valid `floor.Id` from `dungeon.AddFloor(...)` with an
unknown CHILD id.** Their `NotFound` comes from the area/object lookup **inside `Floor`**, one level
below `Dungeon`'s `floorId` guard. A survey matching the name shape counts all five and concludes those
paths are covered. They are not covered at all.

| # | `Dungeon` method | Floor-absent returns | Floor-absent test | State |
|---|---|---|---|---|
| 1 | `RemoveFloor` | `NotFound` | `DungeonAggregateTests.cs:115` | covered |
| 2 | `ReorderFloor` | `NotFound` | `:145` | covered |
| 3 | `UpdateFloor` | `NotFound` | `:215` | covered |
| 4 | `AddArea` | **`null`** | `:233` | covered |
| 5 | `AddPlacedObject` | **`null`** | `:456` | covered |
| 6 | `RemoveArea` | `NotFound` | — | **none** |
| 7 | `UpdateArea` | `NotFound` | — | **none** |
| 8 | `UpdateAreaOutline` | `NotFound` | — | **none** |
| 9 | `RemovePlacedObject` | `NotFound` | — | **none** |
| 10 | `UpdatePlacedObject` | `NotFound` | — | **none** |

**5 covered, 5 uncovered.** So the session's real test work is exactly **5 new cases**, and the
post-change suite prediction is **980**.

`AreaOutlineTests.cs:35 UpdateAreaOutline_WithUnknownFloor_IsNoOp` does pass an unknown floor id, but it
asserts only `act.Should().NotThrow()` and never inspects the return — **it would pass if the method
returned `Success`** — so it does not cover row 8. It also lives in a file no task owns, which is why
row 8 gets a new test in `DungeonAggregateTests.cs` rather than that one being strengthened.

**Consequence for the criteria, which is the Gate 2 vacuity check run before the freeze:** `spec.md`
item 8 (the 2 `null` sites) is **already satisfied at `startRef`** — a legitimate preserved-behaviour
item whose only evidence route is injected defect. `spec.md` item 7 (the 8 `NotFound` sites) is
**3-of-8 done**. Written from the item text alone, three criteria would have been vacuous on arrival.

---

## 5. The same pattern exists in `Floor`, 5 more times — OUT OF SCOPE, recorded

`grep -n "FirstOrDefault"` over `FloorMethods.cs` (**3**: `:90`, `:128`, `:162`) and
`FloorObjectMethods.cs` (**2**: `:56`, `:95`), each paired with an `is null` guard — 3 + 2 = 5, and
`t.count("is null)")` returns 3 and 2 respectively.

So the Domain holds **15** instances of lookup-and-guard: 10 in `Dungeon`, 5 in `Floor`.
`CORE-015` describes only `Dungeon`'s.

**CORRECTED — a precedent for the helper shape does exist.** This paragraph first concluded that "this
refactor introduces a pattern rather than matching one", on the basis that no private helper exists in
the Domain entities. **The scoped claim survives and the conclusion does not**, caught by
`dir-architect`. `grep -rnE "^\tprivate " src/DungeonMaster.Domain --include=*.cs` returns **9** lines:

- **8 under `Entities/`**, and none is a helper method — 4 private backing fields (`Area.cs:10`,
  `Dungeon.cs:8`, `Floor.cs:10`, `Floor.cs:11`) and 4 private parameterless EF constructors
  (`Area.cs:12`, `Dungeon.cs:10`, `Floor.cs:13`, `PlacedObject.cs:10`).
- **the 9th is a private static method** — `src/DungeonMaster.Domain/ValueObjects/PathSegment.cs:40`,
  `private static PathSegmentKind ValidateControlPoints(` — under `ValueObjects/`, not `Entities/`.

So "no private helper in the Domain **entities**" is true, and "this refactor introduces a pattern" is
false at Domain scope: `ValidateControlPoints` is a precedent for a private static helper in the Domain.
The distinction matters because the false version was on its way into a criterion, and the architect
reported catching and correcting its own first draft of `B2` after restating it.

**Not in scope.** Expanding to `Floor` is a scope change, not an implementation detail.

---

## 6. The two backlog items — one is a true finding, one is FALSE

Both concern the `AMENDED 6b-xiii` block in `docs/audit/01-domain-application.md`, which this session
has to edit anyway.

### Item 1 — `:993`, "within ten lines" — **TRUE finding, needs correcting**

The block states: *"no `Update` overload of either arity was within ten lines of `:62-63`."*

`git show b39a65d:src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs | grep -n "void Update"`
returns **`70:`** and **`83:`**. `:70` is **7 lines** below `:63`.

**The claim is false.** Confirmed independently of the backlog item.

### Item 2 — `:1000`, the "48 lines" figure — **the BACKLOG ITEM is false; the register sentence is sound**

The block states: *`AddPlacedObject` "drifted **downward** by 48 lines, from the cited `:323` to the
measured `:371`"*. The backlog item asserts this is *"arithmetic on a stale citation, not a measured
displacement — `:323` was never a measured position of `AddPlacedObject`, and nobody read the entry's
authorship commit."*

**`:323` was a measured position of `AddPlacedObject`, exactly.** At the authoring commit `16d594b`,
`git show 16d594b:src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs | sed -n '323,333p'`
returns the four-parameter scale-less `AddPlacedObject` declaration in full — opening at `:323`,
closing at `:333`. **The cited range `:323-333` is that declaration and nothing else.**

And the displacement is real. Tracking the declaration line across every commit that touched the file
(`git show <c>:<path> | grep -n "AddPlacedObject(" | grep -v "=> AddPlacedObject"`):

| Commit | 4-param | 5-param |
|---|---|---|
| `bb7405c` | **323** | 356 |
| `781f73b` | **371** | 404 |
| `a9a5935` | *deleted* | 374 |

`323 → 371` is **+48**, one member, two adjacent commits. **The register sentence at `:1000` is
accurate as written**, and the backlog item asserting otherwise is the thing that is wrong.

**The irony is exact and belongs in the record:** the finding's stated reason for doubting the figure
is that nobody read the authorship commit. Reading the authorship commit is what refutes it.

### Item 3 — 6b-xiii's stated CAUSE for both stale citations is false

Found after the architect was dispatched, so it is not reflected in the criteria it wrote. The block
states, at `:995-997`:

> **The drift has a named cause, and a direction.** … Both were caused by the **ObjectScale feature
> work**, which inserted the scale-carrying overloads above and beside the scale-less ones in all three
> files

**Three things are wrong with it.**

**(a) The ObjectScale work predates the entry by three months.**
`git log --no-walk --format='%h %ad %s' --date=short ec6c239 16d594b`:

| Commit | Date | Subject |
|---|---|---|
| `ec6c239` | **2026-05-21** | `feat(dungeons) Add object scale to placed objects` |
| `16d594b` | **2026-08-17** | `docs: add code-quality audit for domain and application` |

`git merge-base --is-ancestor ec6c239 16d594b` → true. The entry was authored **after** the ObjectScale
work, citing lines correct at that moment. What ObjectScale created was the scale-less / scale-carrying
overload **pairs** `CORE-019` is *about*; it did not move the line numbers. Those are two different
commits three months apart.

**(b) There are two causes, not one, and they are different commits.**

`DungeonMethods.cs:323-333` → **`781f73b`**, the first of exactly three commits to touch the file after
authorship, landing **one day** after it:

| Commit | Date | 4-param `AddPlacedObject` |
|---|---|---|
| `16d594b` | 2026-08-17 | `:323` |
| `781f73b` | 2026-08-18 | `:371` |

`git diff --numstat 781f73b^ 781f73b -- "*DungeonMethods.cs"` → `99  33`. Subject: *"report mutation
outcomes instead of silently no-opping"*. **That one commit is the whole +48.**

`PlacedObject.cs:62-63` → **`b4e2a74`**. The two-parameter `Update` held at `:62` through the first two
of four commits and moved at the third:

| Commit | Date | 2-param `Update` | file lines |
|---|---|---|---|
| `16d594b` | 2026-08-17 | `:62` | 93 |
| `413be78` | 2026-08-17 | `:62` | 93 |
| `070d535` | 2026-09-16 | `:62` | 93 |
| `b4e2a74` | **2026-09-17** | **`:70`** | **101** |

Subject: *"add `ObjectKind.None` and validate placement"* — the `ObjectKind` work, not ObjectScale.
**6b-xiii's arithmetic was right and its attribution was wrong:** it reconstructed the shift as "the
eight lines the four-parameter constructor and its blank separator occupied", and 93 → 101 is exactly
+8. It derived the *size* correctly and named the wrong commit.

**(c) One cause is claimed for "all three files".**
`git diff --numstat 781f73b^ 781f73b -- "*PlacedObject.cs"` returns **nothing**. No single commit moved
both citations.

### The larger finding both items sit inside

**Both of `CORE-019`'s cited ranges were exact at authorship, and 6b-xiii's corrections do not say so.**

- `DungeonMethods.cs:323-333` at `16d594b` → the four-parameter `AddPlacedObject`, exactly.
- `PlacedObject.cs:62-63` at `16d594b` → `:62` the two-parameter `Update` signature, `:63` its
  expression body. Exactly. (`grep -n "void Update"` at `16d594b` returns `62:` and `75:`.)

Correction 1 says the range "does not hold `AddPlacedObject` at any arity" and Correction 2 says
`:62-63` "holds the `Rotation` property's doc comment, not `Update` of either arity." **Both are true
of the file at `b39a65d` and both read as though the entry mis-cited.** It did not.

**And `CORE-015`'s 8 locations show the identical property** — all 8 exact at `16d594b`, all 8 drifted
by `20b0b09`. This is not one entry's defect. **It is a property of the whole audit document: citations
were exact on 2026-08-17 and have drifted with the code since.** That framing is worth more to a future
session than another per-entry correction, and it is the one thing here no prior session has stated.

---

## 7. Suite baseline

Measured in the worktree `C:/dm-wt/6b-xiv` at `20b0b09` — `dotnet test src/DungeonMaster.sln --nologo`.

**975 passed, 0 failed, 0 skipped.** Re-derived rather than inherited; it agrees with the handoff's
figure exactly.

| Assembly | Passed | Failed | Skipped | Total |
|---|---|---|---|---|
| `DungeonMaster.Tests` | 850 | 0 | 0 | 850 |
| `DungeonMaster.App.UnitTests` | 23 | 0 | 0 | 23 |
| `DungeonMaster.App.Tests` | 102 | 0 | 0 | 102 |
| **Sum** | **975** | **0** | **0** | **975** |

These are all **passed** counts. `850 / 23 / 102` are three assemblies' passed totals and nothing else —
**not** "850 passed, 23 skipped, 102 inconclusive", which is how a prior `commit-task` dispatch
relabelled them into `a116b2c`'s body. Any dispatch handed these figures must carry both number and
label verbatim.

Zero-warning check: `grep -icE "warning|error"` over the `dotnet test` log → **0**, and the qualified
form `grep -cE "(warning|error) [A-Z]+[0-9]+"` → **0**.

**The `Passed!` line's TFM is wrong for the third assembly, reproduced again this session.** All three
`Passed!` lines read `(net10.0)`, but the `Test run for ...` lines show `DungeonMaster.App.Tests` ran
from `bin\Debug\net10.0-windows10.0.19041.0\`. Read `Test run for`, never `Passed!`, for the TFM.

`DungeonMaster.App.Tests` can shed tests silently — a prior session saw it report
`Failed: 0, Passed: 81, Total: 81`, 21 short, with nothing failed or skipped. Treat its **102** as a
prediction to be checked, not a guarantee.
