import vscode = require('vscode');
import { ServerResponse, Smell, RelatedSmell } from './messages';
import { startServer, shutDownServer, readMessage, sendMessage } from './server_logic';
import { TextDocument, Uri } from 'vscode';

export async function activate(context: vscode.ExtensionContext) {
  let { workspace } = require("vscode");
  context.subscriptions.push(vscode.commands.registerCommand('ck3_beagle.ping', ping));
  context.subscriptions.push(vscode.commands.registerCommand('ck3_beagle.analyse', () => analyse()));
  context.subscriptions.push(vscode.commands.registerCommand('ck3_beagle.partial_analyse', () => partial_analyse()));
  context.subscriptions.push(workspace.onDidSaveTextDocument((e: TextDocument) => handleFileSave(e)));
  savedFiles = [];
  await startServer(context);
}

export async function deactivate(): Promise<void> {
  await shutDownServer();
}

async function ping() {
  sendMessage({ command: 'ping' });
  const response = (await readMessage()) as ServerResponse;
  if (response.type === 'error') {
    vscode.window.showErrorMessage(response.payload.message);
  } else if (response.type === 'basic') {
    vscode.window.showInformationMessage(response.payload.message);
  }
}

var savedFiles: Uri[];
function handleFileSave(e: TextDocument){
  if (e.fileName.endsWith('.txt')){
    savedFiles.push(e.uri);
  }
}

function getSettings() {
  const configuration = vscode.workspace.getConfiguration('ck3_beagle');
  const otherData = { environmentPath: vscode.workspace.workspaceFolders[0].uri.fsPath };
  const allSettings = { ...configuration, ...otherData };
  return allSettings;
}

async function partial_analyse(){
  if (savedFiles.length === 0){
    vscode.window.showInformationMessage('You triggered a partial analysis but there\'s no edited files - run a full analysis instead!');
    return;
  }

  const diagnosticCollection = getDiagnosticCollection();
  savedFiles.forEach(file => {
    diagnosticCollection.set(file, []);
  });

  var filePaths : string[];
  filePaths = savedFiles.map(f => f.fsPath);
  var settings = getSettings();

  var payload = {
    editedFiles: filePaths,
    ...settings
  };
  var message = { command: 'partial_analyse', payload };
  await analyse(message);
  savedFiles = [];
}

async function analyse(message: any = null){
  if (!message){
    var settings = getSettings();
    message = { command: 'analyse', payload: settings };
  }
  sendMessage(message);

  while (true) {
    const response = (await readMessage()) as ServerResponse;
    if (response.type === 'error') {
      vscode.window.showErrorMessage(response.payload.message);
      return;
    }

    if (response.type === 'basic') {
      vscode.window.showInformationMessage(response.payload.message);
      continue;
    }

    if (response.type === 'analysis_initial') {
      vscode.window.showInformationMessage(response.payload.message);
      continue;
    }
    if (response.type === 'analysis_median') {
      console.log('Got chunk!');
      processSmells(response.payload.smells);
    }
    if (response.type === 'analysis_final') {
      vscode.window.showInformationMessage(response.payload.message);
      return;
    }

    if (response.type === 'analysis') {
      vscode.window.showInformationMessage(response.payload.summary);
      processSmells(response.payload.smells);
      return;
    }
  }
}

let globalDiagnosticCollection: vscode.DiagnosticCollection;

function processSmells(smells: Smell[]) {
  const diagnosticsByFile = new Map<string, vscode.Diagnostic[]>();
  smells.forEach(smell => {
    smellToDiagnostic(smell, diagnosticsByFile);
  });

  const diagnosticCollection = getDiagnosticCollection();
  for (const [filePath, diagnostics] of diagnosticsByFile) {
    const fileUri = vscode.Uri.file(filePath);
    diagnosticCollection.set(fileUri, diagnostics);
  }
}

export function initDiagnosticCollection(): vscode.DiagnosticCollection {
  globalDiagnosticCollection =
    vscode.languages.createDiagnosticCollection("ck3beagle");

  return globalDiagnosticCollection;
}

export function getDiagnosticCollection(): vscode.DiagnosticCollection {
  if (!globalDiagnosticCollection) {
    return initDiagnosticCollection();
  }

  return globalDiagnosticCollection;
}

function smellToDiagnostic(
  smell: Smell,
  diagnosticsByFile: Map<string, vscode.Diagnostic[]>
): void {
  const primaryDiagnostic = createDiagnostic(smell);
  
  // Add the primary diagnostic to the collection - get existing array or create a new one
  const primaryDiagnostics = diagnosticsByFile.get(smell.file) || [];
  primaryDiagnostics.push(primaryDiagnostic);
  diagnosticsByFile.set(smell.file, primaryDiagnostics);
}

const severityMap: Record<number, vscode.DiagnosticSeverity> = {
  0: vscode.DiagnosticSeverity.Hint,
  1: vscode.DiagnosticSeverity.Information,
  2: vscode.DiagnosticSeverity.Warning,
  3: vscode.DiagnosticSeverity.Error
};

function createDiagnostic(
  smell: Smell
): vscode.Diagnostic {
  const range = createRange(smell);

  // add tip info if exists

  // Get severity from the mapping
  const severity = severityMap[smell.severity];

  // Create a diagnostic for the current problem
  const diagnostic = new vscode.Diagnostic(range, smell.message, severity);
  diagnostic.source = "ck3beagle";
  diagnostic.code = {
    value: smell.code.value,
    target: vscode.Uri.parse(smell.code.target)
  };

  // Get related entries
  var relatedEntries : vscode.DiagnosticRelatedInformation[] = [];

  smell.relatedLogEntries.forEach(element => {
    relatedEntries.push( {
      message: element.message,
      location: relatedLogLocation(element)
    } as vscode.DiagnosticRelatedInformation);
  });
  diagnostic.relatedInformation = relatedEntries;

  return diagnostic;
} 

function createRange(smell: Smell | RelatedSmell): vscode.Range {
  return new vscode.Range(smell.startLine, smell.startIndex, smell.endLine, smell.endIndex);
}

function relatedLogLocation(element: RelatedSmell) : vscode.Location {
  var fileUri = vscode.Uri.file(element.file);
  var range = createRange(element);
  return new vscode.Location(fileUri, range);
}