namespace Varwin.Public
{
    public interface IHierarchyGrabStartAware
    {
        void OnGrabStart(ObjectController sender);
    }
    
    public interface IHierarchyGrabEndAware
    {
        void OnGrabEnd(ObjectController sender);
    }
}