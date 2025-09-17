
Shader "Varwin/Mobile/Standard (Dev)"
{
    Properties 
    {
        _MainTex ("Albedo", 2D) = "white" {}
        [MaterialToggle] _DisableAlbedo("Disable Albedo", Float) = 0
        _Color("Color", Color) = (1,1,1,1)
        
        _MetallicGlossMap ("MetallicGloss", 2D) = "white" {}

        _Metallic("Additional Metal", Range(-1, 1)) = 0
        [MaterialToggle] _DisableMetal("Disable Metal", Float) = 0
        _Metal("Disabled Metal", Range(0, 1)) = 0

        _Glossiness("Additional Gloss", Range(-1, 1)) = 0
        [MaterialToggle] _DisableGloss("Disable Gloss", Float) = 0
        _Gloss("Disabled Gloss", Range(0, 1)) = 0

        [MaterialToggle] _DisableOcclusion("Disable Occlusion", Float) = 0

        [Normal]_BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1
        [MaterialToggle] _DisableBump("Disable Normal", Float) = 0

        [MaterialToggle] _DrawAlbedo("Draw Albedo", Float) = 1
        [MaterialToggle] _DrawMetal("Draw Metal", Float) = 0
        [MaterialToggle] _DrawGloss("Draw Gloss", Float) = 0
        [MaterialToggle] _DrawNormal("Draw Normal", Float) = 0
        [MaterialToggle] _DrawTransparency("Draw Transparency", Float) = 0
        [MaterialToggle] _DrawOcclusion("Draw Occlusion", Float) = 0

        [MaterialToggle] _UseDSH("Use Diffuse Shading", Float) = 1
        [MaterialToggle] _UseLM("Use Shadows", Float) = 1

        [MaterialToggle] _CheckNormals("Backface Check", Float) = 0
        _FFColor("Front Color", Color) = (0,0,1,0.7)
        _BFColor("Back Color", Color) = (1,0,0,1)
    }

    SubShader 
    {
        Tags { "RenderType"="Opaque" }
                
        UsePass "VertexLit/SHADOWCASTER"

        Pass 
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            Cull Back
            CGPROGRAM
            #pragma vertex vert_surf
            #pragma fragment frag_surf
            #pragma only_renderers gles3 d3d11 vulkan// d3d11 for Editor. glcore gles metal vulkan d3d11_9x
            #pragma multi_compile_fwdbase
            #pragma shader_feature FOG_OFF FOG_LINEAR
            
            #include "HLSLSupport.cginc"
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"


            sampler2D _MainTex;
            sampler2D _MetallicGlossMap;
            sampler2D _BumpMap;

            float _DrawAlbedo;
            float _DrawMetal;
            float _DrawGloss;
            float _DrawNormal;
            float _DrawTransparency;
            float _DrawOcclusion;

            float _UseDSH;
            float _UseLM;
            float _UseSM;
            float _UseRefl;

            float _DisableAlbedo;
            float3 _Color;
            float _Metallic;
            float _DisableMetal;
            float _Metal;
            float _Glossiness;
            float _DisableGloss;
            float _Gloss;
            float _DisableOcclusion;
            float _BumpScale;
            float _DisableBump;

            float _CheckNormals;
            float4 _FFColor;
            float3 _BFColor;


            struct Input {
                float2 uv_MainTex;

                float3 viewDir;
                float3 invView;

                float3 tangent;
                float3 normal;
                float3 binormal;

                float3 normalWorld;
                float3 fresnelFac;
            };

            struct bdsmOutputStandard
            {
                fixed3 Albedo;
                fixed3 Normal;
                fixed3 MultiMap;
            };

            float3 UnpackTBN(float4 normalmap, float3 tangent, float3 binormal, float3 normalWorld)
            {
                // Approximated UnpackNormal
                //float3 localCoords = float3(2.0 * normalmap.a - 1.0, 
                    //2.0 * normalmap.g - 1.0, 0.0);
                //localCoords.z = 1.0 - 0.5 * dot(localCoords, localCoords);
                fixed3 localCoords = UnpackNormal(normalmap);

                float3x3 local2WorldMatrix = float3x3(tangent, binormal, normalWorld);
                float3 result = normalize(mul(localCoords, local2WorldMatrix));

                return result;
            }


            float fresnelFactor(float3 viewDir, float3 surfaceNormal)
            {
                float fresnelFactor = 1-float(1*max(0.0, dot(normalize(-viewDir), surfaceNormal)));
                return fresnelFactor;
            }


            float3 blinnSpecular(float3 viewDir, float3 surfaceNormal, float roughness)
            {
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 H = normalize(viewDir + lightDirection);
                float3 specular = pow(max(0.0, dot(reflect(-lightDirection, surfaceNormal), viewDir)), (roughness)*50);
                return _LightColor0.rgb*(specular*2);
            }


            float3 diffuseReflection(float3 surfaceNormal)
            {
                float3 diffuseReflection = _LightColor0.rgb*0.58*max(0.0, dot(surfaceNormal, normalize(_WorldSpaceLightPos0.xyz)));
                return diffuseReflection;
            }


            float3 AlbedoDebug(float3 albedo)
            {
                float3 result = albedo*_Color;
                if(_DisableAlbedo==1)
                {
                    result = _Color;
                }
                return result;
            }

            float3 MetallicDebug(float metallic)
            {
                metallic = metallic+_Metallic;
                if(_DisableMetal==1)
                {
                    metallic = _Metal+_Metallic;
                }
                return metallic;
            }

            float3 GlossDebug(float gloss)
            {
                gloss+=_Glossiness;
                
                if(_DisableGloss==1)
                {
                    gloss = _Gloss+_Glossiness;
                }
                return gloss;
            }

            float3 OcclusionDebug(float occlusion)
            {
                if(_DisableOcclusion==1)
                {
                    occlusion = 1;
                }
                return occlusion;
            }

            float3 NormalDebug(float3 normal, float3 worldspace)
            {
                if(_DisableBump==1)
                {
                    normal = worldspace;
                }
                return normal;
            }

            struct v2f_surf 
            {
                float4 pos : SV_POSITION;
                float2 pack0 : TEXCOORD0;

                #ifndef LIGHTMAP_ON
                fixed3 normal : NORMAL;
                #endif

                #ifdef LIGHTMAP_ON
                float2 lmap : TEXCOORD2;
                #endif

                float fresnelFac : TEXCOORD3;
                float3 posWorld : TEXCOORD4;

                float3 normalWorld : TEXCOORD5;
                float3 binormal : TEXCOORD6;
                float3 tangent : TANGENT;

                SHADOW_COORDS(8)
                UNITY_FOG_COORDS(9)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;
            v2f_surf vert_surf (appdata_full v)
            {
                v2f_surf o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                float3 worldN = UnityObjectToWorldNormal(v.normal);

                #ifdef LIGHTMAP_ON
                o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                float4x4 modelMatrix = unity_ObjectToWorld;
                float4x4 modelMatrixInverse = unity_WorldToObject;
                o.posWorld = mul(modelMatrix, v.vertex).xyz;

                o.normalWorld = UnityObjectToWorldNormal(v.normal);
                o.tangent = normalize(mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
                o.binormal = normalize(cross(o.normalWorld, o.tangent) * v.tangent.w);
                #endif

                #ifndef LIGHTMAP_ON
                o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                float4x4 modelMatrix = unity_ObjectToWorld;
                float4x4 modelMatrixInverse = unity_WorldToObject;
                o.posWorld = mul(modelMatrix, v.vertex).xyz;

                o.normalWorld = UnityObjectToWorldNormal(v.normal);
                o.tangent = normalize(mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
                o.binormal = normalize(cross(o.normalWorld, o.tangent) * v.tangent.w);
                #endif

                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }


            fixed3 frag_surf (v2f_surf IN) : SV_Target
            {
                Input surfIN;
                surfIN.uv_MainTex = IN.pack0.xy;
                float4 AlbedoMap = tex2D(_MainTex, IN.pack0.xy);
                float4 MultiMap = tex2D(_MetallicGlossMap, IN.pack0.xy);
                float4 NormalMap = tex2D(_BumpMap, IN.pack0.xy);

                float3 viewDirection = normalize(_WorldSpaceCameraPos - IN.posWorld.xyz);
                float3 invView = normalize(IN.posWorld.xyz - _WorldSpaceCameraPos);

                float3 surfaceNormal = UnpackTBN(NormalMap, IN.tangent, IN.binormal, IN.normalWorld);

                surfaceNormal = NormalDebug(surfaceNormal, IN.normalWorld);

                float roughness = MultiMap.a;
                float metal = MultiMap.r;
                float occlusion = MultiMap.g;
                float3 albedo = float3(1, 1, 1);

                if (_DrawAlbedo==1)
                {
                    albedo = AlbedoMap.rgb;
                }
                if (_DrawMetal==1)
                {
                    albedo = metal;
                }
                if (_DrawGloss==1)
                {
                    albedo = roughness;
                }
                if (_DrawNormal==1)
                {
                    albedo = surfaceNormal;
                }

                albedo = AlbedoDebug(albedo);
                roughness = GlossDebug(roughness);
                metal = MetallicDebug(metal);
                occlusion = OcclusionDebug(occlusion);

                fixed atten = SHADOW_ATTENUATION(IN);

                float fresnel = fresnelFactor(invView, surfaceNormal) * occlusion;
                float3 reflectedDir = reflect(invView, surfaceNormal);

                float3 diffuseLight = diffuseReflection(surfaceNormal) * occlusion;
                float3 blinn = clamp(blinnSpecular(viewDirection , surfaceNormal, roughness) * (occlusion-0.1), 0, 100);

                float4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, (1-roughness)*10);
                float4 cubemap = float4(DecodeHDR(skyData, unity_SpecCube0_HDR), 1.0) * occlusion;

                float3 SunLight;
                float3 plastic_light;
                float3 shadow_component;
                float3 ambient;

                if(_UseDSH==0)
                {
                    diffuseLight = 1;
                }

                if(_UseLM==0)
                {
                    atten = 1;

                }

                #ifndef LIGHTMAP_ON
                shadow_component = (diffuseLight*2)*atten;
                ambient = UNITY_LIGHTMODEL_AMBIENT*(1-(diffuseLight*atten));
                #endif

                #ifdef LIGHTMAP_ON
                fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap.xy));

                if (_UseLM==0)
                {
                    lm = (1,1,1);

                    if (_UseDSH==1)
                    {
                    shadow_component = diffuseLight;
                    ambient = UNITY_LIGHTMODEL_AMBIENT*(1-(diffuseLight*atten));
                    }
                }

                if (_UseLM==1)
                {
                #ifdef SHADOWS_SCREEN
                shadow_component = min((diffuseLight*2)*atten, lm);
                ambient = UNITY_LIGHTMODEL_AMBIENT*(1-(diffuseLight*atten));
                #else
                shadow_component = lm;
                ambient = 0;
                #endif
                }
                #endif

                float3 specular = (blinn*((diffuseLight*atten*occlusion))*(roughness+0.5));
                plastic_light = shadow_component + ambient + cubemap/20 + specular;

                float3 plasticMix = lerp(albedo*plastic_light, cubemap, clamp(fresnel/3, 0, 1));
                float3 metalsh = ((albedo)*cubemap)+((specular*10));

                float3 result = lerp(plasticMix, metalsh, metal);

                UNITY_APPLY_FOG(IN.fogCoord, result);

                if (_CheckNormals==1)
                {
                    result = lerp(result, _FFColor.rgb, _FFColor.a);
                }

                return result;
            }
            ENDCG
        }

        Pass
        {
            Name "BackFace"
            Tags { "LightMode" = "ForwardBase" }
            Lighting Off
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma only_renderers gles3 d3d11 // d3d11 for Editor. glcore gles metal vulkan d3d11_9x
            #pragma multi_compile_fwdbase
            #pragma shader_feature FOG_OFF FOG_LINEAR

            #include "UnityCG.cginc"


            struct appdata
            {
                float3 color : COLOR;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 color : COLOR;
                float4 vertex : SV_POSITION;
                UNITY_FOG_COORDS(1)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = float3(1, 0, 0);
                return o;
            }
            fixed3 frag (v2f IN) : SV_Target
            {
                return IN.color;
            }
            ENDCG
        }
    }
}
