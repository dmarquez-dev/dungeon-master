<!-- frozen: 20261104-1726-test-hygiene-sweep, Gate 2 -->

# C1 -- Measurement: every instrument, its controls, and the startRef figures

This contract has no callable operations. It is the single place where the session's census
predicates are stated, so that implementers, reviewers and the register task all run the same
instrument. A criterion that says "per C1 section N" means: run the instrument in that section,
verbatim, in the session worktree.

startRef for every figure below is `9c931b4abda23eed7c08cb40ec9931382473991b` (session base, branch
`chore/repo-audit`), measured by the architect in the worktree `C:/dm-wt/20261104-1726-test-hygiene-sweep`
on 2026-11-04. Figures attributed to the orchestrator are marked so.

## 0. How to run these

- Shell is Git Bash. Commands that read many files are run from
  `C:/dm-wt/20261104-1726-test-hygiene-sweep/src/DungeonMaster.Tests` unless the section says otherwise.
  Tasks cannot run git, so where a section uses `git ls-files` a task may substitute
  `find . -name '*.cs' -not -path './obj/*' -not -path './bin/*'`; the two populations are equal at
  startRef (123 `.cs` files, `git ls-files '*.cs' | wc -l`) provided no task adds or deletes a `.cs`
  file, and no task in this session does.
- The two Perl programs in sections 3 and 4 are instruments, not product code. Copy each, byte for
  byte, into your own scratch directory and run it from there. Never write them into the worktree.
- Every instrument here was run against a CRLF fixture before its project figure was trusted. The
  fixture text is given with each section so a reviewer can rebuild it with `printf`.

## 1. P1, P2 -- single-letter lambda parameters (MASTER-126)

P1 (unparenthesised single parameter):

```
perl -ne 'print "$ARGV:$.: $_" if /(?<![A-Za-z0-9_.])[A-Za-z]\s*=>/; close ARGV if eof' <files>
```

P2 (parenthesised parameter lists, typed or untyped):

```
perl -ne 'print "$ARGV:$.: $_" if /\(\s*(?:[A-Za-z_][A-Za-z0-9_]*\s+)?[A-Za-z]\s*(?:,[^()]*)?\)\s*=>|,\s*[A-Za-z]\s*\)\s*=>/; close ARGV if eof' <files>
```

Controls (run, fixture `FxTests.cs`, CRLF): P1 matched `var items = new[] { 1 }.Where(x => x > 0);`
and did not match `items.Select((a, b) => a)`. P2 matched `items.Select((a, b) => a)`. **P2 also
matched the near-miss `private static int Add(int a, int b) => a + b;`**, an expression-bodied member
and not a lambda: P2 over-reports, it never under-reports. The orchestrator's own control (P1 matches
`u =>` and `var f = c => 1`, not `(a, b) =>`) agrees.

startRef, over `git ls-files '*.cs'` in `src/DungeonMaster.Tests`:

- P1 prints **7** lines: `Application/Features/Dungeons/Validators/NameOnlyValidatorTestBase.cs:65`
  and `:80` (`e =>`), `Domain/Entities/Dungeon/DungeonAggregateTests.cs:32`, `:46`, `:71` (`f =>`),
  `Infrastructure/Persistence/Repositories/DungeonReadRepositoryTests.cs:29`, `:30` (`s =>`).
- P2 prints **0** lines.
- Independent cross-check, a third predicate that captures every identifier immediately before `=>`
  (optionally through one `)`) and keeps those of length 1:
  `perl -ne 'while (/([A-Za-z_][A-Za-z0-9_]*)\s*\)?\s*=>/g) { print "$ARGV:$.:$1\n" if length($1)==1 } close ARGV if eof'`.
  Excluding `_` (the discard, which the rule does not govern), it returns the same seven sites plus
  one more, `AppCore/Presentation/DungeonDetailPresentationMapperTests.cs:18`, which is
  `PlacedObjectDtoWith(float scale = 1f) =>` -- the `f` suffix of a float literal before an
  expression-bodied member. It is not a lambda parameter. The disagreement is explained, so 7 stands.

Post-condition for the session: P1 and P2 each print nothing over `src/DungeonMaster.Tests`.
Replacement names (from the row): `e` -> `failure`, `f` -> `floor`, `s` -> `summary`. None collides
with a local in its method at startRef (read: `NameOnlyValidatorTestBase.cs:55-82`,
`DungeonAggregateTests.cs:23-75`, `DungeonReadRepositoryTests.cs:12-31`).

## 2. PA -- an assertion between `// Arrange` and `// Act` (MASTER-128)

```
perl -ne 'if (m{//\s*Arrange}) {$in=1} elsif (m{//\s*(Act|Assert)\b}) {$in=0} elsif (/^\s*\[(Fact|Theory)/) {$in=0} print "$ARGV:$.: $_" if $in && /\.Should\(\)|\bAssert\.|\.Throw/; if (eof) {close ARGV; $in=0}' <files>
```

Control (fixture `FxTests.cs`): it printed exactly the one line
`var arranged = list.Should().ContainSingle().Which;` that sits under `// Arrange`, and nothing from
the `b.Should().Be(2);` lines that sit under `// Assert` (near-miss).

startRef, over every `.cs` in `src/DungeonMaster.Tests`: **2** lines, both in
`Application/Features/Dungeons/Commands/UpdatePlacedObjectCommandHandlerTests.cs`:
`:145` `var theObj = floor.Objects.Should().ContainSingle().Which;` (method
`HandleAsync_WithUnknownObjectId_ReturnsNotFoundAndDoesNotSave`, header `:132`) and `:195`
`var obj = floor.Objects.Should().ContainSingle().Which;` (method
`HandleAsync_WithCustomScale_StoresScaleOnObject`, header `:183`). The orchestrator's awk census
found the same two.

The same file also binds `var obj = floor.Objects[0];` at `:39` (method
`HandleAsync_ValidCommand_UpdatesObjectAndReturnsSuccess`). It is not an assertion; it is an
abbreviated name, renamed in this session for consistency with the other two (section 9 of C2 is not
involved; T1 owns it).

**Half-row STALE.** `TEST-030`'s second location,
`src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/AreaPersistenceDiagnosticTests.cs`,
does not exist at startRef: `git ls-files | grep -i diagnostic` prints nothing, and
`git log --diff-filter=D --format='%h %ad %s' --date=short -- '*AreaPersistenceDiagnosticTests.cs'`
prints `cf32061 2026-09-08 test(persistence): extract shared fixture; switch migrations to
Database.Migrate; add cardinality guard`. Both measured by the architect at spec time; tasks cannot
run git, so the register task quotes this attribution rather than re-measuring it.

Post-condition: PA prints nothing over `src/DungeonMaster.Tests`.

## 3. TM -- test-method census (naming, MASTER-144; counts for MASTER-137)

Save as `tests.pl`:

```perl
#!/usr/bin/perl
# Enumerate xUnit test methods. Output, tab-separated:
# file, header line, name, Fact|Theory, AAA flags (arrange act assert as 0/1), body line span, statement count
# statement count = body lines (after the header) that end in ';' (optionally followed by a // comment)
use strict; use warnings;
for my $file (@ARGV) {
	open my $fh, '<', $file or die "$file: $!";
	my @l = <$fh>; close $fh;
	s/\r?\n\z// for @l;
	my $i = 0;
	while ($i < @l) {
		if ($l[$i] =~ /^\s*\[(Fact|Theory)\b/) {
			my $kind = $1;
			my $j = $i; my $depth = 0;
			while ($j < @l) {
				my $t = $l[$j];
				if ($depth == 0 && $t =~ /^\s*public\s+(?:static\s+)?(?:async\s+)?(?:Task|void|ValueTask)\s+(\w+)\s*\(/) { last; }
				$depth += () = $t =~ /\[/g; $depth -= () = $t =~ /\]/g;
				$depth = 0 if $depth < 0;
				$j++;
			}
			last if $j >= @l;
			$l[$j] =~ /\s(\w+)\s*\(/; my $name = $1;
			my $k = $j; my $b = 0; my $started = 0;
			my ($arr, $act, $ass, $semi) = (0, 0, 0, 0);
			while ($k < @l) {
				my $t = $l[$k];
				$arr = 1 if $t =~ m{//\s*Arrange};
				$act = 1 if $t =~ m{//\s*Act\b};
				$ass = 1 if $t =~ m{//\s*Assert\b};
				$semi++ if $k > $j && $t =~ m{;\s*(?://.*)?$};
				my $o = () = $t =~ /\{/g; my $c = () = $t =~ /\}/g;
				$started = 1 if $o;
				$b += $o - $c;
				last if !$started && $t =~ /=>/ && $t =~ /;\s*$/;
				last if $started && $b <= 0;
				$k++;
			}
			printf "%s\t%d\t%s\t%s\t%d%d%d\t%d\t%d\n", $file, $j + 1, $name, $kind, $arr, $act, $ass, $k - $j + 1, $semi;
			$i = $k + 1;
			next;
		}
		$i++;
	}
}
```

Run: `git ls-files '*.cs' | xargs perl <scratch>/tests.pl > <scratch>/methods.tsv` from
`src/DungeonMaster.Tests`.

Controls (fixture `FxTests.cs`, CRLF): it reported all four test methods, including
`SentenceNameUnderAMultiLineAttribute` whose `[Theory(DisplayName = "x " + "y")]` attribute and
`[InlineData(` argument each span two lines; it reported `111` for the method carrying all three
markers and `000` for the one carrying none.

Second count of the same population: `git ls-files '*.cs' | xargs grep -c "^\s*\[\(Fact\|Theory\)" | awk -F: '{s+=$2} END {print s}'`
prints **664**, and `tests.pl` emits **664** rows. They agree. (The orchestrator's scan matched 662;
its 2 misses, multi-line attributes, are `App/Theme/ThemeHygieneTests.cs:203`
`EveryAllowListedFileExistsAndStillOffends` and
`Application/Common/Handlers/MutationOutcomeExtensionsTests.cs:38`
`ToResult_WithEachOutcome_MapsToTheContractedCategory`; `tests.pl` reports both.)

Naming census at startRef. "Underscore" means the method name contains `_`.

| Sub-tree | methods | with `_` | without `_` |
| --- | --- | --- | --- |
| `App/` | 146 | 58 | 88 |
| `AppCore/` | 152 | 152 | 0 |
| `Application/` | 207 | 207 | 0 |
| `Domain/` | 120 | 118 | 2 |
| `Infrastructure/` | 39 | 38 | 1 |

Command per row: `awk -F'\t' -v d='^<SubTree>/' '$1 ~ d' methods.tsv | wc -l`, and the same with
`&& $3 ~ /_/` / `&& $3 !~ /_/`. 146+152+207+120+39 = 664.

Post-conditions for MASTER-144 (Option A, see C2):
- **N1**: `awk -F'\t' '$1 ~ /^App\// && $3 ~ /_/' methods.tsv | wc -l` prints `0`.
- **N2**: `awk -F'\t' '$1 !~ /^App\// && $3 !~ /_/' methods.tsv | wc -l` prints `0`.

Outside `App/`, the underscore-separated part count, `awk -F'\t' '$1 !~ /^App\// {print split($3,a,"_")}' methods.tsv | sort | uniq -c`:
1 part 3, 2 parts 70, 3 parts 440, 4 parts 5 (518 total). Two-part names (`Method_Expected`, no
scenario) and four-part names (two scenario clauses) are **not** renamed in this session: the row's
decision is to keep the scheme used outside `App/`, and C3 states that scheme with the scenario part
optional so it describes the code truthfully.

## 4. AAA -- Arrange/Act/Assert structure checker (MASTER-137)

Save as `aaa.pl`:

```perl
#!/usr/bin/perl
# AAA checker. For every [Fact]/[Theory] method whose body holds two or more statement lines
# (lines ending in ';', optionally followed by a // comment), prints one line per violation:
#   <file>:<header line>:<method>: <reason>
# Rules: (1) a '// Act' marker line and a '// Assert' marker line are present;
# (2) markers appear in the order Arrange < Act < Assert;
# (3) if any statement line precedes the '// Act' marker, a '// Arrange' marker is present;
# (4) every marker line other than the first marker in the body is immediately preceded by a blank line.
# A marker line is a line whose first non-blank text is // Arrange, // Act or // Assert (word-bounded).
# Methods with fewer than two statement lines are skipped (single-statement bodies are exempt).
use strict; use warnings;
my $violations = 0;
for my $file (@ARGV) {
	open my $fh, '<', $file or die "$file: $!";
	my @l = <$fh>; close $fh;
	s/\r?\n\z// for @l;
	my $i = 0;
	while ($i < @l) {
		if ($l[$i] =~ /^\s*\[(Fact|Theory)\b/) {
			my $j = $i; my $depth = 0;
			while ($j < @l) {
				my $t = $l[$j];
				last if $depth == 0 && $t =~ /^\s*public\s+(?:static\s+)?(?:async\s+)?(?:Task|void|ValueTask)\s+\w+\s*\(/;
				$depth += () = $t =~ /\[/g; $depth -= () = $t =~ /\]/g;
				$depth = 0 if $depth < 0;
				$j++;
			}
			last if $j >= @l;
			$l[$j] =~ /\s(\w+)\s*\(/; my $name = $1;
			my $k = $j; my $b = 0; my $started = 0;
			my (%m, @order, $firstStmt, $semi, @noblank);
			$semi = 0;
			while ($k < @l) {
				my $t = $l[$k];
				if ($t =~ m{^\s*//\s*(Arrange|Act|Assert)\b}) {
					my $w = $1;
					$m{$w} //= $k;
					push @noblank, "$w at line " . ($k + 1) if @order && $l[$k - 1] !~ /^\s*$/;
					push @order, $w;
				}
				elsif ($k > $j && $t =~ m{;\s*(?://.*)?$}) {
					$semi++;
					$firstStmt //= $k;
				}
				my $o = () = $t =~ /\{/g; my $c = () = $t =~ /\}/g;
				$started = 1 if $o;
				$b += $o - $c;
				last if !$started && $t =~ /=>/ && $t =~ /;\s*$/;
				last if $started && $b <= 0;
				$k++;
			}
			if ($semi >= 2) {
				my @r;
				push @r, 'no // Act marker' unless defined $m{Act};
				push @r, 'no // Assert marker' unless defined $m{Assert};
				if (defined $m{Act} && defined $m{Assert} && $m{Act} > $m{Assert}) { push @r, 'Act after Assert' }
				if (defined $m{Arrange} && defined $m{Act} && $m{Arrange} > $m{Act}) { push @r, 'Arrange after Act' }
				if (defined $m{Act} && !defined $m{Arrange} && defined $firstStmt && $firstStmt < $m{Act}) { push @r, 'statement before // Act with no // Arrange' }
				push @r, "no blank line before $_" for @noblank;
				for (@r) { printf "%s:%d:%s: %s\n", $file, $j + 1, $name, $_; $violations++ }
			}
			$i = $k + 1;
			next;
		}
		$i++;
	}
}
exit($violations ? 1 : 0);
```

Run: `git ls-files '<SubTree>/*.cs' | xargs perl <scratch>/aaa.pl` from `src/DungeonMaster.Tests`.
It exits 1 when it prints anything (xargs then reports 123); the printed lines are the measurement.

Controls (fixture `AaaTests.cs`, CRLF, six methods), all run: `Good_Full` (Arrange/Act/Assert, blank
lines) -- silent; `Good_NoArrangeNeeded` (Act then Assert, no setup) -- silent; `Exempt_Single` (one
statement) -- silent; `Bad_NoMarkers` -- `no // Act marker` and `no // Assert marker`;
`Bad_NoBlankLine` (`// Act` directly under the Arrange statement) -- `no blank line before Act`;
`Bad_SetupWithoutArrange` -- `statement before // Act with no // Arrange`.

**Positive control on the reference sub-tree:** over `Application/` (the handler tests whose
structure this row propagates) it prints 3 lines for 3 of 207 methods:
`ApplicationServiceRegistrationTests.cs:102` (no `// Act`), `AddFloorCommandHandlerTests.cs:18` and
`RemoveFloorCommandHandlerTests.cs:18` (no blank line before `// Act`). `Application/` is not in this
session's MASTER-137 population (it is the sub-tree that has the structure); these 3 are reported,
not worked.

startRef, distinct methods flagged (`... | cut -d: -f1-3 | sort -u | wc -l`):

| Sub-tree | methods | flagged | single-statement (exempt) |
| --- | --- | --- | --- |
| `AppCore/` | 152 | 126 | 26 |
| `Domain/` | 120 | 63 | 5 (and 52 already conforming) |
| `Infrastructure/` | 39 | 16 | 0 (and 23 already conforming) |

Single-statement figures are from `methods.tsv` (section 3): column 7 `<= 1` and markers absent.

**Population of MASTER-137 in this session: the 205 flagged methods in `AppCore/`, `Domain/`,
`Infrastructure/`.** `AppCore/Map/` is where `TEST-022`'s `Application/Common/Map/` now lives; the
rest of `AppCore/` (13 `*Tests.cs`, `git ls-files 'AppCore/*Tests.cs' | wc -l`) is the same kind of
code and is included. `App/` is excluded: `TEST-022` does not name it, and its facts are scans over
source files, not arrange-act-assert unit tests.

Post-condition **A0**: `aaa.pl` prints nothing over each of `AppCore/`, `Domain/`, `Infrastructure/`.

## 5. DOC -- test files carrying no `/// <summary>` (MASTER-143)

```
git ls-files '*Tests.cs' | xargs grep -L "/// <summary>" | wc -l
```

Second predicate (anchored, different tool):
`git ls-files '*Tests.cs' | xargs perl -0777 -ne 'print "$ARGV\n" unless m{^\s*///\s*<summary>}m' | wc -l`.

Controls (fixtures, CRLF): `DocYes.cs` holding `\t/// <summary>` is not listed; `DocNo.cs` holding
`\t// <summary>` (two slashes, near-miss) is listed.

startRef: both print **18**, of **109** `*Tests.cs` (`git ls-files '*Tests.cs' | wc -l`); by sub-tree
`AppCore` 10, `Domain` 8. No task in this session adds or removes a `*Tests.cs` file, and no task
adds or removes a doc comment to close MASTER-143 (amendment A6, escalation E3: T5's split facts
added 7 `/// <summary>` blocks, all in files that already had one), so the figure is expected to be
18 of 109 at the end as well; the register task re-measures
it rather than assuming so. The row's "24 of 45" was a different population (45 files at the audit
date).

## 6. B -- line-ending byte census

```
tr -cd '\r' < <file> | wc -c
tr -cd '\n' < <file> | wc -c
```

and, for lone CRs, `perl -0777 -ne '$cr=()=/\r/g;$lf=()=/\n/g;$p=()=/\r\n/g;printf "CR=%d LF=%d CRLF=%d loneCR=%d\n",$cr,$lf,$p,$cr-$p' <file>`.
Control: on `printf 'a\r\nb\rc\n'` the Perl form prints `CR=2 LF=2 CRLF=1 loneCR=1` (run).

A tracked file passes when CR = LF = CRLF and loneCR = 0. startRef: all 123 `.cs` files under
`src/DungeonMaster.Tests` pass (Perl form over `git ls-files 'src/DungeonMaster.Tests/*.cs'`,
`nonCRLF=0`; `git ls-files --eol` prints `i/lf w/crlf` for all 123). Also CRLF with loneCR 0:
`src/DungeonMaster.App.Core/Map/MapDrawingSession.cs` (re-measure before injecting),
`src/DungeonMaster.App.UnitTests/Theme/DmThemeResolutionTests.cs` 344,
`src/DungeonMaster.App/Resources/Theme/Controls.Base.xaml` 157,
`src/DungeonMaster.App/Resources/Theme/Tokens.xaml` 725,
`src/DungeonMaster.App/Services/Platform/PlatformSeams.Unsupported.cs` 60,
`docs/audit/00-master-index.md` 691, `docs/audit/05-tests.md` 2229.

Never `grep $'\r'` and never `grep -c` with a `\r` in the pattern.

## 7. R -- register row cell count, and the amendment-block census

```
tr -d '\r' < docs/audit/00-master-index.md | awk -v L=<line> 'NR==L{ s=$0; n=0; for(i=1;i<=length(s);i++){ c=substr(s,i,1); if(c=="|" && substr(s,i-1,1)!="\\") n++ } print n }'
```

startRef: prints **11** (10 cells) on lines 313, 314, 316, 325, 327, 331, 332 (run) and on 368 (run).
Control: line 368, a closed row whose cell is full of escaped `\|`, also prints 11.

Amendment census on `docs/audit/05-tests.md`:
- openers `tr -d '\r' < docs/audit/05-tests.md | grep -c '^\*\*\[AMENDED'` -> **38** at startRef;
- closers `tr -d '\r' < docs/audit/05-tests.md | grep -c '\]\*\*$'` -> **38** at startRef. The
  closer form is `**]**` at line end (e.g. `:115`, `:161`).

## 8. Build and test

Run from anywhere, absolute paths:

- `dotnet build C:/dm-wt/20261104-1726-test-hygiene-sweep/src/DungeonMaster.sln` -- quote the three
  summary lines `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`. Never `grep -icE "warning|error"`,
  which prints 2 on a clean `dotnet build`.
- `dotnet test C:/dm-wt/20261104-1726-test-hygiene-sweep/src/DungeonMaster.sln --no-build` -- quote
  each assembly's summary line. A run that prints no count is "not runnable", not a pass.
- Baseline (orchestrator, at startRef): build 0 warnings 0 errors; DungeonMaster.Tests **943**
  passed, App.UnitTests 187, App.Tests 123.
- `GenerateDocumentationFile` is set nowhere (`git grep -l GenerateDocumentationFile -- '*.props' '*.csproj' '*.targets' | wc -l` -> 0),
  so the build does not validate `<see cref="..."/>` targets. A renamed method referenced by a
  `cref` is checked by section 10, not by the build.
- **Every task that runs either command shares one `obj/`/`bin/` tree. Two tasks must never run
  them at the same time;** the manifest orders every building task.

Expected DungeonMaster.Tests count: renames and AAA edits change nothing. MASTER-139 splits add
**+2** (T2) and **+7** (T5), per C5. End of session: 943 + 2 + 7 = **952**, plus the E1 guard facts
(amendment A1: a precondition assertion that AAA markers would leave under `// Arrange` is removed
where a dedicated fact asserts it, otherwise moved into a guard fact of its own; T2 adds 1). App.UnitTests and
App.Tests are unchanged (187, 123).

## 9. Editing hazards (bind every task and every reviewer who touches a file)

- No `sed -i`, ever (rewrites CRLF to LF). No `Write` on an existing file (emits LF).
- `Edit` keeps CRLF terminators but strips trailing whitespace from `new_string`; never end a
  `new_string` on whitespace. `Edit` normalises embedded lone CRs; every file named in section 6 has
  loneCR 0, so this is safe here. Re-run B after editing.
- C# indentation is tabs.

## 10. REF -- references to a renamed test method

For each renamed or split-away method name `<N>`, from the repository root of the worktree:

```
grep -rnw --include='*.cs' --include='*.xaml' --include='*.md' --include='*.json' --include='*.props' --include='*.csproj' -- '<N>' src
```

A task may not run `git grep`; `grep -rnw ... src` covers the same tracked population under `src`
apart from `obj/`/`bin/` output, which a reviewer discounts. Post-condition: every hit is the new
name's own declaration or a reference that was updated to the new name; no hit carries the old name.

startRef census of the 61 names C2 renames (`git grep -n -w -F -f renamed.txt`, declarations
excluded), hits **inside `src/`**:

| Old name | Site |
| --- | --- |
| `EveryKeyDmThemeResolves_IsDefinedInTheThemeDictionaries` | `src/DungeonMaster.App.UnitTests/Theme/DmThemeResolutionTests.cs:92` (doc comment) |
| `BorderBaseStrokeShapeRadius_EqualsDmRadiusDefault` | `src/DungeonMaster.App/Resources/Theme/Controls.Base.xaml:114` (XML comment) |
| `BorderBaseStrokeShapeRadius_EqualsDmRadiusDefault` | `src/DungeonMaster.App/Resources/Theme/Tokens.xaml:511` (XML comment) |
| `AppSource_NoLineStatesADiscardedTask` | `src/DungeonMaster.App/Services/Platform/PlatformSeams.Unsupported.cs:22` (doc comment, fully qualified) |
| `EveryReferencedThemeKey_IsDefinedUnderResources` | `src/DungeonMaster.Tests/App/Theme/ThemeTokenParityTests.cs:96` (`<see cref>`) |

Hits **outside `src/`** are historical records and are **not** rewritten: `.dir-sdlc/**`,
`.sir/sessions/**`, `docs/audit/05-tests.md:1862`, `docs/audit/00-master-index.md:235`. The names
MASTER-139 splits (`TheIconFontCmapParsesToPrivateUseGlyphs`,
`EveryDmFontIconTextIsInTheIconFontsCmap`, `EveryIconOnlyControlHasASemanticDescription`,
`EveryRegisteredExceptionStillExistsAndStillOffends`, `PlaceVertex_FirstVertex_ReturnsAdded`) have no
reference in `src` other than their own declaration (`git grep -n -w <name> -- src`, one line each).
