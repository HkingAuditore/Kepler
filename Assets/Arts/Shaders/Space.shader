Shader "Skybox/Space"
{
    Properties
    {
        [Header(Sky Setting)]
        _Color1 ("Top Color", Color) = (1, 1, 1, 0)
        _Color2 ("Horizon Color", Color) = (1, 1, 1, 0)
        _Color3 ("Bottom Color", Color) = (1, 1, 1, 0)
        _Exponent1 ("Exponent Factor for Top Half", Float) = 1.0
        _Exponent2 ("Exponent Factor for Bottom Half", Float) = 1.0
        _Intensity ("Intensity Amplifier", Float) = 1.0

        [Header(Rotation)]
        _Rotation ("Rotation", Range(0, 360)) = 0
        _RotationAxis("Rotation axis", Vector) = (0, 1, 0)
        _RotationSpeed("Rotation vector", Vector) = (0, 1, 0)

        [Header(Star Setting)]
        [HDR]_StarColor0 ("Star Color 0", Color) = (1,1,1,0)
        [HDR]_StarColor1 ("Star Color 1", Color) = (1,1,1,0)
        [HDR]_StarColor2 ("Star Color 2", Color) = (1,1,1,0)
        _StarIntensity("Star Intensity", Range(0,1)) = 0.5
        _StarSpeed("Star Speed", Range(0,1)) = 0.5

        [Header(Cloud Setting)]
        [HDR]_CloudColor ("Cloud Color", Color) = (1,1,1,0)
        _CloudIntensity("Cloud Intensity", Range(0,1)) = 0.5
        _CloudSpeed("CloudSpeed", Range(0,1)) = 0.5

        [Header(Aurora Setting)]
        [HDR]_AuroraColor0 ("Aurora Color 0", Color) = (1,1,1,0)
        [HDR]_AuroraColor1 ("Aurora Color 1", Color) = (1,1,1,0)
        _AuroraIntensity("Aurora Intensity", Range(0,1)) = 0.5
        _AuroraSpeed("AuroraSpeed", Range(0,1)) = 0.5


    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata
    {
        float4 position : POSITION;
        float3 texcoord : TEXCOORD0;
        float3 normal : NORMAL;
    };

    struct v2f
    {
        float4 position : SV_POSITION;
        float3 texcoord : TEXCOORD0;
        float3 normal : TEXCOORD1;
    };

    //旋转
    float _Rotation;
    float3 _RotationAxis;
    float3 _RotationSpeed;

    // 环境背景颜色
    half4 _Color1;
    half4 _Color2;
    half4 _Color3;
    half _Intensity;
    half _Exponent1;
    half _Exponent2;


    //星星 
    half4 _StarColor0;
    half4 _StarColor1;
    half4 _StarColor2;
    half _StarIntensity;
    half _StarSpeed;

    // 云
    half4 _CloudColor;
    half _CloudIntensity;
    half _CloudSpeed;

    // 极光
    half4 _AuroraColor0;
    half4 _AuroraColor1;
    half _AuroraIntensity;
    half _AuroraSpeed;


    float4x4 rotationMatrix(float3 axis, float angle)
    {
        axis = normalize(axis);
        float s = sin(angle);
        float c = cos(angle);
        float oc = 1.0 - c;

        return float4x4(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s,
                        0.0,
                        oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s,
                        0.0,
                        oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c,
                        0.0,
                        0.0, 0.0, 0.0, 1.0);
    }

    v2f vert(appdata v)
    {
        v2f o;
        _RotationAxis += _Time.y * _RotationSpeed;
        _Rotation += _Time.y;
        float3 rotated = mul(rotationMatrix(normalize(_RotationAxis.xyz), (_Rotation+ _Time.y*_RotationSpeed) * UNITY_PI / 180.0),
                             v.position).xyz;
        o.position = UnityObjectToClipPos(rotated);

        o.texcoord = v.texcoord;
        o.normal = v.normal;
        return o;
    }


    // 星空散列哈希
    float StarAuroraHash(float3 x)
    {
        float3 p = float3(dot(x, float3(214.1, 127.7, 125.4)),
                          dot(x, float3(260.5, 183.3, 954.2)),
                          dot(x, float3(209.5, 571.3, 961.2)));

        return -0.001 + _StarIntensity * frac(sin(p) * 43758.5453123);
    }

    // 星空噪声
    float StarNoise(float3 st)
    {
        // 卷动星空
        st += float3(0, _Time.y * _StarSpeed, 0);

        // fbm
        float3 i = floor(st);
        float3 f = frac(st);

        float3 u = f * f * (3.0 - 1.0 * f);

        return lerp(lerp(dot(StarAuroraHash(i + float3(0.0, 0.0, 0.0)), f - float3(0.0, 0.0, 0.0)),
                         dot(StarAuroraHash(i + float3(1.0, 0.0, 0.0)), f - float3(1.0, 0.0, 0.0)), u.x),
                    lerp(dot(StarAuroraHash(i + float3(0.0, 1.0, 0.0)), f - float3(0.0, 1.0, 0.0)),
                         dot(StarAuroraHash(i + float3(1.0, 1.0, 0.0)), f - float3(1.0, 1.0, 0.0)), u.y), u.z);
    }


    // 云散列哈希
    float CloudHash(float2 st)
    {
        return frac(sin(dot(st.xy,
                            float2(12.9898, 78.233))) *
            43758.5453123);
    }

    // 云噪声
    float CloudNoise(float2 st, int flow)
    {
        // 云卷动
        st += float2(0, _Time.y * _CloudSpeed * flow);

        float2 i = floor(st);
        float2 f = frac(st);

        float a = CloudHash(i);
        float b = CloudHash(i + float2(1.0, 0.0));
        float c = CloudHash(i + float2(0.0, 1.0));
        float d = CloudHash(i + float2(1.0, 1.0));

        float2 u = f * f * (3.0 - 2.0 * f);

        return lerp(a, b, u.x) +
            (c - a) * u.y * (1.0 - u.x) +
            (d - b) * u.x * u.y;
    }

    // 云分型
    float Cloudfbm(float2 st, int flow)
    {
        float value = 0.0;
        float amplitude = .5;
        float frequency = 0.;

        for (int i = 0; i < 6; i++)
        {
            value += amplitude * CloudNoise(st, flow);
            st *= 2.;
            amplitude *= .5;
        }
        return value;
    }


    //极光噪声
    float AuroraHash(float n)
    {
        return frac(sin(n) * 758.5453);
    }

    float AuroraNoise(float3 x)
    {
        float3 p = floor(x);
        float3 f = frac(x);
        float n = p.x + p.y * 57.0 + p.z * 800.0;
        float res = lerp(lerp(lerp(AuroraHash(n + 0.0), AuroraHash(n + 1.0), f.x),
                              lerp(AuroraHash(n + 57.0), AuroraHash(n + 58.0), f.x), f.y),
                         lerp(lerp(AuroraHash(n + 800.0), AuroraHash(n + 801.0), f.x),
                              lerp(AuroraHash(n + 857.0), AuroraHash(n + 858.0), f.x), f.y), f.z);
        return res;
    }

    //极光分型
    float Aurorafbm(float3 p)
    {
        float f = 0.50000 * AuroraNoise(p);
        p *= 2.02;
        f += 0.25000 * AuroraNoise(p);
        p *= 2.03;
        f += 0.12500 * AuroraNoise(p);
        p *= 2.01;
        f += 0.06250 * AuroraNoise(p);
        p *= 2.04;
        f += 0.03125 * AuroraNoise(p);
        return f * 1.032258;
    }

    float GetAurora(float3 p)
    {
        p += Aurorafbm(float3(p.x, p.y, 0.0) * 0.5) * 2.25;
        float a = smoothstep(.0, .9, Aurorafbm(p * 2.) * 2.2 - 1.1);

        return a < 0.0 ? 0.0 : a;
    }


    /**************带状极光***************/

    // 旋转矩阵
    float2x2 RotateMatrix(float a)
    {
        float c = cos(a);
        float s = sin(a);
        return float2x2(c, s, -s, c);
    }

    float tri(float x)
    {
        return clamp(abs(frac(x) - 0.5), 0.01, 0.49);
    }

    float2 tri2(float2 p)
    {
        return float2(tri(p.x) + tri(p.y), tri(p.y + tri(p.x)));
    }

    // 极光噪声
    float SurAuroraNoise(float2 pos)
    {
        float intensity = 1.8;
        float size = 2.5;
        float rz = 0;
        pos = mul(RotateMatrix(pos.x * 0.06), pos);
        float2 bp = pos;
        for (int i = 0; i < 5; i++)
        {
            float2 dg = tri2(bp * 1.85) * .75;
            dg = mul(RotateMatrix(_Time.y * _AuroraSpeed), dg);
            pos -= dg / size;

            bp *= 1.3;
            size *= .45;
            intensity *= .42;
            pos *= 1.21 + (rz - 1.0) * .02;

            rz += tri(pos.x + tri(pos.y)) * intensity;
            pos = mul(-float2x2(0.95534, 0.29552, -0.29552, 0.95534), pos);
        }
        return clamp(1.0 / pow(rz * 29., 1.3), 0, 0.55);
    }

    float SurHash(float2 n)
    {
        return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
    }


    half4 frag(v2f i) : COLOR
    {
        // return fixed4(SurAuroraNoise(i.texcoord.xy),SurAuroraNoise(i.texcoord.xy),SurAuroraNoise(i.texcoord.xy),1);
        // 底色
        float p = normalize(i.texcoord).y;
        float p1 = 1.0f - pow(min(1.0f, 1.0f - p), _Exponent1);
        float p3 = 1.0f - pow(min(1.0f, 1.0f + p), _Exponent2);
        float p2 = 1.0f - p1 - p3;
        int reflection = i.texcoord.y < 0 ? -1 : 1;

        // 云
        float cloud = Cloudfbm(fixed2(i.texcoord.x, i.texcoord.z) * 8, 1);
        float4 cloudCol = float4(cloud * _CloudColor.rgb, cloud * 0.8) * _CloudIntensity;

        // 星星
        float star = StarNoise(fixed3(i.texcoord.x, i.texcoord.y * reflection, i.texcoord.z) * 64);
        float4 starOriCol = float4(
            _StarColor0.r * 1.25 * sin(frac(i.texcoord.x)) + _StarColor1.r * 1.25 * sin(frac(i.texcoord.x)) +
            _StarColor2.r * 1.25 * sin(frac(i.texcoord.x)) + 1.45 * (sin(_Time.y * _StarSpeed) + 1) * 0.5,
            _StarColor0.g * 1.85 * sin(frac(i.texcoord.y)) + _StarColor1.g * 1.85 * sin(frac(i.texcoord.y)) +
            _StarColor2.g * 1.85 * sin(frac(i.texcoord.y)) + 1.45 * (sin(_Time.y * _StarSpeed) + 1) * 0.5,
            _StarColor0.b * 1.45 * sin(frac(i.texcoord.z)) + _StarColor1.b * 1.45 * sin(frac(i.texcoord.z)) +
            _StarColor2.b * 1.45 * sin(frac(i.texcoord.z)) + 1.45 * (sin(_Time.y * _StarSpeed) + 1) * 0.5,
            _StarColor0.a + 3.85 * star);
        star = star > 0.8 ? star : smoothstep(0.81, 0.98, star);

        float4 starCol = fixed4((starOriCol * star).rgb, star);


        //极光
        fixed3 auroraColTmp = _AuroraColor0.rgb * sin(_Time.y* _AuroraSpeed) + _AuroraColor1.rgb * (1-sin(_Time.y* _AuroraSpeed));
        float3 aurora = float3(
            auroraColTmp * GetAurora(
                float3(i.texcoord.xy * float2(1.2, 1.0), _Time.y * _AuroraSpeed * 0.22)) * 0.9
            +
            auroraColTmp * GetAurora(
                float3(i.texcoord.xy * float2(1.0, 0.7), _Time.y * _AuroraSpeed * 0.27)) * 0.9
            +
            auroraColTmp * GetAurora(
                float3(i.texcoord.xy * float2(0.8, 0.6), _Time.y * _AuroraSpeed * 0.29)) * 0.5
            +
            auroraColTmp * GetAurora(
                float3(i.texcoord.xy * float2(0.9, 0.5), _Time.y * _AuroraSpeed * 0.20)) *
            0.57);
        float4 auroraCol = float4(aurora, 0.1354);


        //混合
        float4 skyCol = (_Color1 * p1 + _Color2 * p2 + _Color3 * p3) * _Intensity;
        // starCol = reflection == 1 ? starCol : starCol * 0.5;
        skyCol = skyCol * (1 - starCol.a) + starCol * starCol.a;
        skyCol = skyCol * (1 - auroraCol.a) + auroraCol * auroraCol.a;
        skyCol = skyCol * (1 - cloudCol.a) + cloudCol * cloudCol.a;


        return skyCol;
    }
    ENDCG

    SubShader
    {
        Tags
        {
            "RenderType"="Background" "Queue"="Background"
        }
        Pass
        {
            ZWrite Off
            Cull Off
            Fog
            {
                Mode Off
            }
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}