shader "Minimalist/BRP/Standard LocalSpace" {
	Properties{
		//Texture Module
        _ShowTexture ("Show Texture settngs", Float) = 0
        _MainTexture ("Main Texture", 2D) = "white" { }
        _MainTexturePower ("Main Texture Power", Range(-1, 1)) = 0
        //Custom Shading
        _ShowCustomShading ("Show Custom Shading settngs", Float) = 0
        
        _ShowFront ("Front", Float) = 0
        _Shading_F ("Shading mode", Float) = 0
        _GradSettings_F ("Shading mode", Float) = 0
        _GizmoPosition_F ("front gizmo", Vector) = (0, 0, 10, 10)
        
        _Color1_F ("Forward Color 1", Color) = (1, 1, 1, 1)
        _Color2_F ("Forward Color 2", Color) = (1, 1, 1, 1)
        _GradientYStartPos_F ("Gradient start Y", Vector) = (0, 0, 0, 0)
        _GradientHeight_F ("Gradient Height", Float) = 1
        _Rotation_F ("Rotation", Range(0, 360)) = 0
        
        _ShowBack ("Front", Float) = 0
        _Shading_B ("Shading mode", Float) = 0
        _GradSettings_B ("Shading mode", Float) = 0
        _GizmoPosition_B ("back gizmo", Vector) = (0, 0, 10, 10)
        
        _Color1_B ("Backward Color 1", Color) = (1, 1, 1, 1)
        _Color2_B ("Backward Color 2", Color) = (1, 1, 1, 1)
        _GradientYStartPos_B ("Gradient start Y", Vector) = (0, 0, 0, 0)
        _GradientHeight_B ("Gradient Height", Float) = 1
        _Rotation_B ("Rotation", Range(0, 360)) = 0
        
        _ShowLeft ("Front", Float) = 0
        _Shading_L ("Shading mode", Float) = 0
        _GradSettings_L ("Shading mode", Float) = 0
        _GizmoPosition_L ("Left gizmo", Vector) = (0, 0, 10, 10)
        
        _Color1_L ("Left Color 1", Color) = (1, 1, 1, 1)
        _Color2_L ("Left Color 2", Color) = (1, 1, 1, 1)
        _GradientYStartPos_L ("Gradient start Y", Vector) = (0, 0, 0, 0)
        _GradientHeight_L ("Gradient Height", Float) = 1
        _Rotation_L ("Rotation", Range(0, 360)) = 0
        
        _ShowRight ("Front", Float) = 0
        _Shading_R ("Shading mode", Float) = 0
        _GradSettings_R ("Shading mode", Float) = 0
        _GizmoPosition_R ("Right gizmo", Vector) = (0, 0, 10, 10)
        
        _Color1_R ("Right Color 1", Color) = (1, 1, 1, 1)
        _Color2_R ("Right Color 2", Color) = (1, 1, 1, 1)
        _GradientYStartPos_R ("Gradient start Y", Vector) = (0, 0, 0, 0)
        _GradientHeight_R ("Gradient Height", Float) = 1
        _Rotation_R ("Rotation", Range(0, 360)) = 0
        
        _ShowTop ("Top", Float) = 0
        _Shading_T ("Shading mode", Float) = 0
        _GradSettings_T ("Gradient mode", Float) = 0
        _GizmoPosition_T ("Top gizmo", Vector) = (0, 0, 10, 10)
        
        _Color1_T ("Top Color 1", Color) = (1, 1, 1, 1)
        _Color2_T ("Top Color 2", Color) = (1, 1, 1, 1)
        _GradientXStartPos_T ("Gradient start X", Vector) = (0, 0, 0, 0)
        _GradientHeight_T ("Gradient Height", Float) = 1
        _Rotation_T ("Rotation", Range(0, 360)) = 0
        
        _ShowBottom ("Botttom", Float) = 0
        _Shading_D ("Shading mode", Float) = 0
        _GradSettings_D ("Gradient mode", Float) = 0
        _GizmoPosition_D ("Down gizmo", Vector) = (0, 0, 10, 10)
        
        _Color1_D ("Bottom Color 1", Color) = (1, 1, 1, 1)
        _Color2_D ("Bottom Color 2", Color) = (1, 1, 1, 1)
        _GradientXStartPos_D ("Gradient start X", Vector) = (0, 0, 0, 0)
        _GradientHeight_D ("Gradient Height", Float) = 1
        _Rotation_D ("Rotation", Range(0, 360)) = 0
        //Ambient Occlution
        _ShowAO ("AO", Float) = 0
        _AOEnable ("Enable", Float) = 0
        _AOTexture ("AO Texture", 2D) = "white" { }
        _AOColor ("AO Color", Color) = (1, 1, 1, 1)
        _AOPower ("AO Texture Power", Range(0, 3)) = 0
        _AOuv ("UV", Float) = 0
        //Lightmap
        _ShowLMap ("Lightmap", Float) = 0
        _LmapEnable ("Enable", Float) = 0
        _LmapBlendingMode ("Blend Mode", Float) = 0
        _LMColor ("LightMap Color", Color) = (1, 1, 1, 1)
        _LMPower ("LightMap Power", Range(0, 5.0)) = 0
        //Fog
        _ShowFog ("ShowFog", Float) = 0
        _UnityFogEnable ("Fog", Float) = 0
        _HFogEnable ("Fog", Float) = 0
        _Color_Fog ("Fog Color", Color) = (0.5, 0.5, 0.5, 1)
        _FogYStartPos ("Gradient start Y", Float) = 0
        _FogHeight ("Gradient Height", Float) = 10
        //Color Correction
        _ShowColorCorrection ("Color Correction", Float) = 0
        _ColorCorrectionEnable ("Enable", Float) = 0
        _TintColor ("Tint Color", Color) = (1, 1, 1, 1)
        _Saturation ("Saturation", Range(0, 1)) = 1
        _Brightness ("Brightness", Range(-1, 1)) = 0
        //OtherSettings
        _OtherSettings ("OtherSettings", Float) = 0
        
        _ShowGlobalGradientSettings ("Show Global Gradient Settings", Float) = 0
        _GradientYStartPos_G ("Gradient start Y", Vector) = (0, 0, 0, 0)
        _GradientHeight_G ("Gradient Height", Float) = 1
        _Rotation_G ("Rotation", Float) = 0
        
        _ShowAmbientSettings ("Show Ambient Settings", Float) = 0
        _AmbientColor ("Ambient Color", Color) = (0, 0, 0, 0)
        _AmbientPower ("Ambient Power", Range(0, 2.0)) = 0
        
        _RimEnable ("Use Rim", Float) = 0
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Power", Range(0, 4)) = 1
        
        _RealtimeShadow ("RealTime Shadow", Float) = 0
        _ShadowColor ("ShadowColor", Color) = (0.1, 0.1, 0.1, 1)
        _ShadowInfluence ("Influence", Range(0.001, 2)) = 0.5
        _ShadowBlend ("Strength", Range(0, 1)) = 0.5
        
        _GradientSpace ("Gradient Space", Float) = 0
        _DontMix ("Don't Mix Color", Float) = 0
        _Fade ("Fade", Range(0, 1)) = 1
        
        _Mode ("Mode", Float) = 0
        _SrcBlend ("_src", Float) = 1.0
        _DstBlend ("_dst", Float) = 0.0
        _ZWrite ("_zWrite", Float) = 0.0
        _ZWriteOverride ("_zWrite", Float) = 0.0
        _Cull ("Cull", Float) = 2
	}

	SubShader{
		Tags{"DisableBatching" = "True"}
		
		UsePass "Minimalist/BRP/Standard/StandardPass"
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
	FallBack "Minimalist/Standard"
	CustomEditor "Minimalist.MinimalistStandardEditor"
}
