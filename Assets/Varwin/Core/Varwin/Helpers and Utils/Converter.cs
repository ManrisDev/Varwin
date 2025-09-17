using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Varwin.Data.ServerData;
using WanzyeeStudio.Json;

namespace Varwin
{
    public static class Converter
    {
        public static void CastValue(FieldInfo fieldInfo, object value, object thisObject)
        {
            object castedValue = CastValue(fieldInfo.FieldType, value);
            try
            {
                fieldInfo.SetValue(thisObject, castedValue);
            }
            catch (Exception e)
            {
                DefaultValue(fieldInfo, thisObject);
            }
        }

        public static void CastValue(PropertyInfo propertyInfo, object value, object thisObject)
        {
            var getMethod = propertyInfo.GetGetMethod();
            var originalValue = getMethod == null ? null : propertyInfo.GetValue(thisObject);
            object castedValue = CastValue(propertyInfo.PropertyType, value, originalValue);
            try
            {
                propertyInfo.SetValue(thisObject, castedValue);
            }
            catch (Exception e)
            {
                DefaultValue(propertyInfo, thisObject);
            }
        }

        public static bool CastValue(Type valueType, object value, out object result)
        {
            result = CastValue(valueType, value);

            if (result != null)
            {
                return true;
            }

            DefaultValue(valueType, out result);
            return false;
        }

        private static float CastFloat(object value)
        {
            float result;
            try
            {
                result = Convert.ToSingle(value);
            }
            catch (Exception)
            {
                float.TryParse(((string) value).Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            }

            return result;
        }

        private static double CastDouble(object value)
        {
            double result;
            try
            {
                result = Convert.ToDouble(value);
            }
            catch (Exception)
            {
                double.TryParse(((string) value).Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
            }

            return result;
        }
        
        private static int CastInteger(object value)
        {
            int result;
            try
            {
                result = Convert.ToInt32(value);
            }
            catch (Exception)
            {
                int.TryParse((string) value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            }

            return result;
        }

        public static object CastValue(Type type, object value, object originalValue = null)
        {
            if (type.BaseType == typeof(Enum))
            {
                if (value is string stringValue)
                {
                    return Enum.Parse(type, stringValue);
                }

                return value;
            }

            if (type == typeof(bool))
            {
                return Convert.ToBoolean(value);
            }

            if (type == typeof(float))
            {
                return CastFloat(value);
            }

            if (type == typeof(int))
            {
                return CastInteger(value);
            }

            if (type == typeof(decimal))
            {
                return Convert.ToDecimal(value);
            }

            if (type == typeof(double))
            {
                return CastDouble(value);
            }

            if (type == typeof(string))
            {
                return Convert.ToString(value);
            }

            if (type == typeof(char))
            {
                return Convert.ToChar(value);
            }

            if (type == typeof(Color))
            {
                return value is string or JObject ? JsonConvert.DeserializeObject<Color>(value.ToString(), new WanzyeeStudio.Json.ColorConverter()) : value;
            }

            if (type == typeof(Vector2))
            {
                return value is Vector2 vector2 ? vector2 : JsonConvert.DeserializeObject<Vector2>(value.ToString(), new Vector2Converter());
            }

            if (type == typeof(Vector2Int))
            {
                switch (value)
                {
                    case Vector2Int vector2Int:
                        return vector2Int;
                    case Vector2 vector2:
                        return Vector2Int.FloorToInt(vector2);
                    default:
                        return Vector2Int.FloorToInt(JsonConvert.DeserializeObject<Vector2>(value.ToString(), new Vector2Converter()));
                }
            }

            if (type == typeof(Vector3))
            {
                return value is Vector3 vector3 ? vector3 : JsonConvert.DeserializeObject<Vector3>(value.ToString(), new Vector3Converter());
            }

            if (type == typeof(Vector3Int))
            {
                switch (value)
                {
                    case Vector3Int vector3Int:
                        return vector3Int;
                    case Vector3 vector3:
                        return Vector3Int.FloorToInt(vector3);
                    default:
                        return Vector3Int.FloorToInt(JsonConvert.DeserializeObject<Vector3>(value.ToString(), new Vector3Converter()));
                }
            }

            if (type == typeof(Vector4))
            {
                return value is Vector4 vector4 ? vector4 : JsonConvert.DeserializeObject<Vector4>(value.ToString(), new Vector4Converter());
            }

            if (type == typeof(Quaternion))
            {
                return value is Quaternion quaternion ? quaternion : JsonConvert.DeserializeObject<Quaternion>(value.ToString(), new QuaternionConverter());
            }

            if (type == typeof(Rect))
            {
                return value is Rect rect ? rect : JsonConvert.DeserializeObject<Rect>(value.ToString(), new RectConverter());
            }

            if (type == typeof(Vector3Int))
            {
                switch (value)
                {
                    case RectInt rectInt:
                        return rectInt;
                    case Rect rect:
                        return new RectInt(Vector2Int.FloorToInt(rect.position), Vector2Int.FloorToInt(rect.size));
                    default:
                    {
                        var rectFromJson = JsonConvert.DeserializeObject<Rect>(value.ToString(), new RectConverter());
                        return new RectInt(Vector2Int.FloorToInt(rectFromJson.position), Vector2Int.FloorToInt(rectFromJson.size));
                    }
                }
            }

            if (type == typeof(RectOffset))
            {
                return value is RectOffset rectOffset ? rectOffset : JsonConvert.DeserializeObject<RectOffset>(value.ToString(), new RectOffsetConverter());
            }

            if (value is null)
            {
                return null;
            }

            if (type == typeof(VarwinVideoClip) && value is string url)
            {
                return new VarwinVideoClip(url);
            }

            if (type == typeof(GameObject))
            {
                return (GameObject)value;
            }

            if (IsObjectAndTypeMatch<Texture>(type, value) 
                || IsObjectAndTypeMatch<AudioClip>(type, value))
            {
                return value;
            }

            if (type == typeof(Sprite) && value is Texture2D texture && texture)
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 0.5f * Vector2.one, 100f, 0u, SpriteMeshType.FullRect, Vector4.zero, false);
            }

            if (type.IsSubclassOf(typeof(ResourceOnDemand)))
            {
                var resource = (ResourceDto) value;

                return Activator.CreateInstance(type, resource);
            }

            if (type == typeof(string) || !type.ImplementsInterface<IEnumerable>())
            {
                return null;
            }

            var elementType = type.GetElementType() ?? type.GenericTypeArguments.FirstOrDefault();
            
            if (value is JArray jArray)
            {
                var objectList = (IList) jArray.ToObject(typeof(IList));

                if (type.IsArray)
                {
                    int count = objectList.Count;
                    var array = Array.CreateInstance(elementType, count);

                    for (int i = 0; i < count; i++)
                    {
                        array.SetValue(CastValue(elementType, objectList[i]), i);
                    }

                    return array;
                }

                Type listType = typeof(List<>);
                Type itemType = type.GetGenericArguments().Single();
                Type constructedListType = listType.MakeGenericType(itemType);
                var genericList = (IList) Activator.CreateInstance(constructedListType);
                
                foreach (object item in objectList)
                {
                    CastValue(itemType, item, out object result);
                    genericList.Add(result);
                }

                return genericList;
            }

            if (type.IsArray)
            {
                var list = (IList) value;
                Array array = Array.CreateInstance(elementType, list.Count);
                
                int count = list.Count;

                for (int i = 0; i < count; i++)
                {
                    array.SetValue(CastValue(elementType, list[i]), i);
                }
                
                return array;
            }
            else
            {
                var list = (IList) value;
                Type itemType = type.GetGenericArguments().Single();
                Type listType = typeof(List<>);
                Type genericListType = listType.MakeGenericType(itemType);
                
                if (originalValue == null)
                {
                    IList instance = (IList) Activator.CreateInstance(genericListType);
                    
                    foreach (object item in list)
                    {
                        instance.Add(CastValue(itemType, item));
                    }

                    return instance;
                }

                var originalValueList = (IList) value;
                var valueList = (IList) value;
                var originalList = (IList) originalValue;
                var count = originalValueList.Count;

                if (Equals(originalList, valueList))
                {
                    valueList = (IList) Activator.CreateInstance(genericListType);
                    
                    for (var i = 0; i < count; i++)
                    {
                        valueList.Add(originalValueList[i]);
                    }
                }

                originalList.Clear();

                for (int i = 0; i < count; i++)
                {
                    originalList.Add(CastValue(itemType, valueList[i]));
                }
                    
                return originalList;
            }
        }

        public static object DefaultValue(Type type)
        {
            DefaultValue(type, out var result);
            return result;
        }

        public static void DefaultValue(Type type, out object result)
        {
            if (type == null)
            {
                result = null;
                return;
            }

            if (type == typeof(bool))
            {
                result = false;
                return;
            }

            if (type == typeof(float) || type == typeof(decimal) || type == typeof(int))
            {
                result = 0;
                return;
            }

            if (type == typeof(string))
            {
                result = string.Empty;
                return;
            }

            if (type == typeof(char))
            {
                result = '\0';
                return;
            }

            if (type == typeof(Color))
            {
                result = Color.white;
                return;
            }

            if (type.IsEnum)
            {
                result = Activator.CreateInstance(type);
                return;
            }

            result = null;
        }

        private static bool IsObjectAndTypeMatch<T>(Type type, object obj)
        {
            var isTypeMatch = typeof(T) == type;
            var objectType = obj.GetType();
            
            var isObjectTypeMatch = objectType.IsSubclassOf(type) || objectType.IsAssignableFrom(type);
            
            return isTypeMatch && isObjectTypeMatch;
        }

        private static void DefaultValue(FieldInfo fieldInfo, object thisObject)
        {
            DefaultValue(fieldInfo.FieldType, out thisObject);
        }

        private static void DefaultValue(PropertyInfo propertyInfo, object thisObject)
        {
            DefaultValue(propertyInfo.PropertyType, out thisObject);
        }
    }
}