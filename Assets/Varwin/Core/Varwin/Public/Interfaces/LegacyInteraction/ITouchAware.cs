using System;

namespace Varwin.Public
{
    [Obsolete("This interface is obsolete. Use ITouchStartInteractionAware")]
    public interface ITouchStartAware : IVarwinInputAware
    {
        void OnTouchStart();
    }

    [Obsolete("This interface is obsolete. Use ITouchEndInteractionAware")]
    public interface ITouchEndAware : IVarwinInputAware
    {
        void OnTouchEnd();
    }
}
