using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Comparing.Domain;
using CK3BeagleServer.Core.Domain;

namespace CK3BeagleServer.Analysing.Diff.Detectors
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
