using UnityEngine;

namespace Varwin.XR
{
    /// <summary>
    /// Класс описания типа перемещения.
    /// </summary>
    public abstract class VarwinXRPlayerMoveBase : MonoBehaviour
    {
        /// <summary>
        /// Активен ли способ перемещения.
        /// </summary>
        public bool IsEnabled
        {
            get => enabled;
            set
            {
                if (value == enabled)
                {
                    return;
                }

                enabled = value;
                if (value)
                {
                    OnEnable();                    
                }
                else
                {
                    OnDisable();
                }
            }
        }
        
        /// <summary>
        /// Ключ локализации названия.
        /// </summary>
        public abstract string LocalizationNameKey { get; }

        /// <summary>
        /// Левый контроллер.
        /// </summary>
        protected VarwinXRControllerInputHandler _leftController;

        /// <summary>
        /// Правый контроллер.
        /// </summary>
        protected VarwinXRControllerInputHandler _rightController;
        
        /// <summary>
        /// Класс перемещения игрока.
        /// </summary>
        protected VarwinXRPlayerMoveController _player;

        /// <summary>
        /// Инициализация контроллера перемещений.
        /// </summary>
        /// <param name="player">Игрок.</param>
        /// <param name="leftController">Левый контроллер.</param>
        /// <param name="rightController">Правый контроллер.</param>
        public void OnInit(VarwinXRPlayerMoveController player, VarwinXRControllerInputHandler leftController, VarwinXRControllerInputHandler rightController)
        {
            _player = player;
            _leftController = leftController;
            _rightController = rightController;
        }

        /// <summary>
        /// При активации.
        /// </summary>
        protected virtual void OnEnable()
        {
            
        }
        
        /// <summary>
        /// При деактивации.
        /// </summary>
        protected virtual void OnDisable()
        {
            
        }
    }
}