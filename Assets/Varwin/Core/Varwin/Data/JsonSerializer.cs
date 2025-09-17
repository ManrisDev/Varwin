using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Varwin.Data
{
    public static class JsonSerializer
    {
        public static string ToJson(this IJsonSerializable self)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };
            return JsonConvert.SerializeObject(self, settings);
        }

        public static T JsonDeserialize<T>(this string json) where T : IJsonSerializable
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                return JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (Exception e)
            {
                throw new Exception($"Can't deserialize {typeof(T)} json. Message: {e.Message} Data: {json}");
            }
        }

        public static List<T> JsonDeserializeList<T>(this string json) where T : IJsonSerializable
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                return JsonConvert.DeserializeObject<List<T>>(json, settings);
            }
            catch (Exception e)
            {
                throw new Exception($"Can't deserialize {typeof(T)} json. Message: {e.Message} Data: {json}");
            }
        }
    }

    public interface IJsonSerializable
    {
    }
}