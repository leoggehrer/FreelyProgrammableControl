# FPC – Vector Store Example Set (Kuratierte Pattern-Sammlung)

Diese Sammlung enthält grundlegende und fortgeschrittene Muster für die FreelyProgrammableControl (FPC) Stack-Maschine.
Jeder Abschnitt stellt ein in sich geschlossenes, verifiziertes Pattern dar.

---

### Direktes Durchschalten (Pass-Through)

**Kategorie**: Grundlagen
**Level**: Einfach
**Beschreibung**: Der einfachste Fall: Ein Eingang wird direkt auf einen Ausgang gelegt.
**Anwendung**: Signalweiterleitung, Test der Steuerung

```
# Pass-Through: Output 0 = Input 0
# Stack: [] -> [I0] -> []
GET I 0
MOV O 0
```

Testfälle:
- I0=F → O0=F
- I0=T → O0=T

Tags: pass-through, direkt, GET, MOV, einfach

---

### AND-Verknüpfung zweier Inputs

**Kategorie**: Grundlagen
**Level**: Einfach
**Beschreibung**: Grundlegende binäre UND-Verknüpfung. Output nur AN wenn BEIDE Inputs AN sind. Demonstriert den typischen Push-Push-Operate-Pop-Fluss.
**Anwendung**: Zwei-Hand-Bedienung, Freigabelogik

```
# AND-Verknüpfung: Output 0 = Input 0 AND Input 1
# Stack: [] -> [I0] -> [I0,I1] -> [I0&&I1] -> []
GET I 0
GET I 1
AND
MOV O 0
```

Testfälle:
- I0=F, I1=F → O0=F
- I0=T, I1=F → O0=F
- I0=F, I1=T → O0=F
- I0=T, I1=T → O0=T

Tags: AND, Logik, Grundlagen, Zwei-Input, Freigabe

---

### OR-Verknüpfung (Logisches ODER)

**Kategorie**: Grundlagen
**Level**: Einfach
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

Testfälle:
- I0=F, I1=F → O0=F
- I0=T, I1=F → O0=T
- I0=F, I1=T → O0=T
- I0=T, I1=T → O0=T

Tags: OR, Logik, Grundlagen, Parallelschaltung, Taster

---

### Negation eines Inputs

**Kategorie**: Grundlagen
**Level**: Einfach
**Beschreibung**: Invertierung eines Eingangssignals mit dem unären NOT-Operator.
**Anwendung**: Ruhekontakt, invertierte Logik

```
# Invertierung: Output 0 = NOT Input 0
# Stack: [] -> [I0] -> [!I0] -> []
GET I 0
NOT
MOV O 0
```

Testfälle:
- I0=F → O0=T
- I0=T → O0=F

Tags: NOT, Invertierung, Grundlagen, Ruhekontakt

---

### XOR-Verknüpfung (Exklusiv-ODER)

**Kategorie**: Grundlagen
**Level**: Einfach
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

Testfälle:
- I0=F, I1=F → O0=F
- I0=T, I1=F → O0=T
- I0=F, I1=T → O0=T
- I0=T, I1=T → O0=F

Tags: XOR, Wechselschaltung, Grundlagen, Exklusiv-ODER

---

### NAND-Verknüpfung (Negiertes UND)

**Kategorie**: Grundlagen
**Level**: Einfach
**Beschreibung**: Output ist AUS nur wenn beide Inputs AN sind. Invertiertes AND.
**Anwendung**: Basis für komplexe Logik, Überwachung

```
# NAND-Verknüpfung: Output 0 = NOT (Input 0 AND Input 1)
# Stack: [] -> [I0] -> [I0,I1] -> [I0&&I1] -> [!(I0&&I1)] -> []
GET I 0
GET I 1
AND
NOT
MOV O 0
```

Testfälle:
- I0=F, I1=F → O0=T
- I0=T, I1=F → O0=T
- I0=F, I1=T → O0=T
- I0=T, I1=T → O0=F

Tags: NAND, Logik, Grundlagen, invertiertes-AND

---

### NOR-Verknüpfung (Negiertes ODER)

**Kategorie**: Grundlagen
**Level**: Einfach
**Beschreibung**: Output ist AN nur wenn beide Inputs AUS sind. Invertiertes OR.
**Anwendung**: Ruhe-/Störungszustand, Alles-OK-Signal

```
# NOR-Verknüpfung: Output 0 = NOT (Input 0 OR Input 1)
# Stack: [] -> [I0] -> [I0,I1] -> [I0||I1] -> [!(I0||I1)] -> []
GET I 0
GET I 1
OR
NOT
MOV O 0
```

Testfälle:
- I0=F, I1=F → O0=T
- I0=T, I1=F → O0=F
- I0=F, I1=T → O0=F
- I0=T, I1=T → O0=F

Tags: NOR, Logik, Grundlagen, Ruhezustand, invertiertes-OR

---

### Drei-Input AND-Verknüpfung

**Kategorie**: Grundlagen
**Level**: Einfach
**Beschreibung**: Output nur AN wenn alle drei Inputs AN sind. Zeigt Verkettung von AND.
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

Testfälle (Auswahl):
- I0=F, I1=F, I2=F → O0=F
- I0=T, I1=T, I2=F → O0=F
- I0=T, I1=T, I2=T → O0=T

Tags: AND, Drei-Input, Mehrfachbedingung, Verkettung

---

### Mehrere Outputs aus einer Logik

**Kategorie**: Grundlagen
**Level**: Einfach
**Beschreibung**: Ein Ergebnis wird mit DUP dupliziert und auf mehrere Outputs geschrieben.
**Anwendung**: Parallele Anzeigen, Signalverteilung

```
# AND-Ergebnis auf O0 und O1 gleichzeitig
# Stack: [] -> [I0] -> [I0,I1] -> [result] -> [result,result] -> [result] -> []
GET I 0
GET I 1
AND
DUP
MOV O 0
MOV O 1
```

Testfälle:
- I0=T, I1=T → O0=T, O1=T
- I0=T, I1=F → O0=F, O1=F

Tags: DUP, mehrere-outputs, signalverteilung, parallel

---

### Bedingtes Setzen eines Outputs (CMOV)

**Kategorie**: Grundlagen
**Level**: Einfach
**Beschreibung**: CMOV poppt den Stack und setzt den Output NUR wenn der Wert true ist. Der Output behält seinen Wert wenn die Bedingung false ist.
**Anwendung**: Einmaliges Setzen, bedingte Steuerung

```
# Wenn I0 true → O1 wird auf 1 gesetzt (und bleibt gesetzt!)
GET I 0
CMOV O 1 1
```

WICHTIG: CMOV setzt den Wert nur bei true. Bei false passiert nichts — der alte Wert bleibt!

Tags: CMOV, bedingt, Grundlagen, einmaliges-setzen

---

### Bedingtes Setzen eines Timers (CSET)

**Kategorie**: Grundlagen
**Level**: Einfach
**Beschreibung**: CSET poppt den Stack und startet den Timer NUR wenn der Wert true ist.
**Anwendung**: Bedingte Timer-Steuerung

```
# Wenn I0 true → Timer 0 mit 500ms starten
GET I 0
CSET T 0 500
```

WARNUNG: Wenn CSET in jedem Zyklus mit true aufgerufen wird, wird der Timer ständig neu gestartet! Siehe Anti-Pattern weiter unten.

Tags: CSET, Timer, bedingt, Grundlagen

---

### Anti-Pattern: Timer mit SET in der Hauptschleife (FALSCH!)

**Kategorie**: Timer und Blinker
**Level**: Mittel
**Beschreibung**: DIESES PATTERN IST FALSCH! SET T im Hauptprogramm wird in JEDEM Zyklus ausgeführt und startet den Timer ständig neu. Der Timer kommt nie in die FALSE-Phase.
**Anwendung**: So NICHT machen! Zeigt den häufigsten Timer-Fehler.

```
# FALSCH! Timer wird in jedem Zyklus neu gestartet!
# Der Timer ist IMMER im true-Zustand weil er ständig neu beginnt!
SET T 0 500
GET T 0
MOV O 0
```

Problem: SET T 0 500 wird bei JEDEM Programmzyklus (alle 50ms) ausgeführt. Der Timer startet jedes Mal neu und kommt nie in die false-Phase. Output 0 ist dadurch IMMER true — kein Blinken!

LÖSUNG: Timer einmalig initialisieren mit Memory-Flag (siehe "Einfacher Blinker").

Tags: anti-pattern, timer, SET, kein-blinken, immer-true

---

### Anti-Pattern: Timer-Initialisierung ohne DUP (Stack-Fehler)

**Kategorie**: Timer und Blinker
**Level**: Mittel
**Beschreibung**: Typischer Stack-Fehler bei einmaliger Timer-Initialisierung ohne Sicherung des Stack-Wertes mit DUP.
**Anwendung**: So NICHT machen! Stack-Unterlauf-Fehler.

```
# FALSCH – Stack-Wert wird durch CSET verbraucht
GETNOT M 0
CSET T 0 500
CMOV M 0 1
```

Problem: CSET poppt den Stack-Wert. Für CMOV ist kein Wert mehr vorhanden → Stack-Unterlauf! Das Initialisierungs-Flag M0 wird nicht gesetzt, Timer wird in jedem Zyklus neu gestartet.

LÖSUNG: DUP vor CSET verwenden (siehe "Korrekte Timer-Initialisierung").

Tags: anti-pattern, timer-init, stack-error, DUP-vergessen

---

### Korrekte Timer-Initialisierung mit Flag (VERBINDLICHES MUSTER)

**Kategorie**: Timer und Blinker
**Level**: Mittel
**Beschreibung**: Das VERBINDLICHE Muster für einmalige Timer-Initialisierung. Memory-Flag verhindert wiederholte Ausführung, DUP sichert den Stack-Wert für zwei konsumierende Befehle.
**Anwendung**: Timer nur einmal starten, Initialisierungs-Pattern

```
# KORREKT – Initialisiere Timer 0 einmalig mit 500 ms
# Stack: [] -> [!M0] -> [!M0,!M0] -> [!M0] -> []
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1
```

Stack-Fluss:
1. GETNOT M 0: Prüft ob noch nicht initialisiert → [true] (beim ersten Mal)
2. DUP: Dupliziert für zwei konsumierende Befehle → [true, true]
3. CSET T 0 500: Poppt true, startet Timer → [true]
4. CMOV M 0 1: Poppt true, setzt Flag M0=1 → []

Ab dem zweiten Zyklus: M0=true → GETNOT M 0 = false → CSET und CMOV tun nichts.

DIESES MUSTER IMMER VERWENDEN wenn ein Timer initialisiert werden soll!

Tags: pattern, timer-init, stack-dup, initialization, verbindlich

---

### Einfacher Blinker (1 Hz)

**Kategorie**: Timer und Blinker
**Level**: Mittel
**Beschreibung**: Output blinkt kontinuierlich mit 1 Hz (500ms AN, 500ms AUS). Verwendet das verbindliche Timer-Init-Pattern mit Memory-Flag.
**Anwendung**: Statusanzeige, Warnblinker, LED-Blinker

```
# Blinker mit 1 Hz (500ms an/aus)
# Timer einmalig initialisieren
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1
# Output folgt Timer-Zustand
GET T 0
MOV O 0
```

Funktionsweise:
- Zyklus 1: M0=false → Timer wird gestartet, M0 wird gesetzt
- Zyklus 2+: M0=true → Timer-Init wird übersprungen
- Timer pulsiert: 500ms true, 500ms false → O0 blinkt

Tags: blinker, timer, 1Hz, statusanzeige

---

### Eingangsgesteuerter Blinker (Blinken nur bei aktivem Input)

**Kategorie**: Timer und Blinker
**Level**: Mittel
**Beschreibung**: Output blinkt nur solange Input aktiv ist. Timer läuft permanent, wird aber nur bei I0=true auf den Output durchgeschaltet (Gating-Pattern).
**Anwendung**: Aktivierungsabhängige Blinkanzeige, Warnblinker mit Freigabe

```
# Output 0 blinkt mit 1 Hz, nur wenn Input 0 = true
# Timer einmalig initialisieren (läuft immer!)
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1
# Blinksignal mit Freigabe I0 verbinden (Gating)
GET I 0
GET T 0
AND
MOV O 0
```

WICHTIG: Timer-Init und Gating sind GETRENNT!
- Timer wird einmalig gestartet und läuft dauerhaft (auch wenn I0=false)
- Output = I0 AND T0 → blinkt nur wenn I0=true
- KEIN CSET im I0-Pfad! Das würde den Timer ständig neu triggern.

Tags: blinker, gating, eingangsgesteuert, timer, AND

---

### Schneller Blinker (5 Hz)

**Kategorie**: Timer und Blinker
**Level**: Mittel
**Beschreibung**: Output blinkt schnell mit 5 Hz (100ms AN, 100ms AUS).
**Anwendung**: Alarmanzeige, Aufmerksamkeitssignal, Störungsmeldung

```
# Schneller Blinker mit 5 Hz (100ms an/aus)
GETNOT M 0
DUP
CSET T 0 100
CMOV M 0 1
GET T 0
MOV O 0
```

Tags: blinker, timer, 5Hz, alarm, schnell

---

### Zwei unabhängige Blinker mit verschiedenen Frequenzen

**Kategorie**: Timer und Blinker
**Level**: Mittel
**Beschreibung**: Zwei Outputs blinken unabhängig mit verschiedenen Frequenzen. Jeder Timer und jedes Memory-Flag muss separat sein.
**Anwendung**: Mehrkanal-Statusanzeige, Ampelsteuerung-Basis

```
# Blinker 1: O0 mit 1 Hz (Timer 0, Memory 0)
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1
GET T 0
MOV O 0
# Blinker 2: O1 mit 2 Hz (Timer 1, Memory 1)
GETNOT M 1
DUP
CSET T 1 250
CMOV M 1 1
GET T 1
MOV O 1
```

WICHTIG: Verschiedene Timer-Indizes (T0, T1) und verschiedene Memory-Flags (M0, M1) verwenden!

Tags: blinker, multi-timer, zwei-frequenzen, unabhaengig

---

### Timer stoppen

**Kategorie**: Timer und Blinker
**Level**: Einfach
**Beschreibung**: Korrekte Deaktivierung eines pulsierenden Timers mittels CSET auf 0.
**Anwendung**: Timer abschalten, Blinker stoppen

```
# Stoppe Timer 0 wenn I0 true
GET I 0
CSET T 0 0
```

SET T n 0 oder CSET T n 0 stoppt den Timer sofort.

Tags: timer, stoppen, deaktivieren, CSET

---

### Counter inkrementieren bei Bedingung

**Kategorie**: Counter Operationen
**Level**: Mittel
**Beschreibung**: Counter wird bei jedem Zyklus erhöht solange die Bedingung true ist.
**Anwendung**: Zyklenzähler, Laufzeitmessung

```
# Counter 0 wird erhöht solange I0 true ist
# ACHTUNG: Wird bei JEDEM Zyklus (50ms) erhöht!
GET I 0
CINC C 0
```

WARNUNG: CINC wird bei jedem Programmzyklus ausgeführt solange I0=true! Bei 50ms Zykluszeit sind das 20 Inkremente pro Sekunde. Für flankengesteuertes Zählen siehe "Flankengesteuerter Zähler".

Tags: counter, CINC, zaehler, zyklisch

---

### Vergleich eines Counters

**Kategorie**: Counter Operationen
**Level**: Mittel
**Beschreibung**: Counter-Wert wird mit einem festen Wert verglichen. Ergebnis liegt als boolean auf dem Stack.
**Anwendung**: Schwellenwert prüfen, Zählerauswertung

```
# Wenn Counter 0 == 5 → O2 = true
CMP C 0 5
MOV O 2
```

Vergleichsoperatoren:
- CMP C n v → true wenn C[n] == v (exakt gleich)
- GT C n v → true wenn C[n] > v (strikt größer)
- LE C n v → true wenn C[n] < v (strikt kleiner)

Tags: counter, CMP, vergleich, schwellenwert

---

### Flankengesteuerter Zähler mit Schwellenwert

**Kategorie**: Counter Operationen
**Level**: Komplex
**Beschreibung**: Zählt steigende Flanken von I0 (nicht jeden Zyklus!). Wenn Schwellenwert erreicht, wird O0 gesetzt. I1 setzt alles zurück.
**Anwendung**: Stückzähler, Ereigniszähler, Batch-Counter

```
# Flankengesteuerter Zähler: Zählt Flanken von I0, O0 bei C0 >= 5
# M0 = Flanken-Merker (war I0 im letzten Zyklus true?)
# I1 = Reset
# Flanke erkennen: I0=true UND M0=false
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
# Schwellenwert prüfen: O0 = (C0 >= 5) → GT C 0 4
GT C 0 4
MOV O 0
```

Tags: counter, flanke, zaehler, schwellenwert, reset, flankenerkennung

---

### Counter dekrementieren (Countdown)

**Kategorie**: Counter Operationen
**Level**: Mittel
**Beschreibung**: Counter wird heruntergezählt. Zeigt CDEC und LE Verwendung.
**Anwendung**: Countdown, Restmengen-Anzeige

```
# Counter 0 runterzählen wenn I0 true, O0 wenn Counter < 1 (also 0)
GET I 0
CDEC C 0
LE C 0 1
MOV O 0
```

Tags: counter, CDEC, countdown, LE

---

### Toggle-Logik mit Memory (zyklisch)

**Kategorie**: Toggle-Logik
**Level**: Mittel
**Beschreibung**: Memory-Wert wird in jedem Zyklus umgeschaltet solange I0 true ist. ACHTUNG: Toggelt bei JEDEM Zyklus, nicht pro Tastendruck!
**Anwendung**: Zustandsumschaltung (nur mit Flanke sinnvoll)

```
# Toggle M0 wenn I0 true (ACHTUNG: toggelt jeden Zyklus!)
GET I 0
GET M 0
NOT
CMOV M 0 1
```

Für einmaliges Toggeln pro Tastendruck ist eine Flankenerkennung erforderlich.

Tags: toggle, memory, zustandsumschaltung, zyklisch

---

### Sicherheitsabschaltung (Not-Aus Vorrang)

**Kategorie**: Toggle-Logik
**Level**: Mittel
**Beschreibung**: Output aktiv wenn I0 gesetzt, aber I1 (Not-Aus) hat immer Vorrang und schaltet ab.
**Anwendung**: Fail-Safe-Logik, Sicherheitskreis, Maschinensteuerung

```
# O0 nur aktiv wenn I0 AND NOT I1
# I1 = Not-Aus (Öffner: true = Not-Aus aktiv = STOP)
GET I 0
GETNOT I 1
AND
MOV O 0
```

Testfälle:
- I0=F, I1=F → O0=F (Maschine aus)
- I0=T, I1=F → O0=T (Maschine läuft)
- I0=T, I1=T → O0=F (Not-Aus! Vorrang!)
- I0=F, I1=T → O0=F (Not-Aus aktiv)

Tags: sicherheit, not-aus, fail-safe, vorrang, GETNOT

---

### SR-Latch (Setzen/Rücksetzen mit Vorrang)

**Kategorie**: Toggle-Logik
**Level**: Mittel
**Beschreibung**: Set-Reset Flip-Flop. I0 setzt M0 (und damit O0), I1 setzt zurück. Rücksetzen hat Vorrang.
**Anwendung**: Speichernde Logik, Selbsthaltung, Verriegelung

```
# SR-Latch: I0=Set, I1=Reset (Reset hat Vorrang)
# Setzen
GET I 0
CMOV M 0 1
# Rücksetzen (hat Vorrang, kommt NACH Setzen)
GET I 1
CMOV M 0 0
# Output folgt Memory
GET M 0
MOV O 0
```

Testfälle:
- I0=F, I1=F → O0=letzter Wert (gespeichert!)
- I0=T, I1=F → O0=T (gesetzt)
- I0=F, I1=T → O0=F (zurückgesetzt)
- I0=T, I1=T → O0=F (Reset hat Vorrang)

Tags: SR-latch, flip-flop, selbsthaltung, speichernd, set-reset

---

### Selbsthaltung mit Start/Stop

**Kategorie**: Toggle-Logik
**Level**: Mittel
**Beschreibung**: Klassische Selbsthalteschaltung: I0=Start, I1=Stop. Einmal gestartet bleibt O0 aktiv bis Stop gedrückt wird.
**Anwendung**: Motorsteuerung, Maschinenanlauf, Start/Stop-Logik

```
# Selbsthaltung: I0=Start, I1=Stop
# Start setzt Merker
GET I 0
CMOV M 0 1
# Stop löscht Merker (hat Vorrang)
GET I 1
CMOV M 0 0
# Ausgang folgt Merker
GET M 0
MOV O 0
```

Tags: selbsthaltung, start-stop, motor, verriegelung

---

### AND-Gate mit Blinker-Output

**Kategorie**: Kombinierte Muster
**Level**: Komplex
**Beschreibung**: Output blinkt nur wenn beide Inputs gesetzt sind. Kombination aus Logik und Timer-Gating.
**Anwendung**: Warnblinker bei Doppelbedingung, bedingte Statusanzeige

```
# O0 blinkt mit 1Hz nur wenn I0 UND I1 beide true
# Timer einmalig initialisieren
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1
# Logik: I0 AND I1 AND T0
GET I 0
GET I 1
AND
GET T 0
AND
MOV O 0
```

Tags: and, blinker, gating, kombiniert, komplex

---

### Zeitgesteuerter Output – Abschaltung nach N Timer-Perioden

**Kategorie**: Kombinierte Muster
**Level**: Komplex
**Beschreibung**: Output ist AN sobald das Programm startet und schaltet sich automatisch nach einer definierten Anzahl vollständiger Timer-Perioden ab. Ein Perioden-Zähler erfasst je eine steigende und eine fallende Timer-Flanke (M2/M3). Erst wenn beide Flanken einer Periode gesehen wurden, wird der Counter inkrementiert und die Flanken-Merker zurückgesetzt. Sobald der Counter den Schwellenwert erreicht, geht der Output AUS.
**Anwendung**: Zeitbegrenzte Aktivierung, Ablauf-Steuerung, Einschalt-Verzögerungsabschaltung

```
# Zeitgesteuerter Output: O0 aktiv für 10 Timer-Perioden (20 Sekunden)
# Timer 0: 1000 ms → Periode = 2 s → 10 Perioden = 20 s
# M0 = Initialisierungs-Flag, M2 = T0-AN-Flanke, M3 = T0-AUS-Flanke, C0 = Periodenzähler

# --- Block 1: Timer einmalig initialisieren (verbindliches Muster) ---
# Stack: [] -> [!M0] -> [!M0,!M0] -> [!M0] -> []
GETNOT M 0
DUP
CSET T 0 1000
CMOV M 0 1

# --- Block 2: Flanken des Timers erfassen ---
# Stack: [] -> [T0] -> []  dann  [] -> [!T0] -> []
GET T 0
CMOV M 2 1        # T0 war AN → M2 merken
GETNOT T 0
CMOV M 3 1        # T0 war AUS → M3 merken

# --- Block 3: Vollständige Periode zählen, Flanken-Merker zurücksetzen ---
# Stack: [] -> [M2] -> [M2,M3] -> [M2&&M3] -> [M2&&M3,M2&&M3]
#         -> [M2&&M3] -> [M2&&M3,M2&&M3] -> [M2&&M3] -> []
GET M 2
GET M 3
AND
DUP
CINC C 0          # Periode vollständig → Counter erhöhen
DUP
CMOV M 2 0        # M2 zurücksetzen
CMOV M 3 0        # M3 zurücksetzen

# --- Block 4: Output aktiv solange C0 < 10 Perioden ---
# Stack: [] -> [M0] -> [M0,C0<10] -> [M0&&(C0<10)] -> []
GET M 0
LE C 0 10
AND
MOV O 0
```

Stack-Fluss Block 3 (kritischer Pfad, da 3× konsumierend mit DUP-Sicherung):
1. `GET M 2`: → `[M2]`
2. `GET M 3`: → `[M2, M3]`
3. `AND`: → `[M2&&M3]`
4. `DUP`: → `[M2&&M3, M2&&M3]`
5. `CINC C 0`: konsumiert Top → `[M2&&M3]`
6. `DUP`: → `[M2&&M3, M2&&M3]`
7. `CMOV M 2 0`: konsumiert Top → `[M2&&M3]`
8. `CMOV M 3 0`: konsumiert Top → `[]`

Funktionsweise (Zeitablauf):
- Start: M0=0, M2=0, M3=0, C0=0
- Zyklus 1: Timer initialisiert (M0=1), O0=T (C0=0 < 10)
- T0 geht AN: M2=1 gesetzt
- T0 geht AUS (nach 1000 ms): M3=1 gesetzt → M2&&M3=T → C0=1, M2=0, M3=0
- Nach jeder vollständigen Periode (2000 ms) steigt C0 um 1
- Nach 10 Perioden (20 s): C0=10, LE C 0 10 = false → O0=F (bleibt AUS)

Testfälle:
- Nach 0 Perioden (t=0 s): C0=0 → O0=T
- Nach 5 Perioden (t=10 s): C0=5 → O0=T
- Nach 10 Perioden (t=20 s): C0=10 → O0=F
- C0=9, M2=1, M3=0: O0=T (Periode noch nicht komplett)

WICHTIG: Anzahl Perioden und Timer-Intervall sind unabhängig einstellbar:
- Kürzere Perioden: `CSET T 0 500` → Periode = 1 s → Schwellenwert 10 = 10 s Gesamtlaufzeit
- Mehr Perioden: `LE C 0 30` → 30 Perioden Gesamtlaufzeit

Tags: timer, periodenzähler, zeitbegrenzt, abschaltung, flanke, M2-M3-muster, CINC, LE, komplex

---

### FPC Stack-Disziplin (Regeln)

**Kategorie**: Referenz
**Level**: Einfach
**Beschreibung**: Verbindliche Regeln für korrekten FPC-Code. Stack muss am Programmende leer sein.
**Anwendung**: Code-Validierung, Fehlerprüfung

Stack-Änderungen pro Befehl:
- Push (+1): GET, GETNOT, CMP, GT, LE, DUP
- Neutral (0): NOT, SET, CINC, CDEC
- Pop (-1): AND, OR, XOR, MOV, CMOV, CSET

Regeln:
1. Vor Ausgabe: Stack-Simulation über jede Zeile
2. Stack-Tiefe darf nie negativ werden
3. Für jeden konsumierenden Befehl müssen genug Werte vorhanden sein
4. Stack am Programmende muss leer sein (depth = 0)
5. Wenn zwei konsumierende Befehle hintereinander: DUP verwenden!

Tags: referenz, regeln, stack, validierung

---

### FPC Timer-Semantik (Referenz)

**Kategorie**: Referenz
**Level**: Einfach
**Beschreibung**: Verbindliche Definition des Timer-Verhaltens in FPC.
**Anwendung**: Timer-Programmierung, Fehlerverständnis

Timer sind PULSIEREND:
- SET T n v / CSET T n v startet pulsierenden Timer
- v Millisekunden TRUE, dann v Millisekunden FALSE (Periode = 2*v ms)
- Beispiel: SET T 0 500 → 500ms true, 500ms false, 500ms true, ...
- SET T n 0 / CSET T n 0 stoppt den Timer

VERBINDLICH: Timer IMMER einmalig initialisieren:
```
GETNOT M n
DUP
CSET T n v
CMOV M n 1
```

NIE SET T oder CSET T in der Hauptschleife ohne Memory-Flag-Schutz!

Tags: referenz, timer, pulsierend, semantik, verbindlich
