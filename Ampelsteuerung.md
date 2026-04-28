Erstelle eine Ampelsteuerung mit folgendem Verhalten:

Eingang...I
Ausgang...O

| Zeit in Sekunden | I 0 (NOTAUS) | O 0 (ROT) | O 1 (GELB) | O 2 (GRÜN) |
| ---------------- | ------------ | --------- | ---------- | ----------- |
| beliebig         | 1            | 1         | 0          | 0           |
| 0 bis 10         | 0            | 1         | 0          | 0           |
| 10 bis 13        | 0            | 1         | 1          | 0           |
| 13 bis 20        | 0            | 0         | 0          | 1           |
| 20 bis 20.5      | 0            | 0         | 0          | 0           |
| 20.5 bis 21      | 0            | 0         | 0          | 1           |
| 21 bis 21.5      | 0            | 0         | 0          | 0           |
| 21.5 bis 22      | 0            | 0         | 0          | 1           |
| 22 bis 22. 5     | 0            | 0         | 0          | 0           |
| 22.5 bis 23      | 0            | 0         | 0          | 1           |
| 23 bis 25        | 0            | 0         | 1          | 0           |

ACHTUNG: Nach der Deaktivierung vom NOTAUS startet die ZEIT wieder von Beginn an!

Dieser Ablauf wiederholt sich immer wieder!

Teste das Programm ausführlich!

