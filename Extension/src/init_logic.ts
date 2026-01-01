import { promises as fs } from "fs";
import * as path from "path";
import vscode = require('vscode');
import { startServer } from './server_logic';

export async function initExtension(context: vscode.ExtensionContext){
  await initContext();
  await startServer(context);
}

async function initContext(){
  var workspaceRootFolders = vscode.workspace.workspaceFolders;

  const modRootFolders: string[] = (
    await Promise.all(
      workspaceRootFolders.map(async folder =>
        await findModRelatedDirectories(folder.uri.fsPath)
      )
    )
  ).flat();
  vscode.window.showInformationMessage('Detected ' + modRootFolders.length + ' mod root folders');
  vscode.commands.executeCommand('setContext', 'ck3beagle.modRootFolders', modRootFolders);
}

async function findModRelatedDirectories(
  rootDir: string
): Promise<string[]> {
  const results: string[] = [];

  // Check the root directory itself
  if (await isModRootDirectory(rootDir)) {
    results.push(rootDir);
    return results; //Mod folders can never contain more mod folders
  }

  let entries;
  try {
    entries = await fs.readdir(rootDir, { withFileTypes: true });
  } catch {
    return results;
  }

  for (const entry of entries) {
    if (!entry.isDirectory()) {
      continue;
    }

    const subDirPath = path.join(rootDir, entry.name);

    if (await isModRootDirectory(subDirPath)) {
      results.push(subDirPath);
    }
  }

  return results;
}


async function isModRootDirectory(dirPath: string): Promise<boolean> {
  const parentDirName = path.basename(path.dirname(dirPath));

  // Condition 1: parent folder is called "mod"
  if (parentDirName === "mod") {
    return true;
  }

  // Condition 2: directory contains a .mod file
  try {
    const entries = await fs.readdir(dirPath, { withFileTypes: true });

    return entries.some(
      entry =>
        entry.isFile() &&
        path.extname(entry.name).toLowerCase() === ".mod"
    );
  } catch {
    // If the directory cannot be read, treat it as not matching
    return false;
  }
}