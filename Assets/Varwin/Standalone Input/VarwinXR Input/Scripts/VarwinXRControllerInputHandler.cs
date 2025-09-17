using UnityEngine;
using UnityEngine.XR;
using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    /// <summary>
    /// Обработчик взаимодействий с контроллером.
    /// </summary>
    public class VarwinXRControllerInputHandler : MonoBehaviour
    {
        /// <summary>
        /// Делегат события обработчика.
        /// </summary>
        /// <param name="sender">Объект, который вызвал событие.</param>
        public delegate void EventHandler(VarwinXRControllerInputHandler sender);

        /// <summary>
        /// Начальное значение поворота.
        /// </summary>
        private const float _turnThreshold = 0.75f;

        /// <summary>
        /// Начальное значение телепорта.
        /// </summary>
        private const float _teleportThreshold = 0.75f;

        /// <summary>
        /// Событие, вызываемое при инициализации.
        /// </summary>
        public event EventHandler Initialized;
        
        /// <summary>
        /// Событие, вызываемое при деинициализации.
        /// </summary>
        public event EventHandler Deinitialized;
        
        /// <summary>
        /// Событие, вызываемое при нажатии на кнопку меню.
        /// </summary>
        public event EventHandler ButtonMenuPressed;

        /// <summary>
        /// Событие, вызываемое при отпускании кнопки меню.
        /// </summary>
        public event EventHandler ButtonMenuReleased;
        
        /// <summary>
        /// Событие, вызываемое при нажатии на первую кнопку.
        /// </summary>
        public event EventHandler ButtonOnePressed;

        /// <summary>
        /// Событие, вызываемое при отпускании первой кнопки.
        /// </summary>
        public event EventHandler ButtonOneReleased;

        /// <summary>
        /// Событие, вызываемое при нажатии на первую кнопку.
        /// </summary>
        public event EventHandler ButtonTwoPressed;

        /// <summary>
        /// Событие, вызываемое при отпускании второй кнопки.
        /// </summary>
        public event EventHandler ButtonTwoReleased;

        /// <summary>
        /// Событие, вызываемое при нажатии на телепорт.
        /// </summary>
        public event EventHandler TeleportPressed;

        /// <summary>
        /// Событие, вызываемое при отжатии телепорта.
        /// </summary>
        public event EventHandler TeleportReleased;

        /// <summary>
        /// Событие, вызываемое при нажатии на поворот влево.
        /// </summary>
        public event EventHandler TurnLeftPressed;

        /// <summary>
        /// Событие, вызываемое при нажатии на поворота вправо.
        /// </summary>
        public event EventHandler TurnRightPressed;

        /// <summary>
        /// Событие, вызываемое при нажатии на триггер.
        /// </summary>
        public event EventHandler TriggerPressed;

        /// <summary>
        /// Событие, вызываемое при отпускании триггера.
        /// </summary>
        public event EventHandler TriggerReleased;
        
        /// <summary>
        /// Событие, вызываемое при нажатии на джойстик.
        /// </summary>
        public event EventHandler ThumbstickPressed;

        /// <summary>
        /// Событие, вызываемое при отпускании джойстика.
        /// </summary>
        public event EventHandler ThumbstickReleased;

        /// <summary>
        /// Событие, вызываемое при нажатии на захват.
        /// </summary>
        public event EventHandler GripPressed;

        /// <summary>
        /// Событие, вызываемое при отпускании кнопки захвата.
        /// </summary>
        public event EventHandler GripReleased;
        
        /// <summary>
        /// Нажат ли тачпад (поворот).
        /// </summary>
        public bool IsTurnPressed { get; private set; }
        
        /// <summary>
        /// Нажат ли телепорт.
        /// </summary>
        public bool IsTeleportPressed { get; private set; }

        /// <summary>
        /// Отпущен ли телепорт.
        /// </summary>
        public bool IsTeleportReleased { get; private set; }

        /// <summary>
        /// Нажат ли триггер.
        /// </summary>
        public bool IsTriggerPressed { get; private set; }

        /// <summary>
        /// Отпущен ли триггер.
        /// </summary>
        public bool IsTriggerReleased { get; private set; }

        /// <summary>
        /// Нажат ли захват.
        /// </summary>
        public bool IsGripPressed { get; private set; }

        /// <summary>
        /// Отпущен ли захват.
        /// </summary>
        public bool IsGripReleased { get; private set; }

        /// <summary>
        /// Нажата ли кнопка меню.
        /// </summary>
        public bool IsMenuButtonPressed { get; private set; }
        
        /// <summary>
        /// Нажата ли основная кнопка.
        /// </summary>
        public bool IsPrimaryButtonPressed { get; private set; }

        /// <summary>
        /// Нажата ли вспомогательная кнопка.
        /// </summary>
        public bool IsSecondaryButtonPressed { get; private set; }
        
        /// <summary>
        /// Значение стика.
        /// </summary>
        public Vector2 Primary2DAxisValue { get; private set; }
        
        /// <summary>
        /// Значение кнопки стика.
        /// </summary>
        public bool IsPrimary2DAxisButtonPressed { get; private set; }
        
        /// <summary>
        /// Значение стика при нажатии.
        /// </summary>
        public bool IsPrimary2DAxisPressed { get; private set; }

        /// <summary>
        /// Значение триггера.
        /// </summary>
        public float TriggerValue { get; private set; }

        /// <summary>
        /// Значение захвата.
        /// </summary>
        public float GripValue { get; private set; }

        /// <summary>
        /// XR устройство.
        /// </summary>
        private InputDevice _inputDevice;
        
        /// <summary>
        /// Телепорт с помощью стика.
        /// </summary>
        public bool TeleportOnThumbstick { get; private set; }
        
        /// <summary>
        /// Является ли трехосевым.
        /// </summary>
        public bool Is3Dof { get; private set; }

        /// <summary>
        /// Левый ли.
        /// </summary>
        public bool IsLeft => _inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Left);

        /// <summary>
        /// Фича для кнопки меню.
        /// </summary>
        private InputFeatureUsage<bool> _menuButtonFeature;

        /// <summary>
        /// Фича для первой кнопки.
        /// </summary>
        private InputFeatureUsage<bool> _primaryButtonFeature;

        /// <summary>
        /// Фича для второй кнопки.
        /// </summary>
        private InputFeatureUsage<bool> _secondaryButtonFeature;
        
        /// <summary>
        /// Фича для триггера.
        /// </summary>
        private InputFeatureUsage<float> _triggerFeature;

        /// <summary>
        /// Фича для оси.
        /// </summary>
        private InputFeatureUsage<Vector2> _primary2DAxisFeature;

        /// <summary>
        /// Фича для клика оси.
        /// </summary>
        private InputFeatureUsage<bool> _primary2DAxisClickFeature;
        
        /// <summary>
        /// Фича для захвата.
        /// </summary>
        private InputFeatureUsage<float> _gripFeature;

        /// <summary>
        /// Проинициализирован ли.
        /// </summary>
        private bool _initailized = false;

        /// <summary>
        /// Вызывать ли событие телепорта.
        /// </summary>
        public bool InvokeTeleport = true;

        /// <summary>
        /// Вызывать ли событие поворота.
        /// </summary>
        public bool InvokeRotate = true;

        /// <summary>
        /// Инициализация.
        /// </summary>
        /// <param name="inputDevice">XR устройство.</param>
        /// <param name="teleportOnThumbstick">Телепорт по стику.</param>
        /// <param name="is3dof">Является ли трехосевым.</param>
        /// <param name="keyMap">Компонент, отражающий соотношения InputDevice Action и ключом.</param>
        public void Initialize(InputDevice inputDevice, bool teleportOnThumbstick, bool is3dof, FeatureUsageMap keyMap)
        {
            _inputDevice = inputDevice;
            TeleportOnThumbstick = teleportOnThumbstick;
            Is3Dof = is3dof;

            var menuButton = keyMap ? keyMap.GetFeatureName(FeatureUsageKey.MenuButton) : FeatureUsageMap.GetDefaultValue(FeatureUsageKey.MenuButton);
            var primaryButton = keyMap ? keyMap.GetFeatureName(FeatureUsageKey.ButtonOne) : FeatureUsageMap.GetDefaultValue(FeatureUsageKey.ButtonOne);
            var secondaryButton = keyMap ? keyMap.GetFeatureName(FeatureUsageKey.ButtonTwo) : FeatureUsageMap.GetDefaultValue(FeatureUsageKey.ButtonTwo);
            var triggerButton = keyMap ? keyMap.GetFeatureName(FeatureUsageKey.Trigger) : FeatureUsageMap.GetDefaultValue(FeatureUsageKey.Trigger);
            var primary2DAxis = keyMap ? keyMap.GetFeatureName(FeatureUsageKey.PrimaryAxis2D) : FeatureUsageMap.GetDefaultValue(FeatureUsageKey.PrimaryAxis2D);
            var primary2DAxisClick = keyMap ? keyMap.GetFeatureName(FeatureUsageKey.PrimaryAxis2DClick) : FeatureUsageMap.GetDefaultValue(FeatureUsageKey.PrimaryAxis2DClick);
            var gripButton = keyMap ? keyMap.GetFeatureName(FeatureUsageKey.Grip) : FeatureUsageMap.GetDefaultValue(FeatureUsageKey.Grip);
            
            _menuButtonFeature = new InputFeatureUsage<bool>(menuButton);
            _primaryButtonFeature = new InputFeatureUsage<bool>(primaryButton);
            _secondaryButtonFeature = new InputFeatureUsage<bool>(secondaryButton);
            _triggerFeature = new InputFeatureUsage<float>(triggerButton);
            _primary2DAxisFeature = new InputFeatureUsage<Vector2>(primary2DAxis);
            _primary2DAxisClickFeature = new InputFeatureUsage<bool>(primary2DAxisClick);
            _gripFeature = new InputFeatureUsage<float>(gripButton);
            
            _initailized = true;
            Initialized?.Invoke(this);
        }

        /// <summary>
        /// Деинициализация.
        /// </summary>
        public void Deinitialize()
        {
            _initailized = false;
            Deinitialized?.Invoke(this);
        }
        
        /// <summary>
        /// Обработка событий.
        /// </summary>
        private void Update()
        {
            Process2DAxis();
            ProcessGrip();
            ProcessTrigger();
            ProcessButtons();
        }

        /// <summary>
        /// Обработка кнопок.
        /// </summary>
        private void ProcessButtons()
        {
            if (_inputDevice.TryGetFeatureValue(_menuButtonFeature, out var menuButtonValue))
            {
                if (menuButtonValue != IsMenuButtonPressed)
                {
                    if (menuButtonValue)
                    {
                        ButtonMenuPressed?.Invoke(this);
                    }
                    else
                    {
                        ButtonMenuReleased?.Invoke(this);
                    }

                    IsMenuButtonPressed = menuButtonValue;
                }
            }

            if (_inputDevice.TryGetFeatureValue(_primaryButtonFeature, out var primaryButtonValue))
            {
                if (primaryButtonValue != IsPrimaryButtonPressed)
                {
                    if (primaryButtonValue)
                    {
                        ButtonOnePressed?.Invoke(this);
                    }
                    else
                    {
                        ButtonOneReleased?.Invoke(this);
                    }

                    IsPrimaryButtonPressed = primaryButtonValue;
                }
            }

            if (_inputDevice.TryGetFeatureValue(_secondaryButtonFeature, out var secondaryButtonValue))
            {
                if (secondaryButtonValue != IsSecondaryButtonPressed)
                {
                    if (secondaryButtonValue)
                    {
                        ButtonTwoPressed?.Invoke(this);
                    }
                    else
                    {
                        ButtonTwoReleased?.Invoke(this);
                    }

                    IsSecondaryButtonPressed = secondaryButtonValue;
                }
            }
        }

        /// <summary>
        /// Обработка триггера.
        /// </summary>
        private void ProcessTrigger()
        {
            if (!_inputDevice.TryGetFeatureValue(_triggerFeature, out var value))
            {
                return;
            }

            IsTriggerReleased = false;
            if (value >= _turnThreshold && TriggerValue < _turnThreshold)
            {
                TriggerPressed?.Invoke(this);
                IsTriggerPressed = true;
            }
            else if (value < _turnThreshold && TriggerValue >= _turnThreshold)
            {
                IsTriggerReleased = true;
                TriggerReleased?.Invoke(this);
                IsTriggerPressed = false;
            }

            TriggerValue = value;
        }

        /// <summary>
        /// Обработка стика.
        /// </summary>
        private void Process2DAxis()
        {
            ProcessPrimaryAxisButton();
            
            if (Is3Dof)
            {
                ProcessTrackpad3Dof();
                return;
            }

            if (TeleportOnThumbstick)
            {
                ProcessThumbstick();
            }
            else
            {
                ProcessTrackpad();
            }
        }

        /// <summary>
        /// Обработка нажатия на стик.
        /// </summary>
        private void ProcessPrimaryAxisButton()
        {
            if (!_inputDevice.TryGetFeatureValue(_primary2DAxisClickFeature, out var clickValue))
            {
                clickValue = false;
            }

            var oldValue = IsPrimary2DAxisButtonPressed;
            IsPrimary2DAxisButtonPressed = clickValue;
            
            if (!oldValue && clickValue)
            {
                ThumbstickPressed?.Invoke(this);
            }
            else if (oldValue && !clickValue)
            {
                ThumbstickReleased?.Invoke(this);
            }
        }

        /// <summary>
        /// Обработка нажатий 3Dof контроллера.
        /// </summary>
        private void ProcessTrackpad3Dof()
        {
            IsTeleportReleased = false;
            IsPrimary2DAxisPressed = false;

            if (!_inputDevice.TryGetFeatureValue(_primary2DAxisFeature, out var vectorValue))
            {
                return;
            }
            
            if (!_inputDevice.TryGetFeatureValue(_primary2DAxisClickFeature, out var clickValue))
            {
                return;
            }

            IsPrimary2DAxisPressed = clickValue;
            
            if (!InvokeTeleport)
            {
                ResetTeleport();
            }
            else
            {
                if (clickValue && !IsTeleportPressed && vectorValue.x > -_turnThreshold && vectorValue.x < _turnThreshold && !IsTurnPressed && vectorValue.y > 0)
                {
                    IsTeleportPressed = true;
                    TeleportPressed?.Invoke(this);
                }
                else if (!clickValue && IsTeleportPressed)
                {
                    IsTeleportPressed = false;
                    IsTeleportReleased = true;
                    TeleportReleased?.Invoke(this);
                }
            }

            if (InvokeRotate)
            {
                if (vectorValue.x > _turnThreshold && clickValue && !IsTurnPressed && !IsTeleportPressed)
                {
                    TurnRightPressed?.Invoke(this);
                    IsTurnPressed = true;
                }

                if (vectorValue.x < -_turnThreshold && clickValue && !IsTurnPressed && !IsTeleportPressed)
                {
                    TurnLeftPressed?.Invoke(this);
                    IsTurnPressed = true;
                }
            }

            if (!clickValue)
            {
                IsTurnPressed = false;
            }
            
            IsGripReleased = false;
            if (vectorValue.x > -_turnThreshold && vectorValue.x < _turnThreshold && clickValue && !IsGripPressed && vectorValue.y < 0)
            {
                IsGripPressed = true;
                GripPressed?.Invoke(this);
                GripValue = 1f;
            }
            else if (!clickValue && IsGripPressed)
            {
                IsGripReleased = true;
                IsGripPressed = false;
                GripReleased?.Invoke(this);
                GripValue = 0f;
            }
            
            Primary2DAxisValue = vectorValue;
        }

        /// <summary>
        /// Сброс телепорта.
        /// </summary>
        private void ResetTeleport()
        {
            if (IsTeleportPressed)
            {
                IsTeleportPressed = false;
                IsTeleportReleased = true;
                TeleportReleased?.Invoke(this);
            }
        }

        /// <summary>
        /// Обработка трекпада.
        /// </summary>
        private void ProcessTrackpad()
        {
            IsTeleportReleased = false;
            IsPrimary2DAxisPressed = false;

            if (!_inputDevice.TryGetFeatureValue(_primary2DAxisFeature, out var vectorValue))
            {
                return;
            }
            
            if (!_inputDevice.TryGetFeatureValue(_primary2DAxisClickFeature, out var clickValue))
            {
                return;
            }

            IsPrimary2DAxisPressed = clickValue;

            if (!InvokeTeleport)
            {
                ResetTeleport();
            }
            else
            {
                if (clickValue && !IsTeleportPressed && vectorValue.x > -_turnThreshold && vectorValue.x < _turnThreshold && !IsTurnPressed)
                {
                    IsTeleportPressed = true;
                    TeleportPressed?.Invoke(this);
                }
                else if (!clickValue && IsTeleportPressed)
                {
                    IsTeleportPressed = false;
                    IsTeleportReleased = true;
                    TeleportReleased?.Invoke(this);
                }
            }

            if (InvokeRotate)
            {
                if (vectorValue.x > _turnThreshold && clickValue && !IsTurnPressed && !IsTeleportPressed)
                {
                    TurnRightPressed?.Invoke(this);
                    IsTurnPressed = true;
                }

                if (vectorValue.x < -_turnThreshold && clickValue && !IsTurnPressed && !IsTeleportPressed)
                {
                    TurnLeftPressed?.Invoke(this);
                    IsTurnPressed = true;
                }
            }

            if (!clickValue)
            {
                IsTurnPressed = false;
            }
            
            Primary2DAxisValue = vectorValue;
        }

        /// <summary>
        /// Обработка стика.
        /// </summary>
        private void ProcessThumbstick()
        {
            IsTeleportReleased = false;
            IsPrimary2DAxisPressed = false;

            if (!_inputDevice.TryGetFeatureValue(_primary2DAxisFeature, out var value))
            {
                return;
            }
            
            if (!_inputDevice.TryGetFeatureValue(_primary2DAxisClickFeature, out var clickValue))
            {
                return;
            }

            IsPrimary2DAxisPressed = clickValue;

            if (InvokeRotate)
            {
                if (value.x > _turnThreshold && Primary2DAxisValue.x < _turnThreshold)
                {
                    TurnRightPressed?.Invoke(this);
                }

                if (value.x < -_turnThreshold && Primary2DAxisValue.x > -_turnThreshold)
                {
                    TurnLeftPressed?.Invoke(this);
                }
            }

            if (!InvokeTeleport)
            {
                ResetTeleport();
            }
            else
            {
                if (Mathf.Abs(Primary2DAxisValue.y) < _teleportThreshold && Mathf.Abs(value.y) > _teleportThreshold)
                {
                    IsTeleportPressed = true;
                    TeleportPressed?.Invoke(this);
                }

                if (Mathf.Abs(Primary2DAxisValue.y) > _teleportThreshold && Mathf.Abs(value.y) < _teleportThreshold)
                {
                    IsTeleportPressed = false;
                    IsTeleportReleased = true;
                    TeleportReleased?.Invoke(this);
                }
            }

            Primary2DAxisValue = value;
        }

        /// <summary>
        /// Обработка захвата.
        /// </summary>
        private void ProcessGrip()
        {
            if (Is3Dof)
            {
                return;
            }
            
            IsGripReleased = false;

            if (!_inputDevice.TryGetFeatureValue(_gripFeature, out var value))
            {
                return;
            }

            if (value >= _turnThreshold && GripValue < _turnThreshold)
            {
                IsGripPressed = true;
                GripPressed?.Invoke(this);
            }

            if (value < _turnThreshold && GripValue >= _turnThreshold)
            {
                IsGripPressed = false;
                IsGripReleased = true;
                GripReleased?.Invoke(this);
            }

            GripValue = value;
        }

        /// <summary>
        /// Нажата ли кнопка.
        /// </summary>
        /// <param name="buttonAlias">Кнопка.</param>
        /// <returns>Истина, если нажата.</returns>
        public bool IsButtonPressed(ControllerInput.ButtonAlias buttonAlias)
        {
            return buttonAlias switch
            {
                ControllerInput.ButtonAlias.ButtonOnePress => IsPrimaryButtonPressed,
                ControllerInput.ButtonAlias.ButtonTwoPress => IsSecondaryButtonPressed,
                ControllerInput.ButtonAlias.GripPress => IsGripPressed,
                ControllerInput.ButtonAlias.TriggerPress => IsTriggerPressed,
                ControllerInput.ButtonAlias.TouchpadPress => IsTeleportPressed,
                _ => false
            };
        }
    }
}
