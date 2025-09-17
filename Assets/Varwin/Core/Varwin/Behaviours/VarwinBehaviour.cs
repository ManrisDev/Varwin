using UnityEngine;

namespace Varwin.Core.Behaviours
{
    public abstract class VarwinBehaviour : MonoBehaviour
    {
        public Wrapper Wrapper { get; set; }
        public ObjectController ObjectController { get; set; }

        private void Awake()
        {
            AwakeOverride();
        }

        public virtual void OnPrepare()
        {
            
        }

        protected virtual void OnDestroy()
        {

        }

        protected virtual void AwakeOverride() { }
    }
}
