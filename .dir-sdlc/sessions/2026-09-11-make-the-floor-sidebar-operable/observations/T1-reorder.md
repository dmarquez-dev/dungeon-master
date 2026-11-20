# T1 observation transcript — floor reorder drop

Run by the orchestrator with the user, per Gate 2's resolution of escalation E5. This is the evidence
`dir-reviewer` judges T1's criterion c3 against; without it that criterion is UNDETERMINED.

Protocol executed unchanged as T1 specified it.

## Conditions

- Build: `dotnet build C:/dm-wt/6b-iv/src/DungeonMaster.sln` → `0 Warning(s), 0 Error(s)`.
- Windows TFM rebuilt so the exe carried T1's changes: `0 Warning(s), 0 Error(s)`,
  `DungeonMaster.App.exe` written 2026-09-11 16:51.
- App launched, **Test Dungeon #1 (11 floors)**, window maximized at 1920x1080.
- **All 11 floors fit with no scrollbar present**, confirmed by screenshot `t1-obs-before.png`. This
  isolates the observation from `MASTER-163` entirely — the overlay scrollbar is not rendered.
- Action: press and hold `FloorDragHandle` on floor 3 (`Guest Rooms`), drag onto the card of
  `Floor 7`, hold ~1s, release.

## What the observer reported

| | Observation | Result |
|---|---|---|
| (a) | Source card invisible at drag start — proves `OnDragStarting` fired | **YES** — "Guest rooms vanished" |
| (b) | Target card shows the gold `DragOver` stroke | **YES** — "floor 7 showed a gold outline" |
| (c) | Placeholder gap appears | **YES** — "a highlight-esque placeholder card appeared as I moved the click-held cursor" |
| (d) | Order after release | **Reordered.** `Guest Rooms` moved to position 7 |
| (e) | `Could not reorder the floors` line | **None** |

The observer's phrasing was that the drop "outright replaced the floor rather than place the Guest
Rooms at its placeholder highlight-esque indicator", and that `Floor 7` had been "replaced by Guest
Rooms". Screenshot `t1-obs-after.png` shows what actually happened, and it is not a replacement:

```
1 Basement      2 Main Tavern   3 Floor 3    4 Floor 4   5 Floor 5   6 Floor 6
7 Guest Rooms   8 Floor 8       9 Floor 9   10 Floor 10 11 Floor 11
```

Still **11 cards**, nothing lost. Floors 4-11 carry blank names and the control renders
`$"Floor {floor.Number}"` as a placeholder (`FloorSidebarItem.xaml.cs` `Populate`), so the row that
now reads "Floor 3" is the previously-fourth floor renumbered to 3. That is precisely a correct
reorder of 3 -> 7 with the passed floors renumbered down. **The apparent "replacement" is the
placeholder-name rendering, not a data change.**

## Database check — and an orchestrator error inside it

**First read was wrong and was reported to the user before being caught.** A copy of
`%LOCALAPPDATA%\DungeonMaster\dungeon-master.db` taken at 17:02 showed `Guest Rooms` still at
`Number 3`, and that was stated as "the database is completely unchanged". It was a stale snapshot:
**only the `.db` file was copied, not its `-wal`.** SQLite holds committed data in the write-ahead log
until checkpoint, and a live `dungeon-master.db-wal` (12,392 bytes, written 17:13:43) existed at that
moment.

Re-read with the WAL present, and again after checkpoint (`.db` written 17:17:43, no `-wal`
remaining):

```
  1  Basement
  2  Main Tavern
  3  (unnamed)
  4  (unnamed)
  5  (unnamed)
  6  (unnamed)
  7  Guest Rooms
  8  (unnamed)
  9  (unnamed)
 10  (unnamed)
 11  (unnamed)
```

**The reorder persisted.** UI and database agree.

## Verdict

**T1 criterion c3 is MET.** (d) is the expected order and (e) showed nothing.

**And the task's premise is void.** Read against T1's own discriminator table, (a)=YES, (b)=YES,
(c)=YES eliminates candidates A and D and leaves B or C — but both B and C predict *no reorder*, and
the reorder happened, correctly, end to end. So none of A, B, C, D is the mechanism, because under
these conditions **there is no defect in the reorder path to have a mechanism.**

## What this means for the finding that opened the session

The human observation recorded in `measurement.md` §2 — "the drag starts, the scrollbar impedes
grabbing rather than blocking it, and no reorder happens" — was taken in a **different
configuration**: a non-maximized window with the floor list scrolling and the WinUI overlay scrollbar
rendered over the handle column. This run had no scrollbar at all.

The difference between the two runs is the scrollbar, and the trace's **candidate #3** already named
the consequence: `FloorListContainer` (`DungeonDetailPage.xaml:668`) carries no `DropGestureRecognizer`,
so a drop landing between cards or on non-handler space is discarded with no handler and no trace.

**So the reorder feature works, and what fails is reaching it** — which is `MASTER-163`, the entry
this session already holds, rather than a new unrecorded functional defect. The "new defect" this
session believed it had found is a *symptom* of `MASTER-163`, not a peer of it.

That also raises `MASTER-163`'s real severity above what §2 concluded: it is not merely awkward
ergonomics. When the drop misses, the operation fails **silently** — no error, no feedback — and the
user reasonably concludes the feature is broken.

## Side effect on user data, disclosed

The observation ran against the real local database, as the harness does. The drag **persisted**:
`Guest Rooms` moved from floor 3 to floor 7 in `Test Dungeon #1`. The user was told before the run
that a successful reorder would write. Original order is recoverable by dragging `Guest Rooms` back to
position 3, or from `db-copy.db` / `db-after-drag.db` in this session's scratch directory, both taken
before the checkpoint.
