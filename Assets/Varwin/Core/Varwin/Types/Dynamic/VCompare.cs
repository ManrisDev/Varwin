using UnityEngine;

namespace Varwin
{
    public static class VCompare
    {
        public new static bool Equals(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0

            if (a == null && b == null)
            {
                return true;
            }

            dynamic resultA = a == null ? 0 : a;
            dynamic resultB = b == null ? 0 : b;

            var typeA = resultA.GetType();
            var typeB = resultB.GetType();
                
            if (typeA != typeB)
            {
                if (resultA is Wrapper || resultB is Wrapper)
                {
                    return false;
                }

                DynamicCast.CastValue(a, b, ref resultA, ref resultB);

                if (resultA.GetType() != resultB.GetType())
                {
                    return resultA.Equals(resultB);
                }
            }

            bool mathEquals = false;
            try
            {
                if (a is decimal || b is decimal)
                {
                    mathEquals = Mathf.Approximately(resultA, resultB);
                }
                else
                {
                    mathEquals = Mathf.Approximately((float) resultA, (float) resultB);
                }
            }
            catch
            {
                
            }

            if (DynamicCast.IsNumericType(resultA, resultB))
            {
                return resultA == resultB || mathEquals;
            }

            return resultA.Equals(resultB);

#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool NotEquals(dynamic a, dynamic b)
        {
            return !Equals(a, b);
        }
        
        public static bool Less(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            if (DynamicCast.CanConvertToCompareValues(a, b))
            {
                double resultA = DynamicCast.ConvertToDouble(a);
                double resultB = DynamicCast.ConvertToDouble(b);
  
                return resultA < resultB;
            }

            return false;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool LessOrEquals(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            if (DynamicCast.CanConvertToCompareValues(a, b))
            {
                double resultA = DynamicCast.ConvertToDouble(a);
                double resultB = DynamicCast.ConvertToDouble(b);
                return resultA <= resultB;
            }

            return false;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool Greater(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            if (DynamicCast.CanConvertToCompareValues(a, b))
            {
                double resultA = DynamicCast.ConvertToDouble(a);
                double resultB = DynamicCast.ConvertToDouble(b);

                return resultA > resultB;
            }

            return false;

#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool GreaterOrEquals(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            if (DynamicCast.CanConvertToCompareValues(a, b))
            {
                double resultA = DynamicCast.ConvertToDouble(a);
                double resultB = DynamicCast.ConvertToDouble(b);

                return resultA >= resultB;
            }

            return false;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool And(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            bool resultA = DynamicCast.ConvertToBoolean(a);
            bool resultB = DynamicCast.ConvertToBoolean(b);
            
            return resultA && resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool Or(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0

            bool resultA = DynamicCast.ConvertToBoolean(a);
            bool resultB = DynamicCast.ConvertToBoolean(b);

            return resultA || resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool Not(dynamic a)
        {
#if !NET_STANDARD_2_0
            bool resultA = DynamicCast.ConvertToBoolean(a);

            return !resultA;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool NotEmpty(dynamic a)
        {
#if !NET_STANDARD_2_0
            if (a == null)
            {
                return false;
            }
            
            if (a is bool && a == false)
            {
                return false;
            }

            if (a is string)
            {
                return !string.IsNullOrEmpty(a);
            }

            if (((object) a).IsNumericType())
            {
                return a != 0;
            }

            return true;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
    }
}