using UnityEngine;
using Varwin.Public;

namespace Varwin.SocketLibrary
{
    /// <summary>
    /// Класс, описывающий логику розетки.
    /// </summary>
    public class SocketPoint : JointPoint
    {
        /// <summary>
        /// Дополнительное пользовательское превью. 
        /// </summary>
        public GameObject ChainPreview { get; internal set; }
        
        /// <summary>
        /// Список допустимых ключей.
        /// </summary>
        public string[] AvailableKeys = new[] {"key"};
        
        /// <summary>
        /// Инициализация точки.
        /// </summary>
        private void Awake()
        {
            SocketController?.AddJointPoint(this);

            if (!InstancedSocketPoints.Contains(this))
            {
                InstancedSocketPoints.Add(this);
            }
            
            gameObject.layer = 2;
        }

        /// <summary>
        /// Событие при удалении.
        /// </summary>
        private void OnDestroy()
        {
            if (InstancedSocketPoints.Contains(this))
            {
                InstancedSocketPoints.Remove(this);
            }
            
            SocketController?.RemoveJointPoint(this);
            Destroy(ChainPreview);
        }
    }
}