Shader "Hisa/URP/VerticalScrollGradientRampBlend_DitherEase"
{
    Properties
    {
        _RampA           ("Gradient Ramp A", 2D) = "white" {}
        _RampB           ("Gradient Ramp B", 2D) = "white" {}
        _Blend           ("Blend (A->B)", Range(0,1)) = 0
        _TilingY         ("Tiling Y", Float) = 1
        _Offset          ("Scroll Offset", Float) = 0
        _RampTexelSize   ("1 / RampWidth", Float) = 0.00390625
        _DitherStrength  ("Dither Strength", Range(0,1)) = 0.35
    }

    // ===== SubShader #1 : 新タグ（Unity6/URP新系） =====
    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry" "RenderType"="Opaque" }
        Cull Off
        ZWrite On
        Blend Off

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode"="Universal2D" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex   vert
            #pragma fragment frag
            // 依存を減らすため UnityCG のみ（互換性重視）
            #include "UnityCG.cginc"

            sampler2D _RampA;
            sampler2D _RampB;
            float _Blend, _TilingY, _Offset, _RampTexelSize, _DitherStrength;

            struct Attributes { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings   { float4 posHCS:SV_POSITION; float2 uv:TEXCOORD0; float4 posClip:TEXCOORD1; };

            Varyings vert (Attributes IN)
            {
                Varyings o;
                o.posHCS  = UnityObjectToClipPos(IN.vertex);
                o.uv      = IN.uv;
                o.posClip = o.posHCS;
                return o;
            }

            float ScreenHashNoise(float2 pix)
            {
                float2 p = floor(pix);
                float n  = frac(sin(dot(p, float2(12.9898,78.233))) * 43758.5453);
                return n - 0.5;
            }

            fixed4 frag (Varyings IN) : SV_Target
            {
                // 画面ピクセル座標
                float  w     = max(IN.posClip.w, 1e-6);
                float2 ndc01 = (IN.posClip.xy / w) * 0.5 + 0.5;
                float2 pixel = ndc01 * _ScreenParams.xy;

                // 縦スクロール + ラップ
                float t = IN.uv.y * max(_TilingY, 1e-4) - _Offset;
                t -= floor(t);
                t = t * t * (3.0 - 2.0 * t);      // smootherstep

                // 端の縫い目回避
                float halfTexel = 0.5 * _RampTexelSize;
                float tx = saturate(t * (1.0 - 2.0 * halfTexel) + halfTexel);

                // ディザ（勾配スケール）
                float grad = fwidth(t);
                tx = saturate(tx + ScreenHashNoise(pixel) * _DitherStrength * grad);

                float2 suv = float2(tx, 0.5);
                fixed3 rgb = lerp(tex2D(_RampA, suv).rgb, tex2D(_RampB, suv).rgb, saturate(_Blend));
                return fixed4(rgb, 1);
            }
            ENDHLSL
        }
    }

    // ===== SubShader #2 : 旧タグ（URP旧系互換） =====
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" "RenderType"="Opaque" }
        Cull Off
        ZWrite On
        Blend Off

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode"="Universal2D" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _RampA;
            sampler2D _RampB;
            float _Blend, _TilingY, _Offset, _RampTexelSize, _DitherStrength;

            struct Attributes { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings   { float4 posHCS:SV_POSITION; float2 uv:TEXCOORD0; float4 posClip:TEXCOORD1; };

            Varyings vert (Attributes IN)
            {
                Varyings o;
                o.posHCS  = UnityObjectToClipPos(IN.vertex);
                o.uv      = IN.uv;
                o.posClip = o.posHCS;
                return o;
            }

            float ScreenHashNoise(float2 pix)
            {
                float2 p = floor(pix);
                float n  = frac(sin(dot(p, float2(12.9898,78.233))) * 43758.5453);
                return n - 0.5;
            }

            fixed4 frag (Varyings IN) : SV_Target
            {
                float  w     = max(IN.posClip.w, 1e-6);
                float2 ndc01 = (IN.posClip.xy / w) * 0.5 + 0.5;
                float2 pixel = ndc01 * _ScreenParams.xy;

                float t = IN.uv.y * max(_TilingY, 1e-4) - _Offset;
                t -= floor(t);
                t = t * t * (3.0 - 2.0 * t);

                float halfTexel = 0.5 * _RampTexelSize;
                float tx = saturate(t * (1.0 - 2.0 * halfTexel) + halfTexel);

                float grad = fwidth(t);
                tx = saturate(tx + ScreenHashNoise(pixel) * _DitherStrength * grad);

                float2 suv = float2(tx, 0.5);
                fixed3 rgb = lerp(tex2D(_RampA, suv).rgb, tex2D(_RampB, suv).rgb, saturate(_Blend));
                return fixed4(rgb, 1);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
