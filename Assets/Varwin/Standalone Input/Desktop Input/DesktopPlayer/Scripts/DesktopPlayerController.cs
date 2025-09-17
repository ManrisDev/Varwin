using UnityEngine;
using UnityEngine.SceneManagement;
#if VARWINCLIENT
using Varwin.Desktop;
using IngameDebugConsole;
#endif
using Varwin.PlatformAdapter;

namespace Varwin.DesktopPlayer
{
    [RequireComponent(typeof(DesktopPlayerInput), typeof(DesktopPlayerInteractionController))]
    public class DesktopPlayerController : MonoBehaviour, IPlayerController
    {
        public static DesktopPlayerController Instance { get; private set; }

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
        
        public float FieldOfView
        {
            get => _currentFieldOfView;
            set
            {
                PlayerCamera.fieldOfView = value;
                _currentFieldOfView = value;
            }
        }

        public bool PlayerCursorIsVisible = true;

        public Camera PlayerCamera;
        public GameObject Hand;
        
        [Header("Camera")] 
        [SerializeField]
        private float _normalFieldOfView = 60f;
        [SerializeField]
        private float _sprintFieldOfViewChange = 2f;

        [SerializeField] private DesktopLocomotionController _locomotionController;

        [Header("Rotation")]
        [SerializeField]
        private float _normalRotationSpeed = 2.0f;
        [SerializeField]
        private float _sprintRotationSpeed = 1.0f;

        private float _rotationSpeed;
        private float _currentFieldOfView;

        private Vector3 _currentRotation;
        
        private DesktopPlayerInput _playerInput;
        private DesktopPlayerInteractionController _interactionController;

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
            _interactionController = GetComponent<DesktopPlayerInteractionController>();
            InputAdapter.Instance.PlayerController.PlayerTeleported += PlayerControllerOnPlayerTeleported;
            
            if (PlayerCamera)
            {
                _currentRotation = PlayerCamera.transform.rotation.eulerAngles;
            }

            _rotationSpeed = _normalRotationSpeed;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (PlayerCamera)
            {
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
            
            _interactionController.SetCursorVisibility(PlayerCursorIsVisible && !Cursor.visible && PlayerManager.CursorIsVisible);
            
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

        private void UpdateCamera()
        {
            UpdateFieldOfView();
            
            var cursorInput = _playerInput.Cursor;
            
            _rotationSpeed = Mathf.Lerp(_rotationSpeed, _playerInput.IsSprinting ? _sprintRotationSpeed : _normalRotationSpeed, 10f * Time.deltaTime);
            
            if (!_playerInput.IsCameraFixed && PlayerManager.MouseLookEnabled)
            {
                Cursor.lockState = Application.isFocused && !ProjectData.IsMultiplayerSceneActive ? CursorLockMode.Locked : CursorLockMode.None;
                if (_interactionController.IsRotatingObject)
                {
                    return;
                }
                
                _currentRotation.x -= cursorInput.y * _rotationSpeed;
                _currentRotation.y += cursorInput.x * _rotationSpeed;
                    
                SetRotation(_currentRotation);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void UpdateFieldOfView()
        {
            var targetFieldOfView = _currentFieldOfView;
            var fieldOfViewLerpSpeed = 10f;
            
            if (_playerInput.IsMoving && _playerInput.IsSprinting)
            {
                targetFieldOfView += _sprintFieldOfViewChange;
                fieldOfViewLerpSpeed = 5f;
            }
            
            PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, targetFieldOfView, fieldOfViewLerpSpeed * Time.deltaTime);
        }

        public void ResetCameraState()
        {
            _currentRotation = Vector3.zero;
            PlayerCamera.transform.localRotation = Quaternion.identity;
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

        public void ForceGrabObject(GameObject gameObject)
        {
            if (_interactionController)
            {
                _interactionController.ForceGrabObject(gameObject);
            }
        }

        public void ForceDropObject(GameObject gameObject)
        {
            if (_interactionController)
            {
                _interactionController.ForceDropObject(gameObject);
            }
        }
        
        public void DropGrabbedObject(bool forced = false)
        {
            if (!_interactionController)
            {
                return;
            }
            
            if (forced)
            {
                _interactionController.ForceDropObject();
            }
            else
            {
                _interactionController.DropGrabbedObject();
            }
        }

        public void ResetVelocity()
        {
            _locomotionController.ResetVelocity();
        }
    }
}
