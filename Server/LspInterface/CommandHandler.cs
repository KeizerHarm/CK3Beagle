using CK3Analyser.Analysis;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Parsing;
using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Resources;
using System;
using System.IO;
using System.Linq;
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
                    await _program.SendMessageAsync(_program.GetErrorMessage(ex.Message));
                }
            }
            else
            {
                await _program.SendMessageAsync(_program.GetErrorMessage("unknown command"));
            }
        }



        private async Task Analyse()
        {
            ParsingService.ParseAllEntities(() => new AntlrParser(), GlobalResources.Modded);
            await _program.SendMessageAsync(_program.GetBasicMessage("Parsing complete!"));

            async void progressDelegate(string msg)
            {
                await _program.SendMessageAsync(_program.GetBasicMessage(msg));
            }

            var analyser = new Analyser();
            analyser.Analyse(GlobalResources.Modded, progressDelegate);


            var response = new
            {
                type = "analysis",
                payload = new
                {
                    summary = $"Found {analyser.LogEntries.Count()} issues",
                    smells = analyser.LogEntries.Select(x =>
                        new
                        {
                            severity = x.Severity,
                            file = x.Location,
                            startLine = x.AffectedAreaStartLine,
                            endLine = x.AffectedAreaEndLine,
                            startIndex = x.AffectedAreaStartIndex,
                            endIndex = x.AffectedAreaEndIndex,
                            message = x.Message,
                            key = x.Smell.GetCode()
                        }
                    )
                }
            };

            await _program.SendMessageAsync( response );
        }

        private bool HandleSettings(JsonElement jsonElement, out string response)
        {
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


            response = "Settings loaded succesfully\n\n";
            response += "Large Unit Settings : File Max Size = " + GlobalResources.Configuration.LargeUnitSettings.File_MaxSize;
            return true;
        }
    }
}
