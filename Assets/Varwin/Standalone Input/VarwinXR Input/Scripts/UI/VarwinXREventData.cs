using UnityEngine;
using UnityEngine.EventSystems;

namespace Varwin.XR
{
    /// <summary>
    /// Доработанный аргумент взаимодействия.
    /// </summary>
    public class VarwinXREventData : PointerEventData
    {
        /// <summary>
        /// Луч от контроллера.
        /// </summary>
        public Ray ControllerRay;
        
        /// <summary>
        /// Камера игрока.
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// Вызвавший событие контроллер.
        /// </summary>
        public GameObject Controller;
        
        /// <summary>
        /// Инициализация.
        /// </summary>
        /// <param name="eventSystem">Система событий.</param>
        public VarwinXREventData(EventSystem eventSystem) : base(eventSystem)
        {
        }
    }
}