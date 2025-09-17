Shader "Varwin/Unlit/UnlitTransparent"
{
    Properties
    {
        [PerRendererData]_Color("Color Tint", Color) = (1,1,1,1)
		[PerRendererData]_MainTex("Base (RGB) Alpha (A)", 2D) = "white"
    }
    SubShader
    {
        Lighting Off
        ZWrite Off
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha

        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        LOD 150

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma instancing_options lodfade

            #include "UnityCG.cginc"

            struct appdata
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO                 
            };

            sampler2D _MainTex;

            UNITY_INSTANCING_BUFFER_START(Props)
			    UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			    UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
			UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                float4 ST = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_ST);

                fixed4 col = tex2D(_MainTex, i.uv * ST.xy + ST.zw) * color;

                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
