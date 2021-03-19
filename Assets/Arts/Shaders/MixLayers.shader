Shader "CustomPostProcessing/MixLayers"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MixTex1("Mix Texture1", 2D) = "white" {}
        _MixTex2("Mix Texture2", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _MixTex1;
            sampler2D _MixTex2;

            fixed4 frag (v2f i) : SV_Target
            {
                // fixed4 col = tex2D(_MainTex, i.uv);
                // fixed4 result = col;
                
                fixed4 layer0 = tex2D(_MixTex1, i.uv);
                fixed4 result = layer0;
                // result = result * (1-layer0.a) + layer0*(layer0.a);
                
                fixed4 layer1 = tex2D(_MixTex2, i.uv);
                result = result * (1-layer1.a) + layer1*(layer1.a);
                
                return result;
            }
            ENDCG
        }
    }
}
