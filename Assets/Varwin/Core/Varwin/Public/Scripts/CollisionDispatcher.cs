using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Varwin.Public
{
    public static class CollisionDispatcher
    {
        public delegate IEnumerator CollisionHandler(object target, object other);
        public delegate IEnumerator CollisionWrapperHandler(Wrapper target, Wrapper other);

        private static List<CollisionWrapperHandler> _collisionStartHandlers;
        private static List<CollisionWrapperHandler> _collisionEndHandlers;

        #region CollisionCallbackHandler

        private class KeyValuePair<TKey, TValue>
        {
            public TKey Key;
            public TValue Value;
            
            public KeyValuePair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        public class CollisionCallbackHandler : MonoBehaviour
        {
            public readonly Dictionary<GameObject, HashSet<CollisionHandler>> ObjectStartCollisionHandlers = new();
            public readonly HashSet<CollisionHandler> AllObjectsStartCollisionHandlers = new();

            public readonly Dictionary<GameObject, HashSet<CollisionHandler>> ObjectEndCollisionHandlers = new();
            public readonly HashSet<CollisionHandler> AllObjectsEndCollisionHandlers = new();

            private List<KeyValuePair<GameObject, List<Collider>>> _colliders;

            private Coroutine _checkCollisionCoroutine;

            private void Awake()
            {
                _colliders = new List<KeyValuePair<GameObject, List<Collider>>>();
            }

            private void OnDisable()
            {
                foreach (var collider in _colliders)
                {
                    OnCollisionEndHandler(collider.Key.gameObject);
                }

                _colliders.Clear();
            }

            private void Update()
            {
                if (_colliders.Count == 0)
                {
                    return;
                }
                
                for (int i = _colliders.Count - 1; i >= 0; i--)
                {
                    var keyValuePair = _colliders[i];
                    var rootObject = keyValuePair.Key;
                    if (!rootObject)
                    {
                        _colliders.RemoveAt(i);
                        continue;
                    }

                    var colliders = keyValuePair.Value;

                    if (colliders.Count > 0)
                    {
                        for (int j = colliders.Count - 1; j >= 0; j--)
                        {
                            var targetCollider = colliders[j];
                            if (!targetCollider || !targetCollider.enabled || !targetCollider.gameObject.activeInHierarchy)
                            {
                                colliders.Remove(colliders[j]);
                            }
                        }
                    }

                    if (colliders.Count == 0)
                    {
                        OnCollisionEndHandler(rootObject);
                        _colliders.RemoveAt(i);
                    }
                }
            }

            private void OnCollisionEnter(Collision other)
            {
                if (other == null || !other.collider)
                {
                    return;
                }
                
                OnTriggerEnter(other.collider);
            }

            private void OnCollisionExit(Collision other)
            {
                if (other == null || !other.collider)
                {
                    return;
                }
                
                OnTriggerExit(other.collider);
            }

            private void OnTriggerEnter(Collider other)
            {
                var otherGameObject = other.transform.root.gameObject;
                var objectContainer = _colliders.Find(a => a.Key == otherGameObject);
                if (objectContainer == null)
                {
                    _colliders.Add(new KeyValuePair<GameObject, List<Collider>>(otherGameObject, new List<Collider> {other}));
                    OnCollisionStartHandler(otherGameObject);
                }
                else if(!objectContainer.Value.Contains(other))
                {
                    objectContainer.Value.Add(other);
                }
            }

            private void OnTriggerExit(Collider other)
            {
                var otherGameObject = other.transform.root.gameObject;
                var objectContainer = _colliders.Find(a => a.Key == otherGameObject);
                objectContainer?.Value.Remove(other);
            }
            
            private void OnCollisionStartHandler(GameObject other)
            {
                if (gameObject.activeInHierarchy && other && ProjectData.IsPlayMode)
                {
                    var selfWrapper = gameObject.GetWrapper();
                    var otherWrapper = other.GetWrapper();
                    if (selfWrapper != null && otherWrapper != null)
                    {
                        AnyCollisionStart(this, selfWrapper, otherWrapper);    
                    }
                    
                    OnCollisionHandler(other, ObjectStartCollisionHandlers, AllObjectsStartCollisionHandlers);
                }
            }

            private void OnCollisionEndHandler(GameObject other)
            {
                if (gameObject.activeInHierarchy && other && ProjectData.IsPlayMode)
                {
                    var selfWrapper = gameObject.GetWrapper();
                    var otherWrapper = other.GetWrapper();
                    if (selfWrapper != null && otherWrapper != null)
                    {
                        AnyCollisionEnd(this, selfWrapper, otherWrapper);    
                    }
                    
                    OnCollisionHandler(other, ObjectEndCollisionHandlers, AllObjectsEndCollisionHandlers);
                }
            }

            private void OnCollisionHandler(GameObject other, IReadOnlyDictionary<GameObject, HashSet<CollisionHandler>> objectCollisionHandlers, HashSet<CollisionHandler> allCollisionHandlers)
            {
                var otherWrapper = other.GetWrapper();
                if (objectCollisionHandlers.TryGetValue(other, out var callbackSet))
                {
                    foreach (var callback in callbackSet)
                    {
                        var selfWrapper = gameObject.GetWrapper();
                        RunCoroutine(callback?.Invoke(selfWrapper == null ? gameObject : selfWrapper, otherWrapper == null ? other : otherWrapper));
                    }
                }
#if VARWINCLIENT
                else if (otherWrapper.GetGameObject() && CloneManager.TryGetOriginal(otherWrapper, out var originalObject) && objectCollisionHandlers.TryGetValue(originalObject.GetGameObject(), out var originalCallbackSet))
                {
                    foreach (var callback in originalCallbackSet)
                    {
                        var selfWrapper = gameObject.GetWrapper();
                        RunCoroutine(callback?.Invoke(selfWrapper == null ? gameObject : selfWrapper, otherWrapper == null ? other : otherWrapper));
                    }
                }
#endif

                foreach (var callback in allCollisionHandlers)
                {
                    var selfWrapper = gameObject.GetWrapper();
                    RunCoroutine(callback?.Invoke(selfWrapper == null ? gameObject : selfWrapper, otherWrapper == null ? other : otherWrapper));
                }
            }
            
            public void RunCoroutine(IEnumerator enumerator)
            {
#if VARWINCLIENT
                if (SceneLogic.Instance)
                {
                    SceneLogic.Instance.StartCoroutine(enumerator);
                }
                else
                {
                    StartCoroutine(enumerator);
                }
#endif
            }
        }

        #endregion // CollisionCallbackHandler

        #region Collision Start Handlers

        public static void AddCollisionStartHandler(CollisionWrapperHandler callback)
        {
            TryInitializeCollisionDispatcher();
            
            _collisionStartHandlers ??= new List<CollisionWrapperHandler>();

            if (_collisionStartHandlers.Contains(callback))
            {
                return;
            }
            
            _collisionStartHandlers.Add(callback);
        }

        private static void TryInitializeCollisionDispatcher()
        {
            var wrapperCollection = GameStateData.GetWrapperCollection();
            foreach (var wrapper in wrapperCollection.All)
            {
                var gameObject = wrapper.GetGameObject();
                if (!gameObject.GetComponent<CollisionCallbackHandler>())
                {
                    gameObject.AddComponent<CollisionCallbackHandler>();
                }
            }
        }

        public static void AddCollisionStartHandler(Wrapper wrapper, Wrapper other, CollisionHandler callback)
        {
            AddCollisionStartHandler(wrapper, new DynamicWrapperCollection(new List<Wrapper> {other}), callback);
        }

        public static void AddCollisionStartHandler(DynamicWrapperCollection wrappers, Wrapper other, CollisionHandler callback)
        {
            foreach (Wrapper wrapper in wrappers)
            {
                AddCollisionStartHandler(wrapper, new DynamicWrapperCollection(new List<Wrapper> {other}), callback);
            }
        }

        public static void AddCollisionStartHandler(DynamicWrapperCollection wrappers, DynamicWrapperCollection collection, CollisionHandler callback)
        {
            foreach (Wrapper wrapper in wrappers)
            {
                AddCollisionStartHandler(wrapper, collection, callback);
            }
        }

        public static void AddCollisionStartHandler(Wrapper wrapper, DynamicWrapperCollection collection, CollisionHandler callback)
        {
            GameObject wrapperObject = wrapper.GetGameObject();
            var callbackHandler = wrapperObject.GetComponent<CollisionCallbackHandler>();
            if (!callbackHandler)
            {
                callbackHandler = wrapperObject.AddComponent<CollisionCallbackHandler>();
            }

            AddCollisionHandler(wrapperObject, collection, callback, callbackHandler.ObjectStartCollisionHandlers, callbackHandler.AllObjectsStartCollisionHandlers);
        }

        #endregion // Collision Start Handlers

        #region Collision End Handlers

        
        public static void AddCollisionEndHandler(CollisionWrapperHandler callback)
        {
            TryInitializeCollisionDispatcher();
            
            _collisionEndHandlers ??= new List<CollisionWrapperHandler>();

            if (_collisionEndHandlers.Contains(callback))
            {
                return;
            }
            
            _collisionEndHandlers.Add(callback);
        }

        public static void AddCollisionEndHandler(Wrapper wrapper, Wrapper other, CollisionHandler callback)
        {
            AddCollisionEndHandler(wrapper, new DynamicWrapperCollection(new List<Wrapper> {other}), callback);
        }

        public static void AddCollisionEndHandler(DynamicWrapperCollection wrappers, Wrapper other, CollisionHandler callback)
        {
            foreach (Wrapper wrapper in wrappers)
            {
                AddCollisionEndHandler(wrapper, new DynamicWrapperCollection(new List<Wrapper> {other}), callback);
            }
        }

        public static void AddCollisionEndHandler(DynamicWrapperCollection wrappers, DynamicWrapperCollection collection, CollisionHandler callback)
        {
            foreach (Wrapper wrapper in wrappers)
            {
                AddCollisionEndHandler(wrapper, collection, callback);
            }
        }

        public static void AddCollisionEndHandler(Wrapper wrapper, DynamicWrapperCollection collection, CollisionHandler callback)
        {
            GameObject wrapperObject = wrapper.GetGameObject();
            var callbackHandler = wrapperObject.GetComponent<CollisionCallbackHandler>();
            if (!callbackHandler)
            {
                callbackHandler = wrapperObject.AddComponent<CollisionCallbackHandler>();
            }

            AddCollisionHandler(wrapperObject, collection, callback, callbackHandler.ObjectEndCollisionHandlers, callbackHandler.AllObjectsEndCollisionHandlers);
        }

        #endregion // Collision End Handlers

        private static void AddCollisionHandler(Object target, DynamicWrapperCollection collection, CollisionHandler callback, IDictionary<GameObject, HashSet<CollisionHandler>> objectCollisionHandlers, ISet<CollisionHandler> allCollisionHandlers)
        {
            if (collection == null || collection.Count == 0)
            {
                allCollisionHandlers.Add(callback);
            }
            else
            {
                foreach (Wrapper wrapper in collection)
                {
                    GameObject itemObject = wrapper.GetGameObject();

                    if (!itemObject || itemObject == target)
                    {
                        continue;
                    }

                    if (!objectCollisionHandlers.ContainsKey(itemObject))
                    {
                        objectCollisionHandlers.Add(itemObject, new HashSet<CollisionHandler>());
                    }

                    objectCollisionHandlers[itemObject].Add(callback);
                }
            }
        }

        private static void AnyCollisionStart(CollisionCallbackHandler handler, Wrapper sender, Wrapper other)
        {
            if (_collisionStartHandlers == null)
            {
                return;
            }

            foreach (var collisionStartHandler in _collisionStartHandlers.Where(collisionStartHandler => collisionStartHandler != null))
            {
                handler.RunCoroutine(collisionStartHandler?.Invoke(sender, other));
            }
        }
        
        private static void AnyCollisionEnd(CollisionCallbackHandler handler, Wrapper sender, Wrapper other)
        {
            if (_collisionEndHandlers == null)
            {
                return;
            }

            foreach (var collisionEndHandler in _collisionEndHandlers.Where(collisionEndHandler => collisionEndHandler != null))
            {
                handler.RunCoroutine(collisionEndHandler?.Invoke(sender, other));
            }
        }

        public static void RemoveAllGlobalEventHandlers()
        {
            _collisionStartHandlers?.Clear();
            _collisionEndHandlers?.Clear();
        }
    }
}