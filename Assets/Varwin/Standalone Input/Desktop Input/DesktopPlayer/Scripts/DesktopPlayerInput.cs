using System;
using UnityEngine;

namespace Varwin.DesktopPlayer
{
    public class DesktopPlayerInput : MonoBehaviour
    {
        [SerializeField]
        private Vector2 _movementInput = Vector2.zero;
        
        [SerializeField]
        private Vector2 _cursorInput = Vector2.zero;
        
        [SerializeField]
        private Vector2 _mouseInput = Vector2.zero;

        [SerializeField]
        private bool _grabbingInput;

        [SerializeField]
        private bool _usingInput;

        [SerializeField]
        private bool _cameraFixedInput;

        [SerializeField]
        private bool _sprintInput;

        [SerializeField]
        private bool _crouchInput;

        [SerializeField]
        private bool _jumpInput;

        [SerializeField]
        private bool _teleportInput;

        #region DESKTOP INPUT ADAPTER

        public event Action UsePressed;
        public event Action UseReleased;
        public event Action GrabPressed;
        public event Action GrabReleased;
        public event Action TeleportPressed;
        public event Action TeleportReleased;

        private bool _isUsePressed;
        private bool _isUseReleased;
        private bool _isGrabPressed;
        private bool _isGrabReleased;
        private bool _isTeleportPressed;
        private bool _isTeleportReleased;
        
        public bool IsTeleportActive { get; private set; }
        #endregion
        
        private void Update()
        {
            _movementInput = new Vector2(Input.GetAxis("Strafe"), Input.GetAxis("Walk"));
            _cursorInput = new Vector2(Input.GetAxis("Cursor X"), Input.GetAxis("Cursor Y"));
            _mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            _grabbingInput = Input.GetButton("Grab");
            _usingInput = Input.GetButton("Use");
            _cameraFixedInput = Input.GetButton("FixCamera");
            _sprintInput = Input.GetButton("Sprint");
            _crouchInput = Input.GetButton("Crouch");
            _jumpInput = Input.GetButton("Jump");
            _teleportInput = Input.GetButtonDown("Teleport");
            
            ProcessDesktopAdapterInput();
            InvokeDesktopAdapterEvents();
        }

        #region DESKTOP INPUT ADAPTER

        private void ProcessDesktopAdapterInput()
        {
            _isUsePressed = Input.GetButtonDown("Use");
            _isUseReleased = Input.GetButtonUp("Use");
            _isGrabPressed = Input.GetButtonDown("Grab");
            _isGrabReleased = Input.GetButtonUp("Grab");


            _isTeleportPressed = IsTeleportActive;
            
            if (IsTeleportActive && _isUsePressed)
            {
                _isTeleportReleased = true;
                IsTeleportActive = false;
            }
            else
            {
                _isTeleportReleased = false;
            }

            if (_teleportInput)
            {
                IsTeleportActive = !IsTeleportActive;
            }
        }
        
        private void InvokeDesktopAdapterEvents()
        {
            if (_isUsePressed)
            {
                UsePressed?.Invoke();
            }

            if (_isUseReleased)
            {
                UseReleased?.Invoke();
            }

            if (_isGrabPressed)
            {
                GrabPressed?.Invoke();
            }

            if (_isGrabReleased)
            {
                GrabReleased?.Invoke();
            }

            if (_isTeleportPressed)
            {
                TeleportPressed?.Invoke();
            }

            if (_isTeleportReleased)
            {
                TeleportReleased?.Invoke();
            }
        }

        #endregion

        #region PUBLIC INPUT

        public bool IsMoving => _movementInput != Vector2.zero;

        public Vector2 PlayerMovement => _movementInput;

        public Vector2 Cursor => _cursorInput;

        public Vector2 Mouse => _mouseInput;

        public bool IsGrabbing => _grabbingInput;

        public bool IsUsing => _usingInput;

        public bool IsCameraFixed => _cameraFixedInput;

        public bool IsSprinting => _sprintInput;

        public bool IsCrouching => _crouchInput;

        public bool IsJumping => _jumpInput;
        
        public bool IsTeleporting
        {
            get => _teleportInput;
            set => _teleportInput = value;
        }

        #endregion
        
    }
}