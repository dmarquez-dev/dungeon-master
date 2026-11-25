<!-- frozen: 20261124-0126-maui-namespace-filters, Gate 2 -->

# C1 - MAUI default-namespace element predicate

Markdown fallback: a C# static member cannot be declared without a body, so the declarations are
given as signatures only.

## Owner and home

- Declared in the source tree at `src/DungeonMaster.Tests/App/Theme/ThemeSource.cs`, inside the
  existing `internal static class ThemeSource` (namespace `DungeonMaster.Tests.App.Theme`).
- Owning task: T1. Consumers import nothing new: every consumer already reaches `ThemeSource`
  (same namespace, or `using DungeonMaster.Tests.App.Theme;` as `SemanticDescriptionTests.cs:5`
  has at 452b18f). No consumer re-declares either member.

## Declarations

```csharp
/// The MAUI default XML namespace.
public const string MauiNamespace = "http://schemas.microsoft.com/dotnet/2021/maui";

/// True exactly when element.Name == XName.Get(localName, MauiNamespace).
public static bool IsMauiElement(XElement element, string localName);
```

## Semantics

- Returns `true` exactly when the element's expanded name is `{http://schemas.microsoft.com/dotnet/2021/maui}<localName>`,
  whatever prefix (or none) the document used to bind that namespace.
- Returns `false` for the same local name in any other namespace: a `clr-namespace:` namespace,
  the XAML language namespace `http://schemas.microsoft.com/winfx/2009/xaml`, the WinUI
  presentation namespace, or no namespace.
- Comparison is ordinal and case-sensitive on both namespace and local name (XName equality).
- Pure: reads only `element.Name`; no allocation beyond the `XName` lookup, no caching contract,
  safe to call concurrently.

## Errors

- `element` null: `NullReferenceException` from reading `element.Name`. Not guarded; every caller
  passes an element it enumerated from a document. No test is required for it.
- `localName` not a valid XML NCName (for example the qualified spelling `"x:Style"`, or `""`):
  `System.Xml.XmlException` from `XName.Get`. This is the property that makes a caller still
  passing a `QualifiedElementName`-style `x:` name fail loudly instead of silently matching
  nothing. Measured at Gate 2: `[System.Xml.Linq.XName]::Get("x:Name", "<maui ns>")` under
  .NET 10.0.9 (PowerShell 7) raised `System.Xml.XmlException: The ':' character, hexadecimal
  value 0x3A, cannot be included in a name.` T1 pins it with a fact.
- `localName` null: `ArgumentNullException` from `XName.Get`. Not tested.

## Unchanged by this contract

`ThemeSource.QualifiedElementName(XName)` keeps its signature and body; `ThemeSource.cs:256`
and `:639` (452b18f) are not callers of the new predicate.

## Consumers

| Operation | Implemented by | Called by |
|---|---|---|
| `IsMauiElement` | T1 | T1 (`ImplicitStyleTests` SetterProperties, XamlNames, IsStyle; `ThemeDictionaryTests` MergedThemeSourcesIn), T2 (`ConfirmButtonStateParityTests.IsNamed`, `IconButtonStyleParityTests.IsNamed`, `SemanticDescriptionTests.IconOnlyControls`) |
| `MauiNamespace` | T1 | T1 (`IsMauiElement` itself; optionally new synthetic documents), T2 optional |
