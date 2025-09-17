using UnityEngine;
using UnityEngine.XR;

namespace Varwin.XR
{
    /// <summary>
    /// Обработчик логического переключателя.
    /// </summary>
    public class InputDeviceHandlerBool : InputDeviceHandlerBase
    {
        /// <summary>
        /// Фича InputDevice.
        /// </summary>
        private InputFeatureUsage<bool> _usage;
        
        /// <summary>
        /// Имя свойства в аниматоре.
        /// </summary>
        public string AnimatorParameterName;
        
        /// <summary>
        /// Предыдущее значение.
        /// </summary>
        private bool _oldValue;

        /// <summary>
        /// Инициализация.
        /// </summary>
        private void Awake()
        {
            _usage = new InputFeatureUsage<bool>(PrimaryInputFeatureName);
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

            IsUsing = _oldValue != value;
            if (animator && !string.IsNullOrEmpty(AnimatorParameterName))
            {
                animator.SetFloat(AnimatorParameterName, value ? 1 : 0);
            }

            _oldValue = value;
        }
    }
}