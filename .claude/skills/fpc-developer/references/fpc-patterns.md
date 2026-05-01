# FPC Pattern-Bibliothek

> Vollständige kuratierte Beispielsammlung mit Testfällen und Tags:
> **`/FPCSamples.md`** im Projektverzeichnis (bei komplexen oder unbekannten Anforderungen nachlesen!)

## Grundlegende Logik

### Pass-Through
```
GET I 0
MOV O 0
```

### AND / OR / XOR / NAND
```
# AND: O0 = I0 AND I1
GET I 0
GET I 1
AND
MOV O 0

# OR: O0 = I0 OR I1
GET I 0
GET I 1
OR
MOV O 0

# NAND: O0 = NOT (I0 AND I1)
GET I 0
GET I 1
AND
NOT
MOV O 0
```

### Mehrere Outputs (DUP)
```
# O0 und O1 = I0 AND I1
GET I 0
GET I 1
AND
DUP
MOV O 0
MOV O 1
```

### Sicherheitsabschaltung (Not-Aus Vorrang)
```
# O0 nur aktiv wenn I0 UND NICHT I1 (Not-Aus)
GET I 0
GETNOT I 1
AND
MOV O 0
```

---

## Timer und Blinker

### Einfacher Blinker (1 Hz, 500ms)
```
# Timer einmalig initialisieren
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1
# Output folgt Timer
GET T 0
MOV O 0
```

### Eingangsgesteuerter Blinker
```
# Timer läuft permanent, Output nur bei I0=true
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1
# Gating: Output = I0 AND Timer
GET I 0
GET T 0
AND
MOV O 0
```

### Zwei unabhängige Blinker
```
# Blinker 1: O0 mit 1 Hz (T0, M0)
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1
GET T 0
MOV O 0
# Blinker 2: O1 mit 2 Hz (T1, M1)
GETNOT M 1
DUP
CSET T 1 250
CMOV M 1 1
GET T 1
MOV O 1
```

### Einschaltverzögerung TON (5 Sekunden)
```
# Reset wenn I1 inaktiv
GETNOT I 1
DUP
CMOV M 0 0
CMOV M 1 0
# Timer starten bei steigender Flanke von I1
GET I 1
GETNOT M 0
AND
DUP
CMOV M 0 1
CSET T 0 5000
# Merker setzen wenn Verzögerung abgelaufen
GET M 0
GETNOT T 0
AND
CMOV M 1 1
# Output folgt Merker
GET M 1
MOV O 1
```

### Ausschaltverzögerung TOF (5 Sekunden)
```
# Merker setzen wenn Eingang aktiv
GET I 1
CMOV M 0 1
# Timer-Reset wenn kein Merker und kein Timer
GETNOT M 0
GETNOT T 0
AND
CSET T 0 0
# Timer starten bei fallender Flanke
GETNOT I 1
GET M 0
AND
DUP
CMOV M 0 0
CSET T 0 5000
# Output = Eingang ODER Timer
GET I 1
GET T 0
OR
MOV O 1
```

---

## Counter-Muster

### Einfacher Aufwärtszähler
```
# Inkrementieren bei I0 (ACHTUNG: zählt jeden Zyklus!)
GET I 0
CINC C 0
# Output wenn C0 == 10
CMP C 0 10
MOV O 0
```

### Flankengesteuerter Zähler
```
# Flanke erkennen: I0=true UND M0=false (vorher war I0 false)
GET I 0
GETNOT M 0
AND
CINC C 0
# Merker aktualisieren: M0 = I0
GET I 0
DUP
CMOV M 0 1
NOT
CMOV M 0 0
# Reset mit I1
GET I 1
CSET C 0 0
# Output wenn C0 > 4 (also >= 5)
GT C 0 4
MOV O 0
```

### Bereichsprüfung
```
# O0 = true wenn 11 <= C0 <= 20
GT C 0 10
LE C 0 20
AND
MOV O 0
```

---

## Speichernde Logik

### SR-Latch (Reset hat Vorrang)
```
# I0=Set, I1=Reset
GET I 0
CMOV M 0 1
# Reset (kommt NACH Set → hat Vorrang!)
GET I 1
CMOV M 0 0
# Output folgt Merker
GET M 0
MOV O 0
```

### Selbsthaltung Start/Stop
```
# I0=Start, I1=Stop
GET I 0
CMOV M 0 1
GET I 1
CMOV M 0 0
GET M 0
MOV O 0
```

---

## Taktgeber mit Ablaufsteuerung

### Taktgeber + Counter + Phasen (Ampel-Basis)
```
# Timer T10 einmalig starten (100ms Takt)
GETNOT M 10
DUP
CSET T 10 100
CMOV M 10 1

# Counter C0 einmalig auf 1 setzen
GETNOT M 11
DUP
CSET C 0 1
CMOV M 11 1

# C0 bei jedem Takt erhöhen
GET T 10
CINC C 0

# Reset nach 250 Ticks (25 Sekunden)
GT C 0 250
CSET C 0 1

# Phase 1 (Ticks 1–100, 0–10s): O0
GT C 0 0
LE C 0 100
AND
MOV O 0

# Phase 2 (Ticks 101–130, 10–13s): O1
GT C 0 100
LE C 0 130
AND
MOV O 1

# Phase 3 (Ticks 131–200, 13–20s): O2
GT C 0 130
LE C 0 200
AND
MOV O 2
```

---

### Zeitgesteuerter Output – Abschaltung nach N Timer-Perioden

Output ist AN ab Programmstart und schaltet automatisch nach N vollständigen Timer-Perioden ab.
Verwendet M2/M3 als Flanken-Merker zum Erkennen vollständiger Perioden.

```
# O0 aktiv für 10 Timer-Perioden (Timer 1000ms → Periode 2s → 20s Gesamtlaufzeit)
# M0=Init-Flag, M2=T0-AN-Flanke, M3=T0-AUS-Flanke, C0=Periodenzähler

# Block 1: Timer einmalig initialisieren
GETNOT M 0
DUP
CSET T 0 1000
CMOV M 0 1

# Block 2: Flanken erfassen
GET T 0
CMOV M 2 1
GETNOT T 0
CMOV M 3 1

# Block 3: Vollständige Periode zählen (M2 AND M3 = beide Flanken gesehen)
GET M 2
GET M 3
AND
DUP
CINC C 0
DUP
CMOV M 2 0
CMOV M 3 0

# Block 4: Output aktiv solange C0 < 10 Perioden
GET M 0
LE C 0 10
AND
MOV O 0
```

Testfälle: C0=0→O0=T, C0=9→O0=T, C0=10→O0=F
Anpassung: `CSET T 0 500` für 1s-Perioden, `LE C 0 30` für 30 Perioden.

---

## Anti-Patterns (NIEMALS verwenden!)

### Timer in der Hauptschleife ohne Memory-Flag
```
# FALSCH! Timer wird jeden Zyklus neu gestartet → kein Blinken!
SET T 0 500
GET T 0
MOV O 0
```

### Timer-Init ohne DUP (Stack-Unterlauf)
```
# FALSCH! CSET verbraucht Wert, CMOV findet leeren Stack!
GETNOT M 0
CSET T 0 500   # verbraucht Stack-Wert
CMOV M 0 1     # Stack leer → Fehler!
```

### Inline-Kommentar
```
GET I 0  # FALSCH! Führt zu Parse-Fehler
```
