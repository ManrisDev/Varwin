using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Varwin.NettleDeskPlayer;
using Varwin.PlatformAdapter;

namespace Varwin.NettleDesk
{
    /// <summary>
    /// Управление состояниями указок.
    /// </summary>
    public class NettleDeskPointerControllerComponent : PointerControllerComponent
    {
        /// <summary>
        /// Контроллер.
        /// </summary>
        public NettleDeskStylus Controller;

        /// <summary>
        /// Целевая рука.
        /// </summary>
        public ControllerInteraction.ControllerHand Hand => Controller.IsLeft ? ControllerInteraction.ControllerHand.Left : ControllerInteraction.ControllerHand.Right;

        /// <summary>
        /// Указка удаленного захвата объекта.
        /// </summary>
        public NettleDeskStylusDistancePointer DistancePointer;
        
        /// <summary>
        /// Указка для взаимодействия с элементами UI.
        /// </summary>
        public NettleDeskStylusUIPointer StylusUIPointer;

        /// <summary>
        /// Указка для телепортации.
        /// </summary>
        public TeleportPointer TeleportPointer;
        
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
            get => StylusUIPointer.ShowPointer;
            set => StylusUIPointer.ShowPointer = value;
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
            _pointers = new List<IBasePointer> {TeleportPointer, StylusUIPointer, DistancePointer};

            foreach (var pointer in _pointers)
            {
                pointer.Init();
            }
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