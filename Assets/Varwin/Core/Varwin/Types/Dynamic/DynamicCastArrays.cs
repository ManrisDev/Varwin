using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Varwin
{
    /// <summary>
    /// Часть класса DynamicCast, отвечающая за конвертацию в массивы и листы.
    /// </summary>
    public static partial class DynamicCast
    {
        /// <summary>
        /// Попытаться сконвертировать исходное значение в лист.
        /// </summary>
        /// <param name="value">Исхожное значение.</param>
        /// <param name="convertToType">Тип, в который нужно сконвертировать.</param>
        /// <param name="resultArray">Результат каста.</param>
        /// <returns>Удалось ли скастить исходное значение в массив.</returns>
        public static bool TryConvertToArray(dynamic value, Type convertToType, out Array resultArray)
        {
            resultArray = null;

            if (value == null || convertToType == null ||!convertToType.IsArray)
            {
                return false;
            }

            Type valueType = value.GetType();
            Type targetArrayElementType = convertToType.GetElementType();
            if (targetArrayElementType == null)
            {
                return false;
            }

            // Попытка скастить в массив или лист.
            if (typeof(IEnumerable).IsAssignableFrom(valueType))
            {
                // array[] -> array[]
                if (valueType.IsArray)
                {
                    Type valueArrayElementType = valueType.GetElementType();

                    if (valueArrayElementType == null)
                    {
                        resultArray = null;
                        return false;
                    }

                    Array sourceArray = (Array)value;
                    resultArray = Array.CreateInstance(targetArrayElementType, sourceArray.Length);

                    // Если элемент требуемого массива можно напрямую засетить из элемента базового массива.
                    if (targetArrayElementType.IsAssignableFrom(valueArrayElementType))
                    {
                        for (int i = 0; i < sourceArray.Length; i++)
                        {
                            resultArray.SetValue(sourceArray.GetValue(i), i);
                        }
     
                        return true;
                    }

                    // Если элемент исходного массива можно сконвертировать в элемент требуемого массива.
                    if (IsTypeIsConvertableType(valueArrayElementType) && IsTypeIsConvertableType(targetArrayElementType))
                    {
                        for (int i = 0; i < sourceArray.Length; i++)
                        {
                            if (TryCastValueToType(sourceArray.GetValue(i), targetArrayElementType, out dynamic castedElement))
                            {
                                resultArray.SetValue(castedElement);
                            }
                        }

                        return true;
                    }

                    return false;
                }

                //list -> array[]
                if (typeof(IList).IsAssignableFrom(valueType))
                {
                    return TryConvertListToArray(value, convertToType, out resultArray);
                }
            }

            // Тип value не является массивом или листом
            // value -> new array[] {value}
            resultArray = Array.CreateInstance(targetArrayElementType, 1);
            if (IsTypeIsConvertableType(valueType) && IsTypeIsConvertableType(targetArrayElementType))
            {
                resultArray.SetValue(ConvertValueToType(value, targetArrayElementType), 0);
                return true;
            }

            if (targetArrayElementType.IsAssignableFrom(valueType))
            {
                resultArray = Array.CreateInstance(targetArrayElementType, 1);
                resultArray.SetValue(value, 0);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Попытаться сконвертировать исходное значение в лист.
        /// </summary>
        /// <param name="value">Исходное значение.</param>
        /// <param name="convertToType">Тип листа, в который будет произведен каст.</param>
        /// <param name="resultList">Результат каста.</param>
        /// <returns>Успешно ли произошел каст исходного значения в лист.</returns>
        public static bool TryConvertToList(dynamic value, Type convertToType, out IList resultList)
        {
            resultList = null;
            if (!typeof(IList).IsAssignableFrom(convertToType))
            {
                return false;
            }

            Type valueType = value.GetType();
            if (convertToType.IsAssignableFrom(valueType))
            {
                resultList = value as IList;
                return resultList is not null;
            }

            Type targetListElementType = convertToType.GetGenericArguments().FirstOrDefault();
            if (targetListElementType is null)
            {
                return false;
            }

            // array -> list
            if (valueType.IsArray)
            {
                return TryCastArrayToList(value, convertToType, out resultList);
            }
            
            Type genericListType = typeof(List<>).MakeGenericType(targetListElementType);
            resultList = (IList)Activator.CreateInstance(genericListType);

            //list -> list
            if (typeof(IList).IsAssignableFrom(valueType))
            {
                IList valueList = value as IList;
                Type valueListElementType = valueType.GetGenericArguments().FirstOrDefault();

                if (valueList is null || valueListElementType is null)
                {
                    return false;
                }

                // Требуемый тип элемента можно назначить из исходного типа элемета. 
                if (targetListElementType.IsAssignableFrom(valueListElementType))
                {
                    foreach (dynamic item in valueList)
                    {
                        resultList.Add(item);
                    }

                    return true;
                }

                // Требуемый тип элемента листа и исходный тип элемента листа можно сконвертировать друг в друга.
                if (IsTypeIsConvertableType(targetListElementType) && IsTypeIsConvertableType(valueListElementType))
                {
                    foreach (dynamic element in valueList)
                    {
                        if (TryCastValueToType(element, targetListElementType, out dynamic castedElement))
                        {
                            resultList.Add(castedElement);
                        }
                    }

                    return true;
                }

                return false;
            }

            // Попытка скастить исходное значение как единственный элемент листа. value:ValueType -> list<ValueType> {value}
            if (targetListElementType.IsAssignableFrom(valueType))
            {
                resultList.Add(value);
                return true;
            }

            // Попытка сконвертировать исходное значение как единственный элемент листа.
            //value:ValueType -> list<ListElementType> {Cast(value->ListElementType)}
            if (TryCastValueToType(value, targetListElementType, out dynamic castedValue))
            {
                resultList.Add(castedValue);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Попытка сконвертировать лист в массив.
        /// </summary>
        /// <param name="list">Исходный лист</param>
        /// <param name="arrayType">Тип массива, в который нужно сконвертировать.</param>
        /// <param name="resultArray">Массив, полученный в результате каста.</param>
        /// <returns>Улалось ли скастить массив.</returns>
        public static bool TryConvertListToArray(dynamic list, Type arrayType, out Array resultArray)
        {
            resultArray = null;

            if (list is null || arrayType is null || !arrayType.IsArray || list is not IList sourceList)
            {
                return false;
            }

            Type listType = list.GetType();
            if (listType is null)
            {
                return false;
            }

            Type listElementType = listType.GetGenericArguments().FirstOrDefault();
            Type arrayElementType = arrayType.GetElementType();

            if (listElementType is null || arrayElementType is null)
            {
                return false;
            }

            resultArray = Array.CreateInstance(arrayElementType, sourceList.Count);

            // Элемент листа можно напрямую скастить в элемент массива
            if (arrayElementType.IsAssignableFrom(listElementType))
            {
                for (var i = 0; i < sourceList.Count; i++)
                {
                    resultArray.SetValue(sourceList[i], i);
                }

                return true;
            }

            // Элемент листа можно сконвертировать в элемент массива при помощи DynamicCast
            if (IsTypeIsConvertableType(arrayElementType) && IsTypeIsConvertableType(listElementType))
            {
                for (var i = 0; i < sourceList.Count; i++)
                {
                    if (TryCastValueToType(sourceList[i], arrayElementType, out dynamic convertedElement))
                    {
                        resultArray.SetValue(convertedElement, i);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Попытаться скастить массив в лист.
        /// </summary>
        /// <param name="array">Исходный массив.</param>
        /// <param name="listType">Тип листа, в который будет произведен каст.</param>
        /// <param name="resultList">Результат каста массивав в лист.</param>
        /// <returns>Успешно ли произошел каст.</returns>
        public static bool TryCastArrayToList(dynamic array, Type listType, out IList resultList)
        {
            resultList = null;

            if (array == null || listType == null)
            {
                return false;
            }

            Type arrayType = array.GetType();

            if (arrayType is not { IsArray: true } || typeof(IList<>).IsAssignableFrom(listType))
            {
                return false;
            }

            Type listElementType = listType.GetGenericArguments().FirstOrDefault();
            Type arrayElementType = arrayType.GetElementType();

            if (listElementType is null || arrayElementType is null || array is not Array sourceArray)
            {
                return false;
            }
            
            Type genericListType = typeof(List<>).MakeGenericType(arrayElementType);
            resultList = Activator.CreateInstance(genericListType) as IList;
            if (resultList is null)
            {
                return false;
            }

            if (listElementType.IsAssignableFrom(arrayElementType))
            {
                foreach (dynamic element in sourceArray)
                {
                    resultList.Add(element);
                }

                return true;
            }

            if (IsTypeIsConvertableType(listElementType) && IsTypeIsConvertableType(arrayElementType))
            {
                foreach (dynamic element in sourceArray)
                {
                    if (TryCastValueToType(element, arrayElementType, out dynamic castedElement))
                    {
                        resultList.Add(castedElement);
                    }
                }

                return sourceArray.Length ==resultList.Count;
            }

            return false;
        }
    }
}