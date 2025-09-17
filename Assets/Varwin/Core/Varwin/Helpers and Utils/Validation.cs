using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Varwin
{
    /// <summary>
    /// Класс для валидации.
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// Проверка на соответствие типа исходного значения и конвертация в target type, если это возможно.
        /// </summary>
        /// <param name="value">Исходное значение.</param>
        /// <param name="targetType">Требуемый тип.</param>
        /// <param name="convertedType">Сконвертированное в требуемый тип исходное значение.</param>
        /// <returns>Удалось ли сконвертировать value в targetType.</returns>
        [UsedImplicitly]
        public static bool ValidateAndConvert(dynamic value, Type targetType, out dynamic convertedType)
        {
            convertedType = null;
            if (targetType is null)
            {
                return false;
            }

            if (!targetType.IsValueType && value is null)
            {
                return true;
            }

            return TypeValidationUtils.TryConvertToType(value, targetType, out convertedType);
        }
    }
}