using System;
using UnityEngine;
using UnityEngine.Events;

namespace Varwin.Public
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(VarwinObjectDescriptor))]
    public class InteractableObjectBehaviour : MonoBehaviour, 
        IGrabStartInteractionAware,
        IGrabEndInteractionAware,
        IUseStartInteractionAware,
        IUseEndInteractionAware,
        ITouchStartInteractionAware,
        ITouchEndInteractionAware
    {
        [Header("Interaction settings")] [SerializeField]
        private bool _isGrabbable;

        [SerializeField] private bool _isUsable;

        [SerializeField] private bool _isTouchable;

        public bool IsInteractable => IsTouchable || IsUsable || IsGrabbable;

        public bool IsGrabbable
        {
            get => _isGrabbable;
            set
            {
                _isGrabbable = value;
                Wrapper.GrabEnabled = value;
            }
        }

        public bool IsUsable
        {
            get => _isUsable;
            set
            {
                _isUsable = value;
                Wrapper.UseEnabled = value;
            }
        }

        public bool IsTouchable
        {
            get => _isTouchable;
            set
            {
                _isTouchable = value;
                Wrapper.TouchEnabled = value;
            }
        }

        [NonSerialized] public bool IsGrabbed;

        [NonSerialized] public bool IsUsed;

        [NonSerialized] public bool IsTouched;

        [Space(5)] [Header("Events")] 
        public UnityEvent OnGrabStarted = new();
        public UnityEvent OnGrabEnded = new();
        public UnityEvent OnUseStarted = new();
        public UnityEvent OnUseEnded = new();
        public UnityEvent OnTouchStarted = new();
        public UnityEvent OnTouchEnded = new();

        private Wrapper _wrapper;
        private Wrapper Wrapper => _wrapper ??= gameObject.GetWrapper();

        private void OnDestroy()
        {
            _wrapper = null;
        }

        public void OnGrabStart(GrabInteractionContext context)
        {
            if (!_isGrabbable)
            {
                return;
            }

            IsGrabbed = true;
            OnGrabStarted?.Invoke();
        }

        public void OnGrabEnd(GrabInteractionContext context)
        {
            IsGrabbed = false;
            OnGrabEnded?.Invoke();
        }

        public void OnUseStart(UseInteractionContext context)
        {
            if (!_isUsable)
            {
                return;
            }

            IsUsed = true;
            OnUseStarted?.Invoke();
        }

        public void OnUseEnd(UseInteractionContext context)
        {
            IsUsed = false;
            OnUseEnded?.Invoke();
        }

        public void OnTouchStart(TouchInteractionContext context)
        {
            if (!_isTouchable)
            {
                return;
            }

            IsTouched = true;
            OnTouchStarted?.Invoke();
        }

        public void OnTouchEnd(TouchInteractionContext context)
        {
            IsTouched = false;
            OnTouchEnded?.Invoke();
        }

        public void SetIsGrabbable(bool state)
        {
            _isGrabbable = state;
        }

        public void SetIsUsable(bool state)
        {
            _isUsable = state;
        }

        public void SetIsTouchable(bool state)
        {
            _isTouchable = state;
        }
    }
}