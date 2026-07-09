Shader "BiTile/ChapterCompletedGlow"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.82, 0.25, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 0.82, 0.25, 1)
        _BaseIntensity ("Base Intensity", Float) = 0.25
        _FlowIntensity ("Flow Intensity", Float) = 1.5
        _FlowOffset ("Flow Offset", Range(0, 1)) = 0
        _FlowWidth ("Flow Width", Float) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows addshadow vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _GlowColor;
        float _BaseIntensity;
        float _FlowIntensity;
        float _FlowOffset;
        float _FlowWidth;

        struct Input
        {
            float2 uv_MainTex;
            float3 localPosition;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.localPosition = v.vertex.xyz;
        }

        void surf(Input i, inout SurfaceOutput o)
        {
            fixed4 tex = tex2D(_MainTex, i.uv_MainTex);
            float diagonalPosition = i.localPosition.y + i.localPosition.x * 0.55;
            float flowCenter = lerp(-0.6, 4.6, _FlowOffset);
            float flow = saturate(1.0 - abs(diagonalPosition - flowCenter) / _FlowWidth);
            flow = smoothstep(0.0, 1.0, flow);

            fixed3 color = tex.rgb * _Color.rgb;
            color += _GlowColor.rgb * _BaseIntensity;
            color += _GlowColor.rgb * flow * _FlowIntensity;

            o.Albedo = color;
            o.Alpha = tex.a * _Color.a;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
