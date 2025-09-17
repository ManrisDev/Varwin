using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    public class VarwinXRObjectInteraction : ObjectInteraction
    {
        public VarwinXRObjectInteraction()
        {
            Object = new ComponentWrapFactory<InteractObject, VarwinXRInteractObject, VarwinXRInteractableObject>();
            Haptics = new ComponentWrapFactory<InteractHaptics, VarwinXRInteractHaptics, VarwinXRInteractHapticsDUMMY>();
        }
            
        
        private class VarwinXRInteractObject : InteractObject, IInitializable<VarwinXRInteractableObject>
        {
            private VarwinXRInteractableObject _interactableObject;

            public void Init(VarwinXRInteractableObject interactableObject)
            {
                _interactableObject = interactableObject;

                _interactableObject.ObjectGrabbed += (sender, grabbingObject) =>
                {
                    InteractableObjectGrabbed?.Invoke(sender, new InteractableObjectEventArgs{ interactingObject = (GameObject)grabbingObject, Hand = InputAdapter.Instance.PlayerController.Nodes.GetControllerHand((GameObject)grabbingObject)});
                };
                
                _interactableObject.ObjectDropped += (sender, grabbingObject) =>
                {
                    InteractableObjectUngrabbed?.Invoke(sender, new InteractableObjectEventArgs{ interactingObject = (GameObject)grabbingObject, Hand = InputAdapter.Instance.PlayerController.Nodes.GetControllerHand((GameObject)grabbingObject)});
                };
                
                _interactableObject.ObjectTouched += (sender, touchingObject) =>
                {
                    InteractableObjectTouched?.Invoke(sender, new InteractableObjectEventArgs{ interactingObject = (GameObject)touchingObject, Hand = InputAdapter.Instance.PlayerController.Nodes.GetControllerHand((GameObject)touchingObject)});
                };
                
                _interactableObject.ObjectUntouched += (sender, touchingObject) =>
                {
                    InteractableObjectUntouched?.Invoke(sender, new InteractableObjectEventArgs{ interactingObject = (GameObject)touchingObject, Hand = InputAdapter.Instance.PlayerController.Nodes.GetControllerHand((GameObject)touchingObject)});
                };
                
                _interactableObject.ObjectUsed += (sender, touchingObject) =>
                {
                    InteractableObjectUsed?.Invoke(sender, new InteractableObjectEventArgs{ interactingObject = (GameObject)touchingObject, Hand = InputAdapter.Instance.PlayerController.Nodes.GetControllerHand((GameObject)touchingObject)});
                };
                
                _interactableObject.ObjectUnused += (sender, touchingObject) =>
                {
                    InteractableObjectUnused?.Invoke(sender, new InteractableObjectEventArgs{ interactingObject = (GameObject)touchingObject, Hand = InputAdapter.Instance.PlayerController.Nodes.GetControllerHand((GameObject)touchingObject)});
                };
            }

            public override bool isGrabbable
            {
                get => _interactableObject.IsGrabbable;
                set => _interactableObject.IsGrabbable = value;
            }

            public override bool isUsable
            {
                get => _interactableObject.IsUsable;
                set => _interactableObject.IsUsable = value;
            }

            public override bool isTouchable
            {
                get => _interactableObject.IsTouchable;
                set => _interactableObject.IsTouchable = value;
            }

            public override ValidDropTypes ValidDrop
            {
                get => _interactableObject.CanBeDrop ? ValidDropTypes.DropAnywhere : ValidDropTypes.NoDrop;
                set => _interactableObject.CanBeDrop = value == ValidDropTypes.DropAnywhere;
            }

            public override ControllerInput.ButtonAlias useOverrideButton
            {
                set { }
            }

            public override ControllerInput.ButtonAlias grabOverrideButton
            {
                set { }
            }

            public override bool SwapControllersFlag { get; set; }

            public override event InteractableObjectEventHandler InteractableObjectGrabbed;
            public override event InteractableObjectEventHandler InteractableObjectUngrabbed;
            public override event InteractableObjectEventHandler InteractableObjectTouched;
            public override event InteractableObjectEventHandler InteractableObjectUntouched;
            public override event InteractableObjectEventHandler InteractableObjectUsed;
            public override event InteractableObjectEventHandler InteractableObjectUnused;
            
            public override void DestroyComponent()
            {
                UnityEngine.Object.Destroy(_interactableObject);
            }

            public override GameObject GetGrabbingObject() => _interactableObject && _interactableObject.GrabbedBy ? _interactableObject.GrabbedBy.gameObject : null;

            public override void ForceStopInteracting()
            {
                _interactableObject.GrabbedBy?.ForceDrop();
                _interactableObject.UsedBy?.ForceUnUse();
                _interactableObject.TouchedBy?.ForceUnTouch();
            }

            public override void DropGrabbedObjectAndDeactivate()
            {
                ForceStopInteracting();
                _interactableObject.gameObject.SetActive(false);
            }

            public override bool IsGrabbed() => _interactableObject.IsGrabbed;

            public override bool IsUsing() => _interactableObject.IsUsed;
            
            public override bool IsTouching() => _interactableObject.IsTouched;

            public override bool IsForceGrabbed() => _interactableObject && _interactableObject.IsForceGrabbed;

            public override GameObject GetUsingObject() => _interactableObject.LastInteractedBy.gameObject;

        }

        private class VarwinXRInteractHaptics : InteractHaptics, IInitializable<VarwinXRInteractHapticsDUMMY>
        {
            public override float StrengthOnUse
            {
                set { }
            }

            public override float IntervalOnUse
            {
                set { }
            }

            public override float DurationOnUse
            {
                set { }
            }

            public override float StrengthOnTouch
            {
                set { }
            }

            public override float IntervalOnTouch
            {
                set { }
            }

            public override float DurationOnTouch
            {
                set { }
            }

            public override float StrengthOnGrab
            {
                set { }
            }

            public override float IntervalOnGrab
            {
                set { }
            }

            public override float DurationOnGrab
            {
                set { }
            }

            public void Init(VarwinXRInteractHapticsDUMMY interactableObject)
            {
            }
        }
    }

    public class VarwinXRInteractHapticsDUMMY : MonoBehaviour
    {
    }
}