namespace Varwin.Public
{
    public interface IPointerClickInteractionAware : IVarwinInputAware
    {
        void OnPointerClick(PointerInteractionContext context);
    }

    public interface IPointerInInteractionAware : IVarwinInputAware
    {
        void OnPointerIn(PointerInteractionContext context);
    }
    
    public interface IPointerOutInteractionAware : IVarwinInputAware
    {
        void OnPointerOut(PointerInteractionContext context);
    }

    public interface IPointerDownInteractionAware : IVarwinInputAware
    {
        void OnPointerDown(PointerInteractionContext context);
    }

    public interface IPointerUpInteractionAware : IVarwinInputAware
    {
        void OnPointerUp(PointerInteractionContext context);
    }
}