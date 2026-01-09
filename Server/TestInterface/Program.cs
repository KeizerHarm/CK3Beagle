using System;
using System.Linq;
using System.Threading.Tasks;
using CK3BeagleServer.Orchestration;

namespace CK3BeagleServer.TestInterface
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var program = new Program();
            await program.Go();
        }

        private static string VanillaPath = @"C:\Program Files (x86)\Steam\steamapps\common\Crusader Kings III\game";
        //private static string ModdedPath = @"C:\Users\Harm\Documents\Paradox Interactive\Crusader Kings III\mod\TestUAMod";
        private static string ModdedPath = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\1158310\2962333032";
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

            var logs = await orchestrator.HandleAnalysis();

            Console.WriteLine($"Found {logs.Count()} issues");
            var uaErrors = logs.Where(x => x.Smell == Analysing.Smell.UnencapsulatedAddition).ToArray();
            Console.WriteLine($"Found {uaErrors.Count()} UA issues");
        }
    }
}
