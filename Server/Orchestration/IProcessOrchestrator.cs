using CK3Analyser.Analysing.Logging;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CK3Analyser.Orchestration
{
    public interface IProcessOrchestrator
    {
        Task<bool> InitiateFromJson(JsonElement jsonElement);
        void InitiateFromMinimalConfig(string vanillaPath, string modPath, string logsPath);
        Task<IEnumerable<LogEntry>> HandleAnalysis(bool reportTiming = false);
        Task<IEnumerable<LogEntry>> HandleComparativeAnalysis(bool reportTiming = false);
        void WrapUp();
    }
}