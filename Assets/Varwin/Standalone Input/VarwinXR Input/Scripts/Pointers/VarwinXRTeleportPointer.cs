using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    /// <summary>
    /// Указка телепорта с измененным расположением точки выхода луча.
    /// </summary>
    public class VarwinXRTeleportPointer : TeleportPointer
    {
        /// <summary>
        /// Целевой объект контроллера.
        /// </summary>
        public VarwinXRController Controller;
        
        /// <summary>
        /// Инициализация.
        /// </summary>
        public override void Init()
        {
            base.Init();
            
            if (Controller.ControllerModel)
            {
                _pointerOrigin = Controller.ControllerModel.PointerAnchor;
            }

            Controller.InputHandler.Initialized += OnControllerInitialized;
            VarwinXRSettings.UseVarwinPointerDirectionChanged += OnUseVarwinPointerDirectionChanged;
        }

        /// <summary>
        /// При смене флага использования нативной указки.
        /// </summary>
        /// <param name="useNativeDirection">Используется ли нативная указка.</param>
        private void OnUseVarwinPointerDirectionChanged(bool useNativeDirection)
        {
            if (!Controller || !Controller.ControllerModel)
            {
                _pointerOrigin = transform;
                return;
            }
            
            _pointerOrigin = Controller.ControllerModel.PointerAnchor;
        }

        /// <summary>
        /// При инициализации контроллера изменяется точка выхода луча.
        /// </summary>
        /// <param name="sender">Вызвавший событие компонент.</param>
        private void OnControllerInitialized(VarwinXRControllerInputHandler sender)
        {
            _pointerOrigin = Controller.ControllerModel.PointerAnchor;
        }

        /// <summary>
        /// Отписка.
        /// </summary>
        private void OnDestroy()
        {
            Controller.InputHandler.Initialized -= OnControllerInitialized;
            VarwinXRSettings.UseVarwinPointerDirectionChanged -= OnUseVarwinPointerDirectionChanged;
        }
    }
}