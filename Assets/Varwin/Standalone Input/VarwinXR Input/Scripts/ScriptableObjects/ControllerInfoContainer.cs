using System;
using System.Linq;
using UnityEngine;

namespace Varwin.XR
{
    /// <summary>
    /// Контейнер информации о контроллере.
    /// </summary>
    [Serializable]
    public class ControllerInfoContainer
    {
        /// <summary>
        /// Имя модели контроллера.
        /// </summary>
        public string Name;

        /// <summary>
        /// Имена контроллеров, которым подходит эта модель.
        /// </summary>
        public string[] AssignedControllerNames;
        
        /// <summary>
        /// Имена шлемов этого контроллера.
        /// </summary>
        public string[] HeadsetNames;
        
        /// <summary>
        /// Префаб левого контроллера
        /// </summary>
        public GameObject LeftPrefab;
        
        /// <summary>
        /// Префаб правого контроллера.
        /// </summary>
        public GameObject RightPrefab;

        /// <summary>
        /// При поиске использовать подстроку.
        /// </summary>
        public bool UseSubstring = false;

        /// <summary>
        /// Поиск по типу контроллера.
        /// </summary>
        /// <returns>Истина если такое имя есть.</returns>
        public bool ContainsControllerName(string controllerName)
        {
            return UseSubstring ? AssignedControllerNames.Any(a => controllerName.Contains(a)) : AssignedControllerNames.Any(a => a.Equals(controllerName));
        }
        
        /// <summary>
        /// Поиск по имени шлема.
        /// </summary>
        /// <returns>Истина если такое имя есть.</returns>
        public bool ContainsHeadsetName(string headsetName)
        {
            return UseSubstring ? HeadsetNames.Any(a => headsetName.Contains(a)) : HeadsetNames.Any(a => a.Equals(headsetName));
        }

    }
}