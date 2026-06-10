<#
.SYNOPSIS
    Parsar en dungeon-kartbild till en ASCII level-fil.

.DESCRIPTION
    Delar upp bilden i ett rutnät (t.ex. 51x21) där varje ruta motsvarar en vit
    ruta i bilden. Regler:
      - 100 % svart i rutan  -> '#'
      - Allt annat           -> '.'  (golv, siffror, bokstäver, streckade dörrar)
      - Ytterram (rad 0, sista rad, kol 0, sista kol) tvingas till '#'

    Grå rutlinjer ligger på kanterna mellan rutor. CellMargin exkluderar dessa
    så att väggar inte felaktigt blir golv.

    Efter parsning tas överflödiga väggar bort: en '#' som i alla 8 riktningar
    bara gränsar till '#' eller kant ersätts med blanksteg.

.EXAMPLE
    .\ParseDungeonMap.ps1 `
        -ImagePath "C:\path\to\map.png" `
        -OutputPath "..\Dungeoneer\Content\LevelFiles\Level4-proposal.txt" `
        -Cols 51 -Rows 21

.EXAMPLE
    # Större karta (t.ex. Level 5)
    .\ParseDungeonMap.ps1 `
        -ImagePath "C:\path\to\map.png" `
        -OutputPath "..\Dungeoneer\Content\LevelFiles\Level5-proposal.txt" `
        -Cols 51 -Rows 39
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$ImagePath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [int]$Cols = 51,
    [int]$Rows = 21,

    # Pixel med luminans >= tröskel räknas som icke-svart.
    [int]$LuminanceThreshold = 128,

    # Pixlar att hoppa över på varje rutkant (exkluderar grå rutlinjer).
    [int]$CellMargin = 2,

    # Hoppa över borttagning av överflödiga väggar (internt #-block).
    [switch]$NoTrimRedundantWalls
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Test-CellIsAllDark {
    param(
        [System.Drawing.Bitmap]$Bitmap,
        [int]$Col,
        [int]$Row,
        [double]$CellWidth,
        [double]$CellHeight,
        [int]$Margin,
        [int]$Threshold
    )

    $x0 = [int]($Col * $CellWidth) + $Margin
    $x1 = [int](($Col + 1) * $CellWidth) - $Margin
    $y0 = [int]($Row * $CellHeight) + $Margin
    $y1 = [int](($Row + 1) * $CellHeight) - $Margin

    if ($x1 -le $x0 -or $y1 -le $y0) {
        return $false
    }

    for ($y = $y0; $y -lt $y1; $y++) {
        for ($x = $x0; $x -lt $x1; $x++) {
            $pixel = $Bitmap.GetPixel($x, $y)
            $luminance = ($pixel.R + $pixel.G + $pixel.B) / 3
            if ($luminance -ge $Threshold) {
                return $false
            }
        }
    }

    return $true
}

function Get-GridCell {
    param(
        [char[][]]$Grid,
        [int]$Col,
        [int]$Row,
        [int]$Rows,
        [int]$Cols
    )

    if ($Col -lt 0 -or $Row -lt 0 -or $Col -ge $Cols -or $Row -ge $Rows) {
        return '#'
    }

    return $Grid[$Row][$Col]
}

function Remove-RedundantWalls {
    param(
        [char[][]]$Grid,
        [int]$Rows,
        [int]$Cols
    )

    $directions = @(
        @(-1, -1), @(-1, 0), @(-1, 1),
        @(0, -1),           @(0, 1),
        @(1, -1),  @(1, 0),  @(1, 1)
    )

    $removed = 0
    for ($row = 0; $row -lt $Rows; $row++) {
        for ($col = 0; $col -lt $Cols; $col++) {
            if ($Grid[$row][$col] -ne '#') { continue }

            $touchesFloor = $false
            foreach ($dir in $directions) {
                if ((Get-GridCell -Grid $Grid -Col ($col + $dir[0]) -Row ($row + $dir[1]) -Rows $Rows -Cols $Cols) -eq '.') {
                    $touchesFloor = $true
                    break
                }
            }

            if (-not $touchesFloor) {
                $Grid[$row][$col] = ' '
                $removed++
            }
        }
    }

    return $removed
}

function Set-BorderWalls {
    param(
        [char[][]]$Grid,
        [int]$Rows,
        [int]$Cols
    )

    $lastRow = $Rows - 1
    $lastCol = $Cols - 1

    for ($col = 0; $col -lt $Cols; $col++) {
        $Grid[0][$col] = '#'
        $Grid[$lastRow][$col] = '#'
    }
    for ($row = 0; $row -lt $Rows; $row++) {
        $Grid[$row][0] = '#'
        $Grid[$row][$lastCol] = '#'
    }
}

if (-not (Test-Path -LiteralPath $ImagePath)) {
    throw "Bildfilen hittades inte: $ImagePath"
}

$outputDir = Split-Path -Parent $OutputPath
if ($outputDir -and -not (Test-Path -LiteralPath $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

Add-Type -AssemblyName System.Drawing

$bmp = [System.Drawing.Bitmap]::FromFile($ImagePath)
try {
    $cellWidth = $bmp.Width / $Cols
    $cellHeight = $bmp.Height / $Rows

    Write-Host "Bild: $($bmp.Width)x$($bmp.Height) px -> rutnät ${Cols}x${Rows} (${cellWidth:N1}x${cellHeight:N1} px/ruta)"

    $grid = New-Object 'char[][]' $Rows
    for ($row = 0; $row -lt $Rows; $row++) {
        $grid[$row] = New-Object char[] $Cols
        for ($col = 0; $col -lt $Cols; $col++) {
            $grid[$row][$col] = if (Test-CellIsAllDark -Bitmap $bmp -Col $col -Row $row -CellWidth $cellWidth -CellHeight $cellHeight -Margin $CellMargin -Threshold $LuminanceThreshold) {
                '#'
            } else {
                '.'
            }
        }
    }

    Set-BorderWalls -Grid $grid -Rows $Rows -Cols $Cols

    $trimmed = 0
    if (-not $NoTrimRedundantWalls) {
        $trimmed = Remove-RedundantWalls -Grid $grid -Rows $Rows -Cols $Cols
    }

    $lines = for ($row = 0; $row -lt $Rows; $row++) { -join $grid[$row] }
    [System.IO.File]::WriteAllLines($OutputPath, $lines)

    $chars = ($lines -join '').ToCharArray()
    $wallCount = ($chars | Where-Object { $_ -eq '#' } | Measure-Object).Count
    $floorCount = ($chars | Where-Object { $_ -eq '.' } | Measure-Object).Count
    $emptyCount = ($chars | Where-Object { $_ -eq ' ' } | Measure-Object).Count
    Write-Host "Klart: $OutputPath ($Rows rader x $Cols kolumner, $wallCount väggar, $floorCount golv, $emptyCount tomma, $trimmed överflödiga # borttagna)"
}
finally {
    $bmp.Dispose()
}
