# Fehleranalyse: "Stack is empty!"

## Fehlerhaftes Programm (Original)

GETNOT M 0
CSET T 0 500    <- verbraucht Stack-Wert
CMOV M 0 1      <- Stack leer! -> "Stack is empty!"
GET I 0
GET T 0
AND
MOV O 0
MOV O 1         <- ebenfalls Stack leer nach MOV O 0

## Fehler 1: Timer-Init ohne DUP (Hauptfehler)

GETNOT M 0 legt einen Wert auf den Stack.
CSET T 0 500 verbraucht (pop) diesen Wert.
CMOV M 0 1 versucht ebenfalls einen Wert zu holen -- Stack ist leer.

Stack-Trace:
  GETNOT M 0    depth: 0 -> 1   [!M0]
  CSET T 0 500  depth: 1 -> 0   []
  CMOV M 0 1    depth: 0 -> -1  FEHLER: Stack is empty!

Regel: Wenn ein Wert zwei konsumierenden Befehlen zugefuehrt werden muss, DUP verwenden.
Verbindliches Timer-Init-Muster:
  GETNOT M n
  DUP
  CSET T n v
  CMOV M n 1

## Fehler 2: Zwei MOV ohne DUP

AND erzeugt einen Wert. MOV O 0 verbraucht ihn.
MOV O 1 findet dann einen leeren Stack.

Stack-Trace (nach Fix von Fehler 1):
  GET I 0   depth: 0 -> 1   [I0]
  GET T 0   depth: 1 -> 2   [I0, T0]
  AND       depth: 2 -> 1   [I0&&T0]
  MOV O 0   depth: 1 -> 0   []
  MOV O 1   depth: 0 -> -1  FEHLER: Stack is empty!

Korrektur: DUP vor den beiden MOV-Befehlen.

## Stack-Trace des korrigierten Programms

  GETNOT M 0    depth: 0 -> 1  [!M0]
  DUP           depth: 1 -> 2  [!M0, !M0]
  CSET T 0 500  depth: 2 -> 1  [!M0]
  CMOV M 0 1    depth: 1 -> 0  []
  GET I 0       depth: 0 -> 1  [I0]
  GET T 0       depth: 1 -> 2  [I0, T0]
  AND           depth: 2 -> 1  [I0&&T0]
  DUP           depth: 1 -> 2  [I0&&T0, I0&&T0]
  MOV O 0       depth: 2 -> 1  [I0&&T0]
  MOV O 1       depth: 1 -> 0  []

Stack am Ende: leer (Tiefe = 0) -- korrekt.
