#if !NET_STANDARD_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;
using UnityEngine;
using Varwin.Core;

namespace Varwin
{
    /// <summary>
    /// Класс для работы со списками из блокли.
    /// </summary>
    public static class VList
    {
        /// <summary>
        /// Получить значение из списка.
        /// </summary>
        /// <param name="listName">Имя списка.</param>
        /// <param name="valueName">Имя значения.</param>
        /// <returns>Значение элемента списка.</returns>
        public static ListValue Value(string listName, string valueName)
        {
            return new ListValue(listName, valueName);
        }

        /// <summary>
        /// Получить случайный элемент из списка.
        /// </summary>
        /// <param name="list">Список.</param>
        /// <param name="remove">Нужно ли удалить элемент.</param>
        /// <returns>Случайный элемент списка.</returns>
        public static dynamic GetRandomItem(dynamic list, bool remove)
        {
            ThrowExceptionIfNotList(list, "LIST_ERROR_GET_RANDOM_ITEM");
            ThrowExceptionIfIsEmpty(list, "LIST_ERROR_GET_RANDOM_ITEM");

            int randomIndex = Utils.RandomInt(0, list.Count - 1);
            dynamic result = list[randomIndex];

            if (remove)
            {
                list.RemoveAt(randomIndex);
            }

            return result;
        }

        /// <summary>
        /// Получить элемент из списка по индексу.
        /// </summary>
        /// <param name="list">Список.</param>
        /// <param name="index">Индекс элемента.</param>
        /// <param name="remove">Нужно ли удалить элемент.</param>
        /// <returns>Элемент списка.</returns>
        public static dynamic GetItem(dynamic list, dynamic index, bool remove)
        {
            ThrowExceptionIfNotList(list, "LIST_ERROR_GET_ITEM");
            ThrowExceptionIfIsEmpty(list, "LIST_ERROR_GET_ITEM");

            if (!int.TryParse(index.ToString(), out int i))
            {
                var keyValuePairs = new KeyValuePair<string, object>[]
                {
                    new ("current_index", index.GetType()),
                };

                throw new Exception(GetErrorText("LIST_ERROR_GET_ITEM", "ERROR_LIST_WRONG_INDEX_TYPE", keyValuePairs));
            }

            if (i >= list.Count)
            {
                var keyValuePairs = new KeyValuePair<string, object>[]
                {
                    new ("current_index", (index + 1).ToString()),
                    new ("array_length", list.Count)
                };

                throw new Exception(GetErrorText("LIST_ERROR_GET_ITEM", "ERROR_LIST_OUT_OF_RANGE", keyValuePairs));
            }

            dynamic result = list[i];

            if (remove)
            {
                list.RemoveAt(i);
            }

            return result;
        }

        /// <summary>
        /// Является ли список пустым.
        /// </summary>
        /// <param name="list">Список.</param>
        /// <returns>true - спсиок пустой. false - список не пустой.</returns>
        public static bool IsEmpty(dynamic list)
        {
            if (!IsList(list))
            {
                return true;
            }

            return list.Count <= 0;
        }

        /// <summary>
        /// Получить индекс элемента в списке.
        /// </summary>
        /// <param name="list">Список.</param>
        /// <param name="item">Элемент списка.</param>
        /// <returns>Индекс элемента списка. Если 0, то элемент не найден в списке.</returns>
        public static int GetItemIndexOf(dynamic list, dynamic item)
        {
            ThrowExceptionIfNotList(list, "LIST_ERROR_GET_INDEX_OF_ITEM");

            int result = list.IndexOf(item) + 1; //Humans count elements from 1

            return result;
        }

        /// <summary>
        /// Получить индекс последнего вхождения элемента в список.
        /// </summary>
        /// <param name="list">Список.</param>
        /// <param name="item">Элемент списка.</param>
        /// <returns>Индекс последнего вхождения элемента в список.</returns>
        public static int GetItemLastIndexOf(dynamic list, dynamic item)
        {
            ThrowExceptionIfNotList(list, "LIST_ERROR_GET_LAST_INDEX_OF");

            int result = list.LastIndexOf(item) + 1; //Humans count elements from 1

            return result;
        }

        /// <summary>
        /// Установить элемент в списке по индексу.
        /// </summary>
        /// <param name="list">Список.</param>
        /// <param name="index">Индекс.</param>
        /// <param name="value">Значение, которое нужно установить.</param>
        /// <param name="insert">Нужно ли всавить новый элемент.</param>
        public static void SetItem(dynamic list, dynamic index, dynamic value, bool insert)
        {
            ThrowExceptionIfNotList(list, "LIST_ERROR_SET_ITEM");

            if (!int.TryParse(index.ToString(), out int i))
            {
                throw new Exception(GetErrorText("LIST_ERROR_SET_ITEM", "ERROR_LIST_WRONG_INDEX_TYPE", index.GetType()));
            }
            
            if (insert)
            {
                i = Mathf.Clamp(i, 0, list.Count);
                list.Insert(i, value);
            }
            else
            {
                if (list.Count == 0)
                {
                    list.Add(value);
                }
                else
                {
                    i = Mathf.Clamp(i, 0, list.Count - 1);
                    list[i] = value;
                }
            }
        }

        /// <summary>
        /// Добавить к списку случайный элемент.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <param name="insert"></param>
        public static void SetRandomItem(dynamic list, dynamic value, bool insert)
        {
            ThrowExceptionIfNotList(list, "LIST_ERROR_SET_ITEM");
            dynamic randomIndex = Utils.RandomInt(0, list.Count);
            
            if (insert)
            {
                randomIndex = Mathf.Clamp(randomIndex, 0, list.Count);
                list.Insert(randomIndex, value);
            }
            else
            {
                if (list.Count == 0)
                {
                    list.Add(value);
                }
                else
                {
                    randomIndex = Mathf.Clamp(randomIndex, 0, list.Count - 1);
                    list[randomIndex] = value;
                }
            }
        }

        public static dynamic GetSubList(dynamic list, string where1, int at1, string where2, int at2)
        {
            ThrowExceptionIfNotList(list, "LIST_ERROR_GET_SUBLIST");

            //copied from google blockly default
            var getIndex = new Func<dynamic, int, int>((@where, at) =>
            {
                switch (@where)
                {
                    case "FROM_START":
                        at--;

                        break;
                    case "FROM_END":
                        at = list.Count - at;

                        break;
                    case "FIRST":
                        at = 0;

                        break;
                    case "LAST":
                        at = list.Count - 1;

                        break;
                    default:
                        throw new ApplicationException("Unhandled option (lists_getSublist).");
                }

                return at;
            });

            at1 = getIndex(where1, at1);
            at2 = getIndex(where2, at2);

            if (at1 > at2)
            {
                int swap = at2;
                at2 = at1;
                at1 = swap;
            }

            return list.GetRange(at1, at2 - at1 + 1);
        }

        public static dynamic GetFirstItem(dynamic list, bool remove)
        {
            ThrowExceptionIfNotList(list, "LIST_ERROR_GET_FIRST_ITEM");
            ThrowExceptionIfIsEmpty(list, "LIST_ERROR_GET_FIRST_ITEM");

            dynamic result = list.First();

            if (remove)
            {
                list.RemoveAt(0);
            }

            return result;
        }

        public static dynamic GetLastItem(dynamic list, bool remove)
        {
            ThrowExceptionIfNotList(list, "LIST_ERROR_GET_LAST_ITEM");
            ThrowExceptionIfIsEmpty(list, "LIST_ERROR_GET_LAST_ITEM");

            dynamic result = list.Last();

            if (remove)
            {
                list.RemoveAt(list.Count - 1);
            }

            return result;
        }

        private static bool IsList(object o)
        {
            if (o == null)
            {
                return false;
            }

            var interfaces = o.GetType().GetInterfaces();
            return interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
        }

        public static bool Contains(dynamic sender, dynamic other)
        {
            if (sender == null)
            {
                return false;
            }
            
            if (sender.Equals(other))
            {
                return true;
            }

            return sender switch
            {
                IEnumerable senderList when other is not IEnumerable => senderList.Cast<dynamic>().Any(a => VCompare.Equals(a, other)),
                IEnumerable senderList when other is IEnumerable otherList => otherList.Cast<dynamic>().All(item => senderList.Cast<dynamic>().Any(a => VCompare.Equals(a, item))),
                _ => false
            };
        }

        private static void ThrowExceptionIfIsEmpty(dynamic list, string errorTypeKey)
        {
            if (list.Count == 0)
            {
                throw new Exception(GetErrorText(errorTypeKey, "ERROR_LIST_IS_EMPTY"));
            }
        }

        private static void ThrowExceptionIfNotList(dynamic list, string errorTypeKey)
        {
            if (!IsList(list))
            {
                Type listType = list == null ? null : list.GetType();
                var keyValuePairs = new KeyValuePair<string, object>[]
                {
                    new("provided_type", LocalizationUtils.GetLocalizedType(listType)),
                    new("required_type", LocalizationUtils.GetLocalizedType(typeof(IList))),
                };

                throw new Exception(GetErrorText(errorTypeKey, "ERROR_LIST_WRONG_TYPE", keyValuePairs));
            }
        }

        private static string GetErrorText(string errorTypeKey, string errorKey, params KeyValuePair<string, object>[] values)
        {
            return string.Concat(
                LanguageManager.Instance.GetTextValue(errorTypeKey),
                " ",
                I18next.Format(LanguageManager.Instance.GetTextValue(errorKey), values)
            );
        }
    }
}
#endif