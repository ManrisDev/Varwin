using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Varwin.Core
{
    public class VarwinStandardEditor : ShaderGUI
    {
        private enum RenderingMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }

        private MaterialProperty _blendMode = null;
        private MaterialProperty _albedoMap = null;
        private MaterialProperty _albedoColor = null;
        private MaterialProperty _alphaCutoff = null;
        private MaterialProperty _metallicMap = null;
        private MaterialProperty _metallic = null;
        private MaterialProperty _smoothness = null;
        private MaterialProperty _highlights = null;
        private MaterialProperty _bumpScale = null;
        private MaterialProperty _bumpMap = null;
        private MaterialProperty _occlusionStrength = null;
        private MaterialProperty _occlusionMap = null;
        private MaterialProperty _heigtMapScale = null;
        private MaterialProperty _heightMap = null;
        private MaterialProperty _emissionColorForRendering = null;
        private MaterialProperty _emissionMap = null;
        private MaterialProperty _detailMask = null;
        private MaterialProperty _detailAlbedoMap = null;
        private MaterialProperty _detailNormalMapScale = null;
        private MaterialProperty _detailNormalMap = null;
        private MaterialProperty _uvSetSecondary = null;

        private MaterialEditor _materialEditor;
        private string _previousShaderName = "";

        public void FindProperties(MaterialProperty[] props)
        {
            _blendMode = FindProperty("_Mode", props);
            _albedoMap = FindProperty("_MainTex", props);
            _albedoColor = FindProperty("_Color", props);
            _alphaCutoff = FindProperty("_Cutoff", props);
            _metallicMap = FindProperty("_MetallicGlossMap", props, false);
            _metallic = FindProperty("_Metallic", props, false);
            _smoothness = FindProperty("_Glossiness", props);
            _highlights = FindProperty("_SpecularHighlights", props, false);
            _bumpScale = FindProperty("_BumpScale", props);
            _bumpMap = FindProperty("_BumpMap", props);
            _occlusionStrength = FindProperty("_OcclusionStrength", props);
            _occlusionMap = FindProperty("_OcclusionMap", props);
            _emissionColorForRendering = FindProperty("_EmissionColor", props);
            _emissionMap = FindProperty("_EmissionMap", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);
            SetBackendValues();

            _materialEditor = materialEditor;
            var material = (materialEditor.target as Material);
            var renderingModeChanged = BlendModePopup();
            EditorGUILayout.LabelField(VarwinShaderStyles.primaryMapsText, EditorStyles.boldLabel);

            materialEditor.TexturePropertySingleLine(VarwinShaderStyles.albedoText, _albedoMap, _albedoColor);
            if ((RenderingMode) _blendMode.floatValue == RenderingMode.Cutout)
            {
                materialEditor.ShaderProperty(_alphaCutoff, VarwinShaderStyles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
            }

            materialEditor.TexturePropertySingleLine(VarwinShaderStyles.metallicMapText, _metallicMap, _metallicMap.textureValue ? null : _metallic);
            materialEditor.ShaderProperty(_smoothness, VarwinShaderStyles.smoothnessText, 2);
            materialEditor.TexturePropertySingleLine(VarwinShaderStyles.normalMapText, _bumpMap, _bumpMap.textureValue ? _bumpScale : null);
            materialEditor.TexturePropertySingleLine(VarwinShaderStyles.occlusionText, _occlusionMap, _occlusionMap.textureValue ? _occlusionStrength : null);
            materialEditor.TextureScaleOffsetProperty(_albedoMap);
            if (materialEditor.EmissionEnabledProperty())
            {
                material.EnableKeyword("_EMISSION");
                materialEditor.TexturePropertySingleLine(VarwinShaderStyles.emissionText, _emissionMap, _emissionColorForRendering);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }

            EditorGUILayout.LabelField(VarwinShaderStyles.forwardText, EditorStyles.boldLabel);
            var highlights = EditorGUILayout.Toggle(VarwinShaderStyles.highlightsText, !material.IsKeywordEnabled("_SPECULARHIGHLIGHTS_OFF"));
            if (!highlights)
            {
                material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            }
            else
            {
                material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
            }

            EditorGUILayout.LabelField(VarwinShaderStyles.advancedText, EditorStyles.boldLabel);

            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();

            var forceRebuildShaderParams = material.shader.name != _previousShaderName;
            _previousShaderName = material.shader.name;
            
            if (forceRebuildShaderParams || renderingModeChanged)
            {
                SetupMaterialWithBlendMode(materialEditor.target as Material, GetRenderingMode(), false);
            }
        }

        private bool BlendModePopup()
        {
            var mode = (RenderingMode) _blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (RenderingMode) EditorGUILayout.Popup(VarwinShaderStyles.renderingMode, (int) mode, Enum.GetNames(typeof(RenderingMode)));
            bool result = EditorGUI.EndChangeCheck();
            if (result)
            {
                _materialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                _blendMode.floatValue = (float) mode;
            }

            return result;
        }

        private RenderingMode GetRenderingMode()
        {
            return (RenderingMode) _blendMode.floatValue;
        }

        private void SetBackendValues()
        {
            bool hasMetallicMap = _metallicMap.textureValue;

            if (hasMetallicMap)
            {
                _metallic.floatValue = 1;
            }
        }
        
        private void SetupMaterialWithBlendMode(Material material, RenderingMode blendMode, bool overrideRenderQueue)
        {
            int minRenderQueue = -1;
            int maxRenderQueue = 5000;
            int defaultRenderQueue = -1;
            switch (blendMode)
            {
                case RenderingMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetFloat("_SrcBlend", (float) BlendMode.One);
                    material.SetFloat("_DstBlend", (float) BlendMode.Zero);
                    material.SetFloat("_ZWrite", 1.0f);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = -1;
                    maxRenderQueue = (int) RenderQueue.AlphaTest - 1;
                    defaultRenderQueue = -1;
                    break;
                case RenderingMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetFloat("_SrcBlend", (float) BlendMode.One);
                    material.SetFloat("_DstBlend", (float) BlendMode.Zero);
                    material.SetFloat("_ZWrite", 1.0f);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int) RenderQueue.AlphaTest;
                    maxRenderQueue = (int) RenderQueue.GeometryLast;
                    defaultRenderQueue = (int) RenderQueue.AlphaTest;
                    break;
                case RenderingMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetFloat("_SrcBlend", (float) BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend", (float) BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_ZWrite", 0.0f);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int) RenderQueue.GeometryLast + 1;
                    maxRenderQueue = (int) RenderQueue.Overlay - 1;
                    defaultRenderQueue = (int) RenderQueue.Transparent;
                    break;
                case RenderingMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetFloat("_SrcBlend", (float) BlendMode.One);
                    material.SetFloat("_DstBlend", (float) BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_ZWrite", 0.0f);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int) RenderQueue.GeometryLast + 1;
                    maxRenderQueue = (int) RenderQueue.Overlay - 1;
                    defaultRenderQueue = (int) RenderQueue.Transparent;
                    break;
            }

            if (overrideRenderQueue || material.renderQueue < minRenderQueue || material.renderQueue > maxRenderQueue)
            {
                if (!overrideRenderQueue)
                {
                    Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null,
                        "Render queue value outside of the allowed range ({0} - {1}) for selected Blend mode, resetting render queue to default", minRenderQueue, maxRenderQueue);
                }

                material.renderQueue = defaultRenderQueue;
            }
        }
    }
}