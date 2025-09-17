using System;

namespace Varwin.Core
{
    public static class EnumEx
    {
        public static T Next<T>(this T src) where T : Enum
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");
            }

            var arr = (T[]) Enum.GetValues(src.GetType());
            int j = (Array.IndexOf(arr, src) + 1) % arr.Length;
            while (Equals(arr[j], src))
            {
                j = (j + 1) % arr.Length;
            }
            return arr.Length == j ? arr[0] : arr[j];
        }
    }
}