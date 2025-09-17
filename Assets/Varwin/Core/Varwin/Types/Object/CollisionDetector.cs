using UnityEngine;

namespace Varwin
{
    public class CollisionDetector : MonoBehaviour
    {
        public delegate void EventDelegate(Collider other);

        public event EventDelegate CollisionEntered;
        public event EventDelegate CollisionExited;

        private void OnCollisionEnter(Collision collision)
        {
            CollisionEntered?.Invoke(collision.collider);
        }

        private void OnCollisionExit(Collision collision)
        {
            CollisionExited?.Invoke(collision.collider);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            CollisionEntered?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            CollisionExited?.Invoke(other);
        }
    }
}