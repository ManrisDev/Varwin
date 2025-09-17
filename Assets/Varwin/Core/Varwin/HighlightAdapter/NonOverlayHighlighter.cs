using UnityEngine;

namespace Varwin
{
    public class NonOverlayHighlighter : MonoBehaviour, IHighlightAware
    {
        public IHighlightConfig HighlightConfig()
        {
            return HighlightAdapter.Instance.Configs.TouchHighlight;
        }
    }
}