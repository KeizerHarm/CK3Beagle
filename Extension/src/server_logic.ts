import * as vscode from 'vscode';
import { spawn } from 'child_process';
import * as path from 'path';
import { ChildProcess } from 'child_process';
import * as fs from 'fs';

export var proc: ChildProcess;
export let readBuffer = Buffer.alloc(0);


export async function startServer(context: vscode.ExtensionContext) {
  try {
    vscode.window.showInformationMessage('Attempting to start server');
    let serverPath = getServerExe(context);
    createServerProcess(serverPath);
    vscode.window.showInformationMessage('Started server');
  }
  catch (err) {
    vscode.window.showInformationMessage('Error starting server!');
    console.error('Error:', err);
    proc.stdin?.end();

    await new Promise((resolve) => proc.on('exit', resolve));
    console.log('Service stopped.');
    process.exit(1);
  }
}

export async function shutDownServer() {
  sendMessage({ command: 'exit' });
  const exitResponse = await readMessage();
  console.log('Exit response:', exitResponse);
  proc.stdin?.end();

  await new Promise((resolve) => proc.on('exit', resolve));
  console.log('Service stopped.');
}

export function sendMessage(obj: any) {
  const json = JSON.stringify(obj);
  const content = Buffer.from(json, 'utf8');
  const header = Buffer.from(`Content-Length: ${content.length}\r\n\r\n`, 'ascii');
  proc.stdin?.write(header);
  proc.stdin?.write(content);
}

function getServerExe(context: vscode.ExtensionContext) {
  var serverFile = '';
  if (process.platform === 'win32'){
    serverFile = 'beagle_windows.exe';
  }
  else if (process.platform === 'linux'){
    serverFile = 'beagle_linux';
  } else {
    throw new Error(`Unsupported platform: ${process.platform}`);
  }
  const serverPath = context.asAbsolutePath(path.join('server', serverFile));

  if (process.platform === 'linux'){
    fs.chmodSync(serverPath, 0o755);
  }
  return serverPath;
}

function createServerProcess(serverPath: string) {
  proc = spawn(serverPath, {
    stdio: ['pipe', 'pipe', 'inherit'],
    windowsHide: true
  });

  proc.stderr?.on('data', (data) => {
    console.error('[server err]', data.toString());
  });

  proc.on('exit', (code) => {
    console.log(`Server exited with code ${code}`);
  });

  proc.stdout.on('data', handleServerData);

  return proc;
}

const LARGE_MESSAGE_THRESHOLD = 50_000;
async function handleServerData(chunk: Buffer) {
  readBuffer = Buffer.concat([readBuffer, chunk]);
  let messages: any[] = [];

  while (true) {
    const headerEnd = readBuffer.indexOf('\r\n\r\n');
    if (headerEnd === -1) {
      break;
    }

    const header = readBuffer.subarray(0, headerEnd).toString('ascii');
    const match = header.match(/Content-Length:\s*(\d+)/i);
    if (!match) {
      break;
    }

    const contentLength = parseInt(match[1], 10);
    const bodyStart = headerEnd + 4;
    const totalLength = bodyStart + contentLength;

    if (readBuffer.length < totalLength) {
      break;
    }


    const body = readBuffer.subarray(bodyStart, totalLength);
    readBuffer = readBuffer.subarray(totalLength);


    try {
      const message = JSON.parse(body.toString('utf8'));
      messages.push(message);
    } catch (err) {
      console.error('Error parsing message:', err);
      console.error(body.toString('utf8'));
    }
    if (contentLength > LARGE_MESSAGE_THRESHOLD) {
      await new Promise(r => setTimeout(r, 1000));
    }
  }

  for (const msg of messages) {
    pendingResolvers.shift()?.(msg);
  }
}
const pendingResolvers: ((value: any) => void)[] = [];

export function readMessage(): Promise<any> {
  return new Promise((resolve) => {
    pendingResolvers.push(resolve);
  });
}