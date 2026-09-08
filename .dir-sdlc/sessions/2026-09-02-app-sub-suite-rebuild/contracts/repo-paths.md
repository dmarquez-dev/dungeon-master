# Contract — `RepoPaths`

Producer: **T2**. Consumers: **T3, T4, T5, T6, T7, T8, T9**, and seven files no task in this session
owns — `App/AsyncDiscipline/*.cs` (4 of the 5) and `App/ViewModels/*.cs` (2) — which must keep
compiling and passing untouched.

File: `src/DungeonMaster.Tests/App/Theme/RepoPaths.cs`
Namespace: `DungeonMaster.Tests.App.Theme`

Closes `MASTER-008` / `TEST-003`.

## 1. Frozen surface — signatures may not change

```csharp
public static class RepoPaths
{
	public static string Root { get; }
	public static string AppRoot { get; }
	public static IReadOnlyList<string> AppSourceFiles(string searchPattern);
}
```

T2 changes the **body** of the private `FindRoot` and the **text** of its failure message. It changes
no signature, no member name, no accessibility, and adds no required member.

Measured in the worktree on 2026-09-02:

```
grep -rn "RepoPaths\." src/DungeonMaster.Tests/App --include=*.cs | wc -l   -> 37
grep -rl "RepoPaths\." src/DungeonMaster.Tests/App --include=*.cs | wc -l   -> 16
grep -rn "RepoPaths" src --include=*.cs | grep -v /obj/ | grep -v /bin/ | grep -v "src/DungeonMaster.Tests/App/"
                                                                           -> (no output)
grep -c "RepoPaths\." src/DungeonMaster.Tests/App/Theme/RepoPaths.cs        -> 0
```

**37 referencing lines across 16 files, none of them `RepoPaths.cs` itself**, and no reference
anywhere in `src` outside `src/DungeonMaster.Tests/App/`. (Twenty `.cs` files live under that
directory; the four that do not reference `RepoPaths` are `RepoPaths.cs`,
`AsyncDiscipline/AsyncDisciplineScanner.cs`, `Theme/ImplicitStyleTests.cs` and
`Theme/TokenNamingTests.cs`.) Any signature change desyncs all 37 lines at once.

## 2. Behavioural guarantees consumers may rely on

1. `Root` returns the absolute path of a directory that contains **both** a `.git` entry **and** a
   `src/DungeonMaster.App` directory. An ancestor satisfying only the first is skipped and the walk
   continues upward. This second condition is the whole of `TEST-003`.
2. The `.git` entry may be a **file or a directory**. A `.git` *file* marks a linked worktree — the
   layout this repository actively uses and the layout every task in this session runs in. The
   existing code handles it and comments why at `RepoPaths.cs:57-61`; T2 preserves that, and no
   change may narrow the test back to `Directory.Exists`.
3. `AppRoot` is `Path.Combine(Root, "src", "DungeonMaster.App")` and is guaranteed to exist whenever
   `Root` returned.
4. `AppSourceFiles(pattern)` returns absolute paths under `AppRoot` with `obj/` and `bin/` segments
   excluded, ordinal-ascending. Order is stable across calls within a run.

## 3. Error cases — the part consumers most often guess differently

| Condition | Surfaces as |
|---|---|
| No ancestor of `AppContext.BaseDirectory` satisfies **both** conditions in §2.1 | `InvalidOperationException` thrown from `FindRoot` through `Lazy<string>.Value` |
| The same, on any later access in the same process | The **same cached** `InvalidOperationException` instance. `Lazy<T>` caches the exception; this is existing behaviour and T2 preserves it. |
| `AppSourceFiles` called with a pattern matching nothing | Returns an **empty list**. It does not throw and it does not assert. Non-emptiness is the caller's guard — see `anti-vacuity.md`. |

The `InvalidOperationException` message must name, in this order: the directory the walk started from
(`AppContext.BaseDirectory`), and both conditions it was looking for. It must not name a directory
the walk never visited. The message is what a maintainer sees in a source archive or a tarball-fetch
CI job, which is one of the two layouts `TEST-003` says this defect breaks.

## 4. What consumers must not do

No consumer re-implements repository-root discovery. A file needing the repository root calls
`RepoPaths`. This is already true of all 16 files and stays true.

## 5. Testing it — the part `TEST-003` makes possible for the first time

`RepoPaths` has no test file today (`find src/DungeonMaster.Tests/App -name 'RepoPaths*'` returns
`RepoPaths.cs` only). T2 adds `src/DungeonMaster.Tests/App/Theme/RepoPathsTests.cs`.

The walk's decision procedure must be reachable from a test without depending on the machine's real
directory layout — that dependence is precisely what `TEST-003` objects to. T2 exposes the decision
as a pure, testable function taking the starting directory and a pair of existence predicates, or an
equivalent shape of its choosing, and `FindRoot` becomes the one caller that supplies the real
filesystem. The public surface in §1 does not change; whatever T2 adds for testability is `internal`
and is not part of this contract.

Three layouts `RepoPathsTests` must discriminate between, all constructible in a temporary directory
without a real clone:

1. An ancestor with a `.git` **directory** and `src/DungeonMaster.App` beneath it → accepted.
2. An ancestor with a `.git` **file** (the linked-worktree form) and `src/DungeonMaster.App` beneath
   it → accepted. This is the case the repository actually runs in.
3. An ancestor with a `.git` entry and **no** `src/DungeonMaster.App` beneath it, above which sits a
   directory satisfying both → the outer one is chosen, not the inner. **This is the case the current
   code gets wrong, and a test that does not distinguish it has not tested `TEST-003`.**

A fourth case establishes the error path: no ancestor satisfies both → `InvalidOperationException`
whose message contains the starting directory.
