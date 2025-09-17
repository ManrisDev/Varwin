
Shader "Varwin/Mobile/Standard (Transparent)" // Boris Design Shader for Mobile
{
    Properties 
    {
        _Color("RGB", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        _MetallicGlossMap ("MetallicGloss", 2D) = "white" {}
        [Normal]_BumpMap ("Normal Map", 2D) = "bump" {}
    }

    SubShader 
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
                
        Pass 
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert_surf
            #pragma fragment frag_surf
            #pragma multi_compile_fwdbase
            #pragma shader_feature FOG_OFF FOG_LINEAR
            
            #include "HLSLSupport.cginc"
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"


            sampler2D _MainTex;
            sampler2D _MetallicGlossMap;
            sampler2D _BumpMap;
            float4 _Color;


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


            struct v2f_surf 
            {
                fixed4 pos : SV_POSITION;
                fixed2 pack0 : TEXCOORD0;

                #ifndef LIGHTMAP_ON
                fixed3 normal : NORMAL;
                #endif

                #ifdef LIGHTMAP_ON
                fixed2 lmap : TEXCOORD2;
                #endif

                fixed3 posWorld : TEXCOORD3;

                fixed3 normalWorld : TEXCOORD4;
                fixed3 binormal : TEXCOORD5;
                fixed3 tangent : TANGENT;

                SHADOW_COORDS(6)
                UNITY_FOG_COORDS(7)
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


            fixed4 frag_surf (v2f_surf IN) : SV_Target
            {
                Input surfIN;
                surfIN.uv_MainTex = IN.pack0.xy;
                float4 AlbedoMap = tex2D(_MainTex, IN.pack0.xy);
                float4 MultiMap = tex2D(_MetallicGlossMap, IN.pack0.xy);
                float4 NormalMap = tex2D(_BumpMap, IN.pack0.xy);

                float3 viewDirection = normalize(_WorldSpaceCameraPos - IN.posWorld.xyz);
                float3 invView = normalize(IN.posWorld.xyz - _WorldSpaceCameraPos);

                float3 surfaceNormal = UnpackTBN(NormalMap, IN.tangent, IN.binormal, IN.normalWorld);
                
                float roughness = MultiMap.a;
                float metal = MultiMap.r;
                float occlusion = MultiMap.g;
                float3 albedo = AlbedoMap.rgb * _Color.rgb;
                float transparency = AlbedoMap.a * _Color.a;

                fixed atten = SHADOW_ATTENUATION(IN);

                float fresnel = fresnelFactor(invView, surfaceNormal) * occlusion;
                float3 reflectedDir = reflect(invView, surfaceNormal);

                float3 diffuseLight = diffuseReflection(surfaceNormal);
                float3 blinn = clamp(blinnSpecular(viewDirection , surfaceNormal, roughness) * (occlusion-0.1), 0, 100);

                float4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, (1-roughness)*10);
                float4 cubemap = float4(DecodeHDR(skyData, unity_SpecCube0_HDR), 1.0);
                float4 skyData2 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, 7);
                float4 cubemap2 = float4(DecodeHDR(skyData2, unity_SpecCube0_HDR), 1.0);

                float3 SunLight;
                float3 plastic_light;
                float3 shadow_component;
                float3 ambient;
                float3 specular;

                #ifndef LIGHTMAP_ON
                shadow_component = (diffuseLight*2)*atten;
                ambient = UNITY_LIGHTMODEL_AMBIENT*(1-(diffuseLight*atten))+cubemap2;
                #endif

                #ifdef LIGHTMAP_ON
                fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap.xy));
                    #ifdef SHADOWS_SCREEN
                    shadow_component = min((diffuseLight*2)*atten, lm);
                    ambient = UNITY_LIGHTMODEL_AMBIENT*(1-(diffuseLight*atten))+cubemap2;
                    #else
                    shadow_component = lm;
                    ambient = 0;//UNITY_LIGHTMODEL_AMBIENT*(1-(diffuseLight*atten))+cubemap2;
                    #endif
                #endif

                specular = (blinn*((diffuseLight*atten*occlusion))*(roughness+0.5));
                plastic_light = shadow_component + ambient + cubemap/20 + specular;

                float3 plasticMix = lerp(albedo*plastic_light, cubemap, clamp(fresnel/3, 0, 1));
                float3 metalsh = ((albedo)*cubemap)+((specular*10));

                float3 result = lerp(plasticMix, metalsh, metal);

                UNITY_APPLY_FOG(IN.fogCoord, result);

                return fixed4(result, transparency);
            }
            ENDCG
        }
    }
}
