using CK3BeagleServer.Core.Generated;

namespace CK3BeagleServer.Core.Domain
{
    public static class DeclarationTypeExtensions
    {
        public static bool IsMacroType(this DeclarationType declarationType)
        {
            return declarationType switch
            {
                DeclarationType.ScriptedEffect => true,
                DeclarationType.ScriptedTrigger => true,
                _ => false
            };
        }
    }
}
