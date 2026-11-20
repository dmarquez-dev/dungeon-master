# Contract — MAUI application-host test fixture and the `MauiProgram` database seam

**Producer:** T1. **Consumers:** T4, T5.
Frozen at Gate 2. An implementer needing a change here stops and escalates.

Rests on `spike-host-report.md` §2c, §2d and §5. Every claim below that the spike measured is
marked *(measured)*; every claim it did not is marked *(unmeasured — implementer must establish)*.

---

## 1. Public surface

```csharp
namespace DungeonMaster.App.Tests.Hosting;

/// <summary>
///     Starts the real application entry point — <c>DungeonMaster.App.WinUI.App</c> —
///     on a background STA thread once per test process, and marshals work onto its
///     WinUI dispatcher. One host per process: <c>Microsoft.UI.Xaml.Application.Start</c>
///     can be called only once.
/// </summary>
public sealed class MauiHostFixture : IDisposable
{
	// DatabasePath and IsUp were removed at Gate 2: no task consumed either, and an
	// operation with an implementer and no caller is surface nothing proves.

	/// <summary>Runs <paramref name="work"/> on the host's UI dispatcher and waits for it.</summary>
	public void Invoke(Action work);

	/// <summary>Runs <paramref name="work"/> on the host's UI dispatcher and returns its result.</summary>
	public T Invoke<T>(Func<T> work);

	/// <summary>Runs an asynchronous <paramref name="work"/> on the host's UI dispatcher.</summary>
	public Task InvokeAsync(Func<Task> work);

	/// <summary>Runs an asynchronous <paramref name="work"/> on the host's UI dispatcher.</summary>
	public Task<T> InvokeAsync<T>(Func<Task<T>> work);

	/// <summary>Resolves a service from the running application's DI container.</summary>
	public TService Resolve<TService>() where TService : notnull;

	public void Dispose();
}

/// <summary>
///     The one xunit collection every host-requiring test class joins. Membership is what
///     serialises those classes against the single dispatcher.
/// </summary>
[CollectionDefinition(MauiHostCollection.Name)]
public sealed class MauiHostCollection : ICollectionFixture<MauiHostFixture>
{
	public const string Name = "maui-host";
}
```

Consumer usage is exactly:

```csharp
[Collection(MauiHostCollection.Name)]
public sealed class SomeHostTests(MauiHostFixture host)
{
	[Fact]
	public void Something() => host.Invoke(() => { /* MAUI types are constructible here */ });
}
```

### Guarantees

- **One host per process** *(measured — spike §5)*. The fixture is an `ICollectionFixture`, not a
  class fixture. A second construction attempt in the same process throws
  `InvalidOperationException`.
- **Serialisation.** Every `Invoke` / `InvokeAsync` call runs on the single WinUI dispatcher thread,
  one at a time, in call order. Consumers may assume no two `Invoke` bodies overlap.
  *(unmeasured — the spike did not measure concurrent dispatch; the fixture must serialise
  explicitly rather than rely on the dispatcher queue.)*
- **`Resolve<T>()`** reads `IPlatformApplication.Current.Services` *(measured — spike §2c, probe P22
  section C resolved `DungeonDetailPage`)*.
- **What is constructible inside `Invoke`** *(measured — spike §3)*: anything deriving from
  `Microsoft.Maui.Controls.Element` (`ContentView`, `ContentPage`, `DmCard`, gesture recognizers),
  `View.Behaviors` attach/detach, `PlatformImage.FromStream` on a real pack asset,
  `Application.Current.Resources`, `Application.Current.Windows[0].Page`.
- **What is NOT promised.** The fixture does **not** promise that `MapCanvas`, or any `GraphicsView`,
  acquires a real handler and native `FrameworkElement`. The spike explicitly could not settle this
  (§5) and no consumer criterion may depend on it.

### Error cases

| Situation | Surface |
|---|---|
| Host does not reach `Application.Current` within the startup timeout | `Invoke`/`InvokeAsync`/`Resolve` throw `InvalidOperationException` whose message names the timeout and includes the captured startup exception, if any, as `InnerException` |
| `work` throws on the dispatcher | The original exception is rethrown on the calling thread with its stack preserved (`ExceptionDispatchInfo.Capture(...).Throw()`). The exception type is not wrapped |
| `Invoke` called after `Dispose` | `ObjectDisposedException` |
| Second `MauiHostFixture` constructed in the same process | `InvalidOperationException` |

### Startup exception masking

The spike's working host set `app.UnhandledException += (_, e) => e.Handled = true`, without which
VSTest reports only `Test host process crashed` *(measured — spike §2c)*. Spike §5 records that the
masked `InvalidOperationException` was never identified. The fixture must **not** mask blanketly:
it records every unhandled exception it marks handled and exposes them, so a consumer failure is
diagnosable and the unidentified startup exception is visible rather than swallowed. The exact
exposure shape is T1's to choose; that it is exposed is the contract.

---

## 2. Build requirements the fixture owns

Both are properties of `src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj`, owned by T1.
The spike measured each necessary by removing it and watching the host die (§2d).

1. `<WindowsAppSdkBootstrapInitialize>true</WindowsAppSdkBootstrapInitialize>` — without it the
   Windows App SDK runtime is not loaded and the process crashes.
2. `$(OutDir)DungeonMaster.App.pri` copied to `$(OutDir)testhost.pri` **as a build step**, because
   WinUI resolves the app PRI by the process executable name and the process is `testhost.exe`.
   The three framework PRIs are *not* needed. The spike copied this by hand every run; it must
   become part of the build or no criterion resting on the host is checkable twice.

Off-the-documented-path note, carried from spike §5 so no consumer rediscovers it: Microsoft
documents an `OutputType=Exe` self-hosted WinUI test app under MTP and states "VSTest doesn't
support this unpackaged WinUI configuration"
(https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-winui#configure-an-unpackaged-test-app).
The spike measured that it does work under VSTest for this configuration. `testhost.pri` is brittle
against a change of runner executable name.

---

## 3. `MauiProgram` database-location seam

Scope accepted at escalation E2. Named by no audit entry.

```csharp
namespace DungeonMaster.App;

public static class MauiProgram
{
	/// <summary>
	///     Overrides the SQLite file the application migrates and opens. Set before the
	///     WinUI application starts; read once, inside <see cref="CreateMauiApp"/>.
	/// </summary>
	/// <remarks>
	///     A process-wide static rather than a parameter because the only call site is
	///     <c>MauiWinUIApplication.CreateMauiApp()</c>, which takes none. Single-writer:
	///     the test host fixture sets it before <c>Application.Start</c> and nothing
	///     writes it afterwards.
	/// </remarks>
	public static string? DatabasePathOverride { get; set; }

	public static MauiApp CreateMauiApp();
}
```

### Guarantees

- `DatabasePathOverride` null, empty, or whitespace → behaviour is byte-for-byte what it is today:
  `%LOCALAPPDATA%\DungeonMaster\dungeon-master.db`.
- Otherwise the connection string is `Data Source={DatabasePathOverride}` and the directory created
  at startup is `Path.GetDirectoryName(DatabasePathOverride)`.
- Read exactly once, at the point the connection string is built. Changing it after `CreateMauiApp`
  has returned has no effect.
- The value is not branched on an environment name and is not read from an environment variable —
  it is a declared value set by its one writer (backend discipline, Configuration).

### Error cases

- An unusable path surfaces through the **existing** `catch` in `CreateMauiApp`, which logs
  `Startup failed while creating the database folder` / `applying database migrations` and lets the
  app start. No new exception type is introduced and the existing filter list is not narrowed.
