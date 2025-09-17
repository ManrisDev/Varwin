Shader "Unlit/Varwin/HeadCollisionFade"
{
    Properties
    {
        _Color("Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent+1998" "Queue"="Transparent+1998"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Front
        ZTest Always
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float3 worldDir : TEXCOORD1;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half3 _WorldDirection;
            half4 _Color;
            half _Force;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldDir = UnityObjectToWorldDir(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half dotFactor = dot(_WorldDirection, i.worldDir);

                half alpha = saturate(1.0 -  pow(saturate(dotFactor), 3.0 * _Force)) * _Force;
                return fixed4(_Color.xyz, alpha);
            }
            ENDCG
        }
    }
}
