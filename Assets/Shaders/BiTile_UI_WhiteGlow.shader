Shader "BiTile/UI/White Glow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowSize ("Glow Size", Float) = 3
        _GlowAlpha ("Glow Alpha", Range(0, 1)) = 0.7
        _BulgeStrength ("Bulge Strength", Range(0, 0.35)) = 0.12
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _GlowSize;
            float _GlowAlpha;
            float _BulgeStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centered = i.uv - 0.5;
                float radius = dot(centered, centered);
                float2 warpedUv = 0.5 + centered * (1.0 - radius * _BulgeStrength);
                float2 offset = _MainTex_TexelSize.xy * _GlowSize;
                float visible = step(0.0, warpedUv.x) * step(warpedUv.x, 1.0) * step(0.0, warpedUv.y) * step(warpedUv.y, 1.0);
                fixed4 baseColor = tex2D(_MainTex, warpedUv) * i.color * visible;
                float glow = tex2D(_MainTex, warpedUv + float2(offset.x, 0)).a;
                glow += tex2D(_MainTex, warpedUv + float2(-offset.x, 0)).a;
                glow += tex2D(_MainTex, warpedUv + float2(0, offset.y)).a;
                glow += tex2D(_MainTex, warpedUv + float2(0, -offset.y)).a;
                glow += tex2D(_MainTex, warpedUv + offset).a * 0.7;
                glow += tex2D(_MainTex, warpedUv - offset).a * 0.7;
                glow += tex2D(_MainTex, warpedUv + float2(offset.x, -offset.y)).a * 0.7;
                glow += tex2D(_MainTex, warpedUv + float2(-offset.x, offset.y)).a * 0.7;
                glow = saturate(glow * 0.16 * _GlowAlpha) * i.color.a;

                fixed3 glowColor = fixed3(0.85, 1.0, 0.92);
                fixed3 color = baseColor.rgb + glowColor * glow;
                float alpha = saturate(baseColor.a + glow);
                return fixed4(color, alpha);
            }
            ENDCG
        }
    }
}
