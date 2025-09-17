namespace Varwin
{
    public abstract class DefaultHighlightConfigs
    {
        public abstract IHighlightConfig EditorSelected { get; }
        public abstract IHighlightConfig EditorHovered { get; }
        public abstract IHighlightConfig CollisionHighlight { get; }
        public abstract IHighlightConfig JointHighlight { get; }
        public abstract IHighlightConfig TouchHighlight { get; }
        public abstract IHighlightConfig UseHighlight { get; }
        public abstract IHighlightConfig ControllerTooltip { get; }
        public abstract IHighlightConfig MeshPreview { get; }
    }
}
