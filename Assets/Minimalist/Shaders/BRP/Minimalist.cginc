#ifndef BRP_MINIMALIST_INCLUDED
    #define BRP_MINIMALIST_INCLUDED
    #if TEXTUREMODULE_ON
        uniform sampler2D _MainTexture; uniform half4 _MainTexture_ST;
        uniform half _MainTexturePower;
    #endif
    
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
    
    uniform half _LMPower;
    
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
    
    uniform half3 _AmbientColor;
    uniform half _AmbientPower;
    #if SHADOW_ON
        uniform half3 _ShadowColor;
        uniform half _ShadowInfluence;
        uniform half _ShadowBlend;
    #endif
    uniform half _GradientSpace;
    uniform half _Fade;
    
    //Direction vector constants
    static const half3 FrontDir = half3(0, 0, 1);
    static const half3 BackDir = half3(0, 0, -1);
    static const half3 LeftDir = half3(1, 0, 0);
    static const half3 RightDir = half3(-1, 0, 0);
    static const half3 TopDir = half3(0, 1, 0);
    static const half3 BottomDir = half3(0, -1, 0);
    static const half3 whiteColor = half3(1, 1, 1);
    static const half3 blackColor = half3(0, 0, 0);
    
    struct vertexInput
    {
        float4 vertex: POSITION;
        half3 normal: NORMAL;
        half3 vColor: COLOR;
        float4 uv0: TEXCOORD0;
        float4 uv1: TEXCOORD1;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    struct vertexOutput
    {
        float4 pos: POSITION;
        float4 worldPos: TEXCOORD0;
        #if TEXTUREMODULE_ON
            float2 uv: TEXCOORD1;
        #endif
        
        #if LIGHTMAP_ADD || LIGHTMAP_MULTIPLY || LIGHTMAP_AO
            float2 lightmapUV: TEXCOORD2;
        #endif
        float3 customLighting: COLOR0;
        
        #if AO_ON_UV0 || AO_ON_UV1
            float2 aouv: TEXCOORD4;
        #endif
        #if UNITY_FOG
            UNITY_FOG_COORDS(5)
        #endif
        #if SHADOW_ON
            LIGHTING_COORDS(6, 7)
            float3 normalDir: TEXCOORD8;
        #endif
        #if USERIM
            float rimCol: COLOR1;
        #endif
    };
    
    float3 CalculateDirectionalColor(float4 GradPos, float2 GradientYStartPos, float Rotation, float GradientHeight, float3 Color1, float3 Color2){
        half RotatedGrad = (GradPos.x - GradientYStartPos.x) * sin(Rotation / 57.32) + (GradPos.y - GradientYStartPos.y) * cos(Rotation / 57.32);
        half GradientFactor = saturate(RotatedGrad / - GradientHeight);
        return lerp(Color1, Color2, GradientFactor);
    }
    
    #define RESOLVE_DIR(dir, norm) max(dot(norm, dir), 0.0)
    
    vertexOutput vert(vertexInput v)
    {
        vertexOutput o;
        UNITY_SETUP_INSTANCE_ID(v);
        o.pos = UnityObjectToClipPos(v.vertex);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        half3 normalWS = UnityObjectToWorldNormal(v.normal);
        half3 gradNorm = _GradientSpace == 0 ? normalWS : v.normal; 
        //Maping Texture
        #if TEXTUREMODULE_ON
            o.uv = TRANSFORM_TEX(v.uv0, _MainTexture);
        #endif
        
        //Maping AO maps
        #if AO_ON_UV0
            o.aouv = TRANSFORM_TEX(v.uv0, _AOTexture);
        #endif
        #if AO_ON_UV1
            o.aouv = TRANSFORM_TEX(v.uv1, _AOTexture);
        #endif
        
        
        //Calculating custom shadings
        half3 colorFront, colorBack, colorLeft, colorRight, colorTop, colorDown;
        colorFront = colorBack = colorLeft = colorRight = colorTop = colorDown = v.vColor;
        
        half dirFront   = RESOLVE_DIR(gradNorm, FrontDir);
        half dirBack    = RESOLVE_DIR(gradNorm, BackDir);
        half dirLeft    = RESOLVE_DIR(gradNorm, LeftDir);
        half dirRight   = RESOLVE_DIR(gradNorm, RightDir);
        half dirTop     = RESOLVE_DIR(gradNorm, TopDir);
        half dirBottom  = RESOLVE_DIR(gradNorm, BottomDir);
        
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
        
        float4 GradPos = _GradientSpace == 0? o.worldPos : v.vertex;
        
        #if FRONTGRADIENT
            colorFront = CalculateDirectionalColor(GradPos, _GradientYStartPos_F, _Rotation_F, _GradientHeight_F, _Color1_F, _Color2_F);
        #endif
        #if BACKGRADIENT
            colorBack = CalculateDirectionalColor(GradPos, _GradientYStartPos_B, _Rotation_B, _GradientHeight_B, _Color1_B, _Color2_B);
        #endif
        #if LEFTGRADIENT
            colorLeft = CalculateDirectionalColor(GradPos, _GradientYStartPos_L, _Rotation_L, _GradientHeight_L, _Color1_L, _Color2_L);
        #endif
        #if RIGHTGRADIENT
            colorRight = CalculateDirectionalColor(GradPos, _GradientYStartPos_R, _Rotation_R, _GradientHeight_R, _Color1_R, _Color2_R);
        #endif
        #if TOPGRADIENT
            colorTop = CalculateDirectionalColor(GradPos, _GradientXStartPos_T, _Rotation_T, _GradientHeight_T, _Color1_T, _Color2_T);
        #endif
        #if BOTTOMGRADIENT
            colorDown = CalculateDirectionalColor(GradPos, _GradientXStartPos_D, _Rotation_D, _GradientHeight_D, _Color1_D, _Color2_D);
        #endif
        
        
        half3 Maincolor = half3(1, 1, 1);
        #if DONTMIX
            Maincolor = colorFront * dirFront + colorBack * dirBack + colorLeft * dirLeft + colorRight * dirRight + colorTop * dirTop + colorDown * dirBottom;
        #else
            Maincolor = lerp(colorFront, whiteColor, 1 - dirFront) * lerp(colorBack, whiteColor, 1 - dirBack) * lerp(colorLeft, whiteColor, 1 - dirLeft) * lerp(colorRight, whiteColor, 1 - dirRight) * lerp(colorTop, whiteColor, 1 - dirTop) * lerp(colorDown, whiteColor, 1 - dirBottom);
        #endif
        o.customLighting = Maincolor + (_AmbientColor * _AmbientPower);
        
        //Lightmap
        #if	LIGHTMAP_ADD || LIGHTMAP_MULTIPLY || LIGHTMAP_AO
            o.lightmapUV = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        #endif
        // Transfer realtime shadows
        #if SHADOW_ON
            TRANSFER_SHADOW(o);
            o.normalDir = normalWS;
        #endif
        //Apply Unity fog
        #if UNITY_FOG
            UNITY_TRANSFER_FOG(o, o.pos);
        #endif
        
        #if USERIM
            float3 viewDir = normalize(WorldSpaceViewDir(v.vertex));
            o.rimCol = pow(1 - dot(normalWS, viewDir), _RimPower);
        #endif
        
        return o;
    }
    
    half4 frag(vertexOutput i): COLOR
    {
        half4 Result = half4(whiteColor, 1);
        //applying main texture
        #if TEXTUREMODULE_ON
            half4 _MainTexture_var = tex2D(_MainTexture, i.uv) + half4(_MainTexturePower, _MainTexturePower, _MainTexturePower, 0);
            _MainTexture_var = clamp(_MainTexture_var, 0.0, 1.0);
            Result *= _MainTexture_var;
        #endif
        //Applying AO
        #if AO_ON_UV0
            half4 AOTexVar = lerp(half4(_AOColor, 1), half4(whiteColor, 1), lerp(half4(1, 1, 1, 1), tex2D(_AOTexture, i.aouv), _AOPower));
            Result *= AOTexVar;
        #endif
        #if AO_ON_UV1
            half4 AOTexVar = lerp(half4(_AOColor, 1), half4(whiteColor, 1), lerp(half4(1, 1, 1, 1), tex2D(_AOTexture, i.aouv), _AOPower));
            Result *= AOTexVar;
        #endif
        //applying custom Lighting Data
        Result *= half4(i.customLighting, 1);
        
        //Calculating Lightmap
        #if LIGHTMAP_AO
            half4 lmColor = UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV);
            half4 lmPower = lerp(half4(1, 1, 1, 1), half4(DecodeLightmap(lmColor), 0), _LMPower);
            Result *= lerp(half4(_LMColor, 0), half4(1, 1, 1, 1), lmPower);
        #endif
        #if LIGHTMAP_ADD
            Result += (half4(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV)), 0) * _LMPower);
            Result = clamp(Result, 0.0, 1.0);
        #endif
        #if LIGHTMAP_MULTIPLY
            Result *= half4(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV)), 0) * _LMPower;
            Result = clamp(Result, 0.0, 1.0);
        #endif
        //Calculating Shadows with shadow color
        #if SHADOW_ON
            i.normalDir = normalize(i.normalDir);
            float3 normalDirection = i.normalDir;
            float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.worldPos.xyz, _WorldSpaceLightPos0.w));
            half attenuation = LIGHT_ATTENUATION(i);
            float NdotL = max(0.0, dot(normalDirection, lightDirection));
            float shadow = pow(max(0.0, NdotL) * attenuation, _ShadowInfluence);
            half4 shadowColor = lerp(half4(_ShadowColor, 1), Result, shadow);
            Result = lerp(Result, shadowColor, _ShadowBlend);
        #endif
        
        #if USERIM
            Result = half4(lerp(Result.rgb, _RimColor, i.rimCol), 1);
        #endif
        
        #if COLORCORRECTION_ON
            Result *= half4(_TintColor, 1);
            Result = clamp(Result + Result * _Brightness, 0.0, 1.0);
            half maxVal = max(max(Result.r, Result.g), Result.b);
            Result = half4(lerp(Result.r, maxVal, 1 - _Saturation), lerp(Result.g, maxVal, 1 - _Saturation), lerp(Result.b, maxVal, 1 - _Saturation), Result.a);
        #endif
        
        //HeightFog
        #if HEIGHT_FOG
            half3 fogGradient = lerp(_Color_Fog, Result.rgb, smoothstep(_FogYStartPos, _FogHeight, i.worldPos.y)).rgb;
            Result = half4(fogGradient, 1);
        #endif
        //Unity Fog
        #if UNITY_FOG
            UNITY_APPLY_FOG(i.fogCoord, Result);
        #endif
        Result *= half4(1, 1, 1, _Fade);
        return Result;
    }
#endif