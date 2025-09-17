using System;

namespace Varwin.XR
{
    /// <summary>
    /// Описание фичи.
    /// </summary>
    [Serializable]
    public class FeatureUsageDescription
    {
        /// <summary>
        /// Тип фичи.
        /// </summary>
        public FeatureUsageKey Key;
        
        /// <summary>
        /// Наименование фичи.
        /// </summary>
        public string FeatureName;
    }
}