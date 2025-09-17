using System;
using System.Linq;
using UnityEngine;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    [Obsolete]
    public class MaterialChangeBehaviourHelper : VarwinBehaviourHelper
    {
        private static readonly string[] RequiredMaterialProperties = 
        {
            "_Color",
            "_MainTex",
            "_Glossiness",
            "_Metallic"
        };

        public override bool CanAddBehaviour(GameObject gameObject, Type behaviourType)
        {
            if (!base.CanAddBehaviour(gameObject, behaviourType))
            {
                return false;
            }

            var renderers = gameObject.GetComponentsInChildren<Renderer>();

            if (renderers.Length <= 0)
            {
                return false;
            }

            var materials = renderers.Select(x => x.sharedMaterial
            ).Where(HasRequiredProperties);
            Material firstMaterial = materials.FirstOrDefault();
            
            if (!firstMaterial)
            {
                return false;
            }

            Color firstColor = firstMaterial.color;
                
            return materials.All(x => x.color == firstColor);
        }

        private bool HasRequiredProperties(Material material)
        {
            return material && RequiredMaterialProperties.All(material.HasProperty);
        }
    }
    
    [Obsolete]
    [RequireComponentInChildren(typeof(Renderer))]
    [VarwinComponent(English: "Material", Russian: "Материал", Chinese: "材料")]
    public class MaterialChangeBehaviour : VarwinBehaviour
    {
        public enum ShadowCastingMode
        {
            [Item(English: "Off", Russian: "Отключено", Chinese: "離開")] Off,
            [Item(English: "On", Russian: "Включено", Chinese: "開")] On,
            [Item(English: "Two sided", Russian: "Двухстороннее", Chinese: "雙面")] TwoSided,
            [Item(English: "Shadows only", Russian: "Только тени", Chinese: "僅陰影")] ShadowsOnly
        }

        #region MATERIAL PARAMETERS

        [Obsolete]
        [VarwinInspector(English: "Material texture", Russian: "Текстура материала", Chinese: "材質質感")]
        public Texture MaterialTexture { get; set; }

        [Obsolete]
        [VarwinInspector(English: "Material color", Russian: "Цвет материала", Chinese: "材質顏色")]
        public Color MainColor { get; set; }
        
        [Obsolete]
        [Action(English:"Change color", Russian:"Изменить цвет", Chinese: "變更色彩")]
        [ArgsFormat(English:"(values 0-1) r{%} g{%} b{%} a{%}", Russian: "(значения 0-1) r{%} g{%} b{%} a{%}")]
        public void ChangeColor(float r, float g, float b, float a)
        {
            MainColor = new Color(r,g,b,a);
        }

        [Obsolete]
        [VarwinInspector("Cast shadows", Russian: "Отбрасывание теней", Chinese: "投射陰影")]
        public ShadowCastingMode CastShadows { get; set; }

        [Obsolete]
        [Variable(English: "Receive shadows", Russian: "Отображать тени других объектов", Chinese: "接收陰影")]
        [VarwinInspector("Receive shadows", Russian: "Отображать тени других объектов", Chinese: "接收陰影")]
        public bool ReceiveShadows { get; set; }

        [Obsolete]
        [Variable(English: "Unlit", Russian: "Неосвещенный материал", Chinese: "不發光")]
        [VarwinInspector("Unlit", Russian: "Неосвещенный материал", Chinese: "不發光")]
        public bool Unlit { get; set; }
        
        [Obsolete]
        [Variable(English: "Material metalness", Russian: "Металличность материала", Chinese: "金屬化程度")]
        [VarwinInspector(English: "Material metalness", Russian: "Металличность материала", Chinese: "金屬化程度")]
        public float Metallic { get; set; }

        [Obsolete]
        [Variable(English: "Material smoothness", Russian: "Гладкость материала", Chinese: "光滑度")]
        [VarwinInspector(English: "Material smoothness", Russian: "Гладкость материала", Chinese: "光滑度")]
        public float Smoothness { get; set; }

        [Obsolete]
        [Variable(English: "Texture tiling X", Russian: "Тайлинг текстуры по X", Chinese: "X方向的材質塊數")]
        [VarwinInspector(English: "Texture tiling X", Russian: "Тайлинг текстуры по X", Chinese: "X方向的材質塊數")]
        public float TilingX { get; set; }

        [Obsolete]
        [Variable(English: "Texture tiling Y", Russian: "Тайлинг текстуры по Y", Chinese: "Y方向的材質塊數")]
        [VarwinInspector(English: "Texture tiling Y", Russian: "Тайлинг текстуры по Y", Chinese: "Y方向的材質塊數")]
        public float TilingY { get; set; }
        
        [Obsolete]
        [Variable(English: "Texture offset X", Russian: "Смещение текстуры по X", Chinese: "材質在X方向的偏移量")]
        [VarwinInspector(English: "Texture offset X", Russian: "Смещение текстуры по X", Chinese: "材質在X方向的偏移量")]
        public float OffsetX { get; set; }
        
        [Obsolete]
        [Variable(English: "Texture offset Y", Russian: "Смещение текстуры по Y", Chinese: "材質在Y方向的偏移量")]
        [VarwinInspector(English: "Texture offset Y", Russian: "Смещение текстуры по Y", Chinese: "材質在Y方向的偏移量")]
        public float OffsetY { get; set; }

        #endregion //MATERIAL PARAMETERS
        
        [Obsolete]
        [Action(English: "Change shadow casting mode", Russian: "Изменить режим отбрасывания теней", Chinese: "更改陰影投射模式")]
        public void SetShadowCastingMode(ShadowCastingMode shadowCastingMode)
        {
            CastShadows = shadowCastingMode;
        }

        #region RPC calls
        
        public void SetColor(Color color)
        {
            MainColor = color;
        }
        
        public void SetUnlit(bool isUnlit)
        {
            Unlit = isUnlit;
        }
        
        public void SetMetallic(float metallic)
        {
            Metallic = metallic;
        }
        
        public void SetSmoothness(float smoothness)
        {
            Smoothness = smoothness;
        }

        public void SetTilingX(float tiling)
        {
            TilingX = tiling;
        }

        public void SetTilingY(float tiling)
        {
            TilingY = tiling;
        }

        public void SetOffsetX(float offset)
        {
            OffsetX = offset;
        }

        public void SetOffsetY(float offset)
        {
            OffsetY = offset;
        }

        public void SetShadowMode(ShadowCastingMode shadowCastingMode)
        {
            CastShadows = shadowCastingMode;
        }

        public void SetShadowReceive(bool receiveShadows)
        {
            ReceiveShadows = receiveShadows;
        }
        
        #endregion
    }
}