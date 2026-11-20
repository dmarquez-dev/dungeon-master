<!-- frozen: 20261104-1726-test-hygiene-sweep, Gate 2 -->

# C5 -- MASTER-139: the population, the split of each fact, and the injection protocol

No callable operations.

## 1. Population -- bounded to the five facts the row names

| # | Fact (current location, under `src/DungeonMaster.Tests/`) | Owner | Facts after | Delta |
| --- | --- | --- | --- | --- |
| S1 | `App/Accessibility/SemanticDescriptionTests.cs:81` `EveryIconOnlyControlHasASemanticDescription` | T5 | 2 | +1 |
| S2 | `App/Theme/IconFontCoverageTests.cs:60` `EveryDmFontIconTextIsInTheIconFontsCmap` | T5 | 2 | +1 |
| S3 | `App/Theme/IconFontCoverageTests.cs:104` `TheIconFontCmapParsesToPrivateUseGlyphs` | T5 | 4 | +3 |
| S4 | `App/Theme/TokenNamingTests.cs:261` `EveryRegisteredExceptionStillExistsAndStillOffends` | T5 | 3 | +2 |
| S5 | `AppCore/Map/MapDrawingSessionTests.cs:10` `PlaceVertex_FirstVertex_ReturnsAdded` | T2 | 3 | +2 |

T5 total +7, T2 total +2. The audit's `Application/Common/Map/MapDrawingSessionTests.cs:13-17` is
now `AppCore/Map/MapDrawingSessionTests.cs` (moved; DRIFT).

**Not in the population.** A census of test methods whose body holds both a `.NotBeEmpty(` and a
`.BeEmpty(` (a guard beside an offender assertion) returns 15 methods across `App/` and
`Application/`, the five-named facts S1, S2, S4 among them. That census attributes a line to the
nearest preceding `public` method header and was **not** validated against a fixture, so its total
is reported for Gate 2 and freezes into no criterion. No claim is made about any fact outside S1-S5.

## 2. What each split produces

Every new fact is a `[Fact]` with exactly one reason to fail. In `App/` names are sentence form (C2
section 1); in `AppCore/` they are `Method_Scenario_Expected`. A guard fact follows the naming the
project already uses for them (`The<Scan>Reaches<What>` / `The<Scan>FindsAtLeastOne<What>`, e.g.
`TokenNamingTests.TheTokenNamingScanReachesAtLeastOneThemeDictionary`) and asserts non-emptiness of
exactly one scan and nothing else. Assertion text and `because` strings are carried across unchanged.

- **S1.** The `reached.Should().NotBeEmpty(...)` guard and its explanatory comment (`:98-110` at
  startRef: comment `:98-105`, assertion `:106-110`) move to a new guard fact over `ScannedIconOnlyControls()`.
  `EveryIconOnlyControlHasASemanticDescription` keeps only the offenders assertion.
- **S2.** The `this.fixture.IconTexts.Should().NotBeEmpty(...)` guard (`:67-72`) moves to a new guard
  fact. The comment at `:62-66`, which says the guard "stays INSIDE this fact because that is the
  correct place for it", is false after the split and is removed or rewritten to describe the new
  arrangement. `EveryDmFontIconTextIsInTheIconFontsCmap` keeps only the offenders loop and assertion.
- **S3.** Four facts, one per property of the parse: (i) the parse finds at least one codepoint;
  (ii) it finds at least one codepoint in `0xE000..0xF8FF`; (iii) it contains `0xEA8F`; (iv) it
  contains `0xEAD4`. The existing name may be kept for (ii). The doc comment on the current fact is
  carried to whichever fact it describes.
- **S4.** Three facts: the original keeps the two register loops and `complaints.Should().BeEmpty()`;
  a new fact asserts every reason in `StructuralQualifierExceptions` and `CategorylessByDesign` is
  non-blank (the current `OnlyContain(...)` assertion, `:318-324`); a new guard fact asserts
  `ColourTokens()` is non-empty (the current `:270-274`, with its comment `:265-269`).
  `EveryColourTokenObeysTheNamingRule` (`:223`) keeps its own in-fact guard: it is outside the
  population.
- **S5.** Three facts with the same Arrange and Act: the result is `DrawSessionResult.Added`; the
  session holds one vertex; the session is active. Each carries the AAA markers (C1 section 4).

## 3. Known tension with an earlier session's convention -- resolved at Gate 2 in favour of the split

`IconFontCoverageTests.cs:62-66` cites a rule from DIR session `2026-09-02-app-sub-suite-rebuild`
(`.dir-sdlc/sessions/2026-09-02-app-sub-suite-rebuild/contracts/anti-vacuity.md` section 3): where an
offenders assertion sits over a scan, the non-empty guard goes **in the same test, first**, and
"another test asserting non-emptiness elsewhere" does not satisfy it. Gate 1 decided the opposite
for this row ("anti-vacuity assertions into facts of their own"). After S1, S2 and S4 the offender
facts are, in isolation, green on an empty scan; the paired guard fact is what goes red. This
contract is written to Gate 1's decision. That old contract coordinated its own session and is not
a project convention; no README or discipline restates it.

## 4. Injection protocol -- showing each new fact can fail

Every new fact in S1-S5 is shown able to fail by **demonstrated discrimination**: a defect injected
into the code the fact observes, the narrowest command that shows the failure, then a revert. The
implementer reports, per injection: file and line, the text injected, the command, the failing fact
names it produced, the passing sibling names, and that the revert was verified by the B census
(C1 section 6) and by re-reading the line.

Narrowest command form: `dotnet test C:/dm-wt/20261104-1726-test-hygiene-sweep/src/DungeonMaster.Tests/DungeonMaster.Tests.csproj --filter "FullyQualifiedName~<ClassName>"`.

The **discriminating property** a split exists for is that one defect fails one fact and not its
siblings. Where the table below says "siblings pass", the report shows them passing in the same run.

| Injection | Site (owned by the injecting task) | Defect | Must fail | Must pass |
| --- | --- | --- | --- | --- |
| J1 (T2) | `src/DungeonMaster.App.Core/Map/MapDrawingSession.cs:60-61`, the `IsActive` expression `_vertices.Count > 0` (one site) | a session holding one vertex reports inactive | S5 "active" fact | S5 "Added", S5 "one vertex" |
| J2 (T2) | same file, `:90` `_vertices.Add(point);` in `PlaceVertex` (one site) | the vertex is not recorded | S5 "one vertex" fact and S5 "active" fact (`IsActive` is derived from the vertex count at `:61`, so these two cannot be separated by an injection here) | S5 "Added" |
| J3 (T5) | `SemanticDescriptionTests.cs`, the return of `ScannedIconOnlyControls()` (`:346` at startRef) | the scan returns no controls | S1 guard fact | `EveryIconOnlyControlHasASemanticDescription` |
| J4 (T5) | `src/DungeonMaster.Tests/App/Theme/IconFontFixture.cs:88` `IconTexts = ScanIconTexts();` | the XAML scan reaches nothing | S2 guard fact | `EveryDmFontIconTextIsInTheIconFontsCmap` |
| J5 (T5) | `src/DungeonMaster.Tests/App/Theme/TrueTypeCmap.cs`, `:86`, the final `return covered;` of `Codepoints(byte[] font)` (the file has two, `:54` and `:86`; inject at `:86` only) | the parse drops codepoint `0xEA8F` | S3(iii) | S3(i), S3(ii), S3(iv) |
| J6 (T5) | same site as J5 | the parse drops `0xEAD4` | S3(iv) | S3(i), S3(ii), S3(iii) |
| J7 (T5) | same site as J5 | the parse drops every codepoint in `0xE000..0xF8FF` | S3(ii), S3(iii), S3(iv) | S3(i) |
| J8 (T5) | same site as J5 | the parse returns nothing | S3(i) (and the others) | -- |
| J9 (T5) | `TokenNamingTests.cs`, one entry's reason string in `StructuralQualifierExceptions` or `CategorylessByDesign` | a blank reason | S4 reason fact | `EveryRegisteredExceptionStillExistsAndStillOffends` |
| J10 (T5) | `TokenNamingTests.cs`, the return of `ColourTokens()` | the colour-token scan returns nothing | S4 guard fact | S4 reason fact |

Injection text is the implementer's; it must compile at its site (fully qualify any name that could
bind to a member of the enclosing type). Where an observed outcome differs from the table, the
implementer reports what it observed; the table states intent, and a J whose "must pass" column
cannot hold for a reason in the code is reported, not forced.

**Files owned only for injection** -- `MapDrawingSession.cs` (T2), `IconFontFixture.cs` and
`TrueTypeCmap.cs` (T5) -- end the task byte-identical to startRef: absent from the task's diff.
While an injection is live the shared tree is broken for every command that compiles or tests it;
the manifest orders every building task so that no peer builds during that window.
