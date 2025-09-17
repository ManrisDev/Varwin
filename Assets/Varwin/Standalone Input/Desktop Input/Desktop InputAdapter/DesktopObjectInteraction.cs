using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.DesktopInput
{
    public class DesktopObjectInteraction : ObjectInteraction
    {
        public DesktopObjectInteraction()
        {
            Object = new ComponentWrapFactory<InteractObject, DesktopInteractObject, DesktopInteractableObject>();
        }

        private class DesktopInteractObject : InteractObject, IInitializable<DesktopInteractableObject>
        {
            public override bool SwapControllersFlag { get; set; }
            public override event InteractableObjectEventHandler InteractableObjectGrabbed;
            public override event InteractableObjectEventHandler InteractableObjectUngrabbed;
            public override event InteractableObjectEventHandler InteractableObjectTouched;
            public override event InteractableObjectEventHandler InteractableObjectUntouched;
            public override event InteractableObjectEventHandler InteractableObjectUsed;
            public override event InteractableObjectEventHandler InteractableObjectUnused;

            private DesktopInteractableObject _interactableObject;

            public override bool isGrabbable
            {
                set => _interactableObject.IsGrabbable = value;
                get => _interactableObject.IsGrabbable;
            }

            public override bool isUsable
            {
                set => _interactableObject.IsUsable = value;
                get => _interactableObject.IsUsable;
            }

            public override bool isTouchable
            {
                set => _interactableObject.IsTouchable = value;
                get => _interactableObject.IsTouchable;
            }

            public override void DestroyComponent()
            {
                _interactableObject.Destroy();
            }

            public override bool IsGrabbed() => _interactableObject.IsGrabbed();

            public override bool IsUsing() => _interactableObject.IsUsing();
            public override bool IsTouching() => _interactableObject.IsTouching();

            public override GameObject GetGrabbingObject() => _interactableObject.GetGrabbedObject();

            public override GameObject GetUsingObject() => _interactableObject.GetUsingObject();

            public override void ForceStopInteracting() => _interactableObject.ForceStopInteracting();
            
            public override void DropGrabbedObjectAndDeactivate() => _interactableObject.DropGrabbedObjectAndDeactivate();

            public override bool IsForceGrabbed() => _interactableObject && _interactableObject.IsForceGrabbed;

            public override ValidDropTypes ValidDrop
            {
                get => _interactableObject.ValidDrop;
                set => _interactableObject.ValidDrop = value;
            }

            public override ControllerInput.ButtonAlias useOverrideButton
            {
                set => _interactableObject.UseOverrideButton = value;
            }

            public override ControllerInput.ButtonAlias grabOverrideButton
            {
                set => _interactableObject.GrabOverrideButton = value;
            }

            public void Init(DesktopInteractableObject monoBehaviour)
            {
                _interactableObject = monoBehaviour;

                _interactableObject.InteractableObjectGrabbed += (sender, args) => { InteractableObjectGrabbed?.Invoke(sender, args); };

                _interactableObject.InteractableObjectUngrabbed += (sender, args) => { InteractableObjectUngrabbed?.Invoke(sender, args); };

                _interactableObject.InteractableObjectTouched += (sender, args) => { InteractableObjectTouched?.Invoke(sender, args); };

                _interactableObject.InteractableObjectUntouched += (sender, args) => { InteractableObjectUntouched?.Invoke(sender, args); };

                _interactableObject.InteractableObjectUsed += (sender, args) => { InteractableObjectUsed?.Invoke(sender, args); };

                _interactableObject.InteractableObjectUnused += (sender, args) => { InteractableObjectUnused?.Invoke(sender, args); };
            }
        }
    }
}