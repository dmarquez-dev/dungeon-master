# Measurement — session 6b-x, `2026-09-18-dispatcher-test-gaps`

Every figure here was produced by the command quoted beside it, in this session, against the tree at
`506cbba` on `chore/repo-audit`, before any decomposition. Nothing is carried from the 6b-ix handoff.
Where a claim is a universal — *no test*, *every path*, *only* — the predicate that produced it is
validated against a case that must match and a case that must not, per the standing rule that a
universal is a count without a digit in it.

---

## 1. Repository state, re-derived

| Property | Command | Result |
|---|---|---|
| Session base ref | `git rev-parse chore/repo-audit` | `506cbba3bbd2847103b323e531108eed2a0740ba` |
| Remote parity | `git rev-list --left-right --count chore/repo-audit...origin/chore/repo-audit` | `0  0` |
| `main` | `git rev-parse main`; `git rev-list --left-right --count main...origin/main` | `45495c1bd39a52dbc942eceab897b3c1ac553ee9`, `0  0` |
| Working tree | `git status --porcelain=v1` | empty |
| Worktrees at session start | `git worktree list` | one — the main checkout |
| `dir/*` branches | `git branch --list 'dir/*' \| wc -l` / `git ls-remote --heads origin "dir/*" \| wc -l` | 13 local, 0 remote |
| `C:/dm-wt` | `ls -A /c/dm-wt` | empty |

**The 6b-ix handoff's section 1 states the tip as `8c2ac20`. It is `506cbba`.** The handoff is not
wrong — two commits landed after it was written, both of them corrections to the handoff itself
(`8c2ac20`, `506cbba`). Recorded because the handoff instructs its reader to re-derive rather than
cite it, and this is what re-deriving returned.

## 2. The master-index survey

Re-run with the command the handoff's section 6 supplies, unmodified:

```
total entry rows: 179
   126  closed
    50  open
     3  part done
```

Open by layer — CORE 16, UI 10, UIKIT 9, TEST 8, INFRA 6, `UI, XCUT` 1.
Open by severity — S3 8, S4 32, S5 10. **Zero rows return `UNCLASSIFIED`.** No open S1 and no open S2.

`b.count(b'\n')` on `docs/audit/00-master-index.md` in binary: **687**. `MASTER-172` is at `:360` and
`MASTER-173` at `:361`; both status cells are **empty**, which is what the survey classifies as open.

`.dir-sdlc/backlog.md`: **401** newlines and **8** lone CR characters, by
`b.count(b'\n')` and `b.count(b'\r') - b.count(b'\r\n')` in binary. Both figures match what 6b-ix
recorded. The 8 lone CRs date to 6b-iv and are again outside this session's approved scope.

---

## 3. `MASTER-172` — no test asserts `Dispatcher` propagates its `CancellationToken`

**Entry text (index `:360`):** *"No test asserts that Dispatcher propagates its CancellationToken to
the handler it resolves."* Filed by 6b-iii as a residual of `MASTER-020`'s closure, recorded at
`docs/audit/05-tests.md:317`, which states explicitly that neither `MASTER-020`'s closure nor
`MASTER-070`'s claims it.

**Verdict: the claim still holds at `506cbba`.**

### What the dispatch tests actually assert

`src/DungeonMaster.Tests/Application/Common/Dispatch/DispatcherTests.cs` is 402 lines and carries
**6** `[Fact]` (`grep -c "\[Fact\]"`):

| Line | Fact |
|---|---|
| `:17` | `SendAsync_WithTwoBehaviours_RunsThemFirstRegisteredOutermost` |
| `:56` | `SendAsyncOfT_WithTwoBehaviours_RunsThemInTheSameOrder` |
| `:96` | `QueryAsync_WithABehaviour_RunsItAroundTheHandler` |
| `:129` | `QueryAsync_WithNoBehaviour_ReturnsTheHandlerResult` |
| `:155` | `SendAsync_DispatchedTwice_ReflectsTypeOnlyOnce` |
| `:184` | `SendAsync_WithBothCommandArities_SharesOneInvokerCache` |

Behaviour ordering across the three request paths, and the memoised invoker cache. **Not one of the
six passes a token**: every `Act` block calls `SendAsync(new OrderingCommand())` or the query
equivalent with the argument omitted, taking the `CancellationToken ct = default` on `IDispatcher`.
Inside the file, `CancellationToken` appears only in the signatures of the test doubles
(`:256`, `:316`, `:330`, `:344`, `:358`, `:371`, `:381`, `:395`) — parameters those doubles accept
and none of them records.

### The census, and the predicate behind it

Across all three test projects — `DungeonMaster.Tests`, `DungeonMaster.App.Tests`,
`DungeonMaster.App.UnitTests`:

- `grep -rn "CancellationTokenSource" --include=*.cs` → **12** sites. None is in
  `Application/Common/Dispatch/`. They are in `ValidationBehaviorTests` (2),
  `CommandHandlerTestBase`, `DungeonNotFoundHandlerTestBase`, four feature handler test classes, and
  four Infrastructure repository/unit-of-work test classes.
- `grep -rn "new CancellationToken(" --include=*.cs` → **0**.

**Predicate validation — the case that must match.** This project has exactly one idiom for asserting
token propagation, and it is at `src/DungeonMaster.Tests/Application/Common/Handlers/CommandHandlerTestBase.cs:72`:
construct a `CancellationTokenSource`, capture `cts.Token`, invoke, then assert the receiving call got
**that same token** (`UnitOfWorkFactory.Received(1).CreateAsync(token)`). That test is found by the
`CancellationTokenSource` grep. So the search would have found a `Dispatcher` equivalent had one
existed; it is not a predicate that is blind to its own target.

**The case that must not match — checked, because it is the near-miss.**
`ValidationBehaviorTests.cs:222` and `:366` do assert token propagation, and they are two of the 12
hits. They call `behavior.HandleAsync(...)` **directly** on a constructed `ValidationBehavior`. They
never go through `IDispatcher`, so they say nothing about whether `Dispatcher` hands its own `ct`
onward. Counting them would have closed this entry on evidence that does not reach it.

### What is unasserted, precisely

`Dispatcher` has **three** request paths, and `ct` leaves each of them in **two** directions:

| Path | Handler hand-off | Pipeline hand-off |
|---|---|---|
| `SendAsync(ICommand)` | `Dispatcher.cs:139` | `:140` |
| `SendAsync<TResult>(ICommand<TResult>)` | `:172` | `:173` |
| `QueryAsync<TResult>` | `:205` | `:206` |

The pipeline hand-off reaches `BuildPipeline` (`:88`), which passes the same token to every registered
behaviour at `:105`. The entry's own wording names only "the handler it resolves"; the behaviour-chain
hand-off is adjacent and equally unasserted. **Which of these six the session covers is a
decomposition decision, not something the entry settles**, and it is put to the user at Gate 2 rather
than chosen here.

---

## 4. `MASTER-173` — the missing-handler failure mode is untested

**Entry text (index `:361`):** *"Dispatcher's missing-handler failure mode is untested."* Same filing,
same provenance.

**Verdict: the claim still holds at `506cbba` — with a near-miss that changes what it is worth.**

### The failure mode

Each of the three paths resolves its handler through `GetRequiredService`, at `Dispatcher.cs:131`,
`:164` and `:197` — three sites, one per path, by `grep -n "GetRequiredService"` on that file. With no
handler registered, `GetRequiredService` throws `InvalidOperationException`. Nothing in `Dispatcher`
catches it, so it surfaces from `SendAsync`/`QueryAsync` to the caller. No test observes this.

### The near-miss, stated because absorbing it would end the tracking of a real gap

`src/DungeonMaster.Tests/Application/Common/Dispatch/ApplicationServiceRegistrationTests.cs:46` is
named `ValidateHandlerRegistrations_WithAMissingHandler_ThrowsNamingTheCommand`. It is in the **same
folder**, it removes a handler descriptor, and it asserts `InvalidOperationException`. It is not this
entry:

- It calls `ApplicationServiceCollectionExtensions.ValidateHandlerRegistrations(provider)` — the
  **startup self-check** — not `IDispatcher`. `Dispatcher` is never constructed or invoked in it.
- The two mechanisms are independent. The self-check enumerates the Application assembly at startup
  and names the offending command; the Dispatcher's is a resolution failure at dispatch time.

`grep -rn "InvalidOperationException" --include=*.cs` across the three test projects returns **20**
sites; this is the only one inside `Application/Common/Dispatch/`, and it was read in full rather than
classified from its name.

### What the near-miss means for the entry's value

The self-check is the reason the Dispatcher's runtime failure is **unreachable for every real command
in the real container**: `ApplicationServiceRegistrationTests` proves at `:29` that the self-check
passes against the container the application actually builds, and at `:129`/`:169` that every command
and every query in the assembly resolves a handler. A test of the Dispatcher's own runtime behaviour
must therefore use a request type the self-check never scans — one declared in the test assembly.

**This does not falsify the entry.** `Dispatcher`'s documented contract is unproven either way, and a
later refactor that drops or weakens the self-check would leave nothing at all. But the gap is a
contract-coverage gap rather than a live defect, and this session should say so in the closure instead
of implying it closed a reachable failure. `MASTER-173` is rated S3; nothing measured here argues for
raising it.

---

## 5. Adjacent work found at rung 1, deliberately not folded in

`.dir-sdlc/backlog.md:149-157` carries a standing item from
`2026-09-11-measure-then-close-the-test-gaps`: file a numbered master-index entry for
`IUnitOfWorkFactory.CreateAsync`'s documented-but-unproven cancellation contract
(`src/DungeonMaster.Application/Common/Interfaces/IUnitOfWorkFactory.cs:17` documents
`<exception cref="OperationCanceledException">`; no test proves it). The backlog item itself says it
belongs *"alongside `MASTER-172`/`173`, since it is a coverage gap of the same shape as the dispatcher
residuals"*.

It is the same shape and it is adjacent. **It is not in the Gate 1 scope**, so it is recorded in the
spec's `### Not in scope` and put to the user as an explicit in-or-out decision at Gate 2, before the
manifest freezes — the framework's own mechanism for this — rather than absorbed quietly on the
grounds that it is nearby. Quiet absorption is exactly what `docs/audit/05-tests.md:317` warns against,
and it is how `MASTER-172` and `MASTER-173` came to exist in the first place.

---

## 6. What this measurement did not establish

- ~~**The suite baseline is not in this document.**~~ **Measured.** `dotnet test C:/dm-wt/6b-x/src/DungeonMaster.sln -c Debug`,
  run in the session worktree at `506cbba` before any code change, exit code 0:

  | Assembly | Failed | Passed |
  |---|---|---|
  | `DungeonMaster.Tests.dll` (net10.0) | 0 | 840 |
  | `DungeonMaster.App.Tests.dll` (net10.0) | 0 | 97 |
  | `DungeonMaster.App.UnitTests.dll` (net10.0) | 0 | 23 |
  | **Total** | **0** | **960** |

  `grep -icE "warning\|error"` over the full captured output returns **0** — no warning or error line
  of any kind, not merely a zero in a summary line. All three App TFMs appear in the output
  (`net10.0`, `net10.0-windows10.0.19041.0`, `net10.0-maccatalyst`), so the Windows TFM did build.
  This agrees exactly with the 960 the handoff carried; it is now measured rather than inherited.
- **Nothing here was measured on the Windows TFM.** The scope is `DungeonMaster.Tests`, which builds
  on the plain TFM. No claim is made about `net10.0-windows10.0.19041.0`.
- **No claim is made about analysers.** `CA2208` is escalated to an error in this solution
  (measured 6b-ix); whether any analyser has an opinion about an untested resolution failure was not
  probed and is not asserted.

---

## 7. The `GetRequiredService` message — measured, not believed

`dir-architect` recorded as a `judgment`-rung assumption that `GetRequiredService`'s
`InvalidOperationException` message contains the rendered service type, and stated that if it did not,
criteria A7–A9 would be **unsatisfiable as written**. Three of sixteen criteria resting on an
unconfirmed belief is worth one probe, so it was run rather than reasoned about.

A throwaway file-based app in scratch space (`C:\dm-wt\probe`, deleted after the run — no repository
file touched, no project added to the solution), pinned to the version this repository itself pins,
`Microsoft.Extensions.DependencyInjection` **10.0.11** per `Directory.Packages.props:24`:

```
TYPE: System.InvalidOperationException
MSG : No service for type 'ICommandHandler`1[FakeCommand]' has been registered.
TYPE: System.InvalidOperationException
MSG : No service for type 'IQueryHandler`2[FakeQuery,System.Int32]' has been registered.
```

The message is `No service for type '<Type.ToString()>' has been registered.`, and it contains
`ICommandHandler` and `IQueryHandler` as substrings. **A7–A9 are satisfiable as written.** The
assumption moves from rung 4 to measured evidence, and the implementer has no cause to escalate on it.

Run first against the 9.x line and then re-run against 10.0.11 specifically, because the two could
have differed and the first run would have been evidence about a version this repository does not use.

## 8. The A14 hash and the two citations behind A13 and A15, verified independently

- `sha256sum src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs` returns
  `e748d9ea13842984d24f60f9cbb224ee8e61eac8731998ab0fe0920035910375` in **both** the worktree and the
  main checkout — the same value the architect recorded in A14.
- `.editorconfig:8-9` reads `end_of_line = crlf` and `insert_final_newline = true`. A13's citation is
  correct.
- A15's check form was run against the existing file as a positive control:
  `grep -n -B1 "\[Fact\]" DispatcherTests.cs | grep -c "summary"` returns **6** against 6 `[Fact]`, so
  the form detects the convention it is asserting rather than passing vacuously.
