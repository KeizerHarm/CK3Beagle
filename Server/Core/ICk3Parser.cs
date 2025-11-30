using CK3BeagleServer.Core.Domain.Entities;

namespace CK3BeagleServer.Core
{
    public interface ICk3Parser
    {
        public void ParseFile(ScriptFile file);
        //ScriptFile ParseText(string input, string relativePath, Context context, DeclarationType expectedDeclarationType);
    }
}
