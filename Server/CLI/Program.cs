using System;
using System.Linq;
using System.Threading.Tasks;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Orchestration;

namespace CK3BeagleServer.CLI
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var program = new Program();
            await program.Go();
        }

        private static string VanillaPath = @"C:\Program Files (x86)\Steam\steamapps\common\Crusader Kings III\game";
        private static string ModdedPath = @"C:\Users\Harm\Documents\Paradox Interactive\Crusader Kings III\mod\T4N-CK3\T4N";
        private static string LogsFolder = @"C:\Users\Harm\Documents\Paradox Interactive\Crusader Kings III\logs";

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
            var orchestrator = new ProcessOrchestrator(posProgressDelegate, negProgressDelegate);
            orchestrator.InitiateFromMinimalConfig(VanillaPath, ModdedPath, LogsFolder);
            GlobalResources.Configuration.ReadVanilla = true;
            GlobalResources.Configuration.VanillaFileHandling = VanillaFileHandling.AnalyseModsAdditions;

            var logs = await orchestrator.HandleAnalysis(true);

            Console.WriteLine($"Found {logs.Count()} issues");
       }
    }
}
