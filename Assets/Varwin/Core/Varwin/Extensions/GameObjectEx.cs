using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Varwin.Public
{
    public static class GameObjectEx
    {
        /// <summary>
        /// Копирование компонента в объект.
        /// </summary>
        /// <param name="destination">Объект назначения.</param>
        /// <param name="sourceComponent">Исходный компонент.</param>
        /// <typeparam name="T">Тип компонента.</typeparam>
        /// <returns>Клонированный компонент.</returns>
        public static T CopyComponent<T>(this GameObject destination, T sourceComponent) where T : Component
        {
            if (!sourceComponent)
            {
                return null;
            }

            var type = sourceComponent.GetType();
            var destinationComponent = destination.GetComponent(type) as T;
            if (!destinationComponent)
            {
                destinationComponent = destination.AddComponent(type) as T;
            }

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.IsStatic)
                {
                    continue;
                }

                field.SetValue(destinationComponent, field.GetValue(sourceComponent));
            }

            var props = type.GetProperties();

            foreach (var prop in props)
            {
                if (!prop.CanWrite || prop.Name == "name")
                {
                    continue;
                }

                prop.SetValue(destinationComponent, prop.GetValue(sourceComponent, null), null);
            }

            return destinationComponent;
        }


        public static bool Requires(Type obj, Type requirement)
        {
            //also check for m_Type1 and m_Type2 if required
            return Attribute.IsDefined(obj, typeof(RequireComponent))
                   && Attribute.GetCustomAttributes(obj, typeof(RequireComponent))
                       .OfType<RequireComponent>()
                       .Any(rc => rc.m_Type0.IsAssignableFrom(requirement));
        }

        public static bool CanDestroy(this GameObject go, Type t)
        {
            return !go.GetComponents<Component>().Any(c => Requires(c.GetType(), t));
        }

        public static void DestroyComponents(this GameObject preview)
        {
            var saveIter = 10;
            while (true)
            {
                var components = preview.GetComponentsInChildren<Component>(true);
                var requiresComponents = new List<Component>();

                foreach (Component component in components)
                {
                    if (!component
                        || component is Highlighter
                        || component is MeshFilter
                        || component is Renderer
                        || component is Transform)
                    {
                        continue;
                    }

                    try
                    {
                        if (component.gameObject.CanDestroy(component.GetType()))
                        {
                            Object.Destroy(component);
                        }

                        else
                        {
                            requiresComponents.Add(component);
                        }
                    }
                    catch
                    {
                        TryFixDestroy(component);
                    }
                }

                if (requiresComponents.Count <= 0 || saveIter <= 0)
                {
                    return;
                }

                saveIter--;
            }
        }

        private static void TryFixDestroy(Component component)
        {
            var behaviour = component as Behaviour;

            if (behaviour)
            {
                behaviour.enabled = false;
            }

            var collider = component as Collider;

            if (collider)
            {
                collider.enabled = false;
            }
        }

        public static Wrapper GetWrapper(this GameObject self)
        {
            Transform parentTransform = self.transform;
            VarwinObjectDescriptor varwinObjectDescriptor = null;
            do
            {
                varwinObjectDescriptor = parentTransform.GetComponent<VarwinObjectDescriptor>();
                parentTransform = parentTransform.parent;
            } 
            while (parentTransform && !varwinObjectDescriptor);

            if (!varwinObjectDescriptor)
            {
                return new NullWrapper(self);
            }

            return varwinObjectDescriptor.Wrapper();
        }

        public static List<InputController> GetInputControllers(this GameObject self) =>
            GetInputControllersFromGO(self).Values.ToList();

        public static InputController GetRootInputController(this GameObject self)
        {
            return GetInputControllersFromGO(self).Values.Single(x => x.IsRoot);
        }

        private static Dictionary<int, InputController> GetInputControllersFromGO(GameObject gameObject)
        {
            var objectBehaviour = gameObject.GetComponentInParent<ObjectBehaviourWrapper>(true);

            if (!objectBehaviour)
            {
                return null;
            }

            var objectController = objectBehaviour.OwdObjectController;
            return objectController.GetInputControllersWithIds();
        }

        /// <summary>
        /// Выполнение действия для каждого найденного в дочерних объектах компонента.
        /// </summary>
        /// <param name="obj"><Объект.</param>
        /// <param name="action">Действие.</param>
        /// <typeparam name="T">Тип компонента</typeparam>
        public static void ForEachComponent<T>(this GameObject obj, Action<T> action) where T : Component
        {
            var components = obj.GetComponentsInChildren<T>();

            if (components == null || components.Length == 0)
            {
                return;
            }

            foreach (var component in components)
            {
                action?.Invoke(component);
            }
        }
    }
}