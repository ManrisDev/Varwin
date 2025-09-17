using System.Collections.Generic;
using HighlightPlus;
using UnityEngine;

namespace Varwin
{
    public class HighlightPlusEffect : Highlighter
    {

        public override bool IsEnabled
        {
            get => Effect.enabled;
            set
            {
                if (Effect)
                {
                    Effect.enabled = value;
                    Effect.highlighted = value;
                }
            }
        }

        private HighlightPlusConfig _config;
        public override IHighlightConfig Config
        {
            get => _config;
            set
            {
                _config = (HighlightPlusConfig)value;
                SetupWithConfig();
            }
        }

        private HighlightEffect _effect;

        public HighlightEffect Effect
        {
            get
            {
                if (!_effect && gameObject)
                {
                    _effect = gameObject.GetComponent<HighlightEffect>();

                    if (!_effect)
                    {
                        _effect = gameObject.AddComponent<HighlightEffect>();
                    }
                }

                return _effect;
            }
            set
            {
                if (value)
                {
                    _effect = value;
                }
            }
        }

        private void Awake()
        {
            SetupWithConfig();
            IsEnabled = false;
        }

        private void OnDestroy()
        {
            if (Effect)
            {
                Destroy(Effect);
            }
        }

        public override void SetConfig(IHighlightConfig config, IHighlightComponent newEffect = null, bool needsRefresh = true)
        {
            _config = (HighlightPlusConfig) config;
            Effect = (HighlightEffect) newEffect;
            SetupWithConfig();
        }

        public override void SetIgnoredRenderers(IEnumerable<Renderer> renderers)
        {
            Effect.SetIgnoredRenderers(renderers);
        }

        private void SetupWithConfig()
        {
            if (_config == null)
            {
                return;
            }

            Effect.outlineQuality = _config.OutlineQualityLevel;
            Effect.glowQuality = _config.GlowQualityLevel;
            
            Effect.outlineColor = _config.OutlineColor;
            Effect.outlineWidth = _config.OutlineWidth;
            
            Effect.overlayColor = _config.OverlayColor;

            Effect.glowWidth = _config.GlowWidth;
        }
    }
}
