using CK3Analyser.Analysing.Diff.Detectors;
using CK3Analyser.Core.Comparing.Domain;
using System.Collections.Generic;

namespace CK3Analyser.Analysing.Diff
{
    public class AnalysisDeltaVisitor : BaseDeltaVisitor
    {
        public List<BaseDiffDetector> Detectors { get; } = new List<BaseDiffDetector>();

        public override void VisitChanged(Delta delta)
        {
            foreach (var detector in Detectors)
            {
                detector.EnterChanged(delta);
            }
            base.VisitChanged(delta);
            foreach (var detector in Detectors)
            {
                detector.LeaveChanged(delta);
            }
        }

        public override void VisitChangedInChildren(Delta delta)
        {
            foreach (var detector in Detectors)
            {
                detector.EnterChangedInChildren(delta);
            }
            base.VisitChanged(delta);
            foreach (var detector in Detectors)
            {
                detector.LeaveChangedInChildren(delta);
            }
        }

        public override void VisitAdded(Delta delta)
        {
            foreach (var detector in Detectors)
            {
                detector.VisitAdded(delta);
            }
            base.VisitAdded(delta);
        }

        public override void VisitDeleted(Delta delta)
        {
            foreach (var detector in Detectors)
            {
                detector.VisitDeleted(delta);
            }
            base.VisitDeleted(delta);
        }

        public override void VisitUnchanged(Delta delta)
        {
            foreach (var detector in Detectors)
            {
                detector.VisitUnchanged(delta);
            }
            base.VisitUnchanged(delta);
        }

        public void Finish()
        {
            foreach (var detector in Detectors)
            {
                detector.Finish();
            }
        }
    }
}
