using UnityEngine;
using Varwin.NettleDesk;
using Varwin.PlatformAdapter;

namespace Varwin.NettleDeskPlayer
{
    /// <summary>
    /// Компонент переподключения ссылок на стилус в случае, когда стилус доступен.
    /// </summary>
    public class NettleDeskControllerInteractionComponentStylus : NettleDeskControllerInteractionComponentBase
    {
        /// <summary>
        /// Стилус.
        /// </summary>
        public NettleDeskStylus Stylus;

        /// <summary>
        /// Подписка на событие изменения настроек.
        /// </summary>
        private void Awake()
        {
            SetControllerPreference();

            NettleDeskSettings.SettingsChanged += SetControllerPreference;
        }

        /// <summary>
        /// Если стилус доступен, то указывается на стилус.
        /// </summary>
        private void SetControllerPreference()
        {
            if (!NettleDeskSettings.StylusSupport)
            {
                return;
            }
            
            if (Stylus.IsLeft)
            {
                InputAdapter.Instance.PlayerController.Nodes.LeftHand.SetNode(gameObject);
            }
            else
            {
                InputAdapter.Instance.PlayerController.Nodes.RightHand.SetNode(gameObject);
            }
        }

        /// <summary>
        /// Выпустить объект из руки.
        /// </summary>
        /// <param name="force">Истина, если принудительно.</param>
        public override void DropGrabbedObject(bool force = false)
        {
            if (Stylus.ForcedGrab && force || !Stylus.ForcedGrab)
            {
                Stylus.ForceDrop();
            }
        }

        /// <summary>
        /// Принудительно взять объект в руку.
        /// </summary>
        /// <param name="targetObject">Целевой объект.</param>
        public override void ForceGrabObject(GameObject targetObject)
        {
            Stylus.ForceGrab(targetObject);
        }

        /// <summary>
        /// Принудительный выпуск объекта из руки.
        /// </summary>
        /// <param name="targetObject">Целевой объект.</param>
        public override void ForceDropObject(GameObject targetObject)
        {
            Stylus.ForceDrop();
        }

        /// <summary>
        /// Получить объект взятый в руку.
        /// </summary>
        /// <returns>Объект, взятый в руку.</returns>
        public override GameObject GetGrabbedObject()
        {
            return Stylus.GrabbedObject ? Stylus.GrabbedObject.gameObject : null;
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