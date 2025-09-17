using UnityEngine;

namespace Varwin.NettleDeskPlayer
{
    public abstract class NettleDeskControllerBase : MonoBehaviour
    {
        public bool ForcedGrab { get; protected set; }
        public abstract void ForceDrop();
        public abstract void ForceUnUse();
        public abstract void ForceUnTouch();
    }
}