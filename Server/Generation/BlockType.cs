namespace CK3Analyser.Generation
{
    public enum BlockType
    {
        Trigger,
        Effect,
        Other
    }

    public static class BlockTypeExtensions
    {
        public static string GetName(this BlockType blockType)
        {
            switch (blockType)
            {
                case BlockType.Trigger:
                    return "BlockType.Trigger";
                case BlockType.Effect:
                    return "BlockType.Effect";
                default:
                    return "";
            }
        }
    }
}
