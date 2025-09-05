Shader "Hisa/UI/InverseMaskURP_Unlit"
{
    Properties
    {
        _MainTex("Main Tex", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)

        _MaskTex("Mask Tex (white=hide, black=show)", 2D) = "black" {}
        _MaskTilingOffset("Mask TilingOffset (xy=tiling, zw=offset)", Vector) = (1,1,0,0)
        _Threshold("Mask Threshold", Range(0,1)) = 0.5
        _Feather("Mask Feather", Range(0,0.2)) = 0.02

        _Cutoff("Alpha Clip Threshold", Range(0,1)) = 0.001
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "IgnoreProjector"="True"
            "PreviewType"    = "Plane"
            // UIでも問題ないブレンド・深度
        }

        Pass
        {
            Name "ForwardUnlit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            // UIと同じくスコッっと抜けるようにアルファクリップを使う
            // （実際のクリップはfrag内の clip() で実施）

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            // URPコア
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _MaskTex;

            float4 _Color;

            float4 _MaskTilingOffset; // (tileX, tileY, offX, offY)
            float  _Threshold;
            float  _Feather;
            float  _Cutoff;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;     // UIやSpriteの頂点カラー
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uvMain      : TEXCOORD0;
                float2 uvMask      : TEXCOORD1;
                float4 color       : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Main: 通常のST
                OUT.uvMain = TRANSFORM_TEX(IN.uv, _MainTex);

                // Mask: 独立した Tiling/Offset（Vector4: xy=tiling, zw=offset）
                OUT.uvMask = IN.uv * _MaskTilingOffset.xy + _MaskTilingOffset.zw;

                OUT.color = IN.color;
                return OUT;
            }

            float smoothMask(float invMask, float th, float feather)
            {
                // feather=0の場合の安全処理（stepに近い挙動）
                float e1 = th + max(feather, 1e-5);
                return smoothstep(th, e1, invMask);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Main
                float4 mainCol = tex2D(_MainTex, IN.uvMain) * _Color * IN.color;

                // Mask（AがなければRを使う運用でもOK：プロジェクトに合わせて変更）
                float maskA = tex2D(_MaskTex, IN.uvMask).a;

                // 逆マスク：白(1) → 0 / 黒(0) → 1
                float invMask = 1.0 - maskA;

                // しきい値＋フェザー
                float factor = smoothMask(invMask, _Threshold, _Feather);

                // 合成
                mainCol.rgb *= factor;
                mainCol.a   *= factor;

                // 透過を確実に抜く
                clip(mainCol.a - _Cutoff);

                return mainCol;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
