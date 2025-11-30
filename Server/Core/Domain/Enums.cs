namespace CK3BeagleServer.Core.Domain
{
    /// <summary>
    /// Refers to what a block provides for its children; trigger, effect, or none. 
    /// 'limit' is Trigger Context (but not a Trigger Node)
    /// </summary>
    public enum BlockContext
    {
        None,
        Trigger,
        Effect
    }

    /// <summary>
    /// Refers to what a node itself is. Links are either effects or triggers depending on context.
    /// 'create_character' is an effect node, but not an effect context (it cannot have effect children).
    /// </summary>
    public enum NodeType
    {
        NonStatement,
        Effect,
        Trigger
    }

    /// <summary>
    /// ('=' | '?=' | '<' | '<=' | '>' | '>=' | '!=' | '==')
    /// </summary>
    public enum Scoper
    {
        /// <summary>
        /// =
        /// </summary>
        Equal,
        /// <summary>
        /// ?=
        /// </summary>
        ConditionalEqual,
        /// <summary>
        /// &lt;
        /// </summary>
        LessThan,
        /// <summary>
        /// &lt;=
        /// </summary>
        LessThanOrEqualTo,
        /// <summary>
        /// &gt;
        /// </summary>
        GreaterThan,
        /// <summary>
        /// &gt;=
        /// </summary>
        GreaterThanOrEqualTo,
        /// <summary>
        /// !=
        /// </summary>
        NotEqual,
        /// <summary>
        /// ==
        /// </summary>
        StrictEquals
    }

    public static class ScoperExtensions
    {
        public static string ScoperToString(this Scoper scoper)
        {
            return scoper switch
            {
                Scoper.Equal => "=",
                Scoper.ConditionalEqual => "?=",
                Scoper.LessThan => "<",
                Scoper.LessThanOrEqualTo => "<=",
                Scoper.GreaterThan => ">",
                Scoper.GreaterThanOrEqualTo => ">=",
                Scoper.NotEqual => "!=",
                Scoper.StrictEquals => "==",
                _ => throw new System.Exception("What??"),
            };
        }

        public static Scoper StringToScoper(this string scoper)
        {
            return scoper switch
            {
                "=" => Scoper.Equal,
                "?=" => Scoper.ConditionalEqual,
                "<" => Scoper.LessThan,
                "<=" => Scoper.LessThanOrEqualTo,
                ">" => Scoper.GreaterThan,
                ">=" => Scoper.GreaterThanOrEqualTo,
                "!=" => Scoper.NotEqual,
                "==" => Scoper.StrictEquals,
                _ => throw new System.Exception("What??")
            };
        }
    }
}
