# Session 6b-xiii -- Remove the five scale-less Domain overloads and close `MASTER-047`

**Goal.** Delete the five scale-less overloads `CORE-019` names, dispose of their 22 test call sites
correctly rather than mechanically, and close `MASTER-047` in the audit record with its two stale
citations repaired.

## Context

`MASTER-047` / `CORE-019` is an S3 `dead-code` entry that **no session had ever re-measured**. It
claims five Domain overloads have no production caller and survive only because
`DungeonMaster.Tests` exercises them through `InternalsVisibleTo`.

I measured it at Gate 1 before the scope was chosen, against the fixed commit `b39a65d`.
`measurement.md` in this directory is the authority for every number, and where `CORE-019` disagrees
with it, `CORE-019` is what needs correcting. Three things came out of that pass:

1. **The load-bearing claim holds.** Zero production callers for four of the five members; the fifth's
   single production caller is itself one of the dead members. But only an arity-aware instrument can
   establish that — this repository splits every call one argument per line, so no grep sees arity,
   and a local test helper in `MapHitTesterTests.cs` shares the name `PlacedObject`.
2. **2 of `CORE-019`'s 6 line citations are stale**, and neither was caused by work on this entry.
   Both were caused by the ObjectScale feature work — the same neighbour-falsification mechanism
   6b-xii filed as a backlog item when `CORE-006`'s fix falsified `CORE-005`.
3. **Seven of the affected tests are named after the API being deleted.** Their subject is the
   defaulting itself, so appending an argument would leave them asserting that passing `2.0f` yields
   `2.0f`. This is the fact neither the entry nor the inherited handoff anticipated, and it is why the
   test work is a task in its own right rather than a footnote to the deletion.

The ordering follows from the build: the call sites are converted **before** the overloads are
removed, so no intermediate state is red, and the clean build after the deletion is itself the proof
that no call site was missed.

## Definition of done

1. **D1.** The 13 mechanical call sites pass an explicit scale argument and resolve to the
   scale-carrying overload.
2. **D2.** The two `DungeonAggregateTests` `AddPlacedObject` tests survive as test cases, call the
   five-argument overload with `ObjectScale.Default` explicit, and no longer carry `WithoutScale` in
   their names.
3. **D3.** The five `[Fact]` methods whose subject the deletion removes no longer exist anywhere under
   `src/DungeonMaster.Tests/`, checked over `*.cs` sources only.
4. **D4.** The four tests that are the sole remaining cover for something D3 deletes still exist and
   still carry the assertions that make them that cover.
5. **D5.** The five scale-less members no longer exist in the Domain, their XML doc comments included,
   and each of the three affected types declares the surviving overload exactly once.
6. **D6.** `PlacedObject`'s private parameterless constructor and its `Scale` field initializer are
   unchanged — both are EF Core's materialization path, not members of the dead cluster.
7. **D7.** `src/DungeonMaster.Domain/AssemblyInfo.cs` is unchanged. `CORE-019` lists it as a location;
   it is the grant the rest of the test project reaches Domain internals through.
8. **D8.** `dotnet build src/DungeonMaster.sln --nologo` prints `Build succeeded` and
   `grep -cE "(warning|error) [A-Z]+[0-9]+"` over its output returns 0.
9. **D9.** `dotnet test src/DungeonMaster.sln --nologo` reports `Failed: 0` for all three assemblies.
   The totals are predicted at 850 / 23 / 102 = 975 against 855 / 23 / 102 = 980 at `b39a65d`; the
   verdict turns on `Failed: 0`, and any other total is reported as a surprise, never reconciled by
   assertion.
10. **D10.** `CORE-019`'s two stale citations are replaced by the corrected ranges, each qualified as
    a position at `b39a65d` rather than a current line range.
11. **D11.** `CORE-019` carries an `AMENDED 6b-xiii` block classifying the drift, quoting both
    previous citations verbatim, naming the cause, and stating that all three cited ranges are
    historical and not navigable at `HEAD`.
12. **D12.** `MASTER-047`'s `Status` cell in `00-master-index.md` opens `**done** (6b-xiii)`, records
    the three-way test disposition, records which 2 of 6 locations were stale, records the
    post-change suite total measured rather than predicted, and **carries no commit hash**.
13. **D13.** No file outside the three tasks' ownership sets is modified. In particular
    `01-domain-application.md:211` — `CORE-004`'s separate, differently-stale citation of the same
    line range — is untouched.
14. **D14.** `KindGuardRunsBeforeTheScaleRejection` asserts both `ParamName == "kind"` and
    `ActualValue == ObjectKind.None`, the second added before D3 deletes the test that currently
    carries it. Authorised by escalation E1, resolved at Gate 2 before the freeze.

## Scope

The five members named in `measurement.md` section 3; the 22 call sites in section 3's file table;
`CORE-019`'s entry body and `MASTER-047`'s index row.

### Not in scope

- **`ObjectScale` becoming a value object.** A deferred register item proposes it. Nothing here
  depends on which way it goes.
- **`CORE-004`'s stale citation** at `01-domain-application.md:211`. Its pre-pluralisation paths are
  its own subject; D13 pins it as must-not-change.
- **Renaming `FiveParameterConstructor_WithUndefinedKind_Throws`**, whose name encodes an arity
  distinction that stops existing once its four-parameter sibling is deleted. Stale, not false, and
  its call site is already arity 5. Filed as advisory.
- **Making the aggregate's defaulting symmetric.** `Dungeon` has a scale-less `AddPlacedObject` and no
  scale-less `UpdatePlacedObject`; this session removes the former rather than adding the latter, and
  B11 pins the asymmetry so nobody "fixes" it mid-session.
- **Enabling `GenerateDocumentationFile`.** Unset repo-wide, so XML doc markup is unchecked by the
  build. Recorded, not changed — turning it on surfaces CS1591 across every undocumented public
  member and nobody has measured that.
- **The 8 lone `CR` characters in `.dir-sdlc/backlog.md`.** They date to 6b-iv and are outside scope
  for the eighth session running.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Which commit anchors every citation in this session? | `b39a65d`, the tip of `chore/repo-audit` and the base of the session branch | 1 | `git rev-parse` in the main checkout; base and branch SHAs identical |
| Can a name grep answer "no production caller" here? | No | 1 | Calls split one argument per line, so arity is invisible to a single-line grep; and `MapHitTesterTests.cs:278` declares a local helper also named `PlacedObject` with 7 call sites |
| Does deleting the five dangle anything? | No — the cluster is closed | 1 | Each member's body only delegates inward to a live overload (`measurement.md` §4.1) |
| Is `ObjectScale.Default` reachable from the test project without a new grant? | Yes | 1 | `public const float` on a `public static class`; `ObjectScale.cs:8` |
| Does the deletion orphan `ObjectScale.Default`? | No | 1 | 10 production references survive the 3 removed, including both command records' default parameter values |
| Is `PlacedObject.cs:67`'s field initializer part of the dead cluster? | No — it must stay | 1 | The private parameterless ctor at `:10` is EF's materialization path and assigns nothing |
| Which test projects can see these members? | `DungeonMaster.Tests` only | 1 | `Domain/AssemblyInfo.cs:3` is Domain's sole `InternalsVisibleTo` grant; 5 such attributes exist across `src/`, none a second Domain grant |
| Which diagnostic fires if a call site is missed? | `CS7036`, at error severity, for both the method and the constructor case | 2 | Architect built a scratch project and compiled it; `CS1501` is what an unverified guess would have written |
| Does a missed call site fail the build rather than warn? | Yes | 1 | `TreatWarningsAsErrors` true, `Directory.Build.props:5` |
| Can the build diagnose malformed XML doc markup? | No | 1 | `GenerateDocumentationFile` unset repo-wide, so CS1574 cannot fire |
| Which grep form counts build diagnostics correctly? | `grep -cE "(warning\|error) [A-Z]+[0-9]+"` | 1 | The plain form always returns ≥2 over `dotnet build`, because MSBuild prints `0 Warning(s)` and `0 Error(s)` literally |
| Can `dotnet test`'s `Passed!` line identify the TFM that ran? | No — read the `Test run for …` line | 1 | It prints `(net10.0)` for all three assemblies including `App.Tests`, which ran from `net10.0-windows10.0.19041.0`; reproduced this session |
| Who writes `MASTER-047`'s commit hash? | The orchestrator, at Gate 3 | 1 | The register's own convention, visible in `MASTER-163` (`c519781`) and `MASTER-167` (`925fb55`); a task cannot invoke git |
| Does the repository hold a citation-checking script? | No | 1 | Architect searched; `MASTER-098`'s "134 citations machine-checked" was a one-off session artifact |

## Contracts

**None.** `manifest.json` records `"contracts": []`.

The only interface between the three tasks is the set of C# overload signatures T1 converts its call
sites onto, and those already exist at `b39a65d` — they are not something to author and freeze. There
is no new type and no new boundary, and T1 → T2 → T3 is a strictly serial chain over disjoint files,
so there is no contract operation that could reach an implementer without a consumer. The absence is
declared rather than implied; 6b-xii declared the same and recorded its reason the same way.

## Open questions

**None open at the freeze.** One was raised and resolved at Gate 2:

| Question | Resolution | Where it lives now |
|---|---|---|
| Escalation **E1** — deleting `FourParameterConstructor_WithNoneKind_Throws` removes the only assertion in the tree that the `ActualValue` for the `ObjectKind.None` case *is* `None`. `KindGuardRunsBeforeTheScaleRejection` asserts `ParamName` only; `FiveParameterConstructor_WithUndefinedKind_Throws` asserts `(ObjectKind)999`. May T1 add one assertion to a test outside the 22 approved call sites to preserve it? | **Yes** — add `.Which.ActualValue.Should().Be(ObjectKind.None);` to `KindGuardRunsBeforeTheScaleRejection`, then delete as planned. Resolved by the user at Gate 2, **before** the freeze, so it is an ordinary criterion rather than an amendment | D14 above; `manifest.json` T1 criterion **A8**, with A4 reconciled against it so the two do not compete |

A note on A8's check, because it is the one place in this session where the obvious command is
vacuous. The assertion is not being *added* to the tree — it is being **moved** from the deleted test
into the surviving one, so a flat `grep -c` over `ObjectKindGuardTests.cs` returns 1 both before and
after. A8 therefore scopes the count to the target method with `awk`, and that form was validated with
a positive control: 0 against `KindGuardRunsBeforeTheScaleRejection` (must not match yet) and 1
against `FourParameterConstructor_WithNoneKind_Throws` (must match), proving the command reads the
method body it names rather than the file.
