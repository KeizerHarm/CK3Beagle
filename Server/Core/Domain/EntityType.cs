namespace CK3Analyser.Core.Domain
{
    public enum EntityType
    {
        ScriptedEffect,
        ScriptedTrigger,
        Root
    }

    public static class EntityTypeExtensions
    {
        public static string GetEntityHome(this EntityType entityType)
        {
            return entityType switch
            {
                EntityType.ScriptedEffect => "common/scripted_effects",
                EntityType.ScriptedTrigger => "common/scripted_triggers",
                EntityType.Root => "test",
                _ => "",
            };
        }
    }
}
