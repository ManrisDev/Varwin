using UnityEngine;

namespace Varwin.XR
{
    /// <summary>
    /// Вспомогательный класс сравнения.
    /// </summary>
    public static class Compare
    {
        /// <summary>
        /// Разница, при которой будет считаться, что значения вещественных чисел равны.
        /// </summary>
        public const float EqualThreshold = 0.001f;

        /// <summary>
        /// Являются ли вектора равными.
        /// </summary>
        /// <param name="left">Первый вектор.</param>
        /// <param name="right">Второй вектор.</param>
        /// <returns>Истина, если равны.</returns>
        public static bool IsEqual(Vector2 left, Vector2 right)
        {
            return Mathf.Abs(left.x - right.x) < EqualThreshold && Mathf.Abs(left.y - right.y) < EqualThreshold;
        }
        
        /// <summary>
        /// Являются ли числа равными.
        /// </summary>
        /// <param name="left">Первое число.</param>
        /// <param name="right">Второе число.</param>
        /// <returns>Истина, если равны.</returns>
        public static bool IsEqual(float left, float right)
        {
            return Mathf.Abs(left - right) < EqualThreshold;
        }
    }
}