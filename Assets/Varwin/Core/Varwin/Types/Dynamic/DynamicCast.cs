using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Varwin
{
    public static partial class DynamicCast
    {
        private static HashSet<Type> UnityTypes = new()
        {
            typeof(Color), typeof(Vector4), typeof(Vector3), typeof(Vector3Int), typeof(Vector2), typeof(Vector2Int)
        };

        public static dynamic DefaultNullValue(Type resultType)
        {
            dynamic result = 0;

            if (resultType == typeof(float))
            {
                result = 0f;
            }

            if (resultType == typeof(decimal))
            {
                result = 0m;
            }

            if (resultType == typeof(double))
            {
                result = 0d;
            }

            if (resultType == typeof(int))
            {
                result = 0;
            }

            if (resultType == typeof(string))
            {
                result = "";
            }

            if (resultType == typeof(bool))
            {
                result = false;
            }

            return result;
        }

#if !NET_STANDARD_2_0
        public static bool CanConvertToCompareValues(dynamic a, dynamic b) => CanConvertToComparingValue(a) && CanConvertToComparingValue(b);
#else
        public static bool CanConvertToCompareValues(object a, object b) => CanConvertToComparingValue(a) && CanConvertToComparingValue(b);
#endif


#if !NET_STANDARD_2_0
        public static bool CanConvertToComparingValue(dynamic o)
#else
        public static bool CanConvertToComparingValue(object o)
#endif
        {
            if (o is null)
            {
                return false;
            }

            var type = o.GetType();
            return IsNumericType(o) || type == typeof(bool) || type == typeof(string) || TypeIsUnityType(type);
        }

        public static bool IsTypeIsConvertableType(Type type) => TypeIsNumericType(type) || type == typeof(bool) || type == typeof(string);

        public static bool TypeIsNumericType(Type type) => type.TypeIsNumeric() || type.TypeIsFloatingPointNumeric();

        public static bool TypeIsUnityType(Type type) => UnityTypes.Contains(type);

        public static bool IsNumericType(object a, object b) => a.IsNumericType() && b.IsNumericType();

        public static bool IsNumericType(this object o)
        {
            return o != null && (o.GetType().TypeIsNumeric() || o.GetType().TypeIsFloatingPointNumeric());
        }


        public static bool TypeIsNumeric(this Type o)
        {
            if (o == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(o))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        public static bool TypeIsFloatingPointNumeric(this Type o)
        {
            if (o == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(o))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static float ConvertToFloat(dynamic a)
        {
#if !NET_STANDARD_2_0
            dynamic resultA = a == null ? 0f : a;

            if (a is float)
            {
                return a;
            }

            if (a is double || a is int)
            {
                return Convert.ToSingle(resultA, CultureInfo.InvariantCulture);
            }

            if (a is bool)
            {
                return a ? 1f : 0f;
            }

            string resultStringA = resultA.ToString(CultureInfo.InvariantCulture);

            try
            {
                resultA = Convert.ToSingle(resultA, CultureInfo.InvariantCulture);

                return resultA;
            }

            catch
            {
#if UNITY_EDITOR
                Log($"Can not convert \"{resultStringA}\" to float!");
#endif
                return 0f;
            }

#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static double ConvertToDouble(dynamic a)
        {
#if !NET_STANDARD_2_0
            dynamic resultA = a == null ? 0d : a;

            if (a is double)
            {
                return a;
            }

            if (a is float || a is int)
            {
                return Convert.ToDouble(resultA, CultureInfo.InvariantCulture);
            }

            if (a is bool)
            {
                return a ? 1d : 0d;
            }

            string resultStringA = resultA.ToString();

            if (IsNumericType(resultA))
            {
                resultStringA = resultA.ToString(CultureInfo.InvariantCulture);
            }

            try
            {
                resultA = Convert.ToDouble(resultA, CultureInfo.InvariantCulture);

                return resultA;
            }

            catch
            {
#if UNITY_EDITOR
                Log($"Can not convert \"{resultStringA}\" to double!");
#endif
                return 0d;
            }

#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static void CastValue(dynamic a, dynamic b, ref dynamic resultA, ref dynamic resultB)
        {
#if !NET_STANDARD_2_0
            resultA = a == null ? 0 : a;
            resultB = b == null ? 0 : b;

            if (!CanConvertToCompareValues(resultA, resultB))
            {
                return;
            }

            Type resultType = resultA.GetType();

            string stringB = b != null ? b.ToString() : "";

            if (IsNumericType(b))
            {
                stringB = resultB.ToString(CultureInfo.InvariantCulture);
            }

            if (resultB is bool)
            {
                stringB = (resultB == true) ? "1" : "";
            }

            if (resultA is bool)
            {
                try
                {
                    if (resultB is string)
                    {
                        if (resultA == false && resultB == "")
                        {
                            resultB = false;
                        }
                        else if (ConvertToDouble(stringB) == 1.0)
                        {
                            resultB = true;
                        }
                        else
                        {
                            resultB = !resultA;
                        }
                    }
                    else
                    {
                        resultB = (ConvertToDouble(stringB) == 1.0);
                    }

                }
                catch
                {
#if UNITY_EDITOR
                    Log($"Can not convert {b} to boolean");
#endif
                    resultB = DefaultNullValue(resultType);
                }

                return;
            }

            if (resultA is float)
            {
                try
                {
                    resultB = Convert.ToSingle(stringB, CultureInfo.InvariantCulture);
                }
                catch
                {
#if UNITY_EDITOR
                    Log($"Can not convert {b} to float");
#endif
                    resultB = DefaultNullValue(resultType);
                }

                return;
            }

            if (resultA is int)
            {
                try
                {
                    resultB = Convert.ToInt32(stringB, CultureInfo.InvariantCulture);
                }
                catch
                {
#if UNITY_EDITOR
                    Log($"Can not convert {b} to int");
#endif
                    resultB = DefaultNullValue(resultType);
                }

                return;
            }

            if (resultA is decimal)
            {
                try
                {
                    resultB = Convert.ToDecimal(stringB, CultureInfo.InvariantCulture);
                }
                catch
                {
#if UNITY_EDITOR
                    Log($"Can not convert {b} to decimal");
#endif
                    resultB = DefaultNullValue(resultType);
                }

                return;
            }

            if (resultA is double)
            {
                try
                {
                    resultB = Convert.ToDouble(stringB, CultureInfo.InvariantCulture);
                }
                catch
                {
#if UNITY_EDITOR
                    Log($"Can not convert {b} to double");
#endif
                    resultB = DefaultNullValue(resultType);
                }

                return;
            }

            if (resultA is string)
            {
                resultB = stringB;
            }
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        private static void Log(string message)
        {
            Debug.Log(message);
        }

        public static int ConvertToInt(dynamic a)
        {
#if !NET_STANDARD_2_0
            if (a is null)
            {
                return 0;
            }

            if (a is int)
            {
                return a;
            }

            if (a is bool)
            {
                return a ? 1 : 0;
            }

            string stringInt = VString.ConvertToString(a);

            try
            {
                double f = Convert.ToDouble(stringInt, CultureInfo.InvariantCulture);
                int result = (int)Math.Floor(f + 0.5f);

                return result;

            }
            catch (Exception e)
            {
                Log($"Cannot convert {a} to int");

                return 0;
            }
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool ConvertToBoolean(dynamic a)
        {
#if !NET_STANDARD_2_0
            if (a is null)
            {
                return false;
            }

            if (a is bool)
            {
                return a;
            }

            if (a is int)
            {
                return a > 0;
            }

            string stringBool = VString.ConvertToString(a);

            try
            {
                bool result = Convert.ToBoolean(stringBool, CultureInfo.InvariantCulture);

                return result;

            }
            catch (Exception e)
            {
                Log($"Cannot convert {a} to bool");

                return false;
            }
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static decimal ConvertToDecimal(dynamic a)
        {
#if !NET_STANDARD_2_0
            if (a is null)
            {
                return default;
            }

            if (a is decimal)
            {
                return a;
            }

            string decimalString = VString.ConvertToString(a);

            try
            {
                return Convert.ToDecimal(decimalString, CultureInfo.InvariantCulture);
            }
            catch
            {
#if UNITY_EDITOR
                Log($"Can not convert {a} to decimal");
#endif
                return DefaultNullValue(typeof(decimal));
#else
                throw new WrongApiCompatibilityLevelException();
#endif
            }
        }

        public static Vector3 ConvertToVector3(dynamic x, dynamic y, dynamic z)
        {
            float convertedX = ConvertToFloat(x);
            float convertedY = ConvertToFloat(y);
            float convertedZ = ConvertToFloat(z);

            return new Vector3(convertedX, convertedY, convertedZ);
        }

        public static Vector3Int ConvertToVector3Int(dynamic x, dynamic y, dynamic z)
        {
            int convertedX = ConvertToInt(x);
            int convertedY = ConvertToInt(y);
            int convertedZ = ConvertToInt(z);

            return new Vector3Int(convertedX, convertedY, convertedZ);
        }

        public static Vector2 ConvertToVector2(dynamic x, dynamic y)
        {
            float convertedX = ConvertToFloat(x);
            float convertedY = ConvertToFloat(y);

            return new Vector2(convertedX, convertedY);
        }

        public static Vector2Int ConvertToVector2Int(dynamic x, dynamic y)
        {
            int convertedX = ConvertToInt(x);
            int convertedY = ConvertToInt(y);

            return new Vector2Int(convertedX, convertedY);
        }
    }
}