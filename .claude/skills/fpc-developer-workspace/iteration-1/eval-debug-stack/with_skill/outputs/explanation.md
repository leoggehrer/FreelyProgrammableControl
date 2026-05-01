# Erklarung der Korrekturen

## Was war falsch?

Das Programm hatte zwei Stack-Unterlauf-Fehler ("Stack is empty!"):

1. **Timer-Init ohne DUP**: `GETNOT M 0` legt einen Wert auf den Stack. Sowohl `CSET T 0 500` als auch `CMOV M 0 1` wollen diesen Wert verbrauchen — aber es gibt nur einen. Losung: `DUP` zwischen `GETNOT M 0` und `CSET` einfugen, damit beide Befehle ihren Wert bekommen.

2. **Zwei MOV ohne DUP**: `AND` erzeugt einen Wert. `MOV O 0` verbraucht ihn. Danach ist der Stack leer, `MOV O 1` findet nichts. Losung: `DUP` vor den beiden `MOV`-Befehlen einfugen.

## Regel

Immer wenn ein Stack-Wert von mehr als einem konsumierenden Befehl benotigt wird, muss er mit `DUP` dupliziert werden. Das verbindliche Timer-Initialisierungsmuster ist:

```
GETNOT M n
DUP
CSET T n v
CMOV M n 1
```
