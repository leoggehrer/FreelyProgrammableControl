Erstelle eine Ampelsteuerung mit folgendem Verhalten:

| Zeit in Sekunden | Eingang 0 (NOTAUS) | Ausgang 0 (ROT) | Ausgang 1 (GELB) | Ausgang 2 (GRÜN) |
| ---------------- | ------------------ | --------------- | ---------------- | ----------------- |
| beliebig         | 1                  | 1               | 0                | 0                 |
| 0 bis 10         | 0                  | 1               | 0                | 0                 |
| 10 bis 13        | 0                  | 1               | 1                | 0                 |
| 13 bis 20        | 0                  | 0               | 0                | 1                 |
| 20 bis 20.5      | 0                  | 0               | 0                | 0                 |
| 20.5 bis 21      | 0                  | 0               | 0                | 1                 |
| 21 bis 21.5      | 0                  | 0               | 0                | 0                 |
| 21.5 bis 22      | 0                  | 0               | 0                | 1                 |
| 22 bis 22. 5     | 0                  | 0               | 0                | 0                 |
| 22.5 bis 23      | 0                  | 0               | 0                | 1                 |
| 23 bis 25        | 0                  | 0               | 1                | 0                 |

> Dieser Ablauf wiederholt sich immer wieder!
