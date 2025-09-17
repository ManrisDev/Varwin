using System.Collections.Generic;
using UnityEngine;

namespace Varwin.XR
{
    /// <summary>
    /// Механизм карты фичи.
    /// </summary>
    public class FeatureUsageMap : MonoBehaviour
    {
        /// <summary>
        /// Описание фич.
        /// </summary>
        [SerializeField] private List<FeatureUsageDescription> _map;

        /// <summary>
        /// Получить имя фичи.
        /// </summary>
        /// <param name="key">Ключ фичи.</param>
        /// <returns>Наименование фичи для InputDeviceFeature.</returns>
        public string GetFeatureName(FeatureUsageKey key)
        {
            if (_map == null)
            {
                return GetDefaultValue(key);
            }

            var finded = _map.Find(a => a.Key == key);
            if (finded == null)
            {
                return GetDefaultValue(key);
            }

            return finded.FeatureName;
        }
        
        /// <summary>
        /// Получить стандартное значение фичи.
        /// </summary>
        /// <param name="key">Ключ фичи.</param>
        /// <returns>Наименование фичи для InputDeviceFeature.</returns>
        public static string GetDefaultValue(FeatureUsageKey key)
        {
            switch (key)
            {
                case FeatureUsageKey.PrimaryAxis2D: return "Primary2DAxis";
                case FeatureUsageKey.PrimaryAxis2DClick: return "Primary2DAxisClick";
                case FeatureUsageKey.SecondaryAxis2D:return "Secondary2DAxis";
                case FeatureUsageKey.SecondaryAxis2DClick: return "Secondary2DAxisClick";
                case FeatureUsageKey.ButtonOne: return "PrimaryButton";
                case FeatureUsageKey.ButtonTwo: return "SecondaryButton";
                case FeatureUsageKey.MenuButton: return "MenuButton";
                case FeatureUsageKey.Grip: return "Grip";
                case FeatureUsageKey.Trigger: return "Trigger";
            }

            return null;
        }
    }
}