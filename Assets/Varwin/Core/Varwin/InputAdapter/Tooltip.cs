using UnityEngine;

namespace Varwin.PlatformAdapter
{
    public abstract class Tooltip
    {
        public abstract Transform drawLineTo { set; }
        public abstract string displayText { set; }
        public abstract void ResetTooltip();
    }
}