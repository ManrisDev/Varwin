using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Varwin.ObjectsInteractions;
using Varwin.Public;
using Varwin.DesktopInput;
using Varwin.PlatformAdapter;

namespace Varwin.DesktopPlayer
{
    public enum DesktopPlayerCursorState
    {
        Idle,
        Touch,
        Use,
        Grab,
        Pointer
    }
    
    public class DesktopPlayerInteractionController : MonoBehaviour
    {
        private const int FilterListCount = 5;

        [Header("Config")] 
        [SerializeField] private DesktopPlayerInteractionConfig _config;
        [SerializeField] private DesktopLocomotionController _locomotionController;
        [SerializeField] private DesktopUIPointer _uiPointer;

        [Header("Cursor")]
        [SerializeField] private RectTransform _cursorPivot;

        [Space]
        [SerializeField] private Image _idleCursor; 
        [SerializeField] private Image _touchCursor;
        [SerializeField] private Image _useCursor;
        [SerializeField] private Image _grabCursor;

        [Header("Raycaster")]
        [SerializeField] private DesktopPlayerRaycaster _raycaster;

        public bool IsRotatingObject { get; private set; }

        private float _maxGrabDistance;
        private Vector3 _interactionCursorPosition;

        private float _currentCursorDistance;
        private float _targetCursorDistance;
        
        private DesktopPlayerController _playerController;
        private Camera _camera;

        private DesktopInteractableObject _lastTouchedInteractable;
        private Collider _lastTouchedCollider;

        private DesktopInteractableObject _grabbedObject;
        private DesktopInteractableObject _usedObject;
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

        private DesktopPlayerInput _input;

        private GameObject _forceGrabbedObject;
        
        private Vector3 _oldGrabbedPosition;
        private Quaternion _oldGrabbedRotation;

        private readonly FixedList<Vector3> _velocityList = new(FilterListCount);
        private readonly FixedList<Vector3> _angularVelocityList = new(FilterListCount);

        private void Awake()
        {
            _playerController = GetComponent<DesktopPlayerController>();
            _camera = _playerController.PlayerCamera;
            _input = GetComponent<DesktopPlayerInput>();
            _maxGrabDistance = _config.MaxGrabDistance;

            _idleCursor.sprite = _config.IdleCursor;
            _touchCursor.sprite = _config.TouchCursor;
            _useCursor.sprite = _config.UseCursor;
            _grabCursor.sprite = _config.GrabCursor;

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
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            DropGrabbedObject();
            ForceDropObject();

            _input.GrabReleased -= OnGrabReleased;
            _input.UsePressed -= OnUsePressed;
            _input.UseReleased -= OnUseReleased;
            
            Cursor.visible = true;
            ProjectData.UIOverlapStatusChaned -= OnUIOverlapChangeStatus;
            OnUIOverlapChangeStatus(false);
        }

        private void OnPositionUpdated()
        {
            _maxGrabDistance = DistancePointer.CustomSettings ? DistancePointer.CustomSettings.Distance + _config.GrabOffset : _config.MaxGrabDistance;
            
            _cursorPivot.anchoredPosition = MouseInput.MousePosition - 0.5f * new Vector3(Screen.width, Screen.height, 0);
            _targetCursorDistance = ClampCursorDistance(_targetCursorDistance + 0.05f * Input.mouseScrollDelta.y);
            _currentCursorDistance = Mathf.Lerp(_currentCursorDistance, _targetCursorDistance, 12f * Time.deltaTime);

            UpdateUiPointer();
            
            if (_grabbedObject && (!_grabbedCollisionController || !_grabbedCollisionController.IsBlocked()) && _grabbedWasBlocked)
            {
                DropGrabbedObject();
            }

            IsRotatingObject = _grabbedObject && Input.GetMouseButton(2) && !_grabbedObject.HasVarwinAttachPoint;
            if (IsRotatingObject)
            {
                float x = _config.ObjectRotationSpeed * Input.GetAxis("Mouse X");
                float y = _config.ObjectRotationSpeed * Input.GetAxis("Mouse Y");

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

        public GameObject GetGrabbedObject()
        {
            return _grabbedObject ? _grabbedObject.gameObject : null;
        }
        
        public void SetCursorVisibility(bool cursorVisibility)
        {
            _cursorPivot.gameObject.SetActive(cursorVisibility);
        }

        public void OnUIOverlapChangeStatus(bool popupIsActive) => _layoutPopupIsActive = popupIsActive;

        #region Update Sub Methods
        
        private void UpdateUiPointer()
        {
            if (_input.IsTeleportActive)
            {
                return;
            }
            
            if (_uiPointer.CanClick() && _raycaster.NearPointableObject)
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

        private bool HitAnyInteractableObject() => _raycaster.NearInteractableObject;

        private void TouchClosestObject()
        {
            if (_raycaster.NearObjectRaycastHit == null)
            {
                return;
            }

            var hit = _raycaster.NearObjectRaycastHit.Value;

            _raycastPosition = hit.point;
                
            if (hit.collider == _lastTouchedCollider)
            {
                return;
            }
                
            var interactable = _raycaster.NearInteractableObject;
            if (!interactable)
            {
                SetCursorState(DesktopPlayerCursorState.Idle);
                return;
            }

            if (interactable != _lastTouchedInteractable)
            {
                if (_lastTouchedInteractable)
                {
                    _lastTouchedInteractable.StopTouch();
                }

                interactable.StartTouch();

                _lastTouchedInteractable = interactable;
                _lastTouchedCollider = hit.collider;

                SetCursorState(interactable.IsGrabbable ? DesktopPlayerCursorState.Grab :
                    interactable.IsUsable ? DesktopPlayerCursorState.Use : DesktopPlayerCursorState.Idle);
            }
        }

        private void ForgetDroppedObject()
        {
            if (_lastTouchedInteractable)
            {
                _lastTouchedInteractable.StopTouch();

                SetCursorState(DesktopPlayerCursorState.Idle);
            }

            _lastTouchedInteractable = null;
            _lastTouchedCollider = null;

            _raycastPosition = _camera.transform.position;
        }
        
        #endregion Update Sub Methods
        
        #region Late Update Sub Methods
        
        private bool InteractingWithObject() => _grabbedObject || _lastTouchedInteractable && _lastTouchedInteractable.IsUsing();

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
            
            if (_uiPointer.CanClick())
            {
                _uiPointer.Press();
            }

            if (_grabbedObject && _grabbedObject.IsUsable && !_forceGrabbedObject)
            {
                _usedObject = _grabbedObject;
                _usedObject.StartUse(_playerController.Hand);
                SetCursorState(DesktopPlayerCursorState.Use);
            }
            else if (_lastTouchedInteractable && _lastTouchedInteractable.IsUsable && !_lastTouchedInteractable.IsUsing())
            {
                _usedObject = _lastTouchedInteractable;
                _usedObject.StartUse(_playerController.Hand);
                SetCursorState(DesktopPlayerCursorState.Use);
            }
        }

        private void OnUseReleased()
        {
            if (_uiPointer.CanClick())
            {
                _uiPointer.Release();
            }

            if (_usedObject)
            {
                _usedObject.StopUse();
                SetCursorState(DesktopPlayerCursorState.Idle);
            }

            _usedObject = null;
        }

        private void FixedUpdate()
        {
            _raycaster.Raycast(_camera.ScreenPointToRay(MouseInput.MousePosition));
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
                _lastTouchedInteractable.StartGrab(_playerController.Hand);

                var grabbedObjectIsDeleting = _lastTouchedInteractable.gameObject.GetWrapper()?.GetObjectController()?.IsDeleted ?? true;
                if (grabbedObjectIsDeleting)
                {
                    _grabbedWasBlocked = false;
                    return;
                }

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
            _grabbedGrabSettings = _lastTouchedInteractable.gameObject.GetComponentInChildren<GrabSettings>(true);
            return _grabbedGrabSettings;
        }
        
        public void ForceGrabObject(GameObject gameObject)
        {
            DesktopInteractableObject interactableToGrab = gameObject.GetComponent<DesktopInteractableObject>();
            Collider collider = gameObject.GetComponent<Collider>();

            if (_grabbedObject != _forceGrabbedObject)
            {
                DropGrabbedObject();
            }

            if (interactableToGrab && _grabbedObject != interactableToGrab)
            {
                if (_lastTouchedInteractable)
                {
                    _lastTouchedInteractable.StopTouch();
                }

                _lastTouchedInteractable = interactableToGrab;
                _lastTouchedCollider = collider ? collider : gameObject.GetComponentInChildren<Collider>(true);

                _currentCursorDistance = _config.ForceGrabCursorDistance;

                GetGrabbedGrabSettings();

                var canDetachCursorOnForceGrab = _grabbedGrabSettings && _grabbedGrabSettings.DetachCursor;
                var detachRelativeGrabPosition = canDetachCursorOnForceGrab ? new Vector3(Screen.width / 2, Screen.height / 2) : MouseInput.MousePosition;
                var cursorOffset = GetCursorWorldPoint(detachRelativeGrabPosition, _currentCursorDistance);

                gameObject.transform.position = cursorOffset;
                _forceGrabbedObject = gameObject;
                interactableToGrab.IsForceGrabbed = true;
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
                _grabbedObject.StopGrab();
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
                
                grabbedVelocity = _velocityList.Count == 0 ? Vector3.zero : grabbedVelocity / _velocityList.Count;
                grabbedAngularVelocity = _angularVelocityList.Count == 0 ? Vector3.zero : grabbedAngularVelocity / _angularVelocityList.Count;

                _grabbedRigidbody.angularVelocity = grabbedAngularVelocity;
                _grabbedRigidbody.velocity = grabbedVelocity;
            }
            
            _grabbedObject.StopGrab();

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

            if (ProjectData.IsMultiplayerSceneActive)
            {
                _idleCursor.gameObject.SetActive(true);
                return;                
            }

            _idleCursor.gameObject.SetActive(state == DesktopPlayerCursorState.Idle);
            _touchCursor.gameObject.SetActive(state == DesktopPlayerCursorState.Touch);
            _useCursor.gameObject.SetActive(state == DesktopPlayerCursorState.Use || state == DesktopPlayerCursorState.Pointer);
            _grabCursor.gameObject.SetActive(state == DesktopPlayerCursorState.Grab);

            if (state == DesktopPlayerCursorState.Pointer && _uiPointer.CanClick())
            {
                _useCursor.GetComponent<Image>().color = Color.cyan;
            }
            else
            {
                _useCursor.GetComponent<Image>().color = Color.white;
            }
        }
        
        private float ClampCursorDistance(float cursorDistance)
        {
            return Mathf.Clamp(cursorDistance, _config.MinGrabDistance, _maxGrabDistance);
        }

        private Vector3 GetCursorWorldPoint(Vector3 screenPosition, float distance)
        {
            screenPosition.z = ClampCursorDistance(distance);
            return _camera.ScreenToWorldPoint(screenPosition);
        }
    }
}
