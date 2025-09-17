using System;
using System.Collections.Generic;
using UnityEngine;

namespace Varwin
{
    public abstract class Highlighter : MonoBehaviour
    {
        public bool IsSelected { get; set; }
        public abstract bool IsEnabled { get; set; }
        public abstract IHighlightConfig Config  { get; set; }
        
        public abstract void SetConfig(IHighlightConfig config, IHighlightComponent newEffect = null, bool needsRefresh = true);

        public virtual void SetIgnoredRenderers(IEnumerable<Renderer> renderers)
        {
            throw new NotImplementedException();
        }

        private void OnDisable()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            
            IsEnabled = false;
        }
    }
}
