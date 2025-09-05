Shader "Hisa/Outline/URP_OutlineOnly"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width (world units)", Range(0,0.1)) = 0.02
        _DistanceComp ("Distance Compensation", Range(0,2)) = 0.3
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

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
                float _DistanceComp;
            CBUFFER_END

            struct Attributes {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varyings OUT;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 posWS = TransformObjectToWorld(IN.positionOS);
                float3 nWS   = normalize(TransformObjectToWorldNormal(IN.normalOS));

                // ãóó£ï‚ê≥Åiâìãóó£Ç≈ç◊Ç≠å©Ç¶Ç…Ç≠Ç≠Ç∑ÇÈÅj
                float4 posCS  = TransformWorldToHClip(posWS);
                float  comp   = 1.0 + _DistanceComp * saturate(posCS.w * 0.02);

                posWS += nWS * (_OutlineWidth * comp);
                OUT.positionCS = TransformWorldToHClip(posWS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
