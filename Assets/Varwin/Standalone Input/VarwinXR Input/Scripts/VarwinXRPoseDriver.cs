using UnityEngine;
using UnityEngine.XR;

namespace Varwin.XR
{
    /// <summary>
    /// Класс, отвечающий за перемещение и поворот XR устройства.
    /// </summary>
    public class VarwinXRPoseDriver : MonoBehaviour
    {
        /// <summary>
        /// XR устройство.
        /// </summary>
        private InputDevice _inputDevice;

        /// <summary>
        /// Инициализирован ли.
        /// </summary>
        private bool _initalized = false;
        
        /// <summary>
        /// Сдвиг по позиции.
        /// </summary>
        public Vector3 _offsetPosition;
        
        /// <summary>
        /// Сдвиг по поворота.
        /// </summary>
        public Quaternion _offsetRotation;

        /// <summary>
        /// Инициализация.
        /// </summary>
        /// <param name="inputDevice">XR устройство.</param>
        /// <param name="offsetPosition">Сдвиг по позиции.</param>
        /// <param name="offsetRotation">Сдвиг по повороту.</param>
        public void Initialize(InputDevice inputDevice, Vector3 offsetPosition, Quaternion offsetRotation)
        {
            _inputDevice = inputDevice;
            _offsetPosition = offsetPosition;
            _offsetRotation = offsetRotation;
            _initalized = true;
        }

        /// <summary>
        /// Задать сдвиг по позиции и повороту.
        /// </summary>
        /// <param name="offsetRotation">Сдвиг по повороту.</param>
        /// <param name="offsetPosition">Сдвиг по позиции.</param>
        public void SetOffsets(Quaternion offsetRotation, Vector3 offsetPosition)
        {
            _offsetPosition = offsetPosition;
            _offsetRotation = offsetRotation;
        }
        
        /// <summary>
        /// Метод вызываемый каждый кадр.
        /// </summary>
        private void Update()
        {
            UpdateTransform();
        }

        /// <summary>
        /// Метод вызываемый каждый пересчет физики.
        /// </summary>
        private void FixedUpdate()
        {
            UpdateTransform();
        }

        /// <summary>
        /// Метод вызываемый каждый кадр.
        /// </summary>
        private void LateUpdate()
        {
            UpdateTransform();
        }

        /// <summary>
        /// Обновление трансформа.
        /// </summary>
        private void UpdateTransform()
        {
            if (!_initalized)
            {
                return;
            }

            if (_inputDevice.TryGetFeatureValue(CommonUsages.isTracked, out var value))
            {
                if (!value)
                {
                    return;
                }
            }

            if (_inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out var rotation))
            {
                transform.localRotation = rotation * _offsetRotation;
            }

            if (_inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out var position))
            {
                transform.localPosition = position + transform.localRotation * _offsetPosition;
            }
        }
    }
}