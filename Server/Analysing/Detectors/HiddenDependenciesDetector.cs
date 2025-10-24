using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;
using System.Text;

namespace CK3Analyser.Analysis.Detectors
{
    public class HiddenDependenciesDetector : BaseDetector
    {
        public readonly struct Settings
        {
            public bool Enabled { get; init; }
            public Severity Severity_UseOfRoot { get; init; }
            public bool UseOfRoot_IgnoreIfInName { get; init; }
            public bool UseOfRoot_IgnoreIfInComment { get; init; }
            public bool UseOfRoot_AllowInEventFile { get; init; }

            public Severity Severity_UseOfPrev { get; init; }
            public bool UseOfPrev_IgnoreIfInName { get; init; }
            public bool UseOfPrev_IgnoreIfInComment { get; init; }
            public bool UseOfPrev_AllowInEventFile { get; init; }

            public Severity Severity_UseOfSavedScope { get; init; }
            public bool UseOfSavedScope_IgnoreIfInName { get; init; }
            public bool UseOfSavedScope_IgnoreIfInComment { get; init; }
            public bool UseOfSavedScope_AllowInEventFile { get; init; }

            public Severity Severity_UseOfVariable { get; init; }
            public bool UseOfVariable_IgnoreIfInName { get; init; }
            public bool UseOfVariable_IgnoreIfInComment { get; init; }
            public bool UseOfVariable_AllowInEventFile { get; init; }

            public HashSet<string> VariablesWhitelist { get; init; }
        }

        private readonly Settings _settings;

        private Declaration thisDeclaration;
        private bool usedRoot = false;
        private bool usedPrev = false;
        private HashSet<string> usedSavedScopes = [];
        private HashSet<string> setSavedScopes = [];
        private HashSet<string> usedVariables = [];
        private HashSet<string> setVariables = [];

        public HiddenDependenciesDetector(ILogger logger, Context context, Settings settings) : base(logger, context)
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
        }

        private void HandleUseOfSavedScope(Declaration declaration, string commentText, string scopeName)
        {
            if (((ScriptFile)declaration.Parent).ExpectedDeclarationType == DeclarationType.Event &&
                _settings.UseOfSavedScope_AllowInEventFile)
                return;

            (bool isSmelly, string msg) = HandleUseKeyword(scopeName, declaration, commentText, _settings.UseOfSavedScope_IgnoreIfInComment, _settings.UseOfSavedScope_IgnoreIfInName);

            if (isSmelly)
            {
                logger.Log(
                    Smell.HiddenDependencies_UseOfSavedScope,
                    _settings.Severity_UseOfSavedScope,
                    "Declaration uses 'scope:" + scopeName + "'." + msg,
                    declaration.GetIdentifier(),
                    declaration.StartIndex,
                    declaration.EndIndex
                );
            }
        }

        private void HandleUseOfVariables(Declaration declaration, string commentText, string varName)
        {
            if (((ScriptFile)declaration.Parent).ExpectedDeclarationType == DeclarationType.Event &&
                _settings.UseOfVariable_AllowInEventFile)
                return;

            if (_settings.VariablesWhitelist.Contains(varName))
                return;

            (bool isSmelly, string msg) = HandleUseKeyword(varName, declaration, commentText, _settings.UseOfSavedScope_IgnoreIfInComment, _settings.UseOfSavedScope_IgnoreIfInName);

            if (isSmelly)
            {
                logger.Log(
                    Smell.HiddenDependencies_UseOfSavedScope,
                    _settings.Severity_UseOfSavedScope,
                    "Declaration uses 'var:" + varName + "'." + msg,
                    declaration.GetIdentifier(),
                    declaration.StartIndex,
                    declaration.EndIndex
                );
            }
        }


        private void HandleUseOfPrev(Declaration declaration, string commentText)
        {
            if (((ScriptFile)declaration.Parent).ExpectedDeclarationType == DeclarationType.Event &&
                _settings.UseOfPrev_AllowInEventFile)
                return;

            (bool isSmelly, string msg) = HandleUseKeyword("prev", declaration, commentText, _settings.UseOfPrev_IgnoreIfInComment, _settings.UseOfPrev_IgnoreIfInName);
            
            if (isSmelly)
            {
                logger.Log(
                    Smell.HiddenDependencies_UseOfPrev,
                    _settings.Severity_UseOfPrev,
                    "Declaration uses 'prev' at the top level." + msg,
                    declaration.GetIdentifier(),
                    declaration.StartIndex,
                    declaration.EndIndex
                );
            }
        }
        private void HandleUseOfRoot(Declaration declaration, string commentText)
        {
            if (((ScriptFile)declaration.Parent).ExpectedDeclarationType == DeclarationType.Event &&
                _settings.UseOfRoot_AllowInEventFile)
                return;

            (bool isSmelly, string msg) = HandleUseKeyword("root", declaration, commentText, _settings.UseOfRoot_IgnoreIfInComment, _settings.UseOfRoot_IgnoreIfInName);

            if (isSmelly)
            {
                logger.Log(
                    Smell.HiddenDependencies_UseOfRoot,
                    _settings.Severity_UseOfRoot,
                    "Declaration uses 'root'." + msg,
                    declaration.GetIdentifier(),
                    declaration.StartIndex,
                    declaration.EndIndex
                );
            }
        }

        private (bool, string) HandleUseKeyword(string keyword, Declaration declaration, string commentText, bool ignoreIfInComment, bool ignoreIfInName)
        {
            bool tolerate = false;
            var msg = "";
            if (ignoreIfInComment && commentText.ToLowerInvariant().Contains(keyword))
                tolerate = true;

            if (ignoreIfInName && declaration.Key.ToLowerInvariant().Contains(keyword))
                tolerate = true;

            if (!tolerate)
            {
                if (ignoreIfInComment && ignoreIfInName)
                    msg += $" To remedy: add the word '{keyword}' to the declaration name, a preceding comment, or refactor it to a parameter.";

                else if (ignoreIfInComment)
                    msg += $" To remedy: add the word '{keyword}' to a preceding comment, or refactor it to a parameter.";

                else if (ignoreIfInName)
                    msg += $" To remedy: add the word '{keyword}' to the declaration name, or refactor it to a parameter.";

                else
                    msg += " To remedy, refactor it to a parameter.";
            }

            return (!tolerate, msg);
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
            AnalyseToken(key, isTopLevel);
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
            AnalyseToken(key, isTopLevel);
            AnalyseToken(binaryExpression.Value.ToLowerInvariant(), isTopLevel);
        }

        private void AnalyseToken(string key, bool isTopLevel)
        {
            if (key.Contains('.'))
            {
                var subkeys = key.Split('.');
                foreach (var subkey in subkeys)
                {
                    DetermineKeywordUsage(subkey, isTopLevel);
                }
            }
            else
            {
                DetermineKeywordUsage(key, isTopLevel);
            }
        }

        private void DetermineKeywordUsage(string key, bool isTopLevel)
        {
            if (key == "root")
            {
                usedRoot = true;
            }
            else if (key == "prev" && isTopLevel)
            {
                usedPrev = true;
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
