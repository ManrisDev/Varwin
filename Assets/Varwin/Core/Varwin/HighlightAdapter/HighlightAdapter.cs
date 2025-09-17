using UnityEngine;

namespace Varwin
{
    public abstract class HighlightAdapter
    {
        public static HighlightAdapter Instance { get; private set; }

        public static void Init(HighlightAdapter newHighlightAdapter)
        {
            Instance = newHighlightAdapter;
        }
        
        public DefaultHighlightConfigs Configs { get; }
        
        protected HighlightAdapter(DefaultHighlightConfigs defaultHighlightConfigs)
        {
            Configs = defaultHighlightConfigs;
        }

        public abstract Highlighter AddHighlighter(GameObject gameObject);
    }
}
