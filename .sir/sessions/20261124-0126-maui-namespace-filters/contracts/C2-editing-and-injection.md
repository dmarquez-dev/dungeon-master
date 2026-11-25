<!-- frozen: 20261124-0126-maui-namespace-filters, Gate 2 -->

# C2 - Editing rules and the injection protocol (T1, T2)

## 1. Harness instruction that this contract overrides

The harness's standing auto-mode note permits making "small, mechanical file changes with sed,
heredocs, or short scripts". **That note conflicts with this contract and this contract wins.**
Every file either task owns is CRLF with TAB indentation. Measured at 452b18f in the worktree:

| file | CR | LF | CRLF pairs | lines starting with a space |
|---|---|---|---|---|
| src/DungeonMaster.Tests/App/Theme/ThemeSource.cs | 759 | 759 | 759 | 0 |
| src/DungeonMaster.Tests/App/Theme/ThemeSourceTests.cs | 440 | 440 | 440 | 0 |
| src/DungeonMaster.Tests/App/Theme/ImplicitStyleTests.cs | 450 | 450 | 450 | 0 |
| src/DungeonMaster.Tests/App/Theme/ThemeDictionaryTests.cs | 474 | 474 | 474 | 0 |
| src/DungeonMaster.Tests/App/Theme/ConfirmButtonStateParityTests.cs | 290 | 290 | 290 | 0 |
| src/DungeonMaster.Tests/App/Theme/IconButtonStyleParityTests.cs | 461 | 461 | 461 | 0 |
| src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs | 566 | 566 | 566 | 0 |

Commands, per file `f`: CR `tr -cd '\r' < f | wc -c`; LF `tr -cd '\n' < f | wc -c`; CRLF pairs
`perl -0777 -ne 'print scalar(() = /\r\n/g)' f`; space-indented lines `grep -c '^ ' f`.
Control: the fixture `printf 'a\r\nb\rc\n'` gives CR=2 LF=2 CRLF=1, so CR=LF alone does not prove
CRLF and the three-way equality is the check. Control for the space count: it prints 20 on
`src/DungeonMaster.App/Platforms/Windows/App.xaml` (space-indented).

Rules:

- Edit existing files with the **Edit tool only**. Never `sed -i` and never the Write tool on an
  existing file: both rewrite the whole file as LF.
- Edit strips trailing whitespace from `new_string`: never end a `new_string` on a tab or space;
  key whitespace-bearing edits on a line's first text character.
- Indent with TABs, inside raw string literals too.
- No `.sir/` path anywhere in an owned file (comment, doc, string, test name).
- No git command of any kind.

## 2. Injection protocol

Each injection replaces exactly one filter site's selection test with the comparison that site
made at 452b18f, spelled fully qualified so it resolves wherever it lands:
`global::DungeonMaster.Tests.App.Theme.ThemeSource.QualifiedElementName(element.Name) == <name>`.
The identifiers `element` (the lambda or method parameter holding the element) and, where a
site takes the name as a parameter, `localName` are fixed by the criteria so the injection text
compiles as given.

For every injection, in order, one at a time:

1. Copy the file to be injected into your own scratch directory.
2. Apply the injection at the one named site with Edit.
3. Run the criterion's filtered command, which rebuilds:
   `dotnet test C:/dm-wt/20261124-0126-maui-namespace-filters/src/DungeonMaster.Tests/DungeonMaster.Tests.csproj --filter "FullyQualifiedName=<fact>"`.
   Red means the summary line reports `Failed: 1` and `Total: 1` (vstest pads the numbers). A
   build error is not red. "No test matches the given testcase filter" exits 0 and is not green
   either; a correct filter run always prints a `Total:` line.
4. Revert with Edit and read the file back against the scratch copy (`cmp` prints nothing,
   exit 0). No second injection goes in before this read-back.
5. Run the same filtered command **on its own**, not batched with any other tool call, and
   observe `Failed: 0`, `Passed: 1`, `Total: 1`.
6. Report the window: site (file:line at the time), injected text, the step-3 and step-5 summary
   lines, and the read-back result.

Population: an exact `FullyQualifiedName=` filter selects only the fact it names (checked at
Gate 2 on `ImplicitStyleTests.NestedStyleSettersAreAttributedToTheDeclaringStyle`: `Total: 1`;
the prefix `...ImplicitStyleTests.NestedStyleSettersAreAttributed` selected nothing). Two clauses joined by `|` select both
(checked: `Total: 2`).

## 3. Schedule

T1 and T2 build the same project (`src/DungeonMaster.Tests` obj/bin, and the App projects it
references) and T1 injects into `ThemeSource.cs`, which T2's tests call. T2 depends on T1; they
never run concurrently.
