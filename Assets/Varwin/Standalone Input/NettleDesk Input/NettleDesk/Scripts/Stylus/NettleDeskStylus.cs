using System.Linq;
using UnityEngine;
using Varwin.NettleDesk;
using Varwin.PlatformAdapter;
using Varwin.Public;

namespace Varwin.NettleDeskPlayer
{
    /// <summary>
    /// Компонент, реализующий поведение стилуса.
    /// </summary>
    public class NettleDeskStylus : NettleDeskControllerBase
    {
        /// <summary>
        /// Размер cписка элементов для фильтра.
        /// </summary>
        private const int FilterListSize = 5;

        /// <summary>
        /// Левый ли контроллер.
        /// </summary>
        [SerializeField] private bool _isLeft;

        /// <summary>
        /// Рейкастер.
        /// </summary>
        [SerializeField] private NettleDeskRaycaster _raycaster;

        public RaycastHit? ActiveRaycastHit => _raycaster.NearObjectRaycastHit;

        /// <summary>
        /// Левый ли контроллер.
        /// </summary>
        public bool IsLeft => _isLeft;

        /// <summary>
        /// Проинициализирован ли контроллер.
        /// </summary>
        public bool Initialized { get; private set; }
        
        /// <summary>
        /// Обработчик контроллера.
        /// </summary>
        public NettleDeskStylusInputHandler InputHandler;

        /// <summary>
        /// Текущий взятый в руку объект.
        /// </summary>
        public NettleDeskInteractableObject GrabbedObject { get; private set; }

        /// <summary>
        /// Текущий касаемый рукой объект.
        /// </summary>
        public NettleDeskInteractableObject TouchedObject { get; private set; }

        /// <summary>
        /// Текущий используемый рукой объект.
        /// </summary>
        public NettleDeskInteractableObject UsedObject { get; private set; }

        /// <summary>
        /// Радиус каста.
        /// </summary>
        public float RadiusCast = 0.075f;

        /// <summary>
        /// Позиция объекта относительно руки.
        /// </summary>
        private Vector3 _grabbedPositionOffset;

        /// <summary>
        /// Поворот объекта относительно руки.
        /// </summary>
        private Quaternion _grabbedRotationOffset;

        /// <summary>
        /// Дополнительный интерфейс грабнутого объекта.
        /// </summary>
        private IGrabbingAware _grabbingAware;

        /// <summary>
        /// Указатель.
        /// </summary>
        public NettleDeskStylusDistancePointer DistancePointer;

        /// <summary>
        /// Предыдущая позиция.
        /// </summary>
        private Vector3 _oldGrabbedPosition;
        
        /// <summary>
        /// Предыдущий поворот.
        /// </summary>
        private Quaternion _oldGrabbedRotation;

        /// <summary>
        /// Скорость перемещения объекта.
        /// </summary>
        private FixedList<Vector3> _velocityList;

        /// <summary>
        /// Угловая скорость перемещения объекта.
        /// </summary>
        private FixedList<Vector3> _angularVelocityList;
        
        /// <summary>
        /// Кандидат на взятие в руку.
        /// </summary>
        private NettleDeskInteractableObject _hoveredInteractableObject;

        /// <summary>
        /// Инициализация работы с физикой.
        /// </summary>
        private void Start()
        {
            Initialized = true;
            _velocityList = new FixedList<Vector3>(FilterListSize);
            _angularVelocityList = new FixedList<Vector3>(FilterListSize);
            
            InputAdapter.Instance.PlayerController.PlayerTeleported += OnPlayerTeleported;
            InputAdapter.Instance.PlayerController.PlayerRotated += OnPlayerRotated;

            InputHandler.Initialized += OnInitialized;
            InputHandler.Deinitialized += OnDeinitialized;
            InputHandler.PoseChanged += OnPoseChanged;
            InputHandler.UsePressed += OnUsePressed;
            InputHandler.UsePressed += OnUseReleased;
            InputHandler.GrabPressed += OnGrabPressed;
            InputHandler.GrabReleased += OnGrabReleased;
        }

        /// <summary>
        /// Метод, вызываемый при уничтожении.
        /// </summary>
        private void OnDeinitialized()
        {
            ForceDrop();
            ForceUnTouch();
            ForceUnUse();
            
            Initialized = false;
        }

        /// <summary>
        /// Метод, вызываемый при инициализации.
        /// </summary>
        private void OnInitialized()
        {

        }

        /// <summary>
        /// При нажатии на триггер.
        /// </summary>
        private void OnUsePressed()
        {
            if (GrabbedObject)
            {
                UseObject(GrabbedObject);
            }
            else
            {
                if (_hoveredInteractableObject && !UsedObject)
                {
                    UseObject(_hoveredInteractableObject);
                }
            }
        }

        /// <summary>
        /// При отпускании триггера.
        /// </summary>
        private void OnUseReleased()
        {
            UnUseObject();
        }

        /// <summary>
        /// При нажатии на захват.
        /// </summary>
        private void OnGrabPressed()
        {
            if (GrabbedObject && (GrabbedObject.GrabSettings && GrabbedObject.GrabSettings.GrabType == GrabSettings.GrabTypes.Toggle) && !ForcedGrab)
            {
                ForceDrop();    
                return;
            }
            
            if (!_hoveredInteractableObject || GrabbedObject)
            {
                return;
            }

            GrabObject(_hoveredInteractableObject);
        }

        /// <summary>
        /// При отпускании захвата.
        /// </summary>
        private void OnGrabReleased()
        {
            if (!GrabbedObject || ForcedGrab || !GrabbedObject.CanBeDrop || (GrabbedObject.GrabSettings && GrabbedObject.GrabSettings.GrabType == GrabSettings.GrabTypes.Toggle))
            {
                return;
            }

            ForceDrop();
        }

        /// <summary>
        /// При повороте игрока.
        /// </summary>
        /// <param name="newRotation">Новый поворот.</param>
        /// <param name="oldRotation">Старый поворот.</param>
        private void OnPlayerRotated(Quaternion newRotation, Quaternion oldRotation)
        {
            ForceUpdateTransform();
        }

        /// <summary>
        /// При телепортировании игрока.
        /// </summary>
        /// <param name="position">Новая позиция.</param>
        private void OnPlayerTeleported(Vector3 position)
        {
            ForceUpdateTransform();
        }

        /// <summary>
        /// Быстрое перемещение объекта.
        /// </summary>
        private void ForceUpdateTransform()
        {
            if (!GrabbedObject)
            {
                return;
            }

            var targetPosition = transform.TransformPoint(_grabbedPositionOffset);
            var targetRotation = transform.rotation * _grabbedRotationOffset;

            GrabbedObject.transform.position = targetPosition;
            GrabbedObject.transform.rotation = targetRotation;
            
            _oldGrabbedPosition = targetPosition;
            _oldGrabbedRotation = targetRotation;

            _grabbingAware?.OnHandTransformChanged(Vector3.zero, Vector3.zero);
        }
        
        /// <summary>
        /// Обработка логической составляющей.
        /// </summary>
        private void Update()
        {
            if (!Initialized)
            {
                return;
            }
            
            if (ProjectData.InteractionWithObjectsLocked)
            {
                if (UsedObject)
                {
                    ForceUnUse();
                }
            }

            ProcessGrabbedObject();
        }

        /// <summary>
        /// Обработка касания.
        /// </summary>
        private void ProcessTouch()
        {
            if (GrabbedObject)
            {
                return;
            }
            
            if (ProjectData.InteractionWithObjectsLocked)
            {
                if (TouchedObject)
                {
                    UnTouchObject();
                }

                return;
            }

            if (_hoveredInteractableObject)
            {
                TouchObject(_hoveredInteractableObject);
            }
            else
            {
                UnTouchObject();
            }
        }

        /// <summary>
        /// Обработка физической составляющей.
        /// </summary>
        private void FixedUpdate()
        {
            if (!Initialized)
            {
                return;
            }

            ProcessFindCandidate();
            ProcessTouch();
        }
        
        /// <summary>
        /// Процесс поиска кандидата на граб.
        /// </summary>
        private void ProcessFindCandidate()
        {
            if (ProjectData.InteractionWithObjectsLocked)
            {
                _hoveredInteractableObject = null;
                return;
            }
            
            _raycaster.RaycastAndOverlap(transform.position, transform.forward, transform.position, RadiusCast);
            _hoveredInteractableObject = _raycaster.NearInteractableObject;
        }

        /// <summary>
        /// Взять в руку.
        /// </summary>
        /// <param name="interactableObject">Интерактивный объект.</param>
        /// <param name="force">ForceGrab.</param>
        private void GrabObject(NettleDeskInteractableObject interactableObject, bool force = false)
        {
            if (TouchedObject != interactableObject)
            {
                UnTouchObject();
            }

            if (!interactableObject || (!interactableObject.IsGrabbable && !force))
            {
                return;
            }
            
            if (!interactableObject.Rigidbody)
            {
                return;
            }

            if (interactableObject.GrabbedBy && interactableObject.GrabbedBy.ForcedGrab)
            {
                return;
            }

            if (interactableObject.IsGrabbed)
            {
                interactableObject.GrabbedBy.ForceDrop();
            }
            
            if (force)
            {
                interactableObject.transform.position = transform.position;
                interactableObject.transform.rotation = transform.rotation;
            }

            var grabSettings = interactableObject.GrabSettings;
            if (!grabSettings || grabSettings.AttachPoints == null || grabSettings.AttachPoints.Length == 0)
            {
                _grabbedPositionOffset = transform.InverseTransformPoint(interactableObject.transform.position);
                _grabbedRotationOffset = Quaternion.Inverse(transform.rotation) * interactableObject.transform.rotation;
            }
            else
            {
                var firstGrabSettings = grabSettings.AttachPoints.FirstOrDefault();
                var attachPoint = IsLeft ? firstGrabSettings.LeftGrabPoint : firstGrabSettings.RightGrabPoint;

                if (!attachPoint)
                {
                    _grabbedPositionOffset = transform.InverseTransformPoint(interactableObject.transform.position);
                    _grabbedRotationOffset = Quaternion.Inverse(transform.rotation) * interactableObject.transform.rotation;
                }
                else
                {
                    _grabbedPositionOffset = Quaternion.Inverse(attachPoint.rotation) * (interactableObject.transform.position - attachPoint.transform.position);
                    _grabbedRotationOffset = Quaternion.Inverse(attachPoint.rotation) * interactableObject.transform.rotation;
                }
            }

            GrabbedObject = interactableObject;
            if (GrabbedObject)
            {
                GrabbedObject.GrabStart(this);
                _grabbingAware = GrabbedObject.GetComponent<IGrabbingAware>();
                _oldGrabbedPosition = GrabbedObject.Rigidbody.position;
                _oldGrabbedRotation = GrabbedObject.Rigidbody.rotation;
                _velocityList.Clear();
                _angularVelocityList.Clear();
                if (!force)
                {
                    return;
                }
                
                ForcedGrab = true;
                GrabbedObject.IsForceGrabbed = true;
            }
        }

        /// <summary>
        /// Коснуться объекта.
        /// </summary>
        /// <param name="interactableObject">Интерактивный объект.</param>
        private void TouchObject(NettleDeskInteractableObject interactableObject)
        {
            if (TouchedObject == interactableObject)
            {
                return;
            }
            
            UnTouchObject();
            
            if (!interactableObject || interactableObject.IsTouched)
            {
                return;
            }

            TouchedObject = interactableObject;
            if (TouchedObject)
            {
                TouchedObject.TouchStart(this);
            }
        }

        /// <summary>
        /// Перестать касаться объекта.
        /// </summary>
        private void UnTouchObject()
        {
            if (!TouchedObject)
            {
                return;
            }

            TouchedObject.TouchEnd();
            TouchedObject = null;
        }

        /// <summary>
        /// Коснуться объекта.
        /// </summary>
        /// <param name="interactableObject">Интерактивный объект.</param>
        private void UseObject(NettleDeskInteractableObject interactableObject)
        {
            if (!interactableObject || interactableObject.IsUsed || !interactableObject.IsUsable)
            {
                return;
            }

            UsedObject = interactableObject;
            UsedObject.UseStart(this);
        }

        /// <summary>
        /// Перестать касаться объекта.
        /// </summary>
        private void UnUseObject()
        {
            if (!UsedObject)
            {
                return;
            }

            UsedObject.UseEnd();
            UsedObject = null;
        }

        /// <summary>
        /// Обработка взятого в руку объекта.
        /// </summary>
        private void ProcessGrabbedObject()
        {
            if (!GrabbedObject)
            {
                return;
            }
            
            if (ProjectData.InteractionWithObjectsLocked)
            {
                ForceDrop();
                return;
            }

            var targetPosition = transform.TransformPoint(_grabbedPositionOffset);
            var targetRotation = transform.rotation * _grabbedRotationOffset;

            if (GrabbedObject.Rigidbody.isKinematic)
            {
                GrabbedObject.transform.position = targetPosition;
                GrabbedObject.transform.rotation = targetRotation;
            }
            else
            {
                GrabbedObject.Rigidbody.position = targetPosition;
                GrabbedObject.Rigidbody.rotation = targetRotation;
                GrabbedObject.transform.position = targetPosition;
                GrabbedObject.transform.rotation = targetRotation;
                GrabbedObject.Rigidbody.velocity = Vector3.zero;
                GrabbedObject.Rigidbody.angularVelocity = Vector3.zero;
                GrabbedObject.Rigidbody.ResetInertiaTensor();
            }

            var rotationDelta = GrabbedObject.Rigidbody.rotation * Quaternion.Inverse(_oldGrabbedRotation);
            rotationDelta.ToAngleAxis(out var angle, out var axis);
            if (angle > 180f)
            {
                angle -= 360f;
            }

            var newAngularVelocity = (angle * axis * Mathf.Deg2Rad) / Time.fixedDeltaTime;
            var newVelocity = (GrabbedObject.Rigidbody.position - _oldGrabbedPosition) / Time.fixedDeltaTime;

            _angularVelocityList.Add(newAngularVelocity); 
            _velocityList.Add(newVelocity);

            _oldGrabbedPosition = GrabbedObject.Rigidbody.position;
            _oldGrabbedRotation = GrabbedObject.Rigidbody.rotation;
            _grabbingAware?.OnHandTransformChanged(newAngularVelocity, newVelocity);
        }

        /// <summary>
        /// Выбросить из руки объект. 
        /// </summary>
        public override void ForceDrop()
        {
            if (!GrabbedObject)
            {
                return;
            }

            Vector3 grabbedVelocity = Vector3.zero;
            Vector3 grabbedAngularVelocity = Vector3.zero;
            
            for (int i = 0; i < _velocityList.Count; i++)
            {
                grabbedVelocity += _velocityList[i];
            }

            for (int i = 0; i < _angularVelocityList.Count; i++)
            {
                grabbedAngularVelocity += _angularVelocityList[i];
            }

            grabbedVelocity /= _velocityList.Count;
            grabbedAngularVelocity /= _angularVelocityList.Count;
            
            GrabbedObject.Rigidbody.velocity = grabbedVelocity;
            GrabbedObject.Rigidbody.angularVelocity = grabbedAngularVelocity;
            GrabbedObject.GrabEnd();
            GrabbedObject = null;
            _grabbingAware = null;
            ForcedGrab = false;
        }

        /// <summary>
        /// Перестать использовать объект.
        /// </summary>
        public override void ForceUnUse()
        {
            if (!UsedObject)
            {
                return;
            }

            UsedObject.UseEnd();
            UsedObject = null;
        }

        /// <summary>
        /// Перестать касаться объекта.
        /// </summary>
        public override void ForceUnTouch()
        {
            if (!TouchedObject)
            {
                return;
            }

            TouchedObject.TouchEnd();
            TouchedObject = null;
        }

        /// <summary>
        /// Принудительно взять объект в руку.
        /// </summary>
        /// <param name="targetGameObject">Объект для взятия в руку.</param>
        public void ForceGrab(GameObject targetGameObject)
        {
            if (!targetGameObject)
            {
                return;
            }

            var interactableObject = targetGameObject.GetComponentInParent<NettleDeskInteractableObject>(true);
            if (!interactableObject)
            {
                return;
            }

            GrabObject(interactableObject, true);
        }
        
        /// <summary>
        /// При изменении позы в пространстве.
        /// </summary>
        /// <param name="pose">Поза.</param>
        private void OnPoseChanged(Pose pose)
        {
            transform.position = pose.position;
            transform.rotation = pose.rotation;
        }

        /// <summary>
        /// При уничтожении объекта.
        /// </summary>
        private void OnDestroy()
        {
            if (GrabbedObject)
            {
                ForceDrop();
            }

            if (UsedObject)
            {
                UnUseObject();
            }

            if (TouchedObject)
            {
                UnTouchObject();
            }

            InputAdapter.Instance.PlayerController.PlayerTeleported -= OnPlayerTeleported;
            InputAdapter.Instance.PlayerController.PlayerRotated -= OnPlayerRotated;

            InputHandler.Initialized -= OnInitialized;
            InputHandler.Deinitialized -= OnDeinitialized;
            InputHandler.PoseChanged -= OnPoseChanged;
            InputHandler.UsePressed -= OnUsePressed;
            InputHandler.UsePressed -= OnUseReleased;
            InputHandler.GrabPressed -= OnGrabPressed;
            InputHandler.GrabReleased -= OnGrabReleased;
        }
    }
}