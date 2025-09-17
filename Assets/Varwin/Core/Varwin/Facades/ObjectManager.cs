using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Varwin.Public;
using Object = UnityEngine.Object;

namespace Varwin
{
    [UsedImplicitly]
    public static class ObjectManager
    {
        public static GameObject Instantiate(GameObject original)
        {
            return Instantiate(original, original.transform.root);
        }
        
        public static GameObject Instantiate(GameObject original, Transform parent)
        {
            return Instantiate(original, parent, false);
        }
        
        public static GameObject Instantiate(GameObject original, Transform parent, bool worldPositionStays)
        {
            GameObject gameObject = Object.Instantiate(original, parent, worldPositionStays);
            
            InitializeInputControl(gameObject);

            return gameObject;
        }
        
        private static void InitializeInputControl(GameObject gameObject)
        {
            var behaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(true).Where(x => x is IVarwinInputAware);
            
            foreach (var monoBehaviour in behaviours)
            {
                var objectId = monoBehaviour.GetComponent<ObjectId>() ?? 
                               monoBehaviour.gameObject.AddComponent<ObjectId>();
            
                objectId.Id = objectId.GetInstanceID();
                gameObject.GetWrapper()?.GetObjectController()?.AddInputControl(monoBehaviour, objectId, out bool haveInputControl);
            }
        }
        
        public static void DestroyImmediate(GameObject gameObject)
        {
            DestroyInputControl(gameObject);
            Object.DestroyImmediate(gameObject);
        }

        private static void DestroyInputControl(GameObject gameObject)
        {
            var behaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(true).Where(x => x is IVarwinInputAware);
            
            foreach (var monoBehaviour in behaviours)
            {
                var objectId = monoBehaviour.GetComponent<ObjectId>();

                if (!objectId)
                {
                    continue;
                }
            
                objectId.Id = objectId.GetInstanceID();
                gameObject.GetWrapper()?.GetObjectController()?.TryDestroyInputControl(monoBehaviour, objectId);
            }
        }

        /// <summary>
        /// Создание объекта с указанными трансформациями на основе паттерна для поиска в библиотеке.
        /// </summary>
        /// <param name="search">Паттерн поиска.</param>
        /// <param name="position">Позиция.</param>
        /// <param name="rotation">Поворот.</param>
        /// <param name="scale">Масштаб.</param>
        /// <param name="callback">Метод, вызываемый после спауна объекта.</param>
        public static IEnumerator Spawn(string search, Vector3 position, Quaternion rotation, Vector3 scale, Action<ObjectController> callback = null)
        {
#if VARWINCLIENT
            yield return SpawnManager.Spawn(search, position, rotation, scale, callback);
#else
            Debug.LogWarning("Spawn is only supported in client build!");
            yield return null;
#endif
        }

        /// <summary>
        /// Удаление указанного объекта.
        /// </summary>
        /// <param name="objectController">Объект.</param>
        public static IEnumerator Destroy(ObjectController objectController)
        {
            ObjectControllerUtils.DeselectObjects();            
            yield return ObjectControllerUtils.DeleteObjectsWithChildren(new[] {objectController});
        }
    }
}