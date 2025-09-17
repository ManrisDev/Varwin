using UnityEngine;

namespace Varwin.ObjectsInteractions
{
    public class CollisionControllerElement : MonoBehaviour
    {
        public delegate void CollisionDelegate(Collider other);
        
        public event CollisionDelegate OnCollisionEnterDelegate;
        public event CollisionDelegate OnTriggerExitDelegate;    
        
        private void OnTriggerEnter(Collider other)
        {
            if (CheckCollision(other))
            {
                return;
            }

            RaiseOnTriggerEnter(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (CheckCollision(other))
            {
                return;
            }
            
            OnCollisionEnterDelegate?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (CheckCollision(other))
            {
                return;
            }
            
            RaiseOnTriggerExit(other);
        }

        protected virtual void RaiseOnTriggerEnter(Collider other)
        {
            OnCollisionEnterDelegate?.Invoke(other);
        }
        
        protected virtual void RaiseOnTriggerExit(Collider other)
        {
            OnTriggerExitDelegate?.Invoke(other);
        }

        private bool CheckCollision(Collider other)
        {
            if (other.transform.root == transform.root)
            {
                return true;
            }
            
            var layer = other.gameObject.layer;

            return layer == LayerMask.NameToLayer("Ignore Raycast")
                   || layer == LayerMask.NameToLayer("Zones")
                   || layer == LayerMask.NameToLayer("VRControllers")
                   || other.gameObject.GetComponent<CollisionControllerElement>();
        }
    }
}