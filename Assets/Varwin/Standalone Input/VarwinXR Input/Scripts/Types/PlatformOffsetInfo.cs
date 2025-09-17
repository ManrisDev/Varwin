using System;
using UnityEngine;

namespace Varwin.XR.Types
{
    /// <summary>
    /// Информация о сдвиге контроллера для заданной платформы.
    /// </summary>
    [Serializable]
    public class PlatformOffsetInfo
    {
        /// <summary>
        /// Имя платформы.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Угол сдвига относительно якоря.
        /// </summary>
        public Vector3 AngleOffset;
        
        /// <summary>
        /// Якорная точка.
        /// </summary>
        public Transform Anchor;

        /// <summary>
        /// Является ли сдвиг исключительно влияющий в мобильном шлеме.
        /// </summary>
        public bool IsDesktopOnly = false;
    }
}