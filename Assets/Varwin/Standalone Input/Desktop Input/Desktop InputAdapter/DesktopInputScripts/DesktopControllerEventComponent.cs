using System;
using UnityEngine;
using Varwin.PlatformAdapter;
using Varwin.DesktopPlayer;

namespace Varwin.DesktopInput
{
    public class DesktopControllerEventComponent : MonoBehaviour
    {
        public event ControllerInput.ControllerInteractionEventHandler ControllerEnabled;
        public event ControllerInput.ControllerInteractionEventHandler LeftMouseButtonPressed;
        public event ControllerInput.ControllerInteractionEventHandler LeftMouseButtonReleased;
        public event ControllerInput.ControllerInteractionEventHandler RightMouseButtonPressed;
        public event ControllerInput.ControllerInteractionEventHandler RightMouseButtonReleased;
        public event ControllerInput.ControllerInteractionEventHandler TeleportPressed;
        public event ControllerInput.ControllerInteractionEventHandler TeleportReleased;

        private bool _isTeleportPressed;
        private bool _isTeleportReleased;
        private bool _isLeftMouseButtonPressed;
        private bool _isLeftMouseButtonReleased;
        private bool _isRightMouseButtonPressed;

        private void Awake()
        {
            SetupInputEvents();
        }

        private void Start()
        {
            ControllerEnabled?.Invoke(null);
        }

        private void LateUpdate()
        {
            _isTeleportReleased = false;
            _isTeleportPressed = false;
        }

        public bool IsTouchpadPressed() => _isTeleportPressed;

        public bool IsTouchpadReleased() => _isTeleportReleased;

        public bool IsTriggerPressed() => _isLeftMouseButtonPressed;

        public bool IsTriggerReleased() => _isLeftMouseButtonReleased;

        public bool IsButtonPressed(ControllerInput.ButtonAlias gripPress)
        {
            switch (gripPress)
            {
                case ControllerInput.ButtonAlias.GripPress: return _isRightMouseButtonPressed;
                default: throw new NotImplementedException();
            }
        }

        public void OnGripReleased(ControllerInput.ControllerInteractionEventArgs controllerInteractionEventArgs)
        {
            RightMouseButtonReleased?.Invoke(this, controllerInteractionEventArgs);
        }

        public GameObject GetController() => gameObject;

        private void SetupInputEvents()
        {
            DesktopPlayerInput desktopPlayerInput = GetComponentInParent<DesktopPlayerInput>();
            
            ControllerInput.ControllerInteractionEventArgs controllerInteractionEventArgs =
                new ControllerInput.ControllerInteractionEventArgs
                {
                    controllerReference = new PlayerController.ControllerReferenceArgs
                    {
                        hand = ControllerInteraction.ControllerHand.Right
                    }
                };

            desktopPlayerInput.UsePressed += () =>
            {
                LeftMouseButtonPressed?.Invoke(this, controllerInteractionEventArgs);
                _isLeftMouseButtonPressed = true;
                _isLeftMouseButtonReleased = false;
            };

            desktopPlayerInput.UseReleased += () =>
            {
                LeftMouseButtonReleased?.Invoke(this, controllerInteractionEventArgs);
                _isLeftMouseButtonPressed = false;
                _isLeftMouseButtonReleased = true;
            };

            desktopPlayerInput.GrabPressed += () =>
            {
                RightMouseButtonPressed?.Invoke(this, controllerInteractionEventArgs);
                _isRightMouseButtonPressed = true;
            };

            desktopPlayerInput.GrabReleased += () => { _isRightMouseButtonPressed = false; };

            desktopPlayerInput.TeleportPressed += () =>
            {
                TeleportPressed?.Invoke(this, controllerInteractionEventArgs);
                _isTeleportPressed = true;
                _isTeleportReleased = false;
            };

            desktopPlayerInput.TeleportReleased += () =>
            {
                TeleportReleased?.Invoke(this, controllerInteractionEventArgs);
                _isTeleportPressed = false;
                _isTeleportReleased = true;
            };
        }
    }
}
