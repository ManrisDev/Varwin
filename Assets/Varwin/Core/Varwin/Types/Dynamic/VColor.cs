using UnityEngine;

namespace Varwin
{
    /// <summary>
    /// Класс для работы с цветом из блокли.
    /// </summary>
    public static class VColor
    {
        public static Color SetRedComponent(Color color, float value) => new(Mathf.Clamp01(value), color.g, color.b, color.a);
        public static Color SetGreenComponent(Color color, float value) => new(color.r, Mathf.Clamp01(value), color.b, color.a);
        public static Color SetBlueComponent(Color color, float value) => new(color.r, color.g, Mathf.Clamp01(value), color.a);
        public static Color SetAlphaComponent(Color color, float value) => new(color.r, color.g, color.b, Mathf.Clamp01(value));

        public static float GetRedComponent(Color color) => color.r;
        public static float GetGreenComponent(Color color) => color.g;
        public static float GetBlueComponent(Color color) => color.b;
        public static float GetAlphaComponent(Color color) => color.a;

        /// <summary>
        /// Получить яркость цвета (grayscale).
        /// </summary>
        /// <returns>Яркость цвета (grayscale).</returns>
        public static float GetGrayscale(Color color) => color.grayscale;

        /// <summary>
        /// Инвертирование цвета.
        /// </summary>
        /// <param name="color">Цвет.</param>
        /// <returns>Инвертированный цвет.</returns>
        public static Color Invert(Color color) => new(1f - color.r, 1f - color.g, 1f - color.b, color.a);

        #region operations

        /// <summary>
        /// Сложение двух цветов.
        /// </summary>
        /// <param name="color1">Первый цвет.</param>
        /// <param name="color2">Второй цвет.</param>
        /// <returns>Сумма цветов.</returns>
        public static Color Sum(Color color1, Color color2) => color1 + color2;

        /// <summary>
        /// Вычитание двух цветов.
        /// </summary>
        /// <param name="color1">Первый цвет.</param>
        /// <param name="color2">Второй цвет.</param>
        /// <returns>Разница цветов.</returns>
        public static Color Subtract(Color color1, Color color2) => color1 - color2;

        /// <summary>
        /// Умножение цвета на скаляр.
        /// </summary>
        /// <param name="color">Цвет.</param>
        /// <param name="factor">Число, на которое производится умножение.</param>
        /// <returns>Результат умножения цвета на число.</returns>
        public static Color Multiply(Color color, float factor) => color * factor;

        /// <summary>
        /// Деление цвета на скаляр.
        /// </summary>
        /// <param name="color">Цвет.</param>
        /// <param name="factor">Число, на которое производится деление.</param>
        /// <returns>Результат деления цвета на число.</returns>
        public static Color Divide(Color color, float factor) => color / factor;

        /// <summary>
        /// Умножить цвета друг на друга.
        /// </summary>
        /// <param name="color1">Первый цвет.</param>
        /// <param name="color2">Второй цвет.</param>
        /// <returns>Результат умножения двух цветов друг на друга.</returns>
        public static Color Multiply(Color color1, Color color2) => color1 * color2;

        /// <summary>
        /// Разделить два цвета друг на друга.
        /// </summary>
        /// <param name="color1">Первый цвет.</param>
        /// <param name="color2">Второй цвет.</param>
        /// <returns>Результат деления двух цветов друг на друга.</returns>
        public static Color Divide(Color color1, Color color2)
        {
            return new Color
            {
                r = Mathf.Clamp01(color1.r / Mathf.Max(0.0001f,color2.r)),
                g = Mathf.Clamp01(color1.g / Mathf.Max(0.0001f, color2.g)),
                b = Mathf.Clamp01(color1.b / Mathf.Max(0.0001f,color2.b)),
                a = Mathf.Clamp01(color1.a / Mathf.Max(0.0001f,color2.a))
            };
        }

        /// <summary>
        /// Интерполяция (получение промежуточного значения) цвета.
        /// </summary>
        /// <param name="color1">Первый цвет.</param>
        /// <param name="color2">Второй цвет.</param>
        /// <param name="t">Коэффициент интерполяции.</param>
        /// <returns>Промежуточное значение между цветами по коэффициенту 't'.</returns>
        public static Color Lerp(Color color1, Color color2, float t) => Color.Lerp(color1, color2, Mathf.Clamp01(t));

        #endregion
    }
}