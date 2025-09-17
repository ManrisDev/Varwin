using UnityEngine;

namespace Varwin.XR
{
    /// <summary>
    /// Дополнительный класс для расширения функционала кватерниона.
    /// </summary>
    public static class QuaternionEx
    {
        /// <summary>
        /// Получение угла до заданного (в эйлерах).
        /// </summary>
        /// <param name="sourceQuaternion">Исходный кватернион.</param>
        /// <param name="targetQuaternion">Конечный кватернион.</param>
        /// <returns>Дельта в эйлеровых углах</returns>
        public static Vector3 GetAngle(this Quaternion sourceQuaternion, Quaternion targetQuaternion)
        {
            var delta = (targetQuaternion * Quaternion.Inverse(sourceQuaternion)).eulerAngles;
            return new Vector3(delta.x > 180f ? delta.x - 360f : delta.x, delta.y > 180f ? delta.y - 360f : delta.y, delta.z > 180f ? delta.z - 360f : delta.z);
        }
    }
}