using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Comparing.Domain;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources.DetectorSettings;
using System.Collections.Generic;
using System.Linq;

namespace CK3BeagleServer.Analysing.Diff.Detectors
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
            //Don't consider additions that aren't statements within Declarations
            if (delta.Node is Declaration || delta.Node.NodeType == NodeType.NonStatement)
                return;

            //We need to find a sequence of additions, so check if this is the last addition, otherwise return
            var nextSibling = delta.GetNextSibling();
            if (   nextSibling != null 
                && nextSibling.Kind == DeltaKind.Added 
                && delta.Node.NextSibling == nextSibling.Node
                && nextSibling.Node.NodeType != NodeType.NonStatement)
                return;

            //Now to gather a sequence of all preceding additions
            var sequence = new List<Delta>() { delta };
            var prevDeltaSibling = delta.GetPrevSibling();
            var prevNodeSibling = delta.Node.PrevSibling;
            while (prevDeltaSibling != null 
                && prevDeltaSibling.Kind == DeltaKind.Added
                && prevDeltaSibling.Node == prevNodeSibling)
            {
                sequence.Insert(0, prevDeltaSibling);
                prevDeltaSibling = prevDeltaSibling.GetPrevSibling();
                prevNodeSibling = prevNodeSibling.PrevSibling;
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
                        delta.Node.End));
            }
        }
    }
}