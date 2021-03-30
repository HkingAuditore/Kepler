#ifndef URP_MINIMALIST_INPUT_SRPB_INCLUDED
    #define URP_MINIMALIST_INPUT_SRPB_INCLUDED
    
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    
    CBUFFER_START(UnityPerMaterial)

    uniform sampler2D _MainTexture; 
    uniform half4 _MainTexture_ST;
    uniform half _MainTexturePower;
    
    uniform half3 _Color1_F;
    uniform half3 _Color2_F;
    uniform half2 _GradientYStartPos_F;
    uniform half _GradientHeight_F;
    uniform half _Rotation_F;
    
    uniform half3 _Color1_B;
    uniform half3 _Color2_B;
    uniform half2 _GradientYStartPos_B;
    uniform half _GradientHeight_B;
    uniform half _Rotation_B;
    
    uniform half3 _Color1_L;
    uniform half3 _Color2_L;
    uniform half2 _GradientYStartPos_L;
    uniform half _GradientHeight_L;
    uniform half _Rotation_L;
    
    uniform half3 _Color1_R;
    uniform half3 _Color2_R;
    uniform half2 _GradientYStartPos_R;
    uniform half _GradientHeight_R;
    uniform half _Rotation_R;
    
    uniform half3 _Color1_T;
    uniform half3 _Color2_T;
    uniform half2 _GradientXStartPos_T;
    uniform half _GradientHeight_T;
    uniform half _Rotation_T;
    
    uniform half3 _Color1_D;
    uniform half3 _Color2_D;
    uniform half2 _GradientXStartPos_D;
    uniform half _GradientHeight_D;
    uniform half _Rotation_D;
    
    uniform sampler2D _AOTexture; 
    uniform half4 _AOTexture_ST;
    uniform half3 _AOColor;
    uniform half _AOPower;
    
    uniform half3 _LMColor;
    uniform half _LMPower;
    
    uniform half3 _Color_Fog;
    uniform half _FogYStartPos;
    uniform half _FogHeight;
    
    uniform half3 _RimColor;
    uniform half _RimPower;
    
    uniform half3 _TintColor;
    uniform half _Saturation;
    uniform half _Brightness;
    
    uniform half3 _AmbientColor;
    uniform half _AmbientPower;
    
    uniform half3 _ShadowColor;
    uniform half _ShadowInfluence;
    uniform half _ShadowBlend;
    
    uniform half _GradientSpace;
    uniform half _Fade;
    CBUFFER_END
    
#endif