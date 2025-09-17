using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

namespace Varwin
{
    /// <summary>
    /// По сути копипаста DynamicCast, нл реализованная через подход TryGetValue.
    /// </summary>
    public static partial class DynamicCast
    {
        public static bool TryConvertToFloat(dynamic a, out dynamic result)
        {
#if !NET_STANDARD_2_0
            dynamic resultA = a == null ? 0f : a;

            if (a is float)
            {
                result = a;
                return true;
            }

            if (a is double || a is int)
            {
                result = Convert.ToSingle(resultA, CultureInfo.InvariantCulture);
                return true;
            }

            if (a is bool)
            {
                result = a ? 1f : 0f;
                return true;
            }

            string resultStringA = string.Empty;
            try
            {
                //try get number with culture
                resultStringA = resultA.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                resultStringA = resultA.ToString();
            }

            try
            {
                result = Convert.ToSingle(resultStringA, CultureInfo.InvariantCulture);

                return true;
            }
            catch
            {
#if UNITY_EDITOR
                Log($"Can not convert \"{resultStringA}\" to float!");
#endif
                result = 0f;
                return false;
            }

#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool TryConvertToDouble(dynamic a, out dynamic result)
        {
#if !NET_STANDARD_2_0
            dynamic resultA = a == null ? 0d : a;

            if (a is double)
            {
                result = a;
                return true;
            }

            if (a is float || a is int)
            {
                result = Convert.ToDouble(resultA, CultureInfo.InvariantCulture);
                return true;
            }

            if (a is bool)
            {
                result = a ? 1d : 0d;
                return true;
            }

            string resultStringA = resultA.ToString();

            if (IsNumericType(resultA))
            {
                resultStringA = resultA.ToString(CultureInfo.InvariantCulture);
            }

            try
            {
                result = Convert.ToDouble(resultA, CultureInfo.InvariantCulture);

                return true;
            }

            catch
            {
#if UNITY_EDITOR
                Log($"Can not convert \"{resultStringA}\" to double!");
#endif
                result = 0d;
                return false;
            }

#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool TryConvertToInt(dynamic a, out dynamic result)
        {
#if !NET_STANDARD_2_0
            if (a is null)
            {
                result = 0;
                return false;
            }

            if (a is int)
            {
                result = a;
                return true;
            }

            if (a is bool)
            {
                result = a ? 1 : 0;
                return true;
            }

            if (a is Enum)
            {
                result = (int) a;
                return true;
            }

            string stringInt = VString.ConvertToString(a);

            try
            {
                double f = Convert.ToDouble(stringInt, CultureInfo.InvariantCulture);
                result = (int)Math.Floor(f + 0.5f);

                return true;

            }
            catch (Exception e)
            {
                #if UNITY_EDITOR
                Log($"Cannot convert {a} to int");
                #endif

                result = 0;
                return false;
            }
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool TryConvertToBoolean(dynamic a, out dynamic result)
        {
#if !NET_STANDARD_2_0
            if (a is null)
            {
                result = false;
                return false;
            }

            if (a is bool)
            {
                result = true;
                return true;
            }

            if (a is int)
            {
                result = a > 0;
                return true;
            }

            string stringBool = VString.ConvertToString(a);

            try
            {
                result = Convert.ToBoolean(stringBool, CultureInfo.InvariantCulture);

                return true;

            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Log($"Cannot convert {a} to bool");
                
#endif
                result = false;
                return false;
            }
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool TryConvertToDecimal(dynamic a, out dynamic result)
        {
#if !NET_STANDARD_2_0
            if (a is null)
            {
                result = 0;
                return false;
            }

            if (a is decimal)
            {
                result = a;
                return true;
            }

            string decimalString = VString.ConvertToString(a);

            try
            {
                result = Convert.ToDecimal(decimalString, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
#if UNITY_EDITOR
                Log($"Can not convert {a} to decimal");
#endif
                result = 0;
                return false;
#else
                throw new WrongApiCompatibilityLevelException();
#endif
            }
        }
        
        public static bool TryCastValueToType(dynamic value, Type convertToType, out dynamic result)
        {
            if (convertToType == typeof(bool))
            {
                return TryConvertToBoolean(value, out result);
            }

            if (convertToType == typeof(float))
            {
                return TryConvertToFloat(value, out result);
            }

            if (convertToType == typeof(int))
            {
                return TryConvertToInt(value, out result);
            }

            if (convertToType == typeof(decimal))
            {
                return TryConvertToDecimal(value, out result);
            }

            if (convertToType == typeof(double))
            {
                return TryConvertToDouble(value, out result);
            }

            if (convertToType == typeof(Color))
            {
                return TryConvertColor(value, out result);
            }

            if (convertToType == typeof(string))
            {
                result = value.ToString();
                return true;
            }

            if (!convertToType.IsValueType & TryConvertReference(value, convertToType, out dynamic castedValue))
            {
                result = castedValue;
                return true;
            }

            if (convertToType.IsEnum)
            {
                return TryConvertEnum(value, convertToType, out result);
            }

            result = DefaultNullValue(convertToType);

            return false;
        }

        private static bool TryConvertColor(dynamic value, out dynamic result)
        {
            result = Color.black;

            if (value == null)
            {
                return false;
            }

            Type valueType = value.GetType();
            if (valueType == null)
            {
                return false;
            }

            if (valueType == typeof(Color))
            {
                result = value;
                return true;
            }

            if (valueType == typeof(string))
            {
                string valueString = (string)value;
                if (ColorUtility.TryParseHtmlString(valueString, out Color parsedColor))
                {
                    result = parsedColor;
                    return true;
                }
            }

            return false;
        }

        private static bool TryConvertEnum(dynamic value, Type convertToType, out dynamic result)
        {
            result = null;

            if (value == null || !convertToType.IsEnum)
            {
                return false;
            }

            Type valueType = value.GetType();
            if (valueType == typeof(int))
            {
                int valueInt = (int)value;
                Array enumValues = Enum.GetValues(convertToType);

                if (enumValues.Length > valueInt)
                {
                    result = enumValues.GetValue(valueInt);
                    return true;
                }

                return false;
            }

            string valueString = value.ToString();
            if (Enum.IsDefined(convertToType, valueString))
            {
                return Enum.TryParse(convertToType, valueString, out result);
            }

            return false;
        }

        public static dynamic ConvertValueToType(dynamic value, Type convertToType)
        {
            if (convertToType == typeof(bool))
            {
                return ConvertToBoolean(value);
            }

            if (convertToType == typeof(float))
            {
                return ConvertToFloat(value);
            }

            if (convertToType == typeof(int))
            {
                return ConvertToInt(value);
            }

            if (convertToType == typeof(decimal))
            {
                return ConvertToDecimal(value);
            }

            if (convertToType == typeof(double))
            {
                return ConvertToDouble(value);
            }

            if (convertToType == typeof(string))
            {
                return value.ToString();
            }

            if (!convertToType.IsValueType & TryConvertReference(value, convertToType, out dynamic convertedValue))
            {
                return convertedValue;
            }

            return null;
        }

        /// <summary>
        /// Попытаться сконвертить исходное значение ссылочного типа.
        /// </summary>
        /// <param name="value">Исходное значение ссылочного типа.</param>
        /// <param name="convertToType">Тип, в который будет происходить конвертация.</param>
        /// <param name="result">Результат конвертации.</param>
        /// <returns>Удалось ли сконвертировать.</returns>
        public static bool TryConvertReference(dynamic value, Type convertToType, out dynamic result)
        {
            Type valueType = value.GetType();
            result = null;

            if (valueType.IsValueType)
            {
                return false;
            }

            if (convertToType.IsAssignableFrom(valueType))
            {
                result = value;
                return true;
            }

            // Каст листов и массивов.
            if (typeof(IEnumerable).IsAssignableFrom(convertToType))
            {
                if (convertToType.IsArray)
                {
                    if (TryConvertToArray(value, convertToType, out Array castedArray))
                    {
                        result = castedArray;
                        return true;
                        
                    }

                    return false;
                }

                if (typeof(IList).IsAssignableFrom(convertToType))
                {
                    if (TryConvertToList(value, convertToType, out IList castedList))
                    {
                        result = castedList;
                        return true;
                    }

                    return false;
                }

                return false;
            }

            return false;
        }
    }
}