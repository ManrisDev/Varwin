using System;

namespace Varwin.Public
{
    [Obsolete("This interface is obsolete. Use IUseStartInteractionAware")]
    public interface IUseStartAware : IVarwinInputAware
    {
        void OnUseStart(UsingContext context);
    }

    [Obsolete("This interface is obsolete. Use IUseEndInteractionAware")]
    public interface IUseEndAware : IVarwinInputAware
    {
        void OnUseEnd();
    }
}
