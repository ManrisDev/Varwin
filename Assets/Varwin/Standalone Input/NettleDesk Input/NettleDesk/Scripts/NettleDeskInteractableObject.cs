using UnityEngine;
using Varwin.NettleDeskPlayer;

namespace Varwin.NettleDesk
{
    /// <summary>
    /// Поведение интерактивного объекта.
    /// </summary>
    public class NettleDeskInteractableObject : MonoBehaviour, IInteractableObject
    {
        /// <summary>
        /// Контроллер, который последним взаимодействовал с объектом.
        /// </summary>
        public NettleDeskControllerBase LastInteractedBy { get; private set; }
        
        /// <summary>
        /// Контроллер, что взял в руку.
        /// </summary>
        public NettleDeskControllerBase GrabbedBy { get; private set; }

        /// <summary>
        /// Контроллер, что использует.
        /// </summary>
        public NettleDeskControllerBase UsedBy { get; private set; }

        /// <summary>
        /// Контроллер, что касается.
        /// </summary>
        public NettleDeskControllerBase TouchedBy { get; private set; }

        /// <summary>
        /// Настройки граба.
        /// </summary>
        public GrabSettings GrabSettings { get; private set; }

        /// <summary>
        /// Может ли быть выпущен из руки.
        /// </summary>
        public bool CanBeDrop = true;

        /// <summary>
        /// Событие интерактивного объекта.
        /// </summary>
        /// <param name="sender">Интерактивный объект.</param>
        /// <param name="interactingObject">Контроллер, который взаимодействует с объектом.</param>
        public delegate void InteractableObjectEventHandler(GameObject sender, GameObject interactingObject);

        /// <summary>
        /// Событие, вызываемое при взятии в руку объекта.
        /// </summary>
        public event InteractableObjectEventHandler ObjectGrabbed;

        /// <summary>
        /// Событие, вызываемое при выпускании из рук объекта.
        /// </summary>
        public event InteractableObjectEventHandler ObjectDropped;

        /// <summary>
        /// Событие, вызываемое при касании объекта.
        /// </summary>
        public event InteractableObjectEventHandler ObjectTouched;

        /// <summary>
        /// Событие, вызываемое при прекращении касания объекта.
        /// </summary>
        public event InteractableObjectEventHandler ObjectUntouched;

        /// <summary>
        /// Событие, вызываемое при использовании объекта.
        /// </summary>
        public event InteractableObjectEventHandler ObjectUsed;

        /// <summary>
        /// Событие, вызываемое при окончании использования объекта.
        /// </summary>
        public event InteractableObjectEventHandler ObjectUnused;

        /// <summary>
        /// Используется ли.
        /// </summary>
        public bool IsUsed { get; private set; }

        /// <summary>
        /// Касаюется ли контролллер.
        /// </summary>
        public bool IsTouched { get; private set; }

        /// <summary>
        /// Может ли быть взят в руку.
        /// </summary>
        public bool IsGrabbable = false;

        /// <summary>
        /// Может ли быть использован.
        /// </summary>
        public bool IsUsable = false;

        /// <summary>
        /// Может ли быть касаемым.
        /// </summary>
        public bool IsTouchable = false;
        
        /// <summary>
        /// Взят ли методом ForceGrab'a.
        /// </summary>
        public bool IsForceGrabbed;

        /// <summary>
        /// Интерактивный ли.
        /// </summary>
        public bool IsInteractable => IsTouchable || IsUsable || IsGrabbable;

        /// <summary>
        /// Взят ли в руку.
        /// </summary>
        public bool IsGrabbed => GrabbedBy;

        /// <summary>
        /// Твердое тело для взаимодействия.
        /// </summary>
        private Rigidbody _rigidbody;

        /// <summary>
        /// Твердое тело для взаимодействия.
        /// </summary>
        public Rigidbody Rigidbody
        {
            get
            {
                if (!_rigidbody)
                {
                    _rigidbody = gameObject.GetComponent<Rigidbody>();
                }

                return _rigidbody;
            }
        }

        /// <summary>
        /// Инициализация интерактивного объекта.
        /// </summary>
        private void Awake()
        {
            GrabSettings = GetComponentInParent<GrabSettings>();
        }
        
        /// <summary>
        /// Взять в руку.
        /// </summary>
        /// <param name="controllerBase">Контроллер.</param>
        public void GrabStart(NettleDeskControllerBase controllerBase)
        {
            if (ProjectData.InteractionWithObjectsLocked)
            {
                return;
            }

            GrabbedBy = controllerBase;
            LastInteractedBy = controllerBase;
            ObjectGrabbed?.Invoke(gameObject, GrabbedBy.gameObject);
        }

        /// <summary>
        /// Выпустить из руку.
        /// </summary>
        public void GrabEnd()
        {
            if (!IsGrabbed)
            {
                return;
            }

            CanBeDrop = true;
            ObjectDropped?.Invoke(gameObject, GrabbedBy.gameObject);
            GrabbedBy = null;
            IsForceGrabbed = false;
        }

        /// <summary>
        /// Коснуться.
        /// </summary>
        /// <param name="controllerBase">Контроллер.</param>
        public void TouchStart(NettleDeskControllerBase controllerBase)
        {
            if (ProjectData.InteractionWithObjectsLocked)
            {
                return;
            }

            IsTouched = true;
            TouchedBy = controllerBase;
            LastInteractedBy = controllerBase;
            ObjectTouched?.Invoke(gameObject, controllerBase.gameObject);
        }

        /// <summary>
        /// Перестать касаться.
        /// </summary>
        public void TouchEnd()
        {
            if (!IsTouched)
            {
                return;
            }

            IsTouched = false;
            ObjectUntouched?.Invoke(gameObject, TouchedBy.gameObject);
            TouchedBy = null;
        }

        /// <summary>
        /// Начать использовать.
        /// </summary>
        /// <param name="controllerBase">Контроллер.</param>
        public void UseStart(NettleDeskControllerBase controllerBase)
        {
            if (ProjectData.InteractionWithObjectsLocked)
            {
                return;
            }

            IsUsed = true;
            UsedBy = controllerBase;
            LastInteractedBy = controllerBase;
            ObjectUsed?.Invoke(gameObject, controllerBase.gameObject);
        }

        /// <summary>
        /// Перестать использовать.
        /// </summary>
        public void UseEnd()
        {
            if (!IsUsed)
            {
                return;
            }
            
            IsUsed = false;
            ObjectUnused?.Invoke(gameObject, UsedBy.gameObject);
            UsedBy = null;
        }
    }
}