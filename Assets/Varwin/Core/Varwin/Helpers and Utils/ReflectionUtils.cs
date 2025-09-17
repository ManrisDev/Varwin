using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Varwin
{
    public class PropertyWrapper
    {
        public Func<object, object> Getter;
        public Action<object, object> Setter;
    }
    
    public static class ReflectionUtils
    {
        public static PropertyWrapper BuildPropertyWrapper(PropertyInfo propertyInfo)
        {
            return new PropertyWrapper
            {
                Getter = propertyInfo.GetValue,
                Setter = propertyInfo.SetValue
            };
        }
    }
}