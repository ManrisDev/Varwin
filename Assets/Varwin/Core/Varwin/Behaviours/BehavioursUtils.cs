using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Varwin.Core.Behaviours;

namespace Varwin
{
    public static class BehavioursUtils
    {
        public static List<KeyValuePair<string, dynamic>> GenerateBehavioursEnumKeyValuePairs()
        {
            var allBehavioursEnumTypes = GetAllBehavioursEnumTypes();
            var result = new List<KeyValuePair<string, dynamic>>();

            foreach (var behavioursEnumType in allBehavioursEnumTypes)
            {
                if (behavioursEnumType.BaseType != typeof(Enum))
                {
                    continue;
                }

                var values = Enum.GetValues(behavioursEnumType);
                foreach (var value in values)
                {
                    var key = $"{value.GetType().ToString().Replace('+', '.')}.{value}";
                    result.Add(new KeyValuePair<string, dynamic>(key, value));
                }
            }

            return result;
        }

        private static List<Type> GetAllBehavioursEnumTypes()
        {
            var types = BehavioursCollection.GetAllBehavioursTypes();

            var methodCollection = types.Select(x => x.GetMethods(BindingFlags.Public | BindingFlags.Instance));
            var propertiesCollection = types.Select(x => x.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            var methods = new List<MethodInfo>();
            var properties = new List<PropertyInfo>();

            foreach (var methodInfos in methodCollection)
            {
                methods.AddRange(methodInfos);
            }

            foreach (var propertyInfos in propertiesCollection)
            {
                properties.AddRange(propertyInfos);
            }

            var enumTypes = new List<Type>();
            enumTypes.AddRange(ParseMethodsForEnums(methods));
            enumTypes.AddRange(ParsePropertiesForEnums(properties));

            return enumTypes;
        }

        private static IEnumerable<Type> ParseMethodsForEnums(IEnumerable<MethodInfo> methodInfos)
        {
            var enumTypes = new HashSet<Type>();

            foreach (var methodInfo in methodInfos)
            {
                var enums = methodInfo.GetParameters()
                    .Where(x => x.ParameterType.Namespace.StartsWith("Varwin.Core.Behaviours") && x.ParameterType.BaseType == typeof(Enum))
                    .Select(x => x.ParameterType);

                enumTypes.AddRange(enums);
            }

            return enumTypes;
        }

        private static IEnumerable<Type> ParsePropertiesForEnums(IEnumerable<PropertyInfo> propertyInfos)
        {
            var enumTypes = new HashSet<Type>();

            foreach (var propertyInfo in propertyInfos)
            {
                var propertyType = propertyInfo.PropertyType;

                if (propertyType.Namespace != null && propertyType.Namespace.StartsWith("Varwin.Core.Behaviours") && propertyType.BaseType == typeof(Enum))
                {
                    enumTypes.Add(propertyType);
                }
            }

            return enumTypes;
        }
    }
}