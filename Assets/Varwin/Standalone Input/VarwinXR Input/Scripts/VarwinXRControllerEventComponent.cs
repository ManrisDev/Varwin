using System;
using UnityEngine;
using UnityEngine.XR;
using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    /// <summary>
    /// Обработчик событий контроллера.
    /// </summary>
    public class VarwinXRControllerEventComponent : MonoBehaviour
    {
        /// <summary>
        /// Обработчик событий контроллера.
        /// </summary>
        public VarwinXRControllerInputHandler InputHandler;
        
        /// <summary>
        /// Контроллер.
        /// </summary>
        private VarwinXRController _controller;

        /// <summary>
        /// Контроллер.
        /// </summary>
        public VarwinXRController Controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<VarwinXRController>();
                }

                return _controller;
            }
        }

        /// <summary>
        /// Определение события.
        /// </summary>
        /// <param name="sender">Контроллер событий.</param>
        public delegate void ButtonEventHandler(VarwinXRControllerEventComponent sender);

        /// <summary>
        /// Событие, вызываемое при нажатии на первую кнопку.
        /// </summary>
        public event ButtonEventHandler ButtonOnePressed;

        /// <summary>
        /// Событие, вызываемое при отпускании первой кнопки.
        /// </summary>
        public event ButtonEventHandler ButtonOneReleased;

        /// <summary>
        /// Событие, вызываемое при нажатии на первую кнопку.
        /// </summary>
        public event ButtonEventHandler ButtonTwoPressed;

        /// <summary>
        /// Событие, вызываемое при отпускании второй кнопки.
        /// </summary>
        public event ButtonEventHandler ButtonTwoReleased;

        /// <summary>
        /// Событие, вызываемое при нажатии на телепорт.
        /// </summary>
        public event ButtonEventHandler ThumbstickPressed;

        /// <summary>
        /// Событие, вызываемое при отжатии телепорта.
        /// </summary>
        public event ButtonEventHandler ThumbstickReleased;
        
        /// <summary>
        /// Событие, вызываемое при нажатии джойстика.
        /// </summary>
        public event ButtonEventHandler ThumbstickButtonPressed;

        /// <summary>
        /// Событие, вызываемое при отпускании джойстика.
        /// </summary>
        public event ButtonEventHandler ThumbstickButtonReleased;

        /// <summary>
        /// Событие, вызываемое при нажатии на поворот влево.
        /// </summary>
        public event ButtonEventHandler TurnLeftPressed;

        /// <summary>
        /// Событие, вызываемое при нажатии на поворота вправо.
        /// </summary>
        public event ButtonEventHandler TurnRightPressed;

        /// <summary>
        /// Событие, вызываемое при нажатии на триггер.
        /// </summary>
        public event ButtonEventHandler TriggerPressed;

        /// <summary>
        /// Событие, вызываемое при отпускании триггера.
        /// </summary>
        public event ButtonEventHandler TriggerReleased;

        /// <summary>
        /// Событие, вызываемое при нажатии на захват.
        /// </summary>
        public event ButtonEventHandler GripPressed;

        /// <summary>
        /// Событие, вызываемое при отпускании кнопки захвата.
        /// </summary>
        public event ButtonEventHandler GripReleased;
        
        /// <summary>
        /// Событие, вызываемое при инициализации.
        /// </summary>
        public event ButtonEventHandler Initialized;

        /// <summary>
        /// Событие, вызываемое при деинициализации.
        /// </summary>
        public event ButtonEventHandler Deinitialized;

        /// <summary>
        /// Проинициализирован ли.
        /// </summary>
        /// <returns>Истина, если проинициализирован.</returns>
        public bool IsInitialized() => Controller.Initialized;

        /// <summary>
        /// Нажат ли телепорт.
        /// </summary>
        /// <returns>Истина, если нажата.</returns>
        public bool IsThumbstickPressed() => InputHandler.IsTeleportPressed;

        /// <summary>
        /// Отпущен ли телепорт.
        /// </summary>
        /// <returns>Истина, если отпущен.</returns>
        public bool IsThumbstickReleased() =>InputHandler.IsTeleportReleased;

        /// <summary>
        /// Нажат ли триггер.
        /// </summary>
        /// <returns>Истина, если нажат.</returns>
        public bool IsTriggerPressed() => InputHandler.IsTriggerPressed;

        /// <summary>
        /// Отпущен ли триггер.
        /// </summary>
        /// <returns>Истина, если отпущен.</returns>
        public bool IsTriggerReleased() => InputHandler.IsTriggerReleased;

        /// <summary>
        /// Есть ли взятый объект.
        /// </summary>
        /// <returns>Истина, если есть.</returns>
        public bool HasGrabbedObject => Controller && Controller.GrabbedObject;

        /// <summary>
        /// Значение стика.
        /// </summary>
        public Vector2 Primary2DAxisValue => InputHandler.Primary2DAxisValue;

        /// <summary>
        /// Значение триггера.
        /// </summary>
        public float TriggerValue => InputHandler.TriggerValue;

        /// <summary>
        /// Значение захвата.
        /// </summary>
        public float GripValue => InputHandler.GripValue;

        /// <summary>
        /// Левый ли.
        /// </summary>
        public bool IsLeft => InputHandler.IsLeft;

        /// <summary>
        /// Инициализация контроллера.
        /// </summary>
        private void Start()
        {
            InputHandler.Initialized += OnInitialized;
            InputHandler.Deinitialized += OnDeinitialized;
            InputHandler.ButtonOnePressed += OnButtonOnePressed;
            InputHandler.ButtonOneReleased += OnButtonOneReleased;
            InputHandler.ButtonTwoPressed += OnButtonTwoPressed;
            InputHandler.ButtonTwoReleased += OnButtonTwoReleased;
            InputHandler.TeleportPressed += OnTeleportPressed;
            InputHandler.TeleportReleased += OnTeleportReleased;
            InputHandler.GripPressed += OnGripPressed;
            InputHandler.GripReleased += OnGripReleased;
            InputHandler.TriggerPressed += OnTriggerPressed;
            InputHandler.TriggerReleased += OnTriggerReleased;
            InputHandler.TurnLeftPressed += OnTurnLeftPressed;
            InputHandler.TurnRightPressed += OnTurnRightPressed;
            InputHandler.ThumbstickPressed += OnThumbstickPressed;
            InputHandler.ThumbstickReleased += OnThumbstickReleased;
        }

        /// <summary>
        /// При инициализации.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnInitialized(VarwinXRControllerInputHandler sender)
        {   
            Initialized?.Invoke(this);
        }
        
        /// <summary>
        /// При деинициализации.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnDeinitialized(VarwinXRControllerInputHandler sender)
        {
            Deinitialized?.Invoke(this);
        }

        /// <summary>
        /// При нажатии кнопки джойстика.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnThumbstickPressed(VarwinXRControllerInputHandler sender)
        {
            ThumbstickButtonPressed?.Invoke(this);
        }

        /// <summary>
        /// При отпускании кнопки джойстика.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnThumbstickReleased(VarwinXRControllerInputHandler sender)
        { 
            ThumbstickButtonReleased?.Invoke(this);
        }
        
        /// <summary>
        /// При нажатии основной кнопки.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnButtonOnePressed(VarwinXRControllerInputHandler sender)
        {
            ButtonOnePressed?.Invoke(this);
        }

        /// <summary>
        /// При отпускании основной кнопки.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnButtonOneReleased(VarwinXRControllerInputHandler sender)
        {
            ButtonOneReleased?.Invoke(this);
        }

        /// <summary>
        /// При нажатии второстепенной кнопки.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnButtonTwoPressed(VarwinXRControllerInputHandler sender)
        {
            ButtonTwoPressed?.Invoke(this);   
        }

        /// <summary>
        /// При отпускании второстепенной кнопки.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnButtonTwoReleased(VarwinXRControllerInputHandler sender)
        {
            ButtonTwoReleased?.Invoke(this);
        }

        /// <summary>
        /// При нажатии телепорта.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnTeleportPressed(VarwinXRControllerInputHandler sender)
        {
            ThumbstickPressed?.Invoke(this);
        }

        /// <summary>
        /// При отпускании телепорта.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnTeleportReleased(VarwinXRControllerInputHandler sender)
        {
            ThumbstickReleased?.Invoke(this);
        }

        /// <summary>
        /// При нажатии захвата.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnGripPressed(VarwinXRControllerInputHandler sender)
        {
            GripPressed?.Invoke(this);
        }

        /// <summary>
        /// При отпускании захвата.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnGripReleased(VarwinXRControllerInputHandler sender)
        {
            GripReleased?.Invoke(this);
        }

        /// <summary>
        /// При нажатии триггера.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnTriggerPressed(VarwinXRControllerInputHandler sender)
        {
            TriggerPressed?.Invoke(this);
        }

        /// <summary>
        /// При отпускании триггера.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnTriggerReleased(VarwinXRControllerInputHandler sender)
        {
            TriggerReleased?.Invoke(this);
        }

        /// <summary>
        /// При повороте налево.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnTurnLeftPressed(VarwinXRControllerInputHandler sender)
        {
            TurnLeftPressed?.Invoke(this);
        }

        /// <summary>
        /// При повороте направо.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        private void OnTurnRightPressed(VarwinXRControllerInputHandler sender)
        {
            TurnRightPressed?.Invoke(this);
        }

        /// <summary>
        /// Нажата ли кнопка.
        /// </summary>
        /// <param name="buttonAlias">Кнопка.</param>
        /// <returns>Истина, если нажата.</returns>
        public bool IsButtonPressed(ControllerInput.ButtonAlias buttonAlias)
        {
            return InputHandler.IsButtonPressed(buttonAlias);
        }

        /// <summary>
        /// Метод, вызываемый при выпускании.
        /// </summary>
        /// <param name="controllerInteractionEventArgs">Аргументы метода.</param>
        public void OnGripReleased(ControllerInput.ControllerInteractionEventArgs controllerInteractionEventArgs)
        {
        }

        /// <summary>
        /// Отписка.
        /// </summary>
        private void OnDestroy()
        {
            if (!InputHandler)
            {
                return;
            }
            
            InputHandler.Initialized -= OnInitialized;
            InputHandler.Deinitialized -= OnDeinitialized;
            InputHandler.ButtonOnePressed -= OnButtonOnePressed;
            InputHandler.ButtonOneReleased -= OnButtonOneReleased;
            InputHandler.ButtonTwoPressed -= OnButtonTwoPressed;
            InputHandler.ButtonTwoReleased -= OnButtonTwoReleased;
            InputHandler.TeleportPressed -= OnTeleportPressed;
            InputHandler.TeleportReleased -= OnTeleportReleased;
            InputHandler.GripPressed -= OnGripPressed;
            InputHandler.GripReleased -= OnGripReleased;
            InputHandler.TriggerPressed -= OnTriggerPressed;
            InputHandler.TriggerReleased -= OnTriggerReleased;
            InputHandler.TurnLeftPressed -= OnTurnLeftPressed;
            InputHandler.TurnRightPressed -= OnTurnRightPressed;
        }
    }
}