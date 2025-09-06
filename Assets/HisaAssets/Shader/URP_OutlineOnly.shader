Shader "Hisa/Outline/URP_OutlineOnly_Extended"
{
    Properties
    {
        // ����
        _OutlineColor ("Outline Color (tint)", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width (world units)", Range(0,1)) = 0.02
        _DistanceComp ("Distance Compensation", Range(0,20)) = 0.3

        // �ǉ�: Z�I�t�Z�b�g�i�|���S���I�t�Z�b�g�j
        _ZOffsetFactor ("Z Offset Factor", Range(-5,5)) = 0.0
        _ZOffsetUnits  ("Z Offset Units", Range(-200,200)) = 0.0

        // �ǉ�: �O���f�[�V����
        _GradEnable ("Enable Gradient (0/1)", Range(0,1)) = 0
        _GradColorA ("Gradient Color A", Color) = (0,0,0,1)
        _GradColorB ("Gradient Color B", Color) = (1,1,1,1)
        _GradAxis   ("Gradient Axis (0=X,1=Y,2=Z)", Range(0,2)) = 1
        _GradScale  ("Gradient Scale", Float) = 1.0
        _GradOffset ("Gradient Offset", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ZTest LEqual

            // �� Z�������i�|���S���I�t�Z�b�g�j
            //   Factor�̓|���S���̎Γx�ˑ��AUnits�͌Œ�o�C�A�X�B
            Offset [_ZOffsetFactor], [_ZOffsetUnits]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
                float _DistanceComp;

                float _ZOffsetFactor;
                float _ZOffsetUnits;

                float _GradEnable;
                half4 _GradColorA;
                half4 _GradColorB;
                float _GradAxis;
                float _GradScale;
                float _GradOffset;
            CBUFFER_END

            struct Attributes {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 posWS      : TEXCOORD0; // �O���f�[�V�����p�Ƀ��[���h���W��n��
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varyings OUT;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 posWS = TransformObjectToWorld(IN.positionOS);
                float3 nWS   = normalize(TransformObjectToWorldNormal(IN.normalOS));

                // �����␳�i�������ōׂ������ɂ�������j
                float4 posCS  = TransformWorldToHClip(posWS);
                float  comp   = 1.0 + _DistanceComp * saturate(posCS.w * 0.02);

                // �@�������֖c���i�A�E�g���C�����j
                posWS += nWS * (_OutlineWidth * comp);

                OUT.positionCS = TransformWorldToHClip(posWS);
                OUT.posWS = posWS;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // ��{�F�i�e�B���g�j
                half4 baseCol = _OutlineColor;

                // �O���f�[�V����
                if (_GradEnable > 0.5)
                {
                    // ���I���i0=X, 1=Y, 2=Z�j
                    float axisValue = (_GradAxis < 0.5) ? IN.posWS.x :
                                      (_GradAxis < 1.5) ? IN.posWS.y :
                                                          IN.posWS.z;

                    // t���I�t�Z�b�g���X�P�[���Ő��K��
                    float t = saturate( (axisValue + _GradOffset) * _GradScale );

                    half4 grad = lerp(_GradColorA, _GradColorB, t);

                    // �e�B���g�Ə�Z�i_OutlineColor�őS�̂̃g�[�����ȒP�ɒ����ł���j
                    return baseCol * grad;
                }

                return baseCol;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
