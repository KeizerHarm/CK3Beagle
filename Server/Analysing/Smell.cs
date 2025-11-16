using System;

namespace CK3Analyser.Analysis
{
    public enum Smell
    {
        None,

        InconsistentIndentation_UnexpectedType,
        InconsistentIndentation_Inconsistency,

        LargeUnit_File,
        LargeUnit_NonMacroBlock,
        LargeUnit_Macro,

        OvercomplicatedBoolean_Associativity,
        OvercomplicatedBoolean_Idempotency,
        OvercomplicatedBoolean_Complementation,
        OvercomplicatedBoolean_DoubleNegation,
        OvercomplicatedBoolean_Distributivity,
        OvercomplicatedBoolean_Absorption,

        NotIsNotNor,

        Duplication,

        HiddenDependencies_UseOfRoot,
        HiddenDependencies_UseOfPrev,
        HiddenDependencies_UseOfSavedScope,
        HiddenDependencies_UseOfVariable,

        MagicNumber,

        KeywordAsScopeName_RootPrev,
        KeywordAsScopeName_ScopeLink,
        KeywordAsScopeName_ScopeType
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

                case Smell.OvercomplicatedBoolean_Associativity:
                    return "OB.1";
                case Smell.OvercomplicatedBoolean_Idempotency:
                    return "OB.2";
                case Smell.OvercomplicatedBoolean_Complementation:
                    return "OB.3";
                case Smell.OvercomplicatedBoolean_DoubleNegation:
                    return "OB.4";
                case Smell.OvercomplicatedBoolean_Distributivity:
                    return "OB.5";
                case Smell.OvercomplicatedBoolean_Absorption:
                    return "OB.6";

                case Smell.NotIsNotNor:
                    return "NNR.1";

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

                case Smell.KeywordAsScopeName_RootPrev:
                    return "KASN.1";
                case Smell.KeywordAsScopeName_ScopeLink:
                    return "KASN.2";
                case Smell.KeywordAsScopeName_ScopeType:
                    return "KASN.3";

                default:
                    throw new ArgumentException("What is that smell, that smelly smell...?");
            }
        }
    }
}