//using CK3Analyser.Core.Comparing.Domain;
//using CK3Analyser.Core.Domain.Entities;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace CK3Analyser.Core.Comparing.PogingLCS
//{

//    public class DeltaTreeBuilder
//    {
//        public ScriptFile Source { get; }
//        public ScriptFile Edit { get; }
//        public HashSet<string> AddedDeclarations { get; private set; }
//        public HashSet<string> ChangedDeclarations { get; private set; }
//        public HashSet<string> RemovedDeclarations { get; private set; }
//        public HashSet<string> UntouchedDeclarations { get; private set; }
//        private static TrueHashBasedNodeComparer defaultComparator = new TrueHashBasedNodeComparer();


//        public DeltaTreeBuilder(ScriptFile source, ScriptFile edit)
//        {
//            Source = source;
//            Edit = edit;

//            (AddedDeclarations, RemovedDeclarations, ChangedDeclarations, UntouchedDeclarations)
//                = ComparisonHelpers.SimpleListComparison(Source.Declarations.ToDictionary(), Edit.Declarations.ToDictionary(),
//                    (first, second) => first.GetTrueHash() == second.GetTrueHash());
//        }

//        public void Build(ScriptFile source, ScriptFile edit)
//        {
//            var rootDelta = new Delta();

//            //HandleBlocks(source, edit, rootDelta);
            
//        }

//        public Delta BinExpDelta(BinaryExpression source, BinaryExpression edit)
//        {
//            if (source.Key == edit.Key && source.Value == edit.Value && source.Scoper == edit.Scoper)
//            {
//                return Delta.Unchanged(edit);
//            }
//            return Delta.Changed(edit, new ShadowNode(source));
//        }

//        public Delta AnonymousTokenDelta(AnonymousToken source, AnonymousToken edit)
//        {
//            if (source.Value == edit.Value)
//            {
//                return Delta.Unchanged(edit);
//            }
//            return Delta.Changed(edit, new ShadowNode(source));
//        }

//        public Delta CommentDelta(Comment source, Comment edit)
//        {
//            if (source.RawWithoutHashtag == edit.RawWithoutHashtag)
//            {
//                return Delta.Unchanged(edit);
//            }
//            return Delta.Changed(edit, new ShadowNode(source));
//        }

//        //public Delta AnonymousBlockDelta(AnonymousBlock source, AnonymousBlock edit)
//        //{
//        //    if (source.Children.Count == 0 && edit.Children.Count == 0)
//        //    {
//        //        return Delta.UnchangedNode(edit);
//        //    }
//        //    var lcsLength = LcsCalculator.ComputeLcsLength(
//        //        CollectionsMarshal.AsSpan(source.Children),
//        //        CollectionsMarshal.AsSpan(edit.Children),
//        //        defaultComparator);

//        //    if (lcsLength == 0) //No nodes in common, consider entire block changed
//        //    {
//        //        return Delta.ModifiedNode(edit, new ShadowNode(source));
//        //    }

//        //    if (source.Children.Count == lcsLength && lcsLength == edit.Children.Count)
//        //    {
//        //        return Delta.UnchangedNode(edit);
//        //    }


//        //    var maxChildCount = Math.Max(source.Children.Count, edit.Children.Count);



//        //    var lcs = LcsCalculator.ComputeLcs(
//        //        CollectionsMarshal.AsSpan(source.Children),
//        //        CollectionsMarshal.AsSpan(edit.Children),
//        //        defaultComparator).ToArray();
//        //    int i = 0;
//        //    Node sourceChild = source.Children[0]; Node editChild = edit.Children[0];
//        //    while (i < lcsLength &&)
//        //}
//    }
//}
