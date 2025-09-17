using System.Linq;
using UnityEngine;

namespace Varwin.PlatformAdapter
{
    /// <summary>
    /// Класс-хэлпер для поинтеров.
    /// </summary>
    public static class PointerHelper
    {
        /// <summary>
        /// Нужно ли игнорировать коллайдеры при рейкасте поинтера.
        /// </summary>
        /// <param name="otherCollider">Коллайдер для проверки.</param>
        /// <param name="checkControllers">Контроллеры, которые необходимо проверять.</param>
        /// <returns>true - коллайдер нужно игнорировать.</returns>
        public static bool IsColliderGrabbed(
            Collider otherCollider,
            params ControllerInteraction.ControllerSelf[] checkControllers
        )
        {
            return checkControllers.Any(controller => controller.CheckIfColliderPresent(otherCollider));
        }
    }
}