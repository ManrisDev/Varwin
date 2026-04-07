using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Varwin.Public;
using Varwin.SocketLibrary.Extension;

namespace Varwin.SocketLibrary
{
    /// <summary>
    /// Класс, реализующий логику контроллера соединений одного объекта.
    /// </summary>
    public partial class SocketController : BaseComponentsBehaviour, IHierarchyGrabStartAware, IHierarchyGrabEndAware
    {
        /// <summary>
        /// Граф соединений.
        /// </summary>
        private ConnectionGraphBehaviour _connectionGraphBehaviour;

        /// <summary>
        /// Граф соединений.
        /// </summary>
        public ConnectionGraphBehaviour ConnectionGraphBehaviour => GetConnectingGraphBehaviour();

        /// <summary>
        /// Описание события контроллера соединений.
        /// </summary>
        /// <param name="socketPoint">Вилка.</param>
        /// <param name="plugPoint">Розетка.</param>
        public delegate void SocketHandler(SocketPoint socketPoint, PlugPoint plugPoint);

        /// <summary>
        /// Событие, вызываемое при соединении объектов.
        /// </summary>
        public event SocketHandler OnConnect;

        /// <summary>
        /// Событие, вызываемое при разъединении объектов.
        /// </summary>
        public event SocketHandler OnDisconnect;

        /// <summary>
        /// Список точек - вилок.
        /// </summary>
        public List<PlugPoint> PlugPoints => GetPlugPoints();

        /// <summary>
        /// Список точек - розеток.
        /// </summary>
        public List<SocketPoint> SocketPoints => GetSocketPoints();

        /// <summary>
        /// Список точек для соединения.
        /// </summary>
        public List<JointPoint> JointPoints { get; private set; }

        /// <summary>
        /// Контроллер превью.
        /// </summary>
        public PreviewBehaviour PreviewBehaviour { get; private set; }

        /// <summary>
        /// Предварительная вилка для подключения.
        /// </summary>
        private PlugPoint _previewPlugPoint;

        /// <summary>
        /// Предварительная розетка для подключения.
        /// </summary>
        private SocketPoint _previewSocketPoint;

        /// <summary>
        /// Взят ли объект в руку с учетом цепи.
        /// </summary>
        public bool IsGrabbed => ConnectionGraphBehaviour.IsGrabbed();

        /// <summary>
        /// Взят ли объект в руку.
        /// </summary>
#if VARWINCLIENT    
        public bool IsLocalGrabbed => ObjectController.GetHierarchyController().IsGrabbed;
#else
        public bool IsLocalGrabbed => InteractableObjectBehaviour.IsGrabbed;
#endif

        /// <summary>
        /// Подключается ли.
        /// </summary>
        public bool Connecting => _previewPlugPoint && _previewSocketPoint;

        /// <summary>
        /// Инициализация превью.
        /// </summary>
        private void Awake()
        {
            InitPreview();
        }

        /// <summary>
        /// Инициализация событий.
        /// </summary>
        private void Start()
        {
#if VARWINCLIENT            
            ObjectController.GetHierarchyController().InitializeHandlers();
#else
            InteractableObjectBehaviour.OnGrabStarted.AddListener(OnGrabStart);
            InteractableObjectBehaviour.OnGrabEnded.AddListener(OnGrabEnd);      
#endif
        }

        /// <summary>
        /// Подключить, если имеются обе точки.
        /// </summary>
        public void ConnectIfPossible()
        {
            if (!_previewPlugPoint || !_previewSocketPoint)
            {
                return;
            }

            if (isActiveAndEnabled)
            {
                StartCoroutine(ConnectIfPossibleAfterFrame());
            }
            else
            {
                Connect(_previewPlugPoint, _previewSocketPoint);
            }
        }

        /// <summary>
        /// Подключение розетки к вилке. Нужно для игнорирования перехвата из одной руки в другую. 
        /// </summary>
        private IEnumerator ConnectIfPossibleAfterFrame()
        {
            yield return null;
            if (!ConnectionGraphBehaviour.IsGrabbed())
            {
                Connect(_previewPlugPoint, _previewSocketPoint);  
            }
        }

        /// <summary>
        /// Метод, вызываемый при поднятии объекта.
        /// </summary>
        private void OnGrabStart()
        {
            OnGrabStart(ObjectController);
        }
        
        /// <summary>
        /// Метод, вызываемый при броске объекта.
        /// </summary>
        private void OnGrabEnd()
        {
            OnGrabEnd(ObjectController);
        }

        /// <summary>
        /// Метод, вызываемый при поднятии объекта.
        /// </summary>
        public void OnGrabStart(ObjectController sender)
        {
            DisconnectIfPossible();
            ConnectionGraphBehaviour.OnGrabStart();
        }
        
        /// <summary>
        /// Метод, вызываемый при броске объекта.
        /// </summary>
        public void OnGrabEnd(ObjectController sender)
        {
            ConnectionGraphBehaviour.OnGrabEnd();
        }

        /// <summary>
        /// Отключить, если возможно.
        /// </summary>
        public void DisconnectIfPossible()
        {
            if (PlugPoints == null)
            {
                return;
            }

            foreach (var plugPoint in PlugPoints.Where(plugPoint => plugPoint.CanDisconnect))
            {
                Disconnect(plugPoint);
            }
        }

        /// <summary>
        /// Инициализация превью.
        /// </summary>
        private void InitPreview()
        {
            PreviewBehaviour = gameObject.GetPreviewObject().GetComponent<PreviewBehaviour>();
            PreviewBehaviour.Init(this);
        }

        /// <summary>
        /// Обновление превью объекта.
        /// </summary>
        public void UpdatePreview()
        {
            if (PreviewBehaviour)
            {
                Destroy(PreviewBehaviour.gameObject);
            }

            InitPreview();
        }

        /// <summary>
        /// При поднесении одного объекта к другому.
        /// </summary>
        /// <param name="plugPoint">Вилка.</param>
        /// <param name="socketPoint">Розетка.</param>
        public void OnPlugPointTriggerEnter(PlugPoint plugPoint, SocketPoint socketPoint)
        {
            if (!CanConnecting(socketPoint, plugPoint))
            {
                return;
            }

            _previewPlugPoint = plugPoint;
            
            plugPoint.CandidateJointPoint = socketPoint;
            socketPoint.CandidateJointPoint = plugPoint;
            
            _previewSocketPoint = socketPoint;

            socketPoint.SocketController._previewPlugPoint = plugPoint;
            socketPoint.SocketController._previewSocketPoint = socketPoint;

            plugPoint.InvokeOnPreviewShow(plugPoint, socketPoint);
            socketPoint.InvokeOnPreviewShow(plugPoint, socketPoint);
        }

        /// <summary>
        /// При прекращении коллизии точек.
        /// </summary>
        /// <param name="plugPoint">Вилка.</param>
        /// <param name="socketPoint">Розетка.</param>
        public void OnPlugPointTriggerExit(PlugPoint plugPoint, SocketPoint socketPoint)
        {
            plugPoint.SocketController.ResetState();
            socketPoint.SocketController.ResetState();
            
            plugPoint.InvokeOnPreviewHide(plugPoint, socketPoint);
            socketPoint.InvokeOnPreviewHide(plugPoint, socketPoint);
        }

        /// <summary>
        /// Сброс состояний.
        /// </summary>
        public void ResetState()
        {
            if (_previewPlugPoint)
            {
                _previewPlugPoint.InvokeOnPreviewHide(_previewPlugPoint, _previewSocketPoint);    
            }

            if (_previewSocketPoint)
            {
                _previewSocketPoint.InvokeOnPreviewHide(_previewPlugPoint, _previewSocketPoint);
            }
            
            _previewPlugPoint = null;
            _previewSocketPoint = null;
            PreviewBehaviour.Hide();
            ConnectionGraphBehaviour.ForEach(controller => controller.ResetHighlight());

            // Фикс дополнительных превью (между двух объектов).
            foreach (var jointPoint in JointPoints)
            {
                if (jointPoint.CandidateJointPoint && jointPoint.CandidateJointPoint.SocketController)
                {
                    var candidate = jointPoint.CandidateJointPoint;
                    jointPoint.CandidateJointPoint = null;
                    candidate.SocketController.ResetState();
                }
            }
        }

        /// <summary>
        /// Получение графа соединений.
        /// </summary>
        /// <returns>Граф соединений.</returns>
        private ConnectionGraphBehaviour GetConnectingGraphBehaviour()
        {
            if (!_connectionGraphBehaviour)
            {
                _connectionGraphBehaviour = ConnectionGraphBehaviour.InitTree(this);
            }

            return _connectionGraphBehaviour;
        }

        /// <summary>
        /// Добавление точки в контроллер.
        /// </summary>
        /// <param name="jointPoint">Точка для соединения.</param>
        public void AddJointPoint(JointPoint jointPoint)
        {
            if (!jointPoint)
            {
                return;
            }

            if (JointPoints == null)
            {
                JointPoints = new List<JointPoint>();
            }

            if (!JointPoints.Contains(jointPoint))
            {
                JointPoints.Add(jointPoint);
            }
        }

        /// <summary>
        /// Удаление точки из списка.
        /// </summary>
        /// <param name="jointPoint">Точка для соединения.</param>
        public void RemoveJointPoint(JointPoint jointPoint)
        {
            JointPoints?.Remove(jointPoint);

            if (JointPoints?.Count == 0)
            {
                Destroy(this);
            }
        }
        
        /// <summary>
        /// Получение списка розеток.
        /// </summary>
        /// <returns>Список розеток.</returns>
        private List<SocketPoint> GetSocketPoints()
        {
            return JointPoints?.FindAll(a => a is SocketPoint).ConvertAll(a => a as SocketPoint);
        }

        /// <summary>
        /// Получение списка вилок.
        /// </summary>
        /// <returns>Список вилок.</returns>
        private List<PlugPoint> GetPlugPoints()
        {
            return JointPoints?.FindAll(a => a is PlugPoint).ConvertAll(a => a as PlugPoint);
        }

        /// <summary>
        /// Рендеринг превью, если таковое возможно. Также разрешаем возможность отпустить, если хайлайтится.
        /// </summary>
        private void LateUpdate()
        {
            if (!IsGrabbed)
            {
                return;
            }
            
            if (ConnectionGraphBehaviour.IsConnecting())
            {
                CollisionProvider.CheckCanDrop();
            }
            
            if (Connecting)
            {
                _previewPlugPoint.SocketController.ConnectionGraphBehaviour.ForEach(controller => controller.SetJoinHighlight());
                _previewSocketPoint.SocketController.ConnectionGraphBehaviour.ForEach(controller => controller.SetJoinHighlight());

                _previewPlugPoint.SocketController.PreviewBehaviour.Show(_previewSocketPoint, _previewPlugPoint);
            }
        }

        /// <summary>
        /// Удаление объекта провайдера.
        /// </summary>
        private void OnDestroy()
        {
#if VARWINCLIENT            
            ObjectController?.GetHierarchyController()?.InitializeHandlers();
#else
            InteractableObjectBehaviour.OnGrabStarted.RemoveListener(OnGrabStart);
            InteractableObjectBehaviour.OnGrabEnded.RemoveListener(OnGrabEnd);      
#endif
            
            foreach (var jointPoint in JointPoints)
            {
                Disconnect(jointPoint);
            }
            
            if (PreviewBehaviour)
            {
                Destroy(PreviewBehaviour.gameObject);
            }
            
            if (_connectionGraphBehaviour)
            {
                DestroyImmediate(_connectionGraphBehaviour.gameObject);
            }

            if (_collisionProvider)
            {
                DestroyImmediate(_collisionProvider.gameObject);
            }
        }
    }
}