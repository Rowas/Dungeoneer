# Dungeon-karta: bild → ASCII level-fil

Referens för att generera nya kartfiler från rutnätsbilder.

## Regler

| Symbol | Betydelse |
|--------|-----------|
| `#` | Vägg — rutan är **100 % svart**, och gränsar till golv |
| `.` | Golv — allt annat (vitt, siffror, bokstäver, streckade dörrar) |
| ` ` (blanksteg) | Internt väggblock — `#` som i alla 8 riktningar bara gränsar till `#` eller kant |

- Rutnätet ska matcha bildens **vita rutor** (kolumner × rader).
- **Ytterram** (första/sista rad, första/sista kolumn) ska alltid vara `#`.
- Grå rutlinjer mellan rutor ignoreras via `CellMargin` (standard: 2 px).

## Snabbstart

```powershell
cd "D:\Dev\Source\Private\Repos\Dungeoneer\Cursor OCR Script"

.\ParseDungeonMap.ps1 `
    -ImagePath "C:\sökväg\till\karta.png" `
    -OutputPath "..\Dungeoneer\Content\LevelFiles\Level4-proposal.txt" `
    -Cols 51 `
    -Rows 21
```

## Parametrar

| Parameter | Standard | Beskrivning |
|-----------|----------|-------------|
| `ImagePath` | — | PNG/JPG med dungeon-kartan |
| `OutputPath` | — | Målfil, t.ex. `LevelFiles\LevelX-proposal.txt` |
| `Cols` | 51 | Antal kolumner |
| `Rows` | 21 | Antal rader |
| `LuminanceThreshold` | 128 | Gräns svart/vitt (0–255) |
| `CellMargin` | 2 | Pixlar att exkludera på rutkant |
| `NoTrimRedundantWalls` | av | Sätt flaggan för att behålla alla `#` utan trimning |

## Kända kartstorlekar

| Level | Kolumner | Rader | Exempelfil |
|-------|----------|-------|------------|
| 4 | 51 | 21 | `Level4-proposal.txt` |
| 5 | 51 | 39 | `Level5-proposal.txt` |

## Algoritm (steg för steg)

1. Läs bildens pixelstorlek (`Width` × `Height`).
2. Beräkna rutorstorlek: `cellWidth = Width / Cols`, `cellHeight = Height / Rows`.
3. För varje ruta `(col, row)`:
   - Ta pixelområdet inuti rutan (minus `CellMargin` på kanterna).
   - Om **alla** pixlar har luminans &lt; 128 → `#`.
   - Annars → `.`.
4. Tvinga ytterram till `#`.
5. Ta bort överflödiga `#`: om ingen av 8 grannar är `.` (kant räknas som `#`) → blanksteg.
6. Skriv en rad per rad till textfil (en tecken per kolumn).

## Efter parsning

Parsningen lägger **inte** till spelarinnehåll. Det görs manuellt i level-filen:

- `@` — spelarstart
- `G`, `W`, `B`, `E` — fiender/objekt (se befintliga levels)
- `r`, `b`, `P` — props

Validera alltid:

- Alla rader har samma längd som `Cols`.
- Antal rader = `Rows`.
- Ytterram är `#`.
- Dörrar (streckade linjer i bilden) blev `.`.

## Felsökning

| Problem | Lösning |
|---------|---------|
| Väggar blev golv | Öka `CellMargin` (t.ex. 3) |
| Golv blev väggar | Sänk `LuminanceThreshold` (t.ex. 100) eller minska `CellMargin` |
| Siffror blev `#` | Förväntat om siffran fyller hela rutan med svart; sätt manuellt till `.` om önskat |
| Fel rutnätsstorlek | Räkna vita rutor i bilden, uppdatera `-Cols` / `-Rows` |
