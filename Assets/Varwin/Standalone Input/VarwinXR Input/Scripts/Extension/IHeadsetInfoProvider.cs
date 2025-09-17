using System;

namespace Varwin.XR
{
    /// <summary>
    /// Контекст предоставления информации о шлеме.
    /// </summary>
    public interface IHeadsetInfoProvider
    {
        /// <summary>
        /// Экземпляр.
        /// </summary>
        protected static IHeadsetInfoProvider _instance;

        /// <summary>
        /// Есть ли наследованный тип.
        /// </summary>
        private static bool _hasChildType = true;

        /// <summary>
        /// Получение имени шлема.
        /// </summary>
        /// <returns>Имя шлема.</returns>
        string GetHeadsetName();

        /// <summary>
        /// Является ли левый контроллер правым, а правый левым.
        /// </summary>
        /// <returns>Истина, если является.</returns>
        public virtual bool IsSidesInverted() => false;
        
        /// <summary>
        /// Получение экземпляра провайдера.
        /// </summary>
        /// <returns>Экземпляр провайдера.</returns>
        public static IHeadsetInfoProvider GetInstance()
        {
            if (!_hasChildType)
            {
                return null;
            }

            if (_instance != null)
            {
                return _instance;
            }
            
            _instance = CreateInstance();
            if (_instance != null)
            {
                return _instance;
            }
            
            _hasChildType = false;
            return null;
        }

        /// <summary>
        /// Создание экземпляа из всех возможных типов.
        /// </summary>
        /// <returns>Созданный экземпляр.</returns>
        private static IHeadsetInfoProvider CreateInstance()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }
                
                foreach (var type in assembly.GetExportedTypes())
                {
                    if (type.ImplementInterface(typeof(IHeadsetInfoProvider)))
                    {
                        return (IHeadsetInfoProvider) Activator.CreateInstance(type);
                    }
                }
            }

            return null;
        }
    }
}