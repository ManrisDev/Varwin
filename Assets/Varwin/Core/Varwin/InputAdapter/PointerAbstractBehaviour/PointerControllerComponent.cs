using System.Collections.Generic;
using UnityEngine;

namespace Varwin.PlatformAdapter
{
    public abstract class PointerControllerComponent : MonoBehaviour
    {
        protected abstract List<IBasePointer> _pointers { get; set; }
        public IBasePointer CurrentPointer;

        protected virtual void UpdatePointers()
        {
            _pointers.ForEach(a => a.UpdateState());
            foreach (IBasePointer pointer in _pointers)
            {
                if (pointer.CanRelease() && pointer.IsActive())
                {
                    pointer.Release();
                    return;
                }
                
                if (!pointer.CanPress())
                {
                    if (!pointer.CanToggle())
                    {
                        continue;
                    }

                    TryChangePointer(pointer);
                    return;
                }

                pointer.Press();
                return;
            }

            TryChangePointer(null);
        }

        protected virtual bool TryChangePointer(IBasePointer pointer)
        {
            if (CurrentPointer == pointer)
            {
                return false;
            }
                
            CurrentPointer?.Toggle(false);
            CurrentPointer = pointer;
            CurrentPointer?.Toggle(true);

            return true;
        }
    }
}
