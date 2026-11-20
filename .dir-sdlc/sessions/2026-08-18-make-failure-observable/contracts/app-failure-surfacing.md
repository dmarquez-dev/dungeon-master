# Contract: surfacing failure in the App layer

Frozen at Gate 2. **Authored by T8 (§1–§6) and T9 (§7).** Consumed by nobody — this is the end of
the chain, which is the point: the signal T1 produced and T4 mapped finally reaches a person.

Closes: **MASTER-001** (the session's only S1), **MASTER-045**'s consumer half, **MASTER-005**.

The conventions repository has **no** MAUI, XAML or view-model document — `frontend/` is React and
TypeScript only. Every rule below is therefore derived from this repository's own two working
examples, which is rung-1 evidence and stronger than a doc would be.

---

## 1. The pattern already exists here. Follow it; do not invent one.

Two view models already surface failure, and their pages already bind it.

`CreateDungeonViewModel.cs`:

```
30:	public partial string? ErrorMessage { get; set; }
56:		if (result.IsSuccess)
62:			ErrorMessage = result.Error;
```

`DungeonListViewModel.cs`:

```
32:	public partial string? ErrorMessage { get; set; }
69:		ErrorMessage = null;
85:		if (result.IsSuccess)
91:			ErrorMessage = result.Error;
```

`DungeonListPage.xaml:19-26` and `CreateDungeonPopup.xaml:36-42` both bind it identically:

```xml
        <Label
            Grid.Row="1"
            Text="{Binding ErrorMessage}"
            FontFamily="{StaticResource DmFontBody}"
            FontSize="{StaticResource DmFontSizeBody}"
            TextColor="{StaticResource DmDestructive}"
            IsVisible="{Binding ErrorMessage, Converter={StaticResource NotNullConverter}}" />
```

`NotNullConverter` is registered once, application-wide, at `App.xaml:20`. Nothing needs adding.

**This is the answer. `DungeonDetailViewModel` gets the same channel.** UI-004's suggested direction
says exactly that: "Give this view model the same `ErrorMessage` channel the list and create view
models already have."

### 1.1 Known interaction, deliberately not fixed here

`NotNullConverter` has its own defect — `MASTER-007`, it reports an unresolved binding as non-null,
so the `Label` can occupy space with no text. That entry is assigned to **Session 5**. Use the
converter as it is. Do not fix it, do not work around it, and do not write an acceptance criterion
that depends on it being correct. Note it in the task report so Session 5 inherits the context.

---

## 2. `DungeonDetailViewModel` — the frozen shape

`src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs`.

**Add**, beside the existing observable properties:

```csharp
	/// <summary>
	///		Gets or sets the message describing the most recent failed operation, or
	///		<see langword="null"/> when the most recent operation succeeded.
	/// </summary>
	[ObservableProperty]
	public partial string? ErrorMessage { get; set; }
```

Match the sibling files' attribute form exactly — read `DungeonListViewModel.cs:32` and copy it.
CommunityToolkit.Mvvm's source generator is version-sensitive and a mismatched form fails the build
for both target frameworks.

**Every `SendAsync` call site branches.** There are **14** in this file:

```
grep -c 'dispatcher.SendAsync' src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs   # 14
```

Exactly **one** of them captures the result today (`AddAreaWithOutlineAsync`, at the `var result =
await dispatcher.SendAsync<Guid>(` line); the other **13** discard it. The frozen shape at every one
of the 14:

```csharp
		ErrorMessage = null;

		var result = await dispatcher.SendAsync(
			new RemoveFloorCommand(
				_dungeonId,
				floorId),
			CancellationToken.None);

		if (result.IsFailure)
		{
			ErrorMessage = ResultMessage.ForFailure(result);

			return;
		}

		await LoadDungeonAsync(_dungeonId);
```

**No type argument on the ten no-payload dispatches.** `RemoveFloorCommand` and its nine siblings
implement the non-generic `ICommand` (`contracts/dispatch-and-pipeline.md` §1), so
`dispatcher.SendAsync(command)` binds `IDispatcher.SendAsync(ICommand, CancellationToken)` and
`result` is a non-generic `Result`. The four `SendAsync<Guid>(...)` sites keep their type argument
and their `Result<Guid>`. T4 has already made this change mechanically
(`contracts/handler-skeleton.md` §6); T8 must not reintroduce a type argument.

`ResultMessage.ForFailure` has an overload for each shape
(`contracts/result-and-error-surface.md` §6), so the line below is written identically at all
fourteen sites regardless of which shape `result` has. That uniformity is the point of the two
overloads.

Four binding properties:

1. **`ErrorMessage = null` before the dispatch.** `DungeonListViewModel.cs:69` already does this.
   Without it a stale message from a previous failure survives a later success.
2. **A failure skips the reload.** UI-004: "branch on `result.IsSuccess` at each call site before
   reloading, so a failure both skips the reload and says why." The unconditional reload is what
   currently repaints the pre-edit values and makes the rejection invisible.
3. **`ResultMessage.ForFailure(result)` is written with no type argument and no cast**, on both
   result shapes. Overload resolution picks the right one.
4. **The message comes from `ResultMessage.ForFailure`, not from `result.Error`.** This is the
   difference between this task and the two existing view models, and it is the half of MASTER-045
   that says "no production caller reads the category". `ResultMessage` switches on
   `ErrorCategory`; routing all three view models through it makes the category a read field in
   production for the first time.

`AddAreaWithOutlineAsync` dispatches **twice** and both dispatches must branch — the second one is
currently discarded even though the first is captured.

`LoadDungeonAsync`'s `QueryAsync` call is **not** wrapped. Queries return a DTO directly
(`backend-conventions.md` §2) and this session does not change that. A `null` DTO stays a `null`
DTO.

---

## 3. `DungeonListViewModel` and `CreateDungeonViewModel`

`src/DungeonMaster.App/ViewModels/DungeonListViewModel.cs` and
`src/DungeonMaster.App/ViewModels/CreateDungeonViewModel.cs`.

One change each: `ErrorMessage = result.Error;` becomes
`ErrorMessage = ResultMessage.ForFailure(result);`. Nothing else. These two already branch
correctly; do not restructure them.

---

## 4. `DungeonDetailPage.xaml` — the frozen edit

`src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml`, 640 lines.

The page's outer grid is `<Grid RowDefinitions="Auto,Auto,Auto,*" RowSpacing="0" Padding="16,12,16,0">`
with exactly four top-level children:

| Line | Child | `Grid.Row` before | after |
|---|---|---|---|
| 41 | nav bar `<Grid>` | 0 | **0** |
| — | **new error `<Label>`** | — | **1** |
| 120 | `<controls:DmFlagBanner x:Name="TitleBanner">` | 1 | **2** |
| 125 | `<controls:DmDivider>` | 2 | **3** |
| 136 | map + sidebar `<Grid>` | 3 | **4** |

`RowDefinitions` becomes `"Auto,Auto,Auto,Auto,*"`.

The nested `Grid.Row` attributes at lines 154, 268, 540, 582, 585, 588, 614 and 633 belong to
**inner** grids with their own `RowDefinitions` and **must not change**. The command that
distinguishes them — an outer-grid child is at 8 spaces of XAML indent, an inner one deeper:

```
grep -n 'Grid.Row=' src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml
```

**12 matches before, 13 after** — four outer-grid children plus eight nested. Exactly three of the
existing values change.

The new element, placed immediately after the nav-bar grid's closing tag, copying
`DungeonListPage.xaml:19-26` verbatim except for the row index:

```xml
        <!-- Row 1: failure channel. Collapsed while ErrorMessage is null. -->
        <Label
            Grid.Row="1"
            Text="{Binding ErrorMessage}"
            FontFamily="{StaticResource DmFontBody}"
            FontSize="{StaticResource DmFontSizeBody}"
            TextColor="{StaticResource DmDestructive}"
            IsVisible="{Binding ErrorMessage, Converter={StaticResource NotNullConverter}}" />
```

Note the XAML in this file is **space**-indented at 4, unlike the `.cs` files. `.editorconfig` sets
`[*.xaml] indent_style = tab`, but this file predates it and Session 1 deliberately excluded `.xaml`
from its reformat. **Match the surrounding file, not the `.editorconfig`.** Reformatting this file
is out of scope and is a blocking finding.

The page declares `x:DataType="vm:DungeonDetailViewModel"` (line 10), so the compiled binding
resolves `ErrorMessage` at build time — a typo here is a build error, not a silent no-op.

---

## 5. Why MASTER-001 has no unit test, and what stands in its place

`src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` targets `net10.0` and references **Domain,
Application and Infrastructure only**. It does **not** reference `DungeonMaster.App`, and it cannot:
the App project multi-targets `net10.0-windows10.0.19041.0` and `net10.0-maccatalyst`. The existing
`Tests/App/` suite works entirely by reading App **source files from disk** through
`Tests/App/Theme/RepoPaths.cs`.

Giving the App project a testable seam is `MASTER-026`, assigned to **Session 5**. It is not
in scope here, and adding a project reference to make one test possible would be a larger and
riskier change than the fix itself.

So MASTER-001's evidence is three-layered, and all three layers are required:

1. **A real unit test of the decision logic** — `ResultMessage` is in the Application layer
   precisely so it can be tested. T2 owns those tests
   (`contracts/result-and-error-surface.md` §7). They are behavioural and they fail before T2.
2. **A source-scanning test**, in the established `Tests/App/` idiom —
   `src/DungeonMaster.Tests/App/ViewModels/FailureIsSurfacedTests.cs`:
   - `EveryDispatchInDungeonDetailViewModel_BranchesOnTheResult` — parse the file via `RepoPaths`,
     find every `dispatcher.SendAsync` occurrence, and assert each is preceded within its enclosing
     method by an assignment of the result and followed by a `result.IsFailure` branch. Assert the
     occurrence count is 14, read from the file, and assert **zero** discarded dispatches. This test
     fails today with 13 discards.
   - `EveryViewModelWithADispatch_DeclaresAnErrorMessageProperty` — asserts all three view models
     declare `ErrorMessage`. Fails today for `DungeonDetailViewModel`.
   - `EveryViewModelErrorAssignment_RoutesThroughResultMessage` — asserts no view model assigns
     `result.Error` directly. Fails today for the other two.

   These are source assertions, and §9 of `contracts/coding-standards.md` warns against them. They
   are admitted **here and only here**, as supporting evidence, because the alternative is no
   automated evidence at all for the session's S1. They are explicitly **not sufficient on their
   own** — criterion 3 is what closes the entry.
3. **A manual observation, performed and reported.** Run the app on Windows, open a dungeon, open an
   area's notebook, clear the area name, and save. `UpdateAreaCommandValidator` rejects an empty
   name (`DungeonDetailPage.xaml.cs` around line 548 documents this rejection). Before the change
   the edit vanishes silently; after it, the red label appears above the banner reading the
   validator's message and the pre-edit values are **not** repainted. The implementer states in the
   report what was typed, what was displayed, and confirms the failing edit was not silently lost.

**This is stated plainly rather than dressed up:** MASTER-001 cannot be closed by `dotnet test`
alone at this point in the repository's life. It can be closed. It needs a human to look once.

---

## 6. What T8 must not do

- No new project reference from the test project to the App project.
- No change to `NotNullConverter`.
- No change to `DungeonDetailPage.xaml.cs` — it contains no dispatcher call.
- No reformatting of `DungeonDetailPage.xaml`.
- No `try`/`catch` added around a dispatch. The layer reports failure through `Result`; wrapping it
  in exception handling re-creates the problem in a new place.

---

## 7. `ObjectPaletteView` — MASTER-005

`src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs`, the `cb.Loaded` handler
inside the `#if WINDOWS` block (roughly lines 72–108). Today: ~35 statements inside
`try { ... } catch { /* non-critical visual styling — fail silently */ }` with no type and no
filter, covering a `ResourceDictionary` build, a `ThemeDictionaries["Light"]` assignment, and an
eight-level recursive visual-tree walk.

**Frozen shape:**

1. **Move `SetTextTrimmingOnChildren(cb, depth: 0)` out of the `try` entirely.** It is a recursion
   over the visual tree, not theming, and UI-022's whole complaint is that a genuine bug in it is
   caught identically to a missing resource key.
2. **Move the seven `SolidColorBrush` constructions and the `ResourceDictionary` population out of
   the `try`.** They cannot fail in a way worth swallowing.
3. **Narrow the `try` to the single statement UI-022 identifies as the one that can plausibly
   fail:**

```csharp
				try
				{
					cb.Resources.ThemeDictionaries["Light"] = d;
				}
				catch (Exception ex) when (
					ex is ArgumentException
						or InvalidOperationException
						or System.Runtime.InteropServices.COMException)
				{
					System.Diagnostics.Debug.WriteLine(
						$"ObjectPaletteView: failed to install the parchment ComboBox theme dictionary. {ex}");
				}

				SetTextTrimmingOnChildren(cb, depth: 0);
```

`Debug.WriteLine` rather than an injected `ILogger`: this view is constructed by XAML, not by DI,
and reaching into `IPlatformApplication.Current?.Services` from a `Loaded` handler to obtain a
logger is more machinery than the entry warrants. The record's ask is "log rather than discard", and
this logs. `Platforms/Windows/App.xaml.cs:62-68` — the sibling file UI-022 cites as having been
bitten by this exact failure — is the precedent for handling it visibly rather than silently.

4. **The comment `/* non-critical visual styling — fail silently */` is deleted.** It is no longer
   true and it is what scoped the reader's expectation wrongly.

### 7.1 Evidence for MASTER-005

This code is `#if WINDOWS`, runs in a `Loaded` event handler, and constructs WinUI types. It is not
unit-testable from a `net10.0` test project, and no amount of restructuring within this entry's
scope makes it so. Its criteria are therefore:

- `dotnet build src/DungeonMaster.sln` succeeds at 0 warnings for **both** target frameworks —
  which is what proves the `#if WINDOWS` branch still compiles, since the `net10.0-maccatalyst`
  build does not include it.
- `grep -n 'catch' src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs` shows
  **no** bare `catch` and **no** `catch (Exception)` without a filter, in this file, before or
  after.
- A manual run on Windows: open a dungeon, open the object palette, open the `ComboBox` dropdown,
  and confirm the parchment styling (border, background, item hover) renders exactly as it did
  before. The implementer states in the report that this was done and what was seen.

T9 is the one task in this session whose primary evidence is a human looking at a screen. That is
called out here so it is not discovered at review.
