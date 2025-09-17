using UnityEngine;

namespace Varwin.SocketLibrary
{
    /// <summary>
    /// Часть контроллера, отвечающая за коллизии.
    /// </summary>
    public partial class SocketController
    {
        /// <summary>
        /// Тип провайдера коллизий.
        /// </summary>
        private CollisionProvider _collisionProvider;

        /// <summary>
        /// Тип провайдера коллизий.
        /// </summary>
        public CollisionProvider CollisionProvider => GetCollisionProvider();
        
        /// <summary>
        /// Получение провайдера обработчика коллизий.
        /// </summary>
        /// <returns>Провайдер обработчика коллизий.</returns>
        public CollisionProvider GetCollisionProvider()
        {
            if (_collisionProvider)
            {
                return _collisionProvider;
            }
            
            var providerObj = new GameObject("CollisionProvider");
            providerObj.transform.parent = transform;
            _collisionProvider = providerObj.AddComponent<CollisionProvider>();
            _collisionProvider.MainSocketController = this;

            return _collisionProvider;
        }
        
        
    }
}