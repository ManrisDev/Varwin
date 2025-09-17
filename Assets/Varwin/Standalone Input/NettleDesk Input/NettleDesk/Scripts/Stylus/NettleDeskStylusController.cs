using Illumetry.Unity.Stylus;
using UnityEngine;

namespace Varwin.NettleDeskPlayer
{
    /// <summary>
    /// Контроллер инициализации стилусов.
    /// </summary>
    public class NettleDeskStylusController : MonoBehaviour
    {
        /// <summary>
        /// Левый стилус.
        /// </summary>
        public NettleDeskStylus LeftStylus;
        
        /// <summary>
        /// Правый стилус.
        /// </summary>
        public NettleDeskStylus RightStylus;

        /// <summary>
        /// Подписка на контроллер стилусов, а также на настройки. 
        /// </summary>
        private void Awake()
        {
            StylusesCreator.OnCreatedStylus += OnStylusInitialized;
            NettleDeskSettings.SettingsChanged += OnSettingsChanged;
        }

        /// <summary>
        /// В случае отключении поддержки стилусов происходит деинициализация стилусов.
        /// </summary>
        private void OnSettingsChanged()
        {
            if (!NettleDeskSettings.StylusSupport)
            {
                LeftStylus.InputHandler.Deinitialize();
                RightStylus.InputHandler.Deinitialize();
            }
            else
            {
                if (StylusesCreator.Styluses.Length == 0)
                {
                    return;
                }
                
                RightStylus.InputHandler.Initialize(StylusesCreator.Styluses[0]);

                if (StylusesCreator.Styluses.Length > 1)
                {
                    LeftStylus.InputHandler.Initialize(StylusesCreator.Styluses[1]);
                }
            }
        }

        /// <summary>
        /// Метод, вызываемый при деинициализации стилуса.
        /// </summary>
        /// <param name="targetStylus">Целевой стилус.</param>
        private void OnStylusInitialized(Stylus targetStylus)
        {
            if (!NettleDeskSettings.StylusSupport)
            {
                return;
            }
            
            if (!RightStylus.Initialized)
            {
                RightStylus.InputHandler.Initialize(targetStylus);
            }
            else if (RightStylus.Initialized && !LeftStylus.Initialized)
            {
                LeftStylus.InputHandler.Initialize(targetStylus);
            }
        }

        /// <summary>
        /// Отписка.
        /// </summary>
        private void OnDestroy()
        {
            StylusesCreator.OnCreatedStylus -= OnStylusInitialized;
            NettleDeskSettings.SettingsChanged -= OnSettingsChanged;
        }
    }
}