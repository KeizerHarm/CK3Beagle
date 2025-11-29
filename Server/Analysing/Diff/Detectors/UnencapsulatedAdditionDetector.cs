using CK3Analyser.Analysing.Logging;
using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources.DetectorSettings;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Analysing.Diff.Detectors
{
    public class UnencapsulatedAdditionDetector : BaseDiffDetector
    {
        private readonly UnencapsulatedAdditionSettings _settings;

        public UnencapsulatedAdditionDetector(ILogger logger, UnencapsulatedAdditionSettings settings) : base(logger)
        {
            _settings = settings;
        }

        public override void VisitAdded(Delta delta)
        {
            //Don't even start on comments
            if (delta.Node is Comment)
                return;

            //We need to find a sequence of additions, so check if this is the last addition, otherwise return
            var nextSibling = delta.GetNextSibling();
            if (   nextSibling != null 
                && nextSibling.Kind == DeltaKind.Added 
                && delta.Node.NextSibling == nextSibling.Node
                && nextSibling.Node is not Comment) {
                return;
            }

            //Now to gather a sequence of all preceding additions
            var sequence = new List<Delta>() { delta };
            var prevSibling = delta.GetPrevSibling();
            while (prevSibling != null 
                && prevSibling.Kind == DeltaKind.Added
                && prevSibling.Node == delta.Node.PrevSibling)
            {
                sequence.Insert(0, prevSibling);
            }

            var totalSize = sequence.Sum(x => x.Node.GetSize());

            if (totalSize >= _settings.Threshold) {
                logger.Log(
                    new LogEntry(
                        Smell.UnencapsulatedAddition,
                        _settings.Severity,
                        "Added sequence of " + totalSize + " statements, this should be a macro!",
                        delta.Node.File.AbsolutePath,
                        sequence.First().Node.Start,
                        delta.Node.End
                        ));
            }
        }
    }
}
