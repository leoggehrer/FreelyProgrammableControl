# FPC – Vector Store Example Set (Kuratierte Pattern-Sammlung)

Diese Sammlung enthält grundlegende und fortgeschrittene Muster für die FreelyProgrammableControl (FPC) Stack-Maschine.
Jeder Abschnitt stellt ein in sich geschlossenes Pattern dar.

---

## Kategorie: Grundlagen (Einfach)

---

### AND-Verknüpfung zweier Inputs

**Beschreibung**: Dieses Pattern zeigt die grundlegende binäre Verknüpfung zweier Eingangssignale innerhalb der Stack-Maschine. Es demonstriert den typischen Push-Push-Operate-Pop-Fluss.
**Anwendung**: Grundlegende logische Verknüpfung

```
# AND-Verknüpfung: Output 0 = Input 0 AND Input 1
# Stack: [] -> [I0] -> [I0,I1] -> [I0&&I1] -> []
GET I 0
GET I 1
AND
MOV O 0
```

---

### OR-Verknüpfung (Logisches ODER)

**Beschreibung**: Output ist AN wenn mindestens ein Input AN ist.
**Anwendung**: Mehrere Taster für eine Funktion, Parallelschaltung

```
# OR-Verknüpfung: Output 0 = Input 0 OR Input 1
# Stack: [] -> [I0] -> [I0,I1] -> [I0||I1] -> []
GET I 0
GET I 1
OR
MOV O 0
```

---

### Negation eines Inputs

**Beschreibung**: Dieses Muster demonstriert die Verwendung eines unären logischen Operators auf dem Stack.
**Anwendung**: Invertierung eines Eingangssignals

```
# Invertierung: Output 0 = NOT Input 0
# Stack: [] -> [I0] -> [!I0] -> []
GET I 0
NOT
MOV O 0
```

---

### XOR-Verknüpfung (Exklusiv-ODER)

**Beschreibung**: Output ist AN wenn genau ein Input AN ist.
**Anwendung**: Wechselschaltung (Licht), Zustandsänderungserkennung

```
# XOR-Verknüpfung: Output 0 = Input 0 XOR Input 1
# Stack: [] -> [I0] -> [I0,I1] -> [I0^I1] -> []
GET I 0
GET I 1
XOR
MOV O 0
```

---

### NAND-Verknüpfung (Negiertes UND)

**Beschreibung**: Output ist AUS nur wenn beide Inputs AN sind.
**Anwendung**: Basis für komplexe Logik

```
# NAND-Verknüpfung: Output 0 = NOT (Input 0 AND Input 1)
# Stack: [] -> [I0] -> [I0,I1] -> [I0&&I1] -> [!(I0&&I1)] -> []
GET I 0
GET I 1
AND
NOT
MOV O 0
```

---

### NOR-Verknüpfung (Negiertes ODER)

**Beschreibung**: Output ist AN nur wenn beide Inputs AUS sind.
**Anwendung**: Ruhe-/Störungszustand

```
# NOR-Verknüpfung: Output 0 = NOT (Input 0 OR Input 1)
# Stack: [] -> [I0] -> [I0,I1] -> [I0||I1] -> [!(I0||I1)] -> []
GET I 0
GET I 1
OR
NOT
MOV O 0
```

---

### Drei-Input AND-Verknüpfung

**Beschreibung**: Output nur AN wenn alle drei Inputs AN sind.
**Anwendung**: Drei-Hand-Bedienung, Mehrfachbedingung

```
# Drei-Input AND: Output 0 = Input 0 AND Input 1 AND Input 2
# Stack: [] -> [I0] -> [I0,I1] -> [I0&&I1] -> [I0&&I1,I2] -> [I0&&I1&&I2] -> []
GET I 0
GET I 1
AND
GET I 2
AND
MOV O 0
```

---

### Bedingtes Setzen eines Outputs (CMOV)

**Beschreibung**: Dieses Pattern zeigt die bedingte Ausführung einer Aktion auf Basis des obersten Stack-Wertes.
**Anwendung**: Aktion nur bei erfüllter Bedingung

```
# Wenn I0 true → O1 = 1
GET I 0
CMOV O 1 1
```

---

### Bedingtes Setzen eines Timers (CSET)

**Beschreibung**: Dieses Pattern zeigt die bedingte Ausführung einer Aktion auf Basis des obersten Stack-Wertes.
**Anwendung**: Aktion nur bei erfüllter Bedingung

```
# Wenn I0 true → O1 = 1
GET I 0
CSET T 0 500
```

---

## Kategorie: Timer und Blinker (Mittel)

---

### Einfacher Blinker (1 Hz)

**Beschreibung**: Output blinkt kontinuierlich mit 1 Hz (500ms AN, 500ms AUS).
**Anwendung**: Statusanzeige, Warnblinker

```
# Blinker mit 1 Hz (500ms an/aus)
# Memory 0 verhindert Timer-Neustart

# Timer nur beim ersten Zyklus starten
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1

# Output folgt Timer-Zustand
GET T 0
MOV O 0
```

---

### Eingangsgesteuerter Blinker (1 Hz nur bei I0)

**Beschreibung**: Output 0 blinkt mit 1 Hz, aber nur solange Input 0 aktiv ist. Der Timer wird einmalig initialisiert und danach nur noch als Signalquelle verwendet.
**Anwendung**: Aktivierungsabhängige Blinkanzeige

```
# Output 0 blinkt mit 1 Hz, nur wenn Input 0 = true
# WICHTIG: Timer nicht im Eingangspfad zyklisch neu setzen

# Timer 0 einmalig initialisieren (500 ms an / 500 ms aus)
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1

# Blinksignal mit Freigabe I0 verbinden
GET I 0
GET T 0
AND
MOV O 0
```

Hinweis:

Wenn `CSET T 0 500` in der laufenden Logik immer wieder unter aktiver Bedingung ausgeführt wird, kann der Timer ständig neu getriggert werden. Dadurch entsteht kein sauberes Blinken.

---

### Schneller Blinker (5 Hz)

**Beschreibung**: Output blinkt schnell mit 5 Hz (100ms AN, 100ms AUS).
**Anwendung**: Alarmanzeige, Aufmerksamkeitssignal

```
# Schneller Blinker mit 5 Hz (100ms an/aus)

# Timer initialisieren
GETNOT M 0
DUP
CSET T 0 100
CMOV M 0 1

# Output folgt Timer
GET T 0
MOV O 0
```

---

### Timer stoppen

**Beschreibung**: Dieses Muster zeigt die korrekte Deaktivierung eines pulsierenden Timers mittels bedingtem Setzen auf 0.
**Anwendung**: Pulsierenden Timer deaktivieren

```
# Stoppe Timer 0 wenn I0 true
GET I 0
CSET T 0 0
```

---

### Einschaltverzögerung mit Timer

**Beschreibung**: Dieses Muster zeigt wie ein Ausgang mit einer Verzögerung gesetzt wird.
**Anwendung**: Ausgang 1 wird nach 5 Sekunden gesetzt, wenn Eingang 1 gesetzt ist.

```
# AUSGANG 1 WIRD NACH 5 SEKUNDEN GESETZT, WENN EINGANG 1 AKTIV IST
# TIMER 0  WIRD GESTARTET, WENN EINGANG 1 AKTIV IST

GETNOT I 1      # ALLES ZURUECKSETZEN, WENN EINGANG 1 INAKTIV IST
DUP
CMOV M 0 0      
CMOV M 1 0

GET I 1        
GETNOT M 0
AND
DUP
CMOV M 0 1      # MERKER 0 SETZEN
CSET T 0 5000   # TIMER 0 AUF 5 SEKUNDEN SETZEN

GET M 0
GETNOT T 0
AND
CMOV M 1 1      # NACH ABLAUF VON 5 SEKUNDEN WIRD MERKER 1 GESETZT

GET M 1
MOV O 1         # AUSGANG 1 NACH 5 SEKUNDEN SETZEN
```

---

### Ausschaltverzögerung mit Timer

**Beschreibung**: Dieses Muster zeigt wie ein Ausgang mit einer Verzögerung gesetzt wird.
**Anwendung**: Ausgang 1 wird nach 5 Sekunden gesetzt, wenn Eingang 1 gesetzt ist.

```
# AUSGANG 1 WIRD NACH 5 SEKUNDEN GESETZT, WENN EINGANG 1 AKTIV IST
# TIMER 0  WIRD GESTARTET, WENN EINGANG 1 AKTIV IST

GET I 1
CMOV M 0 1.    # SETZEN VON MERKER 

GETNOT M 0
GETNOT T 0
AND
CSET T0 0.      # TIMER WIEDER ZURUECKSETZEN

GETNOT I 1
GET M 0
AND
DUP
CMOV M 0 0
CSET T 0 5000.  # TIMER 0 MIT 5 SEKUNDEN SETZEN

GET I 1
GET T 0
OR
MOV O 1         # AUSGANG SETZEN MIT EINGANG 1 ODER TIMER 0
```

---

## Kategorie: Counter Operationen (Mittel)

---

### Counter inkrementieren bei Bedingung

**Beschreibung**: Dieses Muster demonstriert die ereignisbasierte Erhöhung eines Counters mittels bedingter Ausführung.
**Anwendung**: Ereigniszähler

```
# Wenn I0 true → Counter 0++
GET I 0
CINC C 0
```

---

### Vergleich eines Counters

**Beschreibung**: Dieses Pattern zeigt die Verwendung eines Vergleichsbefehls zur Zustandsprüfung eines Counters.
**Anwendung**: Schwellenwert prüfen

```
# Wenn Counter 0 == 5 → O2 = true
CMP C 0 5
MOV O 2
```

---

## Kategorie: Toggle-logic (Mittel)

---

### Toggle-Logik mit Memory

**Beschreibung**: Dieses Muster demonstriert eine einfache Zustandsumschaltung unter Verwendung eines Memory-Flags.
**Anwendung**: Zustandsumschaltung

```
# Toggle M0 wenn I0 true
GET I 0
GET M 0
NOT
CMOV M 0 1
```

---

### Sicherheitsabschaltung (Not-Aus Vorrang)

**Beschreibung**: Dieses Pattern implementiert eine einfache Fail-Safe-Logik, bei der ein Not-Aus-Signal Vorrang hat.
**Anwendung**: Fail-Safe-Logik

```
# O0 nur aktiv wenn I0 AND NOT I1
# I1 = Not-Aus
GET I 0
GETNOT I 1
AND
MOV O 0
```

---

## Kategorie: Pattern (Mittel)

---

### Anti-Pattern – Timer-Initialisierung ohne DUP (Stack-Fehler)

**Beschreibung**: Dieses Beispiel zeigt einen typischen Stack-Fehler bei einmaliger Timer-Initialisierung ohne Sicherung des Stack-Wertes.
**Anwendung**: Typischer Fehler bei einmaliger Timer-Initialisierung

```
# FALSCH – Stack-Wert wird durch CSET verbraucht
GETNOT M 0
CSET T 0 500
CMOV M 0 1
```

Problem:

CSET poppt den Stack-Wert.

Für CMOV ist kein Wert mehr vorhanden.

Initialisierungs-Flag wird nicht korrekt gesetzt.

Tags: anti-pattern, timer-init, stack-error

---

### Correct Pattern – Einmalige Timer-Initialisierung mit Flag

**Beschreibung**: Dieses Pattern zeigt die korrekte einmalige Initialisierung eines Timers unter Verwendung eines Memory-Flags und DUP zur Sicherung des Stack-Wertes.
**Anwendung**: Timer nur einmal starten und Initialisierung speichern

```
# KORREKT – Initialisiere Timer 0 einmalig mit 500 ms
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1
```

Erklärung des Stack-Flusses:

GETNOT M 0 prüft, ob noch nicht initialisiert

DUP erhält den Wert für zwei Operationen

CSET startet Timer (poppt einen Wert)

CMOV setzt Initialisierungs-Flag

Tags: pattern, timer-init, stack-dup, initialization

Verbindliche Timer-Definition (Ergänzung)

Timer sind pulsierend.

SET T n v oder CSET T n v bedeutet:

Der Timer ist v Millisekunden TRUE.

Danach v Millisekunden FALSE.

Die Gesamtperiode beträgt 2 * v Millisekunden.

Beispiel: 500 ms bedeutet 500 ms TRUE und 500 ms FALSE.

Diese Definition gilt immer und darf nicht anders interpretiert werden.

---
