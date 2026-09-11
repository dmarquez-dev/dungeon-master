# Contract — `GreaterThanZeroConverter` behaviour after the MASTER-167 guard

Frozen at Gate 2. Produced by T3; consumed by T3 (its tests call both operations) and by T5
(the record correction restates this table and must not contradict it).

Authority for the decision: escalation **E3**, resolved — guard *any* non-`int`, not the
sentinel alone.

## Shape (load-bearing)

The guard is an early return placed **above** the `Invert` block, so the negation can never
see an "unresolved" answer:

```csharp
if (value is not int n)
{
    return false;
}

bool result = n > 0;

if (parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase))
{
    result = !result;
}

return result;
```

Placement is part of the contract. A guard applied *after* the negation, or a guard that only
tests `ReferenceEquals(value, BindableProperty.UnsetValue)`, does not satisfy it.

## Operation — `object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)`

Returns a boxed `bool`. Never throws for any input in this table.

### Rows that must NOT change (regression guard)

| `value` | `parameter` | result |
|---|---|---|
| `5` (int) | `null` | `true` |
| `0` (int) | `null` | `false` |
| `-1` (int) | `null` | `false` |
| `5` (int) | `"Invert"` | `false` |
| `0` (int) | `"Invert"` | `true` |
| `-1` (int) | `"Invert"` | `true` |
| `0` (int) | `"invert"` | `true` (match is `StringComparison.OrdinalIgnoreCase`) |
| `0` (int) | `"INVERT"` | `true` |
| `5` (int) | `"Something"` | `true` (an unrecognised string parameter is ignored) |
| `0` (int) | `42` (non-string) | `false` (a non-string parameter is ignored) |
| `BindableProperty.UnsetValue` | `null` | `false` |

### Rows the fix changes (all currently return `true`)

| `value` | `parameter` | result before | result after |
|---|---|---|---|
| `BindableProperty.UnsetValue` | `"Invert"` | `true` | **`false`** |
| `null` | `"Invert"` | `true` | **`false`** |
| `1.0d` (double) | `"Invert"` | `true` | **`false`** |
| `1L` (long) | `"Invert"` | `true` | **`false`** |
| `"5"` (string) | `"Invert"` | `true` | **`false`** |

`BindableProperty.UnsetValue` + `"Invert"` is the row MASTER-167 names. The other four are the
amplification the re-measurement found; E3 closes all five.

## Operation — `object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)`

Returns `null` for every input. Unchanged by this session, and asserted so it stays inert.

## Consumers in the product (not changed by this session)

Measured at `1407090` in the session worktree by
`grep -rn "GreaterThanZeroConverter" src/DungeonMaster.App/` — **7 lines**: one resource
declaration (`src/DungeonMaster.App/App.xaml:21`) and six binding sites. Exactly **two** pass
`ConverterParameter=Invert`:

- `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml:151`
- `src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml:115`

Both are transparent `BoxView` overlays carrying the identical tooltip text
`"Add at least one floor before starting a session"`. Both pages declare a root `x:DataType`
(`DungeonDetailPage.xaml:10`, `DungeonListPage.xaml:10`) and the App project sets
`<MauiXamlInflator>SourceGen</MauiXamlInflator>` (`src/DungeonMaster.App/DungeonMaster.App.csproj:45`).

No XAML is edited by this session.
