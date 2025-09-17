using UnityEngine;
using System.Linq;
using Varwin.DesktopPlayer;
#if VARWINCLIENT
using IngameDebugConsole;
using Varwin.Desktop;
#endif
using Varwin.PlatformAdapter;
using Varwin.Public;

namespace Varwin.SpectatorPlayer
{
    [RequireComponent(typeof(DesktopPlayerInput))]
    public class SpectatorPlayerController : MonoBehaviour, IPlayerController
    {
        public static SpectatorPlayerController Instance { get; private set; }
        
        public bool IsSprinting { get; private set; }

        public Quaternion Rotation
        {
            get => Quaternion.Euler(_currentRotation);
            set => SetRotation(value);
        }
        
        public Vector3 Position
        {
            get => transform.position;
            set => SetPosition(value);
        }

        public bool PlayerCursorIsVisible = true;

        [Header("Interaction")]
        [SerializeField]
        public Camera PlayerCamera;

        [Header("Camera")] 
        [SerializeField]
        private float _normalFieldOfView = 60f;
        [SerializeField]
        private float _sprintFieldOfViewChange = 2f;


        [Header("Movement")]
        [SerializeField]
        private float _movementSpeed = 4f;
        [SerializeField]
        private float _sprintSpeedMultiplier = 2f;

        [Header("Rotation")]
        [SerializeField]
        private float _normalRotationSpeed = 2.0f;
        [SerializeField]
        private float _sprintRotationSpeed = 1.0f;

        private float _rotationSpeed;
        private float _speedMultiplier;

        private float _currentFieldOfView;
        private float _targetLevel;

        private Vector3 _beforeJumpFootPosition;

        private Vector3 _cameraLocalPosition;
        private Vector3 _currentRotation;
        
        private DesktopPlayerInput _playerInput;

        private Collider _blockingCollider;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            
            PlayerManager.PlayerRespawned += OnPlayerRespawned;
            ProjectData.GameModeChanged += OnGameModeChanged;
            
            gameObject.SetActive(ProjectData.GameMode != GameMode.Edit);

            FieldOfView = _normalFieldOfView;
        }

        private void Start()
        {
            _playerInput = GetComponent<DesktopPlayerInput>();
            InputAdapter.Instance.PlayerController.PlayerTeleported += PlayerControllerOnPlayerTeleported;
            
            if (PlayerCamera)
            {
                _cameraLocalPosition = PlayerCamera.transform.localPosition;
                _currentRotation = PlayerCamera.transform.rotation.eulerAngles;
            }

            _rotationSpeed = _normalRotationSpeed;
            _speedMultiplier = 1f;

            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (PlayerCamera)
            {
                _cameraLocalPosition = PlayerCamera.transform.localPosition;
                _currentRotation = PlayerCamera.transform.rotation.eulerAngles;
            }

            SetPosition(transform.position);
            SetRotation(transform.rotation);
        }

        private void OnDisable()
        {
            ResetCursor();
        }

        private void OnDestroy()
        {
            InputAdapter.Instance.PlayerController.PlayerTeleported -= PlayerControllerOnPlayerTeleported;
            PlayerManager.PlayerRespawned -= OnPlayerRespawned;
            ProjectData.GameModeChanged -= OnGameModeChanged;
        }

        private void OnGameModeChanged(GameMode gameMode)
        {
            if (gameMode == GameMode.Edit)
            {
                PlayerManager.Respawn();
            }

            gameObject.SetActive(gameMode != GameMode.Edit);
        }

        private void Update()
        {
            if (ProjectData.GameMode == GameMode.Undefined)
            {
                return;
            }
            
#if VARWINCLIENT
            if (DesktopPopupManager.IsPopupShown || DesktopEscapeMenu.IsActive)
            {
                ResetCursor();
                return;
            }

            if (DebugLogManager.Instance && DebugLogManager.Instance.IsLogWindowVisible)
            {
                ResetCursor();
                return;
            }
#endif
            
            UpdateInput();
            UpdateMovement();
            UpdateCamera();

            Cursor.visible = ProjectData.IsMultiplayerSceneActive;
        }

        private void ResetCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnPlayerRespawned()
        {
            SetPosition(PlayerAnchorManager.SpawnPoint.position);
            SetRotation(PlayerAnchorManager.SpawnPoint.rotation);
        }

        private void PlayerControllerOnPlayerTeleported(Vector3 position)
        {
            SetPosition(position);
        }

        private void UpdateInput()
        {
            IsSprinting = _playerInput.IsSprinting;
        }
        
        private void UpdateMovement()
        {
            if (!PlayerManager.MovementEnabled || !PlayerManager.WasdMovementEnabled || !_playerInput.IsMoving)
            {
                return;
            }

            Vector2 offset = _playerInput.PlayerMovement;

            if (offset.sqrMagnitude > 1f)
            {
                offset = offset.normalized;
            }
            
            var positionOffset = PlayerCamera.transform.forward * offset.x + PlayerCamera.transform.right * offset.y;

            var targetMovementMultiplier = 1f;
            if (IsSprinting)
            {
                targetMovementMultiplier = _sprintSpeedMultiplier;
            }

            _speedMultiplier = Mathf.Lerp(_speedMultiplier, targetMovementMultiplier, 10f * Time.deltaTime);

            MoveIfPossibleTo(_speedMultiplier * Time.deltaTime * _movementSpeed * positionOffset);

        }

        private void UpdateCamera()
        {
            float cameraHeight = Mathf.Lerp(PlayerCamera.transform.localPosition.y, _cameraLocalPosition.y, 10f * Time.deltaTime);
            PlayerCamera.transform.localPosition = new Vector3(_cameraLocalPosition.x, cameraHeight, _cameraLocalPosition.z);

            Vector2 cursorInput = _playerInput.Cursor;
            
            UpdateFieldOfView();
            _currentRotation.x -= cursorInput.y * _rotationSpeed;
            _currentRotation.y += cursorInput.x * _rotationSpeed;
                    
            SetRotation(_currentRotation);
            
            _rotationSpeed = Mathf.Lerp(_rotationSpeed, IsSprinting ? _sprintRotationSpeed : _normalRotationSpeed, 10f * Time.deltaTime);
            
            if (!_playerInput.IsCameraFixed)
            {
                Cursor.lockState = Application.isFocused && !ProjectData.IsMultiplayerSceneActive ? CursorLockMode.Locked : CursorLockMode.None;            
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void UpdateFieldOfView()
        {
            float targetFieldOfView = _currentFieldOfView;
            float fieldOfViewLerpSpeed = 10f;
            
            if (_playerInput.IsMoving && IsSprinting)
            {
                targetFieldOfView += _sprintFieldOfViewChange;
                fieldOfViewLerpSpeed = 5f;
            }
            
            PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, targetFieldOfView, fieldOfViewLerpSpeed * Time.deltaTime);
        }

        public float FieldOfView
        {
            get => _currentFieldOfView;
            set
            {
                PlayerCamera.fieldOfView = value;
                _currentFieldOfView = value;
            }
        }

        private void MoveIfPossibleTo(Vector3 moveDelta)
        {
            Vector3 futurePosition = transform.position + moveDelta;

            transform.position = futurePosition;
        }
        
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            SetRotation(rotation.eulerAngles);
        }

        public void SetRotation(Vector3 rotation)
        {
            _currentRotation = rotation;
            
            if (_currentRotation.x >= 270f)
            {
                _currentRotation.x -= 360f;
            }
                    
            _currentRotation.y = Mathf.Repeat(_currentRotation.y, 360);
            _currentRotation.x = Mathf.Clamp(_currentRotation.x, -90, 90);
            
            transform.rotation = Quaternion.Euler(0, _currentRotation.y, 0);
            PlayerCamera.transform.localRotation = Quaternion.Euler(_currentRotation.x, 0, 0);
        }
        
        public void CopyTransform(Transform targetTransform)
        {
            SetRotation(targetTransform.rotation);
            SetPosition(targetTransform.position);
        }
    }
}
