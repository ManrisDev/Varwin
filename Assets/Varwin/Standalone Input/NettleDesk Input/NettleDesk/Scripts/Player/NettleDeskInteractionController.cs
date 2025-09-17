using System;
using System.Linq;
using Illumetry.Unity;
using Illumetry.Unity.DisplayHandle;
using UnityEngine;
using Varwin.DesktopPlayer;
using Varwin.NettleDesk;
using Varwin.ObjectsInteractions;
using Varwin.PlatformAdapter;
using Varwin.Public;
using Display = Illumetry.Unity.Display;

namespace Varwin.NettleDeskPlayer
{
    public enum DesktopPlayerCursorState
    {
        Idle,
        Touch,
        Use,
        Grab,
        Pointer
    }

    [RequireComponent(typeof(NettleDeskPlayerController))]
    public class NettleDeskInteractionController : MonoBehaviour
    {
        private const int FilterListCount = 5;
        private const float DesktopGrabOffset = 1.3f;

        [Header("Interaction")] [SerializeField]
        private float minGrabDistance = 0.21f;

        [SerializeField] private float maxGrabDistance = 1.5f;
        private float _defaultMaxGrabDistance;

        [SerializeField] private NettleDeskLocomotionController _locomotionController;
        [SerializeField] private NettleDeskRaycaster _displayRaycaster;
        [SerializeField] private NettleDeskUIPointer _uiPointer;

        [Header("Primary cursor")] [SerializeField]
        private GameObject _cursorPivot;

        [Space] [SerializeField] private MeshRenderer _idleCursor;
        [SerializeField] private MeshRenderer _touchCursor;
        [SerializeField] private MeshRenderer _useCursor;
        [SerializeField] private MeshRenderer _grabCursor;

        [Header("Rotation")] public bool IsRotatingObject;
        public float RotationSpeed = 15f;

        private Vector3 _interactionCursorPosition;

        private float _currentCursorDistance;
        private float _targetCursorDistance;
        private const float ForceGrabCursorDistance = 1.5f;

        private NettleDeskPlayerController _playerController;
        private Camera _camera;

        private NettleDeskInteractableObject _lastTouchedInteractable;
        private Collider _lastTouchedCollider;

        private NettleDeskInteractableObject _usedObject;
        private NettleDeskInteractableObject _grabbedObject;
        private CollisionController _grabbedCollisionController;
        private Rigidbody _grabbedRigidbody;
        private GrabSettings _grabbedGrabSettings;
        private VarwinAttachPoint _grabbedAttachPoint;

        private bool _grabbedWasBlocked;

        private Vector3 _objectPositionOffset;
        private Quaternion _objectRotationOffset;

        private Vector3 _raycastPosition;

        private bool _uiPointerCanClick;
        private bool _layoutPopupIsActive;
        private Vector3 _oldCursorPosition;

        private DesktopPlayerInput _input;

        private GameObject _forceGrabbedObject;

        private int _numberOfHits;
        private RaycastHit[] _raycastHits = new RaycastHit[20];
        private RaycastHit[] _cursorRaycastHits = new RaycastHit[20];

        public MonoRenderingController MonoRenderingController;
        private MaterialPropertyBlock _useCursorMaterialPropertyBlock;

        public Vector3 CursorPos { get; private set; }

        public bool CursorLocked;
        private bool _cursorIsVisible = true;

        private Vector3 _oldGrabbedPosition;
        private Quaternion _oldGrabbedRotation;

        private readonly FixedList<Vector3> _velocityList = new(FilterListCount);
        private readonly FixedList<Vector3> _angularVelocityList = new(FilterListCount);

        public DisplayHandle DisplayHandle;
        public Display Display;

        public float StartCursorScale = 0.01f;

        private void Awake()
        {
            _playerController = GetComponent<NettleDeskPlayerController>();
            _camera = _playerController.HeadCamera;
            _input = GetComponent<DesktopPlayerInput>();
            _defaultMaxGrabDistance = maxGrabDistance;
            _useCursorMaterialPropertyBlock = new MaterialPropertyBlock();
            _locomotionController.PositionUpdated += OnPositionUpdated;
        }

        private void OnDestroy()
        {
            if (!_locomotionController)
            {
                return;
            }

            _locomotionController.PositionUpdated -= OnPositionUpdated;
        }

        private void Start()
        {
            SetCursorState(DesktopPlayerCursorState.Idle);
        }

        private void OnEnable()
        {
            OnUIOverlapChangeStatus(false);
            ProjectData.UIOverlapStatusChaned += OnUIOverlapChangeStatus;

            _input.GrabReleased += OnGrabReleased;
            _input.UsePressed += OnUsePressed;
            _input.UseReleased += OnUseReleased;

            SetCursorState(DesktopPlayerCursorState.Idle);
            NettleDeskSettings.SettingsChanged += OnSettingsChanged;
            // Cursor.visible = false;
        }

        private void OnSettingsChanged()
        {
            _cursorPivot.gameObject.SetActive(_cursorIsVisible && !NettleDeskSettings.StylusSupport);

            if (NettleDeskSettings.StylusSupport)
            {
                ForceDropObject();
            }
        }

        private void OnDisable()
        {
            DropGrabbedObject();
            ForceDropObject();

            _input.GrabReleased -= OnGrabReleased;
            _input.UsePressed -= OnUsePressed;
            _input.UseReleased -= OnUseReleased;

            //   Cursor.visible = true;
            ProjectData.UIOverlapStatusChaned -= OnUIOverlapChangeStatus;
            NettleDeskSettings.SettingsChanged -= OnSettingsChanged;
            OnUIOverlapChangeStatus(false);
        }

        public Vector3 CastToNettleDeskScreen(Vector2 screenPosition)
        {
#if !UNITY_EDITOR
          if (!Screen.fullScreen)
          {
              return screenPosition;
          }
#endif
            if (MonoRenderingController.MonoMode)
            {
                return screenPosition;
            }

            var x = screenPosition.x;
            var y = Mathf.Clamp(screenPosition.y, 0, MonoRenderingController.ScreenHeightMono);

            return new Vector2(x, y);
        }

        public Vector3 GetCenterOfScreen()
        {
#if !UNITY_EDITOR
          if (!Screen.fullScreen)
          {
              return new Vector3(Screen.width / 2f, Screen.height / 2f);
          }
#endif
            if (MonoRenderingController.MonoMode)
            {
                return new Vector3(Screen.width / 2f, Screen.height / 2f);
            }

            var x = Screen.width / 2f;
            var y = MonoRenderingController.ScreenHeightMono / 2f;

            return new Vector2(x, y);
        }

        private void OnPositionUpdated()
        {
            if (NettleDeskSettings.StylusSupport)
            {
                return;
            }

            maxGrabDistance = DistancePointer.CustomSettings ? DistancePointer.CustomSettings.Distance + DesktopGrabOffset : _defaultMaxGrabDistance;

            UpdateCursor();
            UpdateCursorPosition();

            _targetCursorDistance = ClampCursorDistance(_targetCursorDistance + 0.05f * Input.mouseScrollDelta.y);
            _currentCursorDistance = Mathf.Lerp(_currentCursorDistance, _targetCursorDistance, 12f * Time.deltaTime);

            UpdateUiPointer();

            if (_grabbedObject && (!_grabbedCollisionController || !_grabbedCollisionController.IsBlocked()) && _grabbedWasBlocked)
            {
                DropGrabbedObject();
            }

            IsRotatingObject = _grabbedObject && Input.GetMouseButton(2);
            if (IsRotatingObject)
            {
                float x = RotationSpeed * Input.GetAxis("Mouse X");
                float y = RotationSpeed * Input.GetAxis("Mouse Y");

                _objectRotationOffset = Quaternion.AngleAxis(y, Vector3.right) * _objectRotationOffset;
                _objectRotationOffset = Quaternion.AngleAxis(-x, Vector3.up) * _objectRotationOffset;
            }

            if (InteractingWithObject())
            {
                MoveAndRotateObject();
            }
            else
            {
                _playerController.Hand.transform.position = _raycastPosition;
            }

            var canDetach = _grabbedGrabSettings && _grabbedGrabSettings.DetachCursor;
            if (_grabbedObject && !_forceGrabbedObject && !canDetach)
            {
                return;
            }

            bool canTouch = !_layoutPopupIsActive && HitAnyInteractableObject();
            if (canTouch)
            {
                TouchClosestObject();
            }
            else
            {
                ForgetDroppedObject();
            }
        }

        private void UpdateCursorPosition()
        {
            if (CursorLocked)
            {
                CursorPos = GetCenterOfScreen();
            }
            else
            {
#if !UNITY_EDITOR
              if (!Screen.fullScreen)
              {
                  CursorPos = Input.mousePosition;
              }
#endif

                var delta = new Vector3(Input.GetAxis("Cursor X"), Input.GetAxis("Cursor Y")) * 15f;
                CursorPos = CastToNettleDeskScreen(CursorPos + delta);
            }

            _oldCursorPosition = Input.mousePosition;
        }

        private void UpdateCursor()
        {
            var ray = _camera.ScreenPointToRay(CursorPos);

            if (!_displayRaycaster.NearInteractableObject)
            {
                var plane = new Plane(DisplayHandle.transform.forward, DisplayHandle.transform.position);
                plane.Raycast(ray, out var pos);
                _cursorPivot.transform.position = ray.GetPoint(pos);
                _cursorPivot.transform.rotation = Quaternion.LookRotation(ray.direction);
                UpdateScaleCursorByDistance(pos);
                return;
            }

            var raycastHit = _displayRaycaster.NearObjectRaycastHit.Value;

            _cursorPivot.transform.position = raycastHit.point;
            _cursorPivot.transform.rotation = Quaternion.LookRotation(raycastHit.normal);
            UpdateScaleCursorByDistance(raycastHit.distance);
        }

        private void UpdateScaleCursorByDistance(float distance)
        {
            _cursorPivot.transform.localScale = Vector3.one * (StartCursorScale * distance * (1f / Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad / 2f)));
        }

        public GameObject GetGrabbedObject()
        {
            return _grabbedObject ? _grabbedObject.gameObject : null;
        }

        public void SetCursorVisibility(bool cursorVisibility)
        {
            _cursorIsVisible = cursorVisibility;
            _cursorPivot.gameObject.SetActive(_cursorIsVisible && !NettleDeskSettings.StylusSupport);
        }

        public void OnUIOverlapChangeStatus(bool popupIsActive) => _layoutPopupIsActive = popupIsActive;

        #region Update Sub Methods

        private void UpdateUiPointer()
        {
            if (_input.IsTeleportActive)
            {
                return;
            }

            if (_uiPointer.CanClick() && _displayRaycaster.NearPointableObject)
            {
                SetCursorState(DesktopPlayerCursorState.Pointer);
                _uiPointerCanClick = true;
            }
            else if (_uiPointerCanClick)
            {
                SetCursorState(DesktopPlayerCursorState.Idle);
                _uiPointerCanClick = false;
            }
        }

        private bool HitAnyInteractableObject()
        {
            return _displayRaycaster.NearInteractableObject && _displayRaycaster.NearInteractableObject.IsInteractable;
        }

        private void FixedUpdate()
        {
            var ray = _camera.ScreenPointToRay(CursorPos);
            var grabDistance = maxGrabDistance + _camera.transform.localPosition.magnitude;

            _displayRaycaster.Raycast(ray, NettleDeskUIPointer.CastDistance, grabDistance);
        }

        private void TouchClosestObject()
        {
            var interactable = _displayRaycaster.NearInteractableObject;
            if (!interactable)
            {
                SetCursorState(DesktopPlayerCursorState.Idle);
                return;
            }

            _raycastPosition = _displayRaycaster.NearObjectRaycastHit.HasValue
                ? _displayRaycaster.NearObjectRaycastHit.Value.point
                : _raycastPosition;

            Collider interactCollider = _displayRaycaster.NearObjectRaycastHit.HasValue ? _displayRaycaster.NearObjectRaycastHit.Value.collider : null;

            if (interactCollider == _lastTouchedCollider)
            {
                return;
            }

            if (_lastTouchedInteractable)
            {
                _lastTouchedInteractable.TouchEnd();
            }

            interactable.TouchStart(_playerController.Hand);
            _lastTouchedInteractable = interactable;
            _lastTouchedCollider = interactCollider;

            SetCursorState(interactable.IsGrabbable ? DesktopPlayerCursorState.Grab : interactable.IsUsable ? DesktopPlayerCursorState.Use : DesktopPlayerCursorState.Idle);
        }

        private void ForgetDroppedObject()
        {
            if (_lastTouchedInteractable)
            {
                _lastTouchedInteractable.TouchEnd();

                SetCursorState(DesktopPlayerCursorState.Idle);
            }

            _lastTouchedInteractable = null;
            _lastTouchedCollider = null;

            _raycastPosition = _camera.transform.position;
        }

        #endregion Update Sub Methods

        #region Late Update Sub Methods

        private bool InteractingWithObject() => _grabbedObject || _lastTouchedInteractable && _lastTouchedInteractable.IsUsed;

        private void MoveAndRotateObject()
        {
            var mousePosition = MouseInput.MousePosition;
            if (_grabbedAttachPoint || _grabbedGrabSettings && _grabbedGrabSettings.DetachCursor)
            {
                mousePosition = _interactionCursorPosition;
            }

            var cursorWorldPosition = _camera.ScreenPointToRay(mousePosition).GetPoint(_currentCursorDistance);
            var objectRotation = _camera.transform.rotation * _objectRotationOffset;
            var objectPosition = objectRotation * _objectPositionOffset + cursorWorldPosition;

            _playerController.Hand.transform.position = cursorWorldPosition;

            if (!_grabbedObject)
            {
                return;
            }
            
            _grabbedRigidbody.velocity = Vector3.zero;
            _grabbedRigidbody.angularVelocity = Vector3.zero;

            _grabbedRigidbody.transform.position = objectPosition;
            _grabbedRigidbody.transform.rotation = objectRotation;
            
            var rotationDelta = _grabbedRigidbody.rotation * Quaternion.Inverse(_oldGrabbedRotation);
            rotationDelta.ToAngleAxis(out var angle, out var axis);
            if (angle > 180f)
            {
                angle -= 360f;
            }
            
            var newAngularVelocity = axis * (angle * Mathf.Deg2Rad) / Time.deltaTime;
            var newVelocity = (_grabbedRigidbody.position - _oldGrabbedPosition) / Time.deltaTime;

            _angularVelocityList.Add(newAngularVelocity); 
            _velocityList.Add(newVelocity);
            
            _oldGrabbedPosition = _grabbedRigidbody.position;
            _oldGrabbedRotation = _grabbedRigidbody.rotation;
        }

        #endregion Late Update Sub Methods

        #region Input Handlers

        private void OnGrabReleased()
        {
            if (_grabbedObject)
            {
                _grabbedWasBlocked = _grabbedCollisionController && _grabbedCollisionController.IsBlocked();

                if (!_grabbedWasBlocked)
                {
                    DropGrabbedObject();
                }
            }
            else
            {
                GrabObject();
            }
        }

        private void OnUsePressed()
        {
            if (ProjectData.InteractionWithObjectsLocked || _input.IsTeleportActive)
            {
                return;
            }

            if (_uiPointer.CanClick() && _displayRaycaster.NearPointableObject)
            {
                _uiPointer.Press();
            }

            if (_grabbedObject && _grabbedObject.IsUsable && !_forceGrabbedObject)
            {
                _usedObject = _grabbedObject;
                _usedObject.UseStart(_playerController.Hand);
                SetCursorState(DesktopPlayerCursorState.Use);
            }
            else if (_lastTouchedInteractable && _lastTouchedInteractable.IsUsable && !_lastTouchedInteractable.IsUsed)
            {
                _usedObject = _lastTouchedInteractable;
                _usedObject.UseStart(_playerController.Hand);
                SetCursorState(DesktopPlayerCursorState.Use);
            }
        }

        private void OnUseReleased()
        {
            if (_uiPointer.CanClick())
            {
                _uiPointer.Release();
                return;
            }

            if (_usedObject)
            {
                _usedObject.UseEnd();
                SetCursorState(DesktopPlayerCursorState.Idle);
            }

            _usedObject = null;
        }

        #endregion Input Handlers

        #region Grab and Drod Methods

        private void GrabObject(bool forced = false)
        {
            if ((!ProjectData.InteractionWithObjectsLocked || forced) && _lastTouchedInteractable && (_lastTouchedInteractable.IsGrabbable || forced) && !_grabbedObject)
            {
                if (_lastTouchedCollider)
                {
                    _grabbedRigidbody = _lastTouchedCollider.attachedRigidbody;
                }

                if (!_grabbedRigidbody)
                {
                    _grabbedRigidbody = _lastTouchedCollider.GetComponentInParent<Rigidbody>(true);
                }

                if (!_grabbedRigidbody)
                {
                    return;
                }

                _grabbedAttachPoint = _lastTouchedInteractable.gameObject.GetComponentInChildren<VarwinAttachPoint>(true);

                var anchorPoint = !forced ? _raycastPosition : _lastTouchedInteractable.transform.position;

                if (_grabbedAttachPoint)
                {
                    _lastTouchedInteractable.transform.rotation = Quaternion.LookRotation(-_playerController.Hand.transform.forward, _playerController.Hand.transform.up);
                }
                else
                {
                    GetGrabbedGrabSettings();

                    if (_grabbedGrabSettings)
                    {
                        var grabPoint = _grabbedGrabSettings.AttachPoints.FirstOrDefault(a => a.RightGrabPoint);

                        if (grabPoint != null)
                        {
                            MoveObjectToAnchorPoint(grabPoint.RightGrabPoint, forced);
                            anchorPoint = grabPoint.RightGrabPoint.position;
                        }
                    }
                }

                _grabbedObject = _lastTouchedInteractable;
                _lastTouchedInteractable.GrabStart(_playerController.Hand);
                _grabbedCollisionController = _grabbedObject.GetComponent<CollisionController>();

                SetupHandRelativeParameters(_grabbedObject.transform.position, anchorPoint, _grabbedObject.transform.rotation, forced);

                SetCursorState(DesktopPlayerCursorState.Grab);

                _oldGrabbedPosition = _grabbedObject.transform.position;
                _oldGrabbedRotation = _grabbedObject.transform.rotation;
                _velocityList.Clear();
                _angularVelocityList.Clear();
            }

            _grabbedWasBlocked = false;
        }


        private GrabSettings GetGrabbedGrabSettings()
        {
            _grabbedGrabSettings = _lastTouchedInteractable.gameObject.GetComponentInChildren<GrabSettings>();
            return _grabbedGrabSettings;
        }

        public void ForceGrabObject(GameObject gameObject)
        {
            var interactableToGrab = gameObject.GetComponent<NettleDeskInteractableObject>();
            Collider collider = gameObject.GetComponent<Collider>();

            if (_grabbedObject != _forceGrabbedObject)
            {
                DropGrabbedObject();
            }

            if (interactableToGrab && _grabbedObject != interactableToGrab)
            {
                if (_lastTouchedInteractable)
                {
                    _lastTouchedInteractable.TouchEnd();
                }

                _lastTouchedInteractable = interactableToGrab;
                _lastTouchedCollider = collider ? collider : gameObject.GetComponentInChildren<Collider>();

                _currentCursorDistance = ForceGrabCursorDistance;

                GetGrabbedGrabSettings();

                var canDetachCursorOnForceGrab = _grabbedGrabSettings && _grabbedGrabSettings.DetachCursor;
                var centerOfScreen = GetCenterOfScreen();
                var detachRelativeGrabPosition = CastToNettleDeskScreen(canDetachCursorOnForceGrab ? centerOfScreen : CursorPos);
                var cursorOffset = GetCursorWorldPoint(detachRelativeGrabPosition, _currentCursorDistance);

                gameObject.transform.position = cursorOffset;
                _forceGrabbedObject = gameObject;
                GrabObject(true);
            }
        }

        public void ForceDropObject(GameObject gameObject)
        {
            if (_forceGrabbedObject == gameObject)
            {
                ForceDropObject();
            }
        }

        public void ForceDropObject()
        {
            if (_forceGrabbedObject || _grabbedObject)
            {
                _forceGrabbedObject = null;
                _grabbedObject.GrabEnd();
                _grabbedObject = null;
            }

            DropGrabbedObject();
        }

        public void DropGrabbedObject()
        {
            if (_forceGrabbedObject)
            {
                return;
            }

            StopGrab();
        }

        private void StopGrab()
        {
            if (!_grabbedObject)
            {
                return;
            }

            if (_grabbedRigidbody)
            {
                var grabbedVelocity = Vector3.zero;
                var grabbedAngularVelocity = Vector3.zero;

                foreach (var vector3 in _velocityList)
                {
                    grabbedVelocity += vector3;
                }

                foreach (var vector3 in _angularVelocityList)
                {
                    grabbedAngularVelocity += vector3;
                }

                grabbedVelocity /= _velocityList.Count;
                grabbedAngularVelocity /= _angularVelocityList.Count;

                grabbedVelocity = _velocityList.Count == 0 ? Vector3.zero : grabbedVelocity / _velocityList.Count;
                grabbedAngularVelocity = _angularVelocityList.Count == 0 ? Vector3.zero : grabbedAngularVelocity / _angularVelocityList.Count;
            }

            _grabbedObject.GrabEnd();

            _grabbedObject = null;
            _grabbedCollisionController = null;
            _grabbedWasBlocked = false;

            _grabbedGrabSettings = null;
            _grabbedAttachPoint = null;

            SetCursorState(DesktopPlayerCursorState.Idle);
        }

        #endregion Grab and Drod Methods

        private void MoveObjectToAnchorPoint(Transform anchorPoint, bool forceGrabInit = false)
        {
            var canDetachCursorOnForceGrab = forceGrabInit && _grabbedGrabSettings && _grabbedGrabSettings.DetachCursor;
            var cursorPosition = canDetachCursorOnForceGrab ? new Vector3(Screen.width / 2f, Screen.height / 2f) : MouseInput.MousePosition;
            var ray = _camera.ScreenPointToRay(cursorPosition);
            var grabDistance = ClampCursorDistance(Vector3.Distance(anchorPoint.position, _camera.transform.position) + 0.1f * Input.mouseScrollDelta.y);

            var grabPointPosition = ray.GetPoint(grabDistance);

            var grabRotationOffset = Quaternion.Inverse(anchorPoint.rotation) * _lastTouchedInteractable.transform.rotation;
            
            _lastTouchedInteractable.transform.rotation = _camera.transform.rotation * grabRotationOffset;
            _lastTouchedInteractable.transform.position = _lastTouchedInteractable.transform.position - anchorPoint.position + grabPointPosition;
            
            _objectPositionOffset = Vector3.zero;
            _objectRotationOffset = Quaternion.identity;
        }
        
        private void SetupHandRelativeParameters(Vector3 position, Vector3 anchorPoint, Quaternion rotation, bool forceGrabInit = false)
        {
            var canDetachCursorOnForceGrab = forceGrabInit && _grabbedGrabSettings && _grabbedGrabSettings.DetachCursor;
            var cursorPosition = canDetachCursorOnForceGrab ? new Vector3(Screen.width / 2f, Screen.height / 2f) : MouseInput.MousePosition;

            _targetCursorDistance = ClampCursorDistance(Vector3.Distance(anchorPoint, _camera.transform.position) + 0.1f * Input.mouseScrollDelta.y);
            _currentCursorDistance = _targetCursorDistance;
            _interactionCursorPosition = cursorPosition;

            var cursorWorldPosition = _camera.ScreenPointToRay(cursorPosition).GetPoint(_currentCursorDistance);

            var cameraInversedRotation = Quaternion.Inverse(_camera.transform.rotation);
            _objectRotationOffset = cameraInversedRotation * rotation;
            _objectPositionOffset = Quaternion.Inverse(_camera.transform.rotation * _objectRotationOffset) * (position - cursorWorldPosition);
        }

        private void SetCursorState(DesktopPlayerCursorState state)
        {
            if (!this)
            {
                return;
            }

            _idleCursor.gameObject.SetActive(state == DesktopPlayerCursorState.Idle);
            _touchCursor.gameObject.SetActive(state == DesktopPlayerCursorState.Touch);
            _useCursor.gameObject.SetActive(state == DesktopPlayerCursorState.Use || state == DesktopPlayerCursorState.Pointer);
            _grabCursor.gameObject.SetActive(state == DesktopPlayerCursorState.Grab);

            if (state == DesktopPlayerCursorState.Pointer && _uiPointer.CanClick())
            {
                _useCursor.GetPropertyBlock(_useCursorMaterialPropertyBlock);
                _useCursorMaterialPropertyBlock.SetColor("_MainColor", Color.cyan);
                _useCursor.SetPropertyBlock(_useCursorMaterialPropertyBlock);
            }
            else
            {
                _useCursor.GetPropertyBlock(_useCursorMaterialPropertyBlock);
                _useCursorMaterialPropertyBlock.SetColor("_MainColor", Color.white);
                _useCursor.SetPropertyBlock(_useCursorMaterialPropertyBlock);
            }
        }

        private float ClampCursorDistance(float cursorDistance)
        {
            return Mathf.Clamp(cursorDistance, minGrabDistance, maxGrabDistance);
        }
        
        private Vector3 GetCursorWorldPoint(Vector3 screenPosition, float distance)
        {
            screenPosition.z = ClampCursorDistance(distance);
            return _camera.ScreenToWorldPoint(screenPosition);
        }
    }
}