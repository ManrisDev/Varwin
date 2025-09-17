using UnityEngine;
using UnityEngine.XR;

namespace Varwin.XR
{
    /// <summary>
    /// Обработчик дискретного элемента управления.
    /// </summary>
    public class InputDeviceHandlerFloat : InputDeviceHandlerBase
    {
        /// <summary>
        /// Объявленная фича для работы с вещественным числом.
        /// </summary>
        private InputFeatureUsage<float> _usage;
        
        /// <summary>
        /// Имя поля в аниматоре.
        /// </summary>
        public string AnimatorParameterName;
        
        /// <summary>
        /// Предыдущее значение.
        /// </summary>
        private float _oldValue;

        /// <summary>
        /// Инициализация обработчика.
        /// </summary>
        private void Awake()
        {
            _usage = new InputFeatureUsage<float>(PrimaryInputFeatureName);
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

            IsUsing = Compare.IsEqual(_oldValue, value);
            animator.SetFloat(AnimatorParameterName, value);
            _oldValue = value;
        }
    }
}