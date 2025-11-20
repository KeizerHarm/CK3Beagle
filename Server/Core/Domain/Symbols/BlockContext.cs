namespace CK3Analyser.Core.Domain.Symbols
{
    /// <summary>
    /// Refers to what a block provides for its children; trigger, effect, or none. 'limit' is Trigger Context (but not a Trigger Node)
    /// </summary>
    public enum BlockContext
    {
        None,
        Trigger,
        Effect
    }
}
