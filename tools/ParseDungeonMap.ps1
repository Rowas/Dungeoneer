# Wrapper — kör huvudskriptet i Cursor OCR Script.
param(
    [string]$ImagePath,
    [string]$OutputPath,
    [int]$Cols = 51,
    [int]$Rows = 21,
    [int]$LuminanceThreshold = 128,
    [int]$CellMargin = 2,
    [switch]$NoTrimRedundantWalls
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$mainScript = Join-Path (Split-Path -Parent $scriptDir) 'Cursor OCR Script\ParseDungeonMap.ps1'

$params = @{
    ImagePath            = $ImagePath
    OutputPath           = $OutputPath
    Cols                 = $Cols
    Rows                 = $Rows
    LuminanceThreshold   = $LuminanceThreshold
    CellMargin           = $CellMargin
}

if ($NoTrimRedundantWalls) {
    $params.NoTrimRedundantWalls = $true
}

& $mainScript @params
