using System;
using System.Text.RegularExpressions;

namespace DungeonMaster.App.UiTests;

/// <summary>
/// Reads which element holds the keyboard focus out of a UIA page source document.
/// </summary>
/// <remarks>
/// <para>
/// This exists because neither W3C key actions nor the active-element endpoint survive the
/// Appium 3.7.0 to WinAppDriver 1.2.200902003 proxy, MEASURED on 2026-09-14 against this pair:
/// <c>POST /session/{id}/actions</c> with a <c>key</c> input source answers
/// <c>unsupported operation: Currently key input source type is not supported</c>, and both
/// <c>GET</c> and <c>POST</c> on <c>/session/{id}/element/active</c> answer 405 and 404. Sending
/// the key to an element and then reading focus out of the page source is what does work.
/// </para>
/// <para>
/// THE LAST MATCH IS THE FOCUSED ONE, and that is not a detail. The host chain reports
/// <c>HasKeyboardFocus="True"</c> on the <c>InputSiteWindowClass</c> pane AS WELL AS on the real
/// control inside it, so a reader that takes the FIRST match reports a featureless pane forever
/// and a tab-order walk built on it looks broken when it is fine. That was observed before this
/// reader was written the other way round.
/// </para>
/// </remarks>
internal static partial class UiaFocus
{
	/// <summary>Reported for a focused node that carries no AutomationId of its own.</summary>
	internal const string UnnamedFocus = "<unnamed>";

	/// <summary>Reported when no node in the document claims the keyboard focus.</summary>
	internal const string NoFocus = "<none>";

	/// <summary>
	/// The <c>AutomationId</c> of the deepest node flagged <c>HasKeyboardFocus="True"</c>.
	/// </summary>
	/// <param name="pageSource">A UIA page source document, as the driver returns it.</param>
	/// <returns>
	/// The focused node's <c>AutomationId</c>; <see cref="UnnamedFocus"/> with its class name when
	/// the focused node declares no id; <see cref="NoFocus"/> when nothing claims focus.
	/// </returns>
	internal static string FocusedAutomationId(string pageSource)
	{
		ArgumentNullException.ThrowIfNull(pageSource);

		var focused = NoFocus;
		foreach (var element in ElementTag().Matches(pageSource).AsEnumerable())
		{
			var tag = element.Value;
			if (!tag.Contains("HasKeyboardFocus=\"True\"", StringComparison.Ordinal))
			{
				continue;
			}

			var id = Attribute(tag, AutomationIdAttribute());
			focused = id.Length > 0
				? id
				: string.Concat(UnnamedFocus, ":", Attribute(tag, ClassNameAttribute()));
		}

		return focused;
	}

	private static string Attribute(string tag, Regex attribute)
	{
		var match = attribute.Match(tag);
		return match.Success ? match.Groups[1].Value : string.Empty;
	}

	[GeneratedRegex(@"<[A-Za-z]+\s[^>]*?>", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
	private static partial Regex ElementTag();

	[GeneratedRegex(@"\sAutomationId=""([^""]*)""", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
	private static partial Regex AutomationIdAttribute();

	[GeneratedRegex(@"\sClassName=""([^""]*)""", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
	private static partial Regex ClassNameAttribute();
}
