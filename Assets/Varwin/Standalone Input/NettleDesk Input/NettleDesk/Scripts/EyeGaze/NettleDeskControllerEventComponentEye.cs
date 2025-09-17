using System;
using UnityEngine;
using Varwin.DesktopPlayer;
using Varwin.PlatformAdapter;

namespace Varwin.NettleDesk
{
    /// <summary>
    /// Компонент, отвечающий за взаимодействие взглядом.
    /// </summary>
    public class NettleDeskControllerEventComponentEye : NettleDeskControllerEventComponentBase
    {
        /// <summary>
        /// Нажат ли телепорт.
        /// </summary>
        private bool _isTeleportPressed;

        /// <summary>
        /// Отпущен ли телепорт.
        /// </summary>
        private bool _isTeleportReleased;
        
        /// <summary>
        /// Нажато ли использование.
        /// </summary>
        private bool _isLeftMouseButtonPressed;
        
        /// <summary>
        /// Отпущен ли использование.
        /// </summary>
        private bool _isLeftMouseButtonReleased;
        
        /// <summary>
        /// Нажат ли граб.
        /// </summary>
        private bool _isRightMouseButtonPressed;

        /// <summary>
        /// Подписка.
        /// </summary>
        private void Awake()
        {
            SetupInputEvents();
        }

        /// <summary>
        /// Сообщение о том, что контроллер активен.
        /// </summary>
        private void Start()
        {
            InvokeControllerEnabled(this, new ControllerInput.ControllerInteractionEventArgs());
        }

        /// <summary>
        /// Сброс телепорта, потому что автоматическое обновление этих полей происходит в событии.
        /// </summary>
        private void LateUpdate()
        {
            _isTeleportReleased = false;
            _isTeleportPressed = false;
        }

        /// <summary>
        /// Нажат ли телепорт.
        /// </summary>
        /// <returns>Истина, если нажат.</returns>
        public override bool IsTouchpadPressed() => _isTeleportPressed;

        /// <summary>
        /// Отпущен ли телепорт.
        /// </summary>
        /// <returns>Истина, если отпущен.</returns>
        public override bool IsTouchpadReleased() => _isTeleportReleased;

        /// <summary>
        /// Нажато ли использование.
        /// </summary>
        /// <returns>Истина, если нажат.</returns>
        public override bool IsTriggerPressed() => _isLeftMouseButtonPressed;

        /// <summary>
        /// Отпущено ли использование.
        /// </summary>
        /// <returns>Истина, если отпущено.</returns>
        public override bool IsTriggerReleased() => _isLeftMouseButtonReleased;

        /// <summary>
        /// Нажата ли кнопка.
        /// </summary>
        /// <param name="gripPress">Кнопка.</param>
        /// <returns>Истина, если нажат.</returns>
        public override bool IsButtonPressed(ControllerInput.ButtonAlias gripPress)
        {
            switch (gripPress)
            {
                case ControllerInput.ButtonAlias.GripPress: return _isRightMouseButtonPressed;
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Метод, вызываемый для принудительного выпускания из руки.
        /// </summary>
        /// <param name="controllerInteractionEventArgs">Аргументы события.</param>
        public override void OnGripReleased(ControllerInput.ControllerInteractionEventArgs controllerInteractionEventArgs)
        {
            InvokeGripReleased(this, controllerInteractionEventArgs);
        }

        /// <summary>
        /// Получить контроллер.
        /// </summary>
        /// <returns>Объект, содержащий контроллер.</returns>
        public override GameObject GetController() => gameObject;

        /// <summary>
        /// Подписка на события.
        /// </summary>
        private void SetupInputEvents()
        {
            DesktopPlayerInput desktopPlayerInput = GetComponentInParent<DesktopPlayerInput>();
            
            ControllerInput.ControllerInteractionEventArgs controllerInteractionEventArgs =
                new ControllerInput.ControllerInteractionEventArgs
                {
                    controllerReference = new PlayerController.ControllerReferenceArgs
                    {
                        hand = ControllerInteraction.ControllerHand.Right
                    }
                };

            desktopPlayerInput.UsePressed += () =>
            {
                InvokeTriggerPressed(this, controllerInteractionEventArgs);
                _isLeftMouseButtonPressed = true;
                _isLeftMouseButtonReleased = false;
            };

            desktopPlayerInput.UseReleased += () =>
            {
                InvokeTriggerReleased(this, controllerInteractionEventArgs);
                _isLeftMouseButtonPressed = false;
                _isLeftMouseButtonReleased = true;
            };

            desktopPlayerInput.GrabPressed += () =>
            {
                InvokeGripPressed(this, controllerInteractionEventArgs);
                _isRightMouseButtonPressed = true;
            };

            desktopPlayerInput.GrabReleased += () => { _isRightMouseButtonPressed = false; };

            desktopPlayerInput.TeleportPressed += () =>
            {
                InvokeTeleportPressed(this, controllerInteractionEventArgs);
                _isTeleportPressed = true;
                _isTeleportReleased = false;
            };

            desktopPlayerInput.TeleportReleased += () =>
            {
                InvokeTeleportReleased(this, controllerInteractionEventArgs);
                _isTeleportPressed = false;
                _isTeleportReleased = true;
            };
        }
    }
}