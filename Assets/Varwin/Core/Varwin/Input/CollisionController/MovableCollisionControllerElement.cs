using UnityEngine;
using Varwin.Core.Behaviours.ConstructorLib;

namespace Varwin.ObjectsInteractions
{
    public class MovableCollisionControllerElement : CollisionControllerElement
    {
        public MovableBehaviour MovableBehaviour { get; set; }
    
        protected override void RaiseOnTriggerEnter(Collider other)
        {
            base.RaiseOnTriggerEnter(other);
            if (MovableBehaviour)
            {
                MovableBehaviour.OnGrabbedCollisionEnter(other.gameObject);    
            }
        }

        protected override void RaiseOnTriggerExit(Collider other)
        {
            base.RaiseOnTriggerExit(other);
            if (MovableBehaviour)
            {
                MovableBehaviour.OnGrabbedCollisionExit(other.gameObject);
            }
        }
    }
}