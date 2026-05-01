# FPC Sprachreferenz

## Befehlstabelle

| Kategorie | Befehl | Operanden | Stack-Effekt | Beschreibung |
|-----------|--------|-----------|--------------|--------------|
| **Konstanten** | `GET 0` | — | Push(false) | Konstante false auf Stack |
| | `GET 1` | — | Push(true) | Konstante true auf Stack |
| **Lesen** | `GET I n` | n=Index | Push(I[n]) | Input n lesen |
| | `GET O n` | n=Index | Push(O[n]) | Output n lesen |
| | `GET M n` | n=Index | Push(M[n]) | Memory n lesen |
| | `GET T n` | n=Index | Push(T[n]) | Timer n lesen |
| **Negiert lesen** | `GETNOT I n` | n=Index | Push(!I[n]) | Input n negiert lesen |
| | `GETNOT O n` | n=Index | Push(!O[n]) | Output n negiert |
| | `GETNOT M n` | n=Index | Push(!M[n]) | Memory n negiert |
| | `GETNOT T n` | n=Index | Push(!T[n]) | Timer n negiert |
| **Stack** | `DUP` | — | Push(Top) | Oberstes Element duplizieren |
| | `DUP n` | n=Anzahl | Push(Top)×n | n-fach duplizieren |
| | `NOP` | — | — | Keine Operation |
| **Logik** | `NOT` | — | A → !A | Negation |
| | `AND` | — | A,B → A&&B | Logisches UND (2 Pop, 1 Push) |
| | `OR` | — | A,B → A\|\|B | Logisches ODER (2 Pop, 1 Push) |
| | `XOR` | — | A,B → A^B | Exklusiv-ODER (2 Pop, 1 Push) |
| **Schreiben** | `MOV O n` | n=Index | Pop→O[n] | Wert in Output schreiben |
| | `MOV M n` | n=Index | Pop→M[n] | Wert in Memory schreiben |
| **Bedingt schreiben** | `CMOV O n v` | n=Index, v=0/1 | Pop, wenn true: O[n]=v | Bedingtes Output-Setzen |
| | `CMOV M n v` | n=Index, v=0/1 | Pop, wenn true: M[n]=v | Bedingtes Memory-Setzen |
| **Timer/Counter** | `SET T n v` | n=Index, v=ms | — | Timer starten (unkonditioniert) |
| | `SET C n v` | n=Index, v=Wert | — | Counter setzen (unkonditioniert) |
| | `CSET T n v` | n=Index, v=ms | Pop, wenn true: T[n]=v | Timer bedingt starten |
| | `CSET C n v` | n=Index, v=Wert | Pop, wenn true: C[n]=v | Counter bedingt setzen |
| **Counter Ops** | `CINC C n` | n=Index | Pop, wenn true: C[n]++ | Inkrementieren |
| | `CDEC C n` | n=Index | Pop, wenn true: C[n]-- | Dekrementieren |
| **Vergleiche** | `CMP C n v` | n=Index, v=Wert | Push(C[n]==v) | Gleich-Vergleich |
| | `GT C n v` | n=Index, v=Wert | Push(C[n]>v) | Größer-als-Vergleich |
| | `LE C n v` | n=Index, v=Wert | Push(C[n]<v) | Kleiner-als-Vergleich |
| **Kommentar** | `# text` | — | — | Kommentar (MUSS eigene Zeile sein!) |

---

## Stack-Regeln (KRITISCH)

### Befehlsklassen nach Stack-Effekt

| Klasse | Befehle | Effekt |
|--------|---------|--------|
| Push (+1) | `GET`, `GETNOT`, `CMP`, `GT`, `LE`, `DUP` | Legt Wert auf Stack |
| Neutral (0) | `NOT`, `SET`, `NOP` | Kein Netto-Effekt |
| Pop (−1) | `AND`, `OR`, `XOR`, `MOV`, `CMOV`, `CSET`, `CINC`, `CDEC` | Verbraucht Wert vom Stack |

**WICHTIG: `CINC` und `CDEC` sind POP-Befehle!** Sie erwarten einen Boolean auf dem Stack und inkrementieren/dekrementieren nur wenn `true`. Vor jedem `CINC`/`CDEC` muss ein Wert auf dem Stack liegen (z.B. via `GET I n`, `CMP`, Flankenbedingung). Ohne Stack-Wert → "Stack is empty!"

### Checkliste vor jedem Befehl

1. Wie viele Werte braucht der Befehl?
2. Wie viele Werte sind auf dem Stack?
3. Wenn zu wenige → `GET` oder `DUP` einfügen!
4. Wenn ein Wert für zwei Befehle gebraucht wird → `DUP` verwenden!
5. Stack am Programmende muss leer sein (Tiefe = 0)

### Häufige Fehler

```
# FALSCH: AND ohne 2 Werte
GET I 0
AND             # Stack-Unterlauf!

# RICHTIG:
GET I 0
GET I 1
AND

# FALSCH: MOV ohne Wert
MOV O 0         # Stack leer!

# FALSCH: Timer-Init ohne DUP
GETNOT M 0
CSET T 0 500    # verbraucht Wert
CMOV M 0 1      # Stack leer!

# RICHTIG:
GETNOT M 0
DUP
CSET T 0 500
CMOV M 0 1
```

---

## Timer-Semantik (KRITISCH)

### Wie Timer funktionieren

Timer in FPC sind **pulsierend** (nicht einmalig!):
- `SET T n v` / `CSET T n v` → startet pulsierenden Timer
- **v ms true** → **v ms false** → **v ms true** → ... (Endlos)
- `SET T n 0` / `CSET T n 0` → Timer stoppen
- `GET T n` → aktueller Zustand (true/false)

### VERBINDLICHES Timer-Init-Muster

```
# Timer einmalig starten mit Memory-Flag n
GETNOT M n    # true beim ersten Zyklus (M[n]=false)
DUP           # für zwei konsumierende Befehle
CSET T n v    # Timer starten
CMOV M n 1    # Flag setzen → verhindert Neustart
```

Danach: `GET T n` → pulsiert mit v ms.

### Warum KEIN SET T in der Hauptschleife?

```
# FALSCH – Timer wird jeden Zyklus (z.B. alle 50ms) neu gestartet!
# Timer kommt nie in die false-Phase → Output IMMER true
SET T 0 500
GET T 0
MOV O 0
```

---

## Zyklische Ausführung

- Programme laufen in einer **Endlosschleife**
- Nach letzter Zeile → sofort wieder bei Zeile 1
- Standard-Zykluszeit: 100ms (konfigurierbar)
- Jede Zeile wird **jeden Zyklus** ausgeführt
- Konsequenz: Initialisierungen mit Memory-Flag schützen!

---

## Kommentar-Regeln

```
# Korrekt: Kommentar in eigener Zeile
GET I 0

GET I 0  # FALSCH: Inline-Kommentar → Parse-Fehler!
```

---

## Bereichsprüfung mit Counter

```
# Bereich [x+1 .. y]: GT C 0 x → LE C 0 y → AND
GT C 0 10
LE C 0 20
AND
MOV O 0    # Output wenn 11 <= C0 <= 20
```
