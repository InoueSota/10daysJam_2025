Shader "Hisa/Outline/URP_OutlineOnly_Extended_Safe"
{
    Properties
    {
        _OutlineColor ("Outline Color (tint)", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width (world units)", Range(0,1)) = 0.02
        _DistanceComp ("Distance Compensation", Range(0,20)) = 0.3

        _ZOffsetFactor ("Z Offset Factor", Range(-5,50)) = 0.0
        _ZOffsetUnits  ("Z Offset Units", Range(-200,200)) = 0.0

        _GradEnable ("Enable Gradient (0/1)", Range(0,1)) = 0
        _GradColorA ("Gradient Color A", Color) = (0,0,0,1)
        _GradColorB ("Gradient Color B", Color) = (1,1,1,1)
        _GradAxis   ("Gradient Axis (0=X,1=Y,2=Z)", Range(0,2)) = 1
        _GradScale  ("Gradient Scale", Float) = 1.0
        _GradOffset ("Gradient Offset", Float) = 0.0
    }

    // ====================== 共通コードはここ1か所だけ ======================
    CGINCLUDE
        #pragma target 3.0
        #pragma multi_compile_instancing
        #include "UnityCG.cginc"

        float4 _OutlineColor;
        float4 _GradColorA;
        float4 _GradColorB;

        float  _OutlineWidth;
        float  _DistanceComp;
        float  _ZOffsetFactor;
        float  _ZOffsetUnits;

        float  _GradEnable;
        float  _GradAxis;
        float  _GradScale;
        float  _GradOffset;

        struct Attributes {
            float3 positionOS : POSITION;
            float3 normalOS   : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Varyings {
            float4 positionCS : SV_POSITION;
            float3 posWS      : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        // (M^-1)^T 近似で法線をワールドへ
        float3 ToWorldNormal(float3 nOS)
        {
            float len2 = dot(nOS, nOS);
            float3 nFix = (len2 < 1e-8) ? float3(0,0,1) : nOS;
            float3 nWS = normalize(mul(nFix, (float3x3)unity_WorldToObject));
            return nWS;
        }

        Varyings OutlineVert(Attributes IN)
        {
            UNITY_SETUP_INSTANCE_ID(IN);
            Varyings OUT;
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

            float3 posWS = mul(unity_ObjectToWorld, float4(IN.positionOS, 1)).xyz;
            float3 nWS   = ToWorldNormal(IN.normalOS);

            // 距離補正（clip.w）
            float4 posCS = mul(UNITY_MATRIX_VP, float4(posWS, 1));
            float  comp  = 1.0 + _DistanceComp * saturate(posCS.w * 0.02);

            // 押し出し
            posWS += nWS * (_OutlineWidth * comp);

            OUT.positionCS = mul(UNITY_MATRIX_VP, float4(posWS, 1));
            OUT.posWS      = posWS;
            return OUT;
        }

        float4 OutlineFrag(Varyings IN) : SV_Target
        {
            float4 baseCol = _OutlineColor;
            if (_GradEnable > 0.5)
            {
                float axisValue = (_GradAxis < 0.5) ? IN.posWS.x :
                                  (_GradAxis < 1.5) ? IN.posWS.y :
                                                      IN.posWS.z;
                float t = saturate((axisValue + _GradOffset) * _GradScale);
                float4 grad = lerp(_GradColorA, _GradColorB, t);
                return baseCol * grad;
            }
            return baseCol;
        }
    ENDCG
    // ======================================================================

    //======================== SubShader #1 : 新タグ（Unity6/URP新） ========================
    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off

        // 3D/デフォルト向け
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ZTest LEqual
            Blend One Zero
            Offset [_ZOffsetFactor], [_ZOffsetUnits]

            HLSLPROGRAM
            #pragma vertex   OutlineVert
            #pragma fragment OutlineFrag
            ENDHLSL
        }

        // 2D Renderer 向け
        Pass
        {
            Name "Outline2D"
            Tags { "LightMode"="Universal2D" }
            Cull Front
            ZWrite On
            ZTest LEqual
            Blend One Zero
            Offset [_ZOffsetFactor], [_ZOffsetUnits]

            HLSLPROGRAM
            #pragma vertex   OutlineVert
            #pragma fragment OutlineFrag
            ENDHLSL
        }
    }

    //======================== SubShader #2 : 旧タグ（互換） ========================
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ZTest LEqual
            Blend One Zero
            Offset [_ZOffsetFactor], [_ZOffsetUnits]

            HLSLPROGRAM
            #pragma vertex   OutlineVert
            #pragma fragment OutlineFrag
            ENDHLSL
        }

        Pass
        {
            Name "Outline2D"
            Tags { "LightMode"="Universal2D" }
            Cull Front
            ZWrite On
            ZTest LEqual
            Blend One Zero
            Offset [_ZOffsetFactor], [_ZOffsetUnits]

            HLSLPROGRAM
            #pragma vertex   OutlineVert
            #pragma fragment OutlineFrag
            ENDHLSL
        }
    }

    FallBack Off
}
