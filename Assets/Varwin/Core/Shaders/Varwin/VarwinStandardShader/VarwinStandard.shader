Shader "Varwin/Standard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        [Normal]_BumpMap ("Normal", 2D) = "bump" {}
        _BumpScale("Normal Scale", float) = 1
        _Metallic("Metallic", Range(0,1)) = 0
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _MetallicGlossMap("Metallic gloss map", 2D) = "white" {}
        _OcclusionMap("Occlusion map", 2D) = "white" {}
        _OcclusionStrength("Occlusion Strength", Range(0,1)) = 1
        _Cutoff("Cutoff", Range(0,1)) = 0.5
        _SmoothnessTextureChannel("SmoothnessTextureChannel", float) = 0
        
        _EmissionMap("Emission map", 2D) = "white" {}
        [HDR]_EmissionColor("Emission color", Color) = (1,1,1,1)

        _SrcBlend("SrcBlend", float) = 1
        _DstBlend("DstBlend", float)= 0
        _ZWrite("ZWrite", float) = 1
        _Mode("Mode", float) = 0
    }

    CustomEditor "Varwin.Core.VarwinStandardEditor"
    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 150
        Pass
        {
            Name "Base"
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Tags
            {
                "LightMode"="ForwardBase"
            }

            CGPROGRAM
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Varwin.cginc"

            #pragma multi_complie
            #pragma vertex VarwinVertexProgram
            #pragma multi_compile_instancing
            #pragma fragment frag
            #pragma shader_feature _EMISSION
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _ALPHABLEND_ON 
            #pragma multi_compile_fwdbase UNITY_LIGHT_PROBE_PROXY_VOLUME
            #pragma multi_compile_fog
            #pragma only_renderers gles3 d3d11 vulkan            

            float4 _LightColor0;
            sampler2D _MainTex;
            fixed4 _Color;
            sampler2D _BumpMap;
            float _BumpScale;
            float _Metallic;
            float _Glossiness;
            sampler2D _MetallicGlossMap;
            sampler2D _OcclusionMap;
            float _OcclusionStrength;
            float _Cutoff;
            sampler2D _EmissionMap;
            half4 _EmissionColor;

            fixed4 frag(VarwinFragData i) : SV_Target
            {
                half3 worldNormal = UnpackBumpNormal(tex2D(_BumpMap, i.uv), _BumpScale, i.worldNormal, i.worldTangent, i.worldBitangent);
                half3 worldViewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                half3 worldLightDir = normalize(_WorldSpaceLightPos0);

                half4 diffuse = tex2D(_MainTex, i.uv.xy) * _Color;
                half4 occlusionMap = tex2D(_OcclusionMap, i.uv.xy);
                half4 metallicGloss = tex2D(_MetallicGlossMap, i.uv.xy);

                half smoothness = _Glossiness * metallicGloss.a;
                half perceptualRoughness = 1.0 - smoothness;
                half occlusion = lerp(1, occlusionMap.g, _OcclusionStrength);
                half roughness = max(0.005, perceptualRoughness * perceptualRoughness);
                half metallic = metallicGloss.r * _Metallic;

                half3 reflectedDir = reflect(-worldViewDir, worldNormal);
                half4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, (1 - smoothness) * 10);
                half4 cubemap = fixed4(DecodeHDR(skyData, unity_SpecCube0_HDR), 1.0);
                half3 lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightMapUV.xy));
                
                half3 H = normalize(worldLightDir + worldViewDir);
                half NdotH = max(0.0, dot(worldNormal, H));
                half LdotH = max(0.0, dot(worldLightDir, H));
                half NdotL = max(0.0, dot(worldNormal, worldLightDir));
                half NdotV = max(0.0, dot(worldNormal, worldViewDir));

                half3 environment = i.ambient * occlusion;

                half3 directionalLight = _LightColor0 * LIGHT_ATTENUATION(i);               
                half3 specColor;
                half oneMinusReflectivity;

                DiffuseAndSpecularFromMetallic(diffuse.rgb, metallic, specColor, oneMinusReflectivity);

                half diffuseTerm = DisneyDiffuse(NdotV, NdotL, LdotH, perceptualRoughness) * NdotL * oneMinusReflectivity;
                half surfaceReduction = 1.0 / (roughness * roughness + 1.0);
                #ifndef _SPECULARHIGHLIGHTS_OFF
                half V = GGXGeometricShadowingFunction(NdotL, NdotV, roughness);
                half D = GGXNormalDistribution(roughness, NdotH) ;
                half specularTerm = V * D * UNITY_PI ;
                specularTerm = max(0, specularTerm * NdotL) ;
                specularTerm *= (any(specColor) ? 1.0 : 0.0);
                #else
                half specularTerm = 0;
                #endif
                half grazingTerm = saturate(smoothness + (1 - oneMinusReflectivity));

                half alpha = diffuse.a;
                half3 diffuseLight;
                #ifdef LIGHTMAP_ON
                diffuseLight = lerp(lightmap, 0, metallic);
                #else
                diffuseLight = (directionalLight * diffuseTerm + lerp(environment, 0, metallic));               
                #endif

                #ifndef _ALPHAPREMULTIPLY_ON 
                half3 color = diffuse.rgb * diffuseLight
                #else
                half3 color = diffuse.rgb * diffuseLight * alpha
                #endif
                    + saturate(specularTerm * directionalLight * FresnelTerm(specColor, LdotH)) 
                    + surfaceReduction * cubemap * occlusion * FresnelLerp(specColor, grazingTerm, NdotV);

                #ifdef _ALPHAPREMULTIPLY_ON 
                half targetAlpha = lerp(saturate(alpha + 0.1), 1, metallic);
                #else
                half targetAlpha = alpha;
                #endif
                half4 result = half4(color, targetAlpha);

                #ifdef _EMISSION
                result = saturate(result + tex2D(_EmissionMap, i.uv) * _EmissionColor);
                #endif
                
                #ifdef _ALPHATEST_ON
                clip(alpha - _Cutoff);
                #endif

                #if !defined(_ALPHABLEND_ON) && !defined(_ALPHAPREMULTIPLY_ON)
                result.a = 1;
                #endif
                
                UNITY_APPLY_FOG(i.fogCoord, result);
                return result;
            }
            ENDCG
        }

        Pass
        {
            Name "Lights"
            Tags
            {
                "LightMode"="ShadowCaster"
            }

            ZWrite On ZTest LEqual
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #pragma only_renderers gles3 d3d11 vulkan
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _ALPHABLEND_ON

            sampler3D _DitherMaskLOD;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Cutoff;
            sampler2D _MetallicGlossMap;
            float _Metallic;

            struct VarwinFragData
            {
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
                float2 uv : TEXCOORD1;
            };

            VarwinFragData vert(appdata_base v)
            {
                VarwinFragData o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(VarwinFragData i) : SV_Target
            {
                half alpha = tex2D(_MainTex, i.uv).a * _Color.a;
                half alphaRef;

                #ifdef _ALPHATEST_ON
                    clip(alpha - _Cutoff);
                #endif
                
                #ifdef _ALPHAPREMULTIPLY_ON 
                    half4 metallicGloss = tex2D(_MetallicGlossMap, i.uv.xy);
                    half metallic = metallicGloss.r * _Metallic;

                    #ifdef LOD_FADE_CROSSFADE
                        #define _LOD_FADE_ON_ALPHA
                        alpha *= unity_LODFade.y;
                    #endif
                    half targetAlpha = lerp(saturate(alpha + 0.1), 1, metallic);
                    alphaRef = tex3D(_DitherMaskLOD, float3(i.pos.xy*0.25,targetAlpha*0.9375)).a;
                    clip (alphaRef - 0.01);
                #endif

                #ifdef _ALPHABLEND_ON
                    #ifdef LOD_FADE_CROSSFADE
                        #define _LOD_FADE_ON_ALPHA
                        alpha *= unity_LODFade.y;
                    #endif
                    alphaRef = tex3D(_DitherMaskLOD, float3(i.pos.xy*0.25,alpha*0.9375)).a;
                    clip (alphaRef - 0.01);
                #endif


                return 0;
            }
            ENDCG
        }

        Pass
        {
            Name "Light support"
            Tags
            {
                "LightMode" = "ForwardAdd"
            }

            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } 
            ZWrite Off
            ZTest LEqual
            CGPROGRAM
            #pragma multi_compile_fwdadd_fullshadows

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Varwin.cginc"

            #pragma only_renderers gles3 d3d11 vulkan
            #pragma vertex VarwinVertexProgram
            #pragma fragment frag
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ALPHAPREMULTIPLY_ON 

            float4 _LightColor0;
            sampler2D _MainTex;
            fixed4 _Color;
            sampler2D _BumpMap;
            float _BumpScale;
            float _Metallic;
            float _Glossiness;
            sampler2D _MetallicGlossMap;
            float _OcclusionStrength;
            float _Cutoff;

            fixed4 frag(VarwinFragData i) : COLOR
            {
                half3 worldNormal = UnpackBumpNormal(tex2D(_BumpMap, i.uv), _BumpScale, i.worldNormal, i.worldTangent, i.worldBitangent);
                half3 worldViewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                half3 worldLightDir = normalize(_WorldSpaceLightPos0 - i.worldPos);

                half4 diffuse = tex2D(_MainTex, i.uv.xy) * _Color;
                half4 metallicGloss = tex2D(_MetallicGlossMap, i.uv.xy);

                half smoothness = _Glossiness * metallicGloss.a;
                half perceptualRoughness = 1.0 - smoothness;
                half roughness = max(0.005, perceptualRoughness * perceptualRoughness);
                half metallic = metallicGloss.r * _Metallic;

                half3 H = normalize(worldLightDir + worldViewDir);
                half NdotH = max(0.0, dot(worldNormal, H));
                half LdotH = max(0.0, dot(worldLightDir, H));
                half NdotL = max(0.0, dot(worldNormal, worldLightDir));
                half NdotV = max(0.0, dot(worldNormal, worldViewDir));

                half3 reflectedDir = reflect(-worldViewDir, worldNormal);
                half4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, roughness * 10);

                half3 light = _LightColor0.rgb * 2 * LIGHT_ATTENUATION(i);

                half3 specColor;
                half oneMinusReflectivity;

                DiffuseAndSpecularFromMetallic(diffuse, metallic, specColor, oneMinusReflectivity);

                half diffuseTerm = DisneyDiffuse(NdotV, NdotL, LdotH, perceptualRoughness) * NdotL * oneMinusReflectivity;

                #ifndef _SPECULARHIGHLIGHTS_OFF
                half V = GGXGeometricShadowingFunction(NdotL, NdotV, roughness);
                half D = GGXNormalDistribution(roughness, NdotH);
                half specularTerm = V * D * UNITY_PI;
                specularTerm = max(0, specularTerm * NdotL);
                #else
                half specularTerm = 0;
                #endif
                
                half alpha = diffuse.a;
                half3 color = diffuse.rgb * alpha * (light * diffuseTerm )
                    + saturate(specularTerm * light * FresnelTerm(specColor, LdotH));

                #ifdef _ALPHATEST_ON
                clip(alpha - _Cutoff);
                #endif

                half4 result = half4(color, lerp(alpha, 1, metallic));
                return result;

            }
            ENDCG
        }

        Pass
        {
            Name "META"
            Tags
            {
                "LightMode" = "Meta"
            }
            Cull Off
            CGPROGRAM
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardMeta.cginc"
            #pragma only_renderers gles3 d3d11 vulkan           
            
            float4 frag_meta2(v2f_meta i) : SV_Target
            {
                half4 diffuse = tex2D(_MainTex, i.uv.xy) * _Color; 
                half4 emission = tex2D(_EmissionMap, i.uv.xy) * _EmissionColor;
                half4 result = diffuse;
                #ifdef _EMISSION
                result += emission;
                #endif

                return result;
            }

            #pragma vertex vert_meta
            #pragma fragment frag_meta2
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
            ENDCG
        }
    }
}