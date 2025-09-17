using UnityEngine;
using Varwin.NettleDeskPlayer;
using Varwin.PlatformAdapter;

namespace Varwin.NettleDesk
{
    /// <summary>
    /// Компонент взаимодействия при управлении глазами (режим DP).
    /// </summary>
    public class NettleDeskControllerInteractionComponentEye : NettleDeskControllerInteractionComponentBase
    {
        /// <summary>
        /// Объект игрока.
        /// </summary>
        private NettleDeskInteractionController _playerInteractionController;
        
        /// <summary>
        /// Подписка на событие изменения настроек.
        /// </summary>
        private void Awake()
        {
            _playerInteractionController = GetComponentInParent<NettleDeskInteractionController>();
            NettleDeskSettings.SettingsChanged += SetControllerPreference;
            SetControllerPreference();
        }

        /// <summary>
        /// Указание о том, что этот класс является рукой в случае, когда стилус недоступен.
        /// </summary>
        private void SetControllerPreference()
        {
            if (NettleDeskSettings.StylusSupport)
            {
                return;
            }
            
            InputAdapter.Instance.PlayerController.Nodes.LeftHand.SetNode(gameObject);
            InputAdapter.Instance.PlayerController.Nodes.RightHand.SetNode(gameObject);
        }

        /// <summary>
        /// Выбросить объект из руки.
        /// </summary>
        /// <param name="force">Истина, если принудительно.</param>
        public override void DropGrabbedObject(bool force = false)
        {
            _playerInteractionController.DropGrabbedObject();
        }

        /// <summary>
        /// Принудительно взять объект в руку.
        /// </summary>
        /// <param name="targetObject">Целевой объект.</param>
        public override void ForceGrabObject(GameObject targetObject)
        {
            _playerInteractionController.ForceGrabObject(targetObject);
        }

        /// <summary>
        /// Принудительно выбросить объект из руки.
        /// </summary>
        /// <param name="targetObject">Целевой объект.</param>
        public override void ForceDropObject(GameObject targetObject)
        {
            _playerInteractionController.ForceDropObject(targetObject);
        }

        /// <summary>
        /// Получить объект, находящийся в руке.
        /// </summary>
        /// <returns>Объект, находящийся в руке.</returns>
        public override GameObject GetGrabbedObject()
        {
            return _playerInteractionController.GetGrabbedObject();
        }

        /// <summary>
        /// Отписка.
        /// </summary>
        private void OnDestroy()
        {
            NettleDeskSettings.SettingsChanged -= SetControllerPreference;
        }
    }
}