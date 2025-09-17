using UnityEngine;

namespace Varwin.PlatformAdapter
{
    /// <summary>
    /// Базовый объект для взаимодействия с UI поинтером.
    /// Хранит набор методов для обработки событий.
    /// </summary>
    public abstract class PointableObject : MonoBehaviour
    {
        /// <summary>
        /// Вхождение UI поинтера на границы объекта.
        /// </summary>
        public abstract void OnPointerIn();
        
        /// <summary>
        /// Выхождение UI поинтера за границы объекта.
        /// </summary>
        public abstract void OnPointerOut();

        /// <summary>
        /// Нажатие UI поинтера на объект.
        /// </summary>
        public abstract void OnPointerDown();

        /// <summary>
        /// Отпускание UI поинтера с объекта.
        /// </summary>
        public abstract void OnPointerUp();

        /// <summary>
        /// Отпускание UI поинтера с объекта.
        /// </summary>
        public abstract void OnPointerUpAsButton();
        
        protected virtual void Awake() { }
    }
}
