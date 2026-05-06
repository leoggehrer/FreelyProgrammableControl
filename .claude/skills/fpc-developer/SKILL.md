---
name: fpc-developer
description: >
  Skill für die Entwicklung von FPC-Programmen (Freely Programmable Control).
  Verwende diesen Skill immer dann, wenn der Benutzer ein FPC-Programm entwickeln,
  ändern, debuggen, testen oder auf der Steuerung laden/ausführen möchte – auch wenn
  er nur "Programm schreiben", "Steuerung programmieren", "Ampelsteuerung", "Blinker",
  "Schritt-für-Schritt", "was macht mein Programm" oder ähnliche Formulierungen
  verwendet. Auch bei Fragen zu Timern, Countern, Stack-Fehlern, Memory-Flags oder
  I/O-Zuständen diesen Skill verwenden.
---

# FPC-Programmentwicklung

FPC-Programme sind stackbasierte, zyklisch ausgeführte Boole'sche Steuerprogramme.
Jedes Programm läuft in einer **Endlosschleife** auf der Desktop-App.

---

## Entwicklungsworkflow

Führe diese Schritte der Reihe nach aus:

### 1. Anforderungen klären

Bevor du Code schreibst, kläre:
- Welche Eingänge (I) und Ausgänge (O) werden benötigt?
- Werden Timer, Counter oder Memory-Merker gebraucht?
- Was soll das Programm bei welcher Bedingung tun?

### 2. MCP-Server-Status prüfen

Prüfe zuerst, ob die Desktop-App erreichbar ist:

```
Tool: check_desktop_status
```

Falls nicht erreichbar: Desktop-App starten (läuft auf Port 5555).
Falls erreichbar: weiter mit Schritt 3.

### 3. Befehlsreferenz holen (bei Bedarf)

**Token-optimiert: Referenzen NIEMALS vollständig lesen! Immer zuerst greppen:**

```bash
# Schritt 1: Relevante Zeilen finden
grep -n "Timer\|Blinker\|Ampel\|Counter\|SR-Latch" \
  .claude/skills/fpc-developer/references/fpc-patterns.md | head -30

# Schritt 2: Nur den gefundenen Abschnitt lesen (offset + limit)
# Beispiel: Zeile 58 gefunden → Read offset=55 limit=50
```

Vollständige Dateien **nur lesen wenn grep nicht reicht:**
- `references/fpc-language.md` — Befehlstabelle + Stack-Regeln (154 Zeilen, ~1.200 Tokens)
- `references/fpc-patterns.md` — Pattern-Bibliothek (336 Zeilen, ~2.600 Tokens)
- `/FPCSamples.md` — Vollsammlung; **zuerst grep**, dann Abschnitt lesen

### 4. Programm schreiben

Schreibe das FPC-Programm. Kritische Regeln (alles Wichtige steht bereits hier — Referenzen nur bei Unsicherheit laden):

**Stack-Disziplin** – vor jedem Befehl prüfen:
- Push (+1): `GET`, `GETNOT`, `CMP`, `GT`, `LE`, `DUP`
- Pop (−1): `AND`, `OR`, `XOR`, `MOV`, `CMOV`, `CSET`, `CINC`, `CDEC`
- Stack muss am Programmende leer sein (Tiefe = 0)
- Wenn ein Wert zwei Befehlen zugeführt werden muss → `DUP` verwenden!

**Timer-Initialisierung** – VERBINDLICHES Muster:
```
GETNOT M n   # true wenn noch nicht initialisiert
DUP          # für zwei konsumierende Befehle
CSET T n v   # Timer nur einmal starten
CMOV M n 1   # Initialisierungsflag setzen
```
NIEMALS `SET T` oder `CSET T` ohne Memory-Flag-Schutz in der Hauptschleife!

**Counter-Phasensteuerung** – Bewährtes Muster (Ampel, Ablaufsteuerung):
```
# Init Takt-Timer T0 (100ms) + Counter C0 auf 1
GETNOT M 0
DUP
CSET T 0 100
CMOV M 0 1
GETNOT M 1
DUP
CSET C 0 1
CMOV M 1 1
# Tick
GET T 0
CINC C 0
# Reset nach N Ticks
GT C 0 N
CSET C 0 1
# Bereich [x+1..y]: GT C 0 x → LE C 0 y → AND → MOV M phase
```
LE C 0 y bedeutet C0 ≤ y (inklusiv). GT C 0 x bedeutet C0 > x.

**Keine Inline-Kommentare** – Kommentare immer in eigener Zeile mit `#`.

### 5. Validieren und laden

```
Tool: parse_fpc_program    # Syntax-Check
Tool: load_program_to_desktop  # Laden (stoppt Ausführung automatisch)
```

Alternativ direkt per HTTP:
```
POST http://localhost:5555/api/program
Content-Type: text/plain
<programm-code als plain text>
```
ACHTUNG: Der Endpunkt erwartet den FPC-Quellcode als **reinen Text** im Body — KEIN JSON-Wrapper wie `{"source": "..."}`. JSON führt zu "Zeile 0: Unknown instruction".

Danach Parse-Fehler prüfen:
```
GET http://localhost:5555/api/status
```

### 6. Testen und debuggen

**Normaler Test:**
```
Tool: start_execution          # Ausführung starten
Tool: get_input_states         # Eingänge anzeigen
Tool: toggle_input index=0     # Eingang simulieren
Tool: get_output_states        # Ausgänge prüfen
Tool: get_timers               # Timer-Zustand
Tool: get_counters             # Counter-Werte
```

**Token-optimiertes Monitoring** – NIEMALS enge Polling-Schleifen! Stattdessen:
```bash
# Einmalige Prüfung nach bekannter Wartezeit
sleep 22 && curl -s http://localhost:5555/api/outputs | python3 -c \
  "import sys,json; [print(f'O{o[\"index\"]}={o[\"value\"]}') \
   for o in json.load(sys.stdin)['outputs'][:4]]"

# Oder: Counter + Outputs in einem Call
curl -s http://localhost:5555/api/counters && \
curl -s http://localhost:5555/api/outputs
```
Faustregel: **max. 3 Monitoring-Calls pro Testphase**. Bei zeitgesteuerten Programmen
Wartezeit aus der Phasenlänge ableiten, nicht blind pollen.

**Debug-Modus (Schritt für Schritt):**
```
Tool: set_debug_mode enable=true
Tool: step_program             # einen Schritt ausführen
Tool: debug_snapshot           # vollständiger Zustand (Stack, I/O, Zeile, Fehler)
Tool: debug_step_and_inspect   # Schritt + Snapshot atomar
```

**Ausführung stoppen:**
```
Tool: stop_execution
```

---

## Verfügbare MCP-Tools (Übersicht)

| Tool | Zweck |
|------|-------|
| `check_desktop_status` | Verbindung + Ausführungszustand prüfen |
| `get_fpc_command_reference` | Vollständige Befehlsreferenz laden |
| `parse_fpc_program` | Syntaxprüfung ohne Laden |
| `validate_fpc_execution` | Laden + Validieren in ExecutionUnit |
| `load_program_to_desktop` | Programm in Desktop-App laden |
| `start_execution` / `stop_execution` | Ausführung starten/stoppen |
| `set_debug_mode` | Debug-Modus ein/aus |
| `step_program` | Einzelschritt (Debug-Modus) |
| `debug_snapshot` | Vollständiger Systemzustand |
| `debug_step_and_inspect` | Schritt + Snapshot atomar |
| `get_debug_info` | Aktuelle Ausführungszeile |
| `get_input_states` | Alle Eingänge mit Labels |
| `toggle_input` | Eingang simulieren |
| `set_input_label` | Eingangs-Label setzen |
| `get_output_states` | Alle Ausgänge mit Labels |
| `set_output_label` | Ausgangs-Label setzen |
| `get_memory` | Memory-Werte (M0–M63) |
| `get_timers` | Timer-Zustände |
| `get_counters` | Counter-Werte |
| `get_cycle_time` / `set_cycle_time` | Zykluszeit lesen/setzen |
| `delay` | Pause zwischen Operationen |

---

## REST-API (Port 5555) – Schnellreferenz

```
GET  /api/status              → running, debugEnabled, parseError
POST /api/program             → Programm laden (body: plain text, Content-Type: text/plain, KEIN JSON!)
POST /api/start               → Ausführung starten
POST /api/stop                → Ausführung stoppen
POST /api/debug               → Debug-Modus (body: {"enabled":true/false})
POST /api/step                → Einzelschritt (nur im Debug-Modus)
GET  /api/debug/snapshot      → Vollständiger Zustand (JSON)
GET  /api/inputs              → Alle Eingänge
POST /api/inputs/{n}/toggle   → Eingang n umschalten
GET  /api/outputs             → Alle Ausgänge
GET  /api/timers              → Timer-Zustände
GET  /api/counters            → Counter-Werte
GET  /api/memory              → Memory-Werte
POST /api/cycletime           → Zykluszeit setzen (body: {"cycleTimeMs":50})
POST /api/reset/outputs       → Alle Ausgänge zurücksetzen
```

---

## Ressourcen-Limits

| Ressource | Limit | Hinweis |
|-----------|-------|---------|
| Memory (M) | 1024 | Boolean-Merker |
| Timer (T) | 264 | Pulsierend, in ms |
| Counter (C) | 264 | Integer-Zähler |
| Inputs (I) | 64 | Konfigurierbar |
| Outputs (O) | 64 | Konfigurierbar |

---

## Referenz-Dateien

- `references/fpc-language.md` — Vollständige Befehlstabelle, Stack-Regeln, Timer-Semantik
- `references/fpc-patterns.md` — Bewährte Muster: Blinker, SR-Latch, TON/TOF, Zähler, Ampel, Anti-Patterns
- `/FPCSamples.md` (Projektverzeichnis) — Kuratierte Vollsammlung mit Testfällen und Tags für alle Kategorien; bei komplexen Anforderungen oder unbekannten Mustern nachlesen!
