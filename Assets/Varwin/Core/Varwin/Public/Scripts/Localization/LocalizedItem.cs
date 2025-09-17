using System;
using UnityEngine;

namespace Varwin.Public
{
    /// <summary>
    /// Локализация.
    /// </summary>
    /// <typeparam name="T">Тип локализуемого объекта.</typeparam>
    [Serializable]
    public class LocalizedItem<T>
    {
        /// <summary>
        /// Локаль.
        /// </summary>
        public Language Locale;
        
        /// <summary>
        /// Значение для локали.
        /// </summary>
        public T Value;

        /// <summary>
        /// Инициализация.
        /// </summary>
        /// <param name="locale">Локаль.</param>
        /// <param name="value">Перевод.</param>
        public LocalizedItem(Language locale, T value)
        {
            Locale = locale;
            Value = value;
        }
    }
}