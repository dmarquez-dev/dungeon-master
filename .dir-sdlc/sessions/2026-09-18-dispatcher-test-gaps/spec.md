# Session 6b-x — Dispatcher test gaps

Close `MASTER-172` and `MASTER-173` by asserting, for the first time, that `Dispatcher` hands its
`CancellationToken` onward and that its missing-handler failure mode behaves as its callers assume.

## Context

`src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs` routes every command and query through a
behaviour pipeline to a handler resolved from the container. It has three request paths, implemented by
three private generic invoker classes that share no code on either axis this session tests.

Session 6b-iii closed `MASTER-020` and recorded two residuals rather than folding them into that closure
(`docs/audit/05-tests.md:317`). They became `MASTER-172` and `MASTER-173`, both `S3`, both `TEST` layer,
and neither has been re-measured since it was filed. **Both were re-measured in this session against
`506cbba` and both still hold** — see `measurement.md` §3 and §4. Five of the six preceding sessions
found an inherited claim wrong; this one did not, and that is worth recording rather than glossing.

The gap is real but its shape matters, and the closure text must say so: the startup self-check
(`ApplicationServiceCollectionExtensions.ValidateHandlerRegistrations`, proven by
`ApplicationServiceRegistrationTests.cs:29/:129/:169`) makes the missing-handler path **unreachable for
every real command in the real container**. `MASTER-173` is a contract-coverage gap, not a live defect.
Nothing measured argues for changing its `S3` rating.

This is a **tests-only** session. No production behaviour changes.

## Definition of done

1. A test asserts `Dispatcher` passes the caller's `CancellationToken` to the **handler** it resolves, on
   each of the three request paths — `SendAsync(ICommand)`, `SendAsync<TResult>`, `QueryAsync<TResult>`.
2. A test asserts the same token reaches the **behaviour pipeline** on each of those three paths.
3. A test observes `Dispatcher`'s missing-handler failure on each of those three paths, asserting an
   `InvalidOperationException` whose message names the handler service rather than merely asserting the
   exception type.
4. Each of the three defect classes above is **shown able to fail**, by a specified production injection
   that reddens exactly the facts covering it while a named set of sibling facts and all six pre-existing
   `DispatcherTests` facts stay green.
5. `Dispatcher.cs` is byte-identical to its `startRef` content when the task finishes —
   `sha256sum` reports `e748d9ea13842984d24f60f9cbb224ee8e61eac8731998ab0fe0920035910375`.
6. Both new files are CRLF throughout with a final newline, verified in binary, and every `[Fact]` and
   type in them carries an XML doc comment, matching what `DispatcherTests.cs` already does.
7. `dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` reports `Failed: 0` and exits 0 with
   the new files in place and `Dispatcher.cs` restored.
8. The full solution suite is green and warning-free, verified **in the main checkout after the merge**,
   not only in the worktree.
9. `MASTER-172` and `MASTER-173` are closed in `docs/audit/00-master-index.md` with commit evidence, and
   the residual note at `docs/audit/05-tests.md:317` records what this session closed and what it did
   not — including that `MASTER-173` was a contract-coverage gap rather than a reachable defect.

Items 8 and 9 **cannot be performed by any task**: tasks cannot invoke git, so a commit hash cannot reach
an index cell from inside one, and the post-merge main-checkout run happens after every task is done.
They are orchestrator work, and `coverage.md` records them as such rather than as covered.

## Scope

`src/DungeonMaster.Tests/Application/Common/Dispatch/` — two new test files, one per concern.
`src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs` is owned by the task **solely** so the
temporary injections are legal; it is restored byte-for-byte, and item 5 is the guard on that.

Post-merge, the orchestrator edits `docs/audit/00-master-index.md` and `docs/audit/05-tests.md`.

### Not in scope

- **Any permanent change to production code.** If either entry turns out to need one, that is a finding
  and an escalation, not a quiet task.
- **`.dir-sdlc/backlog.md:149-157`** — filing a master-index entry for `IUnitOfWorkFactory.CreateAsync`'s
  documented-but-unproven `OperationCanceledException` contract. The backlog item itself says it belongs
  *"alongside `MASTER-172`/`173`"*, and it is the same shape. It is adjacent, not approved, and it is put
  to the user at Gate 2 as an explicit in-or-out decision rather than absorbed on grounds of proximity —
  quiet absorption of an adjacent gap is precisely what created these two entries.
- **End-to-end cancellation semantics** — dispatching an already-cancelled token and expecting
  `OperationCanceledException`. That asserts a handler's cooperation, not `Dispatcher`'s hand-off, and its
  verdict would depend on a test double rather than on the code under test.
- **The 8 lone CR characters in `.dir-sdlc/backlog.md`**, outside approved scope for the fifth session.
- **The Windows TFM.** The scope builds on the plain TFM and this session deliberately stays off
  `net10.0-windows10.0.19041.0`.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Are `MASTER-172` and `MASTER-173` still accurate? | Yes, both | 1 | `measurement.md` §3, §4 — `DispatcherTests.cs` has 6 `[Fact]`, none passing a token; 12 `CancellationTokenSource` sites across the three test projects, none in `Application/Common/Dispatch/`; the one `InvalidOperationException` hit in that folder is the startup self-check |
| Does `GetRequiredService`'s message contain the handler interface name, so A7–A9 are satisfiable? | Yes | 1 | `measurement.md` §7 — probed against `Microsoft.Extensions.DependencyInjection` **10.0.11**, the version `Directory.Packages.props:24` pins: `No service for type 'ICommandHandler`1[FakeCommand]' has been registered.` |
| Is `e748d9e…` the right `startRef` hash for `Dispatcher.cs`? | Yes | 1 | `measurement.md` §8 — `sha256sum` agrees in the worktree and the main checkout |
| Two new files, or extend `DispatcherTests.cs`? | Two new files, one per concern | 1 | That folder already splits by concern (`DispatcherTests.cs` / `ApplicationServiceRegistrationTests.cs`); it also leaves the existing 6 facts untouched and avoids rewriting the class summary at `DispatcherTests.cs:9`, which accurately describes only what is there now |
| Must every `[Fact]` carry an XML doc comment? | Yes, by convention | 1 | All 6 existing `[Fact]` and all 11 nested types in `DispatcherTests.cs` carry one. **Not build-enforced** — `GenerateDocumentationFile` is absent from `Directory.Build.props`, so `CS1591` does not fire; hence criterion A15 rather than reliance on the build |
| Is `CancellationTokenSource` mandated for producing the test token? | No — cited, not mandated | 1 | `CommandHandlerTestBase.cs:72` is the project's one propagation idiom; the criteria state the outcome and cite it, because mandating a mechanism is a proxy for the property |
| Will the injections be caught by the build instead of by the tests? | No, as specified | 1 | `new CancellationToken(false)` is neither `default` nor `CancellationToken.None`, so `CA2016` cannot see it; `GetService<…>()!` carries the null-forgiving operator `CS8602` would otherwise demand. No `ParamName` is involved in any round, so the `CA2208`-as-error trap measured in 6b-ix is not in play |
| Can the session be parallelised? | No — one task | 1 | One worktree, one `obj/`/`bin/`, one test project, one subject file. Two tasks would need `dependsOn` for build serialisation and deliver zero concurrency while doubling the ownership surface |

## Contracts

**None.** This is a tests-only session against an existing, unchanged production interface, and with a
single task there is no inter-task boundary to freeze. `contracts/` is empty and `manifest.json` carries
`"contracts": []`. A contract invented here would be an artifact with an implementer and no consumer —
the exact failure the coverage mapping's second table exists to catch.

## Open questions

| Question | Who resolves it | When |
|---|---|---|
| Cover all nine facts, or the six the entries' literal wording names? | User | Gate 2 |
| Does the `IUnitOfWorkFactory.CreateAsync` backlog item join this session? | User | Gate 2 |
| Who performs definition-of-done items 8 and 9? | User confirms; orchestrator performs | Gate 2, executed post-merge |
