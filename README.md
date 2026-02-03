# FreelyProgrammableControl

FreelyProgrammableControl ist eine kleine Steuerungs- und Simulationsumgebung in C#/.NET 8. Sie besteht aus
* einer Logik-Engine mit eigenem Befehlssatz ("FPC Instruction Set"), die Eingänge, Ausgänge, Speicher, Timer und Counter auswertet,
* einer Konsolen-App zum Laden und Ausführen von .fpc-Programmen sowie zum Beobachten/Toggeln der Eingänge,
* einer Avalonia-Desktop-App als UI-Starter, die aktuell als Basis dient.

## Architektur
- **Logic**: Kernkomponenten wie `ExecutionUnit`, `Inputs`, `Outputs`, `Memory<T>`, `Timers`, `Counters`, `Switch` ([FreelyProgrammableControl.Logic](FreelyProgrammableControl.Logic)). Die `ExecutionUnit` arbeitet mit einem Stack und führt die geparsten Befehlszeilen zyklisch aus.
- **Console App**: Menügestützte Steuerung der `ExecutionUnit`, Anzeige von Ausgängen und Countern in der Konsole, Laden von .fpc-Dateien ([FreelyProgrammableControl.ConApp](FreelyProgrammableControl.ConApp)).
- **Desktop App**: Avalonia-Anwendung (net8.0) mit Fluent-Theme, vorbereitet für eine UI auf Basis der Logik ([FreelyProgrammableControl.DesktopApp](FreelyProgrammableControl.DesktopApp)).

## Voraussetzungen
- .NET 8 SDK
- macOS, Linux oder Windows

## Build
```bash
dotnet build FreelyProgrammableControl.sln
```

## Konsole starten
```bash
dotnet run --project FreelyProgrammableControl.ConApp -- --program=Test.fpc --cycle=100
```
- Das Menü bietet Start/Stop der `ExecutionUnit` und pro Eingang einen Toggle.
- Argumente:
	- `--program=<pfad>`: Pfad zu einer `.fpc`-Datei (relativ oder absolut). Standard: `Test.fpc`.
	- `--cycle=<ms>`: Zykluszeit der Ausführung in Millisekunden. Standard: `100`.
- Beim Start wird die angegebene Datei geladen und geparst; Parse-Fehler werden in der Konsole angezeigt.
- Ausgänge und Counter werden laufend unterhalb des Menüs ausgegeben.

## Avalonia-Desktop starten
```bash
dotnet run --project FreelyProgrammableControl.DesktopApp
```
Aktuell zeigt die App nur ein "Welcome to Avalonia!"-Gerüst und bindet noch keine `ExecutionUnit`. Sie kann als Basis für eine Visualisierung genutzt werden.

## .fpc-Befehlssatz - Vollständige Befehlsübersicht

Jede Zeile wird zu einer `ParsedLine` verarbeitet. Kommentare beginnen mit `#`.

### Befehlstabelle

| Kategorie | Befehl | Operanden | Beschreibung | Stack-Effekt |
|-----------|--------|-----------|--------------|--------------|
| **Konstanten** | `GET 0` | - | Lädt die Konstante `false` (0) auf den Stack | Push(0) |
| | `GET 1` | - | Lädt die Konstante `true` (1) auf den Stack | Push(1) |
| **Lesen** | `GET I n` | n = Index | Lädt den Wert von Input `n` auf den Stack | Push(I[n]) |
| | `GET O n` | n = Index | Lädt den Wert von Output `n` auf den Stack | Push(O[n]) |
| | `GET M n` | n = Index | Lädt den Wert von Memory `n` auf den Stack | Push(M[n]) |
| | `GET T n` | n = Index | Lädt den Wert von Timer `n` auf den Stack | Push(T[n]) |
| **Negiertes Lesen** | `GETNOT I n` | n = Index | Lädt den negierten Wert von Input `n` auf den Stack | Push(!I[n]) |
| | `GETNOT O n` | n = Index | Lädt den negierten Wert von Output `n` auf den Stack | Push(!O[n]) |
| | `GETNOT M n` | n = Index | Lädt den negierten Wert von Memory `n` auf den Stack | Push(!M[n]) |
| | `GETNOT T n` | n = Index | Lädt den negierten Wert von Timer `n` auf den Stack | Push(!T[n]) |
| **Stack-Operationen** | `DUP` | - | Dupliziert den obersten Stack-Wert einmal | Push(Top) |
| | `DUP n` | n = Anzahl | Dupliziert den obersten Stack-Wert n-mal | Push(Top) × n |
| **Logische Operationen** | `NOT` | - | Negiert den obersten Stack-Wert | A → !A |
| | `AND` | - | Logisches UND der obersten zwei Stack-Werte | A, B → A && B |
| | `OR` | - | Logisches ODER der obersten zwei Stack-Werte | A, B → A \|\| B |
| | `XOR` | - | Logisches XOR der obersten zwei Stack-Werte | A, B → A ^ B |
| **Schreiben** | `MOV O n` | n = Index | Pop vom Stack und schreibt in Output `n` | Pop() → O[n] |
| | `MOV M n` | n = Index | Pop vom Stack und schreibt in Memory `n` | Pop() → M[n] |
| **Bedingtes Schreiben** | `CMOV O n v` | n = Index, v = Wert | Pop vom Stack, wenn true: Output `n` = v (0/1) | Pop(), wenn true: O[n] = v |
| | `CMOV M n v` | n = Index, v = Wert | Pop vom Stack, wenn true: Memory `n` = v (0/1) | Pop(), wenn true: M[n] = v |
| **Timer/Counter setzen** | `SET T n v` | n = Index, v = Wert | Startet pulsierenden Timer `n` mit v ms (true/false Wechsel). Timer pulsiert bis v=0 gesetzt wird | - |
| | `SET C n v` | n = Index, v = Wert | Setzt Counter `n` auf Wert v | - |
| **Bedingtes Setzen** | `CSET T n v` | n = Index, v = Wert | Pop vom Stack, wenn true: startet pulsierenden Timer `n` mit v ms. Timer pulsiert bis v=0 gesetzt wird | Pop(), wenn true: T[n] = v |
| | `CSET C n v` | n = Index, v = Wert | Pop vom Stack, wenn true: Counter `n` = v | Pop(), wenn true: C[n] = v |
| **Counter-Operationen** | `CINC C n` | n = Index | Pop vom Stack, wenn true: Counter `n` inkrementieren | Pop(), wenn true: C[n]++ |
| | `CDEC C n` | n = Index | Pop vom Stack, wenn true: Counter `n` dekrementieren | Pop(), wenn true: C[n]-- |
| **Vergleiche** | `CMP C n v` | n = Index, v = Wert | Vergleich: Counter `n` == v auf den Stack | Push(C[n] == v) |
| | `GT C n v` | n = Index, v = Wert | Vergleich: Counter `n` > v auf den Stack | Push(C[n] > v) |
| | `LE C n v` | n = Index, v = Wert | Vergleich: Counter `n` < v auf den Stack | Push(C[n] < v) |
| **Kommentare** | `# Text` | - | Kommentar, wird ignoriert | - |

### Operanden-Typen

| Typ | Bedeutung | Beispiel |
|-----|-----------|----------|
| `I` | Input (Eingang) | `GET I 0` - Liest Input 0 |
| `O` | Output (Ausgang) | `MOV O 5` - Schreibt in Output 5 |
| `M` | Memory (Speicher) | `GET M 10` - Liest Memory 10 |
| `T` | Timer (pulsierend) | `SET T 2 1000` - Timer 2 pulsiert mit 1000 ms (1s true, 1s false) |
| `C` | Counter (Zähler) | `CINC C 3` - Inkrementiert Counter 3 |

### Timer-Verhalten (WICHTIG!)

Timer in FPC funktionieren als **pulsierende Timer**:

- **`SET T n v`** startet einen pulsierenden Timer mit Periode `v` Millisekunden
- Der Timer wechselt automatisch zwischen `true` (v ms) und `false` (v ms)
- **Beispiel:** `SET T 0 500` 
  - Timer 0 ist 500 ms lang `true`
  - Dann 500 ms lang `false`
  - Dann wieder 500 ms `true`, usw.
- Der Timer pulsiert kontinuierlich, bis er mit `SET T n 0` gestoppt wird
- **Anwendung:** Ideal für Blinker, Taktgeber und periodische Signale

**Beispiel - Blinker:**
```
# LED blinkt mit 1 Hz (500ms an, 500ms aus)
SET T 0 500
GET T 0
MOV O 0
```

### Ressourcen-Limits

| Ressource | Standard-Größe | Beschreibung |
|-----------|----------------|--------------|
| Memory | 1024 | Boolean-Speicherzellen |
| Timers | 128 | Timer (in Millisekunden) |
| Counters | 128 | Integer-Zähler |
| Inputs | Konfigurierbar | Input-Geräte (default: 20) |
| Outputs | Konfigurierbar | Output-Geräte (default: 20) |
| Stack | Dynamisch | Boolean-Stack für Operationen |

## Beispielprogramme
### AND-Beispiel
[FreelyProgrammableControl.ConApp/And.fpc](FreelyProgrammableControl.ConApp/And.fpc)
```
G I 0
G I 1
AND
M O 0
```

### Testprogramm
[FreelyProgrammableControl.ConApp/Test.fpc](FreelyProgrammableControl.ConApp/Test.fpc) enthält weitere Beispiele für AND/OR/XOR, einen Blinker mit Timern und einen einfachen Zähler.

## Weiterentwicklungsideen
- Desktop-App mit Visualisierung der Ein-/Ausgänge und Timer/Counter anbinden
- Validierung/Fehleranzeige für .fpc-Dateien in der UI
- Tests für Parser (`ParsedLine`) und `ExecutionUnit`-Instruktionen ergänzen