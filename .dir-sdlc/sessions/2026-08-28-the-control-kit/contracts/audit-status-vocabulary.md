# CONTRACT — audit index status vocabulary

Frozen at Gate 2, session `2026-08-28-the-control-kit`.

**PRODUCERS:** T1, T2, T3, T4, T5, T6, T7, T8 — each reports the outcome of every audit entry it owns,
using exactly the vocabulary in §2.
**CONSUMER:** T9 — writes those outcomes into the `Status` column of
`docs/audit/00-master-index.md`, and writes nothing it was not handed.

T9 is the last task in the session and reconciles Session 5 as a whole, not only 5c. It cannot
re-derive an outcome from the diff: several entries here close by observation of the running app, and
two close only partially. This contract is the channel. An implementer needing it changed **stops and
escalates**.

---

## 1. What T9 must reconcile — measured, not inherited

Session 5 declares 32 entries at `docs/audit/00-master-index.md:422`. Measured in the worktree on
2026-08-28 with:

```
for id in 007 026 027 030 034 035 036 037 052 053 054 056 057 058 059 060 061 076 077 082 083 084 \
          087 094 095 096 097 098 099 105 106 108 159 161; do
  grep -n "| MASTER-$id |" docs/audit/00-master-index.md | head -1 \
    | awk -F'|' -v i="$id" '{printf "MASTER-%s  line %s  status=[%s]\n", i, $1, $11}'
done
```

run from the worktree root. Result: **5 rows carry a marker and 29 rows have an empty `Status` cell.**

- **Already marked, T9 does not touch them:** `MASTER-026` (:214), `MASTER-027` (:215),
  `MASTER-030` (:218), `MASTER-082` (:270), `MASTER-087` (:275) — all `**done** (5a)`.
- **5b closed these and never marked them. T9 marks all 14:** `MASTER-052` (:240),
  `MASTER-053` (:241), `MASTER-054` (:242), `MASTER-056` (:244), `MASTER-057` (:245),
  `MASTER-058` (:246), `MASTER-076` (:264), `MASTER-083` (:271), `MASTER-084` (:272),
  `MASTER-094` (:282), `MASTER-095` (:283), `MASTER-096` (:284), `MASTER-106` (:294),
  `MASTER-108` (:296).
- **5c's own 15:** `MASTER-007` (:195), `MASTER-034` (:222), `MASTER-035` (:223),
  `MASTER-036` (:224), `MASTER-037` (:225), `MASTER-059` (:247), `MASTER-060` (:248),
  `MASTER-061` (:249), `MASTER-077` (:265), `MASTER-097` (:285), `MASTER-098` (:286),
  `MASTER-099` (:287), `MASTER-105` (:293), `MASTER-159` (:347), `MASTER-161` (:349).

14 + 15 = 29, which matches the measured count of empty cells. Line numbers are as measured **before**
any 5c edit; T9 must re-derive them by ID rather than by line, because nothing else in this session
edits this file but the numbers are quoted here for cross-checking, not for addressing.

## 2. The vocabulary — these strings and no others

The `Status` cell is the tenth pipe-delimited field. Existing precedent in the file
(`:189`, `:214`, `:218`, `:201`) fixes the form:

| Outcome | Exact cell text |
|---|---|
| Entry fully closed in this sub-session | `**done** (5c)` |
| Entry fully closed in an earlier sub-session | `**done** (5b)` / `**done** (5a)` |
| Entry closed in part, remainder deliberately deferred | `part done (5b) -- <what remains>, deferred to a later session` |
| Entry found already fixed, no code change needed | `**done** (5c) -- found already fixed, see session report` |
| Entry attempted and NOT closed | `not done (5c) -- <one clause naming why>` |

`**done**` is bold; the sub-session tag is unbolded and parenthesised; any qualifier follows a
space-hyphen-hyphen-space. Do not invent a sixth form. If a task's outcome fits none of these, that is
an escalation, not a new string.

## 3. The two that must NOT be marked done

`MASTER-094` and `MASTER-108` closed **partially** in 5b by the user's decision at Gate 2. Marking
either `**done**` would record work that was never done and would remove the only pointer to it.

- `MASTER-094` (`UI-007`) — 5b made the reload policy uniform (13 mutating members, 13 reload sites,
  zero deviations) but did **not** narrow it: the mutating commands still re-query the aggregate
  rather than returning or applying the changed projection. That half changes Application-layer
  command results, a layer no 5b task owned, and it is **not in 5c's scope either**.
- `MASTER-108` (`UI-016`) — 5b extracted the editor seam but did **not** convert
  `RebuildFloorList` to a templated `BindableLayout`; it is still the imperative loop at
  `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:717`.

**T6 is warned about this directly.** MASTER-099's remedy also mentions `BindableLayout`, but T6's
`BindableLayout` work is over `AreaSidebarItem` children inside
`FloorSidebarItem.RebuildAreas` (`FloorSidebarItem.xaml.cs:158-177`) — a *different* loop in a
*different* file from `RebuildFloorList`. T6 converting `RebuildFloorList` would be scope creep into a
deferred entry. If T6 reports it converted `RebuildFloorList`, T9 must **not** upgrade `MASTER-108`;
it must escalate, because the entry's status was decided by the user at 5b's Gate 2.

## 4. Entries closed across more than one task

One 5c entry does not map one-to-one onto a task:

- **`MASTER-077`** (`UIKIT-007`) names three controls. `DmFlagBanner` and `DmInlineEditor` belong to
  **T8**; `DmCard`'s `SubtitleProperty` clause also belongs to **T8**, which owns
  `Controls/DmCard/**` after T2 has finished with it. So `MASTER-077` is **T8's alone** and T2 must
  not touch the subtitle mechanism. T9 takes its outcome from T8 only.

Every other entry has exactly one owning task. T9 must refuse to mark a row for which it holds no
producer report, and say so rather than inferring from the diff.

## 5. Documents T9 also reconciles

Marking the index is not the whole job. Where a detail record's own text is now false, T9 corrects
the record rather than silently leaving it contradicting the index:

- `docs/audit/04-app-controls-and-theme.md` — `UIKIT-012` (`:507-511`) asserts `FloorSidebarItem`
  "exposes 12 public events in total". Measured 2026-08-28 with
  `grep -c "^\tpublic event" src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs`
  → **14**. `UIKIT-015` (`:627-630`) says `ScrollGoldLight` has "four call sites"; measured with
  `grep -rn 'ScrollGoldLight' src/ --include=*.cs --include=*.xaml | grep -v '/obj/\|/bin/'`
  → **three** (`FloorSidebarItem.xaml.cs:146`, `NotebookAffordance.cs:45`,
  `DungeonDetailPage.xaml.cs:582`).
- `docs/audit/03-app-shell-and-views.md` — reconciled only if a 5c task reports a false claim in it.

T9 corrects a record's **number** and leaves its **judgement** alone. Rewriting a finding's reasoning
is not reconciliation.

## 6. Handing an outcome to T9

Each producing task's report must contain a block of the form:

```
AUDIT OUTCOMES
MASTER-0NN: <one of the §2 strings>
            evidence: <the command, or the observation, that establishes it>
```

A producer that cannot state the evidence states `evidence: none` rather than omitting the line, so
T9 can see the gap instead of inheriting a silent one.
