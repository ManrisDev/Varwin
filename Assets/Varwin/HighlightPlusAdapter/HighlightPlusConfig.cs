using System;
using UnityEngine;
using QualityLevel = HighlightPlus.QualityLevel;
namespace Varwin
{
    [Serializable]
    public class HighlightPlusConfig : IHighlightConfig
    {
        public QualityLevel OutlineQualityLevel { get; set; } = QualityLevel.Fastest;
        public QualityLevel GlowQualityLevel { get; set; } = QualityLevel.Fastest;
        public Color OutlineColor { get; set; } = Color.cyan;
        public float OutlineWidth { get; set; } = 0.1f;
        public Color OverlayColor { get; set; } = Color.cyan;

        public float GlowWidth { get; set; } = 0.1f;
    }
}
