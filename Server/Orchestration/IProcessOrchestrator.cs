using CK3BeagleServer.Analysing.Logging;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CK3BeagleServer.Orchestration
{
    public interface IProcessOrchestrator
    {
        Task<bool> InitiateFromJson(JsonElement jsonElement);
        void InitiateFromMinimalConfig(string vanillaPath, string modPath, string logsPath);
        Task<IEnumerable<LogEntry>> HandleAnalysis(bool reportTiming = false);
        void WrapUp();
    }
}