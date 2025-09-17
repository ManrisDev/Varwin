using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Varwin.Public
{
    /// <summary>
    /// Локализируемый словарь.
    /// </summary>
    /// <typeparam name="T">Тип значения.</typeparam>
    [Serializable]
    public class LocalizedDictionary<T> : LocalizedDictionaryBase
    {
        /// <summary>
        /// Cловарь.
        /// </summary>
        [SerializeField] private List<LocalizedItem<T>> _dictionary = new() {new LocalizedItem<T>(Language.English, default)};

        /// <summary>
        /// Получение значения из словаря.
        /// </summary>
        /// <param name="locale">Локаль.</param>
        /// <returns>Значение в заданной локали.</returns>
        public T GetValue(Language locale)
        {
            if (_dictionary == null)
            {
                return default;
            }

            var item = _dictionary.Find(a => a.Locale == locale);

            if (item == null)
            {
                item = _dictionary.Find(a => a.Locale == Language.English);
            }
            
            return item == null ? default : item.Value;
        }

        /// <summary>
        /// Получение значения из словаря.
        /// </summary>
        /// <param name="locale">Локаль.</param>
        /// <returns>Значение в заданной локали.</returns>
        [Obsolete]
        public T GetValue(SystemLanguage locale)
        {
            return GetValue((Language) locale);
        }
        
        /// <summary>
        /// Получение значения из словаря.
        /// </summary>
        /// <param name="localeName">Имя локали.</param>
        /// <returns>Значение в заданной локали.</returns>
        public T GetValue(string localeName)
        {
            return GetValue(GetSystemLanguage(localeName));
        }

        /// <summary>
        /// Получение SystemLanguage по имени локали.
        /// </summary>
        /// <param name="localeName">Имя локали.</param>
        /// <returns>SystemLanguage.</returns>
        private Language GetSystemLanguage(string localeName)
        {
            return localeName switch
            {
                "en" => Language.English,
                "ru" => Language.Russian,
                "cn" => Language.Chinese,
                "kk" => Language.Kazakh,
                "ko" => Language.Korean,
                _ => Language.Unknown
            };
        }

        /// <summary>
        /// Получить список доступных язков в словаре.
        /// </summary>
        /// <returns>Список доступных языков в словаре.</returns>
        public IEnumerable<Language> GetLanguages()
        {
            return _dictionary.Select(a => a.Locale);
        }
        
        /// <summary>
        /// Добавление локализации в словарь.
        /// </summary>
        /// <param name="locale">Локаль.</param>
        /// <param name="value">Значение.</param>
        public void SetLocale(Language locale, T value)
        {
            _dictionary ??= new List<LocalizedItem<T>>();
            var item = _dictionary.Find(a => a.Locale == locale);
            if (item == null)
            {
                _dictionary.Add(new LocalizedItem<T>(locale, value));
            }
            else
            {
                item.Value = value;
            }
        }
        
        /// <summary>
        /// Добавление локализации в словарь.
        /// </summary>
        /// <param name="locale">Локаль.</param>
        /// <param name="value">Значение.</param>
        [Obsolete]
        public void SetLocale(SystemLanguage locale, T value)
        {
            _dictionary ??= new List<LocalizedItem<T>>();
            var item = _dictionary.Find(a => a.Locale == (Language) locale);
            if (item == null)
            {
                _dictionary.Add(new LocalizedItem<T>((Language) locale, value));
            }
            else
            {
                item.Value = value;
            }
        }

        /// <summary>
        /// Удалить локализацию.
        /// </summary>
        /// <param name="locale">Локализация.</param>
        public void RemoveLocalization(Language locale)
        {
            var item = _dictionary?.Find(a => a.Locale == locale);
            if (item == null)
            {
                return;
            }

            _dictionary.Remove(item);
        }
        
        /// <summary>
        /// Удалить локализацию.
        /// </summary>
        /// <param name="locale">Локализация.</param>
        [Obsolete]
        public void RemoveLocalization(SystemLanguage locale)
        {
            var item = _dictionary?.Find(a => a.Locale == (Language) locale);
            if (item == null)
            {
                return;
            }

            _dictionary.Remove(item);
        }
    }
}