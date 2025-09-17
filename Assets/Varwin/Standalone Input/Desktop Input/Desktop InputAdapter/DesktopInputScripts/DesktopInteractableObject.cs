using UnityEngine;
using Varwin.PlatformAdapter;
using Varwin.Public;

namespace Varwin.DesktopInput
{
    public class DesktopInteractableObject : MonoBehaviour, IInteractableObject
    {
        public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectTouched;
        public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectUntouched;
        public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectUsed;
        public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectUnused;
        public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectGrabbed;
        public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectUngrabbed;

        private bool _isUsing;
        private bool _isTouching;
        private GameObject _grabbingObject;
        private GameObject _usingObject;
        private DesktopDetachable _detachable;
        public bool IsForceGrabbed;

        public bool HasVarwinAttachPoint { get; private set; }

        public bool IsInteractable => !ProjectData.InteractionWithObjectsLocked && (IsGrabbable || IsUsable || IsTouchable);
        
        public bool IsGrabbable
        {
            get
            {
                if (!_detachable)
                {
                    return false;
                }

                return _detachable.IsGrabbable;
            }

            set
            {
                if (!_detachable)
                {
                    return;
                }

                _detachable.IsGrabbable = value;
            }
        }

        public bool IsUsable { get; set; }

        public bool IsTouchable { get; set; }
        
        public ObjectInteraction.InteractObject.ValidDropTypes ValidDrop
        {
            get
            {
                if (_detachable && _detachable.CanBeDetached)
                {
                    return ObjectInteraction.InteractObject.ValidDropTypes.DropAnywhere;
                }

                return ObjectInteraction.InteractObject.ValidDropTypes.NoDrop;
            }
            set
            {
                if (!_detachable)
                {
                    return;
                }

                switch (value)
                {
                    case ObjectInteraction.InteractObject.ValidDropTypes.DropAnywhere:
                        _detachable.CanBeDetached = true;

                        break;
                    case ObjectInteraction.InteractObject.ValidDropTypes.NoDrop:
                        _detachable.CanBeDetached = false;

                        break;
                }
            }
        }

        public ControllerInput.ButtonAlias UseOverrideButton { get; set; }

        public ControllerInput.ButtonAlias GrabOverrideButton { get; set; }

        private void Awake()
        {
            if (GetComponent<Rigidbody>())
            {
                _detachable = gameObject.AddComponent<DesktopDetachable>();
            }

            HasVarwinAttachPoint = gameObject.GetComponentInChildren<VarwinAttachPoint>();
        }

        public void Destroy()
        {
            Destroy(_detachable);
            Destroy(this);
        }

        public bool IsGrabbed() => _grabbingObject;

        public bool IsUsing() => _isUsing;

        public bool IsTouching() => _isTouching;
        
        public GameObject GetGrabbedObject() => _grabbingObject;

        public GameObject GetUsingObject()
        {
            if (_grabbingObject)
            {
                return _grabbingObject;
            }

            if (_usingObject)
            {
                return _usingObject;
            }

            return null;
        }

        public void ForceStopInteracting() => DesktopPlayer.DesktopPlayerController.Instance.DropGrabbedObject();

        public void DropGrabbedObjectAndDeactivate()
        {
            ForceStopInteracting();
            gameObject.SetActive(false);
        }

        public void StartTouch()
        {
            if (ProjectData.InteractionWithObjectsLocked || !IsTouchable && !IsUsable && !IsGrabbable)
            {
                return;
            }

            _isTouching = true;
            
            InteractableObjectTouched?.Invoke(this,
                new ObjectInteraction.InteractableObjectEventArgs
                {
                    interactingObject = gameObject, Hand = ControllerInteraction.DefaultDesktopIterationHand
                });
        }

        public void StopTouch()
        {
            if (!_isTouching)
            {
                return;
            }

            InteractableObjectUntouched?.Invoke(this,
                new ObjectInteraction.InteractableObjectEventArgs
                {
                    interactingObject = gameObject, Hand = ControllerInteraction.DefaultDesktopIterationHand
                });

            _isTouching = false;
        }
        
        public void StartUse(GameObject interactingObject)
        {
            if (ProjectData.InteractionWithObjectsLocked || !IsUsable)
            {
                return;
            }

            _isUsing = true;
            
            _usingObject = interactingObject;
            
            InteractableObjectUsed?.Invoke(this,
                new ObjectInteraction.InteractableObjectEventArgs
                {
                    interactingObject = gameObject, Hand = ControllerInteraction.DefaultDesktopIterationHand
                });
        }

        public void StopUse()
        {
            if (!_isUsing)
            {
                return;
            }

            _usingObject = null;
            
            InteractableObjectUnused?.Invoke(this,
                new ObjectInteraction.InteractableObjectEventArgs
                {
                    interactingObject = gameObject, Hand = ControllerInteraction.DefaultDesktopIterationHand
                });

            _isUsing = false;
        }

        public void StartGrab(GameObject interactingObject)
        {
            if (ProjectData.InteractionWithObjectsLocked || !IsGrabbable)
            {
                return;
            }

            _grabbingObject = interactingObject;

            InteractableObjectGrabbed?.Invoke(this,
                new ObjectInteraction.InteractableObjectEventArgs
                {
                    interactingObject = gameObject, Hand = ControllerInteraction.DefaultDesktopIterationHand
                });
        }

        public void StopGrab()
        {
            IsForceGrabbed = false;
            _grabbingObject = null;

            InteractableObjectUngrabbed?.Invoke(this,
                new ObjectInteraction.InteractableObjectEventArgs
                {
                    interactingObject = gameObject, Hand = ControllerInteraction.DefaultDesktopIterationHand
                });
        }
    }
}
