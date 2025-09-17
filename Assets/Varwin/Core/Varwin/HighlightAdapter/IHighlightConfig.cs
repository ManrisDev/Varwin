using UnityEngine;

namespace Varwin
{
    public interface IHighlightConfig
    {
        public Color OutlineColor { get; set; }
        public Color OverlayColor { get; set; }

        public float OutlineWidth { get; set; }
    }
}
