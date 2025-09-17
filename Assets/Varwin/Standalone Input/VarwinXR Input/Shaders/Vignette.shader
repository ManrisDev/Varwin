Shader "Unlit/Varwin/Vignette"
{
    Properties
    {
        _Color("Color", Color) = (0,0,0,0)
        _Force("Force", float) = 0
        [KeywordEnum(Off, Weak, Medium, Strong)]_Type("Type", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent+1999" "Queue"="Transparent+1999"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Front
        ZTest Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 localPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half4 _Color;
            half _Force;
            half _Type;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half3 forward = half3(0, 0, 1);
                half sinForce = sin(saturate(_Force) * UNITY_PI / 2);
                half force = sinForce * (_Type / 10 + 0.5);
                half angle = (1 - force) * UNITY_PI / 2 ;
                half currentAngle = acos(dot(normalize(i.localPos.xyz), forward));
                half alpha = saturate((currentAngle - angle) * 10) * pow(sinForce, 0.1);
                return _Color * alpha;
            }
            ENDCG
        }
    }
}
