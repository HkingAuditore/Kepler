Shader "CustomPostProcessing/Blur"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BlurSize("_BlurSize", range(0, 300)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }

        ZTest Off
        cull Off
        ZWrite Off

        Pass
        {
            Name "Horizontal"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex:POSITION;
                float2 uv:TEXCOORD0;
            };

            struct v2f
            {
                float4 pos:SV_POSITION;
                float2 uv[5]:TEXCOORD0;
            };

            uniform sampler2D _MainTex;
            //xxx_TexelSize 是用来访问xxx纹理对应的每个文素的大小 ，如一张512*512的图该值大小为1/512≈0.001953125
            uniform float4 _MainTex_TexelSize;
            uniform half _BlurSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                half2 uv = v.uv;
                o.uv[0] = uv;
                o.uv[1] = uv + _MainTex_TexelSize.xy * half2(_BlurSize, 0);
                o.uv[2] = uv + _MainTex_TexelSize.xy * half2(-_BlurSize, 0);
                o.uv[3] = uv + _MainTex_TexelSize.xy * half2(_BlurSize * 2, 0);
                o.uv[4] = uv + _MainTex_TexelSize.xy * half2(-_BlurSize * 2, 0);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = 0.4 * tex2D(_MainTex, i.uv[0]);
                col += 0.24 * tex2D(_MainTex, i.uv[1]);
                col += 0.24 * tex2D(_MainTex, i.uv[2]);
                col += 0.06 * tex2D(_MainTex, i.uv[3]);
                col += 0.06 * tex2D(_MainTex, i.uv[4]);
                return col;
            }
            ENDCG
        }

        Pass
        {
            Name "Vertical"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex:POSITION;
                float2 uv:TEXCOORD0;
            };

            struct v2f
            {
                float4 pos:SV_POSITION;
                float2 uv[5]:TEXCOORD0;
            };

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_TexelSize;
            uniform half _BlurSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                half2 uv = v.uv;
                o.uv[0] = uv;
                o.uv[1] = uv + _MainTex_TexelSize.xy * half2(0, _BlurSize);
                o.uv[2] = uv + _MainTex_TexelSize.xy * half2(0, -_BlurSize);
                o.uv[3] = uv + _MainTex_TexelSize.xy * half2(0, _BlurSize * 2);
                o.uv[4] = uv + _MainTex_TexelSize.xy * half2(0, -_BlurSize * 2);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = 0.4 * tex2D(_MainTex, i.uv[0]);
                col += 0.24 * tex2D(_MainTex, i.uv[1]);
                col += 0.24 * tex2D(_MainTex, i.uv[2]);
                col += 0.06 * tex2D(_MainTex, i.uv[3]);
                col += 0.06 * tex2D(_MainTex, i.uv[4]);
                return col;
            }
            ENDCG
        }

}
}