using System;
using UnityEngine;
using UnityEngine.XR;

namespace Varwin.XR
{
    /// <summary>
    /// Базовый элемент управления.
    /// </summary>
    public abstract class InputDeviceHandlerBase : MonoBehaviour
    {
        /// <summary>
        /// Событие, вызываемое при использовании элемента управления.
        /// </summary>
        public event Action<InputDeviceHandlerBase> Used;

        /// <summary>
        /// Имя фичи в InputDevice.
        /// </summary>
        public string PrimaryInputFeatureName;
        
        /// <summary>
        /// Индекс слоя в аниматоре.
        /// </summary>
        public int LayerIndex;
        
        /// <summary>
        /// Используется ли.
        /// </summary>
        private bool _isUsing = false;

        /// <summary>
        /// Используется ли.
        /// </summary>
        public bool IsUsing
        {
            get => _isUsing;
            set
            {
                if (value && !_isUsing)
                {
                    Used?.Invoke(this);
                }

                _isUsing = value;
            }
        }

        /// <summary>
        /// Обновление состояния элемента управления.
        /// </summary>
        /// <param name="animator">Аниматор.</param>
        /// <param name="inputDevice">Целевое XR устройство.</param>
        public abstract void UpdateState(Animator animator, InputDevice inputDevice);
    }
}