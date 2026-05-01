# Stack-Fehler Analyse

## Originalprogramm

```
# Blinker mit Freigabe
GETNOT M 0
CSET T 0 500
CMOV M 0 1
GET I 0
GET T 0
AND
MOV O 0
MOV O 1
```

## Stack-Trace (Schritt für Schritt)

| Schritt | Befehl       | Stack davor | Operation                             | Stack danach |
|---------|--------------|-------------|---------------------------------------|--------------|
| 1       | GETNOT M 0   | []          | Push NOT(M[0])                        | [NOT_M0]     |
| 2       | CSET T 0 500 | [NOT_M0]    | Pop Bedingung, setze T[0]=500 ms wenn | []           |
| 3       | CMOV M 0 1   | []          | FEHLER: Pop noetig, Stack leer!       | —            |

## Fehler 1: CMOV M 0 1 bei leerem Stack (Zeile 4)

CSET T 0 500 verbraucht (pop) den einzigen Stack-Wert von GETNOT M 0.
Anschliessend versucht CMOV M 0 1 einen Wert vom Stack zu lesen — der Stack ist leer.
=> "Stack is empty!"

## Fehler 2: MOV O 1 bei leerem Stack (Zeile 9)

AND produziert einen Wert, MOV O 0 verbraucht ihn.
Danach versucht MOV O 1 erneut zu poppen — Stack ist leer.
=> zweites "Stack is empty!"

## Ursache

- CSET und CMOV benoetigen je einen eigenen Stack-Wert als Bedingung.
- Es wird aber nur ein Wert gepusht (GETNOT M 0).
- Das AND-Ergebnis wird zweimal benoetigt (fuer O 0 und O 1), aber nur einmal produziert.

## Korrekturen

1. Vor CMOV M 0 1 ein weiteres GETNOT M 0 einfuegen (neue Bedingung fuer CMOV).
2. Nach AND ein DUP einfuegen, um das Ergebnis fuer beide Ausgaenge bereitzustellen.
