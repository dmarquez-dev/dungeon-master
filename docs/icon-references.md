# Icon set references

`docs/phosphor-ref.html` and `docs/lucide-ref.html` are self-contained, offline browsable
references for two candidate icon fonts. They exist because the designer chrome needs a real icon
font: several glyphs in the map designer are Unicode symbols rendered out of the body text font,
which do not sit centred in their line box and currently require hand-measured per-glyph
translations to look right.

Each page lists every icon in the set with its **name**, its codepoint and a live preview, is
filterable by name, and pins the 17 icons the designer needs at the top. That is deliberately
different from `docs/rpg-awesome-ref.html`, which is a codepoint dump with no semantic names —
finding "the plus icon" in it means visually scanning 498 glyphs.

**The font binary is embedded in each page as a base64 data URI.** That is why the two files are
large (≈834 KB and ≈1.4 MB) and why they render correctly with no network access, no CDN and no
build step — open either file in a browser and it works. It is also why the licence notices below
are reproduced in full: the fonts are redistributed as part of this repository, and both licences
require their notice to accompany the distribution.

Both sets were checked for coverage of all 17 required designer icons. Both have all 17; nothing is
missing from either.

## Selection

**Phosphor has been selected.** It is chosen over Lucide for its weight axis: a tool palette has
idle / hover / active states, and the designer palette currently expresses "active" through colour
alone. Shipping `regular` and `fill` of the same icon is the conventional way to show tool
selection. Lucide remains the right choice if bundle size ever matters more than states.

Both references are kept, not just the selected one, so the comparison stays reproducible.

## Licences

### Phosphor Icons 2.1.2 — MIT

Copyright (c) 2020-2021 Phosphor Icons

> Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
> associated documentation files (the "Software"), to deal in the Software without restriction,
> including without limitation the rights to use, copy, modify, merge, publish, distribute,
> sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
>
> The above copyright notice and this permission notice shall be included in all copies or
> substantial portions of the Software.
>
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
> NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
> NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
> DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
> OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

### Lucide 1.30.0 — ISC, plus a Feather-derived subset under MIT

Lucide carries **two** notices. The bulk of the set is ISC; roughly 110 icons inherited from Feather
(including `plus`, `minus`, `zoom-in`, `zoom-out`, `move` and `trash-2`) remain under Feather's MIT
licence.

**ISC — Lucide Contributors:**

> ISC License
>
> Copyright (c) for portions of Lucide are held by Cole Bemis 2013-2022 as part of Feather (MIT).
> All other copyright (c) for Lucide are held by Lucide Contributors 2022.
>
> Permission to use, copy, modify, and/or distribute this software for any purpose with or without
> fee is hereby granted, provided that the above copyright notice and this permission notice appear
> in all copies.
>
> THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS
> SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE
> AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
> WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT,
> NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE
> OF THIS SOFTWARE.

**MIT — Feather-derived subset, © Cole Bemis:**

> MIT License
>
> Copyright (c) 2013-2022 Cole Bemis
>
> Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
> associated documentation files (the "Software"), to deal in the Software without restriction,
> including without limitation the rights to use, copy, modify, merge, publish, distribute,
> sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
>
> The above copyright notice and this permission notice shall be included in all copies or
> substantial portions of the Software.
>
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
> NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
> NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
> DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
> OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

Both licences permit commercial distribution, with attribution required in the distributed
materials rather than inside the application UI. That is what this file provides.

## Also in this directory

`docs/rpg-awesome-ref.html` is the reference for **RPG Awesome**, the font the app ships today
(`Resources/Fonts/rpgawesome-webfont.ttf`) for thematic icons — crossed swords, scrolls, and the
like. RPG Awesome is not being replaced. The designer set is additive: thematic icons keep RPG
Awesome, design tools get Phosphor.
