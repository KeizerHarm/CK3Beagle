using System;

namespace CK3Analyser.Analysis
{
    public enum Smell
    {
        None,
        InconsistentIndentation_UnexpectedType,
        InconclusiveIndentation_Inconsistency,
        LargeFile,
        LargeUnit,
        OvercomplicatedBoolean_Associativity,
        OvercomplicatedBoolean_Idempotency,
        OvercomplicatedBoolean_Complementation,
        OvercomplicatedBoolean_DoubleNegation,
        OvercomplicatedBoolean_Distributivity,
        OvercomplicatedBoolean_Absorption,
        NotIsNotNor
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
                case Smell.InconclusiveIndentation_Inconsistency:
                    return "II.2";
                case Smell.LargeFile:
                    return "LF.1";
                case Smell.LargeUnit:
                    return "LU.1";
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
                    return "NINR.1";
                default:
                    throw new ArgumentException();
            }
        }
    }
}
