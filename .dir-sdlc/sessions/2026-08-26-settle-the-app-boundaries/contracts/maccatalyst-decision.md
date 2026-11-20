# Contract — the macCatalyst ADR

Frozen at Gate 2. Session `2026-08-26-settle-the-app-boundaries` (sub-session 5a).
Authored by `T4`. Consumed by every later reader of the repository, and by sub-sessions 5b and 5c,
which must not re-litigate the decision.

This contract fixes **where the ADR goes, what format it takes, and every fact it must state**. It
does not dictate its prose.

---

## 1. Where it goes, and why

**There is no ADR directory in this repository.** `find . -iname "*adr*" -o -iname "*decision*"`
over the repository root on 2026-08-26 returns only `IDungeonReadRepository.cs`,
`DungeonReadRepository.cs` and `DungeonReadRepositoryTests.cs` — three unrelated source files
matching on "read". `docs/` contains `TODO.md`, `audit/`, `icon-references.md`, `lucide-ref.html`,
`mockups/`, `persistence-contract.md`, `phosphor-ref.html`, `rpg-awesome-ref.html` and
`superpowers/`. Nothing else.

The format authority is
`C:\Users\dmarq\Development\Repos\dm-conventions\architecture\architecture-decisions.md`. That
artifact is a **single markdown document** holding numbered ADRs — `ADR-001` through `ADR-009` —
separated by `---`, each with the four headings shown in section 2. The repository's ADR therefore
takes the same form and the same filename:

**`docs/architecture-decisions.md`**, a new file, whose first and only record is **`ADR-001`**.

A `docs/adr/` directory of one-file-per-decision is a common convention elsewhere and is **not**
what this repository's convention authority prescribes. Do not create one.

`docs/persistence-contract.md` is the closest existing precedent for a decision document living
directly under `docs/`, which is consistent with this placement.

## 2. Required structure

Mirror the conventions document exactly:

```
# Architecture Decision Records

<one or two sentences saying what this file is>

---

## ADR-001: <title>

**Status:** Accepted

**Context:**
…

**Decision:**
…

**Consequences:**
- …

**Alternatives Considered:**
- **<option>:** Rejected — …
```

`.editorconfig` sets `indent_style = space`, `indent_size = 2` for `*.md`, with
`end_of_line = crlf`, `charset = utf-8` and `insert_final_newline = true`.

## 3. Facts the ADR must state — each one verifiable, none of them optional

### 3.1 The target is build-verified and not run-verified

State this distinction in those terms. The evidence, with the command that produced it — all run by
`dir-architect` on 2026-08-26 in the worktree
`C:\Users\dmarq\Development\Repos\dungeon-master\.claude-worktrees\dir\2026-08-26-settle-the-app-boundaries`:

- `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-maccatalyst --no-incremental`
  → `Build succeeded. 0 Warning(s). 0 Error(s).`
- `dotnet workload list` → `maccatalyst 26.5.10301/10.0.100` installed.

**Re-run both before writing them down.** A number or a string quoted from this contract rather than
observed is exactly the defect four consecutive sessions in this repository produced.

The ADR must **not** state or imply that any macCatalyst runtime behaviour has been observed.
Nothing in this environment can run a macCatalyst build. A sentence such as "the app runs correctly
on macCatalyst" is a false claim and fails this contract.

### 3.2 The five `TODO(macOS)` paths, named, as a known and accepted gap

`grep -rn "TODO(macOS)" src` finds five occurrences in `src/DungeonMaster.App`, verified this
session. A sixth match exists at
`src/DungeonMaster.Tests/App/AsyncDiscipline/SyncOverAsyncIsAbsentTests.cs:54`; it is a string
literal inside a test fixture, not a code path, and is **not** one of the five.

| # | Location | What is unimplemented |
|---|---|---|
| 1 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:329` | No UIKit equivalent of a global focus manager, so `IsTextInputActive` returns `false` unconditionally and keyboard shortcuts are not suppressed while the user is typing. |
| 2 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:516` | MAUI's `PinchGestureRecognizer` exposes no pinch centroid, so pinch zoom always zooms to the canvas centre. |
| 3 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1056` | `NSCursor` has no shape mapping 1:1 to WinUI's `SizeAll`. |
| 4 | `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1313` | No `GetAsyncKeyState` equivalent, so right-click suppression has no equivalent guard. |
| 5 | `src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:46` | The macCatalyst `Picker` popup (`UIPickerView`) styling is untested. |

`UI-021` judges #1 as reachable user-visible data loss on that platform and #4 as a path where a
right-click may execute draw or place logic before the secondary handler runs. The ADR must carry
that assessment rather than listing the five neutrally — a reader must be able to tell that the
accepted gap is not cosmetic.

**These five are NOT implemented by this session, by 5b, or by 5c.** Say so.

### 3.3 What is accepted unchanged, and what is therefore not a defect any more

The ADR converts three open observations into recorded, accepted state. After it lands, each is a
decision rather than an ambiguity — which is precisely what `XCUT-004` says the defect is
("I believe the ambiguity is the defect, more than either resolution of it would be").

- **`net10.0-maccatalyst` stays in `TargetFrameworks`** at
  `src/DungeonMaster.App/DungeonMaster.App.csproj:4`. Nothing in this session removes it.
- **`UI-036` — `src/DungeonMaster.App/Properties/launchSettings.json` declares only a
  `Windows Machine` profile — is accepted and the file is NOT changed.** `UI-036` itself states "No
  action needed as a file change"; macCatalyst launch profiles are supplied by the IDE on macOS and
  are not conventionally checked in.
- **`XCUT-004`'s missing solution platform is accepted and `src/DungeonMaster.sln` gains no Mac
  platform entry.** Its `SolutionConfigurationPlatforms` are `Debug|Any CPU` and `Release|Any CPU`,
  verified this session; the audit's "the solution's platform list is `Any CPU`, `x64` and `x86`"
  does not match the file as it stands and the ADR must describe what is there, not what the record
  says. Adding a platform nobody can build against would be adding the appearance of support.
- **There is no macCatalyst test host, and one cannot be built here.** Two failures observed this
  session when a test project was multi-targeted to `net10.0-maccatalyst`:
  `NU1201 — Project DungeonMaster.App is not compatible with net10.0-maccatalyst26.0`, and, after
  pinning the platform version to 26.5, `Empty ResolveFrameworkReference.RuntimePackPath while
  trying to read runtime components manifest`. The new `DungeonMaster.App.Tests` project introduced
  by this session is Windows-only for that reason (see `contracts/new-projects.md` section 3.2).

### 3.4 The consequence for the `#elif MACCATALYST` branches

`grep -rn "MACCATALYST" src/DungeonMaster.App --include=*.cs` finds, verified this session, **ten**
preprocessor directives across four files:

- `#if MACCATALYST` — `src/DungeonMaster.App/Behaviors/HandCursorBehavior.cs:29`, `:48`, `:62` (3)
- `#elif MACCATALYST` — `src/DungeonMaster.App/MauiProgram.cs:75`, `:102`;
  `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:328`, `:512`, `:533`,
  `:552`, `:1052` (7)

Two further matches are mentions inside comments and are not directives:
`src/DungeonMaster.App/Controls/DmCard/DmCard.xaml.cs:91` and
`src/DungeonMaster.App/Views/DungeonDetailPage/ObjectPaletteView.xaml.cs:47`.

**Count these yourself before writing a number down.** The grep's output mixes `#if`, `#elif` and
comment forms, which is how a count of this kind goes wrong. If your count disagrees with this
contract, the tree wins — write what you counted and report the discrepancy with the command that
showed it.

The ADR must record the consequence `XCUT-004` names: each of these branches is maintained twice
and compiled once per build but exercised on one platform only, so the macCatalyst half is
accepted as unverified at runtime for as long as this decision stands.

### 3.5 The revisit condition

`XCUT-004` asks for a decision written down; a parked decision with no revisit trigger is an
ambiguity wearing a different hat. State, at minimum, that the decision is revisited when **a
macOS machine enters the development loop** — at which point the five paths in 3.2 become a sized
worklist and the run-verification gap closes — and that until then no work is scheduled against
them. Any additional trigger the author judges real may be added; none may be vaguer than this one.

### 3.6 The one backlog item this unblocks, and does not close

`.dir-sdlc/backlog.md`, under "For Session 5", parks the evaluation of replacing
`Plugin.Maui.KeyListener` (pinned at `1.0.0-preview4` in `Directory.Packages.props`, `MASTER-104` /
`XCUT-011`) explicitly because "dropping macCatalyst would change the answer". Keeping the target
settles that input. The ADR should note the dependency is resolved in favour of keeping the target,
and therefore that the evaluation is unblocked.

**It must not claim the evaluation was performed.** `MASTER-104` is not one of this session's five
entries, no package change is in scope, and the item stays in the backlog.

---

## 4. What T4 must not do

- Not implement any of the five `TODO(macOS)` paths.
- Not remove `net10.0-maccatalyst`, or edit any `.csproj`, `.sln`, `launchSettings.json` or `.cs`
  file. `T4` owns `docs/architecture-decisions.md` and nothing else.
- Not write a criterion, or a sentence, resting on observed macCatalyst runtime behaviour.
- Not add an `ADR-002`. One decision is being recorded.
