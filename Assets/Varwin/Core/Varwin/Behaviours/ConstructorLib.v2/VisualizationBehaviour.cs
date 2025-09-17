using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Varwin;
using UnityEngine;
using Varwin.Public;
using static Varwin.Core.Behaviours.ConstructorLib.VisualizationBehaviourHelper;
using static Varwin.TypeValidationUtils;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    public class VisualizationBehaviourHelper : VarwinBehaviourHelper
    {
        public const int TransparentRenderQueue = -1;

        public enum PropertyType
        {
            Color,
            MainTexture,
            Glossiness,
            Metallic,
            MainTextureOffset,
            BumpTexture,
            MetallicTexture,
            OcclusionTexture,
            BumpScale
        }

        public enum MaterialType
        {
            Opaque,
            Transparent
        }

        public static Dictionary<PropertyType, int> MaterialProperties = new()
        {
            {PropertyType.Color, Shader.PropertyToID("_Color")},  
            {PropertyType.MainTexture, Shader.PropertyToID("_MainTex")},  
            {PropertyType.Glossiness, Shader.PropertyToID("_Glossiness")},  
            {PropertyType.Metallic, Shader.PropertyToID("_Metallic")},
            {PropertyType.MainTextureOffset, Shader.PropertyToID("_MainTex_ST")},
            {PropertyType.BumpTexture, Shader.PropertyToID("_BumpMap")},
            {PropertyType.MetallicTexture, Shader.PropertyToID("_MetallicGlossMap")},
            {PropertyType.OcclusionTexture, Shader.PropertyToID("_OcclusionMap")},
            {PropertyType.BumpScale, Shader.PropertyToID("_BumpScale")}
        };

        public static Dictionary<MaterialType, Material> LitMaterials;
        public static Dictionary<MaterialType, Material> UnlitMaterials;
        private static bool _initialized;

        private static readonly string[] RequiredMaterialProperties = 
        {
            "_Color",
            "_MainTex",
        };

        public static void Initialize()
        {
            var litOpaqueMaterial = (Material)Resources.Load("Materials/OpaqueLit", typeof(Material));
            var litTransparent = (Material)Resources.Load("Materials/TransparentLit", typeof(Material));
            var unlitTransparent = (Material)Resources.Load("Materials/TransparentUnlit", typeof(Material));

            litTransparent.SetInt("_ZWrite", 1);

            LitMaterials = new()
            {
                {MaterialType.Opaque, litOpaqueMaterial},
                {MaterialType.Transparent, litTransparent}
            };

            UnlitMaterials = new()
            {
                {MaterialType.Opaque, unlitTransparent},
                {MaterialType.Transparent, unlitTransparent}
            };

            _initialized = true;
        }
        
        public VisualizationBehaviourHelper()
        {
            if (!_initialized)
            {
                Initialize();
            }
        }

        public override bool CanAddBehaviour(GameObject gameObject, Type behaviourType)
        {
            if (!base.CanAddBehaviour(gameObject, behaviourType))
            {
                return false;
            }

            var renderers = gameObject.GetComponentsInChildren<Renderer>().Where(x => x.GetType() != typeof(LineRenderer)).ToArray();

            if (renderers.Length <= 0)
            {
                return false;
            }

            var sharedMaterial = renderers.Select(x => x.sharedMaterial).ToArray();
            var appropriateMaterials = sharedMaterial.Where(HasRequiredProperties);

            if (appropriateMaterials.Count() < sharedMaterial.Length)
            {
                return false;
            }

            Material firstMaterial = appropriateMaterials.FirstOrDefault();
            
            if (!firstMaterial)
            {
                return false;
            }

            Color firstColor = firstMaterial.color;
                
            return appropriateMaterials.All(x => x.color == firstColor);
        }

        public override bool IsDisabledBehaviour(GameObject targetGameObject) => IsDisabledBehaviour(targetGameObject, BehaviourType.Visualization);


        private bool HasRequiredProperties(Material material) => material && RequiredMaterialProperties.All(material.HasProperty);
    }
    
    [VarwinComponent(English:"Visualization",Russian:"Визуализация",Chinese:"視覺化",Kazakh:"Визуалдау",Korean:"시각화")]
    [RequireComponentInChildren(typeof(Renderer))]
    public class VisualizationBehaviour : ConstructorVarwinBehaviour
    {
        [VarwinSerializable]
        public bool BackwardCompatibilityIsInitialized { get; set; }
        
        public enum State
        {
            [Item(English:"Stop",Russian:"завершить",Chinese:"停止",Kazakh:"Аяқтау",Korean:"중지")]
            Stop,
            [Item(English:"Pause",Russian:"приостановить",Chinese:"暫停",Kazakh:"Тоқтата тұру",Korean:"일지 정지")]
            Pause,
            [Item(English:"Continue",Russian:"продолжить",Chinese:"繼續",Kazakh:"Жалғастыру",Korean:"계속")]
            Continue
        }
    
        public enum ShadowCastingMode
        {
            [Item(English:"Off",Russian:"Отключено",Chinese:"離開",Kazakh:"Ағытылған",Korean:"꺼짐")] Off,
            [Item(English:"On",Russian:"Включено",Chinese:"開",Kazakh:"Қосылған",Korean:"켜짐")] On,
            [Item(English:"Two sided",Russian:"Двухстороннее",Chinese:"雙面",Kazakh:"Екі жақты",Korean:"양면")] TwoSided,
            [Item(English:"Shadows only",Russian:"Только тени",Chinese:"僅陰影",Kazakh:"Тек көлеңкелер",Korean:"그림자만")] ShadowsOnly
        }

        private State _currentState;
        private Vector4 Offset => new (TilingX, TilingY, OffsetX, OffsetY);
        
        private Renderer[] _renderers;
        private Renderer[] Renderers => _renderers ??= GetRenderers();

        private Dictionary<Renderer, MaterialPropertyBlock[]> _rendererBlocks;
        private List<Texture[]> _rendererMainTextures;
        private List<Texture[]> _rendererBumpTextures;
        private List<Texture[]> _rendererGlossinessTextures;
        private List<Texture[]> _rendererOcclusionTextures;
        private List<float[]> _rendererBumpScale;

        private bool _hasMoreThenOneTexture;

        private Dictionary<Renderer, MaterialPropertyBlock[]> RendererBlocks => _rendererBlocks ??= InitializeBlocks();

        [HideInInspector, SerializeField]
        private bool _receiveShadowsOtherObjects;
        [HideInInspector, SerializeField]
        private Texture _materialTexture;
        [HideInInspector, SerializeField]
        private bool _transparent;
        [HideInInspector, SerializeField]
        private Color _mainColor = Color.white;
        [HideInInspector, SerializeField]
        private ShadowCastingMode _castShadows;
        [HideInInspector, SerializeField]
        private bool _unlit;
        [HideInInspector, SerializeField]
        private float _glossiness;
        [HideInInspector, SerializeField]
        private float _tilingX;
        [HideInInspector, SerializeField]
        private float _tilingY;
        [HideInInspector, SerializeField]
        private float _offsetX;
        [HideInInspector, SerializeField]
        private float _offsetY;
        [HideInInspector, SerializeField]
        private float _metallic;
        [SerializeField]
        private int _renderQueueOffset;

        public override void OnPrepare()
        {
            InitializeProperties();
            ChangeMaterials();
        }

        private Dictionary<Renderer, MaterialPropertyBlock[]> InitializeBlocks()
        {
            var propertyBlocksMap = new Dictionary<Renderer, MaterialPropertyBlock[]>();
            
            foreach (var renderer in Renderers)
            {
                var blocks = new MaterialPropertyBlock[renderer.sharedMaterials.Length];

                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    var block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block, i);
                    blocks[i] = block;
                }

                propertyBlocksMap.Add(renderer, blocks);
            }

            return propertyBlocksMap;
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            Wrapper ??= gameObject.GetWrapper();
            var prevMaterialChangeBehaviour = Wrapper.GetBehaviour<MaterialChangeBehaviour>();
            if (!BackwardCompatibilityIsInitialized)
            {
                if (prevMaterialChangeBehaviour && prevMaterialChangeBehaviour.MainColor != Color.clear)
                {
                    yield return null;

                    MainColor = prevMaterialChangeBehaviour.MainColor;
                    MaterialTexture = prevMaterialChangeBehaviour.MaterialTexture;
                    Metallic = prevMaterialChangeBehaviour.Metallic;
                    Smoothness = prevMaterialChangeBehaviour.Smoothness;
                    Unlit = prevMaterialChangeBehaviour.Unlit;
                    CastShadows = (ShadowCastingMode)prevMaterialChangeBehaviour.CastShadows;
                    TilingX = prevMaterialChangeBehaviour.TilingX;
                    TilingY = prevMaterialChangeBehaviour.TilingY;
                    OffsetX = prevMaterialChangeBehaviour.OffsetX;
                    OffsetY = prevMaterialChangeBehaviour.OffsetY;
                }

                BackwardCompatibilityIsInitialized = true;
                ObjectController.RefreshInspector();
            }
            else if (!MaterialTexture && prevMaterialChangeBehaviour.MaterialTexture)
            {
                MaterialTexture = prevMaterialChangeBehaviour.MaterialTexture;
            }

            if (Offset == Vector4.zero)
            {
                TilingX = 1;
                TilingY = 1;
            }
        }

        #region VarwinInspector

        [VarwinInspector(English:"Texture",Russian:"Текстура",Chinese:"材質",Kazakh:"Текстура",Korean:"텍스처")]
        public Texture MaterialTexture
        {
            get => _materialTexture;
            set
            {
                if (_materialTexture == value)
                {
                    return;
                }

                _materialTexture = value;

                if (_rendererMainTextures == null)
                {
                    return;
                }

                if (value)
                {
                    SetTexture(MaterialProperties[PropertyType.MainTexture], value);
                }
                else
                {
                    ChangeMaterials();
                }
            }
        }

        [VarwinInspector(English:"Color",Russian:"Цвет",Chinese:"色彩",Kazakh:"Түр-түс",Korean:"색")]
        public Color MainColor
        {
            get => _mainColor;
            set
            {
                if (_mainColor == value)
                {
                    return;
                }

                _mainColor = value;
                SetColor();
            }
        }

        [VarwinInspector(English:"Cast shadows",Russian:"Отбрасывание теней",Chinese:"投射陰影",Kazakh:"Көлеңке түсіру",Korean:"그림자 드리우기")]
        public ShadowCastingMode CastShadows
        {
            get => _castShadows;
            set
            {
                if (_castShadows == value)
                {
                    return;
                }

                _castShadows = value;
                Renderers[0].shadowCastingMode = (UnityEngine.Rendering.ShadowCastingMode) value;
            }
        }
        
        [VarwinInspector(English:"Receive shadows",Russian:"Отображать тени других объектов",Chinese:"接收陰影",Kazakh:"Басқа объектілердің көлеңкелерін көрсету",Korean:"그림자 받기")]
        [WarningTooltip(English:"Attention! Enabling shadow rendering disables transparency support on the object",Russian:"Внимание! Включение отображения теней отключает поддержку прозрачности на объекте",Chinese:"注意力！ 启用阴影渲染会禁用对象的透明度支持",Kazakh:"Назар аударыңыз! Көлеңке көрсетуді қосу объектідегі мөлдірлікті қолдауды өшіреді",Korean:"주의! 그림자 렌더링을 활성화하면 객체의 투명도 지원이 비활성화 됩니다.")]
        public bool ReceiveShadowsOtherObjects
        {
            get => _receiveShadowsOtherObjects;
            set
            {
                if (_receiveShadowsOtherObjects == value)
                {
                    return;
                }

                _receiveShadowsOtherObjects = value;
                Renderers[0].receiveShadows = _receiveShadowsOtherObjects;
                ChangeMaterials();
            }
        }
        
        [VarwinInspector(English:"Unlit",Russian:"Неосвещенный материал",Chinese:"不發光",Kazakh:"Жарықтандырылмаған материал",Korean:"라이팅 제외")]
        public bool Unlit
        {
            get => _unlit;
            set
            {
                if (_unlit == value)
                {
                    return;
                }

                _unlit = value;
                ChangeMaterials();
            }
        }
        
        [VarwinInspector(English:"Metalness",Russian:"Металличность",Chinese:"金屬化程度",Kazakh:"Металдылық",Korean:"금속성")]
        public float Metallic
        {
            get => _metallic;
            set
            {
                if (_metallic.ApproximatelyEquals(value))
                {
                    return;
                }

                _metallic = value;
                SetFloat(MaterialProperties[PropertyType.Metallic], value);
            }
        }
        
        [VarwinInspector(English:"Smoothness",Russian:"Гладкость",Chinese:"光滑度",Kazakh:"Тегістік",Korean:"매끄러움")]
        public float Smoothness
        {
            get => _glossiness;
            set
            {
                if (_glossiness.ApproximatelyEquals(value))
                {
                    return;
                }

                _glossiness = value;
                SetFloat(MaterialProperties[PropertyType.Glossiness], value);
            }
        }
        
        [VarwinInspector(English:"Texture tiling X",Russian:"Тайлинг текстуры по X",Chinese:"X方向的材質塊數",Kazakh:"Х бойынша текстура тайлингі",Korean:"텍스처 타일링 X")]
        public float TilingX
        {
            get => _tilingX;
            set
            {
                if (_tilingX.ApproximatelyEquals(value))
                {
                    return;
                }

                _tilingX = value;
                var vector = new Vector4(value, TilingY, OffsetX, OffsetY);
                SetVector(MaterialProperties[PropertyType.MainTextureOffset], vector);
            }
        }
        
        [VarwinInspector(English:"Texture tiling Y",Russian:"Тайлинг текстуры по Y",Chinese:"Y方向的材質塊數",Kazakh:"Y бойынша текстура тайлингі",Korean:"텍스처 타일링 Y")]
        public float TilingY
        {
            get => _tilingY;
            set
            {
                if (_tilingY.ApproximatelyEquals(value))
                {
                    return;
                }

                _tilingY = value;
                var vector = new Vector4(TilingX, value, OffsetX, OffsetY);
                SetVector(MaterialProperties[PropertyType.MainTextureOffset], vector);
            }
        }
        
        [VarwinInspector(English:"Texture offset X",Russian:"Смещение текстуры по X",Chinese:"材質在X方向的偏移量",Kazakh:"Текстураның Х бойынша ығысуы",Korean:"텍스처 오프셋 X")]
        public float OffsetX
        {
            get => _offsetX;
            set
            {
                if (_offsetX.ApproximatelyEquals(value))
                {
                    return;
                }

                _offsetX = value;
                var vector = new Vector4(TilingX, TilingY, value, OffsetY);
                SetVector(MaterialProperties[PropertyType.MainTextureOffset], vector);
            }
        }
        
        [VarwinInspector(English:"Texture offset Y",Russian:"Смещение текстуры по Y",Chinese:"材質在Y方向的偏移量",Kazakh:"Текстураның Y бойынша ығысуы",Korean:"텍스처 오프셋 Y")]
        public float OffsetY
        {
            get => _offsetY;
            set
            {
                if (_offsetY.ApproximatelyEquals(value))
                {
                    return;
                }

                _offsetY = value;
                var vector = new Vector4(TilingX, TilingY, OffsetX, value);
                SetVector(MaterialProperties[PropertyType.MainTextureOffset], vector);
            }
        }

        [VarwinInspector(English:"Render queue offset",Russian:"Смещение очереди отрисовки",Chinese:"渲染佇列偏移",Kazakh:"Суретін салу кезегінің ығысуы",Korean:"렌더링 대기열 오프셋")]
        public int RenderQueueOffset
        {
            get => _renderQueueOffset;
            set
            {
                _renderQueueOffset = Mathf.Clamp(value,  -32768, 32767);
                SetSortingOrderOffset(_renderQueueOffset);
            }
        }
        #endregion

        #region Actions

        [LogicGroup(English:"Visualization",Russian:"Визуализация",Chinese:"視覺化",Kazakh:"Визуалдау",Korean:"시각화")]
        [LogicTooltip(
English:"Instantly changes the object's color to the selected color. If you use the block with an object that has textures, the block will give it the selected hue.",Russian:"Мгновенно меняет цвет объекта на выбранный. При использовании блока с объектом, имеющим текстуры, блок придаст ему выбранный оттенок.",Chinese:"更改物件的色彩至指定的顏色，如果您使用包含材質的物件，在方塊上會賦予選擇的色調",Kazakh:"Объектінің түсін таңдалған түске лезде өзгертеді. Текстуралы объектісі бар блокты пайдаланған кезде, блок оған таңдалған реңк береді.",Korean:"객체의 색을 선택한 색으로 즉시 변경합니다. 텍스처가 있는 객체를 포함한 블록을 사용하는 경우 블록은 선택한 색상을 제공합니다.")]
        [Action(English:"change color to",Russian:"изменить цвет",Chinese:"變更顏色為",Kazakh:"түр-түсті өзгерту",Korean:"의 색상 변경")]
        public void ChangeObjectColor([SourceTypeContainer(typeof(Color))] dynamic color)
        {
            if (!ValidateMethodWithLog(this, color, nameof(ChangeObjectColor), 0, out Color convertedColor))
            {
                return;
            }

            MainColor = convertedColor;
        }

        [LogicGroup(English:"Visualization",Russian:"Визуализация",Chinese:"視覺化",Kazakh:"Визуалдау",Korean:"시각화")]
        [LogicTooltip(English:"Starts changing the object's color to the selected color for the specified amount of time. If you use the block with an object that has textures, the block will give it the selected hue.",Russian:"Запускает изменение цвета объекта на выбранный в течение заданного времени. При использовании блока с объектом, имеющим текстуры, блок придаст ему выбранный оттенок.",Chinese:"在指定的時間內逐漸變更物件的色彩，如果您使用包含材質的物件，在方塊上會賦予選擇的色調",Kazakh:"Объектінің түсін таңдалған түске берілген уақыт ішінде өзгертуді іске қосады. Текстуралы объектісі бар блокты пайдаланған кезде, блок оған таңдалған реңк береді.",Korean:"지정된 시간 동안 객체의 색을 선택한 색으로 변경하기 시작합니다. 텍스처가 있는 객체를 포함한 블록을 사용하는 경우 블록은 선택한 색상을 제공합니다.")]
        [Action(English:" ",Russian:" ",Chinese:" ",Kazakh:" ",Korean:" ")]
        [ArgsFormat(English:"change color up to {%} for {%} s",Russian:"изменить цвет до {%} в течение {%} с",Chinese:"逐漸變更顏色至{%}，在{%}秒內",Kazakh:"{%} с ішінде түр-түсті {%} дейін өзгерту",Korean:"다음의 색상을 초 단위로 변경 {%} 초:{%} s")]
        public IEnumerator SmoothlyChangeColor([SourceTypeContainer(typeof(Color))] dynamic color, [SourceTypeContainer(typeof(float))] dynamic duration)
        {
            var methodName = nameof(SmoothlyChangeColor);

            if (!ValidateMethodWithLog(this, color, methodName, 0, out Color convertedColor))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, duration, methodName, 1, out float convertedDuration))
            {
                yield break;
            }

            var time = 0f;
            _currentState = State.Continue;
            var startColors = _mainColor;
            while (time < 1.0f) 
            {
                if (_currentState == State.Pause)
                {
                    yield return null;
                }
                else if(_currentState == State.Stop)
                {
                    break;
                }
                else
                {
                    time += Time.deltaTime / convertedDuration;
                    if (time > 1) time = 1;

                    var nextColor = Color.Lerp(startColors, convertedColor, time);
                    MainColor = nextColor;
                    
                    yield return null;
                }
            }

            _currentState = State.Stop;
        }
        
        [LogicGroup(English:"Visualization",Russian:"Визуализация",Chinese:"視覺化",Kazakh:"Визуалдау",Korean:"시각화")]
        [LogicTooltip(English:"Controls any color changing. A paused color changing can be continued with the corresponding block.",Russian:"Управляет любым изменением цвета. Приостановленное изменение цвета можно продолжить соответствующим блоком.",Chinese:"控制任何的色彩變化，暫停中的色彩變化可以被相關的方塊恢復動作",Kazakh:"Кез келген түр-түсті өзгертуді басқарады. Тоқтатыла тұрған түр-түс өзгертуді тиісті блокпен қайта бастауға болады.",Korean:"색의 변경을 제어합니다. 일시 정지된 색의 변경은 해당 블록에서 계속할 수 있습니다.")]
        [Action(English:" ",Russian:" ",Chinese:" ",Kazakh:" ",Korean:" ")]
        [ArgsFormat(English:"{%} color changing",Russian:"{%} изменение цвета",Chinese:"{%}色彩變化",Kazakh:"{%} түр-түсті өзгерту",Korean:"색상 변경{%}")]
        public void SetGhangingColorState([SourceTypeContainer(typeof(State))] dynamic targetState)
        {
            if (!ValidateMethodWithLog(this, targetState, nameof(SetGhangingColorState), 0, out State convertedState))
            {
                return;
            }

            _currentState = convertedState;
        }

        #endregion

        #region Checkers

        [LogicGroup(English:"Visualization",Russian:"Визуализация",Chinese:"視覺化",Kazakh:"Визуалдау",Korean:"시각화")]
        [LogicTooltip(
English:"Returns true if the specified object is currently changing color. Otherwise it returns false",Russian:"Возвращает “истину”, если указанный объект изменяет цвет в данный момент. В противном случае возвращает “ложь”.",Chinese:"當物件正在改變顏色時回傳值為真；反之，回傳值為假",Kazakh:"Егер көрсетілген объект осы сәтте түр-түсті өзгертіп жатса, “шындықты” қайтарады. Олай болмаған жағдайда “өтірікті” қайтарады.",Korean:"지정된 객체가 현재 색을 변경하고 있는 경우\" 참 ( true ) \"을 반환함. 그렇지 않으면 \" 거짓 ( false ) \"을 반환함.")]
        [Checker(English:"is changing color at the moment",Russian:"изменяет цвет в данный момент",Chinese:"正在改變顏色",Kazakh:"осы сәтте түр-түсті өзгертуде",Korean:"의 색이 변경되는 중이라면")]
        public bool IsColorChangingNow() => _currentState == State.Continue;

        #endregion
        
        #region Variables

        [LogicGroup(English:"Visualization",Russian:"Визуализация",Chinese:"視覺化",Kazakh:"Визуалдау",Korean:"시각화")]
        [LogicTooltip(English:"Returns the color of the specified object as a color block.",Russian:"Возвращает цвет указанного объекта в виде блока цвета.",Chinese:"以顏色方塊回傳指定物件的顏色",Kazakh:"Көрсетілген объектінің түсін түр-түс блогы түрінде қайтарады.",Korean:"지정된 객체의 색을 색 블록으로 반환합니다.")]
        [Variable(English:"color",Russian:"цвет",Chinese:"色彩",Kazakh:"түр-түс",Korean:"의 색상")]
        public Color CurrentColor => MainColor;

        #endregion

        #region Private Helpers

        private void InitializeProperties()
        {
            MainColor = Renderers[0].sharedMaterial.HasColor(MaterialProperties[PropertyType.Color]) ? Renderers[0].sharedMaterial.color : _mainColor;
            
            Metallic = Renderers[0].sharedMaterial.HasFloat(MaterialProperties[PropertyType.Metallic]) ? Renderers[0].sharedMaterial.GetFloat(MaterialProperties[PropertyType.Metallic]) : 0;
            Smoothness = Renderers[0].sharedMaterial.HasFloat(MaterialProperties[PropertyType.Glossiness]) ? Renderers[0].sharedMaterial.GetFloat(MaterialProperties[PropertyType.Glossiness]) : 0;

            var hasProperty = Renderers[0].sharedMaterial.HasProperty(MaterialProperties[PropertyType.MainTextureOffset]);

            if (hasProperty)
            {
                var offsetVector = Renderers[0].sharedMaterial.GetVector(MaterialProperties[PropertyType.MainTextureOffset]);

                TilingX = offsetVector.x;
                TilingY = offsetVector.y;
                OffsetX = offsetVector.z;
                OffsetY = offsetVector.w;
            }

            _rendererMainTextures = new List<Texture[]>(Renderers.Length);
            _rendererBumpTextures = new List<Texture[]>(Renderers.Length);
            _rendererOcclusionTextures = new List<Texture[]>(Renderers.Length);
            _rendererGlossinessTextures = new List<Texture[]>(Renderers.Length);
            _rendererBumpScale = new List<float[]>(Renderers.Length);

            for (var i = 0; i < Renderers.Length; i++)
            {
                var renderer = Renderers[i];
                _rendererMainTextures.Add(new Texture[renderer.sharedMaterials.Length]);
                _rendererBumpTextures.Add(new Texture[renderer.sharedMaterials.Length]);
                _rendererOcclusionTextures.Add(new Texture[renderer.sharedMaterials.Length]);
                _rendererGlossinessTextures.Add(new Texture[renderer.sharedMaterials.Length]);
                _rendererBumpScale.Add(new float[renderer.sharedMaterials.Length]);
                
                for (var index = 0; index < renderer.sharedMaterials.Length; index++)
                {
                    var sharedMaterial = renderer.sharedMaterials[index];
                    _rendererMainTextures[i][index] = sharedMaterial.GetTexture("_MainTex");
                    _rendererBumpTextures[i][index] = sharedMaterial.GetTexture("_BumpMap");
                    _rendererGlossinessTextures[i][index] = sharedMaterial.GetTexture("_MetallicGlossMap");
                    _rendererOcclusionTextures[i][index] = sharedMaterial.GetTexture("_OcclusionMap");
                    _rendererBumpScale[i][index] = sharedMaterial.GetFloat("_BumpScale");

                    if (!_rendererMainTextures[i][index])
                    {
                        _rendererMainTextures[i][index] = Texture2D.whiteTexture;
                    }
                }
            }

            CastShadows = (ShadowCastingMode) Renderers[0].shadowCastingMode;
        }

        private void ChangeMaterials()
        {
            var material = _unlit ? UnlitMaterials[MaterialType.Transparent] : _receiveShadowsOtherObjects ? LitMaterials[MaterialType.Opaque] : LitMaterials[MaterialType.Transparent];
            for (var i = 0; i < Renderers.Length; i++)
            {
                var materials = new Material[Renderers[i].sharedMaterials.Length];
                FillMaterialArray(ref materials, material);

                Renderers[i].sharedMaterials = materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    SetMaterialBlockTexture(PropertyType.MainTexture, Renderers[i], !_materialTexture ? _rendererMainTextures[i][j] : _materialTexture, j);
                    SetMaterialBlockTexture(PropertyType.BumpTexture, Renderers[i], _rendererBumpTextures[i][j], j);
                    SetMaterialBlockTexture(PropertyType.MetallicTexture, Renderers[i], _rendererGlossinessTextures[i][j], j);
                    SetMaterialBlockTexture(PropertyType.OcclusionTexture, Renderers[i], _rendererOcclusionTextures[i][j], j);
                    SetMaterialBlockFloat(PropertyType.BumpScale, Renderers[i], _rendererBumpScale[i][j], j);
                }
            }
        }

        private Renderer[] GetRenderers()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            return renderers.Where(a => a.gameObject.GetWrapper() == Wrapper).ToArray();
        }

        public void PassPropertiesToRenderers()
        {
            ChangeMaterials();
        }

        public void CopyPropertiesTo(VisualizationBehaviour visualizationBehaviour)
        {
            visualizationBehaviour._renderers = null;
            visualizationBehaviour._rendererMainTextures = _rendererMainTextures;
            visualizationBehaviour._rendererBumpTextures = _rendererBumpTextures;
            visualizationBehaviour._rendererGlossinessTextures = _rendererGlossinessTextures;
            visualizationBehaviour._rendererOcclusionTextures = _rendererOcclusionTextures;
            visualizationBehaviour._rendererBumpScale = _rendererBumpScale;
            visualizationBehaviour._materialTexture = _materialTexture;

            visualizationBehaviour.PassPropertiesToRenderers();
            
            visualizationBehaviour.SetColor();
            visualizationBehaviour.SetVector(MaterialProperties[PropertyType.MainTextureOffset], Offset);
            visualizationBehaviour.SetFloat(MaterialProperties[PropertyType.Glossiness], Smoothness);
            visualizationBehaviour.SetFloat(MaterialProperties[PropertyType.Metallic], Metallic);
        }

        private void FillMaterialArray(ref Material[] materials, Material setupMaterial)
        {
            for (var i = 0; i < materials.Length; i++)
            {
                materials[i] = setupMaterial;
            }
        }

        private void SetVector(int propertyId, Vector4 vector)
        {
            for (int i = 0; i < Renderers.Length; i++)
            {
                var blocks = RendererBlocks[Renderers[i]];

                for (int materialIndex = 0; materialIndex < blocks.Length; materialIndex++)
                {
                    blocks[materialIndex].SetVector(propertyId, vector);
                    Renderers[i].SetPropertyBlock(blocks[materialIndex], materialIndex);
                }
            }
        }

        private void SetColor()
        {
            foreach (var meshRenderer in Renderers)
            {
                var blocks = RendererBlocks[meshRenderer];
                var propertyId = MaterialProperties[PropertyType.Color];

                for (var i = 0; i < blocks.Length; i++)
                {
                    blocks[i].SetColor(propertyId, _mainColor);
                    meshRenderer.SetPropertyBlock(blocks[i], i);
                }
            }
        }

        public void SetTexture(int propertyId, Texture texture)
        {
            for (int i = 0; i < Renderers.Length; i++)
            {
                var blocks = RendererBlocks[Renderers[i]];

                for (int materialIndex = 0; materialIndex < blocks.Length; materialIndex++)
                {
                    blocks[materialIndex].SetTexture(propertyId, texture);
                    Renderers[i].SetPropertyBlock(blocks[materialIndex], materialIndex);
                }
            }
        }

        private void SetMaterialBlockTexture(PropertyType type, Renderer renderer, Texture texture, int materialIndex)
        {
            if (!texture)
            {
                return;
            }
            
            var blocks = RendererBlocks[renderer];
            blocks[materialIndex].SetTexture(MaterialProperties[type], texture);
            renderer.SetPropertyBlock(blocks[materialIndex], materialIndex);
        }
        
        private void SetMaterialBlockFloat(PropertyType type, Renderer renderer, float value, int materialIndex)
        {
            var blocks = RendererBlocks[renderer];
            blocks[materialIndex].SetFloat(MaterialProperties[type], value);
            renderer.SetPropertyBlock(blocks[materialIndex], materialIndex);
        }

        private void SetFloat(int propertyId, float value)
        {
            for (int i = 0; i < Renderers.Length; i++)
            {
                var blocks = RendererBlocks[Renderers[i]];

                for (int materialIndex = 0; materialIndex < blocks.Length; materialIndex++)
                {
                    blocks[materialIndex].SetFloat(propertyId, value);
                    Renderers[i].SetPropertyBlock(blocks[materialIndex], materialIndex);
                }
            }
        }

        private void SetSortingOrderOffset(int renderQueueOffset)
        {
            for (int i = 0; i < Renderers.Length; i++)
            {
                var blocks = RendererBlocks[Renderers[i]];

                for (int materialIndex = 0; materialIndex < blocks.Length; materialIndex++)
                {
                    Renderers[i].sortingOrder = TransparentRenderQueue + renderQueueOffset;
                }
            }
        }

        #endregion
        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}