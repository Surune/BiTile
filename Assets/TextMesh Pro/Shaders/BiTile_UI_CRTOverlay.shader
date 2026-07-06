Shader "BiTile/UI/CRT Overlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _ScanlineDensity ("Scanline Density", Float) = 360
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 0.72
        _FineLineStrength ("Fine Line Strength", Range(0, 1)) = 0.85
        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.88
        _FlickerStrength ("Flicker Strength", Range(0, 1)) = 0.08
        _CurvatureStrength ("Curvature Strength", Range(0, 0.25)) = 0.18
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float4 screenPosition : TEXCOORD1;
            };

            float _ScanlineDensity;
            float _ScanlineStrength;
            float _FineLineStrength;
            float _VignetteStrength;
            float _FlickerStrength;
            float _CurvatureStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.screenPosition = ComputeScreenPos(o.vertex);
                return o;
            }

            float Random(float2 value)
            {
                return frac(sin(dot(value, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenUv = i.screenPosition.xy / i.screenPosition.w;
                float2 centered = screenUv - 0.5;

                float scanlineCenter = abs(frac(screenUv.y * _ScanlineDensity) - 0.5);
                float scanline = 1.0 - smoothstep(0.03, 0.07, scanlineCenter);
                float fineLine = 1.0 - smoothstep(0.008, 0.02, scanlineCenter);
                float radius = dot(centered, centered);
                float vignette = saturate(radius * 2.45);
                float rim = smoothstep(0.16, 0.42, radius);
                float horizontalBend = smoothstep(0.18, 0.48, abs(centered.x));
                float verticalBend = smoothstep(0.2, 0.5, abs(centered.y));
                float noise = Random(screenUv * _ScreenParams.xy + _Time.yy);
                float flicker = sin(_Time.y * 55.0) * 0.5 + 0.5;

                float alpha = scanline * _ScanlineStrength;
                alpha += fineLine * _FineLineStrength;
                alpha += vignette * _VignetteStrength;
                alpha += rim * _CurvatureStrength;
                alpha += (horizontalBend + verticalBend) * _CurvatureStrength;
                alpha += noise * 0.035;
                alpha += flicker * _FlickerStrength;

                fixed3 tint = fixed3(0.0, 0.005, 0.0);
                return fixed4(tint, saturate(alpha) * i.color.a);
            }
            ENDCG
        }
    }
}
