#ifndef URP_MINIMALIST_META_INCLUDED
    #define URP_MINIMALIST_META_INCLUDED
    
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
    #ifdef MINIMALIST_SRP_BATCHING
         #include "MinimalistInputSrpBatched.hlsl"
    #else
        #include "MinimalistInput.hlsl"
    #endif

    struct Attributes
    {
        float4 positionOS   : POSITION;
        float3 normalOS     : NORMAL;
        float2 uv0          : TEXCOORD0;
        float2 uv1          : TEXCOORD1;
        float2 uv2          : TEXCOORD2;
    #ifdef _TANGENT_TO_WORLD
        float4 tangentOS     : TANGENT;
    #endif
    };
    
    struct Varyings
    {
        float4 positionCS   : SV_POSITION;
        float2 uv           : TEXCOORD0;
    };
    
    Varyings MinimalistVertexMeta(Attributes input)
    {
        Varyings output;
        output.positionCS = MetaVertexPosition(input.positionOS, input.uv1, input.uv2,
            unity_LightmapST, unity_DynamicLightmapST);
        output.uv = TRANSFORM_TEX(input.uv0, _MainTexture);
        return output;
    }
    
    half4 MinimalistFragmentMeta(Varyings input) : SV_Target
    {
        float3 surfaceColor = float3(1, 1, 1); // Get custom shading color here
        MetaInput metaInput;
        metaInput.Albedo = surfaceColor * tex2D(_MainTexture, input.uv).rgb;
        //metaInput.SpecularColor = SampleSpecularSmoothness(uv, 1.0h, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap)).xyz;
        metaInput.SpecularColor = half3(0, 0, 0);
        //metaInput.Emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
        metaInput.Emission = half3(0, 0, 0);
        return MetaFragment(metaInput);
    }
    
#endif