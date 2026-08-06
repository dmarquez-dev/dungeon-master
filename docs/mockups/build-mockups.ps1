<#
.SYNOPSIS
    Builds the design mockups in this folder into self-contained HTML files.

.DESCRIPTION
    Each *.src.html here is a design POC that references the app's real fonts via
    __FONT_*__ placeholders. This script substitutes each placeholder with the
    base64 of the matching font from the App project and writes the result to
    dist/, which is gitignored.

    Keeping the sources tracked and the built output ignored means the repo
    carries ~25 KB of readable HTML instead of ~1.4 MB of embedded font data,
    and the mockups stay in sync with the real Resources/Fonts if they change.

.EXAMPLE
    pwsh docs/mockups/build-mockups.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$fontDir = Join-Path $here '..\..\src\DungeonMaster.App\Resources\Fonts'
$distDir = Join-Path $here 'dist'

$fontMap = @{
    '__FONT_CINZEL__'      = 'Cinzel-Regular.ttf'
    '__FONT_CINZEL_SB__'   = 'Cinzel-SemiBold.ttf'
    '__FONT_GARAMOND__'    = 'EBGaramond-Regular.ttf'
    '__FONT_GARAMOND_IT__' = 'EBGaramond-Italic.ttf'
    '__FONT_RPG__'         = 'rpgawesome-webfont.ttf'
}

# Read each font once — several mockups may share the same placeholders.
$encoded = @{}
foreach ($key in $fontMap.Keys) {
    $path = Join-Path $fontDir $fontMap[$key]
    if (-not (Test-Path $path)) { throw "Font not found: $path" }
    $encoded[$key] = [Convert]::ToBase64String([IO.File]::ReadAllBytes($path))
}

New-Item -ItemType Directory -Force -Path $distDir | Out-Null

$sources = Get-ChildItem -Path $here -Filter '*.src.html'
if (-not $sources) { throw "No *.src.html mockups found in $here" }

foreach ($src in $sources) {
    $html = [IO.File]::ReadAllText($src.FullName)

    foreach ($key in $encoded.Keys) {
        $html = $html.Replace($key, $encoded[$key])
    }

    # A leftover placeholder means the mockup asked for a font this script
    # does not know about — fail loudly rather than shipping a broken page.
    if ($html -match '__FONT_[A-Z_]+__') {
        throw "$($src.Name) references an unmapped font placeholder: $($Matches[0])"
    }

    $outName = $src.Name -replace '\.src\.html$', '.html'
    $outPath = Join-Path $distDir $outName
    [IO.File]::WriteAllText($outPath, $html, (New-Object Text.UTF8Encoding $false))

    '{0} -> dist/{1} ({2:N0} KB)' -f $src.Name, $outName, ((Get-Item $outPath).Length / 1KB)
}
