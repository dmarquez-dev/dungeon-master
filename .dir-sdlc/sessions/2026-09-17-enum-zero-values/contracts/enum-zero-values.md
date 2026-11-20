# Contract — enum zero values (session 6b-ix)

Frozen at Gate 2. Declarations only. No implementation, no worked sketch.

Scope of this contract: the two Class A enums the session changes in code
(`MutationOutcome`, `DrawSessionResult`) and the one Application-layer mapping whose
observable behaviour changes as a result. The twelve enums in Class B and Class C are
**not** changed by any task in this session and are therefore not declared here; they
appear only in the audit prose T3 writes.

---

## C1 — `DungeonMaster.Domain.Entities.MutationOutcome`

Producer: T1. Consumers: `MutationOutcomeExtensions.ToResult` (T1), the audit prose (T3).

Post-change declaration. Member order is normative; no member carries an explicit `= n`.

```csharp
namespace DungeonMaster.Domain.Entities;

public enum MutationOutcome
{
    None,        // 0 — no outcome was reported. Never returned by any aggregate mutation.
    Success,     // 1
    NotFound,    // 2
    OutOfRange,  // 3
}
```

Normative consequences:

- `(int)MutationOutcome.None == 0` and `default(MutationOutcome) == MutationOutcome.None`.
- `(int)MutationOutcome.Success == 1`, `(int)MutationOutcome.NotFound == 2`,
  `(int)MutationOutcome.OutOfRange == 3`. The three pre-existing members keep their
  declaration order relative to one another.
- `Enum.IsDefined(MutationOutcome.None)` is `true`. An `IsDefined`-only guard therefore
  admits it, which is why the rejection in C3 is stated as a member-specific rejection and
  not as a definedness check. (Same mechanism recorded for `ObjectKind.None` by 6b-viii in
  `ObjectKindGuardTests.NoneIsADefinedMember_WhichIsWhyTheGuardNeedsItsOwnNoneCondition`.)

No numeric value of this enum is persisted or serialised anywhere. Measured at
`80beda93a689e47ed899d64fe7ad41fb8e99a677` by
`grep -rln "MutationOutcome" src/DungeonMaster.Infrastructure/` (no files) and
`grep -rn "(MutationOutcome)\|GetValues<MutationOutcome\|typeof(MutationOutcome)" --include=*.cs src/`
(one hit, `MutationOutcomeExtensionsTests.cs:109`, which casts the literal `99`).
Renumbering is therefore free for this enum. It is **not** free for `PathSegmentKind`; see
the note at the end of this file.

---

## C2 — `DungeonMaster.App.Core.Map.DrawSessionResult`

Producer: T2. Consumers: `MapDrawingSession.PlaceVertex` (T2), the audit prose (T3), and
the two equality sites in `DungeonDetailPage.xaml.cs` (`:1473`, `:1489`), which **no task in
this session modifies**.

Post-change declaration. Member order is normative; no member carries an explicit `= n`.

```csharp
namespace DungeonMaster.App.Core.Map;

public enum DrawSessionResult
{
    None,     // 0 — no placement attempt was made. Never returned by PlaceVertex.
    Added,    // 1
    Closed,   // 2
    Ignored,  // 3
}
```

Normative consequences:

- `(int)DrawSessionResult.None == 0` and `default(DrawSessionResult) == DrawSessionResult.None`.
- `(int)Added == 1`, `(int)Closed == 2`, `(int)Ignored == 3`.
- `MapDrawingSession.PlaceVertex` returns one of `Added`, `Closed`, `Ignored` on every path
  and never returns `None`. Three explicit `return` statements, at `MapDrawingSession.cs:65`,
  `:70` and `:74` as of `80beda9`, with no fall-through path.
- The two consumer sites compare for equality with `Added` and with `Closed` and are left
  unchanged, so a value of `None` reaching them takes neither branch and no vertex is
  committed. **This is the whole behavioural effect of C2.** Adding an explicit rejection at
  those sites is deliberately out of scope; see T3's audit entry.

No numeric value of this enum is persisted or serialised. Measured at `80beda9` by
`grep -rn "DrawSessionResult" --include=*.cs --include=*.xaml src/`, which returns seven hits
outside the declaring file: two equality comparisons in `DungeonDetailPage.xaml.cs` and five
assertions in `MapDrawingSessionTests.cs`.

---

## C3 — `MutationOutcomeExtensions.ToResult` — behaviour at the zero member

Producer: T1. Callers: the existing Application command handlers, unchanged by this session.

Signatures are unchanged:

```csharp
public static Result ToResult(this MutationOutcome outcome, string notFoundMessage);
public static Result ToResult(this MutationOutcome outcome, string notFoundMessage, string outOfRangeMessage);
```

Behaviour, normative:

| Input | Result |
|---|---|
| `MutationOutcome.None` (i.e. `default(MutationOutcome)`) | throws `ArgumentOutOfRangeException` with `ParamName == "outcome"` and `ActualValue == MutationOutcome.None` |
| `MutationOutcome.Success` | `Result.Success()` — unchanged |
| `MutationOutcome.NotFound` | `Result.Failure(notFoundMessage, ErrorCategory.NotFound)` — unchanged |
| `MutationOutcome.OutOfRange` | `Result.Failure(outOfRangeMessage, ErrorCategory.Validation)` — unchanged |
| any value outside the declared members, e.g. `(MutationOutcome)99` | throws `ArgumentOutOfRangeException` with `ParamName == "outcome"` and `ActualValue` equal to that value — unchanged in kind |

The **zero-member rejection and the undefined-value rejection are two separate covering
facts**, and the contract requires them to be separately removable. Whether they are
implemented as two switch arms or as one is not constrained; what is constrained is that
removing the zero-member rejection alone leaves the undefined-value rejection working, and
removing the undefined-value rejection alone leaves the zero-member rejection working. T1's
acceptance criteria state the two injections that establish this.

`ParamName` and `ActualValue` are both normative. `ArgumentOutOfRangeException` alone is not
a sufficient assertion here: `Result.Failure` and the surrounding handlers can raise the same
exception type from a level below, so an assertion on the type alone cannot distinguish this
guard from a downstream rejection.

The XML `<exception>` documentation on both overloads currently reads "Thrown when the outcome
is not a recognised `MutationOutcome` member." After this change that wording is false for the
`None` case, since `None` **is** a recognised member. T1 owns correcting it.

---

## Note — why `PathSegmentKind` is not in this contract

`PathSegmentKind` is the fourth Class B enum and is the one case where inserting a zero member
is not a free renumbering. `AreaConfiguration.cs:16` builds its serializer options as
`new JsonSerializerOptions(JsonSerializerDefaults.General)`, and
`grep -rn "JsonStringEnumConverter\|JsonConverter" --include=*.cs src/` returns **zero hits**
across the whole of `src/` at `80beda9`. `System.Text.Json` therefore writes the enum as its
**ordinal** into the `Areas.Outline` TEXT column, and `PathSegmentJsonRoundTripTests.cs:26-28`
pins that persisted form: `"Kind": 0` deserialises to `Line`, `"Kind": 1` to `CubicBezier`.

Inserting `None = 0` would reinterpret every already-persisted outline — `Line` becomes `None`,
`CubicBezier` becomes `Line` — which is a data migration on a JSON column, not a renumbering.
This is the measured counter-example to the generalisation left standing in `CORE-017`'s 6b-viii
amendment, which established that renumbering was free for `ObjectKind` because that enum maps
with `.HasConversion<string>()`. It is free there and it is not free here. No task in this
session changes `PathSegmentKind`.
