# Brief — start here for 6b-xvi

**One page. `HANDOFF-session-6b-xvi.md` beside this file is the reference document — 491 lines, and it
answers everything this page does not.** Read this to start; read that when you need a figure, a command, or
the reason behind a rule.

**Written 2026-10-07. Every figure below was re-derived on that date**, six days after 6b-xv closed. The
repository had not moved: `git rev-list --count 49fd02c..chore/repo-audit` returns **0**. That is why this
brief and the handoff agree — not because this one trusted that one.

---

## Where the arc stands

| | | Command |
|---|---|---|
| Branch | `chore/repo-audit` at **`49fd02c`**, **0 ahead / 0 behind** origin | `git rev-parse --short`, `git rev-list --left-right --count` |
| `main` | **`45495c1`**, local and remote identical, untouched by the whole 6b arc | `git rev-parse --short main`, then `origin/main` **separately** — the two-revision form fails |
| Working tree | clean | `git status --porcelain` |
| `C:/dm-wt` | **empty**, ready for a new worktree | `ls -A /c/dm-wt \| wc -l` |
| Register | **179 rows — 132 closed, 43 open, 4 part done**, zero `UNCLASSIFIED` | the survey in handoff §6 |
| Open severity | **S1 0, S2 0, S3 1, S4 32, S5 10** | same |
| Open layers | CORE 12, UI 9, UIKIT 9, INFRA 6, TEST 6, `UI, XCUT` 1 | same |
| `.dir-sdlc/backlog.md` | **453** newlines, **8** lone CR | `b.count(b'\n')`, `b.count(b'\r')-b.count(b'\r\n')` |
| Suite baseline | **1021 passed** — `DungeonMaster.Tests` **890**, `App.UnitTests` **23**, `App.Tests` **108** — 0 failed, 0 skipped, exit 0 | `dotnet test src/DungeonMaster.sln` in the main checkout, **re-run 2026-10-07**, per-assembly figures read from the three `Test run for ...` lines |

**The arc is nine sessions into the 6b stream and the severity band is nearly spent.** One open S3 remains.
After it, selection is 32 S4 and 10 S5 and must run on coherence or layer concentration rather than severity.

---

## The recommended scope, and the argument in one paragraph

**`MASTER-051` / `UI-003` — the Entry and Editor handler mappers repeat the same eight WinUI resource
assignments verbatim.** Row at `docs/audit/00-master-index.md:239`; entry at
`docs/audit/03-app-shell-and-views.md:171-198` — the heading is `:171` and the last non-blank line `:198`,
with the next entry's `### UI-004` at `:200`. **UI, S3, `effort: S`, `risk: low`, `independent: true`.**

> **This range was wrong in this brief's first draft, which said `:171-199`** — a blank line counted into the
> entry. Caught by printing `:199` and finding it empty rather than trusting the arithmetic. It is a one-line
> error in a document about not making one-line errors, and it is left recorded here for the same reason the
> handoff records its own: a correction that hides its cause teaches nothing.

It is **the last open S3 in the register**, and closing it retires the selection criterion that has chosen
nine of the last ten sessions. It is small, it is self-contained, and it touches a layer this stream has
worked least. Its cited locations are two adjacent blocks in one file —
`src/DungeonMaster.App/MauiProgram.cs:59-84` and `:86-113`, both within a **210-line** file
(`wc -l`) — and its suggested direction is a single private static method taking the `TextBox`, leaving the
genuinely platform-divergent `#elif MACCATALYST` arms inline.

**Re-measure it before decomposing, and expect it to be wrong somewhere.** Every entry re-measured in this
stream has had at least one false or stale specific, and **6b-xv falsified both of its recommended entry's
load-bearing premises** — `MASTER-081` claimed a validator did not exist when it did, and claimed three
records accepted any input when one had been guarded two sessions earlier. Implemented as written it would
have built a policy layer for a problem that no longer existed. `UI-003` makes four checkable claims, and
every one is a count or a universal:

1. **"the same eight properties and resource keys"** — a count. Count them in both blocks, separately.
2. **"byte-identical apart from the local variable's declaration site"** — a universal. Diff the two blocks.
3. **"only the `#elif MACCATALYST` halves differ"** — a universal, and the one most likely to have drifted.
4. **"on Windows both handlers already resolve to the same platform type"** — a claim about a mechanism.

**`:59-84` and `:86-113` are positions in a file the audit has not touched since.** Read them; do not cite
them.

---

## The three things that will bite on day one

1. **Use a short worktree root — `C:/dm-wt/6b-xvi`, never the scratchpad path.** This is a MAUI/WinUI
   solution and a long path breaks the WinUI XAML compiler with `MAX_PATH` errors. `C:/dm-wt` is empty and
   exists for exactly this.
2. **`sed -i` is suspended, and the harness WILL re-issue its `sed`/heredoc preference mid-run to every
   agent, reviewers included.** It rewrites whole-file CRLF to LF and fails the build. **Restate the
   suspension in every dispatch.** 21 consecutive agents refused it across 6b-xiv and 6b-xv; the one that
   does not will break the build in a way that looks like a formatting failure.
3. **If you append to `.dir-sdlc/backlog.md`, append CRLF and assert the lone-CR count before and after.**
   It is the **one** tracked file in this repository stored CRLF — everything else, source and session
   artifact alike, stores LF. It carries **8** lone CR characters from 6b-iv that ten sessions have
   deliberately preserved. An LF append makes the file mixed; a normalization pass destroys all 8.

Two more that cost time rather than breaking things: **the solution is at `src/DungeonMaster.sln`**, not the
repository root (`MSB1009` otherwise); and **`dotnet test`'s `Passed!` line cannot tell you which TFM ran** —
it prints `(net10.0)` for `DungeonMaster.App.Tests` while that assembly runs from
`net10.0-windows10.0.19041.0`. Read the `Test run for ...` line.

---

## The one habit worth carrying from 6b-xv

**Eight of the ten errors that session caught were the orchestrator's own, and the two that cost real work
were PREDICATE failures where the total was correct.** A census told a task its change would break nothing.
The number was right, the command was right, the anchor was right — and it was blind to values arriving as
xUnit `[InlineData]` theory parameters, so the pattern could never match the population under test. The guard
broke seven tests and the session paid an amendment and an extra task.

> **Ask what form the thing you are counting could take that your pattern cannot match**, and count a second
> way. In 6b-xv both second counts disagreed with the first, and in both cases **the disagreement was the
> finding.**

---

## Where to look for the rest

| You need | Go to |
|---|---|
| The nine errors and what caught each | handoff **§0** |
| Commits, change surface, index arithmetic | **§1** |
| The survey, open rows, part-done warnings | **§2** and **§6** |
| Five ranked alternatives to the recommendation | **§3** |
| Inherited discipline, instrument traps, structural limits | **§4** |
| Six carried-forward items and everything unscheduled | **§5** |
| Worktree, TFM, model tiers, metrics, Gate-3 shape | **§7** |

**Four alternatives worth knowing without opening the file:** the **`git notes` convention** — now with four
candidate commits and a measured reason, since five consecutive sessions have put a false figure in a
permanent commit body while every repository write was correct; **`B11`'s missing mechanical guard**, where
one test closes a hole that was measured rather than suspected and has waited two sessions; **the UI latency
spike**, still the only item in the register or backlog that came from running the application; and the
**`PathSegmentKind` migration**, still the only item requiring a data migration on persisted rows.
