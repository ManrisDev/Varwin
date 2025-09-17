using System.Reflection;

namespace Varwin.Core
{
    public static class CustomAttributeEx
    {
        public static T FastGetCustomAttribute<T>(this PropertyInfo t, bool inherit)
        {
            var objs = t.GetCustomAttributes(typeof(T), inherit);
            if (objs.Length > 0)
            {
                return (T) objs[0];
            }

            return default(T);
        }
        
        public static T FastGetCustomAttribute<T>(this MethodInfo t, bool inherit)
        {
            var objs = t.GetCustomAttributes(typeof(T), inherit);
            if (objs.Length > 0)
            {
                return (T) objs[0];
            }

            return default(T);
        }
        
        public static T FastGetCustomAttribute<T>(this MemberInfo t, bool inherit)
        {
            var objs = t.GetCustomAttributes(typeof(T), inherit);
            if (objs.Length > 0)
            {
                return (T) objs[0];
            }

            return default(T);
        }
    }
}