using System;

namespace Varwin.Public
{
    [Obsolete]
    public interface IPointerClickAware : IVarwinInputAware
    {
        void OnPointerClick();
    }

    [Obsolete]
    public interface IPointerInAware : IVarwinInputAware
    {
        void OnPointerIn();
    }

    [Obsolete]
    public interface IPointerOutAware : IVarwinInputAware
    {
        void OnPointerOut();
    }
    
    [Obsolete]
    public interface IPointerDownAware : IVarwinInputAware
    {
        void OnPointerDown();
    }
    
    [Obsolete]
    public interface IPointerUpAware : IVarwinInputAware
    {
        void OnPointerUp();
    }
}
