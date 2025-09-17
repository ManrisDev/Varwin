using System.Collections.Generic;
using UnityEngine;
using Varwin.Public;

namespace Varwin.SocketLibrary
{
    /// <summary>
    /// Базовый класс, реализующий логику одной соединяемой точки.
    /// </summary>
    public abstract class JointPoint : MonoBehaviour
    {
        /// <summary>
        /// Предварительная точка.
        /// </summary>
        public JointPoint CandidateJointPoint { get; set; }
        
        /// <summary>
        /// Событие точки соединения.
        /// </summary>
        /// <param name="plugPoint">Вилка.</param>
        /// <param name="socketPoint">Розетка.</param>
        public delegate void JointPointEvent(PlugPoint plugPoint, SocketPoint socketPoint);

        /// <summary>
        /// Событие при подключении.
        /// </summary>
        public event JointPointEvent OnConnect;

        /// <summary>
        /// Событие при разъединении.
        /// </summary>
        public event JointPointEvent OnDisconnect;
        
        /// <summary>
        /// Событие при появлении превью.
        /// </summary>
        public event JointPointEvent OnPreviewShow;
        
        /// <summary>
        /// Событие при пропадании превью.
        /// </summary>
        public event JointPointEvent OnPreviewHide;

        /// <summary>
        /// Может ли быть подключен.
        /// </summary>
        public bool CanConnect = true;
        
        /// <summary>
        /// Объект-идентификатор
        /// </summary>
        private ObjectId _objectId;

        /// <summary>
        /// Идентификатор объекта.
        /// </summary>
        public int Id
        {
            get
            {
                if (_objectId == null)
                {
                    _objectId = GetComponent<ObjectId>();
                }

                return _objectId ? _objectId.Id : -1;
            }
        }

        /// <summary>
        /// Список созданных вилок.
        /// </summary>
        public static List<PlugPoint> InstancedPlugPoints = new List<PlugPoint>();
        
        /// <summary>
        /// Список созданных розеток.
        /// </summary>
        public static List<SocketPoint> InstancedSocketPoints = new List<SocketPoint>();

        /// <summary>
        /// Свободна ли точка.
        /// </summary>
        private bool _isFree = true;

        /// <summary>
        /// Свободна ли точка.
        /// </summary>
        public bool IsFree
        {
            get => _isFree;
            set
            {
                Collider.enabled = value;
                _isFree = value;
            }
        }

        /// <summary>
        /// Контроллер соединений.
        /// </summary>
        private SocketController _socketController;

        /// <summary>
        /// Контроллер соединений.
        /// </summary>
        public SocketController SocketController
        {
            set
            {
                if (_socketController)
                {
                    _socketController.RemoveJointPoint(this);
                }

                _socketController = value;
                if (_socketController)
                {
                    _socketController.AddJointPoint(this);
                }
            }

            get
            {
                if (!_socketController)
                {
                    InitSocketController();
                }

                return _socketController;
            }
        }

        /// <summary>
        /// Инициализация контроллера соединений.
        /// </summary>
        protected void InitSocketController()
        {
            _socketController = GetComponentInParent<SocketController>();

            if (!_socketController)
            {
                if (gameObject.GetWrapper().GetGameObject() == gameObject)
                {
                    var objectController = gameObject.GetWrapper().GetObjectController();
                    if (objectController?.Parent != null)
                    {
                        _socketController = objectController.Parent.gameObject.AddComponent<SocketController>();
                    }
                }
                else
                {
                    _socketController = gameObject.GetWrapper().GetGameObject().AddComponent<SocketController>();
                }
            }

            if (!_socketController)
            {
                return;
            }

            _socketController.AddJointPoint(this);
        }

        /// <summary>
        /// Коллайдер.
        /// </summary>
        private Collider _collider;

        /// <summary>
        /// Коллайдер.
        /// </summary>
        public Collider Collider
        {
            get
            {
                if (!_collider)
                {
                    _collider = GetComponent<Collider>();
                }

                return _collider;
            }
        }

        /// <summary>
        /// Подключенная точка.
        /// </summary>
        private JointPoint _connectedPoint;

        /// <summary>
        /// Подключенная точка.
        /// </summary>
        public JointPoint ConnectedPoint
        {
            get => _connectedPoint;
            set
            {
                IsFree = !value;
                _connectedPoint = value;
            }
        }

        /// <summary>
        /// При взятии основного объекта переключаем коллайдер на объекте для повторного вызова событий триггера на точках соединения. 
        /// </summary>
        public void OnSocketControllerGrabStart()
        {
            Collider.enabled = false;
            Collider.enabled = IsFree;
        }

        /// <summary>
        /// Метод, вызывающие событие подключения.
        /// </summary>
        /// <param name="plugPoint">Вилка.</param>
        /// <param name="socketPoint">Розетка.</param>
        public void InvokeOnConnect(PlugPoint plugPoint, SocketPoint socketPoint)
        {
            if (socketPoint.ChainPreview)
            {
                SocketController.HidePreview(socketPoint);
            }

            OnConnect?.Invoke(plugPoint, socketPoint);
        }
        
        /// <summary>
        /// Метод, вызывающие событие разъединеия.
        /// </summary>
        /// <param name="plugPoint">Вилка.</param>
        /// <param name="socketPoint">Розетка.</param>
        public void InvokeOnDisconnect(PlugPoint plugPoint, SocketPoint socketPoint)
        {
            OnDisconnect?.Invoke(plugPoint, socketPoint);
        }
        
        /// <summary>
        /// Метод, вызывающие событие появления превью.
        /// </summary>
        /// <param name="plugPoint">Вилка.</param>
        /// <param name="socketPoint">Розетка.</param>
        public void InvokeOnPreviewShow(PlugPoint plugPoint, SocketPoint socketPoint)
        {
            OnPreviewShow?.Invoke(plugPoint, socketPoint);
        }

        /// <summary>
        /// Метод, вызывающие событие скрытии превью.
        /// </summary>
        /// <param name="plugPoint">Вилка.</param>
        /// <param name="socketPoint">Розетка.</param>
        public void InvokeOnPreviewHide(PlugPoint plugPoint, SocketPoint socketPoint)
        {
            OnPreviewHide?.Invoke(plugPoint, socketPoint);
        }

    }
}