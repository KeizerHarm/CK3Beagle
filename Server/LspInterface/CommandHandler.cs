using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Parsing;
using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.Semantics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CK3Analyser.LspInterface
{
    public class CommandHandler
    {
        private readonly Program _program;

        public CommandHandler(Program program)
        {
            _program = program;
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

            await ParsingService.ParseAllEntities(() => new AntlrParser(), GlobalResources.Modded, progressDelegate);
            await _program.SendMessageAsync(_program.GetBasicMessage("Parsing complete!"));

            var analyser = new Analyser();
            await analyser.Analyse(GlobalResources.Modded, progressDelegate);

            //Send all in one go
            if (analyser.LogEntries.Count() < 2000)
            {
                var response = new
                {
                    type = "analysis",
                    payload = new
                    {
                        summary = $"Found {analyser.LogEntries.Count()} issues",
                        smells = analyser.LogEntries.Select(LogEntryToReport)
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
                        message = $"Found {analyser.LogEntries.Count()} issues, transmitting in chunks"
                    }
                };
                await _program.SendMessageAsync(response);

                const int MAX_CHUNK_BYTES = 1_000_000;
                var currentChunk = new List<object>();
                int currentSize = 0;

                foreach (var entry in analyser.LogEntries)
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
                }


                //var chunks = analyser.LogEntries.Chunk(100).ToArray();

                //var response = new
                //{
                //    type = "analysis_initial",
                //    payload = new
                //    {
                //        message = $"Found {analyser.LogEntries.Count()} issues, transmitting in chunks of 100"
                //    }
                //};
                //await _program.SendMessageAsync(response);

                //for (int i = 0; i < chunks.Length; i++)
                //{
                //    LogEntry[] chunk = chunks[i];
                //    var partialReport = new
                //    {
                //        type = "analysis_median",
                //        payload = new
                //        {
                //            message = "Chunk " + (i + 1) + " of " + chunks.Length,
                //            smells = chunk.Select(LogEntryToReport).ToArray()
                //        }
                //    };
                //    await _program.SendMessageAsync(partialReport);
                //}
                var finalResponse = new
                {
                    type = "analysis_final",
                    payload = new
                    {
                        message = "Done transmitting"
                    }
                };
                await _program.SendMessageAsync(finalResponse);
            }

            GlobalResources.ClearEverything();
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
            GlobalResources.ClearEverything();

            var vanillaPath = jsonElement.GetProperty("vanillaCk3Path").GetString();
            if (string.IsNullOrEmpty(vanillaPath))
            {
                response = "Missing VanillaPath setting";
                return false;
            }
            if (!Directory.Exists(vanillaPath))
            {
                response = "VanillaPath setting points to a non-existent directory";
                return false;
            }

            var logsFolderPath = jsonElement.GetProperty("logsFolderPath").GetString();
            if (string.IsNullOrEmpty(logsFolderPath))
            {
                response = "Missing LogsFolderPath setting";
                return false;
            }
            if (!Directory.Exists(logsFolderPath))
            {
                response = "LogsFolderPath setting points to a non-existent directory";
                return false;
            }

            var modPath = jsonElement.GetProperty("modPath").GetString();
            if (!string.IsNullOrWhiteSpace(modPath) && !Directory.Exists(modPath))
            {
                response = "ModPath setting points to a non-existent directory";
                return false;
            }

            if (string.IsNullOrWhiteSpace(modPath))
                modPath = jsonElement.GetProperty("environmentPath").GetString();

            LogsParser.ParseLogs(logsFolderPath);

            GlobalResources.Old = new Context(vanillaPath, ContextType.Old);
            GlobalResources.Modded = new Context(modPath, ContextType.Modded);

            GlobalResources.Configuration = new Configuration(jsonElement);
            GlobalResources.SymbolTable = new SymbolTable();

            response = "Settings loaded succesfully: " + GlobalResources.Configuration.ToString();
            return true;
        }
    }
}
