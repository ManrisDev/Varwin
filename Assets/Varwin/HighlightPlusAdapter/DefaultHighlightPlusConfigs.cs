using UnityEngine;
namespace Varwin
{
    public class DefaultHighlightPlusConfigs : DefaultHighlightConfigs
    {
        public override IHighlightConfig EditorSelected { get; } = new HighlightPlusConfig
        {
            OutlineColor = Color.cyan,
            OverlayColor = Color.clear
        };
        
        public override IHighlightConfig EditorHovered { get; } = new HighlightPlusConfig
        {
            OutlineColor = Color.yellow,
            OverlayColor = Color.clear
        };
        
        public override IHighlightConfig CollisionHighlight { get; } = new HighlightPlusConfig
        {
            OutlineColor = Color.red,
            OverlayColor = Color.red
        };

        public override IHighlightConfig JointHighlight { get; } = new HighlightPlusConfig
        {
            OutlineColor = Color.green,
            OverlayColor = Color.green
        };
        
        public override IHighlightConfig TouchHighlight { get; } = new HighlightPlusConfig
        {
            OutlineColor = Color.cyan,
            OverlayColor = Color.clear
        };

        public override IHighlightConfig UseHighlight { get; } = new HighlightPlusConfig
        {
            OutlineColor = Color.yellow,
            OverlayColor = Color.black
        };
        
        public override IHighlightConfig ControllerTooltip { get; } = new HighlightPlusConfig
        {
            OutlineColor = Color.yellow,
            OverlayColor = Color.clear
        };
        
        public override IHighlightConfig MeshPreview { get; } = new HighlightPlusConfig
        {
            OutlineColor = Color.white,
            OverlayColor = Color.white
        };
    }
}
