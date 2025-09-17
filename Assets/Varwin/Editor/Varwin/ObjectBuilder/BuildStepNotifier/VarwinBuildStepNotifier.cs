using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Varwin.Editor
{
    /// <summary>
    /// Класс, вызывающий методы шагов билда билдящихся объектов.
    /// </summary>
    public static class VarwinBuildStepNotifier
    {
        /// <summary>
        /// Объекты, которые билдятся в данный момент.
        /// </summary>
        public static List<ObjectBuildDescription> BuildObjects;

        /// <summary>
        /// Кэш MethodInfo для MonoBehaviour'ов
        /// </summary>
        private static Dictionary<MonoBehaviour, List<MethodInfo>> MethodsCache;

        /// <summary>
        /// Очистка. Автоматически вызывается после завершения билда.
        /// </summary>
        public static void Clear()
        {
            BuildObjects = null;
            MethodsCache = null;
        }

        /// <summary>
        /// Обновить список билдящихся объектов.
        /// </summary>
        /// <param name="objectsToBuild">Список билдящихся объектов.</param>
        public static void UpdateBuildObjects(List<ObjectBuildDescription> objectsToBuild)
        {
            BuildObjects = objectsToBuild;
            MethodsCache = new();
            
            foreach (var description in BuildObjects)
            {
                if(!description.GameObject)
                    continue;

                var monoBehaviours = description.GameObject.GetComponentsInChildren<MonoBehaviour>();

                foreach (var monoBehaviour in monoBehaviours) 
                    MethodsCache.Add(monoBehaviour, monoBehaviour.GetType().GetRuntimeMethods().ToList());
            }
        }

        /// <summary>
        /// Уведомить о входе или выходе из состояния.
        /// </summary>
        /// <param name="gameObject">Объект, для которого срабатывает состояние.</param>
        /// <param name="stateType">Тип состояния.</param>
        /// <param name="isStarted">true - вход в состояние. false - выход из состояния.</param>
        public static void NotifyState(GameObject gameObject, Type stateType, bool isStarted)
        {
            if (!stateType.ImplementInterface(typeof(IBuildingState)))
            {
                Debug.LogError($"Type {stateType} is not building state.");
                return;
            }

            var monoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();
            var methodName = isStarted ? $"On{stateType.Name}Enter" : $"On{stateType.Name}Exit";
            
            NotifyState(monoBehaviours, methodName, null);
        }

        /// <summary>
        /// Уведомить о шаге билда объекта.
        /// </summary>
        /// <param name="gameObject">Объект, для которого срабатывает шаг.</param>
        /// <param name="stateType">Тип шага.</param>
        /// <param name="previewObject">Игровой объект иконки/превью.</param>
        public static void NotifyStateWithPayload(GameObject gameObject, Type stateType, GameObject previewObject)
        {
            if (!stateType.ImplementInterface(typeof(IBuildingState)))
            {
                Debug.LogError($"Type {stateType} is not building state.");
                return;
            }

            var monoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();
            var methodName =$"On{stateType.Name}Work";
            
            NotifyState(monoBehaviours, methodName, previewObject);
        }

        private static void NotifyState(MonoBehaviour[] monoBehaviours, string methodName, object payload)
        {
            foreach (var monoBehaviour in monoBehaviours)
            {
                if (MethodsCache == null || !MethodsCache.TryGetValue(monoBehaviour, out List<MethodInfo> methods)) 
                    methods = GetTypes(monoBehaviour);

                var method = methods.FirstOrDefault(x => x.Name == methodName);

                if (method == null)
                    continue;
                
                method.Invoke(monoBehaviour, method.GetParameters().Length == 1 ? new[] {payload} : null);
            }
        }

        private static List<MethodInfo> GetTypes(MonoBehaviour monoBehaviour)
        {
            MethodsCache ??= new();
            var methods = monoBehaviour.GetType().GetRuntimeMethods().ToList();

            MethodsCache.TryAdd(monoBehaviour, methods);
            return methods;
        }
    }
}