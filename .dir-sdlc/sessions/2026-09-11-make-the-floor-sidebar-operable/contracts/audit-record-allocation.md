# Contract C4 — audit record allocation and the correction/closure split

Frozen at Gate 2. **AMENDED 2026-09-11 after freeze, by the user's resolution of escalation E6** — the only authority that may change a frozen contract. Amended passages are marked in place. Written by `T6`. Its id allocation is consumed by `T1`, `T3` and `T4` (which quote
entry ids in their reports) and by the orchestrator at Gate 3.

## 0. Why this contract keeps getting corrected — READ BEFORE WRITING A WORD OF `T6`

This contract exists to correct false claims in the audit index. **It has itself carried two false
claims about which false claim was where**, both caught by the orchestrator re-running a command
rather than reading the sentence: correction 10 named the wrong host file for the `:717` citation, and
correction 11 asserted that `measurement.md` §3 repeated `55x55` when what it repeated was `16x23`.

That is the same mechanism four other times this session: an agent's grep-based "dead type"
conclusion, the orchestrator's citation misattribution, its WAL-less database read, and `dir-git`'s
invented member name. **Every one was a statement about a file restated from another statement about
that file, instead of re-derived from the file.** None was a reasoning error; each was a skipped read.

`T6` writes ~thirteen claims about what other files say. **It is the single highest-density
opportunity in this session to commit this error again.** So, binding on `T6` and on its reviewer:

- **Open the file and read the line.** A citation is written from the file it cites, never from this
  contract, never from `measurement.md`, never from a task report — those say *which* line to check,
  not what it says.
- **Quote the previous wording verbatim, from the file**, which §5 already requires. Verbatim quotation
  is what makes a restated claim fail visibly instead of silently.
- **Where the file cannot be reached** — the `b3b1641` line numbers in files `T1`–`T4` edited — say so
  in the report and mark the citation as unverified, per `T6`'s last acceptance criterion. An
  unreachable citation stated as current is this error with extra steps.

## 1. New entry ids — FROZEN

The highest id in `docs/audit/00-master-index.md` is **`MASTER-173`** (`00-master-index.md:361`),
counted from that file and not from a grep of the repository, with
`grep -o "MASTER-1[0-9][0-9]" docs/audit/00-master-index.md | sort -u | tail -4` re-run in the
worktree on 2026-09-12 and still returning `MASTER-170`..`MASTER-173` — `T6` has not run, so 174 and
175 are allocated here but not yet written to the file. **`MASTER-176` is therefore the next free id
after `175`.**

| New id | Entry | Filed by |
| --- | --- | --- |
| `MASTER-174` | **A drop that misses every drop target is discarded silently — no error, no feedback, no trace.** `FloorListContainer` (`DungeonDetailPage.xaml:668`) carries no `DropGestureRecognizer`, so a release between cards, or on any space no handler owns, reaches nothing. **AMENDED at escalation E6: this entry is NOT "the reorder does not reorder"** — that characterisation is now known false and filing it would put a false claim into the index this session exists to correct. The reorder works end to end when the drop lands (orchestrator observation with the user, 2026-09-11, `observations/T1-reorder.md`). Fixed by `T3`. | filed by `T6`, fixed by `T3` |
| `MASTER-175` | **`AreaSidebarItem`'s card body carries no `HandCursorBehavior` at all.** The root `Border x:Name="ItemBorder"` (`AreaSidebarItem.xaml:12-13`) carries a `TapGestureRecognizer` (`:138`) and `HoverOrLongPressBehavior` (`:141-145`); the file's only two `HandCursorBehavior` attachments are the `Button`s at `:111` and `:131`. This is **absence**, not the inertness `MASTER-165` alleges, and it is the surviving half of the 2026-09-01 observation. **FIXED IN `T3` by the Gate 2 decision** — the entry records a defect this session closed. | filed by `T6`, fixed by `T3` |

| `MASTER-176` | **The floor drag handle has no keyboard route at all, so floor reordering is pointer-only.** `OnKeyDown` (`DungeonDetailPage.xaml.cs:361-430`) declares no reorder key — contract C1 §3 records this — and the frontend discipline requires every interactive element to be reachable and operable by keyboard alone. **Significance:** `MASTER-164` restored keyboard access to a floor's *areas* this session, which leaves **reorder** as the remaining pointer-only operation in the sidebar. Raised by `T3` as an advisory and correctly **not** acted on: `T3`'s task is layout-only and C1 §3 freezes the gesture host. Filed **OPEN**; no task in this session implements it. Escalation E9. | filed by `T6` |

Ids are append-only. `T6` must not renumber any existing entry: every rank in this file has been
cited by id elsewhere, which is the reason the file itself gives at `00-master-index.md:367`.

`MASTER-174` and `MASTER-175` have **no** source record in `01-`..`06-`, like every entry added after
the 2026-08-17 audit. `T6` must extend the coverage-attestation note at `00-master-index.md:367` and
the attestation bullet below it to account for two more entries outside the four statements, rather
than leaving those counts silently stale.

## 2. The correction / closure split — FROZEN

This is the load-bearing half of this contract. A DIR task **cannot invoke git**, so it cannot cite a
commit hash and cannot state what a session's commits did.

| Kind of edit | Who | Why |
| --- | --- | --- |
| **Claim corrections** — a stated fact is wrong, a premise is void, a severity does not survive, a citation has drifted | `T6` | Every one of these is settled by `measurement.md` alone, at `b3b1641`, with no commit evidence needed |
| **Status closures** — marking an entry `done (6b-iv)` with the commit hash, the red-step evidence and the file:line of what shipped | **orchestrator, at Gate 3** | Requires git, requires the task reports, requires the merged result |

`T6` therefore **leaves the Status column of `MASTER-163`, `MASTER-164`, `MASTER-166` and
`MASTER-174` unclosed**, and writes only the corrected claim into each. It does not write "done", it
does not write a commit hash, and it does not predict one. Writing a plausible-looking hash is a
known failure mode in this repository's tooling and is explicitly forbidden here.

Two entries are the exception, because their disposition is settled by observation and needs no
commit at all:

- **`MASTER-161` closes `done`.** Its 5c remedy works at `b3b1641`; the hand cursor was human-observed
  on the as-shipped configuration (`measurement.md` §2). `T6` closes it with the observation as the
  evidence and **no** commit hash, because no code was changed for it this session.
- **`MASTER-175` is filed as FOUND AND FIXED, with no status and no hash.** `T3` attached the missing
  behavior. `T6` writes what was found and what shipped, quoting `T3`'s report; the orchestrator writes
  the status with commit evidence at Gate 3 like every other closure in §2. The entry must not read as
  an open item, and must not read as a duplicate of `MASTER-165`: this one is **absence**, that one
  alleges **inertness** and is false.
- **`MASTER-165` is FALSIFIED.** Its stated claim — that `HandCursorBehavior` takes effect on
  `Button`s and not on layouts and labels — is false. `T6` rewrites it as falsified, or closes it as
  superseded by `MASTER-175`. **It must not be re-attempted as work**, and no task fixes it.

## 3. Corrections `T6` must make — FROZEN as the minimum set

| # | Entry / file | Correction | Authority |
| --- | --- | --- | --- |
| 1 | `MASTER-161` (`00-master-index.md:349`, detail `:567`) | Status `not done (5c)` → `done`. The behavior **is** attached, at `FloorSidebarItem.xaml:88` with its comment at `:76-78`, and it works. | `measurement.md` §1, §2 |
| 2 | `MASTER-165` (`:353`, detail `:571`) | Claim falsified. Root cause is **not** a platform-type cast: `SetCursor` casts to `Microsoft.UI.Xaml.UIElement` (`Behaviors/HandCursorBehavior.cs:157`), which `Button`, `Label`, `Grid`, `Border` and `VerticalStackLayout` all satisfy, and `BindingFlags.NonPublic` (`:116-122`) defeats `ProtectedCursor`'s protection with no subclass. `DmCard` self-attaches on a `ContentView` root (`DmCard.xaml.cs:116`) and was observed working in Session 4. Superseded by `MASTER-175`. | `measurement.md` §1, §2 |
| 3 | `MASTER-163` (`:351`, detail `:569`) | **REWRITTEN AT ESCALATION E6 — the severity goes UP, not down.** The literal *"cannot be performed at all"* clause is still FALSE: the scrollbar impedes grabbing rather than blocking it, and the reorder **works end to end when the drop lands** (orchestrator observation with the user, 2026-09-11, all 11 floors, no scrollbar, move persisted `Guest Rooms` 3 → 7; `observations/T1-reorder.md`). But this is now **the session's primary defect**, not an ergonomic one: reaching the drop target is impractical whenever the list scrolls, **and a drop that misses fails SILENTLY** — `FloorListContainer` (`DungeonDetailPage.xaml:668`) carries no `DropGestureRecognizer` — which is why the feature reads as broken. `MASTER-174` is this entry's **symptom**, not a peer defect. **A downward severity correction does not satisfy this row.** | `observations/T1-reorder.md`; escalation E6 |
| 4 | `MASTER-098` (`:286`) | Its deferral premise — *"the DragOver state is unverifiable while the drag handle is unreachable (MASTER-163)"* — is **void**. The handle is reachable. The entry stays open; only the premise is struck. | `measurement.md` §2 |
| 5 | `MASTER-166` (`:354`, detail `:572`) | The **wiring half is false**: `AddGhostArea` is fully wired (`FloorMapDrawable.cs:378-390`; one call site `DungeonDetailPage.xaml.cs:1754`; `RaiseRedrawRequested` `:499-500` subscribed at `DungeonDetailPage.xaml.cs:77-78`) and **four tests already exist** (`FloorMapDrawableStateTests.cs:69-74`, `:79-84`, `:148-160`; `FloorMapDrawableAreaAlphaTests.cs:96-110`). The open question is timing only. | `measurement.md` §1 |
| 6 | `00-master-index.md:567` | Citation: recogniser is at `FloorSidebarItem.xaml:84-86`, not `:33-35` (which is now a comment block). | `measurement.md` §4 |
| 7 | `00-master-index.md:567` | Citation: `DmCard.xaml.cs:116`, not `:93`. | `measurement.md` §4 |
| 8 | `00-master-index.md:569` | Citation: handle is at `FloorSidebarItem.xaml:164-179`, not `:164-171` (gesture recognizers were added). | `measurement.md` §4 |
| 9 | `docs/audit/04-app-controls-and-theme.md:376` and `:481` | Neither `GhostAreas` as a public `List<>` nor a separate `DrawGhostAreas(canvas)` exists. It is a private `_ghostAreas` field (`FloorMapDrawable.cs:117`) and one merged `DrawAreas` loop (`:535-553`). | `measurement.md` §4 |
| 10 | `.dir-sdlc/backlog.md:225` | Citation: `RebuildFloorList` is declared at `DungeonDetailPage.xaml.cs:752`, not `:717`. `measurement.md` §4 originally attributed this citation to `HANDOFF-session-6b-iv.md` §4, where no such citation exists — `grep -n "RebuildFloorList"` over that file returns nothing. **The orchestrator re-ran both commands and corrected `measurement.md` §4 at Gate 2**, so this row is now the only place the misattribution is recorded and `T6` need not re-litigate it: correct the citation at `.dir-sdlc/backlog.md:225` and note in the amendment that the earlier attribution was wrong. | this contract; both commands re-run by the orchestrator at Gate 2 |

| 11 | `00-master-index.md:570` (`MASTER-164`'s detail entry) | **BOTH figures in this sentence are wrong, and this is not merely a number fix.** The entry states the remedy cost as *"a header-row height change, since those siblings render **55x55** against the `Label`'s **16x23**"*. `T2` measured both in a **running application**, maximized 1920x1080, Test Dungeon #1, 11 floors: the sibling `Button`s are **44x44**, not 55x55; the `ExpandToggle` `Label` is **13x19**, not 16x23; and the header row is **44 and UNCHANGED**, not grown. `T6` must correct **both** figures — a correction that fixes 55x55 and leaves 16x23 standing does not satisfy this row. **The load-bearing half: `55x55` is what the user's sub-session 5c escalation-E5 refusal rested on**, so the remedy was deferred for two years of session time on a measurement nobody re-took; the amendment must record that, and correcting the digits without it does not satisfy this row either. AUTHORITY, and each figure has a different one so `T6` cites them separately: **44x44 and the unchanged 44-high row** from `observations/T2-keyboard.md:73`; **13x19** from `measurement.md` §7 (`:320`), which is where `T2`'s measurement line `card=300x54 header=282x44 toggle=13x19 name=160x22 notebook=44x44 delete=44x44 handle=7x24` was recorded — it is **not** in `observations/T2-keyboard.md` and **not** in `reviews/T2.md`, verified by `grep -rn "13x19"` over the session directory returning that one line only. `T2`'s own report, supplied by the orchestrator at dispatch, is the primary source for both. **This session's `measurement.md` §3 repeated the `16x23` figure** — not 55x55 — inherited from the index without re-measuring, in the bullet beginning *"The objection that sank it at E5 is resolvable"* (`:171`, restated `:175`). The orchestrator owns that file and has appended §7 recording it; **`T6` must not edit `measurement.md`** and must not treat §3 as an index correction. | `observations/T2-keyboard.md:73`; `measurement.md` §7 (`:320`); `T2`'s report |

## 4. Entries `T6` must NOT touch — FROZEN

- `UIKIT-004` in `docs/audit/04-app-controls-and-theme.md`. `MASTER-167`'s closed record instructs
  twice that it is **not** falsified and must not be recorded as such. Correction #9 above touches
  different records in the same file; `UIKIT-004` stays as it is.
- Any entry outside `MASTER-098`, `-161`, `-163`, `-164`, `-165`, `-166` and the two new ids.
- The 158/167 reconciliation arithmetic at `00-master-index.md`'s coverage attestation. It counts the
  2026-08-17 audit and is not changed by appended entries.

## 5. Format — FROZEN

Corrections follow the convention the file already uses and nothing else: an inline
`**[AMENDED 6b-iv, 2026-09-11 -- <what>. Classification: <MEASUREMENT ERROR | DRIFT | FALSIFIED>.**
Previous wording, verbatim: *"..."* ... **]**` block appended to the detail entry, with the previous
wording quoted verbatim so a reader who tested the old claim can see why it did not reproduce.
Precedent: the three `[AMENDED 6b-iii]` blocks under `MASTER-167`.
