using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    /// <summary>
    /// Управление состояниями указок.
    /// </summary>
    public class VarwinXRPointerControllerComponent : PointerControllerComponent
    {
        /// <summary>
        /// Контроллер.
        /// </summary>
        public VarwinXRController Controller;
        
        /// <summary>
        /// Целевая рука.
        /// </summary>
        public ControllerInteraction.ControllerHand Hand { get; protected set; }

        /// <summary>
        /// Указка удаленного захвата объекта.
        /// </summary>
        public VarwinXRDistancePointer DistancePointer;
        
        /// <summary>
        /// Указка для взаимодействия с элементами UI.
        /// </summary>
        public VarwinXRUIPointer UIPointer;
        
        /// <summary>
        /// Является ли сценой меню.
        /// </summary>
        private bool _isMenuScene = true;

        /// <summary>
        /// Открыто ли меню.
        /// </summary>
        public bool IsMenuOpened = false;

        /// <summary>
        /// Отображать ли указку до UI.
        /// </summary>
        public bool ShowUIPointer
        {
            get => UIPointer.ShowPointer;
            set => UIPointer.ShowPointer = value;
        }

        /// <summary>
        /// Список доступных указок.
        /// </summary>
        protected override List<IBasePointer> _pointers { get; set; }

        /// <summary>
        /// Подписка и инициализация указок.
        /// </summary>
        private void Awake()
        {
            var teleportPointer = gameObject.AddComponent<VarwinXRTeleportPointer>();
            teleportPointer.Controller = Controller;
            _pointers = new List<IBasePointer> {teleportPointer, UIPointer, DistancePointer};

            foreach (var pointer in _pointers)
            {
                pointer.Init();
            }

            Hand = Controller.IsLeft ? ControllerInteraction.ControllerHand.Left : ControllerInteraction.ControllerHand.Right;
        }
        
        /// <summary>
        /// Обновление указкок.
        /// </summary>
        private void LateUpdate()
        {
            UpdatePointers();
        }
        
        /// <summary>
        /// Обновление указок.
        /// </summary>
        protected override void UpdatePointers()
        {
            if (!Controller.Initialized)
            {
                TryChangePointer(null);
                return;
            }
            
            bool hidePointers = false;
#if VARWINCLIENT
            hidePointers = true;
#endif
            _isMenuScene = SceneManager.GetActiveScene().buildIndex != -1;

            base.UpdatePointers();
            
            if ((IsMenuOpened || _isMenuScene) && hidePointers)
            {
                _pointers.ForEach(a => a.Toggle(false));
                CurrentPointer = null;
            }
        }
    }
}