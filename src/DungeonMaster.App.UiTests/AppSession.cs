using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DungeonMaster.App.UiTests;

/// <summary>
/// One Appium session against one freshly launched <c>DungeonMaster.App.exe</c>.
/// </summary>
/// <remarks>
/// <para>
/// Each test opens and disposes its own session, so no test can be handed a window another test
/// left in a state it did not create. That is more expensive than sharing one session and it is
/// what makes the tests order-independent.
/// </para>
/// <para>
/// Driver lifetime here is an ordinary <see cref="IDisposable"/> owned by the test method. This
/// project uses xunit, so there is no framework-scoped setup class holding a static driver.
/// </para>
/// </remarks>
internal sealed class AppSession : IDisposable
{
	/// <summary>How long a lookup waits for an element to appear before failing.</summary>
	internal static readonly TimeSpan ElementTimeout = TimeSpan.FromSeconds(15);

	private readonly WindowsDriver _driver;
	private bool _disposed;

	private AppSession(WindowsDriver driver) => _driver = driver;

	/// <summary>The session id the server issued. Non-null once the session is open.</summary>
	internal string SessionId =>
		_driver.SessionId?.ToString() ?? throw new InvalidOperationException("The driver has no session id.");

	/// <summary>The capabilities this session was opened with, for the run's own evidence.</summary>
	internal static IReadOnlyDictionary<string, object> Capabilities(string appExecutablePath) =>
		new Dictionary<string, object>(StringComparer.Ordinal)
		{
			["platformName"] = "Windows",
			["appium:automationName"] = "Windows",
			["appium:app"] = appExecutablePath,
		};

	/// <summary>
	/// Launches the app under Appium and returns the open session.
	/// </summary>
	/// <remarks>
	/// The capability spelling is the one the installed <c>appium-windows-driver@6.1.1</c>
	/// documents in its own README: <c>platformName</c> <c>windows</c>, <c>appium:automationName</c>
	/// <c>windows</c>, and <c>appium:app</c> set to the full path of a classic app. No AUMID form is
	/// used, because the app has no package identity.
	/// </remarks>
	internal static AppSession Open()
	{
		var options = new AppiumOptions
		{
			PlatformName = "Windows",
			AutomationName = "Windows",
			App = UiTestEnvironment.AppExecutablePath,
		};

		var driver = new WindowsDriver(UiTestEnvironment.ServerUri, options, TimeSpan.FromMinutes(3));
		driver.Manage().Timeouts().ImplicitWait = ElementTimeout;
		return new AppSession(driver);
	}

	/// <summary>
	/// Finds the element carrying <paramref name="automationId"/>, waiting on its appearance up to
	/// <see cref="ElementTimeout"/>. The wait is the driver's implicit wait — a condition wait on
	/// the server, not a fixed delay.
	/// </summary>
	internal AppiumElement WaitForElement(string automationId) =>
		(AppiumElement)_driver.FindElement(MobileBy.AccessibilityId(automationId));

	/// <summary>
	/// How many elements currently carry <paramref name="automationId"/>, with no waiting at all.
	/// </summary>
	/// <remarks>
	/// The implicit wait is dropped to zero for the duration and restored afterwards. Without that,
	/// asking whether something is ABSENT would block for the full timeout before answering, and
	/// the answer would be the same.
	/// </remarks>
	internal int CountElements(string automationId)
	{
		_driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
		try
		{
			return _driver.FindElements(MobileBy.AccessibilityId(automationId)).Count;
		}
		finally
		{
			_driver.Manage().Timeouts().ImplicitWait = ElementTimeout;
		}
	}

	/// <summary>
	/// How long a condition wait keeps re-reading before it gives up.
	/// </summary>
	internal static readonly TimeSpan ConditionTimeout = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Every element carrying <paramref name="automationId"/>, in tree order, waiting for the
	/// first one to appear.
	/// </summary>
	/// <remarks>
	/// The page's floor list is a templated BindableLayout over FloorList.Items, which builds one
	/// FloorSidebarItem per floor from its row template, so every id contract C2 section 1
	/// tables appears ONCE PER FLOOR in the UIA tree. Nothing in this project may assume a
	/// unique match; a card's own controls are reached positionally within the card's index.
	/// </remarks>
	internal IReadOnlyList<AppiumElement> FindAllElements(string automationId)
	{
		_ = WaitForElement(automationId);
		return _driver.FindElements(MobileBy.AccessibilityId(automationId)).Cast<AppiumElement>().ToArray();
	}

	/// <summary>Finds one element by XPath over the UIA tree.</summary>
	/// <remarks>
	/// Used for the sidebar's scroll viewport, which declares no AutomationId. It is NOT given
	/// one: contract C2 section 1 freezes the id table and this project adds none, so the
	/// viewport is reached as the floor card's parent and its ClassName is asserted at the point
	/// of use rather than trusted.
	/// </remarks>
	internal AppiumElement FindByXPath(string xpath) =>
		(AppiumElement)_driver.FindElement(By.XPath(xpath));

	/// <summary>
	/// The current UIA page source, re-read on a transient server failure until
	/// <see cref="ConditionTimeout"/> elapses.
	/// </summary>
	/// <remarks>
	/// The retry is not a wait for the UI to settle. A source read was MEASURED failing once
	/// mid-transition on 2026-09-14 while every read either side of it succeeded, so a single
	/// failed read must not be reported as a failed assertion.
	/// </remarks>
	internal string PageSource
	{
		get
		{
			var deadline = Stopwatch.StartNew();
			WebDriverException? last = null;
			do
			{
				try
				{
					return _driver.PageSource;
				}
				catch (WebDriverException exception)
				{
					last = exception;
					Thread.Sleep(TimeSpan.FromMilliseconds(250));
				}
			}
			while (deadline.Elapsed < ConditionTimeout);

			throw new InvalidOperationException(
				$"The driver would not return a page source within {ConditionTimeout}.", last);
		}
	}

	/// <summary>The AutomationId of the element that currently holds the keyboard focus.</summary>
	internal string FocusedAutomationId() => UiaFocus.FocusedAutomationId(PageSource);

	/// <summary>
	/// Presses Tab once, by sending the key to the application window element.
	/// </summary>
	/// <remarks>
	/// The key goes to an ELEMENT rather than through a W3C action chain because this
	/// server/driver pair rejects a key input source outright — measured 2026-09-14, see
	/// <see cref="UiaFocus"/>. The window element is the recipient because the key is meant for
	/// whatever holds focus, not for a particular control.
	/// </remarks>
	internal void PressTab() => FindByXPath("/*").SendKeys(Keys.Tab);

	/// <summary>
	/// Re-evaluates <paramref name="condition"/> until it holds or
	/// <see cref="ConditionTimeout"/> elapses; returns whether it came to hold.
	/// </summary>
	/// <remarks>
	/// This is what the no-sleeping rule asks for: the test waits on the condition it cares
	/// about with a bounded timeout, instead of guessing a duration the UI will have settled in.
	/// </remarks>
	internal static bool WaitUntil(Func<bool> condition)
	{
		ArgumentNullException.ThrowIfNull(condition);

		var deadline = Stopwatch.StartNew();
		do
		{
			if (condition())
			{
				return true;
			}

			Thread.Sleep(TimeSpan.FromMilliseconds(100));
		}
		while (deadline.Elapsed < ConditionTimeout);

		return condition();
	}

	/// <summary>A one-line description of this session, for console evidence.</summary>
	internal string Describe() => string.Create(
		CultureInfo.InvariantCulture,
		$"session={SessionId} app={UiTestEnvironment.AppExecutablePath} server={UiTestEnvironment.ServerUri}");

	/// <summary>Ends the session, which closes the application window it launched.</summary>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;
		try
		{
			_driver.Quit();
		}
		finally
		{
			_driver.Dispose();
		}
	}
}
