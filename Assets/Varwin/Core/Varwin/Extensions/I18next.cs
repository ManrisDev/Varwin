using System.Collections.Generic;

namespace Varwin.Core
{
    /// <summary>
    /// Класс расширений i18next.
    /// Типичная локаль в i18next выглядит как "Hello.".
    /// Если имеется placeholder, то "Hello, {{user_name}}.".
    /// </summary>
    public static class I18next
    {
        /// <summary>
        /// Возвращает строку с заполненными placeholder'ами заданными значениями.
        /// </summary>
        /// <param name="sourceString">Исходная строка.</param>
        /// <param name="values">Значения.</param>
        /// <returns>Преобразованная строка.</returns>
        public static string Format(string sourceString, params KeyValuePair<string, object>[] values)
        {
            if (string.IsNullOrEmpty(sourceString))
            {
                return sourceString;
            }
            
            foreach (var attribute in values)
            {
                sourceString = sourceString.Replace($"{{{{{attribute.Key}}}}}", $"{attribute.Value}");
            }

            return sourceString;
        }
    }
}