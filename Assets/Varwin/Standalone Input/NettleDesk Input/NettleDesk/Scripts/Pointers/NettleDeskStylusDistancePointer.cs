using UnityEngine;
using Varwin.NettleDeskPlayer;
using Varwin.PlatformAdapter;

namespace Varwin.NettleDesk
{
    /// <summary>
    /// Указка до объекта (использование, захват или касание). Эта указка носит лишь информационных характер.
    /// </summary>
    public class NettleDeskStylusDistancePointer : DistancePointer
    {
        /// <summary>
        /// Оффсет длины указки.
        /// </summary>
        private const float RaycastDistanceOffset = 20f;
        
        /// <summary>
        /// Объект указки.
        /// </summary>
        private GameObject _pointerGameObject;
        
        /// <summary>
        /// Активность указки.
        /// </summary>
        private bool _activity;
        
        /// <summary>
        /// Активность указки.
        /// </summary>
        public bool Activity
        {
            get => _activity;
            set
            {
                _activity = value;
                _lineRenderer.enabled = value;
            }
        }
        
        /// <summary>
        /// Объект-контроллер.
        /// </summary>
        public NettleDeskStylus Controller;
        
        /// <summary>
        /// Компонент отрисовки линии.
        /// </summary>
        private LineRenderer _lineRenderer;
        
        /// <summary>
        /// Дистанция касания.
        /// </summary>
        public override float RaycastDistance => Settings.Distance + RaycastDistanceOffset;

        /// <summary>
        /// Текущий объект столкновения.
        /// </summary>
        private NettleDeskInteractableObject _currentObject;
        
        /// <summary>
        /// Компонент событий.
        /// </summary>
        private ControllerInteraction.ControllerSelf _controllerReference;

        /// <summary>
        /// Можно ли переключить указку.
        /// </summary>
        /// <returns>Истина, если можно.</returns>
        public override bool CanToggle()
        {
            if (Activity)
            {
                return true;
            }
            
            return Controller.ActiveRaycastHit.HasValue && !_controllerReference.GetGrabbedObject() || Settings.AlwaysDrawRay;
        }

        /// <summary>
        /// Можно ли нажать. Поскольку эта указка носит лишь информативный характер, то здесь по умолчанию false.
        /// </summary>
        /// <returns>Истина, если возможно.</returns>
        public override bool CanPress()
        {
            return false;
        }

        /// <summary>
        /// Можно ли отпустить. Поскольку эта указка носит лишь информативный характер, то здесь по умолчанию false.
        /// </summary>
        /// <returns>Истина, если возможно.</returns>
        public override bool CanRelease()
        {
            return false;
        }

        /// <summary>
        /// Включение отображения указки.
        /// </summary>
        /// <param name="value">Истина, если включить.</param>
        public override void Toggle(bool value)
        {
            Activity = value;
        }

        /// <summary>
        /// Переключить состояние указки.
        /// </summary>
        public override void Toggle()
        {
            Activity = !Activity;
        }

        /// <summary>
        /// Вызвать метод нажатия.
        /// </summary>
        public override void Press()
        {
            
        }

        /// <summary>
        /// Вызвать метод выпускания.
        /// </summary>
        public override void Release()
        {
            
        }

        /// <summary>
        /// Обновление состояния, а также точек расположения указки.
        /// </summary>
        public override void UpdateState()
        {
            var pointerAnchor = transform;
            var rayEnabled = Controller.ActiveRaycastHit.HasValue && !ProjectData.InteractionWithObjectsLocked;

            if (Settings.AlwaysDrawRay)
            {
                _lineRenderer.enabled = IsActive();
                
                _lineRenderer.SetPosition(0, pointerAnchor.position);
                _lineRenderer.SetPosition(1, pointerAnchor.position + pointerAnchor.rotation * Vector3.forward * 10000f);
            }
            else
            {
                _lineRenderer.enabled = rayEnabled && IsActive();
            }

            if (rayEnabled)
            {
                _lineRenderer.SetPosition(0, pointerAnchor.position);
                _lineRenderer.SetPosition(1, Controller.ActiveRaycastHit.Value.point);
            }
        }

        /// <summary>
        /// Активна ли указка.
        /// </summary>
        /// <returns></returns>
        public override bool IsActive()
        {
            return Activity;
        }

        /// <summary>
        /// Создание указки.
        /// </summary>
        protected override void OnInit()
        {
            _controllerReference = InputAdapter.Instance.ControllerInteraction.Controller.GetFrom(gameObject);

            if (!_pointerGameObject)
            {
                _pointerGameObject = new GameObject("DistancePointer");

                _pointerGameObject.transform.parent = gameObject.transform;
                _pointerGameObject.transform.localPosition = Vector3.zero;
                _pointerGameObject.transform.localScale = Vector3.one;
                _pointerGameObject.transform.localRotation = Quaternion.identity;
                _pointerGameObject.layer = LayerMask.NameToLayer("Highlight");
            }

            var oldValueEnabled = _lineRenderer && _lineRenderer.enabled;
            _lineRenderer = Settings.CreateRenderer(_pointerGameObject);
            _lineRenderer.enabled = oldValueEnabled;
        }
    }
}