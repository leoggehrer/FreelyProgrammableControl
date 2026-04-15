#!/usr/bin/env node
const fs = require('fs');
const path = require('path');

const repoRoot = process.cwd();

function readJson(relPath) {
  const abs = path.join(repoRoot, relPath);
  return JSON.parse(fs.readFileSync(abs, 'utf8'));
}

function writeJson(relPath, data) {
  const abs = path.join(repoRoot, relPath);
  fs.writeFileSync(abs, JSON.stringify(data, null, 2) + '\n', 'utf8');
}

function listNodeNames(workflow) {
  return (workflow.nodes || []).map((n) => n.name).filter(Boolean).sort();
}

function findNodeByNames(workflow, names, workflowLabel) {
  const node = (workflow.nodes || []).find((n) => names.includes(n.name));
  if (!node) {
    throw new Error(
      `${workflowLabel}: Kein passender Node fuer [${names.join(', ')}]. Vorhandene Nodes: ${listNodeNames(workflow).join(', ')}`
    );
  }
  return node;
}

function ensurePath(obj, pathParts, label) {
  let cur = obj;
  for (const part of pathParts) {
    if (cur == null || !(part in cur)) {
      throw new Error(`${label}: Pfad fehlt: ${pathParts.join('.')}`);
    }
    cur = cur[part];
  }
  return cur;
}

function applyReplacements(content, replacements, label) {
  let result = content;
  let applied = 0;
  for (const [from, to] of replacements) {
    if (result.includes(from)) {
      result = result.replace(from, to);
      applied += 1;
    }
  }
  return { result, applied, total: replacements.length, label };
}

function updateValidatorCodeString(jsCode) {
  let out = jsCode;
  // Normalize tool input handling to the resilient form used across workflows.
  if (out.includes("const code = $input.first().json.query || $input.first().json.chatInput || '")) {
    out = out.replace(
      "const code = $input.first().json.query || $input.first().json.chatInput || '';",
      "const inp = $input.first().json;\nconst code = inp.query || inp.input || inp.code || inp.programCode || '';"
    );
  }
  if (out.includes("const code = $input.first().json.query || $input.first().json.chatInput || '';")) {
    out = out.replace(
      "const code = $input.first().json.query || $input.first().json.chatInput || '';",
      "const inp = $input.first().json;\nconst code = inp.query || inp.input || inp.code || inp.programCode || '';"
    );
  }
  return out;
}

function updateMultiAgentPipeline() {
  const relPath = 'FPCMultiAgentPipeline.json';
  const wf = readJson(relPath);
  const orchestrator = findNodeByNames(wf, ['Orchestrator'], relPath);
  const validatorNode = findNodeByNames(wf, ['FPCValidator', 'fpc_validator'], relPath);

  const systemMessage = ensurePath(orchestrator, ['parameters', 'options', 'systemMessage'], `${relPath}/Orchestrator`);
  const messageReplacements = [
    [
      '- `GET`/`GETNOT` pushen Werte auf den Stack',
      '- `GET`/`GETNOT` pushen Werte auf den Stack\n- `GET 0` und `GET 1` sind boolesche Konstanten'
    ],
    [
      '- `MOV`/`CMOV` konsumieren (poppen) Stack-Werte und schreiben in Ausgaenge/Memory',
      '- `MOV` poppt genau einen Stack-Wert und schreibt ihn nach `O` oder `M`'
    ],
    [
      '- Bedingte Befehle (`CMOV`, `CSET`, `CINC`, `CDEC`) poppen den Stack und fuehren die Aktion nur bei true aus',
      '- `CMOV`, `CSET`, `CINC`, `CDEC` poppen genau einen Stack-Wert und fuehren die Aktion nur bei true aus\n\n### Gueltige Operandenformen\n- `GET 0`, `GET 1`, `GET I/O/M/T n`\n- `GETNOT I/O/M/T n`\n- `MOV O/M n`\n- `CMOV O/M n v`\n- `SET T/C n v`\n- `CSET T/C n v`\n- `CINC C n` oder `CINC C n v`\n- `CDEC C n` oder `CDEC C n v`\n- `CMP/GT/LE C n v`\n- `GET C` existiert NICHT\n- `MOV` mit vier Teilen wie `MOV M 0 1` ist ungueltig'
    ],
    [
      '- `MOV`, `CMOV`, `CSET` -> -1 (benoetigt 1 Wert)',
      '- `MOV`, `CMOV`, `CSET`, `CINC`, `CDEC` -> -1 (benoetigt 1 Wert)'
    ],
    [
      '- `SET`, `CINC`, `CDEC` -> 0',
      '- `SET` -> 0'
    ],
    [
      '- Das Tool prueft: erlaubte Befehle, Stack-Balance, Ressourcen-Grenzen',
      '- Das Tool prueft: erlaubte Befehle, Stack-Balance, Ressourcen-Grenzen, Operand-Typen und Befehlsformate'
    ]
  ];
  const msgUpdate = applyReplacements(systemMessage, messageReplacements, `${relPath}/Orchestrator`);
  orchestrator.parameters.options.systemMessage = msgUpdate.result;

  const validatorCode = ensurePath(validatorNode, ['parameters', 'jsCode'], `${relPath}/${validatorNode.name}`);
  validatorNode.parameters.jsCode = updateValidatorCodeString(validatorCode);

  writeJson(relPath, wf);
  return `${relPath}: ${msgUpdate.applied}/${msgUpdate.total} Text-Anpassungen, Validator '${validatorNode.name}' robust normalisiert.`;
}

function updateRequestEnricher() {
  const relPath = 'FPCRequestEnricher.json';
  const wf = readJson(relPath);
  const enricherNode = findNodeByNames(wf, ['EnricherChain'], relPath);
  const text = ensurePath(enricherNode, ['parameters', 'text'], `${relPath}/EnricherChain`);
  const replacements = [
    [
      '- Lesen: GET I/O/M/T/C n, GETNOT I/O/M/T n',
      '- Lesen: GET 0/1, GET I/O/M/T n, GETNOT I/O/M/T n'
    ],
    [
      '- Counter: SET C n v, CINC C n, CDEC C n, CMP/GT/LE C n v',
      '- Counter: SET C n v, CSET C n v, CINC C n [v], CDEC C n [v], CMP/GT/LE C n v'
    ],
    [
      'Gib GENAU dieses Format aus (kein Markdown ausserhalb des Blocks):',
      'WICHTIG:\n- GET C existiert NICHT. Counter werden nur mit CMP, GT, LE gelesen.\n- MOV hat genau 3 Teile: MOV O n oder MOV M n.\n- CMOV hat genau 4 Teile: CMOV O/M n v.\n- CINC und CDEC sind bedingt und benoetigen eine Bedingung auf dem Stack.\n\nGib GENAU dieses Format aus (kein Markdown ausserhalb des Blocks):'
    ]
  ];
  const update = applyReplacements(text, replacements, `${relPath}/EnricherChain`);
  enricherNode.parameters.text = update.result;
  writeJson(relPath, wf);
  return `${relPath}: ${update.applied}/${update.total} Text-Anpassungen angewendet.`;
}

function updateDebugValidator() {
  const relPath = 'FPCDebug.json';
  const wf = readJson(relPath);
  const validatorNode = findNodeByNames(wf, ['FPCValidator', 'fpc_validator'], relPath);
  const jsCode = ensurePath(validatorNode, ['parameters', 'jsCode'], `${relPath}/${validatorNode.name}`);
  validatorNode.parameters.jsCode = updateValidatorCodeString(jsCode);
  writeJson(relPath, wf);
  return `${relPath}: Validator '${validatorNode.name}' robust normalisiert.`;
}

function updateSelfLearning() {
  const relPath = 'FPCSelfLearning.json';
  const wf = readJson(relPath);

  const validatorNode = findNodeByNames(wf, ['fpc_validator', 'FPCValidator'], relPath);
  const jsCode = ensurePath(validatorNode, ['parameters', 'jsCode'], `${relPath}/${validatorNode.name}`);
  validatorNode.parameters.jsCode = updateValidatorCodeString(jsCode);

  const agentNode = findNodeByNames(wf, ['LearningAgent'], relPath);
  const systemMessage = ensurePath(agentNode, ['parameters', 'options', 'systemMessage'], `${relPath}/LearningAgent`);
  const selfLearningReplacements = [
    [
      '### Fall D: Benutzer beantwortet die Speicherfrage\\n- Ja: store_fpc_pattern aufrufen\\n- Nein: nicht speichern\\n- Von Steuerung: get_program_from_controller -> fpc_validator -> store_fpc_pattern\\n\\n---',
      '### Fall D: Benutzer beantwortet die Speicherfrage\\n- Ja: Nur speichern, wenn die Lösung wirklich eine neue Gesamtlösung ist. Sonst transparent ablehnen und auf das vorhandene Muster verweisen.\\n- Nein: nicht speichern\\n- Von Steuerung: get_program_from_controller -> fpc_validator -> bei gueltigem und eigenstaendigem Ergebnis store_fpc_pattern\\n\\nAntworttemplates fuer die Ausgabe:\\n- Ja und Speicherung erfolgt: \"Ich habe die Loesung in FPCKnowledge gespeichert.\"\\n- Ja aber nicht speicherfaehig (kein neues Muster): \"Ich speichere diese Loesung nicht, weil sie im Kern auf einem bereits vorhandenen Muster basiert und keine eigenstaendige neue Gesamtlösung darstellt.\"\\n- Nein: \"Alles klar, ich speichere die Loesung nicht in FPCKnowledge.\"\\n- Von Steuerung und Speicherung erfolgt: \"Ich habe den aktuellen Stand von der Steuerung geholt, validiert und in FPCKnowledge gespeichert.\"\\n- Von Steuerung aber nicht speicherfaehig: \"Ich habe den Stand von der Steuerung geholt und validiert, speichere ihn aber nicht, weil kein neues eigenstaendiges Muster vorliegt.\"\\n\\n---'
    ],
    [
      '5. Uebernimm nie ungeprueft ein komplettes Muster. Integriere nur das, was zur konkreten Aufgabe passt.\\n\\n### 4) Gesamtlösung entwickeln',
      '5. Uebernimm nie ungeprueft ein komplettes Muster. Integriere nur das, was zur konkreten Aufgabe passt.\\n6. Dokumentiere intern, welche Teilmuster verwendet wurden und welche Anteile neu entwickelt wurden. Diese Information steuert spaeter die Speicherentscheidung.\\n\\n### 4) Gesamtlösung entwickeln'
    ],
    [
      'Dann frage: \"Soll ich die Loesung in FPCKnowledge speichern? (Ja / Nein / Von Steuerung)\"\\n\\nBeim Speichern transparent machen:\\n- Wenn passende Teilloesungen aus FPCKnowledge genutzt wurden, speichere nur dann, wenn daraus eine neue, eigenstaendige Gesamtlösung fuer die konkrete Aufgabe entstanden ist.\\n- Wenn praktisch nur ein bestehendes Muster wiederverwendet wurde, nicht erneut speichern. Weise stattdessen darauf hin.\\n\\n---',
      'Dann frage: \"Soll ich die Loesung in FPCKnowledge speichern? (Ja / Nein / Von Steuerung)\"\\n\\n### 9) Strenge Speicherentscheidung\\nWenn der Benutzer \"Ja\" oder \"Von Steuerung\" antwortet, entscheide vor dem Speichern strikt nach diesen Regeln:\\n1. Speichern erlaubt: mehrere Teilmuster wurden sinnvoll zu einer neuen Gesamtlösung kombiniert oder wesentliche neue Logik wurde entwickelt.\\n2. Speichern NICHT erlaubt: es wurde im Wesentlichen nur ein vorhandenes Pattern uebernommen, geringfuegig umbenannt oder nur auf andere Ein-/Ausgaenge gemappt.\\n3. Im Nicht-Speichern-Fall erklaere knapp, welches bestehende Muster bereits die Basis war und warum kein neuer FPCKnowledge-Eintrag erzeugt wird.\\n4. Im Speichern-Fall muss pageContent klar enthalten: Aufgabenstellung, finale Kanalbelegung, finaler Code, Testmatrix, Iterationen, verwendete Teilmuster und den Hinweis, warum dies eine neue Gesamtlösung ist.\\n5. Wenn \"Von Steuerung\": hole den aktuellen Code, validiere ihn und speichere auch dann nur, wenn die Regeln 1 bis 4 erfuellt sind.\\n\\n---'
    ]
  ];
  const msgUpdate = applyReplacements(systemMessage, selfLearningReplacements, `${relPath}/LearningAgent`);
  agentNode.parameters.options.systemMessage = msgUpdate.result;

  const storeTool = findNodeByNames(wf, ['StoreFPCPattern'], relPath);
  const desc = ensurePath(storeTool, ['parameters', 'description'], `${relPath}/StoreFPCPattern`);
  const descFrom = 'Speichert ein verifiziertes FPC-Programm-Muster im Vector Store. NUR aufrufen wenn der Benutzer explizit Ja gesagt hat oder sein angepasstes Programm von der Steuerung gespeichert werden soll, UND kein passendes Muster bereits im Vector Store vorhanden ist. Uebergib: pageContent (Beschreibung + Code + Stack-Fluss + Testergebnisse), title, category (Grundlagen/Timer und Blinker/Counter Operationen/Toggle-Logik/Pattern/Komplex), level (Einfach/Mittel/Komplex), tags (komma-separiert), commands (verwendete FPC-Befehle komma-separiert).';
  const descTo = 'Speichert ein verifiziertes FPC-Programm-Muster im Vector Store. NUR aufrufen wenn der Benutzer explizit Ja gesagt hat oder sein angepasstes Programm von der Steuerung gespeichert werden soll, UND kein passendes Muster bereits im Vector Store vorhanden ist, UND die Loesung eine eigenstaendige neue Gesamtlösung ist statt nur ein vorhandenes Muster minimal umzubenennen. Uebergib: pageContent (Beschreibung + Code + Stack-Fluss + Testergebnisse), title, category (Grundlagen/Timer und Blinker/Counter Operationen/Toggle-Logik/Pattern/Komplex), level (Einfach/Mittel/Komplex), tags (komma-separiert), commands (verwendete FPC-Befehle komma-separiert).';
  storeTool.parameters.description = desc.includes(descFrom) ? desc.replace(descFrom, descTo) : desc;

  writeJson(relPath, wf);
  return `${relPath}: Validator '${validatorNode.name}' normalisiert, Prompt-Anpassungen ${msgUpdate.applied}/${msgUpdate.total}.`;
}

function main() {
  const actions = [
    updateMultiAgentPipeline,
    updateRequestEnricher,
    updateDebugValidator,
    updateSelfLearning
  ];
  const results = [];
  for (const fn of actions) {
    results.push(fn());
  }
  console.log('Workflow-Update erfolgreich:');
  for (const line of results) {
    console.log(`- ${line}`);
  }
}

try {
  main();
} catch (err) {
  console.error('Workflow-Update fehlgeschlagen:');
  console.error(err.message || String(err));
  process.exit(1);
}
