using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Varwin.XR
{
    /// <summary>
    /// Контейнер списка доступных контроллеров.
    /// </summary>
    [CreateAssetMenu(fileName = "Controller models", menuName = "Controller models", order = 0)]
    public class ControllerModels : ScriptableObject
    {
        /// <summary>
        /// Список доступных контроллеров.
        /// </summary>
        [SerializeField] private List<ControllerInfoContainer> _сontrollers;
        
        /// <summary>
        /// В случае если нет доступного контроллера, метод получения будет возвращать Fallback.
        /// </summary>
        [SerializeField] private ControllerInfoContainer _fallbackControllerModel;

        /// <summary>
        /// Метод для получения доступных имен пресетов.
        /// </summary>
        /// <returns>Список доступных пресетов.</returns>
        public IEnumerable<string> GetAvailableHeadsetNames()
        {
            return _сontrollers.Select(a => a.Name);
        }

        /// <summary>
        /// Метод получения контроллера по названию.
        /// </summary>
        /// <param name="inputDeviceName">Название контроллера.</param>
        /// <param name="headsetName">Название шлема.</param>
        /// <param name="leftController">Истина, если нужен левый контроллер.</param>
        /// <param name="presetName">Имя пресета в таблице.</param>
        /// <returns>Префаб необходимого контроллера, если найден.</returns>
        public GameObject GetControllerPrefab(string inputDeviceName, string headsetName, ref string presetName, bool leftController = false)
        {
            if (_сontrollers == null || _сontrollers.Count == 0)
            {
                return null;
            }

            var result = _сontrollers.FindAll(a => a.ContainsControllerName(inputDeviceName));
            if (result.Count == 0)
            {
                presetName = _fallbackControllerModel.Name;
                return leftController ? _fallbackControllerModel.LeftPrefab : _fallbackControllerModel.RightPrefab;
            }

            var controllerInfoContainer = result.Find(a => a.ContainsHeadsetName(headsetName));
            if (result.Count == 1 || controllerInfoContainer == null)
            {
                presetName = result[0].Name;
                return leftController ? result[0].LeftPrefab : result[0].RightPrefab;
            }

            presetName = controllerInfoContainer.Name;
            return leftController ? controllerInfoContainer.LeftPrefab : controllerInfoContainer.RightPrefab;
        }

        /// <summary>
        /// Метод получения контроллера по названию шлема.
        /// </summary>
        /// <param name="headsetName">Имя шлема.</param>
        /// <param name="leftController">Левый ли контроллер.</param>
        /// <returns>Префаб необходимого контроллера, если найден.</returns>
        public GameObject GetControllerPrefab(string headsetName, bool leftController = false)
        {
            if (_сontrollers == null || _сontrollers.Count == 0)
            {
                return null;
            }

            var preset = _сontrollers.Find(a => a.Name == headsetName);
            if (preset == null)
            {
                return null;
            }
            
            return leftController ? preset.LeftPrefab : preset.RightPrefab;
        }

        /// <summary>
        /// Является ли шлем 3Dof.
        /// </summary>
        /// <param name="headsetName">Имя шлема.</param>
        /// <returns>Истина, если является.</returns>
        public bool Is3Dof(string headsetName)
        {
            var controller = GetControllerPrefab(headsetName);
            return controller && controller.GetComponent<VarwinXRControllerModel>().Is3Dof;
        }
    }
}