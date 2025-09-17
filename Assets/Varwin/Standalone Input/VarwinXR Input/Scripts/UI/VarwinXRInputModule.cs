using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Varwin.XR
{
    /// <summary>
    /// Модуль, отвечающий за взаимодействие с UI в режиме VarwinXR.
    /// </summary>
    public class VarwinXRInputModule : BaseInputModule
    {
        /// <summary>
        /// Дистанция, на которой активируется перетаскивание.
        /// </summary>
        private const float DragDistance = 10f;
        
        /// <summary>
        /// Камера игрока.
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// Объект-точка, который отображает текущее расположение курсора.
        /// </summary>
        public GameObject Pointer;

        /// <summary>
        /// Объект, отвечающий за отрисовку линии до объекта.
        /// </summary>
        public LineRenderer Ray;

        /// <summary>
        /// Поток данных к объектам.
        /// </summary>
        private VarwinXREventData _eventData;

        /// <summary>
        /// Нажат ли триггер на доминирующем контроллере.
        /// </summary>
        public bool TriggerDown => _dominatedController && _dominatedController.IsTriggerPressed();

        /// <summary>
        /// Положение стика (трекпада).
        /// </summary>
        public Vector2 Axis => _dominatedController ? _dominatedController.Primary2DAxisValue : Vector2.zero;

        /// <summary>
        /// Доминирующий контроллер.
        /// </summary>
        private VarwinXRControllerEventComponent _dominatedController;

        /// <summary>
        /// Левый контроллер.
        /// </summary>
        public VarwinXRControllerEventComponent LeftControllerEvents;

        /// <summary>
        /// Правый контроллер.
        /// </summary>
        public VarwinXRControllerEventComponent RightControllerEvents;

        /// <summary>
        /// Инициализация и подписка.
        /// </summary>
        protected override void Awake()
        {
            LeftControllerEvents.TriggerReleased += OnTriggerReleased;
            RightControllerEvents.TriggerReleased += OnTriggerReleased;
            LeftControllerEvents.Initialized += OnInitialized;
            LeftControllerEvents.Deinitialized += OnDeinitialized;
            RightControllerEvents.Initialized += OnInitialized;
            RightControllerEvents.Deinitialized += OnDeinitialized;
            Pointer.SetActive(false);
            Ray.gameObject.SetActive(false);
        }
                        
        /// <summary>
        /// Проверка наличия доминирующего контроллера.
        /// </summary>
        protected override void Start()
        {
            if (LeftControllerEvents.Controller.Initialized)
            {
                if (!_dominatedController)
                {
                    _dominatedController = LeftControllerEvents;
                }
            }

            if (RightControllerEvents.Controller.Initialized)
            {
                _dominatedController = RightControllerEvents;
            }
        }
        
        /// <summary>
        /// При инициализации контроллера.
        /// </summary>
        /// <param name="sender">Вызывающий событие объект.</param>
        private void OnInitialized(VarwinXRControllerEventComponent sender)
        {
            if (sender.IsLeft)
            {
                if (!_dominatedController)
                {
                    _dominatedController = sender;
                }
            }
            else
            {
                _dominatedController = sender;
            }
        }
        
        /// <summary>
        /// При деинициализации контроллера.
        /// </summary>
        /// <param name="sender">Вызывающий событие объект.</param>
        private void OnDeinitialized(VarwinXRControllerEventComponent sender)
        {
            
        }

        /// <summary>
        /// При отпускании триггера на контроллере.
        /// </summary>
        /// <param name="sender">Контроллер, который вызвал событие.</param>
        private void OnTriggerReleased(VarwinXRControllerEventComponent sender)
        {
            _dominatedController = sender;
        }
        
        /// <summary>
        /// Поиск первого рейкаста, если его модулем является VarwinXRRaycast'ер.
        /// </summary>
        /// <param name="candidates">Список кандидатов на рейкаст.</param>
        /// <returns>Найденная информация о рейксте.</returns>
        private static RaycastResult FindFirstRaycastWithCheckType(List<RaycastResult> candidates)
        {
            var candidatesCount = candidates.Count;
            for (var i = 0; i < candidatesCount; ++i)
            {
                if (candidates[i].gameObject == null || candidates[i].module.GetType() != typeof(VarwinXRRaycaster))
                {
                    continue;
                }

                return candidates[i];
            }
            
            return new RaycastResult();
        }

        /// <summary>
        /// Обработка событий UI.
        /// </summary>
        public override void Process()
        {
            if (!_dominatedController || !_dominatedController.Controller.Initialized)
            {
                return;
            }

            _eventData ??= new VarwinXREventData(EventSystem.current);

            var pointerAnchor = _dominatedController.Controller.ControllerModel.PointerAnchor.transform;
            _eventData.ControllerRay = new Ray(pointerAnchor.position, pointerAnchor.forward);
            _eventData.Camera = Camera;
            _eventData.Controller = _dominatedController.gameObject;
            _eventData.button = PointerEventData.InputButton.Left;

            var raycastResults = new List<RaycastResult>();
            eventSystem.RaycastAll(_eventData, raycastResults);
            var result = FindFirstRaycastWithCheckType(raycastResults);

            _eventData.pointerCurrentRaycast = result;
            _eventData.pointerPressRaycast = result;
            
            _eventData.delta = result.screenPosition - _eventData.position;
            _eventData.position = result.screenPosition;

            _eventData.button = PointerEventData.InputButton.Left;

            ProcessInteraction();
        }

        /// <summary>
        /// Проверка взаимодействий.
        /// </summary>
        private void ProcessInteraction()
        {
            ProcessHover();
            ProcessPointerDown();
            ProcessPointerClick();
            ProcessPointerUp();
            ProcessBeginDrag();
            ProcessDrag();
            ProcessEndDrag();
            ProcessScroll();
            UpdatePointer();
        }

        /// <summary>
        /// Проверка наведения курсора на объект.
        /// </summary>
        private void ProcessHover()
        {
            if (_eventData.pointerEnter)
            {
                ExecuteEvents.ExecuteHierarchy(_eventData.pointerEnter, _eventData, ExecuteEvents.pointerMoveHandler);
            }

            if (_eventData.pointerEnter == _eventData.pointerCurrentRaycast.gameObject)
            {
                return;
            }

            if (_eventData.pointerEnter)
            {
                ExecuteEvents.ExecuteHierarchy(_eventData.pointerEnter, _eventData, ExecuteEvents.pointerExitHandler);
            }

            _eventData.pointerEnter = _eventData.pointerCurrentRaycast.gameObject;

            if (_eventData.pointerEnter)
            {
                ExecuteEvents.ExecuteHierarchy(_eventData.pointerEnter, _eventData, ExecuteEvents.pointerEnterHandler);
            }
        }

        /// <summary>
        /// Проверка нажатия курсора на объекте.
        /// </summary>
        private void ProcessPointerDown()
        {
            if (!_eventData.pointerEnter || !TriggerDown || _eventData.pointerPress)
            {
                return;
            }

            _eventData.pointerPress = _eventData.pointerEnter;
            _eventData.pressPosition = _eventData.position;
            ExecuteEvents.ExecuteHierarchy(_eventData.pointerPress, _eventData, ExecuteEvents.pointerDownHandler);
        }

        /// <summary>
        /// Проверка отпускания курсора на объекте.
        /// </summary>
        private void ProcessPointerUp()
        {
            if (!_eventData.pointerPress || TriggerDown)
            {
                return;
            }

            ExecuteEvents.ExecuteHierarchy(_eventData.pointerPress, _eventData, ExecuteEvents.pointerUpHandler);
            _eventData.pointerPress = null;
        }

        /// <summary>
        /// Проверка клика.
        /// </summary>
        private void ProcessPointerClick()
        {
            if (!_eventData.pointerPress || TriggerDown || _eventData.pointerPress != _eventData.pointerEnter)
            {
                return;
            }

            _eventData.pointerClick = _eventData.pointerPress;
            _eventData.clickCount = 1;
            ExecuteEvents.ExecuteHierarchy(_eventData.pointerClick, _eventData, ExecuteEvents.pointerClickHandler);
            _eventData.pointerClick = null;
            _eventData.clickCount = 0;
        }

        /// <summary>
        /// Проверка перетаскивания.
        /// </summary>
        private void ProcessBeginDrag()
        {
            var distance = (_eventData.pressPosition - _eventData.position).magnitude;
            if (_eventData.dragging || !_eventData.pointerPress || distance < DragDistance)
            {
                return;
            }

            _eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(_eventData.pointerPress);

            if (_eventData.pointerDrag)
            {
                _eventData.dragging = true;
                ExecuteEvents.ExecuteHierarchy(_eventData.pointerDrag, _eventData, ExecuteEvents.initializePotentialDrag);
                ExecuteEvents.ExecuteHierarchy(_eventData.pointerDrag, _eventData, ExecuteEvents.beginDragHandler);
            }
        }

        /// <summary>
        /// Обновление перетаскивания.
        /// </summary>
        private void ProcessDrag()
        {
            if (!_eventData.dragging)
            {
                return;
            }
            
            ExecuteEvents.ExecuteHierarchy(_eventData.pointerDrag, _eventData, ExecuteEvents.dragHandler);
        }

        /// <summary>
        /// Проверка окончания перетаскивания.
        /// </summary>
        private void ProcessEndDrag()
        {
            if (!_eventData.dragging || !_eventData.pointerDrag || _eventData.pointerPress)
            {
                return;
            }
            
            _eventData.dragging = false;
            ExecuteEvents.ExecuteHierarchy(_eventData.pointerDrag, _eventData, ExecuteEvents.endDragHandler);
            _eventData.pointerDrag = null;
        }

        /// <summary>
        /// Проверка скролла.
        /// </summary>
        private void ProcessScroll()
        {
            if (!_eventData.pointerEnter)
            {
                return;
            }

            _eventData.scrollDelta = Axis;
            ExecuteEvents.ExecuteHierarchy(_eventData.pointerEnter, _eventData, ExecuteEvents.scrollHandler);
            var axisEventData = GetAxisEventData(Axis.x, Axis.y, 0.6f);
            ExecuteEvents.ExecuteHierarchy(_eventData.pointerEnter, axisEventData, ExecuteEvents.moveHandler);
        }

        /// <summary>
        /// Обновление положения указателя.
        /// </summary>
        private void UpdatePointer()
        {
            if (!_eventData.pointerCurrentRaycast.gameObject)
            {
                Ray.gameObject.SetActive(false);
                Pointer.SetActive(false);
                return;
            }

            Pointer.transform.position = _eventData.pointerCurrentRaycast.worldPosition;
            Pointer.transform.forward = _eventData.pointerCurrentRaycast.worldNormal;

            Ray.SetPositions(new[] {_dominatedController.Controller.ControllerModel.PointerAnchor.transform.position, _eventData.pointerCurrentRaycast.worldPosition});
            Pointer.SetActive(true);
            Ray.gameObject.SetActive(true);
        }

        /// <summary>
        /// Отписка.
        /// </summary>
        protected override void OnDestroy()
        {
            LeftControllerEvents.TriggerReleased -= OnTriggerReleased;
            RightControllerEvents.TriggerReleased -= OnTriggerReleased;
            LeftControllerEvents.Initialized -= OnInitialized;
            RightControllerEvents.Deinitialized -= OnDeinitialized;
        }
    }
}