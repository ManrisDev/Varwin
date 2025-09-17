using System.Collections.Generic;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.NettleDesk
{
    public class NettleDeskControllerInteraction : ControllerInteraction
    {
        public NettleDeskControllerInteraction()
        {
            Controller = new ComponentWrapFactory<ControllerSelf, NettleDeskController, NettleDeskControllerInteractionComponentBase>();
        }

        private class NettleDeskController : ControllerSelf, IInitializable<NettleDeskControllerInteractionComponentBase>
        {
            private NettleDeskControllerInteractionComponentBase _interactionComponent;
            private HashSet<Collider> _colliders;

            public override void TriggerHapticPulse(float strength, float duration, float interval)
            {
            }

            public override void StopInteraction()
            {
                _interactionComponent.DropGrabbedObject();
            }

            public override void ForceGrabObject(GameObject gameObject)
            {
                _interactionComponent.ForceGrabObject(gameObject);
            }

            public override void ForceDropObject()
            {
                _interactionComponent.DropGrabbedObject(true);
            }

            public override void ForceDropObject(GameObject gameObject)
            {
                _interactionComponent.ForceDropObject(gameObject);
            }

            public override GameObject GetGrabbedObject()
            {
                return _interactionComponent.GetGrabbedObject();
            }

            public override void SetGrabColliders(Collider[] colliders)
            {
                if (colliders == null)
                {
                    _colliders = null;
                }
                else
                {
                    _colliders = new HashSet<Collider>(colliders);
                }
            }

            public override bool CheckIfColliderPresent(Collider coll)
            {
                if (_colliders == null)
                {
                    return false;
                }

                return _colliders.Contains(coll);
            }

            public void Init(NettleDeskControllerInteractionComponentBase monoBehaviour)
            {
                _interactionComponent = monoBehaviour;
            }
        }
    }
}