# Contract — `ILongPressTimer`, its dispatcher implementation, and its test double

**Producer:** T3. **Consumers:** T4 (`HoverOrLongPressBehavior`), T5 (`CanvasLongPressDetector`).
Frozen at Gate 2. An implementer needing a change here stops and escalates.

Covers `MASTER-062` (`UIKIT-017`)'s fix shape. `UIKIT-017`'s suggested direction offered two:
mark both fields `volatile`, or replace `System.Timers.Timer` with a dispatcher timer that ticks on
the UI thread. This contract takes the second, because the first is only assertable by reflecting
over a keyword — the structural evidence class the user rejected in Session 4 — while the second
removes the cross-thread read outright and is testable through an injected double.

**What this contract does not claim.** It does not demonstrate the memory-visibility race.
`UIKIT-017` itself says "I have not observed the misfire and cannot state it as fact", and spike §4
places the race at tier 3: no test can force a reordering. What is demonstrated is that after the
fix there is no cross-thread read left to reorder — the flag is written and read on one thread —
and that a released press cannot raise `LongPressed`.

---

## 1. The abstraction

Lives in `DungeonMaster.App.Core` (`net10.0`, no MAUI reference), so consumers can be tested
without a host.

```csharp
namespace DungeonMaster.App.Core.Timing;

/// <summary>
///     A one-shot delay whose <see cref="Elapsed"/> event is raised on the same thread that
///     called <see cref="Start"/>.
/// </summary>
public interface ILongPressTimer : IDisposable
{
	/// <summary>Raised at most once per <see cref="Start"/> call.</summary>
	event EventHandler? Elapsed;

	/// <summary>Starts, or restarts, the delay.</summary>
	void Start(TimeSpan delay);

	/// <summary>Cancels a running delay. A no-op if none is running.</summary>
	void Stop();
}
```

### Guarantees

- **One-shot.** `Elapsed` is raised at most once per `Start`. It does not auto-repeat.
- **Same-thread delivery.** `Elapsed` is raised on the thread that called `Start`. This is the
  property that replaces the `MainThread.BeginInvokeOnMainThread` hop and the cross-thread read of
  `_active` / `_touching`; consumers therefore must **not** re-marshal inside their handler.
- **`Stop()` before elapse means no `Elapsed`.** Once `Stop` returns on the owning thread, the
  pending tick will not be delivered.
- **Restart.** `Start` while running cancels the pending delay and begins a new one. Only the last
  `Start` can produce an `Elapsed`.
- **`Dispose()` implies `Stop()`**, is idempotent, and unsubscribes nothing on the caller's behalf.

### Error cases

| Situation | Surface |
|---|---|
| `Start` after `Dispose` | `ObjectDisposedException` |
| `Stop` after `Dispose` | No-op, no throw |
| `delay` negative | `ArgumentOutOfRangeException` naming `delay` |
| A subscriber's `Elapsed` handler throws | Propagates to the dispatcher/caller unchanged; the timer does not swallow it |

---

## 2. The production implementation

Lives in the App project because it needs `Microsoft.Maui.Dispatching`.

```csharp
namespace DungeonMaster.App.Behaviors;

/// <summary>
///     <see cref="ILongPressTimer"/> over <c>IDispatcher.CreateTimer()</c>. The tick arrives on the
///     UI thread, so the pressed flag its consumers guard is written and read on one thread.
/// </summary>
public sealed class DispatcherLongPressTimer : ILongPressTimer
{
	/// <exception cref="ArgumentNullException"><paramref name="dispatcher"/> is null.</exception>
	public DispatcherLongPressTimer(Microsoft.Maui.Dispatching.IDispatcher dispatcher);
}
```

`IsRepeating` is set false on the underlying `IDispatcherTimer`. `NotebookView` already uses
`Dispatcher.CreateTimer()` for its debounce, so the pattern is established in this layer
(`UIKIT-017`, Suggested direction).

**Construction requires a MAUI host** — an `IDispatcher` comes from a `VisualElement`, and spike §3
puts everything `Element`-derived behind the host. Any test of this class is therefore a host test.

---

## 3. The test double

T3 owns the file; T4 and T5 use it. Freezing it here is what lets all three write tests without
coordinating.

```csharp
namespace DungeonMaster.App.Tests.Behaviors;

/// <summary>
///     A hand-driven <see cref="ILongPressTimer"/>. Nothing here sleeps or waits: a test calls
///     <see cref="Tick"/> to make the delay elapse, on the calling thread.
/// </summary>
public sealed class FakeLongPressTimer : ILongPressTimer
{
	public bool IsRunning { get; }

	// LastDelay, StartCount and StopCount were removed at Gate 2: no task consumed
	// any of them, and an operation with an implementer and no caller is surface
	// nothing proves.

	public bool IsDisposed { get; }

	/// <summary>
	///     Raises <see cref="ILongPressTimer.Elapsed"/> on the calling thread if the timer is
	///     running, and clears <see cref="IsRunning"/>. A no-op if it is not running — which is how
	///     a test asserts that a cancelled press produced nothing.
	/// </summary>
	public void Tick();
}
```

Its `Start`/`Stop`/`Dispose` honour every guarantee and every error case in §1, including the
`ObjectDisposedException` on `Start` after `Dispose`. A consumer test may rely on `Tick()` after
`Stop()` raising nothing.
