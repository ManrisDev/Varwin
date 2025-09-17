Shader "Varwin/Effects/ShadedWireframe"
{
    Properties
    {
        _MainColor ("Main color", Color) = (0,0,0,0)
        _ShadedTransparency("Shaded transparency", FLOAT) = 0.5
        _LineThickness ("Line thickness", FLOAT) = 3
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha //TODO stop alpha from multiplying
        Pass
        {
            ZWrite Off
            Offset 0, -500

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag Lambert

            #include "UnityCG.cginc"

            uniform float _LineThickness;
            uniform fixed4 _MainColor;
            uniform float _ShadedTransparency;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2_g
            {
                float4 projection_space_vertex : SV_POSITION;
                float4 world_space_position : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct g2_f
            {
                float4 projection_space_vertex : SV_POSITION;
                float4 world_space_position : TEXCOORD0;
                float4 dist : TEXCOORD1;
            };

            v2_g vert(const appdata v)
            {
                v2_g o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.projection_space_vertex = UnityObjectToClipPos(v.vertex);
                o.world_space_position = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2_g i[3], inout TriangleStream<g2_f> triangle_stream)
            {
                const float2 p0 = i[0].projection_space_vertex.xy / i[0].projection_space_vertex.w;
                const float2 p1 = i[1].projection_space_vertex.xy / i[1].projection_space_vertex.w;
                const float2 p2 = i[2].projection_space_vertex.xy / i[2].projection_space_vertex.w;

                const float2 edge0 = p2 - p1;
                const float2 edge1 = p2 - p0;
                const float2 edge2 = p1 - p0;

                const float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
                const float wire_thickness = 800 - _LineThickness;

                g2_f o;
                o.world_space_position = i[0].world_space_position;
                o.projection_space_vertex = i[0].projection_space_vertex;
                o.dist.xyz = float3(area / length(edge0), 0.0, 0.0) * o.projection_space_vertex.w * wire_thickness;
                o.dist.w = 1.0 / o.projection_space_vertex.w;
                triangle_stream.Append(o);

                o.world_space_position = i[1].world_space_position;
                o.projection_space_vertex = i[1].projection_space_vertex;
                o.dist.xyz = float3(0.0, area / length(edge1), 0.0) * o.projection_space_vertex.w * wire_thickness;
                o.dist.w = 1.0 / o.projection_space_vertex.w;
                triangle_stream.Append(o);

                o.world_space_position = i[2].world_space_position;
                o.projection_space_vertex = i[2].projection_space_vertex;
                o.dist.xyz = float3(0.0, 0.0, area / length(edge2)) * o.projection_space_vertex.w * wire_thickness;
                o.dist.w = 1.0 / o.projection_space_vertex.w;
                triangle_stream.Append(o);
            }

            fixed4 frag(const g2_f IN) : COLOR
            {
                const float d = min(IN.dist.x, min(IN.dist.y, IN.dist.z));
                const float i = exp2(-(1 / _LineThickness) * d * d);

                float camDist = clamp(length(IN.world_space_position - _WorldSpaceCameraPos), 1, 1000);

                float interpolationFactor = clamp(pow(i, 1 / camDist) * _LineThickness, 0, 1);
                
                return lerp(float4(_MainColor.rgb, 1) * _ShadedTransparency,
                            float4(1.0 - _MainColor.r, 1.0 - _MainColor.g, 1.0 - _MainColor.b, 1), interpolationFactor);
            }
            ENDCG
        }
    }
    Fallback "Transparent/VertexLit"
}