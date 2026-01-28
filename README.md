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

## .fpc-Befehlssatz (Kurzreferenz)
Jede Zeile wird zu einer `ParsedLine` verarbeitet. Kommentare beginnen mit `#`.

### Operanden laden
- `GET 1` / `GET 0`: Konstante auf den Stack
- `GET I|O|M|T n`: Wert von Input/Output/Memory/Timer `n` auf den Stack
- `GETNOT I|O|M|T n`: Negierten Wert auf den Stack

### Stack/Logik
- `DUP` oder `DUP n`: Obersten Stackwert duplizieren (n-mal)
- `NOT`, `AND`, `OR`, `XOR`: Logische Verknüpfungen auf den obersten Stackwerten

### Schreiben
- `MOV O|M n`: Pop und in Output/Memory `n` schreiben
- `CMOV O|M n v`: Pop, wenn true: Output/Memory `n` auf `v` (0/1) setzen

### Timer/Counter
- `SET T|C n v`: Timer/Counter setzen
- `CSET T|C n v`: Pop, wenn true: Timer/Counter setzen
- `CINC C n` / `CDEC C n`: Pop, wenn true: Counter inkrementieren/dekrementieren

### Vergleiche
- `CMP C n v`: Counter `n` == `v` auf den Stack
- `GT C n v`: Counter `n` > `v` auf den Stack
- `LE C n v`: Counter `n` < `v` auf den Stack

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