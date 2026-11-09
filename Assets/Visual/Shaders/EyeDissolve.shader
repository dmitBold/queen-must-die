// Глаз-призрак из HoMScene: виден без освещения и растворяется по шумовой маске.
// Шум считается по позиции в пространстве объекта, поэтому не рвётся на UV-швах.
Shader "QMD/EyeDissolve"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _BodyBrightness ("Body Brightness", Range(0, 1)) = 0.55
        _EmissionStrength ("Emission Strength", Float) = 3.5
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _NoiseScale ("Noise Scale", Float) = 350
        _EdgeWidth ("Edge Width", Range(0.001, 0.3)) = 0.08
        [HDR] _EdgeColor ("Edge Color (at the burn line)", Color) = (1.3, 0.85, 0.12, 1)
        [HDR] _EdgeOuterColor ("Edge Outer Color", Color) = (0.55, 0.06, 1.2, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "AlphaTest" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _EmissionMap_ST;
            float _BodyBrightness;
            float _EmissionStrength;
            float _Dissolve;
            float _NoiseScale;
            float _EdgeWidth;
            float4 _EdgeColor;
            float4 _EdgeOuterColor;
        CBUFFER_END

        float Hash(float3 p)
        {
            p = frac(p * 0.3183099 + 0.1);
            p *= 17.0;
            return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
        }

        float ValueNoise(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            return lerp(
                lerp(lerp(Hash(i), Hash(i + float3(1, 0, 0)), f.x), lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x), lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y),
                f.z);
        }

        float DissolveNoise(float3 positionOS)
        {
            float3 p = positionOS * _NoiseScale;
            return ValueNoise(p) * 0.6 + ValueNoise(p * 2.3) * 0.3 + ValueNoise(p * 5.1) * 0.1;
        }

        // Запас по краям гарантирует, что при 0 глаз целый, а при 1 исчезает полностью.
        float DissolveThreshold()
        {
            return lerp(-0.1, 1.1, _Dissolve);
        }
        ENDHLSL

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 positionOS : TEXCOORD1; };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.positionOS = input.positionOS.xyz;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float noise = DissolveNoise(i.positionOS);
                float threshold = DissolveThreshold();
                clip(noise - threshold);

                // Без освещения: глаз обязан читаться в полной темноте.
                half3 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _BodyBrightness;
                color += SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.uv).rgb * _EmissionStrength;
                if (_Dissolve > 0.0)
                {
                    float edge = 1.0 - saturate((noise - threshold) / _EdgeWidth);
                    // Квадрат прижимает золото к самой кромке, дальше от неё остаётся фиолетовый.
                    color += lerp(_EdgeOuterColor.rgb, _EdgeColor.rgb, edge * edge) * edge;
                }
                return half4(color, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionOS : TEXCOORD0; };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.positionOS = input.positionOS.xyz;
                return o;
            }

            half Frag(Varyings i) : SV_Target
            {
                clip(DissolveNoise(i.positionOS) - DissolveThreshold());
                return 0;
            }
            ENDHLSL
        }
    }
}
