using UnityEngine;
using Varwin.PlatformAdapter;
using Varwin.Raycasters;

namespace Varwin.XR
{
    /// <summary>
    /// Указка для UI элементов.
    /// </summary>
    public class VarwinXRUIPointer : DefaultVarwinUIPointer
    {
        /// <summary>
        /// Контроллер.
        /// </summary>
        public VarwinXRController Controller;

        /// <summary>
        /// Переопределение рейкастера для базового класса.
        /// </summary>
        protected override IPointableRaycaster Raycaster => ControllerRaycaster;

        /// <summary>
        /// Луч контроллера для обработки всех столкновений с объектами.
        /// </summary>
        public VarwinXRControllerRaycaster ControllerRaycaster;

        /// <summary>
        /// Объект-отрисовки линии.
        /// </summary>
        private LineRenderer _lineRenderer;

        /// <summary>
        /// Конфигурация для отрисовки линии.
        /// </summary>
        [SerializeField] private VarwinUiPointerConfig _config;

        /// <summary>
        /// Переопределение типа взаимодействующей руки в зависимости от типа контроллера.
        /// </summary>
        protected override ControllerInteraction.ControllerHand Hand => _controllerHand;

        /// <summary>
        /// Определение энамки руки для текущего контроллера.
        /// </summary>
        private ControllerInteraction.ControllerHand _controllerHand;

        /// <summary>
        /// Может ли быть нажат.
        /// </summary>
        /// <returns>Истина если может.</returns>
        public override bool CanPress()
        {
            return base.CanPress() && Controller.InputHandler.IsTriggerPressed;
        }
        
        /// <summary>
        /// Задать состояние указки.
        /// </summary>
        /// <param name="value">Истина, если активировать.</param>
        public override void Toggle(bool value)
        {
            _lineRenderer.enabled = value;
        }

        /// <summary>
        /// Переключить состояние указки.
        /// </summary>
        public override void Toggle()
        {
            _lineRenderer.enabled = !_lineRenderer.enabled;
        }

        /// <summary>
        /// Может ли быть отжат.
        /// </summary>
        /// <returns>Истина если может.</returns>
        public override bool CanRelease()
        {
            return Controller.InputHandler.IsTriggerReleased;
        }

        /// <summary>
        /// Вызвать нажатие на объект.
        /// </summary>
        public override void Press()
        {
            if (_pressedPointableObject)
            {
                return;
            }

            base.Press();

            if (_pressedPointableObject)
            {
                _lineRenderer.startColor = _config.PressedColor;
                _lineRenderer.endColor = _config.PressedColor;
            }
        }

        /// <summary>
        /// Вызвать прекращение нажатия на объект.
        /// </summary>
        public override void Release()
        {
            base.Release();

            _lineRenderer.startColor = _config.IdleColor;
            _lineRenderer.endColor = _config.IdleColor;
        }

        /// <summary>
        /// Инициализация указки.
        /// </summary>
        public override void Init()
        {
            _lineRenderer = CreateDefaultControllerLine(gameObject.transform, _config);
            _controllerHand = Controller.IsLeft ? ControllerInteraction.ControllerHand.Left : ControllerInteraction.ControllerHand.Right;
        }

        /// <summary>
        /// Обновление состояния указки, а также поиск нового захваченного указкой объекта.
        /// </summary>
        public override void UpdateState()
        {
            base.UpdateState();

            if (!Controller || ControllerRaycaster.NearObjectRaycastHit == null || ProjectData.InteractionWithObjectsLocked)
            {
                return;
            }

            var originPosition = ControllerRaycaster.Controller.ControllerModel.PointerAnchor.position;

            if (DistancePointer.CustomSettings)
            {
                _lineRenderer.startWidth = DistancePointer.CustomSettings.StartWidth;
                _lineRenderer.endWidth = DistancePointer.CustomSettings.EndWidth;
            }

            _lineRenderer.SetPosition(0, originPosition);
            _lineRenderer.SetPosition(1, ControllerRaycaster.NearObjectRaycastHit.Value.point);
        }
    }
}