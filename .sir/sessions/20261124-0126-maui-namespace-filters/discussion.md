# MAUI-namespace element-selection filters

Make the element-selection filters in `src/DungeonMaster.Tests` that are built on `ThemeSource.QualifiedElementName` require the MAUI default namespace, so a `Style`, `Setter`, `Label` or `ResourceDictionary` element in a foreign `clr-namespace` namespace is no longer selected as a MAUI one. This closes the last row of `.sir/backlog.md` (line 85 at `452b18f`, filed by 20261123-0703-test-strength-rows).

## Context

- `ThemeSource.QualifiedElementName` (`src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:272-277` at `452b18f`) returns `x:<local>` for the XAML language namespace and the bare local name for every other namespace, so a filter comparing its result with a bare name such as `"Style"` accepts that local name in any namespace.
- Census at `452b18f`: `git grep -n "QualifiedElementName(" -- src` prints 21 lines. `src` is unchanged between `89e6aae` (where the row was measured) and `452b18f`; the row's eight cited lines are all at the same numbers.
- 20261123-0703-test-strength-rows already fixed the `FloorDragHandleTests` and `FloorExpandToggleTests` assertions by comparing against `XName.Get(<name>, MauiNamespace)`; that is the precedent for the fix shape.
- PR mode, `gh` ready (preflight at `452b18f`); no `CLAUDE.md` line overrides the PR closeout.

## Decisions

1. Track: Standard (Gate 1). Branch `test/maui-namespace-filters`, type `test`.
2. In scope, seven filters, each to require the MAUI default namespace: `src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs:397`, `src/DungeonMaster.Tests/App/Theme/ConfirmButtonStateParityTests.cs:273`, `src/DungeonMaster.Tests/App/Theme/IconButtonStyleParityTests.cs:444`, `src/DungeonMaster.Tests/App/Theme/ImplicitStyleTests.cs:382`, `:412`, `:448`, and `src/DungeonMaster.Tests/App/Theme/ThemeDictionaryTests.cs:407`, all at `452b18f`. The spec phase reads the names each site's callers pass (`IsNamed` callers at `ConfirmButtonStateParityTests.cs:211`, `:220`, `:229`, `:238` and `IconButtonStyleParityTests.cs:339`, `:353`, `:379`, `:388`, `:400`; the `ImplicitStyleTests.cs:407` helper's callers) and drops any site whose compared names are all `x:` names, as for `:639` below.
3. The row's eighth site, `ThemeSource.cs:639` (`TokenText`), is out of scope and the row was wrong about it: its only callers, `:296`, `:327` and `:350`, pass `"x:Double"`, `"x:Int32"` and `"x:String"`, and `QualifiedElementName` produces an `x:` prefix only for the XAML namespace, so it cannot select a foreign-namespace element.
4. Also unchanged: `ThemeSource.QualifiedElementName` itself; `ThemeSource.cs:256` (it builds `KeyedElement`'s name, not a filter); `ThemeSourceTests.cs:45` (the function's own test); the three `TokenBindingFixture.cs` sites (`:327`, `:356`, `:435`) and the attribute-name sites (`ConfirmButtonStateParityTests.cs:287`, `IconButtonStyleParityTests.cs:458`, `ImplicitStyleTests.cs:417`, `ThemeTokenParityTests.cs:171`), as the row says.
5. Fix shape: compare against an `XName` in the MAUI default namespace, as the 20261123-0703 precedent does; whether through a shared helper in `ThemeSource.cs` or per site is the spec phase's call.
6. Each fixed filter gets a synthetic control: an element of each selected name in a `clr-namespace` namespace that must not be selected, plus the existing `DungeonMaster.Tests` run passing. Each control is shown able to fail (an injected revert of the filter turns it red).
7. No behaviour change and no human checks: the change is confined to `src/DungeonMaster.Tests` and `.sir/backlog.md`.
8. Gate 3 removes backlog row 85 and records in the session's report that its `ThemeSource.cs:639` claim was wrong.
