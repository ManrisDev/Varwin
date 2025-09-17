using System.Collections.Generic;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.DesktopInput
{
    public class DesktopControllerInteraction : ControllerInteraction
    {
        public DesktopControllerInteraction()
        {
            Controller = new ComponentWrapFactory<ControllerSelf, DesktopController, DesktopControllerInteractionComponent>();
        }
        
        private class DesktopController : ControllerSelf, IInitializable<DesktopControllerInteractionComponent>
        {
            private DesktopControllerInteractionComponent _interactionComponent;
            private HashSet<Collider> _colliders;
            
            public override void TriggerHapticPulse(float strength, float duration, float interval)
            {
            }

            public override void StopInteraction()
            {
                DesktopPlayer.DesktopPlayerController.Instance.DropGrabbedObject();
            }

            public override void ForceGrabObject(GameObject gameObject)
            {
                DesktopPlayer.DesktopPlayerController.Instance.ForceGrabObject(gameObject);
            }

            public override void ForceDropObject()
            {
                DesktopPlayer.DesktopPlayerController.Instance.DropGrabbedObject(true);
            }

            public override void ForceDropObject(GameObject gameObject)
            {
                DesktopPlayer.DesktopPlayerController.Instance.ForceDropObject(gameObject);
            }
            
            public override GameObject GetGrabbedObject()
            {
               return _interactionComponent.GetGrabbedObject();
            }

            public override void SetGrabColliders(Collider[] colliders)
            {
                _colliders = colliders == null ? null : new HashSet<Collider>(colliders);
            }

            public override bool CheckIfColliderPresent(Collider coll)
            {
                if (_colliders == null)
                {
                    return false;
                }

                return _colliders.Contains(coll);
            }

            public void Init(DesktopControllerInteractionComponent monoBehaviour)
            {
                _interactionComponent = monoBehaviour;
            }
        }
    }
}
