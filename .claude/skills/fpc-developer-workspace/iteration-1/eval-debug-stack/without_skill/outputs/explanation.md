# Erlaeuterung der Korrekturen

Das Originalprogramm hat zwei Stack-Fehler:

## Fehler 1: CSET verbraucht den einzigen Stack-Wert

GETNOT M 0 schiebt einen Wert auf den Stack.
CSET T 0 500 liest (pop) diesen Wert als Bedingung.
CMOV M 0 1 findet danach einen leeren Stack => "Stack is empty!"

Korrektur: GETNOT M 0 vor CMOV wiederholen, damit beide Befehle je einen Wert bekommen.

## Fehler 2: AND-Ergebnis wird zweimal benoetigt

AND produziert einen Bool-Wert. MOV O 0 verbraucht ihn.
MOV O 1 findet danach einen leeren Stack => "Stack is empty!"

Korrektur: DUP vor den beiden MOV-Befehlen, um den Wert zu duplizieren.

## Korrigiertes Programm

```
# Blinker mit Freigabe
GETNOT M 0
CSET T 0 500
GETNOT M 0
CMOV M 0 1
GET I 0
GET T 0
AND
DUP
MOV O 0
MOV O 1
```

Stack-Trace des korrigierten Programms:
1. GETNOT M 0   => [NOT_M0]
2. CSET T 0 500 => []          (T 0 auf 500ms gesetzt wenn M 0 false)
3. GETNOT M 0   => [NOT_M0]
4. CMOV M 0 1   => []          (M 0 auf 1 gesetzt wenn M 0 false => einmaliges Init-Flag)
5. GET I 0      => [I0]
6. GET T 0      => [I0, T0]
7. AND          => [I0 AND T0]
8. DUP          => [I0 AND T0, I0 AND T0]
9. MOV O 0      => [I0 AND T0]
10. MOV O 1     => []

Stack am Ende leer: kein Fehler.
