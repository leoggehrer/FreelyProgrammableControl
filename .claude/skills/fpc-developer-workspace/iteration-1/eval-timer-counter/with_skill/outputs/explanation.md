# Erklärung: 10x-Taster -> O0 leuchtet 3 Sekunden

## Verwendete Ressourcen

| Ressource | Zweck |
|-----------|-------|
| I0 | Taster (steigende Flanken zählen) |
| I1 | Zähler zurücksetzen |
| C0 | Flanken-Zähler für I0 |
| M0 | Flanken-Merker (speichert vorherigen I0-Zustand) |
| M1 | Timer-Aktiv-Merker (O0 soll leuchten) |
| M2 | Timer-Init-Flag (verhindert mehrfaches Starten) |
| T0 | 3-Sekunden-Timer für O0 |
| O0 | Ausgang (leuchtet 3 Sekunden nach 10 Drücken) |

## Programm-Abschnitte

### Abschnitt 1: Steigende Flanke erkennen und zählen
Eine **steigende Flanke** liegt vor, wenn I0 jetzt `true` ist und im letzten Zyklus `false` war (M0=false).  
Das Muster `GET I0 AND GETNOT M0` ergibt genau dann `true`, wenn diese Bedingung erfüllt ist.  
`CINC C0` zählt nur bei `true` - also nur bei echter steigender Flanke.  
Danach wird M0 mit dem aktuellen I0-Zustand aktualisiert (für den nächsten Zyklus).

### Abschnitt 2: Reset mit I1
Wenn I1 gedrückt wird: Zähler C0 auf 0, Timer-Aktiv-Merker M1 auf 0, Timer-Init-Flag M2 auf 0.  
Das `DUP`-Muster wird verwendet weil I1 für drei konsumierende Befehle gebraucht wird.

### Abschnitt 3: Timer starten bei 10. Druck
Die Bedingung `CMP C0 10 AND GETNOT M2` ist genau dann `true`, wenn C0 exakt 10 erreicht hat **und** der Timer noch nicht läuft (M2=false).  
Dann: Timer T0 für 3000ms starten, M1=1 (O0 soll leuchten), M2=1 (verhindert erneutes Starten).  
Das doppelte `DUP` ist nötig, weil die Bedingung drei konsumierende Befehle braucht.

### Abschnitt 4: O0 automatisch erlöschen
Prüft: `M1=true` (Timer läuft gerade) **und** `T0=false` (3-Sekunden-Phase gerade abgelaufen).  
Wenn beide wahr: M1=0 (O0 erlöscht), C0=0 (Zähler für nächsten Durchlauf zurücksetzen).  
**Wichtig:** Timer T0 pulsiert weiter (3s true -> 3s false -> ...), aber da M1=false ist, bleibt O0 aus.

### Abschnitt 5: Output setzen
O0 folgt dem Merker M1. So einfach ist es.

## Warum Memory-Flag M2?

Ohne M2 würde der Timer in jedem Zyklus neu gestartet, sobald C0==10 ist - und zwar auch während die 3 Sekunden laufen. M2 sichert, dass der Timer nur **einmal** gestartet wird. I1 setzt M2 zurück, damit der nächste Durchlauf funktioniert.

## Getestetes Verhalten

- 10x I0 drücken: O0 leuchtet auf
- O0 erlischt automatisch nach 3 Sekunden
- I1 setzt Zähler zurück (auch während O0 leuchtet)
- Nach Reset: erneutes 10x Drücken funktioniert wieder
