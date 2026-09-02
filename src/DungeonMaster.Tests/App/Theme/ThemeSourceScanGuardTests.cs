using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		One named guard per <c>ThemeSource</c> scan.
///		<para>
///		These assertions used to sit inside the scan helpers themselves, which meant a scan that
///		had stopped reaching anything was reported against whichever test called the helper
///		first, and no single test owned "the scan reaches files". Each guard here asserts
///		non-emptiness of exactly one scan and nothing else: a guard fact with a second reason to
///		fail is two facts.
///		</para>
/// </summary>
public class ThemeSourceScanGuardTests
{
	/// <summary>
	///		The dictionary file scan reaches at least one file.
	/// </summary>
	[Fact]
	public void TheThemeFileScanReachesAtLeastOneDictionary()
	{
		ThemeSource
			.ThemeFiles()
			.Should()
			.NotBeEmpty("the theme dictionary scan must find files or every fact built on it is vacuous");
	}

	/// <summary>
	///		The parsed-dictionary scan reaches at least one document.
	/// </summary>
	[Fact]
	public void TheThemeDocumentScanReachesAtLeastOneDictionary()
	{
		ThemeSource
			.ThemeDocuments()
			.Should()
			.NotBeEmpty("the parsed theme dictionary scan must find documents or every fact built on it is vacuous");
	}

	/// <summary>
	///		The application-wide XAML scan reaches at least one document.
	/// </summary>
	[Fact]
	public void TheAppDocumentScanReachesAtLeastOneXamlFile()
	{
		ThemeSource
			.AppDocuments()
			.Should()
			.NotBeEmpty("the application XAML scan must find documents or every fact built on it is vacuous");
	}

	/// <summary>
	///		The keyed-element scan reaches at least one declaration.
	/// </summary>
	[Fact]
	public void TheKeyedElementScanReachesAtLeastOneDeclaration()
	{
		ThemeSource
			.KeyedElements()
			.Should()
			.NotBeEmpty("the keyed-element scan must find declarations or every fact built on it is vacuous");
	}
}
