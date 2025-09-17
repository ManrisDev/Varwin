using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.Raycasters
{
    /// <summary>
    /// Интерфейс, описывающий свойства рейкастера, взаимодействующего с PointableObject'ами.
    /// </summary>
    public interface IPointableRaycaster
    {
        /// <summary>
        /// Ближний объект UI.
        /// </summary>
        public PointableObject NearPointableObject { get; }

        /// <summary>
        /// Информация о столкновении с ближайшим объектом.
        /// </summary>
        public RaycastHit? NearObjectRaycastHit { get; }
    }
}