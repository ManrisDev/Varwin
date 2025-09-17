namespace Varwin.Public
{
    public interface ITouchStartInteractionAware : IVarwinInputAware
    {
        void OnTouchStart(TouchInteractionContext context);
    }

    public interface ITouchEndInteractionAware : IVarwinInputAware
    {
        void OnTouchEnd(TouchInteractionContext context);
    }
}