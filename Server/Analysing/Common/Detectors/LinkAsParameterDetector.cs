using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Generated;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources.DetectorSettings;
using System.Collections.Generic;
using System.Linq;
using CK3BeagleServer.Core.Resources;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class LinkAsParameterDetector : BaseDetector
    {
        private readonly LinkAsParameterSettings _settings;

        public LinkAsParameterDetector(ILogger logger, Context context, LinkAsParameterSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        private HashSet<(string, string)> _unremediedMacroParameters = [];
        private HashSet<(string, string)> _remediedMacroParameters = [];

        public override void EnterNamedBlock(NamedBlock namedBlock)
        {
            if (namedBlock.NodeType == NodeType.NonStatement)
                return;

            var binExpChildren = namedBlock.Children.OfType<BinaryExpression>();
            if (!binExpChildren.Any())
                return;

            var parametersWithScopeLinks = binExpChildren.Where(x => 
                GlobalResources.EVENTTARGETS.Contains(x.Value) 
                && x.Value.ToLower() != "root"
                && x.Value.ToLower() != "yes"
                && x.Value.ToLower() != "no"
                );
            if (!parametersWithScopeLinks.Any())
                return;

            var macroName = namedBlock.Key;
            Declaration macro = null;

            if (!(context.Declarations[(int)DeclarationType.ScriptedEffect].TryGetValue(macroName, out macro)
                || context.Declarations[(int)DeclarationType.ScriptedTrigger].TryGetValue(macroName, out macro)))
            {
                return;
            }

            foreach (var parameter in parametersWithScopeLinks)
            {
                bool isRemedied = IsRemedied(macro, parameter.Key);

                if (!isRemedied)
                {
                    var message = "";
                    switch (macro.DeclarationType)
                    {
                        case DeclarationType.ScriptedEffect:
                            message = "Passed link as scripted effect parameter, and the effect does not immediately save it";
                            break;
                        case DeclarationType.ScriptedTrigger:
                            message = "Passed link as scripted trigger parameter, and the trigger does not immediately save it";
                            break;
                    }

                    var logEntry = new LogEntry(Smell.LinkAsParameter, _settings.Severity,
                        message,
                        parameter);
                    var secondaryEntry = LogEntry.MinimalLogEntry(
                        macro.DeclarationType == DeclarationType.ScriptedTrigger 
                            ? "Scripted trigger" 
                            : "Scripted effect",
                        macro);
                    logger.Log(logEntry, secondaryEntry);
                }
            }
        }

        private bool IsRemedied(Declaration macro, string parameter)
        {
            if (_unremediedMacroParameters.Contains((macro.Key, parameter)))
                return false;

            if (_remediedMacroParameters.Contains((macro.Key, parameter)))
                return true;

            var visitor = new ParameterUsageVisitor
            {
                Parameter = "$" + parameter + "$"
            };
            macro.Accept(visitor);

            if (visitor.ProblemFound)
            {
                _unremediedMacroParameters.Add((macro.Key, parameter));
                return false;
            }
            _remediedMacroParameters.Add((macro.Key, parameter));
            return true;
        }

        //For the passed paramater to be remedied, either:
        //- That parameter is only used by saving it as a scope/variable, immediately
        //- That parameter is only used concatenated with other terms
        class ParameterUsageVisitor : BaseDomainVisitor
        {
            public string Parameter { get; set; }
            public bool ProblemFound { get; set; } = false;

            public override void Visit(NamedBlock namedBlock)
            {
                if (namedBlock.Key == Parameter)
                {
                    // $PARAMETER$ = { ...
                    // Only acceptable if the FIRST action inside is saving it as a scope
                    var firstChild = namedBlock.Children.FirstOrDefault(x => x.NodeType != NodeType.NonStatement);
                    if (firstChild == null || firstChild is not BinaryExpression firstBinExp ||
                        !(firstBinExp.Key == "save_scope_as" || firstBinExp.Key == "save_temporary_scope_as"))
                    {
                        ProblemFound = true;
                    }
                }
                base.Visit(namedBlock);
            }

            public override void Visit(BinaryExpression binaryExpression)
            {
                //Bin exps where key or value are exactly equal to the parameter (not concatenated with anything) are a problem
                if (binaryExpression.Key == Parameter || binaryExpression.Value == Parameter)
                {
                    ProblemFound = true;
                }
                base.Visit(binaryExpression);
            }
        }
    }
}
