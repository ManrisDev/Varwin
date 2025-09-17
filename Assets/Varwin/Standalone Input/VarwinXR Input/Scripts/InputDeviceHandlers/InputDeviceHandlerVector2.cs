using UnityEngine;
using UnityEngine.XR;

namespace Varwin.XR
{
    /// <summary>
    /// Обработчик векторного представления значения.
    /// </summary>
    public class InputDeviceHandlerVector2 : InputDeviceHandlerBase
    {
        /// <summary>
        /// Фича на получение векторного значения.
        /// </summary>
        private InputFeatureUsage<Vector2> _usage;
        
        /// <summary>
        /// Имя параметра в аниматоре для оси X.
        /// </summary>
        public string AnimatorParameterXName;

        /// <summary>
        /// Имя параметра в аниматоре для оси Y.
        /// </summary>
        public string AnimatorParameterYName;
        
        /// <summary>
        /// Предыдущее значение.
        /// </summary>
        private Vector2 _oldValue;

        /// <summary>
        /// Инвертирована ли горизонтальная ось.
        /// </summary>
        public bool InvertedX;

        /// <summary>
        /// Инвертирована ли вертикальная ось.
        /// </summary>
        public bool InvertedY;
        
        /// <summary>
        /// Инициализация обработчика.
        /// </summary>
        private void Awake()
        {
            _usage = new InputFeatureUsage<Vector2>(PrimaryInputFeatureName);
        }

        /// <summary>
        /// Обновление состояния элемента управления.
        /// </summary>
        /// <param name="animator">Аниматор.</param>
        /// <param name="inputDevice">Целевое XR устройство.</param>
        public override void UpdateState(Animator animator, InputDevice inputDevice)
        {
            if (!inputDevice.TryGetFeatureValue(_usage, out var value))
            {
                return;
            }

            value.x = InvertedX ? -value.x : value.x;
            value.y = InvertedY ? -value.y : value.y;

            IsUsing = Compare.IsEqual(_oldValue, value);
            animator.SetFloat(AnimatorParameterXName, value.x);
            animator.SetFloat(AnimatorParameterYName, value.y);
            _oldValue = value;
        }
    }
}