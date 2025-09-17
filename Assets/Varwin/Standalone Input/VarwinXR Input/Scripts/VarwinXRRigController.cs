using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace Varwin.XR
{
    /// <summary>
    /// Контроллер инициализации рига.
    /// </summary>
    public class VarwinXRRigController : MonoBehaviour
    {
        /// <summary>
        /// Проинициализиованные контроллеры.
        /// </summary>
        private Dictionary<string, GameObject> _loadedControllerInputDevices;

        /// <summary>
        /// Доступные модели контроллеров.
        /// </summary>
        public ControllerModels ControllerModels;

        /// <summary>
        /// Объект левой руки.
        /// </summary>
        public VarwinXRController LeftController;

        /// <summary>
        /// Объект правой руки.
        /// </summary>
        public VarwinXRController RightController;

        /// <summary>
        /// Имя текущего шлема.
        /// </summary>
        private string _currentHeadsetName;

        /// <summary>
        /// Имя эмулируемого шлема.
        /// </summary>
        public string EmulatedHeadsetName => VarwinXRSettings.EmulatedHeadsetName;
        
        /// <summary>
        /// Имя текущего шлема.
        /// </summary>
        public string CurrentHeadsetName => _currentHeadsetName;

        /// <summary>
        /// Система событий.
        /// </summary>
        public EventSystem EventSystem;
        
        /// <summary>
        /// Инициализация рига.
        /// </summary>
        private void Start()
        {
            DontDestroyOnLoad(this);
            _loadedControllerInputDevices = new Dictionary<string, GameObject>();

            InputDevices.deviceConnected += OnDeviceConnected;
            InputDevices.deviceDisconnected += OnDeviceDisconnected;
            ProjectData.SceneLoaded += OnSceneLoaded;

            VarwinXRSettings.HeadsetNameChanged += OnHeadsetNameChanged;

            TryInitializeDevices();
        }

        /// <summary>
        /// При смене имени шлема.
        /// </summary>
        /// <param name="headsetName">Имя шлема.</param>
        private void OnHeadsetNameChanged(string headsetName)
        {
            foreach (var loadedController in _loadedControllerInputDevices)
            {
                DestroyImmediate(loadedController.Value);
            }

            _loadedControllerInputDevices.Clear();
            TryInitializeDevices();
        }

        /// <summary>
        /// При наличии нескольких EventSystem почему-то становится главной именно система от XR. Сделал перезапуск при инициализации сцены.
        /// </summary>
        private void OnSceneLoaded()
        {
            EventSystem.enabled = false;
            EventSystem.enabled = true;
        }

        /// <summary>
        /// Подключение устройств.
        /// </summary>
        private void TryInitializeDevices()
        {
            var listDevices = new List<InputDevice>();
            InputDevices.GetDevices(listDevices);
            foreach (var inputDevice in listDevices)
            {
                OnDeviceConnected(inputDevice);
            }
        }

        /// <summary>
        /// При подключении устройства.
        /// </summary>
        /// <param name="inputDevice">XR устройство.</param>
        private void OnDeviceConnected(InputDevice inputDevice)
        {
            Debug.Log($"[Varwin XR]. Connected {inputDevice.name}. Subsystem: {XRSettings.loadedDeviceName}");
            if (_loadedControllerInputDevices.ContainsKey(GetUniqueName(inputDevice)))
            {
                return;
            }

            if (inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
            {
                InitController(inputDevice);
            }
        }

        /// <summary>
        /// Инициализация контроллера.
        /// </summary>
        /// <param name="inputDevice">XR устройство.</param>
        private void InitController(InputDevice inputDevice)
        {
            var isLeftController = inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Left);
            if (IHeadsetInfoProvider.GetInstance()?.IsSidesInverted() ?? false)
            {
                isLeftController = !isLeftController;
            }
            
            var headsetName = IHeadsetInfoProvider.GetInstance()?.GetHeadsetName();

            Debug.Log($"[Varwin XR]. Connected Headset '{headsetName}'. Subsystem {XRSettings.loadedDeviceName}'");

            GameObject prefab;
            if (!string.IsNullOrEmpty(VarwinXRSettings.EmulatedHeadsetName))
            {
                prefab = ControllerModels.GetControllerPrefab(VarwinXRSettings.EmulatedHeadsetName, isLeftController);
                _currentHeadsetName = VarwinXRSettings.EmulatedHeadsetName;
                if (!prefab)
                {
                    VarwinXRSettings.EmulatedHeadsetName = null;
                    return;
                }
            }
            else
            {
                prefab = ControllerModels.GetControllerPrefab(inputDevice.name, headsetName, ref _currentHeadsetName, isLeftController);
            }

            var hand = isLeftController ? LeftController : RightController;
            var poseDriver = hand.gameObject.GetComponent<VarwinXRPoseDriver>();
            if (!poseDriver)
            {
                poseDriver = hand.gameObject.AddComponent<VarwinXRPoseDriver>();
            }

            var instance = Instantiate(prefab, hand.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            instance.name = instance.name.Replace("(Clone)", "");

            var controllerModel = instance.GetComponent<VarwinXRControllerModel>();
            controllerModel.Initialize(inputDevice);
            poseDriver.Initialize(inputDevice, controllerModel.GetOffsetPosition(), controllerModel.GetOffsetRotation());
            _loadedControllerInputDevices.Add(GetUniqueName(inputDevice), instance);
            hand.Initialize(inputDevice, controllerModel);
        }

        /// <summary>
        /// Эмулировать модели контроллеров для шлема..
        /// </summary>
        /// <param name="headsetName">Имя шлема.</param>
        [Obsolete("Use VarwinXRSettings.SetEmulatedHeadset(..)")]
        public void SetEmulatedHeadset(string headsetName)
        {
            VarwinXRSettings.EmulatedHeadsetName = headsetName;
        }

        /// <summary>
        /// При отключении устройства.
        /// </summary>
        /// <param name="inputDevice">XR устройство.</param>
        private void OnDeviceDisconnected(InputDevice inputDevice)
        {
            var uniqueName = GetUniqueName(inputDevice);
            if (_loadedControllerInputDevices.ContainsKey(uniqueName))
            {
                Destroy(_loadedControllerInputDevices[uniqueName].gameObject);
                _loadedControllerInputDevices.Remove(uniqueName);
            }
            
            var isLeftController = inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Left);
            if (inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
            {
                var controller = isLeftController ? LeftController : RightController;
                controller.Deinitialize();
            }
        }

        /// <summary>
        /// Получение уникального имени устройства.
        /// </summary>
        /// <param name="inputDevice">XR устройство.</param>
        /// <returns>Уникальное имя устройства.</returns>
        private string GetUniqueName(InputDevice inputDevice)
        {
            return $"{inputDevice.name}_{inputDevice.characteristics}";
        }

        /// <summary>
        /// Отписка при удалении.
        /// </summary>
        private void OnDestroy()
        {
            InputDevices.deviceConnected -= OnDeviceConnected;
            InputDevices.deviceDisconnected -= OnDeviceDisconnected;
            ProjectData.SceneLoaded -= OnSceneLoaded;
            
            VarwinXRSettings.HeadsetNameChanged -= OnHeadsetNameChanged;
        }
    }
}