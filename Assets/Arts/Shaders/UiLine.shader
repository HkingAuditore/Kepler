Shader "Line/UILine"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [HDR]_Color("Color",Color) = (0,0,0,0)
        _Speed ("Speed",Range(0,1)) = .5
        _Density("Density",float) = 30
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Transparent"
        }
        ZTest Off
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        CGPROGRAM
        #pragma surface surf NoLight vertex:vert alpha noforwardadd

        //光照方程，名字为Lighting接#pragma suface后的光照方程名称 
  		//lightDir :顶点到光源的单位向量
		//viewDir  :顶点到摄像机的单位向量   
		//atten	   :关照的衰减系数
        float4 LightingNoLight(SurfaceOutput s, float3 lightDir, half3 viewDir, half atten)
        {
            float4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;
            return c;
        }

        sampler2D _MainTex;
        fixed4 _SelfCol;
        fixed4 _Color;

        fixed _Speed;
        fixed _Density;


        struct Input
        {
            float2 uv_MainTex;
            float4 vertColor;
        };

        void vert(inout appdata_full v, out Input o)
        {
            o.vertColor = v.color;
            o.uv_MainTex = v.texcoord;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed2 uv = float2(IN.uv_MainTex.x  * _Density ,IN.uv_MainTex.y) + float2((IN.uv_MainTex.x- _Time.y * _Speed )+1,0);
            half4 c = tex2D(_MainTex, frac(uv));
            o.Alpha = IN.vertColor.a * c.a;
            // o.Alpha =1;
            o.Albedo = c * _Color;
            // o.Albedo = float3(uv.x,uv.x,uv.x);
        }
        ENDCG
    }
    FallBack "Diffuse"
}