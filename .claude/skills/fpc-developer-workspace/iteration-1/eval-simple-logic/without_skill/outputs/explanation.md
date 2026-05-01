# FPC-Programm: Einfache Förderanlage – Erklärung

## Überblick

Das Programm steuert eine einfache Förderanlage mit Selbsthaltung, Stop-Bedingungen und einer blinkenden Warnleuchte.

## I/O-Belegung

| Signal | Typ | Beschreibung |
|--------|-----|--------------|
| I0 | Eingang | Start-Taster |
| I1 | Eingang | Stop-Taster |
| I2 | Eingang | Lichtschranke |
| O0 | Ausgang | Fördermotor |
| O1 | Ausgang | Warnleuchte (2 Hz Blinken) |
| M0 | Memory | Selbsthaltung Motor |
| T0 | Timer | 2-Hz-Blinktakt (250 ms) |

## Programmablauf

### 1. Selbsthaltungslogik

```
GET I 0     # Start-Taster auf Stack
GET M 0     # Selbsthaltungs-Flag auf Stack
OR          # Start ODER Selbsthaltung
GETNOT I 1  # Stop-Taster negiert (0 wenn gedrückt)
AND         # Verknüpfung mit Abschaltbedingung Stop
GETNOT I 2  # Lichtschranke negiert (0 wenn ausgelöst)
AND         # Verknüpfung mit Abschaltbedingung Lichtschranke
MOV M 0     # Ergebnis in Selbsthaltungs-Memory schreiben
```

Die Logik entspricht: `M0 = (I0 OR M0) AND NOT I1 AND NOT I2`

- **Einschalten:** Start-Taster (I0) setzt M0 auf 1
- **Selbsthaltung:** M0 hält sich über den OR-Zweig selbst aktiv
- **Ausschalten:** Stop-Taster (I1) oder Lichtschranke (I2) unterbrechen die Kette → M0 wird 0

### 2. Motor-Ausgang

```
GET M 0
MOV O 0
```

Der Motor (O0) folgt direkt dem Selbsthaltungs-Memory M0.

### 3. Warnleuchte mit 2-Hz-Blinken

```
CSET T 0 250   # Timer T0 nur setzen wenn Stack-Top = 1 (hier immer aktiv)
GET M 0        # Motor-Status
GET T 0        # Timer-Puls (250 ms an / 250 ms aus)
AND            # Blinken nur wenn Motor läuft
MOV O 1        # Warnleuchte setzen
```

Timer T0 ist ein pulsierender Timer mit 250 ms Periode: 250 ms EIN, 250 ms AUS → 2 Hz Frequenz. Die Warnleuchte leuchtet nur, wenn der Motor läuft (M0 = 1) UND der Timer gerade im EIN-Zustand ist.

## Hinweis zu CSET

`CSET T 0 250` setzt den Timer bedingt – da in diesem Kontext der Stack-Top immer 1 ist (der Timer soll dauerhaft laufen), läuft T0 kontinuierlich. Die 2-Hz-Maskierung erfolgt erst durch die AND-Verknüpfung mit M0.
