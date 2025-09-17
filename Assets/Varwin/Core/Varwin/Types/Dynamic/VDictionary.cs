#if !NET_STANDARD_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Varwin
{
    public static class VDictionary
    {
        public static Dictionary<dynamic, dynamic> Create()
        {
            return new Dictionary<dynamic, dynamic>();
        }
        
        public static Dictionary<dynamic, dynamic> Create(params KeyValuePair<dynamic, dynamic>[] pairs)
        {
            return pairs.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        
        public static bool IsEmpty(dynamic o)
        {
            if (!CastToDictionary(o, out Dictionary<dynamic, dynamic> dictionary))
            {
                throw new Exception("Dictionary is empty error! Variable is not a Dictionary<dynamic, dynamic>.");
            }

            return dictionary.Count == 0;
        }

        public static dynamic GetFirstValue(dynamic o, bool remove = false)
        {
            if (!CastToDictionary(o, out Dictionary<dynamic, dynamic> dictionary))
            {
                throw new Exception("Get dictionary first item error! Variable is not a Dictionary<dynamic, dynamic>.");
            }

            if (dictionary.Count == 0)
            {
                return null;
            }

            var result = dictionary.First();

            if (remove)
            {
                dictionary.Remove(result.Key);
            }

            return result.Value;
        }

        public static dynamic GetLastValue(dynamic o, bool remove = false)
        {
            if (!CastToDictionary(o, out Dictionary<dynamic, dynamic> dictionary))
            {
                throw new Exception("Get dictionary last item error! Variable is not a Dictionary<dynamic, dynamic>.");
            }

            if (dictionary.Count == 0)
            {
                return null;
            }

            var result = dictionary.Last();

            if (remove)
            {
                dictionary.Remove(result.Key);
            }

            return result.Value;
        }

        public static dynamic GetValueByKey(dynamic o, dynamic key, bool remove = false)
        {
            if (!CastToDictionary(o, out Dictionary<dynamic, dynamic> dictionary))
            {
                throw new Exception("Get dictionary value by key error! Variable is not a Dictionary<dynamic, dynamic>.");
            }

            if (!dictionary.TryGetValue(key, out dynamic value))
            {
                return null;
            }

            if (remove)
            {
                dictionary.Remove(key);
            }

            return value;
        }

        public static void AddOrSetValueByKey(dynamic o, dynamic key, dynamic value)
        {
            if (!CastToDictionary(o, out Dictionary<dynamic, dynamic> dictionary))
            {
                throw new Exception("Add or set dictionary value by key error! Variable is not a Dictionary<dynamic, dynamic>.");
            }
            
            dictionary[key] = value;
        }

        public static void AddOrSetValueByKey(dynamic o, KeyValuePair<dynamic, dynamic> pair)
        {
            if (!CastToDictionary(o, out Dictionary<dynamic, dynamic> dictionary))
            {
                throw new Exception("Add or set dictionary value by key error! Variable is not a Dictionary<dynamic, dynamic>.");
            }
            
            dictionary[pair.Key] = pair.Value;
        }

        public static List<dynamic> GetKeysList(dynamic o)
        { 
            if (!CastToDictionary(o, out Dictionary<dynamic, dynamic> dictionary))
            {
                throw new Exception("Get dictionary keys list error! Variable is not a Dictionary<dynamic, dynamic>.");
            }

            return dictionary.Keys.ToList();
        }

        public static List<dynamic> GetValuesList(dynamic o)
        { 
            if (!CastToDictionary(o, out Dictionary<dynamic, dynamic> dictionary))
            {
                throw new Exception("Get dictionary values list error! Variable is not a Dictionary<dynamic, dynamic>.");
            }

            return dictionary.Values.ToList();
        }

        public static List<dynamic> GetFirstKeyByValue(dynamic o, dynamic value)
        { 
            if (!CastToDictionary(o, out Dictionary<dynamic, dynamic> dictionary))
            {
                throw new Exception("Get first key from dictionary by value error! Variable is not a Dictionary<dynamic, dynamic>.");
            }

            foreach (var pair in dictionary)
            {
                if (VCompare.Equals(pair.Value, value))
                {
                    return pair.Key;
                }
            }

            return null;
        }

        public static List<dynamic> GetLastKeyByValue(dynamic o, dynamic value)
        { 
            if (!CastToDictionary(o, out Dictionary<dynamic, dynamic> dictionary))
            {
                throw new Exception("Get first key from dictionary by value error! Variable is not a Dictionary<dynamic, dynamic>.");
            }

            return GetFirstKeyByValue(dictionary.Reverse(), value);
        }

        private static bool CastToDictionary(object o, out Dictionary<dynamic, dynamic> dictionary)
        {
            if (o != null)
            {
                dictionary = o as Dictionary<dynamic, dynamic>;
                return dictionary != null;
            }

            dictionary = null;
            return false;
        }
    }
}
#endif