using System;
using UnityEngine;

namespace Varwin.XR
{
    /// <summary>
    /// Интерфейс для получения информации о положении глаз.
    /// </summary>
    public interface IEyePoseProvider
    {
        /// <summary>
        /// Экземпляр фабрики.
        /// </summary>
        public static IEyePoseProvider _instance;

        /// <summary>
        /// Экземпляр фабрики.
        /// </summary>
        public static IEyePoseProvider Instance => GetInstance();

        /// <summary>
        /// Добавление компонента на объект.
        /// </summary>
        /// <returns>Компонент.</returns>
        public Component AddComponent(GameObject targetObject, bool isLeft);

        /// <summary>
        /// Получение экземпляра провайдера.
        /// </summary>
        /// <returns>Фабрика компонентов.</returns>
        private static IEyePoseProvider GetInstance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = CreateInstance();
            return _instance;
        }

        /// <summary>
        /// Создание экземпляа из всех возможных типов.
        /// </summary>
        /// <returns>Созданный экземпляр.</returns>
        private static IEyePoseProvider CreateInstance()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }
                
                foreach (var type in assembly.GetExportedTypes())
                {
                    if (type.ImplementInterface(typeof(IEyePoseProvider)))
                    {
                        return (IEyePoseProvider) Activator.CreateInstance(type);
                    }
                }
            }

            return null;
        }
    }
}