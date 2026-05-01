# Erklärung: Zähler mit 10 Tastendrücken → O0 für 3 Sekunden

## Aufgabe

- **I0**: Taster – steigende Flanken (0→1) werden gezählt
- **I1**: Reset-Taster – setzt Zähler und Ausgabe zurück
- **O0**: Lampe – leuchtet 3 Sekunden, nachdem I0 exakt 10-mal gedrückt wurde

---

## Verwendete Ressourcen

| Ressource | Bedeutung |
|-----------|-----------|
| `C0`      | Flankenzähler für I0 |
| `T0`      | 3000ms-Timer (One-Shot) |
| `M0`      | Vorheriger Zustand von I0 (für Flankenerkennung) |
| `M1`      | Latch: O0 soll leuchten |
| `M2`      | Flag: Timer T0 wurde bereits gestartet |
| `M9`      | Wegwerf-Merker (zum Entfernen von Stack-Werten) |

---

## Programmablauf (pro Zyklus)

### 1. Steigende Flanke erkennen (Schritt 1–2)
Eine steigende Flanke liegt vor, wenn I0 aktuell **HIGH** ist und im letzten Zyklus **LOW** war (gespeichert in M0). Diese Bedingung ergibt `I0 AND NOT(M0)`. Ist sie wahr, wird C0 via `CINC` um 1 erhöht.

### 2. Vorherigen Zustand speichern (Schritt 3)
Am Ende jedes Zyklus wird der aktuelle Wert von I0 in M0 gespeichert, damit im nächsten Zyklus die Flanke erkannt werden kann.

### 3. Zähler-Vergleich und Latch setzen (Schritt 4–6)
Mit `CMP C 0 10` wird geprüft, ob C0 genau 10 beträgt. Ist das der Fall:
- M1 (Latch) wird auf 1 gesetzt → O0 soll leuchten
- C0 wird auf 0 zurückgesetzt, damit der Zähler nicht bei 10 stecken bleibt und kein Dauerfeuer entsteht

### 4. Timer einmalig starten (Schritt 7)
Der Timer T0 wird **nur gestartet, wenn M1=1 UND M2=0** – also nur im ersten Zyklus nach Latch-Setzen. Danach wird M2=1 gesetzt, sodass T0 nicht jedes Zyklus neu gestartet wird. Würde T0 jedes Zyklus auf 3000ms gesetzt, würde er nie ablaufen.

### 5. Timer-Ablauf und Latch löschen (Schritt 8)
Wenn M1=1, M2=1 und T0=FALSE (Timer abgelaufen), werden M1 und M2 auf 0 gesetzt. Dadurch erlischt O0 automatisch.

### 6. Ausgabe setzen (Schritt 9)
O0 folgt direkt dem Latch M1: `O0 = M1`.

### 7. Reset durch I1 (Schritt 10)
Wenn I1 HIGH ist, werden C0, M1 und M2 sofort auf 0 gesetzt. Damit wird der gesamte Vorgang abgebrochen und O0 erlischt.

---

## Hinweis zum Timer

FPC-Timer sind pulsierend: nach `CSET T 0 3000` ist T0 für 3000ms TRUE, dann 3000ms FALSE, usw. Der Trick ist, den Timer **nur einmal zu starten** (via M2-Flag) und das Abfallen von T0 auf FALSE als Signal zu nutzen, dass 3 Sekunden abgelaufen sind. Wird der Timer jedes Zyklus neu gesetzt, läuft er nie ab – daher das M2-Schutz-Flag.
