using System.Linq;
using UnityEngine;
using Varwin.ObjectsInteractions;
using Varwin.Public;

namespace Varwin.SocketLibrary
{
    /// <summary>
    /// Костыль, нужен для обхода ограничений CollisionController'a.
    /// </summary>
    public class CollisionProvider : ChainedJointControllerBase
    {
        /// <summary>
        /// Главный контроллер.
        /// </summary>
        public SocketController MainSocketController { get; internal set; }

        /// <summary>
        /// Обработчик коллизий.
        /// </summary>
        private CollisionController _collisionController;

        /// <summary>
        /// Обработчик коллизий.
        /// </summary>
        public CollisionController CollisionController => GetCollisionController();

        /// <summary>
        /// При взятии в руку.
        /// </summary>
        public void OnGrabStart()
        {
            CollisionController.enabled = true;
        }

        /// <summary>
        /// При отпускании.
        /// </summary>
        public void OnGrabEnd()
        {
            if (_collisionController)
            {
                CollisionController.Destroy();
            }
        }

        /// <summary>
        /// Получение контроллера коллизий.
        /// </summary>
        /// <returns>Обработчик коллизий.</returns>
        public CollisionController GetCollisionController()
        {
            if (!_collisionController)
            {
                _collisionController = GetComponentsInParent<CollisionController>().FirstOrDefault(a => !a.IsDestroying);
            }

            var gameObjectDestroying = gameObject.GetWrapper()?.GetObjectController()?.IsDeleted ?? true;
            if (gameObjectDestroying && _collisionController)
            {
                return _collisionController;
            }
            
            if (!_collisionController || (_collisionController && _collisionController.IsDestroying))
            {
                _collisionController = gameObject.GetWrapper()?.GetGameObject()?.AddComponent<CollisionController>() ?? _collisionController;
                
                var inputController = MainSocketController.gameObject.GetRootInputController();
                _collisionController?.InitializeController(inputController);
                _collisionController?.SubscribeToJointControllerEvents(this);
            }

            return _collisionController;
        }

        /// <summary>
        /// При соприкосновении CollisionController'a.
        /// </summary>
        /// <param name="other">Другой коллайдер.</param>
        public override void CollisionEnter(Collider other)
        {
            if (MainSocketController.ConnectionGraphBehaviour.HasChild(other.GetComponentInParent<SocketController>()))
            {
                return;
            }
            
            MainSocketController.ConnectionGraphBehaviour.ForEach(a => a.CollisionProvider.OnCollisionEnterInvoke(other));
        }

        /// <summary>
        /// При выходе из коллизии CollisionController'a.
        /// </summary>
        /// <param name="other">Другой коллайдер.</param>
        public override void CollisionExit(Collider other)
        {
            if (MainSocketController.ConnectionGraphBehaviour.HasChild(other.GetComponentInParent<SocketController>()))
            {
                return;
            }

            MainSocketController.ConnectionGraphBehaviour.ForEach(a => a.CollisionProvider.OnCollisionExitInvoke(other));
        }

        /// <summary>
        /// Разрешить отпустить из руки.
        /// </summary>
        public void CheckCanDrop()
        {
            if (!_collisionController || _collisionController.InputController == null || !_collisionController.IsBlocked() || !MainSocketController.IsLocalGrabbed)
            {
                return;
            }

            if (!_collisionController.InputController.IsDropEnabled() && ProjectData.PlatformMode == PlatformMode.Vr)
            {
                _collisionController.InputController.EnableDrop();
            }
            else
            {
                _collisionController.ForcedUnblock();
            }
        }
    }
}