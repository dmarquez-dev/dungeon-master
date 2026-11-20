# Contract — where an anti-vacuity guard lives, and what it must guard

Producer: none singly — this is a convention every task in the session builds against.
Consumers: **T2, T3, T4, T5, T6, T7, T8, T9, T10, T11**.

Two audit entries and one backlog item are the same defect seen from three angles:

- `MASTER-102` / `TEST-016` — assertions live inside shared scan helpers, so a broken scan is
  reported against whichever test called the helper first, and no single test owns "the scan reaches
  files".
- `MASTER-149` / `TEST-031` — one ratchet iterates a collection that is empty by construction, so it
  asserts that an empty list is empty.
- 6a's backlog item — `Assert.All(...)` and a bare `foreach` wrapping the assertions both no-op on
  empty input; three sites were named and the general rule was called the more valuable half.

This contract states the rule once so that ten tasks do not each invent it.

## 1. Helpers return. Tests assert.

**No non-test helper calls `.Should()`, `Assert.*`, or throws to signal an empty scan.** A helper
returns what it found, including nothing.

### 1.1 The full inventory — the audit's `locations` list is incomplete, and so was this one

`TEST-016` names seven locations. There are **sixteen** assertion sites inside non-test helpers,
across **nine** files.

**[AMENDED E8, 2026-09-02.** This table said "fourteen across eight" and called itself the full
inventory. It was short by two, in a ninth file: `DmThemeKeysResolveTests.ResolveCalls()` and
`.KeyedElementsInThemeDictionaries()`. Found by T3's reviewer running
`grep -n "private static\|Should()\|\[Fact\|\[Theory" src/DungeonMaster.Tests/App/Theme/DmThemeKeysResolveTests.cs`,
and confirmed by the architect with `sed -n '178,226p'` on the same file. An inventory that calls
itself complete and is not is the exact defect class this contract exists to prevent, so it is
corrected in place rather than footnoted.**]**

Enumerated by listing each file's helper declarations against its assertion lines:

```
grep -n "private static\|Should()\|\[Fact\|\[Theory" <file>
```

run over each of the eight files in the worktree on 2026-09-02. The table is the artifact; every line
number below was read from that output.

| # | Helper (declared at) | Assertion at | Owning task | In `TEST-016`? |
|---|---|---|---|---|
| 1 | `ThemeSource.ThemeFiles()` (`ThemeSource.cs:73`) | `ThemeSource.cs:87` | **T3** | yes |
| 2 | `ThemeSource.DoubleToken(...)` | `ThemeSource.cs:145` | **T3** | yes |
| 3 | `ThemeSource.Int32Token(...)` | `ThemeSource.cs:178` | **T3** | yes |
| 4 | `ThemeSource.StringToken(...)` | `ThemeSource.cs:205` | **T3** | **no** |
| 5 | `ImplicitStyleTests.Styles()` (`:186`) | `ImplicitStyleTests.cs:204` | **T4** | **no** |
| 6 | `ThemeDictionaryTests.MergedThemeSourcesInAppXaml()` (`:350`) | `ThemeDictionaryTests.cs:369` | **T4** | **no** |
| 7 | `ThemeTokenParityTests.ResourceKeyDefinitions()` (`:142`) | `ThemeTokenParityTests.cs:158` | **T4** | **no** |
| 8 | `IconFontCoverageTests.IconFontCodepoints()` (`:255`) | `IconFontCoverageTests.cs:265` | **T6** | **no** |
| 9 | `ThemeHygieneTests.Offenders(...)` (`:174`) | `ThemeHygieneTests.cs:198` | **T7** | yes |
| 10 | `GeometryScaleTests.Scan(...)` (`:738`) | `GeometryScaleTests.cs:759` | **T7** | mis-cited — see below |
| 11 | `TokenTypeMatchesTargetPropertyTests.XamlFiles()` (`:351`) | `:356` | **T8** | yes |
| 12 | `TokenTypeMatchesTargetPropertyTests.TokenTypesByKey()` (`:372`) | `:408` | **T8** | **no** |
| 13 | same helper | `:414` | **T8** | **no** |
| 14 | `TokenTypeMatchesTargetPropertyTests.AppPropertyTypes()` (`:424`) | `:439` | **T8** | yes |
| 15 | `DmThemeKeysResolveTests.ResolveCalls()` (`:178`) | `DmThemeKeysResolveTests.cs:195` | **T9** | **no** |
| 16 | `DmThemeKeysResolveTests.KeyedElementsInThemeDictionaries()` (`:201`) | `DmThemeKeysResolveTests.cs:222` | **T9** | **no** |

**`TEST-016` cites `GeometryScaleTests.cs:640-643` and that is the wrong site.** Line 657 in that
file is `stale.Should().BeEmpty();`, which sits inside a `[Fact]` and is therefore correct where it
is. The genuine helper assertion is at `:759`, inside `Scan(...)`. A task that fixes what the audit
pointed at will have fixed nothing.

Every one of the sixteen is retired. **A task does not get to skip a site because the audit did not
list it — and, after E8, not because THIS table did not list it either.** A task that meets a
helper-level assertion in a file it owns which appears in neither list retires it and reports the
addition.

Rows 1–4 were retired by **T3**, landed at `ea06a76`. Their line numbers are as frozen on
2026-09-02 and `ThemeSource.cs` has changed length since, so search for the member names.

**Row 16 is load-bearing and must not simply be deleted.** `KeyedElementsInThemeDictionaries()`
iterates `ThemeSource.ThemeFiles()` at `DmThemeKeysResolveTests.cs:205`, so it is strictly
downstream of that scan: an empty file list yields an empty dictionary, and the assertion at `:222`
fires. **It is what protects this class now that T3 has removed the assertion from `ThemeFiles()`
itself.** T3's implementer flagged `:205` as a consumer left unguarded; T3's reviewer checked and
found the flag over-cautious — for exactly this reason. The protection was already there, in the one
site this table had omitted. T9's criterion 8 rewrites that helper, and it now requires the guard to
survive as a named `[Fact]`, demonstrated red rather than inspected. Retiring a helper assertion
means **moving** the protection to a named fact, never dropping it; §2 says so, and this is the row
where getting it wrong would be silent.

### 1.2 The token readers are the one exception in shape, not in principle

`DoubleToken`, `Int32Token` and `StringToken` cannot "return nothing" — a `double` has no empty. They
throw the typed `ThemeTokenNotFoundException` defined in `theme-source.md` §5 instead of asserting. A
typed exception names its own cause; a `.Should()` in Arrange names the wrong test.

## 2. Every retired guard is replaced by a named `[Fact]`, in the same task

The protection is identical and the failure names itself. A guard fact:

- is a `[Fact]`, not a `[Theory]`, and not a line inside another fact;
- is named for **what it guards**, in the form `The<Scan>Reaches<What>` or
  `<Scan>FindsAtLeastOne<What>` — a name a reader can act on without opening the file;
- asserts non-emptiness of exactly one scan, and nothing else. A guard fact with a second reason to
  fail is two facts.

Example shape:

```csharp
[Fact]
public void TheThemeDictionaryScanReachesAtLeastOneFile()
{
	ThemeSource.ThemeFiles().Should().NotBeEmpty(
		"every fact built on the theme dictionary scan is vacuous if it reaches nothing");
}
```

**The guard fact lives in the task that owns the *consuming* file**, wherever the helper is shared.
That is what makes each consumer's vacuity its own problem. `ThemeSource`'s own scans are the
exception: T3 places their guard facts in a new
`src/DungeonMaster.Tests/App/Theme/ThemeSourceScanGuardTests.cs`, which T3 owns.

The `because` strings already written at the sixteen sites are good and should be carried across
rather than re-invented — they name the failure mode in the words of the person who found it.

## 3. Every assertion inside an enumeration carries a non-empty guard

`Assert.All(xs, ...)`, `xs.Should().AllSatisfy(...)`, and `foreach (var x in xs) { Assert...; }` all
pass green on an empty `xs`. Where the substantive claim of a test lives inside such a construct, the
test asserts the collection is non-empty **first**, in the same test.

This is not satisfied by a comment, by a `.Should().BeEmpty()` on a separate offenders list, or by
another test asserting non-emptiness elsewhere.

Sites in `DungeonMaster.App.Tests`, measured on 2026-09-02:

```
grep -rn "Assert\.All\|Assert\.NotEmpty" src/DungeonMaster.App.Tests --include=*.cs
```

→ **6 `Assert.All` call sites and 0 `Assert.NotEmpty` sites**, every `Assert.All` in
`Graphics/FloorMapDrawableDisposalTests.cs`, at lines **145, 150, 168, 187, 203 and 224**. 6a's
backlog item named two of the containing facts; the rest are the same defect and **T10 closes all
six**.

A seventh site takes the `foreach` form and so does not appear in that grep:
`MauiHostFixtureTests.EveryExceptionTheHostMarksHandledIsRecordedWithATypeAndAMessage`
(`src/DungeonMaster.App.Tests/Hosting/MauiHostFixtureTests.cs`) puts both its assertions inside a
`foreach` over `host.HandledExceptions`. **It is a different problem and §4 governs it — §3's remedy
is explicitly wrong for it.**

## 4. A guard that cannot fail must say so in the test report, not only in the source

Two tests in this session's territory are structurally incapable of failing today. They are **not**
treated the same way, because they are not the same thing.

### 4.1 `ThemeHygieneTests.EveryAllowListedFileExistsAndStillOffends` — `TEST-031`. **Kept.**

It iterates `PendingMigration`, initialised empty at `ThemeHygieneTests.cs:65`, so the `Where`
produces nothing and it asserts that an empty list is empty (`ThemeHygieneTests.cs:150`).

The audit accepts the author's self-arming-ratchet argument and so does this contract: the guard is
needed at the exact moment somebody adds the first allow-list entry, which is also the moment nobody
would think to write it. The equivalent guards in `GeometryScaleTests` and `SemanticDescriptionTests`
do bite today, so the shape is proven.

What changes is only its **visibility in a test run**. It gains a
`[Fact(DisplayName = "...")]` naming the condition — for example
`"Every allow-listed file still exists and still offends (inert while the allow list is empty)"` — so
a reader of a test *report*, not only of the source, can tell it apart from a scan that has silently
stopped reaching anything. That is the audit's own suggested direction, and it is `TEST-031`'s whole
remedy.

**It must not acquire an `Assert.NotEmpty` on `PendingMigration`.** That would turn a ratchet that is
correct while empty into a test that cannot pass.

### 4.2 `MauiHostFixtureTests.EveryExceptionTheHostMarksHandledIsRecordedWithATypeAndAMessage`. **Renamed, not guarded.**

Its input cannot become non-empty by a normal edit. The parallelism spike measured (Q5, 3/3 runs)
that an exception thrown from a `Dispatcher.Dispatch` callback **fail-fasts the test host process**
and never reaches `MauiHostFixture`'s `app.UnhandledException` handler, so it never lands in
`HandledExceptions`. The one category of exception a test is most likely to generate by accident is
the one the recorder demonstrably does not capture.

So: its **name must stop promising a guarantee it does not give**. T10 renames it to state the
condition it actually establishes, or narrows it to what it can prove.

**It must not acquire an `Assert.NotEmpty`** — that would be a test that cannot pass. And **no
criterion anywhere in this session may rest on `HandledExceptions` firing.**

## 5. What "non-empty" does not mean

A collection being non-empty is the floor, not the claim.

- A fact asserting `NotBeEmpty` on a scan and nothing else is a **guard fact** (§2) and must be named
  as one.
- A fact whose substantive claim is `BeEmpty` on an offenders list still needs §3's guard on the scan
  that produced the list. **An offenders list is empty both when everything is correct and when
  nothing was examined**, and those two states must not look the same from outside.

## 6. Showing a test can fail — binding on every task

The composed test discipline requires that every test is shown able to fail. In this session that
means, per criterion whose evidence is a test, **one** of:

- **Red step** — the command, and the failure observed, *before* the implementation existed; or
- **Demonstrated discrimination** — the defect the test exists to catch, injected into the code as it
  stands, with the command and the failure it produced.

"Verified by inspection" is neither. A test never seen failing has not been shown to test anything.

**And one warning specific to this project.** Where two mechanisms suppress the same outcome,
disabling one proves nothing about the other. 6a wrote two criteria saying "remove this guard flag,
watch the named test go red" and it stayed green both times, because the same code arm also stopped a
timer and a stopped timer ignores its tick. Before reporting a discrimination, **run it** — a
prescribed injection that leaves its named test green certifies nothing, and reading the code will
not tell you which case you are in.
