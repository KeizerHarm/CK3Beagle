using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Core
{
    public interface ICk3Parser
    {
        public ScriptFile ParseFile(string path, Context context, DeclarationType expectedDeclarationType);
        ScriptFile ParseText(string input, string relativePath, Context context, DeclarationType expectedDeclarationType);
    }
}
