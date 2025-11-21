using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.Semantics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CK3Analyser.LspInterface
{
    public class CommandHandler
    {
        private readonly Program _program;
        private readonly ProcessOrchestrator _orchestrator;

        public CommandHandler(Program program)
        {
            _program = program;
            _orchestrator = new ProcessOrchestrator();
        }

        public async Task HandleCommand(string commandType, JsonElement payload)
        {
            if (commandType == "ping")
            {
                await _program.SendMessageAsync(_program.GetBasicMessage("pong"));
            }
            else if (commandType == "settings")
            {
                bool success = HandleSettings(payload, out string settingsResponse);
                if (success)
                {
                    await _program.SendMessageAsync(_program.GetBasicMessage(settingsResponse));
                }
                else
                {
                    await _program.SendMessageAsync(_program.GetErrorMessage(settingsResponse));
                }
            }
            else if (commandType == "analyse")
            {
                try
                {
                    await Analyse();
                }
                catch (Exception ex)
                {
                    await _program.SendMessageAsync(_program.GetErrorMessage(ex.ToString()));
                }
            }
            else
            {
                await _program.SendMessageAsync(_program.GetErrorMessage("unknown command"));
            }
        }

        private async Task Analyse()
        {
            async Task progressDelegate(string msg)
            {
                await _program.SendMessageAsync(_program.GetBasicMessage(msg));
            }

            var logEntries = await _orchestrator.HandleAnalysis(progressDelegate);

            //Send all in one go
            if (logEntries.Count() < 2000)
            {
                var response = new
                {
                    type = "analysis",
                    payload = new
                    {
                        summary = $"Found {logEntries.Where(x => x.Severity > Severity.Debug).Count()} issues",
                        smells = logEntries.Select(LogEntryToReport)
                    }
                };
                await _program.SendMessageAsync(response);
            }
            else //Chunks
            {
                var response = new
                {
                    type = "analysis_initial",
                    payload = new
                    {
                        message = $"Found {logEntries.Where(x => x.Severity > Severity.Debug).Count()} issues, transmitting in chunks"
                    }
                };
                await _program.SendMessageAsync(response);

                const int MAX_CHUNK_BYTES = 1_000_000;
                var currentChunk = new List<object>();
                int currentSize = 0;

                foreach (var entry in logEntries)
                {
                    var report = LogEntryToReport(entry);
                    var serializedEntry = JsonSerializer.Serialize(report);
                    int entrySize = Encoding.UTF8.GetByteCount(serializedEntry);

                    if (currentSize + entrySize > MAX_CHUNK_BYTES)
                    {
                        await _program.SendMessageAsync(
                            new { 
                                type = "analysis_median", 
                                payload = new 
                                { 
                                    smells = currentChunk.ToArray(),
                                    message = "Chunk size: " + currentSize
                                }
                            }
                        );
                        Thread.Sleep(500);
                        currentChunk.Clear();
                        currentSize = 0;
                    }

                    currentChunk.Add(report);
                    currentSize += entrySize;
                }

                if (currentChunk.Count > 0)
                {
                    await _program.SendMessageAsync(
                        new { 
                            type = "analysis_median",
                            payload = new
                            { 
                                smells = currentChunk.ToArray(),
                                message = "Last of the chunks!"
                            } 
                        }
                    );
                    Thread.Sleep(500);
                }

                var finalResponse = new
                {
                    type = "analysis_final",
                    payload = new
                    {
                        message = "All smells transmitted!"
                    }
                };
                await _program.SendMessageAsync(finalResponse);
            }

            _orchestrator.WrapUp();
        }

        private static object LogEntryToReport(LogEntry x)
        {
            return new
            {
                severity = x.Severity,
                file = x.Location,
                startLine = x.AffectedAreaStartLine,
                endLine = x.AffectedAreaEndLine,
                startIndex = x.AffectedAreaStartIndex,
                endIndex = x.AffectedAreaEndIndex,
                message = x.Message,
                key = x.Smell.GetCode(),
                relatedLogEntries = x.RelatedLogEntries.Select(y =>
                    new
                    {
                        file = y.Location,
                        startLine = y.AffectedAreaStartLine,
                        endLine = y.AffectedAreaEndLine,
                        startIndex = y.AffectedAreaStartIndex,
                        endIndex = y.AffectedAreaEndIndex,
                        message = y.Message
                    }
                )
            };
        }

        private bool HandleSettings(JsonElement jsonElement, out string response)
        {
            return _orchestrator.Initiate(jsonElement, out response);
        }
    }
}
