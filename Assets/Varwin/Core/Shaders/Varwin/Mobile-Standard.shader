
Shader "Varwin/Mobile/Standard"
{
    Properties 
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _MetallicGlossMap ("MetallicGloss", 2D) = "white" {}
        [Normal]_BumpMap ("Normal Map", 2D) = "bump" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        UsePass "VertexLit/SHADOWCASTER"
        
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert_surf
            #pragma fragment frag_surf
            #pragma only_renderers gles3 d3d11 vulkan
            #pragma multi_compile_fwdbase
            #pragma shader_feature FOG_OFF FOG_LINEAR
            #pragma multi_compile_instancing
            
            #include "HLSLSupport.cginc"
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"


            sampler2D _MainTex;
            sampler2D _MetallicGlossMap;
            sampler2D _BumpMap;


            struct Input {
                fixed2 uv_MainTex;

                fixed3 viewDir;
                fixed3 invView;

                fixed3 tangent;
                fixed3 normal;
                fixed3 binormal;

                fixed3 normalWorld;
                fixed3 fresnelFac;
            };


            struct bdsmOutputStandard
            {
                fixed3 Albedo;
                fixed3 Normal;
                fixed3 MultiMap;
            };


            fixed3 UnpackTBN(fixed4 normalmap, fixed3 tangent, fixed3 binormal, fixed3 normalWorld)
            {
                // fixed3 localCoords = fixed3(2.0 * normalmap.a - 1.0, 
                    // 2.0 * normalmap.g - 1.0, 0.0);
                // localCoords.z = 1.0 - 0.5 * dot(localCoords, localCoords);

                fixed3 localCoords = UnpackNormal(normalmap);

                fixed3x3 local2WorldMatrix = fixed3x3(tangent, binormal, normalWorld);
                fixed3 result = normalize(mul(localCoords, local2WorldMatrix));

                return result;
            }


            fixed fresnelFactor(fixed3 viewDir, fixed3 surfaceNormal)
            {
                fixed fresnelFactor = 1-fixed(1*max(0.0, dot(normalize(-viewDir), surfaceNormal)));
                return fresnelFactor;
            }


            fixed3 blinnSpecular(fixed3 viewDir, fixed3 surfaceNormal, fixed roughness)
            {
                fixed3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                fixed3 H = normalize(viewDir + lightDirection);
                fixed3 specular = pow(max(0.0, dot(reflect(-lightDirection, surfaceNormal), viewDir)), (roughness)*50);
                return _LightColor0.rgb*(specular*2);
            }


            fixed3 diffuseReflection(fixed3 surfaceNormal)
            {
                fixed3 diffuseReflection = _LightColor0.rgb*0.58*max(0.0, dot(surfaceNormal, normalize(_WorldSpaceLightPos0.xyz)));
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            fixed4 _MainTex_ST;
            v2f_surf vert_surf (appdata_full v)
            {
                v2f_surf o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                fixed3 worldN = UnityObjectToWorldNormal(v.normal);

                #ifdef LIGHTMAP_ON
                o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                #endif

                fixed4x4 modelMatrix = unity_ObjectToWorld;
                fixed4x4 modelMatrixInverse = unity_WorldToObject;
                o.posWorld = mul(modelMatrix, v.vertex).xyz;

                o.normalWorld = UnityObjectToWorldNormal(v.normal);
                o.tangent = normalize(mul(modelMatrix, fixed4(v.tangent.xyz, 0.0)).xyz);
                o.binormal = normalize(cross(o.normalWorld, o.tangent) * v.tangent.w);

                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }


            fixed4 frag_surf (v2f_surf IN) : SV_Target
            {
                Input surfIN;
                surfIN.uv_MainTex = IN.pack0.xy;
                fixed4 AlbedoMap = tex2D(_MainTex, IN.pack0.xy);
                fixed4 MultiMap = tex2D(_MetallicGlossMap, IN.pack0.xy);
                fixed4 NormalMap = tex2D(_BumpMap, IN.pack0.xy);

                fixed3 viewDirection = normalize(_WorldSpaceCameraPos - IN.posWorld.xyz);
                fixed3 invView = normalize(IN.posWorld.xyz - _WorldSpaceCameraPos);

                fixed3 surfaceNormal = UnpackTBN(NormalMap, IN.tangent, IN.binormal, IN.normalWorld);

                fixed roughness = MultiMap.a;
                fixed metal = MultiMap.r;
                fixed occlusion = MultiMap.g;
                fixed3 albedo = AlbedoMap.rgb;

                fixed atten = SHADOW_ATTENUATION(IN);

                fixed fresnel = fresnelFactor(invView, surfaceNormal) * occlusion;
                fixed3 reflectedDir = reflect(invView, surfaceNormal);
                fixed3 diffuseLight = diffuseReflection(surfaceNormal);
                fixed3 blinn = clamp(blinnSpecular(viewDirection , surfaceNormal, roughness) * (occlusion-0.1), 0, 100);
                fixed4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, (1-roughness)*10);
                fixed4 cubemap = fixed4(DecodeHDR(skyData, unity_SpecCube0_HDR), 1.0);
                fixed4 skyData2 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, 7);
                fixed4 cubemap2 = fixed4(DecodeHDR(skyData2, unity_SpecCube0_HDR), 1.0);
                fixed3 shadow_component;
                fixed3 ambient;

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
                    ambient = 0;
                    #endif
                #endif

                fixed3 specular = (blinn*((diffuseLight*atten*occlusion))*(roughness+0.5));
                fixed3 plastic_light = shadow_component + ambient + cubemap/20 + specular;
                fixed3 plasticMix = lerp(albedo*plastic_light, cubemap, clamp(fresnel/3, 0, 1));
                
                fixed3 metalsh = ((albedo)*cubemap)+((specular*10));

                fixed3 result = lerp(plasticMix, metalsh, metal);

                UNITY_APPLY_FOG(IN.fogCoord, result);

                return float4(result, 1);
            }
            
            ENDCG
        }
    }
}
