using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Varwin.ObjectsInteractions;
using Varwin.PlatformAdapter;
using Varwin.Public;

namespace Varwin.XR
{
    /// <summary>
    /// Класс, описываюший работу взаимодействия руки с объектами.
    /// </summary>
    public class VarwinXRController : MonoBehaviour
    {
        /// <summary>
        /// Размер списка для фильтра.
        /// </summary>
        private const int FilterListCount = 5;

        /// <summary>
        /// Обработчик контроллера.
        /// </summary>
        public VarwinXRControllerInputHandler InputHandler;

        /// <summary>
        /// Текущий взятый в руку объект.
        /// </summary>
        public VarwinXRInteractableObject GrabbedObject { get; private set; }

        /// <summary>
        /// Текущий касаемый рукой объект.
        /// </summary>
        public VarwinXRInteractableObject TouchedObject { get; private set; }

        /// <summary>
        /// Текущий используемый рукой объект.
        /// </summary>
        public VarwinXRInteractableObject UsedObject { get; private set; }

        /// <summary>
        /// Радиус каста.
        /// </summary>
        public float RadiusCast = 0.075f;

        /// <summary>
        /// XR устройство для отслеживания событий контроллера.
        /// </summary>
        public InputDevice InputDevice { get; private set; }

        /// <summary>
        /// Левый ли контроллер.
        /// </summary>
        [SerializeField] private bool _isLeft;

        /// <summary>
        /// Левый ли контроллер.
        /// </summary>
        public bool IsLeft => _isLeft;

        /// <summary>
        /// Форсграбнутый ли объект.
        /// </summary>
        public bool ForcedGrab { get; private set; }

        /// <summary>
        /// Модель контроллера.
        /// </summary>
        public VarwinXRControllerModel ControllerModel { get; set; }

        /// <summary>
        /// Кандидат на взятие в руку.
        /// </summary>
        private VarwinXRInteractableObject _hoveredInteractableObject;

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
        /// Инициализирован ли.
        /// </summary>
        public bool Initialized => ControllerModel;

        /// <summary>
        /// Является ли трехосевым.
        /// </summary>
        public bool Is3Dof => ControllerModel && ControllerModel.Is3Dof;

        /// <summary>
        /// Расчет вспомогательной длины указки.
        /// </summary>
        public float RaycastDistanceOffset => Is3Dof ? 1f : 0;

        /// <summary>
        /// Луч контроллера для обработки всех столкновений с объектами.
        /// </summary>
        public VarwinXRControllerRaycaster ControllerRaycaster;
        
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
        /// Инициализация работы с физикой.
        /// </summary>
        private void Start()
        {
            _velocityList = new FixedList<Vector3>(FilterListCount);
            _angularVelocityList = new FixedList<Vector3>(FilterListCount);

            InputAdapter.Instance.PlayerController.PlayerTeleported += OnPlayerTeleported;
            InputAdapter.Instance.PlayerController.PlayerRotated += OnPlayerRotated;

            InputHandler.TriggerPressed += OnTriggerPressed;
            InputHandler.TriggerReleased += OnTriggerReleased;
            InputHandler.GripPressed += OnGripPressed;
            InputHandler.GripReleased += OnGripReleased;
        }

        /// <summary>
        /// При нажатии на триггер.
        /// </summary>
        /// <param name="sender">Компонент, который вызвал событие.</param>
        private void OnTriggerPressed(VarwinXRControllerInputHandler sender)
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
        /// <param name="sender">Компонент, который вызвал событие.</param>
        private void OnTriggerReleased(VarwinXRControllerInputHandler sender)
        {
            UnUseObject();
        }

        /// <summary>
        /// При нажатии на захват.
        /// </summary>
        /// <param name="sender">Компонент, который вызвал событие.</param>
        private void OnGripPressed(VarwinXRControllerInputHandler sender)
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
        /// <param name="sender">Компонент, который вызвал событие.</param>
        private void OnGripReleased(VarwinXRControllerInputHandler sender)
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

            var targetPosition = ControllerModel.GrabAnchor.transform.TransformPoint(_grabbedPositionOffset);
            var targetRotation = ControllerModel.GrabAnchor.transform.rotation * _grabbedRotationOffset;

            GrabbedObject.Rigidbody.position = targetPosition;
            GrabbedObject.Rigidbody.rotation = targetRotation;
            GrabbedObject.transform.position = targetPosition;
            GrabbedObject.transform.rotation = targetRotation;
            GrabbedObject.Rigidbody.velocity = Vector3.zero;
            GrabbedObject.Rigidbody.angularVelocity = Vector3.zero;
            GrabbedObject.Rigidbody.ResetInertiaTensor();

            _oldGrabbedPosition = targetPosition;
            _oldGrabbedRotation = targetRotation;

            _grabbingAware?.OnHandTransformChanged(Vector3.zero, Vector3.zero);
        }

        /// <summary>
        /// Инициализация управления контроллером.
        /// </summary>
        /// <param name="inputDevice">XR устройство.</param>
        /// <param name="controllerModel">Модель контроллера.</param>
        public void Initialize(InputDevice inputDevice, VarwinXRControllerModel controllerModel)
        {
            InputDevice = inputDevice;
            ControllerModel = controllerModel;
            InputHandler.Initialize(inputDevice, ControllerModel.TeleportOnThumbstick, ControllerModel.Is3Dof, controllerModel.GetComponent<FeatureUsageMap>());
        }

        /// <summary>
        /// Метод деинициализации контроллера.
        /// </summary>
        public void Deinitialize()
        {
            if (GrabbedObject)
            {
                ForceDrop();    
            }
            
            ControllerModel = null;
            InputHandler.Deinitialize();
        }

        /// <summary>
        /// Обработка логической составляющей.
        /// </summary>
        private void Update()
        {
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
            if (!ControllerModel)
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
            
            var pointerAnchor = ControllerModel.PointerAnchor.transform;

            ControllerRaycaster.RaycastAndOverlap(pointerAnchor.position, pointerAnchor.forward, pointerAnchor.position, RadiusCast);
            
            _hoveredInteractableObject = ControllerRaycaster.NearInteractableObject;
        }

        /// <summary>
        /// Взять в руку.
        /// </summary>
        /// <param name="interactableObject">Интерактивный объект.</param>
        /// <param name="force">ForceGrab.</param>
        private void GrabObject(VarwinXRInteractableObject interactableObject, bool force = false)
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
                interactableObject.transform.position = ControllerModel.GrabAnchor.transform.position;
                interactableObject.transform.rotation = ControllerModel.GrabAnchor.transform.rotation;
            }
            
            var grabSettings = interactableObject.GrabSettings;
            if (!grabSettings || grabSettings.AttachPoints == null || grabSettings.AttachPoints.Length == 0)
            {
                _grabbedPositionOffset = ControllerModel.GrabAnchor.transform.InverseTransformPoint(interactableObject.transform.position);
                _grabbedRotationOffset = Quaternion.Inverse(ControllerModel.GrabAnchor.transform.rotation) * interactableObject.transform.rotation;
            }
            else
            {
                var firstGrabSettings = grabSettings.AttachPoints.FirstOrDefault();
                var attachPoint = IsLeft ? firstGrabSettings.LeftGrabPoint : firstGrabSettings.RightGrabPoint;

                if (!attachPoint)
                {
                    _grabbedPositionOffset = ControllerModel.GrabAnchor.transform.InverseTransformPoint(interactableObject.transform.position);
                    _grabbedRotationOffset = Quaternion.Inverse(ControllerModel.GrabAnchor.transform.rotation) * interactableObject.transform.rotation;
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

                var isDeletedAfterGrab = !GrabbedObject || (GrabbedObject.gameObject.GetWrapper()?.GetObjectController()?.IsDeleted ?? true);
                if (isDeletedAfterGrab)
                {
                    return;
                }
                
                ControllerModel.gameObject.SetActive(false);
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
        private void TouchObject(VarwinXRInteractableObject interactableObject)
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
        private void UseObject(VarwinXRInteractableObject interactableObject)
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

            var targetPosition = ControllerModel.GrabAnchor.transform.TransformPoint(_grabbedPositionOffset);
            var targetRotation = ControllerModel.GrabAnchor.transform.rotation * _grabbedRotationOffset;

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
            
            var newAngularVelocity = (angle * axis * Mathf.Deg2Rad) / Time.deltaTime;
            var newVelocity = (GrabbedObject.Rigidbody.position - _oldGrabbedPosition) / Time.deltaTime;

            _angularVelocityList.Add(newAngularVelocity); 
            _velocityList.Add(newVelocity);
            
            _oldGrabbedPosition = GrabbedObject.Rigidbody.position;
            _oldGrabbedRotation = GrabbedObject.Rigidbody.rotation;
            _grabbingAware?.OnHandTransformChanged(newAngularVelocity, newVelocity);
        }

        /// <summary>
        /// Отправка сигнала на вибрацию.
        /// </summary>
        /// <param name="strength">Сила вибрации.</param>
        /// <param name="duration">Длительность.</param>
        /// <param name="interval">Периодичность.</param>
        public void SetTriggerHapticPulse(float strength, float duration, float interval)
        {
            InputDevice.SendHapticImpulse(0, strength, duration);
        }

        /// <summary>
        /// Выбросить из руки объект. 
        /// </summary>
        public void ForceDrop()
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

            grabbedVelocity = _velocityList.Count == 0 ? Vector3.zero : grabbedVelocity / _velocityList.Count;
            grabbedAngularVelocity = _angularVelocityList.Count == 0 ? Vector3.zero : grabbedAngularVelocity / _angularVelocityList.Count;
            
            GrabbedObject.Rigidbody.velocity = grabbedVelocity;
            GrabbedObject.Rigidbody.angularVelocity = grabbedAngularVelocity;

            GrabbedObject.GrabEnd();
            GrabbedObject = null;
            ControllerModel.gameObject.SetActive(true);
            _grabbingAware = null;
            ForcedGrab = false;
        }

        /// <summary>
        /// Перестать использовать объект.
        /// </summary>
        public void ForceUnUse()
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
        public void ForceUnTouch()
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
            if (!Initialized || !targetGameObject)
            {
                return;
            }
            
            var interactableObject = targetGameObject.GetComponentInParent<VarwinXRInteractableObject>(true);
            if (!interactableObject)
            {
                return;
            }

            GrabObject(interactableObject, true);
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

            InputHandler.TriggerPressed -= OnTriggerPressed;
            InputHandler.TriggerReleased -= OnTriggerReleased;
            InputHandler.GripPressed -= OnGripPressed;
            InputHandler.GripReleased -= OnGripReleased;
        }
    }
}