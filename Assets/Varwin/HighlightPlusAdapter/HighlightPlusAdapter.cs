using UnityEngine;
namespace Varwin
{
    public class HighlightPlusAdapter : HighlightAdapter
    {
        public HighlightPlusAdapter() : base(new DefaultHighlightPlusConfigs())
        {}
        
        public override Highlighter AddHighlighter(GameObject gameObject)
        {
            return gameObject.AddComponent<HighlightPlusEffect>();
        }
    }
}
