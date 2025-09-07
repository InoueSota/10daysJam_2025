Shader "Hisa/URP/VerticalScrollGradientRampBlend_DitherEase"
{
    Properties
    {
        _RampA           ("Gradient Ramp A", 2D) = "white" {}
        _RampB           ("Gradient Ramp B", 2D) = "white" {}
        _Blend           ("Blend (A->B)", Range(0,1)) = 0
        _TilingY         ("Tiling Y", Float) = 1
        _Offset          ("Scroll Offset", Float) = 0
        _RampTexelSize   ("1 / RampWidth", Float) = 0.00390625 // 1/256 を初期値
        _DitherStrength  ("Dither Strength", Range(0,1)) = 0.35
    }

    SubShader
    {
        // 不透明・URP
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalRenderPipeline" }
        LOD 100

        Cull Off
        ZWrite On
        Blend Off

        Pass
        {
            Name "Forward"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Blend;
                float _TilingY;
                float _Offset;
                float _RampTexelSize;   // C#から 1.0 / rampWidth を設定
                float _DitherStrength;  // 0..1
            CBUFFER_END

            TEXTURE2D(_RampA); SAMPLER(sampler_RampA);
            TEXTURE2D(_RampB); SAMPLER(sampler_RampB);

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };
            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float2 screenXY    : TEXCOORD1; // 画面ピクセル座標（ディザ用）
            };

            Varyings vert (Attributes IN)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.uv = IN.uv;

                // スクリーン座標（ピクセル）を計算
                float4 sp = ComputeScreenPos(o.positionHCS);
                float2 ndc = sp.xy / max(sp.w, 1e-6);    // 0..1
                o.screenXY = ndc * _ScreenParams.xy;     // ピクセル座標
                return o;
            }

            // 8x8 Bayer マトリクス（オーダードディザ）
            float bayer8x8(int2 p)
            {
                // 0..63
                static const int m[64] = {
                     0,32, 8,40, 2,34,10,42,
                    48,16,56,24,50,18,58,26,
                    12,44, 4,36,14,46, 6,38,
                    60,28,52,20,62,30,54,22,
                     3,35,11,43, 1,33, 9,41,
                    51,19,59,27,49,17,57,25,
                    15,47, 7,39,13,45, 5,37,
                    63,31,55,23,61,29,53,21
                };
                int idx = ((p.y & 7) << 3) | (p.x & 7);
                return (m[idx] / 64.0) - 0.5;   // -0.5 .. +0.5
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // ---- 縦スクロール座標（下→上）+ 手動ラップ ----
                float t = IN.uv.y * max(_TilingY, 1e-4) - _Offset;
                t = t - floor(t);                         // 0..1 に折り返し

                // ---- easeInOut（smootherstep）----
                t = t * t * (3.0 - 2.0 * t);

                // ---- 端の縫い目回避：半テクセル内側だけをサンプル ----
                float halfTexel = 0.5 * _RampTexelSize;   // 1 / (2 * width)
                // tx = t を [0.5/width, 1-0.5/width] へ射影
                float tx = mad(t, (1.0 - 2.0 * halfTexel), halfTexel);

                // ---- バンディング対策：Bayer ディザ（強度は fwidth にスケール）----
                int2 ip = int2(IN.screenXY + 0.5);
                float d = bayer8x8(ip);
                float g = fwidth(t);                      // 勾配に応じて加減
                tx = saturate(tx + d * _DitherStrength * g);

                float2 suv = float2(tx, 0.5);

                float4 colA = SAMPLE_TEXTURE2D(_RampA, sampler_RampA, suv);
                float4 colB = SAMPLE_TEXTURE2D(_RampB, sampler_RampB, suv);
                float4 col  = lerp(colA, colB, saturate(_Blend));

                return float4(col.rgb, 1);                // 不透明
            }
            ENDHLSL
        }
    }
}
