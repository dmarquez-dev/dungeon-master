# Coverage mapping -- 2026-09-02-app-sub-suite-rebuild

Written at Gate 2, before the approval that freezes `manifest.json`. The `Gate 2` columns are
complete; `End state` is filled at Gate 3, and `Implementing task` for contract operations is
filled at Gate 3 because it is only derivable once the definition-site search has run.

Item numbers refer to `spec.md`'s `## Definition of done`.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. App builds for plain `net10.0`; library for bare TFM, exe for Windows | T1 | covered | met |
| 2. `DungeonMaster.App.UnitTests` exists, plain `net10.0`, references App, passes | T1 | covered | met |
| 3. `DungeonMaster.Tests` free of the App reference and of MAUI global usings | T1 (three greps), re-asserted T8, T9 | covered | met |
| 4. Shipping app unaffected: solution clean, 640 / 77 hold, macCatalyst builds | T1 | covered | met |
| 5. `RepoPaths` rejects a root without `src/DungeonMaster.App` beneath it *(MASTER-008)* | T2 | covered | met |
| 6. Shared scan exposes an `XDocument` model; consumers query it *(MASTER-028)* | T3, T4, T5, T6, T7, T8 | covered -- Gap 1 closed at Gate 2, see below | met |
| 7. Rune-correct character-entity decoding *(MASTER-040)* | T3, T5, T6 | covered | met |
| 8. No compiled `Regex` constructed in a per-item loop *(MASTER-063)* | T3 (survey + rule), T4 (3 sites), T9 (2 sites) | covered -- Gap 3 closed at Gate 2, see below | met |
| 9. Type-matching and icon-font scans computed once per process *(MASTER-068)* | T3, T6, T8 | covered | met |
| 10. MAUI property types and conversion matrix by reflection *(MASTER-100)* | T8 | covered | met |
| 11. `cmap` parser extracted and its unexercised branches exercised *(MASTER-101)* | T6 | covered | met |
| 12. No assertion inside a shared scan helper *(MASTER-102)* | T3 (4 sites), T4 (3), T6 (1), T7 (2), T8 (4) | covered -- 14 sites, not the audit's 7 | met |
| 13. App C# assertions read real declarations *(MASTER-109)* | T8, T9 | covered | met |
| 14. Comment-stripping patterns and line-number helper shared *(MASTER-138)* | T3 (homes), T4, T5, T6, T7 | covered | met |
| 15. Inert theme ratchet fails on regression or is legible as a tripwire *(MASTER-149)* | T7 | covered | met |
| 16. Enumerating assertions guarded against vacuity *(6a backlog)* | T10 (6 sites) + `anti-vacuity.md` §3, binding on all tasks | covered | met |
| 17. `MauiHostFixture.InvokeAsync` gains a caller or is removed *(6a backlog)* | T10 -- removal expected, and the criterion says so | covered | met |
| 18. Appium harness drives one real interaction *(accepted addition)* | T11 | covered | met |

**Declined, explicitly:** the fourth 6a backlog item, `ShippedPackKinds` duplicating `pack.json`.
It is graphics and catalogue work, not App sub-suite work. Recorded here rather than dropped
silently.

**Not carried forward as coverage:** the third 6a backlog item, `HandledExceptions`' unexercised
capture path. `spike-parallelism-report.md` Q5 narrowed it -- an exception thrown from a raw
`Dispatcher.Dispatch` fail-fasts the process (3/3 runs) and never reaches the recorder. T10 renames
only; `anti-vacuity.md` §4.2 forbids any criterion resting on it. Nothing is built on it.

### Uncovered rows, in full

**Gap 1 -- CLOSED at Gate 2, and the reasoning that produced it was falsified by measurement.**

As reported, `GeometryScaleTests.cs` (990 lines) and `ThemeHygieneTests.cs` were left on raw-text
scanning, on the architect's stated belief that both scan for literals *anywhere, including inside
attribute values*, where an XML model could not see them. The user's decision was to fold the
question into T7 as a criterion rather than accept the belief. The orchestrator required the
prescribed injection to be **run before the criterion was written** -- the rule Session 6a adopted
after two criteria named a test that stayed green under the injection they prescribed.

The injection ran against a real `XDocument` on .NET 10.0.400, comparing
`ThemeSource.HexLiteralPattern` (verbatim from `ThemeSource.cs:18-21`) against an `XDocument` walk
over attribute values, text nodes and comments. Ten cases:

| Case | Raw text | `XDocument` | |
|---|---|---|---|
| A attribute value | caught | caught | same |
| B element text | caught | caught | same |
| C attribute containing `>` | caught | caught | same |
| D inside an XML comment | caught | caught | same |
| E single-quoted attribute | caught | caught | same |
| F split by a character entity | **missed** | **caught** | XML-only |
| G in a CDATA section | caught | caught | same |
| H in an attribute name | caught | caught | same |
| I in an element name | missed | missed | same |
| J in a processing instruction | caught | missed | raw-only |
| K file not well-formed | caught | throws | raw-only |

**Case A -- the exact claim the rationale rested on -- is caught by both.** The single substantive
disagreement went the *other* way: `#FF&#x30;000` is missed by the regex, which sees two hex digits
before the entity, and caught by `XDocument`, which resolves it to `#FF0000`. J is a gap in the
probe's own walk (`XProcessingInstruction` was not enumerated, and no XAML file here carries a
processing instruction); K is a non-well-formed file, where `XDocument.Parse` throws loudly rather
than silently missing, and `theme-source.md` §5 already requires the relative path be prepended.

The criterion therefore requires the migration outright, with no "or demonstrate" branch, and
**`MASTER-028` closes fully rather than partially.**

What survives is a different and definitional boundary, frozen as `theme-source.md` §3.5 so that T4,
T5, T6, T7 and T8 all read it: **`XDocument` cannot parse C#**, and both classes scan both languages
-- `ThemeHygieneTests` calls `Offenders` with `"*.cs"` at `:74`, `:87`, `:100`, and
`GeometryScaleTests` calls `Scan` with `"*.cs"` at `:381`, `:416`, `:466`, `:509`, `:547`, `:587`.
The `.xaml`/`.cs` split is the boundary, not a coverage argument.

**Gap 2 -- CLOSED at Gate 2: T4 takes ownership.**
`src/DungeonMaster.Tests/App/Theme/TokenNamingTests.cs` (589 lines) had no owning task and is a live
`ThemeSource` consumer. It calls `XmlCommentPattern` (`:308`, `:581`), `ThemeFileText` (`:582`) and
`ThemeFiles()` (`:305`). It keeps compiling, but when `ThemeFiles()` stops asserting under T3, it
loses its only vacuity protection.

T4 now owns the file and carries two criteria. The first adds a named guard `[Fact]` asserting that
the file collection *this class* scans is non-empty -- declared on a member of the class rather than
calling `ThemeSource.ThemeFiles()` inline, **so the demonstration is possible entirely inside the
file T4 owns**, which is what keeps the criterion satisfiable. The second redirects the class's own
scan source to an empty directory and requires both observations to be quoted: the guard
**failing**, and every other fact in the class **passing while examining nothing**. The second
observation is the direct evidence that the file was vacuous-capable.

This gap belongs to no definition-of-done item directly -- it is a consequence of item 12's
helper-assertion removal.

**Gap 3 -- CLOSED at Gate 2.** The audit asks both to hoist compiled `Regex` objects out of per-item
loops *and* to drop `RegexOptions.Compiled` from any pattern that is genuinely short-lived. The
hoisting criteria covered the first; nothing audited `Compiled` usage across the sub-suite. T3 now
carries the survey and the rule, and the fixes sit with the tasks that own the files -- T4 for
`ThemeDictionaryTests.cs:304`, `:308` and `:358`, T9 for the two in its own files.

Measured while writing the criterion, and re-verified independently by the orchestrator:

```
grep -rn "RegexOptions.Compiled" src/DungeonMaster.Tests/App --include=*.cs | wc -l   -> 64
grep -rl "RegexOptions.Compiled" src/DungeonMaster.Tests/App --include=*.cs | wc -l   -> 13
grep -rn "new Regex(" src/DungeonMaster.Tests/App --include=*.cs                      -> 6 sites
```

The six method-body construction sites: `BindablePropertyDefaultsMatchTokensTests.cs:84` and `:129`,
`DmThemeKeysResolveTests.cs:136`, and `ThemeDictionaryTests.cs:304`, `:308` and `:358`.

**Two of those six are absent from `MASTER-063`'s own `locations` list** --
`ThemeDictionaryTests.cs:358` and `DmThemeKeysResolveTests.cs:136`. A task fixing only what the audit
named would have left two sites standing. `MASTER-063` closes fully.

## Contract operations

`Consuming task` is the task responsible for **calling** the operation, which is never the task that
implements it. `Implementing task` is filled at Gate 3 from the definition-site search.

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `RepoPaths.Root` | T2 | T8 | covered | called (TokenBindingFixture.cs) |
| `RepoPaths.AppRoot` | T2 | T3, T5, T6, T7, T9 | covered | called (ThemeHygieneTests.cs) |
| `RepoPaths.AppSourceFiles` | T2 | T3, T7, T8, T9 | covered | called (GeometryScaleTests.cs) |
| `ThemeSource.HexLiteralPattern` | T3 | T4, T7 | covered | called (ThemeHygieneTests.cs) |
| `ThemeSource.KeyDefinitionPattern` | T3 | T4 | covered | **not called** |
| `ThemeSource.LegacyTolerantKeyDefinitionPattern` | T3 | T4 | covered | **not called** |
| `ThemeSource.XmlCommentPattern` | T3 | T4 (incl. `TokenNamingTests.cs`), T8, T9 | covered | called (TokenNamingTests.cs) |
| `ThemeSource.ThemeFileSources()` | T3 | T4 | covered | called (ThemeDictionaryTests.cs) |
| `ThemeSource.ThemeFileText` | T3 | T4 (incl. `TokenNamingTests.cs`) | covered | **not called** |
| `ThemeSource.LineNumber` | T3 | T7, T9 | covered | called (GeometryScaleTests.cs) |
| `ThemeSource.ThemeFiles()` | T3 | T4 (incl. `TokenNamingTests.cs`), T8, T9 | covered | called (ThemeDictionaryTests.cs) |
| `ThemeSource.DoubleToken` / `Int32Token` / `StringToken` | T3 | T9 only, **and T9 may remove the last caller** | **at risk** -- see below | called |
| `ThemeTokenNotFoundException` | T3 | T3 (own tests), T9 | covered | **not called** |
| `ThemeSource.ThemeDocuments()` | T3 | T4 | covered | called (ThemeDictionaryTests.cs) |
| `ThemeSource.AppDocuments()` | T3 | T5, T6, T8 | covered | called (ThemeHygieneTests.cs) |
| `ThemeSource.KeyedElements()` | T3 | T4, T9 | covered | called (DmThemeKeysResolveTests.cs) |
| `ThemeSource.BlankXmlComments` | T3 | T5, T6, T7 | covered | called (IconFontFixture.cs) |
| `ThemeSource.BlankCSharpComments` | T3 | T4 | covered | called (GeometryScaleTests.cs) |
| `ThemeSource.DecodeCharacterEntities` | T3 | T5, T6 | covered | called (IconFontFixture.cs) |
| `ThemeSource.IsSingleRune` | T3 | T5 | covered | called (SemanticDescriptionTests.cs) |
| `ThemeSource.ThemeDirectory` | T3 | none | **uncovered** -- internal to `ThemeSource`, kept for signature stability only | **not called** |
| `AppFixture.Application` | T1 | T9 | covered | called (DmThemeResolutionTests.cs) |
| `AppFixture.FindResource` | T1 | T9 | covered | called (BindablePropertyDefaultTests.cs) |
| `AppFixture.Construct<T>(Func<T>)` | T1 | T8 | covered -- criterion added requiring the probes to route through it | called (MauiPropertyTypeTests.cs) |
| `AppTestCollection.Name` | T1 | T8, T9 | covered | called (MauiConversionMatrixTests.cs) |

### Operations at risk of shipping with an implementer and no caller

**`DoubleToken` / `Int32Token` / `StringToken`.** Their sole consumer today is
`BindablePropertyDefaultsMatchTokensTests.cs` at `:148`, `:194` and `:153`, owned by T9 -- which
moves those cases into `DungeonMaster.App.UnitTests` to read the live merged dictionary instead. If
T9 moves all of them, three helpers T3 has just converted are left with no caller. T9 carries an
explicit criterion to **report and escalate** rather than delete members of T3's file, since that
file belongs to another task.

**`AppFixture.Construct<T>`.** T1's own smoke tests were its only caller. A T8 criterion now
requires every MAUI instance in the conversion probes to be created through it, with an instruction
to report if that proves impossible.

This is the check the artifact exists to run. An operation with an implementer and no consumer means
every task can pass every criterion while the product cannot do the thing the operation exists for.

## How the Gate 3 contract-operation search was run, and the one rule that had to be adapted

Search range: `git diff --name-only 83ba80c..4133a68` -- **45 files, 9,069 insertions, 1,755
deletions**, `.cs` files only. Each operation matched as a **whole word on its bare member name, never
on the qualified form**: this codebase formats many calls as `ThemeSource` then `.Member()` on the
next line, and the qualified single-line grep cannot see them. That pitfall cost two agents a wrong
count in this session, one of whom documented the pitfall and fell into it in the same report.

Two of the three prescribed discards applied unchanged: the definition site, and every hit in a file
owned by the same task that owns the definition site.

**The third could not apply as written.** The rule discards hits inside a test file, because an
operation exercised only by its own tests is not reached by the application. **In this session every
file is a test file** -- the product *is* the test suite -- so applying it literally would discard
every hit and report all 25 operations as `not called`, which would be both absurd and useless.

The adaptation, stated rather than applied silently: **the owns-scoping discard already does the work
the test-file discard was written for.** An operation reached only from files owned by the task that
declares it is reached only by its own tests, which is exactly the condition the third rule exists to
detect. So the third rule is subsumed by the second here, and nothing is weakened -- `ThemeSource`'s
members reached only from T3's own `ThemeSourceTests.cs` are correctly reported `not called`.

### The six `not called` operations

**This is the finding the artifact exists to produce**, and it is stated with its weight rather than
dressed up: an operation with an implementer and no consumer means every task passed every criterion
and the product still cannot do the thing the operation exists for. Verified individually by direct
grep after the automated pass, because this is the number a reader trusts most.

| Operation | What the search found |
|---|---|
| `ThemeSource.ThemeFileText` | **Declaration only.** Zero call sites anywhere in the repository -- not even a doc reference. The strongest case of the six. |
| `ThemeSource.ThemeDirectory` | Declaration plus three uses, **all inside `ThemeSource.cs` itself**. Known and accepted at Gate 2 as internal, kept for signature stability. |
| `ThemeSource.StaticResourceKeys` | Declaration plus one use inside `ThemeSource.cs` (`References()`). Found by T12 and confirmed by its reviewer; **contract-declared public surface in §3.6** and correctly left alone. |
| `ThemeSource.KeyDefinitionPattern` | One call, `ThemeSourceTests.cs:458` -- T3's **own** tests, and a test *of the pattern* rather than a consumer. |
| `ThemeSource.LegacyTolerantKeyDefinitionPattern` | One call, `ThemeSourceTests.cs:452` -- same shape. |
| `ThemeTokenNotFoundException` | Thrown in `ThemeSource.cs`, caught only in T3's own `ThemeSourceTests.cs:375`. |

Three of these -- `ThemeFileText`, `StaticResourceKeys` and `KeyDefinitionPattern` -- were surfaced
independently by three different tasks and their reviewers during implementation, before this search
ran. **The search confirmed them and found three more.**

`DoubleToken` / `Int32Token` / `StringToken` were flagged **at risk** at Gate 2 on the expectation that
T9's migration would remove their last caller. It did not: T9 deliberately retained an `Int32Token`
consumer, and T3's own tests call all three at seven sites. **The risk did not materialise**, and T9
reported that the criterion asserting "exactly one consumer" was already stale when frozen.
