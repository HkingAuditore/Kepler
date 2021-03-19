Shader "Minimalist/URP/Standard Localspace"
{
	Properties
	{
		//Texture Module
		[HideInInspector][MaterialToggle] _ShowTexture ("Show Texture settngs", Float ) = 0
		_MainTexture ("Main Texture", 2D) = "white" {}
		_MainTexturePower ("Main Texture Power", Range(-1, 1)) = 0
		//Custom Shading
		[HideInInspector][MaterialToggle] _ShowCustomShading ("Show Custom Shading settngs", Float ) = 0

		[HideInInspector][MaterialToggle] _ShowFront ("Front", Float ) = 0
		[HideInInspector][KeywordEnum(VertexColor, SolidColor, Gradient)] _Shading_F("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_F("Shading mode", Float) = 0
		[HideInInspector]_GizmoPosition_F("front gizmo", Vector) = (0, 0, 10, 10)

		_Color1_F("Forward Color 1", Color) = (1, 1, 1, 1)
		_Color2_F("Forward Color 2", Color) = (1, 1, 1, 1) 
		_GradientYStartPos_F ("Gradient start Y", Vector) = (0, 0, 0, 0)
		_GradientHeight_F("Gradient Height", Float) = 1
		_Rotation_F("Rotation", Range(0, 360)) = 0

		[HideInInspector][MaterialToggle] _ShowBack ("Front", Float ) = 0
		[HideInInspector][KeywordEnum(Vertex Color, Solid Color, Gradient)] _Shading_B("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_B("Shading mode", Float) = 0
		[HideInInspector]_GizmoPosition_B("back gizmo", Vector) = (0, 0, 10, 10)

		_Color1_B("Backward Color 1", Color) = (1, 1, 1, 1)
		_Color2_B("Backward Color 2", Color) = (1, 1, 1, 1)
		_GradientYStartPos_B ("Gradient start Y", Vector) = (0, 0, 0, 0)
		_GradientHeight_B("Gradient Height", Float) = 1
		_Rotation_B("Rotation", Range(0, 360)) = 0

		[HideInInspector][MaterialToggle] _ShowLeft ("Front", Float ) = 0
		[HideInInspector][KeywordEnum(Vertex Color, Solid Color, Gradient)] _Shading_L("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_L("Shading mode", Float) = 0
		[HideInInspector]_GizmoPosition_L("Left gizmo", Vector) = (0, 0, 10, 10)

		_Color1_L("Left Color 1", Color) = (1, 1, 1, 1)
		_Color2_L("Left Color 2", Color) = (1, 1, 1, 1)
		_GradientYStartPos_L ("Gradient start Y", Vector) = (0, 0, 0, 0)
		_GradientHeight_L("Gradient Height", Float) = 1
		_Rotation_L("Rotation", Range(0, 360)) = 0

		[HideInInspector][MaterialToggle] _ShowRight ("Front", Float ) = 0
		[HideInInspector][KeywordEnum(Vertex Color, Solid Color, Gradient)] _Shading_R("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_R("Shading mode", Float) = 0
		[HideInInspector]_GizmoPosition_R("Right gizmo", Vector) = (0, 0, 10, 10)

		_Color1_R("Right Color 1", Color) = (1, 1, 1, 1)
		_Color2_R("Right Color 2", Color) = (1, 1, 1, 1)
		_GradientYStartPos_R ("Gradient start Y", Vector) = (0, 0, 0, 0)
		_GradientHeight_R("Gradient Height", Float) = 1
		_Rotation_R("Rotation", Range(0, 360)) = 0

		[HideInInspector][MaterialToggle] _ShowTop ("Top", Float ) = 0
		[HideInInspector][KeywordEnum(Vertex Color, Solid Color, Gradient)] _Shading_T("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_T("Gradient mode", Float) = 0
		[HideInInspector]_GizmoPosition_T("Top gizmo", Vector) = (0, 0, 10, 10)

		_Color1_T ("Top Color 1", Color) = (1, 1, 1, 1)
		_Color2_T ("Top Color 2", Color) = (1, 1, 1, 1)
		_GradientXStartPos_T ("Gradient start X", Vector) = (0, 0, 0, 0)
		_GradientHeight_T("Gradient Height", Float) = 1
		_Rotation_T("Rotation", Range(0, 360)) = 0

		[HideInInspector][MaterialToggle] _ShowBottom ("Botttom", Float ) = 0
		[HideInInspector][KeywordEnum(Vertex Color, Solid Color, Gradient)] _Shading_D("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_D("Gradient mode", Float) = 0
		[HideInInspector]_GizmoPosition_D("Down gizmo", Vector) = (0, 0, 10, 10)

		_Color1_D ("Bottom Color 1", Color) = (1, 1, 1, 1)
		_Color2_D ("Bottom Color 2", Color) = (1, 1, 1, 1)
		_GradientXStartPos_D ("Gradient start X", Vector) = (0, 0, 0, 0)
		_GradientHeight_D("Gradient Height", Float) = 1
		_Rotation_D("Rotation", Range(0, 360)) = 0
		//Ambient Occlution
		[HideInInspector][MaterialToggle] _ShowAO ("AO", Float ) = 0
		[HideInInspector][MaterialToggle] _AOEnable ("Enable", Float ) = 0
		_AOTexture ("AO Texture", 2D) = "white" {}
		_AOColor ("AO Color", Color) = (1, 1, 1, 1)
		_AOPower ("AO Texture Power", Range(0, 3)) = 0
		[HideInInspector][KeywordEnum(uv0, uv1)] _AOuv("UV", Float) = 0
		//Lightmap
		[HideInInspector][MaterialToggle] _ShowLMap ("Lightmap", Float ) = 0
		[HideInInspector][MaterialToggle] _LmapEnable ("Enable", Float ) = 0
		[HideInInspector][KeywordEnum(Add, Multiply, AO)] _LmapBlendingMode("Blend Mode", Float) = 0
		_LMColor ("LightMap Color", Color) = (1, 1, 1, 1)
		_LMPower ("LightMap Power", Range(0, 5.0)) = 0
		//Fog
		[HideInInspector][MaterialToggle] _ShowFog ("ShowFog", Float ) = 0
		[MaterialToggle] _UnityFogEnable ("Fog", Float ) = 0
		[MaterialToggle] _HFogEnable ("Fog", Float ) = 0
		_Color_Fog ("Fog Color",     Color) = (0.5, 0.5, 0.5, 1)
		_FogYStartPos ("Gradient start Y", Float) = 0
		_FogHeight("Gradient Height", Float) = 10
		//Color Correction
		[HideInInspector][MaterialToggle] _ShowColorCorrection ("Color Correction", Float ) = 0
		[HideInInspector][MaterialToggle] _ColorCorrectionEnable ("Enable", Float ) = 0
		_TintColor ("Tint Color", Color) = (1, 1, 1, 1)
		_Saturation ("Saturation", Range(0, 1)) = 1
		_Brightness ("Brightness", Range(-1, 1)) = 0
		//OtherSettings
		[MaterialToggle] _OtherSettings ("OtherSettings", Float ) = 0

		[HideInInspector][MaterialToggle] _ShowGlobalGradientSettings ("Show Global Gradient Settings", Float ) = 0
		_GradientYStartPos_G ("Gradient start Y", Vector) = (0, 0, 0, 0)
		_GradientHeight_G("Gradient Height", Float) = 1
		_Rotation_G("Rotation", Float) = 0

		[HideInInspector][MaterialToggle] _ShowAmbientSettings ("Show Ambient Settings", Float ) = 0
		_AmbientColor("Ambient Color",Color) = (0, 0, 0, 0)
		_AmbientPower("Ambient Power", Range(0, 2.0)) = 0

		[HideInInspector][MaterialToggle] _RimEnable ("Use Rim", Float ) = 0
		_RimColor ("Rim Color", Color) = (1, 1, 1, 1)
		_RimPower ("Power", Range(0, 4)) = 1
		
		[MaterialToggle] _RealtimeShadow ("RealTime Shadow", Float ) = 0
		_ShadowColor("ShadowColor",    Color) = (0.1, 0.1, 0.1, 1)
		_ShadowInfluence("Influence", Range(0.001, 2)) = 0.5
		_ShadowBlend("Strength", Range(0, 1)) = 0.5

		[KeywordEnum(World Space, Local Space)] _GradientSpace("Gradient Space", Float) = 0
		[MaterialToggle] _DontMix ("Don't Mix Color", Float ) = 0
		_Fade ("Fade", Range(0, 1)) = 1
		
		
		[HideInInspector] _Mode("Mode", Float) = 0
		[HideInInspector] _SrcBlend ("_src", Float) = 1.0
		[HideInInspector] _DstBlend ("_dst", Float) = 0.0
		[HideInInspector] _ZWrite ("_zWrite", Float) = 0.0
		[HideInInspector] _ZWriteOverride ("_zWrite", Float) = 0.0
		[HideInInspector] _Cull("Cull", Float) = 2
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "DisableBatching" = "True" }
		
		HLSLINCLUDE
		    #define MINIMALIST_LOCALSPACE
		ENDHLSL
		
		UsePass "Minimalist/URP/Standard/StandardPass"
		UsePass "Minimalist/URP/Standard/ShadowCaster"
		UsePass "Minimalist/URP/Standard/DepthOnly"
		UsePass "Minimalist/URP/Standard/Meta"
	}
	
	FallBack "Hidden/InternalErrorShader"
	CustomEditor "Minimalist.MinimalistStandardEditor"
}
