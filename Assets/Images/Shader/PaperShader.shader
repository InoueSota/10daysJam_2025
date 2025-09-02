Shader "Custom/PaperOverlay"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}          // カメラの描画結果（スクリーンカラー）
        _PaperTex ("Paper Texture", 2D) = "white" {}   // 紙のテクスチャ
        _PaperStrength ("Paper Strength", Range(0,1)) = 0.5
        _PaperScale ("Paper Scale", Float) = 1
        _PaperOffset ("Paper Offset", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            sampler2D _PaperTex;
            float4 _PaperTex_ST;

            float _PaperStrength;
            float _PaperScale;
            float4 _PaperOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 画面色
                fixed4 col = tex2D(_MainTex, i.uv);

                // 紙テクスチャ
                float2 paperUV = i.uv * _PaperScale + _PaperOffset.xy;
                fixed4 paper = tex2D(_PaperTex, paperUV);

                // 紙感をブレンド
                return col * (1 - _PaperStrength) + col * paper * _PaperStrength;
            }
            ENDCG
        }
    }
}