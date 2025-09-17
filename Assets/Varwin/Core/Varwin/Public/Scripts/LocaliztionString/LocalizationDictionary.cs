using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Core;

namespace Varwin.Public
{
    [Serializable]
    public class LocalizationDictionary : IList, IList<LocalizationString>
    {
        public List<LocalizationString> LocalizationStrings;

        public bool IsValidDictionary = true;
        
        public LocalizationDictionary()
        {
            LocalizationStrings = new List<LocalizationString>();
        }
        
        public LocalizationDictionary(IEnumerable<LocalizationString> list) : this()
        {
            LocalizationStrings.AddRange(list);
        }

        public bool Validate()
        {
            var languages = new HashSet<Language>();

            IsValidDictionary = true;

            foreach (var localizationString in this)
            {
                if (!languages.Contains(localizationString.key))
                {
                    languages.Add(localizationString.key);
                }
                else
                {
                    IsValidDictionary = false;
                    break;
                }
            }

            return IsValidDictionary;
        }
        
        public IEnumerator<LocalizationString> GetEnumerator()
        {
            return LocalizationStrings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Add(object value)
        {
            if (value.GetType() == typeof(LocalizationString))
            {
                LocalizationStrings.Add((LocalizationString) value);
                return LocalizationStrings.Count - 1;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public void Add(LocalizationString item)
        {
            LocalizationStrings.Add(item);
        }

        public void Add(Language key, string value)
        {
            LocalizationStrings.Add(new LocalizationString(key, value));
        }
        
        [Obsolete]
        public void Add(SystemLanguage key, string value)
        {
            LocalizationStrings.Add(new LocalizationString((Language) key, value));
        }

        public LocalizationString Get(Language key)
        {
            return LocalizationStrings.FirstOrDefault(x => x.key == key);
        }
        
        [Obsolete]
        public LocalizationString Get(SystemLanguage key)
        {
            return LocalizationStrings.FirstOrDefault(x => x.key == (Language) key);
        }
        
        public void Clear()
        {
            LocalizationStrings.Clear();
        }

        public void Remove(object value)
        {
            if (value.GetType() == typeof(LocalizationString))
            {
                Remove((LocalizationString) value);
            }
            else if (value.GetType() == typeof(Language))
            {
                Remove(Get((Language) value));
            }
        }

        public bool Remove(LocalizationString item)
        {
            return LocalizationStrings.Remove(item);
        }

        public bool Contains(Language key)
        {
            return Get(key) != null;
        }
        
        public bool Contains(SystemLanguage key)
        {
            return Get((Language) key) != null;
        }

        public bool Contains(object value)
        {
            if (value.GetType() == typeof(LocalizationString))
            {
                return Contains((LocalizationString) value);
            }
            
            if (value.GetType() == typeof(Language))
            {
                return Contains((Language) value);
            }
            
            if (value.GetType() == typeof(SystemLanguage))
            {
                return Contains((SystemLanguage) value);
            }
            
            throw new ArgumentException();
        }
        
        public bool Contains(LocalizationString item)
        {
            return LocalizationStrings.Contains(item);
        }

        public void CopyTo(Array array, int arrayIndex)
        {
            LocalizationStrings.CopyTo((LocalizationString[]) array, arrayIndex);
        }

        public void CopyTo(LocalizationString[] array, int arrayIndex)
        {
            LocalizationStrings.CopyTo(array, arrayIndex);
        }

        public int IndexOf(object value)
        {
            if (value.GetType() == typeof(LocalizationString))
            {
                return IndexOf((LocalizationString) value);
            }
            
            if (value.GetType() == typeof(Language))
            {
                return IndexOf((Language) value);
            }
            
            throw new ArgumentException();
        }
        
        public int IndexOf(LocalizationString item)
        {
            return LocalizationStrings.IndexOf(item);
        }

        public void Insert(int index, object value)
        {
            if (value.GetType() == typeof(LocalizationString))
            {
                LocalizationStrings.Insert(index, (LocalizationString) value);
            }
        }

        public void Insert(int index, LocalizationString item)
        {
            LocalizationStrings.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            LocalizationStrings.RemoveAt(index);
        }

        public I18n ToI18N()
        {
            return LocalizationUtils.GetI18nFromLocalizationStrings(LocalizationStrings);
        }

        public string GetCurrentLocale()
        {
            return Settings.Instance.Language switch
            {
                "en" => this[Language.English],
                "ru" => this[Language.Russian],
                "cn" => this[Language.Chinese],
                "kk" => this[Language.Kazakh],
                "ko" => this[Language.Korean],
                _ => null
            };
        }
        
        public int Count => LocalizationStrings.Count;
        
        public bool IsSynchronized => false;

        public object SyncRoot => new object();
        
        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public LocalizationString this[int index]
        {
            get => LocalizationStrings[index];
            set => LocalizationStrings[index] = value;
        }

        public string this[Language language]
        {
            get => LocalizationStrings.FirstOrDefault(x => x.key == language)?.value;
            set
            {
                LocalizationString localizationString = LocalizationStrings.FirstOrDefault(x => x.key == language);
                if (localizationString != null)
                {
                    localizationString.value = value;
                }
            }
        }
        
        [Obsolete]
        public string this[SystemLanguage language]
        {
            get => LocalizationStrings.FirstOrDefault(x => x.key == (Language) language)?.value;
            set
            {
                LocalizationString localizationString = LocalizationStrings.FirstOrDefault(x => x.key == (Language) language);
                if (localizationString != null)
                {
                    localizationString.value = value;
                }
            }
        }

        object IList.this[int index]
        {
            get => LocalizationStrings[index];
            set => LocalizationStrings[index] = (LocalizationString) value;
        }
    }
}