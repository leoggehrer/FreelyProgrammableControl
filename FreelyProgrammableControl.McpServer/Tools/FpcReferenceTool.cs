using System.ComponentModel;
using ModelContextProtocol.Server;

namespace FreelyProgrammableControl.McpServer.Tools
{
    /// <summary>
    /// MCP-Tool zur Rückgabe der vollständigen FPC-Befehlsübersicht.
    /// </summary>
    [McpServerToolType]
    public static class FpcReferenceTool
    {
        private const string CommandReference = """
            # FPC – Befehlsübersicht (Freely Programmable Control)

            ## Grundregeln
            - Zeilenbasierte Sprache; jeder Befehl steht auf einer eigenen Zeile.
            - Kommentare beginnen mit `#` (ganze Zeile oder nach dem Befehl).
            - Leere Zeilen werden als NOP interpretiert.
            - Befehlsnamen können kurz (G, GN, D, N, A, O, X, M, CM, S, CS, CI, CD, C, GT, LE) oder lang geschrieben werden.
            - Alle Befehle arbeiten auf einem booleschen Stack (LIFO).
            - Der Stack wird zu Beginn jedes Zyklus automatisch geleert.

            ## Adressierbare Ressourcen
            | Ressource | Kürzel | Bereich                   | Typ    |
            |-----------|--------|---------------------------|--------|
            | Eingang   | I      | I0 … I63 (je nach Konfig) | bool   |
            | Ausgang   | O      | O0 … O63 (je nach Konfig) | bool   |
            | Merker    | M      | M0 … M1023                | bool   |
            | Timer     | T      | T0 … T127                 | bool   |
            | Zähler    | C      | C0 … C127                 | int    |

            **Timer-Verhalten:** Ein Timer mit Dauer D ms erzeugt ein Rechtecksignal.
            Er ist `TRUE` für die ersten D ms, dann `FALSE` für D ms, usw.
            Ein nicht gesetzter (Dauer = 0) Timer liefert immer `FALSE`.

            ## Lade-Befehle (Push auf Stack)

            ### GET – Wert lesen und auf Stack legen
            | Syntax              | Beschreibung                                      |
            |---------------------|---------------------------------------------------|
            | `GET 1`             | Schiebt konstant TRUE auf den Stack               |
            | `GET 0`             | Schiebt konstant FALSE auf den Stack              |
            | `GET I <adr>`       | Schiebt den Wert von Eingang I<adr> auf den Stack |
            | `GET O <adr>`       | Schiebt den Wert von Ausgang O<adr> auf den Stack |
            | `GET M <adr>`       | Schiebt den Wert von Merker M<adr> auf den Stack  |
            | `GET T <adr>`       | Schiebt den Wert von Timer T<adr> auf den Stack   |

            ### GETNOT – Negierten Wert lesen
            | Syntax              | Beschreibung                                           |
            |---------------------|--------------------------------------------------------|
            | `GETNOT I <adr>`    | Schiebt !I<adr> auf den Stack                          |
            | `GETNOT O <adr>`    | Schiebt !O<adr> auf den Stack                          |
            | `GETNOT M <adr>`    | Schiebt !M<adr> auf den Stack                          |
            | `GETNOT T <adr>`    | Schiebt !T<adr> auf den Stack                          |

            ## Stack-Operatoren

            | Befehl       | Beschreibung                                                     |
            |--------------|------------------------------------------------------------------|
            | `NOP`        | Keine Operation                                                  |
            | `DUP`        | Dupliziert das oberste Element zweimal (+1 auf Stack)            |
            | `DUP <n>`    | Schiebt das oberste Element n-1 mal zusätzlich (+n-1 auf Stack). `DUP 1` = dupliziert einmal (+1). |
            | `NOT`        | Negiert das oberste Stack-Element (pop → !wert → push)           |
            | `AND`        | Verknüpft die beiden obersten Elemente mit UND (2 pop, 1 push)   |
            | `OR`         | Verknüpft die beiden obersten Elemente mit ODER (2 pop, 1 push)  |
            | `XOR`        | Verknüpft die beiden obersten Elemente mit XOR (2 pop, 1 push)   |

            **Wichtiger Hinweis zu DUP:**
            `DUP 1` dupliziert den Wert genau einmal, sodass das nächste CSET/CMOV-Paar den Stack sauber hält.
            `DUP` (ohne Argument) dupliziert zweimal (+2 netto), was meist zu Stack-Überläufen führt.

            ## Schreib-Befehle (Pop vom Stack)

            ### MOV – Wert vom Stack schreiben
            | Syntax          | Beschreibung                                     |
            |-----------------|--------------------------------------------------|
            | `MOV O <adr>`   | Schreibt Stack-Pop → Ausgang O<adr>              |
            | `MOV M <adr>`   | Schreibt Stack-Pop → Merker M<adr>               |

            ### CMOV – Bedingtes Schreiben eines Konstantwerts
            | Syntax               | Beschreibung                                                 |
            |----------------------|--------------------------------------------------------------|
            | `CMOV O <adr> <val>` | Wenn Stack-Pop == TRUE → Ausgang O<adr> = (val > 0)          |
            | `CMOV M <adr> <val>` | Wenn Stack-Pop == TRUE → Merker M<adr> = (val > 0)           |

            ## Timer & Zähler

            ### SET – Unbedingtes Setzen
            | Syntax               | Beschreibung                            |
            |----------------------|-----------------------------------------|
            | `SET T <adr> <ms>`   | Setzt Timer T<adr> auf <ms> Millisekunden (kein Stack-Zugriff) |
            | `SET C <adr> <val>`  | Setzt Zähler C<adr> auf <val>           |

            ### CSET – Bedingtes Setzen (konsumiert Stack-Top)
            | Syntax                | Beschreibung                                         |
            |-----------------------|------------------------------------------------------|
            | `CSET T <adr> <ms>`   | Wenn Stack-Pop == TRUE → Timer T<adr> auf <ms> setzen |
            | `CSET C <adr> <val>`  | Wenn Stack-Pop == TRUE → Zähler C<adr> auf <val> setzen |

            ### CINC – Bedingtes Inkrementieren
            | Syntax                  | Beschreibung                                              |
            |-------------------------|-----------------------------------------------------------|
            | `CINC C <adr>`          | Wenn Stack-Pop == TRUE → C<adr> += 1                      |
            | `CINC C <adr> <schritt>`| Wenn Stack-Pop == TRUE → C<adr> += <schritt>              |

            ### CDEC – Bedingtes Dekrementieren
            | Syntax                  | Beschreibung                                              |
            |-------------------------|-----------------------------------------------------------|
            | `CDEC C <adr>`          | Wenn Stack-Pop == TRUE → C<adr> -= 1                      |
            | `CDEC C <adr> <schritt>`| Wenn Stack-Pop == TRUE → C<adr> -= <schritt>              |

            ## Vergleichsbefehle (Push Ergebnis auf Stack, kein Pop)

            | Syntax               | Beschreibung                                      |
            |----------------------|---------------------------------------------------|
            | `CMP C <adr> <val>`  | Schiebt (C<adr> == <val>) auf den Stack           |
            | `GT C <adr> <val>`   | Schiebt (C<adr> > <val>) auf den Stack            |
            | `LE C <adr> <val>`   | Schiebt (C<adr> <= <val>) auf den Stack           |

            ## Typische Muster

            ### Einmalige Initialisierung (mit Merker-Flag)
            ```fpc
            GETNOT M 10      # Wenn M10 noch nicht gesetzt
            DUP 1            # Wert duplizieren (für CSET + CMOV)
            CSET T 5 1000    # Timer T5 auf 1000ms setzen
            CMOV M 10 1      # M10 = TRUE (Flag setzen)
            ```

            ### Ausgang aus kombinierten Bedingungen setzen
            ```fpc
            GET I 0          # I0 lesen
            GET M 5          # M5 lesen
            AND              # I0 AND M5
            GET T 3          # T3 lesen
            OR               # (I0 AND M5) OR T3
            MOV O 2          # Ergebnis → Ausgang O2
            ```

            ### Zähler-Bereichsprüfung
            ```fpc
            GT C 0 100       # C0 > 100
            LE C 0 200       # C0 <= 200
            AND              # 100 < C0 <= 200
            MOV O 1          # Ergebnis → Ausgang O1
            ```

            ### Zähler per Timer inkrementieren und rücksetzen
            ```fpc
            GET T 10         # Timer-Takt
            CINC C 0         # C0 += 1 wenn T10=TRUE

            GT C 0 250       # C0 > 250?
            CSET C 0 1       # Wenn ja: C0 = 1 (Reset auf 1)
            ```
            """;

        /// <summary>
        /// Gibt die vollständige FPC-Befehlsübersicht zurück.
        /// </summary>
        [McpServerTool(Name = "get_fpc_command_reference")]
        [Description(
            "Returns the complete FPC (Freely Programmable Control) command reference in German Markdown format. " +
            "Includes all instructions (GET, GETNOT, DUP, NOT, AND, OR, XOR, MOV, CMOV, SET, CSET, CINC, CDEC, CMP, GT, LE, NOP), " +
            "their syntax, stack behaviour, addressable resources (I/O/M/T/C), and typical usage patterns. " +
            "Use this tool whenever you need to write or verify an FPC program.")]
        public static FpcReferenceResult GetFpcCommandReference()
        {
            return new FpcReferenceResult
            {
                Success = true,
                Reference = CommandReference,
                Message = "FPC-Befehlsübersicht abgerufen"
            };
        }
    }

    /// <summary>Result returned by <see cref="FpcReferenceTool.GetFpcCommandReference"/>.</summary>
    public class FpcReferenceResult
    {
        public bool Success { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
