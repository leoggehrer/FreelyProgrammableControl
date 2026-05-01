Erstelle eine Ampelsteuerung mit Gegenampel mit folgendem Verhalten:

Eingang...I
Ausgang...O

### Hauptampel

| Zeit in Sekunden | I 0 (NOTAUS) | O 0 (ROT) | O 1 (GELB) | O 2 (GRÜN) |
| ---------------- | ------------ | --------- | ---------- | ----------- |
| beliebig         | 0            | 1         | 0          | 0           |
| 0 bis 10         | 1            | 1         | 0          | 0           |
| 10 bis 13        | 1            | 1         | 1          | 0           |
| 13 bis 20        | 1            | 0         | 0          | 1           |
| 20 bis 20.5      | 1            | 0         | 0          | 0           |
| 20.5 bis 21      | 1            | 0         | 0          | 1           |
| 21 bis 21.5      | 1            | 0         | 0          | 0           |
| 21.5 bis 22      | 1            | 0         | 0          | 1           |
| 22 bis 22.5      | 1            | 0         | 0          | 0           |
| 22.5 bis 23      | 1            | 0         | 0          | 1           |
| 23 bis 25        | 1            | 0         | 1          | 0           |

### Gegenampel

| Zeit in Sekunden | I 0 (NOTAUS) | O 3 (GEGEN-ROT) | O 4 (GEGEN-GELB) | O 5 (GEGEN-GRÜN) |
| ---------------- | ------------ | --------------- | ---------------- | ---------------- |
| beliebig         | 0            | 1               | 0                | 0                |
| 0 bis 10         | 1            | 0               | 0                | 1                |
| 10 bis 13        | 1            | 0               | 1                | 0                |
| 13 bis 23        | 1            | 1               | 0                | 0                |
| 23 bis 25        | 1            | 1               | 1                | 0                |

**Setze:** 
- Eingang 0...NOTAUS
- Ausgang 0...Rot, Ausgang 1...Gelb, Ausgang 2...Grün
- Ausgang 3...Gegen-Rot, Ausgang 4...Gegen-Gelb, Ausgang 5...Gegen-Grün

ACHTUNG: Nach der Aktivierung vom NOTAUS startet die ZEIT wieder von Beginn an!

Dieser Ablauf wiederholt sich immer wieder!

Teste das Programm ausführlich!

---

## FPC-Programm (mit Gegenampel)

```fpc
# ============================================================
# Ampelsteuerung mit Gegenampel
# I0=NOTAUS (0=Notaus aktiv), O0=Rot, O1=Gelb, O2=Gruen
# O3=Gegen-Rot, O4=Gegen-Gelb, O5=Gegen-Gruen
# T10=100ms Takt, M10=T10-Init, M11=C0-Init
# C0: Zykluszaehler 1..250 (1 Tick = 100ms)
# ============================================================

# --- Block 1: Takt-Timer T10 einmalig initialisieren (100ms) ---
GETNOT M 10
DUP
CSET T 10 100
CMOV M 10 1

# --- Block 2: Counter C0 einmalig auf 1 setzen ---
GETNOT M 11
DUP
CSET C 0 1
CMOV M 11 1

# --- Block 3: C0 bei jedem Takt-Impuls erhoehen ---
GET T 10
CINC C 0

# --- Block 4: NOTAUS-Erkennung (I0=0 bedeutet NOTAUS aktiv) ---
# Bei NOTAUS: Counter und Init-Flags zuruecksetzen
GETNOT I 0
DUP
CMOV M 10 0
DUP
CMOV M 11 0
CSET C 0 0

# --- Block 5: Reset C0 nach 250 Ticks ---
GT C 0 250
CSET C 0 1

# ============================================================
# HAUPTAMPEL-AUSGAENGE
# ============================================================

# --- O0: ROT ---
# Phase 1-100 (0-10s): Rot
# Phase 101-130 (10-13s): Rot+Gelb
# NOTAUS: Rot
GT C 0 0
LE C 0 130
AND
GETNOT I 0
OR
MOV O 0

# --- O1: GELB ---
# Phase 101-130 (10-13s): Gelb
# Phase 231-250 (23-25s): Gelb
# NOTAUS: kein Gelb
GT C 0 100
LE C 0 130
AND
GT C 0 230
LE C 0 250
AND
OR
GET I 0
AND
MOV O 1

# --- O2: GRUEN ---
# Phase 131-200 (13-20s): Gruen
# Phase 206-210, 216-220, 226-230 (Blinken)
# NOTAUS: kein Gruen
GT C 0 130
LE C 0 200
AND
GT C 0 205
LE C 0 210
AND
OR
GT C 0 215
LE C 0 220
AND
OR
GT C 0 225
LE C 0 230
AND
OR
GET I 0
AND
MOV O 2

# ============================================================
# GEGENAMPEL-AUSGAENGE
# ============================================================

# --- O3: GEGEN-ROT ---
# Phase 131-250 (13-25s): Rot
# NOTAUS: Rot
GT C 0 130
GETNOT I 0
OR
MOV O 3

# --- O4: GEGEN-GELB ---
# Phase 101-130 (10-13s): Gelb
# Phase 231-250 (23-25s): Gelb
# NOTAUS: kein Gelb
GT C 0 100
LE C 0 130
AND
GT C 0 230
LE C 0 250
AND
OR
GET I 0
AND
MOV O 4

# --- O5: GEGEN-GRUEN ---
# Phase 1-100 (0-10s): Gruen
# NOTAUS: kein Gruen
GT C 0 0
LE C 0 100
AND
GET I 0
AND
MOV O 5
```

## Ressourcen

| Ressource | Index | Verwendung |
|-----------|-------|------------|
| T 10 | Timer 10 | 100ms Tick-Generator |
| M 10 | Memory 10 | T10-Init-Flag |
| M 11 | Memory 11 | C0-Init-Flag |
| C 0 | Counter 0 | Phasenzähler (1–250 = 25 Sekunden) |

## Phasen-Mapping (Counter C0)

| Counter | Zeit | ROT | GELB | GRÜN | G-ROT | G-GELB | G-GRÜN |
|---------|------|-----|------|------|-------|--------|--------|
| 1–100 | 0–10s | 1 | 0 | 0 | 0 | 0 | 1 |
| 101–130 | 10–13s | 1 | 1 | 0 | 0 | 1 | 0 |
| 131–200 | 13–20s | 0 | 0 | 1 | 1 | 0 | 0 |
| 201–205 | 20–20.5s | 0 | 0 | 0 | 1 | 0 | 0 |
| 206–210 | 20.5–21s | 0 | 0 | 1 | 1 | 0 | 0 |
| 211–215 | 21–21.5s | 0 | 0 | 0 | 1 | 0 | 0 |
| 216–220 | 21.5–22s | 0 | 0 | 1 | 1 | 0 | 0 |
| 221–225 | 22–22.5s | 0 | 0 | 0 | 1 | 0 | 0 |
| 226–230 | 22.5–23s | 0 | 0 | 1 | 1 | 0 | 0 |
| 231–250 | 23–25s | 0 | 1 | 0 | 1 | 1 | 0 |

NOTAUS (I0=0): ROT=1, GEGEN-ROT=1, alle anderen=0, Counter wird auf 0 zurückgesetzt.
