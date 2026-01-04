using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Orchestration;
using System.Text.Json;

namespace CLInterface
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var program = new Program();
            await program.Go();
        }

        private async Task Go()
        {
            Task posProgressDelegate(string msg)
            {
                Console.WriteLine("Info: " + msg);
                return Task.CompletedTask;
            }
            Task negProgressDelegate(string msg)
            {
                Console.WriteLine("ERROR: " + msg);
                return Task.CompletedTask;
            }
            IEnumerable<LogEntry> logs = [];

            var orchestrator = new ProcessOrchestrator(posProgressDelegate, negProgressDelegate);

            bool success = TryFindInput(out JsonElement input, out string message);
            if (!success)
            {
                await negProgressDelegate(message);
                return;
            }

            try
            {
                await orchestrator.InitiateFromJson(input);
                logs = await orchestrator.HandleAnalysis();
                await posProgressDelegate($"Found {logs.Count()} issues!");
            }
            catch (Exception ex) { 
                await negProgressDelegate(ex.Message);
                return;
            }

            success = WriteLogsToFile(logs, out message);
            if (!success)
            {
                await negProgressDelegate(message);
                return;
            }
        }

        private const string _inputFileName = "input.txt"; 
        private bool TryFindInput(out JsonElement input, out string message)
        {
            message = ""; input = new JsonElement();
            var inputFileLocation = Path.Combine(AppContext.BaseDirectory, _inputFileName);
            if (!File.Exists(inputFileLocation))
            {
                message = "input.txt not found! Expected its location in " + inputFileLocation;
                return false;
            }
            try
            {
                input = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(inputFileLocation));
            }
            catch (Exception ex) { 
                message = "Issue deserializing input.txt; " + ex.Message;
                return false;
            }
            return true;
        }

        private const string _outputFileName = "output.txt";
        private bool WriteLogsToFile(IEnumerable<LogEntry> logs, out string message)
        {
            message = "";
            var outputFileLocation = Path.Combine(AppContext.BaseDirectory, _outputFileName);
            try
            {
                var stream = File.OpenWrite(outputFileLocation);
                using StreamWriter sw = new StreamWriter(stream);
                foreach (LogEntry logEntry in logs)
                {
                    sw.WriteLine(logEntry.Print());
                }
            }
            catch (Exception ex)
            {
                message = "Issue writing output; " + ex.Message;
                return false;
            }

            return true;
        }
    }
}
