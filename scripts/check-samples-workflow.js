#!/usr/bin/env node
const fs = require('fs');
const path = require('path');

function readText(filePath) {
  return fs.readFileSync(filePath, 'utf8');
}

function readJson(filePath) {
  return JSON.parse(readText(filePath));
}

function exists(filePath) {
  return fs.existsSync(filePath);
}

function collectSampleTitles(markdown) {
  const matches = [...markdown.matchAll(/^###\s+(.+)$/gm)];
  return matches.map((m) => m[1].trim());
}

function addResult(results, ok, check, details) {
  results.push({ ok, check, details });
}

function findNodeByName(workflow, name) {
  return (workflow.nodes || []).find((n) => n.name === name);
}

function isRetrieveToolNode(node) {
  return (
    node &&
    node.type === '@n8n/n8n-nodes-langchain.vectorStorePGVector' &&
    node.parameters &&
    node.parameters.mode === 'retrieve-as-tool'
  );
}

function checkRetrievalWorkflow(workflowPath, nodeName, expectedTopKMin, results) {
  if (!exists(workflowPath)) {
    addResult(results, false, `${path.basename(workflowPath)} exists`, 'Datei nicht gefunden');
    return;
  }

  const wf = readJson(workflowPath);
  const node = findNodeByName(wf, nodeName);
  if (!node) {
    addResult(results, false, `${path.basename(workflowPath)} has ${nodeName}`, 'Node fehlt');
    return;
  }

  addResult(results, true, `${path.basename(workflowPath)} has ${nodeName}`, 'Node vorhanden');

  const retrieveOk = isRetrieveToolNode(node);
  addResult(
    results,
    retrieveOk,
    `${path.basename(workflowPath)} ${nodeName} retrieve-as-tool`,
    retrieveOk ? 'Mode und Typ korrekt' : `type=${node.type}, mode=${node.parameters?.mode}`
  );

  const tableName = node.parameters?.tableName;
  const tableOk = tableName === 'fpc_doc_vectors';
  addResult(
    results,
    tableOk,
    `${path.basename(workflowPath)} uses fpc_doc_vectors`,
    `tableName=${tableName || '(leer)'}`
  );

  const topK = Number(node.parameters?.topK || 0);
  const topKOk = topK >= expectedTopKMin;
  addResult(
    results,
    topKOk,
    `${path.basename(workflowPath)} topK >= ${expectedTopKMin}`,
    `topK=${topK}`
  );
}

function checkIngestionWorkflow(workflowPath, results) {
  if (!exists(workflowPath)) {
    addResult(results, false, `${path.basename(workflowPath)} exists`, 'Datei nicht gefunden');
    return;
  }

  const wf = readJson(workflowPath);
  addResult(results, true, `${path.basename(workflowPath)} exists`, `active=${wf.active === true ? 'true' : 'false'}`);

  const asText = JSON.stringify(wf);
  const targetsVectors = asText.includes('fpc_doc_vectors');
  addResult(
    results,
    targetsVectors,
    `${path.basename(workflowPath)} targets fpc_doc_vectors`,
    targetsVectors ? 'ok' : 'kein Zieltable gefunden'
  );

  if (path.basename(workflowPath) === 'FPCUploadToVector.json') {
    const referencesSampleFile = asText.includes('FPCSamplesV2') || asText.includes('FPCSamples.md');
    addResult(
      results,
      referencesSampleFile,
      'FPCUploadToVector sample split hint',
      referencesSampleFile ? 'Hinweis auf Sample-Datei vorhanden' : 'kein Hinweis auf Sample-Datei'
    );
  }
}

function main() {
  const repoRoot = process.cwd();
  const files = {
    samples: path.join(repoRoot, 'FPCSamples.md'),
    selfLearning: path.join(repoRoot, 'FPCSelfLearning.json'),
    multiAgent: path.join(repoRoot, 'FPCMultiAgentPipeline.json'),
    uploadToVector: path.join(repoRoot, 'FPCUploadToVector.json'),
    vectorIngestion: path.join(repoRoot, 'FPCVectorStoreIngestion.json'),
    sourceToVector: path.join(repoRoot, 'FPCSourceToVector.json'),
  };

  const results = [];

  if (!exists(files.samples)) {
    addResult(results, false, 'FPCSamples.md exists', 'Datei fehlt');
  } else {
    const samplesMd = readText(files.samples);
    const titles = collectSampleTitles(samplesMd);
    addResult(results, titles.length > 0, 'FPCSamples.md has sample sections', `count=${titles.length}`);
    addResult(results, titles.length >= 10, 'FPCSamples.md has enough curated examples', `count=${titles.length}`);
  }

  checkRetrievalWorkflow(files.selfLearning, 'FPCKnowledge', 3, results);
  checkRetrievalWorkflow(files.multiAgent, 'FPCKnowledge', 5, results);

  checkIngestionWorkflow(files.uploadToVector, results);
  checkIngestionWorkflow(files.vectorIngestion, results);
  checkIngestionWorkflow(files.sourceToVector, results);

  const warnings = [];

  try {
    const wf = readJson(files.selfLearning);
    if (wf.active !== true) {
      warnings.push('FPCSelfLearning.json ist nicht active=true (nur relevant, falls n8n den gespeicherten Stand direkt nutzt).');
    }
  } catch (e) {
    warnings.push(`FPCSelfLearning.json konnte nicht gelesen werden: ${e.message}`);
  }

  try {
    const upload = readJson(files.uploadToVector);
    if (upload.active !== true) {
      warnings.push('FPCUploadToVector.json ist active=false. Neue Samples werden ohne manuellen Start nicht automatisch ingestiert.');
    }
  } catch (e) {
    warnings.push(`FPCUploadToVector.json konnte nicht gelesen werden: ${e.message}`);
  }

  const failed = results.filter((r) => !r.ok);
  const passed = results.filter((r) => r.ok);

  console.log('=== Samples/Workflow Consistency Check ===');
  console.log(`PASS: ${passed.length}`);
  console.log(`FAIL: ${failed.length}`);
  console.log('');

  for (const r of results) {
    console.log(`- ${r.ok ? 'OK' : 'FAIL'}: ${r.check} | ${r.details}`);
  }

  if (warnings.length > 0) {
    console.log('');
    console.log('Warnings:');
    for (const w of warnings) {
      console.log(`- ${w}`);
    }
  }

  if (failed.length > 0) {
    process.exit(2);
  }
}

main();
