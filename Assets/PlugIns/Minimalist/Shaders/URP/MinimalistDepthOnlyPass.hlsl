#ifndef URP_MINIMALIST_DEPTHONLY_INCLUDED
    #define URP_MINIMALIST_DEPTHONLY_INCLUDED
    
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #ifdef MINIMALIST_SRP_BATCHING
        #include "MinimalistInputSrpBatched.hlsl"
    #endif
    #ifndef MINIMALIST_SRP_BATCHING
        #include "MinimalistInput.hlsl"
    #endif
    
    struct Attributes
    {
        float4 position     : POSITION;
        float2 texcoord     : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    struct Varyings
    {
        float2 uv           : TEXCOORD0;
        float4 positionCS   : SV_POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };
    
    Varyings DepthOnlyVertex(Attributes input)
    {
        Varyings output = (Varyings)0;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    
        output.uv = TRANSFORM_TEX(input.texcoord, _MainTexture);
        output.positionCS = TransformObjectToHClip(input.position.xyz);
        return output;
    }
    
    half4 DepthOnlyFragment(Varyings input) : SV_TARGET
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
        half alpha = tex2D(_MainTexture, input.uv).a;
        #if defined(_ALPHATEST_ON)
            clip(alpha - 0.05);
        #endif
        return 0;
    }
#endif