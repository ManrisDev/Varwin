using System;
using UnityEngine;
using Varwin.PlatformAdapter;
using Varwin.Raycasters;

namespace Varwin
{
    /// <summary>
    /// Базовый класс для UI поинтеров.
    /// </summary>
    public class DefaultVarwinUIPointer : MonoBehaviour, IBasePointer 
    {
        /// <summary>
        /// Отображать ли указку.
        /// </summary>
        public bool ShowPointer = true;

        /// <summary>
        /// Текущий объект, с которым можно производить взаимодействия при помощи UI поинтера.
        /// </summary>
        public PointableObject HoveredPointableObject
        {
            get => _hoveredPointableObject;
            private set
            {
                if (value == _hoveredPointableObject)
                {
                    return;
                }

                if (_hoveredPointableObject)
                {
                    InvokePointerAction(_hoveredPointableObject, _hoveredPointableObject.OnPointerOut);                    
                }

                _pressedPointableObject = null;
                _hoveredPointableObject = value;

                if (_hoveredPointableObject)
                {
                    InvokePointerAction(_hoveredPointableObject, _hoveredPointableObject.OnPointerIn);
                }
            }
        }

        /// <summary>
        /// Рука, которая взаимодействует с PointableObject'ом.
        /// Используется для получения и передачи контекста в ObjectPointerBehaviour.
        /// </summary>
        protected virtual ControllerInteraction.ControllerHand Hand => ControllerInteraction.DefaultDesktopIterationHand;

        /// <summary>
        /// Ссылка на рейкстер, который используется для получения PointableObject'ов.
        /// Должен быть переопределен для каждой конкретной платформы.
        /// </summary>
        /// <exception cref="NotImplementedException">Исключение, что рейкастер не определен для текущей платформы.</exception>
        protected virtual IPointableRaycaster Raycaster => throw new NotImplementedException();

        /// <summary>
        /// Текущий нажатый объект. Сбрасывается при изменении HoveredPointableObject.
        /// Используется для обработки события Click и PointerUp. 
        /// </summary>
        protected PointableObject _pressedPointableObject;

        /// <summary>
        /// Текущий объект, с которым можно производить взаимодействия при помощи UI поинтера. 
        /// </summary>
        private PointableObject _hoveredPointableObject;

        /// <summary>
        /// Открыто ли окно оверлапа.
        /// </summary>
        private bool _isUiOverlapped;

        /// <summary>
        /// Подписка на событие открытия окна оверлапа.
        /// </summary>
        private void OnEnable()
        {
            ProjectData.UIOverlapStatusChaned += OnUiOverlapStatusChanged;
        }

        /// <summary>
        /// Отписка от события открытия окна оверлапа.
        /// </summary>
        private void OnDisable()
        {
            ProjectData.UIOverlapStatusChaned += OnUiOverlapStatusChanged;
        }

        /// <summary>
        /// Обработка события оверлапа UI.
        /// Используется для отключения возможности взаимодействия с PointableObject'ами во время показа esc попапа.
        /// </summary>
        /// <param name="status">Открыт ли окно оверлапа.</param>
        private void OnUiOverlapStatusChanged(bool status)
        {
            HoveredPointableObject = null;
            _isUiOverlapped = status;
        }

        /// <summary>
        /// Выполнение события через проверку обработку исключений.
        /// </summary>
        /// <param name="pointableObject">Целевой объект.</param>
        /// <param name="action">Метод-событие, который необходимо вызвать.</param>
        protected void InvokePointerAction(PointableObject pointableObject, Action action)
        {
            if (!pointableObject)
            {
                return;
            }

            if (pointableObject is ObjectPointerBehaviour behaviour)
            {
                behaviour.SetHoveringHand(Hand);                
            }

            DebugUtils.ExecuteActionWithTryCatch(action, pointableObject.gameObject);
        }

        #region IBasePointer

        /// <summary>
        /// Может ли поинтер быть включен.
        /// </summary>
        /// <returns>true - может; false - не может.</returns>
        public bool CanToggle() => HoveredPointableObject && ShowPointer;

        /// <summary>
        /// Можно ли нажать на текущий PointableObject.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanPress() => ! _isUiOverlapped && HoveredPointableObject && !_pressedPointableObject;

        /// <summary>
        /// Можно ли вызвать отжать.
        /// Переопределяется на платформах с контроллерами.
        /// </summary>
        /// <returns>true - можно отжать.</returns>
        public virtual bool CanRelease() => true;

        /// <summary>
        /// Задать состояние указки.
        /// Используется на платформах XR и NettleDesk для переключение LineRenderer'а указки.
        /// </summary>
        /// <param name="value">true - включить указку. false - отключить.</param>
        public virtual void Toggle(bool value)
        {
            
        }

        /// <summary>
        /// Изменить состояние указки на противоположное.
        /// Используется на платформах XR и NettleDesk для переключение LineRenderer'а указки.
        /// </summary>
        public virtual void Toggle()
        {
            
        }

        /// <summary>
        /// Вызвать нажатие на PointableObject.
        /// </summary>
        public virtual void Press()
        {
            if (_pressedPointableObject)
            {
                return;
            }

            _pressedPointableObject = _hoveredPointableObject;

            if (_pressedPointableObject)
            {
                InvokePointerAction(_pressedPointableObject, _pressedPointableObject.OnPointerDown);
            }
        }

        /// <summary>
        /// Вызвать прекращение нажатия на PointableObject.
        /// </summary>
        public virtual void Release()
        {
            if (_pressedPointableObject && _hoveredPointableObject == _pressedPointableObject)
            {
                InvokePointerAction(_hoveredPointableObject, _hoveredPointableObject.OnPointerUpAsButton);
            }

            if (_hoveredPointableObject)
            {
                InvokePointerAction(_hoveredPointableObject, _hoveredPointableObject.OnPointerUp);
            }

            _pressedPointableObject = null;
        }

        /// <summary>
        /// Инициализация указки. Необходимо переопределить на каждой из платформ.
        /// Обычно используется для инициализации указки на платформах XR и NettleDesk.
        /// </summary>
        /// <exception cref="NotImplementedException">Инициализация указки не переопределена.</exception>
        public virtual void Init() => throw new NotImplementedException();

        /// <summary>
        /// Обновить состояние текущего PointableObject'а на основе данных из рейкастера.
        /// </summary>
        public virtual void UpdateState()
        {
            if (ProjectData.InteractionWithObjectsLocked)
            {
                if (HoveredPointableObject)
                {
                    HoveredPointableObject = null;
                }
                
                return;
            }

            if (_isUiOverlapped || Raycaster == null || !Raycaster.NearPointableObject || Raycaster.NearObjectRaycastHit == null)
            {
                HoveredPointableObject = null;
                return;
            }

            HoveredPointableObject = Raycaster.NearPointableObject;
        }

        /// <summary>
        /// Получение активности указки.
        /// </summary>
        /// <returns>Является ли указка активной.</returns>
        public virtual bool IsActive() => true;

        #endregion

        /// <summary>
        /// Метод для создания дефотлного LineRenderer'а для UI поинтера.
        /// Используется на XR и NettleDesk контроллерах.
        /// </summary>
        /// <param name="parent">Родительски объект для поинтера.</param>
        /// <param name="config">Конфигурация поинтера.</param>
        /// <returns>Созданный LineRenderer.</returns>
        protected LineRenderer CreateDefaultControllerLine(Transform parent, VarwinUiPointerConfig config)
        {
            var lineRendererGameObject = new GameObject("UIPointer");

            lineRendererGameObject.transform.parent = parent;
            lineRendererGameObject.transform.localPosition = Vector3.zero;
            lineRendererGameObject.transform.localScale = Vector3.one;
            lineRendererGameObject.transform.localRotation = Quaternion.identity;
            lineRendererGameObject.layer = LayerMask.NameToLayer("Highlight");

            var lineRenderer = lineRendererGameObject.AddComponent<LineRenderer>();
            lineRenderer.material = config.LineMaterial;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = config.StartWidth;
            lineRenderer.endWidth = config.EndWidth;
            lineRenderer.startColor = config.IdleColor;
            lineRenderer.endColor = config.IdleColor;
            lineRenderer.enabled = false;

            lineRenderer.material.SetFloat("_Cull", 0);

            return lineRenderer;
        }
    }
}