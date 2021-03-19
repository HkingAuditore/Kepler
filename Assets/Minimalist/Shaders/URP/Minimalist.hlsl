#ifndef URP_MINIMALIST_INCLUDED
    #define URP_MINIMALIST_INCLUDED
    
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

    //region Structs
    struct vertexInput
    {
        half4  positionOS: POSITION;
        half3   normalOS: NORMAL;
        half4  tangentOS: TANGENT;
        half3   vColor: COLOR;
        half4  uv0: TEXCOORD0;
        half4  uv1: TEXCOORD1;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    struct vertexOutput
    {
        half4  positionCS: SV_POSITION;
        half4  positionWSFF: TEXCOORD2;
        half3   normalWS: TEXCOORD3;
        half3 customLighting: COLOR0;
        
        #if TEXTUREMODULE_ON
            half2  uv: TEXCOORD0;
        #endif
        
        #if AO_ON_UV1 || AO_ON_UV0
            half2  aouv: TEXCOORD1;
        #endif
        
        #if LIGHTMAP_ADD || LIGHTMAP_MULTIPLY || LIGHTMAP_AO
            half2 lightmapUV: TEXCOORD4;
        #endif
        
        #if SHADOW_ON
            #ifdef _MAIN_LIGHT_SHADOWS
                half4  shadowCoord: TEXCOORD5;
            #endif
        #endif
        
        #if USERIM
            half rimCol: COLOR1;
        #endif
    };
    //endregion

    //region Direction vector constants
    static const half3 FrontDir = half3(0, 0, 1);
    static const half3 BackDir = half3(0, 0, -1);
    static const half3 LeftDir = half3(1, 0, 0);
    static const half3 RightDir = half3(-1, 0, 0);
    static const half3 TopDir = half3(0, 1, 0);
    static const half3 BottomDir = half3(0, -1, 0);
    static const half3 whiteColor = half3(1, 1, 1);
    //endregion
    
    ///////////////////////////////////////////////////////////////////////////////
    //                  Vertex and Fragment functions                            //
    ///////////////////////////////////////////////////////////////////////////////
    vertexOutput vert(vertexInput input)
    {
        vertexOutput output;
        UNITY_SETUP_INSTANCE_ID(input);
        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
        
        output.positionCS = vertexInput.positionCS;
        output.positionWSFF = half4(vertexInput.positionWS, 0);
        output.normalWS = vertexNormalInput.normalWS;
        
        //Mapping Texture
        #if TEXTUREMODULE_ON
            output.uv = TRANSFORM_TEX(input.uv0, _MainTexture);
        #endif
        
        //Mapping AO
        #if AO_ON_UV0
            output.aouv = TRANSFORM_TEX(input.uv0, _AOTexture);
        #endif
        #if AO_ON_UV1
            output.aouv = TRANSFORM_TEX(input.uv1, _AOTexture);
        #endif
        
        half3 GradNorm = lerp(vertexNormalInput.normalWS, input.normalOS, _GradientSpace);
        
        //Calculating custom shadings
        half3 colorFront, colorBack, colorLeft, colorRight, colorTop, colorDown;
        half dirFront = max(dot(GradNorm, FrontDir), 0.0);
        half dirBack = max(dot(GradNorm, BackDir), 0.0);
        half dirLeft = max(dot(GradNorm, LeftDir), 0.0);
        half dirRight = max(dot(GradNorm, RightDir), 0.0);
        half dirTop = max(dot(GradNorm, TopDir), 0.0);
        half dirBottom = max(dot(GradNorm, BottomDir), 0.0);
        
        colorFront = input.vColor;
        colorBack = input.vColor;
        colorLeft = input.vColor;
        colorRight = input.vColor;
        colorTop = input.vColor;
        colorDown = input.vColor;
        
        
        #if FRONTSOLID
            colorFront = _Color1_F;
        #endif
        #if BACKSOLID
            colorBack = _Color1_B;
        #endif
        #if LEFTSOLID
            colorLeft = _Color1_L;
        #endif
        #if RIGHTSOLID
            colorRight = _Color1_R;
        #endif
        #if TOPSOLID
            colorTop = _Color1_T;
        #endif
        #if BOTTOMSOLID
            colorDown = _Color1_D;
        #endif
        
        half3 GradPos = lerp(vertexInput.positionWS.xyz, input.positionOS.xyz, _GradientSpace).xyz;
        
        #if FRONTGRADIENT
            half RotatedGrad_F = (GradPos.x - _GradientYStartPos_F.x) * sin(_Rotation_F / 57.32) + (GradPos.y - _GradientYStartPos_F.y) * cos(_Rotation_F / 57.32);
            half GradientFactor_F = saturate(RotatedGrad_F / - _GradientHeight_F);
            colorFront = lerp(_Color1_F, _Color2_F, GradientFactor_F);
        #endif
        #if BACKGRADIENT
            half RotatedGrad_B = (GradPos.x - _GradientYStartPos_B.x) * sin(_Rotation_B / 57.32) + (GradPos.y - _GradientYStartPos_B.y) * cos(_Rotation_B / 57.32);
            half GradientFactor_B = saturate(RotatedGrad_B / - _GradientHeight_B);
            colorBack = lerp(_Color1_B, _Color2_B, GradientFactor_B);
        #endif
        #if LEFTGRADIENT
            half RotatedGrad_L = (GradPos.z - _GradientYStartPos_L.x) * sin(_Rotation_L / 57.32) + (GradPos.y - _GradientYStartPos_L.y) * cos(_Rotation_L / 57.32);
            half GradientFactor_L = saturate(RotatedGrad_L / - _GradientHeight_L);
            colorLeft = lerp(_Color1_L, _Color2_L, GradientFactor_L);
        #endif
        #if RIGHTGRADIENT
            half RotatedGrad_R = (GradPos.z - _GradientYStartPos_R.x) * sin(_Rotation_R / 57.32) + (GradPos.y - _GradientYStartPos_R.y) * cos(_Rotation_R / 57.32);
            half GradientFactor_R = saturate(RotatedGrad_R / - _GradientHeight_R);
            colorRight = lerp(_Color1_R, _Color2_R, GradientFactor_R);
        #endif
        #if TOPGRADIENT
            half RotatedGrad_T = (GradPos.z - _GradientXStartPos_T.x) * cos(_Rotation_T / 57.32) + (GradPos.x - _GradientXStartPos_T.y) * sin(_Rotation_T / 57.32);
            half GradientFactor_T = saturate(RotatedGrad_T / - _GradientHeight_T);
            colorTop = lerp(_Color1_T, _Color2_T, GradientFactor_T);
        #endif
        #if BOTTOMGRADIENT
            half RotatedGrad_D = (GradPos.z - _GradientXStartPos_D.x) * cos(_Rotation_D / 57.32) + (GradPos.x - _GradientXStartPos_D.y) * sin(_Rotation_D / 57.32);
            half GradientFactor_D = saturate(RotatedGrad_D / - _GradientHeight_D);
            colorDown = lerp(_Color1_D, _Color2_D, GradientFactor_D);
        #endif
        
        
        half3 Maincolor;
        #if DONTMIX
            Maincolor = colorFront * dirFront + colorBack * dirBack + colorLeft * dirLeft + colorRight * dirRight + colorTop * dirTop + colorDown * dirBottom;
        #else
            Maincolor = lerp(colorFront, whiteColor, 1 - dirFront) * lerp(colorBack, whiteColor, 1 - dirBack) * lerp(colorLeft, whiteColor, 1 - dirLeft) * lerp(colorRight, whiteColor, 1 - dirRight) * lerp(colorTop, whiteColor, 1 - dirTop) * lerp(colorDown, whiteColor, 1 - dirBottom);
        #endif
        output.customLighting = Maincolor + (_AmbientColor * _AmbientPower);
        
        //Lightmap
        #if LIGHTMAP_ON
            #if	LIGHTMAP_ADD || LIGHTMAP_MULTIPLY || LIGHTMAP_AO
                output.lightmapUV = input.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
            #endif
        #endif
        
        //Apply Unity fog
        #if UNITY_FOG
            half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
            output.positionWSFF = half4(vertexInput.positionWS, fogFactor);
        #endif
        
        //Realtime shadow
        #if SHADOW_ON
            #ifdef _MAIN_LIGHT_SHADOWS
                output.shadowCoord = GetShadowCoord(vertexInput);
            #endif
        #endif
        
        #if USERIM
            half3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - vertexInput.positionWS);
            output.rimCol = pow(abs(1 - dot(vertexNormalInput.normalWS, viewDirectionWS)), _RimPower);
        #endif
        
        return output;
    }
    
    half4 frag(vertexOutput input): SV_Target
    {
        
        half4 Result = half4(1, 1, 1, 1);
        
        //applying main texture
        #if TEXTUREMODULE_ON
            half4 _MainTexture_var = tex2D(_MainTexture, input.uv) + half4(_MainTexturePower, _MainTexturePower, _MainTexturePower, 0);
            _MainTexture_var = clamp(_MainTexture_var, 0.0, 1.0);
            Result *= _MainTexture_var;
        #endif
        
        //Applying AO
        #if AO_ON_UV0
            half4 AOTexVar = lerp(half4(_AOColor, 1), half4(whiteColor, 1), lerp(half4(1, 1, 1, 1), tex2D(_AOTexture, input.aouv), _AOPower));
            Result *= AOTexVar;
        #elif AO_ON_UV1
            half4 AOTexVar = lerp(half4(_AOColor, 1), half4(whiteColor, 1), lerp(half4(1, 1, 1, 1), tex2D(_AOTexture, input.aouv), _AOPower));
            Result *= AOTexVar;
        #endif
        
        //applying custom Lighting Data
        Result *= half4(input.customLighting, 1);
        
        //Applying lightmap
        #if LIGHTMAP_ON
            #if LIGHTMAP_AO
                half3 lmColor = SampleLightmap(input.lightmapUV, input.normalWS);
                half4 lmPower = lerp(half4(1, 1, 1, 1), half4(lmColor, 1), _LMPower);
                Result *= lerp(half4(_LMColor, 0), half4(1, 1, 1, 1), lmPower);
            #endif
            
            #if LIGHTMAP_ADD
                half3 lmColor = SampleLightmap(input.lightmapUV, input.normalWS);
                half4 lmPower = lerp(half4(1, 1, 1, 1), half4(lmColor, 0), _LMPower);
                Result = clamp(Result + lmPower, 0.0, 1.0);
            #endif
            
            #if LIGHTMAP_MULTIPLY
                half3 lmColor = SampleLightmap(input.lightmapUV, input.normalWS);
                half4 lmPower = lerp(half4(1, 1, 1, 1), half4(lmColor, 0), _LMPower);
                Result *= lmPower;
            #endif
        #endif
        
        
        //Realtime Shadow
        #if SHADOW_ON
            #ifdef _MAIN_LIGHT_SHADOWS
                Light mainLight = GetMainLight(input.shadowCoord);
            #else
                Light mainLight = GetMainLight();
            #endif
            
            half3 normalDirection = SafeNormalize(input.normalWS);
            half3 lightDirection = mainLight.direction;
            half NdotL = max(0.0, dot(normalDirection, lightDirection));
            half attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
            half shadow = pow(max(0.0, NdotL) * attenuation, _ShadowInfluence);
            half4 shadowColor = lerp(half4(_ShadowColor, 1), Result, shadow);
            Result = lerp(Result, shadowColor, _ShadowBlend);
            
        #endif
        
        #if USERIM
            Result = half4(lerp(Result.rgb, _RimColor, input.rimCol), 1);
        #endif
        
        #if COLORCORRECTION_ON
            Result *= half4(_TintColor, 1);
            Result = clamp(Result + Result * _Brightness, 0.0, 1.0);
            half maxVal = max(max(Result.r, Result.g), Result.b);
            Result = half4(lerp(Result.r, maxVal, 1 - _Saturation), lerp(Result.g, maxVal, 1 - _Saturation), lerp(Result.b, maxVal, 1 - _Saturation), Result.a);
        #endif
        
        #if HEIGHT_FOG
            half3 fogGradient = lerp(_Color_Fog, Result.rgb, smoothstep(_FogYStartPos, _FogHeight, input.positionWSFF.y)).rgb;
            Result = half4(fogGradient, 1);
        #endif
        
        #if UNITY_FOG
            half fogFactor = input.positionWSFF.w;
            Result = half4(MixFog(Result.xyz, fogFactor), 1);
        #endif
        
        Result *= half4(1, 1, 1, _Fade);
        return  Result;
    }
    
#endif