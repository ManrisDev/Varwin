using System;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;
using UnityEngine;
using Varwin.Core;
using Varwin.Public;

namespace Varwin
{
    public class WrappersCollection
    {
        private readonly Dictionary<int, Wrapper> _wrappers = new();
        private readonly Dictionary<string, Type> _types = new();
        private DynamicWrapperCollection _dynamicWrapperCollection;
        public DynamicWrapperCollection All => _dynamicWrapperCollection ??= new DynamicWrapperCollection(Wrappers());

        public void Init()
        {
            _dynamicWrapperCollection = null;
        }
        
        /// <summary>
        /// Add new wrapper to collection
        /// </summary>
        /// <param name="idInstance">Object id</param>
        /// <param name="wrapper">Object wrapper</param>
        public void Add(int idInstance, Wrapper wrapper)
        {
            _wrappers.Add(idInstance, wrapper);
        }

        /// <summary>
        /// Get all wrappers from collection
        /// </summary>
        /// <returns></returns>
        public List<Wrapper> Wrappers()
        {
            List<Wrapper> result = new List<Wrapper>();
            result.AddRange(_wrappers.Values);
            return result;
        }

        /// <summary>
        /// Get all wrappers of type from collection
        /// </summary>
        /// <param name="typeName">Wrapper type name</param>
        /// <returns>List of wrappers of type</returns>
        public List<Wrapper> GetWrappersOfType(string typeName)
        {
            Type type = GetType(typeName);

            return _wrappers.Values.Where(wrapper => wrapper.GetType() == type).ToList();
        }

        /// <summary>
        /// Get wrapper by id
        /// </summary>
        /// <param name="id">Object id</param>
        /// <returns></returns>
        public Wrapper Get(int id)
        {
            if (_wrappers.ContainsKey(id))
            {
                return _wrappers[id];
            }

            var locale = LanguageManager.Instance.GetTextValue("ERROR_WRAPPER_WITH_ID_NOT_FOUND");
            throw new Exception(I18next.Format(locale, new KeyValuePair<string, object>("id", id)));
        }

        /// <summary>
        /// Get collection of wrappers by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public dynamic Get(string type)
        {
            var wrappers = GetWrappersOfType(type);
            DynamicWrapperCollection collection = new DynamicWrapperCollection(wrappers);

            return collection;
        }

        /// <summary>
        /// Determines whether the collection contains the specified object instance id.
        /// </summary>
        /// <param name="id">Object instance id</param>
        /// <returns></returns>
        public bool ContainsKey(int id) => _wrappers.ContainsKey(id);

        /// <summary>
        /// Determines whether the collection contains the wrapper.
        /// </summary>
        /// <param name="wrapper">Wrapper instance</param>
        /// <returns></returns>
        public bool ContainsValue(Wrapper wrapper) => _wrappers.ContainsValue(wrapper);

        /// <summary>
        /// Check existing object with id
        /// </summary>
        /// <param name="id">Object instance id</param>
        /// <returns></returns>
        public bool Exist(int id) => _wrappers.ContainsKey(id);

        /// <summary>
        /// Get wrappers of children object 
        /// </summary>
        /// <param name="target">Target wrapper</param>
        /// <returns></returns>
        public dynamic GetChildren(Wrapper target)
        {
            List<Wrapper> result = new List<Wrapper>();

            foreach (ObjectController child in target.GetObjectController().Children)
            {
                result.Add(child.VarwinObjectDescriptor.Wrapper());
            }

            return new DynamicWrapperCollection(result);
        }

        /// <summary>
        /// Get wrappers of descendants object
        /// </summary>
        /// <param name="target">Target wrapper</param>
        /// <returns></returns>
        public dynamic GetDescendants(Wrapper target)
        {
            List<Wrapper> result = new List<Wrapper>();

            foreach (ObjectController child in target.GetObjectController().Descendants)
            {
                result.Add(child.VarwinObjectDescriptor.Wrapper());
            }

            return new DynamicWrapperCollection(result);
        }

        /// <summary>
        /// Get wrapper of parent object
        /// </summary>
        /// <param name="target">Target wrapper</param>
        /// <returns></returns>
        public dynamic GetParent(Wrapper target)
        {
            ObjectController objectController = target.GetObjectController();
            return objectController.Parent?.VarwinObjectDescriptor.Wrapper();
        }

        /// <summary>
        /// Get wrapper of ancestry object
        /// </summary>
        /// <param name="target">Target wrapper</param>
        /// <returns></returns>
        public dynamic GetAncestry(Wrapper target)
        {
            List<Wrapper> result = new List<Wrapper>();

            ObjectController objectController = target.GetObjectController();
            ObjectController parent = objectController.Parent;

            while (parent != null)
            {
                result.Add(parent.VarwinObjectDescriptor.Wrapper());
                parent = parent.Parent;
            }

            return new DynamicWrapperCollection(result);
        }

        /// <summary>
        /// Clear all wrappers from collection
        /// </summary>
        public void Clear()
        {
            var wrappers = new List<Wrapper>(_wrappers.Values);
            foreach (Wrapper wrapper in wrappers)
            {
                wrapper?.Delete();
            }

            wrappers.Clear();
            _wrappers.Clear();
        }

        /// <summary>
        /// Remove wrapper by id
        /// </summary>
        /// <param name="id"></param>
        public void Remove(int id)
        {
            if (_wrappers.ContainsKey(id))
            {
                _wrappers[id].Delete();
                _wrappers.Remove(id);
            }
            else
            {
                Debug.Log($"Object {id} have no wrapper!");
            }
        }

        private Type GetType(string typeName)
        {
            Type type = null;
            if (_types.ContainsKey(typeName))
            {
                type = _types[typeName];
            }
            else
            {
                var wrapper = _wrappers.Values.FirstOrDefault(w => w.GetType().ToString() == typeName);
                if (wrapper != null)
                {
                    type = wrapper.GetType();
                    _types.Add(typeName, type);
                }
            }

            return type;
        }
    }
}