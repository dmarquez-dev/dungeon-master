using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

using Microsoft.Extensions.DependencyInjection;

namespace DungeonMaster.App.Tests.Hosting;

/// <summary>
///		Starts the real application entry point - <c>DungeonMaster.App.WinUI.App</c> - on a
///		background STA thread once per test process, and marshals work onto its WinUI dispatcher.
///		One host per process: <c>Microsoft.UI.Xaml.Application.Start</c> can be called only once.
/// </summary>
/// <remarks>
///		<para>
///			Three things are required and each was measured necessary by removing it and watching
///			the host die (spike-host-report.md, sections 2b and 2d):
///		</para>
///		<list type="number">
///			<item>
///				<description>
///					<c>&lt;WindowsAppSdkBootstrapInitialize&gt;true&lt;/WindowsAppSdkBootstrapInitialize&gt;</c>
///					in DungeonMaster.App.Tests.csproj, without which the Windows App SDK runtime is
///					never loaded into testhost.exe.
///				</description>
///			</item>
///			<item>
///				<description>
///					A <c>testhost.pri</c> beside testhost.exe, copied from DungeonMaster.App.pri by
///					the <c>CopyAppPriForTestHost</c> target in that same csproj. WinUI resolves the
///					application PRI by the process executable name.
///				</description>
///			</item>
///			<item>
///				<description>
///					A dedicated background STA thread. The xunit test thread is MTA, and
///					<c>Microsoft.Maui.Controls.Element</c>'s static constructor fails on it with
///					<c>RPC_E_WRONG_THREAD</c>.
///				</description>
///			</item>
///		</list>
///		<para>
///			The host runs <c>MauiProgram.CreateMauiApp()</c>, which applies EF migrations. The
///			constructor points <see cref="MauiProgram.DatabasePathOverride"/> at a fresh file under
///			<see cref="Path.GetTempPath"/> before the WinUI application starts, so no test run
///			touches the user's real database.
///		</para>
/// </remarks>
public sealed class MauiHostFixture : IDisposable
{
	private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(60);
	private static readonly TimeSpan WorkTimeout = TimeSpan.FromSeconds(30);

	/// <summary>
	///		Zero until the one permitted host has been constructed. <c>Microsoft.UI.Xaml.Application.Start</c>
	///		may be called once per process, so this is never reset - not even by <see cref="Dispose"/>.
	/// </summary>
	private static int _constructed;

	private readonly ConcurrentQueue<Exception> _handledExceptions = new();
	private readonly SemaphoreSlim _dispatchGate = new(1, 1);
	private readonly Microsoft.Maui.Controls.Application? _application;
	private readonly Exception? _startupFailure;
	private readonly string _databaseDirectory;
	private bool _disposed;

	/// <summary>
	///		Starts the host and blocks until <c>Microsoft.Maui.Controls.Application.Current</c> is
	///		available or <see cref="StartupTimeout"/> elapses. A startup that does not complete is
	///		reported from <see cref="Invoke(Action)"/> and <see cref="Resolve{TService}"/> rather
	///		than from here, so the failure reaches the test that needed the host.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	///		A second fixture is constructed in the same process.
	/// </exception>
	public MauiHostFixture()
	{
		if (Interlocked.Exchange(ref _constructed, 1) is not 0)
		{
			throw new InvalidOperationException(
				"A MauiHostFixture already exists in this process. Microsoft.UI.Xaml.Application.Start "
				+ "can be called only once, so every host-requiring test class must join "
				+ $"[Collection(\"{MauiHostCollection.Name}\")] and share this one fixture.");
		}

		// Set BEFORE the WinUI application starts: MauiProgram reads it while building the
		// connection string, which happens on the host thread moments from now.
		_databaseDirectory = Path.Combine(
			Path.GetTempPath(),
			"DungeonMaster.App.Tests",
			Guid.NewGuid().ToString("N"));

		MauiProgram.DatabasePathOverride = Path.Combine(_databaseDirectory, "dungeon-master.db");

		var uiThread = new Thread(RunHost)
		{
			IsBackground = true,
			Name = "MAUI host",
		};

		uiThread.SetApartmentState(ApartmentState.STA);
		uiThread.Start();

		var deadline = DateTime.UtcNow + StartupTimeout;

		while (Microsoft.Maui.Controls.Application.Current is null && DateTime.UtcNow < deadline)
		{
			Thread.Sleep(50);
		}

		_application = Microsoft.Maui.Controls.Application.Current;

		if (_application is null)
		{
			_startupFailure = _handledExceptions.FirstOrDefault();
		}
	}

	/// <summary>
	///		Every exception this fixture marked <c>Handled</c> on the WinUI dispatcher, in the order
	///		they were raised, startup included.
	/// </summary>
	/// <remarks>
	///		<para>
	///			The spike that established this host recipe found that an exception raised on the
	///			WinUI dispatcher during startup fail-fasted the process, leaving VSTest reporting
	///			nothing but "Test host process crashed", and that marking it handled was what kept
	///			the host alive (spike-host-report.md, sections 2c and 5; the exception was never
	///			identified there).
	///		</para>
	///		<para>
	///			Measured on 2026-09-01 in this configuration: nothing was raised. This collection was
	///			empty after a full run, and removing <c>args.Handled = true</c> altogether left all
	///			four host tests passing. The mask is retained as a safety net rather than as
	///			something observed to be load-bearing, and every exception it swallows lands here so
	///			the mask can never be blanket: a startup regression is readable instead of silent.
	///		</para>
	///		<para>
	///			This member is an addition to section 1 of contracts/maui-host-fixture.md, made under
	///			that contract's "Startup exception masking" clause, which requires the exposure and
	///			leaves its shape to this task.
	///		</para>
	/// </remarks>
	public IReadOnlyList<Exception> HandledExceptions => [.. _handledExceptions];

	/// <summary>Runs <paramref name="work"/> on the host's UI dispatcher and waits for it.</summary>
	public void Invoke(Action work)
	{
		ArgumentNullException.ThrowIfNull(work);

		Invoke(() =>
		{
			work();

			return true;
		});
	}

	/// <summary>Runs <paramref name="work"/> on the host's UI dispatcher and returns its result.</summary>
	[SuppressMessage(
		"Design",
		"CA1031:Do not catch general exception types",
		Justification = "The exception is captured and rethrown on the calling thread with its stack intact.")]
	public T Invoke<T>(Func<T> work)
	{
		ArgumentNullException.ThrowIfNull(work);

		var application = EnsureUp();

		// Explicit serialisation. The contract lets consumers assume no two Invoke bodies overlap,
		// and the spike did not measure what concurrent dispatch onto the one dispatcher does.
		_dispatchGate.Wait();

		try
		{
			var result = default(T)!;
			ExceptionDispatchInfo? failure = null;
			using var done = new ManualResetEventSlim(false);

			application.Dispatcher.Dispatch(() =>
			{
				try
				{
					result = work();
				}
				catch (Exception exception)
				{
					failure = ExceptionDispatchInfo.Capture(exception);
				}
				finally
				{
					done.Set();
				}
			});

			if (!done.Wait(WorkTimeout))
			{
				throw new TimeoutException(
					$"The work dispatched to the MAUI host did not complete within {WorkTimeout}.");
			}

			failure?.Throw();

			return result;
		}
		finally
		{
			_dispatchGate.Release();
		}
	}

	/// <summary>Resolves a service from the running application's DI container.</summary>
	/// <remarks>
	///		Anything deriving from <c>Microsoft.Maui.Controls.Element</c> must be resolved from
	///		inside <see cref="Invoke(Action)"/>: this method does not marshal, it only reads the
	///		container.
	/// </remarks>
	public TService Resolve<TService>()
		where TService : notnull
	{
		EnsureUp();

		var services = IPlatformApplication.Current?.Services
			?? throw new InvalidOperationException(
				"The MAUI host is up but IPlatformApplication.Current.Services is null.");

		return services.GetRequiredService<TService>();
	}

	/// <inheritdoc/>
	[SuppressMessage(
		"Design",
		"CA1031:Do not catch general exception types",
		Justification = "Best-effort cleanup of a temporary directory must not fail a test run.")]
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;
		_dispatchGate.Dispose();

		// Application.Exit() is deliberately NOT called: the UI thread is a background thread, so
		// the process exits with the message loop still running, and exiting the application here
		// takes the process down before the test runner reports.
		try
		{
			if (Directory.Exists(_databaseDirectory))
			{
				Directory.Delete(_databaseDirectory, recursive: true);
			}
		}
		catch (Exception)
		{
			// The temporary database may still be held open by SQLite. It is under the system
			// temporary directory and costs nothing to leave behind.
		}
	}

	private void RunHost()
	{
		Microsoft.UI.Xaml.Application.Start(_ =>
		{
			var app = new DungeonMaster.App.WinUI.App();

			app.UnhandledException += (_, args) =>
			{
				_handledExceptions.Enqueue(
					args.Exception
					?? new InvalidOperationException(args.Message));

				args.Handled = true;
			};
		});
	}

	private Microsoft.Maui.Controls.Application EnsureUp()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		return _application
			?? throw new InvalidOperationException(
				$"The MAUI application host did not reach Application.Current within {StartupTimeout}.",
				_startupFailure);
	}
}
