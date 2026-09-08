using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Dispatching;

using Xunit.Sdk;

namespace DungeonMaster.App.UnitTests.Fixtures;

/// <summary>
///		Ambient MAUI state a XAML-declared control needs in order to construct, with no MAUI host
///		and no WinUI. Constructed once per test collection.
/// </summary>
/// <remarks>
///		<para>
///		Two prerequisites, both measured. A constructed <see cref="DungeonMaster.App.App"/> merges
///		the seven theme dictionaries, without which <c>InitializeComponent</c> throws
///		<c>XamlParseException: StaticResource not found for key DmStrokeHairline</c>. A registered
///		dispatcher provider lets a control's attached behaviour reach
///		<c>DispatcherExtensions.FindDispatcher</c>, without which attachment throws
///		<c>InvalidOperationException</c>. Neither is supplied by anything else in the repository.
///		</para>
///		<para>
///		<c>Application.Current</c> and <c>DispatcherProvider.Current</c> are process-global mutable
///		statics, so every test class that touches a <c>Microsoft.Maui.Controls</c> type carries
///		<c>[Collection(AppTestCollection.Name)]</c> and shares this one instance.
///		</para>
/// </remarks>
public sealed class AppFixture : IDisposable
{
	private bool _disposed;

	/// <summary>
	///		Initializes the fixture: registers the dispatcher provider, then constructs the
	///		application.
	/// </summary>
	/// <remarks>
	///		The order is load-bearing. <c>new App()</c> runs <c>InitializeComponent</c>, which
	///		inflates <c>App.xaml</c> and can construct <c>BindableObject</c>s; those look for a
	///		dispatcher as they are created, so the provider is in place first.
	/// </remarks>
	public AppFixture()
	{
		DispatcherProvider.SetCurrent(new FakeDispatcherProvider());
		Application = new DungeonMaster.App.App();
	}

	/// <summary>
	///		The constructed <see cref="DungeonMaster.App.App"/>. Non-null for the fixture's whole
	///		lifetime. <c>Application.Current</c> is this instance while the fixture is alive.
	/// </summary>
	public Microsoft.Maui.Controls.Application Application { get; }

	/// <summary>
	///		Looks a merged theme resource up by key.
	/// </summary>
	/// <param name="key">The resource key, for example <c>DmStrokeHairline</c>.</param>
	/// <returns>
	///		The resource, or <see langword="null"/> when the key is not defined in the merged
	///		dictionaries.
	/// </returns>
	/// <remarks>
	///		Never throws for a missing key. Absence is a fact a consumer reports, not an exception:
	///		a test asking whether a token exists is asking a question, and a throw would make the
	///		negative answer indistinguishable from a broken fixture.
	/// </remarks>
	public object? FindResource(string key)
	{
		ArgumentNullException.ThrowIfNull(key);

		return Application.Resources.TryGetValue(key, out var value)
			? value
			: null;
	}

	/// <summary>
	///		Invokes <paramref name="factory"/> with the ambient state guaranteed ready.
	/// </summary>
	/// <typeparam name="T">The element type the factory produces.</typeparam>
	/// <param name="factory">The construction to run, for example <c>() =&gt; new DmCard()</c>.</param>
	/// <returns>Exactly what <paramref name="factory"/> returned.</returns>
	/// <exception cref="XunitException">
	///		The factory threw a <see cref="XamlParseException"/>. The message names the position and
	///		the key or element XAML could not resolve, and the original exception is the inner one.
	/// </exception>
	/// <remarks>
	///		The translation exists because a raw <see cref="XamlParseException"/> from inside a
	///		generated <c>InitializeComponent</c> reads as a defect in the control under test, when in
	///		practice it is almost always the fixture failing to supply a resource. Naming the fixture
	///		in the failure puts the reader in the right file. Any other exception passes through
	///		untouched — it is not this boundary's to explain.
	/// </remarks>
	public T Construct<T>(Func<T> factory)
		where T : Microsoft.Maui.Controls.Element
	{
		ArgumentNullException.ThrowIfNull(factory);

		try
		{
			return factory();
		}
		catch (XamlParseException exception)
		{
			throw new XunitException(
				$"Constructing {typeof(T).FullName} failed while XAML was being inflated: " +
				$"{exception.Message} " +
				"This is usually a resource the fixture's merged theme dictionaries do not carry, " +
				"not a defect in the element itself.",
				exception);
		}
	}

	/// <summary>
	///		Unregisters the dispatcher provider.
	/// </summary>
	/// <remarks>
	///		<c>Application.Current</c> is deliberately NOT cleared: its setter is internal to
	///		<c>Microsoft.Maui.Controls</c> and there is no public way to reset it. Nothing in this
	///		assembly may assume a process with no current application after disposal.
	/// </remarks>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		DispatcherProvider.SetCurrent(null);
		_disposed = true;
	}
}
