using System;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    public class LightBehaviourHelper : VarwinBehaviourHelper
    {
        public override bool CanAddBehaviour(GameObject gameObject, Type behaviourType)
        {
            if (!base.CanAddBehaviour(gameObject, behaviourType))
            {
                return false;
            }

            return gameObject.GetComponentInChildren<Light>();
        }
    }
    
    [RequireComponentInChildren(typeof(Light))]
    [VarwinComponent(English:"Light",Russian:"Свет",Chinese:"光源",Kazakh:"Жарық",Korean:"빛")]
    public class LightBehaviour : VarwinBehaviour, ISwitchModeSubscriber
    {
        [SerializeField] private GameObject PreviewRender;
        [SerializeField] private Material  PreviewRenderMaterial;
        
        public enum ShadowType
        {
            [Item(English:"no shadows",Russian:"без теней",Chinese:"無陰影",Kazakh:"көлеңкелерсіз",Korean:"그림자 없음")] None,
            [Item(English:"hard shadows",Russian:"жесткие тени",Chinese:"重陰影",Kazakh:"қатты көлеңкелер",Korean:"단단한 그림자")] Hard,
            [Item(English:"soft shadows",Russian:"мягкие тени",Chinese:"柔和的陰影",Kazakh:"жұмсақ көлеңкелер",Korean:"부드러운 그림자")] Soft
        }

        private Light _light;
        private Light LightComponent
        {
            get
            {
                if (!_light)
                {
                    _light = GetComponentInChildren<Light>();
                }

                return _light;
            }
        }

        private Color _color;
        [VarwinInspector(English:"Light Color",Russian:"Цвет",Chinese:"光源色彩",Kazakh:"Түр-түс",Korean:"빛의 색")]
        public Color MainColor
        {
            get => _color;
            set
            {
                if (_color == value)
                {
                    return;
                }

                _color = value;
                LightComponent.color = _color;
                SetPreviewRendererColor(_color);
            }
        }

        private float _range;
        [VarwinInspector(English:"Range",Russian:"Дистанция",Chinese:"範圍",Kazakh:"Дистанция",Korean:"범위")]
        public float Range
        {
            get => _range;
            set
            {
                if (_range.ApproximatelyEquals(value))
                {
                    return;
                }

                _range = value;
                
                LightComponent.range = _range;
            }
        }

        private float _intensity;
        [VarwinInspector(English:"Intensity",Russian:"Интенсивность",Chinese:"強度",Kazakh:"Интенсивтілік",Korean:"강도")]
        public float Intensity
        {
            get => _intensity;
            set
            {
                if (_intensity.ApproximatelyEquals(value))
                {
                    return;
                }

                _intensity = value;
                
                LightComponent.intensity = _intensity;
            }
        }

        private ShadowType _shadows;
        [VarwinInspector(English:"Shadow type",Russian:"Тип теней",Chinese:"影子類型",Kazakh:"Көлеңкелердің типі",Korean:"그림자 유형")]
        public ShadowType Shadows
        {
            get => _shadows;
            set
            {
                if (_shadows == value)
                {
                    return;
                }

                _shadows = value;
                LightComponent.shadows = (LightShadows) _shadows;
            }
        }

        private float _shadowStrength;
        [VarwinInspector(English:"Shadow strength",Russian:"Сила теней",Chinese:"影子強度",Kazakh:"Көлеңкелердің күші",Korean:"그림자 세기")]
        public float ShadowStrength
        {
            get => _shadowStrength;
            set
            {
                if (_shadowStrength.ApproximatelyEquals(value))
                {
                    return;
                }

                _shadowStrength = value;
                LightComponent.shadowStrength = _shadowStrength;
            }
        }
        
        [Action(English:"Change intensity",Russian:"Изменить интенсивность",Chinese:"變更強度",Kazakh:"Қарқындылықты өзгерту",Korean:"변화 강도")]
        public void ChangeIntensity([SourceTypeContainer(typeof(float))] dynamic value)
        {
            if (!TypeValidationUtils.ValidateMethodWithLog(this, value, nameof(ChangeIntensity), 0, out float convertedValue))
            {
                return;
            }

            Intensity = convertedValue;
        }
        
        [Action(English:"Change range",Russian:"Изменить дальность",Chinese:"變更範圍",Kazakh:"Алыстықты өзгерту",Korean:"변경 범위")]
        public void ChangeRange([SourceTypeContainer(typeof(float))] dynamic value)
        {
            if (!TypeValidationUtils.ValidateMethodWithLog(this, value, nameof(ChangeRange), 0, out float convertedValue))
            {
                return;
            }

            Range = convertedValue;
        }
        
        [Action(English:"Change color",Russian:"Изменить цвет",Chinese:"變更色彩",Kazakh:"Түр-түсті өзгерту",Korean:"변경 색상")]
        [ArgsFormat(English:"(values 0-1) r{%} g{%} b{%} a{%}",Russian:"(значения 0-1) r{%} g{%} b{%} a{%}",Chinese:"(值 0-1) r{%} g{%} b{%} a{%}",Kazakh:"(values 0-1) r{%} g{%} b{%} a{%}",Korean:"(값 0-1) r{%} g{%} b{%} a{%}")]
        public void ChangeColor(
            [SourceTypeContainer(typeof(float))] dynamic r, 
            [SourceTypeContainer(typeof(float))] dynamic g, 
            [SourceTypeContainer(typeof(float))] dynamic b,
            [SourceTypeContainer(typeof(float))] dynamic a)
        {
            if (!TypeValidationUtils.ValidateMethodWithLog(this, r, nameof(ChangeColor), 0, out float convertedR))
            {
                return;
            }

            if (!TypeValidationUtils.ValidateMethodWithLog(this, g, nameof(ChangeColor), 1, out float convertedG))
            {
                return;
            }

            if (!TypeValidationUtils.ValidateMethodWithLog(this, b, nameof(ChangeColor), 2, out float convertedB))
            {
                return;
            }

            if (!TypeValidationUtils.ValidateMethodWithLog(this, a, nameof(ChangeColor), 3, out float convertedA))
            {
                return;
            }

            MainColor = new Color(convertedR,convertedG,convertedB,convertedA);
        }
        
        [Action(English:"Change shadow type",Russian:"Изменить тип теней",Chinese:"變更影子類型",Kazakh:"Көлеңкелердің типін өзгерту",Korean:"그림자 유형 변경")]
        public void ChangeShadowType([SourceTypeContainer(typeof(ShadowType))] dynamic value)
        {
            if (!TypeValidationUtils.ValidateMethodWithLog(this, value, nameof(ChangeShadowType), 0, out ShadowType convertedValue))
            {
                return;
            }

            Shadows = convertedValue;
        }
        
        [Action(English:"Change shadow strength",Russian:"Изменить силу теней",Chinese:"變更影子強度",Kazakh:"Көлеңкелердің күшін өзгерту",Korean:"그림자 세기 변경")]
        public void ChangeShadowStrength([SourceTypeContainer(typeof(float))] dynamic value)
        {
            if (!TypeValidationUtils.ValidateMethodWithLog(this, value, nameof(ChangeShadowStrength), 0, out float convertedValue))
            {
                return;
            }

            ShadowStrength = convertedValue;
        }

        private Material _previewMaterial;
        private Renderer[] _renderers;
        private Renderer[] Renderers => _renderers ?? (_renderers = PreviewRender.GetComponentsInChildren<Renderer>());
        
        protected override void AwakeOverride()
        {
            _intensity = LightComponent.intensity;
            _range = LightComponent.range;
            _color = LightComponent.color;
            _shadows = (ShadowType) LightComponent.shadows;
            _shadowStrength = LightComponent.shadowStrength;
            
            if (!PreviewRenderMaterial || !PreviewRender)
            {
                return;
            }
            
            return;

            _previewMaterial = new Material(PreviewRenderMaterial);

            foreach (var rend in Renderers)
            {
                rend.material = _previewMaterial;
            }
        }

        private void SetPreviewRendererColor(Color value)
        {
            if (!PreviewRenderMaterial)
            {
                return;
            }

            var newColor = new Color(value.r, value.g, value.b, PreviewRenderMaterial.color.a);
            foreach (var rend in Renderers)
            {
                rend.material.color = newColor;
            }
        }

        public void OnSwitchMode(GameMode newMode, GameMode oldMode)
        {
            SwitchPreviewRenderVisibility();
        }

        private void OnEnable()
        {
            SwitchPreviewRenderVisibility();
        }

        private void SwitchPreviewRenderVisibility()
        {
            if (!PreviewRender)
            {
                return;
            }

            PreviewRender.SetActive(!ProjectData.IsPlayMode);
        }

        public void SetRange(float range)
        {
            Range = range;
        }
        
        public void SetIntensity(float intensity)
        {
            Intensity = intensity;
        }
        
        public void SetColor(Color color)
        {
            MainColor = color;
        }
        
        public void SetShadowType(ShadowType shadows)
        {
            Shadows = shadows;
        }
        
        public void SetShadowStrength(float shadowStrength)
        {
            ShadowStrength = shadowStrength;
        }
    }
}
