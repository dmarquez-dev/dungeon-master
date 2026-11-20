# Handoff — Session 6b-ii, "Fixtures and de-duplication"

Written 2026-09-08, after Session 6b-i merged into `chore/repo-audit`. **Every figure below was
measured on 2026-09-08 with the command shown, in the main checkout.** Where something is inherited
rather than measured, it says so.

> **Read this first.** 6b-i found **eighteen** defective citations, counts and prescribed commands in
> its own frozen artifacts, and **four injections that looked discriminating and were inert**. Not one
> was caught by reading. Every single one was caught by an agent running the check and looking at what
> came back — and **three of the eighteen were the orchestrator's**, including one made at a gate to
> justify a scope change. Treat this document the same way: a starting point for measurement, not a
> substitute for it.

## Where things stand

6b-i is **merged and pushed**. Its fixed points: the merge commit is `ce4113d` and the artifact commit
on top of it is `6eda0a9`. **The branch tip is deliberately not quoted here** — the previous handoff
was corrected once by a commit that invalidated its own HEAD line. Run `git log --oneline -1` and
`git rev-parse --short origin/chore/repo-audit`; they should agree, and the tip should be a descendant
of `6eda0a9`.

The worktree was removed; branch `dir/2026-09-02-app-sub-suite-rebuild` was kept, because twelve
review files cite it. There is still **no project-level `.dir-sdlc/log.md`** — the session log is
`.dir-sdlc/sessions/2026-09-02-app-sub-suite-rebuild/log.md`.

Main checkout, verified there and not only in the worktree:

```
dotnet build src/DungeonMaster.sln          Build succeeded.  0 Warning(s)  0 Error(s)
dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj              Passed: 695
dotnet test src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj      Passed:  77
dotnet test src/DungeonMaster.App.UnitTests/...UnitTests.csproj             Passed:  23
dotnet build src/DungeonMaster.App.UiTests/...UiTests.csproj   (not in the sln)  0/0
```

**There are now three test assemblies in the solution and a fourth outside it.** There is still no
aggregate figure. Any criterion citing a count must cite it **per assembly with the command that
produced it**.

## What 6b-i changed about what 6b-ii can do

**The application is now reachable from a plain `net10.0` test project, with no MAUI host and no
WinUI.** This is the single biggest change to your evidence surface and it is already built,
committed and reviewed.

- `DungeonMaster.App` gained a third, platform-less target framework: `net10.0`, alongside its two
  platform TFMs, with `OutputType` conditioned so the bare TFM builds a library. Cost was three csproj
  lines plus `src/DungeonMaster.App/Services/Platform/PlatformSeams.Unsupported.cs`.
- `src/DungeonMaster.App.UnitTests` is a plain `net10.0` xunit project that **references
  `DungeonMaster.App` directly**. `Fixtures/AppFixture.cs` supplies a constructed `App` — so the seven
  merged theme dictionaries resolve — plus a dispatcher provider. Join with
  `[Collection(AppTestCollection.Name)]`; create MAUI instances through `AppFixture.Construct<T>`.
- Measured there: `new BoxView()` constructs; `new AppShell()` constructs; a XAML `ContentView`
  constructs with `InitializeComponent` running and its `x:Name`d fields populated; and
  **`BindableProperty` metadata reads back correctly by reflection** — `PropertyName`, `ReturnType`,
  `DeclaringType`, `DefaultValue`.
- **`DungeonMaster.Tests` deliberately has no reference to `DungeonMaster.App` and no MAUI global
  usings, and that is an invariant.** Acquiring the reference imports 18 MAUI global usings and
  produced **31 `CS0104` errors across 3 files** when measured. Anything needing App types goes in
  `DungeonMaster.App.UnitTests`. Three greps enforce it; T1's criterion 11 has them.

**Your ten entries are Domain, Application and Infrastructure test work and probably need none of
this.** It is here because it is the thing most likely to be useful and least likely to be guessed.

**Two limits, both real:**

1. **The MAUI host recipe is off Microsoft's documented path.** Its supported shape is MSTest under
   Microsoft.Testing.Platform, and `testhost.pri` is coupled to the runner's executable name — **xunit
   v3 defaults to MTP and would break it**. Unchanged from 6a; restated because it did not bite this
   session and so nobody re-measured it.
2. **The Appium layer applies pending EF migrations to the user's real database on every run.**
   `MauiProgram.DatabasePathOverride` is an in-process static (`MauiProgram.cs:31`) with no
   out-of-process seam, so the mechanism 6a built to keep tests off the real database **does not reach
   an out-of-process app**. Measured by hashing the file before and after a read-only test: 9 bytes at
   `0x1fee`–`0x1ff9`, an `__EFMigrationsLock` timestamp, from `MauiProgram.cs:193`. No domain data is
   written today. **A UI test that creates anything would write real domain data.** In the backlog.

## Your scope — 10 entries

From `docs/audit/00-master-index.md`, Session 6b's list minus 6b-i's eleven and 6b-iii's five.
**Count it yourself before quoting it.** The arithmetic: Session 6b held 26; 6b-i closed 11
(`008`, `028`, `040`, `063`, `068`, `100`, `101`, `102`, `109`, `138`, `149`); 6b-iii holds 5
(`020`, `086`, `070`, `155`, `167`). 26 − 11 − 5 = **10**, and the three sets partition the 26 exactly
with no overlap — verified by set arithmetic, not by reading.

`MASTER-014`, `041`, `042`, `064`, `065`, `066`, `067`, `069`, `085`, `127`.

| Entry | Sev / effort | One line |
|---|---|---|
| `MASTER-014` | S2 / S | Every persistence test builds its schema with `EnsureCreated`, so no test exercises a migration |
| `MASTER-065` | S3 / S | The SQLite in-memory fixture is copy-pasted verbatim into four Infrastructure test classes |
| `MASTER-085` | S3 / M | Sixteen handler test classes repeat an identical fixture constructor and an identical not-found test |
| `MASTER-064` | S3 / S | The four validator test classes are identical except for the command type |
| `MASTER-067` | S3 / S | Five geometry facts and five snap-zone facts differ only in their data |
| `MASTER-066` | S3 / S | Four tests assert `Received(1)` on a save that happens on a path where nothing changed |
| `MASTER-069` | S3 / S | The two query-handler tests assert only that the stubbed DTO came back |
| `MASTER-042` | S3 / XS | `AreaPersistenceDiagnosticTests` is a committed debugging session with two subsumed tests |
| `MASTER-041` | S3 / XS | Task-returning substitutes are given explicit `Returns(Task.CompletedTask)` stubs |
| `MASTER-127` | S4 / XS | Four non-sealed test classes implement `IDisposable` with no `Dispose(bool)` overload |

**`MASTER-065` and `MASTER-014` are named in the index as one unit** — the shared fixture and the
`EnsureCreated` schema are the same four files.

**Explicitly not yours:** 6b-iii's five, and `MASTER-161`, `163`, `164`, `165`, `166`, which were left
out of Session 6 entirely at its Gate 1 as sidebar and UI defects. Do not let a suggested-home line
draw them in.

**`MASTER-066` and `MASTER-069` are the two worth reading first.** Both are entries about tests that
assert something weaker than their names claim — `Received(1)` on a path where nothing changed, and a
query-handler test that only proves the stub returned. That is the same family 6b-i spent itself on,
and the section below is about how to tell.

## What 6b-i learned about proving a test can fail

This is the most transferable thing the session produced, and it cost real time to find.

**1. Run every injection before you write it into a criterion or into evidence.** Four injections
looked discriminating and were inert:

- Removing an entry from a conversion table left the fact **green at 10/10** — no token in the app bound to that type.
- Blinding a literal scanner left the fact **green**, because the file's own prose comments carried the literals.
- Shifting a `cmap` array left all format-4 tests **green**, because the parser tested the derived glyph id only for `!= 0`.
- A prescribed injection would not compile at all: `if (true) { return …; }` gives `CS0162` under `TreatWarningsAsErrors`.

**2. Include a control that must pass.** An experiment where everything fails cannot distinguish *"the
defect was detected"* from *"the apparatus is broken"*. One task's injection failed on its **positive
control** — a case the old scan certainly reached — revealing a literal backspace introduced by shell
escaping. Without the control it would have reported a false result. This repository already has the
scar: `MASTER-159` was a false defect report from a DPI-broken probe that never reached its target.

The strongest form the session found: **make the control produce a number, not just a pass.** The
`MASTER-149` ratchet, armed with a real offender, ran in **15 ms against its inert `< 1 ms`**. A green
result alone could not have told armed-and-passing from inert-and-passing.

**3. A guard inside a fact protects that fact; a guard inside a helper protects nothing.** Measured in
both directions. Blinding a scan left **all ten facts built on it green** — only the newly promoted
guards failed. And in one case a fact stayed green because it `continue`s past a missing declaration.
But the mirror also happened: a file *claimed* to be losing its only protection already had in-fact
guards and was never vacuous-capable, so **check which kind you have before you assert either.**

**4. Enumeration assertions no-op on empty input.** `Assert.All(...)` and a bare `foreach` both pass
green over an empty collection. 6b-i measured **four facts passing green on an empty catalogue,
executing zero per-image assertions**; a fifth was protected only by an incidental index crash. Put
the non-emptiness assertion **before** the enumeration, in the fact.

## Environment traps — the ones that actually fired this time

**The LF window is real and it broke a peer's build.** A newly created `.cs` file sat on disk in
whole-file LF state for about twenty minutes and produced **`0 Warning(s) / 176 Error(s)`**, all
`IDE0055`, blocking another task entirely. **The remedy is ordering, not vigilance:** convert to CRLF
**immediately after writing each file**, before any command that could build the assembly — not as a
cleanup pass at the end. A window that only closes when a task finishes is a window a concurrent peer
will land in.

Note the mechanism is **not** the one `build-file-ownership.md` §5 documents. That warns about
`sed -i` rewriting an *existing* file; this was a *newly created* file written LF-first. Same symptom,
different cause — a reader following §5 would hunt for a `sed` that never ran. A reviewer separately
lost a file to `sed -i` and another to `git show > file`, both recovering from byte copies.

**Never check leading tabs or line endings with `grep`.** `grep -cE "^\t..."` returns **0** on a fully
tab-indented file, because GNU ERE reads `\t` as a literal `t`. This fired **five times in 6b-i, twice
inside frozen acceptance criteria** — and the second occurrence was authored *after* the form had been
adjudicated and the measurement sent to its author, in a task whose criterion 10 forbids the construct
its criterion 2 mandates. Use `[[:space:]]` or `\s`, or count in Python; compare `tr -cd '\r' | wc -c`
against `tr -cd '\n' | wc -c` for line endings. **A manifest lint is in the backlog, because prose has
demonstrably not stopped this.**

**A qualified single-line grep misses a split call.** `grep -n "Type.Member"` does not see `Type`
newline `.Member`, which is how this codebase formats many calls. It produced a wrong zero-call-site
claim from a task that **documented that very pitfall two items later in the same report.** Count call
sites on the bare member name.

**A failure in a peer's files may be a third agent's live injection.** One task reported three of a
peer's tests failing, stable across three runs. They were not broken — it was watching that peer's
*reviewer* run branch disablings. Adjudicated afterwards: all passed. **Report it and attribute it
carefully; do not retry it away and do not assume the peer's work is broken.** Stat-only index entries
also appeared twice, looking like modifications until `git update-index --refresh` cleared them.

**`git checkout -- <path>` is safe only on a file with no uncommitted changes.** Mid-task your own work
is not in git either. The dispatch briefs in 6b-i deliberately overrode `dir-shell` §5's "no backup
copies" for this reason — **and that override should have been declared rather than issued silently.**
An agent flagged the contradiction rather than complying, which was right.

**Concurrent peer builds in one worktree still produce spurious failures**, all clearing on a plain
retry. Unchanged from 6a. **macCatalyst is parked (`ADR-001`)** — it builds, it cannot be run here.

## The backlog you inherit

`.dir-sdlc/backlog.md` gained **eight** items from 6b-i (`grep -c '^- 2026-09-08' .dir-sdlc/backlog.md`
→ 8). Seven are under `### For a later session -- from sub-session 6b-i`; the eighth, the grep-form
manifest lint, is under `## DIR process artifacts`. The four items 6a left are still open, and three of
them are now closed in substance by 6b-i's work — **the file has not been reconciled for that, and
reconciling it is not this handoff's to do.**

The two most likely to touch your work:

- **Six contract operations shipped with an implementer and no consumer**, to be decided as **one**
  unit because narrowing any is a contract change: `ThemeFileText` (zero call sites anywhere),
  `ThemeDirectory` and `StaticResourceKeys` (used only inside their own file), and
  `KeyDefinitionPattern`, `LegacyTolerantKeyDefinitionPattern` and `ThemeTokenNotFoundException`
  (reached only from their declaring task's own tests).
- **The Appium database exposure**, above.

## Process lessons worth carrying verbatim

1. **Hand agents the artifact, not your summary of it.** Every 6b-i dispatch pointed the agent at
   `manifest.json` and told it to read its own `acceptance` array verbatim, with a standing line that
   the manifest wins over the brief and any conflict is to be reported. **Fourteen reviews, fourteen
   first-round passes, zero blocking findings.**
2. **Give every agent the standing rule: implement what is correct, and report the discrepancy with
   the command that showed it.** All eighteen citation and count defects surfaced this way.
3. **Have the reviewer reproduce the demonstration, and prefer a different injection.** One reviewer
   authored eleven of its own; another found an implementer's demonstration *confounded* — the
   conclusion was right but the evidence shifted two arrays instead of one — and produced a cleaner
   one. That is a correction of a correction, and only running it found it.
4. **Sequence the measurement before the decomposition.** 6b-i's foundation was chosen by two spikes,
   not by the audit's suggestion, and the second spike overturned the plan the first had implied.
5. **State an override, do not issue it silently.** See `dir-shell` §5 above.
6. **A count is not a fact until a command produced it.** Including a file count in a dispatch brief:
   an agent measured 29 where the orchestrator said 24, reported the discrepancy — and in the same
   task fabricated three figures it had composed from prose rather than measured. **The difference was
   not care; it was whether a command ran.**
7. **Ask the user to look.** Unchanged, and still the cheapest source of ground truth this project has.

## Suggested shape

Ten entries, all `S`/`XS` except one `M`, all host-free `DungeonMaster.Tests` work. **This is smaller
and more uniform than 6b-i's eleven and probably does not want splitting.**

A natural ordering, not a decomposition: `MASTER-014` + `MASTER-065` first, since the index names them
as one unit and the shared SQLite fixture is what the other Infrastructure work builds on; then the
de-duplication cluster (`085`, `064`, `067`); then the assertion-quality pair (`066`, `069`), which is
where the vacuity lessons above apply most directly; then the three small cleanups (`042`, `041`,
`127`).

**`MASTER-066` and `MASTER-069` deserve the same treatment 6b-i gave its vacuity entries:** before
changing either, measure what the current test actually proves — arrange the state so the assertion
*should* fail, and see whether it does. Both entries allege the tests assert less than their names
claim, and that allegation is exactly the kind this project has repeatedly found to be true, false, or
true-for-a-different-reason.

Finally: this session **owns no index reconciliation for a prior session.** 6b-i marked its own eleven
rows and wrote its own outcome record. It also corrected the Session 6 summary row, which had read
"Not started" against sixteen landed entries — and said so in its outcome record. Session 6b-ii marks
only its own.
