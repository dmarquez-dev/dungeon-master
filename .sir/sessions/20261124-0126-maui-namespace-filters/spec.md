# MAUI-namespace element-selection filters

Goal: the seven element-selection filters in `src/DungeonMaster.Tests` that compare `ThemeSource.QualifiedElementName(element.Name)` with a bare MAUI name select only elements in the MAUI default namespace, through one shared predicate on `ThemeSource`.

## Context

- `ThemeSource.QualifiedElementName` (`src/DungeonMaster.Tests/App/Theme/ThemeSource.cs:272-277` at `452b18f`) returns `x:<local>` for the XAML language namespace and the bare local name for every other namespace, so a filter comparing its result with `"Style"` also accepts a `Style` in any `clr-namespace:` namespace.
- `git grep -n "QualifiedElementName(" -- src` prints 21 lines at `452b18f`. Seven of them are element filters comparing against bare MAUI names (the sites below); every caller of the two `IsNamed` helpers and of `ImplicitStyleTests.XamlNames` passes a bare MAUI name (read by `grep -n` at `452b18f`: ConfirmButton `:211`, `:220`, `:229`, `:238`; IconButton `:339`, `:353`, `:379`, `:388`, `:400`; XamlNames `:213`, `:222`), so no site drops out.
- Precedent: 20261123-0703-test-strength-rows compared against `XName.Get(<name>, MauiNamespace)` in `FloorDragHandleTests.cs:100` and `FloorExpandToggleTests.cs:72`.
- This closes the last row of `.sir/backlog.md` (the 20261123-0703-test-strength-rows row "Make the 8 element-selection filters ..."), whose eighth site `ThemeSource.cs:639` was wrong: its callers pass only `x:` names.

## Definition of done

1. `ThemeSource` declares `public const string MauiNamespace` and `public static bool IsMauiElement(XElement element, string localName)` per `contracts/C1-maui-element.md`, pinned by two facts in `ThemeSourceTests` (namespace discrimination, and `XmlException` on `"x:Style"`), each shown able to fail by an injected revert of the predicate's body.
2. `SemanticDescriptionTests.IconOnlyControls` (`:397-399` at `452b18f`) selects only MAUI-namespace `Label` and `Button`; a synthetic control with `clr-namespace:` twins passes, goes red under an injected revert, and passes after the revert.
3. `ConfirmButtonStateParityTests.IsNamed` (`:273`): same, for `Style`, `VisualStateGroup`, `VisualState`, `Setter`.
4. `IconButtonStyleParityTests.IsNamed` (`:444`): same, for the same four names.
5. `ImplicitStyleTests.SetterProperties` (`:382`): same, for `Setter`.
6. `ImplicitStyleTests.XamlNames` (`:412`): same, for `VisualStateGroup` and `VisualState`.
7. `ImplicitStyleTests.IsStyle` (`:448`): same, for `Style`.
8. `ThemeDictionaryTests.MergedThemeSourcesInAppXaml` (`:407`): selection moved into a document-taking `MergedThemeSourcesIn(XDocument)`, then same, for `ResourceDictionary`.
9. Unchanged: `QualifiedElementName`'s body, `ThemeSource.cs:256` and `:639`, `ThemeSourceTests.cs:45`, and the attribute-name sites in the three files a task owns (`ConfirmButtonStateParityTests.cs:287`, `IconButtonStyleParityTests.cs:458`, `ImplicitStyleTests.cs:417`).
10. Every owned file stays CRLF-only and tab-indented, and none contains a `.sir/` path.
11. `dotnet build src/DungeonMaster.sln` gives 0 Warning(s) 0 Error(s); `DungeonMaster.Tests` passes 1023 of 1023 (1014 at `452b18f`, plus 6 from T1 and 3 from T2); App.UnitTests 217 and App.Tests 146 still pass.
12. The session's task commits change only paths under `src/DungeonMaster.Tests/`.
13. Gate 3 removes the 20261123-0703-test-strength-rows backlog row ("Make the 8 element-selection filters ...") and adds no others for this work, and the report records that the row's `ThemeSource.cs:639` claim was wrong.

## Scope

Seven filter sites, one shared predicate, seven synthetic controls plus two predicate facts, and a document-taking seam in `ThemeDictionaryTests`.

### Not in scope

- `ThemeSource.QualifiedElementName` itself, `ThemeSource.cs:256` and `:639`, `ThemeSourceTests.cs:45` (DoD 9, T1-21).
- The three `TokenBindingFixture.cs` sites (`:327`, `:356`, `:435`) and `ThemeTokenParityTests.cs:171`: no task owns either file.
- Consolidating the existing private `MauiNamespace` consts in `SemanticDescriptionTests.cs:55`, `FloorDragHandleTests.cs:53` and `FloorExpandToggleTests.cs:57` (T2-09 keeps the one at `:55`; the other two files are owned by no task).
- Any behaviour change, and any human check.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Shared helper or per-site fix? | Shared: `ThemeSource.MauiNamespace` + `ThemeSource.IsMauiElement`. ThemeSource already centralises the XAML namespace and its doc records copies drifting apart; per-site would make seven private copies of the URI. | repo | `ThemeSource.cs:72`, `:100-110` at `452b18f`; discussion decision 5 left it to this phase |
| Does a qualified name fail loudly? | Yes: `XName.Get("x:Style", ns)` throws `XmlException`, so a leftover `x:` caller fails rather than matching nothing; T1-03 pins it. | external | architect measured `[System.Xml.Linq.XName]::Get("x:Name", ...)` under .NET 10.0.9: `The ':' character ... cannot be included in a name.` |
| Does the fix drop any real element? | No: no app XAML outside `obj`/`bin` declares a prefixed `Label`, `Button`, `Style`, `Setter`, `VisualStateGroup`, `VisualState` or `ResourceDictionary`; the one file without the MAUI default xmlns (`Platforms/Windows/App.xaml`) is never read by these filters. The 1023 total is the check. | repo | architect's `grep -rLE` and prefixed-element search over the 27 `.xaml` files at `452b18f` (control: `x:Double` found 28 times) |
| Can tasks build concurrently? | No: both build `src/DungeonMaster.Tests` and share obj/bin, and T1 injects into `ThemeSource.cs`, which T2's tests call. T2 depends on T1. | repo | `contracts/C2-editing-and-injection.md` section 3 |
| How are the owned files edited? | Edit tool only; never `sed -i` or Write on an existing file; no trailing whitespace in `new_string`; tabs. This overrides the harness's auto-mode sed note. | repo | C2 section 1; all seven files CR = LF = CRLF pairs at `452b18f` (fixture `a\r\nb\rc\n` gives CR=2 LF=2 CRLF=1) |
| Is a filtered test run trustworthy? | Only with a `Total:` line: an exact `FullyQualifiedName=` filter selects 1, `A\|B` selects 2, a non-matching filter selects nothing and exits 0. | repo | C2 section 2; re-run by the orchestrator at Gate 2: the `A\|B` filter over two `ImplicitStyleTests` facts printed `Total: 2` |
| Baselines | DungeonMaster.Tests 1014, App.UnitTests 217, App.Tests 146, build 0/0, at `452b18f`. | repo | probe in `state.json` `verification` |

## Contracts

- [`contracts/C1-maui-element.md`](contracts/C1-maui-element.md): `ThemeSource.MauiNamespace` and `ThemeSource.IsMauiElement`.
- [`contracts/C2-editing-and-injection.md`](contracts/C2-editing-and-injection.md): editing rules and the one-at-a-time injection protocol.

## Open questions

None.
