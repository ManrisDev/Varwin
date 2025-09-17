using System;

namespace Varwin.Public
{
    [Obsolete("This interface is obsolete. Use IGrabStartInteractionAware")]
    public interface IGrabStartAware : IVarwinInputAware
    {
        void OnGrabStart(GrabingContext context);
    }

    [Obsolete("This interface is obsolete. Use IGrabEndInteractionAware")]
    public interface IGrabEndAware : IVarwinInputAware
    {
        void OnGrabEnd();
    }
}
