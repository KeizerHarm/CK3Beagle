namespace CK3Analyser.Core.Comparing.Domain
{
    public interface IDeltaVisitor
    {
        void VisitAdded(Delta delta);
        void VisitChanged(Delta delta);
        void VisitChangedInChildren(Delta delta);
        void VisitDeleted(Delta delta);
        void VisitUnchanged(Delta delta);
    }
}
