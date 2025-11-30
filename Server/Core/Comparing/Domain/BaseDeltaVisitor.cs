namespace CK3BeagleServer.Core.Comparing.Domain
{
    public class BaseDeltaVisitor : IDeltaVisitor
    {
        public virtual void VisitAdded(Delta delta)
        {
        }

        public virtual void VisitChanged(Delta delta)
        {
            if (delta.Children != null)
            {
                foreach (var child in delta.Children)
                {
                    child.Accept(this);
                }
            }
        }

        public virtual void VisitChangedInChildren(Delta delta)
        {
            foreach (var child in delta.Children)
            {
                child.Accept(this);
            }
        }

        public virtual void VisitDeleted(Delta delta)
        {
        }

        public virtual void VisitUnchanged(Delta delta)
        {
        }
    }
}
