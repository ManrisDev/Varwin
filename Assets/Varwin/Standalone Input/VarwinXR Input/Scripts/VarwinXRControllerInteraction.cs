using System.Collections.Generic;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    public class VarwinXRControllerInteraction : ControllerInteraction
    {
        public VarwinXRControllerInteraction()
        {
            Controller = new ComponentWrapFactory<ControllerSelf, VarwinXRController, XR.VarwinXRController>();
            Haptics = new VarwinXRControllerHaptics();
        }
        
        private class VarwinXRController : ControllerSelf, IInitializable<XR.VarwinXRController>
        {
            private XR.VarwinXRController _varwinXRController;
            private HashSet<Collider> _colliders;
            
            public override void TriggerHapticPulse(float strength, float duration, float interval)
            {
                _varwinXRController.SetTriggerHapticPulse(strength, duration, interval);
            }

            public override void StopInteraction()
            {
                if (!_varwinXRController.ForcedGrab)
                {
                    ForceDropObject();
                }
            }

            public override void ForceGrabObject(GameObject gameObject)
            {
                _varwinXRController.ForceGrab(gameObject);
            }

            public override void ForceDropObject()
            {
                if (_varwinXRController.GrabbedObject)
                {
                    _varwinXRController.ForceDrop();
                }
            }

            public override void ForceDropObject(GameObject gameObject)
            {
                var grabbedObject = _varwinXRController.GrabbedObject;
                if (grabbedObject && grabbedObject.gameObject == gameObject)
                {
                    _varwinXRController.ForceDrop();
                }
            }

            public override GameObject GetGrabbedObject() => _varwinXRController.GrabbedObject ? _varwinXRController.GrabbedObject.gameObject : null;

            public override void SetGrabColliders(Collider[] colliders)
            {
                _colliders = colliders == null ? null : new HashSet<Collider>(colliders);
            }

            public override bool CheckIfColliderPresent(Collider coll)
            {
                return _colliders != null && _colliders.Contains(coll);
            }

            public void Init(XR.VarwinXRController interactableObject)
            {
                _varwinXRController = interactableObject;
            }
        }

        private class VarwinXRControllerHaptics : ControllerHaptics
        {
            public override void TriggerHapticPulse(PlayerController.PlayerNodes.ControllerNode playerController, float strength, float duration, float interval)
            {
                playerController.Controller.TriggerHapticPulse(strength, duration, interval);
            }
        }
    }
}