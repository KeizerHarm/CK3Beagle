using CK3Analyser.Core.Domain;

namespace CK3Analyser.Core
{
    public interface ICk3Parser
    {
        public ScriptFile ParseFile(string path, Context context, EntityType expectedEntityType);
        ScriptFile ParseText(string input, string relativePath, Context context, EntityType expectedEntityType);
    }
}
