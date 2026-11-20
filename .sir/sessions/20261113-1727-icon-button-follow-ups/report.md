sir-sdlc 1.0.36 — Gate 3 report — Small

This is the final render. Gate 3 took two renders. **Render 1** carried one decision (C5 contradicted itself) and two advisories. You answered:
- C5: accept the delivered wrap and amend C5 (A1, authorised by E1);
- advisory 1 (an unmeasured claim at `IconButtonStyleParityTests.cs:25-26`): fix now;
- advisory 2 (implicit-style side not demonstrated): drop;
- the approval you gave in the same prompt was superseded by the fix-now and is re-asked over this diff.

One revision round followed, then one re-review. **Superseded verdicts from render 1**, in the reviewer's words:
- C5: "not met. Words are unchanged (190 = 190 whitespace tokens, Python), and the file outside the comment is identical after LF-to-CRLF normalisation of the `git show` blob (sha256 match). But `Controls.Base.xaml:108` still breaks after `which has been`, which the criterion forbids."
- Render-1 advisory 1, now closed: "`IconButtonStyleParityTests.cs:25-26` adds a claim that nobody measured: 'Any setter in those states outranks a local value the same way'. Only Opacity was measured."
- Render-1 advisory 3, settled before render 1: "C6's build was incremental". The orchestrator's `dotnet build src/DungeonMaster.sln --no-incremental` gave 0 Warning(s), 0 Error(s).

**Decisions needed:** none — the one escalation (E1, C5's wording) was answered at this gate, with no deferred action reaching the backlog.

## 2. Acceptance criteria

| Criterion | Verdict | Evidence (re-review) | Tag |
|---|---|---|---|
| C1 full setter-set parity per shared state | met | Test code unchanged apart from the comment, and `Controls.Base.xaml` still sha256 9068f8a0e6ba…, so the earlier runs hold: (a) BackgroundColor in PointerOver failed `EveryDmIconButtonSetterOtherThanMinimumWidthEqualsTheImplicitButtonSetter` ("found one extraneous item \"BackgroundColor\""); (b) Pressed 0.6 failed it ("to be 0.7 ... but found 0.6"). | injected defect; reviewer-reproduced (a) |
| C2 Disabled is exactly one Opacity 0.35 setter | met | `:272-284` unchanged. (c) BackgroundColor in Disabled failed `DmIconButtonDisabledVisualStateSetsOpacityThirtyFivePercent` ("{\"Opacity\", \"BackgroundColor\"} contains 1 item(s) too many"). | reviewer-reproduced |
| C3 vacuity guard and earlier coverage kept | met | The scan fact's code is unchanged at `:132-161`. v1 (PointerOver emptied) and v2 (group renamed) turned the scan fact red, and the four 20261112-2120 C4 injections (a)-(d) each turned a fact red. | injected defect |
| C4 local 0.35 reads as removed history | met | The removed-history wording is at `:23-24`, `:73-74` and `:283-284`, and the mechanism statement stays at `:22`. The new sentence at `:25-27` narrows a claim this same change made and alters no comment that existed at c816f38. | — |
| C5 header comment rewrapped (amended, A1) | met | `Controls.Base.xaml:106-110` widths are 93/95/93/95/26, none past the 95 columns at c816f38; there is no break after `The style`; the words and the bytes outside the comment are unchanged, sha256 9068f8a0…. | — |
| C6 build 0/0, all tests pass | met | `dotnet test src/DungeonMaster.sln`: DungeonMaster.Tests 1004/1004, App.UnitTests 217/217, App.Tests 146/146, and the class still has 4 facts. The build is the orchestrator's `--no-incremental` 0/0. The criterion's 999/143 baseline was the orchestrator's error: those figures came from session 20261112-0031. `git diff f09f2fa c816f38 -- src` adds 4 + 1 facts in DungeonMaster.Tests and 3 in App.Tests, which gives 1004 and 146. | — |
| C7 line endings, BOM, indentation, byte-clean reverts | met | Test file: CRLF 461 = LF 461 = CR 461, no BOM, no space-led lines, no trailing whitespace. Controls.Base.xaml: 209 = 209 = 209, no BOM, 0 tab bytes, sha256 equal to the injection snapshot. All 9 implementer reverts and both reviewer reverts matched, and the one-byte controls differed. | — |

**Human checks:** none for this session. The open human-check rows in `.sir/backlog.md` are:
- the UiTests chevron measurement (20261112-0153-floor-name-tooltip);
- the Appium drag-handle run and the floor-name DIU measurement (2026-09-16-floor-sidebar-keyboard-route).

They need the Appium server or a measurement pass, so no command here puts them in front of you, and they stay open.

## 3. What ran

- T1 was implemented by `sir-test-engineer` (opus), with one fix-now revision at Gate 3.
- `sir-reviewer` (opus) reviewed it twice: 1 revision round.
- Amendment A1 (E1) on C5: "instead of breaking after `which has been` and `The style`" became "(95 columns at c816f38) instead of breaking after `The style`".
- Change surface: `git diff --stat HEAD -- src/DungeonMaster.Tests/App/Theme/IconButtonStyleParityTests.cs src/DungeonMaster.App/Resources/Theme/Controls.Base.xaml` gives 2 files changed, 104 insertions, 51 deletions.
- `git status --short` over the same paths shows both files modified and no untracked file. The full diff is `git diff HEAD -- <those two paths>`.

## 4. Advisory findings

**test and CI concerns (1):** C1's "either style" was demonstrated only on the DmIconButton side, because `Implicit.xaml` was outside the file set. The code is symmetric at `:238-248`. Dropped at this gate. Source: the in-thread review.
