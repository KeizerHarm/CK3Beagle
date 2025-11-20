//using CK3Analyser.Core.Domain.Entities;
//using CK3Analyser.Core.Resources;
//using CK3Analyser.Core.Resources.Semantics;
//using System.Collections.Generic;
//using System.Linq;

//namespace CK3Analyser.Core.Domain.Symbols
//{
//    public struct AccoladeName : ISymbol
//    {
//        public static int Construct(NamedBlock node)
//        {
//            var symbol = new AccoladeName
//            {
//                Options = GeneratedSymbolExtensions.ConstructMany<AccoladeName_Option>(node, "option", AccoladeName_Option.Construct),
//                Potential = GeneratedSymbolExtensions.Construct<AccoladeName_Potential>(node, "potential", AccoladeName_Potential.Construct)
//            };
//            node.SemanticId = GlobalResources.SymbolTable.Insert(SymbolType.AccoladeName, symbol);
//            node.SymbolType = SymbolType.AccoladeName;
//            return node.SemanticId;
//        }

//        public List<int> Options { get; set; }
//        public readonly IEnumerable<AccoladeName_Option> OptionSymbols(SymbolTable symbolTable) =>
//            Options.Select(x => (AccoladeName_Option)symbolTable.Lookup(SymbolType.AccoladeName_Option, x));

//        public int? Potential { get; set; }
//        public readonly AccoladeName_Potential PotentialSymbol(SymbolTable symbolTable) =>
//            (AccoladeName_Potential)symbolTable.Lookup(SymbolType.AccoladeName_Potential, Potential);
//    }

//    public struct AccoladeName_Option : ISymbol
//    {
//        public static int Construct(NamedBlock node)
//        {
//            var symbol = new AccoladeName_Option
//            {
//                Trigger = GeneratedSymbolExtensions.Construct<AccoladeName_Option_Trigger>(node, "trigger", AccoladeName_Option_Trigger.Construct)
//            };
//            node.SemanticId = GlobalResources.SymbolTable.Insert(SymbolType.AccoladeName_Option, symbol);
//            node.SymbolType = SymbolType.AccoladeName_Option;
//            return node.SemanticId;
//        }

//        public int? Trigger { get; set; }
//        public readonly AccoladeName_Option_Trigger TriggerSymbol(SymbolTable symbolTable) =>
//            (AccoladeName_Option_Trigger)symbolTable.Lookup(SymbolType.AccoladeName_Option_Trigger, Trigger);
//    }

//    public struct AccoladeName_Option_Trigger : ISymbol
//    {
//        public static int Construct(NamedBlock node)
//        {
//            var symbol = new AccoladeName_Option_Trigger();
//            node.SemanticId = GlobalResources.SymbolTable.Insert(SymbolType.AccoladeName_Option_Trigger, symbol);
//            node.SymbolType = SymbolType.AccoladeName_Option_Trigger;
//            return node.SemanticId;
//        }
//    }

//    public struct AccoladeName_Potential : ISymbol
//    {
//        public static int Construct(NamedBlock node)
//        {
//            var symbol = new AccoladeName_Potential();
//            node.SemanticId = GlobalResources.SymbolTable.Insert(SymbolType.AccoladeName_Potential, symbol);
//            node.SymbolType = SymbolType.AccoladeName_Potential;
//            return node.SemanticId;
//        }
//    }
//}
