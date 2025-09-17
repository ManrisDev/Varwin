using UnityEngine;
using UnityEngine.XR;

namespace Varwin.XR
{
    /// <summary>
    /// Настройщик шлема в момент инициализации на Oculus шлеме.
    /// В начальный момент положение камеры находится в нулях. Данный класс искусственно сдвигает камеру
    /// </summary>
    public class VarwinXRCameraAdjust : MonoBehaviour
    {
        /// <summary>
        /// Стандартный рост игрока.
        /// </summary>
        private const float DefaultPlayerHeight = 1.3f;
        
        /// <summary>
        /// Камера.
        /// </summary>
        [SerializeField] private Camera _camera;
        
        /// <summary>
        /// Инициализировано ли устройство.
        /// </summary>
        private bool _isDeviceInitialized = false;

        /// <summary>
        /// Фикс проверки инициализации устройства.
        /// </summary>
        private void FixedUpdate()
        {
            if (_isDeviceInitialized)
            {
                return;
            }
            
            _camera.transform.localPosition = Vector3.up * DefaultPlayerHeight;
            var head = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
            if (!head.TryGetFeatureValue(CommonUsages.centerEyePosition, out var position))
            {
                return;
            }
            
            if (position.magnitude > 0.001f)
            {
                _isDeviceInitialized = true;
            }
        }
    }
}