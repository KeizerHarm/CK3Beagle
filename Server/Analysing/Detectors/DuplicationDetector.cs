using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK3Analyser.Analysis.Detectors
{
    public class DuplicationDetector : BaseDetector
    {
        public DuplicationDetector(ILogger logger, Context context) : base(logger, context)
        {
        }

        private readonly Dictionary<string, List<NamedBlock>> namedBlocksByKey = [];
        private readonly Dictionary<string, List<BinaryExpression>> binaryExpressionsByKey = [];

        public override void AnalyseBinaryExpression(BinaryExpression binaryExpression)
        {
            if (!binaryExpressionsByKey.TryGetValue(binaryExpression.Key, out var list))
            {
                list = new List<BinaryExpression>();
                binaryExpressionsByKey.Add(binaryExpression.Key, list);
            }
            list.Add(binaryExpression);
        }

        public override void AnalyseNamedBlock(NamedBlock namedBlock)
        {
        }

        public override void Finish()
        {

        }
    }
}
