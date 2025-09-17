using UnityEngine;

namespace Varwin.NettleDesk
{
    public abstract class NettleDeskControllerInteractionComponentBase : MonoBehaviour
    {
        public abstract void DropGrabbedObject(bool force = false);
        public abstract void ForceGrabObject(GameObject targetObject);
        public abstract void ForceDropObject(GameObject targetObject);
        public abstract GameObject GetGrabbedObject();
    }
}