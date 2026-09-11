# Addendum to the T5 evidence handover — authoring-commit observations

Written by the orchestrator after T5's round-1 review, to settle what T5 correctly declined to guess.

**Why this exists.** T5 wrote `UNDETERMINED` for five classifications and stated why: deciding whether a
claim was `DRIFT` (true when written, overtaken since) or `MEASUREMENT ERROR` (false when written) requires
observing the repository **at the authoring commit**, and DIR bars tasks from invoking git. Its reviewer
checked and confirmed that neither `measurement.md` nor `handover.md` carried any audit-date observation, so
the five were genuinely unreachable from the evidence T5 held. That was the correct call.

The orchestrator is not barred from read-only git. Every observation below was produced by the command shown,
run in `C:/dm-wt/6b-iii`.

**The audit commit is `413be78`, dated 2026-08-17.** Confirmed:

```
$ git show -s --format='%h %ad  %s' --date=short 413be78 8bb2d79
413be78 2026-08-17  build: standardize tab indentation and enforce via build
8bb2d79 2026-08-18  docs: reconcile audit tracking after Session 1 completion
```

---

## 1. The `contradicts` finding — resolved, verdict unchanged, anchor corrected

`measurement.md:114-115` claimed the `TEST-032` cited line was wrong when written and cited **`8bb2d79`** as
showing it. `8bb2d79` is dated **2026-08-18** — one day *after* the audit, on the wrong side of the very
boundary this document's `DRIFT` / `MEASUREMENT ERROR` split turns on. The reviewer was right to call it: that
commit cannot establish what was true when the record was written.

**The orchestrator's citation was wrong; the conclusion was not.** At the actual audit commit:

```
$ git show 413be78:src/DungeonMaster.Tests/DungeonMaster.Tests.csproj | grep -n "PackageReference"
9:    <PackageReference Include="coverlet.collector" />
10:    <PackageReference Include="FluentAssertions" />
11:    <PackageReference Include="Microsoft.NET.Test.Sdk" />
12:    <PackageReference Include="NSubstitute" />
13:    <PackageReference Include="xunit" />
14:    <PackageReference Include="xunit.runner.visualstudio" />
```

coverlet is at **line 9** on the audit date; line 11 is `Microsoft.NET.Test.Sdk`. `TEST-032`'s cited location
`DungeonMaster.Tests.csproj:11` was therefore **wrong when written**.

**Action:** keep the `MEASUREMENT ERROR` classification, and re-anchor it on `413be78` with the command above.
Remove the `8bb2d79` citation and replace it. Reviewer's ranked option 1, adopted.

---

## 2. `TEST-001`'s three `UNDETERMINED` classifications — all resolve to `DRIFT`

Each was **accurate when written** and has been overtaken since.

**(a) "`src/DungeonMaster.Tests/Application/` contains `Common/Map/` and `Features/Dungeons/` and nothing else"**

```
$ git ls-tree -r --name-only 413be78 -- src/DungeonMaster.Tests/Application/Common/ | sed 's|.*/Common/||' | cut -d/ -f1 | sort -u
Map
$ git ls-tree -r --name-only 413be78 -- src/DungeonMaster.Tests/Application/Features/ | sed 's|.*/Features/||' | cut -d/ -f1 | sort -u
Dungeons
```

True at `413be78`. → **DRIFT.** Today `Common/` holds `Behaviors`, `Dispatch`, `Handlers`, `Results` and no
`Map` at all.

**(b) "The four validator test classes"**

```
$ git ls-tree -r --name-only 413be78 -- src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/ | wc -l
4
```
The four were `AddAreaCommandValidatorTests.cs`, `CreateDungeonCommandValidatorTests.cs`,
`UpdateAreaCommandValidatorTests.cs`, `UpdateDungeonCommandValidatorTests.cs`. True at `413be78`. → **DRIFT.**
There are 14 concrete validator test classes today.

**(c) The anchor `AddAreaCommandValidatorTests.cs:17`**

```
$ git show 413be78:src/.../Validators/AddAreaCommandValidatorTests.cs | sed -n '15,19p'

		// Act
		var result = _validator.Validate(command);

		// Assert
```

Line 17 was exactly the `_validator.Validate(command)` call the record cites it for. True at `413be78`.
→ **DRIFT.** The anchor is dead today because `8784326` reshaped the file; line 17 now lands on
`protected override string NamePropertyName`.

---

## 3. `TEST-032`'s two `UNDETERMINED` classifications — both resolve to `DRIFT`

**(d) "`coverlet.collector` 6.0.4 is referenced by the test project" (singular)**

```
$ git ls-tree -r --name-only 413be78 -- src/DungeonMaster.App.Tests      | wc -l
0
$ git ls-tree -r --name-only 413be78 -- src/DungeonMaster.App.UnitTests  | wc -l
0
```

Neither project existed at the audit commit, so `DungeonMaster.Tests` was the **only** test project and the
singular was correct. → **DRIFT.** The scope tripled afterwards, as later sessions added the two App-side
projects; the record did not under-count, it was overtaken.

**(e) "nothing pins the parallelism settings the suite currently relies on"**

```
$ git ls-tree -r --name-only 413be78 | grep -ci "MauiHostCollection\|AppTestCollection\|MauiHostFixture"
0
```

None of the three existed at the audit commit. The claim was **true when written**. → **DRIFT.** The pinning
arrived later, in the form the spike settled on — `[CollectionDefinition]` plus explicit membership — which is
also why the record's own suggested `[assembly: CollectionBehavior]` remedy was never the thing that fixed it.

---

## What T5 should change

Five `UNDETERMINED` classifications become measured, and one mis-anchored classification gets its real
evidence. **Nothing else in the diff changes** — no verdict flips, no correction is added or removed, and the
`MEASUREMENT ERROR` classifications in `TEST-020` are untouched.

Note for the amendment's own wording: **(d) and (e) are worth stating as drift rather than as the record being
sloppy.** Both read today like the auditor missed something; measured against the commit that wrote them, both
were accurate and the repository moved. That is the same correction this session already had to make about
itself — the 6b-ii handoff described the audit as "systematically under-counting" when it had in fact been
overtaken by 28 commits.
