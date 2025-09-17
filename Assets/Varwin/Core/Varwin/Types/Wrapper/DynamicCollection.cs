using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Varwin
{
    public class DynamicCollection<T> : DynamicObject, IList<T>
    {
        public List<T> Collection;

        protected readonly Type CollectionType;
        protected IList<Type> LastUsedTypeArgs;

        private readonly Dictionary<string, FieldInfo> _fieldInfos;
        private readonly Dictionary<string, PropertyInfo> _propertyInfos;

        public DynamicCollection(List<T> collection)
        {
            if (collection == null)
            {
                return;
            }

            Collection = collection;
            CollectionType = collection.Count > 0 ? collection[0].GetType() : null;

            if (CollectionType is not null)
            {
                _fieldInfos = CollectionType.GetFields().ToDictionary(field => field.Name);
                _propertyInfos = CollectionType.GetProperties().ToDictionary(property => property.Name);
            }
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (CollectionType == null)
            {
                result = null;
                return false;
            }

            var methodName = binder.Name;
            var types = args.Select(x => x.GetType()).ToArray();
            var methodInfo = CollectionType.GetMethod(methodName, types);

            if (methodInfo == null)
            {
                result = null;
                return false;
            }

            if (methodInfo.ContainsGenericParameters)
            {
                var csharpBinder = binder.GetType().GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");
                LastUsedTypeArgs = csharpBinder.GetProperty("TypeArguments")?.GetValue(binder, null) as IList<Type>;

                methodInfo = methodInfo.MakeGenericMethod(LastUsedTypeArgs[0]);
            }

            var resultList = Collection.Select(obj => methodInfo.Invoke(obj, args)).Where(res => res != null).ToList();

            result = resultList;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_fieldInfos != null && _fieldInfos.TryGetValue(binder.Name, out var fieldInfo))
            {
                SetField(fieldInfo, value);
                return true;
            }

            if (_propertyInfos != null && _propertyInfos.TryGetValue(binder.Name, out var propertyInfo))
            {
                SetProperty(propertyInfo, value);
                return true;
            }

            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object value)
        {
            if (_fieldInfos != null && _fieldInfos.TryGetValue(binder.Name, out var fieldInfo))
            {
                value = fieldInfo.GetValue(Collection.First());
                return true;
            }

            if (_propertyInfos != null && _propertyInfos.TryGetValue(binder.Name, out var propertyInfo))
            {
                value = propertyInfo.GetValue(Collection.First());
                return true;
            }

            value = null;
            return false;
        }

        private void SetField(FieldInfo field, object value)
        {
            if (Collection == null)
            {
                return;
            }
            
            foreach (var obj in Collection)
            {
                field.SetValue(obj, value);
            }
        }

        private void SetProperty(PropertyInfo property, object value)
        {
            if (Collection == null)
            {
                return;
            }

            foreach (var obj in Collection)
            {
                property.SetValue(obj, value);
            }
        }

        #region IList implementation

        public IEnumerator<T> GetEnumerator()
        {
            return Collection?.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item) => Collection.Add(item);
        public void Clear() => Collection.Clear();
        public bool Contains(T item) => Collection.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => Collection.CopyTo(array, arrayIndex);
        public bool Remove(T item) => Collection.Remove(item);

        public DynamicCollection<T> GetRange(int index, int count) => new(Collection.GetRange(index, count));

        public int Count => Collection?.Count ?? 0;
        public bool IsReadOnly { get; }

        public int IndexOf(T item) => Collection.IndexOf(item);
        public void Insert(int index, T item) => Collection.Insert(index, item);
        public void RemoveAt(int index) => Collection.RemoveAt(index);

        public T this[int index]
        {
            get => Collection[index];
            set => Collection[index] = value;
        }

        #endregion
    }
}