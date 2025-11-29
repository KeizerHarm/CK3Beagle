using CK3Analyser.Analysing.Logging;
using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Domain;

namespace CK3Analyser.Analysing.Diff.Detectors
{
    public class BaseDiffDetector
    {
        protected ILogger logger;

        public BaseDiffDetector(ILogger logger)
        {
            this.logger = logger;
        }

        public virtual void VisitUnchanged(Delta delta) { }
        public virtual void VisitAdded(Delta delta) { }
        public virtual void VisitDeleted(Delta delta) { }
        public virtual void EnterChanged(Delta delta) { }
        public virtual void LeaveChanged(Delta delta) { }
        public virtual void EnterChangedInChildren(Delta delta) { }
        public virtual void LeaveChangedInChildren(Delta delta) { }
        public virtual void Finish() { }
    }
}
