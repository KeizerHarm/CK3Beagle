using CK3Analyser.Analysing.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Generated;
using CK3Analyser.Core.Resources.DetectorSettings;
using System.Collections.Generic;
using System.Text;

namespace CK3Analyser.Analysing.Common.Detectors
{
    public class HiddenDependenciesDetector : BaseDetector
    {
        private readonly HiddenDependenciesSettings _settings;

        private Declaration thisDeclaration;
        private bool usedRoot = false;
        private bool usedPrev = false;
        private List<Node> rootUsages = [];
        private List<Node> prevUsages = [];
        private HashSet<string> usedSavedScopes = [];
        private HashSet<string> setSavedScopes = [];
        private HashSet<string> usedVariables = [];
        private HashSet<string> setVariables = [];

        private string _usedRootMsg;
        private string GetUsedRootMsg()
        {
            _usedRootMsg ??= "'root' is used, making this macro dependent on where it is invoked."
                + GetGenericMsg(_settings.UseOfRoot_AllowedIfInComment, _settings.UseOfRoot_AllowedIfInName);
            return _usedRootMsg;
        }
        private string _usedPrevMsg;
        private string GetUsedPrevMsg()
        {
            _usedPrevMsg ??= "'prev' is used at the top level, making this macro dependent on where it is invoked." 
                + GetGenericMsg(_settings.UseOfPrev_AllowedIfInComment, _settings.UseOfPrev_AllowedIfInName);
            return _usedPrevMsg;
        }
        private string _usedSavedScopeMsg;
        private string GetUsedSavedScopeMsg()
        {
            _usedSavedScopeMsg ??= "A saved scope is used, making this macro dependent on from where it is invoked."
                + GetGenericMsg(_settings.UseOfPrev_AllowedIfInComment, _settings.UseOfPrev_AllowedIfInName);
            return _usedSavedScopeMsg;
        }
        private string _usedVariableMsg;
        private string GetUsedVariableMsg()
        {
            _usedVariableMsg ??= "A variable is used, making this macro dependent on from where it is invoked."
                + GetGenericMsg(_settings.UseOfPrev_AllowedIfInComment, _settings.UseOfPrev_AllowedIfInName);
            return _usedVariableMsg;
        }

        private static string GetGenericMsg(bool allowedIfInComment, bool allowedIfInName)
        {
            var msg = "";
            if (allowedIfInComment && allowedIfInName)
                msg += $" To remedy: add the keyword to the declaration name, a preceding comment, or refactor it to a parameter.";

            else if (allowedIfInComment)
                msg += $" To remedy: add the keyword to a preceding comment, or refactor it to a parameter.";

            else if (allowedIfInName)
                msg += $" To remedy: add the keyword to the declaration name, or refactor it to a parameter.";

            else
                msg += " To remedy, refactor it to a parameter.";
            return msg;
        }



        public HiddenDependenciesDetector(ILogger logger, Context context, HiddenDependenciesSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void EnterDeclaration(Declaration declaration)
        {
            if (!declaration.DeclarationType.IsMacroType())
                return;

            thisDeclaration = declaration;
        }

        public override void LeaveDeclaration(Declaration declaration)
        {
            if (thisDeclaration == null)
                return;

            if (!usedRoot && !usedPrev && usedSavedScopes.Count == 0 && usedVariables.Count == 0)
            {
                thisDeclaration = null;
                return;
            }

            var commentText = GatherPrecedingComments(declaration);

            if (usedRoot)
                HandleUseOfRoot(declaration, commentText);

            if (usedPrev)
                HandleUseOfPrev(declaration, commentText);

            //foreach (var scope in usedSavedScopes)
            //{
            //    HandleUseOfSavedScope(declaration, commentText, scope);
            //}

            //foreach (var var in usedVariables)
            //{
            //    HandleUseOfVariables(declaration, commentText, var);
            //}
            

            thisDeclaration = null;
            usedRoot = false;
            usedPrev = false;
            usedSavedScopes = [];
            usedVariables = [];
            rootUsages = [];
            prevUsages = [];
        }

        private void HandleUseOfSavedScope(Declaration declaration, string commentText, string scopeName)
        {
            if (((ScriptFile)declaration.Parent).ExpectedDeclarationType == DeclarationType.Event &&
                _settings.UseOfSavedScope_AllowedIfInEventFile)
                return;

            var isSmelly = HandleUseKeyword(scopeName, declaration, commentText, _settings.UseOfSavedScope_AllowedIfInComment, _settings.UseOfSavedScope_AllowedIfInName);

            if (isSmelly)
            {
                var msg = GetUsedSavedScopeMsg();

                logger.Log(
                    Smell.HiddenDependencies_UseOfSavedScope,
                    _settings.UseOfSavedScope_Severity,
                    msg,
                    declaration
                );
            }
        }

        private void HandleUseOfVariables(Declaration declaration, string commentText, string varName)
        {
            if (((ScriptFile)declaration.Parent).ExpectedDeclarationType == DeclarationType.Event &&
                _settings.UseOfVariable_AllowedIfInEventFile)
                return;

            if (_settings.UseOfVariable_Whitelist.Contains(varName))
                return;

            var isSmelly = HandleUseKeyword(varName, declaration, commentText, _settings.UseOfSavedScope_AllowedIfInComment, _settings.UseOfSavedScope_AllowedIfInName);

            if (isSmelly)
            {
                var msg = GetUsedVariableMsg();
                logger.Log(
                    Smell.HiddenDependencies_UseOfSavedScope,
                    _settings.UseOfSavedScope_Severity,
                    "Declaration uses 'var:" + varName + "'." + msg,
                    declaration
                );
            }
        }


        private void HandleUseOfPrev(Declaration declaration, string commentText)
        {
            if (((ScriptFile)declaration.Parent).ExpectedDeclarationType == DeclarationType.Event &&
                _settings.UseOfPrev_AllowedIfInEventFile)
                return;

            var isSmelly = HandleUseKeyword("prev", declaration, commentText, _settings.UseOfPrev_AllowedIfInComment, _settings.UseOfPrev_AllowedIfInName);
            
            if (isSmelly)
            {
                var msg = GetUsedPrevMsg();
                foreach (var usage in prevUsages)
                {
                    logger.Log(
                        Smell.HiddenDependencies_UseOfPrev,
                        _settings.UseOfPrev_Severity,
                        msg,
                        usage
                    );
                }
            }
        }
        private void HandleUseOfRoot(Declaration declaration, string commentText)
        {
            if (((ScriptFile)declaration.Parent).ExpectedDeclarationType == DeclarationType.Event &&
                _settings.UseOfRoot_AllowedIfInEventFile)
                return;

            var isSmelly = HandleUseKeyword("root", declaration, commentText, _settings.UseOfRoot_AllowedIfInComment, _settings.UseOfRoot_AllowedIfInName);

            if (isSmelly)
            {
                var msg = GetUsedRootMsg();

                foreach (var usage in rootUsages)
                {
                    logger.Log(
                        Smell.HiddenDependencies_UseOfRoot,
                        _settings.UseOfRoot_Severity,
                        msg,
                        usage
                    );
                }
            }
        }

        private bool HandleUseKeyword(string keyword, Declaration declaration, string commentText, bool ignoreIfInComment, bool ignoreIfInName)
        {
            bool tolerate = false;
            if (ignoreIfInComment && commentText.ToLowerInvariant().Contains(keyword))
                tolerate = true;

            if (ignoreIfInName && declaration.Key.ToLowerInvariant().Contains(keyword))
                tolerate = true;

            return !tolerate;
        }

        private string GatherPrecedingComments(Declaration declaration)
        {
            var prevSibling = declaration.PrevSibling;

            // Event file declarations have one extra token before comments can start
            if (((ScriptFile)declaration.Parent).ExpectedDeclarationType == DeclarationType.Event){
                prevSibling = prevSibling.PrevSibling;
            }

            if (prevSibling == null || prevSibling.GetType() != typeof(Comment))
                return "";

            var sb = new StringBuilder();
            while (prevSibling != null && prevSibling is Comment prevComment)
            {
                sb.Insert(0, prevComment.RawWithoutHashtag + "\n");
                prevSibling = prevSibling.PrevSibling;
            }
            return sb.ToString();
        }

        //private readonly HashSet<string> scopeSettingBlocks = [
        //    "save_scope_value_as",
        //    "save_temporary_scope_value_as",
        //    "save_opinion_value_as",
        //    "save_temporary_opinion_value_as"
        //];

        public override void EnterNamedBlock(NamedBlock namedBlock)
        {
            if (thisDeclaration == null)
                return;

            var key = namedBlock.Key.ToLowerInvariant();
            var isTopLevel = namedBlock.Parent == thisDeclaration;
            AnalyseToken(key, isTopLevel, namedBlock);
            //if (scopeSettingBlocks.Contains(key))
            //{
            //    var setScopeName = namedBlock.Children.OfType<BinaryExpression>().FirstOrDefault(x => x.Key == "name")?.Value;
            //    if (setScopeName != null)
            //    {
            //        setSavedScopes.Add(setScopeName);
            //    }
            //}
        }

        public override void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (thisDeclaration == null)
                return;

            var key = binaryExpression.Key.ToLowerInvariant();
            var isTopLevel = binaryExpression.Parent == thisDeclaration;
            AnalyseToken(key, isTopLevel, binaryExpression);
            AnalyseToken(binaryExpression.Value.ToLowerInvariant(), isTopLevel, binaryExpression);
        }

        private void AnalyseToken(string key, bool isTopLevel, Node node)
        {
            if (key.Contains('.'))
            {
                var subkeys = key.Split('.');
                foreach (var subkey in subkeys)
                {
                    DetermineKeywordUsage(subkey, isTopLevel, node);
                }
            }
            else
            {
                DetermineKeywordUsage(key, isTopLevel, node);
            }
        }

        private void DetermineKeywordUsage(string key, bool isTopLevel, Node node)
        {
            if (key == "root")
            {
                usedRoot = true;
                rootUsages.Add(node);
            }
            else if (key == "prev" && isTopLevel)
            {
                usedPrev = true;
                prevUsages.Add(node);
            }
            //else if (key.StartsWith("scope:"))
            //{
            //    var scopename = key.Substring(6);
            //    usedSavedScopes.Add(scopename);
            //}
            //else if (key.StartsWith("var:"))
            //{
            //    var varname = key.Substring(4);
            //    usedVariables.Add(varname);
            //}
        }
    }
}
