using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin;

namespace HighlightPlus
{
    public class HighlightEffect : MonoBehaviour, IHighlightComponent
    {
        private bool _highlighted;
        private SkinnedMeshRenderer[] highlightSkinnedRenderers;
        private SkinnedMeshRenderer[] existingSkinnedRenderers;
        private MeshRenderer[] highlightRenderers;
        private MeshRenderer[] existingRenderers;
        private static Material highlightMat;
        private GameObject highlightHolder;
        private static readonly int OutlineColorProperty = Shader.PropertyToID("g_vOutlineColor");
        [Tooltip("An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.")]
        public GameObject[] hideHighlight;
        public bool outlineAlwaysOnTop;
        public bool ignoreObjectVisibility;
        
        public bool highlighted
        {
            get => _highlighted;
            set => ToggleHighlight(value);
        }

        private void ToggleHighlight(bool value)
        {
            _highlighted = value;

            if (value)
            {
                CreateHighlightRenderers();
                UpdateHighlightRenderers();
            }

            else
            {
                Destroy(highlightHolder);
            }
           
        }
        
        protected virtual bool ShouldIgnoreHighlight(Component component) => ShouldIgnore(component.gameObject);

        protected virtual bool ShouldIgnore(GameObject check)
        {
            return hideHighlight != null && hideHighlight.Any(t => check == t);
        }

        protected virtual void CreateHighlightRenderers()
        {
            existingSkinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);

            if (highlightHolder)
            {
                Destroy(highlightHolder);
            }
            
            highlightHolder = new GameObject("Highlighter");
            highlightSkinnedRenderers = new SkinnedMeshRenderer[existingSkinnedRenderers.Length];

            for (var skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
            {
                SkinnedMeshRenderer existingSkinned = existingSkinnedRenderers[skinnedIndex];

                if (ShouldIgnoreHighlight(existingSkinned))
                {
                    continue;
                }

                var newSkinnedHolder = new GameObject("SkinnedHolder");
                newSkinnedHolder.transform.parent = highlightHolder.transform;
                var newSkinned = newSkinnedHolder.AddComponent<SkinnedMeshRenderer>();
                var materials = new Material[existingSkinned.sharedMaterials.Length];
                for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = highlightMat;
                }

                newSkinned.sharedMaterials = materials;
                newSkinned.sharedMesh = existingSkinned.sharedMesh;
                newSkinned.rootBone = existingSkinned.rootBone;
                newSkinned.updateWhenOffscreen = existingSkinned.updateWhenOffscreen;
                newSkinned.bones = existingSkinned.bones;

                highlightSkinnedRenderers[skinnedIndex] = newSkinned;
            }

            var existingFilters = GetComponentsInChildren<MeshFilter>(true);
            existingRenderers = new MeshRenderer[existingFilters.Length];
            highlightRenderers = new MeshRenderer[existingFilters.Length];

            for (var filterIndex = 0; filterIndex < existingFilters.Length; filterIndex++)
            {
                MeshFilter existingFilter = existingFilters[filterIndex];
                var existingRenderer = existingFilter.GetComponent<MeshRenderer>();

                if (!existingFilter || !existingRenderer || ShouldIgnoreHighlight(existingFilter))
                {
                    continue;
                }

                var newFilterHolder = new GameObject("FilterHolder");
                newFilterHolder.transform.parent = highlightHolder.transform;
                var newFilter = newFilterHolder.AddComponent<MeshFilter>();
                newFilter.sharedMesh = existingFilter.sharedMesh;
                var newRenderer = newFilterHolder.AddComponent<MeshRenderer>();

                var materials = new Material[existingRenderer.sharedMaterials.Length];
                for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = highlightMat;
                }

                newRenderer.sharedMaterials = materials;

                highlightRenderers[filterIndex] = newRenderer;
                existingRenderers[filterIndex] = existingRenderer;
            }

        }
        
        protected virtual void UpdateHighlightRenderers()
        {
            if (!highlightHolder)
            {
                return;
            }

            for (var skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
            {
                SkinnedMeshRenderer existingSkinned = existingSkinnedRenderers[skinnedIndex];
                SkinnedMeshRenderer highlightSkinned = highlightSkinnedRenderers[skinnedIndex];
                bool isIgnored = _ignoredRenderers.Contains(existingSkinned) || _ignoredRenderers.Contains(highlightSkinned);

                if (existingSkinned && highlightSkinned && !isIgnored)
                {
                    highlightSkinned.transform.position = existingSkinned.transform.position;
                    highlightSkinned.transform.rotation = existingSkinned.transform.rotation;
                    highlightSkinned.transform.localScale = existingSkinned.transform.lossyScale;
                    highlightSkinned.localBounds = existingSkinned.localBounds;
                    highlightSkinned.enabled = existingSkinned.enabled && existingSkinned.gameObject.activeInHierarchy;

                    int blendShapeCount = existingSkinned.sharedMesh.blendShapeCount;
                    for (var blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
                    {
                        highlightSkinned.SetBlendShapeWeight(blendShapeIndex, existingSkinned.GetBlendShapeWeight(blendShapeIndex));
                    }
                }
                else if (highlightSkinned)
                {
                    highlightSkinned.enabled = false;
                }
            }

            for (var rendererIndex = 0; rendererIndex < highlightRenderers.Length; rendererIndex++)
            {
                MeshRenderer existingRenderer = existingRenderers[rendererIndex];
                MeshRenderer highlightRenderer = highlightRenderers[rendererIndex];
                bool isIgnored = _ignoredRenderers.Contains(existingRenderer) || _ignoredRenderers.Contains(highlightRenderer);

                if (existingRenderer && highlightRenderer && !isIgnored)
                {
                    highlightRenderer.transform.position = existingRenderer.transform.position;
                    highlightRenderer.transform.rotation = existingRenderer.transform.rotation;
                    highlightRenderer.transform.localScale = existingRenderer.transform.lossyScale;
                    highlightRenderer.enabled = existingRenderer.enabled && existingRenderer.gameObject.activeInHierarchy;
                }
                else if (highlightRenderer)
                {
                    highlightRenderer.enabled = false;
                }
            }
        }

        private Color _outLineColor;

        public int outline;
        public float outlineWidth;

        public Color outlineColor
        {
            get => _outLineColor;
            set => SetMaterialColor(value);
        }

        private void SetMaterialColor(Color value)
        {
            _outLineColor = value;

            if (!highlightMat)
            {
                highlightMat = (Material) Resources.Load("Highlight", typeof(Material));
            }
            
            if (highlightMat)
            {
                highlightMat.SetColor(OutlineColorProperty, value);
            }
        }

        public int glow;
        public float glowWidth;
        public Color glowHQColor;
        public GlowPassData[] glowPasses;
        public bool glowDithering;
        public int glowAnimationSpeed;
        public SeeThroughMode seeThrough;
        public float seeThroughIntensity;
        public float seeThroughTintAlpha;
        public Color seeThroughTintColor;
        public Color overlayColor;
        public float overlayMinIntensity;
        public float overlay;
        public float overlayAnimationSpeed;
        public QualityLevel outlineQuality;
        public Visibility outlineVisibility = Visibility.Normal;
        public QualityLevel glowQuality;
        public int outlineDownsampling;
        private HashSet<Renderer> _ignoredRenderers = new();

        public void Refresh()
        {
             
        }

        private void Start()
        {
            highlightMat = (Material) Resources.Load("SteamVR_HoverHighlight", typeof(Material));
            hideHighlight = new GameObject[] { };
        }

        private void LateUpdate()
        {
            if (_highlighted)
            {
                UpdateHighlightRenderers();
            }
        }

        private void OnDestroy()
        {
            if (highlightHolder != null)
            {
                Destroy(highlightHolder);
            }
        }
        
        private void OnDisable()
        {
            if (highlightHolder != null)
            {
                Destroy(highlightHolder);
            }
        }

        public Color OutlineColor
        {
            get => _outLineColor;
            set => _outLineColor = value;
        }

        public void SetIgnoredRenderers(IEnumerable<Renderer> renderers)
        {
            _ignoredRenderers = renderers.ToHashSet();
        }
    }

    public class GlowPassData
    {
        public float alpha;
        public float offset;
        public Color color;
    }

    public enum QualityLevel
    {
        Fastest,
        High,
        Highest
    }

    public enum SeeThroughMode
    {
        WhenHighlighted,
        Never
    }

    public enum Visibility 
    {
        Normal,
        AlwaysOnTop,
        OnlyWhenOccluded
    }
}