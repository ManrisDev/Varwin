using UnityEngine;
using UnityEngine.UI;

namespace Varwin
{
    public static class UiUtils
    {
        public static void RebuildLayouts(Transform root, bool immediate = true)
        {
            RebuildLayouts(root.gameObject, immediate);
        }
        public static void RebuildLayouts(Component root, bool immediate = true)
        {
            RebuildLayouts(root.gameObject, immediate);
        }
        
        public static void RebuildLayouts(GameObject root, bool immediate = true)
        {
            var layouts = root.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>();
            foreach (var layout in layouts)
            {
                if (immediate)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) layout.transform);
                }
                else
                {
                    LayoutRebuilder.MarkLayoutForRebuild((RectTransform) layout.transform);
                }
            }
        }
    }
}