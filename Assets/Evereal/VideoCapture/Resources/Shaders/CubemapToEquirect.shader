// Copyright (c) 2019-present Evereal. All rights reserved.

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VideoCapture/CubemapToEquirect"
{
  Properties
  {
    _MainTex("Cubemap (RGB)", CUBE) = "" {}
  }

  Subshader
  {
    Pass
    {
      ZTest Always Cull Off ZWrite Off
      Fog { Mode off }

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma fragmentoption ARB_precision_hint_fastest
      //#pragma fragmentoption ARB_precision_hint_nicest
      #include "UnityCG.cginc"

      #define PI    3.141592653589793
      #define TWOPI 6.283185307179587

      struct v2f
      {
        float4 pos : POSITION;
        float2 uv : TEXCOORD0;
      };

      samplerCUBE _MainTex;
      float4x4 _CubeTransform;

      v2f vert(appdata_img v)
      {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord.xy * float2(TWOPI, PI);
        return o;
      }

      // convert spherical coordinates (azimuth, elevation angles) to Cartesian coordinates (x, y, z) on sphere
      float3 sphericalToCartesian(float2 a)
      {
        float theta = a.y;
        float phi = a.x;
        float3 unit = float3(0,0,0);

        unit.x = sin(phi) * sin(theta) * -1;
        unit.y = cos(theta) * -1;
        unit.z = cos(phi) * sin(theta) * -1;

        return unit;
      }

      fixed4 frag(v2f i) : COLOR {
        float3 dir = sphericalToCartesian(i.uv);
        dir = mul(_CubeTransform, float4(dir, 1)).xyz;

        return texCUBE(_MainTex, dir);
      }
      ENDCG
    }
  }
  Fallback Off
}