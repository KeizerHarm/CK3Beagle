using System;
using System.Collections.Generic;

namespace CK3BeagleServer.Analysing
{
    public enum Smell
    {
        None,

        InconsistentIndentation_UnexpectedType,
        InconsistentIndentation_Inconsistency,

        LargeUnit_File,
        LargeUnit_NonMacroBlock,
        LargeUnit_Macro,

        OvercomplicatedTrigger_Associativity,
        OvercomplicatedTrigger_Idempotence,
        OvercomplicatedTrigger_Contradiction,
        OvercomplicatedTrigger_DoubleNegation,
        OvercomplicatedTrigger_Distributivity,
        OvercomplicatedTrigger_Absorption,

        NotIsNotNor,

        Duplication,

        HiddenDependencies_UseOfRoot,
        HiddenDependencies_UseOfPrev,
        HiddenDependencies_UseOfSavedScope,
        HiddenDependencies_UseOfVariable,

        MagicNumber,

        KeywordAsScopeName_RootPrevThis,
        KeywordAsScopeName_ScopeLink,

        UnencapsulatedAddition,

        EntityKeyReused_SameType,
        EntityKeyReused_DifferentType,

        MisuseOfThis
    }

    public static class SmellExtensions
    {
        public static string GetCode(this Smell smell)
        {
            switch (smell)
            {
                case Smell.None:
                    return "";

                case Smell.InconsistentIndentation_UnexpectedType:
                    return "II.1";
                case Smell.InconsistentIndentation_Inconsistency:
                    return "II.2";

                case Smell.LargeUnit_File:
                    return "LU.1";
                case Smell.LargeUnit_Macro:
                    return "LU.2";
                case Smell.LargeUnit_NonMacroBlock:
                    return "LU.3";

                case Smell.OvercomplicatedTrigger_Associativity:
                    return "OT.1";
                case Smell.OvercomplicatedTrigger_Idempotence:
                    return "OT.2";
                case Smell.OvercomplicatedTrigger_Contradiction:
                    return "OT.3";
                case Smell.OvercomplicatedTrigger_DoubleNegation:
                    return "OT.4";
                case Smell.OvercomplicatedTrigger_Distributivity:
                    return "OT.5";
                case Smell.OvercomplicatedTrigger_Absorption:
                    return "OT.6";

                case Smell.NotIsNotNor:
                    return "NNN.1";

                case Smell.Duplication:
                    return "DUP.1";

                case Smell.HiddenDependencies_UseOfRoot:
                    return "HD.1";
                case Smell.HiddenDependencies_UseOfPrev:
                    return "HD.2";
                case Smell.HiddenDependencies_UseOfSavedScope:
                    return "HD.3";
                case Smell.HiddenDependencies_UseOfVariable:
                    return "HD.4";

                case Smell.MagicNumber:
                    return "MN.1";

                case Smell.KeywordAsScopeName_RootPrevThis:
                    return "KSN.1";
                case Smell.KeywordAsScopeName_ScopeLink:
                    return "KSN.2";

                case Smell.UnencapsulatedAddition:
                    return "UA.1";

                case Smell.EntityKeyReused_SameType:
                    return "EKR.1";
                case Smell.EntityKeyReused_DifferentType:
                    return "EKR.2";

                case Smell.MisuseOfThis:
                    return "MT.1";

                default:
                    throw new ArgumentException("What is that smell, that smelly smell...?");
            }
        }

        private static readonly Dictionary<Smell, string> cachedUrls = [];

        public static string GetDocumentationUrl(this Smell smell)
        {
            if (cachedUrls.TryGetValue(smell, out var url))
                return url;

            var urlPrefix = "https://github.com/KeizerHarm/CK3Beagle/blob/main/Documentation/Smells/";
            var smellNameParts = smell.ToString().Split('_');
            var smellName = smellNameParts[0];

            if (smellNameParts.Length == 1)
                url = urlPrefix + smellName + ".md";
            else
            {
                var smellCaseName = smellNameParts[1].ToKebabCase();
                var smellCode = smell.GetCode().Replace(".", "").ToLower();
                var urlFinish = "#" + smellCode + "-" + smellCaseName;

                url = urlPrefix + smellName + ".md" + urlFinish;
            }
            cachedUrls[smell] = url;
            return url;

        }
        public static string ToKebabCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                        builder.Append('-');
                    builder.Append(char.ToLower(c));
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

    }
}