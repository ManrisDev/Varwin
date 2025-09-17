using System;
using System.Collections.Generic;
using UnityEngine;

namespace Varwin
{
    public static class DynamicValueDictionary
    {
        private static readonly Dictionary<string, dynamic> ValuesDictionary = new();

        static DynamicValueDictionary()
        {
            ProjectData.SceneChanged += Clear;
            Application.quitting += Unsubscribe;
        }

        public static dynamic Get(string key)
        {
            if (ValuesDictionary.ContainsKey(key))
            {
                return ValuesDictionary[key];
            }

            throw new IndexOutOfRangeException($"{ValuesDictionary[key]}");
        }

        public static void Set(string key, dynamic value)
        {
            ValuesDictionary[key] = value;
        }

        public static bool Contains(string key)
        {
            return ValuesDictionary.ContainsKey(key);
        }

        public static void Remove(string key)
        {
            ValuesDictionary.Remove(key);
        }

        public static void Clear()
        {
            ValuesDictionary.Clear();
        }

        public static void Unsubscribe()
        {
            ProjectData.SceneChanged -= Clear;
            Application.quitting -= Unsubscribe;
        }
    }
}