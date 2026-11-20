<!-- frozen: 20261104-1726-test-hygiene-sweep, Gate 2 -->

# C2 -- Test naming (MASTER-144): the two schemes, and the classification of the 58

No callable operations. This contract fixes which test methods are renamed, under which rule, and
the classification of the 58. Gate 2 chose Option A (2026-11-04): all 58 are renamed.

## 1. The two schemes

- **`App/` -- sentence form.** A declarative sentence in PascalCase stating the condition and the
  outcome, containing **no underscore**: `NoRawColourLiteralsOutsideTheThemeDictionaries`.
- **Everywhere else (`AppCore/`, `Application/`, `Domain/`, `Infrastructure/`) --
  `Method_Scenario_ExpectedBehaviour`.** The first part names the member or type under test; the
  scenario part is omitted when there is none (`Method_Expected`). At least one underscore.

Population figures are in C1 section 3.

## 2. Option A (CHOSEN at Gate 2) -- every underscore name in `App/` is renamed

All 58 names in section 4 are renamed to sentence form. The README rule is purely by directory.
Post-condition N1 (C1 section 3) prints 0.

## 3. Option B (REJECTED at Gate 2; kept for the record, binds no task) -- rename only class (b)

Only the 27 class-(b) names are renamed; the 31 class-(a) names keep `Method_Scenario_Expected`, and
the README rule for `App/` reads "sentence form, except a fact that exercises one named helper method
directly, which uses `Method_Scenario_Expected`". Choosing B changes T6's criteria (N1 becomes
"exactly the 31 class-(a) names remain") and C3 section 2; nothing else.

## 4. Classification of the 58 underscore names in `App/` at startRef

**Rule (mechanical):** a name is class **(a)** when the text before its first underscore is, verbatim,
the identifier of a method declared under `src/` that the test body invokes. Otherwise it is class
**(b)**, a sentence split by an underscore. Checked by searching each prefix (with any leading `The`
also tried stripped) for a declaration under `src/**/*.cs`; the `ScanG1`..`ScanG4` prefixes are
declared at `src/DungeonMaster.Tests/App/AsyncDiscipline/AsyncDisciplineScanner.cs:43`, `:104`,
`:125`, `:167` and called by name in their test files (`grep -c "ScanG[1-4]("`: 3, 4, 4, 5 in the
four scanner-test files). Counts: `awk` over `methods.tsv` -> **31 (a), 27 (b)**.

### Class (a) -- 31

| File (under `src/DungeonMaster.Tests/`) | Line | Name | Method named |
| --- | --- | --- | --- |
| App/AsyncDiscipline/AsyncVoidIsSignatureForcedTests.cs | 59 | `ScanG3_WhenAnAsyncVoidMethodIsNeitherAnOverrideNorAnOnHandler_ReportsThatDeclaration` | `AsyncDisciplineScanner.ScanG3` |
| App/AsyncDiscipline/AsyncVoidIsSignatureForcedTests.cs | 81 | `ScanG3_WhenEveryAsyncVoidMethodIsAnOverrideOrAnOnHandler_ReportsNothing` | `ScanG3` |
| App/AsyncDiscipline/AsyncVoidIsSignatureForcedTests.cs | 99 | `ScanG3_WhenTheOnlyAsyncVoidDeclarationIsInACommentLine_ReportsNothing` | `ScanG3` |
| App/AsyncDiscipline/AsyncVoidUnitsAreGuardedTests.cs | 74 | `ScanG1_WhenAnAsyncVoidMethodAwaitsWithNoPrecedingTry_ReportsTheDeclaration` | `ScanG1` |
| App/AsyncDiscipline/AsyncVoidUnitsAreGuardedTests.cs | 96 | `ScanG1_WhenAnAsyncVoidMethodOpensTryBeforeItsFirstAwait_ReportsNothing` | `ScanG1` |
| App/AsyncDiscipline/AsyncVoidUnitsAreGuardedTests.cs | 114 | `ScanG1_WhenAnAsyncEventHandlerLambdaAwaitsWithNoPrecedingTry_ReportsTheSubscription` | `ScanG1` |
| App/AsyncDiscipline/AsyncVoidUnitsAreGuardedTests.cs | 136 | `ScanG1_WhenTheOnlyAwaitInTheBodyIsInACommentLine_ReportsNothing` | `ScanG1` |
| App/AsyncDiscipline/DiscardedTasksAreAbsentTests.cs | 67 | `ScanG2_WhenALineStartsWithADiscardAssignment_ReportsThatLine` | `ScanG2` |
| App/AsyncDiscipline/DiscardedTasksAreAbsentTests.cs | 89 | `ScanG2_WhenNoLineStartsWithADiscardAssignment_ReportsNothing` | `ScanG2` |
| App/AsyncDiscipline/DiscardedTasksAreAbsentTests.cs | 107 | `ScanG2_WhenTheOnlyDiscardAssignmentIsInACommentLine_ReportsNothing` | `ScanG2` |
| App/AsyncDiscipline/DiscardedTasksAreAbsentTests.cs | 127 | `ScanG2_WhenALineIsASwitchExpressionDiscardArm_ReportsNothing` | `ScanG2` |
| App/AsyncDiscipline/SyncOverAsyncIsAbsentTests.cs | 72 | `ScanG4_WhenALineBlocksOnATaskWithGetAwaiterGetResult_ReportsThatLine` | `ScanG4` |
| App/AsyncDiscipline/SyncOverAsyncIsAbsentTests.cs | 94 | `ScanG4_WhenALineBlocksOnATaskWithWait_ReportsThatLine` | `ScanG4` |
| App/AsyncDiscipline/SyncOverAsyncIsAbsentTests.cs | 116 | `ScanG4_WhenNoLineBlocksOnATask_ReportsNothing` | `ScanG4` |
| App/AsyncDiscipline/SyncOverAsyncIsAbsentTests.cs | 135 | `ScanG4_WhenTheOnlyBlockingCallIsInACommentLine_ReportsNothing` | `ScanG4` |
| App/AsyncDiscipline/SyncOverAsyncIsAbsentTests.cs | 154 | `ScanG4_WhenALineReadsAnOrdinaryResultProperty_ReportsNothing` | `ScanG4` |
| App/Theme/ThemeSourceTests.cs | 37 | `QualifiedElementName_PrefixesTheXamlNamespaceAndLeavesTheDefaultNamespaceBare` | `ThemeSource.QualifiedElementName` (`ThemeSource.cs:311`) |
| App/Theme/ThemeSourceTests.cs | 60 | `BlankXmlComments_ReplacesTheCommentWithSpacesAndMovesNothingElse` | `ThemeSource.BlankXmlComments` (`:408`) |
| App/Theme/ThemeSourceTests.cs | 123 | `BlankCSharpComments_BlanksWholeLineCommentsAndLeavesAUrlIntact` | `ThemeSource.BlankCSharpComments` (`:428`) |
| App/Theme/ThemeSourceTests.cs | 183 | `DecodeCharacterEntities_ExpandsASupplementaryPlaneHexEntityToOneRune` | `ThemeSource.DecodeCharacterEntities` (`:460`) |
| App/Theme/ThemeSourceTests.cs | 213 | `DecodeCharacterEntities_ExpandsTheDecimalFormAndTheAmpersandEntity` | same |
| App/Theme/ThemeSourceTests.cs | 226 | `DecodeCharacterEntities_ThrowsNamingTheEntityWhenItIsOutOfRange` | same |
| App/Theme/ThemeSourceTests.cs | 242 | `IsSingleRune_CountsRunesRatherThanChars` | `ThemeSource.IsSingleRune` (`:509`) |
| App/Theme/ThemeSourceTests.cs | 303 | `KeyedElements_CarryTheirQualifiedElementFileAndLineNumber` | `ThemeSource.KeyedElements` (`:282`) |
| App/Theme/ThemeSourceTests.cs | 366 | `DoubleToken_ThrowsNamingTheFileKeyAndElementWhenTheTokenIsAbsent` | `ThemeSource.DoubleToken` (`:330`) |
| App/Theme/ThemeSourceTests.cs | 387 | `Int32Token_ThrowsNamingTheFileKeyAndElementWhenTheTokenIsAbsent` | `ThemeSource.Int32Token` (`:361`) |
| App/Theme/ThemeSourceTests.cs | 406 | `StringToken_ThrowsNamingTheFileKeyAndElementWhenTheTokenIsAbsent` | `ThemeSource.StringToken` (`:385`) |
| App/Theme/ThemeSourceTests.cs | 428 | `Int32Token_ThrowsRatherThanParsingAKeyDeclaredAsADouble` | `ThemeSource.Int32Token` |
| App/ViewModels/NavigationLoadIsObservedTests.cs | 36 | `ApplyQueryAttributes_StartsNoDiscardedTask` | `DungeonDetailViewModel.ApplyQueryAttributes` (`src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs:116`) |
| App/ViewModels/NavigationLoadIsObservedTests.cs | 61 | `ApplyQueryAttributes_HoldsTheStartedLoadOnAnAwaitableMember` | same |
| App/ViewModels/NavigationLoadIsObservedTests.cs | 88 | `ApplyQueryAttributes_WhenTheIdIsUnusable_AssignsErrorMessage` | same |

### Class (b) -- 27

| File (under `src/DungeonMaster.Tests/`) | Line | Name | Why (b) |
| --- | --- | --- | --- |
| App/AsyncDiscipline/AsyncVoidIsSignatureForcedTests.cs | 117 | `AppSource_EveryAsyncVoidMethodDeclaration_IsAnOverrideOrAnOnHandler` | `AppSource` is no declared method |
| App/AsyncDiscipline/AsyncVoidUnitsAreGuardedTests.cs | 155 | `AppSource_EveryAsyncVoidUnit_StatesATryAboveItsFirstAwait` | same |
| App/AsyncDiscipline/DiscardedTasksAreAbsentTests.cs | 145 | `AppSource_NoLineStatesADiscardedTask` | same |
| App/AsyncDiscipline/SyncOverAsyncIsAbsentTests.cs | 172 | `AppSource_NoLineStatesASyncOverAsyncCall` | same |
| App/AsyncDiscipline/MapCanvasHandlerChangedIsGuardedTests.cs | 31 | `StructuralSourceScan_OfOnMapCanvasHandlerChanged_StatesNoStatementAboveItsTry` | no such method |
| App/AsyncDiscipline/MapCanvasHandlerChangedIsGuardedTests.cs | 60 | `StructuralSourceScan_OfOnMapCanvasHandlerChanged_FindsPreloadObjectImagesAsyncAfterItsTry` | no such method |
| App/Theme/BindablePropertyDefaultsMatchTokensTests.cs | 48 | `BorderBaseStrokeShapeRadius_EqualsDmRadiusDefault` | no such method |
| App/Theme/DmThemeKeysResolveTests.cs | 125 | `EveryKeyDmThemeResolves_IsDefinedInTheThemeDictionaries` | sentence split |
| App/Theme/DmThemeKeysResolveTests.cs | 149 | `EveryKeyDmThemeResolves_IsDeclaredAsTheTypeItIsResolvedAs` | sentence split |
| App/Theme/ImplicitStyleTests.cs | 162 | `ImplicitStyle_DeclaresItsRequiredSetters` | no such method |
| App/Theme/ImplicitStyleTests.cs | 200 | `ImplicitButtonStyle_CarriesItsCommonStates` | no such method |
| App/Theme/ImplicitStyleTests.cs | 244 | `ImplicitControlStyles_SetNoColourOrFontSize` | no such method |
| App/Theme/ThemeDictionaryTests.cs | 122 | `RequiredKey_IsDefinedInThemeDictionaries` | no such method |
| App/Theme/ThemeSourceTests.cs | 270 | `ThemeDocumentsAndAppDocuments_ShareOneParsedInstancePerFile` | names two methods, not one |
| App/Theme/ThemeSourceTests.cs | 336 | `TheTokenReaders_ReturnTheDeclaredValue` | a group, not a method |
| App/Theme/ThemeSourceTests.cs | 447 | `TheLegacyTolerantKeyPattern_StillMatchesThePrefixCasingTheStrictPatternRejects` | a field (`LegacyTolerantKeyDefinitionPattern`), not this identifier |
| App/Theme/ThemeTokenParityTests.cs | 62 | `EveryReferencedThemeKey_IsDefinedUnderResources` | sentence split |
| App/Theme/ThemeTokenParityTests.cs | 107 | `EveryThemeKeyStringLiteralInCode_IsDefinedUnderResources` | sentence split |
| App/ViewModels/FailureIsSurfacedTests.cs | 61 | `EveryDispatchInDungeonDetailViewModel_BranchesOnTheResult` | sentence split |
| App/ViewModels/FailureIsSurfacedTests.cs | 141 | `EveryViewModelWithADispatch_DeclaresAnErrorMessageProperty` | sentence split |
| App/ViewModels/FailureIsSurfacedTests.cs | 183 | `EveryViewModelErrorAssignment_RoutesThroughResultMessage` | sentence split |
| App/ViewModels/NavigationLoadIsObservedTests.cs | 133 | `EveryCommandThatWritesTheLoadedDungeon_IsGuardedWhileNoDungeonIsLoaded` | sentence split |
| App/ViewModels/ResultSentinelIsAbsentTests.cs | 73 | `NoDungeonDetailViewModel_ComparesAResultValueAgainstGuidEmptyOrDefault` | sentence split |
| App/ViewModels/ResultSentinelIsAbsentTests.cs | 104 | `TheDetector_FlagsTheRemovedSentinelForm` | no such method (`SentinelFindings` is the helper) |
| App/ViewModels/ResultSentinelIsAbsentTests.cs | 120 | `TheDetector_DoesNotFlagABareResultValueRead` | same |
| App/ViewModels/ResultSentinelIsAbsentTests.cs | 136 | `TheDetector_DoesNotFlagAGuidEmptyComparisonOnANonResultValue` | same |
| App/ViewModels/ResultSentinelIsAbsentTests.cs | 152 | `TheDetector_DoesNotFlagValueAsASubstringOfAnUnrelatedIdentifier` | same |

The 14 files above are the complete set of `App/` files holding an underscore test name at startRef
(`awk -F'\t' '$1 ~ /^App\// && $3 ~ /_/ {print $1}' methods.tsv | sort -u | wc -l` -> 14).

## 5. The three sentence names outside `App/` -- renamed to `Method_Scenario_Expected`

| File (under `src/DungeonMaster.Tests/`) | Line | Name | Owner |
| --- | --- | --- | --- |
| Domain/Entities/Floor/ObjectKindGuardTests.cs | 66 | `KindGuardRunsBeforeTheScaleRejection` | T3 |
| Domain/Entities/MutationOutcomeZeroValueTests.cs | 19 | `DeclarationOrderOfTheExistingMembersSurvived` | T3 |
| Infrastructure/Async/ConfigureAwaitDisciplineTests.cs | 37 | `EveryAwaitInScopedInfrastructureFilesIsConfiguredFalse` | T4 |

Decision (signed at Gate 2): renamed, per "keep `Method_Scenario_Expected` elsewhere" --
the rule is by directory, and these three are the only names outside `App/` with no underscore
(C1 section 3, N2). Outside `App/`, hybrid names that already carry an underscore (for example
`ObjectKindGuardTests.cs:39` `NoneIsADefinedMember_WhichIsWhyTheGuardNeedsItsOwnNoneCondition`) are
not in this population and are not renamed.

## 6. Rules every new name obeys

- `App/`: no underscore; states condition and outcome; claims no more than the test asserts
  (test discipline, Naming).
- Elsewhere: first part names the member or type under test; two or three underscore-separated parts.
- A renamed method's body is unchanged. Only the identifier on its declaration line changes, plus any
  reference to it (C1 section 10). Exception (amendment A4, escalation E2): a renamed method under
  `Domain/` or `Infrastructure/` that aaa.pl flags may also take the AAA edit classes its sibling methods
  take (marker lines, blank lines, Act-value extraction), and nothing else.
- Every new name is unique within its class.
- The implementing task reports an old-name -> new-name table for every rename.
