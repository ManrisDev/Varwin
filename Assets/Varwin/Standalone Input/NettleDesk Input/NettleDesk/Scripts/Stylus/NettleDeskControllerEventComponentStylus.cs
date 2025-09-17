using UnityEngine;
using Varwin.NettleDesk;
using Varwin.PlatformAdapter;

namespace Varwin.NettleDeskPlayer
{
    /// <summary>
    /// Компонент, описыващий взаимодействие через стилус.
    /// </summary>
    public class NettleDeskControllerEventComponentStylus : NettleDeskControllerEventComponentBase
    {
        /// <summary>
        /// Объект стилуса.
        /// </summary>
        public NettleDeskStylus Stylus;

        /// <summary>
        /// Подписка на события стилуса.
        /// </summary>
        private void Awake()
        {
            Stylus.InputHandler.UsePressed += OnUsePressed;
            Stylus.InputHandler.UseReleased += OnUseReleased;
            Stylus.InputHandler.GrabPressed += OnGrabPressed;
            Stylus.InputHandler.GrabReleased += OnGrabReleased;
            Stylus.InputHandler.Initialized += OnInitialized;
            Stylus.InputHandler.Deinitialized += OnDeinitialized;
        }
        
        /// <summary>
        /// Получить аргументы событий.
        /// </summary>
        /// <returns>Аргументы событий.</returns>
        private ControllerInput.ControllerInteractionEventArgs GetArguments()
        {
            return new()
            {
                controllerReference = new PlayerController.ControllerReferenceArgs()
                {
                    hand = Stylus.IsLeft ? ControllerInteraction.ControllerHand.Left : ControllerInteraction.ControllerHand.Right
                }
            };
        }
        
        /// <summary>
        /// При деинициализации сообщаем InputAdapter'у об отключении.
        /// </summary>
        private void OnDeinitialized()
        {
            InvokeControllerDisabled(this, GetArguments());
        }
        
        /// <summary>
        /// При инициализации сообщаем InputAdapter'у о подключении.
        /// </summary>
        private void OnInitialized()
        {
            InvokeControllerEnabled(this, GetArguments());
        }

        /// <summary>
        /// При использовании сообщаем InputAdapter'у об использовании.
        /// </summary>
        private void OnUsePressed()
        {
            InvokeTriggerPressed(this, GetArguments());
        }

        /// <summary>
        /// При прекращении использования сообщаем InputAdapter'у о прекращении использования.
        /// </summary>
        private void OnUseReleased()
        {
            InvokeTriggerReleased(this, GetArguments());
        }

        /// <summary>
        /// При грабе сообщаем InputAdapter'у о захвате.
        /// </summary>
        private void OnGrabPressed()
        {
            InvokeGripPressed(this, GetArguments());
        }

        /// <summary>
        /// При выпускании из руки сообщаем InputAdapter'у о прекращении захвата.
        /// </summary>
        private void OnGrabReleased()
        {
            InvokeGripReleased(this, GetArguments());
        }

        /// <summary>
        /// При нажатии на телепорт сообщаем InputAdapter'у. Однако в случае со стилусом нет возможности регулировать данный метод, потому всегда ложь. 
        /// </summary>
        /// <returns>Истина, если кнопка нажата.</returns>
        public override bool IsTouchpadPressed()
        {
            return false;
        }

        /// <summary>
        /// При отпускании телепорта сообщаем InputAdapter'у. Однако в случае со стилусом нет возможности регулировать данный метод, потому всегда ложь. 
        /// </summary>
        /// <returns>Истина, если кнопка нажата.</returns>
        public override bool IsTouchpadReleased()
        {
            return false;
        }

        /// <summary>
        /// При нажатии на кнопку использования сообщаем InputAdapter'у. 
        /// </summary>
        /// <returns>Истина, если кнопка нажата.</returns>
        public override bool IsTriggerPressed()
        {
            return Stylus.InputHandler.IsUsePressed;
        }

        /// <summary>
        /// При отпускании кнопки использования сообщаем InputAdapter'у. 
        /// </summary>
        /// <returns>Истина, если кнопка отпущена.</returns>
        public override bool IsTriggerReleased()
        {
            return Stylus.InputHandler.IsUseReleased;
        }

        /// <summary>
        /// Нажата ли кнопка.
        /// </summary>
        /// <param name="gripPress">Целевая кнопка.</param>
        /// <returns>Истиан если нажата.</returns>
        public override bool IsButtonPressed(ControllerInput.ButtonAlias gripPress)
        {
            return gripPress switch
            {
                ControllerInput.ButtonAlias.GripPress => Stylus.InputHandler.IsGrabPressed,
                ControllerInput.ButtonAlias.TriggerPress => Stylus.InputHandler.IsUsePressed,
                ControllerInput.ButtonAlias.Undefined => false,
                ControllerInput.ButtonAlias.TriggerHairline => false,
                ControllerInput.ButtonAlias.TriggerTouch => false,
                ControllerInput.ButtonAlias.TriggerClick => false,
                ControllerInput.ButtonAlias.GripHairline => false,
                ControllerInput.ButtonAlias.GripTouch => false,
                ControllerInput.ButtonAlias.GripClick => false,
                ControllerInput.ButtonAlias.TouchpadTouch => false,
                ControllerInput.ButtonAlias.TouchpadPress => false,
                ControllerInput.ButtonAlias.TouchpadTwoTouch => false,
                ControllerInput.ButtonAlias.TouchpadTwoPress => false,
                ControllerInput.ButtonAlias.ButtonOneTouch => false,
                ControllerInput.ButtonAlias.ButtonOnePress => false,
                ControllerInput.ButtonAlias.ButtonTwoTouch => false,
                ControllerInput.ButtonAlias.ButtonTwoPress => false,
                ControllerInput.ButtonAlias.StartMenuPress => false,
                ControllerInput.ButtonAlias.TouchpadSense => false,
                ControllerInput.ButtonAlias.TriggerSense => false,
                ControllerInput.ButtonAlias.MiddleFingerSense => false,
                ControllerInput.ButtonAlias.RingFingerSense => false,
                ControllerInput.ButtonAlias.PinkyFingerSense => false,
                ControllerInput.ButtonAlias.GripSense => false,
                ControllerInput.ButtonAlias.GripSensePress => false,
                _ => false
            };
        }

        /// <summary>
        /// Принудительный вызов выпускания объекта из руки.
        /// </summary>
        /// <param name="controllerInteractionEventArgs">Аргументы события.</param>
        public override void OnGripReleased(ControllerInput.ControllerInteractionEventArgs controllerInteractionEventArgs)
        {
            
        }
        
        /// <summary>
        /// Получить объект-контроллер.
        /// </summary>
        /// <returns>Объект-контроллер.</returns>
        public override GameObject GetController()
        {
            return Stylus.gameObject;
        }

        /// <summary>
        /// Отписка.
        /// </summary>
        private void OnDestroy()
        {
            Stylus.InputHandler.UsePressed -= OnUsePressed;
            Stylus.InputHandler.UseReleased -= OnUseReleased;
            Stylus.InputHandler.GrabPressed -= OnGrabPressed;
            Stylus.InputHandler.GrabReleased -= OnGrabReleased;
            Stylus.InputHandler.Initialized -= OnInitialized;
            Stylus.InputHandler.Deinitialized -= OnDeinitialized;
        }
    }
}