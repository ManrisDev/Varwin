float4 _MainTex_ST;

struct VarwinVertexData
{
    float4 vertex : POSITION;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
    float4 texcoord : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VarwinFragData
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 worldNormal : TEXCOORD1;
    float3 worldPos : TEXCOORD2;
    float3 worldTangent : TEXCOORD3;
    float3 worldBitangent : TEXCOORD4;
    float2 lightMapUV : TEXCOORD5;
    LIGHTING_COORDS(6, 7)
    UNITY_VERTEX_OUTPUT_STEREO
    UNITY_FOG_COORDS(8)
    half3 ambient : TEXCOORD9;
};

VarwinFragData VarwinVertexProgram(VarwinVertexData v)
{
    VarwinFragData o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
    o.worldNormal = UnityObjectToWorldNormal(v.normal);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.worldTangent = UnityObjectToWorldDir(v.tangent);
    o.worldBitangent = cross(o.worldNormal, o.worldTangent) * tangentSign;
    o.lightMapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.ambient = ShadeSH9(float4(o.worldNormal, 1));
    TRANSFER_SHADOW(o);
    TRANSFER_VERTEX_TO_FRAGMENT(o);
    UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}

half3 UnpackBumpNormal(half4 bumpMap, half bumpScale, half3 worldNormal, half3 worldTangent, half3 worldBitangent)
{
    half3 tspace0 = half3(worldTangent.x, worldBitangent.x, worldNormal.x);
    half3 tspace1 = half3(worldTangent.y, worldBitangent.y, worldNormal.y);
    half3 tspace2 = half3(worldTangent.z, worldBitangent.z, worldNormal.z);

    half3 tnormal = UnpackNormal(bumpMap);

    return normalize(lerp(worldNormal, half3(dot(tspace0, tnormal.xyz), dot(tspace1, tnormal.xyz), dot(tspace2, tnormal.xyz)), bumpScale));
}

half Square(half x)
{
    return x * x;
}

half Pow5(half x)
{
    return x * x * x * x * x;
}

half3 FresnelTerm(half3 F0, half cosA)
{
    half t = Pow5(1 - cosA);
    return F0 + (1 - F0) * t;
}

half3 FresnelLerp(half3 F0, half3 F90, float cosA)
{
    float t = Pow5(1 - cosA);
    return lerp(F0, F90, t);
}

half DisneyDiffuse(half NdotV, half NdotL, half LdotH, half perceptualRoughness)
{
    half fd90 = 0.5 + 2 * LdotH * LdotH * perceptualRoughness;
    half lightScatter = (1 + (fd90 - 1) * Pow5(1 - NdotL));
    half viewScatter = (1 + (fd90 - 1) * Pow5(1 - NdotV));

    return lightScatter * viewScatter;
}

half OneMinusReflectivityFromMetallic(half metallic)
{
    half oneMinusDielectricSpec = unity_ColorSpaceDielectricSpec.a;
    return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}

half3 DiffuseAndSpecularFromMetallic(half3 albedo, half metallic, out half3 specColor, out half oneMinusReflectivity)
{
    specColor = lerp(unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
    oneMinusReflectivity = OneMinusReflectivityFromMetallic(metallic);
    return albedo * oneMinusReflectivity;
}

half GGXNormalDistribution(half roughness, half NdotH)
{
    half roughnessSqr = roughness * roughness;
    half NdotHSqr = NdotH * NdotH;
    half TanNdotHSqr = (1 - NdotHSqr) / NdotHSqr;
    return (1.0 / 3.1415926535) * Square(roughness / (NdotHSqr * (roughnessSqr + TanNdotHSqr)));
}

half GGXGeometricShadowingFunction(half NdotL, half NdotV, half roughness)
{
    half roughnessSqr = roughness * roughness;
    half NdotLSqr = NdotL * NdotL;
    half NdotVSqr = NdotV * NdotV;

    half SmithL = (2 * NdotL) / (NdotL + sqrt(roughnessSqr + (1 - roughnessSqr) * NdotLSqr));
    half SmithV = (2 * NdotV) / (NdotV + sqrt(roughnessSqr + (1 - roughnessSqr) * NdotVSqr));

    half Gs = (SmithL * SmithV);
    return Gs;
}
