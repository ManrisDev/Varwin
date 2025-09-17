using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.Raycasters
{
    /// <summary>
    /// Дефолтный класс рейкастера.
    /// </summary>
    /// <typeparam name="TInteractable">Тип интерактивного объекта для данного рейкастера.</typeparam>
    public class DefaultVarwinRaycaster<TInteractable> : MonoBehaviour, IPointableRaycaster where TInteractable: class, IInteractableObject
    {
        /// <summary>
        /// Количество возможных столкновений с объектами.
        /// </summary>
        protected const int RaycastCount = 16;
        
        /// <summary>
        /// Дистанция UI Pointer'a.
        /// </summary>
        protected const int UIPointerDistance = 100;

        /// <summary>
        /// Ближний объект UI.
        /// </summary>
        public PointableObject NearPointableObject { get; private set; }

        /// <summary>
        /// Ближний интерактивный объект.
        /// </summary>
        public TInteractable NearInteractableObject { get; set; }

        /// <summary>
        /// Информация о столкновении с ближайшим объектом.
        /// </summary>
        public RaycastHit? NearObjectRaycastHit { get; protected set; }

        /// <summary>
        /// Стандартные настройки для Distance Pointer'а.
        /// </summary>
        [field: SerializeField] public DistancePointerSettings DefaultPointerSettings { get; protected set; }

        /// <summary>
        /// Дистанция взаимодействия для Distance Pointer'а с учетом кастомных настроек.
        /// </summary>
        protected virtual float ObjectInteractDistance => DistancePointer.CustomSettings 
            ? DistancePointer.CustomSettings.Distance 
            : DefaultPointerSettings.Distance;

        /// <summary>
        /// Рука, по которой производится рейкаст. Используется для определения игнорируемых коллайдеров.
        /// </summary>
        protected virtual ControllerInteraction.ControllerHand Hand => ControllerInteraction.DefaultDesktopIterationHand;

        /// <summary>
        /// Маска слоев, по которой производится рейкаст.
        /// </summary>
        protected virtual LayerMask LayerMask
        {
            get => _layerMask;
            set => _layerMask = value;
        }

        /// <summary>
        /// Массив столкновений с объектами.
        /// </summary>
        private RaycastHit[] _raycastHits;

        /// <summary>
        /// Массив для сохранения коллайдеров оверлапа. Полезен на XR и NettleDesk платформах.
        /// </summary>
        protected Collider[] _overlapColliders;

        /// <summary>
        /// Маска слоя. Выставляется автоматически из DistancePointer.
        /// </summary>
        [SerializeField] private LayerMask _layerMask;

        protected virtual void Start()
        {
            _raycastHits = new RaycastHit[RaycastCount];
            _overlapColliders = new Collider[RaycastCount];

            LayerMask = DefaultPointerSettings.LayerMask;
            DistancePointer.SettingsUpdated += UpdateLayerMask;
        }

        private void OnDestroy()
        {
            DistancePointer.SettingsUpdated -= UpdateLayerMask;
        }

        /// <summary>
        /// Обновление маски слоя для рейкаста.
        /// </summary>
        private void UpdateLayerMask()
        {
            if (DistancePointer.CustomSettings)
            {
                LayerMask = DistancePointer.CustomSettings.LayerMask;
            }
        }

        /// <summary>
        /// Выполнить рейкаст.
        /// </summary>
        /// <param name="origin">Точка начала рейкаста.</param>
        /// <param name="direction">Направление рейкаста.</param>
        /// <param name="raycastDistanceOverride">Переопределение дистанции рейкаста. Если null, то применяются стандартные правила расчета расстояния.</param>
        /// <param name="interactDistanceOverride">Переопределение дистанции взаимодействия с интерактивными объектами.</param>
        public void Raycast(Vector3 origin, Vector3 direction, float? raycastDistanceOverride = null, float? interactDistanceOverride = null)
        {
            Raycast(new Ray(origin, direction), raycastDistanceOverride, interactDistanceOverride);
        }

        /// <summary>
        /// Выполнить рейкаст.
        /// </summary>
        /// <param name="ray">Луч рейкаста.</param>
        /// <param name="raycastDistanceOverride">Переопределение дистанции рейкаста. Если null, то применяются стандартные правила расчета расстояния.</param>
        /// <param name="interactDistanceOverride">Переопределение дистанции взаимодействия с интерактивными объектами. Если null, то применятся настройки из DistancePointerSettings.</param>
        public void Raycast(Ray ray, float? raycastDistanceOverride = null, float? interactDistanceOverride = null)
        {
            ResetState();

            var interactDistance = interactDistanceOverride ?? ObjectInteractDistance;
            var raycastDistance = raycastDistanceOverride ?? Mathf.Max(interactDistance, UIPointerDistance) ;
            var raycastHitCount = Physics.RaycastNonAlloc(ray, _raycastHits, raycastDistance, LayerMask);

            if (raycastHitCount <= 0)
            {
                return;
            }

            var interactingController = Hand == ControllerInteraction.ControllerHand.Left
                ? InputAdapter.Instance.PlayerController.Nodes.LeftHand?.Controller
                : InputAdapter.Instance.PlayerController.Nodes.RightHand?.Controller;

            var nearObjectDistance = Mathf.Infinity;

            for (var i = 0; i < raycastHitCount; i++)
            {
                var hit = _raycastHits[i];
                if (hit.collider.gameObject.TryGetComponent<PointableObject>(out var pointableObject))
                {
                    if (hit.distance > nearObjectDistance || hit.distance > UIPointerDistance)
                    {
                        continue;
                    }
                    
                    ResetState();
                    NearPointableObject = pointableObject;
                    NearObjectRaycastHit = hit;
                    nearObjectDistance = hit.distance;

                    var interactableObject = hit.collider.gameObject.GetComponentInParent<TInteractable>();

                    if (interactableObject != null && hit.distance <= interactDistance && interactableObject.IsInteractable)
                    {
                        NearInteractableObject = interactableObject;
                    }
                }
                else
                {
                    var interactableObject = hit.collider.gameObject.GetComponentInParent<TInteractable>();
                    if (interactableObject is { IsInteractable: false }
                        || hit.distance > nearObjectDistance 
                        || hit.distance > interactDistance
                        || PointerHelper.IsColliderGrabbed(hit.collider, interactingController))
                    {
                        continue;
                    }

                    ResetState();
                    NearInteractableObject = interactableObject;
                    NearObjectRaycastHit = hit;
                    nearObjectDistance = hit.distance;
                }
            }
        }

        /// <summary>
        /// Выполнить рейкст и оверлап. Оверлап коллайдеров будет произведен из позиции transform.position.
        /// Объекты, найденные во время оверлапа, являются более приоритеными, чем найденные во время рейкаста.
        /// </summary>
        /// <param name="origin">Позиция, из которой будет произведен рейкаст.</param>
        /// <param name="direction">Направление рейкаста.</param>
        /// <param name="overlapPosition">Позиция, из которой будет произведена проверка оверлапа коллайдеров.</param>
        /// <param name="overlapRadius">Радиус оверлапа.</param>
        /// <param name="raycastDistanceOverride">Переопределение дистанции рейкаста. Если null, то применяются стандартные правила расчета расстояния.</param>
        /// <param name="interactDistanceOverride">Переопределение дистанции взаимодействия с интерактивными объектами. Если null, то применятся настройки из DistancePointerSettings.</param>
        public virtual void RaycastAndOverlap(
            Vector3 origin,
            Vector3 direction,
            Vector3 overlapPosition,
            float overlapRadius,
            float? raycastDistanceOverride = null,
            float? interactDistanceOverride = null
        )
        {
            Raycast(origin, direction, raycastDistanceOverride, interactDistanceOverride);
            var overlapCount = Physics.OverlapSphereNonAlloc(overlapPosition, overlapRadius, _overlapColliders, LayerMask);

            for (var i = 0; i < overlapCount; i++)
            {
                var interactableObject = _overlapColliders[i].gameObject.GetComponentInParent<TInteractable>();
                if (interactableObject is { IsInteractable: true })
                {
                    NearInteractableObject = interactableObject;
                }
            }
        }

        /// <summary>
        /// Сброс данных.
        /// </summary>
        private void ResetState()
        {
            NearInteractableObject = null;
            NearPointableObject = null;

            NearObjectRaycastHit = null;
        }
    }
}