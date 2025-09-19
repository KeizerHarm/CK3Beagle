namespace CK3Analyser.Core.Domain
{
    public enum DeclarationType
    {
        Debug,
        ScriptedEffect,
        ScriptedTrigger,
        Event
    }

    public static class DeclarationTypeExtensions
    {
        public static string GetEntityHome(this DeclarationType declarationType)
        {
            return declarationType switch
            {
                DeclarationType.ScriptedEffect => "common/scripted_effects",
                DeclarationType.ScriptedTrigger => "common/scripted_triggers",
                DeclarationType.Event => "events",
                DeclarationType.Debug => "test",
                _ => "",
            };
        }
    }
}
