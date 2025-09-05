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
            // UI�ł����Ȃ��u�����h�E�[�x
        }

        Pass
        {
            Name "ForwardUnlit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            // UI�Ɠ������X�R�b���Ɣ�����悤�ɃA���t�@�N���b�v���g��
            // �i���ۂ̃N���b�v��frag���� clip() �Ŏ��{�j

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            // URP�R�A
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
                float4 color      : COLOR;     // UI��Sprite�̒��_�J���[
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

                // Main: �ʏ��ST
                OUT.uvMain = TRANSFORM_TEX(IN.uv, _MainTex);

                // Mask: �Ɨ����� Tiling/Offset�iVector4: xy=tiling, zw=offset�j
                OUT.uvMask = IN.uv * _MaskTilingOffset.xy + _MaskTilingOffset.zw;

                OUT.color = IN.color;
                return OUT;
            }

            float smoothMask(float invMask, float th, float feather)
            {
                // feather=0�̏ꍇ�̈��S�����istep�ɋ߂������j
                float e1 = th + max(feather, 1e-5);
                return smoothstep(th, e1, invMask);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Main
                float4 mainCol = tex2D(_MainTex, IN.uvMain) * _Color * IN.color;

                // Mask�iA���Ȃ����R���g���^�p�ł�OK�F�v���W�F�N�g�ɍ��킹�ĕύX�j
                float maskA = tex2D(_MaskTex, IN.uvMask).a;

                // �t�}�X�N�F��(1) �� 0 / ��(0) �� 1
                float invMask = 1.0 - maskA;

                // �������l�{�t�F�U�[
                float factor = smoothMask(invMask, _Threshold, _Feather);

                // ����
                mainCol.rgb *= factor;
                mainCol.a   *= factor;

                // ���߂��m���ɔ���
                clip(mainCol.a - _Cutoff);

                return mainCol;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
