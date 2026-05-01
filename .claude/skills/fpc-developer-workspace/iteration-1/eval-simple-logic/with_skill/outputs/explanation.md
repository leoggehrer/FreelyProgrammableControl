# Foerderanlage – Programmerklärung

## Belegung

| Signal | Bedeutung |
|--------|-----------|
| I0 | Start-Taster |
| I1 | Stop-Taster |
| I2 | Lichtschranke |
| O0 | Fördermotor |
| O1 | Warnleuchte (blinkt 2 Hz) |
| M0 | Motormerker (Selbsthaltung) |
| M1 | Timer-Initialisierungsflag |
| T0 | Blinker-Timer 250 ms |

## Programmstruktur

### 1. Blinker-Timer initialisieren (einmalig)

```fpc
GETNOT M 1
DUP
CSET T 0 250
CMOV M 1 1
```

`M1` ist beim ersten Zyklus `false`, daher ist `GETNOT M1 = true`. Der Timer T0 wird mit 250 ms gestartet (2 Hz = 500 ms Periode, jeweils 250 ms EIN / 250 ms AUS). Danach wird `M1=1` gesetzt, sodass der Timer-Start in allen folgenden Zyklen übersprungen wird. Das ist das verbindliche Timer-Init-Muster: **Memory-Flag + DUP + CSET + CMOV**.

### 2. SR-Latch – Motormerker M0

```fpc
GET I 0
CMOV M 0 1

GET I 1
GET I 2
OR
CMOV M 0 0
```

- **Set:** Wird I0 (Start) betätigt, setzt `CMOV M0 1` den Motormerker.
- **Reset (hat Vorrang):** I1 (Stop) ODER I2 (Lichtschranke) werden per `OR` verknüpft. Ist einer davon aktiv, wird `M0` auf 0 zurückgesetzt. Da Reset nach Set steht, hat er Vorrang – drückt man Start und Stop gleichzeitig, bleibt der Motor aus.

### 3. Fördermotor O0

```fpc
GET M 0
MOV O 0
```

Der Ausgang O0 folgt direkt dem Motormerker M0.

### 4. Warnleuchte O1 (gated Blinker)

```fpc
GET M 0
GET T 0
AND
MOV O 1
```

O1 ist nur dann aktiv, wenn sowohl der Motor läuft (`M0=true`) als auch der Blinker-Timer gerade in der EIN-Phase ist (`T0=true`). Das ergibt ein 2-Hz-Blinken, das nur während des Motorbetriebs sichtbar ist.

## Zeitverhalten

- Blinker-Periode: 500 ms (250 ms EIN + 250 ms AUS) → 2 Hz
- Zyklusziet: Standard 100 ms (ausreichend für präzises Timing)

## Stack-Verifikation

Alle Sequenzen enden mit einem leeren Stack (Tiefe = 0). Kein `DUP`-Missbrauch, kein Stack-Unterlauf.
