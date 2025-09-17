using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Varwin.PlatformAdapter;
using Varwin.XR.Types;

namespace Varwin.XR
{
    /// <summary>
    /// Контроллер модели.
    /// </summary>
    public class VarwinXRControllerModel : MonoBehaviour
    {
        /// <summary>
        /// Аниматор контроллера.
        /// </summary>
        public Animator InputDeviceHandler;

        /// <summary>
        /// Используемое устройство управления.
        /// </summary>
        private InputDevice _inputDevice;

        /// <summary>
        /// Список элементов управления
        /// </summary>
        private InputDeviceHandlerBase[] _controls;

        /// <summary>
        /// Кастомная точка вызова указки.
        /// </summary>
        public Transform CustomPointerAnchor;
        
        /// <summary>
        /// Нативная точка вызова указки.
        /// </summary>
        [SerializeField] private Transform _nativePointerAnchor;

        /// <summary>
        /// Точка вызова указки.
        /// </summary>
        public Transform PointerAnchor => GetPointerAnchor();

        /// <summary>
        /// Левая ли рука.
        /// </summary>
        public bool IsLeftHand => _inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Left);

        /// <summary>
        /// Информация о сдвиге для каждой платформы.
        /// </summary>
        public List<PlatformOffsetInfo> PlatformOffsetInfos;

        /// <summary>
        /// Контейнер объектов управления.
        /// </summary>
        [SerializeField]
        private List<ControlTooltipHandler> _controlTooltipHandlers;

        /// <summary>
        /// Телепорт на стике.
        /// </summary>
        public bool TeleportOnThumbstick;

        /// <summary>
        /// Является ли трехосевым.
        /// </summary>
        public bool Is3Dof;

        /// <summary>
        /// Якорь руки для взаимодействия.
        /// </summary>
        public GameObject GrabAnchor;

        /// <summary>
        /// Заполнение списка.
        /// </summary>
        private void OnValidate()
        {
            if (_controlTooltipHandlers == null || _controlTooltipHandlers.Count == 0)
            {
                _controlTooltipHandlers = gameObject.GetComponentsInChildren<ControlTooltipHandler>(true)?.ToList();
            }
        }
        
        /// <summary>
        /// Инициализация контроллера.
        /// </summary>
        /// <param name="inputDevice">XR устройство.</param>
        public void Initialize(InputDevice inputDevice)
        {
            _inputDevice = inputDevice;
            _controls = GetComponentsInChildren<InputDeviceHandlerBase>();
        }

        /// <summary>
        /// Возвращает якорь привязки указки.
        /// </summary>
        /// <returns>Якорь привязки указки.</returns>
        private Transform GetPointerAnchor()
        {
            if (VarwinXRSettings.UseVarwinPointerDirection)
            {
                return CustomPointerAnchor ? CustomPointerAnchor : transform;
            }
            
            if (!_nativePointerAnchor)
            {
                return CustomPointerAnchor ? CustomPointerAnchor : transform;
            }

            return _nativePointerAnchor;
        }
        
        /// <summary>
        /// Обновление аниматора.
        /// </summary>
        private void Update()
        {
            UpdateControls();
        }

        /// <summary>
        /// Обновление состояния кнопок.
        /// </summary>
        private void UpdateControls()
        {
            foreach (var control in _controls)
            {
                control.UpdateState(InputDeviceHandler, _inputDevice);
            }
        }

        /// <summary>
        /// Получение сдвига по повороту с учетом якорной точки.
        /// </summary>
        /// <returns>Кватернион сдвига.</returns>
        public Quaternion GetOffsetRotation()
        {
            var offsetInfo = GetOffsetInfo();
            if (offsetInfo == null)
            {
                return Quaternion.identity;
            }

            return Quaternion.Inverse(transform.rotation) * offsetInfo.Anchor.rotation * Quaternion.Euler(offsetInfo.AngleOffset);
        }

        /// <summary>
        /// Получение информации о сдвиге.
        /// </summary>
        /// <returns>Информация о сдвиге.</returns>
        private PlatformOffsetInfo GetOffsetInfo()
        {
            var offsetsList = PlatformOffsetInfos.FindAll(a => a.Name == XRSettings.loadedDeviceName);
            
#if UNITY_ANDROID && !UNITY_EDITOR            
            var offsetInfo = offsetsList.Find(a => !a.IsDesktopOnly);
#else
            var offsetInfo = offsetsList.Find(a => a.IsDesktopOnly);
#endif
            return offsetInfo ?? offsetsList.FirstOrDefault();
        }

        /// <summary>
        /// Получение сдвига по позиции с учетом якорной точки.
        /// </summary>
        /// <returns>Кватернион сдвига.</returns>
        public Vector3 GetOffsetPosition()
        {
            var offsetInfo = GetOffsetInfo();
            if (offsetInfo == null)
            {
                return Vector3.zero;
            }

            return Quaternion.Euler(offsetInfo.AngleOffset) * (Quaternion.Inverse(transform.rotation) * (offsetInfo.Anchor.position - transform.position));
        }
        
        /// <summary>
        /// Получить объект отдельного органа управления по типу. 
        /// </summary>
        /// <param name="type">Тип.</param>
        /// <returns>Объект.</returns>
        public GameObject GetControlWithType(ControllerInteraction.ControllerElements type)
        {
            return _controlTooltipHandlers?.Find(a => a.Type == type)?.gameObject;
        }
    }
}