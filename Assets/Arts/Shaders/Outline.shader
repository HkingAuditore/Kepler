Shader "CustomPostProcessing/Outline"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _EdgeSize("Edge Size",Range(0.3,1)) = 0.5
        _EdgeOnly("Edge Only",Float) = 1
        _EdgeColor("Edge Color",Color) = (0,0,0,0)
        _BackgroundColor("Background Color",Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        Pass
        {
            ZTest always
            Cull off
            ZWrite off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture;
            half _EdgeSize;
            half4 _CameraDepthTexture_TexelSize;
            fixed _EdgeOnly;
            fixed4 _EdgeColor;
            fixed4 _BackgroundColor;
            sampler2D _MainTex;

            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 scrPos:TEXCOORD0;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.scrPos = ComputeScreenPos(o.pos);


                return o;
            }

            fixed GetDepthCol(v2f i,int idx)
            {
                half4 size = half4(_CameraDepthTexture_TexelSize.x * (1/_EdgeSize),
                                   _CameraDepthTexture_TexelSize.y* (1/_EdgeSize),
                                   _CameraDepthTexture_TexelSize.z * _EdgeSize,
                                   _CameraDepthTexture_TexelSize.w * _EdgeSize);
                return Linear01Depth(
                    tex2Dproj(
                        _CameraDepthTexture, UNITY_PROJ_COORD(
                            (i.scrPos + size  * half4((idx%3)-1,-((idx/3)-1),0,0))
                            )
                        ).r);
            }
            
            half EdgeDetect(v2f i)
            {
                const half Gx[9] = {
                    - 1,0,1,
                    - 2,0,2,
                    - 1,0,1
                };
                const half Gy[9] = {
                    -1,-2,-1,
                     0, 0, 0,
                     1, 2, 1
                };
                //横向梯度值
                half graX = 0;
                //纵向梯度值
                half graY = 0;

                for (int it = 0; it < 9; it++)
                {
                    graX += Gx[it] * GetDepthCol(i, it);
                    graY += Gy[it] * GetDepthCol(i, it);
                }
                
                return abs(graX) + abs(graY);
            }

            //Fragment Shader
            half4 frag(v2f i) : SV_Target
            {
                half edge = EdgeDetect(i);
                float depthValue = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
                fixed4 withEdgeColor = lerp( depthValue, _EdgeColor, edge);
                fixed4 onlyEdgeColor = lerp( _BackgroundColor, _EdgeColor, edge);
                fixed4 color = lerp(withEdgeColor, onlyEdgeColor, _EdgeOnly);
                
                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}