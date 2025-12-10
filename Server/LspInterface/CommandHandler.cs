using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Analysing;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CK3BeagleServer.LspInterface
{
    public class CommandHandler
    {
        private readonly Program _program;
        private readonly IProcessOrchestrator _orchestrator;

        async Task positiveProgressDelegate(string msg)
        {
            await _program.SendMessageAsync(_program.GetBasicMessage(msg));
        }

        async Task negativeProgressDelegate(string msg)
        {
            await _program.SendMessageAsync(_program.GetBasicMessage(msg));
        }

        public CommandHandler(Program program)
        {
            _program = program;
            _orchestrator = new ProcessOrchestrator(positiveProgressDelegate, negativeProgressDelegate);
        }

        public async Task HandleCommand(string commandType, JsonElement payload)
        {
            if (commandType == "ping")
            {
                await _program.SendMessageAsync(_program.GetBasicMessage("pong"));
            }
            else if (commandType == "settings")
            {
                try
                {
                    await _orchestrator.InitiateFromJson(payload);
                }
                catch(Exception ex)
                {
                    await _program.SendMessageAsync(_program.GetErrorMessage(ex.ToString()));
                }
            }
            else if (commandType == "analyse")
            {
                try
                {
                    var logEntries = await _orchestrator.HandleAnalysis();
                    await SendLogs(logEntries);
                    _orchestrator.WrapUp();
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

        private async Task SendLogs(IEnumerable<LogEntry> logEntries)
        {
            var summary = GetSummary(logEntries);

            //Send all in one go
            if (logEntries.Count() < 2000)
            {
                var response = new
                {
                    type = "analysis",
                    payload = new
                    {
                        summary = summary,
                        smells = logEntries.Select(LogEntryToReport)
                    }
                };
                await _program.SendMessageAsync(response);
                return;
            }

            //Chunks
            var initialResponse = new
            {
                type = "analysis_initial",
                payload = new
                {
                    message = summary + ": transmitting in chunks"
                }
            };
            await _program.SendMessageAsync(initialResponse);

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
                        new
                        {
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
                    new
                    {
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

        private string GetSummary(IEnumerable<LogEntry> logEntries)
        {
            var histogram = logEntries.GroupBy(l => l.Smell);
            var summary = "Found " + logEntries.Count() + " issues. ";
            summary += string.Join("; ", histogram.Select(x => $"{x.Key.GetCode()}: {x.Count()}"));
            return summary;
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
                code = new
                {
                    value = x.Smell.GetCode(),
                    target = x.Smell.GetDocumentationUrl()
                },
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
    }
}
