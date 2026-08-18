using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Shared scanning primitives for the theme facts.
///		The patterns and file scans here were duplicated across five test classes; a divergence
///		between two copies is invisible until one of them silently stops matching, which is the
///		failure mode this whole suite exists to prevent.
/// </summary>
internal static class ThemeSource
{
	/// <summary>
	///		Matches a 6- or 8-digit hex colour literal.
	/// </summary>
	public static readonly Regex HexLiteralPattern =
		new(
			@"#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?",
			RegexOptions.Compiled);

	/// <summary>
	///		Matches the declaration of a current, <c>Dm</c>-prefixed resource key.
	///		Use this everywhere except the legacy ratchet.
	/// </summary>
	public static readonly Regex KeyDefinitionPattern =
		new(
			@"x:Key=""(Dm[A-Za-z0-9]*)""",
			RegexOptions.Compiled);

	/// <summary>
	///		Matches the declaration of a resource key under EITHER casing of the prefix.
	///		The <c>D[Mm]</c> divergence from <see cref="KeyDefinitionPattern"/> is deliberate and
	///		load-bearing, and this is the one place it lives. Task 10 deleted the legacy
	///		<c>Resources/Styles/Colors.xaml</c>, whose keys were <c>DM</c>-prefixed; the current
	///		tokens are <c>Dm</c>-prefixed. The parity facts must see a surviving <c>DM</c>
	///		reference in order to report it as dangling, so their definition scan has to be able
	///		to name a <c>DM</c> key at all — a <c>Dm</c>-only pattern would not match the
	///		reference, would not add it to the missing list, and would pass. Every other scan
	///		wants the strict <c>Dm</c> form, because for those a <c>DM</c> key is not a key.
	/// </summary>
	public static readonly Regex LegacyTolerantKeyDefinitionPattern =
		new(
			@"x:Key=""(D[Mm][A-Za-z0-9]*)""",
			RegexOptions.Compiled);

	/// <summary>
	///		Matches an XML comment, including one spanning lines.
	///		The theme dictionaries carry long prose comments that name properties and elements;
	///		a scan that does not strip them reads the prose as markup.
	/// </summary>
	public static readonly Regex XmlCommentPattern =
		new(
			@"<!--.*?-->",
			RegexOptions.Compiled | RegexOptions.Singleline);

	/// <summary>
	///		Gets the absolute path of the Resources/Theme directory.
	/// </summary>
	public static string ThemeDirectory =>
		Path.Combine(
			RepoPaths.AppRoot,
			"Resources",
			"Theme");

	/// <summary>
	///		Returns every dictionary under Resources/Theme, at any depth.
	///		<c>AllDirectories</c> is deliberate: a file in a subdirectory used to be exempt from
	///		every hygiene pattern AND invisible to every definition scan AND invisible to the
	///		merge guard, which is the widest blind spot this suite ever had.
	/// </summary>
	public static IReadOnlyList<string> ThemeFiles()
	{
		var files = Directory
			.EnumerateFiles(
				ThemeDirectory,
				"*.xaml",
				SearchOption.AllDirectories)
			.OrderBy(
				path =>
					path,
				StringComparer.Ordinal)
			.ToList();

		files
			.Should()
			.NotBeEmpty("the theme dictionary scan must find files or every fact built on it is vacuous");

		return files;
	}

	/// <summary>
	///		Returns the path of each theme dictionary relative to the application project root,
	///		using forward slashes so it can be compared directly against an
	///		<c>App.xaml</c> <c>Source=</c> attribute.
	/// </summary>
	public static IReadOnlyList<string> ThemeFileSources()
	{
		return ThemeFiles()
			.Select(path =>
				Path
					.GetRelativePath(
						RepoPaths.AppRoot,
						path)
					.Replace(
						Path.DirectorySeparatorChar,
						'/'))
			.ToList();
	}

	/// <summary>
	///		Returns the text of a named dictionary under Resources/Theme.
	/// </summary>
	/// <param name="fileName">
	///		The dictionary's file name, for example "Tokens.xaml".
	/// </param>
	public static string ThemeFileText(string fileName)
	{
		return File.ReadAllText(
			Path.Combine(
				ThemeDirectory,
				fileName));
	}

	/// <summary>
	///		Returns the value of an <c>x:Double</c> token declared in the named dictionary.
	/// </summary>
	/// <param name="fileName">
	///		The declaring dictionary's file name, for example "Typography.xaml".
	/// </param>
	/// <param name="key">
	///		The token's resource key.
	/// </param>
	public static double DoubleToken(
		string fileName,
		string key)
	{
		var match = Regex.Match(
			ThemeFileText(fileName),
			$@"<x:Double x:Key=""{Regex.Escape(key)}"">(?<value>[0-9.]+)</x:Double>");

		match
			.Success
			.Should()
			.BeTrue($"token '{key}' should be defined in {fileName}");

		return double.Parse(
			match.Groups["value"].Value,
			CultureInfo.InvariantCulture);
	}

	/// <summary>
	///		Returns the value of an <c>x:Int32</c> token declared in the named dictionary.
	///		<para>
	///		Separate from <see cref="DoubleToken"/> on purpose. Both would happily parse "3",
	///		so a single lenient reader that accepted either element would keep passing through
	///		exactly the retype that broke the radius tokens — and the declaring element is the
	///		thing that decides whether the token binds at all.
	///		</para>
	/// </summary>
	/// <param name="fileName">
	///		The declaring dictionary's file name, for example "Tokens.xaml".
	/// </param>
	/// <param name="key">
	///		The token's resource key.
	/// </param>
	public static int Int32Token(
		string fileName,
		string key)
	{
		var match = Regex.Match(
			ThemeFileText(fileName),
			$@"<x:Int32 x:Key=""{Regex.Escape(key)}"">(?<value>-?[0-9]+)</x:Int32>");

		match
			.Success
			.Should()
			.BeTrue($"token '{key}' should be defined as an x:Int32 in {fileName}");

		return int.Parse(
			match.Groups["value"].Value,
			CultureInfo.InvariantCulture);
	}

	/// <summary>
	///		Returns the value of an <c>x:String</c> token declared in the named dictionary.
	/// </summary>
	/// <param name="fileName">
	///		The declaring dictionary's file name, for example "Typography.xaml".
	/// </param>
	/// <param name="key">
	///		The token's resource key.
	/// </param>
	public static string StringToken(
		string fileName,
		string key)
	{
		var match = Regex.Match(
			ThemeFileText(fileName),
			$@"<x:String x:Key=""{Regex.Escape(key)}"">(?<value>[^<]+)</x:String>");

		match
			.Success
			.Should()
			.BeTrue($"token '{key}' should be defined in {fileName}");

		return match.Groups["value"].Value;
	}

	/// <summary>
	///		Returns the 1-based line number of an offset within the supplied text.
	/// </summary>
	/// <param name="text">
	///		The text the offset indexes into.
	/// </param>
	/// <param name="index">
	///		The character offset.
	/// </param>
	public static int LineNumber(
		string text,
		int index)
	{
		return text
			.Take(index)
			.Count(character =>
				character == '\n')
			+ 1;
	}
}
