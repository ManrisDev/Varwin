namespace Varwin.Public
{
    public interface IUseStartInteractionAware : IVarwinInputAware
    {
        void OnUseStart(UseInteractionContext context);
    }

    public interface IUseEndInteractionAware : IVarwinInputAware
    {
        void OnUseEnd(UseInteractionContext context);
    }
}