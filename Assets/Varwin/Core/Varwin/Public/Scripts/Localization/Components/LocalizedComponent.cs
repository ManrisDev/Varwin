using System;
using UnityEngine;

namespace Varwin.Public
{
    /// <summary>
    /// Компонент локализации.
    /// </summary>
    /// <typeparam name="T">Целевой тип.</typeparam>
    public abstract class LocalizedComponent<T> : MonoBehaviour
    {
        /// <summary>
        /// Словарь.
        /// </summary>
        public LocalizedDictionary<T> Dictionary = new();
        
        /// <summary>
        /// Смена локализации в зависимости от языка.
        /// </summary>
        private void Awake()
        {
            var language = Settings.Instance?.Language == null ? "en" : Settings.Instance.Language;
            var value = Dictionary.GetValue(language);
            OnValueReceived(value);
        }

        /// <summary>
        /// При получении данных.
        /// </summary>
        /// <param name="value">Значение.</param>
        public abstract void OnValueReceived(T value);
    }
}