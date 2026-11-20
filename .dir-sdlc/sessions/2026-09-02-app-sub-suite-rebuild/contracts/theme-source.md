# Contract — `ThemeSource`, the XAML object model, and the shared text helpers

Producer: **T3**. Consumers: **T4, T5, T6, T7, T8**.

File: `src/DungeonMaster.Tests/App/Theme/ThemeSource.cs`
Namespace: `DungeonMaster.Tests.App.Theme`
Accessibility: `internal static class ThemeSource` — unchanged; the whole `App/` sub-suite is one
assembly.

## 0. Why this contract exists

Four audit entries land on this one file and five tasks read it, so it is frozen here rather than
negotiated later.

- `MASTER-028` / `TEST-004` — the shared scan matches regular expressions against XAML text, with
  three named silent false passes.
- `MASTER-102` / `TEST-016` — anti-vacuity guards assert from inside Arrange helpers, so a broken
  scan is reported against whichever test called the helper first.
- `MASTER-138` / `TEST-028` — the comment-stripping pattern and the line-number computation are
  re-declared away from the class created to hold them.
- `MASTER-040` / `TEST-006` — two character-entity decoders exist three files apart, one correct and
  one not. The audit's own suggested direction is to lift the correct one into a shared helper "so
  the two cannot drift again", which makes it this file's business.

### 0.1 The counts, recounted — the audit's are stale

Measured in the worktree on 2026-09-02:

```
find src/DungeonMaster.Tests/App -name '*.cs' | wc -l              -> 20
grep -rl "Regex"     src/DungeonMaster.Tests/App --include=*.cs | wc -l  -> 14
grep -rl "XDocument" src/DungeonMaster.Tests/App --include=*.cs | wc -l  ->  1
```

`TEST-004`'s "eleven of the thirteen App files" is **stale**: there are **20** `.cs` files under
`App/`, **14** name `Regex`, and **1** names `XDocument`
(`Theme/TokenTypeMatchesTargetPropertyTests.cs`). Of the 14, five are outside this session's scope —
`AsyncDiscipline/AsyncDisciplineScanner.cs`, the async-discipline facts that use it, and
`ViewModels/FailureIsSurfacedTests.cs`. **No task in this session may state the audit's figure.**

## 1. Members that survive unchanged — signature **and** semantics

Every consumer compiles against these today and must keep compiling:

```csharp
public static readonly Regex HexLiteralPattern;
public static readonly Regex KeyDefinitionPattern;
public static readonly Regex LegacyTolerantKeyDefinitionPattern;
public static readonly Regex XmlCommentPattern;
public static string ThemeDirectory { get; }
public static IReadOnlyList<string> ThemeFileSources();
public static string ThemeFileText(string fileName);
public static int LineNumber(string text, int index);
```

Call sites, measured on 2026-09-02 with
`grep -rn "<member>" src/DungeonMaster.Tests/App --include=*.cs` excluding `ThemeSource.cs` itself:

| Member | Call sites | Owning tasks |
|---|---|---|
| `HexLiteralPattern` | `ThemeDictionaryTests.cs:130`; `ThemeHygieneTests.cs:73, 74, 166` | T4, T7 |
| `KeyDefinitionPattern` | `ThemeDictionaryTests.cs:380` | T4 |
| `LegacyTolerantKeyDefinitionPattern` | `ThemeTokenParityTests.cs:150` | T4 |
| `XmlCommentPattern` | `DmThemeKeysResolveTests.cs:207`; `ImplicitStyleTests.cs:190`; `ThemeDictionaryTests.cs:335`; `TokenTypeMatchesTargetPropertyTests.cs:364`; `TokenNamingTests.cs:308, 581` | T4, T8, T9 |
| `ThemeFileSources()` | `ThemeDictionaryTests.cs:150` | T4 |
| `ThemeFileText(...)` | `ImplicitStyleTests.cs:191`; `TokenNamingTests.cs:582` | T4 |
| `LineNumber(...)` | `DmThemeKeysResolveTests.cs:161`; `GeometryScaleTests.cs:786`; `ThemeHygieneTests.cs:215` | T7, T9 |
| `ThemeFiles()` (§2) | `DmThemeKeysResolveTests.cs:205`; `ThemeDictionaryTests.cs:124, 272, 378`; `TokenTypeMatchesTargetPropertyTests.cs:377`; `TokenNamingTests.cs:305` | T4, T8, T9 |
| `DoubleToken` / `Int32Token` / `StringToken` (§2) | `BindablePropertyDefaultsMatchTokensTests.cs:148, 194, 153` — **all three, one file, T9 only** | T9 |
| `ThemeDirectory` | none outside `ThemeSource.cs` | — |

**`src/DungeonMaster.Tests/App/Theme/TokenNamingTests.cs` is owned by T4** — added at Gate 2; it was
unowned in the draft this contract replaced. It calls `XmlCommentPattern`, `ThemeFileText` and
`ThemeFiles()`. Because §1 and §2 preserve every signature it keeps compiling. **T4 added a named
guard `[Fact]` in that file** and demonstrated it red by redirecting the class's own scan to an empty
directory. T3 does not touch the file.

**[CORRECTED, 2026-09-02. This paragraph claimed that when `ThemeFiles()` stopped asserting,
`TokenNamingTests.cs` "loses its only vacuity protection". That is FALSE, and it was measured false
twice — by T4 and independently by T4's reviewer.** At `405f576`, before T4 ran, the file already
carried in-fact `NotBeEmpty` guards at `:197` and `:318`, with the relevant `because` strings
verbatim — `"the colour-token scan must find tokens or this fact is vacuous"` and `"the theme
dictionaries must declare keyed styles or this fact is vacuous"` — and its third fact fails loudly on
an empty token set through its register-existence complaints. At `8f7e88a`, after T4, the file has
four facts and four in-fact guards (`grep -n "NotBeEmpty\|\[Fact\]"` → facts at `:201, :222, :260,
:342`, guards at `:206, :229, :271, :360`). Emptying the scan reddens **all four**, not one. The
claim was the architect's, made at Gate 2, and it is what justified extending T4's ownership to this
file. **The extension was still worth making — the file is now explicitly owned and explicitly
guarded — but the stated reason was wrong and is corrected here rather than left standing.**

**The distinction that survives, and it is the useful half.** `TokenNamingTests`'s guards sat
*inside its facts*, so retiring the shared helper's assertion cost it nothing. `DmThemeKeysResolveTests`'s
guards at `:195` and `:222` sit *inside private helpers* with no in-fact backup — which is why
`anti-vacuity.md` §1.1 row 16 genuinely is load-bearing and this file's rows never were. **T9 has now
confirmed that empirically**: with its keyed-element scan emptied,
`EveryKeyDmThemeResolves_IsDeclaredAsTheTypeItIsResolvedAs` **stayed green**, because it `continue`s
on a missing declaration, and only the promoted guard caught it. A guard inside a fact protects that
fact; a guard inside a helper protects nothing once the helper stops asserting. That is the rule, and
this correction is what established it.]

**`XmlCommentPattern` was absent from the first draft of this contract and has six external call
sites.** It is in §1 and T3 may not remove or retype it.

`LegacyTolerantKeyDefinitionPattern`'s `D[Mm]` divergence from `KeyDefinitionPattern` is
**load-bearing and documented in place** (`ThemeSource.cs:36-46`). It stays a regex and it stays
lenient: the parity facts must be able to *name* a surviving `DM`-prefixed key in order to report it
as dangling, and a `Dm`-only pattern would not match the reference, would not add it to the missing
list, and would pass. **Nothing in this session may collapse the two.**

## 2. Members whose signature survives but whose assertion is removed

```csharp
public static IReadOnlyList<string> ThemeFiles();
public static double DoubleToken(string fileName, string key);
public static int    Int32Token (string fileName, string key);
public static string StringToken(string fileName, string key);
```

Today each calls `.Should()` from inside a non-test helper. Verified sites
(`grep -n "Should()" src/DungeonMaster.Tests/App/Theme/ThemeSource.cs`) → lines **87, 145, 178,
205** — four, not the three the audit lists. **[Line numbers as frozen on 2026-09-02, before T3
ran. T3 landed at `ea06a76` and `ThemeSource.cs` is now 616 lines, so these four are historical.
The members are `ThemeFiles`, `DoubleToken`, `Int32Token` and `StringToken`, and the names are what
a later reader should search for — see `build-file-ownership.md` §6.]** `StringToken` (205) is absent from `TEST-016`'s
`locations` and is the same defect.

| Member | Before | After |
|---|---|---|
| `ThemeFiles()` | asserts `NotBeEmpty`, then returns | returns, asserting nothing. May return an empty list. |
| `DoubleToken` | asserts the regex matched | throws `ThemeTokenNotFoundException` (§5) |
| `Int32Token` | asserts the regex matched | throws `ThemeTokenNotFoundException` (§5) |
| `StringToken` | asserts the regex matched | throws `ThemeTokenNotFoundException` (§5) |

**All four are converted together.** A token reader is the one shape where "return nothing" is not
expressible — a `double` has no empty — so the three token readers throw a typed exception instead.
A typed exception names its own cause; a `.Should()` in Arrange names the wrong test.

The protection those assertions gave does not disappear. It moves; see `anti-vacuity.md`.

## 3. New members — the `XDocument` model

Additive. Nothing in §1 or §2 is removed to make room, so a consumer task may be implemented before
or after its own migration without a compile break.

```csharp
/// A parsed theme dictionary, with line info retained.
public sealed record ThemeDocument(
	string Path,             // absolute, as ThemeFiles() returns it
	string RelativePath,     // relative to RepoPaths.AppRoot, forward slashes
	XDocument Document);     // parsed with LoadOptions.SetLineInfo

/// Every dictionary under Resources/Theme, at any depth, parsed at most once per process.
/// Ordinal-ascending by Path. Empty when the directory holds no .xaml. Asserts nothing.
public static IReadOnlyList<ThemeDocument> ThemeDocuments();

/// Every .xaml file under RepoPaths.AppRoot (obj/ and bin/ excluded), parsed at most once
/// per process. This is the WHOLE application tree, not only Resources/Theme, and it is what
/// the scans in SemanticDescriptionTests, IconFontCoverageTests, ThemeHygieneTests,
/// GeometryScaleTests and TokenTypeMatchesTargetPropertyTests read.
/// ThemeDocuments() is the Resources/Theme subset of this. Ordinal-ascending by Path.
/// Empty when nothing matches. Asserts nothing.
public static IReadOnlyList<ThemeDocument> AppDocuments();

/// Every x:Key-bearing element in the theme dictionaries.
/// A duplicate key is a fact for a consumer to report, so this returns EVERY declaration
/// rather than a dictionary in which a later one silently wins.
public static IReadOnlyList<KeyedElement> KeyedElements();

public sealed record KeyedElement(
	string Key,                  // the x:Key value verbatim, prefix casing preserved
	XElement Element,
	string QualifiedElementName, // "x:Double", "Color", "SolidColorBrush", "Style", ...
	string RelativePath,
	int LineNumber);             // 1-based from IXmlLineInfo; 0 when line info is unavailable

/// Returns the element name with its XAML prefix restored, so x:Int32 is distinguishable
/// from a hypothetical Int32 in the default MAUI namespace. Same value as the
/// KeyedElement.QualifiedElementName property, exposed as a function so a consumer can
/// qualify a name it holds itself - including one built in a synthetic document.
/// [ADDED E6, 2026-09-02, at the user's decision. See 3.1.]
public static string QualifiedElementName(XName name);

/// Replaces XML comment characters with spaces, PRESERVING every other character offset.
public static string BlankXmlComments(string xaml);

/// Replaces C#-comment characters with spaces, PRESERVING every other character offset.
public static string BlankCSharpComments(string csharp);

/// Expands &#NNN; and &#xNNNN; character entities and &amp;, using char.ConvertFromUtf32.
public static string DecodeCharacterEntities(string attributeValue);

/// True when the decoded text is exactly one Unicode scalar value (Rune), supplementary
/// plane included. This is the predicate that string.Length == 1 gets wrong.
public static bool IsSingleRune(string text);
```

### 3.1 `QualifiedElementName`

`prefix:LocalName` when the element's namespace is the XAML `x` namespace
(`http://schemas.microsoft.com/winfx/2009/xaml`) — so `x:Double`, `x:Int32`, `x:String` — and the
bare `LocalName` for the default MAUI namespace. This is exactly the spelling
`DmThemeKeysResolveTests.ExpectedDeclaringElement` already uses as its values (`"Color"`,
`"SolidColorBrush"`, `"x:Double"`, `"x:Int32"`, `"x:String"`, `"Thickness"`, `"Style"`), and exactly
what the private `TokenTypeMatchesTargetPropertyTests.QualifiedName(XName)` computes, so both
consumers compare against their existing tables without translation.

**[AMENDED E6, 2026-09-02.]** The value is now reachable **two** ways: as the `KeyedElement`
record's `QualifiedElementName` property, computed over the real tree, and as the standalone
`public static string QualifiedElementName(XName)` added to §3 above. The first draft exposed only
the property, which left T3's criterion 6 — "a test asserts both forms against a synthetic
document" — with no injection point, because a consumer could not qualify a name it had constructed
itself. T3 shipped the function at `ea06a76`.

**Consequence, and it is a requirement rather than a permission: the private
`TokenTypeMatchesTargetPropertyTests.QualifiedName` is now a duplicate of declared shared surface,
and T8 deletes it**, pointing its four call sites at `ThemeSource.QualifiedElementName`. The two
return expressions were confirmed identical by T3's reviewer and by comparing
`sed -n '291,297p' .../ThemeSource.cs` against
`sed -n '471,477p' .../TokenTypeMatchesTargetPropertyTests.cs`. Keeping a private copy would make
this member's own doc-comment claim — that a consumer compares against its existing table "without
a translation layer" — false, and it is the same divergence `ThemeSource` was created to prevent.
It is required rather than optional because an optional cleanup in a file being rewritten anyway is
one nobody performs, and MASTER-138 is a record of exactly that.

### 3.2 Offset preservation is the whole point of "blank"

`BlankXmlComments` and `BlankCSharpComments` replace comment characters with spaces and newlines
with newlines. They do **not** delete. `GeometryScaleTests` already depends on this — its own
`XmlCommentPattern` and `WholeLineCommentPattern` each capture a `body` group precisely so the
interior can be blanked with offsets kept — and
`LineNumber(text, index)` is meaningless against a shortened string. A consumer may pass a blanked
string and an index taken from it to `LineNumber` and get the line number in the original file.

### 3.3 The two C#-comment strippers disagree today, and T3 picks one

`ThemeDictionaryTests.CSharpLineCommentPattern` and `ThemeTokenParityTests.CommentLine` declare
C#-comment patterns that differ by an anchor; `GeometryScaleTests.WholeLineCommentPattern` documents
why *its* pattern must be anchored, in the comment immediately above its own declaration: an
unanchored `//.*$` strips from the `//` inside `ms-appx:///…` and every other URL, erasing the rest
of a real line of code. `BlankCSharpComments` adopts the **anchored** form and T3 records the reason
in the XML doc comment on the member. A consumer that believes the anchored form is wrong for its
case escalates; it does not re-declare a local pattern.

**[AMENDED E5, 2026-09-02.]** This paragraph cited `GeometryScaleTests.cs:727-730` and §3.2 cited
`:720-723`. **Both are wrong** — that range is inside a method body, the `offends` computation.
`grep -n "private static readonly Regex.*CommentPattern =" src/DungeonMaster.Tests/App/Theme/GeometryScaleTests.cs`
returns **`838`** for `XmlCommentPattern` and **`845`** for `WholeLineCommentPattern`, and
`sed -n '718,732p'` on the same file shows what the cited range actually holds. Confirmed
independently three times — by T3's implementer, by T3's reviewer, and by the orchestrator. T7 would
have been sent to the wrong code. **Both patterns are now cited by name rather than by line**, which
is the form T3 adopted in its own doc comments and which both its reviewer and the orchestrator
judged the better one: a name cannot go stale when a file shifts, and `GeometryScaleTests.cs` will
shift under T7. See `build-file-ownership.md` §6.

### 3.4 `DecodeCharacterEntities` — the `TEST-006` fix, in one place

Two decoders exist today:

- `Accessibility/SemanticDescriptionTests.cs:222-235` — `(char)int.Parse(...)`. **Wrong.** A code
  point above `U+FFFF` needs a surrogate pair and cannot be held in one `char`, so `&#x1F6E1;` is
  truncated to an unrelated BMP character. Its entity pattern also matches **hex only**
  (`&#x(?<code>[0-9A-Fa-f]+);`).
- `Theme/IconFontCoverageTests.cs:247-259` — `char.ConvertFromUtf32(...)`. **Correct.** Its entity
  pattern matches **both** forms (`&#(?<hex>x?)(?<code>[0-9A-Fa-f]+);`), choosing `NumberStyles`
  accordingly.

`DecodeCharacterEntities` is the second one, lifted. It handles both the decimal and the hex form,
uses `char.ConvertFromUtf32`, and also replaces `&amp;` with `&` — as both existing copies do.
`IsSingleRune` replaces the `text.Length != 1` test in `SemanticDescriptionTests.IsIconOnly`, which
rejects a surrogate pair outright and is the second half of the same defect.

**This member has a guaranteed discrimination available and consumers must use it:** a direct unit
test over the helper with a synthetic supplementary-plane input. `DecodeCharacterEntities("&#x1F6E1;")`
must return a two-`char` string whose single `Rune` value is `0x1F6E1`; the current
`(char)int.Parse` implementation returns one `char` of `0xF6E1`. That is arithmetic, not a
suppression interaction, so the test cannot be quietly satisfied by a second mechanism. It does not
require any supplementary-plane entity to exist in the app's XAML — and none is known to.

### 3.6 `ThemeMarkupReferences` — declared surface, moved by T12

**[ADDED E9, 2026-09-02, at the user's decision.]**

T4 needed a shared `{StaticResource …}` reference scan across two of the files it owned. This
contract declared no member for it and T4 owned no file it could be added to, so T4 declared
`internal static class ThemeMarkupReferences` at the bottom of `ThemeTokenParityTests.cs` — the file
whose subject *is* references — and **reported that as a working answer rather than the right one**,
saying in the class's own doc comment that "relocating it onto `ThemeSource` is a contract change and
is reported as one". It was, and this is it.

The class becomes declared surface on `ThemeSource`. Five public members, unchanged in signature:

```csharp
/// Every {StaticResource …} key referenced anywhere in the application's XAML, paired with the
/// referencing file's path relative to the application root. Attribute values and element text
/// only — comments are XML comment NODES and are never read. Asserts nothing; may be empty.
public static IReadOnlyList<KeyValuePair<string, string>> References();

/// Every Dm- or DM-prefixed string literal in the supplied C# source text.
/// Blank its comments first with BlankCSharpComments.
public static IEnumerable<string> CodeKeyLiterals(string csharpSource);

/// Every key named by a {StaticResource …} markup extension inside one attribute value or text node.
public static IEnumerable<string> StaticResourceKeys(string value);

public static bool IsCurrentKey(string key);          // strict Dm prefix
public static bool IsLegacyTolerantKey(string key);   // D[Mm] prefix — see §1
```

**T12 performs the move**, and owns `ThemeSource.cs`, `ThemeTokenParityTests.cs` and
`ThemeDictionaryTests.cs` for the duration. It depends on **T3, T4, T6, T7 and T9** — every task that
owns or heavily reads those files — so that the ownership overlap can never become a concurrent one.

**Three doc comments move with the members and must not be edited away.** Each records a measurement
that cost someone a debugging session:

- `CSharpKeyLiteralPattern` is **the one regular expression left in that file** and is never matched
  against XAML. That is §3.5's boundary, stated where a reader will meet it.
- Code-behind resolves some theme colours through `Application.Current.Resources[…]` with a plain
  string key, which XAML-only scanning cannot see. `NotebookView.RibbonBrush` carried
  `"DMRibbonRead"` inside a ternary through the whole of Task 5's migration for exactly that reason.
  **This is why C# string-literal scanning still exists** and is not redundant with the XML model.
- `References()` reads attribute values and element text only, because comments are XML comment
  *nodes* in the parsed document. The hand census this replaced got that wrong and recorded
  `DmBorderTavernStyle` as live on the strength of a mention inside a prose comment.

`IsLegacyTolerantKey`'s `D[Mm]` leniency is the same load-bearing divergence §1 protects on
`LegacyTolerantKeyDefinitionPattern`. **Nothing in the move may collapse it into `IsCurrentKey`.**

### 3.5 The `XDocument` model loses nothing a raw-text scan over `.xaml` catches — measured

Frozen here because five tasks would otherwise each have to decide it, and the first draft of this
decomposition decided it **wrongly**, on reasoning alone.

The claim under test was that a raw-text scan reaches literals an XML model cannot see, "including
inside attribute values". Ten synthetic cases were run through `ThemeSource.HexLiteralPattern`
(verbatim, `ThemeSource.cs:18-21`) and through an `XDocument` walk over every attribute value, text
node and comment, on .NET 10.0.400:

| Case | raw text | `XDocument` |
|---|---|---|
| hex literal in an attribute value | catches | **catches** |
| in element text | catches | catches |
| in an attribute value containing an escaped `>` | catches | catches |
| inside an XML comment | catches | catches |
| in a single-quoted attribute | catches | catches |
| in a CDATA section | catches | catches |
| in an attribute name / `xmlns` declaration | catches | catches |
| in an element name (no `#` present) | misses | misses |
| **split by a character entity** | **misses** | **catches** |

**The attribute-value case — the whole stated rationale — is caught by both.** The only disagreement
in ten went the XML model's way: a hex literal interrupted by a numeric character reference leaves
the raw regex with too few consecutive hex digits to match, while `XDocument` resolves the reference
first and yields the complete literal.

Two raw-text-only differences exist and neither is coverage worth keeping. A literal inside an XML
*processing instruction*: no XAML file in this repository carries one, and `XDocument` exposes PIs
anyway — the walk above simply did not enumerate them. And a **non-well-formed** `.xaml` file, where
the raw scan reports and `XDocument.Parse` throws `XmlException`; a throw is a loud failure, not
silent loss, and §5 already requires the offending relative path be prepended to it.

**So: the `.xaml` half of every scan in this suite migrates. The `.cs` half never does** — `XDocument`
cannot parse C#, and both `ThemeHygieneTests` and `GeometryScaleTests` scan both languages. That
split, and not a coverage argument, is the boundary.

## 4. Caching, ordering, idempotency

- `ThemeFiles()`, `ThemeDocuments()`, `AppDocuments()` and `KeyedElements()` are ordinal-ascending by
  path and, within a document, in document order. Order is stable across calls in one process.
- `ThemeDocuments()` and `AppDocuments()` read and parse each file **at most once per process**, and
  they share one cache: a file under `Resources/Theme` is parsed once and appears in both, as the
  **same** `ThemeDocument` instance. Consumers may call either in a loop without re-reading disk.
  This is `MASTER-068` / `TEST-017` answered at the `ThemeSource` level; the per-class half is each
  consumer's own.
- The cache is keyed by absolute path and is populated on first use. A consumer must not assume a
  file changed on disk mid-run is re-read; nothing in this suite writes to the App tree.
- No member of `ThemeSource` mutates anything a later call can observe.
- A compiled `Regex` declared on `ThemeSource` is a `static readonly` field constructed once. **No
  member of `ThemeSource` constructs a `Regex` inside a loop or per call** — that is `MASTER-063` /
  `TEST-005` at this level, and the cited page
  (https://learn.microsoft.com/dotnet/standard/base-types/compilation-and-reuse-in-regular-expressions)
  is the authority: `RegexOptions.Compiled` raises construction cost to lower match cost, so paying
  it per iteration is the exact inversion of what the option is for.

## 5. Error cases

| Condition | Surfaces as |
|---|---|
| `ThemeDirectory` does not exist | `DirectoryNotFoundException` from `Directory.EnumerateFiles`, unchanged from today |
| A theme dictionary is not well-formed XML | `System.Xml.XmlException` from `XDocument.Parse`, propagated with the offending **relative path** prepended to the message |
| `DoubleToken` / `Int32Token` / `StringToken` cannot find the key | `ThemeTokenNotFoundException` — a new `internal sealed class ThemeTokenNotFoundException : Exception` whose message names the file name, the key, and the element type expected (`x:Double` / `x:Int32` / `x:String`) |
| `Int32Token` finds the key declared as `x:Double` | `ThemeTokenNotFoundException`, **not** a lenient parse. The declaring element decides whether the token binds at all; `ThemeSource.cs:150-158` documents why a lenient reader would pass through exactly the retype that broke the radius tokens. |
| `DecodeCharacterEntities` meets an entity outside `0x0`–`0x10FFFF` or in the surrogate range | The `ArgumentOutOfRangeException` `char.ConvertFromUtf32` throws, with the offending entity text appended to the message. It does not silently drop the entity. |
| Any scan reaches nothing | An **empty collection**. Never an exception, never an assertion. |

## 6. Scope boundary — what T3 does *not* do

T3 converts the **shared** scan to `XDocument`, publishes the model, and lifts the shared text
helpers. It does **not** rewrite the consuming classes. Migrating a given fact off its own local
regex is the owning task's work — T4, T5, T6, T7, T8 — against this frozen surface. `TEST-004` is
therefore delivered across six tasks, not one, and the coverage mapping says so.

T3 also does not touch `AsyncDiscipline/AsyncDisciplineScanner.cs` or the four async-discipline facts
that use it, nor `ViewModels/*.cs`. Those five files name `Regex` and are outside 6b-i's scope.
