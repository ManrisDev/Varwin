using System.Linq;
using Illumetry.Unity;
using UnityEngine;
#if VARWINCLIENT
using Varwin.Desktop;
using IngameDebugConsole;
#endif
using Varwin.DesktopPlayer;
using Varwin.NettleDesk;
using Varwin.PlatformAdapter;
using Varwin.Public;

namespace Varwin.NettleDeskPlayer
{
    [RequireComponent(typeof(DesktopPlayerInput), typeof(NettleDeskInteractionController))]
    public class NettleDeskPlayerController : MonoBehaviour, IPlayerController
    {
        private const string WidthKey = "Screenmanager Resolution Width"; 
        private const string HeightKey = "Screenmanager Resolution Height";
        private const string FullscreenModeKey = "Screenmanager Fullscreen mode";
        private const string ScreenPositionXKey = "Screenmanager Window Position X";
        private const string ScreenPositionYKey = "Screenmanager Window Position Y";

        public static NettleDeskPlayerController Instance { get; private set; }
        
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
        
        public GameObject Head;
        public Camera HeadCamera;
        public NettleDeskControllerEye Hand;
        
        [Header("Rotation")] [SerializeField] private float _normalRotationSpeed = 2.0f;
        [SerializeField] private float _sprintRotationSpeed = 1.0f;
        [SerializeField] private NettleDeskLocomotionController _locomotionController;

        private float _rotationSpeed;

        private Vector3 _currentRotation;

        private DesktopPlayerInput _playerInput;
        private NettleDeskInteractionController _interactionController;

        private Collider _blockingCollider;
        private int _oldWidth;
        private int _oldHeight;
        private int _oldScreenPositionX;
        private int _oldScreenPositionY;
        private int _oldScreenMode;

        public DefaultScreenResolution DefaultScreenResolution;

        private void Awake()
        {
            UpdateOldScreenParams();

            if (!Instance)
            {
                Instance = this;
            }
            
            PlayerManager.PlayerRespawned += OnPlayerRespawned;
            ProjectData.GameModeChanged += OnGameModeChanged;

            gameObject.SetActive(ProjectData.GameMode != GameMode.Edit);
            
            QualitySettings.vSyncCount = 1;
        }

        private void UpdateOldScreenParams()
        {
            _oldWidth = PlayerPrefs.GetInt(WidthKey);
            _oldHeight = PlayerPrefs.GetInt(HeightKey);
            _oldScreenPositionX = PlayerPrefs.GetInt(ScreenPositionXKey);
            _oldScreenPositionY = PlayerPrefs.GetInt(ScreenPositionYKey);
            _oldScreenMode = PlayerPrefs.GetInt(FullscreenModeKey);
        }

        private void Start()
        {
            _playerInput = GetComponent<DesktopPlayerInput>();
            _interactionController = GetComponent<NettleDeskInteractionController>();
            InputAdapter.Instance.PlayerController.PlayerTeleported += PlayerControllerOnPlayerTeleported;

            if (Head)
            {
                _currentRotation = Head.transform.rotation.eulerAngles;
            }

            _rotationSpeed = _normalRotationSpeed;

            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (Head)
            {
                _currentRotation = Head.transform.rotation.eulerAngles;
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
            
            RestoreParams();
        }

        private void RestoreParams()
        {
            PlayerPrefs.SetInt(WidthKey, _oldWidth);
            PlayerPrefs.SetInt(HeightKey, _oldHeight);
            PlayerPrefs.SetInt(FullscreenModeKey, 3);
            PlayerPrefs.SetInt(ScreenPositionXKey, _oldScreenPositionX);
            PlayerPrefs.SetInt(ScreenPositionYKey, _oldScreenPositionY);
            PlayerPrefs.SetInt(FullscreenModeKey, _oldScreenMode);
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
            
            Cursor.lockState = CursorLockMode.Locked;
            
            _interactionController.SetCursorVisibility(PlayerCursorIsVisible && !Cursor.visible && PlayerManager.CursorIsVisible);

#if VARWINCLIENT
            if (DesktopPopupManager.IsPopupShown || DesktopEscapeMenu.IsActive || ProjectData.PlatformMode != PlatformMode.NettleDesk)
            {
                if (Screen.fullScreen)
                {
                    Screen.SetResolution(_oldWidth, _oldHeight, false);
                    Screen.MoveMainWindowTo(Screen.mainWindowDisplayInfo, new Vector2Int(_oldScreenPositionX, _oldScreenPositionY));
                }

                UpdateOldScreenParams();
                ResetCursor();
                return;
            }

            if (Screen.width != DefaultScreenResolution.Width || Screen.height != DefaultScreenResolution.Height || !Screen.fullScreen)
            {
                Screen.SetResolution(DefaultScreenResolution.Width, DefaultScreenResolution.Height, FullScreenMode.ExclusiveFullScreen);
            }

            if (DebugLogManager.Instance && DebugLogManager.Instance.IsLogWindowVisible)
            {
                ResetCursor();
                return;
            }
#endif

            UpdateHotkeys();
            UpdateCamera();

            Cursor.visible = false;
        }

        private void UpdateHotkeys()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && 
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) &&
                Input.GetKeyDown(KeyCode.C))
            {
                NettleDeskSettings.StylusSupport = !NettleDeskSettings.StylusSupport;
            }
        }

        private void LateUpdate()
        {
            RestoreParams();
        }

        private void ResetCursor()
        {
            _interactionController.CursorLocked = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
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
            var cursorInput = _playerInput.Cursor;

            _rotationSpeed = Mathf.Lerp(_rotationSpeed, _playerInput.IsSprinting ? _sprintRotationSpeed : _normalRotationSpeed, 10f * Time.deltaTime);

            if (!_playerInput.IsCameraFixed && PlayerManager.MouseLookEnabled)
            {
                _interactionController.CursorLocked = Application.isFocused;
                if (!_interactionController.IsRotatingObject)
                {
                    _currentRotation.x -= cursorInput.y * _rotationSpeed;
                    _currentRotation.y += cursorInput.x * _rotationSpeed;

                    SetRotation(_currentRotation);
                }
            }
            else
            {
                _interactionController.CursorLocked = false;
            }
        }
        
        public void ResetCameraState()
        {
            _currentRotation = Vector3.zero;
            Head.transform.localRotation = Quaternion.identity;
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
            Head.transform.localRotation = Quaternion.Euler(_currentRotation.x, 0, 0);
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
            if (_interactionController)
            {
                if (forced)
                {
                    _interactionController.ForceDropObject();
                }
                else
                {
                    _interactionController.DropGrabbedObject();
                }
            }
        }

        public void ResetVelocity()
        {
            _locomotionController.ResetVelocity();
        }
    }
}