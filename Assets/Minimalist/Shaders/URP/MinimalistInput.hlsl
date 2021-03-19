#ifndef URP_MINIMALIST_INPUT_INCLUDED
    #define URP_MINIMALIST_INPUT_INCLUDED
    
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    uniform sampler2D _MainTexture; uniform half4 _MainTexture_ST;
    uniform half _MainTexturePower;
    uniform half _GradientSpace;
    uniform half _Fade;
    uniform half _LMPower;
    uniform half3 _AmbientColor;
    uniform half _AmbientPower;

    #if FRONTSOLID || FRONTGRADIENT
        uniform half3 _Color1_F;
        #if FRONTGRADIENT
            uniform half3 _Color2_F;
            uniform half2 _GradientYStartPos_F;
            uniform half _GradientHeight_F;
            uniform half _Rotation_F;
        #endif
    #endif

    #if BACKSOLID || BACKGRADIENT
        uniform half3 _Color1_B;
        #if BACKGRADIENT
            uniform half3 _Color2_B;
            uniform half2 _GradientYStartPos_B;
            uniform half _GradientHeight_B;
            uniform half _Rotation_B;
        #endif
    #endif

    #if LEFTSOLID || LEFTGRADIENT
        uniform half3 _Color1_L;
        #if LEFTGRADIENT
            uniform half3 _Color2_L;
            uniform half2 _GradientYStartPos_L;
            uniform half _GradientHeight_L;
            uniform half _Rotation_L;
        #endif
    #endif

    #if RIGHTSOLID || RIGHTGRADIENT
        uniform half3 _Color1_R;
        #if RIGHTGRADIENT
            uniform half3 _Color2_R;
            uniform half2 _GradientYStartPos_R;
            uniform half _GradientHeight_R;
            uniform half _Rotation_R;
        #endif
    #endif

    #if TOPSOLID || TOPGRADIENT
        uniform half3 _Color1_T;
        #if TOPGRADIENT
            uniform half3 _Color2_T;
            uniform half2 _GradientXStartPos_T;
            uniform half _GradientHeight_T;
            uniform half _Rotation_T;
        #endif
    #endif

    #if BOTTOMSOLID || BOTTOMGRADIENT
        uniform half3 _Color1_D;
        #if BOTTOMGRADIENT
            uniform half3 _Color2_D;
            uniform half2 _GradientXStartPos_D;
            uniform half _GradientHeight_D;
            uniform half _Rotation_D;
        #endif
    #endif

    #if AO_ON_UV0 || AO_ON_UV1
        uniform sampler2D _AOTexture; uniform half4 _AOTexture_ST;
        uniform half3 _AOColor;
        uniform half _AOPower;
    #endif

    #if LIGHTMAP_AO
        uniform half3 _LMColor;
    #endif

    #if HEIGHT_FOG
        uniform half3 _Color_Fog;
        uniform half _FogYStartPos;
        uniform half _FogHeight;
    #endif

    #if USERIM
        uniform half3 _RimColor;
        uniform half _RimPower;
    #endif

    #if COLORCORRECTION_ON
        uniform half3 _TintColor;
        uniform half _Saturation;
        uniform half _Brightness;
    #endif
    
    #if SHADOW_ON
        uniform half3 _ShadowColor;
        uniform half _ShadowInfluence;
        uniform half _ShadowBlend;
    #endif
    
#endif