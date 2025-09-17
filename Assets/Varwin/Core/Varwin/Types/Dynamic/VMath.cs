using System;
using UnityEngine;

namespace Varwin
{
    public static class VMath
    {
        public static dynamic Sum(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            int intA = 0;
            int intB = 0;

            if (TryGetInteger(a, out intA) && TryGetInteger(b, out intB))
            {
                return intA + intB;
            }

            float resultA = DynamicCast.ConvertToFloat(a);
            float resultB = DynamicCast.ConvertToFloat(b);

            return resultA + resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Subtraction(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            int intA = 0;
            int intB = 0;

            if (TryGetInteger(a, out intA) && TryGetInteger(b, out intB))
            {
                return intA - intB;
            }

            float resultA = DynamicCast.ConvertToFloat(a);
            float resultB = DynamicCast.ConvertToFloat(b);

            return resultA - resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Multiply(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            int intA = 0;
            int intB = 0;

            if (TryGetInteger(a, out intA) && TryGetInteger(b, out intB))
            {
                return intA * intB;
            }

            float resultA = DynamicCast.ConvertToFloat(a);
            float resultB = DynamicCast.ConvertToFloat(b);

            return resultA * resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Division(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            int intA = 0;
            int intB = 0;

            if (TryGetInteger(a, out intA) && TryGetInteger(b, out intB))
            {
                if (intB == 0)
                {
                    return float.PositiveInfinity;
                }

                if (intA % intB == 0)
                {
                    return intA / intB;
                }

                return (float) intA / intB;
            }

            float resultA = DynamicCast.ConvertToFloat(a);
            float resultB = DynamicCast.ConvertToFloat(b);

            if (Mathf.Abs(resultB) < Mathf.Epsilon)
            {
                return float.PositiveInfinity;
            }

            return resultA / resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Pow(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            int intA = 0;
            int intB = 0;

            if (TryGetInteger(a, out intA) && TryGetInteger(b, out intB))
            {
                return Mathf.RoundToInt(Mathf.Pow(intA, intB));
            }

            float resultA = DynamicCast.ConvertToFloat(a);
            float resultB = DynamicCast.ConvertToFloat(b);

            return Mathf.Pow(resultA, resultB);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Sqrt(dynamic a)
        {
#if !NET_STANDARD_2_0
            int intA = 0;

            if (TryGetInteger(a, out intA))
            {
                var result = Mathf.Sqrt(intA);
                if (Mathf.Abs(result % 1) < Mathf.Epsilon)
                {
                    return Mathf.RoundToInt(result);
                }

                return result;
            }

            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Sqrt(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Exp(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Exp(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Pow10(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);
            if (TryGetInteger(a, out int intA))
            {
                return Mathf.RoundToInt(Mathf.Pow(10, intA));
            }

            return Mathf.Pow(10.0f, resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Abs(dynamic a)
        {
#if !NET_STANDARD_2_0
            int intA = 0;

            if (TryGetInteger(a, out intA))
            {
                return Mathf.Abs(intA);
            }

            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Abs(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Negative(dynamic a)
        {
#if !NET_STANDARD_2_0
            int intA = 0;

            if (TryGetInteger(a, out intA))
            {
                return -intA;
            }

            float resultA = DynamicCast.ConvertToFloat(a);

            return -resultA;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Log(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Math.Log(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Log10(dynamic a)
        {
#if !NET_STANDARD_2_0
            if (TryGetInteger(a, out int intA))
            {
                var result = Mathf.Log10(intA);
                if (Mathf.Abs(result % 1) < Mathf.Epsilon)
                {
                    return Mathf.RoundToInt(result);
                }

                return result;
            }
            
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Log10(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Sin(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Sin(resultA / 180f * Mathf.PI);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Cos(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Cos(resultA / 180f * Mathf.PI);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Tan(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Tan(resultA / 180f * Mathf.PI);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Asin(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Asin(resultA) / Mathf.PI * 180f;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Acos(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Acos(resultA) / Mathf.PI * 180f;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Atan(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Atan(resultA) / Mathf.PI * 180f;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool IsEven(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Abs(resultA % 2) < Mathf.Epsilon;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool IsOdd(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Abs(resultA % 2 - 1f) < Mathf.Epsilon;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool IsPrime(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            // http://en.wikipedia.org/wiki/Primality_test#Naive_methods
            if (Mathf.Abs(resultA - 2f) < Mathf.Epsilon || Mathf.Abs(resultA - 3f) < Mathf.Epsilon)
            {
                return true;
            }

            // False if n is NaN, negative, is 1, or not whole. And false if n is divisible by 2 or 3.
            if (double.IsNaN(resultA) || resultA <= 1 || Mathf.Abs(resultA % 1) > Mathf.Epsilon || Mathf.Abs(resultA % 2) < Mathf.Epsilon || Mathf.Abs(resultA % 3) < Mathf.Epsilon)
            {
                return false;
            }

            // Check all the numbers of form 6k +/- 1, up to sqrt(n).
            for (var x = 6; x <= Mathf.Sqrt(resultA) + 1; x += 6)
            {
                if (Mathf.Abs(resultA % (x - 1)) < Mathf.Epsilon || Mathf.Abs(resultA % (x + 1)) < Mathf.Epsilon)
                {
                    return false;
                }
            }

            return true;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool IsWhole(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.Abs(resultA % 1) < Mathf.Epsilon;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool IsPositive(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return resultA > 0;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool IsNegative(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return resultA < 0;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool DivisionBy(dynamic a, dynamic divisionBy)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);
            float resultDivisionBy = DynamicCast.ConvertToFloat(divisionBy);

            return Mathf.Abs(resultA % resultDivisionBy) < Mathf.Epsilon;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Round(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);

            return Mathf.RoundToInt(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic RoundUp(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);
            return Mathf.CeilToInt(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic RoundDown(dynamic a)
        {
#if !NET_STANDARD_2_0
            float resultA = DynamicCast.ConvertToFloat(a);
            return Mathf.FloorToInt(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic RandomInt(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            int resultA = DynamicCast.ConvertToInt(a);
            int resultB = DynamicCast.ConvertToInt(b);

            return resultA < resultB ? Utils.RandomInt(resultA, resultB) : Utils.RandomInt(resultB, resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic RandomDouble() => Utils.RandomDouble();

        public static dynamic RandomFloat() => Utils.RandomFloat();

        private static bool TryGetInteger(dynamic value, out int intValue)
        {
            if (value is int or uint or byte or sbyte)
            {
                intValue = value;
                return true;
            }

            if (value is string str && int.TryParse(str, out var stringInt))
            {
                intValue = stringInt;
                return true;
            }

            if (value is float f && Mathf.Abs(f - Mathf.RoundToInt(f)) < Mathf.Epsilon)
            {
                intValue = (int) f;
                return true;
            }

            intValue = 0;
            return false;
        }
    }
}